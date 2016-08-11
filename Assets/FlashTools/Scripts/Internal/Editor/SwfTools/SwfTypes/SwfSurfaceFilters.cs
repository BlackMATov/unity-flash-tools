using UnityEngine;
using System.Collections.Generic;

namespace FlashTools.Internal.SwfTools.SwfTypes {
	public struct SwfSurfaceFilters {
		public enum FilterType {
			DropShadow,
			Blur,
			Glow,
			Bevel,
			GradientGlow,
			Convolution,
			ColorMatrix,
			GradientBevel
		}

		public abstract class Filter {
			public abstract FilterType Type { get; }
		}

		public class DropShadowFilter : Filter {
			public override FilterType Type {
				get { return FilterType.DropShadow; }
			}
		}

		public class BlurFilter : Filter {
			public override FilterType Type {
				get { return FilterType.Blur; }
			}
		}

		public class GlowFilter : Filter {
			public override FilterType Type {
				get { return FilterType.Glow; }
			}
			public SwfColor GlowColor;
			public float    BlurX;
			public float    BlurY;
			public float    Strength;
			public bool     InnerGlow;
			public bool     Knockout;
			public bool     CompositeSource;
			public uint     Passes;
		}

		public class BevelFilter : Filter {
			public override FilterType Type {
				get { return FilterType.Bevel; }
			}
		}

		public class GradientGlowFilter : Filter {
			public override FilterType Type {
				get { return FilterType.GradientGlow; }
			}
		}

		public class ConvolutionFilter : Filter {
			public override FilterType Type {
				get { return FilterType.Convolution; }
			}
		}

		public class ColorMatrixFilter : Filter {
			public override FilterType Type {
				get { return FilterType.ColorMatrix; }
			}
		}

		public class GradientBevelFilter : Filter {
			public override FilterType Type {
				get { return FilterType.GradientBevel; }
			}
		}

		public List<Filter> Filters;

		public static SwfSurfaceFilters identity {
			get {
				return new SwfSurfaceFilters{
					Filters = new List<Filter>()
				};
			}
		}

		public static SwfSurfaceFilters Read(SwfStreamReader reader) {
			var surface_filters = SwfSurfaceFilters.identity;
			byte count = reader.ReadByte();
			for ( var i = 0; i < count; ++i ) {
				surface_filters.Filters.Add(ReadFilter(reader));
			}
			return surface_filters;
		}

		public override string ToString() {
			return string.Format(
				"SwfSurfaceFilters. Filters: {0}",
				Filters.Count);
		}

		// ------------------------------------------------------------------------
		//
		// ReadFilters
		//
		// ------------------------------------------------------------------------

		static Filter ReadFilter(SwfStreamReader reader) {
			var type_id = reader.ReadByte();
			return CreateFilterFromTypeId(type_id, reader);
		}

		static Filter CreateFilterFromTypeId(byte type_id, SwfStreamReader reader) {
			switch ( type_id ) {
			case 0: return ReadConcreteFilter(new DropShadowFilter   (), reader);
			case 1: return ReadConcreteFilter(new BlurFilter         (), reader);
			case 2: return ReadConcreteFilter(new GlowFilter         (), reader);
			case 3: return ReadConcreteFilter(new BevelFilter        (), reader);
			case 4: return ReadConcreteFilter(new GradientGlowFilter (), reader);
			case 5: return ReadConcreteFilter(new ConvolutionFilter  (), reader);
			case 6: return ReadConcreteFilter(new ColorMatrixFilter  (), reader);
			case 7: return ReadConcreteFilter(new GradientBevelFilter(), reader);
			default:
				throw new UnityException(string.Format(
					"Incorrect surface filter type id: {0}", type_id));
			}
		}

		static Filter ReadConcreteFilter(DropShadowFilter filter, SwfStreamReader reader) {
			//TODO: IMPLME
			throw new UnityException(string.Format(
				"Unsupported surface filter type: {0}", filter.Type));
		}

		static Filter ReadConcreteFilter(BlurFilter filter, SwfStreamReader reader) {
			//TODO: IMPLME
			throw new UnityException(string.Format(
				"Unsupported surface filter type: {0}", filter.Type));
		}

		static Filter ReadConcreteFilter(GlowFilter filter, SwfStreamReader reader) {
			filter.GlowColor       = SwfColor.Read(reader, true);
			filter.BlurX           = reader.ReadFixedPoint_16_16();
			filter.BlurY           = reader.ReadFixedPoint_16_16();
			filter.Strength        = reader.ReadFixedPoint_8_8();
			filter.InnerGlow       = reader.ReadBit();
			filter.Knockout        = reader.ReadBit();
			filter.CompositeSource = reader.ReadBit();
			filter.Passes          = reader.ReadUnsignedBits(5);
			return filter;
		}

		static Filter ReadConcreteFilter(BevelFilter filter, SwfStreamReader reader) {
			//TODO: IMPLME
			throw new UnityException(string.Format(
				"Unsupported surface filter type: {0}", filter.Type));
		}

		static Filter ReadConcreteFilter(GradientGlowFilter filter, SwfStreamReader reader) {
			//TODO: IMPLME
			throw new UnityException(string.Format(
				"Unsupported surface filter type: {0}", filter.Type));
		}

		static Filter ReadConcreteFilter(ConvolutionFilter filter, SwfStreamReader reader) {
			//TODO: IMPLME
			throw new UnityException(string.Format(
				"Unsupported surface filter type: {0}", filter.Type));
		}

		static Filter ReadConcreteFilter(ColorMatrixFilter filter, SwfStreamReader reader) {
			//TODO: IMPLME
			throw new UnityException(string.Format(
				"Unsupported surface filter type: {0}", filter.Type));
		}

		static Filter ReadConcreteFilter(GradientBevelFilter filter, SwfStreamReader reader) {
			//TODO: IMPLME
			throw new UnityException(string.Format(
				"Unsupported surface filter type: {0}", filter.Type));
		}
	}
}