using System;
using System.Collections.Generic;

// StateCU_Location_JFrame - dialog to display location info

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
// StateCU_Location_JFrame - dialog to display location info
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
//
// 2003-07-14	J. Thomas Sapienza, RTi	Initial version.
// 2003-07-22	JTS, RTi		Revised following SAM's review. 
// 2004-02-28	Steven A. Malers, RTi	Moved some methods from StateCU_Data to
//					StateCU_Util.
// 2005-01-17	JTS, RTi		Changed getOriginalRow() to 
//					getOriginalRowNumber().
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
//------------------------------------------------------------------------------

// TODO SAM 2011-06-22 This class was initially developed based on the StateMod GUI
// code to prototype a StateCU GUI.  However, it was never used in production.
// It is being pressed into service now to display StateCU structure information in the
// StateMod GUI as read-only.  For this reason some information is disabled because
// the files are not available from a StateMod data set.

namespace DWR.StateCU
{


	using StateMod_DataSet_WindowManager = DWR.StateMod.StateMod_DataSet_WindowManager;
	using StateMod_DiversionRight = DWR.StateMod.StateMod_DiversionRight;
	using StateMod_GUIUtil = DWR.StateMod.StateMod_GUIUtil;
	using TSViewJFrame = RTi.GRTS.TSViewJFrame;
	using TS = RTi.TS.TS;
	using JGUIUtil = RTi.Util.GUI.JGUIUtil;
	using JWorksheet = RTi.Util.GUI.JWorksheet;
	using ResponseJDialog = RTi.Util.GUI.ResponseJDialog;
	using DataSetComponent = RTi.Util.IO.DataSetComponent;
	using PropList = RTi.Util.IO.PropList;
	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// This class is a GUI for displaying location data.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class StateCU_Location_JFrame extends javax.swing.JFrame implements java.awt.event.ActionListener, java.awt.event.KeyListener, java.awt.event.MouseListener, java.awt.event.WindowListener
	public class StateCU_Location_JFrame : JFrame, ActionListener, KeyListener, MouseListener, WindowListener
	{

	/// <summary>
	/// Button labels.
	/// </summary>
	private readonly string __BUTTON_FIND_NEXT = "Find Next", __BUTTON_ID = "ID", __BUTTON_NAME = "Name", __BUTTON_HELP = "Help", __BUTTON_CLOSE = "Close", __BUTTON_APPLY = "Apply", __BUTTON_GRAPH = "Graph", __BUTTON_TABLE = "Table", __BUTTON_SUMMARY = "Summary", __BUTTON_CANCEL = "Cancel", __BUTTON_GRAPH_ALL = "Graph All Time Series";

	/// <summary>
	/// Whether the class is being used in the StateMod GUI or StateCU GUI.
	/// </summary>
	private bool __isStateCU = false; // false since code is only used in StateMod GUI

	/// <summary>
	/// Boolean specifying whether the form is editable or not.
	/// </summary>
	private bool __editable = false;

	/// <summary>
	/// Button group for selecting what to search the jworksheet for.
	/// </summary>
	private ButtonGroup __searchCriteriaGroup;

	/// <summary>
	/// Array of the elements of __textDisables that should never be editable.
	/// </summary>
	private int[] __textUneditables;

	/// <summary>
	/// Index of the currently-selected location.
	/// </summary>
	private int __currentLocationIndex;
	/// <summary>
	/// Index of the previously-selected location.
	/// </summary>
	private int __lastLocationIndex;

	/// <summary>
	/// GUI buttons.
	/// </summary>
	private JButton __findNextLocation;

	/// <summary>
	/// Checkboxes for selecting the kind of time series to graph.
	/// </summary>
	private JCheckBox __cropPatternCheckBox, __irrigationPracticeCheckBox, __diversionCheckBox, __precipitationCheckBox, __temperatureCheckBox, __frostDatesCheckBox;

	/// <summary>
	/// Array of JComponents that should be disabled if nothing is selected.
	/// </summary>
	private JComponent[] __disables;

	/// <summary>
	/// Radio buttons for selecting the field on which to search through the
	/// JWorksheet.
	/// </summary>
	private JRadioButton __searchIDJRadioButton, __searchNameJRadioButton;

	/// <summary>
	/// Array of JTextComponents that should be disabled if nothing is selected.
	/// </summary>
	private JTextComponent[] __textDisables;

	/// <summary>
	/// GUI text fields.
	/// </summary>
	private JTextField __searchID, __searchName, __locationIDJTextField, __nameJTextField, __latitudeJTextField, __elevationJTextField, __region1JTextField, __region2JTextField, __awcJTextField;

	/// <summary>
	/// Worksheet for displaying location information.
	/// </summary>
	private JWorksheet __worksheet;

	/// <summary>
	/// Dataset containing the location data.
	/// </summary>
	private StateCU_DataSet __dataset;

	/// <summary>
	/// DataSetComponent containing location data.
	/// </summary>
	private DataSetComponent __locationComponent;

	/// <summary>
	/// List of locations data in the DataSetComponent.
	/// </summary>
	private IList<StateCU_Location> __locationsList;

	private JWorksheet __delayWorksheet;
	private StateCU_Location_TableModel __delayModel;
	private DataSetComponent __delaysComponent;
	private IList<StateCU_DelayTableAssignment> __delaysVector;

	private JWorksheet __stationWorksheet;
	private StateCU_Location_TableModel __stationModel;
	private DataSetComponent __stationsComponent;
	private IList<StateCU_ClimateStation> __stationsVector;

	private JWorksheet __rightsWorksheet;
	private StateCU_Location_TableModel __rightsModel;
	private DataSetComponent __rightsComponent;

	private JTextField __messageJTextField, __statusJTextField;

	/// <summary>
	/// Data set window manager.
	/// </summary>
	private StateMod_DataSet_WindowManager __dataset_wm;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="isStateCU"> whether the UI is being used by the StateCU/StateDMI (true) or StateMod GUI (false). </param>
	/// <param name="dataset"> the StateCU dataset that is managing the data </param>
	/// <param name="dataset_wm"> the StateCU dataset window manager for the data </param>
	/// <param name="isStateCU"> if true, the UI is used in a StateCU GUI, if false, the StateMod GUI. </param>
	/// <param name="dataset"> dataset containing data to display </param>
	/// <param name="editable"> whether the display should be editable or not </param>
	public StateCU_Location_JFrame(bool isStateCU, StateCU_DataSet dataset, StateMod_DataSet_WindowManager dataset_wm, bool editable)
	{
		StateMod_GUIUtil.setTitle(this, dataset, "CU Locations", null);
		JGUIUtil.setIcon(this, JGUIUtil.getIconImage());
		__isStateCU = isStateCU;
		__currentLocationIndex = -1;
		__editable = editable;

		__dataset = dataset;
		__dataset_wm = dataset_wm;
		__locationComponent = __dataset.getComponentForComponentType(StateCU_DataSet.COMP_CU_LOCATIONS);
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<StateCU_Location> locationsList0 = (java.util.List<StateCU_Location>)__locationComponent.getData();
		IList<StateCU_Location> locationsList0 = (IList<StateCU_Location>)__locationComponent.getData();
		__locationsList = locationsList0;
		Message.printStatus(2, "", "Have " + __locationsList.Count + " locations");

		__delaysComponent = __dataset.getComponentForComponentType(StateCU_DataSet.COMP_DELAY_TABLE_ASSIGNMENT_MONTHLY);
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List <StateCU_DelayTableAssignment> delaysVector0 = (java.util.List <StateCU_DelayTableAssignment>)__delaysComponent.getData();
		IList<StateCU_DelayTableAssignment> delaysVector0 = (IList <StateCU_DelayTableAssignment>)__delaysComponent.getData();
		__delaysVector = delaysVector0;

		__stationsComponent = __dataset.getComponentForComponentType(StateCU_DataSet.COMP_CLIMATE_STATIONS);
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<StateCU_ClimateStation> stationsVector0 = (java.util.List<StateCU_ClimateStation>)__stationsComponent.getData();
		IList<StateCU_ClimateStation> stationsVector0 = (IList<StateCU_ClimateStation>)__stationsComponent.getData();
		__stationsVector = stationsVector0;

		__rightsComponent = __dataset.getComponentForComponentType(StateCU_DataSet.COMP_DIVERSION_RIGHTS);

		setupGUI(0);
	}

	/// <summary>
	/// Constructor. </summary>
	/// <param name="isStateCU"> whether the UI is being used by the StateCU (true) or StateMod/StateDMI GUI (false). </param>
	/// <param name="dataset"> the StateCU dataset that is managing the data </param>
	/// <param name="dataset_wm"> the StateCU dataset window manager for the data </param>
	/// <param name="location"> StateCU_Location object to display </param>
	/// <param name="editable"> whether the display should be editable or not. </param>
	public StateCU_Location_JFrame(bool isStateCU, StateCU_DataSet dataset, StateMod_DataSet_WindowManager dataset_wm, StateCU_Location location, bool editable) : base()
	{
		StateMod_GUIUtil.setTitle(this, dataset, "CU Locations", null);
		JGUIUtil.setIcon(this, JGUIUtil.getIconImage());
		__isStateCU = isStateCU;
		__dataset = dataset;
		__dataset_wm = dataset_wm;
		__currentLocationIndex = -1;
		__locationsList = new List<StateCU_Location>();

		__locationComponent = __dataset.getComponentForComponentType(StateCU_DataSet.COMP_CU_LOCATIONS);
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<StateCU_Location> locationsList0 = (java.util.List<StateCU_Location>)__locationComponent.getData();
		IList<StateCU_Location> locationsList0 = (IList<StateCU_Location>)__locationComponent.getData();
		__locationsList = locationsList0;

		string id = location.getID();
		int index = StateCU_Util.IndexOf(__locationsList, id);

		__delaysComponent = __dataset.getComponentForComponentType(StateCU_DataSet.COMP_DELAY_TABLE_ASSIGNMENT_MONTHLY);
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<StateCU_DelayTableAssignment> delaysVector0 = (java.util.List<StateCU_DelayTableAssignment>)__delaysComponent.getData();
		IList<StateCU_DelayTableAssignment> delaysVector0 = (IList<StateCU_DelayTableAssignment>)__delaysComponent.getData();
		__delaysVector = delaysVector0;

		__stationsComponent = __dataset.getComponentForComponentType(StateCU_DataSet.COMP_CLIMATE_STATIONS);
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<StateCU_ClimateStation> stationsVector0 = (java.util.List<StateCU_ClimateStation>)__stationsComponent.getData();
		IList<StateCU_ClimateStation> stationsVector0 = (IList<StateCU_ClimateStation>)__stationsComponent.getData();
		__stationsVector = stationsVector0;

		__rightsComponent = __dataset.getComponentForComponentType(StateCU_DataSet.COMP_DIVERSION_RIGHTS);

		__editable = editable;

		setupGUI(index);
	}

	/// <summary>
	/// Responds to action performed events. </summary>
	/// <param name="e"> the ActionEvent that happened. </param>
	public virtual void actionPerformed(ActionEvent e)
	{

		try
		{
		string action = e.getActionCommand();
		object source = e.getSource();

		if (action.Equals(__BUTTON_HELP))
		{
			// REVISIT HELP (JTS - 2003-06-10
		}
		else if (action.Equals(__BUTTON_CLOSE))
		{
			saveCurrentRecord();
			if (__dataset_wm != null)
			{
				__dataset_wm.closeWindow(StateMod_DataSet_WindowManager.WINDOW_CONSUMPTIVE_USE);
			}
			else
			{
				JGUIUtil.close(this);
			}
		}
		else if (action.Equals(__BUTTON_APPLY))
		{
			saveCurrentRecord();
		}
		else if (action.Equals(__BUTTON_CANCEL))
		{
			if (__dataset_wm != null)
			{
				__dataset_wm.closeWindow(StateMod_DataSet_WindowManager.WINDOW_CONSUMPTIVE_USE);
			}
			else
			{
				JGUIUtil.close(this);
			}
		}
		else if (action.Equals(__BUTTON_GRAPH) || action.Equals(__BUTTON_TABLE) || action.Equals(__BUTTON_SUMMARY) || action.Equals(__BUTTON_GRAPH_ALL))
		{
			displayTSViewJFrame(action);
		}
		else if (source == __findNextLocation)
		{
			searchWorksheet(__worksheet.getSelectedRow() + 1);
		}
		else if (source == __searchID || source == __searchName)
		{
			searchWorksheet();
		}
		else if (source == __searchNameJRadioButton)
		{
			__searchName.setEditable(true);
			__searchID.setEditable(false);
		}
		else if (source == __searchIDJRadioButton)
		{
			__searchName.setEditable(false);
			__searchID.setEditable(true);
		}

		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
			Console.Write(ex.StackTrace);
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
		string label = "The following error" + plural + "encountered "
			+ "trying to save the record:\n";
		for (int i = 0; i < errorCount; i++)
		{
			label += errors[i] + "\n";
		}
		// TODO SAM 2007-03-01 Why not using Message/Logging?
		new ResponseJDialog(this, "Errors encountered", label, ResponseJDialog.OK);
		return false;
	}

	private void displayTSViewJFrame(string action)
	{
		string routine = "displayTSViewJFrame";

		IList<TS> tslist = new List<TS>();

		bool graphAll = false;
		if (action.Equals(__BUTTON_GRAPH_ALL))
		{
			graphAll = true;
		}

		if (__precipitationCheckBox.isSelected() || graphAll)
		{
	//		tslist.add(
		}
		if (__temperatureCheckBox.isSelected() || graphAll)
		{
	//		tslist.add(
		}
		if (__frostDatesCheckBox.isSelected() || graphAll)
		{
	//		tslist.add(
		}
		if (__cropPatternCheckBox.isSelected() || graphAll)
		{
	//		tslist.add(
		}
		if (__irrigationPracticeCheckBox.isSelected() || graphAll)
		{
	//		tslist.add(
		}
		if (__diversionCheckBox.isSelected() || graphAll)
		{
	//		tslist.add(
		}

		PropList graphProps = new PropList("TSView");
		if (action.Equals(__BUTTON_GRAPH) || graphAll)
		{
			graphProps.set("InitialView", "Graph");
		}
		else if (action.Equals(__BUTTON_TABLE))
		{
			graphProps.set("InitialView", "Table");
		}
		else if (action.Equals(__BUTTON_SUMMARY))
		{
			graphProps.set("InitialView", "Summary");
		}
		// graphProps.set("HelpKey", "TSTool.ExportMenu");
		graphProps.set("TotalWidth", "600");
		graphProps.set("TotalHeight", "400");
		graphProps.set("Title", "Demand");
		graphProps.set("DisplayFont", "Courier");
		graphProps.set("DisplaySize", "11");
		graphProps.set("PrintFont", "Courier");
		graphProps.set("PrintSize", "7");
		graphProps.set("PageLength", "100");

		try
		{
			new TSViewJFrame(tslist, graphProps);
		}
		catch (Exception e)
		{
			Message.printWarning(2, routine, "Error displays TSViewJFrame");
			Message.printWarning(2, routine, e);
		}
	}

	/// <summary>
	/// Clean up before garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~StateCU_Location_JFrame()
	{
		__searchCriteriaGroup = null;
		__textUneditables = null;
		__findNextLocation = null;
		__disables = null;
		__searchIDJRadioButton = null;
		__searchNameJRadioButton = null;
		__textDisables = null;
		__searchID = null;
		__searchName = null;
		__locationIDJTextField = null;
		__nameJTextField = null;
		__latitudeJTextField = null;
		__elevationJTextField = null;
		__region1JTextField = null;
		__region2JTextField = null;
		__worksheet = null;
		__dataset = null;
		__locationComponent = null;
		__locationsList = null;
		__cropPatternCheckBox = null;
		__irrigationPracticeCheckBox = null;
		__diversionCheckBox = null;
		__precipitationCheckBox = null;
		__temperatureCheckBox = null;
		__frostDatesCheckBox = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Responds to key pressed events; does nothing. </summary>
	/// <param name="e"> the KeyEvent that happened. </param>
	public virtual void keyPressed(KeyEvent e)
	{
	}

	/// <summary>
	/// Responds to key released events; calls 'processTableSelection' with the 
	/// newly-selected index in the table. </summary>
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
	/// Responds to mouse released events; calls 'processTableSelection' with the 
	/// newly-selected index in the table. </summary>
	/// <param name="e"> the MouseEvent that happened. </param>
	public virtual void mouseReleased(MouseEvent e)
	{
		processTableSelection(__worksheet.getSelectedRow());
	}

	/// <summary>
	/// Processes a table selection (either via a mouse press or programmatically 
	/// from selectTableIndex() by writing the old data back to the data set component
	/// and getting the next selection's data out of the data and displaying it on the form. </summary>
	/// <param name="index"> the index of the reservoir to display on the form. </param>
	private void processTableSelection(int index)
	{
		__lastLocationIndex = __currentLocationIndex;
		__currentLocationIndex = __worksheet.getOriginalRowNumber(index);

		saveLastRecord();

		if (__worksheet.getSelectedRow() == -1)
		{
			nothingSelected();
			return;
		}

		somethingSelected();

		StateCU_Location location = (StateCU_Location)__locationsList[__currentLocationIndex];

		__locationIDJTextField.setText(location.getID());
		__nameJTextField.setText(location.getName());
		StateCU_Util.checkAndSet(location.getLatitude(), __latitudeJTextField);
		StateCU_Util.checkAndSet(location.getElevation(),__elevationJTextField);
		__region1JTextField.setText(location.getRegion1());
		__region2JTextField.setText(location.getRegion2());
		StateCU_Util.checkAndSet(location.getAwc(), __awcJTextField);

		int dindex = StateCU_Util.IndexOf(__delaysVector, location.getID());
		if (__delayModel != null)
		{
			if (dindex == -1 || __delaysVector == null)
			{
				__delayModel.setDelay(location, null);
			}
			else
			{
				__delayModel.setDelay(location, (StateCU_DelayTableAssignment)__delaysVector[dindex]);
			}
		}

		__stationModel.setStations(location, __stationsVector);

		IList<StateMod_DiversionRight> v = new List<StateMod_DiversionRight>();
		if ((__rightsComponent != null) && (__rightsComponent.getData() != null))
		{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<DWR.StateMod.StateMod_DiversionRight> v0 = (java.util.List<DWR.StateMod.StateMod_DiversionRight>)__rightsComponent.getData();
			IList<StateMod_DiversionRight> v0 = (IList<StateMod_DiversionRight>)__rightsComponent.getData();
			v = v0;
		}
		string did = null;
		string sdid = null;
		string id = location.getID();
		int j = 0;
		IList<StateMod_DiversionRight> rights = new List<StateMod_DiversionRight>();
		StateMod_DiversionRight right = null;
		for (int i = 0; i < v.Count; i++)
		{
			right = v[i];
			did = right.getID();
			j = did.IndexOf(".", StringComparison.Ordinal);
			if (j == -1)
			{
				if (id.Equals(did))
				{
					rights.Add(right);
				}
			}
			else
			{
				sdid = did.Substring(0, j);
				if (id.Equals(sdid))
				{
					rights.Add(right);
				}
			}
		}

		if (__rightsModel != null)
		{
			__rightsModel.setRights(location, rights);
		}
	}

	/// <summary>
	/// Saves the prior record selected in the table; called when moving to a new 
	/// record by a table selection.
	/// </summary>
	private void saveLastRecord()
	{
		saveInformation(__lastLocationIndex);
	}

	/// <summary>
	/// Saves the current record selected in the table; called when the window is closed
	/// or minimized or apply is pressed.
	/// </summary>
	private void saveCurrentRecord()
	{
		saveInformation(__currentLocationIndex);
	}

	/// <summary>
	/// Saves the information associated with the currently-selected location.
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

		StateCU_Location location = (StateCU_Location)__locationsList[record];

		location.setName(__nameJTextField.getText());
		location.setID(__locationIDJTextField.getText());
		location.setLatitude((new double?(__latitudeJTextField.getText())));
		location.setElevation((new double?(__elevationJTextField.getText())));
		location.setRegion1(__region1JTextField.getText());
		location.setRegion2(__region2JTextField.getText());
		location.setAwc((new double?(__awcJTextField.getText())));
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
			col = 1;
		}
		else
		{
			searchFor = __searchName.getText().Trim();
			col = 2;
		}
		int index = __worksheet.find(searchFor, col, row, JWorksheet.FIND_EQUAL_TO | JWorksheet.FIND_CONTAINS | JWorksheet.FIND_CASE_INSENSITIVE | JWorksheet.FIND_WRAPAROUND);
		if (index != -1)
		{
			selectTableIndex(index);
		}
	}

	/// <summary>
	/// Selects the desired index in the table, but also displays the appropriate data
	/// in the remainder of the window. </summary>
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
	/// <param name="index"> the index to select </param>
	private void setupGUI(int index)
	{
		string routine = "setupGUI";

		addWindowListener(this);

		JPanel p1 = new JPanel(); // first 6 months' efficiency
		//JPanel p2 = new JPanel();	// last 6 months' efficiency
		JPanel p3 = new JPanel(); // div sta id -> switch for diversion
		JPanel p4 = new JPanel(); // user name -> data type switch

		JPanel left_panel = new JPanel(); // multilist and search area
		JPanel right_panel = new JPanel(); // everything else

		__locationIDJTextField = new JTextField(12);
		__nameJTextField = new JTextField(24);
		__latitudeJTextField = new JTextField(12);
		__elevationJTextField = new JTextField(12);
		__region1JTextField = new JTextField(12);
		__region2JTextField = new JTextField(12);
		__awcJTextField = new JTextField(12);

		__searchID = new JTextField(10);
		__searchName = new JTextField(10);
		__searchName.setEditable(false);
		__findNextLocation = new JButton(__BUTTON_FIND_NEXT);
		__searchCriteriaGroup = new ButtonGroup();
		__searchIDJRadioButton = new JRadioButton(__BUTTON_ID, true);
		__searchNameJRadioButton = new JRadioButton(__BUTTON_NAME, false);
		__searchCriteriaGroup.add(__searchIDJRadioButton);
		__searchCriteriaGroup.add(__searchNameJRadioButton);

		JButton applyJButton = new JButton(__BUTTON_APPLY);
		JButton cancelJButton = new JButton(__BUTTON_CANCEL);
		JButton helpJButton = new JButton(__BUTTON_HELP);
		helpJButton.setEnabled(false);
		helpJButton.setVisible(false);
		JButton closeJButton = new JButton(__BUTTON_CLOSE);

		GridBagLayout gb = new GridBagLayout();
		JPanel mainJPanel = new JPanel();
		mainJPanel.setLayout(gb);
		p1.setLayout(new GridLayout(4, 6, 2, 0));
		p3.setLayout(gb);
		p4.setLayout(gb);
		right_panel.setLayout(gb);
		left_panel.setLayout(gb);

		int y;

		PropList p = new PropList("StateCU_Location_JFrame.JWorksheet");

		p.add("JWorksheet.ShowPopupMenu=true");
		p.add("JWorksheet.AllowCopy=true");
		p.add("JWorksheet.SelectionMode=SingleRowSelection");

		int[] widths = null;
		try
		{
			StateCU_Location_TableModel tmw = new StateCU_Location_TableModel(__locationsList);
			StateCU_Location_CellRenderer crw = new StateCU_Location_CellRenderer(tmw);

			__worksheet = new JWorksheet(crw, tmw, p);

			__worksheet.removeColumn(0);
			__worksheet.removeColumn(3);
			__worksheet.removeColumn(4);
			__worksheet.removeColumn(5);
			__worksheet.removeColumn(6);
			__worksheet.removeColumn(7);
			__worksheet.removeColumn(8);
			__worksheet.removeColumn(9);
			__worksheet.removeColumn(10);
			__worksheet.removeColumn(11);
			__worksheet.removeColumn(12);
			__worksheet.removeColumn(13);

			widths = crw.getColumnWidths();
		}
		catch (Exception e)
		{
			Message.printWarning(2, routine, e);
			__worksheet = new JWorksheet(0, 0, p);
			Console.WriteLine(e.ToString());
			Console.Write(e.StackTrace);
		}
		__worksheet.setPreferredScrollableViewportSize(null);
		__worksheet.setHourglassJFrame(this);
		__worksheet.addMouseListener(this);
		__worksheet.addKeyListener(this);

		JGUIUtil.addComponent(left_panel, new JScrollPane(__worksheet), 0, 0, 6, 9, 1, 1, 0, 0, 1, 1, GridBagConstraints.BOTH, GridBagConstraints.CENTER);

		y = 0;
		JGUIUtil.addComponent(p3, new JLabel("Location ID:"), 0, y, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(p3, __locationIDJTextField, 1, y, 1, 1, 1, 1, 1, 0, 0, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);
		__locationIDJTextField.setEditable(false);

		y++;
		JGUIUtil.addComponent(p3, new JLabel("Name:"), 0, y, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(p3, __nameJTextField, 1, y, 1, 1, 1, 1, 1, 0, 0, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);

		y++;
		JGUIUtil.addComponent(p3, new JLabel("Latitude (decimal degrees):"), 0, y, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(p3, __latitudeJTextField, 1, y, 1, 1, 1, 1, 1, 0, 0, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);

		y++;
		JGUIUtil.addComponent(p3, new JLabel("Elevation (feet):"), 0, y, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(p3, __elevationJTextField, 1, y, 1, 1, 1, 1, 1, 0, 0, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);

		y++;
		JGUIUtil.addComponent(p3, new JLabel("Region 1 (typically county):"), 0, y, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(p3, __region1JTextField, 1, y, 1, 1, 1, 1, 1, 0, 0, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);

		y++;
		JGUIUtil.addComponent(p3, new JLabel("Region 2 (typically not used):"), 0, y, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(p3, __region2JTextField, 1, y, 1, 1, 1, 1, 1, 0, 0, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);

		y++;
		JGUIUtil.addComponent(p3, new JLabel("Available Water Content (fraction):"), 0, y, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(p3, __awcJTextField, 1, y, 1, 1, 1, 1, 1, 0, 0, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);
		y = 0;

		// two top panels of info
		JGUIUtil.addComponent(right_panel, p3, 0, 0, 2, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.NORTHWEST);

		int[] widths2 = null;
		try
		{
			__stationModel = new StateCU_Location_TableModel(new List<StateCU_Location>());
			StateCU_Location_CellRenderer crw = new StateCU_Location_CellRenderer(__stationModel);

			__stationWorksheet = new JWorksheet(crw, __stationModel, p);

			__stationWorksheet.removeColumn(0);
			__stationWorksheet.removeColumn(1);
			__stationWorksheet.removeColumn(2);
			__stationWorksheet.removeColumn(3);
			__stationWorksheet.removeColumn(4);
			__stationWorksheet.removeColumn(9);
			__stationWorksheet.removeColumn(10);
			__stationWorksheet.removeColumn(11);
			__stationWorksheet.removeColumn(12);
			__stationWorksheet.removeColumn(13);

			widths2 = crw.getColumnWidths();
		}
		catch (Exception e)
		{
			Message.printWarning(3, routine, e);
			__stationWorksheet = new JWorksheet(0, 0, p);
		}
		__stationWorksheet.setPreferredScrollableViewportSize(null);
		__stationWorksheet.setHourglassJFrame(this);
		JScrollPane jsp2 = new JScrollPane(__stationWorksheet);
		jsp2.setBorder(BorderFactory.createTitledBorder(jsp2.getBorder(), "Climate Stations"));
		y = 1;
		JGUIUtil.addComponent(right_panel, jsp2, 0, y++, 1, 1, 1, 1, 0, 0, 0, 0, GridBagConstraints.BOTH, GridBagConstraints.NORTHWEST);

		int[] widths3 = null;
		if (__dataset.getDataSetType() >= StateCU_DataSet.TYPE_WATER_SUPPLY_LIMITED_BY_WATER_RIGHTS)
		{
			try
			{
				__rightsModel = new StateCU_Location_TableModel(new List<StateCU_Location>());
				StateCU_Location_CellRenderer crw = new StateCU_Location_CellRenderer(__rightsModel);

				__rightsWorksheet = new JWorksheet(crw, __rightsModel, p);

				__rightsWorksheet.removeColumn(1);
				__rightsWorksheet.removeColumn(2);
				__rightsWorksheet.removeColumn(3);
				__rightsWorksheet.removeColumn(4);
				__rightsWorksheet.removeColumn(5);
				__rightsWorksheet.removeColumn(6);
				__rightsWorksheet.removeColumn(7);
				__rightsWorksheet.removeColumn(8);

				widths3 = crw.getColumnWidths();
			}
			catch (Exception e)
			{
				Message.printWarning(3, routine, e);
				__rightsWorksheet = new JWorksheet(0, 0, p);
			}
			__rightsWorksheet.setPreferredScrollableViewportSize(null);
			__rightsWorksheet.setHourglassJFrame(this);
			JScrollPane jsp3 = new JScrollPane(__rightsWorksheet);
			jsp3.setBorder(BorderFactory.createTitledBorder(jsp3.getBorder(), "Diversion Rights"));
			JGUIUtil.addComponent(right_panel, jsp3, 0, y++, 1, 1, 1, 1, 0, 0, 0, 0, GridBagConstraints.BOTH, GridBagConstraints.NORTHWEST);
		}

		JPanel graphPanel = new JPanel();
		graphPanel.setLayout(gb);
		graphPanel.setBorder(BorderFactory.createTitledBorder("Time Series"));
		if (!__isStateCU)
		{
			// Data for graphs are only available when used with a full StateCU data set
			graphPanel.setVisible(false);
		}
		int yy = 0;
		__precipitationCheckBox = new JCheckBox("Precipitation (Monthly)");
		__temperatureCheckBox = new JCheckBox("Temperature (Monthly)");
		__frostDatesCheckBox = new JCheckBox("Frost Dates (Yearly)");
		__cropPatternCheckBox = new JCheckBox("Crop Pattern (Yearly)");
		__irrigationPracticeCheckBox = new JCheckBox("Irrigation Practice (Yearly)");
		__diversionCheckBox = new JCheckBox("Diversion (Monthly)");

		JGUIUtil.addComponent(graphPanel, __precipitationCheckBox, 0, yy++, 3, 1, 1, 0, 0, 0, 0, 0, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);
		JGUIUtil.addComponent(graphPanel, __temperatureCheckBox, 0, yy++, 3, 1, 1, 0, 0, 0, 0, 0, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);
		JGUIUtil.addComponent(graphPanel, __frostDatesCheckBox, 0, yy++, 3, 1, 1, 0, 0, 0, 0, 0, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);
		JGUIUtil.addComponent(graphPanel, __cropPatternCheckBox, 0, yy++, 3, 1, 1, 0, 0, 0, 0, 0, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);
		if (__dataset.getDataSetType() >= StateCU_DataSet.TYPE_WATER_SUPPLY_LIMITED)
		{
			JGUIUtil.addComponent(graphPanel, __irrigationPracticeCheckBox, 0, yy++, 3, 1, 1, 0, 0, 0, 0, 0, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);
		}
		if (__dataset.getDataSetType() >= StateCU_DataSet.TYPE_WATER_SUPPLY_LIMITED_BY_WATER_RIGHTS)
		{
			JGUIUtil.addComponent(graphPanel, __diversionCheckBox, 0, yy++, 3, 1, 1, 0, 0, 0, 0, 0, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);
		}

		JButton graphButton = new JButton(__BUTTON_GRAPH);
		JButton tableButton = new JButton(__BUTTON_TABLE);
		JButton summaryButton = new JButton(__BUTTON_SUMMARY);
		JButton graphAllButton = new JButton(__BUTTON_GRAPH_ALL);

		JGUIUtil.addComponent(graphPanel, graphButton, 0, yy, 1, 1, 1, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(graphPanel, tableButton, 1, yy, 1, 1, 1, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(graphPanel, summaryButton, 2, yy++, 1, 1, 1, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);

		JGUIUtil.addComponent(graphPanel, graphAllButton, 0, yy, 2, 1, 1, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);

		JGUIUtil.addComponent(right_panel, graphPanel, 0, y++, 1, 1, 1, 0, 0, 0, 0, 0, GridBagConstraints.HORIZONTAL, GridBagConstraints.NORTHWEST);

		int[] widths4 = null;
		if (__dataset.getDataSetType() >= StateCU_DataSet.TYPE_RIVER_DEPLETION)
		{
			try
			{
				__delayModel = new StateCU_Location_TableModel(new List<StateCU_Location>());
				StateCU_Location_CellRenderer crw = new StateCU_Location_CellRenderer(__delayModel);

				__delayWorksheet = new JWorksheet(crw, __delayModel, p);

				__delayWorksheet.removeColumn(1);
				__delayWorksheet.removeColumn(2);
				__delayWorksheet.removeColumn(5);
				__delayWorksheet.removeColumn(6);
				__delayWorksheet.removeColumn(7);
				__delayWorksheet.removeColumn(8);
				__delayWorksheet.removeColumn(9);
				__delayWorksheet.removeColumn(10);
				__delayWorksheet.removeColumn(11);
				__delayWorksheet.removeColumn(12);
				__delayWorksheet.removeColumn(13);

				widths4 = crw.getColumnWidths();
			}
			catch (Exception e)
			{
				Message.printWarning(2, routine, e);
				__delayWorksheet = new JWorksheet(0, 0, p);
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}
			__delayWorksheet.setPreferredScrollableViewportSize(null);
			__delayWorksheet.setHourglassJFrame(this);
			JScrollPane jsp4 = new JScrollPane(__delayWorksheet);
			jsp4.setBorder(BorderFactory.createTitledBorder(jsp4.getBorder(), "Delay Table Assignment"));
			JGUIUtil.addComponent(right_panel, jsp4, 0, y++, 1, 1, 1, 1, 0, 0, 0, 0, GridBagConstraints.BOTH, GridBagConstraints.NORTHWEST);
		}

		// add search areas
		y = 10;
		JPanel searchPanel = new JPanel();
		searchPanel.setLayout(gb);
		searchPanel.setBorder(BorderFactory.createTitledBorder("Search above list for:     "));

		JGUIUtil.addComponent(left_panel, searchPanel, 0, y, 4, 1, 0, 0, 10, 10, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);

		int y2 = 0;
		JGUIUtil.addComponent(searchPanel, __searchIDJRadioButton, 0, y2, 1, 1, 0, 0, 5, 10, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		__searchIDJRadioButton.addActionListener(this);
		JGUIUtil.addComponent(searchPanel, __searchID, 1, y2, 1, 1, 1, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		__searchID.addActionListener(this);

		y2++;
		JGUIUtil.addComponent(searchPanel, __searchNameJRadioButton, 0, y2, 1, 1, 0, 0, 5, 10, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		__searchNameJRadioButton.addActionListener(this);
		JGUIUtil.addComponent(searchPanel, __searchName, 1, y2, 1, 1, 1, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		__searchName.addActionListener(this);

		y2++;
		JGUIUtil.addComponent(searchPanel, __findNextLocation, 0, y2, 4, 1, 0, 0, 20, 10, 20, 10, GridBagConstraints.NONE, GridBagConstraints.WEST);
		__findNextLocation.addActionListener(this);
		// add buttons which lead to location
		// direct flow demand, and return flow information
		y = 6;
		FlowLayout fl = new FlowLayout(FlowLayout.CENTER);
		JPanel p5 = new JPanel();
		p5.setLayout(new GridLayout(5, 2));

		// add help and close buttons
		y = 10;
		JPanel p6 = new JPanel();
		p6.setLayout(fl);
		if (__editable)
		{
			p6.add(applyJButton);
			p6.add(cancelJButton);
		}
		p6.add(helpJButton);
		p6.add(closeJButton);
		JGUIUtil.addComponent(right_panel, p6, GridBagConstraints.RELATIVE, y, 4, 1, 1, 0, 30, 0, 0, 0, GridBagConstraints.BOTH, GridBagConstraints.SOUTH);
		applyJButton.addActionListener(this);
		cancelJButton.addActionListener(this);
		helpJButton.addActionListener(this);
		closeJButton.addActionListener(this);

		JGUIUtil.addComponent(mainJPanel, left_panel, 0, 0, 4, 10, 1, 1, 10, 10, 10, 0, GridBagConstraints.BOTH, GridBagConstraints.WEST);
		JGUIUtil.addComponent(mainJPanel, right_panel, GridBagConstraints.RELATIVE, 0, 4, 10, .4, 1, 10, 10, 10, 10, GridBagConstraints.BOTH, GridBagConstraints.EAST);

		JPanel bottomJPanel = new JPanel();
		bottomJPanel.setLayout(gb);
		__messageJTextField = new JTextField();
		__messageJTextField.setEditable(false);

		JGUIUtil.addComponent(bottomJPanel, __messageJTextField, 0, 0, 7, 1, 1.0, 0.0, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);
		__statusJTextField = new JTextField(5);
		__statusJTextField.setEditable(false);
		JGUIUtil.addComponent(bottomJPanel, __statusJTextField, 7, 0, 1, 1, 0.0, 0.0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		getContentPane().add("South", bottomJPanel);

		getContentPane().add(mainJPanel);

		if (__dataset_wm != null)
		{
			__dataset_wm.setWindowOpen(StateMod_DataSet_WindowManager.WINDOW_CONSUMPTIVE_USE, this);
		}

		initializeDisables();

	//	JGUIUtil.center(this);
		pack();
		if (__isStateCU)
		{
			setSize(800,820);
		}
		else
		{
			setSize(800,420);
		}
		JGUIUtil.center(this);
		selectTableIndex(index);
		setVisible(true);

		if (widths != null)
		{
			__worksheet.setColumnWidths(widths);
		}
		if (widths2 != null)
		{
			__stationWorksheet.setColumnWidths(widths2);
		}
		if (widths3 != null)
		{
			__rightsWorksheet.setColumnWidths(widths3);
		}
		if (widths4 != null)
		{
			__delayWorksheet.setColumnWidths(widths4);
		}
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
	/// in StateCU_GUIUtil. </summary>
	/// <param name="e"> the WindowEvent that happened. </param>
	public virtual void windowClosing(WindowEvent e)
	{
		saveCurrentRecord();
		if (__dataset_wm != null)
		{
			__dataset_wm.closeWindow(StateMod_DataSet_WindowManager.WINDOW_CONSUMPTIVE_USE);
		}
	}

	/// <summary>
	/// Responds to Window deactivated events; saves the current information. </summary>
	/// <param name="e"> the WindowEvent that happened. </param>
	public virtual void windowDeactivated(WindowEvent e)
	{
		saveCurrentRecord();
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

	private void initializeDisables()
	{
		__disables = new JComponent[0];

		__textDisables = new JTextComponent[7];
		__textDisables[0] = __locationIDJTextField;
		__textDisables[1] = __nameJTextField;
		__textDisables[2] = __latitudeJTextField;
		__textDisables[3] = __elevationJTextField;
		__textDisables[4] = __region1JTextField;
		__textDisables[5] = __region2JTextField;
		__textDisables[6] = __awcJTextField;

		__textUneditables = new int[1];
		__textUneditables[0] = 0;
	}

	private void somethingSelected()
	{
		if (!__editable)
		{
			for (int i = 0; i < __disables.Length; i++)
			{
				__disables[i].setEnabled(false);
			}
			for (int i = 0; i < __textDisables.Length; i++)
			{
				__textDisables[i].setEditable(false);
			}
		}
		else
		{
			for (int i = 0; i < __disables.Length; i++)
			{
				__disables[i].setEnabled(true);
			}
			for (int i = 0; i < __textDisables.Length; i++)
			{
				__textDisables[i].setEditable(true);
			}
			for (int i = 0; i < __textUneditables.Length; i++)
			{
				__textDisables[__textUneditables[i]].setEditable(false);
			}
		}
	}

	/// <summary>
	/// Disables everything (in response to nothing being selected)
	/// </summary>
	private void nothingSelected()
	{
		for (int i = 0; i < __disables.Length; i++)
		{
			__disables[i].setEnabled(false);
		}
		for (int i = 0; i < __textDisables.Length; i++)
		{
			__textDisables[i].setText("");
			__textDisables[i].setEditable(false);
		}
		if (__delayWorksheet != null)
		{
			__delayWorksheet.clear();
		}
		if (__stationWorksheet != null)
		{
			__stationWorksheet.clear();
		}
		if (__rightsWorksheet != null)
		{
			__rightsWorksheet.clear();
		}

	}

	}

}