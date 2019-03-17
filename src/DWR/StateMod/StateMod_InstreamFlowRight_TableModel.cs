using System;

// StateMod_InstreamFlowRight_TableModel - Table model for displaying data in Instream Flow right tables

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
// StateMod_InstreamFlowRight_TableModel - Table model for displaying data in
//	Instream Flow right tables
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
// 2003-06-09	J. Thomas Sapienza, RTi	Initial version.
// 2003-06-10	JTS, RTi		Added right fields.
// 2003-06-19	JTS, RTi		Table model now displays actual data.
// 2003-07-29	JTS, RTi		JWorksheet_RowTableModel changed to
//					JWorksheet_AbstractRowTableModel.
// 2003-08-29	Steven A. Malers, RTi	Change setRightsVector() call to
//					setRights().
// 2003-10-13	JTS, RTi		* Added getColumnToolTips().
//					* Removed reference to the parent
//					  instream flow.
// 2004-01-21	JTS, RTi		Removed the row count column and 
//					changed all the other column numbers.
// 2004-08-26	JTS, RTi		The on/off field now accepts combo box
//					values as well as integers.
// 2004-10-28	SAM, RTi		Split out of
//					StateMod_InstreamFlow_TableModel.
// 					Change setValueAt() to support sort.
// 2005-01-21	JTS, RTi		Added parameter to set whether the 
//					table is displaying several instream
//					flows' worth of rights or only one
//					instream flow's rights.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
// ----------------------------------------------------------------------------
// EndHeader

namespace DWR.StateMod
{

	using JWorksheet_AbstractRowTableModel = RTi.Util.GUI.JWorksheet_AbstractRowTableModel;

	/// <summary>
	/// This table model displays instream flow right data.
	/// </summary>
	public class StateMod_InstreamFlowRight_TableModel : JWorksheet_AbstractRowTableModel
	{

	/// <summary>
	/// Number of columns in the table model.
	/// </summary>
	private int __COLUMNS = 6;

	/// <summary>
	/// References to columns.
	/// </summary>
	public const int COL_ISF_ID = -1, COL_RIGHT_ID = 0, COL_RIGHT_NAME = 1, COL_STRUCT_ID = 2, COL_ADMIN_NUM = 3, COL_DCR_AMT = 4, COL_ON_OFF = 5;

	/// <summary>
	/// Whether the table data is editable or not.
	/// </summary>
	private bool __editable = false;

	/// <summary>
	/// Whether only a single instream flow's rights are being shown in the table (true)
	/// or multiple instream flows' rights are being shown (false).
	/// </summary>
	private bool __singleInstreamFlow = true;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="data"> the data that will be displayed in the table. </param>
	/// <param name="editable"> whether the table data is editable or not </param>
	/// <param name="singleInstreamFlow"> whether a single instream flow's rights are being 
	/// shown in the table (true) or if multiple instream flows' rights are being shown. </param>
	/// <exception cref="Exception"> if an invalid data or dmi was passed in. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public StateMod_InstreamFlowRight_TableModel(java.util.List data, boolean editable, boolean singleInstreamFlow) throws Exception
	public StateMod_InstreamFlowRight_TableModel(System.Collections.IList data, bool editable, bool singleInstreamFlow)
	{
		if (data == null)
		{
			throw new Exception("Invalid data Vector passed to " + "StateMod_InstreamFlowRight_TableModel constructor.");
		}
		_rows = data.Count;
		_data = data;

		__editable = editable;
		__singleInstreamFlow = singleInstreamFlow;

		if (!__singleInstreamFlow)
		{
			__COLUMNS++;
		}
	}

	/// <summary>
	/// Returns the class of the data stored in a given column. </summary>
	/// <param name="columnIndex"> the column for which to return the data class. </param>
	public virtual Type getColumnClass(int columnIndex)
	{
		if (!__singleInstreamFlow)
		{
			columnIndex--;
		}

		switch (columnIndex)
		{
			case COL_ISF_ID:
				return typeof(string);
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
		if (!__singleInstreamFlow)
		{
			columnIndex--;
		}

		switch (columnIndex)
		{
			case COL_ISF_ID:
				return "\n\nINSTREAM\nFLOW ID";
			case COL_RIGHT_ID:
				return "\n\n\nRIGHT ID";
			case COL_RIGHT_NAME:
				return "\n\n\nRIGHT NAME";
			case COL_STRUCT_ID:
				return "INSTREAM FLOW\nSTATION ID\nASSOCIATED"
					+ "\nWITH RIGHT";
				goto case COL_ADMIN_NUM;
			case COL_ADMIN_NUM:
				return "\n\nADMIN\nNUMBER";
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
	/// <returns> the format (as used by StringUtil.formatString() in which to display the
	/// column. </returns>
	public virtual string getFormat(int column)
	{
		if (!__singleInstreamFlow)
		{
			column--;
		}

		switch (column)
		{
			case COL_ISF_ID:
				return "%-40s";
			case COL_RIGHT_ID:
				return "%-40s";
			case COL_RIGHT_NAME:
				return "%-40s";
			case COL_STRUCT_ID:
				return "%-40s";
			case COL_ADMIN_NUM:
				return "%-40s";
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

		if (!__singleInstreamFlow)
		{
			col--;
		}

		StateMod_InstreamFlowRight infr = (StateMod_InstreamFlowRight)_data.get(row);
		switch (col)
		{
			case COL_ISF_ID:
				return infr.getCgoto();
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

		int mod = 0;

		if (!__singleInstreamFlow)
		{
			mod = 1;
			widths[COL_ISF_ID + mod] = 8;
		}

		widths[COL_RIGHT_ID + mod] = 8;
		widths[COL_RIGHT_NAME + mod] = 20;
		widths[COL_STRUCT_ID + mod] = 12;
		widths[COL_ADMIN_NUM + mod] = 8;
		widths[COL_DCR_AMT + mod] = 10;
		widths[COL_ON_OFF + mod] = 8;

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
		if (!__singleInstreamFlow)
		{
			columnIndex--;
		}

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

		if (!__singleInstreamFlow)
		{
			col--;
		}

		switch (col)
		{
			case COL_ISF_ID:
				ifr.setCgoto((string)value);
				break;
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

		if (!__singleInstreamFlow)
		{
			col++;
		}

		base.setValueAt(value, row, col);
	}

	/// <summary>
	/// Returns the text to be assigned to worksheet tooltips. </summary>
	/// <returns> a String array of tool tips. </returns>
	public virtual string[] getColumnToolTips()
	{
		string[] tips = new string[__COLUMNS];

		int mod = 0;

		if (!__singleInstreamFlow)
		{
			mod = 1;
			tips[COL_ISF_ID] = "ID of instream flow for which the right information is "
			+ "displayed.";
		}

		tips[COL_RIGHT_ID + mod] = "<html>The instream flow right ID is typically the "
			+ "instream flow ID<br> followed by .01, .02, "
			+ "etc.</html>";
		tips[COL_RIGHT_NAME + mod] = "Instream flow right name.";
		tips[COL_STRUCT_ID + mod] = "<HTML>The instream flow ID is the link between instream "
			+ " flows and their right<BR>(not editable here).</HTML>";
		tips[COL_ADMIN_NUM + mod] = "<HTML>Lower admininistration numbers indicate greater"
			+ "seniority.<BR>99999 is typical for a very junior"
			+ " right.</html>";
		tips[COL_DCR_AMT + mod] = "Decreed amount (CFS).";
		tips[COL_ON_OFF + mod] = "<HTML>0 = OFF<BR>1 = ON<BR>"
			+ "YYYY indicates to turn on the right in year YYYY."
			+ "<BR>-YYYY indicates to turn off the right in year"
			+ " YYYY.</HTML>";

		return tips;
	}

	}

}