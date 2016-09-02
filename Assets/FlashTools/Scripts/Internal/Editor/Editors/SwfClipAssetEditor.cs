using UnityEngine;
using UnityEditor;

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace FlashTools.Internal {
	[CustomEditor(typeof(SwfClipAsset)), CanEditMultipleObjects]
	public class SwfClipAssetEditor : Editor {
		List<SwfClipAsset> _clips = new List<SwfClipAsset>();

		static string GetClipPath(SwfClipAsset clip) {
			return clip
				? AssetDatabase.GetAssetPath(clip)
				: string.Empty;
		}

		static string GetPrefabPath(SwfClipAsset clip) {
			var clip_path = GetClipPath(clip);
			return string.IsNullOrEmpty(clip_path)
				? string.Empty
				: Path.ChangeExtension(clip_path, ".prefab");
		}

		//
		//
		//

		static GameObject CreateClipGO(SwfClipAsset clip) {
			if ( clip ) {
				var clip_go = new GameObject(clip.name);
				clip_go.AddComponent<MeshFilter>();
				clip_go.AddComponent<MeshRenderer>();
				clip_go.AddComponent<SwfClip>().clip = clip;
				clip_go.AddComponent<SwfClipController>();
				return clip_go;
			}
			return null;
		}

		static void CreateClipPrefab(SwfClipAsset clip) {
			var clip_go = CreateClipGO(clip);
			if ( clip_go ) {
				var prefab_path = GetPrefabPath(clip);
				if ( !string.IsNullOrEmpty(prefab_path) ) {
					var prefab = AssetDatabase.LoadMainAssetAtPath(prefab_path);
					if ( !prefab ) {
						prefab = PrefabUtility.CreateEmptyPrefab(prefab_path);
					}
					PrefabUtility.ReplacePrefab(
						clip_go,
						prefab,
						ReplacePrefabOptions.ConnectToPrefab);
				}
				GameObject.DestroyImmediate(clip_go, true);
			}
		}

		static void CreateClipOnScene(SwfClipAsset clip) {
			var clip_go = CreateClipGO(clip);
			if ( clip_go ) {
				Undo.RegisterCreatedObjectUndo(clip_go, "Instance SwfClip");
			}
		}

		//
		//
		//

		void AllClipsForeach(Action<SwfClipAsset> act) {
			foreach ( var clip in _clips ) {
				act(clip);
			}
		}

		void CreateAllClipsPrefabs() {
			AllClipsForeach(p => CreateClipPrefab(p));
		}

		void CreateAllClipsOnScene() {
			AllClipsForeach(p => CreateClipOnScene(p));
		}

		//
		//
		//

		void DrawGUIProperties() {
			SwfEditorUtils.DoWithEnabledGUI(false, () => {
				var script_prop = SwfEditorUtils.GetPropertyByName(serializedObject, "m_Script");
				EditorGUILayout.PropertyField(script_prop, true);

				var atlas_prop = SwfEditorUtils.GetPropertyByName(serializedObject, "Atlas");
				EditorGUILayout.PropertyField(atlas_prop, true);

				var frame_rate_prop = SwfEditorUtils.GetPropertyByName(serializedObject, "FrameRate");
				EditorGUILayout.PropertyField(frame_rate_prop, true);

				var sequences_prop = SwfEditorUtils.GetPropertyByName(serializedObject, "Sequences");
				if ( sequences_prop.isArray ) {
					SwfEditorUtils.DoWithMixedValue(
						sequences_prop.hasMultipleDifferentValues, () => {
							EditorGUILayout.IntField("Sequence count", sequences_prop.arraySize);
						});
				}
			});
		}

		void DrawGUIControls() {
			GUILayout.BeginHorizontal();
			{
				if ( GUILayout.Button("Create prefab") ) {
					CreateAllClipsPrefabs();
				}
				if ( GUILayout.Button("Instance to scene") ) {
					CreateAllClipsOnScene();
				}
			}
			GUILayout.EndHorizontal();
		}

		// ---------------------------------------------------------------------
		//
		// Messages
		//
		// ---------------------------------------------------------------------

		void OnEnable() {
			_clips = targets
				.OfType<SwfClipAsset>()
				.ToList();
		}

		public override void OnInspectorGUI() {
			DrawGUIProperties();
			DrawGUIControls();
		}
	}
}