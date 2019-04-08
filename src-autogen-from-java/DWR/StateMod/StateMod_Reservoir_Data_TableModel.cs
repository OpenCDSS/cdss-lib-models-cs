using System;
using System.Collections.Generic;

// StateMod_Reservoir_Data_TableModel - table model for displaying reservoir station data

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
// StateMod_Reservoir_Data_TableModel - table model for displaying reservoir 
//	station data
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
//
// 2005-04-04	J. Thomas Sapienza, RTi	Initial version.
// 2007-04-27	Kurt Tometich, RTi		Added getValidators method for check
//									file and data check implementation.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace DWR.StateMod
{

	using JWorksheet_AbstractRowTableModel = RTi.Util.GUI.JWorksheet_AbstractRowTableModel;
	using Validator = RTi.Util.IO.Validator;
	using Validators = RTi.Util.IO.Validators;

	/// <summary>
	/// This table model displays reservoir station data.
	/// </summary>
	public class StateMod_Reservoir_Data_TableModel : JWorksheet_AbstractRowTableModel, StateMod_Data_TableModel
	{

	/// <summary>
	/// Number of columns in the table model.
	/// </summary>
	private int __COLUMNS = 14;

	/// <summary>
	/// References to columns.
	/// </summary>
	public const int COL_ID = 0, COL_NAME = 1, COL_NODE_ID = 2, COL_SWITCH = 3, COL_ONE_FILL_DATE = 4, COL_MIN_CONTENT = 5, COL_MAX_CONTENT = 6, COL_MAX_RELEASE = 7, COL_DEAD_STORAGE = 8, COL_DAILY_ID = 9, COL_NUM_OWNERS = 10, COL_NUM_EVAP_STA = 11, COL_NUM_PRECIP_STA = 12, COL_NUM_CURVE_ROWS = 13;

	/// <summary>
	/// Whether the table data is editable or not.
	/// </summary>
	private bool __editable = true;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="data"> the data that will be displayed in the table. </param>
	/// <param name="editable"> whether the table data can be modified or not. </param>
	public StateMod_Reservoir_Data_TableModel(System.Collections.IList data, bool editable)
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
	/// <param name="columnIndex"> the column for which to return the data class. </param>
	public virtual Type getColumnClass(int columnIndex)
	{
		switch (columnIndex)
		{
			case COL_ID:
				return typeof(string);
			case COL_NAME:
				return typeof(string);
			case COL_NODE_ID:
				return typeof(string);
			case COL_SWITCH:
				return typeof(Integer);
			case COL_ONE_FILL_DATE:
				return typeof(Integer);
			case COL_MIN_CONTENT:
				return typeof(Double);
			case COL_MAX_CONTENT:
				return typeof(Double);
			case COL_MAX_RELEASE:
				return typeof(Double);
			case COL_DEAD_STORAGE:
				return typeof(Double);
			case COL_DAILY_ID:
				return typeof(string);
			case COL_NUM_OWNERS:
				return typeof(Integer);
			case COL_NUM_PRECIP_STA:
				return typeof(Integer);
			case COL_NUM_EVAP_STA:
				return typeof(Integer);
			case COL_NUM_CURVE_ROWS:
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
	/// <param name="columnIndex"> the position of the column for which to return the name. </param>
	/// <returns> the name of the column at the given position. </returns>
	public virtual string getColumnName(int columnIndex)
	{
		switch (columnIndex)
		{
			case COL_ID:
				return "\n\nID";
			case COL_NAME:
				return "\n\nNAME";
			case COL_NODE_ID:
				return "\nRIVER\nNODE ID";
			case COL_SWITCH:
				return "\nON/OFF\nSWITCH";
			case COL_ONE_FILL_DATE:
				return "ONE\nFILL\nDATE";
			case COL_MIN_CONTENT:
				return "MIN\nCONTENT\n(ACFT)";
			case COL_MAX_CONTENT:
				return "MAX\nCONTENT\n(ACFT)";
			case COL_MAX_RELEASE:
				return "MAX\nRELEASE\n(CFS)";
			case COL_DEAD_STORAGE:
				return "DEAD\nSTORAGE\n(ACFT)";
			case COL_DAILY_ID:
				return "\nDAILY\nID";
			case COL_NUM_OWNERS:
				return "NUMBER\nOF\nOWNERS";
			case COL_NUM_PRECIP_STA:
				return "NUMBER\nOF PRECIP.\nSTATIONS";
			case COL_NUM_EVAP_STA:
				return "NUMBER\nOF EVAP.\nSTATIONS";
			case COL_NUM_CURVE_ROWS:
				return "NUMBER\nOF CURVE\nROWS";
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

		tips[COL_ID] = "<html>The reservoir station identifier is the main link" +
			" between reservoir data<BR>" +
			"and must be unique in the data set.</html>";
		tips[COL_NAME] = "Reservoir station name.";

		tips[COL_NODE_ID] = "Node where reservoir is located.";
		tips[COL_SWITCH] = "<html>Switch.<br>0 = off<br>1 = on</html>";

		tips[COL_ONE_FILL_DATE] = "<html>Date for one fill rule admin.</html>";
		tips[COL_MIN_CONTENT] = "<html>Minimum reservoir content (ACFT).</html>";
		tips[COL_MAX_CONTENT] = "<html>Maximum reservoir content (ACFT).</html>";
		tips[COL_MAX_RELEASE] = "<html>Maximum release (CFS).</html>";
		tips[COL_DEAD_STORAGE] = "<html>Dead storage in reservoir (ACFT).</html>";
		tips[COL_DAILY_ID] = "Identifier for daily time series.";
		tips[COL_NUM_OWNERS] = "Number of owners.";
		tips[COL_NUM_PRECIP_STA] = "Number of precipitation stations.";
		tips[COL_NUM_EVAP_STA] = "Number of evaporation stations.";
		tips[COL_NUM_CURVE_ROWS] = "Number of curve rows.";

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
		widths[COL_NAME] = 23;

		widths[COL_NODE_ID] = 8;
		widths[COL_SWITCH] = 5;
		widths[COL_ONE_FILL_DATE] = 4;
		widths[COL_MIN_CONTENT] = 8;
		widths[COL_MAX_CONTENT] = 8;
		widths[COL_MAX_RELEASE] = 8;
		widths[COL_DEAD_STORAGE] = 8;
		widths[COL_DAILY_ID] = 5;
		widths[COL_NUM_OWNERS] = 8;
		widths[COL_NUM_PRECIP_STA] = 8;
		widths[COL_NUM_EVAP_STA] = 8;
		widths[COL_NUM_CURVE_ROWS] = 8;

		return widths;
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
				return "%-12.12s";
			case COL_NAME:
				return "%-24.24s";
			case COL_NODE_ID:
				return "%-12.12s";
			case COL_SWITCH:
				return "%8d";
			case COL_ONE_FILL_DATE:
				return "%8d";
			case COL_MIN_CONTENT:
				return "%10.2f";
			case COL_MAX_CONTENT:
				return "%10.2f";
			case COL_MAX_RELEASE:
				return "%10.2f";
			case COL_DEAD_STORAGE:
				return "%10.2f";
			case COL_DAILY_ID:
				return "%-12.12s";
			case COL_NUM_OWNERS:
				return "%8d";
			case COL_NUM_PRECIP_STA:
				return "%8d";
			case COL_NUM_EVAP_STA:
				return "%8d";
			case COL_NUM_CURVE_ROWS:
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
	/// Returns general validators based on column of data being checked. </summary>
	/// <param name="col"> Column of data to check. </param>
	/// <returns> List of validators for a column of data. </returns>
	public virtual Validator[] getValidators(int col)
	{
		Validator[] no_checks = new Validator[] {};
		// Switch must be 0, 1, 2 or 3.
		Validator[] data_type = new Validator[] {Validators.isEquals(new int?(0)), Validators.isEquals(new int?(1)), Validators.isEquals(new int?(2)), Validators.isEquals(new int?(3))};
		Validator[] switch_Validators = new Validator[] {Validators.or(data_type)};
		Validator[] rdate = new Validator[] {Validators.notBlankValidator(), Validators.rangeValidator(-2, 13)};

		switch (col)
		{
		case COL_ID:
			return StateMod_Data_TableModel_Fields.ids;
		case COL_NAME:
			return StateMod_Data_TableModel_Fields.blank;
		case COL_NODE_ID:
			return StateMod_Data_TableModel_Fields.ids;
		case COL_SWITCH:
			return switch_Validators;
		case COL_ONE_FILL_DATE:
			return rdate;
		case COL_MIN_CONTENT:
			return StateMod_Data_TableModel_Fields.nums;
		case COL_MAX_CONTENT:
			return StateMod_Data_TableModel_Fields.nums;
		case COL_MAX_RELEASE:
			return StateMod_Data_TableModel_Fields.nums;
		case COL_DEAD_STORAGE:
			return StateMod_Data_TableModel_Fields.nums;
		case COL_DAILY_ID:
			return StateMod_Data_TableModel_Fields.ids;
		case COL_NUM_OWNERS:
			return StateMod_Data_TableModel_Fields.nums;
		case COL_NUM_PRECIP_STA:
			return StateMod_Data_TableModel_Fields.nums;
		case COL_NUM_EVAP_STA:
			return StateMod_Data_TableModel_Fields.nums;
		case COL_NUM_CURVE_ROWS:
			return StateMod_Data_TableModel_Fields.nums;
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

		StateMod_Reservoir r = (StateMod_Reservoir)_data.get(row);
		switch (col)
		{
			case COL_ID:
				return r.getID();
			case COL_NAME:
				return r.getName();
			case COL_NODE_ID:
				return r.getCgoto();
			case COL_SWITCH:
				return new int?(r.getSwitch());
			case COL_ONE_FILL_DATE:
				return new int?((int)r.getRdate());
			case COL_MIN_CONTENT:
				return new double?(r.getVolmin());
			case COL_MAX_CONTENT:
				return new double?(r.getVolmax());
			case COL_MAX_RELEASE:
				return new double?(r.getFlomax());
			case COL_DEAD_STORAGE:
				return new double?(r.getDeadst());
			case COL_DAILY_ID:
				return r.getCresdy();
			case COL_NUM_OWNERS:
				return new int?(r.getNowner());
			case COL_NUM_PRECIP_STA:
				int nptpx = StateMod_ReservoirClimate.getNumPrecip(r.getClimates());
				return new int?(nptpx);
			case COL_NUM_EVAP_STA:
				int nevap = StateMod_ReservoirClimate.getNumEvap(r.getClimates());
				return new int?(nevap);
			case COL_NUM_CURVE_ROWS:
				System.Collections.IList v = r.getAreaCaps();
				if (v == null)
				{
					return new int?(0);
				}
				else
				{
					return new int?(v.Count);
				}
			default:
				return "";
		}
	}

	/// <summary>
	/// Returns whether the cell at the given position is editable or not.  Currently no columns are editable. </summary>
	/// <param name="rowIndex"> unused </param>
	/// <param name="columnIndex"> the index of the column to check for whether it is editable. </param>
	/// <returns> whether the cell at the given position is editable. </returns>
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
		int ival;
		StateMod_Reservoir smr = (StateMod_Reservoir)_data.get(row);
		switch (col)
		{
			case COL_ID:
				smr.setID((string)value);
				break;
			case COL_NAME:
				smr.setName((string)value);
				break;
			case COL_NODE_ID:
				smr.setCgoto((string)value);
				break;
			case COL_SWITCH:
				if (value is int?)
				{
					ival = ((int?)value).Value;
					smr.setSwitch(ival);
				}
				else if (value is string)
				{
					string onOff = (string)value;
					int index = onOff.IndexOf(" -", StringComparison.Ordinal);
					ival = (Convert.ToInt32(onOff.Substring(0, index)));
					smr.setSwitch(ival);
				}
				break;
			case COL_ONE_FILL_DATE:
				smr.setRdate(((int?)value).Value);
				break;
			case COL_MIN_CONTENT:
				smr.setVolmin(((double?)value).Value);
				break;
			case COL_MAX_CONTENT:
				smr.setVolmax(((double?)value).Value);
				break;
			case COL_MAX_RELEASE:
				smr.setFlomax(((double?)value).Value);
				break;
			case COL_DEAD_STORAGE:
				smr.setDeadst(((double?)value).Value);
				break;
			case COL_DAILY_ID:
				smr.setCresdy((string)value);
				break;
		}
	}

	}

}