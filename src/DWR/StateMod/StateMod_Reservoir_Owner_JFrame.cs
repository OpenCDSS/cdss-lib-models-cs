using System;
using System.Collections.Generic;

// StateMod_Reservoir_Owner_JFrame - dialog to edit a reservoir's ownership information

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
// StateMod_Reservoir_Owner_JFrame - dialog to edit a reservoir's 
//	ownership information
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
// 
// 24 Dec 1997	Catherine E.		Created initial version of class
//		Nutting-Lane, RTi
// 25 Feb 1998	CEN, RTi		Added header information
// 01 Apr 2001	Steven A. Malers, RTi	Change GUI to JGUIUtil.  Add finalize().
//					Remove import *.
//------------------------------------------------------------------------------
// 2003-06-09	J. Thomas Sapienza, RTi	Initial swing version from 
//					SMresOwnersFrame.
// 2003-06-16	JTS, RTi		Javadoc'd.
// 2003-07-15	JTS, RTi		* Added status bar.
//					* Changed to use new dataset design.
// 2003-07-17	JTS, RTI		Change so that constructor takes a 
//					boolean that says whether the form's
//					data can be modified.
// 2003-07-23	JTS, RTi		Updated JWorksheet code following
//					JWorksheet revisions.
// 2003-08-03	SAM, RTi		Changed isDirty() back to setDirty().
// 2003-08-29	SAM, RTi		Update due to changes in
//					StateMod_Reservoir.
// 2003-09-19	JTS, RTi		Account ID is automatically generated
//					upon adding a new account.
// 2003-09-23	JTS, RTi		Uses new StateMod_GUIUtil code for
//					setting titles.
// 2003-10-13	JTS, RTi		* Worksheet now uses multiple-line
//					  headers.
// 					* Added saveData().
//					* Added checkInput().
//					* Added apply and cancel buttons.
// 2004-01-21	JTS, RTi		Updated to use JScrollWorksheet and
//					the new row headers.
// 2004-07-15	JTS, RTi		Changed layout of buttons to be
//					aligned in the lower-right.
// 2004-10-28	SAM, RTi		Use the table model specific to account
//					data.
// 2005-01-21	JTS, RTi		Table model constructor changed.
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

	/// <summary>
	/// This class displays reservoir owner account information and allows 
	/// owner accounts to be added or deleted from the current reservoir.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class StateMod_Reservoir_Owner_JFrame extends javax.swing.JFrame implements java.awt.event.ActionListener, java.awt.event.KeyListener, java.awt.event.MouseListener, java.awt.event.WindowListener
	public class StateMod_Reservoir_Owner_JFrame : JFrame, ActionListener, KeyListener, MouseListener, WindowListener
	{

	/// <summary>
	/// Strings to be displayed on the action buttons.
	/// </summary>
	private const string __BUTTON_ADD_OWNER = "Add owner", __BUTTON_APPLY = "Apply", __BUTTON_CANCEL = "Cancel", __BUTTON_DEL_OWNER = "Delete owner", __BUTTON_HELP = "Help", __BUTTON_CLOSE = "Close";

	/// <summary>
	/// Whether the gui data is editable or not.
	/// </summary>
	private bool __editable = false;

	/// <summary>
	/// GUI Buttons.
	/// </summary>
	private JButton __addOwner, __closeJButton, __deleteOwner, __helpJButton;

	/// <summary>
	/// Status bar textfields 
	/// </summary>
	private JTextField __messageJTextField, __statusJTextField;

	/// <summary>
	/// JWorksheet to display owner data.
	/// </summary>
	private JWorksheet __worksheet;

	/// <summary>
	/// The dataset in which the data is stored.
	/// </summary>
	private StateMod_DataSet __dataset;

	/// <summary>
	/// The current reservoir for which data will be displayed.
	/// </summary>
	private StateMod_Reservoir __currentRes = null;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="dataset"> the dataset in which the data is contained. </param>
	/// <param name="res"> the reservoir for which to display data </param>
	/// <param name="editable"> whether the gui data is editable or not. </param>
	public StateMod_Reservoir_Owner_JFrame(StateMod_DataSet dataset, StateMod_Reservoir res, bool editable)
	{
		StateMod_GUIUtil.setTitle(this, dataset, res.getName() + " - Reservoir Owner Accounts", null);
		JGUIUtil.setIcon(this, JGUIUtil.getIconImage());
		__currentRes = res;

		__dataset = dataset;

		__editable = editable;

		setupGUI();
	}

	/// <summary>
	/// Responds to action performed events. </summary>
	/// <param name="e"> the ActionEvent that occurred. </param>
	public virtual void actionPerformed(ActionEvent e)
	{
		string action = e.getActionCommand();

		if (action.Equals(__BUTTON_ADD_OWNER))
		{
			StateMod_ReservoirAccount anAccount = new StateMod_ReservoirAccount();
			anAccount._isClone = true;
			int rowCount = __worksheet.getRowCount();
			if (rowCount == 0)
			{
				anAccount.setID(1);
			}
			else
			{
				StateMod_ReservoirAccount lastAccount = (StateMod_ReservoirAccount) __worksheet.getLastRowData();
				string id = lastAccount.getID();
				anAccount.setID("" + ((Convert.ToInt32(id)) + 1));
			}
			__worksheet.addRow(anAccount);
			__worksheet.scrollToLastRow();
			__worksheet.selectLastRow();
			__deleteOwner.setEnabled(true);
		}
		else if (action.Equals(__BUTTON_DEL_OWNER))
		{
			int row = __worksheet.getSelectedRow();
			if (row != -1)
			{
				int x = (new ResponseJDialog(this, "Delete owner", "Delete owner?", ResponseJDialog.YES | ResponseJDialog.NO)).response();
				if (x == ResponseJDialog.NO)
				{
					return;
				}
				__worksheet.deleteRow(row);
				__deleteOwner.setEnabled(false);
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
		else if (action.Equals(__BUTTON_HELP))
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
		string routine = "StateMod_Reservoir_Owner_JFrame.checkInput";
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<StateMod_ReservoirAccount> v = (java.util.List<StateMod_ReservoirAccount>)__worksheet.getAllData();
		IList<StateMod_ReservoirAccount> v = (IList<StateMod_ReservoirAccount>)__worksheet.getAllData();

		int size = v.Count;
		StateMod_ReservoirAccount acct = null;
		string warning = "";
		string id;
		string name;
		int fatalCount = 0;
		int lastID = 0;
		int currID = 0;

		if (size > 0)
		{
			acct = v[0];

			id = acct.getID();
			if (!id.Trim().Equals("1"))
			{
				warning += "\nThe first reservoir account must have "
					+ "an ID of '1', not '" + id + "'";
				fatalCount++;
			}
		}

		for (int i = 0; i < size; i++)
		{
			acct = v[i];

			id = acct.getID();

			if (i == 0)
			{
				lastID = (Convert.ToInt32(id));
			}
			else
			{
				currID = (Convert.ToInt32(id));
				if (currID > (lastID + 1))
				{
					warning += "\nOwner ID values must be "
						+ "consecutive (row #" + (i) + " is "
						+ lastID + ", row #" + (i + 1) + " is "
						+ currID + ")";
					fatalCount++;
				}
				lastID = currID;
			}

			name = acct.getName();

			if (id.Length > 12)
			{
				warning += "\nReservoir acct ID (" + id + ") is "
					+ "longer than 12 characters.";
				fatalCount++;
			}

			if (id.IndexOf(" ", StringComparison.Ordinal) > -1 || id.IndexOf("-", StringComparison.Ordinal) > -1)
			{
				warning += "\nReservoir acct ID (" + id + ") cannot "
					+ "contain spaces or dashes.";
				fatalCount++;
			}

			if (name.Length > 24)
			{
				warning += "\nReservoir name (" + name + ") is "
					+ "longer than 24 characters.";
				fatalCount++;
			}

			/* REVISIT SAM 2004-10-29 should be enforced by the table
			model since it is a choice
			ownerTie = acct.getN2owns();
			if (ownerTie == null) {
				warning += "\nMust fill in Ownership Tie.";
				fatalCount++;
			}
			*/

			// the rest are handled automatically by the worksheet
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
		string routine = "StateMod_Reservoir_Owner_JFrame.saveData";
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
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<StateMod_ReservoirAccount> wv = (java.util.List<StateMod_ReservoirAccount>)__worksheet.getAllData();
		IList<StateMod_ReservoirAccount> wv = (IList<StateMod_ReservoirAccount>)__worksheet.getAllData(); // w for worksheet
		IList<StateMod_ReservoirAccount> rv = __currentRes.getAccounts(); // i for instream flow

		needToSave = !(StateMod_ReservoirAccount.Equals(wv, rv));

		Message.printStatus(1, routine, "Saving? .........[" + needToSave + "]");

		if (!needToSave)
		{
			// there's nothing different -- users may even have deleted
			// some rights and added back in identical values
			return true;
		}

		// now add the elements from the new Vector to the reservoirRights 
		// Vector.
		int size = wv.Count;
		IList<StateMod_ReservoirAccount> clone = new List<StateMod_ReservoirAccount>();
		StateMod_ReservoirAccount ra;
		for (int i = 0; i < size; i++)
		{
			ra = (StateMod_ReservoirAccount)wv[i].clone();
			clone.Add(ra);
		}

		__currentRes.setAccounts(clone);
		__dataset.setDirty(StateMod_DataSet.COMP_RESERVOIR_STATIONS, true);
		return true;
	}

	/// <summary>
	/// Checks to see if the __deleteOwner button should be enabled or not.
	/// </summary>
	private void checkDeleteOwnerButton()
	{
		int row = __worksheet.getSelectedRow();
		if (row == -1)
		{
			__deleteOwner.setEnabled(false);
		}
		else
		{
			__deleteOwner.setEnabled(true);
		}
	}

	/// <summary>
	/// Helper method used when data is put into the table model. </summary>
	/// <param name="n2own"> the value of n2own </param>
	/// <returns> a String to display in the gui for n2own. </returns>
	/* TODO SAM 2007-03-01 Evaluate use
	private String fillN2owns(int n2own) {
		if (n2own == 1) {
			return "1 - To First Fill Right(s)";
		}
		else {
			return "2 - To Second Fill Right(s)";
		}
	}
	*/

	/// <summary>
	/// Clean up before garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~StateMod_Reservoir_Owner_JFrame()
	{
		__currentRes = null;
		__worksheet = null;
		__addOwner = null;
		__deleteOwner = null;
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
	/// Responds to key released events; checks if the __deleteOwner button should
	/// be enabled. </summary>
	/// <param name="e"> the KeyEvent that happened. </param>
	public virtual void keyReleased(KeyEvent e)
	{
		checkDeleteOwnerButton();
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
	/// Responds to mouse released events; checks to see if the __deleteOwner button
	/// should be enabled or not. </summary>
	/// <param name="e"> the MouseEvent that happened. </param>
	public virtual void mouseReleased(MouseEvent e)
	{
		checkDeleteOwnerButton();
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
	private void setupGUI()
	{
		string routine = "setupGUI";

		addWindowListener(this);

		__addOwner = new JButton(__BUTTON_ADD_OWNER);
		__deleteOwner = new JButton(__BUTTON_DEL_OWNER);
		__deleteOwner.setEnabled(false);
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

		info_panel.add(new JLabel("Reservoir ID:"));
		info_panel.add(new JLabel(__currentRes.getID()));
		info_panel.add(new JLabel("Reservoir name:"));
		info_panel.add(new JLabel(__currentRes.getName()));

		if (__editable)
		{
			p1.add(__addOwner);
			p1.add(__deleteOwner);
		}
		p1.add(applyJButton);
		p1.add(cancelJButton);
	//	p1.add(__helpJButton);
		p1.add(__closeJButton);

		PropList p = new PropList("StateMod_Reservoir_JFrame.JWorksheet");
		p.add("JWorksheet.ShowPopupMenu=true");
		p.add("JWorksheet.AllowCopy=true");
		p.add("JWorksheet.SelectionMode=SingleRowSelection");

		int[] widths = null;
		JScrollWorksheet jsw = null;
		try
		{
			IList<StateMod_ReservoirAccount> v = new List<StateMod_ReservoirAccount>();
			IList<StateMod_ReservoirAccount> v2 = __currentRes.getAccounts();
			StateMod_ReservoirAccount ra;
			for (int i = 0; i < v2.Count; i++)
			{
				ra = (StateMod_ReservoirAccount)v2[i].clone();
				v.Add(ra);
			}
			StateMod_ReservoirAccount_TableModel tmr = new StateMod_ReservoirAccount_TableModel(v, __editable, true);
			StateMod_ReservoirAccount_CellRenderer crr = new StateMod_ReservoirAccount_CellRenderer(tmr);

			jsw = new JScrollWorksheet(crr, tmr, p);
			__worksheet = jsw.getJWorksheet();

			IList<string> owner = StateMod_ReservoirAccount.getN2ownChoices(true);
			__worksheet.setColumnJComboBoxValues(StateMod_ReservoirAccount_TableModel.COL_OWNERSHIP_TIE, owner, false);

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

		main_panel.add(jsw, "Center");
		main_panel.add(p1, "South");

		JGUIUtil.addComponent(bigPanel, info_panel, 0, 0, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.NORTHWEST);
		JGUIUtil.addComponent(bigPanel, main_panel, 0, 1, 10, 10, 1, 1, GridBagConstraints.BOTH, GridBagConstraints.SOUTH);
		__addOwner.addActionListener(this);
		__deleteOwner.addActionListener(this);
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
		setSize(700, 400);
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