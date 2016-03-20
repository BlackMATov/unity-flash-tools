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
			AssetDatabase.ImportAsset(
				AssetDatabase.GetAssetPath(asset),
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
