﻿// StateMod_Response_CellRenderer - Class for rendering cells for the response table.

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
// StateMod_Response_CellRenderer - Class for rendering cells for the
//	response table.
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
// 2003-09-10	J. Thomas Sapienza, RTi	Initial version.
// ----------------------------------------------------------------------------

namespace DWR.StateMod
{
	using JWorksheet_AbstractExcelCellRenderer = RTi.Util.GUI.JWorksheet_AbstractExcelCellRenderer;

	/// <summary>
	/// This class renders cells for the response table.
	/// </summary>
	public class StateMod_Response_CellRenderer : JWorksheet_AbstractExcelCellRenderer
	{

	/// <summary>
	/// Table model for which this class renders the cells.
	/// </summary>
	private StateMod_Response_TableModel __tableModel;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="tableModel"> the table model for which this class renders cells. </param>
	public StateMod_Response_CellRenderer(StateMod_Response_TableModel tableModel)
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