using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FlashTools {
	[ExecuteInEditMode]
	[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
	public class FlashAnim : MonoBehaviour {
		public FlashAnimAsset Asset = null;

		int           _current_frame  = 0;
		int           _current_symbol = -1;
		float         _frame_timer    = 0.0f;
		float         _current_z      = 0.0f;

		List<Vector2> _uvs           = new List<Vector2>();
		List<Color>   _mulcolors     = new List<Color>();
		List<Vector4> _addcolors     = new List<Vector4>();
		List<Vector3> _vertices      = new List<Vector3>();
		List<int>     _triangles     = new List<int>();

		public void Play() {
		}

		public void Stop() {
		}

		public void Pause() {
		}

		public void GoToFrame(int frame) {
		}

		public int currentFrame {
			get { return _current_frame; }
			set {
				_current_frame = Mathf.Clamp(value, 0, frameCount - 1);
			}
		}

		public int currentSymbol {
			get { return _current_symbol; }
			set {
				_current_symbol = value;
			}
		}

		public int frameCount {
			get {
				int frames = 0;
				if ( Asset ) {
					var layers = GetCurrentSymbol().Layers;
					for ( var i = 0; i < layers.Count; ++i ) {
						var layer = layers[i];
						frames = Mathf.Max(frames, layer.Frames.Count);
					}
				}
				return frames;
			}
		}

		FlashAnimSymbolData GetCurrentSymbol() {
			if ( currentSymbol >= 0 && currentSymbol < Asset.Data.Library.Symbols.Count ) {
				return Asset.Data.Library.Symbols[currentSymbol];
			} else {
				return Asset.Data.Stage;
			}
		}

		int GetNumFrameByNum(FlashAnimLayerData layer, int num) {
			return num % layer.Frames.Count;
		}

		FlashAnimFrameData GetFrameByNum(FlashAnimLayerData layer, int num) {
			var frame_num = GetNumFrameByNum(layer, num);
			if ( frame_num >= 0 && frame_num < layer.Frames.Count ) {
				return layer.Frames[frame_num];
			}
			return layer.Frames.Count > 0 ? layer.Frames[layer.Frames.Count - 1] : null;
		}

		FlashAnimSymbolData FindSymbol(FlashAnimLibraryData library, string symbol_id) {
			for ( var i = 0; i < library.Symbols.Count; ++i ) {
				var symbol = library.Symbols[i];
				if ( symbol.Id == symbol_id ) {
					return symbol;
				}
			}
			return null;
		}

		FlashAnimBitmapData FindBitmap(FlashAnimLibraryData library, string bitmap_id) {
			for ( var i = 0; i < library.Bitmaps.Count; ++i ) {
				var bitmap = library.Bitmaps[i];
				if ( bitmap.Id == bitmap_id ) {
					return bitmap;
				}
			}
			return null;
		}

		void RenderInstance(FlashAnimInstData elem_data, int frame_num, Matrix4x4 matrix, FlashAnimColorTransform color_trans) {
			if ( elem_data.Type == FlashAnimInstType.Bitmap ) {
				var bitmap = Asset ? FindBitmap(Asset.Data.Library, elem_data.Asset) : null;
				if ( bitmap != null ) {
					var width  = bitmap.RealSize.x;
					var height = bitmap.RealSize.y;

					var v0 = new Vector3(     0,       0, _current_z);
					var v1 = new Vector3( width,       0, _current_z);
					var v2 = new Vector3( width,  height, _current_z);
					var v3 = new Vector3(     0,  height, _current_z);
					_current_z -= 20f;

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
					_uvs.Add(new Vector2(source_rect.xMin, source_rect.yMax));
					_uvs.Add(new Vector2(source_rect.xMax, source_rect.yMax));
					_uvs.Add(new Vector2(source_rect.xMax, source_rect.yMin));
					_uvs.Add(new Vector2(source_rect.xMin, source_rect.yMin));

					_mulcolors.Add(color_trans.Mul);
					_mulcolors.Add(color_trans.Mul);
					_mulcolors.Add(color_trans.Mul);
					_mulcolors.Add(color_trans.Mul);

					_addcolors.Add(color_trans.Add);
					_addcolors.Add(color_trans.Add);
					_addcolors.Add(color_trans.Add);
					_addcolors.Add(color_trans.Add);
				}
			} else if ( elem_data.Type == FlashAnimInstType.Symbol ) {
				var symbol = Asset ? FindSymbol(Asset.Data.Library, elem_data.Asset) : null;
				if ( symbol != null ) {
					RenderSymbol(symbol, frame_num, matrix, color_trans);
				}
			}
		}

		void RenderSymbol(FlashAnimSymbolData symbol, int frame_num, Matrix4x4 matix, FlashAnimColorTransform color_trans) {
			for ( var i = 0; i < symbol.Layers.Count; ++i ) {
				var layer = symbol.Layers[i];
				if ( layer.LayerType != FlashAnimLayerType.Guide &&
					 layer.LayerType != FlashAnimLayerType.Mask &&
					 layer.LayerType != FlashAnimLayerType.Folder )
				{
					var frame = GetFrameByNum(layer, frame_num);
					if ( frame != null ) {
						for ( var j = 0; j < frame.Elems.Count; ++j ) {
							var elem = frame.Elems[j];
							if ( elem.Instance != null && elem.Instance.Visible ) {
								RenderInstance(
									elem.Instance,
									elem.Instance.FirstFrame,
									matix * elem.Matrix,
									color_trans * elem.Instance.ColorTransform);
							}
						}
					}
				}
			}
		}

		void MartDirtySelf() {
		#if UNITY_EDITOR
			EditorUtility.SetDirty(this);
		#endif
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
					//Debug.LogFormat("Cur frame: {0}", _current_frame);
				}
			}
		}

		void OnRenderObject() {
			if ( Asset ) {
				_vertices.Clear();
				_triangles.Clear();
				_uvs.Clear();
				_mulcolors.Clear();
				_addcolors.Clear();
				_current_z = 0.0f;

				RenderSymbol(
					GetCurrentSymbol(),
					_current_frame,
					Matrix4x4.Scale(new Vector3(
						 1.0f / Asset.PixelsPerUnit,
						-1.0f / Asset.PixelsPerUnit,
						 1.0f / Asset.PixelsPerUnit)),
					FlashAnimColorTransform.identity);
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