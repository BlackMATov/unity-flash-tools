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

	var ftd = {};
	
	ftd.prepare_folders = function (document) {
		ft.type_assert(document, Document);
		var export_folder = ftd.get_export_folder(document);
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
	
	ftd.full_exit_edit_mode = function (document) {
		for (var i = 0; i < 100; ++i) {
			document.exitEditMode();
		}
	};
	
	ftd.delete_unused_items = function(document) {
		var unused_items = document.library.unusedItems;
		ft.array_foreach(unused_items, function (item) {
			ft.trace_fmt("Remove unused item: {0}", item.name);
			document.library.deleteItem(item.name);
		});
	};
	
	ftd.convert_shapes = function(document) {
		ftt.convert(document.getTimeline());
		ftlib.convert(document.library, document);
		
		ftt.prepare(document.getTimeline(), document);
		ftlib.prepare(document.library, document);
	};
	
	ftd.prepare_bitmaps = function (document) {
		ft.type_assert(document, Document);
		ft.array_foreach(document.library.items, function(item) {
			//ft.trace_fmt("-Item: {0}", item.name);
			if ( item.itemType == "bitmap" ) {
				item.compressionType = "lossless";
			}
		});
	};
	
	ftd.export_swf = function (document) {
		ft.type_assert(document, Document);
		document.exportSWF(ftd.get_swf_export_path(document));
	};
	
	ftd.get_document_name = function (document) {
		ft.type_assert(document, Document);
		return document.name;
	};

	ftd.get_document_path = function (document) {
		ft.type_assert(document, Document);
		return ft.escape_path(document.pathURI);
	};

	ftd.get_export_folder = function (document) {
		ft.type_assert(document, Document);
		return ft.combine_path(
			ftd.get_document_path(document),
			"_export/");
	};
	
	ftd.get_swf_export_path = function (document) {
		ft.type_assert(document, Document);
		return ft.combine_path(
			ftd.get_export_folder(document),
			ftd.get_document_name(document) + ".swf");
	};
	
	// ------------------------------------
	// Timeline
	// ------------------------------------

	var ftt = {};
	
	ftt.remove_empty_layers = function(timeline) {
		var layers = timeline.layers;
		for ( var i = layers.length - 1; i >= 0; --i ) {
			if (ftl.is_empty(layers[i])) {
				timeline.deleteLayer(i);
				layers = timeline.layers;
			}
		}
	};
	
	ftt.convert = function(timeline) {
		ftt.remove_empty_layers(timeline);
		if ( timeline.layers.length > 0 && timeline.frameCount > 1 ) {
			timeline.selectAllFrames();
			timeline.convertToKeyframes();
		}
		ft.array_reverse_foreach(timeline.layers, function(layer, index) {
			timeline.setSelectedLayers(index);
			ftl.convert(layer);
		}.bind(this));
	};
	
	ftt.prepare = function(timeline, document) {
		ft.array_reverse_foreach(timeline.layers, function(layer, index) {
			timeline.setSelectedLayers(index);
			ftl.prepare(layer, timeline, document);
		}.bind(this));
	};
	
	// ------------------------------------
	// Layer
	// ------------------------------------

	var ftl = {};
	
	ftl.do_in_unlocked = function(layer, func) {
		ft.type_assert(func, Function);
		var prev_locked  = layer.locked;
		var prev_visible = layer.visible;
		layer.locked  = false;
		layer.visible = true;
		func();
		layer.locked  = prev_locked;
		layer.visible = prev_visible;
	};
	
	ftl.convert = function(layer) {
		ftl.do_in_unlocked(layer, function() {
			ft.array_foreach(layer.frames, function(frame, index) {
				frame.convertToFrameByFrameAnimation();
			}.bind(this));
		}.bind(this));
	};
	
	ftl.is_element_shape = function(element) {
		return element.elementType == "shape"
	};
	
	ftl.is_frame_empty = function(frame) {
		return frame.elements.length == 0;
	};
	
	ftl.is_empty = function(layer) {
		if ( !layer.visible ) {
			return true;
		}
		if ( layer.layerType == "guide" || layer.layerType == "mask" || layer.layerType == "folder" ) {
			return false;
		}
		var frames = layer.frames;
		for ( var i = 0; i < frames.length; ++i ) {
			if (!ftl.is_frame_empty(frames[i])) {
				return false;
			}
		}
		return true;
	};
	
	ftl.prepare = function(layer, timeline, document) {
		ftl.do_in_unlocked(layer, function() {
			ft.array_foreach(layer.frames, function(frame, index) {
				if (frame.startFrame == index) {
					timeline.setSelectedFrames(index, index + 1);
					
					timeline.currentFrame = frame.startFrame;
					document.selectNone();
					document.selection = ft.array_filter(
						frame.elements,
						ftl.is_element_shape.bind(this));
					if ( document.selection.length > 0 ) {
						document.convertSelectionToBitmap();
						document.arrange("back");
					}
				}
			}.bind(this));
		}.bind(this));
	}
	
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
	
	ftlib.convert = function(library, document) {
		ft.type_assert(document, Document);
		ftlib.for_each_by_items(library, function (item) {
			if ( library.editItem(item.name) ) {
				ft.trace_fmt("Convert: {0}", item.name);
				ftsym.convert(item, document);
				document.exitEditMode();
			}
		}.bind(this), this.is_symbol_item.bind(this));
	};
	
	ftlib.prepare = function(library, document) {
		ft.type_assert(document, Document);
		ftlib.for_each_by_items(library, function (item) {
			if ( library.editItem(item.name) ) {
				ft.trace_fmt("Prepare: {0}", item.name);
				ftsym.prepare(item, document);
				document.exitEditMode();
			}
		}.bind(this), this.is_symbol_item.bind(this));
	};
	
	// ------------------------------------
	// Symbol
	// ------------------------------------

	var ftsym = {};
	
	ftsym.convert = function(item, document) {
		ftt.convert(item.timeline);
	};
	
	ftsym.prepare = function(item, document) {
		ftt.prepare(item.timeline, document);
	};
	
	// ------------------------------------
	// Main
	// ------------------------------------

	(function () {
		ft.clear_output();
		ft.array_foreach(fl.documents, function (document) {
			ft.trace_fmt("Doc: {0}", document.name);
			ftd.prepare_folders(document);
			ftd.full_exit_edit_mode(document);
			ftd.delete_unused_items(document);
			ftd.convert_shapes(document);
			ftd.prepare_bitmaps(document);
			ftd.export_swf(document);
			//fl.revertDocument(document);
		});
	})();
})();