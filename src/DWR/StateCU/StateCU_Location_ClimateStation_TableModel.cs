using System;
using System.Collections.Generic;

// StateCU_Location_ClimateStation_TableModel - Table model for displaying location climate station data.

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
// StateCU_Location_ClimateStation_TableModel - Table model for displaying 
//	location climate station data.
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
//
// 2005-03-29	J. Thomas Sapienza, RTi	Initial version.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace DWR.StateCU
{

	using JWorksheet_AbstractRowTableModel = RTi.Util.GUI.JWorksheet_AbstractRowTableModel;
	using Validator = RTi.Util.IO.Validator;

	/// <summary>
	/// This class is a table model for displaying location climate station data.
	/// </summary>
	public class StateCU_Location_ClimateStation_TableModel : JWorksheet_AbstractRowTableModel, StateCU_Data_TableModel
	{

	/// <summary>
	/// Number of columns in the table model.
	/// </summary>
	private int __COLUMNS = 6;

	/// <summary>
	/// Column references.
	/// </summary>
	private readonly int __COL_ID = 0, __COL_STA_ID = 1, __COL_TEMP_WT = 2, __COL_PRECIP_WT = 3, __COL_ORO_TEMP_ADJ = 4, __COL_ORO_PRECIP_ADJ = 5;

	/// <summary>
	/// Whether the data are editable or not.
	/// </summary>
	private bool __editable = false;

	/// <summary>
	/// The parent location for which subdata is displayed.
	/// </summary>
	// TODO SAM 2007-03-01 Evaluate use
	//private StateCU_Location __parentLocation;

	/// <summary>
	/// The data displayed in the table.
	/// </summary>
	private System.Collections.IList[] __data = null;

	/// <summary>
	/// Constructor.  This builds the Model for displaying location data </summary>
	/// <param name="data"> the data that will be displayed in the table. </param>
	public StateCU_Location_ClimateStation_TableModel(System.Collections.IList data) : this(data, false)
	{
	}

	/// <summary>
	/// Constructor.  This builds the Model for displaying location data </summary>
	/// <param name="data"> the data that will be displayed in the table. </param>
	/// <param name="editable"> whether the data are editable or not. </param>
	public StateCU_Location_ClimateStation_TableModel(System.Collections.IList data, bool editable)
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
			case __COL_STA_ID:
				return typeof(string);
			case __COL_TEMP_WT:
				return typeof(Double);
			case __COL_PRECIP_WT:
				return typeof(Double);
			case __COL_ORO_TEMP_ADJ:
				return typeof(Double);
			case __COL_ORO_PRECIP_ADJ:
				return typeof(Double);
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
				return "\nCU\nLOCATION\nID";
			case __COL_STA_ID:
				return "\nCLIMATE\nSTATION\nID";
			case __COL_TEMP_WT:
				return "TEMPERATURE\nSTATION\nWEIGHT\n(FRACTION)";
			case __COL_PRECIP_WT:
				return "PRECIPITATION\nSTATION\nWEIGHT\n(FRACTION)";
			case __COL_ORO_TEMP_ADJ:
				return "OROGRAPHIC\nTEMPERATURE\nADJUSTMENT\n(DEGF/1000 FT)";
			case __COL_ORO_PRECIP_ADJ:
				return "OROGRAPHIC\nPRECIPITATION\nADJUSTMENT\n(FRACTION)";
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
				return "%-20.20s";
			case __COL_STA_ID:
				return "%-20.20s";
			case __COL_TEMP_WT:
				return "%8.2f";
			case __COL_PRECIP_WT:
				return "%8.2f";
			case __COL_ORO_TEMP_ADJ:
				return "%8.2f";
			case __COL_ORO_PRECIP_ADJ:
				return "%8.2f";
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
	/// Returns general validators based on column of data being checked. </summary>
	/// <param name="col"> Column of data to check. </param>
	/// <returns> List of validators for a column of data. </returns>
	public virtual Validator[] getValidators(int col)
	{
		Validator[] no_checks = new Validator[] {};

		// TODO KAT 2007-04-12 Add checks here ...
		return no_checks;
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
		widths[__COL_ID] = 7;
		widths[__COL_STA_ID] = 6;
		widths[__COL_TEMP_WT] = 10;
		widths[__COL_PRECIP_WT] = 10;
		widths[__COL_ORO_TEMP_ADJ] = 10;
		widths[__COL_ORO_PRECIP_ADJ] = 10;
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

		return false;
	}

	/// <summary>
	/// Sets up the data Vectors to display the location climate station data in the GUI.
	/// </summary>
	private void setupData()
	{
		int num = 0;
		int size = _data.size();
		StateCU_Location l = null;
		string id = null;
		__data = new System.Collections.IList[__COLUMNS];
		for (int i = 0; i < __COLUMNS; i++)
		{
			__data[i] = new List<object>();
		}

		int rows = 0;

		for (int i = 0; i < size; i++)
		{
			l = (StateCU_Location)_data.get(i);
			id = l.getID();
			num = l.getNumClimateStations();

			for (int j = 0; j < num; j++)
			{
				__data[__COL_ID].Add(id);
				__data[__COL_STA_ID].Add(l.getClimateStationID(j));
				__data[__COL_TEMP_WT].Add(new double?(l.getTemperatureStationWeight(j)));
				__data[__COL_PRECIP_WT].Add(new double?(l.getPrecipitationStationWeight(j)));
				__data[__COL_ORO_TEMP_ADJ].Add(new double?(l.getOrographicTemperatureAdjustment(j)));
				__data[__COL_ORO_PRECIP_ADJ].Add(new double?(l.getOrographicPrecipitationAdjustment(j)));
				rows++;
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