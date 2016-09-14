using UnityEngine;
using FlashTools.Internal;
using System.Collections.Generic;

namespace FlashTools {
	public class SwfClipAsset : ScriptableObject {
		[System.Serializable]
		public class SubMeshData {
			public int StartVertex;
			public int IndexCount;
		}

		[System.Serializable]
		public class MeshData {
			public List<SubMeshData> SubMeshes = new List<SubMeshData>();
			public List<Vector2>     Vertices  = new List<Vector2>();
			public List<uint>        UVs       = new List<uint>();
			public List<uint>        AddColors = new List<uint>();
			public List<uint>        MulColors = new List<uint>();

			public void FillMesh(Mesh mesh) {
				if ( SubMeshes.Count > 0 ) {
					mesh.subMeshCount = SubMeshes.Count;

					SwfClipAssetCache.FillVertices(Vertices);
					mesh.SetVertices(SwfClipAssetCache.Vertices);

					for ( int i = 0, e = SubMeshes.Count; i < e; ++i ) {
						SwfClipAssetCache.FillTriangles(
							SubMeshes[i].StartVertex, SubMeshes[i].IndexCount);
						mesh.SetTriangles(SwfClipAssetCache.Indices, i);
					}

					SwfClipAssetCache.FillUVs(UVs);
					mesh.SetUVs(0, SwfClipAssetCache.UVs);

					SwfClipAssetCache.FillAddColors(AddColors);
					mesh.SetUVs(1, SwfClipAssetCache.AddColors);

					SwfClipAssetCache.FillMulColors(MulColors);
					mesh.SetColors(SwfClipAssetCache.MulColors);
				}
			}
		}

		[System.Serializable]
		public class Frame {
			public MeshData   MeshData  = new MeshData();
			public Material[] Materials = new Material[0];

			public Frame() {
				MeshData  = new MeshData();
				Materials = new Material[0];
			}

			public Frame(MeshData mesh_data, Material[] materials) {
				MeshData  = mesh_data;
				Materials = materials;
			}

			Mesh _cachedMesh = null;
			public Mesh CachedMesh {
				get {
					if ( !_cachedMesh ) {
						_cachedMesh = new Mesh();
						MeshData.FillMesh(_cachedMesh);
					}
					return _cachedMesh;
				}
			}
		}
		[System.Serializable]
		public class Sequence {
			public string      Name   = string.Empty;
			public List<Frame> Frames = new List<Frame>();
		}

		[SwfReadOnly]
		public Texture2D       Atlas;
		[SwfAssetGUID(true)]
		public string          Container;
		[SwfReadOnly]
		public float           FrameRate;
		[HideInInspector]
		public List<Sequence>  Sequences;

		#if UNITY_EDITOR
		void Reset() {
			Atlas     = null;
			Container = string.Empty;
			FrameRate = 1.0f;
			Sequences = new List<Sequence>();
		}
		#endif
	}

	// ---------------------------------------------------------------------
	//
	// SwfClipAssetCache
	//
	// ---------------------------------------------------------------------

	static class SwfClipAssetCache {
		const int PreallocatedVertices = 500;

		public static List<int> Indices = new List<int>(PreallocatedVertices * 6 / 4);
		public static void FillTriangles(int start_vertex, int index_count) {
			Indices.Clear();
			if ( Indices.Capacity < index_count ) {
				Indices.Capacity = index_count * 2;
			}
			for ( var i = 0; i < index_count; i += 6 ) {
				Indices.Add(start_vertex + 2);
				Indices.Add(start_vertex + 1);
				Indices.Add(start_vertex + 0);
				Indices.Add(start_vertex + 0);
				Indices.Add(start_vertex + 3);
				Indices.Add(start_vertex + 2);
				start_vertex += 4;
			}
		}

		public static List<Vector3> Vertices = new List<Vector3>(PreallocatedVertices);
		public static void FillVertices(List<Vector2> vertices) {
			Vertices.Clear();
			if ( Vertices.Capacity < vertices.Count ) {
				Vertices.Capacity = vertices.Count * 2;
			}
			for ( int i = 0, e = vertices.Count; i < e; ++i ) {
				Vertices.Add(vertices[i]);
			}
		}

		public static List<Vector2> UVs = new List<Vector2>(PreallocatedVertices);
		public static void FillUVs(List<uint> uvs) {
			UVs.Clear();
			if ( UVs.Capacity < uvs.Count * 2 ) {
				UVs.Capacity = uvs.Count * 2 * 2;
			}
			for ( int i = 0, e = uvs.Count; i < e; i += 2 ) {
				var min = SwfUtils.UnpackUV(uvs[i+0]);
				var max = SwfUtils.UnpackUV(uvs[i+1]);
				UVs.Add(new Vector2(min.x, min.y));
				UVs.Add(new Vector2(max.x, min.y));
				UVs.Add(new Vector2(max.x, max.y));
				UVs.Add(new Vector2(min.x, max.y));
			}
		}

		public static List<Vector4> AddColors = new List<Vector4>(PreallocatedVertices);
		public static void FillAddColors(List<uint> colors) {
			AddColors.Clear();
			if ( AddColors.Capacity < colors.Count * 2 ) {
				AddColors.Capacity = colors.Count * 2 * 2;
			}
			for ( int i = 0, e = colors.Count; i < e; i += 2 ) {
				Vector4 color;
				SwfUtils.UnpackFColorFromUInts(
					colors[i+0], colors[i+1], out color);
				AddColors.Add(color);
				AddColors.Add(color);
				AddColors.Add(color);
				AddColors.Add(color);
			}
		}

		public static List<Color> MulColors = new List<Color>(PreallocatedVertices);
		public static void FillMulColors(List<uint> colors) {
			MulColors.Clear();
			if ( MulColors.Capacity < colors.Count * 2 ) {
				MulColors.Capacity = colors.Count * 2 * 2;
			}
			for ( int i = 0, e = colors.Count; i < e; i += 2 ) {
				Color color;
				SwfUtils.UnpackFColorFromUInts(
					colors[i+0], colors[i+1], out color);
				MulColors.Add(color);
				MulColors.Add(color);
				MulColors.Add(color);
				MulColors.Add(color);
			}
		}
	}
}