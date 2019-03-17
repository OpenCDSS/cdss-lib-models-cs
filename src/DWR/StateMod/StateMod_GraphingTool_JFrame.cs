using System;
using System.Collections.Generic;

// StateMod_GraphingTool_JFrame - dialog to create templates for graphing

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
// StateMod_GraphingTool_JFrame - dialog to create templates for graphing
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
// 
// 24 Dec 1997	Catherine E.		Created initial version of class
//		Nutting-Lane, RTi
// 03 Jul 1998	CEN, RTi		Modify to work for graphing
//					or for the output control edit.
// 28 Sep 1998	SAM, RTi		Added ability to specify the
//					period when getting time series
//					so that graph performs better.
// 29 Sep 1998	CEN, RTi		Adding radio buttons to toggle
//					user entered id vs. generated lists.
// 20 Nov 1998	CEN, RTi		Adding big picture stuff.
// 21 Dec 1998	CEN, RTi		Added try/catch to IO routines
// 12 May 1999	CEN, RTi		Changed .att to .txt for big
//					picture output.
// 25 Oct 1999	CEN, RTi		Added _templateLocation
// 06 Nov 2000	CEN, RTi		Added auto-line copy checkbox preference
// 01 Apr 2001	Steven A. Malers, RTi	Change GUI to JGUIUtil.  Add finalize().
//					Remove import *.
// 04 May 2001	SAM, RTi		Verify TSView usage.  Enable all time
//					series defined in
//					TSGraph.GRAPH_TYPE_NAMES.  Remove limit
//					that only monthly time series can be
//					displayed.
// 19 Jul 2001	SAM, RTi		Change so the output file(s)are removed
//					before each run to make sure that new
//					data are used.
// 13 Aug 2001	SAM, RTi		Add wells(xwg).
// 17 Aug 2001	SAM, RTi		Add argument to RunSMOption to wait
//					after the run so that the output file is
//					created.  This may only be necessary on
//					fast machines.
// 23 Sep 2001	SAM, RTi		Change Table to DataTable.  Lengthen the
//					total time in waitForFile()calls to
//					60 seconds total.
// 2001-12-11	SAM, RTi		Update to use NoSwing GeoView classes so
//					parallel Swing development can occur.
// 2002-03-07	SAM, RTi		Use TSProduct to get the graph types.
// 2002-06-20	SAM, RTi		Update to pass well information to big
//					picture grid.
// 2002-07-26	SAM, RTi		Update to support the new GeoView for
//					the big picture plot.
// 2002-08-02	SAM, RTi		Make additional enhancements to
//					streamline running delplt.
//					Add __templateLocation to try to avoid
//					possible side-effects from multiple
//					windows being open at the same time.
// 2002-08-07	SAM, RTi		Figure out why the graph was not working
//					correctly - need to remove old files
//					each time.  Remove StringTokenizer - for
//					now do not handle reservoir accounts.
// 2002-08-26	SAM, RTi		Change constructor to take an integer
//					for the SMMainGUI interface type.
//					Change big picture constructor to take
//					a reference to SMMainGUI to pass
//					information.
// 2002-09-12	SAM, RTi		For graphing, add Baseflow as a data
//					type if the node is a baseflow node with
//					baseflow time series.
//					Also add a button to retrieve the time
//					series before graphing.  This allows
//					different graphs to be shown without
//					rereading.
// 2002-09-19	SAM, RTi		Use isDirty()instead of setDirty()to
//					indicate edits.  Add a JTextField at the
//					bottom of the window to indicate the
//					status of the GUI.
// 2002-10-11	SAM, RTi		Change ProcessManager* to
//					ProcessManager1* to be allow transition
//					to Java 1.4.x.
// 2002-10-16	SAM, RTi		Move back to ProcessManger since the
//					updated version seems to work well with
//					Java 1.18 and 1.4.0.  Use version that
//					takes command arguments rather than a
//					single string.
//------------------------------------------------------------------------------
// 2003-07-02	J. Thomas Sapienza, RTi	Began work on initial swing version.
// 2003-07-07	JTS, RTi		Continued work on GUI.
// 2003-07-08	JTS, RTi		Graphing implemented.
// 2003-07-15	JTS, RTi		Changed to use new dataset design.
// 2003-07-23	JTS, RTi		Updated JWorksheet code following
//					JWorksheet revisions.
// 2003-07-30	SAM, RTi		* Change COMP_STREAM_STATIONS to
//					  COMP_RIVER_STATIONS.
//					* Change StateMod_RiverInfo to
//					  StateMod_RiverStation.
//					* Change runStateModOption() to
//					  runStateMod().
//					* Change to get control and response
//					  information from the data set.
// 2003-08-14	SAM, RTi		Change wereTSRead() to areTSRead().
// 2003-08-16	SAM, RTi		Change the window type to
//					WINDOW_GRAPHING_TOOL.
// 2003-08-26	SAM, RTi		Enable StateMod_DataSet_WindowManager.
// 2003-09-11	SAM, RTi		Update due to changes in the river
//					station components.
// 2003-09-23	JTS, RTi		Uses new StateMod_GUIUtil code for
//					setting titles.
// 2003-10-26	SAM, RTi		* Remove needToRerunSM() - not needed
//					  because binary data files are read.
//					* Change from "template" notation to
//					  "time series product" notation.
//					* Comment out Help button - use tool
//					  tips instead.
//					* Implement TSSupplier to supply the
//					  time series for graphs - although for
//					  now don't use.
//					* Use the JFileChooserFactory to get the
//					  file chooser because of a bug in Java.
//					* Change checkButtons() to
//					  checkGUIState() to be consistent with
//					  other code - and more than just
//					  buttons are checked.
// 2003-11-29	SAM, RTi		When loading a graph, handle reservoir
//					accounts in the location part of the
//					identifier.
// 2004-01-21	JTS, RTi		Updated to use JScrollWorksheet and
//					the new row headers.
// 2006-01-23	SAM, RTi		* Reword dialog to warn user to save
//					  before closing (now consistent with
//					  GRTS).
// 		JTS, RTi		* Added a boolean so that item state
//					  changes can be ignored when loading
//					  a TSP.
//					* Changed some column reference numbers
//					  to reflect the fact that the worksheet
//					  column numbering is now 0-based.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
//------------------------------------------------------------------------------
// EndHeader

namespace DWR.StateMod
{


	using TSGraphType = RTi.GRTS.TSGraphType;
	using TSProduct = RTi.GRTS.TSProduct;
	using DateValueTS = RTi.TS.DateValueTS;
	using TS = RTi.TS.TS;
	using TSIdent = RTi.TS.TSIdent;
	using TSSupplier = RTi.TS.TSSupplier;
	using JFileChooserFactory = RTi.Util.GUI.JFileChooserFactory;
	using JGUIUtil = RTi.Util.GUI.JGUIUtil;
	using JScrollWorksheet = RTi.Util.GUI.JScrollWorksheet;
	using JWorksheet = RTi.Util.GUI.JWorksheet;
	using ResponseJDialog = RTi.Util.GUI.ResponseJDialog;
	using SimpleFileFilter = RTi.Util.GUI.SimpleFileFilter;
	using SimpleJButton = RTi.Util.GUI.SimpleJButton;
	using SimpleJComboBox = RTi.Util.GUI.SimpleJComboBox;
	using DataSetComponent = RTi.Util.IO.DataSetComponent;
	using IOUtil = RTi.Util.IO.IOUtil;
	using PropList = RTi.Util.IO.PropList;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;
	using DateTime = RTi.Util.Time.DateTime;
	using YearType = RTi.Util.Time.YearType;

	/// <summary>
	/// This class is a gui for displaying and editing graphing templates.
	/// </summary>
	public class StateMod_GraphingTool_JFrame : JFrame, ActionListener, ItemListener, MouseListener, TSSupplier, WindowListener
	{

	/// <summary>
	/// Whether the data has been changed or not, to know whether to pop up a dialog
	/// box warning of changes.
	/// </summary>
	private bool __dirty = false;

	/// <summary>
	/// Whether item state change events should be ignored or not.
	/// </summary>
	private bool __ignoreItemStateChange = false;

	/// <summary>
	/// Button labels.
	/// </summary>
	private readonly string __BUTTON_ADD_ROW = "Add a Row (Append)", __BUTTON_DELETE_ROW = "Delete Selected Row(s)", __BUTTON_CLEAR_TEMPLATE = "Delete All Rows", __BUTTON_LOAD_TEMPLATE = "Load Graph", __BUTTON_SAVE_TEMPLATE = "Save Graph", __BUTTON_CLOSE = "Close", __BUTTON_EXPORT_DATA = "Export Time Series Data", __BUTTON_GET_TIME_SERIES = "Get Time Series for Graph", __BUTTON_GRAPH = "Graph:";

	/// <summary>
	/// Check box for specifying to copy data from the row above to a new row when
	/// a new row is added.
	/// </summary>
	private JCheckBox __autoLineCopyJCheckBox;

	/// <summary>
	/// Status and message text fields at the bottom of the GUI.
	/// </summary>
	private JTextField __messageTextField, __statusTextField;

	/// <summary>
	/// Worksheet for displaying data.
	/// </summary>
	private JWorksheet __worksheet;

	/// <summary>
	/// GUI Buttons.
	/// </summary>
	private SimpleJButton __addRow_JButton, __deleteRow_JButton, __deleteAll_JButton, __load_JButton, __save_JButton, __help_JButton, __close_JButton, __getTS_JButton, __graph_JButton, __export_JButton;

	/// <summary>
	/// Combobox for selecting the kind of graph to make.
	/// </summary>
	private SimpleJComboBox __graphType_JComboBox;

	/// <summary>
	/// Vector of time series to graph.
	/// </summary>
	private System.Collections.IList __tsVector = null;

	/// <summary>
	/// The dataset for the statemod run containing all the data.
	/// </summary>
	private StateMod_DataSet __dataset = null;

	/// <summary>
	/// Data set window manager.
	/// </summary>
	private StateMod_DataSet_WindowManager __dataset_wm;

	/// <summary>
	/// Table model for the worksheet.
	/// </summary>
	private StateMod_GraphingTool_TableModel __tableModel = null;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="dataset"> the dataset containing the statemod data </param>
	public StateMod_GraphingTool_JFrame(StateMod_DataSet dataset, StateMod_DataSet_WindowManager dataset_wm)
	{
		StateMod_GUIUtil.setTitle(this, dataset, "Graphing Tool", null);
		JGUIUtil.setIcon(this, JGUIUtil.getIconImage());

		__dataset = dataset;
		__dataset_wm = dataset_wm;

		setupGUI();
	}

	/// <summary>
	/// Responds to action performed events. </summary>
	/// <param name="ae"> the ActionEvent that happened. </param>
	public virtual void actionPerformed(ActionEvent ae)
	{
		string routine = "StateMod_GraphingTool_JFrame.actionPerformed";
		object o = ae.getSource();

		if (o == __addRow_JButton)
		{
			if (!__tableModel.canAddNewRow())
			{
				return;
			}
			__dirty = true;
			int row = __worksheet.getRowCount();

			TSIdent new_tsident = new TSIdent();
			if (autoLineCopy() && (row > 0))
			{
				// Copy the previous row's contents.  This facilitates
				// setting up the new row...
				TSIdent old_tsident = (TSIdent) __worksheet.getRowData((row - 1));
				new_tsident.setType(old_tsident.getType());
				// The alias is used for the station type...
				new_tsident.setAlias(old_tsident.getAlias());
				new_tsident.setLocation(old_tsident.getLocation());
				try
				{
					new_tsident.setInterval(old_tsident.getInterval());
				}
				catch (Exception)
				{
					// Should not happen.
				}
				new_tsident.setInputType(old_tsident.getInputType());
				new_tsident.setInputName(old_tsident.getInputName());
				__worksheet.addRow(new_tsident);
				__tableModel.fillIDColumn(row, new_tsident.getAlias());
				__tableModel.fillDataTypeColumn(row, false, new_tsident.getAlias(), new_tsident.getLocation(), new_tsident.getInterval());
				__tableModel.setValueAt(old_tsident.getAlias(), row, __tableModel._COL_STATION_TYPE);
				__tableModel.setValueAt(old_tsident.getLocation(), row, __tableModel._COL_ID);
				__tableModel.setValueAt(old_tsident.getInterval(), row, __tableModel._COL_INTERVAL);
				__tableModel.setValueAt(old_tsident.getType(), row, __tableModel._COL_DATA_TYPE);
				__tableModel.setValueAt(old_tsident.getInputType(), row, __tableModel._COL_INPUT_TYPE);
				__tableModel.setValueAt(old_tsident.getInputName(), row, __tableModel._COL_INPUT_NAME);
	//			__worksheet.addRow(new_tsident);

	// REVISIT 
	// add row needs to set up the column information with the old row's data
	// instead of just adding a row as it does.
			}
			else
			{ // Add the a blank row using the new TSIdent...
				__worksheet.addRow(new_tsident);
				__worksheet.setCellEditable(row, __tableModel._COL_ID, false);
				__worksheet.setCellEditable(row, __tableModel._COL_DATA_TYPE, false);
			}
			checkGUIState();
		}
		else if (o == __deleteAll_JButton)
		{
			// Nothing in template so no need to save...
			__dirty = false;
			__worksheet.clear();
			setMessages("Add time series to graph", "Ready");
			checkGUIState();
		}
		else if (o == __close_JButton)
		{
			closeWindow();
		}
		else if (o == __deleteRow_JButton)
		{
			int[] rows = __worksheet.getSelectedRows();

			int length = rows.Length;

			if (length == 0)
			{
				return;
			}

			for (int i = (length - 1); i >= 0; i--)
			{
				__worksheet.deleteRow(rows[i]);
			}
			__dirty = true;
			// Disable the graph button because different time series are
			// now available...
			__graph_JButton.setEnabled(false);
			setMessages("Time series list has changed.", "Ready");
			checkGUIState();
		}
		else if (o == __export_JButton)
		{
			JGUIUtil.setWaitCursor(this, true);
			string lastDirectorySelected = JGUIUtil.getLastFileDialogDirectory();

			JFileChooser fc = JFileChooserFactory.createJFileChooser(lastDirectorySelected);

			fc.setDialogTitle("Export Time Series Data");
			SimpleFileFilter dv_ff = new SimpleFileFilter("dv", "Date Value Time Series File");
			fc.addChoosableFileFilter(dv_ff);
			SimpleFileFilter stm_ff = new SimpleFileFilter("stm", "StateMod Time Series File");
			fc.addChoosableFileFilter(stm_ff);
			fc.setAcceptAllFileFilterUsed(false);
			fc.setFileFilter(dv_ff);
			fc.setDialogType(JFileChooser.SAVE_DIALOG);

			JGUIUtil.setWaitCursor(this, false);
			int retVal = fc.showSaveDialog(this);
			if (retVal != JFileChooser.APPROVE_OPTION)
			{
				return;
			}

			string currDir = (fc.getCurrentDirectory()).ToString();

			if (!currDir.Equals(lastDirectorySelected, StringComparison.OrdinalIgnoreCase))
			{
				JGUIUtil.setLastFileDialogDirectory(currDir);
			}
			string path = fc.getSelectedFile().getPath();

			JGUIUtil.setWaitCursor(this, true);

			if (fc.getFileFilter() == stm_ff)
			{
				try
				{
					PropList props = new PropList("StateModGUI");
					props.set("OutputFile=" + path);
					// Calendar type from data set...
					if (__dataset.getCyrl() == YearType.WATER)
					{
						props.set("CalendarType=WYR");
					}
					else if (__dataset.getCyrl() == YearType.NOV_TO_OCT)
					{
						props.set("CalendarType=IYR");
					}
					StateMod_TS.writeTimeSeriesList(__tsVector, props);
				}
				catch (Exception e)
				{
					JGUIUtil.setWaitCursor(this, false);
					Message.printWarning(1, routine, "Error writing time series to StateMod " + "file \"" + path + "\"");
					Message.printWarning(2, routine, e);
				}
			}
			else if (fc.getFileFilter() == dv_ff)
			{
				// Write the time series as a date value file...
				try
				{
					DateValueTS.writeTimeSeriesList(__tsVector, path);
				}
				catch (Exception e)
				{
					JGUIUtil.setWaitCursor(this, false);
					Message.printWarning(1, routine, "Error writing time series to DateValue " + "file \"" + path + "\"");
					Message.printWarning(2, routine, e);
				}
			}
			JGUIUtil.setWaitCursor(this, false);
		}
		else if (o == __getTS_JButton)
		{
			getTimeSeries();
			checkGUIState();
		}
		else if (o == __graph_JButton)
		{
			graphTimeSeries();
		}
		else if (o == __help_JButton)
		{
			// REVISIT HELP (JTS - 2003-07-07)
		}
		else if (o == __load_JButton)
		{
			loadTSProduct();
			checkGUIState();
		}
		else if (o == __save_JButton)
		{
			saveTSProduct();
		}
	}

	/// <summary>
	/// Checks to see if the previous line should be automatically copied down to the 
	/// next one when a new line is added. </summary>
	/// <returns> true if the line should be copied, false if not </returns>
	public virtual bool autoLineCopy()
	{
		if (__autoLineCopyJCheckBox.isSelected())
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	/// <summary>
	/// Check the status of the the interface and enable/disable components as
	/// appropriate.
	/// </summary>
	public virtual void checkGUIState()
	{ // Only use to test whether the grid has data rows.  If so then the
		// "Get Time Series" button is set to enabled.
		if (__worksheet.getRowCount() > 0)
		{
			JGUIUtil.setEnabled(__getTS_JButton, true);
			JGUIUtil.setEnabled(__save_JButton, true);
			JGUIUtil.setEnabled(__deleteAll_JButton, true);
		}
		else
		{
			JGUIUtil.setEnabled(__getTS_JButton, false);
			JGUIUtil.setEnabled(__save_JButton, false);
			JGUIUtil.setEnabled(__deleteAll_JButton, false);
		}

		if (__worksheet.getSelectedRowCount() > 0)
		{
			JGUIUtil.setEnabled(__deleteRow_JButton, true);
		}
		else
		{
			JGUIUtil.setEnabled(__deleteRow_JButton, false);
		}

		if ((__tsVector == null) || (__tsVector.Count == 0))
		{
			JGUIUtil.setEnabled(__graph_JButton, false);
			JGUIUtil.setEnabled(__export_JButton, false);
			JGUIUtil.setEnabled(__graphType_JComboBox, false);
		}
		else
		{ // Make sure that at least one time series is not null...
			bool enabled = false;
			int size = __tsVector.Count;
			for (int i = 0; i < size; i++)
			{
				if (((TS)__tsVector[i]) != null)
				{
					enabled = true;
					break;
				}
			}
			JGUIUtil.setEnabled(__graph_JButton, enabled);
			JGUIUtil.setEnabled(__export_JButton, enabled);
			JGUIUtil.setEnabled(__graphType_JComboBox, enabled);
		}
	}

	/// <summary>
	/// Closes the window.
	/// </summary>
	private void closeWindow()
	{
		// set files to clean, all of these files must be saved while
		// the StateMod_GraphingTool_JFrame is still available.
		if (__dirty)
		{
			int x = (new ResponseJDialog(this, "Time Series Product has Changed", "Changes have been made.  Do you want to save changes?", ResponseJDialog.YES | ResponseJDialog.NO | ResponseJDialog.CANCEL)).response();
			if (x == ResponseJDialog.YES)
			{
				// Save the TSProduct...
				saveTSProduct();
			}
			if (x == ResponseJDialog.CANCEL)
			{
				// Just return control back...
				setDefaultCloseOperation(WindowConstants.DO_NOTHING_ON_CLOSE);
				return;
			}
		}
		// If here then YES or NO was selected...
		if (__dataset_wm != null)
		{
			__dataset_wm.closeWindow(StateMod_DataSet_WindowManager.WINDOW_GRAPHING_TOOL);
		}
		else
		{
			JGUIUtil.close(this);
		}
	}

	/// <summary>
	/// Clean up before garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~StateMod_GraphingTool_JFrame()
	{
		__autoLineCopyJCheckBox = null;
		__messageTextField = null;
		__statusTextField = null;
		__worksheet = null;
		__addRow_JButton = null;
		__deleteRow_JButton = null;
		__deleteAll_JButton = null;
		__load_JButton = null;
		__save_JButton = null;
		__help_JButton = null;
		__close_JButton = null;
		__getTS_JButton = null;
		__graph_JButton = null;
		__export_JButton = null;
		__graphType_JComboBox = null;
		__tsVector = null;
		__dataset = null;
		__tableModel = null;
	}

	/// <summary>
	/// Retrieve the time series to be used in the graph. </summary>
	/// <returns> time series based on the identifiers in the graph template. </returns>
	protected internal virtual void getTimeSeries()
	{
		string routine = "StateMod_GraphingTool_JFrame.getTimeSeries";

		// The worksheet data are a Vector of TSIdent that can be used to
		// retrieve information for getting the time series.  The only tricky
		// part is the alias is used for the station type, to facilitate getting
		// time series from the correct component.  The general procedure to
		// get prepare time series for graphing is:
		//
		// 1)	If the input type is StateModB, use the TSIdent information to
		//	form a TSID string for the StateMod_BTS.readTimeSeries() method.
		// 2)	If the input type is StateMod, check the memory first to see if
		//	the time series is available.  If so, use it as is.  If not,
		//	use StateMod_TS.readTimeSeries() to read the time seris from
		//	the text input file.

		System.Collections.IList tsident_Vector = __worksheet.getAllData();

		// tsident_Vector will contain vector of TSIdent objects.
		int numTS = tsident_Vector.Count;
		Message.printStatus(1, routine, "User specified " + numTS + " TS");

		// Initialize the time series vector to all null time series.  This
		// will keep the rows of the worksheet consistent with the time series
		// list...

		__tsVector = new List<object>(numTS);
		for (int i = 0; i < numTS; i++)
		{
			__tsVector.Add(null);
		}

		// Fill out the run period dates so that we can optimize the read
		// performance(although for the most part we will likely read the
		// last year(s)from the TS files!)...

		DateTime run_date1 = new DateTime(DateTime.PRECISION_MONTH);
		DateTime run_date2 = new DateTime(DateTime.PRECISION_MONTH);

		if (__dataset.getCyrl() == YearType.CALENDAR)
		{
			// Calendar year...
			run_date1.setMonth(1);
			run_date1.setYear(__dataset.getIystr());
			run_date2.setMonth(12);
			run_date2.setYear(__dataset.getIyend());
		}
		else if (__dataset.getCyrl() == YearType.NOV_TO_OCT)
		{
			// Irrigation year...
			run_date1.setMonth(11);
			run_date1.setYear(__dataset.getIystr() - 1);
			run_date2.setMonth(10);
			run_date2.setYear(__dataset.getIyend());
		}
		else
		{ // Default is water year...
			run_date1.setMonth(10);
			run_date1.setYear(__dataset.getIystr() - 1);
			run_date2.setMonth(9);
			run_date2.setYear(__dataset.getIyend());
		}

		// Loop through the TSIdent and retrieve time series...

		DateTime req_date1 = null, req_date2 = null; // For now, get all.
		TSIdent tsident;
		string req_units = null; // Use input.
		TS ts = null;
		string input_type = null; // StateMod or StateModB
		string input_name = null; // Input file name
		string id; // Location for reset.
		string data_type = null; // Data type for reset.
		JGUIUtil.setWaitCursor(this, true);
		for (int i = 0; i < numTS; i++)
		{
			setMessages("Retrieving time series " + (i + 1) + " of " + numTS, "Wait");
			try
			{
				tsident = new TSIdent((TSIdent)tsident_Vector[i]);
			}
			catch (Exception)
			{
				// Should not happen because it is a copy...
				Message.printWarning(2, routine, "Error creating TSIdent copy for \"" + (TSIdent)tsident_Vector[i]);
				continue; // TS will be null in the Vector of TS
			}
			// Reset the ID to the first token because the ID from the
			// display may contain the name ID (name)...
			id = StringUtil.getToken(tsident.getLocation(), " (", 0, 0);
			tsident.setLocation(id);
			// Reset the data type to the first token because the data type
			// from the display contains " - Input" and " - Output"...
			data_type = StringUtil.getToken(tsident.getType(), " ", 0, 0);
			tsident.setType(data_type);
			ts = null;
			input_type = tsident.getInputType();
			if (input_type.Equals("StateModB", StringComparison.OrdinalIgnoreCase))
			{
				// Read the time series from the binary file, using the
				// full time series identifier with input name.  The
				// working directory will be used to make relative paths
				// absolute.  Note that the binary file uses the river
				// node ID.  However, we don't want the TSID to
				// magically switch from an ID they picked to a river
				// node, so always pass the "common" ID and let the
				// StateMod_BTS translate to a river node internally.
				input_name = tsident.getInputName();
				if (input_name.StartsWith("*", StringComparison.Ordinal))
				{
					// Replace the asterisk with the current
					// base name...
					input_name = __dataset.getBaseName() + input_name.Substring(1);
				}
				try
				{
					ts = StateMod_BTS.readTimeSeries(tsident.ToString(true), input_name, req_date1, req_date2, req_units, true);
				}
				catch (Exception e)
				{
					Message.printWarning(2, routine, "Error reading time series \"" + tsident.ToString(true) + "\".");
					Message.printWarning(2, routine, e);
					ts = null;
				}
			}
			else
			{ // Input time series that need to be read from
				// memory or from the text file.
				try
				{
					ts = __dataset.getTimeSeries(tsident.ToString(true), req_date1, req_date2, req_units, true);
				}
				catch (Exception e)
				{
					Message.printWarning(2, routine, "Error getting time series \"" + tsident.ToString(true) + "\".");
					Message.printWarning(2, routine, e);
					ts = null;
				}
			}

			if (ts == null)
			{
				Message.printWarning(1, routine, "Error reading time series for row " + (i + 1), this);
			}

			__tsVector[i] = ts;
		}

		if (__tsVector.Count > 0)
		{
			setMessages("Retrieved " + __tsVector.Count + " time series " + "out of " + numTS + ".  Use Graph or Export.", "Ready");
		}
		else
		{
			setMessages("Retrieved " + __tsVector.Count + " time series " + "out of " + numTS + ". Unable to Graph or Export.", "Ready");
		}
		JGUIUtil.setWaitCursor(this, false);
	}

	/// <summary>
	/// Method needed for TSSupplier interface.
	/// Return the name of this supplier. </summary>
	/// <returns> the name of this supplier. </returns>
	public virtual string getTSSupplierName()
	{
		return "StateMod GUI";
	}

	/// <summary>
	/// Graph the time series in the worksheet.
	/// </summary>
	private void graphTimeSeries()
	{
		JGUIUtil.setWaitCursor(this, true);
		setMessages("Processing Time Series", "Wait");
		PropList proplist = new PropList("StateModGUI.GraphWindow");
		string graphType = __graphType_JComboBox.getSelected();
		proplist.set("GraphType", graphType);
		StateMod_GUIUtil.displayGraphForTS(__tsVector, proplist, __dataset);
		setMessages("", "Ready");
		JGUIUtil.setWaitCursor(this, false);
	}

	/// <summary>
	/// Responds to item state changed event. </summary>
	/// <param name="ie"> the ItemEvent that heppened. </param>
	public virtual void itemStateChanged(ItemEvent ie)
	{
		if (ie.getSource() == __graphType_JComboBox && ie.getStateChange() == ItemEvent.SELECTED && !__ignoreItemStateChange)
		{
			graphTimeSeries();
		}
	}

	/// <summary>
	/// Load a TSProduct file and setup the rows in the worksheet accordingly.
	/// </summary>
	private void loadTSProduct()
	{
		string routine = "StateMod_GraphingTool_JFrame.loadTSProduct";
		JGUIUtil.setWaitCursor(this, true);
		string lastDirectorySelected = JGUIUtil.getLastFileDialogDirectory();

		JFileChooser fc = JFileChooserFactory.createJFileChooser(lastDirectorySelected);

		fc.setDialogTitle("Select Graph Product");
		SimpleFileFilter tsp_ff = new SimpleFileFilter("tsp", "Time Series Product");
		fc.addChoosableFileFilter(tsp_ff);
		fc.setFileFilter(tsp_ff);
		fc.setDialogType(JFileChooser.OPEN_DIALOG);

		JGUIUtil.setWaitCursor(this, false);
		if (fc.showOpenDialog(this) != JFileChooser.APPROVE_OPTION)
		{
			return;
		}

		string currDir = (fc.getCurrentDirectory()).ToString();

		if (!currDir.Equals(lastDirectorySelected, StringComparison.OrdinalIgnoreCase))
		{
			JGUIUtil.setLastFileDialogDirectory(currDir);
		}
		string filename = fc.getSelectedFile().getPath();
		JGUIUtil.setWaitCursor(this, true);

		__dirty = false;

		// Clear the existing worksheet...

		__worksheet.clear();

		// Read the product from the file...

		int row; // Row to add
		TSIdent new_tsident; // New TSIdent to add to worksheet table model
		StateMod_Data smdata; // StateMod data matching the TSID.
		DataSetComponent comp = null;
		System.Collections.IList comp_data = null;
		string station_type = null; // Station type for row.
		int pos = 0;
		string loc_main; // Main part of the location in the TSID
		string res_account; // Reservoir account part of the location in the
					// TSID, null if not a reservoir account
		try
		{
			Message.printStatus(1, routine, "Loading graph from \"" + filename + "\"");
			TSProduct tsproduct = new TSProduct(filename, null);

			// Currently the only things that we expect to see in the file
			// are Product.GraphType and Data 1.N.TSID.

			string prop_val = tsproduct.getLayeredPropValue("GraphType", -1, -1);

			// Select in the interface...

			__ignoreItemStateChange = true;
			try
			{
				JGUIUtil.selectTokenMatches(__graphType_JComboBox, true, " -", 0, 0, prop_val, "Line");
			}
			catch (Exception)
			{
				__graphType_JComboBox.select("Line");
			}
			__ignoreItemStateChange = false;

			// Loop through time series identifiers and add a row...

			for (int i = 0; ; i++)
			{
				prop_val = tsproduct.getLayeredPropValue("TSID", 0, i);
				if (string.ReferenceEquals(prop_val, null))
				{
					// No more data...
					break;
				}

				row = __worksheet.getRowCount();

				new_tsident = new TSIdent(prop_val);
				// Determine the station type, assuming IDs are unique
				// in the data set.  If the identifier includes a dash,
				// it is assumed to be a reservoir account and only the
				// first part of the identifier is used to match the
				// stations in the data set.
				comp = __dataset.getComponentForComponentType(StateMod_DataSet.COMP_DIVERSION_STATIONS);
				comp_data = (System.Collections.IList)comp.getData();
				loc_main = new_tsident.getLocation();
				res_account = null;
				if (loc_main.IndexOf("-", StringComparison.Ordinal) > 0)
				{
					// Save the account before truncating...
					res_account = StringUtil.getToken(loc_main,"-",0,1);
					// Now truncate the location to only the main
					// part...
					loc_main = StringUtil.getToken(loc_main,"-",0,0);
				}
				pos = StateMod_Util.IndexOf(comp_data, loc_main);
				if (pos >= 0)
				{
					// TSID is for a diversion...
					station_type = StateMod_Util.STATION_TYPE_DIVERSION;
				}
				if (pos < 0)
				{
					// Check reservoirs...
					comp = __dataset.getComponentForComponentType(StateMod_DataSet.COMP_RESERVOIR_STATIONS);
					comp_data = (System.Collections.IList)comp.getData();
					pos = StateMod_Util.IndexOf(comp_data,loc_main);
					if (pos >= 0)
					{
						// TSID is for a reservoir...
						station_type = StateMod_Util.STATION_TYPE_RESERVOIR;
					}
				}
				if (pos < 0)
				{
					// Check instream flows...
					comp = __dataset.getComponentForComponentType(StateMod_DataSet.COMP_INSTREAM_STATIONS);
					comp_data = (System.Collections.IList)comp.getData();
					pos = StateMod_Util.IndexOf(comp_data,loc_main);
					if (pos >= 0)
					{
						// TSID is for an instream flow...
						station_type = StateMod_Util.STATION_TYPE_INSTREAM_FLOW;
					}
				}
				if (pos < 0)
				{
					// Check stream gage...
					comp = __dataset.getComponentForComponentType(StateMod_DataSet.COMP_STREAMGAGE_STATIONS);
					comp_data = (System.Collections.IList)comp.getData();
					pos = StateMod_Util.IndexOf(comp_data,loc_main);
					if (pos >= 0)
					{
						// TSID is for a stream gage...
						station_type = StateMod_Util.STATION_TYPE_STREAMGAGE;
					}
				}
				if (pos < 0)
				{
					// Check stream estimate...
					comp = __dataset.getComponentForComponentType(StateMod_DataSet.COMP_STREAMESTIMATE_STATIONS);
					comp_data = (System.Collections.IList)comp.getData();
					pos = StateMod_Util.IndexOf(comp_data,loc_main);
					if (pos >= 0)
					{
						// TSID is for a stream estimate...
						station_type = StateMod_Util.STATION_TYPE_STREAMESTIMATE;
					}
				}
				if (pos < 0)
				{
					// Check well...
					comp = __dataset.getComponentForComponentType(StateMod_DataSet.COMP_WELL_STATIONS);
					comp_data = (System.Collections.IList)comp.getData();
					pos = StateMod_Util.IndexOf(comp_data,loc_main);
					if (pos >= 0)
					{
						// TSID is for a well...
						station_type = StateMod_Util.STATION_TYPE_WELL;
					}
				}
				if (pos < 0)
				{
					Message.printWarning(1, routine, "Unable to determine station type for \"" + prop_val + "\" uniquetempvar.  Not loading.");
					__dirty = true; // Because something lost
					continue;
				}
				// Set the TSID alias to the station type.  This is a
				// "trick" to carry around the station type while it is
				// in memory...
				new_tsident.setAlias(station_type);
				// Replace the location with the ID - name combination
				// for the matched station.
				smdata = (StateMod_Data)comp_data[pos];
				if (!string.ReferenceEquals(res_account, null))
				{
					// Need to handle the account in the ID and
					// also the name...
					new_tsident.setLocation(StateMod_Util.formatDataLabel(smdata.getID() + "-" + res_account, smdata.getName() + " - " + ((StateMod_Reservoir)smdata).getAccount(StringUtil.atoi(res_account) - 1).getName()));
				}
				else
				{ // All other data with a "simple" ID - Name
					// choice...
					new_tsident.setLocation(StateMod_Util.formatDataLabel(smdata.getID(), smdata.getName()));
				}
				// Replace the data type with DataType - Input|Output
				// to match the data type JComboBox.  "StateModB" time
				// series will be considered input and "StateMod" as
				// input.  Also reset the input type to exactly match
				// what is in the list so there is no
				// uppercase/lowercase problem later.
				if (new_tsident.getInputType().equalsIgnoreCase("StateModB"))
				{
					new_tsident.setType(new_tsident.getType() + " - Output");
					new_tsident.setInputType("StateModB");
				}
				else if (new_tsident.getInputType().equalsIgnoreCase("StateMod"))
				{
					new_tsident.setType(new_tsident.getType() + " - Input");
					new_tsident.setInputType("StateMod");
				}
				else
				{
					Message.printWarning(1, routine, "Input type for \"" + prop_val + "\".  Not recognized in this tool.  Skipping.");
					__dirty = true; // Because something lost
					continue;
				}
				// Make sure the interval matches a recognized value.
				// Reset to the exact string so there is not a problem
				// with uppercase/lowercase.
				if (new_tsident.getInterval().equalsIgnoreCase("Month"))
				{
					new_tsident.setInterval("Month");
				}
				else if (new_tsident.getInterval().equalsIgnoreCase("Day"))
				{
					new_tsident.setInterval("Day");
				}
				else
				{
					Message.printWarning(1, routine, "Interval in \"" + prop_val + "\" is not recognized.  Not loading.");
					__dirty = true; // Because something lost
					continue;
				}
				__worksheet.addRow(new TSIdent());
				__tableModel.fillIDColumn(row, new_tsident.getAlias());
				__tableModel.fillDataTypeColumn(row, false, new_tsident.getAlias(), new_tsident.getLocation(), new_tsident.getInterval());
				__tableModel.setValueAt(new_tsident.getAlias(), row, __tableModel._COL_STATION_TYPE);
				__tableModel.setValueAt(new_tsident.getLocation(), row, __tableModel._COL_ID);
				__tableModel.setValueAt(new_tsident.getInterval(), row, __tableModel._COL_INTERVAL);
				__tableModel.setValueAt(new_tsident.getType(), row, __tableModel._COL_DATA_TYPE);
				__tableModel.setValueAt(new_tsident.getInputType(), row, __tableModel._COL_INPUT_TYPE);
				__tableModel.setValueAt(new_tsident.getInputName(), row, __tableModel._COL_INPUT_NAME);

				// REVISIT - SAM not sure what the order of calls
				// should be?  Does everything cascade each time a cell
				// value is set?
				//
				// Now add a row to the worksheet.  The behaviour should
				// be as follows:
				//
				// 1)	The station types list should be the standard
				//	JComboBox and and select "station_type" as
				//	as determined above.
				// 2)	This should trigger a refresh of the ID - Name
				//	list for the station type.  Select using
				//	new_tsident.getLocation(). 
				// 3)	Select the interval using
				//	new_tsident.getInterval().
				// 4)	Display the data types based on 1-3 and select
				//	new_tsident.getType().
				// 5)	Display the input type based on 1-4 and select
				//	new_tsident.getInputType().
				// 6)	Display the input name based on 1-5 and select
				//	from the list or add a new file name.
	/*			
				__worksheet.addRow(new_tsident);
				__tableModel.fillIDColumn( row, new_tsident.getAlias());
				__tableModel.fillDataTypeColumn(row,
					new_tsident.getAlias(),
					new_tsident.getLocation(), 
					new_tsident.getInterval() ); 
				// REVISIT - what else?
				__worksheet.setCellEditable(
					row, __tableModel._COL_ID, false);
				__worksheet.setCellEditable(
					row, __tableModel._COL_DATA_TYPE, false);
	*/				
			}
		}
		catch (Exception e)
		{
			Message.printWarning(1, routine, "Error loading graph product file " + "\"" + filename + "\"", this);
				Message.printWarning(2, routine, e);
		}

		JGUIUtil.setWaitCursor(this, false);
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void mouseClicked(MouseEvent @event)
	{
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void mouseEntered(MouseEvent @event)
	{
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void mouseExited(MouseEvent @event)
	{
	}

	/// <summary>
	/// Calls checkGUIState().
	/// </summary>
	public virtual void mousePressed(MouseEvent @event)
	{
		checkGUIState();
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void mouseReleased(MouseEvent @event)
	{
	}

	/// <summary>
	/// Method needed for TSSupplier interface, to supply the time series for the graph.
	/// This method is called from the TSProcessor when creating graphs and other products.
	/// </summary>
	public virtual TS readTimeSeries(string tsident, DateTime req_date1, DateTime req_date2, string req_units, bool read_data)
	{
		TS ts = null;
		return ts;
	}

	/// <summary>
	/// Method needed for TSSupplier interface - not used.
	/// </summary>
	public virtual TS readTimeSeries(TS req_ts, string fname, DateTime req_date1, DateTime req_date2, string req_units, bool read_data)
	{
		return null;
	}

	/// <summary>
	/// Method needed for TSSupplier interface, to supply the time series for the graph.
	/// This method is not used.
	/// </summary>
	public virtual System.Collections.IList readTimeSeriesList(string tsident, DateTime req_date1, DateTime req_date2, string req_units, bool read_data)
	{
		return null;
	}

	/// <summary>
	/// Method needed for TSSupplier interface - not used.
	/// </summary>
	public virtual System.Collections.IList readTimeSeriesList(TSIdent tsident, string fname, DateTime req_date1, DateTime req_date2, string req_units, bool read_data)
	{
		return null;
	}

	/// <summary>
	/// Save the current worksheet contents as a TSProduct.  Currently only the graph
	/// type and list of time series identifiers is saved.
	/// </summary>
	private void saveTSProduct()
	{
		string routine = "StateMod_GraphingTool_JFrame.saveTSProduct";
		string lastDirectorySelected = JGUIUtil.getLastFileDialogDirectory();
		JFileChooser fc = JFileChooserFactory.createJFileChooser(lastDirectorySelected);
		fc.setDialogTitle("Select Graph");
		SimpleFileFilter tsp_ff = new SimpleFileFilter("tsp", "Time Series Product");
		fc.addChoosableFileFilter(tsp_ff);
		fc.setFileFilter(tsp_ff);
		fc.setDialogType(JFileChooser.SAVE_DIALOG);

		int retVal = fc.showSaveDialog(this);
		if (retVal != JFileChooser.APPROVE_OPTION)
		{
			return;
		}

		string currDir = (fc.getCurrentDirectory()).ToString();

		if (!currDir.Equals(lastDirectorySelected, StringComparison.OrdinalIgnoreCase))
		{
			JGUIUtil.setLastFileDialogDirectory(currDir);
		}
		string filename = fc.getSelectedFile().getPath();

		filename = IOUtil.enforceFileExtension(filename, "tsp");

		__dirty = false;

		// REVISIT - need to decide how intelligent this should be - for now
		// only save the graph type and TSIDs.
		// Create a TSProduct and save...

		PropList props = new PropList("tsp");

		// Set the graph type from the combobox...
		int sub = 1;
		int its = 0;
		props.set("Product.GraphType=" + __graphType_JComboBox.getSelected());
		System.Collections.IList tsident_Vector = __worksheet.getAllData();
		int nrows = tsident_Vector.Count;
		TSIdent tsident = null;
		string id; // Location for reset.
		string data_type = null; // Data type for reset.
		for (int i = 0; i < nrows; i++)
		{
			try
			{
				tsident = new TSIdent((TSIdent)tsident_Vector[i]);
			}
			catch (Exception)
			{
				// Should not happen.
				continue;
			}
			// Reset the ID to the first token because the ID from the
			// display may contain the name ID (name)...
			id = StringUtil.getToken(tsident.getLocation(), " (", 0, 0);
			tsident.setLocation(id);
			// Reset the data type to the first token because the data type
			// from the display contains " - Input" and " - Output"...
			data_type = StringUtil.getToken(tsident.getType(), " ", 0, 0);
			tsident.setType(data_type);
			// Save with the input name...
			props.set("Data " + sub + "." + (++its) + ".TSID=" + tsident.ToString(true));
		}

		// Write the file, with all properties that were set here...

		try
		{
			TSProduct tsp = new TSProduct(props, null);
			Message.printStatus(1, "", "Writing TSProduct \"" + filename + "\"");
			tsp.writeFile(filename, true);
		}
		catch (Exception)
		{
			Message.printWarning(1, routine, "Error writing time series product \"" + filename + "\"", this);
		}
	}

	/// <summary>
	/// Set the messages that are visible in the bottom of the window. </summary>
	/// <param name="message"> General message string. </param>
	/// <param name="status"> Status string(e.g., "Ready", "Wait". </param>
	private void setMessages(string message, string status)
	{
		if (!string.ReferenceEquals(message, null))
		{
			__messageTextField.setText(message);
		}
		if (!string.ReferenceEquals(status, null))
		{
			__statusTextField.setText(status);
		}
	}

	/// <summary>
	/// Sets up the GUI.
	/// </summary>
	private void setupGUI()
	{
		string routine = "StateMod_GraphingTool_JFrame.setupGUI";

		addWindowListener(this);

		__addRow_JButton = new SimpleJButton(__BUTTON_ADD_ROW, this);
		__addRow_JButton.setToolTipText("<html>Add a row for a time series.<br>" + "The default is to copy the previous row (see checkbox below).</html>");
		__deleteRow_JButton = new SimpleJButton(__BUTTON_DELETE_ROW, this);
		__deleteRow_JButton.setToolTipText("<html>Delete the selected rows from below.</html>");
		__deleteAll_JButton = new SimpleJButton(__BUTTON_CLEAR_TEMPLATE,this);
		__deleteAll_JButton.setToolTipText("<html>Delete all rows.</html>");
		__save_JButton = new SimpleJButton(__BUTTON_SAVE_TEMPLATE, this);
		__save_JButton.setToolTipText("<html>Save the graph as a time series product.<br>" + "The graph can be reloaded later.</html>");
		__load_JButton = new SimpleJButton(__BUTTON_LOAD_TEMPLATE, this);
		__load_JButton.setToolTipText("<html>Load an existing time series graph.</hbml>");

		// REVISIT - enable when a better help system is implemented
		//__help_JButton = new SimpleJButton(__BUTTON_HELP, this);
		__getTS_JButton = new SimpleJButton(__BUTTON_GET_TIME_SERIES, this);
		__getTS_JButton.setToolTipText("<html>Get time series data from memory, input, and output files.</html>");
		__graph_JButton = new SimpleJButton(__BUTTON_GRAPH, this);
		__graph_JButton.setToolTipText("<html>Graph available time series using the specified graph type.</html>");
		__graph_JButton.setEnabled(false);
		__close_JButton = new SimpleJButton(__BUTTON_CLOSE, this);
		__close_JButton.setToolTipText("<html>Close this window, prompting for save if necessary.</html>");
		__export_JButton = new SimpleJButton(__BUTTON_EXPORT_DATA, this);
		__export_JButton.setToolTipText("<html>Save the time series data to a file.</html>");
		__export_JButton.setEnabled(false);

		__autoLineCopyJCheckBox = new JCheckBox("Automatically fill " + "initial line content on \"Add a Row\".", true);
		__autoLineCopyJCheckBox.setToolTipText("<html>Causes new rows to be a copy of the previous row.</html>");

		__graphType_JComboBox = new SimpleJComboBox();
		__graphType_JComboBox.setToolTipText("<html>The graph type indicates how to format in-memory time series.<BR>" + "Multiple graphs can be displayed from the same time series.</html>");
		foreach (TSGraphType graphType in TSGraphType.values())
		{
			if ((graphType == TSGraphType.DOUBLE_MASS) || (graphType == TSGraphType.UNKNOWN))
			{
				continue;
			}
			__graphType_JComboBox.add("" + graphType);
		}
		__graphType_JComboBox.select("Line");
		__graphType_JComboBox.addItemListener(this);

		// Make a main panel to be the resizable body of the frame...

		JPanel mainJPanel = new JPanel();
		GridBagLayout gb = new GridBagLayout();
		mainJPanel.setLayout(gb);

		GridLayout gl = new GridLayout(2, 2, 2, 2);
		JPanel topJPanel = new JPanel();
		topJPanel.setLayout(gl);

		GridLayout gl2 = new GridLayout(1, 0, 2, 0);
		gl2 = new GridLayout(2, 0, 2, 0);

		JPanel bottomJPanel = new JPanel();
		bottomJPanel.setLayout(gl2);

		FlowLayout fl = new FlowLayout(FlowLayout.CENTER);
		JPanel final_buttonJPanel = new JPanel();
		final_buttonJPanel.setLayout(fl);

		JPanel gridJPanel = new JPanel();
		gridJPanel.setLayout(gb);

		// Buttons at the top to edit the worksheet.

		topJPanel.add(__addRow_JButton);
		topJPanel.add(__deleteRow_JButton);
		topJPanel.add(__deleteAll_JButton);
		topJPanel.add(__load_JButton);
		int y = 0;
		JGUIUtil.addComponent(mainJPanel, topJPanel, 0, y, 10, 3, 0, 0, 10, 10, 10, 10, GridBagConstraints.NONE, GridBagConstraints.NORTH);

		y += 3;
		// Add grid...
		JGUIUtil.addComponent(mainJPanel, __autoLineCopyJCheckBox, 0, ++y, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);

		PropList p = new PropList("StateMod_GraphingTool_JFrame.JWorksheet");
		p.add("JWorksheet.ShowRowHeader=true");
		p.add("JWorksheet.AllowCopy=true");
		p.add("JWorksheet.RowColumnBackground=LightGray");
		p.add("JWorksheet.ShowPopupMenu=true");

		int[] widths = null;
		JScrollWorksheet jsw = null;
		try
		{
			__tableModel = new StateMod_GraphingTool_TableModel(this, __dataset, new List<object>());

			StateMod_GraphingTool_CellRenderer crg = new StateMod_GraphingTool_CellRenderer(__tableModel);

			jsw = new JScrollWorksheet(crg, __tableModel, p);
			__worksheet = jsw.getJWorksheet();

			IList<string> v = StateMod_Util.getStationTypes();
			__worksheet.setColumnJComboBoxValues(__tableModel._COL_STATION_TYPE, v);

			// Initialize a combo box for columns that use fixed-data.

			__worksheet.setCellSpecificJComboBoxColumn(__tableModel._COL_ID, false);
			__worksheet.setCellSpecificJComboBoxColumn(__tableModel._COL_INTERVAL, false);
			__worksheet.setCellSpecificJComboBoxColumn(__tableModel._COL_DATA_TYPE, false);
			__worksheet.setCellSpecificJComboBoxColumn(__tableModel._COL_INPUT_TYPE, false);
			__worksheet.setCellSpecificJComboBoxColumn(__tableModel._COL_INPUT_NAME, true);

			__tableModel.setWorksheet(__worksheet);

			widths = crg.getColumnWidths();
		}
		catch (Exception e)
		{
			Message.printWarning(1, routine, "Error building worksheet.");
			Message.printWarning(2, routine, e);
			jsw = new JScrollWorksheet(0, 0, p);
			__worksheet = jsw.getJWorksheet();
		}
		__worksheet.setPreferredScrollableViewportSize(null);
		__worksheet.setHourglassJFrame(this);
		__worksheet.addMouseListener(this);

		JGUIUtil.addComponent(gridJPanel, jsw, 0, 0, 1, 1, 1, 1, GridBagConstraints.BOTH, GridBagConstraints.CENTER);
		JGUIUtil.addComponent(mainJPanel, gridJPanel, 0, ++y, 10, 12, 1, 1, GridBagConstraints.BOTH, GridBagConstraints.CENTER);

		y += 11; // To account for grid height

		// Add bottom buttons
		JPanel graph_controlJPanel = new JPanel();
		graph_controlJPanel.setLayout(fl);
		graph_controlJPanel.add(__getTS_JButton);
		graph_controlJPanel.add(__graph_JButton);
		__graph_JButton.setEnabled(false);
		graph_controlJPanel.add(__graphType_JComboBox);
		graph_controlJPanel.add(__export_JButton);
		bottomJPanel.add(graph_controlJPanel);

		//final_buttonJPanel.add(__help_JButton);
		//__help_JButton.setEnabled(false);
		final_buttonJPanel.add(__save_JButton);
		final_buttonJPanel.add(__close_JButton);

		// Add the final buttons on the bottom to the bottom panel...
		bottomJPanel.add(final_buttonJPanel);
		// Add the button panel to the frame...
		JGUIUtil.addComponent(mainJPanel, bottomJPanel, 0, ++y, 10, 1, 0, 0, GridBagConstraints.VERTICAL, GridBagConstraints.SOUTH);

		// Add the main panel as the resizable content...
		getContentPane().add("Center", mainJPanel);

		// Add JTextFields for messages...
		JPanel messageJPanel = new JPanel();
		messageJPanel.setLayout(gb);
		__messageTextField = new JTextField();
		__messageTextField.setEditable(false);
		__statusTextField = new JTextField("", 5);
		__statusTextField.setEditable(false);
		JGUIUtil.addComponent(messageJPanel, __messageTextField, 0, 0, 9, 1, 1, 0, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);
		JGUIUtil.addComponent(messageJPanel, __statusTextField, 9, 0, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.SOUTH);
		getContentPane().add("South", messageJPanel);

		if (__dataset_wm != null)
		{
			__dataset_wm.setWindowOpen(StateMod_DataSet_WindowManager.WINDOW_GRAPHING_TOOL, this);
		}

		pack();
		setSize(900, 600); // Allows for lists.
		setMessages("Add time series to graph", "Ready");
		JGUIUtil.center(this);
		setVisible(true);

		if (widths != null)
		{
			__worksheet.setColumnWidths(widths);
		}
		checkGUIState();
	}

	/// <summary>
	/// Responds to Window Activated events; does nothing. </summary>
	/// <param name="e"> the WindowEvent that happened. </param>
	public virtual void windowActivated(WindowEvent e)
	{
	}
	/// <summary>
	/// Responds to Window closed events; does nothing. </summary>
	/// <param name="e"> the WindowEvent that happened. </param>
	public virtual void windowClosed(WindowEvent e)
	{
	}
	/// <summary>
	/// Responds to Window closing events; closes the window and marks it closed
	/// in StateMod_GUIUtil. </summary>
	/// <param name="e"> the WindowEvent that happened. </param>
	public virtual void windowClosing(WindowEvent e)
	{
		closeWindow();
	}
	/// <summary>
	/// Responds to Window deactivated events; saves the current information. </summary>
	/// <param name="e"> the WindowEvent that happened. </param>
	public virtual void windowDeactivated(WindowEvent e)
	{
	}
	/// <summary>
	/// Responds to Window deiconified events; does nothing. </summary>
	/// <param name="e"> the WindowEvent that happened. </param>
	public virtual void windowDeiconified(WindowEvent e)
	{
	}
	/// <summary>
	/// Responds to Window iconified events; saves the current information. </summary>
	/// <param name="e"> the WindowEvent that happened. </param>
	public virtual void windowIconified(WindowEvent e)
	{
	}
	/// <summary>
	/// Responds to Window opened events; does nothing. </summary>
	/// <param name="e"> the WindowEvent that happened. </param>
	public virtual void windowOpened(WindowEvent e)
	{
	}
	/// <summary>
	/// Responds to Window opening events; does nothing. </summary>
	/// <param name="e"> the WindowEvent that happened. </param>
	public virtual void windowOpening(WindowEvent e)
	{
	}

	}

}