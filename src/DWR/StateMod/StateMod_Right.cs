// StateMod_Right - interface defining behavior for a StateMod water right

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
	/// This interface describes general behavior for a StateMod water right,
	/// to allow generalized handling of water rights.
	/// </summary>
	public interface StateMod_Right
	{

	/// <summary>
	/// Return the administration number. </summary>
	/// <returns> the administration number as a String, to preserve exact
	/// precision. </returns>
	string getAdministrationNumber();

	/// <summary>
	/// Return the decree amount. </summary>
	/// <returns> the water right decree amount, in units for the data. </returns>
	double getDecree();

	/// <summary>
	/// Return the units for the decree;
	/// </summary>
	string getDecreeUnits();

	/// <summary>
	/// Return the right identifier. </summary>
	/// <returns> the right identifier. </returns>
	string getIdentifier();

	/// <summary>
	/// Return the right location identifier. </summary>
	/// <returns> the right location identifier. </returns>
	string getLocationIdentifier();

	/// <summary>
	/// Return the right name. </summary>
	/// <returns> the right name. </returns>
	string getName();

	/// <summary>
	/// Return the on/off switch. </summary>
	/// <returns> the on/off switch. </returns>
	int getSwitch();

	/// <summary>
	/// Set the decree amount.
	/// </summary>
	void setDecree(double decree);

	}

}