using UnityEngine;
using FlashTools.Internal;
using System.Collections.Generic;

namespace FlashTools {
	[System.Serializable]
	public struct SwfColorTransData {
		public Vector4 Mul;
		public Vector4 Add;

		public static SwfColorTransData identity {
			get {
				return new SwfColorTransData{
					Mul = Vector4.one,
					Add = Vector4.zero};
			}
		}

		public static SwfColorTransData operator*(
			SwfColorTransData a, SwfColorTransData b)
		{
			return new SwfColorTransData{
				Mul = new Vector4(
					b.Mul.x * a.Mul.x,
					b.Mul.y * a.Mul.y,
					b.Mul.z * a.Mul.z,
					b.Mul.w * a.Mul.w),
				Add = new Vector4(
					b.Add.x * a.Mul.x + a.Add.x,
					b.Add.y * a.Mul.y + a.Add.y,
					b.Add.z * a.Mul.z + a.Add.z,
					b.Add.w * a.Mul.w + a.Add.w)};
		}
	}

	[System.Serializable]
	public class SwfInstanceData {
		public enum Types {
			Mask,
			Group,
			Masked,
			MaskReset
		}
		public Types                 Type       = Types.Group;
		public ushort                ClipDepth  = 0;
		public ushort                Bitmap     = 0;
		public Matrix4x4             Matrix     = Matrix4x4.identity;
		public SwfColorTransData     ColorTrans = SwfColorTransData.identity;
	}

	[System.Serializable]
	public class SwfFrameData {
		public string                Name       = string.Empty;
		public List<SwfInstanceData> Instances  = new List<SwfInstanceData>();
	}

	[System.Serializable]
	public class SwfSymbolData {
		public string                Name       = string.Empty;
		public List<SwfFrameData>    Frames     = new List<SwfFrameData>();
	}

	[System.Serializable]
	public class SwfBitmapData {
		public int                   Id         = 0;
		public Vector2               RealSize   = Vector2.zero;
		public Rect                  SourceRect = new Rect();
	}

	[System.Serializable]
	public class SwfAssetData {
		public float                 FrameRate  = 0.0f;
		public List<SwfSymbolData>   Symbols    = new List<SwfSymbolData>();
		public List<SwfBitmapData>   Bitmaps    = new List<SwfBitmapData>();
	}

	public class SwfAsset : ScriptableObject {
		public SwfAssetData       Data;
		public Texture2D          Atlas;
		public List<SwfClipAsset> Clips;
		public SwfSettingsData    Settings;
		public SwfSettingsData    Overridden;

	#if UNITY_EDITOR
		void Reset() {
			Data       = new SwfAssetData();
			Atlas      = null;
			Clips      = new List<SwfClipAsset>();
			Settings   = SwfSettings.GetDefault();
			Overridden = SwfSettings.GetDefault();
		}
	#endif
	}
}