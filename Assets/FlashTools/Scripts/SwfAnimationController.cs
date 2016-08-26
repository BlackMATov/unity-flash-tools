using UnityEngine;

namespace FlashTools {
	[ExecuteInEditMode, DisallowMultipleComponent]
	[RequireComponent(typeof(SwfAnimation))]
	public class SwfAnimationController : MonoBehaviour {
		public enum LoopModes {
			Once,
			Loop
		}

		public enum States {
			Stopped,
			Paused,
			Playing
		}

		//
		//
		//

		public bool      AutoPlay      = false;
		public LoopModes LoopMode      = LoopModes.Once;

		//
		//
		//

		SwfAnimation     _animation    = null;
		float            _frameTimer   = 0.0f;
		States           _currentState = States.Stopped;

		// ---------------------------------------------------------------------
		//
		// Internal
		//
		// ---------------------------------------------------------------------

		public void InitWithAsset() {
			_animation = GetComponent<SwfAnimation>();
			if ( AutoPlay ) {
				Play();
			}
		}

		public void InternalUpdate(float dt) {
			if ( !_animation ) {
				return;
			}
			if ( currentState == States.Playing ) {
				var frame_rate    = _animation.frameRate;
				var frame_count   = _animation.frameCount;
				var current_frame = _animation.currentFrame;
				_frameTimer += frame_rate * dt;
				if ( _frameTimer > 1.0f ) {
					while ( _frameTimer > 1.0f ) {
						_frameTimer -= 1.0f;
						++current_frame;
						if ( current_frame > frame_count - 1 ) {
							switch ( LoopMode ) {
							case LoopModes.Once:
								current_frame = frame_count > 0 ? frame_count - 1 : 0;
								_currentState = States.Stopped;
								break;
							case LoopModes.Loop:
								current_frame = 0;
								break;
							default:
								throw new UnityException(string.Format(
									"SwfAnimationController. Incorrect loop mode: {0}",
									LoopMode));
							}
						}
					}
					_animation.currentFrame = current_frame;
				}
			}
		}

		// ---------------------------------------------------------------------
		//
		// Functions
		//
		// ---------------------------------------------------------------------

		public void Stop() {
			_frameTimer = 0.0f;
			_animation.currentFrame = 0;
			_currentState = States.Stopped;
		}

		public void Pause() {
			if ( currentState == States.Playing ) {
				_currentState = States.Paused;
			}
		}

		public void Resume() {
			if ( currentState == States.Paused ) {
				_currentState = States.Playing;
			}
		}

		public void Play() {
			_frameTimer = 0.0f;
			_animation.currentFrame = 0;
			_currentState = States.Playing;
		}

		public bool isStopped {
			get { return currentState == States.Stopped; }
		}

		public bool isPaused {
			get { return currentState == States.Paused; }
		}

		public bool isPlaying {
			get { return currentState == States.Playing; }
		}

		public States currentState {
			get { return _currentState; }
		}

		// ---------------------------------------------------------------------
		//
		// Messages
		//
		// ---------------------------------------------------------------------

		void OnEnable() {
			var swf_manager = SwfManager.GetInstance(true);
			if ( swf_manager ) {
				swf_manager.AddSwfAnimationController(this);
			}
		}

		void OnDisable() {
			var swf_manager = SwfManager.GetInstance(false);
			if ( swf_manager ) {
				swf_manager.RemoveSwfAnimationController(this);
			}
		}
	}
}