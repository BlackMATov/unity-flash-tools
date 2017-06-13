var script_dir = fl.scriptURI.replace('FlashExport_SD.jsfl', '');
fl.runScript(script_dir + 'Internal/FTBase.jsfl');
fl.runScript(script_dir + 'Internal/FTMain.jsfl', "ft_main", {
	graphics_scale : 0.5
});