using System;
using System.Collections.Generic;

// StateMod_OperationalRight_Metadata_RuleType - enumeration to store values
// for operational right rule types

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
	/// <summary>
	/// This enumeration stores values for operational right rule types, which can be used to determine the type
	/// of right, consistent with the StateMod documentation
	/// </summary>
	public sealed class StateMod_OperationalRight_Metadata_RuleType
	{ // List these in the order of common use, as per StateMod documentation
		/// <summary>
		/// Water source is a reservoir.
		/// </summary>
		public static readonly StateMod_OperationalRight_Metadata_RuleType SOURCE_RESERVOIR = new StateMod_OperationalRight_Metadata_RuleType("SOURCE_RESERVOIR", InnerEnum.SOURCE_RESERVOIR, "Source=Reservoir");
		/// <summary>
		/// Water source is a direct flow right.
		/// </summary>
		public static readonly StateMod_OperationalRight_Metadata_RuleType SOURCE_DIRECT_FLOW_RIGHT = new StateMod_OperationalRight_Metadata_RuleType("SOURCE_DIRECT_FLOW_RIGHT", InnerEnum.SOURCE_DIRECT_FLOW_RIGHT, "Source=Direct Flow Right");
		/// <summary>
		/// Water source is a plan.
		/// </summary>
		public static readonly StateMod_OperationalRight_Metadata_RuleType SOURCE_PLAN_STRUCTURE = new StateMod_OperationalRight_Metadata_RuleType("SOURCE_PLAN_STRUCTURE", InnerEnum.SOURCE_PLAN_STRUCTURE, "Source=Plan Structure");
		/// <summary>
		/// Water source is groundwater.
		/// </summary>
		public static readonly StateMod_OperationalRight_Metadata_RuleType SOURCE_GROUNDWATER = new StateMod_OperationalRight_Metadata_RuleType("SOURCE_GROUNDWATER", InnerEnum.SOURCE_GROUNDWATER, "Source=Groundwater");
		/// <summary>
		/// Storage operations.
		/// </summary>
		public static readonly StateMod_OperationalRight_Metadata_RuleType STORAGE_OPERATIONS = new StateMod_OperationalRight_Metadata_RuleType("STORAGE_OPERATIONS", InnerEnum.STORAGE_OPERATIONS, "Storage Operations");
		/// <summary>
		/// Related to soil moisture.
		/// </summary>
		public static readonly StateMod_OperationalRight_Metadata_RuleType SOIL_MOISTURE = new StateMod_OperationalRight_Metadata_RuleType("SOIL_MOISTURE", InnerEnum.SOIL_MOISTURE, "Soil Moisture");
		/// <summary>
		/// Interstate compacts.
		/// </summary>
		public static readonly StateMod_OperationalRight_Metadata_RuleType COMPACT = new StateMod_OperationalRight_Metadata_RuleType("COMPACT", InnerEnum.COMPACT, "COMPACT");
		/// <summary>
		/// Other.
		/// </summary>
		public static readonly StateMod_OperationalRight_Metadata_RuleType OTHER = new StateMod_OperationalRight_Metadata_RuleType("OTHER", InnerEnum.OTHER, "Other");
		/// <summary>
		/// Not applicable (not used).
		/// </summary>
		public static readonly StateMod_OperationalRight_Metadata_RuleType NA = new StateMod_OperationalRight_Metadata_RuleType("NA", InnerEnum.NA, "NA");

		private static readonly IList<StateMod_OperationalRight_Metadata_RuleType> valueList = new List<StateMod_OperationalRight_Metadata_RuleType>();

		static StateMod_OperationalRight_Metadata_RuleType()
		{
			valueList.Add(SOURCE_RESERVOIR);
			valueList.Add(SOURCE_DIRECT_FLOW_RIGHT);
			valueList.Add(SOURCE_PLAN_STRUCTURE);
			valueList.Add(SOURCE_GROUNDWATER);
			valueList.Add(STORAGE_OPERATIONS);
			valueList.Add(SOIL_MOISTURE);
			valueList.Add(COMPACT);
			valueList.Add(OTHER);
			valueList.Add(NA);
		}

		public enum InnerEnum
		{
			SOURCE_RESERVOIR,
			SOURCE_DIRECT_FLOW_RIGHT,
			SOURCE_PLAN_STRUCTURE,
			SOURCE_GROUNDWATER,
			STORAGE_OPERATIONS,
			SOIL_MOISTURE,
			COMPACT,
			OTHER,
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
		private StateMod_OperationalRight_Metadata_RuleType(string name, InnerEnum innerEnum, string displayName)
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
	public static StateMod_OperationalRight_Metadata_RuleType valueOfIgnoreCase(string name)
	{
		StateMod_OperationalRight_Metadata_RuleType[] values = values();
		// Currently supported values
		foreach (StateMod_OperationalRight_Metadata_RuleType t in values)
		{
			if (name.Equals(t.ToString(), StringComparison.OrdinalIgnoreCase))
			{
				return t;
			}
		}
		return null;
	}


		public static IList<StateMod_OperationalRight_Metadata_RuleType> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static StateMod_OperationalRight_Metadata_RuleType valueOf(string name)
		{
			foreach (StateMod_OperationalRight_Metadata_RuleType enumInstance in StateMod_OperationalRight_Metadata_RuleType.valueList)
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