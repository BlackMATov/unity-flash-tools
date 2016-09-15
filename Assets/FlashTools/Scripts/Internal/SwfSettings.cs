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

		[HideInInspector] public Material       SimpleMaterial;
		[HideInInspector] public Material       IncrMaskMaterial;
		[HideInInspector] public Material       DecrMaskMaterial;
		[HideInInspector] public List<Material> MaskedMaterials;

	#if UNITY_EDITOR

		// ---------------------------------------------------------------------
		//
		// Internal
		//
		// ---------------------------------------------------------------------

		const string SwfSimpleMatName    = "SwfSimpleMat";
		const string SwfIncrMaskMatName  = "SwfIncrMaskMat";
		const string SwfDecrMaskMatName  = "SwfDecrMaskMat";
		const string SwfMaskedMatNameFmt = "SwfMaskedMat_{0}";

		void FillMaterialsCache() {
			SimpleMaterial   = SafeLoadMaterial(SwfSimpleMatName,   true);
			IncrMaskMaterial = SafeLoadMaterial(SwfIncrMaskMatName, true);
			DecrMaskMaterial = SafeLoadMaterial(SwfDecrMaskMatName, true);
			MaskedMaterials  = new List<Material>();
			for ( var i = 0; i < int.MaxValue; ++i ) {
				var mat = SafeLoadMaterial(string.Format(SwfMaskedMatNameFmt, i), false);
				if ( mat ) {
					MaskedMaterials.Add(mat);
				} else {
					break;
				}
			}
			EditorUtility.SetDirty(this);
			AssetDatabase.SaveAssets();
		}

		Material SafeLoadMaterial(string name, bool exception) {
			var filter   = string.Format("t:Material {0}", name);
			var material = LoadFirstAssetByFilter<Material>(filter);
			if ( !material && exception ) {
				throw new UnityException(string.Format(
					"SwfSettings. Material not found: {0}",
					name));
			}
			return material;
		}

		Material CheckExistsMaterial(Material material) {
			if ( !material ) {
				throw new UnityException("SwfSettings. Material not found");
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

		public Material GetMaskedMaterial(int stencil_id) {
			if ( MaskedMaterials == null || stencil_id >= MaskedMaterials.Count ) {
				FillMaterialsCache();
			}
			if ( stencil_id < 0 || stencil_id >= MaskedMaterials.Count ) {
				throw new UnityException(string.Format(
					"SwfSettings. Unsupported stencil id: {0}",
					stencil_id));
			}
			return CheckExistsMaterial(MaskedMaterials[stencil_id]);
		}

		public Material GetSimpleMaterial() {
			if ( !SimpleMaterial ) {
				FillMaterialsCache();
			}
			return CheckExistsMaterial(SimpleMaterial);
		}

		public Material GetIncrMaskMaterial() {
			if ( !IncrMaskMaterial ) {
				FillMaterialsCache();
			}
			return CheckExistsMaterial(IncrMaskMaterial);
		}

		public Material GetDecrMaskMaterial() {
			if ( !DecrMaskMaterial ) {
				FillMaterialsCache();
			}
			return CheckExistsMaterial(DecrMaskMaterial);
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