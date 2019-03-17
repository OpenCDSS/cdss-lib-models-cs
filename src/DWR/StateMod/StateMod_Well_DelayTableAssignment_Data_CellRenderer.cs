// StateMod_Well_DelayTableAssignment_Data_CellRenderer - Class for rendering cells for delay table assignment tables

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
// StateMod_Well_DelayTableAssignment_Data_CellRenderer - Class for 
//	rendering cells for delay table assignment tables
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
//
// 2005-04-04	JTS, RTi		Initial version.
// ----------------------------------------------------------------------------

namespace DWR.StateMod
{
	using JWorksheet_AbstractExcelCellRenderer = RTi.Util.GUI.JWorksheet_AbstractExcelCellRenderer;

	/// <summary>
	/// This class is used to render delay table assignment cells.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class StateMod_Well_DelayTableAssignment_Data_CellRenderer extends RTi.Util.GUI.JWorksheet_AbstractExcelCellRenderer
	public class StateMod_Well_DelayTableAssignment_Data_CellRenderer : JWorksheet_AbstractExcelCellRenderer
	{

	/// <summary>
	/// The table this will render cells for.
	/// </summary>
	private StateMod_Well_DelayTableAssignment_Data_TableModel __tableModel;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="tableModel"> the table model for which to render cells </param>
	public StateMod_Well_DelayTableAssignment_Data_CellRenderer(StateMod_Well_DelayTableAssignment_Data_TableModel tableModel)
	{
		__tableModel = tableModel;
	}

	/// <summary>
	/// Returns the format for a given column. </summary>
	/// <param name="column"> the colum for which to return the format. </param>
	/// <returns> the format (as used by StringUtil.format) for a column. </returns>
	public virtual string getFormat(int column)
	{
		return __tableModel.getFormat(column);
	}

	/// <summary>
	/// Returns the widths of the columns in the table. </summary>
	/// <returns> an integer array of the widths of the columns in the table. </returns>
	public virtual int[] getColumnWidths()
	{
		return __tableModel.getColumnWidths();
	}

	}

}