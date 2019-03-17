﻿using System;
using System.Collections.Generic;

// StateMod_InstreamFlow_JFrame - JFrame to edit the instream flow information.

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
// StateMod_InstreamFlow_JFrame - JFrame to edit the instream flow information.
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
// 
// 06 Oct 1997	Catherine E.		Created initial version of class.
//		Nutting-Lane, RTi
// 22 Sep 1998	CEN, RTi		Changed list to multilist
// 08 Mar 2000	CEN, RTi		Added radio buttons to search
// 01 Apr 2001	Steven A. Malers, RTi	Change GUI to JGUIUtil.  Add finalize().
//					Remove import *.
// 04 May 2001	SAM, RTi		Enable TSView.
// 15 Aug 2001	SAM, RTi		Select the first item initially.
//------------------------------------------------------------------------------
// 2003-06-09	J. Thomas Sapienza, RTi	Initial version from 
//					SMinsflowsWindow
// 2003-06-19	JTS, RTi		First functional version.
// 2003-06-20	JTS, RTi		Constructor now takes a data set as
//					a parameter, instead of a data set
//					component.
// 2003-06-23	JTS, RTi		Opened code related to graphing 
//					monthly time series.
// 2003-07-15	JTS, RTi		* Added checkInput() framework for 
//					validating user input prior to the 
//					values being saved.
// 					* Added status bar.
//					* Changed to use new dataset design.
// 2003-07-16	JTS, RTi		Added constructor that allows an 
//					instream flow to be initially selected.
// 2003-07-17	JTS, RTI		Change so that constructor takes a 
//					boolean that says whether the form's
//					data can be modified.
// 2003-07-23	JTS, RTi		Updated JWorksheet code following
//					JWorksheet revisions.
// 2003-08-03	SAM, RTi		* indexOf() is now in StateMod_Util.
//					* Force title parameter in constructor.
// 2003-08-16	SAM, RTi		Change the window type to
//					WINDOW_INSTREAM.
// 2003-08-26	SAM, RTi		Enable StateMod_DataSet_WindowManager.
// 2003-08-27	JTS, RTi		Added selectID() to select an ID 
//					on the worksheet from outside the GUI.
// 2003-08-29	SAM, RTi		Update due to changed in
//					StateMod_InstreamFlow.
// 2003-09-03	JTS, RTi		Removed buttons for selecting time
//					series, added checkboxes.  
// 2003-09-04	JTS, RTi		* Changed Daily ID to be a combo box
//					  and changed its save and load logic.
//					* Added Data Type field.
// 2003-09-05	JTS, RTi		Class is now an item listener in 
//					order to enable/disable graph buttons
//					based on selected checkboxes.
// 2003-09-06	SAM, RTi		Fix size problem with graph.
// 2003-09-08	JTS, RTi		* Added checkTimeSeriesButtonsStates()
//					  to to properly determine when the
//					  buttons should be enabled.
//					* Adjusted the layout.
// 2003-09-23	JTS, RTi		Uses new StateMod_GUIUtil code for
//					setting titles.
// 2004-01-21	JTS, RTi		Updated to use JScrollWorksheet and
//					the new row headers.
// 2004-07-15	JTS, RTi		* For data changes, enabled the
//					  Apply and Cancel buttons through new
//					  methods in the data classes.
//					* Changed layout of buttons to be
//					  aligned in the lower-right.
// 2004-08-26	JTS, RTi		The apply button was closing the form
//					instead of just applying changes.  
//					Corrected.
// 2004-10-28	SAM, RTi		Update to reflect the table model not
//					having rights any more.
// 2005-01-18	JTS, RTi		Removed calls to removeColumn() as 
//					the table model handles that now.
// 2006-01-19	JTS, RTi		* Now implements JWorksheet_SortListener
//					* Reselects the record that was selected
//					  when the worksheet is sorted.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
//------------------------------------------------------------------------------
// EndHeader

namespace DWR.StateMod
{


	using HydrologyNode = cdss.domain.hydrology.network.HydrologyNode;

	using GeoProjection = RTi.GIS.GeoView.GeoProjection;
	using GeoRecord = RTi.GIS.GeoView.GeoRecord;
	using HasGeoRecord = RTi.GIS.GeoView.HasGeoRecord;
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
	using SimpleJComboBox = RTi.Util.GUI.SimpleJComboBox;
	using DataSetComponent = RTi.Util.IO.DataSetComponent;
	using PropList = RTi.Util.IO.PropList;
	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// This class is a gui for displaying and editing Instream Flow information.
	/// </summary>
	public class StateMod_InstreamFlow_JFrame : JFrame, ActionListener, ItemListener, KeyListener, MouseListener, WindowListener, JWorksheet_SortListener
	{

	/// <summary>
	/// Button labels.
	/// </summary>
	private readonly string __BUTTON_SHOW_ON_MAP = "Show on Map", __BUTTON_SHOW_ON_NETWORK = "Show on Network", __BUTTON_APPLY = "Apply", __BUTTON_CANCEL = "Cancel", __BUTTON_CLOSE = "Close", __BUTTON_FIND_NEXT = "Find Next", __BUTTON_HELP = "Help", __BUTTON_WATER_RIGHTS = "Water Rights...";

	/// <summary>
	/// Whether the gui data is editable or not.
	/// </summary>
	private bool __editable = false;

	/// <summary>
	/// The button group for the search radio buttons.
	/// </summary>
	private ButtonGroup __searchCriteriaGroup;

	/// <summary>
	/// The index in __disables[] of textfields that should NEVER be made editable (e.g., ID fields).
	/// </summary>
	private int[] __textUneditables;

	/// <summary>
	/// The position in the worksheet of the currently-selected instream flow.
	/// </summary>
	private int __currentInstreamFlowIndex = -1;
	/// <summary>
	/// The position in the worksheet of the last-selected instream flow.
	/// </summary>
	private int __lastInstreamFlowIndex = -1;

	/// <summary>
	/// Stores the index of the record that was selected before the worksheet is sorted,
	/// in order to reselect it after the sort is complete.
	/// </summary>
	private int __sortSelectedRow = -1;

	/// <summary>
	/// GUI Buttons.
	/// </summary>
	private JButton __applyJButton, __cancelJButton, __closeJButton, __findNextInsf, __helpJButton, __waterRightsJButton, __showOnMap_JButton = null, __showOnNetwork_JButton = null;

	/// <summary>
	/// Checkboxes for selecting the time series to view.
	/// </summary>
	private JCheckBox __demandsMonthlyTS, __demandsAveMonthlyTS, __demandsDailyTS, __demandsEstDailyTS;

	/// <summary>
	/// Array of JComponents that should be disabled when nothing is selected from the list.
	/// </summary>
	private JComponent[] __disables;

	/// <summary>
	/// The radio buttons to select the kind of search to do.
	/// </summary>
	private JRadioButton __searchIDJRadioButton, __searchNameJRadioButton;

	/// <summary>
	/// Textfields for displaying the current instream flow's data.
	/// </summary>
	private JTextField __downstreamRiverNode, __instreamFlowStationID, __instreamFlowName, __upstreamRiverNode;

	/// <summary>
	/// Status bar textfields 
	/// </summary>
	private JTextField __messageJTextField, __statusJTextField;

	/// <summary>
	/// Textfields for entering the data on which to search the worksheet.
	/// </summary>
	private JTextField __searchID, __searchName;

	/// <summary>
	/// The worksheet to hold the list of instream flows.
	/// </summary>
	private JWorksheet __worksheet;

	/// <summary>
	/// Buttons to determine how to view the selected time series.
	/// </summary>
	private SimpleJButton __graph_JButton = null, __summary_JButton = null, __table_JButton = null;

	/// <summary>
	/// SimpleJComboBoxes.
	/// </summary>
	private SimpleJComboBox __dataType, __instreamDailyID, __insflowSwitch;

	/// <summary>
	/// The StateMod_DataSet that contains the statemod data.
	/// </summary>
	private StateMod_DataSet __dataset;

	/// <summary>
	/// Data set window manager.
	/// </summary>
	private StateMod_DataSet_WindowManager __dataset_wm;

	/// <summary>
	/// The DataSetComponent containing the instream flow data.
	/// </summary>
	private DataSetComponent __instreamFlowComponent;

	/// <summary>
	/// The list of instream flows to be displayed.
	/// </summary>
	private IList<StateMod_InstreamFlow> __instreamFlowsVector;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="dataset"> the dataset containing the instream flow information </param>
	/// <param name="dataset_wm"> the dataset window manager or null if the data set windows are not being managed. </param>
	/// <param name="editable"> whether the data are editable or not. </param>
	public StateMod_InstreamFlow_JFrame(StateMod_DataSet dataset, StateMod_DataSet_WindowManager dataset_wm, bool editable)
	{
		StateMod_GUIUtil.setTitle(this, dataset, "Instream Flows", null);
		JGUIUtil.setIcon(this, JGUIUtil.getIconImage());

		__dataset = dataset;
		__dataset_wm = dataset_wm;
		__instreamFlowComponent = __dataset.getComponentForComponentType(StateMod_DataSet.COMP_INSTREAM_STATIONS);

		__instreamFlowsVector = (System.Collections.IList)__instreamFlowComponent.getData();
		int size = __instreamFlowsVector.Count;
		StateMod_InstreamFlow isf = null;
		for (int i = 0; i < size; i++)
		{
			isf = (StateMod_InstreamFlow)__instreamFlowsVector[i];
			isf.createBackup();
		}

		__editable = editable;

		setupGUI(0);
	}

	/// <summary>
	/// Constructor. </summary>
	/// <param name="dataset"> the dataset containing the instream flow information </param>
	/// <param name="dataset_wm"> the dataset window manager or null if the data set windows are not being managed. </param>
	/// <param name="instreamFlow"> the instreamFlow to select from the list </param>
	/// <param name="editable"> whether the gui data is editable or not </param>
	public StateMod_InstreamFlow_JFrame(StateMod_DataSet dataset, StateMod_DataSet_WindowManager dataset_wm, StateMod_InstreamFlow instreamFlow, bool editable)
	{
		StateMod_GUIUtil.setTitle(this, dataset, "Instream Flows", null);
		JGUIUtil.setIcon(this, JGUIUtil.getIconImage());
		__dataset = dataset;
		__dataset_wm = dataset_wm;
		__instreamFlowComponent = __dataset.getComponentForComponentType(StateMod_DataSet.COMP_INSTREAM_STATIONS);

		__instreamFlowsVector = (System.Collections.IList)__instreamFlowComponent.getData();
		int size = __instreamFlowsVector.Count;
		StateMod_InstreamFlow isf = null;
		for (int i = 0; i < size; i++)
		{
			isf = (StateMod_InstreamFlow)__instreamFlowsVector[i];
			isf.createBackup();
		}

		string id = instreamFlow.getID();
		int index = StateMod_Util.IndexOf(__instreamFlowsVector, id);

		__editable = editable;

		setupGUI(index);
	}

	/// <summary>
	/// Responds to action performed events. </summary>
	/// <param name="e"> the ActionEvent that happened. </param>
	public virtual void actionPerformed(ActionEvent e)
	{
		string routine = "StateMod_InstreamFlow_JFrame.actionPerformed";

		try
		{
		if (Message.isDebugOn)
		{
			Message.printDebug(1, routine, "In actionPerformed: " + e.getActionCommand());
		}

		string action = e.getActionCommand();
		object source = e.getSource();

		if (source == __findNextInsf)
		{
			searchWorksheet(__worksheet.getSelectedRow() + 1);
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
		else if (source == __searchID || source == __searchName)
		{
			searchWorksheet();
		}
		else if (action.Equals(__BUTTON_HELP))
		{
			// TODO HELP (JTS - 2003-06-09)
		}
		// Time series buttons...
		else if ((source == __graph_JButton) || (source == __table_JButton) || (source == __summary_JButton))
		{
			displayTSViewJFrame(source);
		}
		else if (action.Equals(__BUTTON_CLOSE))
		{
			saveCurrentRecord();
			int size = __instreamFlowsVector.Count;
			StateMod_InstreamFlow isf = null;
			bool changed = false;
			for (int i = 0; i < size; i++)
			{
				isf = __instreamFlowsVector[i];
				if (!changed && isf.changed())
				{
					changed = true;
				}
				isf.acceptChanges();
			}
			if (changed)
			{
				__dataset.setDirty(StateMod_DataSet.COMP_INSTREAM_STATIONS, true);
			}
			if (__dataset_wm != null)
			{
				__dataset_wm.closeWindow(StateMod_DataSet_WindowManager.WINDOW_INSTREAM);
			}
			else
			{
				JGUIUtil.close(this);
			}
		}
		else if (action.Equals(__BUTTON_APPLY))
		{
			saveCurrentRecord();
			int size = __instreamFlowsVector.Count;
			StateMod_InstreamFlow isf = null;
			bool changed = false;
			for (int i = 0; i < size; i++)
			{
				isf = __instreamFlowsVector[i];
				if (!changed && isf.changed())
				{
					changed = true;
				}
				isf.createBackup();
			}
			if (changed)
			{
				__dataset.setDirty(StateMod_DataSet.COMP_INSTREAM_STATIONS, true);
			}
		}
		else if (action.Equals(__BUTTON_CANCEL))
		{
			int size = __instreamFlowsVector.Count;
			StateMod_InstreamFlow isf = null;
			for (int i = 0; i < size; i++)
			{
				isf = __instreamFlowsVector[i];
				isf.restoreOriginal();
			}
			if (__dataset_wm != null)
			{
				__dataset_wm.closeWindow(StateMod_DataSet_WindowManager.WINDOW_INSTREAM);
			}
			else
			{
				JGUIUtil.close(this);
			}
		}
		else if (source == __showOnMap_JButton)
		{
			// The button is only enabled if some spatial data exist, which can be one or more of
			// upstream (this) or downstream location...
			StateMod_InstreamFlow ifs = getSelectedInstreamFlow();
			GRLimits limits = null;
			GeoProjection limitsProjection = null; // for the data limits
			if (ifs is HasGeoRecord)
			{
				GeoRecord geoRecord = ((HasGeoRecord)ifs).getGeoRecord();
				if (geoRecord != null)
				{
					GRShape shapeUpstream = geoRecord.getShape();
					if (shapeUpstream != null)
					{
						limits = new GRLimits(shapeUpstream.xmin, shapeUpstream.ymin, shapeUpstream.xmax, shapeUpstream.ymax);
						limitsProjection = geoRecord.getLayer().getProjection();
					}
				}
			}
			// Extend the limits for the downstream
			StateMod_Data smdata = ifs.lookupDownstreamDataObject(__dataset);
			if ((smdata != null) && (smdata is HasGeoRecord))
			{
				HasGeoRecord hasGeoRecord = (HasGeoRecord)smdata;
				GeoRecord geoRecord = hasGeoRecord.getGeoRecord();
				if (geoRecord != null)
				{
					GRShape shapeDownstream = geoRecord.getShape();
					if (shapeDownstream != null)
					{
						GeoProjection layerProjection = geoRecord.getLayer().getProjection();
						if (limitsProjection == null)
						{
							limitsProjection = layerProjection;
						}
						bool doProject = GeoProjection.needToProject(layerProjection, limitsProjection);
						if (doProject)
						{
							shapeDownstream = GeoProjection.projectShape(layerProjection, limitsProjection, shapeDownstream, false);
						}
						if (limits == null)
						{
							limits = new GRLimits(shapeDownstream.xmin,shapeDownstream.ymin,shapeDownstream.xmax,shapeDownstream.ymax);
						}
						else
						{
							limits.max(shapeDownstream.xmin,shapeDownstream.ymin,shapeDownstream.xmax,shapeDownstream.ymax,true);
						}
					}
				}
			}
			__dataset_wm.showOnMap(ifs, "Instream: " + ifs.getID() + " - " + ifs.getName(), limits, limitsProjection);
		}
		else if (source == __showOnNetwork_JButton)
		{
			StateMod_Network_JFrame networkEditor = __dataset_wm.getNetworkEditor();
			if (networkEditor != null)
			{
				HydrologyNode node = networkEditor.getNetworkJComponent().findNode(getSelectedInstreamFlow().getID(), false, false);
				if (node != null)
				{
					__dataset_wm.showOnNetwork(getSelectedInstreamFlow(), "Instream: " + getSelectedInstreamFlow().getID() + " - " + getSelectedInstreamFlow().getName(), new GRLimits(node.getX(),node.getY(),node.getX(),node.getY()));
				}
			}
		}
		else
		{
			if (__currentInstreamFlowIndex == -1)
			{
				new ResponseJDialog(this, "You must first select an instream flow from the list.", ResponseJDialog.OK);
				return;
			}

			// set placeholder to current instream flow
			StateMod_InstreamFlow insf = __instreamFlowsVector[__currentInstreamFlowIndex];

			if (source == __waterRightsJButton)
			{
				new StateMod_InstreamFlow_Right_JFrame(__dataset, insf, __editable);
			}
		}
		}
		catch (Exception ex)
		{
			Message.printWarning(2, routine, "Error in action performed");
			Message.printWarning(2, routine, ex);
		}
	}

	/// <summary>
	/// Checks the text fields for validity before they are saved back into the data object. </summary>
	/// <returns> true if the text fields are okay, false if not. </returns>
	private bool checkInput()
	{
		System.Collections.IList errors = new List<object>();
		int errorCount = 0;

		// for each field, check if it contains valid input.  If not,
		// create a string of the format "fieldname -- reason why it
		// is not correct" and add it to the errors vector.  also increment error count

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
	/// Checks the states of the time series checkboxes and enables/disables the time series buttons appropriately.
	/// </summary>
	private void checkTimeSeriesButtonsStates()
	{
		bool enabled = false;

		if ((__demandsMonthlyTS.isSelected() && __demandsMonthlyTS.isEnabled()) || (__demandsAveMonthlyTS.isSelected() && __demandsAveMonthlyTS.isEnabled()) || (__demandsDailyTS.isSelected() && __demandsDailyTS.isEnabled()) || (__demandsEstDailyTS.isSelected() && __demandsDailyTS.isEnabled()))
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
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		string routine = this.GetType().FullName + ".checkViewButtonState";
		StateMod_InstreamFlow ifs = getSelectedInstreamFlow();
		StateMod_Data smdataDown = null;
		bool mapEnabled = false;
		try
		{
			smdataDown = ifs.lookupDownstreamDataObject(__dataset);
		}
		catch (Exception e)
		{
			Message.printWarning(3, routine, e);
			__showOnMap_JButton.setEnabled(false);
			__showOnNetwork_JButton.setEnabled(false);
			return;
		}
		if (ifs is HasGeoRecord)
		{
			HasGeoRecord hasGeoRecord = (HasGeoRecord)ifs;
			if (hasGeoRecord.getGeoRecord() != null)
			{
				mapEnabled = true;
			}
		}
		if (smdataDown is HasGeoRecord)
		{
			HasGeoRecord hasGeoRecord = (HasGeoRecord)smdataDown;
			if (hasGeoRecord.getGeoRecord() != null)
			{
				mapEnabled = true;
			}
		}
		__showOnMap_JButton.setEnabled(mapEnabled);
		__showOnNetwork_JButton.setEnabled(true);
	}

	/// <summary>
	/// Display the time series. </summary>
	/// <param name="action"> Event action that initiated the display. </param>
	private void displayTSViewJFrame(object o)
	{
		string routine = "displayTSViewJFrame";

		// Initialize the display...

		PropList display_props = new PropList("Reservoir");
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

		StateMod_InstreamFlow insf = (StateMod_InstreamFlow)__instreamFlowsVector[__currentInstreamFlowIndex];

		// display_props.set("HelpKey", "TSTool.ExportMenu");
		display_props.set("TSViewTitleString", StateMod_Util.createDataLabel(insf,true) + " Time Series");
		display_props.set("DisplayFont", "Courier");
		display_props.set("DisplaySize", "11");
		display_props.set("PrintFont", "Courier");
		display_props.set("PrintSize", "7");
		display_props.set("PageLength", "100");

		PropList props = new PropList("Instream Flow");
		props.set("Product.TotalWidth", "600");
		props.set("Product.TotalHeight", "400");

		System.Collections.IList tslist = new List<object>();

		int sub = 0;
		int its = 0;
		TS ts = null;

		if ((__demandsMonthlyTS.isSelected() && (insf.getDemandMonthTS() != null)))
		{
			// Do the monthly graph...
			++sub;
			its = 0;
			props.set("SubProduct " + sub + ".GraphType=Line");
			props.set("SubProduct " + sub + ".SubTitleString=Monthly Data for Instream Flow " + insf.getID() + " (" + insf.getName() + ")");
			props.set("SubProduct " + sub + ".SubTitleFontSize=12");
			ts = insf.getDemandMonthTS();
			if (ts != null)
			{
				props.set("Data " + sub + "." + (++its) + ".TSID=" + ts.getIdentifierString());
				tslist.Add(ts);
			}
		}

		if ((__demandsAveMonthlyTS.isSelected() && (insf.getDemandAverageMonthTS() != null)))
		{
			// Do the monthly average graph...
			++sub;
			its = 0;
			props.set("SubProduct " + sub + ".GraphType=Line");
			props.set("SubProduct " + sub + ".SubTitleString=Monthly Average Data for Instream Flow " + insf.getID() + " (" + insf.getName() + ")");
			props.set("SubProduct " + sub + ".SubTitleFontSize=12");
			ts = insf.getDemandAverageMonthTS();
			if (ts != null)
			{
				props.set("Data " + sub + "." + (++its) + ".TSID=" + ts.getIdentifierString());
				tslist.Add(ts);
			}
		}

		// TODO - need to add estimated daily demands...
		if ((__demandsDailyTS.isSelected() && (insf.getDemandDayTS() != null)))
		{
			// Do the daily graph...
			++sub;
			its = 0;
			props.set("SubProduct " + sub + ".GraphType=Line");
			props.set("SubProduct " + sub + ".SubTitleString=Daily Data for Instream Flow " + insf.getID() + " (" + insf.getName() + ")");
			props.set("SubProduct " + sub + ".SubTitleFontSize=12");
			ts = insf.getDemandDayTS();
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
			Message.printWarning(1,routine,"Error displaying time series (" + e + ").");
			Message.printWarning(2, routine, e);
		}
	}

	/// <summary>
	/// Clean up before garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~StateMod_InstreamFlow_JFrame()
	{
		__searchCriteriaGroup = null;
		__applyJButton = null;
		__cancelJButton = null;
		__graph_JButton = null;
		__summary_JButton = null;
		__table_JButton = null;
		__closeJButton = null;
		__demandsMonthlyTS = null;
		__demandsAveMonthlyTS = null;
		__findNextInsf = null;
		__helpJButton = null;
		__demandsDailyTS = null;
		__demandsEstDailyTS = null;
		__waterRightsJButton = null;
		__searchIDJRadioButton = null;
		__searchNameJRadioButton = null;
		__downstreamRiverNode = null;
		__instreamDailyID = null;
		__instreamFlowStationID = null;
		__instreamFlowName = null;
		__upstreamRiverNode = null;
		__searchID = null;
		__searchName = null;
		__worksheet = null;
		__insflowSwitch = null;
		__instreamFlowsVector = null;

//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Get the selected instream flow, based on the current index in the list.
	/// </summary>
	private StateMod_InstreamFlow getSelectedInstreamFlow()
	{
		return __instreamFlowsVector[__currentInstreamFlowIndex];
	}

	/// <summary>
	/// Initializes the arrays that are used when items are selected and deselected.
	/// This should be called from setupGUI() before the a call is made to selectTableIndex().
	/// </summary>
	private void initializeDisables()
	{
		__disables = new JComponent[16];
		int i = 0;
		__disables[i++] = __instreamFlowStationID;
		__disables[i++] = __applyJButton;
		__disables[i++] = __demandsMonthlyTS;
		__disables[i++] = __demandsAveMonthlyTS;
		__disables[i++] = __demandsDailyTS;
		__disables[i++] = __demandsEstDailyTS;
		__disables[i++] = __waterRightsJButton;
		__disables[i++] = __insflowSwitch;
		__disables[i++] = __downstreamRiverNode;
		__disables[i++] = __instreamDailyID;
		__disables[i++] = __instreamFlowName;
		__disables[i++] = __upstreamRiverNode;
		__disables[i++] = __table_JButton;
		__disables[i++] = __graph_JButton;
		__disables[i++] = __summary_JButton;
		__disables[i++] = __dataType;

		__textUneditables = new int[1];
		__textUneditables[0] = 0;
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
	/// Populates the diversion daily id combo box.
	/// </summary>
	private void populateInstreamDailyID()
	{
		__instreamDailyID.removeAllItems();

		__instreamDailyID.add("0 - Use average daily value from monthly time series");
		__instreamDailyID.add("3 - Daily time series are supplied");
		__instreamDailyID.add("4 - Daily time series interpolated from midpoints of monthly data");

		IList<string> idNameVector = StateMod_Util.createIdentifierListFromStateModData(__instreamFlowsVector, true, null);
		int size = idNameVector.Count;

		string s = null;
		for (int i = 0; i < size; i++)
		{
			s = idNameVector[i];
			__instreamDailyID.add(s.Trim());
		}
	}

	/// <summary>
	/// Processes a table selection (either via a mouse press or programmatically 
	/// from selectTableIndex() by writing the old data back to the data set component
	/// and getting the next selection's data out of the data and displaying it on the form. </summary>
	/// <param name="index"> the index of the instream flow to display on the form. </param>
	private void processTableSelection(int index)
	{
		string routine = "StateMod_InstreamFlow_JFrame.processTableSelection";
		__lastInstreamFlowIndex = __currentInstreamFlowIndex;
		__currentInstreamFlowIndex = __worksheet.getOriginalRowNumber(index);

		saveLastRecord();

		if (__worksheet.getSelectedRow() == -1)
		{
			JGUIUtil.disableComponents(__disables, true);
			return;
		}

		JGUIUtil.enableComponents(__disables, __textUneditables, __editable);
		checkTimeSeriesButtonsStates();

		StateMod_InstreamFlow insf = (StateMod_InstreamFlow)__instreamFlowsVector[__currentInstreamFlowIndex];

		__instreamFlowStationID.setText(insf.getID());
		__instreamFlowName.setText(insf.getName());
		__upstreamRiverNode.setText(insf.getCgoto());
		__downstreamRiverNode.setText(insf.getIfrrdn());

		// For checkboxes, do not change the state of the checkbox, only
		// whether enabled - that way if the user has picked a combination of
		// parameters it is easy for them to keep the same settings when
		// switching between stations.  Make sure to do the following after
		// the generic enable/disable code is called above!

		if (insf.getDemandMonthTS() != null)
		{
			__demandsMonthlyTS.setEnabled(true);
		}
		else
		{
			__demandsMonthlyTS.setEnabled(false);
		}

		if (insf.getDemandAverageMonthTS() != null)
		{
			__demandsAveMonthlyTS.setEnabled(true);
		}
		else
		{
			__demandsAveMonthlyTS.setEnabled(false);
		}

		if (insf.getDemandDayTS() != null)
		{
			__demandsDailyTS.setEnabled(true);
		}
		else
		{
			__demandsDailyTS.setEnabled(false);
		}

		// TODO - need to enable pattern logic based on daily ID.

		__demandsEstDailyTS.setEnabled(false);

		// switch
		if (insf.getSwitch() == 1)
		{
			__insflowSwitch.select("1 - On");
		}
		else
		{
			__insflowSwitch.select("0 - Off");
		}

		if (__lastInstreamFlowIndex != -1)
		{
			string s = __instreamDailyID.getStringAt(3);
			__instreamDailyID.removeAt(3);
			__instreamDailyID.addAlpha(s, 3);
		}
		string s = "" + insf.getID() + " - " + insf.getName();
		__instreamDailyID.remove(s);
		__instreamDailyID.addAt(s, 3);

		string c = insf.getCifridy();
		if (c.Trim().Equals(""))
		{
			if (!__instreamDailyID.setSelectedPrefixItem(insf.getID()))
			{
				Message.printWarning(2, routine, "No cifridy value matching '" + insf.getID() + "' found in combo box.");
				__instreamDailyID.select(0);
			}
			setOriginalCifridy(insf, "" + insf.getID());
		}
		else
		{
			if (!__instreamDailyID.setSelectedPrefixItem(c))
			{
				Message.printWarning(2, routine, "No cifridy value matching '" + insf.getID() + "' found in combo box.");
				__instreamDailyID.select(0);
			}
		}

		int iifcom = insf.getIifcom();
		__dataType.select(iifcom);

		checkViewButtonState();
	}

	/// <summary>
	/// Saves the prior record selected in the table; called when moving to a new record by a table selection.
	/// </summary>
	private void saveLastRecord()
	{
		saveInformation(__lastInstreamFlowIndex);
	}

	/// <summary>
	/// Saves the current record selected in the table; called when the window is closed
	/// or minimized or apply is pressed.
	/// </summary>
	private void saveCurrentRecord()
	{
		saveInformation(__currentInstreamFlowIndex);
	}

	/// <summary>
	/// Saves the information associated with the currently-selected instream flow.
	/// The user doesn't need to hit the return key for the gui to recognize changes.
	/// The info is saved each time the user selects a different station or pressed the close button.
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

		// set placeholder to last instream flow
		StateMod_InstreamFlow insf = (StateMod_InstreamFlow)__instreamFlowsVector[record];

		insf.setID(__instreamFlowStationID.getText().Trim());
		insf.setName(__instreamFlowName.getText().Trim());
		insf.setCgoto(__upstreamRiverNode.getText().Trim());
		insf.setIfrrdn(__downstreamRiverNode.getText().Trim());
		insf.setSwitch(__insflowSwitch.getSelectedIndex());

		string cifridy = __instreamDailyID.getSelected();
		int index = cifridy.IndexOf(" - ", StringComparison.Ordinal);
		if (index > -1)
		{
			cifridy = cifridy.Substring(0, index);
		}
		cifridy = cifridy.Trim();
		if (!cifridy.Equals(insf.getCifridy(), StringComparison.OrdinalIgnoreCase))
		{
			insf.setCifridy(cifridy);
			/*
			TODO TS (JTS - 2003-06-19)
			connect the daily ts to the linked list
			insf.connectDailyDemandTS(__dailyInsfTSVector);
			*/
		}

		insf.setIifcom(__dataType.getSelectedIndex());
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
		StateMod_InstreamFlow isf = null;
		for (int i = 0; i < rows; i++)
		{
			isf = (StateMod_InstreamFlow)__worksheet.getRowData(i);
			if (isf.getID().Trim().Equals(id.Trim()))
			{
				selectTableIndex(i);
				return;
			}
		}
	}

	/// <summary>
	/// Selects the desired index in the and also displays the appropriate data in the remainder of the window. </summary>
	/// <param name="index"> the index to select. </param>
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
	/// Sets up the gui. </summary>
	/// <param name="index"> the index of the flow to be initially selected </param>
	private void setupGUI(int index)
	{
		string routine = "setupGUI";

		addWindowListener(this);

		JPanel p3 = new JPanel(); // div sta id -> switch for diversion

		__instreamFlowStationID = new JTextField(12);
		__instreamFlowName = new JTextField(24);
		__upstreamRiverNode = new JTextField(12);
		__downstreamRiverNode = new JTextField(12);

		__instreamDailyID = new SimpleJComboBox();
		__insflowSwitch = new SimpleJComboBox();
		__insflowSwitch.add("0 - Off");
		__insflowSwitch.add("1 - On");

		__searchID = new JTextField(15);
		__searchName = new JTextField(15);
		__findNextInsf = new JButton(__BUTTON_FIND_NEXT);
		__searchCriteriaGroup = new ButtonGroup();
		__searchIDJRadioButton = new JRadioButton("ID", true);
		__searchCriteriaGroup.add(__searchIDJRadioButton);
		__searchIDJRadioButton.addActionListener(this);
		__searchNameJRadioButton = new JRadioButton("Name", false);
		__searchCriteriaGroup.add(__searchNameJRadioButton);
		__searchNameJRadioButton.addActionListener(this);

		__waterRightsJButton = new JButton(__BUTTON_WATER_RIGHTS);
		__demandsMonthlyTS = new JCheckBox("Demands (Monthly)");
		__demandsMonthlyTS.addItemListener(this);
		__demandsAveMonthlyTS = new JCheckBox("Demands (Average Monthly)");
		__demandsAveMonthlyTS.addItemListener(this);
		__demandsDailyTS = new JCheckBox("Demands (Daily)");
		__demandsDailyTS.addItemListener(this);
		__demandsEstDailyTS = new JCheckBox("Demands (Estimated Daily)");
		__demandsEstDailyTS.addItemListener(this);

		__dataType = new SimpleJComboBox();
		__dataType.add("0 - Average Monthly");
		__dataType.add("1 - Monthly");
		__dataType.add("2 - Average Monthly");

		if (!__dataset.getComponentForComponentType(StateMod_DataSet.COMP_INSTREAM_DEMAND_TS_MONTHLY).hasData())
		{
			__demandsMonthlyTS.setEnabled(false);
		}
		if (!__dataset.getComponentForComponentType(StateMod_DataSet.COMP_INSTREAM_DEMAND_TS_AVERAGE_MONTHLY).hasData())
		{
			__demandsAveMonthlyTS.setEnabled(false);
		}
		if (!__dataset.getComponentForComponentType(StateMod_DataSet.COMP_INSTREAM_DEMAND_TS_DAILY).hasData())
		{
			__demandsMonthlyTS.setEnabled(false);
		}
		__demandsEstDailyTS.setEnabled(false);

		__showOnMap_JButton = new SimpleJButton(__BUTTON_SHOW_ON_MAP, this);
		__showOnMap_JButton.setToolTipText("Annotate map with location (button is disabled if layer does not have matching ID)");
		__showOnNetwork_JButton = new SimpleJButton(__BUTTON_SHOW_ON_NETWORK, this);
		__showOnNetwork_JButton.setToolTipText("Annotate network with location");
		__helpJButton = new JButton(__BUTTON_HELP);
		__helpJButton.setEnabled(false);
		__closeJButton = new JButton(__BUTTON_CLOSE);
		__applyJButton = new JButton(__BUTTON_APPLY);
		__cancelJButton = new JButton(__BUTTON_CANCEL);

		GridBagLayout gb = new GridBagLayout();
		FlowLayout fl = new FlowLayout(FlowLayout.RIGHT);
		JPanel mainPanel = new JPanel();
		mainPanel.setLayout(gb);
		p3.setLayout(gb);

		int y;

		PropList p = new PropList("StateMod_InstreamFlow_JFrame.JWorksheet");
		p.add("JWorksheet.ShowPopupMenu=true");
		p.add("JWorksheet.AllowCopy=true");
		p.add("JWorksheet.SelectionMode=SingleRowSelection");

		int[] widths = null;
		JScrollWorksheet jsw = null;
		try
		{
			StateMod_InstreamFlow_TableModel tmi = new StateMod_InstreamFlow_TableModel(__instreamFlowsVector, __editable);
			StateMod_InstreamFlow_CellRenderer cri = new StateMod_InstreamFlow_CellRenderer(tmi);

			jsw = new JScrollWorksheet(cri, tmi, p);
			__worksheet = jsw.getJWorksheet();

			widths = cri.getColumnWidths();
		}
		catch (Exception e)
		{
			Message.printWarning(2, routine, e);
			jsw = new JScrollWorksheet(0, 0, p);
			__worksheet = jsw.getJWorksheet();
		}
		__worksheet.setPreferredScrollableViewportSize(null);
		__worksheet.setHourglassJFrame(this);
		__worksheet.addMouseListener(this);
		__worksheet.addKeyListener(this);

		// add top labels and text areas to panels
		y = 0;
		JGUIUtil.addComponent(p3, new JLabel("Station ID:"), 0, y, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(p3, __instreamFlowStationID, 1, y, 1, 1, 0, 0, 1, 0, 0, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);
		__instreamFlowStationID.addActionListener(this);
		__instreamFlowStationID.setEditable(false);
		y++;
		JGUIUtil.addComponent(p3, new JLabel("Name:"), 0, y, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(p3, __instreamFlowName, 1, y, 1, 1, 0, 0, 1, 0, 0, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);
		__instreamFlowName.addActionListener(this);
		y++;
		JGUIUtil.addComponent(p3, new JLabel("Daily Data ID:"), 0, y, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(p3, __instreamDailyID, 1, y, 1, 1, 0, 0, 1, 0, 0, 1, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);
		y++;
		JGUIUtil.addComponent(p3, new JLabel("Upstream river node:"), 0, y, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(p3, __upstreamRiverNode, 1, y, 1, 1, 0, 0, 1, 0, 0, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);
		__upstreamRiverNode.addActionListener(this);
		y++;
		JGUIUtil.addComponent(p3, new JLabel("Downstream river node:"), 0, y, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(p3, __downstreamRiverNode, 1, y, 1, 1, 0, 0, 1, 0, 0, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);
		__downstreamRiverNode.addActionListener(this);
		y++;
		JGUIUtil.addComponent(p3, new JLabel("On/Off Switch:"), 0, y, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(p3, __insflowSwitch, 1, y, 1, 1, 0, 0, 1, 0, 0, 1, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);
		y++;
		JGUIUtil.addComponent(p3, new JLabel("Data Type:"), 0, y, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(p3, __dataType, 1, y, 1, 1, 0, 0, 1, 0, 0, 1, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);

		JPanel subformsPanel = new JPanel();
		subformsPanel.setLayout(gb);
		subformsPanel.setBorder(BorderFactory.createTitledBorder("Related Data"));

		JGUIUtil.addComponent(subformsPanel, __waterRightsJButton, 1, 0, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.NORTHWEST);

		JPanel tsPanel = new JPanel();
		tsPanel.setLayout(gb);
		tsPanel.setBorder(BorderFactory.createTitledBorder("Time Series"));

		y++;

		int y3 = 0;
		JGUIUtil.addComponent(tsPanel, __demandsMonthlyTS, 0, y3, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(tsPanel, __demandsAveMonthlyTS, 0, ++y3, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(tsPanel, __demandsDailyTS, 0, ++y3, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(tsPanel, __demandsEstDailyTS, 0, ++y3, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		// Buttons for the time series...
		JPanel tsb_JPanel = new JPanel();
		tsb_JPanel.setLayout(new FlowLayout());
		__graph_JButton = new SimpleJButton("Graph", this);
		tsb_JPanel.add(__graph_JButton);
		__table_JButton = new SimpleJButton("Table", this);
		tsb_JPanel.add(__table_JButton);
		__summary_JButton = new SimpleJButton("Summary", this);
		tsb_JPanel.add(__summary_JButton);

		JGUIUtil.addComponent(tsPanel, tsb_JPanel, 0, ++y3, 1, 1, 1, 1, 1, 0, 0, 1, GridBagConstraints.NONE, GridBagConstraints.CENTER);

		__waterRightsJButton.addActionListener(this);

		// add search areas
		JPanel searchPanel = new JPanel();
		searchPanel.setLayout(gb);
		searchPanel.setBorder(BorderFactory.createTitledBorder("Search above list for:"));

		int y2 = 0;
		JGUIUtil.addComponent(searchPanel, __searchIDJRadioButton, 0, y2, 1, 1, 0, 0, 5, 10, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(searchPanel, __searchID, 1, y2, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		__searchID.addActionListener(this);
		y2++;
		JGUIUtil.addComponent(searchPanel, __searchNameJRadioButton, 0, y2, 1, 1, 0, 0, 5, 10, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(searchPanel, __searchName, 1, y2, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		__searchName.addActionListener(this);
		__searchName.setEditable(false);
		y2++;
		JGUIUtil.addComponent(searchPanel, __findNextInsf, 0, y2, 4, 1, 0, 0, 20, 10, 20, 10, GridBagConstraints.NONE, GridBagConstraints.WEST);
		__findNextInsf.addActionListener(this);

		// add the help and close buttons
		JPanel pfinal = new JPanel();
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
		__closeJButton.addActionListener(this);
		__helpJButton.addActionListener(this);

		// add all the panels to the main form
		JGUIUtil.addComponent(mainPanel, jsw, 0, 0, 1, 1, 1, 1, 10, 10, 0, 0, GridBagConstraints.BOTH, GridBagConstraints.CENTER);
		JGUIUtil.addComponent(mainPanel, searchPanel, 0, 1, 1, 1, 0, 0, 10, 10, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);

		JPanel right = new JPanel();
		right.setLayout(gb);
		JGUIUtil.addComponent(right, p3, 0, 0, 2, 1, 0, 0, 10, 10, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(right, tsPanel, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.NORTHWEST);
		JGUIUtil.addComponent(right, subformsPanel, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.NORTHWEST);

		JGUIUtil.addComponent(mainPanel, right, 1, 0, 1, 2, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.NORTHWEST);
	/*
		JGUIUtil.addComponent(mainPanel, pfinal,
			1, 2, 1, 1, 0, 0,
			0, 0, 0, 0,
			GridBagConstraints.NONE, GridBagConstraints.SOUTHWEST);
	*/
		getContentPane().add(mainPanel);

		JPanel bottomJPanel = new JPanel();
		bottomJPanel.setLayout(gb);
		__messageJTextField = new JTextField();
		__messageJTextField.setEditable(false);
		JGUIUtil.addComponent(bottomJPanel, pfinal, 0, 0, 8, 1, 1, 1, GridBagConstraints.HORIZONTAL, GridBagConstraints.EAST);
		JGUIUtil.addComponent(bottomJPanel, __messageJTextField, 0, 1, 7, 1, 1.0, 0.0, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);
		__statusJTextField = new JTextField(5);
		__statusJTextField.setEditable(false);
		JGUIUtil.addComponent(bottomJPanel, __statusJTextField, 7, 1, 1, 1, 0.0, 0.0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		getContentPane().add("South", bottomJPanel);

		initializeDisables();

		if (__dataset_wm != null)
		{
			__dataset_wm.setWindowOpen(StateMod_DataSet_WindowManager.WINDOW_INSTREAM, this);
		}

		// must be called before the first call to selectTableIndex()
		populateInstreamDailyID();

		pack();
		setSize(835,500);
		JGUIUtil.center(this);
		selectTableIndex(index);
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
	/// Responds to Window closing events; closes the window and marks it closed. </summary>
	/// <param name="e"> the WindowEvent that happened. </param>
	public virtual void windowClosing(WindowEvent e)
	{
		saveCurrentRecord();
		int size = __instreamFlowsVector.Count;
		StateMod_InstreamFlow isf = null;
		bool changed = false;
		for (int i = 0; i < size; i++)
		{
			isf = (StateMod_InstreamFlow)__instreamFlowsVector[i];
			if (!changed && isf.changed())
			{
				changed = true;
			}
			isf.acceptChanges();
		}
		if (changed)
		{
			__dataset.setDirty(StateMod_DataSet.COMP_INSTREAM_STATIONS, true);
		}
		if (__dataset_wm != null)
		{
			__dataset_wm.closeWindow(StateMod_DataSet_WindowManager.WINDOW_INSTREAM);
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

	private void setOriginalCifridy(StateMod_InstreamFlow i, string cifridy)
	{
		((StateMod_InstreamFlow)i._original)._cifridy = cifridy;
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