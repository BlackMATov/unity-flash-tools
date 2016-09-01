using UnityEngine;
using UnityEditor;

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace FlashTools.Internal {
	[CustomEditor(typeof(SwfAnimationClipAsset)), CanEditMultipleObjects]
	public class SwfAnimationClipAssetEditor : Editor {
		List<SwfAnimationClipAsset> _clips = new List<SwfAnimationClipAsset>();

		static string GetClipPath(SwfAnimationClipAsset clip) {
			return clip
				? AssetDatabase.GetAssetPath(clip)
				: string.Empty;
		}

		static string GetPrefabPath(SwfAnimationClipAsset clip) {
			var clip_path = GetClipPath(clip);
			return string.IsNullOrEmpty(clip_path)
				? string.Empty
				: Path.ChangeExtension(clip_path, ".prefab");
		}

		//
		//
		//

		static GameObject CreateAnimationGO(SwfAnimationClipAsset clip) {
			if ( clip ) {
				var anim_go = new GameObject(clip.name);
				anim_go.AddComponent<MeshFilter>();
				anim_go.AddComponent<MeshRenderer>();
				anim_go.AddComponent<SwfAnimation>().clip = clip;
				anim_go.AddComponent<SwfAnimationController>();
				return anim_go;
			}
			return null;
		}

		static void CreateClipPrefab(SwfAnimationClipAsset clip) {
			var anim_go = CreateAnimationGO(clip);
			if ( anim_go ) {
				var prefab_path = GetPrefabPath(clip);
				if ( !string.IsNullOrEmpty(prefab_path) ) {
					var prefab = AssetDatabase.LoadMainAssetAtPath(prefab_path);
					if ( !prefab ) {
						prefab = PrefabUtility.CreateEmptyPrefab(prefab_path);
					}
					PrefabUtility.ReplacePrefab(
						anim_go,
						prefab,
						ReplacePrefabOptions.ConnectToPrefab);
				}
				GameObject.DestroyImmediate(anim_go, true);
			}
		}

		static void CreateClipOnScene(SwfAnimationClipAsset clip) {
			var anim_go = CreateAnimationGO(clip);
			if ( anim_go ) {
				Undo.RegisterCreatedObjectUndo(anim_go, "Instance SwfAnimation");
			}
		}

		//
		//
		//

		void AllClipsForeach(Action<SwfAnimationClipAsset> act) {
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
				.OfType<SwfAnimationClipAsset>()
				.ToList();
		}

		public override void OnInspectorGUI() {
			DrawGUIProperties();
			DrawGUIControls();
		}
	}
}