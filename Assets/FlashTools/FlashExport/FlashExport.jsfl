var script_dir = fl.scriptURI.replace('FlashExport.jsfl', '');
fl.runScript(script_dir + 'Internal/FTBase.jsfl');
fl.runScript(script_dir + 'Internal/FTMain.jsfl', "ft_main", {
	profile_mode : true,
	verbose_mode : true,
	graphics_scale : 1.0,
	revert_after_conversion : true,
	optimize_static_items : true,
	optimize_single_graphics : true,
	optimize_big_items : true,
	optimize_small_items : true
});