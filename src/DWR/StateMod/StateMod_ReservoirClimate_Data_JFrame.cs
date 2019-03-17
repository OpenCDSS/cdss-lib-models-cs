﻿using System.Collections.Generic;

// StateMod_ReservoirClimate_Data_JFrame - JFrame that displays ReservoirClimate data in a tabular format.

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
// StateMod_ReservoirClimate_Data_JFrame - This is a JFrame that displays 
// 	ReservoirClimate data in a tabular format.
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
// 2005-03-30	JTS, RTi		Converted constructor to expect a 
//					Vector of StateMod_Reservoir objects.
// ----------------------------------------------------------------------------

namespace DWR.StateMod
{

	using JScrollWorksheet = RTi.Util.GUI.JScrollWorksheet;
	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// This class is a JFrame for displaying a Vector of StateMod_ReservoirClimate 
	/// data in a worksheet.  Climate data for 1+ reservoirs can be displayed in the 
	/// same worksheet.  The worksheet data can be exported to a file or printed.
	/// </summary>
	public class StateMod_ReservoirClimate_Data_JFrame : StateMod_Data_JFrame
	{

	/// <summary>
	/// Constructor. </summary>
	/// <param name="data"> the data to display in the worksheet.  Can be null or empty, in
	/// which case an empty worksheet is shown. </param>
	/// <param name="titleString"> the String to display in the title of the GUI. </param>
	/// <param name="editable"> whether the data in the JFrame can be edited or not.  If true
	/// the data can be edited, if false they can not. </param>
	/// <param name="precip"> if true, then the climate stations to view are precip stations.
	/// If false, they are evap stations. </param>
	/// <exception cref="Exception"> if there is an error building the worksheet. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public StateMod_ReservoirClimate_Data_JFrame(java.util.List data, String titleString, boolean editable, boolean precip) throws Exception
	public StateMod_ReservoirClimate_Data_JFrame(System.Collections.IList data, string titleString, bool editable, bool precip) : base()
	{

		int j = 0;
		int size = 0;
		int size2 = 0;
		StateMod_Reservoir r = null;
		StateMod_ReservoirClimate c = null;
		System.Collections.IList climates = null;
		System.Collections.IList v = new List<object>();

		if (data != null)
		{
			size = data.Count;
		}

		for (int i = 0; i < size; i++)
		{
			r = (StateMod_Reservoir)data[i];
			climates = r.getClimates();
			if (climates == null)
			{
				continue;
			}

			size2 = climates.Count;

			for (j = 0; j < size2; j++)
			{
				c = (StateMod_ReservoirClimate)climates[j];
				if (c == null)
				{
					// skip
				}
				else if (!precip && c.getType() == StateMod_ReservoirClimate.CLIMATE_EVAP)
				{
						c.setCgoto(r.getID());
						v.Add(c);
				}
				else if (precip && c.getType() == StateMod_ReservoirClimate.CLIMATE_PTPX)
				{
						c.setCgoto(r.getID());
						v.Add(c);
				}
			}
		}

		initialize(v, titleString, editable);
		setSize(377, getHeight());
	}

	/// <summary>
	/// Called when the Apply button is pressed. This commits any changes to the data objects.
	/// </summary>
	protected internal override void apply()
	{
		StateMod_ReservoirClimate clim = null;
		int size = _data.Count;
		for (int i = 0; i < size; i++)
		{
			clim = (StateMod_ReservoirClimate)_data[i];
			clim.createBackup();
		}
	}

	/// <summary>
	/// Creates a JScrollWorksheet for the current data and returns it. </summary>
	/// <returns> a JScrollWorksheet containing the data Vector passed in to the constructor. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected RTi.Util.GUI.JScrollWorksheet buildJScrollWorksheet() throws Exception
	protected internal override JScrollWorksheet buildJScrollWorksheet()
	{
		StateMod_ReservoirClimate_Data_TableModel tableModel = new StateMod_ReservoirClimate_Data_TableModel(_data, _editable);
		StateMod_ReservoirClimate_Data_CellRenderer cellRenderer = new StateMod_ReservoirClimate_Data_CellRenderer(tableModel);

		// _props is defined in the super class
		return new JScrollWorksheet(cellRenderer, tableModel, _props);
	}

	/// <summary>
	/// Called when the cancel button is pressed.  This discards any changes made to the data objects.
	/// </summary>
	protected internal override void cancel()
	{
		StateMod_ReservoirClimate clim = null;
		int size = _data.Count;
		for (int i = 0; i < size; i++)
		{
			clim = (StateMod_ReservoirClimate)_data[i];
			clim.restoreOriginal();
		}
	}

	/// <summary>
	/// Creates backups of all the data objects in the Vector so that changes can later be cancelled if necessary.
	/// </summary>
	protected internal override void createDataBackup()
	{
		StateMod_ReservoirClimate clim = null;
		int size = _data.Count;
		for (int i = 0; i < size; i++)
		{
			Message.printStatus(1, "", "climate1: " + _data[i]);
			Message.printStatus(1, "", "climate2: " + _data[i].GetType());
			clim = (StateMod_ReservoirClimate)_data[i];
			clim.createBackup();
		}
	}

	}

}