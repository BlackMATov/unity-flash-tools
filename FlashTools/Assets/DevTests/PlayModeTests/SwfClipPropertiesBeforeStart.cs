using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

using FTRuntime;

public class SwfClipPropertiesBeforeStart
{
	[Test]
	public void SwfClipPropertiesBeforeStartTest() {
		var animation_prefab = Resources.Load<GameObject>("Animations/npc_chicken.fla_export/npc_chicken.fla.Chicken");
		Assert.True(animation_prefab);

		var animation_instance = GameObject.Instantiate(animation_prefab);
		Assert.True(animation_instance);

		var animation_clip = animation_instance.GetComponent<SwfClip>();
		Assert.True(animation_clip);

		Assert.True(animation_clip.sequence == "walk");
		Assert.True(animation_clip.currentFrame == 0);
		Assert.True(animation_clip.frameCount == 24);
		Assert.True(animation_clip.frameRate == 30);

		GameObject.Destroy(animation_instance);
	}
}
