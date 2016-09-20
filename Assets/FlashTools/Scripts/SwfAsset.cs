using UnityEngine;
using FlashTools.Internal;
using System.Collections.Generic;

namespace FlashTools {
	[System.Serializable]
	public struct SwfMatrixData {
		public Vector2 Sc;
		public Vector2 Sk;
		public Vector2 Tr;

		public static SwfMatrixData identity {
			get {
				return new SwfMatrixData{
					Sc = Vector2.one,
					Sk = Vector2.zero,
					Tr = Vector2.zero};
			}
		}

		public Matrix4x4 ToUMatrix() {
			var mat = Matrix4x4.identity;
			mat.m00 = Sc.x;
			mat.m11 = Sc.y;
			mat.m10 = Sk.x;
			mat.m01 = Sk.y;
			mat.m03 = Tr.x;
			mat.m13 = Tr.y;
			return mat;
		}

		public static SwfMatrixData FromUMatrix(Matrix4x4 mat) {
			return new SwfMatrixData{
				Sc = new Vector2(mat.m00, mat.m11),
				Sk = new Vector2(mat.m10, mat.m01),
				Tr = new Vector2(mat.m03, mat.m13)};
		}
	}

	[System.Serializable]
	public struct SwfColorTransData {
		public Vector4 Mul;
		public Vector4 Add;

		public Color ApplyToColor(Color color) {
			return new Color(
				Mathf.Clamp01(color.r * Mul.x + Add.x),
				Mathf.Clamp01(color.g * Mul.y + Add.y),
				Mathf.Clamp01(color.b * Mul.z + Add.z),
				Mathf.Clamp01(color.a * Mul.w + Add.w));
		}

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
		public SwfMatrixData         Matrix     = SwfMatrixData.identity;
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
		public ushort                Id         = 0;
		public byte[]                ARGB32     = new byte[0];
		public ushort                Redirect   = 0;
		public int                   RealWidth  = 0;
		public int                   RealHeight = 0;
		public Rect                  SourceRect = new Rect();
	}

	[System.Serializable]
	public class SwfAssetData {
		public float                 FrameRate  = 0.0f;
		public List<SwfSymbolData>   Symbols    = new List<SwfSymbolData>();
		public List<SwfBitmapData>   Bitmaps    = new List<SwfBitmapData>();
	}

	public class SwfAsset : ScriptableObject {
		[System.Serializable]
		public struct ConvertingState {
			public int Stage;
		}
		[HideInInspector]
		public SwfAssetData       Data;
		[SwfReadOnly]
		public Texture2D          Atlas;
		[HideInInspector]
		public List<SwfClipAsset> Clips;
		[HideInInspector]
		public SwfSettingsData    Settings;
		[SwfDisplayName("Settings")]
		public SwfSettingsData    Overridden;
		[HideInInspector]
		public ConvertingState    Converting;

	#if UNITY_EDITOR
		void Reset() {
			Data       = new SwfAssetData();
			Atlas      = null;
			Clips      = new List<SwfClipAsset>();
			Settings   = SwfSettings.GetDefault();
			Overridden = SwfSettings.GetDefault();
			Converting = new ConvertingState();
		}
	#endif
	}
}