using UnityEngine;

namespace FlashTools.Internal {
	public class SwfPowerOfTwoIfAttribute : PropertyAttribute {
		public string BoolProp;
		public int    Min;
		public int    Max;
		public SwfPowerOfTwoIfAttribute(string bool_prop, int min, int max) {
			BoolProp = bool_prop;
			Min      = min;
			Max      = max;
		}
	}
}