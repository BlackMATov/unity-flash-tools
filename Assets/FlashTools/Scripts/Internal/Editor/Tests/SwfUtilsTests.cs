using UnityEngine;
using NUnit.Framework;

namespace FlashTools.Internal.Tests {
	public static class SwfUtilsTests {

		static void AssertAreEqualVectors(Vector2 v0, Vector2 v1, float delta) {
			Assert.AreEqual(v0.x, v1.x, delta);
			Assert.AreEqual(v0.y, v1.y, delta);
		}

		static void AssertAreEqualVectors(Vector4 v0, Vector4 v1, float delta) {
			Assert.AreEqual(v0.x, v1.x, delta);
			Assert.AreEqual(v0.y, v1.y, delta);
			Assert.AreEqual(v0.z, v1.z, delta);
			Assert.AreEqual(v0.w, v1.w, delta);
		}

		//
		//
		//

		[Test]
		public static void PackUShortsToUIntTests() {
			ushort v0 = 11, v1 = 99;
			ushort o0, o1;
			SwfUtils.UnpackUShortsFromUInt(
				SwfUtils.PackUShortsToUInt(v0, v1), out o0, out o1);
			Assert.AreEqual(v0, o0);
			Assert.AreEqual(v1, o1);

			ushort v2 = 16789, v3 = 31234;
			ushort o2, o3;
			SwfUtils.UnpackUShortsFromUInt(
				SwfUtils.PackUShortsToUInt(v2, v3), out o2, out o3);
			Assert.AreEqual(v2, o2);
			Assert.AreEqual(v3, o3);
		}

		[Test]
		public static void PackUVTests() {
			var v0 = new Vector2(0.9999f, 0.1111f);
			float u0, u1;
			SwfUtils.UnpackUV(SwfUtils.PackUV(v0.x, v0.y), out u0, out u1);
			AssertAreEqualVectors(v0, new Vector2(u0, u1), SwfUtils.UVPrecision);

			var v1 = new Vector2(0.0987f, 0.0123f);
			float u2, u3;
			SwfUtils.UnpackUV(SwfUtils.PackUV(v1.x, v1.y), out u2, out u3);
			AssertAreEqualVectors(v1, new Vector2(u2, u3), SwfUtils.UVPrecision);

			var v2 = new Vector2(1.0f, 0.0f);
			float u4, u5;
			SwfUtils.UnpackUV(SwfUtils.PackUV(v2.x, v2.y), out u4, out u5);
			AssertAreEqualVectors(v2, new Vector2(u4, u5), SwfUtils.UVPrecision);
		}

		[Test]
		public static void PackFloatColorToUShortTests() {
			float v0 = -5.678f;
			Assert.AreEqual(
				v0,
				SwfUtils.UnpackFloatColorFromUShort(SwfUtils.PackFloatColorToUShort(v0)),
				SwfUtils.FColorPrecision);

			float v1 = 60.678f;
			Assert.AreEqual(
				v1,
				SwfUtils.UnpackFloatColorFromUShort(SwfUtils.PackFloatColorToUShort(v1)),
				SwfUtils.FColorPrecision);

			float v2 = 0.678f;
			Assert.AreEqual(
				v2,
				SwfUtils.UnpackFloatColorFromUShort(SwfUtils.PackFloatColorToUShort(v2)),
				SwfUtils.FColorPrecision);
		}

		[Test]
		public static void PackColorToUIntsTests() {
			var v0 = new Color(0.01f, 0.02f, 0.33f, 1.0f);
			uint u0, u1;
			SwfUtils.PackFColorToUInts(v0, out u0, out u1);
			Color c0;
			SwfUtils.UnpackFColorFromUInts(
				u0, u1,
				out c0.r, out c0.g, out c0.b, out c0.a);
			AssertAreEqualVectors(
				v0, c0, SwfUtils.FColorPrecision);

			var v1 = new Vector4(0.01f, 0.02f, 0.33f, 1.0f);
			uint u2, u3;
			SwfUtils.PackFColorToUInts(v1, out u2, out u3);
			Vector4 c1;
			SwfUtils.UnpackFColorFromUInts(
				u2, u3,
				out c1.x, out c1.y, out c1.z, out c1.w);
			AssertAreEqualVectors(
				v1, c1, SwfUtils.FColorPrecision);
		}
	}
}