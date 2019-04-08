using System;
using System.Collections.Generic;

// StateMod_StreamEstimate_JFrame - JFrame to edit the stream estimate stations

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
// StateMod_StreamEstimate_JFrame - JFrame to edit the stream estimate stations
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
// 
// 02 Jan 1998	Catherine E.		Created initial version of class.
//		Nutting-Lane, RTi
// 17 Feb 1998	CEN, RTi		Move help/close to bottom.
// 31 Aug 1998	CEN, RTi		added check for areTSRead.
// 22 Sep 1998	CEN, RTi		Changed list to multilist.
// 08 Mar 2000	CEN, RTi		Added radio buttons to search.
// 01 Apr 2001	Steven A. Malers, RTi	Change GUI to JGUIUtil.  Add finalize().
//					Remove import *.
// 04 May 2001	SAM, RTi		Change "Baseflow Results" to
//					"Baseflow Time Series" and change the
//					MonthTS title to be more friendly.
// 2002-09-12	SAM, RTi		Move baseflow time series to the .ris
//					(River Stations)window.  Change the
//					title to say Baseflow Coefficients.
//------------------------------------------------------------------------------
// 2003-08-20	J. Thomas Sapienza, RTi	Initial swing version adapted from 
//					the old baseflow coefficients window.
// 2003-08-26	SAM, RTi		Enable StateMod_DataSet_WindowManager.
// 2003-08-27	JTS, RTi		Added selectID() to select an ID 
//					on the worksheet from outside the GUI.
// 2003-09-02	JTS, RTi		* Added a border around the search area
//					  components.
//					* TS Graph/Summary/Table selection is
//					  now done through check boxes.
// 2003-09-04	JTS, RTi		Added cancel and apply buttons.
// 2003-09-04	SAM, RTi		Enable graph features.
// 2003-09-05	JTS, RTi		Class is now an item listener in 
//					order to enable/disable graph buttons
//					based on selected checkboxes.
// 2003-09-06	SAM, RTi		Fix problem with graph sizing.
// 2003-09-08	JTS, RTi		* Added checkTimeSeriesButtonsStates()
//					  to enable/disable the time series
//					  display buttons according to how
//					  the time series checkboxes are 
//					  selected.
//					* Adjusted layout.
// 2003-09-11	SAM, RTi		* Rename the class from
//					  StateMod_BaseFlowStation_JFrame to
//					  StateMod_StreamEstimate_JFrame and
//					  make changes accordingly.
//					* Remove findBaseflowCoefficients()
//					  since StateMod_Util.indexOf() will
//					  work.
// 2003-09-23	JTS, RTi		Uses new StateMod_GUIUtil code for
//					setting titles.
// 2004-01-22	JTS, RTi		Updated to use JScrollWorksheet and
//					the new row headers.
// 2004-07-15	JTS, RTi		* For data changes, enabled the
//					  Apply and Cancel buttons through new
//					  methods in the data classes.
//					* Changed layout of buttons to be
//					  aligned in the lower-right.
// 					* windowDeactivated() no longer saves
//					  data as it was causing problems with
//					  the cancel code.
// 2006-01-19	JTS, RTi		* Now implements JWorksheet_SortListener
//					* Reselects the record that was selected
//					  when the worksheet is sorted.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
//------------------------------------------------------------------------------

namespace DWR.StateMod
{


	using HydrologyNode = cdss.domain.hydrology.network.HydrologyNode;

	using GeoRecord = RTi.GIS.GeoView.GeoRecord;
	using GRLimits = RTi.GR.GRLimits;
	using GRShape = RTi.GR.GRShape;
	using TSProduct = RTi.GRTS.TSProduct;
	using TSViewJFrame = RTi.GRTS.TSViewJFrame;
	using TS = RTi.TS.TS;
	using JGUIUtil = RTi.Util.GUI.JGUIUtil;
	using JScrollWorksheet = RTi.Util.GUI.JScrollWorksheet;
	using JWorksheet = RTi.Util.GUI.JWorksheet;
	using JWorksheet_SortListener = RTi.Util.GUI.JWorksheet_SortListener;
	using ResponseJDialog = RTi.Util.GUI.ResponseJDialog;
	using SimpleJButton = RTi.Util.GUI.SimpleJButton;
	using DataSetComponent = RTi.Util.IO.DataSetComponent;
	using PropList = RTi.Util.IO.PropList;
	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// This class is a GUI for displaying stream estimate station data and the
	/// associated coefficient data.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class StateMod_StreamEstimate_JFrame extends javax.swing.JFrame implements java.awt.event.ActionListener, java.awt.event.ItemListener, java.awt.event.KeyListener, java.awt.event.MouseListener, java.awt.event.WindowListener, RTi.Util.GUI.JWorksheet_SortListener
	public class StateMod_StreamEstimate_JFrame : JFrame, ActionListener, ItemListener, KeyListener, MouseListener, WindowListener, JWorksheet_SortListener
	{

	/// <summary>
	/// Whether the data on the GUI is editable or not.
	/// </summary>
	private bool __editable = true;

	/// <summary>
	/// Button group for the search radio buttons.
	/// </summary>
	private ButtonGroup __searchCriteriaGroup;

	/// <summary>
	/// The data set component containing base flow coefficient data.
	/// </summary>
	private DataSetComponent __coefficientsComp;

	/// <summary>
	/// The data set component containing base flow station data.
	/// </summary>
	private DataSetComponent __stationsComp;

	/// <summary>
	/// The index in __disables[] of textfields that should NEVER be made
	/// editable (e.g., ID fields).
	/// </summary>
	private int[] __textUneditables;

	/// <summary>
	/// Indices of the currently- and last-selected station.
	/// </summary>
	private int __currentStationIndex = -1, __lastStationIndex = -1;

	/// <summary>
	/// Stores the index of the record that was selected before the worksheet is sorted,
	/// in order to reselect it after the sort is complete.
	/// </summary>
	private int __sortSelectedRow = -1;

	/// <summary>
	/// GUI buttons.
	/// </summary>
	private JButton __applyJButton, __cancelJButton, __findNextButton, __helpJButton, __closeJButton, __graph_JButton, __table_JButton, __summary_JButton, __showOnMap_JButton = null, __showOnNetwork_JButton = null;

	private JCheckBox __ts_streamflow_base_monthly_JCheckBox, __ts_streamflow_base_daily_JCheckBox;

	/// <summary>
	/// Array of JComponents that should be disabled when nothing is selected from the list.
	/// </summary>
	private JComponent[] __disables;

	/// <summary>
	/// Search radio buttons.
	/// </summary>
	private JRadioButton __searchIDJRadioButton, __searchNameJRadioButton;

	/// <summary>
	/// GUI text fields.
	/// </summary>
	private JTextField __nameJTextField, __idJTextField, __prorationFactorJTextField, __searchIDJTextField, __searchNameJTextField;

	/// <summary>
	/// Worksheets for displaying data.  __worksheetR is on the right and displays
	/// stream estimate coefficients.  __worksheetL is on the left and displays station 
	/// ids and names.
	/// </summary>
	private JWorksheet __worksheetL, __worksheetR;

	/// <summary>
	/// The data set containing the data to display.
	/// </summary>
	private StateMod_DataSet __dataset;

	/// <summary>
	/// Data set window manager.
	/// </summary>
	private StateMod_DataSet_WindowManager __dataset_wm;

	/// <summary>
	/// GUI strings.
	/// </summary>
	private readonly string __BUTTON_SHOW_ON_MAP = "Show on Map", __BUTTON_SHOW_ON_NETWORK = "Show on Network", __BUTTON_APPLY = "Apply", __BUTTON_CANCEL = "Cancel", __BUTTON_CLOSE = "Close", __BUTTON_FIND_NEXT = "Find Next", __BUTTON_HELP = "Help";

	/// <summary>
	/// The table model for displaying data in the right worksheet.
	/// </summary>
	private StateMod_StreamEstimate_Coefficients_TableModel __tableModelR;

	/// <summary>
	/// The stream estimate stations data.
	/// </summary>
	private IList<StateMod_StreamEstimate> __stationsVector;

	/// <summary>
	/// The coefficients data.
	/// </summary>
	private IList<StateMod_StreamEstimate_Coefficients> __coefficientsVector;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="dataset"> the dataset containing the stream estimate stations data. </param>
	/// <param name="dataset_wm"> the dataset window manager or null if the data set windows are not being managed. </param>
	/// <param name="editable"> whether the data on the gui is editable or not </param>
	public StateMod_StreamEstimate_JFrame(StateMod_DataSet dataset, StateMod_DataSet_WindowManager dataset_wm, bool editable)
	{
		JGUIUtil.setIcon(this, JGUIUtil.getIconImage());
		StateMod_GUIUtil.setTitle(this, dataset, "Stream Estimate Stations", null);

		__dataset = dataset;
		__dataset_wm = dataset_wm;
		__stationsComp = __dataset.getComponentForComponentType(StateMod_DataSet.COMP_STREAMESTIMATE_STATIONS);
		__stationsVector = (IList<StateMod_StreamEstimate>)__stationsComp.getData();
		int size = __stationsVector.Count;
		StateMod_StreamEstimate s = null;
		for (int i = 0; i < size; i++)
		{
			s = (StateMod_StreamEstimate)__stationsVector[i];
			s.createBackup();
		}

		__coefficientsComp = __dataset.getComponentForComponentType(StateMod_DataSet.COMP_STREAMESTIMATE_COEFFICIENTS);
		__coefficientsVector = (IList<StateMod_StreamEstimate_Coefficients>)__coefficientsComp.getData();
		size = __coefficientsVector.Count;
		StateMod_StreamEstimate_Coefficients c = null;
		for (int i = 0; i < size; i++)
		{
			c = (StateMod_StreamEstimate_Coefficients)__coefficientsVector[i];
			c.createBackup();
		}

		__editable = editable;

		setupGUI(0);
	}

	/// <summary>
	/// Constructor. </summary>
	/// <param name="dataset"> the dataset containing the baseflow data. </param>
	/// <param name="dataset_wm"> the dataset window manager or null if the data set windows
	/// are not being managed. </param>
	/// <param name="station"> the station to preselect on the GUI. </param>
	/// <param name="editable"> whether the data on the gui is editable or not </param>
	public StateMod_StreamEstimate_JFrame(StateMod_DataSet dataset, StateMod_DataSet_WindowManager dataset_wm, StateMod_StreamEstimate station, bool editable)
	{
		JGUIUtil.setIcon(this, JGUIUtil.getIconImage());
		StateMod_GUIUtil.setTitle(this, dataset, "Stream Estimate Stations", null);

		__dataset = dataset;
		__dataset_wm = dataset_wm;
		__stationsComp = __dataset.getComponentForComponentType(StateMod_DataSet.COMP_STREAMESTIMATE_STATIONS);
		__stationsVector = (IList<StateMod_StreamEstimate>)__stationsComp.getData();
		int size = __stationsVector.Count;
		StateMod_StreamEstimate s = null;
		for (int i = 0; i < size; i++)
		{
			s = (StateMod_StreamEstimate)__stationsVector[i];
			s.createBackup();
		}

		__coefficientsComp = __dataset.getComponentForComponentType(StateMod_DataSet.COMP_STREAMESTIMATE_COEFFICIENTS);
		__coefficientsVector = (IList<StateMod_StreamEstimate_Coefficients>)__coefficientsComp.getData();
		size = __coefficientsVector.Count;
		StateMod_StreamEstimate_Coefficients c = null;
		for (int i = 0; i < size; i++)
		{
			c = (StateMod_StreamEstimate_Coefficients)__coefficientsVector[i];
			c.createBackup();
		}

		string id = station.getID();
		int index = StateMod_Util.IndexOf(__stationsVector, id);

		__editable = editable;

		setupGUI(index);
	}

	/// <summary>
	/// Responds to action performed events. </summary>
	/// <param name="e"> the ActionEvent that happened. </param>
	public virtual void actionPerformed(ActionEvent e)
	{
		string action = e.getActionCommand();
		object source = e.getSource();

		if (source == __searchIDJRadioButton)
		{
			__searchIDJTextField.setEditable(true);
			__searchNameJTextField.setEditable(false);
		}
		else if (source == __searchNameJRadioButton)
		{
			__searchIDJTextField.setEditable(false);
			__searchNameJTextField.setEditable(true);
		}
		else if (action.Equals(__BUTTON_APPLY))
		{
			saveCurrentRecord();
			int size = __stationsVector.Count;
			StateMod_StreamEstimate s = null;
			bool changed = false;
			for (int i = 0; i < size; i++)
			{
				s = __stationsVector[i];
				if (!changed && s.changed())
				{
					changed = true;
				}
				s.createBackup();
			}
			if (changed)
			{
				__dataset.setDirty(StateMod_DataSet.COMP_STREAMESTIMATE_STATIONS,true);
			}
			size = __coefficientsVector.Count;
			StateMod_StreamEstimate_Coefficients c = null;
			changed = false;
			for (int i = 0; i < size; i++)
			{
				c = __coefficientsVector[i];
				if (!changed && c.changed())
				{
					changed = true;
				}
				c.createBackup();
			}
			if (changed)
			{
				__dataset.setDirty(StateMod_DataSet.COMP_STREAMESTIMATE_COEFFICIENTS,true);
			}
		}
		else if (action.Equals(__BUTTON_CANCEL))
		{
			int size = __stationsVector.Count;
			StateMod_StreamEstimate s = null;
			for (int i = 0; i < size; i++)
			{
				s = __stationsVector[i];
				s.restoreOriginal();
			}
			size = __coefficientsVector.Count;
			StateMod_StreamEstimate_Coefficients c = null;
			for (int i = 0; i < size; i++)
			{
				c = __coefficientsVector[i];
				c.restoreOriginal();
			}
			if (__dataset_wm != null)
			{
				__dataset_wm.closeWindow(StateMod_DataSet_WindowManager.WINDOW_STREAMESTIMATE);
			}
			else
			{
				JGUIUtil.close(this);
			}
		}
		else if (action.Equals(__BUTTON_CLOSE))
		{
			saveCurrentRecord();
			int size = __stationsVector.Count;
			StateMod_StreamEstimate s = null;
			bool changed = false;
			for (int i = 0; i < size; i++)
			{
				s = __stationsVector[i];
				if (!changed && s.changed())
				{
					changed = true;
				}
				s.acceptChanges();
			}
			if (changed)
			{
				__dataset.setDirty(StateMod_DataSet.COMP_STREAMESTIMATE_STATIONS,true);
			}
			size = __coefficientsVector.Count;
			StateMod_StreamEstimate_Coefficients c = null;
			changed = false;
			for (int i = 0; i < size; i++)
			{
				c = __coefficientsVector[i];
				if (!changed && c.changed())
				{
					changed = true;
				}
				c.acceptChanges();
			}
			if (changed)
			{
				__dataset.setDirty(StateMod_DataSet.COMP_STREAMESTIMATE_COEFFICIENTS,true);
			}
			if (__dataset_wm != null)
			{
				__dataset_wm.closeWindow(StateMod_DataSet_WindowManager.WINDOW_STREAMESTIMATE);
			}
			else
			{
				JGUIUtil.close(this);
			}
		}
		else if (action.Equals(__BUTTON_HELP))
		{
			// TODO Enable HELP (JTS - 2003-08-20)
		}
		else if (action.Equals(__BUTTON_FIND_NEXT))
		{
			searchLWorksheet(__worksheetL.getSelectedRow() + 1);
		}
		else if ((source == __searchIDJTextField) || (source == __searchNameJTextField))
		{
			searchLWorksheet(0);
		}
		else if (source == __showOnMap_JButton)
		{
			GeoRecord geoRecord = getSelectedStreamEstimate().getGeoRecord();
			GRShape shape = geoRecord.getShape();
			__dataset_wm.showOnMap(getSelectedStreamEstimate(), "StreamEst: " + getSelectedStreamEstimate().getID() + " - " + getSelectedStreamEstimate().getName(), new GRLimits(shape.xmin, shape.ymin, shape.xmax, shape.ymax), geoRecord.getLayer().getProjection());
		}
		else if (source == __showOnNetwork_JButton)
		{
			StateMod_Network_JFrame networkEditor = __dataset_wm.getNetworkEditor();
			if (networkEditor != null)
			{
				HydrologyNode node = networkEditor.getNetworkJComponent().findNode(getSelectedStreamEstimate().getID(), false, false);
				if (node != null)
				{
					__dataset_wm.showOnNetwork(getSelectedStreamEstimate(), "StreamEst: " + getSelectedStreamEstimate().getID() + " - " + getSelectedStreamEstimate().getName(), new GRLimits(node.getX(),node.getY(),node.getX(),node.getY()));
				}
			}
		}
		else if ((source == __graph_JButton) || (source == __table_JButton) || (source == __summary_JButton))
		{
			displayTSViewJFrame(source);
		}
	}

	/// <summary>
	/// Checks the text fields for validity before they are saved back into the
	/// data object. </summary>
	/// <returns> true if the text fields are okay, false if not. </returns>
	private bool checkInput()
	{
		IList<string> errors = new List<string>();
		int errorCount = 0;

		// for each field, check if it contains valid input.  If not,
		// create a string of the format "fieldname -- reason why it
		// is not correct" and add it to the errors vector.  also
		// increment error count

		if (errorCount == 0)
		{
			return true;
		}

		string plural = " was ";
		if (errorCount > 1)
		{
			plural = "s were ";
		}
		string label = "The following error" + plural + "encountered trying to save the record:\n";
		for (int i = 0; i < errorCount; i++)
		{
			label += errors[i] + "\n";
		}
		new ResponseJDialog(this, "Errors encountered", label, ResponseJDialog.OK);
		return false;
	}

	/// <summary>
	/// Enables or disables the time series display buttons according to how the 
	/// time series JCheckBoxes are selected.
	/// </summary>
	private void checkTimeSeriesButtonsStates()
	{
		bool enabled = false;

		if ((__ts_streamflow_base_monthly_JCheckBox.isSelected() && __ts_streamflow_base_monthly_JCheckBox.isEnabled()) || (__ts_streamflow_base_daily_JCheckBox.isSelected() && __ts_streamflow_base_daily_JCheckBox.isEnabled()))
		{
			enabled = true;
		}

		__graph_JButton.setEnabled(enabled);
		__table_JButton.setEnabled(enabled);
		__summary_JButton.setEnabled(enabled);
	}

	/// <summary>
	/// Checks the states of the map and network view buttons based on the selected stream estimate station.
	/// </summary>
	private void checkViewButtonState()
	{
		StateMod_StreamEstimate ses = getSelectedStreamEstimate();
		if (ses.getGeoRecord() == null)
		{
			// No spatial data are available
			__showOnMap_JButton.setEnabled(false);
		}
		else
		{
			// Enable the button...
			__showOnMap_JButton.setEnabled(true);
		}
	}

	/// <summary>
	/// Display the time series. </summary>
	/// <param name="action"> Event action that initiated the display. </param>
	private void displayTSViewJFrame(object o)
	{
		string routine = "displayTSViewJFrame";

		// Initialize the display...

		PropList display_props = new PropList("StreamEstimate");
		if (o == __graph_JButton)
		{
			display_props.set("InitialView", "Graph");
		}
		else if (o == __table_JButton)
		{
			display_props.set("InitialView", "Table");
		}
		else if (o == __summary_JButton)
		{
			display_props.set("InitialView", "Summary");
		}

		StateMod_StreamEstimate sta = (StateMod_StreamEstimate)__stationsVector[__currentStationIndex];

		// display_props.set("HelpKey", "TSTool.ExportMenu");
		display_props.set("TSViewTitleString", StateMod_Util.createDataLabel(sta,true) + " Time Series");
		display_props.set("DisplayFont", "Courier");
		display_props.set("DisplaySize", "11");
		display_props.set("PrintFont", "Courier");
		display_props.set("PrintSize", "7");
		display_props.set("PageLength", "100");

		PropList props = new PropList("StreamEstimate");
		props.set("Product.TotalWidth", "600");
		props.set("Product.TotalHeight", "400");

		IList<TS> tslist = new List<TS>();

		int sub = 0;
		int its = 0;
		TS ts = null;

		if ((__ts_streamflow_base_monthly_JCheckBox.isSelected() && (sta.getBaseflowMonthTS() != null)))
		{
			// Do the monthly graph...
			++sub;
			props.set("SubProduct " + sub + ".GraphType=Line");
			props.set("SubProduct " + sub + ".SubTitleString=Monthly Data for Stream Estimate Station " + sta.getID() + " (" + sta.getName() + ")");
			props.set("SubProduct " + sub + ".SubTitleFontSize=12");
			ts = sta.getBaseflowMonthTS();
			if (ts != null)
			{
				props.set("Data " + sub + "." + (++its) + ".TSID=" + ts.getIdentifierString());
				tslist.Add(ts);
			}
		}

		if ((__ts_streamflow_base_daily_JCheckBox.isSelected() && (sta.getBaseflowDayTS() != null)))
		{
			// Do the daily graph...
			++sub;
			its = 0;
			props.set("SubProduct " + sub + ".GraphType=Line");
			props.set("SubProduct " + sub + ".SubTitleString=Daily Data for Stream Estimate Station " + sta.getID() + " (" + sta.getName() + ")");
			props.set("SubProduct " + sub + ".SubTitleFontSize=12");
			ts = sta.getBaseflowDayTS();
			if (ts != null)
			{
				props.set("Data " + sub + "." + (++its) + ".TSID=" + ts.getIdentifierString());
				tslist.Add(ts);
			}
		}
		try
		{
			TSProduct tsproduct = new TSProduct(props, display_props);
			tsproduct.setTSList(tslist);
			new TSViewJFrame(tsproduct);
		}
		catch (Exception e)
		{
			Message.printWarning(1,routine,"Error displaying time series.");
			Message.printWarning(2, routine, e);
		}
	}

	/// <summary>
	/// Clean up before garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~StateMod_StreamEstimate_JFrame()
	{
		__searchCriteriaGroup = null;
		__coefficientsComp = null;
		__stationsComp = null;
		__textUneditables = null;
		__findNextButton = null;
		__helpJButton = null;
		__closeJButton = null;
		__disables = null;
		__searchIDJRadioButton = null;
		__searchNameJRadioButton = null;
		__nameJTextField = null;
		__idJTextField = null;
		__prorationFactorJTextField = null;
		__searchIDJTextField = null;
		__ts_streamflow_base_monthly_JCheckBox = null;
		__ts_streamflow_base_daily_JCheckBox = null;
		__searchNameJTextField = null;
		__worksheetR = null;
		__worksheetL = null;
		__dataset = null;
		__tableModelR = null;
		__stationsVector = null;
		__coefficientsVector = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Finds coefficients in the __coefficientsVector that has the specified id. </summary>
	/// <param name="id"> the id to match. </param>
	/// <returns> the matching coefficients object, or null if no matches could be found. </returns>
	private StateMod_StreamEstimate_Coefficients findCoefficients(string id)
	{
		StateMod_StreamEstimate_Coefficients coef = null;
		int pos = StateMod_Util.IndexOf(__coefficientsVector, id);
		if (pos >= 0)
		{
			;
			coef = (StateMod_StreamEstimate_Coefficients)__coefficientsVector[pos];
		}
		return coef;
	}

	/// <summary>
	/// Get the selected stream estimate station, based on the current index in the list.
	/// </summary>
	private StateMod_StreamEstimate getSelectedStreamEstimate()
	{
		return __stationsVector[__currentStationIndex];
	}

	/// <summary>
	/// Responds to item state changed events. </summary>
	/// <param name="e"> the ItemEvent that happened. </param>
	public virtual void itemStateChanged(ItemEvent e)
	{
		checkTimeSeriesButtonsStates();
	}

	/// <summary>
	/// Initializes the arrays that are used when items are selected and deselected.
	/// This should be called from setupGUI() before the a call is made to 
	/// selectTableIndex().
	/// </summary>
	private void initializeDisables()
	{
		__disables = new JComponent[8];
		int i = 0;
		__disables[i++] = __nameJTextField;
		__disables[i++] = __idJTextField;
		__disables[i++] = __prorationFactorJTextField;
		__disables[i++] = __graph_JButton;
		__disables[i++] = __table_JButton;
		__disables[i++] = __summary_JButton;
		__disables[i++] = __ts_streamflow_base_monthly_JCheckBox;
		__disables[i++] = __ts_streamflow_base_daily_JCheckBox;

		__textUneditables = new int[2];
		__textUneditables[0] = 0;
		__textUneditables[0] = 1;
	}

	/// <summary>
	/// Responds to key pressed events; does nothing. </summary>
	/// <param name="e"> the KeyEvent that happened. </param>
	public virtual void keyPressed(KeyEvent e)
	{
	}

	/// <summary>
	/// Responds to key released events; calls 'processLTableSelection' with the 
	/// newly-selected index in the table. </summary>
	/// <param name="e"> the KeyEvent that happened. </param>
	public virtual void keyReleased(KeyEvent e)
	{
		processLTableSelection(__worksheetL.getSelectedRow());
	}

	/// <summary>
	/// Responds to key typed events; does nothing. </summary>
	/// <param name="e"> the KeyEvent that happened. </param>
	public virtual void keyTyped(KeyEvent e)
	{
	}

	/// <summary>
	/// Responds to mouse clicked events; does nothing. </summary>
	/// <param name="e"> the MouseEvent that happened. </param>
	public virtual void mouseClicked(MouseEvent e)
	{
	}
	/// <summary>
	/// Responds to mouse entered events; does nothing. </summary>
	/// <param name="e"> the MouseEvent that happened. </param>
	public virtual void mouseEntered(MouseEvent e)
	{
	}
	/// <summary>
	/// Responds to mouse exited events; does nothing. </summary>
	/// <param name="e"> the MouseEvent that happened. </param>
	public virtual void mouseExited(MouseEvent e)
	{
	}
	/// <summary>
	/// Responds to mouse pressed events; does nothing. </summary>
	/// <param name="e"> the MouseEvent that happened. </param>
	public virtual void mousePressed(MouseEvent e)
	{
	}

	/// <summary>
	/// Responds to mouse released events; calls 'processLTableSelection' with the 
	/// newly-selected index in the table. </summary>
	/// <param name="e"> the MouseEvent that happened. </param>
	public virtual void mouseReleased(MouseEvent e)
	{
		processLTableSelection(__worksheetL.getSelectedRow());
	}

	/// <summary>
	/// Processes a table selection (either via a mouse press or programmatically 
	/// from selectLTableIndex() by writing the old data back to the data set component
	/// and getting the next selection's data out of the data and displaying it 
	/// on the form. </summary>
	/// <param name="index"> the index of the reservoir to display on the form. </param>
	private void processLTableSelection(int index)
	{
		__lastStationIndex = __currentStationIndex;
		__currentStationIndex = __worksheetL.getOriginalRowNumber(index);

		saveLastRecord();

		if (__worksheetL.getSelectedRow() == -1)
		{
			JGUIUtil.disableComponents(__disables, true);
			__tableModelR.setStreamEstimateCoefficients(null);
			__worksheetR.refresh();
			return;
		}

		StateMod_StreamEstimate sta = (StateMod_StreamEstimate) __stationsVector[__currentStationIndex];

		JGUIUtil.enableComponents(__disables, __textUneditables, __editable);
		checkTimeSeriesButtonsStates();

		__idJTextField.setText(sta.getID());
		__nameJTextField.setText(sta.getName());

		StateMod_StreamEstimate_Coefficients coef = findCoefficients(sta.getID());

		if (coef != null)
		{
			StateMod_GUIUtil.checkAndSet(coef.getProratnf(), __prorationFactorJTextField);
			__tableModelR.setStreamEstimateCoefficients(coef);
			__worksheetR.refresh();
		}
		else
		{
			__prorationFactorJTextField.setText("");
			__tableModelR.setStreamEstimateCoefficients(null);
			__worksheetR.refresh();
		}

		// For checkboxes, do not change the state of the checkbox, only
		// whether enabled - that way if the user has picked a combination of
		// parameters it is easy for them to keep the same settings when
		// switching between stations.  Make sure to do the following after
		// the generic enable/disable code is called above!

		if (sta.getBaseflowMonthTS() != null)
		{
			__ts_streamflow_base_monthly_JCheckBox.setEnabled(true);
		}
		else
		{
			__ts_streamflow_base_monthly_JCheckBox.setEnabled(false);
		}

		if (sta.getBaseflowDayTS() != null)
		{
			__ts_streamflow_base_daily_JCheckBox.setEnabled(true);
		}
		else
		{
			__ts_streamflow_base_daily_JCheckBox.setEnabled(false);
		}
		checkViewButtonState();
	}

	/// <summary>
	/// Saves the prior record selected in the table; called when moving to a new 
	/// record by a table selection.
	/// </summary>
	private void saveLastRecord()
	{
		saveInformation(__lastStationIndex);
	}

	/// <summary>
	/// Saves the current record selected in the table; called when the window is closed
	/// or minimized or apply is pressed.
	/// </summary>
	private void saveCurrentRecord()
	{
		saveInformation(__currentStationIndex);
	}

	/// <summary>
	/// Saves the information associated with the currently-selected reservoir.
	/// The user doesn't need to hit the return key for the gui to recognize changes.
	/// The info is saved each time the user selects a differents tation or pressed
	/// the close button.
	/// </summary>
	private void saveInformation(int record)
	{
		if (!__editable || record == -1)
		{
			return;
		}

		if (!checkInput())
		{
			return;
		}

		StateMod_StreamEstimate sta = (StateMod_StreamEstimate)__stationsVector[record];

		sta.setName(__nameJTextField.getText());
		sta.setID(__idJTextField.getText());

		StateMod_StreamEstimate_Coefficients coef = findCoefficients(sta.getID());

		if (coef != null)
		{
			coef.setProratnf(__prorationFactorJTextField.getText());
		}
	}

	/// <summary>
	/// Searches through the worksheet for a value, starting at the first row.
	/// If the value is found, the row is selected.
	/// </summary>
	public virtual void searchLWorksheet()
	{
		searchLWorksheet(0);
	}

	/// <summary>
	/// Selects the desired ID in the left table and displays the appropriate data
	/// in the remainder of the window. </summary>
	/// <param name="id"> the identifier to select in the list. </param>
	public virtual void selectID(string id)
	{
		int rows = __worksheetL.getRowCount();
		StateMod_StreamEstimate sta = null;
		for (int i = 0; i < rows; i++)
		{
			sta = (StateMod_StreamEstimate)__worksheetL.getRowData(i);
			if (sta.getID().Trim().Equals(id.Trim()))
			{
				selectLTableIndex(i);
				return;
			}
		}
	}

	/// <summary>
	/// Searches through the worksheet for a value, starting at the given row.
	/// If the value is found, the row is selected. </summary>
	/// <param name="row"> the row to start searching from. </param>
	public virtual void searchLWorksheet(int row)
	{
		string searchFor = null;
		int col = -1;
		if (__searchIDJRadioButton.isSelected())
		{
			searchFor = __searchIDJTextField.getText().Trim();
			col = 0;
		}
		else
		{
			searchFor = __searchNameJTextField.getText().Trim();
			col = 1;
		}
		int index = __worksheetL.find(searchFor, col, row, JWorksheet.FIND_EQUAL_TO | JWorksheet.FIND_CONTAINS | JWorksheet.FIND_CASE_INSENSITIVE | JWorksheet.FIND_WRAPAROUND);
		if (index != -1)
		{
			selectLTableIndex(index);
		}
	}

	/// <summary>
	/// Selects the desired index in the table, but also displays the appropriate data
	/// in the remainder of the window. </summary>
	/// <param name="index"> the index to select in the list. </param>
	public virtual void selectLTableIndex(int index)
	{
		int rowCount = __worksheetL.getRowCount();
		if (rowCount == 0)
		{
			return;
		}
		if (index > (rowCount + 1))
		{
			return;
		}
		if (index < 0)
		{
			return;
		}
		__worksheetL.scrollToRow(index);
		__worksheetL.selectRow(index);

		processLTableSelection(index);
	}

	/// <summary>
	/// Sets up the GUI. </summary>
	/// <param name="index"> the index of the stream estimate station to preselect from the
	/// list. </param>
	private void setupGUI(int index)
	{
		string routine = "StateMod_StreamEstimate_JFrame.setupGUI";

		addWindowListener(this);

		// AWT portion
		JPanel p1 = new JPanel(); // selection list and grid
		JPanel p2 = new JPanel(); // search widgets
		JPanel pmain = new JPanel(); // everything but close and help buttons

		__nameJTextField = new JTextField(24);
		__nameJTextField.setEditable(false);
		__idJTextField = new JTextField(12);
		__idJTextField.setEditable(false);
		__prorationFactorJTextField = new JTextField(12);

		__searchIDJTextField = new JTextField(12);
		__searchIDJTextField.addActionListener(this);
		__searchNameJTextField = new JTextField(12);
		__searchNameJTextField.addActionListener(this);
		__searchNameJTextField.setEditable(false);
		__findNextButton = new JButton(__BUTTON_FIND_NEXT);
		__searchCriteriaGroup = new ButtonGroup();
		__searchIDJRadioButton = new JRadioButton("ID", true);
		__searchIDJRadioButton.addActionListener(this);
		__searchCriteriaGroup.add(__searchIDJRadioButton);
		__searchNameJRadioButton = new JRadioButton("Name", false);
		__searchNameJRadioButton.addActionListener(this);
		__searchCriteriaGroup.add(__searchNameJRadioButton);

		__showOnMap_JButton = new SimpleJButton(__BUTTON_SHOW_ON_MAP, this);
		__showOnMap_JButton.setToolTipText("Annotate map with location (button is disabled if layer does not have matching ID)");
		__showOnNetwork_JButton = new SimpleJButton(__BUTTON_SHOW_ON_NETWORK, this);
		__showOnNetwork_JButton.setToolTipText("Annotate network with location");
		__applyJButton = new JButton(__BUTTON_APPLY);
		__cancelJButton = new JButton(__BUTTON_CANCEL);
		__helpJButton = new JButton(__BUTTON_HELP);
		__closeJButton = new JButton(__BUTTON_CLOSE);

		GridBagLayout gb = new GridBagLayout();
		p1.setLayout(gb);
		p2.setLayout(gb);
		pmain.setLayout(gb);

		int y;

		PropList p = new PropList("StateMod_StreamEstimate_JFrame.JWorksheet");
		p.add("JWorksheet.AllowCopy=true");
		p.add("JWorksheet.ShowPopupMenu=true");
		p.add("JWorksheet.SelectionMode=SingleRowSelection");

		int[] widthsL = null;
		JScrollWorksheet jswL = null;
		try
		{
			StateMod_StreamEstimate_TableModel tmr = new StateMod_StreamEstimate_TableModel(__stationsVector);
			StateMod_StreamEstimate_CellRenderer crr = new StateMod_StreamEstimate_CellRenderer(tmr);

			jswL = new JScrollWorksheet(crr, tmr, p);
			__worksheetL = jswL.getJWorksheet();

			widthsL = crr.getColumnWidths();
		}
		catch (Exception e)
		{
			Message.printWarning(2, routine, e);
			jswL = new JScrollWorksheet(0, 0, p);
			__worksheetL = jswL.getJWorksheet();
		}
		__worksheetL.setPreferredScrollableViewportSize(null);
		__worksheetL.setHourglassJFrame(this);
		__worksheetL.addMouseListener(this);
		__worksheetL.addKeyListener(this);

		JGUIUtil.addComponent(pmain, jswL, 0, 0, 4, 30, 1, 1, 10, 10, 1, 10, GridBagConstraints.BOTH, GridBagConstraints.WEST);

		JGUIUtil.addComponent(p1, new JLabel("ID:"), 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(p1, __idJTextField, 1, 0, 1, 1, 1, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(p1, new JLabel("Name:"), 0, 1, 1, 1, 1, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(p1, __nameJTextField, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(p1, new JLabel("Proration Factor:"), 0, 2, 1, 1, 1, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(p1, __prorationFactorJTextField, 1, 2, 1, 1, 1, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);

		JGUIUtil.addComponent(pmain, p1, 5, 0, 4, 4, 0, 0, 10, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);

		int[] widthsR = null;
		JScrollWorksheet jswR = null;
		try
		{
			IList<StateMod_StreamEstimate_Coefficients> v = new List<StateMod_StreamEstimate_Coefficients>();
			/*
			for (int i = 0; i < StateMod_RiverBaseFlow.MAX_BASEFLOWS;
				i++) {
				v.add("");
			}
			*/
			__tableModelR = new StateMod_StreamEstimate_Coefficients_TableModel(v);
			StateMod_StreamEstimate_Coefficients_CellRenderer crr = new StateMod_StreamEstimate_Coefficients_CellRenderer(__tableModelR);

			jswR = new JScrollWorksheet(crr, __tableModelR, p);
			__worksheetR = jswR.getJWorksheet();

			widthsR = crr.getColumnWidths();
		}
		catch (Exception e)
		{
			Message.printWarning(2, routine, e);
			jswR = new JScrollWorksheet(0, 0, p);
			__worksheetR = jswR.getJWorksheet();
		}
		__worksheetR.setPreferredScrollableViewportSize(null);
		__worksheetR.setHourglassJFrame(this);
		__worksheetR.addMouseListener(this);
		__worksheetR.addKeyListener(this);

		JPanel worksheetRPanel = new JPanel();
		worksheetRPanel.setLayout(gb);

		JGUIUtil.addComponent(worksheetRPanel, jswR, 0, 0, 1, 1, .5, 1, 0, 0, 0, 0, GridBagConstraints.BOTH, GridBagConstraints.SOUTHEAST);

		worksheetRPanel.setBorder(BorderFactory.createTitledBorder("Stream Estimate Coefficients"));

		JGUIUtil.addComponent(pmain, worksheetRPanel, 5, 5, 18, 24, .5, 1, 10, 10, 10, 10, GridBagConstraints.BOTH, GridBagConstraints.SOUTHWEST);

		JPanel tsPanel = new JPanel();
		tsPanel.setLayout(gb);

		__ts_streamflow_base_monthly_JCheckBox = new JCheckBox("Streamflow (Baseflow Monthly)");
		__ts_streamflow_base_monthly_JCheckBox.addItemListener(this);
		if (!__dataset.getComponentForComponentType(StateMod_DataSet.COMP_STREAMESTIMATE_NATURAL_FLOW_TS_MONTHLY).hasData())
		{
			__ts_streamflow_base_monthly_JCheckBox.setEnabled(false);
		}
		__ts_streamflow_base_daily_JCheckBox = new JCheckBox("Streamflow (Baseflow Daily)");
		__ts_streamflow_base_daily_JCheckBox.addItemListener(this);
		if (!__dataset.getComponentForComponentType(StateMod_DataSet.COMP_STREAMESTIMATE_NATURAL_FLOW_TS_DAILY).hasData())
		{
			__ts_streamflow_base_daily_JCheckBox.setEnabled(false);
		}

		JGUIUtil.addComponent(tsPanel, __ts_streamflow_base_monthly_JCheckBox, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.SOUTHWEST);
		JGUIUtil.addComponent(tsPanel, __ts_streamflow_base_daily_JCheckBox, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.SOUTHWEST);

		// Buttons for the time series...
		JPanel tsb_JPanel = new JPanel();
		tsb_JPanel.setLayout(new FlowLayout());
		__graph_JButton = new SimpleJButton("Graph", this);
		tsb_JPanel.add(__graph_JButton);
		__table_JButton = new SimpleJButton("Table", this);
		tsb_JPanel.add(__table_JButton);
		__summary_JButton = new SimpleJButton("Summary", this);
		tsb_JPanel.add(__summary_JButton);
		JGUIUtil.addComponent(tsPanel, tsb_JPanel, 0, 4, 1, 1, 1, 1, 1, 0, 0, 1, GridBagConstraints.NONE, GridBagConstraints.CENTER);

		tsPanel.setBorder(BorderFactory.createTitledBorder("Time Series"));

		JGUIUtil.addComponent(pmain, tsPanel, 5, 30, 1, 1, 0, 0, 10, 10, 10, 10, GridBagConstraints.BOTH, GridBagConstraints.SOUTHEAST);

		//
		// close and help buttons
		//
		JPanel pfinal = new JPanel();
		FlowLayout fl = new FlowLayout(FlowLayout.RIGHT);
		pfinal.setLayout(fl);
		pfinal.add(__showOnMap_JButton);
		pfinal.add(__showOnNetwork_JButton);
		if (__editable)
		{
			pfinal.add(__applyJButton);
			pfinal.add(__cancelJButton);
		}
		pfinal.add(__closeJButton);
	//	pfinal.add(__helpJButton);
		__applyJButton.addActionListener(this);
		__cancelJButton.addActionListener(this);
		__helpJButton.addActionListener(this);
		__helpJButton.setEnabled(false);
		__closeJButton.addActionListener(this);


		//
		// add search areas
		//
		y = 0;
		p2.setBorder(BorderFactory.createTitledBorder("Search above list for:     "));
		y++;
		JGUIUtil.addComponent(p2, __searchIDJRadioButton, 0, y, 1, 1, 0, 0, 5, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(p2, __searchIDJTextField, 1, y, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.HORIZONTAL, GridBagConstraints.EAST);
		__searchIDJTextField.addActionListener(this);
		y++;
		JGUIUtil.addComponent(p2, __searchNameJRadioButton, 0, y, 1, 1, 0, 0, 5, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(p2, __searchNameJTextField, 1, y, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.HORIZONTAL, GridBagConstraints.EAST);
		__searchNameJTextField.addActionListener(this);
		y++;
		JGUIUtil.addComponent(p2, __findNextButton, 0, y, 4, 1, 0, 0, 10, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.CENTER);
		__findNextButton.addActionListener(this);
		JGUIUtil.addComponent(pmain, p2, 0, GridBagConstraints.RELATIVE, 4, 1, 0, 0, 5, 10, 20, 10, GridBagConstraints.NONE, GridBagConstraints.SOUTHWEST);

		getContentPane().add("Center", pmain);
		getContentPane().add("South", pfinal);

		initializeDisables();

		selectLTableIndex(index);

		if (__dataset_wm != null)
		{
			__dataset_wm.setWindowOpen(StateMod_DataSet_WindowManager.WINDOW_STREAMESTIMATE, this);
		}
		pack();
		setSize(770, 500);
		JGUIUtil.center(this);
		setVisible(true);

		if (widthsR != null)
		{
			__worksheetR.setColumnWidths(widthsR);
		}
		if (widthsL != null)
		{
			__worksheetL.setColumnWidths(widthsL);
		}
		__graph_JButton.setEnabled(false);
		__table_JButton.setEnabled(false);
		__summary_JButton.setEnabled(false);

		__worksheetL.addSortListener(this);
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
		saveCurrentRecord();
		int size = __stationsVector.Count;
		StateMod_StreamEstimate s = null;
		bool changed = false;
		for (int i = 0; i < size; i++)
		{
			s = (StateMod_StreamEstimate)__stationsVector[i];
			if (!changed && s.changed())
			{
				changed = true;
			}
			s.acceptChanges();
		}
		if (changed)
		{
			__dataset.setDirty(StateMod_DataSet.COMP_STREAMESTIMATE_STATIONS, true);
		}
		size = __coefficientsVector.Count;
		StateMod_StreamEstimate_Coefficients c = null;
		changed = false;
		for (int i = 0; i < size; i++)
		{
			c = (StateMod_StreamEstimate_Coefficients)__coefficientsVector[i];
			if (!changed && c.changed())
			{
				changed = true;
			}
			c.acceptChanges();
		}
		if (changed)
		{
			__dataset.setDirty(StateMod_DataSet.COMP_STREAMESTIMATE_COEFFICIENTS,true);
		}
		if (__dataset_wm != null)
		{
			__dataset_wm.closeWindow(StateMod_DataSet_WindowManager.WINDOW_STREAMESTIMATE);
		}
	}

	/// <summary>
	/// Responds to Window deactivated events; does nothing. </summary>
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
		saveCurrentRecord();
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

	/// <summary>
	/// Called just before the worksheet is sorted.  Stores the index of the record
	/// that is selected. </summary>
	/// <param name="worksheet"> the worksheet being sorted. </param>
	/// <param name="sort"> the type of sort being performed. </param>
	public virtual void worksheetSortAboutToChange(JWorksheet worksheet, int sort)
	{
		__sortSelectedRow = __worksheetL.getOriginalRowNumber(__worksheetL.getSelectedRow());
	}

	/// <summary>
	/// Called when the worksheet is sorted.  Reselects the record that was selected
	/// prior to the sort. </summary>
	/// <param name="worksheet"> the worksheet being sorted. </param>
	/// <param name="sort"> the type of sort being performed. </param>
	public virtual void worksheetSortChanged(JWorksheet worksheet, int sort)
	{
		__worksheetL.deselectAll();
		__worksheetL.selectRow(__worksheetL.getSortedRowNumber(__sortSelectedRow));
	}

	}

}