using System;

// StateMod_InstreamFlow_TableModel - Table model for displaying data in the Instream Flow station tables

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
// StateMod_InstreamFlow_TableModel - Table model for displaying data in the
//	Instream Flow station tables
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
// 2003-06-09	J. Thomas Sapienza, RTi	Initial version.
// 2003-06-10	JTS, RTi		Added right fields.
// 2003-06-19	JTS, RTi		Table model now displays actual data.
// 2003-07-29	JTS, RTi		JWorksheet_RowTableModel changed to
//					JWorksheet_AbstractRowTableModel.
// 2003-08-29	Steven A. Malers, RTi	Change setRightsVector() call to
//					setRights().
// 2003-10-13	JTS, RTi		* Added getColumnToolTips().
//					* Removed reference to the parent
//					  instream flow.
// 2004-01-21	JTS, RTi		Removed the row count column and 
//					changed all the other column numbers.
// 2004-08-26	JTS, RTi		The on/off field now accepts combo box
//					values as well as integers.
// 2004-10-28	SAM, RTi		Remove water rights code since a
//					separate table model is now used for
//					rights.
// 					Change setValueAt() to support sort.
// 2005-01-18	JTS, RTi		* Added 3 new columns (daily id,
//					  downstream node, data type).
//					* Added compactForm option to limit
//					  how many columns are displayed.
// 2005-01-20	JTS, RTi		Renamed a field to COL_DEMAND_TYPE.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
// ----------------------------------------------------------------------------
// EndHeader

namespace DWR.StateMod
{

	using JWorksheet_AbstractRowTableModel = RTi.Util.GUI.JWorksheet_AbstractRowTableModel;

	/// <summary>
	/// This table model displays instream flow data.
	/// </summary>
	public class StateMod_InstreamFlow_TableModel : JWorksheet_AbstractRowTableModel
	{

	/// <summary>
	/// Number of columns in the table model.
	/// </summary>
	private int __COLUMNS = 5;

	/// <summary>
	/// References to columns.
	/// </summary>
	public const int COL_ID = 0, COL_NAME = 1, COL_DAILY_ID = 2, COL_DOWN_NODE = 3, COL_DEMAND_TYPE = 4;

	/// <summary>
	/// Whether all the columns are shown (false) or only the ID and name columns are shown (true).
	/// </summary>
	private bool __compactForm = true;

	/// <summary>
	/// Whether the table data are editable or not.
	/// </summary>
	private bool __editable = false;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="data"> the data that will be displayed in the table. </param>
	/// <param name="editable"> whether the table data is editable or not </param>
	/// <exception cref="Exception"> if an invalid data or dmi was passed in. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public StateMod_InstreamFlow_TableModel(java.util.List data, boolean editable) throws Exception
	public StateMod_InstreamFlow_TableModel(System.Collections.IList data, bool editable) : this(data, editable, true)
	{
	}

	/// <summary>
	/// Constructor. </summary>
	/// <param name="data"> the data that will be displayed in the table. </param>
	/// <param name="editable"> whether the table data is editable or not </param>
	/// <param name="compactForm"> whether to show only the ID and name column (true) or all columns. </param>
	/// <exception cref="Exception"> if an invalid data or dmi was passed in. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public StateMod_InstreamFlow_TableModel(java.util.List data, boolean editable, boolean compactForm) throws Exception
	public StateMod_InstreamFlow_TableModel(System.Collections.IList data, bool editable, bool compactForm)
	{
		if (data == null)
		{
			throw new Exception("Invalid data Vector passed to " + "StateMod_InstreamFlow_TableModel constructor.");
		}
		_rows = data.Count;
		_data = data;

		__editable = editable;
		__compactForm = compactForm;

		if (__compactForm)
		{
			__COLUMNS = 2;
		}
		else
		{
			__COLUMNS = 5;
		}
	}

	/// <summary>
	/// returns the class of the data stored in a given column. </summary>
	/// <param name="columnIndex"> the column for which to return the data class. </param>
	public virtual Type getColumnClass(int columnIndex)
	{
		switch (columnIndex)
		{
			case COL_ID:
				return typeof(string);
			case COL_NAME:
				return typeof(string);
			case COL_DAILY_ID:
				return typeof(string);
			case COL_DOWN_NODE:
				return typeof(string);
			case COL_DEMAND_TYPE:
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
			case COL_ID:
				return "\nID";
			case COL_NAME:
				return "\nNAME";
			case COL_DAILY_ID:
				return "\nDAILY ID";
			case COL_DOWN_NODE:
				return "DOWNSTREAM\nNODE";
			case COL_DEMAND_TYPE:
				return "DEMAND\nTYPE";
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

		tips[COL_ID] = "<html>The instream flow identifier is the main link"
			+ " between instream data data<BR>"
			+ "and must be unique in the data set.</html>";
		tips[COL_NAME] = "<html>Instream flow name.</html>";
		if (!__compactForm)
		{
			tips[COL_DAILY_ID] = "<html>Daily instream flow ID.</html>";
			tips[COL_DOWN_NODE] = "<html>Downstream river node, for instream flow "
				+ "reach.</html>";
			tips[COL_DEMAND_TYPE] = "<html>Data type switch.</html>";
		}

		return tips;
	}

	/// <summary>
	/// Returns an array containing the widths (in number of characters) that the 
	/// fields in the table should be sized to. </summary>
	/// <returns> an integer array containing the widths for each field. </returns>
	public virtual int[] getColumnWidths()
	{
		int[] widths = new int[__COLUMNS];
		widths[COL_ID] = 9;
		widths[COL_NAME] = 21;

		if (!__compactForm)
		{
			widths[COL_DAILY_ID] = 12;
			widths[COL_DOWN_NODE] = 12;
			widths[COL_DEMAND_TYPE] = 4;
		}

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
				return "%-40s";
			case COL_NAME:
				return "%-40s";
			case COL_DAILY_ID:
				return "%-40s";
			case COL_DOWN_NODE:
				return "%-40s";
			case COL_DEMAND_TYPE:
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

		StateMod_InstreamFlow isf = (StateMod_InstreamFlow)_data.get(row);
		switch (col)
		{
			case COL_ID:
				return isf.getID();
			case COL_NAME:
				return isf.getName();
			case COL_DAILY_ID:
				return isf.getCifridy();
			case COL_DOWN_NODE:
				return isf.getIfrrdn();
			case COL_DEMAND_TYPE:
				return new int?(isf.getIifcom());
			default:
				return "";
		}
	}

	/// <summary>
	/// Returns whether the cell is editable or not.  Currently no cells are editable. </summary>
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
		StateMod_InstreamFlow isf = (StateMod_InstreamFlow)_data.get(row);
		switch (col)
		{
			case COL_ID:
				isf.setID((string)value);
				break;
			case COL_NAME:
				isf.setName((string)value);
				break;
			case COL_DAILY_ID:
				isf.setCifridy((string)value);
				break;
			case COL_DOWN_NODE:
				isf.setIfrrdn((string)value);
				break;
			case COL_DEMAND_TYPE:
				isf.setIifcom(((int?)value).Value);
				break;
		}
		base.setValueAt(value, row, col);
	}

	}

}