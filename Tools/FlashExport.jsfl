
var g_export_path = "";
var g_library_path = "";
var g_current_doc = null;
fl.showIdleMessage(false);
Main();

function Main()
{
	var documents = fl.documents;
	if ( documents && documents.length )
	{
		fl.saveAll();
		fl.outputPanel.clear();

		for ( var i = 0; i < documents.length; ++i )
		{
			g_current_doc = documents[i];
			ExportDocument();
			fl.closeDocument(g_current_doc, false);
		};

		alert("-= Process Complete =-");
	}
}

function ExportDocument()
{
	var document_path = g_current_doc.path;
	var last_slash    = document_path.lastIndexOf("/");
	var last_dot      = document_path.lastIndexOf(".");
	
	// g_export_path
	var export_folder = document_path.substr(0, last_slash) + "/../" + document_path.substr(last_slash, last_dot-last_slash) + "_export/";
	g_export_path     = FLfile.platformPathToURI(export_folder);
	
	// g_library_path
	var library_folder = document_path.substr(0, last_slash) + "/LIBRARY/";
	g_library_path     = FLfile.platformPathToURI(library_folder);
	
	// create export folder
	FLfile.remove(g_export_path);
	FLfile.createFolder(g_export_path);
	
	// run
	ExportLibrary(g_export_path + "Library.xml");
}

function PrintNormal( text )
{
	fl.outputPanel.trace(text);
}

function PrintWarning( text )
{
	fl.outputPanel.trace("-= PROCESS WARNING =-\n -- " + text);
}

function ToRad( deg )
{
	return deg * Math.PI / 180;
}

function ExitEditMode()
{
	for ( var i = 0; i < 100; ++i )
		g_current_doc.exitEditMode();
}

function ExportLibrary( file )
{
	ExitEditMode();

	var doc        = g_current_doc; 
	var library    = doc.library;
	var temp_layer = doc.getTimeline().addNewLayer("bme_temp_export_layer");
	var file_str   = "<Library>\n"
	
	for ( var i = 0; i < library.items.length; ++i )
	{
		var item = library.items[i];
		
		if ( item.itemType == "folder" )
		{
			// create folder
			var path = g_export_path + item.name;
			FLfile.createFolder(path);
		}
		else
		{	
			if ( item.itemType == "bitmap" )
			{
				file_str +=
					"\t<Asset"     +
					" name='"      + item.name           + "'" +
					" type='"      + item.itemType       + "'" +
					" filename='"  + item.name           + "'" +
					" smoothing='" + item.allowSmoothing + "'/>\n";
				
				// copy bitmap to export
				FLfile.copy(g_library_path + item.name, g_export_path + item.name);
			}
			else if ( item.itemType == "movie clip" || item.itemType == "graphic" || item.itemType == "component" )
			{
				file_str +=
					"\t<Asset"     +
					" name='"      + item.name          + "'" +
					" type='"      + item.itemType      + "'" +
					" filename='"  + item.name + ".xml" + "'";

				// create and get default component parameters

				var is_parameters = false;
				if ( library.addItemToDocument({x:20, y:20}, item.name) &&
					doc.selection && doc.selection.length > 0 )
				{
					var sel = doc.selection[0];
					if ( sel.parameters && sel.parameters.length > 0 )
					{
						is_parameters = true;
						file_str += ">\n";
						file_str = ExportParameters(sel.parameters, file_str, "\t\t");
					}
				}

				if ( is_parameters )
					file_str += "\t</Asset>\n";
				else
					file_str += "/>\n";

				// export item timeline

				library.editItem(item.name);
				ExportTimeline(g_export_path + item.name + ".xml")
				ExitEditMode();
			}
			else
			{
				PrintWarning("Unsupported library type(" + item.itemType + ")");
			}
		}
	}
	
	FLfile.write(file, file_str + "</Library>", "append");
	doc.getTimeline().deleteLayer();
}

function ExportTimeline( file )
{
	var doc_timeline = g_current_doc.getTimeline();

	// [2:-] for disable dublicate single frame
	if ( doc_timeline.frameCount > 1 )
	{
		doc_timeline.selectAllFrames();
		doc_timeline.convertToKeyframes();
	}
	
	var doc_frames    = doc_timeline.frameCount;
	var doc_framerate = g_current_doc.frameRate;
	
	var file_str = "<Timeline";
	file_str += " frames='" + doc_frames    + "'";
	file_str += " speed='"  + doc_framerate + "'";
	file_str += ">\n";
	
	for ( var i = doc_timeline.layerCount-1; i >= 0; --i )
	{
		var doc_layer = doc_timeline.layers[i];
		if ( !doc_layer || !doc_layer.visible )
			continue;
		
		file_str = ExportLayer(doc_layer, i, file_str, "\t")
	}
	
	FLfile.write(file, file_str + "</Timeline>", "append" );
}

function ExportLayer( layer, index, file_str, prefix )
{
	file_str += prefix + "<Layer name='" + layer.name + "'>\n";
	
	for ( var i = 0; i < layer.frameCount; ++i )
	{
		var frame = layer.frames[i];
		if ( !frame || i != frame.startFrame )
			continue;
			
		file_str = ExportFrame(frame, i, file_str, prefix + "\t");
	}
	
	return file_str + prefix + "</Layer>\n";
}

function ExportFrame( frame, index, file_str, prefix )
{
	file_str += prefix + "<Frame";
	file_str += " name='"     + frame.name     + "'";
	file_str += " index='"    + index          + "'";
	file_str += " duration='" + frame.duration + "'";
	file_str += ">\n";
	
	for ( var i = 0; i < frame.elements.length; ++i )
	{
		var element = frame.elements[i];
		if ( element.instanceType == "symbol" ||  element.instanceType == "bitmap" )
		{
			file_str = ExportElement(element, i, file_str, prefix + "\t");
		}
		else
		{
			PrintWarning("Unsupported element type(" + element.instanceType + ")");
		}
	}
	
	return file_str + prefix + "</Frame>\n";
}

function ExportElement( element, index, file_str, prefix )
{
	file_str += prefix + "<Element";
	file_str += " name='"  + element.name                 + "'";
	file_str += " type='"  + element.libraryItem.itemType + "'";
	file_str += " asset='" + element.libraryItem.name     + "'";

	if ( element.libraryItem.linkageExportForAS )
	{
		file_str += " base_class='" + element.libraryItem.linkageBaseClass + "'";
		file_str += " class='"      + element.libraryItem.linkageClassName + "'";
	}

	if ( element.libraryItem.linkageExportForRS )
	{
		file_str += " url='" + element.libraryItem.linkageURL + "'";
	}

	file_str += ">\n";
	
	file_str = ExportInstance(element, file_str, prefix + "\t");
	
	return file_str + prefix + "</Element>\n";
}

function ExportInstance( instance, file_str, prefix )
{
	// --------------------------------
	// parameters
	// --------------------------------

	if ( instance.parameters && instance.parameters.length > 0 )
	{
		file_str = ExportParameters(instance.parameters, file_str, prefix);
	}

	// --------------------------------
	// transform
	// --------------------------------

	file_str += prefix + "<Transform";
	
	var position   = "'" + instance.x      + ";" + instance.y      + "'";
	var scale      = "'" + instance.scaleX + ";" + instance.scaleY + "'";
	
	file_str += " position=" + position;
	file_str += " scale="    + scale;
	
	if ( isNaN(instance.rotation) )
	{
		if ( Math.abs(Math.abs(instance.skewX) - Math.abs(instance.skewY)) > 0.01 )
			PrintWarning("Skew transformation unsupported! Element type(" + instance.libraryItem.name + ")");
		
		var rotation = "'" + ToRad(instance.skewX) + "'";
		file_str += " rotation=" + rotation;
	}
	else
	{
		var rotation = "'" + ToRad(instance.rotation) + "'";
		file_str += " rotation=" + rotation;
	}

	// --------------------------------
	// color
	// --------------------------------

	if ( instance.colorAlphaPercent )
	{
		var alpha = "'" + instance.colorAlphaPercent / 100 + "'";
		file_str += " alpha=" + alpha;
	}

	// --------------------------------
	// blend
	// --------------------------------

	if ( instance.blendMode )
	{
		var blend = "'ALPHA'";
		     if ( instance.blendMode == "normal" )    blend = "'ALPHA'";
		else if ( instance.blendMode == "add" )       blend = "'ADD'";
		else if ( instance.blendMode == "multiply" )  blend = "'MULTIPLY'";
		else PrintWarning("Unsupported blend mode(" + instance.blendMode + ")");
		
		file_str += " blend=" + blend;
	}
	
	return file_str + "/>\n";
}

function ExportParameters( params, file_str, prefix )
{
	file_str += prefix + "<Parameters count='" + params.length + "'>\n";

	for ( var i = 0; i < params.length; ++i )
	{
		var param = params[i];

		if ( param.valueType == "Number" || param.valueType == "Boolean" || param.valueType == "String" )
		{
			var name  = "'" + param.name      + "'";
			var value = "'" + param.value     + "'";
			var type  = "'" + param.valueType + "'";

			if ( param.valueType == "Number" )
				value = value.replace(",", ".");

			file_str += prefix + "\t<Parameter";
			file_str += " name="  + name;
			file_str += " value=" + value;
			file_str += " type="  + type;
			file_str += "/>\n";
		}
		else
		{
			PrintWarning("Unsupported parameter type(" + param.valueType + ")");
		}
	}

	return file_str + prefix + "</Parameters>\n";
}
