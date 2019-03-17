using System;
using System.Collections.Generic;
using System.Text;

// StateMod_OperationalRight_Data_TableModel - Table model to display operational right data.

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

	/// <summary>
	/// Table model to display operational right data.
	/// </summary>
	public class StateMod_OperationalRight_Data_TableModel : JWorksheet_AbstractRowTableModel
	{

	/// <summary>
	/// Number of columns in the table model.
	/// </summary>
	private readonly int __COLUMNS = 20;

	/// <summary>
	/// References to columns.
	/// </summary>
	public const int COL_ID = 0, COL_NAME = 1, COL_ADMINISTRATION_NUMBER = 2, COL_MONTH_STR_SWITCH = 3, COL_ONOFF_SWITCH = 4, COL_DEST = 5, COL_DEST_ACCOUNT = 6, COL_SOURCE1 = 7, COL_SOURCE1_ACCOUNT = 8, COL_SOURCE2 = 9, COL_SOURCE2_ACCOUNT = 10, COL_RULE_TYPE = 11, COL_REUSE_PLAN = 12, COL_DIVERSION_TYPE = 13, COL_LOSS = 14, COL_LIMIT = 15, COL_START_YEAR = 16, COL_END_YEAR = 17, COL_MONTHLY_SWITCH = 18, COL_INTERVENING_STRUCTURES = 19;
	// TODO SAM 2010-12-13 Evaluate whether monthly switch and intervening structures should be separate columns

	/// <summary>
	/// Whether the gui data is editable or not.
	/// </summary>
	private bool __editable = false;

	/// <summary>
	/// Constructor.  This builds the Model for displaying the diversion data. </summary>
	/// <param name="data"> the data that will be displayed in the table. </param>
	/// <param name="editable"> whether the gui data is editable or not. </param>
	public StateMod_OperationalRight_Data_TableModel(System.Collections.IList data, bool editable)
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
			case COL_ADMINISTRATION_NUMBER:
				return typeof(string);
			case COL_MONTH_STR_SWITCH:
				return typeof(Integer);
			case COL_ONOFF_SWITCH:
				return typeof(Integer);
			case COL_DEST:
				return typeof(string);
			case COL_DEST_ACCOUNT:
				return typeof(string);
			case COL_SOURCE1:
				return typeof(string);
			case COL_SOURCE1_ACCOUNT:
				return typeof(string);
			case COL_SOURCE2:
				return typeof(string);
			case COL_SOURCE2_ACCOUNT:
				return typeof(string);
			case COL_RULE_TYPE:
				return typeof(Integer);
			case COL_REUSE_PLAN:
				return typeof(string);
			case COL_DIVERSION_TYPE:
				return typeof(string);
			case COL_LOSS:
				return typeof(Double);
			case COL_LIMIT:
				return typeof(Double);
			case COL_START_YEAR:
				return typeof(Integer);
			case COL_END_YEAR:
				return typeof(Integer);
			case COL_MONTHLY_SWITCH:
				return typeof(string);
			case COL_INTERVENING_STRUCTURES:
				return typeof(string);
		}
		return typeof(string);
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
				return "\nID";
			case COL_NAME:
				return "\nNAME";
			case COL_ADMINISTRATION_NUMBER:
				return "ADMINISTRATION\nNUMBER";
			case COL_MONTH_STR_SWITCH:
				return "MONTH/STRUCTURE\nSWITCH";
			case COL_ONOFF_SWITCH:
				return "\nON/OFF";
			case COL_DEST:
				return "DESTINATION\nID";
			case COL_DEST_ACCOUNT:
				return "DESTINATION\nACCOUNT";
			case COL_SOURCE1:
				return "SOURCE 1\nID";
			case COL_SOURCE1_ACCOUNT:
				return "SOURCE 1\nACCOUNT";
			case COL_SOURCE2:
				return "SOURCE 2\nID";
			case COL_SOURCE2_ACCOUNT:
				return "SOURCE 2\nACCOUNT";
			case COL_RULE_TYPE:
				return "RULE\nTYPE";
			case COL_REUSE_PLAN:
				return "REUSE\nPLAN ID";
			case COL_DIVERSION_TYPE:
				return "DIVERSION\nTYPE";
			case COL_LOSS:
				return "% TRANSIT\nLOSS";
			case COL_LIMIT:
				return "CAPACITY\nLIMIT";
			case COL_START_YEAR:
				return "START\nYEAR";
			case COL_END_YEAR:
				return "END\nYEAR";
			case COL_MONTHLY_SWITCH:
				return "MONTHLY SWITCH\nVALUES";
			case COL_INTERVENING_STRUCTURES:
				return "INTERVENING\nSTRUCTURE IDs";
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
		switch (column)
		{
			case COL_ID:
				return "%-12.12s";
			case COL_NAME:
				return "%-24.24s";
			case COL_ADMINISTRATION_NUMBER:
				return "%-12.12s";
			case COL_MONTH_STR_SWITCH:
				return "%8.0f";
			case COL_ONOFF_SWITCH:
				return "%8d";
			case COL_DEST:
				return "%-12.12s";
			case COL_DEST_ACCOUNT:
				return "%-8.8s";
			case COL_SOURCE1:
				return "%-12.12s";
			case COL_SOURCE1_ACCOUNT:
				return "%-8.8s";
			case COL_SOURCE2:
				return "%-12.12s";
			case COL_SOURCE2_ACCOUNT:
				return "%-8.8s";
			case COL_RULE_TYPE:
				return "%8d";
			case COL_REUSE_PLAN:
				return "%-12.12s";
			case COL_DIVERSION_TYPE:
				return "%-12.12s";
			case COL_LOSS:
				return "%8.2f";
			case COL_LIMIT:
				return "%12.2f";
			case COL_START_YEAR:
				return "%d";
			case COL_END_YEAR:
				return "%d";
			case COL_MONTHLY_SWITCH:
				return "%24.24s";
			case COL_INTERVENING_STRUCTURES:
				return "%40.40s";
		}
		return "%8s";
	}

	/// <summary>
	/// Returns the number of rows of data in the table.
	/// </summary>
	public virtual int getRowCount()
	{
		return _rows;
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

		StateMod_OperationalRight opr = (StateMod_OperationalRight)_data.get(row);
		switch (col)
		{
			case COL_ID:
				return opr.getID();
			case COL_NAME:
				return opr.getName();
			case COL_ADMINISTRATION_NUMBER:
				return opr.getRtem();
			case COL_MONTH_STR_SWITCH:
				return new int?(opr.getDumx());
			case COL_ONOFF_SWITCH:
				return new int?(opr.getSwitch());
			case COL_DEST:
				return opr.getCiopde();
			case COL_DEST_ACCOUNT:
				return opr.getIopdes();
			case COL_SOURCE1:
				return opr.getCiopso1();
			case COL_SOURCE1_ACCOUNT:
				return opr.getIopsou1();
			case COL_SOURCE2:
				return opr.getCiopso2();
			case COL_SOURCE2_ACCOUNT:
				return opr.getIopsou2();
			case COL_RULE_TYPE:
				return new int?(opr.getItyopr());
			case COL_REUSE_PLAN:
				return opr.getCreuse();
			case COL_DIVERSION_TYPE:
				return opr.getCdivtyp();
			case COL_LOSS:
				return new double?(opr.getOprLoss());
			case COL_LIMIT:
				return new double?(opr.getOprLimit());
			case COL_START_YEAR:
				return new int?(opr.getIoBeg());
			case COL_END_YEAR:
				return new int?(opr.getIoEnd());
			case COL_MONTHLY_SWITCH:
				int[] imonsw = opr.getImonsw();
				if ((imonsw == null) || (imonsw.Length == 0))
				{
					return "";
				}
				else
				{
					StringBuilder b = new StringBuilder();
					for (int i = 0; i < imonsw.Length; i++)
					{
						if (i > 0)
						{
							b.Append(",");
						}
						b.Append("" + imonsw[i]);
					}
					return b.ToString();
				}
			case COL_INTERVENING_STRUCTURES:
				string[] intern = opr.getIntern();
				if ((intern == null) || (intern.Length == 0))
				{
					return "";
				}
				else
				{
					StringBuilder b = new StringBuilder();
					for (int i = 0; i < intern.Length; i++)
					{
						if (i > 0)
						{
							b.Append(",");
						}
						b.Append("" + intern[i]);
					}
					return b.ToString();
				}
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
		for (int i = 0; i < __COLUMNS; i++)
		{
			widths[i] = 0;
		}
		widths[COL_ID] = 12;
		widths[COL_NAME] = 23;
		widths[COL_ADMINISTRATION_NUMBER] = 12;
		widths[COL_MONTH_STR_SWITCH] = 14;
		widths[COL_ONOFF_SWITCH] = 4;
		widths[COL_DEST] = 10;
		widths[COL_DEST_ACCOUNT] = 9;
		widths[COL_SOURCE1] = 10;
		widths[COL_SOURCE1_ACCOUNT] = 9;
		widths[COL_SOURCE2] = 10;
		widths[COL_SOURCE2_ACCOUNT] = 9;
		widths[COL_RULE_TYPE] = 5;
		widths[COL_REUSE_PLAN] = 12;
		widths[COL_DIVERSION_TYPE] = 7;
		widths[COL_LOSS] = 8;
		widths[COL_LIMIT] = 8;
		widths[COL_START_YEAR] = 5;
		widths[COL_END_YEAR] = 5;
		widths[COL_MONTHLY_SWITCH] = 13;
		widths[COL_INTERVENING_STRUCTURES] = 20;

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
		if (columnIndex > 1)
		{
			return true;
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
		if (_sortOrder != null)
		{
			row = _sortOrder[row];
		}
	/* TODO SAM 2010-12-13 Not sure if this is used but if so enable
	
		switch (col) {
			case COL_STRUCTURE_ID:	
				__interns.set(row, value);
				__parentOperationalRight.setInterns(__interns);
			break;
		}
	
		super.setValueAt(value, row, col);	
		*/
	}

	}

}