using System;
using System.Collections.Generic;

// StateMod_Well_Collection_Data_TableModel - Table model for displaying reservoir collection data.

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
// StateMod_Well_Collection_Data_TableModel - Table model for displaying 
//	reservoir collection data.
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
//
// 2005-04-07	J. Thomas Sapienza, RTi	Initial version.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace DWR.StateMod
{

	using JWorksheet_AbstractRowTableModel = RTi.Util.GUI.JWorksheet_AbstractRowTableModel;

	/// <summary>
	/// This class is a table model for displaying reservoir collection data.
	/// </summary>
	public class StateMod_Well_Collection_Data_TableModel : JWorksheet_AbstractRowTableModel
	{

	/// <summary>
	/// Number of columns in the table model.
	/// </summary>
	private int __COLUMNS = 6;

	/// <summary>
	/// Column references.
	/// </summary>
	private readonly int __COL_ID = 0, __COL_YEAR = 1, __COL_COL_TYPE = 2, __COL_PART_TYPE = 3, __COL_PART_ID = 4, __COL_PART_ID_TYPE = 5;

	/// <summary>
	/// Whether the data are editable or not.
	/// </summary>
	private bool __editable = false;

	/// <summary>
	/// The data displayed in the table, which is created in this method based on the source StateMod_Well data.
	/// </summary>
	private System.Collections.IList[] __data = null;

	/// <summary>
	/// Constructor.  This builds the Model for displaying reservoir data </summary>
	/// <param name="data"> the StateMod_Well data that will be displayed in the table. </param>
	public StateMod_Well_Collection_Data_TableModel(System.Collections.IList data) : this(data, false)
	{
	}

	/// <summary>
	/// Constructor.  This builds the Model for displaying reservoir data </summary>
	/// <param name="data"> the data that will be displayed in the table. </param>
	/// <param name="editable"> whether the data are editable or not. </param>
	public StateMod_Well_Collection_Data_TableModel(System.Collections.IList data, bool editable)
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
	/// From AbstractTableModel.  Returns the class of the data stored in a given column. </summary>
	/// <param name="columnIndex"> the column for which to return the data class. </param>
	public virtual Type getColumnClass(int columnIndex)
	{
		switch (columnIndex)
		{
			case __COL_ID:
				return typeof(string);
			case __COL_YEAR:
				return typeof(Integer);
			case __COL_COL_TYPE:
				return typeof(string);
			case __COL_PART_TYPE:
				return typeof(string);
			case __COL_PART_ID:
				return typeof(string);
			case __COL_PART_ID_TYPE:
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
				return "WELL\nID";
			case __COL_YEAR:
				return "\nYEAR";
			case __COL_COL_TYPE:
				return "COLLECTION\nTYPE";
			case __COL_PART_TYPE:
				return "PART\nTYPE";
			case __COL_PART_ID:
				return "PART\nID";
			case __COL_PART_ID_TYPE:
				return "PART\nID TYPE";
		}

		return " ";
	}

	/// <summary>
	/// Returns the text to be assigned to worksheet tooltips. </summary>
	/// <returns> a String array of tool tips. </returns>
	public virtual string[] getColumnToolTips()
	{
		string[] tips = new string[__COLUMNS];

		tips[__COL_ID] = "StateMod well ID for aggregate/system";
		tips[__COL_YEAR] = "Year for aggregate/system (used when aggregating parcels)";
		tips[__COL_COL_TYPE] = "Aggregate (aggregate water rights) or system (consider water rights individually)";
		tips[__COL_PART_TYPE] = "Ditch, Well, or Parcel identifiers are specified as parts of aggregate/system";
		tips[__COL_PART_ID] = "The identifier for the aggregate/system parts";
		tips[__COL_PART_ID_TYPE] = "The identifier type for the aggregate/system, WDID or Receipt when applied to wells";
		return tips;
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
			case __COL_YEAR:
				return "%8d";
			case __COL_COL_TYPE:
				return "%-12.12s";
			case __COL_PART_TYPE:
				return "%-12.12s";
			case __COL_PART_ID:
				return "%-12.12s";
			case __COL_PART_ID_TYPE:
				return "%-7.7s";
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
	/// From AbstractTableMode.  Returns the data that should be placed in the JTable at the given row and column. </summary>
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
		widths[__COL_YEAR] = 5;
		widths[__COL_COL_TYPE] = 9;
		widths[__COL_PART_TYPE] = 5;
		widths[__COL_PART_ID] = 6;
		widths[__COL_PART_ID_TYPE] = 8;
		return widths;
	}

	/// <summary>
	/// Returns whether the cell is editable or not.  In this model, all the cells in
	/// columns 3 and greater are editable. </summary>
	/// <param name="rowIndex"> unused. </param>
	/// <param name="columnIndex"> the index of the column to check whether it is editable. </param>
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
	/// Sets up the data lists to display the reservoir collection data in the GUI.
	/// </summary>
	private void setupData()
	{
		int[] years = null;
		int yearArrayLen = 0;
		int nwell = _data.size();
		int nParts = 0;
		int nIdTypes = 0;
		StateMod_Well well = null;
		string colType = null;
		string id = null;
		string partType = null;
		System.Collections.IList ids = null;
		System.Collections.IList idTypes = null;
		string idType = null;
		__data = new System.Collections.IList[__COLUMNS];
		for (int i = 0; i < __COLUMNS; i++)
		{
			__data[i] = new List<object>();
		}

		int rows = 0;

		for (int i = 0; i < nwell; i++)
		{
			well = (StateMod_Well)_data.get(i);
			id = well.getID();

			years = well.getCollectionYears();
			colType = well.getCollectionType();
			partType = well.getCollectionPartType();

			if (years == null)
			{
				yearArrayLen = 1; // Cause the loop below to go through once
			}
			else
			{
				yearArrayLen = years.Length;
			}

			for (int j = 0; j < yearArrayLen; j++)
			{
				// Part IDs for the year
				if ((years == null) || (years.Length == 0))
				{
					ids = well.getCollectionPartIDs(0);
				}
				else
				{
					ids = well.getCollectionPartIDs(years[j]);
				}
				if (ids == null)
				{
					nParts = 0;
				}
				else
				{
					nParts = ids.Count;
				}
				// Part ID types for the year.
				idTypes = well.getCollectionPartIDTypes();
				if (idTypes == null)
				{
					nIdTypes = 0;
				}
				else
				{
					nIdTypes = idTypes.Count;
				}

				for (int k = 0; k < nParts; k++)
				{
					__data[__COL_ID].Add(id);
					__data[__COL_YEAR].Add(new int?(years[j]));
					__data[__COL_COL_TYPE].Add(colType);
					__data[__COL_PART_TYPE].Add(partType);
					__data[__COL_PART_ID].Add(ids[k]);
					idType = "";
					if (nIdTypes != 0)
					{
						idType = (string)idTypes[k]; // Should align with ids.get(k)
					}
					__data[__COL_PART_ID_TYPE].Add(idType);
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