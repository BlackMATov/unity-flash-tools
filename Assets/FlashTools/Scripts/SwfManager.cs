using UnityEngine;
using System.Collections.Generic;

namespace FlashTools {
	[ExecuteInEditMode, DisallowMultipleComponent]
	public class SwfManager : MonoBehaviour {

		HashSet<SwfAnimation>           _animations  = new HashSet<SwfAnimation>();
		HashSet<SwfAnimationController> _controllers = new HashSet<SwfAnimationController>();

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

		public int AllAnimationCount {
			get { return _animations.Count; }
		}

		public void AddSwfAnimation(SwfAnimation animation) {
			_animations.Add(animation);
		}

		public void RemoveSwfAnimation(SwfAnimation animation) {
			_animations.Remove(animation);
		}

		public int AllAnimationControllerCount {
			get { return _controllers.Count; }
		}

		public void AddSwfAnimationController(SwfAnimationController controller) {
			_controllers.Add(controller);
		}

		public void RemoveSwfAnimationController(SwfAnimationController controller) {
			_controllers.Remove(controller);
		}

		// ---------------------------------------------------------------------
		//
		// Private
		//
		// ---------------------------------------------------------------------

		void GrabEnabledAnimations() {
			var all_animations = FindObjectsOfType<SwfAnimation>();
			for ( int i = 0, e = all_animations.Length; i < e; ++i ) {
				var animation = all_animations[i];
				if ( animation.enabled ) {
					_animations.Add(animation);
				}
			}
		}

		void GrabEnabledControllers() {
			var all_controllers = FindObjectsOfType<SwfAnimationController>();
			for ( int i = 0, e = all_controllers.Length; i < e; ++i ) {
				var controller = all_controllers[i];
				if ( controller.enabled ) {
					_controllers.Add(controller);
				}
			}
		}

		void DropAnimations() {
			_animations.Clear();
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
			GrabEnabledAnimations();
			GrabEnabledControllers();
		}

		void OnDisable() {
			DropAnimations();
			DropControllers();
		}

		void Update() {
			UpdateControllers();
		}
	}
}