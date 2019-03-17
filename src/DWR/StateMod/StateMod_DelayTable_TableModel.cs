using System;
using System.Collections.Generic;

// StateMod_DelayTable_TableModel - class for displaying delay table data in a jworksheet

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
// StateMod_DelayTable_TableModel - class for displaying delay table data
//	in a jworksheet
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
// 2003-06-09	J. Thomas Sapienza, RTi	Initial version.
// 2003-07-29	JTS, RTi		JWorksheet_RowTableModel changed to
//					JWorksheet_AbstractRowTableModel.
// 2003-09-04	SAM, RTi		Add monthlyData boolean to the
//					constructor.
// 2004-01-21	JTS, RTi		Removed the row count column and 
//					changed all the other column numbers.
// 2004-08-25	JTS, RTi		Based on the value of 'interv' in 
//					the control file, the header for
//					the return column says whether it is
//					a percent or a fraction.
// 2004-10-28	SAM, RTi		Change setValueAt() to support sort.
// 2005-03-28	JTS, RTi		Adjusted column sizes.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
// ----------------------------------------------------------------------------
// EndHeader

namespace DWR.StateMod
{

	using JWorksheet_AbstractRowTableModel = RTi.Util.GUI.JWorksheet_AbstractRowTableModel;

	/// <summary>
	/// This class displays delay table related data.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class StateMod_DelayTable_TableModel extends RTi.Util.GUI.JWorksheet_AbstractRowTableModel
	public class StateMod_DelayTable_TableModel : JWorksheet_AbstractRowTableModel
	{

	/// <summary>
	/// Number of columns in the table model.
	/// </summary>
	private readonly int __COLUMNS = 3;

	/// <summary>
	/// Whether the table data is editable or not.
	/// </summary>
	private bool __editable = false;

	/// <summary>
	/// Return values under a delay table.
	/// </summary>
	private System.Collections.IList __subDelays = null;

	/// <summary>
	/// Indicate whether the delay table is for monthly (true) or daily (false) data.
	/// </summary>
	private bool __monthlyData = true;

	/// <summary>
	/// Indicates whether the return amounts are in percents (true) or fractions (false).
	/// </summary>
	private bool __returnIsPercent = true;

	/// <summary>
	/// References to columns.
	/// </summary>
	public const int COL_ID = 0, COL_DATE = 1, COL_RETURN_AMT = 2;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="data"> the data that will be displayed in the table. </param>
	/// <param name="monthlyData"> If true the data are for monthly delay tables.  If false, the delay tables are daily. </param>
	/// <param name="editable"> whether the table data is editable or not </param>
	/// <param name="returnIsPercent"> whether the return amounts are in percents (true) or fractions (false). </param>
	/// <exception cref="Exception"> if an invalid data or dmi was passed in. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public StateMod_DelayTable_TableModel(java.util.List data, boolean monthlyData, boolean editable, boolean returnIsPercent) throws Exception
	public StateMod_DelayTable_TableModel(System.Collections.IList data, bool monthlyData, bool editable, bool returnIsPercent)
	{
		if (data == null)
		{
			throw new Exception("Invalid data Vector passed to " + "StateMod_DelayTable_TableModel constructor.");
		}
		_rows = data.Count;
		_data = data;

		__monthlyData = monthlyData;
		__editable = editable;
		__returnIsPercent = returnIsPercent;
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
			case COL_DATE:
				return typeof(Integer);
			case COL_RETURN_AMT:
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
	/// <returns> the name of the column at the given position. </returns>
	public virtual string getColumnName(int columnIndex)
	{
		switch (columnIndex)
		{
			// REVISIT (SAM - 2005-01-20)
			// how is this class being used with Well Depletion displays
			// in the StateMod GUI?  We might needa  flag for the header.
			case COL_ID:
				return "DELAY\nTABLE ID";
			case COL_DATE:
				if (__monthlyData)
				{
					return "\nMONTH";
				}
				else
				{
					return "\nDAY";
				}
			case COL_RETURN_AMT:
				if (__returnIsPercent)
				{
					return "\nPERCENT";
				}
				else
				{
					return "\nFRACTION";
				}
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
				return "%-40s";
			case COL_DATE:
				return "%8d";
			case COL_RETURN_AMT:
				return "%12.6f";
			default:
				return "%-8s";
		}
	}

	/// <summary>
	/// Returns the number of rows of data in the table.
	/// </summary>
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

		switch (col)
		{
			case COL_ID:
				StateMod_DelayTable dt = (StateMod_DelayTable)_data.get(row);
				return dt.getTableID();
			case COL_DATE:
				return new int?(row + 1);
			case COL_RETURN_AMT:
				if (__subDelays != null)
				{
					return __subDelays[row];
				}
				else
				{
					return new double?(0.0);
				}
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
		widths[COL_ID] = 7;
		widths[COL_DATE] = 4;
		widths[COL_RETURN_AMT] = 8;
		return widths;
	}

	/// <summary>
	/// Sets the delay table data that will be displayed for a particular delay. </summary>
	/// <param name="subDelays"> list of subDelay information (month/day, pct) </param>
	public virtual void setSubDelays(IList<double?> subDelays)
	{
		__subDelays = subDelays;
		_data = subDelays;
		if (__subDelays == null)
		{
			_rows = 0;
		}
		else
		{
			_rows = __subDelays.Count;
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
		if (columnIndex > 1)
		{
			return true;
		}
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
		// REVISIT (JTS - 2005-01-17)
		// not fleshed out for ID, Name
		switch (col)
		{
			case COL_ID:
				break;
			case COL_DATE:
				break;
			case COL_RETURN_AMT:
				__subDelays[row] = (double?)value;
			break;
		}
		base.setValueAt(value, row, col);
	}

	}

}