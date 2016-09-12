using UnityEngine;
using FlashTools.Internal;
using System.Collections.Generic;

namespace FlashTools {
	public static class SwfClipAssetCache {
		public static List<int> Triangles = new List<int>();
		public static void FillTriangles(int start_vertex, int triangle_count) {
			Triangles.Clear();
			if ( Triangles.Capacity < triangle_count ) {
				Triangles.Capacity = triangle_count * 2;
			}
			for ( var i = 0; i < triangle_count; i += 6 ) {
				Triangles.Add(start_vertex + 2);
				Triangles.Add(start_vertex + 1);
				Triangles.Add(start_vertex + 0);
				Triangles.Add(start_vertex + 0);
				Triangles.Add(start_vertex + 3);
				Triangles.Add(start_vertex + 2);
				start_vertex += 4;
			}
		}

		public static List<Vector3> Vertices = new List<Vector3>();
		public static void FillVertices(
			Vector2 mesh_min, float mesh_scale, List<uint> vertices)
		{
			Vertices.Clear();
			if ( Vertices.Capacity < vertices.Count ) {
				Vertices.Capacity = vertices.Count * 2;
			}
			for ( int i = 0, e = vertices.Count; i < e; ++i ) {
				Vertices.Add(
					(mesh_min +
					SwfUtils.UnpackCoordsFromUInt(vertices[i])) / mesh_scale);
			}
		}

		public static List<Vector2> UVs = new List<Vector2>();
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

		public static List<Vector4> AddColors = new List<Vector4>();
		public static void FillAddColors(List<uint> colors) {
			AddColors.Clear();
			if ( AddColors.Capacity < colors.Count * 2 ) {
				AddColors.Capacity = colors.Count * 2 * 2;
			}
			for ( int i = 0, e = colors.Count; i < e; i += 2 ) {
				Vector4 color;
				SwfUtils.UnpackColorFromUInts(colors[i+0], colors[i+1], out color);
				AddColors.Add(color);
				AddColors.Add(color);
				AddColors.Add(color);
				AddColors.Add(color);
			}
		}

		public static List<Color> MulColors = new List<Color>();
		public static void FillMulColors(List<uint> colors) {
			MulColors.Clear();
			if ( MulColors.Capacity < colors.Count * 2 ) {
				MulColors.Capacity = colors.Count * 2 * 2;
			}
			for ( int i = 0, e = colors.Count; i < e; i += 2 ) {
				Color color;
				SwfUtils.UnpackColorFromUInts(colors[i+0], colors[i+1], out color);
				MulColors.Add(color);
				MulColors.Add(color);
				MulColors.Add(color);
				MulColors.Add(color);
			}
		}
	}

	public class SwfClipAsset : ScriptableObject {
		[System.Serializable]
		public class SubMeshData {
			public int StartVertex;
			public int TriangleCount;
		}

		[System.Serializable]
		public class MeshData {
			public List<SubMeshData> SubMeshes = new List<SubMeshData>();
			public Vector2           MeshMin   = Vector2.zero;
			public float             MeshScale = 1.0f;
			public List<uint>        Vertices  = new List<uint>();
			public List<uint>        UVs       = new List<uint>();
			public List<uint>        AddColors = new List<uint>();
			public List<uint>        MulColors = new List<uint>();

			public void FillMesh(Mesh mesh) {
				if ( SubMeshes.Count > 0 ) {
					mesh.subMeshCount = SubMeshes.Count;

					SwfClipAssetCache.FillVertices(MeshMin, MeshScale, Vertices);
					mesh.SetVertices(SwfClipAssetCache.Vertices);

					for ( var i = 0; i < SubMeshes.Count; ++i ) {
						SwfClipAssetCache.FillTriangles(
							SubMeshes[i].StartVertex, SubMeshes[i].TriangleCount);
						mesh.SetTriangles(SwfClipAssetCache.Triangles, i);
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
}