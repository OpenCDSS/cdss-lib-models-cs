using System;
using System.Collections.Generic;

// StateMod_Reservoir_Right_JFrame - dialog to edit a reservoir's rights information

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
// StateMod_Reservoir_Right_JFrame - dialog to edit a reservoir's 
//	rights information
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
// 
// 16 Dec 1997	Catherine E.s		Created initial version of class
//		Nutting-Lane, RTi
// 25 Feb 1998	CEN, RTi		Adding information panel.
// 17 Mar 1998	CEN, RTi		Adding delete right.
// 01 Apr 2001	Steven A. Malers, RTi	Change GUI to JGUIUtil.  Add finalize().
//					Remove import *.
// 2002-09-19	SAM, RTi		Use isDirty()instead of setDirty()to
//					indicate edits.
//------------------------------------------------------------------------------
// 2003-06-09	J. Thomas Sapienza, RTi	Initial swing version from 
//					SMresRightsFrame
// 2003-07-15	JTS, RTi		* Added status bar.
//					* Changed to use new dataset design.
// 2003-07-17	JTS, RTI		Change so that constructor takes a 
//					boolean that says whether the form's
//					data can be modified.
// 2003-07-23	JTS, RTi		Updated JWorksheet code following
//					JWorksheet revisions.
// 2003-08-03	SAM, RTi		Changed isDirty() back to setDirty().
// 2003-08-28	SAM, RTi		Changed to reflect changes in
//					StateMod_ReservoirRight - no longer a
//					linked list.
// 2003-09-23	JTS, RTi		Uses new StateMod_GUIUtil code for
//					setting titles.
// 2003-10-13	JTS, RTi		* Worksheet now uses multiple-line
//					  headers.
// 					* Added saveData().
//					* Added checkInput().
//					* Added apply and cancel buttons.
// 2004-01-21	JTS, RTi		Updated to use JScrollWorksheet and
//					the new row headers.
// 2004-07-15	JTS, RTi		* For data changes, enabled the
//					  Apply and Cancel buttons through new
//					  methods in the data classes.
//					* Changed layout of buttons to be
//					  aligned in the lower-right.
// 2004-10-28	SAM, RTi		Use new table model that contains only
//					reservoir rights.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
//------------------------------------------------------------------------------
// EndHeader

namespace DWR.StateMod
{


	using JGUIUtil = RTi.Util.GUI.JGUIUtil;
	using JScrollWorksheet = RTi.Util.GUI.JScrollWorksheet;
	using JWorksheet = RTi.Util.GUI.JWorksheet;
	using ResponseJDialog = RTi.Util.GUI.ResponseJDialog;
	using PropList = RTi.Util.IO.PropList;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// This class is a gui for displaying the rights associated with a reservoir, 
	/// as well as deleting and adding rights with that reservoir.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class StateMod_Reservoir_Right_JFrame extends javax.swing.JFrame implements java.awt.event.ActionListener, java.awt.event.KeyListener, java.awt.event.MouseListener, java.awt.event.WindowListener
	public class StateMod_Reservoir_Right_JFrame : JFrame, ActionListener, KeyListener, MouseListener, WindowListener
	{

	/// <summary>
	/// Labels for the buttons.
	/// </summary>
	private const string __BUTTON_ADD_RIGHT = "Add right", __BUTTON_APPLY = "Apply", __BUTTON_CANCEL = "Cancel", __BUTTON_DEL_RIGHT = "Delete right", __BUTTON_HELP = "Help", __BUTTON_CLOSE = "Close";

	/// <summary>
	/// Whether the gui data is editable or not.
	/// </summary>
	private bool __editable = false;

	/// <summary>
	/// GUI buttons.
	/// </summary>
	private JButton __addRight, __closeJButton, __deleteRight, __helpJButton;

	/// <summary>
	/// Status bar textfields 
	/// </summary>
	private JTextField __messageJTextField, __statusJTextField;

	/// <summary>
	/// Worksheet in which the rights are shown.
	/// </summary>
	private JWorksheet __worksheet;

	/// <summary>
	/// Dataset that contains the data.
	/// </summary>
	private StateMod_DataSet __dataset;

	/// <summary>
	/// The current reservoir for which rights are being shown.
	/// </summary>
	private StateMod_Reservoir __currentRes;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="dataset"> the dataset in which the data is contained. </param>
	/// <param name="res"> the Reservoir about which to display right information. </param>
	/// <param name="editable"> whether the gui data is editable or not. </param>
	public StateMod_Reservoir_Right_JFrame(StateMod_DataSet dataset, StateMod_Reservoir res, bool editable)
	{
		StateMod_GUIUtil.setTitle(this, dataset, res.getName() + " - Reservoir Water Rights", null);
		JGUIUtil.setIcon(this, JGUIUtil.getIconImage());
		__currentRes = res;

		__dataset = dataset;

		__editable = editable;

		setupGUI();
	}

	/// <summary>
	/// Responds to action performed events. </summary>
	/// <param name="e"> the ActionEvent that happened. </param>
	public virtual void actionPerformed(ActionEvent e)
	{
		string routine = "StateMod_Reservoir_Right_JFrame::actionPerformed";

		string action = e.getActionCommand();

		if (action.Equals(__BUTTON_ADD_RIGHT))
		{
			StateMod_ReservoirRight aRight = new StateMod_ReservoirRight();
			aRight._isClone = true;
			StateMod_ReservoirRight last = (StateMod_ReservoirRight)__worksheet.getLastRowData();

			if (last == null)
			{
				aRight.setID(StateMod_Util.createNewID(__currentRes.getID()));
				aRight.setCgoto(__currentRes.getID());
			}
			else
			{
				aRight.setID(StateMod_Util.createNewID(last.getID()));
				aRight.setCgoto(last.getCgoto());
			}
			__worksheet.scrollToLastRow();
			__worksheet.addRow(aRight);
			__worksheet.selectLastRow();
			__deleteRight.setEnabled(true);
		}
		else if (action.Equals(__BUTTON_DEL_RIGHT))
		{
			int row = __worksheet.getSelectedRow();
			if (row != -1)
			{
				int x = (new ResponseJDialog(this, "Delete right", "Delete reservoir right?", ResponseJDialog.YES | ResponseJDialog.NO)).response();
				if (x == ResponseJDialog.NO)
				{
					return;
				}

				__worksheet.cancelEditing();
				__worksheet.deleteRow(row);
				__deleteRight.setEnabled(false);
			}
			else
			{
				Message.printWarning(1, routine, "Must select desired right to delete.");
			}
		}
		else if (action.Equals(__BUTTON_CLOSE))
		{
			if (saveData())
			{
				setVisible(false);
				dispose();
			}
		}
		else if (action.Equals(__BUTTON_APPLY))
		{
			saveData();
		}
		else if (action.Equals(__BUTTON_CANCEL))
		{
			setVisible(false);
			dispose();
		}
		else if (e.getSource() == __helpJButton)
		{
			// REVISIT HELP (JTS - 2003-06-09)
		}
	}

	/// <summary>
	/// Checks the data to make sure that all the data are valid. </summary>
	/// <returns> 0 if the data are valid, 1 if errors exist and -1 if non-fatal errors
	/// exist. </returns>
	private int checkInput()
	{
		string routine = "StateMod_Reservoir_Right_JFrame.checkInput";
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<StateMod_ReservoirRight> v = (java.util.List<StateMod_ReservoirRight>)__worksheet.getAllData();
		IList<StateMod_ReservoirRight> v = (IList<StateMod_ReservoirRight>)__worksheet.getAllData();

		int size = v.Count;
		StateMod_ReservoirRight right = null;
		string warning = "";
		string id;
		string name;
		string resID;
		string adminNum;
		int fatalCount = 0;
		for (int i = 0; i < size; i++)
		{
			right = (StateMod_ReservoirRight)(v[i]);

			id = right.getID();
			name = right.getName();
			resID = right.getCgoto();
			adminNum = right.getRtem();
			//copID = right.getCopid();

			if (id.Length > 12)
			{
				warning += "\nReservoir right ID (" + id + ") is "
					+ "longer than 12 characters.";
				fatalCount++;
			}

			if (id.IndexOf(" ", StringComparison.Ordinal) > -1 || id.IndexOf("-", StringComparison.Ordinal) > -1)
			{
				warning += "\nReservoir right ID (" + id + ") cannot "
					+ "contain spaces or dashes.";
				fatalCount++;
			}

			if (name.Length > 24)
			{
				warning += "\nReservoir name (" + name + ") is "
					+ "longer than 24 characters.";
				fatalCount++;
			}

			if (resID.Length > 12)
			{
				warning += "\nReservoir ID associated with right ("
					+ resID + ") is longer than 12 characters.";
			}

			if (!StringUtil.isDouble(adminNum))
			{
				warning += "\nAdministration number (" + adminNum + ") is not a number.";
				fatalCount++;
			}

			/* REVISIT SAM 2004-10-29 These don't need to be checked
			because they are enforced by the table model?
	
			switchx = right.getSwitch();
			iresco = right.getIresco();
			ityrstr = right.getItyrstr();
			n2fill = right.getN2fill();
	
			if (switchx == null) {
				warning += "\nMust fill in On/Off switch.";
				fatalCount++;
			}
			if (iresco == null) {
				warning += "\nMust fill in Accounts served.";
				fatalCount++;
			}
			if (ityrstr == null) {
				warning += "\nMust fill in Right type.";
				fatalCount++;
			}
			if (n2fill == null) {
				warning += "\nMust fill in Fill type.";
				fatalCount++;
			}
			*/

			// copid is not handled yet

			// decreed amount is not checked to be a double because that
			// is enforced by the worksheet and its table model
		}
		// REVISIT - if daily time series are supplied, check for time series
		// and allow creation if not available.
		if (warning.Length > 0)
		{
			warning += "\nCorrect or Cancel.";
			Message.printWarning(1, routine, warning, this);
			if (fatalCount > 0)
			{
				// Fatal errors...
				Message.printStatus(1, routine, "Returning 1 from checkInput()");
				return 1;
			}
			else
			{ // Nonfatal errors...
				Message.printStatus(1, routine, "Returning -1 from checkInput()");
				return -1;
			}
		}
		else
		{ // No errors...
			Message.printStatus(1, routine, "Returning 0 from checkInput()");
			return 0;
		}
	}

	/// <summary>
	/// Saves the input back into the dataset. </summary>
	/// <returns> true if the data was saved successfuly.  False if not. </returns>
	private bool saveData()
	{
		string routine = "StateMod_Reservoir_Right_JFrame.saveData";
		if (!__worksheet.stopEditing())
		{
			// don't save if there are errors.
			Message.printWarning(1, routine, "There are errors in the data " + "that must be corrected before data can be saved.", this);
			return false;
		}

		if (checkInput() > 0)
		{
			return false;
		}

		// now only save data if any are different.
		bool needToSave = false;

		// if the Vectors are differently-sized, they're different
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<StateMod_ReservoirRight> wv = (java.util.List<StateMod_ReservoirRight>)__worksheet.getAllData();
		IList<StateMod_ReservoirRight> wv = (IList<StateMod_ReservoirRight>)__worksheet.getAllData(); // w for worksheet
		IList<StateMod_ReservoirRight> rv = __currentRes.getRights(); // i for reservoir

		needToSave = !(StateMod_ReservoirRight.Equals(wv, rv));

		Message.printStatus(1, routine, "Saving? .........[" + needToSave + "]");

		if (!needToSave)
		{
			// there's nothing different -- users may even have deleted
			// some rights and added back in identical values
			return true;
		}

		// at this point, remove the old diversion rights from the original
		// component list
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<StateMod_ReservoirRight> reservoirRights = (java.util.List<StateMod_ReservoirRight>)(__dataset.getComponentForComponentType(StateMod_DataSet.COMP_RESERVOIR_RIGHTS)).getData();
		IList<StateMod_ReservoirRight> reservoirRights = (IList<StateMod_ReservoirRight>)(__dataset.getComponentForComponentType(StateMod_DataSet.COMP_RESERVOIR_RIGHTS)).getData();
		int size = rv.Count;
		StateMod_ReservoirRight ir;
		for (int i = 0; i < size; i++)
		{
			ir = (StateMod_ReservoirRight)rv[i];
			StateMod_Util.removeFromVector(reservoirRights, ir);
		}

		// now add the elements from the new Vector to the reservoirRights 
		// Vector.
		size = wv.Count;
		StateMod_ReservoirRight cdr = null;

		for (int i = 0; i < size; i++)
		{
			ir = (StateMod_ReservoirRight)wv[i];
			cdr = (StateMod_ReservoirRight)ir.clone();
			cdr._isClone = false;
			reservoirRights.Add(cdr);
		}

		// sort the reservoirRights Vector
		// REVISIT (JTS - 2003-10-10)
		// here we are sorting the full data array -- may be a performance
		// issue
		IList<StateMod_ReservoirRight> sorted = StateMod_Util.sortStateMod_DataVector(reservoirRights);
		__dataset.getComponentForComponentType(StateMod_DataSet.COMP_RESERVOIR_RIGHTS).setData(sorted);
		__currentRes.disconnectRights();
		__currentRes.connectRights(sorted);
		__dataset.setDirty(StateMod_DataSet.COMP_RESERVOIR_RIGHTS, true);
		return true;
	}

	/// <summary>
	/// Checks to see if the __deleteRight button should be enabled or not.
	/// </summary>
	private void checkDeleteRightButton()
	{
		int row = __worksheet.getSelectedRow();
		if (row == -1)
		{
			__deleteRight.setEnabled(false);
		}
		else
		{
			__deleteRight.setEnabled(true);
		}
	}

	/// <summary>
	/// Clean up before garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~StateMod_Reservoir_Right_JFrame()
	{
		__currentRes = null;
		__worksheet = null;
		__addRight = null;
		__deleteRight = null;
		__helpJButton = null;
		__closeJButton = null;
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
	/// Responds to key released events; checks if the __deleteRight button should
	/// be enabled. </summary>
	/// <param name="e"> the KeyEvent that happened. </param>
	public virtual void keyReleased(KeyEvent e)
	{
		checkDeleteRightButton();
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
	/// Responds to mouse pressed events; does nothing. </summary>
	/// <param name="e"> the MouseEvent that happened. </param>
	public virtual void mousePressed(MouseEvent e)
	{
	}

	/// <summary>
	/// Responds to mouse released events; checks to see if the __deleteRight button
	/// should be enabled or not. </summary>
	/// <param name="e"> the MouseEvent that happened. </param>
	public virtual void mouseReleased(MouseEvent e)
	{
		checkDeleteRightButton();
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
	/// Sets up the GUI.
	/// </summary>
	public virtual void setupGUI()
	{
		string routine = "setupGUI";

		addWindowListener(this);

		PropList p = new PropList("StateMod_Reservoir_JFrame.JWorksheet");
		p.add("JWorksheet.AllowCopy=true");
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
		JScrollWorksheet jsw = null;
		try
		{
			IList<StateMod_ReservoirAccount> accounts = __currentRes.getAccounts();
			IList<string> v3 = new List<string>();
			int size = accounts.Count;
			StateMod_ReservoirAccount ra = null;
			for (int i = 0; i < size; i++)
			{
				ra = accounts[i];
				v3.Add("" + ra.getID() + " - " + ra.getName());
			}
			for (int i = 1; i < size; i++)
			{
				v3.Add("-" + (i + 1) + " - Fill first " + (i + 1) + " accounts");
			}

			IList<StateMod_ReservoirRight> v = new List<StateMod_ReservoirRight>();
			IList<StateMod_ReservoirRight> v2 = __currentRes.getRights();
			StateMod_ReservoirRight rr;
			for (int i = 0; i < v2.Count; i++)
			{
				rr = (StateMod_ReservoirRight)v2[i].clone();
				v.Add(rr);
			}
			StateMod_ReservoirRight_TableModel tmr = new StateMod_ReservoirRight_TableModel(v, __editable);
			StateMod_ReservoirRight_CellRenderer crr = new StateMod_ReservoirRight_CellRenderer(tmr);

			jsw = new JScrollWorksheet(crr, tmr, p);
			__worksheet = jsw.getJWorksheet();

			IList<string> onOff = StateMod_ReservoirRight.getIrsrswChoices(true);
			__worksheet.setColumnJComboBoxValues(StateMod_ReservoirRight_TableModel.COL_ON_OFF, onOff, false);
			__worksheet.setColumnJComboBoxValues(StateMod_ReservoirRight_TableModel.COL_ACCOUNT_DIST, v3, false);
			IList<string> rightTypes = StateMod_ReservoirRight.getItyrsrChoices(true);
			__worksheet.setColumnJComboBoxValues(StateMod_ReservoirRight_TableModel.COL_RIGHT_TYPE, rightTypes, false);
			IList<string> fillTypes = StateMod_ReservoirRight.getN2fillChoices(true);
			__worksheet.setColumnJComboBoxValues(StateMod_ReservoirRight_TableModel.COL_FILL_TYPE, fillTypes, false);

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

		__addRight = new JButton(__BUTTON_ADD_RIGHT);
		__deleteRight = new JButton(__BUTTON_DEL_RIGHT);
		__deleteRight.setEnabled(false);
		__helpJButton = new JButton(__BUTTON_HELP);
		__helpJButton.setEnabled(false);
		__closeJButton = new JButton(__BUTTON_CLOSE);
		JButton cancelJButton = new JButton(__BUTTON_CANCEL);
		JButton applyJButton = new JButton(__BUTTON_APPLY);

		GridBagLayout gb = new GridBagLayout();
		JPanel bigPanel = new JPanel();
		bigPanel.setLayout(gb);

		FlowLayout fl = new FlowLayout(FlowLayout.RIGHT);
		JPanel p1 = new JPanel();
		p1.setLayout(fl);

		GridLayout gl = new GridLayout(2, 2, 1, 1);
		JPanel info_panel = new JPanel();
		info_panel.setLayout(gl);

		JPanel main_panel = new JPanel();
		main_panel.setLayout(new BorderLayout());

		info_panel.add(new JLabel("Reservoir:"));
		info_panel.add(new JLabel(__currentRes.getID()));
		info_panel.add(new JLabel("Reservoir name:"));
		info_panel.add(new JLabel(__currentRes.getName()));

		if (__editable)
		{
			p1.add(__addRight);
			p1.add(__deleteRight);
		}
		p1.add(applyJButton);
		p1.add(cancelJButton);
	//	p1.add(__helpJButton);
		p1.add(__closeJButton);

		main_panel.add(jsw, "Center");
		main_panel.add(p1, "South");

		JGUIUtil.addComponent(bigPanel, info_panel, 0, 0, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.NORTHWEST);
		JGUIUtil.addComponent(bigPanel, main_panel, 0, 1, 10, 10, 1, 1, GridBagConstraints.BOTH, GridBagConstraints.SOUTH);
		__addRight.addActionListener(this);
		__deleteRight.addActionListener(this);
		__helpJButton.addActionListener(this);
		__closeJButton.addActionListener(this);
		applyJButton.addActionListener(this);
		cancelJButton.addActionListener(this);

		getContentPane().add(bigPanel);

		JPanel bottomJPanel = new JPanel();
		bottomJPanel.setLayout(gb);
		__messageJTextField = new JTextField();
		__messageJTextField.setEditable(false);
		JGUIUtil.addComponent(bottomJPanel, __messageJTextField, 0, 0, 7, 1, 1.0, 0.0, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);
		__statusJTextField = new JTextField(5);
		__statusJTextField.setEditable(false);
		JGUIUtil.addComponent(bottomJPanel, __statusJTextField, 7, 0, 1, 1, 0.0, 0.0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		getContentPane().add("South", bottomJPanel);

		pack();
		setSize(760, 400);
		JGUIUtil.center(this);
		setVisible(true);
		if (widths != null)
		{
			__worksheet.setColumnWidths(widths);
		}
	}

	/// <summary>
	/// Responds to window activated events; does nothing. </summary>
	/// <param name="e"> the WindowEvent that happened. </param>
	public virtual void windowActivated(WindowEvent e)
	{
	}

	/// <summary>
	/// Responds to window closed events; does nothing. </summary>
	/// <param name="e"> the WindowEvent that happened. </param>
	public virtual void windowClosed(WindowEvent e)
	{
	}

	/// <summary>
	/// Responds to window closing events; closes up the window properly. </summary>
	/// <param name="e"> the WindowEvent that happened. </param>
	public virtual void windowClosing(WindowEvent e)
	{
		if (saveData())
		{
			setDefaultCloseOperation(WindowConstants.DISPOSE_ON_CLOSE);
			setVisible(false);
			dispose();
		}
		else
		{
			setDefaultCloseOperation(WindowConstants.DO_NOTHING_ON_CLOSE);
		}
	}

	/// <summary>
	/// Responds to window deactivated events; does nothing. </summary>
	/// <param name="e"> the WindowEvent that happened. </param>
	public virtual void windowDeactivated(WindowEvent e)
	{
	}

	/// <summary>
	/// Responds to window deiconified events; does nothing. </summary>
	/// <param name="e"> the WindowEvent that happened. </param>
	public virtual void windowDeiconified(WindowEvent e)
	{
	}

	/// <summary>
	/// Responds to window iconified events; does nothing. </summary>
	/// <param name="e"> the WindowEvent that happened. </param>
	public virtual void windowIconified(WindowEvent e)
	{
	}

	/// <summary>
	/// Responds to window opened events; does nothing. </summary>
	/// <param name="e"> the WindowEvent that happened. </param>
	public virtual void windowOpened(WindowEvent e)
	{
	}

	/// <summary>
	/// Responds to window opening events; does nothing. </summary>
	/// <param name="e"> the WindowEvent that happened. </param>
	public virtual void windowOpening(WindowEvent e)
	{
	}

	}

}