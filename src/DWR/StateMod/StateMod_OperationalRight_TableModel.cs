using System;
using System.Collections.Generic;

// StateMod_OperationalRight_TableModel - class to display operational right data

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

namespace DWR.StateMod
{

	using JWorksheet_AbstractRowTableModel = RTi.Util.GUI.JWorksheet_AbstractRowTableModel;

	/// <summary>
	/// This class displays operational right data, in particular the list of rights in the StateMod GUI
	/// editor, and also the list of intervening structures (in which case the first two columns are hidden).
	/// </summary>
	public class StateMod_OperationalRight_TableModel : JWorksheet_AbstractRowTableModel
	{

	/// <summary>
	/// Number of columns in the table model.
	/// </summary>
	private readonly int __COLUMNS = 4;

	/// <summary>
	/// References to columns.
	/// </summary>
	public const int COL_ID = 0, COL_NAME = 1, COL_TYPE = 2, COL_STRUCTURE_ID = 3;

	/// <summary>
	/// Whether the gui data is editable or not.
	/// </summary>
	private bool __editable = false;

	/// <summary>
	/// The parent diversion under which the right and return flow is stored.
	/// </summary>
	private StateMod_OperationalRight __parentOperationalRight = null;

	/// <summary>
	/// A 10-element list of an Operational Right's intern data (identifiers).
	/// </summary>
	private IList<string> __interns = null;

	/// <summary>
	/// Constructor.  This builds the Model for displaying the diversion data. </summary>
	/// <param name="data"> the data that will be displayed in the table. </param>
	/// <param name="editable"> whether the gui data are editable or not. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public StateMod_OperationalRight_TableModel(java.util.List data, boolean editable) throws Exception
	public StateMod_OperationalRight_TableModel(System.Collections.IList data, bool editable)
	{
		if (data == null)
		{
			throw new Exception("Invalid data list passed to " + "StateMod_OperationalRight_TableModel constructor.");
		}
		_rows = data.Count;
		_data = data;

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
			case COL_TYPE:
				return typeof(string);
			case COL_STRUCTURE_ID:
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
	/// <returns> the name of the column at the given position. </returns>
	public virtual string getColumnName(int columnIndex)
	{
		switch (columnIndex)
		{
			case COL_ID:
				return "ID";
			case COL_NAME:
				return "NAME";
			case COL_TYPE:
				return "TYPE";
			case COL_STRUCTURE_ID:
				return "STRUCTURE ID";
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
			case COL_ID:
				return "%-40s";
			case COL_NAME:
				return "%-40s";
			case COL_TYPE:
				return "%s";
			case COL_STRUCTURE_ID:
				return "%-40s";
		}
		return "%8s";
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
		StateMod_OperationalRight smo = (StateMod_OperationalRight)_data.get(row);
		switch (col)
		{
			case COL_ID:
				return smo.getID();
			case COL_NAME:
				return smo.getName();
			case COL_TYPE:
				return "" + smo.getItyopr();
			case COL_STRUCTURE_ID:
				return "N/A";
				//return (String)__interns.elementAt(row);		
		}
		return " ";
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
		widths[COL_TYPE] = 4;
		widths[COL_STRUCTURE_ID] = 15;

		return widths;
	}

	/// <summary>
	/// Returns whether the cell is editable or not.  In this model, all the cells in
	/// columns 3 and greater are editable. </summary>
	/// <param name="rowIndex"> unused. </param>
	/// <param name="columnIndex"> the index of the column to check whether it is editable or not. </param>
	/// <returns> whether the cell is editable or not. </returns>
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

		switch (col)
		{
			case COL_STRUCTURE_ID:
				__interns[row] = (string)value;
				__parentOperationalRight.setInterns(__interns);
			break;
		}

		base.setValueAt(value, row, col);
	}

	/// <summary>
	/// Sets the parent diversion under which the right and return flow data is stored. </summary>
	/// <param name="parent"> the parent diversion. </param>
	public virtual void setParentOperationalRight(StateMod_OperationalRight parent)
	{
		__parentOperationalRight = parent;
	}

	/// <summary>
	/// The list of intern data associated with a specific Operational Right. </summary>
	/// <param name="interns"> a list of 10 elements containing an Operational Right's intern data. </param>
	public virtual void setInterns(IList<string> interns)
	{
		_rows = 10;
		__interns = interns;
	}

	}

}