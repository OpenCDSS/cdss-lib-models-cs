using System;

// StateMod_ReservoirAreaCap_TableModel - table model for displaying reservoir area/capacity/seepage data

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
// StateMod_ReservoirAreaCap_TableModel - table model for displaying reservoir 
//	area/capacity/seepage data
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
// 2003-06-09	J. Thomas Sapienza, RTi	Initial version.
// 2003-06-11	JTS, RTi		Revised so that it displays real data
//					instead of dummy data for the main
//					Reservoir display.
// 2003-06-13	JTS, RTi		* Created code for displaying the
//					  area cap and climate data
//					* Added code to handle editing
//					  data
// 2003-06-16	JTS, RTi		* Added code for the reservoir account
//					  data
//					* Added code for the reservoir right
//					  data
// 2003-06-17	JTS, RTi		Revised javadocs.
// 2003-07-17	JTS, RTi		Constructor now takes a editable flag
//					to specify whether the data should be
//					editable.
// 2003-07-29	JTS, RTi		JWorksheet_RowTableModel changed to
//					JWorksheet_AbstractRowTableModel.
// 2003-08-16	Steven A. Malers, RTi	Update because of changes in the
//					StateMod_ReservoirClimate class.
// 2003-08-22	JTS, RTi		* Changed headings for the rights table.
//					* Changed headings for the owner account
//					  table.
//					* Added code to accomodate tables which
//					  are now using comboboxes for entering
//					  data.
// 2003-08-25	JTS, RTi		Added partner table code for saving
//					data in the reservoir climate gui.
// 2003-08-28	SAM, RTi		* Change setRightsVector() call to
//					  setRights().
//					* Update for changes in
//					  StateMod_Reservoir.
// 2003-09-18	JTS, RTi		Added ID column for reservoir
//					accounts.
// 2003-10-10	JTS, RTi		* Removed reference to parent reservoir.
//					* Added getColumnToolTips().
// 2004-01-21	JTS, RTi		Removed the row count column and 
//					changed all the other column numbers.
// 2004-10-28	SAM, RTi		Split code out of
//					StateMod_Reservoir_TableModel.
//					Change setValueAt() to support sort.
//					Define tool tips.
// 2005-01-21	JTS, RTi		Added ability to display data for either
//					one or many reservoirs.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
// ----------------------------------------------------------------------------
// EndHeader

namespace DWR.StateMod
{

	using JWorksheet_AbstractRowTableModel = RTi.Util.GUI.JWorksheet_AbstractRowTableModel;

	/// <summary>
	/// This table model displays reservoir area capacity data.  The model can display
	/// area capacity data for a single reservoir or for 1+ reservoirs.  The difference
	/// is specified in the constructor and affects how many columns of data are shown.
	/// </summary>
	public class StateMod_ReservoirAreaCap_TableModel : JWorksheet_AbstractRowTableModel
	{

	/// <summary>
	/// Number of columns in the table model.  For table models that display area
	/// capacity for a single reservoir the tables only have 3 columns.  Table models
	/// that display area capacities for 1+ reservoirs have 4.  The variable is 
	/// modified in the constructor.
	/// </summary>
	private int __COLUMNS = 3;

	/// <summary>
	/// References to columns.
	/// </summary>
	public const int COL_RESERVOIR_ID = -1, COL_CAPACITY = 0, COL_AREA = 1, COL_SEEPAGE = 2;

	/// <summary>
	/// Whether the table data is editable.
	/// </summary>
	private bool __editable = true;

	/// <summary>
	/// Whether the table model should be set up for displaying the rights for only
	/// a single reservoir (true) or for multiple reservoirs (false).  If true, then
	/// the reservoir ID field will not be displayed.  If false, then it will be.
	/// </summary>
	private bool __singleReservoir = true;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="data"> the reservoir area/cap/seepage data that will be displayed in the table. </param>
	/// <param name="editable"> whether the table data can be modified. </param>
	/// <param name="singleReservoir"> if true, then the table model is set up to only display
	/// a single reservoir's right data.  This means that the reservoir ID field will
	/// not be shown.  If false then the reservoir right field will be included. </param>
	/// <exception cref="Exception"> if an invalid data or dmi was passed in. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public StateMod_ReservoirAreaCap_TableModel(java.util.List data, boolean editable, boolean singleReservoir) throws Exception
	public StateMod_ReservoirAreaCap_TableModel(System.Collections.IList data, bool editable, bool singleReservoir)
	{
		if (data == null)
		{
			throw new Exception("Invalid data Vector passed to " + "StateMod_ReservoirAreaCap_TableModel constructor.");
		}
		_rows = data.Count;
		_data = data;

		__editable = editable;
		__singleReservoir = singleReservoir;

		if (!__singleReservoir)
		{
			// this is done because if area capacities for 1+ reservoirs 
			// are shown in the worksheet the ID for the associated 
			// reservoirs needs shown as well.  So instead of the usual 
			// 3 columns of data, an additional one is necessary.
			__COLUMNS++;
		}
	}

	/// <summary>
	/// Returns the class of the data stored in a given column. </summary>
	/// <param name="col"> the column for which to return the data class. </param>
	/// <returns> the class of the data stored in a given column. </returns>
	public virtual Type getColumnClass(int col)
	{
		// necessary for worksheets that display area capacities for 1+ 
		// reservoirs, so that the -1st column (ID) can also be displayed.  By
		// doing it this way, code can be shared between the two kinds of 
		// table models and less maintenance is necessary.
		if (!__singleReservoir)
		{
			col--;
		}

		switch (col)
		{
			case COL_RESERVOIR_ID:
				return typeof(string);
			case COL_CAPACITY:
				return typeof(Double);
			case COL_AREA:
				return typeof(Double);
			case COL_SEEPAGE:
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
	/// <param name="col"> the position of the column for which to return the name. </param>
	/// <returns> the name of the column at the given position. </returns>
	public virtual string getColumnName(int col)
	{
		// necessary for worksheets that display area capacities for 1+ 
		// reservoirs, so that the -1st column (ID) can also be displayed.  By
		// doing it this way, code can be shared between the two kinds of 
		// table models and less maintenance is necessary.
		if (!__singleReservoir)
		{
			col--;
		}

		switch (col)
		{
			case COL_RESERVOIR_ID:
				return "RESERVOIR\nID";
			case COL_CAPACITY:
				return "CONTENT\n(ACFT)";
			case COL_AREA:
				return "AREA\n(ACRE)";
			case COL_SEEPAGE:
				return "SEEPAGE\n(AF/M)";
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

		// the offset is used because in worksheets that have area capacity
		// data for 1+ reservoirs the first column is numbered -1.  The 
		// offset calculation allows this column number, which is normally
		// invalid for worksheets, to be used for those worksheets that need
		// to display the -1st column.
		int offset = 0;
		if (!__singleReservoir)
		{
			offset = 1;
			tips[COL_RESERVOIR_ID + offset] = "<html>The reservoir station ID of the reservoir to "
				+ "which<br>the area capacity information belongs."
				+ "</html>";
		}

		tips[COL_CAPACITY + offset] = "Reservoir content (ACFT).";
		tips[COL_AREA + offset] = "Reservoir area (ACRE).";
		tips[COL_SEEPAGE + offset] = "Reservoir seepage (AF/M).";
		return tips;
	}

	/// <summary>
	/// Returns the format that the specified column should be displayed in when
	/// the table is being displayed in the given table format. </summary>
	/// <param name="col"> column for which to return the format. </param>
	/// <returns> the format (as used by StringUtil.formatString() in which to display the
	/// column. </returns>
	public virtual string getFormat(int col)
	{
		// necessary for worksheets that display area capacities for 1+ 
		// reservoirs, so that the -1st column (ID) can also be displayed.  By
		// doing it this way, code can be shared between the two kinds of 
		// table models and less maintenance is necessary.
		if (!__singleReservoir)
		{
			col--;
		}

		switch (col)
		{
			case COL_RESERVOIR_ID:
				return "%-12s";
			case COL_CAPACITY:
				return "%12.1f";
			case COL_AREA:
				return "%12.1f";
			case COL_SEEPAGE:
				return "%12.1f";
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
	/// Returns the data that should be placed in the JTable at the 
	/// given row and column. </summary>
	/// <param name="row"> the row for which to return data. </param>
	/// <param name="col"> the column for which to return data. </param>
	/// <returns> the data that should be placed in the JTable at the given row and col. </returns>
	public virtual object getValueAt(int row, int col)
	{
		if (_sortOrder != null)
		{
			row = _sortOrder[row];
		}

		StateMod_ReservoirAreaCap ra = (StateMod_ReservoirAreaCap)_data.get(row);

		// necessary for worksheets that display area capacities for 1+ 
		// reservoirs, so that the -1st column (ID) can also be displayed.  By
		// doing it this way, code can be shared between the two kinds of 
		// table models and less maintenance is necessary.
		if (!__singleReservoir)
		{
			col--;
		}

		switch (col)
		{
			case COL_RESERVOIR_ID:
				return ra.getCgoto();
			case COL_CAPACITY:
				return new double?(ra.getConten());
			case COL_AREA:
				return new double?(ra.getSurarea());
			case COL_SEEPAGE:
				return new double?(ra.getSeepage());
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

		// the offset is used because in worksheets that have area capacity
		// data for 1+ reservoirs the first column is numbered -1.  The 
		// offset calculation allows this column number, which is normally
		// invalid for worksheets, to be used for those worksheets that need
		// to display the -1st column.
		int offset = 0;
		if (!__singleReservoir)
		{
			offset = 1;
			widths[COL_RESERVOIR_ID + offset] = 8;
		}

		widths[COL_CAPACITY + offset] = 8;
		widths[COL_AREA + offset] = 8;
		widths[COL_SEEPAGE + offset] = 8;
		return widths;
	}

	/// <summary>
	/// Returns whether the cell at the given position is editable.  In this
	/// table model all columns are editable (unless the table is not editable). </summary>
	/// <param name="rowIndex"> unused </param>
	/// <param name="col"> the index of the column to check for whether it is editable. </param>
	/// <returns> whether the cell at the given position is editable. </returns>
	public virtual bool isCellEditable(int rowIndex, int col)
	{
		// necessary for worksheets that display area capacities for 1+ 
		// reservoirs, so that the -1st column (ID) can also be displayed.  By
		// doing it this way, code can be shared between the two kinds of 
		// table models and less maintenance is necessary.
		if (!__singleReservoir)
		{
			col--;
		}

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
		StateMod_ReservoirAreaCap ra = (StateMod_ReservoirAreaCap)_data.get(row);

		// necessary for worksheets that display area capacities for 1+ 
		// reservoirs, so that the -1st column (ID) can also be displayed.  By
		// doing it this way, code can be shared between the two kinds of 
		// table models and less maintenance is necessary.
		if (!__singleReservoir)
		{
			col--;
		}

		switch (col)
		{
			case COL_RESERVOIR_ID:
				ra.setCgoto((string)value);
				break;
			case COL_CAPACITY:
				dval = ((double?)value).Value;
				ra.setConten(dval);
				break;
			case COL_AREA:
				dval = ((double?)value).Value;
				ra.setSurarea(dval);
				break;
			case COL_SEEPAGE:
				dval = ((double?)value).Value;
				ra.setSeepage(dval);
				break;
		}

		if (!__singleReservoir)
		{
			col++;
		}

		base.setValueAt(value, row, col);
	}

	}

}