// StateMod_Right_Comparator - This class compares StateMod_Right instances to allow for sorting.

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
	/// This class compares StateMod_Right instances to allow for sorting.
	/// </summary>
	public class StateMod_Right_Comparator : System.Collections.IComparer
	{

	/// <summary>
	/// Sort field and order - sort right ID in ascending order.
	/// </summary>
	public const int IDAscending = 0;

	/// <summary>
	/// Sort field and order - sort location ID in ascending order.
	/// </summary>
	public const int LocationIDAscending = 1;

	private int __order = IDAscending;
	private int __order2 = -1;

	/// <summary>
	/// Compare two StateMod_Right according to the orders that have been set.
	/// See setOrder() and setOrder2().
	/// </summary>
	public virtual int Compare(object o1, object o2)
	{
		StateMod_Right right1 = (StateMod_Right)o1;
		StateMod_Right right2 = (StateMod_Right)o2;
		string id1 = right1.getIdentifier();
		string id2 = right2.getIdentifier();
		string locid1 = right1.getLocationIdentifier();
		string locid2 = right2.getLocationIdentifier();
		int compare_result = 0; // Result of compare
		// Do the first level check...
		if (__order == IDAscending)
		{
			compare_result = id1.CompareTo(id2);
		}
		else if (__order == LocationIDAscending)
		{
			compare_result = locid1.CompareTo(locid2);
		}
		if (compare_result != 0)
		{
			return compare_result;
		}
		// Do the second level check...
		if (__order2 >= 0)
		{
			if (__order2 == IDAscending)
			{
				compare_result = id1.CompareTo(id2);
			}
			else if (__order2 == LocationIDAscending)
			{
				compare_result = locid1.CompareTo(locid2);
			}
			if (compare_result != 0)
			{
				return compare_result;
			}
		}
		// Do the third level check (always sort by administration number)...
		string adminnum1 = right1.getAdministrationNumber();
		string adminnum2 = right2.getAdministrationNumber();
		compare_result = adminnum1.CompareTo(adminnum2);
		if (compare_result != 0)
		{
			return compare_result;
		}
		// Do the fourth level check (always sort by decree)...
		double decree1 = right1.getDecree();
		double decree2 = right2.getDecree();
		if (decree1 < decree2)
		{
			return -1;
		}
		else if (decree1 > decree2)
		{
			return 1;
		}
		else
		{
			return 0;
		}
	}

	/// <summary>
	/// Set the first sort order criteria for comparison.
	/// </summary>
	public virtual void setOrder(int order)
	{
		__order = order;
	}

	/// <summary>
	/// Set the second sort order criteria for comparison.
	/// </summary>
	public virtual void setOrder2(int order)
	{
		__order2 = order;
	}

	}

}