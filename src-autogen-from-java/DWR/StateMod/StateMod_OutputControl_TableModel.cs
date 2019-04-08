using System;
using System.Collections.Generic;

// StateMod_OutputControl_TableModel - Table model for displaying data for output control-related tables

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
// StateMod_OutputControl_TableModel - Table model for displaying data for 
//	output control-related tables
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
// 2003-07-09	J. Thomas Sapienza, RTi	Initial version.
// 2003-07-29	JTS, RTi		JWorksheet_RowTableModel changed to
//					JWorksheet_AbstractRowTableModel.
// 2004-01-21	JTS, RTi		Removed the row count column and 
//					changed all the other column numbers.
// 2004-10-28	SAM, RTi		Change setValueAt() to support sort.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
// ----------------------------------------------------------------------------
// EndHeader

namespace DWR.StateMod
{

	using JWorksheet = RTi.Util.GUI.JWorksheet;
	using JWorksheet_AbstractRowTableModel = RTi.Util.GUI.JWorksheet_AbstractRowTableModel;

	/// <summary>
	/// This table model displays output control data.
	/// </summary>
	public class StateMod_OutputControl_TableModel : JWorksheet_AbstractRowTableModel
	{

	/// <summary>
	/// The kinds of station types that can appear in the table.
	/// </summary>
	private readonly string __DIVtype = "DIV", __REStype = "RES", __ISFtype = "ISF", __FLOtype = "FLO", __WELtype = "WEL", __OTHtype = "OTH";

	/// <summary>
	/// Number of columns in the table model.
	/// </summary>
	private readonly int __COLUMNS = 3;

	/// <summary>
	/// References to columns.
	/// </summary>
	public const int COL_TYPE = 0, COL_ID = 1, COL_SWITCH = 2;

	/// <summary>
	/// Whether the data has been edited or not.
	/// </summary>
	private bool __dirty = false;

	/// <summary>
	/// The worksheet in which this table model is working.
	/// </summary>
	private JWorksheet __worksheet;

	/// <summary>
	/// Vectors of data for filling ID lists.
	/// </summary>
	private System.Collections.IList __riverNetwork;

	/// <summary>
	/// ID lists to be displayed in the combo boxes.
	/// </summary>
	private System.Collections.IList __reservoirIDs = null, __diversionIDs = null, __instreamFlowIDs = null, __streamIDs = null, __wellIDs = null, __otherIDs = null;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="parent"> the StateMod_OutputControl_JFrame in which the table is displayed </param>
	/// <param name="data"> the data that will be used to fill in the table. </param>
	/// <param name="riverNetwork"> the data that will be used to fill in the table IDS. </param>
	/// <exception cref="Exception"> if an invalid data or dmi was passed in. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public StateMod_OutputControl_TableModel(StateMod_OutputControl_JFrame parent, java.util.List data, java.util.List riverNetwork) throws Exception
	public StateMod_OutputControl_TableModel(StateMod_OutputControl_JFrame parent, System.Collections.IList data, System.Collections.IList riverNetwork)
	{
		__riverNetwork = riverNetwork;

		if (data == null)
		{
			throw new Exception("Invalid data Vector passed to " + "StateMod_OutputControl_TableModel constructor.");
		}
		_rows = data.Count;
		_data = data;

		__diversionIDs = createAvailableIDsList(getTypeIDVector(__DIVtype));
		__instreamFlowIDs = createAvailableIDsList(getTypeIDVector(__ISFtype));
		__reservoirIDs = createAvailableIDsList(getTypeIDVector(__REStype));
		__streamIDs = createAvailableIDsList(getTypeIDVector(__FLOtype));
		__wellIDs = createAvailableIDsList(getTypeIDVector(__WELtype));
		__otherIDs = createAvailableIDsList(getTypeIDVector(__OTHtype));
	}

	/// <summary>
	/// Checks whether enough values have been entered in the current last row to tell
	/// whether a new row can be added.  A new row can only be added if values have 
	/// been set for columns 1 and 2 and 3 </summary>
	/// <returns> whether it is valid to add a new row </returns>
	public virtual bool canAddNewRow()
	{
		int rows = getRowCount();

		if (rows == 0)
		{
			return true;
		}

		string type = (string)getValueAt((rows - 1), 0);
		string id = (string)getValueAt((rows - 1), 1);
		string offOn = (string)getValueAt((rows - 1), 2);

		if (string.ReferenceEquals(type, null) || type.Trim().Equals(""))
		{
			return false;
		}
		if (string.ReferenceEquals(id, null) || id.Trim().Equals(""))
		{
			return false;
		}
		if (string.ReferenceEquals(offOn, null) || offOn.Trim().Equals(""))
		{
			return false;
		}

		return true;
	}

	/// <summary>
	/// Creates a list of the available IDs for a Vector of StateMod_Data-extending objects. </summary>
	/// <param name="nodes"> the nodes for which to create a list of IDs. </param>
	/// <returns> a Vector of Strings, each of which contains an ID followed by the 
	/// name of Structure in parentheses </returns>
	private System.Collections.IList createAvailableIDsList(System.Collections.IList nodes)
	{
		System.Collections.IList v = new List<object>();

		int num = 0;
		if (nodes != null)
		{
			num = nodes.Count;
		}

		string name = null;
		for (int i = 0; i < (num - 1); i++)
		{
			name = ((StateMod_Data)nodes[i]).getName();
			name = name.Substring(0, name.Length - 4).Trim();
			v.Add(((StateMod_Data)nodes[i]).getID() + " (" + name + ")");
		}
		return v;
	}

	/// <summary>
	/// Fills the ID column based on the kind of structure selected. </summary>
	/// <param name="row"> the row of the ID column being dealt with </param>
	/// <param name="type"> the type of structure selected (column 1) </param>
	public virtual void fillIDColumn(int row, string type)
	{
		System.Collections.IList ids = new List<object>();
		if (type.Equals("Diversion", StringComparison.OrdinalIgnoreCase))
		{
			ids = __diversionIDs;
		}
		else if (type.Equals("Instream flow", StringComparison.OrdinalIgnoreCase))
		{
			ids = __instreamFlowIDs;
		}
		else if (type.Equals("Reservoir", StringComparison.OrdinalIgnoreCase))
		{
			ids = __reservoirIDs;
		}
		else if (type.Equals("Streamflow", StringComparison.OrdinalIgnoreCase))
		{
			ids = __streamIDs;
		}
		else if (type.Equals("Well", StringComparison.OrdinalIgnoreCase))
		{
			ids = __wellIDs;
		}
		else if (type.Equals("Other", StringComparison.OrdinalIgnoreCase))
		{
			ids = __otherIDs;
		}
		else if (type.Equals("INS", StringComparison.OrdinalIgnoreCase))
		{
			ids = __instreamFlowIDs;
		}
		else if (type.Equals("DIV", StringComparison.OrdinalIgnoreCase))
		{
			ids = __diversionIDs;
		}
		else if (type.Equals("RES", StringComparison.OrdinalIgnoreCase))
		{
			ids = __reservoirIDs;
		}
		else if (type.Equals("STR", StringComparison.OrdinalIgnoreCase))
		{
			ids = __streamIDs;
		}
		else if (type.Equals("WEL", StringComparison.OrdinalIgnoreCase))
		{
			ids = __wellIDs;
		}
		else
		{
			ids = __otherIDs;
		}


		if (__worksheet != null)
		{
			__worksheet.setCellSpecificJComboBoxValues(row, 1, ids);
		}
	}

	/// <summary>
	/// Finds the ID of the appropriate type that starts with the given ID. </summary>
	/// <param name="type"> the type of ID to search </param>
	/// <param name="ID"> the character string that the matching ID starts with </param>
	/// <returns> the matching ID. </returns>
	private string findIDMatch(string type, string ID)
	{
		System.Collections.IList search = null;

		if (type.Equals("INS", StringComparison.OrdinalIgnoreCase))
		{
			search = __instreamFlowIDs;
		}
		else if (type.Equals("DIV", StringComparison.OrdinalIgnoreCase))
		{
			search = __diversionIDs;
		}
		else if (type.Equals("RES", StringComparison.OrdinalIgnoreCase))
		{
			search = __reservoirIDs;
		}
		else if (type.Equals("STR", StringComparison.OrdinalIgnoreCase))
		{
			search = __streamIDs;
		}
		else if (type.Equals("WEL", StringComparison.OrdinalIgnoreCase))
		{
			search = __wellIDs;
		}
		else
		{
			search = __otherIDs;
		}

		int index = ID.IndexOf(" ", StringComparison.Ordinal);
		if (index != -1)
		{
			ID = ID.Substring(0, index);
		}

		int size = search.Count;
		string val = null;
		for (int i = 0; i < size; i++)
		{
			val = (string)search[i];
			if (val.StartsWith(ID, StringComparison.Ordinal))
			{
				return val;
			}
		}
		return "";
	}

	/// <summary>
	/// Returns the class of the data stored in a given column. </summary>
	/// <param name="columnIndex"> the column for which to return the data class. </param>
	public virtual Type getColumnClass(int columnIndex)
	{
		switch (columnIndex)
		{
			case COL_TYPE:
				return typeof(string);
			case COL_ID:
				return typeof(string);
			case COL_SWITCH:
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
	/// <returns> the name of the column at the given position. </returns>
	public virtual string getColumnName(int columnIndex)
	{
		switch (columnIndex)
		{
			case COL_TYPE:
				return "STATION TYPE";
			case COL_ID:
				return "ID";
			case COL_SWITCH:
				return "SWITCH";
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
			case COL_TYPE:
				return "%-40s";
			case COL_ID:
				return "%-40s";
			case COL_SWITCH:
				return "%-40s";
			default:
				return "%-8s";
		}
	}

	/// <summary>
	/// Returns the appropriate ID Vector for the given structure type. </summary>
	/// <param name="type"> the structure type for which to return the ID Vector. </param>
	/// <returns> the ID Vector for the structure type. </returns>
	private System.Collections.IList getTypeIDVector(string type)
	{
		int size = __riverNetwork.Count;

		StateMod_Data node = null;
		string name = null;
		System.Collections.IList v = new List<object>();
		// loops through to (num - 1) because the last element of the network Vector is "END"
		for (int i = 0; i < size; i++)
		{
			node = (StateMod_Data)__riverNetwork[i];

			name = node.getName();
			if (type.Equals(__OTHtype) || name.EndsWith(type, StringComparison.Ordinal))
			{
				v.Add(node);
			}
		}
		return v;
	}

	/// <summary>
	/// Returns the number of rows of data in the table.
	/// </summary>
	public virtual int getRowCount()
	{
		return _rows;
	}

	/// <summary>
	/// Returns the data that should be placed in the JTable at the given row and column. </summary>
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

		string ID = gn.getID();
		switch (col)
		{
			case COL_TYPE:
				string type = gn.getType();
				if (type.Equals(""))
				{
					return "";
				}
				else if (type.Equals("INS", StringComparison.OrdinalIgnoreCase) || type.Equals("Instream Flow", StringComparison.OrdinalIgnoreCase))
				{
					return "Instream Flow";
				}
				else if (type.Equals("DIV", StringComparison.OrdinalIgnoreCase) || type.Equals("Diversion", StringComparison.OrdinalIgnoreCase))
				{
					return "Diversion";
				}
				else if (type.Equals("RES", StringComparison.OrdinalIgnoreCase) || type.Equals("Reservoir", StringComparison.OrdinalIgnoreCase))
				{
					return "Reservoir";
				}
				else if (type.Equals("STR", StringComparison.OrdinalIgnoreCase) || type.Equals("Streamflow", StringComparison.OrdinalIgnoreCase))
				{
					return "Streamflow";
				}
				else if (type.Equals("WEL", StringComparison.OrdinalIgnoreCase) || type.Equals("Well", StringComparison.OrdinalIgnoreCase))
				{
					return "Well";
				}
				else
				{
					if (ID.Equals("All"))
					{
						return "";
					}
					return "Other";
				}
			case COL_ID:
				if (ID.Equals("All"))
				{
					return "All";
				}
				else if (ID.Equals(""))
				{
					return "";
				}
				return findIDMatch(gn.getType(), gn.getID());
			case COL_SWITCH:
				if (ID.Equals("All"))
				{
					return "";
				}
				if (gn.getSwitch() == 0)
				{
					return "Off";
				}
				else if (gn.getSwitch() == 1)
				{
					return "On";
				}
				else
				{
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
		widths[COL_TYPE] = 15; // station type
		widths[COL_ID] = 37; // id
		widths[COL_SWITCH] = 17; // switch
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
	/// Returns whether any of the data has been modified. </summary>
	/// <returns> whether any of the data has been modified. </returns>
	public virtual bool isDirty()
	{
		return __dirty;
	}

	/// <summary>
	/// Sets whether any of the data has been modified. </summary>
	/// <param name="dirty"> whether any of the data has been modified </param>
	public virtual void setDirty(bool dirty)
	{
		__dirty = dirty;
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
		setDirty(true);
		StateMod_GraphNode gn = (StateMod_GraphNode)_data.get(row);

		switch (col)
		{
			case COL_TYPE:
				string type = gn.getType();
				if (type.Equals((string)value))
				{
					break;
				}
				gn.setType((string)value);
				gn.setID("");
				gn.setSwitch(-1);
				setValueAt("", row, 1);
				setValueAt("", row, 2);
				fireTableDataChanged();
				overrideCellEdit(row, 1, true);
				overrideCellEdit(row, 2, false);
				fillIDColumn(row, (string)value);
				break;
			case COL_ID:
				gn.setID((string)value);
				gn.setSwitch(-1);
				setValueAt("", row, 2);
				fireTableDataChanged();
				overrideCellEdit(row, 2, true);
				break;
			case COL_SWITCH:
				string offOn = (string)value;
				if (offOn.Equals("Off"))
				{
					gn.setSwitch(0);
				}
				else if (offOn.Equals("On"))
				{
					gn.setSwitch(1);
				}
				else
				{
					gn.setSwitch(-1);
				}
			break;
		}

		base.setValueAt(value, row, col);
	}

	/// <summary>
	/// Sets the worksheet in which this model is being used. </summary>
	/// <param name="worksheet"> the worksheet in which this model is being used
	///  </param>
	public virtual void setWorksheet(JWorksheet worksheet)
	{
		__worksheet = worksheet;
	}

	}

}