using UnityEngine;
using FTRuntime.Internal;
using System.Collections.Generic;

namespace FTRuntime {
	[ExecuteInEditMode, DisallowMultipleComponent]
	public class SwfManager : MonoBehaviour {
		SwfAssocList<SwfClip>           _clips           = new SwfAssocList<SwfClip>();
		SwfAssocList<SwfClipController> _controllers     = new SwfAssocList<SwfClipController>();
		SwfList<SwfClipController>      _safeUpdates     = new SwfList<SwfClipController>();

		bool                            _isPaused        = false;
		float                           _rateScale       = 1.0f;
		HashSet<string>                 _groupPauses     = new HashSet<string>();
		Dictionary<string, float>       _groupRateScales = new Dictionary<string, float>();

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

		public int clipCount {
			get { return _clips.Count; }
		}

		public int controllerCount {
			get { return _controllers.Count; }
		}

		public bool isPaused {
			get { return _isPaused; }
			set { _isPaused = value; }
		}

		public bool isPlaying {
			get { return !_isPaused; }
			set { _isPaused = !value; }
		}

		public float rateScale {
			get { return _rateScale; }
			set { _rateScale = Mathf.Clamp(value, 0.0f, float.MaxValue); }
		}

		// ---------------------------------------------------------------------
		//
		// Functions
		//
		// ---------------------------------------------------------------------

		public void Pause() {
			isPaused = true;
		}

		public void Resume() {
			isPlaying = true;
		}

		public void PauseGroup(string group_name) {
			if ( !string.IsNullOrEmpty(group_name) ) {
				_groupPauses.Add(group_name);
			}
		}

		public void ResumeGroup(string group_name) {
			if ( !string.IsNullOrEmpty(group_name) ) {
				_groupPauses.Remove(group_name);
			}
		}

		public bool IsGroupPaused(string group_name) {
			return _groupPauses.Contains(group_name);
		}

		public bool IsGroupPlaying(string group_name) {
			return !IsGroupPaused(group_name);
		}

		public void SetGroupRateScale(string group_name, float rate_scale) {
			if ( !string.IsNullOrEmpty(group_name) ) {
				_groupRateScales[group_name] = Mathf.Clamp(rate_scale, 0.0f, float.MaxValue);
			}
		}

		public float GetGroupRateScale(string group_name) {
			float rate_scale;
			return _groupRateScales.TryGetValue(group_name, out rate_scale)
				? rate_scale
				: 1.0f;
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

		public void GetAllClips(List<SwfClip> clips) {
			_clips.AssignTo(clips);
		}

		public void AddController(SwfClipController controller) {
			_controllers.Add(controller);
		}

		public void RemoveController(SwfClipController controller) {
			_controllers.Remove(controller);
		}

		public void GetAllControllers(List<SwfClipController> controllers) {
			_controllers.AssignTo(controllers);
		}

		void GrabEnabledClips() {
			var clips = FindObjectsOfType<SwfClip>();
			for ( int i = 0, e = clips.Length; i < e; ++i ) {
				var clip = clips[i];
				if ( clip.enabled ) {
					_clips.Add(clip);
				}
			}
		}

		void GrabEnabledControllers() {
			var controllers = FindObjectsOfType<SwfClipController>();
			for ( int i = 0, e = controllers.Length; i < e; ++i ) {
				var controller = controllers[i];
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
					var group_name = ctrl.groupName;
					if ( string.IsNullOrEmpty(group_name) ) {
						ctrl.InternalUpdate(dt);
					} else if ( IsGroupPlaying(group_name) ) {
						var group_rate_scale = GetGroupRateScale(group_name);
						ctrl.InternalUpdate(group_rate_scale * dt);
					}
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