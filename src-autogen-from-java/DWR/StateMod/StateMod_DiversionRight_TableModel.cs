using System;

// StateMod_DiversionRight_TableModel - table model for displaying data in the diversion right tables.

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
// StateMod_DiversionRight_TableModel - Table model for displaying data in the
//	diversion right tables.
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
// 2003-06-09	J. Thomas Sapienza, RTi	Initial version.
// 2003-06-10	JTS, RTi		* Added the right fields
//					* Added the return flow fields
// 2003-06-17	JTS, RTi		Return flow data is now displayable
//					and editable.
// 2003-07-17	JTS, RTi		Constructor has switch to determine
//					if data is editable.
// 2003-07-29	JTS, RTi		JWorksheet_RowTableModel changed to
//					JWorksheet_AbstractRowTableModel.
// 2003-10-07	JTS, RTi		Changed 'readOnly' to 'editable'.
// 2004-01-21	JTS, RTi		Removed the row count column and 
//					changed all the other column numbers.
// 2004-10-26	SAM, RTi		Split out code from
//					StateMod_Diversion_TableModel.
// 2005-01-21	JTS, RTi		Added ability to display data for either
//					one or many diversions.
// 2005-01-24	JTS, RTi		* Touched up the javadocs.
//					* Removed reference to a dataset.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace DWR.StateMod
{

	using JWorksheet_AbstractRowTableModel = RTi.Util.GUI.JWorksheet_AbstractRowTableModel;

	/// <summary>
	/// This table model displays diversion right data.  The model can display rights
	/// data for a single diversion or for 1+ diversion.  The difference is specified
	/// in the constructor and affects how many columns of data are shown.
	/// </summary>
	public class StateMod_DiversionRight_TableModel : JWorksheet_AbstractRowTableModel
	{

	/// <summary>
	/// Number of columns in the table model.  For table models that display rights
	/// for a single diversion, the tables only have 6 columns.  Table models that
	/// display rights for 1+ diversions have 7.  The variable is modified in the
	/// constructor.
	/// </summary>
	private int __COLUMNS = 6;

	/// <summary>
	/// References to columns.
	/// </summary>
	public const int COL_DIVERSION_ID = -1, COL_RIGHT_ID = 0, COL_RIGHT_NAME = 1, COL_STRUCT_ID = 2, COL_ADMIN_NUM = 3, COL_DCR_AMT = 4, COL_ON_OFF = 5;

	/// <summary>
	/// Whether the table data can be edited.
	/// </summary>
	private bool __editable = true;

	/// <summary>
	/// Whether only one diversion's rights are shown (true) or many diversions' rights
	/// are shown (false).
	/// </summary>
	private bool __singleDiversion = true;

	/// <summary>
	/// Constructor.  This builds the table model for displaying the diversion right data. </summary>
	/// <param name="dataset"> the dataset for the data being displayed. </param>
	/// <param name="data"> the diversion right data that will be displayed in the table. </param>
	/// <param name="editable"> whether the data can be edited </param>
	/// <param name="singleDiversion"> whether data for a single diversion is shown (true)
	/// or data for multiple diversions is shown (false). </param>
	/// <exception cref="Exception"> if an invalid data or dmi was passed in. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public StateMod_DiversionRight_TableModel(java.util.List data, boolean editable, boolean singleDiversion) throws Exception
	public StateMod_DiversionRight_TableModel(System.Collections.IList data, bool editable, bool singleDiversion)
	{
		if (data == null)
		{
			throw new Exception("Invalid data Vector passed to " + "StateMod_DiversionRight_TableModel constructor.");
		}
		_rows = data.Count;
		_data = data;

		__editable = editable;
		__singleDiversion = singleDiversion;

		if (!__singleDiversion)
		{
			// this is done because if rights for 1+ diversions are 
			// shown in the worksheet the ID for the associated diversions 
			// needs shown as well.  So instead of the usual 6 columns
			// of data, an additional one is necessary.
			__COLUMNS++;
		}
	}

	/// <summary>
	/// Returns the class of the data stored in a given column. </summary>
	/// <param name="col"> the column for which to return the data class.  This is
	/// base 0. </param>
	/// <returns> the class of the data stored in a given column. </returns>
	public virtual Type getColumnClass(int col)
	{
		// necessary for table models that display rights for 1+ diversions,
		// so that the -1st column (ID) can also be displayed.  By doing it
		// this way, code can be shared between the two kinds of table models
		// and less maintenance is necessary.
		if (!__singleDiversion)
		{
			col--;
		}

		switch (col)
		{
			case COL_DIVERSION_ID:
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
	/// <param name="col"> the index of the column for which to return the name.  This
	/// is base 0. </param>
	/// <returns> the name of the column at the given position. </returns>
	public virtual string getColumnName(int col)
	{
		// necessary for table models that display rights for 1+ diversions,
		// so that the -1st column (ID) can also be displayed.  By doing it
		// this way, code can be shared between the two kinds of table models
		// and less maintenance is necessary.
		if (!__singleDiversion)
		{
			col--;
		}

		switch (col)
		{
			case COL_DIVERSION_ID:
				return "\nDIVERSION\nID";
			case COL_RIGHT_ID:
				return "\nDIVERSION\nRIGHT ID";
			case COL_RIGHT_NAME:
				return "\nDIVERSION RIGHT\nNAME";
			case COL_STRUCT_ID:
				return "DIVERSION ID\nASSOCIATED\nWITH RIGHT";
			case COL_ADMIN_NUM:
				return "\nADMINISTRATION\nNUMBER";
			case COL_DCR_AMT:
				return "DECREE\nAMOUNT\n(CFS)";
			case COL_ON_OFF:
				return "\nON/OFF\nSWITCH";
			default:
				return " ";
		}
	}

	/// <summary>
	/// Returns the format that the specified column should be displayed in when
	/// the table is being displayed in the given table format. </summary>
	/// <param name="col"> column for which to return the format.  This is base 0. </param>
	/// <returns> the format (as used by StringUtil.formatString() in which to display the
	/// column. </returns>
	public virtual string getFormat(int col)
	{
		// necessary for table models that display rights for 1+ diversions,
		// so that the -1st column (ID) can also be displayed.  By doing it
		// this way, code can be shared between the two kinds of table models
		// and less maintenance is necessary.
		if (!__singleDiversion)
		{
			col--;
		}

		switch (col)
		{
			case COL_DIVERSION_ID:
				return "%-12s";
			case COL_RIGHT_ID:
				return "%-12s";
			case COL_RIGHT_NAME:
				return "%-24s";
			case COL_STRUCT_ID:
				return "%-12s";
			case COL_ADMIN_NUM:
				return "%-40s";
			case COL_DCR_AMT:
				return "%12.1f";
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
	/// Returns the data that should be placed in the JTable at the given row 
	/// and column. </summary>
	/// <param name="row"> the row for which to return data. </param>
	/// <param name="col"> the column for which to return data.  This is base 0. </param>
	/// <returns> the data that should be placed in the JTable at the given row and col. </returns>
	public virtual object getValueAt(int row, int col)
	{
		if (_sortOrder != null)
		{
			row = _sortOrder[row];
		}
		StateMod_DiversionRight dr = (StateMod_DiversionRight)_data.get(row);

		// necessary for table models that display rights for 1+ diversions,
		// so that the -1st column (ID) can also be displayed.  By doing it
		// this way, code can be shared between the two kinds of table models
		// and less maintenance is necessary.
		if (!__singleDiversion)
		{
			col--;
		}

		switch (col)
		{
			case COL_DIVERSION_ID:
				return dr.getCgoto();
			case COL_RIGHT_ID:
				return dr.getID();
			case COL_RIGHT_NAME:
				return dr.getName();
			case COL_STRUCT_ID:
				return dr.getCgoto();
			case COL_ADMIN_NUM:
				return dr.getIrtem();
			case COL_DCR_AMT:
				return new double?(dr.getDcrdiv());
			case COL_ON_OFF:
				return new int?(dr.getSwitch());
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

		// the offset is used because in worksheets that have rights data for 1+
		// diversions the first column is numbered -1.  The offset calculation 
		// allows this column number, which is normally invalid for a table, 
		// to be used only for those worksheets that need to display the 
		// -1st column.
		int offset = 0;
		if (!__singleDiversion)
		{
			offset = 1;
			widths[COL_DIVERSION_ID + offset] = 8;
		}

		widths[COL_RIGHT_ID + offset] = 8;
		widths[COL_RIGHT_NAME + offset] = 18;
		widths[COL_STRUCT_ID + offset] = 9;
		widths[COL_ADMIN_NUM + offset] = 12;
		widths[COL_DCR_AMT + offset] = 7;
		widths[COL_ON_OFF + offset] = 5;
		return widths;
	}

	/// <summary>
	/// Returns whether the cell is editable.  If the table is editable, only
	/// the diversion ID is not editable - this reflects the display being opened from
	/// the diversion station window and maintaining agreement between the two windows. </summary>
	/// <param name="row"> unused. </param>
	/// <param name="col"> the index of the column to check whether it is editable.  This is
	/// base 0. </param>
	/// <returns> whether the cell is editable. </returns>
	public virtual bool isCellEditable(int row, int col)
	{
		// necessary for table models that display rights for 1+ diversions,
		// so that the -1st column (ID) can also be displayed.  By doing it
		// this way, code can be shared between the two kinds of table models
		// and less maintenance is necessary.
		if (!__singleDiversion)
		{
			col--;
		}

		if (!__editable || (col == COL_STRUCT_ID))
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
		double dval;
		int ival;
		int index;

		// necessary for table models that display rights for 1+ diversions,
		// so that the -1st column (ID) can also be displayed.  By doing it
		// this way, code can be shared between the two kinds of table models
		// and less maintenance is necessary.
		if (!__singleDiversion)
		{
			col--;
		}

		StateMod_DiversionRight dr = (StateMod_DiversionRight)_data.get(row);
		switch (col)
		{
			case COL_DIVERSION_ID:
				dr.setCgoto((string)value);
				break;
			case COL_RIGHT_ID:
				dr.setID((string)value);
				break;
			case COL_RIGHT_NAME:
				dr.setName((string)value);
				break;
			case COL_STRUCT_ID:
				dr.setCgoto((string)value);
				break;
			case COL_ADMIN_NUM:
				dr.setIrtem((string)value);
				break;
			case COL_DCR_AMT:
				dval = ((double?)value).Value;
				dr.setDcrdiv(dval);
				break;
			case COL_ON_OFF:
				if (value is int?)
				{
					ival = ((int?)value).Value;
					dr.setSwitch(ival);
				}
				else if (value is string)
				{
					string onOff = (string)value;
					index = onOff.IndexOf(" -", StringComparison.Ordinal);
					ival = (Convert.ToInt32(onOff.Substring(0, index)));
					dr.setSwitch(ival);
				}
				break;
		}

		if (!__singleDiversion)
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

		int offset = 0;
		if (!__singleDiversion)
		{
			offset = 1;
		}

		if (!__singleDiversion)
		{
			tips[COL_DIVERSION_ID + offset] = "<html>The ID of the diversion to which the rights<br>"
				+ "belong.</html>";
		}

		tips[COL_RIGHT_ID + offset] = "<html>The diversion right ID is typically the diversion" +
			" station ID<br> followed by .01, .02, etc.</html>";
		tips[COL_RIGHT_NAME + offset] = "Diversion right name";
		tips[COL_STRUCT_ID + offset] = "<HTML>The diversion ID is the link between diversion stations "
			+ "and their right(s).</HTML>";
		tips[COL_ADMIN_NUM + offset] = "<HTML>Lower admininistration numbers indicate greater " +
			"seniority.<BR>99999 is typical for a very junior" +
			" right.</html>";
		tips[COL_DCR_AMT + offset] = "Decree amount (CFS)";
		tips[COL_ON_OFF + offset] = "<HTML>0 = OFF<BR>1 = ON<BR>" +
			"YYYY indicates to turn on the right in year YYYY."+
			"<BR>-YYYY indicates to turn off the right in year" +
			" YYYY.</HTML>";

		return tips;
	}

	}

}