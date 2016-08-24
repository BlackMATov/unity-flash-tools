using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

namespace FlashTools.Internal.SwfEditorTools {
	[CustomPropertyDrawer(typeof(SwfSortingLayerAttribute))]
	public class SwfSortingLayerDrawer : PropertyDrawer {

		List<GUIContent> GetAllSortingLayers() {
			var result = new List<GUIContent>();
			var tag_manager_so = new SerializedObject(
				AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
			var layers = tag_manager_so.FindProperty("m_SortingLayers");
			if ( layers != null && layers.isArray ) {
				for ( var i = 0; i < layers.arraySize; ++i ) {
					var layer_prop = layers.GetArrayElementAtIndex(i);
					var layer_name_prop = layer_prop.FindPropertyRelative("name");
					if ( !string.IsNullOrEmpty(layer_name_prop.stringValue) ) {
						result.Add(new GUIContent(layer_name_prop.stringValue));
					}
				}
			}
			return result;
		}

		public override void OnGUI(
			Rect position, SerializedProperty property, GUIContent label)
		{
			var all_sorting_layers = GetAllSortingLayers();
			if ( property.propertyType == SerializedPropertyType.String ) {
				var new_sorting_layer = EditorGUI.Popup(
					position,
					label,
					all_sorting_layers.FindIndex(p => p.text == property.stringValue),
					all_sorting_layers.ToArray());
				property.stringValue = all_sorting_layers[new_sorting_layer].text;
			} else {
				EditorGUI.LabelField(position, label.text, "Use SwfSortingLayer with string attribute.");
			}
		}
	}
}