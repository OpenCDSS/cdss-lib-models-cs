using System;
using System.Collections.Generic;

// StateMod_RiverNetworkNode_Data_TableModel - table model for displaying reservoir data

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
// StateMod_RiverNetworkNode_Data_TableModel - table model for displaying reservoir 
//	data
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
// 2003-08-18	J. Thomas Sapienza, RTi	Initial version.
// 2004-01-22	JTS, RTi		Removed the row count column and 
//					changed all the other column numbers.
// 2004-10-28	SAM, RTi		Change setValueAt() to support sort.
// 2007-01-07   Kurt Tometich, RTi
//								Added new fields for the data model for
//								RiverNetworkNode.  Added Downstream Node ID
//								and Maximum Recharge Limit.  Formatted the
//								fields to match other commands.
// ----------------------------------------------------------------------------
// EndHeader

namespace DWR.StateMod
{

	using JWorksheet_AbstractRowTableModel = RTi.Util.GUI.JWorksheet_AbstractRowTableModel;
	using Validator = RTi.Util.IO.Validator;

	/// <summary>
	/// This table model displays reservoir data.
	/// </summary>
	public class StateMod_RiverNetworkNode_Data_TableModel : JWorksheet_AbstractRowTableModel, StateMod_Data_TableModel
	{

	/// <summary>
	/// Number of columns in the table model.
	/// </summary>
		private readonly int __COLUMNS = 5;
	/// <summary>
	/// References to columns.
	/// </summary>
	public const int COL_ID = 0, COL_NAME = 1, COL_COMMENT = 3, COL_CSTADN = 2, COL_GWMAXR = 4; // Maximum Recharge Limit (CFS)

	/// <summary>
	/// Constructor. </summary>
	/// <param name="data"> the data that will be displayed in the table. </param>
	public StateMod_RiverNetworkNode_Data_TableModel(System.Collections.IList data)
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
			case COL_COMMENT:
				return typeof(string);
			case COL_CSTADN:
				return typeof(string);
			case COL_GWMAXR:
				return typeof(Double);
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
	/// <param name="columnIndex"> the position of the column for which to return the name. </param>
	/// <returns> the name of the column at the given position. </returns>
	public virtual string getColumnName(int columnIndex)
	{
		switch (columnIndex)
		{
			case COL_ID:
				return "RIVER NODE ID";
			case COL_NAME:
				return "STATION NAME";
			case COL_COMMENT:
				return "COMMENT";
			case COL_CSTADN:
				return "DOWNSTREAM \nRIVER NODE ID";
			case COL_GWMAXR:
				return "MAX RECHARGE \nLIMIT (CFS)";
			default:
				return " ";
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
		widths[COL_ID] = 11;
		widths[COL_NAME] = 23;
		widths[COL_CSTADN] = 11;
		widths[COL_GWMAXR] = 11;
		widths[COL_COMMENT] = 16;

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
			case COL_ID:
				return "%-12.12s";
			case COL_NAME:
				return "%-24.24s";
			case COL_COMMENT:
				return "%-80.80s";
			case COL_CSTADN:
				return "%-12.12s";
			case COL_GWMAXR:
				return "%12.2f";
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
	/// Returns general validators based on column of data being checked. </summary>
	/// <param name="col"> Column of data to check. </param>
	/// <returns> List of validators for a column of data. </returns>
	public virtual Validator[] getValidators(int col)
	{
		Validator[] no_checks = new Validator[] {};

		switch (col)
		{
		case COL_ID:
			return StateMod_Data_TableModel_Fields.ids;
		case COL_NAME:
			return StateMod_Data_TableModel_Fields.blank;
		case COL_COMMENT:
			return no_checks; // can be blank
		case COL_CSTADN:
			return no_checks; // can be blank
		case COL_GWMAXR:
			return StateMod_Data_TableModel_Fields.nums;
		default:
			return no_checks;
		}
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

		StateMod_RiverNetworkNode r = (StateMod_RiverNetworkNode)_data.get(row);

		switch (col)
		{
			case COL_ID:
				return r.getID();
			case COL_NAME:
				return r.getName();
			case COL_COMMENT:
				return r.getComment();
			case COL_CSTADN:
				return r.getCstadn();
			case COL_GWMAXR:
				return new double?(r.getGwmaxr());
			default:
				return "";
		}
	}

	/// <summary>
	/// Returns whether the cell at the given position is editable or not.  In this
	/// table model all columns above #2 are editable. </summary>
	/// <param name="rowIndex"> unused </param>
	/// <param name="columnIndex"> the index of the column to check for whether it is editable. </param>
	/// <returns> whether the cell at the given position is editable. </returns>
	public virtual bool isCellEditable(int rowIndex, int columnIndex)
	{
		return false;
	}

	/// <summary>
	/// Sets the value at the specified position to the specified value. </summary>
	/// <param name="value"> the value to set the cell to. </param>
	/// <param name="row"> the row of the cell for which to set the value. </param>
	/// <param name="col"> the col of the cell for which to set the value. </param>
	public virtual void setValueAt(object value, int row, int col)
	{
		if (_sortOrder != null)
		{
			row = _sortOrder[row];
		}
		base.setValueAt(value, row, col);
	}

	}

}