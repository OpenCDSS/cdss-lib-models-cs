using System;
using System.Collections.Generic;

// StateMod_Plan_JFrame - a gui that displays a list of all the plans and the data for each plan

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

namespace DWR.StateMod
{


	using HydrologyNode = cdss.domain.hydrology.network.HydrologyNode;

	using GeoRecord = RTi.GIS.GeoView.GeoRecord;
	using GRLimits = RTi.GR.GRLimits;
	using GRShape = RTi.GR.GRShape;
	using JGUIUtil = RTi.Util.GUI.JGUIUtil;
	using JScrollWorksheet = RTi.Util.GUI.JScrollWorksheet;
	using JWorksheet = RTi.Util.GUI.JWorksheet;
	using JWorksheet_SortListener = RTi.Util.GUI.JWorksheet_SortListener;
	using SimpleJButton = RTi.Util.GUI.SimpleJButton;
	using SimpleJComboBox = RTi.Util.GUI.SimpleJComboBox;
	using DataSetComponent = RTi.Util.IO.DataSetComponent;
	using PropList = RTi.Util.IO.PropList;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;
	using TimeUtil = RTi.Util.Time.TimeUtil;
	using YearType = RTi.Util.Time.YearType;

	// The layout for the GUI is as follows, for the most part using grid bag
	// layout.  Only the button_JPanel and bottom_JPanel use FlowLayout.  In this
	// layout, only the worksheet is truly resizable.  The left panel is 1 column
	// and the right is 3 columns.
	//
	//	--------------------------main_JPanel-----------------------------------
	//	||--left_JPanel------||-------------right_JPanel----------------------||
	//	||                   ||                                               ||
	//	||                   ||          param_JPanel                         ||
	//	||   worksheet       ||                                               ||
	//	||                   || --------------------------------------------- ||
	//	||                   ||                                               ||
	//	|| ----------------- ||         eff_JPanel                            ||
	//	||                   ||                                               ||
	//	||   search_JPanel   ||                                               ||
	//	||                   ||                                               ||
	//	||                   || --------------------------------------------- ||
	//	||                   ||        button_JPanel                          ||
	//	||-------------------||-----------------------------------------------||
	//	------------------------------------------------------------------------
	//	|                         bottom_JPanel                                |
	//	------------------------------------------------------------------------

	/// <summary>
	/// This class is a gui that displays a list of all the plans and the 
	/// data for each plan, once it is selected.
	/// </summary>
	public class StateMod_Plan_JFrame : JFrame, ActionListener, ItemListener, KeyListener, MouseListener, WindowListener, JWorksheet_SortListener
	{

	/// <summary>
	/// Whether the form is editable
	/// </summary>
	private bool __editable = false;

	/// <summary>
	/// String labels for buttons.
	/// </summary>
	private const string __BUTTON_RETURN_FLOW = "Return Flow...", __BUTTON_SHOW_ON_MAP = "Show on Map", __BUTTON_SHOW_ON_NETWORK = "Show on Network", __BUTTON_APPLY = "Apply", __BUTTON_CANCEL = "Cancel", __BUTTON_CLOSE = "Close";

	/// <summary>
	/// Whether itemStateChanged() should ignore next state change that occurs.
	/// This is to prevent endless loops caused by the setMonthlyEff and setConstantEff methods.
	/// </summary>
	private bool __ignoreNextStateChange = false;

	/// <summary>
	/// Whether the entire GUI has been initialized yet.  Used to ignore completely all
	/// item state change events prior to the GUI being ready.
	/// </summary>
	private bool __initialized = false;

	/// <summary>
	/// The index in __all_JComponents[] of textfields that should NEVER be made editable (e.g., ID fields).
	/// </summary>
	private int[] __disabled_JComponents;

	/// <summary>
	/// The index of the currently-selected plan.
	/// </summary>
	private int __currentPlanIndex = -1;

	/// <summary>
	/// Stores the index of the record that was selected before the worksheet is sorted,
	/// in order to reselect it after the sort is complete.
	/// </summary>
	private int __sortSelectedRow = -1;

	/// <summary>
	/// GUI JButtons.
	/// </summary>
	private JButton __findNextPlan_JButton = null, __help_JButton = null, __close_JButton = null, __cancel_JButton = null, __apply_JButton = null, __returnFlow_JButton = null, __showOnMap_JButton = null, __showOnNetwork_JButton = null;

	/// <summary>
	/// Array of JComponents that should be disabled when nothing is selected from the list.
	/// </summary>
	private JComponent[] __all_JComponents;

	/// <summary>
	/// GUI JRadioButtons.
	/// </summary>
	private JRadioButton __searchID_JRadioButton = null, __searchName_JRadioButton = null;

	/// <summary>
	/// GUI JTextFields.
	/// </summary>
	private JTextField __searchID_JTextField = null, __searchName_JTextField = null, __planStationID_JTextField = null, __planName_JTextField = null, __riverNodeID_JTextField = null, __iPrf_JTextField = null, __Psto1_JTextField = null, __Psource_JTextField = null;
	private JTextField[] __Peff_JTextField = new JTextField[12];

	/// <summary>
	/// Status bar textfields 
	/// </summary>
	private JTextField __message_JTextField, __status_JTextField;

	/// <summary>
	/// The worksheet displaying the list of plans.
	/// </summary>
	private JWorksheet __worksheet;

	/// <summary>
	/// GUI SimpleJComboBoxes.
	/// </summary>
	private SimpleJComboBox __planSwitch_JComboBox = null, __iPlnTyp_JComboBox = null, __iPfail_JComboBox = null, __PeffFlag_JComboBox = null;

	/// <summary>
	/// The StateMod_DataSet that contains the StateMod data.
	/// </summary>
	private StateMod_DataSet __dataset;

	/// <summary>
	/// Data set window manager.
	/// </summary>
	private StateMod_DataSet_WindowManager __dataset_wm;

	/// <summary>
	/// The DataSetComponent that contains the plan data.
	/// </summary>
	private DataSetComponent __plansComponent;

	/// <summary>
	/// The list of plans to fill the worksheet with.
	/// </summary>
	private IList<StateMod_Plan> __plansVector;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="dataset"> StateMod_DataSet containing plan data. </param>
	/// <param name="dataset_wm"> the dataset window manager or null if the data set windows are not being managed. </param>
	/// <param name="editable"> whether the data values on the form can be edited or not. </param>
	public StateMod_Plan_JFrame(StateMod_DataSet dataset, StateMod_DataSet_WindowManager dataset_wm, bool editable)
	{
		StateMod_GUIUtil.setTitle(this, dataset, "Plans", null);
		JGUIUtil.setIcon(this, JGUIUtil.getIconImage());
		__dataset = dataset;
		__dataset_wm = dataset_wm;
		__plansComponent = __dataset.getComponentForComponentType(StateMod_DataSet.COMP_PLANS);

		__plansVector = (System.Collections.IList)__plansComponent.getData();
		int size = __plansVector.Count;
		StateMod_Plan plan = null;
		for (int i = 0; i < size; i++)
		{
			plan = (StateMod_Plan)__plansVector[i];
			plan.createBackup();
		}

		__editable = editable;

		setupGUI(0);
	}

	/// <summary>
	/// Constructor. </summary>
	/// <param name="dataset"> StateMod data set being displayed. </param>
	/// <param name="dataset_wm"> the dataset window manager or null if the data set windows are not being managed. </param>
	/// <param name="plan_sel"> the plan to select and show in the list of plans </param>
	/// <param name="editable"> whether the data values on the form can be edited or not. </param>
	public StateMod_Plan_JFrame(StateMod_DataSet dataset, StateMod_DataSet_WindowManager dataset_wm, StateMod_Plan plan_sel, bool editable)
	{
		StateMod_GUIUtil.setTitle(this, dataset, "Plans", null);
		JGUIUtil.setIcon(this, JGUIUtil.getIconImage());
		__dataset = dataset;
		__dataset_wm = dataset_wm;
		__plansComponent = __dataset.getComponentForComponentType(StateMod_DataSet.COMP_PLANS);

		__plansVector = (System.Collections.IList)__plansComponent.getData();
		int size = __plansVector.Count;
		StateMod_Plan plan = null;
		for (int i = 0; i < size; i++)
		{
			plan = (StateMod_Plan)__plansVector[i];
			plan.createBackup();
		}

		string id = plan_sel.getID();
		int index = StateMod_Util.IndexOf(__plansVector, id);

		__editable = editable;

		setupGUI(index);
	}

	/// <summary>
	/// Responds to actionPerformed events. </summary>
	/// <param name="e"> the ActionEvent that happened. </param>
	public virtual void actionPerformed(ActionEvent e)
	{
		string routine = "StateMod_Plan_JFrame.actionPerformed";

		object source = e.getSource();

		try
		{

		if (source == __help_JButton)
		{
			// TODO HELP (JTS 2003-06-09)
		}
		else if (source == __close_JButton)
		{
			saveCurrentPlan();
			int size = __plansVector.Count;
			StateMod_Plan plan = null;
			bool changed = false;
			for (int i = 0; i < size; i++)
			{
				plan = __plansVector[i];
				if (!changed && plan.changed())
				{
					changed = true;
				}
				plan.acceptChanges();
			}
			if (changed)
			{
				__dataset.setDirty(StateMod_DataSet.COMP_PLANS, true);
			}
			if (__dataset_wm != null)
			{
				__dataset_wm.closeWindow(StateMod_DataSet_WindowManager.WINDOW_PLAN);
			}
			else
			{
				JGUIUtil.close(this);
			}
		}
		else if (source == __apply_JButton)
		{
			saveCurrentPlan();
			int size = __plansVector.Count;
			StateMod_Plan plan = null;
			bool changed = false;
			for (int i = 0; i < size; i++)
			{
				plan = __plansVector[i];
				if (!changed && plan.changed())
				{
					changed = true;
				}
				plan.createBackup();
			}
			if (changed)
			{
				__dataset.setDirty(StateMod_DataSet.COMP_PLANS, true);
			}
		}
		else if (source == __cancel_JButton)
		{
			int size = __plansVector.Count;
			StateMod_Plan plan = null;
			for (int i = 0; i < size; i++)
			{
				plan = __plansVector[i];
				plan.restoreOriginal();
			}

			if (__dataset_wm != null)
			{
				__dataset_wm.closeWindow(StateMod_DataSet_WindowManager.WINDOW_PLAN);
			}
			else
			{
				JGUIUtil.close(this);
			}
		}
		else if (source == __findNextPlan_JButton)
		{
			searchWorksheet(__worksheet.getSelectedRow() + 1);
		}
		else if (source == __searchID_JTextField || source == __searchID_JTextField)
		{
			searchWorksheet();
		}
		else if (source == __searchID_JRadioButton)
		{
			__searchName_JTextField.setEditable(false);
			__searchID_JTextField.setEditable(true);
		}
		else if (source == __searchName_JRadioButton)
		{
			__searchID_JTextField.setEditable(false);
			__searchName_JTextField.setEditable(true);
		}
		else if (source == __showOnMap_JButton)
		{
			GeoRecord geoRecord = getSelectedPlan().getGeoRecord();
			GRShape shape = geoRecord.getShape();
			__dataset_wm.showOnMap(getSelectedPlan(), "Plan: " + getSelectedPlan().getID() + " - " + getSelectedPlan().getName(), new GRLimits(shape.xmin, shape.ymin, shape.xmax, shape.ymax), geoRecord.getLayer().getProjection());
		}
		else if (source == __showOnNetwork_JButton)
		{
			StateMod_Network_JFrame networkEditor = __dataset_wm.getNetworkEditor();
			if (networkEditor != null)
			{
				HydrologyNode node = networkEditor.getNetworkJComponent().findNode(getSelectedPlan().getID(), false, false);
				if (node != null)
				{
					__dataset_wm.showOnNetwork(getSelectedPlan(), "Plan: " + getSelectedPlan().getID() + " - " + getSelectedPlan().getName(), new GRLimits(node.getX(),node.getY(),node.getX(),node.getY()));
				}
			}
		}
		}
		catch (Exception ex)
		{
			Message.printWarning(2, routine, "Error processing action");
			Message.printWarning(2, routine, ex);
		}
	}

	/// <summary>
	/// Checks the text fields for validity before they are saved back into the data object. </summary>
	/// <returns> 0 if the text fields are okay, 1 if fatal errors exist, and -1 if only non-fatal errors exist. </returns>
	private int checkInput()
	{
		string routine = "StateMod_Plan_JFrame.checkInput";
		//String name = __planName_JTextField.getText().trim();
		string rivernode = __riverNodeID_JTextField.getText().Trim();
		string PeffFlag = __PeffFlag_JComboBox.getSelected().Trim();
		string iPrf = __iPrf_JTextField.getText().Trim();
		string Psto1 = __Psto1_JTextField.getText().Trim();
		//String Psource = __Psource_JTextField.getText().trim();

		string warning = "";
		int fatal_count = 0;
		int nonfatal_count = 0;

		/* TODO SAM 2006-08-22 No constraint
		if ( name.length() > 24 ) {
			warning += "\nPlan name is > 24 characters.";
			++fatal_count;
		}
		*/
		if (!StringUtil.isDouble(PeffFlag))
		{
			warning += "\nEfficiency (" + PeffFlag + ") is not a number.";
			++fatal_count;
		}
		if (!StringUtil.isInteger(iPrf))
		{
			warning += "\nReturn flow table (" + iPrf + ") is not an integer.";
			++fatal_count;
		}
		if (!StringUtil.isDouble(Psto1))
		{
			warning += "\nInitial storage (" + Psto1 + ") is not a number.";
			++fatal_count;
		}
		// Non-fatal errors (need to be corrected somehow)...
		if (__dataset != null)
		{
			DataSetComponent comp = __dataset.getComponentForComponentType(StateMod_DataSet.COMP_RIVER_NETWORK);
			System.Collections.IList data = (System.Collections.IList)comp.getData();
			if (!rivernode.Equals("") && (StateMod_Util.IndexOf(data,rivernode) < 0))
			{
				warning += "\nRiver node ID (" + rivernode + ") is not in the network.";
				++nonfatal_count;
			}
		}
		if (warning.Length > 0)
		{
			StateMod_Plan plan = (StateMod_Plan)__plansVector[__currentPlanIndex];
			warning = "\nPlan:  " + StateMod_Util.formatDataLabel(plan.getID(), plan.getName()) + warning + "\nCorrect or Cancel.";
			Message.printWarning(1, routine, warning, this);
			if (fatal_count > 0)
			{
				// Fatal errors...
				Message.printStatus(2, routine, "Returning 1 from checkInput()");
				return 1;
			}
			else
			{
				// Nonfatal errors...
				Message.printStatus(2, routine, "Returning -1 from checkInput()");
				return -1;
			}
		}
		else
		{
			// No errors...
			Message.printStatus(1, routine, "Returning 0 from checkInput()");
			return 0;
		}
	}

	/// <summary>
	/// Checks the states of the map and network view buttons based on the selected diversion.
	/// </summary>
	private void checkViewButtonState()
	{
		StateMod_Plan plan = getSelectedPlan();
		if (plan.getGeoRecord() == null)
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
	/// Clean up before garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~StateMod_Plan_JFrame()
	{
		__findNextPlan_JButton = null;
		__help_JButton = null;
		__close_JButton = null;
		__apply_JButton = null;
		__cancel_JButton = null;
		__searchID_JRadioButton = null;
		__searchName_JRadioButton = null;
		__searchID_JTextField = null;
		__searchName_JTextField = null;
		__iPrf_JTextField = null;
		__Psto1_JTextField = null;
		__Psource_JTextField = null;
		__planStationID_JTextField = null;
		__planName_JTextField = null;
		__riverNodeID_JTextField = null;
		__iPlnTyp_JComboBox = null;
		__iPfail_JComboBox = null;
		__worksheet = null;
		__planSwitch_JComboBox = null;

//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Get the selected diversion, based on the current index in the list.
	/// </summary>
	private StateMod_Plan getSelectedPlan()
	{
		return __plansVector[__currentPlanIndex];
	}

	/// <summary>
	/// Return the worksheet that displays the list of plans.
	/// </summary>
	private JWorksheet getWorksheet()
	{
		return __worksheet;
	}

	/// <summary>
	/// Initializes the arrays that are used when items are selected and deselected.
	/// This should be called from setupGUI() before the a call is made to 
	/// selectTableIndex().
	/// </summary>
	private void initializeJComponents()
	{
		__all_JComponents = new JComponent[10];
		int i = 0;
		// These may be disabled...
		__all_JComponents[i++] = __planStationID_JTextField;
		__all_JComponents[i++] = __riverNodeID_JTextField;
		// The rest are always enabled...
		__all_JComponents[i++] = __apply_JButton;
		__all_JComponents[i++] = __planName_JTextField;
		__all_JComponents[i++] = __planSwitch_JComboBox;
		__all_JComponents[i++] = __iPlnTyp_JComboBox;
		__all_JComponents[i++] = __PeffFlag_JComboBox;
		__all_JComponents[i++] = __iPrf_JTextField;
		__all_JComponents[i++] = __iPfail_JComboBox;
		__all_JComponents[i++] = __Psource_JTextField;

		// Indicate components that are never enabled.
		// The ID and the node ID are disabled in all cases.
		// Other components are disabled because the control data indicate that some data are not used.
		// All time series are enabled because it may be useful to compare
		// time series, regardless of the control settings.

		__disabled_JComponents = new int[1];
		__disabled_JComponents[0] = 0; //__planStationID_JTextField
		//__disabled_JComponents[1] = 1;	// __riverNodeID_JTextField

	}

	/// <summary>
	/// Responds to item state changed events. </summary>
	/// <param name="e"> the ItemEvent that happened. </param>
	public virtual void itemStateChanged(ItemEvent e)
	{
		if (!__initialized)
		{
			return;
		}
		if (__ignoreNextStateChange)
		{
			__ignoreNextStateChange = false;
			return;
		}

		if (__currentPlanIndex == -1)
		{
			return;
		}

		// set placeholder to current plan
		// TODO SAM 2007-03-01 Evaluate logic
		//StateMod_Plan plan = (StateMod_Plan)__plansVector.elementAt(__currentPlanIndex);
	}

	/// <summary>
	/// Responds to key pressed events; does nothing. </summary>
	/// <param name="e"> the KeyEvent that happened. </param>
	public virtual void keyPressed(KeyEvent e)
	{
	}

	/// <summary>
	/// Responds to key released events; does nothing. </summary>
	/// <param name="e"> the KeyEvent that happened. </param>
	public virtual void keyReleased(KeyEvent e)
	{
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
		processTableSelection(__worksheet.getSelectedRow(), true);
	}

	/// <summary>
	/// Processes a table selection (either via a mouse press or programmatically 
	/// from selectTableIndex()) by writing the old data back to the data set component
	/// and getting the next selection's data out of the data and displaying it on the form. </summary>
	/// <param name="index"> the index of the reservoir to display on the form. </param>
	/// <param name="try_to_save"> Indicates whether the current data should try to be saved.
	/// false should be specified if the call is being made after checkInput() fails. </param>
	private void processTableSelection(int index, bool try_to_save)
	{
		string routine = "processTableSelection";

		 // First save the previous information before displaying the new information...

		Message.printStatus(1, "", "processTableSelection index " + index + " save flag: " + try_to_save);

	/*
		if ( try_to_save && !saveCurrentPlan() ) {
			// Save was unsuccessful.  Revert to the previous index.  This
			// will eventually end up being a recursive call back to here
			// but try_to_save will be false.
			Message.printStatus ( 2, "", "processTableSelection index " + index + " save flag: " + try_to_save +
				" Error saving so display old index: " + __currentPlanIndex );
			selectTableIndex(__currentPlanIndex, false, false);
			return;
		}
	*/

		// Now switch to show data for the selected plan...

		// TODO SAM 2007-03-01 Evaluate logic
		//__lastPlanIndex = __currentPlanIndex;
		__currentPlanIndex = __worksheet.getOriginalRowNumber(index);

		if (__worksheet.getSelectedRow() == -1)
		{
			JGUIUtil.disableComponents(__all_JComponents, true);
			return;
		}

		JGUIUtil.enableComponents(__all_JComponents, __disabled_JComponents, __editable);

		// List these in the order of the GUI...

		StateMod_Plan plan = (StateMod_Plan)__plansVector[__currentPlanIndex];

		// Plan identifier...

		__planStationID_JTextField.setText(plan.getID());

		// Plan name...

		__planName_JTextField.setText(plan.getName());

		// River node...
		// TODO - if river node is made editable, change to a JComboBox...

		__riverNodeID_JTextField.setText(plan.getCgoto());

		// On/off switch...

		string Pon = "" + plan.getSwitch();
		// Select the switch that matches the first token in the available choices...
		try
		{
			JGUIUtil.selectTokenMatches(__planSwitch_JComboBox, true, " ", 0, 0, Pon, null);
		}
		catch (Exception)
		{
			// Default...
			Message.printWarning(2, routine, "Using default value Pon = \"" + StateMod_Plan.getPonDefault(true) + "\" because data value " + Pon + " is unknown.");
			__planSwitch_JComboBox.select(StateMod_Plan.getPonDefault(true));
		}

		// Plan type...

		string iPlnTyp = "" + plan.getIPlnTyp();
		// Select the switch that matches the first token in the available choices...
		try
		{
			JGUIUtil.selectTokenMatches(__iPlnTyp_JComboBox, true, " ", 0, 0, iPlnTyp, null);
		}
		catch (Exception)
		{
			// Default...
			Message.printWarning(2, routine, "Using default value iPlnTyp = \"" + StateMod_Plan.getIPlnTypDefault(true) + "\" because data value " + iPlnTyp + " is unknown.");
			__iPlnTyp_JComboBox.select(StateMod_Plan.getIPlnTypDefault(true));
		}

		// Efficiency flag...

		string PeffFlag = "" + plan.getPeffFlag();
		// Select the switch that matches the first token in the available choices...
		try
		{
			JGUIUtil.selectTokenMatches(__PeffFlag_JComboBox, true, " ", 0, 0, "" + PeffFlag, null);
		}
		catch (Exception)
		{
			// Default...
			Message.printWarning(2, routine, "Using default value PeffFlag = \"" + StateMod_Plan.getPeffFlagDefault(true) + "\" because data value " + PeffFlag + " is unknown.");
			__PeffFlag_JComboBox.select(StateMod_Plan.getPeffFlagDefault(true));
		}

		// Return flow table...

		StateMod_GUIUtil.checkAndSet(plan.getIPrf(), __iPrf_JTextField);

		// Fail switch...

		string iPfail = "" + plan.getIPfail();
		// Select the switch that matches the first token in the available choices...
		try
		{
			JGUIUtil.selectTokenMatches(__iPfail_JComboBox, true, " ", 0, 0, iPfail, null);
		}
		catch (Exception)
		{
			// Default...
			Message.printWarning(2, routine, "Using default value iPfail = \"" + StateMod_Plan.getIPfailDefault(true) + "\" because data value " + iPfail + " is unknown.");
			__iPfail_JComboBox.select(StateMod_Plan.getIPfailDefault(true));
		}

		// Initial storage...

		StateMod_GUIUtil.checkAndSet(plan.getPsto1(), __Psto1_JTextField);

		// Source ID...

		__Psource_JTextField.setText(plan.getPsource());

		checkViewButtonState();
	}

	/// <summary>
	/// Saves the current record selected in the table; called when the window is closed
	/// or minimized or apply is pressed. </summary>
	/// <returns> true if the save was successful, false if not. </returns>
	private bool saveCurrentPlan()
	{
		Message.printStatus(1, "", "saveCurrentPlan - start");
		return saveData(__currentPlanIndex);
	}

	/// <summary>
	/// Saves the information associated with the currently-selected plan.
	/// The user doesn't need to hit the return key for the GUI to recognize changes.
	/// The info is saved each time the user selects a different station, presses
	/// the Close button, or presses the Apply button. </summary>
	/// <returns> true if the save was successful, false if not.  True is also returned
	/// if edits are not allowed or if no previous item has been selected. </returns>
	private bool saveData(int record)
	{
		if (!__editable || (record == -1))
		{
			return true;
		}

		if (checkInput() > 0)
		{
			// Fatal errors so cannot save...
			Message.printStatus(1, "", "saveData detected error in checkInput - returning false");
			return false;
		}

		StateMod_Plan plan = (StateMod_Plan)__plansVector[record];

		// Save in the order of the GUI.  Save all the items, even if not
		// currently editable/enabled (like ID and river node ID) because later
		// these fields may be made editable and checkInput() will warn the user about issues

		plan.setID(__planStationID_JTextField.getText().Trim());

		// Plan name...

		plan.setName(__planName_JTextField.getText().Trim());

		// River node...

		plan.setCgoto(__riverNodeID_JTextField.getText().Trim());

		// On/off switch...

		string Pon = StringUtil.getToken(__planSwitch_JComboBox.getSelected()," ",0,0);
		plan.setSwitch(StringUtil.atoi(Pon));

		// Plan type...

		string iPlnTyp = StringUtil.getToken(__iPlnTyp_JComboBox.getSelected()," ",0,0);
		plan.setIPlnTyp(StringUtil.atoi(iPlnTyp));

		// Efficiency...

		string PeffFlag = __PeffFlag_JComboBox.getSelected().Trim();
		if (PeffFlag.Equals("") && !StateMod_Util.isMissing(plan.getPeffFlag()))
		{
			// User has blanked to missing for some reason...
			plan.setPeffFlag(StateMod_Util.MISSING_INT);
		}
		else if (!PeffFlag.Equals(""))
		{
			// Something has changed so set it...
			plan.setPeffFlag(PeffFlag);
		}

		// Return flow table...

		string iPrf = __iPrf_JTextField.getText().Trim();
		if (iPrf.Equals("") && !StateMod_Util.isMissing(plan.getIPrf()))
		{
			// User has blanked to missing for some reason...
			plan.setIPrf(StateMod_Util.MISSING_INT);
		}
		else if (!iPrf.Equals(""))
		{
			// Something has changed so set it...
			plan.setIPrf(iPrf);
		}

		// Fail switch...

		string iPfail = StringUtil.getToken(__iPfail_JComboBox.getSelected()," ",0,0);
		plan.setIPfail(StringUtil.atoi(iPfail));

		// Initial storage...

		string Psto1 = __Psto1_JTextField.getText().Trim();
		if (Psto1.Equals("") && !StateMod_Util.isMissing(plan.getPsto1()))
		{
			// User has blanked to missing for some reason...
			plan.setPsto1(StateMod_Util.MISSING_DOUBLE);
		}
		else if (!Psto1.Equals(""))
		{
			// Something has changed so set it...
			plan.setPsto1(Psto1);
		}

		// Source ID...

		plan.setPsource(__Psource_JTextField.getText().Trim());

		// Update the main interface - if the data set is dirty, the File...Save
		// menu will be enabled...

		if (__dataset_wm != null)
		{
			__dataset_wm.updateWindowStatus(StateMod_DataSet_WindowManager.WINDOW_MAIN);
		}

		return true;
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
		if (__searchID_JRadioButton.isSelected())
		{
			searchFor = __searchID_JTextField.getText().Trim();
			col = 0;
		}
		else
		{
			searchFor = __searchName_JTextField.getText().Trim();
			col = 1;
		}
		int index = __worksheet.find(searchFor, col, row, JWorksheet.FIND_EQUAL_TO | JWorksheet.FIND_CONTAINS | JWorksheet.FIND_CASE_INSENSITIVE | JWorksheet.FIND_WRAPAROUND);
		if (index != -1)
		{
			// TODO - how to handle the last parameter...
			selectTableIndex(index, true, true);
		}
	}

	/// <summary>
	/// Selects the desired ID in the table and displays the appropriate data
	/// in the remainder of the window.  This method is called when the network is edited. </summary>
	/// <param name="id"> the identifier to select in the list. </param>
	public virtual void selectID(string id)
	{
		int rows = __worksheet.getRowCount();
		StateMod_Plan plan = null;
		for (int i = 0; i < rows; i++)
		{
			plan = (StateMod_Plan)__worksheet.getRowData(i);
			if (plan.getID().Equals(id, StringComparison.OrdinalIgnoreCase))
			{
				// REVISIT - how to handle the last parameter...
				selectTableIndex(i, true, true);
				return;
			}
		}
	}

	/// <summary>
	/// Selects the desired index in the table, but also displays the appropriate data
	/// in the remainder of the window. </summary>
	/// <param name="index"> the index to select in the list. </param>
	/// <param name="try_to_save"> Indicates whether the current contents should be saved before
	/// selecting the new row.  A value of false should be passed only at startup or
	/// when checkInput() has failed for a selection, in which case the display will
	/// revert to the failed data rather than a new selection. </param>
	/// <param name="process_selection"> If true, then the display will be updated based on the
	/// row that is selected - this should be the case most of the time.  If false, then
	/// the previous contents are retained - this should be the case if checkInput()
	/// detects an error, in which case we want the previous (and erroneous)
	/// user-supplied to be shown because they need to correct the data. </param>
	public virtual void selectTableIndex(int index, bool try_to_save, bool process_selection)
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
		// TODO SAM 2006-08-22 Was commented out for diversions also
		//__currentPlanIndex = __worksheet.getSelectedRow();
		Message.printStatus(1, "", "selectTableIndex index " + index + " save flag: " + try_to_save);
		if (process_selection)
		{
			processTableSelection(index, try_to_save);
		}
	}

	/// <summary>
	/// Sets up the GUI. </summary>
	/// <param name="index"> the index of the element to highlight and display on the form. </param>
	private void setupGUI(int index)
	{
		string routine = "StateMod_Plan_JFrame.setupGUI";

		addWindowListener(this);

		GridBagLayout gbl = new GridBagLayout();

		JPanel main_JPanel = new JPanel(); // Contains all in center panel of the JFrame
		main_JPanel.setLayout(gbl);
		JPanel left_JPanel = new JPanel(); // Left side (list and search)
		left_JPanel.setLayout(gbl);
		JPanel right_JPanel = new JPanel(); // Right side (all data)
		right_JPanel.setLayout(gbl);

		// Plan list...

		JGUIUtil.addComponent(left_JPanel, setupGUI_PlanList(routine), 0, 0, 1, 5, 1, 1, GridBagConstraints.BOTH, GridBagConstraints.NORTHWEST);

		// Search panel...

		JGUIUtil.addComponent(left_JPanel, setupGUI_SearchPanel(routine), 0, 5, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);

		JGUIUtil.addComponent(main_JPanel, left_JPanel, 0, 0, 1, 10, 1, 1, GridBagConstraints.BOTH, GridBagConstraints.NORTHWEST);

		// Add the parameters panel to the right panel (make it 12 wide)...

		int yRight = 0;
		JGUIUtil.addComponent(right_JPanel, setupGUI_PlanAttributes(routine), 0, yRight, 12, 1, 0, 0, GridBagConstraints.BOTH, GridBagConstraints.CENTER);

		JGUIUtil.addComponent(right_JPanel, setupGUI_PlanEfficiency(routine), 0, ++yRight, 12, 1, 0, 0, GridBagConstraints.BOTH, GridBagConstraints.CENTER);

		JGUIUtil.addComponent(right_JPanel, setupGUI_RelatedData(routine), 0, ++yRight, 12, 1, 0, 0, GridBagConstraints.BOTH, GridBagConstraints.CENTER);

		// Main buttons...

		JGUIUtil.addComponent(right_JPanel, setupGUI_ButtonPanel(), 0, ++yRight, 11, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.CENTER);

		// Add the right panel to the main panel

		JGUIUtil.addComponent(main_JPanel, right_JPanel, 1, 0, 3, 10, 0, 0, GridBagConstraints.NONE, GridBagConstraints.NORTHEAST);

		getContentPane().add(main_JPanel);

		// Panel at the bottom for messages...

		JPanel bottom_JPanel = new JPanel();
		bottom_JPanel.setLayout(gbl);
		__message_JTextField = new JTextField();
		__message_JTextField.setEditable(false);
		JGUIUtil.addComponent(bottom_JPanel, __message_JTextField, 0, 0, 7, 1, 1.0, 0.0, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);
		__status_JTextField = new JTextField(5);
		__status_JTextField.setEditable(false);
		JGUIUtil.addComponent(bottom_JPanel, __status_JTextField, 7, 0, 1, 1, 0.0, 0.0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		getContentPane().add("South", bottom_JPanel);

		if (__dataset_wm != null)
		{
			__dataset_wm.setWindowOpen(StateMod_DataSet_WindowManager.WINDOW_PLAN, this);
		}

		pack();
		setSize(700,500);
		JGUIUtil.center(this);
		initializeJComponents();
		selectTableIndex(index, false, true);
		setVisible(true);

		int[] widths = getWorksheet().getCellRenderer().getColumnWidths();
		if (widths != null)
		{
			__worksheet.setColumnWidths(widths);
		}
		__initialized = true;
		__worksheet.addSortListener(this);
	}

	/// <summary>
	/// Set up the main button panel.
	/// </summary>
	private JPanel setupGUI_ButtonPanel()
	{
		JPanel button_JPanel = new JPanel();
		button_JPanel.setLayout(new FlowLayout());
		//__help_JButton = new SimpleJButton(__BUTTON_HELP, this);
		//__help_JButton.setEnabled(false);
		__showOnMap_JButton = new SimpleJButton(__BUTTON_SHOW_ON_MAP, this);
		__showOnMap_JButton.setToolTipText("Annotate map with location (button is disabled if layer does not have matching ID)");
		__showOnNetwork_JButton = new SimpleJButton(__BUTTON_SHOW_ON_NETWORK, this);
		__showOnNetwork_JButton.setToolTipText("Annotate network with location");
		__close_JButton = new SimpleJButton(__BUTTON_CLOSE, this);
		__apply_JButton = new SimpleJButton(__BUTTON_APPLY, this);
		__cancel_JButton = new SimpleJButton(__BUTTON_CANCEL, this);
		button_JPanel.add(__showOnMap_JButton);
		button_JPanel.add(__showOnNetwork_JButton);
		if (__editable)
		{
			button_JPanel.add(__apply_JButton);
			button_JPanel.add(__cancel_JButton);
		}
		button_JPanel.add(__close_JButton);
		//button_JPanel.add(__help_JButton);
		return button_JPanel;
	}

	/// <summary>
	/// Set up the parameter part of the GUI.
	/// </summary>
	private JPanel setupGUI_PlanAttributes(string routine)
	{
		JPanel param_JPanel = new JPanel();
		GridBagLayout gbl = new GridBagLayout();
		param_JPanel.setLayout(gbl);
		int y = 0;
		JGUIUtil.addComponent(param_JPanel, new JLabel("Plan ID:"), 0, y, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		__planStationID_JTextField = new JTextField(12);
		__planStationID_JTextField.setEditable(false);
		__planStationID_JTextField.setToolTipText("<html>The plan ID is the primary identifier for the plan.<br>" + "The ID is used to relate data in various data files.</html>");
		JGUIUtil.addComponent(param_JPanel, __planStationID_JTextField, 1, y, 2, 1, 0, 0, 1, 0, 0, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);

		JGUIUtil.addComponent(param_JPanel, new JLabel("Plan name:"), 0, ++y, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		__planName_JTextField = new JTextField(24);
		__planName_JTextField.setToolTipText("The plan name is used for labels and output.");
		JGUIUtil.addComponent(param_JPanel, __planName_JTextField, 1, y, 2, 1, 0, 0, 1, 0, 0, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);

		JGUIUtil.addComponent(param_JPanel, new JLabel("River node ID:"), 0, ++y, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		__riverNodeID_JTextField = new JTextField(12);
		__riverNodeID_JTextField.setEditable(true);
		__riverNodeID_JTextField.setToolTipText("<html>The river node is used in the network file.<br>" + "In most cases the river node ID is the same as the plan " + "ID,<br>although StateMod internally uses two identifiers.</html>");
		JGUIUtil.addComponent(param_JPanel, __riverNodeID_JTextField, 1, y, 2, 1, 0, 0, 1, 0, 0, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);

		JGUIUtil.addComponent(param_JPanel, new JLabel("On/off switch:"), 0, ++y, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		__planSwitch_JComboBox = new SimpleJComboBox();
		__planSwitch_JComboBox.setData(StateMod_Plan.getPonChoices(true));
		__planSwitch_JComboBox.addItemListener(this);
		__planSwitch_JComboBox.setToolTipText("The on/off switch tells StateMod whether to include the plan in the analysis.");
		JGUIUtil.addComponent(param_JPanel, __planSwitch_JComboBox, 1, y, 2, 1, 0, 0, 1, 0, 0, 1, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);

		JGUIUtil.addComponent(param_JPanel, new JLabel("Plan type:"), 0, ++y, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		__iPlnTyp_JComboBox = new SimpleJComboBox();
		__iPlnTyp_JComboBox.setData(StateMod_Plan.getIPlnTypChoices(true));
		__iPlnTyp_JComboBox.addItemListener(this);
		__iPlnTyp_JComboBox.setToolTipText("The plan type indicates the behavior of the plan.");
		JGUIUtil.addComponent(param_JPanel, __iPlnTyp_JComboBox, 1, y, 2, 1, 0, 0, 1, 0, 0, 1, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);

		JGUIUtil.addComponent(param_JPanel, new JLabel("Return flow table:"), 0, ++y, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		__iPrf_JTextField = new JTextField(11);
		__iPrf_JTextField.setToolTipText("The return flow table for the plan.");
		JGUIUtil.addComponent(param_JPanel, __iPrf_JTextField, 1, y, 2, 1, 0, 0, 1, 0, 0, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);

		JGUIUtil.addComponent(param_JPanel, new JLabel("Failure switch:"), 0, ++y, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		__iPfail_JComboBox = new SimpleJComboBox();
		__iPfail_JComboBox.setData(StateMod_Plan.getIPfailChoices(true));
		__iPfail_JComboBox.addItemListener(this);
		__iPfail_JComboBox.setToolTipText("Plan failure switch.");
		JGUIUtil.addComponent(param_JPanel, __iPfail_JComboBox, 1, y, 2, 1, 0, 0, 1, 0, 0, 1, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);

		// Put in blanks to have better spacing in layout...
		JGUIUtil.addComponent(param_JPanel, new JLabel("Initial storage (AF):"), 0, ++y, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		__Psto1_JTextField = new JTextField(11);
		__Psto1_JTextField.setToolTipText("The plan initial storage (for types 3 & 5).");
		JGUIUtil.addComponent(param_JPanel, __Psto1_JTextField, 1, y, 2, 1, 0, 0, 1, 0, 0, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);

		JGUIUtil.addComponent(param_JPanel, new JLabel("Source ID:"), 0, ++y, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		__Psource_JTextField = new JTextField(24);
		__Psource_JTextField.setToolTipText("Source ID of the structure where reuse water " + "became available or a T&C condition originated.");
		JGUIUtil.addComponent(param_JPanel, __Psource_JTextField, 1, y, 2, 1, 0, 0, 1, 0, 0, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);
		return param_JPanel;
	}

	/// <summary>
	/// Set up the plan efficiency part of the GUI.
	/// </summary>
	private JPanel setupGUI_PlanEfficiency(string routine)
	{
		JPanel effJPanel = new JPanel();
		effJPanel.setBorder(BorderFactory.createTitledBorder("Efficiency Data"));
		GridBagLayout gbl = new GridBagLayout();
		effJPanel.setLayout(gbl);
		int y = 0;
		JGUIUtil.addComponent(effJPanel, new JLabel("Efficiency Flag:"), 0, y, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		__PeffFlag_JComboBox = new SimpleJComboBox();
		__PeffFlag_JComboBox.setData(StateMod_Plan.getPeffFlagChoices(true));
		__PeffFlag_JComboBox.addItemListener(this);
		__PeffFlag_JComboBox.setToolTipText("The plan efficiency.");
		JGUIUtil.addComponent(effJPanel, __PeffFlag_JComboBox, 1, y, 2, 1, 0, 0, 1, 0, 0, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JPanel monthEffJPanel = new JPanel();
		monthEffJPanel.setLayout(gbl);
		int[] monthsCyr = new int[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12};
		int[] monthsWyr = new int[] {10, 11, 12, 1, 2, 3, 4, 5, 6, 7, 8, 9};
		int[] monthsIyr = new int[] {11, 12, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10};
		int[] months = monthsCyr;
		if (__dataset.getCyrl() == YearType.WATER)
		{
			months = monthsWyr;
		}
		else if (__dataset.getCyrl() == YearType.NOV_TO_OCT)
		{
			months = monthsIyr;
		}
		for (int i = 0; i < 12; i++)
		{
			__Peff_JTextField[i] = new JTextField(4);
			JGUIUtil.addComponent(monthEffJPanel, new JLabel(TimeUtil.monthAbbreviation(months[i])), i, 0, 1, 1, 0, 0, 1, 0, 0, 1, GridBagConstraints.HORIZONTAL, GridBagConstraints.CENTER);
			JGUIUtil.addComponent(monthEffJPanel, __Peff_JTextField[i], i, 1, 1, 1, 0, 0, 1, 0, 0, 1, GridBagConstraints.HORIZONTAL, GridBagConstraints.CENTER);
		}
		JGUIUtil.addComponent(effJPanel, monthEffJPanel, 0, ++y, 3, 1, 0, 0, 1, 0, 0, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);
		return effJPanel;
	}

	/// <summary>
	/// Set up the plan list part of the GUI.
	/// </summary>
	private JScrollWorksheet setupGUI_PlanList(string routine)
	{
		PropList p = new PropList("StateMod_Plan_JFrame.JWorksheet");
		p.add("JWorksheet.ShowPopupMenu=true");
		p.add("JWorksheet.AllowCopy=true");
		p.add("JWorksheet.SelectionMode=SingleRowSelection");

		JScrollWorksheet jsw = null;
		JWorksheet worksheet = null;
		try
		{
			StateMod_Plan_TableModel tmd = new StateMod_Plan_TableModel(__plansVector, __editable, true);
			StateMod_Plan_CellRenderer crd = new StateMod_Plan_CellRenderer(tmd);

			jsw = new JScrollWorksheet(crd, tmd, p);
		}
		catch (Exception e)
		{
			Message.printWarning(2, routine, e);
			jsw = new JScrollWorksheet(0, 0, p);
		}
		worksheet = jsw.getJWorksheet();
		worksheet.setPreferredScrollableViewportSize(null);
		worksheet.setHourglassJFrame(this);
		worksheet.addMouseListener(this);
		worksheet.addKeyListener(this);
		setWorksheet(worksheet);
		return jsw;
	}

	/// <summary>
	/// Setup the related data panel for the GUI.
	/// </summary>
	private JPanel setupGUI_RelatedData(string routine)
	{
		JPanel relatedDataJPanel = new JPanel();
		GridBagLayout gbl = new GridBagLayout();
		relatedDataJPanel.setLayout(gbl);
		relatedDataJPanel.setBorder(BorderFactory.createTitledBorder("Related Data"));
		__returnFlow_JButton = new JButton(__BUTTON_RETURN_FLOW);
		JGUIUtil.addComponent(relatedDataJPanel, __returnFlow_JButton, 0, 0, 1, 1, 1, 1, 1, 0, 0, 1, GridBagConstraints.NONE, GridBagConstraints.CENTER);
		__returnFlow_JButton.addActionListener(this);
		return relatedDataJPanel;
	}

	// TODO SAM 2011-01-02 Need to put into a reusable component
	/// <summary>
	/// Setup up the search panel for the GUI.
	/// </summary>
	private JPanel setupGUI_SearchPanel(string routine)
	{
		JPanel search_JPanel = new JPanel();
		GridBagLayout gbl = new GridBagLayout();
		search_JPanel.setLayout(gbl);
		search_JPanel.setBorder(BorderFactory.createTitledBorder("Search above list for:"));
		int y = 0;
		ButtonGroup searchCriteriaGroup = new ButtonGroup();
		__searchID_JRadioButton = new JRadioButton("ID", true);
		__searchID_JRadioButton.addActionListener(this);
		__searchID_JRadioButton.setSelected(true);
		searchCriteriaGroup.add(__searchID_JRadioButton);
		JGUIUtil.addComponent(search_JPanel, __searchID_JRadioButton, 0, y, 1, 1, 0, 0, 5, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		__searchID_JTextField = new JTextField(15);
		__searchID_JTextField.setEnabled(true);
		__searchID_JTextField.addActionListener(this);
		JGUIUtil.addComponent(search_JPanel, __searchID_JTextField, 1, y, 2, 1, 1, 0, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);
		__searchName_JRadioButton = new JRadioButton("Name", false);
		__searchName_JRadioButton.addActionListener(this);
		searchCriteriaGroup.add(__searchName_JRadioButton);
		JGUIUtil.addComponent(search_JPanel, __searchName_JRadioButton, 0, ++y, 1, 1, 0, 0, 5, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		__searchName_JTextField = new JTextField(15);
		__searchName_JTextField.setEditable(false);
		__searchName_JTextField.setEnabled(false);
		__searchName_JTextField.addActionListener(this);
		JGUIUtil.addComponent(search_JPanel, __searchName_JTextField, 1, y, 2, 1, 0, 0, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);
		__findNextPlan_JButton = new SimpleJButton("Find Next", this);
		JGUIUtil.addComponent(search_JPanel, __findNextPlan_JButton, 1, ++y, 2, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		return search_JPanel;
	}

	/// <summary>
	/// Set the worksheet that displays the list of plans.
	/// </summary>
	private void setWorksheet(JWorksheet worksheet)
	{
		__worksheet = worksheet;
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
		saveCurrentPlan();
		int size = __plansVector.Count;
		StateMod_Plan plan = null;
		bool changed = false;
		for (int i = 0; i < size; i++)
		{
			plan = (StateMod_Plan)__plansVector[i];
			if (!changed && plan.changed())
			{
				changed = true;
			}
			plan.acceptChanges();
		}
		if (changed)
		{
			__dataset.setDirty(StateMod_DataSet.COMP_PLANS, true);
		}
		if (__dataset_wm != null)
		{
			__dataset_wm.closeWindow(StateMod_DataSet_WindowManager.WINDOW_PLAN);
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
		saveCurrentPlan();
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