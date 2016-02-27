using UnityEngine;
using System.Collections.Generic;

namespace FlashTools {
	public enum FlashAnimBlendMode {
		Normal,
		Layer,
		Multiply,
		Screen,
		Overlay,
		Hardlight,
		Lighten,
		Darken,
		Difference,
		Add,
		Subtract,
		Invert,
		Alpha,
		Erase
	}

	public enum FlashAnimLayerType {
		Normal,
		Guide,
		Guided,
		Mask,
		Masked,
		Folder
	}

	public enum FlashAnimLoopingType {
		Loop,
		PlayOnce,
		SingleFrame
	}

	public enum FlashAnimInstType {
		Bitmap,
		Symbol
	}

	public enum FlashAnimInstSymbolType {
		Graphic,
		MovieClip
	}

	[System.Serializable]
	public class FlashAnimBitmapData {
		public string  Id          = string.Empty;
		public Vector2 RealSize    = Vector2.zero;
		public Rect    SourceRect  = new Rect();
		public string  ImageSource = string.Empty;
		public void CopyDataFrom(FlashAnimBitmapData other) {
			RealSize    = other.RealSize;
			SourceRect  = other.SourceRect;
			ImageSource = other.ImageSource;
		}
	}

	[System.Serializable]
	public class FlashAnimInstData {
		public FlashAnimInstType       Type              = FlashAnimInstType.Bitmap;
		public FlashAnimInstSymbolType SymbolType        = FlashAnimInstSymbolType.Graphic;
		public FlashAnimBlendMode      BlendMode         = FlashAnimBlendMode.Normal;
		public string                  Asset             = string.Empty;
		public bool                    Visible           = true;
		public FlashAnimLoopingType    LoopingType       = FlashAnimLoopingType.SingleFrame;
		public int                     LoopingFirstFrame = 0;
		// TODO: color_mode, filters
	}

	[System.Serializable]
	public class FlashAnimElemData {
		public string            Id       = string.Empty;
		public Matrix4x4         Matrix   = Matrix4x4.identity;
		public FlashAnimInstData Instance = null;
	}

	[System.Serializable]
	public class FlashAnimFrameData {
		public string                  Id    = string.Empty;
		public List<FlashAnimElemData> Elems = new List<FlashAnimElemData>();
	}

	[System.Serializable]
	public class FlashAnimLayerData {
		public string                   Id        = string.Empty;
		public FlashAnimLayerType       LayerType = FlashAnimLayerType.Normal;
		public List<FlashAnimFrameData> Frames    = new List<FlashAnimFrameData>();
	}

	[System.Serializable]
	public class FlashAnimSymbolData {
		public string                   Id     = string.Empty;
		public List<FlashAnimLayerData> Layers = new List<FlashAnimLayerData>();
	}

	[System.Serializable]
	public class FlashAnimLibraryData {
		public List<FlashAnimBitmapData> Bitmaps = new List<FlashAnimBitmapData>();
		public List<FlashAnimSymbolData> Symbols = new List<FlashAnimSymbolData>();
	}

	[System.Serializable]
	public class FlashAnimData {
		public FlashAnimSymbolData  Stage   = new FlashAnimSymbolData();
		public FlashAnimLibraryData Library = new FlashAnimLibraryData();
		public List<string>         Strings = new List<string>();
	}

	public class FlashAnimAsset : ScriptableObject {
		[HideInInspector]
		public FlashAnimData Data          = new FlashAnimData();
		public Texture2D     Atlas         = null;
		public int           MaxAtlasSize  = 1024;
		public int           AtlasPadding  = 1;
		public int           PixelsPerUnit = 100;
	}
}
