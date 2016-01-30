// ------------------------------------
// JS core
// ------------------------------------

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

(function() {

	// ------------------------------------
	// ft
	// ------------------------------------

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
		for ( var i = 0; i < arr.length; ++i ) {
			var val = arr[i];
			if ( !filter || filter(val) ) {
				func(val);
			}
		}
	};

	ft.object_foreach = function(obj, func, filter) {
		ft.type_assert(obj, 'object');
		ft.type_assert(func, Function);
		ft.type_assert_if_defined(filter, Function);
		for ( var key in obj ) {
			if ( obj.hasOwnProperty(key) ) {
				var val = obj[key];
				if ( !filter || filter(key, val) ) {
					func(key, val);
				}
			}
		}
	};

	// ------------------------------------
	// UniqueIds
	// ------------------------------------

	var UniqueIds = function() {
		this.clear();
	};

	UniqueIds.prototype.clear = function() {
		this.stringIds    = {};
		this.lastStringId = 0;
	};

	UniqueIds.prototype.get_string_id = function(str) {
		ft.type_assert(str, 'string');
		var id = this.stringIds[str];
		if ( id === undefined ) {
			this.stringIds[str] = ++this.lastStringId;
			return this.lastStringId;
		} else {
			return id;
		}
	};

	UniqueIds.prototype.save = function(xml_path) {
		ft.type_assert(xml_path, 'string');
		var xml_node = new XmlNode("strings");
		ft.object_foreach(this.stringIds, function(key, val) {
			xml_node.child("string")
				.attr("id" , val)
				.attr("str", ft.escape_string(key));
		});
		xml_node.save(xml_path);
	};

	// ------------------------------------
	// XmlNode
	// ------------------------------------

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

	XmlNode.prototype.content = function(indent) {
		indent = indent || "";
		var str = '{0}<{1}'.format(indent, this.name);
		ft.array_foreach(this.attrs, function(attr) {
			str += ' {0}="{1}"'.format(attr.name, attr.value);
		});
		if ( this.children.length > 0 ) {
			str += ">\n";
			ft.array_foreach(this.children, function(child) {
				str += child.content(indent + "  ") + "\n";
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
	// Bitmap item
	// ------------------------------------

	FlashTools.prototype.BitmapItem_TraceInfo = function(item) {
		ft.type_assert(item, BitmapItem);
		ft.trace_fmt("Name           : {0}", item.name);
		ft.trace_fmt("ExportFilename : {0}", this.BitmapItem_GetExportFilename(item));
	};

	FlashTools.prototype.BitmapItem_GetExportFilename = function(item) {
		ft.type_assert(item, BitmapItem);
		var item_id = this.uniqueIds.get_string_id(item.name);
		return "bitmaps/{0}.png".format(item_id);
	};

	FlashTools.prototype.BitmapItem_GetExportFullFilename = function(document, item) {
		ft.type_assert(document, Document);
		ft.type_assert(item, BitmapItem);
		return ft.combine_path(
			this.Document_GetExportFolder(document),
			this.BitmapItem_GetExportFilename(item));
	};

	FlashTools.prototype.BitmapItem_Export = function(document, item) {
		ft.type_assert(document, Document);
		ft.type_assert(item, BitmapItem);
		var item_export_path = this.BitmapItem_GetExportFullFilename(document, item);
		if ( !item.exportToFile(item_export_path) ) {
			throw "Can't export bitmap ({0})!"
				.format(item_export_path);
		}
	};

	FlashTools.prototype.BitmapItem_ExportXmlDescription = function(xml_node, item) {
		ft.type_assert(xml_node, XmlNode);
		ft.type_assert(item, BitmapItem);
		xml_node.child("bitmap")
			.attr("name"    , this.uniqueIds.get_string_id(item.name))
			.attr("type"    , "bitmap")
			.attr("filename", this.BitmapItem_GetExportFilename(item));
	};

	// ------------------------------------
	// Symbol item
	// ------------------------------------

	FlashTools.prototype.SymbolItem_TraceInfo = function(item) {
		ft.type_assert(item, SymbolItem);
		ft.trace_fmt("Name           : {0}", item.name);
		ft.trace_fmt("ExportFilename : {0}", this.SymbolItem_GetExportFilename(item));
	};

	FlashTools.prototype.SymbolItem_GetExportFilename = function(item) {
		ft.type_assert(item, SymbolItem);
		var item_id = this.uniqueIds.get_string_id(item.name);
		return "symbols/{0}.xml".format(item_id);
	};

	FlashTools.prototype.SymbolItem_GetExportFullFilename = function(document, item) {
		ft.type_assert(item, SymbolItem);
		return ft.combine_path(
			this.Document_GetExportFolder(document),
			this.SymbolItem_GetExportFilename(item));
	};

	FlashTools.prototype.SymbolItem_Export = function(document, item) {
		ft.type_assert(document, Document);
		ft.type_assert(item, SymbolItem);
		var xml_node = new XmlNode("symbol")
			.attr("name", this.uniqueIds.get_string_id(item.name));
		this.Timeline_ExportXmlContent(xml_node, item.timeline);
		xml_node.save(this.SymbolItem_GetExportFullFilename(document, item));
	};

	FlashTools.prototype.SymbolItem_ExportXmlDescription = function(xml_node, item) {
		ft.type_assert(xml_node, XmlNode);
		ft.type_assert(item, SymbolItem);
		xml_node.child("symbol")
			.attr("name"    , this.uniqueIds.get_string_id(item.name))
			.attr("type"    , "symbol")
			.attr("filename", this.SymbolItem_GetExportFilename(item));
	};
	
	// ------------------------------------
	// Bitmap
	// ------------------------------------
	
	FlashTools.prototype.Bitmap_ExportXmlContent = function(xml_node, bitmap) {
		xml_node.child("asset")
			.attr("name", this.uniqueIds.get_string_id(bitmap.libraryItem.name))
			.attr("type", "bitmap");
	};
	
	// ------------------------------------
	// Symbol
	// ------------------------------------
	
	FlashTools.prototype.Symbol_ExportXmlContent = function(xml_node, symbol) {
		var asset_node = xml_node.child("asset")
			.attr("name", this.uniqueIds.get_string_id(symbol.libraryItem.name))
			.attr("type", "symbol");

		asset_node
			.attr("color_mode", symbol.colorMode);
		
		if ( symbol.colorAlphaPercent ) {
			asset_node.attr("alpha", symbol.colorAlphaPercent / 100.0);
		}
		if ( symbol.blendMode ) {
			/// \TODO check blend mode
			asset_node.attr("blend", symbol.blendMode);
		}
		if ( symbol.colorMode == "brightness" ) {
			asset_node.attr("brightness", symbol.brightness);
		}
	};

	// ------------------------------------
	// Element
	// ------------------------------------

	FlashTools.prototype.Element_ExportXmlContent = function(xml_node, element) {
		ft.type_assert(xml_node, XmlNode);
		ft.type_assert(element, Element);

		var element_node = xml_node.child("element")
			.attr("name" , this.uniqueIds.get_string_id(element.name))
			.attr("depth", element.depth);
		this.ElementMatrix_ExportXmlContent(element_node, element);
		
		if ( element.elementType == "shape" ) {
			/// \TODO: shape to bitmap
		} else if ( element.elementType == "instance" ) {
			if ( element.instanceType == "bitmap" ) {
				this.Bitmap_ExportXmlContent(element_node, element);
			} else if ( element.instanceType == "symbol" ) {
				this.Symbol_ExportXmlContent(element_node, element);
			} else {
				throw "Unsupported element instance type ({0})!".format(
					element.instanceType);
			}
		} else {
			throw "Unsupported element type ({0})!".format(
				element.elementType);
		}
	};

	FlashTools.prototype.ElementMatrix_ExportXmlContent = function(xml_node, element) {
		ft.type_assert(xml_node, XmlNode);
		ft.type_assert(element, Element);
		xml_node.child("matrix")
			.attr("a" , element.matrix.a)
			.attr("b" , element.matrix.b)
			.attr("c" , element.matrix.c)
			.attr("d" , element.matrix.d)
			.attr("tx", element.matrix.tx)
			.attr("ty", element.matrix.ty);
	};

	// ------------------------------------
	// Frame
	// ------------------------------------

	FlashTools.prototype.Frame_ExportXmlContent = function(xml_node, frame) {
		ft.type_assert(xml_node, XmlNode);
		ft.type_assert(frame, Frame);
		var frame_node = xml_node.child("frame")
			.attr("name"       , this.uniqueIds.get_string_id(frame.name))
			.attr("start_frame", frame.startFrame)
			.attr("duration"   , frame.duration)
			.attr("tween_type" , frame.tweenType)
			.attr("elements"   , frame.elements.length);
		this.FrameElements_ExportXmlContent(frame_node, frame);
		
		if ( frame.isMotionObject() ) {
			ft.trace("!!!");
		}
	};

	FlashTools.prototype.FrameElements_ExportXmlContent = function(xml_node, frame) {
		ft.type_assert(xml_node, XmlNode);
		ft.type_assert(frame, Frame);
		ft.array_foreach(frame.elements, function(element) {
			this.Element_ExportXmlContent(xml_node, element);
		}.bind(this));
	};

	// ------------------------------------
	// Layer
	// ------------------------------------

	FlashTools.prototype.Layer_ExportXmlContent = function(xml_node, layer) {
		ft.type_assert(xml_node, XmlNode);
		ft.type_assert(layer, Layer);
		var layer_node = xml_node.child("layer")
			.attr("name"          , this.uniqueIds.get_string_id(layer.name))
			.attr("type"          , layer.layerType)
			.attr("frames"        , layer.frameCount)
			.attr("locked"        , layer.locked)
			.attr("visible"       , layer.visible)
			.attr("animation_type", layer.animationType);
		if ( layer.parentLayer ) {
			layer_node.attr("parent_layer", this.uniqueIds.get_string_id(layer.parentLayer.name));
		}
		this.LayerFrames_ExportXmlContent(layer_node, layer);
	};

	FlashTools.prototype.LayerFrames_ExportXmlContent = function(xml_node, layer) {
		ft.type_assert(xml_node, XmlNode);
		ft.type_assert(layer, Layer);
		ft.array_foreach(layer.frames, function(frame) {
			this.Frame_ExportXmlContent(xml_node, frame);
		}.bind(this));
	};

	// ------------------------------------
	// Timeline
	// ------------------------------------

	FlashTools.prototype.Timeline_TraceInfo = function(timeline) {
		ft.type_assert(timeline, Timeline);
		ft.trace_fmt("Name        : {0}", timeline.name);
		ft.trace_fmt("Layer count : {0}", timeline.layerCount);
		ft.trace_fmt("Frame count : {0}", timeline.frameCount);
	};

	FlashTools.prototype.Timeline_ExportXmlContent = function(xml_node, timeline) {
		ft.type_assert(xml_node, XmlNode);
		ft.type_assert(timeline, Timeline);
		var timeline_node = xml_node.child("timeline")
			.attr("layers", timeline.layerCount)
			.attr("frames", timeline.frameCount);
		this.TimelineLayers_ExportXmlContent(timeline_node, timeline);
	};

	FlashTools.prototype.TimelineLayers_ExportXmlContent = function(xml_node, timeline) {
		ft.type_assert(xml_node, XmlNode);
		ft.type_assert(timeline, Timeline);
		ft.array_foreach(timeline.layers, function(layer) {
			this.Layer_ExportXmlContent(xml_node, layer);
		}.bind(this));
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
				this.BitmapItem_ExportXmlDescription(xml_node, item);
			} else if ( this.IsSymbolLibraryItem(item) ) {
				this.SymbolItem_ExportXmlDescription(xml_node, item);
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
			this.BitmapItem_Export(document, item);
		}.bind(this), this.IsBitmapLibraryItem.bind(this));
	};

	FlashTools.prototype.Document_ExportSymbols = function(document) {
		ft.type_assert(document, Document);
		this.Document_ForEachByLibraryItems(document, function(item) {
			this.SymbolItem_Export(document, item);
		}.bind(this), this.IsSymbolLibraryItem.bind(this));
	};

	FlashTools.prototype.Document_ExportStage = function(document) {
		ft.type_assert(document, Document);
		this.Document_ExitEditMode(document);
		var xml_node = new XmlNode("stage");
		this.Timeline_ExportXmlContent(xml_node, document.getTimeline());
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
