using System;
using System.Collections.Generic;

// StateMod_InstreamFlowRight_Data_TableModel - Table model for displaying data in Instream Flow right data tables

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
// StateMod_InstreamFlowRight_Data_TableModel - Table model for displaying 
//	data in Instream Flow right data tables
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
//
// 2005-03-31	J. Thomas Sapienza, RTi	Initial version.
// 2006-04-11	JTS, RTi		Corrected the classes returned from
//					getColumnClass().
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
// ----------------------------------------------------------------------------
// EndHeader

namespace DWR.StateMod
{

	using JWorksheet_AbstractRowTableModel = RTi.Util.GUI.JWorksheet_AbstractRowTableModel;
	using Validator = RTi.Util.IO.Validator;

	/// <summary>
	/// This table model displays instream flow right data.
	/// </summary>
	public class StateMod_InstreamFlowRight_Data_TableModel : JWorksheet_AbstractRowTableModel, StateMod_Data_TableModel
	{

	/// <summary>
	/// Number of columns in the table model.
	/// </summary>
	private int __COLUMNS = 6;

	/// <summary>
	/// References to columns.
	/// </summary>
	public const int COL_RIGHT_ID = 0, COL_RIGHT_NAME = 1, COL_STRUCT_ID = 2, COL_ADMIN_NUM = 3, COL_DCR_AMT = 4, COL_ON_OFF = 5;

	/// <summary>
	/// Whether the table data is editable or not.
	/// </summary>
	private bool __editable = false;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="data"> the data that will be displayed in the table. </param>
	/// <param name="editable"> whether the table data is editable or not </param>
	public StateMod_InstreamFlowRight_Data_TableModel(System.Collections.IList data, bool editable)
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
			case COL_RIGHT_ID:
				return "\n\n\nRIGHT ID";
			case COL_RIGHT_NAME:
				return "\n\n\nRIGHT NAME";
			case COL_STRUCT_ID:
				return "INSTREAM FLOW\nSTATION ID\nASSOCIATED"
					+ "\nWITH RIGHT";
				goto case COL_ADMIN_NUM;
			case COL_ADMIN_NUM:
				return "\n\nADMINISTRATION\nNUMBER";
			case COL_DCR_AMT:
				return "\n\nDECREED\nAMOUNT (CFS)";
			case COL_ON_OFF:
				return "\n\nON/OFF\nSWITCH";
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
			case COL_RIGHT_ID:
				return "%-12.12s";
			case COL_RIGHT_NAME:
				return "%-24.24s";
			case COL_STRUCT_ID:
				return "%-12.12s";
			case COL_ADMIN_NUM:
				return "%-12.12s";
			case COL_DCR_AMT:
				return "%8.2f";
			case COL_ON_OFF:
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
			default:
				return no_checks;
		}
	}

	/// <summary>
	/// Returns the data that should be placed in the JTable at the given row 
	/// and column. </summary>
	/// <param name="row"> the row for which to return data. </param>
	/// <param name="col"> the column for which to return data. </param>
	/// <returns> the data that should be placed in the JTable at the given row and col. </returns>
	public virtual object getValueAt(int row, int col)
	{
		if (_sortOrder != null)
		{
			row = _sortOrder[row];
		}

		StateMod_InstreamFlowRight infr = (StateMod_InstreamFlowRight)_data.get(row);
		switch (col)
		{
			case COL_RIGHT_ID:
				return infr.getID();
			case COL_RIGHT_NAME:
				return infr.getName();
			case COL_STRUCT_ID:
				return infr.getCgoto();
			case COL_ADMIN_NUM:
				return infr.getIrtem();
			case COL_DCR_AMT:
				return new double?(infr.getDcrifr());
			case COL_ON_OFF:
				return new int?(infr.getSwitch());
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

		widths[COL_RIGHT_ID] = 8;
		widths[COL_RIGHT_NAME] = 20;
		widths[COL_STRUCT_ID] = 12;
		widths[COL_ADMIN_NUM] = 12;
		widths[COL_DCR_AMT] = 10;
		widths[COL_ON_OFF] = 8;

		return widths;
	}

	/// <summary>
	/// Returns whether the cell is editable or not.  If the table is editable, only
	/// the instream flow ID is not editable - this reflects the display being opened
	/// from the instream flow station window and maintaining agreement between the two
	/// windows. </summary>
	/// <param name="rowIndex"> unused. </param>
	/// <param name="columnIndex"> the index of the column to check whether it is editable
	/// or not. </param>
	/// <returns> whether the cell is editable or not. </returns>
	public virtual bool isCellEditable(int rowIndex, int columnIndex)
	{
		if (!__editable || (columnIndex == COL_STRUCT_ID))
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
		StateMod_InstreamFlowRight ifr = (StateMod_InstreamFlowRight)_data.get(row);

		switch (col)
		{
			case COL_RIGHT_ID:
				ifr.setID((string)value);
				break;
			case COL_RIGHT_NAME:
				ifr.setName((string)value);
				break;
			case COL_STRUCT_ID:
				ifr.setCgoto((string)value);
				break;
			case COL_ADMIN_NUM:
				ifr.setIrtem((string)value);
				break;
			case COL_DCR_AMT:
				dval = ((double?)value).Value;
				ifr.setDcrifr(dval);
				break;
			case COL_ON_OFF:
				if (value is int?)
				{
					ival = ((int?)value).Value;
					ifr.setSwitch(ival);
				}
				else if (value is string)
				{
					string onOff = (string)value;
					int index = onOff.IndexOf(" -", StringComparison.Ordinal);
					ival = (Convert.ToInt32(onOff.Substring(0, index)));
						ifr.setSwitch(ival);
				}
				/*
				ival = ((Integer)value).intValue();
				ifr.setSwitch(ival);
				break;
				*/
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

		tips[COL_RIGHT_ID] = "<html>The instream flow right ID is typically the "
			+ "instream flow ID<br> followed by .01, .02, "
			+ "etc.</html>";
		tips[COL_RIGHT_NAME] = "Instream flow right name.";
		tips[COL_STRUCT_ID] = "<HTML>The instream flow ID is the link between instream "
			+ " flows and their right<BR>(not editable here).</HTML>";
		tips[COL_ADMIN_NUM] = "<HTML>Lower admininistration numbers indicate greater"
			+ "seniority.<BR>99999 is typical for a very junior"
			+ " right.</html>";
		tips[COL_DCR_AMT] = "Decreed amount (CFS).";
		tips[COL_ON_OFF] = "<HTML>0 = OFF<BR>1 = ON<BR>"
			+ "YYYY indicates to turn on the right in year YYYY."
			+ "<BR>-YYYY indicates to turn off the right in year"
			+ " YYYY.</HTML>";

		return tips;
	}

	}

}