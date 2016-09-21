using UnityEngine;
using UnityEditor;

using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

using Ionic.Zlib;

namespace FlashTools.Internal {
	public static class SwfEditorUtils {

		// ---------------------------------------------------------------------
		//
		// Functions
		//
		// ---------------------------------------------------------------------

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

		public static void DoHorizontalGUI(System.Action act) {
			GUILayout.BeginHorizontal();
			try {
				act();
			} finally {
				GUILayout.EndHorizontal();
			}
		}

		public static void DoRightHorizontalGUI(System.Action act) {
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			try {
				act();
			} finally {
				GUILayout.EndHorizontal();
			}
		}

		public static void DoCenterHorizontalGUI(System.Action act) {
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			try {
				act();
			} finally {
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
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

		public static void RemoveAllSubAssets(string asset_path) {
			var assets = AssetDatabase.LoadAllAssetsAtPath(asset_path);
			foreach ( var asset in assets ) {
				if ( !AssetDatabase.IsMainAsset(asset) ) {
					GameObject.DestroyImmediate(asset, true);
				}
			}
		}

		public static T LoadOrCreateAsset<T>(string asset_path, System.Action<T> act) where T : ScriptableObject {
			var asset = AssetDatabase.LoadAssetAtPath<T>(asset_path);
			if ( asset ) {
				act(asset);
			} else {
				asset = ScriptableObject.CreateInstance<T>();
				act(asset);
				AssetDatabase.CreateAsset(asset, asset_path);
			}
			AssetDatabase.ImportAsset(asset_path);
			return asset;
		}

		public static byte[] CompressAsset<T>(T asset) {
			var bytes  = AssetToBytes(asset);
			var result = ZlibStream.CompressBuffer(bytes);
			return result;
		}

		public static T DecompressAsset<T>(byte[] data) {
			var bytes  = ZlibStream.UncompressBuffer(data);
			var result = BytesToAsset<T>(bytes);
			return result;
		}

		static byte[] AssetToBytes<T>(T asset) {
			var formatter = new BinaryFormatter();
			using ( var stream = new MemoryStream() ) {
				formatter.Serialize(stream, asset);
				return stream.ToArray();
			}
		}

		static T BytesToAsset<T>(byte[] bytes) {
			var formatter = new BinaryFormatter();
			using ( var stream = new MemoryStream(bytes) ) {
				return (T)formatter.Deserialize(stream);
			}
		}

		// ---------------------------------------------------------------------
		//
		// Internal
		//
		// ---------------------------------------------------------------------

		[MenuItem("Tools/FlashTools/Open settings...")]
		static void Tools_FlashTools_OpenSettings() {
			var settings_holder = SwfSettings.GetHolder();
			Selection.objects = new Object[]{settings_holder};
		}

		[MenuItem("Tools/FlashTools/Reimport all swf files")]
		static void Tools_FlashTools_ReimportAllSwfFiles() {
			var swf_paths = GetAllSwfFilePaths();
			var title     = "Reimport";
			var message   = string.Format(
				"Do you really want to reimport all ({0}) swf files?",
				swf_paths.Length);
			if ( EditorUtility.DisplayDialog(title, message, "Ok", "Cancel") ) {
				foreach ( var swf_path in swf_paths ) {
					AssetDatabase.ImportAsset(swf_path);
				}
			}
		}

		static string[] GetAllSwfFilePaths() {
			return AssetDatabase.GetAllAssetPaths()
				.Where(p => Path.GetExtension(p).ToLower().Equals(".swf"))
				.ToArray();
		}
	}
}