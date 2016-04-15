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
			}
			if ( GUILayout.Button("Create animation on scene") ) {
			}
			GUILayout.EndHorizontal();
		}
	}
}
