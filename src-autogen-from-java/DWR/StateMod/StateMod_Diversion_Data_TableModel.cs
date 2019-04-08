using System;
using System.Collections.Generic;

// StateMod_Diversion_Data_TableModel - Table model for displaying data in the diversion station tables.

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
// StateMod_Diversion_Data_TableModel - Table model for displaying data in the
//	diversion station tables.
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
	/// This table model display data in diversion tables.
	/// </summary>
	public class StateMod_Diversion_Data_TableModel : JWorksheet_AbstractRowTableModel, StateMod_Data_TableModel
	{

	/// <summary>
	/// Number of columns in the table model (this includes all data - other code
	/// can hide columns if necessary).
	/// </summary>
	private int __COLUMNS = 25;

	/// <summary>
	/// References to columns.
	/// </summary>
	public const int COL_ID = 0, COL_NAME = 1, COL_RIVER_NODE_ID = 2, COL_ON_OFF = 3, COL_CAPACITY = 4, COL_REPLACE_RES_OPTION = 5, COL_DAILY_ID = 6, COL_USER_NAME = 7, COL_DEMAND_TYPE = 8, COL_EFF_ANNUAL = 9, COL_AREA = 10, COL_USE_TYPE = 11, COL_DEMAND_SOURCE = 12, COL_EFF_01 = 13, COL_EFF_02 = 14, COL_EFF_03 = 15, COL_EFF_04 = 16, COL_EFF_05 = 17, COL_EFF_06 = 18, COL_EFF_07 = 19, COL_EFF_08 = 20, COL_EFF_09 = 21, COL_EFF_10 = 22, COL_EFF_11 = 23, COL_EFF_12 = 24;

	/// <summary>
	/// Whether the table data can be edited or not.
	/// </summary>
	private bool __editable = true;

	/// <summary>
	/// Constructor.  This builds the Model for displaying the diversion data. </summary>
	/// <param name="data"> the data that will be displayed in the table. </param>
	/// <param name="editable"> whether the data can be edited or not </param>
	public StateMod_Diversion_Data_TableModel(System.Collections.IList data, bool editable)
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
			case COL_CAPACITY:
				return typeof(Double);
			case COL_REPLACE_RES_OPTION:
				return typeof(Integer);
			case COL_DAILY_ID:
				return typeof(string);
			case COL_USER_NAME:
				return typeof(string);
			case COL_DEMAND_TYPE:
				return typeof(Integer);
			case COL_EFF_ANNUAL:
				return typeof(Double);
			case COL_AREA:
				return typeof(Double);
			case COL_USE_TYPE:
				return typeof(Integer);
			case COL_DEMAND_SOURCE:
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
			case COL_CAPACITY:
				return "CAPACITY\n(CFS)";
			case COL_REPLACE_RES_OPTION:
				return "REPLACE.\nRES. OPTION";
			case COL_DAILY_ID:
				return "DAILY\nID";
			case COL_USER_NAME:
				return "USER\nNAME";
			case COL_DEMAND_TYPE:
				return "DEMAND\nTYPE";
			case COL_EFF_ANNUAL:
				return "EFFICIENCY\nANNUAL (%)";
			case COL_AREA:
				return "AREA\n(ACRE)";
			case COL_USE_TYPE:
				return "USE\nTYPE";
			case COL_DEMAND_SOURCE:
				return "DEMAND\nSOURCE";
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
		widths[COL_CAPACITY] = 7;
		widths[COL_REPLACE_RES_OPTION] = 8;
		widths[COL_DAILY_ID] = 8;
		widths[COL_USER_NAME] = 18;
		widths[COL_DEMAND_TYPE] = 5;
		widths[COL_EFF_ANNUAL] = 8;
		widths[COL_AREA] = 6; // Wider than title for big
							// ditches
		widths[COL_USE_TYPE] = 7;
		widths[COL_DEMAND_SOURCE] = 6;
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
			case COL_CAPACITY:
				return "%8.2f";
			case COL_REPLACE_RES_OPTION:
				return "%8d";
			case COL_DAILY_ID:
				return "%-12.12s";
			case COL_USER_NAME:
				return "%-24.24s";
			case COL_DEMAND_TYPE:
				return "%8d";
			case COL_EFF_ANNUAL:
				return "%8.1f";
			case COL_AREA:
				return "%8.2f";
			case COL_USE_TYPE:
				return "%8d";
			case COL_DEMAND_SOURCE:
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
		case COL_CAPACITY:
			return StateMod_Data_TableModel_Fields.nums;
		case COL_REPLACE_RES_OPTION:
			return StateMod_Data_TableModel_Fields.nums;
		case COL_DAILY_ID:
			return dailyIDValidators;
		case COL_USER_NAME:
			return StateMod_Data_TableModel_Fields.blank;
		case COL_DEMAND_TYPE:
			return demand_type;
		case COL_AREA:
			return StateMod_Data_TableModel_Fields.nums;
		case COL_USE_TYPE:
			return use_type;
		case COL_DEMAND_SOURCE:
			return demand_source;
		case COL_EFF_ANNUAL:
			return generalAndRangeZeroToHundred;
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
	/// Returns the data that should be placed in the JTable at the given row and 
	/// column. </summary>
	/// <param name="row"> the row for which to return data. </param>
	/// <param name="col"> the column for which to return data. </param>
	/// <returns> the data that should be placed in the JTable at the given row and col. </returns>
	public virtual object getValueAt(int row, int col)
	{
		if (_sortOrder != null)
		{
			row = _sortOrder[row];
		}

		StateMod_Diversion smd = (StateMod_Diversion)_data.get(row);
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
			case COL_CAPACITY:
				return new double?(smd.getDivcap());
			case COL_REPLACE_RES_OPTION:
						return new int?(smd.getIreptype());
			case COL_DAILY_ID:
				return smd.getCdividy();
			case COL_USER_NAME:
				return smd.getUsername();
			case COL_DEMAND_TYPE:
				return new int?(smd.getIdvcom());
			case COL_AREA:
				return new double?(smd.getArea());
			case COL_USE_TYPE:
				return new int?(smd.getIrturn());
			case COL_DEMAND_SOURCE:
				return new int?(smd.getDemsrc());
			case COL_EFF_ANNUAL:
				return new double?(smd.getDivefc());
			case COL_EFF_01:
				return new double?(smd.getDiveff(0));
			case COL_EFF_02:
				return new double?(smd.getDiveff(1));
			case COL_EFF_03:
				return new double?(smd.getDiveff(2));
			case COL_EFF_04:
				return new double?(smd.getDiveff(3));
			case COL_EFF_05:
				return new double?(smd.getDiveff(4));
			case COL_EFF_06:
				return new double?(smd.getDiveff(5));
			case COL_EFF_07:
				return new double?(smd.getDiveff(6));
			case COL_EFF_08:
				return new double?(smd.getDiveff(7));
			case COL_EFF_09:
				return new double?(smd.getDiveff(8));
			case COL_EFF_10:
				return new double?(smd.getDiveff(9));
			case COL_EFF_11:
				return new double?(smd.getDiveff(10));
			case COL_EFF_12:
				return new double?(smd.getDiveff(11));
			default:
				return "";
		}
	}

	/// <summary>
	/// Returns whether the cell is editable or not.  Currently no cells are editable. </summary>
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

		StateMod_Diversion smd = (StateMod_Diversion)_data.get(row);

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
					ival = (Convert.ToInt32(onOff.Substring(0, index)));
					smd.setSwitch(ival);
				}
				break;
			case COL_CAPACITY:
				smd.setDivcap((double?)value);
				break;
			case COL_REPLACE_RES_OPTION:
				if (value is int?)
				{
					ival = ((int?)value).Value;
						smd.setIreptype(ival);
				}
				else if (value is string)
				{
					string ireptyp = (string)value;
					index = ireptyp.IndexOf(" -", StringComparison.Ordinal);
					ival = (Convert.ToInt32(ireptyp.Substring(0, index)));
					smd.setIreptype(ival);
				}
				break;
			case COL_DAILY_ID:
				smd.setCdividy((string)value);
				break;
			case COL_USER_NAME:
				smd.setUsername((string)value);
				break;
			case COL_DEMAND_TYPE:
				if (value is int?)
				{
					ival = ((int?)value).Value;
						smd.setIdvcom(ival);
				}
				else if (value is string)
				{
					string idvcom = (string)value;
					index = idvcom.IndexOf(" -", StringComparison.Ordinal);
					ival = (Convert.ToInt32(idvcom.Substring(0, index)));
					smd.setIdvcom(ival);
				}
				break;
			case COL_EFF_ANNUAL:
				smd.setDivefc((double?)value);
				break;
			case COL_AREA:
				smd.setArea((double?)value);
				break;
			case COL_USE_TYPE:
				if (value is int?)
				{
					ival = ((int?)value).Value;
						smd.setIrturn(ival);
				}
				else if (value is string)
				{
					string irturn = (string)value;
					index = irturn.IndexOf(" -", StringComparison.Ordinal);
					ival = (Convert.ToInt32(irturn.Substring(0, index)));
					smd.setIrturn(ival);
				}
				break;
			case COL_DEMAND_SOURCE:
				if (value is int?)
				{
					ival = ((int?)value).Value;
						smd.setDemsrc(ival);
				}
				else if (value is string)
				{
					string demsrc = (string)value;
					index = demsrc.IndexOf(" -", StringComparison.Ordinal);
					ival = (Convert.ToInt32(demsrc.Substring(0, index)));
					smd.setDemsrc(ival);
				}
				break;
			case COL_EFF_01:
				smd.setDiveff(0, (double?)value);
				break;
			case COL_EFF_02:
				smd.setDiveff(1, (double?)value);
				break;
			case COL_EFF_03:
				smd.setDiveff(2, (double?)value);
				break;
			case COL_EFF_04:
				smd.setDiveff(3, (double?)value);
				break;
			case COL_EFF_05:
				smd.setDiveff(4, (double?)value);
				break;
			case COL_EFF_06:
				smd.setDiveff(5, (double?)value);
				break;
			case COL_EFF_07:
				smd.setDiveff(6, (double?)value);
				break;
			case COL_EFF_08:
				smd.setDiveff(7, (double?)value);
				break;
			case COL_EFF_09:
				smd.setDiveff(8, (double?)value);
				break;
			case COL_EFF_10:
				smd.setDiveff(9, (double?)value);
				break;
			case COL_EFF_11:
				smd.setDiveff(10, (double?)value);
				break;
			case COL_EFF_12:
				smd.setDiveff(11, (double?)value);
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
			" between diversion data<BR>" +
			"and must be unique in the data set.</html>";
		tips[COL_NAME] = "Diversion station name.";

		tips[COL_RIVER_NODE_ID] = "River node where diversion station is located.";
		tips[COL_ON_OFF] = "Indicates whether diversion station is on (1) or off (0)";
		tips[COL_CAPACITY] = "Diversion station capacity (CFS)";
		tips[COL_REPLACE_RES_OPTION] = "Replacement reservoir option.";
		tips[COL_DAILY_ID] = "Daily identifier (for daily time series).";
		tips[COL_USER_NAME] = "User name.";
		tips[COL_DEMAND_TYPE] = "(Monthly) demand type.";
		tips[COL_EFF_ANNUAL] = "Efficiency, annual (%).  Negative indicates "
			+ "monthly efficiencies.";
		tips[COL_AREA] = "Irrigated area (ACRE).";
		tips[COL_USE_TYPE] = "Use type.";
		tips[COL_DEMAND_SOURCE] = "Demand source.";
		tips[COL_EFF_01] = "Diversion efficiency for month 1 of year.";
		tips[COL_EFF_02] = "Diversion efficiency for month 2 of year.";
		tips[COL_EFF_03] = "Diversion efficiency for month 3 of year.";
		tips[COL_EFF_04] = "Diversion efficiency for month 4 of year.";
		tips[COL_EFF_05] = "Diversion efficiency for month 5 of year.";
		tips[COL_EFF_06] = "Diversion efficiency for month 6 of year.";
		tips[COL_EFF_07] = "Diversion efficiency for month 7 of year.";
		tips[COL_EFF_08] = "Diversion efficiency for month 8 of year.";
		tips[COL_EFF_09] = "Diversion efficiency for month 9 of year.";
		tips[COL_EFF_10] = "Diversion efficiency for month 10 of year.";
		tips[COL_EFF_11] = "Diversion efficiency for month 11 of year.";
		tips[COL_EFF_12] = "Diversion efficiency for month 12 of year.";
		return tips;
	}

	}

}