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

	public enum FlashAnimLoopingMode {
		Loop,
		PlayOnce,
		SingleFrame
	}

	public enum FlashAnimLayerType {
		Normal,
		Guide,
		Guided,
		Mask,
		Masked,
		Folder
	}

	public enum FlashAnimInstType {
		Bitmap,
		Symbol
	}

	[System.Serializable]
	public struct FlashAnimColorTransform {
		public Vector4 Mul;
		public Vector4 Add;

		public FlashAnimColorTransform(Vector4 Mul, Vector4 Add) {
			this.Mul = Mul;
			this.Add = Add;
		}

		public static FlashAnimColorTransform identity {
			get {
				return new FlashAnimColorTransform(
					new Vector4(1,1,1,1),
					new Vector4(0,0,0,0));
			}
		}

		public static FlashAnimColorTransform operator*(
			FlashAnimColorTransform a, FlashAnimColorTransform b)
		{
			var res = new FlashAnimColorTransform();
			res.Mul.x = b.Mul.x * a.Mul.x;
			res.Mul.y = b.Mul.y * a.Mul.y;
			res.Mul.z = b.Mul.z * a.Mul.z;
			res.Mul.w = b.Mul.w * a.Mul.w;
			res.Add.x = b.Add.x * a.Mul.x + a.Add.x;
			res.Add.y = b.Add.y * a.Mul.y + a.Add.y;
			res.Add.z = b.Add.z * a.Mul.z + a.Add.z;
			res.Add.w = b.Add.w * a.Mul.w + a.Add.w;
			return res;
		}
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
		public FlashAnimInstType       Type           = FlashAnimInstType.Bitmap;
		public FlashAnimBlendMode      BlendMode      = FlashAnimBlendMode.Normal;
		public string                  Asset          = string.Empty;
		public bool                    Visible        = true;
		public int                     FirstFrame     = 0;
		public FlashAnimLoopingMode    LoopingMode    = FlashAnimLoopingMode.SingleFrame;
		public FlashAnimColorTransform ColorTransform = FlashAnimColorTransform.identity;
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
		public FlashAnimSymbolData  Stage     = new FlashAnimSymbolData();
		public FlashAnimLibraryData Library   = new FlashAnimLibraryData();
		public List<string>         Strings   = new List<string>();
		public int                  FrameRate = 24;
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
