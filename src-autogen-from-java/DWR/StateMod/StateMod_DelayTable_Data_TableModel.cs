using System;
using System.Collections.Generic;

// StateMod_DelayTable_Data_TableModel - class for displaying delay table data in a jworksheet

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
// StateMod_DelayTable_Data_TableModel - class for displaying delay table data
//	in a jworksheet
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
// 2005-03-29	J. Thomas Sapienza, RTi	Initial version.
// 2005-03-31	JTS, RTi		Added the code to put TOTALS lines
//					in the worksheet.
// 2007-04-27	Kurt Tometich, RTi		Added getValidators method for check
//									file and data check implementation.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
// ----------------------------------------------------------------------------
// EndHeader

namespace DWR.StateMod
{

	using JWorksheet = RTi.Util.GUI.JWorksheet;
	using JWorksheet_AbstractRowTableModel = RTi.Util.GUI.JWorksheet_AbstractRowTableModel;
	using Validator = RTi.Util.IO.Validator;

	/// <summary>
	/// This class displays delay table related data.
	/// </summary>
	public class StateMod_DelayTable_Data_TableModel : JWorksheet_AbstractRowTableModel, StateMod_Data_TableModel
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
	/// Indicate whether the delay table is for monthly (true) or daily (false) data.
	/// </summary>
	private bool __monthlyData = true;

	/// <summary>
	/// Whether to show rows with "TOTAL" values.
	/// </summary>
	private bool __showTotals = true;

	/// <summary>
	/// The worksheet that this model is displayed in.  
	/// </summary>
	private JWorksheet __worksheet = null;

	/// <summary>
	/// A Vector that maps rows in the display when totals are NOT being shown to rows
	/// in the overall data Vectors.  Used to make switching between displays with and
	/// without totals relatively efficient.  See getValueAt() and setupData().
	/// </summary>
	private System.Collections.IList __rowMap = null;

	/// <summary>
	/// The Vectors of data that will actually be shown in the table.
	/// </summary>
	private System.Collections.IList[] __data = null;

	/// <summary>
	/// References to columns.
	/// </summary>
	public const int COL_ID = 0, COL_DATE = 1, COL_RETURN_AMT = 2;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="data"> the data that will be displayed in the table. </param>
	/// <param name="monthlyData"> If true the data are for monthly delay tables.  If false,
	/// the delay tables are daily. </param>
	/// <param name="editable"> whether the table data is editable or not </param>
	/// <param name="returnIsPercent"> whether the return amounts are in percents (true) or fractions (false). </param>
	/// <exception cref="Exception"> if an invalid data or dmi was passed in. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public StateMod_DelayTable_Data_TableModel(java.util.List data, boolean monthlyData, boolean editable) throws Exception
	public StateMod_DelayTable_Data_TableModel(System.Collections.IList data, bool monthlyData, bool editable)
	{
		if (data == null)
		{
			throw new Exception("Invalid data Vector passed to " + "StateMod_DelayTable_Data_TableModel constructor.");
		}

		__monthlyData = monthlyData;
		__editable = editable;

		setupData(data);
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
			// TODO (SAM - 2005-01-20) how is this class being used with Well Depletion displays
			// in the StateMod GUI?  We might need a flag for the header.
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
				return "\nPERCENT";
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
		widths[COL_ID] = 7;
		widths[COL_DATE] = 4;
		widths[COL_RETURN_AMT] = 8;
		return widths;
	}

	/// <summary>
	/// Returns the format that the specified column should be displayed in when
	/// the table is being displayed in the given table format. </summary>
	/// <param name="column"> column for which to return the format. </param>
	/// <returns> the format (as used by StringUtil.formatString() in which to display the column. </returns>
	public virtual string getFormat(int column)
	{
		switch (column)
		{
			case COL_ID:
				return "%-12.12s";
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
	/// Returns general validators based on column of data being checked. </summary>
	/// <param name="col"> Column of data to check. </param>
	/// <returns> List of validators for a column of data. </returns>
	public virtual Validator[] getValidators(int col)
	{
		// TODO KAT 2007-04-16
		// Need to add general validators but don't know
		// what data is going to be checked.
		Validator[] no_checks = new Validator[] {};
		return no_checks;
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

		if (!__showTotals)
		{
			row = ((int?)__rowMap[row]).Value;
		}
		return __data[col][row];
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
	/// Sets up the data to be displayed in the table. </summary>
	/// <param name="data"> a Vector of StateMod_DelayTable objects from which the data to b
	/// be displayed in the table will be gathered. </param>
	private void setupData(System.Collections.IList data)
	{
		int num = 0;
		int size = data.Count;
		StateMod_DelayTable dt = null;
		string id = null;
		__data = new System.Collections.IList[__COLUMNS];
		for (int i = 0; i < __COLUMNS; i++)
		{
			__data[i] = new List<object>();
		}

		__rowMap = new List<object>();

		double total = 0;
		int rowCount = 0;
		for (int i = 0; i < size; i++)
		{
			total = 0;
			dt = (StateMod_DelayTable)data[i];
			id = dt.getID();
			num = dt.getNdly();
			for (int j = 0; j < num; j++)
			{
				__data[COL_ID].Add(id);
				__data[COL_DATE].Add(new int?(j + 1));
				__data[COL_RETURN_AMT].Add(new double?(dt.getRet_val(j)));
				total += dt.getRet_val(j);
				__rowMap.Add(new int?(rowCount));
				rowCount++;
			}

			__data[COL_ID].Add("TOTAL " + id);
			__data[COL_DATE].Add(new int?(-999));
			__data[COL_RETURN_AMT].Add(new double?(total));

			rowCount++;
		}
		_rows = rowCount;
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
				break;
		}
		base.setValueAt(value, row, col);
	}

	/// <summary>
	/// Sets whether to show lines with totals.  setJWorksheet() must have been called
	/// with a non-null worksheet prior to this method.  The worksheet will be updated instantly. </summary>
	/// <param name="showTotals"> whether to show lines with totals in the worksheet. </param>
	public virtual void setShowTotals(bool showTotals)
	{
		__showTotals = showTotals;
		_sortOrder = null;

		if (__showTotals)
		{
			_rows = __data[COL_ID].Count;
		}
		else
		{
			_rows = __rowMap.Count;
		}
		__worksheet.refresh();
	}

	/// <summary>
	/// Sets the worksheet that this model appears in. </summary>
	/// <param name="worksheet"> the worksheet the model appears in. </param>
	public virtual void setJWorksheet(JWorksheet worksheet)
	{
		__worksheet = _worksheet;
	}

	}

}