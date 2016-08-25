using UnityEngine;
using System.Collections.Generic;

namespace FlashTools {
	[ExecuteInEditMode]
	public class SwfManager : MonoBehaviour {
		// ---------------------------------------------------------------------
		//
		// Properties
		//
		// ---------------------------------------------------------------------

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
		// Messages
		//
		// ---------------------------------------------------------------------

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