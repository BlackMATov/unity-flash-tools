using UnityEngine;
using System.Collections.Generic;

using FTRuntime;

namespace FlashTools.Examples {
	[RequireComponent(typeof(SwfClipController))]
	public class PurpleFlowerLogic : MonoBehaviour {
		static string[] _idleSequences    = new string[]{"talk", "idle0", "idle1", "idle2"};
		static string   _fadeInSequence   = "fadeIn";
		static string   _fadeOutSequence  = "fadeOut";

		enum States {
			FadeIn,
			Idle,
			FadeOut
		}

		float  _idleTimer    = 0.0f;
		States _currentState = States.FadeIn;

		void Start() {
			var ctrl = GetComponent<SwfClipController>();
			ctrl.OnStopPlayingEvent += OnStopPlayingEvent;
			ctrl.loopMode = SwfClipController.LoopModes.Once;

			_currentState = States.FadeIn;
			ctrl.Play(_fadeInSequence);
		}

		void OnStopPlayingEvent(SwfClipController ctrl) {
			switch ( _currentState ) {
			case States.FadeIn:
				_idleTimer    = Time.time;
				_currentState = States.Idle;
				ctrl.Play(_idleSequences[Random.Range(0, _idleSequences.Length)]);
				break;
			case States.Idle: {
					if ( Time.time - _idleTimer > 5.0f ) {
						_currentState = States.FadeOut;
						ctrl.Play(_fadeOutSequence);
					} else {
						var last_seq = ctrl.clip.sequence;
						do {
							var seq_index = Random.Range(0, _idleSequences.Length);
							ctrl.Play(_idleSequences[seq_index]);
						} while ( last_seq == ctrl.clip.sequence );
					}
				}
				break;
			case States.FadeOut:
				_currentState = States.FadeIn;
				ctrl.Play(_fadeInSequence);
				break;
			}
		}
	}
}