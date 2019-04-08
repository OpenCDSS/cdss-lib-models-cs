using System;

// StateCU_Supply - class to store water supply information

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
// StateCU_Supply - class to store water supply information
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
// 
// 2007-05-15  Steven A. Malers, RTi   Initial version.  Copy from
//                                     StateCU_Parcel.
//------------------------------------------------------------------------------
// EndHeader

namespace DWR.StateCU
{
	/// <summary>
	/// This class is not part of the core StateCU classes.  Instead, it is used with
	/// StateDMI to track the water supply for a parcel.  The data may ultimately be
	/// useful in StateCU and is definitely useful for data checks in StateDMI.  For
	/// groundwater only lands, the supply consists of well right/permit identifiers
	/// and decree/yield for amount.
	/// </summary>
	public class StateCU_Supply : StateCU_Data, ICloneable, IComparable<StateCU_Data>
	{

	// Base class has ID and name

	/// <summary>
	/// Supply amount (rate) associated with the supply, CFS for wells.
	/// </summary>
	private double __amount;

	// Indicate if a groundwater and/or surface water supply (likely only one but not both)
	private bool __is_ground = false;
	private bool __is_surface = false;

	/// <summary>
	/// Constructor.
	/// </summary>
	public StateCU_Supply() : base()
	{
		initialize();
	}

	/// <summary>
	/// Clones the data object. </summary>
	/// <returns> a cloned object. </returns>
	public override object clone()
	{
		StateCU_Supply supply = (StateCU_Supply)base.clone();
		supply._isClone = true;
		return supply;
	}

	/// <summary>
	/// Compares this object to another StateCU_Supply object based on the sorted
	/// order from the StateCU_Data variables, and then by amount. </summary>
	/// <param name="data"> the object to compare against. </param>
	/// <returns> 0 if they are the same, 1 if this object is greater than the other
	/// object, or -1 if it is less. </returns>
	public virtual int CompareTo(StateCU_Data data)
	{
		int res = base.CompareTo(data);
		if (res != 0)
		{
			return res;
		}

		StateCU_Supply supply = (StateCU_Supply)data;

		if (__amount < supply.__amount)
		{
			return -1;
		}
		else if (__amount > supply.__amount)
		{
			return 1;
		}

		return 0;
	}

	/// <summary>
	/// Creates a backup of the current data object and stores it in _original,
	/// for use in determining if an object was changed inside of a GUI.
	/// </summary>
	public virtual void createBackup()
	{
		_original = clone();
		((StateCU_Parcel)_original)._isClone = false;
		_isClone = true;
	}

	// TODO SAM 2007-05-15
	// Not sure if something like this is needed.
	/// <summary>
	/// Compare two rights Vectors and see if they are the same. </summary>
	/// <param name="v1"> the first Vector of StateMod_ReservoirAreaCap s to check.  Can not
	/// be null. </param>
	/// <param name="v2"> the second Vector of StateMod_ReservoirAreaCap s to check.  Can not
	/// be null. </param>
	/// <returns> true if they are the same, false if not. </returns>
	/*
	public static boolean equals(Vector v1, Vector v2) {
		String routine = "StateMod_ReservoirAreaCap.equals(Vector, Vector)";
		StateMod_ReservoirAreaCap r1;	
		StateMod_ReservoirAreaCap r2;	
		if (v1.size() != v2.size()) {
			Message.printStatus(1, routine, "Vectors are different sizes");
			return false;
		}
		else {
			// sort the Vectors and compare item-by-item.  Any differences
			// and data will need to be saved back into the dataset.
			int size = v1.size();
			Message.printStatus(1, routine, "Vectors are of size: " + size);
			Vector v1Sort = StateMod_Util.sortStateMod_DataVector(v1);
			Vector v2Sort = StateMod_Util.sortStateMod_DataVector(v2);
			Message.printStatus(1, routine, "Vectors have been sorted");
		
			for (int i = 0; i < size; i++) {			
				r1 = (StateMod_ReservoirAreaCap)v1Sort.elementAt(i);	
				r2 = (StateMod_ReservoirAreaCap)v2Sort.elementAt(i);	
				Message.printStatus(1, routine, r1.toString());
				Message.printStatus(1, routine, r2.toString());
				Message.printStatus(1, routine, "Element " + i 
					+ " comparison: " + r1.compareTo(r2));
				if (r1.compareTo(r2) != 0) {
					return false;
				}
			}
		}	
		return true;
	}
	*/

	/// <summary>
	/// Tests to see if two parcels equal.  Strings are compared with case sensitivity. </summary>
	/// <param name="supply"> The supply instance to compare. </param>
	/// <returns> true if they are equal, false otherwise. </returns>
	public virtual bool Equals(StateCU_Supply supply)
	{
		if (!base.Equals(supply))
		{
			 return false;
		}

		if ((__amount == supply.__amount) && (__is_ground == supply.__is_ground) && (__is_surface == supply.__is_surface))
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Clean up before garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~StateCU_Supply()
	{
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Returns the supply amount. </summary>
	/// <returns> the supply amount (CFS for wells). </returns>
	public virtual double getAmount()
	{
		return __amount;
	}

	/// <summary>
	/// Initializes member variables.
	/// </summary>
	private void initialize()
	{
		__amount = StateCU_Util.MISSING_DOUBLE;
	}

	/// <summary>
	/// Indicate whether the supply is a groundwater source. </summary>
	/// <returns> true if a groundwater supply, false if not. </returns>
	public virtual bool isGroundWater()
	{
		return __is_ground;
	}

	/// <summary>
	/// Indicate whether the supply is a surface water source. </summary>
	/// <returns> true if a surface water supply, false if not. </returns>
	public virtual bool isSurfaceWater()
	{
		return __is_surface;
	}

	/// <summary>
	/// Cancels any changes made to this object within a GUI since createBackup()
	/// was called and sets _original to null.
	/// </summary>
	public override void restoreOriginal()
	{
		StateCU_Supply supply = (StateCU_Supply)_original;
		base.restoreOriginal();

		__amount = supply.__amount;
		__is_ground = supply.__is_ground;
		__is_surface = supply.__is_surface;
		_isClone = false;
		_original = null;
	}

	/// <summary>
	/// Set the supply amount. </summary>
	/// <param name="amount"> the supply amount. </param>
	public virtual void setAmount(double amount)
	{
		if (amount != __amount)
		{
			/* REVISIT SAM 2006-04-09
			Supply is not currently part of the data set.
			if ( !_isClone && _dataset != null ) {
				_dataset.setDirty(_dataset.COMP_RESERVOIR_STATIONS,
				true);
			}
			*/
			__amount = amount;
		}
	}

	/// <summary>
	/// Set whether it is a groundwater supply. </summary>
	/// <param name="whether"> a groundwater supply. </param>
	public virtual void setIsGroundWater(bool is_ground)
	{
		if (is_ground != __is_ground)
		{
			/* REVISIT SAM 2006-04-09
			Supply is not currently part of the data set.
			if ( !_isClone && _dataset != null ) {
				_dataset.setDirty(_dataset.COMP_RESERVOIR_STATIONS,
				true);
			}
			*/
			__is_ground = is_ground;
		}
	}

	/// <summary>
	/// Set whether it is a surface water supply. </summary>
	/// <param name="whether"> a surface water supply. </param>
	public virtual void setIsSurfaceWater(bool is_surface)
	{
		if (is_surface != __is_surface)
		{
			/* REVISIT SAM 2006-04-09
			Supply is not currently part of the data set.
			if ( !_isClone && _dataset != null ) {
				_dataset.setDirty(_dataset.COMP_RESERVOIR_STATIONS,
				true);
			}
			*/
			__is_surface = is_surface;
		}
	}

	/// <summary>
	/// Returns a String representation of this object. </summary>
	/// <returns> a String representation of this object. </returns>
	public override string ToString()
	{
		return base.ToString() + ", " + _id + ", " + __amount + ", " + __is_ground +
		", " + __is_surface;
	}

	}

}