using UnityEngine;
using UnityEditor;
using System.IO;

namespace FlashTools.Internal {
	[CustomEditor(typeof(SwfAnimationAsset)), CanEditMultipleObjects]
	public class SwfAnimationAssetEditor : Editor {
		SwfAnimationAsset _asset           = null;
		bool              _settingsFoldout = false;

		void OverriddenSettingsToDefault() {
			if ( _asset ) {
				_asset.Overridden = SwfConverterSettings.GetDefaultSettings();
			}
		}

		void RevertOverriddenSettings() {
			if ( _asset ) {
				_asset.Overridden = _asset.Settings;
			}
		}

		void ApplyOverriddenSettings() {
			if ( _asset ) {
				if ( File.Exists(GetSwfPath()) ) {
					_asset.Settings = _asset.Overridden;
					ReconvertAnimation();
				} else {
					Debug.LogErrorFormat(
						"Swf source for animation not found: '{0}'",
						GetSwfPath());
					RevertOverriddenSettings();
				}
			}
		}

		void ReconvertAnimation() {
			if ( _asset && _asset.Atlas ) {
				AssetDatabase.DeleteAsset(
					AssetDatabase.GetAssetPath(_asset.Atlas));
				_asset.Atlas = null;
			}
			AssetDatabase.ImportAsset(
				GetSwfPath(),
				ImportAssetOptions.ForceUpdate);
		}

		void ShowUnappliedDialog() {
			var title =
				"Unapplied swf animation settings";
			var message = string.Format(
				"Unapplied swf animation settings for '{0}'",
				GetAssetPath());
			if ( EditorUtility.DisplayDialog(title, message, "Apply", "Revert") ) {
				ApplyOverriddenSettings();
			} else {
				RevertOverriddenSettings();
			}
		}

		GameObject CreateAnimationGO() {
			if ( _asset ) {
				var anim_go = new GameObject(_asset.name);
				anim_go.AddComponent<MeshFilter>();
				anim_go.AddComponent<MeshRenderer>();
				anim_go.AddComponent<SwfAnimation>().asset = _asset;
				anim_go.AddComponent<SwfAnimationController>();
				return anim_go;
			}
			return null;
		}

		void CreateAnimationPrefab() {
			var anim_go = CreateAnimationGO();
			if ( anim_go ) {
				var prefab_path = GetPrefabPath();
				if ( !string.IsNullOrEmpty(prefab_path) ) {
					var prefab = AssetDatabase.LoadMainAssetAtPath(prefab_path);
					if ( !prefab ) {
						prefab = PrefabUtility.CreateEmptyPrefab(prefab_path);
					}
					PrefabUtility.ReplacePrefab(
						anim_go,
						prefab,
						ReplacePrefabOptions.ConnectToPrefab);
				}
				GameObject.DestroyImmediate(anim_go, true);
			}
		}

		void CreateAnimationOnScene() {
			var anim_go = CreateAnimationGO();
			if ( anim_go ) {
				Undo.RegisterCreatedObjectUndo(anim_go, "Instance SwfAnimation");
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

		void DrawGUISettings() {
			var last_gui_enabled = GUI.enabled;
			GUI.enabled = false;
			var script_prop = serializedObject.FindProperty("m_Script");
			if ( script_prop != null ) {
				EditorGUILayout.PropertyField(script_prop, true);
			}
			var atlas_prop = serializedObject.FindProperty("Atlas");
			if ( atlas_prop != null ) {
				EditorGUILayout.PropertyField(atlas_prop, true);
			}
			var frames_prop = serializedObject.FindProperty("Frames");
			if ( frames_prop != null && frames_prop.isArray ) {
				EditorGUILayout.IntField("Frame count", frames_prop.arraySize);
			}
			GUI.enabled = last_gui_enabled;
			_settingsFoldout = EditorGUILayout.Foldout(_settingsFoldout, "Settings");
			if ( _settingsFoldout ) {
				var it = serializedObject.FindProperty("Overridden");
				while ( it.NextVisible(true) ) {
					EditorGUILayout.PropertyField(it, true);
				}
				DrawGUISettingsControls();
			}
		}

		void DrawGUISettingsControls() {
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			{
				var default_settings = SwfConverterSettings.GetDefaultSettings();
				GUI.enabled = !_asset.Overridden.CheckEquals(default_settings);
				if ( GUILayout.Button("Default") ) {
					OverriddenSettingsToDefault();
				}
				GUI.enabled = !_asset.Overridden.CheckEquals(_asset.Settings);
				if ( GUILayout.Button("Revert") ) {
					RevertOverriddenSettings();
				}
				GUI.enabled = !_asset.Overridden.CheckEquals(_asset.Settings);
				if ( GUILayout.Button("Apply") ) {
					ApplyOverriddenSettings();
				}
				GUI.enabled = true;
			}
			GUILayout.EndHorizontal();
		}

		void DrawGUIAnimation() {
			GUILayout.BeginHorizontal();
			{
				if ( GUILayout.Button("Create prefab") ) {
					CreateAnimationPrefab();
				}
				if ( GUILayout.Button("Instance to scene") ) {
					CreateAnimationOnScene();
				}
			}
			GUILayout.EndHorizontal();
		}

		// ---------------------------------------------------------------------
		//
		// Messages
		//
		// ---------------------------------------------------------------------

		void OnEnable() {
			_asset           = target as SwfAnimationAsset;
			_settingsFoldout = _asset && !_asset.Settings.CheckEquals(
				SwfConverterSettings.GetDefaultSettings());
		}

		void OnDisable() {
			if ( _asset && !_asset.Settings.CheckEquals(_asset.Overridden) ) {
				ShowUnappliedDialog();
			}
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();
			DrawGUISettings();
			DrawGUIAnimation();
			if ( GUI.changed ) {
				serializedObject.ApplyModifiedProperties();
			}
		}
	}
}