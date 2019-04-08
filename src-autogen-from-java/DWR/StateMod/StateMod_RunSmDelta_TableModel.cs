using System;
using System.Collections.Generic;

// StateMod_RunSmDelta_TableModel - Table model for displaying data for delta plot-related tables

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
// StateMod_RunSmDelta_TableModel - Table model for displaying data for 
//	delta plot-related tables
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
// 2003-08-21	J. Thomas Sapienza, RTi	Initial version.
// 2003-08-25	JTS, RTi		Expanded with much more logic including:
//					- filling in parameter types based on
//					  selections
//					- filling in IDs based on selections
//					- browsing for filenames
// 2003-08-26	JTS, RTi		Continued expanding the logic to
//					fill out the table data properly.
// 2003-08-27	JTS, RTi		Further continued work on the logic.
// 2004-01-22	JTS, RTi		Removed the row count column and 
//					changed all the other column numbers.
// 2004-10-28	SAM, RTi		Change setValueAt() to support sort.
// 2006-03-06	JTS, RTi		* The last row in the worksheet can now
//					  be deleted with preDeleteRow().
//					* Added javadocs for all the methods.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
// ----------------------------------------------------------------------------
// EndHeader

namespace DWR.StateMod
{

	using JGUIUtil = RTi.Util.GUI.JGUIUtil;
	using JWorksheet = RTi.Util.GUI.JWorksheet;
	using JWorksheet_AbstractRowTableModel = RTi.Util.GUI.JWorksheet_AbstractRowTableModel;
	using JWorksheet_CellAttributes = RTi.Util.GUI.JWorksheet_CellAttributes;
	using SimpleFileFilter = RTi.Util.GUI.SimpleFileFilter;

	/// <summary>
	/// This table model display delta plot data.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings({ "serial", "rawtypes" }) public class StateMod_RunSmDelta_TableModel extends RTi.Util.GUI.JWorksheet_AbstractRowTableModel
	public class StateMod_RunSmDelta_TableModel : JWorksheet_AbstractRowTableModel
	{

	/// <summary>
	/// Number of columns in the table model.
	/// </summary>
	private const int __COLUMNS = 5;

	/// <summary>
	/// The parent frame on which the JWorksheet for this model is displayed.
	/// </summary>
	private StateMod_RunSmDelta_JFrame __parent = null;

	/// <summary>
	/// The worksheet in which this table model is working.
	/// </summary>
	private JWorksheet __worksheet;

	/// <summary>
	/// List of data for filling ID lists.
	/// </summary>
	private IList<StateMod_Reservoir> __reservoirs;
	private IList<StateMod_Diversion> __diversions;
	private IList<StateMod_InstreamFlow> __instreamFlows;
	private IList<StateMod_Well> __wells;
	private IList<StateMod_StreamGage> __streamGages;

	/// <summary>
	/// ID lists to be displayed in the combo boxes.
	/// </summary>
	private IList<string> __reservoirIDs = null;
	private IList<string> __diversionIDs = null;
	private IList<string> __instreamFlowIDs = null;
	private IList<string> __streamflowIDs = null;
	private IList<string> __streamflow0IDs = null;
	private IList<string> __wellIDs = null;

	protected internal string _ABOVE_STRING = "";

	private JWorksheet_CellAttributes __needsFilled;

	public const int COL_FILE = 0, COL_TYPE = 1, COL_PARM = 2, COL_YEAR = 3, COL_ID = 4;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="parent"> the StateMod_RunDeltaPlot_JFrame in which the table is displayed </param>
	/// <param name="data"> the data that will be displayed in the table. </param>
	/// <param name="reservoirs"> list of StateMod_Reservoir objects </param>
	/// <param name="diversions"> list of StateMod_Diversion objects </param>
	/// <param name="instreamFlows"> list of StateMod_InstreamFlow objects. </param>
	/// <param name="streamGages"> list of StateMod_??? objects </param>
	/// <param name="wells"> list of StateMod_Well objects </param>
	/// <exception cref="Exception"> if an invalid data or dmi was passed in. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public StateMod_RunSmDelta_TableModel(StateMod_RunSmDelta_JFrame parent, java.util.List data, java.util.List<StateMod_Reservoir> reservoirs, java.util.List<StateMod_Diversion> diversions, java.util.List<StateMod_InstreamFlow> instreamFlows, java.util.List<StateMod_StreamGage> streamGages, java.util.List<StateMod_Well> wells) throws Exception
	public StateMod_RunSmDelta_TableModel(StateMod_RunSmDelta_JFrame parent, System.Collections.IList data, IList<StateMod_Reservoir> reservoirs, IList<StateMod_Diversion> diversions, IList<StateMod_InstreamFlow> instreamFlows, IList<StateMod_StreamGage> streamGages, IList<StateMod_Well> wells)
	{
		__parent = parent;

		__reservoirs = reservoirs;
		__diversions = diversions;
		__instreamFlows = instreamFlows;
		__wells = wells;
		__streamGages = streamGages;

		__needsFilled = new JWorksheet_CellAttributes();
		__needsFilled.borderColor = Color.red;

		if (data == null)
		{
			throw new Exception("Invalid data Vector passed to StateMod_RunSmDelta_TableModel constructor.");
		}
		_rows = data.Count;
		_data = data;
	}

	/// <summary>
	/// Opens a dialog from which users can browse for delta plot files. </summary>
	/// <returns> the path to the file the user chose, or "" if no file was selected. </returns>
	private string browseForFile()
	{
		JGUIUtil.setWaitCursor(__parent, true);
		string directory = JGUIUtil.getLastFileDialogDirectory();

		JFileChooser fc = null;
		if (!string.ReferenceEquals(directory, null))
		{
			fc = new JFileChooser(directory);
		}
		else
		{
			fc = new JFileChooser();
		}

		fc.setDialogTitle("Select File");
		SimpleFileFilter xre = new SimpleFileFilter("xdd", "ASCII xre");
		SimpleFileFilter b44 = new SimpleFileFilter("b44", "Binary b44");
		SimpleFileFilter xdd = new SimpleFileFilter("xdd", "ASCII xdd");
		SimpleFileFilter b43 = new SimpleFileFilter("b43", "Binary b43");

		fc.addChoosableFileFilter(xre);
		fc.addChoosableFileFilter(b44);
		fc.addChoosableFileFilter(xdd);
		fc.addChoosableFileFilter(b43);
		fc.setAcceptAllFileFilterUsed(true);
		fc.setFileFilter(xre);
		fc.setDialogType(JFileChooser.SAVE_DIALOG);

		JGUIUtil.setWaitCursor(__parent, false);
		int retVal = fc.showSaveDialog(__parent);
		if (retVal != JFileChooser.APPROVE_OPTION)
		{
			return "";
		}

		string currDir = (fc.getCurrentDirectory()).ToString();

		if (!currDir.Equals(directory, StringComparison.OrdinalIgnoreCase))
		{
			JGUIUtil.setLastFileDialogDirectory(currDir);
		}
		string filename = fc.getSelectedFile().getName();

		return currDir + File.separator + filename;
	}

	/// <summary>
	/// Checks to see if a row can be added to the table.  Rows cannot be added if the first row is not fully
	/// filled out.  
	/// TODO (JTS - 2006-03-06)
	/// I think this is bad code.  I think the elementAt() call should return the 
	/// last data value -- as it is, it is only checking that the first row is 
	/// set up properly. </summary>
	/// <returns> true if a new row can be added, false if not. </returns>
	public virtual bool canAddRow()
	{
		if (_rows == 0)
		{
			return true;
		}
		StateMod_GraphNode gn = (StateMod_GraphNode)_data.get(0);
		if (gn.getFileName().Trim().Equals(""))
		{
			return false;
		}
		if (gn.getType().Trim().Equals(""))
		{
			return false;
		}
		if (gn.getDtype().Trim().Equals(""))
		{
			return false;
		}
		if (gn.getYrAve().Trim().Equals(""))
		{
			return false;
		}
		if (gn.getID().Trim().Equals(""))
		{
			return false;
		}
		return true;
	}

	/// <summary>
	/// This method is called when a new row is added, and it copies the nearest combo
	/// box type from above the new row and sets the new row's columns to the same value
	/// as that in the combo box.  Also fills the parameter and ID columns the same.
	/// </summary>
	public virtual void copyDownComboBoxes()
	{
		if (_rows == 1)
		{
			return;
		}

		string type = null;
		int i = _rows - 2;
		while (i >= 0 && string.ReferenceEquals(type, null))
		{
			type = (string)(getValueAt(i, COL_TYPE));
			if (type.Trim().Equals(""))
			{
				type = null;
			}
			i--;
		}
		if (string.ReferenceEquals(type, null))
		{
			return;
		}

		setValueAt("", _rows - 1, COL_PARM);
		fillParameterColumn(_rows - 1, type);
		fillIDColumn(_rows - 1, type);
	}

	/// <summary>
	/// Creates a list of the available IDs for a Vector of StateMod_Data-extending objects. </summary>
	/// <param name="nodes"> the nodes for which to create a list of IDs. </param>
	/// <returns> a list of Strings, each of which contains an ID followed by the 
	/// name of Structure in parentheses </returns>
	private IList<string> createAvailableIDsList<T1>(IList<T1> nodes) where T1 : StateMod_Data
	{
		IList<string> v = new List<string>();

		int num = 0;
		if (nodes != null)
		{
			num = nodes.Count;
		}

		v.Add("0 (All)");

		for (int i = 0; i < num; i++)
		{
			v.Add(nodes[i].getID() + " (" + nodes[i].getName() + ")");
		}
		return v;
	}

	/// <summary>
	/// Clears the values in the parameter ID combo box in the given row. </summary>
	/// <param name="row"> the row to clear the combo box values. </param>
	public virtual void emptyParmIDComboBoxes(int row)
	{
		string s = null;
		bool skip = true;
		for (int i = row; i < _rows; i++)
		{
			if (skip)
			{
				s = "";
				skip = false;
			}
			else
			{
				s = (string)(getValueAt(i, COL_TYPE));
			}

			if (!s.Equals(""))
			{
				return;
			}
			fillParameterColumn(i, "");
			fillIDColumn(i, "");
			setValueAt("", i, COL_PARM);
			setValueAt("", i, COL_ID);
		}
	}

	/// <summary>
	/// Fills the parameter column in the given row based on the station type selected for that row. </summary>
	/// <param name="row"> the row for which to fill the parameter column. </param>
	/// <param name="type"> the type of the station in the given row. </param>
	public virtual void fillParameterColumn(int row, string type)
	{
		IList<string> datatypes = new List<string>();

		// Get the model output data types (no input since SmDelta deals with output).
		if (type.Equals("Diversion", StringComparison.OrdinalIgnoreCase))
		{
			datatypes = StateMod_GraphNode.getGraphDataType(StateMod_GraphNode.DIVERSION_TYPE, false);
		}
		else if (type.Equals("Instream flow", StringComparison.OrdinalIgnoreCase))
		{
			datatypes = StateMod_GraphNode.getGraphDataType(StateMod_GraphNode.INSTREAM_TYPE, false);
		}
		else if (type.Equals("Reservoir", StringComparison.OrdinalIgnoreCase))
		{
			datatypes = StateMod_GraphNode.getGraphDataType(StateMod_GraphNode.RESERVOIR_TYPE, false);
		}
		else if (type.Equals("Streamflow", StringComparison.OrdinalIgnoreCase))
		{
			datatypes = StateMod_GraphNode.getGraphDataType(StateMod_GraphNode.STREAM_TYPE, false);
		}
		else if (type.Equals("Well", StringComparison.OrdinalIgnoreCase))
		{
			datatypes = StateMod_GraphNode.getGraphDataType(StateMod_GraphNode.WELL_TYPE, false);
		}
		else if (type.Equals("Stream ID (0* Gages)", StringComparison.OrdinalIgnoreCase))
		{
			datatypes = StateMod_GraphNode.getGraphDataType(StateMod_GraphNode.STREAM_TYPE, false);
		}

		IList<string> finalTypes = new List<string>();
		finalTypes.Add("");

		for (int i = 0; i < datatypes.Count; i++)
		{
			// FIXME SAM 2008-03-20 No need to remove underscores for newer versions of StateMod, right?.
			// Use data types from binary file.
			//finalTypes.add(((String)datatypes.elementAt(i)).replace('_', ' '));
			finalTypes.Add(datatypes[i]);
		}

		if (__worksheet != null)
		{
			//System.out.println("Setting cell-specific stuff");
			__worksheet.setCellSpecificJComboBoxValues(row, COL_PARM, finalTypes);
		}
	}

	/// <summary>
	/// Fills the ID column based on the kind of structure selected. </summary>
	/// <param name="row"> the row of the ID column being dealt with </param>
	/// <param name="type"> the type of structure selected (column 1) </param>
	public virtual void fillIDColumn(int row, string type)
	{
		IList<string> ids = new List<string>();
		if (type.Equals("Diversion", StringComparison.OrdinalIgnoreCase))
		{
			if (__diversionIDs == null)
			{
				__diversionIDs = createAvailableIDsList(__diversions);
			}
			ids = __diversionIDs;
		}
		else if (type.Equals("Instream flow", StringComparison.OrdinalIgnoreCase))
		{
			if (__instreamFlowIDs == null)
			{
				__instreamFlowIDs = createAvailableIDsList(__instreamFlows);
			}
			ids = __instreamFlowIDs;
		}
		else if (type.Equals("Reservoir", StringComparison.OrdinalIgnoreCase))
		{
			if (__reservoirIDs == null)
			{
				__reservoirIDs = createAvailableIDsList(__reservoirs);
			}
			ids = __reservoirIDs;
		}
		else if (type.Equals("Streamflow", StringComparison.OrdinalIgnoreCase))
		{
			if (__streamflowIDs == null)
			{
				__streamflowIDs = createAvailableIDsList(__streamGages);
			}
			ids = __streamflowIDs;
		}
		else if (type.Equals("Well", StringComparison.OrdinalIgnoreCase))
		{
			if (__wellIDs == null)
			{
				__wellIDs = createAvailableIDsList(__wells);
			}
			ids = __wellIDs;
		}
		else if (type.Equals("Stream ID (0* Gages)", StringComparison.OrdinalIgnoreCase))
		{
			if (__streamflow0IDs == null)
			{
				IList<string> v = createAvailableIDsList(__streamGages);
				string s = null;
				__streamflow0IDs = new List<string>();
				for (int i = 0; i < v.Count; i++)
				{
					s = v[i];
					if (s.StartsWith("0", StringComparison.Ordinal))
					{
						__streamflow0IDs.Add(s);
					}
				}
			}
			ids = __streamflow0IDs;
		}

		if (__worksheet != null)
		{
			__worksheet.setCellSpecificJComboBoxValues(row, COL_ID, ids);
		}
	}

	/// <summary>
	/// Creates a list of objects suitable for use in the worksheet from the data
	/// read from a delta plot file. </summary>
	/// <param name="fileData"> the fileData to process. </param>
	/// <returns> a list of objects suitable for use within a form. </returns>
	public virtual IList<StateMod_GraphNode> formLoadData(IList<StateMod_GraphNode> fileData)
	{
		int rows = fileData.Count;

		if (rows == 0)
		{
			return new List<StateMod_GraphNode>();
		}

		// gnf will be a node used to read data FROM the _F_ile nodes
		StateMod_GraphNode gnf = fileData[0];

		string pfile = "";
		string ptype = "";
		string pyear = "";

		string file = null;
		string type = null;
		string dtype = null;
		string year = null;

		// gnw will be a node used for creating the _W_orksheet nodes
		StateMod_GraphNode gnw = null;

		IList<StateMod_GraphNode> v = new List<StateMod_GraphNode>();

		int ids = 0;

		for (int i = 0; i < rows; i++)
		{
			gnf = fileData[i];
			ids = gnf.getIDVectorSize();

			file = gnf.getFileName().Trim();
			type = gnf.getType().Trim();
			dtype = gnf.getDtype().Trim();
			year = gnf.getYrAve().Trim();

			for (int j = 0; j < ids; j++)
			{
				if (j == 0)
				{
					gnw = new StateMod_GraphNode();
					if (!file.Equals(pfile))
					{
						gnw.setFileName(file);
					}
					else
					{
						gnw.setFileName("");
					}
					if (!type.Equals(ptype))
					{
						gnw.setType(type);
					}
					else
					{
						gnw.setType("");
					}
					if (!dtype.Equals(dtype))
					{
						gnw.setDtype(dtype);
					}
					else
					{
						gnw.setDtype("");
					}
					if (!year.Equals(pyear))
					{
						gnw.setYrAve(year);
					}
					else
					{
						gnw.setYrAve("");
					}
					gnw.setID(gnf.getID(0).Trim());
				}
				else
				{
					gnw.setFileName("");
					gnw.setType("");
					gnw.setDtype("");
					gnw.setYrAve("");
					gnw.setID(gnf.getID(j).Trim());
				}
				gnw.setSwitch(gnf.getSwitch());
				v.Add(gnw);
			}

			pfile = file;
			ptype = type;
			pyear = year;
		}

		return v;
	}

	/// <summary>
	/// Saves form data to the data set. </summary>
	/// <param name="worksheetData"> the data in the worksheet to save. </param>
	/// <returns> a list of data objects created from the data in the worksheet. </returns>
	public virtual IList<StateMod_GraphNode> formSaveData(IList<StateMod_GraphNode> worksheetData)
	{
		int rows = worksheetData.Count;

		if (rows == 0)
		{
			return new List<StateMod_GraphNode>();
		}

		// gnw will be a node used to read data FROM the _W_orksheet nodes
		StateMod_GraphNode gnw = worksheetData[0];

		string pfile = gnw.getFileName().Trim();
		string ptype = gnw.getType().Trim();
		string pdtype = gnw.getDtype().Trim();
		string pyear = gnw.getYrAve().Trim();
		string pid = gnw.getID().Trim();

		string file = null;
		string type = null;
		string dtype = null;
		string year = null;
		string id = null;

		// gno will be a node used for creating the _O_utput nodes
		StateMod_GraphNode gno = new StateMod_GraphNode();
		gno.setFileName(pfile);
		gno.setType(ptype);
		gno.setDtype(pdtype);
		gno.setYrAve(pyear);
		int paren = pid.IndexOf("(", StringComparison.Ordinal);
		if (paren > -1)
		{
			gno.addID(pid.Substring(0, paren).Trim());
		}
		else
		{
			gno.addID(pid);
		}

		IList<StateMod_GraphNode> v = new List<StateMod_GraphNode>();

		for (int i = 1; i < rows; i++)
		{
			gnw = worksheetData[i];

			file = gnw.getFileName().Trim();
			if (file.Equals(""))
			{
				file = pfile;
			}
			type = gnw.getType().Trim();
			if (type.Equals(""))
			{
				type = ptype;
			}
			dtype = gnw.getDtype().Trim();
			if (dtype.Equals(""))
			{
				dtype = pdtype;
			}
			year = gnw.getYrAve().Trim();
			if (year.Equals(""))
			{
				year = pyear;
			}
			id = gnw.getID().Trim();
			if (id.Equals(""))
			{
				id = pid;
			}

			if (file.Equals(pfile) && type.Equals(ptype) && dtype.Equals(pdtype) && year.Equals(pyear))
			{
				// all the fields match, so this is a different ID
				// added to the Vector of IDs in the node's vector.
				paren = id.IndexOf("(", StringComparison.Ordinal);
				if (paren > -1)
				{
					gno.addID(id.Substring(0, paren).Trim());
				}
				else
				{
					gno.addID(id);
				}
			}
			else
			{
				// otherwise, values other than just the ID are 
				// different, so a new node needs created
				v.Add(gno);

				gno = new StateMod_GraphNode();
				gno.setFileName(file);
				gno.setType(type);
				gno.setDtype(dtype);
				gno.setYrAve(year);
				paren = id.IndexOf("(", StringComparison.Ordinal);
				if (paren > -1)
				{
					gno.addID(id.Substring(0, paren).Trim());
				}
				else
				{
					gno.addID(id);
				}
			}

			pfile = file;
			ptype = type;
			pdtype = dtype;
			pyear = year;
			pid = id;
		}

		v.Add(gno);

		return v;
	}

	/// <summary>
	/// From AbstractTableModel.  Returns the class of the data stored in a given column. </summary>
	/// <param name="columnIndex"> the column for which to return the data class. </param>
	public virtual Type getColumnClass(int columnIndex)
	{
		switch (columnIndex)
		{
			case COL_FILE:
				return typeof(string);
			case COL_TYPE:
				return typeof(string);
			case COL_PARM:
				return typeof(string);
			case COL_YEAR:
				return typeof(string);
			case COL_ID:
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
			case COL_FILE:
				return "FILE (1)";
			case COL_TYPE:
				return "STATION TYPE (2)";
			case COL_PARM:
				return "PARAMETER (3)";
			case COL_YEAR:
				return "YEAR/AVE (4)";
			case COL_ID:
				return "ID (5)";
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
			case COL_FILE:
				return "%-40s";
			case COL_TYPE:
				return "%-40s";
			case COL_PARM:
				return "%-40s";
			case COL_YEAR:
				return "%-40s";
			case COL_ID:
				return "%-40s";
			default:
				return "%-8s";
		}
	}

	/// <summary>
	/// Returns the nearest station type to the given row.  It does this by looking
	/// throw the worksheet data starting at the row prior to the given one and running
	/// back through row #0, stopping when it finds a row that has the station type
	/// set. </summary>
	/// <returns> the type of the nearest station, or "" if no type could be found. </returns>
	public virtual string getNearestType(int row)
	{
		string s = null;
		for (int i = (row - 1); i >= 0; i--)
		{
			s = (string)(getValueAt(i, COL_TYPE));
			if (!s.Equals(""))
			{
				return s;
			}
		}
		return "";
	}

	/// <summary>
	/// From AbstractTableModel; returns the number of rows of data in the table.
	/// </summary>
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
		if (_sortOrder != null)
		{
			row = _sortOrder[row];
		}

		StateMod_GraphNode gn = (StateMod_GraphNode)_data.get(row);

		switch (col)
		{
			case COL_FILE:
				return gn.getFileName();
			case COL_TYPE:
				return gn.getType();
			case COL_PARM:
				string s = gn.getDtype();
				// FIXME SAM 2008-03-24 No need to do this with newer StateMod
				// since binary file uses underscores for data types.
				//s = (s.replace('_', ' ')).trim();
				return s;
			case COL_YEAR:
				return gn.getYrAve();
			case COL_ID:
				return gn.getID();
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
		widths[COL_FILE] = 15;
		widths[COL_TYPE] = 20;
		widths[COL_PARM] = 20;
		widths[COL_YEAR] = 12;
		widths[COL_ID] = 30;
		return widths;
	}

	/// <summary>
	/// Returns whether the cell is editable or not.  In this model, all the cells in
	/// columns 3 and greater are editable. </summary>
	/// <param name="rowIndex"> unused. </param>
	/// <param name="columnIndex"> the index of the column to check whether it is editable
	/// or not. </param>
	/// <returns> whether the cell is editable or not. </returns>
	public virtual bool isCellEditable(int rowIndex, int columnIndex)
	{
		if (columnIndex == COL_PARM)
		{
			string value = (string)getValueAt(rowIndex, COL_TYPE);
			if (string.ReferenceEquals(value, null) || value.Equals(""))
			{
				return true;
			}
			else
			{
				return true;
			}
		}
		else
		{
			return true;
		}
	}

	/// <summary>
	/// Called in order to delete a data row from the worksheet.  This method does
	/// some operations prior to the actual row delete in order to keep the delta 
	/// plot bookkeeping going. </summary>
	/// <param name="row"> the row to delete. </param>
	public virtual void preDeleteRow(int row)
	{
		if (row < 0 || row >= _rows)
		{
			return;
		}

		__worksheet.editingCanceled(new javax.swing.@event.ChangeEvent(__worksheet));

		if (row < (_rows - 1))
		{
			string dfile = (string)(getValueAt(row, COL_FILE));
			string file = (string)(getValueAt(row + 1, COL_FILE));
			string dtype = (string)(getValueAt(row, COL_TYPE));
			string type = (string)(getValueAt(row + 1, COL_TYPE));
			string dparm = (string)(getValueAt(row, COL_PARM));
			string parm = (string)(getValueAt(row + 1, COL_PARM));
			string dyear = (string)(getValueAt(row, COL_YEAR));
			string year = (string)(getValueAt(row + 1, COL_YEAR));
			string did = (string)(getValueAt(row, COL_ID));
			string id = (string)(getValueAt(row + 1, COL_ID));

			if (file.Equals(""))
			{
				if (!dfile.Equals(""))
				{
					setValueAt(dfile, row + 1, COL_FILE);
				}
			}
			if (type.Equals(""))
			{
				if (!dtype.Equals(""))
				{
					setValueAt(dtype, row + 1, COL_TYPE);
				}
			}
			if (parm.Equals(""))
			{
				if (!dparm.Equals(""))
				{
					setValueAt(dparm, row + 1, COL_PARM);
				}
			}
			if (year.Equals(""))
			{
				if (!dyear.Equals(""))
				{
					setValueAt(dyear, row + 1, COL_YEAR);
				}
			}
			if (id.Equals(""))
			{
				if (!did.Equals(""))
				{
					setValueAt(did, row + 1, COL_ID);
				}
			}
		}

		__worksheet.deleteRow(row);
	}

	/// <summary>
	/// Sets the attributes for the cell at the given location. </summary>
	/// <param name="row"> the row of the cell. </param>
	/// <param name="column"> the column of the cell. </param>
	/// <param name="useAttributes"> if true then attributes will be set on the cell to 
	/// indicate that a value needs to be placed in the row.  If false, then the
	/// cell's attributes will be cleared. </param>
	public virtual void setCellAttributes(int row, int column, bool useAttributes)
	{
		if (useAttributes)
		{
			__worksheet.setCellAttributes(row, column, __needsFilled);
		}
		else
		{
			__worksheet.setCellAttributes(row, column, null);
		}
	}

	/// <summary>
	/// Blanks out all the data values in the final row.
	/// </summary>
	public virtual void setLastRowAsBlank()
	{
		setValueAt("", _rows - 1, 1);
		setValueAt("", _rows - 1, 2);
		setValueAt("", _rows - 1, 3);
		setValueAt("", _rows - 1, 4);
		setValueAt("", _rows - 1, 5);
	}

	/// <summary>
	/// Sets the parameter ID values in the given row for the given type of station. </summary>
	/// <param name="row"> the row in which to set the values. </param>
	/// <param name="type"> the type of station in the row. </param>
	public virtual void setParmIDComboBoxes(int row, string type)
	{
		string s = (string)(getValueAt(row, COL_TYPE));
		bool skip = true;
		if (s.Equals(""))
		{
			skip = false;
		}
		for (int i = row; i < _rows; i++)
		{
			if (skip)
			{
				s = "";
				skip = false;
			}
			else
			{
				s = (string)(getValueAt(i, COL_TYPE));
			}

			if (!s.Equals(""))
			{
				return;
			}
			fillParameterColumn(i, type);
			fillIDColumn(i, type);
			setValueAt("", i, COL_PARM);
			setValueAt("", i, COL_ID);
		}
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

		if (row >= _data.size())
		{
			// this is probably happening as a result of an edit that did not end in time.
		}

		StateMod_GraphNode gn = (StateMod_GraphNode)_data.get(row);
	//Message.printStatus(1, "", "Set value at: " + row + ", " + col + " '" + value + "'");
		switch (col)
		{
			case COL_FILE:
				string filename = (string)value;
				string oldFilename = (string)(getValueAt(row, col));
				if (filename.Equals(oldFilename))
				{
					return;
				}

				if (filename.Equals(StateMod_RunSmDelta_JFrame.OPTION_BROWSE, StringComparison.OrdinalIgnoreCase))
				{
					string newfile = browseForFile();
					gn.setFileName(newfile);
					if (row == 0)
					{
						if (newfile.Trim().Equals(""))
						{
							setCellAttributes(row, col, true);
						}
						else
						{
							setCellAttributes(row, col, false);
						}
					}
					base.setValueAt(newfile, row, col);
					return;
				}
				else
				{
					gn.setFileName(filename);
				}
				if (row == 0)
				{
					if (filename.Trim().Equals(""))
					{
						setCellAttributes(row, col, true);
					}
					else
					{
						setCellAttributes(row, col, false);
					}
				}
				break;
			case COL_TYPE:
				string type = (string)value;
				string oldType = (string)(getValueAt(row, col));

				if (type.Equals(oldType))
				{
					return;
				}

				gn.setType(type);

				if (type.Equals(""))
				{
					if (row == 0)
					{
						setCellAttributes(row, col, true);
						emptyParmIDComboBoxes(0);
					}
					else
					{
						string aboveType = getNearestType(row);
						if (aboveType.Equals(""))
						{
							// error!
							emptyParmIDComboBoxes(row);
						}
						else
						{
							setParmIDComboBoxes(row, aboveType);
						}
					}
				}
				else
				{
					if (row == 0)
					{
						setCellAttributes(row, col, false);
					}
					string aboveType = getNearestType(row);
					if (!type.Equals(aboveType))
					{
						setCellAttributes(row, COL_PARM,true);
						setParmIDComboBoxes(row, type);
					}
					else
					{
						setCellAttributes(row,COL_PARM,false);
					}
				}

				break;
			case COL_PARM:
				string dtype = ((string)value).Trim();
				// FIXME SAM 2008-03-24 No need to do this with newer StateMod
				// since binary file uses underscores for data types.
				//dtype = dtype.replace(' ', '_');
				string oldDtype = (string)(getValueAt(row, col));
				if (dtype.Equals(oldDtype))
				{
					return;
				}
				gn.setDtype(dtype);
				if (row == 0)
				{
					if (dtype.Trim().Equals(""))
					{
						setCellAttributes(row, col, true);
					}
					else
					{
						setCellAttributes(row, col, false);
					}
				}
				else if (!dtype.Trim().Equals(""))
				{
					setCellAttributes(row, col, false);
				}
				break;
			case COL_YEAR:
				string year = ((string)value).Trim();
				string oldYear = (string)(getValueAt(row, col));
				if (year.Equals(oldYear))
				{
					return;
				}
				gn.setYrAve(year);
				if (row == 0)
				{
					if (year.Trim().Equals(""))
					{
						setCellAttributes(row, col, true);
					}
					else
					{
						setCellAttributes(row, col, false);
					}
				}
				break;
			case COL_ID:
				string id = (string)value;
				string oldID = (string)(getValueAt(row, col));
				if (id.Equals(oldID))
				{
					return;
				}
				/*
				int index = id.indexOf("(");
				if (index > -1) {
					id = id.substring(0, index);
				}
				id = id.trim();
				*/
				gn.setID(id);
				if (id.Trim().Equals(""))
				{
					setCellAttributes(row, col, true);
				}
				else
				{
					setCellAttributes(row, col, false);
				}
				break;
		}
		base.setValueAt(value, row, col);
	}

	/// <summary>
	/// Sets the worksheet in which this model is being used. </summary>
	/// <param name="worksheet"> the worksheet in which this model is being used </param>
	public virtual void setWorksheet(JWorksheet worksheet)
	{
		__worksheet = worksheet;
	}

	}

}