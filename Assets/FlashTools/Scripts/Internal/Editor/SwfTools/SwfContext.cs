using System.Collections.Generic;
using FlashTools.Internal.SwfTools.SwfTypes;

namespace FlashTools.Internal.SwfTools {

	//
	// SwfDisplayList
	//

	public enum SwfDisplayInstType {
		Shape,
		Sprite
	}

	public abstract class SwfDisplayInst {
		public abstract SwfDisplayInstType Type { get; }

		public ushort            Id;
		public ushort            Depth;
		public SwfMatrix         Matrix;
		public SwfColorTransform ColorTransform;
	}

	public class SwfDisplayShapeInst : SwfDisplayInst {
		public override SwfDisplayInstType Type {
			get { return SwfDisplayInstType.Shape; }
		}
	}

	public class SwfDisplaySpriteInst : SwfDisplayInst {
		public int            CurrentTag  = 0;
		public SwfDisplayList DisplayList = new SwfDisplayList();
		public override SwfDisplayInstType Type {
			get { return SwfDisplayInstType.Sprite; }
		}
	}

	public class SwfDisplayList {
		public string FrameName = string.Empty;
		public SortedDictionary<ushort, SwfDisplayInst> Insts =
			new SortedDictionary<ushort, SwfDisplayInst>();
	}

	//
	// SwfLibrary
	//

	public enum SwfLibraryDefineType {
		Shape,
		Bitmap,
		Sprite
	}

	public abstract class SwfLibraryDefine {
		public abstract SwfLibraryDefineType Type { get; }
	}

	public class SwfLibraryShapeDefine : SwfLibraryDefine {
		public ushort[]    Bitmaps  = new ushort[0];
		public SwfMatrix[] Matrices = new SwfMatrix[0];
		public override SwfLibraryDefineType Type {
			get { return SwfLibraryDefineType.Shape; }
		}
	}

	public class SwfLibraryBitmapDefine : SwfLibraryDefine {
		public override SwfLibraryDefineType Type {
			get { return SwfLibraryDefineType.Bitmap; }
		}
	}

	public class SwfLibrarySpriteDefine : SwfLibraryDefine {
		public SwfControlTags ControlTags = new SwfControlTags();
		public override SwfLibraryDefineType Type {
			get { return SwfLibraryDefineType.Sprite; }
		}
	}

	public class SwfLibrary {
		public SortedDictionary<ushort, SwfLibraryDefine> Defines =
			new SortedDictionary<ushort, SwfLibraryDefine>();

		public bool HasDefine<T>(ushort define_id) where T : SwfLibraryDefine {
			SwfLibraryDefine def;
			if ( Defines.TryGetValue(define_id, out def) ) {
				return (def as T) != null;
			}
			return false;
		}

		public T FindDefine<T>(ushort define_id) where T : SwfLibraryDefine {
			SwfLibraryDefine def;
			if ( Defines.TryGetValue(define_id, out def) ) {
				return def as T;
			}
			return null;
		}
	}

	//
	// SwfContext
	//

	public class SwfContext {
		public SwfLibrary     Library     = new SwfLibrary();
		public SwfDisplayList DisplayList = new SwfDisplayList();
	}
}