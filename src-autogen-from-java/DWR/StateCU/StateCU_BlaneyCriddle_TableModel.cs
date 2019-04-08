using System;

// StateCU_BlaneyCriddle_TableModel - table model for displaying data for Blaney Criddle worksheets.

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
// StateCU_BlaneyCriddle_TableModel - Table model for displaying data for 
//	Blaney Criddle worksheets.
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
// 2005-01-24	JTS, RTi	Initial version.
// 2005-03-28	JTS, RTi	Adjusted column sizes.
// 2007-01-10   Kurt Tometich, RTi
// 							Fixed the format for the cropName to 
//							30 chars instead of 20.
// 2007-01-10	KAT, RTi	Adding new field Blaney-Criddle Method.
// 2007-03-01	SAM, RTi	Clean up code based on Eclipse feedback.
// 2007-03-04	SAM, RTi	Change method signature consistent with other code.
// ----------------------------------------------------------------------------

namespace DWR.StateCU
{

	using JWorksheet_AbstractRowTableModel = RTi.Util.GUI.JWorksheet_AbstractRowTableModel;
	using Validator = RTi.Util.IO.Validator;
	using Validators = RTi.Util.IO.Validators;

	/// <summary>
	/// This class is a table model for displaying crop char data.
	/// </summary>
	public class StateCU_BlaneyCriddle_TableModel : JWorksheet_AbstractRowTableModel, StateCU_Data_TableModel
	{

	/// <summary>
	/// Number of columns in the table model.
	/// </summary>
	//private final int __COLUMNS = 3;
	private readonly int __COLUMNS = 4;
	/// <summary>
	/// Columns
	/// </summary>
	private readonly int __COL_CROP_NAME = 0, __COL_DAY_PCT = 1, __COL_COEFF = 2, __COL_BCM = 3;

	/// <summary>
	/// This array stores, for each StateCU_BlaneyCriddle object in the _data 
	/// Vector, whether the object has daily (true) or percentage (false) data.
	/// </summary>
	private bool[] __day = null;

	/// <summary>
	/// Whether the data are editable or not.
	/// </summary>
	private bool __editable = true;

	/// <summary>
	/// An array that stores the number of the first row of data for each 
	/// StateCU_BlaneyCriddle object to that object's position in the _data list.
	/// </summary>
	private int[] __firstRows = null;

	/// <summary>
	/// Constructor.  This builds the Model for displaying crop char data </summary>
	/// <param name="data"> the data that will be displayed in the table. </param>
	/// <exception cref="Exception"> if an invalid data or dmi was passed in. </exception>
	public StateCU_BlaneyCriddle_TableModel(System.Collections.IList data) : this(data, true)
	{
	}

	/// <summary>
	/// Constructor.  This builds the Model for displaying crop char data </summary>
	/// <param name="data"> the data that will be displayed in the table. </param>
	/// <param name="editable"> whether the data are editable or not. </param>
	/// <exception cref="Exception"> if an invalid data or dmi was passed in. </exception>
	public StateCU_BlaneyCriddle_TableModel(System.Collections.IList data, bool editable)
	{
		if (data == null)
		{
			_rows = 0;
		}
		else
		{
			initialize(data);
		}

		_data = data;
		__editable = editable;
	}

	/// <summary>
	/// Returns the class of the data stored in a given column. </summary>
	/// <param name="columnIndex"> the column for which to return the data class. </param>
	/// <returns> the class of the data stored in a given column. </returns>
	public virtual Type getColumnClass(int columnIndex)
	{
		switch (columnIndex)
		{
			case __COL_CROP_NAME:
				return typeof(string);
			case __COL_DAY_PCT:
				return typeof(Integer);
			case __COL_COEFF:
				return typeof(Double);
			case __COL_BCM:
				return typeof(Integer);
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
	/// <param name="columnIndex"> the position for which to return the column name. </param>
	/// <returns> the name of the column at the given position. </returns>
	public virtual string getColumnName(int columnIndex)
	{
		switch (columnIndex)
		{
			case __COL_CROP_NAME:
				return "CROP\nNAME";
			case __COL_DAY_PCT:
				return "DAY OR\nPERCENT";
			case __COL_COEFF:
				return "COEFFICIENT";
			case __COL_BCM:
				return "BLANEY\nCRIDDLE\nMETHOD";
		}
		return " ";
	}

	/// <summary>
	/// Returns the tool tips for the columns. </summary>
	/// <returns> the tool tips for the columns. </returns>
	public virtual string[] getColumnToolTips()
	{
		string[] tips = new string[__COLUMNS];
		for (int i = 0; i < __COLUMNS; i++)
		{
			tips[i] = null;
		}

		tips[__COL_CROP_NAME] = "Crop name";
		tips[__COL_DAY_PCT] = "<html>Day of year if Perennial (start, middle, end of month)."
			+ "<br>Percent of year if annual (5% increments).</html>";
		tips[__COL_COEFF] = "Crop coefficient";
		tips[__COL_BCM] = "Blaney-Criddle Method";

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
			case __COL_CROP_NAME:
				return "%-30.30s";
			case __COL_DAY_PCT:
				return "%8d";
			case __COL_COEFF:
				return "%10.2f";
			case __COL_BCM:
				return "%1d";
		}
		return "%8d";
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
		// KTSW must be 0,1,2,3,4 or blank.
		Validator[] KTSW = new Validator[] {Validators.isEquals(new int?(0)), Validators.isEquals(new int?(1)), Validators.isEquals(new int?(2)), Validators.isEquals(new int?(3)), Validators.isEquals(new int?(4)), Validators.isEquals("")};
			Validator[] ktswValidators = new Validator[] {Validators.or(KTSW)};

		switch (col)
		{
			case __COL_CROP_NAME:
				return StateCU_Data_TableModel_Fields.blank;
			case __COL_DAY_PCT:
				return StateCU_Data_TableModel_Fields.nums;
			case __COL_COEFF:
				return StateCU_Data_TableModel_Fields.nums;
			case __COL_BCM:
				return ktswValidators;
			default:
				return no_checks;
		}
	}

	/// <summary>
	/// From AbstractTableMode.  Returns the data that should be placed in the JTable
	/// at the given row and column. </summary>
	/// <param name="row"> the row for which to return data. </param>
	/// <param name="col"> the column for which to return data. </param>
	/// <returns> the data that should be placed in the JTable at the given row and col. </returns>
	public virtual object getValueAt(int row, int col)
	{
		// make sure the row numbers are never sorted ...
		if (_sortOrder != null)
		{
			row = _sortOrder[row];
		}

		int dataPos = lookupVectorPositionForRow(row);

		StateCU_BlaneyCriddle bc = (StateCU_BlaneyCriddle)_data.get(dataPos);

		int num = row - __firstRows[dataPos];

		switch (col)
		{
			case __COL_CROP_NAME:
				return bc.getName();
			case __COL_DAY_PCT:
				if (__day[dataPos])
				{
					return new int?(bc.getNckcp(num));
				}
				else
				{
					return new int?(bc.getNckca(num));
				}
			case __COL_COEFF:
				if (__day[dataPos])
				{
					return new double?(bc.getCkcp(num));
				}
				else
				{
					return new double?(bc.getCkca(num));
				}
			case __COL_BCM:
				return new int?(bc.getKtsw());
		}
		return "";
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
		widths[__COL_CROP_NAME] = 22;
		widths[__COL_DAY_PCT] = 6;
		widths[__COL_COEFF] = 10;
		widths[__COL_BCM] = 6;

		return widths;
	}

	/// <summary>
	/// Sets up internal arrays. </summary>
	/// <param name="data"> the Vector of data (non-null) that will be displayed in the table model. </param>
	private void initialize(System.Collections.IList data)
	{
		int size = data.Count;
		__firstRows = new int[size];
		__day = new bool[size];

		int row = 0;
		StateCU_BlaneyCriddle bc = null;

		for (int i = 0; i < size; i++)
		{
			bc = (StateCU_BlaneyCriddle)data[i];

			__firstRows[i] = row;

			if (bc.getFlag().Equals("Percent", StringComparison.OrdinalIgnoreCase))
			{
				row += 21;
				__day[i] = false;
			}
			else
			{
				row += 25;
				__day[i] = true;
			}
		}

		_rows = row;
	}

	/// <summary>
	/// Returns whether the cell is editable or not.  In this model, all the cells in
	/// columns 3 and greater are editable. </summary>
	/// <param name="rowIndex"> unused. </param>
	/// <param name="columnIndex"> the index of the column to check whether it is editable. </param>
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
	/// Given a row number in the table, returns the object in the _data Vector that
	/// has data displayed at that row. </summary>
	/// <param name="row"> the number of the row in the table. </param>
	/// <returns> the number of the object in the _data Vector which has its data at
	/// the specified row. </returns>
	private int lookupVectorPositionForRow(int row)
	{
		for (int i = 0; i < __firstRows.Length; i++)
		{
			if (row < __firstRows[i])
			{
				return (i - 1);
			}
		}
		return (__firstRows.Length - 1);
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

		int dataPos = lookupVectorPositionForRow(row);

		StateCU_BlaneyCriddle bc = (StateCU_BlaneyCriddle)_data.get(dataPos);

		int num = row - __firstRows[dataPos];

		switch (col)
		{
			case __COL_CROP_NAME:
				bc.setName((string)value);
				break;
			case __COL_DAY_PCT:
				int ival = ((int?)value).Value;
				bc.setCurvePosition(num, ival);
				break;
			case __COL_COEFF:
				double dval = ((double?)value).Value;
				bc.setCurveValue(num, dval);
				break;
			case __COL_BCM:
				int bcmval = ((int?)value).Value;
				bc.setKtsw(bcmval);
				break;
		}

		base.setValueAt(value, row, col);
	}

	}

}