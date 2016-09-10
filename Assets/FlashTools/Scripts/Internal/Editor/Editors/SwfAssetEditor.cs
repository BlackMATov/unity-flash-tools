using UnityEngine;
using UnityEditor;

using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace FlashTools.Internal {
	[CustomEditor(typeof(SwfAsset)), CanEditMultipleObjects]
	public class SwfAssetEditor : Editor {
		List<SwfAsset> _assets          = new List<SwfAsset>();
		bool           _clipsFoldout    = false;
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
				: Path.ChangeExtension(asset_path, ".swf");
		}

		static SwfSettings _defaultSettingsCache = null;
		static SwfSettings GetDefaultSettings() {
			if ( !_defaultSettingsCache ) {
				_defaultSettingsCache = SwfSettings.GetHolder();
			}
			return _defaultSettingsCache;
		}

		//
		//
		//

		static void RevertOverriddenSettings(SwfAsset asset) {
			asset.Overridden = asset.Settings;
		}

		static void OverriddenSettingsToDefault(SwfAsset asset) {
			asset.Overridden = GetDefaultSettings().Settings;
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

		void AllAssetsForeach(System.Action<SwfAsset> act) {
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
						AssetDatabase.GetAssetPath(unapplied[0]))
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

		void DrawGUIDefaultSettings() {
			SwfEditorUtils.DoWithEnabledGUI(false, () => {
				EditorGUILayout.ObjectField(
					"Default Settings",
					GetDefaultSettings(),
					typeof(SwfSettings),
					false);
			});
		}

		void DrawGUIClips() {
			var asset = _assets.Count == 1 ? _assets.First() : null;
			if ( asset && asset.Clips != null && asset.Clips.Count > 0 ) {
				_clipsFoldout = EditorGUILayout.Foldout(_clipsFoldout, "Clips");
				if ( _clipsFoldout ) {
					SwfEditorUtils.DoWithEnabledGUI(false, () => {
						var clip_count = Mathf.Min(asset.Data.Symbols.Count, asset.Clips.Count);
						for ( var i = 0; i < clip_count; ++i ) {
							EditorGUILayout.ObjectField(
								asset.Data.Symbols[i].Name,
								asset.Clips[i],
								typeof(SwfClipAsset),
								false);
						}
					});
					DrawGUIClipsControls();
				}
			}
		}

		void DrawGUIClipsControls() {
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			{
				if ( GUILayout.Button("Reimport all") ) {
					RevertAllOverriddenSettings();
					ApplyAllOverriddenSettings();
				}
			}
			GUILayout.EndHorizontal();
		}

		void DrawGUISettings() {
			_settingsFoldout = EditorGUILayout.Foldout(_settingsFoldout, "Settings");
			if ( _settingsFoldout ) {
				var it = SwfEditorUtils.GetPropertyByName(serializedObject, "Overridden");
				while ( it.Next(true) ) {
					EditorGUILayout.PropertyField(it, true);
				}
				DrawGUISettingsControls();
			}
		}

		void DrawGUISettingsControls() {
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			{
				var default_settings = GetDefaultSettings().Settings;
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
			_assets          = targets.OfType<SwfAsset>().ToList();
			_clipsFoldout    = false;
			_settingsFoldout = true;
		}

		void OnDisable() {
			ShowUnappliedDialog();
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();
			DrawDefaultInspector();
			DrawGUIDefaultSettings();
			DrawGUIClips();
			DrawGUISettings();
			if ( GUI.changed ) {
				serializedObject.ApplyModifiedProperties();
			}
		}
	}
}