// StateMod_InstreamFlowRight_Data_JFrame - JFrame that displays InstreamFlow data in a tabular format.

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
// StateMod_InstreamFlowRight_Data_JFrame - This is a JFrame that displays 
//	InstreamFlow data in a tabular format.
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
// 
// 2005-03-31	J. Thomas Sapienza, RTi	Initial version.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace DWR.StateMod
{

	using JScrollWorksheet = RTi.Util.GUI.JScrollWorksheet;

	/// <summary>
	/// This class is a JFrame for displaying a Vector of StateMod_InstreamFlowRight 
	/// data in a worksheet.  The worksheet data can be exported to a file or printed.
	/// </summary>
	public class StateMod_InstreamFlowRight_Data_JFrame : StateMod_Data_JFrame
	{

	/// <summary>
	/// Constructor. </summary>
	/// <param name="data"> the data to display in the worksheet.  Can be null or empty, in
	/// which case an empty worksheet is shown. </param>
	/// <param name="titleString"> the String to display as the title of the GUI. </param>
	/// <param name="editable"> whether the data in the JFrame can be edited or not.  If true
	/// the data can be edited, if false they can not. </param>
	/// <exception cref="Exception"> if there is an error building the worksheet. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public StateMod_InstreamFlowRight_Data_JFrame(java.util.List data, String titleString, boolean editable) throws Exception
	public StateMod_InstreamFlowRight_Data_JFrame(System.Collections.IList data, string titleString, bool editable) : base(data, titleString, editable)
	{
		setSize(729, getHeight());
	}

	/// <summary>
	/// Called when the Apply button is pressed. This commits any changes to the data objects.
	/// </summary>
	protected internal override void apply()
	{
		StateMod_InstreamFlowRight isf = null;
		int size = _data.Count;
		for (int i = 0; i < size; i++)
		{
			isf = (StateMod_InstreamFlowRight)_data[i];
			isf.createBackup();
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
		StateMod_InstreamFlowRight_Data_TableModel tableModel = new StateMod_InstreamFlowRight_Data_TableModel(_data, _editable);
		StateMod_InstreamFlowRight_Data_CellRenderer cellRenderer = new StateMod_InstreamFlowRight_Data_CellRenderer(tableModel);

		// _props is defined in the super class
		return new JScrollWorksheet(cellRenderer, tableModel, _props);
	}

	/// <summary>
	/// Called when the cancel button is pressed.  This discards any changes made to the data objects.
	/// </summary>
	protected internal override void cancel()
	{
		StateMod_InstreamFlowRight isf = null;
		int size = _data.Count;
		for (int i = 0; i < size; i++)
		{
			isf = (StateMod_InstreamFlowRight)_data[i];
			isf.restoreOriginal();
		}
	}

	/// <summary>
	/// Creates backups of all the data objects in the Vector so that changes can later be cancelled if necessary.
	/// </summary>
	protected internal override void createDataBackup()
	{
		StateMod_InstreamFlowRight isf = null;
		int size = _data.Count;
		for (int i = 0; i < size; i++)
		{
			isf = (StateMod_InstreamFlowRight)_data[i];
			isf.createBackup();
		}
	}

	}

}