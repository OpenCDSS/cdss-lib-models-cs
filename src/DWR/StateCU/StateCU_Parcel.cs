using System;
using System.Collections.Generic;

// StateCU_Parcel - used with StateDMI to track whether a CU Location has parcels

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

namespace DWR.StateCU
{

	/// <summary>
	/// This class is not part of the core StateCU classes.  Instead, it is used with
	/// StateDMI to track whether a CU Location has parcels.  The data may ultimately be
	/// useful in StateCU and is definitely useful for data checks in StateDMI.
	/// </summary>
	public class StateCU_Parcel : StateCU_Data, ICloneable, IComparable<StateCU_Data>
	{

	// Base class has ID and name (not useful and same as ID)

	/// <summary>
	/// Crop name.
	/// </summary>
	private string __crop;

	/// <summary>
	/// Area associated with the parcel, acres (should reflect percent_irrig from HydroBase).
	/// </summary>
	private double __area;

	/// <summary>
	/// Area units.
	/// </summary>
	private string __area_units;

	/// <summary>
	/// Year for the data.
	/// </summary>
	private int __year;

	/// <summary>
	/// Irrigation method.
	/// </summary>
	private string __irrigation_method;

	/// <summary>
	/// Water supply sources - initialize so non-null.
	/// </summary>
	private IList<StateCU_Supply> __supply_List = new List<StateCU_Supply>();

	/// <summary>
	/// Constructor.
	/// </summary>
	public StateCU_Parcel() : base()
	{
		initialize();
	}

	/// <summary>
	/// Add a supply object.
	/// </summary>
	public virtual void addSupply(StateCU_Supply supply)
	{
		__supply_List.Add(supply);
	}

	/// <summary>
	/// Clones the data object. </summary>
	/// <returns> a cloned object. </returns>
	public override object clone()
	{
		StateCU_Parcel parcel = (StateCU_Parcel)base.clone();
		parcel._isClone = true;
		return parcel;
	}

	/// <summary>
	/// Compares this object to another StateCU_Data object based on the sorted order from the StateCU_Data
	/// variables, and then by crop, irrigation method, area, and year,in that order. </summary>
	/// <param name="data"> the object to compare against (should be a StateCU_Parcel). </param>
	/// <returns> 0 if they are the same, 1 if this object is greater than the other object, or -1 if it is less. </returns>
	public virtual int CompareTo(StateCU_Data data)
	{
		int res = base.CompareTo(data);
		if (res != 0)
		{
			return res;
		}

		StateCU_Parcel parcel = (StateCU_Parcel)data;
		res = __crop.CompareTo(parcel.getCrop());
		if (res != 0)
		{
			return res;
		}

		res = __irrigation_method.CompareTo(parcel.getIrrigationMethod());
		if (res != 0)
		{
			return res;
		}

		if (__area < parcel.__area)
		{
			return -1;
		}
		else if (__area > parcel.__area)
		{
			return 1;
		}

		if (__year < parcel.__year)
		{
			return -1;
		}
		else if (__year > parcel.__year)
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
	/// <param name="v1"> the first Vector of StateMod_ReservoirAreaCap s to check.  Cannot be null. </param>
	/// <param name="v2"> the second Vector of StateMod_ReservoirAreaCap s to check.  Cannot be null. </param>
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
	/// <param name="ac"> the ac to compare. </param>
	/// <returns> true if they are equal, false otherwise. </returns>
	public virtual bool Equals(StateCU_Parcel parcel)
	{
		if (!base.Equals(parcel))
		{
			 return false;
		}

		if (__crop.Equals(parcel.__crop) && __irrigation_method.Equals(parcel.__irrigation_method, StringComparison.OrdinalIgnoreCase) && (__area == parcel.__area) && __area_units.Equals(parcel.__area_units, StringComparison.OrdinalIgnoreCase) && (__year == parcel.__year))
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
	~StateCU_Parcel()
	{
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
		__crop = null;
		__irrigation_method = null;
	}

	/// <summary>
	/// Returns the area for the crop (acres). </summary>
	/// <returns> the area for the crop (acres). </returns>
	public virtual double getArea()
	{
		return __area;
	}

	/// <summary>
	/// Returns the area units for the crop. </summary>
	/// <returns> the area units for the crop. </returns>
	public virtual string getAreaUnits()
	{
		return __area_units;
	}

	/// <summary>
	/// Returns the crop. </summary>
	/// <returns> the crop. </returns>
	public virtual string getCrop()
	{
		return __crop;
	}

	/// <summary>
	/// Returns the irrigation method. </summary>
	/// <returns> the irrigation method. </returns>
	public virtual string getIrrigationMethod()
	{
		return __irrigation_method;
	}

	/// <summary>
	/// Return the list of StateCU_Supply for the parcel. </summary>
	/// <returns> the list of StateCU_Supply for the parcel. </returns>
	public virtual IList<StateCU_Supply> getSupplyList()
	{
		return __supply_List;
	}

	/// <summary>
	/// Returns the number of wells associated with the parcel. </summary>
	/// <returns> the number of wells associated with the parcel. </returns>
	/*
	public int getWellCount() {
		return _well_count;
	}
	*/

	/// <summary>
	/// Returns the year for the crop. </summary>
	/// <returns> the year for the crop. </returns>
	public virtual int getYear()
	{
		return __year;
	}

	/// <summary>
	/// Indicate whether the parcel has groundwater supply.  This will be true if
	/// any of the StateCU_Supply associated with the parcel return isGroundWater as true.
	/// </summary>
	public virtual bool hasGroundWaterSupply()
	{
		int size = __supply_List.Count;
		StateCU_Supply supply = null;
		for (int i = 0; i < size; i++)
		{
			supply = (StateCU_Supply)__supply_List[i];
			if (supply.isGroundWater())
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Initializes member variables.
	/// </summary>
	private void initialize()
	{
		__crop = "";
		__irrigation_method = "";
		__area = StateCU_Util.MISSING_DOUBLE;
		__area_units = "";
		__year = StateCU_Util.MISSING_INT;
	}

	/// <summary>
	/// Cancels any changes made to this object within a GUI since createBackup()
	/// was called and sets _original to null.
	/// </summary>
	public override void restoreOriginal()
	{
		StateCU_Parcel parcel = (StateCU_Parcel)_original;
		base.restoreOriginal();

		__crop = parcel.__crop;
		__irrigation_method = parcel.__irrigation_method;
		__area = parcel.__area;
		__area_units = parcel.__area_units;
		__year = parcel.__year;
		_isClone = false;
		_original = null;
	}

	/// <summary>
	/// Set the crop area. </summary>
	/// <param name="area"> area to set. </param>
	public virtual void setArea(double area)
	{
		if (area != __area)
		{
			/* TODO SAM 2006-04-09 Parcels are not currently part of the data set.
			if ( !_isClone && _dataset != null ) {
				_dataset.setDirty(_dataset.COMP_RESERVOIR_STATIONS,
				true);
			}
			*/
			__area = area;
		}
	}

	/// <summary>
	/// Set the area units. </summary>
	/// <param name="area_units"> Area units to set. </param>
	public virtual void setAreaUnits(string area_units)
	{
		if (!area_units.Equals(__area_units, StringComparison.OrdinalIgnoreCase))
		{
			/* TODO SAM 2006-04-09 parcels are not currently part of StateMod data set.
			if ( !_isClone && _dataset != null ) {
				_dataset.setDirty(_dataset.COMP_RESERVOIR_STATIONS,
				true);
			}
			*/
			__area_units = area_units;
		}
	}

	/// <summary>
	/// Set the crop. </summary>
	/// <param name="crop"> Crop to set. </param>
	public virtual void setCrop(string crop)
	{
		if (!crop.Equals(__crop, StringComparison.OrdinalIgnoreCase))
		{
			/* TODO SAM 2006-04-09 parcels are not currently part of StateMod data set.
			if ( !_isClone && _dataset != null ) {
				_dataset.setDirty(_dataset.COMP_RESERVOIR_STATIONS,
				true);
			}
			*/
			__crop = crop;
		}
	}

	/// <summary>
	/// Set the irrigation method. </summary>
	/// <param name="irrigation_method"> Irrigation method to set. </param>
	public virtual void setIrrigationMethod(string irrigation_method)
	{
		if (!irrigation_method.Equals(__irrigation_method, StringComparison.OrdinalIgnoreCase))
		{
			/* TODO SAM 2006-04-09 parcels are not currently part of StateMod data set.
			if ( !_isClone && _dataset != null ) {
				_dataset.setDirty(_dataset.COMP_RESERVOIR_STATIONS,
				true);
			}
			*/
			__irrigation_method = irrigation_method;
		}
	}

	/// <summary>
	/// Set the number of wells associated with the parcel. </summary>
	/// <param name="well_count"> Number of wells associated with the parcel. </param>
	/*
	public void setWellCount(int well_count) {
		if ( well_count != _well_count) {
			/ * REVISIT SAM 2006-04-09
			parcels are not currently part of StateMod data set.
			if ( !_isClone && _dataset != null ) {
				_dataset.setDirty(_dataset.COMP_RESERVOIR_STATIONS,
				true);
			}
			* /
			_well_count = well_count;
		}
	}
	*/

	/// <summary>
	/// Set the year associated with the crop. </summary>
	/// <param name="year"> Year to set. </param>
	public virtual void setYear(int year)
	{
		if (year != __year)
		{
			/* TODO SAM 2006-04-09 parcels are not currently part of StateMod data set.
			if ( !_isClone && _dataset != null ) {
				_dataset.setDirty(_dataset.COMP_RESERVOIR_STATIONS,
				true);
			}
			*/
			__year = year;
		}
	}

	/// <summary>
	/// Returns a String representation of this object. </summary>
	/// <returns> a String representation of this object. </returns>
	public override string ToString()
	{
		return base.ToString() + ", " + __crop + ", " + __irrigation_method + ", " + __area + ", " + __area_units + ", " + __year;
	}

	}

}