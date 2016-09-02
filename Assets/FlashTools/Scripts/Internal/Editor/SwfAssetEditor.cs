using UnityEngine;
using UnityEditor;

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace FlashTools.Internal {
	[CustomEditor(typeof(SwfAsset)), CanEditMultipleObjects]
	public class SwfAssetEditor : Editor {
		List<SwfAsset> _assets          = new List<SwfAsset>();
		bool           _settingsFoldout = false;

		//
		//
		//

		static string GetAssetPath(SwfAsset asset) {
			return asset
				? AssetDatabase.GetAssetPath(asset)
				: string.Empty;
		}

		static string GetSwfPath(SwfAsset asset) {
			var asset_path = GetAssetPath(asset);
			return string.IsNullOrEmpty(asset_path)
				? string.Empty
				: SwfEditorUtils.GetSwfPathFromSettingsPath(asset_path);
		}

		//
		//
		//

		static void RevertOverriddenSettings(SwfAsset asset) {
			asset.Overridden = asset.Settings;
		}

		static void OverriddenSettingsToDefault(SwfAsset asset) {
			asset.Overridden = SwfSettings.GetDefault();
		}

		static void ApplyOverriddenSettings(SwfAsset asset) {
			if ( File.Exists(GetSwfPath(asset)) ) {
				asset.Settings = asset.Overridden;
				ReconvertAsset(asset);
			} else {
				Debug.LogErrorFormat(
					"Swf source for asset not found: '{0}'",
					GetSwfPath(asset));
				RevertOverriddenSettings(asset);
			}
		}

		static void ReconvertAsset(SwfAsset asset) {
			AssetDatabase.ImportAsset(GetSwfPath(asset));
		}

		//
		//
		//

		void AllAssetsForeach(Action<SwfAsset> act) {
			foreach ( var asset in _assets ) {
				act(asset);
			}
		}

		void AllOverriddenSettingsToDefault() {
			AllAssetsForeach(p => OverriddenSettingsToDefault(p));
		}

		void RevertAllOverriddenSettings() {
			AllAssetsForeach(p => RevertOverriddenSettings(p));
		}

		void ApplyAllOverriddenSettings() {
			AllAssetsForeach(p => ApplyOverriddenSettings(p));
		}

		//
		//
		//

		void ShowUnappliedDialog() {
			var unapplied = _assets
				.Where(p => !p.Settings.CheckEquals(p.Overridden))
				.ToArray();
			if ( unapplied.Length > 0 ) {
				var title =
					"Unapplied swf asset settings";
				var message = unapplied.Length == 1
					? string.Format(
						"Unapplied swf asset settings for '{0}'",
						GetAssetPath(unapplied[0]))
					: string.Format(
						"Unapplied multiple({0}) swf asset settings",
						unapplied.Length);
				if ( EditorUtility.DisplayDialog(title, message, "Apply", "Revert") ) {
					ApplyAllOverriddenSettings();
				} else {
					RevertAllOverriddenSettings();
				}
			}
		}

		void DrawGUISettings() {
			SwfEditorUtils.DoWithEnabledGUI(false, () => {
				var script_prop = SwfEditorUtils.GetPropertyByName(serializedObject, "m_Script");
				EditorGUILayout.PropertyField(script_prop, true);

				var atlas_prop = SwfEditorUtils.GetPropertyByName(serializedObject, "Atlas");
				EditorGUILayout.PropertyField(atlas_prop, true);

				var clips_prop = SwfEditorUtils.GetPropertyByName(serializedObject, "Clips");
				if ( clips_prop.isArray ) {
					SwfEditorUtils.DoWithMixedValue(
						clips_prop.hasMultipleDifferentValues, () => {
							EditorGUILayout.IntField("Clips count", clips_prop.arraySize);
						});
				}
			});
			_settingsFoldout = EditorGUILayout.Foldout(_settingsFoldout, "Settings");
			if ( _settingsFoldout ) {
				var it = SwfEditorUtils.GetPropertyByName(serializedObject, "Overridden");
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
				var default_settings = SwfSettings.GetDefault();
				SwfEditorUtils.DoWithEnabledGUI(
					_assets.Any(p => !p.Overridden.CheckEquals(default_settings)), () => {
						if ( GUILayout.Button("Default") ) {
							AllOverriddenSettingsToDefault();
						}
					});
				SwfEditorUtils.DoWithEnabledGUI(
					_assets.Any(p => !p.Overridden.CheckEquals(p.Settings)), () => {
						if ( GUILayout.Button("Revert") ) {
							RevertAllOverriddenSettings();
						}
						if ( GUILayout.Button("Apply") ) {
							ApplyAllOverriddenSettings();
						}
					});
			}
			GUILayout.EndHorizontal();
		}

		// ---------------------------------------------------------------------
		//
		// Messages
		//
		// ---------------------------------------------------------------------

		void OnEnable() {
			_assets = targets
				.OfType<SwfAsset>()
				.ToList();
			_settingsFoldout =
				_assets.Any(p => !p.Settings.CheckEquals(SwfSettings.GetDefault()));
		}

		void OnDisable() {
			ShowUnappliedDialog();
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();
			DrawGUISettings();
			if ( GUI.changed ) {
				serializedObject.ApplyModifiedProperties();
			}
		}
	}
}