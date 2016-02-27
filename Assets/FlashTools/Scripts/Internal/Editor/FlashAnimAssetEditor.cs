using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using System;
using System.IO;

namespace FlashTools.Internal {
	[CustomEditor(typeof(FlashAnimAsset))]
	public class FlashAnimAssetEditor : Editor {
		FlashAnimAsset _asset = null;

		static void ApplySettings(FlashAnimAsset asset) {
			if ( asset.Atlas ) {
				AssetDatabase.DeleteAsset(
					AssetDatabase.GetAssetPath(asset.Atlas));
			}
			AssetDatabase.ImportAsset(
				AssetDatabase.GetAssetPath(asset),
				ImportAssetOptions.ForceUncompressedImport);
		}

		static void CreateFlashAnim(FlashAnimAsset asset, GameObject anim_go) {
			var mesh_renderer = anim_go.AddComponent<MeshRenderer>();
			mesh_renderer.sharedMaterial = null;
			mesh_renderer.useLightProbes = false;
			mesh_renderer.receiveShadows = false;
			mesh_renderer.shadowCastingMode = ShadowCastingMode.Off;
			mesh_renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
			anim_go.AddComponent<MeshFilter>().sharedMesh = null;
			anim_go.AddComponent<FlashAnim>().Asset = asset;
		}

		// ------------------------------------------------------------------------
		//
		// Public
		//
		// ------------------------------------------------------------------------

		public static void CreateFlashAnimPrefab(FlashAnimAsset asset) {
			var prefab_path = Path.ChangeExtension(AssetDatabase.GetAssetPath(asset), ".prefab");
			var flash_anim_go = CreateFlashAnimOnScene(asset);
			PrefabUtility.CreatePrefab(prefab_path, flash_anim_go);
			GameObject.DestroyImmediate(flash_anim_go, true);
		}

		public static GameObject CreateFlashAnimOnScene(FlashAnimAsset asset) {
			var anim_go = new GameObject("FlashAnim");
			try {
				CreateFlashAnim(asset, anim_go);
			} catch ( Exception e ) {
				Debug.LogErrorFormat("Create animation error: {0}", e.Message);
				DestroyImmediate(anim_go, true);
			}
			Undo.RegisterCreatedObjectUndo(anim_go, "Create Animation");
			return anim_go;
		}

		// ------------------------------------------------------------------------
		//
		// Messages
		//
		// ------------------------------------------------------------------------

		void OnEnable() {
			_asset = target as FlashAnimAsset;
		}

		public override void OnInspectorGUI() {
			DrawDefaultInspector();
			if ( GUILayout.Button("Apply settings") ) {
				ApplySettings(_asset);
			}
			GUILayout.BeginHorizontal();
				if ( GUILayout.Button("Create animation prefab") ) {
					CreateFlashAnimPrefab(_asset);
				}
				if ( GUILayout.Button("Create animation on scene") ) {
					CreateFlashAnimOnScene(_asset);
				}
			GUILayout.EndHorizontal();
		}
	}
}
