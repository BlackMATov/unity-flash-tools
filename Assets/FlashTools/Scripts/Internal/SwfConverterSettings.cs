using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

namespace FlashTools.Internal {
	public class SwfConverterSettings : ScriptableObject {

		// ---------------------------------------------------------------------
		//
		// Constants
		//
		// ---------------------------------------------------------------------

		const string DefaultBasePath     = "Assets/FlashTools/Resources/";
		const string DefaultSettingsPath = DefaultBasePath + "SwfConverterSettings.asset";
		const string SwfSimpleMatPath    = DefaultBasePath + "Materials/SwfSimpleMat.mat";
		const string SwfIncrMaskMatPath  = DefaultBasePath + "Materials/SwfIncrMaskMat.mat";
		const string SwfDecrMaskMatPath  = DefaultBasePath + "Materials/SwfDecrMaskMat.mat";
		const string SwfMaskedMatPathFmt = DefaultBasePath + "Materials/SwfMaskedMat_{0}.mat";

		// ---------------------------------------------------------------------
		//
		// Properties
		//
		// ---------------------------------------------------------------------

		[Header("Settings")]
		public SwfSettings    DefaultSettings;

		[Header("Materials cache")]
		public Material       SimpleMaterial;
		public Material       IncrMaskMaterial;
		public Material       DecrMaskMaterial;
		public List<Material> MaskedMaterials;

	#if UNITY_EDITOR

		// ---------------------------------------------------------------------
		//
		// Private
		//
		// ---------------------------------------------------------------------

		void FillMaterialsCache() {
			SimpleMaterial   = SafeLoadMaterial(SwfSimpleMatPath,   true);
			IncrMaskMaterial = SafeLoadMaterial(SwfIncrMaskMatPath, true);
			DecrMaskMaterial = SafeLoadMaterial(SwfDecrMaskMatPath, true);
			MaskedMaterials  = new List<Material>();
			for ( var i = 0; i < int.MaxValue; ++i ) {
				var mat = SafeLoadMaterial(string.Format(SwfMaskedMatPathFmt, i), false);
				if ( mat ) {
					MaskedMaterials.Add(mat);
				} else {
					break;
				}
			}
			EditorUtility.SetDirty(this);
			AssetDatabase.SaveAssets();
		}

		Material SafeLoadMaterial(string path, bool exception) {
			var material = AssetDatabase.LoadAssetAtPath<Material>(path);
			if ( !material && exception ) {
				throw new UnityException(string.Format(
					"SwfManager. Material not found: {0}",
					path));
			}
			if ( material ) {
				material.hideFlags = HideFlags.HideInInspector;
			}
			return material;
		}

		// ---------------------------------------------------------------------
		//
		// Functions
		//
		// ---------------------------------------------------------------------

		public Material GetMaskedMaterial(int stencil_id) {
			if ( stencil_id < 0 || stencil_id >= MaskedMaterials.Count ) {
				throw new UnityException(string.Format(
					"SwfConverterSettings. Unsupported stencil id: {0}",
					stencil_id));
			}
			return MaskedMaterials[stencil_id];
		}

		public Material GetSimpleMaterial() {
			return SimpleMaterial;
		}

		public Material GetIncrMaskMaterial() {
			return IncrMaskMaterial;
		}

		public Material GetDecrMaskMaterial() {
			return DecrMaskMaterial;
		}

		// ---------------------------------------------------------------------
		//
		// Messages
		//
		// ---------------------------------------------------------------------

		void Reset() {
			DefaultSettings = SwfSettings.identity;
			FillMaterialsCache();
		}

		public static SwfConverterSettings GetDefaultConverter() {
			var settings_path = DefaultSettingsPath;
			var settings = AssetDatabase.LoadAssetAtPath<SwfConverterSettings>(settings_path);
			if ( !settings ) {
				settings = ScriptableObject.CreateInstance<SwfConverterSettings>();
				CreateAssetDatabaseFolders(Path.GetDirectoryName(settings_path));
				AssetDatabase.CreateAsset(settings, settings_path);
				AssetDatabase.SaveAssets();
			}
			return settings;
		}

		public static SwfSettings GetDefaultSettings() {
			return GetDefaultConverter().DefaultSettings;
		}

		static void CreateAssetDatabaseFolders(string path) {
			if ( !AssetDatabase.IsValidFolder(path) ) {
				var parent = Path.GetDirectoryName(path);
				if ( !string.IsNullOrEmpty(parent) ) {
					CreateAssetDatabaseFolders(parent);
				}
				AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
			}
		}
	#endif
	}
}