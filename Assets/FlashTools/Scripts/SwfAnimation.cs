using UnityEngine;
using FlashTools.Internal;

namespace FlashTools {
	[ExecuteInEditMode, DisallowMultipleComponent]
	[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
	public class SwfAnimation : MonoBehaviour {

		MeshFilter   _meshFilter   = null;
		MeshRenderer _meshRenderer = null;

		// ------------------------------------------------------------------------
		//
		// Properties
		//
		// ------------------------------------------------------------------------

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
				return asset && asset.Data != null && asset.Data.Frames != null
					? asset.Data.Frames.Count
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

		// ------------------------------------------------------------------------
		//
		// Functions
		//
		// ------------------------------------------------------------------------

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

		// ------------------------------------------------------------------------
		//
		// Private
		//
		// ------------------------------------------------------------------------

		void ChangeSortingProperties() {
			_meshRenderer.sortingOrder     = sortingOrder;
			_meshRenderer.sortingLayerName = sortingLayer;
		}

		void ChangeAsset() {
			_meshRenderer.enabled = !!asset;
			if ( asset ) {
				var prop_block = new MaterialPropertyBlock();
				prop_block.SetTexture("_MainTex", asset.Atlas);
				_meshRenderer.SetPropertyBlock(prop_block);
			}
			UpdateCurrentMesh();
		}

		void ChangeCurrentFrame() {
			_currentFrame = frameCount > 0
				? Mathf.Clamp(currentFrame, 0, frameCount - 1)
				: 0;
			UpdateCurrentMesh();
		}

		void UpdateCurrentMesh() {
			var baked_frame               = GetCurrentBakedFrame();
			_meshFilter.sharedMesh        = baked_frame.Mesh;
			_meshRenderer.sharedMaterials = baked_frame.Materials;
		}

		SwfAnimationAsset.Frame GetCurrentBakedFrame() {
			return currentFrame >= 0 && currentFrame < frameCount
				? asset.Frames[currentFrame]
				: SwfAnimationAsset.Frame.identity;
		}

		// ---------------------------------------------------------------------
		//
		// Messages
		//
		// ---------------------------------------------------------------------

		void Awake() {
			_meshFilter   = GetComponent<MeshFilter>();
			_meshRenderer = GetComponent<MeshRenderer>();
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
			OnValidate();
		}

		void OnValidate() {
			asset        = _asset;
			currentFrame = _currentFrame;
			sortingLayer = _sortingLayer;
			sortingOrder = _sortingOrder;
		}
	#endif
	}
}