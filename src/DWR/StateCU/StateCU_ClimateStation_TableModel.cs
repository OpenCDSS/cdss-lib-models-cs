using System;

// StateCU_ClimateStation_TableModel - table model to display climate station data

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

namespace DWR.StateCU
{

	using JWorksheet_AbstractRowTableModel = RTi.Util.GUI.JWorksheet_AbstractRowTableModel;
	using Validator = RTi.Util.IO.Validator;

	/// <summary>
	/// This class is a table model for displaying StateCU climate station data.
	/// </summary>
	public class StateCU_ClimateStation_TableModel : JWorksheet_AbstractRowTableModel, StateCU_Data_TableModel
	{

	/// <summary>
	/// Number of columns in the table model.
	/// </summary>
	private int __COLUMNS = 8;

	/// <summary>
	/// References to the columns.
	/// </summary>
	private readonly int __COL_ID = 0, __COL_NAME = 1, __COL_LATITUDE = 2, __COL_ELEVATION = 3, __COL_REGION1 = 4, __COL_REGION2 = 5, __COL_ZH = 6, __COL_ZM = 7;

	/// <summary>
	/// Whether the data are editable or not.
	/// </summary>
	private bool __editable = false;

	/// <summary>
	/// The parent climate station for which subdata is displayed.
	/// </summary>
	// TODO SAM 2007-03-01 Evaluate use
	//private StateCU_ClimateStation __parentStation;

	/// <summary>
	/// Constructor.  This builds the Model for displaying climate station data </summary>
	/// <param name="data"> the data that will be displayed in the table. </param>
	/// <exception cref="Exception"> if invalid data or dmi was passed in. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public StateCU_ClimateStation_TableModel(java.util.List data) throws Exception
	public StateCU_ClimateStation_TableModel(System.Collections.IList data) : this(data, true)
	{
	}

	/// <summary>
	/// Constructor.  This builds the Model for displaying climate station data </summary>
	/// <param name="data"> the data that will be displayed in the table. </param>
	/// <param name="editable"> whether the data are editable or not. </param>
	/// <exception cref="Exception"> if invalid data or dmi was passed in. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public StateCU_ClimateStation_TableModel(java.util.List data, boolean editable) throws Exception
	public StateCU_ClimateStation_TableModel(System.Collections.IList data, bool editable)
	{
		if (data == null)
		{
			throw new Exception("Invalid data list passed to " + "StateCU_ClimateStation_TableModel constructor.");
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
			case __COL_ID:
				return typeof(string);
			case __COL_NAME:
				return typeof(string);
			case __COL_ELEVATION:
				return typeof(Double);
			case __COL_LATITUDE:
				return typeof(Double);
			case __COL_REGION1:
				return typeof(string);
			case __COL_REGION2:
				return typeof(string);
			case __COL_ZH:
				return typeof(Double);
			case __COL_ZM:
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
			case __COL_ID:
				return "\nID";
			case __COL_NAME:
				return "\nNAME";
			case __COL_ELEVATION:
				return "ELEVATION\n(FT)";
			case __COL_LATITUDE:
				return "LATITUDE\n(DEC. DEG.)";
			case __COL_REGION1:
				return "\nREGION1";
			case __COL_REGION2:
				return "\nREGION2";
			case __COL_ZH:
				return "HEIGHT HUMID.&TEMP.\nMEASUREMENTS (FT)";
			case __COL_ZM:
				return "HEIGHT WIND\nMEASUREMENT (FT)";
			default:
				return " ";
		}
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
				return "%-20.20s";
			case __COL_NAME:
				return "%-20.20s";
			case __COL_ELEVATION:
				return "%10.2f";
			case __COL_LATITUDE:
				return "%10.2f";
			case __COL_REGION1:
				return "%-20.20s";
			case __COL_REGION2:
				return "%-20.20s";
			case __COL_ZH:
				return "%8.2f";
			case __COL_ZM:
				return "%8.2f";
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
		Validator[] no_checks = new Validator[] {};

		switch (col)
		{
			case __COL_ID:
				return StateCU_Data_TableModel_Fields.ids;
			case __COL_NAME:
				return StateCU_Data_TableModel_Fields.blank;
			case __COL_ELEVATION:
				return StateCU_Data_TableModel_Fields.nums;
			case __COL_LATITUDE:
				return StateCU_Data_TableModel_Fields.nums;
			case __COL_REGION1:
				return StateCU_Data_TableModel_Fields.blank;
			case __COL_REGION2:
				return StateCU_Data_TableModel_Fields.blank;
			default:
				return no_checks;
		}
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

		StateCU_ClimateStation station = (StateCU_ClimateStation)_data.get(row);
		switch (col)
		{
			case __COL_ID:
				return station.getID();
			case __COL_NAME:
				return station.getName();
			case __COL_ELEVATION:
				return new double?(station.getElevation());
			case __COL_LATITUDE:
				return new double?(station.getLatitude());
			case __COL_REGION1:
				return station.getRegion1();
			case __COL_REGION2:
				return station.getRegion2();
			case __COL_ZH:
				if (StateCU_Util.isMissing(station.getZh()))
				{
					return null;
				}
				else
				{
					return new double?(station.getZh());
				}
			case __COL_ZM:
				if (StateCU_Util.isMissing(station.getZm()))
				{
					return null;
				}
				else
				{
					return new double?(station.getZm());
				}
		}

		return "";
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
		widths[__COL_ID] = 6;
		widths[__COL_NAME] = 24;
		widths[__COL_ELEVATION] = 7;
		widths[__COL_LATITUDE] = 7;
		widths[__COL_REGION1] = 9;
		widths[__COL_REGION2] = 6;
		widths[__COL_ZH] = 15;
		widths[__COL_ZM] = 15;
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

		return true;
	}

	/// <summary>
	/// Inserts the specified value into the table at the given position. </summary>
	/// <param name="value"> the object to store in the table cell. </param>
	/// <param name="row"> the row of the cell in which to place the object. </param>
	/// <param name="col"> the column of the cell in which to place the object. </param>
	public virtual void setValueAt(object value, int row, int col)
	{
		StateCU_ClimateStation station = (StateCU_ClimateStation)_data.get(row);
		switch (col)
		{
			case __COL_ID:
				station.setID((string)value);
				break;
			case __COL_NAME:
				station.setName((string)value);
				break;
			case __COL_ELEVATION:
				station.setElevation(((double?)value).Value);
				break;
			case __COL_LATITUDE:
				station.setLatitude(((double?)value).Value);
				break;
			case __COL_REGION1:
				station.setRegion1((string)value);
				break;
			case __COL_REGION2:
				station.setRegion2((string)value);
				break;
			case __COL_ZH:
				station.setZh(((double?)value).Value);
				break;
			case __COL_ZM:
				station.setZm(((double?)value).Value);
				break;
		}

		base.setValueAt(value, row, col);
	}

	/// <summary>
	/// Sets the parent well under which the right and return flow data is stored. </summary>
	/// <param name="parent"> the parent well. </param>
	public virtual void setParentClimateStation(StateCU_ClimateStation parent)
	{
		// TODO SAM 2007-03-01 Evaluate use
		//__parentStation = parent;
	}

	}

}