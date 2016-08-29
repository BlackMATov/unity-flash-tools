using UnityEngine;
using FlashTools.Internal;
using System;

namespace FlashTools {
	[ExecuteInEditMode, DisallowMultipleComponent]
	[RequireComponent(typeof(SwfAnimation))]
	public class SwfAnimationController : MonoBehaviour {

		SwfAnimation _animation  = null;
		float        _frameTimer = 0.0f;

		// ---------------------------------------------------------------------
		//
		// Events
		//
		// ---------------------------------------------------------------------

		public event Action<SwfAnimationController> OnStopPlayingEvent;
		public event Action<SwfAnimationController> OnRewindPlayingEvent;

		public event Action<SwfAnimationController> OnPausePlayingEvent;
		public event Action<SwfAnimationController> OnResumePausedEvent;

		// ---------------------------------------------------------------------
		//
		// Properties
		//
		// ---------------------------------------------------------------------

		public enum PlayModes {
			Forward,
			Backward
		}

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
		[SwfFloatRange(0.0f, float.MaxValue)]
		float _rateScale = 1.0f;
		public float rateScale {
			get { return _rateScale; }
			set { _rateScale = Mathf.Clamp(value, 0.0f, float.MaxValue); }
		}

		[SerializeField]
		PlayModes _playMode = PlayModes.Forward;
		public PlayModes playMode {
			get { return _playMode; }
			set { _playMode = value; }
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

		public bool isStopped {
			get { return currentState == States.Stopped; }
		}

		public bool isPaused {
			get { return currentState == States.Paused; }
		}

		public bool isPlaying {
			get { return currentState == States.Playing; }
		}

		// ---------------------------------------------------------------------
		//
		// Functions
		//
		// ---------------------------------------------------------------------

		public void Stop() {
			Stop(true);
		}

		public void Stop(bool rewind) {
			var is_playing = isPlaying;
			_frameTimer = 0.0f;
			_currentState = States.Stopped;
			if ( rewind ) {
				Rewind();
			}
			if ( is_playing && OnStopPlayingEvent != null ) {
				OnStopPlayingEvent(this);
			}
		}

		public void Pause() {
			if ( isPlaying ) {
				_currentState = States.Paused;
				if ( OnPausePlayingEvent != null ) {
					OnPausePlayingEvent(this);
				}
			}
		}

		public void Resume() {
			if ( isPaused ) {
				_currentState = States.Playing;
				if ( OnResumePausedEvent != null ) {
					OnResumePausedEvent(this);
				}
			}
		}

		public void Play() {
			Rewind();
			_frameTimer = 0.0f;
			_currentState = States.Playing;
		}

		public void Rewind() {
			switch ( playMode ) {
			case PlayModes.Forward:
				_animation.ToBeginFrame();
				break;
			case PlayModes.Backward:
				_animation.ToEndFrame();
				break;
			default:
				throw new UnityException(string.Format(
					"SwfAnimationController. Incorrect play mode: {0}",
					playMode));
			}
			if ( isPlaying && OnRewindPlayingEvent != null ) {
				OnRewindPlayingEvent(this);
			}
		}

		// ---------------------------------------------------------------------
		//
		// Internal
		//
		// ---------------------------------------------------------------------

		public void InternalUpdate(float dt) {
			if ( isPlaying ) {
				UpdateAnimationTimer(dt);
			}
		}

		void UpdateAnimationTimer(float dt) {
			_frameTimer += _animation.frameRate * rateScale * dt;
			while ( _frameTimer > 1.0f ) {
				_frameTimer -= 1.0f;
				AnimationTimerTick();
			}
		}

		void AnimationTimerTick() {
			if ( !NextAnimationFrame() ) {
				switch ( loopMode ) {
				case LoopModes.Once:
					Stop(false);
					break;
				case LoopModes.Loop:
					Rewind();
					break;
				default:
					throw new UnityException(string.Format(
						"SwfAnimationController. Incorrect loop mode: {0}",
						loopMode));
				}
			}
		}

		bool NextAnimationFrame() {
			switch ( playMode ) {
			case PlayModes.Forward:
				return _animation.ToNextFrame();
			case PlayModes.Backward:
				return _animation.ToPrevFrame();
			default:
				throw new UnityException(string.Format(
					"SwfAnimationController. Incorrect play mode: {0}",
					playMode));
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