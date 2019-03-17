using System;
using System.Collections.Generic;

// StateMod_ReservoirAccount_Data_TableModel - table model for displaying reservoir account data

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
// StateMod_ReservoirAccount_Data_TableModel - table model for displaying 
//	reservoir account data
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

	using JWorksheet_AbstractRowTableModel = RTi.Util.GUI.JWorksheet_AbstractRowTableModel;

	/// <summary>
	/// This table model displays reservoir account data.  The model can display 
	/// account data for a single reservoir or for 1+ reservoirs.  The difference is
	/// specified in the constructor and affects how many columns of data are shown.
	/// </summary>
	public class StateMod_ReservoirAccount_Data_TableModel : JWorksheet_AbstractRowTableModel
	{

	/// <summary>
	/// Number of columns in the table model.  
	/// </summary>
	private int __COLUMNS = 7;

	/// <summary>
	/// References to columns.
	/// </summary>
	public const int COL_RESERVOIR_ID = 0, COL_OWNER_ID = 1, COL_OWNER_ACCOUNT = 2, COL_MAX_STORAGE = 3, COL_INITIAL_STORAGE = 4, COL_PRORATE_EVAP = 5, COL_OWNERSHIP_TIE = 6;

	/// <summary>
	/// Whether the table data is editable.
	/// </summary>
	private bool __editable = true;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="data"> the data that will be displayed in the table. </param>
	/// <param name="editable"> whether the table data can be modified. </param>
	public StateMod_ReservoirAccount_Data_TableModel(System.Collections.IList data, bool editable)
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
	/// <param name="col"> the column for which to return the data class. </param>
	/// <returns> the class of the data stored in a given column. </returns>
	public virtual Type getColumnClass(int col)
	{
		switch (col)
		{
			case COL_RESERVOIR_ID:
				return typeof(string);
			case COL_OWNER_ID:
				return typeof(string);
			case COL_OWNER_ACCOUNT:
				return typeof(string);
			case COL_MAX_STORAGE:
				return typeof(Double);
			case COL_INITIAL_STORAGE:
				return typeof(Double);
			case COL_PRORATE_EVAP:
				return typeof(Double);
			case COL_OWNERSHIP_TIE:
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
	/// <param name="col"> the position of the column for which to return the name. </param>
	/// <returns> the name of the column at the given position. </returns>
	public virtual string getColumnName(int col)
	{
		switch (col)
		{
			case COL_RESERVOIR_ID:
				return "\nRESERVOIR\nID";
			case COL_OWNER_ID:
				return "\nOWNER\nID";
			case COL_OWNER_ACCOUNT:
				return "\nOWNER\nACCOUNT";
			case COL_MAX_STORAGE:
				return "MAXIMUM\nSTORAGE\n(ACFT)";
			case COL_INITIAL_STORAGE:
				return "INITIAL\nSTORAGE\n(ACFT)";
			case COL_PRORATE_EVAP:
				return "EVAPORATION\nDISTRIBUTION\nFLAG";
			case COL_OWNERSHIP_TIE:
				return "ACCOUNT\nONE FILL\nCALCULATION";
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

		tips[COL_RESERVOIR_ID] = "<html>The reservoir station ID of the reservoir to "
			+ "which<br>the rights belong.</html>";
		tips[COL_OWNER_ID] = "Sequential number 1+ (not used by StateMod)";
		tips[COL_OWNER_ACCOUNT] = "Account name.";
		tips[COL_MAX_STORAGE] = "Maximum account storage (ACFT).";
		tips[COL_INITIAL_STORAGE] = "Initial account storage (ACFT).";
		tips[COL_PRORATE_EVAP] = "How to prorate evaporation.";
		tips[COL_OWNERSHIP_TIE] = "One fill rule calculation flag.";
		return tips;
	}

	/// <summary>
	/// Returns the format that the specified column should be displayed in when
	/// the table is being displayed in the given table format. </summary>
	/// <param name="col"> column for which to return the format. </param>
	/// <returns> the format (as used by StringUtil.formatString() in which to display the column. </returns>
	public virtual string getFormat(int col)
	{
		switch (col)
		{
			case COL_RESERVOIR_ID:
				return "%-12.12s";
			case COL_OWNER_ID:
				return "%-12.12s";
			case COL_OWNER_ACCOUNT:
				return "%-24.24s";
			case COL_MAX_STORAGE:
				return "%12.1f";
			case COL_INITIAL_STORAGE:
				return "%12.1f";
			case COL_PRORATE_EVAP:
				return "%8.0f";
			case COL_OWNERSHIP_TIE:
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

		StateMod_ReservoirAccount rac = (StateMod_ReservoirAccount)_data.get(row);

		switch (col)
		{
			case COL_RESERVOIR_ID:
				return rac.getCgoto();
			case COL_OWNER_ID:
				return rac.getID();
			case COL_OWNER_ACCOUNT:
				return rac.getName();
			case COL_MAX_STORAGE:
				return new double?(rac.getOwnmax());
			case COL_INITIAL_STORAGE:
						return new double?(rac.getCurown());
			case COL_PRORATE_EVAP:
				return new double?(rac.getPcteva());
			case COL_OWNERSHIP_TIE:
				return new int?(rac.getN2own());
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

		widths[COL_RESERVOIR_ID] = 8;
		widths[COL_OWNER_ID] = 7;
		widths[COL_OWNER_ACCOUNT] = 14;
		widths[COL_MAX_STORAGE] = 7;
		widths[COL_INITIAL_STORAGE] = 7;
		widths[COL_PRORATE_EVAP] = 10;
		widths[COL_OWNERSHIP_TIE] = 9;

		return widths;
	}

	/// <summary>
	/// Returns whether the cell at the given position is editable.  All columns
	/// are editable unless the table is not editable. </summary>
	/// <param name="rowIndex"> unused </param>
	/// <param name="col"> the index of the column to check for whether it is editable. </param>
	/// <returns> whether the cell at the given position is editable. </returns>
	public virtual bool isCellEditable(int rowIndex, int col)
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
		double dval;
		int ival;
		StateMod_ReservoirAccount rac = (StateMod_ReservoirAccount)_data.get(row);

		switch (col)
		{
			case COL_RESERVOIR_ID:
				rac.setCgoto((string)value);
				break;
			case COL_OWNER_ID:
				rac.setID((string)value);
				break;
			case COL_OWNER_ACCOUNT:
				rac.setName((string)value);
				break;
			case COL_MAX_STORAGE:
				dval = ((double?)value).Value;
				rac.setOwnmax(dval);
				break;
			case COL_INITIAL_STORAGE:
				dval = ((double?)value).Value;
				rac.setCurown(dval);
				break;
			case COL_PRORATE_EVAP:
				if (value is double?)
				{
					dval = ((double?)value).Value;
					rac.setPcteva(dval);
				}
				else if (value is string)
				{
					int index = ((string)value).IndexOf(" -", StringComparison.Ordinal);
					string s = ((string)value).Substring(0, index);
					dval = (Convert.ToDouble(s));
					rac.setPcteva(dval);
				}
				break;
			case COL_OWNERSHIP_TIE:
				if (value is int?)
				{
					ival = ((int?)value).Value;
					rac.setN2own(ival);
				}
				else if (value is string)
				{
					string n2owns = (string)value;
					int index = n2owns.IndexOf(" -", StringComparison.Ordinal);
					ival = (Convert.ToInt32(n2owns.Substring(0, index)));
					rac.setN2own(ival);
				}
				break;
		}

		base.setValueAt(value, row, col);
	}

	}

}