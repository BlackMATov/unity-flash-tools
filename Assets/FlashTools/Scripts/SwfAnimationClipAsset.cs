using UnityEngine;
using FlashTools.Internal;
using System.Collections.Generic;

namespace FlashTools {
	public class SwfAnimationClipAsset : ScriptableObject {
		[System.Serializable]
		public class Frame {
			public Mesh          Mesh      = new Mesh();
			public Material[]    Materials = new Material[0];
		}
		[System.Serializable]
		public class Sequence {
			public string        Name      = string.Empty;
			public List<Frame>   Frames    = new List<Frame>();
		}
		public Texture2D         Atlas;
		public float             FrameRate;
		public List<Sequence>    Sequences;

		#if UNITY_EDITOR
		void Reset() {
			Atlas     = null;
			FrameRate = 1.0f;
			Sequences = new List<Sequence>();
		}
		#endif
	}
}