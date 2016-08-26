using UnityEngine;
using System.Collections.Generic;
using FlashTools.Internal;

namespace FlashTools {
	[ExecuteInEditMode, DisallowMultipleComponent]
	[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
	public class SwfAnimation : MonoBehaviour {
		[SwfReadOnly]
		public SwfAnimationAsset Asset         = null;
		[SwfSortingLayer]
		public string            SortingLayer  = "Default";
		public int               SortingOrder  = 0;

		//
		//
		//

		MeshFilter               _meshFilter   = null;
		MeshRenderer             _meshRenderer = null;
		MaterialPropertyBlock    _matPropBlock = null;

		//
		//
		//

		int _currentFrame = 0;
		public int currentFrame {
			get { return _currentFrame; }
			set {
				_currentFrame = frameCount > 0
					? Mathf.Clamp(value, 0, frameCount - 1)
					: 0;
				UpdateCurrentMesh();
			}
		}

		public int frameCount {
			get {
				return Asset ? Asset.Data.Frames.Count : 0;
			}
		}

		public float frameRate {
			get {
				return Asset ? Asset.Data.FrameRate : 1.0f;
			}
		}

		// ------------------------------------------------------------------------
		//
		// Private
		//
		// ------------------------------------------------------------------------

		SwfAnimationAsset.Frame GetCurrentBakedFrame() {
			var frames = Asset ? Asset.Frames : null;
			return frames != null && frames.Count > 0
				? frames[Mathf.Clamp(currentFrame, 0, frames.Count)]
				: null;
		}

		void UpdateCurrentMesh() {
			var baked_frame = GetCurrentBakedFrame();
			_meshFilter.sharedMesh         = baked_frame != null ? baked_frame.Mesh      : null;
			_meshRenderer.sharedMaterials  = baked_frame != null ? baked_frame.Materials : new Material[0];
			_meshRenderer.sortingOrder     = SortingOrder;
			_meshRenderer.sortingLayerName = SortingLayer;
			_meshRenderer.SetPropertyBlock(_matPropBlock);
		}

		// ---------------------------------------------------------------------
		//
		// Internal
		//
		// ---------------------------------------------------------------------

		public void InitWithAsset(SwfAnimationAsset asset) {
			Asset = asset;
			if ( asset ) {
				_matPropBlock = new MaterialPropertyBlock();
				_matPropBlock.SetTexture("_MainTex", asset.Atlas);
			}
			var controller = GetComponent<SwfAnimationController>();
			if ( controller ) {
				controller.InitWithAsset();
			}
			UpdateCurrentMesh();
		}

		// ---------------------------------------------------------------------
		//
		// Messages
		//
		// ---------------------------------------------------------------------

		void Awake() {
			_meshFilter   = GetComponent<MeshFilter>();
			_meshRenderer = GetComponent<MeshRenderer>();
			InitWithAsset(Asset);
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
	}
}