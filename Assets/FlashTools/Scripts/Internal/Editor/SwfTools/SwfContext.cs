using System.Collections.Generic;
using FlashTools.Internal.SwfTools.SwfTypes;

namespace FlashTools.Internal.SwfTools {

	using LibraryDefines   = SortedDictionary<ushort, SwfLibraryDefine>;
	using DisplayInstances = SortedDictionary<ushort, SwfDisplayInstance>;

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
		public int    Width  = 0;
		public int    Height = 0;
		public byte[] ARGB32 = new byte[0];
		public override SwfLibraryDefineType Type {
			get { return SwfLibraryDefineType.Bitmap; }
		}
	}

	public class SwfLibrarySpriteDefine : SwfLibraryDefine {
		public SwfControlTags ControlTags = SwfControlTags.identity;
		public override SwfLibraryDefineType Type {
			get { return SwfLibraryDefineType.Sprite; }
		}
	}

	public class SwfLibrary {
		public LibraryDefines Defines = new LibraryDefines();

		public bool HasDefine<T>(ushort define_id) where T : SwfLibraryDefine {
			return FindDefine<T>(define_id) != null;
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
	// SwfDisplayList
	//

	public enum SwfDisplayInstType {
		Shape,
		Sprite
	}

	public abstract class SwfDisplayInstance {
		public abstract SwfDisplayInstType Type { get; }

		public ushort            Id;
		public ushort            Depth;
		public SwfMatrix         Matrix;
		public SwfColorTransform ColorTransform;
	}

	public class SwfDisplayShapeInst : SwfDisplayInstance {
		public override SwfDisplayInstType Type {
			get { return SwfDisplayInstType.Shape; }
		}
	}

	public class SwfDisplaySpriteInst : SwfDisplayInstance {
		public int            CurrentTag  = 0;
		public SwfDisplayList DisplayList = new SwfDisplayList();
		public override SwfDisplayInstType Type {
			get { return SwfDisplayInstType.Sprite; }
		}
	}

	public class SwfDisplayList {
		public string           FrameName = string.Empty;
		public DisplayInstances Instances = new DisplayInstances();
	}

	//
	// SwfContext
	//

	public class SwfContext {
		public SwfLibrary     Library     = new SwfLibrary();
		public SwfDisplayList DisplayList = new SwfDisplayList();
	}
}