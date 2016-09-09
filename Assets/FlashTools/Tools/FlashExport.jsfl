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
	
	var ft = {
		profile_mode             : false,
		verbose_mode             : true,
		optimize_static_items    : true,
		optimize_single_graphics : true
	};
	
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
	
	ft.profile_function = function (func, msg) {
		ft.type_assert(func, Function);
		ft.type_assert(msg, 'string');
		if (!ft.profile_function_stack) {
			ft.profile_function_stack = [];
		}
		if (!ft.profile_function_level) {
			ft.profile_function_level = 0;
		}
		var stack_index = ft.profile_function_stack.length;
		ft.profile_function_stack.push({
			msg   : msg,
			level : ft.profile_function_level,
			time  : 0});
		++ft.profile_function_level;
		var func_time = ft.get_call_function_time(func);
		--ft.profile_function_level;
		ft.profile_function_stack[stack_index].time = func_time;
		if (stack_index == 0) {
			for (var i = 0; i < ft.profile_function_stack.length; ++i) {
				var info  = ft.profile_function_stack[i];
				var ident = "--";
				for (var j = 0; j < info.level; ++j) {
					ident += "--";
				}
				if (ft.profile_mode) {
					ft.trace_fmt("{0} Profile: '{1}' : {2}", ident, info.msg, info.time);
				}
			}
			ft.profile_function_stack = [];
		}
	};
	
	ft.get_call_function_time = function (func) {
		ft.type_assert(func, Function);
		var b_time = Date.now();
		try {
			func();
		} finally {
			var e_time = Date.now();
			return (e_time - b_time)/1000;
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
		if (!ft.gen_unique_name_index) {
			ft.gen_unique_name_index = 0;
		}
		++ft.gen_unique_name_index;
		return "ft_unique_name_" + ft.gen_unique_name_index;
	};
	
	//
	// document
	//
	
	var ftdoc = {};
	
	ftdoc.prepare = function (doc) {
		ft.type_assert(doc, Document);
		ft.profile_function(function() { ftdoc.prepare_folders(doc);        }, "Prepare folders");
		ft.profile_function(function() { ftdoc.full_exit_edit_mode(doc);    }, "Full exit edit mode");
		ft.profile_function(function() { ftdoc.prepare_all_bitmaps(doc);    }, "Prepare all bitmaps");
		ft.profile_function(function() { ftdoc.unlock_all_timelines(doc);   }, "Unlock all timelines");
		ft.profile_function(function() { ftdoc.optimize_all_timelines(doc); }, "Optimize all timelines");
		ft.profile_function(function() { ftdoc.rasterize_all_shapes(doc);   }, "Rasterize all shapes");
		ft.profile_function(function() { ftdoc.export_swf(doc);             }, "Export swf");
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
	
	ftdoc.unlock_all_timelines = function (doc) {
		ft.type_assert(doc, Document);
		ftlib.unlock_all_timelines(doc, doc.library);
		fttim.unlock(doc.getTimeline());
	};
	
	ftdoc.optimize_all_timelines = function (doc) {
		ft.type_assert(doc, Document);
		if (ft.optimize_static_items) {
			ft.profile_function(function() {
				ftlib.optimize_static_items(doc, doc.library);
			}, "Optimize static items");
		}
		if (ft.optimize_single_graphics) {
			ft.profile_function(function() {
				ftlib.optimize_single_graphics(doc, doc.library);
			}, "Optimize single graphics");
		}
	};
	
	ftdoc.rasterize_all_shapes = function (doc) {
		ft.type_assert(doc, Document);
		ftlib.rasterize_all_shapes(doc, doc.library);
		fttim.rasterize_all_shapes(doc, doc.getTimeline());
	};
	
	ftdoc.prepare_all_bitmaps = function (doc) {
		ft.type_assert(doc, Document);
		ftlib.prepare_all_bitmaps(doc.library);
	};
	
	ftdoc.export_swf = function (doc) {
		ft.type_assert(doc, Document);
		ft.trace_fmt("!!!Document!!!: '{0}' conversion complete!", doc.name);
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
	
	ftlib.optimize_static_items = function (doc, library) {
		ft.type_assert(doc, Document);
		ft.type_assert(library, Library);
		
		var replaces = {};
		ft.array_reverse_foreach(library.items, function (item) {
			var new_item_name = ft.gen_unique_name();
			ftlib.bake_symbol_item(doc, library, item.name, new_item_name, 0);
			replaces[item.name] = new_item_name;
			if (ft.verbose_mode) {
				ft.trace_fmt("Optimize static item: '{0}'", item.name);
			}
		}, function(item) {
			return ftlib.is_symbol_item(item) && fttim.is_static(item.timeline);
		});
		
		ftlib.edit_all_symbol_items(doc, library, function(item) {
			fttim.replace_baked_symbols(doc, item.timeline, replaces);
		});
		fttim.replace_baked_symbols(doc, doc.getTimeline(), replaces);
	};
	
	ftlib.bake_symbol_item = function (doc, library, item_name, new_item_name, first_frame) {
		ft.type_assert(doc, Document);
		ft.type_assert(library, Library);
		ft.type_assert(item_name, 'string');
		ft.type_assert(new_item_name, 'string');
		ft.type_assert(first_frame, 'number');
		
		if (!library.itemExists(new_item_name)) {
			library.addNewItem("graphic", new_item_name);
			if (library.editItem(new_item_name)) {
				if (library.addItemToDocument({x:0, y:0}, item_name)) {
					var new_item_elem = doc.selection[0];
					new_item_elem.symbolType = "graphic";
					new_item_elem.firstFrame = first_frame;
					new_item_elem.setTransformationPoint({x:0, y:0});
					new_item_elem.transformX = 0;
					new_item_elem.transformY = 0;
					doc.convertSelectionToBitmap();
				}
				doc.exitEditMode();
			}
		}
	};
	
	ftlib.optimize_single_graphics = function (doc, library) {
		ft.type_assert(doc, Document);
		ft.type_assert(library, Library);
		
		ft.array_reverse_foreach(library.items, function (item) {
			fttim.optimize_single_graphics(doc, item.timeline, item);
		}, ftlib.is_symbol_item);
		fttim.optimize_single_graphics(doc, doc.getTimeline(), null);
	};
	
	ftlib.rasterize_all_shapes = function (doc, library) {
		ft.type_assert(doc, Document);
		ft.type_assert(library, Library);
		ftlib.edit_all_symbol_items(doc, library, function(item) {
			fttim.rasterize_all_shapes(doc, item.timeline);
		});
	};
	
	ftlib.prepare_all_bitmaps = function (library) {
		ft.type_assert(library, Library);
		ft.array_foreach(library.items, function (item) {
			item.compressionType = "lossless";
		}, ftlib.is_bitmap_item);
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
	
	fttim.is_shape_frame = function (frame) {
		ft.type_assert(frame, Frame);
		return frame.tweenType == "shape";
	};
	
	fttim.is_keyframe = function (frame, frame_index) {
		ft.type_assert(frame, Frame);
		return frame.startFrame == frame_index;
	};
	
	fttim.unlock = function (timeline) {
		ft.type_assert(timeline, Timeline);
		ft.array_foreach(timeline.layers, function(layer) {
			layer.locked  = false;
			layer.visible = true;
		});
	};
	
	fttim.replace_baked_symbols = function (doc, timeline, replaces) {
		ft.type_assert(doc, Document);
		ft.type_assert(timeline, Timeline);
		ft.array_foreach(timeline.layers, function(layer) {
			ft.array_foreach(layer.frames, function(frame, frame_index) {
				if ( timeline.currentFrame != frame_index ) {
					timeline.currentFrame = frame_index;
				}
				ft.array_foreach(frame.elements, function(elem) {
					if (replaces.hasOwnProperty(elem.libraryItem.name)) {
						doc.selectNone();
						doc.selection = [elem];
						doc.swapElement(replaces[elem.libraryItem.name]);
					}
				}, fttim.is_symbol_instance);
			}, fttim.is_keyframe);
		});
	};
	
	fttim.optimize_single_graphics = function (doc, timeline, opt_item) {
		ft.type_assert(doc, Document);
		ft.type_assert(timeline, Timeline);
		ft.array_foreach(timeline.layers, function(layer) {
			ft.array_foreach(layer.frames, function(frame, frame_index) {
				ft.array_foreach(frame.elements, function(elem) {
					var lib_item_name       = elem.libraryItem.name;
					var lib_item_cache_name = "ft_cache_name_" + lib_item_name + "_" + elem.firstFrame;
					ftlib.bake_symbol_item(doc, doc.library, lib_item_name, lib_item_cache_name, elem.firstFrame);
					
					if (ft.verbose_mode) {
						ft.trace_fmt("Optimize single graphic '{0}' for frame '{1}' in '{2}'",
							lib_item_name, elem.firstFrame, timeline.name);
					}
					
					if (opt_item == null || doc.library.editItem(opt_item.name)) {
						if ( timeline.currentFrame != frame_index ) {
							timeline.currentFrame = frame_index;
						}
						doc.selectNone();
						doc.selection = [elem];
						doc.swapElement(lib_item_cache_name);
						doc.selection[0].firstFrame = 0;
						doc.exitEditMode();
					}
				}, function(elem) {
					return fttim.is_symbol_graphic_single_frame_instance(elem) && !fttim.is_static(elem.libraryItem.timeline);
				});
			}, fttim.is_keyframe);
		});
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
	
	fttim.rasterize_all_shapes = function (doc, timeline) {
		ft.type_assert(doc, Document);
		ft.type_assert(timeline, Timeline);
		
		ft.array_reverse_foreach(timeline.layers, function(layer, layer_index) {
			timeline.setSelectedLayers(layer_index);
			ft.array_foreach(layer.frames, function(frame, frame_index) {
				frame.convertToFrameByFrameAnimation();
			}, function (frame, frame_index) {
				return fttim.is_keyframe(frame, frame_index) && fttim.is_shape_frame(frame);
			});
		});
		
		var any_rasterize = false;
		ft.array_reverse_foreach(timeline.layers, function(layer, layer_index) {
			timeline.setSelectedLayers(layer_index);
			ft.array_foreach(layer.frames, function(frame, frame_index) {
				timeline.currentFrame = frame_index;
				timeline.setSelectedFrames(frame_index, frame_index + 1, true);
				doc.selectNone();
				doc.selection = ft.array_filter(frame.elements, fttim.is_shape_instance);
				if (doc.selection.length > 0) {
					doc.convertSelectionToBitmap();
					doc.arrange("back");
					any_rasterize = true;
				}
			}, fttim.is_keyframe);
		});
		if (any_rasterize && ft.verbose_mode) {
			ft.trace_fmt("Rasterize vector shapes in '{0}'", timeline.name);
		}
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
				ft.trace_fmt("!!!Document!!!: '{0}'", doc.name);
				ftdoc.prepare(doc);
			} catch (e) {
				ft.trace_fmt("!!!Document!!!: '{0}' conversion error: '{1}'", doc.name, e);
			}
		});
		ft.profile_function(function() {
			ft.array_foreach(fl.documents, function (doc) {
				if ( doc.canRevert() ) {
					fl.revertDocument(doc);
				}
			});
		}, "Revert documents");
		ft.trace("- Finish -");
	})();
})();