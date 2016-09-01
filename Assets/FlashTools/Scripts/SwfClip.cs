using UnityEngine;
using FlashTools.Internal;

namespace FlashTools {
	[ExecuteInEditMode, DisallowMultipleComponent]
	[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
	public class SwfClip : MonoBehaviour {

		MeshFilter            _meshFilter   = null;
		MeshRenderer          _meshRenderer = null;

		SwfClipAsset.Sequence _curSequence  = null;
		MaterialPropertyBlock _curPropBlock = null;

		// ---------------------------------------------------------------------
		//
		// Properties
		//
		// ---------------------------------------------------------------------

		[Header("Sorting")]
		[SerializeField][SwfSortingLayer]
		public string _sortingLayer = "Default";
		public string sortingLayer {
			get { return _sortingLayer; }
			set {
				_sortingLayer = value;
				ChangeSortingProperties();
			}
		}

		[SerializeField]
		public int _sortingOrder = 0;
		public int sortingOrder {
			get { return _sortingOrder; }
			set {
				_sortingOrder = value;
				ChangeSortingProperties();
			}
		}

		[Header("Animation")]
		[SerializeField]
		SwfClipAsset _clip = null;
		public SwfClipAsset clip {
			get { return _clip; }
			set {
				_clip = value;
				ChangeClip();
			}
		}

		[SerializeField][HideInInspector]
		string _sequence = "Default";
		public string sequence {
			get { return _sequence; }
			set {
				_sequence = value;
				ChangeSequence();
			}
		}

		[SerializeField][HideInInspector]
		int _currentFrame = 0;
		public int currentFrame {
			get { return _currentFrame; }
			set {
				_currentFrame = value;
				ChangeCurrentFrame();
			}
		}

		public int frameCount {
			get {
				return _curSequence != null && _curSequence.Frames != null
					? _curSequence.Frames.Count
					: 0;
			}
		}

		public float frameRate {
			get {
				return clip ? clip.FrameRate : 1.0f;
			}
		}

		// ---------------------------------------------------------------------
		//
		// Functions
		//
		// ---------------------------------------------------------------------

		public void ToBeginFrame() {
			currentFrame = 0;
		}

		public void ToEndFrame() {
			currentFrame = frameCount - 1;
		}

		public bool ToPrevFrame() {
			if ( currentFrame > 0 ) {
				--currentFrame;
				return true;
			}
			return false;
		}

		public bool ToNextFrame() {
			if ( currentFrame < frameCount - 1 ) {
				++currentFrame;
				return true;
			}
			return false;
		}

		// ---------------------------------------------------------------------
		//
		// Private
		//
		// ---------------------------------------------------------------------

		public void UpdateAllProperties() {
			clip         = _clip;
			sequence     = _sequence;
			currentFrame = _currentFrame;
			sortingLayer = _sortingLayer;
			sortingOrder = _sortingOrder;
		}

		void ChangeClip() {
			if ( _meshRenderer ) {
				_meshRenderer.enabled = !!clip;
			}
			UpdatePropertyBlock();
			ChangeSequence();
		}

		void ChangeSequence() {
			_curSequence = null;
			if ( clip && clip.Sequences != null ) {
				for ( int i = 0, e = clip.Sequences.Count; i < e; ++i ) {
					var clip_sequence = clip.Sequences[i];
					if ( clip_sequence != null && clip_sequence.Name == sequence ) {
						_curSequence = clip_sequence;
					}
				}
				if ( _curSequence == null ) {
					for ( int i = 0, e = clip.Sequences.Count; i < e; ++i ) {
						var clip_sequence = clip.Sequences[i];
						if ( clip_sequence != null ) {
							_sequence    = clip_sequence.Name;
							_curSequence = clip_sequence;
							break;
						}
					}
				}
			}
			ChangeCurrentFrame();
		}

		void ChangeCurrentFrame() {
			_currentFrame = frameCount > 0
				? Mathf.Clamp(currentFrame, 0, frameCount - 1)
				: 0;
			UpdateCurrentMesh();
		}

		void ChangeSortingProperties() {
			if ( _meshRenderer ) {
				_meshRenderer.sortingOrder     = sortingOrder;
				_meshRenderer.sortingLayerName = sortingLayer;
			}
		}

		void UpdatePropertyBlock() {
			if ( _meshRenderer ) {
				if ( _curPropBlock == null ) {
					_curPropBlock = new MaterialPropertyBlock();
				}
				_meshRenderer.GetPropertyBlock(_curPropBlock);
				var atlas = clip ? clip.Atlas : null;
				if ( atlas ) {
					_curPropBlock.SetTexture("_MainTex", atlas);
				}
				_meshRenderer.SetPropertyBlock(_curPropBlock);
			}
		}

		void UpdateCurrentMesh() {
			if ( _meshFilter && _meshRenderer ) {
				var baked_frame = GetCurrentBakedFrame();
				_meshFilter.sharedMesh = baked_frame.Mesh;
				_meshRenderer.sharedMaterials = baked_frame.Materials;
			}
		}

		SwfClipAsset.Frame GetCurrentBakedFrame() {
			var frames = _curSequence != null ? _curSequence.Frames : null;
			return frames != null && currentFrame >= 0 && currentFrame < frames.Count
				? frames[currentFrame]
				: new SwfClipAsset.Frame();
		}

		// ---------------------------------------------------------------------
		//
		// Messages
		//
		// ---------------------------------------------------------------------

		void Awake() {
			_meshFilter   = GetComponent<MeshFilter>();
			_meshRenderer = GetComponent<MeshRenderer>();
			_curSequence  = null;
			_curPropBlock = null;
			UpdateAllProperties();
		}

		void OnEnable() {
			var swf_manager = SwfManager.GetInstance(true);
			if ( swf_manager ) {
				swf_manager.AddSwfClip(this);
			}
		}

		void OnDisable() {
			var swf_manager = SwfManager.GetInstance(false);
			if ( swf_manager ) {
				swf_manager.RemoveSwfClip(this);
			}
		}

	#if UNITY_EDITOR
		void Reset() {
			UpdateAllProperties();
		}

		void OnValidate() {
			UpdateAllProperties();
		}
	#endif
	}
}