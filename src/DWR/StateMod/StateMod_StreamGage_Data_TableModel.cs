using System;
using System.Collections.Generic;

// StateMod_StreamGage_Data_TableModel - table model for displaying stream gage station data

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
// StateMod_StreamGage_Data_TableModel - table model for displaying stream gage
//					station data
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
//
// 2005-03-30	J. Thomas Sapienza, RTi	Initial version.
// 2007-04-27	Kurt Tometich, RTi		Added getValidators method for check
//									file and data check implementation.
// ----------------------------------------------------------------------------

namespace DWR.StateMod
{

	using JWorksheet_AbstractRowTableModel = RTi.Util.GUI.JWorksheet_AbstractRowTableModel;
	using Validator = RTi.Util.IO.Validator;

	/// <summary>
	/// This table model displays reservoir data.
	/// </summary>
	public class StateMod_StreamGage_Data_TableModel : JWorksheet_AbstractRowTableModel, StateMod_Data_TableModel
	{

	/// <summary>
	/// Number of columns in the table model.
	/// </summary>
	private readonly int __COLUMNS = 4;

	/// <summary>
	/// References to columns.
	/// </summary>
	public const int COL_ID = 0, COL_NAME = 1, COL_NODE_ID = 2, COL_DAILY_ID = 3;

	/// <summary>
	/// Whether the data are editable or not.
	/// </summary>
	private bool __editable = true;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="data"> the data that will be displayed in the table. </param>
	/// <exception cref="Exception"> if an invalid data or dmi was passed in. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public StateMod_StreamGage_Data_TableModel(java.util.List data) throws Exception
	public StateMod_StreamGage_Data_TableModel(System.Collections.IList data) : this(data, true)
	{
	}

	/// <summary>
	/// Constructor. </summary>
	/// <param name="data"> the data that will be displayed in the table. </param>
	public StateMod_StreamGage_Data_TableModel(System.Collections.IList data, bool editable)
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
			case COL_NODE_ID:
				return typeof(string);
			case COL_DAILY_ID:
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
	/// <param name="columnIndex"> the position of the column for which to return the name. </param>
	/// <returns> the name of the column at the given position. </returns>
	public virtual string getColumnName(int columnIndex)
	{
		switch (columnIndex)
		{
			case COL_ID:
				return "\nID";
			case COL_NAME:
				return "\nNAME";
			case COL_NODE_ID:
				return "RIVER\nNODE ID";
			case COL_DAILY_ID:
				return "DAILY\nID";
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
		widths[COL_ID] = 10;
		widths[COL_NAME] = 20;
		widths[COL_NODE_ID] = 8;
		widths[COL_DAILY_ID] = 8;

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
			case COL_NODE_ID:
				return "%-12.12s";
			case COL_DAILY_ID:
				return "%-12.12s";
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
			case COL_NODE_ID:
				return StateMod_Data_TableModel_Fields.ids;
			case COL_DAILY_ID:
				return StateMod_Data_TableModel_Fields.ids;
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

		StateMod_StreamGage sg = (StateMod_StreamGage)_data.get(row);

		switch (col)
		{
			case COL_ID:
				return sg.getID();
			case COL_NAME:
				return sg.getName();
			case COL_NODE_ID:
				return sg.getCgoto();
			case COL_DAILY_ID:
				return sg.getCrunidy();
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
		if (!__editable)
		{
			return false;
		}

		return true;
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

		StateMod_StreamGage sg = (StateMod_StreamGage)_data.get(row);

		switch (col)
		{
			case COL_ID:
				sg.setID((string)value);
				break;
			case COL_NAME:
				sg.setName((string)value);
				break;
			case COL_NODE_ID:
				sg.setCgoto((string)value);
				break;
			case COL_DAILY_ID:
				sg.setCrunidy((string)value);
				break;
		}

		base.setValueAt(value, row, col);
	}

	}

}