﻿using System;

// StateMod_Well_TableModel - Table model for displaying data for well station tables

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
// StateMod_Well_TableModel - Table model for displaying data for well station
//	tables
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
// 2003-06-09	J. Thomas Sapienza, RTi	Initial version.
// 2003-06-10	JTS, RTi		* Added 'right' fields.
//					* Added 'return flow' fields.
// 2003-07-29	JTS, RTi		JWorksheet_RowTableModel changed to
//					JWorksheet_AbstractRowTableModel.
// 2003-08-29	Steven A. Malers, RTi	Update for changes in StateMod_Well.
// 2003-10-13	JTS, RTi		Removed references to parent well.
// 2004-01-22	JTS, RTi		Removed the row count column and 
//					changed all the other column numbers.
// 2004-10-28	SAM, RTi		Split out well right code into a
//					separate table model.  Use the generic
//					StateMod_ReturnFlow_TableModel for
//					return flows and depletions.
//					Also add tool tips.
// 2004-10-28	SAM, RTi		Change setValueAt() to support sort.
// 2005-01-18	JTS, RTi		Added columns beyond just ID and name.
// 2005-01-20	JTS, RTi		Following review:
//					* Added new columns.
//					* Changed column order.
// 2006-04-11	JTS, RTi		Corrected the classes returned from
//					getColumnClass().
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace DWR.StateMod
{

	using JWorksheet_AbstractRowTableModel = RTi.Util.GUI.JWorksheet_AbstractRowTableModel;

	/// <summary>
	/// This table model displays well data.
	/// </summary>
	public class StateMod_Well_TableModel : JWorksheet_AbstractRowTableModel
	{

	/// <summary>
	/// Whether the table data is editable or not.
	/// </summary>
	private bool __editable = false;

	/// <summary>
	/// Number of columns in the table model.
	/// </summary>
	private int __COLUMNS = 2;

	/// <summary>
	/// References to columns.
	/// </summary>
	public const int COL_ID = 0, COL_NAME = 1, COL_CAPACITY = 2, COL_DAILY_ID = 3, COL_PRIMARY = 4, COL_DIVERSION_ID = 5, COL_DEMAND_TYPE = 6, COL_EFF_ANNUAL = 7, COL_AREA = 8, COL_USE_TYPE = 9, COL_DEMAND_SOURCE = 10, COL_EFF_01 = 11, COL_EFF_02 = 12, COL_EFF_03 = 13, COL_EFF_04 = 14, COL_EFF_05 = 15, COL_EFF_06 = 16, COL_EFF_07 = 17, COL_EFF_08 = 18, COL_EFF_09 = 19, COL_EFF_10 = 20, COL_EFF_11 = 21, COL_EFF_12 = 22;

	/// <summary>
	/// Whether all the columns are shown (false) or only the ID and name columns are
	/// shown (true).
	/// </summary>
	private bool __compactForm = true;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="data"> the well station data that will be displayed in the table. </param>
	/// <param name="editable"> whether the table data is editable or not </param>
	/// <exception cref="Exception"> if an invalid data or dmi was passed in. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public StateMod_Well_TableModel(java.util.List data, boolean editable) throws Exception
	public StateMod_Well_TableModel(System.Collections.IList data, bool editable) : this(data, editable, true)
	{
	}

	/// <summary>
	/// Constructor. </summary>
	/// <param name="data"> the well station data that will be displayed in the table. </param>
	/// <param name="editable"> whether the table data is editable or not </param>
	/// <param name="compactForm"> whether to only show the ID and name columns (true) or all data columns. </param>
	/// <exception cref="Exception"> if an invalid data or dmi was passed in. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public StateMod_Well_TableModel(java.util.List data, boolean editable, boolean compactForm) throws Exception
	public StateMod_Well_TableModel(System.Collections.IList data, bool editable, bool compactForm)
	{
		if (data == null)
		{
			throw new Exception("Invalid data Vector passed to " + "StateMod_Well_TableModel constructor.");
		}
		_rows = data.Count;
		_data = data;

		__editable = editable;
		__compactForm = compactForm;

		if (__compactForm)
		{
			__COLUMNS = 2;
		}
		else
		{
			__COLUMNS = 11;
		}
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
			case COL_DAILY_ID:
				return typeof(string);
			case COL_CAPACITY:
				return typeof(Double);
			case COL_DIVERSION_ID:
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
			case COL_PRIMARY:
				return typeof(Double);
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
				return "\nID";
			case COL_NAME:
				return "\nNAME";
			case COL_DAILY_ID:
				return "DAILY\nID";
			case COL_CAPACITY:
				return "CAPACITY\n(CFS)\n";
			case COL_DIVERSION_ID:
				return "TIED-TO\nDIVERSION";
			case COL_DEMAND_TYPE:
				return "DEMAND\nCODE";
			case COL_EFF_ANNUAL:
				return "SYSTEM\nEFFICIENCY";
			case COL_AREA:
				return "WELL IRRIG\nAREA";
			case COL_USE_TYPE:
				return "USE\nTYPE";
			case COL_DEMAND_SOURCE:
				return "IRRIGATED\nACRE SOURCE";
			case COL_PRIMARY:
				return "ADMIN. NUM.\nSWITCH";
			case COL_EFF_01:
				return "EFF\nMONTH 1";
			case COL_EFF_02:
				return "EFF\nMONTH 2";
			case COL_EFF_03:
				return "EFF\nMONTH 3";
			case COL_EFF_04:
				return "EFF\nMONTH 4";
			case COL_EFF_05:
				return "EFF\nMONTH 5";
			case COL_EFF_06:
				return "EFF\nMONTH 6";
			case COL_EFF_07:
				return "EFF\nMONTH 7";
			case COL_EFF_08:
				return "EFF\nMONTH 8";
			case COL_EFF_09:
				return "EFF\nMONTH 9";
			case COL_EFF_10:
				return "EFF\nMONTH 10";
			case COL_EFF_11:
				return "EFF\nMONTH 11";
			case COL_EFF_12:
				return "EFF\nMONTH 12";
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

		tips[COL_ID] = "<html>The well station identifier is the main link" +
			" between well data<BR>" +
			"and must be unique in the data set.</html>";
		tips[COL_NAME] = "Well station name.";

		if (__compactForm)
		{
			return tips;
		}

		tips[COL_DAILY_ID] = "<html>Well ID to use for daily data.</html>";
		tips[COL_CAPACITY] = "<html>Well capacity (CFS)</html>";
		tips[COL_DIVERSION_ID] = "<html>Diversion this well is tied to.</html>";
		tips[COL_DEMAND_TYPE] = "<html>Demand code.</html>";
		tips[COL_EFF_ANNUAL] = "<html>System efficiency (%).</html>";
		tips[COL_AREA] = "<html>Irrigated area associated with the well.</html>";
		tips[COL_USE_TYPE] = "<html>Use type.</html>";
		tips[COL_DEMAND_SOURCE] = "<html>Irrigated acreage source.</html>";
		tips[COL_PRIMARY] = "<html>Priority switch.</html>";
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

	/// <summary>
	/// Returns an array containing the widths (in number of characters) that the 
	/// fields in the table should be sized to. </summary>
	/// <returns> an integer array containing the widths for each field. </returns>
	public virtual int[] getColumnWidths()
	{
		int[] widths = new int[__COLUMNS];

		widths[COL_ID] = 9;
		widths[COL_NAME] = 19;

		if (__compactForm)
		{
			return widths;
		}

		widths[COL_DAILY_ID] = 10;
		widths[COL_CAPACITY] = 10;
		widths[COL_DIVERSION_ID] = 10;
		widths[COL_DEMAND_TYPE] = 10;
		widths[COL_EFF_ANNUAL] = 10;
		widths[COL_AREA] = 10;
		widths[COL_USE_TYPE] = 10;
		widths[COL_DEMAND_SOURCE] = 10;
		widths[COL_PRIMARY] = 10;
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
				return "%-12s";
			case COL_NAME:
				return "%-40s";
			case COL_DAILY_ID:
				return "%-40s";
			case COL_CAPACITY:
				return "%10.2f";
			case COL_DIVERSION_ID:
				return "%-40s";
			case COL_DEMAND_TYPE:
				return "%8d";
			case COL_EFF_ANNUAL:
				return "%10.2f";
			case COL_AREA:
				return "%10.2f";
			case COL_USE_TYPE:
				return "%8d";
			case COL_DEMAND_SOURCE:
				return "%8d";
			case COL_PRIMARY:
				return "%10.2f";
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

		StateMod_Well well = (StateMod_Well)_data.get(row);

		switch (col)
		{
			case COL_ID:
				return well.getID();
			case COL_NAME:
				return well.getName();
			case COL_DAILY_ID:
				return well.getCdividyw();
			case COL_CAPACITY:
				return new double?(well.getDivcapw());
			case COL_DIVERSION_ID:
				return well.getIdvcow2();
			case COL_DEMAND_TYPE:
				return new int?(well.getIdvcomw());
			case COL_EFF_ANNUAL:
				return new double?(well.getDivefcw());
			case COL_AREA:
				return new double?(well.getAreaw());
			case COL_USE_TYPE:
				return new int?(well.getIrturnw());
			case COL_DEMAND_SOURCE:
				return new int?(well.getDemsrcw());
			case COL_PRIMARY:
				return new double?(well.getPrimary());
			case COL_EFF_01:
				return new double?(well.getDiveff(0));
			case COL_EFF_02:
				return new double?(well.getDiveff(1));
			case COL_EFF_03:
				return new double?(well.getDiveff(2));
			case COL_EFF_04:
				return new double?(well.getDiveff(3));
			case COL_EFF_05:
				return new double?(well.getDiveff(4));
			case COL_EFF_06:
				return new double?(well.getDiveff(5));
			case COL_EFF_07:
				return new double?(well.getDiveff(6));
			case COL_EFF_08:
				return new double?(well.getDiveff(7));
			case COL_EFF_09:
				return new double?(well.getDiveff(8));
			case COL_EFF_10:
				return new double?(well.getDiveff(9));
			case COL_EFF_11:
				return new double?(well.getDiveff(10));
			case COL_EFF_12:
				return new double?(well.getDiveff(11));
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

		StateMod_Well smw = (StateMod_Well)_data.get(row);

		switch (col)
		{
			case COL_ID:
				smw.setID((string)value);
				break;
			case COL_NAME:
				smw.setName((string)value);
				break;
			case COL_DAILY_ID:
				smw.setCdividyw((string)value);
				break;
			case COL_CAPACITY:
				smw.setDivcapw(((double?)value).Value);
				break;
			case COL_DIVERSION_ID:
				smw.setIdvcow2((string)value);
				break;
			case COL_DEMAND_TYPE:
				smw.setIdvcomw(((int?)value).Value);
				break;
			case COL_EFF_ANNUAL:
				smw.setDivefcw(((double?)value).Value);
				break;
			case COL_AREA:
				smw.setAreaw(((double?)value).Value);
				break;
			case COL_USE_TYPE:
				smw.setIrturnw(((int?)value).Value);
				break;
			case COL_DEMAND_SOURCE:
				smw.setDemsrcw(((int?)value).Value);
				break;
			case COL_PRIMARY:
				smw.setPrimary(((double?)value).Value);
				break;
			case COL_EFF_01:
				smw.setDiveff(0, (double?)value);
				break;
			case COL_EFF_02:
				smw.setDiveff(1, (double?)value);
				break;
			case COL_EFF_03:
				smw.setDiveff(2, (double?)value);
				break;
			case COL_EFF_04:
				smw.setDiveff(3, (double?)value);
				break;
			case COL_EFF_05:
				smw.setDiveff(4, (double?)value);
				break;
			case COL_EFF_06:
				smw.setDiveff(5, (double?)value);
				break;
			case COL_EFF_07:
				smw.setDiveff(6, (double?)value);
				break;
			case COL_EFF_08:
				smw.setDiveff(7, (double?)value);
				break;
			case COL_EFF_09:
				smw.setDiveff(8, (double?)value);
				break;
			case COL_EFF_10:
				smw.setDiveff(9, (double?)value);
				break;
			case COL_EFF_11:
				smw.setDiveff(10, (double?)value);
				break;
			case COL_EFF_12:
				smw.setDiveff(11, (double?)value);
				break;
		}

		base.setValueAt(value, row, col);
	}

	}

}