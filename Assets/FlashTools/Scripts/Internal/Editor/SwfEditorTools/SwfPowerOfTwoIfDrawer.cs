using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

namespace FlashTools.Internal.SwfEditorTools {
	[CustomPropertyDrawer(typeof(SwfPowerOfTwoIfAttribute))]
	public class SwfPowerOfTwoIfDrawer : PropertyDrawer {

		SerializedProperty FindBoolProperty(SerializedProperty property, string bool_prop) {
			var prop = property.Copy();
			while ( prop.NextVisible(false) ) {
				if ( prop.propertyType == SerializedPropertyType.Boolean && prop.name == bool_prop ) {
					return prop;
				}
			}
			return null;
		}

		void PropertyToPowerOfTwo(SerializedProperty property) {
			if ( property.propertyType == SerializedPropertyType.Integer ) {
				if ( !Mathf.IsPowerOfTwo(property.intValue) ) {
					property.intValue = Mathf.ClosestPowerOfTwo(property.intValue);
					property.serializedObject.ApplyModifiedProperties();
				}
			}
		}

		int[] GenPowerOfTwoValues(int min, int max) {
			var values = new List<int>();
			if ( !Mathf.IsPowerOfTwo(min) ) {
				min = Mathf.NextPowerOfTwo(min);
			}
			while ( min <= max ) {
				values.Add(min);
				min = Mathf.NextPowerOfTwo(min + 1);
			}
			return values.ToArray();
		}

		public override void OnGUI(
			Rect position, SerializedProperty property, GUIContent label)
		{
			if ( property.propertyType == SerializedPropertyType.Integer ) {
				var attr      = attribute as SwfPowerOfTwoIfAttribute;
				var bool_prop = FindBoolProperty(property, attr.BoolProp);
				if ( bool_prop != null && bool_prop.boolValue ) {
					PropertyToPowerOfTwo(property);
					var values = GenPowerOfTwoValues(attr.Min, attr.Max);
					var vnames = values.Select(p => new GUIContent(p.ToString())).ToArray();
					EditorGUI.IntPopup(position, property, vnames, values, label);
				} else {
					EditorGUI.PropertyField(position, property, label);
				}
			} else {
				EditorGUI.LabelField(position, label.text, "Use SwfPowerOfTwoIf with integer attribute.");
			}
		}
	}
}