using UnityEngine;

namespace FlashTools.Internal {
	public class SwfReadOnlyAttribute : PropertyAttribute {
	}

	public class SwfSortingLayerAttribute : PropertyAttribute {
	}

	public class SwfPowerOfTwoIfAttribute : PropertyAttribute {
		public int    MinPow2;
		public int    MaxPow2;
		public string BoolProp;
		public SwfPowerOfTwoIfAttribute(int min_pow2, int max_pow2, string prop) {
			MinPow2  = min_pow2;
			MaxPow2  = max_pow2;
			BoolProp = prop;
		}
	}
}