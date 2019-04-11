using System;

// StateMod_Parcel - class to store parcel information

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
// StateMod_Parcel - class to store parcel information
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
// 
// 2006-04-09	Steven A. Malers, RTi	Initial version.
// 2006-04-12	SAM, RTi		Add well count and area data.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
//------------------------------------------------------------------------------
// EndHeader

namespace DWR.StateMod
{
	/// <summary>
	/// This class is not part of the core StateMod classes.  Instead, it is used with
	/// StateDMI to track whether a well or diversion station have parcels.  This
	/// approach will be evaluated.
	/// </summary>
	public class StateMod_Parcel : StateMod_Data //, ICloneable, IComparable<StateMod_Data>
	{

	// Base class has ID import and name (not important)

	/// <summary>
	/// Content in area cap table.
	/// </summary>
	protected internal string _crop;

	/// <summary>
	/// Area associated with the parcel, acres.
	/// </summary>
	protected internal double _area;

	/// <summary>
	/// Year for the data.
	/// </summary>
	protected internal int _year;

	/// <summary>
	/// Count of wells on the parcel.
	/// </summary>
	protected internal int _well_count;

	/// <summary>
	/// Constructor.
	/// </summary>
	public StateMod_Parcel() : base()
	{
		initialize();
	}

	/// <summary>
	/// Clones the data object. </summary>
	/// <returns> a cloned object. </returns>
	//public override object clone()
	//{
	//	StateMod_Parcel parcel = (StateMod_Parcel)base.clone();
	//	parcel._isClone = true;
	//	return parcel;
	//}

	/// <summary>
	/// Compares this object to another StateMod_Data object based on the sorted
	/// order from the StateMod_Data variables, and then by crop, area, and year,
	/// in that order. </summary>
	/// <param name="data"> the object to compare against. </param>
	/// <returns> 0 if they are the same, 1 if this object is greater than the other
	/// object, or -1 if it is less. </returns>
	//public virtual int CompareTo(StateMod_Data data)
	//{
	//	int res = base.CompareTo(data);
	//	if (res != 0)
	//	{
	//		return res;
	//	}

	//	StateMod_Parcel parcel = (StateMod_Parcel)data;

	//	res = _crop.CompareTo(parcel.getCrop());
	//	if (res != 0)
	//	{
	//		return res;
	//	}

	//	if (_area < parcel._area)
	//	{
	//		return -1;
	//	}
	//	else if (_area > parcel._area)
	//	{
	//		return 1;
	//	}

	//	if (_year < parcel._year)
	//	{
	//		return -1;
	//	}
	//	else if (_year > parcel._year)
	//	{
	//		return 1;
	//	}

	//	return 0;
	//}

	/// <summary>
	/// Creates a backup of the current data object and stores it in _original,
	/// for use in determining if an object was changed inside of a GUI.
	/// </summary>
	//public virtual void createBackup()
	//{
	//	_original = (StateMod_Parcel)clone();
	//	((StateMod_Parcel)_original)._isClone = false;
	//	_isClone = true;
	//}

	// REVISIT SAM 2006-04-09
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
	/// <param name="ac"> the ac to compare. </param>
	/// <returns> true if they are equal, false otherwise. </returns>
	public virtual bool Equals(StateMod_Parcel parcel)
	{
		if (!base.Equals(parcel))
		{
			 return false;
		}

		if (_crop.Equals(parcel._crop) && (_area == parcel._area) && (_year == parcel._year))
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
	~StateMod_Parcel()
	{
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
		_crop = null;
	}

	/// <summary>
	/// Returns the area for the crop (acres). </summary>
	/// <returns> the area for the crop (acres). </returns>
	public virtual double getArea()
	{
		return _area;
	}

	/// <summary>
	/// Returns the crop. </summary>
	/// <returns> the crop. </returns>
	public virtual string getCrop()
	{
		return _crop;
	}

	/// <summary>
	/// Returns the number of wells associated with the parcel. </summary>
	/// <returns> the number of wells associated with the parcel. </returns>
	public virtual int getWellCount()
	{
		return _well_count;
	}
	/// <summary>
	/// Returns the year for the crop. </summary>
	/// <returns> the year for the crop. </returns>
	public virtual int getYear()
	{
		return _year;
	}

	/// <summary>
	/// Initializes member variables.
	/// </summary>
	private void initialize()
	{
		_crop = "";
		_area = StateMod_Util.MISSING_DOUBLE;
		_year = StateMod_Util.MISSING_INT;
	}

	/// <summary>
	/// Cancels any changes made to this object within a GUI since createBackup()
	/// was called and sets _original to null.
	/// </summary>
	//public override void restoreOriginal()
	//{
	//	StateMod_Parcel parcel = (StateMod_Parcel)_original;
	//	base.restoreOriginal();

	//	_crop = parcel._crop;
	//	_area = parcel._area;
	//	_year = parcel._year;
	//	_isClone = false;
	//	_original = null;
	//}

	/// <summary>
	/// Set the crop area. </summary>
	/// <param name="area"> area to set. </param>
	public virtual void setArea(double area)
	{
		if (area != _area)
		{
			/* REVISIT SAM 2006-04-09
			Parcels are not currently part of the data set.
			if ( !_isClone && _dataset != null ) {
				_dataset.setDirty(_dataset.COMP_RESERVOIR_STATIONS,
				true);
			}
			*/
			_area = area;
		}
	}

	/// <summary>
	/// Set the crop. </summary>
	/// <param name="crop"> Crop to set. </param>
	public virtual void setCrop(string crop)
	{
		if (!crop.Equals(_crop, StringComparison.OrdinalIgnoreCase))
		{
			/* REVISIT SAM 2006-04-09
			parcels are not currently part of StateMod data set.
			if ( !_isClone && _dataset != null ) {
				_dataset.setDirty(_dataset.COMP_RESERVOIR_STATIONS,
				true);
			}
			*/
			_crop = crop;
		}
	}

	/// <summary>
	/// Set the number of wells assoiated with the parcel. </summary>
	/// <param name="well_count"> Number of wells associated with the parcel. </param>
	public virtual void setWellCount(int well_count)
	{
		if (well_count != _well_count)
		{
			/* REVISIT SAM 2006-04-09
			parcels are not currently part of StateMod data set.
			if ( !_isClone && _dataset != null ) {
				_dataset.setDirty(_dataset.COMP_RESERVOIR_STATIONS,
				true);
			}
			*/
			_well_count = well_count;
		}
	}

	/// <summary>
	/// Set the year associated with the crop. </summary>
	/// <param name="year"> Year to set. </param>
	public virtual void setYear(int year)
	{
		if (year != _year)
		{
			/* REVISIT SAM 2006-04-09
			parcels are not currently part of StateMod data set.
			if ( !_isClone && _dataset != null ) {
				_dataset.setDirty(_dataset.COMP_RESERVOIR_STATIONS,
				true);
			}
			*/
			_year = year;
		}
	}

	/// <summary>
	/// Returns a String representation of this object. </summary>
	/// <returns> a String representation of this object. </returns>
	public override string ToString()
	{
		return base.ToString() + ", " + _crop + ", " + _area + ", "
			+ _year;
	}

	}

}