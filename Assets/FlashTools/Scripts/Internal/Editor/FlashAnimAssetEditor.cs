using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using System;

namespace FlashTools.Internal {
	[CustomEditor(typeof(FlashAnimAsset))]
	public class FlashAnimAssetEditor : Editor {
		FlashAnimAsset _asset = null;

		void CreateFlashAnimOnScene() {
			var anim_go = new GameObject("FlashAnim");
			try {
				CreateFlashAnim(anim_go);
			} catch ( Exception e ) {
				Debug.LogErrorFormat("Create animation error: {0}", e.Message);
				DestroyImmediate(anim_go, true);
			}
			Undo.RegisterCreatedObjectUndo(anim_go, "Create Animation");
		}

		void CreateFlashAnim(GameObject anim_go) {
			var flash_anim   = anim_go.AddComponent<FlashAnim>();
			flash_anim.Asset = _asset;

			var mesh_filter = anim_go.AddComponent<MeshFilter>();
			mesh_filter.mesh = null;

			var material = new Material(Shader.Find("Sprites/Default"));
			material.SetTexture("_MainTex", _asset.Data.Atlas);

			var mesh_renderer                  = anim_go.AddComponent<MeshRenderer>();
			mesh_renderer.sharedMaterial       = material;
			mesh_renderer.useLightProbes       = false;
			mesh_renderer.receiveShadows       = false;
			mesh_renderer.shadowCastingMode    = ShadowCastingMode.Off;
			mesh_renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
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
			if ( GUILayout.Button("Create animation on scene") ) {
				CreateFlashAnimOnScene();
			}
		}
	}
}