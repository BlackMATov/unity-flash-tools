// ------------------------------------
// JS functions
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
		return fToBind.apply(this instanceof fNOP && oThis
			? this
			: oThis,
			aArgs.concat(Array.prototype.slice.call(arguments)));
		};
		fNOP.prototype = this.prototype;
		fBound.prototype = new fNOP();
		return fBound;
	};
}

// ------------------------------------
// FlashTools 
// ------------------------------------

var FlashTools = function() {
	this.defaultIndent = "  ";
	this.ClearStringIds();
};

// ------------------------------------
// Common functions
// ------------------------------------

FlashTools.prototype.Trace = function(text) {
	this.TypeAssert(text, 'string');
	fl.outputPanel.trace(text);
};

FlashTools.prototype.TraceError = function(text) {
	this.TypeAssert(text, 'string');
	this.Trace("!!!Error!!!: " + text);
};

FlashTools.prototype.Assert = function(expr, msg) {
	if ( !expr ) {
		throw msg != undefined ? "Assert! " + msg : "Assert!";
	}
};

FlashTools.prototype.TypeAssert = function(item, type) {
	this.Assert(
		(typeof type === 'string' && typeof item === type) || (item instanceof type),
		"Type error: {0} != {1}".format(typeof item, type.name));
};

FlashTools.prototype.TypeAssertIfDefined = function(item, type) {
	if ( item != undefined ) {
		this.TypeAssert(item, type);
	}
};

FlashTools.prototype.ClearOutput = function() {
	fl.outputPanel.clear();
};

FlashTools.prototype.EscapePath = function(path) {
	this.TypeAssert(path, 'string');
	return path.replace(/ /g, '%20');
};

FlashTools.prototype.EscapeString = function(str) {
	this.TypeAssert(str, 'string');
	return str
		.replace(/\&/g, '&amp;')
		.replace(/\"/g, '&quot;')
		.replace(/\'/g, '&apos;')
		.replace(/\</g, '&lt;')
		.replace(/\>/g, '&gt;');
};

FlashTools.prototype.CombinePath = function(lhs, rhs) {
	this.TypeAssert(lhs, 'string');
	this.TypeAssert(rhs, 'string');
	return this.EscapePath(lhs) + this.EscapePath(rhs);
};

FlashTools.prototype.ClearStringIds = function() {
	this.stringIds    = {};
	this.lastStringId = 0;
};

FlashTools.prototype.GetStringId = function(str) {
	this.TypeAssert(str, 'string');
	var id = this.stringIds[str];
	if ( id == undefined ) {
		this.stringIds[str] = ++this.lastStringId;
		return this.lastStringId;
	} else {
		return id;
	}
};

FlashTools.prototype.ExportStringIdsXmlContent = function() {
	var xml_content = "<strings>\n";
	for ( var str in this.stringIds ) {
		if ( this.stringIds.hasOwnProperty(str) ) {
			xml_content += '{0}<string id="{1}" str="{2}"/>\n'.format(
				this.defaultIndent,
				this.stringIds[str],
				this.EscapeString(str));
		}
	}
	return xml_content + "</strings>";
};

// ------------------------------------
// Xml functions
// ------------------------------------

FlashTools.prototype.AssertTypeXmlNode = function(xml_node) {
	this.Assert(
		xml_node != undefined &&
		xml_node.IsXmlNode != undefined &&
		xml_node.IsXmlNode(),
		"Type error: {0} != {1}".format(typeof xml_node, "XmlNode"));
};

FlashTools.prototype.XmlNode = function(node_name) {
	var Assert              = this.Assert.bind(this);
	var TypeAssert          = this.TypeAssert.bind(this);
	var TypeAssertIfDefined = this.TypeAssertIfDefined.bind(this);
	var EscapeString        = this.EscapeString.bind(this);
	var DefaultIndent       = this.defaultIndent;
	
	var Ctor = function(node_name, node_parent) {
		TypeAssert(node_name, 'string');
		TypeAssertIfDefined(node_parent, Ctor);
		this.name     = node_name;
		this.parent   = node_parent;
		this.attrs    = [];
		this.children = [];
	};
	
	Ctor.prototype.IsXmlNode = function() {
		return true;
	};
	
	Ctor.prototype.Attr = function(attr_name, attr_value) {
		TypeAssert(attr_name, 'string');
		attr_value = EscapeString(attr_value.toString());
		for ( var i = 0; i < this.attrs.length; ++i ) {
			var attr = this.attrs[i];
			if ( attr.name == attr_name ) {
				attr.value = attr_value;
				return this;
			}
		}
		this.attrs.push({name:attr_name, value:attr_value});
		return this;
	};
	
	Ctor.prototype.Child = function(child_name) {
		TypeAssert(child_name, 'string');
		var child = new Ctor(child_name, this);
		this.children.push(child);
		return child;
	};
	
	Ctor.prototype.Parent = function() {
		Assert(this.parent != undefined, "node parent is undefined");
		return this.parent;
	};
	
	Ctor.prototype.Content = function(indent) {
		indent = indent == undefined ? "" : indent;
		var str = '{0}<{1}'.format(indent, this.name);
		if ( this.attrs.length > 0 ) {
			for ( var i = 0; i < this.attrs.length; ++i ) {
				var attr = this.attrs[i];
				str += ' {0}="{1}"'.format(attr.name, attr.value);
			}
		}
		if ( this.children.length > 0 ) {
			str += ">\n";
			for ( var i = 0; i < this.children.length; ++i ) {
				var child = this.children[i];
				str += child.Content(indent + DefaultIndent) + "\n";
			}
			return str + "{0}<{1}/>".format(indent, this.name);
		} else {
			return str + "/>";
		}
	};
	
	return new Ctor(node_name);
};

// ------------------------------------
// Library item functions
// ------------------------------------

FlashTools.prototype.IsFolderLibraryItem = function(item) {
	this.TypeAssert(item, LibraryItem);
	return item.itemType == "folder";
};

FlashTools.prototype.IsBitmapLibraryItem = function(item) {
	this.TypeAssert(item, LibraryItem);
	return item.itemType == "bitmap";
};

FlashTools.prototype.IsSymbolLibraryItem = function(item) {
	this.TypeAssert(item, LibraryItem);
	var item_type = item.itemType;
	return item_type == "graphic" || item_type == "component" || item_type == "movie clip";
};

// ------------------------------------
// Bitmap item functions
// ------------------------------------

FlashTools.prototype.BitmapItem_TraceInfo = function(item) {
	this.TypeAssert(item, BitmapItem);
	this.Trace("{0}Name           : {1}".format(this.defaultIndent, item.name));
	this.Trace("{0}ExportFilename : {1}".format(this.defaultIndent, this.BitmapItem_GetExportFilename(item)));
};

FlashTools.prototype.BitmapItem_GetExportFilename = function(item) {
	this.TypeAssert(item, BitmapItem);
	var item_id = this.GetStringId(item.name);
	return "bitmaps/{0}.png".format(item_id);
};

FlashTools.prototype.BitmapItem_GetExportFullFilename = function(document, item) {
	this.TypeAssert(document, Document);
	this.TypeAssert(item, BitmapItem);
	return this.CombinePath(
		this.Document_GetExportFolder(document),
		this.BitmapItem_GetExportFilename(item));
};

FlashTools.prototype.BitmapItem_Export = function(document, item) {
	this.TypeAssert(document, Document);
	this.TypeAssert(item, BitmapItem);
	var item_export_path = this.BitmapItem_GetExportFullFilename(document, item);
	if ( !item.exportToFile(item_export_path) ) {
		throw "Can't export bitmap ({0})!"
			.format(item_export_path);
	}
};

FlashTools.prototype.BitmapItem_ExportXmlDescription = function(xml_node, item) {
	this.AssertTypeXmlNode(xml_node);
	this.TypeAssert(item, BitmapItem);
	xml_node.Child("asset")
		.Attr("name"    , this.GetStringId(item.name))
		.Attr("type"    , "bitmap")
		.Attr("filename", this.BitmapItem_GetExportFilename(item));
};

// ------------------------------------
// Symbol item functions
// ------------------------------------

FlashTools.prototype.SymbolItem_TraceInfo = function(item) {
	this.TypeAssert(item, SymbolItem);
	this.Trace("{0}Name           : {1}".format(this.defaultIndent, item.name));
	this.Trace("{0}ExportFilename : {1}".format(this.defaultIndent, this.SymbolItem_GetExportFilename(item)));
};

FlashTools.prototype.SymbolItem_GetExportFilename = function(item) {
	this.TypeAssert(item, SymbolItem);
	var item_id = this.GetStringId(item.name);
	return "symbols/{0}.xml".format(item_id);
};

FlashTools.prototype.SymbolItem_GetExportFullFilename = function(document, item) {
	this.TypeAssert(item, SymbolItem);
	return this.CombinePath(
		this.Document_GetExportFolder(document),
		this.SymbolItem_GetExportFilename(item));
};

FlashTools.prototype.SymbolItem_Export = function(document, item) {
	this.TypeAssert(document, Document);
	this.TypeAssert(item, SymbolItem);
	var xml_node = this.XmlNode("symbol")
		.Attr("name", this.GetStringId(item.name));
	this.Timeline_ExportXmlContent(xml_node, item.timeline);
	var item_export_path = this.SymbolItem_GetExportFullFilename(document, item);
	if ( !FLfile.write(item_export_path, xml_node.Content()) ) {
		throw "Can't create symbol ({0})!"
			.format(item_export_path);
	}
};

FlashTools.prototype.SymbolItem_ExportXmlDescription = function(xml_node, item) {
	this.AssertTypeXmlNode(xml_node);
	this.TypeAssert(item, SymbolItem);
	xml_node.Child("symbol")
		.Attr("name"    , this.GetStringId(item.name))
		.Attr("type"    , "symbol")
		.Attr("filename", this.SymbolItem_GetExportFilename(item));
};

// ------------------------------------
// Element functions
// ------------------------------------

FlashTools.prototype.Element_ExportXmlContent = function(xml_node, element) {
	this.AssertTypeXmlNode(xml_node);
	this.TypeAssert(element, Element);
	
	if ( element.elementType == "shape" ) {
		/// \TODO: shape to bitmap
	} else if ( element.elementType == "instance" ) {
		if ( element.instanceType == "bitmap" ) {
		} else if ( element.instanceType == "symbol" ) {
		} else {
			throw "Unsupported element type ({0})!"
				.format(element.elementType);
		}
		//this.Trace("Instance type : " + element.instanceType);
		//this.Trace("Library item  : " + element.libraryItem.name);
	} else {
		throw "Unsupported element type ({0})!"
			.format(element.elementType);
	}
	
	var element_node = xml_node.Child("element")
		.Attr("name" , this.GetStringId(element.name))
		.Attr("depth", element.depth);
	this.ElementTransform_ExportXmlContent(element_node, element);
};

FlashTools.prototype.ElementTransform_ExportXmlContent = function(xml_node, element) {
	this.AssertTypeXmlNode(xml_node);
	this.TypeAssert(element, Element);
	xml_node.Child("transform")
		.Attr("a" , element.matrix.a)
		.Attr("b" , element.matrix.b)
		.Attr("c" , element.matrix.c)
		.Attr("d" , element.matrix.d)
		.Attr("tx", element.matrix.tx)
		.Attr("ty", element.matrix.ty);
};

// ------------------------------------
// Frame functions
// ------------------------------------

FlashTools.prototype.Frame_ExportXmlContent = function(xml_node, frame) {
	this.AssertTypeXmlNode(xml_node);
	this.TypeAssert(frame, Frame);
	var frame_node = xml_node.Child("frame")
		.Attr("name"       , this.GetStringId(frame.name))
		.Attr("start_frame", frame.startFrame)
		.Attr("duration"   , frame.duration)
		.Attr("elements"   , frame.elements.length);
	this.FrameElements_ExportXmlContent(frame_node, frame);
};

FlashTools.prototype.FrameElements_ExportXmlContent = function(xml_node, frame) {
	this.AssertTypeXmlNode(xml_node);
	this.TypeAssert(frame, Frame);
	for ( var i = 0; i < frame.elements.length; ++i ) {
		var element = frame.elements[i];
		this.Element_ExportXmlContent(xml_node, element);
	}
};

// ------------------------------------
// Layer functions
// ------------------------------------

FlashTools.prototype.Layer_ExportXmlContent = function(xml_node, layer) {
	this.AssertTypeXmlNode(xml_node);
	this.TypeAssert(layer, Layer);
	var layer_node = xml_node.Child("layer")
		.Attr("name"          , this.GetStringId(layer.name))
		.Attr("type"          , layer.layerType)
		.Attr("frames"        , layer.frameCount)
		.Attr("locked"        , layer.locked)
		.Attr("visible"       , layer.visible)
		.Attr("animation_type", layer.animationType);
	if ( layer.parentLayer ) {
		layer_node.Attr("parent_layer", this.GetStringId(layer.parentLayer.name));
	}
	this.LayerFrames_ExportXmlContent(layer_node, layer);
};

FlashTools.prototype.LayerFrames_ExportXmlContent = function(xml_node, layer) {
	this.AssertTypeXmlNode(xml_node);
	this.TypeAssert(layer, Layer);
	for ( var i = 0; i < layer.frames.length; ++i ) {
		var frame = layer.frames[i];
		this.Frame_ExportXmlContent(xml_node, frame);
	}
};

// ------------------------------------
// Timeline functions
// ------------------------------------

FlashTools.prototype.Timeline_TraceInfo = function(timeline) {
	this.TypeAssert(timeline, Timeline);
	this.Trace("{0}Name        : {1}".format(this.defaultIndent, timeline.name));
	this.Trace("{0}Layer count : {1}".format(this.defaultIndent, timeline.layerCount));
	this.Trace("{0}Frame count : {1}".format(this.defaultIndent, timeline.frameCount));
};

FlashTools.prototype.Timeline_ExportXmlContent = function(xml_node, timeline) {
	this.AssertTypeXmlNode(xml_node);
	this.TypeAssert(timeline, Timeline);
	var timeline_node = xml_node.Child("timeline")
		.Attr("layers", timeline.layerCount)
		.Attr("frames", timeline.frameCount);
	this.TimelineLayers_ExportXmlContent(timeline_node, timeline);
};

FlashTools.prototype.TimelineLayers_ExportXmlContent = function(xml_node, timeline) {
	this.AssertTypeXmlNode(xml_node);
	this.TypeAssert(timeline, Timeline);
	for ( var i = 0; i < timeline.layers.length; ++i ) {
		var layer = timeline.layers[i];
		this.Layer_ExportXmlContent(xml_node, layer);
	}
};

// ------------------------------------
// Document functions
// ------------------------------------

FlashTools.prototype.Document_TraceInfo = function(document) {
	this.TypeAssert(document, Document);
	this.Trace("{0}Name         : {1}".format(this.defaultIndent, document.name));
	this.Trace("{0}Path         : {1}".format(this.defaultIndent, this.Document_GetPath(document)));
	this.Trace("{0}ExportFolder : {1}".format(this.defaultIndent, this.Document_GetExportFolder(document)));
};

FlashTools.prototype.Document_GetPath = function(document) {
	this.TypeAssert(document, Document);
	return this.EscapePath(document.pathURI);
};

FlashTools.prototype.Document_GetExportFolder = function(document) {
	this.TypeAssert(document, Document);
	return this.Document_GetPath(document) + "_export/";
};

FlashTools.prototype.Document_GetStageExportPath = function(document) {
	this.TypeAssert(document, Document);
	return this.Document_GetExportFolder(document) + "stage.xml";
};

FlashTools.prototype.Document_GetLibraryExportPath = function(document) {
	this.TypeAssert(document, Document);
	return this.Document_GetExportFolder(document) + "library.xml";
};

FlashTools.prototype.Document_GetStringIdsExportPath = function(document) {
	this.TypeAssert(document, Document);
	return this.Document_GetExportFolder(document) + "strings.xml";
};

FlashTools.prototype.Document_ExitEditMode = function(document) {
	this.TypeAssert(document, Document);
	for ( var i = 0; i < 100; ++i ) {
		document.exitEditMode();
	}
};

FlashTools.prototype.Document_ForEachByLibraryItems = function(document, func, filter_func) {
	this.TypeAssert(document, Document);
	this.TypeAssert(func, 'function');
	this.TypeAssertIfDefined(filter_func, 'function');
	for ( var i = 0; i < document.library.items.length; ++i ) {
		var item = document.library.items[i];
		if ( filter_func == undefined || filter_func(item) ) {
			func(item);
		}
	}
};

FlashTools.prototype.Document_PrepareExportFolder = function(document) {
	this.TypeAssert(document, Document);
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
};

FlashTools.prototype.Document_ExportLibrary = function(document) {
	this.TypeAssert(document, Document);
	var xml_node = this.XmlNode("library")
		.Attr("frame_rate", document.frameRate);
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
	var library_path = this.Document_GetLibraryExportPath(document);
	if ( !FLfile.write(library_path, xml_node.Content()) ) {
		throw "Can't create library xml ({0})!"
			.format(library_path);
	}
};

FlashTools.prototype.Document_ExportBitmaps = function(document) {
	this.TypeAssert(document, Document);
	this.Document_ForEachByLibraryItems(document, function(item) {
		this.BitmapItem_Export(document, item);
	}.bind(this), this.IsBitmapLibraryItem.bind(this));
};

FlashTools.prototype.Document_ExportSymbols = function(document) {
	this.TypeAssert(document, Document);
	this.Document_ForEachByLibraryItems(document, function(item) {
		this.SymbolItem_Export(document, item);
	}.bind(this), this.IsSymbolLibraryItem.bind(this));
};

FlashTools.prototype.Document_ExportStage = function(document) {
	this.TypeAssert(document, Document);
	this.Document_ExitEditMode(document);
	var xml_node = this.XmlNode("stage");
	this.Timeline_ExportXmlContent(xml_node, document.getTimeline());
	var stage_path = this.Document_GetStageExportPath(document);
	if ( !FLfile.write(stage_path, xml_node.Content()) ) {
		throw "Can't create stage xml ({0})!"
			.format(stage_path);
	}
};

FlashTools.prototype.Document_ExportStringIds = function(document) {
	this.TypeAssert(document, Document);
	var xml_content = this.ExportStringIdsXmlContent();
	var xml_path = this.Document_GetStringIdsExportPath(document);
	if ( !FLfile.write(xml_path, xml_content) ) {
		throw "Can't create string ids xml ({0})!"
			.format(xml_path);
	}
};

// ------------------------------------
// Convert functions
// ------------------------------------

FlashTools.prototype.ConvertAll = function() {
	var documents = fl.documents;
	for ( var i = 0; i < documents.length; ++i ) {
		this.ConvertOne(documents[i]);
	}
};

FlashTools.prototype.ConvertOne = function(document) {
	this.TypeAssert(document, Document);
	this.ClearStringIds();
	this.Trace("-= Convert document start =-");
	try {
		this.Document_TraceInfo(document);
		this.Document_PrepareExportFolder(document);
		this.Document_ExportLibrary(document);
		this.Document_ExportBitmaps(document);
		this.Document_ExportSymbols(document);
		this.Document_ExportStage(document);
		this.Document_ExportStringIds(document);
		this.Trace("-= Convert document finish =-");
	} catch ( e ) {
		this.Trace("-= Convert document error =- : " + e);
	}
};

// ------------------------------------
// Test functions
// ------------------------------------

FlashTools.prototype.Test0 = function() {
	this.Assert(true);
};

FlashTools.prototype.Test1 = function() {
	this.Assert(true);
};

FlashTools.prototype.RunTests = function() {
	try {
		this.Test0();
		this.Test1();
		return true;
	} catch ( e ) {
		this.TraceError("Unit test fail: " + e);
		return false;
	}
};

// ------------------------------------
// Run
// ------------------------------------

var ft = new FlashTools();
ft.ClearOutput();
if ( ft.RunTests() ) {
	ft.ConvertAll();
}

