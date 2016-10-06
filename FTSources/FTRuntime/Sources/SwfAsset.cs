using UnityEngine;
using System.Collections.Generic;

namespace FTRuntime {
	public class SwfAsset : ScriptableObject {
		[System.Serializable]
		public struct ConvertingState {
			public int Stage;
		}
		[HideInInspector]
		public byte[]             Data;
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

		void Reset() {
			Data       = new byte[0];
			Atlas      = null;
			Clips      = new List<SwfClipAsset>();
			Settings   = SwfSettingsData.identity;
			Overridden = SwfSettingsData.identity;
			Converting = new ConvertingState();
		}
	}
}