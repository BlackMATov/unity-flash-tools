using UnityEngine;
using FlashTools.Internal;

namespace FlashTools {
	[ExecuteInEditMode, DisallowMultipleComponent]
	[RequireComponent(typeof(SwfClip))]
	public class SwfClipController : MonoBehaviour {

		SwfClip _clip      = null;
		bool    _isPlaying = false;
		float   _tickTimer = 0.0f;

		// ---------------------------------------------------------------------
		//
		// Events
		//
		// ---------------------------------------------------------------------

		public event System.Action<SwfClipController> OnStopPlayingEvent;
		public event System.Action<SwfClipController> OnPlayStoppedEvent;
		public event System.Action<SwfClipController> OnRewindPlayingEvent;

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

		public SwfClip clip {
			get { return _clip; }
		}

		public bool isPlaying {
			get { return _isPlaying; }
		}

		public bool isStopped {
			get { return !isPlaying; }
		}

		// ---------------------------------------------------------------------
		//
		// Functions
		//
		// ---------------------------------------------------------------------

		public void GotoAndStop(int frame) {
			if ( clip ) {
				clip.currentFrame = frame;
			}
			Stop(false);
		}

		public void GotoAndStop(string sequence, int frame) {
			if ( clip ) {
				clip.sequence = sequence;
			}
			GotoAndStop(frame);
		}

		//
		//
		//

		public void GotoAndPlay(int frame) {
			if ( clip ) {
				clip.currentFrame = frame;
			}
			Play(false);
		}

		public void GotoAndPlay(string sequence, int frame) {
			if ( clip ) {
				clip.sequence = sequence;
			}
			GotoAndPlay(frame);
		}

		//
		//
		//

		public void Stop(bool rewind) {
			var is_playing = isPlaying;
			if ( is_playing ) {
				_isPlaying = false;
				_tickTimer = 0.0f;
			}
			if ( rewind ) {
				Rewind();
			}
			if ( is_playing && OnStopPlayingEvent != null ) {
				OnStopPlayingEvent(this);
			}
		}

		public void Stop(string sequence) {
			if ( clip ) {
				clip.sequence = sequence;
			}
			Stop(true);
		}

		//
		//
		//

		public void Play(bool rewind) {
			var is_stopped = isStopped;
			if ( is_stopped ) {
				_isPlaying = true;
				_tickTimer = 0.0f;
			}
			if ( rewind ) {
				Rewind();
			}
			if ( is_stopped && OnPlayStoppedEvent != null ) {
				OnPlayStoppedEvent(this);
			}
		}

		public void Play(string sequence) {
			if ( clip ) {
				clip.sequence = sequence;
			}
			Play(true);
		}

		//
		//
		//

		public void Rewind() {
			switch ( playMode ) {
			case PlayModes.Forward:
				if ( clip ) {
					clip.ToBeginFrame();
				}
				break;
			case PlayModes.Backward:
				if ( clip ) {
					clip.ToEndFrame();
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
			var frame_rate = clip ? clip.frameRate : 1.0f;
			_tickTimer += frame_rate * rateScale * dt;
			while ( _tickTimer > 1.0f ) {
				_tickTimer -= 1.0f;
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
				return clip ? clip.ToNextFrame() : false;
			case PlayModes.Backward:
				return clip ? clip.ToPrevFrame() : false;
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
			if ( autoPlay && Application.isPlaying ) {
				Play(false);
			}
		}

		void OnEnable() {
			var swf_manager = SwfManager.GetInstance(true);
			if ( swf_manager ) {
				swf_manager.AddController(this);
			}
		}

		void OnDisable() {
			var swf_manager = SwfManager.GetInstance(false);
			if ( swf_manager ) {
				swf_manager.RemoveController(this);
			}
		}
	}
}