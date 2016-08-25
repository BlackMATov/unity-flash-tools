using UnityEngine;

namespace FlashTools.Internal {
	[System.Serializable]
	public struct SwfSettings {
		public enum AtlasFilter {
			Point,
			Bilinear,
			Trilinear
		}

		public enum AtlasFormat {
			AutomaticCompressed,
			Automatic16bit,
			AutomaticTruecolor,
			AutomaticCrunched
		}

		[SwfPowerOfTwoIfAttribute(5, 13, "AtlasPowerOfTwo")]
		public int         MaxAtlasSize;
		[SwfIntRange(0, int.MaxValue)]
		public int         AtlasPadding;
		[SwfFloatRange(float.Epsilon, float.MaxValue)]
		public float       PixelsPerUnit;
		public bool        GenerateMipMaps;
		public bool        AtlasPowerOfTwo;
		public bool        AtlasForceSquare;
		public AtlasFilter AtlasTextureFilter;
		public AtlasFormat AtlasTextureFormat;

		public static SwfSettings identity {
			get {
				return new SwfSettings{
					MaxAtlasSize       = 1024,
					AtlasPadding       = 1,
					PixelsPerUnit      = 100.0f,
					GenerateMipMaps    = true,
					AtlasPowerOfTwo    = true,
					AtlasForceSquare   = false,
					AtlasTextureFilter = AtlasFilter.Bilinear,
					AtlasTextureFormat = AtlasFormat.AutomaticTruecolor};
			}
		}

		public bool CheckEquals(SwfSettings other) {
			return
				MaxAtlasSize       == other.MaxAtlasSize &&
				AtlasPadding       == other.AtlasPadding &&
				Mathf.Approximately(PixelsPerUnit, other.PixelsPerUnit) &&
				GenerateMipMaps    == other.GenerateMipMaps &&
				AtlasPowerOfTwo    == other.AtlasPowerOfTwo &&
				AtlasForceSquare   == other.AtlasForceSquare &&
				AtlasTextureFilter == other.AtlasTextureFilter &&
				AtlasTextureFormat == other.AtlasTextureFormat;
		}
	}
}