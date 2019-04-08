using System;
using System.Collections.Generic;
using System.Text;

// StateMod_Util - Utility functions for StateMod operation

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

//------------------------------------------------------------------------------
// StateMod_Util - Utility functions for StateMod operation
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
//
// 2003-07-02	J. Thomas Sapienza, RTi	Initial version.
// 2003-07-30	Steven A. Malers, RTi	* Remove import for
//					  StateMod_DataSetComponnent.
//					* Remove static __basinName, which is
//					  the response file name without the
//					  .rsp.  StateMod now can take the name
//					  with or without the .rsp so just pass
//					  the response file name to the
//					  runStateMod() method.
//					* Change runStateModOption() to
//					  runStateMod() and pass the data set
//					  to the method.
//					* Make __statemod_version and
//					  __statemod_executable private and
//					  add set/get methods.
//					* Move remaining static methods from
//					  StateMod_Data.
//					* Alphabetize methods.
// 2003-08-21	SAM, RTi		* Add lookupTimeSeries() to simplify
//					  finding time series for components.
//					* Add createDataList() to help with
//					  choices, etc.
// 2003-08-25	SAM, RTi		Add getUpstreamNetworkNodes() from
//					old SMRiverInfo.retrieveUpstreams().
//					Change it to return data objects, not
//					strings.
// 2003-09-11	SAM, RTi		Update due to changes in the river
//					station component names.
// 2003-09-19	JTS, RTi		Added createCgotoDataList().
// 2003-09-24	SAM, RTi		* Change findEarliestPOR() to
//					  findEarliestDateInPOR().
// 					* Change findLatestPOR() to
//					  findLatestDateInPOR().
//					* Change the above methods to return
//					  null if no date can be found (e.g.,
//					  for a new data set).
// 2003-09-29	SAM, RTi		Add formatDataLabel().
// 2003-10-09	JTS, RTi		* Added removeFromVector().
//					* Added sortStateMod_DataVector().
// 2003-10-10	SAM, RTi		Add estimateDayTS ().
// 2003-10-24	SAM, RTi		Overload runStateMod() to take a 
//					StateMod_DataSet, so the response file
//					can be determined.
// 2003-10-29	SAM, RTi		* Change estimateDailyTS() to
//					  createDailyEstimateTS().
//					* Add createWaterRightTS().
// 2003-11-03	SAM, RTi		Change From_Well parameter to
//					From_River_By_Well.
// 2003-11-05	SAM, RTi		Got clarification from Ray Bennett on
//					which parameters should be listed for
//					output.
// 2003-11-14	SAM, RTi		Ray Bennett provided documentation for
//					the reservoir and well monthly binary
//					files as well as all the daily binary
//					files.  Therefore update the data type
//					lists, etc.
// 2003-11-29	SAM, RTi		In getTimeSeriesDataTypes(),
//					automatically turn off input types if
//					the request is for reservoirs and
//					the identifier has an account part.
// 2004-06-01	SAM, RTi		Update getTimeSeriesDataTypes() to have
//					a flag for data groups and use Ray
//					Bennett feedback for the groups.
// 2004-07-02	SAM, RTi		Add indexOfRiverNodeID().
// 2004-07-06	SAM, RTi		Overload sortStateMod_DataVector() to
//					allow option of creating new or using
//					existing data Vector.
// 2004-08-12	JTS, RTi		Added calculateTimeSeriesDifference().
// 2004-08-25	JTS, RTi		Removed the property that defined a
//					"HelpKey" for the dialog that runs 
//					StateMod.
// 2004-09-07	SAM, RTi		* Reordered some methods to be
//					  alphabetical.
//					* Add findWaterRightInsertPosition().
// 2004-09-14	SAM, RTi		For findWaterRightInsertPosition(), just
//					insert based on the right ID.
// 2004-10-05	SAM, RTi		* Add data type notes as per recent
//					  documentation (? are removed).
//					* Add River_Outflow for reservoir
//					  station output parameters.
// 2005-03-03	SAM, RTi		* Add compareFiles() to help with
//					  testing.
// 2005-04-01	SAM, RTi		* Add createTotalTimeSeries() method to
//					  facilitate summarizing information.
// 2005-04-05	SAM, RTi		* Add lookupTimeSeriesGraphTitle() to
//					  provide default titles based on the
//					  component type.
// 2005-04-18	JTS, RTi		Added the lookup methods.
// 2005-04-19	JTS, RTi		Removed testDirty().
// 2005-05-06	SAM, RTi		Correct a couple of typos in reservoir
//					subcomponent IDs in lookupPropValue().
// 2005-08-30	SAM, RTi		Add getTimeSeriesOutputPrecision().
// 2005-10-05	SAM, RTi		Handle well historical pumping time
//					series in createTotalTS().
// 2005-12-20	SAM, RTi		Add VERSION_XXX and isVersionAtLeast()
//					to help with binary file format
//					versions.
// 2006-01-15	SAM, RTi		Overload getTimeSeriesDataTypes() to
//					take the file name, to facilitate
//					reading the parameters from the newer
//					binary files.
// 2006-03-05	SAM, RTi		calculateTimeSeriesDifference() was
//					resulting in a division by zero, with
//					infinity values being returned.
// 2006-04-10	SAM, RTi		Add getRightsForStation(), which
//					extracts rights for an identifier.
// 2006-06-13	SAM, RTi		Add properties for downstream ID for
//					river network file.
// 2006-08-20	SAM, RTi		Move code to check for edits before
//					running to StateModGUI_JFrame.
// 2007-04-15	Kurt Tometich, RTi		Added some helper methods that
//								return validators for data checks.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
//------------------------------------------------------------------------------
// EndHeader

namespace DWR.StateMod
{

	using DayTS = RTi.TS.DayTS;
	using MonthTS = RTi.TS.MonthTS;
	using TS = RTi.TS.TS;
	using TSData = RTi.TS.TSData;
	using TSIdent = RTi.TS.TSIdent;
	using TSIterator = RTi.TS.TSIterator;
	using TSLimits = RTi.TS.TSLimits;
	using TSUtil = RTi.TS.TSUtil;
	using YearTS = RTi.TS.YearTS;
	using DataFormat = RTi.Util.IO.DataFormat;
	using DataSetComponent = RTi.Util.IO.DataSetComponent;
	using DataUnits = RTi.Util.IO.DataUnits;
	using IOUtil = RTi.Util.IO.IOUtil;
	using ProcessManager = RTi.Util.IO.ProcessManager;
	using ProcessManagerJDialog = RTi.Util.IO.ProcessManagerJDialog;
	using ProcessManagerOutputFilter = RTi.Util.IO.ProcessManagerOutputFilter;
	using PropList = RTi.Util.IO.PropList;
	using Validator = RTi.Util.IO.Validator;
	using Validators = RTi.Util.IO.Validators;
	using MathUtil = RTi.Util.Math.MathUtil;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;
	using DateTime = RTi.Util.Time.DateTime;
	using TimeInterval = RTi.Util.Time.TimeInterval;
	using TimeUtil = RTi.Util.Time.TimeUtil;

	/// <summary>
	/// This class contains utility methods related to a StateMod data set.
	/// </summary>
	public class StateMod_Util
	{

	/// <summary>
	/// Strings used when handling free water rights.
	/// </summary>
	public static string AlwaysOn = "AlwaysOn";
	public static string UseSeniorRightAppropriationDate = "UseSeniorRightAppropriationDate";

	public static string MISSING_STRING = "";
	public static int MISSING_INT = -999;
	public static float MISSING_FLOAT = (float) - 999.0;
	public static double MISSING_DOUBLE = -999.0;
	private static double MISSING_DOUBLE_FLOOR = -999.1;
	private static double MISSING_DOUBLE_CEILING = -998.9;
	public static DateTime MISSING_DATE = null;

	/// <summary>
	/// Floating point values indicating binary file versions.  These are used by StateMod_BTS and
	/// other software to determine appropriate parameters to list.
	/// </summary>
	public const double VERSION_9_01_DOUBLE = 9.01;
	public const double VERSION_9_69_DOUBLE = 9.69;
	public const double VERSION_11_00_DOUBLE = 11.00;

	/// <summary>
	/// Strings indicating binary file versions.  These are used by StateMod_BTS and
	/// other software to determine appropriate parameters to list.
	/// </summary>
	public const string VERSION_9_01 = "9.01";
	public const string VERSION_9_69 = "9.69";
	public const string VERSION_11_00 = "11.00";

	/// <summary>
	/// Strings for the station types.  These should be used in displays (e.g., graphing
	/// tool) for consistency.  They can also be used to compare GUI values, rather than
	/// hard-coding the literal strings.  Make sure that the following lists agree with
	/// the StateMod_BTS file - currently the lists are redundant because StateMod_BTS
	/// may be used independent of a data set.
	/// </summary>
	public const string STATION_TYPE_DIVERSION = "Diversion";
	public const string STATION_TYPE_INSTREAM_FLOW = "Instream Flow";
	public const string STATION_TYPE_RESERVOIR = "Reservoir";
	public const string STATION_TYPE_STREAMESTIMATE = "Stream Estimate Station";
	public const string STATION_TYPE_STREAMGAGE = "Stream Gage Station";
	public const string STATION_TYPE_WELL = "Well";

	/// <summary>
	/// Used for looking up properties for data types which do not have separate components.
	/// </summary>
	public const int COMP_RESERVOIR_AREA_CAP = -102;

	// Used by getStationTypes().
	private static readonly string[] __station_types = new string[] {STATION_TYPE_DIVERSION, STATION_TYPE_INSTREAM_FLOW, STATION_TYPE_RESERVOIR, STATION_TYPE_STREAMESTIMATE, STATION_TYPE_STREAMGAGE, STATION_TYPE_WELL};

	/// <summary>
	/// The following arrays list the output time series data types for various station
	/// types and various significant versions of StateMod.  These are taken from the
	/// StateMod binary data file(s).  Time series should ultimately be read from the
	/// following files (the StateMod_BTS class handles):
	/// <pre>
	/// Diversions      *.B43
	/// Reservoirs      *.B44
	/// Wells           *.B65
	/// StreamGage      *.B43
	/// StreamEstimate  *.B43
	/// IntreamFlow     *.B43
	/// </pre>
	/// Use getTimeSeriesDataTypes() to get the list of parameters to use
	/// for graphical interfaces, etc.  Important:  the lists are in the order of the
	/// StateMod binary file parameters, with no gaps.  If the lists need to be
	/// alphabetized, this should be done separately, not by reordering the arrays below.
	/// </summary>

	/// <summary>
	/// The stream station (stream gage and stream estimate) parameters are written to
	/// the *.xdg file by StateMod's -report module.  The raw data are in the *.B43
	/// (monthly) binary output file.
	/// As per Ray Bennett 2003-11-05 email, all parameters are valid for output.
	/// Include a group and remove the group later if necessary.
	/// </summary>
	private static readonly string[] __output_ts_data_types_stream_0100 = new string[] {"Station In/Out - UpstreamInflow", "Station In/Out - ReachGain", "Station In/Out - ReturnFlow", "Station Balance - RiverInflow", "Station Balance - RiverDivert", "Station Balance - RiverOutflow"};

	private static readonly string[] __output_ts_data_types_stream_0901 = new string[] {"Station In/Out - UpstreamInflow", "Station In/Out - ReachGain", "Station In/Out - ReturnFlow", "Station Balance - RiverInflow", "Station Balance - RiverDivert", "Station Balance - RiverOutflow", "Available Flow - AvailableFlow"};

	private static readonly string[] __output_ts_data_types_stream_0969 = new string[] {"Demand - Total_Demand", "Demand - CU_Demand", "Water Supply - From_River_By_Priority", "Water Supply - From_River_By_Storage", "Water Supply - From_River_By_Exchange", "Water Supply - From_River_By_Well", "Water Supply - From_Carrier_By_Priority", "Water Supply - From_Carrier_By_Storage", "Water Supply - Carried_Water", "Water Supply - From_Soil", "Water Supply - Total_Supply", "Shortage - Total_Short", "Shortage - CU_Short", "Water Use - Consumptive_Use", "Water Use - To_Soil", "Water Use - Total_Return", "Water Use - Loss", "Station In/Out - Upstream_Inflow", "Station In/Out - Reach_Gain", "Station In/Out - Return_Flow", "Station In/Out - Well_Depletion", "Station In/Out - To_From_GW_Storage", "Station Balance - River_Inflow", "Station Balance - River_Divert", "Station Balance - River_By_Well", "Station Balance - River_Outflow", "Available Flow - Available_Flow"};

	/// <summary>
	/// The reservoir station parameters are written to
	/// the *.xrg file by StateMod's -report module.  The raw monthly data are in the
	/// .B44 (monthly) binary output file.  The raw daily data are in the
	/// .B50 (daily) binary output file.
	/// </summary>
	private static readonly string[] __output_ts_data_types_reservoir_0100 = new string[] {"General - InitialStorage", "Supply From River by - RiverPriority", "Supply From River by - RiverStorage", "Supply From River by - RiverExchange", "Supply From Carrier by - CarrierPriority", "Supply From Carrier by - CarrierStorage", "Supply From Carrier by - TotalSupply", "Water Use from Storage to - StorageUse", "Water Use from Storage to - StorageExchange", "Water Use from Storage to - CarrierUse", "Water Use from Storage to - TotalRelease", "Other - Evap", "Other - SeepSpill", "Other - SimEOM", "Other - TargetLimit", "Other - FillLimit", "Station Balance - Inflow", "Station Balance - Outflow"};

	private static readonly string[] __output_ts_data_types_reservoir_0901 = new string[] {"General - InitialStorage", "Supply From River by - RiverPriority", "Supply From River by - RiverStorage", "Supply From River by - RiverExchange", "Supply From Carrier by - CarrierPriority", "Supply From Carrier by - CarrierStorage", "Supply From Carrier by - TotalSupply", "Water Use from Storage to - StorageUse", "Water Use from Storage to - StorageExchange", "Water Use from Storage to - CarrierUse", "Water Use from Storage to - TotalRelease", "Other - Evap", "Other - SeepSpill", "Other - SimEOM", "Other - TargetLimit", "Other - FillLimit", "Station Balance - RiverInflow", "Station Balance - TotalRelease", "Station Balance - TotalSupply", "Station Balance - RiverByWell", "Station Balance - RiverOutflow"};

	private static readonly string[] __output_ts_data_types_reservoir_0969 = new string[] {"General - Initial_Storage", "Supply From River by - River_Priority", "Supply From River by - River_Storage", "Supply From River by - River_Exchange", "Supply From Carrier by - Carrier_Priority", "Supply From Carrier by - Carrier_Storage", "Supply From Carrier by - Total_Supply", "Water Use from Storage to - Storage_Use", "Water Use from Storage to - Storage_Exchange", "Water Use from Storage to - Carrier_Use", "Water Use from Storage to - Total_Release", "Other - Evap", "Other - Seep_Spill", "Other - Sim_EOM", "Other - Target_Limit", "Other - Fill_Limit", "Station Balance - River_Inflow", "Station Balance - Total_Release", "Station Balance - Total_Supply", "Station Balance - River_By_Well", "Station Balance - River_Outflow"};

	/// <summary>
	/// The instream flow station parameters are written to
	/// the *.xdg file by StateMod's -report module.  The raw monthly data are in the
	/// .B43 (monthly) binary output file.  The raw daily data are in the *.B49 (daily) binary output file.
	/// As per Ray Bennett 2003-11-05 email, all parameters are valid for output.
	/// </summary>
	private static readonly string[] __output_ts_data_types_instream_0100 = new string[] {"Demand - ConsDemand", "Water Supply - FromRiverByPriority", "Water Supply - FromRiverByStorage", "Water Supply - FromRiverByExchange", "Water Supply - TotalSupply", "Shortage - Short", "Water Use - WaterUse,TotalReturn", "Station In/Out - UpstreamInflow", "Station In/Out - ReachGain", "Station In/Out - ReturnFlow", "Station Balance - RiverInflow", "Station Balance - RiverDivert", "Station Balance - RiverOutflow"};

	private static readonly string[] __output_ts_data_types_instream_0901 = new string[] {"Demand - ConsDemand", "Water Supply - FromRiverByPriority", "Water Supply - FromRiverByStorage", "Water Supply - FromRiverByExchange", "Water Supply - TotalSupply", "Shortage - Short", "Water Use - WaterUse,TotalReturn", "Station In/Out - UpstreamInflow", "Station In/Out - ReachGain", "Station In/Out - ReturnFlow", "Station Balance - RiverInflow", "Station Balance - RiverDivert", "Station Balance - RiverOutflow", "Available Flow - AvailFlow"};

	private static readonly string[] __output_ts_data_types_instream_0969 = new string[] {"Demand - Total_Demand", "Demand - CU_Demand", "Water Supply - From_River_By_Priority", "Water Supply - From_River_By_Storage", "Water Supply - From_River_By_Exchange", "Water Supply - From_River_By_Well", "Water Supply - From_Carrier_By_Priority", "Water Supply - From_Carrier_By_Storage", "Water Supply - Carried_Water", "Water Supply - From_Soil", "Water Supply - Total_Supply", "Shortage - Total_Short", "Shortage - CU_Short", "Water Use - Consumptive_Use", "Water Use - To_Soil", "Water Use - Total_Return", "Water Use - Loss", "Station In/Out - Upstream_Inflow", "Station In/Out - Reach_Gain", "Station In/Out - Return_Flow", "Station In/Out - Well_Depletion", "Station In/Out - To_From_GW_Storage", "Station Balance - River_Inflow", "Station Balance - River_Divert", "Station Balance - River_By_Well", "Station Balance - River_Outflow", "Available Flow - Available_Flow"};

	/// <summary>
	/// The diversion station parameters are written to
	/// the *.xdg file by StateMod's -report module.  The raw data are in the *.B43 (monthly) binary output file.
	/// As per Ray Bennett 2003-11-05 email, all parameters are valid for output.
	/// </summary>
	private static readonly string[] __output_ts_data_types_diversion_0100 = new string[] {"Demand - ConsDemand", "Water Supply - FromRiverByPriority", "Water Supply - FromRiverByStorage", "Water Supply - FromRiverByExchange", "Water Supply - FromCarrierByPriority", "Water Supply - FromCarierByStorage", "Water Supply - CarriedWater", "Water Supply - TotalSupply", "Shortage - Short", "Water Use - ConsumptiveWaterUse", "Water Use - WaterUse,TotalReturn", "Station In/Out - UpstreamInflow", "Station In/Out - ReachGain", "Station In/Out - ReturnFlow", "Station Balance - RiverInflow", "Station Balance - RiverDivert", "Station Balance - RiverOutflow"};

	private static readonly string[] __output_ts_data_types_diversion_0901 = new string[] {"Demand - ConsDemand", "Water Supply - FromRiverByPriority", "Water Supply - FromRiverByStorage", "Water Supply - FromRiverByExchange", "Water Supply - FromWell", "Water Supply - FromCarrierByPriority", "Water Supply - FromCarierByStorage", "Water Supply - CarriedWater", "Water Supply - TotalSupply", "Shortage - Short", "Water Use - ConsumptiveWaterUse", "Water Use - WaterUse,TotalReturn", "Station In/Out - UpstreamInflow", "Station In/Out - ReachGain", "Station In/Out - ReturnFlow", "Station In/Out - WellDepletion", "Station In/Out - To/FromGWStorage", "Station Balance - RiverInflow", "Station Balance - RiverDivert", "Station Balance - RiverByWell", "Station Balance - RiverOutflow", "Available Flow - AvailableFlow"};

	private static readonly string[] __output_ts_data_types_diversion_0969 = new string[] {"Demand - Total_Demand", "Demand - CU_Demand", "Water Supply - From_River_By_Priority", "Water Supply - From_River_By_Storage", "Water Supply - From_River_By_Exchange", "Water Supply - From_River_By_Well", "Water Supply - From_Carrier_By_Priority", "Water Supply - From_Carrier_By_Storage", "Water Supply - Carried_Water", "Water Supply - From_Soil", "Water Supply - Total_Supply", "Shortage - Total_Short", "Shortage - CU_Short", "Water Use - Consumptive_Use", "Water Use - To_Soil", "Water Use - Total_Return", "Water Use - Loss", "Station In/Out - Upstream_Inflow", "Station In/Out - Reach_Gain", "Station In/Out - Return_Flow", "Station In/Out - Well_Depletion", "Station In/Out - To_From_GW_Storage", "Station Balance - River_Inflow", "Station Balance - River_Divert", "Station Balance - River_By_Well", "Station Balance - River_Outflow", "Available Flow - Available_Flow"};

		// Ray Bennett says not to include these (2003-11-05 email) - they
		// are useful internally but users should not see...
		//"Divert_For_Instream_Flow",
		//"Divert_For_Power",
		//"Diversion_From_Carrier",
		// "N/A"
		// "Structure Type"
		// "Number of Structures at Node"

	/// <summary>
	/// The well station parameters are written to
	/// the *.wdg file by StateMod's -report module.  The raw monthly data are in the
	/// .B42 (monthly) binary output file.  The raw daily data are in the *.B65 (daily)
	/// binary output file.
	/// </summary>
	private static readonly string[] __output_ts_data_types_well_0901 = new string[] {"Demand - Demand", "Water Supply - FromWell", "Water Supply - FromOther", "Shortage - Short", "Water Use - ConsumptiveUse", "Water Use - Return", "Water Use - Loss", "Water Source - River", "Water Source - GWStor", "Water Source - Salvage"};

	public static readonly string[] __output_ts_data_types_well_0969 = new string[] {"Demand - Total_Demand", "Demand - CU_Demand", "Water Supply - From_Well", "Water Supply - From_SW", "Water Supply - From_Soil", "Water Supply - Total_Supply", "Shortage - Total_Short", "Shortage - CU_Short", "Water Use - Total_CU", "Water Use - To_Soil", "Water Use - Total_Return", "Water Use - Loss", "Water Use - Total_Use", "Water Source - From_River", "Water Source - To_From_GW_Storage", "Water Source - From_Salvage", "Water Source - From_Soil", "Water Source - Total_Source"};

	/// <summary>
	/// The version of StateMod that is being run as a number (e.g., 11.5).  This is normally set at the
	/// beginning of a StateMod GUI session by calling runStateMod ( ... "-version" ).
	/// Then its value can be checked with getStateModVersion();
	/// </summary>
	private static string __statemodVersion = "";

	/// <summary>
	/// The StateMod revision date.  This is normally set at the
	/// beginning of a StateMod GUI session by calling runStateMod ( ... "-version" ).
	/// Then its value can be checked with getStateModVersion();
	/// </summary>
	private static string __statemodRevisionDate = "Unknown";

	/// <summary>
	/// The latest known version is returned by getStateModVersionLatest() as a
	/// default.  This is used by StateMod_BTS when requesting parameters.
	/// </summary>
	private static string __statemodVersionLatest = "12.20";

	/// <summary>
	/// The program to use when running StateMod.  If relying on the path, this should just be the
	/// program name.  However, a full path can be specified to override the PATH.
	/// See the system/StateModGUI.cfg file for configuration properties, which should be used to set
	/// the executable as soon as the GUI starts.
	/// </summary>
	private static string __statemodExecutable = "StateMod";

	/// <summary>
	/// The program to use when running the SmDelta utility program.  If relying on the path, this should just be the
	/// program name.  However, a full path can be specified to override the PATH.
	/// See the system/StateModGUI.cfg file for configuration properties.
	/// </summary>
	private static string __smdeltaExecutable = "SmDelta";

	/// <summary>
	/// Turns an array of Strings into a list of Strings.
	/// </summary>
	public static IList<string> arrayToList(string[] array)
	{
		IList<string> v = new List<string>();

		if (array == null)
		{
			return v;
		}

		for (int i = 0; i < array.Length; i++)
		{
			v.Add(array[i]);
		}

		return v;
	}

	// TODO SAM 2004-09-07 JTS needs to javadoc.
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static double calculateTimeSeriesDifference(RTi.TS.TS ts1, RTi.TS.TS ts2, boolean percent) throws Exception
	public static double calculateTimeSeriesDifference(TS ts1, TS ts2, bool percent)
	{ // Loop through the time series and convert to yearly...  Do it the brute force way right now...

		if (ts1 == null || ts2 == null)
		{
			return -999.0;
		}

		TS[] tsArray = new TS[2];
		tsArray[0] = ts1;
		tsArray[1] = ts2;

		DateTime dt1 = null;
		DateTime dt2 = null;
		double total = 0.0;
		double value = 0.0;
		int count = 0;
		TSIdent tsident = null;
		YearTS yts = null;
		YearTS[] ytsArray = new YearTS[2];

		for (int its = 0; its < 2; its++)
		{
			if (tsArray[its].getDataIntervalBase() == TimeInterval.YEAR)
			{
				// Just add to the list...
				yts = (YearTS)tsArray[its];
			}
			else if (tsArray[its].getDataIntervalBase() == TimeInterval.MONTH)
			{
				// Create a new time series and accumulate...
				tsident = new TSIdent(tsArray[its].getIdentifier());
				tsident.setInterval("Year");
				yts = new YearTS();
				yts.setIdentifier(tsident);
				yts.setDescription(tsArray[its].getDescription());
				yts.setDate1(tsArray[its].getDate1());
				yts.setDate2(tsArray[its].getDate2());
				yts.setDataUnits(tsArray[its].getDataUnits());
				yts.allocateDataSpace();
				dt1 = new DateTime(tsArray[its].getDate1());
				// Accumulate in calendar time...
				dt1.setMonth(1);
				dt2 = tsArray[its].getDate2();
				dt2.setMonth(12);
				for (; dt1.lessThanOrEqualTo(dt2); dt1.addMonth(1))
				{
					value = tsArray[its].getDataValue(dt1);
					if (!tsArray[its].isDataMissing(value))
					{
						total += value;
						++count;
					}
					if (dt1.getMonth() == 12)
					{
						// Transfer to year time series only if all data are available in month...
						if (count == 12)
						{
							yts.setDataValue(dt1, total);
						}
						// Reset the accumulators...
						total = 0.0;
						count = 0;
					}
				}
			}

			// Add to the list...
			ytsArray[its] = yts;
		}

		dt1 = new DateTime(ytsArray[0].getDate1());
		if (ytsArray[1].getDate1().lessThan(dt1))
		{
			dt1 = new DateTime(ytsArray[1].getDate1());
		}
		dt2 = new DateTime(ytsArray[0].getDate2());
		if (ytsArray[1].getDate2().greaterThan(dt2))
		{
			dt2 = new DateTime(ytsArray[1].getDate2());
		}

		count = 0;
		double total1 = 0;
		double total2 = 0;
		double value1 = 0;
		double value2 = 0;
		while (dt1.lessThanOrEqualTo(dt2))
		{
			value1 = ytsArray[0].getDataValue(dt1);
			value2 = ytsArray[1].getDataValue(dt1);
			if (!ytsArray[0].isDataMissing(value1) && !ytsArray[1].isDataMissing(value2))
			{
				total1 += value1;
				total2 += value2;
				count++;
			}
			dt1.addYear(1);
		}

		double result = -999.0; // Unable to calculate
		if (count > 0)
		{
			result = (total1 - total2) / count;

			if (percent)
			{
				if (total1 != 0.0)
				{
					result = (result / total1) * 100;
				}
			}
		}

		return result;
	}

	/// <summary>
	/// Fills in missing values with a space.  This is needed for the HTML table
	/// to show a blank value and not an empty cell. </summary>
	/// <param name="String">[] data - The array of data to check. </param>
	/// <returns> String[] - Formatted data array. </returns>
	public static string[] checkForMissingValues(string[] data)
	{
		if (data != null)
		{
			for (int i = 0; i < data.Length; i++)
			{
				string element = data[i].Trim();
				if (string.ReferenceEquals(element, null) || element.Length == 0)
				{
					data[i] = " ";
				}
				else
				{
					data[i] = element;
				}
			}
		}
		return data;
	}

	// TODO SAM 2005-03-03 This simple test needs to be evaluated to determine
	// if it should be supported in all the file types.  For example, this code
	// could be moved to each StateMod class.
	/// <summary>
	/// Compare similar StateMod files and generate a summary of the files, with differences. </summary>
	/// <param name="path1"> Path to first file. </param>
	/// <param name="path2"> Path to second file. </param>
	/// <param name="comp_type"> Component type. </param>
	/// <exception cref="Exception"> if an error is generated. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.List<String> compareFiles(String path1, String path2, int comp_type) throws Exception
	public static IList<string> compareFiles(string path1, string path2, int comp_type)
	{
		IList<string> v = new List<string>(50);
		string full_path1 = IOUtil.getPathUsingWorkingDir(path1);
		string full_path2 = IOUtil.getPathUsingWorkingDir(path2);
		int n1, n2; // Size of data vectors
		int i, pos, size;
		StringBuilder b = new StringBuilder();
		if (comp_type == StateMod_DataSet.COMP_WELL_RIGHTS)
		{
			StateMod_WellRight wer1, wer2;
			IList<StateMod_WellRight> data1_Vector = StateMod_WellRight.readStateModFile(full_path1);
			n1 = data1_Vector.Count;
			IList<StateMod_WellRight> data2_Vector = StateMod_WellRight.readStateModFile(full_path2);
			n2 = data2_Vector.Count;
			IList<string> allids_Vector = new List<string>(n1 * 3 / 2); // guess at size
			double decree;
			double decree1_alltotal = 0.0; // All decrees in file
			double decree2_alltotal = 0.0;
			int missing_n1 = 0;
			int missing_n2 = 0;
			IList<StateMod_WellRight> onlyin1_Vector = new List<StateMod_WellRight>();
			IList<StateMod_WellRight> onlyin2_Vector = new List<StateMod_WellRight>();
			// Get a list of all identifiers.  This is used to summarize results by location...
			for (i = 0; i < n1; i++)
			{
				// Add to list of all identifiers...
				wer1 = data1_Vector[i];
				allids_Vector.Add(wer1.getCgoto());
			}
			for (i = 0; i < n2; i++)
			{
				// Add to list of all identifiers...
				wer2 = data2_Vector[i];
				allids_Vector.Add(wer2.getCgoto());
			}
			// Sort all the identifiers...
			allids_Vector = StringUtil.sortStringList(allids_Vector, StringUtil.SORT_ASCENDING, null, false, true);
			Message.printStatus(1, "", "Count after sort= " + allids_Vector.Count);
			// Remove the duplicates...
			StringUtil.removeDuplicates(allids_Vector, true, true);
			Message.printStatus(1, "","Count after removing duplicates= " + allids_Vector.Count);
			int nall = allids_Vector.Count;
			// Initialize the totals for each location...
			double[] decree1_total = new double[nall];
			double[] decree2_total = new double[nall];
			for (i = 0; i < nall; i++)
			{
				decree1_total[i] = -999.0;
				decree2_total[i] = -999.0;
			}

			// Now process each list...

			for (i = 0; i < n1; i++)
			{
				wer1 = data1_Vector[i];
				decree = wer1.getDcrdivw();
				if (StateMod_Util.isMissing(decree))
				{
					++missing_n1;
				}
				else
				{
					// Data set...
					decree1_alltotal += decree;
					// By structure...
					pos = StringUtil.IndexOf(allids_Vector, wer1.getCgoto());
					if (pos >= 0)
					{
						if (decree1_total[pos] < 0.0)
						{
							decree1_total[pos] = decree;
						}
						else
						{
							decree1_total[pos] += decree;
						}
					}
				}
				// Search other list...
				pos = StateMod_Util.IndexOf(data2_Vector, wer1.getID());
				if (pos < 0)
				{
					onlyin1_Vector.Add(wer1);
				}
			}
			for (i = 0; i < n2; i++)
			{
				wer2 = data2_Vector[i];
				decree = wer2.getDcrdivw();
				if (StateMod_Util.isMissing(decree))
				{
					++missing_n2;
				}
				else
				{
					// By data set...
					decree2_alltotal += decree;
					// By structure...
					pos = StringUtil.IndexOf(allids_Vector, wer2.getCgoto());
					if (pos >= 0)
					{
						if (decree2_total[pos] < 0.0)
						{
							decree2_total[pos] = decree;
						}
						else
						{
							decree2_total[pos] += decree;
						}
					}
				}
				// Search other list...
				pos = StateMod_Util.IndexOf(data1_Vector, wer2.getID());
				if (pos < 0)
				{
					onlyin2_Vector.Add(wer2);
				}
			}

			// Now print the results...

			v.Add("First file:            " + full_path1);
			v.Add("Number of rights:      " + n1);
			v.Add("Total decrees (CFS):   " + StringUtil.formatString(decree1_alltotal,"%.2f"));
			v.Add("Second file:           " + full_path2);
			v.Add("Number of rights:      " + n2);
			v.Add("Total decrees (CFS):   " + StringUtil.formatString(decree2_alltotal,"%.2f"));
			v.Add("");
			v.Add("Summary of decree differences, by location:");
			v.Add("");
			v.Add("Well ID      | File2 Total | File1 Total | File2-File1");
			for (i = 0; i < nall; i++)
			{
				b.Length = 0;
				b.Append(StringUtil.formatString(allids_Vector[i],"%-12.12s") + " | ");
				if (decree2_total[i] >= 0.0)
				{
					b.Append(StringUtil.formatString(decree2_total[i],"%11.2f") + " | ");
				}
				else
				{
					b.Append("            | ");
				}
				if (decree1_total[i] >= 0.0)
				{
					b.Append(StringUtil.formatString(decree1_total[i],"%11.2f") + " | ");
				}
				else
				{
					b.Append("            | ");
				}
				if ((decree1_total[i] >= 0.0) && (decree2_total[i] >= 0.0))
				{
					b.Append(StringUtil.formatString((decree2_total[i] - decree1_total[i]),"%11.2f"));
				}
				else
				{
					b.Append("           ");
				}
				v.Add(b.ToString());
			}
			v.Add("Total        | " + StringUtil.formatString(decree2_alltotal,"%11.2f") + " | " + StringUtil.formatString(decree1_alltotal,"%11.2f") + " | " + StringUtil.formatString((decree2_alltotal - decree1_alltotal),"%11.2f"));
			v.Add("");
			v.Add("First file:");
			v.Add("");
			v.Add("Rights with no decree: " + missing_n1);
			size = onlyin1_Vector.Count;
			if (size == 0)
			{
				v.Add("Rights only in first file:");
				v.Add("All are found in 2nd file.");
			}
			else
			{
				v.Add("Rights only in first file (" + size + " total):");
				v.Add("    ID           Decree    AdminNumber");
				for (i = 0; i < size; i++)
				{
					wer1 = (StateMod_WellRight)onlyin1_Vector[i];
					v.Add(StringUtil.formatString(wer1.getID(),"%-12.12s") + " " + StringUtil.formatString(wer1.getDcrdivw(),"%11.2f") + " " + wer1.getIrtem());
				}
			}
			v.Add("");
			v.Add("Second file:");
			v.Add("");
			v.Add("Rights with no decree: " + missing_n2);
			size = onlyin2_Vector.Count;
			if (size == 0)
			{
				v.Add("Rights only in second file:");
				v.Add("All are found in 1st file.");
			}
			else
			{
				v.Add("Rights only in second file (" + size + " total):");
				v.Add("    ID           Decree    AdminNumber");
				for (i = 0; i < size; i++)
				{
					wer2 = (StateMod_WellRight)onlyin2_Vector[i];
					v.Add(StringUtil.formatString(wer2.getID(),"%-12.12s") + " " + StringUtil.formatString(wer2.getDcrdivw(),"%11.2f") + " " + wer2.getIrtem());
				}
			}
		}
		return v;
	}

	/// <summary>
	/// Create a label for a single data objects, for use in choices, etc. </summary>
	/// <returns> a String containing formatted identifiers and names. </returns>
	/// <param name="smdata"> A single StateMod_Data object. </param>
	/// <param name="include_name"> If false, the string will consist of only the value
	/// returned from StateMod_Data.getID().  If true the string will contain the ID,
	/// followed by " - xxxx", where xxxx is the value returned from
	/// StateMod_Data.getName().  If the identifier and name are the same only one part will be returned. </param>
	public static string createDataLabel(StateMod_Data smdata, bool include_name)
	{
		if (smdata == null)
		{
			return "";
		}
		string id = "", name = "";
		id = smdata.getID();
		name = smdata.getName();
		if (id.Equals(name, StringComparison.OrdinalIgnoreCase))
		{
			return id;
		}
		else if (include_name)
		{
			return id + " - " + name;
		}
		else
		{
			return id;
		}
	}

	/// <summary>
	/// Create a list of data objects, for use in choices, etc -- this method differs
	/// from createDataList in that it contains the Cgoto instead of the ID. </summary>
	/// <returns> a list of String containing formatted identifiers and names.  A
	/// non-null list is guaranteed; however, the list may have zero items. </returns>
	/// <param name="include_name"> If false, each string will consist of only the value
	/// returned from StateMod_Data.getID().  If true the string will contain the ID,
	/// followed by " - xxxx", where xxxx is the value returned from StateMod_Data.getName(). </param>
	public static IList<string> createCgotoDataList(IList<StateMod_Data> smdata_Vector, bool include_name)
	{
		IList<string> v = null;
		if (smdata_Vector == null)
		{
			v = new List<string>();
			return v;
		}
		else
		{
			// This optimizes memory management...
			v = new List<string> (smdata_Vector.Count);
		}
		int size = smdata_Vector.Count;
		StateMod_Data smdata;
		string cgoto = "", name = "";
		TS ts;
		object o;
		for (int i = 0; i < size; i++)
		{
			o = smdata_Vector[i];
			if (o == null)
			{
				continue;
			}
			if (o is StateMod_Data)
			{
				smdata = (StateMod_Data)o;
				cgoto = smdata.getCgoto();
				name = smdata.getName();
			}
			else if (o is TS)
			{
				ts = (TS)o;
				cgoto = ts.getLocation();
				name = ts.getDescription();
			}
			else
			{
				Message.printWarning(2,"StateMod_Util.createDataList", "Unrecognized StateMod data.");
			}
			if (cgoto.Equals(name, StringComparison.OrdinalIgnoreCase))
			{
				v.Add(cgoto);
			}
			else if (include_name)
			{
				v.Add(cgoto + " - " + name);
			}
			else
			{
				v.Add(cgoto);
			}
		}
		return v;
	}

	/// <summary>
	/// Create a daily estimate a time series, for viewing only.  The time series
	/// should match the estimate that StateMod will make at run time. </summary>
	/// <returns> a new time series containing the daily estimate, or null if the
	/// estimate cannot be created.  The period of the daily time series will be that of the monthly time series. </returns>
	/// <param name="id"> Identifier (location) for the time series. </param>
	/// <param name="desc"> Description for the time series. </param>
	/// <param name="datatype"> Data type for the time series. </param>
	/// <param name="units"> Data units for the time series. </param>
	/// <param name="dayflag"> The daily ID flag from diversion stations, etc.  The daily time
	/// series is estimated using one of the following methods:
	/// <ol>
	/// <li>	A value of "0" will estimate an average daily CFS value from the monthly
	/// ACFT by assuming 30 days in the month.</li>
	/// <li>	A value of "4" will estimate a daily CFS time series by interpolating
	/// from the mid-points of the monthly time series (day 15).</li>
	/// <li>	Other values indicate that the monthly total time series should be used
	/// to get the ACFT total for the month and the daily values should use the
	/// pattern in the supplied daily time series to generate a new daily time series.<li>
	/// </ol> </param>
	/// <param name="monthts"> The monthly time series needed for estimation. </param>
	/// <param name="dayts"> The daily time series needed for estimation. </param>
	public static DayTS createDailyEstimateTS(string id, string desc, string datatype, string units, string dayflag, MonthTS monthts, DayTS dayts)
	{
		DayTS estdayts = null;

		string tsid = id + ".StateMod." + datatype + ".Day.Estimated";
		DateTime date1;
		DateTime date2;
		if (dayflag.Equals("0"))
		{
			// Convert the monthly time series to daily by assuming the monthly value is the average daily value...
			if (monthts == null)
			{
				return null;
			}
			try
			{
				estdayts = (DayTS)TSUtil.newTimeSeries(tsid, true);
				estdayts.setIdentifier(tsid);
			}
			catch (Exception)
			{
				// Should not occur.
			}
			estdayts.setDescription(desc);
			estdayts.setDataUnits(units);
			date1 = new DateTime(monthts.getDate1());
			date1.setPrecision(DateTime.PRECISION_DAY);
			date1.setDay(1);
			date2 = new DateTime(monthts.getDate2());
			date2.setPrecision(DateTime.PRECISION_DAY);
			date2.setDay(TimeUtil.numDaysInMonth(date2));
			estdayts.setDate1(date1);
			estdayts.setDate1(date1);
			estdayts.setDate2(date2);
			estdayts.setDate2(date2);
			if (estdayts.allocateDataSpace() != 0)
			{
				return null;
			}
			// Iterate based on the daily data and grab data from the monthly when day = 1...
			double value = 0.0;
			int ndays_in_month;
			for (DateTime date = new DateTime(date1); date.lessThanOrEqualTo(date2); date.addInterval(TimeInterval.DAY,1))
			{
				if (date.getDay() == 1)
				{
					// Get a new value from the monthly time series...
					value = monthts.getDataValue(date);
					if (!monthts.isDataMissing(value))
					{
						ndays_in_month = TimeUtil.numDaysInMonth(date);
						if (units.Equals("cfs", StringComparison.OrdinalIgnoreCase))
						{
							// Monthlies are always ACFT so convert to CFS...
							// (ACFT/daysInMonth)(43560FT2/AC)(1day/86400s).
							value = value / (1.9835 * ndays_in_month);
						}
						// Else leave value as the monthly value for assignment.
					}
				}
				estdayts.setDataValue(date, value);
			}
			if (units.Equals("cfs", StringComparison.OrdinalIgnoreCase))
			{
				estdayts.addToGenesis("Daily data were estimated by using monthly data as average" + " daily values (divide by 1.9835*DaysInMonth).");
			}
			else
			{
				estdayts.addToGenesis("Daily data were estimated by using monthly data as average" + " daily values (Assign monthly value to daily).");
			}
		}
		else if (dayflag.Equals("4"))
		{
			// Convert the monthly time series to daily by interpolating the midpoint values.
			if (monthts == null)
			{
				return null;
			}
			try
			{
				estdayts = (DayTS)TSUtil.newTimeSeries(tsid, true);
				estdayts.setIdentifier(tsid);
			}
			catch (Exception)
			{
				// Should not occur.
			}
			estdayts.setDescription(desc);
			estdayts.setDataUnits(units);
			date1 = new DateTime(monthts.getDate1());
			date1.setPrecision(DateTime.PRECISION_DAY);
			date1.setDay(1);
			date2 = new DateTime(monthts.getDate2());
			date2.setPrecision(DateTime.PRECISION_DAY);
			date2.setDay(TimeUtil.numDaysInMonth(date2));
			estdayts.setDate1(date1);
			estdayts.setDate1(date1);
			estdayts.setDate2(date2);
			estdayts.setDate2(date2);
			if (estdayts.allocateDataSpace() != 0)
			{
				return null;
			}
			// Iterate based on the daily data and grab data from the monthly when day = 1...
			double value = 0.0;
			int ndays_in_month;
			for (DateTime date = new DateTime(date1); date.lessThanOrEqualTo(date2); date.addInterval(TimeInterval.DAY,1))
			{
				if (date.getDay() == 1)
				{
					// Get a new value from the monthly time series...
					value = monthts.getDataValue(date);
					if (!monthts.isDataMissing(value))
					{
						ndays_in_month = TimeUtil.numDaysInMonth(date);
						if (units.Equals("cfs", StringComparison.OrdinalIgnoreCase))
						{
							// Monthlies are always ACFT so convert to CFS...
							// (ACFT/daysInMonth)(43560FT2/AC)(1day/86400s).
							value = value / (1.9835 * ndays_in_month);
						}
						// Else leave value as the monthly value for assignment.
					}
				}
				estdayts.setDataValue(date, value);
			}
			if (units.Equals("cfs", StringComparison.OrdinalIgnoreCase))
			{
				estdayts.addToGenesis("Daily data were estimated by using monthly data as average" + " daily values (divide by 1.9835*DaysInMonth).");
			}
			else
			{
				estdayts.addToGenesis("Daily data were estimated by using monthly data as average" + " daily values (Assign monthly value to daily).");
			}
		}
		return estdayts;
	}

	/// <summary>
	/// Create a list of strings for use in operating rule associated plan choices. </summary>
	/// <returns> a list of String containing formatted identifiers and names.  A
	/// non-null list is guaranteed; however, the list may have zero items. </returns>
	/// <param name="associatedPlanAllowedTypes"> a list of allowed plan types </param>
	/// <param name="includeName"> If false, each string will consist of only the value
	/// returned from StateMod_Data.getID().  If true the string will contain the ID,
	/// followed by " - xxxx", where xxxx is the value returned from StateMod_Data.getName(). </param>
	/// <param name="type"> if non-null and non-blank, include as "(type)" before the name, to indicate a location type,
	/// for example "ID - (T&C Plan) Name". </param>
	public static IList<string> createIdentifierList(StateMod_DataSet dataset, StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] associatedPlanAllowedTypes, bool includeName)
	{
		IList<string> idList = new List<string>();
		// Return a list of plans identifiers that match the requested types
		// Loop through the source/destination types...
		StateMod_OperationalRight_Metadata_SourceOrDestinationType sourceOrDestType;
		for (int i = 0; i < associatedPlanAllowedTypes.Length; i++)
		{
			// Get the data objects
			sourceOrDestType = associatedPlanAllowedTypes[i].getMatchingSourceOrDestinationType();
			System.Collections.IList dataList = getDataList(sourceOrDestType, dataset, null, false);
			// Format the identifiers...
			string type = "" + associatedPlanAllowedTypes[i];
			if ((dataList != null) && (dataList.Count > 0))
			{
				// Format the identifiers...
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<String> idList2 = createIdentifierListFromStateModData((java.util.List<StateMod_Data>)dataList, includeName, type);
				IList<string> idList2 = createIdentifierListFromStateModData((IList<StateMod_Data>)dataList, includeName, type);
				// Add to the list (probably no need to sort, especially since type is included
				((IList<string>)idList).AddRange(idList2);
			}
		}
		return idList;
	}

	/// <summary>
	/// Create a list of strings for use in operating rule source/destination choices. </summary>
	/// <returns> a list of String containing formatted identifiers and names.  A
	/// non-null list is guaranteed; however, the list may have zero items. </returns>
	/// <param name="smdataList"> a list of StateMod_Data or TS objects </param>
	/// <param name="includeName"> If false, each string will consist of only the value
	/// returned from StateMod_Data.getID().  If true the string will contain the ID,
	/// followed by " - xxxx", where xxxx is the value returned from StateMod_Data.getName(). </param>
	/// <param name="type"> if non-null and non-blank, include as "(type)" before the name, to indicate a location type,
	/// for example "ID - (Reservoir) Name". </param>
	public static IList<string> createIdentifierList(StateMod_DataSet dataset, StateMod_OperationalRight_Metadata_SourceOrDestinationType[] sourceOrDestTypes, bool includeName)
	{
		IList<string> idList = new List<string>();
		// Loop through the source/destination types...
		for (int i = 0; i < sourceOrDestTypes.Length; i++)
		{
			// Get the data objects
			// TODO SAM 2011-02-02 Why was true added below?
			//List dataList = getDataList(sourceOrDestTypes[i], dataset, null, true);
			System.Collections.IList dataList = getDataList(sourceOrDestTypes[i], dataset, null, false);
			if ((dataList != null) && (dataList.Count > 0))
			{
				// Format the identifiers...
				string type = "" + sourceOrDestTypes[i];
				object object0 = dataList[0];
				if (object0 is StateMod_Data)
				{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<String> idList2 = createIdentifierListFromStateModData((java.util.List<StateMod_Data>)dataList, includeName, type);
					IList<string> idList2 = createIdentifierListFromStateModData((IList<StateMod_Data>)dataList, includeName, type);
					// Add to the list (probably no need to sort, especially since type is included
					((IList<string>)idList).AddRange(idList2);
				}
				else if (object0 is TS)
				{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<String> idList2 = createIdentifierListFromTS((java.util.List<RTi.TS.TS>)dataList, includeName, type);
					IList<string> idList2 = createIdentifierListFromTS((IList<TS>)dataList, includeName, type);
					// Add to the list (probably no need to sort, especially since type is included
					((IList<string>)idList).AddRange(idList2);
				}
			}
		}
		return idList;
	}

	/// <summary>
	/// Create a list of strings for use in choices, etc. </summary>
	/// <returns> a list of String containing formatted identifiers and names.  A
	/// non-null list is guaranteed; however, the list may have zero items. </returns>
	/// <param name="includeName"> If false, each string will consist of only the value
	/// returned from StateMod_Data.getID().  If true the string will contain the ID,
	/// followed by " - xxxx", where xxxx is the value returned from StateMod_Data.getName(). </param>
	public static IList<string> createIdentifierList<T1>(IList<T1> smdataList, bool includeName) where T1 : StateMod_Data
	{
		return createIdentifierListFromStateModData(smdataList, includeName, null);
	}

	/// <summary>
	/// Create a list of strings for use in choices, etc. </summary>
	/// <returns> a list of String containing formatted identifiers and names.  A
	/// non-null list is guaranteed; however, the list may have zero items. </returns>
	/// <param name="smdataList"> a list of StateMod_Data objects </param>
	/// <param name="includeName"> If false, each string will consist of only the value
	/// returned from StateMod_Data.getID().  If true the string will contain the ID,
	/// followed by " - xxxx", where xxxx is the value returned from StateMod_Data.getName(). </param>
	/// <param name="type"> if non-null and non-blank, include as "(type)" before the name, to indicate a location type,
	/// for example "ID - (Reservoir) Name". </param>
	public static IList<string> createIdentifierListFromStateModData<T1>(IList<T1> smdataList, bool includeName, string type) where T1 : StateMod_Data
	{
		IList<string> v = null;
		if (smdataList == null)
		{
			v = new List<string>();
			return v;
		}
		else
		{
			// This optimizes memory management...
			v = new List<string> (smdataList.Count);
		}
		int size = smdataList.Count;
		StateMod_Data smdata;
		string id = "", name = "";
		string typeString = "";
		for (int i = 0; i < size; i++)
		{
			smdata = smdataList[i];
			if (smdata == null)
			{
				continue;
			}
			id = smdata.getID();
			name = smdata.getName();
			if ((!string.ReferenceEquals(type, null)) && (type.Length > 0))
			{
				typeString = "(" + type + ") ";
			}
			if (includeName)
			{
				v.Add(id + " - " + typeString + name);
			}
			else
			{
				v.Add(id);
			}
		}
		return v;
	}

	/// <summary>
	/// Create a list of strings for use in choices, etc. </summary>
	/// <returns> a list of String containing formatted identifiers and names.  A
	/// non-null list is guaranteed; however, the list may have zero items. </returns>
	/// <param name="tslist"> a list of TS objects </param>
	/// <param name="includeName"> If false, each string will consist of only the value
	/// returned from StateMod_Data.getID().  If true the string will contain the ID,
	/// followed by " - xxxx", where xxxx is the value returned from StateMod_Data.getName(). </param>
	/// <param name="type"> if non-null and non-blank, include as "(type)" before the name, to indicate a location type,
	/// for example "ID - (Reservoir) Name". </param>
	public static IList<string> createIdentifierListFromTS(IList<TS> tslist, bool includeName, string type)
	{
		IList<string> v = null;
		if (tslist == null)
		{
			v = new List<string>();
			return v;
		}
		else
		{
			// This optimizes memory management...
			v = new List<string> (tslist.Count);
		}
		int size = tslist.Count;
		string id = "", name = "";
		TS ts;
		string typeString = "";
		for (int i = 0; i < size; i++)
		{
			ts = tslist[i];
			if (ts == null)
			{
				continue;
			}
			id = ts.getLocation();
			name = ts.getDescription();
			if ((!string.ReferenceEquals(type, null)) && (type.Length > 0))
			{
				typeString = "(" + type + ") ";
			}
			if (includeName)
			{
				v.Add(id + " - " + typeString + name);
			}
			else
			{
				v.Add(id);
			}
		}
		return v;
	}

	/// <summary>
	/// Create a sum of the time series in a list, representing the total water for
	/// a data set.  This time series can be used for summaries.
	/// The period of the time series is the maximum of the time series in the list.
	/// The total value will be set if one or more values in the parts is available.  If
	/// no value is available, the total will be missing. </summary>
	/// <returns> a time series that is the sum of all the time series in the input Vector. </returns>
	/// <param name="tslist"> a list of time series to process. </param>
	/// <param name="dataset_location"> the location to use for the total time series. </param>
	/// <param name="dataset_datasource"> the data source to use for the total time series. </param>
	/// <param name="dataset_location"> the description to use for the total time series. </param>
	/// <param name="comp_type"> Component type, used to determine the units and other
	/// information for the new time series. </param>
	/// <exception cref="Exception"> if there is an error creating the time series. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static RTi.TS.TS createTotalTS(java.util.List<RTi.TS.TS> tslist, String dataset_location, String dataset_datasource, String dataset_description, int comp_type) throws Exception
	public static TS createTotalTS(IList<TS> tslist, string dataset_location, string dataset_datasource, string dataset_description, int comp_type)
	{
		string routine = "StateMod_Util.createTotalTS";
		TS newts = null;
		string interval = "";
		if ((comp_type == StateMod_DataSet.COMP_DIVERSION_TS_MONTHLY) || (comp_type == StateMod_DataSet.COMP_DEMAND_TS_MONTHLY) || (comp_type == StateMod_DataSet.COMP_WELL_PUMPING_TS_MONTHLY) || (comp_type == StateMod_DataSet.COMP_WELL_DEMAND_TS_MONTHLY))
		{
			newts = new MonthTS();
			interval = "Month";
		}
		else if ((comp_type == StateMod_DataSet.COMP_DIVERSION_TS_DAILY) || (comp_type == StateMod_DataSet.COMP_DEMAND_TS_DAILY) || (comp_type == StateMod_DataSet.COMP_WELL_PUMPING_TS_DAILY) || (comp_type == StateMod_DataSet.COMP_WELL_DEMAND_TS_DAILY))
		{
			newts = new DayTS();
			interval = "Day";
		}
		else
		{
			Message.printWarning(3, routine, "Cannot create total - cannot handle component type " + comp_type);
			throw new Exception("Cannot create total TS.");
		}
		if ((string.ReferenceEquals(dataset_location, null)) || (dataset_location.Length == 0))
		{
			dataset_location = "DataSet";
		}
		if ((string.ReferenceEquals(dataset_datasource, null)) || (dataset_datasource.Length == 0))
		{
			dataset_datasource = "StateMod";
		}
		if ((string.ReferenceEquals(dataset_description, null)) || (dataset_description.Length == 0))
		{
			dataset_description = "Dataset " + StateMod_DataSet.lookupTimeSeriesDataType(comp_type);
		}
		newts.setIdentifier(new TSIdent(dataset_location, dataset_datasource, StateMod_DataSet.lookupTimeSeriesDataType(comp_type), interval, ""));
		newts.setDataUnits(StateMod_DataSet.lookupTimeSeriesDataUnits(comp_type));
		// Get the period for the total time series...
		// Try to get from the data...
		TSLimits valid_dates = TSUtil.getPeriodFromTS(tslist, TSUtil.MAX_POR);
		DateTime start = valid_dates.getDate1();
		DateTime end = valid_dates.getDate2();
		if ((start == null) || (end == null))
		{
			Message.printStatus(3,routine,"Cannot get period from data.");
			throw new Exception("Cannot create total TS.");
		}
		newts.setDate1(start);
		newts.setDate1Original(start);
		newts.setDate2(end);
		newts.setDate2Original(end);
		newts.allocateDataSpace();
		TSUtil.add(newts, tslist);
		// Reset the description to something short...
		newts.setDescription(dataset_description);
		return newts;
	}

	/// <summary>
	/// Create a long time series for the average monthly time series.  This time
	/// series can be used for displays but should not be edited.  It is assumed that
	/// the input time series is 12 months long.  Therefore, the repeating time series
	/// is created by allocating a time series for the requested period and setting
	/// all January values to the January value of ts, etc.  All of the header
	/// information is retained (description, etc.). </summary>
	/// <returns> a longer time series consisting of repeating the 12 values in the original time series. </returns>
	/// <param name="date1"> Start date for long time series. </param>
	/// <param name="date2"> End date for long time series. </param>
	public static MonthTS createRepeatingAverageMonthTS(TS ts, DateTime date1, DateTime date2)
	{
		MonthTS newts = new MonthTS();
		try
		{
			newts.setIdentifier(new TSIdent(ts.getIdentifier()));
		}
		catch (Exception)
		{
			// Should not happen.
		}
		newts.setDataUnits(ts.getDataUnits());
		newts.setDate1(date1);
		newts.setDate2(date2);
		newts.allocateDataSpace();
		// Get the old data...
		double[] ts_data = new double[12]; // Data [0] = january
		DateTime date = new DateTime(ts.getDate1());
		for (int i = 0; i < 0; i++, date.addMonth(1))
		{
			ts_data[date.getMonth() - 1] = ts.getDataValue(date);
		}
		// Fill the data in the new time series...
		date = new DateTime(date1);
		for (; date.lessThanOrEqualTo(date2); date.addMonth(1))
		{
			newts.setDataValue(date, ts_data[date.getMonth() - 1]);
		}
		return newts;
	}

	/// <summary>
	/// Create a water right time series, in which each interval has a value of the
	/// total water rights in effect at the time.  Switches are considered.  This method
	/// can be used when processing rights for a single structure (e.g., when plotting rights in the StateMod GUI). </summary>
	/// <param name="smdata"> A StateMod data object (e.g., StateMod_Diversion) that the time series is being created for. </param>
	/// <param name="interval"> TimeInterval.MONTH or TimeInterval.DAY. </param>
	/// <param name="units"> Data units for the time series. </param>
	/// <param name="date1"> Starting date for the time series.  Must not be null. </param>
	/// <param name="date2"> Ending date for the time series.  Must not be null. </param>
	public static TS createWaterRightTS(StateMod_Data smdata, int interval, string units, DateTime date1, DateTime date2)
	{
		TS ts = null;
		string tsid = null;
		if (interval == TimeInterval.MONTH)
		{
			tsid = smdata.getID() + ".StateMod.TotalWaterRights.Month";
			try
			{
				ts = (MonthTS)TSUtil.newTimeSeries(tsid, true);
				ts.setIdentifier(tsid);
			}
			catch (Exception)
			{
				// Should not occur.
			}
		}
		else
		{
			tsid = smdata.getID() + ".StateMod.TotalWaterRights.Day";
			try
			{
				ts = (DayTS)TSUtil.newTimeSeries(tsid, true);
				ts.setIdentifier(tsid);
			}
			catch (Exception)
			{
				// Should not occur.
			}
		}
		ts.setDescription(smdata.getName());
		ts.setDataUnits(units);
		ts.setDate1(date1);
		ts.setDate1Original(date1);
		ts.setDate2(date2);
		ts.setDate2Original(date2);
		// Initialize to zero...
		if (interval == TimeInterval.MONTH)
		{
			if (((MonthTS)ts).allocateDataSpace(0.0) != 0)
			{
				return null;
			}
		}
		else
		{
			if (((DayTS)ts).allocateDataSpace(0.0) != 0)
			{
				return null;
			}
		}

		// Loop through each water right...

		int size = 0;
		int onoff = 0;
		double decree = 0.0;
		StateMod_DiversionRight dright = null;
		StateMod_Diversion div = null;
		DateTime fill_date1 = null; // Dates to fill the water right time
		DateTime fill_date2 = null; // series.
		if (smdata is StateMod_Diversion)
		{
			div = (StateMod_Diversion)smdata;
			size = div.getRights().Count;
		}
		for (int i = 0; i < size; i++)
		{
			if (smdata is StateMod_Diversion)
			{
				dright = div.getRight(i);
				onoff = dright.getSwitch();
				decree = dright.getDcrdiv();
			}
			if (onoff == 0)
			{
				// Not on...
				continue;
			}
			else if (onoff != 0)
			{
				// Turn on for the full period...
				if (onoff == 1)
				{
					fill_date1 = date1;
					fill_date2 = date2;
				}
				else if (onoff > 1)
				{
					// Turn on starting in the given year...
					fill_date1 = new DateTime();
					fill_date1.setYear(onoff);
					fill_date2 = date2;
				}
				else if (onoff < 0)
				{
					// On at beginning but off in a given year...
					fill_date1 = date1;
					fill_date2 = new DateTime();
					fill_date2.setYear(onoff);
					// TODO SAM 2007-03-01 Evaluate logic decree = (decree);
				}
			}
			if (interval == TimeInterval.MONTH)
			{
				for (DateTime date = new DateTime(fill_date1); date.lessThanOrEqualTo(fill_date2); date.addInterval(TimeInterval.MONTH,1))
				{
					// Monthlies are always ACFT so convert from CFS...
					// (ACFT/daysInMonth)(43560FT2/AC)(1day/86400s).
					ts.setDataValue(date, ts.getDataValue(date) + decree * (1.9835 * TimeUtil.numDaysInMonth(date)));
				}
			}
			else if (interval == TimeInterval.DAY)
			{
				for (DateTime date = new DateTime(fill_date1); date.lessThanOrEqualTo(fill_date2); date.addInterval(TimeInterval.DAY,1))
				{
					// Dailies are always CFS...
					ts.setDataValue(date, ts.getDataValue(date) + decree);
				}
			}
		}
		return ts;
	}

	/// <summary>
	/// Create a list of time series from a list of water rights.  A non-null list is guaranteed. </summary>
	/// <param name="smrights"> A list of StateMod_Right. </param>
	/// <param name="interval_base"> Time series interval for returned time series, either
	/// TimeInterval.DAY, TimeInterval.MONTH, TimeInterval.YEAR, or TimeInterval.IRREGULAR
	/// (smaller interval is slower). </param>
	/// <param name="spatial_aggregation"> If 0, create a time series for the location,
	/// which is the sum of the water rights at that location.  If 1, time series will
	/// be created by parcel (requires parcel information in right - only for well rights).
	/// If 2, individual time series will be created (essentially step functions with one step). </param>
	/// <param name="parcel_year"> If spatial_aggregation = 1, include the year to specify the years for parcel identifiers. </param>
	/// <param name="include_dataset_totals"> If true, create a time series including a total of all time series. </param>
	/// <param name="start"> Start DateTime for the time series.  If not specified, the date
	/// corresponding to the first right for a location will be used. </param>
	/// <param name="end"> End DateTime for the time series.  If not specified, the date
	/// corresponding to the last right for a location will be used. </param>
	/// <param name="FreeWaterAdministrationNumber_double"> A value >= to this is considered a free
	/// water right and will be handled as per FreeWaterMethod.  Specify a number larger
	/// than 99999.99999 to avoid adjusting for free water rights. </param>
	/// <param name="FreeWaterMethod"> If null, handle the right as any other right, generally
	/// meaning that the decree in the time series will take effect in the future.
	/// If UseSeniorRightApropriationDate, use the appropriation date of the senior right for the location.
	/// If AlwaysOn, use the earliest available date specified by
	/// "start" or that of data for the free water right appropriation date. </param>
	/// <param name="FreeWaterAppropriationDate_DateTime"> A DateTime that is used for the appropriation
	/// date if free water and FreeWaterMethod=UseSpecifiedDate. </param>
	/// <param name="process_data"> If true, process the time series data.  If false, only
	/// create the time series header information. </param>
	/// <returns> a list of time series created from a list of water rights. </returns>
	/// <exception cref="Exception"> if there is an error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.List<RTi.TS.TS> createWaterRightTimeSeriesList(java.util.List<? extends StateMod_Right> smrights, int interval_base, int spatial_aggregation, int parcel_year, boolean include_dataset_totals, RTi.Util.Time.DateTime OutputStart_DateTime, RTi.Util.Time.DateTime OutputEnd_DateTime, double FreeWaterAdministrationNumber_double, String FreeWaterMethod, RTi.Util.Time.DateTime FreeWaterAppropriationDate_DateTime, boolean process_data) throws Exception
	public static IList<TS> createWaterRightTimeSeriesList<T1>(IList<T1> smrights, int interval_base, int spatial_aggregation, int parcel_year, bool include_dataset_totals, DateTime OutputStart_DateTime, DateTime OutputEnd_DateTime, double FreeWaterAdministrationNumber_double, string FreeWaterMethod, DateTime FreeWaterAppropriationDate_DateTime, bool process_data) where T1 : StateMod_Right
	{
		string routine = "StateMod_Util.createWaterRightTimeSeriesList";
		Message.printStatus(2, routine, "Creating time series of water rights for requested period " + OutputStart_DateTime + " to " + OutputEnd_DateTime);
		int size = 0;
		// Spatial aggregation values
		int BYLOC = 0; // Time series for location (default)
		int BYPARCEL = 1; // Time series for parcel
		int BYRIGHT = 2; // Time series for right (one point)
		// Free water methods...
		int AlwaysOn_int = 0;
		int UseSeniorRightAppropriationDate_int = 1;
		int AsSpecified_int = 1;
		int FreeWaterMethod_int = AsSpecified_int;
		if (string.ReferenceEquals(FreeWaterMethod, null))
		{
			FreeWaterMethod_int = AsSpecified_int;
		}
		else if (FreeWaterMethod.Equals(AlwaysOn, StringComparison.OrdinalIgnoreCase))
		{
			FreeWaterMethod_int = AlwaysOn_int;
		}
		else if (FreeWaterMethod.Equals(UseSeniorRightAppropriationDate, StringComparison.OrdinalIgnoreCase))
		{
			FreeWaterMethod_int = UseSeniorRightAppropriationDate_int;
		}

		IList<TS> tslist = new List<TS>();
		TS ts = null; // Time series to add.
		StateMod_Right smright;
		StateMod_WellRight smwellright; // Only for parcel processing.
		bool need_to_create_ts; // Indicate whether new TS needed
		string tsid = null; // Time series identifier
		string id = null; // ID part of tsid
		int pos = 0; // Position of time series in list
		string adminnum_String = null;
		double adminnum_double;
		StateMod_AdministrationNumber adminnum = null;
							// Administration number corresponding to date for right.
		DateTime decree_DateTime = null; // Right appropriation date, to day.
		// Get the locations that have water rights.
		IList<string> loc_Vector = null;
		if (spatial_aggregation == BYPARCEL)
		{
			loc_Vector = getWaterRightParcelList((IList<StateMod_Right>)smrights, parcel_year);
		}
		else
		{
			// Process by location or individual rights...
			loc_Vector = getWaterRightLocationList(smrights, parcel_year);
		}
		int loc_size = 0;
		if (loc_Vector != null)
		{
			loc_size = loc_Vector.Count;
		}
		int smrights_size = 0;
		if (smrights != null)
		{
			smrights_size = smrights.Count;
		}
		if (spatial_aggregation == BYLOC)
		{
			Message.printStatus(2, routine, "Found " + loc_size + " locations from " + smrights_size + " rights.");
		}
		else if (spatial_aggregation == BYPARCEL)
		{
			Message.printStatus(2, routine, "Found " + loc_size + " parcels from " + smrights_size + " rights.");
		}
		else
		{
			Message.printStatus(2, routine, "Found " + loc_size + " rights from " + smrights_size + " rights.");
		}
		string loc_id = null; // Identifier for a location or parcel
		IList<StateMod_Right> loc_rights = null;
		DateTime min_DateTime = null;
		DateTime max_DateTime = null;
		double decree = 0; // Decree for water right
		string datatype = "WaterRight"; // Default data type, reset below
		string nodetype = ""; // Node type, for description, etc.
		int status = 0; // Used for error handling
		int onoff; // On/off switch for the right
		int free_right_count; // count of free water rights at location
		// Process the list of locations.
		for (int iloc = 0; iloc < loc_size; iloc++)
		{
			loc_id = loc_Vector[iloc];
			Message.printStatus(2, routine, "Processing location \"" + loc_id + "\"");
			if (spatial_aggregation == BYPARCEL)
			{
				loc_rights = getWaterRightsForParcel(smrights, loc_id, parcel_year);
			}
			else
			{
				// Process by location or individual rights...
				loc_rights = getWaterRightsForLocation(smrights, loc_id, parcel_year);
			}
			size = 0;
			if (loc_rights != null)
			{
				size = loc_rights.Count;
			}
			// If processing for the location or parcel, set the period of the time
			// series data to the bounding limits of the dates.
			min_DateTime = null; // Initialize
			max_DateTime = null;
			free_right_count = 0;
			if ((spatial_aggregation == BYLOC) || (spatial_aggregation == BYPARCEL))
			{
				for (int i = 0; i < size; i++)
				{
					smright = (StateMod_Right)loc_rights[i];
					if (smright == null)
					{
						continue;
					}
					adminnum_String = smright.getAdministrationNumber();
					adminnum_double = StringUtil.atod(adminnum_String);
					adminnum = new StateMod_AdministrationNumber(adminnum_double);
					decree_DateTime = new DateTime(adminnum.getAppropriationDate());
					if ((min_DateTime == null) || decree_DateTime.lessThan(min_DateTime))
					{
						min_DateTime = decree_DateTime;
					}
					if ((max_DateTime == null) || decree_DateTime.greaterThan(max_DateTime))
					{
						max_DateTime = decree_DateTime;
					}
					// Check whether a free water right...
					if ((adminnum_double >= FreeWaterAdministrationNumber_double))
					{
						++free_right_count;
					}
				}
			}
			// Now process each right for the location...
			for (int i = 0; i < size; i++)
			{
				smright = (StateMod_Right)loc_rights[i];
				if (smright == null)
				{
					continue;
				}
				decree = smright.getDecree();
				onoff = smright.getSwitch();
				// Get the appropriation date from the admin number.
				adminnum_String = smright.getAdministrationNumber();
				adminnum_double = StringUtil.atod(adminnum_String);
				if ((adminnum_double >= FreeWaterAdministrationNumber_double) && (!string.ReferenceEquals(FreeWaterMethod, null)))
				{
					// The right is a free water right.  Adjust if requested
					if (FreeWaterMethod_int == AlwaysOn_int)
					{
						// Set to earliest of starting date and most senior
						if ((spatial_aggregation == BYRIGHT) || (free_right_count == size))
						{
							// Minimum date will not have been determined.
							decree_DateTime = OutputStart_DateTime;
						}
						else
						{
							// have a valid minimum
							decree_DateTime = min_DateTime;
							if (OutputStart_DateTime.lessThan(decree_DateTime))
							{
								decree_DateTime = OutputStart_DateTime;
							}
						}
					}
					else if (FreeWaterMethod_int == UseSeniorRightAppropriationDate_int)
					{
						if (min_DateTime != null)
						{
							decree_DateTime = min_DateTime;
						}
						else
						{
							decree_DateTime = FreeWaterAppropriationDate_DateTime;
						}
					}
				}
				else
				{
					// Process the admin number to get the decree date...
					adminnum = new StateMod_AdministrationNumber(adminnum_double);
					decree_DateTime = new DateTime(adminnum.getAppropriationDate());
				}
				// TODO SAM 2007-05-16 Can optimize by saving instances above in memory
				// so they don't need to be recreated.
				need_to_create_ts = false;
				if (spatial_aggregation == BYLOC)
				{
					// Search for the location in the time series list.
					// If found, add to the time series.  Otherwise, create a new time series.
					pos = TSUtil.IndexOf(tslist, smright.getLocationIdentifier(), "Location", 1);
					if (pos >= 0)
					{
						// Will add to the matched right
						ts = (TS)tslist[pos];
					}
					else
					{
						// Need to create a new total right.
						id = smright.getLocationIdentifier();
						need_to_create_ts = true;
					}
				}
				else if (spatial_aggregation == BYPARCEL)
				{
					// Search for the location in the time series list.
					// If found, add to the time series.  Otherwise, create a new time series.
					smwellright = (StateMod_WellRight)smright;
					pos = TSUtil.IndexOf(tslist, smwellright.getParcelID(), "Location", 1);
					if (pos >= 0)
					{
						// Will add to the matched right
						ts = (TS)tslist[pos];
					}
					else
					{
						// Need to create a new total right.
						id = smwellright.getParcelID();
						need_to_create_ts = true;
					}
				}
				else
				{
					// Create an individual time series for each right.
					need_to_create_ts = true;
					id = smright.getLocationIdentifier() + "-" + smright.getIdentifier();
				}
				// Create the time series (either first right for a location or
				// time series are being created for each right).
				if (need_to_create_ts)
				{
					if (smright is StateMod_DiversionRight)
					{
						datatype = "DiversionWaterRight";
						nodetype = "Diversion";
					}
					else if (smright is StateMod_InstreamFlowRight)
					{
						datatype = "InstreamFlowWaterRight";
						nodetype = "InstreamFlow";
					}
					else if (smright is StateMod_ReservoirRight)
					{
						datatype = "ReservoirWaterRight";
						nodetype = "Reservoir";
					}
					else if (smright is StateMod_WellRight)
					{
						datatype = "WellWaterRight";
						nodetype = "Well";
					}
					if (spatial_aggregation == BYLOC)
					{
						// Append to the datatype
						datatype += "sTotal";
					}
					else if (spatial_aggregation == BYPARCEL)
					{
						// Append to the datatype
						datatype += "sParcelTotal";
					}
					if (interval_base == TimeInterval.DAY)
					{
						tsid = id + ".StateMod." + datatype + ".Day";
					}
					else if (interval_base == TimeInterval.MONTH)
					{
						tsid = id + ".StateMod." + datatype + ".Month";
					}
					else if (interval_base == TimeInterval.YEAR)
					{
						tsid = id + ".StateMod." + datatype + ".Year";
					}
					else if (interval_base == TimeInterval.IRREGULAR)
					{
						tsid = id + ".StateMod." + datatype + ".Irregular";
					}
					ts = TSUtil.newTimeSeries(tsid, true);
					ts.setIdentifier(tsid);
					if (spatial_aggregation == BYLOC)
					{
						ts.setDescription(smright.getLocationIdentifier() + " Total " + nodetype + " Rights for Location");
					}
					else if (spatial_aggregation == BYPARCEL)
					{
						if (smright is StateMod_WellRight)
						{
							ts.setDescription(((StateMod_WellRight)smright).getParcelID() + " Total " + nodetype + " Rights for Parcel");
						}
						else
						{
							ts.setDescription(smright.getLocationIdentifier() + " Total " + nodetype + " Rights for Parcel");
						}
					}
					else
					{
						// Individual rights
						ts.setDescription(smright.getName());
					}
					ts.setDataUnits(smright.getDecreeUnits());
					// Set the dates for the time series. If a single right is being used, use the specific date.
					// Otherwise, use the extent of the dates found for all rights.  If
					// a period has been specified, use that. Set the original dates to that from the data...
					if ((spatial_aggregation == BYLOC) || (spatial_aggregation == BYPARCEL))
					{
						ts.setDate1Original(min_DateTime);
						ts.setDate2Original(max_DateTime);
					}
					else
					{
						ts.setDate1Original(decree_DateTime);
						ts.setDate2Original(decree_DateTime);
					}
					// Set the active dates to that requested or found...
					if ((OutputStart_DateTime != null) && (OutputEnd_DateTime != null))
					{
						ts.setDate1(OutputStart_DateTime);
						ts.setDate2(OutputEnd_DateTime);
						Message.printStatus(2, routine, "Setting right time series period to requested " + ts.getDate1() + " to " + ts.getDate2());
					}
					else
					{
						if ((spatial_aggregation == BYLOC) || (spatial_aggregation == BYPARCEL))
						{
							ts.setDate1(min_DateTime);
							ts.setDate2(max_DateTime);
						}
						else
						{
							ts.setDate1(decree_DateTime);
							ts.setDate2(decree_DateTime);
						}
						Message.printStatus(2, routine, "Setting right time series period to data limit " + ts.getDate1() + " to " + ts.getDate2());
					}
					// Initialize to zero...
					if (process_data)
					{
						if (interval_base == TimeInterval.DAY)
						{
							status = ((DayTS)ts).allocateDataSpace(0.0);
						}
						else if (interval_base == TimeInterval.MONTH)
						{
							status = ((MonthTS)ts).allocateDataSpace(0.0);
						}
						else if (interval_base == TimeInterval.YEAR)
						{
							status = ((YearTS)ts).allocateDataSpace(0.0);
						}
						if (status != 0)
						{
							// Don't add the time series
							continue;
						}
					}
					// No need to allocate space for irregular.  Add the time series to the list...
					tslist.Add(ts);
				}
				// Now add to the right.  If daily, add to each time step.
				if (process_data)
				{
					// Set data in the time series.
					if (onoff == 0)
					{
						// Do not process.
						continue;
					}
					else if (onoff == 1)
					{
						// Right is always on.  Fall through to processing below.
					}
					else if (onoff > 1)
					{
						// On/off is a year.
						// Only turn on the right for the indicated year.  Reset the year of the right to the on/off but
						// only if it is later than the year from the admin number.
						if (onoff > decree_DateTime.getYear())
						{
							Message.printStatus(2,"", "Resetting decree year from " + decree_DateTime.getYear() + " to on/off " + onoff);
							decree_DateTime.setYear(onoff);
						}
					}
					else if (onoff < 0)
					{
						// Not yet handled - not expected to occur with well rights.
						// TODO SAM 2007-06-08 Evaluate how to handle negative switch - skip
						Message.printStatus(2, routine, "Software not able to handle negative on/off well right switch.  Skipping right.");
						continue;
					}
					if ((interval_base == TimeInterval.DAY) || (interval_base == TimeInterval.MONTH) || (interval_base == TimeInterval.YEAR))
					{
						if ((spatial_aggregation == BYLOC) || (spatial_aggregation == BYPARCEL))
						{
							if (decree > 0.0)
							{
								Message.printStatus(2,"", "Adding constant decree " + decree + " starting in " + decree_DateTime + " to " + ts.getDate2());
								TSUtil.addConstant(ts, decree_DateTime, ts.getDate2(), -1, decree, TSUtil.IGNORE_MISSING);
							}
						}
						else
						{
							// Set the single value since one point in time...
							ts.setDataValue(decree_DateTime, decree);
						}
					}
					else
					{
						// Irregular...
						ts.setDataValue(decree_DateTime, decree);
					}
				}
			}
		}
		size = tslist.Count;
		if (include_dataset_totals && (size > 0))
		{
			// Include one time series that is the sum of all other time series.
			if (interval_base == TimeInterval.DAY)
			{
				tsid = "DataSet.StateMod." + datatype + ".Day";
			}
			else if (interval_base == TimeInterval.MONTH)
			{
				tsid = "DataSet.StateMod." + datatype + ".Month";
			}
			else if (interval_base == TimeInterval.YEAR)
			{
				tsid = "DataSet.StateMod." + datatype + ".Year";
			}
			TS totalts = TSUtil.newTimeSeries(tsid, true);
			totalts.setIdentifier(tsid);

			TSLimits limits = TSUtil.getPeriodFromTS(tslist, TSUtil.MAX_POR);
			totalts.setDate1(limits.getDate1());
			totalts.setDate1Original(limits.getDate1());
			totalts.setDate2(limits.getDate2());
			totalts.setDate2Original(limits.getDate2());
			Message.printStatus(2, routine, "Date limits for total time series are " + limits.getDate1() + " to " + limits.getDate2());

			totalts.allocateDataSpace();
			bool units_set = false;
			DateTime date = null;
			double value;
			for (int i = 0; i < size; i++)
			{
				ts = (TS)tslist[i];
				if (!units_set && (ts.getDataUnits().length() > 0))
				{
					totalts.setDataUnits(ts.getDataUnits());
					totalts.setDataUnitsOriginal(ts.getDataUnits());
					units_set = true;
				}
				// The time series have different periods but want
				// the last value to be continued to the end of the
				// period.  Therefore, first add each time series, and then add
				// the last value from one interval past the end date to the
				// of the time series to the end of the total time series.
				Message.printStatus(2, routine, "Add " + ts.getLocation() + " " + ts.getDate1() + " to " + ts.getDate2());
				TSUtil.add(totalts, ts);
				date = new DateTime(ts.getDate2());
				// Should be non-missing...
				value = ts.getDataValue(date);
				if (!ts.isDataMissing(value))
				{
					// Add constant at the end of the time series.
					date.addInterval(ts.getDataIntervalBase(), ts.getDataIntervalMult());
					TSUtil.addConstant(totalts, date, totalts.getDate2(), -1, value, TSUtil.IGNORE_MISSING);
					Message.printStatus(2, routine, "Add constant " + value + " " + ts.getLocation() + " " + date + " to " + totalts.getDate2());
				}
			}
			totalts.setDescription("Total " + nodetype + " water right time series.");
			tslist.Add(totalts);
		}
		return tslist;
	}

	/// <summary>
	/// Creates a new ID given an ID used for rights.  If the previous right ID was
	/// 12345.05, 12345.06 will be returned.  This also works for alphanumeric IDs
	/// (ABC_003.05 becomes ABC_003.06).  If the old ID doesn't contain a '.', the same old ID is returned. </summary>
	/// <returns> the new ID. </returns>
	public static string createNewID(string oldID)
	{
		string routine = "StateMod_Util.createNewID";
		// create new id
		int dotIndex = oldID.IndexOf(".", StringComparison.Ordinal);
		if (dotIndex == -1)
		{
			return oldID + ".01";
		}

		string front = oldID.Substring(0, dotIndex);

		string back = oldID.Substring(dotIndex + 1);

		int? D = null;
		try
		{
			D = Convert.ToInt32(back);
		}
		catch (System.FormatException)
		{
			Message.printWarning(1, routine, "Could not create new ID " + "because old ID does not have a number after the decimal: '" + back + "'");
			return oldID;
		}
		int d = D.Value;
		d += 1;
		int? DD = new int?(d);

		string newBack = DD.ToString();

		string zero = "";
		if (d < 10)
		{
			zero = "0";
		}
		string newid = front + "." + zero + newBack;

		return newid;
	}

	/// <summary>
	/// Calculates the earliest date in the period of record for the time series in the
	/// dataset.  Note that this is always a calendar date.  The time series that are checked are:<br>
	/// <ol>
	/// <li>StateMod_DataSet.COMP_STREAMGAGE_BASEFLOW_TS_MONTHLY</li>
	/// <li>StateMod_DataSet.COMP_INSTREAM_DEMAND_TS_AVERAGE_MONTHLY</li>
	/// <li>StateMod_DataSet.COMP_INSTREAM_DEMAND_TS_MONTHLY</li>
	/// <li>StateMod_DataSet.COMP_DEMAND_TS_MONTHLY</li>
	/// <li>StateMod_DataSet.COMP_DIVERSION_TS_MONTHLY</li>
	/// <li>StateMod_DataSet.COMP_RESERVOIR_CONTENT_TS_MONTHLY</li>
	/// <li>StateMod_DataSet.COMP_RESERVOIR_TARGET_TS_MONTHLY</li>
	/// <li>StateMod_DataSet.COMP_STREAMGAGE_HISTORICAL_TS_MONTHLY</li>
	/// </ol> </summary>
	/// <param name="dataset"> the dataset in which to check for the earliest POR </param>
	/// <returns> a DateTime object with the earliest POR, or null if the earliest date cannot be found. </returns>
	public static DateTime findEarliestDateInPOR(StateMod_DataSet dataset)
	{
		DateTime newDate = null;
		IList<TS> tsVector = null;
		DateTime tempDate = null;

		int numFiles = 8;
		int[] files = new int[numFiles];
		files[0] = StateMod_DataSet.COMP_STREAMGAGE_NATURAL_FLOW_TS_MONTHLY;
		files[1] = StateMod_DataSet.COMP_INSTREAM_DEMAND_TS_AVERAGE_MONTHLY;
		files[2] = StateMod_DataSet.COMP_INSTREAM_DEMAND_TS_MONTHLY;
		files[3] = StateMod_DataSet.COMP_DEMAND_TS_MONTHLY;
		files[4] = StateMod_DataSet.COMP_DIVERSION_TS_MONTHLY;
		files[5] = StateMod_DataSet.COMP_RESERVOIR_CONTENT_TS_MONTHLY;
		files[6] = StateMod_DataSet.COMP_RESERVOIR_TARGET_TS_MONTHLY;
		files[7] = StateMod_DataSet.COMP_STREAMGAGE_HISTORICAL_TS_MONTHLY;

		DateTime seedDate = new DateTime();
		seedDate.setYear(3000);

		for (int i = 0; i < numFiles; i++)
		{
			if (dataset.getComponentForComponentType(files[i]).hasData())
			{
				tsVector = (System.Collections.IList)((dataset.getComponentForComponentType(files[i])).getData());
				if (newDate == null)
				{
					tempDate = findEarliestDateInPORHelper(tsVector,seedDate);
				}
				else
				{
					tempDate = findEarliestDateInPORHelper(tsVector,newDate);
				}
				if (tempDate != null)
				{
					newDate = tempDate;
				}
			}
		}

		return newDate;
	}

	/// <summary>
	/// A private helper method for <b>findEarliestDateInPOR()</b> for finding the
	/// earliest date in the period.  Because the vector of time series is assumed to
	/// have been read from a single file, just check the first time series with a
	/// non-zero date (no need to check all time series after that). </summary>
	/// <param name="tsVector"> a list of time series. </param>
	/// <param name="newDate"> the data against which to check the first date in the time series </param>
	/// <returns> first date of the time series or null if the first date of the time
	/// series is not earlier than newDate </returns>
	private static DateTime findEarliestDateInPORHelper(IList<TS> tsVector, DateTime newDate)
	{
		DateTime date = null;
		if (tsVector.Count > 0)
		{
			date = (tsVector[0]).getDate1();
			if (date.getYear() > 0 && date.lessThan(newDate))
			{
				return date;
			}
		}
		return null;
	}

	/// <summary>
	/// Calculates the latest date in the period of record for the time series in the
	/// dataset.  Note that this is always a calendar date.  The time series that are checked are:<br>
	/// <ol>
	/// <li>StateMod_DataSet.COMP_STREAMGAGE_BASEFLOW_TS_MONTHLY</li>
	/// <li>StateMod_DataSet.COMP_INSTREAM_DEMAND_TS_AVERAGE_MONTHLY</li>
	/// <li>StateMod_DataSet.COMP_INSTREAM_DEMAND_TS_MONTHLY</li>
	/// <li>StateMod_DataSet.COMP_DEMAND_TS_MONTHLY</li>
	/// <li>StateMod_DataSet.COMP_DIVERSION_TS_MONTHLY</li>
	/// <li>StateMod_DataSet.COMP_RESERVOIR_CONTENT_TS_MONTHLY</li>
	/// <li>StateMod_DataSet.COMP_RESERVOIR_TARGET_TS_MONTHLY</li>
	/// <li>StateMod_DataSet.COMP_STREAMGAGE_HISTORICAL_TS_MONTHLY</li>
	/// </ol> </summary>
	/// <param name="dataset"> the dataset in which to check for the latest POR </param>
	/// <returns> a DateTime object with the latest date in the period, or null if no data are available. </returns>
	public static DateTime findLatestDateInPOR(StateMod_DataSet dataset)
	{
		DateTime newDate = null;
		DateTime tempDate = null;

		int numFiles = 8;
		int[] files = new int[numFiles];
		files[0] = StateMod_DataSet.COMP_STREAMGAGE_NATURAL_FLOW_TS_MONTHLY;
		files[1] = StateMod_DataSet.COMP_INSTREAM_DEMAND_TS_AVERAGE_MONTHLY;
		files[2] = StateMod_DataSet.COMP_INSTREAM_DEMAND_TS_MONTHLY;
		files[3] = StateMod_DataSet.COMP_DEMAND_TS_MONTHLY;
		files[4] = StateMod_DataSet.COMP_DIVERSION_TS_MONTHLY;
		files[5] = StateMod_DataSet.COMP_RESERVOIR_CONTENT_TS_MONTHLY;
		files[6] = StateMod_DataSet.COMP_RESERVOIR_TARGET_TS_MONTHLY;
		files[7] = StateMod_DataSet.COMP_STREAMGAGE_HISTORICAL_TS_MONTHLY;

		DateTime seedDate = new DateTime();
		seedDate.setYear(-3000);

		for (int i = 0; i < numFiles; i++)
		{
			if (dataset.getComponentForComponentType(files[i]).hasData())
			{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<RTi.TS.TS> tslist = (java.util.List<RTi.TS.TS>)((dataset.getComponentForComponentType(files[i])).getData());
				IList<TS> tslist = (IList<TS>)((dataset.getComponentForComponentType(files[i])).getData());
				if (newDate == null)
				{
					tempDate = findLatestDateInPORHelper(tslist,seedDate);
				}
				else
				{
					tempDate = findLatestDateInPORHelper(tslist,newDate);
				}
				if (tempDate != null)
				{
					newDate = tempDate;
				}
			}
		}

		return newDate;
	}

	/// <summary>
	/// A private helper method for <b>findLatestDateInPOR()</b> for finding the latest
	/// POR.  Because the list of time series is assumed to have been read from a
	/// single file, just check the first time series with a non-zero date (no need
	/// to check all time series after that). </summary>
	/// <param name="tslist"> a list of time series. </param>
	/// <param name="newDate"> the data against which to check the last date in the time series </param>
	/// <returns> last date of the time series or null if the last date of the time series is not later than newDate </returns>
	private static DateTime findLatestDateInPORHelper(IList<TS> tslist, DateTime newDate)
	{
		DateTime date = null;
		if (tslist == null)
		{
			return null;
		}
		if (tslist.Count > 0)
		{
			date = tslist[0].getDate2();
			if (date.getYear() > 0 && date.greaterThan(newDate))
			{
				return date;
			}
		}
		return null;
	}

	// TODO SAM 2004-09-07 JTS needs to javadoc?
	public static string findNameInVector<T1>(string id, IList<T1> v, bool includeDash) where T1 : StateMod_Data
	{
		int size = v.Count;

		StateMod_Data data;
		for (int i = 0; i < size; i++)
		{
			data = v[i];
			if (data.getID().Equals(id))
			{
				if (includeDash)
				{
					return " - " + data.getName();
				}
				else
				{
					return data.getName();
				}
			}
		}
		return "";
	}

	/// <summary>
	/// Find the insert position for a new water right, in the full list of rights.
	/// The position that is returned can be used with list.insertElementAt(), for the
	/// full data array.  The position is determined by finding the an item in the
	/// data vector with the same "cgoto" value.  The insert then considers the value of
	/// "irtem" so that the result after the insert is water rights sorted by "irtem". 
	/// It is assumed that the water rights for the same "cgoto" are grouped together
	/// and are sorted by "irtem".  If no matching "cgoto" is found, the insert position
	/// will be according to cgoto order. </summary>
	/// <returns> the insert position for a new water right, in the full list of rights,
	/// or -1 if the right should be inserted at the end (no other option). </returns>
	/// <param name="data_Vector"> a list of StateMod_*Right, with data members populated. </param>
	/// <param name="item"> A single StateMod_*Right to insert. </param>
	public static int findWaterRightInsertPosition<T1>(IList<T1> data_Vector, StateMod_Data item) where T1 : StateMod_Data
	{
		if ((data_Vector == null) || (data_Vector.Count == 0))
		{
			// Add at the end...
			return -1;
		}
		int size = 0;
		if (data_Vector != null)
		{
			size = data_Vector.Count;
		}
		StateMod_Data data = null; // StateMod data object to evaluate
		for (int i = 0; i < size; i++)
		{
			data = data_Vector[i];
			if (data.getID().CompareTo(item.getID()) > 0)
			{
				// Vector item is greater than the new item
				// to insert so insert at this position...
				return i;
			}
		}
		// Add at the end...
		return -1;
		/* TODO - all of this seemed to be getting too complicated.  Just
		 inserting based on the right ID seems to be simplest
		int pos = locateIndexFromCGOTO ( item.getCgoto(), data_Vector );
		int size = 0;
		if ( data_Vector != null ) {
			size = data_Vector.size();
		}
		int sizem1 = size - 1;		// To check end of loop.
		StateMod_Data data = null;	// StateMod data object to evaluate
		if ( pos < 0 ) {
			/ * For now sort by right ID because CGOTO seems to sometimes
			   have a totally different spelling.
			// Unable to find the cgoto.  The water right to be inserted is
			// the first one for the structure.  Assume alphabetical and
			// find a right with the next cgoto and insert before it...
			for ( int i = 0; i < size; i++ ) {
				data = (StateMod_Data)data_Vector.elementAt(i);
				if (data.getCgoto().compareTo(item.getCgoto()) > 0 ) {
					// Vector item is greater than the new item
					// to insert so insert at this position...
					return i;
				}
			}
			* /
			// Unable to find the cgoto.  The water right to be inserted is
			// the first one for the structure.  Assume alphabetical right
			// identifiers and insert before accordingly...
			for ( int i = 0; i < size; i++ ) {
				data = (StateMod_Data)data_Vector.elementAt(i);
				if (data.getID().compareTo(item.getID()) > 0 ) {
					// Vector item is greater than the new item
					// to insert so insert at this position...
					return i;
				}
			}
			// Add at the end...
			return -1;
		}
		// Loop through rights with the same "cgoto" until the item.irtem is
		// greater than a value in the list (in which case the insert position
		// is the last Vector item processed)...
		String irtem = null;
		if ( item instanceof StateMod_DiversionRight ) {
			irtem = ((StateMod_DiversionRight)item).getIrtem();
		}
		else if ( item instanceof StateMod_ReservoirRight ) {
			irtem = ((StateMod_ReservoirRight)item).getRtem();
		}
		else if ( item instanceof StateMod_InstreamFlowRight ) {
			irtem = ((StateMod_InstreamFlowRight)item).getIrtem();
		}
		else if ( item instanceof StateMod_WellRight ) {
			irtem = ((StateMod_WellRight)item).getIrtem();
		}
		double irtem_double = StringUtil.atod ( irtem );
						// The irtem from the specific right
		String irtem_data = null;	// The irtem from a data object, as a
						// string
		double irtem_data_double = 0.0;	// The irtem from a data object, as a
						// double.
		data = (StateMod_Data)data_Vector.elementAt(pos);
		for ( int i = pos; i < size; ) {
			// Get the double value in the vector...
			if ( data instanceof StateMod_DiversionRight ) {
				irtem_data = ((StateMod_DiversionRight)data).getIrtem();
			}
			else if ( data instanceof StateMod_ReservoirRight ) {
				irtem_data = ((StateMod_ReservoirRight)data).getRtem();
			}
			else if ( data instanceof StateMod_InstreamFlowRight ) {
				irtem_data =
					((StateMod_InstreamFlowRight)data).getIrtem();
			}
			else if ( data instanceof StateMod_WellRight ) {
				irtem_data = ((StateMod_WellRight)data).getIrtem();
			}
			// Compare the double values...
			irtem_data_double = StringUtil.atod ( irtem_data );
			Message.printStatus ( 2, "",
			"Checking " + item.getCgoto() + " " + irtem_double +
				" against vector " + data.getCgoto() + " " +
				irtem_data_double );
			if ( irtem_data_double > irtem_double ) {
				// Have gone past the "irtem" and need to insert before
				// For debugging...
				Message.printStatus ( 2, "", "returning " + i );
				// the current item...
				return i;
			}
			if ( i != sizem1 ) {
				// Now check for overrun to the next structure...
				data = (StateMod_Data)data_Vector.elementAt(++i);
				if (!data.getCgoto().equalsIgnoreCase(item.getCgoto())){
					// This is a new cgoto so need to insert before
					// it...
					return i;
				}
			}
			else {	// Last iteration
				break;
			}
		}
		return -1;
		*/
	}

	/// <summary>
	/// Format a label to use for a data object.  The format will be "ID (Name)".  If
	/// the name is null or the same as the ID, then the format will be ID. </summary>
	/// <param name="id"> Data identifier. </param>
	/// <param name="name"> Data name. </param>
	/// <returns> formatted label for data. </returns>
	public static string formatDataLabel(string id, string name)
	{
		if (id.Equals(name, StringComparison.OrdinalIgnoreCase))
		{
			return id;
		}
		else if (id.Equals(""))
		{
			return name;
		}
		else if (name.Equals(""))
		{
			return id;
		}
		else
		{
			return id + " (" + name + ")";
		}
	}

	/// <summary>
	/// Works in coordination with isDailyTimeSeriesAvailable-make sure to keep in sync </summary>
	/// <returns> daily time series based on rules for getting daily TS
	/// dailyID = dayTS identifier, return daily ts
	/// dailyID = 0, return daily average based on month ts
	/// else return a calculated daily by getting ratio of daily to monthly 
	///		and multiplying to daily (daily is then a pattern) </returns>
	/// <param name="dailyID"> daily id to use in comparison as described above </param>
	/// <param name="calculate"> only used when daily ID != dayTS identifier;  
	/// if true, calculates time series using ratio of monthly to daily
	/// if false, returns original dayTS (pattern) </param>
	public static DayTS getDailyTimeSeries(string ID, string dailyID, MonthTS monthTS, DayTS dayTS, bool calculate)
	{
		string rtn = "StateMod_GUIUtil.getDailyTimeSeries";
		// if dailyID is 0
		if (dailyID.Equals("0", StringComparison.OrdinalIgnoreCase))
		{
			if (monthTS == null)
			{
				Message.printWarning(2, "StateMod_GUIUtil.getDailyTimeSeries", "Monthly time series is null.  Unable to calculate daily.");
				return null;
			}
			string monthTSUnits = monthTS.getDataUnits();
			if (!(monthTSUnits.Equals("ACFT", StringComparison.OrdinalIgnoreCase) || monthTSUnits.Equals("AF/M", StringComparison.OrdinalIgnoreCase)))
			{
				Message.printWarning(2, "StateMod_GUIUtil.getDailyTimeSeries", "Monthly time series units \"" + monthTSUnits + "\" not \"ACFT\".  Unable to process daily ts.");
				return null;
			}

			double convertAFtoCFS = 43560.0 / (3600.0 * 24.0);

			DayTS newDayTS = new DayTS();

			newDayTS.copyHeader(monthTS);
			newDayTS.setDataUnits("CFS");
			DateTime date1 = monthTS.getDate1();
			date1.setDay(1);
			DateTime date2 = monthTS.getDate2();
			date2.setDay(TimeUtil.numDaysInMonth(date2));

			newDayTS.setDate1(date1);
			newDayTS.setDate2(date2);
			newDayTS.allocateDataSpace();

			int numDaysInMonth;
			double avgValue;
			DateTime ddate;
			for (DateTime date = new DateTime(date1); date.lessThanOrEqualTo(date2); date.addInterval(TimeInterval.MONTH, 1))
			{
				numDaysInMonth = TimeUtil.numDaysInMonth(date);
				avgValue = monthTS.getDataValue(date) * convertAFtoCFS / numDaysInMonth;

				ddate = new DateTime(date);
				for (int i = 0; i < numDaysInMonth; i++)
				{
					newDayTS.setDataValue(ddate, avgValue);
					ddate.addInterval(TimeInterval.DAY, 1);
				}
			}
			return newDayTS;
		}

		// if dailyID = ID identifier
		else if (dailyID.Equals(ID, StringComparison.OrdinalIgnoreCase))
		{
			if (dayTS == null)
			{
				Message.printWarning(1, "StateMod_GUIUtil.getDailyTimeSeries", "Daily time series is null.");
				return null;
			}
			return dayTS;
		}

		// if dailyID != ID and pattern is desired
		else if (!calculate)
		{
			if (dayTS == null)
			{
				Message.printWarning(1, "StateMod_GUIUtil.getDailyTimeSeries", "Daily time series pattern is null.");
				return null;
			}
			return dayTS;
		}
		// if dailyID != dayTS identifier and calculation is desired
		else
		{
			if (dayTS == null)
			{
				Message.printWarning(1, "StateMod_GUIUtil.getDailyTimeSeries", "Daily time series pattern is null.");
				return null;
			}
			if (monthTS == null)
			{
				Message.printWarning(1, "StateMod_GUIUtil.getDailyTimeSeries", "Monthly time series is null.");
				return null;
			}

			int numValuesInMonth;
			double sum, ratio, value;
			DateTime ddate, enddate;
			bool isFlow = true;

			DayTS newDayTS = new DayTS();
			newDayTS.copyHeader(dayTS);
			newDayTS.allocateDataSpace();

			double convertAFtoCFS = 43560.0 / (3600.0 * 24.0);
			string dayTSUnits = dayTS.getDataUnits();
			if (dayTSUnits.Equals("ACFT", StringComparison.OrdinalIgnoreCase) || dayTSUnits.Equals("AF/M", StringComparison.OrdinalIgnoreCase))
			{
				isFlow = false;
			}
			else if (dayTSUnits.Equals("CFS", StringComparison.OrdinalIgnoreCase))
			{
				isFlow = true;
			}
			else
			{
				Message.printWarning(2, "StateMod_GUIUtil.getDailyTimeSeries", "Unable to process dayTS due to units \"" + dayTSUnits + "\"");
				return null;
			}

			if (Message.isDebugOn)
			{
				Message.printDebug(30, rtn, "convertAFtoCFS is " + convertAFtoCFS);
			}

			// check for units we can handle

			Message.printStatus(1, rtn, "Looking through dates " + dayTS.getDate1() + " " + dayTS.getDate2());
			for (DateTime date = new DateTime(dayTS.getDate1()); date.lessThanOrEqualTo(dayTS.getDate2()); date.addInterval(TimeInterval.MONTH, 1))
			{
				try
				{
					enddate = new DateTime(date);
					enddate.addInterval(TimeInterval.MONTH, 1);

					numValuesInMonth = 0;
					sum = 0;
					for (ddate = new DateTime(date); ddate.lessThan(enddate); ddate.addInterval(TimeInterval.DAY, 1))
					{
						value = dayTS.getDataValue(ddate);
						if (!dayTS.isDataMissing(value))
						{
							numValuesInMonth++;
							// value is CFS, need AF/D
							sum += value;
						}
					}

					if (Message.isDebugOn)
					{
						Message.printDebug(30, rtn, "Sum for month " + date.getMonth() + " is " + sum);
						Message.printDebug(30, rtn, "monthTS data value is " + monthTS.getDataValue(date));
						Message.printDebug(30, rtn, "numDaysInMonth is " + TimeUtil.numDaysInMonth(date));
						Message.printDebug(30, rtn, "numValuesInMonth is " + numValuesInMonth);
					}

					// to accomodate for missing, create a ratio as follows
					//   monthly value / numDaysInMonth divided by sum / numValuesInMonth
					if (isFlow)
					{
						ratio = (monthTS.getDataValue(date) * convertAFtoCFS / TimeUtil.numDaysInMonth(date)) / (sum / numValuesInMonth);
					}
					else
					{
						ratio = monthTS.getDataValue(date) / (sum / numValuesInMonth);
					}
					if (Message.isDebugOn)
					{
						Message.printDebug(30, "StateMod_GUIUtil.getDailyTimeSeries", "Ratio is " + ratio);
					}

					for (ddate = new DateTime(date); ddate.lessThan(enddate); ddate.addInterval(TimeInterval.DAY, 1))
					{
						newDayTS.setDataValue(ddate, dayTS.getDataValue(ddate) * ratio);
					}
				}
				catch (Exception e)
				{
					Message.printWarning(2, rtn, e);
				}
			}
			return newDayTS;
		}
	}

	/// <summary>
	/// Get a list of data objects, as a subset of a full data component list. </summary>
	/// <param name="smdataList"> a list of StateMod_Data to search. </param>
	/// <param name="idToMatch"> the identifier to match (case-insensitive). </param>
	/// <returns> a list of objects matching the idToMatch.  A
	/// non-null list is guaranteed; however, the list may have zero items. </returns>
	public static IList<T> getDataList<T>(IList<T> smdataList, string idToMatch)
	{
		IList<T> v = new List<T>();
		if (smdataList == null)
		{
			return v;
		}

		foreach (T smdata in smdataList)
		{
			if (smdata == default(T))
			{
				continue;
			}
			else
			{
				StateMod_Data smdata2 = (StateMod_Data)smdata;
				if (smdata2.getID().Equals(idToMatch, StringComparison.OrdinalIgnoreCase))
				{
					v.Add(smdata);
				}
			}
		}
		return v;
	}

	// TODO SAM 2010-12-28 Implement generics - had some problems 
	/// <summary>
	/// Get the list of data objects matching the operational right metadata source/destination type.
	/// Depending on the type, this may return a list of stations or rights. </summary>
	/// <param name="type"> the type of source or destination that is allowed </param>
	/// <param name="dataset"> the full dataset, from which lists are extracted </param>
	/// <param name="idToMatch"> the identifier to match to limit the list (generally will limit the list to one return item) </param>
	/// <param name="returnStations"> if the list is a list of rights, then specifying true will return the list of stations
	/// associated with the rights (ignored in cases where the type is stations) </param>
	public static System.Collections.IList getDataList(StateMod_OperationalRight_Metadata_SourceOrDestinationType type, StateMod_DataSet dataset, string idToMatch, bool returnStations)
	{
		System.Collections.IList dataList = new List<object>();
		if (type == StateMod_OperationalRight_Metadata_SourceOrDestinationType.CARRIER)
		{
			// Diversions where irturn = 3
			IList<StateMod_Diversion> stationList2 = (IList<StateMod_Diversion>)dataset.getComponentForComponentType(StateMod_DataSet.COMP_DIVERSION_STATIONS).getData();
			foreach (StateMod_Diversion div in stationList2)
			{
				if (div.getIrturn() == 3)
				{
					if ((string.ReferenceEquals(idToMatch, null)) || idToMatch.Equals(div.getID(), StringComparison.OrdinalIgnoreCase))
					{
						dataList.Add(div);
					}
				}
			}
		}
		else if (type == StateMod_OperationalRight_Metadata_SourceOrDestinationType.DIVERSION)
		{
			// All diversion stations
			dataList = (IList<StateMod_Diversion>)dataset.getComponentForComponentType(StateMod_DataSet.COMP_DIVERSION_STATIONS).getData();
		}
		else if (type == StateMod_OperationalRight_Metadata_SourceOrDestinationType.DIVERSION_RIGHT)
		{
			// Diversions rights
			IList<StateMod_DiversionRight> dataList2 = (IList<StateMod_DiversionRight>)dataset.getComponentForComponentType(StateMod_DataSet.COMP_DIVERSION_RIGHTS).getData();
			foreach (StateMod_DiversionRight divRight in dataList2)
			{
				if (returnStations)
				{
					// Get the diversion station associated with the right
					System.Collections.IList divList = (IList<StateMod_Diversion>)dataset.getComponentForComponentType(StateMod_DataSet.COMP_DIVERSION_STATIONS).getData();
					int pos = indexOf(divList, divRight.getLocationIdentifier());
					if (pos >= 0)
					{
						if ((string.ReferenceEquals(idToMatch, null)) || idToMatch.Equals(divRight.getID(), StringComparison.OrdinalIgnoreCase))
						{
							dataList.Add(divList[pos]);
						}
					}
				}
				else
				{
					if ((string.ReferenceEquals(idToMatch, null)) || idToMatch.Equals(divRight.getID(), StringComparison.OrdinalIgnoreCase))
					{
						dataList.Add(divRight);
					}
				}
			}
		}
		else if (type == StateMod_OperationalRight_Metadata_SourceOrDestinationType.INSTREAM_FLOW)
		{
			// All instream flow stations
			dataList = (IList<StateMod_InstreamFlow>)dataset.getComponentForComponentType(StateMod_DataSet.COMP_INSTREAM_STATIONS).getData();
		}
		else if (type == StateMod_OperationalRight_Metadata_SourceOrDestinationType.INSTREAM_FLOW_RIGHT)
		{
			// InstreamFlows rights
			IList<StateMod_InstreamFlowRight> dataList2 = (IList<StateMod_InstreamFlowRight>)dataset.getComponentForComponentType(StateMod_DataSet.COMP_INSTREAM_RIGHTS).getData();
			foreach (StateMod_InstreamFlowRight instreamRight in dataList2)
			{
				if (returnStations)
				{
					// Get the instream flow station associated with the right
					System.Collections.IList instreamList = (IList<StateMod_InstreamFlow>)dataset.getComponentForComponentType(StateMod_DataSet.COMP_INSTREAM_STATIONS).getData();
					int pos = indexOf(instreamList, instreamRight.getLocationIdentifier());
					if (pos >= 0)
					{
						if ((string.ReferenceEquals(idToMatch, null)) || idToMatch.Equals(instreamRight.getID(), StringComparison.OrdinalIgnoreCase))
						{
							dataList.Add(instreamList[pos]);
						}
					}
				}
				else
				{
					if ((string.ReferenceEquals(idToMatch, null)) || idToMatch.Equals(instreamRight.getID(), StringComparison.OrdinalIgnoreCase))
					{
						dataList.Add(instreamRight);
					}
				}
			}
		}
		else if (type == StateMod_OperationalRight_Metadata_SourceOrDestinationType.OPERATIONAL_RIGHT)
		{
			// All operational rights
			dataList = (IList<StateMod_OperationalRight>)dataset.getComponentForComponentType(StateMod_DataSet.COMP_OPERATION_RIGHTS).getData();
		}
		else if (type == StateMod_OperationalRight_Metadata_SourceOrDestinationType.OTHER)
		{
			// Stations in the network that are not in any other list
			dataList = (IList<StateMod_RiverNetworkNode>)dataset.getComponentForComponentType(StateMod_DataSet.COMP_RIVER_NETWORK).getData();
			System.Collections.IList divList = (IList<StateMod_Diversion>)dataset.getComponentForComponentType(StateMod_DataSet.COMP_DIVERSION_STATIONS).getData();
			System.Collections.IList resList = (IList<StateMod_Reservoir>)dataset.getComponentForComponentType(StateMod_DataSet.COMP_RESERVOIR_STATIONS).getData();
			System.Collections.IList ifsList = (IList<StateMod_InstreamFlow>)dataset.getComponentForComponentType(StateMod_DataSet.COMP_INSTREAM_STATIONS).getData();
			System.Collections.IList wellList = (IList<StateMod_Well>)dataset.getComponentForComponentType(StateMod_DataSet.COMP_WELL_STATIONS).getData();
			System.Collections.IList gageList = (IList<StateMod_StreamGage>)dataset.getComponentForComponentType(StateMod_DataSet.COMP_STREAMGAGE_STATIONS).getData();
			System.Collections.IList estList = (IList<StateMod_StreamEstimate>)dataset.getComponentForComponentType(StateMod_DataSet.COMP_STREAMESTIMATE_STATIONS).getData();
			System.Collections.IList planList = (IList<StateMod_Plan>)dataset.getComponentForComponentType(StateMod_DataSet.COMP_PLANS).getData();
			IList<StateMod_RiverNetworkNode> returnList = new List<object>();
			foreach (StateMod_RiverNetworkNode node in (IList<StateMod_RiverNetworkNode>)dataList)
			{
				// Check the other lists
				if (StateMod_Util.IndexOf(divList, node.getID()) >= 0)
				{
					// Diversion so not other
					continue;
				}
				else if (StateMod_Util.IndexOf(resList, node.getID()) >= 0)
				{
					// Reservoir so not other
					continue;
				}
				else if (StateMod_Util.IndexOf(ifsList, node.getID()) >= 0)
				{
					// Instream flow so not other
					continue;
				}
				else if (StateMod_Util.IndexOf(wellList, node.getID()) >= 0)
				{
					// Well so not other
					continue;
				}
				else if (StateMod_Util.IndexOf(gageList, node.getID()) >= 0)
				{
					// Stream gage so not other
					continue;
				}
				else if (StateMod_Util.IndexOf(estList, node.getID()) >= 0)
				{
					// Stream estimate so not other
					continue;
				}
				else if (StateMod_Util.IndexOf(planList, node.getID()) >= 0)
				{
					// Plan so not other
					continue;
				}
				else
				{
					// Did not match any stations so must be other
					returnList.Add(node);
				}
			}
			return returnList;
		}
		else if (type == StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_ACCOUNTING)
		{
			// Plan where iPlnTyp = 11
			IList<StateMod_Plan> dataList2 = (IList<StateMod_Plan>)dataset.getComponentForComponentType(StateMod_DataSet.COMP_PLANS).getData();
			foreach (StateMod_Plan plan in dataList2)
			{
				if (plan.getIPlnTyp() == 11)
				{
					if ((string.ReferenceEquals(idToMatch, null)) || idToMatch.Equals(plan.getID(), StringComparison.OrdinalIgnoreCase))
					{
						dataList.Add(plan);
					}
				}
			}
		}
		else if (type == StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_OUT_OF_PRIORITY)
		{
			// Plan where iPlnTyp = 9
			IList<StateMod_Plan> dataList2 = (IList<StateMod_Plan>)dataset.getComponentForComponentType(StateMod_DataSet.COMP_PLANS).getData();
			foreach (StateMod_Plan plan in dataList2)
			{
				if (plan.getIPlnTyp() == 9)
				{
					if ((string.ReferenceEquals(idToMatch, null)) || idToMatch.Equals(plan.getID(), StringComparison.OrdinalIgnoreCase))
					{
						dataList.Add(plan);
					}
				}
			}
		}
		else if (type == StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_RECHARGE)
		{
			// Plan where iPlnTyp = 8
			IList<StateMod_Plan> dataList2 = (IList<StateMod_Plan>)dataset.getComponentForComponentType(StateMod_DataSet.COMP_PLANS).getData();
			foreach (StateMod_Plan plan in dataList2)
			{
				if (plan.getIPlnTyp() == 8)
				{
					if ((string.ReferenceEquals(idToMatch, null)) || idToMatch.Equals(plan.getID(), StringComparison.OrdinalIgnoreCase))
					{
						dataList.Add(plan);
					}
				}
			}
		}
		else if (type == StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_RELEASE_LIMIT)
		{
			// Plan where iPlnTyp = 12
			IList<StateMod_Plan> dataList2 = (IList<StateMod_Plan>)dataset.getComponentForComponentType(StateMod_DataSet.COMP_PLANS).getData();
			foreach (StateMod_Plan plan in dataList2)
			{
				if (plan.getIPlnTyp() == 12)
				{
					if ((string.ReferenceEquals(idToMatch, null)) || idToMatch.Equals(plan.getID(), StringComparison.OrdinalIgnoreCase))
					{
						dataList.Add(plan);
					}
				}
			}
		}
		else if (type == StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_REUSE_TO_DIVERSION)
		{
			// Plan where iPlnTyp = 4
			IList<StateMod_Plan> dataList2 = (IList<StateMod_Plan>)dataset.getComponentForComponentType(StateMod_DataSet.COMP_PLANS).getData();
			foreach (StateMod_Plan plan in dataList2)
			{
				if (plan.getIPlnTyp() == 4)
				{
					if ((string.ReferenceEquals(idToMatch, null)) || idToMatch.Equals(plan.getID(), StringComparison.OrdinalIgnoreCase))
					{
						dataList.Add(plan);
					}
				}
			}
		}
		else if (type == StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_REUSE_TO_DIVERSION_FROM_TRANSMOUNTAIN)
		{
			// Plan where iPlnTyp = 6
			IList<StateMod_Plan> dataList2 = (IList<StateMod_Plan>)dataset.getComponentForComponentType(StateMod_DataSet.COMP_PLANS).getData();
			foreach (StateMod_Plan plan in dataList2)
			{
				if (plan.getIPlnTyp() == 6)
				{
					if ((string.ReferenceEquals(idToMatch, null)) || idToMatch.Equals(plan.getID(), StringComparison.OrdinalIgnoreCase))
					{
						dataList.Add(plan);
					}
				}
			}
		}
		else if (type == StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_REUSE_TO_RESERVOIR)
		{
			// Plan where iPlnTyp = 3
			IList<StateMod_Plan> dataList2 = (IList<StateMod_Plan>)dataset.getComponentForComponentType(StateMod_DataSet.COMP_PLANS).getData();
			foreach (StateMod_Plan plan in dataList2)
			{
				if (plan.getIPlnTyp() == 3)
				{
					if ((string.ReferenceEquals(idToMatch, null)) || idToMatch.Equals(plan.getID(), StringComparison.OrdinalIgnoreCase))
					{
						dataList.Add(plan);
					}
				}
			}
		}
		else if (type == StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_REUSE_TO_RESERVOIR_FROM_TRANSMOUNTAIN)
		{
			// Plan where iPlnTyp = 5
			IList<StateMod_Plan> dataList2 = (IList<StateMod_Plan>)dataset.getComponentForComponentType(StateMod_DataSet.COMP_PLANS).getData();
			foreach (StateMod_Plan plan in dataList2)
			{
				if (plan.getIPlnTyp() == 5)
				{
					if ((string.ReferenceEquals(idToMatch, null)) || idToMatch.Equals(plan.getID(), StringComparison.OrdinalIgnoreCase))
					{
						dataList.Add(plan);
					}
				}
			}
		}
		else if (type == StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_SPECIAL_WELL_AUGMENTATION)
		{
			// Plan where iPlnTyp = 10
			IList<StateMod_Plan> dataList2 = (IList<StateMod_Plan>)dataset.getComponentForComponentType(StateMod_DataSet.COMP_PLANS).getData();
			foreach (StateMod_Plan plan in dataList2)
			{
				if (plan.getIPlnTyp() == 10)
				{
					if ((string.ReferenceEquals(idToMatch, null)) || idToMatch.Equals(plan.getID(), StringComparison.OrdinalIgnoreCase))
					{
						dataList.Add(plan);
					}
				}
			}
		}
		else if (type == StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_TC)
		{
			// Plan where iPlnTyp = 1
			IList<StateMod_Plan> dataList2 = (IList<StateMod_Plan>)dataset.getComponentForComponentType(StateMod_DataSet.COMP_PLANS).getData();
			foreach (StateMod_Plan plan in dataList2)
			{
				if (plan.getIPlnTyp() == 1)
				{
					if ((string.ReferenceEquals(idToMatch, null)) || idToMatch.Equals(plan.getID(), StringComparison.OrdinalIgnoreCase))
					{
						dataList.Add(plan);
					}
				}
			}
		}
		else if (type == StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_TRANSMOUNTAIN_IMPORT)
		{
			// Plan where iPlnTyp = 7
			IList<StateMod_Plan> dataList2 = (IList<StateMod_Plan>)dataset.getComponentForComponentType(StateMod_DataSet.COMP_PLANS).getData();
			foreach (StateMod_Plan plan in dataList2)
			{
				if (plan.getIPlnTyp() == 7)
				{
					if ((string.ReferenceEquals(idToMatch, null)) || idToMatch.Equals(plan.getID(), StringComparison.OrdinalIgnoreCase))
					{
						dataList.Add(plan);
					}
				}
			}
		}
		else if (type == StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_WELL_AUGMENTATION)
		{
			// Plan where iPlnTyp = 2
			IList<StateMod_Plan> dataList2 = (IList<StateMod_Plan>)dataset.getComponentForComponentType(StateMod_DataSet.COMP_PLANS).getData();
			foreach (StateMod_Plan plan in dataList2)
			{
				if (plan.getIPlnTyp() == 2)
				{
					if ((string.ReferenceEquals(idToMatch, null)) || idToMatch.Equals(plan.getID(), StringComparison.OrdinalIgnoreCase))
					{
						dataList.Add(plan);
					}
				}
			}
		}
		else if (type == StateMod_OperationalRight_Metadata_SourceOrDestinationType.RESERVOIR)
		{
			// All reservoir stations
			dataList = (IList<StateMod_Reservoir>)dataset.getComponentForComponentType(StateMod_DataSet.COMP_RESERVOIR_STATIONS).getData();
		}
		else if (type == StateMod_OperationalRight_Metadata_SourceOrDestinationType.RESERVOIR_RIGHT)
		{
			// Reservoir rights
			IList<StateMod_ReservoirRight> dataList2 = (IList<StateMod_ReservoirRight>)dataset.getComponentForComponentType(StateMod_DataSet.COMP_RESERVOIR_RIGHTS).getData();
			foreach (StateMod_ReservoirRight resRight in dataList2)
			{
				if (returnStations)
				{
					// Get the reservoir station associated with the right
					System.Collections.IList resList = (IList<StateMod_Reservoir>)dataset.getComponentForComponentType(StateMod_DataSet.COMP_RESERVOIR_STATIONS).getData();
					int pos = indexOf(resList, resRight.getLocationIdentifier());
					if (pos >= 0)
					{
						if ((string.ReferenceEquals(idToMatch, null)) || idToMatch.Equals(resRight.getID(), StringComparison.OrdinalIgnoreCase))
						{
							dataList.Add(resList[pos]);
						}
					}
				}
				else
				{
					if ((string.ReferenceEquals(idToMatch, null)) || idToMatch.Equals(resRight.getID(), StringComparison.OrdinalIgnoreCase))
					{
						dataList.Add(resRight);
					}
				}
			}
		}
		else if (type == StateMod_OperationalRight_Metadata_SourceOrDestinationType.RIVER_NODE)
		{
			// All river nodes
			dataList = (IList<StateMod_RiverNetworkNode>)dataset.getComponentForComponentType(StateMod_DataSet.COMP_RIVER_NETWORK).getData();
		}
		else if (type == StateMod_OperationalRight_Metadata_SourceOrDestinationType.STREAM_GAGE)
		{
			// All stream gage stations
			dataList = (IList<StateMod_StreamGage>)dataset.getComponentForComponentType(StateMod_DataSet.COMP_STREAMGAGE_STATIONS).getData();
		}
		else if (type == StateMod_OperationalRight_Metadata_SourceOrDestinationType.WELL)
		{
			// All well stations
			dataList = (IList<StateMod_Well>)dataset.getComponentForComponentType(StateMod_DataSet.COMP_WELL_STATIONS).getData();
		}
		else if (type == StateMod_OperationalRight_Metadata_SourceOrDestinationType.WELL_RIGHT)
		{
			// Wells rights
			IList<StateMod_WellRight> dataList2 = (IList<StateMod_WellRight>)dataset.getComponentForComponentType(StateMod_DataSet.COMP_WELL_RIGHTS).getData();
			foreach (StateMod_WellRight wellRight in dataList2)
			{
				if (returnStations)
				{
					// Get the well station associated with the right
					System.Collections.IList wellList = (IList<StateMod_Well>)dataset.getComponentForComponentType(StateMod_DataSet.COMP_WELL_STATIONS).getData();
					int pos = indexOf(wellList, wellRight.getLocationIdentifier());
					if (pos >= 0)
					{
						if ((string.ReferenceEquals(idToMatch, null)) || idToMatch.Equals(wellRight.getID(), StringComparison.OrdinalIgnoreCase))
						{
							dataList.Add(wellList[pos]);
						}
					}
				}
				else
				{
					if ((string.ReferenceEquals(idToMatch, null)) || idToMatch.Equals(wellRight.getID(), StringComparison.OrdinalIgnoreCase))
					{
						dataList.Add(wellRight);
					}
				}
			}
		}
		if ((!string.ReferenceEquals(idToMatch, null)) && ((type == StateMod_OperationalRight_Metadata_SourceOrDestinationType.DIVERSION) || (type == StateMod_OperationalRight_Metadata_SourceOrDestinationType.INSTREAM_FLOW) || (type == StateMod_OperationalRight_Metadata_SourceOrDestinationType.OPERATIONAL_RIGHT) || (type == StateMod_OperationalRight_Metadata_SourceOrDestinationType.RESERVOIR) || (type == StateMod_OperationalRight_Metadata_SourceOrDestinationType.RIVER_NODE) || (type == StateMod_OperationalRight_Metadata_SourceOrDestinationType.STREAM_GAGE) || (type == StateMod_OperationalRight_Metadata_SourceOrDestinationType.WELL)))
		{
			// Further process the initial lists
			IList<StateMod_Data> dataList2 = new List<object>();
			foreach (StateMod_Data smdata in (IList<StateMod_Data>)dataList)
			{
				if (smdata.getID().Equals(idToMatch, StringComparison.OrdinalIgnoreCase))
				{
					dataList2.Add(smdata);
				}
			}
			dataList = dataList2;
		}
		return dataList;
	}

	/// <summary>
	/// Return a list of identifiers given a list of StateMod data. </summary>
	/// <param name="smdataList"> list of StateMod data objects </param>
	/// <param name="sort"> if true, sort the identifiers before returning </param>
	/// <returns> a list of identifiers given a list of StateMod data, guaranteed to be non-null. </returns>
	public static IList<string> getIDList<T1>(IList<T1> smdataList, bool sort) where T1 : StateMod_Data
	{
		IList<string> idList = new List<string>();
		if (smdataList == null)
		{
			return idList;
		}
		foreach (StateMod_Data smdata in smdataList)
		{
			idList.Add(smdata.getID());
		}
		if (sort)
		{
			idList.Sort();
		}
		return idList;
	}

	/// <summary>
	/// Helper method to return validators to check an ID. </summary>
	/// <returns> List of Validators. </returns>
	public static Validator[] getIDValidators()
	{
		return new Validator[] {Validators.notBlankValidator(), Validators.regexValidator("^[0-9a-zA-Z\\._]+$")};
	}

	/// <summary>
	/// Helper method to return general validators for numbers. </summary>
	/// <returns> List of Validators. </returns>
	public static Validator[] getNumberValidators()
	{
		return new Validator[] {Validators.notBlankValidator(), Validators.rangeValidator(0, 999999)};
	}

	/// <summary>
	/// Helper method to return general validators for an on/off switch. </summary>
	/// <returns> List of Validators. </returns>
	public static Validator[] getOnOffSwitchValidator()
	{
		Validator[] orValidator = new Validator[] {Validators.isEquals(new int?(0)), Validators.isEquals(new int?(1))};
		return new Validator[] {Validators.notBlankValidator(), Validators.or(orValidator)};
	}

	/// <summary>
	/// Return the list of water rights for a station.  The "cgoto" value in the water
	/// rights is compared with the supplied station identifier. </summary>
	/// <param name="station_id"> Station identifier to match. </param>
	/// <param name="rights_Vector"> The full list of water rights to search. </param>
	/// <returns> the list of water rights that match the station identifier.  A non-null
	/// list is guaranteed (but may have zero length). </returns>
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public static java.util.List<? extends StateMod_Right> getRightsForStation(String station_id, java.util.List<? extends StateMod_Right> rights_Vector)
	public static IList<StateMod_Right> getRightsForStation<T1>(string station_id, IList<T1> rights_Vector) where T1 : StateMod_Right
	{
		IList<StateMod_Right> matches = new List<StateMod_Right>();
		int size = 0;
		if (rights_Vector != null)
		{
			size = rights_Vector.Count;
		}
		object o;
		StateMod_DiversionRight ddr;
		StateMod_InstreamFlowRight ifr;
		StateMod_ReservoirRight rer;
		StateMod_WellRight wer;
		for (int i = 0; i < size; i++)
		{
			o = (object)rights_Vector[i];
			if (o is StateMod_DiversionRight)
			{
				ddr = (StateMod_DiversionRight)o;
				if (ddr.getCgoto().Equals(station_id, StringComparison.OrdinalIgnoreCase))
				{
					matches.Add(ddr);
				}
			}
			else if (o is StateMod_InstreamFlowRight)
			{
				ifr = (StateMod_InstreamFlowRight)o;
				if (ifr.getCgoto().Equals(station_id, StringComparison.OrdinalIgnoreCase))
				{
					matches.Add(ifr);
				}
			}
			else if (o is StateMod_ReservoirRight)
			{
				rer = (StateMod_ReservoirRight)o;
				if (rer.getCgoto().Equals(station_id, StringComparison.OrdinalIgnoreCase))
				{
					matches.Add(rer);
				}
			}
			else if (o is StateMod_WellRight)
			{
				wer = (StateMod_WellRight)o;
				if (wer.getCgoto().Equals(station_id, StringComparison.OrdinalIgnoreCase))
				{
					matches.Add(wer);
				}
			}
		}
		return matches;
	}

	/// <summary>
	/// Return the SmDelta executable.  This can be a full path or if not, the PATH
	/// environment variable is relied on to find the executable.
	/// By default this is expanded if any ${property} tokens are used (e.g., ${Home} for
	/// the software installation home). </summary>
	/// <returns> the StateMod executable name. </returns>
	public static string getSmDeltaExecutable()
	{
		return getSmDeltaExecutable(true);
	}

	/// <summary>
	/// Return the SmDelta executable.  This can be a full path or if not, the PATH
	/// environment variable is relied on to find the executable.
	/// By default this is expanded if any ${property} tokens are used (e.g., ${Home} for
	/// the software installation home). </summary>
	/// <returns> the SmDelta executable name. </returns>
	/// <param name="expanded"> If false, ${properties} will not be expanded (e.g., use to show in
	/// configuration dialogs).  If true, properties will be expanced.  Currently ${Home}
	/// (case-insensitive) is expanced using a call to RTi.Util.IO.IOUtil.getApplicationHomeDir(), if not empty. </param>
	public static string getSmDeltaExecutable(bool expanded)
	{
		if (string.ReferenceEquals(__smdeltaExecutable, null))
		{
			return null;
		}
		else if (!expanded)
		{
			return __smdeltaExecutable;
		}
		// Expand properties if found.
		string home = IOUtil.getApplicationHomeDir();
		if ((!string.ReferenceEquals(home, null)) && (home.Length > 0))
		{
			// Replace ${Home} with supplied value.  Since there is currently no elegant
			// way to deal with case (and not enforcing on the input end yet), check several variants.
			string s = StringUtil.replaceString(__smdeltaExecutable, "${Home}", home);
			s = StringUtil.replaceString(s, "${HOME}", home);
			s = StringUtil.replaceString(s, "${home}", home);
			return s;
		}
		else
		{
			return __smdeltaExecutable;
		}
	}

	/// <summary>
	/// Return the StateMod executable.  This can be a full path or if not, the PATH
	/// environment variable is relied on to find the executable.
	/// By default this is expanded if any ${property} tokens are used (e.g., ${Home} for
	/// the software installation home, ${WorkingDir} for the response file folder). </summary>
	/// <returns> the StateMod executable name. </returns>
	public static string getStateModExecutable(string home, string workingDir)
	{
		return getStateModExecutable(home, workingDir, true);
	}

	/// <summary>
	/// Return the StateMod executable that is by default used with the GUI.
	/// This can be a full path or if not, the PATH environment variable is relied on to find the executable.
	/// By default this is expanded if any ${property} tokens are used (e.g., ${Home} for
	/// the software installation home, ${WorkingDir} for the response file folder). </summary>
	/// <returns> the StateMod executable name. </returns>
	/// <param name="home"> home (installation) folder for StateMod GUI </param>
	/// <param name="workingDir"> response file folder </param>
	/// <param name="expanded"> If false, ${properties} will not be expanded (e.g., use to show in
	/// configuration dialogs).  If true, properties will be expanced. </param>
	public static string getStateModExecutable(string home, string workingDir, bool expanded)
	{
		if (string.ReferenceEquals(__statemodExecutable, null))
		{
			return null;
		}
		else if (!expanded)
		{
			return __statemodExecutable;
		}
		// Expand properties if found.
		string statemodExecutable = __statemodExecutable;
		if ((!string.ReferenceEquals(home, null)) && (home.Length > 0))
		{
			// Replace ${Home} with supplied value.  Since there is currently no elegant
			// way to deal with case (and not enforcing on the input end yet), check several variants.
			statemodExecutable = StringUtil.replaceString(statemodExecutable, "${Home}", home);
			statemodExecutable = StringUtil.replaceString(statemodExecutable, "${HOME}", home);
			statemodExecutable = StringUtil.replaceString(statemodExecutable, "${home}", home);
		}
		if ((!string.ReferenceEquals(workingDir, null)) && (workingDir.Length > 0))
		{
			// Replace ${WorkingDir} with supplied value.  Since there is currently no elegant
			// way to deal with case (and not enforcing on the input end yet), check several variants.
			statemodExecutable = StringUtil.replaceString(statemodExecutable, "${WorkingDir}", workingDir);
			statemodExecutable = StringUtil.replaceString(statemodExecutable, "${WORKINGDIR}", workingDir);
			statemodExecutable = StringUtil.replaceString(statemodExecutable, "${workingdir}", workingDir);
		}
		return statemodExecutable;
	}

	/// <summary>
	/// Return the StateMod model version, which was determined in the last call to runStateMod ( ... "-version" ).
	/// </summary>
	public static string getStateModVersion()
	{
		return __statemodVersion;
	}

	/// <summary>
	/// Get the StateMod model revision date, which was determined in the last call to runStateMod ( ... "-version" ).
	/// </summary>
	public static string getStateModRevisionDate()
	{
		return __statemodRevisionDate;
	}

	/// <summary>
	/// Return the latest known StateMod model version.  This can be used as a default
	/// if getStateModVersion() returns blank, meaning the version has not been
	/// determined.  In this case, the latest version is a good guess, especially for
	/// determining binary file parameters, which don't change much between versions.
	/// </summary>
	public static string getStateModVersionLatest()
	{
		return __statemodVersionLatest;
	}

	// TODO- may need to add parameters to modify the list based on what is
	// available in a data set (e.g., leave out wells).
	/// <summary>
	/// Return a list of station types for use in the GUI. </summary>
	/// <returns> a list of station types for use in the GUI. </returns>
	public static IList<string> getStationTypes()
	{
		return StringUtil.toList(__station_types);
	}

	// TODO SAM 2010-12-21 Should plans be included?
	/// <summary>
	/// Get the time series data types associated with a component, for use with the
	/// graphing tool.  Currently this returns all possible data types but does not
	/// cut down the lists based on what is actually available. </summary>
	/// <param name="comp_type"> Component type for a station:
	/// StateMod_DataSet.COMP_STREAMGAGE_STATIONS,
	/// StateMod_DataSet.COMP_DIVERSION_STATIONS,
	/// StateMod_DataSet.COMP_RESERVOIR_STATIONS,
	/// StateMod_DataSet.COMP_INSTREAM_STATIONS,
	/// StateMod_DataSet.COMP_WELL_STATIONS, or
	/// StateMod_DataSet.COMP_STREAMESTIMATE_STATIONS. </param>
	/// <param name="id"> If non-null, it will be used with the data set to limit returned
	/// choices to those appropriate for the dataset. </param>
	/// <param name="dataset"> If a non-null StateMod_DataSet is specified, it will be used with
	/// the id to check for valid time series data types.  For example, it can be used
	/// to return data types for estimated time series. </param>
	/// <param name="statemodVersion"> StateMod version as a string.
	/// If unknown (blank), the parameters for the older version 9.69 will be returned. </param>
	/// <param name="interval"> TimeInterval.DAY or TimeInterval.MONTH. </param>
	/// <param name="include_input"> If true, input time series including historic data from
	/// ASCII input files are returned with the list (suitable for StateMod GUI graphing tool). </param>
	/// <param name="include_input_estimated"> If true, input time series that are estimated are included. </param>
	/// <param name="include_output"> If true, output time series are included in the list (this
	/// is used by the graphing tool).  Note that some output time series are for
	/// internal model use and are not suitable for viewing (as per Ray Bennett) and
	/// are therefore not returned in this list. </param>
	/// <param name="check_availability"> If true, an input data type will only be added if it
	/// is available in the input data set.  Because it is difficult and somewhat time
	/// consuming to check for the validity of output time series, output time series
	/// are not checked.  This flag is currently not used. </param>
	/// <param name="add_group"> If true, a group is added to the front of the data type to
	/// allow grouping of the parameters.  Currently this should only be used for
	/// output parametes (e.g., in TSTool) because other data types have not been grouped. </param>
	/// <param name="add_note"> If true, the string " - Input", " - Output" will be added to the
	/// data types, to help identify input and output parameters.  This is particularly
	/// useful when retrieving time series. </param>
	/// <returns> a non-null list of data types.  The list will have zero size if no
	/// data types are requested or are valid. </returns>
	public static IList<string> getTimeSeriesDataTypes(int comp_type, string id, StateMod_DataSet dataset, string statemodVersion, int interval, bool include_input, bool include_input_estimated, bool include_output, bool check_availability, bool add_group, bool add_note)
	{
		return getTimeSeriesDataTypes(null, comp_type, id, dataset, statemodVersion, interval, include_input, include_input_estimated, include_output, check_availability, add_group, add_note);
	}

	/// <summary>
	/// Get the time series data types associated with a component, for use with the
	/// graphing tool.  Currently this returns all possible data types but does not
	/// cut down the lists based on what is actually available. </summary>
	/// <param name="comp_type"> Component type for a station:
	/// StateMod_DataSet.COMP_STREAMGAGE_STATIONS,
	/// StateMod_DataSet.COMP_DIVERSION_STATIONS,
	/// StateMod_DataSet.COMP_RESERVOIR_STATIONS,
	/// StateMod_DataSet.COMP_INSTREAM_STATIONS,
	/// StateMod_DataSet.COMP_WELL_STATIONS, or
	/// StateMod_DataSet.COMP_STREAMESTIMATE_STATIONS. </param>
	/// <param name="binary_filename"> name of the binary output file for which data types (parameters) are being returned,
	/// typically selected by the user with a file chooser.  The path to the file is not adjusted to a working
	/// directory so do that before calling, if necessary. </param>
	/// <param name="id"> If non-null, it will be used with the data set to limit returned
	/// choices to those appropriate for the dataset. </param>
	/// <param name="dataset"> If a non-null StateMod_DataSet is specified, it will be used with
	/// the id to check for valid time series data types.  For example, it can be used
	/// to return data types for estimated time series. </param>
	/// <param name="statemodVersion"> StateMod version as a string.  If this
	/// is greater than or equal to VERSION_11_00, then binary file parameters are read from the
	/// file.  Otherwise, the parameters are hard-coded in this method, based on StateMod documentation.
	/// If blank, the parameters for version 9.69 will be returned. </param>
	/// <param name="interval"> TimeInterval.DAY or TimeInterval.MONTH. </param>
	/// <param name="include_input"> If true, input time series including historic data from
	/// ASCII input files are returned with the list (suitable for StateMod GUI graphing tool). </param>
	/// <param name="include_input_estimated"> If true, input time series that are estimated are included. </param>
	/// <param name="include_output"> If true, output time series are included in the list (this
	/// is used by the graphing tool).  Note that some output time series are for
	/// internal model use and are not suitable for viewing (as per Ray Bennett) and
	/// are therefore not returned in this list. </param>
	/// <param name="check_availability"> If true, an input data type will only be added if it
	/// is available in the input data set.  Because it is difficult and somewhat time
	/// consuming to check for the validity of output time series, output time series
	/// are not checked.  This flag is currently not used. </param>
	/// <param name="add_group"> If true, a group is added to the front of the data type to
	/// allow grouping of the parameters.  Currently this should only be used for
	/// output parametes (e.g., in TSTool) because other data types have not been grouped. </param>
	/// <param name="add_note"> If true, the string " - Input", " - Output" will be added to the
	/// data types, to help identify input and output parameters.  This is particularly
	/// useful when retrieving time series. </param>
	/// <returns> a non-null list of data types.  The list will have zero size if no
	/// data types are requested or are valid. </returns>
	public static IList<string> getTimeSeriesDataTypes(string binary_filename, int comp_type, string id, StateMod_DataSet dataset, string statemodVersion, int interval, bool include_input, bool include_input_estimated, bool include_output, bool check_availability, bool add_group, bool add_note)
	{
		string routine = "StateMod_Util.getTimeSeriesDataTypes";
		IList<string> data_types = new List<string>();
		string[] diversion_types0 = null;
		string[] instream_types0 = null;
		string[] reservoir_types0 = null;
		string[] stream_types0 = null;
		string[] well_types0 = null;
		string[] diversion_types = null;
		string[] instream_types = null;
		string[] reservoir_types = null;
		string[] stream_types = null;
		string[] well_types = null;

		// If a filename is given and reading it shows a version >= 11.0, read
		// information from the file for use below.

		StateMod_BTS bts = null;
		if (!string.ReferenceEquals(binary_filename, null))
		{
			try
			{
				bts = new StateMod_BTS(binary_filename);
			}
			catch (Exception e)
			{
				// Error reading the file.  Print a warning but go on and just do not have a list of parameters...
				Message.printWarning(3, routine, "Error opening/reading binary file \"" + binary_filename + "\" to determine parameters.");
				Message.printWarning(3, routine, e);
				bts = null;
			}
			// Close the file below after getting information...
			string version = bts.getVersion();
			if (isVersionAtLeast(version, VERSION_11_00))
			{
				// Reset information to override possible user flags because information in file controls.
				statemodVersion = version;
				add_group = false; // Not available from file
				add_note = false; // Not available from file
			}
		}

		if ((comp_type == StateMod_DataSet.COMP_RESERVOIR_STATIONS) && (!string.ReferenceEquals(id, null)) && (id.IndexOf('-') > 0))
		{
			// If the identifier includes a dash, turn off the input data
			// types because only output time series are available for owner/accounts...
			include_input = false;
			include_input_estimated = false;
		}

		// Get the list of output data types based on the StateMod version.  These are then used below.
		if (statemodVersion.Equals(""))
		{
			// Default is the latest version before the parameters are in the binary file...
			statemodVersion = VERSION_9_69;
		}
		if (isVersionAtLeast(statemodVersion, VERSION_11_00))
		{
			// The parameters come from the binary file header.  Close the file because it is no longer needed...
			string[] parameters = null;
			if (bts != null)
			{
				parameters = bts.getParameters();
				// TODO SAM 2006-01-15 Remove when tested in production.
				//Message.printStatus ( 2, routine, "Parameters from file:  " + StringUtil.toVector(parameters) );
				try
				{
					bts.close();
				}
				catch (Exception)
				{
					// Ignore - problem would have occurred at open.
				}
				bts = null;
			}
			// The binary file applies only to certain node types...
			if ((comp_type == StateMod_DataSet.COMP_STREAMGAGE_STATIONS) || (comp_type == StateMod_DataSet.COMP_STREAMESTIMATE_STATIONS) || (comp_type == StateMod_DataSet.COMP_DIVERSION_STATIONS) || (comp_type == StateMod_DataSet.COMP_INSTREAM_STATIONS))
			{
				diversion_types0 = parameters;
				instream_types0 = parameters;
				stream_types0 = parameters;
			}
			else if (comp_type == StateMod_DataSet.COMP_RESERVOIR_STATIONS)
			{
				reservoir_types0 = parameters;
			}
			else if (comp_type == StateMod_DataSet.COMP_WELL_STATIONS)
			{
				well_types0 = parameters;
			}
		}
		else if (isVersionAtLeast(statemodVersion, VERSION_9_69))
		{
			// The parameters are hard-coded because they are not in the binary file header.
			diversion_types0 = __output_ts_data_types_diversion_0969;
			instream_types0 = __output_ts_data_types_instream_0969;
			reservoir_types0 = __output_ts_data_types_reservoir_0969;
			stream_types0 = __output_ts_data_types_stream_0969;
			well_types0 = __output_ts_data_types_well_0969;
		}
		else if (isVersionAtLeast(statemodVersion, VERSION_9_01))
		{
			// The parameters are hard-coded because they are not in the binary file header.
			diversion_types0 = __output_ts_data_types_diversion_0901;
			instream_types0 = __output_ts_data_types_instream_0901;
			reservoir_types0 = __output_ts_data_types_reservoir_0901;
			stream_types0 = __output_ts_data_types_stream_0901;
			well_types0 = __output_ts_data_types_well_0901;
		}
		else
		{
			// Assume very old...
			// The parameters are hard-coded because they are not in the binary file header.
			diversion_types0 = __output_ts_data_types_diversion_0100;
			instream_types0 = __output_ts_data_types_instream_0100;
			reservoir_types0 = __output_ts_data_types_reservoir_0100;
			stream_types0 = __output_ts_data_types_stream_0100;
			well_types0 = null; // Should never happen.
		}
		// TODO SAM 2006-01-15 Remove when tested in production.
		/*
		Message.printStatus ( 2, routine, "Diversion parameters from file:  " + StringUtil.toVector(diversion_types0) );
		Message.printStatus ( 2, routine, "Reservoir parameters from file:  " + StringUtil.toVector(reservoir_types0) );
		Message.printStatus ( 2, routine, "Instream parameters from file:  " + StringUtil.toVector(instream_types0) );
		Message.printStatus ( 2, routine, "Stream parameters from file:  " + StringUtil.toVector(stream_types0) );
		Message.printStatus ( 2, routine, "Well parameters from file:  " + StringUtil.toVector(well_types0) );
		*/

		// Based on the requested data type, put together a list of time series
		// data types.  To simplify determination of whether a type is input or
		// output, add one of the following descriptors to the end if requested...
		string input = "";
		string output = "";
		// The above lists contain the data group.  If the group is NOT desired, remove the group below...
		if (add_note)
		{
			input = " - Input";
			output = " - Output";
		}
		int diversion_types_length = 0;
		int instream_types_length = 0;
		int reservoir_types_length = 0;
		int stream_types_length = 0;
		int well_types_length = 0;
		if (diversion_types0 != null)
		{
			diversion_types_length = diversion_types0.Length;
			diversion_types = new string[diversion_types_length];
		}
		if (instream_types0 != null)
		{
			instream_types_length = instream_types0.Length;
			instream_types = new string[instream_types_length];
		}
		if (reservoir_types0 != null)
		{
			reservoir_types_length = reservoir_types0.Length;
			reservoir_types = new string[reservoir_types_length];
		}
		if (stream_types0 != null)
		{
			stream_types_length = stream_types0.Length;
			stream_types = new string[stream_types_length];
		}
		if (well_types0 != null)
		{
			well_types_length = well_types0.Length;
			well_types = new string[well_types_length];
		}
		for (int i = 0; i < diversion_types_length; i++)
		{
			if (add_group)
			{
				diversion_types[i] = diversion_types0[i] + output;
			}
			else
			{
				// Remove group from front if necessary...
				if (diversion_types0[i].IndexOf("-", StringComparison.Ordinal) > 0)
				{
					diversion_types[i] = StringUtil.getToken(diversion_types0[i],"-",0,1).Trim() + output;
				}
				else
				{
					diversion_types[i] = diversion_types0[i];
				}
			}
		}
		for (int i = 0; i < instream_types_length; i++)
		{
			if (add_group)
			{
				instream_types[i] = instream_types0[i] + output;
			}
			else
			{
				// Remove group from front if necessary...
				if (instream_types0[i].IndexOf("-", StringComparison.Ordinal) > 0)
				{
					instream_types[i] = StringUtil.getToken(instream_types0[i],"-",0,1).Trim() + output;
				}
				else
				{
					instream_types[i] = instream_types0[i];
				}
			}
		}
		for (int i = 0; i < reservoir_types_length; i++)
		{
			if (add_group)
			{
				reservoir_types[i] = reservoir_types0[i] + output;
			}
			else
			{
				// Remove group from front if necessary...
				if (reservoir_types0[i].IndexOf("-", StringComparison.Ordinal) > 0)
				{
					reservoir_types[i] = StringUtil.getToken(reservoir_types0[i],"-",0,1).Trim() + output;
				}
				else
				{
					reservoir_types[i] = reservoir_types0[i];
				}
			}
		}
		for (int i = 0; i < stream_types_length; i++)
		{
			if (add_group)
			{
				stream_types[i] = stream_types0[i] + output;
			}
			else
			{
				// Remove group from front if necessary...
				if (stream_types0[i].IndexOf("-", StringComparison.Ordinal) > 0)
				{
					stream_types[i] = StringUtil.getToken(stream_types0[i],"-",0,1).Trim() + output;
				}
				else
				{
					stream_types[i] = stream_types0[i];
				}
			}
		}
		for (int i = 0; i < well_types_length; i++)
		{
			if (add_group)
			{
				well_types[i] = well_types0[i] + output;
			}
			else
			{
				// Remove group from front if necessary...
				if (well_types0[i].IndexOf("-", StringComparison.Ordinal) > 0)
				{
					well_types[i] = StringUtil.getToken(well_types0[i],"-",0,1).Trim() + output;
				}
				else
				{
					well_types[i] = well_types0[i];
				}
			}
		}

		if ((comp_type == StateMod_DataSet.COMP_STREAMGAGE_STATIONS) || (comp_type == StateMod_DataSet.COMP_STREAMESTIMATE_STATIONS))
		{
			// Stream gage and stream estimate stations are the same other
			// than stream estimate do not have historical time series...
			// Include input time series if reqeusted...
			// Input baseflow...
			if (include_input && (interval == TimeInterval.MONTH))
			{
				data_types.Add(StateMod_DataSet.lookupTimeSeriesDataType(StateMod_DataSet.COMP_STREAMGAGE_NATURAL_FLOW_TS_MONTHLY) + input);
			}
			else if (include_input && (interval == TimeInterval.DAY))
			{
				data_types.Add(StateMod_DataSet.lookupTimeSeriesDataType(StateMod_DataSet.COMP_STREAMGAGE_NATURAL_FLOW_TS_DAILY) + input);
			}
			// Input historical time series if requested...
			if (include_input && (interval == TimeInterval.MONTH) && (comp_type == StateMod_DataSet.COMP_STREAMGAGE_STATIONS))
			{
				data_types.Add(StateMod_DataSet.lookupTimeSeriesDataType(StateMod_DataSet.COMP_STREAMGAGE_HISTORICAL_TS_MONTHLY) + input);
			}
			else if (include_input && (interval == TimeInterval.DAY) && (comp_type == StateMod_DataSet.COMP_STREAMGAGE_STATIONS))
			{
				data_types.Add(StateMod_DataSet.lookupTimeSeriesDataType(StateMod_DataSet.COMP_STREAMGAGE_HISTORICAL_TS_DAILY) + input);
			}
			// Include the estimated input time series if requested...
			// Add the estimated input...
			if (include_input_estimated && (interval == TimeInterval.DAY))
			{
				// TODO - need to check daily ID on station...
				data_types.Add(StateMod_DataSet.lookupTimeSeriesDataType(StateMod_DataSet.COMP_STREAMGAGE_NATURAL_FLOW_TS_DAILY) + "Estimated" + input);
			}
			// Input historical time series if requested...
			if (include_input_estimated && (interval == TimeInterval.DAY) && (comp_type == StateMod_DataSet.COMP_STREAMGAGE_STATIONS))
			{
				// TODO - need to check daily ID on station...
				data_types.Add(StateMod_DataSet.lookupTimeSeriesDataType(StateMod_DataSet.COMP_STREAMGAGE_HISTORICAL_TS_DAILY) + "Estimated" + input);
			}
			// Include the output time series if requested...
			if (include_output)
			{
				data_types = StringUtil.addListToStringList(data_types, StringUtil.toList(stream_types));
			}
		}
		else if (comp_type == StateMod_DataSet.COMP_DIVERSION_STATIONS)
		{
			if (include_input && (interval == TimeInterval.MONTH))
			{
				data_types.Add(StateMod_DataSet.lookupTimeSeriesDataType(StateMod_DataSet.COMP_DIVERSION_TS_MONTHLY) + input);
				data_types.Add(StateMod_DataSet.lookupTimeSeriesDataType(StateMod_DataSet.COMP_DEMAND_TS_MONTHLY) + input);
				data_types.Add(StateMod_DataSet.lookupTimeSeriesDataType(StateMod_DataSet.COMP_DEMAND_TS_OVERRIDE_MONTHLY) + input);
				data_types.Add(StateMod_DataSet.lookupTimeSeriesDataType(StateMod_DataSet.COMP_DEMAND_TS_AVERAGE_MONTHLY) + input);
				data_types.Add(StateMod_DataSet.lookupTimeSeriesDataType(StateMod_DataSet.COMP_CONSUMPTIVE_WATER_REQUIREMENT_TS_MONTHLY) + input);
			}
			else if (include_input && (interval == TimeInterval.DAY))
			{
				data_types.Add(StateMod_DataSet.lookupTimeSeriesDataType(StateMod_DataSet.COMP_DIVERSION_TS_DAILY) + input);
				data_types.Add(StateMod_DataSet.lookupTimeSeriesDataType(StateMod_DataSet.COMP_DEMAND_TS_DAILY) + input);
				data_types.Add(StateMod_DataSet.lookupTimeSeriesDataType(StateMod_DataSet.COMP_CONSUMPTIVE_WATER_REQUIREMENT_TS_DAILY) + input);
			}
			if (include_input)
			{
				data_types.Add(StateMod_DataSet.lookupTimeSeriesDataType(StateMod_DataSet.COMP_DIVERSION_RIGHTS) + input);
			}
			if (include_input_estimated && (interval == TimeInterval.DAY))
			{
				data_types.Add(StateMod_DataSet.lookupTimeSeriesDataType(StateMod_DataSet.COMP_DIVERSION_TS_DAILY) + "Estimated" + input);
				data_types.Add(StateMod_DataSet.lookupTimeSeriesDataType(StateMod_DataSet.COMP_DEMAND_TS_DAILY) + "Estimated" + input);
				data_types.Add(StateMod_DataSet.lookupTimeSeriesDataType(StateMod_DataSet.COMP_CONSUMPTIVE_WATER_REQUIREMENT_TS_DAILY) + "Estimate" + input);
			}
			if (include_output)
			{
				data_types = StringUtil.addListToStringList(data_types, StringUtil.toList(diversion_types));
			}
		}
		else if (comp_type == StateMod_DataSet.COMP_RESERVOIR_STATIONS)
		{
			// Include input time series if requested...
			if (include_input && (interval == TimeInterval.MONTH))
			{
				data_types.Add(StateMod_DataSet.lookupTimeSeriesDataType(StateMod_DataSet.COMP_RESERVOIR_CONTENT_TS_MONTHLY) + input);
				data_types.Add(StateMod_DataSet.lookupTimeSeriesDataType(StateMod_DataSet.COMP_RESERVOIR_TARGET_TS_MONTHLY) + "Min" + input);
				data_types.Add(StateMod_DataSet.lookupTimeSeriesDataType(StateMod_DataSet.COMP_RESERVOIR_TARGET_TS_MONTHLY) + "Max" + input);
			}
			else if (include_input && (interval == TimeInterval.DAY))
			{
				data_types.Add(StateMod_DataSet.lookupTimeSeriesDataType(StateMod_DataSet.COMP_RESERVOIR_CONTENT_TS_DAILY) + input);
				data_types.Add(StateMod_DataSet.lookupTimeSeriesDataType(StateMod_DataSet.COMP_RESERVOIR_TARGET_TS_DAILY) + "Min" + input);
				data_types.Add(StateMod_DataSet.lookupTimeSeriesDataType(StateMod_DataSet.COMP_RESERVOIR_TARGET_TS_DAILY) + "Max" + input);
			}
			if (include_input)
			{
				data_types.Add(StateMod_DataSet.lookupTimeSeriesDataType(StateMod_DataSet.COMP_RESERVOIR_RIGHTS) + input);
			}
			// Include estimated input if requested...
			if (include_input_estimated && (interval == TimeInterval.DAY))
			{
				data_types.Add(StateMod_DataSet.lookupTimeSeriesDataType(StateMod_DataSet.COMP_RESERVOIR_CONTENT_TS_DAILY) + "Estimated" + input);
				data_types.Add(StateMod_DataSet.lookupTimeSeriesDataType(StateMod_DataSet.COMP_RESERVOIR_TARGET_TS_DAILY) + "MinEstimated" + input);
				data_types.Add(StateMod_DataSet.lookupTimeSeriesDataType(StateMod_DataSet.COMP_RESERVOIR_TARGET_TS_DAILY) + "MaxEstimated" + input);
			}
			// Include output if requested...
			if (include_output)
			{
				data_types = StringUtil.addListToStringList(data_types, StringUtil.toList(reservoir_types));
			}
		}
		else if (comp_type == StateMod_DataSet.COMP_INSTREAM_STATIONS)
		{
			if (include_input && (interval == TimeInterval.MONTH))
			{
				data_types.Add(StateMod_DataSet.lookupTimeSeriesDataType(StateMod_DataSet.COMP_INSTREAM_DEMAND_TS_MONTHLY) + input);
				data_types.Add(StateMod_DataSet.lookupTimeSeriesDataType(StateMod_DataSet.COMP_INSTREAM_DEMAND_TS_AVERAGE_MONTHLY) + input);
			}
			else if (include_input && (interval == TimeInterval.DAY))
			{
				data_types.Add(StateMod_DataSet.lookupTimeSeriesDataType(StateMod_DataSet.COMP_INSTREAM_DEMAND_TS_DAILY) + input);
			}
			if (include_input)
			{
				data_types.Add(StateMod_DataSet.lookupTimeSeriesDataType(StateMod_DataSet.COMP_INSTREAM_RIGHTS) + input);
			}
			if (include_input_estimated && (interval == TimeInterval.DAY))
			{
				data_types.Add(StateMod_DataSet.lookupTimeSeriesDataType(StateMod_DataSet.COMP_INSTREAM_DEMAND_TS_DAILY) + "Estimated" + input);
			}
			if (include_output)
			{
				data_types = StringUtil.addListToStringList(data_types, StringUtil.toList(instream_types));
			}
		}
		else if (comp_type == StateMod_DataSet.COMP_WELL_STATIONS)
		{
			if (include_input && (interval == TimeInterval.MONTH))
			{
				data_types.Add(StateMod_DataSet.lookupTimeSeriesDataType(StateMod_DataSet.COMP_WELL_PUMPING_TS_MONTHLY) + input);
				data_types.Add(StateMod_DataSet.lookupTimeSeriesDataType(StateMod_DataSet.COMP_WELL_DEMAND_TS_MONTHLY) + input);
			}
			else if (include_input && (interval == TimeInterval.DAY))
			{
				data_types.Add(StateMod_DataSet.lookupTimeSeriesDataType(StateMod_DataSet.COMP_WELL_PUMPING_TS_DAILY) + input);
				data_types.Add(StateMod_DataSet.lookupTimeSeriesDataType(StateMod_DataSet.COMP_WELL_DEMAND_TS_DAILY) + input);
			}
			if (include_input)
			{
				data_types.Add(StateMod_DataSet.lookupTimeSeriesDataType(StateMod_DataSet.COMP_WELL_RIGHTS) + input);
			}
			if (include_input_estimated && (interval == TimeInterval.DAY))
			{
				data_types.Add(StateMod_DataSet.lookupTimeSeriesDataType(StateMod_DataSet.COMP_WELL_PUMPING_TS_DAILY) + "Estimated" + input);
				data_types.Add(StateMod_DataSet.lookupTimeSeriesDataType(StateMod_DataSet.COMP_WELL_DEMAND_TS_DAILY) + "Estimated" + input);
			}
			if (include_output)
			{
				data_types = StringUtil.addListToStringList(data_types, StringUtil.toList(well_types));
			}
		}

		return data_types;
	}

	/// <summary>
	/// Determine the output precision for a list of time series (e.g., for use with the
	/// time series write methods or to display data in a table).  The default is to get
	/// the precision from the units of the first time series.
	/// </summary>
	public static int getTimeSeriesOutputPrecision(IList<TS> tslist)
	{
		int list_size = 0, precision = -2; // Default
		TS tspt = null;
		if ((tslist != null) && (tslist.Count > 0))
		{
			tspt = tslist[0];
			list_size = tslist.Count;
		}
		if (tspt != null)
		{
			string units = tspt.getDataUnits();
			//Message.printStatus ( 2, "", "Data units are " + units );
			DataFormat outputformat = DataUnits.getOutputFormat(units,10);
			if (outputformat != null)
			{
				precision = outputformat.getPrecision();
				if (precision > 0)
				{
					// Change to negative so output code will handle overflow...
					precision *= -1;
				}
			}
			outputformat = null;
			Message.printStatus(2, "", "Precision from units output format *-1 is " + precision);
		}
		// Old code that we still need to support...
		// In year of CRDSS 2, we changed the precision to 0 for RSTO.
		// See if any of the TS in the list are RSTO...
		for (int ilist = 0; ilist < list_size; ilist++)
		{
			tspt = tslist[ilist];
			if (tspt == null)
			{
				continue;
			}
			if (tspt.getIdentifier().getType().equalsIgnoreCase("RSTO"))
			{
				precision = 0;
				break;
			}
		}
		return precision;
	}

	// TODO - might move this to a different class once the network builder falls into place.
	/// <summary>
	/// Determine the nodes that are immediately upstream of a given downstream node. </summary>
	/// <returns> list of StateMod_RiverNetworkNode that are upstream of the node for
	/// the given identifier.  If none are found, an empty non-null Vector is returned. </returns>
	/// <param name="node_Vector"> list of StateMod_RiverNetworkNode. </param>
	/// <param name="downstream_id"> Downstream identifier of interest. </param>
	public static IList<StateMod_RiverNetworkNode> getUpstreamNetworkNodes(IList<StateMod_RiverNetworkNode> node_Vector, string downstream_id)
	{
		string rtn = "StateMod_Util.getUpstreamNetworkNodes";
		if (Message.isDebugOn)
		{
			Message.printDebug(1, rtn, "Trying to find upstream nodes for " + downstream_id);
		}
		IList<StateMod_RiverNetworkNode> v = new List<StateMod_RiverNetworkNode>();
		if (node_Vector == null)
		{
			return v;
		}
		int num = node_Vector.Count;
		StateMod_RiverNetworkNode riv;
		for (int i = 0; i < num; i++)
		{
			riv = node_Vector[i];
			if (riv.getCstadn().Equals(downstream_id, StringComparison.OrdinalIgnoreCase))
			{
				if (Message.isDebugOn)
				{
					Message.printDebug(1, rtn, "Adding upstream node " + riv.getID());
				}
				v.Add(riv);
			}
		}
		return v;
	}

	/// <summary>
	/// Get a list of water right identifiers for a location.  The locations are the
	/// nodes at which the rights apply.  One or more water right can exist with the same identifier. </summary>
	/// <param name="smrights"> List of StateMod_Right to search. </param>
	/// <param name="loc_id"> Location identifier to match (case-insensitive). </param>
	/// <param name="req_parcel_year"> Parcel year for data or -1 to use all (only used with well rights). </param>
	/// <returns> a list of locations for water rights, in the order found in the original list. </returns>
	public static IList<string> getWaterRightIdentifiersForLocation<T1>(IList<T1> smrights, string loc_id, int req_parcel_year) where T1 : StateMod_Right
	{
		IList<string> matchlist = new List<string>(); // Returned data, identifiers (not full right)
		int size = 0;
		if (smrights != null)
		{
			size = smrights.Count;
		}
		StateMod_Right right = null;
		int parcel_year;
		string right_id; // Right identifier
		int matchlist_size = 0;
		bool found = false; // used to indicate matching ID found
		for (int i = 0; i < size; i++)
		{
			right = smrights[i];
			if ((req_parcel_year != -1) && right is StateMod_WellRight)
			{
				// Allow the year to filter.
				parcel_year = ((StateMod_WellRight)right).getParcelYear();
				if (parcel_year != req_parcel_year)
				{
					// No need to process right.
					continue;
				}
			}
			if ((!string.ReferenceEquals(loc_id, null)) && !loc_id.Equals(right.getLocationIdentifier(), StringComparison.OrdinalIgnoreCase))
			{
				// Not a matching location
				continue;
			}
			// If here need to add the identifier if not already in the list...
			right_id = right.getIdentifier();
			found = false;
			for (int j = 0; j < matchlist_size; j++)
			{
				if (right_id.Equals((string)matchlist[j], StringComparison.OrdinalIgnoreCase))
				{
					found = true;
					break;
				}
			}
			if (!found)
			{
				// Add to the list
				matchlist.Add(right_id);
				matchlist_size = matchlist.Count;
			}
		}
		return matchlist;
	}

	/// <summary>
	/// Get a list of water rights for a location.  The locations are the nodes at which the rights apply. </summary>
	/// <param name="smrights"> List of StateMod_Right to search. </param>
	/// <param name="loc_id"> Location identifier to match (case-insensitive).  If null, return all water rights for the
	/// requested parcel year. </param>
	/// <param name="req_parcel_year"> Parcel year for data or -1 to use all (only used with well rights). </param>
	/// <returns> a list of locations for water rights, in the order found in the original list. </returns>
	public static IList<StateMod_Right> getWaterRightsForLocation<T1>(IList<T1> smrights, string loc_id, int req_parcel_year) where T1 : StateMod_Right
	{
		IList<StateMod_Right> matchlist = new List<StateMod_Right>(); // Returned data
		int size = 0;
		if (smrights != null)
		{
			size = smrights.Count;
		}
		StateMod_Right right = null;
		int parcel_year;
		for (int i = 0; i < size; i++)
		{
			right = smrights[i];
			if ((req_parcel_year != -1) && right is StateMod_WellRight)
			{
				// Allow the year to filter.
				parcel_year = ((StateMod_WellRight)right).getParcelYear();
				if (parcel_year != req_parcel_year)
				{
					// No need to process right.
					continue;
				}
			}
			if ((string.ReferenceEquals(loc_id, null)) || loc_id.Equals(right.getLocationIdentifier(), StringComparison.OrdinalIgnoreCase))
			{
				matchlist.Add(right);
			}
		}
		return matchlist;
	}

	/// <summary>
	/// Get a list of water rights for a location matching a right identifier.  The locations are the
	/// nodes at which the rights apply. </summary>
	/// <param name="smrights"> List of StateMod_Right to search. </param>
	/// <param name="loc_id"> Location identifier to match (case-insensitive). </param>
	/// <param name="right_id"> Right identifier to match (case-insensitive). </param>
	/// <param name="req_parcel_year"> Parcel year for data or -1 to use all (only used with well rights). </param>
	/// <returns> a list of locations for water rights, in the order found in the original list. </returns>
	public static IList<StateMod_Right> getWaterRightsForLocationAndRightIdentifier<T1>(IList<T1> smrights, string loc_id, string right_id, int req_parcel_year) where T1 : StateMod_Right
	{
		IList<StateMod_Right> matchlist = new List<StateMod_Right>(); // Returned data
		int size = 0;
		if (smrights != null)
		{
			size = smrights.Count;
		}
		StateMod_Right right = null;
		int parcel_year;
		for (int i = 0; i < size; i++)
		{
			right = smrights[i];
			if ((req_parcel_year != -1) && right is StateMod_WellRight)
			{
				// Allow the year to filter.
				parcel_year = ((StateMod_WellRight)right).getParcelYear();
				if (parcel_year != req_parcel_year)
				{
					// No need to process right.
					continue;
				}
			}
			if ((!string.ReferenceEquals(loc_id, null)) && !loc_id.Equals(right.getLocationIdentifier(), StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}
			if ((!string.ReferenceEquals(right_id, null)) && !right_id.Equals(right.getIdentifier(), StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}
			// If here it is a match...
			matchlist.Add(right);
		}
		return matchlist;
	}

	/// <summary>
	/// Get a list of water rights for a parcel. </summary>
	/// <param name="smrights"> List of StateMod_WellRight to search. </param>
	/// <param name="parcel_id"> Parcel identifier to match (case-insensitive). </param>
	/// <param name="req_parcel_year"> Parcel year for data or -1 to use all. </param>
	/// <returns> a list of water rights for the parcel, in the order found in the original list. </returns>
	public static IList<StateMod_Right> getWaterRightsForParcel<T1>(IList<T1> smrights, string parcel_id, int req_parcel_year) where T1 : StateMod_Right
	{
		IList<StateMod_Right> matchlist = new List<StateMod_Right>(); // Returned data
		int size = 0;
		if (smrights != null)
		{
			size = smrights.Count;
		}
		StateMod_Right right = null;
		StateMod_WellRight wellright = null;
		for (int i = 0; i < size; i++)
		{
			right = smrights[i];
			if (right is StateMod_WellRight)
			{
				wellright = (StateMod_WellRight)right;
				if ((req_parcel_year != -1) && (wellright.getParcelYear() != req_parcel_year))
				{
					// No need to process right.
					continue;
				}
				if (parcel_id.Equals(wellright.getParcelID(), StringComparison.OrdinalIgnoreCase))
				{
					matchlist.Add(wellright);
				}
			}
		}
		return matchlist;
	}

	/// <summary>
	/// Get a list of locations from a list of water rights.  The locations are the
	/// nodes at which the rights apply. </summary>
	/// <param name="smrights"> list of StateMod_Right to search. </param>
	/// <param name="req_parcel_year"> Specific parcel year to match, or -1 to match all, if input is a
	/// list of StateMod_WellRight. </param>
	/// <returns> a list of locations for water rights, in the order found in the original list. </returns>
	public static IList<string> getWaterRightLocationList<T1>(IList<T1> smrights, int req_parcel_year) where T1 : StateMod_Right
	{
		IList<string> loclist = new List<string>(); // Returned data
		int size = 0;
		if (smrights != null)
		{
			size = smrights.Count;
		}
		StateMod_Right right = null;
		int size_loc = 0; // size of location list
		bool found = false; // Indicate whether the location has been found.
		string right_loc_id = null; // ID for location
		int parcel_year = 0; // Parcel year to process.
		int j = 0; // Loop index for found locations.
		for (int i = 0; i < size; i++)
		{
			right = smrights[i];
			if (req_parcel_year != -1)
			{
				// Check the parcel year and skip if necessary.
				if (right is StateMod_WellRight)
				{
					parcel_year = ((StateMod_WellRight)right).getParcelYear();
					if (parcel_year != req_parcel_year)
					{
						// No need to consider the right.
						continue;
					}
				}
			}
			right_loc_id = right.getLocationIdentifier();
			// Search the list to see if it is a new item...
			found = false;
			for (j = 0; j < size_loc; j++)
			{
				if (right_loc_id.Equals(loclist[j], StringComparison.OrdinalIgnoreCase))
				{
					found = true;
					break;
				}
			}
			if (!found)
			{
				// Add to the list
				loclist.Add(right_loc_id);
				size_loc = loclist.Count;
			}
		}
		return loclist;
	}

	/// <summary>
	/// Get a list of parcels from a list of well water rights.  The parcels are the
	/// locations at which well rights have been matched. </summary>
	/// <param name="smrights"> a list of StateMod_WellRight to process. </param>
	/// <param name="req_parcel_year"> a requested year to constrain the parcel list (or -1 to return all). </param>
	/// <returns> a list of parcels for water rights, in the order found in the original list. </returns>
	public static IList<string> getWaterRightParcelList(IList<StateMod_Right> smrights, int req_parcel_year)
	{
		IList<string> loclist = new List<string>(); // Returned data
		int size = 0;
		if (smrights != null)
		{
			size = smrights.Count;
		}
		StateMod_Right right = null;
		StateMod_WellRight wellright = null;
		int size_loc = 0; // size of location list
		bool found = false; // Indicate whether the location has been found.
		string parcel_id = null; // ID for location
		int parcel_year = 0; // Year for parcels.
		int j = 0; // Loop index for found locations.
		for (int i = 0; i < size; i++)
		{
			right = smrights[i];
			if (right is StateMod_WellRight)
			{
				wellright = (StateMod_WellRight)right;
				parcel_id = wellright.getParcelID();
				parcel_year = wellright.getParcelYear();
				if ((req_parcel_year != -1) && (parcel_year != req_parcel_year))
				{
					// No need to process right
					continue;
				}
				// Search the list to see if it is a new item...
				found = false;
				for (j = 0; j < size_loc; j++)
				{
					if (parcel_id.Equals(loclist[j], StringComparison.OrdinalIgnoreCase))
					{
						found = true;
						break;
					}
				}
				if (!found)
				{
					// Add to the list
					loclist.Add(parcel_id);
					size_loc = loclist.Count;
				}
			}
		}
		return loclist;
	}

	/// <summary>
	/// Get a list of parcel years from a list of well water rights. </summary>
	/// <param name="smrights"> a list of StateMod_WellRight to process. </param>
	/// <returns> a list of parcel years for water rights, in ascending order. </returns>
	public static int [] getWaterRightParcelYearList(System.Collections.IList smrights)
	{
		System.Collections.IList yearlist = new List<object>(); // Returned data
		int size = 0;
		if (smrights != null)
		{
			size = smrights.Count;
		}
		StateMod_WellRight right = null;
		int size_years = 0; // size of location list
		bool found = false; // Indicate whether the year has been found.
		int parcel_year = 0; // Year for parcels.
		int j = 0; // Loop index for found years.
		for (int i = 0; i < size; i++)
		{
			right = (StateMod_WellRight)smrights[i];
			parcel_year = right.getParcelYear();
			// Search the list to see if it is a new item...
			found = false;
			for (j = 0; j < size_years; j++)
			{
				if (parcel_year == ((int?)yearlist[j]).Value)
				{
					found = true;
					break;
				}
			}
			if (!found)
			{
				// Add to the list
				yearlist.Add(new int?(parcel_year));
				size_years = yearlist.Count;
			}
		}
		int[] parcel_years = new int[yearlist.Count];
		for (int i = 0; i < size_years; i++)
		{
			parcel_years[i] = ((int?)yearlist[i]).Value;
		}
		// Sort the array...
		MathUtil.sort(parcel_years, MathUtil.SORT_QUICK, MathUtil.SORT_ASCENDING, null, false);
		return parcel_years;
	}

	/// <summary>
	/// Works in coordination with getDailyTimeSeries - make sure to keep in sync </summary>
	/// <returns> whether daily time series is available or not, based on rules for getting daily TS
	/// dailyID = dayTS identifier, return true if daily ts exists
	/// dailyID = 0, return true if month ts exists
	/// else return true if both monthly and daily ts exist </returns>
	/// <param name="dailyID"> daily id to use in comparison as described above </param>
	/// <param name="calculate"> only used when daily ID != dayTS identifier;  
	/// if true, returns true if monthly and daily exist
	/// if false, returns true if daily exists </param>
	public static bool isDailyTimeSeriesAvailable(string ID, string dailyID, MonthTS monthTS, DayTS dayTS, bool calculate)
	{
		if (Message.isDebugOn)
		{
			Message.printDebug(30, "StateMod_GUIUtil.isDailyTimeSeriesAvailable", "ID: " + ID + ", dailyID: " + dailyID + ", monthTS: " + (monthTS != null) + ", dayTS: " + (dayTS != null) + ", calculate: " + calculate);
		}

		// if dailyID is 0
		if (dailyID.Equals("0", StringComparison.OrdinalIgnoreCase))
		{
			if (monthTS == null)
			{
				return false;
			}
			else
			{
				return true;
			}
		}
		// if dailyID = ID identifier
		else if (dailyID.Equals(ID, StringComparison.OrdinalIgnoreCase))
		{
			if (dayTS == null)
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		// if dailyID != ID and pattern is desired
		else if (!calculate)
		{
			if (dayTS == null)
			{
				return false;
			}
			else
			{
				return true;
			}
		}
		// if dailyID != dayTS identifier and calculation is desired
		else
		{
			if (dayTS == null)
			{
				return false;
			}
			if (monthTS == null)
			{
				return false;
			}
			return true;
		}
	}

	/// <summary>
	/// Indicate whether the StateMod version is at least some standard value.  This is
	/// useful when checking binary formats against a recognized version. </summary>
	/// <returns> true if the version is >= the known version that is being checked.  Return false
	/// if the version is null or empty. </returns>
	/// <param name="version"> A version to check. </param>
	/// <param name="knownVersion"> A known version to check against (see VERSION_*). </param>
	public static bool isVersionAtLeast(string version, string knownVersion)
	{
		if ((string.ReferenceEquals(version, null)) || version.Equals(""))
		{
			return false;
		}
		if (version.CompareTo(knownVersion) >= 0)
		{
			if (Message.isDebugOn)
			{
				Message.printDebug(1,"isVersionAtLeast","Checking version \"" + version + "\" against known version \"" + knownVersion + "\" - returning true because it is at least " + knownVersion);
			}
			return true;
		}
		else
		{
			if (Message.isDebugOn)
			{
				Message.printStatus(1,"isVersionAtLeast","Checking version \"" + version + "\" against known version \"" + knownVersion + "\" - returning false because it is not at least " + knownVersion);
			}
			return false;
		}
	}

	/// <summary>
	/// Find the position of a StateMod_Data object in the data Vector, using the
	/// identifier.  The position for the first match is returned. </summary>
	/// <returns> the position, or -1 if not found. </returns>
	/// <param name="id"> StateMod_Data identifier. </param>
	public static int indexOf(System.Collections.IList data, string id)
	{
		int size = 0;
		if (string.ReferenceEquals(id, null))
		{
			return -1;
		}
		if (data != null)
		{
			size = data.Count;
		}
		StateMod_Data d = null;
		for (int i = 0; i < size; i++)
		{
			d = (StateMod_Data)data[i];
			if (id.Equals(d._id, StringComparison.OrdinalIgnoreCase))
			{
				return i;
			}
		}
		return -1;
	}

	/// <summary>
	/// Find the position of a StateMod_Data object in the data Vector, using the name.
	/// The position for the first match is returned. </summary>
	/// <returns> the position, or -1 if not found. </returns>
	/// <param name="name"> StateMod_Data name. </param>
	public static int indexOfName(System.Collections.IList data, string name)
	{
		int size = 0;
		if (data != null)
		{
			size = data.Count;
		}
		StateMod_Data d = null;
		for (int i = 0; i < size; i++)
		{
			d = (StateMod_Data)data[i];
			if (name.Equals(d._name, StringComparison.OrdinalIgnoreCase))
			{
				return i;
			}
		}
		return -1;
	}

	/// <summary>
	/// Find the position of a StateMod_Data object in the data Vector, using the
	/// river node identifier.  The position for the first match is returned.
	/// This method can only be used for station data objects that have a river node identifier. </summary>
	/// <returns> the position, or -1 if not found. </returns>
	/// <param name="id"> StateMod_Data identifier. </param>
	public static int indexOfRiverNodeID(System.Collections.IList data, string id)
	{
		int size = 0;
		if (data != null)
		{
			size = data.Count;
		}
		StateMod_Data d = null;
		for (int i = 0; i < size; i++)
		{
			// Stream gage and stream estimate have their own CGOTO
			// data members.  All other use the data in the StateMod_Data base class.
			d = (StateMod_Data)data[i];
			if (d is StateMod_StreamGage)
			{
				if (id.Equals(((StateMod_StreamGage)d).getCgoto(), StringComparison.OrdinalIgnoreCase))
				{
					return i;
				}
			}
			else if (d is StateMod_StreamEstimate)
			{
				if (id.Equals(((StateMod_StreamEstimate)d).getCgoto(), StringComparison.OrdinalIgnoreCase))
				{
					return i;
				}
			}
			else
			{
				if (id.Equals(((StateMod_Data)d).getCgoto(), StringComparison.OrdinalIgnoreCase))
				{
					return i;
				}
			}
		}
		return -1;
	}

	/// <summary>
	/// Determine whether a date value is missing. </summary>
	/// <param name="value"> the date to be checked </param>
	/// <returns> true if the date is missing, false if not </returns>
	public static bool isMissing(DateTime value)
	{
		if (value == MISSING_DATE)
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Determine whether a double value is missing. </summary>
	/// <param name="d"> Double precision value to check. </param>
	/// <returns> true if the value is missing, false, if not. </returns>
	public static bool isMissing(double d)
	{
		if ((d < MISSING_DOUBLE_CEILING) && (d > MISSING_DOUBLE_FLOOR))
		{
			return true;
		}
		else if (double.IsNaN(d))
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Determine whether a float value is missing. </summary>
	/// <param name="f"> Float precision value to check. </param>
	/// <returns> true if the value is missing, false, if not. </returns>
	public static bool isMissing(float f)
	{
		if ((f < (float)MISSING_DOUBLE_CEILING) && (f > (float)MISSING_DOUBLE_FLOOR))
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Determine whether an integer value is missing. </summary>
	/// <param name="i"> Integer value to check. </param>
	/// <returns> true if the value is missing, false, if not. </returns>
	public static bool isMissing(int i)
	{
		if (i == MISSING_INT)
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Determine whether a long integer value is missing. </summary>
	/// <param name="i"> Integer value to check. </param>
	/// <returns> true if the value is missing, false, if not. </returns>
	public static bool isMissing(long i)
	{
		if (i == MISSING_INT)
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Determine whether if a String value is missing. </summary>
	/// <param name="s"> String value to check. </param>
	/// <returns> true if the value is missing (null or empty), false, if not. </returns>
	public static bool isMissing(string s)
	{
		if ((string.ReferenceEquals(s, null)) || (s.Length == 0))
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Locates the index of an StateMod_Data node.  The node can be a diversion, 
	/// reservoir, or any other StateMod object which has been derived from the StateMod_Data type
	/// from the specified CGoto. </summary>
	/// <param name="ID"> CGoto ID to search for </param>
	/// <param name="theData"> vector of StateMod_Data objects </param>
	public static int locateIndexFromCGOTO(string ID, System.Collections.IList theData)
	{
		int num = 0;
		if (theData != null)
		{
			num = theData.Count;
		}

		for (int i = 0; i < num; i++)
		{
			if (ID.Equals(((StateMod_Data)theData[i]).getCgoto(), StringComparison.OrdinalIgnoreCase))
			{
				return i;
			}
		}
		return StateMod_Data.MISSING_INT;
	}

	/// <summary>
	/// Locates the index of a data object derived from StateMod_Data in a Vector. </summary>
	/// <param name="ID"> ID to search for </param>
	/// <param name="theData"> vector of StateMod_Data objects </param>
	/// <returns> index or -999 when not found </returns>
	public static int locateIndexFromID(string ID, System.Collections.IList theData)
	{
		int num = 0;
		if (theData != null)
		{
			num = theData.Count;
		}

		for (int i = 0; i < num; i++)
		{
			if (ID.Equals(((StateMod_Data)theData[i]).getID(), StringComparison.OrdinalIgnoreCase))
			{
				return i;
			}
		}
		return StateMod_Data.MISSING_INT;
	}

	/// <summary>
	/// Returns the property value for a component. </summary>
	/// <param name="componentType"> the kind of component to look up for. </param>
	/// <param name="propType"> the property to look up.  One of "FieldName", "FieldNameHeader", "ToolTip", or "Format". </param>
	/// <param name="field"> the field for which to return the property. </param>
	/// <returns> the property, or if it could not be found null will be returned. </returns>
	public static string lookupPropValue(int componentType, string propType, string field)
	{
		if (componentType == StateMod_DataSet.COMP_DELAY_TABLES_DAILY)
		{
			return lookupDelayTableDailyPropValue(propType, field);
		}
		else if (componentType == StateMod_DataSet.COMP_DELAY_TABLES_MONTHLY)
		{
			return lookupDelayTableMonthlyPropValue(propType, field);
		}
		else if (componentType == StateMod_DataSet.COMP_DIVERSION_RIGHTS)
		{
			return lookupDiversionRightPropValue(propType, field);
		}
		else if (componentType == StateMod_DataSet.COMP_DIVERSION_STATIONS)
		{
			return lookupDiversionPropValue(propType, field);
		}
		else if (componentType == StateMod_DataSet.COMP_DIVERSION_STATION_COLLECTIONS)
		{
			   return lookupDiversionCollectionPropValue(propType, field);
		}
		else if (componentType == StateMod_DataSet.COMP_DIVERSION_STATION_DELAY_TABLES)
		{
			   return lookupDiversionReturnFlowPropValue(propType, field);
		}
		else if (componentType == StateMod_DataSet.COMP_INSTREAM_RIGHTS)
		{
			return lookupInstreamFlowRightPropValue(propType, field);
		}
		else if (componentType == StateMod_DataSet.COMP_INSTREAM_STATIONS)
		{
			return lookupInstreamFlowPropValue(propType, field);
		}
		else if (componentType == StateMod_DataSet.COMP_RESERVOIR_STATIONS)
		{
			return lookupReservoirPropValue(propType, field);
		}
		else if (componentType == StateMod_DataSet.COMP_RESERVOIR_STATION_ACCOUNTS)
		{
			return lookupReservoirAccountPropValue(propType, field);
		}
		else if (componentType == StateMod_Util.COMP_RESERVOIR_AREA_CAP)
		{
			return lookupReservoirAreaCapPropValue(propType, field);
		}
		else if (componentType == StateMod_DataSet.COMP_RESERVOIR_STATION_PRECIP_STATIONS)
		{
			return lookupReservoirPrecipStationPropValue(propType, field);
		}
		else if (componentType == StateMod_DataSet.COMP_RESERVOIR_STATION_EVAP_STATIONS)
		{
			return lookupReservoirEvapStationPropValue(propType, field);
		}
		else if (componentType == StateMod_DataSet.COMP_RESERVOIR_STATION_COLLECTIONS)
		{
			   return lookupReservoirCollectionPropValue(propType, field);
		}
		else if (componentType == StateMod_DataSet.COMP_RESERVOIR_RIGHTS)
		{
			return lookupReservoirRightPropValue(propType, field);
		}
		else if (componentType == StateMod_DataSet.COMP_RIVER_NETWORK)
		{
			return lookupRiverNodePropValue(propType, field);
		}
		else if (componentType == StateMod_DataSet.COMP_STREAMESTIMATE_STATIONS)
		{
			   return lookupStreamEstimatePropValue(propType, field);
		}
		else if (componentType == StateMod_DataSet.COMP_STREAMESTIMATE_COEFFICIENTS)
		{
			   return lookupStreamEstimateCoefficientPropValue(propType,field);
		}
		else if (componentType == StateMod_DataSet.COMP_STREAMGAGE_STATIONS)
		{
			return lookupStreamGagePropValue(propType, field);
		}
		else if (componentType == StateMod_DataSet.COMP_WELL_STATIONS)
		{
			return lookupWellPropValue(propType, field);
		}
		else if (componentType == StateMod_DataSet.COMP_WELL_STATION_COLLECTIONS)
		{
			   return lookupWellCollectionPropValue(propType, field);
		}
		else if (componentType == StateMod_DataSet.COMP_WELL_STATION_DEPLETION_TABLES)
		{
			return lookupWellDepletionPropValue(propType, field);
		}
		else if (componentType == StateMod_DataSet.COMP_WELL_STATION_DELAY_TABLES)
		{
			   return lookupWellReturnFlowPropValue(propType, field);
		}
		else if (componentType == StateMod_DataSet.COMP_WELL_RIGHTS)
		{
			return lookupWellRightPropValue(propType, field);
		}

		return null;
	}

	/// <summary>
	/// Returns property values for daily delay tables. </summary>
	/// <param name="propType"> the property to look up.  One of "FieldName", "FieldNameHeader", "ToolTip", or "Format". </param>
	/// <param name="field"> the field for which to return the property. </param>
	/// <returns> the property, or if it could not be found null will be returned. </returns>
	private static string lookupDelayTableDailyPropValue(string propType, string field)
	{
		if (propType.Equals("FieldName", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("DelayTableID", StringComparison.OrdinalIgnoreCase))
			{
				return "DELAY TABLE ID";
			}
			else if (field.Equals("Date", StringComparison.OrdinalIgnoreCase))
			{
				return "DAY";
			}
			else if (field.Equals("ReturnAmount", StringComparison.OrdinalIgnoreCase))
			{
				return "PERCENT";
			}
		}
		else if (propType.Equals("FieldNameHeader", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("DelayTableID", StringComparison.OrdinalIgnoreCase))
			{
				return "DELAY\nTABLE ID";
			}
			else if (field.Equals("Date", StringComparison.OrdinalIgnoreCase))
			{
				return "\nDAY";
			}
			else if (field.Equals("ReturnAmount", StringComparison.OrdinalIgnoreCase))
			{
				return "\nPERCENT";
			}
		}
		else if (propType.Equals("Format", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("DelayTableID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
			else if (field.Equals("Date", StringComparison.OrdinalIgnoreCase))
			{
				return "%8d";
			}
			else if (field.Equals("ReturnAmount", StringComparison.OrdinalIgnoreCase))
			{
				return "%12.6f";
			}
		}
		else if (propType.Equals("ToolTip", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("DelayTableID", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("Date", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("ReturnAmount", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
		}

		return null;
	}

	/// <summary>
	/// Returns property values for monthly delay tables. </summary>
	/// <param name="propType"> the property to look up.  One of "FieldName", 
	/// "FieldNameHeader", "ToolTip", or "Format". </param>
	/// <param name="field"> the field for which to return the property. </param>
	/// <returns> the property, or if it could not be found null will be returned. </returns>
	private static string lookupDelayTableMonthlyPropValue(string propType, string field)
	{
		if (propType.Equals("FieldName", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("DelayTableID", StringComparison.OrdinalIgnoreCase))
			{
				return "DELAY TABLE ID";
			}
			else if (field.Equals("Date", StringComparison.OrdinalIgnoreCase))
			{
				return "MONTH";
			}
			else if (field.Equals("ReturnAmount", StringComparison.OrdinalIgnoreCase))
			{
				return "PERCENT";
			}
		}
		else if (propType.Equals("FieldNameHeader", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("DelayTableID", StringComparison.OrdinalIgnoreCase))
			{
				return "DELAY\nTABLE ID";
			}
			else if (field.Equals("Date", StringComparison.OrdinalIgnoreCase))
			{
				return "\nMONTH";
			}
			else if (field.Equals("ReturnAmount", StringComparison.OrdinalIgnoreCase))
			{
				return "\nPERCENT";
			}
		}
		else if (propType.Equals("Format", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("DelayTableID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
			else if (field.Equals("Date", StringComparison.OrdinalIgnoreCase))
			{
				return "%8d";
			}
			else if (field.Equals("ReturnAmount", StringComparison.OrdinalIgnoreCase))
			{
				return "%12.6f";
			}
		}
		else if (propType.Equals("ToolTip", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("DelayTableID", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("Date", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("ReturnAmount", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
		}

		return null;
	}

	/// <summary>
	/// Returns property values for diversions </summary>
	/// <param name="propType"> the property to look up.  One of "FieldName", 
	/// "FieldNameHeader", "ToolTip", or "Format". </param>
	/// <param name="field"> the field for which to return the property. </param>
	/// <returns> the property, or if it could not be found null will be returned. </returns>
	private static string lookupDiversionPropValue(string propType, string field)
	{
		if (propType.Equals("FieldName", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "ID";
			}
			else if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "NAME";
			}
			else if (field.Equals("RiverNodeID", StringComparison.OrdinalIgnoreCase))
			{
				return "RIVER NODE ID";
			}
			else if (field.Equals("OnOff", StringComparison.OrdinalIgnoreCase))
			{
				return "ON/OFF SWITCH";
			}
			else if (field.Equals("Capacity", StringComparison.OrdinalIgnoreCase))
			{
				return "CAPACITY (CFS)";
			}
			else if (field.Equals("ReplaceResOption", StringComparison.OrdinalIgnoreCase))
			{
				return "REPLACE. RES. OPTION";
			}
			else if (field.Equals("DailyID", StringComparison.OrdinalIgnoreCase))
			{
				return "DAILY ID";
			}
			else if (field.Equals("UserName", StringComparison.OrdinalIgnoreCase))
			{
				return "USER NAME";
			}
			else if (field.Equals("DemandType", StringComparison.OrdinalIgnoreCase))
			{
				return "DEMAND TYPE";
			}
			else if (field.Equals("IrrigatedAcres", StringComparison.OrdinalIgnoreCase))
			{
				return "AREA (ACRE)";
			}
			else if (field.Equals("UseType", StringComparison.OrdinalIgnoreCase))
			{
				return "USE TYPE";
			}
			else if (field.Equals("DemandSource", StringComparison.OrdinalIgnoreCase))
			{
				return "DEMAND SOURCE";
			}
			else if (field.Equals("EffAnnual", StringComparison.OrdinalIgnoreCase))
			{
				return "EFFICIENCY ANNUAL (%)";
			}
			else if (field.Equals("EffMonthly01", StringComparison.OrdinalIgnoreCase))
			{
				return "EFFICIENCY MONTH 1";
			}
			else if (field.Equals("EffMonthly02", StringComparison.OrdinalIgnoreCase))
			{
				return "EFFICIENCY MONTH 2";
			}
			else if (field.Equals("EffMonthly03", StringComparison.OrdinalIgnoreCase))
			{
				return "EFFICIENCY MONTH 3";
			}
			else if (field.Equals("EffMonthly04", StringComparison.OrdinalIgnoreCase))
			{
				return "EFFICIENCY MONTH 4";
			}
			else if (field.Equals("EffMonthly05", StringComparison.OrdinalIgnoreCase))
			{
				return "EFFICIENCY MONTH 5";
			}
			else if (field.Equals("EffMonthly06", StringComparison.OrdinalIgnoreCase))
			{
				return "EFFICIENCY MONTH 6";
			}
			else if (field.Equals("EffMonthly07", StringComparison.OrdinalIgnoreCase))
			{
				return "EFFICIENCY MONTH 7";
			}
			else if (field.Equals("EffMonthly08", StringComparison.OrdinalIgnoreCase))
			{
				return "EFFICIENCY MONTH 8";
			}
			else if (field.Equals("EffMonthly09", StringComparison.OrdinalIgnoreCase))
			{
				return "EFFICIENCY MONTH 9";
			}
			else if (field.Equals("EffMonthly10", StringComparison.OrdinalIgnoreCase))
			{
				return "EFFICIENCY MONTH 10";
			}
			else if (field.Equals("EffMonthly11", StringComparison.OrdinalIgnoreCase))
			{
				return "EFFICIENCY MONTH 11";
			}
			else if (field.Equals("EffMonthly12", StringComparison.OrdinalIgnoreCase))
			{
				return "EFFICIENCY MONTH 12";
			}
		}
		else if (propType.Equals("FieldNameHeader", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "\nID";
			}
			else if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "\nNAME";
			}
			else if (field.Equals("RiverNodeID", StringComparison.OrdinalIgnoreCase))
			{
				return "RIVER\nNODE ID";
			}
			else if (field.Equals("OnOff", StringComparison.OrdinalIgnoreCase))
			{
				return "ON/OFF\nSWITCH";
			}
			else if (field.Equals("Capacity", StringComparison.OrdinalIgnoreCase))
			{
				return "CAPACITY\n(CFS)";
			}
			else if (field.Equals("ReplaceResOption", StringComparison.OrdinalIgnoreCase))
			{
				return "REPLACE.\nRES. OPTION";
			}
			else if (field.Equals("DailyID", StringComparison.OrdinalIgnoreCase))
			{
				return "DAILY\nID";
			}
			else if (field.Equals("UserName", StringComparison.OrdinalIgnoreCase))
			{
				return "USER\nNAME";
			}
			else if (field.Equals("DemandType", StringComparison.OrdinalIgnoreCase))
			{
				return "DEMAND\nTYPE";
			}
			else if (field.Equals("IrrigatedAcres", StringComparison.OrdinalIgnoreCase))
			{
				return "AREA\n(ACRE)";
			}
			else if (field.Equals("UseType", StringComparison.OrdinalIgnoreCase))
			{
				return "USE\nTYPE";
			}
			else if (field.Equals("DemandSource", StringComparison.OrdinalIgnoreCase))
			{
				return "DEMAND\nSOURCE";
			}
			else if (field.Equals("EffAnnual", StringComparison.OrdinalIgnoreCase))
			{
				return "EFFICIENCY\nANNUAL (%)";
			}
			else if (field.Equals("EffMonthly01", StringComparison.OrdinalIgnoreCase))
			{
				return "EFFICIENCY\nMONTH 1";
			}
			else if (field.Equals("EffMonthly02", StringComparison.OrdinalIgnoreCase))
			{
				return "EFFICIENCY\nMONTH 2";
			}
			else if (field.Equals("EffMonthly03", StringComparison.OrdinalIgnoreCase))
			{
				return "EFFICIENCY\nMONTH 3";
			}
			else if (field.Equals("EffMonthly04", StringComparison.OrdinalIgnoreCase))
			{
				return "EFFICIENCY\nMONTH 4";
			}
			else if (field.Equals("EffMonthly05", StringComparison.OrdinalIgnoreCase))
			{
				return "EFFICIENCY\nMONTH 5";
			}
			else if (field.Equals("EffMonthly06", StringComparison.OrdinalIgnoreCase))
			{
				return "EFFICIENCY\nMONTH 6";
			}
			else if (field.Equals("EffMonthly07", StringComparison.OrdinalIgnoreCase))
			{
				return "EFFICIENCY\nMONTH 7";
			}
			else if (field.Equals("EffMonthly08", StringComparison.OrdinalIgnoreCase))
			{
				return "EFFICIENCY\nMONTH 8";
			}
			else if (field.Equals("EffMonthly09", StringComparison.OrdinalIgnoreCase))
			{
				return "EFFICIENCY\nMONTH 9";
			}
			else if (field.Equals("EffMonthly10", StringComparison.OrdinalIgnoreCase))
			{
				return "EFFICIENCY\nMONTH 10";
			}
			else if (field.Equals("EffMonthly11", StringComparison.OrdinalIgnoreCase))
			{
				return "EFFICIENCY\nMONTH 11";
			}
			else if (field.Equals("EffMonthly12", StringComparison.OrdinalIgnoreCase))
			{
				return "EFFICIENCY\nMONTH 12";
			}
		}
		else if (propType.Equals("Format", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12.s";
			}
			else if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "%-24.24s";
			}
			else if (field.Equals("RiverNodeID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
			else if (field.Equals("OnOff", StringComparison.OrdinalIgnoreCase))
			{
				return "%8d";
			}
			else if (field.Equals("Capacity", StringComparison.OrdinalIgnoreCase))
			{
				return "%8.2f";
			}
			else if (field.Equals("ReplaceResOption", StringComparison.OrdinalIgnoreCase))
			{
				return "%8d";
			}
			else if (field.Equals("DailyID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
			else if (field.Equals("UserName", StringComparison.OrdinalIgnoreCase))
			{
				return "%-24.24s";
			}
			else if (field.Equals("DemandType", StringComparison.OrdinalIgnoreCase))
			{
				return "%8d";
			}
			else if (field.Equals("IrrigatedAcres", StringComparison.OrdinalIgnoreCase))
			{
				return "%8.2f";
			}
			else if (field.Equals("UseType", StringComparison.OrdinalIgnoreCase))
			{
				return "%8d";
			}
			else if (field.Equals("DemandSource", StringComparison.OrdinalIgnoreCase))
			{
				return "%8d";
			}
			else if (field.Equals("EffAnnual", StringComparison.OrdinalIgnoreCase))
			{
				return "%8.1f";
			}
			else if (field.Equals("EffMonthly01", StringComparison.OrdinalIgnoreCase))
			{
				return "%10.2f";
			}
			else if (field.Equals("EffMonthly02", StringComparison.OrdinalIgnoreCase))
			{
				return "%10.2f";
			}
			else if (field.Equals("EffMonthly03", StringComparison.OrdinalIgnoreCase))
			{
				return "%10.2f";
			}
			else if (field.Equals("EffMonthly04", StringComparison.OrdinalIgnoreCase))
			{
				return "%10.2f";
			}
			else if (field.Equals("EffMonthly05", StringComparison.OrdinalIgnoreCase))
			{
				return "%10.2f";
			}
			else if (field.Equals("EffMonthly06", StringComparison.OrdinalIgnoreCase))
			{
				return "%10.2f";
			}
			else if (field.Equals("EffMonthly07", StringComparison.OrdinalIgnoreCase))
			{
				return "%10.2f";
			}
			else if (field.Equals("EffMonthly08", StringComparison.OrdinalIgnoreCase))
			{
				return "%10.2f";
			}
			else if (field.Equals("EffMonthly09", StringComparison.OrdinalIgnoreCase))
			{
				return "%10.2f";
			}
			else if (field.Equals("EffMonthly10", StringComparison.OrdinalIgnoreCase))
			{
				return "%10.2f";
			}
			else if (field.Equals("EffMonthly11", StringComparison.OrdinalIgnoreCase))
			{
				return "%10.2f";
			}
			else if (field.Equals("EffMonthly12", StringComparison.OrdinalIgnoreCase))
			{
				return "%10.2f";
			}
		}
		else if (propType.Equals("ToolTip", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "<html>The diversion station identifier is the main link between diversion data<BR>"
					+ "and must be unique in the data set.</html>";
			}
			else if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "Diversion station name.";
			}
			else if (field.Equals("RiverNodeID", StringComparison.OrdinalIgnoreCase))
			{
				return "River node where diversion station is located.";
			}
			else if (field.Equals("OnOff", StringComparison.OrdinalIgnoreCase))
			{
				return "Indicates whether diversion station is on (1) or off (0)";
			}
			else if (field.Equals("Capacity", StringComparison.OrdinalIgnoreCase))
			{
				return "Diversion station capacity (CFS)";
			}
			else if (field.Equals("ReplaceResOption", StringComparison.OrdinalIgnoreCase))
			{
				return "Replacement reservoir option.";
			}
			else if (field.Equals("DailyID", StringComparison.OrdinalIgnoreCase))
			{
				return "Daily identifier (for daily time series).";
			}
			else if (field.Equals("UserName", StringComparison.OrdinalIgnoreCase))
			{
				return "User name.";
			}
			else if (field.Equals("DemandType", StringComparison.OrdinalIgnoreCase))
			{
				return "(Monthly) demand type.";
			}
			else if (field.Equals("IrrigatedAcres", StringComparison.OrdinalIgnoreCase))
			{
				return "Irrigated area (ACRE).";
			}
			else if (field.Equals("UseType", StringComparison.OrdinalIgnoreCase))
			{
				return "Use type.";
			}
			else if (field.Equals("DemandSource", StringComparison.OrdinalIgnoreCase))
			{
				return "Demand source.";
			}
			else if (field.Equals("EffAnnual", StringComparison.OrdinalIgnoreCase))
			{
				return "Efficiency, annual (%).  Negative indicates monthly efficiencies.";
			}
			else if (field.Equals("EffMonthly01", StringComparison.OrdinalIgnoreCase))
			{
				return "Diversion efficiency for month 1 of year.";
			}
			else if (field.Equals("EffMonthly02", StringComparison.OrdinalIgnoreCase))
			{
				return "Diversion efficiency for month 2 of year.";
			}
			else if (field.Equals("EffMonthly03", StringComparison.OrdinalIgnoreCase))
			{
				return "Diversion efficiency for month 3 of year.";
			}
			else if (field.Equals("EffMonthly04", StringComparison.OrdinalIgnoreCase))
			{
				return "Diversion efficiency for month 4 of year.";
			}
			else if (field.Equals("EffMonthly05", StringComparison.OrdinalIgnoreCase))
			{
				return "Diversion efficiency for month 5 of year.";
			}
			else if (field.Equals("EffMonthly06", StringComparison.OrdinalIgnoreCase))
			{
				return "Diversion efficiency for month 6 of year.";
			}
			else if (field.Equals("EffMonthly07", StringComparison.OrdinalIgnoreCase))
			{
				return "Diversion efficiency for month 7 of year.";
			}
			else if (field.Equals("EffMonthly08", StringComparison.OrdinalIgnoreCase))
			{
				return "Diversion efficiency for month 8 of year.";
			}
			else if (field.Equals("EffMonthly09", StringComparison.OrdinalIgnoreCase))
			{
				return "Diversion efficiency for month 9 of year.";
			}
			else if (field.Equals("EffMonthly10", StringComparison.OrdinalIgnoreCase))
			{
				return "Diversion efficiency for month 10 of year.";
			}
			else if (field.Equals("EffMonthly11", StringComparison.OrdinalIgnoreCase))
			{
				return "Diversion efficiency for month 11 of year.";
			}
			else if (field.Equals("EffMonthly12", StringComparison.OrdinalIgnoreCase))
			{
				return "Diversion efficiency for month 12 of year.";
			}
		}
		return null;
	}

	/// <summary>
	/// Returns property values for diversion collections. </summary>
	/// <param name="propType"> the property to look up.  One of "FieldName", 
	/// "FieldNameHeader", "ToolTip", or "Format". </param>
	/// <param name="field"> the field for which to return the property. </param>
	/// <returns> the property, or if it could not be found null will be returned. </returns>
	private static string lookupDiversionCollectionPropValue(string propType, string field)
	{
		if (propType.Equals("FieldName", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("LocationID", StringComparison.OrdinalIgnoreCase))
			{
				return "DIVERSION ID";
			}
			else if (field.Equals("Year", StringComparison.OrdinalIgnoreCase))
			{
				return "YEAR";
			}
			else if (field.Equals("CollectionType", StringComparison.OrdinalIgnoreCase))
			{
				return "COLLECTION TYPE";
			}
			else if (field.Equals("PartType", StringComparison.OrdinalIgnoreCase))
			{
				return "PART TYPE";
			}
			else if (field.Equals("PartID", StringComparison.OrdinalIgnoreCase))
			{
				return "PART ID";
			}
		}
		else if (propType.Equals("FieldNameHeader", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("LocationID", StringComparison.OrdinalIgnoreCase))
			{
				return "DIVERSION\nID";
			}
			else if (field.Equals("Year", StringComparison.OrdinalIgnoreCase))
			{
				return "\nYEAR";
			}
			else if (field.Equals("CollectionType", StringComparison.OrdinalIgnoreCase))
			{
				return "COLLECTION\nTYPE";
			}
			else if (field.Equals("PartType", StringComparison.OrdinalIgnoreCase))
			{
				return "PART\nTYPE";
			}
			else if (field.Equals("PartID", StringComparison.OrdinalIgnoreCase))
			{
				return "PART\nID";
			}
		}
		else if (propType.Equals("Format", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("LocationID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
			else if (field.Equals("Year", StringComparison.OrdinalIgnoreCase))
			{
				return "%8d";
			}
			else if (field.Equals("CollectionType", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
			else if (field.Equals("PartType", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
			else if (field.Equals("PartID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
		}
		else if (propType.Equals("ToolTip", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("LocationID", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("Year", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("CollectionType", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("PartType", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("PartID", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
		}
		return null;
	}

	/// <summary>
	/// Returns property values for diversion return flows. </summary>
	/// <param name="propType"> the property to look up.  One of "FieldName", "FieldNameHeader", "ToolTip", or "Format". </param>
	/// <param name="field"> the field for which to return the property. </param>
	/// <returns> the property, or if it could not be found null will be returned. </returns>
	private static string lookupDiversionReturnFlowPropValue(string propType, string field)
	{
		if (propType.Equals("FieldName", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "DIVERSION STATION ID";
			}
			else if (field.Equals("RiverNodeID", StringComparison.OrdinalIgnoreCase))
			{
				return "RIVER NODE ID RECEIVING RETURN FLOW";
			}
			else if (field.Equals("ReturnPercent", StringComparison.OrdinalIgnoreCase))
			{
				return "PERCENT";
			}
			else if (field.Equals("DelayTableID", StringComparison.OrdinalIgnoreCase))
			{
				return "DELAY TABLE ID";
			}
			else if (field.Equals("Comment", StringComparison.OrdinalIgnoreCase))
			{
				return "COMMENT";
			}
		}
		else if (propType.Equals("FieldNameHeader", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "\n\nDIVERSION\nSTATION\nID";
			}
			else if (field.Equals("RiverNodeID", StringComparison.OrdinalIgnoreCase))
			{
				return "RIVER\nNODE ID\nRECEIVING\nRETURN\nFLOW";
			}
			else if (field.Equals("ReturnPercent", StringComparison.OrdinalIgnoreCase))
			{
				return "\n\n\n\nPERCENT";
			}
			else if (field.Equals("DelayTableID", StringComparison.OrdinalIgnoreCase))
			{
				return "\n\n\nDELAY\nTABLE ID";
			}
			else if (field.Equals("Comment", StringComparison.OrdinalIgnoreCase))
			{
				return "\n\n\n\nCOMMENT";
			}
		}
		else if (propType.Equals("Format", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
			else if (field.Equals("RiverNodeID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
			else if (field.Equals("ReturnPercent", StringComparison.OrdinalIgnoreCase))
			{
				return "%12.6f";
			}
			else if (field.Equals("DelayTableID", StringComparison.OrdinalIgnoreCase))
			{
				return "%8d";
			}
			else if (field.Equals("Comment", StringComparison.OrdinalIgnoreCase))
			{
				return "%s";
			}
		}
		else if (propType.Equals("ToolTip", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "Associated diversion station ID.";
			}
			else if (field.Equals("RiverNodeID", StringComparison.OrdinalIgnoreCase))
			{
				return "River node ID receiving return flow.";
			}
			else if (field.Equals("ReturnPercent", StringComparison.OrdinalIgnoreCase))
			{
				return "% of return (0-100)";
			}
			else if (field.Equals("DelayTableID", StringComparison.OrdinalIgnoreCase))
			{
				return "Delay table identifier";
			}
			else if (field.Equals("Comment", StringComparison.OrdinalIgnoreCase))
			{
				return "Comment explaining return assignment";
			}
		}
		return null;
	}

	/// <summary>
	/// Returns property values for diversion rights </summary>
	/// <param name="propType"> the property to look up.  One of "FieldName", "FieldNameHeader", "ToolTip", or "Format". </param>
	/// <param name="field"> the field for which to return the property. </param>
	/// <returns> the property, or if it could not be found null will be returned. </returns>
	private static string lookupDiversionRightPropValue(string propType, string field)
	{
		if (propType.Equals("FieldName", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "DIVERSION RIGHT ID";
			}
			else if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "DIVERSION RIGHT NAME";
			}
			else if (field.Equals("StationID", StringComparison.OrdinalIgnoreCase))
			{
				return "DIVERSION ID ASSOCIATED WITH RIGHT";
			}
			else if (field.Equals("AdministrationNumber", StringComparison.OrdinalIgnoreCase))
			{
				return "ADMINISTRATION NUMBER";
			}
			else if (field.Equals("Decree", StringComparison.OrdinalIgnoreCase))
			{
				return "DECREE AMOUNT (CFS)";
			}
			else if (field.Equals("OnOff", StringComparison.OrdinalIgnoreCase))
			{
				return "ON/OFF SWITCH";
			}
		}
		else if (propType.Equals("FieldNameHeader", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "\nDIVERSION\nRIGHT ID";
			}
			else if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "\nDIVERSION RIGHT\nNAME";
			}
			else if (field.Equals("StationID", StringComparison.OrdinalIgnoreCase))
			{
				return "DIVERSION ID\nASSOCIATED\nWITH RIGHT";
			}
			else if (field.Equals("AdministrationNumber", StringComparison.OrdinalIgnoreCase))
			{
				return "\nADMINISTRATION\nNUMBER";
			}
			else if (field.Equals("Decree", StringComparison.OrdinalIgnoreCase))
			{
				return "DECREE\nAMOUNT\n(CFS)";
			}
			else if (field.Equals("OnOff", StringComparison.OrdinalIgnoreCase))
			{
				return "\nON/OFF\nSWITCH";
			}
		}
		else if (propType.Equals("Format", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
			else if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "%-24.24s";
			}
			else if (field.Equals("StationID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
			else if (field.Equals("AdministrationNumber", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
			else if (field.Equals("Decree", StringComparison.OrdinalIgnoreCase))
			{
				return "%12.2f";
			}
			else if (field.Equals("OnOff", StringComparison.OrdinalIgnoreCase))
			{
				return "%8d";
			}
		}
		else if (propType.Equals("ToolTip", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "<html>The diversion right ID is typically the "
					+ "diversion station ID<br> followed by .01, .02, etc.</html>";
			}
			else if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "Diversion right name";
			}
			else if (field.Equals("StationID", StringComparison.OrdinalIgnoreCase))
			{
				return "<HTML>The diversion ID is the link between "
					+ "diversion stations and their right(s).</HTML>";
			}
			else if (field.Equals("AdministrationNumber", StringComparison.OrdinalIgnoreCase))
			{
				return "<HTML>Lower admininistration numbers indicate "
					+ "greater seniority.<BR>99999 is typical for a very junior right.</html>";
			}
			else if (field.Equals("Decree", StringComparison.OrdinalIgnoreCase))
			{
				return "Decree amount (CFS)";
			}
			else if (field.Equals("OnOff", StringComparison.OrdinalIgnoreCase))
			{
				return "<HTML>0 = OFF<BR>1 = ON<BR>YYYY indicates to turn on the right in "
					+ "year YYYY.<BR>-YYYY indicates to turn off the right in year YYYY.</HTML>";
			}
		}
		return null;
	}

	/// <summary>
	/// Returns property values for instream flows. </summary>
	/// <param name="propType"> the property to look up.  One of "FieldName", "FieldNameHeader", "ToolTip", or "Format". </param>
	/// <param name="field"> the field for which to return the property. </param>
	/// <returns> the property, or if it could not be found null will be returned. </returns>
	private static string lookupInstreamFlowPropValue(string propType, string field)
	{
		if (propType.Equals("FieldName", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "ID";
			}
			else if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "NAME";
			}
			else if (field.Equals("UpstreamRiverNodeID", StringComparison.OrdinalIgnoreCase))
			{
				return "RIVER NODE ID";
			}
			else if (field.Equals("OnOff", StringComparison.OrdinalIgnoreCase))
			{
				return "ON/OFF SWITCH";
			}
			else if (field.Equals("DownstreamRiverNodeID", StringComparison.OrdinalIgnoreCase))
			{
				return "DOWNSTREAM RIVER NODE ID";
			}
			else if (field.Equals("DailyID", StringComparison.OrdinalIgnoreCase))
			{
				return "DAILY ID";
			}
			else if (field.Equals("DemandType", StringComparison.OrdinalIgnoreCase))
			{
				return "DEMAND TYPE";
			}
		}
		else if (propType.Equals("FieldNameHeader", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "\n\nID";
			}
			else if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "\n\nNAME";
			}
			else if (field.Equals("UpstreamRiverNodeID", StringComparison.OrdinalIgnoreCase))
			{
				return "\nRIVER\nNODE ID";
			}
			else if (field.Equals("OnOff", StringComparison.OrdinalIgnoreCase))
			{
				return "\nON/OFF\nSWITCH";
			}
			else if (field.Equals("DownstreamRiverNodeID", StringComparison.OrdinalIgnoreCase))
			{
				return "DOWNSTREAM\nRIVER\nNODE ID";
			}
			else if (field.Equals("DailyID", StringComparison.OrdinalIgnoreCase))
			{
				return "\n\nDAILY ID";
			}
			else if (field.Equals("DemandType", StringComparison.OrdinalIgnoreCase))
			{
				return "\nDEMAND\nTYPE";
			}
		}
		else if (propType.Equals("Format", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
			else if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "%-24.24s";
			}
			else if (field.Equals("UpstreamRiverNodeID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
			else if (field.Equals("OnOff", StringComparison.OrdinalIgnoreCase))
			{
				return "%8d";
			}
			else if (field.Equals("DownstreamRiverNodeID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
			else if (field.Equals("DailyID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
			else if (field.Equals("DemandType", StringComparison.OrdinalIgnoreCase))
			{
				return "%8d";
			}
		}
		else if (propType.Equals("ToolTip", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "<html>The instream flow identifier is the main link between instream data data<BR>"
					+ "and must be unique in the data set.</html>";
			}
			else if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "<html>Instream flow name.</html>";
			}
			else if (field.Equals("UpstreamRiverNodeID", StringComparison.OrdinalIgnoreCase))
			{
				return "Upstream river ID where instream flow is located.";
			}
			else if (field.Equals("OnOff", StringComparison.OrdinalIgnoreCase))
			{
				return "<html>Switch.<br>0 = off<br>1 = on</html";
			}
			else if (field.Equals("DownstreamRiverNodeID", StringComparison.OrdinalIgnoreCase))
			{
				return "<html>Daily instream flow ID.</html>";
			}
			else if (field.Equals("DailyID", StringComparison.OrdinalIgnoreCase))
			{
				return "<html>Downstream river node, for instream flow reach.</html>";
			}
			else if (field.Equals("DemandType", StringComparison.OrdinalIgnoreCase))
			{
				return "<html>Data type switch.</html>";
			}
		}

		return null;
	}

	/// <summary>
	/// Returns property values for instream flow rights. </summary>
	/// <param name="propType"> the property to look up.  One of "FieldName", "FieldNameHeader", "ToolTip", or "Format". </param>
	/// <param name="field"> the field for which to return the property. </param>
	/// <returns> the property, or if it could not be found null will be returned. </returns>
	private static string lookupInstreamFlowRightPropValue(string propType, string field)
	{
		if (propType.Equals("FieldName", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "RIGHT ID";
			}
			else if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "RIGHT NAME";
			}
			else if (field.Equals("StationID", StringComparison.OrdinalIgnoreCase))
			{
				return "INSTREAM FLOW STATION ID ASSOCIATED WITH RIGHT";
			}
			else if (field.Equals("AdministrationNumber", StringComparison.OrdinalIgnoreCase))
			{
				return "ADMINISTRATION NUMBER";
			}
			else if (field.Equals("Decree", StringComparison.OrdinalIgnoreCase))
			{
				return "DECREED AMOUNT (CFS)";
			}
			else if (field.Equals("OnOff", StringComparison.OrdinalIgnoreCase))
			{
				return "ON/OFF SWITCH";
			}
		}
		else if (propType.Equals("FieldNameHeader", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "\n\n\nRIGHT ID";
			}
			else if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "\n\n\nRIGHT NAME";
			}
			else if (field.Equals("StationID", StringComparison.OrdinalIgnoreCase))
			{
				return "INSTREAM FLOW\nSTATION ID\nASSOCIATED\nWITH RIGHT";
			}
			else if (field.Equals("AdministrationNumber", StringComparison.OrdinalIgnoreCase))
			{
				return "\n\nADMINISTRATION\nNUMBER";
			}
			else if (field.Equals("Decree", StringComparison.OrdinalIgnoreCase))
			{
				return "\n\nDECREED\nAMOUNT (CFS)";
			}
			else if (field.Equals("OnOff", StringComparison.OrdinalIgnoreCase))
			{
				return "\n\nON/OFF\nSWITCH";
			}
		}
		else if (propType.Equals("Format", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
			else if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "%-24.24s";
			}
			else if (field.Equals("StationID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
			else if (field.Equals("AdministrationNumber", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
			else if (field.Equals("Decree", StringComparison.OrdinalIgnoreCase))
			{
				return "%8.2f";
			}
			else if (field.Equals("OnOff", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
		}
		else if (propType.Equals("ToolTip", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "<html>The instream flow right ID is typically "
					+ "the instream flow ID<br> followed by .01, .02, etc.</html>";
			}
			else if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "Instream flow right name.";
			}
			else if (field.Equals("StationID", StringComparison.OrdinalIgnoreCase))
			{
				return "<HTML>The instream flow ID is the link between "
					+ "instream  flows and their right<BR>(not editable here).</HTML>";
			}
			else if (field.Equals("AdministrationNumber", StringComparison.OrdinalIgnoreCase))
			{
				return "<HTML>Lower admininistration numbers indicate "
					+ "greater seniority.<BR>99999 is typical for a very junior right.</html>";
			}
			else if (field.Equals("Decree", StringComparison.OrdinalIgnoreCase))
			{
				return "Decreed amount (CFS).";
			}
			else if (field.Equals("OnOff", StringComparison.OrdinalIgnoreCase))
			{
				return "<HTML>0 = OFF<BR>1 = ON<BR>YYYY indicates to turn on the right in year "
					+ "YYYY.<BR>-YYYY indicates to turn off the right in year YYYY.</HTML>";
			}
		}
		return null;
	}

	/// <summary>
	/// Returns property values for reservoirs. </summary>
	/// <param name="propType"> the property to look up.  One of "FieldName", "FieldNameHeader", "ToolTip", or "Format". </param>
	/// <param name="field"> the field for which to return the property. </param>
	/// <returns> the property, or if it could not be found null will be returned. </returns>
	private static string lookupReservoirPropValue(string propType, string field)
	{
		if (propType.Equals("FieldName", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "ID";
			}
			else if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "NAME";
			}
			else if (field.Equals("RiverNodeID", StringComparison.OrdinalIgnoreCase))
			{
				return "RIVER NODE ID";
			}
			else if (field.Equals("OnOff", StringComparison.OrdinalIgnoreCase))
			{
				return "ON/OFF SWITCH";
			}
			else if (field.Equals("OneFillRule", StringComparison.OrdinalIgnoreCase))
			{
				return "ONE FILL DATE";
			}
			else if (field.Equals("ContentMin", StringComparison.OrdinalIgnoreCase))
			{
				return "MIN CONTENT (ACFT)";
			}
			else if (field.Equals("ContentMax", StringComparison.OrdinalIgnoreCase))
			{
				return "MAX CONTENT (ACFT)";
			}
			else if (field.Equals("ReleaseMax", StringComparison.OrdinalIgnoreCase))
			{
				return "MAX RELEASE (CFS)";
			}
			else if (field.Equals("DeadStorage", StringComparison.OrdinalIgnoreCase))
			{
				return "DEAD STORAGE (ACFT)";
			}
			else if (field.Equals("DailyID", StringComparison.OrdinalIgnoreCase))
			{
				return "DAILY ID";
			}
			else if (field.Equals("NumOwners", StringComparison.OrdinalIgnoreCase))
			{
				return "NUMBER OF OWNERS";
			}
			else if (field.Equals("NumPrecipStations", StringComparison.OrdinalIgnoreCase))
			{
				return "NUMBER OF PRECIP. STATIONS";
			}
			else if (field.Equals("NumEvapStations", StringComparison.OrdinalIgnoreCase))
			{
				return "NUMBER OF EVAP. STATIONS";
			}
			else if (field.Equals("NumCurveRows", StringComparison.OrdinalIgnoreCase))
			{
				return "NUMBER OF CURVE ROWS";
			}
		}
		else if (propType.Equals("FieldNameHeader", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "\n\nID";
			}
			else if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "\n\nNAME";
			}
			else if (field.Equals("RiverNodeID", StringComparison.OrdinalIgnoreCase))
			{
				return "\nRIVER\nNODE ID";
			}
			else if (field.Equals("OnOff", StringComparison.OrdinalIgnoreCase))
			{
				return "\nON/OFF\nSWITCH";
			}
			else if (field.Equals("OneFillRule", StringComparison.OrdinalIgnoreCase))
			{
				return "ONE\nFILL\nDATE";
			}
			else if (field.Equals("ContentMin", StringComparison.OrdinalIgnoreCase))
			{
				return "MIN\nCONTENT\n(ACFT)";
			}
			else if (field.Equals("ContentMax", StringComparison.OrdinalIgnoreCase))
			{
				return "MAX\nCONTENT\n(ACFT)";
			}
			else if (field.Equals("ReleaseMax", StringComparison.OrdinalIgnoreCase))
			{
				return "MAX\nRELEASE\n(CFS)";
			}
			else if (field.Equals("DeadStorage", StringComparison.OrdinalIgnoreCase))
			{
				return "DEAD\nSTORAGE\n(ACFT)";
			}
			else if (field.Equals("DailyID", StringComparison.OrdinalIgnoreCase))
			{
				return "\nDAILY\nID";
			}
			else if (field.Equals("NumOwners", StringComparison.OrdinalIgnoreCase))
			{
				return "NUMBER\nOF\nOWNERS";
			}
			else if (field.Equals("NumPrecipStations", StringComparison.OrdinalIgnoreCase))
			{
				return "NUMBER\nOF PRECIP.\nSTATIONS";
			}
			else if (field.Equals("NumEvapStations", StringComparison.OrdinalIgnoreCase))
			{
				return "NUMBER\nOF EVAP.\nSTATIONS";
			}
			else if (field.Equals("NumCurveRows", StringComparison.OrdinalIgnoreCase))
			{
				return "NUMBER\nOF CURVE\nROWS";
			}
		}
		else if (propType.Equals("Format", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
			else if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "%-24.24s";
			}
			else if (field.Equals("RiverNodeID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
			else if (field.Equals("OnOff", StringComparison.OrdinalIgnoreCase))
			{
				return "%8d";
			}
			else if (field.Equals("OneFillRule", StringComparison.OrdinalIgnoreCase))
			{
				return "%8d";
			}
			else if (field.Equals("ContentMin", StringComparison.OrdinalIgnoreCase))
			{
				return "%10.2f";
			}
			else if (field.Equals("ContentMax", StringComparison.OrdinalIgnoreCase))
			{
				return "%10.2f";
			}
			else if (field.Equals("ReleaseMax", StringComparison.OrdinalIgnoreCase))
			{
				return "%10.2f";
			}
			else if (field.Equals("DeadStorage", StringComparison.OrdinalIgnoreCase))
			{
				return "%10.2f";
			}
			else if (field.Equals("DailyID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
			else if (field.Equals("NumOwners", StringComparison.OrdinalIgnoreCase))
			{
				return "%8d";
			}
			else if (field.Equals("NumPrecipStations", StringComparison.OrdinalIgnoreCase))
			{
				return "%8d";
			}
			else if (field.Equals("NumEvapStations", StringComparison.OrdinalIgnoreCase))
			{
				return "%8d";
			}
			else if (field.Equals("NumCurveRows", StringComparison.OrdinalIgnoreCase))
			{
				return "%8d";
			}
		}
		else if (propType.Equals("ToolTip", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "<html>The reservoir station identifier is the "
					+ "main link between reservoir data<BR>and must be unique in the data set.</html>";
			}
			else if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "Reservoir station name.";
			}
			else if (field.Equals("RiverNodeID", StringComparison.OrdinalIgnoreCase))
			{
				return "Node where reservoir is located.";
			}
			else if (field.Equals("OnOff", StringComparison.OrdinalIgnoreCase))
			{
				return "<html>Switch.<br>0 = off<br>1 = on</html>";
			}
			else if (field.Equals("OneFillRule", StringComparison.OrdinalIgnoreCase))
			{
				return "<html>Date for one fill rule admin.</html>";
			}
			else if (field.Equals("ContentMin", StringComparison.OrdinalIgnoreCase))
			{
				return "<html>Minimum reservoir content (ACFT).</html>";
			}
			else if (field.Equals("ContentMax", StringComparison.OrdinalIgnoreCase))
			{
				return "<html>Maximum reservoir content (ACFT).</html>";
			}
			else if (field.Equals("ReleaseMax", StringComparison.OrdinalIgnoreCase))
			{
				return "<html>Maximum release (CFS).</html>";
			}
			else if (field.Equals("DeadStorage", StringComparison.OrdinalIgnoreCase))
			{
				return "<html>Dead storage in reservoir (ACFT).</html>";
			}
			else if (field.Equals("DailyID", StringComparison.OrdinalIgnoreCase))
			{
				return "Identifier for daily time series.";
			}
			else if (field.Equals("NumOwners", StringComparison.OrdinalIgnoreCase))
			{
				return "Number of owners.";
			}
			else if (field.Equals("NumPrecipStations", StringComparison.OrdinalIgnoreCase))
			{
				return "Number of precipitation stations.";
			}
			else if (field.Equals("NumEvapStations", StringComparison.OrdinalIgnoreCase))
			{
				return "Number of evaporation stations.";
			}
			else if (field.Equals("NumCurveRows", StringComparison.OrdinalIgnoreCase))
			{
				return "Number of curve rows.";
			}
		}
		return null;
	}

	/// <summary>
	/// Returns property values for reservoir accounts. </summary>
	/// <param name="propType"> the property to look up.  One of "FieldName", "FieldNameHeader", "ToolTip", or "Format". </param>
	/// <param name="field"> the field for which to return the property. </param>
	/// <returns> the property, or if it could not be found null will be returned. </returns>
	private static string lookupReservoirAccountPropValue(string propType, string field)
	{
		if (propType.Equals("FieldName", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ReservoirID", StringComparison.OrdinalIgnoreCase))
			{
				return "RESERVOIR ID";
			}
			else if (field.Equals("OwnerID", StringComparison.OrdinalIgnoreCase))
			{
				return "OWNER ID";
			}
			else if (field.Equals("OwnerAccount", StringComparison.OrdinalIgnoreCase))
			{
				return "OWNER ACCOUNT";
			}
			else if (field.Equals("MaxStorage", StringComparison.OrdinalIgnoreCase))
			{
				return "MAXIMUM STORAGE (ACFT)";
			}
			else if (field.Equals("InitialStorage", StringComparison.OrdinalIgnoreCase))
			{
				return "INITIAL STORAGE (ACFT)";
			}
			else if (field.Equals("ProrateEvap", StringComparison.OrdinalIgnoreCase))
			{
				return "EVAPORATION DISTRIBUTION FLAG";
			}
			else if (field.Equals("OwnershipTie", StringComparison.OrdinalIgnoreCase))
			{
				return "ACCOUNT ONE FILL CALCULATION";
			}
		}
		else if (propType.Equals("FieldNameHeader", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ReservoirID", StringComparison.OrdinalIgnoreCase))
			{
				return "\nRESERVOIR\nID";
			}
			else if (field.Equals("OwnerID", StringComparison.OrdinalIgnoreCase))
			{
				return "\nOWNER\nID";
			}
			else if (field.Equals("OwnerAccount", StringComparison.OrdinalIgnoreCase))
			{
				return "\nOWNER\nACCOUNT";
			}
			else if (field.Equals("MaxStorage", StringComparison.OrdinalIgnoreCase))
			{
				return "MAXIMUM\nSTORAGE\n(ACFT)";
			}
			else if (field.Equals("InitialStorage", StringComparison.OrdinalIgnoreCase))
			{
				return "INITIAL\nSTORAGE\n(ACFT)";
			}
			else if (field.Equals("ProrateEvap", StringComparison.OrdinalIgnoreCase))
			{
				return "EVAPORATION\nDISTRIBUTION\nFLAG";
			}
			else if (field.Equals("OwnershipTie", StringComparison.OrdinalIgnoreCase))
			{
				return "ACCOUNT\nONE FILL\nCALCULATION";
			}
		}
		else if (propType.Equals("Format", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ReservoirID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
			else if (field.Equals("OwnerID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
			else if (field.Equals("OwnerAccount", StringComparison.OrdinalIgnoreCase))
			{
				return "%-24.24s";
			}
			else if (field.Equals("MaxStorage", StringComparison.OrdinalIgnoreCase))
			{
				return "%12.1f";
			}
			else if (field.Equals("InitialStorage", StringComparison.OrdinalIgnoreCase))
			{
				return "%12.1f";
			}
			else if (field.Equals("ProrateEvap", StringComparison.OrdinalIgnoreCase))
			{
				return "%8.0f";
			}
			else if (field.Equals("OwnershipTie", StringComparison.OrdinalIgnoreCase))
			{
				return "%8d";
			}
		}
		else if (propType.Equals("ToolTip", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ReservoirID", StringComparison.OrdinalIgnoreCase))
			{
				return "<html>The reservoir station ID of the reservoir to which<br>the rights belong.</html>";
			}
			else if (field.Equals("OwnerID", StringComparison.OrdinalIgnoreCase))
			{
				return "Sequential number 1+ (not used by StateMod)";
			}
			else if (field.Equals("OwnerAccount", StringComparison.OrdinalIgnoreCase))
			{
				return "Account name.";
			}
			else if (field.Equals("MaxStorage", StringComparison.OrdinalIgnoreCase))
			{
				return "Maximum account storage (ACFT).";
			}
			else if (field.Equals("InitialStorage", StringComparison.OrdinalIgnoreCase))
			{
				return "Initial account storage (ACFT).";
			}
			else if (field.Equals("ProrateEvap", StringComparison.OrdinalIgnoreCase))
			{
				return "How to prorate evaporation.";
			}
			else if (field.Equals("OwnershipTie", StringComparison.OrdinalIgnoreCase))
			{
				return "One fill rule calculation flag.";
			}
		}
		return null;
	}

	/// <summary>
	/// Returns property values for reservoir area caps. </summary>
	/// <param name="propType"> the property to look up.  One of "FieldName", "FieldNameHeader", "ToolTip", or "Format". </param>
	/// <param name="field"> the field for which to return the property. </param>
	/// <returns> the property, or if it could not be found null will be returned. </returns>
	private static string lookupReservoirAreaCapPropValue(string propType, string field)
	{
		if (propType.Equals("FieldName", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ReservoirID", StringComparison.OrdinalIgnoreCase))
			{
				return "RESERVOIR ID";
			}
			else if (field.Equals("Content", StringComparison.OrdinalIgnoreCase))
			{
				return "CONTENT (ACFT)";
			}
			else if (field.Equals("Area", StringComparison.OrdinalIgnoreCase))
			{
				return "AREA (ACRE)";
			}
			else if (field.Equals("Seepage", StringComparison.OrdinalIgnoreCase))
			{
				return "SEEPAGE (AF/M)";
			}
		}
		else if (propType.Equals("FieldNameHeader", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ReservoirID", StringComparison.OrdinalIgnoreCase))
			{
				return "RESERVOIR\nID";
			}
			else if (field.Equals("Content", StringComparison.OrdinalIgnoreCase))
			{
				return "CONTENT\n(ACFT)";
			}
			else if (field.Equals("Area", StringComparison.OrdinalIgnoreCase))
			{
				return "AREA\n(ACRE)";
			}
			else if (field.Equals("Seepage", StringComparison.OrdinalIgnoreCase))
			{
				return "SEEPAGE\n(AF/M)";
			}
		}
		else if (propType.Equals("Format", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ReservoirID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
			else if (field.Equals("Content", StringComparison.OrdinalIgnoreCase))
			{
				return "%12.1f";
			}
			else if (field.Equals("Area", StringComparison.OrdinalIgnoreCase))
			{
				return "%12.1f";
			}
			else if (field.Equals("Seepage", StringComparison.OrdinalIgnoreCase))
			{
				return "%12.1f";
			}
		}
		else if (propType.Equals("ToolTip", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ReservoirID", StringComparison.OrdinalIgnoreCase))
			{
				return "<html>The reservoir station ID of the reservoir to which<br>the area capacity "
					+ "information belongs.</html>";
			}
			else if (field.Equals("Content", StringComparison.OrdinalIgnoreCase))
			{
				return "Reservoir content (ACFT).";
			}
			else if (field.Equals("Area", StringComparison.OrdinalIgnoreCase))
			{
				return "Reservoir area (ACRE).";
			}
			else if (field.Equals("Seepage", StringComparison.OrdinalIgnoreCase))
			{
				return "Reservoir seepage (AF/M).";
			}
		}
		return null;
	}

	/// <summary>
	/// Returns property values for reservoir precipitation stations. </summary>
	/// <param name="propType"> the property to look up.  One of "FieldName", "FieldNameHeader", "ToolTip", or "Format". </param>
	/// <param name="field"> the field for which to return the property. </param>
	/// <returns> the property, or if it could not be found null will be returned. </returns>
	private static string lookupReservoirPrecipStationPropValue(string propType, string field)
	{
		if (propType.Equals("FieldName", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ReservoirID", StringComparison.OrdinalIgnoreCase))
			{
				return "RESERVOIR ID";
			}
			else if (field.Equals("StationID", StringComparison.OrdinalIgnoreCase))
			{
				return "STATION ID";
			}
			else if (field.Equals("PercentWeight", StringComparison.OrdinalIgnoreCase))
			{
				return "WEIGHT (%)";
			}
		}
		else if (propType.Equals("FieldNameHeader", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ReservoirID", StringComparison.OrdinalIgnoreCase))
			{
				return "RESERVOIR ID";
			}
			else if (field.Equals("StationID", StringComparison.OrdinalIgnoreCase))
			{
				return "STATION ID";
			}
			else if (field.Equals("PercentWeight", StringComparison.OrdinalIgnoreCase))
			{
				return "WEIGHT (%)";
			}
		}
		else if (propType.Equals("Format", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ReservoirID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
			else if (field.Equals("StationID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
			else if (field.Equals("PercentWeight", StringComparison.OrdinalIgnoreCase))
			{
				return "%12.1f";
			}
		}
		else if (propType.Equals("ToolTip", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ReservoirID", StringComparison.OrdinalIgnoreCase))
			{
				return "<html>The reservoir station ID of the reservoir to which<br>the climate data belong.</html>";
			}
			else if (field.Equals("StationID", StringComparison.OrdinalIgnoreCase))
			{
				return "Station identifier.";
			}
			else if (field.Equals("PercentWeight", StringComparison.OrdinalIgnoreCase))
			{
				return "Weight for station's data (%).";
			}
		}
		return null;
	}

	/// <summary>
	/// Returns property values for reservoir evaporation stations. </summary>
	/// <param name="propType"> the property to look up.  One of "FieldName", "FieldNameHeader", "ToolTip", or "Format". </param>
	/// <param name="field"> the field for which to return the property. </param>
	/// <returns> the property, or if it could not be found null will be returned. </returns>
	private static string lookupReservoirEvapStationPropValue(string propType, string field)
	{
		if (propType.Equals("FieldName", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ReservoirID", StringComparison.OrdinalIgnoreCase))
			{
				return "RESERVOIR ID";
			}
			else if (field.Equals("StationID", StringComparison.OrdinalIgnoreCase))
			{
				return "STATION ID";
			}
			else if (field.Equals("PercentWeight", StringComparison.OrdinalIgnoreCase))
			{
				return "WEIGHT (%)";
			}
		}
		else if (propType.Equals("FieldNameHeader", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ReservoirID", StringComparison.OrdinalIgnoreCase))
			{
				return "RESERVOIR ID";
			}
			else if (field.Equals("StationID", StringComparison.OrdinalIgnoreCase))
			{
				return "STATION ID";
			}
			else if (field.Equals("PercentWeight", StringComparison.OrdinalIgnoreCase))
			{
				return "WEIGHT (%)";
			}
		}
		else if (propType.Equals("Format", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ReservoirID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
			else if (field.Equals("StationID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
			else if (field.Equals("PercentWeight", StringComparison.OrdinalIgnoreCase))
			{
				return "%12.1f";
			}
		}
		else if (propType.Equals("ToolTip", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ReservoirID", StringComparison.OrdinalIgnoreCase))
			{
				return "<html>The reservoir station ID of the reservoir to which<br>the climate data belong.</html>";
			}
			else if (field.Equals("StationID", StringComparison.OrdinalIgnoreCase))
			{
				return "Station identifier.";
			}
			else if (field.Equals("PercentWeight", StringComparison.OrdinalIgnoreCase))
			{
				return "Weight for station's data (%).";
			}
		}
		return null;
	}

	/// <summary>
	/// Returns property values for reservoir collections. </summary>
	/// <param name="propType"> the property to look up.  One of "FieldName", "FieldNameHeader", "ToolTip", or "Format". </param>
	/// <param name="field"> the field for which to return the property. </param>
	/// <returns> the property, or if it could not be found null will be returned. </returns>
	private static string lookupReservoirCollectionPropValue(string propType, string field)
	{
		if (propType.Equals("FieldName", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("LocationID", StringComparison.OrdinalIgnoreCase))
			{
				return "RESERVOIR ID";
			}
			else if (field.Equals("Year", StringComparison.OrdinalIgnoreCase))
			{
				return "YEAR";
			}
			else if (field.Equals("CollectionType", StringComparison.OrdinalIgnoreCase))
			{
				return "COLLECTION TYPE";
			}
			else if (field.Equals("PartType", StringComparison.OrdinalIgnoreCase))
			{
				return "PART TYPE";
			}
			else if (field.Equals("PartID", StringComparison.OrdinalIgnoreCase))
			{
				return "PART ID";
			}
		}
		else if (propType.Equals("FieldNameHeader", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("LocationID", StringComparison.OrdinalIgnoreCase))
			{
				return "RESERVOIR\nID";
			}
			else if (field.Equals("Year", StringComparison.OrdinalIgnoreCase))
			{
				return "\nYEAR";
			}
			else if (field.Equals("CollectionType", StringComparison.OrdinalIgnoreCase))
			{
				return "COLLECTION\nTYPE";
			}
			else if (field.Equals("PartType", StringComparison.OrdinalIgnoreCase))
			{
				return "PART\nTYPE";
			}
			else if (field.Equals("PartID", StringComparison.OrdinalIgnoreCase))
			{
				return "PART\nID";
			}
		}
		else if (propType.Equals("Format", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("LocationID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
			else if (field.Equals("Year", StringComparison.OrdinalIgnoreCase))
			{
				return "%8d";
			}
			else if (field.Equals("CollectionType", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
			else if (field.Equals("PartType", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
			else if (field.Equals("PartID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
		}
		else if (propType.Equals("ToolTip", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("LocationID", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("Year", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("CollectionType", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("PartType", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("PartID", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
		}
		return null;
	}

	/// <summary>
	/// Returns property values for reservoir rights. </summary>
	/// <param name="propType"> the property to look up.  One of "FieldName", "FieldNameHeader", "ToolTip", or "Format". </param>
	/// <param name="field"> the field for which to return the property. </param>
	/// <returns> the property, or if it could not be found null will be returned. </returns>
	private static string lookupReservoirRightPropValue(string propType, string field)
	{
		if (propType.Equals("FieldName", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "RIGHT ID";
			}
			else if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "RIGHT NAME";
			}
			else if (field.Equals("StructureID", StringComparison.OrdinalIgnoreCase))
			{
				return "RESERVOIR STATION ID ASSOC. W/ RIGHT";
			}
			else if (field.Equals("AdministrationNumber", StringComparison.OrdinalIgnoreCase))
			{
				return "ADMINISTRATION NUMBER";
			}
			else if (field.Equals("DecreedAmount", StringComparison.OrdinalIgnoreCase))
			{
				return "DECREE AMOUNT (ACFT)";
			}
			else if (field.Equals("OnOff", StringComparison.OrdinalIgnoreCase))
			{
				return "ON/OFF SWITCH";
			}
			else if (field.Equals("AccountDistribution", StringComparison.OrdinalIgnoreCase))
			{
				return "ACCOUNT DISTRIBUTION";
			}
			else if (field.Equals("Type", StringComparison.OrdinalIgnoreCase))
			{
				return "RIGHT TYPE";
			}
			else if (field.Equals("FillType", StringComparison.OrdinalIgnoreCase))
			{
				return "FILL TYPE";
			}
			else if (field.Equals("OopRight", StringComparison.OrdinalIgnoreCase))
			{
				return "OUT OF PRIORITY RIGHT";
			}
		}
		else if (propType.Equals("FieldNameHeader", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "\nRIGHT\nID";
			}
			else if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "\nRIGHT\nNAME";
			}
			else if (field.Equals("StructureID", StringComparison.OrdinalIgnoreCase))
			{
				return "RESERVOIR\nSTATION ID\nASSOC. W/ RIGHT";
			}
			else if (field.Equals("AdministrationNumber", StringComparison.OrdinalIgnoreCase))
			{
				return "\nADMINISTRATION\nNUMBER";
			}
			else if (field.Equals("DecreedAmount", StringComparison.OrdinalIgnoreCase))
			{
				return "DECREE\nAMOUNT\n(ACFT)";
			}
			else if (field.Equals("OnOff", StringComparison.OrdinalIgnoreCase))
			{
				return "\nON/OFF\nSWITCH";
			}
			else if (field.Equals("AccountDistribution", StringComparison.OrdinalIgnoreCase))
			{
				return "\nACCOUNT\nDISTRIBUTION";
			}
			else if (field.Equals("Type", StringComparison.OrdinalIgnoreCase))
			{
				return "\n\nRIGHT TYPE";
			}
			else if (field.Equals("FillType", StringComparison.OrdinalIgnoreCase))
			{
				return "\n\nFILL TYPE";
			}
			else if (field.Equals("OopRight", StringComparison.OrdinalIgnoreCase))
			{
				return "OUT OF\nPRIORITY\nRIGHT";
			}
		}
		else if (propType.Equals("Format", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
			else if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "%-24.24s";
			}
			else if (field.Equals("StructureID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-8.8s";
			}
			else if (field.Equals("AdministrationNumber", StringComparison.OrdinalIgnoreCase))
			{
				return "%-20.20s";
			}
			else if (field.Equals("DecreedAmount", StringComparison.OrdinalIgnoreCase))
			{
				return "%12.1f";
			}
			else if (field.Equals("OnOff", StringComparison.OrdinalIgnoreCase))
			{
				return "%8d";
			}
			else if (field.Equals("AccountDistribution", StringComparison.OrdinalIgnoreCase))
			{
				return "%8d";
			}
			else if (field.Equals("Type", StringComparison.OrdinalIgnoreCase))
			{
				return "%8d";
			}
			else if (field.Equals("FillType", StringComparison.OrdinalIgnoreCase))
			{
				return "%8d";
			}
			else if (field.Equals("OopRight", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
		}
		else if (propType.Equals("ToolTip", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "<html>The reservoir right ID is typically the reservoir station ID<br> followed by .01, "
					+ ".02, etc.</html>";
			}
			else if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "Reservoir right name";
			}
			else if (field.Equals("StructureID", StringComparison.OrdinalIgnoreCase))
			{
				return "<HTML>The reservoir ID is the link between reservoir stations and their right(s).</HTML>";
			}
			else if (field.Equals("AdministrationNumber", StringComparison.OrdinalIgnoreCase))
			{
				return "<HTML>Lower admininistration numbers indicate greater seniority.<BR>99999 is typical for "
					+ "a very junior right.</html>";
			}
			else if (field.Equals("DecreedAmount", StringComparison.OrdinalIgnoreCase))
			{
				return "Decreed amount (ACFT)";
			}
			else if (field.Equals("OnOff", StringComparison.OrdinalIgnoreCase))
			{
				return "<HTML>0 = OFF<BR>1 = ON<BR>YYYY indicates to turn on the right in year "
					+ "YYYY.<BR>-YYYY indicates to turn off the right in year YYYY.</HTML>";
			}
			else if (field.Equals("AccountDistribution", StringComparison.OrdinalIgnoreCase))
			{
				return "Account distribution switch.";
			}
			else if (field.Equals("Type", StringComparison.OrdinalIgnoreCase))
			{
				return "Right type.";
			}
			else if (field.Equals("FillType", StringComparison.OrdinalIgnoreCase))
			{
				return "Fill type.";
			}
			else if (field.Equals("OopRight", StringComparison.OrdinalIgnoreCase))
			{
				return "Out-of-priority associated operational right.";
			}
		}

		return null;
	}

	/// <summary>
	/// Returns property values for river nodes. </summary>
	/// <param name="propType"> the property to look up.  One of "FieldName", "FieldNameHeader", "ToolTip", or "Format". </param>
	/// <param name="field"> the field for which to return the property. </param>
	/// <returns> the property, or if it could not be found null will be returned. </returns>
	private static string lookupRiverNodePropValue(string propType, string field)
	{
		if (propType.Equals("FieldName", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "RIVER NODE ID";
			}
			else if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "STATION NAME";
			}
			else if (field.Equals("DownstreamID", StringComparison.OrdinalIgnoreCase))
			{
				return "DOWNSTREAM RIVER NODE ID";
			}
			else if (field.Equals("Comment", StringComparison.OrdinalIgnoreCase))
			{
				return "COMMENT";
			}
			else if (field.Equals("GWMaxRecharge", StringComparison.OrdinalIgnoreCase))
			{
				return "GW MAX RECHARGE";
			}
		}
		else if (propType.Equals("FieldNameHeader", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "RIVER NODE ID";
			}
			else if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "STATION NAME";
			}
			else if (field.Equals("DownstreamID", StringComparison.OrdinalIgnoreCase))
			{
				return "DOWNSTREAM RIVER NODE ID";
			}
			else if (field.Equals("Comment", StringComparison.OrdinalIgnoreCase))
			{
				return "COMMENT";
			}
			else if (field.Equals("GWMaxRecharge", StringComparison.OrdinalIgnoreCase))
			{
				return "GW MAX RECHARGE";
			}
		}
		else if (propType.Equals("Format", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
			else if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "%-24.24s";
			}
			else if (field.Equals("DownstreamID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
			else if (field.Equals("Comment", StringComparison.OrdinalIgnoreCase))
			{
				return "%-80.80s";
			}
			else if (field.Equals("GWMaxRecharge", StringComparison.OrdinalIgnoreCase))
			{
				return "%8.8s";
			}
		}
		else if (propType.Equals("ToolTip", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("DownstreamID", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("Comment", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("GWMaxRecharge", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
		}
		return null;
	}

	/// <summary>
	/// Returns property values for stream estimate stations. </summary>
	/// <param name="propType"> the property to look up.  One of "FieldName", "FieldNameHeader", "ToolTip", or "Format". </param>
	/// <param name="field"> the field for which to return the property. </param>
	/// <returns> the property, or if it could not be found null will be returned. </returns>
	private static string lookupStreamEstimatePropValue(string propType, string field)
	{
		if (propType.Equals("FieldName", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "ID";
			}
			else if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "NAME";
			}
			else if (field.Equals("RiverNodeID", StringComparison.OrdinalIgnoreCase))
			{
				return "RIVER NODE ID";
			}
			else if (field.Equals("DailyID", StringComparison.OrdinalIgnoreCase))
			{
				return "DAILY ID";
			}
		}
		else if (propType.Equals("FieldNameHeader", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "\nID";
			}
			else if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "\nNAME";
			}
			else if (field.Equals("RiverNodeID", StringComparison.OrdinalIgnoreCase))
			{
				return "RIVER\nNODE ID";
			}
			else if (field.Equals("DailyID", StringComparison.OrdinalIgnoreCase))
			{
				return "DAILY\nID";
			}
		}
		else if (propType.Equals("Format", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
			else if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "%-24.24s";
			}
			else if (field.Equals("RiverNodeID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
			else if (field.Equals("DailyID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
		}
		else if (propType.Equals("ToolTip", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("RiverNodeID", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("DailyID", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
		}

		return null;
	}

	/// <summary>
	/// Returns property values for stream estimate coefficients. </summary>
	/// <param name="propType"> the property to look up.  One of "FieldName", "FieldNameHeader", "ToolTip", or "Format". </param>
	/// <param name="field"> the field for which to return the property. </param>
	/// <returns> the property, or if it could not be found null will be returned. </returns>
	private static string lookupStreamEstimateCoefficientPropValue(string propType, string field)
	{
		if (propType.Equals("FieldName", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "ID";
			}
			else if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "STREAM TERM";
			}
			else if (field.Equals("UpstreamGage", StringComparison.OrdinalIgnoreCase))
			{
				return "UPSTREAM TERM GAGE";
			}
			else if (field.Equals("ProrationFactor", StringComparison.OrdinalIgnoreCase))
			{
				return "GAIN TERM PRORATION FACTOR";
			}
			else if (field.Equals("Weight", StringComparison.OrdinalIgnoreCase))
			{
				return "GAIN TERM WEIGHT";
			}
			else if (field.Equals("GageID", StringComparison.OrdinalIgnoreCase))
			{
				return "GAIN TERM GAGE ID";
			}
		}
		else if (propType.Equals("FieldNameHeader", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "\n\nID";
			}
			else if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "\nSTREAM\nTERM";
			}
			else if (field.Equals("UpstreamGage", StringComparison.OrdinalIgnoreCase))
			{
				return "\nUPSTREAM\nTERM GAGE";
			}
			else if (field.Equals("ProrationFactor", StringComparison.OrdinalIgnoreCase))
			{
				return "GAIN TERM\nPRORATION\nFACTOR";
			}
			else if (field.Equals("Weight", StringComparison.OrdinalIgnoreCase))
			{
				return "\nGAIN TERM\nWEIGHT";
			}
			else if (field.Equals("GageID", StringComparison.OrdinalIgnoreCase))
			{
				return "\nGAIN TERM\nGAGE ID";
			}
		}
		else if (propType.Equals("Format", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
			else if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "%8.1f";
			}
			else if (field.Equals("UpstreamGage", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
			else if (field.Equals("ProrationFactor", StringComparison.OrdinalIgnoreCase))
			{
				return "%8.1f";
			}
			else if (field.Equals("Weight", StringComparison.OrdinalIgnoreCase))
			{
				return "%8.1f";
			}
			else if (field.Equals("GageID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
		}
		else if (propType.Equals("ToolTip", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("UpstreamGage", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("ProrationFactor", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("Weight", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("GageID", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
		}
		return null;
	}

	/// <summary>
	/// Returns property values for stream gages. </summary>
	/// <param name="propType"> the property to look up.  One of "FieldName", "FieldNameHeader", "ToolTip", or "Format". </param>
	/// <param name="field"> the field for which to return the property. </param>
	/// <returns> the property, or if it could not be found null will be returned. </returns>
	private static string lookupStreamGagePropValue(string propType, string field)
	{
		if (propType.Equals("FieldName", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "ID";
			}
			else if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "NAME";
			}
			else if (field.Equals("NodeID", StringComparison.OrdinalIgnoreCase))
			{
				return "RIVER NODE ID";
			}
			else if (field.Equals("DailyID", StringComparison.OrdinalIgnoreCase))
			{
				return "DAILY ID";
			}
		}
		else if (propType.Equals("FieldNameHeader", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "\nID";
			}
			else if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "\nNAME";
			}
			else if (field.Equals("NodeID", StringComparison.OrdinalIgnoreCase))
			{
				return "RIVER\nNODE ID";
			}
			else if (field.Equals("DailyID", StringComparison.OrdinalIgnoreCase))
			{
				return "DAILY\nID";
			}
		}
		else if (propType.Equals("Format", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
			else if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "%-24.24s";
			}
			else if (field.Equals("NodeID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
			else if (field.Equals("DailyID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
		}
		else if (propType.Equals("ToolTip", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("NodeID", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("DailyID", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
		}
		return null;
	}

	/// <summary>
	/// Get the matching time series for an identifier.  This is used, for example, 
	/// to find the time series associated with a StateMod data object, when the time
	/// series is not a reference data member within the data object (e.g., climate time series for reservoirs. </summary>
	/// <param name="id"> Identifier associated with a StateMod data object, which will be
	/// compared with the location part of the time series identifier. </param>
	/// <param name="tslist"> Vector of time series to search, typically read from one of the time series data files. </param>
	/// <param name="match_count"> Indicates which match to return.  In most cases this will be
	/// 1 but for some time series (e.g., reservoir targets) the second match may be requested. </param>
	/// <returns> matching time series or null if no match is found. </returns>
	public static TS lookupTimeSeries(string id, System.Collections.IList tslist, int match_count)
	{
		if ((string.ReferenceEquals(id, null)) || id.Equals(""))
		{
			return null;
		}
		int size = 0;
		if (tslist != null)
		{
			size = tslist.Count;
		}
		TS ts = null;
		object o = null;
		int match_count2 = 0;
		for (int i = 0; i < size; i++)
		{
			o = tslist[i];
			if (o == null)
			{
				continue;
			}
			ts = (TS)o;
			if (id.Equals(ts.getLocation(), StringComparison.OrdinalIgnoreCase))
			{
				++match_count2;
				if (match_count2 == match_count)
				{
					// Match is found so return.
					return ts;
				}
			}
		}
		return null;
	}

	/// <summary>
	/// Look up a title to use for a time series graph, given the data set component.
	/// Currently this simply returns the component name, replacing " TS " with " Time Series ". </summary>
	/// <param name="comp_type"> StateMod component type. </param>
	public static string lookupTimeSeriesGraphTitle(int comp_type)
	{
		StateMod_DataSet dataset = new StateMod_DataSet();
		return dataset.lookupComponentName(comp_type).replaceAll(" TS ", " Time Series ");
	}

	/// <summary>
	/// Returns property values for wells. </summary>
	/// <param name="propType"> the property to look up.  One of "FieldName", "FieldNameHeader", "ToolTip", or "Format". </param>
	/// <param name="field"> the field for which to return the property. </param>
	/// <returns> the property, or if it could not be found null will be returned. </returns>
	private static string lookupWellPropValue(string propType, string field)
	{
		if (propType.Equals("FieldName", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "ID";
			}
			else if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "NAME";
			}
			else if (field.Equals("RiverNodeID", StringComparison.OrdinalIgnoreCase))
			{
				return "RIVER NODE ID";
			}
			else if (field.Equals("OnOff", StringComparison.OrdinalIgnoreCase))
			{
				return "ON/OFF SWITCH";
			}
			else if (field.Equals("Capacity", StringComparison.OrdinalIgnoreCase))
			{
				return "CAPACITY (CFS)";
			}
			else if (field.Equals("DailyID", StringComparison.OrdinalIgnoreCase))
			{
				return "DAILY ID";
			}
			else if (field.Equals("Primary", StringComparison.OrdinalIgnoreCase))
			{
				return "ADMINISTRATION NUMBER SWITCH";
			}
			else if (field.Equals("DiversionID", StringComparison.OrdinalIgnoreCase))
			{
				return "RELATED DIVERSION ID";
			}
			else if (field.Equals("DemandType", StringComparison.OrdinalIgnoreCase))
			{
				return "DATA TYPE";
			}
			else if (field.Equals("EffAnnual", StringComparison.OrdinalIgnoreCase))
			{
				return "ANNUAL EFFICIENCY (PERCENT)";
			}
			else if (field.Equals("IrrigatedAcres", StringComparison.OrdinalIgnoreCase))
			{
				return "WELL IRRIGATED AREA (ACRE)";
			}
			else if (field.Equals("UseType", StringComparison.OrdinalIgnoreCase))
			{
				return "USE TYPE";
			}
			else if (field.Equals("DemandSource", StringComparison.OrdinalIgnoreCase))
			{
				return "DEMAND SOURCE";
			}
			else if (field.Equals("EffMonthly01", StringComparison.OrdinalIgnoreCase))
			{
				return "EFFICIENCY MONTH 1 (PERCENT)";
			}
			else if (field.Equals("EffMonthly02", StringComparison.OrdinalIgnoreCase))
			{
				return "EFFICIENCY MONTH 2 (PERCENT)";
			}
			else if (field.Equals("EffMonthly03", StringComparison.OrdinalIgnoreCase))
			{
				return "EFFICIENCY MONTH 3 (PERCENT)";
			}
			else if (field.Equals("EffMonthly04", StringComparison.OrdinalIgnoreCase))
			{
				return "EFFICIENCY MONTH 4 (PERCENT)";
			}
			else if (field.Equals("EffMonthly05", StringComparison.OrdinalIgnoreCase))
			{
				return "EFFICIENCY MONTH 5 (PERCENT)";
			}
			else if (field.Equals("EffMonthly06", StringComparison.OrdinalIgnoreCase))
			{
				return "EFFICIENCY MONTH 6 (PERCENT)";
			}
			else if (field.Equals("EffMonthly07", StringComparison.OrdinalIgnoreCase))
			{
				return "EFFICIENCY MONTH 7 (PERCENT)";
			}
			else if (field.Equals("EffMonthly08", StringComparison.OrdinalIgnoreCase))
			{
				return "EFFICIENCY MONTH 8 (PERCENT)";
			}
			else if (field.Equals("EffMonthly09", StringComparison.OrdinalIgnoreCase))
			{
				return "EFFICIENCY MONTH 9 (PERCENT)";
			}
			else if (field.Equals("EffMonthly10", StringComparison.OrdinalIgnoreCase))
			{
				return "EFFICIENCY MONTH 10 (PERCENT)";
			}
			else if (field.Equals("EffMonthly11", StringComparison.OrdinalIgnoreCase))
			{
				return "EFFICIENCY MONTH 11 (PERCENT)";
			}
			else if (field.Equals("EffMonthly12", StringComparison.OrdinalIgnoreCase))
			{
				return "EFFICIENCY MONTH 12 (PERCENT)";
			}
		}
		else if (propType.Equals("FieldNameHeader", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "\n\nID";
			}
			else if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "\n\nNAME";
			}
			else if (field.Equals("RiverNodeID", StringComparison.OrdinalIgnoreCase))
			{
				return "\nRIVER\nNODE ID";
			}
			else if (field.Equals("OnOff", StringComparison.OrdinalIgnoreCase))
			{
				return "\nON/OFF\nSWITCH";
			}
			else if (field.Equals("Capacity", StringComparison.OrdinalIgnoreCase))
			{
				return "\nCAPACITY\n(CFS)\n";
			}
			else if (field.Equals("DailyID", StringComparison.OrdinalIgnoreCase))
			{
				return "\nDAILY\nID";
			}
			else if (field.Equals("Primary", StringComparison.OrdinalIgnoreCase))
			{
				return "ADMINISTRATION\nNUMBER\nSWITCH";
			}
			else if (field.Equals("DiversionID", StringComparison.OrdinalIgnoreCase))
			{
				return "\nRELATED\nDIVERSION ID";
			}
			else if (field.Equals("DemandType", StringComparison.OrdinalIgnoreCase))
			{
				return "\nDATA\nTYPE";
			}
			else if (field.Equals("EffAnnual", StringComparison.OrdinalIgnoreCase))
			{
				return "ANNUAL\nEFFICIENCY\n(PERCENT)";
			}
			else if (field.Equals("IrrigatedAcres", StringComparison.OrdinalIgnoreCase))
			{
				return "WELL\nIRRIGATED\nAREA (ACRE)";
			}
			else if (field.Equals("UseType", StringComparison.OrdinalIgnoreCase))
			{
				return "\nUSE\nTYPE";
			}
			else if (field.Equals("DemandSource", StringComparison.OrdinalIgnoreCase))
			{
				return "\nDEMAND\nSOURCE";
			}
			else if (field.Equals("EffMonthly01", StringComparison.OrdinalIgnoreCase))
			{
				return "EFFICIENCY\nMONTH 1\n(PERCENT)";
			}
			else if (field.Equals("EffMonthly02", StringComparison.OrdinalIgnoreCase))
			{
				return "EFFICIENCY\nMONTH 2\n(PERCENT)";
			}
			else if (field.Equals("EffMonthly03", StringComparison.OrdinalIgnoreCase))
			{
				return "EFFICIENCY\nMONTH 3\n(PERCENT)";
			}
			else if (field.Equals("EffMonthly04", StringComparison.OrdinalIgnoreCase))
			{
				return "EFFICIENCY\nMONTH 4\n(PERCENT)";
			}
			else if (field.Equals("EffMonthly05", StringComparison.OrdinalIgnoreCase))
			{
				return "EFFICIENCY\nMONTH 5\n(PERCENT)";
			}
			else if (field.Equals("EffMonthly06", StringComparison.OrdinalIgnoreCase))
			{
				return "EFFICIENCY\nMONTH 6\n(PERCENT)";
			}
			else if (field.Equals("EffMonthly07", StringComparison.OrdinalIgnoreCase))
			{
				return "EFFICIENCY\nMONTH 7\n(PERCENT)";
			}
			else if (field.Equals("EffMonthly08", StringComparison.OrdinalIgnoreCase))
			{
				return "EFFICIENCY\nMONTH 8\n(PERCENT)";
			}
			else if (field.Equals("EffMonthly09", StringComparison.OrdinalIgnoreCase))
			{
				return "EFFICIENCY\nMONTH 9\n(PERCENT)";
			}
			else if (field.Equals("EffMonthly10", StringComparison.OrdinalIgnoreCase))
			{
				return "EFFICIENCY\nMONTH 10\n(PERCENT)";
			}
			else if (field.Equals("EffMonthly11", StringComparison.OrdinalIgnoreCase))
			{
				return "EFFICIENCY\nMONTH 11\n(PERCENT)";
			}
			else if (field.Equals("EffMonthly12", StringComparison.OrdinalIgnoreCase))
			{
				return "EFFICIENCY\nMONTH 12\n(PERCENT)";
			}
		}
		else if (propType.Equals("Format", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
			else if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "%-24.24s";
			}
			else if (field.Equals("RiverNodeID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
			else if (field.Equals("OnOff", StringComparison.OrdinalIgnoreCase))
			{
				return "%8d";
			}
			else if (field.Equals("Capacity", StringComparison.OrdinalIgnoreCase))
			{
				return "%10.2f";
			}
			else if (field.Equals("DailyID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
			else if (field.Equals("Primary", StringComparison.OrdinalIgnoreCase))
			{
				return "%15.5f";
			}
			else if (field.Equals("DiversionID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
			else if (field.Equals("DemandType", StringComparison.OrdinalIgnoreCase))
			{
				return "%8d";
			}
			else if (field.Equals("EffAnnual", StringComparison.OrdinalIgnoreCase))
			{
				return "%10.2f";
			}
			else if (field.Equals("IrrigatedAcres", StringComparison.OrdinalIgnoreCase))
			{
				return "%10.2f";
			}
			else if (field.Equals("UseType", StringComparison.OrdinalIgnoreCase))
			{
				return "%8d";
			}
			else if (field.Equals("DemandSource", StringComparison.OrdinalIgnoreCase))
			{
				return "%8d";
			}
			else if (field.Equals("EffMonthly01", StringComparison.OrdinalIgnoreCase))
			{
				return "%10.2f";
			}
			else if (field.Equals("EffMonthly02", StringComparison.OrdinalIgnoreCase))
			{
				return "%10.2f";
			}
			else if (field.Equals("EffMonthly03", StringComparison.OrdinalIgnoreCase))
			{
				return "%10.2f";
			}
			else if (field.Equals("EffMonthly04", StringComparison.OrdinalIgnoreCase))
			{
				return "%10.2f";
			}
			else if (field.Equals("EffMonthly05", StringComparison.OrdinalIgnoreCase))
			{
				return "%10.2f";
			}
			else if (field.Equals("EffMonthly06", StringComparison.OrdinalIgnoreCase))
			{
				return "%10.2f";
			}
			else if (field.Equals("EffMonthly07", StringComparison.OrdinalIgnoreCase))
			{
				return "%10.2f";
			}
			else if (field.Equals("EffMonthly08", StringComparison.OrdinalIgnoreCase))
			{
				return "%10.2f";
			}
			else if (field.Equals("EffMonthly09", StringComparison.OrdinalIgnoreCase))
			{
				return "%10.2f";
			}
			else if (field.Equals("EffMonthly10", StringComparison.OrdinalIgnoreCase))
			{
				return "%10.2f";
			}
			else if (field.Equals("EffMonthly11", StringComparison.OrdinalIgnoreCase))
			{
				return "%10.2f";
			}
			else if (field.Equals("EffMonthly12", StringComparison.OrdinalIgnoreCase))
			{
				return "%10.2f";
			}
		}
		else if (propType.Equals("ToolTip", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "<html>The well station identifier is the main link between well data<BR>and must be "
					+ "unique in the data set.</html>";
			}
			else if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "Well station name.";
			}
			else if (field.Equals("RiverNodeID", StringComparison.OrdinalIgnoreCase))
			{
				return "River node where well station is located.";
			}
			else if (field.Equals("OnOff", StringComparison.OrdinalIgnoreCase))
			{
				return "Indicates whether well station is on (1) or off (0)";
			}
			else if (field.Equals("Capacity", StringComparison.OrdinalIgnoreCase))
			{
				return "<html>Well capacity (CFS)</html>";
			}
			else if (field.Equals("DailyID", StringComparison.OrdinalIgnoreCase))
			{
				return "<html>Well ID to use for daily data.</html>";
			}
			else if (field.Equals("Primary", StringComparison.OrdinalIgnoreCase))
			{
				return "<html>Priority switch.</html>";
			}
			else if (field.Equals("DiversionID", StringComparison.OrdinalIgnoreCase))
			{
				return "<html>Diversion this well is tied to.</html>";
			}
			else if (field.Equals("DemandType", StringComparison.OrdinalIgnoreCase))
			{
				return "<html>Demand code.</html>";
			}
			else if (field.Equals("EffAnnual", StringComparison.OrdinalIgnoreCase))
			{
				return "<html>System efficiency (%).</html>";
			}
			else if (field.Equals("IrrigatedAcres", StringComparison.OrdinalIgnoreCase))
			{
				return "<html>Irrigated area associated with the well.</html>";
			}
			else if (field.Equals("UseType", StringComparison.OrdinalIgnoreCase))
			{
				return "<html>Use type.</html>";
			}
			else if (field.Equals("DemandSource", StringComparison.OrdinalIgnoreCase))
			{
				return "<html>Irrigated acreage source.</html>";
			}
			else if (field.Equals("EffMonthly01", StringComparison.OrdinalIgnoreCase))
			{
				return "Well efficiency for month 1 of year.";
			}
			else if (field.Equals("EffMonthly02", StringComparison.OrdinalIgnoreCase))
			{
				return "Well efficiency for month 2 of year.";
			}
			else if (field.Equals("EffMonthly03", StringComparison.OrdinalIgnoreCase))
			{
				return "Well efficiency for month 3 of year.";
			}
			else if (field.Equals("EffMonthly04", StringComparison.OrdinalIgnoreCase))
			{
				return "Well efficiency for month 4 of year.";
			}
			else if (field.Equals("EffMonthly05", StringComparison.OrdinalIgnoreCase))
			{
				return "Well efficiency for month 5 of year.";
			}
			else if (field.Equals("EffMonthly06", StringComparison.OrdinalIgnoreCase))
			{
				return "Well efficiency for month 6 of year.";
			}
			else if (field.Equals("EffMonthly07", StringComparison.OrdinalIgnoreCase))
			{
				return "Well efficiency for month 7 of year.";
			}
			else if (field.Equals("EffMonthly08", StringComparison.OrdinalIgnoreCase))
			{
				return "Well efficiency for month 8 of year.";
			}
			else if (field.Equals("EffMonthly09", StringComparison.OrdinalIgnoreCase))
			{
				return "Well efficiency for month 9 of year.";
			}
			else if (field.Equals("EffMonthly10", StringComparison.OrdinalIgnoreCase))
			{
				return "Well efficiency for month 10 of year.";
			}
			else if (field.Equals("EffMonthly11", StringComparison.OrdinalIgnoreCase))
			{
				return "Well efficiency for month 11 of year.";
			}
			else if (field.Equals("EffMonthly12", StringComparison.OrdinalIgnoreCase))
			{
				return "Well efficiency for month 12 of year.";
			}
		}
		return null;
	}

	/// <summary>
	/// Returns property values for well collections. </summary>
	/// <param name="propType"> the property to look up.  One of "FieldName", "FieldNameHeader", "ToolTip", or "Format". </param>
	/// <param name="field"> the field for which to return the property. </param>
	/// <returns> the property, or if it could not be found null will be returned. </returns>
	private static string lookupWellCollectionPropValue(string propType, string field)
	{
		if (propType.Equals("FieldName", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("LocationID", StringComparison.OrdinalIgnoreCase))
			{
				return "WELL ID";
			}
			else if (field.Equals("Year", StringComparison.OrdinalIgnoreCase))
			{
				return "YEAR";
			}
			else if (field.Equals("CollectionType", StringComparison.OrdinalIgnoreCase))
			{
				return "COLLECTION TYPE";
			}
			else if (field.Equals("PartType", StringComparison.OrdinalIgnoreCase))
			{
				return "PART TYPE";
			}
			else if (field.Equals("PartID", StringComparison.OrdinalIgnoreCase))
			{
				return "PART ID";
			}
			else if (field.Equals("PartIDType", StringComparison.OrdinalIgnoreCase))
			{
				return "PART ID TYPE";
			}
		}
		else if (propType.Equals("FieldNameHeader", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("LocationID", StringComparison.OrdinalIgnoreCase))
			{
				return "WELL\nID";
			}
			else if (field.Equals("Year", StringComparison.OrdinalIgnoreCase))
			{
				return "\nYEAR";
			}
			else if (field.Equals("CollectionType", StringComparison.OrdinalIgnoreCase))
			{
				return "COLLECTION\nTYPE";
			}
			else if (field.Equals("PartType", StringComparison.OrdinalIgnoreCase))
			{
				return "PART\nTYPE";
			}
			else if (field.Equals("PartID", StringComparison.OrdinalIgnoreCase))
			{
				return "PART\nID";
			}
			else if (field.Equals("PartIDType", StringComparison.OrdinalIgnoreCase))
			{
				return "PART\nID TYPE";
			}
		}
		else if (propType.Equals("Format", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("LocationID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
			else if (field.Equals("Year", StringComparison.OrdinalIgnoreCase))
			{
				return "%8d";
			}
			else if (field.Equals("CollectionType", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
			else if (field.Equals("PartType", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
			else if (field.Equals("PartID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
			else if (field.Equals("PartIDType", StringComparison.OrdinalIgnoreCase))
			{
				return "%-7.7s";
			}
		}
		else if (propType.Equals("ToolTip", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("LocationID", StringComparison.OrdinalIgnoreCase))
			{
				return "StateMod well location ID for aggregate/system";
			}
			else if (field.Equals("Year", StringComparison.OrdinalIgnoreCase))
			{
				return "Year for aggregate/system (used when aggregating parcels)";
			}
			else if (field.Equals("Division", StringComparison.OrdinalIgnoreCase))
			{
				return "Water division for aggregate/system (used when aggregating using parcel IDs)";
			}
			else if (field.Equals("CollectionType", StringComparison.OrdinalIgnoreCase))
			{
				return "Aggregate (aggregate water rights) or system (consider water rights individually)";
			}
			else if (field.Equals("PartType", StringComparison.OrdinalIgnoreCase))
			{
				return "Ditch, Well, or Parcel identifiers are specified as parts of aggregate/system";
			}
			else if (field.Equals("PartID", StringComparison.OrdinalIgnoreCase))
			{
				return "The identifier for the aggregate/system parts";
			}
			else if (field.Equals("PartIDType", StringComparison.OrdinalIgnoreCase))
			{
				return "The identifier type for the aggregate/system, WDID or Receipt when applied to wells";
			}
		}
		return null;
	}

	/// <summary>
	/// Returns property values for well depletions. </summary>
	/// <param name="propType"> the property to look up.  One of "FieldName", "FieldNameHeader", "ToolTip", or "Format". </param>
	/// <param name="field"> the field for which to return the property. </param>
	/// <returns> the property, or if it could not be found null will be returned. </returns>
	private static string lookupWellDepletionPropValue(string propType, string field)
	{
		if (propType.Equals("FieldName", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "WELL ID";
			}
			else if (field.Equals("RiverNodeID", StringComparison.OrdinalIgnoreCase))
			{
				return "RIVER NODE BEING DEPLETED";
			}
			else if (field.Equals("DepletionPercent", StringComparison.OrdinalIgnoreCase))
			{
				return "PERCENT";
			}
			else if (field.Equals("DelayTableID", StringComparison.OrdinalIgnoreCase))
			{
				return "DELAY TABLE ID";
			}
			else if (field.Equals("Comment", StringComparison.OrdinalIgnoreCase))
			{
				return "COMMENT";
			}
		}
		else if (propType.Equals("FieldNameHeader", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "\nWELL\nSTATION\nID";
			}
			else if (field.Equals("RiverNodeID", StringComparison.OrdinalIgnoreCase))
			{
				return "RIVER\nNODE ID\nBEING\nDEPLETED";
			}
			else if (field.Equals("DepletionPercent", StringComparison.OrdinalIgnoreCase))
			{
				return "\n\n\nPERCENT";
			}
			else if (field.Equals("DelayTableID", StringComparison.OrdinalIgnoreCase))
			{
				return "\n\nDELAY\nTABLE ID";
			}
			else if (field.Equals("Comment", StringComparison.OrdinalIgnoreCase))
			{
				return "\n\n\nCOMMENT";
			}
		}
		else if (propType.Equals("Format", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
			else if (field.Equals("RiverNodeID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
			else if (field.Equals("DepletionPercent", StringComparison.OrdinalIgnoreCase))
			{
				return "%12.6f";
			}
			else if (field.Equals("DelayTableID", StringComparison.OrdinalIgnoreCase))
			{
				return "%8d";
			}
			else if (field.Equals("Comment", StringComparison.OrdinalIgnoreCase))
			{
				return "%s";
			}
		}
		else if (propType.Equals("ToolTip", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "Associated well station ID.";
			}
			else if (field.Equals("RiverNodeID", StringComparison.OrdinalIgnoreCase))
			{
				return "River node ID being depleted.";
			}
			else if (field.Equals("DepletionPercent", StringComparison.OrdinalIgnoreCase))
			{
				return "% of depletion (0-100)";
			}
			else if (field.Equals("DelayTableID", StringComparison.OrdinalIgnoreCase))
			{
				return "Delay table identifier";
			}
			else if (field.Equals("Comment", StringComparison.OrdinalIgnoreCase))
			{
				return "Explanation of delay table assignment";
			}
		}
		return null;
	}

	/// <summary>
	/// Returns property values for well return flows. </summary>
	/// <param name="propType"> the property to look up.  One of "FieldName", "FieldNameHeader", "ToolTip", or "Format". </param>
	/// <param name="field"> the field for which to return the property. </param>
	/// <returns> the property, or if it could not be found null will be returned. </returns>
	private static string lookupWellReturnFlowPropValue(string propType, string field)
	{
		if (propType.Equals("FieldName", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "WELL ID";
			}
			else if (field.Equals("RiverNodeID", StringComparison.OrdinalIgnoreCase))
			{
				return "RIVER NODE ID RECEIVING RETURN FLOW";
			}
			else if (field.Equals("ReturnPercent", StringComparison.OrdinalIgnoreCase))
			{
				return "PERCENT";
			}
			else if (field.Equals("DelayTableID", StringComparison.OrdinalIgnoreCase))
			{
				return "DELAY TABLE ID";
			}
			else if (field.Equals("Comment", StringComparison.OrdinalIgnoreCase))
			{
				return "COMMENT";
			}
		}
		else if (propType.Equals("FieldNameHeader", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "\n\nWELL\nSTATION\nID";
			}
			else if (field.Equals("RiverNodeID", StringComparison.OrdinalIgnoreCase))
			{
				return "RIVER\nNODE ID\nRECEIVING\nRETURN\nFLOW";
			}
			else if (field.Equals("ReturnPercent", StringComparison.OrdinalIgnoreCase))
			{
				return "\n\n\n\nPERCENT";
			}
			else if (field.Equals("DelayTableID", StringComparison.OrdinalIgnoreCase))
			{
				return "\n\n\nDELAY\nTABLE ID";
			}
			else if (field.Equals("Comment", StringComparison.OrdinalIgnoreCase))
			{
				return "\n\n\n\nCOMMENT";
			}
		}
		else if (propType.Equals("Format", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
			else if (field.Equals("RiverNodeID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
			else if (field.Equals("ReturnPercent", StringComparison.OrdinalIgnoreCase))
			{
				return "%12.6f";
			}
			else if (field.Equals("DelayTableID", StringComparison.OrdinalIgnoreCase))
			{
				return "%8d";
			}
			else if (field.Equals("Comment", StringComparison.OrdinalIgnoreCase))
			{
				return "%s";
			}
		}
		else if (propType.Equals("ToolTip", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "Associated well station ID.";
			}
			else if (field.Equals("RiverNodeID", StringComparison.OrdinalIgnoreCase))
			{
				return "River node ID receiving return flow.";
			}
			else if (field.Equals("ReturnPercent", StringComparison.OrdinalIgnoreCase))
			{
				return "% of return (0-100)";
			}
			else if (field.Equals("DelayTableID", StringComparison.OrdinalIgnoreCase))
			{
				return "Delay table identifier";
			}
			else if (field.Equals("Comment", StringComparison.OrdinalIgnoreCase))
			{
				return "Explanation of delay assignment";
			}
		}
		return null;
	}

	/// <summary>
	/// Returns property values for well rights. </summary>
	/// <param name="propType"> the property to look up.  One of "FieldName", "FieldNameHeader", "ToolTip", or "Format". </param>
	/// <param name="field"> the field for which to return the property. </param>
	/// <returns> the property, or if it could not be found null will be returned. </returns>
	private static string lookupWellRightPropValue(string propType, string field)
	{
		if (propType.Equals("FieldName", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "RIGHT ID";
			}
			else if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "WELL RIGHT NAME";
			}
			else if (field.Equals("StationID", StringComparison.OrdinalIgnoreCase))
			{
				return "WELL ID ASSOCIATED W/ RIGHT";
			}
			else if (field.Equals("AdministrationNumber", StringComparison.OrdinalIgnoreCase))
			{
				return "ADMINISTRATION NUMBER";
			}
			else if (field.Equals("Decree", StringComparison.OrdinalIgnoreCase))
			{
				return "DECREED AMOUNT (CFS)";
			}
			else if (field.Equals("OnOff", StringComparison.OrdinalIgnoreCase))
			{
				return "ON/OFF SWITCH";
			}
		}
		else if (propType.Equals("FieldNameHeader", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "\n\nRIGHT ID";
			}
			else if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "\n\nWELL RIGHT NAME";
			}
			else if (field.Equals("StationID", StringComparison.OrdinalIgnoreCase))
			{
				return "WELL ID\nASSOCIATED\nW/ RIGHT";
			}
			else if (field.Equals("AdministrationNumber", StringComparison.OrdinalIgnoreCase))
			{
				return "\nADMINISTRATION\nNUMBER";
			}
			else if (field.Equals("Decree", StringComparison.OrdinalIgnoreCase))
			{
				return "\nDECREED\nAMOUNT (CFS)";
			}
			else if (field.Equals("OnOff", StringComparison.OrdinalIgnoreCase))
			{
				return "\nON/OFF\nSWITCH";
			}
		}
		else if (propType.Equals("Format", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
			else if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "%-24.24s";
			}
			else if (field.Equals("StationID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
			else if (field.Equals("AdministrationNumber", StringComparison.OrdinalIgnoreCase))
			{
				return "%-12.12s";
			}
			else if (field.Equals("Decree", StringComparison.OrdinalIgnoreCase))
			{
				return "%12.2f";
			}
			else if (field.Equals("OnOff", StringComparison.OrdinalIgnoreCase))
			{
				return "%8d";
			}
		}
		else if (propType.Equals("ToolTip", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "<html>The well right ID is typically the well station ID<br> followed by .01, .02, "
					+ "etc.</html>";
			}
			else if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "Well right name";
			}
			else if (field.Equals("StationID", StringComparison.OrdinalIgnoreCase))
			{
				return "<HTML>The well ID is the link between well stations and their right(s).</HTML>";
			}
			else if (field.Equals("AdministrationNumber", StringComparison.OrdinalIgnoreCase))
			{
				return "<HTML>Lower admininistration numbers indicate "
					+ "greater seniority.<BR>99999 is typical for a very junior right.</html>";
			}
			else if (field.Equals("Decree", StringComparison.OrdinalIgnoreCase))
			{
				return "Decreed amount (CFS)";
			}
			else if (field.Equals("OnOff", StringComparison.OrdinalIgnoreCase))
			{
				return "<HTML>0 = OFF<BR>1 = ON<BR>YYYY indicates to turn on the right in "
					+ "year YYYY.<BR>-YYYY indicates to turn off the right in year YYYY.</HTML>";
			}
		}
		return null;
	}

	/// <summary>
	/// Removes all the objects that match the specified object (with a compareTo() call) from the Vector. </summary>
	/// <param name="v"> the Vector from which to remove the element. </param>
	/// <param name="data"> the object to match and remove. </param>
	public static void removeFromVector(System.Collections.IList v, StateMod_Data data)
	{
		if (v == null || v.Count == 0)
		{
			return;
		}
		int size = v.Count;
		StateMod_Data element = null;
		for (int i = size - 1; i >= 0; i--)
		{
			element = (StateMod_Data)v[i];
			if (element.CompareTo(data) == 0)
			{
				v.RemoveAt(i);
			}
		}
	}

	/// <summary>
	/// Run the command "statemod <response_file_name> <option>". </summary>
	/// <param name="dataset"> Data set to get the response file from. </param>
	/// <param name="option"> Option to run (e.g., "-simx" for a fast simulate). </param>
	/// <param name="withGUI"> If true, the process manager gui will be displayed.  True should
	/// typcially be used for model run options but is normally false when running the StateMod report mode. </param>
	/// <param name="parent"> Calling JFrame, used when withGUI is true. </param>
	/// <exception cref="Exception"> if there is an error running the command (non Stop 0 from StateMod). </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void runStateMod(StateMod_DataSet dataset, String option, boolean withGUI, javax.swing.JFrame parent) throws Exception
	public static void runStateMod(StateMod_DataSet dataset, string option, bool withGUI, JFrame parent)
	{
		string responseFile = null;
		if (dataset != null)
		{
			DataSetComponent comp = dataset.getComponentForComponentType(StateMod_DataSet.COMP_RESPONSE);
			responseFile = dataset.getDataFilePathAbsolute(comp.getDataFileName());
		}
		runStateMod(responseFile, option, withGUI, parent, 0);
	}

	/// <summary>
	/// Run the command "statemod <response_file_name> <option>".
	/// The response file is typically the original response file that was used to open the data set. </summary>
	/// <param name="response_file_name"> Response file name, with full path. </param>
	/// <param name="option"> Option to run (e.g., "-simx" for a fast simulate). </param>
	/// <param name="withGUI"> If true, the process manager gui will be displayed.  True should
	/// typcially be used for model run options but is normally false when running the StateMod report mode. </param>
	/// <param name="parent"> Calling JFrame, used when withGUI is true. </param>
	/// <exception cref="Exception"> if there is an error running the command (non Stop 0 from StateMod). </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void runStateMod(String response_file_name, String option, boolean withGUI, javax.swing.JFrame parent) throws Exception
	public static void runStateMod(string response_file_name, string option, bool withGUI, JFrame parent)
	{
		runStateMod(response_file_name, option, withGUI, parent, 0);
	}

	/// <summary>
	/// Run the command "statemod <response_file_name> <option>".
	/// The response file is typically the original response file that was used to open
	/// the data set.  It can be null if not needed for the option (e.g., -help).  If the -v
	/// option is run, then the internal statemod version information is set.  In other
	/// words, use -v to reset the StateMod version number that is known. </summary>
	/// <param name="response_file_name"> Response file name, with full path. </param>
	/// <param name="option"> Option to run(parameters after the program name). </param>
	/// <param name="withGUI"> If true, a ProcessManagerDialog will be used.  If false, the GUI
	/// will not be shown (although a DOS window may pop up). </param>
	/// <param name="parent"> Calling JFrame, used when withGUI is true. </param>
	/// <param name="wait_after"> Number of milliseconds to wait after running.  This is
	/// sometimes needed to allow the output file (e.g., .x*g) file to be recognized
	/// by the operating system.  This will not be needed if time series are read from
	/// binary model output files but may be needed if reports are viewed immediately after running. </param>
	/// <exception cref="Exception"> if there is an error running the command (non Stop 0 from StateMod). </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void runStateMod(String response_file_name, String option, boolean withGUI, javax.swing.JFrame parent, int wait_after) throws Exception
	public static void runStateMod(string response_file_name, string option, bool withGUI, JFrame parent, int wait_after)
	{
		string routine = "StateMod_Util.runStateMod";
		string workingDir = null;
		if (string.ReferenceEquals(response_file_name, null))
		{
			response_file_name = "";
		}
		else
		{
			File file = new File(response_file_name);
			workingDir = file.getParent();
		}
		//Message.printStatus(2, routine, "Running StateMod, response_file_name=\"" + response_file_name +
		//	"\" workingDir=\"" + workingDir + "\" option=\"" + option + "\" withGUI=" + withGUI + " wait_after=" + wait_after );

		string stateModExecutable = getStateModExecutable(IOUtil.getApplicationHomeDir(), workingDir);
		string command = stateModExecutable + " " + response_file_name + " " + option;
		string str;

		Message.printStatus(1, routine, "Running \"" + command + "\"");

		if (withGUI)
		{
			// Run using a process manager dialog...
			PropList props = new PropList("StateMod_Util.runStateMod");
			PropList pm_props = new PropList("StateMod_Util.runStateMod");
			if (option.StartsWith("-update", StringComparison.Ordinal) || option.StartsWith("-help", StringComparison.Ordinal) || option.StartsWith("-version", StringComparison.Ordinal))
			{
				// Display all the output...
				props.set("BufferSize","0");
			}
			else
			{
				// Display a reasonably large number of lines of output...
				props.set("BufferSize","1000");
			}
			// Tell the process manager to check for "STOP" as an exit
			// code - this seems to work better than a process exit code.
			// Comment out for now.  The exit() calls built into the
			// statemod.exe seem to be working
			//pm_props.set("ExitStatusTokens","STOP");
			// Use the following to test the Statemod.exe with exit() codes
			// Use the array of command arguments - seems to work better...
			//String [] test = new String[3];
			//test[0] = "statemod";
			//test[1] = "-test";
			//test[2] = "-0";
			//new ProcessManagerJDialog(parent, "StateMod", new ProcessManager( test, pm_props), props);
			ProcessManagerOutputFilter filter = new StateMod_OutputFilter();
			new ProcessManagerJDialog(parent, "StateMod", new ProcessManager(StringUtil.toArray(StringUtil.breakStringList(command, " ", StringUtil.DELIM_SKIP_BLANKS)), pm_props), filter, props);
		}
		else if (option.Equals("-v", StringComparison.OrdinalIgnoreCase) || option.Equals("-version", StringComparison.OrdinalIgnoreCase))
		{
			// Run StateMod -version and search the output for the version information...
			string[] command_array = new string[2];
			command_array[0] = stateModExecutable;
			command_array[1] = "-version";
			ProcessManager sp = new ProcessManager(command_array);
			sp.saveOutput(true);
			sp.run();
			IList<string> output = sp.getOutputList();
			int size = 0;
			if (output != null)
			{
				size = output.Count;
			}
			bool versionFound = false;
			for (int i = 0; i < size; i++)
			{
				str = output[i];
				if (str.IndexOf("Version:", StringComparison.Ordinal) >= 0)
				{
					string version = StringUtil.getToken(str.Trim(),":", StringUtil.DELIM_SKIP_BLANKS,1).Trim();
					// For now treat as a floating point number...
					setStateModVersion(version);
					versionFound = true;
				}
				if (str.IndexOf("revision date:", StringComparison.Ordinal) >= 0)
				{
					string revisionDate = StringUtil.getToken(str.Trim(),":", StringUtil.DELIM_SKIP_BLANKS,1).Trim();
					setStateModRevisionDate(revisionDate);
				}
			}
			if (!versionFound)
			{
				Message.printWarning(1, routine, "Unable to determine StateMod version from version output using:\n" + "   " + stateModExecutable + "\n" + "StateMod may not run and output may not be accessible.\n" + "Is statemod.exe in the PATH or specified as a full path (see Tools ... Options)?");
				return; // To skip sleep below.
			}
		}
		else
		{
			// No GUI and not getting the version.
			ProcessManager sp = new ProcessManager(StringUtil.toArray(StringUtil.breakStringList(command, " \t", StringUtil.DELIM_SKIP_BLANKS)));
			sp.saveOutput(true);
			sp.run();
			if (sp.getExitStatus() != 0)
			{
				// There was an error running StateMod.
				Message.printWarning(2, routine, "Error running \"" + command + "\"");
				throw new Exception("Error running \"" + command + "\"");
			}
			System.Collections.IList output = sp.getOutputList();
			int size = 0;
			if (output != null)
			{
				size = output.Count;
			}
			for (int i = 0; i < size; i++)
			{
				// Print the output as status messages since no GUI is
				// being shown but we may want to see what is going on in the log and console...
				str = (string)output[i];
				if (!string.ReferenceEquals(str, null))
				{
					Message.printStatus(1, routine, str);
				}
			}
			// Appears that in some cases the model is not completing saving its output so sleep .1 second...
			if (wait_after > 0)
			{
				Message.printStatus(1,"", "Waiting " + wait_after + " milliseconds to let output finish.");
				TimeUtil.sleep(wait_after);
				Message.printStatus(1,"", "Done waiting.");
			}
		}
	}

	/// <summary>
	/// Set the program to use when running SmDelta.  In general, this should just be
	/// the program name and rely on the PATH to find.  However, a full path can be
	/// specified to override the PATH. </summary>
	/// <param name="smdeltaExecutable"> name of StateMod executable to run. </param>
	public static void setSmDeltaExecutable(string smdeltaExecutable)
	{
		if (!string.ReferenceEquals(smdeltaExecutable, null))
		{
			__smdeltaExecutable = smdeltaExecutable;
		}
	}

	/// <summary>
	/// Set the program to use when running StateMod.  In general, this should just be
	/// the program name and rely on the PATH to find.  However, a full path can be specified to override the PATH. </summary>
	/// <param name="statemodExecutable"> name of StateMod executable to run. </param>
	public static void setStateModExecutable(string statemodExecutable)
	{
		if (!string.ReferenceEquals(statemodExecutable, null))
		{
			__statemodExecutable = statemodExecutable;
		}
	}

	/// <summary>
	/// Set the StateMod revision date. </summary>
	/// <param name="statemodRevisionDate"> Revision date string from running statemod -v </param>
	public static void setStateModRevisionDate(string statemodRevisionDate)
	{
		if (!string.ReferenceEquals(statemodRevisionDate, null))
		{
			__statemodRevisionDate = statemodRevisionDate;
		}
	}

	/// <summary>
	/// Set the StateMod version, for internal use.  This information is useful
	/// for checking version "greater than" for software features and file formats, etc. </summary>
	/// <param name="statemodVersion"> StateMod version as a string. </param>
	private static void setStateModVersion(string statemodVersion)
	{
		__statemodVersion = statemodVersion;
	}

	/// <summary>
	/// Sorts a list of StateMod_Data objects, depending on the compareTo() method for the specific object. </summary>
	/// <param name="data"> a list of StateMod_Data objects.  Can be null. </param>
	/// <returns> a new sorted list with references to the same data objects in the
	/// passed-in list.  If a null Vector is passed in, an empty list will be returned. </returns>
	public static IList<T> sortStateMod_DataVector<T>(IList<T> data)
	{
		return sortStateMod_DataVector(data, true);
	}

	/// <summary>
	/// Sorts a list of StateMod_Data objects, depending on the compareTo() method for the specific object. </summary>
	/// <param name="data"> a list of StateMod_Data objects.  Can be null. </param>
	/// <param name="returnNew"> If true, return a new list with references to the data.
	/// If false, return the original list, with sorted contents. </param>
	/// <returns> a sorted list with references to the same data objects in the
	/// passed-in list.  If null is passed in, an empty list will be returned. </returns>
	public static IList<T> sortStateMod_DataVector<T>(IList<T> data, bool returnNew)
	{
		if (data == null)
		{
			return new List<T>();
		}
		System.Collections.IList dataSorted = data;
		int size = data.Count;
		if (returnNew)
		{
			if (size == 0)
			{
				return new List<T>();
			}
			dataSorted = new List<T>(data);
		}

		dataSorted.Sort();
		return dataSorted;
	}

	/// <summary>
	/// Sets description field in each time series using supplied StateMod_Data object
	/// identifiers.  The StateMod time series files include only the start/end period
	/// of record, units, year type, ID and values only, no descriptions are included.
	/// This method correlates the descriptions in the stations files with the time series. </summary>
	/// <param name="theData"> StateMod_Data objects from which we will use the name is used 
	/// to fill in the description field in the time series </param>
	/// <param name="theTS"> vector of time series </param>
	/// <param name="mult"> the number of TS in a row which will use the description.  For
	/// example, the reservoir min/max vector has two time series for each node in
	/// theData (min and max) whereas most have a one-to-one correlation. </param>
	public static void setTSDescriptions(IList<StateMod_Data> theData, IList<TS> theTS, int mult)
	{
		if ((theData == null) || (theTS == null))
		{
			return;
		}
		if (theData.Count == 0 || theTS.Count == 0)
		{
			return;
		}

		int size = theData.Count;

		StateMod_Data smdata = null;
		TS ts = null;
		for (int i = 0; i < size; i++)
		{
			smdata = theData[i];
			for (int j = 0; j < mult; j++)
			{
				try
				{
					ts = theTS[i * mult + j];

					if (smdata.getID().Equals(ts.getIdentifier().getLocation(), StringComparison.OrdinalIgnoreCase))
					{
						ts.setDescription(smdata.getName());
					}
				}
				catch (Exception)
				{
					Message.printWarning(2,"StateMod_GUIUtil.setTSDescriptions", "Unable to set description for ts");
				}
			}

		}
	}

	/// <summary>
	/// Validate a time series, similar to validating other components.  This method is needed because time series
	/// objects do not themselves include a validation method like StateMod components.  The following checks are
	/// performed:
	/// <ol>
	/// <li>	
	/// </ol> </summary>
	/// <param name="validation"> if non null, validation results will be appended.  If null, a validation object will be
	/// created for returned information. </param>
	/// <param name="ts"> time series to validate </param>
	/// <param name="stationList"> List of stations, to make sure that time series matches a station (if null or empty the
	/// cross check is ignored). </param>
	/// <returns> validation results </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static StateMod_ComponentValidation validateTimeSeries(StateMod_ComponentValidation validation, boolean checkForMissing, boolean checkForNegative, RTi.TS.TS ts, java.util.List<StateMod_Data> stationList) throws Exception
	public static StateMod_ComponentValidation validateTimeSeries(StateMod_ComponentValidation validation, bool checkForMissing, bool checkForNegative, TS ts, IList<StateMod_Data> stationList)
	{
		if (validation == null)
		{
			validation = new StateMod_ComponentValidation();
		}
		TSIterator tsi = ts.GetEnumerator();
		TSData tsdata;
		double value;
		string id = "" + ts.getIdentifier();
		while ((tsdata = tsi.next()) != null)
		{
			value = tsdata.getDataValue();
			if (ts.isDataMissing(value))
			{
				if (checkForMissing)
				{
					// TODO Evaluate whether to create StateMod_TimeSeries or similar class to wrap TS, and
					// implement StateMod_ComponentValidator so that null does not need to be passed below.
					validation.add(new StateMod_ComponentValidationProblem(null,"Time series \"" + id + "\" has missing value at " + tsi.getDate(), "Check input data and processing to fill the value."));
				}
			}
			else
			{
				if (checkForNegative && (value < 0.0))
				{
					validation.add(new StateMod_ComponentValidationProblem(null,"Time series \"" + id + "\" value (" + StringUtil.formatString(value,"%.4f") + ") is negative at " + tsi.getDate(), "Check input data and processing."));
				}
			}
		}
		// Check to make sure the time series matches a station, using only the location ID
		if ((stationList != null) && (stationList.Count > 0))
		{
			string loc = ts.getLocation();
			if (indexOf(stationList,loc) < 0)
			{
				validation.add(new StateMod_ComponentValidationProblem(null,"Time series \"" + id + "\" location does not match any stations.", "Verify that the time series is being created for a valid station."));
			}
		}
		return validation;
	}

	}

}