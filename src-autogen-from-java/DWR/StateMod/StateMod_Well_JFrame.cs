using System;
using System.Collections.Generic;

// StateMod_Well_JFrame - dialog to edit the well information.

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
// StateMod_Well_JFrame - dialog to edit the well information.
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
// 
// 28 Sep 1999	Catherine E.		Created initial version of class
//		Nutting-Lane, RTi
// 25 Oct 1999	CEN, RTi		Moved daily id to allow addition of
//					daily id to diversion, reservoir and
//					instream flow windows in a consistent
//					manner.
// 01 Apr 2001	Steven A. Malers, RTi	Change GUI to JGUIUtil.  Add finalize().
//					Remove import *.
// 15 Aug 2001	SAM, RTi		Add disabled buttons for new data.
// 2001-12-11	SAM, RTi		Fixed bug where use type was getting
//					set incorrectly(was using getNroutinew()
//					instead of getIrturnw()).
// 2002-09-19	SAM, RTi		Use isDirty()instead of setDirty()to
//					indicate edits.
//------------------------------------------------------------------------------
// 2003-06-10	J. Thomas Sapienza, RTi	Initial swing version from
//					SMwellsWindow
// 2003-06-24	JTS, RTi		First functioning version.
// 2003-07-15	JTS, RTi		* Added checkInput() framework for 
//					validating user input prior to the 
//					values being saved.
// 					* Added status bar.
//					* Changed to use new dataset design.
// 2003-07-16	JTS, RTi		Added constructor that allows a well
//					to be initially selected
// 2003-07-17	JTS, RTI		Change so that constructor takes a 
//					boolean that says whether the form's
//					data can be modified.
// 2003-07-23	JTS, RTi		Updated JWorksheet code following
//					JWorksheet revisions.
// 2003-08-03	SAM, RTi		* Change pumping component type.
//					  indexOf() is now in StateMod_Util.
//					* Require title as parameter in
//					  constructor.
// 2003-08-16	SAM, RTi		Change window ID to WINDOW_WELL.
// 2003-08-26	SAM, RTi		Enable StateMod_DataSet_WindowManager.
// 2003-08-27	JTS, RTi		Added selectID() to select an ID 
//					on the worksheet from outside the GUI.
// 2003-08-29	JTS, RTi		Update based on changes in
//					StateMod_Well.
// 2003-09-03	JTS, RTi		Removed buttons for graphing time
//					series and replaced with JCheckBoxes.
// 2003-09-05	JTS, RTi		* Changed some field labels.
//					* Made Daily ID a combo box selection
//					  and changed its load and save logic.
//					* Added a combo box for Senior Supply.
//					* Made Associated Diversion a combo box
//					  selection and changed its load and 
//					  save logic.
//					* Organized the time series checkboxes
//					  and added buttons to display time
//					  series.
// 					* Class is now an item listener in 
//					  order to enable/disable graph buttons
//					  based on selected checkboxes.
// 2003-09-06	SAM, RTi		Fix problem with graph not sizing.
// 2003-09-08	JTS, RTi		* Added checkTimeSeriesButtonsStates()
//					  to enable time series display buttons
//					  appropriately.
//					* Added saveEfficiencies() so that
//					  efficiency information is actually
//					  saved.
//					* Enable reading of monthly and
//					  constant efficiencies.
// 2003-09-09	JTS, RTi		* Put a label around the system 
//				 	  efficiency panel.
//					* Moved the system efficiency panel
//					  above the time series and subforms
//					  panels.
// 2003-09-19	SAM, RTi		Add estimated daily historical and
//					demands.
// 2003-09-23	JTS, RTi		Uses new StateMod_GUIUtil code for
//					setting titles.
// 2004-01-22	JTS, RTi		Updated to use JScrollWorksheet and
//					the new row headers.
// 2005-01-18	JTS, RTi		Removed calls to removeColumn() as 
//					the table model handles that now.
// 2006-03-05	SAM, RTi		* Comment out the help button since
//					  on-line help is not currently enabled.
// 2006-03-06	JTS, RTi		If an associated diversion cannot be 
//					found in the list, it is added (in order
//					to support multiple ways in which N/A
//					is typed in the data).
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
	using DayTS = RTi.TS.DayTS;
	using TS = RTi.TS.TS;
	using JGUIUtil = RTi.Util.GUI.JGUIUtil;
	using JScrollWorksheet = RTi.Util.GUI.JScrollWorksheet;
	using JWorksheet = RTi.Util.GUI.JWorksheet;
	using ResponseJDialog = RTi.Util.GUI.ResponseJDialog;
	using SimpleJButton = RTi.Util.GUI.SimpleJButton;
	using SimpleJComboBox = RTi.Util.GUI.SimpleJComboBox;
	using DataSetComponent = RTi.Util.IO.DataSetComponent;
	using PropList = RTi.Util.IO.PropList;
	using Message = RTi.Util.Message.Message;
	using StopWatch = RTi.Util.Time.StopWatch;

	/// <summary>
	/// This class is a GUI for displaying and editing well data.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class StateMod_Well_JFrame extends javax.swing.JFrame implements java.awt.event.ActionListener, java.awt.event.ItemListener, java.awt.event.KeyListener, java.awt.event.MouseListener, java.awt.event.WindowListener
	public class StateMod_Well_JFrame : JFrame, ActionListener, ItemListener, KeyListener, MouseListener, WindowListener
	{

	/// <summary>
	/// Data types for the data type combo box.
	/// </summary>
	private readonly string __DATA_TYPE_MONTHLY = "1 - Monthly", __DATA_TYPE_ANNUAL = "2 - Annual", __DATA_TYPE_ESTIMATE_ZERO = "5 - Zero", __DATA_TYPE_DIRECT_DIV = "6 - Direct Diversion";

	/// <summary>
	/// Strings for the supply type combo box.
	/// </summary>
	private readonly string __SUPPLY_0 = "0 - Water right priorities determine diversion (SW primary)", __SUPPLY_ELSE = "Adjust well water rights by ...";

	/// <summary>
	/// Use type values
	/// </summary>
	private readonly string __USE_INBASIN1 = "1 - In Basin", __USE_INBASIN2 = "2 - In Basin", __USE_INBASIN3 = "3 - In Basin", __USE_TRANSMOUNTAIN = "4 - Transmountain";

	/// <summary>
	/// Demsrc option strings
	/// </summary>
	private readonly string __DEMSRC_IRR_GIS = "1 - Irr. acr. from GIS DB", __DEMSRC_IRR_TIA = "2 - Irr. acr. from structure file (tia)", __DEMSRC_IRR_MULT_GIS = "3 - Irr. acr. from GIS DB, primary comp. served by mult. structs", __DEMSRC_GIS_TIA = "4 - Same as 3 but data from struct. file (tia)", __DEMSRC_IRR_MULT_GIS_SEC = "5 - Irr. acr. from GIS DB, second. comp. served by mult. structs", __DEMSRC_MUNI_IND_XMTN = "6 - Municipal, industrial, or transmountain structure", __DEMSRC_CARRIER = "7 - Carrier structure", __DEMSRC_USER = "8 - Acreage data provided by user", __DEMSRC_UNKNOWN = "-999 -  Acreage data unknown";

	/// <summary>
	/// Button labels.
	/// </summary>
	private readonly string __BUTTON_SHOW_ON_MAP = "Show on Map", __BUTTON_SHOW_ON_NETWORK = "Show on Network", __BUTTON_FIND_NEXT = "Find Next", __BUTTON_ID = "ID", __BUTTON_NAME = "Name", __BUTTON_WATER_RIGHTS = "Water Rights...", __BUTTON_RETURN_FLOW = "Return Flow ...", __BUTTON_DEPLETION = "Depletion ...", __BUTTON_HELP = "Help", __BUTTON_CLOSE = "Close", __BUTTON_CANCEL = "Cancel", __BUTTON_APPLY = "Apply";

	/// <summary>
	/// Whether the gui data is editable or not.
	/// </summary>
	private bool __editable = false;

	/// <summary>
	/// Button group for selecting between efficiency types.
	/// </summary>
	private ButtonGroup __effCheckboxGroup;
	/// <summary>
	/// Button group for selecting what to search the jworksheet for.
	/// </summary>
	private ButtonGroup __searchCriteriaGroup;

	/// <summary>
	/// The index in __disables[] of textfields that should NEVER be made editable (e.g., ID fields).
	/// </summary>
	private int[] __textUneditables;

	/// <summary>
	/// Index of the currently-selected well.
	/// </summary>
	private int __currentWellIndex;
	/// <summary>
	/// Index of the previously-selected well.
	/// </summary>
	private int __lastWellIndex;

	/// <summary>
	/// GUI buttons.
	/// </summary>
	private JButton __findNextWell, __waterRightsJButton, __returnFlowInformationJButton, __depletionInformationJButton, __helpJButton, __closeJButton, __applyJButton, __cancelJButton, __makeViewFrame, __showOnMap_JButton = null, __showOnNetwork_JButton = null;

	/// <summary>
	/// Buttons to determine how to view the selected time series.
	/// </summary>
	private SimpleJButton __graph_JButton = null, __summary_JButton = null, __table_JButton = null;

	/// <summary>
	/// GUI check boxes.
	/// </summary>
	private JCheckBox __wellPumpMonthlyTS, __wellPumpDailyTS, __wellPumpEstDailyTS, __demandsMonthlyTS, __demandsDailyTS, __demandsEstDailyTS;

	/// <summary>
	/// Array of JComponents that should be disabled when nothing is selected from the list.
	/// </summary>
	private JComponent[] __disables;

	/// <summary>
	/// Status bar textfields 
	/// </summary>
	private JTextField __messageJTextField, __statusJTextField;

	/// <summary>
	/// Combo box for selecting the type of view to generate.
	/// </summary>
	private SimpleJComboBox __viewtype;

	/// <summary>
	/// GUI data combo boxes.
	/// </summary>
	private SimpleJComboBox __wellSwitch_SimpleJComboBox, __useTypeSimpleJComboBox, __dataTypeSwitch_SimpleJComboBox, __demandSrcCodeSimpleJComboBox;

	/// <summary>
	/// GUI data radio buttons for selecting among efficiencies.
	/// </summary>
	private JRadioButton __effConstant, __effMonthly;

	/// <summary>
	/// Radio buttons for selecting the field on which to search through the JWorksheet.
	/// </summary>
	private JRadioButton __searchIDJRadioButton, __searchNameJRadioButton;

	/// <summary>
	/// GUI text fields.
	/// </summary>
	private JTextField __searchID, __searchName, __stationIDJTextField, __nameJTextField, __locationJTextField, __capacityJTextField, __supplyJTextField, __areaJTextField;

	/// <summary>
	/// Array of text fields for efficiency data.
	/// </summary>
	private JTextField[] __monthlyEff;

	/// <summary>
	/// Worksheet for displaying well information.
	/// </summary>
	private JWorksheet __worksheet;

	/// <summary>
	/// Combo boxes.
	/// </summary>
	private SimpleJComboBox __dailyIDComboBox, __associatedDiversionsComboBox, __supplyComboBox;

	/// <summary>
	/// Dataset containing the well data.
	/// </summary>
	private StateMod_DataSet __dataset;

	/// <summary>
	/// Data set window manager.
	/// </summary>
	private StateMod_DataSet_WindowManager __dataset_wm;

	/// <summary>
	/// DataSetComponent containing well data.
	/// </summary>
	private DataSetComponent __wellComponent;

	/// <summary>
	/// List of wells data in the DataSetComponent.
	/// </summary>
	private IList<StateMod_Well> __wellsVector;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="dataset"> dataset containing the well data </param>
	/// <param name="dataset_wm"> the dataset window manager or null if the data set windows are not being managed. </param>
	/// <param name="editable"> whether the gui is editable or not </param>
	public StateMod_Well_JFrame(StateMod_DataSet dataset, StateMod_DataSet_WindowManager dataset_wm, bool editable)
	{
		StateMod_GUIUtil.setTitle(this, dataset, "Wells", null);
		JGUIUtil.setIcon(this, JGUIUtil.getIconImage());
		__currentWellIndex = -1;

		__dataset = dataset;
		__dataset_wm = dataset_wm;
		__wellComponent = __dataset.getComponentForComponentType(StateMod_DataSet.COMP_WELL_STATIONS);
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<StateMod_Well> wellsVector0 = (java.util.List<StateMod_Well>)__wellComponent.getData();
		IList<StateMod_Well> wellsVector0 = (IList<StateMod_Well>)__wellComponent.getData();
		__wellsVector = wellsVector0;

		int size = __wellsVector.Count;
		StateMod_Well well = null;
		for (int i = 0; i < size; i++)
		{
			well = (StateMod_Well)__wellsVector[i];
			well.createBackup();
		}

		__editable = editable;

		setupGUI(0);
	}

	/// <summary>
	/// Constructor. </summary>
	/// <param name="dataset"> dataset containing the well data. </param>
	/// <param name="dataset_wm"> the dataset window manager or null if the data set windows are not being managed. </param>
	/// <param name="well"> the well to select from the list. </param>
	/// <param name="editable"> whether the data are editable or not. </param>
	public StateMod_Well_JFrame(StateMod_DataSet dataset, StateMod_DataSet_WindowManager dataset_wm, StateMod_Well well, bool editable)
	{
		JGUIUtil.setIcon(this, JGUIUtil.getIconImage());
		StateMod_GUIUtil.setTitle(this, dataset, "Wells", null);
		__currentWellIndex = -1;

		__dataset = dataset;
		__dataset_wm = dataset_wm;
		__wellComponent = __dataset.getComponentForComponentType(StateMod_DataSet.COMP_WELL_STATIONS);
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<StateMod_Well> wellsVector0 = (java.util.List<StateMod_Well>)__wellComponent.getData();
		IList<StateMod_Well> wellsVector0 = (IList<StateMod_Well>)__wellComponent.getData();
		__wellsVector = wellsVector0;

		int size = __wellsVector.Count;
		StateMod_Well w = null;
		for (int i = 0; i < size; i++)
		{
			w = (StateMod_Well)__wellsVector[i];
			w.createBackup();
		}

		string id = well.getID();
		int index = StateMod_Util.IndexOf(__wellsVector, id);

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

		if (source == __effMonthly)
		{
			setMonthlyEff(__currentWellIndex);
		}
		else if (source == __effConstant)
		{
			setConstantEff(__currentWellIndex);
		}
		if (action.Equals(__BUTTON_HELP))
		{
			// TODO HELP (JTS - 2003-06-10
		}
		else if (action.Equals(__BUTTON_CLOSE))
		{
			saveCurrentRecord();
			int size = __wellsVector.Count;
			StateMod_Well well = null;
			bool changed = false;
			for (int i = 0; i < size; i++)
			{
				well = (StateMod_Well)__wellsVector[i];
				if (!changed && well.changed())
				{
					Message.printStatus(1, "", "Changed: " + i);
					changed = true;
				}
				well.acceptChanges();
			}
			if (changed)
			{
				__dataset.setDirty(StateMod_DataSet.COMP_WELL_STATIONS, true);
			}

			if (__dataset_wm != null)
			{
				__dataset_wm.closeWindow(StateMod_DataSet_WindowManager.WINDOW_WELL);
			}
			else
			{
				JGUIUtil.close(this);
			}
		}
		else if (action.Equals(__BUTTON_APPLY))
		{
			saveCurrentRecord();
			int size = __wellsVector.Count;
			StateMod_Well well = null;
			bool changed = false;
			for (int i = 0; i < size; i++)
			{
				well = (StateMod_Well)__wellsVector[i];
				if (!changed && well.changed())
				{
					changed = true;
				}
				well.createBackup();
			}
			if (changed)
			{
				__dataset.setDirty(StateMod_DataSet.COMP_WELL_STATIONS, true);
			}
		}
		else if (action.Equals(__BUTTON_CANCEL))
		{
			int size = __wellsVector.Count;
			StateMod_Well well = null;
			for (int i = 0; i < size; i++)
			{
				well = (StateMod_Well)__wellsVector[i];
				well.restoreOriginal();
			}

			if (__dataset_wm != null)
			{
				__dataset_wm.closeWindow(StateMod_DataSet_WindowManager.WINDOW_WELL);
			}
			else
			{
				JGUIUtil.close(this);
			}
		}
		// Time series buttons...
		else if ((e.getSource() == __graph_JButton) || (e.getSource() == __table_JButton) || (e.getSource() == __summary_JButton))
		{
			displayTSViewJFrame(e.getSource());
		}
		else if (source == __findNextWell)
		{
			searchWorksheet(__worksheet.getSelectedRow() + 1);
		}
		else if (source == __searchID || source == __searchName)
		{
			searchWorksheet();
		}
		else if (source == __showOnMap_JButton)
		{
			GeoRecord geoRecord = getSelectedWell().getGeoRecord();
			GRShape shape = geoRecord.getShape();
			__dataset_wm.showOnMap(getSelectedWell(), "Well: " + getSelectedWell().getID() + " - " + getSelectedWell().getName(), new GRLimits(shape.xmin, shape.ymin, shape.xmax, shape.ymax), geoRecord.getLayer().getProjection());
		}
		else if (source == __showOnNetwork_JButton)
		{
			StateMod_Network_JFrame networkEditor = __dataset_wm.getNetworkEditor();
			if (networkEditor != null)
			{
				HydrologyNode node = networkEditor.getNetworkJComponent().findNode(getSelectedWell().getID(), false, false);
				if (node != null)
				{
					__dataset_wm.showOnNetwork(getSelectedWell(), "Well: " + getSelectedWell().getID() + " - " + getSelectedWell().getName(), new GRLimits(node.getX(),node.getY(),node.getX(),node.getY()));
				}
			}
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
		else if (source == __supplyComboBox)
		{
			if (__supplyComboBox.getSelected().Equals(__SUPPLY_0))
			{
				__supplyJTextField.setText("");
				__supplyJTextField.setEditable(false);
				__supplyJTextField.setEnabled(false);
			}
			else
			{
				__supplyJTextField.setEditable(true);
				__supplyJTextField.setEnabled(true);
			}
		}
		else
		{
			if (__currentWellIndex == -1)
			{
				new ResponseJDialog(this, "You must first select a well from the list.", ResponseJDialog.OK);
				return;
			}
			// set placeholder to current well
			StateMod_Well well = (StateMod_Well)__wellsVector[__currentWellIndex];

			// spreadsheet requests ...
			if (action.Equals(__BUTTON_WATER_RIGHTS))
			{
				new StateMod_Well_Right_JFrame(__dataset, well,__editable);
			}
			else if (action.Equals(__BUTTON_RETURN_FLOW))
			{
				new StateMod_Well_ReturnFlow_JFrame(__dataset, well, __editable);
			}
			else if (action.Equals(__BUTTON_DEPLETION))
			{
				new StateMod_Well_Depletion_JFrame(__dataset, well, __editable);
			}
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
		string label = "The following error" + plural + " encountered trying to save the record:\n";
		for (int i = 0; i < errorCount; i++)
		{
			label += errors[i] + "\n";
		}
		new ResponseJDialog(this, "Errors encountered", label, ResponseJDialog.OK);
		return false;
	}

	/// <summary>
	/// Enables/disables the time series display buttons appropriately based on the
	/// selected in the time seriesJCheckBoxes.
	/// </summary>
	private void checkTimeSeriesButtonsStates()
	{
		bool enabled = false;

		if ((__wellPumpMonthlyTS.isSelected() && __wellPumpMonthlyTS.isEnabled()) || (__wellPumpDailyTS.isSelected() && __wellPumpDailyTS.isEnabled()) || (__wellPumpEstDailyTS.isSelected() && __wellPumpEstDailyTS.isEnabled()) || (__demandsMonthlyTS.isSelected() && __demandsMonthlyTS.isEnabled()) || (__demandsDailyTS.isSelected() && __demandsDailyTS.isEnabled()) || (__demandsEstDailyTS.isSelected() && __demandsEstDailyTS.isEnabled()))
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
		StateMod_Well well = getSelectedWell();
		if (well.getGeoRecord() == null)
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

		StateMod_Well well = (StateMod_Well)__wellsVector[__currentWellIndex];

		// display_props.set("HelpKey", "TSTool.ExportMenu");
		display_props.set("TSViewTitleString", StateMod_Util.createDataLabel(well,true) + " Time Series");
		display_props.set("DisplayFont", "Courier");
		display_props.set("DisplaySize", "11");
		display_props.set("PrintFont", "Courier");
		display_props.set("PrintSize", "7");
		display_props.set("PageLength", "100");

		PropList props = new PropList("Reservoir");
		props.set("Product.TotalWidth", "600");
		props.set("Product.TotalHeight", "400");

		IList<TS> tslist = new List<TS>();

		int sub = 0;
		int its = 0;
		TS ts = null;

		if ((__wellPumpMonthlyTS.isSelected() && (well.getPumpingMonthTS() != null)) || (__demandsMonthlyTS.isSelected() && (well.getDemandMonthTS() != null)))
		{
			// Do the monthly graph...
			++sub;
			its = 0;
			props.set("SubProduct " + sub + ".GraphType=Line");
			props.set("SubProduct " + sub + ".SubTitleString=Monthly Data for Well " + well.getID() + " (" + well.getName() + ")");
			props.set("SubProduct " + sub + ".SubTitleFontSize=12");
			ts = well.getPumpingMonthTS();
			if (ts != null)
			{
				props.set("Data " + sub + "." + (++its) + ".TSID=" + ts.getIdentifierString());
				tslist.Add(ts);
			}
			ts = well.getDemandMonthTS();
			if (ts != null)
			{
				props.set("Data " + sub + "." + (++its) + ".TSID=" + ts.getIdentifierString());
				tslist.Add(ts);
			}
		}

		// TODO - need to add estimated daily demands and estimated historical...
		if ((__wellPumpDailyTS.isSelected() && (well.getPumpingDayTS() != null)) || (__demandsDailyTS.isSelected() && (well.getDemandDayTS() != null)))
		{
			// Do the daily graph...
			++sub;
			its = 0;
			props.set("SubProduct " + sub + ".GraphType=Line");
			props.set("SubProduct " + sub + ".SubTitleString=Daily Data for Well " + well.getID() + " (" + well.getName() + ")");
			props.set("SubProduct " + sub + ".SubTitleFontSize=12");
			ts = well.getPumpingDayTS();
			if (ts != null)
			{
				props.set("Data " + sub + "." + (++its) + ".TSID=" + ts.getIdentifierString());
				tslist.Add(ts);
			}
			ts = well.getDemandDayTS();
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
			Message.printWarning(1,routine,"Error displaying time series (" + e + " ).");
			Message.printWarning(2, routine, e);
		}
	}

	/// <summary>
	/// Clean up before garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~StateMod_Well_JFrame()
	{
		__worksheet = null;

		__stationIDJTextField = null;
		__nameJTextField = null;
		__locationJTextField = null;
		__capacityJTextField = null;
		__dailyIDComboBox = null;
		__associatedDiversionsComboBox = null;
		__useTypeSimpleJComboBox = null;
		__demandSrcCodeSimpleJComboBox = null;
		__areaJTextField = null;
		__wellSwitch_SimpleJComboBox = null;
		__dataTypeSwitch_SimpleJComboBox = null;
		__graph_JButton = null;
		__table_JButton = null;
		__summary_JButton = null;

		__effCheckboxGroup = null;
		__effConstant = null;
		__effMonthly = null;
		__monthlyEff = null;

		__searchID = null;
		__searchName = null;
		__findNextWell = null;
		__searchCriteriaGroup = null;
		__searchIDJRadioButton = null;
		__searchNameJRadioButton = null;

		__waterRightsJButton = null;
		__wellPumpMonthlyTS = null;
		__demandsMonthlyTS = null;
		__demandsDailyTS = null;
		__wellPumpDailyTS = null;
		__demandsEstDailyTS = null;
		__returnFlowInformationJButton = null;
		__depletionInformationJButton = null;

		__helpJButton = null;
		__closeJButton = null;

//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Get the selected divwellersion, based on the current index in the list.
	/// </summary>
	private StateMod_Well getSelectedWell()
	{
		return __wellsVector[__currentWellIndex];
	}

	/// <summary>
	/// Initializes the arrays that are used when items are selected and deselected.
	/// This should be called from setupGUI() before the a call is made to selectTableIndex().
	/// </summary>
	private void initializeDisables()
	{
		__disables = new JComponent[39];
		int i = 0;
		__disables[i++] = __stationIDJTextField;
		__disables[i++] = __waterRightsJButton;
		__disables[i++] = __wellPumpMonthlyTS;
		__disables[i++] = __demandsMonthlyTS;
		__disables[i++] = __demandsDailyTS;
		__disables[i++] = __wellPumpDailyTS;
		__disables[i++] = __returnFlowInformationJButton;
		__disables[i++] = __depletionInformationJButton;
		__disables[i++] = __applyJButton;
		__disables[i++] = __wellSwitch_SimpleJComboBox;
		__disables[i++] = __useTypeSimpleJComboBox;
		__disables[i++] = __dataTypeSwitch_SimpleJComboBox;
		__disables[i++] = __demandSrcCodeSimpleJComboBox;
		__disables[i++] = __effConstant;
		__disables[i++] = __effMonthly;
		__disables[i++] = __nameJTextField;
		__disables[i++] = __locationJTextField;
		__disables[i++] = __capacityJTextField;
		__disables[i++] = __dailyIDComboBox;
		__disables[i++] = __associatedDiversionsComboBox;
		__disables[i++] = __areaJTextField;
		__disables[i++] = __supplyComboBox;
		__disables[i++] = __supplyJTextField;
		__disables[i++] = __graph_JButton;
		__disables[i++] = __table_JButton;
		__disables[i++] = __summary_JButton;
		__disables[i++] = __demandsEstDailyTS;
		for (int j = 0; j < 12; j++)
		{
			__disables[i + j] = __monthlyEff[j];
		}

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
	/// Populates the well daily id combo box.
	/// </summary>
	private void populateWellDailyID()
	{
		__dailyIDComboBox.removeAllItems();

		IList<string> idNameVector = StateMod_Util.createIdentifierListFromStateModData(__wellsVector,true,null);
		idNameVector.Insert(0, "0 - Use average daily value from monthly time series");
		idNameVector.Insert(1, "3 - Daily time series are supplied");
		idNameVector.Insert(2, "4 - Daily time series interpolated from midpoints of monthly data");

		DefaultComboBoxModel<string> dcbm = new DefaultComboBoxModel<string>(new List<string>(idNameVector));
		__dailyIDComboBox.setModel(dcbm);
	}

	/// <summary>
	/// Processes a table selection (either via a mouse press or programmatically 
	/// from selectTableIndex() by writing the old data back to the data set component
	/// and getting the next selection's data out of the data and displaying it on the form. </summary>
	/// <param name="index"> the index of the reservoir to display on the form. </param>
	private void processTableSelection(int index)
	{
		string routine = "StateMod_Well_JFrame.processTableSelection";

		__lastWellIndex = __currentWellIndex;
		__currentWellIndex = __worksheet.getOriginalRowNumber(index);

		saveLastRecord();

		if (__worksheet.getSelectedRow() == -1)
		{
			JGUIUtil.disableComponents(__disables, true);
			return;
		}

		JGUIUtil.enableComponents(__disables, __textUneditables, __editable);
		checkTimeSeriesButtonsStates();

		StateMod_Well well = (StateMod_Well)__wellsVector[__currentWellIndex];

		__stationIDJTextField.setText(well.getID());
		__nameJTextField.setText(well.getName());
		__locationJTextField.setText(well.getCgoto());
		StateMod_GUIUtil.checkAndSet(well.getDivcapw(), __capacityJTextField);

		int irturnw = well.getIrturnw();
		if (irturnw == 1)
		{
			__useTypeSimpleJComboBox.select(0);
		}
		else if (irturnw == 2)
		{
			__useTypeSimpleJComboBox.select(1);
		}
		else if (irturnw == 3)
		{
			__useTypeSimpleJComboBox.select(2);
		}
		else if (irturnw == 4)
		{
			__useTypeSimpleJComboBox.select(3);
		}

		int demsrc = well.getDemsrcw();
		if (demsrc == 1)
		{
			__demandSrcCodeSimpleJComboBox.select(0);
		}
		else if (demsrc == 2)
		{
			__demandSrcCodeSimpleJComboBox.select(1);
		}
		else if (demsrc == 3)
		{
			__demandSrcCodeSimpleJComboBox.select(2);
		}
		else if (demsrc == 4)
		{
			__demandSrcCodeSimpleJComboBox.select(3);
		}
		else if (demsrc == 5)
		{
			__demandSrcCodeSimpleJComboBox.select(4);
		}
		else if (demsrc == 6)
		{
			__demandSrcCodeSimpleJComboBox.select(5);
		}
		else if (demsrc == 7)
		{
			__demandSrcCodeSimpleJComboBox.select(6);
		}
		else if (demsrc == 8)
		{
			__demandSrcCodeSimpleJComboBox.select(7);
		}
		else if (demsrc == -999)
		{
			__demandSrcCodeSimpleJComboBox.select(8);
		}

		StateMod_GUIUtil.checkAndSet(well.getAreaw(), __areaJTextField);

		// For checkboxes, do not change the state of the checkbox, only
		// whether enabled - that way if the user has picked a combination of
		// parameters it is easy for them to keep the same settings when
		// switching between stations.  Make sure to do the following after
		// the generic enable/disable code is called above!

		if (well.getPumpingMonthTS() != null)
		{
			__wellPumpMonthlyTS.setEnabled(true);
		}
		else
		{
			__wellPumpMonthlyTS.setEnabled(false);
		}

		if (well.getPumpingDayTS() != null)
		{
			__wellPumpDailyTS.setEnabled(true);
		}
		else
		{
			__wellPumpDailyTS.setEnabled(false);
		}

		if (well.getDemandMonthTS() != null)
		{
			__demandsMonthlyTS.setEnabled(true);
		}
		else
		{
			__demandsMonthlyTS.setEnabled(false);
		}

		if (well.getDemandDayTS() != null)
		{
			__demandsDailyTS.setEnabled(true);
		}
		else
		{
			__demandsDailyTS.setEnabled(false);
		}

		/* TODO - need to check daily ID and verify that appropriate time
		series are available to estimate...
		if ( div.getDemandDayTS() != null ) {
			__demandsEstDailyTS.setEnabled(true);
		}
		else {
			__demandsEstDailyTS.setEnabled(false);
		}
		*/
		__demandsEstDailyTS.setEnabled(false);

		// switch
		if (well.getSwitch() == 1)
		{
			__wellSwitch_SimpleJComboBox.select("1 - On");
		}
		else
		{
			__wellSwitch_SimpleJComboBox.select("0 - Off");
		}

		// idvcomw(data type switch)
		int idvcomw = well.getIdvcomw();
		if (idvcomw == 1)
		{
			__dataTypeSwitch_SimpleJComboBox.select(__DATA_TYPE_MONTHLY);
		}
		else if (idvcomw == 2)
		{
			__dataTypeSwitch_SimpleJComboBox.select(__DATA_TYPE_ANNUAL);
		}
		else if (idvcomw == 5)
		{
			__dataTypeSwitch_SimpleJComboBox.select(__DATA_TYPE_ESTIMATE_ZERO);
		}
		else if (idvcomw == 6)
		{
			__dataTypeSwitch_SimpleJComboBox.select(__DATA_TYPE_DIRECT_DIV);
		}

		// effeciency switch
		if (well.getDivefcw() < 0)
		{
			setMonthlyEff(index);
		}
		else
		{
			setConstantEff(index);
		}

		if (__lastWellIndex != -1)
		{
			string s = __dailyIDComboBox.getStringAt(3);
			__dailyIDComboBox.removeAt(3);
			__dailyIDComboBox.addAlpha(s, 3);
		}
		string s = "" + well.getID() + " - " + well.getName();
		__dailyIDComboBox.remove(s);
		__dailyIDComboBox.addAt(s, 3);

		string c = well.getCdividyw();
		if (c.Trim().Equals(""))
		{
			if (!__dailyIDComboBox.setSelectedPrefixItem(c))
			{
				Message.printWarning(2, routine, "No matching Cdividyw  value to '" + c + "' found " + "in combo box.");
				__dailyIDComboBox.select(0);
			}
			setOriginalCdividyw(well, "" + c);
		}
		else
		{
			if (!__dailyIDComboBox.setSelectedPrefixItem(c))
			{
				Message.printWarning(2, routine, "No Cdividyw value matching '" + c + "' found in combo box.");
				__dailyIDComboBox.select(0);
			}
		}

		if (well.getPrimary() == 0)
		{
			__supplyComboBox.select(__SUPPLY_0);
			__supplyJTextField.setText("");
			__supplyJTextField.setEnabled(false);
			__supplyJTextField.setEditable(false);
		}
		else
		{
			__supplyComboBox.select(__SUPPLY_ELSE);
			__supplyJTextField.setEditable(true);
			__supplyJTextField.setEnabled(true);
			StateMod_GUIUtil.checkAndSet(well.getPrimary(), __supplyJTextField);
		}

		if (!__associatedDiversionsComboBox.setSelectedPrefixItem(well.getIdvcow2()))
		{
			Message.printWarning(2, routine, "No Idvcow2 value matching '" + well.getIdvcow2() + "' found in combo box.");
			__associatedDiversionsComboBox.add(well.getIdvcow2());
			__associatedDiversionsComboBox.select(well.getIdvcow2());
		}

		checkViewButtonState();
	}

	/// <summary>
	/// Saves the prior record selected in the table; called when moving to a new record by a table selection.
	/// </summary>
	private void saveLastRecord()
	{
		saveInformation(__lastWellIndex);
	}

	/// <summary>
	/// Saves the current record selected in the table; called when the window is closed or minimized or apply is pressed.
	/// </summary>
	private void saveCurrentRecord()
	{
		saveInformation(__currentWellIndex);
	}

	/// <summary>
	/// Saves the information associated with the currently-selected well.
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

		StateMod_Well well = (StateMod_Well)__wellsVector[record];

		well.setName(__nameJTextField.getText());
		well.setCgoto(__locationJTextField.getText());
		well.setDivcapw(__capacityJTextField.getText());
		well.setIrturnw(__useTypeSimpleJComboBox.getSelectedIndex() + 1);

		int demsrc = __demandSrcCodeSimpleJComboBox.getSelectedIndex();
		if (demsrc == 0)
		{
			demsrc = 1;
		}
		else if (demsrc == 1)
		{
			demsrc = 2;
		}
		else if (demsrc == 2)
		{
			demsrc = 3;
		}
		else if (demsrc == 3)
		{
			demsrc = 4;
		}
		else if (demsrc == 4)
		{
			demsrc = 5;
		}
		else if (demsrc == 5)
		{
			demsrc = 6;
		}
		else if (demsrc == 6)
		{
			demsrc = 7;
		}
		else if (demsrc == 7)
		{
			demsrc = 8;
		}
		else if (demsrc == 8)
		{
			demsrc = -999;
		}

		well.setDemsrcw(demsrc);

		well.setAreaw(__areaJTextField.getText());

		// if not enabled, idvcomw has been saved in itemStateChanged
		well.setSwitch(__wellSwitch_SimpleJComboBox.getSelectedIndex());

		string idvcom = __dataTypeSwitch_SimpleJComboBox.getSelected();
		if (idvcom.Equals(__DATA_TYPE_MONTHLY))
		{
			well.setIdvcomw(1);
		}
		else if (idvcom.Equals(__DATA_TYPE_ANNUAL))
		{
			well.setIdvcomw(2);
		}
		else if (idvcom.Equals(__DATA_TYPE_ESTIMATE_ZERO))
		{
			well.setIdvcomw(5);
		}
		else if (idvcom.Equals(__DATA_TYPE_DIRECT_DIV))
		{
			well.setIdvcomw(6);
		}

		saveEfficiencies(well);

		string cdividyw = __dailyIDComboBox.getSelected();
		int index = cdividyw.IndexOf(" - ", StringComparison.Ordinal);
		if (index > -1)
		{
			cdividyw = cdividyw.Substring(0, index);
		}
		cdividyw = cdividyw.Trim();
		if (!cdividyw.Equals(well.getCdividyw(), StringComparison.OrdinalIgnoreCase))
		{
			well.setCdividyw(cdividyw);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<RTi.TS.DayTS> dailyWellDemandTSVector = (java.util.List<RTi.TS.DayTS>)(__dataset.getComponentForComponentType(StateMod_DataSet.COMP_WELL_DEMAND_TS_DAILY).getData());
		IList<DayTS> dailyWellDemandTSVector = (IList<DayTS>)(__dataset.getComponentForComponentType(StateMod_DataSet.COMP_WELL_DEMAND_TS_DAILY).getData());
		well.connectDemandDayTS(dailyWellDemandTSVector);

		if (__supplyComboBox.getSelected().Equals(__SUPPLY_0))
		{
			well.setPrimary(0);
		}
		else
		{
			well.setPrimary(__supplyJTextField.getText());
		}

		string ad = __associatedDiversionsComboBox.getSelected();
		index = ad.IndexOf(" - ", StringComparison.Ordinal);
		if (index > -1)
		{
			ad = ad.Substring(0, index);
		}
		well.setIdvcow2(ad);
	}

	/// <summary>
	/// Saves the efficiency information back into the object.
	/// </summary>
	private void saveEfficiencies(StateMod_Well well)
	{
		if (__effMonthly.isSelected())
		{
			for (int i = 0; i < 12; i++)
			{
				well.setDiveff(i, __monthlyEff[i].getText());
			}
			//well.setDivefcw(-1);
		}
		else
		{
			well.setDivefcw(__monthlyEff[0].getText());
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
	/// Sets the monthly eff for the given record </summary>
	/// <param name="record"> record number of the well for which to set the eff </param>
	internal virtual void setMonthlyEff(int record)
	{
		if (__currentWellIndex == -1)
		{
			return;
		}

		StateMod_Well well = (StateMod_Well)__wellsVector[record];
		__effMonthly.setSelected(true);
		double efc = well.getDivefcw();
		if (efc > 0)
		{
			efc = -1 * efc;
		}
		//well.setDivefcw(efc);

		for (int i = 0; i < 12; i++)
		{
			StateMod_GUIUtil.checkAndSet(well.getDiveff(i), __monthlyEff[i]);
			if (i > 0)
			{
				__monthlyEff[i].setEditable(true);
			}
		}
	}

	/// <summary>
	/// Sets the constant eff for the given record </summary>
	/// <param name="record"> record number of the well for which to set the eff </param>
	internal virtual void setConstantEff(int record)
	{
		if (__currentWellIndex == -1)
		{
			return;
		}

		StateMod_Well well = (StateMod_Well)__wellsVector[record];
		__effConstant.setSelected(true);
		double efc = well.getDivefcw();
		if (efc < 0)
		{
			efc = -1 * efc;
		}
		// well.setDivefcw(efc);

		for (int i = 0; i < 12; i++)
		{
			StateMod_GUIUtil.checkAndSet(efc, __monthlyEff[i]);

			if (i > 0)
			{
				__monthlyEff[i].setEditable(false);
			}
		}
	}

	/// <summary>
	/// Selects the desired ID in the table and displays the appropriate data in the remainder of the window. </summary>
	/// <param name="id"> the identifier to select in the list. </param>
	public virtual void selectID(string id)
	{
		int rows = __worksheet.getRowCount();
		StateMod_Well well = null;
		for (int i = 0; i < rows; i++)
		{
			well = (StateMod_Well)__worksheet.getRowData(i);
			if (well.getID().Trim().Equals(id.Trim()))
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
	/// <param name="index"> the index of the well to be initially selected </param>
	private void setupGUI(int index)
	{
		StopWatch sw = new StopWatch();
		sw.clear();
		sw.start();
		string routine = "setupGUI";

		addWindowListener(this);

		JPanel p1 = new JPanel(); // first 6 months' efficiency
		JPanel p3 = new JPanel(); // div sta id -> switch for diversion

		JPanel left_panel = new JPanel(); // multilist and search area
		JPanel right_panel = new JPanel(); // everything else

		__stationIDJTextField = new JTextField(12);
		__nameJTextField = new JTextField(24);
		__locationJTextField = new JTextField(12);
		__capacityJTextField = new JTextField(11);
		__areaJTextField = new JTextField(10);
		__supplyJTextField = new JTextField(8);

		__associatedDiversionsComboBox = new SimpleJComboBox();
		__associatedDiversionsComboBox.add("N/A - Not associated with diversion");

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<StateMod_Diversion> diversions = (java.util.List<StateMod_Diversion>)(__dataset.getComponentForComponentType(StateMod_DataSet.COMP_DIVERSION_STATIONS).getData());
		IList<StateMod_Diversion> diversions = (IList<StateMod_Diversion>)(__dataset.getComponentForComponentType(StateMod_DataSet.COMP_DIVERSION_STATIONS).getData());
		IList<string> diversionIds = StateMod_Util.createIdentifierListFromStateModData(diversions, true, null);
		DefaultComboBoxModel<string> dcbm = new DefaultComboBoxModel<string>(new List<string>(diversionIds));
		__associatedDiversionsComboBox.setModel(dcbm);
		/*
		for (int i = 0; i < size; i++) {
			__associatedDiversionsComboBox.add(	(String)diversions.elementAt(i));
		}
		*/

		__supplyComboBox = new SimpleJComboBox();
		__supplyComboBox.addActionListener(this);
		__supplyComboBox.add(__SUPPLY_0);
		__supplyComboBox.add(__SUPPLY_ELSE);

		__dailyIDComboBox = new SimpleJComboBox();

		__effCheckboxGroup = new ButtonGroup();
		__effMonthly = new JRadioButton("Monthly efficiency  ", false);
		__effMonthly.addActionListener(this);
		__effConstant = new JRadioButton("Constant efficiency", true);
		__effConstant.addActionListener(this);
		__effCheckboxGroup.add(__effMonthly);
		__effCheckboxGroup.add(__effConstant);

		__monthlyEff = new JTextField[12];
		for (int i = 0; i < 12; i++)
		{
			__monthlyEff[i] = new JTextField(5);
			__monthlyEff[i].addActionListener(this);
		}
		__wellSwitch_SimpleJComboBox = new SimpleJComboBox();
		__wellSwitch_SimpleJComboBox.add("0 - Off");
		__wellSwitch_SimpleJComboBox.add("1 - On");

		__dataTypeSwitch_SimpleJComboBox = new SimpleJComboBox();
		__dataTypeSwitch_SimpleJComboBox.add(__DATA_TYPE_MONTHLY);
		__dataTypeSwitch_SimpleJComboBox.add(__DATA_TYPE_ANNUAL);
		__dataTypeSwitch_SimpleJComboBox.add(__DATA_TYPE_ESTIMATE_ZERO);
		__dataTypeSwitch_SimpleJComboBox.add(__DATA_TYPE_DIRECT_DIV);

		__useTypeSimpleJComboBox = new SimpleJComboBox();
		__useTypeSimpleJComboBox.add(__USE_INBASIN1);
		__useTypeSimpleJComboBox.add(__USE_INBASIN2);
		__useTypeSimpleJComboBox.add(__USE_INBASIN3);
		__useTypeSimpleJComboBox.add(__USE_TRANSMOUNTAIN);

		__demandSrcCodeSimpleJComboBox = new SimpleJComboBox();
		__demandSrcCodeSimpleJComboBox.add(__DEMSRC_IRR_GIS);
		__demandSrcCodeSimpleJComboBox.add(__DEMSRC_IRR_TIA);
		__demandSrcCodeSimpleJComboBox.add(__DEMSRC_IRR_MULT_GIS);
		__demandSrcCodeSimpleJComboBox.add(__DEMSRC_GIS_TIA);
		__demandSrcCodeSimpleJComboBox.add(__DEMSRC_IRR_MULT_GIS_SEC);
		__demandSrcCodeSimpleJComboBox.add(__DEMSRC_MUNI_IND_XMTN);
		__demandSrcCodeSimpleJComboBox.add(__DEMSRC_CARRIER);
		__demandSrcCodeSimpleJComboBox.add(__DEMSRC_USER);
		__demandSrcCodeSimpleJComboBox.add(__DEMSRC_UNKNOWN);

		__searchID = new JTextField(10);
		__searchName = new JTextField(10);
		__searchName.setEditable(false);
		__findNextWell = new JButton(__BUTTON_FIND_NEXT);
		__searchCriteriaGroup = new ButtonGroup();
		__searchIDJRadioButton = new JRadioButton(__BUTTON_ID, true);
		__searchNameJRadioButton = new JRadioButton(__BUTTON_NAME, false);
		__searchCriteriaGroup.add(__searchIDJRadioButton);
		__searchCriteriaGroup.add(__searchNameJRadioButton);

		__returnFlowInformationJButton = new JButton(__BUTTON_RETURN_FLOW);
		__depletionInformationJButton = new JButton(__BUTTON_DEPLETION);

		__showOnMap_JButton = new SimpleJButton(__BUTTON_SHOW_ON_MAP, this);
		__showOnMap_JButton.setToolTipText("Annotate map with location (button is disabled if layer does not have matching ID)");
		__showOnNetwork_JButton = new SimpleJButton(__BUTTON_SHOW_ON_NETWORK, this);
		__showOnNetwork_JButton.setToolTipText("Annotate network with location");
		__applyJButton = new JButton(__BUTTON_APPLY);
		__cancelJButton = new JButton(__BUTTON_CANCEL);
		__helpJButton = new JButton(__BUTTON_HELP);
		__helpJButton.setEnabled(false);
		__closeJButton = new JButton(__BUTTON_CLOSE);

		GridBagLayout gb = new GridBagLayout();
		JPanel mainJPanel = new JPanel();
		mainJPanel.setLayout(gb);
		p1.setLayout(new GridLayout(4, 6, 2, 0));
		p3.setLayout(gb);
		right_panel.setLayout(gb);
		left_panel.setLayout(gb);

		int y;

		PropList p = new PropList("StateMod_Well_JFrame.JWorksheet");
		p.add("JWorksheet.ShowPopupMenu=true");
		p.add("JWorksheet.AllowCopy=true");
		p.add("JWorksheet.SelectionMode=SingleRowSelection");

		int[] widths = null;
		JScrollWorksheet jsw = null;
		try
		{
			StateMod_Well_TableModel tmw = new StateMod_Well_TableModel(__wellsVector, __editable);
			StateMod_Well_CellRenderer crw = new StateMod_Well_CellRenderer(tmw);

			jsw = new JScrollWorksheet(crw, tmw, p);
			__worksheet = jsw.getJWorksheet();
			widths = crw.getColumnWidths();
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
		__worksheet.addKeyListener(this);

		JGUIUtil.addComponent(left_panel, jsw, 0, 0, 6, 6, 1, 1, 0, 0, 1, 1, GridBagConstraints.BOTH, GridBagConstraints.CENTER);

		y = 0;
		JGUIUtil.addComponent(p3, new JLabel("Station ID:"), 0, y, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(p3, __stationIDJTextField, 1, y, 1, 1, 1, 1, 1, 0, 0, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);
		__stationIDJTextField.setEditable(false);

		y++;
		JGUIUtil.addComponent(p3, new JLabel("Name:"), 0, y, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(p3, __nameJTextField, 1, y, 1, 1, 1, 1, 1, 0, 0, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);

		y++;
		JGUIUtil.addComponent(p3, new JLabel("River node ID:"), 0, y, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(p3, __locationJTextField, 1, y, 1, 1, 1, 1, 1, 0, 0, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);

		y++;
		JGUIUtil.addComponent(p3, new JLabel("Daily data ID:"), 0, y, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(p3, __dailyIDComboBox, 1, y, 1, 1, 1, 1, 1, 0, 0, 1, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);

		y++;
		JGUIUtil.addComponent(p3, new JLabel("Capacity (CFS):"), 0, y, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(p3, __capacityJTextField, 1, y, 1, 1, 1, 1, 1, 0, 0, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);

		y++;
		JGUIUtil.addComponent(p3, new JLabel("On/off switch:"), 0, y, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(p3, __wellSwitch_SimpleJComboBox, 1, y, 1, 1, 1, 1, 1, 0, 0, 1, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);
		y++;

		JPanel supplyPanel = new JPanel();
		supplyPanel.setLayout(gb);
		JGUIUtil.addComponent(supplyPanel, __supplyComboBox, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(supplyPanel, __supplyJTextField, 1, 0, 1, 1, 0, 0, 0, 1, 0, 0, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);

		JGUIUtil.addComponent(p3, new JLabel("Senior supply:"), 0, y, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(p3, supplyPanel, 1, y, 1, 1, 1, 1, 1, 0, 0, 1, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);
		y++;
		JGUIUtil.addComponent(p3, new JLabel("Associated diversion:"), 0, y, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(p3, __associatedDiversionsComboBox, 1, y, 1, 1, 1, 1, 1, 0, 0, 1, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);

		y++;
		JGUIUtil.addComponent(p3, new JLabel("Use type:"), 0, y, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(p3, __useTypeSimpleJComboBox, 1, y, 1, 1, 1, 1, 1, 0, 0, 1, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);

		y++;
		JGUIUtil.addComponent(p3, new JLabel("Demand source:"), 0, y, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(p3, __demandSrcCodeSimpleJComboBox, 1, y, 1, 1, 1, 1, 1, 0, 0, 1, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);

		y++;
		JGUIUtil.addComponent(p3, new JLabel("Data type switch:"), 0, y, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(p3, __dataTypeSwitch_SimpleJComboBox, 1, y, 1, 1, 1, 1, 1, 0, 0, 1, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);
		y++;
		JGUIUtil.addComponent(p3, new JLabel("Irrigated acreage:"), 0, y, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(p3, __areaJTextField, 1, y, 1, 1, 1, 1, 1, 0, 0, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);

		// two top panels of info
		JGUIUtil.addComponent(right_panel, p3, 0, 0, 2, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.NORTHWEST);
		// effeciency information
		y = 4;
		p1.add(new JLabel("Oct"));
		p1.add(new JLabel("Nov"));
		p1.add(new JLabel("Dec"));
		p1.add(new JLabel("Jan"));
		p1.add(new JLabel("Feb"));
		p1.add(new JLabel("Mar"));
		for (int i = 0; i < 6; i++)
		{
			p1.add(__monthlyEff[i]);
		}
		p1.add(new JLabel("Apr"));
		p1.add(new JLabel("May"));
		p1.add(new JLabel("Jun"));
		p1.add(new JLabel("Jul"));
		p1.add(new JLabel("Aug"));
		p1.add(new JLabel("Sep"));
		for (int i = 6; i < 12; i++)
		{
			p1.add(__monthlyEff[i]);
		}

		JPanel eff_panel = new JPanel();
		eff_panel.setLayout(gb);
		JPanel radio_panel = new JPanel();
		radio_panel.setLayout(gb);

		JGUIUtil.addComponent(radio_panel, __effConstant, 0, 0, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(radio_panel, __effMonthly, 0, 1, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(eff_panel, radio_panel, 0, 0, 1, 4, 0, 0, 0, 0, 0, 0, GridBagConstraints.VERTICAL, GridBagConstraints.CENTER);
		JGUIUtil.addComponent(eff_panel, p1, 2, 0, 2, 4, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		eff_panel.setBorder(BorderFactory.createTitledBorder("System Efficiency"));

		// add search areas
		y = 7;
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
		JGUIUtil.addComponent(searchPanel, __findNextWell, 0, y2, 4, 1, 0, 0, 20, 10, 20, 10, GridBagConstraints.NONE, GridBagConstraints.WEST);
		__findNextWell.addActionListener(this);
		// add buttons which lead to well water rights,
		// direct flow demand, and return flow information
		y = 6;
		FlowLayout fl = new FlowLayout(FlowLayout.CENTER);
		JPanel p5 = new JPanel();
		p5.setLayout(new GridLayout(5, 2));

		JPanel subformsPanel = new JPanel();
		subformsPanel.setLayout(gb);
		subformsPanel.setBorder(BorderFactory.createTitledBorder("Related Data"));
		JPanel tsPanel = new JPanel();
		tsPanel.setLayout(gb);
		tsPanel.setBorder(BorderFactory.createTitledBorder("Time Series"));

		__waterRightsJButton = new JButton(__BUTTON_WATER_RIGHTS);

		__wellPumpMonthlyTS = new JCheckBox("Well Pumping (Historical Monthly)");
		__wellPumpMonthlyTS.addItemListener(this);
		__wellPumpDailyTS = new JCheckBox("Well Pumping (Historical Daily)");
		__wellPumpDailyTS.addItemListener(this);
		__wellPumpEstDailyTS = new JCheckBox("Well Pumping (Estimated Historical Daily)");
		__wellPumpEstDailyTS.addItemListener(this);
		__demandsMonthlyTS = new JCheckBox("Demands (Monthly)");
		__demandsMonthlyTS.addItemListener(this);
		__demandsDailyTS = new JCheckBox("Demands (Daily)");
		__demandsDailyTS.addItemListener(this);
		__demandsEstDailyTS = new JCheckBox("Demands (Estimated Daily)");
		__demandsEstDailyTS.addItemListener(this);

		y = 0;
		JGUIUtil.addComponent(tsPanel, __wellPumpMonthlyTS, 0, y, 1, 1, 1, 1, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(tsPanel, __wellPumpDailyTS, 0, ++y, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(tsPanel, __wellPumpEstDailyTS, 0, ++y, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(tsPanel, __demandsMonthlyTS, 0, ++y, 1, 1, 1, 1, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(tsPanel, __demandsDailyTS, 0, ++y, 1, 1, 1, 1, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(tsPanel, __demandsEstDailyTS, 0, ++y, 1, 1, 1, 1, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);

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
	/*		
		JGUIUtil.addComponent(tsPanel, __demandsEstDailyTS,
			0, ++y, 1, 1, 1, 1,
			0, 0, 0, 0, 
			GridBagConstraints.NONE, GridBagConstraints.CENTER);	
	*/

		JGUIUtil.addComponent(subformsPanel, __depletionInformationJButton, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.CENTER);
		JGUIUtil.addComponent(subformsPanel, __returnFlowInformationJButton, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.CENTER);
		JGUIUtil.addComponent(subformsPanel, __waterRightsJButton, 0, 2, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.CENTER);

		__viewtype = new SimpleJComboBox();
		__viewtype.add("Graph");
		__viewtype.add("Table");
		__viewtype.add("Summary");
	//	p5.add(__viewtype);

		__makeViewFrame = new JButton("Display View Frame");
		__makeViewFrame.addActionListener(this);
	//	p5.add(__makeViewFrame);

	//	JGUIUtil.addComponent(right_panel, p5, 0, y, 4, 1, 1, 0,
	//		0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.SOUTH);
		JGUIUtil.addComponent(right_panel, eff_panel, 0, y, 4, 1, 0, 0, 20, 0, 30, 0, GridBagConstraints.NONE, GridBagConstraints.NORTHWEST);
		JGUIUtil.addComponent(right_panel, tsPanel, 0, ++y, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.NORTHWEST);
		JGUIUtil.addComponent(right_panel, subformsPanel, 1, y, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.NORTHWEST);
		__waterRightsJButton.addActionListener(this);
		__returnFlowInformationJButton.addActionListener(this);
		__depletionInformationJButton.addActionListener(this);

		// add help and close buttons
		y = 10;
		JPanel p6 = new JPanel();
		p6.setLayout(fl);
		p6.add(__showOnMap_JButton);
		p6.add(__showOnNetwork_JButton);
		if (__editable)
		{
			p6.add(__applyJButton);
			p6.add(__cancelJButton);
		}
		p6.add(__closeJButton);
		// TODO SAM 2006-03-05
		// Comment out since help is not currently enabled.
		//p6.add(__helpJButton);
		JGUIUtil.addComponent(right_panel, p6, GridBagConstraints.RELATIVE, y, 4, 1, 1, 0, 30, 0, 0, 0, GridBagConstraints.BOTH, GridBagConstraints.SOUTH);
		__helpJButton.addActionListener(this);
		__closeJButton.addActionListener(this);
		__cancelJButton.addActionListener(this);
		__applyJButton.addActionListener(this);

		JGUIUtil.addComponent(mainJPanel, left_panel, 0, 0, 4, 10, 1, 1, 10, 10, 10, 0, GridBagConstraints.BOTH, GridBagConstraints.WEST);
		JGUIUtil.addComponent(mainJPanel, right_panel, GridBagConstraints.RELATIVE, 0, 4, 10, 0, 1, 10, 10, 10, 10, GridBagConstraints.BOTH, GridBagConstraints.EAST);

		getContentPane().add(mainJPanel);

		JPanel bottomJPanel = new JPanel();
		bottomJPanel.setLayout(gb);
		__messageJTextField = new JTextField();
		__messageJTextField.setEditable(false);
		JGUIUtil.addComponent(bottomJPanel, __messageJTextField, 0, 0, 7, 1, 1.0, 0.0, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);
		__statusJTextField = new JTextField(5);
		__statusJTextField.setEditable(false);
		JGUIUtil.addComponent(bottomJPanel, __statusJTextField, 7, 0, 1, 1, 0.0, 0.0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		getContentPane().add("South", bottomJPanel);

		initializeDisables();

		if (__dataset_wm != null)
		{
			__dataset_wm.setWindowOpen(StateMod_DataSet_WindowManager.WINDOW_WELL, this);
		}

		// TODO TS (JTS - 2003-09-03)
		// need code to disable the time series checkboxes:
		/*
		__wellPumpMonthlyTS,
		__wellPumpDailyTS,
		__demandsMonthlyTS,
		__demandsDailyTS;	
		*/
		// according to the dataset components that are available.

		// this must be done before the first call to selectTableIndex();
		populateWellDailyID();

		pack();
		setSize(845,770);
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
		if (__dataset_wm != null)
		{
			__dataset_wm.closeWindow(StateMod_DataSet_WindowManager.WINDOW_WELL);
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

	private void setOriginalCdividyw(StateMod_Well w, string cdividyw)
	{
		((StateMod_Well)w._original)._cdividyw = cdividyw;
	}

	}

}