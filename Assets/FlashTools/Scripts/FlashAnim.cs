using UnityEngine;
using System.Collections.Generic;

namespace FlashTools {
	[ExecuteInEditMode]
	public class FlashAnim : MonoBehaviour {
		public FlashAnimAsset Asset = null;

		int _current_frame = 0;
		float _frame_timer = 0.0f;

		List<Vector3> _vertices  = new List<Vector3>();
		List<int>     _triangles = new List<int>();
		List<Vector2> _uvs       = new List<Vector2>();

		public void Play() {
		}

		public void Stop() {
		}

		public void Pause() {
		}

		public void GoToFrame(int frame) {
		}

		public int frameCount {
			get {
				int frames = 0;
				if ( Asset ) {
					foreach ( var layer in GetCurrentSymbol().Layers ) {
						frames = Mathf.Max(frames, layer.Frames.Count);
					}
				}
				return frames;
			}
		}

		FlashAnimSymbolData GetCurrentSymbol() {
			//return Asset.Data.Library.Symbols[0];
			return Asset.Data.Stage;
		}

		int GetNumFrameByNum(FlashAnimLayerData layer, int num) {
			return num % layer.Frames.Count;
		}

		FlashAnimFrameData GetFrameByNum(FlashAnimLayerData layer, int num) {
			var frame_num = GetNumFrameByNum(layer, num);
			if ( frame_num >= 0 && frame_num < layer.Frames.Count ) {
				return layer.Frames[frame_num];
			}
			return null;
		}

		FlashAnimSymbolData FindSymbol(FlashAnimLibraryData library, string symbol_id) {
			foreach ( var symbol in library.Symbols ) {
				if ( symbol.Id == symbol_id ) {
					return symbol;
				}
			}
			return null;
		}

		FlashAnimBitmapData FindBitmap(FlashAnimLibraryData library, string bitmap_id) {
			foreach ( var bitmap in library.Bitmaps ) {
				if ( bitmap.Id == bitmap_id ) {
					return bitmap;
				}
			}
			return null;
		}

		void RenderInstance(FlashAnimInstData elem_data, int frame_num, Matrix4x4 matrix) {
			if ( elem_data.Type == FlashAnimInstType.Bitmap ) {
				var bitmap = Asset ? FindBitmap(Asset.Data.Library, elem_data.Asset) : null;
				if ( bitmap != null ) {
					var width  = bitmap.RealSize.x;
					var height = bitmap.RealSize.y;

					var v0 = new Vector3(     0,       0, 0);
					var v1 = new Vector3( width,       0, 0);
					var v2 = new Vector3( width,  height, 0);
					var v3 = new Vector3(     0,  height, 0);

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
				}
			} else if ( elem_data.Type == FlashAnimInstType.Symbol ) {
				var symbol = Asset ? FindSymbol(Asset.Data.Library, elem_data.Asset) : null;
				if ( symbol != null ) {
					RenderSymbol(symbol, frame_num, matrix);
				}
			}
		}

		void RenderSymbol(FlashAnimSymbolData symbol, int frame_num, Matrix4x4 matix) {
			foreach ( var layer in symbol.Layers ) {
				if ( layer.LayerType != FlashAnimLayerType.Mask ) {
					var frame = GetFrameByNum(layer, frame_num);
					if ( frame != null ) {
						foreach ( var elem in frame.Elems ) {
							if ( elem.Instance != null ) {
								RenderInstance(
									elem.Instance, frame_num, matix * elem.Matrix);
							}
						}
					}
				}
			}
		}

		void Update() {
			_frame_timer += 25.0f * Time.deltaTime;
			while ( _frame_timer > 1.0f ) {
				_frame_timer -= 1.0f;
				++_current_frame;
				if ( _current_frame > frameCount - 1 ) {
					_current_frame = 0;
				}
				//Debug.LogFormat("Cur frame: {0}", _current_frame);
			}
		}

		void OnRenderObject() {
			if ( Asset ) {
				_vertices.Clear();
				_triangles.Clear();
				_uvs.Clear();
				RenderSymbol(
					GetCurrentSymbol(),
					_current_frame,
					Matrix4x4.Scale(new Vector3(1,-1,1)));

				var mesh       = new Mesh();
				mesh.vertices  = _vertices.ToArray();
				mesh.triangles = _triangles.ToArray();
				mesh.uv        = _uvs.ToArray();
				mesh.RecalculateNormals();
				GetComponent<MeshFilter>().mesh = mesh;
			}
		}
	}
}