using System;
using System.Collections.Generic;
using System.Threading;

// StateMod_GUIUtil - GUI-related utility functions

/* NoticeStart

CDSS Models Java Library
CDSS Models Java Library is a part of Colorado's Decision Support Systems (CDSS)
Copyright (C) 1994-2019 Colorado Department of Natural Resources

CDSS Models Java Library is free software:  you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    CDSS Models Java Library is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with CDSS Models Java Library.  If not, see <https://www.gnu.org/licenses/>.

NoticeEnd */

//------------------------------------------------------------------------------
// StateMod_GUIUtil - GUI-related utility functions
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
//
// 2003-06-09	J. Thomas Sapienza, RTi	Initial version.
// 2003-07-07	JTS, RTi		Added code for displaying graphs.
// 2003-07-17	JTS, RTi		Added nothingSelected() and 
//					somethingSelected()
// 2003-08-03	Steven A. Malers, RTi	* Constants that were in
//					  StateMod_Control are now in
//					  StateMod_DataSet.
//					* Add READ_START for process listener.
// 2003-08-13	SAM, RTi		* Change ProcessListeners to be more
//					  consistent with StateDMI command
//					  processing and verify that process
//					  listener is working.
//					* Remove map layers window.
//					* Handle basin summary like other
//					  windows.
//					* Remove diagnostics window from the
//					  list of managed windows - it manages
//					  itself.
//					* Clean up the names of the windows to
//					  be consistent with data set
//					  components.
// 2003-08-26	SAM, RTi		Split window manager code into
//					StateMod_DataSet_WindowManager and
//					remove from here.
// 2003-09-24	SAM, RTi		Overload setTitle() to take a JDialog.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
//------------------------------------------------------------------------------

namespace DWR.StateMod
{


	using TSViewJFrame = RTi.GRTS.TSViewJFrame;
	using TS = RTi.TS.TS;
	using JGUIUtil = RTi.Util.GUI.JGUIUtil;
	using SimpleFileFilter = RTi.Util.GUI.SimpleFileFilter;
	using DataSet = RTi.Util.IO.DataSet;
	using ProcessManager = RTi.Util.IO.ProcessManager;
	using PropList = RTi.Util.IO.PropList;
	using Message = RTi.Util.Message.Message;
	using TimeInterval = RTi.Util.Time.TimeInterval;
	using YearType = RTi.Util.Time.YearType;

	/// <summary>
	/// This class provides static data and methods for user interface methods associated with StateMod, mainly
	/// the StateMod GUI.  This class is part of the StateMod package
	/// (rather than StateModGUI) because all of the windows for displaying StateMod
	/// data set components are in the StateMod package).
	/// In the future, this class may be made non-static if it is necessary that a GUI
	/// display more than one data set.
	/// </summary>
	public class StateMod_GUIUtil
	{

	protected internal static string _editorPreference = "NotePad";

	/// <summary>
	/// Settings for use in relaying data back to the calling application via ProcessListener calls.
	/// </summary>
	public const int STATUS_READ_START = 20, STATUS_READ_COMPLETE = 22, STATUS_READ_GVP_START = 50, STATUS_READ_GVP_END = 51; // End reading the GVP file;

	/// <summary>
	/// Add filename filters to the file chooser for time series files.  A general ".stm" entry is added as well
	/// as well rights. </summary>
	/// <param name="fc"> File chooser. </param>
	/// <param name="timeInterval"> the TimeInterval for choices (TimeInterval.DAY, TimeInterval.MONTH,
	/// or TimeInterval.UNKNOWN for all). </param>
	/// <param name="addRightFiles"> indicate whether water right files should be added. </param>
	public static void addTimeSeriesFilenameFilters(JFileChooser fc, int timeInterval, bool addRightFiles)
	{
		// Interleave the entries because TimeInterval.UKNOWN will want a complete list
		if ((timeInterval == TimeInterval.DAY) || (timeInterval == TimeInterval.UNKNOWN))
		{
			SimpleFileFilter sff = new SimpleFileFilter("ddd", "StateMod Diversion Demands (Daily)");
			fc.addChoosableFileFilter(sff);
		}
		if ((timeInterval == TimeInterval.MONTH) || (timeInterval == TimeInterval.UNKNOWN))
		{
			SimpleFileFilter sff = new SimpleFileFilter("ddm", "StateMod Diversion Demands (Monthly)");
			fc.addChoosableFileFilter(sff);
		}
		if ((timeInterval == TimeInterval.MONTH) || (timeInterval == TimeInterval.UNKNOWN))
		{
			SimpleFileFilter sff = new SimpleFileFilter("dda", "StateMod Diversion Demands (Average Monthly)");
			fc.addChoosableFileFilter(sff);
		}
		if ((timeInterval == TimeInterval.MONTH) || (timeInterval == TimeInterval.UNKNOWN))
		{
			SimpleFileFilter sff = new SimpleFileFilter("ddo", "StateMod Diversion Demands Override (Monthly)");
			fc.addChoosableFileFilter(sff);
		}
		if ((timeInterval == TimeInterval.DAY) || (timeInterval == TimeInterval.UNKNOWN))
		{
			SimpleFileFilter sff = new SimpleFileFilter("ddy", "StateMod Diversions, Historical (Daily)");
			fc.addChoosableFileFilter(sff);
		}
		if ((timeInterval == TimeInterval.MONTH) || (timeInterval == TimeInterval.UNKNOWN))
		{
			SimpleFileFilter sff = new SimpleFileFilter("ddh", "StateMod Diversions, Historicial (Monthly)");
			fc.addChoosableFileFilter(sff);
		}
		if (addRightFiles)
		{
			SimpleFileFilter ssf = new SimpleFileFilter("ddr", "StateMod Diversion Rights");
			fc.addChoosableFileFilter(ssf);
		}
		if ((timeInterval == TimeInterval.MONTH) || (timeInterval == TimeInterval.UNKNOWN))
		{
			SimpleFileFilter sff = new SimpleFileFilter("eva", "StateMod Evaporation (Monthly)");
			fc.addChoosableFileFilter(sff);
		}
		if ((timeInterval == TimeInterval.DAY) || (timeInterval == TimeInterval.UNKNOWN))
		{
			SimpleFileFilter sff = new SimpleFileFilter("ifd", "StateMod Instream Flow Demands (Daily)");
			fc.addChoosableFileFilter(sff);
		}
		if ((timeInterval == TimeInterval.MONTH) || (timeInterval == TimeInterval.UNKNOWN))
		{
			SimpleFileFilter sff = new SimpleFileFilter("ifa", "StateMod Instream Flow Demands (Average Monthly)");
			fc.addChoosableFileFilter(sff);
		}
		if ((timeInterval == TimeInterval.MONTH) || (timeInterval == TimeInterval.UNKNOWN))
		{
			SimpleFileFilter sff = new SimpleFileFilter("ifm", "StateMod Instream Flow Demands (Monthly)");
			fc.addChoosableFileFilter(sff);
		}
		if (addRightFiles)
		{
			SimpleFileFilter sff = new SimpleFileFilter("ifr", "StateMod Instream Flow Rights");
			fc.addChoosableFileFilter(sff);
		}
		if ((timeInterval == TimeInterval.MONTH) || (timeInterval == TimeInterval.UNKNOWN))
		{
			SimpleFileFilter sff = new SimpleFileFilter("ddc", "StateMod/StateCU Irrigation Water Requirement (Monthly)");
			fc.addChoosableFileFilter(sff);
		}
		if ((timeInterval == TimeInterval.MONTH) || (timeInterval == TimeInterval.UNKNOWN))
		{
			SimpleFileFilter sff = new SimpleFileFilter("iwr", "StateMod/StateCU Irrigation Water Requirement (Monthly)");
			fc.addChoosableFileFilter(sff);
		}
		if ((timeInterval == TimeInterval.MONTH) || (timeInterval == TimeInterval.UNKNOWN))
		{
			SimpleFileFilter sff = new SimpleFileFilter("xop", "StateMod Output - Operational Rights (Monthly)");
			fc.addChoosableFileFilter(sff);
		}
		if ((timeInterval == TimeInterval.MONTH) || (timeInterval == TimeInterval.UNKNOWN))
		{
			SimpleFileFilter sff = new SimpleFileFilter("pre", "StateMod Precipitation (Monthly)");
			fc.addChoosableFileFilter(sff);
		}
		if ((timeInterval == TimeInterval.DAY) || (timeInterval == TimeInterval.UNKNOWN))
		{
			SimpleFileFilter sff = new SimpleFileFilter("eoy", "StateMod Reservoir Storage (End of Day)");
			fc.addChoosableFileFilter(sff);
		}
		if ((timeInterval == TimeInterval.MONTH) || (timeInterval == TimeInterval.UNKNOWN))
		{
			SimpleFileFilter sff = new SimpleFileFilter("eom", "StateMod Reservoir Storage (End of Month)");
			fc.addChoosableFileFilter(sff);
		}
		if ((timeInterval == TimeInterval.DAY) || (timeInterval == TimeInterval.UNKNOWN))
		{
			SimpleFileFilter sff = new SimpleFileFilter("tad", "StateMod Reservoir Min/Max Targets (Daily)");
			fc.addChoosableFileFilter(sff);
		}
		if ((timeInterval == TimeInterval.MONTH) || (timeInterval == TimeInterval.UNKNOWN))
		{
			SimpleFileFilter sff = new SimpleFileFilter("tar", "StateMod Reservoir Min/Max Targets (Monthly)");
			fc.addChoosableFileFilter(sff);
		}
		if (addRightFiles)
		{
			SimpleFileFilter sff = new SimpleFileFilter("rer", "StateMod Reservoir Rights");
			fc.addChoosableFileFilter(sff);
		}
		if ((timeInterval == TimeInterval.DAY) || (timeInterval == TimeInterval.UNKNOWN))
		{
			SimpleFileFilter sff = new SimpleFileFilter("riy", "StateMod Streamflow, Historical (Daily)");
			fc.addChoosableFileFilter(sff);
		}
		if ((timeInterval == TimeInterval.MONTH) || (timeInterval == TimeInterval.UNKNOWN))
		{
			SimpleFileFilter sff = new SimpleFileFilter("rih", "StateMod Streamflow, Historicial (Monthly)");
			fc.addChoosableFileFilter(sff);
		}
		if ((timeInterval == TimeInterval.DAY) || (timeInterval == TimeInterval.UNKNOWN))
		{
			SimpleFileFilter sff = new SimpleFileFilter("riy", "StateMod Streamflow, Natural (Daily)");
			fc.addChoosableFileFilter(sff);
		}
		if ((timeInterval == TimeInterval.MONTH) || (timeInterval == TimeInterval.UNKNOWN))
		{
			SimpleFileFilter sff = new SimpleFileFilter("rim", "StateMod Streamflow, Natural (Monthly, as input)");
			fc.addChoosableFileFilter(sff);
		}
		if ((timeInterval == TimeInterval.MONTH) || (timeInterval == TimeInterval.UNKNOWN))
		{
			SimpleFileFilter sff = new SimpleFileFilter("xbm", "StateMod Streamflow, Natural (Monthly, as output)");
			fc.addChoosableFileFilter(sff);
		}
		SimpleFileFilter stm_sff = new SimpleFileFilter("stm", "StateMod Time Series");
		fc.addChoosableFileFilter(stm_sff);
		if ((timeInterval == TimeInterval.DAY) || (timeInterval == TimeInterval.UNKNOWN))
		{
			SimpleFileFilter sff = new SimpleFileFilter("wed", "StateMod Well Demands (Daily)");
			fc.addChoosableFileFilter(sff);
		}
		if ((timeInterval == TimeInterval.MONTH) || (timeInterval == TimeInterval.UNKNOWN))
		{
			SimpleFileFilter sff = new SimpleFileFilter("wem", "StateMod Well Demands (Monthly)");
			fc.addChoosableFileFilter(sff);
		}
		if ((timeInterval == TimeInterval.DAY) || (timeInterval == TimeInterval.UNKNOWN))
		{
			SimpleFileFilter sff = new SimpleFileFilter("wey", "StateMod Well Historical Pumping (Daily)");
			fc.addChoosableFileFilter(sff);
		}
		if ((timeInterval == TimeInterval.MONTH) || (timeInterval == TimeInterval.UNKNOWN))
		{
			SimpleFileFilter sff = new SimpleFileFilter("weh", "StateMod Well Historicial Pumping (Monthly)");
			fc.addChoosableFileFilter(sff);
		}
		if (addRightFiles)
		{
			SimpleFileFilter sff = new SimpleFileFilter("wer", "StateMod Well Rights");
			fc.addChoosableFileFilter(sff);
		}
		// Select the "stm" filter as the most generic.
		fc.setFileFilter(stm_sff);
	}

	/// <summary>
	/// Used to set a numeric value in a JTextField.  This method will check the value
	/// and see if it is missing, and if so, will set the JTextField to "".  Otherwise
	/// the text field will be filled with the value of the specified int. </summary>
	/// <param name="i"> the int to check and possibly put in the text field. </param>
	/// <param name="textField"> the JTextField to put the value in. </param>
	public static void checkAndSet(int i, JTextField textField)
	{
		if (StateMod_Util.isMissing(i))
		{
			textField.setText("");
		}
		else
		{
			textField.setText("" + i);
		}
	}

	/// <summary>
	/// Used to set a numeric value in a JTextField.  This method will check the value
	/// and see if it is missing, and if so, will set the JTextField to "".  Otherwise
	/// the text field will be filled with the value of the specified double. </summary>
	/// <param name="d"> the double to check and possibly put in the text field. </param>
	/// <param name="textField"> the JTextField to put the value in. </param>
	public static void checkAndSet(double d, JTextField textField)
	{
		if (StateMod_Util.isMissing(d))
		{
			textField.setText("");
		}
		else
		{
			textField.setText("" + d);
		}
	}

	/// <summary>
	/// Displays a graph for a time series. </summary>
	/// <param name="ts"> the time series to graph. </param>
	/// <param name="title"> the title of the graph </param>
	/// <param name="dataset"> the dataset in which the ts data exists </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void displayGraphForTS(RTi.TS.TS ts, String title, StateMod_DataSet dataset) throws Exception
	public static void displayGraphForTS(TS ts, string title, StateMod_DataSet dataset)
	{
		System.Collections.IList v = new List<object>();
		v.Add(ts);

		// add title to proplist
		PropList props = new PropList("displayGraphForTSProps");
		props.set("titlestring", title);

		displayGraphForTS(v, props, dataset);
	}

	/// <summary>
	/// Displays a graph for a time series. </summary>
	/// <param name="ts"> the time series to graph. </param>
	/// <param name="props"> props defining how the graph should be shown </param>
	/// <param name="dataset"> the dataset in which the ts data exists </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void displayGraphForTS(RTi.TS.TS ts, RTi.Util.IO.PropList props, StateMod_DataSet dataset) throws Exception
	public static void displayGraphForTS(TS ts, PropList props, StateMod_DataSet dataset)
	{
		System.Collections.IList v = new List<object>();
		v.Add(ts);
		displayGraphForTS(v, props, dataset);
	}

	/// <summary>
	/// Displays a graph for a time series. </summary>
	/// <param name="tslist"> Vector of time series to graph </param>
	/// <param name="title"> the title of the graph </param>
	/// <param name="dataset"> the dataset in which the ts data exists </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void displayGraphForTS(java.util.List tslist, String title, StateMod_DataSet dataset) throws Exception
	public static void displayGraphForTS(System.Collections.IList tslist, string title, StateMod_DataSet dataset)
	{
		// add title to proplist
		PropList props = new PropList("displayGraphForTSProps");
		props.set("titlestring", title);
		displayGraphForTS(tslist, props, dataset);
	}

	/// <summary>
	/// Draw a graph. </summary>
	/// <param name="tslist"> Vector of TS to plot. </param>
	/// <param name="props"> Graph properties. </param>
	/// <param name="dataset"> the dataset in which the ts data exists
	/// The properties may contain valid TSViewGraphGUI properies or
	/// may not, in which case defaults will be used.
	/// If some important properties are not set, they are set using the same PropList. </param>
	public static void displayGraphForTS(System.Collections.IList tslist, PropList props, StateMod_DataSet dataset)
	{
		PropList proplist = null;
		if (props == null)
		{
			// Create a new one...
			proplist = new PropList("SMGUIApp");
		}
		else
		{
			// Use what was passed in...
			proplist = props;
		}
		// Make sure some important properties are set...

		if (proplist.getValue("InitialView") == null)
		{
			proplist.set("InitialView", "Graph");
		}
		if (proplist.getValue("DisplayFont") == null)
		{
			proplist.set("DisplayFont", "Courier");
		}
		if (proplist.getValue("DisplaySize") == null)
		{
			proplist.set("DisplaySize", "11");
		}
		if (proplist.getValue("PrintFont") == null)
		{
			proplist.set("PrintFont", "Courier");
		}
		if (proplist.getValue("PrintSize") == null)
		{
			proplist.set("PrintSize", "7");
		}
		if (proplist.getValue("PageLength") == null)
		{
			proplist.set("PageLength", "100");
		}
		// Use titlestring now but Title may be passed in as property
		string title = props.getValue("Title");
		if ((!string.ReferenceEquals(title, null)) && (proplist.getValue("titlestring") == null))
		{
			proplist.set("TitleString", title);
		}

		// CalendarType: Wateryear, IrrigationYear, CalendarYear

		if (dataset.getCyrl() == YearType.CALENDAR)
		{
			proplist.set("CalendarType", "CalendarYear");
		}
		else if (dataset.getCyrl() == YearType.WATER)
		{
			proplist.set("CalendarType", "WaterYear");
		}
		else if (dataset.getCyrl() == YearType.NOV_TO_OCT)
		{
			proplist.set("CalendarType", "IrrigationYear");
		}

		try
		{
			new TSViewJFrame(tslist, proplist);
		}
		catch (Exception e)
		{
			string routine = "StateMod_GUIUtil.displayGraphForTS";
			Message.printWarning(1, routine, "Unable to display graph.");
			Message.printWarning(2, routine, e);
		}
	}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void editFile(String filename) throws Exception
	public static void editFile(string filename)
	{
		string[] command_array = new string[2];
		command_array[0] = _editorPreference;
		command_array[1] = filename;
		ProcessManager p = new ProcessManager(command_array);
		// This will run as a thread until the process is shut down...
		Thread t = new Thread(p);
		t.Start();
	}

	/// <summary>
	/// Return the editor preference (default is "NotePad"), for use in viewing/editing files. </summary>
	/// <returns> the editor preference. </returns>
	public static string getEditorPreference()
	{
		return _editorPreference;
	}

	/// <summary>
	/// Called by JFrames when nothing is selected from the table of ids and names.
	/// This disables all the JComponents on the form that are only relevant if a data object is selected </summary>
	/// <param name="components"> an array of all the JComponents on the form which can be
	/// disabled when nothing is selected. </param>
	/// @deprecated Use JGUIUtil.disableComponents 
	public static void nothingSelected(JComponent[] components)
	{
		JGUIUtil.disableComponents(components, true);
	}

	/// <summary>
	/// Set the program to be used for editing/viewing files. </summary>
	/// <param name="editor"> The editor program to use, as a full path, or the name of a
	/// program in the PATH environment variable (or current directory). </param>
	public static void setEditorPreference(string editor)
	{
		_editorPreference = editor;
	}

	/// <summary>
	/// Sets the title in a uniform fashion, as determined by the values passed in.
	/// The general pattern of the title will be 
	/// "AppName - DataSet Base Name - Window Name (status)" </summary>
	/// <param name="frame"> the frame on which to set the title.  Cannot be null. </param>
	/// <param name="dataset"> the dataset from which to get the base dataset name.   The 
	/// basename can be null or "", in which case it won't be included in the title.
	/// The dataset can be null. </param>
	/// <param name="window_title"> the title of the window.  Can be null or "", in which 
	/// case it won't be included in the title. </param>
	/// <param name="status"> the status of the window.  Can be null or "", in which case 
	/// it won't be included in the title. </param>
	public static void setTitle(JFrame frame, DataSet dataset, string window_title, string status)
	{
		string title = "";
		int count = 0;

		string appName = JGUIUtil.getAppNameForWindows().Trim();
		if (!appName.Trim().Equals(""))
		{
			title += appName;
			count++;
		}

		if (dataset != null)
		{
			string basename = dataset.getBaseName();
			if (!string.ReferenceEquals(basename, null) && !basename.Trim().Equals(""))
			{
				if (count > 0)
				{
					title += " - ";
				}
				title += basename.Trim();
				count++;
			}
		}

		if (!string.ReferenceEquals(window_title, null) && !window_title.Trim().Equals(""))
		{
			if (count > 0)
			{
				title += " - ";
			}
			title += window_title.Trim();
			count++;
		}

		if (!string.ReferenceEquals(status, null) && !status.Trim().Equals(""))
		{
			if (count > 0)
			{
				title += " ";
			}
			title += "(" + status + ")";
		}

		frame.setTitle(title);
	}

	/// <summary>
	/// Sets the title in a uniform fashion, as determined by the values passed in.
	/// The general pattern of the title will be 
	/// "AppName - DataSet Base Name - Window Name (status)" </summary>
	/// <param name="dialog"> the dialog on which to set the title.  Cannot be null. </param>
	/// <param name="dataset"> the dataset from which to get the base dataset name.   The 
	/// basename can be null or "", in which case it won't be included in the title.
	/// The dataset can be null. </param>
	/// <param name="window_title"> the title of the window.  Can be null or "", in which 
	/// case it won't be included in the title. </param>
	/// <param name="status"> the status of the window.  Can be null or "", in which case 
	/// it won't be included in the title. </param>
	public static void setTitle(JDialog dialog, StateMod_DataSet dataset, string window_title, string status)
	{
		string title = "";
		int count = 0;

		string appName = JGUIUtil.getAppNameForWindows().Trim();
		if (!appName.Trim().Equals(""))
		{
			title += appName;
			count++;
		}

		if (dataset != null)
		{
			string basename = dataset.getBaseName();
			if (!string.ReferenceEquals(basename, null) && !basename.Trim().Equals(""))
			{
				if (count > 0)
				{
					title += " - ";
				}
				title += basename.Trim();
				count++;
			}
		}

		if (!string.ReferenceEquals(window_title, null) && !window_title.Trim().Equals(""))
		{
			if (count > 0)
			{
				title += " - ";
			}
			title += window_title.Trim();
			count++;
		}

		if (!string.ReferenceEquals(status, null) && !status.Trim().Equals(""))
		{
			if (count > 0)
			{
				title += " ";
			}
			title += "(" + status + ")";
		}

		dialog.setTitle(title);
	}

	/// <summary>
	/// Called by JFrames when a data object is selected from the table of names and
	/// ids.  This enables what needs to be enabled properly. </summary>
	/// <param name="components"> an array of all the JComponents on the form which can be
	/// enabled when something is selected. </param>
	/// <param name="textUneditables"> an array of the elements in disables[] that should never be editable. </param>
	/// <param name="editable"> whether the form is editable or not. </param>
	/// @deprecated Use JGUIUtil.enableComponents 
	public static void somethingSelected(JComponent[] components, int[] textUneditables, bool editable)
	{
		JGUIUtil.enableComponents(components, textUneditables, editable);
	}

	}

}