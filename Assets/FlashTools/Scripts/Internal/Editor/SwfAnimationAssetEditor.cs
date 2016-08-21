using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using System;
using System.IO;

namespace FlashTools.Internal {
	[CustomEditor(typeof(SwfAnimationAsset))]
	public class SwfAnimationAssetEditor : Editor {
		SwfAnimationAsset _asset = null;

		void Reconvert() {
			if ( _asset && _asset.Atlas ) {
				AssetDatabase.DeleteAsset(
					AssetDatabase.GetAssetPath(_asset.Atlas));
				_asset.Atlas = null;
			}
			if ( !_asset.OverrideSettings ) {
				_asset.OverriddenSettings =
					SwfConverterSettings.LoadOrCreate().DefaultSettings;
			}
			AssetDatabase.ImportAsset(
				GetSwfPath(),
				ImportAssetOptions.ForceUpdate);
		}

		GameObject CreateAnimationGO() {
			if ( _asset ) {
				var anim_go = new GameObject(_asset.name);
				anim_go.AddComponent<MeshFilter>();
				anim_go.AddComponent<MeshRenderer>();
				anim_go.AddComponent<SwfAnimation>().Asset = _asset;
				return anim_go;
			}
			return null;
		}

		void CreateAnimationPrefab() {
			var anim_go = CreateAnimationGO();
			var prefab_path = GetPrefabPath();
			if ( anim_go && !string.IsNullOrEmpty(prefab_path) ) {
				var prefab = AssetDatabase.LoadMainAssetAtPath(prefab_path);
				if ( !prefab ) {
					prefab = PrefabUtility.CreateEmptyPrefab(prefab_path);
				}
				PrefabUtility.ReplacePrefab(
					anim_go,
					prefab,
					ReplacePrefabOptions.ConnectToPrefab);
				Undo.RegisterCreatedObjectUndo(anim_go, "Create SwfAnimation");
			}
		}

		void CreateAnimationOnScene() {
			var anim_go = CreateAnimationGO();
			if ( anim_go ) {
				Undo.RegisterCreatedObjectUndo(anim_go, "Create SwfAnimation");
			}
		}

		string GetAssetPath() {
			return _asset
				? AssetDatabase.GetAssetPath(_asset)
				: string.Empty;
		}

		string GetSwfPath() {
			var asset_path = GetAssetPath();
			return string.IsNullOrEmpty(asset_path)
				? string.Empty
				: Path.ChangeExtension(asset_path, ".swf");
		}

		string GetPrefabPath() {
			var asset_path = GetAssetPath();
			return string.IsNullOrEmpty(asset_path)
				? string.Empty
				: Path.ChangeExtension(asset_path, ".prefab");
		}

		void DrawGUIControls() {
			if ( GUILayout.Button("Reconvert") ) {
				Reconvert();
			}
			GUILayout.BeginHorizontal();
			{
				if ( GUILayout.Button("Create animation prefab") ) {
					CreateAnimationPrefab();
				}
				if ( GUILayout.Button("Create animation on scene") ) {
					CreateAnimationOnScene();
				}
			}
			GUILayout.EndHorizontal();
		}

		void DrawGUIProperties() {
			serializedObject.Update();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("Atlas"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("OverrideSettings"));
			if ( _asset.OverrideSettings ) {
				EditorGUILayout.PropertyField(
					serializedObject.FindProperty("OverriddenSettings"), true);
			} else {
				GUI.enabled = false;
				var so = new SerializedObject(SwfConverterSettings.LoadOrCreate());
				EditorGUILayout.PropertyField(so.FindProperty("DefaultSettings"), true);
				GUI.enabled = true;
			}
			if ( GUI.changed ) {
				serializedObject.ApplyModifiedProperties();
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
			DrawGUIProperties();
			DrawGUIControls();
		}
	}
}