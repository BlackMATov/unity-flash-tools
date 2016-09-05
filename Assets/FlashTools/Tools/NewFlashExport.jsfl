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
	
	//
	// ft
	//
	
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
			.replace(/\&/g, '&amp;' )
			.replace(/\"/g, '&quot;')
			.replace(/\'/g, '&apos;')
			.replace(/</g,  '&lt;'  )
			.replace(/>/g,  '&gt;'  );
	};

	ft.combine_path = function (lhs, rhs) {
		ft.type_assert(lhs, 'string');
		ft.type_assert(rhs, 'string');
		return ft.escape_path(lhs) + ft.escape_path(rhs);
	};
	
	ft.array_foldl = function (arr, func, acc) {
		ft.type_assert(arr, Array);
		ft.type_assert(func, Function);
		for (var index = 0; index < arr.length; ++index) {
			var value = arr[index];
			acc = func(value, acc);
		}
		return acc;
	};
	
	ft.array_foldr = function (arr, func, acc) {
		ft.type_assert(arr, Array);
		ft.type_assert(func, Function);
		for (var index = arr.length - 1; index >= 0; --index) {
			var value = arr[index];
			acc = func(value, acc);
		}
		return acc;
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
	
	ft.gen_unique_name = function() {
		if (!ft.unique_name_index) {
			ft.unique_name_index = 0;
		}
		++ft.unique_name_index;
		return "ft_unique_name_" + ft.unique_name_index;
	};
	
	//
	// document
	//
	
	var ftdoc = {};
	
	ftdoc.prepare = function (doc) {
		ft.type_assert(doc, Document);
		ftdoc.prepare_folders(doc);
		ftdoc.full_exit_edit_mode(doc);
		ftdoc.delete_unused_items(doc);
		ftdoc.unlock_all_timelines(doc);
		ftdoc.optimize_all_timelines(doc);
		ftdoc.export_swf(doc);
	};
	
	ftdoc.prepare_folders = function (doc) {
		ft.type_assert(doc, Document);
		var export_folder = ftdoc.get_export_folder(doc);
		if (!FLfile.exists(export_folder) && !FLfile.createFolder(export_folder)) {
			throw "Can't create document export folder ({0})!"
				.format(export_folder);
		}
	};
	
	ftdoc.get_export_folder = function (doc) {
		ft.type_assert(doc, Document);
		return ft.combine_path(
			ft.escape_path(doc.pathURI),
			"_export/");
	};
	
	ftdoc.full_exit_edit_mode = function (doc) {
		for (var i = 0; i < 100; ++i) {
			doc.exitEditMode();
		}
	};
	
	ftdoc.delete_unused_items = function (doc) {
		ft.type_assert(doc, Document);
		var unused_items = doc.library.unusedItems;
		ft.array_foreach(unused_items, function (item) {
			ft.trace_fmt("Remove unused item: {0}", item.name);
			doc.library.deleteItem(item.name);
		}, function(item) {
			return !item.linkageExportForAS && !item.linkageExportForRS;
		});
	};
	
	ftdoc.unlock_all_timelines = function (doc) {
		ft.type_assert(doc, Document);
		fttim.unlock(doc.getTimeline());
		ftlib.unlock_all_timelines(doc, doc.library);
	};
	
	ftdoc.optimize_all_timelines = function (doc) {
		ft.type_assert(doc, Document);
		fttim.optimize_one_frame_graphics(doc, doc.getTimeline());
		ftlib.optimize_one_frame_graphics(doc, doc.library);
		fttim.optimize_static_symbols(doc, doc.getTimeline());
		ftlib.optimize_static_symbols(doc, doc.library);
	};
	
	ftdoc.export_swf = function (doc) {
		ft.type_assert(doc, Document);
		ft.trace_fmt("Export swf: {0}", doc.name);
		doc.exportSWF(ftdoc.get_export_swf_path(doc));
	};
	
	ftdoc.get_export_swf_path = function (doc) {
		ft.type_assert(doc, Document);
		return ft.combine_path(
			ftdoc.get_export_folder(doc),
			doc.name + ".swf");
	};
	
	//
	// library
	//
	
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
	
	ftlib.edit_all_items = function (doc, library, func, filter) {
		ft.type_assert(doc, Document);
		ft.type_assert(library, Library);
		ft.type_assert(func, Function);
		ft.type_assert_if_defined(filter, Function);
		ft.array_foreach(library.items, function (item) {
			if (library.editItem(item.name)) {
				func(item);
				doc.exitEditMode();
			}
		}, filter);
	};
	
	ftlib.edit_all_symbol_items = function (doc, library, func) {
		ft.type_assert(doc, Document);
		ft.type_assert(library, Library);
		ft.type_assert(func, Function);
		ftlib.edit_all_items(doc, library, func, ftlib.is_symbol_item);
	}
	
	ftlib.unlock_all_timelines = function (doc, library) {
		ft.type_assert(doc, Document);
		ft.type_assert(library, Library);
		ftlib.edit_all_symbol_items(doc, library, function(item) {
			fttim.unlock(item.timeline);
		});
	};
	
	ftlib.optimize_one_frame_graphics = function (doc, library) {
		ft.type_assert(doc, Document);
		ft.type_assert(library, Library);
		ftlib.edit_all_symbol_items(doc, library, function(item) {
			fttim.optimize_one_frame_graphics(doc, item.timeline);
		});
	};
	
	ftlib.optimize_static_symbols = function (doc, library) {
		ft.type_assert(doc, Document);
		ft.type_assert(library, Library);
		ftlib.edit_all_symbol_items(doc, library, function(item) {
			fttim.optimize_static_symbols(doc, item.timeline);
		});
	};
	
	//
	// timeline
	//
	
	var fttim = {};
	
	fttim.is_shape_instance = function (elem) {
		return elem.elementType == "shape";
	};
	
	fttim.is_symbol_instance = function (elem) {
		return elem.elementType == "instance" && elem.instanceType == "symbol";
	};
	
	fttim.is_symbol_graphic_instance = function (elem) {
		return fttim.is_symbol_instance(elem) && elem.symbolType == "graphic";
	};
	
	fttim.is_symbol_graphic_single_frame_instance = function (elem) {
		return fttim.is_symbol_instance(elem) && elem.symbolType == "graphic" && elem.loop == "single frame";
	};
	
	fttim.is_symbol_movie_clip_instance = function (elem) {
		return fttim.is_symbol_instance(elem) && elem.symbolType == "movie clip";
	};
	
	fttim.unlock = function (timeline) {
		ft.type_assert(timeline, Timeline);
		ft.array_foreach(timeline.layers, function(layer) {
			layer.locked  = false;
			layer.visible = true;
		});
	};
	
	fttim.optimize_one_frame_graphics = function (doc, timeline) {
		ft.type_assert(doc, Document);
		ft.type_assert(timeline, Timeline);
		ft.array_foreach(timeline.layers, function(layer) {
			ft.array_foreach(layer.frames, function(frame, frame_index) {
				if ( timeline.currentFrame != frame_index ) {
					timeline.currentFrame = frame_index;
				}
				ft.array_foreach(frame.elements, function(elem) {
					doc.selectNone();
					doc.selection = [elem];
					if (doc.selection.length > 0) {
						var sx = elem.skewX;
						var sy = elem.skewY;
						elem.skewX = 0;
						elem.skewY = 0;
						
						var tx  = elem.matrix.tx;
						var ty  = elem.matrix.ty;
						var tpx = elem.getTransformationPoint().x;
						var tpy = elem.getTransformationPoint().y;
						elem.setTransformationPoint({x:0, y:0});
						
						var new_sym  = doc.convertToSymbol("graphic", ft.gen_unique_name(), "top left");
						var new_elem = doc.selection[0];
						
						var dx = new_elem.matrix.tx - tx;
						var dy = new_elem.matrix.ty - ty;
						
						new_elem.setTransformationPoint({x:tpx-dx, y:tpy-dy});
						new_elem.skewX = sx;
						new_elem.skewY = sy;
						
						ft.trace_fmt("Optimize one frame graphics: {0}", timeline.name);
					}
				}, fttim.is_symbol_graphic_single_frame_instance);
			}, function(frame, frame_index) {
				return frame.startFrame == frame_index;
			});
		});
	};
	
	fttim.optimize_static_symbols = function (doc, timeline) {
		ft.type_assert(doc, Document);
		ft.type_assert(timeline, Timeline);
		if (fttim.is_static(timeline)) {
			doc.selectNone();
			doc.selectAll();
			if (doc.selection.length > 0) {
				doc.convertSelectionToBitmap();
				ft.trace_fmt("Optimize static symbol: {0}", timeline.name);
			}
		}
	};
	
	fttim.is_static = function (timeline) {
		ft.type_assert(timeline, Timeline);
		if (timeline.frameCount > 1) {
			return false;
		}
		return ft.array_foldl(timeline.layers, function(layer, acc) {
			return ft.array_foldl(layer.frames, function(frame, acc2) {
				return ft.array_foldl(frame.elements, function(elem, acc3) {
					return acc3 && fttim.is_symbol_instance(elem)
						? fttim.is_symbol_graphic_single_frame_instance(elem) || fttim.is_static(elem.libraryItem.timeline)
						: acc3;
				}, acc2);
			}, acc);
		}, true);
	};
	
	//
	// main
	//
	
	(function () {
		ft.clear_output();
		fl.showIdleMessage(false);
		ft.trace("- Start -");
		ft.array_foreach(fl.documents, function (doc) {
			try {
				ft.trace_fmt("Document: {0}", doc.name);
				ftdoc.prepare(doc);
			} catch (e) {
				ft.trace_fmt("- Document conversion error: {0}", e);
			}
		});
		ft.array_foreach(fl.documents, function (doc) {
			if ( doc.canRevert() ) {
				fl.revertDocument(document);
			}
		});
		ft.trace("- Finish -");
	})();
})();