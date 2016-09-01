using UnityEngine;
using System.Collections.Generic;

namespace FlashTools {
	[ExecuteInEditMode, DisallowMultipleComponent]
	public class SwfManager : MonoBehaviour {

		HashSet<SwfClip>           _clips       = new HashSet<SwfClip>();
		HashSet<SwfClipController> _controllers = new HashSet<SwfClipController>();

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

		public int AllClipCount {
			get { return _clips.Count; }
		}

		public void AddSwfClip(SwfClip clip) {
			_clips.Add(clip);
		}

		public void RemoveSwfClip(SwfClip clip) {
			_clips.Remove(clip);
		}

		public int AllClipControllerCount {
			get { return _controllers.Count; }
		}

		public void AddSwfClipController(SwfClipController controller) {
			_controllers.Add(controller);
		}

		public void RemoveSwfClipController(SwfClipController controller) {
			_controllers.Remove(controller);
		}

		// ---------------------------------------------------------------------
		//
		// Private
		//
		// ---------------------------------------------------------------------

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

		void UpdateControllers() {
			var dt   = Time.deltaTime;
			var iter = _controllers.GetEnumerator();
			while ( iter.MoveNext() ) {
				iter.Current.InternalUpdate(dt);
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
			UpdateControllers();
		}
	}
}