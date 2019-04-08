using System;

// StateMod_TS_TableModel - This class is a table model for time series header
// information for StateMod time series instances

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

namespace DWR.StateMod
{

	using TS = RTi.TS.TS;
	using JWorksheet_AbstractRowTableModel = RTi.Util.GUI.JWorksheet_AbstractRowTableModel;

	/// <summary>
	/// This class is a table model for time series header information for StateMod time series instances,
	/// in particular time series that are NOT standard time series.  Such time series have additional properties
	/// that don't fit into the standard time series, such as "source" and "destination".
	/// By default the sheet will contain row and column numbers.
	/// </summary>
	public class StateMod_TS_TableModel : JWorksheet_AbstractRowTableModel
	{

	/// <summary>
	/// Number of columns in the table model (with the alias).
	/// </summary>
	private int __COLUMNS = 16;

	/// <summary>
	/// Absolute column indices, for column lookups.
	/// Comment out columns that are not relevant (but may be added later).
	/// </summary>
	public readonly int COL_ID = 0;
	//public final int COL_ALIAS = 1;
	public readonly int COL_NAME = 1;
	public readonly int COL_DATA_SOURCE = 2;
	public readonly int COL_DATA_TYPE = 3;
	public readonly int COL_TIME_STEP = 4;
	//public final int COL_SCENARIO = 6;
	//public final int COL_SEQUENCE = 7;
	public readonly int COL_UNITS = 5;
	public readonly int COL_XOP_OPR_TYPE = 6;
	public readonly int COL_XOP_ADMIN_NUM = 7;
	public readonly int COL_XOP_SOURCE1 = 8;
	public readonly int COL_XOP_DESTINATION = 9;
	public readonly int COL_XOP_YEAR_ON = 10;
	public readonly int COL_XOP_YEAR_OFF = 11;
	public readonly int COL_START = 12;
	public readonly int COL_END = 13;
	public readonly int COL_INPUT_TYPE = 14;
	public readonly int COL_INPUT_NAME = 15;

	/// <summary>
	/// Constructor.  This builds the model for displaying the given time series data. </summary>
	/// <param name="data"> the list of TS that will be displayed in the table (null is allowed). </param>
	/// <exception cref="Exception"> if an invalid results passed in. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public StateMod_TS_TableModel(java.util.List data, String fileExt) throws Exception
	public StateMod_TS_TableModel(System.Collections.IList data, string fileExt) : this(data, false)
	{
		// TODO SAM 2013-11-16 Utilize file extension to modify columns or other mechanism to add flexibility
	}

	/// <summary>
	/// Constructor.  This builds the model for displaying the given time series data. </summary>
	/// <param name="data"> the Vector of TS that will be displayed in the table (null is allowed). </param>
	/// <param name="include_alias"> If true, an alias column will be included after the
	/// location column.  The JWorksheet.removeColumn ( COL_ALIAS ) method should be called. </param>
	/// <exception cref="Exception"> if an invalid results passed in. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public StateMod_TS_TableModel(java.util.List data, boolean include_alias) throws Exception
	public StateMod_TS_TableModel(System.Collections.IList data, bool include_alias)
	{
		if (data == null)
		{
			_rows = 0;
		}
		else
		{
			_rows = data.Count;
		}
		_data = data;
	}

	/// <summary>
	/// From AbstractTableModel.  Returns the class of the data stored in a given
	/// column.  All values are treated as strings. </summary>
	/// <param name="columnIndex"> the column for which to return the data class. </param>
	public virtual Type getColumnClass(int columnIndex)
	{
		switch (columnIndex)
		{
			case COL_ID:
				return typeof(string);
			//case COL_ALIAS: return String.class;
			case COL_NAME:
				return typeof(string);
			case COL_DATA_SOURCE:
				return typeof(string);
			case COL_DATA_TYPE:
				return typeof(string);
			case COL_TIME_STEP:
				return typeof(string);
			//case COL_SCENARIO: return String.class;
			//case COL_SEQUENCE: return String.class;
			case COL_UNITS:
				return typeof(string);
			case COL_XOP_OPR_TYPE:
				return typeof(string);
			case COL_XOP_ADMIN_NUM:
				return typeof(string);
			case COL_XOP_SOURCE1:
				return typeof(string);
			case COL_XOP_DESTINATION:
				return typeof(string);
			case COL_XOP_YEAR_ON:
				return typeof(string);
			case COL_XOP_YEAR_OFF:
				return typeof(string);
			case COL_START:
				return typeof(string);
			case COL_END:
				return typeof(string);
			case COL_INPUT_TYPE:
				return typeof(string);
			case COL_INPUT_NAME:
				return typeof(string);
			default:
				return typeof(string);
		}
	}

	/// <summary>
	/// From AbstractTableMode.  Returns the number of columns of data. </summary>
	/// <returns> the number of columns of data. </returns>
	public virtual int getColumnCount()
	{
		return __COLUMNS;
	}

	/// <summary>
	/// From AbstractTableMode.  Returns the name of the column at the given position. </summary>
	/// <returns> the name of the column at the given position. </returns>
	public virtual string getColumnName(int columnIndex)
	{
		switch (columnIndex)
		{
			case COL_ID:
				return "\nID";
			//case COL_ALIAS: return "\nAlias";
			case COL_NAME:
				return "Name/\nDescription";
			case COL_DATA_SOURCE:
				return "Data\nSource";
			case COL_DATA_TYPE:
				return "Data\nType";
			case COL_TIME_STEP:
				return "Time\nStep";
			//case COL_SCENARIO: return "\nScenario";
			//case COL_SEQUENCE: return "Sequence\nID";
			case COL_UNITS:
				return "\nUnits";
			case COL_XOP_OPR_TYPE:
				return "Operating\nRule Type";
			case COL_XOP_ADMIN_NUM:
				return "Administration\nNumber";
			case COL_XOP_SOURCE1:
				return "\nSource 1";
			case COL_XOP_DESTINATION:
				return "\nDestination";
			case COL_XOP_YEAR_ON:
				return "\nYear On";
			case COL_XOP_YEAR_OFF:
				return "\nYear Off";
			case COL_START:
				return "\nStart";
			case COL_END:
				return "\nEnd";
			case COL_INPUT_TYPE:
				return "Input\nType";
			case COL_INPUT_NAME:
				return "Input\nName";
			default:
				return "";
		}
	}

	/// <summary>
	/// Returns the format to display the specified column. </summary>
	/// <param name="column"> column for which to return the format. </param>
	/// <returns> the format (as used by StringUtil.formatString()). </returns>
	public virtual string getFormat(int column)
	{
		switch (column)
		{
			default:
				return "%s";
		}
	}

	/// <summary>
	/// From AbstractTableMode.  Returns the number of rows of data in the table.
	/// </summary>
	public virtual int getRowCount()
	{
		return _rows;
	}

	/// <summary>
	/// From AbstractTableMode.  Returns the data that should be placed in the JTable at the given row and column. </summary>
	/// <param name="row"> the row for which to return data. </param>
	/// <param name="col"> the absolute column for which to return data. </param>
	/// <returns> the data that should be placed in the JTable at the given row and column. </returns>
	public virtual object getValueAt(int row, int col)
	{ // make sure the row numbers are never sorted ...

		if (_sortOrder != null)
		{
			row = _sortOrder[row];
		}

		TS ts = (TS)_data.get(row);
		if (ts == null)
		{
			return "";
		}
		switch (col)
		{
			case COL_ID:
				return ts.getIdentifier().getLocation();
			//case COL_ALIAS: return ts.getAlias();
			case COL_NAME:
				return ts.getDescription();
			case COL_DATA_SOURCE:
				return ts.getIdentifier().getSource();
			case COL_DATA_TYPE:
				return ts.getDataType();
			case COL_TIME_STEP:
				return ts.getIdentifier().getInterval();
			//case COL_SCENARIO: return ts.getIdentifier().getScenario();
			//case COL_SEQUENCE: return ts.getIdentifier().getSequenceID();
			case COL_UNITS:
				return ts.getDataUnits();
			case COL_XOP_OPR_TYPE:
				return ts.getProperty("OprType") == null ? "" : "" + ts.getProperty("OprType");
			case COL_XOP_ADMIN_NUM:
				return ts.getProperty("AdminNum") == null ? "" : "" + ts.getProperty("AdminNum");
			case COL_XOP_SOURCE1:
				return ts.getProperty("Source1") == null ? "" : "" + ts.getProperty("Source1");
			case COL_XOP_DESTINATION:
				return ts.getProperty("Destination") == null ? "" : "" + ts.getProperty("Destination");
			case COL_XOP_YEAR_ON:
				return ts.getProperty("YearOn") == null ? "" : "" + ts.getProperty("YearOn");
			case COL_XOP_YEAR_OFF:
				return ts.getProperty("YearOff") == null ? "" : "" + ts.getProperty("YearOff");
			case COL_START:
				return ts.getDate1();
			case COL_END:
				return ts.getDate2();
			case COL_INPUT_TYPE:
				return ts.getIdentifier().getInputType();
			case COL_INPUT_NAME:
				return ts.getIdentifier().getInputName();
			default:
				return "";
		}
	}

	/// <summary>
	/// Returns an array containing the column widths (in number of characters). </summary>
	/// <returns> an integer array containing the widths for each field. </returns>
	public virtual int[] getColumnWidths()
	{
		int[] widths = new int[__COLUMNS];
		widths[COL_ID] = 12;
		//widths[COL_ALIAS] = 12;
		widths[COL_NAME] = 28;
		widths[COL_DATA_SOURCE] = 10;
		widths[COL_DATA_TYPE] = 8;
		widths[COL_TIME_STEP] = 8;
		//widths[COL_SCENARIO] = 8;
		//widths[COL_SEQUENCE] = 8;
		widths[COL_UNITS] = 8;
		widths[COL_XOP_OPR_TYPE] = 7;
		widths[COL_XOP_ADMIN_NUM] = 10;
		widths[COL_XOP_SOURCE1] = 8;
		widths[COL_XOP_DESTINATION] = 8;
		widths[COL_XOP_YEAR_ON] = 6;
		widths[COL_XOP_YEAR_OFF] = 6;
		widths[COL_START] = 10;
		widths[COL_END] = 10;
		widths[COL_INPUT_TYPE] = 12;
		widths[COL_INPUT_NAME] = 20;
		return widths;
	}

	}

}