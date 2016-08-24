using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using FlashTools.Internal;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FlashTools {
	[ExecuteInEditMode]
	[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
	public class SwfAnimation : MonoBehaviour {
		public SwfAnimationAsset Asset        = null;
		public int               GroupCount   = 0;

		public int               SortingOrder = 0;
		[SwfSortingLayer]
		public string            SortingLayer = "Default";

		int    _current_frame   = 0;
		float  _frame_timer     = 0.0f;
		string _last_asset_path = string.Empty;

		List<Vector2>   _uvs       = new List<Vector2>();
		List<Color>     _mulcolors = new List<Color>();
		List<Vector4>   _addcolors = new List<Vector4>();
		List<Vector3>   _vertices  = new List<Vector3>();

		class Group {
			public SwfAnimationInstanceType Type;
			public int                      ClipDepth;
			public List<int>                Triangles;
			public Material                 Material;
		}

		List<Group> _groups = new List<Group>();

		public int frameCount {
			get { return Asset ? Asset.Data.Frames.Count : 0; }
		}

		public int currentFrame {
			get { return _current_frame; }
			set { _current_frame = Mathf.Clamp(value, 0, frameCount - 1); }
		}

		// ------------------------------------------------------------------------
		//
		// Messages
		//
		// ------------------------------------------------------------------------

		void Update() {
			if ( Asset ) {
				_frame_timer += Asset.Data.FrameRate * Time.deltaTime;
				while ( _frame_timer > 1.0f ) {
					_frame_timer -= 1.0f;
					++_current_frame;
					if ( _current_frame > frameCount - 1 ) {
						_current_frame = 0;
					}
				}
			} else {
			#if UNITY_EDITOR
				OnValidate();
			#endif
			}
		}

	#if UNITY_EDITOR
		void OnValidate() {
			if ( Asset ) {
				_last_asset_path = AssetDatabase.GetAssetPath(Asset);
			} else {
				if ( !string.IsNullOrEmpty(_last_asset_path) ) {
					Asset = AssetDatabase.LoadAssetAtPath<SwfAnimationAsset>(_last_asset_path);
					EditorUtility.SetDirty(this);
				}
			}
		}
	#endif

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

		void OnRenderObject() {
			if ( Asset ) {
				_vertices.Clear();
				_uvs.Clear();
				_mulcolors.Clear();
				_addcolors.Clear();
				_groups.Clear();

				var frame = Asset.Data.Frames[currentFrame];
				foreach ( var inst in frame.Instances ) {
					var bitmap = FindBitmap(inst.Bitmap);
					if ( bitmap != null ) {
						var width  = bitmap.RealSize.x / 20.0f;
						var height = bitmap.RealSize.y / 20.0f;

						var v0 = new Vector3(    0,      0, 0);
						var v1 = new Vector3(width,      0, 0);
						var v2 = new Vector3(width, height, 0);
						var v3 = new Vector3(    0, height, 0);

						var matrix =
							Matrix4x4.Scale(new Vector3(
								 1.0f / Asset.Settings.PixelsPerUnit,
								-1.0f / Asset.Settings.PixelsPerUnit,
								 1.0f / Asset.Settings.PixelsPerUnit)) * inst.Matrix;

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

						if ( _groups.Count == 0 || _groups[_groups.Count - 1].Type != inst.Type || _groups[_groups.Count - 1].ClipDepth != inst.ClipDepth) {
							var gr = new Group();
							gr.Type = inst.Type;
							gr.ClipDepth = inst.ClipDepth;
							gr.Triangles = new List<int>();
							_groups.Add(gr);
						}

						_groups[_groups.Count - 1].Triangles.Add(_vertices.Count - 4 + 2);
						_groups[_groups.Count - 1].Triangles.Add(_vertices.Count - 4 + 1);
						_groups[_groups.Count - 1].Triangles.Add(_vertices.Count - 4 + 0);
						_groups[_groups.Count - 1].Triangles.Add(_vertices.Count - 4 + 0);
						_groups[_groups.Count - 1].Triangles.Add(_vertices.Count - 4 + 3);
						_groups[_groups.Count - 1].Triangles.Add(_vertices.Count - 4 + 2);
					}
				}

				var full_groups = _groups.Where(p => p.Triangles.Count > 0).ToArray();
				for ( var i = 0; i < full_groups.Length; ++i )  {
					var gr = full_groups[i];
					switch ( gr.Type ) {
					case SwfAnimationInstanceType.Mask:
						gr.Material = new Material(Shader.Find("FlashTools/SwfIncrMask"));
						gr.Material.SetTexture("_MainTex", Asset.Atlas);
						break;
					case SwfAnimationInstanceType.Group:
						gr.Material = new Material(Shader.Find("FlashTools/SwfSimple"));
						gr.Material.SetTexture("_MainTex", Asset.Atlas);
						break;
					case SwfAnimationInstanceType.Masked:
						gr.Material = new Material(Shader.Find("FlashTools/SwfMasked"));
						gr.Material.SetTexture("_MainTex", Asset.Atlas);
						gr.Material.SetInt("_StencilID", gr.ClipDepth);
						break;
					case SwfAnimationInstanceType.MaskReset:
						gr.Material = new Material(Shader.Find("FlashTools/SwfDecrMask"));
						gr.Material.SetTexture("_MainTex", Asset.Atlas);
						break;
					}
				}

				var mesh_renderer = GetComponent<MeshRenderer>();
				mesh_renderer.sharedMaterials = full_groups.Select(p => p.Material).ToArray();
				mesh_renderer.sortingOrder = SortingOrder;
				mesh_renderer.sortingLayerName = SortingLayer;

				var mesh_filter = GetComponent<MeshFilter>();
				if ( mesh_filter ) {
					var mesh = mesh_filter.sharedMesh
						? mesh_filter.sharedMesh
						: new Mesh();
					mesh.Clear();
					mesh.subMeshCount = full_groups.Length;
					GroupCount = full_groups.Length;
					mesh.SetVertices(_vertices);
					for ( var i = 0; i < full_groups.Length; ++i ) {
						mesh.SetTriangles(full_groups[i].Triangles, i);
					}
					mesh.SetUVs(0, _uvs);
					mesh.SetUVs(1, _addcolors);
					mesh.SetColors(_mulcolors);
					mesh.RecalculateNormals();
					mesh_filter.sharedMesh = mesh;
				}
			}
		}
	}
}