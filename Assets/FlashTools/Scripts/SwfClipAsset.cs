using UnityEngine;
using FlashTools.Internal;
using System.Collections.Generic;

namespace FlashTools {
	public class SwfClipAsset : ScriptableObject {
		[System.Serializable]
		public class Frame {
			public Mesh        Mesh      = null;
			public Material[]  Materials = new Material[0];
		}
		[System.Serializable]
		public class Sequence {
			public string      Name      = string.Empty;
			public List<Frame> Frames    = new List<Frame>();
		}

		[SwfReadOnly]
		public Texture2D       Atlas;
		[SwfReadOnly]
		public float           FrameRate;
		[HideInInspector]
		public List<Sequence>  Sequences;

		#if UNITY_EDITOR
		void Reset() {
			Atlas     = null;
			FrameRate = 1.0f;
			Sequences = new List<Sequence>();
		}
		#endif
	}
}