using System;
using System.Collections.Generic;

// StateMod_DiversionRight_Data_TableModel - Table model for displaying data in the diversion right data tables.

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
// StateMod_DiversionRight_Data_TableModel - Table model for displaying data 
//	in the diversion right data tables.
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
//
// 2005-03-30	J. Thomas Sapienza, RTi	Initial version.
// 2007-04-27	Kurt Tometich, RTi		Added getValidators method for check
//									file and data check implementation.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace DWR.StateMod
{

	using JWorksheet_AbstractRowTableModel = RTi.Util.GUI.JWorksheet_AbstractRowTableModel;
	using Validator = RTi.Util.IO.Validator;

	/// <summary>
	/// This table model displays diversion right data.  The model can display rights
	/// data for a single diversion or for 1+ diversion.  The difference is specified
	/// in the constructor and affects how many columns of data are shown.
	/// </summary>
	public class StateMod_DiversionRight_Data_TableModel : JWorksheet_AbstractRowTableModel, StateMod_Data_TableModel
	{

	/// <summary>
	/// Number of columns in the table model.  For table models that display rights
	/// for a single diversion, the tables only have 6 columns.  Table models that
	/// display rights for 1+ diversions have 7.  The variable is modified in the constructor.
	/// </summary>
	private int __COLUMNS = 6;

	/// <summary>
	/// References to columns.
	/// </summary>
	public const int COL_RIGHT_ID = 0, COL_RIGHT_NAME = 1, COL_STRUCT_ID = 2, COL_ADMIN_NUM = 3, COL_DCR_AMT = 4, COL_ON_OFF = 5;

	/// <summary>
	/// Whether the table data can be edited.
	/// </summary>
	private bool __editable = true;

	/// <summary>
	/// Constructor.  This builds the table model for displaying the diversion right data. </summary>
	/// <param name="dataset"> the dataset for the data being displayed. </param>
	/// <param name="data"> the diversion right data that will be displayed in the table. </param>
	/// <param name="editable"> whether the data can be edited </param>
	public StateMod_DiversionRight_Data_TableModel(StateMod_DataSet dataset, System.Collections.IList data, bool editable)
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
	/// Constructor.  This builds the table model for displaying the diversion right data. </summary>
	/// <param name="dataset"> the dataset for the data being displayed. </param>
	/// <param name="data"> the diversion right data that will be displayed in the table. </param>
	/// <param name="editable"> whether the data can be edited </param>
	public StateMod_DiversionRight_Data_TableModel(System.Collections.IList data, bool editable) : this(null, data, editable)
	{
	}

	/// <summary>
	/// Returns the class of the data stored in a given column. </summary>
	/// <param name="col"> the column for which to return the data class.  This is base 0. </param>
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
	/// Returns the text to be assigned to worksheet tooltips. </summary>
	/// <returns> a String array of tool tips. </returns>
	public virtual string[] getColumnToolTips()
	{
		string[] tips = new string[__COLUMNS];

		tips[COL_RIGHT_ID] = "<html>The diversion right ID is typically the diversion" +
			" station ID<br> followed by .01, .02, etc.</html>";
		tips[COL_RIGHT_NAME] = "Diversion right name";
		tips[COL_STRUCT_ID] = "<HTML>The diversion ID is the link between diversion stations "
			+ "and their right(s).</HTML>";
		tips[COL_ADMIN_NUM] = "<HTML>Lower admininistration numbers indicate greater " +
			"seniority.<BR>99999 is typical for a very junior" +
			" right.</html>";
		tips[COL_DCR_AMT] = "Decree amount (CFS)";
		tips[COL_ON_OFF] = "<HTML>0 = OFF<BR>1 = ON<BR>" +
			"YYYY indicates to turn on the right in year YYYY."+
			"<BR>-YYYY indicates to turn off the right in year" +
			" YYYY.</HTML>";

		return tips;
	}

	/// <summary>
	/// Returns the name of the column at the given position. </summary>
	/// <param name="col"> the index of the column for which to return the name.  This
	/// is base 0. </param>
	/// <returns> the name of the column at the given position. </returns>
	public virtual string getColumnName(int col)
	{
		switch (col)
		{
			case COL_RIGHT_ID:
				return "\nDIVERSION\nRIGHT ID";
			case COL_RIGHT_NAME:
				return "\nDIVERSION RIGHT\nNAME";
			case COL_STRUCT_ID:
				return "DIVERSION ID\nASSOCIATED\nWITH RIGHT";
			case COL_ADMIN_NUM:
				return "\nADMINISTRATION\nNUMBER";
			case COL_DCR_AMT:
				return "DECREE\nAMOUNT\n(CFS)";
			case COL_ON_OFF:
				return "\nON/OFF\nSWITCH";
			default:
				return " ";
		}
	}

	/// <summary>
	/// Returns the format that the specified column should be displayed in when
	/// the table is being displayed in the given table format. </summary>
	/// <param name="col"> column for which to return the format.  This is base 0. </param>
	/// <returns> the format (as used by StringUtil.formatString() in which to display the column. </returns>
	public virtual string getFormat(int col)
	{
		switch (col)
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
				return "%12.2f";
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
	/// Returns general data validators for a given column. </summary>
	/// <param name="col"> Column to return validators from. </param>
	/// <returns> List of general Validators. </returns>
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
	/// <param name="col"> the column for which to return data.  This is base 0. </param>
	/// <returns> the data that should be placed in the JTable at the given row and col. </returns>
	public virtual object getValueAt(int row, int col)
	{
		if (_sortOrder != null)
		{
			row = _sortOrder[row];
		}
		StateMod_DiversionRight dr = (StateMod_DiversionRight)_data.get(row);

		switch (col)
		{
			case COL_RIGHT_ID:
				return dr.getID();
			case COL_RIGHT_NAME:
				return dr.getName();
			case COL_STRUCT_ID:
				return dr.getCgoto();
			case COL_ADMIN_NUM:
				return dr.getIrtem();
			case COL_DCR_AMT:
				return new double?(dr.getDcrdiv());
			case COL_ON_OFF:
				return new int?(dr.getSwitch());
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
		widths[COL_RIGHT_NAME] = 18;
		widths[COL_STRUCT_ID] = 9;
		widths[COL_ADMIN_NUM] = 12;
		widths[COL_DCR_AMT] = 7;
		widths[COL_ON_OFF] = 5;
		return widths;
	}

	/// <summary>
	/// Returns whether the cell is editable.  If the table is editable, only
	/// the diversion ID is not editable - this reflects the display being opened from
	/// the diversion station window and maintaining agreement between the two windows. </summary>
	/// <param name="row"> unused. </param>
	/// <param name="col"> the index of the column to check whether it is editable.  This is
	/// base 0. </param>
	/// <returns> whether the cell is editable. </returns>
	public virtual bool isCellEditable(int row, int col)
	{
		if (!__editable || (col == COL_STRUCT_ID))
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
		double dval;
		int ival;
		int index;

		StateMod_DiversionRight dr = (StateMod_DiversionRight)_data.get(row);
		switch (col)
		{
			case COL_RIGHT_ID:
				dr.setID((string)value);
				break;
			case COL_RIGHT_NAME:
				dr.setName((string)value);
				break;
			case COL_STRUCT_ID:
				dr.setCgoto((string)value);
				break;
			case COL_ADMIN_NUM:
				dr.setIrtem((string)value);
				break;
			case COL_DCR_AMT:
				dval = ((double?)value).Value;
				dr.setDcrdiv(dval);
				break;
			case COL_ON_OFF:
				if (value is int?)
				{
					ival = ((int?)value).Value;
					dr.setSwitch(ival);
				}
				else if (value is string)
				{
					string onOff = (string)value;
					index = onOff.IndexOf(" -", StringComparison.Ordinal);
					ival = (Convert.ToInt32(onOff.Substring(0, index)));
					dr.setSwitch(ival);
				}
				break;
		}

		base.setValueAt(value, row, col);
	}

	}

}