// ----------------------------------------------------------------------------
//
// JS core
//
// ----------------------------------------------------------------------------

if (!String.prototype.format) {
	String.prototype.format = function () {
		var args = arguments;
		return this.replace(/{(\d+)}/g, function (match, number) {
			return typeof args[number] != 'undefined' ? args[number] : match;
		});
	};
}

if (!Function.prototype.bind) {
	Function.prototype.bind = function (oThis) {
		if (typeof this !== 'function') {
			throw new TypeError('Function.prototype.bind - what is trying to be bound is not callable');
		}
		var aArgs = Array.prototype.slice.call(arguments, 1);
		var fToBind = this;
		var fNOP = function () {};
		var fBound = function () {
			return fToBind.apply(this instanceof fNOP && oThis ? this : oThis,
				aArgs.concat(Array.prototype.slice.call(arguments)));
		};
		fNOP.prototype = this.prototype;
		fBound.prototype = new fNOP();
		return fBound;
	};
}

if (!Array.prototype.find) {
	Array.prototype.find = function (predicate) {
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

if (typeof Object.create != 'function') {
	Object.create = (function () {
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

(function () {

	"use strict";

	// ----------------------------------------------------------------------------
	//
	// ft
	//
	// ----------------------------------------------------------------------------

	var ft = {};

	ft.trace = function () {
		fl.outputPanel.trace(
			Array.prototype.join.call(arguments, " "));
	};

	ft.trace_fmt = function (format) {
		var args = Array.prototype.slice.call(arguments, 1);
		ft.trace(format.format.apply(format, args));
	};

	ft.clear_output = function () {
		fl.outputPanel.clear();
	};

	ft.assert = function (expr, format) {
		if (!expr) {
			if (format === undefined) {
				throw "!!!Assert!!!";
			} else {
				var args = Array.prototype.slice.call(arguments, 2);
				throw "!!!Assert!!! " + format.format.apply(format, args);
			}
		}
	};

	ft.type_assert = function (item, type) {
		var type_is_string = (typeof type === 'string');
		ft.assert(
			(type_is_string && typeof item === type) ||
			(!type_is_string && item instanceof type),
			"Type error: {0} != {1}",
			typeof item,
			type_is_string ? type : type.constructor.name);
	};

	ft.type_assert_if_defined = function (item, type) {
		if (item !== undefined) {
			ft.type_assert(item, type);
		}
	};

	ft.escape_path = function (path) {
		ft.type_assert(path, 'string');
		return path.replace(/ /g, '%20');
	};

	ft.escape_string = function (str) {
		ft.type_assert(str, 'string');
		return str
			.replace(/\&/g, '&amp;')
			.replace(/\"/g, '&quot;')
			.replace(/\'/g, '&apos;')
			.replace(/</g, '&lt;')
			.replace(/>/g, '&gt;');
	};

	ft.combine_path = function (lhs, rhs) {
		ft.type_assert(lhs, 'string');
		ft.type_assert(rhs, 'string');
		return ft.escape_path(lhs) + ft.escape_path(rhs);
	};
	
	ft.array_foreach = function (arr, func, filter) {
		ft.type_assert(arr, Array);
		ft.type_assert(func, Function);
		ft.type_assert_if_defined(filter, Function);
		for (var index = 0; index < arr.length; ++index) {
			var value = arr[index];
			if (filter === undefined || filter(value, index)) {
				func(value, index);
			}
		}
	};

	ft.object_foreach = function (obj, func, filter) {
		ft.type_assert(obj, 'object');
		ft.type_assert(func, Function);
		ft.type_assert_if_defined(filter, Function);
		for (var key in obj) {
			if (obj.hasOwnProperty(key)) {
				var value = obj[key];
				if (filter === undefined || filter(key, value)) {
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

	var UniqueIds = function () {
		this.clear();
	};

	UniqueIds.prototype.clear = function () {
		this.stringIds = Object.create(null);
		this.lastStringId = 0;
	};

	UniqueIds.prototype.get_string_id = function (str) {
		ft.type_assert(str, 'string');
		if (this.stringIds.hasOwnProperty(str)) {
			return this.stringIds[str];
		} else {
			this.stringIds[str] = ++this.lastStringId;
			return this.lastStringId;
		}
	};

	UniqueIds.prototype.save = function (xml_path) {
		ft.type_assert(xml_path, 'string');
		var xml_node = new XmlNode("strings");
		ft.object_foreach(this.stringIds, function (key, value) {
			xml_node.child("string")
				.attr("id", value)
				.attr("str", ft.escape_string(key));
		});
		xml_node.save(xml_path);
	};

	// ----------------------------------------------------------------------------
	//
	// XmlNode
	//
	// ----------------------------------------------------------------------------

	var XmlNode = function (node_name, node_parent) {
		ft.type_assert(node_name, 'string');
		ft.type_assert_if_defined(node_parent, XmlNode);
		this.name = node_name;
		this.parent = node_parent;
		this.attrs = [];
		this.children = [];
	};

	XmlNode.prototype.attr = function (attr_name, attr_value) {
		ft.type_assert(attr_name, 'string');
		attr_value = ft.escape_string(attr_value.toString());
		var attr = this.attrs.find(function (attr) {
			return attr.name == attr_name;
		});
		if (attr) {
			attr.value = attr_value;
		} else {
			this.attrs.push({
				name: attr_name,
				value: attr_value
			});
		}
		return this;
	};

	XmlNode.prototype.child = function (child_name) {
		ft.type_assert(child_name, 'string');
		var child = new XmlNode(child_name, this);
		this.children.push(child);
		return child;
	};

	XmlNode.prototype.parent = function () {
		ft.assert(this.parent !== undefined, "xml node parent is undefined");
		return this.parent;
	};

	XmlNode.prototype.content = function (indent) {
		indent = indent || "";
		ft.type_assert(indent, 'string');
		var str = '{0}<{1}'.format(indent, this.name);
		ft.array_foreach(this.attrs, function (attr) {
			str += ' {0}="{1}"'.format(attr.name, attr.value);
		});
		if (this.children.length > 0) {
			str += ">\n";
			ft.array_foreach(this.children, function (child) {
				str += child.content(indent + "\t") + "\n";
			});
			return str + "{0}<{1}/>".format(indent, this.name);
		} else {
			return str + "/>";
		}
	};

	XmlNode.prototype.save = function (xml_path) {
		if (!FLfile.write(xml_path, this.content())) {
			throw "Can't save xml to {0}!".format(xml_path);
		}
	};
	
	// ----------------------------------------------------------------------------
	//
	// BitmapAsset
	//
	// ----------------------------------------------------------------------------

	var BitmapAsset = function (item, unique_ids) {
		ft.type_assert(item, BitmapItem);
		ft.type_assert(unique_ids, UniqueIds);
		this.item      = item;
		this.uniqueIds = unique_ids;
	};

	BitmapAsset.prototype.trace = function (indent) {
		indent = indent || "";
		ft.type_assert(indent, 'string');
		ft.trace_fmt("{0}-= BitmapAsset =-", indent);
		ft.trace_fmt("{0}-Id             : {1}", indent, this.get_id());
		ft.trace_fmt("{0}-Name           : {1}", indent, this.get_name());
		ft.trace_fmt("{0}-ExportFilename : {1}", indent, this.get_export_filename());
	};

	BitmapAsset.prototype.get_id = function () {
		return this.uniqueIds.get_string_id(this.get_name());
	};

	BitmapAsset.prototype.get_name = function () {
		return this.item.name;
	};

	BitmapAsset.prototype.get_type = function () {
		return "bitmap";
	};

	BitmapAsset.prototype.get_export_filename = function () {
		return "bitmaps/{0}.png".format(this.get_id());
	};

	BitmapAsset.prototype.get_export_fullfilename = function (export_folder) {
		ft.type_assert(export_folder, 'string');
		return ft.combine_path(
			export_folder,
			this.get_export_filename());
	};

	BitmapAsset.prototype.export = function (export_folder, xml_node) {
		ft.type_assert(export_folder, 'string');
		ft.type_assert(xml_node, XmlNode);
		this.export_content(export_folder);
		this.export_description(xml_node);
	};

	BitmapAsset.prototype.export_content = function (export_folder) {
		ft.type_assert(export_folder, 'string');
		var fullfilename = this.get_export_fullfilename(export_folder);
		if (!this.item.exportToFile(fullfilename)) {
			throw "Can't export bitmap ({0})!"
				.format(fullfilename);
		}
	};

	BitmapAsset.prototype.export_description = function (xml_node) {
		ft.type_assert(xml_node, XmlNode);
		xml_node.child("bitmap")
			.attr("id"      , this.get_id())
			.attr("type"    , this.get_type())
			.attr("filename", this.get_export_filename());
	};

	// ----------------------------------------------------------------------------
	//
	// SymbolAsset
	//
	// ----------------------------------------------------------------------------

	var SymbolAsset = function (item, unique_ids) {
		ft.type_assert(item, SymbolItem);
		ft.type_assert(unique_ids, UniqueIds);
		this.item      = item;
		this.uniqueIds = unique_ids;
	};

	SymbolAsset.prototype.trace = function (indent) {
		indent = indent || "";
		ft.type_assert(indent, 'string');
		ft.trace_fmt("{0}-= SymbolAsset =-", indent);
		ft.trace_fmt("{0}-Id             : {1}", indent, this.get_id());
		ft.trace_fmt("{0}-Name           : {1}", indent, this.get_name());
		ft.trace_fmt("{0}-ExportFilename : {1}", indent, this.get_export_filename());
	};

	SymbolAsset.prototype.get_id = function () {
		return this.uniqueIds.get_string_id(this.get_name());
	};

	SymbolAsset.prototype.get_name = function () {
		return this.item.name;
	};

	SymbolAsset.prototype.get_type = function () {
		return "symbol";
	};

	SymbolAsset.prototype.get_export_filename = function () {
		return "symbols/{0}.xml".format(this.get_id());
	};

	SymbolAsset.prototype.get_export_fullfilename = function (export_folder) {
		ft.type_assert(export_folder, 'string');
		return ft.combine_path(
			export_folder,
			this.get_export_filename());
	};
	
	SymbolAsset.prototype.convert = function (document) {
		ft.type_assert(document, Document);
		new TimelineInst(this.item.timeline, this.uniqueIds)
			.convert(document);
	};

	SymbolAsset.prototype.prepare = function (document) {
		ft.type_assert(document, Document);
		new TimelineInst(this.item.timeline, this.uniqueIds)
			.prepare(document);
	};

	SymbolAsset.prototype.export = function (export_folder, xml_node) {
		ft.type_assert(export_folder, 'string');
		ft.type_assert(xml_node, XmlNode);
		this.export_content(export_folder);
		this.export_description(xml_node);
	};

	SymbolAsset.prototype.export_content = function (export_folder) {
		ft.type_assert(export_folder, 'string');
		var xml_node = new XmlNode("symbol")
			.attr("id", this.get_id());
		new TimelineInst(this.item.timeline, this.uniqueIds)
			.export_description(xml_node);
		xml_node.save(this.get_export_fullfilename(export_folder));
	};

	SymbolAsset.prototype.export_description = function (xml_node) {
		ft.type_assert(xml_node, XmlNode);
		xml_node.child("symbol")
			.attr("id"      , this.get_id())
			.attr("type"    , this.get_type())
			.attr("filename", this.get_export_filename());
	};

	// ----------------------------------------------------------------------------
	//
	// LibraryInst
	//
	// ----------------------------------------------------------------------------

	var LibraryInst = function (library, unique_ids) {
		ft.type_assert(library, Library);
		ft.type_assert(unique_ids, UniqueIds);
		this.library   = library;
		this.uniqueIds = unique_ids;
	};

	LibraryInst.prototype.is_folder_item = function (item) {
		ft.type_assert(item, LibraryItem);
		return item.itemType == "folder";
	};

	LibraryInst.prototype.is_bitmap_item = function (item) {
		ft.type_assert(item, LibraryItem);
		return item.itemType == "bitmap";
	};

	LibraryInst.prototype.is_symbol_item = function (item) {
		ft.type_assert(item, LibraryItem);
		return item.itemType == "graphic" || item.itemType == "movie clip";
	};

	LibraryInst.prototype.for_each_by_items = function (func, filter) {
		ft.type_assert(func, Function);
		ft.type_assert_if_defined(filter, Function);
		ft.array_foreach(this.library.items, func, filter);
	};
	
	LibraryInst.prototype.convert = function (document) {
		ft.type_assert(document, Document);
		this.for_each_by_items(function (item) {
			if ( this.library.editItem(item.name) ) {
				ft.trace_fmt("Convert: {0}", item.name);
				new SymbolAsset(item, this.uniqueIds)
					.convert(document);
				document.exitEditMode();
			}
		}.bind(this), this.is_symbol_item.bind(this));
	};

	LibraryInst.prototype.prepare = function (document) {
		ft.type_assert(document, Document);
		this.for_each_by_items(function (item) {
			if ( this.library.editItem(item.name) ) {
				ft.trace_fmt("Prepare: {0}", item.name);
				new SymbolAsset(item, this.uniqueIds)
					.prepare(document);
				document.exitEditMode();
			}
		}.bind(this), this.is_symbol_item.bind(this));
	};

	LibraryInst.prototype.export = function (export_folder, xml_node) {
		ft.type_assert(export_folder, 'string');
		ft.type_assert(xml_node, XmlNode);
		this.for_each_by_items(function (item) {
			if (this.is_bitmap_item(item)) {
				new BitmapAsset(item, this.uniqueIds)
					.export(export_folder, xml_node);
			} else if (this.is_symbol_item(item)) {
				new SymbolAsset(item, this.uniqueIds)
					.export(export_folder, xml_node);
			} else {
				throw "Unsupported library item type ({0})!"
					.format(item.itemType);
			}
		}.bind(this), function (item) {
			return !this.is_folder_item(item);
		}.bind(this));
	};

	// ----------------------------------------------------------------------------
	//
	// TimelineInst
	//
	// ----------------------------------------------------------------------------

	var TimelineInst = function (timeline, unique_ids) {
		ft.type_assert(timeline, Timeline);
		ft.type_assert(unique_ids, UniqueIds);
		this.timeline  = timeline;
		this.uniqueIds = unique_ids;
	};

	TimelineInst.prototype.trace = function (indent) {
		indent = indent || "";
		ft.type_assert(indent, 'string');
		ft.trace_fmt("{0}-= TimelineInst =-", indent);
		ft.trace_fmt("{0}-Id     : {1}", indent, this.get_id());
		ft.trace_fmt("{0}-Name   : {1}", indent, this.get_name());
		ft.trace_fmt("{0}-Layers : {1}", indent, this.timeline.layerCount);
		ft.trace_fmt("{0}-Frames : {1}", indent, this.timeline.frameCount);
	};

	TimelineInst.prototype.get_id = function () {
		return this.uniqueIds.get_string_id(this.get_name());
	};

	TimelineInst.prototype.get_name = function () {
		return this.timeline.name;
	};
	
	TimelineInst.prototype.remove_empty_layers = function () {
		var layers = this.timeline.layers;
		for ( var i = 0; i < layers.length; ) {
			var is_empty = new LayerInst(layers[i], this.uniqueIds).is_empty();
			if ( is_empty ) {
				this.timeline.deleteLayer(i);
				layers = this.timeline.layers;
			} else {
				++i;
			}
		}
	};
	
	TimelineInst.prototype.convert = function (document) {
		ft.type_assert(document, Document);
		this.remove_empty_layers();
		if ( this.timeline.layers.length > 1 ) {
			this.timeline.selectAllFrames();
			this.timeline.convertToKeyframes();
		}
		ft.array_foreach(this.timeline.layers, function(layer, index) {
			this.timeline.setSelectedLayers(index);
			new LayerInst(layer, this.uniqueIds)
				.convert(document, this.timeline);
		}.bind(this));
	};

	TimelineInst.prototype.prepare = function (document) {
		ft.type_assert(document, Document);
		ft.array_foreach(this.timeline.layers, function(layer, index) {
			this.timeline.setSelectedLayers(index);
			new LayerInst(layer, this.uniqueIds)
				.prepare(document, this.timeline);
		}.bind(this));
	};

	TimelineInst.prototype.export_description = function (xml_node) {
		ft.type_assert(xml_node, XmlNode);
		var timeline_node = xml_node.child("timeline")
			.attr("id", this.get_id());
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

	var LayerInst = function (layer, unique_ids) {
		ft.type_assert(layer, Layer);
		ft.type_assert(unique_ids, UniqueIds);
		this.layer     = layer;
		this.uniqueIds = unique_ids;
	};

	LayerInst.prototype.trace = function (indent) {
		indent = indent || "";
		ft.type_assert(indent, 'string');
		ft.trace_fmt("{0}-= LayerInst =-", indent);
		ft.trace_fmt("{0}-Id     : {1}"  , indent, this.get_id());
		ft.trace_fmt("{0}-Name   : {1}"  , indent, this.get_name());
		ft.trace_fmt("{0}-Frames : {1}"  , indent, this.layer.frameCount);
	};

	LayerInst.prototype.get_id = function () {
		return this.uniqueIds.get_string_id(this.get_name());
	};

	LayerInst.prototype.get_name = function () {
		return this.layer.name;
	};
	
	LayerInst.prototype.is_empty = function () {
		var frames = this.layer.frames;
		for ( var i = 0; i < frames.length; ++i ) {
			var is_empty = new FrameInst(frames[i], i, this.uniqueIds).is_empty();
			if ( !is_empty ) {
				return false;
			}
		}
		return true;
	};

	LayerInst.prototype.do_in_unlocked = function (func) {
		ft.type_assert(func, Function);
		var prev_locked    = this.layer.locked;
		var prev_visible   = this.layer.visible;
		this.layer.locked  = false;
		this.layer.visible = true;
		func();
		this.layer.locked  = prev_locked;
		this.layer.visible = prev_visible;
	};
	
	LayerInst.prototype.convert = function (document, timeline) {
		ft.type_assert(document, Document);
		ft.type_assert(timeline, Timeline);
		this.do_in_unlocked(function() {
			ft.array_foreach(this.layer.frames, function(frame, index) {
				frame.convertToFrameByFrameAnimation();
			}.bind(this));
			ft.array_foreach(this.layer.frames, function(frame, index) {
				var inst = new FrameInst(frame, index, this.uniqueIds);
				if (inst.get_start_frame() == index) {
					timeline.setSelectedFrames(index, index + 1);
					inst.convert(document, timeline, this.layer);
				}
			}.bind(this));
		}.bind(this));
	};

	LayerInst.prototype.prepare = function (document, timeline) {
		ft.type_assert(document, Document);
		ft.type_assert(timeline, Timeline);
		this.do_in_unlocked(function() {
			ft.array_foreach(this.layer.frames, function(frame, index) {
				var inst = new FrameInst(frame, index, this.uniqueIds);
				if (inst.get_start_frame() == index) {
					timeline.setSelectedFrames(index, index + 1);
					inst.prepare(document, timeline, this.layer);
				}
			}.bind(this));
		}.bind(this));
	};

	LayerInst.prototype.export_description = function (xml_node) {
		ft.type_assert(xml_node, XmlNode);
		var layer_node = xml_node.child("layer")
			.attr("id"        , this.get_id())
			.attr("visible"   , this.layer.visible)
			.attr("layer_type", this.layer.layerType);
		if (this.layer.parentLayer) {
			var parent_layer = new LayerInst(this.layer.parentLayer, this.uniqueIds);
			layer_node.attr("parent_layer", parent_layer.get_id());
		}
		ft.array_foreach(this.layer.frames, function (frame, index) {
			var inst = new FrameInst(frame, index, this.uniqueIds);
			if (inst.get_start_frame() == index) {
				inst.export_description(layer_node);
			}
		}.bind(this));
	};

	// ----------------------------------------------------------------------------
	//
	// FrameInst
	//
	// ----------------------------------------------------------------------------

	var FrameInst = function (frame, index, unique_ids) {
		ft.type_assert(frame, Frame);
		ft.type_assert(index, 'number');
		ft.type_assert(unique_ids, UniqueIds);
		this.frame     = frame;
		this.index     = index;
		this.uniqueIds = unique_ids;
	};

	FrameInst.prototype.trace = function (indent) {
		indent = indent || "";
		ft.type_assert(indent, 'string');
		ft.trace_fmt("{0}-= FrameInst =-", indent);
		ft.trace_fmt("{0}-Id       : {1}", indent, this.get_id());
		ft.trace_fmt("{0}-Name     : {1}", indent, this.get_name());
		ft.trace_fmt("{0}-Elements : {1}", indent, this.frame.elements.length);
	};

	FrameInst.prototype.get_id = function () {
		return this.uniqueIds.get_string_id(this.get_name());
	};

	FrameInst.prototype.get_name = function () {
		return this.frame.name;
	};

	FrameInst.prototype.get_index = function () {
		return this.index;
	};

	FrameInst.prototype.get_start_frame = function () {
		return this.frame.startFrame;
	};
	
	FrameInst.prototype.is_empty = function () {
		return this.frame.elements.length == 0;
	};

	FrameInst.prototype.is_element_shape = function (element) {
		return element.elementType == "shape";
	};

	FrameInst.prototype.is_element_instance = function (element) {
		return element.elementType == "instance";
	};

	FrameInst.prototype.is_element_bitmap = function (element) {
		return this.is_element_instance(element) && element.instanceType == "bitmap";
	};

	FrameInst.prototype.is_element_symbol = function (element) {
		return this.is_element_instance(element) && element.instanceType == "symbol";
	};
	
	FrameInst.prototype.convert = function (document, timeline, layer) {
		ft.type_assert(document, Document);
		ft.type_assert(timeline, Timeline);
		ft.type_assert(layer, Layer);
	};

	FrameInst.prototype.prepare = function (document, timeline, layer) {
		ft.type_assert(document, Document);
		ft.type_assert(timeline, Timeline);
		ft.type_assert(layer, Layer);
		ft.array_foreach(this.frame.elements, function (element, index) {
			if (this.is_element_shape(element)) {
				timeline.currentFrame = this.frame.startFrame;
				document.selection = [element];
				document.convertSelectionToBitmap();
			}
		}.bind(this));
	};

	FrameInst.prototype.export_element = function (xml_node, element) {
		ft.type_assert(xml_node, XmlNode);
		ft.type_assert(element, Element);
		if (this.is_element_bitmap(element)) {
			new BitmapInst(element, this.uniqueIds)
				.export_description(xml_node);
		} else if (this.is_element_symbol(element)) {
			new SymbolInst(element, this.uniqueIds)
				.export_description(xml_node);
		} else if (this.is_element_instance(element)) {
			ft.assert(false,
				"Unsupported instance type ({0})!",
				element.instanceType);
		} else {
			ft.assert(false,
				"Unsupported element type ({0})!",
				element.elementType);
		}
	};

	FrameInst.prototype.export_description = function (xml_node) {
		ft.type_assert(xml_node, XmlNode);
		var frame_node = xml_node.child("frame")
			.attr("id"      , this.get_id())
			.attr("index"   , this.get_index())
			.attr("duration", this.frame.duration);
		ft.array_foreach(this.frame.elements, function (element) {
			this.export_element(frame_node, element);
		}.bind(this));
	};

	// ----------------------------------------------------------------------------
	//
	// ElementInst
	//
	// ----------------------------------------------------------------------------

	var ElementInst = function (inst, unique_ids) {
		ft.type_assert(inst, Instance);
		ft.type_assert(unique_ids, UniqueIds);
		this.inst      = inst;
		this.uniqueIds = unique_ids;
	};

	ElementInst.prototype.trace = function (indent) {
		indent = indent || "";
		ft.type_assert(indent, 'string');
		ft.trace_fmt("{0}-= ElementInst =-", indent);
		ft.trace_fmt("{0}-Id   : {1}", indent, this.get_id());
		ft.trace_fmt("{0}-Name : {1}", indent, this.get_name());
	};

	ElementInst.prototype.get_id = function () {
		return this.uniqueIds.get_string_id(this.get_name());
	};

	ElementInst.prototype.get_name = function () {
		return this.inst.name;
	};

	ElementInst.prototype.export_description = function (xml_node) {
		ft.type_assert(xml_node, XmlNode);
		return xml_node.child("element")
			.attr("id"    , this.get_id())
			.attr("depth" , this.inst.depth)
			.attr("matrix", "{0};{1};{2};{3};{4};{5}".format(
				this.inst.matrix.a, this.inst.matrix.b,
				this.inst.matrix.c, this.inst.matrix.d,
				this.inst.matrix.tx, this.inst.matrix.ty));
	};

	// ----------------------------------------------------------------------------
	//
	// BitmapInst
	//
	// ----------------------------------------------------------------------------

	var BitmapInst = function (inst, unique_ids) {
		ElementInst.call(this, inst, unique_ids);
	};

	BitmapInst.prototype = Object.create(ElementInst.prototype);

	BitmapInst.prototype.export_description = function (xml_node) {
		ft.type_assert(xml_node, XmlNode);
		ElementInst.prototype.export_description.call(this, xml_node)
			.child("instance")
			.attr("type" , "bitmap")
			.attr("asset", this.uniqueIds.get_string_id(this.inst.libraryItem.name));
	};

	// ----------------------------------------------------------------------------
	//
	// SymbolInst
	//
	// ----------------------------------------------------------------------------

	var SymbolInst = function (inst, unique_ids) {
		ElementInst.call(this, inst, unique_ids);
	};

	SymbolInst.prototype = Object.create(ElementInst.prototype);

	SymbolInst.prototype.export_description = function (xml_node) {
		ft.type_assert(xml_node, XmlNode);
		var instance_node = ElementInst.prototype.export_description.call(this, xml_node)
			.child("instance")
			.attr("type"       , "symbol")
			.attr("symbol_type", this.inst.symbolType)
			.attr("asset"      , this.uniqueIds.get_string_id(this.inst.libraryItem.name))
			.attr("visible"    , this.inst.visible)
			.attr("blend_mode" , this.inst.blendMode);
		if (this.inst.colorMode !== "none") {
			var color_mode_node = instance_node.child("color_mode")
				.attr("color_mode", this.inst.colorMode);
			if (this.inst.colorMode == "brightness") {
				color_mode_node
					.attr("brightness", this.inst.brightness);
			} else if (this.inst.colorMode == "tint") {
				color_mode_node
					.attr("tint" , this.inst.tintPercent)
					.attr("color", this.inst.tintColor);
			} else if (this.inst.colorMode == "alpha") {
				color_mode_node
					.attr("alpha", this.inst.colorAlphaPercent);
			} else if (this.inst.colorMode == "advanced") {
				color_mode_node
					.attr("a", "{0};{1}".format(this.inst.colorAlphaAmount, this.inst.colorAlphaPercent))
					.attr("r", "{0};{1}".format(this.inst.colorRedAmount, this.inst.colorRedPercent))
					.attr("g", "{0};{1}".format(this.inst.colorGreenAmount, this.inst.colorGreenPercent))
					.attr("b", "{0};{1}".format(this.inst.colorBlueAmount, this.inst.colorBluePercent));
			} else {
				ft.assert(false,
					"Unsupported color mode ({0})!",
					this.inst.colorMode);
			}
		}
		if (this.inst.loop !== undefined && this.inst.firstFrame !== undefined) {
			instance_node.child("looping")
				.attr("loop"       , this.inst.loop)
				.attr("first_frame", this.inst.firstFrame);
		}
		if (this.inst.filters && this.inst.filters.length > 0) {
			var filters_node = instance_node.child("filters");
			ft.array_foreach(this.inst.filters, function (filter) {
				/// \TODO export filters
				filters_node.child("filter")
					.attr("name", filter.name);
			});
		}
	};

	// ----------------------------------------------------------------------------
	//
	// Exporter
	//
	// ----------------------------------------------------------------------------

	var Exporter = function (document) {
		ft.type_assert(document, Document);
		this.document     = document;
		this.uniqueIds    = new UniqueIds();
		this.documentPath = ft.escape_path(this.document.pathURI);
	};

	Exporter.prototype.trace = function (indent) {
		indent = indent || "";
		ft.type_assert(indent, 'string');
		ft.trace_fmt("{0}-= Exporter =-", indent);
		ft.trace_fmt("{0}-Document      : {1}", indent, this.document.name);
		ft.trace_fmt("{0}-Document path : {1}", indent, this.get_document_path());
		ft.trace_fmt("{0}-Export folter : {1}", indent, this.get_export_folder());
	};

	Exporter.prototype.get_document_path = function () {
		return this.documentPath;
	};

	Exporter.prototype.get_export_folder = function () {
		return ft.combine_path(
			this.get_document_path(),
			"_export/");
	};

	Exporter.prototype.get_stage_export_path = function () {
		return ft.combine_path(
			this.get_export_folder(),
			"stage.xml");
	};

	Exporter.prototype.get_library_export_path = function () {
		return ft.combine_path(
			this.get_export_folder(),
			"library.xml");
	};

	Exporter.prototype.get_strings_export_path = function () {
		return ft.combine_path(
			this.get_export_folder(),
			"strings.xml");
	};

	Exporter.prototype.export = function () {
		this.trace();
		fl.showIdleMessage(false);
		ft.trace("- Start...");
		try {
			this.prepare_folders();
			this.full_exit_edit_mode();
			this.delete_unused_items();
			this.convert_document();
			this.prepare_document();
			this.export_library();
			this.export_stage();
			this.export_strings();
			ft.trace_fmt("- Finish : {0}", this.get_export_folder());
		} catch (e) {
			ft.trace_fmt("- Error : {0}", e);
		}
		fl.revertDocument(this.document);
		fl.showIdleMessage(true);
	};

	Exporter.prototype.prepare_folders = function () {
		var export_folder = this.get_export_folder();
		if (FLfile.exists(export_folder)) {
			if (!FLfile.remove(export_folder)) {
				throw "Can't remove document export folder ({0})!"
					.format(export_folder);
			}
		}
		if (!FLfile.createFolder(export_folder)) {
			throw "Can't create document export folder ({0})!"
				.format(export_folder);
		}
		if (!FLfile.createFolder(export_folder + "bitmaps/")) {
			throw "Can't create document bitmaps export folder ({0})!"
				.format(export_folder);
		}
		if (!FLfile.createFolder(export_folder + "symbols/")) {
			throw "Can't create document symbols export folder ({0})!"
				.format(export_folder);
		}
	};
	
	Exporter.prototype.full_exit_edit_mode = function () {
		for (var i = 0; i < 100; ++i) {
			this.document.exitEditMode();
		}
	};
	
	Exporter.prototype.delete_unused_items = function() {
		var unused_items = this.document.library.unusedItems;
		ft.array_foreach(unused_items, function (item) {
			ft.trace_fmt("Remove unused item: {0}", item.name);
			this.document.library.deleteItem(item.name);
		});
	};
	
	Exporter.prototype.convert_document = function () {
		new TimelineInst(this.document.getTimeline(), this.uniqueIds)
			.convert(this.document);
		new LibraryInst(this.document.library, this.uniqueIds)
			.convert(this.document);
	};

	Exporter.prototype.prepare_document = function () {
		new TimelineInst(this.document.getTimeline(), this.uniqueIds)
			.prepare(this.document);
		new LibraryInst(this.document.library, this.uniqueIds)
			.prepare(this.document);
	};

	Exporter.prototype.export_library = function () {
		var xml_node = new XmlNode("library")
			.attr("frame_rate", this.document.frameRate);
		new LibraryInst(this.document.library, this.uniqueIds)
			.export(this.get_export_folder(), xml_node);
		xml_node.save(this.get_library_export_path());
	};

	Exporter.prototype.export_stage = function () {
		var xml_node = new XmlNode("stage");
		new TimelineInst(this.document.getTimeline(), this.uniqueIds)
			.export_description(xml_node);
		xml_node.save(this.get_stage_export_path(document));
	};

	Exporter.prototype.export_strings = function () {
		this.uniqueIds.save(this.get_strings_export_path());
	};

	// ------------------------------------
	// Main
	// ------------------------------------

	(function () {
		ft.clear_output();
		ft.array_foreach(fl.documents, function (document) {
			new Exporter(document).export();
		});
	})();
})();