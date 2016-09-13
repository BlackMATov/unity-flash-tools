using UnityEngine;
using NUnit.Framework;

namespace FlashTools.Internal.Tests {
	public static class SwfUtilsTests {

		static void AssertAreEqualVectors(Vector2 v0, Vector2 v1, float delta) {
			Assert.AreEqual(v0.x, v1.x, delta);
			Assert.AreEqual(v0.y, v1.y, delta);
		}

		static void AssertAreEqualVectors(Vector3 v0, Vector3 v1, float delta) {
			Assert.AreEqual(v0.x, v1.x, delta);
			Assert.AreEqual(v0.y, v1.y, delta);
			Assert.AreEqual(v0.z, v1.z, delta);
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
			var v0 = new Vector2(0.99f, 0.11f);
			AssertAreEqualVectors(
				v0,
				SwfUtils.UnpackUV(SwfUtils.PackUV(v0)),
				SwfUtils.UVPrecision);

			var v1 = new Vector2(0.09f, 0.01f);
			AssertAreEqualVectors(
				v1,
				SwfUtils.UnpackUV(SwfUtils.PackUV(v1)),
				SwfUtils.UVPrecision);

			var v2 = new Vector2(1.0f, 0.0f);
			AssertAreEqualVectors(
				v2,
				SwfUtils.UnpackUV(SwfUtils.PackUV(v2)),
				SwfUtils.UVPrecision);
		}

		[Test]
		public static void PackFloatColorToUShortTests() {
			float v0 = -5.678f;
			Assert.AreEqual(
				v0,
				SwfUtils.UnpackFloatColorFromUShort(SwfUtils.PackFloatColorToUShort(v0)),
				SwfUtils.ColorPrecision);

			float v1 = 60.678f;
			Assert.AreEqual(
				v1,
				SwfUtils.UnpackFloatColorFromUShort(SwfUtils.PackFloatColorToUShort(v1)),
				SwfUtils.ColorPrecision);

			float v2 = 0.678f;
			Assert.AreEqual(
				v2,
				SwfUtils.UnpackFloatColorFromUShort(SwfUtils.PackFloatColorToUShort(v2)),
				SwfUtils.ColorPrecision);
		}

		[Test]
		public static void PackColorToUIntsTests() {
			var v0 = new Color(0.01f, 0.02f, 0.33f, 1.0f);
			uint u0, u1;
			SwfUtils.PackColorToUInts(v0, out u0, out u1);
			Color c0;
			SwfUtils.UnpackColorFromUInts(u0, u1, out c0);
			AssertAreEqualVectors(
				v0, c0, SwfUtils.ColorPrecision);

			var v1 = new Vector4(0.01f, 0.02f, 0.33f, 1.0f);
			uint u2, u3;
			SwfUtils.PackColorToUInts(v1, out u2, out u3);
			Vector4 c1;
			SwfUtils.UnpackColorFromUInts(u2, u3, out c1);
			AssertAreEqualVectors(
				v1, c1, SwfUtils.ColorPrecision);
		}

		[Test]
		public static void PackCoordsToUIntTests() {
			var v0 = new Vector2(1.14f, 2.23f);
			AssertAreEqualVectors(
				v0,
				SwfUtils.UnpackCoordsFromUInt(SwfUtils.PackCoordsToUInt(v0)),
				SwfUtils.CoordPrecision);

			var v1 = new Vector2(1234.14f, 3200.23f);
			AssertAreEqualVectors(
				v1,
				SwfUtils.UnpackCoordsFromUInt(SwfUtils.PackCoordsToUInt(v1)),
				SwfUtils.CoordPrecision);
		}
	}
}