﻿using System.Collections.Generic;

// StateMod_StreamEstimateCoefficients_Data_JFrame - JFrame that displays StreamEstimate_Coefficients data in a tabular format.

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

// PRINTING CHECKED
// ----------------------------------------------------------------------------
// StateMod_StreamEstimateCoefficients_Data_JFrame - This is a JFrame that 
//	displays StreamEstimate_Coefficients data in a tabular format.
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
// 
// 2005-01-17	J. Thomas Sapienza, RTi	Initial version.
// 2005-01-20	JTS, RTi		Following review:
//					* Improved some loop performance.
//					* Removed getDataType().
//					* Title string is now passed to the
//					  super constructor.
//					* Editability of data in the worksheet
//					  is now passed in via the constructor.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace DWR.StateMod
{

	using JScrollWorksheet = RTi.Util.GUI.JScrollWorksheet;

	/// <summary>
	/// This class is a JFrame for displaying a Vector of 
	/// StateMod_StreamEstimate_Coefficients data in a worksheet.  The worksheet 
	/// data can be exported to a file or printed.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class StateMod_StreamEstimateCoefficients_Data_JFrame extends StateMod_Data_JFrame
	public class StateMod_StreamEstimateCoefficients_Data_JFrame : StateMod_Data_JFrame
	{

	/// <summary>
	/// Constructor. </summary>
	/// <param name="data"> the data to display in the worksheet.  Can be null or empty, in
	/// which case an empty worksheet is shown. </param>
	/// <param name="titleString"> the String to display in the GUI title. </param>
	/// <param name="editable"> whether the data in the JFrame can be edited or not.  If true
	/// the data can be edited, if false they can not. </param>
	/// <exception cref="Exception"> if there is an error building the worksheet. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public StateMod_StreamEstimateCoefficients_Data_JFrame(java.util.List<StateMod_StreamEstimate_Coefficients> data, String titleString, boolean editable) throws Exception
	public StateMod_StreamEstimateCoefficients_Data_JFrame(IList<StateMod_StreamEstimate_Coefficients> data, string titleString, bool editable) : base(data, titleString, editable)
	{
		setSize(582, getHeight());
	}

	/// <summary>
	/// Called when the Apply button is pressed. This commits any changes to the data
	/// objects.
	/// </summary>
	protected internal override void apply()
	{
		StateMod_StreamEstimate_Coefficients coeff = null;
		int size = _data.Count;
		for (int i = 0; i < size; i++)
		{
			coeff = (StateMod_StreamEstimate_Coefficients)_data[i];
			coeff.createBackup();
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
		StateMod_StreamEstimateCoefficients_Data_TableModel tableModel = new StateMod_StreamEstimateCoefficients_Data_TableModel(_data, _editable);
		StateMod_StreamEstimateCoefficients_Data_CellRenderer cellRenderer = new StateMod_StreamEstimateCoefficients_Data_CellRenderer(tableModel);

		// _props is defined in the super class
		return new JScrollWorksheet(cellRenderer, tableModel, _props);
	}

	/// <summary>
	/// Called when the cancel button is pressed.  This discards any changes made to the data objects.
	/// </summary>
	protected internal override void cancel()
	{
		StateMod_StreamEstimate_Coefficients coeff = null;
		int size = _data.Count;
		for (int i = 0; i < size; i++)
		{
			coeff = (StateMod_StreamEstimate_Coefficients)_data[i];
			coeff.restoreOriginal();
		}
	}

	/// <summary>
	/// Creates backups of all the data objects in the Vector so that changes can later be cancelled if necessary.
	/// </summary>
	protected internal override void createDataBackup()
	{
		StateMod_StreamEstimate_Coefficients coeff = null;
		int size = _data.Count;
		for (int i = 0; i < size; i++)
		{
			coeff = (StateMod_StreamEstimate_Coefficients)_data[i];
			coeff.createBackup();
		}
	}

	}

}