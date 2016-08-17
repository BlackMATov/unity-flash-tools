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
	
	ft.hex_str_to_color32 = function (hstr) {
		ft.type_assert(hstr, 'string');
		ft.assert(hstr.length == 7, "incorrect hex_str");
		var result = [];
		var hex_digit = ['0','1','2','3','4','5','6','7','8','9','A','B','C','D','E','F'];
		for (var i = 1; i < hstr.length; i += 2) {
			result.push(
				hex_digit.indexOf(hstr[i + 0].toUpperCase()) * 16.0 +
				hex_digit.indexOf(hstr[i + 1].toUpperCase())
			);
		}
		return result;
	};
	
	ft.array_filter = function (arr, filter) {
		ft.type_assert(arr, Array);
		ft.type_assert(filter, Function);
		var new_arr = [];
		for (var index = 0; index < arr.length; ++index) {
			var value = arr[index];
			if ( filter(value, index) ) {
				new_arr.push(value);
			}
		}
		return new_arr;
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
	
	ft.array_reverse_foreach = function (arr, func, filter) {
		ft.type_assert(arr, Array);
		ft.type_assert(func, Function);
		ft.type_assert_if_defined(filter, Function);
		for (var index = arr.length - 1; index >= 0; --index) {
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
	
	// ------------------------------------
	// Document
	// ------------------------------------

	var ftdoc = {};
	
	ftdoc.full_exit_edit_mode = function (document) {
		ft.type_assert(document, Document);
		for (var i = 0; i < 100; ++i) {
			document.exitEditMode();
		}
	};
	
	ftdoc.prepare_folders = function (document) {
		ft.type_assert(document, Document);
		var export_folder = ftdoc.get_export_folder(document);
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
	};
	
	ftdoc.delete_unused_items = function(document) {
		ft.type_assert(document, Document);
		ftlib.delete_unused_items(document.library);
	};

	ftdoc.prepare_keyframes = function(document) {
		ftlib.prepare_keyframes(document.library, document);
		fttim.prepare_keyframes(document.getTimeline());
	};
	
	ftdoc.convert_shapes = function(document) {
		ftlib.convert_shapes(document.library, document);
		fttim.convert_shapes(document.getTimeline(), document);
	};
	
	ftdoc.prepare_bitmaps = function (document) {
		ft.type_assert(document, Document);
		ftlib.prepare_bitmaps(document.library);
	};
	
	ftdoc.export_swf = function (document) {
		ft.type_assert(document, Document);
		ft.trace_fmt("Export swf: {0}", document.name);
		document.exportSWF(ftdoc.get_swf_export_path(document));
	};
	
	ftdoc.get_document_name = function (document) {
		ft.type_assert(document, Document);
		return document.name;
	};

	ftdoc.get_document_path = function (document) {
		ft.type_assert(document, Document);
		return ft.escape_path(document.pathURI);
	};

	ftdoc.get_export_folder = function (document) {
		ft.type_assert(document, Document);
		return ft.combine_path(
			ftdoc.get_document_path(document),
			"_export/");
	};
	
	ftdoc.get_swf_export_path = function (document) {
		ft.type_assert(document, Document);
		return ft.combine_path(
			ftdoc.get_export_folder(document),
			ftdoc.get_document_name(document) + ".swf");
	};
	
	// ------------------------------------
	// Library
	// ------------------------------------

	var ftlib = {};
	
	ftlib.is_folder_item = function (item) {
		ft.type_assert(item, LibraryItem);
		return item.itemType == "folder";
	};

	ftlib.is_bitmap_item = function (item) {
		ft.type_assert(item, LibraryItem);
		return item.itemType == "bitmap";
	};

	ftlib.is_symbol_item = function (item) {
		ft.type_assert(item, LibraryItem);
		return item.itemType == "graphic" || item.itemType == "movie clip";
	};

	ftlib.for_each_by_items = function (library, func, filter) {
		ft.type_assert(func, Function);
		ft.type_assert_if_defined(filter, Function);
		ft.array_foreach(library.items, func, filter);
	};

	ftlib.delete_unused_items = function(library) {
		ft.type_assert(library, Library);
		var unused_items = library.unusedItems;
		ft.array_foreach(unused_items, function (item) {
			ft.trace_fmt("Remove unused item: {0}", item.name);
			library.deleteItem(item.name);
		});
	};

	ftlib.prepare_keyframes = function(library, document) {
		ft.type_assert(library, Library);
		ft.type_assert(document, Document);
		ftlib.for_each_by_items(library, function (item) {
			if ( library.editItem(item.name) ) {
				ftsym.prepare_keyframes(item);
				document.exitEditMode();
			}
		}.bind(this), this.is_symbol_item.bind(this));
	};

	ftlib.convert_shapes = function(library, document) {
		ft.type_assert(library, Library);
		ft.type_assert(document, Document);
		ftlib.for_each_by_items(library, function (item) {
			if ( library.editItem(item.name) ) {
				ftsym.convert_shapes(item, document);
				document.exitEditMode();
			}
		}.bind(this), this.is_symbol_item.bind(this));
	};

	ftlib.prepare_bitmaps = function(library) {
		ft.type_assert(library, Library);
		ftlib.for_each_by_items(library, function (item) {
			ft.trace_fmt("Prepare bitmap: {0}", item.name);
			item.compressionType = "lossless";
		}.bind(this), this.is_bitmap_item.bind(this));
	};

	// ------------------------------------
	// Timeline
	// ------------------------------------

	var fttim = {};
	
	fttim.prepare_keyframes = function(timeline) {
		ft.type_assert(timeline, Timeline);
		if ( timeline.layers.length > 0 && timeline.frameCount > 1 ) {
			timeline.selectAllFrames();
			//timeline.convertToKeyframes();
		}
		ft.array_reverse_foreach(timeline.layers, function(layer, index) {
			timeline.setSelectedLayers(index);
			ftlay.prepare_keyframes(layer);
		}.bind(this));
	};
	
	fttim.convert_shapes = function(timeline, document) {
		ft.type_assert(timeline, Timeline);
		ft.array_reverse_foreach(timeline.layers, function(layer, index) {
			timeline.setSelectedLayers(index);
			ftlay.convert_shapes(layer, timeline, document);
		}.bind(this));
	};
	
	// ------------------------------------
	// Layer
	// ------------------------------------

	var ftlay = {};
	
	ftlay.do_in_unlocked = function(layer, func) {
		ft.type_assert(layer, Layer);
		ft.type_assert(func, Function);
		var prev_locked  = layer.locked;
		var prev_visible = layer.visible;
		layer.locked  = false;
		layer.visible = true;
		func();
		layer.locked  = prev_locked;
		layer.visible = prev_visible;
	};
	
	ftlay.prepare_keyframes = function(layer) {
		ft.type_assert(layer, Layer);
		ftlay.do_in_unlocked(layer, function() {
			ft.array_foreach(layer.frames, function(frame, index) {
				frame.convertToFrameByFrameAnimation();
			}.bind(this));
		}.bind(this));
	};
	
	ftlay.convert_shapes = function(layer, timeline, document) {
		ftlay.do_in_unlocked(layer, function() {
			ft.array_foreach(layer.frames, function(frame, index) {
				if (frame.startFrame == index) {
					timeline.setSelectedFrames(index, index + 1);
					timeline.currentFrame = frame.startFrame;
					document.selectNone();
					document.selection = ft.array_filter(
						frame.elements,
						function(element) { return element.elementType == "shape"; });
					if ( document.selection.length > 0 ) {
						document.convertSelectionToBitmap();
						document.arrange("back");
					}
				}
			}.bind(this));
		}.bind(this));
	}
	
	// ------------------------------------
	// Symbol
	// ------------------------------------

	var ftsym = {};
	
	ftsym.prepare_keyframes = function(item) {
		fttim.prepare_keyframes(item.timeline);
	};
	
	ftsym.convert_shapes = function(item, document) {
		fttim.convert_shapes(item.timeline, document);
	};
	
	// ------------------------------------
	// Main
	// ------------------------------------

	(function () {
		ft.clear_output();
		fl.showIdleMessage(false);
		ft.trace("- Start -");
		ft.array_foreach(fl.documents, function (document) {
			try {
				ft.trace_fmt("Document: {0}", document.name);
				ftdoc.full_exit_edit_mode(document);
				ftdoc.prepare_folders(document);
				ftdoc.delete_unused_items(document);
				ftdoc.prepare_keyframes(document);
				ftdoc.convert_shapes(document);
				ftdoc.prepare_bitmaps(document);
				ftdoc.export_swf(document);
			} catch (e) {
				ft.trace_fmt("- Document conversion error: {0}", e);
			}
			fl.revertDocument(document);
		});
		ft.trace("- Finish -");
		fl.showIdleMessage(true);
	})();
})();
