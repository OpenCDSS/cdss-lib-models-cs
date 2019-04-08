using System;
using System.Collections.Generic;

// StateMod_Save_TableModel - table model for displaying the save table.

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
// StateMod_Save_TableModel - table model for displaying the save table.
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
// 2003-09-10	J. Thomas Sapienza, RTi	Initial version.
// 2004-01-22	JTS, RTi		Removed the row count column and 
//					changed all the other column numbers.
// 2004-08-25	JTS, RTi		Renamed column 0.
// ----------------------------------------------------------------------------

namespace DWR.StateMod
{

	using JWorksheet_AbstractRowTableModel = RTi.Util.GUI.JWorksheet_AbstractRowTableModel;
	using DataSetComponent = RTi.Util.IO.DataSetComponent;

	/// <summary>
	/// This table model displays response data.
	/// </summary>
	public class StateMod_Save_TableModel : JWorksheet_AbstractRowTableModel
	{

	/// <summary>
	/// Number of columns in the table model.
	/// </summary>
	private readonly int __COLUMNS = 2;

	/// <summary>
	/// References to columns.
	/// </summary>
	public const int COL_DESC = 0, COL_FILE = 1;

	/// <summary>
	/// The numbers of the components stored in the worksheet at each row.
	/// </summary>
	private int[] __data;

	/// <summary>
	/// The dataset for which to display information in the worksheet.
	/// </summary>
	private StateMod_DataSet __dataset;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="dataset"> the dataset for which to display information in the worksheet. </param>
	/// <exception cref="Exception"> if an invalid data or dmi was passed in. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public StateMod_Save_TableModel(StateMod_DataSet dataset) throws Exception
	public StateMod_Save_TableModel(StateMod_DataSet dataset)
	{
		__dataset = dataset;

		// get the array of the ints that refer to the groups in
		// the data set.
		int[] groups = __dataset.getComponentGroupNumbers();

		System.Collections.IList ints = new List<object>();
		DataSetComponent dsc = null;
		System.Collections.IList v = null;

		// Go through each of the groups and get their data out.  Group data
		// consists of the DataSetComponents the group contains.  For each
		// of the group's DataSetComponents, if it has data, then add its
		// component type to the accumulation vector.
		for (int i = 0; i < groups.Length; i++)
		{

			dsc = __dataset.getComponentForComponentType(groups[i]);
			v = (System.Collections.IList)dsc.getData();
			if (v == null)
			{
				v = new List<object>();
			}
			for (int j = 0; j < v.Count; j++)
			{
				dsc = (DataSetComponent)v[j];
				// get the dirty components -- they can be saved 
				// (or not).
				if (dsc.isDirty())
				{
					ints.Add(new int?(dsc.getComponentType()));
				}
			}
		}

		// now transfer the numbers of the DataSetComponents with data into
		// an int array from the Vector.
		__data = new int[ints.Count];
		for (int i = 0; i < ints.Count; i++)
		{
			__data[i] = ((int?)ints[i]).Value;
		}

		_rows = __data.Length;

	}

	/// <summary>
	/// From AbstractTableModel; returns the class of the data stored in a given
	/// column. </summary>
	/// <param name="columnIndex"> the column for which to return the data class. </param>
	public virtual Type getColumnClass(int columnIndex)
	{
		switch (columnIndex)
		{
			case COL_DESC:
				return typeof(string);
			case COL_FILE:
				return typeof(string);
			default:
				return typeof(string);
		}
	}

	/// <summary>
	/// From AbstractTableModel; returns the number of columns of data. </summary>
	/// <returns> the number of columns of data. </returns>
	public virtual int getColumnCount()
	{
		return __COLUMNS;
	}

	/// <summary>
	/// From AbstractTableModel; returns the name of the column at the given position. </summary>
	/// <param name="columnIndex"> the position of the column for which to return the name. </param>
	/// <returns> the name of the column at the given position. </returns>
	public virtual string getColumnName(int columnIndex)
	{
		switch (columnIndex)
		{
			case COL_DESC:
				return "DATA SET COMPONENT";
			case COL_FILE:
				return "FILE NAME";
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
			case COL_DESC:
				return "%-40s";
			case COL_FILE:
				return "%-40s";
			default:
				return "%-8s";
		}
	}

	/// <summary>
	/// Gets the number of the component for which table is displayed at the specified
	/// row. </summary>
	/// <param name="row"> the row of the component for which to return the component number. </param>
	public virtual int getRowComponentNum(int row)
	{
		return __data[row];
	}

	/// <summary>
	/// From AbstractTableModel; returns the number of rows of data in the table. </summary>
	/// <returns> the number of rows of data in the table. </returns>
	public virtual int getRowCount()
	{
		return _rows;
	}

	/// <summary>
	/// From AbstractTableModel; returns the data that should be placed in the JTable
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

		switch (col)
		{
			case COL_DESC:
				string group = __dataset.getComponentForComponentType(__dataset.lookupComponentGroupTypeForComponent(__data[row])).getComponentName();

				return group + ": "
					+__dataset.getComponentForComponentType(__data[row]).getComponentName();
				goto case COL_FILE;
			case COL_FILE:
				return __dataset.getComponentForComponentType(__data[row]).getDataFileName();
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
		widths[COL_DESC] = 35;
		widths[COL_FILE] = 35;
		return widths;
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

	}

}