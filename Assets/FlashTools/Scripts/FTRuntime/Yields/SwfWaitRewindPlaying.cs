using System.Collections;

namespace FTRuntime.Yields {
	public class SwfWaitRewindPlaying : IEnumerator {
		SwfClipController _waitCtrl;

		public SwfWaitRewindPlaying(SwfClipController ctrl) {
			Subscribe(ctrl);
		}

		public SwfWaitRewindPlaying Reuse(SwfClipController ctrl) {
			return Subscribe(ctrl);
		}

		//
		// Private
		//

		SwfWaitRewindPlaying Subscribe(SwfClipController ctrl) {
			(this as IEnumerator).Reset();
			if ( ctrl ) {
				_waitCtrl = ctrl;
				ctrl.OnRewindPlayingEvent += OnRewindPlaying;
			}
			return this;
		}

		void OnRewindPlaying(SwfClipController ctrl) {
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
				_waitCtrl.OnRewindPlayingEvent -= OnRewindPlaying;
				_waitCtrl = null;
			}
		}

		object IEnumerator.Current {
			get { return null; }
		}
	}
}