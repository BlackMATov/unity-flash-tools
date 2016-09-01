﻿using UnityEngine;
using FlashTools.Internal;
using System.Collections.Generic;

namespace FlashTools {
	[System.Serializable]
	public struct SwfAnimationColorTransform {
		public Vector4 Mul;
		public Vector4 Add;

		public static SwfAnimationColorTransform identity {
			get {
				return new SwfAnimationColorTransform{
					Mul = Vector4.one,
					Add = Vector4.zero};
			}
		}

		public static SwfAnimationColorTransform operator*(
			SwfAnimationColorTransform a, SwfAnimationColorTransform b)
		{
			return new SwfAnimationColorTransform{
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

	public enum SwfAnimationInstanceType {
		Mask,
		Group,
		Masked,
		MaskReset
	}

	[System.Serializable]
	public class SwfAnimationInstanceData {
		public SwfAnimationInstanceType       Type           = SwfAnimationInstanceType.Group;
		public ushort                         ClipDepth      = 0;
		public ushort                         Bitmap         = 0;
		public Matrix4x4                      Matrix         = Matrix4x4.identity;
		public SwfAnimationColorTransform     ColorTransform = SwfAnimationColorTransform.identity;
	}

	[System.Serializable]
	public class SwfAnimationFrameData {
		public string                         Name           = string.Empty;
		public List<SwfAnimationInstanceData> Instances      = new List<SwfAnimationInstanceData>();
	}

	[System.Serializable]
	public class SwfAnimationSymbolData {
		public int                            Id             = 0;
		public string                         Name           = string.Empty;
		public List<SwfAnimationFrameData>    Frames         = new List<SwfAnimationFrameData>();
	}

	[System.Serializable]
	public class SwfAnimationBitmapData {
		public int                            Id             = 0;
		public Vector2                        RealSize       = Vector2.zero;
		public Rect                           SourceRect     = new Rect();
	}

	[System.Serializable]
	public class SwfAnimationData {
		public float                          FrameRate      = 0.0f;
		public List<SwfAnimationSymbolData>   Symbols        = new List<SwfAnimationSymbolData>();
		public List<SwfAnimationBitmapData>   Bitmaps        = new List<SwfAnimationBitmapData>();
	}

	public class SwfAsset : ScriptableObject {
		public SwfAnimationData   Data;
		public Texture2D          Atlas;
		public List<SwfClipAsset> Clips;
		public SwfSettings        Settings;
		public SwfSettings        Overridden;

	#if UNITY_EDITOR
		void Reset() {
			Data       = new SwfAnimationData();
			Atlas      = null;
			Clips      = new List<SwfClipAsset>();
			Settings   = SwfConverterSettings.GetDefaultSettings();
			Overridden = SwfConverterSettings.GetDefaultSettings();
		}
	#endif
	}
}