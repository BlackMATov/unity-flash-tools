using System.Collections;

namespace FTRuntime.Yields {
	public class SwfWaitPlayStopped : IEnumerator {
		SwfClipController _waitCtrl;

		public SwfWaitPlayStopped(SwfClipController ctrl) {
			Subscribe(ctrl);
		}

		public SwfWaitPlayStopped Reuse(SwfClipController ctrl) {
			return Subscribe(ctrl);
		}

		//
		// Private
		//

		SwfWaitPlayStopped Subscribe(SwfClipController ctrl) {
			(this as IEnumerator).Reset();
			if ( ctrl ) {
				_waitCtrl = ctrl;
				ctrl.OnPlayStoppedEvent += OnPlayStopped;
			}
			return this;
		}

		void OnPlayStopped(SwfClipController ctrl) {
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
				_waitCtrl.OnPlayStoppedEvent -= OnPlayStopped;
				_waitCtrl = null;
			}
		}

		object IEnumerator.Current {
			get { return null; }
		}
	}
}