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
	
	ftd.prepare_bitmaps = function (document) {
		ft.type_assert(document, Document);
		ft.array_foreach(document.library.items, function(item) {
			ft.trace_fmt("-Item: {0}", item.name);
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
	// Main
	// ------------------------------------

	(function () {
		ft.clear_output();
		ft.array_foreach(fl.documents, function (document) {
			ft.trace_fmt("Doc: {0}", document.name);
			ftd.prepare_folders(document);
			ftd.prepare_bitmaps(document);
			ftd.export_swf(document);
		});
	})();
})();