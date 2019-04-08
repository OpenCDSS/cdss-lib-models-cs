using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.IO;

// StateMod_DataSet - this class manages data components in a StateMod data set

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
    using IOUtil = RTi.Util.IO.IOUtil;
    using DataSet = RTi.Util.IO.DataSet;
    using DataSetComponent = RTi.Util.IO.DataSetComponent;
    using Prop = RTi.Util.IO.Prop;
    using PropList = RTi.Util.IO.PropList;
    using ProcessListener = RTi.Util.IO.ProcessListener;
    using Message = RTi.Util.Message.Message;
    using MonthTS = RTi.TS.MonthTS;
    //using StringUtil = RTi.Util.String.StringUtil;
    //using DateTime = RTi.Util.Time.DateTime;
    using StopWatch = RTi.Util.Time.StopWatch;
    using TimeInterval = RTi.Util.Time.TimeInterval;
    using TS = RTi.TS.TS;
    //using TimeUtil = RTi.Util.Time.TimeUtil;
    //using YearType = RTi.Util.Time.YearType;

    public class StateMod_DataSet : DataSet
    {

        /// <summary>
        /// Indicates whether time series are read when reading the data set.  This was put in place when software
        /// performance was slow but generally now it is not an issue.  Leave in for some period but phase out if
        /// performance is not an issue.
        /// </summary>
        private bool __readTimeSeries = true;

        private IList<ProcessListener> __processListeners = null;

        // The following should be sequential from 0 because they have lookup positions in DataSet arrays.
        //
        // Some of the following values are for sub-components (e.g., delay table
        // assignment for diversions).  These are typically one-to-many data items that
        // are managed with a component but may need to be displayed separately.  The
        // sub-components have numbers that are the main component*100 + N.  These
        // values are checked in methods like lookupComponentName() but do not have sequential arrays.
        //
        // TODO SAM 2005-01-19 - Evaluate whether sub-components should be handled in the arrays.

        public const int COMP_CONTROL_GROUP = 0, COMP_RESPONSE = 1, COMP_CONTROL = 2, COMP_OUTPUT_REQUEST = 3, COMP_REACH_DATA = 4, COMP_CONSUMPTIVE_USE_GROUP = 5, COMP_STATECU_STRUCTURE = 6, COMP_IRRIGATION_PRACTICE_TS_YEARLY = 7, COMP_CONSUMPTIVE_WATER_REQUIREMENT_TS_MONTHLY = 8, COMP_CONSUMPTIVE_WATER_REQUIREMENT_TS_DAILY = 9, COMP_STREAMGAGE_GROUP = 10, COMP_STREAMGAGE_STATIONS = 11, COMP_STREAMGAGE_HISTORICAL_TS_MONTHLY = 12, COMP_STREAMGAGE_HISTORICAL_TS_DAILY = 13, COMP_STREAMGAGE_NATURAL_FLOW_TS_MONTHLY = 14, COMP_STREAMGAGE_NATURAL_FLOW_TS_DAILY = 15, COMP_DELAY_TABLE_MONTHLY_GROUP = 16, COMP_DELAY_TABLES_MONTHLY = 17, COMP_DELAY_TABLE_DAILY_GROUP = 18, COMP_DELAY_TABLES_DAILY = 19, COMP_DIVERSION_GROUP = 20, COMP_DIVERSION_STATIONS = 21, COMP_DIVERSION_STATION_DELAY_TABLES = 2101, COMP_DIVERSION_STATION_COLLECTIONS = 2102, COMP_DIVERSION_RIGHTS = 22, COMP_DIVERSION_TS_MONTHLY = 23, COMP_DIVERSION_TS_DAILY = 24, COMP_DEMAND_TS_MONTHLY = 25, COMP_DEMAND_TS_OVERRIDE_MONTHLY = 26, COMP_DEMAND_TS_AVERAGE_MONTHLY = 27, COMP_DEMAND_TS_DAILY = 28, COMP_PRECIPITATION_GROUP = 29, COMP_PRECIPITATION_TS_MONTHLY = 30, COMP_PRECIPITATION_TS_YEARLY = 31, COMP_EVAPORATION_GROUP = 32, COMP_EVAPORATION_TS_MONTHLY = 33, COMP_EVAPORATION_TS_YEARLY = 34, COMP_RESERVOIR_GROUP = 35, COMP_RESERVOIR_STATIONS = 36, COMP_RESERVOIR_STATION_ACCOUNTS = 3601, COMP_RESERVOIR_STATION_PRECIP_STATIONS = 3602, COMP_RESERVOIR_STATION_EVAP_STATIONS = 3603, COMP_RESERVOIR_STATION_CURVE = 3604, COMP_RESERVOIR_STATION_COLLECTIONS = 3605, COMP_RESERVOIR_RIGHTS = 37, COMP_RESERVOIR_CONTENT_TS_MONTHLY = 38, COMP_RESERVOIR_CONTENT_TS_DAILY = 39, COMP_RESERVOIR_TARGET_TS_MONTHLY = 40, COMP_RESERVOIR_TARGET_TS_DAILY = 41, COMP_RESERVOIR_RETURN = 42, COMP_INSTREAM_GROUP = 43, COMP_INSTREAM_STATIONS = 44, COMP_INSTREAM_RIGHTS = 45, COMP_INSTREAM_DEMAND_TS_MONTHLY = 46, COMP_INSTREAM_DEMAND_TS_AVERAGE_MONTHLY = 47, COMP_INSTREAM_DEMAND_TS_DAILY = 48, COMP_WELL_GROUP = 49, COMP_WELL_STATIONS = 50, COMP_WELL_STATION_DELAY_TABLES = 5001, COMP_WELL_STATION_DEPLETION_TABLES = 5002, COMP_WELL_STATION_COLLECTIONS = 5003, COMP_WELL_RIGHTS = 51, COMP_WELL_PUMPING_TS_MONTHLY = 52, COMP_WELL_PUMPING_TS_DAILY = 53, COMP_WELL_DEMAND_TS_MONTHLY = 54, COMP_WELL_DEMAND_TS_DAILY = 55, COMP_PLAN_GROUP = 56, COMP_PLANS = 57, COMP_PLAN_WELL_AUGMENTATION = 58, COMP_PLAN_RETURN = 59, COMP_STREAMESTIMATE_GROUP = 60, COMP_STREAMESTIMATE_STATIONS = 61, COMP_STREAMESTIMATE_COEFFICIENTS = 62, COMP_STREAMESTIMATE_NATURAL_FLOW_TS_MONTHLY = 63, COMP_STREAMESTIMATE_NATURAL_FLOW_TS_DAILY = 64, COMP_RIVER_NETWORK_GROUP = 65, COMP_RIVER_NETWORK = 66, COMP_NETWORK = 67, COMP_OPERATION_GROUP = 68, COMP_OPERATION_RIGHTS = 69, COMP_DOWNSTREAM_CALL_TS_DAILY = 70, COMP_SANJUAN_RIP = 71, COMP_RIO_GRANDE_SPILL = 72, COMP_GEOVIEW_GROUP = 73, COMP_GEOVIEW = 74;

        // The data set component names, including the component groups.  Subcomponent
        // names are defined after this array and are currently treated as special cases.
        private static string[] __component_names = new string[] { "Control Data", "Response", "Control", "Output Request", "Reach Data", "Consumptive Use Data", "StateCU Structure", "Irrigation Practice TS (Yearly)", "Consumptive Water Requirement TS (Monthly)", "Consumptive Water Requirement TS (Daily)", "Stream Gage Data", "Stream Gage Stations", "Stream Gage Historical TS (Monthly)", "Stream Gage Historical TS (Daily)", "Stream Gage Natural Flow TS (Monthly)", "Stream Gage Natural Flow TS (Daily)", "Delay Table (Monthly) Data", "Delay Tables (Monthly)", "Delay Table (Daily) Data", "Delay Tables (Daily)", "Diversion Data", "Diversion Stations", "Diversion Rights", "Diversion Historical TS (Monthly)", "Diversion Historical TS (Daily)", "Diversion Demand TS (Monthly)", "Diversion Demand TS Override (Monthly)", "Diversion Demand TS (Average Monthly)", "Diversion Demand TS (Daily)", "Precipitation Data", "Precipitation Time Series (Monthly)", "Precipitation Time Series (Yearly)", "Evaporation Data", "Evaporation Time Series (Monthly)", "Evaporation Time Series (Yearly)", "Reservoir Data", "Reservoir Stations", "Reservoir Rights", "Reservoir Content TS, End of Month (Monthly)", "Reservoir Content TS, End of Day (Daily)", "Reservoir Target TS (Monthly)", "Reservoir Target TS (Daily)", "Reservoir Return Flows", "Instream Flow Data", "Instream Flow Stations", "Instream Flow Rights", "Instream Flow Demand TS (Monthly)", "Instream Flow Demand TS (Average Monthly)", "Instream Flow Demand TS (Daily)", "Well Data", "Well Stations", "Well Rights", "Well Historical Pumping TS (Monthly)", "Well Historical Pumping TS (Daily)", "Well Demand TS (Monthly)", "Well Demand TS (Daily)", "Plan Data", "Plans", "Plan Well Augmentation Data", "Plan Return Flows", "Stream Estimate Data", "Stream Estimate Stations", "Stream Estimate Coefficients", "Stream Estimate Natural Flow TS (Monthly)", "Stream Estimate Natural Flow TS (Daily)", "River Network Data", "River Network", "Network (Graphical)", "Operational Data", "Operational Rights", "Downstream Call Time Series (Daily)", "San Juan Sediment Recovery Plan", "Rio Grande Spill (Monthly)", "Spatial Data", "GeoView Project" };

        /// <summary>
        /// List of all the components, by number (type).
        /// </summary>
        private static int[] __component_types = new int[] { COMP_CONTROL_GROUP, COMP_RESPONSE, COMP_CONTROL, COMP_OUTPUT_REQUEST, COMP_REACH_DATA, COMP_CONSUMPTIVE_USE_GROUP, COMP_STATECU_STRUCTURE, COMP_IRRIGATION_PRACTICE_TS_YEARLY, COMP_CONSUMPTIVE_WATER_REQUIREMENT_TS_MONTHLY, COMP_CONSUMPTIVE_WATER_REQUIREMENT_TS_DAILY, COMP_STREAMGAGE_GROUP, COMP_STREAMGAGE_STATIONS, COMP_STREAMGAGE_HISTORICAL_TS_MONTHLY, COMP_STREAMGAGE_HISTORICAL_TS_DAILY, COMP_STREAMGAGE_NATURAL_FLOW_TS_MONTHLY, COMP_STREAMGAGE_NATURAL_FLOW_TS_DAILY, COMP_DELAY_TABLE_MONTHLY_GROUP, COMP_DELAY_TABLES_MONTHLY, COMP_DELAY_TABLE_DAILY_GROUP, COMP_DELAY_TABLES_DAILY, COMP_DIVERSION_GROUP, COMP_DIVERSION_STATIONS, COMP_DIVERSION_RIGHTS, COMP_DIVERSION_TS_MONTHLY, COMP_DIVERSION_TS_DAILY, COMP_DEMAND_TS_MONTHLY, COMP_DEMAND_TS_OVERRIDE_MONTHLY, COMP_DEMAND_TS_AVERAGE_MONTHLY, COMP_DEMAND_TS_DAILY, COMP_PRECIPITATION_GROUP, COMP_PRECIPITATION_TS_MONTHLY, COMP_PRECIPITATION_TS_YEARLY, COMP_EVAPORATION_GROUP, COMP_EVAPORATION_TS_MONTHLY, COMP_EVAPORATION_TS_YEARLY, COMP_RESERVOIR_GROUP, COMP_RESERVOIR_STATIONS, COMP_RESERVOIR_RIGHTS, COMP_RESERVOIR_CONTENT_TS_MONTHLY, COMP_RESERVOIR_CONTENT_TS_DAILY, COMP_RESERVOIR_TARGET_TS_MONTHLY, COMP_RESERVOIR_TARGET_TS_DAILY, COMP_RESERVOIR_RETURN, COMP_INSTREAM_GROUP, COMP_INSTREAM_STATIONS, COMP_INSTREAM_RIGHTS, COMP_INSTREAM_DEMAND_TS_MONTHLY, COMP_INSTREAM_DEMAND_TS_AVERAGE_MONTHLY, COMP_INSTREAM_DEMAND_TS_DAILY, COMP_WELL_GROUP, COMP_WELL_STATIONS, COMP_WELL_RIGHTS, COMP_WELL_PUMPING_TS_MONTHLY, COMP_WELL_PUMPING_TS_DAILY, COMP_WELL_DEMAND_TS_MONTHLY, COMP_WELL_DEMAND_TS_DAILY, COMP_PLAN_GROUP, COMP_PLANS, COMP_PLAN_WELL_AUGMENTATION, COMP_PLAN_RETURN, COMP_STREAMESTIMATE_GROUP, COMP_STREAMESTIMATE_STATIONS, COMP_STREAMESTIMATE_COEFFICIENTS, COMP_STREAMESTIMATE_NATURAL_FLOW_TS_MONTHLY, COMP_STREAMESTIMATE_NATURAL_FLOW_TS_DAILY, COMP_RIVER_NETWORK_GROUP, COMP_RIVER_NETWORK, COMP_NETWORK, COMP_OPERATION_GROUP, COMP_OPERATION_RIGHTS, COMP_DOWNSTREAM_CALL_TS_DAILY, COMP_SANJUAN_RIP, COMP_RIO_GRANDE_SPILL, COMP_GEOVIEW_GROUP, COMP_GEOVIEW };

        /// <summary>
        /// This array indicates the default file extension to use with each component.
        /// These extensions can be used in file choosers.
        /// </summary>
        private static string[] __component_file_extensions = new string[] { "Control Group", "rsp", "ctl", "out", "rch", "Consumptive Use Group", "str", "ipy", "iwr", "iwd", "Stream Gage Group", "ris", "rih", "riy", "rim", "rid", "Delay Tables (Monthly) Group", "dly", "Delay Tables (Daily) Group", "dld", "Diversion Group", "dds", "ddr", "ddh", "ddy", "ddm", "ddo", "dda", "ddd", "Precipitation Group", "pre", "pra", "Evaporation Group", "evm", "eva", "Reservoir Group", "res", "rer", "eom", "eoy", "tar", "tad", "rrf", "Instream Group", "ifs", "ifr", "ifm", "ifa", "ifd", "Well Group", "wes", "wer", "weh", "wey", "wem", "wed", "Plan Group", "pln", "plw", "prf", "StreamEstimate Group", "ses", "rib", "rim", "rid", "River Network Group", "rin", "net", "Operation Group", "opr", "cal", "sjr", "rgs", "GeoView Group", "gvp" };

        /// <summary>
        /// This array indicates the StateMod response file property name to use with each
        /// component.  The group names are suitable for comments (put a # in front when
        /// writing the response file).  Any value that is a blank string should NOT be written to the StateMod file.
        /// </summary>
        private static string[] __statemod_file_properties = new string[] { "", "Response", "Control", "OutputRequest", "Reach_Data", "", "StateCU_Structure", "IrrigationPractice_Yearly", "ConsumptiveWaterRequirement_Monthly", "ConsumptiveWaterRequirement_Daily", "", "StreamGage_Station", "StreamGage_Historic_Monthly", "StreamGage_Historic_Daily", "Stream_Base_Monthly", "Stream_Base_Daily", "", "DelayTable_Monthly", "", "DelayTable_Daily", "", "Diversion_Station", "Diversion_Right", "Diversion_Historic_Monthly", "Diversion_Historic_Daily", "Diversion_Demand_Monthly", "Diversion_DemandOverride_Monthly", "Diversion_Demand_AverageMonthly", "Diversion_Demand_Daily", "", "Precipitation_Monthly", "Precipitation_Annual", "", "Evaporation_Monthly", "Evaporation_Annual", "", "Reservoir_Station", "Reservoir_Right", "Reservoir_Historic_Monthly", "Reservoir_Historic_Daily", "Reservoir_Target_Monthly", "Reservoir_Target_Daily", "Reservoir_Return", "", "Instreamflow_Station", "Instreamflow_Right", "Instreamflow_Demand_Monthly", "Instreamflow_Demand_AverageMonthly", "Instreamflow_Demand_Daily", "", "Well_Station", "Well_Right", "Well_Historic_Monthly", "Well_Historic_Daily", "Well_Demand_Monthly", "Well_Demand_Daily", "", "Plan_Data", "Plan_Wells", "Plan_Return", "", "StreamEstimate_Station", "StreamEstimate_Coefficients", "Stream_Base_Monthly", "Stream_Base_Daily", "", "River_Network", "Network", "", "Operational_Right", "Downstream_Call", "SanJuanRecovery", "RioGrande_Spill_Monthly", "", "GeographicInformation" };

        /// <summary>
        /// Array indicating which components are groups.
        /// </summary>
        private static int[] __component_groups = new int[] { COMP_CONTROL_GROUP, COMP_CONSUMPTIVE_USE_GROUP, COMP_STREAMGAGE_GROUP, COMP_DELAY_TABLE_MONTHLY_GROUP, COMP_DELAY_TABLE_DAILY_GROUP, COMP_DIVERSION_GROUP, COMP_PRECIPITATION_GROUP, COMP_EVAPORATION_GROUP, COMP_RESERVOIR_GROUP, COMP_INSTREAM_GROUP, COMP_WELL_GROUP, COMP_PLAN_GROUP, COMP_STREAMESTIMATE_GROUP, COMP_RIVER_NETWORK_GROUP, COMP_OPERATION_GROUP, COMP_GEOVIEW_GROUP };

        /// <summary>
        /// Array indicating the primary components within each component group.  The
        /// primary components are used to get the list of identifiers for displays and
        /// processing.  The number of values should agree with the list above.
        /// </summary>
        private static int[] __component_group_primaries = new int[] { COMP_RESPONSE, COMP_STATECU_STRUCTURE, COMP_STREAMGAGE_STATIONS, COMP_DELAY_TABLES_MONTHLY, COMP_DELAY_TABLES_DAILY, COMP_DIVERSION_STATIONS, COMP_PRECIPITATION_TS_MONTHLY, COMP_EVAPORATION_TS_MONTHLY, COMP_RESERVOIR_STATIONS, COMP_INSTREAM_STATIONS, COMP_WELL_STATIONS, COMP_PLANS, COMP_STREAMESTIMATE_STATIONS, COMP_RIVER_NETWORK, COMP_OPERATION_RIGHTS, COMP_GEOVIEW };

        /// <summary>
        /// Array indicating the groups for each component.
        /// </summary>
        private static int[] __component_group_assignments = new int[] { COMP_CONTROL_GROUP, COMP_CONTROL_GROUP, COMP_CONTROL_GROUP, COMP_CONTROL_GROUP, COMP_CONTROL_GROUP, COMP_CONSUMPTIVE_USE_GROUP, COMP_CONSUMPTIVE_USE_GROUP, COMP_CONSUMPTIVE_USE_GROUP, COMP_CONSUMPTIVE_USE_GROUP, COMP_CONSUMPTIVE_USE_GROUP, COMP_STREAMGAGE_GROUP, COMP_STREAMGAGE_GROUP, COMP_STREAMGAGE_GROUP, COMP_STREAMGAGE_GROUP, COMP_STREAMGAGE_GROUP, COMP_STREAMGAGE_GROUP, COMP_DELAY_TABLE_MONTHLY_GROUP, COMP_DELAY_TABLE_MONTHLY_GROUP, COMP_DELAY_TABLE_DAILY_GROUP, COMP_DELAY_TABLE_DAILY_GROUP, COMP_DIVERSION_GROUP, COMP_DIVERSION_GROUP, COMP_DIVERSION_GROUP, COMP_DIVERSION_GROUP, COMP_DIVERSION_GROUP, COMP_DIVERSION_GROUP, COMP_DIVERSION_GROUP, COMP_DIVERSION_GROUP, COMP_DIVERSION_GROUP, COMP_PRECIPITATION_GROUP, COMP_PRECIPITATION_GROUP, COMP_PRECIPITATION_GROUP, COMP_EVAPORATION_GROUP, COMP_EVAPORATION_GROUP, COMP_EVAPORATION_GROUP, COMP_RESERVOIR_GROUP, COMP_RESERVOIR_GROUP, COMP_RESERVOIR_GROUP, COMP_RESERVOIR_GROUP, COMP_RESERVOIR_GROUP, COMP_RESERVOIR_GROUP, COMP_RESERVOIR_GROUP, COMP_RESERVOIR_GROUP, COMP_INSTREAM_GROUP, COMP_INSTREAM_GROUP, COMP_INSTREAM_GROUP, COMP_INSTREAM_GROUP, COMP_INSTREAM_GROUP, COMP_INSTREAM_GROUP, COMP_WELL_GROUP, COMP_WELL_GROUP, COMP_WELL_GROUP, COMP_WELL_GROUP, COMP_WELL_GROUP, COMP_WELL_GROUP, COMP_WELL_GROUP, COMP_PLAN_GROUP, COMP_PLAN_GROUP, COMP_PLAN_GROUP, COMP_PLAN_GROUP, COMP_STREAMESTIMATE_GROUP, COMP_STREAMESTIMATE_GROUP, COMP_STREAMESTIMATE_GROUP, COMP_STREAMESTIMATE_GROUP, COMP_STREAMESTIMATE_GROUP, COMP_RIVER_NETWORK_GROUP, COMP_RIVER_NETWORK_GROUP, COMP_RIVER_NETWORK_GROUP, COMP_OPERATION_GROUP, COMP_OPERATION_GROUP, COMP_OPERATION_GROUP, COMP_OPERATION_GROUP, COMP_OPERATION_GROUP, COMP_GEOVIEW_GROUP, COMP_GEOVIEW_GROUP };

        /// <summary>
        /// The following array assigns the time series data types for use with time series.
        /// For example, StateMod data sets do not contain a data type and therefore after
        /// reading the file, the time series data type must be assumed.  If the data
        /// component is known (e.g., because reading from a response file), then the
        /// following array can be used to look up the data type for the time series.
        /// Components that are not time series have blank strings for data types.
        /// </summary>
        private static string[] __component_ts_data_types = new string[] { "", "", "", "", "", "", "", "", "CWR", "CWR", "", "", "FlowHist", "FlowHist", "FlowNatural", "FlowNatural", "", "", "", "", "", "", "TotalWaterRights", "DiversionHist", "DiversionHist", "Demand", "DemandOverride", "DemandAverage", "Demand", "", "Precipitation", "Precipitation", "", "Evaporation", "Evaporation", "", "", "TotalWaterRights", "ContentEOMHist", "ContentEODHist", "Target", "Target", "", "", "", "TotalWaterRights", "Demand", "DemandAverage", "Demand", "", "", "TotalWaterRights", "PumpingHist", "PumpingHist", "Demand", "Demand", "", "", "", "", "", "", "", "FlowNatural", "FlowNatural", "", "", "", "", "", "Call", "SJRIP", "RioGrandeSpill", "", "" };

        /// <summary>
        /// The following array assigns the time series data intervals for use with time
        /// series.  This information is important because the data types themselves may
        /// not be unique and the interval must be examined.
        /// </summary>
        private static int[] __component_ts_data_intervals = new int[] { TimeInterval.UNKNOWN, TimeInterval.UNKNOWN, TimeInterval.UNKNOWN, TimeInterval.UNKNOWN, TimeInterval.UNKNOWN, TimeInterval.UNKNOWN, TimeInterval.UNKNOWN, TimeInterval.YEAR, TimeInterval.MONTH, TimeInterval.DAY, TimeInterval.UNKNOWN, TimeInterval.UNKNOWN, TimeInterval.MONTH, TimeInterval.DAY, TimeInterval.MONTH, TimeInterval.DAY, TimeInterval.UNKNOWN, TimeInterval.UNKNOWN, TimeInterval.UNKNOWN, TimeInterval.UNKNOWN, TimeInterval.UNKNOWN, TimeInterval.UNKNOWN, TimeInterval.UNKNOWN, TimeInterval.MONTH, TimeInterval.DAY, TimeInterval.MONTH, TimeInterval.MONTH, TimeInterval.MONTH, TimeInterval.DAY, TimeInterval.UNKNOWN, TimeInterval.MONTH, TimeInterval.YEAR, TimeInterval.UNKNOWN, TimeInterval.MONTH, TimeInterval.YEAR, TimeInterval.UNKNOWN, TimeInterval.UNKNOWN, TimeInterval.UNKNOWN, TimeInterval.MONTH, TimeInterval.DAY, TimeInterval.MONTH, TimeInterval.DAY, TimeInterval.UNKNOWN, TimeInterval.UNKNOWN, TimeInterval.UNKNOWN, TimeInterval.UNKNOWN, TimeInterval.MONTH, TimeInterval.MONTH, TimeInterval.DAY, TimeInterval.UNKNOWN, TimeInterval.UNKNOWN, TimeInterval.UNKNOWN, TimeInterval.MONTH, TimeInterval.DAY, TimeInterval.MONTH, TimeInterval.DAY, TimeInterval.UNKNOWN, TimeInterval.UNKNOWN, TimeInterval.UNKNOWN, TimeInterval.UNKNOWN, TimeInterval.UNKNOWN, TimeInterval.UNKNOWN, TimeInterval.UNKNOWN, TimeInterval.MONTH, TimeInterval.DAY, TimeInterval.UNKNOWN, TimeInterval.UNKNOWN, TimeInterval.UNKNOWN, TimeInterval.UNKNOWN, TimeInterval.UNKNOWN, TimeInterval.DAY, TimeInterval.YEAR, TimeInterval.MONTH, TimeInterval.UNKNOWN, TimeInterval.UNKNOWN };

        /// <summary>
        /// The following array assigns the time series data units for use with time series.
        /// These can be used when creating new time series.  If the data
        /// component is known (e.g., because reading from a response file), then the
        /// following array can be used to look up the data units for the time series.
        /// Components that are not time series have blank strings for data units.
        /// </summary>
        private static string[] __component_ts_data_units = new string[] { "", "", "", "", "", "", "", "", "ACFT", "CFS", "", "", "ACFT", "CFS", "ACFT", "CFS", "", "", "", "", "", "", "CFS", "ACFT", "CFS", "ACFT", "ACFT", "ACFT", "CFS", "", "IN", "IN", "", "IN", "IN", "", "", "ACFT", "ACFT", "ACFT", "ACFT", "ACFT", "", "", "", "CFS", "CFS", "CFS", "CFS", "", "", "CFS", "ACFT", "CFS", "ACFT", "CFS", "", "", "", "", "", "", "", "ACFT", "CFS", "", "", "", "", "", "DAY", "", "", "", "" };

        /// <summary>
        /// List of unknown file property names in the *.rsp.  These are properties not understood
        /// by the code but will need to be retained when writing the *.rsp to keep it whole.
        /// This list WILL include special properties like StateModExecutable that are used by the GUI.
        /// </summary>
        private PropList __unhandledResponseFileProperties = new PropList("Unhandled response file properties.");

        // Control file data specific to StateMod.  Defaults are assigned to allow
        // backward compatibility with old data sets - newer settings are set to
        // "no data" values.  Note that after reading the control file, setDirty(false)
        // is called and the new defaults may be ignored if the file is not written.

        /// <summary>
        /// Heading for output.
        /// </summary>
        private string __heading1 = "";
        /// <summary>
        /// Heading for output.
        /// </summary>
        private string __heading2 = "";
        /// <summary>
        /// Starting year of the simulation.  Must be defined.
        /// </summary>
        //private int __iystr = StateMod_Util.MISSING_INT;
        /// <summary>
        /// Ending year of the simulation.  Must be defined.
        /// </summary>
        //private int __iyend = StateMod_Util.MISSING_INT;
        /// <summary>
        /// Switch for output units.  Default is ACFT.
        /// </summary>
        private int __iresop = 2;
        /// <summary>
        /// Monthly or avg monthly evap.  Default to monthly.
        /// </summary>
        private int __moneva = 0;
        /// <summary>
        /// Total or gains streamflow.  Default to total.
        /// </summary>
        private int __iopflo = 1;
        /// <summary>
        /// Number of precipitation stations - should be set when the time series are read -
        /// this will be phased out in the future.
        /// </summary>
        private int __numpre = 0;
        /// <summary>
        /// Number of evaporation stations - should be set when the time series are read -
        /// this will be phased out in the future.
        /// </summary>
        private int __numeva = 0;
        /// <summary>
        /// Max number entries in delay pattern.  Default is variable number as percents.
        /// The following defaults assume normal operation...
        /// </summary>
        private int __interv = -1;
        /// <summary>
        /// Factor, CFS to AF/D
        /// </summary>
        private double __factor = 1.9835;
        /// <summary>
        /// Divisor for streamflow data units.
        /// </summary>
        private double __rfacto = 1.9835;
        /// <summary>
        /// Divisor for diversion data units.
        /// </summary>
        private double __dfacto = 1.9835;
        /// <summary>
        /// Divisor for instream flow data units.
        /// </summary>
        private double __ffacto = 1.9835;
        /// <summary>
        /// Factor, reservoir content data to AF.
        /// </summary>
        private double __cfacto = 1.0;
        /// <summary>
        /// Factor, evaporation data to FT.
        /// </summary>
        private double __efacto = 1.0;
        /// <summary>
        /// Factor, precipitation data to FT.
        /// </summary>
        private double __pfacto = 1.0;
        /// <summary>
        /// Calendar/water/irrigation year - default to calendar.
        /// </summary>
        //private YearType __cyrl = YearType.CALENDAR;
        /// <summary>
        /// Switch for demand type.  Default to historic approach.
        /// </summary>
        private int __icondem = 1;
        /// <summary>
        /// Switch for detailed output.  Default is no detailed output.
        /// </summary>
        private int __ichk = 0;
        /// <summary>
        /// Switch for re-operation control.  Default is yes re-operate.
        /// Unlike most StateMod options this uses 0 for do it.
        /// </summary>
        private int __ireopx = 0;
        /// <summary>
        /// Switch for instream flow approach.  Default to use reaches and average monthly demands.
        /// </summary>
        private int __ireach = 1;
        /// <summary>
        /// Switch for detailed call data.  Default to no data.
        /// </summary>
        private int __icall = 0;
        /// <summary>
        /// Default to not used.  Detailed call water right ID.
        /// </summary>
        private string __ccall = "";
        /// <summary>
        /// Switch for daily analysis.  Default to no daily analysis.
        /// </summary>
        private int __iday = 0;
        /// <summary>
        /// Switch for well analysis.  Default to no well analysis.
        /// </summary>
        private int __iwell = 0;
        /// <summary>
        /// Maximum recharge limit.  Default to not used.
        /// </summary>
        private double __gwmaxrc = 0.0;
        /// <summary>
        /// San Juan recovery program.  Default to no SJRIP.
        /// </summary>
        private int __isjrip = 0;
        /// <summary>
        /// Is IPY data used?  Default to no data.
        /// </summary>
        private int __itsfile = 0;
        /// <summary>
        /// IWR switch - default to no data.
        /// </summary>
        private int __ieffmax = 0;
        /// <summary>
        /// Sprinkler switch.  Default to no sprinkler data.
        /// </summary>
        private int __isprink = 0;
        /// <summary>
        /// Soil moisture accounting.  Default to not used.
        /// </summary>
        private double __soild = 0.0;
        /// <summary>
        /// Significant figures for output.
        /// </summary>
        private int __isig = 0;

        /// <summary>
        /// Constructor.  Makes a blank data set.  It is expected that other information 
        /// will be set during further processing.
        /// </summary>
        public StateMod_DataSet() : base(__component_types, __component_names, __component_groups, __component_group_assignments, __component_group_primaries)
        {

            initialize();
        }
        /// <summary>
        /// Constructor.  Makes a blank data set.  Specific output files, by default, will 
        /// use the output directory and base file name in output file names. </summary>
        /// <param name="type"> Data set type (currently ignored). </param>
        public StateMod_DataSet(int type) : base(__component_types, __component_names, __component_groups, __component_group_assignments, __component_group_primaries)
        {
            try
            {
                setDataSetType(type, true);
            }
            catch (Exception)
            {
                // Type not important
            }

            initialize();
        }

        /// <summary>
        /// Check the data set components for visibility based on the control file settings.
        /// If the control settings indicate
        /// that a file is not needed in a data set, it is marked as not visible and will
        /// not be shown in display components.  Invisible components are retained in the
        /// data set because sometimes they are included in the response file but we don't
        /// want to throw away the data as control settings change.  Only files that are
        /// impacted by control file settings are checked.
        /// </summary>
        public virtual void checkComponentVisibility()
        {
            DataSetComponent comp;
            bool visibility = true;

            // Check for daily data set (some may be reset in other checks below)...

            if (__iday != 0)
            {
                visibility = true;
            }
            else
            {
                visibility = false;
            }
            comp = getComponentForComponentType(COMP_STREAMGAGE_NATURAL_FLOW_TS_DAILY);
            if (comp != null)
            {
                comp.setVisible(visibility);
            }
            comp = getComponentForComponentType(COMP_DEMAND_TS_DAILY);
            if (comp != null)
            {
                comp.setVisible(visibility);
            }
            comp = getComponentForComponentType(COMP_INSTREAM_DEMAND_TS_DAILY);
            if (comp != null)
            {
                comp.setVisible(visibility);
            }
            comp = getComponentForComponentType(COMP_WELL_DEMAND_TS_DAILY);
            if (comp != null)
            {
                comp.setVisible(visibility);
            }
            comp = getComponentForComponentType(COMP_RESERVOIR_TARGET_TS_DAILY);
            if (comp != null)
            {
                comp.setVisible(visibility);
            }
            comp = getComponentForComponentType(COMP_DELAY_TABLE_DAILY_GROUP);
            if (comp != null)
            {
                comp.setVisible(visibility);
            }
            comp = getComponentForComponentType(COMP_DELAY_TABLES_DAILY);
            if (comp != null)
            {
                comp.setVisible(visibility);
            }
            comp = getComponentForComponentType(COMP_CONSUMPTIVE_WATER_REQUIREMENT_TS_DAILY);
            if (comp != null)
            {
                comp.setVisible(visibility);
            }
            comp = getComponentForComponentType(COMP_STREAMGAGE_HISTORICAL_TS_DAILY);
            if (comp != null)
            {
                comp.setVisible(visibility);
            }
            comp = getComponentForComponentType(COMP_DIVERSION_TS_DAILY);
            if (comp != null)
            {
                comp.setVisible(visibility);
            }
            comp = getComponentForComponentType(COMP_WELL_PUMPING_TS_DAILY);
            if (comp != null)
            {
                comp.setVisible(visibility);
            }
            comp = getComponentForComponentType(COMP_RESERVOIR_CONTENT_TS_DAILY);
            if (comp != null)
            {
                comp.setVisible(visibility);
            }

            // The stream estimate natural flow time series are always invisible because
            // they are shared with the stream gage natural time series files...

            comp = getComponentForComponentType(COMP_STREAMESTIMATE_NATURAL_FLOW_TS_MONTHLY);
            if (comp != null)
            {
                comp.setVisible(false);
            }
            comp = getComponentForComponentType(COMP_STREAMESTIMATE_NATURAL_FLOW_TS_DAILY);
            if (comp != null)
            {
                comp.setVisible(false);
            }

            // Check well data set...

            if (hasWellData(false))
            {
                visibility = true;
            }
            else
            {
                visibility = false;
            }
            comp = getComponentForComponentType(COMP_WELL_GROUP);
            if (comp != null)
            {
                comp.setVisible(visibility);
            }
            comp = getComponentForComponentType(COMP_WELL_STATIONS);
            if (comp != null)
            {
                comp.setVisible(visibility);
            }
            comp = getComponentForComponentType(COMP_WELL_RIGHTS);
            if (comp != null)
            {
                comp.setVisible(visibility);
            }
            comp = getComponentForComponentType(COMP_WELL_DEMAND_TS_MONTHLY);
            if (comp != null)
            {
                comp.setVisible(visibility);
            }
            comp = getComponentForComponentType(COMP_WELL_PUMPING_TS_MONTHLY);
            if (comp != null)
            {
                comp.setVisible(visibility);
            }
            if (__iday != 0)
            { // Else checked above
                comp = getComponentForComponentType(COMP_WELL_DEMAND_TS_DAILY);
                if (comp != null)
                {
                    comp.setVisible(visibility);
                }
                comp = getComponentForComponentType(COMP_WELL_PUMPING_TS_DAILY);
                if (comp != null)
                {
                    comp.setVisible(visibility);
                }
            }

            // Check instream demand flag (component is in the instream flow group)...

            if ((__ireach == 2) || (__ireach == 3))
            {
                visibility = true;
            }
            else
            {
                visibility = false;
            }
            comp = getComponentForComponentType(COMP_INSTREAM_DEMAND_TS_MONTHLY);
            if (comp != null)
            {
                comp.setVisible(visibility);
            }

            // Check SJRIP flag...

            if (__isjrip != 0)
            {
                visibility = true;
            }
            else
            {
                visibility = false;
            }
            comp = getComponentForComponentType(COMP_SANJUAN_RIP);
            if (comp != null)
            {
                comp.setVisible(visibility);
            }

            // Check irrigation practice flag (component is in the diversion group)...

            if (__itsfile != 0)
            {
                visibility = true;
            }
            else
            {
                visibility = false;
            }
            comp = getComponentForComponentType(COMP_IRRIGATION_PRACTICE_TS_YEARLY);
            if (comp != null)
            {
                comp.setVisible(visibility);
            }

            // Check variable efficiency flag (component is in the diversions group)...

            if (__ieffmax != 0)
            {
                visibility = true;
            }
            else
            {
                visibility = false;
            }
            comp = getComponentForComponentType(COMP_CONSUMPTIVE_WATER_REQUIREMENT_TS_MONTHLY);
            if (comp != null)
            {
                comp.setVisible(visibility);
            }
            if (__iday != 0)
            { // Else already check above
                comp = getComponentForComponentType(COMP_CONSUMPTIVE_WATER_REQUIREMENT_TS_DAILY);
                if (comp != null)
                {
                    comp.setVisible(visibility);
                }
            }

            // Check the soil moisture flag (component is in the diversions group)...

            if (__soild != 0.0)
            {
                visibility = true;
            }
            else
            {
                visibility = false;
            }
            comp = getComponentForComponentType(COMP_STATECU_STRUCTURE);
            if (comp != null)
            {
                comp.setVisible(visibility);
            }

            // Hide the Network (Graphical) file until it is fully implemented...

            comp = getComponentForComponentType(COMP_NETWORK);
            if (comp != null && comp.hasData())
            {
                comp.setVisible(true);
            }
        }

        /// <summary>
        /// Helper method to check to see whether a file is empty.  Traditionally, StateMod
        /// data files have been set to "xxxx.dum" or "dummy", which were non-existent or
        /// empty files, as place-holders for data.  This can cause read errors if code
        /// attempts to read the files. </summary>
        /// <param name="filename"> Name of file to check. </param>
        /// <returns> true if the file is empty (therefore don't try to read) or false if the
        /// file does not exist or exists but has zero size. </returns>
        private static bool fileIsEmpty(string filename)
        {
            FileInfo f = new FileInfo(filename);
            if (!f.Exists || (f.Length == 0L))
            {
                return true; // File is empty.
            }
            return false; // File might have data.
        }

        /// <summary>
        /// Determine the full path to a component data file, including accounting for the
        /// working directory.  If the file is already an absolute path, the same value is
        /// returned.  Otherwise, the data set directory is prepended to the component data
        /// file name (which may be relative to the data set directory) and then calls IOUtil.getPathUsingWorkingDir(). </summary>
        /// <param name="comp"> Data set component. </param>
        /// <returns> Full path to the data file (absolute), using the working directory. </returns>
        public virtual string getDataFilePathAbsolute(DataSetComponent comp)
        {
            string file = comp.getDataFileName();
            return getDataFilePathAbsolute(file);
        }

        /// <summary>
        /// Determine the full path to a component data file, including accounting for the
        /// working directory.  If the file is already an absolute path, the same value is
        /// returned.  Otherwise, the data set directory is prepended to the component data
        /// file name (which may be relative to the data set directory) and then calls IOUtil.getPathUsingWorkingDir(). </summary>
        /// <param name="file"> File name(e.g., from component getFileName()). </param>
        /// <returns> Full path to the data file (absolute), using the working directory. </returns>
        public virtual string getDataFilePathAbsolute(string file)
        {
            if (IOUtil.isAbsolute(file))
            {
                return file;
            }
            else
            {
                return IOUtil.getPathUsingWorkingDir(getDataSetDirectory() + Path.DirectorySeparatorChar + file);
            }
        }

        /// <summary>
        /// Return the list of unhandled response file properties.  These are entries in the *rsp file that the code
        /// does not specifically handle, such as new files or uninplemented files. </summary>
        /// <returns> properties from the response file that are not explicitly handled </returns>
        public virtual PropList getUnhandledResponseFileProperties()
        {
            return __unhandledResponseFileProperties;
        }

        /// <summary>
        /// Indicate whether the data set has well data (iwell not missing and iwell not equal to 0).
        /// Use this method instead of checking iwell directly to simplify logic and allow
        /// for future changes to the model input. </summary>
        /// <returns> true if the data set includes well data (iwell not missing and
        /// iwell != 0).  Return false if well data are not used. </returns>
        /// <param name="is_active"> Only return true if well data are included in the data set and
        /// the data are active (iwell = 1). </param>
        public virtual bool hasWellData(bool is_active)
        {
            if (is_active)
            {
                if (__iwell == 1)
                {
                    // Well data are included in the data set and are used...
                    return true;
                }
                else
                {
                    // Well data may or may not be included in the data set but are not used...
                    return false;
                }
            }
            else if (!StateMod_Util.isMissing(__iwell) && (__iwell != 0))
            {
                // Data are specified in the data set but are not used...
                return true;
            }
            else
            {
                // Well data are not included...
                return false;
            }
        }

        /// <summary>
        /// Initialize a data set by defining all the components for the data set.  This
        /// ensures that software will be able to evaluate all components.  Nulls are avoided where possible for
        /// data (e.g., empty lists are assigned).  Also initialize the control data to reasonable values.
        /// </summary>
        private void initialize()
        {
            string routine = "StateMod_DataSet.initialize";
            // Initialize the control data
            initializeControlData();
            // Always add all the components to the data set because StateMod does
            // not really differentiate between data set types.  Instead, control
            // file information controls.  Components are added to their groups.
            // Also initialize the data for each sub-component to empty Vectors so
            // that GUI based code does not need to check for nulls.  This is
            // consistent with StateMod GUI initializing data vectors to empty at startup.
            //
            // TODO - need to turn on data set components (set visible, etc.) as
            // the control file is changed.  This allows new components to be enabled in the right order.
            //
            // TODO - should be allowed to have null data Vector but apparently
            // StateMod GUI cannot handle yet - need to allow null later and use
            // hasData() or similar to check.

            DataSetComponent comp, subcomp;
            try
            {
                comp = new DataSetComponent(this, COMP_CONTROL_GROUP);
                comp.setListSource(DataSetComponent.LIST_SOURCE_PRIMARY_COMPONENT);
                addComponent(comp);
                subcomp = new DataSetComponent(this, COMP_RESPONSE);
                subcomp.setData(new List<object>());
                comp.addComponent(subcomp);
                subcomp = new DataSetComponent(this, COMP_CONTROL);
                subcomp.setData(new List<object>());
                comp.addComponent(subcomp);
                subcomp = new DataSetComponent(this, COMP_OUTPUT_REQUEST);
                subcomp.setData(new List<object>());
                comp.addComponent(subcomp);
                subcomp = new DataSetComponent(this, COMP_REACH_DATA);
                subcomp.setData(new List<object>());
                comp.addComponent(subcomp);

                comp = new DataSetComponent(this, COMP_CONSUMPTIVE_USE_GROUP);
                comp.setListSource(DataSetComponent.LIST_SOURCE_PRIMARY_COMPONENT);
                addComponent(comp);
                subcomp = new DataSetComponent(this, COMP_STATECU_STRUCTURE);
                subcomp.setData(new List<object>());
                comp.addComponent(subcomp);
                subcomp = new DataSetComponent(this, COMP_IRRIGATION_PRACTICE_TS_YEARLY);
                subcomp.setData(new List<object>());
                comp.addComponent(subcomp);
                subcomp = new DataSetComponent(this, COMP_CONSUMPTIVE_WATER_REQUIREMENT_TS_MONTHLY);
                subcomp.setData(new List<object>());
                comp.addComponent(subcomp);
                subcomp = new DataSetComponent(this, COMP_CONSUMPTIVE_WATER_REQUIREMENT_TS_DAILY);
                subcomp.setData(new List<object>());
                comp.addComponent(subcomp);

                comp = new DataSetComponent(this, COMP_STREAMGAGE_GROUP);
                comp.setListSource(DataSetComponent.LIST_SOURCE_PRIMARY_COMPONENT);
                addComponent(comp);
                subcomp = new DataSetComponent(this, COMP_STREAMGAGE_STATIONS);
                subcomp.setData(new List<object>());
                comp.addComponent(subcomp);
                subcomp = new DataSetComponent(this, COMP_STREAMGAGE_HISTORICAL_TS_MONTHLY);
                subcomp.setData(new List<object>());
                comp.addComponent(subcomp);
                subcomp = new DataSetComponent(this, COMP_STREAMGAGE_HISTORICAL_TS_DAILY);
                subcomp.setData(new List<object>());
                comp.addComponent(subcomp);
                subcomp = new DataSetComponent(this, COMP_STREAMGAGE_NATURAL_FLOW_TS_MONTHLY);
                subcomp.setData(new List<object>());
                comp.addComponent(subcomp);
                subcomp = new DataSetComponent(this, COMP_STREAMGAGE_NATURAL_FLOW_TS_DAILY);
                subcomp.setData(new List<object>());
                comp.addComponent(subcomp);

                comp = new DataSetComponent(this, COMP_DELAY_TABLE_MONTHLY_GROUP);
                comp.setListSource(DataSetComponent.LIST_SOURCE_PRIMARY_COMPONENT);
                addComponent(comp);
                subcomp = new DataSetComponent(this, COMP_DELAY_TABLES_MONTHLY);
                subcomp.setData(new List<object>());

                comp = new DataSetComponent(this, COMP_DELAY_TABLE_DAILY_GROUP);
                comp.setListSource(DataSetComponent.LIST_SOURCE_PRIMARY_COMPONENT);
                addComponent(comp);
                comp.addComponent(subcomp);
                subcomp = new DataSetComponent(this, COMP_DELAY_TABLES_DAILY);
                subcomp.setData(new List<object>());
                comp.addComponent(subcomp);

                comp = new DataSetComponent(this, COMP_DIVERSION_GROUP);
                comp.setListSource(DataSetComponent.LIST_SOURCE_PRIMARY_COMPONENT);
                addComponent(comp);
                subcomp = new DataSetComponent(this, COMP_DIVERSION_STATIONS);
                subcomp.setData(new List<object>());
                comp.addComponent(subcomp);
                subcomp = new DataSetComponent(this, COMP_DIVERSION_RIGHTS);
                subcomp.setData(new List<object>());
                comp.addComponent(subcomp);
                subcomp = new DataSetComponent(this, COMP_DIVERSION_TS_MONTHLY);
                subcomp.setData(new List<object>());
                comp.addComponent(subcomp);
                subcomp = new DataSetComponent(this, COMP_DIVERSION_TS_DAILY);
                subcomp.setData(new List<object>());
                comp.addComponent(subcomp);
                subcomp = new DataSetComponent(this, COMP_DEMAND_TS_MONTHLY);
                subcomp.setData(new List<object>());
                comp.addComponent(subcomp);
                subcomp = new DataSetComponent(this, COMP_DEMAND_TS_OVERRIDE_MONTHLY);
                subcomp.setData(new List<object>());
                comp.addComponent(subcomp);
                subcomp = new DataSetComponent(this, COMP_DEMAND_TS_AVERAGE_MONTHLY);
                subcomp.setData(new List<object>());
                comp.addComponent(subcomp);
                subcomp = new DataSetComponent(this, COMP_DEMAND_TS_DAILY);
                subcomp.setData(new List<object>());
                comp.addComponent(subcomp);

                comp = new DataSetComponent(this, COMP_PRECIPITATION_GROUP);
                comp.setListSource(DataSetComponent.LIST_SOURCE_PRIMARY_COMPONENT);
                addComponent(comp);
                subcomp = new DataSetComponent(this, COMP_PRECIPITATION_TS_MONTHLY);
                subcomp.setData(new List<object>());
                comp.addComponent(subcomp);
                subcomp = new DataSetComponent(this, COMP_PRECIPITATION_TS_YEARLY);
                subcomp.setData(new List<object>());
                comp.addComponent(subcomp);

                comp = new DataSetComponent(this, COMP_EVAPORATION_GROUP);
                comp.setListSource(DataSetComponent.LIST_SOURCE_PRIMARY_COMPONENT);
                addComponent(comp);
                subcomp = new DataSetComponent(this, COMP_EVAPORATION_TS_MONTHLY);
                subcomp.setData(new List<object>());
                comp.addComponent(subcomp);
                subcomp = new DataSetComponent(this, COMP_EVAPORATION_TS_YEARLY);
                subcomp.setData(new List<object>());
                comp.addComponent(subcomp);

                comp = new DataSetComponent(this, COMP_RESERVOIR_GROUP);
                comp.setListSource(DataSetComponent.LIST_SOURCE_PRIMARY_COMPONENT);
                addComponent(comp);
                subcomp = new DataSetComponent(this, COMP_RESERVOIR_STATIONS);
                subcomp.setData(new List<object>());
                comp.addComponent(subcomp);
                subcomp = new DataSetComponent(this, COMP_RESERVOIR_RIGHTS);
                subcomp.setData(new List<object>());
                comp.addComponent(subcomp);
                subcomp = new DataSetComponent(this, COMP_RESERVOIR_CONTENT_TS_MONTHLY);
                subcomp.setData(new List<object>());
                comp.addComponent(subcomp);
                subcomp = new DataSetComponent(this, COMP_RESERVOIR_CONTENT_TS_DAILY);
                subcomp.setData(new List<object>());
                comp.addComponent(subcomp);
                subcomp = new DataSetComponent(this, COMP_RESERVOIR_TARGET_TS_MONTHLY);
                subcomp.setData(new List<object>());
                comp.addComponent(subcomp);
                subcomp = new DataSetComponent(this, COMP_RESERVOIR_TARGET_TS_DAILY);
                subcomp.setData(new List<object>());
                comp.addComponent(subcomp);
                subcomp = new DataSetComponent(this, COMP_RESERVOIR_RETURN);
                subcomp.setData(new List<object>());
                comp.addComponent(subcomp);

                comp = new DataSetComponent(this, COMP_INSTREAM_GROUP);
                comp.setListSource(DataSetComponent.LIST_SOURCE_PRIMARY_COMPONENT);
                addComponent(comp);
                subcomp = new DataSetComponent(this, COMP_INSTREAM_STATIONS);
                subcomp.setData(new List<object>());
                comp.addComponent(subcomp);
                subcomp = new DataSetComponent(this, COMP_INSTREAM_RIGHTS);
                subcomp.setData(new List<object>());
                comp.addComponent(subcomp);
                subcomp = new DataSetComponent(this, COMP_INSTREAM_DEMAND_TS_MONTHLY);
                subcomp.setData(new List<object>());
                comp.addComponent(subcomp);
                subcomp = new DataSetComponent(this, COMP_INSTREAM_DEMAND_TS_AVERAGE_MONTHLY);
                subcomp.setData(new List<object>());
                comp.addComponent(subcomp);
                subcomp = new DataSetComponent(this, COMP_INSTREAM_DEMAND_TS_DAILY);
                subcomp.setData(new List<object>());
                comp.addComponent(subcomp);

                comp = new DataSetComponent(this, COMP_WELL_GROUP);
                comp.setListSource(DataSetComponent.LIST_SOURCE_PRIMARY_COMPONENT);
                addComponent(comp);
                subcomp = new DataSetComponent(this, COMP_WELL_STATIONS);
                subcomp.setData(new List<object>());
                comp.addComponent(subcomp);
                subcomp = new DataSetComponent(this, COMP_WELL_RIGHTS);
                subcomp.setData(new List<object>());
                comp.addComponent(subcomp);
                subcomp = new DataSetComponent(this, COMP_WELL_PUMPING_TS_MONTHLY);
                subcomp.setData(new List<object>());
                comp.addComponent(subcomp);
                subcomp = new DataSetComponent(this, COMP_WELL_PUMPING_TS_DAILY);
                subcomp.setData(new List<object>());
                comp.addComponent(subcomp);
                subcomp = new DataSetComponent(this, COMP_WELL_DEMAND_TS_MONTHLY);
                subcomp.setData(new List<object>());
                comp.addComponent(subcomp);
                subcomp = new DataSetComponent(this, COMP_WELL_DEMAND_TS_DAILY);
                subcomp.setData(new List<object>());
                comp.addComponent(subcomp);

                comp = new DataSetComponent(this, COMP_PLAN_GROUP);
                comp.setListSource(DataSetComponent.LIST_SOURCE_PRIMARY_COMPONENT);
                addComponent(comp);
                subcomp = new DataSetComponent(this, COMP_PLANS);
                subcomp.setData(new List<object>());
                comp.addComponent(subcomp);
                subcomp = new DataSetComponent(this, COMP_PLAN_WELL_AUGMENTATION);
                subcomp.setData(new List<object>());
                comp.addComponent(subcomp);
                subcomp = new DataSetComponent(this, COMP_PLAN_RETURN);
                subcomp.setData(new List<object>());
                comp.addComponent(subcomp);

                comp = new DataSetComponent(this, COMP_STREAMESTIMATE_GROUP);
                comp.setListSource(DataSetComponent.LIST_SOURCE_PRIMARY_COMPONENT);
                addComponent(comp);
                subcomp = new DataSetComponent(this, COMP_STREAMESTIMATE_STATIONS);
                subcomp.setData(new List<object>());
                comp.addComponent(subcomp);
                subcomp = new DataSetComponent(this, COMP_STREAMESTIMATE_COEFFICIENTS);
                subcomp.setData(new List<object>());
                comp.addComponent(subcomp);
                subcomp = new DataSetComponent(this, COMP_STREAMESTIMATE_NATURAL_FLOW_TS_MONTHLY);
                subcomp.setData(new List<object>());
                comp.addComponent(subcomp);
                subcomp = new DataSetComponent(this, COMP_STREAMESTIMATE_NATURAL_FLOW_TS_DAILY);
                subcomp.setData(new List<object>());
                comp.addComponent(subcomp);

                comp = new DataSetComponent(this, COMP_RIVER_NETWORK_GROUP);
                comp.setListSource(DataSetComponent.LIST_SOURCE_PRIMARY_COMPONENT);
                addComponent(comp);
                subcomp = new DataSetComponent(this, COMP_RIVER_NETWORK);
                subcomp.setData(new List<object>());
                comp.addComponent(subcomp);
                subcomp = new DataSetComponent(this, COMP_NETWORK);
                subcomp.setData(new List<object>());
                comp.addComponent(subcomp);

                comp = new DataSetComponent(this, COMP_OPERATION_GROUP);
                comp.setListSource(DataSetComponent.LIST_SOURCE_PRIMARY_COMPONENT);
                addComponent(comp);
                subcomp = new DataSetComponent(this, COMP_OPERATION_RIGHTS);
                subcomp.setData(new List<object>());
                comp.addComponent(subcomp);
                subcomp = new DataSetComponent(this, COMP_DOWNSTREAM_CALL_TS_DAILY);
                subcomp.setData(new List<object>());
                comp.addComponent(subcomp);
                subcomp = new DataSetComponent(this, COMP_SANJUAN_RIP);
                subcomp.setData(new List<object>());
                comp.addComponent(subcomp);
                subcomp = new DataSetComponent(this, COMP_RIO_GRANDE_SPILL);
                subcomp.setData(new List<object>());
                comp.addComponent(subcomp);

                comp = new DataSetComponent(this, COMP_GEOVIEW_GROUP);
                comp.setListSource(DataSetComponent.LIST_SOURCE_PRIMARY_COMPONENT);
                addComponent(comp);
                subcomp = new DataSetComponent(this, COMP_GEOVIEW);
                subcomp.setData(new List<object>());
                comp.addComponent(subcomp);

                Message.printStatus(2, routine, "Initialized " + getComponents().Count + " components (files) in data set.");
            }
            catch (Exception e)
            {
                // Should not happen...
                Message.printWarning(2, routine, e);
            }
        }

        /// <summary>
        /// Initialize the control data values to reasonable defaults.
        /// </summary>
        public virtual void initializeControlData()
        {
            __heading1 = "";
            __heading2 = "";
            //__iystr = StateMod_Util.MISSING_INT; // Start year of simulation
            //__iyend = StateMod_Util.MISSING_INT;
            __iresop = 2; // Switch for output units (default is ACFT for all)
            __moneva = 0; // Monthly or avg monthly evap.  Default to monthly.
            __iopflo = 1; // Total or gains streamflow.  Default to total.
            __numpre = 0; // Number of precipitation stations - set when the time series are read -
                          // this will be phased out in the future.
            __numeva = 0; // Number of evaporation stations - set when the time series are read -
                          // this will be phased out in the future
            __interv = -1; // Max number entries in delay pattern.  Default is variable number as percents.
            __factor = 1.9835; // Factor, CFS to AF/D
            __rfacto = 1.9835; // Divisor for streamflow data units.
            __dfacto = 1.9835; // Divisor for diversion data units.
            __ffacto = 1.9835; // Divisor for instream flow data units.
            __cfacto = 1.0; // Factor, reservoir content data to AF.
            __efacto = 1.0; // Factor, evaporation data to FT.
            __pfacto = 1.0; // Factor, precipitation data to FT.
            //__cyrl = YearType.CALENDAR; // Calendar/water/irrigation year - default to calendar.
            __icondem = 1; // Switch for demand type.  Default to historic approach.
            __ichk = 0; // Switch for detailed output.  Default is no detailed output.
            __ireopx = 0; // Switch for re-operation control.  Default is yes re-operate.
            __ireach = 1; // Switch for instream flow approach.
                          // Default to use reaches and average monthly demands.
            __icall = 0; // Switch for detailed call data.  Default to no data.
            __ccall = ""; // Detailed call water right ID.  Default to not used.
            __iday = 0; // Switch for daily analysis.  Default to no daily analysis.
            __iwell = 0; // Switch for well analysis.  Default to no well analysis.
            __gwmaxrc = 0.0; // Maximum recharge limit.  Default to not used.
            __isjrip = 0; // San Juan recovery program.  Default to no SJRIP.
            __itsfile = 0; // Is IPY data used?  Default to no data.
            __ieffmax = 0; // IWR switch - default to no data.
            __isprink = 0; // Sprinkler switch.  Default to no sprinkler data.
            __soild = 0.0; // Soil moisture accounting.  Default to not used.
            __isig = 0; // Significant figures for output.
        }

        /**
        Determine whether a StateMod response file is free format.  The response file
        is opened and checked for a non-commented line with the string "Control" followed by "=".
        @param filename Full path to the StateMod response file.
        @return true if the file is a free format file.
        */
        private bool isFreeFormatResponseFile(string filename)
        //TODO @jurentie 03/25/2019 - Need to handle throws error
        //throws IOException
        {
            bool isFreeFormat = false;
            using (StreamReader reader = new StreamReader(filename))
            {
                try
                {
                    string readerString = null;

                    while (reader.Peek() >= 0)
                    {
                        readerString = reader.ReadLine();
                        string stringTrimmed = readerString.Trim();
                        if (stringTrimmed.StartsWith("#") || stringTrimmed.Equals(""))
                        {
                            continue;
                        }
                        if (stringTrimmed.IndexOf("=") >= 0)
                        {
                            isFreeFormat = true;
                            break;
                        }
                    }
                }
                finally
                {
                    if (reader != null)
                    {
                        reader.Close();
                    }
                }
            }

            return isFreeFormat;
        }

        /// <summary>
        /// Determine the time series data type string for a component type. </summary>
        /// <param name="comp_type"> Component type. </param>
        /// <returns> the time series data type string or an empty string if not found.
        /// The only problem is with COMP_RESERVOIR_TARGET_TS_MONTHLY and
        /// COMP_RESERVOIR_TARGET_TS_DAILY, each of which contain both the maximum and
        /// minimum time series.  For these components, add "Max" and "Min" to the returned values. </returns>
        public static string lookupTimeSeriesDataType(int comp_type)
        {
            return __component_ts_data_types[comp_type];
        }

        // TODO - StateCU has this return a DataSet object, not populate an existing one
        /// <summary>
        /// Read the StateMod response file and fill the current StateMod_DataSet object.
        /// The file MUST be a newer free-format response file.
        /// The file and settings that are read are those set when the object was created. </summary>
        /// <param name="filename"> Name of the StateMod response file.  This must be the
        /// full path (e.g., from a JFileChooser, with a drive).  The working directory will
        /// be set to the directory of the response file. </param>
        /// <param name="readData"> if true, then all the data for files in the response file are read, if false, only read
        /// the filenames from the response file but do not try to read the
        /// data from station, rights, time series, etc.  False is useful for testing I/O on the response file itself. </param>
        /// <param name="readTimeSeries"> indicates whether the time series files should be read (this parameter was implemented
        /// when performance was slow and it made sense to avoid reading time series - this paramter may be phased out
        /// because it is not generally how an issue to read the time series).  If readData=false, then time series
        /// will not be read in any case. </param>
        /// <param name="useGUI"> If true, then interactive prompts will be used where necessary. </param>
        /// <param name="parent"> The parent JFrame used to position warning dialogs if useGUI is true. </param>
        /// <exception cref="IllegalArgumentException"> if the specified file does not appear to be a free-format response file. </exception>
        /// <exception cref="IOException"> if there is an unhandled error reading files. </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void readStateModFile(String filename, boolean readData, boolean readTimeSeries, boolean useGUI, javax.swing.JFrame parent) throws IllegalArgumentException, java.io.IOException
        public virtual void readStateModFile(string filename, bool readData, bool readTimeSeries, bool useGUI)//, JFrame parent)
        {
            string routine = "StateMod_DataSet.readStateModFile";
            if (!readData)
            {
                readTimeSeries = false;
            }
            __readTimeSeries = readTimeSeries;

            FileInfo f = new FileInfo(filename);
            setDataSetDirectory(f.Directory.ToString());
            setDataSetFileName(f.Name.ToString());

            // String printed at the end of warning messages
            string warningEndString = "\".";
            if (useGUI)
            {
                // This string is used if there are problems reading.
                warningEndString = "\"\nInteractive edits for file will be disabled.";
            }

            // Check whether the response file is free format.  If it is free
            // format then the file is read into a PropList below...

            if (!isFreeFormatResponseFile(filename))
            {
                string message = "File \"" + filename +
                    "\" does not appear to be free-format response file - unable to read.";
                Message.printWarning(3, routine, message);
                throw new System.ArgumentException(message);
            }

            // The following sets the static reference to the current data set
            // which is then accessible by every data object which extends
            // StateMod_Data.  This is done in order that setting components
            // dirty or not can be handled at a low level when values change.
            StateMod_Data._dataset = this;

            // Set basic information about the response file component - only save
            // the file name - the data itself are stored in this data set object.

            getComponentForComponentType(COMP_RESPONSE).setDataFileName(f.Name.ToString());

            string fn = "";

            int i = 0;
            int size = 0; // For general use

            DataSetComponent comp = null; // Component in StateMod data set

            // Now start reading new scenario...
            StopWatch totalReadTime = new StopWatch();
            StopWatch readTime = new StopWatch();

            Message.printStatus(1, routine, "Reading all information from input directory: \"" + getDataSetDirectory());

            Message.printStatus(1, routine, "Reading response file \"" + filename + "\"");

            totalReadTime.start();

            // Read the response file into a PropList...
            PropList response_props = new PropList("Response");
            response_props.setPersistentName(filename);
            response_props.readPersistent();

            try
            {
                // Try for all reads.

                // Read the lines of the response file.  Of major importance is reading the control file,
                // Read the lines of the response file.  Of major importance is reading the control file,
                // which indicates data set properties that allow figuring out which files are being read.

                // Read the files in the order of the StateMod documentation for the
                // response file, checking the control settings where a decision is needed.

                // Control file (.ctl)...

                try
                {
                    fn = response_props.getValue("Control");

                    // Always set the file name...
                    comp = getComponentForComponentType(COMP_CONTROL);
                    if ((comp != null) && (!string.ReferenceEquals(fn, null)))
                    {
                        comp.setDataFileName(fn);
                    }
                    // Read the data...
                    //if (readData && (!string.ReferenceEquals(fn, null)) && !fileIsEmpty(getDataFilePathAbsolute(fn)))
                    //{
                    //    readTime.clear();
                    //    readTime.start();
                    //    fn = getDataFilePathAbsolute(fn);
                    //    readStateModFile_Announce1(comp);
                    //    readStateModControlFile(fn);
                    //}
                }
                catch (Exception e)
                {
                    Message.printWarning(1, routine, "Unexpected error reading control file:\n" + "\"" + fn + warningEndString + " (See log file for more on error:" + e + ")");
                    Message.printWarning(3, routine, e);
                    comp.setErrorReadingInputFile(true);
                }
                finally
                {
                    comp.setDirty(false);
                    // Control does not have its own data file now so use the data set.
                    comp.setData(this);
                    readTime.stop();
                    readStateModFile_Announce2(comp, readTime.getSeconds());
                }

                // River network file (.rin)...

                try
                {
                    fn = response_props.getValue("River_Network");
                    // Always set the file name...
                    comp = getComponentForComponentType(COMP_RIVER_NETWORK);
                    if ((comp != null) && (!string.ReferenceEquals(fn, null)))
                    {
                        comp.setDataFileName(fn);
                    }
                    // Read the data...
                    if (readData && (!string.ReferenceEquals(fn, null)) && !fileIsEmpty(getDataFilePathAbsolute(fn)))
                    {
                        readTime.clear();
                        readTime.start();
                        fn = getDataFilePathAbsolute(fn);
                        readStateModFile_Announce1(comp);
                        comp.setData(StateMod_RiverNetworkNode.readStateModFile(fn));
                        string s = comp.getComponentName() + " data: \n";
                        foreach(StateMod_RiverNetworkNode node in (List<StateMod_RiverNetworkNode>)comp.getData())
                        {
                            s+= node.ToString() + "\n";
                        }
                        Message.printDebug(1, routine, s);
                    }
                }
                catch (Exception e)
                {
                    Message.printWarning(1, routine, "Error reading river network file:\n\"" + fn + warningEndString + " (See log file for more on error:" + e + ")");
                    Message.printWarning(3, routine, e);
                    comp.setErrorReadingInputFile(true);
                }
                finally
                {
                    comp.setDirty(false);
                    readTime.stop();
                    readStateModFile_Announce2(comp, readTime.getSeconds());
                }

                // Reservoir stations file (.res)...

                try
                {
                    fn = response_props.getValue("Reservoir_Station");
                    // Always set the file name...
                    comp = getComponentForComponentType(COMP_RESERVOIR_STATIONS);
                    if ((comp != null) && (!string.ReferenceEquals(fn, null)))
                    {
                        comp.setDataFileName(fn);
                    }
                    //// Read the data...
                    //if (readData && (!string.ReferenceEquals(fn, null)) && !fileIsEmpty(getDataFilePathAbsolute(fn)))
                    //{
                    //    readTime.clear();
                    //    readTime.start();
                    //    fn = getDataFilePathAbsolute(fn);
                    //    readStateModFile_Announce1(comp);
                    //    comp.setData(StateMod_Reservoir.readStateModFile(fn));
                    //}
                }
                catch (Exception e)
                {
                    Message.printWarning(1, routine, "Error reading reservoir station file:\n\"" + fn + warningEndString + " (See log file for more on error:" + e + ")");
                    Message.printWarning(3, routine, e);
                    comp.setErrorReadingInputFile(true);
                }
                finally
                {
                    comp.setDirty(false);
                    readTime.stop();
                    readStateModFile_Announce2(comp, readTime.getSeconds());
                }

                // Diversion stations file (.dds)...

                try
                {
                    fn = response_props.getValue("Diversion_Station");
                    // Always set the file name...
                    comp = getComponentForComponentType(COMP_DIVERSION_STATIONS);
                    if ((comp != null) && (!string.ReferenceEquals(fn, null)))
                    {
                        comp.setDataFileName(fn);
                    }
                    // Read the data...
                    //if (readData && (!string.ReferenceEquals(fn, null)) && !fileIsEmpty(getDataFilePathAbsolute(fn)))
                    //{
                    //    readTime.clear();
                    //    readTime.start();
                    //    fn = getDataFilePathAbsolute(fn);
                    //    readStateModFile_Announce1(comp);
                    //    comp.setData(StateMod_Diversion.readStateModFile(fn));
                    //}
                }
                catch (Exception e)
                {
                    Message.printWarning(1, routine, "Error reading diversion station file:\n\"" + fn + warningEndString + " (See log file for more on error:" + e + ")");
                    Message.printWarning(3, routine, e);
                    comp.setErrorReadingInputFile(true);
                }
                finally
                {
                    comp.setDirty(false);
                    readTime.stop();
                    readStateModFile_Announce2(comp, readTime.getSeconds());
                }

                // Stream gage stations file (.ris)...

                try
                {
                    fn = response_props.getValue("StreamGage_Station");
                    // Always set the file name...
                    comp = getComponentForComponentType(COMP_STREAMGAGE_STATIONS);
                    if ((comp != null) && (!string.ReferenceEquals(fn, null)))
                    {
                        comp.setDataFileName(fn);
                    }
                    // Read the data...
                    //if (readData && (!string.ReferenceEquals(fn, null)) && !fileIsEmpty(getDataFilePathAbsolute(fn)))
                    //{
                    //    readTime.clear();
                    //    readTime.start();
                    //    fn = getDataFilePathAbsolute(fn);
                    //    readStateModFile_Announce1(comp);
                    //    comp.setData(StateMod_StreamGage.readStateModFile(fn));
                    //}
                }
                catch (Exception e)
                {
                    Message.printWarning(1, routine, "Error reading stream gage stations file:\n\"" + fn + warningEndString + " (See log file for more on error:" + e + ")");
                    Message.printWarning(3, routine, e);
                    comp.setErrorReadingInputFile(true);
                }
                finally
                {
                    comp.setDirty(false);
                    readTime.stop();
                    readStateModFile_Announce2(comp, readTime.getSeconds());
                }

                // If not a free-format data set with separate stream estimate station,
                // re-read the legacy stream station file because some stations will be stream estimate stations.
                // If free format, get the file name...

                try
                {
                    fn = response_props.getValue("StreamEstimate_Station");
                    if (string.ReferenceEquals(fn, null))
                    {
                        // Get from the stream gage component because Ray has not adopted a separate stream
                        // estimate file...
                        Message.printStatus(2, routine, "Using StreamGage_Station for StreamEstimage_Station (no separate 2nd file).");
                        comp = getComponentForComponentType(COMP_STREAMGAGE_STATIONS);
                        if (comp == null)
                        {
                            fn = null;
                        }
                        else
                        {
                            fn = comp.getDataFileName();
                        }
                    }
                    // Always set the file name...
                    comp = getComponentForComponentType(COMP_STREAMESTIMATE_STATIONS);
                    if ((comp != null) && (!string.ReferenceEquals(fn, null)))
                    {
                        comp.setDataFileName(fn);
                    }
                    // (Re)read the data...
                    //if (readData && (!string.ReferenceEquals(fn, null)) && !fileIsEmpty(getDataFilePathAbsolute(fn)))
                    //{
                    //    readTime.clear();
                    //    readTime.start();
                    //    // Use the relative path...
                    //    fn = getDataFilePathAbsolute(fn);
                    //    readStateModFile_Announce1(comp);
                    //    comp.setData(StateMod_StreamEstimate.readStateModFile(fn));
                    //}
                }
                catch (Exception e)
                {
                    Message.printWarning(1, routine, "Error reading stream estimate stations file:\n\"" + fn + warningEndString + " (See log file for more on error:" + e + ")");
                    Message.printWarning(3, routine, e);
                    comp.setErrorReadingInputFile(true);
                }
                finally
                {
                    comp.setDirty(false);
                    readTime.stop();
                    readStateModFile_Announce2(comp, readTime.getSeconds());
                }

                // Instream flow stations file (.ifs)...

                try
                {
                    fn = response_props.getValue("Instreamflow_Station");
                    // Always set the file name...
                    comp = getComponentForComponentType(COMP_INSTREAM_STATIONS);
                    if ((comp != null) && (!string.ReferenceEquals(fn, null)))
                    {
                        comp.setDataFileName(fn);
                    }
                    // Read the data...
                    //if (readData && (!string.ReferenceEquals(fn, null)) && !fileIsEmpty(getDataFilePathAbsolute(fn)))
                    //{
                    //    readTime.clear();
                    //    readTime.start();
                    //    fn = getDataFilePathAbsolute(fn);
                    //    readStateModFile_Announce1(comp);
                    //    comp.setData(StateMod_InstreamFlow.readStateModFile(fn));
                    //}
                }
                catch (Exception e)
                {
                    Message.printWarning(1, routine, "Error reading instream flow station file:\n\"" + fn + warningEndString + " (See log file for more on error:" + e + ")");
                    Message.printWarning(3, routine, e);
                    comp.setErrorReadingInputFile(true);
                }
                finally
                {
                    comp.setDirty(false);
                    readTime.stop();
                    readStateModFile_Announce2(comp, readTime.getSeconds());
                }

                // Well stations...

                try
                {
                    fn = response_props.getValue("Well_Station");
                    // Always set the file name...
                    comp = getComponentForComponentType(COMP_WELL_STATIONS);
                    if ((comp != null) && (!string.ReferenceEquals(fn, null)))
                    {
                        comp.setDataFileName(fn);
                    }
                    // Read the data...
                    //if (readData && hasWellData(false) && (!string.ReferenceEquals(fn, null)) && !fileIsEmpty(getDataFilePathAbsolute(fn)))
                    //{
                    //    readTime.clear();
                    //    readTime.start();
                    //    fn = getDataFilePathAbsolute(fn);
                    //    readStateModFile_Announce1(comp);
                    //    comp.setData(StateMod_Well.readStateModFile(fn));
                    //}
                }
                catch (Exception e)
                {
                    Message.printWarning(1, routine, "Error reading well station file:\n\"" + fn + warningEndString + " (See log file for more on error:" + e + ")");
                    Message.printWarning(3, routine, e);
                    comp.setErrorReadingInputFile(true);
                }
                finally
                {
                    comp.setDirty(false);
                    readTime.stop();
                    readStateModFile_Announce2(comp, readTime.getSeconds());
                }

                // Plans...

                try
                {
                    fn = response_props.getValue("Plan_Data");
                    // Always set the file name...
                    comp = getComponentForComponentType(COMP_PLANS);
                    if (comp == null)
                    {
                        Message.printWarning(2, routine, "Unable to look up plans component " + COMP_PLANS);
                    }
                    if ((comp != null) && (!string.ReferenceEquals(fn, null)))
                    {
                        comp.setDataFileName(fn);
                    }
                    // Read the data...
                    //if (readData && (!string.ReferenceEquals(fn, null)) && !fileIsEmpty(getDataFilePathAbsolute(fn)))
                    //{
                    //    readTime.clear();
                    //    readTime.start();
                    //    fn = getDataFilePathAbsolute(fn);
                    //    readStateModFile_Announce1(comp);
                    //    comp.setData(StateMod_Plan.readStateModFile(fn));
                    //}
                }
                catch (Exception e)
                {
                    Message.printWarning(1, routine, "Error reading plan file:\n\"" + fn + warningEndString + " (See log file for more on error:" + e + ")");
                    Message.printWarning(3, routine, e);
                    comp.setErrorReadingInputFile(true);
                }
                finally
                {
                    comp.setDirty(false);
                    readTime.stop();
                    readStateModFile_Announce2(comp, readTime.getSeconds());
                }

                // Plan well augmentation (.plw)...

                try
                {
                    fn = response_props.getValue("Plan_Wells");
                    // Always set the file name...
                    comp = getComponentForComponentType(COMP_PLAN_WELL_AUGMENTATION);
                    if ((comp != null) && (!string.ReferenceEquals(fn, null)))
                    {
                        comp.setDataFileName(fn);
                    }
                    // Read the data...
                    //if (readData && (!string.ReferenceEquals(fn, null)) && !fileIsEmpty(getDataFilePathAbsolute(fn)))
                    //{
                    //    readTime.clear();
                    //    readTime.start();
                    //    fn = getDataFilePathAbsolute(fn);
                    //    readStateModFile_Announce1(comp);
                    //    comp.setData(StateMod_Plan_WellAugmentation.readStateModFile(fn));
                    //}
                }
                catch (Exception e)
                {
                    Message.printWarning(1, routine, "Error reading plan well augmentation file:\n\"" + fn + warningEndString + " (See log file for more on error:" + e + ")");
                    Message.printWarning(3, routine, e);
                    comp.setErrorReadingInputFile(true);
                }
                finally
                {
                    comp.setDirty(false);
                    readTime.stop();
                    readStateModFile_Announce2(comp, readTime.getSeconds());
                }

                // Plan return (.prf)...

                try
                {
                    fn = response_props.getValue("Plan_Return");
                    // Always set the file name...
                    comp = getComponentForComponentType(COMP_PLAN_RETURN);
                    if ((comp != null) && (!string.ReferenceEquals(fn, null)))
                    {
                        comp.setDataFileName(fn);
                    }
                    // Read the data...
                    //if (readData && (!string.ReferenceEquals(fn, null)) && !fileIsEmpty(getDataFilePathAbsolute(fn)))
                    //{
                    //    readTime.clear();
                    //    readTime.start();
                    //    fn = getDataFilePathAbsolute(fn);
                    //    readStateModFile_Announce1(comp);
                    //    comp.setData(StateMod_ReturnFlow.readStateModFile(fn, COMP_PLAN_RETURN));
                    //}
                }
                catch (Exception e)
                {
                    Message.printWarning(1, routine, "Error reading plan return file:\n\"" + fn + warningEndString + " (See log file for more on error:" + e + ")");
                    Message.printWarning(3, routine, e);
                    comp.setErrorReadingInputFile(true);
                }
                finally
                {
                    comp.setDirty(false);
                    readTime.stop();
                    readStateModFile_Announce2(comp, readTime.getSeconds());
                }

                // Instream flow rights file (.ifr)...

                try
                {
                    fn = response_props.getValue("Instreamflow_Right");
                    // Always set the file name...
                    comp = getComponentForComponentType(COMP_INSTREAM_RIGHTS);
                    if ((comp != null) && (!string.ReferenceEquals(fn, null)))
                    {
                        comp.setDataFileName(fn);
                    }
                    // Read the data...
                    //if (readData && (!string.ReferenceEquals(fn, null)) && !fileIsEmpty(getDataFilePathAbsolute(fn)))
                    //{
                    //    readTime.clear();
                    //    readTime.start();
                    //    fn = getDataFilePathAbsolute(fn);
                    //    readStateModFile_Announce1(comp);
                    //    comp.setData(StateMod_InstreamFlowRight.readStateModFile(fn));
                    //    Message.printStatus(1, routine, "Connecting instream flow rights to stations.");
                    //    StateMod_InstreamFlow.connectAllRights((System.Collections.IList)getComponentForComponentType(COMP_INSTREAM_STATIONS).getData(), (System.Collections.IList)comp.getData());
                    //}
                }
                catch (Exception e)
                {
                    Message.printWarning(1, routine, "Error reading instream flow rights file:\n\"" + fn + warningEndString + " (See log file for more on error:" + e + ")");
                    Message.printWarning(3, routine, e);
                    comp.setErrorReadingInputFile(true);
                }
                finally
                {
                    comp.setDirty(false);
                    readTime.stop();
                    readStateModFile_Announce2(comp, readTime.getSeconds());
                }

                // Reservoir rights file (.rer)...

                try
                {
                    fn = response_props.getValue("Reservoir_Right");
                    // Always set the file name...
                    comp = getComponentForComponentType(COMP_RESERVOIR_RIGHTS);
                    if ((comp != null) && (!string.ReferenceEquals(fn, null)))
                    {
                        comp.setDataFileName(fn);
                    }
                    // Read the data...
                    //if (readData && (!string.ReferenceEquals(fn, null)) && !fileIsEmpty(getDataFilePathAbsolute(fn)))
                    //{
                    //    readTime.clear();
                    //    readTime.start();
                    //    fn = getDataFilePathAbsolute(fn);
                    //    readStateModFile_Announce1(comp);
                    //    comp.setData(StateMod_ReservoirRight.readStateModFile(fn));
                    //    Message.printStatus(1, routine, "Connecting reservoir rights with reservoir stations.");
                    //    StateMod_Reservoir.connectAllRights((System.Collections.IList)getComponentForComponentType(COMP_RESERVOIR_STATIONS).getData(), (System.Collections.IList)comp.getData());
                    //}
                }
                catch (Exception e)
                {
                    Message.printWarning(1, routine, "Error reading reservoir rights file:\n\"" + fn + warningEndString + " (See log file for more on error:" + e + ")");
                    Message.printWarning(3, routine, e);
                    comp.setErrorReadingInputFile(true);
                }
                finally
                {
                    comp.setDirty(false);
                    readTime.stop();
                    readStateModFile_Announce2(comp, readTime.getSeconds());
                }

                // Diversion rights file (.ddr)...

                try
                {
                    fn = response_props.getValue("Diversion_Right");
                    // Always set the file name...
                    comp = getComponentForComponentType(COMP_DIVERSION_RIGHTS);
                    if ((comp != null) && (!string.ReferenceEquals(fn, null)))
                    {
                        comp.setDataFileName(fn);
                    }
                    // Read the data...
                    //if (readData && (!string.ReferenceEquals(fn, null)) && !fileIsEmpty(getDataFilePathAbsolute(fn)))
                    //{
                    //    readTime.clear();
                    //    readTime.start();
                    //    fn = getDataFilePathAbsolute(fn);
                    //    readStateModFile_Announce1(comp);
                    //    comp.setData(StateMod_DiversionRight.readStateModFile(fn));
                    //    Message.printStatus(1, routine, "Connecting diversion rights to diversion stations");
                    //    StateMod_Diversion.connectAllRights((System.Collections.IList)getComponentForComponentType(COMP_DIVERSION_STATIONS).getData(), (System.Collections.IList)comp.getData());
                    //}
                }
                catch (Exception e)
                {
                    Message.printWarning(1, routine, "Error reading diversion rights file:\n\"" + fn + warningEndString + " (See log file for more on error:" + e + ")");
                    Message.printWarning(3, routine, e);
                    comp.setErrorReadingInputFile(true);
                }
                finally
                {
                    comp.setDirty(false);
                    readTime.stop();
                    readStateModFile_Announce2(comp, readTime.getSeconds());
                }

                // Operational rights file (.opr)...

                try
                {
                    fn = response_props.getValue("Operational_Right");
                    // Always set the file name...
                    comp = getComponentForComponentType(COMP_OPERATION_RIGHTS);
                    if ((comp != null) && (!string.ReferenceEquals(fn, null)))
                    {
                        comp.setDataFileName(fn);
                    }
                    // Read the data...
                    //if (readData && (!string.ReferenceEquals(fn, null)) && !fileIsEmpty(getDataFilePathAbsolute(fn)))
                    //{
                    //    readTime.clear();
                    //    readTime.start();
                    //    fn = getDataFilePathAbsolute(fn);
                    //    readStateModFile_Announce1(comp);
                    //    comp.setData(StateMod_OperationalRight.readStateModFile(fn, this));
                    //}
                }
                catch (Exception e)
                {
                    Message.printWarning(1, routine, "Error reading operational rights file:\n\"" + fn + warningEndString + " (See log file for more on error:" + e + ")");
                    comp.setErrorReadingInputFile(true);
                    Message.printWarning(3, routine, e);
                }
                finally
                {
                    comp.setDirty(false);
                    readTime.stop();
                    readStateModFile_Announce2(comp, readTime.getSeconds());
                }

                // Well rights file (.wer)...

                try
                {
                    fn = response_props.getValue("Well_Right");
                    // Always set the file name...
                    comp = getComponentForComponentType(COMP_WELL_RIGHTS);
                    if ((comp != null) && (!string.ReferenceEquals(fn, null)))
                    {
                        comp.setDataFileName(fn);
                    }
                    // Read data...
                    //if (readData && (!string.ReferenceEquals(fn, null)) && !fileIsEmpty(getDataFilePathAbsolute(fn)))
                    //{
                    //    readTime.clear();
                    //    readTime.start();
                    //    fn = getDataFilePathAbsolute(fn);
                    //    readStateModFile_Announce1(comp);
                    //    comp.setData(StateMod_WellRight.readStateModFile(fn));
                    //    Message.printStatus(1, routine, "Connecting well rights to well stations.");
                    //    StateMod_Well.connectAllRights((System.Collections.IList)getComponentForComponentType(COMP_WELL_STATIONS).getData(), (System.Collections.IList)comp.getData());
                    //}
                }
                catch (Exception e)
                {
                    Message.printWarning(1, routine, "Error reading well rights file:\n\"" + fn + warningEndString + " (See log file for more on error:" + e + ")");
                    Message.printWarning(3, routine, e);
                    comp.setErrorReadingInputFile(true);
                }
                finally
                {
                    comp.setDirty(false);
                    readTime.stop();
                    readStateModFile_Announce2(comp, readTime.getSeconds());
                }

                try
                {
                    fn = response_props.getValue("Precipitation_Monthly");
                    // Always set the file name in the component...
                    comp = getComponentForComponentType(COMP_PRECIPITATION_TS_MONTHLY);
                    if ((comp != null) && (!string.ReferenceEquals(fn, null)))
                    {
                        comp.setDataFileName(fn);
                    }
                    //Now read the file...
                    if (readData && (!string.ReferenceEquals(fn, null)) && !fileIsEmpty(getDataFilePathAbsolute(fn)))
                    {
                        readTime.clear();
                        readTime.start();
                        fn = getDataFilePathAbsolute(fn);
                        readStateModFile_Announce1(comp);
                        IList<TS> v = StateMod_TS.readTimeSeriesList(fn, null, null, null, true);
                        if (v == null)
                        {
                            v = new List<TS>();
                        }
                        size = v.Count;
                        // TODO Old-style data that may be removed in new StateMod...
                        setNumpre(size);
                        for (i = 0; i < size; i++)
                        {
                            ((MonthTS)v[i]).setDataType(lookupTimeSeriesDataType(COMP_PRECIPITATION_TS_MONTHLY));
                        }
                        comp.setData(v);
                    }
                }
                catch (Exception e)
                {
                    Message.printWarning(1, routine, "Error reading precipitation time series file:\n\"" + fn + warningEndString + " (See log file for more on error:" + e + ")");
                    Message.printWarning(3, routine, e);
                    comp.setErrorReadingInputFile(true);
                }
                finally
                {
                    comp.setDirty(false);
                    readTime.stop();
                    readStateModFile_Announce2(comp, readTime.getSeconds());
                }

                // Precipitation TS yearly file (.pra) - always read...

                try
                {
                    fn = response_props.getValue("Precipitation_Annual");
                    // Always set the file name in the component...
                    Message.printStatus(2, routine, "StateMod GUI does not yet handle annual precipitation data.");
                    comp = getComponentForComponentType(COMP_PRECIPITATION_TS_YEARLY);
                    if ((comp != null) && (!string.ReferenceEquals(fn, null)))
                    {
                        comp.setDataFileName(fn);
                    }
                    // Now read the file...
                    if (readData && (!string.ReferenceEquals(fn, null)) && !fileIsEmpty(getDataFilePathAbsolute(fn)))
                    {
                        readTime.clear();
                        readTime.start();
                        fn = getDataFilePathAbsolute(fn);
                        readStateModFile_Announce1(comp);
                        IList<TS> v = StateMod_TS.readTimeSeriesList(fn, null, null, null, true);
                        if (v == null)
                        {
                            v = new List<TS>();
                        }
                        size = v.Count;
                        // TODO Old-style data that may be removed in new StateMod...
                        setNumpre(size);
                        for (i = 0; i < size; i++)
                        {
                            ((MonthTS)v[i]).setDataType(lookupTimeSeriesDataType(COMP_PRECIPITATION_TS_YEARLY));
                        }
                        comp.setData(v);
                    }
                }
                catch (Exception e)
                {
                    Message.printWarning(1, routine, "Error reading annual precipitation time series file:\n\"" + fn + warningEndString + " (See log file for more on error:" + e + ")");
                    Message.printWarning(3, routine, e);
                    comp.setErrorReadingInputFile(true);
                }
                finally
                {
                    comp.setDirty(false);
                    readTime.stop();
                    readStateModFile_Announce2(comp, readTime.getSeconds());
                }

                // Evaporation time series file monthly (.eva) - always read...

                try
                {
                    fn = response_props.getValue("Evaporation_Monthly");
                    // Always set the file name...
                    comp = getComponentForComponentType(COMP_EVAPORATION_TS_MONTHLY);
                    if ((comp != null) && (!string.ReferenceEquals(fn, null)))
                    {
                        comp.setDataFileName(fn);
                    }
                    // Read the data...
                    //if (readData && (!string.ReferenceEquals(fn, null)) && !fileIsEmpty(getDataFilePathAbsolute(fn)))
                    //{
                    //    readTime.clear();
                    //    readTime.start();
                    //    fn = getDataFilePathAbsolute(fn);
                    //    readStateModFile_Announce1(comp);
                    //    System.Collections.IList v = StateMod_TS.readTimeSeriesList(fn, null, null, null, true);
                    //    if (v == null)
                    //    {
                    //        v = new List<object>();
                    //    }
                    //    size = v.Count;
                    //    // TODO Old-style data that may be removed in new StateMod...
                    //    setNumeva(size);
                    //    for (i = 0; i < size; i++)
                    //    {
                    //        ((MonthTS)v[i]).setDataType(lookupTimeSeriesDataType(COMP_EVAPORATION_TS_MONTHLY));
                    //    }
                    //    comp.setData(v);
                    //}
                }
                catch (Exception e)
                {
                    Message.printWarning(1, routine, "Error reading evaporation time series file:\n\"" + fn + warningEndString + " (See log file for more on error:" + e + ")");
                    Message.printWarning(3, routine, e);
                    comp.setErrorReadingInputFile(true);
                }
                finally
                {
                    comp.setDirty(false);
                    readTime.stop();
                    readStateModFile_Announce2(comp, readTime.getSeconds());
                }

                // Evaporation time series file yearly (.eva) - always read...

                try
                {
                    fn = response_props.getValue("Evaporation_Annual");
                    Message.printStatus(2, routine, "StateMod GUI does not yet handle annual evaporation data.");

                    // Always set the file name...
                    comp = getComponentForComponentType(COMP_EVAPORATION_TS_YEARLY);
                    if ((comp != null) && (!string.ReferenceEquals(fn, null)))
                    {
                        comp.setDataFileName(fn);
                    }
                    // Read the data...
                    //if (readData && (!string.ReferenceEquals(fn, null)) && !fileIsEmpty(getDataFilePathAbsolute(fn)))
                    //{
                    //    readTime.clear();
                    //    readTime.start();
                    //    fn = getDataFilePathAbsolute(fn);
                    //    readStateModFile_Announce1(comp);
                    //    System.Collections.IList v = StateMod_TS.readTimeSeriesList(fn, null, null, null, true);
                    //    if (v == null)
                    //    {
                    //        v = new List<object>();
                    //    }
                    //    size = v.Count;
                    //    // TODO Old-style data that may be removed in new StateMod...
                    //    setNumeva(size);
                    //    for (i = 0; i < size; i++)
                    //    {
                    //        ((MonthTS)v[i]).setDataType(lookupTimeSeriesDataType(COMP_EVAPORATION_TS_YEARLY));
                    //    }
                    //    comp.setData(v);
                    //}
                }
                catch (Exception e)
                {
                    Message.printWarning(1, routine, "Error reading annual evaporation time series file:\n\"" + fn + warningEndString + " (See log file for more on error:" + e + ")");
                    Message.printWarning(3, routine, e);
                    comp.setErrorReadingInputFile(true);
                }
                finally
                {
                    comp.setDirty(false);
                    readTime.stop();
                    readStateModFile_Announce2(comp, readTime.getSeconds());
                }

                // Stream gage natural flow time series (.rim or .xbm) - always read...

                DataSetComponent comp2 = null;
                try
                {
                    fn = response_props.getValue("Stream_Base_Monthly");
                    // Always set the file name...
                    comp = getComponentForComponentType(COMP_STREAMGAGE_NATURAL_FLOW_TS_MONTHLY);
                    if ((comp != null) && (!string.ReferenceEquals(fn, null)))
                    {
                        comp.setDataFileName(fn);
                    }
                    // Read the data...
                    //if (readData && (!string.ReferenceEquals(fn, null)) && !fileIsEmpty(getDataFilePathAbsolute(fn)))
                    //{
                    //    readTime.clear();
                    //    readTime.start();
                    //    fn = getDataFilePathAbsolute(fn);
                    //    readStateModFile_Announce1(comp);
                    //    System.Collections.IList v = StateMod_TS.readTimeSeriesList(fn, null, null, null, true);
                    //    if (v == null)
                    //    {
                    //        v = new List<object>();
                    //    }
                    //    size = v.Count;
                    //    for (i = 0; i < size; i++)
                    //    {
                    //        ((MonthTS)v[i]).setDataType(lookupTimeSeriesDataType(COMP_STREAMGAGE_NATURAL_FLOW_TS_MONTHLY));
                    //    }
                    //    comp.setData(v);

                    //    // The StreamGage and StreamEstimate groups share the same natural flow time series files...

                    //    comp2 = getComponentForComponentType(COMP_STREAMESTIMATE_NATURAL_FLOW_TS_MONTHLY);
                    //    comp2.setDataFileName(comp.getDataFileName());
                    //    comp2.setData(v);
                    //}
                }
                catch (Exception e)
                {
                    Message.printWarning(1, routine, "Error reading natural flow time series (monthly) file:\n\"" + fn + warningEndString + " (See log file for more on error:" + e + ")");
                    Message.printWarning(3, routine, e);
                    comp.setErrorReadingInputFile(true);
                }
                finally
                {
                    comp.setDirty(false);
                    if (comp2 != null)
                    {
                        // Never read data above so no need to call the following
                        comp2.setDirty(false);
                    }
                    readTime.stop();
                    readStateModFile_Announce2(comp, readTime.getSeconds());
                }

                // Diversion direct flow demand time series (monthly) file (.ddm)...

                try
                {
                    fn = response_props.getValue("Diversion_Demand_Monthly");
                    // Always set the file name in the component...
                    comp = getComponentForComponentType(COMP_DEMAND_TS_MONTHLY);
                    if ((comp != null) && (!string.ReferenceEquals(fn, null)))
                    {
                        comp.setDataFileName(fn);
                    }
                    // Now read the data...
                    //if (readData && __readTimeSeries && (!string.ReferenceEquals(fn, null)) && !fileIsEmpty(getDataFilePathAbsolute(fn)))
                    //{
                    //    readTime.clear();
                    //    readTime.start();
                    //    readStateModFile_Announce1(comp);
                    //    fn = getDataFilePathAbsolute(fn);
                    //    System.Collections.IList v = StateMod_TS.readTimeSeriesList(fn, null, null, null, true);
                    //    if (v == null)
                    //    {
                    //        v = new List<object>();
                    //    }
                    //    size = v.Count;
                    //    for (i = 0; i < size; i++)
                    //    {
                    //        ((MonthTS)v[i]).setDataType(lookupTimeSeriesDataType(COMP_DEMAND_TS_MONTHLY));
                    //    }
                    //    comp.setData(v);
                    //}
                }
                catch (Exception e)
                {
                    Message.printWarning(1, routine, "Error reading demand time series (monthly) file:\n\"" + fn + warningEndString + " (See log file for more on error:" + e + ")");
                    Message.printWarning(3, routine, e);
                    comp.setErrorReadingInputFile(true);
                }
                finally
                {
                    comp.setDirty(false);
                    readTime.stop();
                    readStateModFile_Announce2(comp, readTime.getSeconds());
                }

                // Direct flow demand time series override (monthly) file (.ddo)...

                try
                {
                    fn = response_props.getValue("Diversion_DemandOverride_Monthly");
                    // Set the file name in the component...
                    comp = getComponentForComponentType(COMP_DEMAND_TS_OVERRIDE_MONTHLY);
                    if ((comp != null) && (!string.ReferenceEquals(fn, null)))
                    {
                        comp.setDataFileName(fn);
                    }
                    // Now read the file...
                    //if (readData && __readTimeSeries && (!string.ReferenceEquals(fn, null)) && !fileIsEmpty(getDataFilePathAbsolute(fn)))
                    //{
                    //    readTime.clear();
                    //    readTime.start();
                    //    readStateModFile_Announce1(comp);
                    //    fn = getDataFilePathAbsolute(fn);
                    //    System.Collections.IList v = StateMod_TS.readTimeSeriesList(fn, null, null, null, true);
                    //    if (v == null)
                    //    {
                    //        v = new List<object>();
                    //    }
                    //    size = v.Count;
                    //    for (i = 0; i < size; i++)
                    //    {
                    //        ((MonthTS)v[i]).setDataType(lookupTimeSeriesDataType(COMP_DEMAND_TS_OVERRIDE_MONTHLY));
                    //    }
                    //    comp.setData(v);
                    //}
                }
                catch (Exception e)
                {
                    Message.printWarning(1, routine, "Error reading demand time series override (monthly) file:\n\"" + fn + warningEndString + " (See log file for more on error:" + e + ")");
                    Message.printWarning(3, routine, e);
                    comp.setErrorReadingInputFile(true);
                }
                finally
                {
                    comp.setDirty(false);
                    readTime.stop();
                    readStateModFile_Announce2(comp, readTime.getSeconds());
                }

                // Direct flow demand time series average (monthly) file (.dda)...

                try
                {
                    fn = response_props.getValue("Diversion_Demand_AverageMonthly");
                    // Set the file name in the component...
                    comp = getComponentForComponentType(COMP_DEMAND_TS_AVERAGE_MONTHLY);
                    if ((comp != null) && (!string.ReferenceEquals(fn, null)))
                    {
                        comp.setDataFileName(fn);
                    }
                    // Now read the data file...
                    //if (readData && __readTimeSeries && (!string.ReferenceEquals(fn, null)) && !fileIsEmpty(getDataFilePathAbsolute(fn)))
                    //{
                    //    readTime.clear();
                    //    readTime.start();
                    //    readStateModFile_Announce1(comp);
                    //    fn = getDataFilePathAbsolute(fn);
                    //    System.Collections.IList v = StateMod_TS.readTimeSeriesList(fn, null, null, null, true);
                    //    if (v == null)
                    //    {
                    //        v = new List<object>();
                    //    }
                    //    size = v.Count;
                    //    for (i = 0; i < size; i++)
                    //    {
                    //        ((MonthTS)v[i]).setDataType(lookupTimeSeriesDataType(COMP_DEMAND_TS_AVERAGE_MONTHLY));
                    //    }
                    //    comp.setData(v);
                    //}
                }
                catch (Exception e)
                {
                    Message.printWarning(1, routine, "Error reading demand time series (average monthly) file:\n\"" + fn + warningEndString + " (See log file for more on error:" + e + ")");
                    Message.printWarning(3, routine, e);
                    comp.setErrorReadingInputFile(true);
                }
                finally
                {
                    comp.setDirty(false);
                    readTime.stop();
                    readStateModFile_Announce2(comp, readTime.getSeconds());
                }

                // Monthly instream flow demand...

                try
                {
                    fn = response_props.getValue("Instreamflow_Demand_Monthly");
                    // Set the file name in the component...
                    comp = getComponentForComponentType(COMP_INSTREAM_DEMAND_TS_MONTHLY);
                    if ((comp != null) && (!string.ReferenceEquals(fn, null)))
                    {
                        comp.setDataFileName(fn);
                    }
                    // Read the data file...
                    //if (readData && (!string.ReferenceEquals(fn, null)) && !fileIsEmpty(getDataFilePathAbsolute(fn)))
                    //{
                    //    readTime.clear();
                    //    readTime.start();
                    //    readStateModFile_Announce1(comp);
                    //    fn = getDataFilePathAbsolute(fn);
                    //    System.Collections.IList v = StateMod_TS.readTimeSeriesList(fn, null, null, null, true);
                    //    if (v == null)
                    //    {
                    //        v = new List<object>();
                    //    }
                    //    size = v.Count;
                    //    for (i = 0; i < size; i++)
                    //    {
                    //        ((MonthTS)v[i]).setDataType(lookupTimeSeriesDataType(COMP_INSTREAM_DEMAND_TS_MONTHLY));
                    //    }
                    //    comp.setData(v);
                    //}
                }
                catch (Exception e)
                {
                    Message.printWarning(1, routine, "Error reading monthly instream flow demand time series file:\n\"" + fn + warningEndString + " (See log file for more on error:" + e + ")");
                    Message.printWarning(3, routine, e);
                    comp.setErrorReadingInputFile(true);
                }
                finally
                {
                    comp.setDirty(false);
                    readTime.stop();
                    readStateModFile_Announce2(comp, readTime.getSeconds());
                }

                // Instream demand time series (average monthly) file (.ifa)...

                try
                {
                    fn = response_props.getValue("Instreamflow_Demand_AverageMonthly");
                    // Set the file name in the component...
                    comp = getComponentForComponentType(COMP_INSTREAM_DEMAND_TS_AVERAGE_MONTHLY);
                    if ((comp != null) && (!string.ReferenceEquals(fn, null)))
                    {
                        comp.setDataFileName(fn);
                    }
                    // Now read the data file...
                    //if (readData && __readTimeSeries && (!string.ReferenceEquals(fn, null)) && !fileIsEmpty(getDataFilePathAbsolute(fn)))
                    //{
                    //    readTime.clear();
                    //    readTime.start();
                    //    fn = getDataFilePathAbsolute(fn);
                    //    readStateModFile_Announce1(comp);
                    //    System.Collections.IList v = StateMod_TS.readTimeSeriesList(fn, null, null, null, true);
                    //    if (v == null)
                    //    {
                    //        v = new List<object>();
                    //    }
                    //    size = v.Count;
                    //    for (i = 0; i < size; i++)
                    //    {
                    //        ((MonthTS)v[i]).setDataType(lookupTimeSeriesDataType(COMP_INSTREAM_DEMAND_TS_AVERAGE_MONTHLY));
                    //    }
                    //    comp.setData(v);
                    //}
                }
                catch (Exception e)
                {
                    Message.printWarning(1, routine, "Error reading instream flow demand time series file:\n\"" + fn + warningEndString + " (See log file for more on error:" + e + ")");
                    Message.printWarning(3, routine, e);
                    comp.setErrorReadingInputFile(true);
                }
                finally
                {
                    comp.setDirty(false);
                    readTime.stop();
                    readStateModFile_Announce2(comp, readTime.getSeconds());
                }

                // Well demand time series (monthly) file (.wem)...

                try
                {
                    fn = response_props.getValue("Well_Demand_Monthly");
                    // Set the file name in the component...
                    comp = getComponentForComponentType(COMP_WELL_DEMAND_TS_MONTHLY);
                    if ((comp != null) && (!string.ReferenceEquals(fn, null)))
                    {
                        comp.setDataFileName(fn);
                    }
                    // Now read the data file...
                    //if (readData && __readTimeSeries && (!string.ReferenceEquals(fn, null)) && !fileIsEmpty(getDataFilePathAbsolute(fn)))
                    //{
                    //    readTime.clear();
                    //    readTime.start();
                    //    fn = getDataFilePathAbsolute(fn);
                    //    readStateModFile_Announce1(comp);
                    //    System.Collections.IList v = StateMod_TS.readTimeSeriesList(fn, null, null, null, true);
                    //    if (v == null)
                    //    {
                    //        v = new List<object>();
                    //    }
                    //    size = v.Count;
                    //    for (i = 0; i < size; i++)
                    //    {
                    //        ((MonthTS)v[i]).setDataType(lookupTimeSeriesDataType(COMP_WELL_DEMAND_TS_MONTHLY));
                    //    }
                    //    comp.setData(v);
                    //}
                }
                catch (Exception e)
                {
                    Message.printWarning(1, routine, "Error reading well demand time series (monthly) file:\n" + fn + warningEndString + " (See log file for more on error:" + e + ")");
                    Message.printWarning(3, routine, e);
                    comp.setErrorReadingInputFile(true);
                }
                finally
                {
                    comp.setDirty(false);
                    readTime.stop();
                    readStateModFile_Announce2(comp, readTime.getSeconds());
                }

                // Delay file (monthly) file (.dly)...

                try
                {
                    fn = response_props.getValue("DelayTable_Monthly");
                    // Always set the file name...
                    comp = getComponentForComponentType(COMP_DELAY_TABLES_MONTHLY);
                    if ((comp != null) && (!string.ReferenceEquals(fn, null)))
                    {
                        comp.setDataFileName(fn);
                    }
                    // Read the data...
                    //if (readData && (!string.ReferenceEquals(fn, null)) && !fileIsEmpty(getDataFilePathAbsolute(fn)))
                    //{
                    //    readTime.clear();
                    //    readTime.start();
                    //    fn = getDataFilePathAbsolute(fn);
                    //    readStateModFile_Announce1(comp);
                    //    comp.setData(StateMod_DelayTable.readStateModFile(fn, true, getInterv()));
                    //}
                }
                catch (Exception e)
                {
                    Message.printWarning(1, routine, "Error reading delay table (monthly) file:\n\"" + fn + warningEndString + " (See log file for more on error:" + e + ")");
                    Message.printWarning(3, routine, e);
                    comp.setErrorReadingInputFile(true);
                }
                finally
                {
                    comp.setDirty(false);
                    readTime.stop();
                    readStateModFile_Announce2(comp, readTime.getSeconds());
                }

                // Reservoir target time series (monthly) file (.tar)...

                try
                {
                    fn = response_props.getValue("Reservoir_Target_Monthly");
                    // Set the file name in the component...
                    comp = getComponentForComponentType(COMP_RESERVOIR_TARGET_TS_MONTHLY);
                    if ((comp != null) && (!string.ReferenceEquals(fn, null)))
                    {
                        comp.setDataFileName(fn);
                    }
                    // Now read the data file...
                    //if (readData && __readTimeSeries && (!string.ReferenceEquals(fn, null)) && !fileIsEmpty(getDataFilePathAbsolute(fn)))
                    //{
                    //    readTime.clear();
                    //    readTime.start();
                    //    fn = getDataFilePathAbsolute(fn);
                    //    readStateModFile_Announce1(comp);
                    //    System.Collections.IList v = StateMod_TS.readTimeSeriesList(fn, null, null, null, true);
                    //    if (v == null)
                    //    {
                    //        v = new List<object>();
                    //    }
                    //    size = v.Count;
                    //    for (i = 0; i < size; i++)
                    //    {
                    //        if ((i % 2) == 0)
                    //        {
                    //            ((MonthTS)v[i]).setDataType(lookupTimeSeriesDataType(COMP_RESERVOIR_TARGET_TS_MONTHLY) + "Min");
                    //        }
                    //        else
                    //        {
                    //            ((MonthTS)v[i]).setDataType(lookupTimeSeriesDataType(COMP_RESERVOIR_TARGET_TS_MONTHLY) + "Max");
                    //        }
                    //    }
                    //    comp.setData(v);
                    //}
                }
                catch (Exception e)
                {
                    Message.printWarning(1, routine, "Error reading reservoir target time series (monthly) file:\n\"" + fn + warningEndString + " (See log file for more on error:" + e + ")");
                    Message.printWarning(3, routine, e);
                    comp.setErrorReadingInputFile(true);
                }
                finally
                {
                    comp.setDirty(false);
                    readTime.stop();
                    readStateModFile_Announce2(comp, readTime.getSeconds());
                }

                // Reservoir return (.rrf)...

                try
                {
                    fn = response_props.getValue("Reservoir_Return");
                    // Always set the file name...
                    comp = getComponentForComponentType(COMP_RESERVOIR_RETURN);
                    if ((comp != null) && (!string.ReferenceEquals(fn, null)))
                    {
                        comp.setDataFileName(fn);
                    }
                    // Read the data...
                    //if (readData && (!string.ReferenceEquals(fn, null)) && !fileIsEmpty(getDataFilePathAbsolute(fn)))
                    //{
                    //    readTime.clear();
                    //    readTime.start();
                    //    fn = getDataFilePathAbsolute(fn);
                    //    readStateModFile_Announce1(comp);
                    //    comp.setData(StateMod_ReturnFlow.readStateModFile(fn, COMP_RESERVOIR_RETURN));
                    //}
                }
                catch (Exception e)
                {
                    Message.printWarning(1, routine, "Error reading reservoir return file:\n\"" + fn + warningEndString + " (See log file for more on error:" + e + ")");
                    Message.printWarning(3, routine, e);
                    comp.setErrorReadingInputFile(true);
                }
                finally
                {
                    comp.setDirty(false);
                    readTime.stop();
                    readStateModFile_Announce2(comp, readTime.getSeconds());
                }

                // TODO - San Juan Sediment Recovery

                try
                {
                    fn = response_props.getValue("SanJuanRecovery");
                    // Always set the file name...
                    comp = getComponentForComponentType(COMP_SANJUAN_RIP);
                    if ((comp != null) && (!string.ReferenceEquals(fn, null)))
                    {
                        comp.setDataFileName(fn);
                    }
                    // Read the data...
                    //readInputAnnounce1(comp);
                    //if (readData && (!string.ReferenceEquals(fn, null)) && hasSanJuanData(false) && !fileIsEmpty(getDataFilePathAbsolute(fn)))
                    //{
                    //    Message.printWarning(1, routine, "Do not know how to read the San Juan Recovery file.");
                    //}
                }
                catch (Exception e)
                {
                    Message.printWarning(1, routine, "Error reading San Juan Recovery file:\n\"" + fn + warningEndString + " (See log file for more on error:" + e + ")");
                    Message.printWarning(3, routine, e);
                    comp.setErrorReadingInputFile(true);
                }
                finally
                {
                    comp.setDirty(false);
                    readTime.stop();
                    //readInputAnnounce2(comp, readTime.getSeconds() );
                }

                // TODO SAM 2011-01-16 Enable - Rio Grande Spill

                try
                {
                    fn = response_props.getValue("RioGrande_Spill_Monthly");
                    // Always set the file name...
                    comp = getComponentForComponentType(COMP_RIO_GRANDE_SPILL);
                    if ((comp != null) && (!string.ReferenceEquals(fn, null)))
                    {
                        comp.setDataFileName(fn);
                    }
                    // Read the data...
                    //readInputAnnounce1(comp);
                    //if (readData && (!string.ReferenceEquals(fn, null)) && hasSanJuanData(false) && !fileIsEmpty(getDataFilePathAbsolute(fn)))
                    //{
                    //    Message.printWarning(1, routine, "Reading Rio Grande Spill file is not enabled.");
                    //}
                }
                catch (Exception e)
                {
                    Message.printWarning(1, routine, "Error reading Rio Grande Spill file:\n\"" + fn + warningEndString + " (See log file for more on error:" + e + ")");
                    Message.printWarning(3, routine, e);
                    comp.setErrorReadingInputFile(true);
                }
                finally
                {
                    comp.setDirty(false);
                    readTime.stop();
                    //readInputAnnounce2(comp, readTime.getSeconds() );
                }

                // Irrigation practice time series (tsp/ipy)...

                try
                {
                    fn = response_props.getValue("IrrigationPractice_Yearly");
                    // Set the file name in the component...
                    comp = getComponentForComponentType(COMP_IRRIGATION_PRACTICE_TS_YEARLY);
                    if ((comp != null) && (!string.ReferenceEquals(fn, null)))
                    {
                        comp.setDataFileName(fn);
                    }
                    // Now read the data...
                    //if (readData && __readTimeSeries && (!string.ReferenceEquals(fn, null)) && !fileIsEmpty(getDataFilePathAbsolute(fn)))
                    //{
                    //    readTime.clear();
                    //    readTime.start();
                    //    fn = getDataFilePathAbsolute(fn);
                    //    readStateModFile_Announce1(comp);
                    //    comp.setData(StateCU_IrrigationPracticeTS.readStateCUFile(fn, null, null));
                    //}
                }
                catch (Exception e)
                {
                    Message.printWarning(1, routine, "Error reading irrigation practice file:\n\"" + fn + warningEndString + " (See log file for more on error:" + e + ")");
                    Message.printWarning(3, routine, e);
                    comp.setErrorReadingInputFile(true);
                }
                finally
                {
                    comp.setDirty(false);
                    readTime.stop();
                    readStateModFile_Announce2(comp, readTime.getSeconds());
                }

                // Irrigation water requirement (iwr) - monthly...

                try
                {
                    fn = response_props.getValue("ConsumptiveWaterRequirement_Monthly");
                    // Set the file name in the component...
                    comp = getComponentForComponentType(COMP_CONSUMPTIVE_WATER_REQUIREMENT_TS_MONTHLY);
                    if ((comp != null) && (!string.ReferenceEquals(fn, null)))
                    {
                        comp.setDataFileName(fn);
                    }
                    // Now read the data...
                    //if (readData && __readTimeSeries && (!string.ReferenceEquals(fn, null)) && !fileIsEmpty(getDataFilePathAbsolute(fn)))
                    //{
                    //    readTime.clear();
                    //    readTime.start();
                    //    fn = getDataFilePathAbsolute(fn);
                    //    readStateModFile_Announce1(comp);
                    //    System.Collections.IList v = StateMod_TS.readTimeSeriesList(fn, null, null, null, true);
                    //    if (v == null)
                    //    {
                    //        v = new List<object>();
                    //    }
                    //    size = v.Count;
                    //    for (i = 0; i < size; i++)
                    //    {
                    //        ((MonthTS)v[i]).setDataType(lookupTimeSeriesDataType(COMP_CONSUMPTIVE_WATER_REQUIREMENT_TS_MONTHLY));
                    //    }
                    //    comp.setData(v);
                    //}
                }
                catch (Exception e)
                {
                    Message.printWarning(1, routine, "Error reading irrigation water requirement (monthly) time series " + "file:\n\"" + fn + warningEndString + " (See log file for more on error:" + e + ")");
                    Message.printWarning(3, routine, e);
                    comp.setErrorReadingInputFile(true);
                }
                finally
                {
                    comp.setDirty(false);
                    readTime.stop();
                    readStateModFile_Announce2(comp, readTime.getSeconds());
                }

                // StateMod used to read PAR but the AWC is now in the StateCU STR file.

                try
                {
                    fn = response_props.getValue("StateCU_Structure");
                    // Always set the file name...
                    comp = getComponentForComponentType(COMP_STATECU_STRUCTURE);
                    if ((comp != null) && (!string.ReferenceEquals(fn, null)))
                    {
                        comp.setDataFileName(fn);
                    }
                    // Read the data...
                    //if (readData && (!string.ReferenceEquals(fn, null)) && !fileIsEmpty(getDataFilePathAbsolute(fn)))
                    //{
                    //    readTime.clear();
                    //    readTime.start();
                    //    fn = getDataFilePathAbsolute(fn);
                    //    readStateModFile_Announce1(comp);
                    //    comp.setData(StateCU_Location.readStateCUFile(fn));
                    //    Message.printStatus(2, routine, "Read " + ((System.Collections.IList)comp.getData()).Count + " locations.");
                    //}
                }
                catch (Exception e)
                {
                    Message.printWarning(1, routine, "Error reading StateCU structure file:\n\"" + fn + warningEndString + " (See log file for more on error:" + e + ")");
                    Message.printWarning(3, routine, e);
                    comp.setErrorReadingInputFile(true);
                }
                finally
                {
                    comp.setDirty(false);
                    readTime.stop();
                    readStateModFile_Announce2(comp, readTime.getSeconds());
                }

                // Soil moisture (*.par) file no longer supported (print a warning)...

                fn = response_props.getValue("SoilMoisture");
                if ((!string.ReferenceEquals(fn, null)) && !fileIsEmpty(getDataFilePathAbsolute(fn)))
                {
                    Message.printWarning(2, routine, "StateCU soil moisture file - not supported - not reading \"" + fn + "\"");
                }

                // Reservoir content time series (monthly) file (.eom)...

                try
                {
                    fn = response_props.getValue("Reservoir_Historic_Monthly");
                    // Set the file name in the component...
                    comp = getComponentForComponentType(COMP_RESERVOIR_CONTENT_TS_MONTHLY);
                    if ((comp != null) && (!string.ReferenceEquals(fn, null)))
                    {
                        comp.setDataFileName(fn);
                    }
                    // Now read the data file...
                    //if (readData && __readTimeSeries && (!string.ReferenceEquals(fn, null)) && !fileIsEmpty(getDataFilePathAbsolute(fn)))
                    //{
                    //    readTime.clear();
                    //    readTime.start();
                    //    fn = getDataFilePathAbsolute(fn);
                    //    readStateModFile_Announce1(comp);
                    //    System.Collections.IList v = StateMod_TS.readTimeSeriesList(fn, null, null, null, true);
                    //    if (v == null)
                    //    {
                    //        v = new List<object>();
                    //    }
                    //    // Set the data type because it is not in the StateMod file...
                    //    size = v.Count;
                    //    for (i = 0; i < size; i++)
                    //    {
                    //        ((MonthTS)v[i]).setDataType(lookupTimeSeriesDataType(COMP_RESERVOIR_CONTENT_TS_MONTHLY));
                    //    }
                    //    comp.setData(v);
                    //}
                }
                catch (Exception e)
                {
                    Message.printWarning(1, routine, "Error reading reservoir end of month time series file:\n\"" + fn + warningEndString + " (See log file for more on error:" + e + ")");
                    Message.printWarning(3, routine, e);
                    comp.setErrorReadingInputFile(true);
                }
                finally
                {
                    comp.setDirty(false);
                    readTime.stop();
                    readStateModFile_Announce2(comp, readTime.getSeconds());
                }

                // Stream estimate coefficients file (.rib)...

                try
                {
                    fn = response_props.getValue("StreamEstimate_Coefficients");
                    // Always set the file name...
                    comp = getComponentForComponentType(COMP_STREAMESTIMATE_COEFFICIENTS);
                    if ((comp != null) && (!string.ReferenceEquals(fn, null)))
                    {
                        comp.setDataFileName(fn);
                    }
                    // Read the data...
                    //if (readData && (!string.ReferenceEquals(fn, null)) && !fileIsEmpty(getDataFilePathAbsolute(fn)))
                    //{
                    //    readTime.clear();
                    //    readTime.start();
                    //    fn = getDataFilePathAbsolute(fn);
                    //    readStateModFile_Announce1(comp);
                    //    comp.setData(StateMod_StreamEstimate_Coefficients.readStateModFile(fn));
                    //}
                }
                catch (Exception e)
                {
                    Message.printWarning(1, routine, "Error reading stream estimate coefficient file:\n\"" + fn + warningEndString + " (See log file for more on error:" + e + ")");
                    Message.printWarning(3, routine, e);
                    comp.setErrorReadingInputFile(true);
                }
                finally
                {
                    comp.setDirty(false);
                    readTime.stop();
                    readStateModFile_Announce2(comp, readTime.getSeconds());
                }

                // Historical streamflow (monthly) file (.rih)...

                try
                {
                    fn = response_props.getValue("StreamGage_Historic_Monthly");
                    // Always set the file name...
                    comp = getComponentForComponentType(COMP_STREAMGAGE_HISTORICAL_TS_MONTHLY);
                    if ((comp != null) && (!string.ReferenceEquals(fn, null)))
                    {
                        comp.setDataFileName(fn);
                    }
                    // Read the data...
                    if (readData && (!string.ReferenceEquals(fn, null)) && !fileIsEmpty(getDataFilePathAbsolute(fn)))
                    {
                        readTime.clear();
                        readTime.start();
                        fn = getDataFilePathAbsolute(fn);
                        readStateModFile_Announce1(comp);
                        IList<TS> v = StateMod_TS.readTimeSeriesList(fn, null, null, null, true);
                        if (v == null)
                        {
                            v = new List<TS>();
                        }
                        size = v.Count;
                        for (i = 0; i < size; i++)
                        {
                            // Set this information because it is not in the StateMod time series file...
                            ((MonthTS)v[i]).setDataType(lookupTimeSeriesDataType(COMP_STREAMGAGE_HISTORICAL_TS_MONTHLY));
                        }
                        comp.setData(v);
                        response_props.set("OutputFile", "../../../test/datasets/cdss-yampa/StateMod/StateMod_TS_Output.ddh");
                        StateMod_TS.writeTimeSeriesList(v, response_props);
                    }
                }
                catch (Exception e)
                {
                    Message.printWarning(1, routine, "Error reading historical streamflow time series file:\n\n\"" + fn + warningEndString + " (See log file for more on error:" + e + ")");
                    Message.printWarning(3, routine, e);
                    comp.setErrorReadingInputFile(true);
                }
                finally
                {
                    comp.setDirty(false);
                    readTime.stop();
                    readStateModFile_Announce2(comp, readTime.getSeconds());
                }

                // Diversion time series (historical monthly) file (.ddh)...

                try
                {
                    fn = response_props.getValue("Diversion_Historic_Monthly");
                    // Make sure the file name is set in the component...
                    comp = getComponentForComponentType(COMP_DIVERSION_TS_MONTHLY);
                    if ((comp != null) && (!string.ReferenceEquals(fn, null)))
                    {
                        comp.setDataFileName(fn);
                    }
                    // Now read the file if requested...
                    if (readData && __readTimeSeries && (!string.ReferenceEquals(fn, null)) && !fileIsEmpty(getDataFilePathAbsolute(fn)))
                    {
                        Message.printDebug(1, routine, "----------------------------------------------");
                        Message.printDebug(1, routine, "About to read .ddh StateModTS File...");
                        Message.printDebug(1, routine, "----------------------------------------------");
                        readTime.clear();
                        readTime.start();
                        readStateModFile_Announce1(comp);
                        fn = getDataFilePathAbsolute(fn);
                        IList<TS> v = StateMod_TS.readTimeSeriesList(fn, null, null, null, true);
                        if (v == null)
                        {
                            v = new List<TS>();
                        }
                        size = v.Count;
                        for (i = 0; i < size; i++)
                        {
                            ((MonthTS)v[i]).setDataType(lookupTimeSeriesDataType(COMP_DIVERSION_TS_MONTHLY));
                        }
                        comp.setData(v);
                    }
                }
                catch (Exception e)
                {
                    Message.printWarning(1, routine, "Error reading historical diversion time series (monthly) file:\n\"" + fn + warningEndString + " (See log file for more on error:" + e + ")");
                    Message.printWarning(3, routine, e);
                    comp.setErrorReadingInputFile(true);
                }
                finally
                {
                    comp.setDirty(false);
                    readTime.stop();
                    readStateModFile_Announce2(comp, readTime.getSeconds());
                }

                // Well historical pumping time series (monthly) file (.weh)..

                try
                {
                    fn = response_props.getValue("Well_Historic_Monthly");
                    // Set the file name in the component...
                    comp = getComponentForComponentType(COMP_WELL_PUMPING_TS_MONTHLY);
                    if ((comp != null) && (!string.ReferenceEquals(fn, null)))
                    {
                        comp.setDataFileName(fn);
                    }
                    // Now read the data file...
                    //if (readData && __readTimeSeries && (!string.ReferenceEquals(fn, null)) && !fileIsEmpty(getDataFilePathAbsolute(fn)))
                    //{
                    //    readTime.clear();
                    //    readTime.start();
                    //    fn = getDataFilePathAbsolute(fn);
                    //    readStateModFile_Announce1(comp);
                    //    System.Collections.IList v = StateMod_TS.readTimeSeriesList(fn, null, null, null, true);
                    //    if (v == null)
                    //    {
                    //        v = new List<object>();
                    //    }
                    //    size = v.Count;
                    //    for (i = 0; i < size; i++)
                    //    {
                    //        ((MonthTS)v[i]).setDataType(lookupTimeSeriesDataType(COMP_WELL_PUMPING_TS_MONTHLY));
                    //    }
                    //    comp.setData(v);
                    //}
                }
                catch (Exception e)
                {
                    Message.printWarning(1, routine, "Error reading well pumping time series (monthly) file:\n\"" + fn + warningEndString + " (See log file for more on error:" + e + ")");
                    Message.printWarning(3, routine, e);
                    comp.setErrorReadingInputFile(true);
                }
                finally
                {
                    comp.setDirty(false);
                    readTime.stop();
                    readStateModFile_Announce2(comp, readTime.getSeconds());
                }

                // GeoView project file...

                try
                {
                    fn = response_props.getValue("GeographicInformation");
                    // Always set the file name...
                    comp = getComponentForComponentType(COMP_GEOVIEW);
                    if ((comp != null) && (!string.ReferenceEquals(fn, null)))
                    {
                        comp.setDataFileName(fn);
                    }
                    if ((!string.ReferenceEquals(fn, null)) && !fileIsEmpty(getDataFilePathAbsolute(fn)))
                    {
                        readTime.clear();
                        readTime.start();
                        fn = getDataFilePathAbsolute(fn);
                        //readInputAnnounce1(comp);
                    }
                }
                catch (Exception e)
                {
                    // Print this at level 2 because the main GUI will warn if it
                    // cannot read the file.  We don't want 2 warnings.
                    Message.printWarning(2, routine, "Unable to read/process GeoView project file \"" + fn + warningEndString + " (See log file for more on error:" + e + ")");
                    Message.printWarning(3, routine, e);
                    comp.setErrorReadingInputFile(true);
                }
                finally
                {
                    comp.setDirty(false);
                    readTime.stop();
                    //readInputAnnounce2(comp, readTime.getSeconds() );
                    // Read data and display when the GUI is shown - no read for data to be read if no GUI
                }

                // TODO - output control - this is usually read separately when
                // running reports, etc.  Just read the line but do not read the file...

                try
                {
                    fn = response_props.getValue("OutputRequest");
                    // Always set the file name...
                    comp = getComponentForComponentType(COMP_OUTPUT_REQUEST);
                    if ((comp != null) && (!string.ReferenceEquals(fn, null)))
                    {
                        comp.setDataFileName(fn);
                    }
                    //readInputAnnounce1(comp, readTime.getSeconds() );
                }
                catch (Exception e)
                {
                    Message.printWarning(1, routine, "Error reading output control file:\n\"" + fn + warningEndString + " (See log file for more on error:" + e + ")");
                    Message.printWarning(3, routine, e);
                    comp.setErrorReadingInputFile(true);
                }
                finally
                {
                    comp.setDirty(false);
                    readTime.stop();
                    //readInputAnnounce2(comp, readTime.getSeconds() );
                    // Read data and display when the GUI is shown - no read for data to be read if no GUI
                }

                // TODO SAM 2011-01-16 Eanble reach data file

                try
                {
                    fn = response_props.getValue("Reach_Data");
                    // Always set the file name...
                    comp = getComponentForComponentType(COMP_REACH_DATA);
                    if ((comp != null) && (!string.ReferenceEquals(fn, null)))
                    {
                        comp.setDataFileName(fn);
                    }
                    // Read the data...
                    //readInputAnnounce1(comp, readTime.getSeconds());
                    //if (readData && (!string.ReferenceEquals(fn, null)) && !fileIsEmpty(getDataFilePathAbsolute(fn)))
                    //{
                    //    Message.printWarning(2, routine, "Reach data file - not yet supported.");
                    //}
                }
                catch (Exception e)
                {
                    Message.printWarning(1, routine, "Error reading reach data file:\n\"" + fn + warningEndString + " (See log file for more on error:" + e + ")");
                    Message.printWarning(3, routine, e);
                    comp.setErrorReadingInputFile(true);
                }
                finally
                {
                    comp.setDirty(false);
                    readTime.stop();
                    //readInputAnnounce2(comp, readTime.getSeconds());
                }

                // Stream natural flow flow time series (daily) file (.rid)...
                // Always read if a daily data set.

                try
                {
                    fn = response_props.getValue("Stream_Base_Daily");
                    // Always set the file name...
                    comp = getComponentForComponentType(COMP_STREAMGAGE_NATURAL_FLOW_TS_DAILY);
                    if ((comp != null) && (!string.ReferenceEquals(fn, null)))
                    {
                        comp.setDataFileName(fn);
                    }
                    // Read the data...
                    //if (readData && (!string.ReferenceEquals(fn, null)) && !fileIsEmpty(getDataFilePathAbsolute(fn)))
                    //{
                    //    readTime.clear();
                    //    readTime.start();
                    //    fn = getDataFilePathAbsolute(fn);
                    //    readStateModFile_Announce1(comp);
                    //    System.Collections.IList v = StateMod_TS.readTimeSeriesList(fn, null, null, null, true);
                    //    if (v == null)
                    //    {
                    //        v = new List<object>();
                    //    }
                    //    size = v.Count;
                    //    for (i = 0; i < size; i++)
                    //    {
                    //        ((DayTS)v[i]).setDataType(lookupTimeSeriesDataType(COMP_STREAMGAGE_NATURAL_FLOW_TS_DAILY));
                    //    }
                    //    comp.setData(v);

                    //    // The StreamGage and StreamEstimate groups share the same natural flow time series files...

                    //    comp2 = getComponentForComponentType(COMP_STREAMESTIMATE_NATURAL_FLOW_TS_DAILY);
                    //    comp2.setDataFileName(comp.getDataFileName());
                    //    comp2.setData(v);
                    //}
                }
                catch (Exception e)
                {
                    Message.printWarning(1, routine, "Error reading daily natural flow time series file:\n\"" + fn + warningEndString + " (See log file for more on error:" + e + ")");
                    Message.printWarning(3, routine, e);
                    comp.setErrorReadingInputFile(true);
                }
                finally
                {
                    comp.setDirty(false);
                    if (comp2 != null)
                    {
                        // Never read data above so no need to call the following
                        comp2.setDirty(false);
                    }
                    readTime.stop();
                    readStateModFile_Announce2(comp, readTime.getSeconds());
                }

                // Direct diversion demand time series (daily) file (.ddd)...

                try
                {
                    fn = response_props.getValue("Diversion_Demand_Daily");
                    // Set the file name in the component...
                    comp = getComponentForComponentType(COMP_DEMAND_TS_DAILY);
                    if ((comp != null) && (!string.ReferenceEquals(fn, null)))
                    {
                        comp.setDataFileName(fn);
                    }
                    // Now read the data file...
                    //if (readData && __readTimeSeries && (!string.ReferenceEquals(fn, null)) && !fileIsEmpty(getDataFilePathAbsolute(fn)))
                    //{
                    //    readTime.clear();
                    //    readTime.start();
                    //    fn = getDataFilePathAbsolute(fn);
                    //    readStateModFile_Announce1(comp);
                    //    System.Collections.IList v = StateMod_TS.readTimeSeriesList(fn, null, null, null, true);
                    //    if (v == null)
                    //    {
                    //        v = new List<object>();
                    //    }
                    //    size = v.Count;
                    //    for (i = 0; i < size; i++)
                    //    {
                    //        ((DayTS)v[i]).setDataType(lookupTimeSeriesDataType(COMP_DEMAND_TS_DAILY));
                    //    }
                    //    comp.setData(v);
                    //}
                }
                catch (Exception e)
                {
                    Message.printWarning(1, routine, "Error reading daily demand time series file:\n\"" + fn + warningEndString + " (See log file for more on error:" + e + ")");
                    Message.printWarning(3, routine, e);
                    comp.setErrorReadingInputFile(true);
                }
                finally
                {
                    comp.setDirty(false);
                    readTime.stop();
                    readStateModFile_Announce2(comp, readTime.getSeconds());
                }

                // Instream flow demand time series (daily) file (.ifd)...

                try
                {
                    fn = response_props.getValue("Instreamflow_Demand_Daily");
                    // Set the file name in the component...
                    comp = getComponentForComponentType(COMP_INSTREAM_DEMAND_TS_DAILY);
                    if ((comp != null) && (!string.ReferenceEquals(fn, null)))
                    {
                        comp.setDataFileName(fn);
                    }
                    // Now read the data file...
                    //if (readData && __readTimeSeries && (!string.ReferenceEquals(fn, null)) && !fileIsEmpty(getDataFilePathAbsolute(fn)))
                    //{
                    //    readTime.clear();
                    //    readTime.start();
                    //    fn = getDataFilePathAbsolute(fn);
                    //    readStateModFile_Announce1(comp);
                    //    System.Collections.IList v = StateMod_TS.readTimeSeriesList(fn, null, null, null, true);
                    //    if (v == null)
                    //    {
                    //        v = new List<object>();
                    //    }
                    //    size = v.Count;
                    //    for (i = 0; i < size; i++)
                    //    {
                    //        ((DayTS)v[i]).setDataType(lookupTimeSeriesDataType(COMP_INSTREAM_DEMAND_TS_DAILY));
                    //    }
                    //    comp.setData(v);
                    //}
                }
                catch (Exception e)
                {
                    Message.printWarning(1, routine, "Error reading daily instream flow demand time series" + " file:\n\"" + fn + warningEndString + " (See log file for more on error:" + e + ")");
                    Message.printWarning(3, routine, e);
                    comp.setErrorReadingInputFile(true);
                }
                finally
                {
                    comp.setDirty(false);
                    readTime.stop();
                    readStateModFile_Announce2(comp, readTime.getSeconds());
                }

                // Well demand time series (daily) file (.wed)...

                try
                {
                    fn = response_props.getValue("Well_Demand_Daily");
                    // Set the file name in the component...
                    comp = getComponentForComponentType(COMP_WELL_DEMAND_TS_DAILY);
                    if ((comp != null) && (!string.ReferenceEquals(fn, null)))
                    {
                        comp.setDataFileName(fn);
                    }
                    // Now read the data file...
                    //if (readData && __readTimeSeries && (!string.ReferenceEquals(fn, null)) && !fileIsEmpty(getDataFilePathAbsolute(fn)))
                    //{
                    //    readTime.clear();
                    //    readTime.start();
                    //    fn = getDataFilePathAbsolute(fn);
                    //    readStateModFile_Announce1(comp);
                    //    System.Collections.IList v = StateMod_TS.readTimeSeriesList(fn, null, null, null, true);
                    //    if (v == null)
                    //    {
                    //        v = new List<object>();
                    //    }
                    //    size = v.Count;
                    //    for (i = 0; i < size; i++)
                    //    {
                    //        ((DayTS)v[i]).setDataType(lookupTimeSeriesDataType(COMP_WELL_DEMAND_TS_DAILY));
                    //    }
                    //    comp.setData(v);
                    //}
                }
                catch (Exception e)
                {
                    Message.printWarning(1, routine, "Error reading daily well demand time series file:\n\"" + fn + warningEndString + " (See log file for more on error:" + e + ")");
                    Message.printWarning(3, routine, e);
                    comp.setErrorReadingInputFile(true);
                }
                finally
                {
                    comp.setDirty(false);
                    readTime.stop();
                    readStateModFile_Announce2(comp, readTime.getSeconds());
                }

                // Reservoir target time series (daily) file (.tad)...

                try
                {
                    fn = response_props.getValue("Reservoir_Target_Daily");
                    // Set the file name in the component...
                    comp = getComponentForComponentType(COMP_RESERVOIR_TARGET_TS_DAILY);
                    if ((comp != null) && (!string.ReferenceEquals(fn, null)))
                    {
                        comp.setDataFileName(fn);
                    }
                    // Now read the data file...
                    //if (readData && __readTimeSeries && (!string.ReferenceEquals(fn, null)) && !fileIsEmpty(getDataFilePathAbsolute(fn)))
                    //{
                    //    readTime.clear();
                    //    readTime.start();
                    //    fn = getDataFilePathAbsolute(fn);
                    //    readStateModFile_Announce1(comp);
                    //    System.Collections.IList v = StateMod_TS.readTimeSeriesList(fn, null, null, null, true);
                    //    if (v == null)
                    //    {
                    //        v = new List<object>();
                    //    }
                    //    size = v.Count;
                    //    for (i = 0; i < size; i++)
                    //    {
                    //        if ((i % 2) == 0)
                    //        {
                    //            ((DayTS)v[i]).setDataType(lookupTimeSeriesDataType(COMP_RESERVOIR_TARGET_TS_DAILY) + "Min");
                    //        }
                    //        else
                    //        {
                    //            ((DayTS)v[i]).setDataType(lookupTimeSeriesDataType(COMP_RESERVOIR_TARGET_TS_DAILY) + "Max");
                    //        }
                    //    }
                    //    comp.setData(v);
                    //}
                }
                catch (Exception e)
                {
                    Message.printWarning(1, routine, "Error reading daily reservoir target time series file:\n\"" + fn + warningEndString + " (See log file for more on error:" + e + ")");
                    Message.printWarning(3, routine, e);
                    comp.setErrorReadingInputFile(true);
                }
                finally
                {
                    comp.setDirty(false);
                    readTime.stop();
                    readStateModFile_Announce2(comp, readTime.getSeconds());
                }

                // Delay table (daily)...

                try
                {
                    fn = response_props.getValue("DelayTable_Daily");
                    // Always set the file name...
                    comp = getComponentForComponentType(COMP_DELAY_TABLES_DAILY);
                    if ((comp != null) && (!string.ReferenceEquals(fn, null)))
                    {
                        comp.setDataFileName(fn);
                    }
                    // Read the data...
                    //if (readData && (!string.ReferenceEquals(fn, null)) && !fileIsEmpty(getDataFilePathAbsolute(fn)))
                    //{
                    //    readTime.clear();
                    //    readTime.start();
                    //    fn = getDataFilePathAbsolute(fn);
                    //    readStateModFile_Announce1(comp);
                    //    comp.setData(StateMod_DelayTable.readStateModFile(fn, false, getInterv()));
                    //}
                }
                catch (Exception e)
                {
                    Message.printWarning(1, routine, "Error reading delay table (daily) file:\n\"" + fn + warningEndString + " (See log file for more on error:" + e + ")");
                    Message.printWarning(3, routine, e);
                    comp.setErrorReadingInputFile(true);
                }
                finally
                {
                    comp.setDirty(false);
                    readTime.stop();
                    readStateModFile_Announce2(comp, readTime.getSeconds());
                }

                // Irrigation water requirement (iwr) - daily...

                try
                {
                    fn = response_props.getValue("ConsumptiveWaterRequirement_Daily");
                    // Set the file name in the component...
                    comp = getComponentForComponentType(COMP_CONSUMPTIVE_WATER_REQUIREMENT_TS_DAILY);
                    if ((comp != null) && (!string.ReferenceEquals(fn, null)))
                    {
                        comp.setDataFileName(fn);
                    }
                    // Now read the data file...
                    //if (readData && __readTimeSeries && (!string.ReferenceEquals(fn, null)) && !fileIsEmpty(getDataFilePathAbsolute(fn)))
                    //{
                    //    readTime.clear();
                    //    readTime.start();
                    //    fn = getDataFilePathAbsolute(fn);
                    //    readStateModFile_Announce1(comp);
                    //    System.Collections.IList v = StateMod_TS.readTimeSeriesList(fn, null, null, null, true);
                    //    if (v == null)
                    //    {
                    //        v = new List<object>();
                    //    }
                    //    size = v.Count;
                    //    for (i = 0; i < size; i++)
                    //    {
                    //        ((DayTS)v[i]).setDataType(lookupTimeSeriesDataType(COMP_CONSUMPTIVE_WATER_REQUIREMENT_TS_DAILY));
                    //    }
                    //    comp.setData(v);
                    //}
                }
                catch (Exception e)
                {
                    Message.printWarning(1, routine, "Error reading irrigation water requirement (daily) time series " + "file:\n\"" + fn + warningEndString + " (See log file for more on error:" + e + ")");
                    Message.printWarning(3, routine, e);
                    comp.setErrorReadingInputFile(true);
                }
                finally
                {
                    comp.setDirty(false);
                    readTime.stop();
                    readStateModFile_Announce2(comp, readTime.getSeconds());
                }

                // Streamflow historical time series (daily) file (.riy) - always read...

                try
                {
                    fn = response_props.getValue("StreamGage_Historic_Daily");
                    // Always set the file name...
                    comp = getComponentForComponentType(COMP_STREAMGAGE_HISTORICAL_TS_DAILY);
                    if ((comp != null) && (!string.ReferenceEquals(fn, null)))
                    {
                        comp.setDataFileName(fn);
                    }
                    // Read the data...
                    //if (readData && __readTimeSeries && (!string.ReferenceEquals(fn, null)) && !fileIsEmpty(getDataFilePathAbsolute(fn)))
                    //{
                    //    readTime.clear();
                    //    readTime.start();
                    //    fn = getDataFilePathAbsolute(fn);
                    //    readStateModFile_Announce1(comp);
                    //    System.Collections.IList v = StateMod_TS.readTimeSeriesList(fn, null, null, null, true);
                    //    if (v == null)
                    //    {
                    //        v = new List<object>();
                    //    }
                    //    size = v.Count;
                    //    for (i = 0; i < size; i++)
                    //    {
                    //        // Set this information because it is not in the StateMod time series file...
                    //        ((DayTS)v[i]).setDataType(lookupTimeSeriesDataType(COMP_STREAMGAGE_HISTORICAL_TS_DAILY));
                    //    }
                    //    comp.setData(v);
                    //}
                }
                catch (Exception e)
                {
                    Message.printWarning(1, routine, "Error reading daily historical streamflow time series file:\n\"" + fn + warningEndString + " (See log file for more on error:" + e + ")");
                    Message.printWarning(3, routine, e);
                    comp.setErrorReadingInputFile(true);
                }
                finally
                {
                    comp.setDirty(false);
                    readTime.stop();
                    readStateModFile_Announce2(comp, readTime.getSeconds());
                }

                // Diversion (daily) time series (.ddd)...

                try
                {
                    fn = response_props.getValue("Diversion_Historic_Daily");
                    // Set the file name in the component...
                    comp = getComponentForComponentType(COMP_DIVERSION_TS_DAILY);
                    if ((comp != null) && (!string.ReferenceEquals(fn, null)))
                    {
                        comp.setDataFileName(fn);
                    }
                    // Now read the data file...
                    //if (readData && __readTimeSeries && (!string.ReferenceEquals(fn, null)) && !fileIsEmpty(getDataFilePathAbsolute(fn)))
                    //{
                    //    readTime.clear();
                    //    readTime.start();
                    //    fn = getDataFilePathAbsolute(fn);
                    //    readStateModFile_Announce1(comp);
                    //    System.Collections.IList v = StateMod_TS.readTimeSeriesList(fn, null, null, null, true);
                    //    if (v == null)
                    //    {
                    //        v = new List<object>();
                    //    }
                    //    // Set the data type because it is not in the StateMod file...
                    //    size = v.Count;
                    //    for (i = 0; i < size; i++)
                    //    {
                    //        ((DayTS)v[i]).setDataType(lookupTimeSeriesDataType(COMP_DIVERSION_TS_DAILY));
                    //    }
                    //    comp.setData(v);
                    //}
                }
                catch (Exception e)
                {
                    Message.printWarning(1, routine, "Error reading diversion (daily) time series file:\n\"" + fn + warningEndString + " (See log file for more on error:" + e + ")");
                    Message.printWarning(3, routine, e);
                    comp.setErrorReadingInputFile(true);
                }
                finally
                {
                    comp.setDirty(false);
                    readTime.stop();
                    readStateModFile_Announce2(comp, readTime.getSeconds());
                }

                // Well pumping (daily) time series...

                try
                {
                    fn = response_props.getValue("Well_Historic_Daily");
                    // Set the file name in the component...
                    comp = getComponentForComponentType(COMP_WELL_PUMPING_TS_DAILY);
                    if ((comp != null) && (!string.ReferenceEquals(fn, null)))
                    {
                        comp.setDataFileName(fn);
                    }
                    // Now read the data file...
                    //if (readData && __readTimeSeries && (!string.ReferenceEquals(fn, null)) && !fileIsEmpty(getDataFilePathAbsolute(fn)))
                    //{
                    //    readTime.clear();
                    //    readTime.start();
                    //    fn = getDataFilePathAbsolute(fn);
                    //    readStateModFile_Announce1(comp);
                    //    System.Collections.IList v = StateMod_TS.readTimeSeriesList(fn, null, null, null, true);
                    //    if (v == null)
                    //    {
                    //        v = new List<object>();
                    //    }
                    //    // Set the data type because it is not in the StateMod file...
                    //    size = v.Count;
                    //    for (i = 0; i < size; i++)
                    //    {
                    //        ((MonthTS)v[i]).setDataType(lookupTimeSeriesDataType(COMP_WELL_PUMPING_TS_DAILY));
                    //    }
                    //    comp.setData(v);
                    //}
                }
                catch (Exception e)
                {
                    Message.printWarning(1, routine, "Error reading well pumping (daily) time series file:\n\"" + fn + warningEndString + " (See log file for more on error:" + e + ")");
                    Message.printWarning(3, routine, e);
                    comp.setErrorReadingInputFile(true);
                }
                finally
                {
                    comp.setDirty(false);
                    readTime.stop();
                    readStateModFile_Announce2(comp, readTime.getSeconds());
                }

                // Daily reservoir content "eoy"...

                try
                {
                    fn = response_props.getValue("Reservoir_Historic_Daily");
                    // Set the file name in the component...
                    comp = getComponentForComponentType(COMP_RESERVOIR_CONTENT_TS_DAILY);
                    if ((comp != null) && (!string.ReferenceEquals(fn, null)))
                    {
                        comp.setDataFileName(fn);
                    }
                    // Read the data file...
                    //if (readData && __readTimeSeries && (!string.ReferenceEquals(fn, null)) && !fileIsEmpty(getDataFilePathAbsolute(fn)))
                    //{
                    //    readTime.clear();
                    //    readTime.start();
                    //    fn = getDataFilePathAbsolute(fn);
                    //    readStateModFile_Announce1(comp);
                    //    System.Collections.IList v = StateMod_TS.readTimeSeriesList(fn, null, null, null, true);
                    //    if (v == null)
                    //    {
                    //        v = new List<object>();
                    //    }
                    //    // Set the data type because it is not in the StateMod file...
                    //    size = v.Count;
                    //    for (i = 0; i < size; i++)
                    //    {
                    //        ((MonthTS)v[i]).setDataType(lookupTimeSeriesDataType(COMP_RESERVOIR_CONTENT_TS_DAILY));
                    //    }
                    //    comp.setData(v);
                    //}
                }
                catch (Exception e)
                {
                    Message.printWarning(1, routine, "Error reading reservoir end of day time series file:\n\"" + fn + warningEndString + " (See log file for more on error:" + e + ")");
                    Message.printWarning(3, routine, e);
                    comp.setErrorReadingInputFile(true);
                }
                finally
                {
                    comp.setDirty(false);
                    readTime.stop();
                    readStateModFile_Announce2(comp, readTime.getSeconds());
                }

                // Downstream call (.cal)...

                try
                {
                    fn = response_props.getValue("Downstream_Call");
                    // Always set the file name...
                    comp = getComponentForComponentType(COMP_DOWNSTREAM_CALL_TS_DAILY);
                    if ((comp != null) && (!string.ReferenceEquals(fn, null)))
                    {
                        comp.setDataFileName(fn);
                    }
                    // Read the data...
                    //if (readData && (!string.ReferenceEquals(fn, null)) && !fileIsEmpty(getDataFilePathAbsolute(fn)))
                    //{
                    //    readTime.clear();
                    //    readTime.start();
                    //    fn = getDataFilePathAbsolute(fn);
                    //    readStateModFile_Announce1(comp);
                    //    comp.setData(StateMod_DownstreamCall.readStateModFile(fn));
                    //}
                }
                catch (Exception e)
                {
                    Message.printWarning(1, routine, "Error reading downstream call file:\n\"" + fn + warningEndString + " (See log file for more on error:" + e + ")");
                    Message.printWarning(3, routine, e);
                    comp.setErrorReadingInputFile(true);
                }
                finally
                {
                    comp.setDirty(false);
                    readTime.stop();
                    readStateModFile_Announce2(comp, readTime.getSeconds());
                }

                // Keep track of files/properties that are not explicitly handled in this class
                // These may be new files added to the model, old files being phased out, or simple properties.
                PropList unhandledResponseFileProperties = getUnhandledResponseFileProperties();
                unhandledResponseFileProperties.clear();
                for (int iProp = 0; iProp < response_props.size(); iProp++)
                {
                    Prop prop = response_props.elementAt(iProp);
                    string fileKey = prop.getKey();
                    // See if the key is matched in the known StateMod file type keys...
                    bool found = false;
                    for (int iFile = 0; iFile < __statemod_file_properties.Length; iFile++)
                    {
                        if (fileKey.Equals(__statemod_file_properties[iFile], StringComparison.OrdinalIgnoreCase))
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        // The file property was not found so need to carry around as unknown
                        Message.printStatus(2, routine, "Unhandled response file property:  " + prop.getKey() + " = " + prop.getValue());
                        unhandledResponseFileProperties.set(prop);
                    }
                }

                // Print information about all the data (for troubleshooting)...
                Message.printStatus(2, routine, "\n" + toStringDefinitions());

                // After reading, link objects using identifiers in the various files...

                // Connect all the instream flow time series to the stations...

                Message.printStatus(1, routine, "Connect all instream flow time series");
                //StateMod_InstreamFlow.connectAllTS(
                //    (List)getComponentForComponentType(COMP_INSTREAM_STATIONS).getData(),
                //    (List)getComponentForComponentType(COMP_INSTREAM_DEMAND_TS_MONTHLY).getData(),
                //    (List)getComponentForComponentType(COMP_INSTREAM_DEMAND_TS_AVERAGE_MONTHLY).getData(),
                //    (List)getComponentForComponentType(COMP_INSTREAM_DEMAND_TS_DAILY).getData());

                // Connect all the reservoir time series to the stations...

                //StateMod_Reservoir.connectAllTS(
                //    (List)getComponentForComponentType(COMP_RESERVOIR_STATIONS).getData(),
                //    (List)getComponentForComponentType(COMP_RESERVOIR_CONTENT_TS_MONTHLY).getData(),
                //    (List)getComponentForComponentType(COMP_RESERVOIR_CONTENT_TS_DAILY).getData(),
                //    (List)getComponentForComponentType(COMP_RESERVOIR_TARGET_TS_MONTHLY).getData(),
                //    (List)getComponentForComponentType(COMP_RESERVOIR_TARGET_TS_DAILY).getData());

                // Connect all the diversion time series to the stations...

                Message.printStatus(1, routine, "Connect all diversion time series");
                //StateMod_Diversion.connectAllTS(
                //    (List)getComponentForComponentType(COMP_DIVERSION_STATIONS).getData(),
                //    (List)getComponentForComponentType(COMP_DIVERSION_TS_MONTHLY).getData(),
                //    (List)getComponentForComponentType(COMP_DIVERSION_TS_DAILY).getData(),
                //    (List)getComponentForComponentType(COMP_DEMAND_TS_MONTHLY).getData(),
                //    (List)getComponentForComponentType(COMP_DEMAND_TS_OVERRIDE_MONTHLY).getData(),
                //    (List)getComponentForComponentType(COMP_DEMAND_TS_AVERAGE_MONTHLY).getData(),
                //    (List)getComponentForComponentType(COMP_DEMAND_TS_DAILY).getData(),
                //    (List)getComponentForComponentType(COMP_IRRIGATION_PRACTICE_TS_YEARLY).getData(),
                //    (List)getComponentForComponentType(COMP_CONSUMPTIVE_WATER_REQUIREMENT_TS_MONTHLY).getData(),
                //    (List)getComponentForComponentType(COMP_CONSUMPTIVE_WATER_REQUIREMENT_TS_DAILY).getData());

                // Connect all the well time series to the stations...

                Message.printStatus(1, routine, "Connect all well time series");
                //StateMod_Well.connectAllTS(
                //    (List)getComponentForComponentType(COMP_WELL_STATIONS).getData(),
                //    (List)getComponentForComponentType(COMP_WELL_PUMPING_TS_MONTHLY).getData(),
                //    (List)getComponentForComponentType(COMP_WELL_PUMPING_TS_DAILY).getData(),
                //    (List)getComponentForComponentType(COMP_WELL_DEMAND_TS_MONTHLY).getData(),
                //    (List)getComponentForComponentType(COMP_WELL_DEMAND_TS_DAILY).getData(),
                //    (List)getComponentForComponentType(COMP_IRRIGATION_PRACTICE_TS_YEARLY).getData(),
                //    (List)getComponentForComponentType(COMP_CONSUMPTIVE_WATER_REQUIREMENT_TS_MONTHLY).getData(),
                //    (List)getComponentForComponentType(COMP_CONSUMPTIVE_WATER_REQUIREMENT_TS_DAILY).getData());

                // Process the old-style ris, rim, rid files for the new convention...

                // TODO SAM 2009-06-30 Evaluate if the following is being handled ok free format
                /*
		        if ( !__is_free_format ) {
			        StateMod_StreamEstimate.processStreamData ( 
			        (List)getComponentForComponentType( COMP_STREAMGAGE_STATIONS).getData(),
			        (List)getComponentForComponentType( COMP_STREAMGAGE_HISTORICAL_TS_MONTHLY).getData(),
			        (List)getComponentForComponentType( COMP_STREAMESTIMATE_STATIONS).getData(),
			        (List)getComponentForComponentType( COMP_STREAMESTIMATE_COEFFICIENTS).getData() );
		        }	// Else the StreamGage and StreamEstimate stations are already split into separate files.
		        */

                // Connect all the stream gage station time series to the stations...

                Message.printStatus(1, routine, "Connect all river station time series");
                //StateMod_StreamGage.connectAllTS(
                //    (List)getComponentForComponentType(COMP_STREAMGAGE_STATIONS).getData(),
                //    (List)getComponentForComponentType(COMP_STREAMGAGE_HISTORICAL_TS_MONTHLY).getData(),
                //    (List)getComponentForComponentType(COMP_STREAMGAGE_HISTORICAL_TS_DAILY).getData(),
                //    (List)getComponentForComponentType(COMP_STREAMGAGE_NATURAL_FLOW_TS_MONTHLY).getData(),
                //    (List)getComponentForComponentType(COMP_STREAMGAGE_NATURAL_FLOW_TS_DAILY).getData());

                // Connect all the stream estimate station time series to the stations...

                Message.printStatus(1, routine, "Connect all stream estimate station time series");
                //StateMod_StreamEstimate.connectAllTS(
                //    (List)getComponentForComponentType(COMP_STREAMESTIMATE_STATIONS).getData(),
                //    (List)getComponentForComponentType(COMP_STREAMESTIMATE_NATURAL_FLOW_TS_MONTHLY).getData(),
                //    (List)getComponentForComponentType(COMP_STREAMESTIMATE_NATURAL_FLOW_TS_DAILY).getData());

                totalReadTime.stop();
                Message.printStatus(1, routine, "Total time to read StateMod files is " + String.Format(totalReadTime.getSeconds().ToString(), "%.3f") + " seconds");
                totalReadTime.start();

                // Read the generalized network...	
                Message.printStatus(1, routine, "Reading generalized network.");

                try
                {
                    fn = response_props.getValue("Network");
                    // Always set the file name...
                    comp = getComponentForComponentType(COMP_NETWORK);
                    if ((comp != null) && (!string.ReferenceEquals(fn, null)))
                    {
                        comp.setDataFileName(fn);
                    }
                    // Read the data...
                    //if ((!string.ReferenceEquals(fn, null)) && !fileIsEmpty(getDataFilePathAbsolute(fn)))
                    //{
                    //    readTime.clear();
                    //    readTime.start();
                    //    fn = getDataFilePathAbsolute(fn);
                    //    readStateModFile_Announce1(comp);
                    //    StateMod_NodeNetwork network = (StateMod_NodeNetwork)StateMod_NodeNetwork.readStateModNetworkFile(fn, null, true);
                    //    comp.setData(network);
                    //    comp.setVisible(true);
                    //}
                }
                catch (Exception e)
                {
                    Message.printWarning(1, routine, "Unexpected error reading network file:\n\"" + fn + warningEndString + " (See log file for more on error:" + e + ")");
                    Message.printWarning(3, routine, e);
                    comp.setErrorReadingInputFile(true);
                }
                finally
                {
                    comp.setDirty(false);
                    readTime.stop();
                    readStateModFile_Announce2(comp, readTime.getSeconds());
                }

            }
            catch (Exception e)
            {
                // Main catch for all reads.
                string message = "Unexpected error during read (" + e + ") - please contact support.";
                Message.printStatus(1, routine, message);
                Message.printWarning(3, routine, e);
                // TODO Just rethrow for now
                throw new IOException(message);
            }

            // Check the filenames for 8.3.  This limitation may be removed at some point...
            // TODO - may move this to the front again, but only if the response
            // file is read up front for the check and then reading continues as per the above logic.

            /* TODO 
                if (	!checkComponentFilenames(files_from_response, 1) &&
                    useGUI && (parent != null) ) {
                    Message.printWarning ( 1, routine, "StateMod may not run." );
                }
            */

            // Set component visibility based on the the control information...

            checkComponentVisibility();

            totalReadTime.stop();
            string msg = "Total time to read all files is "
                + String.Format(totalReadTime.getSeconds().ToString(), "%.3f") + " seconds";
            Message.printStatus(1, routine, msg);
            sendProcessListenerMessage(StateMod_GUIUtil.STATUS_READ_COMPLETE, msg);
            setDirty(COMP_CONTROL, false);

            readTime = null;
            totalReadTime = null;
            // TODO - uncomment for debugging
            //Message.printStatus ( 2, routine,
            //		"SAMX - After reading all files, control dirty =" +
            //		getComponentForComponentType(
            //		COMP_CONTROL).isDirty() );
            //Message.printStatus ( 2, routine, super.toString () );
        }

        /// <summary>
        /// Send a message to ProcessListener that have been registered with this object.
        /// This is usually a main application that is giving feedback to a user via the messages.
        /// </summary>
        public virtual void sendProcessListenerMessage(int status, string message)
        {
            int size = 0;
            if (__processListeners != null)
            {
                size = __processListeners.Count;
            }

            ProcessListener p = null;
            for (int i = 0; i < size; i++)
            {
                p = __processListeners[i];
                p.processStatus(status, message);
            }
        }

        /// <summary>
        /// Set number of precipitation stations. </summary>
        /// <param name="numpre"> number of stations </param>
        public virtual void setNumpre(int numpre)
        {
            if (numpre != __numpre)
            {
                __numpre = numpre;
                setDirty(COMP_CONTROL, true);
            }
        }

        /// <summary>
        /// Set number of precipitation stations. </summary>
        /// <param name="numpre"> number of stations </param>
        public virtual void setNumpre(int? numpre)
        {
            setNumpre(numpre.Value);
        }

        /// <summary>
        /// Set number of precipitation stations. </summary>
        /// <param name="numpre"> number of stations </param>
        public virtual void setNumpre(string numpre)
        {
            if (!string.ReferenceEquals(numpre, null))
            {
                setNumpre(int.Parse(numpre.Trim()));
            }
        }

        /// <summary>
        /// Return a string representation of the data set definition information, useful for troubleshooting.
        /// </summary>
        private string toStringDefinitions()
        {
            StringBuilder b = new StringBuilder();
            DataSetComponent comp = null;
            //string nl = System.getProperty("line.separator");
            for (int i = 0; i < __component_names.Length; i++)
            {
                comp = getComponentForComponentType(i);
                b.Append("[" + i + "] Name=\"" + __component_names[i] + "\" Group=" + __component_group_assignments[i] + " RspProperty=\"" + __statemod_file_properties[i] + "\" Filename=\"" + comp.getDataFileName() + "\" Ext=\"" + __component_file_extensions[i] + "\" TSType=\"" + __component_ts_data_types[i] + "\" TSInt=" + __component_ts_data_intervals[i] + " TSUnits=\"" + __component_ts_data_units[i] + "\"" + "\n");
            }
            return b.ToString();
        }

        /// <summary>
        /// This method is a helper routine to readStateModFile().  It calls
        /// Message.printStatus() with the message that a particular file is being read,
        /// including path.  Then it prints a similar, but shorter,
        /// message to the status bar.  If there is an error with the file (not specified,
        /// does not exist, etc.), then an Exception is thrown.  There are many StateMod
        /// files and therefore the same basic checks are done many times. </summary>
        /// <param name="comp"> Data set component that is being read. </param>
        /// <exception cref="if"> there is a basic error with the file not being found, etc. </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: private void readStateModFile_Announce1(RTi.Util.IO.DataSetComponent comp) throws Exception
        private void readStateModFile_Announce1(DataSetComponent comp)
        {
            string fn = getDataFilePathAbsolute(comp);
            string description = comp.getComponentName();

            if ((string.ReferenceEquals(fn, null)) || (fn.Length == 0))
            {
                throw new Exception(description + " file name unavailable.");
            }
            // TODO - need to know whether this is an error that the user should acknowlege...
            if (!IOUtil.fileExists(fn))
            {
                throw new Exception(description + " file \"" + fn + "\" does not exist.");
            }
            if (!IOUtil.fileReadable(fn))
            {
                throw new Exception(description + " file \"" + fn + "\" not readable.");
            }

            string msg = "Reading " + description + " data from \"" + fn + "\"";
            // The status message is printed becauset process listeners may not be registered.
            Message.printStatus(1, "StateMod_DataSet.readInputAnnounce1", msg);
            sendProcessListenerMessage(StateMod_GUIUtil.STATUS_READ_START, msg);
        }

        /// <summary>
        /// This method is a helper routine to readStateModFile().  It calls
        /// Message.printStatus() with the message that a file has been read successively.
        /// Then it prints a similar, but shorter, message to the status bar. </summary>
        /// <param name="comp"> Component being read. </param>
        /// <param name="seconds"> Number of seconds to read. </param>
        private void readStateModFile_Announce2(DataSetComponent comp, double seconds)
        {
            string routine = "StateMod_DataSet.readInputAnnounce2";
            string fn = getDataFilePathAbsolute(comp);
            string description = comp.getComponentName();

            // The status message is printed becauset process listeners may not be registered.
            string msg = description + " data read from \"" + fn + "\" in "
                + String.Format(seconds.ToString(), "%.3f") + " seconds";
            Message.printStatus(1, routine, msg);
            sendProcessListenerMessage(StateMod_GUIUtil.STATUS_READ_COMPLETE, msg);
        }

        /// <summary>
        /// Set a component dirty (edited).  This method is usually called by the set
        /// methods in the individual StateMod_Data classes.  This marks the component as
        /// dirty independent of the state of the individual data objects in the component.
        /// If a component is dirty, it needs to be written to a file because data or the file name have changed. </summary>
        /// <param name="componentType"> The component type within the data set(see COMP*). </param>
        /// <param name="isDirty"> true if the component should be marked as dirty (edited), false
        /// if the component should be marked clean (from data read, or edits saved). </param>
        public virtual void setDirty(int componentType, bool isDirty)
        {
            DataSetComponent comp = getComponentForComponentType(componentType);
            if (comp != null)
            {
                comp.setDirty(isDirty);
                //if ( Message.isDebugOn ) {
                //	Message.printDebug(1, "", "Setting component \"" + comp.getComponentName() + "\" dirty=" + isDirty);
                //	throw new RuntimeException ( "Find this");
                //}
            }
        }
    }
}
