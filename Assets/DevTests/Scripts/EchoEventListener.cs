using UnityEngine;

using FTRuntime;

namespace FTDevTests {
	public class EchoEventListener : MonoBehaviour {
		void Start () {
			var clip = GetComponent<SwfClip>();
			if ( clip ) {
				clip.OnChangeClipEvent += OnChangeClipEvent;
				clip.OnChangeSequenceEvent += OnChangeSequenceEvent;
				clip.OnChangeCurrentFrameEvent += OnChangeCurrentFrameEvent;
			}
			var ctrl = GetComponent<SwfClipController>();
			if ( ctrl ) {
				ctrl.OnPlayStoppedEvent += OnPlayStoppedEvent;
				ctrl.OnRewindPlayingEvent += OnRewindPlayingEvent;
				ctrl.OnStopPlayingEvent += OnStopPlayingEvent;
			}
		}

		void OnChangeClipEvent(SwfClip clip) {
			Debug.LogFormat(this, "OnChangeClipEvent: {0}", clip.clip);
		}

		void OnChangeSequenceEvent(SwfClip clip) {
			Debug.LogFormat(this, "OnChangeSequenceEvent: {0}", clip.sequence);
		}

		void OnChangeCurrentFrameEvent(SwfClip clip) {
			Debug.LogFormat(this, "OnChangeCurrentFrameEvent: {0}", clip.currentFrame);
		}

		void OnPlayStoppedEvent(SwfClipController ctrl) {
			Debug.LogFormat(this, "OnPlayStoppedEvent: {0}", ctrl.clip);
		}

		void OnRewindPlayingEvent(SwfClipController ctrl) {
			Debug.LogFormat(this, "OnRewindPlayingEvent: {0}", ctrl.clip);
		}

		void OnStopPlayingEvent(SwfClipController ctrl) {
			Debug.LogFormat(this, "OnStopPlayingEvent: {0}", ctrl.clip);
		}
	}
}