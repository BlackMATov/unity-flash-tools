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
	this.uniqueIdsMap = {};
	this.uniqueLastId = 0;
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

FlashTools.prototype.ClearOutput = function() {
	fl.outputPanel.clear();
};

FlashTools.prototype.EscapePath = function(path) {
	this.TypeAssert(path, 'string');
	return path.replace(/ /g, '%20');
};

FlashTools.prototype.CombinePath = function(lhs, rhs) {
	this.TypeAssert(lhs, 'string');
	this.TypeAssert(rhs, 'string');
	return this.EscapePath(lhs) + this.EscapePath(rhs);
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

FlashTools.prototype.GetUniqueItemId = function(item) {
	this.TypeAssert(item, LibraryItem);
	var unique_item_name = "unique_item_name_" + item.name;
	var unique_id_for_name = this.uniqueIdsMap[unique_item_name];
	if ( unique_id_for_name == undefined ) {
		this.uniqueIdsMap[unique_item_name] = ++this.uniqueLastId;
		return this.uniqueLastId;
	} else {
		return unique_id_for_name;
	}
};

// ------------------------------------
// Bitmap item functions
// ------------------------------------

FlashTools.prototype.BitmapItem_TraceInfo = function(item) {
	this.TypeAssert(item, BitmapItem);
	this.Trace("  Name           : " + item.name);
	this.Trace("  ExportFilename : " + this.BitmapItem_GetExportFilename(item));
};

FlashTools.prototype.BitmapItem_GetExportFilename = function(item) {
	this.TypeAssert(item, BitmapItem);
	var item_id = this.GetUniqueItemId(item);
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

FlashTools.prototype.BitmapItem_GetLibraryXmlDescription = function(item, indent) {
	indent = indent == undefined ? "" : indent,
	this.TypeAssert(item, BitmapItem);
	this.TypeAssert(indent, 'string');
	return '{0}<asset name="{1}" type="{2}" filename="{3}"/>\n'.format(
		indent,
		this.GetUniqueItemId(item),
		"bitmap",
		this.BitmapItem_GetExportFilename(item));
};

// ------------------------------------
// Symbol item functions
// ------------------------------------

FlashTools.prototype.SymbolItem_TraceInfo = function(item) {
	this.TypeAssert(item, SymbolItem);
	this.Trace("  Name           : " + item.name);
	this.Trace("  ExportFilename : " + this.SymbolItem_GetExportFilename(item));
};

FlashTools.prototype.SymbolItem_GetExportFilename = function(item) {
	this.TypeAssert(item, SymbolItem);
	var item_id = this.GetUniqueItemId(item);
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
	var xml_content = this.SymbolItem_ExportXmlContent(item);
	var item_export_path = this.SymbolItem_GetExportFullFilename(document, item);
	if ( !FLfile.write(item_export_path, xml_content) ) {
		throw "Can't create symbol ({0})!"
			.format(item_export_path);
	}
};

FlashTools.prototype.SymbolItem_ExportXmlContent = function(item, indent) {
	indent = indent == undefined ? "" : indent,
	this.TypeAssert(item, SymbolItem);
	this.TypeAssert(indent, 'string');
	return "{0}<symbol>\n{1}{0}</symbol>\n".format(
		indent,
		this.Timeline_ExportXmlContent(item.timeline, indent + "  "));
};

FlashTools.prototype.SymbolItem_GetLibraryXmlDescription = function(item, indent) {
	indent = indent == undefined ? "" : indent,
	this.TypeAssert(item, SymbolItem);
	this.TypeAssert(indent, 'string');
	return '{0}<asset name="{1}" type="{2}" filename="{3}"/>\n'.format(
		indent,
		this.GetUniqueItemId(item),
		"symbol",
		this.SymbolItem_GetExportFilename(item));
};

// ------------------------------------
// Timeline functions
// ------------------------------------

FlashTools.prototype.Timeline_TraceInfo = function(timeline) {
	this.TypeAssert(timeline, Timeline);
	this.Trace("  Name        : " + timeline.name);
	this.Trace("  Layer count : " + timeline.layerCount);
	this.Trace("  Frame count : " + timeline.frameCount);
};

FlashTools.prototype.Timeline_ExportXmlContent = function(timeline, indent) {
	indent = indent == undefined ? "" : indent,
	this.TypeAssert(timeline, Timeline);
	this.TypeAssert(indent, 'string');
	return '{0}<timeline layers="{1}" frames="{2}">\n{0}</timeline>\n'.format(
		indent,
		timeline.layerCount,
		timeline.frameCount);
};

// ------------------------------------
// Document functions
// ------------------------------------

FlashTools.prototype.Document_TraceInfo = function(document) {
	this.TypeAssert(document, Document);
	this.Trace("  Name         : " + document.name);
	this.Trace("  Path         : " + this.Document_GetPath(document));
	this.Trace("  ExportFolder : " + this.Document_GetExportFolder(document));
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

FlashTools.prototype.Document_ExitEditMode = function(document) {
	this.TypeAssert(document, Document);
	for ( var i = 0; i < 100; ++i ) {
		document.exitEditMode();
	}
};

FlashTools.prototype.Document_ForEachByLibraryItems = function(document, func, filter_func) {
	filter_func = filter_func == undefined ? function(item) { return true; } : filter_func;
	this.TypeAssert(document, Document);
	this.TypeAssert(func, 'function');
	this.TypeAssert(filter_func, 'function');
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

FlashTools.prototype.Document_ExportStage = function(document) {
	this.TypeAssert(document, Document);
	this.Document_ExitEditMode(document);
	var xml_content = "<stage>\n{0}</stage>".format(
		this.Timeline_ExportXmlContent(document.getTimeline(), "  "));
	var stage_path = this.Document_GetStageExportPath(document);
	if ( !FLfile.write(stage_path, xml_content) ) {
		throw "Can't create stage xml ({0})!"
			.format(stage_path);
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

FlashTools.prototype.Document_ExportLibrary = function(document) {
	this.TypeAssert(document, Document);
	var xml_content = "<library>\n";
	this.Document_ForEachByLibraryItems(document, function(item) {
		if ( this.IsFolderLibraryItem(item) ) {
			// nothing
		} else if ( this.IsBitmapLibraryItem(item) ) {
			xml_content += this.BitmapItem_GetLibraryXmlDescription(item, "  ");
		} else if ( this.IsSymbolLibraryItem(item) ) {
			xml_content += this.SymbolItem_GetLibraryXmlDescription(item, "  ");
		} else {
			throw "Unsupported library item type ({0})!"
				.format(item.itemType);
		}
	}.bind(this));
	xml_content += "</library>";
	var library_path = this.Document_GetLibraryExportPath(document);
	if ( !FLfile.write(library_path, xml_content) ) {
		throw "Can't create library xml ({0})!"
			.format(library_path);
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
	this.Trace("-= Convert document start =-");
	try {
		this.Document_TraceInfo(document);
		this.Document_PrepareExportFolder(document);
		this.Document_ExportStage(document);
		this.Document_ExportBitmaps(document);
		this.Document_ExportSymbols(document);
		this.Document_ExportLibrary(document);
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


