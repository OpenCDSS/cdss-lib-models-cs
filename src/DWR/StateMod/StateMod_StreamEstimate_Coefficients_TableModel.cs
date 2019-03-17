using System;

// StateMod_StreamEstimate_Coefficients_TableModel - table model for displaying stream estimate station coefficients data

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
// StateMod_StreamEstimate_Coefficients_TableModel - table model for displaying 
//	stream estimate station coefficients data
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
// 2003-08-20	J. Thomas Sapienza, RTi	Initial version.
// 2003-09-04	Steven A. Malers, RTi	Make headings multi-row.
// 2003-09-11	SAM, RTi		Rename class from
//					StateMod_BaseFlowCoefficents_TableModel
//					to
//					StateMod_StreamEstimate_Coefficients_
//					TableModel.
// 2004-01-22	JTS, RTi		Removed the row count column and 
//					changed all the other column numbers.
// 2004-10-28	SAM, RTi		Change setValueAt() to support sort.
// 2005-01-21	JTS, RTi		Added the editable flag.
// ----------------------------------------------------------------------------
// EndHeader

namespace DWR.StateMod
{

	using JWorksheet_AbstractRowTableModel = RTi.Util.GUI.JWorksheet_AbstractRowTableModel;

	/// <summary>
	/// This table model displays stream estimate station coefficients data.
	/// </summary>
	public class StateMod_StreamEstimate_Coefficients_TableModel : JWorksheet_AbstractRowTableModel
	{

	/// <summary>
	/// Number of columns in the table model.
	/// </summary>
	private readonly int __COLUMNS = 4;

	/// <summary>
	/// References to columns.
	/// </summary>
	public const int COL_STREAM_NAME = 0, COL_UPSTREAM_GAGE = 1, COL_GAIN_TERM_WT = 2, COL_GAIN_TERM_GAGE_ID = 3;

	/// <summary>
	/// Whether data are editable or not.
	/// </summary>
	private bool __editable = true;

	private StateMod_StreamEstimate_Coefficients __coeff = null;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="data"> the data that will be displayed in the table. </param>
	/// <exception cref="Exception"> if an invalid data or dmi was passed in. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public StateMod_StreamEstimate_Coefficients_TableModel(java.util.List data) throws Exception
	public StateMod_StreamEstimate_Coefficients_TableModel(System.Collections.IList data) : this(data, true)
	{
	}

	/// <summary>
	/// Constructor. </summary>
	/// <param name="data"> the data that will be displayed in the table. </param>
	/// <param name="editable"> whether the data are editable or not. </param>
	/// <exception cref="Exception"> if an invalid data or dmi was passed in. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public StateMod_StreamEstimate_Coefficients_TableModel(java.util.List data, boolean editable) throws Exception
	public StateMod_StreamEstimate_Coefficients_TableModel(System.Collections.IList data, bool editable)
	{
		if (data == null)
		{
			throw new Exception("Invalid data Vector passed to " + "StateMod_StreamEstimate_Coefficients_TableModel " + "constructor.");
		}
		_rows = data.Count;
		_data = data;

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
			case COL_STREAM_NAME:
				return typeof(Double);
			case COL_UPSTREAM_GAGE:
				return typeof(string);
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
			case COL_STREAM_NAME:
				return "STREAM\nTERM";
			case COL_UPSTREAM_GAGE:
				return "UPSTREAM\nTERM GAGE";
			case COL_GAIN_TERM_WT:
				return "GAIN TERM\nWEIGHT";
			case COL_GAIN_TERM_GAGE_ID:
				return "GAIN TERM\nGAGE ID";
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
		switch (column)
		{
			case COL_STREAM_NAME:
				return "%8.1f";
			case COL_UPSTREAM_GAGE:
				return "%-40s";
			case COL_GAIN_TERM_WT:
				return "%8.1f";
			case COL_GAIN_TERM_GAGE_ID:
				return "%-40s";
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

		if (__coeff == null)
		{
			return "";
		}

		switch (col)
		{
			case COL_STREAM_NAME:
			case COL_UPSTREAM_GAGE:
			case COL_GAIN_TERM_WT:
			case COL_GAIN_TERM_GAGE_ID:
				int M = __coeff.getM();
				int N = __coeff.getN();
				switch (col)
				{
					case COL_STREAM_NAME:
						if (row < N)
						{
							return new double?(__coeff.getCoefn(row));
						}
						return "";
					case COL_UPSTREAM_GAGE:
						if (row < N)
						{
							return __coeff.getUpper(row);
						}
						return "";
					case COL_GAIN_TERM_WT:
						if (row < M)
						{
							return new double?(__coeff.getCoefm(row));
						}
						return "";
					case COL_GAIN_TERM_GAGE_ID:
						if (row < M)
						{
							return __coeff.getFlowm(row);
						}
						return "";
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
		widths[COL_STREAM_NAME] = 8;
		widths[COL_UPSTREAM_GAGE] = 8;
		widths[COL_GAIN_TERM_WT] = 8;
		widths[COL_GAIN_TERM_GAGE_ID] = 9;
		return widths;
	}

	/// <summary>
	/// Returns whether the cell at the given position is editable or not.  In this
	/// table model all columns above #2 are editable. </summary>
	/// <param name="rowIndex"> unused </param>
	/// <param name="columnIndex"> the index of the column to check for whether it is editable. </param>
	/// <returns> whether the cell at the given position is editable. </returns>
	public virtual bool isCellEditable(int rowIndex, int columnIndex)
	{
		// REVISIT (JTS - 2004-08-26)
		// when we devise some system for nicely editing the
		// coefficient values, this will need enabled
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
		switch (col)
		{
			case COL_STREAM_NAME:
				dval = ((double?)value).Value;
				__coeff.setCoefn(row, dval);
				break;
			case COL_UPSTREAM_GAGE:
				__coeff.setUpper(row, (string)value);
				break;
			case COL_GAIN_TERM_WT:
				dval = ((double?)value).Value;
				__coeff.setCoefm(row, dval);
				break;
			case COL_GAIN_TERM_GAGE_ID:
				__coeff.setFlowm(row, (string)value);
				break;
			default:
				break;
		}

		base.setValueAt(value, row, col);
	}

	public virtual void setStreamEstimateCoefficients(StateMod_StreamEstimate_Coefficients coeff)
	{
		__coeff = coeff;
		if (__coeff != null)
		{
			_rows = StateMod_StreamEstimate_Coefficients.MAX_BASEFLOWS;
		}
		else
		{
			_rows = 0;
		}
	}

	}

}