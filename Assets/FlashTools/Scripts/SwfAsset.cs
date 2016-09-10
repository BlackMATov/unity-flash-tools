using UnityEngine;
using FlashTools.Internal;
using System.Collections.Generic;

namespace FlashTools {
	[System.Serializable]
	public struct SwfMatrixData {
		public float ScX;
		public float ScY;
		public float SkX;
		public float SkY;
		public float TrX;
		public float TrY;

		public static SwfMatrixData identity {
			get {
				return new SwfMatrixData{
					ScX = 1.0f,
					ScY = 1.0f,
					SkX = 0.0f,
					SkY = 0.0f,
					TrX = 0.0f,
					TrY = 0.0f};
			}
		}

		public static SwfMatrixData FromUnityMatrix(Matrix4x4 mat) {
			return new SwfMatrixData{
				ScX = mat.m00,
				ScY = mat.m11,
				SkX = mat.m10,
				SkY = mat.m01,
				TrX = mat.m03,
				TrY = mat.m13};
		}

		public Matrix4x4 ToUnityMatrix() {
			var mat = Matrix4x4.identity;
			mat.m00 = ScX;
			mat.m11 = ScY;
			mat.m10 = SkX;
			mat.m01 = SkY;
			mat.m03 = TrX;
			mat.m13 = TrY;
			return mat;
		}
	}

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
		[HideInInspector]
		public SwfAssetData       Data;
		[SwfReadOnly]
		public Texture2D          Atlas;
		[HideInInspector]
		public List<SwfClipAsset> Clips;
		[HideInInspector]
		public SwfSettingsData    Settings;
		[HideInInspector]
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