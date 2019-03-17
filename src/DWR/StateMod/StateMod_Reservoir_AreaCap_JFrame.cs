﻿using System;
using System.Collections.Generic;

// StateMod_Reservoir_AreaCap_JFrame - frame to edit a reservoir's content/area/seepage curve

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
// StateMod_Reservoir_AreaCap_JFrame - frame to edit a reservoir's 
//	content/area/seepage curve
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
// 
// 24 Dec 1997	Catherine E.		Created initial version of class
//		Nutting-Lane, RTi
// 01 Apr 2001	Steven A. Malers, RTi	Change GUI to JGUIUtil.  Add finalize().
//					Remove import *.
//------------------------------------------------------------------------------
// 2003-06-09	J. Thomas Sapienza, RTi	Initial swing version from 
//					SMresAreaCapFrame
// 2003-06-13	JTS, RTi		Began using StateMod_Reservoir code
// 2003-06-16	JTS, RTi		Javadoc'd.
// 2003-07-15	JTS, RTi		* Added status bar.
//					* Changed to use new dataset design.
// 2003-07-17	JTS, RTI		Change so that constructor takes a 
//					boolean that says whether the form's
//					data can be modified.
// 2003-07-23	JTS, RTi		Updated JWorksheet code following
//					JWorksheet revisions.
// 2003-08-03	SAM, RTi		Changed isDirty() back to setDirty().
// 2003-08-29	SAM, RTi		Change due to changes in
//					StateMod_Reservoir.
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
// 2004-10-28	SAM, RTi		Use new table model that deals only with
//					area/cap data.
// 2005-01-21	JTS, RTi		Table model constructor changed.
// 2006-02-28	SAM, RTi		* Use JFreeChart to display the area
//					  capacity curves.
//					* Change the "capacity" in the title to
//					  "content".
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
//------------------------------------------------------------------------------
// EndHeader

namespace DWR.StateMod
{


	using JGUIUtil = RTi.Util.GUI.JGUIUtil;
	using JScrollWorksheet = RTi.Util.GUI.JScrollWorksheet;
	using JWorksheet = RTi.Util.GUI.JWorksheet;
	using ResponseJDialog = RTi.Util.GUI.ResponseJDialog;
	using SimpleJButton = RTi.Util.GUI.SimpleJButton;
	using IOUtil = RTi.Util.IO.IOUtil;
	using PropList = RTi.Util.IO.PropList;
	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// This class is a gui for displaying area cap information associated with a 
	/// reservoir, and deleting and adding area cap information to the reservoir.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class StateMod_Reservoir_AreaCap_JFrame extends javax.swing.JFrame implements java.awt.event.ActionListener, java.awt.event.KeyListener, java.awt.event.MouseListener, java.awt.event.WindowListener
	public class StateMod_Reservoir_AreaCap_JFrame : JFrame, ActionListener, KeyListener, MouseListener, WindowListener
	{

	/// <summary>
	/// String labels for the buttons.
	/// </summary>
	private const string __GraphArea_String = "Graph Area", __GraphSeepage_String = "Graph Seepage", __BUTTON_ADD_AREA_CAPACITY = "Add line", __BUTTON_APPLY = "Apply", __BUTTON_CANCEL = "Cancel", __BUTTON_DEL_AREA_CAPACITY = "Delete line", __BUTTON_HELP = "Help", __BUTTON_CLOSE = "Close";

	/// <summary>
	/// Whether the gui data is editable or not.
	/// </summary>
	private bool __editable = false;

	/// <summary>
	/// GUI Buttons.
	/// </summary>
	private SimpleJButton __GraphArea_JButton, __GraphSeepage_JButton;
	private JButton __addAreaCap, __closeJButton, __deleteAreaCap, __helpJButton;

	/// <summary>
	/// Status bar textfields 
	/// </summary>
	private JTextField __messageJTextField, __statusJTextField;

	/// <summary>
	/// Worksheet in which area cap information will be displayed.
	/// </summary>
	private JWorksheet __worksheet;

	/// <summary>
	/// The dataset containing the data.
	/// </summary>
	private StateMod_DataSet __dataset;

	/// <summary>
	/// The current reservoir of which the data is being displayed.
	/// </summary>
	private StateMod_Reservoir __currentRes;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="dataset"> the dataset in which the data is contained. </param>
	/// <param name="res"> the reservoir object to display in the frame. </param>
	/// <param name="editable"> whether the gui data is editable or not </param>
	public StateMod_Reservoir_AreaCap_JFrame(StateMod_DataSet dataset, StateMod_Reservoir res, bool editable)
	{
		StateMod_GUIUtil.setTitle(this, dataset, res.getName() + " - Reservoir Content/Area/Seepage", null);
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
		string action = e.getActionCommand();

		if (action.Equals(__GraphArea_String))
		{
			graph(__GraphArea_String);
		}
		else if (action.Equals(__GraphSeepage_String))
		{
			graph(__GraphSeepage_String);
		}
		else if (action.Equals(__BUTTON_ADD_AREA_CAPACITY))
		{
			StateMod_ReservoirAreaCap anAreaCapNode = new StateMod_ReservoirAreaCap();
			anAreaCapNode._isClone = true;
			__worksheet.addRow(anAreaCapNode);
			__worksheet.scrollToLastRow();
			__worksheet.selectLastRow();
			__deleteAreaCap.setEnabled(true);
		}
		else if (action.Equals(__BUTTON_DEL_AREA_CAPACITY))
		{
			int row = __worksheet.getSelectedRow();
			if (row != -1)
			{
				int x = (new ResponseJDialog(this, "Delete Content/Area/Seepage line?", "Delete Content/Area/Seepage line?", ResponseJDialog.YES | ResponseJDialog.NO)).response();
				if (x == ResponseJDialog.NO)
				{
					return;
				}
				__worksheet.deleteRow(row);
				__deleteAreaCap.setEnabled(false);
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
	/// Check the GUI state.  In particular, indicate whether the graph buttons should
	/// be enabled.
	/// </summary>
	private void checkGUIState()
	{
		IList<StateMod_ReservoirAreaCap> rv = __currentRes.getAreaCaps();
		if ((rv != null) && (rv.Count > 0))
		{
			bool area_ok = true, seepage_ok = true;
			// Check for data all one value...
			int size = rv.Count;
			StateMod_ReservoirAreaCap ac = null;
			double value;
			// REVISIT SAM 2006-08-20
			// JFreeChart has a problem when the values are the same as
			// the previous values.  However, for now, increment the values
			// slightly when graphing rather than disabling the graph.
			// Go ahead and disable if all values are the same.
			double area0 = 0.0, seepage0 = 0.0;
			bool area_all_same = true, seepage_all_same = true;
			for (int i = 0; i < size; i++)
			{
				ac = (StateMod_ReservoirAreaCap)rv[i];
				value = ac.getSurarea();
				/*
				if ( value == value_prev ) {
					area_ok = false;
					break;
				}
				*/
				if (i == 0)
				{
					area0 = value;
				}
				else
				{
					if (value != area0)
					{
						area_all_same = false;
					}
				}
			}
			for (int i = 0; i < size; i++)
			{
				ac = (StateMod_ReservoirAreaCap)rv[i];
				value = ac.getSeepage();
				/*
				if ( value == value_prev ) {
					seepage_ok = false;
					break;
				}
				*/
				if (i == 0)
				{
					seepage0 = value;
				}
				else
				{
					if (value != seepage0)
					{
						seepage_all_same = false;
					}
				}
			}
			if (area_all_same)
			{
				area_ok = false;
			}
			if (seepage_all_same)
			{
				seepage_ok = false;
			}
			JGUIUtil.setEnabled(__GraphArea_JButton, area_ok);
			JGUIUtil.setEnabled(__GraphSeepage_JButton, seepage_ok);
		}
		else
		{
			JGUIUtil.setEnabled(__GraphArea_JButton, false);
			JGUIUtil.setEnabled(__GraphSeepage_JButton, false);
		}
		// Only enable the graph buttons if the new charting package is in the
		// path.  This will allow for some graceful transition to distribution
		// of the new software.
		if (!IOUtil.classCanBeLoaded("org.jfree.chart.ChartPanel"))
		{
			JGUIUtil.setEnabled(__GraphArea_JButton, false);
			JGUIUtil.setEnabled(__GraphSeepage_JButton, false);
		}
	}

	/// <summary>
	/// Checks the data to make sure that all the data are valid. </summary>
	/// <returns> 0 if the data are valid, 1 if errors exist and -1 if non-fatal errors
	/// exist. </returns>
	private int checkInput()
	{
		// all the checking can be handled by the worksheet, since all the
		// values are numeric
		return 0;
	}

	/// <summary>
	/// Saves the input back into the dataset. </summary>
	/// <returns> true if the data was saved successfuly.  False if not. </returns>
	private bool saveData()
	{
		string routine = "StateMod_Reservoir_AreaCap_JFrame.saveData";
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
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<StateMod_ReservoirAreaCap> wv = (java.util.List<StateMod_ReservoirAreaCap>)__worksheet.getAllData();
		IList<StateMod_ReservoirAreaCap> wv = (IList<StateMod_ReservoirAreaCap>)__worksheet.getAllData(); // w for worksheet
		IList<StateMod_ReservoirAreaCap> rv = __currentRes.getAreaCaps();

		needToSave = !(StateMod_ReservoirAreaCap.Equals(wv, rv));

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
		IList<StateMod_ReservoirAreaCap> clone = new List<StateMod_ReservoirAreaCap>();
		for (int i = 0; i < size; i++)
		{
			clone.Add((StateMod_ReservoirAreaCap)wv[i].clone());
		}

		__currentRes.setAreaCaps(clone);
		__dataset.setDirty(StateMod_DataSet.COMP_DIVERSION_STATIONS, true);
		return true;
	}

	/// <summary>
	/// Checks to see if the __deleteAreaCap button should be enabled or not.
	/// </summary>
	private void checkDeleteAreaCapButton()
	{
		int row = __worksheet.getSelectedRow();
		if (row == -1)
		{
			__deleteAreaCap.setEnabled(false);
		}
		else
		{
			__deleteAreaCap.setEnabled(true);
		}
	}

	/// <summary>
	/// Clean up before garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~StateMod_Reservoir_AreaCap_JFrame()
	{
		__currentRes = null;
		__worksheet = null;
		__addAreaCap = null;
		__deleteAreaCap = null;
		__helpJButton = null;
		__closeJButton = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Show a graph of data. </summary>
	/// <param name="choice"> Indicate data to graph, either __GraphArea_String or
	/// __GraphSeepage_String. </param>
	private void graph(string choice)
	{ // For now all the display setup is done in this one method.
		// REVISIT SAM 2006-02-28 This could be made more general.
		if (choice.Equals(__GraphArea_String))
		{
			new StateMod_Reservoir_AreaCap_Graph_JFrame(__dataset, __currentRes, "Area", false);
		}
		else if (choice.Equals(__GraphSeepage_String))
		{
			new StateMod_Reservoir_AreaCap_Graph_JFrame(__dataset, __currentRes, "Seepage", false);
		}
	}

	/// <summary>
	/// Responds to key pressed events; does nothing. </summary>
	/// <param name="e"> the KeyEvent that happened. </param>
	public virtual void keyPressed(KeyEvent e)
	{
	}

	/// <summary>
	/// Responds to key released events; checks if the __deleteAreaCap button should
	/// be enabled. </summary>
	/// <param name="e"> the KeyEvent that happened. </param>
	public virtual void keyReleased(KeyEvent e)
	{
		checkDeleteAreaCapButton();
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
	/// Responds to mouse released events; checks to see if the __deleteAreaCap button
	/// should be enabled or not. </summary>
	/// <param name="e"> the MouseEvent that happened. </param>
	public virtual void mouseReleased(MouseEvent e)
	{
		checkDeleteAreaCapButton();
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

		__addAreaCap = new JButton(__BUTTON_ADD_AREA_CAPACITY);
		__deleteAreaCap = new JButton(__BUTTON_DEL_AREA_CAPACITY);
		__deleteAreaCap.setEnabled(false);
		__helpJButton = new JButton(__BUTTON_HELP);
		__helpJButton.setEnabled(false);
		__closeJButton = new JButton(__BUTTON_CLOSE);
		JButton cancelJButton = new JButton(__BUTTON_CANCEL);
		JButton applyJButton = new JButton(__BUTTON_APPLY);

		GridBagLayout gb = new GridBagLayout();
		JPanel bigPanel = new JPanel();
		bigPanel.setLayout(gb);

		FlowLayout fl = new FlowLayout(FlowLayout.RIGHT);
		JPanel p0 = new JPanel();
		p0.setLayout(fl);
		p0.add(__GraphArea_JButton = new SimpleJButton(__GraphArea_String, __GraphArea_String, this));
		p0.add(__GraphSeepage_JButton = new SimpleJButton(__GraphSeepage_String, __GraphSeepage_String, this));

		GridLayout gl = new GridLayout(2, 2, 1, 1);
		JPanel info_panel = new JPanel();
		info_panel.setLayout(gl);

		//JPanel main_panel = new JPanel();
		//main_panel.setLayout(new BorderLayout());

		info_panel.add(new JLabel("Reservoir:"));
		info_panel.add(new JLabel(__currentRes.getID()));
		info_panel.add(new JLabel("Reservoir name:"));
		info_panel.add(new JLabel(__currentRes.getName()));

		JPanel p1 = new JPanel();
		p1.setLayout(fl);
		if (__editable)
		{
			p1.add(__addAreaCap);
			p1.add(__deleteAreaCap);
		}
		p1.add(applyJButton);
		p1.add(cancelJButton);
	//	p1.add(__helpJButton);
		p1.add(__closeJButton);

		PropList p = new PropList("StateMod_Reservoir__AreaCap_JFrame.JWorksheet");
		/*
		p.add("JWorksheet.CellFont=Courier");
		p.add("JWorksheet.CellStyle=Plain");
		p.add("JWorksheet.CellSize=11");
		p.add("JWorksheet.HeaderFont=Arial");
		p.add("JWorksheet.HeaderStyle=Plain");
		p.add("JWorksheet.HeaderSize=11");
		p.add("JWorksheet.HeaderBackground=LightGray");
		p.add("JWorksheet.RowColumnPresent=false");
		*/
		p.add("JWorksheet.ShowPopupMenu=true");
		p.add("JWorksheet.AllowCopy=true");
		p.add("JWorksheet.SelectionMode=SingleRowSelection");

		int[] widths = null;
		JScrollWorksheet jsw = null;
		try
		{
			IList<StateMod_ReservoirAreaCap> v = new List<StateMod_ReservoirAreaCap>();
			IList<StateMod_ReservoirAreaCap> v2 = __currentRes.getAreaCaps();
			for (int i = 0; i < v2.Count; i++)
			{
				v.Add((StateMod_ReservoirAreaCap)v2[i].clone());
			}
			StateMod_ReservoirAreaCap_TableModel tmr = new StateMod_ReservoirAreaCap_TableModel(v, __editable, true);
			StateMod_ReservoirAreaCap_CellRenderer crr = new StateMod_ReservoirAreaCap_CellRenderer(tmr);

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

		//main_panel.add(jsw, "Center");
		// Does not work well...
		//main_panel.add(p1, "South");

		// assemble parts 
		JGUIUtil.addComponent(bigPanel, info_panel, 0, 0, 1, 1, 0, 0, GridBagConstraints.HORIZONTAL, GridBagConstraints.NORTHWEST);
		JGUIUtil.addComponent(bigPanel, jsw, 0, 1, 10, 10, 1.0, 1.0, GridBagConstraints.BOTH, GridBagConstraints.SOUTH);
		JPanel button_panel = new JPanel();
		button_panel.setLayout(gb);
		JGUIUtil.addComponent(button_panel, p0, 0, 0, 10, 1, 0, 0, GridBagConstraints.HORIZONTAL, GridBagConstraints.SOUTHEAST);
		JGUIUtil.addComponent(button_panel, p1, 0, 1, 10, 1, 0, 0, GridBagConstraints.HORIZONTAL, GridBagConstraints.SOUTHEAST);
		JGUIUtil.addComponent(bigPanel, button_panel, 0, 11, 10, 1, 0, 0, GridBagConstraints.HORIZONTAL, GridBagConstraints.SOUTHEAST);
		__addAreaCap.addActionListener(this);
		__deleteAreaCap.addActionListener(this);
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
		checkGUIState();
		setSize(420, 400);
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