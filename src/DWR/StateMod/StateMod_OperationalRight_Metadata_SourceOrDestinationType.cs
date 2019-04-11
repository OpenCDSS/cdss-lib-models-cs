using System;
using System.Collections.Generic;

// StateMod_OperationalRight_Metadata_SourceOrDestinationType - enumeration to store values
// for allowed operational right source and destination types

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
	// TODO SAM 2010-12-12 Evaluate how these compare to dataset components - seems like sources can in some cases
	// be a subtype of a component (e.g., different types of plans).
	/// <summary>
	/// This enumeration stores values for allowed operational right source and destination types,
	/// which can be used to perform checks and visualization.
	/// </summary>
	public sealed class StateMod_OperationalRight_Metadata_SourceOrDestinationType
	{
		/// <summary>
		/// Carrier (diversion that is a carrier).
		/// </summary>
		public static readonly StateMod_OperationalRight_Metadata_SourceOrDestinationType CARRIER = new StateMod_OperationalRight_Metadata_SourceOrDestinationType("CARRIER", InnerEnum.CARRIER, "Carrier");
		/// <summary>
		/// Diversion station.
		/// </summary>
		public static readonly StateMod_OperationalRight_Metadata_SourceOrDestinationType DIVERSION = new StateMod_OperationalRight_Metadata_SourceOrDestinationType("DIVERSION", InnerEnum.DIVERSION, "Diversion");
		/// <summary>
		/// Diversion right.
		/// </summary>
		public static readonly StateMod_OperationalRight_Metadata_SourceOrDestinationType DIVERSION_RIGHT = new StateMod_OperationalRight_Metadata_SourceOrDestinationType("DIVERSION_RIGHT", InnerEnum.DIVERSION_RIGHT, "Diversion Right");
		/// <summary>
		/// Downstream call.
		/// TODO SAM 2010-12-11 Should this just be a list of node types (other values in enum)?
		/// </summary>
		public static readonly StateMod_OperationalRight_Metadata_SourceOrDestinationType DOWNSTREAM_CALL = new StateMod_OperationalRight_Metadata_SourceOrDestinationType("DOWNSTREAM_CALL", InnerEnum.DOWNSTREAM_CALL, "Downstream Call");
		/// <summary>
		/// Instream flow station.
		/// </summary>
		public static readonly StateMod_OperationalRight_Metadata_SourceOrDestinationType INSTREAM_FLOW = new StateMod_OperationalRight_Metadata_SourceOrDestinationType("INSTREAM_FLOW", InnerEnum.INSTREAM_FLOW, "Instream Flow");
		/// <summary>
		/// Instream flow right.
		/// </summary>
		public static readonly StateMod_OperationalRight_Metadata_SourceOrDestinationType INSTREAM_FLOW_RIGHT = new StateMod_OperationalRight_Metadata_SourceOrDestinationType("INSTREAM_FLOW_RIGHT", InnerEnum.INSTREAM_FLOW_RIGHT, "Instream Flow Right");
		/// <summary>
		/// Operational right.
		/// </summary>
		public static readonly StateMod_OperationalRight_Metadata_SourceOrDestinationType OPERATIONAL_RIGHT = new StateMod_OperationalRight_Metadata_SourceOrDestinationType("OPERATIONAL_RIGHT", InnerEnum.OPERATIONAL_RIGHT, "Operational Right");
		/// <summary>
		/// Other node - in network but not any specific station type.
		/// </summary>
		public static readonly StateMod_OperationalRight_Metadata_SourceOrDestinationType OTHER = new StateMod_OperationalRight_Metadata_SourceOrDestinationType("OTHER", InnerEnum.OTHER, "Other");
		/// <summary>
		/// Plans as per StateMod documentation (shorten names because show up in UI choices with limited space)
		/// </summary>
		public static readonly StateMod_OperationalRight_Metadata_SourceOrDestinationType PLAN_ACCOUNTING = new StateMod_OperationalRight_Metadata_SourceOrDestinationType("PLAN_ACCOUNTING", InnerEnum.PLAN_ACCOUNTING, "Plan (Accounting)"); // Type 11
		public static readonly StateMod_OperationalRight_Metadata_SourceOrDestinationType PLAN_OUT_OF_PRIORITY = new StateMod_OperationalRight_Metadata_SourceOrDestinationType("PLAN_OUT_OF_PRIORITY", InnerEnum.PLAN_OUT_OF_PRIORITY, "Plan (OutOfPriority Div, Strg)"); // Type 9
		public static readonly StateMod_OperationalRight_Metadata_SourceOrDestinationType PLAN_RECHARGE = new StateMod_OperationalRight_Metadata_SourceOrDestinationType("PLAN_RECHARGE", InnerEnum.PLAN_RECHARGE, "Plan (Recharge)"); // Type 8
		public static readonly StateMod_OperationalRight_Metadata_SourceOrDestinationType PLAN_RELEASE_LIMIT = new StateMod_OperationalRight_Metadata_SourceOrDestinationType("PLAN_RELEASE_LIMIT", InnerEnum.PLAN_RELEASE_LIMIT, "Plan (Release Limit)"); // Type 12
		public static readonly StateMod_OperationalRight_Metadata_SourceOrDestinationType PLAN_REUSE_TO_RESERVOIR = new StateMod_OperationalRight_Metadata_SourceOrDestinationType("PLAN_REUSE_TO_RESERVOIR", InnerEnum.PLAN_REUSE_TO_RESERVOIR, "Plan (Reuse to Res)"); // Type 3
		public static readonly StateMod_OperationalRight_Metadata_SourceOrDestinationType PLAN_REUSE_TO_RESERVOIR_FROM_TRANSMOUNTAIN = new StateMod_OperationalRight_Metadata_SourceOrDestinationType("PLAN_REUSE_TO_RESERVOIR_FROM_TRANSMOUNTAIN", InnerEnum.PLAN_REUSE_TO_RESERVOIR_FROM_TRANSMOUNTAIN, "Plan (Reuse to Res Transmtn)"); // Type 5
		public static readonly StateMod_OperationalRight_Metadata_SourceOrDestinationType PLAN_REUSE_TO_DIVERSION = new StateMod_OperationalRight_Metadata_SourceOrDestinationType("PLAN_REUSE_TO_DIVERSION", InnerEnum.PLAN_REUSE_TO_DIVERSION, "Plan (Reuse to Div)"); // Type 4
		public static readonly StateMod_OperationalRight_Metadata_SourceOrDestinationType PLAN_REUSE_TO_DIVERSION_FROM_TRANSMOUNTAIN = new StateMod_OperationalRight_Metadata_SourceOrDestinationType("PLAN_REUSE_TO_DIVERSION_FROM_TRANSMOUNTAIN", InnerEnum.PLAN_REUSE_TO_DIVERSION_FROM_TRANSMOUNTAIN, "Plan (Reuse to Div Transmtn)"); // Type 6
		public static readonly StateMod_OperationalRight_Metadata_SourceOrDestinationType PLAN_SPECIAL_WELL_AUGMENTATION = new StateMod_OperationalRight_Metadata_SourceOrDestinationType("PLAN_SPECIAL_WELL_AUGMENTATION", InnerEnum.PLAN_SPECIAL_WELL_AUGMENTATION, "Plan (Special Well Aug)"); // Type 10
		public static readonly StateMod_OperationalRight_Metadata_SourceOrDestinationType PLAN_TC = new StateMod_OperationalRight_Metadata_SourceOrDestinationType("PLAN_TC", InnerEnum.PLAN_TC, "Plan (T&C)"); // Type 1
		public static readonly StateMod_OperationalRight_Metadata_SourceOrDestinationType PLAN_TRANSMOUNTAIN_IMPORT = new StateMod_OperationalRight_Metadata_SourceOrDestinationType("PLAN_TRANSMOUNTAIN_IMPORT", InnerEnum.PLAN_TRANSMOUNTAIN_IMPORT, "Plan (Transmtn Import)"); // Type 7
		public static readonly StateMod_OperationalRight_Metadata_SourceOrDestinationType PLAN_WELL_AUGMENTATION = new StateMod_OperationalRight_Metadata_SourceOrDestinationType("PLAN_WELL_AUGMENTATION", InnerEnum.PLAN_WELL_AUGMENTATION, "Plan (Well Aug)"); // Type 2
		/// <summary>
		/// Reservoir station.
		/// </summary>
		public static readonly StateMod_OperationalRight_Metadata_SourceOrDestinationType RESERVOIR = new StateMod_OperationalRight_Metadata_SourceOrDestinationType("RESERVOIR", InnerEnum.RESERVOIR, "Reservoir");
		/// <summary>
		/// Reservoir right.
		/// </summary>
		public static readonly StateMod_OperationalRight_Metadata_SourceOrDestinationType RESERVOIR_RIGHT = new StateMod_OperationalRight_Metadata_SourceOrDestinationType("RESERVOIR_RIGHT", InnerEnum.RESERVOIR_RIGHT, "Reservoir Right");
		/// <summary>
		/// River node.
		/// </summary>
		public static readonly StateMod_OperationalRight_Metadata_SourceOrDestinationType RIVER_NODE = new StateMod_OperationalRight_Metadata_SourceOrDestinationType("RIVER_NODE", InnerEnum.RIVER_NODE, "River Node");
		/// <summary>
		/// Stream gage.
		/// </summary>
		public static readonly StateMod_OperationalRight_Metadata_SourceOrDestinationType STREAM_GAGE = new StateMod_OperationalRight_Metadata_SourceOrDestinationType("STREAM_GAGE", InnerEnum.STREAM_GAGE, "Stream Gage");
		/// <summary>
		/// Well station.
		/// </summary>
		public static readonly StateMod_OperationalRight_Metadata_SourceOrDestinationType WELL = new StateMod_OperationalRight_Metadata_SourceOrDestinationType("WELL", InnerEnum.WELL, "Well");
		/// <summary>
		/// Well right.
		/// </summary>
		public static readonly StateMod_OperationalRight_Metadata_SourceOrDestinationType WELL_RIGHT = new StateMod_OperationalRight_Metadata_SourceOrDestinationType("WELL_RIGHT", InnerEnum.WELL_RIGHT, "Well Right");
		/// <summary>
		/// Not applicable.
		/// </summary>
		public static readonly StateMod_OperationalRight_Metadata_SourceOrDestinationType NA = new StateMod_OperationalRight_Metadata_SourceOrDestinationType("NA", InnerEnum.NA, "NA");

		private static readonly IList<StateMod_OperationalRight_Metadata_SourceOrDestinationType> valueList = new List<StateMod_OperationalRight_Metadata_SourceOrDestinationType>();

		static StateMod_OperationalRight_Metadata_SourceOrDestinationType()
		{
			valueList.Add(CARRIER);
			valueList.Add(DIVERSION);
			valueList.Add(DIVERSION_RIGHT);
			valueList.Add(DOWNSTREAM_CALL);
			valueList.Add(INSTREAM_FLOW);
			valueList.Add(INSTREAM_FLOW_RIGHT);
			valueList.Add(OPERATIONAL_RIGHT);
			valueList.Add(OTHER);
			valueList.Add(PLAN_ACCOUNTING);
			valueList.Add(PLAN_OUT_OF_PRIORITY);
			valueList.Add(PLAN_RECHARGE);
			valueList.Add(PLAN_RELEASE_LIMIT);
			valueList.Add(PLAN_REUSE_TO_RESERVOIR);
			valueList.Add(PLAN_REUSE_TO_RESERVOIR_FROM_TRANSMOUNTAIN);
			valueList.Add(PLAN_REUSE_TO_DIVERSION);
			valueList.Add(PLAN_REUSE_TO_DIVERSION_FROM_TRANSMOUNTAIN);
			valueList.Add(PLAN_SPECIAL_WELL_AUGMENTATION);
			valueList.Add(PLAN_TC);
			valueList.Add(PLAN_TRANSMOUNTAIN_IMPORT);
			valueList.Add(PLAN_WELL_AUGMENTATION);
			valueList.Add(RESERVOIR);
			valueList.Add(RESERVOIR_RIGHT);
			valueList.Add(RIVER_NODE);
			valueList.Add(STREAM_GAGE);
			valueList.Add(WELL);
			valueList.Add(WELL_RIGHT);
			valueList.Add(NA);
		}

		public enum InnerEnum
		{
			CARRIER,
			DIVERSION,
			DIVERSION_RIGHT,
			DOWNSTREAM_CALL,
			INSTREAM_FLOW,
			INSTREAM_FLOW_RIGHT,
			OPERATIONAL_RIGHT,
			OTHER,
			PLAN_ACCOUNTING,
			PLAN_OUT_OF_PRIORITY,
			PLAN_RECHARGE,
			PLAN_RELEASE_LIMIT,
			PLAN_REUSE_TO_RESERVOIR,
			PLAN_REUSE_TO_RESERVOIR_FROM_TRANSMOUNTAIN,
			PLAN_REUSE_TO_DIVERSION,
			PLAN_REUSE_TO_DIVERSION_FROM_TRANSMOUNTAIN,
			PLAN_SPECIAL_WELL_AUGMENTATION,
			PLAN_TC,
			PLAN_TRANSMOUNTAIN_IMPORT,
			PLAN_WELL_AUGMENTATION,
			RESERVOIR,
			RESERVOIR_RIGHT,
			RIVER_NODE,
			STREAM_GAGE,
			WELL,
			WELL_RIGHT,
			NA
		}

		public readonly InnerEnum innerEnumValue;
		private readonly string nameValue;
		private readonly int ordinalValue;
		private static int nextOrdinal = 0;

		/// <summary>
		/// The name that should be displayed when the best fit type is used in UIs and reports.
		/// </summary>
		private readonly string displayName;

		/// <summary>
		/// Construct a time series statistic enumeration value. </summary>
		/// <param name="displayName"> name that should be displayed in choices, etc. </param>
		private StateMod_OperationalRight_Metadata_SourceOrDestinationType(string name, InnerEnum innerEnum, string displayName)
		{
			this.displayName = displayName;

			nameValue = name;
			ordinalValue = nextOrdinal++;
			innerEnumValue = innerEnum;
		}

	/// <summary>
	/// Return the display name for the statistic.  This is usually the same as the
	/// value but using appropriate mixed case. </summary>
	/// <returns> the display name. </returns>
	public override string ToString()
	{
		return displayName;
	}

	/// <summary>
	/// Return the enumeration value given a string name (case-independent). </summary>
	/// <returns> the enumeration value given a string name (case-independent), or null if not matched. </returns>
	//public static StateMod_OperationalRight_Metadata_SourceOrDestinationType valueOfIgnoreCase(string name)
	//{
	//	StateMod_OperationalRight_Metadata_SourceOrDestinationType[] values = values();
	//	// Currently supported values
	//	foreach (StateMod_OperationalRight_Metadata_SourceOrDestinationType t in values)
	//	{
	//		if (name.Equals(t.ToString(), StringComparison.OrdinalIgnoreCase))
	//		{
	//			return t;
	//		}
	//	}
	//	return null;
	//}


		public static IList<StateMod_OperationalRight_Metadata_SourceOrDestinationType> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static StateMod_OperationalRight_Metadata_SourceOrDestinationType valueOf(string name)
		{
			foreach (StateMod_OperationalRight_Metadata_SourceOrDestinationType enumInstance in StateMod_OperationalRight_Metadata_SourceOrDestinationType.valueList)
			{
				if (enumInstance.nameValue == name)
				{
					return enumInstance;
				}
			}
			throw new System.ArgumentException(name);
		}
	}

}