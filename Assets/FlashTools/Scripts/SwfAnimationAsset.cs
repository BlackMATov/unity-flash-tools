using UnityEngine;
using System.Collections.Generic;

namespace FlashTools {
	[System.Serializable]
	public struct SwfAnimationColorTransform {
		public Vector4 Mul;
		public Vector4 Add;

		public SwfAnimationColorTransform(Vector4 Mul, Vector4 Add) {
			this.Mul = Mul;
			this.Add = Add;
		}

		public static SwfAnimationColorTransform identity {
			get {
				return new SwfAnimationColorTransform(
					new Vector4(1,1,1,1),
					new Vector4(0,0,0,0));
			}
		}

		public static SwfAnimationColorTransform operator*(
			SwfAnimationColorTransform a, SwfAnimationColorTransform b)
		{
			var res = new SwfAnimationColorTransform();
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
	public class SwfAnimationInstanceData {
		public ushort                         Depth          = 0;
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
	public class SwfAnimationBitmapData {
		public int                            Id             = 0;
		public Vector2                        RealSize       = Vector2.zero;
		public Rect                           SourceRect     = new Rect();
	}

	[System.Serializable]
	public class SwfAnimationData {
		public float                          FrameRate      = 0.0f;
		public List<SwfAnimationFrameData>    Frames         = new List<SwfAnimationFrameData>();
		public List<SwfAnimationBitmapData>   Bitmaps        = new List<SwfAnimationBitmapData>();
	}

	public class SwfAnimationAsset : ScriptableObject {
		//[HideInInspector]
		public SwfAnimationData               Data           = new SwfAnimationData();
		public Texture2D                      Atlas          = null;
		public int                            MaxAtlasSize   = 1024;
		public int                            AtlasPadding   = 1;
		public int                            PixelsPerUnit  = 100;
	}
}