using System;
using System.Collections.Generic;
using System.IO;

// StateMod_GraphNode - store an array of graph options

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
// StateMod_GraphNode - Created to store an array of graph options.  This 
//	includes:
//		* station type ( diversion, instream flow, reservoir, stream),
//		* ID
//		* data type (variable options: See the SMGUI documentation).
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
// 
// 19 Aug 1997	Catherine E.		Created initial version of class.
//		Nutting-Lane, RTi
// 04 Mar 1998	CEN, RTi		Added scenario, wrote SMParseGraphFile>
// 21 Dec 1998	CEN, RTi		Added throws IOException to read/write
//					routines.
// 18 Feb 2001	Steven A. Malers, RTi	Code review.  Add finalize().  Handle
//					nulls and set unused variables to null.
//					Alphabetize methods.  Change IO to
//					IOUtil.
// 19 Jul 2001	SAM, RTi		Update to use the StateMod version to
//					determine the columns for output.
//					Leave out wells from the visible choice
//					for now.
// 13 Aug 2001	SAM, RTi		Change so when writing graph template
//					there are no single quotes around the
//					location.  Enable wells to output.
// 2001-12-27	SAM, RTi		Update to use new fixedRead() to
//					improve performance.
// 2002-05-01	SAM, RTi		Add getNodeTypes() as newer version of
//					getGraphTypes().
// 2002-06-20	SAM, RTi		Update getGraphTypes() to default to new
//					StateMod if version is not known and
//					add a flag indicating whether to return
//					all types.
// 2002-08-05	SAM, RTi		Update SMWriteDelpltFile() to have more
//					current information.  Pass the file name
//					to SMDumpDelpltFile() so that it can be
//					shown in the header.  Change "instream
//					flow" to "instream" to be consistent
//					with Delplt.
// 2002-09-16	SAM, RTi		Change historical streamflow data type
//					from "Historical" to "StreamflowHist".
//					This allows "StreamflowBase" to be
//					added as appropriate.  Overload
//					getGraphDataType() to accept a flag
//					indicating whether a node is a base
//					flow node.
//------------------------------------------------------------------------------
// 2003-07-07	J. Thomas Sapienza, RTi	Renamed to StateMod_GraphNode
// 2003-08-25	JTS, RTi		* Renamed SMParseDelpltFile to 
//					  readStateModDelPltFile.
//					* Renamed SMWriteDelpltFile to 
//					  writeStateModDelPltFile.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
//------------------------------------------------------------------------------

namespace DWR.StateMod
{

	using TSIdent = RTi.TS.TSIdent;
	using IOUtil = RTi.Util.IO.IOUtil;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;

	public class StateMod_GraphNode
	{

	// Note that the following lists are set up as newline delimited lists to
	// support the grid.

	public const int TYPES = 5;
	public const int typesSize = 5;
	public static readonly string typesOptions = "Diversion\n" +
							"Instream Flow\n" +
							"Reservoir\n" +
							"Streamflow\n" +
							"Well";
	public static readonly string[] typesOptionsArray = new string[] {"Diversion", "Instream Flow", "Reservoir", "Streamflow", "Well"};

	public static readonly string[] node_types = new string[] {"Diversion", "Instream Flow", "Reservoir", "Streamflow", "Well"};

	public const int STREAM_TYPE = 0;
	public const int streamOptionsSize_0100 = 7;
	public static readonly string[] streamOptions_0100 = new string[] {"UpstreamInflow", "ReachGain", "ReturnFlow", "RiverInflow", "RiverDivert", "RiverOutflow", "StreamflowHist"};

	public const int streamOptionsSize_0901 = 8;
	public static readonly string[] streamOptions_0901 = new string[] {"UpstreamInflow", "ReachGain", "ReturnFlow", "RiverInflow", "RiverDivert", "RiverOutflow", "AvailableFlow", "StreamflowHist"};

	public const int streamOptionsSize_0969 = 9;
	public static readonly string[] streamOptions_0969 = new string[] {"Upstream_Inflow", "Reach_Gain", "Return_Flow", "River_Inflow", "River_Divert", "River_By_Well", "River_Outflow", "Available_Flow", "StreamflowHist"};

	public const int RESERVOIR_TYPE = 1;
	public const int reservoirOptionsSize_0100 = 21;
	public static readonly string[] reservoirOptions_0100 = new string[] {"InitialStorage", "RiverPriority", "RiverStorage", "RiverExchange", "CarrierPriority", "CarrierStorage", "TotalSupply", "StorageUse", "StorageExchange", "CarrierUse", "TotalRelease", "Evap", "SeepSpill", "SimEOM", "TargetLimit", "FillLimit", "Inflow", "Outflow", "HistoricalEOM", "HistoricalMin", "HistoricalMax"};

	public const int reservoirOptionsSize_0901 = 24;
	public static readonly string[] reservoirOptions_0901 = new string[] {"InitialStorage", "RiverPriority", "RiverStorage", "RiverExchange", "CarrierPriority", "CarrierStorage", "TotalSupply", "StorageUse", "StorageExchange", "CarrierUse", "TotalRelease", "Evap", "SeepSpill", "SimEOM", "TargetLimit", "FillLimit", "RiverInflow", "TotalRelease", "TotalSupply", "RiverByWell", "RiverOutflow", "HistoricalEOM", "HistoricalMin", "HistoricalMax"};

	public const int reservoirOptionsSize_0969 = 24;
	public static readonly string[] reservoirOptions_0969 = new string[] {"Initial_Storage", "River_Priority", "River_Storage", "River_Exchange", "Carrier_Priority", "Carrier_Storage", "Total_Supply", "Storage_Use", "Storage_Exchange", "Carrier_Use", "Total_Release", "Evap", "Seep_Spill", "Sim_EOM", "Target_Limit", "Fill_Limit", "River_Inflow", "Total_Release", "Total_Supply", "River_By_Well", "River_Outflow", "HistoricalEOM", "HistoricalMin", "HistoricalMax"};

	public const int INSTREAM_TYPE = 2;

	public const int instreamOptionsSize_0100 = 14;
	public static readonly string[] instreamOptions_0100 = new string[] {"ConsDemand", "FromRiverByPriority", "FromRiverByStorage", "FromRiverByExchange", "TotalSupply", "Short", "WaterUse,TotalReturn", "UpstreamInflow", "ReachGain", "ReturnFlow", "RiverInflow", "RiverDivert", "RiverOutflow", "Demand"};

	public const int instreamOptionsSize_0901 = 15;
	public static readonly string[] instreamOptions_0901 = new string[] {"ConsDemand", "FromRiverByPriority", "FromRiverByStorage", "FromRiverByExchange", "TotalSupply", "Short", "WaterUse,TotalReturn", "UpstreamInflow", "ReachGain", "ReturnFlow", "RiverInflow", "RiverDivert", "RiverOutflow", "AvailFlow", "Demand"};

	public const int instreamOptionsSize_0969 = 17;
	public static readonly string[] instreamOptions_0969 = new string[] {"Total_Demand", "CU_Demand", "From_River_By_Priority", "From_River_By_Storage", "From_River_By_Exchange", "Total_Supply", "Total_Short", "Total_Return", "Upstream_Inflow", "Reach_Gain", "Return_Flow", "River_Inflow", "River_Divert", "River_By_Well", "River_Outflow", "Available_Flow", "Demand"};

	public const int DIVERSION_TYPE = 3;
	public const int diversionOptionsSize_0100 = 18;
	public static readonly string[] diversionOptions_0100 = new string[] {"ConsDemand", "FromRiverByPriority", "FromRiverByStorage", "FromRiverByExchange", "FromCarrierByPriority", "FromCarierByStorage", "CarriedWater", "TotalSupply", "Short", "ConsumptiveWaterUse", "WaterUse,TotalReturn", "UpstreamInflow", "ReachGain", "ReturnFlow", "RiverInflow", "RiverDivert", "RiverOutflow", "Historical"};

	public const int diversionOptionsSize_0901 = 23;
	public static readonly string[] diversionOptions_0901 = new string[] {"ConsDemand", "FromRiverByPriority", "FromRiverByStorage", "FromRiverByExchange", "FromWell", "FromCarrierByPriority", "FromCarierByStorage", "CarriedWater", "TotalSupply", "Short", "ConsumptiveWaterUse", "WaterUse,TotalReturn", "UpstreamInflow", "ReachGain", "ReturnFlow", "WellDepletion", "To/FromGWStorage", "RiverInflow", "RiverDivert", "RiverByWell", "RiverOutflow", "AvailableFlow", "Historical"};

	public const int diversionOptionsSize_0969 = 28;
	public static readonly string[] diversionOptions_0969 = new string[] {"Total_Demand", "CU_Demand", "From_River_By_Priority", "From_River_By_Storage", "From_River_By_Exchange", "From_Well", "From_Carrier_By_Priority", "From_Carrier_By_Storage", "Carried_Water", "From_Soil", "Total_Supply", "Total_Short", "CU_Short", "Consumptive_Use", "To_Soil", "Total_Return", "Loss", "Upstream_Inflow", "Reach_Gain", "Return_Flow", "Well_Depletion", "To/From_GW_Storage", "River_Inflow", "River_Divert", "River_By_Well", "River_Outflow", "Available_Flow", "Historical"};

	public const int WELL_TYPE = 4;
	public const int wellOptionsSize_0901 = 11;
	public static readonly string[] wellOptions_0901 = new string[] {"Demand", "FromWell", "FromOther", "Short", "ConsumptiveUse", "Return", "Loss", "River", "GWStor", "Salvage", "Historical"};

	public const int wellOptionsSize_0969 = 19;
	public static readonly string[] wellOptions_0969 = new string[] {"Total_Demand", "CU_Demand", "From_Well", "From_SW", "From_Soil", "Total_Supply", "Total_Short", "CU_Short", "Total_CU", "To_Soil", "Total_Return", "Loss", "Total_Use", "From_River", "From_GwStor", "From_Salvage", "From_Soil", "Total_Source", "Historical"};

	public const int RUNTYPE_SINGLE = 0;
	public const int RUNTYPE_MULTIPLE = 1;
	public const int RUNTYPE_DIFFERENCE = 2;
	public const int RUNTYPE_MERGE = 3;
	public const int RUNTYPE_DIFFX = 4;

	protected internal string _type; // diversion, instream, reservoir, stream
	protected internal string _ID; // location
	protected internal string _Name; // used for output control file
	protected internal string _dtype; // data type associated with _type
	protected internal string _scenario; // scenario, optional
	protected internal int _switch; // off=0, on=1, used for output control
						// also used for big picture: run type
	protected internal IList<string> _IDVec; // location(s), used for big picture
	protected internal string _YrAve; // year or average, used for big picture
	protected internal string _fileName; // file name or entire path, big picture

	/// <summary>
	/// Constructor.
	/// </summary>
	public StateMod_GraphNode() : base()
	{
		initialize();
	}

	/// <summary>
	/// Add an ID to the ID Vector.
	/// </summary>
	public virtual void addID(string s)
	{
		if (!string.ReferenceEquals(s, null))
		{
			_IDVec.Add(s);
		}
	}

	//FIXME SAM 2008-03-24 Why is this here?  Does anything use it outside of this class?
	public static IList<string> arrayToVector(string[] array)
	{
		int size = array.Length;
		IList<string> v = new List<string>();
		for (int i = 0; i < size; i++)
		{
			v.Add(array[i]);
		}

		return v;
	}

	/// <summary>
	/// Clean up before garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~StateMod_GraphNode()
	{
		_type = null;
		_ID = null;
		_dtype = null;
		_scenario = null;
		_YrAve = null;
		_fileName = null;
		_IDVec = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Locate column number in output file which contains TS of interest.  
	/// filename must be included so a determination of StateMod version can be 
	/// made (i.e. were well included in this version or not).  The overloaded version
	/// with a StateMod version of -1.0 is used. </summary>
	/// <param name="s_type"> Station/structure type (e.g., "diversion"). </param>
	/// <param name="dtype"> Data type for the s_type. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static int getDataOutputColumn(String s_type, String dtype, String filename) throws Exception
	public static int getDataOutputColumn(string s_type, string dtype, string filename)
	{
		return getDataOutputColumn(s_type, dtype, filename, -1.0);
	}

	/// <summary>
	/// Locate column number in output file which contains TS of interest.
	/// Filename must be included so a determination of StateMod version can be
	/// made (i.e. were well included in this version or not). </summary>
	/// <returns> the column (1 is the first column in the output file). </returns>
	/// <param name="s_type"> Station/structure type (e.g., "diversion"). </param>
	/// <param name="dtype"> Data type for the s_type. </param>
	/// <param name="statemod_version"> StateMod version as floating point number, used to
	/// determine order of columns. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static int getDataOutputColumn(String s_type, String dtype, String filename, double statemod_version) throws Exception
	public static int getDataOutputColumn(string s_type, string dtype, string filename, double statemod_version)
	{
		string rtn = "StateMod_GraphNode.getDataOutputColumn";
		int type = getTypeFromString(s_type);
		int index = getDataOutputIndex(type, dtype, statemod_version);
		if (index == -999)
		{
			Message.printWarning(1, rtn, "Unable to get index for " + s_type + ", " + dtype);
			return -999;
		}

		if (Message.isDebugOn)
		{
			Message.printDebug(30, rtn, "Finding column for " + s_type + " (" + type + "), " + dtype);
		}
		if (type == RESERVOIR_TYPE)
		{
			// Does not depend on the version.  Make all columns available...
			return (index + 5);
		}
		else if (type == STREAM_TYPE)
		{
			if (statemod_version > 9.69)
			{
				if ((index == 0) || (index == 1) || (index == 2))
				{
					return index + 22;
				}
				else if (index >= 3)
				{
					return index + 24;
				}
			}
			else if (statemod_version > 9.01)
			{
				if ((index == 0) || (index == 1) || (index == 2))
				{
					return index + 17;
				}
				else if ((index == 3) || (index == 4))
				{
					return index + 19;
				}
				else if (index >= 5)
				{
					return index + 20;
				}
			}
			else
			{
				// Older...
				return index + 16;
			}
		}
		else if (type == INSTREAM_TYPE)
		{
			if (statemod_version >= 9.69)
			{
				switch (index)
				{
					case 0:
						return 5;
					case 1:
						return 6;
					case 2:
						return 7;
					case 3:
						return 8;
					case 4:
						return 9;
					case 5:
						return 15;
					case 6:
						return 16;
					case 7:
						return 20;
					case 8:
						return 22;
					case 9:
						return 23;
					case 10:
						return 24;
					case 11:
						return 27;
					case 12:
						return 28;
					case 13:
						return 29;
					case 14:
						return 30;
					case 15:
						return 31;
					default:
						return -999;
				}
			}
			else if (statemod_version >= 9.01)
			{
				switch (index)
				{
					case 0:
						return 5;
					case 1:
						return 6;
					case 2:
						return 7;
					case 3:
						return 8;
					case 4:
						return 13;
					case 5:
						return 14;
					case 6:
						return 16;
					case 7:
						return 17;
					case 8:
						return 18;
					case 9:
						return 19;
					case 10:
						return 22;
					case 11:
						return 23;
					case 12:
						return 25;
					case 13:
						return 26;
					default:
						return -999;
				}
			}
			else
			{ // Assume older...
				switch (index)
				{
					case 0:
						return 5;
					case 1:
						return 6;
					case 2:
						return 7;
					case 3:
						return 8;
					case 4:
						return 12;
					case 5:
						return 13;
					case 6:
						return 15;
					case 7:
						return 16;
					case 8:
						return 17;
					case 9:
						return 18;
					case 10:
						return 19;
					case 11:
						return 20;
					case 12:
						return 21;
					default:
						return -999;
				}
			}
		}
		else if (type == DIVERSION_TYPE)
		{
			// Version does not matter...
			return (index + 5);
		}
		else if (type == WELL_TYPE)
		{
			// Full list available...
			return (index + 5);
		}
		return -999;
	}

	/// <summary>
	/// Locate the index of the string matching dtype.
	/// </summary>
	public static int getDataOutputIndex(int type, string dtype)
	{
		return getDataOutputIndex(type, dtype, -1.0);
	}

	/// <summary>
	/// Locate the index of the string matching dtype. </summary>
	/// <param name="type"> Structure/station type as an integer. </param>
	/// <param name="dtype"> Data type to request (e.g., "Total_Return"); </param>
	/// <param name="statemod_version"> Statemod version as a double. </param>
	/// <returns> the index in the data types for the structure type (starting at zero). </returns>
	public static int getDataOutputIndex(int type, string dtype, double statemod_version)
	{
		// first, set Options to correct string list of options depending on the type

		IList<string> Options = getGraphDataType(type, statemod_version, true);
		if (Options == null)
		{
			Message.printWarning(1, "StateMod_GraphNode.getDataOutputIndex", "Unable to determine the StateMod output column " + "for \"" + dtype + "\"");
		}

		for (int i = 0; i < Options.Count; i++)
		{
			string s = Options[i];
			if (s.Equals(dtype, StringComparison.OrdinalIgnoreCase))
			{
				return i;
			}
		}
		return -999;
	}

	/// <summary>
	/// Return the dtype.
	/// </summary>
	public virtual string getDtype()
	{
		return _dtype;
	}

	/// <summary>
	/// Retrieve the fileName.
	/// </summary>
	public virtual string getFileName()
	{
		return _fileName;
	}

	/// <summary>
	/// Get data type associated with type.  It is assumed that StateMod is the newest version.
	/// Input (historical) and output data types are returned. </summary>
	/// <param name="type"> Structure/station data type (see StateMod_GraphNode.*_TYPE). </param>
	public static IList<string> getGraphDataType(int type)
	{
		return getGraphDataType(type, 1000.0, true);
	}

	/// <summary>
	/// Get data type associated with type.
	/// It is assumed that StateMod is the newest version. </summary>
	/// <param name="type"> Structure/station data type (see StateMod_GraphNode.*_TYPE). </param>
	/// <param name="include_all"> If true, input (historical) and output data types are returned.
	/// If false, only output data types are returned. </param>
	public static IList<string> getGraphDataType(int type, bool include_all)
	{
		return getGraphDataType(type, 1000.0, include_all);
	}

	/// <summary>
	/// Get the graph data types associated with type, for use with the StateMod GUI.
	/// It is assumed that the structure/station is NOT a baseflow node. </summary>
	/// <param name="type"> Structure/station data type. </param>
	/// <param name="statemod_version"> StateMod version as a floating point number. </param>
	/// <param name="include_all"> If true, historic time series are returned with the
	/// list (suitable for StateMod GUI graphing tool).  If false, only model output
	/// parameters are returned (suitable for delplt usage with the big picture plot). </param>
	public static IList<string> getGraphDataType(int type, double statemod_version, bool include_all)
	{
		return getGraphDataType(type, statemod_version, include_all, false);
	}

	/// <summary>
	/// Get the graph data types associated with type, for use with the StateMod GUI. </summary>
	/// <param name="type"> Structure/station data type. </param>
	/// <param name="statemod_version"> StateMod version as a floating point number. </param>
	/// <param name="include_all"> If true, historic time series are returned with the
	/// list (suitable for StateMod GUI graphing tool).  If false, only model output
	/// parameters are returned (suitable for delplt usage with the big picture plot). </param>
	/// <param name="is_baseflow"> Handled separately from "include_all".  If true, then
	/// a data type of "StreamflowBase" will be appended to the list. </param>
	/// <returns> a new-line delimited list of appropriate graph parameter types or null
	/// if the requested station type or version does not match a known combination. </returns>
	public static IList<string> getGraphDataType(int type, double statemod_version, bool include_all, bool is_baseflow)
	{
		IList<string> options = null;
		if (statemod_version >= 9.69)
		{
			if (type == STREAM_TYPE)
			{
				options = arrayToVector(streamOptions_0969);
				if (!include_all)
				{
					options = removeLastNElements(options, 1);
				}
			}
			else if (type == RESERVOIR_TYPE)
			{
				options = arrayToVector(reservoirOptions_0969);
				if (!include_all)
				{
					options = removeLastNElements(options, 3);
				}
			}
			else if (type == INSTREAM_TYPE)
			{
				options = arrayToVector(instreamOptions_0969);
				if (!include_all)
				{
					options = removeLastNElements(options, 1);
				}
			}
			else if (type == DIVERSION_TYPE)
			{
				options = arrayToVector(diversionOptions_0969);
				if (!include_all)
				{
					options = removeLastNElements(options, 1);
				}
			}
			else if (type == WELL_TYPE)
			{
				options = arrayToVector(wellOptions_0969);
				if (!include_all)
				{
					options = removeLastNElements(options, 1);
				}
			}
		}
		else if (statemod_version >= 9.01)
		{
			if (type == STREAM_TYPE)
			{
				options = arrayToVector(streamOptions_0901);
				if (!include_all)
				{
					options = removeLastNElements(options, 1);
				}
			}
			else if (type == RESERVOIR_TYPE)
			{
				options = arrayToVector(reservoirOptions_0901);
				if (!include_all)
				{
					options = removeLastNElements(options, 3);
				}
			}
			else if (type == INSTREAM_TYPE)
			{
				options = arrayToVector(instreamOptions_0901);
				if (!include_all)
				{
					options = removeLastNElements(options, 1);
				}
			}
			else if (type == DIVERSION_TYPE)
			{
				options = arrayToVector(diversionOptions_0901);
				if (!include_all)
				{
					options = removeLastNElements(options, 1);
				}
			}
			else if (type == WELL_TYPE)
			{
				options = arrayToVector(wellOptions_0901);
				if (!include_all)
				{
					options = removeLastNElements(options, 1);
				}
			}
		}
		else
		{
			// Assume old...
			if (type == STREAM_TYPE)
			{
				options = arrayToVector(streamOptions_0100);
				if (!include_all)
				{
					options = removeLastNElements(options, 1);
				}
			}
			else if (type == RESERVOIR_TYPE)
			{
				options = arrayToVector(reservoirOptions_0100);
				if (!include_all)
				{
					options = removeLastNElements(options, 3);
				}
			}
			else if (type == INSTREAM_TYPE)
			{
				options = arrayToVector(instreamOptions_0100);
				if (!include_all)
				{
					options = removeLastNElements(options, 1);
				}
			}
			else if (type == DIVERSION_TYPE)
			{
				options = arrayToVector(diversionOptions_0100);
				if (!include_all)
				{
					options = removeLastNElements(options, 1);
				}
			}
		}

		if (options == null)
		{
			options = new List<string>();
		}

		if (is_baseflow)
		{
			// Append "StreamflowBase" on the end of the data types.
			// Based on past work the end of the string may have a "\n " or
			// be a parameter.
			options.Add("StreamflowBase");
		}
		return options;
	}

	/// <summary>
	/// Get number of options associated with type.
	/// </summary>
	public static int getGraphDataTypeSize(int type)
	{
		return getGraphDataTypeSize(type, -1.0);
	}

	/// <summary>
	/// Get number of options associated with type. </summary>
	/// <param name="type"> Structure/station type. </param>
	/// <param name="statemod_version"> StateMod version  </param>
	public static int getGraphDataTypeSize(int type, double statemod_version)
	{
		if (statemod_version >= 9.69)
		{
			if (type == STREAM_TYPE)
			{
				return streamOptionsSize_0969;
			}
			else if (type == RESERVOIR_TYPE)
			{
				return reservoirOptionsSize_0969;
			}
			else if (type == INSTREAM_TYPE)
			{
				return instreamOptionsSize_0969;
			}
			else if (type == DIVERSION_TYPE)
			{
				return diversionOptionsSize_0969;
			}
			else if (type == WELL_TYPE)
			{
				return wellOptionsSize_0969;
			}
			return 0;
		}
		if (statemod_version >= 9.01)
		{
			if (type == STREAM_TYPE)
			{
				return streamOptionsSize_0901;
			}
			else if (type == RESERVOIR_TYPE)
			{
				return reservoirOptionsSize_0901;
			}
			else if (type == INSTREAM_TYPE)
			{
				return instreamOptionsSize_0901;
			}
			else if (type == DIVERSION_TYPE)
			{
				return diversionOptionsSize_0901;
			}
			else if (type == WELL_TYPE)
			{
				return wellOptionsSize_0901;
			}
			return 0;
		}
		else
		{
			// Assume old...
			if (type == STREAM_TYPE)
			{
				return streamOptionsSize_0100;
			}
			else if (type == RESERVOIR_TYPE)
			{
				return reservoirOptionsSize_0100;
			}
			else if (type == INSTREAM_TYPE)
			{
				return instreamOptionsSize_0100;
			}
			else if (type == DIVERSION_TYPE)
			{
				return diversionOptionsSize_0100;
			}
			return 0;
		}
	}

	/// <summary>
	/// Get types available.
	/// </summary>
	public static string getGraphTypes()
	{
		return typesOptions;
	}

	public static string getGraphTypes(int index)
	{
		return typesOptionsArray[index];
	}

	/// <summary>
	/// Return the ID.
	/// </summary>
	public virtual string getID()
	{
		return _ID;
	}

	/// <summary>
	/// Return the ID at a position in the ID vector (used with Delplt runs).
	/// </summary>
	public virtual string getID(int pos)
	{
		return (string)_IDVec[pos];
	}

	/// <summary>
	/// Retrieve the ID Vector.
	/// </summary>
	public virtual IList<string> getIDVec()
	{
		return _IDVec;
	}

	/// <summary>
	/// Retrieve the size of the ID Vector.
	/// </summary>
	public virtual int getIDVectorSize()
	{
		return _IDVec.Count;
	}

	/// <summary>
	/// Return the Name.
	/// </summary>
	public virtual string getName()
	{
		return _Name;
	}

	/// <summary>
	/// Get available node types. </summary>
	/// <returns> array of available node types. </returns>
	public static string [] getNodeTypes()
	{
		return node_types;
	}

	/// <summary>
	/// Get available node type by index. </summary>
	/// <returns> available node type by index. </returns>
	/// <param name="index"> Index in the node types array. </param>
	public static string getNodeType(int index)
	{
		return node_types[index];
	}

	/// <summary>
	/// Return the scenario.
	/// </summary>
	public virtual string getScenario()
	{
		return _scenario;
	}

	/// <summary>
	/// Return the switch.
	/// </summary>
	public virtual int getSwitch()
	{
		return _switch;
	}

	/// <summary>
	/// Return the type.
	/// </summary>
	public virtual string getType()
	{
		return _type;
	}

	/// <summary>
	/// Given a node type (e.g., "diversion") return the internal integer value
	/// corresponding to the type. </summary>
	/// <param name="type"> node type (e.g., "diversion"); </param>
	/// <returns> internal integer value (e.g., DIVERSION_TYPE). </returns>
	public static int getTypeFromString(string type)
	{
		if (string.ReferenceEquals(type, null))
		{
			return -999;
		}
		if (type.regionMatches(true,0,"d",0,1))
		{ // diversion
			return DIVERSION_TYPE;
		}
		else if (type.regionMatches(true,0,"i",0,1))
		{ // instream flow
			return INSTREAM_TYPE;
		}
		else if (type.regionMatches(true,0,"r",0,1))
		{ // reservoir
			return RESERVOIR_TYPE;
		}
		else if (type.regionMatches(true,0,"s",0,1))
		{ // stream
			return STREAM_TYPE;
		}
		else if (type.regionMatches(true,0,"w",0,1))
		{ // stream
			return WELL_TYPE;
		}
		return -999;
	}

	/// <summary>
	/// Retrieve the string containing either a year ("1989") or the string "Ave".
	/// </summary>
	public virtual string getYrAve()
	{
		return _YrAve;
	}

	private void initialize()
	{
		_type = "";
		_ID = "";
		_dtype = "";
		_scenario = "";
		_switch = 1;
		_YrAve = "";
		_fileName = "";
		_IDVec = new List<string>();
	}

	/// <summary>
	/// Set the type.
	/// </summary>
	public virtual void setDtype(string s)
	{
		if (!string.ReferenceEquals(s, null))
		{
			_dtype = s;
		}
	}

	/// <summary>
	/// Set the fileName.
	/// </summary>
	public virtual void setFileName(string s)
	{
		if (!string.ReferenceEquals(s, null))
		{
			_fileName = s;
		}
	}

	/// <summary>
	/// Set the ID.
	/// </summary>
	public virtual void setID(string s)
	{
		if (!string.ReferenceEquals(s, null))
		{
			_ID = s;
		}
	}

	/// <summary>
	/// Set the Name.
	/// </summary>
	public virtual void setName(string s)
	{
		if (!string.ReferenceEquals(s, null))
		{
			_Name = s;
		}
	}

	/// <summary>
	/// Set the scenario.
	/// </summary>
	public virtual void setScenario(string s)
	{
		if (!string.ReferenceEquals(s, null))
		{
			_scenario = s;
		}
	}

	/// <summary>
	/// Set the switch.
	/// </summary>
	public virtual void setSwitch(int i)
	{
		_switch = i;
	}

	public virtual void setSwitch(int? i)
	{
		_switch = i.Value;
	}

	/// <summary>
	/// Set the type.
	/// </summary>
	public virtual void setType(string s)
	{
		string temp = s;
		if (!string.ReferenceEquals(temp, null))
		{
			if (temp.Equals("diversion"))
			{
				temp = "Diversion";
			}
			else if (temp.Equals("instream", StringComparison.OrdinalIgnoreCase) || temp.Equals("InstreamFlow", StringComparison.OrdinalIgnoreCase))
			{
				temp = "Instream Flow";
			}
			else if (temp.Equals("reservoir"))
			{
				temp = "Reservoir";
			}
			else if (temp.Equals("stream", StringComparison.OrdinalIgnoreCase))
			{
				temp = "Streamflow";
			}
			else if (temp.Equals("well"))
			{
				temp = "Well";
			}

			_type = temp;
		}
	}

	/// <summary>
	/// Set the YrAve.
	/// </summary>
	public virtual void setYrAve(string s)
	{
		if (!string.ReferenceEquals(s, null))
		{
			_YrAve = s;
		}
	}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static int SMDumpDelpltFile(java.util.List<StateMod_GraphNode> theTemplate, String filename, java.io.PrintWriter out) throws java.io.IOException
	public static int SMDumpDelpltFile(IList<StateMod_GraphNode> theTemplate, string filename, PrintWriter @out)
	{
		string rtn = "StateMod_GraphNode.SMDumpDelpltFile";
		string temp_cmnt = "#>";
		string cmnt = "#";
		StateMod_GraphNode node = null;
		IList<string> v = null;

		int num = 0;
		if (theTemplate != null)
		{
			num = theTemplate.Count;
		}
		try
		{
			@out.println(temp_cmnt + " " + filename + " - SmDelta input file");
			@out.println(temp_cmnt);
			@out.println(temp_cmnt);

			for (int i = 0; i < num; i++)
			{
				node = theTemplate[i];
				if (i == 0)
				{
					@out.println(cmnt);
					@out.println(cmnt + " Run type (Single, Multiple, Difference, " + "Diffx, Merge):");
					@out.println(cmnt);
					int run_type = node.getSwitch();
					if (run_type == RUNTYPE_SINGLE)
					{
						@out.println("Single");
					}
					else if (run_type == RUNTYPE_MULTIPLE)
					{
						@out.println("Multiple");
					}
					else if (run_type == RUNTYPE_DIFFERENCE)
					{
						@out.println("Difference");
					}
					else if (run_type == RUNTYPE_DIFFX)
					{
						@out.println("Diffx");
					}
					else if (run_type == RUNTYPE_MERGE)
					{
						@out.println("Merge");
					}
				}

				@out.println(cmnt + cmnt + cmnt + cmnt + cmnt);
				@out.println(cmnt);
				@out.println(cmnt + "     File:");
				@out.println(cmnt + "          For reservoirs use .xre or .b44");
				@out.println(cmnt + "          For others use .xde or .b43");
				@out.println(node.getFileName());
				@out.println(cmnt);
				@out.println(cmnt + "     Data type (Diversion, Instream, StreamGage, StreamID, Reservoir, Well):");
				@out.println(node.getType());
				@out.println(cmnt);
				@out.println(cmnt + "     Parameter (same as StateModGUI) or type statemod -h");
				@out.println(node.getDtype());
				@out.println(cmnt);
				@out.println(cmnt + "     ID (0=all, n=ID, end with a -999)");
				v = node.getIDVec();
				int numIDs = v.Count;
				for (int j = 0; j < numIDs; j++)
				{
					@out.println(v[j]);
				}
				@out.println("-999");
				@out.println(cmnt);
				@out.println(cmnt + "     Time (year [e.g., 1989], year and month [e.g. 1989 NOV], or Ave)");
				@out.println(node.getYrAve());
				@out.println(cmnt);
			}
			@out.println(cmnt + cmnt + cmnt + cmnt + cmnt);
			@out.println(cmnt);
			@out.println(cmnt + "     End of file indicator");
			@out.println("-999");
		}
		catch (Exception e)
		{
			Message.printWarning(2, rtn, e);
			throw new IOException(e.Message);
		}
		return 0;
	}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static int SMDumpGraphFile(java.util.List<StateMod_GraphNode> theGraphOpts, java.io.PrintWriter out) throws java.io.IOException
	public static int SMDumpGraphFile(IList<StateMod_GraphNode> theGraphOpts, PrintWriter @out)
	{
		StateMod_GraphNode node = null;
		string ident = null;

		int i;
		int num = 0;
		@out.println("#>");
		@out.println("#> Each line below identifies a time series.");
		@out.println("#> The identifier consists of the following fields:");
		@out.println("#> LocationID..NodeType_DataType.Interval.");
		@out.println("#>");
		if (theGraphOpts != null)
		{
			num = theGraphOpts.Count;
		}
		try
		{
			for (i = 0; i < num; i++)
			{
				node = theGraphOpts[i];
				// Identifier is ID..StructType_DataType.MONTH.Scenario
				ident = node.getID() + ".." + node.getType() + "_" + node.getDtype() + ".MONTH." + node.getScenario();
				@out.println(ident);
			}
		}
		catch (Exception e)
		{
			node = null;
			ident = null;
			Message.printWarning(2, "StateMod_GraphNode.SMDumpGraphFile", e);
			throw new IOException(e.Message);
		}
		return 0;
	}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static int SMDumpOutputControlFile(java.util.List<StateMod_GraphNode> theOC, java.io.PrintWriter out) throws java.io.IOException
	public static int SMDumpOutputControlFile(IList<StateMod_GraphNode> theOC, PrintWriter @out)
	{
		string iline = null;
		string rtn = "StateMod_GraphNode.SMDumpOutputControlFile";
		string format = "%-12.12s %-24.24s %-3.3s %5d";
		string cmnt = "#>";
		StateMod_GraphNode node = null;
		IList<object> v = new List<object> (4);

		int i;
		int num = 0;
		if (theOC != null)
		{
			num = theOC.Count;
		}
		try
		{
			@out.println(cmnt + "*.xou; Output request file for StateMod");
			@out.println(cmnt);
			@out.println(cmnt + "Type ( e.g.  Diversion, StreamGage or Reservoir, or All)");
			@out.println(cmnt);
			@out.println("All"); // hardcoded - may need to change
			@out.println(cmnt);
			@out.println(cmnt + "Parameter (e.g. TotalSupply, SimEOM, RiverOutflow, or All)");
			@out.println(cmnt);
			@out.println("All"); // hardcoded - may need to change
			@out.println(cmnt);
			@out.println(cmnt + "ID Name Type and Print Code (0=no, 1=yes)");
			@out.println(cmnt + "Note: id = All prints all");
			@out.println(cmnt + "      id = -999 = stop");
			@out.println(cmnt + "      default is to turn on all stream gages (FLO)");
			@out.println(cmnt);

			int istart = 0;
			if (num > 0)
			{
				node = theOC[0];

				if (node.getID().Equals("All", StringComparison.OrdinalIgnoreCase))
				{
					@out.println("All");
					istart = 1;
				}
			}

			for (i = istart; i < num; i++)
			{
				node = theOC[i];
				if (node == null)
				{
					continue;
				}

				// need to format
				v.Clear();
				v.Add(node.getID());
				v.Add(node.getName());
				v.Add(node.getType());
				v.Add(new int?(node.getSwitch()));

				iline = StringUtil.formatString(v, format);

				if (Message.isDebugOn)
				{
					Message.printDebug(30, rtn, "Adding " + iline);
				}
				@out.println(iline);
			}
			@out.println("-999"); // stop code to StateMod
		}
		catch (Exception e)
		{
			iline = null;
			rtn = null;
			format = null;
			cmnt = null;
			node = null;
			v = null;
			Message.printWarning(2, rtn, e);
			throw new IOException(e.Message);
		}

		iline = null;
		rtn = null;
		format = null;
		cmnt = null;
		node = null;
		v = null;
		return 0;
	}

	/// <summary>
	/// Parse a SmDelta input file.  Each StateMod_GraphNode contains a
	/// station type/file/parameter combination.  The identifiers associated with the
	/// graph node are stored in a list with each node (e.g., can be a single zero
	/// string in the list or a list of identifiers).  The run mode is stored with
	/// each item returned. </summary>
	/// <param name="theTemplate"> An existing list to have graph nodes added to it. </param>
	/// <param name="filename"> Name of the file name to read. </param>
	/// <returns> 1 if an error or 0 if successful. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static int readStateModDelPltFile(java.util.List<StateMod_GraphNode> theTemplate, String filename) throws java.io.IOException
	public static int readStateModDelPltFile(IList<StateMod_GraphNode> theTemplate, string filename)
	{
		string rtn = "StateMod_GraphNode.readStateModDelPltFile";
		string iline = null;
		StreamReader @in = null;
		StateMod_GraphNode aNode = null;

		int step = 0, run_type = 0;

		Message.printStatus(1, rtn, "Reading SmDelta template: " + filename);
		try
		{
			@in = new StreamReader(filename);
			while (!string.ReferenceEquals((iline = @in.ReadLine()), null))
			{
				// check for comments
				iline = StringUtil.removeNewline(iline);
				if (iline.StartsWith("#", StringComparison.Ordinal) || iline.Trim().Length == 0)
				{
					continue;
				}

				if (Message.isDebugOn)
				{
					Message.printDebug(50, rtn, "line: \"" + iline + "\", step=" + step);
				}

				if (step == 0)
				{
					// Need the run mode...
					if (iline.Equals("single", StringComparison.OrdinalIgnoreCase))
					{
						run_type = RUNTYPE_SINGLE;
					}
					else if (iline.Equals("multiple", StringComparison.OrdinalIgnoreCase))
					{
						run_type = RUNTYPE_MULTIPLE;
					}
					else if (iline.Equals("difference", StringComparison.OrdinalIgnoreCase))
					{
						run_type = RUNTYPE_DIFFERENCE;
					}
					else if (iline.Equals("diffx", StringComparison.OrdinalIgnoreCase))
					{
						run_type = RUNTYPE_DIFFX;
					}
					else if (iline.Equals("merge", StringComparison.OrdinalIgnoreCase))
					{
						run_type = RUNTYPE_MERGE;
					}
					step++;
				}
				else if (step == 1)
				{
					if (iline.Equals("-999"))
					{
						// end of file indicator
						Message.printDebug(40, rtn, "End of file indicator found");
						break;
					}
					else
					{
						// Allocate new graph node and set top-level data like the run mode...
						aNode = new StateMod_GraphNode();

						// File
						// create a new node, store file name
						aNode.setSwitch(run_type);
						aNode.setFileName(iline.Trim());
						step++;
					}
				}
				else if (step == 2)
				{
					// Node type
					aNode.setType(iline.Trim());
					step++;
				}
				else if (step == 3)
				{
					// parameter
					aNode.setDtype(iline.Trim());
					step++;
				}
				else if (step == 4)
				{
					// ID
					// If end of list indicator is found, 
					// increment the step counter.  Otherwise, 
					// add the id to the list, but don't increment
					// the step counter because the next line will
					// be either the end of list indicator or another ID.
					if (iline.Equals("-999"))
					{
						step++;
					}
					else
					{
						aNode.addID(iline.Trim());
					}
				}
				else if (step == 5)
				{
					// Year or Average ( Ave or 1989 NOV)
					// LAST STEP - set the step counter to 1
					// (0 was runtype and will only occur once).
					aNode.setYrAve(iline.Trim());
					step = 1;
					theTemplate.Add(aNode);
				}
			}
		}
		catch (Exception e)
		{
			if (@in != null)
			{
				@in.Close();
			}
			Message.printWarning(2, rtn, e);
			throw new IOException(e.Message);
		}
		if (@in != null)
		{
			@in.Close();
		}
		return 0;
	}

	/// <summary>
	/// Read graph information in and store in a java vector.  The new graph types are
	/// added to the end of the previously stored diversions.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static int readStateModGraphFile(java.util.List<StateMod_GraphNode> theGraphOpts, String filename) throws java.io.IOException
	public static int readStateModGraphFile(IList<StateMod_GraphNode> theGraphOpts, string filename)
	{
		string rtn = "StateMod_GraphNode.readStateModGraphFile";
		string iline = null;
		StreamReader @in = null;
		StateMod_GraphNode aNode = null;
		TSIdent ident = null;
		IList<string> list = null;
		int dtype_pos = 0;

		Message.printStatus(1, rtn, "Reading graph template: " + filename);
		try
		{
			@in = new StreamReader(filename);
			while (!string.ReferenceEquals((iline = @in.ReadLine()), null))
			{
				// check for comments
				if (iline.StartsWith("#", StringComparison.Ordinal) || iline.Trim().Length == 0)
				{
					continue;
				}

				// allocate new diversion node
				aNode = new StateMod_GraphNode();

				if (Message.isDebugOn)
				{
					Message.printDebug(50, rtn, "line 1: " + iline);
				}
				ident = TSIdent.parseIdentifier(iline);

				aNode.setID(ident.getLocation());
				aNode.setScenario(ident.getScenario());

				// break up type into type (e.g., "diversion") and
				// dtype (e.g., "Total_Demand")...
				list = StringUtil.breakStringList(ident.getType(), "_", 0);
				if (Message.isDebugOn)
				{
					Message.printDebug(50, rtn, ident.getType());
				}
				if (list.Count < 2)
				{
					continue;
				}
				aNode.setType(list[0]);
				if (aNode.getType().Equals("instream flow", StringComparison.OrdinalIgnoreCase))
				{
					// New convention is shorter as of 2002-08-06
					aNode.setType("instream");
				}
				if (Message.isDebugOn)
				{
					Message.printDebug(50, rtn, aNode.getType());
				}
				// The data type may have an underscore so just set the
				// specific data type to the remaining string.
				//aNode.setDtype ( (String)list.elementAt(1));
				dtype_pos = ident.getType().IndexOf("_");
				if (dtype_pos < ident.getType().length())
				{
					aNode.setDtype(ident.getType().substring(dtype_pos + 1));
				}
				if (Message.isDebugOn)
				{
					Message.printDebug(50, rtn, aNode.getDtype());
				}

				theGraphOpts.Add(aNode);
			}
		}
		catch (Exception e)
		{
			if (@in != null)
			{
				@in.Close();
			}
			Message.printWarning(2, rtn, e);
			throw new IOException(e.Message);
		}
		if (@in != null)
		{
			@in.Close();
		}
		return 0;
	}

	/// <summary>
	/// Read output control information in and store in a java vector.  The new control
	/// information is added to the end of the previously stored information.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static int readStateModOutputControlFile(java.util.List<StateMod_GraphNode> theOC, String filename) throws java.io.IOException
	public static int readStateModOutputControlFile(IList<StateMod_GraphNode> theOC, string filename)
	{
		string rtn = "StateMod_GraphNode.readStateModOutputControlFile";
		string format = "s12x1s24x1s3x1i5";
		string iline = null;
		IList<object> v = null;
		StreamReader @in = null;
		StateMod_GraphNode aNode = null;

		int skipAll = 0; // there are 2 "All" statements we need to skip over
				// if a 3rd exists, we need to pass that info back

		Message.printStatus(1, rtn, "Reading output control template: " + filename);
		try
		{
			@in = new StreamReader(filename);
			while (!string.ReferenceEquals((iline = @in.ReadLine()), null))
			{
				// check for comments
				if (iline.StartsWith("#", StringComparison.Ordinal) || iline.Trim().Length == 0)
				{
					continue;
				}

				if (iline.Equals("All", StringComparison.OrdinalIgnoreCase) && skipAll < 2)
				{
					skipAll++;
					continue;
				}

				if (iline.StartsWith("-999", StringComparison.Ordinal))
				{
					return 0;
				}

				// allocate new diversion node
				aNode = new StateMod_GraphNode();

				if (Message.isDebugOn)
				{
					Message.printDebug(50, rtn, "line 1: " + iline);
				}
				v = StringUtil.fixedRead(iline, format);
				if (v.Count < 4)
				{
					if (iline.Equals("All", StringComparison.OrdinalIgnoreCase))
					{
						aNode.setID("All");
						theOC.Add(aNode);
						continue;
					}
					else
					{
						Message.printWarning(2, rtn, "Unable to process \"" + iline + "\"");
						continue;
					}
				}

				aNode.setID(((string)v[0]).Trim());
				aNode.setName(((string)v[1]).Trim());
				aNode.setType(((string)v[2]).Trim());
				aNode.setSwitch((int?)v[3]);

				theOC.Add(aNode);
			}
		}
		catch (Exception e)
		{
			if (@in != null)
			{
				@in.Close();
			}
			Message.printWarning(2, rtn, e);
			throw new IOException(e.Message);
		}
		finally
		{
			if (@in != null)
			{
				@in.Close();
			}
		}
		return 0;
	}

	private static IList<string> removeLastNElements(IList<string> v, int count)
	{
		IList<string> r = new List<string>();
		int itransfer = v.Count - count;
		for (int i = 0; i < itransfer; i++)
		{
			r.Add(v[i]);
		}

		return r;
	}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeStateModDelPltFile(String instrfile, String outstrfile, java.util.List<StateMod_GraphNode> theTemplate, String[] new_comments) throws java.io.IOException
	public static void writeStateModDelPltFile(string instrfile, string outstrfile, IList<StateMod_GraphNode> theTemplate, string[] new_comments)
	{
		string rtn = "StateMod_GraphNode.writeStateModDelPltFile";
		string[] comment_str = new string[] {"#"};
		string[] ignore_comment_str = new string[] {"#>"};
		PrintWriter @out = null;
		if (Message.isDebugOn)
		{
			Message.printDebug(1, rtn, "in writeStateModDelPltFile printing file: " + outstrfile);
		}
		try
		{
			@out = IOUtil.processFileHeaders(instrfile, outstrfile, new_comments, comment_str, ignore_comment_str, 0);
			SMDumpDelpltFile(theTemplate, outstrfile, @out);
			@out.flush();
			@out.close();
		}
		catch (Exception e)
		{
			if (@out != null)
			{
				@out.flush();
				@out.close();
			}
			Message.printWarning(2, rtn, e);
			throw new IOException(e.Message);
		}
	}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeStateModGraphFile(String instrfile, String outstrfile, java.util.List<StateMod_GraphNode> theGraphOpts, String[] new_comments) throws java.io.IOException
	public static void writeStateModGraphFile(string instrfile, string outstrfile, IList<StateMod_GraphNode> theGraphOpts, string[] new_comments)
	{
		string rtn = "StateMod_GraphNode.writeStateModGraphFile";
		string[] comment_str = new string[] {"#"};
		string[] ignore_comment_str = new string[] {"#>"};
		PrintWriter @out = null;

		if (Message.isDebugOn)
		{
			Message.printDebug(2, rtn, "in writeStateModGraphFile printing file: " + outstrfile);
		}
		try
		{
			@out = IOUtil.processFileHeaders(instrfile, outstrfile, new_comments, comment_str, ignore_comment_str, 0);
			SMDumpGraphFile(theGraphOpts, @out);
			@out.flush();
			@out.close();
		}
		catch (Exception e)
		{
			if (@out != null)
			{
				@out.flush();
				@out.close();
			}
			throw new IOException(e.Message);
		}
	}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeStateModOutputControlFile(String instrfile, String outstrfile, java.util.List<StateMod_GraphNode> theOC, String[] new_comments) throws java.io.IOException
	public static void writeStateModOutputControlFile(string instrfile, string outstrfile, IList<StateMod_GraphNode> theOC, string[] new_comments)
	{
		string rtn = "StateMod_GraphNode.writeStateModOutputControlFile";
		string[] comment_str = new string[] {"#"};
		string[] ignore_comment_str = new string[] {"#>"};
		PrintWriter @out = null;
		if (Message.isDebugOn)
		{
			Message.printDebug(1, rtn, "Printing file: " + outstrfile);
		}
		try
		{
			@out = IOUtil.processFileHeaders(instrfile, outstrfile, new_comments, comment_str, ignore_comment_str, 0);
			SMDumpOutputControlFile(theOC, @out);
			@out.flush();
			@out.close();
		}
		catch (Exception e)
		{
			if (@out != null)
			{
				@out.flush();
				@out.close();
			}
			Message.printWarning(2, rtn, e);
			throw new IOException(e.Message);
		}
	}

	/// <summary>
	/// Return a string representation of the object. </summary>
	/// <returns> a string representation of the object. </returns>
	public override string ToString()
	{
		return "Type: " + _type + ", ID: " + _ID + ", Name: " + _Name + ", DataType: " + _dtype + ", Scenario: " + _scenario + ", Switch/RunType: " + _switch + ", IDVec: " + _IDVec + ", YrAve: " + _YrAve + ", Filename: " + _fileName;
	}

	}

}