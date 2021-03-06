﻿using System;
using System.Collections.Generic;

// StateMod_StreamGage_JFrame - dialog to edit the stream gage (.ris) file information

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
// StateMod_StreamGage_JFrame - dialog to edit the stream gage (.ris) file
//					information
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
// 
// 07 Jan 1998	Catherine E.		Created initial version of class
//		Nutting-Lane, RTi
// 01 Apr 2001	Steven A. Malers, RTi	Change GUI to JGUIUtil.  Add finalize().
//					Remove import *.
// 2002-09-12	SAM, RTi		Move the baseflow time series display
//					from the baseflows window to this
//					window.  Actually, display both the
//					baseflow and historic time series if
//					available.
//------------------------------------------------------------------------------
// 2003-08-18	J. Thomas Sapienza, RTi	Initial Swing version.
// 2003-08-20	JTS, RTi		* Added code so that the gui data is
//					  editable.
//					* Cleaned up the GUI.
//					* Objects can now be pre-selected from
//					  the second constructor.
// 2003-08-26	SAM, RTi		Enable StateMod_DataSet_WindowManager.
// 2003-08-27	JTS, RTi		Added selectID() to select an ID 
//					on the worksheet from outside the GUI.
// 2003-08-96	SAM, RTi		Update for changes in
//					StateMod_RiverStation
// 2003-09-03	JTS, RTi		Removed buttons for selecting time
//					series to view and replaced with
//					JCheckBoxes.
// 2003-09-03	SAM, RTi		* Always show the checkboxes but disable
//					  base on the time series that are
//					  available from the selected station.
//					* JTS had not fully removed buttons in
//					  previous changes.
// 2003-09-04	JTS, RTi		* Added crunidy combo box. 
//					* Added apply and cancel buttons.
//					* Put search widgets into titled panel.
// 2003-09-05	JTS, RTi		Class is now an item listener in 
//					order to enable/disable graph buttons
//					based on selected checkboxes.
// 2003-09-06	SAM, RTi		Fix problem with graphs from products
//					not recognizing size.
// 2003-09-08	JTS, RTi		* Added checkTimeSeriesButtonsStates()
//					  to enable time series display buttons
//					  appropriately.
//					* Adjusted layout.
// 2003-09-11	SAM, RTi		* Rename class from
//					  StateMod_RiverStation_JFrame to
//					  StateMod_StreamGage_JFrame.
//					* Adjust for general change in name from
//					  "Streamflow Station" to "StreamGage
//					  Station".
// 2003-09-18	SAM, RTi		Add estimated historical daily time
//					series.
// 2003-09-23	JTS, RTi		Uses new StateMod_GUIUtil code for
//					setting titles.
// 2004-01-22	JTS, RTi		Updated to use JScrollWorksheet and
//					the new row headers.
// 2004-07-15	JTS, RTi		* For data changes, enabled the
//					  Apply and Cancel buttons through new
//					  methods in the data classes.
//					* Changed layout of buttons to be
//					  aligned in the lower-right.
//					* windowDeactivated() no longer saves
//					  data because it was causing problems
//					  with the cancel code.
// 2006-01-19	JTS, RTi		* Now implements JWorksheet_SortListener
//					* Reselects the record that was selected
//					  when the worksheet is sorted.
// 2006-08-31	SAM, RTi		* Fix problem where checkboxes were
//					  not being checked to determine which
//					  time series should be plotted.
//					* Fix problem where estimated daily
//					  checkboxes were enabled even though
//					  they should have been disabled always
//					  (until software features are enabled).
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
//------------------------------------------------------------------------------
// EndHeader

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
	using JWorksheet = RTi.Util.GUI.JWorksheet;
	using JWorksheet_SortListener = RTi.Util.GUI.JWorksheet_SortListener;
	using ResponseJDialog = RTi.Util.GUI.ResponseJDialog;
	using SimpleJButton = RTi.Util.GUI.SimpleJButton;
	using SimpleJComboBox = RTi.Util.GUI.SimpleJComboBox;
	using DataSetComponent = RTi.Util.IO.DataSetComponent;
	using PropList = RTi.Util.IO.PropList;
	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// Class to display data about stream gage stations.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class StateMod_StreamGage_JFrame extends javax.swing.JFrame implements java.awt.event.ActionListener, java.awt.event.ItemListener, java.awt.event.KeyListener, java.awt.event.MouseListener, java.awt.event.WindowListener, RTi.Util.GUI.JWorksheet_SortListener
	public class StateMod_StreamGage_JFrame : JFrame, ActionListener, ItemListener, KeyListener, MouseListener, WindowListener, JWorksheet_SortListener
	{

	private bool __editable = true;

	/// <summary>
	/// Button group for selecting the kind of search to do.
	/// </summary>
	private ButtonGroup __searchCriteriaGroup;

	/// <summary>
	/// Data set component containing the form data.
	/// </summary>
	private DataSetComponent __streamGageStationComponent;

	/// <summary>
	/// Indices of the currently-selected and last-selected stations.
	/// </summary>
	private int __currentStationIndex = -1, __lastStationIndex = -1;

	/// <summary>
	/// Stores the index of the record that was selected before the worksheet is sorted,
	/// in order to reselect it after the sort is complete.
	/// </summary>
	private int __sortSelectedRow = -1;

	/// <summary>
	/// Buttons for performing operations on the form.
	/// </summary>
	private JButton __applyJButton, __findNext, __cancelJButton, __helpJButton, __closeJButton, __graph_JButton, __table_JButton, __summary_JButton, __showOnMap_JButton = null, __showOnNetwork_JButton = null;

	/// <summary>
	/// Checkboxes to select the time series to view.
	/// </summary>
	private JCheckBox __ts_streamflow_hist_monthly_JCheckBox, __ts_streamflow_hist_daily_JCheckBox, __ts_streamflow_est_hist_daily_JCheckBox, __ts_streamflow_base_monthly_JCheckBox, __ts_streamflow_base_daily_JCheckBox, __ts_streamflow_est_base_daily_JCheckBox;

	/// <summary>
	/// Radio buttons for selecting the kind of search to do.
	/// </summary>
	private JRadioButton __searchIDJRadioButton, __searchNameJRadioButton;

	/// <summary>
	/// GUI TextFields.
	/// </summary>
	private JTextField __idJTextField, __nameJTextField, __cgotoJTextField;

	/// <summary>
	/// Text fields for searching through the worksheet.
	/// </summary>
	private JTextField __searchID, __searchName;

	/// <summary>
	/// Worksheet for displaying stream gage station data.
	/// </summary>
	private JWorksheet __worksheet;

	/// <summary>
	/// Combo box for displaying crunidy data.
	/// </summary>
	private SimpleJComboBox __crunidyComboBox;

	/// <summary>
	/// Button label strings.
	/// </summary>
	private readonly string __BUTTON_SHOW_ON_MAP = "Show on Map", __BUTTON_SHOW_ON_NETWORK = "Show on Network", __BUTTON_APPLY = "Apply", __BUTTON_CANCEL = "Cancel", __BUTTON_CLOSE = "Close", __BUTTON_HELP = "Help", __BUTTON_FIND_NEXT = "Find Next";

	/// <summary>
	/// Data set containing the data for the form.
	/// </summary>
	private StateMod_DataSet __dataset;

	/// <summary>
	/// Data set window manager.
	/// </summary>
	private StateMod_DataSet_WindowManager __dataset_wm;

	/// <summary>
	/// List of stream gage station data to display in the form.
	/// </summary>
	private IList<StateMod_StreamGage> __streamGageStationsVector;

	/// <summary>
	/// The index in __disables[] of textfields and other components that should NEVER be made editable (e.g., ID fields).
	/// </summary>
	private int[] __textUneditables;

	/// <summary>
	/// Array of JComponents that should be disabled when nothing is selected from the list.
	/// </summary>
	private JComponent[] __disables;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="dataset"> the dataset containing the data to show in the form. </param>
	/// <param name="dataset_wm"> the dataset window manager or null if the data set windows are not being managed. </param>
	/// <param name="editable"> Indicates whether the data in the display should be editable. </param>
	public StateMod_StreamGage_JFrame(StateMod_DataSet dataset, StateMod_DataSet_WindowManager dataset_wm, bool editable)
	{
		StateMod_GUIUtil.setTitle(this, dataset, "Stream Gage Stations", null);
		JGUIUtil.setIcon(this, JGUIUtil.getIconImage());

		__dataset = dataset;
		__dataset_wm = dataset_wm;
		__streamGageStationComponent = __dataset.getComponentForComponentType(StateMod_DataSet.COMP_STREAMGAGE_STATIONS);
		__streamGageStationsVector = (IList<StateMod_StreamGage>)__streamGageStationComponent.getData();
		int size = __streamGageStationsVector.Count;
		StateMod_StreamGage s = null;
		for (int i = 0; i < size; i++)
		{
			s = __streamGageStationsVector[i];
			s.createBackup();
		}
		__editable = editable;

		setupGUI(0);
	}

	/// <summary>
	/// Constructor. </summary>
	/// <param name="dataset"> the dataset containing the data to show in the form. </param>
	/// <param name="station"> the object to preselect in the gui. </param>
	/// <param name="editable"> Indicates whether the data in the display should be editable. </param>
	public StateMod_StreamGage_JFrame(StateMod_DataSet dataset, StateMod_StreamGage station, bool editable)
	{
		StateMod_GUIUtil.setTitle(this, dataset, "Stream Gage Stations", null);
		JGUIUtil.setIcon(this, JGUIUtil.getIconImage());

		__dataset = dataset;
		__streamGageStationComponent = __dataset.getComponentForComponentType(StateMod_DataSet.COMP_STREAMGAGE_STATIONS);
		__streamGageStationsVector = (IList<StateMod_StreamGage>)__streamGageStationComponent.getData();
		int size = __streamGageStationsVector.Count;
		StateMod_StreamGage s = null;
		for (int i = 0; i < size; i++)
		{
			s = __streamGageStationsVector[i];
			s.createBackup();
		}

		string id = station.getID();
		int index = StateMod_Util.locateIndexFromID(id, __streamGageStationsVector);

		__editable = editable;

		setupGUI(index);
	}

	/// <summary>
	/// Responds to action performed events. </summary>
	/// <param name="e"> the ActionEvent that happened. </param>
	public virtual void actionPerformed(ActionEvent e)
	{
		string routine = "StateMod_StreamGage_JFrame.actionPerformed";
		if (Message.isDebugOn)
		{
			Message.printDebug(1, routine, "In actionPerformed: " + e.getActionCommand());
		}

		string action = e.getActionCommand();
		object source = e.getSource();

		if ((source == __graph_JButton) || (source == __table_JButton) || (source == __summary_JButton))
		{
			displayTSViewJFrame(source);
		}
		else if (action.Equals(__BUTTON_APPLY))
		{
			saveCurrentRecord();
			int size = __streamGageStationsVector.Count;
			StateMod_StreamGage s = null;
			bool changed = false;
			for (int i = 0; i < size; i++)
			{
				s = (StateMod_StreamGage)__streamGageStationsVector[i];
				if (!changed && s.changed())
				{
					changed = true;
				}
				s.createBackup();
			}
			if (changed)
			{
				__dataset.setDirty(StateMod_DataSet.COMP_STREAMGAGE_STATIONS,true);
			}
		}
		else if (action.Equals(__BUTTON_CANCEL))
		{
			int size = __streamGageStationsVector.Count;
			StateMod_StreamGage s = null;
			for (int i = 0; i < size; i++)
			{
				s = (StateMod_StreamGage)__streamGageStationsVector[i];
				s.restoreOriginal();
			}
			if (__dataset_wm != null)
			{
				__dataset_wm.closeWindow(StateMod_DataSet_WindowManager.WINDOW_STREAMGAGE);
			}
			else
			{
				JGUIUtil.close(this);
			}
		}
		else if (action.Equals(__BUTTON_CLOSE))
		{
			saveCurrentRecord();
			int size = __streamGageStationsVector.Count;
			StateMod_StreamGage s = null;
			bool changed = false;
			for (int i = 0; i < size; i++)
			{
				s = (StateMod_StreamGage)__streamGageStationsVector[i];
				if (!changed && s.changed())
				{
					changed = true;
				}
				s.acceptChanges();
			}
			if (changed)
			{
				__dataset.setDirty(StateMod_DataSet.COMP_STREAMGAGE_STATIONS,true);
			}
			if (__dataset_wm != null)
			{
				__dataset_wm.closeWindow(StateMod_DataSet_WindowManager.WINDOW_STREAMGAGE);
			}
			else
			{
				JGUIUtil.close(this);
			}
		}
		else if (action.Equals(__BUTTON_HELP))
		{
			// TODO HELP (JTS - 2003-08-18)
		}
		else if (source == __searchIDJRadioButton)
		{
			__searchName.setEditable(false);
			__searchID.setEditable(true);
		}
		else if (source == __searchNameJRadioButton)
		{
			__searchName.setEditable(true);
			__searchID.setEditable(false);
		}
		else if (source == __showOnMap_JButton)
		{
			GeoRecord geoRecord = getSelectedStreamGage().getGeoRecord();
			GRShape shape = geoRecord.getShape();
			__dataset_wm.showOnMap(getSelectedStreamGage(), "Gage: " + getSelectedStreamGage().getID() + " - " + getSelectedStreamGage().getName(), new GRLimits(shape.xmin, shape.ymin, shape.xmax, shape.ymax), geoRecord.getLayer().getProjection());
		}
		else if (source == __showOnNetwork_JButton)
		{
			StateMod_Network_JFrame networkEditor = __dataset_wm.getNetworkEditor();
			if (networkEditor != null)
			{
				HydrologyNode node = networkEditor.getNetworkJComponent().findNode(getSelectedStreamGage().getID(), false, false);
				if (node != null)
				{
					__dataset_wm.showOnNetwork(getSelectedStreamGage(), "Gage: " + getSelectedStreamGage().getID() + " - " + getSelectedStreamGage().getName(), new GRLimits(node.getX(),node.getY(),node.getX(),node.getY()));
				}
			}
		}
		else if (action.Equals(__BUTTON_FIND_NEXT))
		{
			searchWorksheet(__worksheet.getSelectedRow() + 1);
		}
		else if (source == __searchID || e.getSource() == __searchName)
		{
			searchWorksheet(0);
		}
	}

	/// <summary>
	/// Checks the text fields for validity before they are saved back into the data object. </summary>
	/// <returns> true if the text fields are okay, false if not. </returns>
	private bool checkInput()
	{
		IList<string> errors = new List<string>();
		int errorCount = 0;

		// for each field, check if it contains valid input.  If not,
		// create a string of the format "fieldname -- reason why it
		// is not correct" and add it to the errors vector.  Also increment error count

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
	/// Checks whether the time series display buttons need to be enabled or not based on the JCheckBoxes.
	/// </summary>
	private void checkTimeSeriesButtonsStates()
	{
		bool enabled = false;

		if ((__ts_streamflow_hist_monthly_JCheckBox.isSelected() && __ts_streamflow_hist_monthly_JCheckBox.isEnabled()) || (__ts_streamflow_hist_daily_JCheckBox.isSelected() && __ts_streamflow_hist_daily_JCheckBox.isEnabled()) || (__ts_streamflow_est_hist_daily_JCheckBox.isSelected() && __ts_streamflow_est_hist_daily_JCheckBox.isEnabled()) || (__ts_streamflow_base_monthly_JCheckBox.isSelected() && __ts_streamflow_base_monthly_JCheckBox.isEnabled()) || (__ts_streamflow_base_daily_JCheckBox.isSelected() && __ts_streamflow_base_daily_JCheckBox.isEnabled()) || (__ts_streamflow_est_base_daily_JCheckBox.isSelected() && __ts_streamflow_est_base_daily_JCheckBox.isEnabled()))
		{
			enabled = true;
		}

		__graph_JButton.setEnabled(enabled);
		__table_JButton.setEnabled(enabled);
		__summary_JButton.setEnabled(enabled);
	}

	/// <summary>
	/// Checks the states of the map and network view buttons based on the selected diversion.
	/// </summary>
	private void checkViewButtonState()
	{
		StateMod_StreamGage gage = getSelectedStreamGage();
		if (gage.getGeoRecord() == null)
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
	/// Display the time series.  Create two graphs as needed, one with ACFT monthly data, and one with CFS daily data. </summary>
	/// <param name="action"> Event action that initiated the display. </param>
	private void displayTSViewJFrame(object o)
	{
		string routine = "displayTSViewJFrame";

		// Initialize the display...

		PropList display_props = new PropList("StreamGage Station");
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

		StateMod_StreamGage sta = (StateMod_StreamGage)__streamGageStationsVector[__currentStationIndex];

		// display_props.set("HelpKey", "TSTool.ExportMenu");
		display_props.set("TSViewTitleString", StateMod_Util.createDataLabel(sta,true) + " Time Series");
		display_props.set("DisplayFont", "Courier");
		display_props.set("DisplaySize", "11");
		display_props.set("PrintFont", "Courier");
		display_props.set("PrintSize", "7");
		display_props.set("PageLength", "100");

		PropList props = new PropList("StreamGage");
		props.set("Product.TotalWidth", "600");
		props.set("Product.TotalHeight", "400");

		IList<TS> tslist = new List<TS>();

		int sub = 0;
		int its = 0;
		TS ts = null;

		if ((__ts_streamflow_hist_monthly_JCheckBox.isSelected() && (sta.getHistoricalMonthTS() != null)) || (__ts_streamflow_base_monthly_JCheckBox.isSelected() && (sta.getBaseflowMonthTS() != null)))
		{
			// Do the monthly graph...
			++sub;
			props.set("SubProduct " + sub + ".GraphType=Line");
			props.set("SubProduct " + sub + ".SubTitleString=Monthly Data for Stream Gage Station " + sta.getID() + " (" + sta.getName() + ")");
			props.set("SubProduct " + sub + ".SubTitleFontSize=12");
			ts = sta.getHistoricalMonthTS();
			if ((ts != null) && __ts_streamflow_hist_monthly_JCheckBox.isSelected())
			{
				props.set("Data " + sub + "." + (++its) + ".TSID=" + ts.getIdentifierString());
				tslist.Add(ts);
			}
			ts = sta.getBaseflowMonthTS();
			if ((ts != null) && __ts_streamflow_base_monthly_JCheckBox.isSelected())
			{
				props.set("Data " + sub + "." + (++its) + ".TSID=" + ts.getIdentifierString());
				tslist.Add(ts);
			}
		}

		if ((__ts_streamflow_hist_daily_JCheckBox.isSelected() && (sta.getHistoricalDayTS() != null)) || (__ts_streamflow_base_daily_JCheckBox.isSelected() && (sta.getBaseflowDayTS() != null)))
		{
			// Do the daily graph...
			++sub;
			its = 0;
			props.set("SubProduct " + sub + ".GraphType=Line");
			props.set("SubProduct " + sub + ".SubTitleString=Daily Data for Stream Gage Station " + sta.getID() + " (" + sta.getName() + ")");
			props.set("SubProduct " + sub + ".SubTitleFontSize=12");
			ts = sta.getHistoricalDayTS();
			if ((ts != null) && __ts_streamflow_hist_daily_JCheckBox.isSelected())
			{
				props.set("Data " + sub + "." + (++its) + ".TSID=" + ts.getIdentifierString());
				tslist.Add(ts);
			}
			ts = sta.getBaseflowDayTS();
			if ((ts != null) && __ts_streamflow_base_daily_JCheckBox.isSelected())
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
	~StateMod_StreamGage_JFrame()
	{
		__applyJButton = null;
		__cancelJButton = null;
		__searchCriteriaGroup = null;
		__streamGageStationComponent = null;
		__findNext = null;
		__helpJButton = null;
		__closeJButton = null;
		__searchIDJRadioButton = null;
		__searchNameJRadioButton = null;
		__searchID = null;
		__searchName = null;
		__worksheet = null;
		__dataset = null;
		__crunidyComboBox = null;
		__streamGageStationsVector = null;
		__ts_streamflow_est_base_daily_JCheckBox = null;
		__ts_streamflow_est_hist_daily_JCheckBox = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Get the selected stream gage, based on the current index in the list.
	/// </summary>
	private StateMod_StreamGage getSelectedStreamGage()
	{
		return __streamGageStationsVector[__currentStationIndex];
	}

	/// <summary>
	/// Initializes the arrays that are used when items are selected and deselected.
	/// This should be called from setupGUI() before the a call is made to selectTableIndex().
	/// </summary>
	private void initializeDisables()
	{
		__disables = new JComponent[14];
		int i = 0;

		__disables[i++] = __idJTextField;
		__disables[i++] = __nameJTextField;
		__disables[i++] = __cgotoJTextField;
		__disables[i++] = __graph_JButton;
		__disables[i++] = __table_JButton;
		__disables[i++] = __summary_JButton;
		__disables[i++] = __applyJButton;
		__disables[i++] = __ts_streamflow_hist_monthly_JCheckBox;
		__disables[i++] = __ts_streamflow_hist_daily_JCheckBox;
		__disables[i++] = __ts_streamflow_est_hist_daily_JCheckBox;
		__disables[i++] = __ts_streamflow_base_monthly_JCheckBox;
		__disables[i++] = __ts_streamflow_base_daily_JCheckBox;
		__disables[i++] = __ts_streamflow_est_base_daily_JCheckBox;
		__disables[i++] = __crunidyComboBox;

		__textUneditables = new int[4];
		__textUneditables[0] = 0;
		__textUneditables[1] = 1;
		__textUneditables[2] = 9;
		__textUneditables[3] = 12;
	}

	/// <summary>
	/// Responds to item state changed events. </summary>
	/// <param name="e"> the ItemEvent that happened. </param>
	public virtual void itemStateChanged(ItemEvent e)
	{
		checkTimeSeriesButtonsStates();
	}

	/// <summary>
	/// Responds to key pressed events; does nothing. </summary>
	/// <param name="e"> the KeyEvent that happened. </param>
	public virtual void keyPressed(KeyEvent e)
	{
	}

	/// <summary>
	/// Responds to key released events; calls 'processTableSelection' with the newly-selected index in the table. </summary>
	/// <param name="e"> the KeyEvent that happened. </param>
	public virtual void keyReleased(KeyEvent e)
	{
		processTableSelection(__worksheet.getSelectedRow());
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
	/// Responds to mouse released events; calls 'processTableSelection' with the newly-selected index in the table. </summary>
	/// <param name="e"> the MouseEvent that happened. </param>
	public virtual void mouseReleased(MouseEvent e)
	{
		processTableSelection(__worksheet.getSelectedRow());
	}

	/// <summary>
	/// Fills in the values for the crunidy combo box. </summary>
	/// <param name="id"> the id of the currently-selected stream gage station, which will be 
	/// excluded from the list of stream gage stations in the crunidy combo box. </param>
	private void populateCrunidyComboBox()
	{
		__crunidyComboBox.removeAllItems();

		__crunidyComboBox.add("0 - Use average daily value from monthly time series");
		__crunidyComboBox.add("3 - Daily time series are supplied");
		__crunidyComboBox.add("4 - Daily time series interpolated from midpoints of monthly data");

		IList<string> idNameVector = StateMod_Util.createIdentifierListFromStateModData(__streamGageStationsVector, true,null);
		int size = idNameVector.Count;

		string s = null;
		for (int i = 0; i < size; i++)
		{
			s = (string)idNameVector[i];
			__crunidyComboBox.add(s.Trim());
		}
	}

	/// <summary>
	/// Processes a table selection (either via a mouse press or programmatically 
	/// from selectTableIndex() by writing the old data back to the data set component
	/// and getting the next selection's data out of the data and displaying it on the form. </summary>
	/// <param name="index"> the index of the reservoir to display on the form. </param>
	private void processTableSelection(int index)
	{
		string routine = "StateMod_StreamGage_JFrame.processTableSelection";

		__lastStationIndex = __currentStationIndex;
		__currentStationIndex = __worksheet.getOriginalRowNumber(index);
		if (__currentStationIndex < 0)
		{
			JGUIUtil.disableComponents(__disables, true);
			return;
		}

		// If a time series is available, enable the time series button...
		saveLastRecord();

		StateMod_StreamGage r = (StateMod_StreamGage)__streamGageStationsVector[__currentStationIndex];

		JGUIUtil.enableComponents(__disables, __textUneditables, __editable);
		checkTimeSeriesButtonsStates();

		// For checkboxes, do not change the state of the checkbox, only
		// whether enabled - that way if the user has picked a combination of
		// parameters it is easy for them to keep the same settings when
		// switching between stations.  Make sure to do the following after
		// the generic enable/disable code is called above!

		if (r.getHistoricalMonthTS() != null)
		{
			__ts_streamflow_hist_monthly_JCheckBox.setEnabled(true);
		}
		else
		{
			__ts_streamflow_hist_monthly_JCheckBox.setEnabled(false);
		}

		if (r.getHistoricalDayTS() != null)
		{
			__ts_streamflow_hist_daily_JCheckBox.setEnabled(true);
		}
		else
		{
			__ts_streamflow_hist_daily_JCheckBox.setEnabled(false);
		}

		if (r.getBaseflowMonthTS() != null)
		{
			__ts_streamflow_base_monthly_JCheckBox.setEnabled(true);
		}
		else
		{
			__ts_streamflow_base_monthly_JCheckBox.setEnabled(false);
		}

		if (r.getBaseflowDayTS() != null)
		{
			__ts_streamflow_base_daily_JCheckBox.setEnabled(true);
		}
		else
		{
			__ts_streamflow_base_daily_JCheckBox.setEnabled(false);
		}

		__idJTextField.setText(r.getID());
		__nameJTextField.setText(r.getName());
		__cgotoJTextField.setText(r.getCgoto());

		if (__lastStationIndex != -1)
		{
			string s = __crunidyComboBox.getStringAt(3);
			__crunidyComboBox.removeAt(3);
			__crunidyComboBox.addAlpha(s, 3);
		}
		string s = "" + r.getID() + " - " + r.getName();
		__crunidyComboBox.remove(s);
		__crunidyComboBox.addAt(s, 3);

		string c = r.getCrunidy();
		if (c.Trim().Equals(""))
		{
			if (!__crunidyComboBox.setSelectedPrefixItem(r.getID()))
			{
				Message.printWarning(2, routine, "No Crunidy value matching '" + r.getID() + "' found in combo box.");
				__crunidyComboBox.select(0);
			}
			else
			{
				setOriginalCrunidy(r, r.getID());
			}
		}
		else
		{
			if (!__crunidyComboBox.setSelectedPrefixItem(c))
			{
				Message.printWarning(2, routine, "No Crunidy value matching '" + c + "' found in combo box.");
				__crunidyComboBox.select(0);
			}
		}
		checkViewButtonState();
	}

	/// <summary>
	/// Saves the prior record selected in the table; called when moving to a new record by a table selection.
	/// </summary>
	private void saveLastRecord()
	{
		saveInformation(__lastStationIndex);
	}

	/// <summary>
	/// Saves the current record selected in the table; called when the window is closed or minimized or apply is pressed.
	/// </summary>
	private void saveCurrentRecord()
	{
		saveInformation(__currentStationIndex);
	}

	/// <summary>
	/// Saves the information associated with the currently-selected stream gage
	/// station.  The user doesn't need to hit the return key for the gui to recognize
	/// changes.  The info is saved each time the user selects a different station or pressed the close button.
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

		StateMod_StreamGage r = (StateMod_StreamGage)__streamGageStationsVector[record];

		r.setName(__nameJTextField.getText());
		r.setCgoto(__cgotoJTextField.getText());
		string crunidy = __crunidyComboBox.getSelected();
		int index = crunidy.IndexOf(" - ", StringComparison.Ordinal);
		if (index > -1)
		{
			r.setCrunidy(crunidy.Substring(0, index));
		}
		else
		{
			r.setCrunidy("");
		}
	}

	/// <summary>
	/// Searches through the worksheet for a value, starting at the first row.
	/// If the value is found, the row is selected.
	/// </summary>
	public virtual void searchWorksheet()
	{
		searchWorksheet(0);
	}

	/// <summary>
	/// Searches through the worksheet for a value, starting at the given row.
	/// If the value is found, the row is selected. </summary>
	/// <param name="row"> the row to start searching from. </param>
	public virtual void searchWorksheet(int row)
	{
		string searchFor = null;
		int col = -1;
		if (__searchIDJRadioButton.isSelected())
		{
			searchFor = __searchID.getText().Trim();
			col = 0;
		}
		else
		{
			searchFor = __searchName.getText().Trim();
			col = 1;
		}
		int index = __worksheet.find(searchFor, col, row, JWorksheet.FIND_EQUAL_TO | JWorksheet.FIND_CONTAINS | JWorksheet.FIND_CASE_INSENSITIVE | JWorksheet.FIND_WRAPAROUND);
		if (index != -1)
		{
			selectTableIndex(index);
		}
	}

	/// <summary>
	/// Selects the desired ID in the table and displays the appropriate data in the remainder of the window. </summary>
	/// <param name="id"> the identifier to select in the list. </param>
	public virtual void selectID(string id)
	{
		int rows = __worksheet.getRowCount();
		StateMod_StreamGage rs = null;
		for (int i = 0; i < rows; i++)
		{
			rs = (StateMod_StreamGage)__worksheet.getRowData(i);
			if (rs.getID().Trim().Equals(id.Trim()))
			{
				selectTableIndex(i);
				return;
			}
		}
	}

	/// <summary>
	/// Selects the desired index in the table, but also displays the appropriate data in the remainder of the window. </summary>
	/// <param name="index"> the index to select in the list. </param>
	public virtual void selectTableIndex(int index)
	{
		int rowCount = __worksheet.getRowCount();
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
		__worksheet.scrollToRow(index);
		__worksheet.selectRow(index);
		processTableSelection(index);
	}

	/// <summary>
	/// Sets up the GUI. </summary>
	/// <param name="index"> the index of the object to preselect on the gui. </param>
	private void setupGUI(int index)
	{
		string routine = "StateMod_StreamGage_JFrame.setupGUI";

		addWindowListener(this);

		JPanel p1 = new JPanel(); // entire top half

		__searchID = new JTextField(10);
		__searchName = new JTextField(10);
		__findNext = new JButton(__BUTTON_FIND_NEXT);
		__searchCriteriaGroup = new ButtonGroup();
		__searchIDJRadioButton = new JRadioButton("ID", true);
		__searchIDJRadioButton.addActionListener(this);
		__searchCriteriaGroup.add(__searchIDJRadioButton);
		__searchNameJRadioButton = new JRadioButton("Name", false);
		__searchNameJRadioButton.addActionListener(this);
		__searchCriteriaGroup.add(__searchNameJRadioButton);

		__idJTextField = new JTextField(12);
		__idJTextField.setEditable(false);
		__nameJTextField = new JTextField(24);
		__nameJTextField.setEditable(false);
		__cgotoJTextField = new JTextField(12);
		__cgotoJTextField.setEditable(false);
		__crunidyComboBox = new SimpleJComboBox();

		__helpJButton = new JButton(__BUTTON_HELP);
		__closeJButton = new JButton(__BUTTON_CLOSE);
		__cancelJButton = new JButton(__BUTTON_CANCEL);
		__applyJButton = new JButton(__BUTTON_APPLY);
		__showOnMap_JButton = new SimpleJButton(__BUTTON_SHOW_ON_MAP, this);
		__showOnMap_JButton.setToolTipText("Annotate map with location (button is disabled if layer does not have matching ID)");
		__showOnNetwork_JButton = new SimpleJButton(__BUTTON_SHOW_ON_NETWORK, this);
		__showOnNetwork_JButton.setToolTipText("Annotate network with location");

		GridBagLayout gb = new GridBagLayout();
		p1.setLayout(gb);

		int y = 0;

		PropList p = new PropList("StateMod_StreamGage_JFrame.JWorksheet");
		p.add("JWorksheet.ShowPopupMenu=true");
		p.add("JWorksheet.AllowCopy=true");
		p.add("JWorksheet.SelectionMode=SingleRowSelection");

		int[] widths = null;
		try
		{
			StateMod_StreamGage_TableModel tmr = new StateMod_StreamGage_TableModel(__streamGageStationsVector);
			StateMod_StreamGage_CellRenderer crr = new StateMod_StreamGage_CellRenderer(tmr);

			__worksheet = new JWorksheet(crr, tmr, p);

			widths = crr.getColumnWidths();
		}
		catch (Exception e)
		{
			Message.printWarning(2, routine, e);
			__worksheet = new JWorksheet(0, 0, p);
		}
		__worksheet.setPreferredScrollableViewportSize(null);
		__worksheet.setHourglassJFrame(this);
		__worksheet.addMouseListener(this);
		__worksheet.addKeyListener(this);
		JPanel param_JPanel = new JPanel();
		param_JPanel.setLayout(gb);

		JGUIUtil.addComponent(param_JPanel, new JLabel("Station ID:"), 5, y, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(param_JPanel, __idJTextField, 6, y++, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(param_JPanel, new JLabel("Station name:"), 5, y, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(param_JPanel, __nameJTextField, 6, y++, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(param_JPanel, new JLabel("River node ID:"), 5, y, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(param_JPanel, __cgotoJTextField, 6, y++, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(param_JPanel,new JLabel("Daily data ID:"), 5, y, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(param_JPanel, __crunidyComboBox, 6, y++, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);

		JPanel tsPanel = new JPanel();
		tsPanel.setLayout(gb);

		tsPanel.setBorder(BorderFactory.createTitledBorder("Time series"));

		__ts_streamflow_hist_monthly_JCheckBox = new JCheckBox("Streamflow (Historical Monthly)");
		__ts_streamflow_hist_monthly_JCheckBox.addItemListener(this);
		if (!__dataset.getComponentForComponentType(StateMod_DataSet.COMP_STREAMGAGE_HISTORICAL_TS_MONTHLY).hasData())
		{
			__ts_streamflow_hist_monthly_JCheckBox.setEnabled(false);
		}
		__ts_streamflow_hist_daily_JCheckBox = new JCheckBox("Streamflow (Historical Daily)");
		__ts_streamflow_hist_daily_JCheckBox.addItemListener(this);
		if (!__dataset.getComponentForComponentType(StateMod_DataSet.COMP_STREAMGAGE_HISTORICAL_TS_DAILY).hasData())
		{
			__ts_streamflow_hist_daily_JCheckBox.setEnabled(false);
		}

		// TODO SAM 2006-08-31
		// Need to enable - for now always disabled...
		// This checkbox needs to be enabled when the daily identifier
		// indicates using another time series to estimate the daily time series.
		__ts_streamflow_est_hist_daily_JCheckBox = new JCheckBox("Streamflow (Estimated Historical Daily)");
		__ts_streamflow_est_hist_daily_JCheckBox.addItemListener(this);
		__ts_streamflow_est_hist_daily_JCheckBox.setEnabled(false);

		__ts_streamflow_base_monthly_JCheckBox = new JCheckBox("Streamflow (Baseflow Monthly)");
		__ts_streamflow_base_monthly_JCheckBox.addItemListener(this);
		if (!__dataset.getComponentForComponentType(StateMod_DataSet.COMP_STREAMGAGE_NATURAL_FLOW_TS_MONTHLY).hasData())
		{
			__ts_streamflow_base_monthly_JCheckBox.setEnabled(false);
		}
		__ts_streamflow_base_daily_JCheckBox = new JCheckBox("Streamflow (Baseflow Daily)");
		__ts_streamflow_base_daily_JCheckBox.addItemListener(this);
		if (!__dataset.getComponentForComponentType(StateMod_DataSet.COMP_STREAMGAGE_NATURAL_FLOW_TS_DAILY).hasData())
		{
			__ts_streamflow_base_daily_JCheckBox.setEnabled(false);
		}
		// TODO SAM 2006-08-31
		// Need to enable - for now always disabled...
		// This checkbox needs to be enabled when the daily identifier
		// indicates using another time series to estimate the daily
		// time series.  Need to support all the StateMod options for computing the time series.
		__ts_streamflow_est_base_daily_JCheckBox = new JCheckBox("Streamflow (Estimated Baseflow Daily)");
		__ts_streamflow_est_base_daily_JCheckBox.addItemListener(this);
		__ts_streamflow_est_base_daily_JCheckBox.setEnabled(false);

		y = 0;
		JGUIUtil.addComponent(tsPanel, __ts_streamflow_hist_monthly_JCheckBox, 0, y, 1, 1, 1, 1, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.SOUTHWEST);
		JGUIUtil.addComponent(tsPanel, __ts_streamflow_hist_daily_JCheckBox, 0, ++y, 1, 1, 1, 1, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.SOUTHWEST);
		JGUIUtil.addComponent(tsPanel, __ts_streamflow_est_hist_daily_JCheckBox, 0, ++y, 1, 1, 1, 1, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.SOUTHWEST);
		JGUIUtil.addComponent(tsPanel, __ts_streamflow_base_monthly_JCheckBox, 0, ++y, 1, 1, 1, 1, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.SOUTHWEST);
		JGUIUtil.addComponent(tsPanel, __ts_streamflow_base_daily_JCheckBox, 0, ++y, 1, 1, 1, 1, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.SOUTHWEST);
		JGUIUtil.addComponent(tsPanel, __ts_streamflow_est_base_daily_JCheckBox, 0, ++y, 1, 1, 1, 1, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.SOUTHWEST);

		// Buttons for the time series...
		JPanel tsb_JPanel = new JPanel();
		tsb_JPanel.setLayout(new FlowLayout());
		__graph_JButton = new SimpleJButton("Graph", this);
		tsb_JPanel.add(__graph_JButton);
		__table_JButton = new SimpleJButton("Table", this);
		tsb_JPanel.add(__table_JButton);
		__summary_JButton = new SimpleJButton("Summary", this);
		tsb_JPanel.add(__summary_JButton);

		JGUIUtil.addComponent(tsPanel, tsb_JPanel, 0, ++y, 1, 1, 1, 1, 1, 0, 0, 1, GridBagConstraints.NONE, GridBagConstraints.CENTER);

		// Based on whether there are any components with data for the given checkboxes

		//
		// add search areas
		//	

		y = 0;

		JPanel searchPanel = new JPanel();
		searchPanel.setLayout(gb);
		searchPanel.setBorder(BorderFactory.createTitledBorder("Search above list for:"));
		JGUIUtil.addComponent(searchPanel, __searchIDJRadioButton, 0, ++y, 1, 1, 0, 0, 5, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(searchPanel, __searchID, 1, y, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.HORIZONTAL, GridBagConstraints.EAST);
		__searchID.addActionListener(this);
		JGUIUtil.addComponent(searchPanel, __searchNameJRadioButton, 0, ++y, 1, 1, 0, 0, 5, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		__searchName.setEditable(false);
		JGUIUtil.addComponent(searchPanel, __searchName, 1, y, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.HORIZONTAL, GridBagConstraints.EAST);
		__searchName.addActionListener(this);
		JGUIUtil.addComponent(searchPanel, __findNext, 0, ++y, 4, 1, 0, 0, 10, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		__findNext.addActionListener(this);

		JGUIUtil.addComponent(p1, new JScrollPane(__worksheet), 0, 0, 1, 1, 1, 1, 0, 0, 0, 0, GridBagConstraints.BOTH, GridBagConstraints.NORTHWEST);

		JPanel right = new JPanel();
		right.setLayout(gb);

		JGUIUtil.addComponent(p1, right, 1, 0, 1, 2, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.NORTHWEST);

		JGUIUtil.addComponent(right, param_JPanel, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.NORTHWEST);

		JGUIUtil.addComponent(right, tsPanel, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.NORTHWEST);

		JGUIUtil.addComponent(p1, searchPanel, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);

		//
		// add close and help buttons
		//
		JPanel pfinal = new JPanel();
		FlowLayout fl = new FlowLayout(FlowLayout.RIGHT);
		pfinal.setLayout(fl);
		__helpJButton.setEnabled(false);
		pfinal.add(__showOnMap_JButton);
		pfinal.add(__showOnNetwork_JButton);
		if (__editable)
		{
			pfinal.add(__applyJButton);
			pfinal.add(__cancelJButton);
		}
		pfinal.add(__closeJButton);
	//	pfinal.add(__helpJButton);
		__helpJButton.addActionListener(this);
		__closeJButton.addActionListener(this);
		__cancelJButton.addActionListener(this);
		__applyJButton.addActionListener(this);

		getContentPane().add("Center", p1);
		getContentPane().add("South", pfinal);

		// this must be done before the first selectTableIndex() call
		populateCrunidyComboBox();

		initializeDisables();

		selectTableIndex(index);

		if (__dataset_wm != null)
		{
			__dataset_wm.setWindowOpen(StateMod_DataSet_WindowManager.WINDOW_STREAMGAGE, this);
		}

		pack();
		setSize(875, 500);
		JGUIUtil.center(this);
		setVisible(true);

		if (widths != null)
		{
			__worksheet.setColumnWidths(widths);
		}
		__graph_JButton.setEnabled(false);
		__table_JButton.setEnabled(false);
		__summary_JButton.setEnabled(false);

		__worksheet.addSortListener(this);
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
	/// Responds to Window closing events; closes the window and marks it closed in StateMod_GUIUtil. </summary>
	/// <param name="e"> the WindowEvent that happened. </param>
	public virtual void windowClosing(WindowEvent e)
	{
		saveCurrentRecord();
		int size = __streamGageStationsVector.Count;
		StateMod_StreamGage s = null;
		bool changed = false;
		for (int i = 0; i < size; i++)
		{
			s = (StateMod_StreamGage)__streamGageStationsVector[i];
			if (!changed && s.changed())
			{
				changed = true;
			}
			s.acceptChanges();
		}
		if (changed)
		{
			__dataset.setDirty(StateMod_DataSet.COMP_STREAMGAGE_STATIONS,true);
		}
		if (__dataset_wm != null)
		{
			__dataset_wm.closeWindow(StateMod_DataSet_WindowManager.WINDOW_STREAMGAGE);
		}
		else
		{
			JGUIUtil.close(this);
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
	/// Responds to Window iconified events; saves the current record. </summary>
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

	private void setOriginalCrunidy(StateMod_StreamGage r, string crunidy)
	{
		((StateMod_StreamGage)r._original)._crunidy = crunidy;
	}

	/// <summary>
	/// Called just before the worksheet is sorted.  Stores the index of the record that is selected. </summary>
	/// <param name="worksheet"> the worksheet being sorted. </param>
	/// <param name="sort"> the type of sort being performed. </param>
	public virtual void worksheetSortAboutToChange(JWorksheet worksheet, int sort)
	{
		__sortSelectedRow = __worksheet.getOriginalRowNumber(__worksheet.getSelectedRow());
	}

	/// <summary>
	/// Called when the worksheet is sorted.  Reselects the record that was selected prior to the sort. </summary>
	/// <param name="worksheet"> the worksheet being sorted. </param>
	/// <param name="sort"> the type of sort being performed. </param>
	public virtual void worksheetSortChanged(JWorksheet worksheet, int sort)
	{
		__worksheet.deselectAll();
		__worksheet.selectRow(__worksheet.getSortedRowNumber(__sortSelectedRow));
	}

	}

}