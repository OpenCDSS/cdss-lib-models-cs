﻿using System;
using System.Collections.Generic;

// StateMod_Well_ReturnFlow_JFrame - dialog to edit a well's return flow information

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
// StateMod_Well_ReturnFlow_JFrame - dialog to edit a well's return 
//	flow information
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
// 
// 18 Oct 1999	Catherine E.		Created initial version of class
//		Nutting-Lane, RTi
// 01 Apr 2001	Steven A. Malers, RTi	Change GUI to JGUIUtil.  Add finalize().
//					Remove import *.
//------------------------------------------------------------------------------
// 2003-06-10	J. Thomas Sapienza, RTi	Initial swing version from 
//					SMwellReturnFlowFrame
// 2003-06-24	JTS, RTi		First functioning version.
// 2003-07-15	JTS, RTi		* Added status bar.
//					* Changed to use new dataset design.
// 2003-07-17	JTS, RTI		Change so that constructor takes a 
//					boolean that says whether the form's
//					data can be modified.
// 2003-07-23	JTS, RTi		Updated JWorksheet code following
//					JWorksheet revisions.
// 2003-08-03	SAM, RTi		Changed isDirty() back to setDirty().
// 2003-08-29	SAM, RTi		Update due to changes in the
//					StateMod_Well class.
// 2003-09-23	JTS, RTi		Uses new StateMod_GUIUtil code for
//					setting titles.
// 2003-10-01	SAM, RTi		Set the component type in the
//					constructor for StateMod_ReturnFlow.
// 2003-10-13	JTS, RTi		* Worksheet now uses multiple-line
//					  headers.
// 					* Added saveData().
//					* Added checkInput().
//					* Added apply and cancel buttons.
// 2004-01-22	JTS, RTi		Updated to use JScrollWorksheet and
//					the new row headers.
// 2004-07-15	JTS, RTi		* For data changes, enabled the
//					  Apply and Cancel buttons through new
//					  methods in the data classes.
//					* Changed layout of buttons to be
//					  aligned in the lower-right.
// 2004-10-27	SAM, RTi		Use new return flow table model instead
//					of model shared with other well data.
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
	/// This class is a GUI for displaying and editing well return flow data.
	/// </summary>
	public class StateMod_Well_ReturnFlow_JFrame : JFrame, ActionListener, KeyListener, MouseListener, WindowListener
	{

	/// <summary>
	/// Button labels.
	/// </summary>
	private readonly string __BUTTON_ADD_RETURN_FLOW = "Add return flow", __BUTTON_APPLY = "Apply", __BUTTON_CANCEL = "Cancel", __BUTTON_DEL_RETURN_FLOW = "Delete return flow", __BUTTON_CLOSE = "Close", __BUTTON_HELP = "Help";

	/// <summary>
	/// Whether the gui data is editable or not.
	/// </summary>
	private bool __editable = false;

	/// <summary>
	/// GUI Buttons.
	/// </summary>
	private JButton __addReturnFlow, __deleteReturnFlow, __helpJButton, __closeJButton;

	/// <summary>
	/// Status bar textfields 
	/// </summary>
	private JTextField __messageJTextField, __statusJTextField;

	/// <summary>
	/// The worksheet in which the well return flow data is displayed.
	/// </summary>
	private JWorksheet __worksheet;

	/// <summary>
	/// The dataset that contains the data.
	/// </summary>
	private StateMod_DataSet __dataset;

	/// <summary>
	/// The well for which the return flow data is shown.
	/// </summary>
	private StateMod_Well __currentWell;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="dataset"> the dataset in which the data is contained. </param>
	/// <param name="well"> the well to display. </param>
	/// <param name="editable"> whether the gui data is editable or not. </param>
	public StateMod_Well_ReturnFlow_JFrame(StateMod_DataSet dataset, StateMod_Well well, bool editable)
	{
		StateMod_GUIUtil.setTitle(this, dataset, well.getName() + " - Well Return Flow", null);
		JGUIUtil.setIcon(this, JGUIUtil.getIconImage());
		__currentWell = well;

		__dataset = dataset;

		__editable = editable;

		setupGUI();
	}

	/// <summary>
	/// Responds to action performed events. </summary>
	/// <param name="e"> the ActionEvent that happened. </param>
	public virtual void actionPerformed(ActionEvent e)
	{
		string action = e.getActionCommand();

		if (action.Equals(__BUTTON_ADD_RETURN_FLOW))
		{
			StateMod_ReturnFlow aReturnFlow = new StateMod_ReturnFlow(StateMod_DataSet.COMP_WELL_STATIONS);
			aReturnFlow._isClone = true;
			__worksheet.addRow(aReturnFlow);
			__worksheet.scrollToLastRow();
			__worksheet.selectLastRow();
			__deleteReturnFlow.setEnabled(true);
		}
		else if (action.Equals(__BUTTON_DEL_RETURN_FLOW))
		{
			int row = __worksheet.getSelectedRow();
			if (row != -1)
			{
				int x = (new ResponseJDialog(this, "Delete Return Flow", "Delete return flow?", ResponseJDialog.YES | ResponseJDialog.NO)).response();
				if (x == ResponseJDialog.NO)
				{
					return;
				}
				__worksheet.cancelEditing();
				__worksheet.deleteRow(row);
				__deleteReturnFlow.setEnabled(false);
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
			// REVISIT HELP (JTS - 2003-06-10)
		}
	}

	/// <summary>
	/// Saves the input back into the dataset. </summary>
	/// <returns> true if the data was saved successfuly.  False if not. </returns>
	private bool saveData()
	{
		string routine = "StateMod_Well_ReturnFlow_JFrame.saveData";
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
		System.Collections.IList lv = __currentWell.getReturnFlows(); // l for welL

		needToSave = !(StateMod_ReturnFlow.Equals(wv, lv));

		Message.printStatus(1, routine, "Saving? .........[" + needToSave + "]");

		if (!needToSave)
		{
			// there's nothing different -- users may even have deleted
			// some rights and added back in identical values
			return true;
		}

		// clone the objects from the worksheet vector and assign them
		// to the well object as its new return flows.
		int size = wv.Count;
		System.Collections.IList clone = new List<object>();
		StateMod_ReturnFlow rf;
		StateMod_ReturnFlow crf = null;
		for (int i = 0; i < size; i++)
		{
			rf = (StateMod_ReturnFlow)wv[i];
			crf = (StateMod_ReturnFlow)rf.clone();
			crf.setCrtnid(StringUtil.getToken(rf.getCrtnid(), " ", StringUtil.DELIM_SKIP_BLANKS, 0));
			crf._isClone = false;
			clone.Add(crf);
		}

		__currentWell.setReturnFlows(clone);
		__dataset.setDirty(StateMod_DataSet.COMP_WELL_STATIONS, true);

		return true;
	}

	/// <summary>
	/// Checks the data to make sure that all the data are valid. </summary>
	/// <returns> 0 if the data are valid, 1 if errors exist and -1 if non-fatal errors
	/// exist. </returns>
	private int checkInput()
	{
		string routine = "StateMod_Well_ReturnFlow_JFrame.checkInput";
		System.Collections.IList v = __worksheet.getAllData();

		int size = v.Count;
		StateMod_ReturnFlow rf = null;
		string warning = "";
		string riverNode;
		int fatalCount = 0;
		for (int i = 0; i < size; i++)
		{
			rf = (StateMod_ReturnFlow)(v[i]);
			riverNode = rf.getCrtnid();
			riverNode = StringUtil.getToken(riverNode, " ", StringUtil.DELIM_SKIP_BLANKS, 0);
			if (string.ReferenceEquals(riverNode, null))
			{
				riverNode = "";
				warning += "\nMust specify a River Node ID.";
				fatalCount++;
			}
			if (riverNode.Length > 12)
			{
				warning += "\nRiver Node ID (" + riverNode + ") is "
					+ "longer than 12 characters.";
				fatalCount++;
			}

			if (riverNode.IndexOf(" ", StringComparison.Ordinal) > -1 || riverNode.IndexOf("-", StringComparison.Ordinal) > -1)
			{
				warning += "\nRiver Node ID (" + riverNode + ") cannot contain spaces or dashes.";
				fatalCount++;
			}
		}
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
	/// Checks to see if the __deleteReturnFlow button should be enabled or not.
	/// </summary>
	private void checkDeleteReturnFlowButton()
	{
		int row = __worksheet.getSelectedRow();
		if (row == -1)
		{
			__deleteReturnFlow.setEnabled(false);
		}
		else
		{
			__deleteReturnFlow.setEnabled(true);
		}
	}

	/// <summary>
	/// Clean up before garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~StateMod_Well_ReturnFlow_JFrame()
	{
		__addReturnFlow = null;
		__deleteReturnFlow = null;
		__helpJButton = null;
		__closeJButton = null;
		__worksheet = null;
		__currentWell = null;

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
	/// Responds to key released events; checks if the __deleteReturnFlow button should
	/// be enabled. </summary>
	/// <param name="e"> the KeyEvent that happened. </param>
	public virtual void keyReleased(KeyEvent e)
	{
		checkDeleteReturnFlowButton();
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
	/// Responds to mouse released events; checks to see if the __deleteReturnFlow 
	/// button should be enabled or not. </summary>
	/// <param name="e"> the MouseEvent that happened. </param>
	public virtual void mouseReleased(MouseEvent e)
	{
		checkDeleteReturnFlowButton();
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

		__addReturnFlow = new JButton(__BUTTON_ADD_RETURN_FLOW);
		__deleteReturnFlow = new JButton(__BUTTON_DEL_RETURN_FLOW);
		__deleteReturnFlow.setEnabled(false);
		__helpJButton = new JButton(__BUTTON_HELP);
		__helpJButton.setEnabled(false);
		__closeJButton = new JButton(__BUTTON_CLOSE);
		JButton cancelJButton = new JButton(__BUTTON_CANCEL);
		JButton applyJButton = new JButton(__BUTTON_APPLY);

		GridBagLayout gb = new GridBagLayout();
		JPanel mainJPanel = new JPanel();
		mainJPanel.setLayout(gb);

		FlowLayout fl = new FlowLayout(FlowLayout.RIGHT);
		JPanel p1 = new JPanel();
		p1.setLayout(fl);

		GridLayout gl = new GridLayout(2, 2, 1, 1);
		JPanel info_panel = new JPanel();
		info_panel.setLayout(gl);

		JPanel main_panel = new JPanel();
		main_panel.setLayout(new BorderLayout());

		info_panel.add(new JLabel("Well:"));
		info_panel.add(new JLabel(__currentWell.getID()));
		info_panel.add(new JLabel("Well name:"));
		info_panel.add(new JLabel(__currentWell.getName()));

		if (__editable)
		{
			p1.add(__addReturnFlow);
			p1.add(__deleteReturnFlow);
		}
		p1.add(applyJButton);
		p1.add(cancelJButton);
	//	p1.add(__helpJButton);
		p1.add(__closeJButton);

		PropList p = new PropList("StateMod_Well_ReturnFlow_JFrame.JWorksheet");
		p.add("JWorksheet.ShowPopupMenu=true");
		p.add("JWorksheet.AllowCopy=true");
		p.add("JWorksheet.SelectionMode=SingleRowSelection");

		int[] widths = null;
		JScrollWorksheet jsw = null;
		try
		{
			System.Collections.IList nodes = (System.Collections.IList)(__dataset.getComponentForComponentType(StateMod_DataSet.COMP_RIVER_NETWORK).getData());
			System.Collections.IList v = new List<object>();
			System.Collections.IList v2 = __currentWell.getReturnFlows();
			StateMod_ReturnFlow rf;
			for (int i = 0; i < v2.Count; i++)
			{
				rf = (StateMod_ReturnFlow)((StateMod_ReturnFlow)v2[i]).clone();
				rf.setCrtnid(rf.getCrtnid() + StateMod_Util.findNameInVector(rf.getCrtnid(), nodes, true));
				v.Add(rf);
			}

			StateMod_ReturnFlow_TableModel tmw = new StateMod_ReturnFlow_TableModel(__dataset, v,__editable, true);
			StateMod_ReturnFlow_CellRenderer crw = new StateMod_ReturnFlow_CellRenderer(tmw);

			jsw = new JScrollWorksheet(crw, tmw, p);
			__worksheet = jsw.getJWorksheet();

			v = StateMod_Util.createIdentifierListFromStateModData(nodes, true, null);
			__worksheet.setColumnJComboBoxValues(StateMod_ReturnFlow_TableModel.COL_RIVER_NODE, v,false);

			// 10
			System.Collections.IList delayIDs = null;
			if (__dataset.getIday() == 1)
			{
				delayIDs = (System.Collections.IList)(__dataset.getComponentForComponentType(StateMod_DataSet.COMP_DELAY_TABLES_DAILY).getData());
			}
			else
			{
				delayIDs = (System.Collections.IList)(__dataset.getComponentForComponentType(StateMod_DataSet.COMP_DELAY_TABLES_MONTHLY).getData());
			}
			v = StateMod_Util.createIdentifierListFromStateModData(delayIDs, true, null);
			__worksheet.setColumnJComboBoxValues(StateMod_ReturnFlow_TableModel.COL_RETURN_ID, v, false);
			widths = crw.getColumnWidths();
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
		JGUIUtil.addComponent(mainJPanel, info_panel, 0, 0, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.NORTHWEST);
		JGUIUtil.addComponent(mainJPanel, main_panel, 0, 1, 10, 10, 1, 1, GridBagConstraints.BOTH, GridBagConstraints.SOUTH);

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

		__addReturnFlow.addActionListener(this);
		__deleteReturnFlow.addActionListener(this);
		__closeJButton.addActionListener(this);
		__helpJButton.addActionListener(this);
		applyJButton.addActionListener(this);
		cancelJButton.addActionListener(this);

		pack();
		setSize(520, 280);
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