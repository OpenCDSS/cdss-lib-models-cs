using System;
using System.Collections.Generic;

// StateMod_Response_TableModel - table model for displaying the response table

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
// StateMod_Response_TableModel - table model for displaying the response
//	table
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
// 2003-09-10	J. Thomas Sapienza, RTi	Initial version.
// 2003-09-15	JTS, RTi		Group name is now displayed along with
//					component name in the table model.
// 2003-09-18 	JTS, RTi		* Directory information is no longer
//					  shown in the first row of the model
//					* Only visible components are shown.
// 2003-10-13 	Steven A. Malers, RTi	* Modify headers to be more appropriate.
//					* Don't show the component number -
//					  the table model now has two columns.
// 2004-01-21	JTS, RTi		Removed the row count column and 
//					changed all the other column numbers.
// 2004-06-22	JTS, RTi		Added new column to show dirty status
//					of all components.
// 2004-08-25	JTS, RTi		* Split the first column into two:
//					  data set group and data set component.
//					* Modified column now only marks if
//					  something has changed.
//					* Browsed-for filenames are converted 
//					  to relative paths of the main 
//					  directory.
// 2004-10-28	SAM, RTi		Change setValueAt() to support sort.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
// ----------------------------------------------------------------------------
// EndHeader

namespace DWR.StateMod
{

	using JWorksheet = RTi.Util.GUI.JWorksheet;
	using JWorksheet_AbstractRowTableModel = RTi.Util.GUI.JWorksheet_AbstractRowTableModel;
	using DataSetComponent = RTi.Util.IO.DataSetComponent;
	using IOUtil = RTi.Util.IO.IOUtil;
	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// This table model displays response data.
	/// </summary>
	public class StateMod_Response_TableModel : JWorksheet_AbstractRowTableModel
	{

	/// <summary>
	/// Number of columns in the table model.
	/// </summary>
	private int __COLUMNS = 4;

	/// <summary>
	/// References to columns.
	/// </summary>
	public const int COL_GROUP = 0, COL_COMP = 1, COL_NAME = 2, COL_DIRTY = 3; // Whether the component is dirty

	/// <summary>
	/// The data behind the table.  This array contains the int value of the
	/// data set components's type for each row.
	/// </summary>
	private int[] __data;

	/// <summary>
	/// The dataset for which to display data set component information.
	/// </summary>
	private StateMod_DataSet __dataset;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="dataset"> the dataset for which to display data set component information. </param>
	/// <exception cref="Exception"> if an invalid data or dmi was passed in. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public StateMod_Response_TableModel(StateMod_DataSet dataset) throws Exception
	public StateMod_Response_TableModel(StateMod_DataSet dataset)
	{
		__dataset = dataset;
		// get the array of the ints that refer to the groups in
		// the data set.
		int[] groups = __dataset.getComponentGroupNumbers();

		System.Collections.IList ints = new List<object>();
		DataSetComponent dsc = null;
		System.Collections.IList v = null;

		// Go through each of the groups and get their data out.  Group data
		// consists of the DataSetComponents the group contains.  For each
		// of the group's DataSetComponents, if it has data, then add its
		// component type to the accumulation vector.
		for (int i = 0; i < groups.Length; i++)
		{
			dsc = __dataset.getComponentForComponentType(groups[i]);
			v = (System.Collections.IList)dsc.getData();
			if (v == null)
			{
				v = new List<object>();
			}
			for (int j = 0; j < v.Count; j++)
			{
				dsc = (DataSetComponent)v[j];
				// the following makes sure that the response file 
				// is not added here ... the response file is added
				// below because it must always be in the GUI.
				if (dsc.getComponentType() != StateMod_DataSet.COMP_RESPONSE && dsc.isVisible())
				{
					ints.Add(new int?(dsc.getComponentType()));
				}
			}
		}

		// now transfer the numbers of the DataSetComponents with data into
		// an int array from the Vector.
		__data = new int[ints.Count + 1];
		__data[0] = StateMod_DataSet.COMP_RESPONSE;
		for (int i = 0; i < ints.Count; i++)
		{
			__data[i + 1] = ((int?)ints[i]).Value;
		}

		_rows = __data.Length;
	}

	/// <summary>
	/// Returns the class of the data stored in a given column. </summary>
	/// <param name="columnIndex"> the column for which to return the data class. </param>
	public virtual Type getColumnClass(int columnIndex)
	{
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
	/// <param name="columnIndex"> the position of the column for which to return the name. </param>
	/// <returns> the name of the column at the given position. </returns>
	public virtual string getColumnName(int columnIndex)
	{
		switch (columnIndex)
		{
			case COL_GROUP:
				return "\nDATA GROUP";
			case COL_COMP:
				return "\nDATA SET COMPONENT";
			case COL_NAME:
				return "\nFILE NAME";
			case COL_DIRTY:
				return "ARE DATA\nMODIFIED?";
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

		tips[0] = "<html>Data Group<BR>" +
			"Visible components correspond to current control " +
			"settings.</html>";
		tips[0] = "<html>Data Set Component<BR>" +
			"Visible components correspond to current control " +
			"settings.</html>";
		tips[2] = "<html>The file name is relative to the data set " +
			"directory, or is an absolute path.<BR>" +
			"Specify a blank string if the file name is unknown.</html>";
		tips[3] = "<html>The data modified flag tells whether the component "
			+ "has been modified or not.</html>";

		return tips;
	}

	public virtual string getComponentName(int row)
	{
		return __dataset.getComponentForComponentType(__data[row]).getComponentName();
	}

	public virtual int getComponentTypeForRow(int row)
	{
		return __data[row];
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
			case COL_GROUP:
				return "%-40s";
			case COL_COMP:
				return "%-40s";
			case COL_NAME:
				return "%-40s";
			case COL_DIRTY:
				return "%-40s";
			default:
				return "%-8s";
		}
	}

	/// <summary>
	/// From AbstractTableModel; returns the number of rows of data in the table. </summary>
	/// <returns> the number of rows of data in the table. </returns>
	public virtual int getRowCount()
	{
		return _rows;
	}

	/// <summary>
	/// From AbstractTableModel; returns the data that should be placed in the JTable
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

		switch (col)
		{
			case COL_GROUP:
				return __dataset.getComponentForComponentType(__dataset.lookupComponentGroupTypeForComponent(__data[row])).getComponentName();
			case COL_COMP:
				return __dataset.getComponentForComponentType(__data[row]).getComponentName();
			case COL_NAME:
				return __dataset.getComponentForComponentType(__data[row]).getDataFileName();
			case COL_DIRTY:
				if (__dataset.getComponentForComponentType(__data[row]).isDirty())
				{
					return "YES";
				}
				else
				{
					return "";
				}
				/*
				return "" + __dataset.getComponentForComponentType(
					__data[row]).isDirty();
	//				+ " (" + __data[row] + ")";
				*/
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
		widths[COL_GROUP] = 16;
		widths[COL_COMP] = 28;
		widths[COL_NAME] = 18;
		widths[COL_DIRTY] = 8;
		return widths;
	}

	/// <summary>
	/// Returns whether the cell at the given position is editable or not.  In this
	/// table model, column [1] is editable. </summary>
	/// <param name="rowIndex"> unused </param>
	/// <param name="columnIndex"> the index of the column to check for whether it is editable. </param>
	/// <returns> whether the cell at the given position is editable. </returns>
	public virtual bool isCellEditable(int rowIndex, int columnIndex)
	{
		if (columnIndex == 2)
		{
			return true;
		}
		return false;
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
			case COL_NAME: // File name...
				string s = ((string)value).Trim();
				string dir = __dataset.getDataSetDirectory();
				try
				{
					s = IOUtil.toRelativePath(dir, s);
				}
				catch (Exception e)
				{
					string routine = "StateMod_Response_TableModel"
						+ ".setValue";
					Message.printWarning(2, routine, "Error converting to relative path (" + dir + ", " + s + "):");
					Message.printWarning(2, routine, e);
				}

				if (s.Equals(((string)getValueAt(row, col)).Trim()))
				{
					return;
				}

				__dataset.getComponentForComponentType(__data[row]).setDataFileName(s);
				__dataset.getComponentForComponentType(__data[row]).setDirty(true);
				goto default;
			default:
				break;
		}

		base.setValueAt(value, row, col);
	}

	// TODO SAM 2007-03-01 Evaluate use
	/// <summary>
	/// Sets the worksheet on which this table model is being used. </summary>
	/// <param name="worksheet"> the worksheet on which this table model can be used. </param>
	public virtual void setWorksheet(JWorksheet worksheet)
	{
	}
	}

}