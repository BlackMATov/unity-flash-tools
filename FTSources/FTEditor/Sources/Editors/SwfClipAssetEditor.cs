using UnityEngine;
using UnityEditor;

using System.IO;
using System.Linq;
using System.Collections.Generic;

using FTRuntime;

namespace FTEditor.Editors {
	[CustomEditor(typeof(SwfClipAsset)), CanEditMultipleObjects]
	class SwfClipAssetEditor : Editor {
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

		static int GetFrameCount(SwfClipAsset clip) {
			return clip != null ? clip.Sequences.Aggregate(0, (acc, seq) => {
				return seq.Frames.Count + acc;
			}) : 0;
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

		static GameObject CreateClipPrefab(SwfClipAsset clip) {
			GameObject result = null;
			var clip_go = CreateClipGO(clip);
			if ( clip_go ) {
				var prefab_path = GetPrefabPath(clip);
				if ( !string.IsNullOrEmpty(prefab_path) ) {
					var prefab = AssetDatabase.LoadMainAssetAtPath(prefab_path);
					if ( !prefab ) {
						prefab = PrefabUtility.CreateEmptyPrefab(prefab_path);
					}
					result = PrefabUtility.ReplacePrefab(
						clip_go,
						prefab,
						ReplacePrefabOptions.ConnectToPrefab);
				}
				GameObject.DestroyImmediate(clip_go, true);
			}
			return result;
		}

		static GameObject CreateClipOnScene(SwfClipAsset clip) {
			var clip_go = CreateClipGO(clip);
			if ( clip_go ) {
				Undo.RegisterCreatedObjectUndo(clip_go, "Instance SwfClip");
			}
			return clip_go;
		}

		//
		//
		//

		void CreateAllClipsPrefabs() {
			Selection.objects = _clips
				.Select (p => CreateClipPrefab(p))
				.Where  (p => !!p)
				.ToArray();
		}

		void CreateAllClipsOnScene() {
			Selection.objects = _clips
				.Select (p => CreateClipOnScene(p))
				.Where  (p => !!p)
				.ToArray();
		}

		//
		//
		//

		void DrawGUIFrameCount() {
			var counts      = _clips.Select(p => GetFrameCount(p));
			var mixed_value = counts.GroupBy(p => p).Count() > 1;
			SwfEditorUtils.DoWithEnabledGUI(false, () => {
				SwfEditorUtils.DoWithMixedValue(
					mixed_value, () => {
						EditorGUILayout.IntField("Frame count", counts.First());
					});
			});
		}

		void DrawGUISequences() {
			SwfEditorUtils.DoWithEnabledGUI(false, () => {
				var sequences_prop = SwfEditorUtils.GetPropertyByName(
					serializedObject, "Sequences");
				if ( sequences_prop.isArray ) {
					SwfEditorUtils.DoWithMixedValue(
						sequences_prop.hasMultipleDifferentValues, () => {
							EditorGUILayout.IntField("Sequence count", sequences_prop.arraySize);
						});
				}
			});
		}

		void DrawGUIControls() {
			SwfEditorUtils.DoHorizontalGUI(() => {
				if ( GUILayout.Button("Create prefab") ) {
					CreateAllClipsPrefabs();
				}
				if ( GUILayout.Button("Instance to scene") ) {
					CreateAllClipsOnScene();
				}
			});
		}

		// ---------------------------------------------------------------------
		//
		// Messages
		//
		// ---------------------------------------------------------------------

		void OnEnable() {
			_clips = targets.OfType<SwfClipAsset>().ToList();
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();
			DrawDefaultInspector();
			DrawGUIFrameCount();
			DrawGUISequences();
			DrawGUIControls();
			if ( GUI.changed ) {
				serializedObject.ApplyModifiedProperties();
			}
		}

		public override bool RequiresConstantRepaint() {
			return true;
		}
	}
}