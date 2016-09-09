using UnityEngine;
using UnityEditor;

using System.Linq;

namespace FlashTools.Internal {
	[CustomPreview(typeof(SwfClipAsset))]
	public class SwfClipAssetPreview : ObjectPreview {
		int                   _sequenceIndex  = 0;
		MaterialPropertyBlock _matPropBlock   = null;
		PreviewRenderUtility  _previewUtility = null;

		Texture2D targetAtlas {
			get {
				var clip = target as SwfClipAsset;
				return clip.Atlas;
			}
		}

		int targetSequenceCount {
			get {
				var clip = target as SwfClipAsset;
				return clip && clip.Sequences != null
					? clip.Sequences.Count
					: 0;
			}
		}

		SwfClipAsset.Frame targetFrame {
			get {
				var clip = target as SwfClipAsset;
				return GetFrameForClip(clip, _sequenceIndex);
			}
		}

		SwfClipAsset.Sequence targetSequence {
			get {
				var clip = target as SwfClipAsset;
				return GetSequenceForClip(clip, _sequenceIndex);
			}
		}

		static SwfClipAsset.Frame GetFrameForClip(SwfClipAsset clip, int sequence_index) {
			var sequence = GetSequenceForClip(clip, sequence_index);
			var frames = sequence != null && sequence.Frames != null && sequence.Frames.Count > 0
				? sequence.Frames
				: null;
			var frame_time = (float)(EditorApplication.timeSinceStartup * clip.FrameRate);
			return frames != null
				? frames[Mathf.FloorToInt(frame_time) % frames.Count]
				: null;
		}

		static SwfClipAsset.Sequence GetSequenceForClip(SwfClipAsset clip, int sequence_index) {
			return clip && clip.Sequences != null && clip.Sequences.Count > 0
				? clip.Sequences[Mathf.Abs(sequence_index) % clip.Sequences.Count]
				: null;
		}

		static Bounds CalculateBoundsForSequence(SwfClipAsset.Sequence sequence) {
			var bounds = sequence != null && sequence.Frames != null && sequence.Frames.Count > 0
				? sequence.Frames
					.Where (p => !!p.Mesh)
					.Select(p => p.Mesh.bounds)
				: new Bounds[0];
			var result = bounds.Any() ? bounds.First() : new Bounds();
			foreach ( var bound in bounds ) {
				result.Encapsulate(bound);
			}
			return result;
		}

		static void ConfigureCameraForSequence(Camera camera, SwfClipAsset.Sequence sequence) {
			var bounds              = CalculateBoundsForSequence(sequence);
			camera.orthographic     = true;
			camera.orthographicSize = Mathf.Max(
				Mathf.Abs(bounds.extents.x),
				Mathf.Abs(bounds.extents.y));
			camera.transform.position = new Vector3(
				bounds.center.x,
				bounds.center.y,
				-10.0f);
		}

		// ---------------------------------------------------------------------
		//
		// Functions
		//
		// ---------------------------------------------------------------------

		public void SetCurrentSequence(string sequence_name) {
			var clip = target as SwfClipAsset;
			_sequenceIndex = clip && clip.Sequences != null
				? Mathf.Max(0, clip.Sequences.FindIndex(p => p.Name == sequence_name))
				: 0;
		}

		// ---------------------------------------------------------------------
		//
		// Messages
		//
		// ---------------------------------------------------------------------

		public override void Initialize(Object[] targets) {
			base.Initialize(targets);
			_matPropBlock   = new MaterialPropertyBlock();
			_previewUtility = new PreviewRenderUtility();
		}

		public override bool HasPreviewGUI() {
			return true;
		}

		public override void OnPreviewSettings() {
			var any_multi_sequences = m_Targets
				.OfType<SwfClipAsset>()
				.Any(p => p.Sequences != null && p.Sequences.Count > 1);
			if ( any_multi_sequences && GUILayout.Button("<", EditorStyles.miniButton) ) {
				--_sequenceIndex;
			}
			var sequence_names = m_Targets
				.OfType<SwfClipAsset>()
				.Select (p => GetSequenceForClip(p, _sequenceIndex))
				.Where  (p => p != null && !string.IsNullOrEmpty(p.Name))
				.Select (p => p.Name)
				.ToArray();
			var label_text = string.Empty;
			for ( int i = 0, e = sequence_names.Length; i < e; ++i ) {
				label_text += string.Format(
					i < e - 1 ? "{0}, " : "{0}",
					sequence_names[i]);
			}
			GUILayout.Label(label_text, EditorStyles.whiteLabel);
			if ( any_multi_sequences && GUILayout.Button(">", EditorStyles.miniButton) ) {
				++_sequenceIndex;
			}
		}

		public override void OnPreviewGUI(Rect r, GUIStyle background) {
			if ( Event.current.type == EventType.Repaint ) {
				var atlas    = targetAtlas;
				var frame    = targetFrame;
				var sequence = targetSequence;
				if ( atlas && frame != null && sequence != null ) {
					_previewUtility.BeginPreview(r, background);
					{
						_matPropBlock.SetTexture("_MainTex", atlas);
						ConfigureCameraForSequence(_previewUtility.m_Camera, sequence);
						for ( var i = 0; i < frame.Materials.Length; ++i ) {
							_previewUtility.DrawMesh(
								frame.Mesh,
								Matrix4x4.identity,
								frame.Materials[i],
								i,
								_matPropBlock);
						}
						_previewUtility.m_Camera.Render();
					}
					_previewUtility.EndAndDrawPreview(r);
				}
			}
		}
	}
}