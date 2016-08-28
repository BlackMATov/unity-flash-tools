using UnityEngine;

namespace FlashTools {
	[ExecuteInEditMode, DisallowMultipleComponent]
	[RequireComponent(typeof(SwfAnimation))]
	public class SwfAnimationController : MonoBehaviour {

		SwfAnimation _animation  = null;
		float        _frameTimer = 0.0f;

		// ---------------------------------------------------------------------
		//
		// Properties
		//
		// ---------------------------------------------------------------------

		public enum LoopModes {
			Once,
			Loop
		}

		public enum States {
			Stopped,
			Paused,
			Playing
		}

		[SerializeField]
		bool _autoPlay = false;
		public bool autoPlay {
			get { return _autoPlay; }
			set { _autoPlay = value; }
		}

		[SerializeField]
		LoopModes _loopMode = LoopModes.Once;
		public LoopModes loopMode {
			get { return _loopMode; }
			set { _loopMode = value; }
		}

		States _currentState = States.Stopped;
		public States currentState {
			get { return _currentState; }
		}

		// ---------------------------------------------------------------------
		//
		// Functions
		//
		// ---------------------------------------------------------------------

		public void Stop() {
			_frameTimer = 0.0f;
			_currentState = States.Stopped;
			_animation.currentFrame = 0;
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
			_currentState = States.Playing;
			_animation.currentFrame = 0;
		}

		// ---------------------------------------------------------------------
		//
		// Internal
		//
		// ---------------------------------------------------------------------

		public void InternalUpdate(float dt) {
			if ( currentState == States.Playing ) {
				_frameTimer += _animation.frameRate * dt;
				if ( _frameTimer > 1.0f ) {
					while ( _frameTimer > 1.0f ) {
						_frameTimer -= 1.0f;
						if ( !_animation.ToNextFrame() ) {
							switch ( loopMode ) {
							case LoopModes.Once:
								_currentState = States.Stopped;
								break;
							case LoopModes.Loop:
								_animation.ToBeginFrame();
								break;
							default:
								throw new UnityException(string.Format(
									"SwfAnimationController. Incorrect loop mode: {0}",
									loopMode));
							}
						}
					}
				}
			}
		}

		// ---------------------------------------------------------------------
		//
		// Messages
		//
		// ---------------------------------------------------------------------

		void Awake() {
			_animation = GetComponent<SwfAnimation>();
			if ( autoPlay ) {
				Play();
			}
		}

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