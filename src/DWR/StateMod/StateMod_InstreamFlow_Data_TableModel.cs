using System;
using System.Collections.Generic;

// StateMod_InstreamFlow_Data_TableModel - Table model for displaying data in the Instream Flow station tables

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
// StateMod_InstreamFlow_Data_TableModel - Table model for displaying data in 
//	the Instream Flow station tables
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
//
// 2005-03-31	J. Thomas Sapienza, RTi	Initial version.
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
	/// This table model displays instream flow data.
	/// </summary>
	public class StateMod_InstreamFlow_Data_TableModel : JWorksheet_AbstractRowTableModel, StateMod_Data_TableModel
	{

	/// <summary>
	/// Number of columns in the table model.
	/// </summary>
	private int __COLUMNS = 7;

	/// <summary>
	/// References to columns.
	/// </summary>
	public const int COL_ID = 0, COL_NAME = 1, COL_NODE_ID = 2, COL_SWITCH = 3, COL_DOWN_NODE = 4, COL_DAILY_ID = 5, COL_DEMAND_TYPE = 6;

	/// <summary>
	/// Whether the table data are editable or not.
	/// </summary>
	private bool __editable = false;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="data"> the data that will be displayed in the table. </param>
	/// <param name="editable"> whether the table data is editable or not </param>
	public StateMod_InstreamFlow_Data_TableModel(System.Collections.IList data, bool editable)
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
	/// returns the class of the data stored in a given column. </summary>
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
			case COL_DAILY_ID:
				return typeof(string);
			case COL_DOWN_NODE:
				return typeof(string);
			case COL_DEMAND_TYPE:
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
			case COL_ID:
				return "\n\nID";
			case COL_NAME:
				return "\n\nNAME";
			case COL_NODE_ID:
				return "\nRIVER\nNODE ID";
			case COL_SWITCH:
				return "\nON/OFF\nSWITCH";
			case COL_DOWN_NODE:
				return "DOWNSTREAM\nRIVER\nNODE ID";
			case COL_DAILY_ID:
				return "\n\nDAILY ID";
			case COL_DEMAND_TYPE:
				return "\nDEMAND\nTYPE";
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

		tips[COL_ID] = "<html>The instream flow identifier is the main link"
			+ " between instream data data<BR>"
			+ "and must be unique in the data set.</html>";
		tips[COL_NAME] = "<html>Instream flow name.</html>";
		tips[COL_NODE_ID] = "Upstream river ID where instream flow is located.";
		tips[COL_SWITCH] = "<html>Switch.<br>0 = off<br>1 = on</html";
		tips[COL_DAILY_ID] = "<html>Daily instream flow ID.</html>";
		tips[COL_DOWN_NODE] = "<html>Downstream river node, for instream flow "
			+ "reach.</html>";
		tips[COL_DEMAND_TYPE] = "<html>Data type switch.</html>";

		return tips;
	}

	/// <summary>
	/// Returns an array containing the widths (in number of characters) that the 
	/// fields in the table should be sized to. </summary>
	/// <returns> an integer array containing the widths for each field. </returns>
	public virtual int[] getColumnWidths()
	{
		int[] widths = new int[__COLUMNS];
		widths[COL_ID] = 8;
		widths[COL_NAME] = 21;
		widths[COL_NODE_ID] = 8;
		widths[COL_SWITCH] = 6;
		widths[COL_DAILY_ID] = 8;
		widths[COL_DOWN_NODE] = 12;
		widths[COL_DEMAND_TYPE] = 5;

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
			case COL_NODE_ID:
				return "%-12.12s";
			case COL_SWITCH:
				return "%8d";
			case COL_DAILY_ID:
				return "%-12.12s";
			case COL_DOWN_NODE:
				return "%-12.12s";
			case COL_DEMAND_TYPE:
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
		// Data type switch must be 0, 1 or 2.
		Validator[] data_type = new Validator[] {Validators.isEquals(new int?(0)), Validators.isEquals(new int?(1)), Validators.isEquals(new int?(2))};
		Validator[] data_type_Validators = new Validator[] {Validators.or(data_type)};

		switch (col)
		{
			case COL_ID:
				return StateMod_Data_TableModel_Fields.ids;
			case COL_NAME:
				return StateMod_Data_TableModel_Fields.blank;
			case COL_NODE_ID:
				return StateMod_Data_TableModel_Fields.ids;
			case COL_SWITCH:
				return StateMod_Data_TableModel_Fields.on_off_switch;
			case COL_DAILY_ID:
				return StateMod_Data_TableModel_Fields.ids;
			case COL_DOWN_NODE:
				return StateMod_Data_TableModel_Fields.blank;
			case COL_DEMAND_TYPE:
				return data_type_Validators;
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

		StateMod_InstreamFlow isf = (StateMod_InstreamFlow)_data.get(row);
		switch (col)
		{
			case COL_ID:
				return isf.getID();
			case COL_NAME:
				return isf.getName();
			case COL_NODE_ID:
				return isf.getCgoto();
			case COL_SWITCH:
				return new int?(isf.getSwitch());
			case COL_DAILY_ID:
				return isf.getCifridy();
			case COL_DOWN_NODE:
				return isf.getIfrrdn();
			case COL_DEMAND_TYPE:
				return new int?(isf.getIifcom());
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
		StateMod_InstreamFlow isf = (StateMod_InstreamFlow)_data.get(row);
		switch (col)
		{
			case COL_ID:
				isf.setID((string)value);
				break;
			case COL_NAME:
				isf.setName((string)value);
				break;
			case COL_NODE_ID:
				isf.setCgoto((string)value);
				break;
			case COL_SWITCH:
				isf.setSwitch(((int?)value).Value);
				break;
			case COL_DAILY_ID:
				isf.setCifridy((string)value);
				break;
			case COL_DOWN_NODE:
				isf.setIfrrdn((string)value);
				break;
			case COL_DEMAND_TYPE:
				isf.setIifcom(((int?)value).Value);
				break;
		}
		base.setValueAt(value, row, col);
	}

	}

}