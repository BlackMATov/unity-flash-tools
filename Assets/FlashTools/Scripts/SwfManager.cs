using UnityEngine;
using FlashTools.Internal;

namespace FlashTools {
	[ExecuteInEditMode, DisallowMultipleComponent]
	public class SwfManager : MonoBehaviour {

		SwfAssocList<SwfClip>           _clips       = new SwfAssocList<SwfClip>();
		SwfAssocList<SwfClipController> _controllers = new SwfAssocList<SwfClipController>();

		bool                            _isPaused    = false;
		SwfList<SwfClipController>      _safeUpdates = new SwfList<SwfClipController>();

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
		// Properties
		//
		// ---------------------------------------------------------------------

		[SerializeField]
		[SwfFloatRange(0.0f, float.MaxValue)]
		float _rateScale = 1.0f;
		public float rateScale {
			get { return _rateScale; }
			set { _rateScale = Mathf.Clamp(value, 0.0f, float.MaxValue); }
		}

		public int clipCount {
			get { return _clips.Count; }
		}

		public int controllerCount {
			get { return _controllers.Count; }
		}

		public bool isPaused {
			get { return _isPaused; }
		}

		public bool isPlaying {
			get { return !isPaused; }
		}

		// ---------------------------------------------------------------------
		//
		// Functions
		//
		// ---------------------------------------------------------------------

		public void Pause() {
			_isPaused = true;
		}

		public void Resume() {
			_isPaused = false;
		}

		// ---------------------------------------------------------------------
		//
		// Internal
		//
		// ---------------------------------------------------------------------

		public void AddClip(SwfClip clip) {
			_clips.Add(clip);
		}

		public void RemoveClip(SwfClip clip) {
			_clips.Remove(clip);
		}

		public void AddController(SwfClipController controller) {
			_controllers.Add(controller);
		}

		public void RemoveController(SwfClipController controller) {
			_controllers.Remove(controller);
		}

		void GrabEnabledClips() {
			var all_clips = FindObjectsOfType<SwfClip>();
			for ( int i = 0, e = all_clips.Length; i < e; ++i ) {
				var clip = all_clips[i];
				if ( clip.enabled ) {
					_clips.Add(clip);
				}
			}
		}

		void GrabEnabledControllers() {
			var all_controllers = FindObjectsOfType<SwfClipController>();
			for ( int i = 0, e = all_controllers.Length; i < e; ++i ) {
				var controller = all_controllers[i];
				if ( controller.enabled ) {
					_controllers.Add(controller);
				}
			}
		}

		void DropClips() {
			_clips.Clear();
		}

		void DropControllers() {
			_controllers.Clear();
		}

		void UpdateControllers(float dt) {
			_controllers.AssignTo(_safeUpdates);
			for ( int i = 0, e = _safeUpdates.Count; i < e; ++i ) {
				var ctrl = _safeUpdates[i];
				if ( ctrl ) {
					ctrl.InternalUpdate(dt);
				}
			}
			_safeUpdates.Clear();
		}

		void LateUpdateClips() {
			for ( int i = 0, e = _clips.Count; i < e; ++i ) {
				var clip = _clips[i];
				if ( clip ) {
					clip.InternalLateUpdate();
				}
			}
		}

		// ---------------------------------------------------------------------
		//
		// Messages
		//
		// ---------------------------------------------------------------------

		void OnEnable() {
			GrabEnabledClips();
			GrabEnabledControllers();
		}

		void OnDisable() {
			DropClips();
			DropControllers();
		}

		void Update() {
			if ( isPlaying ) {
				var dt = Time.deltaTime;
				UpdateControllers(rateScale * dt);
			}
		}

		void LateUpdate() {
			LateUpdateClips();
		}
	}
}