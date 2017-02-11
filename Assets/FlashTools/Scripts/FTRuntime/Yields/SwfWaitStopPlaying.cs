using System.Collections;

namespace FTRuntime.Yields {
	public class SwfWaitStopPlaying : IEnumerator {
		SwfClipController _waitCtrl;

		public SwfWaitStopPlaying(SwfClipController ctrl) {
			Subscribe(ctrl);
		}

		public SwfWaitStopPlaying Reuse(SwfClipController ctrl) {
			return Subscribe(ctrl);
		}

		//
		// Private
		//

		SwfWaitStopPlaying Subscribe(SwfClipController ctrl) {
			(this as IEnumerator).Reset();
			if ( ctrl ) {
				_waitCtrl = ctrl;
				ctrl.OnStopPlayingEvent += OnStopPlaying;
			}
			return this;
		}

		void OnStopPlaying(SwfClipController ctrl) {
			(this as IEnumerator).Reset();
		}

		//
		// IEnumerator
		//

		bool IEnumerator.MoveNext() {
			return _waitCtrl != null;
		}

		void IEnumerator.Reset() {
			if ( _waitCtrl != null ) {
				_waitCtrl.OnStopPlayingEvent -= OnStopPlaying;
				_waitCtrl = null;
			}
		}

		object IEnumerator.Current {
			get { return null; }
		}
	}
}