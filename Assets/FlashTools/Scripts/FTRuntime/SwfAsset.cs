﻿using UnityEngine;
using FTRuntime.Internal;
using System.Collections.Generic;

namespace FTRuntime {
	public class SwfAsset : ScriptableObject {
		[HideInInspector]
		public byte[]          Data;
		[SwfReadOnly]
		public Texture2D       Atlas;
		[HideInInspector]
		public SwfSettingsData Settings;
		[SwfDisplayName("Settings")]
		public SwfSettingsData Overridden;

		void Reset() {
			Data       = new byte[0];
			Atlas      = null;
			Settings   = SwfSettingsData.identity;
			Overridden = SwfSettingsData.identity;
		}
	}
}