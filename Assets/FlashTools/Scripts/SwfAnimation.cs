using UnityEngine;
using System.Collections.Generic;
using FlashTools.Internal;

namespace FlashTools {
	[ExecuteInEditMode]
	[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
	public class SwfAnimation : MonoBehaviour {
		public SwfAnimationAsset Asset         = null;
		[SwfSortingLayer]
		public string            SortingLayer  = "Default";
		public int               SortingOrder  = 0;

		bool                     _inited       = false;
		MaterialPropertyBlock    _matPropBlock = null;

		MeshFilter               _meshFilter   = null;
		MeshRenderer             _meshRenderer = null;

		int    _current_frame = 0;
		float  _frame_timer   = 0.0f;

		public int frameCount {
			get { return Asset ? Asset.Data.Frames.Count : 0; }
		}

		public int currentFrame {
			get { return _current_frame; }
			set {
				_current_frame = Mathf.Clamp(value, 0, frameCount - 1);
				FixCurrentMesh();
			}
		}

		// ------------------------------------------------------------------------
		//
		// Stuff
		//
		// ------------------------------------------------------------------------

		public void InitWithAsset(SwfAnimationAsset asset) {
			Asset = asset;
			if ( Asset && !_inited ) {
				_inited = true;
				_matPropBlock = new MaterialPropertyBlock();
				_matPropBlock.SetTexture("_MainTex", Asset.Atlas);
			}
			FixCurrentMesh();
		}

		void UpdateFrameTimer() {
			if ( Asset ) {
				_frame_timer += Asset.Data.FrameRate * Time.deltaTime;
				while ( _frame_timer > 1.0f ) {
					_frame_timer -= 1.0f;
					++_current_frame;
					if ( _current_frame > frameCount - 1 ) {
						_current_frame = 0;
					}
					FixCurrentMesh();
				}
			}
		}

		void FixCurrentMesh() {
			if ( Asset && Asset.BakedFrames.Count > 0 ) {
				var frame = Asset.BakedFrames[Mathf.Clamp(currentFrame, 0, Asset.BakedFrames.Count)];
				_meshFilter.sharedMesh         = frame.Mesh;
				_meshRenderer.sharedMaterials  = frame.Materials;
				_meshRenderer.sortingOrder     = SortingOrder;
				_meshRenderer.sortingLayerName = SortingLayer;
				_meshRenderer.SetPropertyBlock(_matPropBlock);
			}
		}

		// ---------------------------------------------------------------------
		//
		// Internal
		//
		// ---------------------------------------------------------------------

		public void InternalUpdate() {
			UpdateFrameTimer();
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