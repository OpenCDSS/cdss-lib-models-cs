using System;
using System.Collections.Generic;

// StateMod_ReturnFlow_Data_TableModel - Table model for displaying data in the return flow tables
// (for return flows and depletions).

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
// StateMod_ReturnFlow_Data_TableModel - Table model for displaying data in the
//	return flow tables (for return flows and depletions).
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
//
// 2005-04-04	J. Thomas Sapienza, RTi	Initial version.
// ----------------------------------------------------------------------------

namespace DWR.StateMod
{

	using JWorksheet_AbstractRowTableModel = RTi.Util.GUI.JWorksheet_AbstractRowTableModel;

	/// <summary>
	/// This table model display data in return flow tables for use with return flows and depletions.
	/// </summary>
	public class StateMod_ReturnFlow_Data_TableModel : JWorksheet_AbstractRowTableModel
	{

	/// <summary>
	/// Number of columns in the table model.
	/// </summary>
	private const int __COLUMNS = 3;

	/// <summary>
	/// References to columns.
	/// </summary>
	public const int COL_RIVER_NODE = 0, COL_RETURN_PCT = 1, COL_RETURN_ID = 2;

	/// <summary>
	/// Whether the table data can be edited or not.
	/// </summary>
	private bool __editable = true;

	/// <summary>
	/// Whether the data are return flows or depletions. If true, the data are return
	/// flows.  If false the data are depletions.
	/// </summary>
	private bool __is_return;

	/// <summary>
	/// Constructor.  This builds the Model for displaying the return flow data. </summary>
	/// <param name="data"> the data that will be displayed in the table. </param>
	/// <param name="editable"> whether the data can be edited or not. </param>
	/// <param name="is_return"> Specify true for return flows and false for depletions. </param>
	public StateMod_ReturnFlow_Data_TableModel(System.Collections.IList data, bool editable, bool is_return)
	{
		if (data == null)
		{
			_data = new List<object>();
		}
		else
		{
			_data = data;
		}
		_rows = _data.size();

		__editable = editable;
		__is_return = is_return;
	}

	/// <summary>
	/// Returns the class of the data stored in a given column. </summary>
	/// <param name="columnIndex"> the column for which to return the data class. </param>
	public virtual Type getColumnClass(int columnIndex)
	{
		switch (columnIndex)
		{
			case COL_RIVER_NODE:
				return typeof(string);
			case COL_RETURN_PCT:
				return typeof(Double);
			case COL_RETURN_ID:
				return typeof(Integer);
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
			case COL_RIVER_NODE:
				if (__is_return)
				{
					return "\nRIVER NODE RECEIVING RETURN FLOW";
				}
				else
				{
					return "\nRIVER NODE BEING DEPLETED";
				}
			case COL_RETURN_PCT:
				if (__is_return)
				{
					return "\n% OF RETURN";
				}
				else
				{
					return "\n% OF DEPLETION";
				}
			case COL_RETURN_ID:
				return "DELAY\nTABLE ID";
			default:
				return " ";
		}
	}

	/// <summary>
	/// Returns the text to be assigned to worksheet tooltips. </summary>
	/// <returns> a String array of tool tips. </returns>
	public virtual string[] getColumnToolTips()
	{
		string[] tips = new string[__COLUMNS];

		if (__is_return)
		{
			tips[COL_RIVER_NODE] = "River node ID receiving return flow.";
			tips[COL_RETURN_PCT] = "% of return (0-100)";
		}
		else
		{
			tips[COL_RIVER_NODE] = "River node ID being depleted.";
			tips[COL_RETURN_PCT] = "% of depletion (0-100)";
		}
		tips[COL_RETURN_ID] = "Delay table identifier";

		return tips;
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
		widths[COL_RIVER_NODE] = 31;
		widths[COL_RETURN_PCT] = 12;
		widths[COL_RETURN_ID] = 14;
		return widths;
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
			case COL_RIVER_NODE:
				return "%-12.12s";
			case COL_RETURN_PCT:
				return "%12.6f";
			case COL_RETURN_ID:
				return "%8d";
			default:
				return "%-8s";
		}
	}

	/// <summary>
	/// Returns the number of rows of data in the table. </summary>
	/// <returns> the number of rows of data in the table. </returns>
	public virtual int getRowCount()
	{
		return _rows;
	}

	/// <summary>
	/// Returns the data that should be placed in the JTable
	/// at the given row and column. </summary>
	/// <param name="row"> the row for which to return data. </param>
	/// <param name="col"> the column for which to return data. </param>
	/// <returns> the data that should be placed in the JTable at the given row and col. </returns>
	public virtual object getValueAt(int row, int col)
	{
		if (_sortOrder != null)
		{
			row = _sortOrder[row];
		}

		StateMod_ReturnFlow rf = (StateMod_ReturnFlow)_data.get(row);
		switch (col)
		{
			case COL_RIVER_NODE:
				return rf.getCrtnid();
			case COL_RETURN_PCT:
				return new double?(rf.getPcttot());
			case COL_RETURN_ID:
				return new int?(rf.getIrtndl());
			default:
				return "";
		}
	}

	/// <summary>
	/// Returns whether the cell is editable or not.  All cells are editable, unless the worksheet is not editable. </summary>
	/// <param name="rowIndex"> unused. </param>
	/// <param name="columnIndex"> the index of the column to check whether it is editable. </param>
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
		double dval;
		int ival;
		int index;
		string s;

		StateMod_ReturnFlow rf = (StateMod_ReturnFlow)_data.get(row);
		switch (col)
		{
			case COL_RIVER_NODE:
				rf.setCrtnid((string)value);
				break;
			case COL_RETURN_PCT:
				dval = ((double?)value).Value;
				rf.setPcttot(dval);
				break;
			case COL_RETURN_ID:
				if (value is string)
				{
					index = ((string)value).IndexOf(" -", StringComparison.Ordinal);
					s = null;
					if (index > -1)
					{
						s = ((string)value).Substring(0,index);
					}
					else
					{
						s = (string)value;
					}
					rf.setIrtndl(s);
				}
				else
				{
					if (value == null)
					{
						// user input a blank value -- just keep what was originally in the table
						return;
					}
					ival = ((int?)value).Value;
					rf.setIrtndl(ival);
				}
				break;
		}

		base.setValueAt(value, row, col);
	}

	}

}