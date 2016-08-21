using UnityEngine;

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

namespace FlashTools {
	public class SwfConverterSettings : ScriptableObject {
		[System.Serializable]
		public struct Settings {
			public int                   MaxAtlasSize;
			public int                   AtlasPadding;
			public int                   PixelsPerUnit;
			public bool                  AtlasPowerOfTwo;
			public bool                  GenerateMipMaps;
			public FilterMode            AtlasFilterMode;
			public TextureImporterFormat AtlasImporterFormat;

			public void Reset() {
				MaxAtlasSize        = 1024;
				AtlasPadding        = 1;
				PixelsPerUnit       = 100;
				AtlasPowerOfTwo     = false;
				GenerateMipMaps     = true;
				AtlasFilterMode     = FilterMode.Bilinear;
				AtlasImporterFormat = TextureImporterFormat.AutomaticTruecolor;
			}
		}
		public Settings DefaultSettings;

	#if UNITY_EDITOR
		void Reset() {
			DefaultSettings.Reset();
		}

		public static SwfConverterSettings LoadOrCreate() {
			var settings_path = GetSettingsPath();
			var settings = AssetDatabase.LoadAssetAtPath<SwfConverterSettings>(settings_path);
			if ( !settings ) {
				settings = ScriptableObject.CreateInstance<SwfConverterSettings>();
				CreateAssetDatabaseFolders(Path.GetDirectoryName(settings_path));
				AssetDatabase.CreateAsset(settings, settings_path);
				AssetDatabase.SaveAssets();
			}
			return settings;
		}

		static void CreateAssetDatabaseFolders(string path) {
			if ( !AssetDatabase.IsValidFolder(path) ) {
				var parent = Path.GetDirectoryName(path);
				if ( !string.IsNullOrEmpty(parent) ) {
					CreateAssetDatabaseFolders(parent);
				}
				AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
			}
		}

		static string GetSettingsPath() {
			return "Assets/FlashTools/Resources/SwfConverterSettings.asset";
		}
	#endif
	}
}