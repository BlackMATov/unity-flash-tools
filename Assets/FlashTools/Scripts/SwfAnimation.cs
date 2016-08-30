using UnityEngine;
using FlashTools.Internal;

namespace FlashTools {
	[ExecuteInEditMode, DisallowMultipleComponent]
	[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
	public class SwfAnimation : MonoBehaviour {

		MeshFilter                 _meshFilter   = null;
		MeshRenderer               _meshRenderer = null;

		SwfAnimationAsset.Sequence _curSequence  = null;
		MaterialPropertyBlock      _curPropBlock = null;

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
		SwfAnimationAsset _asset = null;
		public SwfAnimationAsset asset {
			get { return _asset; }
			set {
				_asset = value;
				ChangeAsset();
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
				return asset && asset.Data != null
					? asset.Data.FrameRate
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
			asset        = _asset;
			sequence     = _sequence;
			currentFrame = _currentFrame;
			sortingLayer = _sortingLayer;
			sortingOrder = _sortingOrder;
		}

		void ChangeAsset() {
			if ( _meshRenderer ) {
				_meshRenderer.enabled = !!asset;
			}
			UpdatePropertyBlock();
			ChangeSequence();
		}

		void ChangeSequence() {
			_curSequence = null;
			if ( asset && asset.Sequences != null ) {
				for ( int i = 0, e = asset.Sequences.Count; i < e; ++i ) {
					var asset_sequence = asset.Sequences[i];
					if ( asset_sequence != null && asset_sequence.Name == sequence ) {
						_curSequence = asset_sequence;
					}
				}
				if ( _curSequence == null ) {
					for ( int i = 0, e = asset.Sequences.Count; i < e; ++i ) {
						var asset_sequence = asset.Sequences[i];
						if ( asset_sequence != null ) {
							_sequence    = asset_sequence.Name;
							_curSequence = asset_sequence;
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
				var atlas = asset && asset.Atlas ? asset.Atlas : null;
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

		SwfAnimationAsset.Frame GetCurrentBakedFrame() {
			var frames = _curSequence != null ? _curSequence.Frames : null;
			return frames != null && currentFrame >= 0 && currentFrame < frames.Count
				? frames[currentFrame]
				: new SwfAnimationAsset.Frame();
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
				swf_manager.AddSwfAnimation(this);
			}
		}

		void OnDisable() {
			var swf_manager = SwfManager.GetInstance(false);
			if ( swf_manager ) {
				swf_manager.RemoveSwfAnimation(this);
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