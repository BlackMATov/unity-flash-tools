using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FlashTools {
	[ExecuteInEditMode]
	public class SwfManager : MonoBehaviour {
		// ---------------------------------------------------------------------
		//
		// Constants
		//
		// ---------------------------------------------------------------------

		const string SwfSimpleMatPath    = "Assets/FlashTools/Resources/Materials/SwfSimpleMat.mat";
		const string SwfIncrMaskMatPath  = "Assets/FlashTools/Resources/Materials/SwfIncrMaskMat.mat";
		const string SwfDecrMaskMatPath  = "Assets/FlashTools/Resources/Materials/SwfDecrMaskMat.mat";
		const string SwfMaskedMatPathFmt = "Assets/FlashTools/Resources/Materials/SwfMaskedMat_{0}.mat";

		// ---------------------------------------------------------------------
		//
		// Properties
		//
		// ---------------------------------------------------------------------

		[SerializeField] [HideInInspector] Material       _simpleMaterial   = null;
		[SerializeField] [HideInInspector] Material       _incrMaskMaterial = null;
		[SerializeField] [HideInInspector] Material       _decrMaskMaterial = null;
		[SerializeField] [HideInInspector] List<Material> _maskedMaterials  = null;

		HashSet<SwfAnimation> _animations = new HashSet<SwfAnimation>();

		// ---------------------------------------------------------------------
		//
		// Instance
		//
		// ---------------------------------------------------------------------

		static SwfManager _instance;
		public static SwfManager GetInstance(bool allow_create) {
			if ( !_instance ) {
				_instance = FindObjectOfType<SwfManager>();
				if ( allow_create && !_instance ) {
					var go = new GameObject("[SwfManager]");
					_instance = go.AddComponent<SwfManager>();
				}
			}
			return _instance;
		}

		// ---------------------------------------------------------------------
		//
		// Internal
		//
		// ---------------------------------------------------------------------

		public void AddSwfAnimation(SwfAnimation animation) {
			_animations.Add(animation);
		}

		public void RemoveSwfAnimation(SwfAnimation animation) {
			_animations.Remove(animation);
		}

		// ---------------------------------------------------------------------
		//
		// Private
		//
		// ---------------------------------------------------------------------

		void FillMaterialsCache() {
			/*
			if ( !_maskedShader ) {
				_maskedShader = SafeLoadShader(SwfMaskedShaderPath);
			}
			if ( _maskedMaterials == null ) {
				_maskedMaterials = new List<Material>(SwfStencilIdCount);
				for ( var i = 0; i < _maskedMaterials.Capacity; ++i ) {
					var material = new Material(_maskedShader);
					material.SetInt("_StencilID", i);
					_maskedMaterials.Add(material);
				}
			}
			if ( !_simpleMaterial ) {
				_simpleMaterial   = new Material(SafeLoadShader(SwfSimpleShaderPath));
			}
			if ( !_incrMaskMaterial ) {
				_incrMaskMaterial = new Material(SafeLoadShader(SwfIncrMaskShaderPath));
			}
			if ( !_decrMaskMaterial ) {
				_decrMaskMaterial = new Material(SafeLoadShader(SwfDecrMaskShaderPath));
			}*/

			if ( !_simpleMaterial ) {
				_simpleMaterial = SafeLoadMaterial(SwfSimpleMatPath, true);
			}

			if ( !_incrMaskMaterial ) {
				_incrMaskMaterial = SafeLoadMaterial(SwfIncrMaskMatPath, true);
			}

			if ( !_decrMaskMaterial ) {
				_decrMaskMaterial = SafeLoadMaterial(SwfDecrMaskMatPath, true);
			}

			if ( _maskedMaterials == null ) {
				_maskedMaterials = new List<Material>();
				for ( var i = 0; i < 100; ++i ) {
					var mat = SafeLoadMaterial(string.Format(SwfMaskedMatPathFmt, i), false);
					if ( mat ) {
						_maskedMaterials.Add(mat);
					} else {
						break;
					}
				}
			}
		}

		Material SafeLoadMaterial(string path, bool exception) {
		#if UNITY_EDITOR
			var material = AssetDatabase.LoadAssetAtPath<Material>(path);
			if ( !material && exception ) {
				throw new UnityException(string.Format(
					"SwfManager. Material not found: {0}",
					path));
			}
			return material;
		#else
			throw new UnityException("IMPLME!");
		#endif
		}

		/*
		Shader SafeLoadShader(string path) {
			var shader = Shader.Find(path);
			if ( !shader ) {
				throw new UnityException(string.Format(
					"SwfManager. Shader not found: {0}",
					path));
			}
			return shader;
		}*/

		void GrabEnabledAnimations() {
			var all_animations = FindObjectsOfType<SwfAnimation>();
			for ( int i = 0, e = all_animations.Length; i < e; ++i ) {
				var animation = all_animations[i];
				if ( animation.enabled ) {
					_animations.Add(animation);
				}
			}
		}

		void DropAnimations() {
			_animations.Clear();
		}

		void UpdateAnimations() {
			var iter = _animations.GetEnumerator();
			while ( iter.MoveNext() ) {
				iter.Current.InternalUpdate();
			}
		}

		// ---------------------------------------------------------------------
		//
		// Functions
		//
		// ---------------------------------------------------------------------

		public Material GetMaskedMaterial(int stencil_id) {
			if ( stencil_id < 0 || stencil_id >= _maskedMaterials.Count ) {
				throw new UnityException(string.Format(
					"SwfManager. Unsupported stencil id: {0}",
					stencil_id));
			}
			return _maskedMaterials[stencil_id];
		}

		public Material GetSimpleMaterial() {
			return _simpleMaterial;
		}

		public Material GetIncrMaskMaterial() {
			return _incrMaskMaterial;
		}

		public Material GetDecrMaskMaterial() {
			return _decrMaskMaterial;
		}

		// ---------------------------------------------------------------------
		//
		// Messages
		//
		// ---------------------------------------------------------------------

		void Awake() {
			FillMaterialsCache();
		}

		void OnEnable() {
			GrabEnabledAnimations();
		}

		void OnDisable() {
			DropAnimations();
		}

		void Update() {
			UpdateAnimations();
		}
	}
}