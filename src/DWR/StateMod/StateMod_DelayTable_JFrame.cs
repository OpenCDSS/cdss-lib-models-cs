using System;
using System.Collections.Generic;

// StateMod_DelayTable_JFrame - dialog to edit the delay information.

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
// StateMod_DelayTable_JFrame - dialog to edit the delay information.
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
// 
// 14 Mar 2000	CEN, RTi		Created class
// 01 Apr 2001	Steven A. Malers, RTi	Change GUI to JGUIUtil.  Add finalize().
//					Remove import *.
// 04 May 2001	SAM, RTi		Verify that TSView code is properly
//					configured for all views.
// 13 Aug 2001	SAM, RTi		Update to handle returns as percent or
//					decimal fraction.
// 2002-03-07	SAM, RTi		Update to select the first item in the
//					list when displayed - actually - don't
//					do this because it may be slow,
//					depending on what data are available.
//------------------------------------------------------------------------------
// 2003-06-09	J. Thomas Sapienza, RTi	Initial swing version from 
//					SMdelaysWindow
// 2003-06-17	JTS, RTi		Created first functional version.
// 2003-06-19	JTS, RTi		Finished first functional version, 
//					Javadoc'd.
// 2003-06-20	JTS, RTi		Constructor now takes a data set as
//					a parameter, instead of a data set
//					component.
// 2003-06-23	JTS, RTi		Implemented graphing of delay series.
// 2003-07-15	JTS, RTi		* Added status bar.
//					* Changed to use new dataset design.
// 2003-07-17	JTS, RTI		Change so that constructor takes a 
//					boolean that says whether the form's
//					data can be modified.
// 2003-07-23	JTS, RTi		Updated JWorksheet code following
//					JWorksheet revisions.
// 2003-08-03	SAM, RTi		* Changed isDirty() back to setDirty().
//					* Require the title parameter in the
//					  constructor.
//					* Add a constructor to select a delay
//					  table at creation.
// 2003-08-16	SAM, RTi		* Change the window type to
//					  WINDOW_DELAY_TABLE_MONTHLY and
//					  WINDOW_DELAY_TABLE_DAILY.  The window
//					  is used for both monthly and daily
//					  delay tables but but both windows can
//					  be open at the same time.
//					* Require a flag for the constructor
//					  indicating whether monthly or daily
//					  delay tables are being displayed.
// 2003-08-26	SAM, RTi		Enable StateMod_DataSet_WindowManager.
// 2003-08-27	JTS, RTi		Added selectID() to select an ID 
//					on the worksheet from outside the GUI.
// 2003-09-04	SAM, RTi		* Change so daily delay table results in
//					  a daily time series being created.
//					* Pass flag to table model constructor
//					  to indicate whether monthly or daily
//					  data.
// 2003-09-23	JTS, RTi		Uses new StateMod_GUIUtil code for
//					setting titles.
// 2003-10-14	JTS, RTi		Updated to use new data saving model.
// 2004-01-21	JTS, RTi		Updated to use JScrollWorksheet and
//					the new row headers.
// 2004-07-15	JTS, RTi		* For data changes, enabled the
//					  Apply and Cancel buttons through new
//					  methods in the data classes.
//					* Changed layout of buttons to be
//					  aligned in the lower-right.
// 2004-08-25	JTS, RTi		Based on the value of 'interv' in 
//					the control file, the header for
//					the return column says whether it is
//					a percent or a fraction.
// 2004-08-26	JTS, RTi		Implement Apply/Cancel/Close 
//					functionality.
// 2006-01-19	JTS, RTi		Made the dialog wider so that the 
//					Graph button is displayed.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
//------------------------------------------------------------------------------
// EndHeader

namespace DWR.StateMod
{


	using TSViewJFrame = RTi.GRTS.TSViewJFrame;
	using DayTS = RTi.TS.DayTS;
	using MonthTS = RTi.TS.MonthTS;
	using TS = RTi.TS.TS;
	using TSIdent = RTi.TS.TSIdent;
	using JGUIUtil = RTi.Util.GUI.JGUIUtil;
	using JScrollWorksheet = RTi.Util.GUI.JScrollWorksheet;
	using JWorksheet = RTi.Util.GUI.JWorksheet;
	using ResponseJDialog = RTi.Util.GUI.ResponseJDialog;
	using DataSetComponent = RTi.Util.IO.DataSetComponent;
	using PropList = RTi.Util.IO.PropList;
	using Message = RTi.Util.Message.Message;
	using DateTime = RTi.Util.Time.DateTime;
	using TimeInterval = RTi.Util.Time.TimeInterval;

	/// <summary>
	/// This class is a gui for displaying and editing DelayTable information.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class StateMod_DelayTable_JFrame extends javax.swing.JFrame implements java.awt.event.ActionListener, java.awt.event.KeyListener, java.awt.event.MouseListener, java.awt.event.WindowListener
	public class StateMod_DelayTable_JFrame : JFrame, ActionListener, KeyListener, MouseListener, WindowListener
	{

	/// <summary>
	/// Local reference to the window number as defined in StateMod_GUIUtil.
	/// </summary>
	public int __window_type = StateMod_DataSet_WindowManager.WINDOW_DELAY_TABLE_MONTHLY;

	/// <summary>
	/// Whether the data are editable or not.
	/// </summary>
	private bool __editable = false;

	/// <summary>
	/// Whether the data are for monthly (true) or daily (false) data.
	/// </summary>
	private bool __monthly_data = true;

	/// <summary>
	/// Button labels.
	/// </summary>
	private readonly string __BUTTON_ADD_RETURN = "Add return", __BUTTON_APPLY = "Apply", __BUTTON_CANCEL = "Cancel", __BUTTON_CLOSE = "Close", __BUTTON_DELETE_RETURN = "Delete return", __BUTTON_HELP = "Help";

	/// <summary>
	/// The kind of component being displayed.
	/// </summary>
	private int __componentType = -1;

	/// <summary>
	/// Currently-selected delay table index.
	/// </summary>
	private int __currentIndex = -1;

	/// <summary>
	/// Form buttons.
	/// </summary>
	private JButton __addReturn, __closeJButton, __deleteReturn, __findNextDelay, __graphDelayJButton, __helpJButton;

	/// <summary>
	/// Textfield for searching the delay list for a certain ID.
	/// </summary>
	private JTextField __searchID;

	/// <summary>
	/// Status bar textfields 
	/// </summary>
	private JTextField __messageJTextField, __statusJTextField;

	/// <summary>
	/// Worksheet for displaying the delay table IDs.
	/// </summary>
	private JWorksheet __worksheetL;
	/// <summary>
	/// Worksheet for displaying the delay table data.
	/// </summary>
	private JWorksheet __worksheetR;

	/// <summary>
	/// The StateMod_DataSet that contains the statemod data.
	/// </summary>
	private StateMod_DataSet __dataset;

	/// <summary>
	/// Data set window manager.
	/// </summary>
	private StateMod_DataSet_WindowManager __dataset_wm;

	/// <summary>
	/// The DataSetComponent that contains the delay data.
	/// </summary>
	private DataSetComponent __delayComponent;

	/// <summary>
	/// The list of delay data in the DataSetComponent.
	/// </summary>
	private IList<StateMod_DelayTable> __delaysVector;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="dataset"> the dataset containing the data to display </param>
	/// <param name="dataset_wm"> the dataset window manager or null if the data set windows
	/// are not being managed. </param>
	/// <param name="monthly_data"> If true, display the monthly delay tables.  If false,
	/// display the daily delay tables. </param>
	/// <param name="editable"> whether the data is editable or not </param>
	public StateMod_DelayTable_JFrame(StateMod_DataSet dataset, StateMod_DataSet_WindowManager dataset_wm, bool monthly_data, bool editable)
	{
		__dataset = dataset;
		__dataset_wm = dataset_wm;
		__monthly_data = monthly_data;
		JGUIUtil.setIcon(this, JGUIUtil.getIconImage());
		string interval = " (Monthly)";
		if (__monthly_data)
		{
			__window_type = StateMod_DataSet_WindowManager.WINDOW_DELAY_TABLE_MONTHLY;
			__delayComponent = __dataset.getComponentForComponentType(StateMod_DataSet.COMP_DELAY_TABLES_MONTHLY);
			__componentType = StateMod_DataSet.COMP_DELAY_TABLES_MONTHLY;
		}
		else
		{
			__window_type = StateMod_DataSet_WindowManager.WINDOW_DELAY_TABLE_DAILY;
			__delayComponent = __dataset.getComponentForComponentType(StateMod_DataSet.COMP_DELAY_TABLES_DAILY);
			__componentType = StateMod_DataSet.COMP_DELAY_TABLES_DAILY;
			interval = " (Daily)";
		}
		StateMod_GUIUtil.setTitle(this, dataset, "Delay Tables" + interval, null);
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<StateMod_DelayTable> delaysList = (java.util.List<StateMod_DelayTable>)__delayComponent.getData();
		IList<StateMod_DelayTable> delaysList = (IList<StateMod_DelayTable>)__delayComponent.getData();
		__delaysVector = delaysList;

		int size = __delaysVector.Count;
		StateMod_DelayTable dt = null;
		for (int i = 0; i < size; i++)
		{
			dt = __delaysVector[i];
			dt.createBackup();
		}

		__editable = editable;
		setupGUI(0);
	}

	/// <summary>
	/// Constructor. </summary>
	/// <param name="dataset"> the dataset containing the data to display </param>
	/// <param name="dataset_wm"> the dataset window manager or null if the data set windows
	/// are not being managed. </param>
	/// <param name="delayTable"> the delay table to display. </param>
	/// <param name="monthly_data"> If true, display the monthly delay tables.  If false,
	/// display the daily delay tables. </param>
	/// <param name="editable"> whether the data is editable or not </param>
	public StateMod_DelayTable_JFrame(StateMod_DataSet dataset, StateMod_DataSet_WindowManager dataset_wm, StateMod_DelayTable delayTable, bool monthly_data, bool editable)
	{
		__dataset = dataset;
		__dataset_wm = dataset_wm;
		__monthly_data = monthly_data;
		JGUIUtil.setIcon(this, JGUIUtil.getIconImage());
		string interval = " (Monthly)";
		if (__monthly_data)
		{
			__window_type = StateMod_DataSet_WindowManager.WINDOW_DELAY_TABLE_MONTHLY;
			__delayComponent = __dataset.getComponentForComponentType(StateMod_DataSet.COMP_DELAY_TABLES_MONTHLY);
			__componentType = StateMod_DataSet.COMP_DELAY_TABLES_MONTHLY;
		}
		else
		{
			__window_type = StateMod_DataSet_WindowManager.WINDOW_DELAY_TABLE_DAILY;
			__delayComponent = __dataset.getComponentForComponentType(StateMod_DataSet.COMP_DELAY_TABLES_DAILY);
			__componentType = StateMod_DataSet.COMP_DELAY_TABLES_DAILY;
			interval = " (Daily)";
		}
		StateMod_GUIUtil.setTitle(this, dataset, "Delay Tables" + interval, null);

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<StateMod_DelayTable> delaysList = (java.util.List<StateMod_DelayTable>)__delayComponent.getData();
		IList<StateMod_DelayTable> delaysList = (IList<StateMod_DelayTable>)__delayComponent.getData();
		__delaysVector = delaysList;

		int size = __delaysVector.Count;
		StateMod_DelayTable dt = null;
		for (int i = 0; i < size; i++)
		{
			dt = __delaysVector[i];
			dt.createBackup();
		}

		string id = delayTable.getID();
		int index = StateMod_Util.IndexOf(__delaysVector, id);

		__editable = editable;
		setupGUI(index);
	}

	/// <summary>
	/// Constructor. </summary>
	/// <param name="delaysVector"> the list of delays to show </param>
	/// <param name="delayTable"> the delay table to display. </param>
	/// <param name="monthly_data"> If true, display the monthly delay tables.  If false,
	/// display the daily delay tables. </param>
	/// <param name="editable"> whether the data is editable or not </param>
	public StateMod_DelayTable_JFrame(IList<StateMod_DelayTable> delaysVector, StateMod_DelayTable delayTable, bool monthly_data, bool editable)
	{
		__monthly_data = monthly_data;
		JGUIUtil.setIcon(this, JGUIUtil.getIconImage());
		string interval = " (Monthly)";
		if (__monthly_data)
		{
			__window_type = StateMod_DataSet_WindowManager.WINDOW_DELAY_TABLE_MONTHLY;
			__componentType = StateMod_DataSet.COMP_DELAY_TABLES_MONTHLY;
		}
		else
		{
			__window_type = StateMod_DataSet_WindowManager.WINDOW_DELAY_TABLE_DAILY;
			__componentType = StateMod_DataSet.COMP_DELAY_TABLES_DAILY;
			interval = " (Daily)";
		}
		StateMod_GUIUtil.setTitle(this, null, "Delay Tables" + interval, null);

		__delaysVector = delaysVector;

		int size = __delaysVector.Count;
		StateMod_DelayTable dt = null;
		for (int i = 0; i < size; i++)
		{
			dt = __delaysVector[i];
			dt.createBackup();
		}

		string id = delayTable.getID();
		int index = StateMod_Util.IndexOf(__delaysVector, id);

		__editable = editable;
		setupGUI(index);
	}

	/// <summary>
	/// Constructor.  This version is used, for example, to display delay tables from
	/// StateDMI when used with a StateCU_DataSet. </summary>
	/// <param name="delaysVector"> the Vector of delays to show </param>
	/// <param name="monthly_data"> If true, display the monthly delay tables.  If false,
	/// display the daily delay tables. </param>
	/// <param name="editable"> whether the data is editable or not </param>
	public StateMod_DelayTable_JFrame(IList<StateMod_DelayTable> delaysVector, bool monthly_data, bool editable)
	{
		__monthly_data = monthly_data;
		string interval = " (Monthly)";
		JGUIUtil.setIcon(this, JGUIUtil.getIconImage());
		if (__monthly_data)
		{
			__window_type = StateMod_DataSet_WindowManager.WINDOW_DELAY_TABLE_MONTHLY;
			__componentType = StateMod_DataSet.COMP_DELAY_TABLES_MONTHLY;
		}
		else
		{
			__window_type = StateMod_DataSet_WindowManager.WINDOW_DELAY_TABLE_DAILY;
			__componentType = StateMod_DataSet.COMP_DELAY_TABLES_DAILY;
			interval = " (Daily)";
		}
		StateMod_GUIUtil.setTitle(this, null, "Delay Tables" + interval, null);

		__delaysVector = delaysVector;

		int size = __delaysVector.Count;
		StateMod_DelayTable dt = null;
		for (int i = 0; i < size; i++)
		{
			dt = __delaysVector[i];
			dt.createBackup();
		}

		__editable = editable;

		setupGUI(0);
	}

	/// <summary>
	/// Constructor. </summary>
	/// <param name="delay"> the Delay to show </param>
	/// <param name="monthly_data"> If true, display the monthly delay tables.  If false,
	/// display the daily delay tables. </param>
	/// <param name="editable"> whether the gui data is editable or not </param>
	public StateMod_DelayTable_JFrame(StateMod_DelayTable delay, bool monthly_data, bool editable)
	{
		__monthly_data = monthly_data;
		JGUIUtil.setIcon(this, JGUIUtil.getIconImage());
		string interval = " (Monthly)";
		if (__monthly_data)
		{
			__window_type = StateMod_DataSet_WindowManager.WINDOW_DELAY_TABLE_MONTHLY;
			__componentType = StateMod_DataSet.COMP_DELAY_TABLES_MONTHLY;
		}
		else
		{
			__window_type = StateMod_DataSet_WindowManager.WINDOW_DELAY_TABLE_DAILY;
			__componentType = StateMod_DataSet.COMP_DELAY_TABLES_DAILY;
			interval = " (Daily)";
		}
		StateMod_GUIUtil.setTitle(this, null, "Delay Table" + interval, null);
		__delaysVector = new List<StateMod_DelayTable>();
		__delaysVector.Add(delay);

		int size = __delaysVector.Count;
		StateMod_DelayTable dt = null;
		for (int i = 0; i < size; i++)
		{
			dt = __delaysVector[i];
			dt.createBackup();
		}

		__editable = editable;

		setupGUI(0);
	}

	/// <summary>
	/// Responds to action performed events. </summary>
	/// <param name="e"> the ActionEvent that happened. </param>
	public virtual void actionPerformed(ActionEvent e)
	{
		string routine = "StateMod_DelayTable_JFrame"
			+ ".actionPerformed";
		if (Message.isDebugOn)
		{
			Message.printDebug(1, routine, "In actionPerformed: " + e.getActionCommand());
		}

		string action = e.getActionCommand();

		if (action.Equals(__BUTTON_HELP))
		{
			// REVISIT HELP (JTS - 2003-06-09)
		}
		else if (action.Equals(__BUTTON_CLOSE))
		{
			closeWindow();
		}
		else if (action.Equals(__BUTTON_APPLY))
		{
			saveDelayTable();
			int size = __delaysVector.Count;
			StateMod_DelayTable dt = null;
			bool changed = false;
			for (int i = 0; i < size; i++)
			{
				dt = (StateMod_DelayTable)__delaysVector[i];
				if (!changed && dt.changed())
				{
					changed = true;
				}
				dt.createBackup();
			}
			if (changed)
			{
				__dataset.setDirty(__componentType, true);
			}
		}
		else if (action.Equals(__BUTTON_CANCEL))
		{
			__worksheetR.deselectAll();
			int size = __delaysVector.Count;
			StateMod_DelayTable dt = null;
			bool changed = false;
			for (int i = 0; i < size; i++)
			{
				dt = (StateMod_DelayTable)__delaysVector[i];
				if (!changed && dt.changed())
				{
					changed = true;
				}
				dt.restoreOriginal();
			}
			if (__dataset_wm != null)
			{
				__dataset_wm.closeWindow(__window_type);
			}
			else
			{
				JGUIUtil.close(this);
			}
		}
		else if (action.Equals(__BUTTON_ADD_RETURN))
		{
			int row = __worksheetR.getSelectedRow();

			int total_num_rows = __worksheetR.getRowCount() - 1;

			if (row == -1)
			{
				row = total_num_rows;
			}

			if (row != -1)
			{
				if (row == total_num_rows)
				{
					int x = new ResponseJDialog(this, "Insert row", "Do you wish to add a new row above " + "the last row?\n" + "uniquetempvar.response();
					if (x == ResponseJDialog.CANCEL)
					{
						return;
					}
					else if (x == ResponseJDialog.NO)
					{
						row += 1;
					}
				}
				__worksheetR.insertRowAt(new double?(0), row);
				__worksheetR.scrollToRow(row);
				__worksheetR.selectRow(row);
			}
			else
			{
				__worksheetR.addRow(new double?(0));
				__worksheetR.scrollToRow(0);
				__worksheetR.selectRow(0);
			}
			__deleteReturn.setEnabled(true);
		}
		else if (action.Equals(__BUTTON_DELETE_RETURN))
		{
			int row = __worksheetR.getSelectedRow();
			if (row != -1)
			{
				int x = (new ResponseJDialog(this, "Delete Return", "Delete return?", ResponseJDialog.YES | ResponseJDialog.NO)).response();
				if (x == ResponseJDialog.NO)
				{
					return;
				}
				//StateMod_DelayTable dt = (StateMod_DelayTable)
					//__worksheetL.getRowData(
					//__worksheetL.getSelectedRow());
				__worksheetR.deleteRow(row);
				__deleteReturn.setEnabled(false);
			}
			else
			{
				Message.printWarning(1, routine, "Must select desired right to delete.");
			}
		}
		else if (e.getSource() == __findNextDelay)
		{
			searchLeftWorksheet(__worksheetL.getSelectedRow() + 1);
		}
		else if (e.getSource() == __searchID)
		{
			searchLeftWorksheet();
		}
		else
		{
			if (__worksheetL.getSelectedRow() == -1)
			{
				new ResponseJDialog(this, "You must first select a delay from the list.", ResponseJDialog.OK);
				return;
			}
			else if (e.getSource() == __graphDelayJButton)
			{
				try
				{
					__worksheetR.deselectAll();

					int index = __worksheetL.getSelectedRow();
					if (index == -1)
					{
						return;
					}

					StateMod_DelayTable currentDelay = ((StateMod_DelayTable) __delaysVector[index]);

					int j;

					DateTime date;

					TSIdent tsident = new TSIdent();
					tsident.setLocation(currentDelay.getID());
					tsident.setSource("StateMod");
					if (__monthly_data)
					{
						tsident.setInterval("Month");
					}
					else
					{
						tsident.setInterval("Day");
					}
					tsident.setType("Delay");

					DateTime date1 = null;
					DateTime date2 = null;
					int interval_base;
					if (__monthly_data)
					{
						date1 = new DateTime(DateTime.PRECISION_MONTH);
						date2 = new DateTime(DateTime.PRECISION_MONTH);
						interval_base = TimeInterval.MONTH;
					}
					else
					{
						date1 = new DateTime(DateTime.PRECISION_DAY);
						date2 = new DateTime(DateTime.PRECISION_DAY);
						interval_base = TimeInterval.DAY;
					}
					date1.setMonth(1);
					date1.setYear(1);
					date2.setMonth(1);
					date2.setYear(1);
					date2.addInterval(interval_base, (currentDelay.getNdly() - 1));

					TS ts = null;
					if (__monthly_data)
					{
						ts = new MonthTS();
					}
					else
					{
						ts = new DayTS();
					}
					ts.setDate1(date1);
					ts.setDate2(date2);
					ts.setIdentifier(tsident);
					if (__monthly_data)
					{
						ts.setDescription(ts.getLocation() + " Monthly Delay Table");
					}
					else
					{
						ts.setDescription(ts.getLocation() + " Daily Delay Table");
					}
					ts.setDataType("Delay");
					ts.setDataUnits(currentDelay.getUnits());
					ts.allocateDataSpace();

					double max = 0.0;
					for (date = new DateTime(date1), j = 0; date.lessThanOrEqualTo(date2); date.addInterval(interval_base, 1), j++)
					{
						ts.setDataValue(date, currentDelay.getRet_val(j));
						if (currentDelay.getRet_val(j) > max)
						{
							max = currentDelay.getRet_val(j);
						}
					}
					IList<TS> tslist = new List<TS>();
					tslist.Add(ts);

					PropList graphProps = new PropList("TSView");
					// If dealing with small values, use a high
					// of precision...
					if (max < 1.0)
					{
						graphProps.set("YAxisPrecision","6");
						graphProps.set("OutputPrecision","6");
					}
					else
					{
						graphProps.set("YAxisPrecision","3");
						graphProps.set("OutputPrecision","3");
					}
					graphProps.set("InitialView", "Graph");
					graphProps.set("TotalWidth", "600");
					graphProps.set("TotalHeight", "400");
					if (__monthly_data)
					{
						graphProps.set("Title", ts.getLocation() + " Monthly Delay Table");
					}
					else
					{
						graphProps.set("Title", ts.getLocation() + " Daily Delay Table");
					}
					graphProps.set("DisplayFont", "Courier");
					graphProps.set("DisplaySize", "11");
					graphProps.set("PrintFont", "Courier");
					graphProps.set("PrintSize", "7");
					graphProps.set("PageLength", "100");
					new TSViewJFrame(tslist, graphProps);
				}
				catch (Exception)
				{
					Message.printWarning(1, routine, "Unable to graph delay. ");
				}
			}
		}
	}

	private int checkInput()
	{
		if (!__worksheetR.stopEditing())
		{
			return 1;
		}
		return 0;
		/*
		int size = __worksheetR.getRowCount();
		String warning = "";
	
		int fatalCount = 0;
		for (int i = 0; i < size; i++) {
			if (!StringUtil.isDouble()) {
				warning += "\nAdministration number (" + adminNum 
					+ ") is not a number.";
				fatalCount++;
			}
		*/
	}

	/// <summary>
	/// Closes the window.
	/// </summary>
	private void closeWindow()
	{
		if (checkInput() <= 0)
		{
			saveDelayTable();
			__worksheetR.deselectAll();
		}
		else
		{
			return;
		}

		int size = __delaysVector.Count;
		StateMod_DelayTable dt = null;
		bool changed = false;
		for (int i = 0; i < size; i++)
		{
			dt = (StateMod_DelayTable)__delaysVector[i];
			if (!changed && dt.changed())
			{
				changed = true;
			}
			dt.acceptChanges();
		}
		if (changed)
		{
			__dataset.setDirty(__componentType, true);
		}
		if (__dataset_wm != null)
		{
			__dataset_wm.closeWindow(__window_type);
		}
		else
		{
			JGUIUtil.close(this);
		}
	}

	/// <summary>
	/// Clean up before garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~StateMod_DelayTable_JFrame()
	{
		__addReturn = null;
		__closeJButton = null;
		__deleteReturn = null;
		__findNextDelay = null;
		__graphDelayJButton = null;
		__helpJButton = null;
		__searchID = null;
		__worksheetL = null;
		__worksheetR = null;
		__dataset = null;
		__delayComponent = null;
		__delaysVector = null;
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
	/// Responds to key released events; calls 'processLeftTableSelection' with the 
	/// newly-selected index in the table. </summary>
	/// <param name="e"> the KeyEvent that happened. </param>
	public virtual void keyReleased(KeyEvent e)
	{
	/*
		if (e.getSource() == __worksheetL) {
			processLeftTableSelection(__worksheetL.getSelectedRow());
		}
		else {
			processRightTableSelection(__worksheetR.getSelectedRow());
		}
	*/
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
	/// Responds to mouse released events; calls 'processXXXTableSelection' with the 
	/// newly-selected index in the table. </summary>
	/// <param name="e"> the MouseEvent that happened. </param>
	public virtual void mouseReleased(MouseEvent e)
	{
		if (e.getSource() == __worksheetL)
		{
			processLeftTableSelection(__worksheetL.getSelectedRow(), true);
		}
		else
		{
			processRightTableSelection(__worksheetR.getSelectedRow());
		}
	}


	/// <summary>
	/// Processes a table selection (either via a mouse press or programmatically 
	/// from selectTableIndex() by getting the next selection's data out of the 
	/// data and displaying it on the form. </summary>
	/// <param name="index"> the index of the delay table info to display on the form. </param>
	private void processLeftTableSelection(int index, bool tryToSave)
	{
		if (index == -1)
		{
			return;
		}

		if (tryToSave && (checkInput() > 0))
		{
			if (__currentIndex > -1)
			{
				selectLeftTableIndex(__currentIndex, false, false);
			}
			return;
		}

		// if gotten this far, then the delay table can be saved back
		// (if it has changed)
		saveDelayTable();

		__currentIndex = index;

		StateMod_DelayTable dt = (StateMod_DelayTable) __worksheetL.getRowData(index);

		updateRightTable(dt);
	}

	/// <summary>
	/// Processes a selection on the right table and either selects or deselects
	/// the delete button, depending on whether a row was selected. </summary>
	/// <param name="index"> the row that was selected (-1 if none are selected). </param>
	private void processRightTableSelection(int index)
	{
		if (index == -1)
		{
			__deleteReturn.setEnabled(false);
		}
		else
		{
			__deleteReturn.setEnabled(true);
		}
	}

	/// <summary>
	/// Checks to see if any data has changed in the delay table and if so, writes
	/// the delay table back into the StateMod_DelayTable object.
	/// </summary>
	private void saveDelayTable()
	{
		int index = __currentIndex;
		if (index < 0)
		{
			return;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<System.Nullable<double>> v = (java.util.List<System.Nullable<double>>)__worksheetR.getAllData();
		IList<double?> v = (IList<double?>)__worksheetR.getAllData();
		/*
		StateMod_DelayTable dt = (StateMod_DelayTable)__worksheetL.getRowData(index);
	
		Vector dv = dt.getRet_val();
	
		boolean needToSave = false;
		if (wv.size() != dv.size()) {
			// definitely save
			needToSave = true;
		}
		else {
			int size = wv.size();
			Double d1;
			Double d2;
			for (int i = 0; i < size; i++) {
				d1 = (Double)wv.elementAt(i);
				d2 = (Double)dv.elementAt(i);
				if (d1.compareTo(d2) != 0) {
					needToSave = true;
					i = size + 1;
				}
			}
		}
	
		Message.printStatus(1, routine, "Saving? .........[" + needToSave +"]");
	
		if (!needToSave) {
			return;
		}
		Vector clone = new Vector();
		int size = wv.size();	
		for (int i = 0; i < size; i++) {
			clone.add(new Double(((Double)wv.elementAt(i)).doubleValue()));
		}
		dt.setRet_val(clone);
		*/
		__delaysVector[index].setRet_val(v);
	}

	/// <summary>
	/// Searches through the worksheet for a value, starting at the first row.
	/// If the value is found, the row is selected.
	/// </summary>
	public virtual void searchLeftWorksheet()
	{
		searchLeftWorksheet(0);
	}

	/// <summary>
	/// Searches through the worksheet for a value, starting at the given row.
	/// If the value is found, the row is selected. </summary>
	/// <param name="row"> the row to start searching from. </param>
	public virtual void searchLeftWorksheet(int row)
	{
		string searchFor = null;
		int col = 0;
		searchFor = __searchID.getText().Trim();

		int index = __worksheetL.find(searchFor, col, row, JWorksheet.FIND_EQUAL_TO | JWorksheet.FIND_CONTAINS | JWorksheet.FIND_CASE_INSENSITIVE | JWorksheet.FIND_WRAPAROUND);
		if (index != -1)
		{
			selectLeftTableIndex(index, true, true);
		}
	}

	/// <summary>
	/// Selects the desired ID in the table and displays the appropriate data
	/// in the remainder of the window. </summary>
	/// <param name="id"> the identifier to select in the list. </param>
	public virtual void selectID(string id)
	{
		int rows = __worksheetL.getRowCount();
		StateMod_DelayTable dt = null;
		for (int i = 0; i < rows; i++)
		{
			dt = (StateMod_DelayTable)__worksheetL.getRowData(i);
			if (dt.getID().Trim().Equals(id.Trim()))
			{
				selectLeftTableIndex(i, true, true);
				return;
			}
		}
	}

	/// <summary>
	/// Selects a row from the left table, scrolls to that row, and displays its
	/// delay table information in the right table. </summary>
	/// <param name="index"> the index of the row to select.  -1 if none were selected. </param>
	/// <param name="try_to_save"> Indicates whether the current contents should be saved before
	/// selecting the new row.  A value of false should be passed only at startup or
	/// when checkInput() has failed for a selection, in which case the display will
	/// revert to the failed data rather than a new selection. </param>
	/// <param name="process_selection"> If true, then the display will be updated based on the
	/// row that is selected - this should be the case most of the time.  If false, then
	/// the previous contents are retained - this should be the case if checkInput()
	/// detects an error, in which case we want the previous (and erroneous)
	/// user-supplied to be shown because they need to correct the data. </param>
	private void selectLeftTableIndex(int index, bool tryToSave, bool processSelection)
	{
		if (index == -1)
		{
			return;
		}
		if (index >= __worksheetL.getRowCount())
		{
			return;
		}

		__worksheetL.scrollToRow(index);
		__worksheetL.selectRow(index);
		processLeftTableSelection(index, tryToSave);
	}

	/// <summary>
	/// Sets up the GUI. </summary>
	/// <param name="index"> Data item to display. </param>
	private void setupGUI(int index)
	{
		string routine = "StateMod_DelayTable_JFrame";

		addWindowListener(this);

		// AWT portion
		JPanel p1 = new JPanel(); // selection list and grid
		JPanel p2 = new JPanel(); // search widgets
		JPanel pmain = new JPanel(); // everything but close and help buttons

		__searchID = new JTextField(10);
		__findNextDelay = new JButton("Find Next");

		PropList p = new PropList("StateMod_DelayTable_JFrame.JWorksheet");
		p.add("JWorksheet.ShowPopupMenu=true");
		p.add("JWorksheet.AllowCopy=true");
		p.add("JWorksheet.SelectionMode=SingleRowSelection");

		bool percent = true;
		if (__dataset.getInterv() == -100 || __dataset.getInterv() < -1)
		{
			percent = false;
		}

		int[] widthsR = null;
		JScrollWorksheet jswR = null;
		try
		{
			StateMod_DelayTable_TableModel tmd = new StateMod_DelayTable_TableModel(new List<double?>(), __monthly_data, __editable, percent);
			tmd.setSubDelays(new List<double?>());
			StateMod_DelayTable_CellRenderer crd = new StateMod_DelayTable_CellRenderer(tmd);

			jswR = new JScrollWorksheet(crd, tmd, p);
			__worksheetR = jswR.getJWorksheet();

			// remove the ID column
			__worksheetR.removeColumn(0);
			widthsR = crd.getColumnWidths();
		}
		catch (Exception e)
		{
			Message.printWarning(1, routine, "Error building worksheet.");
			Message.printWarning(2, routine, e);
			jswR = new JScrollWorksheet(0, 0, p);
			__worksheetR = jswR.getJWorksheet();
		}
		__worksheetR.setPreferredScrollableViewportSize(null);

		// Assume all have the same units so pass in the first one...
		__worksheetR.setHourglassJFrame(this);
		__worksheetR.addMouseListener(this);
		__worksheetR.addKeyListener(this);

		__graphDelayJButton = new JButton("Graph");

		if (__delaysVector.Count == 0)
		{
			__graphDelayJButton.setEnabled(false);
		}

		__helpJButton = new JButton(__BUTTON_HELP);
		__helpJButton.setEnabled(false);
		__closeJButton = new JButton(__BUTTON_CLOSE);
		__addReturn = new JButton(__BUTTON_ADD_RETURN);
		__deleteReturn = new JButton(__BUTTON_DELETE_RETURN);
		__deleteReturn.setEnabled(false);
		JButton cancelJButton = new JButton(__BUTTON_CANCEL);
		JButton applyJButton = new JButton(__BUTTON_APPLY);

		GridBagLayout gb = new GridBagLayout();
		p1.setLayout(gb);
		p2.setLayout(gb);
		pmain.setLayout(gb);

		int y;

		int[] widthsL = null;
		JScrollWorksheet jswL = null;
		try
		{
			StateMod_DelayTable_TableModel tmd = new StateMod_DelayTable_TableModel(__delaysVector, __monthly_data, __editable, percent);
			StateMod_DelayTable_CellRenderer crd = new StateMod_DelayTable_CellRenderer(tmd);

			jswL = new JScrollWorksheet(crd, tmd, p);
			__worksheetL = jswL.getJWorksheet();

			// remove all the columns but the ID column.
			__worksheetL.removeColumn(1);
			__worksheetL.removeColumn(2);
			widthsL = crd.getColumnWidths();
		}
		catch (Exception e)
		{
			Message.printWarning(1, routine, "Error building worksheet.");
			Message.printWarning(2, routine, e);
			jswL = new JScrollWorksheet(0, 0, p);
			__worksheetL = jswL.getJWorksheet();
		}
		__worksheetL.setPreferredScrollableViewportSize(null);
		__worksheetR.setPreferredScrollableViewportSize(null);
		__worksheetL.setHourglassJFrame(this);
		__worksheetL.addMouseListener(this);
		__worksheetL.addKeyListener(this);

		JGUIUtil.addComponent(pmain, jswL, 0, 0, 2, 12, .2, 1, 10, 10, 1, 10, GridBagConstraints.BOTH, GridBagConstraints.WEST);

		JGUIUtil.addComponent(pmain, jswR, 5, 1, 18, 24, 1, 1, 10, 10, 10, 10, GridBagConstraints.BOTH, GridBagConstraints.WEST);

		JPanel bottomJPanel = new JPanel();
		bottomJPanel.setLayout(gb);
		__messageJTextField = new JTextField();
		__messageJTextField.setEditable(false);
		JGUIUtil.addComponent(bottomJPanel, __messageJTextField, 0, 1, 7, 1, 1.0, 0.0, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);
		__statusJTextField = new JTextField(5);
		__statusJTextField.setEditable(false);
		JGUIUtil.addComponent(bottomJPanel, __statusJTextField, 7, 1, 1, 1, 0.0, 0.0, GridBagConstraints.NONE, GridBagConstraints.WEST);

		// close and help buttons
		JPanel pfinal = new JPanel();
		FlowLayout fl = new FlowLayout(FlowLayout.RIGHT);
		pfinal.setLayout(fl);
		if (__editable)
		{
			pfinal.add(__addReturn);
			pfinal.add(__deleteReturn);
		}
		pfinal.add(applyJButton);
		pfinal.add(cancelJButton);
		pfinal.add(__closeJButton);
		pfinal.add(__graphDelayJButton);
	//	pfinal.add(__helpJButton);

		JGUIUtil.addComponent(bottomJPanel, pfinal, 0, 0, 8, 1, 1, 1, GridBagConstraints.HORIZONTAL, GridBagConstraints.CENTER);

		__helpJButton.addActionListener(this);
		__closeJButton.addActionListener(this);
		__graphDelayJButton.addActionListener(this);
		__addReturn.addActionListener(this);
		__deleteReturn.addActionListener(this);
		cancelJButton.addActionListener(this);
		applyJButton.addActionListener(this);

		// add search areas
		y = 0;
		JPanel searchPanel = new JPanel();
		searchPanel.setLayout(gb);
		searchPanel.setBorder(BorderFactory.createTitledBorder("Search above list for:     "));
		JGUIUtil.addComponent(searchPanel, new JLabel("ID"), 0, y, 1, 1, 0, 0, 5, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(searchPanel, __searchID, 1, y, 1, 1, 1, 1, 0, 0, 0, 0, GridBagConstraints.HORIZONTAL, GridBagConstraints.EAST);
		__searchID.addActionListener(this);
		y++;
		JGUIUtil.addComponent(searchPanel, __findNextDelay, 0, y, 4, 1, 0, 0, 10, 0, 0, 0, GridBagConstraints.NONE, GridBagConstraints.CENTER);
		__findNextDelay.addActionListener(this);
		JGUIUtil.addComponent(pmain, searchPanel, 0, GridBagConstraints.RELATIVE, 1, 1, 0, 0, 5, 10, 20, 10, GridBagConstraints.NONE, GridBagConstraints.SOUTHWEST);

		getContentPane().add("Center", pmain);
		getContentPane().add("South", bottomJPanel);

		if (__dataset_wm != null)
		{
			__dataset_wm.setWindowOpen(__window_type, this);
		}
		pack();
		setSize(530, 400);
		JGUIUtil.center(this);
		setVisible(true);

		if (widthsR != null)
		{
			__worksheetR.setColumnWidths(widthsR);
		}
		if (widthsL != null)
		{
			__worksheetL.setColumnWidths(widthsL);
		}

		selectLeftTableIndex(index, false, true);
	}

	/// <summary>
	/// Updates the right table based on a new selection from the left table, 
	/// or if rows were added or deleted to the right table. </summary>
	/// <param name="dt"> the StateMod_DelayTable object selected in the left table. </param>
	private void updateRightTable(StateMod_DelayTable dt)
	{
		IList<double?> v = dt.getRet_val();
		IList<double?> v2 = new List<double?>();
		for (int i = 0; i < v.Count; i++)
		{
			v2.Add(new double?(v[i].Value));
		}

		((StateMod_DelayTable_TableModel)__worksheetR.getModel()).setSubDelays(v2);
		((StateMod_DelayTable_TableModel)__worksheetR.getModel()).fireTableDataChanged();
		((StateMod_DelayTable_TableModel)__worksheetR.getModel()).fireTableDataChanged();
		__deleteReturn.setEnabled(false);
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
	/// Responds to window closing events -- closes the window reference. </summary>
	/// <param name="e"> the WindowEvent that happened. </param>
	public virtual void windowClosing(WindowEvent e)
	{
		closeWindow();
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