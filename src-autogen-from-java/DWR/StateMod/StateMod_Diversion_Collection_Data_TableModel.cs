using System;
using System.Collections.Generic;

// StateMod_Diversion_Collection_Data_TableModel - Table model for displaying diversion collection data.

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
// StateMod_Diversion_Collection_Data_TableModel - Table model for displaying 
//	diversion collection data.
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
//
// 2005-03-30	J. Thomas Sapienza, RTi	Initial version.
// 2006-04-11	JTS, RTi		Corrected the classes returned from
//					getColumnClass().
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace DWR.StateMod
{

	using JWorksheet_AbstractRowTableModel = RTi.Util.GUI.JWorksheet_AbstractRowTableModel;

	/// <summary>
	/// This class is a table model for displaying diversion collection data.
	/// </summary>
	public class StateMod_Diversion_Collection_Data_TableModel : JWorksheet_AbstractRowTableModel
	{

	/// <summary>
	/// Number of columns in the table model.
	/// </summary>
	private int __COLUMNS = 5;

	/// <summary>
	/// Column references.
	/// </summary>
	private readonly int __COL_DIV = -1, __COL_ID = 0, __COL_YEAR = 1, __COL_COL_TYPE = 2, __COL_PART_TYPE = 3, __COL_PART_ID = 4;

	/// <summary>
	/// Whether the data are editable or not.
	/// </summary>
	private bool __editable = false;

	/// <summary>
	/// The data displayed in the table.
	/// </summary>
	private System.Collections.IList[] __data = null;

	/// <summary>
	/// Constructor.  This builds the Model for displaying diversion data </summary>
	/// <param name="data"> the data that will be displayed in the table. </param>
	public StateMod_Diversion_Collection_Data_TableModel(System.Collections.IList data) : this(data, false)
	{
	}

	/// <summary>
	/// Constructor.  This builds the Model for displaying diversion data </summary>
	/// <param name="data"> the data that will be displayed in the table. </param>
	/// <param name="editable"> whether the data are editable or not. </param>
	public StateMod_Diversion_Collection_Data_TableModel(System.Collections.IList data, bool editable)
	{
		if (data == null)
		{
			data = new List<object>();
		}
		_data = data;
		__editable = editable;

		setupData();
	}

	/// <summary>
	/// From AbstractTableModel.  Returns the class of the data stored in a given
	/// column. </summary>
	/// <param name="columnIndex"> the column for which to return the data class. </param>
	public virtual Type getColumnClass(int columnIndex)
	{
		switch (columnIndex)
		{
			case __COL_ID:
				return typeof(string);
			case __COL_DIV:
				return typeof(Integer);
			case __COL_YEAR:
				return typeof(Integer);
			case __COL_COL_TYPE:
				return typeof(string);
			case __COL_PART_TYPE:
				return typeof(string);
			case __COL_PART_ID:
				return typeof(string);
		}
		return typeof(string);
	}

	/// <summary>
	/// From AbstractTableMode.  Returns the number of columns of data. </summary>
	/// <returns> the number of columns of data. </returns>
	public virtual int getColumnCount()
	{
		return __COLUMNS;
	}

	/// <summary>
	/// From AbstractTableMode.  Returns the name of the column at the given position. </summary>
	/// <returns> the name of the column at the given position. </returns>
	public virtual string getColumnName(int columnIndex)
	{
		switch (columnIndex)
		{
			case __COL_ID:
				return "DIVERSION\nID";
			case __COL_DIV:
				return "\nDIVISION";
			case __COL_YEAR:
				return "\nYEAR";
			case __COL_COL_TYPE:
				return "COLLECTION\nTYPE";
			case __COL_PART_TYPE:
				return "PART\nTYPE";
			case __COL_PART_ID:
				return "PART\nID";
		}

		return " ";
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
			case __COL_ID:
				return "%-12.12s";
			case __COL_DIV:
				return "%8d";
			case __COL_YEAR:
				return "%8d";
			case __COL_COL_TYPE:
				return "%-12.12s";
			case __COL_PART_TYPE:
				return "%-12.12s";
			case __COL_PART_ID:
				return "%-12.12s";
		}
		return "%-8s";
	}

	/// <summary>
	/// From AbstractTableMode.  Returns the number of rows of data in the table.
	/// </summary>
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
		if (_sortOrder != null)
		{
			row = _sortOrder[row];
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
		widths[__COL_ID] = 8;
	//	widths[__COL_DIV] =		6;
		widths[__COL_YEAR] = 5;
		widths[__COL_COL_TYPE] = 9;
		widths[__COL_PART_TYPE] = 5;
		widths[__COL_PART_ID] = 6;
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
	/// Sets up the data Vectors to display the diversion collection data in the
	/// GUI.
	/// </summary>
	private void setupData()
	{
		int[] years = null;
		int len = 0;
		int size = _data.size();
		int size2 = 0;
		StateMod_Diversion l = null;
		string colType = null;
		string id = null;
		string partType = null;
		System.Collections.IList ids = null;
		__data = new System.Collections.IList[__COLUMNS];
		for (int i = 0; i < __COLUMNS; i++)
		{
			__data[i] = new List<object>();
		}

		int rows = 0;

		for (int i = 0; i < size; i++)
		{
			l = (StateMod_Diversion)_data.get(i);
			id = l.getID();
	//		div = new Integer(l.getCollectionDiv());

			years = l.getCollectionYears();
			colType = l.getCollectionType();
			partType = l.getCollectionPartType();

			if (years == null)
			{
				len = 0;
			}
			else
			{
				len = years.Length;
			}

			for (int j = 0; j < len; j++)
			{
				ids = l.getCollectionPartIDs(years[j]);
				if (ids == null)
				{
					size2 = 0;
				}
				else
				{
					size2 = ids.Count;
				}

				for (int k = 0; k < size2; k++)
				{
					__data[__COL_ID].Add(id);
	//				__data[__COL_DIV].add(div);
					__data[__COL_YEAR].Add(new int?(years[j]));
					__data[__COL_COL_TYPE].Add(colType);
					__data[__COL_PART_TYPE].Add(partType);
					__data[__COL_PART_ID].Add(ids[k]);
					rows++;
				}
			}
		}
		_rows = rows;
	}

	/// <summary>
	/// Inserts the specified value into the table at the given position. </summary>
	/// <param name="value"> the object to store in the table cell. </param>
	/// <param name="row"> the row of the cell in which to place the object. </param>
	/// <param name="col"> the column of the cell in which to place the object. </param>
	public virtual void setValueAt(object value, int row, int col)
	{
		switch (col)
		{
		}

		base.setValueAt(value, row, col);
	}

	}

}