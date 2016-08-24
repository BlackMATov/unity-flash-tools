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

		HashSet<SwfBakedAnimation> _bakedAnimations = new HashSet<SwfBakedAnimation>();

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

		public void AddSwfBakedAnimation(SwfBakedAnimation animation) {
			_bakedAnimations.Add(animation);
		}

		public void RemoveSwfBakedAnimation(SwfBakedAnimation animation) {
			_bakedAnimations.Remove(animation);
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
			var all_baked_animations = FindObjectsOfType<SwfBakedAnimation>();
			for ( int i = 0, e = all_baked_animations.Length; i < e; ++i ) {
				var baked_animation = all_baked_animations[i];
				if ( baked_animation.enabled ) {
					_bakedAnimations.Add(baked_animation);
				}
			}
		}

		void OnDisable() {
			_bakedAnimations.Clear();
		}

		void Update() {
			var iter = _bakedAnimations.GetEnumerator();
			while ( iter.MoveNext() ) {
				iter.Current.InternalUpdate();
			}
		}
	}
}