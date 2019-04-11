using System;
using System.Collections.Generic;

// StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType - enumeration that stores values
// for whether an operational right allows an associated plan,

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
	/// This enumeration stores values for whether an operational right allows an associated plan,
	/// which can be used to perform checks and visualization.
	/// </summary>
	public sealed class StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType
	{
		/// <summary>
		/// Not applicable (associated plan not allowed).
		/// </summary>
		public static readonly StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType NA = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType("NA", InnerEnum.NA, "NA");
		/// <summary>
		/// Reuse plans
		/// </summary>
		public static readonly StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType PLAN_ACCOUNTING = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType("PLAN_ACCOUNTING", InnerEnum.PLAN_ACCOUNTING, "Plan (Accounting)");
		public static readonly StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType PLAN_OUT_OF_PRIORITY = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType("PLAN_OUT_OF_PRIORITY", InnerEnum.PLAN_OUT_OF_PRIORITY, "Plan (Out of Priority Diversion or Storage)");
		public static readonly StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType PLAN_RECHARGE = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType("PLAN_RECHARGE", InnerEnum.PLAN_RECHARGE, "Plan (Recharge)");
		public static readonly StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType PLAN_RELEASE_LIMIT = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType("PLAN_RELEASE_LIMIT", InnerEnum.PLAN_RELEASE_LIMIT, "Plan (Release Limit)");
		public static readonly StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType PLAN_REUSE_TO_RESERVOIR = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType("PLAN_REUSE_TO_RESERVOIR", InnerEnum.PLAN_REUSE_TO_RESERVOIR, "Plan (Reuse to Reservoir)");
		public static readonly StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType PLAN_REUSE_TO_RESERVOIR_FROM_TRANSMOUNTAIN = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType("PLAN_REUSE_TO_RESERVOIR_FROM_TRANSMOUNTAIN", InnerEnum.PLAN_REUSE_TO_RESERVOIR_FROM_TRANSMOUNTAIN, "Plan (Reuse to Diversion from Transmountain)");
		public static readonly StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType PLAN_REUSE_TO_DIVERSION = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType("PLAN_REUSE_TO_DIVERSION", InnerEnum.PLAN_REUSE_TO_DIVERSION, "Plan (Reuse to Diversion)");
		public static readonly StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType PLAN_REUSE_TO_DIVERSION_FROM_TRANSMOUNTAIN = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType("PLAN_REUSE_TO_DIVERSION_FROM_TRANSMOUNTAIN", InnerEnum.PLAN_REUSE_TO_DIVERSION_FROM_TRANSMOUNTAIN, "Plan (Reuse to Diversion from Transmountain)");
		public static readonly StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType PLAN_SPECIAL_WELL_AUGMENTATION = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType("PLAN_SPECIAL_WELL_AUGMENTATION", InnerEnum.PLAN_SPECIAL_WELL_AUGMENTATION, "Plan (Special Well Augmentation)");
		public static readonly StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType PLAN_TC = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType("PLAN_TC", InnerEnum.PLAN_TC, "Plan (Terms & Conditions)");
		public static readonly StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType PLAN_TRANSMOUNTAIN_IMPORT = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType("PLAN_TRANSMOUNTAIN_IMPORT", InnerEnum.PLAN_TRANSMOUNTAIN_IMPORT, "Plan (Transmountain Import)");
		public static readonly StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType PLAN_WELL_AUGMENTATION = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType("PLAN_WELL_AUGMENTATION", InnerEnum.PLAN_WELL_AUGMENTATION, "Plan (Well Augmentation)");

		private static readonly IList<StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType> valueList = new List<StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType>();

		static StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType()
		{
			valueList.Add(NA);
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
		}

		public enum InnerEnum
		{
			NA,
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
			PLAN_WELL_AUGMENTATION
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
		private StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType(string name, InnerEnum innerEnum, string displayName)
		{
			this.displayName = displayName;

			nameValue = name;
			ordinalValue = nextOrdinal++;
			innerEnumValue = innerEnum;
		}

	/// <summary>
	/// Lookup the source or destination type from a plan type. </summary>
	/// <returns> the matching source or destination type, or null if not matched. </returns>
	//public StateMod_OperationalRight_Metadata_SourceOrDestinationType getMatchingSourceOrDestinationType()
	//{
	//	switch (this)
	//	{
	//		// List in StateMod documentation order
	//		case PLAN_TC:
	//			return StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_TC;
	//		case PLAN_WELL_AUGMENTATION:
	//			return StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_WELL_AUGMENTATION;
	//		case PLAN_REUSE_TO_RESERVOIR:
	//			return StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_REUSE_TO_RESERVOIR;
	//		case PLAN_REUSE_TO_DIVERSION:
	//			return StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_REUSE_TO_DIVERSION;
	//		case PLAN_REUSE_TO_RESERVOIR_FROM_TRANSMOUNTAIN:
	//			return StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_REUSE_TO_RESERVOIR_FROM_TRANSMOUNTAIN;
	//		case PLAN_REUSE_TO_DIVERSION_FROM_TRANSMOUNTAIN:
	//			return StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_REUSE_TO_DIVERSION_FROM_TRANSMOUNTAIN;
	//		case PLAN_TRANSMOUNTAIN_IMPORT:
	//			return StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_TRANSMOUNTAIN_IMPORT;
	//		case PLAN_RECHARGE:
	//			return StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_RECHARGE;
	//		case PLAN_OUT_OF_PRIORITY:
	//			return StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_OUT_OF_PRIORITY;
	//		case PLAN_SPECIAL_WELL_AUGMENTATION:
	//			return StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_SPECIAL_WELL_AUGMENTATION;
	//		case PLAN_ACCOUNTING:
	//			return StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_ACCOUNTING;
	//		case PLAN_RELEASE_LIMIT:
	//			return StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_RELEASE_LIMIT;
	//	}
	//	// No match
	//	return null;
	//}

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
	//public static StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType valueOfIgnoreCase(string name)
	//{
	//	StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] values = values();
	//	// Currently supported values
	//	foreach (StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType t in values)
	//	{
	//		if (name.Equals(t.ToString(), StringComparison.OrdinalIgnoreCase))
	//		{
	//			return t;
	//		}
	//	}
	//	return null;
	//}


		public static IList<StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType valueOf(string name)
		{
			foreach (StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType enumInstance in StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.valueList)
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