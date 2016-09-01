using UnityEngine;
using FlashTools.Internal;
using System;

namespace FlashTools {
	[ExecuteInEditMode, DisallowMultipleComponent]
	[RequireComponent(typeof(SwfClip))]
	public class SwfClipController : MonoBehaviour {

		SwfClip _clip       = null;
		float   _frameTimer = 0.0f;

		// ---------------------------------------------------------------------
		//
		// Events
		//
		// ---------------------------------------------------------------------

		public event Action<SwfClipController> OnStopPlayingEvent;
		public event Action<SwfClipController> OnRewindPlayingEvent;

		public event Action<SwfClipController> OnPausePlayingEvent;
		public event Action<SwfClipController> OnResumePausedEvent;

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
				_clip.ToBeginFrame();
				break;
			case PlayModes.Backward:
				_clip.ToEndFrame();
				break;
			default:
				throw new UnityException(string.Format(
					"SwfClipController. Incorrect play mode: {0}",
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
				UpdateFrameTimer(dt);
			}
		}

		void UpdateFrameTimer(float dt) {
			_frameTimer += _clip.frameRate * rateScale * dt;
			while ( _frameTimer > 1.0f ) {
				_frameTimer -= 1.0f;
				FrameTimerTick();
			}
		}

		void FrameTimerTick() {
			if ( !NextClipFrame() ) {
				switch ( loopMode ) {
				case LoopModes.Once:
					Stop(false);
					break;
				case LoopModes.Loop:
					Rewind();
					break;
				default:
					throw new UnityException(string.Format(
						"SwfClipController. Incorrect loop mode: {0}",
						loopMode));
				}
			}
		}

		bool NextClipFrame() {
			switch ( playMode ) {
			case PlayModes.Forward:
				return _clip.ToNextFrame();
			case PlayModes.Backward:
				return _clip.ToPrevFrame();
			default:
				throw new UnityException(string.Format(
					"SwfClipController. Incorrect play mode: {0}",
					playMode));
			}
		}

		// ---------------------------------------------------------------------
		//
		// Messages
		//
		// ---------------------------------------------------------------------

		void Awake() {
			_clip = GetComponent<SwfClip>();
			if ( autoPlay ) {
				Play();
			}
		}

		void OnEnable() {
			var swf_manager = SwfManager.GetInstance(true);
			if ( swf_manager ) {
				swf_manager.AddSwfClipController(this);
			}
		}

		void OnDisable() {
			var swf_manager = SwfManager.GetInstance(false);
			if ( swf_manager ) {
				swf_manager.RemoveSwfClipController(this);
			}
		}
	}
}