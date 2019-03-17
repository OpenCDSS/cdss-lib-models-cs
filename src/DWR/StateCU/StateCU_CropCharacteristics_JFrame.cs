using System;
using System.Collections.Generic;

// StateCU_CropCharacteristics_JFrame - dialog to display crop char info

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
// StateCU_CropCharacteristics_JFrame - dialog to display crop char info
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
// 2003-07-14	J. Thomas Sapienza, RTi	Initial version.
// 2003-07-22	JTS, RTi		Revised following SAM's review.
// 2004-02-28	Steven A. Malers, RTi	Move some utility code from StateCU_Data
//					to StateCU_Util.
// 2005-01-17	JTS, RTi		Changed getOriginalRow() to 
//					getOriginalRowNumber().
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
//------------------------------------------------------------------------------

namespace DWR.StateCU
{


	using JGUIUtil = RTi.Util.GUI.JGUIUtil;
	using JWorksheet = RTi.Util.GUI.JWorksheet;
	using ResponseJDialog = RTi.Util.GUI.ResponseJDialog;
	using SimpleJComboBox = RTi.Util.GUI.SimpleJComboBox;
	using DataSetComponent = RTi.Util.IO.DataSetComponent;
	using PropList = RTi.Util.IO.PropList;
	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// This class is a GUI for displaying crop char data.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class StateCU_CropCharacteristics_JFrame extends javax.swing.JFrame implements java.awt.event.ActionListener, java.awt.event.KeyListener, java.awt.event.MouseListener, java.awt.event.WindowListener
	public class StateCU_CropCharacteristics_JFrame : JFrame, ActionListener, KeyListener, MouseListener, WindowListener
	{

	/// <summary>
	/// Button labels.
	/// </summary>
	private readonly string __BUTTON_FIND_NEXT = "Find Next", __BUTTON_ID = "ID", __BUTTON_NAME = "Name", __BUTTON_HELP = "Help", __BUTTON_CLOSE = "Close", __BUTTON_CANCEL = "Cancel", __BUTTON_APPLY = "Apply";

	/// <summary>
	/// Strings for the flag combo boxes.
	/// </summary>
	private readonly string __0_MEAN_TEMP = "0 - Mean Temp", __1_28_DEG_FROST = "1 - 28 Degree Frost", __2_32_DEG_FROST = "2 - 32 Degree Frost", __999_NONE = "-999 - N/A";

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
	/// Index of the currently-selected crop.
	/// </summary>
	private int __currentCropIndex;
	/// <summary>
	/// Index of the previously-selected crop.
	/// </summary>
	private int __lastCropIndex;

	/// <summary>
	/// GUI buttons.
	/// </summary>
	private JButton __findNextCrop;

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
	private JTextField __searchID, __searchName, __cropIDJTextField, __nameJTextField, __plantMonthJTextField, __plantDayJTextField, __harvestMonthJTextField, __harvestDayJTextField, __daysToCoverJTextField, __seasonLengthJTextField, __earliestValueJTextField, __latestValueJTextField, __maxRootFeetJTextField, __maxAppDepthJTextField, __firstDaysBetweenJTextField, __secondDaysBetweenJTextField;

	/// <summary>
	/// Worksheet for displaying crop information.
	/// </summary>
	private JWorksheet __worksheet;

	private JWorksheet __coeffWorksheet;

	private SimpleJComboBox __earliestFlagComboBox, __latestFlagComboBox;

	/// <summary>
	/// Dataset containing the crop data.
	/// </summary>
	private StateCU_DataSet __dataset;

	/// <summary>
	/// DataSetComponent containing crop data.
	/// </summary>
	private DataSetComponent __cropComponent;

	private DataSetComponent __blaneyComponent;

	/// <summary>
	/// List of crops data in the DataSetComponent.
	/// </summary>
	private IList<StateCU_CropCharacteristics> __cropsVector;

	private IList<StateCU_BlaneyCriddle> __blaneyVector;

	private StateCU_CropCharacteristics_TableModel __blaneyModel;

	private JTextField __messageJTextField, __statusJTextField;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="dataset"> dataset containing data to display </param>
	/// <param name="editable"> whether the display should be editable or not </param>
	public StateCU_CropCharacteristics_JFrame(StateCU_DataSet dataset, bool editable) : base(dataset.getBaseName() + " - StateCU GUI - Crop Characteristics" + " / Coefficients")
	{
		__currentCropIndex = -1;
		__editable = editable;

		__dataset = dataset;
		__cropComponent = __dataset.getComponentForComponentType(StateCU_DataSet.COMP_CROP_CHARACTERISTICS);
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<StateCU_CropCharacteristics> cropsVector0 = (java.util.List<StateCU_CropCharacteristics>)__cropComponent.getData();
		IList<StateCU_CropCharacteristics> cropsVector0 = (IList<StateCU_CropCharacteristics>)__cropComponent.getData();
		__cropsVector = cropsVector0;

		__blaneyComponent = __dataset.getComponentForComponentType(StateCU_DataSet.COMP_BLANEY_CRIDDLE);
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<StateCU_BlaneyCriddle> blaneyVector0 = (java.util.List<StateCU_BlaneyCriddle>)__blaneyComponent.getData();
		IList<StateCU_BlaneyCriddle> blaneyVector0 = (IList<StateCU_BlaneyCriddle>)__blaneyComponent.getData();
		__blaneyVector = blaneyVector0;

		setupGUI(0);
	}

	/// <summary>
	/// Constructor. </summary>
	/// <param name="crop"> StateCU_CropCharacteristics object to display </param>
	/// <param name="editable"> whether the display should be editable or not. </param>
	public StateCU_CropCharacteristics_JFrame(StateCU_DataSet dataset, StateCU_CropCharacteristics crop, bool editable) : base(dataset.getBaseName() + " - StateCU GUI - Crop Characteristics")
	{
		__currentCropIndex = -1;
		__editable = editable;

		__dataset = dataset;
		__cropComponent = __dataset.getComponentForComponentType(StateCU_DataSet.COMP_CROP_CHARACTERISTICS);
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<StateCU_CropCharacteristics> cropsVector0 = (java.util.List<StateCU_CropCharacteristics>)__cropComponent.getData();
		IList<StateCU_CropCharacteristics> cropsVector0 = (IList<StateCU_CropCharacteristics>)__cropComponent.getData();
		__cropsVector = cropsVector0;

		__blaneyComponent = __dataset.getComponentForComponentType(StateCU_DataSet.COMP_BLANEY_CRIDDLE);
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<StateCU_BlaneyCriddle> blaneyVector0 = (java.util.List<StateCU_BlaneyCriddle>)__blaneyComponent.getData();
		IList<StateCU_BlaneyCriddle> blaneyVector0 = (IList<StateCU_BlaneyCriddle>)__blaneyComponent.getData();
		__blaneyVector = blaneyVector0;

		string id = crop.getID();
		int index = StateCU_Util.IndexOf(__cropsVector, id);

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
			dispose();
		}
		else if (action.Equals(__BUTTON_CANCEL))
		{
			dispose();
		}
		else if (action.Equals(__BUTTON_APPLY))
		{
			saveCurrentRecord();
		}
		else if (source == __findNextCrop)
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
		// TODO SAM 2007-03-01 Why is message/logging not used?
		new ResponseJDialog(this, "Errors encountered", label, ResponseJDialog.OK);
		return false;
	}

	/// <summary>
	/// Clean up before garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~StateCU_CropCharacteristics_JFrame()
	{

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
	/// and getting the next selection's data out of the data and displaying it 
	/// on the form. </summary>
	/// <param name="index"> the index of the reservoir to display on the form. </param>
	private void processTableSelection(int index)
	{
		__lastCropIndex = __currentCropIndex;
		__currentCropIndex = __worksheet.getOriginalRowNumber(index);

		saveLastRecord();

		if (__worksheet.getSelectedRow() == -1)
		{
			nothingSelected();
			return;
		}

		somethingSelected();

		StateCU_CropCharacteristics crop = (StateCU_CropCharacteristics) __cropsVector[__currentCropIndex];

		__cropIDJTextField.setText(crop.getID());
		__nameJTextField.setText(crop.getName());
		StateCU_Util.checkAndSet(crop.getGdate1(), __plantMonthJTextField);
		StateCU_Util.checkAndSet(crop.getGdate2(), __plantDayJTextField);
		StateCU_Util.checkAndSet(crop.getGdate3(), __harvestMonthJTextField);
		StateCU_Util.checkAndSet(crop.getGdate4(), __harvestDayJTextField);
		StateCU_Util.checkAndSet(crop.getGdate5(), __daysToCoverJTextField);
		StateCU_Util.checkAndSet(crop.getGdates(), __seasonLengthJTextField);
		StateCU_Util.checkAndSet(crop.getTmois1(), __earliestValueJTextField);
		StateCU_Util.checkAndSet(crop.getTmois2(), __latestValueJTextField);
		StateCU_Util.checkAndSet(crop.getFrx(), __maxRootFeetJTextField);
		StateCU_Util.checkAndSet(crop.getApd(), __maxAppDepthJTextField);
		StateCU_Util.checkAndSet(crop.getCut2(), __firstDaysBetweenJTextField);
		StateCU_Util.checkAndSet(crop.getCut3(), __secondDaysBetweenJTextField);

		int flag = crop.getTflg1();
		if (flag == -999)
		{
			flag = 3;
		}
		__earliestFlagComboBox.select(flag);

		flag = crop.getTflg2();
		if (flag == -999)
		{
			flag = 3;
		}
		__latestFlagComboBox.select(flag);

		int bcindex = StateCU_Util.indexOfName(__blaneyVector, crop.getID());
		StateCU_BlaneyCriddle bc = null;
		if (bcindex != -1)
		{
			bc = (StateCU_BlaneyCriddle)__blaneyVector[bcindex];
			if (bc.getFlag().Equals("Percent", StringComparison.OrdinalIgnoreCase))
			{
				__coeffWorksheet.setColumnName(3, "PERCENT");
			}
			else
			{
				__coeffWorksheet.setColumnName(3, "DAY");
			}
		}
		__blaneyModel.setBlaneyCriddle(bc);
	}

	/// <summary>
	/// Saves the prior record selected in the table; called when moving to a new 
	/// record by a table selection.
	/// </summary>
	private void saveLastRecord()
	{
		saveInformation(__lastCropIndex);
	}

	/// <summary>
	/// Saves the current record selected in the table; called when the window is closed
	/// or minimized or apply is pressed.
	/// </summary>
	private void saveCurrentRecord()
	{
		saveInformation(__currentCropIndex);
	}

	/// <summary>
	/// Saves the information associated with the currently-selected crop.
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

		StateCU_CropCharacteristics crop = (StateCU_CropCharacteristics)__cropsVector[record];

		crop.setName(__nameJTextField.getText());
		crop.setID(__cropIDJTextField.getText());
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
	/// <param name="index"> the index in the worksheet to first select </param>
	private void setupGUI(int index)
	{
		string routine = "setupGUI";

		addWindowListener(this);

		JPanel p1 = new JPanel(); // first 6 months' effeciency
		//JPanel p2 = new JPanel();	// last 6 months' effeciency
		JPanel p3 = new JPanel(); // div sta id -> switch for diversion
		JPanel p4 = new JPanel(); // user name -> data type switch

		JPanel left_panel = new JPanel(); // multilist and search area
		JPanel right_panel = new JPanel(); // everything else

		__cropIDJTextField = new JTextField(12);
		__nameJTextField = new JTextField(24);
		__plantMonthJTextField = new JTextField(6);
		__plantDayJTextField = new JTextField(6);
		__harvestMonthJTextField = new JTextField(6);
		__harvestDayJTextField = new JTextField(6);
		__daysToCoverJTextField = new JTextField(6);
		__seasonLengthJTextField = new JTextField(6);
		__earliestValueJTextField = new JTextField(6);
		__latestValueJTextField = new JTextField(6);
		__maxRootFeetJTextField = new JTextField(6);
		__maxAppDepthJTextField = new JTextField(6);
		__firstDaysBetweenJTextField = new JTextField(6);
		__secondDaysBetweenJTextField = new JTextField(6);

		IList<string> v = new List<string>();
		v.Add(__0_MEAN_TEMP);
		v.Add(__1_28_DEG_FROST);
		v.Add(__2_32_DEG_FROST);
		v.Add(__999_NONE);
		__earliestFlagComboBox = new SimpleJComboBox(v);
		__latestFlagComboBox = new SimpleJComboBox(v);

		__searchID = new JTextField(10);
		__searchName = new JTextField(10);
		__searchName.setEditable(false);
		__findNextCrop = new JButton(__BUTTON_FIND_NEXT);
		__searchCriteriaGroup = new ButtonGroup();
		__searchIDJRadioButton = new JRadioButton(__BUTTON_ID, true);
		__searchNameJRadioButton = new JRadioButton(__BUTTON_NAME, false);
		__searchCriteriaGroup.add(__searchIDJRadioButton);
		__searchCriteriaGroup.add(__searchNameJRadioButton);

		JButton applyJButton = new JButton(__BUTTON_APPLY);
		JButton cancelJButton = new JButton(__BUTTON_CANCEL);
		JButton helpJButton = new JButton(__BUTTON_HELP);
		helpJButton.setEnabled(false);
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

		PropList p = new PropList("StateCU_CropCharacteristics_JFrame.JWorksheet");

		p.add("JWorksheet.CellFont=Courier");
		p.add("JWorksheet.CellStyle=Plain");
		p.add("JWorksheet.CellSize=11");
		p.add("JWorksheet.HeaderFont=Arial");
		p.add("JWorksheet.HeaderStyle=Plain");
		p.add("JWorksheet.HeaderSize=11");
		p.add("JWorksheet.HeaderBackground=LightGray");
		p.add("JWorksheet.RowColumnPresent=false");
		p.add("JWorksheet.ShowPopupMenu=true");
		p.add("JWorksheet.SelectionMode=SingleRowSelection");

		int[] widths = null;
		try
		{
			StateCU_CropCharacteristics_TableModel tmw = new StateCU_CropCharacteristics_TableModel(__cropsVector);
			StateCU_CropCharacteristics_CellRenderer crw = new StateCU_CropCharacteristics_CellRenderer(tmw);

			__worksheet = new JWorksheet(crw, tmw, p);

			__worksheet.removeColumn(3);
			__worksheet.removeColumn(4);
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

		JGUIUtil.addComponent(left_panel, new JScrollPane(__worksheet), 0, 0, 6, 14, 1, 1, 0, 0, 1, 1, GridBagConstraints.BOTH, GridBagConstraints.CENTER);

		y = 0;
		JGUIUtil.addComponent(p3, new JLabel("Crop ID:"), 0, y, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(p3, __cropIDJTextField, 1, y, 1, 1, 1, 1, 1, 0, 0, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);
		__cropIDJTextField.setEditable(false);

		y++;
		JGUIUtil.addComponent(p3, new JLabel("Name:"), 0, y, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(p3, __nameJTextField, 1, y, 3, 1, 1, 1, 1, 0, 0, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);

		y++;
		JGUIUtil.addComponent(p3, new JLabel("Planting Month and Day:"), 0, y, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(p3, __plantMonthJTextField, 1, y, 1, 1, 1, 1, 1, 0, 0, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(p3, __plantDayJTextField, 2, y, 2, 1, 1, 1, 1, 0, 0, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);

		y++;
		JGUIUtil.addComponent(p3, new JLabel("Harvest Month and Day:"), 0, y, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(p3, __harvestMonthJTextField, 1, y, 1, 1, 1, 1, 1, 0, 0, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(p3, __harvestDayJTextField, 2, y, 2, 1, 1, 1, 1, 0, 0, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);

		y++;
		JGUIUtil.addComponent(p3, new JLabel("Days to Full Cover:"), 0, y, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(p3, __daysToCoverJTextField, 1, y, 1, 1, 1, 1, 1, 0, 0, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);

		y++;
		JGUIUtil.addComponent(p3, new JLabel("Length of Season (days):"), 0, y, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(p3, __seasonLengthJTextField, 1, y, 1, 1, 1, 1, 1, 0, 0, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);

		y++;
		JGUIUtil.addComponent(p3, new JLabel("Earliest Moisture Use:"), 0, y, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(p3, __earliestFlagComboBox, 1, y, 1, 1, 1, 1, 1, 0, 0, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(p3, new JLabel("Value (F Deg.):"), 2, y, 1, 1, 1, 1, 1, 0, 0, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(p3, __earliestValueJTextField, 3, y, 1, 1, 1, 1, 1, 0, 0, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);

		y++;
		JGUIUtil.addComponent(p3, new JLabel("Latest Moisture Use:"), 0, y, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(p3, __latestFlagComboBox, 1, y, 1, 1, 1, 1, 1, 0, 0, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(p3, new JLabel("Value (F Deg.):"), 2, y, 1, 1, 1, 1, 1, 0, 0, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(p3, __latestValueJTextField, 3, y, 1, 1, 1, 1, 1, 0, 0, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);

		y++;
		JGUIUtil.addComponent(p3, new JLabel("Maximum Root Zone (feet):"), 0, y, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(p3, __maxRootFeetJTextField, 1, y, 1, 1, 1, 1, 1, 0, 0, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);

		y++;
		JGUIUtil.addComponent(p3, new JLabel("Maximum Application Depth (inches):"), 0, y, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(p3, __maxAppDepthJTextField, 1, y, 1, 1, 1, 1, 1, 0, 0, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);

		y++;
		JGUIUtil.addComponent(p3, new JLabel("Days between 1st and 2nd cuttings:"), 0, y, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(p3, __firstDaysBetweenJTextField, 1, y, 1, 1, 1, 1, 1, 0, 0, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);

		y++;
		JGUIUtil.addComponent(p3, new JLabel("Days between 2nd and 3rd cuttings:"), 0, y, 1, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(p3, __secondDaysBetweenJTextField, 1, y, 1, 1, 1, 1, 1, 0, 0, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);

		// two top panels of info
		JGUIUtil.addComponent(right_panel, p3, 0, 0, 2, 1, 0, 0, 0, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.NORTHWEST);

		int[] widths2 = null;
		try
		{
			__blaneyModel = new StateCU_CropCharacteristics_TableModel(__cropsVector);
			StateCU_CropCharacteristics_CellRenderer crw = new StateCU_CropCharacteristics_CellRenderer(__blaneyModel);

			__coeffWorksheet = new JWorksheet(crw, __blaneyModel, p);

			__coeffWorksheet.removeColumn(1);
			__coeffWorksheet.removeColumn(2);
			widths2 = crw.getColumnWidths();
		}
		catch (Exception e)
		{
			Message.printWarning(2, routine, e);
			__coeffWorksheet = new JWorksheet(0, 0, p);
			Console.WriteLine(e.ToString());
			Console.Write(e.StackTrace);
		}
		__coeffWorksheet.setPreferredScrollableViewportSize(null);
		__coeffWorksheet.setHourglassJFrame(this);

		JScrollPane jsp = new JScrollPane(__coeffWorksheet);
		jsp.setBorder(BorderFactory.createTitledBorder(jsp.getBorder(), "Blaney-Criddle Crop Coefficients"));
		JGUIUtil.addComponent(right_panel, jsp, 0, y, 4, 4, 1, 1, 0, 0, 0, 0, GridBagConstraints.BOTH, GridBagConstraints.NORTHWEST);

		// add search areas
		y = 14;
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
		JGUIUtil.addComponent(searchPanel, __findNextCrop, 0, y2, 4, 1, 0, 0, 20, 10, 20, 10, GridBagConstraints.NONE, GridBagConstraints.WEST);
		__findNextCrop.addActionListener(this);
		// add buttons which lead to crop
		// direct flow demand, and return flow information
		FlowLayout fl = new FlowLayout(FlowLayout.CENTER);

		// add help and close buttons
		y++;
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

	//	JGUIUtil.center(this);
		pack();
		setSize(850,620);
		selectTableIndex(index);
		setVisible(true);

		if (widths != null)
		{
			__worksheet.setColumnWidths(widths);
		}
		if (widths2 != null)
		{
			__coeffWorksheet.setColumnWidths(widths2);
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
		__disables = new JComponent[2];
		__disables[0] = __latestFlagComboBox;
		__disables[1] = __earliestFlagComboBox;

		__textDisables = new JTextComponent[14];
		__textDisables[0] = __cropIDJTextField;
		__textDisables[1] = __nameJTextField;
		__textDisables[2] = __plantMonthJTextField;
		__textDisables[3] = __plantDayJTextField;
		__textDisables[4] = __harvestMonthJTextField;
		__textDisables[5] = __harvestDayJTextField;
		__textDisables[6] = __daysToCoverJTextField;
		__textDisables[7] = __seasonLengthJTextField;
		__textDisables[8] = __earliestValueJTextField;
		__textDisables[9] = __latestValueJTextField;
		__textDisables[10] = __maxRootFeetJTextField;
		__textDisables[11] = __maxAppDepthJTextField;
		__textDisables[12] = __firstDaysBetweenJTextField;
		__textDisables[13] = __secondDaysBetweenJTextField;

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
				if (__disables[i] is SimpleJComboBox)
				{
					((SimpleJComboBox)__disables[i]).setEditable(false);
				}
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
	}

	}

}