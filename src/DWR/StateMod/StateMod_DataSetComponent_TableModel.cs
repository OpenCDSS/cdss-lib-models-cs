using System;

// StateMod_DataSetComponent_TableModel - table Model for a StateMod_DataSet component to display data objects in a table

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
// StateMod_DataSetComponent_TableModel - Table Model for a
//		StateMod_DataSet component to display data objects in a table
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
//
// 2003-08-10	Steven A. Malers, RTi	Initial version - copy and modify the
//					similar StateCU class.
// 2003-10-14	SAM, RTi		Change irrigation water requirement to
//					consumptive water requirement.
// 2004-01-21	J. Thomas Sapienza, RTi	Removed the row count column and 
//					changed all the other column numbers.
// ----------------------------------------------------------------------------

namespace DWR.StateMod
{

	using StateCU_Data = DWR.StateCU.StateCU_Data;
	using TS = RTi.TS.TS;
	using JWorksheet_AbstractRowTableModel = RTi.Util.GUI.JWorksheet_AbstractRowTableModel;
	using DataSetComponent = RTi.Util.IO.DataSetComponent;
	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// This class is a table model for the data objects in a StateMod_DataSet component.
	/// It is not designed for the group components or control objects.
	/// </summary>
	public class StateMod_DataSetComponent_TableModel : JWorksheet_AbstractRowTableModel
	{

	/// <summary>
	/// Number of columns in the table model.
	/// </summary>
	private int __COLUMNS = 2;

	/// <summary>
	/// References to column numbers.
	/// </summary>
	public const int COL_ID = 0, COL_NAME = 1;

	/// <summary>
	/// The component group that is used for the list.
	/// </summary>
	private DataSetComponent __component_group = null;

	/// <summary>
	/// The specific component (primary component) that is used for the list.
	/// </summary>
	private DataSetComponent __component = null;

	/// <summary>
	/// Constructor.  This builds the model for displaying the given component data. </summary>
	/// <param name="dataset"> StateMod_DataSet that is being displayed.  If not a group
	/// component, the group component will be determined. </param>
	/// <param name="comp"> the DataSetComponent to be displayed. </param>
	/// <exception cref="Exception"> an invalid component is passed in. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public StateMod_DataSetComponent_TableModel(StateMod_DataSet dataset, RTi.Util.IO.DataSetComponent comp) throws Exception
	public StateMod_DataSetComponent_TableModel(StateMod_DataSet dataset, DataSetComponent comp)
	{
		System.Collections.IList data = null;
		string routine = "StateMod_DataSetComponent_TableModel";
		// Make sure that the list is for a group component...
		if ((comp != null) && !comp.isGroup())
		{
			__component_group = comp.getParentComponent();
			//Message.printStatus ( 1, routine,
			//"Component is not a group.  Parent is:  " +__component_group);
		}
		else
		{
			__component_group = comp;
		}
		if (__component_group == null)
		{
			_rows = 0;
			_data = null;
			return;
		}
		// Figure out the data component that is actually used to get the list
		// of data objects.  For example, if working on climate stations, there
		// is no list with the group so we need to use the climate stations
		// component list...
		int comptype = dataset.lookupPrimaryComponentTypeForComponentGroup(__component_group.getComponentType());
		if (comptype >= 0)
		{
			__component = dataset.getComponentForComponentType(comptype);
		}
		else
		{
			comp = null;
			Message.printWarning(2, routine, "Unable to find primary component for group:  " + __component_group.getComponentName());
		}
		if (__component == null)
		{
			_rows = 0;
		}
		else
		{
			data = ((System.Collections.IList)__component.getData());
			if (data == null)
			{
				_rows = 0;
			}
			else
			{
				_rows = data.Count;
			}
		}
		_data = data;
	}

	/// <summary>
	/// From AbstractTableModel.  Returns the class of the data stored in a given column. </summary>
	/// <param name="columnIndex"> the column for which to return the data class. </param>
	public virtual Type getColumnClass(int columnIndex)
	{ // REVISIT - expand this to handle data set component properties for the
		// ID, name, etc. columns
		switch (columnIndex)
		{
			case COL_ID:
				return typeof(string);
			case COL_NAME:
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
				return "ID";
			case COL_NAME:
				return "Name";
			default:
				return "";
		}
	}

	/// <summary>
	/// Return the component group that corresponds to the list.  This can be used, for
	/// example to label visible components. </summary>
	/// <returns> the component group that corresponds to the list (can be null). </returns>
	public virtual DataSetComponent getComponentGroup()
	{
		return __component_group;
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
				return "%s"; // All are strings.
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
	/// <param name="col"> the column for which to return data. </param>
	/// <returns> the data that should be placed in the JTable at the given row and col. </returns>
	public virtual object getValueAt(int row, int col)
	{
		if (_sortOrder != null)
		{
			row = _sortOrder[row];
		}

		object o = (object)_data.get(row);
		if (o is TS)
		{
			TS ts = (TS)o;
			switch (col)
			{
				case COL_ID:
					return ts.getIdentifier().ToString();
				case COL_NAME:
					return ts.getDescription();
				default:
					return "";
			}
		}
		else if ((__component.getComponentType() == StateMod_DataSet.COMP_IRRIGATION_PRACTICE_TS_YEARLY) || (__component.getComponentType() == StateMod_DataSet.COMP_CONSUMPTIVE_WATER_REQUIREMENT_TS_MONTHLY) || (__component.getComponentType() == StateMod_DataSet.COMP_CONSUMPTIVE_WATER_REQUIREMENT_TS_DAILY) || (__component.getComponentType() == StateMod_DataSet.COMP_STATECU_STRUCTURE))
		{
			// StateCU_Data...
			StateCU_Data data = (StateCU_Data)_data.get(row);
			switch (col)
			{
				case COL_ID:
					return data.getID();
				case COL_NAME:
					return data.getName();
				default:
					return "";
			}
		}
		else
		{ // StateMod_Data...
			StateMod_Data data = (StateMod_Data)_data.get(row);
			switch (col)
			{
				case COL_ID:
					return data.getID();
				case COL_NAME:
					return data.getName();
				default:
					return "";
			}
		}
	}

	/// <summary>
	/// Returns an array containing the column widths (in number of characters). </summary>
	/// <returns> an integer array containing the widths for each field. </returns>
	public virtual int[] getColumnWidths()
	{
		int[] widths = new int[__COLUMNS];
		widths[COL_ID] = 12;
		widths[COL_NAME] = 20;
		return widths;
	}

	}

}