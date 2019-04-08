using System;
using System.Collections.Generic;

// StateMod_ReservoirRight_Data_TableModel - table model for displaying reservoir right data
//
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
// StateMod_ReservoirRight_Data_TableModel - table model for displaying 
//	reservoir right data
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
//
// 2005-04-04	J. Thomas Sapienza, RTi	Initial version.
// 2006-04-11	JTS, RTi		Corrected the classes returned from
//							getColumnClass().
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
	/// This table model displays reservoir right data.  The model can display rights
	/// data for a single reservoir or for 1+ reservoirs.  The difference is specified
	/// in the constructor and affects how many columns of data are shown.
	/// </summary>
	public class StateMod_ReservoirRight_Data_TableModel : JWorksheet_AbstractRowTableModel, StateMod_Data_TableModel
	{

	/// <summary>
	/// Number of columns in the table model.  For table models that display rights 
	/// for a single reservoir, the tables only have 10 columns.  Table models that
	/// display rights for 1+ reservoirs have 11.  The variable is modified in the constructor.
	/// </summary>
	private int __COLUMNS = 10;

	/// <summary>
	/// References to columns.
	/// </summary>
	public const int COL_RIGHT_ID = 0, COL_RIGHT_NAME = 1, COL_STRUCT_ID = 2, COL_ADMIN_NUM = 3, COL_DCR_AMT = 4, COL_ON_OFF = 5, COL_ACCOUNT_DIST = 6, COL_RIGHT_TYPE = 7, COL_FILL_TYPE = 8, COL_OOP_RIGHT = 9;

	/// <summary>
	/// Whether the table data is editable or not.
	/// </summary>
	private bool __editable = true;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="data"> the data that will be displayed in the table. </param>
	/// <param name="editable"> whether the table data can be modified or not. </param>
	public StateMod_ReservoirRight_Data_TableModel(System.Collections.IList data, bool editable)
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
	/// <param name="col"> the column for which to return the data class. </param>
	/// <returns> the class of the data stored in a given column. </returns>
	public virtual Type getColumnClass(int col)
	{
		switch (col)
		{
			case COL_RIGHT_ID:
				return typeof(string);
			case COL_RIGHT_NAME:
				return typeof(string);
			case COL_STRUCT_ID:
				return typeof(string);
			case COL_ADMIN_NUM:
				return typeof(string);
			case COL_DCR_AMT:
				return typeof(Double);
			case COL_ON_OFF:
				return typeof(Integer);
			case COL_ACCOUNT_DIST:
				return typeof(Integer);
			case COL_RIGHT_TYPE:
				return typeof(Integer);
			case COL_FILL_TYPE:
				return typeof(Integer);
			case COL_OOP_RIGHT:
				return typeof(string);
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
	/// <param name="col"> the position of the column for which to return the name. </param>
	/// <returns> the name of the column at the given position. </returns>
	public virtual string getColumnName(int col)
	{
		switch (col)
		{
			case COL_RIGHT_ID:
				return "\nRIGHT\nID";
			case COL_RIGHT_NAME:
				return "\nRIGHT\nNAME";
			case COL_STRUCT_ID:
				return "RESERVOIR\nSTATION ID\nASSOC. W/ RIGHT";
			case COL_ADMIN_NUM:
				return "\nADMINISTRATION\nNUMBER";
			case COL_DCR_AMT:
				return "DECREE\nAMOUNT\n(ACFT)";
			case COL_ON_OFF:
				return "\nON/OFF\nSWITCH";
			case COL_ACCOUNT_DIST:
				return "\nACCOUNT\nDISTRIBUTION";
			case COL_RIGHT_TYPE:
				return "\n\nRIGHT TYPE";
			case COL_FILL_TYPE:
				return "\n\nFILL TYPE";
			case COL_OOP_RIGHT:
				return "OUT OF\nPRIORITY\nRIGHT";
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

		tips[COL_RIGHT_ID] = "<html>The reservoir right ID is typically the reservoir" +
			" station ID<br> followed by .01, .02, etc.</html>";
		tips[COL_RIGHT_NAME] = "Reservoir right name";
		tips[COL_STRUCT_ID] = "<HTML>The reservoir ID is the link between reservoir stations "
			+ "and their right(s).</HTML>";
		tips[COL_ADMIN_NUM] = "<HTML>Lower admininistration numbers indicate greater " +
			"seniority.<BR>99999 is typical for a very junior" +
			" right.</html>";
		tips[COL_DCR_AMT] = "Decreed amount (ACFT)";
		tips[COL_ON_OFF] = "<HTML>0 = OFF<BR>1 = ON<BR>" +
			"YYYY indicates to turn on the right in year YYYY."+
			"<BR>-YYYY indicates to turn off the right in year" +
			" YYYY.</HTML>";
		tips[COL_ACCOUNT_DIST] = "Account distribution switch.";
		tips[COL_RIGHT_TYPE] = "Right type.";
		tips[COL_FILL_TYPE] = "Fill type.";
		tips[COL_OOP_RIGHT] = "Out-of-priority associated operational right.";
		return tips;
	}

	/// <summary>
	/// Returns an array containing the widths (in number of characters) that the 
	/// fields in the table should be sized to. </summary>
	/// <returns> an integer array containing the widths for each field. </returns>
	public virtual int[] getColumnWidths()
	{
		int[] widths = new int[__COLUMNS];

		widths[COL_RIGHT_ID] = 8;
		widths[COL_RIGHT_NAME] = 18;
		widths[COL_STRUCT_ID] = 12;
		widths[COL_ADMIN_NUM] = 12;
		widths[COL_DCR_AMT] = 8;
		widths[COL_ON_OFF] = 6;
		widths[COL_ACCOUNT_DIST] = 10;
		widths[COL_RIGHT_TYPE] = 8;
		widths[COL_FILL_TYPE] = 7;
		widths[COL_OOP_RIGHT] = 6;

		return widths;
	}

	/// <summary>
	/// Returns the format that the specified column should be displayed in when
	/// the table is being displayed in the given table format. </summary>
	/// <param name="col"> column for which to return the format. </param>
	/// <returns> the format (as used by StringUtil.formatString() in which to display the
	/// column. </returns>
	public virtual string getFormat(int col)
	{
		switch (col)
		{
			case COL_RIGHT_ID:
				return "%-12.12s";
			case COL_RIGHT_NAME:
				return "%-24.24s";
			case COL_STRUCT_ID:
				return "%-8.8s";
			case COL_ADMIN_NUM:
				return "%-20.20s";
			case COL_DCR_AMT:
				return "%12.1f";
			case COL_ON_OFF:
				return "%8d";
			case COL_ACCOUNT_DIST:
				return "%8d";
			case COL_RIGHT_TYPE:
				return "%8d";
			case COL_FILL_TYPE:
				return "%8d";
			case COL_OOP_RIGHT:
				return "%-12.12s";
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
		// Reservior right type must be -1 or 1
		Validator[] right_type = new Validator[] {Validators.isEquals(new int?(-1)), Validators.isEquals(new int?(1))};
		Validator[] type_Validators = new Validator[] {Validators.or(right_type)};
		// Reservoir fill type must be 1 or 2
		Validator[] fill_type = new Validator[] {Validators.isEquals(new int?(1)), Validators.isEquals(new int?(2))};
		Validator[] fill_Validators = new Validator[] {Validators.or(fill_type)};

		switch (col)
		{
		case COL_RIGHT_ID:
			return StateMod_Data_TableModel_Fields.ids;
		case COL_RIGHT_NAME:
			return StateMod_Data_TableModel_Fields.blank;
		case COL_STRUCT_ID:
			return StateMod_Data_TableModel_Fields.ids;
		case COL_ADMIN_NUM:
			return StateMod_Data_TableModel_Fields.nums;
		case COL_DCR_AMT:
			return StateMod_Data_TableModel_Fields.nums;
		case COL_ON_OFF:
			return StateMod_Data_TableModel_Fields.nums;
		case COL_ACCOUNT_DIST:
			return StateMod_Data_TableModel_Fields.nums;
		case COL_RIGHT_TYPE:
			return type_Validators;
		case COL_FILL_TYPE:
			return fill_Validators;
		case COL_OOP_RIGHT:
			return StateMod_Data_TableModel_Fields.nums;
		default:
			return no_checks;
		}
	}

	/// <summary>
	/// Returns the data that should be placed in the JTable
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

		StateMod_ReservoirRight rr = (StateMod_ReservoirRight)_data.get(row);

		switch (col)
		{
			case COL_RIGHT_ID:
				return rr.getID();
			case COL_RIGHT_NAME:
				return rr.getName();
			case COL_STRUCT_ID:
				return rr.getCgoto();
			case COL_ADMIN_NUM:
				return rr.getRtem();
			case COL_DCR_AMT:
				return new double?(rr.getDcrres());
			case COL_ON_OFF:
				return new int?(rr.getSwitch());
			case COL_ACCOUNT_DIST:
				return new int?(rr.getIresco());
			case COL_RIGHT_TYPE:
				return new int?(rr.getItyrstr());
			case COL_FILL_TYPE:
				return new int?(rr.getN2fill());
			case COL_OOP_RIGHT:
				return rr.getCopid();
			default:
				return "";
		}
	}

	/// <summary>
	/// Returns whether the cell at the given position is editable or not.  In this
	/// table model all columns are editable (unless the table is not editable). </summary>
	/// <param name="rowIndex"> unused </param>
	/// <param name="col"> the index of the column to check for whether it is editable. </param>
	/// <returns> whether the cell at the given position is editable. </returns>
	public virtual bool isCellEditable(int rowIndex, int col)
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
		double dval;
		int ival;
		StateMod_ReservoirRight rr = (StateMod_ReservoirRight)_data.get(row);

		switch (col)
		{
			case COL_RIGHT_ID:
				rr.setID((string)value);
				break;
			case COL_RIGHT_NAME:
				rr.setName((string)value);
				break;
			case COL_STRUCT_ID:
				rr.setCgoto((string)value);
				break;
			case COL_ADMIN_NUM:
				rr.setRtem((string)value);
				break;
			case COL_DCR_AMT:
				dval = ((double?)value).Value;
				rr.setDcrres(dval);
				break;
			case COL_ON_OFF:
				if (value is int?)
				{
					ival = ((int?)value).Value;
					rr.setSwitch(ival);
				}
				else if (value is string)
				{
					string onOff = (string)value;
					int index = onOff.IndexOf(" -", StringComparison.Ordinal);
					ival = (Convert.ToInt32(onOff.Substring(0, index)));
					rr.setSwitch(ival);
				}
				break;
			case COL_ACCOUNT_DIST:
				if (value is int?)
				{
					ival = ((int?)value).Value;
					rr.setIresco(ival);
				}
				else if (value is string)
				{
					string acct = (string)value;
					int index = acct.IndexOf(" -", StringComparison.Ordinal);
					ival = (Convert.ToInt32(acct.Substring(0, index)));
					rr.setIresco(ival);
				}
				break;
			case COL_RIGHT_TYPE:
				if (value is int?)
				{
					ival = ((int?)value).Value;
					rr.setItyrstr(ival);
				}
				else if (value is string)
				{
					string right = (string)value;
					int index = right.IndexOf(" -", StringComparison.Ordinal);
					ival = (Convert.ToInt32(right.Substring(0, index)));
					rr.setItyrstr(ival);
				}
				break;
			case COL_FILL_TYPE:
				if (value is int?)
				{
					ival = ((int?)value).Value;
					rr.setN2fill(ival);
				}
				else if (value is string)
				{
					string fill = (string)value;
					int index = fill.IndexOf(" -", StringComparison.Ordinal);
					ival = (Convert.ToInt32(fill.Substring(0, index)));
					rr.setN2fill(ival);
				}
				break;
			case COL_OOP_RIGHT:
				rr.setCopid((string)value);
				break;
		}

		base.setValueAt(value, row, col);
	}

	}

}