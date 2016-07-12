using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace FlashTools {
	[ExecuteInEditMode]
	[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
	public class SwfAnimation : MonoBehaviour {
		public SwfAnimationAsset Asset = null;

		int    _current_frame   = 0;
		float  _frame_timer     = 0.0f;
		string _last_asset_path = string.Empty;

		List<Vector2> _uvs           = new List<Vector2>();
		List<Color>   _mulcolors     = new List<Color>();
		List<Vector4> _addcolors     = new List<Vector4>();
		List<Vector3> _vertices      = new List<Vector3>();
		List<int>     _triangles     = new List<int>();

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

		void Start() {
			if ( Asset && Asset.Atlas ) {
				var material = new Material(Shader.Find("FlashTools/FlashAnim"));
				material.SetTexture("_MainTex", Asset.Atlas);
				GetComponent<MeshRenderer>().sharedMaterial = material;
			}
		}

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
				OnValidate();
			}
		}

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
				_triangles.Clear();
				_uvs.Clear();
				_mulcolors.Clear();
				_addcolors.Clear();

				var current_z = 0.0f;

				var frame = Asset.Data.Frames[currentFrame];
				foreach ( var inst in frame.Instances ) {
					var bitmap = FindBitmap(inst.Bitmap);
					if ( bitmap != null ) {
						var width  = bitmap.RealSize.x;
						var height = bitmap.RealSize.y;

						var v0 = new Vector3(     0,       0, current_z);
						var v1 = new Vector3( width,       0, current_z);
						var v2 = new Vector3( width,  height, current_z);
						var v3 = new Vector3(     0,  height, current_z);
						current_z -= 20f;

						var matrix =
							Matrix4x4.Scale(new Vector3(
								 1.0f / Asset.PixelsPerUnit,
								-1.0f / Asset.PixelsPerUnit,
								 1.0f / Asset.PixelsPerUnit)) * inst.Matrix;

						_vertices.Add(matrix.MultiplyPoint3x4(v0));
						_vertices.Add(matrix.MultiplyPoint3x4(v1));
						_vertices.Add(matrix.MultiplyPoint3x4(v2));
						_vertices.Add(matrix.MultiplyPoint3x4(v3));

						_triangles.Add(_vertices.Count - 4 + 2);
						_triangles.Add(_vertices.Count - 4 + 1);
						_triangles.Add(_vertices.Count - 4 + 0);
						_triangles.Add(_vertices.Count - 4 + 0);
						_triangles.Add(_vertices.Count - 4 + 3);
						_triangles.Add(_vertices.Count - 4 + 2);

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
					}
				}

				var mesh_filter = GetComponent<MeshFilter>();
				if ( mesh_filter ) {
					var mesh = mesh_filter.sharedMesh
						? mesh_filter.sharedMesh
						: new Mesh();
					mesh.Clear();
					mesh.SetVertices(_vertices);
					mesh.SetTriangles(_triangles, 0);
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