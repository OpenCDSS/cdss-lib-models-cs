using System;
using System.Collections.Generic;

// StateMod_Plan_Data_TableModel - This table model display data in plan tables.

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
	using Validator = RTi.Util.IO.Validator;
	using Validators = RTi.Util.IO.Validators;

	/// <summary>
	/// This table model display data in plan tables.
	/// </summary>
	public class StateMod_Plan_Data_TableModel : JWorksheet_AbstractRowTableModel, StateMod_Data_TableModel
	{

	/// <summary>
	/// Number of columns in the table model (this includes all data - other code
	/// can hide columns if necessary).
	/// </summary>
	private int __COLUMNS = 23;

	/// <summary>
	/// References to columns.
	/// </summary>
	public const int COL_ID = 0, COL_NAME = 1, COL_RIVER_NODE_ID = 2, COL_ON_OFF = 3, COL_TYPE = 4, COL_RETURN_TYPE = 5, COL_FAILURE_SWITCH = 6, COL_INITIAL_STORAGE = 7, COL_SOURCE_ID = 8, COL_SOURCE_ACCOUNT = 9, COL_EFF_FLAG = 10, COL_EFF_01 = 11, COL_EFF_02 = 12, COL_EFF_03 = 13, COL_EFF_04 = 14, COL_EFF_05 = 15, COL_EFF_06 = 16, COL_EFF_07 = 17, COL_EFF_08 = 18, COL_EFF_09 = 19, COL_EFF_10 = 20, COL_EFF_11 = 21, COL_EFF_12 = 22;

	/// <summary>
	/// Whether the table data can be edited or not.
	/// </summary>
	private bool __editable = true;

	/// <summary>
	/// Constructor.  This builds the Model for displaying the diversion data. </summary>
	/// <param name="data"> the data that will be displayed in the table. </param>
	/// <param name="editable"> whether the data can be edited or not </param>
	public StateMod_Plan_Data_TableModel(System.Collections.IList data, bool editable)
	{
		if (data == null)
		{
			_data = new List<object>();
		}
		else
		{
			_data = data;
		}
		_rows = data.Count;

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
			case COL_RIVER_NODE_ID:
				return typeof(string);
			case COL_ON_OFF:
				return typeof(Integer);
			case COL_TYPE:
				return typeof(Integer);
			case COL_RETURN_TYPE:
				return typeof(Integer);
			case COL_FAILURE_SWITCH:
				return typeof(Integer);
			case COL_INITIAL_STORAGE:
				return typeof(Double);
			case COL_SOURCE_ID:
				return typeof(string);
			case COL_SOURCE_ACCOUNT:
				return typeof(string);
			case COL_EFF_FLAG:
				return typeof(Integer);
			case COL_EFF_01:
				return typeof(Double);
			case COL_EFF_02:
				return typeof(Double);
			case COL_EFF_03:
				return typeof(Double);
			case COL_EFF_04:
				return typeof(Double);
			case COL_EFF_05:
				return typeof(Double);
			case COL_EFF_06:
				return typeof(Double);
			case COL_EFF_07:
				return typeof(Double);
			case COL_EFF_08:
				return typeof(Double);
			case COL_EFF_09:
				return typeof(Double);
			case COL_EFF_10:
				return typeof(Double);
			case COL_EFF_11:
				return typeof(Double);
			case COL_EFF_12:
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
			case COL_ID:
				return "ID";
			case COL_NAME:
				return "NAME";
			case COL_RIVER_NODE_ID:
				return "RIVER\nNODE ID";
			case COL_ON_OFF:
				return "ON/OFF\nSWITCH";
			case COL_TYPE:
				return "PLAN\nTYPE";
			case COL_RETURN_TYPE:
				return "RETURN\nTYPE";
			case COL_FAILURE_SWITCH:
				return "FAILURE\nSWITCH";
			case COL_INITIAL_STORAGE:
				return "INITIAL\nSTORAGE (ACFT)";
			case COL_SOURCE_ID:
				return "SOURCE\nID";
			case COL_SOURCE_ACCOUNT:
				return "SOURCE\nACCOUNT";
			case COL_EFF_FLAG:
				return "EFFICIENCY\nFLAG";
			case COL_EFF_01:
				return "EFFICIENCY\nMONTH 1";
			case COL_EFF_02:
				return "EFFICIENCY\nMONTH 2";
			case COL_EFF_03:
				return "EFFICIENCY\nMONTH 3";
			case COL_EFF_04:
				return "EFFICIENCY\nMONTH 4";
			case COL_EFF_05:
				return "EFFICIENCY\nMONTH 5";
			case COL_EFF_06:
				return "EFFICIENCY\nMONTH 6";
			case COL_EFF_07:
				return "EFFICIENCY\nMONTH 7";
			case COL_EFF_08:
				return "EFFICIENCY\nMONTH 8";
			case COL_EFF_09:
				return "EFFICIENCY\nMONTH 9";
			case COL_EFF_10:
				return "EFFICIENCY\nMONTH 10";
			case COL_EFF_11:
				return "EFFICIENCY\nMONTH 11";
			case COL_EFF_12:
				return "EFFICIENCY\nMONTH 12";
			default:
				return " ";
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
		widths[COL_ID] = 8;
		widths[COL_NAME] = 18;
		widths[COL_RIVER_NODE_ID] = 8;
		widths[COL_ON_OFF] = 5;
		widths[COL_TYPE] = 4;
		widths[COL_RETURN_TYPE] = 6;
		widths[COL_FAILURE_SWITCH] = 6;
		widths[COL_INITIAL_STORAGE] = 11;
		widths[COL_SOURCE_ID] = 8;
		widths[COL_SOURCE_ACCOUNT] = 8;
		widths[COL_EFF_FLAG] = 8;
		widths[COL_EFF_01] = 8;
		widths[COL_EFF_02] = 8;
		widths[COL_EFF_03] = 8;
		widths[COL_EFF_04] = 8;
		widths[COL_EFF_05] = 8;
		widths[COL_EFF_06] = 8;
		widths[COL_EFF_07] = 8;
		widths[COL_EFF_08] = 8;
		widths[COL_EFF_09] = 8;
		widths[COL_EFF_10] = 8;
		widths[COL_EFF_11] = 8;
		widths[COL_EFF_12] = 8;
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
				return "%-12.12s";
			case COL_NAME:
				return "%-24.24s";
			case COL_RIVER_NODE_ID:
				return "%-12.12s";
			case COL_ON_OFF:
				return "%8d";
			case COL_TYPE:
				return "%8d";
			case COL_RETURN_TYPE:
				return "%8d";
			case COL_FAILURE_SWITCH:
				return "%8d";
			case COL_INITIAL_STORAGE:
				return "%.2f";
			case COL_SOURCE_ID:
				return "%-12.12s";
			case COL_SOURCE_ACCOUNT:
				return "%-8.8s";
			case COL_EFF_FLAG:
				return "%8d";
			case COL_EFF_01:
				return "%10.2f";
			case COL_EFF_02:
				return "%10.2f";
			case COL_EFF_03:
				return "%10.2f";
			case COL_EFF_04:
				return "%10.2f";
			case COL_EFF_05:
				return "%10.2f";
			case COL_EFF_06:
				return "%10.2f";
			case COL_EFF_07:
				return "%10.2f";
			case COL_EFF_08:
				return "%10.2f";
			case COL_EFF_09:
				return "%10.2f";
			case COL_EFF_10:
				return "%10.2f";
			case COL_EFF_11:
				return "%10.2f";
			case COL_EFF_12:
				return "%10.2f";
			default:
				return "%-s";
		}
	}

	// REVISIT (SAM - 2005-01-20)
	// we might need to display flag values as "1 - XXX" to be readable.  Let's 
	// wait on some user feedback.  If editable, this may mean that choices are 
	// shown.

	/// <summary>
	/// Returns the number of rows of data in the table. </summary>
	/// <returns> the number of rows of data in the table. </returns>
	public virtual int getRowCount()
	{
		return _rows;
	}

	/// <summary>
	/// Returns general data validators for the given column. </summary>
	/// <param name="col"> Column to get validator for. </param>
	/// <returns> List of general Validators. </returns>
	public virtual Validator[] getValidators(int col)
	{
		Validator[] no_checks = new Validator[] {};
		// Daily ID must be an ID, zero, 3, or 4.
		Validator[] dailyID = new Validator[] {Validators.regexValidator("^[0-9a-zA-Z\\.]+$"), Validators.isEquals(new int?(0)), Validators.isEquals(new int?(3)), Validators.isEquals(new int?(4))};
		Validator[] dailyIDValidators = new Validator[] {Validators.notBlankValidator(), Validators.or(dailyID)};
		// Demand type must be between 1 and 5
		Validator[] demand_type = new Validator[] {Validators.notBlankValidator(), Validators.rangeValidator(0, 6)};
		// Use type must be less than 6 and greater than -1
		Validator[] use_type = new Validator[] {Validators.notBlankValidator(), Validators.rangeValidator(-1, 6)};
		// Demand source must be greater than 0, less than 9
		// or equal to -999
		Validator[] demands = new Validator[] {Validators.rangeValidator(0, 9), Validators.isEquals(new int?(-999))};
		Validator[] demand_source = new Validator[] {Validators.notBlankValidator(), Validators.or(demands)};
		// Efficiencies must be between 0 and 100
		Validator[] generalAndRangeZeroToHundred = new Validator [] {Validators.notBlankValidator(), Validators.regexValidator("^[0-9\\-]+$"), Validators.rangeValidator(0, 9999999), Validators.rangeValidator(-1, 101)};

		switch (col)
		{
			case COL_ID:
				return StateMod_Data_TableModel_Fields.ids;
			case COL_NAME:
				return StateMod_Data_TableModel_Fields.blank;
			case COL_RIVER_NODE_ID:
				return StateMod_Data_TableModel_Fields.ids;
			case COL_ON_OFF:
				return StateMod_Data_TableModel_Fields.on_off_switch;
			//case COL_TYPE: return "%8d";
			//case COL_RETURN_TYPE: return "%8d";
			//case COL_FAILURE_SWITCH: return "%8d";
			//case COL_INITIAL_STORAGE: return "%.2f";
			//case COL_SOURCE_ID: return "%-12.12s";
			//case COL_SOURCE_ACCOUNT: return "%-8.8s";
			//case COL_EFF_FLAG: return "%8d";
			case COL_EFF_01:
				return generalAndRangeZeroToHundred;
			case COL_EFF_02:
				return generalAndRangeZeroToHundred;
			case COL_EFF_03:
				return generalAndRangeZeroToHundred;
			case COL_EFF_04:
				return generalAndRangeZeroToHundred;
			case COL_EFF_05:
				return generalAndRangeZeroToHundred;
			case COL_EFF_06:
				return generalAndRangeZeroToHundred;
			case COL_EFF_07:
				return generalAndRangeZeroToHundred;
			case COL_EFF_08:
				return generalAndRangeZeroToHundred;
			case COL_EFF_09:
				return generalAndRangeZeroToHundred;
			case COL_EFF_10:
				return generalAndRangeZeroToHundred;
			case COL_EFF_11:
				return generalAndRangeZeroToHundred;
			case COL_EFF_12:
				return generalAndRangeZeroToHundred;
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

		StateMod_Plan smd = (StateMod_Plan)_data.get(row);
		switch (col)
		{
			case COL_ID:
				return smd.getID();
			case COL_NAME:
				return smd.getName();
			case COL_RIVER_NODE_ID:
				return smd.getCgoto();
			case COL_ON_OFF:
				return new int?(smd.getSwitch());
			case COL_TYPE:
				return new int?(smd.getIPlnTyp());
			case COL_RETURN_TYPE:
				return new int?(smd.getIPrf());
			case COL_FAILURE_SWITCH:
				return new int?(smd.getIPfail());
			case COL_INITIAL_STORAGE:
				return new double?(smd.getPsto1());
			case COL_SOURCE_ID:
				return smd.getPsource();
			case COL_SOURCE_ACCOUNT:
				return smd.getIPAcc();
			case COL_EFF_FLAG:
				return new int?(smd.getPeffFlag());
			case COL_EFF_01:
			case COL_EFF_02:
			case COL_EFF_03:
			case COL_EFF_04:
			case COL_EFF_05:
			case COL_EFF_06:
			case COL_EFF_07:
			case COL_EFF_08:
			case COL_EFF_09:
			case COL_EFF_10:
			case COL_EFF_11:
			case COL_EFF_12:
				int peffFlag = smd.getPeffFlag();
				if (peffFlag == 1)
				{
					return new double?(smd.getPeff(col - COL_EFF_01));
				}
				else
				{
					return "";
				}
			default:
				return "";
		}
	}

	/// <summary>
	/// Returns whether the cell is editable or not.  Currently no cells are editable. </summary>
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
		if (_sortOrder != null)
		{
			row = _sortOrder[row];
		}
		int ival;
		int index;

		StateMod_Plan smd = (StateMod_Plan)_data.get(row);

		switch (col)
		{
			case COL_ID:
				smd.setID((string)value);
				break;
			case COL_NAME:
				smd.setName((string)value);
				break;
			case COL_RIVER_NODE_ID:
				smd.setCgoto((string)value);
				break;
			case COL_ON_OFF:
				if (value is int?)
				{
					ival = ((int?)value).Value;
					smd.setSwitch(ival);
				}
				else if (value is string)
				{
					string onOff = (string)value;
					index = onOff.IndexOf(" -", StringComparison.Ordinal);
					ival = (Convert.ToInt32(onOff.Substring(0,index)));
					smd.setSwitch(ival);
				}
				break;
			case COL_TYPE:
				smd.setIPlnTyp((int?)value);
				break;
			case COL_RETURN_TYPE:
				smd.setIPrf((int?)value);
				break;
			case COL_FAILURE_SWITCH:
				smd.setIPfail((int?)value);
				break;
			case COL_INITIAL_STORAGE:
				smd.setPsto1((double?)value);
				break;
			case COL_SOURCE_ID:
				smd.setPsource((string)value);
				break;
			case COL_SOURCE_ACCOUNT:
				smd.setIPAcc((string)value);
				break;
			case COL_EFF_FLAG:
				smd.setPeffFlag((int?)value);
				break;
			case COL_EFF_01:
			case COL_EFF_02:
			case COL_EFF_03:
			case COL_EFF_04:
			case COL_EFF_05:
			case COL_EFF_06:
			case COL_EFF_07:
			case COL_EFF_08:
			case COL_EFF_09:
			case COL_EFF_10:
			case COL_EFF_11:
			case COL_EFF_12:
				smd.setPeff((col - COL_EFF_01), (double?)value.Value);
				break;
		}

		base.setValueAt(value, row, col);
	}

	/// <summary>
	/// Returns the text to be assigned to worksheet tooltips. </summary>
	/// <returns> a String array of tool tips. </returns>
	public virtual string[] getColumnToolTips()
	{
		string[] tips = new string[__COLUMNS];

		tips[COL_ID] = "<html>The diversion station identifier is the main link" +
			" between diversion data<br>and must be unique in the data set.</html>";
		tips[COL_NAME] = "Diversion station name.";

		tips[COL_RIVER_NODE_ID] = "River node where diversion station is located.";
		tips[COL_ON_OFF] = "Indicates whether diversion station is on (1) or off (0)";
		tips[COL_TYPE] = "Plan type (e.g., 1 = Terms and Conditions)";
		tips[COL_RETURN_TYPE] = "Plan return type";
		tips[COL_FAILURE_SWITCH] = "Plan failure switch";
		tips[COL_INITIAL_STORAGE] = "Plan initial storage";
		tips[COL_SOURCE_ID] = "Source ID where plan station water becomes available (type 8 only)";
		tips[COL_SOURCE_ACCOUNT] = "Source account corresponding to source ID";
		tips[COL_EFF_FLAG] = "Plan efficiency flag (0=not used, 1=provide monthly values, 999=use source efficiency";
		tips[COL_EFF_01] = "Plan efficiency for month 1 of model year.";
		tips[COL_EFF_02] = "Plan efficiency for month 2 of model year.";
		tips[COL_EFF_03] = "Plan efficiency for month 3 of model year.";
		tips[COL_EFF_04] = "Plan efficiency for month 4 of model year.";
		tips[COL_EFF_05] = "Plan efficiency for month 5 of model year.";
		tips[COL_EFF_06] = "Plan efficiency for month 6 of model year.";
		tips[COL_EFF_07] = "Plan efficiency for month 7 of model year.";
		tips[COL_EFF_08] = "Plan efficiency for month 8 of model year.";
		tips[COL_EFF_09] = "Plan efficiency for month 9 of model year.";
		tips[COL_EFF_10] = "Plan efficiency for month 10 of model year.";
		tips[COL_EFF_11] = "Plan efficiency for month 11 of model year.";
		tips[COL_EFF_12] = "Plan efficiency for month 12 of model year.";
		return tips;
	}

	}

}