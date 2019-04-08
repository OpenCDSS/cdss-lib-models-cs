using System;
using System.Collections.Generic;

// StateMod_InstreamFlow_Right_JFrame - dialog to edit a reservoir's rights information

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
// StateMod_InstreamFlow_Right_JFrame - dialog to edit a reservoir's 
//	rights information
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
// 
// 28 Dec 1997	Catherine E.		Created initial version of class.
//		Nutting-Lane, RTi
// 25 Feb 1998	CEN, RTi		Adding information panel.
// 01 Apr 2001	Steven A. Malers, RTi	Change GUI to JGUIUtil.  Add finalize().
//					Remove import *.
// 2002-09-19	SAM, RTi		Use isDirty()instead of setDirty()to
//					indicate edits.
//------------------------------------------------------------------------------
// 2003-06-10	J. Thomas Sapienza, RTi	Initial swing version from 
//					SMinsfRightsFrame
// 2003-07-15	JTS, RTi		* Added status bar.
//					* Changed to use new dataset design.
// 2003-07-17	JTS, RTI		Change so that constructor takes a 
//					boolean that says whether the form's
//					data can be modified.
// 2003-07-23	JTS, RTi		Updated JWorksheet code following
//					JWorksheet revisions.
// 2003-08-03	SAM, RTi		Changed isDirty() back to setDirty().
// 2003-08-29	SAM, RTi		Update due to changes in
//					StateMod_InstreamFlow.
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
// 2004-08-26	JTS, RTi		The on/off column again has a combo box
//					from which the user can choose values.
// 2004-10-28	SAM, RTi		Update to handle separate table model
//					for rights.
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
	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// This class is a GUI for displaying and editing instream flow rights.
	/// </summary>
	public class StateMod_InstreamFlow_Right_JFrame : JFrame, ActionListener, KeyListener, MouseListener, WindowListener
	{

	/// <summary>
	/// Button labels.
	/// </summary>
	private readonly string __BUTTON_ADD_RIGHT = "Add right", __BUTTON_APPLY = "Apply", __BUTTON_CANCEL = "Cancel", __BUTTON_CLOSE = "Close", __BUTTON_DEL_RIGHT = "Delete right", __BUTTON_HELP = "Help";

	/// <summary>
	/// Whether the gui data is editable.
	/// </summary>
	private bool __editable = false;

	/// <summary>
	/// GUI buttons.
	/// </summary>
	private JButton __addRight, __deleteRight, __helpJButton, __closeJButton;

	/// <summary>
	/// Status bar textfields 
	/// </summary>
	private JTextField __messageJTextField, __statusJTextField;

	/// <summary>
	/// The worksheet in which data is displayed.
	/// </summary>
	private JWorksheet __worksheet;

	/// <summary>
	/// The dataset containing the data.
	/// </summary>
	private StateMod_DataSet __dataset;

	/// <summary>
	/// The instream flow for which to display right information.
	/// </summary>
	private StateMod_InstreamFlow __currentInstreamFlow;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="dataset"> the dataset in which the data is contained. </param>
	/// <param name="insf"> the InstreamFlow right to display. </param>
	/// <param name="editable"> whether the gui data is editable or not </param>
	public StateMod_InstreamFlow_Right_JFrame(StateMod_DataSet dataset, StateMod_InstreamFlow insf, bool editable)
	{
		StateMod_GUIUtil.setTitle(this, dataset, insf.getName() + " - Instream Flow Water Rights", null);
		JGUIUtil.setIcon(this, JGUIUtil.getIconImage());
		__currentInstreamFlow = insf;

		__dataset = dataset;

		__editable = editable;

		setupGUI();
	}

	/// <summary>
	/// Responds to action performed events. </summary>
	/// <param name="e"> the ActionEvent that happened. </param>
	public virtual void actionPerformed(ActionEvent e)
	{
		string routine = "StateMod_InstreamFlow_Right_JFrame.actionPerformed";

		string action = e.getActionCommand();
		if (action.Equals(__BUTTON_ADD_RIGHT))
		{
			StateMod_InstreamFlowRight aRight = new StateMod_InstreamFlowRight();
			aRight._isClone = true;
			StateMod_InstreamFlowRight last = (StateMod_InstreamFlowRight) __worksheet.getLastRowData();

			if (last == null)
			{
				aRight.setID(StateMod_Util.createNewID(__currentInstreamFlow.getID()));
				aRight.setCgoto(__currentInstreamFlow.getID());
			}
			else
			{
				aRight.setID(StateMod_Util.createNewID(last.getID()));
				aRight.setCgoto(last.getCgoto());
			}
			__worksheet.addRow(aRight);
			__worksheet.scrollToLastRow();
			__worksheet.selectLastRow();
			__deleteRight.setEnabled(true);
		}
		else if (action.Equals(__BUTTON_DEL_RIGHT))
		{
			int row = __worksheet.getSelectedRow();
			if (row != -1)
			{
				int x = (new ResponseJDialog(this, "Delete right", "Delete instream flow right?", ResponseJDialog.YES | ResponseJDialog.NO)).response();

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
				Message.printWarning(1, routine, "Must selected desired right to delete.");
			}
		}
		else if (action.Equals(__BUTTON_HELP))
		{
			// REVISIT HELP (JTS - 2003-06-10)
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
	}

	/// <summary>
	/// Checks the data to make sure that all the data are valid. </summary>
	/// <returns> 0 if the data are valid, 1 if errors exist and -1 if non-fatal errors
	/// exist. </returns>
	private int checkInput()
	{
		string routine = "StateMod_InstreamFlow_Right_JFrame.checkInput";
		System.Collections.IList v = __worksheet.getAllData();

		int size = v.Count;
		StateMod_InstreamFlowRight right = null;
		string warning = "";
		string id;
		string name;
		string isfID;
		string adminNum;
		int fatalCount = 0;
		for (int i = 0; i < size; i++)
		{
			right = (StateMod_InstreamFlowRight)(v[i]);

			id = right.getID();
			name = right.getName();
			isfID = right.getCgoto();
			adminNum = right.getIrtem();

			if (id.Length > 12)
			{
				warning += "\nInstream flow right ID (" + id + ") is longer than 12 characters.";
				fatalCount++;
			}

			if (id.IndexOf(" ", StringComparison.Ordinal) > -1 || id.IndexOf("-", StringComparison.Ordinal) > -1)
			{
				warning += "\nInstream flow right ID (" + id + ") cannot contain spaces or dashes.";
				fatalCount++;
			}

			if (name.Length > 24)
			{
				warning += "\nInstream flow name (" + name + ") is "
					+ "longer than 24 characters.";
				fatalCount++;
			}

			if (isfID.Length > 12)
			{
				warning += "\nInstream flow ID associated with right ("
					+ isfID + ") is longer than 12 characters.";
			}

			if (!StringUtil.isDouble(adminNum))
			{
				warning += "\nAdministration number (" + adminNum + ") is not a number.";
				fatalCount++;
			}


			// decreed amount is not checked to be a double because that
			// is enforced by the worksheet and its table model

			// on/off is not checked to be an integer because that is
			// enforced by the worksheet and its table model

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
		string routine = "StateMod_InstreamFlow_Right_JFrame.saveData";
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
		System.Collections.IList wv = __worksheet.getAllData(); // w for worksheet
		System.Collections.IList iv = __currentInstreamFlow.getRights(); // i for instream flow

		needToSave = !(StateMod_InstreamFlowRight.Equals(wv, iv));

		Message.printStatus(1, routine, "Saving? .........[" + needToSave + "]");

		if (!needToSave)
		{
			// there's nothing different -- users may even have deleted
			// some rights and added back in identical values
			return true;
		}

		// at this point, remove the old diversion rights from the original
		// component Vector
		System.Collections.IList instreamFlowRights = (System.Collections.IList)(__dataset.getComponentForComponentType(StateMod_DataSet.COMP_INSTREAM_RIGHTS)).getData();
		int size = iv.Count;
		StateMod_InstreamFlowRight ir;
		for (int i = 0; i < size; i++)
		{
			ir = (StateMod_InstreamFlowRight)iv[i];
			StateMod_Util.removeFromVector(instreamFlowRights, ir);
		}

		// now add the elements from the new Vector to the instreamFlowRights 
		// Vector.
		size = wv.Count;
		StateMod_InstreamFlowRight cir = null;
		for (int i = 0; i < size; i++)
		{
			ir = (StateMod_InstreamFlowRight)wv[i];
			cir = (StateMod_InstreamFlowRight)(ir.clone());
			cir._isClone = false;
			instreamFlowRights.Add(cir);
		}

		// sort the instreamFlowRights Vector
		// REVISIT (JTS - 2003-10-10)
		// here we are sorting the full data array -- may be a performance
		// issue
		System.Collections.IList sorted = StateMod_Util.sortStateMod_DataVector(instreamFlowRights);
		__dataset.getComponentForComponentType(StateMod_DataSet.COMP_INSTREAM_RIGHTS).setData(sorted);
		__currentInstreamFlow.disconnectRights();
		__currentInstreamFlow.connectRights(sorted);
		__dataset.setDirty(StateMod_DataSet.COMP_INSTREAM_RIGHTS, true);
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
	~StateMod_InstreamFlow_Right_JFrame()
	{
		__worksheet = null;
		__addRight = null;
		__deleteRight = null;
		__helpJButton = null;
		__closeJButton = null;
		__currentInstreamFlow = null;
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
	private void setupGUI()
	{
		string routine = "setupGUI";

		addWindowListener(this);

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

		info_panel.add(new JLabel("Instream flow:"));
		info_panel.add(new JLabel(__currentInstreamFlow.getID()));
		info_panel.add(new JLabel("Instream flow name:"));
		info_panel.add(new JLabel(__currentInstreamFlow.getName()));

		if (__editable)
		{
			p1.add(__addRight);
			p1.add(__deleteRight);
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
			System.Collections.IList v = new List<object>();
			System.Collections.IList v2 = __currentInstreamFlow.getRights();
			for (int i = 0; i < v2.Count; i++)
			{
				v.Add(((StateMod_InstreamFlowRight)(v2[i])).clone());
			}
			StateMod_InstreamFlowRight_TableModel tmi = new StateMod_InstreamFlowRight_TableModel(v, __editable, true);
			StateMod_InstreamFlowRight_CellRenderer cri = new StateMod_InstreamFlowRight_CellRenderer(tmi);

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

		System.Collections.IList v = new List<object>();
		v.Add("0 - Off");
		v.Add("1 - On");
		__worksheet.setColumnJComboBoxValues(StateMod_InstreamFlowRight_TableModel.COL_ON_OFF, v);

		main_panel.add(jsw, "Center");
		main_panel.add(p1, "South");

		JGUIUtil.addComponent(bigPanel, info_panel, 0, 0, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.NORTHWEST);
		JGUIUtil.addComponent(bigPanel, main_panel, 0, 1, 10, 10, 1, 1, GridBagConstraints.BOTH, GridBagConstraints.SOUTH);
		__addRight.addActionListener(this);
		__deleteRight.addActionListener(this);
		__helpJButton.addActionListener(this);
		__closeJButton.addActionListener(this);
		cancelJButton.addActionListener(this);
		applyJButton.addActionListener(this);

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