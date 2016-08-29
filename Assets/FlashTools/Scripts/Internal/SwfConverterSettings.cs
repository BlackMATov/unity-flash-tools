using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

namespace FlashTools.Internal {
	public class SwfConverterSettings : ScriptableObject {

		public SwfSettings DefaultSettings;

		[HideInInspector] public Material       SimpleMaterial;
		[HideInInspector] public Material       IncrMaskMaterial;
		[HideInInspector] public Material       DecrMaskMaterial;
		[HideInInspector] public List<Material> MaskedMaterials;

	#if UNITY_EDITOR

		// ---------------------------------------------------------------------
		//
		// Private
		//
		// ---------------------------------------------------------------------

		const string DefaultSettingsName         = "SwfConverterSettings.asset";
		const string SwfSimpleMatRelativePath    = "Materials/SwfSimpleMat.mat";
		const string SwfIncrMaskMatRelativePath  = "Materials/SwfIncrMaskMat.mat";
		const string SwfDecrMaskMatRelativePath  = "Materials/SwfDecrMaskMat.mat";
		const string SwfMaskedMatRelativePathFmt = "Materials/SwfMaskedMat_{0}.mat";

		void FillMaterialsCache() {
			var folder       = Path.GetDirectoryName(AssetDatabase.GetAssetPath(this));
			SimpleMaterial   = SafeLoadMaterial(Path.Combine(folder, SwfSimpleMatRelativePath),   true);
			IncrMaskMaterial = SafeLoadMaterial(Path.Combine(folder, SwfIncrMaskMatRelativePath), true);
			DecrMaskMaterial = SafeLoadMaterial(Path.Combine(folder, SwfDecrMaskMatRelativePath), true);
			MaskedMaterials  = new List<Material>();
			for ( var i = 0; i < int.MaxValue; ++i ) {
				var relative_path = string.Format(SwfMaskedMatRelativePathFmt, i);
				var mat = SafeLoadMaterial(Path.Combine(folder, relative_path), false);
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
					"SwfConverterSettings. Material not found: {0}",
					path));
			}
			return material;
		}

		Material CheckExistsMaterial(Material material) {
			if ( !material ) {
				throw new UnityException("SwfConverterSettings. Material not found");
			}
			return material;
		}

		// ---------------------------------------------------------------------
		//
		// Functions
		//
		// ---------------------------------------------------------------------

		public Material GetMaskedMaterial(int stencil_id) {
			if ( MaskedMaterials == null || stencil_id < MaskedMaterials.Count ) {
				FillMaterialsCache();
			}
			if ( stencil_id < 0 || stencil_id >= MaskedMaterials.Count ) {
				throw new UnityException(string.Format(
					"SwfConverterSettings. Unsupported stencil id: {0}",
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
			DefaultSettings = SwfSettings.identity;
			FillMaterialsCache();
		}

		public static SwfConverterSettings GetDefaultConverter() {
			var asset_guids = AssetDatabase.FindAssets("t:SwfConverterSettings");
			foreach ( var asset_guid in asset_guids ) {
				var converter_settings = AssetDatabase.LoadAssetAtPath<SwfConverterSettings>(
					AssetDatabase.GUIDToAssetPath(asset_guid));
				if ( converter_settings ) {
					return converter_settings;
				}
			}
			throw new UnityException("SwfConverterSettings asset not found");
		}

		public static SwfSettings GetDefaultSettings() {
			return GetDefaultConverter().DefaultSettings;
		}
	#endif
	}
}