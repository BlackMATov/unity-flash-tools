using UnityEngine;
using UnityEditor;

using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

using Ionic.Zlib;

using FTRuntime;
using FTRuntime.Internal;

namespace FTEditor {
	public static class SwfEditorUtils {

		// ---------------------------------------------------------------------
		//
		// Inspector
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
			EditorGUI.BeginDisabledGroup(!enabled);
			try {
				act();
			} finally {
				EditorGUI.EndDisabledGroup();
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

		// ---------------------------------------------------------------------
		//
		// Assets
		//
		// ---------------------------------------------------------------------

		public static SwfSettings GetSettingsHolder() {
			var holder = LoadFirstAssetByFilter<SwfSettings>("t:SwfSettings");
			if ( !holder ) {
				throw new UnityException(
					"SwfEditorUtils. SwfSettings asset not found");
			}
			return holder;
		}

		public static T LoadOrCreateAsset<T>(string asset_path, System.Action<T, bool> act) where T : ScriptableObject {
			var asset = AssetDatabase.LoadAssetAtPath<T>(asset_path);
			if ( asset ) {
				act(asset, false);
				EditorUtility.SetDirty(asset);
			} else {
				asset = ScriptableObject.CreateInstance<T>();
				act(asset, true);
				AssetDatabase.CreateAsset(asset, asset_path);
			}
			AssetDatabase.ImportAsset(asset_path);
			return asset;
		}

		public static T LoadFirstAssetByFilter<T>(string filter) where T : UnityEngine.Object {
			var guids = AssetDatabase.FindAssets(filter);
			foreach ( var guid in guids ) {
				var path  = AssetDatabase.GUIDToAssetPath(guid);
				var asset = AssetDatabase.LoadAssetAtPath<T>(path);
				if ( asset ) {
					return asset;
				}
			}
			return null;
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
		// Menu
		//
		// ---------------------------------------------------------------------

		[MenuItem("Tools/FlashTools/Open settings...")]
		static void Tools_FlashTools_OpenSettings() {
			var settings_holder = SwfEditorUtils.GetSettingsHolder();
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

		[MenuItem("Tools/FlashTools/Pregenerate all materials")]
		static void PregenerateAllMaterials() {
			var blend_modes = System.Enum.GetValues(typeof(SwfBlendModeData.Types));
			foreach ( SwfBlendModeData.Types blend_mode in blend_modes ) {
				SwfMaterialCache.GetSimpleMaterial(blend_mode);
				for ( var i = 0; i < 10; ++i ) {
					SwfMaterialCache.GetMaskedMaterial(blend_mode, i);
				}
			}
			SwfMaterialCache.GetIncrMaskMaterial();
			SwfMaterialCache.GetDecrMaskMaterial();
		}

		static string[] GetAllSwfFilePaths() {
			return AssetDatabase.GetAllAssetPaths()
				.Where(p => Path.GetExtension(p).ToLower().Equals(".swf"))
				.ToArray();
		}
	}
}