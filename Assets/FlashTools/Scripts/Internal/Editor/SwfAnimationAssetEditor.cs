using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

namespace FlashTools.Internal {
	[CustomEditor(typeof(SwfAnimationAsset))]
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
				anim_go.AddComponent<SwfAnimation>().Asset = _asset;
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

		void DrawGUISettings() {
			GUI.enabled = false;
			EditorGUILayout.PropertyField(serializedObject.FindProperty("Atlas"));
			GUI.enabled = true;
			_settingsFoldout = EditorGUILayout.Foldout(_settingsFoldout, "Settings");
			if ( _settingsFoldout ) {
				var it = serializedObject.FindProperty("Overridden");
				while ( it.NextVisible(true) ) {
					if ( it.name == "MaxAtlasSize" && _asset.Overridden.AtlasPowerOfTwo ) {
						if ( !Mathf.IsPowerOfTwo(it.intValue) ) {
							it.intValue = Mathf.ClosestPowerOfTwo(it.intValue);
							serializedObject.ApplyModifiedProperties();
						}
						var values = new int[] {32, 64, 128, 256, 512, 1024, 2048, 4096, 8192};
						var names = values.Select(p => new GUIContent(p.ToString())).ToArray();
						EditorGUILayout.IntPopup(it, names, values);
					} else {
						EditorGUILayout.PropertyField(it);
					}
				}
				DrawGUISettingsControls();
			}
		}

		void DrawGUISettingsControls() {
			GUILayout.BeginHorizontal();
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
				if ( GUILayout.Button("Create animation prefab") ) {
					CreateAnimationPrefab();
				}
				if ( GUILayout.Button("Create animation on scene") ) {
					CreateAnimationOnScene();
				}
			}
			GUILayout.EndHorizontal();
		}

		// ------------------------------------------------------------------------
		//
		// Messages
		//
		// ------------------------------------------------------------------------

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