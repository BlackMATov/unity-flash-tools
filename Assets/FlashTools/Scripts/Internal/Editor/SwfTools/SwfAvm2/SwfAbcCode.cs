namespace FlashTools.Internal.SwfTools.SwfAvm2 {
	public class SwfAbcCode {
		public static SwfAbcCode Read(SwfStreamReader reader) {
			return new SwfAbcCode();
		}
	}
}
/*
const int OP_popscope       = 0x1D;
const int OP_pushbyte       = 0x24;
const int OP_pushscope      = 0x30;
const int OP_returnvoid     = 0x47;
const int OP_constructsuper = 0x49;
const int OP_callpropvoid   = 0x4F;
const int OP_newclass       = 0x58;
const int OP_findpropstrict = 0x5D;
const int OP_getlex         = 0x60;
const int OP_getscopeobject = 0x65;
const int OP_getproperty    = 0x66;
const int OP_initproperty   = 0x68;
const int OP_getlocal0      = 0xD0;

static void ParseAVM2Instructions(SwfStreamReader reader) {
	while ( !reader.IsEOF ) {
		var op_code = reader.ReadByte();
		switch ( op_code ) {
		case OP_popscope:
			break;
		case OP_pushbyte:
			{
				var value = reader.ReadByte();
			}
			break;
		case OP_pushscope:
			break;
		case OP_returnvoid:
			break;
		case OP_constructsuper:
			{
				var arg_count = reader.ReadEncodedU32();
			}
			break;
		case OP_callpropvoid:
			{
				var index = reader.ReadEncodedU32();
				var arg_count = reader.ReadEncodedU32();
			}
			break;
		case OP_newclass:
			{
				var index = reader.ReadEncodedU32();
			}
			break;
		case OP_findpropstrict:
			{
				var index = reader.ReadEncodedU32();
			}
			break;
		case OP_getlex:
			{
				var index = reader.ReadEncodedU32();
			}
			break;
		case OP_getscopeobject:
			{
				var index = reader.ReadByte();
			}
			break;
		case OP_getproperty:
			{
				var index = reader.ReadEncodedU32();
			}
			break;
		case OP_initproperty:
			{
				var index = reader.ReadEncodedU32();
			}
			break;
		case OP_getlocal0:
			break;
		default:
			throw new UnityException(string.Format(
				"Unknown OpCode: {0}",
				op_code));
		}
	}
}*/