using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

namespace FlashTools.Internal {
	public class SwfConverterSettings : ScriptableObject {
		public enum SwfAtlasFilter {
			Point,
			Bilinear,
			Trilinear
		}

		public enum SwfAtlasFormat {
			AutomaticCompressed,
			Automatic16bit,
			AutomaticTruecolor,
			AutomaticCrunched
		}

		[System.Serializable]
		public struct Settings {
			[SwfPowerOfTwoIfAttribute("AtlasPowerOfTwo", 32, 8192)]
			public int            MaxAtlasSize;
			public int            AtlasPadding;
			public int            PixelsPerUnit;
			public bool           GenerateMipMaps;
			public bool           AtlasPowerOfTwo;
			public bool           AtlasForceSquare;
			public SwfAtlasFilter AtlasTextureFilter;
			public SwfAtlasFormat AtlasTextureFormat;

			public static Settings identity {
				get {
					return new Settings{
						MaxAtlasSize       = 1024,
						AtlasPadding       = 1,
						PixelsPerUnit      = 100,
						GenerateMipMaps    = true,
						AtlasPowerOfTwo    = true,
						AtlasForceSquare   = false,
						AtlasTextureFilter = SwfAtlasFilter.Bilinear,
						AtlasTextureFormat = SwfAtlasFormat.AutomaticTruecolor};
				}
			}

			public bool CheckEquals(Settings other) {
				return
					MaxAtlasSize       == other.MaxAtlasSize       &&
					AtlasPadding       == other.AtlasPadding       &&
					PixelsPerUnit      == other.PixelsPerUnit      &&
					GenerateMipMaps    == other.GenerateMipMaps    &&
					AtlasPowerOfTwo    == other.AtlasPowerOfTwo    &&
					AtlasForceSquare   == other.AtlasForceSquare   &&
					AtlasTextureFilter == other.AtlasTextureFilter &&
					AtlasTextureFormat == other.AtlasTextureFormat;
			}
		}
		public Settings DefaultSettings;

	#if UNITY_EDITOR
		void Reset() {
			DefaultSettings = Settings.identity;
		}

		public static Settings GetDefaultSettings() {
			var settings_path = DefaultSettingsPath;
			var settings = AssetDatabase.LoadAssetAtPath<SwfConverterSettings>(settings_path);
			if ( !settings ) {
				settings = ScriptableObject.CreateInstance<SwfConverterSettings>();
				CreateAssetDatabaseFolders(Path.GetDirectoryName(settings_path));
				AssetDatabase.CreateAsset(settings, settings_path);
				AssetDatabase.SaveAssets();
			}
			return settings.DefaultSettings;
		}

		static string DefaultSettingsPath {
			get {
				return "Assets/FlashTools/Resources/SwfConverterSettings.asset";
			}
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
	#endif
	}
}