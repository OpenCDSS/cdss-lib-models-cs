using System;

// StateMod_Plan_TableModel - Table model for displaying data in the plan tables.

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

// ----------------------------------------------------------------------------
// StateMod_Plan_TableModel - Table model for displaying data in the
//	plan tables.
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
// 2006-08-22	Steven A. Malers, RTi	Initial version, based on the
//					StateMod_Diversion_TableModel.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace DWR.StateMod
{

	using JWorksheet_AbstractRowTableModel = RTi.Util.GUI.JWorksheet_AbstractRowTableModel;

	/// <summary>
	/// This table model display data in plan tables.
	/// </summary>
	public class StateMod_Plan_TableModel : JWorksheet_AbstractRowTableModel
	{

	/// <summary>
	/// Number of columns in the table model (this includes all data - other code
	/// can hide columns if necessary).
	/// </summary>
	private int __COLUMNS = 10;

	/// <summary>
	/// References to columns.
	/// </summary>
	public const int COL_ID = 0, COL_NAME = 1, COL_RIVER_NODE_ID = 2, COL_ON_OFF = 3, COL_TYPE = 4, COL_EFFICIENCY = 5, COL_RETURN_FLOW_TABLE = 6, COL_FAILURE_SWITCH = 7, COL_INITIAL_STORAGE = 8, COL_SOURCE = 9;

	/// <summary>
	/// Whether the table data can be edited or not.
	/// </summary>
	private bool __editable = true;

	/// <summary>
	/// Constructor.  This builds the Model for displaying the plan station data. </summary>
	/// <param name="data"> the data that will be displayed in the table. </param>
	/// <param name="editable"> whether the data can be edited or not </param>
	/// <exception cref="Exception"> if an invalid data or dmi was passed in. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public StateMod_Plan_TableModel(java.util.List data, boolean editable) throws Exception
	public StateMod_Plan_TableModel(System.Collections.IList data, bool editable) : this(null, data, editable, false)
	{
	}

	/// <summary>
	/// Constructor.  This builds the Model for displaying the plan station data. </summary>
	/// <param name="data"> the data that will be displayed in the table. </param>
	/// <param name="editable"> whether the data can be edited or not </param>
	/// <param name="compactForm"> if true, then the compact form of the table model will be
	/// used.  In the compact form, only the name and ID are shown.  If false, all fields will be shown. </param>
	/// <exception cref="Exception"> if an invalid data or dmi was passed in. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public StateMod_Plan_TableModel(java.util.List data, boolean editable, boolean compactForm) throws Exception
	public StateMod_Plan_TableModel(System.Collections.IList data, bool editable, bool compactForm) : this(null, data, editable, compactForm)
	{
	}

	/// <summary>
	/// Constructor.  This builds the Model for displaying the plan data. </summary>
	/// <param name="dataset"> the dataset for the data being displayed.  Only necessary for return flow tables. </param>
	/// <param name="data"> the data that will be displayed in the table. </param>
	/// <param name="editable"> whether the data can be edited or not </param>
	/// <param name="compactForm"> if true, then the compact form of the table model will be
	/// used.  In the compact form, only the name and ID are shown.  If false, all fields will be shown. </param>
	/// <exception cref="Exception"> if an invalid data or dmi was passed in. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public StateMod_Plan_TableModel(StateMod_DataSet dataset, java.util.List data, boolean editable, boolean compactForm) throws Exception
	public StateMod_Plan_TableModel(StateMod_DataSet dataset, System.Collections.IList data, bool editable, bool compactForm)
	{
		if (data == null)
		{
			throw new Exception("Invalid data Vector passed to StateMod_Plan_TableModel constructor.");
		}
		_rows = data.Count;
		_data = data;

		__editable = editable;

		if (compactForm)
		{
			__COLUMNS = 2;
		}
	}

	/// <summary>
	/// Returns the class of the data stored in a given column. </summary>
	/// <param name="columnIndex"> the column for which to return the data class. </param>
	public virtual Type getColumnClass(int columnIndex)
	{
		switch (columnIndex)
		{
			case COL_ID:
				return typeof(string);
			case COL_NAME:
				return typeof(string);
			case COL_RIVER_NODE_ID:
				return typeof(string);
			case COL_ON_OFF:
				return typeof(Integer);
			case COL_TYPE:
				return typeof(Integer);
			case COL_EFFICIENCY:
				return typeof(Double);
			case COL_RETURN_FLOW_TABLE:
				return typeof(Integer);
			case COL_FAILURE_SWITCH:
				return typeof(Integer);
			case COL_INITIAL_STORAGE:
				return typeof(Double);
			case COL_SOURCE:
				return typeof(string);
			default:
				return typeof(string);
		}
	}

	/// <summary>
	/// Returns the number of columns of data. </summary>
	/// <returns> the number of columns of data. </returns>
	public virtual int getColumnCount()
	{
		return __COLUMNS;
	}

	/// <summary>
	/// Returns the name of the column at the given position. </summary>
	/// <returns> the name of the column at the given position. </returns>
	public virtual string getColumnName(int columnIndex)
	{
		switch (columnIndex)
		{
			case COL_ID:
				return "ID";
			case COL_NAME:
				return "NAME";
			case COL_RIVER_NODE_ID:
				return "RIVER\nNODE ID";
			case COL_ON_OFF:
				return "ON/OFF\nSWITCH";
			case COL_TYPE:
				return "PLAN\nTYPE";
			case COL_EFFICIENCY:
				return "EFFICIENCY\n(%)";
			case COL_RETURN_FLOW_TABLE:
				return "RETURN\nFLOW TABLE";
			case COL_FAILURE_SWITCH:
				return "FAILURE\nSWITCH";
			case COL_INITIAL_STORAGE:
				return "INITIAL\nSTORAGE";
			case COL_SOURCE:
				return "SOURCE\nID";
			default:
				return " ";
		}
	}

	/// <summary>
	/// Returns the format that the specified column should be displayed in when
	/// the table is being displayed in the given table format. </summary>
	/// <param name="column"> column for which to return the format. </param>
	/// <returns> the format (as used by StringUtil.formatString() in which to display the
	/// column. </returns>
	public virtual string getFormat(int column)
	{
		switch (column)
		{
			case COL_ID:
				return "%-12s";
			case COL_NAME:
				return "%-40s";
			case COL_RIVER_NODE_ID:
				return "%-12s";
			case COL_ON_OFF:
				return "%d";
			case COL_TYPE:
				return "%d";
			case COL_EFFICIENCY:
				return "%.1f";
			case COL_RETURN_FLOW_TABLE:
				return "%d";
			case COL_FAILURE_SWITCH:
				return "%d";
			case COL_INITIAL_STORAGE:
				return "%.0f";
			case COL_SOURCE:
				return "%-s";
			default:
				return "%-s";
		}
	}

	// REVISIT (SAM - 2005-01-20)
	// we might need to display flag values as "1 - XXX" to be readable.  Let's 
	// wait on some user feedback.  If editable, this may mean that choices are 
	// shown.

	/// <summary>
	/// Returns the number of rows of data in the table. </summary>
	/// <returns> the number of rows of data in the table. </returns>
	public virtual int getRowCount()
	{
		return _rows;
	}

	/// <summary>
	/// Returns the data that should be placed in the JTable at the given row and column. </summary>
	/// <param name="row"> the row for which to return data. </param>
	/// <param name="col"> the column for which to return data. </param>
	/// <returns> the data that should be placed in the JTable at the given row and col. </returns>
	public virtual object getValueAt(int row, int col)
	{
		if (_sortOrder != null)
		{
			row = _sortOrder[row];
		}

		StateMod_Plan smp = (StateMod_Plan)_data.get(row);
		switch (col)
		{
			case COL_ID:
				return smp.getID();
			case COL_NAME:
				return smp.getName();
			case COL_RIVER_NODE_ID:
				return smp.getCgoto();
			case COL_ON_OFF:
				return new int?(smp.getSwitch());
			case COL_TYPE:
				return new int?(smp.getIPlnTyp());
			case COL_EFFICIENCY:
				return new int?(smp.getPeffFlag());
			case COL_RETURN_FLOW_TABLE:
						return new int?(smp.getIPrf());
			case COL_FAILURE_SWITCH:
				return new int?(smp.getIPfail());
			case COL_INITIAL_STORAGE:
						return new double?(smp.getPsto1());
			case COL_SOURCE:
				return smp.getPsource();
			default:
				return "";
		}
	}

	/// <summary>
	/// Returns an array containing the widths (in number of characters) that the 
	/// fields in the table should be sized to. </summary>
	/// <returns> an integer array containing the widths for each field. </returns>
	public virtual int[] getColumnWidths()
	{
		int[] widths = new int[__COLUMNS];
		for (int i = 0; i < __COLUMNS; i++)
		{
			widths[i] = 0;
		}
		widths[COL_ID] = 8;
		widths[COL_NAME] = 18;

		if (__COLUMNS == 2)
		{
			return widths;
		}

		widths[COL_RIVER_NODE_ID] = 8;
		widths[COL_ON_OFF] = 5;
		widths[COL_TYPE] = 8;
		widths[COL_EFFICIENCY] = 8;
		widths[COL_RETURN_FLOW_TABLE] = 7;
		widths[COL_FAILURE_SWITCH] = 7;
		widths[COL_INITIAL_STORAGE] = 8;
		widths[COL_SOURCE] = 8;
		return widths;
	}

	/// <summary>
	/// Returns whether the cell is editable or not.  Currently no cells are editable. </summary>
	/// <param name="rowIndex"> unused. </param>
	/// <param name="columnIndex"> the index of the column to check whether it is editable
	/// or not. </param>
	/// <returns> whether the cell is editable or not. </returns>
	public virtual bool isCellEditable(int rowIndex, int columnIndex)
	{
		if (!__editable)
		{
			return false;
		}
		return true;
	}

	/// <summary>
	/// Inserts the specified value into the table at the given position. </summary>
	/// <param name="value"> the object to store in the table cell. </param>
	/// <param name="row"> the row of the cell in which to place the object. </param>
	/// <param name="col"> the column of the cell in which to place the object. </param>
	public virtual void setValueAt(object value, int row, int col)
	{
		if (_sortOrder != null)
		{
			row = _sortOrder[row];
		}
		int ival;
		int index;

		StateMod_Plan smp = (StateMod_Plan)_data.get(row);

		switch (col)
		{
			case COL_ID:
				smp.setID((string)value);
				break;
			case COL_NAME:
				smp.setName((string)value);
				break;
			case COL_RIVER_NODE_ID:
				smp.setCgoto((string)value);
				break;
			case COL_ON_OFF:
				if (value is int?)
				{
					ival = ((int?)value).Value;
						smp.setSwitch(ival);
				}
				else if (value is string)
				{
					string onOff = (string)value;
					index = onOff.IndexOf(" -", StringComparison.Ordinal);
					ival = (Convert.ToInt32(onOff.Substring(0, index)));
					smp.setSwitch(ival);
				}
				break;
			case COL_TYPE:
				if (value is int?)
				{
					ival = ((int?)value).Value;
						smp.setIPlnTyp(ival);
				}
				else if (value is string)
				{
					string iPlnTyp = (string)value;
					index = iPlnTyp.IndexOf(" -", StringComparison.Ordinal);
					ival = (Convert.ToInt32(iPlnTyp.Substring(0, index)));
					smp.setIPlnTyp(ival);
				}
				break;
			case COL_EFFICIENCY:
				smp.setPeffFlag((int?)value);
				break;
			case COL_RETURN_FLOW_TABLE:
				if (value is int?)
				{
					ival = ((int?)value).Value;
						smp.setIPrf(ival);
				}
				else if (value is string)
				{
					string iPrf = (string)value;
					index = iPrf.IndexOf(" -", StringComparison.Ordinal);
					ival = (Convert.ToInt32(iPrf.Substring(0, index)));
					smp.setIPrf(ival);
				}
				break;
			case COL_FAILURE_SWITCH:
				if (value is int?)
				{
					ival = ((int?)value).Value;
						smp.setIPfail(ival);
				}
				else if (value is string)
				{
					string iPfail = (string)value;
					index = iPfail.IndexOf(" -", StringComparison.Ordinal);
					ival = (Convert.ToInt32(iPfail.Substring(0, index)));
					smp.setIPfail(ival);
				}
				break;
			case COL_INITIAL_STORAGE:
				smp.setPsto1((double?)value);
				break;
			case COL_SOURCE:
				smp.setPsource((string)value);
				break;
		}

		base.setValueAt(value, row, col);
	}

	/// <summary>
	/// Returns the text to be assigned to worksheet tooltips. </summary>
	/// <returns> a String array of tool tips. </returns>
	public virtual string[] getColumnToolTips()
	{
		string[] tips = new string[__COLUMNS];

		tips[COL_ID] = "<html>The plan station identifier is the main link" +
			" between plan data<BR>" +
			"and must be unique in the data set.</html>";
		tips[COL_NAME] = "Plan name.";

		if (__COLUMNS == 2)
		{
			return tips;
		}

		tips[COL_RIVER_NODE_ID] = "River node where plan is located.";
		tips[COL_ON_OFF] = "Indicates whether plan is on (1) or off (0)";
		tips[COL_TYPE] = "Plan type.";
		tips[COL_EFFICIENCY] = "Efficiency, annual (%).";
		tips[COL_RETURN_FLOW_TABLE] = "Plan return flow table.";
		tips[COL_INITIAL_STORAGE] = "Initial plan storage (AF).";
		tips[COL_SOURCE] = "Source ID for structure where reuse water became" +
			" available or a T&C condition originated.";
		return tips;
	}

	}

}