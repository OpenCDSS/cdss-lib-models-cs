using System;

// StateCU_Location_TableModel - Table model for displaying data for location tables

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
// StateCU_Location_TableModel - Table model for displaying data for 
//	location tables
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
// 2003-07-14	J. Thomas Sapienza, RTi	Initial version.
// 2003-07-22	JTS, RTi		Revised following SAM's review.
// 2004-02-28	Steven A. Malers, RTi	Moved some utility methods from
//					StateCU_Data to StateCU_Util.
// 2005-01-21	JTS, RTi		Added the editable flag.
// 2005-01-24	JTS, RTi		Added the ability to display multiple
//					locations in a single table model.
// 2005-03-28	JTS, RTi		Adjusted column sizes.
// 2005-03-29	JTS, RTi		* Removed the Collection Division 
//					  column.
//					* Adjusted the column order.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace DWR.StateCU
{

	using StateMod_DiversionRight = DWR.StateMod.StateMod_DiversionRight;
	using JWorksheet_AbstractRowTableModel = RTi.Util.GUI.JWorksheet_AbstractRowTableModel;
	using Validator = RTi.Util.IO.Validator;

	/// <summary>
	/// This class is a table model for displaying location data.
	/// </summary>
	public class StateCU_Location_TableModel : JWorksheet_AbstractRowTableModel, StateCU_Data_TableModel
	{

	/// <summary>
	/// Number of columns in the table model.
	/// </summary>
	private int __COLUMNS = 14;

	/// <summary>
	/// Column references.
	/// </summary>
	private readonly int __COL_ID = 0, __COL_NAME = 1, __COL_LATITUDE = 2, __COL_ELEVATION = 3, __COL_REGION1 = 4, __COL_REGION2 = 5, __COL_NUM_STA = 6, __COL_AWC = 7;

	/// <summary>
	/// Whether the data are editable or not.
	/// </summary>
	private bool __editable = true;

	/// <summary>
	/// Whether a single location is being shown in the table model (true) or many
	/// locations are being shown (false).
	/// </summary>
	private bool __singleLocation = true;

	/// <summary>
	/// The parent location for which subdata is displayed.
	/// </summary>
	private StateCU_Location __parentLocation;

	private StateCU_DelayTableAssignment __delays;

	private System.Collections.IList __stations;

	private System.Collections.IList __rights;

	/// <summary>
	/// Constructor.  This builds the Model for displaying location data </summary>
	/// <param name="data"> the data that will be displayed in the table. </param>
	/// <exception cref="Exception"> if an invalid data or dmi was passed in. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public StateCU_Location_TableModel(java.util.List data) throws Exception
	public StateCU_Location_TableModel(System.Collections.IList data) : this(data, true, true)
	{
	}

	/// <summary>
	/// Constructor.  This builds the Model for displaying location data </summary>
	/// <param name="data"> the data that will be displayed in the table. </param>
	/// <param name="editable"> whether the data are editable or not. </param>
	/// <param name="singleLocation"> whether a single location (true) or many locations (false)
	/// are being shown in the table model. </param>
	/// <exception cref="Exception"> if an invalid data or dmi was passed in. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public StateCU_Location_TableModel(java.util.List data, boolean editable, boolean singleLocation) throws Exception
	public StateCU_Location_TableModel(System.Collections.IList data, bool editable, bool singleLocation)
	{
		if (data == null)
		{
			throw new Exception("Invalid data list passed to StateCU_Location_TableModel constructor.");
		}
		_rows = data.Count;
		_data = data;
		__editable = editable;
		__singleLocation = singleLocation;

		if (__singleLocation)
		{
			__COLUMNS = 14;
		}
		else
		{
			__COLUMNS = 8;
		}
	}

	/// <summary>
	/// From AbstractTableModel.  Returns the class of the data stored in a given column. </summary>
	/// <param name="columnIndex"> the column for which to return the data class. </param>
	public virtual Type getColumnClass(int columnIndex)
	{
		if (__singleLocation)
		{
			switch (columnIndex)
			{
				case 0:
					return typeof(Integer); // row #
				case 1:
					return typeof(string); // id
				case 2:
					return typeof(string); // name
				case 3:
					return typeof(Double); // pct return
				case 4:
					return typeof(string); // pattern no
				case 5:
					return typeof(string); // id
				case 6:
					return typeof(string); // name
				case 7:
					return typeof(Double); // precip wt
				case 8:
					return typeof(Double); // temp wt
				case 9:
					return typeof(string); // id
				case 10:
					return typeof(string); // name
				case 11:
					return typeof(Double); // irtem
				case 12:
					return typeof(Double); // dcrdiv
				case 13:
					return typeof(Integer); // switch
			}
		}
		else
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
				case __COL_NUM_STA:
					return typeof(Integer);
				case __COL_AWC:
					return typeof(Double);
			}
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
		if (__singleLocation)
		{
			switch (columnIndex)
			{
				case 0:
					return " ";
				case 1:
					return "ID";
				case 2:
					return "NAME";
				case 3:
					return "% RETURN";
				case 4:
					return "DELAY TABLE ID";
				case 5:
					return "STATION ID";
				case 6:
					return "STATION NAME";
				case 7:
					return "PRECIP WT";
				case 8:
					return "TEMP WT";
				case 9:
					return "RIGHT ID";
				case 10:
					return "RIGHT NAME";
				case 11:
					return "IRTEM";
				case 12:
					return "DCRDIV";
				case 13:
					return "SWITCH";
			}
		}
		else
		{
			switch (columnIndex)
			{
				case __COL_ID:
					return "\n\n\n\nID";
				case __COL_NAME:
					return "\n\n\n\nNAME";
				case __COL_ELEVATION:
					return "\n\n\nELEVATION\n(FT)";
				case __COL_LATITUDE:
					return "\n\n\nLATITUDE\n(DEC. DEG.)";
				case __COL_REGION1:
					return "\n\n\n\nREGION1";
				case __COL_REGION2:
					return "\n\n\n\nREGION2";
				case __COL_NUM_STA:
					return "\n\nNUMBER OF\nCLIMATE\nSTATIONS";
				case __COL_AWC:
					return "AVAILABLE\nWATER\nCONTENT\nAWC,"
						+ "\n(FRACTION)";
					break;
			}
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
		if (__singleLocation)
		{
			switch (column)
			{
				case 0:
					return "%8d"; // row #
				case 1:
					return "%-20.20s"; // id
				case 2:
					return "%-20.20s"; // name
				case 3:
					return "%8.2f"; // pct return
				case 4:
					return "%-20.20s"; // pattern no
				case 5:
					return "%-20.20s"; // station id
				case 6:
					return "%-20.20s"; // station name
				case 7:
					return "%8.2f"; // precip wt
				case 8:
					return "%8.2f"; // temp wt
				case 9:
					return "%-20.20s"; // id
				case 10:
					return "%-20.20s"; // name
				case 11:
					return "%12.6f"; // irtem
				case 12:
					return "%8.1f"; // dcrdiv
				case 13:
					return "%8d"; // switch
				default:
					return "%-8s";
			}
		}
		else
		{
			switch (column)
			{
				case __COL_ID:
					return "%-20.20s";
				case __COL_NAME:
					return "%-20.20s";
				case __COL_ELEVATION:
					return "%8.2f";
				case __COL_LATITUDE:
					return "%8.2f";
				case __COL_REGION1:
					return "%-20.20s";
				case __COL_REGION2:
					return "%-20.20s";
				case __COL_NUM_STA:
					return "%8d";
				case __COL_AWC:
					return "%8.4f";
			}
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

		if (__singleLocation)
		{
			switch (col)
			{
				case 1:
					return StateCU_Data_TableModel_Fields.ids;
				case 2:
					return StateCU_Data_TableModel_Fields.blank;
				//case 3:		return nums;
				//case 4:		return ids;
				case 5:
					return StateCU_Data_TableModel_Fields.ids;
				case 6:
					return StateCU_Data_TableModel_Fields.blank;
				case 7:
					return StateCU_Data_TableModel_Fields.nums;
				case 8:
					return StateCU_Data_TableModel_Fields.nums;
				case 13:
					return StateCU_Data_TableModel_Fields.blank;
				default:
					return no_checks;
			}
		}
		else
		{
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
				case __COL_NUM_STA:
					return StateCU_Data_TableModel_Fields.nums;
				case __COL_AWC:
					return StateCU_Data_TableModel_Fields.nums;
				default:
					return no_checks;
			}
		}
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

		if (__singleLocation)
		{
			switch (col)
			{
				case 1:
				case 2:
					StateCU_Location location = (StateCU_Location)_data.get(row);
					switch (col)
					{
						case 1:
							return location.getID();
						case 2:
							return location.getName();
					}
				case 3:
					return new double?(__delays.getDelayTablePercent(row));
				case 4:
					return __delays.getDelayTableID(row);
				case 5:
					return __parentLocation.getClimateStationID(row);
				case 6:
					int index = StateCU_Util.IndexOf(__stations, __parentLocation.getClimateStationID(row));
					if (index == -1)
					{
						return "N/A";
					}
					return ((StateCU_ClimateStation)__stations[row]).getName();
				case 7:
					return new double?(__parentLocation.getPrecipitationStationWeight(row));
				case 8:
					return new double?(__parentLocation.getTemperatureStationWeight(row));
				case 9:
				case 10:
				case 11:
				case 12:
				case 13:
					StateMod_DiversionRight right = (StateMod_DiversionRight)__rights[row];
					switch (col)
					{
						case 9:
							return right.getID();
						case 10:
							return right.getName();
						case 11:
							return Convert.ToDouble(right.getIrtem());
						case 12:
							return new double?(right.getDcrdiv());
						case 13:
							return new int?(right.getSwitch());
					}
				default:
					return "";
			}
		}
		else
		{
			StateCU_Location location = (StateCU_Location)_data.get(row);
			switch (col)
			{
				case __COL_ID:
					return location.getID();
				case __COL_NAME:
					return location.getName();
				case __COL_ELEVATION:
					return new double?(location.getElevation());
				case __COL_LATITUDE:
					return new double?(location.getLatitude());
				case __COL_REGION1:
					return location.getRegion1();
				case __COL_REGION2:
					return location.getRegion2();
				case __COL_NUM_STA:
					return new int?(location.getNumClimateStations());
				case __COL_AWC:
					return new double?(location.getAwc());
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
		int i = 0;
		if (__singleLocation)
		{
			widths[i++] = 5; // row #
			widths[i++] = 12; // id
			widths[i++] = 20; // name
			widths[i++] = 12; // % return
			widths[i++] = 20; // pattern id
			widths[i++] = 12; // id
			widths[i++] = 20; // name
			widths[i++] = 12; // precip wt
			widths[i++] = 12; // temp wt
			widths[i++] = 15; // right id
			widths[i++] = 24; // right name
			widths[i++] = 13; // irtem
			widths[i++] = 8; // dcrdiv
			widths[i++] = 8; // switch
		}
		else
		{
			widths[__COL_ID] = 5;
			widths[__COL_NAME] = 20;
			widths[__COL_ELEVATION] = 8;
			widths[__COL_LATITUDE] = 7;
			widths[__COL_REGION1] = 10;
			widths[__COL_REGION2] = 6;
			widths[__COL_NUM_STA] = 8;
			widths[__COL_AWC] = 8;
		}
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

		if (columnIndex > 2)
		{
			return false;
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

		switch (col)
		{
		}

		base.setValueAt(value, row, col);
	}

	public virtual void setDelay(StateCU_Location location, StateCU_DelayTableAssignment dta)
	{
		__parentLocation = location;
		__delays = dta;
		if (dta == null)
		{
			_rows = 0;
		}
		else
		{
			_rows = dta.getNumDelayTables();
		}
		fireTableDataChanged();
	}

	public virtual void setStations(StateCU_Location location, System.Collections.IList stations)
	{
		__parentLocation = location;
		__stations = stations;

		_rows = location.getNumClimateStations();
		fireTableDataChanged();
	}

	public virtual void setRights(StateCU_Location location, System.Collections.IList rights)
	{
		__parentLocation = location;
		__rights = rights;

		_rows = __rights.Count;
		fireTableDataChanged();
	}

	}

}