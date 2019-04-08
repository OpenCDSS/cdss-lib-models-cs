using System;
using System.Collections.Generic;

// StateMod_OperationalRight_Metadata_Source2AllowedType - enumeration to store values for
// whether an operational right "source 2" is allowed, required, or not applicable

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
	/// This enumeration stores values for whether an operational right "source 2" is allowed, required, or not
	/// applicable, which can be used to perform checks and visualization.
	/// </summary>
	public sealed class StateMod_OperationalRight_Metadata_Source2AllowedType
	{
		/// <summary>
		/// Allowed.
		/// </summary>
		public static readonly StateMod_OperationalRight_Metadata_Source2AllowedType ALLOWED = new StateMod_OperationalRight_Metadata_Source2AllowedType("ALLOWED", InnerEnum.ALLOWED, "Allowed");
		/// <summary>
		/// Not applicable.
		/// </summary>
		public static readonly StateMod_OperationalRight_Metadata_Source2AllowedType NA = new StateMod_OperationalRight_Metadata_Source2AllowedType("NA", InnerEnum.NA, "NA");
		/// <summary>
		/// Required.
		/// </summary>
		public static readonly StateMod_OperationalRight_Metadata_Source2AllowedType REQUIRED = new StateMod_OperationalRight_Metadata_Source2AllowedType("REQUIRED", InnerEnum.REQUIRED, "Required");

		private static readonly IList<StateMod_OperationalRight_Metadata_Source2AllowedType> valueList = new List<StateMod_OperationalRight_Metadata_Source2AllowedType>();

		static StateMod_OperationalRight_Metadata_Source2AllowedType()
		{
			valueList.Add(ALLOWED);
			valueList.Add(NA);
			valueList.Add(REQUIRED);
		}

		public enum InnerEnum
		{
			ALLOWED,
			NA,
			REQUIRED
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
		private StateMod_OperationalRight_Metadata_Source2AllowedType(string name, InnerEnum innerEnum, string displayName)
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
	public static StateMod_OperationalRight_Metadata_Source2AllowedType valueOfIgnoreCase(string name)
	{
		StateMod_OperationalRight_Metadata_Source2AllowedType[] values = values();
		// Currently supported values
		foreach (StateMod_OperationalRight_Metadata_Source2AllowedType t in values)
		{
			if (name.Equals(t.ToString(), StringComparison.OrdinalIgnoreCase))
			{
				return t;
			}
		}
		return null;
	}


		public static IList<StateMod_OperationalRight_Metadata_Source2AllowedType> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static StateMod_OperationalRight_Metadata_Source2AllowedType valueOf(string name)
		{
			foreach (StateMod_OperationalRight_Metadata_Source2AllowedType enumInstance in StateMod_OperationalRight_Metadata_Source2AllowedType.valueList)
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