﻿using System.Collections.Generic;

// StateCU_ClimateStation_Data_JFrame - This is a JFrame that displays ClimateStation data in a tabular format.

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
// StateCU_ClimateStation_Data_JFrame - This is a JFrame that displays 
//	ClimateStation data in a tabular format.
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
// 
// 2005-01-17	J. Thomas Sapienza, RTi	Initial version.
// 2005-01-20	JTS, RTi		* Constructor now takes parameter to
//					  set the title.
//					* Constructor now takes parameter to
//					  set whether the data are editable or
//					  not.
//					* Removed getDataType().
// 2005-03-28	JTS, RTi		Adjusted GUI size.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace DWR.StateCU
{

	using StateMod_Data_JFrame = DWR.StateMod.StateMod_Data_JFrame;
	using JScrollWorksheet = RTi.Util.GUI.JScrollWorksheet;

	/// <summary>
	/// This class is a JFrame for displaying a Vector of StateCU_ClimateStation data in
	/// a worksheet.  The worksheet data can be exported to a file or printed.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class StateCU_ClimateStation_Data_JFrame extends DWR.StateMod.StateMod_Data_JFrame
	public class StateCU_ClimateStation_Data_JFrame : StateMod_Data_JFrame
	{

	/// <summary>
	/// Constructor. </summary>
	/// <param name="data"> the data to display in the worksheet.  Can be null or empty. </param>
	/// <param name="titleString"> the String to display as the GUI title. </param>
	/// <param name="editable"> whether the data in the JFrame can be edited or not.  If true
	/// the data can be edited, if false they can not. </param>
	/// <exception cref="Exception"> if there is an error building the worksheet. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public StateCU_ClimateStation_Data_JFrame(java.util.List<StateCU_ClimateStation> data, String titleString, boolean editable) throws Exception
	public StateCU_ClimateStation_Data_JFrame(IList<StateCU_ClimateStation> data, string titleString, bool editable) : base(data, titleString, editable)
	{
		setSize(640, getHeight());
	}

	/// <summary>
	/// Called when the Apply button is pressed. This commits any changes to the data
	/// objects.
	/// </summary>
	protected internal override void apply()
	{
		StateCU_ClimateStation station = null;
		int size = _data.Count;
		for (int i = 0; i < size; i++)
		{
			station = (StateCU_ClimateStation)_data[i];
			station.createBackup();
		}
	}

	/// <summary>
	/// Creates a JScrollWorksheet for the current data and returns it. </summary>
	/// <returns> a JScrollWorksheet containing the data Vector passed in to the 
	/// constructor. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected RTi.Util.GUI.JScrollWorksheet buildJScrollWorksheet() throws Exception
	protected internal override JScrollWorksheet buildJScrollWorksheet()
	{
		StateCU_ClimateStation_TableModel tableModel = new StateCU_ClimateStation_TableModel(_data, _editable);
		StateCU_ClimateStation_CellRenderer cellRenderer = new StateCU_ClimateStation_CellRenderer(tableModel);
		// _props is defined in the super class
		return new JScrollWorksheet(cellRenderer, tableModel, _props);
	}

	/// <summary>
	/// Called when the cancel button is pressed.  This discards any changes made to 
	/// the data objects.
	/// </summary>
	protected internal override void cancel()
	{
		StateCU_ClimateStation station = null;
		int size = _data.Count;
		for (int i = 0; i < size; i++)
		{
			station = (StateCU_ClimateStation)_data[i];
			station.restoreOriginal();
		}
	}

	/// <summary>
	/// Creates backups of all the data objects in the Vector so that changes can 
	/// later be cancelled if necessary.
	/// </summary>
	protected internal override void createDataBackup()
	{
		StateCU_ClimateStation station = null;
		int size = _data.Count;
		for (int i = 0; i < size; i++)
		{
			station = (StateCU_ClimateStation)_data[i];
			station.createBackup();
		}
	}

	}

}