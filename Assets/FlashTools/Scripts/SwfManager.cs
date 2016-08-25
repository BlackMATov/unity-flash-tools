using UnityEngine;
using System.Collections.Generic;
using FlashTools.Internal;

namespace FlashTools {
	[ExecuteInEditMode]
	public class SwfManager : MonoBehaviour {
		// ---------------------------------------------------------------------
		//
		// Consts
		//
		// ---------------------------------------------------------------------

		const int    SwfStencilIdCount     = 10;
		const string SwfSimpleShaderPath   = "FlashTools/SwfSimple";
		const string SwfMaskedShaderPath   = "FlashTools/SwfMasked";
		const string SwfIncrMaskShaderPath = "FlashTools/SwfIncrMask";
		const string SwfDecrMaskShaderPath = "FlashTools/SwfDecrMask";

		// ---------------------------------------------------------------------
		//
		// Properties
		//
		// ---------------------------------------------------------------------

		[SerializeField] [HideInInspector] Shader         _maskedShader     = null;
		[SerializeField] [HideInInspector] List<Material> _maskedMaterials  = null;
		[SerializeField] [HideInInspector] Material       _simpleMaterial   = null;
		[SerializeField] [HideInInspector] Material       _incrMaskMaterial = null;
		[SerializeField] [HideInInspector] Material       _decrMaskMaterial = null;

		HashSet<SwfAnimation> _animations = new HashSet<SwfAnimation>();

		// ---------------------------------------------------------------------
		//
		// Instance
		//
		// ---------------------------------------------------------------------

		static SwfManager _instance;
		public static SwfManager Instance {
			get {
				if ( !_instance ) {
					_instance = FindObjectOfType<SwfManager>();
				}
				return _instance;
			}
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
			}
		}

		Shader SafeLoadShader(string path) {
			var shader = Shader.Find(path);
			if ( !shader ) {
				throw new UnityException(string.Format(
					"SwfManager. Shader not found: {0}",
					path));
			}
			return shader;
		}

		// ---------------------------------------------------------------------
		//
		// Functions
		//
		// ---------------------------------------------------------------------

		public Material GetMaskedMaterial(int stencil_id) {
			if ( stencil_id < 0 || stencil_id >= SwfStencilIdCount ) {
				throw new UnityException(string.Format(
					"SwfManager. Unsupported stencil id: {0}",
					stencil_id));
			}
			FillMaterialsCache();
			return _maskedMaterials[stencil_id];
		}

		public Material GetSimpleMaterial() {
			FillMaterialsCache();
			return _simpleMaterial;
		}

		public Material GetIncrMaskMaterial() {
			FillMaterialsCache();
			return _incrMaskMaterial;
		}

		public Material GetDecrMaskMaterial() {
			FillMaterialsCache();
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
			var all_animations = FindObjectsOfType<SwfAnimation>();
			for ( int i = 0, e = all_animations.Length; i < e; ++i ) {
				var animation = all_animations[i];
				if ( animation.enabled ) {
					_animations.Add(animation);
				}
			}
		}

		void OnDisable() {
			_animations.Clear();
		}

		void Update() {
			var iter = _animations.GetEnumerator();
			while ( iter.MoveNext() ) {
				iter.Current.InternalUpdate();
			}
		}
	}
}