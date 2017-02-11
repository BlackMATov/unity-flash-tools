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
				ctrl.Play(_fadeInSequence);
				yield return new SwfWaitStopPlaying(ctrl);

				for ( var i = 0; i < 3; ++i ) {
					var last_seq = ctrl.clip.sequence;
					do {
						var seq_index = Random.Range(0, _idleSequences.Length);
						ctrl.Play(_idleSequences[seq_index]);
					} while ( last_seq == ctrl.clip.sequence );
					yield return new SwfWaitStopPlaying(ctrl);
				}

				ctrl.Play(_fadeOutSequence);
				yield return new SwfWaitStopPlaying(ctrl);
				yield return new WaitForSeconds(2.0f);
			}
		}
	}
}