using System;
using System.Collections.Generic;

// StateMod_RiverNetworkNode_JFrame - dialog to edit the river network (.rin) information

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
// StateMod_RiverNetworkNode_JFrame - dialog to edit the river network (.rin)
//	information
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
// 2003-09-04	JTS, RTi		Added cancel and apply buttons.
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
// 2004-07-17	JTS, RTi		Current and last indices were not 
//					defaulting to -1 when the class was
//					instantiated, and this was causing
//					problems.
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
	/// Class to display data about river stations.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class StateMod_RiverNetworkNode_JFrame extends javax.swing.JFrame implements java.awt.event.ActionListener, java.awt.event.KeyListener, java.awt.event.MouseListener, java.awt.event.WindowListener, RTi.Util.GUI.JWorksheet_SortListener
	public class StateMod_RiverNetworkNode_JFrame : JFrame, ActionListener, KeyListener, MouseListener, WindowListener, JWorksheet_SortListener
	{

	/// <summary>
	/// Button group for selecting the kind of search to do.
	/// </summary>
	private ButtonGroup __searchCriteriaGroup;

	/// <summary>
	/// Stores the index of the record that was selected before the worksheet is sorted,
	/// in order to reselect it after the sort is complete.
	/// </summary>
	private int __sortSelectedRow = -1;

	/// <summary>
	/// Buttons for performing operations on the form.
	/// </summary>
	private JButton __applyJButton, __cancelJButton, __findNext, __helpJButton, __closeJButton, __showOnMap_JButton = null, __showOnNetwork_JButton = null;

	/// <summary>
	/// Radio buttons for selecting the kind of search to do.
	/// </summary>
	private JRadioButton __searchIDJRadioButton, __searchNameJRadioButton;

	/// <summary>
	/// Text fields for searching through the worksheet.
	/// </summary>
	private JTextField __searchID, __searchName;

	/// <summary>
	/// Worksheet for displaying river station data.
	/// </summary>
	private JWorksheet __worksheet;

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

	private DataSetComponent __riverNetworkNodeComponent;
	private IList<StateMod_RiverNetworkNode> __riverNetworkNodesVector;

	private JTextField __idJTextField;
	private JTextField __nameJTextField;
	private JTextField __nodeJTextField;
	private JTextField __commentJTextField;

	private int __lastStationIndex = 1;
	private int __currentStationIndex = -1;

	/// <summary>
	/// The index in __disables[] of textfields that should NEVER be made editable (e.g., ID fields).
	/// </summary>
	private int[] __textUneditables;

	/// <summary>
	/// Array of JComponents that should be disabled when nothing is selected from the list.
	/// </summary>
	private JComponent[] __disables;

	private bool __editable = true;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="dataset"> the dataset containing the data to show in the form. </param>
	/// <param name="dataset_wm"> the dataset window manager or null if the data set windows are not being managed. </param>
	/// <param name="editable"> whether the data on the gui can be edited or not. </param>
	public StateMod_RiverNetworkNode_JFrame(StateMod_DataSet dataset, StateMod_DataSet_WindowManager dataset_wm, bool editable)
	{
		JGUIUtil.setIcon(this, JGUIUtil.getIconImage());
		StateMod_GUIUtil.setTitle(this, dataset, "River Network Nodes", null);

		__dataset = dataset;
		__dataset_wm = dataset_wm;
		__riverNetworkNodeComponent = __dataset.getComponentForComponentType(StateMod_DataSet.COMP_RIVER_NETWORK);
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<StateMod_RiverNetworkNode> riverNetworkNodesList = (java.util.List<StateMod_RiverNetworkNode>)__riverNetworkNodeComponent.getData();
		IList<StateMod_RiverNetworkNode> riverNetworkNodesList = (IList<StateMod_RiverNetworkNode>)__riverNetworkNodeComponent.getData();
		__riverNetworkNodesVector = riverNetworkNodesList;


		int size = __riverNetworkNodesVector.Count;
		StateMod_RiverNetworkNode r = null;
		for (int i = 0; i < size; i++)
		{
			r = (StateMod_RiverNetworkNode) __riverNetworkNodesVector[i];
			r.createBackup();
		}

		__editable = editable;

		setupGUI(0);
	}

	/// <summary>
	/// Constructor. </summary>
	/// <param name="dataset"> the dataset containing the data to show in the form. </param>
	/// <param name="editable"> whether the data on the gui can be edited or not. </param>
	public StateMod_RiverNetworkNode_JFrame(StateMod_DataSet dataset, StateMod_RiverNetworkNode node, bool editable) : base("")
	{
		JGUIUtil.setIcon(this, JGUIUtil.getIconImage());
		StateMod_GUIUtil.setTitle(this, dataset, "River Network Nodes", null);

		__dataset = dataset;
		__riverNetworkNodeComponent = __dataset.getComponentForComponentType(StateMod_DataSet.COMP_RIVER_NETWORK);
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<StateMod_RiverNetworkNode> riverNetworkNodesList = (java.util.List<StateMod_RiverNetworkNode>)__riverNetworkNodeComponent.getData();
		IList<StateMod_RiverNetworkNode> riverNetworkNodesList = (IList<StateMod_RiverNetworkNode>)__riverNetworkNodeComponent.getData();
		__riverNetworkNodesVector = riverNetworkNodesList;

		int size = __riverNetworkNodesVector.Count;
		StateMod_RiverNetworkNode r = null;
		for (int i = 0; i < size; i++)
		{
			r = (StateMod_RiverNetworkNode) __riverNetworkNodesVector[i];
			r.createBackup();
		}

		string id = node.getID();
		int index = StateMod_Util.locateIndexFromID(id, __riverNetworkNodesVector);

		__editable = editable;

		setupGUI(index);
	}

	/// <summary>
	/// Responds to action performed events. </summary>
	/// <param name="e"> the ActionEvent that happened. </param>
	public virtual void actionPerformed(ActionEvent e)
	{
		string routine = "StateMod_RiverNetworkNode_JFrame.actionPerformed";
		if (Message.isDebugOn)
		{
			Message.printDebug(1, routine, "In actionPerformed: " + e.getActionCommand());
		}
		object source = e.getSource();
		if (source == __closeJButton)
		{
			saveCurrentRecord();
			int size = __riverNetworkNodesVector.Count;
			StateMod_RiverNetworkNode r = null;
			bool changed = false;
			for (int i = 0; i < size; i++)
			{
				r = __riverNetworkNodesVector[i];
				if (!changed && r.changed())
				{
					changed = true;
				}
				r.acceptChanges();
			}
			if (changed)
			{
				__dataset.setDirty(StateMod_DataSet.COMP_RIVER_NETWORK, true);
			}
			if (__dataset_wm != null)
			{
				__dataset_wm.closeWindow(StateMod_DataSet_WindowManager.WINDOW_RIVER_NETWORK);
			}
			else
			{
				JGUIUtil.close(this);
			}
		}
		else if (source == __applyJButton)
		{
			saveCurrentRecord();
			int size = __riverNetworkNodesVector.Count;
			StateMod_RiverNetworkNode r = null;
			bool changed = false;
			for (int i = 0; i < size; i++)
			{
				r = __riverNetworkNodesVector[i];
				if (!changed && r.changed())
				{
					changed = true;
				}
				r.createBackup();
			}
			if (changed)
			{
				__dataset.setDirty(StateMod_DataSet.COMP_RIVER_NETWORK, true);
			}
		}
		else if (source == __cancelJButton)
		{
			int size = __riverNetworkNodesVector.Count;
			StateMod_RiverNetworkNode r = null;
			for (int i = 0; i < size; i++)
			{
				r = __riverNetworkNodesVector[i];
				r.restoreOriginal();
			}
			if (__dataset_wm != null)
			{
				__dataset_wm.closeWindow(StateMod_DataSet_WindowManager.WINDOW_RIVER_NETWORK);
			}
			else
			{
				JGUIUtil.close(this);
			}
		}
		else if (source == __helpJButton)
		{
			// REVISIT HELP (JTS - 2003-08-18)
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
			GeoRecord geoRecord = getSelectedRiverNetworkNode().getGeoRecord();
			GRShape shape = geoRecord.getShape();
			__dataset_wm.showOnMap(getSelectedRiverNetworkNode(), "Node: " + getSelectedRiverNetworkNode().getID() + " - " + getSelectedRiverNetworkNode().getName(), new GRLimits(shape.xmin, shape.ymin, shape.xmax, shape.ymax), geoRecord.getLayer().getProjection());
		}
		else if (source == __showOnNetwork_JButton)
		{
			StateMod_Network_JFrame networkEditor = __dataset_wm.getNetworkEditor();
			if (networkEditor != null)
			{
				HydrologyNode node = networkEditor.getNetworkJComponent().findNode(getSelectedRiverNetworkNode().getID(), false, false);
				if (node != null)
				{
					__dataset_wm.showOnNetwork(getSelectedRiverNetworkNode(), "Node: " + getSelectedRiverNetworkNode().getID() + " - " + getSelectedRiverNetworkNode().getName(), new GRLimits(node.getX(),node.getY(),node.getX(),node.getY()));
				}
			}
		}
		else if (source == __findNext)
		{
			searchWorksheet(__worksheet.getSelectedRow() + 1);
		}
		else if (source == __searchID || source == __searchName)
		{
			searchWorksheet(0);
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
		string label = "The following error" + plural + "encountered "
			+ "trying to save the record:\n";
		for (int i = 0; i < errorCount; i++)
		{
			label += errors[i] + "\n";
		}
		new ResponseJDialog(this, "Errors encountered", label, ResponseJDialog.OK);
		return false;
	}

	/// <summary>
	/// Checks the states of the map and network view buttons based on the selected river network node.
	/// </summary>
	private void checkViewButtonState()
	{
		StateMod_RiverNetworkNode rin = getSelectedRiverNetworkNode();
		if (rin.getGeoRecord() == null)
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
	~StateMod_RiverNetworkNode_JFrame()
	{
		__searchCriteriaGroup = null;
		__findNext = null;
		__helpJButton = null;
		__closeJButton = null;
		__searchIDJRadioButton = null;
		__searchNameJRadioButton = null;
		__searchID = null;
		__searchName = null;
		__worksheet = null;
		__dataset = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Finds a river network node in the __riverNetworkNodesVector that has the
	/// specified id. </summary>
	/// <param name="id"> the id to match. </param>
	/// <returns> the matching base flow coefficient object, or null if no matches could
	/// be found. </returns>
	/* TODO SAM 2007-03-01 Evaluate use
	private StateMod_RiverNetworkNode findRiverNetworkNode(String id) {
		StateMod_RiverNetworkNode rnn = null;
		for (int i = 0; i < __riverNetworkNodesVector.size(); i++) {
			rnn = (StateMod_RiverNetworkNode)
				__riverNetworkNodesVector.elementAt(i);
			if (rnn.getID().equals(id)) {
				return rnn;
			}
		}
		return null;
	}
	*/

	/// <summary>
	/// Get the selected river network node, based on the current index in the list.
	/// </summary>
	private StateMod_RiverNetworkNode getSelectedRiverNetworkNode()
	{
		return __riverNetworkNodesVector[__currentStationIndex];
	}

	/// <summary>
	/// Initializes the arrays that are used when items are selected and deselected.
	/// This should be called from setupGUI() before the a call is made to 
	/// selectTableIndex().
	/// </summary>
	private void initializeDisables()
	{
		__disables = new JComponent[4];
		int i = 0;
		__disables[i++] = __idJTextField;
		__disables[i++] = __nameJTextField;
		__disables[i++] = __nodeJTextField;
		__disables[i++] = __commentJTextField;

		__textUneditables = new int[2];
		__textUneditables[0] = 0;
		__textUneditables[1] = 1;
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
	/*
	Message.printStatus(1, "", "Current: " + __currentStationIndex);
	Message.printStatus(1, "", "Last: " + __lastStationIndex);
	Message.printStatus(1, "", "Orig: " + __worksheet.getOriginalRowNumber(index));
	Message.printStatus(1, "", "Index: " + index);
	*/
		__lastStationIndex = __currentStationIndex;
		__currentStationIndex = __worksheet.getOriginalRowNumber(index);

		saveLastRecord();

		if (__worksheet.getSelectedRow() == -1)
		{
			JGUIUtil.disableComponents(__disables, true);
			return;
		}

		JGUIUtil.enableComponents(__disables, __textUneditables, __editable);

		StateMod_RiverNetworkNode rnn = (StateMod_RiverNetworkNode) __riverNetworkNodesVector[__currentStationIndex];

		__idJTextField.setText(rnn.getID());
		__nameJTextField.setText(rnn.getName());
		__nodeJTextField.setText(rnn.getCstadn());
		__commentJTextField.setText(rnn.getComment());
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

		StateMod_RiverNetworkNode rnn = (StateMod_RiverNetworkNode) __riverNetworkNodesVector[record];

	Message.printStatus(1, "", "Setting " + record + " cstadn: " + __nodeJTextField.getText());
		rnn.setCstadn(__nodeJTextField.getText());
	Message.printStatus(1, "", "Setting " + record + " comment: " + __commentJTextField.getText());
		rnn.setComment(__commentJTextField.getText());
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
	/// Selects the desired ID in the left table and displays the appropriate data
	/// in the remainder of the window. </summary>
	/// <param name="id"> the identifier to select in the list. </param>
	public virtual void selectID(string id)
	{
		int rows = __worksheet.getRowCount();
		StateMod_RiverNetworkNode rnn = null;
		for (int i = 0; i < rows; i++)
		{
			rnn = (StateMod_RiverNetworkNode)__worksheet.getRowData(i);
			if (rnn.getID().Trim().Equals(id.Trim()))
			{
				selectTableIndex(i);
				return;
			}
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
	/// <param name="index"> the index of the network node to preselect. </param>
	private void setupGUI(int index)
	{
		string routine = "StateMod_RiverNetworkNode_JFrame.setupGUI";

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
		__nodeJTextField = new JTextField(12);
		__commentJTextField = new JTextField(24);

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

		int y = 0;

		PropList p = new PropList("StateMod_RiverNetworkNode_JFrame.JWorksheet");
		p.add("JWorksheet.ShowPopupMenu=true");
		p.add("JWorksheet.AllowCopy=true");
		p.add("JWorksheet.SelectionMode=SingleRowSelection");

		int[] widths = null;
		JScrollWorksheet jsw = null;
		try
		{
			StateMod_RiverNetworkNode_TableModel tmr = new StateMod_RiverNetworkNode_TableModel(__riverNetworkNodesVector);
			StateMod_RiverNetworkNode_CellRenderer crr = new StateMod_RiverNetworkNode_CellRenderer(tmr);

			jsw = new JScrollWorksheet(crr, tmr, p);
			__worksheet = jsw.getJWorksheet();

			widths = crr.getColumnWidths();
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
		JGUIUtil.addComponent(p1, jsw, 0, y, 4, 9, 1, 1, 0, 0, 0, 0, GridBagConstraints.BOTH, GridBagConstraints.WEST);

		JGUIUtil.addComponent(p1, new JLabel("ID:"), 5, y, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(p1, __idJTextField, 6, y++, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(p1, new JLabel("Name:"), 5, y, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(p1, __nameJTextField, 6, y++, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(p1, new JLabel("Downstream Node:"), 5, y, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(p1, __nodeJTextField, 6, y++, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(p1, new JLabel("Comment:"), 5, y, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(p1, __commentJTextField, 6, y++, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);


		//
		// add search areas
		//	

		y = 10;

		JPanel searchPanel = new JPanel();
		searchPanel.setLayout(gb);
		searchPanel.setBorder(BorderFactory.createTitledBorder("Search list for:     "));
		JGUIUtil.addComponent(p1, searchPanel, 0, y, 1, 1, 0, 0, 10, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(searchPanel, __searchIDJRadioButton, 0, ++y, 1, 1, 0, 0, 5, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(searchPanel, __searchID, 1, y, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.HORIZONTAL, GridBagConstraints.EAST);
		__searchID.addActionListener(this);
		JGUIUtil.addComponent(searchPanel, __searchNameJRadioButton, 0, ++y, 1, 1, 0, 0, 5, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		__searchName.setEditable(false);
		JGUIUtil.addComponent(searchPanel, __searchName, 1, y, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.HORIZONTAL, GridBagConstraints.EAST);
		__searchName.addActionListener(this);
		JGUIUtil.addComponent(searchPanel, __findNext, 0, ++y, 4, 1, 0, 0, 10, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		__findNext.addActionListener(this);

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
		__applyJButton.addActionListener(this);
		__cancelJButton.addActionListener(this);
		__helpJButton.addActionListener(this);
		__closeJButton.addActionListener(this);
		getContentPane().add("Center", p1);
		getContentPane().add("South", pfinal);

		initializeDisables();

		selectTableIndex(index);

		if (__dataset_wm != null)
		{
			__dataset_wm.setWindowOpen(StateMod_DataSet_WindowManager.WINDOW_RIVER_NETWORK, this);
		}

		pack();
		setSize(690, 400);
		JGUIUtil.center(this);
		setVisible(true);

		if (widths != null)
		{
			__worksheet.setColumnWidths(widths);
		}

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
	/// Responds to Window closing events; closes the window and marks it closed
	/// in StateMod_GUIUtil. </summary>
	/// <param name="e"> the WindowEvent that happened. </param>
	public virtual void windowClosing(WindowEvent e)
	{
		saveCurrentRecord();
		int size = __riverNetworkNodesVector.Count;
		StateMod_RiverNetworkNode r = null;
		bool changed = false;
		for (int i = 0; i < size; i++)
		{
			r = (StateMod_RiverNetworkNode) __riverNetworkNodesVector[i];
			if (!changed && r.changed())
			{
				changed = true;
			}
			r.acceptChanges();
		}
		if (changed)
		{
			__dataset.setDirty(StateMod_DataSet.COMP_RIVER_NETWORK, true);
		}
		if (__dataset_wm != null)
		{
			__dataset_wm.closeWindow(StateMod_DataSet_WindowManager.WINDOW_RIVER_NETWORK);
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

	/// <summary>
	/// Called just before the worksheet is sorted.  Stores the index of the record
	/// that is selected. </summary>
	/// <param name="worksheet"> the worksheet being sorted. </param>
	/// <param name="sort"> the type of sort being performed. </param>
	public virtual void worksheetSortAboutToChange(JWorksheet worksheet, int sort)
	{
		__sortSelectedRow = __worksheet.getOriginalRowNumber(__worksheet.getSelectedRow());
	}

	/// <summary>
	/// Called when the worksheet is sorted.  Reselects the record that was selected
	/// prior to the sort. </summary>
	/// <param name="worksheet"> the worksheet being sorted. </param>
	/// <param name="sort"> the type of sort being performed. </param>
	public virtual void worksheetSortChanged(JWorksheet worksheet, int sort)
	{
		__worksheet.deselectAll();
		__worksheet.selectRow(__worksheet.getSortedRowNumber(__sortSelectedRow));
	}

	}

}