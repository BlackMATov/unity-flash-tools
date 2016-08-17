using UnityEngine;
using System;
using System.Collections.Generic;

namespace FlashTools.Internal.SwfTools.SwfAvm2 {
	public class SwfAbcFile {
		public AbcFileInfo Info;

		public static SwfAbcFile Read(SwfStreamReader reader) {
			return new SwfAbcFile{
				Info = ParseAbcFileInfo(reader)};
		}

		// ------------------------------------------------------------------------
		//
		// Data
		//
		// ------------------------------------------------------------------------

		public class AbcFileInfo {
			public ushort               MinorVersion;
			public ushort               MajorVersion;
			public ConstantPoolInfo     ConstantPool;
			public List<MethodInfo>     Methods;
			public List<MetadataInfo>   Metadata;
			public List<InstanceInfo>   Instances;
			public List<ClassInfo>      Classes;
			public List<ScriptInfo>     Scripts;
			public List<MethodBodyInfo> MethodBodies;
		}

		public class ConstantPoolInfo {
			public List<int>            Integers;
			public List<uint>           UIntegers;
			public List<double>         Doubles;
			public List<string>         Strings;
			public List<NamespaceInfo>  Namespaces;
			public List<NSSetInfo>      NSSets;
			public List<MultinameInfo>  Multinames;
		}

		public class NamespaceInfo {
			public enum Kinds : byte {
				Namespace          = 0x08,
				PackageNamespace   = 0x16,
				PackageInternalNs  = 0x17,
				ProtectedNamespace = 0x18,
				ExplicitNamespace  = 0x19,
				StaticProtectedNs  = 0x1A,
				PrivateNs          = 0x05
			}
			public Kinds Kind = Kinds.Namespace;
			public uint  Name = 0;
		}

		public class NSSetInfo {
			public List<uint> Ns = new List<uint>();
		}

		public class MultinameInfo {
			public enum Kinds : byte {
				QName         = 0x07,
				QNameA        = 0x0D,
				RTQName       = 0x0F,
				RTQNameA      = 0x10,
				RTQNameL      = 0x11,
				RTQNameLA     = 0x12,
				Multiname     = 0x09,
				MultinameA    = 0x0E,
				MultinameL    = 0x1B,
				MultinameLA   = 0x1C,
				Multiname0x1D = 0x1D
			}
			public class TypeQName {
				public uint Namespace;
				public uint NameIndex;
			}
			public class TypeRTQName {
				public uint NameIndex;
			}
			public class TypeRTQNameL {
			}
			public class TypeMultiname {
				public uint NameIndex;
				public uint NsSet;
			}
			public class TypeMultinameL {
				public uint NsSet;
			}
			public class Type0x1D {
				public uint       NameIndex;
				public List<uint> TypeIndices;
			}
			public Kinds          Kind           = Kinds.RTQNameL;
			public TypeQName      KindQName      = null;
			public TypeRTQName    KindRTQName    = null;
			public TypeRTQNameL   KindRTQNameL   = new TypeRTQNameL();
			public TypeMultiname  KindMultiname  = null;
			public TypeMultinameL KindMultinameL = null;
			public Type0x1D       Kind0x1D       = null;
		}

		public class MethodInfo {
			public struct OptionInfo {
				public uint Val;
				public byte Kind;
			}
			public uint             ReturnType;
			public List<uint>       ParamTypes;
			public uint             Name;
			public bool             FlagNeedArguments;
			public bool             FlagActivation;
			public bool             FlagNeedRest;
			public bool             FlagHasOptional;
			public bool             FlagSetDxns;
			public bool             FlagHasParamNames;
			public List<OptionInfo> Options;
			public List<uint>       ParamNames;
		}

		public class MetadataInfo {
			public struct ItemInfo {
				public uint Key;
				public uint Value;
			}
			public uint           Name;
			public List<ItemInfo> Items;
		}

		public class InstanceInfo {
			public uint            Name;
			public uint            Supername;
			public bool            FlagClassSealed;
			public bool            FlagClassFinal;
			public bool            FlagClassInterface;
			public bool            FlagClassProtectedNs;
			public uint            ProtectedNs;
			public List<uint>      Interface;
			public uint            Iinit;
			public List<TraitInfo> Traits;
		}

		public class TraitInfo {
			public enum Kinds : byte {
				Slot     = 0,
				Method   = 1,
				Getter   = 2,
				Setter   = 3,
				Class    = 4,
				Function = 5,
				Const    = 6
			}
			public class KindSlot {
				public uint SlotId;
				public uint TypeName;
				public uint Vindex;
				public byte Vkind;
			}

			public class KindClass {
				public uint SlotId;
				public uint ClassI;
			}

			public class KindFunction {
				public uint SlotId;
				public uint Funciton;
			}

			public class KindMethod {
				public uint DispID;
				public uint Method;
			}
			public uint         Name;
			public Kinds        Kind;
			public bool         AttrFinal;
			public bool         AttrOverride;
			public bool         AttrMetadata;
			public KindSlot     Slot;
			public KindClass    Class;
			public KindFunction Function;
			public KindMethod   Method;
			public List<uint>   Metadata;
		}

		public class ClassInfo {
			public uint            Cinit;
			public List<TraitInfo> Traits;
		}

		public class ScriptInfo {
			public uint            Init;
			public List<TraitInfo> Traits;
		}

		public class MethodBodyInfo {
			public uint                Method;
			public uint                MaxStack;
			public uint                LocalCount;
			public uint                InitScopeDepth;
			public uint                MaxScopeDepth;
			public byte[]              Code;
			public List<ExceptionInfo> Exceptions;
			public List<TraitInfo>     Traits;
		}

		public class ExceptionInfo {
			public uint From;
			public uint To;
			public uint Target;
			public uint ExcType;
			public uint VarName;
		}

		// ------------------------------------------------------------------------
		//
		// Parsers
		//
		// ------------------------------------------------------------------------

		/*abcFile{
			u16              minor_version
			u16              major_version
			cpool_info       constant_pool
			u30              method_count
			method_info      method[method_count]
			u30              metadata_count
			metadata_info    metadata[metadata_count]
			u30              class_count
			instance_info    instance[class_count]
			class_info       class[class_count]
			u30              script_count
			script_info      script[script_count]
			u30              method_body_count
			method_body_info method_body[method_body_count]
		}*/
		static AbcFileInfo ParseAbcFileInfo(SwfStreamReader reader) {
			var abc_file_info = new AbcFileInfo();

			abc_file_info.MinorVersion = reader.ReadUInt16();
			abc_file_info.MajorVersion = reader.ReadUInt16();

			abc_file_info.ConstantPool = ParseConstantPoolInfo(reader);

			abc_file_info.Methods = new List<MethodInfo>((int)reader.ReadEncodedU32());
			for ( var i = 0; i < abc_file_info.Methods.Capacity; ++i ) {
				abc_file_info.Methods.Add(ParseMethodInfo(reader));
			}

			abc_file_info.Metadata = new List<MetadataInfo>((int)reader.ReadEncodedU32());
			for ( var i = 0; i < abc_file_info.Metadata.Capacity; ++i ) {
				abc_file_info.Metadata.Add(ParseMetadataInfo(reader));
			}

			abc_file_info.Instances = new List<InstanceInfo>((int)reader.ReadEncodedU32());
			abc_file_info.Classes   = new List<ClassInfo>(abc_file_info.Instances.Capacity);
			for ( var i = 0; i < abc_file_info.Instances.Capacity; ++i ) {
				abc_file_info.Instances.Add(ParseInstanceInfo(reader));
				abc_file_info.Classes.Add(ParseClassInfo(reader));
			}

			abc_file_info.Scripts = new List<ScriptInfo>((int)reader.ReadEncodedU32());
			for ( var i = 0; i < abc_file_info.Scripts.Capacity; ++i ) {
				abc_file_info.Scripts.Add(ParseScriptInfo(reader));
			}

			abc_file_info.MethodBodies = new List<MethodBodyInfo>((int)reader.ReadEncodedU32());
			for ( var i = 0; i < abc_file_info.MethodBodies.Capacity; ++i ) {
				abc_file_info.MethodBodies.Add(ParseMethodBodyInfo(reader));
			}
			return abc_file_info;
		}

		/*cpool_info{
			u30            int_count
			s32            integer[int_count]
			u30            uint_count
			u32            uinteger[uint_count]
			u30            double_count
			d64            double[double_count]
			u30            string_count
			string_info    string[string_count]
			u30            namespace_count
			namespace_info namespace[namespace_count]
			u30            ns_set_count
			ns_set_info    ns_set[ns_set_count]
			u30            multiname_count
			multiname_info multiname[multiname_count]
		}*/
		static ConstantPoolInfo ParseConstantPoolInfo(SwfStreamReader reader) {
			var integer_count = reader.ReadEncodedU32();
			var integers      = new List<int>((int)integer_count);
			integers.Add(0);
			for ( var i = 1; i < integer_count; ++i ) {
				integers.Add(reader.ReadInt32());
			}

			var uinteger_count = reader.ReadEncodedU32();
			var uintegers      = new List<uint>((int)uinteger_count);
			uintegers.Add(0);
			for ( var i = 1; i < uinteger_count; ++i ) {
				uintegers.Add(reader.ReadUInt32());
			}

			var double_count = reader.ReadEncodedU32();
			var doubles      = new List<double>((int)double_count);
			doubles.Add(double.NaN);
			for ( var i = 1; i < double_count; ++i ) {
				doubles.Add(reader.ReadDouble64());
			}

			var string_count = reader.ReadEncodedU32();
			var strings      = new List<string>((int)string_count);
			strings.Add(string.Empty);
			for ( var i = 1; i < string_count; ++i ) {
				strings.Add(ParseStringInfo(reader));
			}

			var namespace_count = reader.ReadEncodedU32();
			var namespaces      = new List<NamespaceInfo>((int)namespace_count);
			namespaces.Add(new NamespaceInfo());
			for ( var i = 1; i < namespace_count; ++i ) {
				namespaces.Add(ParseNamespaceInfo(reader));
			}

			var ns_set_count = reader.ReadEncodedU32();
			var ns_sets      = new List<NSSetInfo>((int)ns_set_count);
			ns_sets.Add(new NSSetInfo());
			for ( var i = 1; i < ns_set_count; ++i ) {
				ns_sets.Add(ParseNSSetInfo(reader));
			}

			var multiname_count = reader.ReadEncodedU32();
			var multinames      = new List<MultinameInfo>((int)multiname_count);
			multinames.Add(new MultinameInfo());
			for ( var i = 1; i < multiname_count; ++i ) {
				multinames.Add(ParseMultinameInfo(reader));
			}

			return new ConstantPoolInfo{
				Integers   = integers,
				UIntegers  = uintegers,
				Doubles    = doubles,
				Strings    = strings,
				Namespaces = namespaces,
				NSSets     = ns_sets,
				Multinames = multinames};
		}

		/*string_info{
			u30 size
			u8  utf8[size]
		}*/
		static string ParseStringInfo(SwfStreamReader reader) {
			var size  = reader.ReadEncodedU32();
			var bytes = reader.ReadBytes((int)size);
			return System.Text.Encoding.UTF8.GetString(bytes);
		}

		/*namespace_info{
			u8  kind
			u30 name
		}*/
		static NamespaceInfo ParseNamespaceInfo(SwfStreamReader reader) {
			var kind_id = reader.ReadByte();
			if ( !Enum.IsDefined(typeof(NamespaceInfo.Kinds), (byte)kind_id) ) {
				throw new UnityException(string.Format(
					"Incorrect NamespaceInfo.Kinds id: {0}",
					kind_id));
			}
			var name = reader.ReadEncodedU32();
			return new NamespaceInfo{
				Kind = (NamespaceInfo.Kinds)kind_id,
				Name = name};
		}

		/*ns_set_info{
			u30 count
			u30 ns[count]
		}*/
		static NSSetInfo ParseNSSetInfo(SwfStreamReader reader) {
			var ns = new List<uint>((int)reader.ReadEncodedU32());
			for ( var i = 0; i < ns.Capacity; ++i ) {
				ns.Add(reader.ReadEncodedU32());
			}
			return new NSSetInfo{
				Ns = ns};
		}

		/*multiname_info {
			u8 kind
			u8 data[]
		}
		multiname_kind_QName {
			u30 ns
			u30 name
		}
		multiname_kind_RTQName {
			u30 name
		}
		multiname_kind_RTQNameL {
		}
		multiname_kind_Multiname {
			u30 name
			u30 ns_set
		}
		multiname_kind_MultinameL {
			u30 ns_set
		}*/
		static MultinameInfo ParseMultinameInfo(SwfStreamReader reader) {
			var info = new MultinameInfo();

			var kind_id = reader.ReadByte();
			if ( !Enum.IsDefined(typeof(MultinameInfo.Kinds), (byte)kind_id) ) {
				throw new UnityException(string.Format(
					"Incorrect MultinameInfo.Kinds id: {0}",
					kind_id));
			}

			info.Kind = (MultinameInfo.Kinds)kind_id;
			switch ( info.Kind ) {
			case MultinameInfo.Kinds.QName:
			case MultinameInfo.Kinds.QNameA:
				{
					info.KindQName           = new MultinameInfo.TypeQName();
					info.KindQName.Namespace = reader.ReadEncodedU32();
					info.KindQName.NameIndex = reader.ReadEncodedU32();
					break;
				}
			case MultinameInfo.Kinds.RTQName:
			case MultinameInfo.Kinds.RTQNameA:
				{
					info.KindRTQName           = new MultinameInfo.TypeRTQName();
					info.KindRTQName.NameIndex = reader.ReadEncodedU32();
					break;
				}
			case MultinameInfo.Kinds.RTQNameL:
			case MultinameInfo.Kinds.RTQNameLA:
				{
					info.KindRTQNameL = new MultinameInfo.TypeRTQNameL();
					break;
				}
			case MultinameInfo.Kinds.Multiname:
			case MultinameInfo.Kinds.MultinameA:
				{
					info.KindMultiname           = new MultinameInfo.TypeMultiname();
					info.KindMultiname.NameIndex = reader.ReadEncodedU32();
					info.KindMultiname.NsSet     = reader.ReadEncodedU32();
					break;
				}
			case MultinameInfo.Kinds.MultinameL:
			case MultinameInfo.Kinds.MultinameLA:
				{
					info.KindMultinameL       = new MultinameInfo.TypeMultinameL();
					info.KindMultinameL.NsSet = reader.ReadEncodedU32();
					break;
				}
			case MultinameInfo.Kinds.Multiname0x1D:
				{
					info.Kind0x1D             = new MultinameInfo.Type0x1D();
					info.Kind0x1D.NameIndex   = reader.ReadEncodedU32();
					info.Kind0x1D.TypeIndices = new List<uint>((int)reader.ReadEncodedU32());
					for ( var i = 0; i < info.Kind0x1D.TypeIndices.Capacity; ++i ) {
						info.Kind0x1D.TypeIndices.Add(reader.ReadEncodedU32());
					}
					break;
				}
			default:
				throw new UnityException(string.Format(
					"Incorrect MultinameInfo.Kinds id: {0}",
					kind_id));
			}
			return info;
		}

		/*method_info
		{
			u30           param_count
			u30           return_type
			u30           param_type[param_count]
			u30           name
			u8            flags
			option_info   options
			param_info    param_names
		}
		option_info
		{
			u30           option_count
			option_detail option[option_count]
		}
		option_detail
		{
			u30           val
			u8            kind
		}
		param_info
		{
			u30           param_name[param_count]
		}*/
		static MethodInfo ParseMethodInfo(SwfStreamReader reader) {
			var info        = new MethodInfo();
			var param_count = reader.ReadEncodedU32();
			info.ReturnType = reader.ReadEncodedU32();
			info.ParamTypes = new List<uint>((int)param_count);
			for ( var i = 0; i < info.ParamTypes.Capacity; ++i ) {
				info.ParamTypes.Add(reader.ReadEncodedU32());
			}
			info.Name = reader.ReadEncodedU32();
			var flags = reader.ReadByte();
			info.FlagNeedArguments = ((flags & 0x01) != 0);
			info.FlagActivation    = ((flags & 0x02) != 0);
			info.FlagNeedRest      = ((flags & 0x04) != 0);
			info.FlagHasOptional   = ((flags & 0x08) != 0);
			info.FlagSetDxns       = ((flags & 0x40) != 0);
			info.FlagHasParamNames = ((flags & 0x80) != 0);
			if ( info.FlagHasOptional ) {
				info.Options = new List<MethodInfo.OptionInfo>((int)reader.ReadEncodedU32());
				for ( var i = 0; i < info.Options.Capacity; ++i ) {
					info.Options.Add(new MethodInfo.OptionInfo{
						Val   = reader.ReadEncodedU32(),
						Kind  = reader.ReadByte()});
				}
			} else {
				info.Options = new List<MethodInfo.OptionInfo>();
			}
			if ( info.FlagHasParamNames ) {
				info.ParamNames = new List<uint>((int)param_count);
				for ( var i = 0; i < info.ParamNames.Capacity; ++i ) {
					info.ParamNames.Add(reader.ReadEncodedU32());
				}
			} else {
				info.ParamNames = new List<uint>();
			}
			return info;
		}

		/*metadata_info
		{
			u30        name
			u30        item_count
			item_info  items[item_count]
		}
		item_info
		{
			u30        key
			u30        value
		}*/
		static MetadataInfo ParseMetadataInfo(SwfStreamReader reader) {
			var info   = new MetadataInfo();
			info.Name  = reader.ReadEncodedU32();
			info.Items = new List<MetadataInfo.ItemInfo>((int)reader.ReadEncodedU32());
			for ( var i = 0; i < info.Items.Capacity; ++i ) {
				info.Items.Add(new MetadataInfo.ItemInfo{
					Key   = reader.ReadEncodedU32(),
					Value = reader.ReadEncodedU32()});
			}
			return info;
		}

		/*traits_info
		{
			u30 name
			u8  kind
			u8  data[]
			u30 metadata_count
			u30 metadata[metadata_count]
		}*/
		static List<TraitInfo> ParseTraitsInfo(SwfStreamReader reader) {
			var traits = new List<TraitInfo>((int)reader.ReadEncodedU32());
			for ( var i = 0; i < traits.Capacity; ++i ) {
				traits.Add(ParseTraitInfo(reader));
			}
			return traits;
		}

		static TraitInfo ParseTraitInfo(SwfStreamReader reader) {
			var info      = new TraitInfo();
			info.Name     = reader.ReadEncodedU32();

			var kind_attr = reader.ReadByte();
			var kind_id   = (kind_attr & 0x0F);
			var attr      = ((kind_attr & 0xF0) >> 4);

			if ( !Enum.IsDefined(typeof(TraitInfo.Kinds), (byte)kind_id) ) {
				throw new UnityException(string.Format(
					"Incorrect TraitInfo.Kinds id: {0}",
					kind_id));
			}

			info.Kind         = (TraitInfo.Kinds)kind_id;
			info.AttrFinal    = (attr & 0x01) != 0;
			info.AttrOverride = (attr & 0x02) != 0;
			info.AttrMetadata = (attr & 0x04) != 0;

			var kind = (TraitInfo.Kinds)kind_id;
			switch ( kind ) {
			case TraitInfo.Kinds.Slot:
			case TraitInfo.Kinds.Const:
				{
					info.Slot          = new TraitInfo.KindSlot();
					info.Slot.SlotId   = reader.ReadEncodedU32();
					info.Slot.TypeName = reader.ReadEncodedU32();
					info.Slot.Vindex   = reader.ReadEncodedU32();
					if ( 0 != info.Slot.Vindex ) {
						info.Slot.Vkind = reader.ReadByte();
					} else {
						info.Slot.Vkind = 0;
					}
					break;
				}
			case TraitInfo.Kinds.Class:
				{
					info.Class        = new TraitInfo.KindClass();
					info.Class.SlotId = reader.ReadEncodedU32();
					info.Class.ClassI = reader.ReadEncodedU32();
					break;
				}
			case TraitInfo.Kinds.Function:
				{
					info.Function          = new TraitInfo.KindFunction();
					info.Function.SlotId   = reader.ReadEncodedU32();
					info.Function.Funciton = reader.ReadEncodedU32();
					break;
				}
			case TraitInfo.Kinds.Method:
			case TraitInfo.Kinds.Getter:
			case TraitInfo.Kinds.Setter:
				{
					info.Method        = new TraitInfo.KindMethod();
					info.Method.DispID = reader.ReadEncodedU32();
					info.Method.Method = reader.ReadEncodedU32();
					break;
				}
			default:
				throw new UnityException(string.Format(
					"Incorrect TraitInfo.Kinds id: {0}",
					kind_id));
			}
			if ( info.AttrMetadata ) {
				info.Metadata = new List<uint>((int)reader.ReadEncodedU32());
				for ( var i = 0; i < info.Metadata.Capacity; ++i ) {
					info.Metadata.Add(reader.ReadEncodedU32());
				}
			} else {
				info.Metadata = new List<uint>();
			}
			return info;
		}

		/*exception_info
		{
			u30 from
			u30 to
			u30 target
			u30 exc_type
			u30 var_name
		}*/
		static List<ExceptionInfo> ParseExceptionsInfo(SwfStreamReader reader) {
			var exceptions = new List<ExceptionInfo>((int)reader.ReadEncodedU32());
			for ( var i = 0; i < exceptions.Capacity; ++i ) {
				exceptions.Add(ParseExceptionInfo(reader));
			}
			return exceptions;
		}

		static ExceptionInfo ParseExceptionInfo(SwfStreamReader reader) {
			var info     = new ExceptionInfo();
			info.From    = reader.ReadEncodedU32();
			info.To      = reader.ReadEncodedU32();
			info.Target  = reader.ReadEncodedU32();
			info.ExcType = reader.ReadEncodedU32();
			info.VarName = reader.ReadEncodedU32();
			return info;
		}

		/*instance_info
		{
			u30         name
			u30         super_name
			u8          flags
			u30         protectedNs
			u30         intrf_count
			u30         interface[intrf_count]
			u30         iinit
			u30         trait_count
			traits_info trait[trait_count]
		}*/
		static InstanceInfo ParseInstanceInfo(SwfStreamReader reader) {
			var info                  = new InstanceInfo();
			info.Name                 = reader.ReadEncodedU32();
			info.Supername            = reader.ReadEncodedU32();
			var flags                 = reader.ReadByte();
			info.FlagClassSealed      = (flags & 0x01 ) != 0;
			info.FlagClassFinal       = (flags & 0x02 ) != 0;
			info.FlagClassInterface   = (flags & 0x04 ) != 0;
			info.FlagClassProtectedNs = (flags & 0x08 ) != 0;
			if ( info.FlagClassProtectedNs ) {
				info.ProtectedNs = reader.ReadEncodedU32();
			} else {
				info.ProtectedNs = 0;
			}

			info.Interface = new List<uint>((int)reader.ReadEncodedU32());
			for ( var i = 0; i < info.Interface.Capacity; ++i ) {
				info.Interface.Add(reader.ReadEncodedU32());
			}
			info.Iinit  = reader.ReadEncodedU32();
			info.Traits = ParseTraitsInfo(reader);
			return info;
		}

		/*class_info
		{
			u30         cinit
			u30         trait_count
			traits_info traits[trait_count]
		}*/
		static ClassInfo ParseClassInfo(SwfStreamReader reader) {
			var info    = new ClassInfo();
			info.Cinit  = reader.ReadEncodedU32();
			info.Traits = ParseTraitsInfo(reader);
			return info;
		}

		/*script_info
		{
			u30         init
			u30         trait_count
			traits_info trait[trait_count]
		}*/
		static ScriptInfo ParseScriptInfo(SwfStreamReader reader) {
			var info    = new ScriptInfo();
			info.Init   = reader.ReadEncodedU32();
			info.Traits = ParseTraitsInfo(reader);
			return info;
		}

		/*method_body_info
		{
			u30            method
			u30            max_stack
			u30            local_count
			u30            init_scope_depth
			u30            max_scope_depth
			u30            code_length
			u8             code[code_length]
			u30            exception_count
			exception_info exception[exception_count]
			u30            trait_count
			traits_info    trait[trait_count]
		}*/
		static MethodBodyInfo ParseMethodBodyInfo(SwfStreamReader reader) {
			var info            = new MethodBodyInfo();
			info.Method         = reader.ReadEncodedU32();
			info.MaxStack       = reader.ReadEncodedU32();
			info.LocalCount     = reader.ReadEncodedU32();
			info.InitScopeDepth = reader.ReadEncodedU32();
			info.MaxScopeDepth  = reader.ReadEncodedU32();
			info.Code           = reader.ReadBytes((int)reader.ReadEncodedU32());
			info.Exceptions     = ParseExceptionsInfo(reader);
			info.Traits         = ParseTraitsInfo(reader);
			return info;
		}
	}
}