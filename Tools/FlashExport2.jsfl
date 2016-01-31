// ----------------------------------------------------------------------------
//
// JS core
//
// ----------------------------------------------------------------------------

if ( !String.prototype.format ) {
	String.prototype.format = function() {
		var args = arguments;
		return this.replace(/{(\d+)}/g, function(match, number) {
			return typeof args[number] != 'undefined' ? args[number] : match;
		});
	};
}

if ( !Function.prototype.bind ) {
	Function.prototype.bind = function(oThis) {
		if ( typeof this !== 'function' ) {
			throw new TypeError('Function.prototype.bind - what is trying to be bound is not callable');
		}
		var aArgs   = Array.prototype.slice.call(arguments, 1);
		var fToBind = this;
		var fNOP    = function() {};
		var fBound  = function() {
		return fToBind.apply(this instanceof fNOP && oThis ? this : oThis,
			aArgs.concat(Array.prototype.slice.call(arguments)));
		};
		fNOP.prototype = this.prototype;
		fBound.prototype = new fNOP();
		return fBound;
	};
}

if ( !Array.prototype.find ) {
	Array.prototype.find = function(predicate) {
		if (this === null) {
			throw new TypeError('Array.prototype.find called on null or undefined');
		}
		if (typeof predicate !== 'function') {
			throw new TypeError('predicate must be a function');
		}
		var list = Object(this);
		var length = list.length >>> 0;
		var thisArg = arguments[1];
		var value;
		for (var i = 0; i < length; i++) {
			value = list[i];
			if (predicate.call(thisArg, value, i, list)) {
				return value;
			}
		}
		return undefined;
	};
}

if ( typeof Object.create != 'function' ) {
	Object.create = (function() {
		function Temp() {}
		var hasOwn = Object.prototype.hasOwnProperty;
		return function (O) {
      		if (typeof O != 'object') {
				throw TypeError('Object prototype may only be an Object or null');
			}
			Temp.prototype = O;
			var obj = new Temp();
			Temp.prototype = null;
			if (arguments.length > 1) {
				var Properties = Object(arguments[1]);
				for (var prop in Properties) {
					if (hasOwn.call(Properties, prop)) {
						obj[prop] = Properties[prop];
					}
				}
			}
			return obj;
    	};
	})();
}

(function() {

	"use strict";
	
	// ----------------------------------------------------------------------------
	//
	// ft
	//
	// ----------------------------------------------------------------------------

	var ft = {};

	ft.trace = function() {
		fl.outputPanel.trace(
			Array.prototype.join.call(arguments, " "));
	};

	ft.trace_fmt = function(format) {
		var args = Array.prototype.slice.call(arguments, 1);
		ft.trace(format.format.apply(format, args));
	};

	ft.clear_output = function() {
		fl.outputPanel.clear();
	};

	ft.assert = function(expr, format) {
		if ( !expr ) {
			if ( format === undefined ) {
				throw "!!!Assert!!!";
			} else {
				var args = Array.prototype.slice.call(arguments, 2);
				throw "!!!Assert!!! " + format.format.apply(format, args);
			}
		}
	};

	ft.type_assert = function(item, type) {
		var type_is_string = (typeof type === 'string');
		ft.assert(
			( type_is_string && typeof item === type) ||
			(!type_is_string && item instanceof type),
			"Type error: {0} != {1}",
				typeof item,
				type_is_string ? type : type.constructor.name);
	};

	ft.type_assert_if_defined = function(item, type) {
		if ( item !== undefined ) {
			ft.type_assert(item, type);
		}
	};

	ft.escape_path = function(path) {
		ft.type_assert(path, 'string');
		return path.replace(/ /g, '%20');
	};

	ft.escape_string = function(str) {
		ft.type_assert(str, 'string');
		return str
			.replace(/\&/g, '&amp;')
			.replace(/\"/g, '&quot;')
			.replace(/\'/g, '&apos;')
			.replace(/</g , '&lt;')
			.replace(/>/g , '&gt;');
	};

	ft.combine_path = function(lhs, rhs) {
		ft.type_assert(lhs, 'string');
		ft.type_assert(rhs, 'string');
		return ft.escape_path(lhs) + ft.escape_path(rhs);
	};

	ft.array_foreach = function(arr, func, filter) {
		ft.type_assert(arr, Array);
		ft.type_assert(func, Function);
		ft.type_assert_if_defined(filter, Function);
		for ( var index = 0; index < arr.length; ++index ) {
			var value = arr[index];
			if ( filter === undefined || filter(value, index) ) {
				func(value, index);
			}
		}
	};

	ft.object_foreach = function(obj, func, filter) {
		ft.type_assert(obj, 'object');
		ft.type_assert(func, Function);
		ft.type_assert_if_defined(filter, Function);
		for ( var key in obj ) {
			if ( obj.hasOwnProperty(key) ) {
				var value = obj[key];
				if ( filter === undefined || filter(key, value) ) {
					func(key, value);
				}
			}
		}
	};

	// ----------------------------------------------------------------------------
	//
	// UniqueIds
	//
	// ----------------------------------------------------------------------------

	var UniqueIds = function() {
		this.clear();
	};

	UniqueIds.prototype.clear = function() {
		this.stringIds    = Object.create(null);
		this.lastStringId = 0;
	};

	UniqueIds.prototype.get_string_id = function(str) {
		ft.type_assert(str, 'string');
		if ( this.stringIds.hasOwnProperty(str) ) {
			return this.stringIds[str];
		} else {
			this.stringIds[str] = ++this.lastStringId;
			return this.lastStringId;
		}
	};

	UniqueIds.prototype.save = function(xml_path) {
		ft.type_assert(xml_path, 'string');
		var xml_node = new XmlNode("strings");
		ft.object_foreach(this.stringIds, function(key, value) {
			xml_node.child("string")
				.attr("id" , value)
				.attr("str", ft.escape_string(key));
		});
		xml_node.save(xml_path);
	};

	// ----------------------------------------------------------------------------
	//
	// XmlNode
	//
	// ----------------------------------------------------------------------------

	var XmlNode = function(node_name, node_parent) {
		ft.type_assert(node_name, 'string');
		ft.type_assert_if_defined(node_parent, XmlNode);
		this.name     = node_name;
		this.parent   = node_parent;
		this.attrs    = [];
		this.children = [];
	};

	XmlNode.prototype.attr = function(attr_name, attr_value) {
		ft.type_assert(attr_name, 'string');
		attr_value = ft.escape_string(attr_value.toString());
		var attr = this.attrs.find(function(attr) {
			return attr.name == attr_name;
		});
		if ( attr ) {
			attr.value = attr_value;
		} else {
			this.attrs.push({name:attr_name, value:attr_value});
		}
		return this;
	};

	XmlNode.prototype.child = function(child_name) {
		ft.type_assert(child_name, 'string');
		var child = new XmlNode(child_name, this);
		this.children.push(child);
		return child;
	};
	
	XmlNode.prototype.parent = function() {
		ft.assert(this.parent !== undefined, "xml node parent is undefined");
		return this.parent;
	};

	XmlNode.prototype.content = function(indent) {
		indent = indent || "";
		ft.type_assert(indent, 'string');
		var str = '{0}<{1}'.format(indent, this.name);
		ft.array_foreach(this.attrs, function(attr) {
			str += ' {0}="{1}"'.format(attr.name, attr.value);
		});
		if ( this.children.length > 0 ) {
			str += ">\n";
			ft.array_foreach(this.children, function(child) {
				str += child.content(indent + "\t") + "\n";
			});
			return str + "{0}<{1}/>".format(indent, this.name);
		} else {
			return str + "/>";
		}
	};

	XmlNode.prototype.save = function(xml_path) {
		if ( !FLfile.write(xml_path, this.content()) ) {
			throw "Can't save xml to {0}!".format(xml_path);
		}
	};
	
	// ----------------------------------------------------------------------------
	//
	// BitmapAsset
	//
	// ----------------------------------------------------------------------------
	
	var BitmapAsset = function(item, unique_ids) {
		ft.type_assert(item, BitmapItem);
		ft.type_assert(unique_ids, UniqueIds);
		this.item      = item;
		this.uniqueIds = unique_ids;
	};
	
	BitmapAsset.prototype.trace = function(indent) {
		indent = indent || "";
		ft.type_assert(indent, 'string');
		ft.trace_fmt("{0}-= BitmapAsset =-"    , indent);
		ft.trace_fmt("{0}-Name           : {1}", indent, this.item.name);
		ft.trace_fmt("{0}-ExportFilename : {1}", indent, this.get_export_filename());
	};
	
	BitmapAsset.prototype.get_id = function() {
		return this.uniqueIds.get_string_id(this.item.name);
	};
	
	BitmapAsset.prototype.get_export_filename = function() {
		return "bitmaps/{0}.png".format(this.get_id());
	};
	
	BitmapAsset.prototype.get_export_fullfilename = function(export_folder) {
		ft.type_assert(document, Document);
		return ft.combine_path(
			export_folder,
			this.get_export_filename());
	};
	
	BitmapAsset.prototype.export_content = function(export_folder) {
		ft.type_assert(document, Document);
		var fullfilename = this.get_export_fullfilename(export_folder);
		if ( !this.item.exportToFile(fullfilename) ) {
			throw "Can't export bitmap ({0})!"
				.format(fullfilename);
		}
	};
	
	BitmapAsset.prototype.export_description = function(xml_node) {
		ft.type_assert(xml_node, XmlNode);
		xml_node.child("bitmap")
			.attr("name"    , this.get_id())
			.attr("type"    , "bitmap")
			.attr("filename", this.get_export_filename());
	};
	
	// ----------------------------------------------------------------------------
	//
	// SymbolAsset
	//
	// ----------------------------------------------------------------------------
	
	var SymbolAsset = function(item, unique_ids) {
		ft.type_assert(item, SymbolItem);
		ft.type_assert(unique_ids, UniqueIds);
		this.item      = item;
		this.uniqueIds = unique_ids;
	};
	
	SymbolAsset.prototype.trace = function(indent) {
		indent = indent || "";
		ft.type_assert(indent, 'string');
		ft.trace_fmt("{0}-= SymbolAsset =-"    , indent);
		ft.trace_fmt("{0}-Name           : {1}", indent, this.item.name);
		ft.trace_fmt("{0}-ExportFilename : {1}", indent, this.get_export_filename());
	};
	
	SymbolAsset.prototype.get_id = function() {
		return this.uniqueIds.get_string_id(this.item.name);
	};
	
	SymbolAsset.prototype.get_export_filename = function() {
		return "symbols/{0}.xml".format(this.get_id());
	};
	
	SymbolAsset.prototype.get_export_fullfilename = function(export_folder) {
		ft.type_assert(document, Document);
		return ft.combine_path(
			export_folder,
			this.get_export_filename());
	};
	
	SymbolAsset.prototype.export_content = function(export_folder) {
		ft.type_assert(document, Document);
		var xml_node = new XmlNode("symbol")
			.attr("name", this.get_id());
		new TimelineInst(this.item.timeline, this.uniqueIds)
			.export_description(xml_node);
		xml_node.save(this.get_export_fullfilename(export_folder));
	};
	
	SymbolAsset.prototype.export_description = function(xml_node) {
		ft.type_assert(xml_node, XmlNode);
		xml_node.child("symbol")
			.attr("name"    , this.get_id())
			.attr("type"    , "symbol")
			.attr("filename", this.get_export_filename());
	};
	
	// ----------------------------------------------------------------------------
	//
	// TimelineInst
	//
	// ----------------------------------------------------------------------------

	var TimelineInst = function(timeline, unique_ids) {
		ft.type_assert(timeline, Timeline);
		ft.type_assert(unique_ids, UniqueIds);
		this.timeline  = timeline;
		this.uniqueIds = unique_ids;
	};
	
	TimelineInst.prototype.trace = function(indent) {
		indent = indent || "";
		ft.type_assert(indent, 'string');
		ft.trace_fmt("{0}-= TimelineInst =-", indent);
		ft.trace_fmt("{0}-Name   : {1}"     , indent, this.timeline.name);
		ft.trace_fmt("{0}-Layers : {1}"     , indent, this.timeline.layerCount);
		ft.trace_fmt("{0}-Frames : {1}"     , indent, this.timeline.frameCount);
	};
	
	TimelineInst.prototype.get_id = function() {
		return this.uniqueIds.get_string_id(this.timeline.name);
	};
	
	TimelineInst.prototype.export_description = function(xml_node) {
		ft.type_assert(xml_node, XmlNode);
		var timeline_node = xml_node.child("timeline");
		ft.array_foreach(this.timeline.layers, function(layer) {
			new LayerInst(layer, this.uniqueIds)
				.export_description(timeline_node);
		}.bind(this));
	};
	
	// ----------------------------------------------------------------------------
	//
	// LayerInst
	//
	// ----------------------------------------------------------------------------
	
	var LayerInst = function(layer, unique_ids) {
		ft.type_assert(layer, Layer);
		ft.type_assert(unique_ids, UniqueIds);
		this.layer     = layer;
		this.uniqueIds = unique_ids;
	};
	
	LayerInst.prototype.trace = function(indent) {
		indent = indent || "";
		ft.type_assert(indent, 'string');
		ft.trace_fmt("{0}-= LayerInst =-", indent);
		ft.trace_fmt("{0}-Name   : {1}"  , indent, this.layer.name);
		ft.trace_fmt("{0}-Frames : {1}"  , indent, this.layer.frameCount);
	};
	
	LayerInst.prototype.get_id = function() {
		return this.uniqueIds.get_string_id(this.layer.name);
	};
	
	LayerInst.prototype.export_description = function(xml_node) {
		ft.type_assert(xml_node, XmlNode);
		var layer_node = xml_node.child("layer")
			.attr("name"           , this.get_id())
			.attr("visible"        , this.layer.visible)
			.attr("layer_type"     , this.layer.layerType);
		if ( this.layer.parentLayer ) {
			var parent_layer = new LayerInst(this.layer.parentLayer, this.uniqueIds);
			layer_node.attr("parent_layer", parent_layer.get_id());
		}
		ft.array_foreach(this.layer.frames, function(frame, index) {
			var inst = new FrameInst(frame, index, this.uniqueIds);
			if ( inst.get_start_frame() == index ) {
				inst.export_description(layer_node);
			}
		}.bind(this));
	};
	
	// ----------------------------------------------------------------------------
	//
	// FrameInst
	//
	// ----------------------------------------------------------------------------

	var FrameInst = function(frame, index, unique_ids) {
		ft.type_assert(frame, Frame);
		ft.type_assert(index, 'number');
		ft.type_assert(unique_ids, UniqueIds);
		this.frame     = frame;
		this.index     = index;
		this.uniqueIds = unique_ids;
	};
	
	FrameInst.prototype.trace = function(indent) {
		indent = indent || "";
		ft.type_assert(indent, 'string');
		ft.trace_fmt("{0}-= FrameInst =-", indent);
		ft.trace_fmt("{0}-Name     : {1}", indent, this.frame.name);
		ft.trace_fmt("{0}-Elements : {1}", indent, this.frame.elements.length);
	};
	
	FrameInst.prototype.get_id = function() {
		return this.uniqueIds.get_string_id(this.frame.name);
	};
	
	FrameInst.prototype.get_index = function() {
		return this.index;
	};
	
	FrameInst.prototype.get_start_frame = function() {
		return this.frame.startFrame;
	};
	
	FrameInst.prototype.export_element = function(xml_node, element) {
		ft.type_assert(xml_node, XmlNode);
		ft.type_assert(element, Element);
		if ( element.elementType == "shape" ) {
			/// \TODO: shape to bitmap
		} else if ( element.elementType == "instance" ) {
			if ( element.instanceType == "bitmap" ) {
				new BitmapInst(element, this.uniqueIds)
					.export_description(xml_node);
			} else if ( element.instanceType == "symbol" ) {
				new SymbolInst(element, this.uniqueIds)
					.export_description(xml_node);
			} else {
				ft.assert(false,
					"Unsupported instance type ({0})!",
					element.instanceType);
			}
		} else {
			ft.assert(false,
				"Unsupported element type ({0})!",
				element.elementType);
		}
	};
	
	FrameInst.prototype.export_description = function(xml_node) {
		ft.type_assert(xml_node, XmlNode);
		var frame_node = xml_node.child("frame")
			.attr("name"        , this.get_id())
			.attr("index"       , this.get_index())
			.attr("duration"    , this.frame.duration)
			.attr("tween_type"  , this.frame.tweenType)
			.attr("tween_easing", this.frame.tweenEasing);
		ft.array_foreach(this.frame.elements, function(element) {
			this.export_element(frame_node, element);
		}.bind(this));
	};
	
	// ----------------------------------------------------------------------------
	//
	// ElementInst
	//
	// ----------------------------------------------------------------------------

	var ElementInst = function(inst, unique_ids) {
		ft.type_assert(inst, Instance);
		ft.type_assert(unique_ids, UniqueIds);
		this.inst      = inst;
		this.uniqueIds = unique_ids;
	};
	
	ElementInst.prototype.trace = function(indent) {
		indent = indent || "";
		ft.type_assert(indent, 'string');
		ft.trace_fmt("{0}-= ElementInst =-", indent);
		ft.trace_fmt("{0}-Name : {1}"      , indent, this.inst.name);
	};
	
	ElementInst.prototype.get_id = function() {
		return this.uniqueIds.get_string_id(this.inst.name);
	};
	
	ElementInst.prototype.export_description = function(xml_node) {
		ft.type_assert(xml_node, XmlNode);
		return xml_node.child("element")
			.attr("name"  , this.get_id())
			.attr("depth" , this.inst.depth)
			.attr("matrix", "{0};{1};{2};{3};{4};{5}".format(
				this.inst.matrix.a,  this.inst.matrix.b,
				this.inst.matrix.c,  this.inst.matrix.d,
				this.inst.matrix.tx, this.inst.matrix.ty));
	};
	
	// ----------------------------------------------------------------------------
	//
	// BitmapInst
	//
	// ----------------------------------------------------------------------------
	
	var BitmapInst = function(inst, unique_ids) {
		ElementInst.call(this, inst, unique_ids);
	};
	
	BitmapInst.prototype = Object.create(ElementInst.prototype);
	
	BitmapInst.prototype.export_description = function(xml_node) {
		ft.type_assert(xml_node, XmlNode);
		ElementInst.prototype.export_description.call(this, xml_node)
			.child("instance")
				.attr("type"  , "bitmap")
				.attr("asset" , this.uniqueIds.get_string_id(this.inst.libraryItem.name));
	};
	
	// ----------------------------------------------------------------------------
	//
	// SymbolInst
	//
	// ----------------------------------------------------------------------------
	
	var SymbolInst = function(inst, unique_ids) {
		ElementInst.call(this, inst, unique_ids);
	};
	
	SymbolInst.prototype = Object.create(ElementInst.prototype);
	
	SymbolInst.prototype.export_description = function(xml_node) {
		ft.type_assert(xml_node, XmlNode);
		var instance_node = ElementInst.prototype.export_description.call(this, xml_node)
			.child("instance")
				.attr("type"        , "symbol")
				.attr("symbol_type" , this.inst.symbolType)
				.attr("asset"       , this.uniqueIds.get_string_id(this.inst.libraryItem.name))
				.attr("visible"     , this.inst.visible)
				.attr("blend_mode"  , this.inst.blendMode);
		if ( this.inst.colorMode !== "none" ) {
			var color_mode_node = instance_node.child("color_mode")
				.attr("color_mode", this.inst.colorMode);
			if ( this.inst.colorMode == "brightness" ) {
				color_mode_node
					.attr("brightness", this.inst.brightness);
			} else if ( this.inst.colorMode == "tint" ) {
				color_mode_node
					.attr("tint" , this.inst.tintPercent)
					.attr("color", this.inst.tintColor);
			} else if ( this.inst.colorMode == "alpha" ) {
				color_mode_node
					.attr("alpha", this.inst.colorAlphaPercent);
			} else if ( this.inst.colorMode == "advanced" ) {
				color_mode_node
					.attr("a", "{0};{1}".format(this.inst.colorAlphaAmount, this.inst.colorAlphaPercent))
					.attr("r", "{0};{1}".format(this.inst.colorRedAmount  , this.inst.colorRedPercent))
					.attr("g", "{0};{1}".format(this.inst.colorGreenAmount, this.inst.colorGreenPercent))
					.attr("b", "{0};{1}".format(this.inst.colorBlueAmount , this.inst.colorBluePercent));
			} else {
				ft.assert(false,
					"Unsupported color mode ({0})!",
					this.inst.colorMode);
			}
		}
		if ( this.inst.loop !== undefined && this.inst.firstFrame !== undefined ) {
			instance_node.child("looping")
				.attr("loop"       , this.inst.loop)
				.attr("first_frame", this.inst.firstFrame);
		}
		if ( this.inst.filters && this.inst.filters.length > 0 ) {
			var filters_node = instance_node.child("filters");
			ft.array_foreach(this.inst.filters, function(filter) {
				/// \TODO export filters
				filters_node.child("filter")
					.attr("name", filter.name);
			});
		}
	};
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	// ------------------------------------
	// FlashTools
	// ------------------------------------

	var FlashTools = function() {
		this.uniqueIds = new UniqueIds();
	};

	// ------------------------------------
	// Library item
	// ------------------------------------

	FlashTools.prototype.IsFolderLibraryItem = function(item) {
		ft.type_assert(item, LibraryItem);
		return item.itemType == "folder";
	};

	FlashTools.prototype.IsBitmapLibraryItem = function(item) {
		ft.type_assert(item, LibraryItem);
		return item.itemType == "bitmap";
	};

	FlashTools.prototype.IsSymbolLibraryItem = function(item) {
		ft.type_assert(item, LibraryItem);
		var item_type = item.itemType;
		return item_type == "graphic" || item_type == "movie clip";
	};

	// ------------------------------------
	// Document
	// ------------------------------------

	FlashTools.prototype.Document_TraceInfo = function(document) {
		ft.type_assert(document, Document);
		ft.trace_fmt("Name         : {0}", document.name);
		ft.trace_fmt("Path         : {0}", this.Document_GetPath(document));
		ft.trace_fmt("ExportFolder : {0}", this.Document_GetExportFolder(document));
	};

	FlashTools.prototype.Document_GetPath = function(document) {
		ft.type_assert(document, Document);
		return ft.escape_path(document.pathURI);
	};

	FlashTools.prototype.Document_GetExportFolder = function(document) {
		ft.type_assert(document, Document);
		return ft.combine_path(
			this.Document_GetPath(document),
			"_export/");
	};

	FlashTools.prototype.Document_GetStageExportPath = function(document) {
		ft.type_assert(document, Document);
		return ft.combine_path(
			this.Document_GetExportFolder(document),
			"stage.xml");
	};

	FlashTools.prototype.Document_GetLibraryExportPath = function(document) {
		ft.type_assert(document, Document);
		return ft.combine_path(
			this.Document_GetExportFolder(document),
			"library.xml");
	};

	FlashTools.prototype.Document_GetStringIdsExportPath = function(document) {
		ft.type_assert(document, Document);
		return ft.combine_path(
			this.Document_GetExportFolder(document),
			"strings.xml");
	};

	FlashTools.prototype.Document_ExitEditMode = function(document) {
		ft.type_assert(document, Document);
		for ( var i = 0; i < 100; ++i ) {
			document.exitEditMode();
		}
	};

	FlashTools.prototype.Document_ForEachByLibraryItems = function(document, func, filter) {
		ft.type_assert(document, Document);
		ft.type_assert(func, Function);
		ft.type_assert_if_defined(filter, Function);
		ft.array_foreach(document.library.items, func, filter);
	};

	FlashTools.prototype.Document_PrepareExportFolder = function(document) {
		ft.type_assert(document, Document);
		var export_folder = this.Document_GetExportFolder(document);
		if ( FLfile.exists(export_folder) ) {
			if ( !FLfile.remove(export_folder) ) {
				throw "Can't remove document export folder ({0})!"
					.format(export_folder);
			}
		}
		if ( !FLfile.createFolder(export_folder) ) {
			throw "Can't create document export folder ({0})!"
				.format(export_folder);
		}
		if ( !FLfile.createFolder(export_folder + "bitmaps/") ) {
			throw "Can't create document bitmaps export folder ({0})!"
				.format(export_folder);
		}
		if ( !FLfile.createFolder(export_folder + "symbols/") ) {
			throw "Can't create document symbols export folder ({0})!"
				.format(export_folder);
		}
	};

	FlashTools.prototype.Document_ExportLibrary = function(document) {
		ft.type_assert(document, Document);
		var xml_node = new XmlNode("library")
			.attr("frame_rate", document.frameRate);
		this.Document_ForEachByLibraryItems(document, function(item) {
			if ( this.IsFolderLibraryItem(item) ) {
				// nothing
			} else if ( this.IsBitmapLibraryItem(item) ) {
				var bitmap_asset = new BitmapAsset(item, this.uniqueIds);
				bitmap_asset.export_description(xml_node);
			} else if ( this.IsSymbolLibraryItem(item) ) {
				var symbol_asset = new SymbolAsset(item, this.uniqueIds);
				symbol_asset.export_description(xml_node);
			} else {
				throw "Unsupported library item type ({0})!"
					.format(item.itemType);
			}
		}.bind(this));
		xml_node.save(this.Document_GetLibraryExportPath(document));
	};

	FlashTools.prototype.Document_ExportBitmaps = function(document) {
		ft.type_assert(document, Document);
		this.Document_ForEachByLibraryItems(document, function(item) {
			var bitmap_asset = new BitmapAsset(item, this.uniqueIds);
			bitmap_asset.export_content(this.Document_GetExportFolder(document));
		}.bind(this), this.IsBitmapLibraryItem.bind(this));
	};

	FlashTools.prototype.Document_ExportSymbols = function(document) {
		ft.type_assert(document, Document);
		this.Document_ForEachByLibraryItems(document, function(item) {
			var symbol_asset = new SymbolAsset(item, this.uniqueIds);
			symbol_asset.export_content(this.Document_GetExportFolder(document));
		}.bind(this), this.IsSymbolLibraryItem.bind(this));
	};

	FlashTools.prototype.Document_ExportStage = function(document) {
		ft.type_assert(document, Document);
		this.Document_ExitEditMode(document);
		var xml_node = new XmlNode("stage");
		new TimelineInst(document.getTimeline(), this.uniqueIds)
			.export_description(xml_node);
		xml_node.save(this.Document_GetStageExportPath(document));
	};

	FlashTools.prototype.Document_ExportStringIds = function(document) {
		ft.type_assert(document, Document);
		this.uniqueIds.save(this.Document_GetStringIdsExportPath(document));
	};

	// ------------------------------------
	// Convert
	// ------------------------------------

	FlashTools.prototype.ConvertAll = function() {
		ft.array_foreach(fl.documents, function(document) {
			this.ConvertOne(document);
		}.bind(this));
	};

	FlashTools.prototype.ConvertOne = function(document) {
		ft.type_assert(document, Document);
		this.uniqueIds.clear();
		ft.trace("-= Convert document start =-");
		try {
			this.Document_TraceInfo(document);
			this.Document_PrepareExportFolder(document);
			this.Document_ExportLibrary(document);
			this.Document_ExportBitmaps(document);
			this.Document_ExportSymbols(document);
			this.Document_ExportStage(document);
			this.Document_ExportStringIds(document);
			ft.trace("-= Convert document finish =-");
		} catch ( e ) {
			ft.trace("-= Convert document error =- : " + e);
		}
	};

	// ------------------------------------
	// Tests
	// ------------------------------------

	FlashTools.prototype.Test0 = function() {
		ft.assert(true);
	};

	FlashTools.prototype.Test1 = function() {
		ft.assert(true);
	};

	FlashTools.prototype.RunTests = function() {
		try {
			this.Test0();
			this.Test1();
			return true;
		} catch ( e ) {
			ft.trace_fmt("!!!Error!!! Unit test fail: {0}", e);
			return false;
		}
	};

	// ------------------------------------
	// Main
	// ------------------------------------

	(function() {
		ft.clear_output();
		var flash_tools = new FlashTools();
		if ( flash_tools.RunTests() ) {
			flash_tools.ConvertAll();
		}
	})();
})();
