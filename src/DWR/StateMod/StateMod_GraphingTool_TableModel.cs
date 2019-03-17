using System;
using System.Collections.Generic;

// StateMod_GraphingTool_TableModel - Table model for displaying data for graphing tool-related tables

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
// StateMod_GraphingTool_TableModel - Table model for displaying data for 
//	graphing tool-related tables
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
// 2003-07-07	J. Thomas Sapienza, RTi	Initial version.
// 2003-07-29	JTS, RTi		JWorksheet_RowTableModel changed to
//					JWorksheet_AbstractRowTableModel.
// 2003-10-26	SAM, RTi		* Decrease width of ID column.
//					* Add interval to allow monthly and
//					  daily time series to be added to
//					  graphs (and together).
//					* Add Input Type to emphasize whether
//					  StateMod input text file or binary
//					  output file.
//					* Change scenario to Input Name.
//					* Define column positions as integer
//					  data members to make references to
//					  columns easier.
//					* Use TSIdent to manage rows of data
//					  instead of StateMod_GraphNode.  The
//					  TSIdent alias is used to store the
//					  station type.
//					* Move lookup data from
//					  StateMod_GraphNode to StateMod_Util.
//					* Pass a StateMod_DataSet instance to
//					  the constructor to be able to make
//					  more intelligent decisions in the
//					  graph choices.
// 2003-11-05	JTS, RTi		* Did work on making entered values
//					  automatically select values in other
//					  columns.
//					* Added setInternalValueAt().
//					* Added browseForFile().
// 2003-11-14	SAM, RTi		Enable file formats other than B43
// 2003-11-29	SAM, RTi		Add reservoir accounts to the
//					reservoir identifiers.  Each reservoir
//					is listed without an account for the
//					total and also is listed for each
//					account.
// 2004-01-21	JTS, RTi		Removed the row count column and 
//					changed all the other column numbers.
// 2004-10-28	SAM, RTi		Change setValueAt() to support sort.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
// ----------------------------------------------------------------------------
// EndHeader

namespace DWR.StateMod
{

	using TSIdent = RTi.TS.TSIdent;
	using JFileChooserFactory = RTi.Util.GUI.JFileChooserFactory;
	using JGUIUtil = RTi.Util.GUI.JGUIUtil;
	using JWorksheet = RTi.Util.GUI.JWorksheet;
	using JWorksheet_AbstractRowTableModel = RTi.Util.GUI.JWorksheet_AbstractRowTableModel;
	using IOUtil = RTi.Util.IO.IOUtil;
	using StringUtil = RTi.Util.String.StringUtil;
	using TimeInterval = RTi.Util.Time.TimeInterval;

	/// <summary>
	/// This table model display graphing tool data.
	/// </summary>
	public class StateMod_GraphingTool_TableModel : JWorksheet_AbstractRowTableModel
	{

	private readonly int __COLUMNS = 6; // The number of columns.
	protected internal readonly int _COL_STATION_TYPE = 0;
	protected internal readonly int _COL_ID = 1;
	protected internal readonly int _COL_INTERVAL = 2;
	protected internal readonly int _COL_DATA_TYPE = 3;
	protected internal readonly int _COL_INPUT_TYPE = 4;
	protected internal readonly int _COL_INPUT_NAME = 5;

	private readonly string __BROWSE_INPUT_NAME_ABSOLUTE = "Browse for file (absolute path)...";
	private readonly string __BROWSE_INPUT_NAME_RELATIVE = "Browse for file (relative path)...";

	/// <summary>
	/// The parent frame on which the JWorksheet for this model is displayed.
	/// </summary>
	private StateMod_GraphingTool_JFrame __parent = null;

	/// <summary>
	/// The worksheet in which this table model is working.
	/// </summary>
	private JWorksheet __worksheet;

	/// <summary>
	/// The StateMod data set that is being processed.
	/// </summary>
	private StateMod_DataSet __dataset = null;

	/// <summary>
	/// Lists of data for filling ID lists.
	/// </summary>
	private System.Collections.IList __diversions, __instreamFlows, __reservoirs, __streamEstimateStations, __streamGageStations, __wells;

	/// <summary>
	/// ID lists to be displayed in the combo boxes.
	/// </summary>
	private System.Collections.IList __diversionIDs = null, __instreamFlowIDs = null, __reservoirIDs = null, __streamEstimateStationIDs = null, __streamGageStationIDs = null, __wellIDs = null;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="parent"> the StateMod_GraphingTool_JFrame in which the table is displayed. </param>
	/// <param name="dataset"> the dataset containing the data </param>
	/// <param name="data"> the data to display in the worksheet. </param>
	/// <exception cref="Exception"> if an invalid data was passed in. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public StateMod_GraphingTool_TableModel(StateMod_GraphingTool_JFrame parent, StateMod_DataSet dataset, java.util.List data) throws Exception
	public StateMod_GraphingTool_TableModel(StateMod_GraphingTool_JFrame parent, StateMod_DataSet dataset, System.Collections.IList data)
	{
		__parent = parent;

		__dataset = dataset;
		__reservoirs = (System.Collections.IList)__dataset.getComponentForComponentType(StateMod_DataSet.COMP_RESERVOIR_STATIONS).getData();
		__diversions = (System.Collections.IList)__dataset.getComponentForComponentType(StateMod_DataSet.COMP_DIVERSION_STATIONS).getData();
		__instreamFlows = (System.Collections.IList)__dataset.getComponentForComponentType(StateMod_DataSet.COMP_INSTREAM_STATIONS).getData();
		__wells = (System.Collections.IList)__dataset.getComponentForComponentType(StateMod_DataSet.COMP_WELL_STATIONS).getData();
		__streamGageStations = (System.Collections.IList)__dataset.getComponentForComponentType(StateMod_DataSet.COMP_STREAMGAGE_STATIONS).getData();
		__streamEstimateStations = (System.Collections.IList)__dataset.getComponentForComponentType(StateMod_DataSet.COMP_STREAMESTIMATE_STATIONS).getData();

		if (data == null)
		{
			throw new Exception("Invalid data Vector passed to StateMod_GraphingTool_TableModel constructor.");
		}
		_rows = data.Count;
		_data = data;
	}

	/// <summary>
	/// Checks whether enough values have been entered in the current last row to tell
	/// whether a new row can be added.  A new row can only be added if values have 
	/// been set for columns COL_STATION_TYPE and COL_ID. </summary>
	/// <returns> whether it is valid to add a new row. </returns>
	public virtual bool canAddNewRow()
	{
		int rows = getRowCount();

		if (rows == 0)
		{
			return true;
		}

		string type = (string)getValueAt((rows - 1), _COL_STATION_TYPE);
		string id = (string)getValueAt((rows - 1), _COL_ID);

		if (string.ReferenceEquals(type, null) || type.Trim().Equals(""))
		{
			return false;
		}
		if (string.ReferenceEquals(id, null) || id.Trim().Equals(""))
		{
			return false;
		}

		return true;
	}

	/// <summary>
	/// Creates a list of the available IDs for a Vector of StateMod_Data-extending
	/// objects.  Reservoirs will include an identifier for each reservoir total and each account for the reservoir. </summary>
	/// <param name="nodes"> the nodes for which to create a list of IDs. </param>
	/// <param name="include_accounts"> If true, the </param>
	/// <returns> a Vector of Strings, each of which contains an ID followed by the name of Structure in parentheses </returns>
	private System.Collections.IList createAvailableIDsList(System.Collections.IList nodes)
	{
		System.Collections.IList v = new List<object>();

		int num = 0;
		bool is_reservoir = false; // To allow check below
		if (nodes != null)
		{
			num = nodes.Count;
			if ((num > 0) && ((StateMod_Data)nodes[0])is StateMod_Reservoir)
			{
				is_reservoir = true;
			}
		}

		StateMod_Reservoir res = null; // These are used if reservoirs.
		int nowner = 0;
		int ia = 0;
		for (int i = 0; i < num; i++)
		{
			// Add the normal item...
			v.Add(StateMod_Util.formatDataLabel(((StateMod_Data)nodes[i]).getID(), ((StateMod_Data)nodes[i]).getName()));
			if (is_reservoir)
			{
				// Also add reservoir owner/accounts...
				res = (StateMod_Reservoir)nodes[i];
				nowner = res.getAccounts().Count;
				for (ia = 0; ia < nowner; ia++)
				{
					v.Add(StateMod_Util.formatDataLabel(res.getID() + "-" + (ia + 1), res.getName() + " - " + res.getAccount(ia).getName()));
				}
			}
		}
		return v;
	}

	/// <summary>
	/// Fills the data type column according to the type of structure selected,
	/// the ID of that structure, and the interval that is selected. </summary>
	/// <param name="row"> the row of the data type column that is being dealt with </param>
	/// <param name="outputOnly"> whether to only display OUTPUT data types. </param>
	/// <param name="station_type"> the type of the station (column _COL_STATION_TYPE) </param>
	/// <param name="id"> the ID of the station (column _COL_ID) </param>
	/// <param name="interval_string"> the data interval (column _COL_INTERVAL ) </param>
	public virtual void fillDataTypeColumn(int row, bool outputOnly, string station_type, string id, string interval_string)
	{
		System.Collections.IList dataTypes = new List<object>();
		int interval = TimeInterval.MONTH;
		if (interval_string.Equals("Day", StringComparison.OrdinalIgnoreCase))
		{
			interval = TimeInterval.DAY;
		}

		if (station_type.Equals(StateMod_Util.STATION_TYPE_DIVERSION, StringComparison.OrdinalIgnoreCase))
		{
			dataTypes = StateMod_Util.getTimeSeriesDataTypes(StateMod_DataSet.COMP_DIVERSION_STATIONS, id, __dataset, "", interval, true, true, true, true, false, true);
		}
		else if (station_type.Equals(StateMod_Util.STATION_TYPE_INSTREAM_FLOW, StringComparison.OrdinalIgnoreCase))
		{
			dataTypes = StateMod_Util.getTimeSeriesDataTypes(StateMod_DataSet.COMP_INSTREAM_STATIONS, id, __dataset, "", interval, true, true, true, true, false, true);
		}
		else if (station_type.Equals(StateMod_Util.STATION_TYPE_RESERVOIR, StringComparison.OrdinalIgnoreCase))
		{
			if (outputOnly)
			{
				dataTypes = StateMod_Util.getTimeSeriesDataTypes(StateMod_DataSet.COMP_RESERVOIR_STATIONS, id, __dataset, "", interval, false, false, true, true, false, true);
			}
			else
			{
				dataTypes = StateMod_Util.getTimeSeriesDataTypes(StateMod_DataSet.COMP_RESERVOIR_STATIONS, id, __dataset, "", interval, true, true, true, true, false, true);
			}
		}
		else if (station_type.Equals(StateMod_Util.STATION_TYPE_STREAMGAGE, StringComparison.OrdinalIgnoreCase))
		{
			dataTypes = StateMod_Util.getTimeSeriesDataTypes(StateMod_DataSet.COMP_STREAMGAGE_STATIONS, id, __dataset, "", interval, true, true, true, true, false, true);
		}
		else if (station_type.Equals(StateMod_Util.STATION_TYPE_STREAMESTIMATE, StringComparison.OrdinalIgnoreCase))
		{
			dataTypes = StateMod_Util.getTimeSeriesDataTypes(StateMod_DataSet.COMP_STREAMESTIMATE_STATIONS, id, __dataset, "", interval, true, true, true, true, false, true);
		}
		else if (station_type.Equals(StateMod_Util.STATION_TYPE_WELL, StringComparison.OrdinalIgnoreCase))
		{
			dataTypes = StateMod_Util.getTimeSeriesDataTypes(StateMod_DataSet.COMP_WELL_STATIONS, id, __dataset, "", interval, true, true, true, true, false, true);
		}

		if (__worksheet != null)
		{
			__worksheet.setCellSpecificJComboBoxValues(row, _COL_DATA_TYPE, dataTypes);
			System.Collections.IList v = __worksheet.getCellSpecificJComboBoxValues(row, _COL_DATA_TYPE);
			string s = null;
			if (v == null || v.Count == 0)
			{
				s = "";
			}
			else
			{
				s = (string)v[0];
			}
			setInternalValueAt(s, row,_COL_DATA_TYPE);
		}
	}

	/// <summary>
	/// Fills the ID column based on the kind of station selected. </summary>
	/// <param name="row"> the row of the ID column being dealt with </param>
	/// <param name="type"> the type of structure selected (column 1) </param>
	public virtual void fillIDColumn(int row, string type)
	{
		System.Collections.IList ids = new List<object>();
		if (type.Equals(StateMod_Util.STATION_TYPE_DIVERSION, StringComparison.OrdinalIgnoreCase))
		{
			if (__diversionIDs == null)
			{
				__diversionIDs = createAvailableIDsList(__diversions);
			}
			ids = __diversionIDs;
		}
		else if (type.Equals(StateMod_Util.STATION_TYPE_INSTREAM_FLOW, StringComparison.OrdinalIgnoreCase))
		{
			if (__instreamFlowIDs == null)
			{
				__instreamFlowIDs = createAvailableIDsList(__instreamFlows);
			}
			ids = __instreamFlowIDs;
		}
		else if (type.Equals(StateMod_Util.STATION_TYPE_RESERVOIR, StringComparison.OrdinalIgnoreCase))
		{
			if (__reservoirIDs == null)
			{
				__reservoirIDs = createAvailableIDsList(__reservoirs);
			}
			ids = __reservoirIDs;
		}
		else if (type.Equals(StateMod_Util.STATION_TYPE_STREAMGAGE, StringComparison.OrdinalIgnoreCase))
		{
			if (__streamGageStationIDs == null)
			{
				__streamGageStationIDs = createAvailableIDsList(__streamGageStations);
			}
			ids = __streamGageStationIDs;
		}
		else if (type.Equals(StateMod_Util.STATION_TYPE_STREAMESTIMATE, StringComparison.OrdinalIgnoreCase))
		{
			if (__streamEstimateStationIDs == null)
			{
				__streamEstimateStationIDs = createAvailableIDsList(__streamEstimateStations);
			}
			ids = __streamEstimateStationIDs;
		}
		else if (type.Equals(StateMod_Util.STATION_TYPE_WELL, StringComparison.OrdinalIgnoreCase))
		{
			if (__wellIDs == null)
			{
				__wellIDs = createAvailableIDsList(__wells);
			}
			ids = __wellIDs;
		}

		if (ids.Count == 0)
		{
			ids.Add(" ");
		}

		if (__worksheet != null)
		{
			__worksheet.setCellSpecificJComboBoxValues(row, _COL_ID, ids);
			System.Collections.IList v = __worksheet.getCellSpecificJComboBoxValues(row, _COL_ID);
			string s = null;
			if (v == null || v.Count == 0)
			{
				s = "";
			}
			else
			{
				s = (string)v[0];
			}
			setInternalValueAt(s, row, _COL_ID);
		}
	}

	/// <summary>
	/// Fills the input name column combo box according to the type of station, ID,
	/// interval, data type, and input type that is selected. </summary>
	/// <param name="row"> the row of the data type column that is being dealt with </param>
	/// <param name="station_type"> the type of the station (column _COL_STATION_TYPE) </param>
	/// <param name="id"> the ID of the structure (column _COL_ID) </param>
	/// <param name="interval_string"> The data interval (column _COL_INTERVAL). </param>
	/// <param name="data_type"> The data type (column _COL_DATA_TYPE). </param>
	/// <param name="input_type"> The input type (column _COL_INPUT_TYPE). </param>
	public virtual void fillInputNameColumn(int row, string station_type, string id, string interval_string, string data_type, string input_type)
	{
		System.Collections.IList input_names = new List<object>();
		int interval = TimeInterval.MONTH;
		if (interval_string.Equals("Day", StringComparison.OrdinalIgnoreCase))
		{
			interval = TimeInterval.DAY;
		}
		if (StringUtil.indexOfIgnoreCase(data_type, "Output", 0) > 0)
		{
			// Have an output time series...
			if (station_type.Equals(StateMod_Util.STATION_TYPE_DIVERSION, StringComparison.OrdinalIgnoreCase) || station_type.Equals(StateMod_Util.STATION_TYPE_STREAMGAGE, StringComparison.OrdinalIgnoreCase) || station_type.Equals(StateMod_Util.STATION_TYPE_STREAMESTIMATE, StringComparison.OrdinalIgnoreCase) || station_type.Equals(StateMod_Util.STATION_TYPE_INSTREAM_FLOW, StringComparison.OrdinalIgnoreCase))
			{
				if (interval == TimeInterval.MONTH)
				{
					// Substitute base name later...
					input_names.Add("*.b43");
					// Explicitly specify base name...
					input_names.Add(__dataset.getBaseName() + ".b43");
				}
				else
				{
					// Daily...
					input_names.Add("*.b49");
					// Explicitly specify base name...
					input_names.Add(__dataset.getBaseName() + ".b49");
				}
			}
			else if (station_type.Equals(StateMod_Util.STATION_TYPE_RESERVOIR, StringComparison.OrdinalIgnoreCase))
			{
				if (interval == TimeInterval.MONTH)
				{
					// Substitute base name later...
					input_names.Add("*.b44");
					// Explicitly specify base name...
					input_names.Add(__dataset.getBaseName() + ".b44");
				}
				else
				{
					// Daily...
					input_names.Add("*.b50");
					// Explicitly specify base name...
					input_names.Add(__dataset.getBaseName() + ".b50");
				}
			}
			else if (station_type.Equals(StateMod_Util.STATION_TYPE_WELL, StringComparison.OrdinalIgnoreCase))
			{
				if (interval == TimeInterval.MONTH)
				{
					// Substitute base name later...
					input_names.Add("*.b42");
					// Explicitly specify base name...
					input_names.Add(__dataset.getBaseName() + ".b42");
				}
				else
				{
					// Daily...
					input_names.Add("*.b65");
					// Explicitly specify base name...
					input_names.Add(__dataset.getBaseName() + ".b65");
				}
			}
		}
		else
		{
			// Need to pick the correct input name from the type...
			// This needs to be relative if at all possible!
			string ext = StateMod_DataSet.lookupTimeSeriesDataFileExtension(StringUtil.getToken(data_type," ",0,0), interval);
			if (!ext.Equals(""))
			{
				input_names.Add("*." + ext);
			}
			string filename = __dataset.getComponentDataFileNameFromTimeSeriesDataType(StringUtil.getToken(data_type," ",0,0), interval);
			if (!filename.Equals(""))
			{
				input_names.Add(filename);
			}
		}

		// Always add a Browse...
		input_names.Add(__BROWSE_INPUT_NAME_ABSOLUTE);
		input_names.Add(__BROWSE_INPUT_NAME_RELATIVE);

		if (__worksheet != null)
		{
			__worksheet.setCellSpecificJComboBoxValues(row, _COL_INPUT_NAME, input_names);
			System.Collections.IList v = __worksheet.getCellSpecificJComboBoxValues(row,_COL_INPUT_NAME);
			string s = null;
			if (v == null || v.Count == 0)
			{
				s = "";
			}
			else
			{
				s = (string)v[0];
			}
			setInternalValueAt(s, row, _COL_INPUT_NAME);
		}
	}

	/// <summary>
	/// Fills the input type column combo box according to the type of station, ID,
	/// interval, and data type that is selected. </summary>
	/// <param name="row"> the row of the data type column that is being dealt with </param>
	/// <param name="station_type"> the type of the station (column _COL_STATION_TYPE) </param>
	/// <param name="id"> the ID of the structure (column _COL_ID) </param>
	/// <param name="interval_string"> The data interval (column _COL_INTERVAL). </param>
	/// <param name="data_type"> The data type (column _COL_DATA_TYPE). </param>
	public virtual void fillInputTypeColumn(int row, string station_type, string id, string interval_string, string data_type)
	{
		System.Collections.IList input_types = new List<object>();

		if (StringUtil.indexOfIgnoreCase(data_type, "Output", 0) > 0)
		{
			// Have an output time series...
			input_types.Add("StateModB");
		}
		else
		{
			input_types.Add("StateMod");
		}

		if (__worksheet != null)
		{
			__worksheet.setCellSpecificJComboBoxValues(row, _COL_INPUT_TYPE, input_types);
			System.Collections.IList v = __worksheet.getCellSpecificJComboBoxValues(row,_COL_INPUT_TYPE);
			string s = null;
			if (v == null || v.Count == 0)
			{
				s = "";
			}
			else
			{
				s = (string)v[0];
			}
			// NOTE: this doesn't call setIntervalValueAt in order that
			// the input name will be populated properly.
			setValueAt(s, row, _COL_INPUT_TYPE);
		}
	}

	// TODO - need to check the data set to see if daily data are available
	/// <summary>
	/// Fills the interval column combo box according to the type of station selected and the ID of that station. </summary>
	/// <param name="row"> the row of the data type column that is being dealt with </param>
	/// <param name="station_type"> the type of the station (column _COL_STATION_TYPE) </param>
	/// <param name="id"> the ID of the structure (column _COL_ID) </param>
	public virtual void fillIntervalColumn(int row, string station_type, string id)
	{
		System.Collections.IList intervals = new List<object>();
		intervals.Add("Month");
		intervals.Add("Day");

		if (__worksheet != null)
		{
			__worksheet.setCellSpecificJComboBoxValues(row, _COL_INTERVAL, intervals);
			System.Collections.IList v = __worksheet.getCellSpecificJComboBoxValues(row, _COL_INTERVAL);
			string s = null;
			if (v == null || v.Count == 0)
			{
				s = "";
			}
			else
			{
				s = (string)v[0];
			}
			setInternalValueAt(s, row, _COL_INTERVAL);
		}
	}

	/// <summary>
	/// From AbstractTableModel.  Returns the class of the data stored in a given column. </summary>
	/// <param name="columnIndex"> the column for which to return the data class. </param>
	public virtual Type getColumnClass(int columnIndex)
	{
		switch (columnIndex)
		{
			case _COL_STATION_TYPE:
				return typeof(string);
			case _COL_ID:
				return typeof(string);
			case _COL_INTERVAL:
				return typeof(string);
			case _COL_DATA_TYPE:
				return typeof(string);
			case _COL_INPUT_TYPE:
				return typeof(string);
			case _COL_INPUT_NAME:
				return typeof(string);
			default:
				return typeof(string);
		}
	}

	/// <summary>
	/// From AbstractTableModel; returns the number of columns of data. </summary>
	/// <returns> the number of columns of data. </returns>
	public virtual int getColumnCount()
	{
		return __COLUMNS;
	}

	/// <summary>
	/// From AbstractTableModel; returns the name of the column at the given position. </summary>
	/// <returns> the name of the column at the given position. </returns>
	public virtual string getColumnName(int columnIndex)
	{
		switch (columnIndex)
		{
			case _COL_STATION_TYPE:
				return "STATION TYPE";
			case _COL_ID:
				return "IDENTIFIER (NAME)";
			case _COL_DATA_TYPE:
				return "DATA TYPE";
			case _COL_INTERVAL:
				return "INTERVAL";
			case _COL_INPUT_TYPE:
				return "INPUT TYPE";
			case _COL_INPUT_NAME:
				return "INPUT NAME";
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

		tips[_COL_STATION_TYPE] = "<html>The station type indicates the list of identifiers that should be displayed.</html>";
		tips[_COL_ID] = "<html>The identifier corresponds to a station that has time series.</html>";
		tips[_COL_INTERVAL] = "<html>The interval indicates whether monthly or daily time series are graphed.</html>";
		tips[_COL_DATA_TYPE] = "<html>Data types identify the time series parameter to be "+
			"graphed.<BR>Parameters are listed as input, estimated input, and output.</HTML>";
		tips[_COL_INPUT_TYPE] = "<HTML>The input type indicates the file format for data." +
			"<BR>Input time series by default are read from Statemod time series files (StateMod)." +
			"<BR>Output time series by default are read from binary output files (StateModB).</HTML>";
		tips[_COL_INPUT_NAME] = "<HTML>The input name indicates the file to be read.<BR>" +
			"Input time series available in memory will be used before reading a matching file.<BR>" +
			"Select a file from a different data set if appropriate.<BR>" +
			"Use the * choice to share a graph between data sets.</HTML>";
		return tips;
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
		widths[_COL_STATION_TYPE] = 13;
		widths[_COL_ID] = 20;
		widths[_COL_INTERVAL] = 7;
		widths[_COL_DATA_TYPE] = 22;
		widths[_COL_INPUT_TYPE] = 7;
		widths[_COL_INPUT_NAME] = 20;
		return widths;
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
			case _COL_ID:
				return "%-16s";
			case _COL_INTERVAL:
				return "%8s";
			case _COL_DATA_TYPE:
				return "%-20s";
			case _COL_INPUT_TYPE:
				return "%-8s";
			case _COL_INPUT_NAME:
				return "%-40s";
			default:
				return "%-8s";
		}
	}

	/// <summary>
	/// From AbstractTableModel; returns the number of rows of data in the table.
	/// </summary>
	public virtual int getRowCount()
	{
		return _rows;
	}

	/// <summary>
	/// From AbstractTableModel; returns the data that should be placed in the JTable at the given row and column. </summary>
	/// <param name="row"> the row for which to return data. </param>
	/// <param name="col"> the column for which to return data. </param>
	/// <returns> the data that should be placed in the JTable at the given row and col. </returns>
	public virtual object getValueAt(int row, int col)
	{
		if (_sortOrder != null)
		{
			row = _sortOrder[row];
		}

		TSIdent tsident = (TSIdent)_data.get(row);

		switch (col)
		{
			case _COL_STATION_TYPE:
				return tsident.getAlias();
			case _COL_ID:
				return tsident.getLocation();
			case _COL_INTERVAL:
				return tsident.getInterval();
			case _COL_DATA_TYPE:
				return tsident.getType();
			case _COL_INPUT_TYPE:
				return tsident.getInputType();
			case _COL_INPUT_NAME:
				return tsident.getInputName();
			default:
				return "";
		}
	}

	/// <summary>
	/// Returns whether the cell is editable or not.  In this model, all the cells in
	/// columns 3 and greater are editable. </summary>
	/// <param name="rowIndex"> unused. </param>
	/// <param name="columnIndex"> the index of the column to check whether it is editable. </param>
	/// <returns> whether the cell is editable or not. </returns>
	public virtual bool isCellEditable(int rowIndex, int columnIndex)
	{
		int size = _cellEditOverride.size();

		if (size > 0)
		{
			int[] temp;
			for (int i = 0; i < size; i++)
			{
				temp = (int[])_cellEditOverride.get(i);
				if (temp[0] == rowIndex && temp[1] == columnIndex)
				{
					if (temp[2] == 1)
					{
						return true;
					}
					else
					{
						return false;
					}
				}
			}
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
		/*
		Message.printStatus(1, "", "---------------------------------------");
		Message.printStatus(1, "", "SET VALUE AT: " + row + ", " +col);
		JWorksheet.except(1, 3);
		Message.printStatus(1, "", "---------------------------------------");
		*/

		TSIdent tsident = (TSIdent)_data.get(row);

		switch (col)
		{
			case _COL_STATION_TYPE:
				string type = tsident.getAlias();
				if (type.Equals((string)value))
				{
					break;
				}
				tsident.setAlias((string)value);
				tsident.setLocation("");
				setValueAt("", row, _COL_INTERVAL);
				// this next line wouldn't seem to make any sense, but leave it in!!
				setValueAt(getValueAt(row, _COL_DATA_TYPE), row, _COL_DATA_TYPE);
				fireTableDataChanged();
				overrideCellEdit(row, _COL_ID, true);
				fillIDColumn(row, (string)value);
				// Since the ID is filled, select the first item by
				// default to force something to be displayed...
				if (__worksheet != null)
				{
					System.Collections.IList ids = __worksheet.getCellSpecificJComboBoxValues(row, _COL_ID);
					if (ids.Count > 0)
					{
						setValueAt(ids[0], row, _COL_ID);
					}
					else
					{
						setValueAt("", row, _COL_ID);
					}
				}
				break;
			case _COL_ID:
				string id = (string)value;
				tsident.setLocation(id);
				bool outputOnly = false;
				string staType = (string)getValueAt(row, _COL_STATION_TYPE);
				if (staType.Equals(StateMod_Util.STATION_TYPE_RESERVOIR))
				{
					if (id.IndexOf("-", StringComparison.Ordinal) > -1)
					{
						outputOnly = true;
					}
				}
				overrideCellEdit(row, _COL_DATA_TYPE, true);
				// Fill the interval cell, given the station type and identifier...
				if (((string)value).Length == 0)
				{
					fireTableDataChanged();
				}
				fillIntervalColumn(row, (string)getValueAt(row, _COL_STATION_TYPE), (string)value);
				if (outputOnly)
				{
					fillDataTypeColumn(row, true, (string)getValueAt(row, _COL_STATION_TYPE), (string)getValueAt(row, _COL_ID), (string)value);
				}
				else
				{
					fillDataTypeColumn(row, false, (string)getValueAt(row, _COL_STATION_TYPE), (string)getValueAt(row, _COL_ID), (string)value);
				}
				fireTableDataChanged();
				break;
			case _COL_INTERVAL:
				try
				{
					tsident.setInterval((string)value);
				}
				catch (Exception)
				{
					// Should not happen.
				}
				bool ioutputOnly = false;
				string istaType = (string)getValueAt(row, _COL_STATION_TYPE);
				string iid = (string)getValueAt(row, _COL_ID);
				if (istaType.Equals(StateMod_Util.STATION_TYPE_RESERVOIR))
				{
					if (iid.IndexOf("-", StringComparison.Ordinal) > -1)
					{
						ioutputOnly = true;
					}
				}
				if (ioutputOnly)
				{
					fillDataTypeColumn(row, true, (string)getValueAt(row, _COL_STATION_TYPE), (string)getValueAt(row, _COL_ID), (string)value);
				}
				else
				{
					fillDataTypeColumn(row, false, (string)getValueAt(row, _COL_STATION_TYPE), (string)getValueAt(row, _COL_ID), (string)value);
				}
				// this next line wouldn't seem to make any sense, but leave it in!!				
				setValueAt(getValueAt(row, _COL_DATA_TYPE), row, _COL_DATA_TYPE);
				fireTableDataChanged();
				break;
			case _COL_DATA_TYPE:
				tsident.setType((string)value);
				// Fill the input type cell, given the station type, identifier, and interval...
				fillInputTypeColumn(row, (string)getValueAt(row, _COL_STATION_TYPE), (string)getValueAt(row, _COL_ID), (string)getValueAt(row, _COL_INTERVAL), (string)value);
				fireTableDataChanged();
				break;
			case _COL_INPUT_TYPE:
				tsident.setInputType((string)value);
				// Fill the input name with defaults...
				fillInputNameColumn(row, (string)getValueAt(row, _COL_STATION_TYPE), (string)getValueAt(row, _COL_ID), (string)getValueAt(row, _COL_INTERVAL), (string)getValueAt(row, _COL_DATA_TYPE), (string)value);
				break;
			case _COL_INPUT_NAME:
				string s = (string)value;
				if (s.Equals(__BROWSE_INPUT_NAME_ABSOLUTE))
				{
					s = browseForFile();
				}
				else if (s.Equals(__BROWSE_INPUT_NAME_RELATIVE))
				{
					string file = browseForFile();
					if (!string.ReferenceEquals(file, null))
					{
						try
						{
							int index = file.LastIndexOf(File.separator);
							string workingDir = __dataset.getDataSetDirectory();
							string dir = IOUtil.toRelativePath(workingDir,file.Substring(0, index));
							s = dir + File.separator + file.Substring(index + 1, file.Length - (index + 1));
						}
						catch (Exception)
						{
							// TODO (JTS - 2003-11-05)  maybe handle this better.  Right now just defaults to the absolute filename
							s = file;
						}
					}
				}
				tsident.setInputName(s);
				// don't go through the super.setValueAt() at the end of the method ...
				base.setValueAt(s, row, col);
				return;
		}

		base.setValueAt(value, row, col);
	}

	/// <summary>
	/// Inserts the specified value into the data object at the given position.  This
	/// is not like setValueAt() because it doesn't change any combo box values or
	/// update other columns' data.  It simply puts the data into the data object
	/// and notifies the table that data has changed so that it displays the updated values. </summary>
	/// <param name="value"> the object to store in the table cell. </param>
	/// <param name="row"> the row of the cell in which to place the object. </param>
	/// <param name="col"> the column of the cell in which to place the object. </param>
	public virtual void setInternalValueAt(object value, int row, int col)
	{
		/*
		Message.printStatus(1, "", "---------------------------------------");
		Message.printStatus(1, "", "SET INTERNAL VALUE AT: " + row + ", " +col);
		JWorksheet.except(1, 3);
		Message.printStatus(1, "", "---------------------------------------");
		*/

		TSIdent tsident = (TSIdent)_data.get(row);

		switch (col)
		{
			case _COL_STATION_TYPE:
				string type = tsident.getAlias();
				if (type.Equals((string)value))
				{
					break;
				}
				tsident.setAlias((string)value);
				break;
			case _COL_ID:
				tsident.setLocation((string)value);
				break;
			case _COL_INTERVAL:
				try
				{
					tsident.setInterval((string)value);
				}
				catch (Exception)
				{
					// Should not happen.
				}
				break;
			case _COL_DATA_TYPE:
				tsident.setType((string)value);
				break;
			case _COL_INPUT_TYPE:
				tsident.setInputType((string)value);
				break;
			case _COL_INPUT_NAME:
				string s = (string)value;
				if (s.Equals(__BROWSE_INPUT_NAME_ABSOLUTE))
				{
					s = browseForFile();
				}
				else if (s.Equals(__BROWSE_INPUT_NAME_RELATIVE))
				{
					string file = browseForFile();
					if (!string.ReferenceEquals(file, null))
					{
						try
						{
							int index = file.LastIndexOf(File.separator);
							string workingDir = __dataset.getDataSetDirectory();
							string dir = IOUtil.toRelativePath(workingDir, file.Substring(0, index));
							s = dir + File.separator + file.Substring(index + 1, file.Length - (index + 1));
						}
						catch (Exception)
						{
							// TODO (JTS - 2003-11-05) maybe handle this better.  Right now just defaults to the absolute filename
							s = file;
						}
					}
				}
				tsident.setInputName(s);
				// don't go through the super.setValueAt() at the end of the method ...
				base.setValueAt(s, row, col);
				return;
		}

		fireTableDataChanged();
		base.setValueAt(value, row, col);
	}

	/// <summary>
	/// Sets the worksheet in which this model is being used. </summary>
	/// <param name="worksheet"> the worksheet in which this model is being used </param>
	public virtual void setWorksheet(JWorksheet worksheet)
	{
		__worksheet = worksheet;
	}

	/// <summary>
	/// Browse for a statemod output file.
	/// </summary>
	private string browseForFile()
	{
		JGUIUtil.setWaitCursor(__parent, true);
		string lastDirectorySelected = JGUIUtil.getLastFileDialogDirectory();

		JFileChooser fc = JFileChooserFactory.createJFileChooser(lastDirectorySelected);

		fc.setDialogTitle("Select file");
	//	SimpleFileFilter ff = new SimpleFileFilter("???", "?Some kind of file?");
	//	fc.addChoosableFileFilter(ff);
	//	fc.setAcceptAllFileFilterUsed(false);
	//	fc.setFileFilter(ff);
		fc.setAcceptAllFileFilterUsed(true);
		fc.setDialogType(JFileChooser.OPEN_DIALOG);

		JGUIUtil.setWaitCursor(__parent, false);
		int retVal = fc.showOpenDialog(__parent);
		if (retVal != JFileChooser.APPROVE_OPTION)
		{
			return null;
		}

		string currDir = (fc.getCurrentDirectory()).ToString();

		if (!currDir.Equals(lastDirectorySelected, StringComparison.OrdinalIgnoreCase))
		{
			JGUIUtil.setLastFileDialogDirectory(currDir);
		}

		string filename = fc.getSelectedFile().getName();

		// do some work with the filename, perhaps

		return currDir + File.separator + filename;
	}

	}

}