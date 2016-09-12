using UnityEngine;
using UnityEditor;

using NUnit.Framework;

using System.IO;
using System.Collections.Generic;

namespace FlashTools.Internal {
	public static class SwfEditorUtils {
		public static void DoWithMixedValue(bool mixed, System.Action act) {
			var last_show_mixed_value = EditorGUI.showMixedValue;
			EditorGUI.showMixedValue = mixed;
			try {
				act();
			} finally {
				EditorGUI.showMixedValue = last_show_mixed_value;
			}
		}

		public static void DoWithEnabledGUI(bool enabled, System.Action act) {
			var last_gui_enabled = GUI.enabled;
			GUI.enabled = enabled;
			try {
				act();
			} finally {
				GUI.enabled = last_gui_enabled;
			}
		}

		public static SerializedProperty GetPropertyByName(SerializedObject obj, string name) {
			var prop = obj.FindProperty(name);
			if ( prop == null ) {
				throw new UnityException(string.Format(
					"SwfEditorUtils. Not found property: {0}",
					name));
			}
			return prop;
		}

		public static void DeleteAssetWithDepends(SwfAsset asset) {
			if ( asset ) {
				if ( asset.Atlas ) {
					AssetDatabase.DeleteAsset(
						AssetDatabase.GetAssetPath(asset.Atlas));
				}
				AssetDatabase.DeleteAsset(
					AssetDatabase.GetAssetPath(asset));
			}
		}

		public static void RemoveAllSubAssets(string asset_path) {
			var assets = AssetDatabase.LoadAllAssetsAtPath(asset_path);
			foreach ( var asset in assets ) {
				if ( !AssetDatabase.IsMainAsset(asset) ) {
					GameObject.DestroyImmediate(asset, true);
				}
			}
		}

		public static T LoadOrCreateAsset<T>(string asset_path) where T : ScriptableObject {
			var asset = AssetDatabase.LoadAssetAtPath<T>(asset_path);
			if ( !asset ) {
				asset = ScriptableObject.CreateInstance<T>();
				AssetDatabase.CreateAsset(asset, asset_path);
			}
			return asset;
		}

		public static string GetAtlasPathFromAsset(SwfAsset asset) {
			var asset_path = AssetDatabase.GetAssetPath(asset);
			return Path.ChangeExtension(asset_path, "._Atlas_.png");
		}

		//
		// SwfUtils tests
		//

		[Test]
		public static void PackUVTests() {
			float v0 = 0.99f, v1 = 0.11f;
			Assert.AreEqual(
				v0,
				SwfUtils.UnpackUV(SwfUtils.PackUV(v0, v1)).x,
				0.0001f);
			Assert.AreEqual(
				v1,
				SwfUtils.UnpackUV(SwfUtils.PackUV(v0, v1)).y,
				0.0001f);

			float v2 = 0.09f, v3 = 0.01f;
			Assert.AreEqual(
				v2,
				SwfUtils.UnpackUV(SwfUtils.PackUV(v2, v3)).x,
				0.0001f);
			Assert.AreEqual(
				v3,
				SwfUtils.UnpackUV(SwfUtils.PackUV(v2, v3)).y,
				0.0001f);

			float v4 = 1.0f, v5 = 0.0f;
			Assert.AreEqual(
				v4,
				SwfUtils.UnpackUV(SwfUtils.PackUV(v4, v5)).x,
				0.0001f);
			Assert.AreEqual(
				v5,
				SwfUtils.UnpackUV(SwfUtils.PackUV(v4, v5)).y,
				0.0001f);
		}

		[Test]
		public static void PackUShortsToUIntTests() {
			ushort v0 = 11, v1 = 99;
			ushort o0, o1;
			SwfUtils.UnpackUShortsFromUInt(
				SwfUtils.PackUShortsToUInt(v0, v1), out o0, out o1);
			Assert.AreEqual(v0, o0);
			Assert.AreEqual(v1, o1);

			ushort v2 = 16789, v3 = 31234;
			ushort o2, o3;
			SwfUtils.UnpackUShortsFromUInt(
				SwfUtils.PackUShortsToUInt(v2, v3), out o2, out o3);
			Assert.AreEqual(v2, o2);
			Assert.AreEqual(v3, o3);
		}

		[Test]
		public static void PackCoordsToUIntTests() {
			var v0 = new Vector2(1.14f, 2.23f);
			Assert.AreEqual(
				v0.x,
				SwfUtils.UnpackCoordsFromUInt(SwfUtils.PackCoordsToUInt(v0)).x,
				0.05f);
			Assert.AreEqual(
				v0.y,
				SwfUtils.UnpackCoordsFromUInt(SwfUtils.PackCoordsToUInt(v0)).y,
				0.05f);

			var v1 = new Vector2(1234.14f, 3200.23f);
			Assert.AreEqual(
				v1.x,
				SwfUtils.UnpackCoordsFromUInt(SwfUtils.PackCoordsToUInt(v1)).x,
				0.05f);
			Assert.AreEqual(
				v1.y,
				SwfUtils.UnpackCoordsFromUInt(SwfUtils.PackCoordsToUInt(v1)).y,
				0.05f);
		}
	}
}