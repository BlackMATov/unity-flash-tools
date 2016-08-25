using UnityEngine;
using System.Collections.Generic;
using FlashTools.Internal;

namespace FlashTools {
	[ExecuteInEditMode]
	[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
	public class SwfAnimation : MonoBehaviour {
		public SwfAnimationAsset Asset = null;

		[SwfSortingLayer]
		public string            SortingLayer = "Default";
		public int               SortingOrder = 0;

		MeshFilter   _meshFilter   = null;
		MeshRenderer _meshRenderer = null;

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

		MaterialPropertyBlock MatPropBlock;

		class Frame {
			public Mesh       Mesh;
			public Material[] Materials;
		}

		List<Frame> _frames = new List<Frame>();

		class Group {
			public SwfAnimationInstanceType Type;
			public int                      ClipDepth;
			public List<int>                Triangles;
			public Material                 Material;
		}

		List<Vector2>  _uvs       = new List<Vector2>();
		List<Color>    _mulcolors = new List<Color>();
		List<Vector4>  _addcolors = new List<Vector4>();
		List<Vector3>  _vertices  = new List<Vector3>();
		List<Group>    _groups    = new List<Group>();
		List<Material> _materials = new List<Material>();

		void ClearTempBakeData() {
			_uvs.Clear();
			_mulcolors.Clear();
			_addcolors.Clear();
			_vertices.Clear();
			_groups.Clear();
			_materials.Clear();
		}

		public void BakeFrameMeshes() {
			if ( Asset && Asset.Atlas && Asset.Data != null && Asset.Data.Frames.Count > 0 ) {
				MatPropBlock = new MaterialPropertyBlock();
				MatPropBlock.SetTexture("_MainTex", Asset.Atlas);
				for ( var i = 0; i < Asset.Data.Frames.Count; ++i ) {
					var frame = Asset.Data.Frames[i];
					BakeFrameMesh(frame);
				}
			}
			FixCurrentMesh();
		}

		void BakeFrameMesh(SwfAnimationFrameData frame) {
			var swf_manager = SwfManager.Instance;
			for ( var i = 0; i < frame.Instances.Count; ++i ) {
				var inst   = frame.Instances[i];
				var bitmap = inst != null ? FindBitmap(inst.Bitmap) : null;
				if ( bitmap != null ) {
					var width  = bitmap.RealSize.x / 20.0f;
					var height = bitmap.RealSize.y / 20.0f;

					var v0 = new Vector3(    0,      0, 0);
					var v1 = new Vector3(width,      0, 0);
					var v2 = new Vector3(width, height, 0);
					var v3 = new Vector3(    0, height, 0);

					var matrix =
						Matrix4x4.Scale(new Vector3(
							+1.0f / Asset.Settings.PixelsPerUnit,
							-1.0f / Asset.Settings.PixelsPerUnit,
							+1.0f / Asset.Settings.PixelsPerUnit)) * inst.Matrix;

					_vertices.Add(matrix.MultiplyPoint3x4(v0));
					_vertices.Add(matrix.MultiplyPoint3x4(v1));
					_vertices.Add(matrix.MultiplyPoint3x4(v2));
					_vertices.Add(matrix.MultiplyPoint3x4(v3));

					var source_rect = bitmap.SourceRect;
					_uvs.Add(new Vector2(source_rect.xMin, source_rect.yMin));
					_uvs.Add(new Vector2(source_rect.xMax, source_rect.yMin));
					_uvs.Add(new Vector2(source_rect.xMax, source_rect.yMax));
					_uvs.Add(new Vector2(source_rect.xMin, source_rect.yMax));

					_mulcolors.Add(inst.ColorTransform.Mul);
					_mulcolors.Add(inst.ColorTransform.Mul);
					_mulcolors.Add(inst.ColorTransform.Mul);
					_mulcolors.Add(inst.ColorTransform.Mul);

					_addcolors.Add(inst.ColorTransform.Add);
					_addcolors.Add(inst.ColorTransform.Add);
					_addcolors.Add(inst.ColorTransform.Add);
					_addcolors.Add(inst.ColorTransform.Add);

					if ( _groups.Count == 0 ||
						 _groups[_groups.Count - 1].Type != inst.Type ||
						 _groups[_groups.Count - 1].ClipDepth != inst.ClipDepth )
					{
						_groups.Add(new Group{
							Type      = inst.Type,
							ClipDepth = inst.ClipDepth,
							Triangles = new List<int>(),
							Material  = null
						});
					}

					_groups[_groups.Count - 1].Triangles.Add(_vertices.Count - 4 + 2);
					_groups[_groups.Count - 1].Triangles.Add(_vertices.Count - 4 + 1);
					_groups[_groups.Count - 1].Triangles.Add(_vertices.Count - 4 + 0);
					_groups[_groups.Count - 1].Triangles.Add(_vertices.Count - 4 + 0);
					_groups[_groups.Count - 1].Triangles.Add(_vertices.Count - 4 + 3);
					_groups[_groups.Count - 1].Triangles.Add(_vertices.Count - 4 + 2);
				}
			}

			for ( var i = 0; i < _groups.Count; ++i ) {
				var group = _groups[i];
				switch ( group.Type ) {
				case SwfAnimationInstanceType.Mask:
					group.Material = swf_manager.GetIncrMaskMaterial();
					break;
				case SwfAnimationInstanceType.Group:
					group.Material = swf_manager.GetSimpleMaterial();
					break;
				case SwfAnimationInstanceType.Masked:
					group.Material = swf_manager.GetMaskedMaterial(group.ClipDepth);
					break;
				case SwfAnimationInstanceType.MaskReset:
					group.Material = swf_manager.GetDecrMaskMaterial();
					break;
				}
			}

			for ( var i = 0; i < _groups.Count; ++i ) {
				var group = _groups[i];
				_materials.Add(group.Material);
			}

			var mesh = new Mesh();
			mesh.subMeshCount = _groups.Count;
			mesh.SetVertices(_vertices);
			for ( var i = 0; i < _groups.Count; ++i ) {
				mesh.SetTriangles(_groups[i].Triangles, i);
			}
			mesh.SetUVs(0, _uvs);
			mesh.SetUVs(1, _addcolors);
			mesh.SetColors(_mulcolors);
			mesh.RecalculateNormals();

			_frames.Add(new Frame{
				Mesh      = mesh,
				Materials = _materials.ToArray()});
			ClearTempBakeData();
		}

		SwfAnimationBitmapData FindBitmap(int bitmap_id) {
			if ( Asset ) {
				for ( var i = 0; i < Asset.Data.Bitmaps.Count; ++i ) {
					var bitmap = Asset.Data.Bitmaps[i];
					if ( bitmap.Id == bitmap_id ) {
						return bitmap;
					}
				}
			}
			return null;
		}

		void UpdateFrameTimer() {
			if ( Asset ) {
				if ( _frames.Count == 0 ) {
					BakeFrameMeshes();
					FixCurrentMesh();
				}
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
			if ( _frames.Count > 0 ) {
				var frame = _frames[Mathf.Clamp(currentFrame, 0, _frames.Count)];
				_meshFilter.sharedMesh         = frame.Mesh;
				_meshRenderer.sharedMaterials  = frame.Materials;
				_meshRenderer.sortingOrder     = SortingOrder;
				_meshRenderer.sortingLayerName = SortingLayer;
				_meshRenderer.SetPropertyBlock(MatPropBlock);
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
		}

		void OnEnable() {
			var swf_manager = SwfManager.Instance;
			if ( swf_manager ) {
				swf_manager.AddSwfAnimation(this);
			}
		}

		void OnDisable() {
			var swf_manager = SwfManager.Instance;
			if ( swf_manager ) {
				swf_manager.RemoveSwfAnimation(this);
			}
		}
	}
}