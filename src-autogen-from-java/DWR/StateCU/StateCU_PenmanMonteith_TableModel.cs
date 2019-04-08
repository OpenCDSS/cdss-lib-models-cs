using System;

// StateCU_PenmanMonteith_TableModel - table model for displaying Penman-Monteith crop coefficients data.

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

namespace DWR.StateCU
{

	using JWorksheet_AbstractRowTableModel = RTi.Util.GUI.JWorksheet_AbstractRowTableModel;
	using Validator = RTi.Util.IO.Validator;

	/// <summary>
	/// This class is a table model for displaying Penman-Monteith crop coefficients data.
	/// </summary>
	public class StateCU_PenmanMonteith_TableModel : JWorksheet_AbstractRowTableModel, StateCU_Data_TableModel
	{

	/// <summary>
	/// Number of columns in the table model.
	/// </summary>
	//private final int __COLUMNS = 3;
	private readonly int __COLUMNS = 4;
	/// <summary>
	/// Columns
	/// </summary>
	private readonly int __COL_CROP_NAME = 0, __COL_GROWTH_STAGE = 1, __COL_PERCENT = 2, __COL_COEFF = 3;

	/// <summary>
	/// Whether the data are editable or not.
	/// </summary>
	private bool __editable = true;

	/// <summary>
	/// An array that stores the number of the first row of data for each 
	/// StateCU_PenmanMonteith object to that object's position in the _data list.
	/// An additional level of lookup is required to determine the growth stage for data lookup.
	/// </summary>
	private int[] __cropFirstRows = null;

	/// <summary>
	/// Constructor.  This builds the model for displaying crop char data </summary>
	/// <param name="data"> the data that will be displayed in the table. </param>
	/// <exception cref="Exception"> if an invalid data. </exception>
	public StateCU_PenmanMonteith_TableModel(System.Collections.IList data) : this(data, true)
	{
	}

	/// <summary>
	/// Constructor.  This builds the Model for displaying crop char data </summary>
	/// <param name="data"> the data that will be displayed in the table. </param>
	/// <param name="editable"> whether the data are editable or not. </param>
	/// <exception cref="Exception"> if an invalid data. </exception>
	public StateCU_PenmanMonteith_TableModel(System.Collections.IList data, bool editable)
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
			case __COL_GROWTH_STAGE:
				return typeof(Integer);
			case __COL_PERCENT:
				return typeof(Double);
			case __COL_COEFF:
				return typeof(Double);
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
			case __COL_GROWTH_STAGE:
				return "GROWTH\nSTAGE";
			case __COL_PERCENT:
				return "\nPERCENT";
			case __COL_COEFF:
				return "\nCOEFFICIENT";
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
		tips[__COL_GROWTH_STAGE] = "Growth stage.";
		tips[__COL_PERCENT] = "<html>Time from start of growth to effective cover (%) or number of days after effective cover.</html>";
		tips[__COL_COEFF] = "Crop coefficient";

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
			case __COL_GROWTH_STAGE:
				return "%1d";
			case __COL_PERCENT:
				return "%5.3f";
			case __COL_COEFF:
				return "%10.3f";
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

		switch (col)
		{
			case __COL_CROP_NAME:
				return StateCU_Data_TableModel_Fields.blank;
			case __COL_GROWTH_STAGE:
				return StateCU_Data_TableModel_Fields.blank;
			case __COL_PERCENT:
				return StateCU_Data_TableModel_Fields.nums;
			case __COL_COEFF:
				return StateCU_Data_TableModel_Fields.nums;
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

		// Position of the data object...
		int dataPos = lookupVectorPositionForRow(row);

		StateCU_PenmanMonteith pm = (StateCU_PenmanMonteith)_data.get(dataPos);

		// Row position in the data object...
		int num = row - __cropFirstRows[dataPos];
		// Which growth stage (0+ index)
		int igs = num / StateCU_PenmanMonteith.getNCoefficientsPerGrowthStage();
		// Which value in the growth stage
		int ipos = num - igs * StateCU_PenmanMonteith.getNCoefficientsPerGrowthStage();

		switch (col)
		{
			case __COL_CROP_NAME:
				return pm.getName();
			case __COL_GROWTH_STAGE:
				return new int?(igs + 1);
			case __COL_PERCENT:
				return new double?(pm.getKcday(igs, ipos));
			case __COL_COEFF:
				return new double?(pm.getKcb(igs, ipos));
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
		widths[__COL_GROWTH_STAGE] = 6;
		widths[__COL_PERCENT] = 6;
		widths[__COL_COEFF] = 10;

		return widths;
	}

	/// <summary>
	/// Sets up internal arrays. </summary>
	/// <param name="data"> the list of data (non-null) that will be displayed in the table model. </param>
	private void initialize(System.Collections.IList data)
	{
		int size = data.Count;
		__cropFirstRows = new int[size];

		int row = 0;
		StateCU_PenmanMonteith kpm;
		for (int i = 0; i < size; i++)
		{
			kpm = (StateCU_PenmanMonteith)data[i];
			__cropFirstRows[i] = row;
			// The number of rows per crop is the number of growth stages times the number of values per stage
			row += kpm.getNGrowthStages() * StateCU_PenmanMonteith.getNCoefficientsPerGrowthStage();
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
	/// <returns> the number of the object in the _data Vector which has its data at the specified row. </returns>
	private int lookupVectorPositionForRow(int row)
	{
		for (int i = 0; i < __cropFirstRows.Length; i++)
		{
			if (row < __cropFirstRows[i])
			{
				return (i - 1);
			}
		}
		return (__cropFirstRows.Length - 1);
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

		StateCU_PenmanMonteith pm = (StateCU_PenmanMonteith)_data.get(dataPos);

		// Row position in the data object...
		int num = row - __cropFirstRows[dataPos];
		// Which growth stage (0+ index)
		int igs = num / StateCU_PenmanMonteith.getNCoefficientsPerGrowthStage();
		// Which value in the growth stage
		int ipos = num - igs * StateCU_PenmanMonteith.getNCoefficientsPerGrowthStage();

		switch (col)
		{
			case __COL_CROP_NAME:
				pm.setName((string)value);
				break;
			case __COL_GROWTH_STAGE:
				/* TODO SAM 2010-03-31 Not editable...
				int gsval = ((Integer)value).intValue();
				pm.setKtsw( gsval );
				*/
				break;
			case __COL_PERCENT:
				double percent = ((double?)value).Value;
				pm.setCurvePosition(igs, ipos, percent);
				break;
			case __COL_COEFF:
				double coeff = ((double?)value).Value;
				pm.setCurveValue(igs, ipos, coeff);
				break;
		}

		base.setValueAt(value, row, col);
	}

	}

}