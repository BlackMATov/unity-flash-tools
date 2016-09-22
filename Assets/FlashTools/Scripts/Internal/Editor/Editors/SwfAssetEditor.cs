﻿using UnityEngine;
using UnityEditor;

using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace FlashTools.Internal {
	[CustomEditor(typeof(SwfAsset)), CanEditMultipleObjects]
	public class SwfAssetEditor : Editor {
		List<SwfAsset> _assets = new List<SwfAsset>();

		//
		//
		//

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
			asset.Settings = asset.Overridden;
			ReconvertAsset(asset);
		}

		static void ReconvertAsset(SwfAsset asset) {
			asset.Converting = new SwfAsset.ConvertingState();
			AssetDatabase.ImportAsset(
				AssetDatabase.GetAssetPath(asset));
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

		void ReconvertAllAsset() {
			AllAssetsForeach(p => ReconvertAsset(p));
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

		void DrawGUISettingsControls() {
			var prop = SwfEditorUtils.GetPropertyByName(serializedObject, "Overridden");
			if ( prop.isExpanded ) {
				GUILayout.BeginHorizontal();
				{
					if ( GUILayout.Button("Reconvert") ) {
						ReconvertAllAsset();
					}
					GUILayout.FlexibleSpace();
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
		}

		// ---------------------------------------------------------------------
		//
		// Messages
		//
		// ---------------------------------------------------------------------

		void OnEnable() {
			_assets = targets.OfType<SwfAsset>().ToList();
		}

		void OnDisable() {
			ShowUnappliedDialog();
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();
			DrawDefaultInspector();
			DrawGUISettingsControls();
			if ( GUI.changed ) {
				serializedObject.ApplyModifiedProperties();
			}
		}
	}
}