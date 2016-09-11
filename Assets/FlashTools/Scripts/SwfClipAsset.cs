using UnityEngine;
using FlashTools.Internal;
using System.Collections.Generic;

namespace FlashTools {
	public class SwfClipAsset : ScriptableObject {
		[System.Serializable]
		public class SubMeshData {
			public List<int> Triangles = new List<int>();
		}

		[System.Serializable]
		public class MeshData {
			public List<SubMeshData> SubMeshes = new List<SubMeshData>();
			public List<Vector3>     Vertices  = new List<Vector3>();
			public List<Vector2>     UVs       = new List<Vector2>();
			public List<Vector4>     AddColors = new List<Vector4>();
			public List<Color>       MulColors = new List<Color>();

			public void FillMesh(Mesh mesh) {
				if ( SubMeshes.Count > 0 ) {
					mesh.subMeshCount = SubMeshes.Count;
					mesh.SetVertices(Vertices);
					for ( var i = 0; i < SubMeshes.Count; ++i ) {
						mesh.SetTriangles(SubMeshes[i].Triangles, i);
					}
					mesh.SetUVs(0, UVs);
					mesh.SetUVs(1, AddColors);
					mesh.SetColors(MulColors);
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