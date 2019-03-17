using System;
using System.Collections.Generic;

// StateCU_ComponentDataCheck - class for checking data for components

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

/// <summary>
///****************************************************************************
/// File: StateCU_ComponentDataCheck.java
/// Author: KAT
/// Date: 2007-04-11
/// Class for checking data on components.  Only StateCU components methods 
/// should be added to this class.  This class extends from ComponentDataCheck in
/// RTi_Common. 
/// *******************************************************************************
/// Revisions 
/// *******************************************************************************
/// 2007-04-11	Kurt Tometich	Initial version.
/// *****************************************************************************
/// </summary>

namespace DWR.StateCU
{

	using CheckFile = RTi.Util.IO.CheckFile;
	using CheckFile_DataModel = RTi.Util.IO.CheckFile_DataModel;
	using DataSetComponent = RTi.Util.IO.DataSetComponent;
	using DataSet_ComponentDataCheck = RTi.Util.IO.DataSet_ComponentDataCheck;
	using PropList = RTi.Util.IO.PropList;
	using Status = RTi.Util.IO.Status;
	using Validator = RTi.Util.IO.Validator;
	using Message = RTi.Util.Message.Message;

	public class StateCU_ComponentDataCheck : DataSet_ComponentDataCheck
	{
		internal CheckFile __check_file; // Keeps track of all data checks
		internal int __type; // StateCU Component type
		internal StateCU_DataSet __dataset; // StateCU dataset object
		private int __gen_problems = 0;

	/// <summary>
	/// Constructor that initializes the component type and CheckFile.
	/// The check file can contain data checks from several components.
	/// This class only checks data for one component.  Each time this
	/// class is called to check a component the same check file should
	/// be sent if the data is to be appended to that check file.  The
	/// CheckFile object is passed around the data checks and data
	/// that fails checks are added to it. </summary>
	/// <param name="comp"> StateCU component type. </param>
	/// <param name="file"> CheckFile to append data checks to. </param>
	public StateCU_ComponentDataCheck(int type, CheckFile file, StateCU_DataSet set) : base(type, file)
	{
		__check_file = file;
		__type = type;
		__dataset = set;
	}

	/// <summary>
	/// Finds out which check method to call based on the input type. </summary>
	/// <param name="props"> Property list for properties on data checks. </param>
	/// <returns> CheckFile A data check file object. </returns>
	public virtual CheckFile checkComponentType(PropList props)
	{
		// reset general data problem count
		__gen_problems = 0;
		// check for component data.  If none exists then do no checks.
		IList<object> data_vector = getComponentData(__type);
		if (data_vector == null || data_vector.Count == 0)
		{
			return __check_file;
		}
		switch (__type)
		{
			case StateCU_DataSet.COMP_BLANEY_CRIDDLE:
				checkBlaneyCriddleData(props, data_vector);
			break;
			case StateCU_DataSet.COMP_CLIMATE_STATIONS:
				checkClimateStationData(props, data_vector);
			break;
			case StateCU_DataSet.COMP_CROP_CHARACTERISTICS:
				checkCropCharacteristicsData(props, data_vector);
			break;
			case StateCU_DataSet.COMP_CROP_PATTERN_TS_YEARLY:
				checkCropPatternTSData(props, data_vector);
			break;
			case StateCU_DataSet.COMP_CU_LOCATIONS:
				checkLocationData(props, data_vector);
			break;
			case StateCU_DataSet.COMP_CU_LOCATION_CLIMATE_STATIONS:
				checkLocationClimateStationsData(props, data_vector);
			break;
			case StateCU_DataSet.COMP_CU_LOCATION_COLLECTIONS:
				checkLocationCollectionData(props, data_vector);
			break;
			case StateCU_DataSet.COMP_DELAY_TABLE_ASSIGNMENT_MONTHLY:
				checkDelayTableAssignmentData(props, data_vector);
			break;
			case StateCU_DataSet.COMP_IRRIGATION_PRACTICE_TS_YEARLY:
				checkIrrigationPracticeTSData(props, data_vector);
			break;
			default:
				; // do nothing
			break;
		}
		return __check_file;
	}

	/// <summary>
	/// Performs general and specific data checks on Blaney-Criddle data. </summary>
	/// <param name="props"> A property list for specific properties. </param>
	/// <param name="data_vector"> list of data to check. </param>
	private void checkBlaneyCriddleData(PropList props, System.Collections.IList data_vector)
	{
		// Create elements for the checks and check file
		string[] header = StateCU_BlaneyCriddle.getDataHeader();
		System.Collections.IList data = new List<object>();
		string title = "Blaney Criddle";

		// Perform the general validation using the Data Table Model
		StateCU_Data_TableModel tm = new StateCU_BlaneyCriddle_TableModel(data_vector, false);
		System.Collections.IList @checked = performDataValidation(tm, title);
		//String [] columnHeader = getDataTableModelColumnHeader( tm );
		string[] columnHeader = getColumnHeader(tm);

		// Do specific checks
		int size = 0;
		if (data_vector != null)
		{
			size = data_vector.Count;
		}
		data = doSpecificDataChecks(data_vector, props);
		// Add the data and checks to the check file.
		// Provides basic header information for this data check table 
		string info = "The following " + title + " (" + data.Count +
		" out of " + size + ") have no .....";

		// Create data models for Check file
		CheckFile_DataModel dm = new CheckFile_DataModel(data, header, title, info, data.Count, tm.getRowCount());
		CheckFile_DataModel gen_dm = new CheckFile_DataModel(@checked, columnHeader, title + " Missing or Invalid Data", "", __gen_problems, tm.getRowCount());
		__check_file.addData(dm, gen_dm);
	}

	/// <summary>
	/// Performs general and specific data checks on climate station data. </summary>
	/// <param name="props"> A property list for specific properties. </param>
	/// <param name="data_vector"> Vector of data to check. </param>
	private void checkClimateStationData(PropList props, System.Collections.IList data_vector)
	{
		// Create elements for the checks and check file
		string[] header = StateCU_ClimateStation.getDataHeader();
		System.Collections.IList data = new List<object>();
		string title = "Climate Station";

		// Perform the general validation using the Data Table Model
		StateCU_Data_TableModel tm;
		try
		{
			tm = new StateCU_ClimateStation_TableModel(data_vector, false);
		}
		catch (Exception e)
		{
			Message.printWarning(3, "StateCU_ComponentDataCheck.checkClimateStationData", e);
			return;
		}
		System.Collections.IList @checked = performDataValidation(tm, title);
		//String [] columnHeader = getDataTableModelColumnHeader( tm );
		string[] columnHeader = getColumnHeader(tm);

		// Do specific checks
		int size = 0;
		if (data_vector != null)
		{
			size = data_vector.Count;
		}
		data = doSpecificDataChecks(data_vector, props);
		// Add the data and checks to the check file.
		// Provides basic header information for this data check table 
		string info = "The following " + title + " (" + data.Count +
		" out of " + size + ") have no .....";

		// Create data models for Check file
		CheckFile_DataModel dm = new CheckFile_DataModel(data, header, title, info, data.Count, tm.getRowCount());
		CheckFile_DataModel gen_dm = new CheckFile_DataModel(@checked, columnHeader, title + " Missing or Invalid Data", "", __gen_problems, tm.getRowCount());
		__check_file.addData(dm, gen_dm);
	}

	/// <summary>
	/// Performs general and specific data checks on crop characteristics data. </summary>
	/// <param name="props"> A property list for specific properties. </param>
	/// <param name="data_vector"> Vector of data to check. </param>
	private void checkCropCharacteristicsData(PropList props, System.Collections.IList data_vector)
	{
		// Create elements for the checks and check file
		string[] header = StateCU_CropCharacteristics.getDataHeader();
		System.Collections.IList data = new List<object>();
		string title = "Crop Characteristics";

		// Perform the general validation using the Data Table Model
		StateCU_Data_TableModel tm;
		try
		{
			tm = new StateCU_CropCharacteristics_TableModel(data_vector, false, false);
		}
		catch (Exception e)
		{
			Message.printWarning(3, "StateCU_ComponentDataCheck.checkCropCharacteristicsData", e);
			return;
		}
		System.Collections.IList @checked = performDataValidation(tm, title);
		//String [] columnHeader = getDataTableModelColumnHeader( tm );
		string[] columnHeader = getColumnHeader(tm);

		// Do specific checks
		int size = 0;
		if (data_vector != null)
		{
			size = data_vector.Count;
		}
		data = doSpecificDataChecks(data_vector, props);
		// Add the data and checks to the check file.
		// Provides basic header information for this data check table 
		string info = "The following " + title + " (" + data.Count +
		" out of " + size + ") have no .....";

		// Create data models for Check file
		CheckFile_DataModel dm = new CheckFile_DataModel(data, header, title, info, data.Count, size);
		CheckFile_DataModel gen_dm = new CheckFile_DataModel(@checked, columnHeader, title + " Missing or Invalid Data", "", __gen_problems, size);
		__check_file.addData(dm, gen_dm);
	}

	/// <summary>
	/// Performs general and specific data checks on crop pattern ts data. </summary>
	/// <param name="props"> A property list for specific properties. </param>
	/// <param name="data_vector"> Vector of data to check. </param>
	private void checkCropPatternTSData(PropList props, System.Collections.IList data_vector)
	{

	}

	/// <summary>
	/// Performs general and specific data checks on delay table assignment data. </summary>
	/// <param name="props"> A property list for specific properties. </param>
	/// <param name="data_vector"> Vector of data to check. </param>
	private void checkDelayTableAssignmentData(PropList props, System.Collections.IList data_vector)
	{
		// Create elements for the checks and check file
		string[] header = StateCU_CropCharacteristics.getDataHeader();
		System.Collections.IList data = new List<object>();
		string title = "Delay Table Assignment";

		// Perform the general validation using the Data Table Model
		StateCU_Data_TableModel tm = new StateCU_DelayTableAssignment_Data_TableModel(data_vector);
		System.Collections.IList @checked = performDataValidation(tm, title);
		//String [] columnHeader = getDataTableModelColumnHeader( tm );
		string[] columnHeader = getColumnHeader(tm);

		// Do specific checks
		int size = 0;
		if (data_vector != null)
		{
			size = data_vector.Count;
		}
		data = doSpecificDataChecks(data_vector, props);
		// Add the data and checks to the check file.
		// Provides basic header information for this data check table 
		string info = "The following " + title + " (" + data.Count +
		" out of " + size + ") have no .....";

		// Create data models for Check file
		CheckFile_DataModel dm = new CheckFile_DataModel(data, header, title, info, data.Count, size);
		CheckFile_DataModel gen_dm = new CheckFile_DataModel(@checked, columnHeader, title + " Missing or Invalid Data", "", __gen_problems, size);
		__check_file.addData(dm, gen_dm);
	}

	/// <summary>
	/// Performs general and specific data checks on irrigation practice ts data. </summary>
	/// <param name="props"> A property list for specific properties. </param>
	/// <param name="data_vector"> Vector of data to check. </param>
	private void checkIrrigationPracticeTSData(PropList props, System.Collections.IList data_vector)
	{

	}

	/// <summary>
	/// Performs general and specific data checks on location climate station data. </summary>
	/// <param name="props"> A property list for specific properties. </param>
	/// <param name="data_vector"> Vector of data to check. </param>
	private void checkLocationClimateStationsData(PropList props, System.Collections.IList data_vector)
	{
		// Create elements for the checks and check file
		string[] header = StateCU_CropCharacteristics.getDataHeader();
		System.Collections.IList data = new List<object>();
		string title = "Location Climate Station";

		// Perform the general validation using the Data Table Model
		StateCU_Data_TableModel tm = new StateCU_Location_ClimateStation_TableModel(data_vector);
		System.Collections.IList @checked = performDataValidation(tm, title);
		//String [] columnHeader = getDataTableModelColumnHeader( tm );
		string[] columnHeader = getColumnHeader(tm);

		// Do specific checks
		int size = 0;
		if (data_vector != null)
		{
			size = data_vector.Count;
		}
		data = doSpecificDataChecks(data_vector, props);
		// Add the data and checks to the check file.
		// Provides basic header information for this data check table 
		string info = "The following " + title + " (" + data.Count +
		" out of " + size + ") have no .....";

		// Create data models for Check file
		CheckFile_DataModel dm = new CheckFile_DataModel(data, header, title, info, data.Count, size);
		CheckFile_DataModel gen_dm = new CheckFile_DataModel(@checked, columnHeader, title + " Missing or Invalid Data", "", __gen_problems, size);
		__check_file.addData(dm, gen_dm);
	}

	/// <summary>
	/// Performs general and specific data checks on location collection data. </summary>
	/// <param name="props"> A property list for specific properties. </param>
	/// <param name="data_vector"> Vector of data to check. </param>
	private void checkLocationCollectionData(PropList props, System.Collections.IList data_vector)
	{

	}

	/// <summary>
	/// Performs general and specific data checks on location data. </summary>
	/// <param name="props"> A property list for specific properties. </param>
	/// <param name="data_vector"> Vector of data to check. </param>
	private void checkLocationData(PropList props, System.Collections.IList data_vector)
	{
		//	 Create elements for the checks and check file
		string[] header = StateCU_Location.getDataHeader();
		System.Collections.IList data = new List<object>();
		string title = "CU Location";

		// Perform the general validation using the Data Table Model
		StateCU_Data_TableModel tm;
		try
		{
			tm = new StateCU_Location_TableModel(data_vector, false, false);
		}
		catch (Exception e)
		{
			Message.printWarning(3, "StateCU_ComponentDataCheck.checkLocationData", e);
			return;
		}
		System.Collections.IList @checked = performDataValidation(tm, title);
		//String [] columnHeader = getDataTableModelColumnHeader( tm );
		string[] columnHeader = getColumnHeader(tm);

		// Do specific checks
		int size = 0;
		if (data_vector != null)
		{
			size = data_vector.Count;
		}
		data = doSpecificDataChecks(data_vector, props);
		// Add the data and checks to the check file.
		// Provides basic header information for this data check table 
		string info = "The following " + title + " (" + data.Count +
		" out of " + size + ") have no .....";

		// Create data models for Check file
		CheckFile_DataModel dm = new CheckFile_DataModel(data, header, title, info, data.Count, size);
		CheckFile_DataModel gen_dm = new CheckFile_DataModel(@checked, columnHeader, title + " Missing or Invalid Data", "", __gen_problems, size);
		__check_file.addData(dm, gen_dm);
	}

	/// <summary>
	/// Checks whether the status object is in the ERROR state and if
	/// it is then the object is formatted and returned. </summary>
	/// <param name="status"> </param>
	/// <param name="value">
	/// @return </param>
	private string checkStatus(Status status, object value)
	{
		if (status.getLevel() == Status.ERROR)
		{
			return createHTMLErrorTooltip(status, value);
		}
		else
		{
			return value.ToString();
		}
	}

	/// <summary>
	/// Performs specific data checks for a component.  The
	/// intelligence and checks are stored in the component itself. </summary>
	/// <param name="data"> List of data objects to check. </param>
	/// <returns> List of data that failed the data checks. </returns>
	private System.Collections.IList doSpecificDataChecks(IList<StateCU_ComponentValidator> data, PropList props)
	{
		System.Collections.IList checks = new List<object>();
		if (data == null)
		{
			return checks;
		}
		// Check each component object by calling the
		// checkComponentData() method.  Each component
		// needs to implement this method and extend from
		// the StateMod_Component interface
		StateCU_ComponentValidator comp = null;
		for (int i = 0; i < data.Count; i++)
		{
			comp = data[i];
			StateCU_ComponentValidation validation = comp.validateComponent(__dataset);
			if (validation.size() > 0)
			{
				checks.Add(validation.getAll());
			}
		}
		return checks;
	}

	/// <summary>
	/// Returns a formatted header from the table model
	/// with HTML tooltips. </summary>
	/// <param name="tm"> Table model for the component under validation. </param>
	/// <returns> List of column headers with tooltips. </returns>
	private string[] getColumnHeader(StateCU_Data_TableModel tm)
	{
		string[] header = getDataTableModelColumnHeader(tm);
		string[] return_header = new string[header.Length];
		// format the header to include HTML tooltips based on
		// the validation being performed on a column of data
		for (int i = 0; i < header.Length; i++)
		{
			Validator[] vals = (tm.getValidators(i));
			string val_desc = "Data validators for this column:\n";
			for (int j = 0; j < vals.Length; j++)
			{
				int num = j + 1;
				val_desc = val_desc + num + ". " + vals[j].ToString();
			}
			// write the final header string with tooltip
			return_header[i] = "%tooltip_start" + val_desc +
				"%tooltip_end" + header[i];
		}
		return return_header;
	}

	/// <summary>
	/// Helper method to return the data list for the component
	/// type.  This is maintained by the StateCU dataset. </summary>
	/// <param name="type"> Component type to get data for. </param>
	/// <returns> list of data for a specific component. </returns>
	private IList<object> getComponentData(int type)
	{
		DataSetComponent comp = __dataset.getComponentForComponentType(type);
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<Object> data_vector = (java.util.List<Object>)comp.getData();
		IList<object> data_vector = (IList<object>)comp.getData();

		return data_vector;
	}

	/// <summary>
	/// Uses the table model object to obtain the column
	/// headers. </summary>
	/// <param name="tm"> StateMod_Data_TableModel Object. </param>
	/// <returns> List of column headers from the table model. </returns>
	private string[] getDataTableModelColumnHeader(StateCU_Data_TableModel tm)
	{
		if (tm == null)
		{
			return new string[] {};
		}
		string[] header = new string[tm.getColumnCount()];
		for (int i = 0; i < tm.getColumnCount(); i++)
		{
			header[i] = tm.getColumnName(i);
		}
		return header;
	}

	/// <summary>
	/// Performs data validation based on the validators found in
	/// the table model for the current component. </summary>
	/// <param name="tm"> Interface implemented by each data table model. </param>
	/// <returns> List of data that has gone through data validation.
	/// If any element fails any one of its validators the content
	/// of that element is formatted to tag it as an error. </returns>
	private IList<string []> performDataValidation(StateCU_Data_TableModel tm, string title)
	{
		IList<string []> data = new List<string []>();
		if (tm == null)
		{
			return data;
		}
		Status status = Status.OKAY;
		bool row_had_problem = false;
		__gen_problems = 0;
		// Validate every row and column of data found in the table model
		for (int i = 0; i < tm.getRowCount(); i++)
		{
			string[] row = new string[tm.getColumnCount()];
			row_had_problem = false;
			for (int j = 0; j < tm.getColumnCount(); j++)
			{
				Validator[] vals = (tm.getValidators(j));
				// If there are no validators for this column then
				// just add the data value.  If the value is blank
				// then add a space so the check file is able to show
				// the column.
				if (vals.Length == 0)
				{
					string value = tm.getValueAt(i, j).ToString();
					if (value.Length == 0)
					{
						value = " ";
					}
					row[j] = value;
				}
				for (int k = 0; k < vals.Length; k++)
				{
					status = vals[k].validate(tm.getValueAt(i, j));
					row[j] = checkStatus(status, tm.getValueAt(i, j));
					// if the current validator fails then don't
					// check finer grained validators since there is no need.
					// Log the error as a runtime message in the check file
					if (status.getLevel() != Status.OK)
					{
						row_had_problem = true;
						break;
					}
				}
			}
			data.Add(row);
			if (row_had_problem)
			{
				__gen_problems++;
			}
		}
		return data;
	}

	}

}