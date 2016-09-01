using UnityEngine;
using FlashTools.Internal;

namespace FlashTools {
	[ExecuteInEditMode, DisallowMultipleComponent]
	[RequireComponent(typeof(SwfClip))]
	public class SwfClipController : MonoBehaviour {

		SwfClip _clip  = null;
		float   _timer = 0.0f;

		// ---------------------------------------------------------------------
		//
		// Events
		//
		// ---------------------------------------------------------------------

		public event System.Action<SwfClipController> OnStopPlayingEvent;
		public event System.Action<SwfClipController> OnRewindPlayingEvent;

		public event System.Action<SwfClipController> OnPausePlayingEvent;
		public event System.Action<SwfClipController> OnResumePausedEvent;

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

		States _state = States.Stopped;
		public States state {
			get { return _state; }
		}

		public bool isStopped {
			get { return state == States.Stopped; }
		}

		public bool isPaused {
			get { return state == States.Paused; }
		}

		public bool isPlaying {
			get { return state == States.Playing; }
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
			_timer = 0.0f;
			_state = States.Stopped;
			if ( rewind ) {
				Rewind();
			}
			if ( is_playing && OnStopPlayingEvent != null ) {
				OnStopPlayingEvent(this);
			}
		}

		public void Pause() {
			if ( isPlaying ) {
				_state = States.Paused;
				if ( OnPausePlayingEvent != null ) {
					OnPausePlayingEvent(this);
				}
			}
		}

		public void Resume() {
			if ( isPaused ) {
				_state = States.Playing;
				if ( OnResumePausedEvent != null ) {
					OnResumePausedEvent(this);
				}
			}
		}

		public void Play() {
			Rewind();
			_timer = 0.0f;
			_state = States.Playing;
		}

		public void Rewind() {
			switch ( playMode ) {
			case PlayModes.Forward:
				if ( _clip ) {
					_clip.ToBeginFrame();
				}
				break;
			case PlayModes.Backward:
				if ( _clip ) {
					_clip.ToEndFrame();
				}
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
				UpdateTimer(dt);
			}
		}

		void UpdateTimer(float dt) {
			var frame_rate = _clip ? _clip.frameRate : 1.0f;
			_timer += frame_rate * rateScale * dt;
			while ( _timer > 1.0f ) {
				_timer -= 1.0f;
				TimerTick();
			}
		}

		void TimerTick() {
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
				return _clip ? _clip.ToNextFrame() : false;
			case PlayModes.Backward:
				return _clip ? _clip.ToPrevFrame() : false;
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