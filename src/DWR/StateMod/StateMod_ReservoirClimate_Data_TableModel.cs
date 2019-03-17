using System;
using System.Collections.Generic;

// StateMod_ReservoirClimate_Data_TableModel - table model for displaying reservoir climate station assignment data

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
// StateMod_ReservoirClimate_Data_TableModel - table model for displaying 
//	reservoir climate station assignment data
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
//					Add tool tips.
// 2005-01-21	JTS, RTi		Added ability to display data for either
//					one or many reservoirs.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
// ----------------------------------------------------------------------------
// EndHeader

namespace DWR.StateMod
{

	using JWorksheet_AbstractRowTableModel = RTi.Util.GUI.JWorksheet_AbstractRowTableModel;

	/// <summary>
	/// This table model displays reservoir climate station assignment data.  The model 
	/// can display climate data for a single reservoir or for 1+ reservoirs.  The
	/// difference is specified in the constructor and affects how many columns of data are shown.
	/// </summary>
	public class StateMod_ReservoirClimate_Data_TableModel : JWorksheet_AbstractRowTableModel
	{

	/// <summary>
	/// Number of columns in the table model.  For table models that display climate
	/// data for a single reservoir, the tables only have 2 columns.  Table models that
	/// display climate data for 1+ reservoirs have 3.  The variable is modified in the constructor.
	/// </summary>
	private int __COLUMNS = 3;

	/// <summary>
	/// References to columns.
	/// </summary>
	public const int COL_RESERVOIR_ID = 0, COL_STATION = 1, COL_PCT_WEIGHT = 2;

	/// <summary>
	/// Whether the table data is editable.
	/// </summary>
	private bool __editable = true;

	/// <summary>
	/// For the reservoir climate gui, the other table model of data that is displaying
	/// station information.  Two table models are shown, one for precipitation
	/// stations and one for evaporation stations.
	/// TODO (JTS - 2005-01-25) is this even needed anymore by stuff that uses this table model??
	/// </summary>
	// TODO SAM 2007-03-01 Evaluate use
	//private StateMod_ReservoirClimate_Data_TableModel __partnerModel;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="data"> the StateMod_ReservoirClimate data that will be displayed in the table. </param>
	/// <param name="editable"> whether the table data can be modified. </param>
	public StateMod_ReservoirClimate_Data_TableModel(System.Collections.IList data, bool editable)
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
	/// <param name="col"> the column for which to return the data class. </param>
	/// <returns> the class of the data stored in a given column. </returns>
	public virtual Type getColumnClass(int col)
	{
		switch (col)
		{
			case COL_RESERVOIR_ID:
				return typeof(string);
			case COL_STATION:
				return typeof(string);
			case COL_PCT_WEIGHT:
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
		switch (col)
		{
			case COL_RESERVOIR_ID:
				return "RESERVOIR ID";
			case COL_STATION:
				return "STATION ID";
			case COL_PCT_WEIGHT:
				return "WEIGHT (%)";
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

		tips[COL_RESERVOIR_ID] = "<html>The reservoir station ID of the reservoir to "
			+ "which<br>the climate data belong.</html>";
		tips[COL_STATION] = "Station identifier.";
		tips[COL_PCT_WEIGHT] = "Weight for station's data (%).";
		return tips;
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
			case COL_RESERVOIR_ID:
				return "%-12.12s";
			case COL_STATION:
				return "%-12.12s";
			case COL_PCT_WEIGHT:
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

		StateMod_ReservoirClimate cl = (StateMod_ReservoirClimate)_data.get(row);

		switch (col)
		{
			case COL_RESERVOIR_ID:
				return cl.getCgoto();
			case COL_STATION:
				return cl.getID();
			case COL_PCT_WEIGHT:
				return new double?(cl.getWeight());
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

		widths[COL_RESERVOIR_ID] = 10;
		widths[COL_STATION] = 8;
		widths[COL_PCT_WEIGHT] = 8;

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
		StateMod_ReservoirClimate cl = (StateMod_ReservoirClimate)_data.get(row);

		switch (col)
		{
			case COL_RESERVOIR_ID:
				cl.setCgoto((string)value);
				break;
			case COL_STATION:
				string s = (string)value;
				int index = s.IndexOf(" - ", StringComparison.Ordinal);
				if (index > -1)
				{
					s = s.Substring(0, index);
				}
				cl.setID(s);
				break;
			case COL_PCT_WEIGHT:
				dval = ((double?)value).Value;
				cl.setWeight(dval);
				break;
		}

		base.setValueAt(value, row, col);
	}

	}

}