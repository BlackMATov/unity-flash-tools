using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
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

		[HideInInspector] public Material       IncrMaskMat;
		[HideInInspector] public Material       DecrMaskMat;

		[HideInInspector] public Material       SimpleMat_Add;
		[HideInInspector] public Material       SimpleMat_Normal;
		[HideInInspector] public Material       SimpleMat_Multiply;

		[HideInInspector] public List<Material> MaskedMats_Add;
		[HideInInspector] public List<Material> MaskedMats_Normal;
		[HideInInspector] public List<Material> MaskedMats_Multiply;

	#if UNITY_EDITOR

		// ---------------------------------------------------------------------
		//
		// Internal
		//
		// ---------------------------------------------------------------------

		const string SwfIncrMaskMatName          = "SwfIncrMaskMat";
		const string SwfDecrMaskMatName          = "SwfDecrMaskMat";

		const string SwfSimpleMatAddName         = "SwfSimpleMat_Add";
		const string SwfSimpleMatNormalName      = "SwfSimpleMat_Normal";
		const string SwfSimpleMatMultiplyName    = "SwfSimpleMat_Multiply";

		const string SwfMaskedMatAddNameFmt      = "SwfMaskedMat_Add_{0}";
		const string SwfMaskedMatNormalNameFmt   = "SwfMaskedMat_Normal_{0}";
		const string SwfMaskedMatMultiplyNameFmt = "SwfMaskedMat_Multiply_{0}";

		void FillMaterialsCache() {
			IncrMaskMat         = SafeLoadMaterial(SwfIncrMaskMatName, true);
			DecrMaskMat         = SafeLoadMaterial(SwfDecrMaskMatName, true);

			SimpleMat_Add       = SafeLoadMaterial(SwfSimpleMatAddName,      true);
			SimpleMat_Normal    = SafeLoadMaterial(SwfSimpleMatNormalName,   true);
			SimpleMat_Multiply  = SafeLoadMaterial(SwfSimpleMatMultiplyName, true);

			MaskedMats_Add      = SafeLoadMaterials(SwfMaskedMatAddNameFmt);
			MaskedMats_Normal   = SafeLoadMaterials(SwfMaskedMatNormalNameFmt);
			MaskedMats_Multiply = SafeLoadMaterials(SwfMaskedMatMultiplyNameFmt);

			EditorUtility.SetDirty(this);
			AssetDatabase.SaveAssets();
		}

		public Material CheckAndGetMaterial(Material material) {
			if ( !material ) {
				FillMaterialsCache();
			}
			return CheckExistsMaterial(material);
		}

		Material GetMaskedMaterial(List<Material> materials, int stencil_id) {
			if ( materials == null || stencil_id >= materials.Count ) {
				FillMaterialsCache();
			}
			if ( stencil_id < 0 || stencil_id >= materials.Count ) {
				throw new UnityException(string.Format(
					"SwfSettings. Unsupported stencil id: {0}",
					stencil_id));
			}
			return CheckExistsMaterial(materials[stencil_id]);
		}

		static Material CheckExistsMaterial(Material material) {
			if ( !material ) {
				throw new UnityException("SwfSettings. Material not found");
			}
			return material;
		}

		static List<Material> SafeLoadMaterials(string name_fmt) {
			var result = new List<Material>();
			for ( var i = 0; i < int.MaxValue; ++i ) {
				var mat = SafeLoadMaterial(string.Format(name_fmt, i), false);
				if ( mat ) {
					result.Add(mat);
				} else {
					break;
				}
			}
			return result;
		}

		static Material SafeLoadMaterial(string name, bool exception) {
			var filter   = string.Format("t:Material {0}", name);
			var material = LoadFirstAssetByFilter<Material>(filter);
			if ( !material && exception ) {
				throw new UnityException(string.Format(
					"SwfSettings. Material not found: {0}",
					name));
			}
			return material;
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

		// ---------------------------------------------------------------------
		//
		// Functions
		//
		// ---------------------------------------------------------------------

		public Material GetIncrMaskMaterial() {
			return CheckAndGetMaterial(IncrMaskMat);
		}

		public Material GetDecrMaskMaterial() {
			return CheckAndGetMaterial(DecrMaskMat);
		}

		public Material GetSimpleAddMaterial() {
			return CheckAndGetMaterial(SimpleMat_Add);
		}

		public Material GetSimpleNormalMaterial() {
			return CheckAndGetMaterial(SimpleMat_Normal);
		}

		public Material GetSimpleMultiplyMaterial() {
			return CheckAndGetMaterial(SimpleMat_Multiply);
		}

		public Material GetMaskedAddMaterial(int stencil_id) {
			return GetMaskedMaterial(MaskedMats_Add, stencil_id);
		}

		public Material GetMaskedNormalMaterial(int stencil_id) {
			return GetMaskedMaterial(MaskedMats_Normal, stencil_id);
		}

		public Material GetMaskedMultiplyMaterial(int stencil_id) {
			return GetMaskedMaterial(MaskedMats_Multiply, stencil_id);
		}

		// ---------------------------------------------------------------------
		//
		// Messages
		//
		// ---------------------------------------------------------------------

		void Reset() {
			Settings = SwfSettingsData.identity;
			FillMaterialsCache();
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