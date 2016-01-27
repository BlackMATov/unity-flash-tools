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
};

// ------------------------------------
// Common functions
// ------------------------------------

FlashTools.prototype.Trace = function(text) {
	fl.outputPanel.trace(text);
};

FlashTools.prototype.TraceError = function(text) {
	this.Trace("!!!Error!!!: " + text);
};

FlashTools.prototype.Assert = function(expr, msg) {
	if ( !expr ) {
		throw msg != undefined ? "Assert! " + msg : "Assert!";
	}
};

FlashTools.prototype.ClearOutput = function() {
	fl.outputPanel.clear();
};

FlashTools.prototype.EscapePath = function(path) {
	return path.replace(/ /g, '%20');
};

FlashTools.prototype.CombinePath = function(lhs, rhs) {
	return this.EscapePath(lhs) + this.EscapePath(rhs);
};

// ------------------------------------
// Library item functions
// ------------------------------------

FlashTools.prototype.IsClipLibraryItem = function(item) {
	return item.itemType == "graphic" || item.itemType == "component" || item.itemType == "movie clip";
};

FlashTools.prototype.IsBitmapLibraryItem = function(item) {
	return item.itemType == "bitmap";
};

FlashTools.prototype.IsFolderLibraryItem = function(item) {
	return item.itemType == "folder";
};

// ------------------------------------
// Clip item functions
// ------------------------------------

FlashTools.prototype.ClipItem_TraceInfo = function(item) {
	this.Trace("\tName           : " + item.name);
	this.Trace("\tExportFilename : " + this.ClipItem_GetExportFilename(item));
};

FlashTools.prototype.ClipItem_GetExportFilename = function(item) {
	return "clips/" + item.name.replace(/\//g, ' ') + ".xml";
};

FlashTools.prototype.ClipItem_GetExportFullFilename = function(document, item) {
	return this.CombinePath(
		this.Document_GetExportFolder(document),
		this.ClipItem_GetExportFilename(item));
};

FlashTools.prototype.ClipItem_Export = function(document, item) {
	this.Document_ExitEditMode(document);
	if ( !document.library.editItem(item.name) ) {
		throw "Can't edit clip ({0})!"
			.format(item.name);
	}
	this.Document_ExitEditMode(document);
	this.Timeline_TraceInfo(item.timeline);
	var xml_content = "<clip>\n";
	xml_content += "</clip>";
	var item_export_path = this.ClipItem_GetExportFullFilename(document, item);
	if ( !FLfile.write(item_export_path, xml_content) ) {
		throw "Can't create clip ({0})!"
			.format(item_export_path);
	}
}

// ------------------------------------
// Bitmap item functions
// ------------------------------------

FlashTools.prototype.BitmapItem_TraceInfo = function(item) {
	this.Trace("\tName           : " + item.name);
	this.Trace("\tExportFilename : " + this.BitmapItem_GetExportFilename(item));
};

FlashTools.prototype.BitmapItem_GetExportFilename = function(item) {
	var export_filename = "bitmaps/" + item.name.replace(/\//g, ' ');
	var regex_has_png_ext = /\.png$/i;
	return regex_has_png_ext.test(export_filename)
		? export_filename
		: export_filename + ".png";
};

FlashTools.prototype.BitmapItem_GetExportFullFilename = function(document, item) {
	return this.CombinePath(
		this.Document_GetExportFolder(document),
		this.BitmapItem_GetExportFilename(item));
};

FlashTools.prototype.BitmapItem_Export = function(document, item) {
	var item_export_path = this.BitmapItem_GetExportFullFilename(document, item);
	if ( !item.exportToFile(item_export_path) ) {
		throw "Can't export bitmap ({0})!"
			.format(item_export_path);
	}
};

// ------------------------------------
// Timeline functions
// ------------------------------------

FlashTools.prototype.Timeline_TraceInfo = function(timeline) {
	this.Trace("\tName        : " + timeline.name);
	this.Trace("\tLayer count : " + timeline.layerCount);
};

// ------------------------------------
// Document functions
// ------------------------------------

FlashTools.prototype.Document_TraceInfo = function(document) {
	this.Trace("\tName         : " + document.name);
	this.Trace("\tPath         : " + this.Document_GetPath(document));
	this.Trace("\tExportFolder : " + this.Document_GetExportFolder(document));
};

FlashTools.prototype.Document_GetPath = function(document) {
	return this.EscapePath(document.pathURI);
};

FlashTools.prototype.Document_GetExportFolder = function(document) {
	return this.Document_GetPath(document) + "_export/";
};

FlashTools.prototype.Document_GetLibraryExportPath = function(document) {
	return this.Document_GetExportFolder(document) + "library.xml";
};

FlashTools.prototype.Document_ExitEditMode = function(document) {
	for ( var i = 0; i < 100; ++i ) {
		document.exitEditMode();
	}
};

FlashTools.prototype.Document_ForEachByLibraryItems = function(document, func, filter_func) {
	for ( var i = 0; i < document.library.items.length; ++i ) {
		var item = document.library.items[i];
		if ( filter_func == undefined || filter_func(item) ) {
			func(item);
		}
	}
};

FlashTools.prototype.Document_PrepareExportFolder = function(document) {
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

FlashTools.prototype.Document_ExportClips = function(document) {
	this.Document_ForEachByLibraryItems(document, function(item) {
		this.ClipItem_Export(document, item);
	}.bind(this), this.IsClipLibraryItem);
};

FlashTools.prototype.Document_ExportBitmaps = function(document) {
	this.Document_ForEachByLibraryItems(document, function(item) {
		this.BitmapItem_Export(document, item);
	}.bind(this), this.IsBitmapLibraryItem);
};

FlashTools.prototype.Document_ExportLibrary = function(document) {
	var xml_content = "<library>\n";
	this.Document_ForEachByLibraryItems(document, function(item) {
		if ( this.IsFolderLibraryItem(item) ) {
			// nothing
		} else if ( this.IsBitmapLibraryItem(item) ) {
			xml_content +=
				"\t<asset name='{0}' type='{1}' filename='{2}'/>\n".format(
					item.name,
					item.itemType,
					this.BitmapItem_GetExportFilename(item));
		} else if ( this.IsClipLibraryItem(item) ) {
			xml_content +=
				"\t<asset name='{0}' type='{1}' filename='{2}'/>\n".format(
					item.name,
					item.itemType,
					this.ClipItem_GetExportFilename(item));
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
	this.Trace("-= Convert document start =-");
	try {
		this.Document_TraceInfo(document);
		this.Document_ExportClips(document);
		this.Document_PrepareExportFolder(document);
		this.Document_ExportClips(document);
		this.Document_ExportBitmaps(document);
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
	}
	catch ( e ) {
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

