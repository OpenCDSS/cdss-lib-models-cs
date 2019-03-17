using System;
using System.Collections.Generic;

// StateMod_StreamEstimateCoefficients_Data_TableModel - table model for displaying stream estimate station coefficients data

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
// StateMod_StreamEstimateCoefficients_Data_TableModel - table model for 
//	displaying stream estimate station coefficients data
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
//
// 2005-04-05	J. Thomas Sapienza, RTi	Initial version.
// 2007-04-27	Kurt Tometich, RTi		Added getValidators method for check
//									file and data check implementation.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace DWR.StateMod
{

	using JWorksheet_AbstractRowTableModel = RTi.Util.GUI.JWorksheet_AbstractRowTableModel;
	using Validator = RTi.Util.IO.Validator;

	/// <summary>
	/// This table model displays stream estimate station coefficients data.
	/// </summary>
	public class StateMod_StreamEstimateCoefficients_Data_TableModel : JWorksheet_AbstractRowTableModel, StateMod_Data_TableModel
	{

	/// <summary>
	/// Number of columns in the table model.
	/// </summary>
	private readonly int __COLUMNS = 6;

	/// <summary>
	/// References to columns.
	/// </summary>
	public const int COL_ID = 0, COL_STREAM_NAME = 1, COL_UPSTREAM_GAGE = 2, COL_GAIN_TERM_PRO = 3, COL_GAIN_TERM_WT = 4, COL_GAIN_TERM_GAGE_ID = 5;

	/// <summary>
	/// Whether data are editable or not.
	/// </summary>
	private bool __editable = true;

	/// <summary>
	/// The data displayed in the table (calculated by setupData()).
	/// </summary>
	private System.Collections.IList[] __data = null;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="data"> the data that will be displayed in the table. </param>
	/// <param name="editable"> whether the data are editable or not. </param>
	public StateMod_StreamEstimateCoefficients_Data_TableModel(System.Collections.IList data, bool editable)
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

		setupData(_data);

		__editable = editable;
	}

	/// <summary>
	/// From AbstractTableModel; returns the class of the data stored in a given
	/// column. </summary>
	/// <param name="columnIndex"> the column for which to return the data class. </param>
	public virtual Type getColumnClass(int columnIndex)
	{
		switch (columnIndex)
		{
			case COL_ID:
				return typeof(string);
			case COL_STREAM_NAME:
				return typeof(Double);
			case COL_UPSTREAM_GAGE:
				return typeof(string);
			case COL_GAIN_TERM_PRO:
				return typeof(Double);
			case COL_GAIN_TERM_WT:
				return typeof(Double);
			case COL_GAIN_TERM_GAGE_ID:
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
	/// <param name="columnIndex"> the position of the column for which to return the name. </param>
	/// <returns> the name of the column at the given position. </returns>
	public virtual string getColumnName(int columnIndex)
	{
		switch (columnIndex)
		{
			case COL_ID:
				return "\n\nID";
			case COL_STREAM_NAME:
				return "\nSTREAM\nTERM";
			case COL_UPSTREAM_GAGE:
				return "\nUPSTREAM\nTERM GAGE";
			case COL_GAIN_TERM_PRO:
				return "GAIN TERM\nPRORATION\nFACTOR";
			case COL_GAIN_TERM_WT:
				return "\nGAIN TERM\nWEIGHT";
			case COL_GAIN_TERM_GAGE_ID:
				return "\nGAIN TERM\nGAGE ID";
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
		widths[COL_ID] = 9;
		widths[COL_STREAM_NAME] = 8;
		widths[COL_UPSTREAM_GAGE] = 8;
		widths[COL_GAIN_TERM_PRO] = 8;
		widths[COL_GAIN_TERM_WT] = 8;
		widths[COL_GAIN_TERM_GAGE_ID] = 9;
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
			case COL_STREAM_NAME:
				return "%8.1f";
			case COL_UPSTREAM_GAGE:
				return "%-12.12s";
			case COL_GAIN_TERM_PRO:
				return "%8.1f";
			case COL_GAIN_TERM_WT:
				return "%8.1f";
			case COL_GAIN_TERM_GAGE_ID:
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

		// TODO KAT 2007-04-16 need to find out which general validators are needed here ...

		return no_checks;
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

		return __data[col][row];
	}

	/// <summary>
	/// Returns whether the cell at the given position is editable or not. </summary>
	/// <param name="rowIndex"> unused </param>
	/// <param name="columnIndex"> the index of the column to check for whether it is editable. </param>
	/// <returns> whether the cell at the given position is editable. </returns>
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
		switch (col)
		{
			case COL_ID:
			case COL_STREAM_NAME:
			case COL_UPSTREAM_GAGE:
			case COL_GAIN_TERM_PRO:
			case COL_GAIN_TERM_WT:
			case COL_GAIN_TERM_GAGE_ID:
			default:
				break;
		}
		base.setValueAt(value, row, col);
	}

	/// <summary>
	/// Sets up the data to be displayed in the table. </summary>
	/// <param name="data"> a Vector of StateMod_StreamEstimate_Coefficients objects from 
	/// which the data to be be displayed in the table will be gathered. </param>
	private void setupData(System.Collections.IList data)
	{
		int num = 0;
		int size = data.Count;
		StateMod_StreamEstimate_Coefficients coeff = null;
		__data = new System.Collections.IList[__COLUMNS];
		for (int i = 0; i < __COLUMNS; i++)
		{
			__data[i] = new List<object>();
		}

		string id = null;
		int rowCount = 0;
		int M = 0;
		int N = 0;
		for (int i = 0; i < size; i++)
		{
			coeff = (StateMod_StreamEstimate_Coefficients)data[i];
			id = coeff.getID();
			M = coeff.getM();
			N = coeff.getN();
			num = M < N ? N : M;
			for (int j = 0; j < num; j++)
			{
				__data[COL_ID].Add(id);

				if (j < N)
				{
					__data[COL_STREAM_NAME].Add(new double?(coeff.getCoefn(j)));
					__data[COL_UPSTREAM_GAGE].Add(coeff.getUpper(j));
				}
				else
				{
					__data[COL_STREAM_NAME].Add(new double?(-999));
					__data[COL_UPSTREAM_GAGE].Add("");
				}

				if (j < M)
				{
					__data[COL_GAIN_TERM_PRO].Add(new double?(coeff.getProratnf()));
					__data[COL_GAIN_TERM_WT].Add(new double?(coeff.getCoefm(j)));
					__data[COL_GAIN_TERM_GAGE_ID].Add(coeff.getFlowm(j));
				}
				else
				{
					__data[COL_GAIN_TERM_PRO].Add(new double?(-999));
					__data[COL_GAIN_TERM_WT].Add(new double?(-999));
					__data[COL_GAIN_TERM_GAGE_ID].Add("");
				}
				rowCount++;
			}
		}
		_rows = rowCount;
	}

	}

}