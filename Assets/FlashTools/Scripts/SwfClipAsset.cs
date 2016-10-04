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
			public SubMeshData[] SubMeshes = new SubMeshData[0];
			public Vector2[]     Vertices  = new Vector2[0];
			public uint[]        UVs       = new uint[0];
			public uint[]        AddColors = new uint[0];
			public uint[]        MulColors = new uint[0];

			public void FillMesh(Mesh mesh) {
				if ( SubMeshes.Length > 0 ) {
					mesh.subMeshCount = SubMeshes.Length;

					SwfClipAssetCache.FillVertices(Vertices);
					mesh.SetVertices(SwfClipAssetCache.Vertices);

					for ( int i = 0, e = SubMeshes.Length; i < e; ++i ) {
						SwfClipAssetCache.FillTriangles(
							SubMeshes[i].StartVertex,
							SubMeshes[i].IndexCount);
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
		public string          Name;
		[SwfReadOnly]
		public Texture2D       Atlas;
		[SwfReadOnly]
		public float           FrameRate;
		[HideInInspector]
		public List<Sequence>  Sequences;

		void Reset() {
			Name      = string.Empty;
			Atlas     = null;
			FrameRate = 1.0f;
			Sequences = new List<Sequence>();
		}
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

		static        Vector3       Vertex   = Vector3.zero;
		public static List<Vector3> Vertices = new List<Vector3>(PreallocatedVertices);
		public static void FillVertices(Vector2[] vertices) {
			Vertices.Clear();
			if ( Vertices.Capacity < vertices.Length ) {
				Vertices.Capacity = vertices.Length * 2;
			}
			for ( int i = 0, e = vertices.Length; i < e; ++i ) {
				var vert = vertices[i];
				Vertex.x = vert.x;
				Vertex.y = vert.y;
				Vertices.Add(Vertex);
			}
		}

		static        Vector2       UV0 = Vector2.zero;
		static        Vector2       UV1 = Vector2.zero;
		static        Vector2       UV2 = Vector2.zero;
		static        Vector2       UV3 = Vector2.zero;
		public static List<Vector2> UVs = new List<Vector2>(PreallocatedVertices);
		public static void FillUVs(uint[] uvs) {
			UVs.Clear();
			if ( UVs.Capacity < uvs.Length * 2 ) {
				UVs.Capacity = uvs.Length * 2 * 2;
			}
			for ( int i = 0, e = uvs.Length; i < e; i += 2 ) {
				float min_x, min_y, max_x, max_y;
				SwfUtils.UnpackUV(uvs[i+0], out min_x, out min_y);
				SwfUtils.UnpackUV(uvs[i+1], out max_x, out max_y);

				UV0.x = min_x; UV0.y = min_y;
				UV1.x = max_x; UV1.y = min_y;
				UV2.x = max_x; UV2.y = max_y;
				UV3.x = min_x; UV3.y = max_y;

				UVs.Add(UV0);
				UVs.Add(UV1);
				UVs.Add(UV2);
				UVs.Add(UV3);
			}
		}

		static        Vector4       AddColor  = Vector4.one;
		public static List<Vector4> AddColors = new List<Vector4>(PreallocatedVertices);
		public static void FillAddColors(uint[] colors) {
			AddColors.Clear();
			if ( AddColors.Capacity < colors.Length * 2 ) {
				AddColors.Capacity = colors.Length * 2 * 2;
			}
			for ( int i = 0, e = colors.Length; i < e; i += 2 ) {
				SwfUtils.UnpackFColorFromUInts(
					colors[i+0], colors[i+1],
					out AddColor.x, out AddColor.y,
					out AddColor.z, out AddColor.w);
				AddColors.Add(AddColor);
				AddColors.Add(AddColor);
				AddColors.Add(AddColor);
				AddColors.Add(AddColor);
			}
		}

		static        Color       MulColor  = Color.white;
		public static List<Color> MulColors = new List<Color>(PreallocatedVertices);
		public static void FillMulColors(uint[] colors) {
			MulColors.Clear();
			if ( MulColors.Capacity < colors.Length * 2 ) {
				MulColors.Capacity = colors.Length * 2 * 2;
			}
			for ( int i = 0, e = colors.Length; i < e; i += 2 ) {
				SwfUtils.UnpackFColorFromUInts(
					colors[i+0], colors[i+1],
					out MulColor.r, out MulColor.g,
					out MulColor.b, out MulColor.a);
				MulColors.Add(MulColor);
				MulColors.Add(MulColor);
				MulColors.Add(MulColor);
				MulColors.Add(MulColor);
			}
		}
	}
}