using System.Collections.Generic;

// StateMod_OperationalRight_Data_JFrame - JFrame for displaying a list of StateMod_OperationalRight data in a worksheet

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

	using JScrollWorksheet = RTi.Util.GUI.JScrollWorksheet;

	/// <summary>
	/// This class is a JFrame for displaying a list of StateMod_OperationalRight 
	/// data in a worksheet.  The worksheet data can be exported to a file or printed.
	/// </summary>
	public class StateMod_OperationalRight_Data_JFrame : StateMod_Data_JFrame
	{

	/// <summary>
	/// Constructor. </summary>
	/// <param name="data"> the data to display in the worksheet.  Can be null or empty, in 
	/// which case an empty worksheet is shown. </param>
	/// <param name="titleString"> the String to display as the GUI title. </param>
	/// <param name="editable"> whether the data in the JFrame can be edited or not.  If true
	/// the data can be edited, if false they can not. </param>
	/// <exception cref="Exception"> if there is an error building the worksheet. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public StateMod_OperationalRight_Data_JFrame(java.util.List<StateMod_OperationalRight> data, String titleString, boolean editable) throws Exception
	public StateMod_OperationalRight_Data_JFrame(IList<StateMod_OperationalRight> data, string titleString, bool editable) : base(data, titleString, editable)
	{
	}

	/// <summary>
	/// Called when the Apply button is pressed. This commits any changes to the data objects.
	/// </summary>
	protected internal override void apply()
	{
		StateMod_OperationalRight op = null;
		int size = _data.Count;
		for (int i = 0; i < size; i++)
		{
			op = (StateMod_OperationalRight)_data[i];
			op.createBackup();
		}
	}

	/// <summary>
	/// Creates a JScrollWorksheet for the current data and returns it. </summary>
	/// <returns> a JScrollWorksheet containing the data Vector passed in to the constructor. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected RTi.Util.GUI.JScrollWorksheet buildJScrollWorksheet() throws Exception
	protected internal override JScrollWorksheet buildJScrollWorksheet()
	{
		StateMod_OperationalRight_Data_TableModel tableModel = new StateMod_OperationalRight_Data_TableModel(_data, _editable);
		StateMod_OperationalRight_Data_CellRenderer cellRenderer = new StateMod_OperationalRight_Data_CellRenderer(tableModel);

		// _props is defined in the super class
		return new JScrollWorksheet(cellRenderer, tableModel, _props);
	}

	/// <summary>
	/// Called when the cancel button is pressed.  This discards any changes made to the data objects.
	/// </summary>
	protected internal override void cancel()
	{
		StateMod_OperationalRight op = null;
		int size = _data.Count;
		for (int i = 0; i < size; i++)
		{
			op = (StateMod_OperationalRight)_data[i];
			op.restoreOriginal();
		}
	}

	/// <summary>
	/// Creates backups of all the data objects in the Vector so that changes can later be cancelled if necessary.
	/// </summary>
	protected internal override void createDataBackup()
	{
		StateMod_OperationalRight op = null;
		int size = _data.Count;
		for (int i = 0; i < size; i++)
		{
			op = (StateMod_OperationalRight)_data[i];
			op.createBackup();
		}
	}

	}

}