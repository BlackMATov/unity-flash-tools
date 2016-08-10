using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using System;
using System.IO;

namespace FlashTools.Internal {
	[CustomEditor(typeof(SwfAnimationAsset))]
	public class SwfAnimationAssetEditor : Editor {
		SwfAnimationAsset _asset = null;

		static void ApplySettings(SwfAnimationAsset asset) {
			if ( asset.Atlas ) {
				AssetDatabase.DeleteAsset(
					AssetDatabase.GetAssetPath(asset.Atlas));
				asset.Atlas = null;
			}
			var swf_asset_path = Path.ChangeExtension(
				AssetDatabase.GetAssetPath(asset), "swf");
			AssetDatabase.ImportAsset(
				swf_asset_path,
				ImportAssetOptions.ForceUncompressedImport);
		}

		void CreateAnimationPrefab() {
			//TODO: IMPLME
		}

		void CreateAnimationOnScene() {
			if ( _asset ) {
				var anim_go = new GameObject(_asset.name);
				var mesh_renderer = anim_go.AddComponent<MeshRenderer>();
				mesh_renderer.sharedMaterial = null;
				mesh_renderer.useLightProbes = false;
				mesh_renderer.receiveShadows = false;
				mesh_renderer.shadowCastingMode = ShadowCastingMode.Off;
				mesh_renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
				anim_go.AddComponent<MeshFilter>().sharedMesh = null;
				anim_go.AddComponent<SwfAnimation>().Asset = _asset;
				Undo.RegisterCreatedObjectUndo(anim_go, "Create SwfAnimation");
			}
		}

		// ------------------------------------------------------------------------
		//
		// Messages
		//
		// ------------------------------------------------------------------------

		void OnEnable() {
			_asset = target as SwfAnimationAsset;
		}

		public override void OnInspectorGUI() {
			DrawDefaultInspector();
			if ( GUILayout.Button("Apply settings") ) {
				ApplySettings(_asset);
			}
			GUILayout.BeginHorizontal();
			if ( GUILayout.Button("Create animation prefab") ) {
				CreateAnimationPrefab();
			}
			if ( GUILayout.Button("Create animation on scene") ) {
				CreateAnimationOnScene();
			}
			GUILayout.EndHorizontal();
		}
	}
}
