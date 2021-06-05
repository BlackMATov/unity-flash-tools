using UnityEngine;
using System.Collections;

using FTRuntime;
using FTRuntime.Yields;

namespace FlashTools.Examples {
	[RequireComponent(typeof(SwfClipController))]
	public class PurpleFlowerLogic : MonoBehaviour {
		static string[] _idleSequences    = {"talk", "idle0", "idle1", "idle2"};
		static string   _fadeInSequence   = "fadeIn";
		static string   _fadeOutSequence  = "fadeOut";

		void Start() {
			var ctrl = GetComponent<SwfClipController>();
			StartCoroutine(StartCoro(ctrl));
		}

		IEnumerator StartCoro(SwfClipController ctrl) {
			while ( true ) {
				yield return ctrl.PlayAndWaitStopOrRewind(_fadeInSequence);
				for ( var i = 0; i < 3; ++i ) {
					var idle_seq = GetRandomIdleSequence(ctrl);
					yield return ctrl.PlayAndWaitStopOrRewind(idle_seq);
				}
				yield return ctrl.PlayAndWaitStopOrRewind(_fadeOutSequence);
				yield return new WaitForSeconds(2.0f);
			}
		}

		string GetRandomIdleSequence(SwfClipController ctrl) {
			var cur_seq = ctrl.clip.sequence;
			do {
				var seq_index    = Random.Range(0, _idleSequences.Length);
				var new_sequence = _idleSequences[seq_index];
				if ( new_sequence != cur_seq ) {
					return new_sequence;
				}
			} while ( true );
		}
	}
}