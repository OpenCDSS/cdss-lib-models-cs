﻿using System;
using System.Collections.Generic;

// StateMod_DataSet_WindowManager - class to manage data set windows

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
// StateMod_DataSet_WindowManager - class to manage data set windows
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
//
// 2003-08-26	Steven A. Malers, RTi	Split relevant code out of
//					StateMod_GUIUtil and the main
//					StateModGUI_JFrame classes.
//					StateMod_DataSet.
// 2003-08-27	J. Thomas Sapienza, RTi	* Added isWindowOpen().
//					* Added setDataSet().
// 2003-08-27	SAM, RTi		Change so methods that open windows
//					return the JFrame to simplify later
//					use.
// 2003-08-29	SAM, RTi		Separate delay table groups into
//					monthly and daily.
// 2003-09-10	JTS, RTi		Enabled Response Window.
// 2003-09-11	SAM, RTi		Updated based on changes in the names
//					of the river stations.
// 2003-09-23	JTS, RTi		GUI constructors changed because of
//					the use of the new StateMod_GUIUtil
//					title-setting code.
// 2003-10-14	SAM, RTi		Add updateWindowStatus() to allow
//					edits in a window to result in a
//					change in the main GUI state.
// 2004-07-15	JTS, RTi		Added closeAllWindows().
// 2004-08-25	JTS, RTi		Data set summary uses 8 point fonts now.
// 2004-10-25	SAM, RTi		Add query tool window.
// 2006-01-18	JTS, RTi		Initial Evaporation view now a table.
// 2006-08-22	SAM, RTi		Add WINDOW_PLAN for plans.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
//------------------------------------------------------------------------------
// EndHeader

namespace DWR.StateMod
{

	using HydrologyNode = cdss.domain.hydrology.network.HydrologyNode;

	using StateCU_DataSet = DWR.StateCU.StateCU_DataSet;
	using StateCU_Location_JFrame = DWR.StateCU.StateCU_Location_JFrame;
	using GeoProjection = RTi.GIS.GeoView.GeoProjection;
	using GeoRecord = RTi.GIS.GeoView.GeoRecord;
	using GeoViewAnnotationRenderer = RTi.GIS.GeoView.GeoViewAnnotationRenderer;
	using GeoViewJComponent = RTi.GIS.GeoView.GeoViewJComponent;
	using GeoViewJPanel = RTi.GIS.GeoView.GeoViewJPanel;
	using HasGeoRecord = RTi.GIS.GeoView.HasGeoRecord;
	using GRColor = RTi.GR.GRColor;
	using GRDrawingArea = RTi.GR.GRDrawingArea;
	using GRDrawingAreaUtil = RTi.GR.GRDrawingAreaUtil;
	using GRLimits = RTi.GR.GRLimits;
	using GRPoint = RTi.GR.GRPoint;
	using GRSymbol = RTi.GR.GRSymbol;
	using GRText = RTi.GR.GRText;
	using TSViewJFrame = RTi.GRTS.TSViewJFrame;
	using TS = RTi.TS.TS;
	using ReportJFrame = RTi.Util.GUI.ReportJFrame;
	using PropList = RTi.Util.IO.PropList;
	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// <para>
	/// The StateMod_DataSet_WindowManager class opens/manages/closes display windows
	/// for StateMod data set component data.  Currently, only the main windows
	/// (e.g., Diversion, Reservoir, but not the secondary windows) are managed and
	/// only one of each main window is allowed to be open at a time.  In the future,
	/// secondary windows may also be managed more closely but currently more than one
	/// copy of secondary windows can be opened at the same time.
	/// </para>
	/// <para>
	/// An instance of the window manager also helps with window coordination, such as initiating a request
	/// to display (annotate) a data object on the map or network.
	/// </para>
	/// </summary>
	public class StateMod_DataSet_WindowManager : WindowListener, GeoViewAnnotationRenderer, StateMod_Network_AnnotationRenderer
	{

	/// <summary>
	/// Window status settings.  See setWindowOpen(), etc.
	/// </summary>
	private readonly int CLOSED = 0, OPEN = 1;
		// INVISIBLE - might be needed if we decide to not fully destroy
		// windows - that is why an integer is tracked (not a boolean)

	/// <summary>
	/// Windows numbers, managed by the StateMod_GUI - list in the order of the data
	/// set components, although there is not a one to one correspondence.
	/// Note that the network editor is not managed by this component and must be opened/closed separately.
	/// </summary>
	public const int WINDOW_MAIN = 0, WINDOW_CONTROL = 1, WINDOW_OUTPUT_CONTROL = 2, WINDOW_RESPONSE = 3, WINDOW_STREAMGAGE = 4, WINDOW_DELAY_TABLE_MONTHLY = 5, WINDOW_DELAY_TABLE_DAILY = 6, WINDOW_DIVERSION = 7, WINDOW_PRECIPITATION = 8, WINDOW_EVAPORATION = 9, WINDOW_RESERVOIR = 10, WINDOW_INSTREAM = 11, WINDOW_WELL = 12, WINDOW_PLAN = 13, WINDOW_STREAMESTIMATE = 14, WINDOW_RIVER_NETWORK = 15, WINDOW_OPERATIONAL_RIGHT = 16, WINDOW_ADD_NODE = 17, WINDOW_DELETE_NODE = 18, WINDOW_DATASET_SUMMARY = 19, WINDOW_RUN_REPORT = 20, WINDOW_GRAPHING_TOOL = 21, WINDOW_RUN_DELPLT = 22, WINDOW_QUERY_TOOL = 23, WINDOW_CONSUMPTIVE_USE = 24; // Consumptive use (StateCU structure)

	/// <summary>
	/// The number of windows handled by the methods in this class (one more than the last value above).
	/// </summary>
	private readonly int __NUM_WINDOWS = 25;

	/// <summary>
	/// Array to keep track of window status (OPEN or CLOSED)
	/// </summary>
	private int[] __windowStatus;

	/// <summary>
	/// Array of all the windows that are open.
	/// </summary>
	private JFrame[] __windows;

	/// <summary>
	/// StateMod data set for which windows are being managed.
	/// </summary>
	private StateMod_DataSet __dataset = null;

	/// <summary>
	/// StateCU data set for which windows are being managed (to handle structure file).
	/// </summary>
	private StateCU_DataSet __datasetStateCU = null;

	/// <summary>
	/// Map panel, needed for interaction between editor windows and the map.
	/// </summary>
	private GeoViewJPanel __mapPanel = null;

	/// <summary>
	/// Network editor window, needed for interaction between editor windows and the network.
	/// </summary>
	private StateMod_Network_JFrame __networkEditor = null;

	/// <summary>
	/// Constructor.
	/// </summary>
	public StateMod_DataSet_WindowManager() : this(null, null)
	{
	}

	/// <summary>
	/// Constructor.
	/// </summary>
	public StateMod_DataSet_WindowManager(StateMod_DataSet dataset) : this(dataset, null)
	{
	}

	/// <summary>
	/// Constructor.
	/// </summary>
	public StateMod_DataSet_WindowManager(StateMod_DataSet dataset, StateCU_DataSet datasetStateCU)
	{
		__dataset = dataset;
		__datasetStateCU = datasetStateCU;
		__windowStatus = new int[__NUM_WINDOWS];
		for (int i = 0; i < __NUM_WINDOWS; i++)
		{
			__windowStatus[i] = CLOSED;
		}

		__windows = new JFrame[__NUM_WINDOWS];
		for (int i = 0; i < __NUM_WINDOWS; i++)
		{
			__windows[i] = null;
		}
	}

	/// <summary>
	/// Closes all windows, except the main window.
	/// </summary>
	public virtual void closeAllWindows()
	{
		closeWindow(WINDOW_CONTROL);
		closeWindow(WINDOW_OUTPUT_CONTROL);
		closeWindow(WINDOW_RESPONSE);
		closeWindow(WINDOW_STREAMGAGE);
		closeWindow(WINDOW_DELAY_TABLE_MONTHLY);
		closeWindow(WINDOW_DELAY_TABLE_DAILY);
		closeWindow(WINDOW_DIVERSION);
		closeWindow(WINDOW_PRECIPITATION);
		closeWindow(WINDOW_EVAPORATION);
		closeWindow(WINDOW_RESERVOIR);
		closeWindow(WINDOW_INSTREAM);
		closeWindow(WINDOW_WELL);
		closeWindow(WINDOW_PLAN);
		closeWindow(WINDOW_STREAMESTIMATE);
		closeWindow(WINDOW_RIVER_NETWORK);
		closeWindow(WINDOW_OPERATIONAL_RIGHT);
		closeWindow(WINDOW_ADD_NODE);
		closeWindow(WINDOW_DELETE_NODE);
		closeWindow(WINDOW_DATASET_SUMMARY);
		closeWindow(WINDOW_RUN_REPORT);
		closeWindow(WINDOW_GRAPHING_TOOL);
		closeWindow(WINDOW_RUN_DELPLT);
		closeWindow(WINDOW_QUERY_TOOL);
		closeWindow(WINDOW_CONSUMPTIVE_USE);
	}

	/// <summary>
	/// Close the window and set its reference to null.  If the window was never opened,
	/// then no action is taken.  Use setWindowOpen() when opening a window to allow
	/// the management of windows to occur. </summary>
	/// <param name="win_type"> the number of the window. </param>
	public virtual void closeWindow(int win_type)
	{
		if (getWindowStatus(win_type) == CLOSED)
		{
			// No need to do anything...
			return;
		}
		// Get the window...
		JFrame window = getWindow(win_type);
		// Now close the window...
		window.setVisible(false);
		window.dispose();
		// Set the "soft" data...
		setWindowStatus(win_type, CLOSED);
		setWindow(win_type, null);
		Message.printStatus(2, "closeWindow", "Window closed: " + win_type);
	}

	/// <summary>
	/// Format a summary report about the data set and display in a report window. </summary>
	/// <param name="dataset"> StateMod_DataSet to display. </param>
	private JFrame displayDataSetSummary(StateMod_DataSet dataset)
	{
		if (dataset == null)
		{
			// Should not happen because menu should be disabled if no data set...
			return null;
		}
		PropList props = new PropList("Data Set Summary");
		props.add("Title=" + dataset.getBaseName() + " - Data Set Summary");
		props.add("PrintSize=8");
		JFrame f = new ReportJFrame(dataset.getSummary(), props);
		setWindow(WINDOW_DATASET_SUMMARY, f);
		// Because the window is a simple ReportJFrame, it will not know how
		// to set its "close" status in this window manager, so need to listen
		// for a close event here...
		f.addWindowListener(this);
		return f;
	}

	/// <summary>
	/// Show the indicated window type and allow editing.  If that window type is
	/// already displayed, bring it to the front.  Otherwise create the window. </summary>
	/// <param name="window_type"> See WINDOW_*. </param>
	/// <returns> the window that is displayed. </returns>
	public virtual JFrame displayWindow(int window_type)
	{
		return displayWindow(window_type, true);
	}

	// TODO - how to deal with printStatus and other calls that need the
	// StateModGUI_JFrame - for now comment out the calls.
	/// <summary>
	/// Show the indicated window type.  If that window type is already displayed,
	/// bring it to the front.  Otherwise create the window. </summary>
	/// <param name="window_type"> See WINDOW_*. </param>
	/// <param name="editable"> Indicates if the data in the window should be editable. </param>
	/// <returns> the window that is displayed. </returns>
	public virtual JFrame displayWindow(int window_type, bool editable)
	{
		string routine = "StateMod_GUIUtil.displayWindow";
		//JGUIUtil.setWaitCursor(this, true);
		JFrame win = null;

		if (getWindowStatus(window_type) == OPEN)
		{
			// Window_type already opened so make sure that it is not
			// minimized and move to the front...
			win = getWindow(window_type);
			if (win.getState() == Frame.ICONIFIED)
			{
				win.setState(Frame.NORMAL);
			}
			// Now make sure it is in the front...
			win.toFront();
			Message.printStatus(2, "displayWindow", "Window displayed (already open): " + window_type);
		}

		// Create window for the appropriate window.
		// List windows in the order of the menus (generally)...

		else if (window_type == WINDOW_DATASET_SUMMARY)
		{
			//printStatus("Initializing data set summary window.", WAIT);
			win = displayDataSetSummary(__dataset);
			setWindowOpen(WINDOW_DATASET_SUMMARY, win);
		}
		else if (window_type == WINDOW_CONTROL)
		{
			//printStatus("Initializing control edit window.", WAIT);
			win = new StateMod_Control_JFrame(__dataset, this, true);
			setWindowOpen(WINDOW_CONTROL, win);
		}
		else if (window_type == WINDOW_RESPONSE)
		{
			// printStatus("Initializing response window.", WAIT);
			win = new StateMod_Response_JFrame(__dataset, this);
			setWindowOpen(WINDOW_RESPONSE, win);
		}
		else if (window_type == WINDOW_CONSUMPTIVE_USE)
		{
			//printStatus("Initializing consumptive use edit window.", WAIT);
			setWindowOpen(WINDOW_CONSUMPTIVE_USE, win);
			// TODO SAM 2011-06-22 For now disable editing
			win = new StateCU_Location_JFrame(false, __datasetStateCU, this, false);
		}
		else if (window_type == WINDOW_STREAMGAGE)
		{
			//printStatus("Initializing river station window.", WAIT);
			setWindowOpen(WINDOW_STREAMGAGE, win);
			win = new StateMod_StreamGage_JFrame(__dataset, this, true);
		}
		else if (window_type == WINDOW_DELAY_TABLE_MONTHLY)
		{
			//printStatus("Initializing delay edit window.", WAIT);
			win = new StateMod_DelayTable_JFrame(__dataset, this, true, true);
			setWindowOpen(WINDOW_DELAY_TABLE_MONTHLY, win);
		}
		else if (window_type == WINDOW_DELAY_TABLE_DAILY)
		{
			//printStatus("Initializing delay edit window.", WAIT);
			win = new StateMod_DelayTable_JFrame(__dataset, this, false, true);
			setWindowOpen(WINDOW_DELAY_TABLE_DAILY, win);
		}
		else if (window_type == WINDOW_DIVERSION)
		{
			//printStatus("Initializing diversions edit window.", WAIT);
			setWindowOpen(WINDOW_DIVERSION, win);
			win = new StateMod_Diversion_JFrame(__dataset, this, true);
		}
		else if (window_type == WINDOW_PRECIPITATION)
		{
			// Don't have a special window - just display all precipitation
			// time series as a graph.  This usually works because there are
			// not that many precipitation stations.  If it becomes a
			// problem, add additional logic.
			//printStatus("Initializing precipitation window.", WAIT);
			PropList props = new PropList("Precipitation");
			props.set("InitialView", "Graph");
			props.set("GraphType", "Bar");
			props.set("TotalWidth", "600");
			props.set("TotalHeight", "400");
			props.set("Title", "Precipitation (Monthly)");
			props.set("DisplayFont", "Courier");
			props.set("DisplaySize", "11");
			props.set("PrintFont", "Courier");
			props.set("PrintSize", "7");
			props.set("PageLength", "100");
			try
			{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<RTi.TS.TS> tslist = (java.util.List<RTi.TS.TS>)(__dataset.getComponentForComponentType(StateMod_DataSet.COMP_PRECIPITATION_TS_MONTHLY)).getData();
				IList<TS> tslist = (IList<TS>)(__dataset.getComponentForComponentType(StateMod_DataSet.COMP_PRECIPITATION_TS_MONTHLY)).getData();
				win = new TSViewJFrame(tslist, props);
				setWindowOpen(WINDOW_PRECIPITATION, win);
				// Use a window listener to know when the window closes
				// so that it can be managed like all the other data windows.
				win.addWindowListener(this);
			}
			catch (Exception e)
			{
				Message.printWarning(1, routine, "Error displaying precipitation data.");
				Message.printWarning(2, routine, e);
			}
		}
		else if (window_type == WINDOW_EVAPORATION)
		{
			// Don't have a special window - just display all evaporation
			// time series as a graph.  This usually works because there are
			// not that many precipitation stations.  If it becomes a
			// problem, add additional logic.
			//printStatus("Initializing evaporation window.", WAIT);
			PropList props = new PropList("Evaporation");
			props.set("InitialView", "Table");
			props.set("GraphType", "Bar");
			props.set("TotalWidth", "600");
			props.set("TotalHeight", "400");
			props.set("Title", "Evaporation (Monthly)");
			props.set("DisplayFont", "Courier");
			props.set("DisplaySize", "11");
			props.set("PrintFont", "Courier");
			props.set("PrintSize", "7");
			props.set("PageLength", "100");
			try
			{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<RTi.TS.TS> tslist = (java.util.List<RTi.TS.TS>)(__dataset.getComponentForComponentType(StateMod_DataSet.COMP_EVAPORATION_TS_MONTHLY)).getData();
				IList<TS> tslist = (IList<TS>)(__dataset.getComponentForComponentType(StateMod_DataSet.COMP_EVAPORATION_TS_MONTHLY)).getData();
				win = new TSViewJFrame(tslist, props);
				setWindowOpen(WINDOW_EVAPORATION, win);
				// Use a window listener to know when the window closes
				// so that it can be managed like all the other data windows.
				win.addWindowListener(this);
			}
			catch (Exception e)
			{
				Message.printWarning(1, routine, "Error displaying precipitation data.");
				Message.printWarning(2, routine, e);
			}
		}
		else if (window_type == WINDOW_RESERVOIR)
		{
			//printStatus("Initializing reservoirs edit window.", WAIT);
			win = new StateMod_Reservoir_JFrame(__dataset, this, true);
			setWindowOpen(WINDOW_RESERVOIR, win);
		}
		else if (window_type == WINDOW_INSTREAM)
		{
			//printStatus("Initializing instream flows edit window.",WAIT);
			setWindowOpen(WINDOW_INSTREAM, win);
			win = new StateMod_InstreamFlow_JFrame(__dataset, this, true);
		}
		else if (window_type == WINDOW_WELL)
		{
			//printStatus("Initializing well edit window.", WAIT);
			win = new StateMod_Well_JFrame(__dataset, this, true);
			setWindowOpen(WINDOW_WELL, win);
		}
		else if (window_type == WINDOW_PLAN)
		{
			//printStatus("Initializing plan edit window.", WAIT);
			win = new StateMod_Plan_JFrame(__dataset, this, true);
			setWindowOpen(WINDOW_PLAN, win);
		}
		else if (window_type == WINDOW_STREAMESTIMATE)
		{
			//printStatus("Initializing baseflows edit window.", WAIT);
			win = new StateMod_StreamEstimate_JFrame(__dataset, this, true);
			setWindowOpen(WINDOW_STREAMESTIMATE, win);
		}
		else if (window_type == WINDOW_RIVER_NETWORK)
		{
			//printStatus("Initializing river network edit window.", WAIT);
			win = new StateMod_RiverNetworkNode_JFrame(__dataset, this, true);
			setWindowOpen(WINDOW_RIVER_NETWORK, win);
		}
		else if (window_type == WINDOW_OPERATIONAL_RIGHT)
		{
			//printStatus("Initializing operational rights window.",WAIT);
			win = new StateMod_OperationalRight_JFrame(__dataset, this, true);
			setWindowOpen(WINDOW_OPERATIONAL_RIGHT, win);
		}
		else if (window_type == WINDOW_QUERY_TOOL)
		{
			win = new StateMod_QueryTool_JFrame(__dataset, this);
			setWindowOpen(WINDOW_QUERY_TOOL, win);
		}
		else
		{
			Message.printWarning(2, routine, "Unable to display specified window type: " + window_type);
		}

		//JGUIUtil.setWaitCursor(this, false);
		//printStatus("Ready", READY);

		// Return the JFrame so that it is easy for other code to do subsequent calls...

		return win;
	}

	/// <summary>
	/// Return the StateMod dataset being managed.
	/// </summary>
	private StateMod_DataSet getDataSet()
	{
		return __dataset;
	}

	/// <summary>
	/// Return the map panel used for StateMod.
	/// </summary>
	private GeoViewJPanel getMapPanel()
	{
		return __mapPanel;
	}

	/// <summary>
	/// Return the network editor used for StateMod.
	/// </summary>
	public virtual StateMod_Network_JFrame getNetworkEditor()
	{
		return __networkEditor;
	}

	/// <summary>
	/// Returns the window at the specified position. </summary>
	/// <param name="win_index"> the position of the window (should be one of the public fields above). </param>
	/// <returns> the window at the specified position. </returns>
	public virtual JFrame getWindow(int win_index)
	{
		return __windows[win_index];
	}

	/// <summary>
	/// Returns the status of the window at the specified position. </summary>
	/// <param name="win_index"> the position of the window (should be one of the public fields above). </param>
	/// <returns> the status of the window at the specified position. </returns>
	public virtual int getWindowStatus(int win_index)
	{
		return __windowStatus[win_index];
	}

	/// <summary>
	/// Returns whether a window is currently open or not. </summary>
	/// <returns> true if the window is open, false if not. </returns>
	public virtual bool isWindowOpen(int win_index)
	{
		if (__windowStatus[win_index] == OPEN)
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Refresh the contents of a window.  Currently this occurs by closing the window
	/// and then opening it again.  In the future, individual windows may be made more
	/// intelligent so that a full refresh of the contents is not necessary.  For now
	/// the close/open sequence should work, and is the same approach taken in the
	/// previous version of the GUI. </summary>
	/// <param name="win_type"> The window type to refresh (see WINDOW_*). </param>
	/// <param name="always_open"> If true, the window will always be opened.  If false, no
	/// action will occur if the window was not originally opened. </param>
	/// <returns> the JFrame that was refreshed.  This is used, for example, to move the
	/// window to the front (but it is not automatically moved to the front by this method). </returns>
	public virtual JFrame refreshWindow(int win_type, bool always_open)
	{ // Check to see if the window was opened...
		int status = getWindowStatus(win_type);
		JFrame window = getWindow(win_type);
		// First close the window...
		closeWindow(win_type);
		// Now open if requested or if it was previously open...
		if ((status == OPEN) || always_open)
		{
			// Window was not opened before but want to open it now...
			window = displayWindow(win_type);
		}
		return window;
	}

	/// <summary>
	/// Render an object as an annotation on the GeoView map. </summary>
	/// <param name="geoviewPanel"> the map object </param>
	/// <param name="objectToRender"> the object to render as an annotation on the map </param>
	/// <param name="label"> the string that is used to label the annotation on the map </param>
	public virtual void renderGeoViewAnnotation(GeoViewJComponent geoview, object objectToRender, string label)
	{
		GeoRecord geoRecord = null;
		GRDrawingArea da = geoview.getDrawingArea();
		GRDrawingAreaUtil.setColor(da, GRColor.black);
		GRDrawingAreaUtil.setFont(da, "Helvetica", "Bold", 14.0);
		// Make the symbol size relatively large so it is visible...
		// The pushpin is always drawn with the pin point on the point so center the text is OK
		// May need to project on the fly depending on the original data
		if (objectToRender is StateMod_OperationalRight)
		{
			// Draw source and destination as separate symbols
			StateMod_OperationalRight opr = (StateMod_OperationalRight)objectToRender;
			StateMod_DataSet dataset = getDataSet();
			StateMod_Data smdata = opr.lookupDestinationDataObject(dataset);
			GRPoint pointDest = null;
			GRPoint pointSource1 = null;
			GRPoint pointSource2 = null;
			string labelDest = "";
			string labelSource1 = "";
			string labelSource2 = "";
			if ((smdata != null) && (smdata is HasGeoRecord))
			{
				HasGeoRecord hasGeoRecord = (HasGeoRecord)smdata;
				geoRecord = hasGeoRecord.getGeoRecord();
				if (geoRecord != null)
				{
					pointDest = (GRPoint)geoRecord.getShape();
					GeoProjection layerProjection = geoRecord.getLayer().getProjection();
					GeoProjection geoviewProjection = geoview.getProjection();
					bool doProject = GeoProjection.needToProject(layerProjection, geoview.getProjection());
					if (doProject)
					{
						pointDest = (GRPoint)GeoProjection.projectShape(layerProjection, geoviewProjection, pointDest, false);
					}
					labelDest = label + "\n(Dest: " + smdata.getID() + " - " + smdata.getName() + ")";
				}
			}
			smdata = opr.lookupSource1DataObject(dataset);
			if ((smdata != null) && (smdata is HasGeoRecord))
			{
				HasGeoRecord hasGeoRecord = (HasGeoRecord)smdata;
				geoRecord = hasGeoRecord.getGeoRecord();
				if (geoRecord != null)
				{
					pointSource1 = (GRPoint)geoRecord.getShape();
					GeoProjection layerProjection = geoRecord.getLayer().getProjection();
					GeoProjection geoviewProjection = geoview.getProjection();
					bool doProject = GeoProjection.needToProject(layerProjection, geoview.getProjection());
					if (doProject)
					{
						pointSource1 = (GRPoint)GeoProjection.projectShape(layerProjection, geoviewProjection, pointSource1, false);
					}
					labelSource1 = label + "\n(Source1: " + smdata.getID() + " - " + smdata.getName() + ")";
				}
			}
			smdata = opr.lookupSource2DataObject(dataset);
			if ((smdata != null) && (smdata is HasGeoRecord))
			{
				HasGeoRecord hasGeoRecord = (HasGeoRecord)smdata;
				geoRecord = hasGeoRecord.getGeoRecord();
				if (geoRecord != null)
				{
					pointSource2 = (GRPoint)geoRecord.getShape();
					GeoProjection layerProjection = geoRecord.getLayer().getProjection();
					GeoProjection geoviewProjection = geoview.getProjection();
					bool doProject = GeoProjection.needToProject(layerProjection, geoview.getProjection());
					if (doProject)
					{
						pointSource2 = (GRPoint)GeoProjection.projectShape(layerProjection, geoviewProjection, pointSource2, false);
					}
					labelSource2 = label + "\n(Source2: " + smdata.getID() + " - " + smdata.getName() + ")";
				}
			}
			// Draw connecting lines first (under the symbols and text)...
			if ((pointDest != null) && (pointSource1 != null))
			{
				GRDrawingAreaUtil.setLineWidth(da, 2);
				GRDrawingAreaUtil.drawLine(da, pointDest.x, pointDest.y, pointSource1.x, pointSource1.y);
			}
			if ((pointDest != null) && (pointSource2 != null))
			{
				GRDrawingAreaUtil.setLineWidth(da, 2);
				GRDrawingAreaUtil.drawLine(da, pointDest.x, pointDest.y, pointSource2.x, pointSource2.y);
			}
			// Now draw all the symbols and text
			if (pointDest != null)
			{
				GRDrawingAreaUtil.drawSymbol(da, GRSymbol.SYM_PUSHPIN_VERTICAL, pointDest.getX(), pointDest.getY(), 32.0, 0, 0);
				GRDrawingAreaUtil.drawText(da, labelDest, pointDest.getX(), pointDest.getY(), 0.0, GRText.CENTER_X | GRText.TOP);
			}
			if (pointSource1 != null)
			{
				GRDrawingAreaUtil.drawSymbol(da, GRSymbol.SYM_PUSHPIN_VERTICAL, pointSource1.getX(), pointSource1.getY(), 32.0, 0, 0);
				GRDrawingAreaUtil.drawText(da, labelSource1, pointSource1.getX(), pointSource1.getY(), 0.0, GRText.CENTER_X | GRText.TOP);
			}
			if (pointSource2 != null)
			{
				GRDrawingAreaUtil.drawSymbol(da, GRSymbol.SYM_PUSHPIN_VERTICAL, pointSource2.getX(), pointSource2.getY(), 32.0, 0, 0);
				GRDrawingAreaUtil.drawText(da, labelSource2, pointSource2.getX(), pointSource2.getY(), 0.0, GRText.CENTER_X | GRText.TOP);
			}
		}
		else if (objectToRender is StateMod_InstreamFlow)
		{
			// Draw upstream and downstream as separate symbols
			StateMod_InstreamFlow ifs = (StateMod_InstreamFlow)objectToRender;
			StateMod_DataSet dataset = getDataSet();
			StateMod_Data smdata = ifs;
			GRPoint pointUpstream = null;
			GRPoint pointDownstream = null;
			string labelUpstream = "";
			string labelDownstream = "";
			if ((smdata != null) && (smdata is HasGeoRecord))
			{
				HasGeoRecord hasGeoRecord = (HasGeoRecord)smdata;
				geoRecord = hasGeoRecord.getGeoRecord();
				if (geoRecord != null)
				{
					pointUpstream = (GRPoint)geoRecord.getShape();
					GeoProjection layerProjection = geoRecord.getLayer().getProjection();
					GeoProjection geoviewProjection = geoview.getProjection();
					bool doProject = GeoProjection.needToProject(layerProjection, geoview.getProjection());
					if (doProject)
					{
						pointUpstream = (GRPoint)GeoProjection.projectShape(layerProjection, geoviewProjection, pointUpstream, false);
					}
					labelUpstream = label + "\n(Up: " + smdata.getID() + " - " + smdata.getName() + ")";
				}
			}
			smdata = ifs.lookupDownstreamDataObject(dataset);
			if ((smdata != null) && (smdata is HasGeoRecord))
			{
				HasGeoRecord hasGeoRecord = (HasGeoRecord)smdata;
				geoRecord = hasGeoRecord.getGeoRecord();
				if (geoRecord != null)
				{
					pointDownstream = (GRPoint)geoRecord.getShape();
					GeoProjection layerProjection = geoRecord.getLayer().getProjection();
					GeoProjection geoviewProjection = geoview.getProjection();
					bool doProject = GeoProjection.needToProject(layerProjection, geoview.getProjection());
					if (doProject)
					{
						pointDownstream = (GRPoint)GeoProjection.projectShape(layerProjection, geoviewProjection, pointDownstream, false);
					}
					labelDownstream = label + "\n(Down: " + smdata.getID() + " - " + smdata.getName() + ")";
				}
			}
			// Draw connecting lines first (under the symbols and text)...
			if ((pointUpstream != null) && (pointDownstream != null))
			{
				GRDrawingAreaUtil.setLineWidth(da, 2);
				GRDrawingAreaUtil.drawLine(da, pointUpstream.x, pointUpstream.y, pointDownstream.x, pointDownstream.y);
			}
			// Now draw all the symbols and text
			if (pointUpstream == pointDownstream)
			{
				labelUpstream = label;
			}
			if (pointUpstream != null)
			{
				GRDrawingAreaUtil.drawSymbol(da, GRSymbol.SYM_PUSHPIN_VERTICAL, pointUpstream.getX(), pointUpstream.getY(), 32.0, 0, 0);
				GRDrawingAreaUtil.drawText(da, labelUpstream, pointUpstream.getX(), pointUpstream.getY(), 0.0, GRText.CENTER_X | GRText.TOP);
			}
			if ((pointDownstream != null) && (pointDownstream != pointUpstream))
			{
				GRDrawingAreaUtil.drawSymbol(da, GRSymbol.SYM_PUSHPIN_VERTICAL, pointDownstream.getX(), pointDownstream.getY(), 32.0, 0, 0);
				GRDrawingAreaUtil.drawText(da, labelDownstream, pointDownstream.getX(), pointDownstream.getY(), 0.0, GRText.CENTER_X | GRText.TOP);
			}
		}
		else if (objectToRender is StateMod_Data)
		{
			Message.printStatus(2, "", "Rendering \"" + label + "\" annotation on map.");
			StateMod_Data smdata = (StateMod_Data)objectToRender;
			GRPoint point = null;
			if (smdata is HasGeoRecord)
			{
				HasGeoRecord hasGeoRecord = (HasGeoRecord)smdata;
				geoRecord = hasGeoRecord.getGeoRecord();
				point = (GRPoint)geoRecord.getShape();
			}
			else
			{
				// Do not have geographic data (should not get here)...
				return;
			}
			GeoProjection layerProjection = geoRecord.getLayer().getProjection();
			GeoProjection geoviewProjection = geoview.getProjection();
			bool doProject = GeoProjection.needToProject(layerProjection, geoview.getProjection());
			if (doProject)
			{
				point = (GRPoint)GeoProjection.projectShape(layerProjection, geoviewProjection, point, false);
			}
			GRDrawingAreaUtil.drawSymbol(da, GRSymbol.SYM_PUSHPIN_VERTICAL, point.getX(), point.getY(), 32.0, 0, 0);
			GRDrawingAreaUtil.drawText(da, label, point.getX(), point.getY(), 0.0, GRText.CENTER_X | GRText.TOP);
		}
		else
		{
			// Do not know how to handle
			return;
		}
	}

	/// <summary>
	/// Render an object as an annotation on the network editor. </summary>
	/// <param name="network"> the StateMod network object being rendered </param>
	/// <param name="annotationData"> annotation data to render </param>
	public virtual void renderStateModNetworkAnnotation(StateMod_Network_JComponent network, StateMod_Network_AnnotationData annotationData)
	{
		//GRPoint point = null;
		string commonID = null;
		object objectToRender = annotationData.getObject();
		string label = annotationData.getLabel();
		// Draw the annotated version on top...
		GRDrawingArea da = network.getDrawingArea();
		GRDrawingAreaUtil.setFont(da, "Helvetica", "Bold", 14.0);
		// Use blue so that red can be used to highlight problems.  The color should allow black
		// symbols on top to be legible
		GRColor blue = new GRColor(0,102,255);
		GRDrawingAreaUtil.setColor(da,blue);
		if (objectToRender is StateMod_OperationalRight)
		{
			// Draw source and destination as separate symbols
			StateMod_OperationalRight opr = (StateMod_OperationalRight)objectToRender;
			StateMod_OperationalRight_Metadata metadata = StateMod_OperationalRight_Metadata.getMetadata(opr.getItyopr());
			StateMod_DataSet dataset = getDataSet();
			HydrologyNode nodeDest = null;
			HydrologyNode nodeSource1 = null;
			HydrologyNode nodeSource2 = null;
			string labelDest = "";
			string labelSource1 = "";
			string labelSource2 = "";
			StateMod_Data smdataDest = opr.lookupDestinationDataObject(dataset);
			if (smdataDest != null)
			{
				nodeDest = network.findNode(smdataDest.getID(), false, false); // Do not zoom to the node
			}
			else
			{
				Message.printStatus(2,"","Unable to find StateMod destination data object for OPR \"" + opr.getID() + "\" destination \"" + opr.getCiopde() + "\"");
			}
			StateMod_Data smdataSource1 = opr.lookupSource1DataObject(dataset);
			if (smdataSource1 != null)
			{
				nodeSource1 = network.findNode(smdataSource1.getID(), false, false); // Do not zoom to the node
			}
			StateMod_Data smdataSource2 = opr.lookupSource2DataObject(dataset);
			if (smdataSource2 != null)
			{
				nodeSource2 = network.findNode(smdataSource2.getID(), false, false); // Do not zoom to the node
			}
			int textPositionDest = 0;
			if (nodeDest != null)
			{
				// The symbol is drawn behind the normal network so make bigger
				double size = nodeDest.getSymbol().getSize() * 2.0;
				labelDest = label + "\n(Dest: " + smdataDest.getID() + " - " + smdataDest.getName() + ")";
				//GRDrawingAreaUtil.drawSymbol(da, GRSymbol.SYM_FCIR, node.getX(), node.getY(), size, 0, 0);
				//GRDrawingAreaUtil.drawText(da, label, node.getX(), node.getY(), 0.0, GRText.CENTER_X|GRText.BOTTOM );
				string labelPosition = nodeDest.getTextPosition();
				if ((labelPosition.IndexOf("Above", StringComparison.Ordinal) >= 0) || (labelPosition.IndexOf("Upper", StringComparison.Ordinal) >= 0))
				{
					textPositionDest = GRText.CENTER_X | GRText.BOTTOM;
				}
				else
				{
					textPositionDest = GRText.CENTER_X | GRText.TOP;
				}
				GRDrawingAreaUtil.drawSymbolText(da, GRSymbol.SYM_FCIR, nodeDest.getX(), nodeDest.getY(), size, labelDest, 0.0, textPositionDest, 0, 0);
			}
			if (nodeSource1 != null)
			{
				// The symbol is drawn behind the normal network so make bigger
				double size = nodeSource1.getSymbol().getSize() * 2.0;
				labelSource1 = label + "\n(Source1: " + smdataSource1.getID() + " - " + smdataSource1.getName() + ")";
				//GRDrawingAreaUtil.drawSymbol(da, GRSymbol.SYM_FCIR, node.getX(), node.getY(), size, 0, 0);
				//GRDrawingAreaUtil.drawText(da, label, node.getX(), node.getY(), 0.0, GRText.CENTER_X|GRText.BOTTOM );
				// TODO SAM 2010-12-29 Need to optimize label positioning
				string labelPosition = nodeSource1.getTextPosition();
				int textPosition = 0;
				if ((labelPosition.IndexOf("Above", StringComparison.Ordinal) >= 0) || (labelPosition.IndexOf("Upper", StringComparison.Ordinal) >= 0))
				{
					textPosition = GRText.CENTER_X | GRText.BOTTOM;
				}
				else
				{
					// Default text below symbol.
					textPosition = GRText.CENTER_X | GRText.TOP;
				}
				// Override to make sure it does not draw on the destination
				if (nodeDest == nodeSource1)
				{
					if (textPositionDest == (GRText.CENTER_X | GRText.TOP))
					{
						textPosition = GRText.CENTER_X | GRText.BOTTOM;
					}
					else
					{
						textPosition = GRText.CENTER_X | GRText.TOP;
					}
				}
				GRDrawingAreaUtil.drawSymbolText(da, GRSymbol.SYM_FCIR, nodeSource1.getX(), nodeSource1.getY(), size, labelSource1, 0.0, textPosition, 0, 0);
			}
			if (nodeSource2 != null)
			{
				// The symbol is drawn behind the normal network so make bigger
				double size = nodeSource2.getSymbol().getSize() * 2.0;
				labelSource2 = label + "\n(Source2: " + smdataSource2.getID() + " - " + smdataSource2.getName() + ")";
				//GRDrawingAreaUtil.drawSymbol(da, GRSymbol.SYM_FCIR, node.getX(), node.getY(), size, 0, 0);
				//GRDrawingAreaUtil.drawText(da, label, node.getX(), node.getY(), 0.0, GRText.CENTER_X|GRText.BOTTOM );
				string labelPosition = nodeSource2.getTextPosition();
				int textPosition = 0;
				if ((labelPosition.IndexOf("Above", StringComparison.Ordinal) >= 0) || (labelPosition.IndexOf("Upper", StringComparison.Ordinal) >= 0))
				{
					textPosition = GRText.CENTER_X | GRText.BOTTOM;
				}
				else
				{
					textPosition = GRText.CENTER_X | GRText.TOP;
				}
				// Override to make sure it does not draw on the destination
				if (nodeDest == nodeSource2)
				{
					if (textPositionDest == (GRText.CENTER_X | GRText.TOP))
					{
						textPosition = GRText.CENTER_X | GRText.BOTTOM;
					}
					else
					{
						textPosition = GRText.CENTER_X | GRText.TOP;
					}
				}
				GRDrawingAreaUtil.drawSymbolText(da, GRSymbol.SYM_FCIR, nodeSource2.getX(), nodeSource2.getY(), size, labelSource2, 0.0, textPosition, 0, 0);
			}
			// Draw the intervening structures specifically mentioned in the operational right
			if (metadata.getRightTypeUsesInterveningStructuresWithoutLoss() || metadata.getRightTypeUsesInterveningStructuresWithLoss(opr.getOprLimit()))
			{
				IList<string> structureIDList = opr.getInterveningStructureIDs();
				foreach (string structureID in structureIDList)
				{
					HydrologyNode nodeIntervening = network.findNode(structureID, false, false); // Do not zoom to the node
					if (nodeIntervening != null)
					{
						double size = nodeIntervening.getSymbol().getSize() * 2.0;
						GRDrawingAreaUtil.drawSymbol(da, GRSymbol.SYM_FCIR, nodeIntervening.getX(), nodeIntervening.getY(), size, 0, 0);
					}
				}
			}
			// Draw the network lines connecting the source and destination
			GRDrawingAreaUtil.setColor(da,blue);
			if ((nodeDest != null) && (nodeSource1 != null))
			{
				GRDrawingAreaUtil.setLineWidth(da, Math.Max(nodeDest.getSymbol().getSize(), nodeSource1.getSymbol().getSize()));
				IList<HydrologyNode> connectingNodes = network.getNetwork().getNodeSequence(nodeDest,nodeSource1);
				if (connectingNodes.Count > 0)
				{
					// Draw a line between each node
					double[] x = new double[connectingNodes.Count];
					double[] y = new double[x.Length];
					int i = -1;
					foreach (HydrologyNode node in connectingNodes)
					{
						++i;
						x[i] = node.getX();
						y[i] = node.getY();
					}
					GRDrawingAreaUtil.drawPolyline(da, x.Length, x, y);
				}
			}
			if ((nodeDest != null) && (nodeSource2 != null))
			{
				GRDrawingAreaUtil.setLineWidth(da, Math.Max(nodeDest.getSymbol().getSize(), nodeSource2.getSymbol().getSize()));
				IList<HydrologyNode> connectingNodes = network.getNetwork().getNodeSequence(nodeDest,nodeSource2);
				if (connectingNodes.Count > 0)
				{
					// Draw a line between each node
					double[] x = new double[connectingNodes.Count];
					double[] y = new double[x.Length];
					int i = -1;
					foreach (HydrologyNode node in connectingNodes)
					{
						++i;
						x[i] = node.getX();
						y[i] = node.getY();
					}
					GRDrawingAreaUtil.drawPolyline(da, x.Length, x, y);
				}
			}
		}
		else if (objectToRender is StateMod_InstreamFlow)
		{
			// Draw source and destination as separate symbols
			StateMod_InstreamFlow ifs = (StateMod_InstreamFlow)objectToRender;
			StateMod_DataSet dataset = getDataSet();
			HydrologyNode nodeUpstream = null;
			HydrologyNode nodeDownstream = null;
			string labelUpstream = "";
			string labelDownstream = "";
			StateMod_Data smdataUpstream = ifs;
			if (smdataUpstream != null)
			{
				nodeUpstream = network.findNode(smdataUpstream.getID(), false, false); // Do not zoom to the node
			}
			StateMod_Data smdataDownstream = ifs.lookupDownstreamDataObject(dataset);
			if (smdataDownstream != null)
			{
				nodeDownstream = network.findNode(smdataDownstream.getID(), false, false); // Do not zoom to the node
			}
			int textPositionUpstream = 0;
			if (nodeUpstream != null)
			{
				// The symbol is drawn behind the normal network so make bigger
				double size = nodeUpstream.getSymbol().getSize() * 2.0;
				if (nodeUpstream == nodeDownstream)
				{
					labelUpstream = label;
				}
				else
				{
					labelUpstream = label + "\n(Up: " + smdataUpstream.getID() + " - " + smdataUpstream.getName() + ")";
				}
				//GRDrawingAreaUtil.drawSymbol(da, GRSymbol.SYM_FCIR, node.getX(), node.getY(), size, 0, 0);
				//GRDrawingAreaUtil.drawText(da, label, node.getX(), node.getY(), 0.0, GRText.CENTER_X|GRText.BOTTOM );
				if ((nodeDownstream == null) || (nodeUpstream.getY() > nodeDownstream.getY()))
				{
					textPositionUpstream = GRText.CENTER_X | GRText.BOTTOM;
				}
				else
				{
					textPositionUpstream = GRText.CENTER_X | GRText.TOP;
				}
				GRDrawingAreaUtil.drawSymbolText(da, GRSymbol.SYM_FCIR, nodeUpstream.getX(), nodeUpstream.getY(), size, labelUpstream, 0.0, textPositionUpstream, 0, 0);
			}
			if ((nodeDownstream != null) && (nodeDownstream != nodeUpstream))
			{
				// The symbol is drawn behind the normal network so make bigger
				double size = nodeDownstream.getSymbol().getSize() * 2.0;
				labelDownstream = label + "\n(Down: " + smdataDownstream.getID() + " - " + smdataDownstream.getName() + ")";
				//GRDrawingAreaUtil.drawSymbol(da, GRSymbol.SYM_FCIR, node.getX(), node.getY(), size, 0, 0);
				//GRDrawingAreaUtil.drawText(da, label, node.getX(), node.getY(), 0.0, GRText.CENTER_X|GRText.BOTTOM );
				// TODO SAM 2010-12-29 Need to optimize label positioning
				int textPositionDownstream = 0;
				if ((nodeDownstream == null) || (nodeUpstream.getY() < nodeDownstream.getY()))
				{
					textPositionDownstream = GRText.CENTER_X | GRText.BOTTOM;
				}
				else
				{
					// Default text below symbol.
					textPositionDownstream = GRText.CENTER_X | GRText.TOP;
				}
				GRDrawingAreaUtil.drawSymbolText(da, GRSymbol.SYM_FCIR, nodeDownstream.getX(), nodeDownstream.getY(), size, labelDownstream, 0.0, textPositionDownstream, 0, 0);
			}
			// Draw the network lines connecting the upstream and downstream
			GRDrawingAreaUtil.setColor(da,blue);
			if ((nodeUpstream != null) && (nodeDownstream != null))
			{
				GRDrawingAreaUtil.setLineWidth(da, Math.Max(nodeUpstream.getSymbol().getSize(), nodeDownstream.getSymbol().getSize()));
				IList<HydrologyNode> connectingNodes = network.getNetwork().getNodeSequence(nodeUpstream,nodeDownstream);
				if (connectingNodes.Count > 0)
				{
					// Draw a line between each node
					double[] x = new double[connectingNodes.Count];
					double[] y = new double[x.Length];
					int i = -1;
					foreach (HydrologyNode node in connectingNodes)
					{
						++i;
						x[i] = node.getX();
						y[i] = node.getY();
					}
					GRDrawingAreaUtil.drawPolyline(da, x.Length, x, y);
				}
			}
		}
		else if (objectToRender is StateMod_Data)
		{
			Message.printStatus(2, "", "Rendering \"" + label + "\" annotation on network.");
			StateMod_Data smdata = (StateMod_Data)objectToRender;
			commonID = smdata.getID();
			if (!string.ReferenceEquals(commonID, null))
			{
				// Find the node in the network and scroll to it.
				HydrologyNode node = network.findNode(commonID, false, false); // Do not zoom to the node
				// The symbol is drawn behind the normal network so make bigger
				double size = node.getSymbol().getSize() * 2.0;
				//GRDrawingAreaUtil.drawSymbol(da, GRSymbol.SYM_FCIR, node.getX(), node.getY(), size, 0, 0);
				//GRDrawingAreaUtil.drawText(da, label, node.getX(), node.getY(), 0.0, GRText.CENTER_X|GRText.BOTTOM );
				GRDrawingAreaUtil.drawSymbolText(da, GRSymbol.SYM_FCIR, node.getX(), node.getY(), size, label, 0.0, GRText.CENTER_X | GRText.BOTTOM, 0, 0);
			}
		}
	}

	/// <summary>
	/// Selects the desired ID in the table and displays the appropriate data
	/// in the remainder of the window. </summary>
	/// <param name="id"> the identifier to select in the list. </param>
	public virtual void selectID(string id)
	{
	}

	/// <summary>
	/// Sets the data set for which this class is the window manager. </summary>
	/// <param name="dataset"> the dataset for which to manage windows. </param>
	public virtual void setDataSet(StateMod_DataSet dataset)
	{
		__dataset = dataset;
	}

	/// <summary>
	/// Sets the data set for which this class is the window manager. </summary>
	/// <param name="dataset"> the dataset for which to manage windows. </param>
	public virtual void setDataSet(StateCU_DataSet datasetStateCU)
	{
		__datasetStateCU = datasetStateCU;
	}

	/// <summary>
	/// Set the map panel.
	/// </summary>
	public virtual void setMapPanel(GeoViewJPanel mapPanel)
	{
		__mapPanel = mapPanel;
	}

	/// <summary>
	/// Set the network editor.
	/// </summary>
	public virtual void setNetworkEditor(StateMod_Network_JFrame networkEditor)
	{
		__networkEditor = networkEditor;
	}

	/// <summary>
	/// Sets the window at the specified position. </summary>
	/// <param name="win_index"> the position of the window (should be one of the public fields above). </param>
	/// <param name="window"> the window to set. </param>
	public virtual void setWindow(int win_index, JFrame window)
	{
		__windows[win_index] = window;
	}

	/// <summary>
	/// Indicate that a window is opened, and provide the JFrame corresponding to the
	/// window.  This method should be called to allow the StateMod GUI to track
	/// windows (so that only one copy of a data set group window is open at a time). </summary>
	/// <param name="win_type"> Window type (see WINDOW_*). </param>
	/// <param name="window"> The JFrame associated with the window. </param>
	public virtual void setWindowOpen(int win_type, JFrame window)
	{
		setWindow(win_type, window);
		setWindowStatus(win_type, OPEN);
		Message.printStatus(1, "setWindowOpen", "Window set open: " + win_type);
	}

	/// <summary>
	/// Sets the window at the specified position to be either OPEN or CLOSED. </summary>
	/// <param name="win_index"> the position of the window (should be one of the public fields above). </param>
	/// <param name="status"> the status of the window (OPEN or CLOSED) </param>
	private void setWindowStatus(int win_index, int status)
	{
		__windowStatus[win_index] = status;
	}

	/// <summary>
	/// Show a StateMod data object on the map.  This method simply helps with the hand-off of information. </summary>
	/// <param name="smData"> the StateMod data object to display on the map </param>
	/// <param name="label"> the label for the data object on the map </param>
	public virtual void showOnMap(StateMod_Data smData, string label, GRLimits limits, GeoProjection projection)
	{
		GeoViewJPanel geoviewPanel = getMapPanel();
		if (geoviewPanel != null)
		{
			geoviewPanel.addAnnotationRenderer(this, smData, label, limits, projection, true);
		}
	}

	/// <summary>
	/// Show a StateMod data object on the network.  This method simply helps with the hand-off of information. </summary>
	/// <param name="smData"> the StateMod data object to display on the network </param>
	/// <param name="label"> the label for the data object on the network </param>
	public virtual void showOnNetwork(StateMod_Data smData, string label, GRLimits limits)
	{
		StateMod_Network_JFrame networkEditor = getNetworkEditor();
		if (networkEditor != null)
		{
			networkEditor.addAnnotationRenderer(this, smData, label, limits, true);
		}
	}

	/// <summary>
	/// Update the status of a window.  This is primarily used to update the status
	/// of the main JFrame, with the title and menus being updated if the data set has been modified.
	/// </summary>
	public virtual void updateWindowStatus(int win_type)
	{
		if (win_type == WINDOW_MAIN)
		{
			StateMod_GUIUpdatable window = (StateMod_GUIUpdatable)getWindow(win_type);
			window.updateWindowStatus();
		}
	}

	/// <summary>
	/// Responds to window activated events; does nothing. </summary>
	/// <param name="evt"> the WindowEvent that happened. </param>
	public virtual void windowActivated(WindowEvent evt)
	{
	}

	// TODO SAM 2006-08-16 Perhaps all windows should be listened for here to simplify the individual
	// window code.
	/// <summary>
	/// Responds to window closed events.  The following windows are handled because
	/// they use generic components that cannot specifically set their close status:
	/// <pre>
	/// Data Set Summary
	/// Precipitation Time Series
	/// Evaporation Time Series
	/// </pre> </summary>
	/// <param name="e"> the WindowEvent that happened. </param>
	public virtual void windowClosed(WindowEvent e)
	{
		Component c = e.getComponent();
		if (c == getWindow(WINDOW_DATASET_SUMMARY))
		{
			setWindowStatus(WINDOW_DATASET_SUMMARY, CLOSED);
			setWindow(WINDOW_DATASET_SUMMARY, null);
		}
		else if (c == getWindow(WINDOW_PRECIPITATION))
		{
			setWindowStatus(WINDOW_PRECIPITATION, CLOSED);
			setWindow(WINDOW_PRECIPITATION, null);
		}
		else if (c == getWindow(WINDOW_EVAPORATION))
		{
			setWindowStatus(WINDOW_EVAPORATION, CLOSED);
			setWindow(WINDOW_EVAPORATION, null);
		}
	}

	/// <summary>
	/// Responds to window closing events; closes the application. </summary>
	/// <param name="evt"> the WindowEvent that happened. </param>
	public virtual void windowClosing(WindowEvent evt)
	{
	}

	/// <summary>
	/// Responds to window deactivated events; does nothing. </summary>
	/// <param name="evt"> the WindowEvent that happened. </param>
	public virtual void windowDeactivated(WindowEvent evt)
	{
	}

	/// <summary>
	/// Responds to window deiconified events; does nothing. </summary>
	/// <param name="evt"> the WindowEvent that happened. </param>
	public virtual void windowDeiconified(WindowEvent evt)
	{
	}

	/// <summary>
	/// Responds to window iconified events; does nothing. </summary>
	/// <param name="evt"> the WindowEvent that happened. </param>
	public virtual void windowIconified(WindowEvent evt)
	{
	}

	/// <summary>
	/// Responds to window opened events; does nothing. </summary>
	/// <param name="evt"> the WindowEvent that happened. </param>
	public virtual void windowOpened(WindowEvent evt)
	{
	}

	}

}