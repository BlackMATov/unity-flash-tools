using System.Collections.Generic;
using FlashTools.Internal.SwfTools.SwfTypes;

namespace FlashTools.Internal.SwfTools {

	/*
	using SwfDisplayStack   = List<SwfDisplayInst>;
	using SwfDisplayLibrary = Dictionary<ushort, SwfDisplayDefine>;

	public enum SwfDisplayDefineType {
		Shape,
		Sprite
	}

	public enum SwfDisplayInstType {
		Shape,
		Sprite
	}

	//
	// SwfDisplayDefine
	//

	public interface SwfDisplayDefine {
		SwfDisplayDefineType Type { get; }
	}

	public struct SwfDisplayShapeDefine : SwfDisplayDefine {
		public ushort[]    Bitmaps;
		public SwfMatrix[] Matrices;
		public SwfDisplayDefineType Type {
			get { return SwfDisplayDefineType.Shape; }
		}
	}

	public struct SwfDisplaySpriteDefine : SwfDisplayDefine {
		public SwfControlTags ControlTags;
		public SwfDisplayDefineType Type {
			get { return SwfDisplayDefineType.Sprite; }
		}
	}

	//
	// SwfDisplayInst
	//

	public interface SwfDisplayInst {
		SwfDisplayInstType Type { get; }
	}

	public struct SwfDisplayShapeInst : SwfDisplayInst {
		public ushort            Id;
		public ushort            Depth;
		public SwfMatrix         Matrix;
		public SwfColorTransform ColorTransform;
		public SwfDisplayInstType Type {
			get { return SwfDisplayInstType.Shape; }
		}
	}

	public struct SwfDisplaySpriteInst : SwfDisplayInst {
		public ushort            Id;
		public ushort            Depth;
		public SwfMatrix         Matrix;
		public SwfColorTransform ColorTransform;
		public SwfDisplayInstType Type {
			get { return SwfDisplayInstType.Sprite; }
		}
	}

	//
	// SwfDisplayList
	//

	public class SwfDisplayList {
		public SwfDisplayStack   Stack     = new SwfDisplayStack();
		public SwfDisplayLibrary Library   = new SwfDisplayLibrary();
		public string            FrameName = string.Empty;
	}*/
}