using UnityEngine;

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine.Rendering;
#endif

namespace FlashTools.Internal {
	[System.Serializable]
	public struct SwfSettingsData {
		public enum AtlasFilter {
			Point,
			Bilinear,
			Trilinear
		}

		public enum AtlasFormat {
			AutomaticCompressed,
			Automatic16bit,
			AutomaticTruecolor,
			AutomaticCrunched
		}

		[SwfPowerOfTwoIfAttribute(5, 13, "AtlasPowerOfTwo")]
		public int         MaxAtlasSize;
		[SwfIntRange(0, int.MaxValue)]
		public int         AtlasPadding;
		[SwfFloatRange(float.Epsilon, float.MaxValue)]
		public float       PixelsPerUnit;
		public bool        GenerateMipMaps;
		public bool        AtlasPowerOfTwo;
		public bool        AtlasForceSquare;
		public AtlasFilter AtlasTextureFilter;
		public AtlasFormat AtlasTextureFormat;

		public static SwfSettingsData identity {
			get {
				return new SwfSettingsData{
					MaxAtlasSize       = 1024,
					AtlasPadding       = 1,
					PixelsPerUnit      = 100.0f,
					GenerateMipMaps    = false,
					AtlasPowerOfTwo    = true,
					AtlasForceSquare   = true,
					AtlasTextureFilter = AtlasFilter.Bilinear,
					AtlasTextureFormat = AtlasFormat.AutomaticCompressed};
			}
		}

		public bool CheckEquals(SwfSettingsData other) {
			return
				MaxAtlasSize       == other.MaxAtlasSize &&
				AtlasPadding       == other.AtlasPadding &&
				Mathf.Approximately(PixelsPerUnit, other.PixelsPerUnit) &&
				GenerateMipMaps    == other.GenerateMipMaps &&
				AtlasPowerOfTwo    == other.AtlasPowerOfTwo &&
				AtlasForceSquare   == other.AtlasForceSquare &&
				AtlasTextureFilter == other.AtlasTextureFilter &&
				AtlasTextureFormat == other.AtlasTextureFormat;
		}
	}

	public class SwfSettings : ScriptableObject {

		public SwfSettingsData Settings;

		[HideInInspector] public Shader SimpleShader;
		[HideInInspector] public Shader MaskedShader;
		[HideInInspector] public Shader SimpleGrabShader;
		[HideInInspector] public Shader MaskedGrabShader;
		[HideInInspector] public Shader IncrMaskShader;
		[HideInInspector] public Shader DecrMaskShader;

	#if UNITY_EDITOR

		// ---------------------------------------------------------------------
		//
		// Internal
		//
		// ---------------------------------------------------------------------

		const string SwfSimpleShaderName     = "SwfSimpleShader";
		const string SwfMaskedShaderName     = "SwfMaskedShader";
		const string SwfSimpleGrabShaderName = "SwfSimpleGrabShader";
		const string SwfMaskedGrabShaderName = "SwfMaskedGrabShader";
		const string SwfIncrMaskShaderName   = "SwfIncrMaskShader";
		const string SwfDecrMaskShaderName   = "SwfDecrMaskShader";

		void FillShadersCache() {
			SimpleShader     = SafeLoadShader(SwfSimpleShaderName);
			MaskedShader     = SafeLoadShader(SwfMaskedShaderName);
			SimpleGrabShader = SafeLoadShader(SwfSimpleGrabShaderName);
			MaskedGrabShader = SafeLoadShader(SwfMaskedGrabShaderName);
			IncrMaskShader   = SafeLoadShader(SwfIncrMaskShaderName);
			DecrMaskShader   = SafeLoadShader(SwfDecrMaskShaderName);
			EditorUtility.SetDirty(this);
			AssetDatabase.SaveAssets();
		}

		void PregenerateMaterials() {
			var blend_types = System.Enum.GetValues(typeof(SwfBlendModeData.Types));
			foreach ( SwfBlendModeData.Types blend_type in blend_types ) {
				GetSimpleMaterial(blend_type);
				for ( var i = 0; i < 10; ++i ) {
					GetMaskedMaterial(blend_type, i);
				}
			}
			GetIncrMaskMaterial();
			GetDecrMaskMaterial();
		}

		static Shader SafeLoadShader(string shader_name) {
			var filter = string.Format("t:Shader {0}", shader_name);
			var shader = LoadFirstAssetByFilter<Shader>(filter);
			if ( !shader ) {
				throw new UnityException(string.Format(
					"SwfSettings. Shader not found: {0}",
					shader_name));
			}
			return shader;
		}

		static T LoadFirstAssetByFilter<T>(string filter) where T : UnityEngine.Object {
			var guids = AssetDatabase.FindAssets(filter);
			foreach ( var guid in guids ) {
				var path  = AssetDatabase.GUIDToAssetPath(guid);
				var asset = AssetDatabase.LoadAssetAtPath<T>(path);
				if ( asset ) {
					return asset;
				}
			}
			return null;
		}

		static Material LoadOrCreateMaterial(
			Shader                              shader,
			System.Func<string, string, string> path_factory,
			System.Func<Material, Material>     fill_material)
		{
			var shader_path   = AssetDatabase.GetAssetPath(shader);
			var shader_dir    = Path.GetDirectoryName(shader_path);
			var generated_dir = Path.Combine(shader_dir, "Generated");
			if ( !AssetDatabase.IsValidFolder(generated_dir) ) {
				AssetDatabase.CreateFolder(shader_dir, "Generated");
			}
			var material_path = path_factory(
				generated_dir,
				Path.GetFileNameWithoutExtension(shader_path));
			var material = AssetDatabase.LoadAssetAtPath<Material>(material_path);
			if ( !material ) {
				material = fill_material(new Material(shader));
				material.hideFlags = HideFlags.HideInInspector;
				AssetDatabase.CreateAsset(material, material_path);
			}
			return material;
		}

		Shader SelectShader(bool masked, SwfBlendModeData.Types blend_type) {
			switch ( blend_type ) {
			case SwfBlendModeData.Types.Normal:
			case SwfBlendModeData.Types.Multiply:
			case SwfBlendModeData.Types.Screen:
			case SwfBlendModeData.Types.Lighten:
			case SwfBlendModeData.Types.Add:
			case SwfBlendModeData.Types.Subtract:
				return CheckAndGetShader(masked ? MaskedShader : SimpleShader);
			case SwfBlendModeData.Types.Darken:
			case SwfBlendModeData.Types.Difference:
			case SwfBlendModeData.Types.Invert:
			case SwfBlendModeData.Types.Overlay:
			case SwfBlendModeData.Types.Hardlight:
				return CheckAndGetShader(masked ? MaskedGrabShader : SimpleGrabShader);
			default:
				throw new UnityException(string.Format(
					"SwfSettings. Incorrect blend type: {0}",
					blend_type));
			}
		}

		Shader CheckAndGetShader(Shader shader) {
			if ( !shader ) {
				FillShadersCache();
			}
			return CheckExistsShader(shader);
		}

		static Shader CheckExistsShader(Shader shader) {
			if ( !shader ) {
				throw new UnityException("SwfSettings. Shader not found");
			}
			return shader;
		}

		static Material FillMaterial(
			Material material, SwfBlendModeData.Types blend_type, int stencil_id)
		{
			switch ( blend_type ) {
			case SwfBlendModeData.Types.Normal:
				material.SetInt("_BlendOp" , (int)BlendOp.Add);
				material.SetInt("_SrcBlend", (int)BlendMode.One);
				material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
				break;
			case SwfBlendModeData.Types.Multiply:
				material.SetInt("_BlendOp" , (int)BlendOp.Add);
				material.SetInt("_SrcBlend", (int)BlendMode.DstColor);
				material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
				break;
			case SwfBlendModeData.Types.Screen:
				material.SetInt("_BlendOp" , (int)BlendOp.Add);
				material.SetInt("_SrcBlend", (int)BlendMode.OneMinusDstColor);
				material.SetInt("_DstBlend", (int)BlendMode.One);
				break;
			case SwfBlendModeData.Types.Lighten:
				material.SetInt("_BlendOp" , (int)BlendOp.Max);
				material.SetInt("_SrcBlend", (int)BlendMode.One);
				material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
				break;
			case SwfBlendModeData.Types.Darken:
				material.SetInt("_BlendOp" , (int)BlendOp.Add);
				material.SetInt("_SrcBlend", (int)BlendMode.One);
				material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
				material.EnableKeyword("SWF_DARKEN_BLEND");
				break;
			case SwfBlendModeData.Types.Difference:
				material.SetInt("_BlendOp" , (int)BlendOp.Add);
				material.SetInt("_SrcBlend", (int)BlendMode.One);
				material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
				material.EnableKeyword("SWF_DIFFERENCE_BLEND");
				break;
			case SwfBlendModeData.Types.Add:
				material.SetInt("_BlendOp" , (int)BlendOp.Add);
				material.SetInt("_SrcBlend", (int)BlendMode.One);
				material.SetInt("_DstBlend", (int)BlendMode.One);
				break;
			case SwfBlendModeData.Types.Subtract:
				material.SetInt("_BlendOp" , (int)BlendOp.ReverseSubtract);
				material.SetInt("_SrcBlend", (int)BlendMode.One);
				material.SetInt("_DstBlend", (int)BlendMode.One);
				break;
			case SwfBlendModeData.Types.Invert:
				material.SetInt("_BlendOp" , (int)BlendOp.Add);
				material.SetInt("_SrcBlend", (int)BlendMode.One);
				material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
				material.EnableKeyword("SWF_INVERT_BLEND");
				break;
			case SwfBlendModeData.Types.Overlay:
				material.SetInt("_BlendOp" , (int)BlendOp.Add);
				material.SetInt("_SrcBlend", (int)BlendMode.One);
				material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
				material.EnableKeyword("SWF_OVERLAY_BLEND");
				break;
			case SwfBlendModeData.Types.Hardlight:
				material.SetInt("_BlendOp" , (int)BlendOp.Add);
				material.SetInt("_SrcBlend", (int)BlendMode.One);
				material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
				material.EnableKeyword("SWF_HARDLIGHT_BLEND");
				break;
			default:
				throw new UnityException(string.Format(
					"SwfSettings. Incorrect blend type: {0}",
					blend_type));
			}
			material.SetInt("_StencilID", stencil_id);
			return material;
		}

		// ---------------------------------------------------------------------
		//
		// Functions
		//
		// ---------------------------------------------------------------------

		public Material GetSimpleMaterial(SwfBlendModeData.Types blend_type) {
			return LoadOrCreateMaterial(
				CheckAndGetShader(SelectShader(false, blend_type)),
				(dir_path, filename) => {
					return string.Format(
						"{0}/{1}_{2}.mat",
						dir_path, filename, blend_type);
				},
				material => FillMaterial(material, blend_type, 0));
		}

		public Material GetMaskedMaterial(SwfBlendModeData.Types blend_type, int stencil_id) {
			return LoadOrCreateMaterial(
				CheckAndGetShader(SelectShader(true, blend_type)),
				(dir_path, filename) => {
					return string.Format(
						"{0}/{1}_{2}_{3}.mat",
						dir_path, filename, blend_type, stencil_id);
				},
				material => FillMaterial(material, blend_type, stencil_id));
		}

		public Material GetIncrMaskMaterial() {
			return LoadOrCreateMaterial(
				CheckAndGetShader(IncrMaskShader),
				(dir_path, filename) => {
					return string.Format(
						"{0}/{1}.mat",
						dir_path, filename);
				},
				material => material);
		}

		public Material GetDecrMaskMaterial() {
			return LoadOrCreateMaterial(
				CheckAndGetShader(DecrMaskShader),
				(dir_path, filename) => {
					return string.Format(
						"{0}/{1}.mat",
						dir_path, filename);
				},
				material => material);
		}

		// ---------------------------------------------------------------------
		//
		// Messages
		//
		// ---------------------------------------------------------------------

		void Reset() {
			Settings = SwfSettingsData.identity;
			FillShadersCache();
			PregenerateMaterials();
		}

		public static SwfSettings GetHolder() {
			var holder = LoadFirstAssetByFilter<SwfSettings>("t:SwfSettings");
			if ( !holder ) {
				throw new UnityException("SwfSettings. SwfSettings asset not found");
			}
			return holder;
		}

		public static SwfSettingsData GetDefault() {
			return GetHolder().Settings;
		}
	#endif
	}
}