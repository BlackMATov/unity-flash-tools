using UnityEngine;

using FTRuntime;

namespace FTDevTests {
	public class DeleteOnEnd : MonoBehaviour {
		void Start () {
			var clip = GetComponent<SwfClip>();
			if ( clip ) {
				clip.OnChangeCurrentFrameEvent += OnChangeCurrentFrameEvent;
			}
		}

		void OnDestroy() {
			Debug.Log("DeleteOnEnd::OnDestroy");
			var clip = GetComponent<SwfClip>();
			if ( clip ) {
				clip.OnChangeCurrentFrameEvent -= OnChangeCurrentFrameEvent;
			}
		}

		void OnChangeCurrentFrameEvent(SwfClip clip) {
			Debug.LogFormat("OnChangeCurrentFrameEvent: {0}", clip.currentFrame);
			if ( clip.currentFrame == clip.frameCount - 1 ) {
				Debug.Log("Delete");
				Destroy(gameObject);
				Debug.Log("After Delete");
			}
		}
	}
}