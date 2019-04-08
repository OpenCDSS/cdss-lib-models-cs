using System;
using System.Collections.Generic;

// StateMod_NodeNetwork_AppendHowType - enumeration to store values for how a network can be appended to another.

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
	/// This enumeration stores values for how a network can be appended to another.
	/// </summary>
	public sealed class StateMod_NodeNetwork_AppendHowType
	{
		/// <summary>
		/// Add the appended network upstream of the existing downstream node (leaving existing nodes upstream
		/// of the existing downstream node).
		/// </summary>
		public static readonly StateMod_NodeNetwork_AppendHowType ADD_UPSTREAM_OF_DOWNSTREAM = new StateMod_NodeNetwork_AppendHowType("ADD_UPSTREAM_OF_DOWNSTREAM", InnerEnum.ADD_UPSTREAM_OF_DOWNSTREAM, "AddUpstreamOfDownstream");
		/// <summary>
		/// Replace the downstream node with the upstream appended network.
		/// </summary>
		// TODO SAM 2011-01-05 Enable this later
		//REPLACE_DOWNSTREAM_WITH_UPSTREAM("ReplaceDownstreamWithUpstream"),
		/// <summary>
		/// Add the appended network upstream of the existing downstream node (replacing all nodes upstream
		/// of the existing downstream node).
		/// </summary>
		public static readonly StateMod_NodeNetwork_AppendHowType REPLACE_UPSTREAM_OF_DOWNSTREAM = new StateMod_NodeNetwork_AppendHowType("REPLACE_UPSTREAM_OF_DOWNSTREAM", InnerEnum.REPLACE_UPSTREAM_OF_DOWNSTREAM, "ReplaceUpstreamOfDownstream");
		public static readonly StateMod_NodeNetwork_AppendHowType  = new StateMod_NodeNetwork_AppendHowType("", InnerEnum.);

		private static readonly IList<StateMod_NodeNetwork_AppendHowType> valueList = new List<StateMod_NodeNetwork_AppendHowType>();

		static StateMod_NodeNetwork_AppendHowType()
		{
			valueList.Add(ADD_UPSTREAM_OF_DOWNSTREAM);
			valueList.Add(REPLACE_UPSTREAM_OF_DOWNSTREAM);
			valueList.Add();
		}

		public enum InnerEnum
		{
			ADD_UPSTREAM_OF_DOWNSTREAM,
			REPLACE_UPSTREAM_OF_DOWNSTREAM,
            
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
		/// Construct an enumeration value. </summary>
		/// <param name="displayName"> name that should be displayed in choices, etc. </param>
		private StateMod_NodeNetwork_AppendHowType(string name, InnerEnum innerEnum, string displayName)
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
	public static StateMod_NodeNetwork_AppendHowType valueOfIgnoreCase(string name)
	{
		if (string.ReferenceEquals(name, null))
		{
			return null;
		}
		StateMod_NodeNetwork_AppendHowType[] values = values();
		// Currently supported values
		foreach (StateMod_NodeNetwork_AppendHowType t in values)
		{
			if (name.Equals(t.ToString(), StringComparison.OrdinalIgnoreCase))
			{
				return t;
			}
		}
		return null;
	}


		public static IList<StateMod_NodeNetwork_AppendHowType> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static StateMod_NodeNetwork_AppendHowType valueOf(string name)
		{
			foreach (StateMod_NodeNetwork_AppendHowType enumInstance in StateMod_NodeNetwork_AppendHowType.valueList)
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