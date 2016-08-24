using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

namespace FlashTools.Internal.SwfEditorTools {

	//
	// SwfSortingLayerDrawer
	//

	[CustomPropertyDrawer(typeof(SwfSortingLayerAttribute))]
	public class SwfSortingLayerDrawer : PropertyDrawer {

		const string DefaultLayerName = "Default";

		static List<string> GetAllSortingLayers() {
			var result = new List<string>();
			var tag_assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
			if ( tag_assets.Length > 0 ) {
				var tag_manager = new SerializedObject(tag_assets[0]);
				var layers = tag_manager.FindProperty("m_SortingLayers");
				if ( layers != null && layers.isArray ) {
					for ( var i = 0; i < layers.arraySize; ++i ) {
						var layer_prop = layers.GetArrayElementAtIndex(i);
						var layer_prop_name = layer_prop != null
							? layer_prop.FindPropertyRelative("name")
							: null;
						var layer_name = layer_prop_name != null && layer_prop_name.propertyType == SerializedPropertyType.String
							? layer_prop_name.stringValue
							: string.Empty;
						if ( !string.IsNullOrEmpty(layer_name) ) {
							result.Add(layer_name);
						}
					}
				}
			}
			if ( !result.Contains(DefaultLayerName) ) {
				result.Add(DefaultLayerName);
			}
			return result;
		}

		static void ValidateProperty(SerializedProperty property) {
			if ( property.propertyType == SerializedPropertyType.String ) {
				var all_sorting_layers = GetAllSortingLayers();
				if ( !all_sorting_layers.Contains(property.stringValue) ) {
					property.stringValue = DefaultLayerName;
					property.serializedObject.ApplyModifiedProperties();
				}
			}
		}

		public override void OnGUI(
			Rect position, SerializedProperty property, GUIContent label)
		{
			var all_sorting_layers = GetAllSortingLayers();
			if ( property.propertyType == SerializedPropertyType.String ) {
				ValidateProperty(property);
				var old_sorting_layer   = property.stringValue;
				var sorting_layer_index = EditorGUI.Popup(
					position,
					label,
					all_sorting_layers.FindIndex(p => p == property.stringValue),
					all_sorting_layers.Select(p => new GUIContent(p)).ToArray());
				property.stringValue = all_sorting_layers[sorting_layer_index];
				if ( old_sorting_layer != property.stringValue ) {
					property.serializedObject.ApplyModifiedProperties();
				}
			} else {
				EditorGUI.LabelField(position, label.text, "Use SwfSortingLayer with string attribute.");
			}
		}
	}

	//
	// SwfPowerOfTwoIfDrawer
	//

	[CustomPropertyDrawer(typeof(SwfPowerOfTwoIfAttribute))]
	public class SwfPowerOfTwoIfDrawer : PropertyDrawer {

		static SerializedProperty FindNextBoolProperty(SerializedProperty property, string next_prop) {
			var prop = property.Copy();
			while ( prop.NextVisible(false) ) {
				if ( prop.name == next_prop && prop.propertyType == SerializedPropertyType.Boolean ) {
					return prop;
				}
			}
			return null;
		}

		static int GetPowerOfTwo(int value) {
			return Mathf.RoundToInt(Mathf.Pow(2, value));
		}

		int[] GenPowerOfTwoValues(int min_pow2, int max_pow2) {
			var values = new List<int>();
			while ( min_pow2 <= max_pow2 ) {
				values.Add(GetPowerOfTwo(min_pow2));
				++min_pow2;
			}
			return values.ToArray();
		}

		static void ValidateProperty(SerializedProperty property, bool need_pow2, int min_pow2, int max_pow2) {
			if ( property.propertyType == SerializedPropertyType.Integer ) {
				var old_value = property.intValue;
				if ( need_pow2 && !Mathf.IsPowerOfTwo(property.intValue) ) {
					property.intValue = Mathf.ClosestPowerOfTwo(property.intValue);
				}
				property.intValue = Mathf.Clamp(
					property.intValue,
					GetPowerOfTwo(min_pow2),
					GetPowerOfTwo(max_pow2));
				if ( old_value != property.intValue ) {
					property.serializedObject.ApplyModifiedProperties();
				}
			}
		}

		public override void OnGUI(
			Rect position, SerializedProperty property, GUIContent label)
		{
			if ( property.propertyType == SerializedPropertyType.Integer ) {
				var attr      = attribute as SwfPowerOfTwoIfAttribute;
				var bool_prop = FindNextBoolProperty(property, attr.BoolProp);
				var need_pow2 = (bool_prop != null && bool_prop.boolValue);
				ValidateProperty(property, need_pow2, attr.MinPow2, attr.MaxPow2);
				if ( need_pow2 ) {
					var values = GenPowerOfTwoValues(attr.MinPow2, attr.MaxPow2);
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