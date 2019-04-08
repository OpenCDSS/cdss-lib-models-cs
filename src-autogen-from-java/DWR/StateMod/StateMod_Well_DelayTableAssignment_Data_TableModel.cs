using System;
using System.Collections.Generic;

// StateMod_Well_DelayTableAssignment_Data_TableModel - Table model for displaying data for delay table assignment worksheets.

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
// StateMod_Well_DelayTableAssignment_Data_TableModel - Table model for 
//	displaying data for delay table assignment worksheets.
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
//
// 2005-04-04	J. Thomas Sapienza, RTi	Initial version.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace DWR.StateMod
{

	using JWorksheet = RTi.Util.GUI.JWorksheet;
	using JWorksheet_AbstractRowTableModel = RTi.Util.GUI.JWorksheet_AbstractRowTableModel;

	/// <summary>
	/// This class is a table model for displaying delay table data.
	/// </summary>
	public class StateMod_Well_DelayTableAssignment_Data_TableModel : JWorksheet_AbstractRowTableModel
	{

	/// <summary>
	/// Number of columns in the table model.
	/// </summary>
	private readonly int __COLUMNS = 4;

	/// <summary>
	/// Columns
	/// </summary>
	private readonly int __COL_ID = 0, __COL_NODE_ID = 1, __COL_PERCENT = 2, __COL_DELAY_ID = 3;

	/// <summary>
	/// Whether the data are editable or not.
	/// </summary>
	private bool __editable = true;

	/// <summary>
	/// Whether the data are depletions or not.
	/// </summary>
	private bool __isDepletion = false;

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
	/// Array of Vectors, each of which holds the data for one of the columns in the
	/// table.  Since the data cannot be pulled out from the data objects directly, 
	/// this is done to make display efficient.
	/// </summary>
	private System.Collections.IList[] __data = null;

	/// <summary>
	/// Constructor.  This builds the Model for displaying delay table data </summary>
	/// <param name="data"> the data that will be displayed in the table. </param>
	/// <param name="editable"> whether the data are editable or not. </param>
	/// <param name="isDepletion"> whether the data shown are return flows or depletions. </param>
	public StateMod_Well_DelayTableAssignment_Data_TableModel(System.Collections.IList data, bool editable, bool isDepletion)
	{
		if (data == null)
		{
			data = new List<object>();
			_rows = 0;
		}

		__isDepletion = isDepletion;
		_data = data;
		__editable = editable;
		setupData();
	}

	/// <summary>
	/// Returns the class of the data stored in a given column. </summary>
	/// <param name="columnIndex"> the column for which to return the data class. </param>
	/// <returns> the class of the data stored in a given column. </returns>
	public virtual Type getColumnClass(int columnIndex)
	{
		switch (columnIndex)
		{
			case __COL_ID:
				return typeof(string);
			case __COL_NODE_ID:
				return typeof(string);
			case __COL_PERCENT:
				return typeof(Double);
			case __COL_DELAY_ID:
				return typeof(string);
		}
		return typeof(string);
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
	/// <param name="columnIndex"> the position for which to return the column name. </param>
	/// <returns> the name of the column at the given position. </returns>
	public virtual string getColumnName(int columnIndex)
	{
		switch (columnIndex)
		{
			case __COL_ID:
				return "\n\nWELL\nSTATION\nID";
			case __COL_NODE_ID:
				if (__isDepletion)
				{
					return "\nRIVER\nNODE ID\nBEING\nDEPLETED";
				}
				else
				{
					return "RIVER\nNODE ID\nRECEIVING\nRETURN" + "\nFLOW";
				}
			case __COL_PERCENT:
				return "\n\n\n\nPERCENT";
			case __COL_DELAY_ID:
				return "\n\n\nDELAY\nTABLE ID";
		}
		return " ";
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
			case __COL_ID:
				return "%-12.12s";
			case __COL_NODE_ID:
				return "%-12.12s";
			case __COL_PERCENT:
				return "%10.2f";
			case __COL_DELAY_ID:
				return "%-12.12s";
		}
		return "%8d";
	}

	/// <summary>
	/// Returns the number of rows of data in the table. </summary>
	/// <returns> the number of rows of data in the table. </returns>
	public virtual int getRowCount()
	{
		return _rows;
	}

	/// <summary>
	/// From AbstractTableMode.  Returns the data that should be placed in the JTable
	/// at the given row and column. </summary>
	/// <param name="row"> the row for which to return data. </param>
	/// <param name="col"> the column for which to return data. </param>
	/// <returns> the data that should be placed in the JTable at the given row and col. </returns>
	public virtual object getValueAt(int row, int col)
	{
		// make sure the row numbers are never sorted ...
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
		widths[__COL_ID] = 9;
		widths[__COL_NODE_ID] = 9;
		widths[__COL_PERCENT] = 7;
		widths[__COL_DELAY_ID] = 7;
		return widths;
	}

	/// <summary>
	/// Returns whether the cell is editable or not.  In this model, all the cells in
	/// columns 3 and greater are editable. </summary>
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
		return false;
	}

	/// <summary>
	/// Sets up the data to be displayed in the table.
	/// </summary>
	private void setupData()
	{
		int num = 0;
		int size = _data.size();
		StateMod_Well well = null;
		string id = null;
		double total = 0;
		int rowCount = 0;
		__data = new System.Collections.IList[__COLUMNS];
		for (int i = 0; i < __COLUMNS; i++)
		{
			__data[i] = new List<object>();
		}

		__rowMap = new List<object>();

		StateMod_ReturnFlow rf = null;
		System.Collections.IList returnFlows = null;
		for (int i = 0; i < size; i++)
		{
			total = 0;
			well = (StateMod_Well)_data.get(i);
			id = well.getID();

			if (__isDepletion)
			{
				num = well.getNrtnw2();
				returnFlows = well.getDepletions();
			}
			else
			{
				num = well.getNrtnw();
				returnFlows = well.getReturnFlows();
			}
			for (int j = 0; j < num; j++)
			{
				rf = (StateMod_ReturnFlow)returnFlows[j];
				__data[__COL_ID].Add(id);
				__data[__COL_NODE_ID].Add(rf.getCrtnid());
				__data[__COL_PERCENT].Add(new double?(rf.getPcttot()));
				__data[__COL_DELAY_ID].Add("" + rf.getIrtndl());
				total += rf.getPcttot();
				__rowMap.Add(new int?(rowCount));
				rowCount++;
			}

			__data[__COL_ID].Add(id);
			__data[__COL_NODE_ID].Add("TOTAL");
			__data[__COL_PERCENT].Add(new double?(total));
			__data[__COL_DELAY_ID].Add("");

			rowCount++;
		}
		_rows = rowCount;
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
			_rows = __data[__COL_ID].Count;
		}
		else
		{
			_rows = __rowMap.Count;
		}
		__worksheet.refresh();
	}

	public virtual void setValueAt(object value, int row, int col)
	{
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