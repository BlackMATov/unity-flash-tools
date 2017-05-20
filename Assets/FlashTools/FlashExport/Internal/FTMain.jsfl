if (!Array.prototype.peek) {
	Array.prototype.peek = function () {
		return this[this.length - 1];
	};
}

if (!String.prototype.format) {
	String.prototype.format = function () {
		var args = arguments;
		return this.replace(/{(\d+)}/g, function (match, number) {
			return typeof args[number] != 'undefined' ? args[number] : match;
		});
	};
}

ft_main = function (opts) {
	opts = opts || {};

	//
	// ft config
	//

	var ft = {
		profile_mode              : opts.profile_mode              === undefined ? false     : opts.profile_mode,
		verbose_mode              : opts.verbose_mode              === undefined ? false     : opts.verbose_mode,

		graphics_scale            : opts.graphics_scale            === undefined ? 1.0       : opts.graphics_scale,
		scale_precision           : opts.scale_precision           === undefined ? 0.01      : opts.scale_precision,

		optimize_big_items        : opts.optimize_big_items        === undefined ? true      : opts.optimize_big_items,
		optimize_small_items      : opts.optimize_small_items      === undefined ? true      : opts.optimize_small_items,
		optimize_static_items     : opts.optimize_static_items     === undefined ? true      : opts.optimize_static_items,
		optimize_single_graphics  : opts.optimize_single_graphics  === undefined ? true      : opts.optimize_single_graphics,

		open_documents            : opts.open_documents            === undefined ? []        : opts.open_documents,
		export_path_postfix       : opts.export_path_postfix       === undefined ? "_export" : opts.export_path_postfix,
		close_after_conversion    : opts.close_after_conversion    === undefined ? false     : opts.close_after_conversion,
		revert_after_conversion   : opts.revert_after_conversion   === undefined ? true      : opts.revert_after_conversion,
		max_convertible_selection : opts.max_convertible_selection === undefined ? 3900      : opts.max_convertible_selection
	};

	//
	// ft base functions
	//

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
		if (item && item !== undefined) {
			ft.type_assert(item, type);
		}
	};

	ft.is_function = function (func) {
		return func && typeof(func) === 'function';
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
			msg: msg,
			level: ft.profile_function_level,
			time: 0
		});
		++ft.profile_function_level;
		var func_time = ft.get_call_function_time(func);
		--ft.profile_function_level;
		ft.profile_function_stack[stack_index].time = func_time;
		if (stack_index === 0) {
			for (var i = 0; i < ft.profile_function_stack.length; ++i) {
				var info = ft.profile_function_stack[i];
				var ident = "-";
				for (var j = 0; j < info.level; ++j) {
					ident += "-";
				}
				if (ft.profile_mode) {
					ft.trace_fmt("{0} [Profile] {1} ({2}s)", ident, info.msg, info.time);
				}
			}
			ft.profile_function_stack = [];
		}
	};

	ft.get_call_function_time = function (func) {
		ft.type_assert(func, Function);
		var b_time = Date.now();
		func();
		var e_time = Date.now();
		return (e_time - b_time) / 1000;
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

	ft.array_any = function (arr, func) {
		ft.type_assert(arr, Array);
		ft.type_assert(func, Function);
		for (var index = 0; index < arr.length; ++index) {
			var value = arr[index];
			if (func(value)) {
				return true;
			}
		}
		return false;
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

	ft.array_clone = function (arr) {
		ft.type_assert(arr, Array);
		var new_arr = [];
		for (var index = 0; index < arr.length; ++index) {
			var value = arr[index];
			new_arr.push(value);
		}
		return new_arr;
	};

	ft.array_filter = function (arr, filter) {
		ft.type_assert(arr, Array);
		ft.type_assert(filter, Function);
		var new_arr = [];
		for (var index = 0; index < arr.length; ++index) {
			var value = arr[index];
			if (filter(value, index)) {
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

	ft.array_group_by = function(arr, func) {
		return ft.array_foldl(arr, function (value, acc) {
			if (acc.length > 0 && func(acc.peek().peek()) == func(value)) {
				acc.peek().push(value);
			} else {
				acc.push([value]);
			}
			return acc;
		}, []);
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

	ft.approximately = function(a, b, precision) {
		ft.type_assert(a, 'number');
		ft.type_assert(b, 'number');
		ft.type_assert(precision, 'number');
		return Math.abs(b - a) < Math.abs(precision);
	};

	ft.gen_unique_name = function () {
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
		ft.profile_function(function() { ftdoc.remove_unused_items(doc);    }, "Remove unused items");
		ft.profile_function(function() { ftdoc.prepare_all_bitmaps(doc);    }, "Prepare all bitmaps");
		ft.profile_function(function() { ftdoc.unlock_all_timelines(doc);   }, "Unlock all timelines");
		ft.profile_function(function() { ftdoc.prepare_all_shapes(doc);     }, "Prepare all shapes");
		ft.profile_function(function() { ftdoc.prepare_all_groups(doc);     }, "Prepare all groups");
		ft.profile_function(function() { ftdoc.calculate_item_scales(doc);  }, "Calculate item scales");
		ft.profile_function(function() { ftdoc.optimize_all_timelines(doc); }, "Optimize all timelines");
		ft.profile_function(function() { ftdoc.rasterize_all_shapes(doc);   }, "Rasterize all shapes");
		ft.profile_function(function() { ftdoc.export_swf(doc);             }, "Export swf");
	};

	ftdoc.get_temp = function (doc) {
		if (!ftdoc.hasOwnProperty("temp")) {
			ftdoc.temp = {
				max_scales : {}
			};
		}
		return ftdoc.temp;
	};

	ftdoc.calculate_item_prefer_scale = function (doc, optional_item) {
		ft.type_assert(doc, Document);
		ft.type_assert_if_defined(optional_item, LibraryItem);
		var final_scale = ft.graphics_scale;
		if (optional_item && (ft.optimize_big_items || ft.optimize_small_items)) {
			var item_name  = optional_item.name;
			var max_scales = ftdoc.get_temp(doc).max_scales;
			if (max_scales.hasOwnProperty(item_name)) {
				var max_scale  = max_scales[item_name];
				var big_item   = ft.optimize_big_items   && (max_scale - ft.scale_precision > 1.0);
				var small_item = ft.optimize_small_items && (max_scale + ft.scale_precision < 1.0);
				if (big_item || small_item) {
					final_scale *= max_scale;
				}
			}
		}
		return final_scale;
	};

	ftdoc.convert_selection_to_bitmap = function (doc, location_name, optional_item) {
		ft.type_assert(doc, Document);
		ft.type_assert(location_name, 'string');
		ft.type_assert_if_defined(optional_item, LibraryItem);

		var selection_r = doc.getSelectionRect();
		var selection_w = selection_r.right  - selection_r.left;
		var selection_h = selection_r.bottom - selection_r.top;

		var max_scale    = ft.max_convertible_selection / Math.max(selection_w, selection_h);
		var prefer_scale = ftdoc.calculate_item_prefer_scale(doc, optional_item);
		var final_scale  = Math.min(prefer_scale, max_scale);

		if (final_scale < prefer_scale) {
			var down_scale = Math.floor(final_scale / prefer_scale * 1000) * 0.001;
			ft.trace_fmt(
				"[Warning] {0}\n" +
				"- Converted element was downscaled ({1}x) to maximum allowed size ({2}px)",
				location_name, down_scale, ft.max_convertible_selection);
		}
		
		if (ft.approximately(final_scale, 1.0, ft.scale_precision)) {
			(function() {
				var elem_r  = doc.getSelectionRect();

				var elem_x  = elem_r.left;
				var elem_y  = elem_r.top;
				var elem_w  = elem_r.right  - elem_r.left;
				var elem_h  = elem_r.bottom - elem_r.top;

				var elem_dx = Math.round(elem_x) - elem_x;
				var elem_dy = Math.round(elem_y) - elem_y;
				var elem_dw = Math.round(elem_w) - elem_w;
				var elem_dh = Math.round(elem_h) - elem_h;

				doc.convertSelectionToBitmap();
				var elem = doc.selection[0];

				elem.x      -= elem_dx;
				elem.y      -= elem_dy;
				elem.width  -= elem_dw;
				elem.height -= elem_dh;
			})();
		} else {
			(function() {
				var wrapper_item_name = ft.gen_unique_name();
				var wrapper_item = doc.convertToSymbol("graphic", wrapper_item_name , "top left");
				fttim.recursive_scale_filters(doc, wrapper_item.timeline, final_scale);

				var elem = doc.selection[0];
				elem.setTransformationPoint({x: 0, y: 0});
				doc.scaleSelection(final_scale, final_scale);

				var elem_x  = elem.x;
				var elem_y  = elem.y;
				var elem_w  = elem.width;
				var elem_h  = elem.height;

				var elem_dx = Math.round(elem_x) - elem_x;
				var elem_dy = Math.round(elem_y) - elem_y;
				var elem_dw = Math.round(elem_w) - elem_w;
				var elem_dh = Math.round(elem_h) - elem_h;

				doc.convertSelectionToBitmap();
				elem = doc.selection[0];

				elem.x      -= elem_dx;
				elem.y      -= elem_dy;
				elem.width  -= elem_dw;
				elem.height -= elem_dh;

				elem.setTransformationPoint({x: (elem_x - elem.x), y: (elem_y - elem.y)});
				doc.scaleSelection(1.0 / final_scale, 1.0 / final_scale);
			})();
		}
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
			ft.export_path_postfix + "/");
	};

	ftdoc.full_exit_edit_mode = function (doc) {
		ft.type_assert(doc, Document);
		for (var i = 0; i < 100; ++i) {
			doc.exitEditMode();
		}
	};

	ftdoc.remove_unused_items = function (doc) {
		ft.type_assert(doc, Document);
		var unused_items = doc.library.unusedItems;
		if (unused_items && unused_items !== undefined) {
			ft.array_reverse_foreach(unused_items, function(item) {
				if (ft.verbose_mode) {
					ft.trace_fmt("Remove unused item: {0}", item.name);
				}
				doc.library.deleteItem(item.name);
			});
		}
	};

	ftdoc.unlock_all_timelines = function (doc) {
		ft.type_assert(doc, Document);
		ftlib.unlock_all_timelines(doc, doc.library);
		fttim.unlock(doc, doc.getTimeline());
	};

	ftdoc.prepare_all_shapes = function (doc) {
		ft.type_assert(doc, Document);
		ftlib.prepare_all_shapes(doc, doc.library);
		fttim.prepare_all_shapes(doc, doc.getTimeline());
	};

	ftdoc.prepare_all_groups = function (doc) {
		ft.type_assert(doc, Document);
		var arr1 = ftlib.prepare_all_groups(doc, doc.library);
		var arr2 = fttim.prepare_all_groups(doc, doc.getTimeline());
		var new_symbols = arr1.concat(arr2);
		var process_item = function (item) {
			if (doc.library.editItem(item.name)) {
				var arr3 = fttim.prepare_all_groups(doc, item.timeline);
				new_symbols = new_symbols.concat(arr3);
				doc.exitEditMode();
			}
		};
		while (new_symbols.length > 0) {
			var new_symbols_copy = ft.array_clone(new_symbols);
			new_symbols = [];
			ft.array_foreach(new_symbols_copy, process_item);
		}
	};

	ftdoc.calculate_item_scales = function (doc) {
		ft.type_assert(doc, Document);

		var max_scales = ftdoc.get_temp(doc).max_scales;

		var walk_by_timeline = function(timeline, func, acc) {
			ft.type_assert(timeline, Timeline);
			ft.type_assert(func, Function);
			ft.array_foreach(timeline.layers, function (layer) {
				ft.array_foreach(layer.frames, function (frame) {
					ft.array_foreach(frame.elements, function (elem) {
						walk_by_timeline(
							elem.libraryItem.timeline,
							func,
							func(elem, acc));
					}, fttim.is_symbol_instance);
				}, fttim.is_keyframe);
			});
		};

		var walk_by_library = function(lib, func, acc) {
			ft.type_assert(lib, Library);
			ft.type_assert(func, Function);
			ft.array_foreach(lib.items, function (item) {
				walk_by_timeline(item.timeline, func, acc);
			}, ftlib.is_symbol_item);
		};

		var x_func = function(elem, acc) {
			var elem_sx   = elem.scaleX * acc;
			var item_name = elem.libraryItem.name;
			max_scales[item_name] = Math.max(
				max_scales.hasOwnProperty(item_name) ? max_scales[item_name] : elem_sx,
				elem_sx);
			return elem_sx;
		};

		var y_func = function(elem, acc) {
			var elem_sy   = elem.scaleY * acc;
			var item_name = elem.libraryItem.name;
			max_scales[item_name] = Math.max(
				max_scales.hasOwnProperty(item_name) ? max_scales[item_name] : elem_sy,
				elem_sy);
			return elem_sy;
		};

		walk_by_library(doc.library, x_func, 1.0);
		walk_by_timeline(doc.getTimeline(), x_func, 1.0);

		walk_by_library(doc.library, y_func, 1.0);
		walk_by_timeline(doc.getTimeline(), y_func, 1.0);

		if (ft.verbose_mode) {
			for (var item_name in max_scales) {
				var max_scale = max_scales.hasOwnProperty(item_name) ? max_scales[item_name] : 1.0;
				if (max_scale - ft.scale_precision > 1.0) {
					ft.trace_fmt("Big item for optimize: {0} - {1}", item_name, max_scale);
				} else if (max_scale + ft.scale_precision < 1.0) {
					ft.trace_fmt("Small item for optimize: {0} - {1}", item_name, max_scale);
				}
			}
		}
	};

	ftdoc.optimize_all_timelines = function (doc) {
		ft.type_assert(doc, Document);
		if (ft.optimize_static_items) {
			ft.profile_function(function () {
				ftlib.optimize_static_items(doc, doc.library);
			}, "Optimize static items");
		}
		if (ft.optimize_single_graphics) {
			ft.profile_function(function () {
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

	ftlib.find_item_by_name = function (library, item_name) {
		ft.type_assert(library, Library);
		ft.type_assert(item_name, 'string');
		for (var i = 0; i < library.items.length; ++i) {
			var item = library.items[i];
			if (item.name == item_name) {
				return item;
			}
		}
		return null;
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
	};

	ftlib.unlock_all_timelines = function (doc, library) {
		ft.type_assert(doc, Document);
		ft.type_assert(library, Library);
		ftlib.edit_all_symbol_items(doc, library, function (item) {
			fttim.unlock(doc, item.timeline);
		});
	};

	ftlib.optimize_static_items = function (doc, library) {
		ft.type_assert(doc, Document);
		ft.type_assert(library, Library);

		var replaces = {};
		ft.array_reverse_foreach(library.items, function (item) {
			var new_item_name = ft.gen_unique_name();
			if (ftlib.bake_symbol_item(doc, library, item.name, new_item_name, 0)) {
				replaces[item.name] = new_item_name;
				if (ft.verbose_mode) {
					ft.trace_fmt("Optimize static item: '{0}'", item.name);
				}
			} else {
				if (ft.verbose_mode) {
					ft.trace_fmt("NOT Optimize static item: '{0}'", item.name);
				}
			}
		}, function (item) {
			return ftlib.is_symbol_item(item) && fttim.is_static(item.timeline);
		});

		ftlib.edit_all_symbol_items(doc, library, function (item) {
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

		if (library.itemExists(new_item_name)) {
			return true;
		}

		var item = ftlib.find_item_by_name(library, item_name);
		if (!item) {
			return false;
		}

		var item_frame_area = fttim.calculate_frame_area(item.timeline, first_frame);
		var item_elems_area = fttim.calculate_elems_area(item.timeline, first_frame);

		if (ft.verbose_mode) {
			ft.trace_fmt(
				"Library item: '{0}'\n- frame area: {1}\n- elems area: {2}",
				item_name, item_frame_area, item_elems_area);
		}

		if (item_frame_area >= item_elems_area) {
			return false;
		}

		if (!library.addNewItem("graphic", new_item_name)) {
			return false;
		}

		if (!library.editItem(new_item_name)) {
			library.deleteItem(new_item_name);
			return false;
		}

		if (library.addItemToDocument({x: 0, y: 0}, item_name)) {
			var new_item_elem = doc.selection[0];
			new_item_elem.symbolType = "graphic";
			new_item_elem.firstFrame = first_frame;
			new_item_elem.setTransformationPoint({x: 0, y: 0});
			new_item_elem.transformX = 0;
			new_item_elem.transformY = 0;
			var location_name = "Symbol: {0}".format(item_name);
			ftdoc.convert_selection_to_bitmap(doc, location_name, item);
			doc.exitEditMode();
			return true;
		} else {
			doc.exitEditMode();
			library.deleteItem(new_item_name);
			return false;
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
		ftlib.edit_all_symbol_items(doc, library, function (item) {
			fttim.rasterize_all_shapes(doc, item.timeline);
		});
	};

	ftlib.prepare_all_shapes = function (doc, library) {
		ft.type_assert(doc, Document);
		ft.type_assert(library, Library);
		ftlib.edit_all_symbol_items(doc, library, function (item) {
			fttim.prepare_all_shapes(doc, item.timeline);
		});
	};

	ftlib.prepare_all_groups = function (doc, library) {
		ft.type_assert(doc, Document);
		ft.type_assert(library, Library);
		var new_symbols = [];
		ftlib.edit_all_symbol_items(doc, library, function (item) {
			var arr = fttim.prepare_all_groups(doc, item.timeline);
			new_symbols = new_symbols.concat(arr);
		});
		return new_symbols;
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

	fttim.is_group_shape_instance = function (elem) {
		return elem.elementType == "shape" && elem.isGroup;
	};

	fttim.is_object_shape_instance = function (elem) {
		return elem.elementType == "shape" && elem.isDrawingObject;
	};

	fttim.is_simple_shape_instance = function (elem) {
		return elem.elementType == "shape" && !elem.isGroup && !elem.isDrawingObject;
	};

	fttim.is_complex_shape_instance = function (elem) {
		return elem.elementType == "shape" && (elem.isGroup || elem.isDrawingObject);
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

	fttim.is_tween_frame = function (frame) {
		ft.type_assert(frame, Frame);
		return frame.tweenType != "none";
	};

	fttim.is_shape_tween_frame = function (frame) {
		ft.type_assert(frame, Frame);
		return frame.tweenType == "shape";
	};

	fttim.is_keyframe = function (frame, frame_index) {
		ft.type_assert(frame, Frame);
		ft.type_assert(frame_index, 'number');
		return frame.startFrame == frame_index;
	};

	fttim.is_not_guide_layer = function(layer) {
		ft.type_assert(layer, Layer);
		return layer.layerType != "guide";
	};

	fttim.unlock = function (doc, timeline) {
		ft.type_assert(doc, Document);
		ft.type_assert(timeline, Timeline);
		ft.array_foreach(timeline.layers, function (layer, layer_index) {
			layer.locked = false;
			layer.visible = true;
			timeline.setSelectedLayers(layer_index);
			ft.array_foreach(layer.frames, function (frame, frame_index) {
				timeline.currentFrame = frame_index;
				timeline.setSelectedFrames(frame_index, frame_index + 1, true);
				try {
					doc.unlockAllElements();
				} catch (e) {}
			}, fttim.is_keyframe);
		});
	};

	fttim.calculate_elems_area = function (timeline, frame_index) {
		ft.type_assert(timeline, Timeline);
		ft.type_assert(frame_index, 'number');
		return ft.array_foldl(timeline.layers, function (layer, acc) {
			if (frame_index >= 0 && frame_index < layer.frames.length) {
				return ft.array_foldl(layer.frames[frame_index].elements, function (elem, acc2) {
					return acc2 + Math.round(elem.width) * Math.round(elem.height);
				}, acc);
			} else {
				return acc;
			}
		}, 0);
	};

	fttim.calculate_frame_area = function (timeline, frame_index) {
		ft.type_assert(timeline, Timeline);
		ft.type_assert(frame_index, 'number');
		var bounds = ft.array_foldl(timeline.layers, function (layer, acc) {
			if (frame_index >= 0 && frame_index < layer.frames.length) {
				return ft.array_foldl(layer.frames[frame_index].elements, function (elem, acc2) {
					acc2.left   = Math.min(acc2.left,   elem.left);
					acc2.right  = Math.max(acc2.right,  elem.left + elem.width);
					acc2.top    = Math.min(acc2.top,    elem.top);
					acc2.bottom = Math.max(acc2.bottom, elem.top + elem.height);
					return acc2;
				}, acc);
			} else {
				return acc;
			}
		}, {
			left:   Number.POSITIVE_INFINITY,
			right:  Number.NEGATIVE_INFINITY,
			top:    Number.POSITIVE_INFINITY,
			bottom: Number.NEGATIVE_INFINITY
		});
		var frame_width  = Math.max(0, bounds.right  - bounds.left);
		var frame_height = Math.max(0, bounds.bottom - bounds.top);
		return Math.round(frame_width) * Math.round(frame_height);
	};

	fttim.recursive_scale_filters = function (doc, timeline, scale) {
		ft.type_assert(doc, Document);
		ft.type_assert(timeline, Timeline);
		ft.array_foreach(timeline.layers, function (layer) {
			ft.array_foreach(layer.frames, function (frame, frame_index) {
				ft.array_foreach(frame.elements, function (elem) {
					var elem_filters = elem.filters;
					if (elem_filters && elem_filters !== undefined) {
						ft.array_foreach(elem_filters, function (elem_filter, filter_index) {
							elem_filter.blurX *= scale;
							elem_filter.blurY *= scale;
						});
						elem.filters = elem_filters;
					}
					fttim.recursive_scale_filters(doc, elem.libraryItem.timeline, scale);
				}, fttim.is_symbol_instance);
			}, fttim.is_keyframe);
		}, fttim.is_not_guide_layer);
	};

	fttim.replace_baked_symbols = function (doc, timeline, replaces) {
		ft.type_assert(doc, Document);
		ft.type_assert(timeline, Timeline);
		ft.array_foreach(timeline.layers, function (layer) {
			ft.array_foreach(layer.frames, function (frame, frame_index) {
				if (timeline.currentFrame != frame_index) {
					timeline.currentFrame = frame_index;
				}
				ft.array_foreach(frame.elements, function (elem) {
					if (replaces.hasOwnProperty(elem.libraryItem.name)) {
						doc.selectNone();
						doc.selection = [elem];
						doc.swapElement(replaces[elem.libraryItem.name]);
					}
				}, fttim.is_symbol_instance);
			}, fttim.is_keyframe);
		}, fttim.is_not_guide_layer);
	};

	fttim.optimize_single_graphics = function (doc, timeline, opt_item) {
		ft.type_assert(doc, Document);
		ft.type_assert(timeline, Timeline);
		ft.array_foreach(timeline.layers, function (layer) {
			ft.array_foreach(layer.frames, function (frame, frame_index) {
				ft.array_foreach(frame.elements, function (elem) {
					var lib_item_name = elem.libraryItem.name;
					var lib_item_cache_name = "ft_cache_name_" + lib_item_name + "_" + elem.firstFrame;
					if (ftlib.bake_symbol_item(doc, doc.library, lib_item_name, lib_item_cache_name, elem.firstFrame)) {
						if (ft.verbose_mode) {
							ft.trace_fmt("Optimize single graphic '{0}' for frame '{1}' in '{2}'",
								lib_item_name, elem.firstFrame, timeline.name);
						}
						if (opt_item === null || doc.library.editItem(opt_item.name)) {
							if (timeline.currentFrame != frame_index) {
								timeline.currentFrame = frame_index;
							}
							doc.selectNone();
							doc.selection = [elem];
							doc.swapElement(lib_item_cache_name);
							doc.selection[0].firstFrame = 0;
							doc.exitEditMode();
						}
					} else {
						if (ft.verbose_mode) {
							ft.trace_fmt("NOT Optimize single graphic '{0}' for frame '{1}' in '{2}'",
								lib_item_name, elem.firstFrame, timeline.name);
						}
					}
				}, function (elem) {
					return fttim.is_symbol_graphic_single_frame_instance(elem) && !fttim.is_static(elem.libraryItem.timeline);
				});
			}, fttim.is_keyframe);
		}, fttim.is_not_guide_layer);
	};

	fttim.is_static = function (timeline) {
		ft.type_assert(timeline, Timeline);
		if (timeline.frameCount > 1) {
			return false;
		}
		return ft.array_foldl(timeline.layers, function (layer, acc) {
			return ft.array_foldl(layer.frames, function (frame, acc2) {
				return ft.array_foldl(frame.elements, function (elem, acc3) {
					return acc3 && fttim.is_symbol_instance(elem) ? elem.blendMode != "erase" && (fttim.is_symbol_graphic_single_frame_instance(elem) || fttim.is_static(elem.libraryItem.timeline)) : acc3;
				}, acc2);
			}, acc);
		}, true);
	};

	fttim.prepare_all_shapes = function (doc, timeline) {
		ft.type_assert(doc, Document);
		ft.type_assert(timeline, Timeline);

		ft.array_reverse_foreach(timeline.layers, function (layer, layer_index) {
			timeline.setSelectedLayers(layer_index);
			ft.array_foreach(layer.frames, function (frame, frame_index) {
				timeline.currentFrame = frame_index;
				timeline.setSelectedFrames(frame_index, frame_index + 1, true);

				if (ft.array_any(frame.elements, fttim.is_shape_instance)) {
					doc.selectNone();
					doc.selectAll();
					if (doc.selection.length > 0) {
						doc.group();
					}
				}
			}, function (frame, frame_index) {
				return fttim.is_keyframe(frame, frame_index) && fttim.is_tween_frame(frame);
			});
		}, fttim.is_not_guide_layer);
	};

	fttim.prepare_all_groups = function (doc, timeline) {
		ft.type_assert(doc, Document);
		ft.type_assert(timeline, Timeline);

		var new_symbols = [];
		ft.array_reverse_foreach(timeline.layers, function (layer, layer_index) {
			timeline.setSelectedLayers(layer_index);
			ft.array_foreach(layer.frames, function (frame, frame_index) {
				timeline.currentFrame = frame_index;
				timeline.setSelectedFrames(frame_index, frame_index + 1, true);

				var elements = ft.array_clone(frame.elements);
				ft.array_foreach(elements, function (elem, elem_index) {
					doc.selectNone();
					doc.selection = [elem];

					if (fttim.is_simple_shape_instance(elem)) {
						// nothing
					} else if (fttim.is_complex_shape_instance(elem)) {
						if (fttim.is_object_shape_instance(elem)) {
							doc.breakApart();
							doc.group();
						}
						doc.unGroup();
						try {
							doc.unlockAllElements();
						} catch (e) {}
						var wrapper_item = doc.convertToSymbol("graphic", ft.gen_unique_name(), "top left");
						new_symbols.push(wrapper_item);
					} else {
						doc.arrange("front");
					}
				});
			}, fttim.is_keyframe);
		}, fttim.is_not_guide_layer);
		return new_symbols;
	};

	fttim.rasterize_all_shapes = function (doc, timeline) {
		ft.type_assert(doc, Document);
		ft.type_assert(timeline, Timeline);

		ft.array_reverse_foreach(timeline.layers, function (layer, layer_index) {
			timeline.setSelectedLayers(layer_index);
			ft.array_foreach(layer.frames, function (frame, frame_index) {
				if ( ft.is_function(frame.convertToFrameByFrameAnimation) ) {
					ft.trace_fmt(
						"[Warning] Timeline: '{0}'\n" +
						"- Shape tween strongly not recommended because it rasterized to frame-by-frame bitmap sequence.",
						timeline.name);
					frame.convertToFrameByFrameAnimation();
				} else {
					throw "Animation uses shape tweens. To export this animation you should use Adobe Animate CC or higher!";
				}
			}, function (frame, frame_index) {
				return fttim.is_keyframe(frame, frame_index) && fttim.is_shape_tween_frame(frame);
			});
		}, fttim.is_not_guide_layer);

		var rasterize_count = 0;
		ft.array_reverse_foreach(timeline.layers, function (layer, layer_index) {
			timeline.setSelectedLayers(layer_index);
			ft.array_foreach(layer.frames, function (frame, frame_index) {
				timeline.currentFrame = frame_index;
				timeline.setSelectedFrames(frame_index, frame_index + 1, true);

				doc.selectNone();
				doc.selection = ft.array_filter(frame.elements, fttim.is_shape_instance);
				if (doc.selection.length > 0) {
					var location_name = "Timeline: {0}".format(timeline.name);
					ftdoc.convert_selection_to_bitmap(doc, location_name, timeline.libraryItem);
					doc.arrange("back");
					++rasterize_count;
				}
			}, fttim.is_keyframe);
		}, fttim.is_not_guide_layer);
		if (rasterize_count > 0 && ft.verbose_mode) {
			ft.trace_fmt("Rasterize vector shapes({0}) in '{1}'", rasterize_count, timeline.name);
		}
	};

	//
	// run
	//

	(function () {
		ft.clear_output();
		fl.showIdleMessage(false);
		ft.trace("[Start]");

		if (ft.open_documents.length > 0) {
			ft.profile_function(function () {
				ft.array_foreach(ft.open_documents, function (uri) {
					fl.openDocument(uri);
				});
			}, "Open documents");
		}
		
		ft.profile_function(function() {
			ft.array_foreach(fl.documents, function (doc) {
				ft.profile_function(function() {
					try {
						ft.trace_fmt("[Document] '{0}' conversion started...", doc.name);
						ftdoc.prepare(doc);
						ft.trace_fmt("[Document] '{0}' conversion complete!", doc.name);
					} catch (e) {
						ft.trace_fmt("[Document] '{0}' conversion error: '{1}'", doc.name, e);
					}
				}, "Prepare document: '{0}'".format(doc.name));
			});
		}, "Prepare documents");

		if (ft.revert_after_conversion) {
			ft.profile_function(function () {
				ft.array_foreach(fl.documents, function (doc) {
					if (doc.canRevert()) {
						fl.revertDocument(doc);
					}
				});
			}, "Revert documents");
		}

		if (ft.close_after_conversion) {
			ft.profile_function(function () {
				ft.array_foreach(fl.documents, function (doc) {
					fl.closeDocument(doc, false);
				});
			}, "Close documents");
		}

		ft.trace("[Finish]");
	})();
};
