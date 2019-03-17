using System.Collections.Generic;

// StateMod_ComponentDataCheck - base class for checking data on components

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
/// File: ComponentDataCheck.java
/// Author: KAT
/// Date: 2007-03-19
/// Base class for checking data on components.  Only StateMod components methods 
/// should be added to this class.  This class extends from ComponentDataCheck in
/// RTi_Common. 
/// *******************************************************************************
/// Revisions 
/// *******************************************************************************
/// 2007-03-15	Kurt Tometich	Initial version.
/// *****************************************************************************
/// </summary>

namespace DWR.StateMod
{

	using CheckFile = RTi.Util.IO.CheckFile;
	using CheckFile_DataModel = RTi.Util.IO.CheckFile_DataModel;
	using DataSetComponent = RTi.Util.IO.DataSetComponent;
	using DataSet_ComponentDataCheck = RTi.Util.IO.DataSet_ComponentDataCheck;
	using PropList = RTi.Util.IO.PropList;
	using Status = RTi.Util.IO.Status;
	using Validator = RTi.Util.IO.Validator;

	public class StateMod_ComponentDataCheck : DataSet_ComponentDataCheck
	{
		internal CheckFile __check_file; // Keeps track of all data checks
		internal int __type; // StateMod Component type
		internal StateMod_DataSet __dataset; // StateMod dataset object
		private int __gen_problems = 0; // Keeps track of the # of
										// general data problems

	/// <summary>
	/// Constructor that initializes the component type and CheckFile.
	/// The check file can contain data checks from several components.
	/// Each time this class is called to check a component the same
	/// check file should be sent if the data is to be appended to 
	/// that check file.  The CheckFile object is passed around the
	/// data checks and data that fails the checks are added to it. </summary>
	/// <param name="comp"> StateMod component type. </param>
	/// <param name="file"> CheckFile to append data checks to. </param>
	public StateMod_ComponentDataCheck(int type, CheckFile file, StateMod_DataSet set) : base(type, file)
	{
		__check_file = file;
		__type = type;
		__dataset = set;
	}

	/// <summary>
	/// Finds out which check method to call based on the input type.  Acts
	/// like a factory for StateMod data checks. </summary>
	/// <param name="props"> Property list for properties on data checks. </param>
	/// <returns> CheckFile A data check file object. </returns>
	public virtual CheckFile checkComponentType(PropList props)
	{
		// reset general data problem count
		__gen_problems = 0;
		// check for component data.  If none exists then do no checks.
		System.Collections.IList data_vector = getComponentData(__type);
		if (data_vector == null || data_vector.Count == 0)
		{
			return __check_file;
		}
		// find out which data type is being checked
		// and call the associated check method.
		switch (__type)
		{
			case StateMod_DataSet.COMP_DELAY_TABLES_MONTHLY:
				checkDelayTableMonthlyData(props, data_vector);
				break;
			case StateMod_DataSet.COMP_DELAY_TABLES_DAILY:
				checkDelayTableDailyData(props, data_vector);
				break;
			case StateMod_DataSet.COMP_DIVERSION_STATIONS:
				checkDiversionStationData(props, data_vector);
			break;
			case StateMod_DataSet.COMP_DIVERSION_RIGHTS:
				checkDiversionRightsData(props, data_vector);
			break;
			case StateMod_DataSet.COMP_INSTREAM_STATIONS:
				checkInstreamFlowStationData(props, data_vector);
			break;
			case StateMod_DataSet.COMP_INSTREAM_RIGHTS:
				checkInstreamFlowRightData(props, data_vector);
			break;
			case StateMod_DataSet.COMP_RESERVOIR_STATIONS:
				checkReservoirStationData(props, data_vector);
			break;
			case StateMod_DataSet.COMP_RESERVOIR_RIGHTS:
				checkReservoirRightData(props, data_vector);
			break;
			case StateMod_DataSet.COMP_RIVER_NETWORK:
				checkRiverNetworkData(props, data_vector);
			break;
			case StateMod_DataSet.COMP_STREAMESTIMATE_STATIONS:
				checkStreamEstimateStationData(props, data_vector);
			break;
			case StateMod_DataSet.COMP_STREAMESTIMATE_COEFFICIENTS:
				checkStreamEstimateCoefficientData(props, data_vector);
			break;
			case StateMod_DataSet.COMP_STREAMGAGE_STATIONS:
				checkStreamGageStationData(props, data_vector);
			break;
			case StateMod_DataSet.COMP_WELL_STATIONS:
				checkWellStationData(props, data_vector);
			break;
			case StateMod_DataSet.COMP_WELL_RIGHTS:
				checkWellStationRights(props, data_vector);
			break;
			default:
				;
			break;
		}
		return __check_file;
	}

	/// <summary>
	/// Performs data checks on delay table daily data. </summary>
	/// <param name="props"> A property list for specific properties
	/// on checking this data. </param>
	/// <param name="data_vector"> Vector of data to check. </param>
	private void checkDelayTableDailyData(PropList props, System.Collections.IList data_vector)
	{

	}

	/// <summary>
	/// Performs data checks on delay table monthly data. </summary>
	/// <param name="props"> A property list for specific properties
	/// on checking this data. </param>
	/// <param name="data_vector"> Vector of data to check. </param>
	private void checkDelayTableMonthlyData(PropList props, System.Collections.IList data_vector)
	{

	}

	/// <summary>
	/// Performs data checks on diversion rights data. </summary>
	/// <param name="props"> A property list for specific properties
	/// on checking this data. </param>
	/// <param name="der_vector"> Vector of data to check. </param>
	private void checkDiversionRightsData(PropList props, System.Collections.IList der_vector)
	{
		// create elements for the checks and check file
		string[] header = StateMod_DiversionRight.getDataHeader();
		System.Collections.IList data = new List<object>();
		string title = "Diversion Rights";

		// Perform the general validation using the Data Table Model
		StateMod_Data_TableModel tm = new StateMod_DiversionRight_Data_TableModel(der_vector, false);
		System.Collections.IList @checked = performDataValidation(tm, title);
		//String [] columnHeader = getDataTableModelColumnHeader( tm );
		string[] columnHeader = getColumnHeader(tm);

		//	 do specific checks
		int size = 0;
		if (der_vector != null)
		{
			size = der_vector.Count;
		}
		data = doSpecificDataChecks(der_vector, props);
		// add the data and checks to the check file	
		// provides basic header information for this data check table 
		string info = "The following diversion rights (" + data.Count +
			" out of " + size +
			") have no .....";

		// create data models for Check file
		CheckFile_DataModel dm = new CheckFile_DataModel(data, header, title, info, data.Count, size);
		CheckFile_DataModel gen_dm = new CheckFile_DataModel(@checked, columnHeader, title + " Missing or Invalid Data", "", __gen_problems, size);
		__check_file.addData(dm, gen_dm);
	}

	/// <summary>
	/// Performs data checks on diversion station data. </summary>
	/// <param name="props"> A property list for specific properties
	/// on checking this data. </param>
	/// <param name="des_vector"> Vector of data to check. </param>
	private void checkDiversionStationData(PropList props, System.Collections.IList des_vector)
	{
		// create elements for the checks and check file
		string[] header = StateMod_Diversion.getDataHeader();
		System.Collections.IList data = new List<object>();
		string title = "Diversion Station";

		// perform the general validation using the Data Table Model
		StateMod_Data_TableModel tm = new StateMod_Diversion_Data_TableModel(des_vector, false);
		System.Collections.IList @checked = performDataValidation(tm, title);
		//String [] columnHeader = getDataTableModelColumnHeader( tm );
		string[] columnHeader = getColumnHeader(tm);

		//	 do specific checks
		int size = 0;
		if (des_vector != null)
		{
			size = des_vector.Count;
		}
		data = doSpecificDataChecks(des_vector, props);
		// add the data and checks to the check file
		// provides basic header information for this data check table 
		string info = "The following diversion stations (" + data.Count +
		" out of " + size +
		") have no .....";

		// create data models for Check file
		CheckFile_DataModel dm = new CheckFile_DataModel(data, header, title, info, data.Count, size);
		CheckFile_DataModel gen_dm = new CheckFile_DataModel(@checked, columnHeader, title + " Missing or Invalid Data", "", __gen_problems, size);
		__check_file.addData(dm, gen_dm);
	}

	/// <summary>
	/// Performs data checks on instream flow right data. </summary>
	/// <param name="props"> A property list for specific properties
	/// on checking this data. </param>
	/// <param name="data_vector"> Vector of data to check. </param>
	private void checkInstreamFlowRightData(PropList props, System.Collections.IList data_vector)
	{
		//	 Create elements for the checks and check file
		string[] header = StateMod_InstreamFlowRight.getDataHeader();
		System.Collections.IList data = new List<object>();
		string title = "Instream Flow Right";

		// Perform the general validation using the Data Table Model
		StateMod_Data_TableModel tm = new StateMod_InstreamFlowRight_Data_TableModel(data_vector, false);
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
	/// Performs data checks on instream flow station data. </summary>
	/// <param name="props"> A property list for specific properties
	/// on checking this data. </param>
	/// <param name="data_vector"> Vector of data to check. </param>
	private void checkInstreamFlowStationData(PropList props, System.Collections.IList data_vector)
	{
		//	 Create elements for the checks and check file
		string[] header = StateMod_InstreamFlow.getDataHeader();
		System.Collections.IList data = new List<object>();
		string title = "Instream Flow Station";

		// Perform the general validation using the Data Table Model
		StateMod_Data_TableModel tm = new StateMod_InstreamFlow_Data_TableModel(data_vector, false);
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
	/// Performs data checks on reservoir right data. </summary>
	/// <param name="props"> A property list for specific properties
	/// on checking this data. </param>
	/// <param name="data_vector"> Vector of data to check. </param>
	private void checkReservoirRightData(PropList props, System.Collections.IList data_vector)
	{
		//	 Create elements for the checks and check file
		string[] header = StateMod_ReservoirRight.getDataHeader();
		System.Collections.IList data = new List<object>();
		string title = "Reservoir Right";

		// Perform the general validation using the Data Table Model
		StateMod_Data_TableModel tm = new StateMod_ReservoirRight_Data_TableModel(data_vector, false);
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
	/// Performs data checks on reservoir station data. </summary>
	/// <param name="props"> A property list for specific properties
	/// on checking this data. </param>
	/// <param name="data_vector"> Vector of data to check. </param>
	private void checkReservoirStationData(PropList props, System.Collections.IList data_vector)
	{
		//	 Create elements for the checks and check file
		string[] header = StateMod_Reservoir.getDataHeader();
		System.Collections.IList data = new List<object>();
		string title = "Reservoir Station";

		// Perform the general validation using the Data Table Model
		StateMod_Data_TableModel tm = new StateMod_Reservoir_Data_TableModel(data_vector, false);
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
	/// Performs general and specific data checks on river network data. </summary>
	/// <param name="props"> A property list for specific properties </param>
	/// <param name="data_vector"> Vector of data to check.
	/// on checking this data. </param>
	private void checkRiverNetworkData(PropList props, System.Collections.IList data_vector)
	{
		//	 Create elements for the checks and check file
		string[] header = StateMod_RiverNetworkNode.getDataHeader();
		System.Collections.IList data = new List<object>();
		string title = "River Network Node";

		// Perform the general validation using the Data Table Model
		StateMod_Data_TableModel tm = new StateMod_RiverNetworkNode_Data_TableModel(data_vector);
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
	/// it is then the object is formatted and returned.  The object needs
	/// to be formatted if there is an error to provide the HTML check file
	/// to format the text and font correctly. </summary>
	/// <param name="status"> Status of the current validation. </param>
	/// <param name="value"> Value currently being validated. </param>
	/// <returns> Status string. </returns>
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
	/// Performs general and specific data checks on
	/// stream estimate coefficient data. </summary>
	/// <param name="props"> A property list for specific properties </param>
	/// <param name="data_vector"> Vector of data to check.
	/// on checking this data. </param>
	private void checkStreamEstimateCoefficientData(PropList props, System.Collections.IList data_vector)
	{
		//	 Create elements for the checks and check file
		string[] header = StateMod_StreamEstimate_Coefficients.getDataHeader();
		System.Collections.IList data = new List<object>();
		string title = "Stream Estimate Coefficients";

		// Perform the general validation using the Data Table Model
		StateMod_Data_TableModel tm = new StateMod_StreamEstimateCoefficients_Data_TableModel(data_vector, false);
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
	/// Performs general and specific data checks on stream estimate station data. </summary>
	/// <param name="props"> A property list for specific properties </param>
	/// <param name="data_vector"> Vector of data to check.
	/// on checking this data. </param>
	private void checkStreamEstimateStationData(PropList props, System.Collections.IList data_vector)
	{
		// Create elements for the checks and check file
		string[] header = StateMod_StreamEstimate.getDataHeader();
		System.Collections.IList data = new List<object>();
		string title = "Stream Estimate Station";

		// Perform the general validation using the Data Table Model
		StateMod_Data_TableModel tm = new StateMod_StreamEstimate_Data_TableModel(data_vector, false);
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
	/// Performs general and specific data checks on stream gage station data. </summary>
	/// <param name="props"> A property list for specific properties </param>
	/// <param name="data_vector"> Vector of data to check.
	/// on checking this data. </param>
	private void checkStreamGageStationData(PropList props, System.Collections.IList data_vector)
	{
		// Create elements for the checks and check file
		string[] header = StateMod_StreamGage.getDataHeader();
		System.Collections.IList data = new List<object>();
		string title = "Stream Gage Station";

		// Perform the general validation using the Data Table Model
		StateMod_Data_TableModel tm = new StateMod_StreamGage_Data_TableModel(data_vector, false);
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
	/// Performs general and specific data checks on well station data. </summary>
	/// <param name="props"> A property list for specific properties </param>
	/// <param name="wes_Vector"> Vector of data to check.
	/// on checking this data. </param>
	private void checkWellStationData(PropList props, System.Collections.IList wes_Vector)
	{
		// create elements for the checks and check file
		string[] header = StateMod_Well.getDataHeader();
		string title = "Well Station";

		// first do the general data validation
		// using this components data table model
		StateMod_Data_TableModel tm = new StateMod_Well_Data_TableModel(wes_Vector, false);
		System.Collections.IList @checked = performDataValidation(tm, title);
		//String [] columnHeader = getDataTableModelColumnHeader( tm );
		string[] columnHeader = getColumnHeader(tm);

		// do specific checks
		int size = 0;
		if (wes_Vector != null)
		{
			size = wes_Vector.Count;
		}
		System.Collections.IList data = new List<object>();
		data = doSpecificDataChecks(wes_Vector, props);
		// add the data and checks to the check file
		// provides basic header information for this data check table 
		string info = "The following well stations (" + data.Count +
		" out of " + size +
		") have no irrigated parcels served by wells.\n" +
		"Data may be OK if the station is an M&I or has no wells.\n" +
		"Parcel count and area in the following table are available " +
		"only if well stations are read from HydroBase.\n";

		// create data models for Check file
		CheckFile_DataModel dm = new CheckFile_DataModel(data, header, title, info, data.Count, size);
		CheckFile_DataModel gen_dm = new CheckFile_DataModel(@checked, columnHeader, title + " Missing or Invalid Data", "", __gen_problems, size);
			__check_file.addData(dm, gen_dm);

		// Check to make sure the sum of well rights equals the well station
		// capacity..
		checkWellRights_CapacityData();
	}

	/// <summary>
	/// Helper method to check well rights component data.  The following are checked:
	/// <ol>
	/// <li>	Well stations without at least one right are listed.  This requires that
	/// the dataset include well stations.</li>
	/// <li>	Well rights with yield <= 0.0</li>
	/// <li>	Well rights summary for a station is not equal to the well capacity.
	/// This requires that the dataset include well stations.<li>
	/// </ol>
	/// </summary>
	private void checkWellStationRights(PropList props, System.Collections.IList wer_Vector)
	{
		int size = 0;
		// create elements for the checks and check file
		string[] header = StateMod_WellRight.getDataHeader();
		System.Collections.IList data = new List<object>();
		string title = "Well Rights";

		// Do the general data validation
		// using this components data table model
		StateMod_Data_TableModel tm = new StateMod_WellRight_Data_TableModel(wer_Vector, false);
		System.Collections.IList @checked = performDataValidation(tm, title);
		//String [] columnHeader = getDataTableModelColumnHeader( tm );
		string[] columnHeader = getColumnHeader(tm);

		// check Well Station data
		PropList props_rights = new PropList("Well Rights");
		props_rights.add("checkRights=true");
		System.Collections.IList wes_Vector = getComponentData(StateMod_DataSet.COMP_WELL_STATIONS);
		if (wes_Vector != null && wes_Vector.Count > 0)
		{
			checkWellStationData(props_rights, wes_Vector);
		}
		props_rights = null; // cleanup

		// Check to make sure the sum of well rights equals the well station
		// capacity...
		checkWellRights_CapacityData();

		// Since well rights are determined from parcel data, print a list of
		// well rights that do not have associated yield (decree)...
		size = 0;
		if (wer_Vector != null)
		{
			size = wer_Vector.Count;
		}
		// Do data checks listed in the StateMod_WellRight class
		// Remove all previous checks from StateMod_Well
		data.Clear();
		data = doSpecificDataChecks(wer_Vector, props);
		// provides basic header information for this data check table 
		string info = "The following well rights (" + data.Count +
		" out of " + size +
		") have no decree (checked to StateMod file .XX precision).\n" +
		"Well yield data may not be available.";

		// create data models for Check file
		CheckFile_DataModel dm = new CheckFile_DataModel(data, header, title, info, data.Count, size);
		CheckFile_DataModel gen_dm = new CheckFile_DataModel(@checked, columnHeader, title + " Missing or Invalid Data", "", __gen_problems, size);
		__check_file.addData(dm, gen_dm);
	}

	/// <summary>
	/// Helper method to check that well rights sum to the well station capacity.  This
	/// is called by the well right and well station checks.  The check is performed
	/// by formatting the capacity and decree sum to .NN precision.
	/// </summary>
	private void checkWellRights_CapacityData()
	{
		// get component data
		System.Collections.IList wes_Vector = (System.Collections.IList)getComponentData(StateMod_DataSet.COMP_WELL_STATIONS);
		System.Collections.IList wer_Vector = (System.Collections.IList)getComponentData(StateMod_DataSet.COMP_WELL_RIGHTS);
		if (wes_Vector == null || wer_Vector == null)
		{
			return;
		}
		System.Collections.IList data = new List<object>();
		int size = 0;

		// initialize some info for the check file
		string[] header = StateMod_Well.getCapacityHeader();
		string title = "Well Station Capacity";

		// check that there data available
		StateMod_Well wes_i = null;
		if (wes_Vector == null)
		{
			return;
		}
		// loop through the vector of data and perform specific
		// data checks
		size = wes_Vector.Count;
		for (int i = 0; i < size; i++)
		{
			wes_i = (StateMod_Well)wes_Vector[i];
			if (wes_i == null)
			{
				continue;
			}
			string[] checks = wes_i.checkComponentData_Capacity(wer_Vector, i);
			// add the data to the data vector
			if (checks != null && checks.Length > 0)
			{
				data.Add(checks);
			}
		}
		// add the data and checks to the check file
		if (data.Count > 0)
		{
			// provides basic header information for this data table 
			string info = "The following well stations (" + data.Count +
				" out of " + size +
				") have capacity different\nfrom the sum of well rights for " +
				"the station.\n" +
				"Parcel count and area in the following table are available " +
				"only if well rights are read from HydroBase.\n";

			// create data models for Check file
			CheckFile_DataModel dm = new CheckFile_DataModel(data, header, title, info, data.Count, size);
			__check_file.addData(dm, null);
		}
	}

	/// <summary>
	/// Performs specific data checks for a component.  The
	/// intelligence and checks are stored in the component itself. </summary>
	/// <param name="data"> List of data objects to check. </param>
	/// <returns> List of data that failed the data checks. </returns>
	private System.Collections.IList doSpecificDataChecks(System.Collections.IList data, PropList props)
	{
		System.Collections.IList checks = new List<object>();
		if (data == null)
		{
			return checks;
		}
		// Check each component object by calling the
		// checkComponentData() method.  Each component
		// needs to implement this method and extend from
		// the StateMod_Component interface.
		StateMod_ComponentValidator comp = null;
		for (int i = 0; i < data.Count; i++)
		{
			comp = (StateMod_ComponentValidator)data[i];
			StateMod_ComponentValidation validation = comp.validateComponent(__dataset);
			if (validation.size() > 0)
			{
				checks.AddRange(validation.getAll());
			}
		}
		return checks;
	}

	/// <summary>
	/// Returns a formatted header from the component's table model
	/// with tooltips needed for the check file.  There is no HTML
	/// here, instead keyword are injected as Strings that the check
	/// file parser will convert into HTML tooltips. </summary>
	/// <param name="tm"> Table model for the component under validation. </param>
	/// <returns> List of column headers with tooltips. </returns>
	private string[] getColumnHeader(StateMod_Data_TableModel tm)
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
			// the %tooltip_start and %tooltip_end are special/keyword
			// Strings that are recognized when the HTML file is rendered.
			// These keywords will be converted into title tags needed to
			// produce an HTML tooltip.  See the checkText() method in 
			// HTMLWriter.java class in RTi_Common for conversions.
			return_header[i] = "%tooltip_start" + val_desc +
				"%tooltip_end" + header[i];
		}
		return return_header;
	}

	/// <summary>
	/// Helper method to return the data vector for the component
	/// type.  This is maintained by the StateMod dataset. </summary>
	/// <param name="type"> Component type to get data for. </param>
	/// <returns> Vector of data for a specific component. </returns>
	private System.Collections.IList getComponentData(int type)
	{
		DataSetComponent comp = __dataset.getComponentForComponentType(type);
		System.Collections.IList data_vector = (System.Collections.IList)comp.getData();

		return data_vector;
	}

	/// <summary>
	/// Uses the table model object to obtain the column
	/// headers. </summary>
	/// <param name="tm"> StateMod_Data_TableModel Object. </param>
	/// <returns> List of column headers from the table model. </returns>
	private string[] getDataTableModelColumnHeader(StateMod_Data_TableModel tm)
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
	private System.Collections.IList performDataValidation(StateMod_Data_TableModel tm, string title)
	{
		System.Collections.IList data = new List<object>();
		if (tm == null)
		{
			return data;
		}
		Status status = Status.OKAY;
		bool row_had_problem = false;
		__gen_problems = 0;
		// Go through rows of data objects
		for (int i = 0; i < tm.getRowCount(); i++)
		{
			string[] row = new string[tm.getColumnCount()];
			row_had_problem = false;
			// Get the data column (this contains the data that needs to
			// be checked).
			for (int j = 0; j < tm.getColumnCount(); j++)
			{
				Validator[] vals = (tm.getValidators(j));
				// if there are no validators for this column then
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
				// Run all validators found in the table model
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
			// Add the data regardless if it fails or not.
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