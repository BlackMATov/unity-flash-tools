using UnityEngine;
using FTRuntime.Internal;

namespace FTRuntime {
	[ExecuteInEditMode, DisallowMultipleComponent]
	[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
	public class SwfClip : MonoBehaviour {

		MeshFilter            _meshFilter   = null;
		MeshRenderer          _meshRenderer = null;

		bool                  _dirtyMesh    = true;
		SwfClipAsset.Sequence _curSequence  = null;
		MaterialPropertyBlock _curPropBlock = null;

		// ---------------------------------------------------------------------
		//
		// Properties
		//
		// ---------------------------------------------------------------------

		[Header("Sorting")]
		[SerializeField][SwfSortingLayer]
		public string _sortingLayer = string.Empty;
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
		Color _tint = Color.white;
		public Color tint {
			get { return _tint; }
			set {
				_tint = value;
				ChangeTint();
			}
		}

		[SerializeField]
		SwfClipAsset _clip = null;
		public SwfClipAsset clip {
			get { return _clip; }
			set {
				_clip         = value;
				_sequence     = string.Empty;
				_currentFrame = 0;
				ChangeClip();
			}
		}

		[SerializeField][HideInInspector]
		string _sequence = string.Empty;
		public string sequence {
			get { return _sequence; }
			set {
				_sequence     = value;
				_currentFrame = 0;
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
				return clip
					? clip.FrameRate
					: 1.0f;
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
			currentFrame = frameCount > 0
				? frameCount - 1
				: 0;
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
		// Internal
		//
		// ---------------------------------------------------------------------

		internal void InternalLateUpdate() {
			if ( _meshFilter && _meshRenderer && _dirtyMesh ) {
				var baked_frame = GetCurrentBakedFrame();
				if ( baked_frame != null ) {
					_meshFilter  .sharedMesh      = baked_frame.CachedMesh;
					_meshRenderer.sharedMaterials = baked_frame.Materials;
				} else {
					_meshFilter  .sharedMesh      = null;
					_meshRenderer.sharedMaterials = new Material[0];
				}
				_dirtyMesh = false;
			}
		}

		public void UpdateAllProperties() {
			ClearCache();
			ChangeTint();
			ChangeClip();
			ChangeSequence();
			ChangeCurrentFrame();
			ChangeSortingProperties();
		}

		void ClearCache() {
			_meshFilter   = GetComponent<MeshFilter>();
			_meshRenderer = GetComponent<MeshRenderer>();
			_dirtyMesh    = true;
			_curSequence  = null;
			_curPropBlock = null;
		}

		void ChangeTint() {
			UpdatePropBlock();
		}

		void ChangeClip() {
			if ( _meshRenderer ) {
				_meshRenderer.enabled = !!clip;
			}
			ChangeSequence();
			UpdatePropBlock();
		}

		void ChangeSequence() {
			_curSequence = null;
			if ( clip && clip.Sequences != null ) {
				if ( !string.IsNullOrEmpty(sequence) ) {
					for ( int i = 0, e = clip.Sequences.Count; i < e; ++i ) {
						var clip_sequence = clip.Sequences[i];
						if ( clip_sequence != null && clip_sequence.Name == sequence ) {
							_curSequence = clip_sequence;
							break;
						}
					}
					if ( _curSequence == null ) {
						Debug.LogWarningFormat(this,
							"<b>[FlashTools]</b> Sequence '{0}' not found",
							sequence);
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
			_dirtyMesh    = true;
			_currentFrame = frameCount > 0
				? Mathf.Clamp(currentFrame, 0, frameCount - 1)
				: 0;
		}

		void ChangeSortingProperties() {
			if ( _meshRenderer ) {
				_meshRenderer.sortingOrder     = sortingOrder;
				_meshRenderer.sortingLayerName = sortingLayer;
			}
		}

		void UpdatePropBlock() {
			if ( _meshRenderer ) {
				if ( _curPropBlock == null ) {
					_curPropBlock = new MaterialPropertyBlock();
				}
				_meshRenderer.GetPropertyBlock(_curPropBlock);
				_curPropBlock.SetColor(
					"_Tint",
					tint);
				_curPropBlock.SetTexture(
					"_MainTex",
					clip && clip.Atlas ? clip.Atlas : Texture2D.whiteTexture);
				_meshRenderer.SetPropertyBlock(_curPropBlock);
			}
		}

		SwfClipAsset.Frame GetCurrentBakedFrame() {
			var frames = _curSequence != null ? _curSequence.Frames : null;
			return frames != null && currentFrame >= 0 && currentFrame < frames.Count
				? frames[currentFrame]
				: null;
		}

		// ---------------------------------------------------------------------
		//
		// Messages
		//
		// ---------------------------------------------------------------------

		void Awake() {
			UpdateAllProperties();
		}

		void OnEnable() {
			var swf_manager = SwfManager.GetInstance(true);
			if ( swf_manager ) {
				swf_manager.AddClip(this);
			}
		}

		void OnDisable() {
			var swf_manager = SwfManager.GetInstance(false);
			if ( swf_manager ) {
				swf_manager.RemoveClip(this);
			}
		}

		void Reset() {
			UpdateAllProperties();
		}

		void OnValidate() {
			UpdateAllProperties();
		}
	}
}