using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

// StateMod_Reservoir - class for reservoir station data

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
// StateMod_Reservoir - class derived from StateMod_Data.  Contains information 
//	read from the reservoir file.
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
// 
// 27 Aug 1997	Catherine E.		Created initial version of class.
//		Nutting-Lane, RTi
// 21 Dec 1998	CEN, RTi		Added throws IOException to read/write
//					routines.
// 25 Oct 1999	CEN, RTi		Adding daily id.
// 10 Nov 1999	CEN, RTi		Added time series pointers and set/get
//					routines for ts similar to diversions
// 19 Feb 2001	Steven A. Malers, RTi	Code review.  Clean up javadoc.  Add
//					finalize.  Handle nulls and set unused
//					variables to null.  Alphabetize methods.
//					Change IO to IOUtil.  Add a switch to
//					allow printing with or without daily
//					data.  Update output header.  Change
//					area capacity output to be printed with
//					numbers 1... rather than 0...
// 02 Mar 2001	SAM, RTi		Change reservoir curve number back to
//					start with zero but make sure the
//					zero record has zeros all the way
//					across to allow interpolation.
//					Add insertAreaCapAt()to allow
//					insertion.
// 22 Aug 2001	SAM, RTi		When writing the reservoir area/capacity
//					file, if the area or capacity is less
//					than 100, write to 2 decimal points.
//					Otherwise, write as before.  This
//					prevents StateMod from complaining.
//					Print counter for curve to 3 digits.
// 2001-12-27	SAM, RTi		Update to use new fixedRead()to
//					improve performance.
// 2002-09-09	SAM, RTi		Add GeoRecord reference to allow 2-way
//					connection between spatial and StateMod
//					data.
// 2002-09-19	SAM, RTi		Use isDirty() instead of setDirty() to
//					indicate edits.
// 2002-11-01	SAM, RTi		Minor revision to add description of
//					first line to the output header.
//------------------------------------------------------------------------------
// 2003-06-04	J. Thomas Sapienza	Renamed from SMReservoir to 
//					StateMod_Reservoir
// 2003-06-10	JTS, RTi		* Folded dumpReservoirFile() into
//					  writeReservoirFile()
//					* Renamed parseReservoirFile() to
//					  readReservoirFile()
// 2003-06-23	JTS, RTi		Renamed writeReservoirFile() to
//					writeStateModFile()
// 2003-06-26	JTS, RTi		Renamed readReservoirFile() to
//					readStateModFile()
// 2003-07-15	JTS, RTi		Changed to use new dataset design.
// 2003-08-03	SAM, RTi		Changed isDirty() back to setDirty().
// 2003-08-15	SAM, RTi		* Changed GeoRecordNoSwing() to
//					  GeoRecord.
//					* Change StateMod_Climate to
//					  StateMod_ReservoirClimate.
// 2003-08-18	SAM, RTi		* Clean up the data members and method
//					  names for time series - daily were
//					  not being properly handled.
//					* Add hasXXXTS() to indicate whether the
//					  reservoir has climate data time
//					  series.
//					* Add getXXXTS() to get climate time
//					  series.
//					* Change so StateMod_ReservoirClimate
//					  uses the StateMod_Data base class for
//					  the identifiers.
// 2003-08-28	SAM, RTi		* Change water rights to be a simple
//					  Vector, not a linked list.
//					* Call setDirty() for the individual
//					  objects as well as the component.
//					* Remove data members for numbers of
//					  items - these can be determined from
//					  the data Vectors as needed.
//					* Clean up Javadoc.
//					* Alphabetize the methods.
//					* Change getRes* setRes* to remove the
//					  "Res" - it is redundant.
// 2003-09-18	SAM, RTi		Change reservoir accounts to use the
//					base class for the name and assign
//					sequential integers for the account
//					ID.
// 2003-10-10	SAM, RTi		Add disconnectRights().
// 2003-10-15	SAM, RTi		Change some initial values to agree
//					with the old SMGUI for new instance.
// 2004-06-05	SAM, RTi		* Add methods to handle collections,
//					  similar to StateCU locations.
// 2004-07-02	SAM, RTi		* Overload the constructor to indicate
//					  whether reasonable defaults should be
//					  assigned.
//					* Add getRdateChoices() and
//					  getRdateDefault() to help provide
//					  information for GUIs.
//					* Add getIresswChoices() and
//					  getIresswDefault() to help provide
//					  information for GUIs.
// 2004-07-14	JTS, RTi		* Added acceptChanges().
//					* Added changed().
//					* Added clone().
//					* Added compareTo().
//					* Added createBackup().
//					* Added restoreOriginal().
//					* Now implements Cloneable.
//					* Now implements Comparable.
//					* Clone status is checked via _isClone
//					  when the component is marked as dirty.
// 2004-09-09	SAM, RTi		When reading and writing, adjust the
//					file paths using the working directory.
// 2004-11-12	SAM, RTi		Remove "Fill #" from second line in
//					output header.
// 2005-02-01	SAM, RTi		The writeStateModFile() method was
//					automatically adding a dead storage
//					account as the last account, if dead
//					storage was specified, and it was
//					decrementing the account numbers by the
//					dead storage value.  Remove this code
//					and include options in StateDMI -
//					handling in the write method is
//					confusing.
// 2005-03-30	JTS, RTi		* Added getCollectionPartType().
//					* Added getCollectionYears().
// 2005-04-18	JTS, RTi		Added writeListFile().
// 2005-04-19	JTS, RTi		Added writeCollectionListFile().
// 2005-05-06	SAM, RTi		Correct a couple of typos in StateMod
//					data set components for writing the
//					delimited files.
// 2006-06-13	SAM, RTi		Change the names of secondary list files
//					to be more appropriate.
// 2006-08-15	SAM, RTi		Fix so that if target time series file
//					has one time series for a reservoir,
//					then assign to the maximum target and
//					leave the minimum as null.
// 2007-04-12	Kurt Tometich, RTi		Added checkComponentData() and
//									getDataHeader() methods for check
//									file and data check support.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
//------------------------------------------------------------------------------
// EndHeader

namespace DWR.StateMod
{

	using GeoRecord = RTi.GIS.GeoView.GeoRecord;
	using HasGeoRecord = RTi.GIS.GeoView.HasGeoRecord;
	using DayTS = RTi.TS.DayTS;
	using MonthTS = RTi.TS.MonthTS;
	using TS = RTi.TS.TS;
	using TSUtil = RTi.TS.TSUtil;
	using DataSetComponent = RTi.Util.IO.DataSetComponent;
	using IOUtil = RTi.Util.IO.IOUtil;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// The Reservoir class holds data for entries in the StateMod reservoir station
	/// file.  Secondary data classes are used in cases where lists of data are used.
	/// </summary>
	public class StateMod_Reservoir : StateMod_Data, ICloneable, IComparable<StateMod_Data>, HasGeoRecord, StateMod_ComponentValidator
	{

	/// <summary>
	/// date for one fill rule admin
	/// </summary>
	protected internal double _rdate;

	/// <summary>
	/// minimum reservoir content
	/// </summary>
	protected internal double _volmin;

	/// <summary>
	/// Maximum reservoir content
	/// </summary>
	protected internal double _volmax;

	/// <summary>
	/// Maximum reservoir release
	/// </summary>
	protected internal double _flomax;

	/// <summary>
	/// Dead storage in reservoir
	/// </summary>
	protected internal double _deadst;

	/// <summary>
	/// List of owners
	/// </summary>
	protected internal IList<StateMod_ReservoirAccount> _owners;

	/// <summary>
	/// Daily id
	/// </summary>
	protected internal string _cresdy;

	/// <summary>
	/// List of evap/precip stations
	/// </summary>
	protected internal IList<StateMod_ReservoirClimate> _climate_Vector;

	/// <summary>
	/// List of area capacity values
	/// </summary>
	protected internal IList<StateMod_ReservoirAreaCap> _areacapvals;

	/// <summary>
	/// List of reservoir rights
	/// </summary>
	protected internal IList<StateMod_ReservoirRight> _rights;

	/// <summary>
	/// End of month content time series.
	/// </summary>
	protected internal MonthTS _content_MonthTS;

	/// <summary>
	/// End of day content time series.
	/// </summary>
	protected internal DayTS _content_DayTS;

	/// <summary>
	/// Minimum target time series (Monthly).
	/// </summary>
	protected internal MonthTS _mintarget_MonthTS;

	/// <summary>
	/// Maximum target time series (Monthly).
	/// </summary>
	protected internal MonthTS _maxtarget_MonthTS;

	/// <summary>
	/// Minimum target time series (Daily).
	/// </summary>
	protected internal DayTS _mintarget_DayTS;

	/// <summary>
	/// Maximum target time series (Daily).
	/// </summary>
	protected internal DayTS _maxtarget_DayTS;

	/// <summary>
	/// Link to spatial data -- currently NOT cloned.
	/// </summary>
	protected internal GeoRecord _georecord;

	// Collections are set up to be specified by year, although currently for
	// reservoir collections are always the same for the full period.

	/// <summary>
	/// Types of collections.  An aggregate merges the water rights whereas
	/// a system keeps all the water rights but just has one ID.
	/// </summary>
	public static string COLLECTION_TYPE_AGGREGATE = "Aggregate";
	public static string COLLECTION_TYPE_SYSTEM = "System";

	private string __collection_type = StateMod_Util.MISSING_STRING;

	/// <summary>
	/// Used by DMI software - currently no options.
	/// </summary>
	private string __collection_part_type = "Reservoir";

	/// <summary>
	/// The identifiers for data that are collected - null if not a collection location.
	/// This is a list of List where the __collection_year is the first dimension.
	/// This is ugly but need to use the code to see if it can be made cleaner.
	/// </summary>
	private IList<IList<string>> __collection_Vector = null;

	/// <summary>
	/// An array of years that correspond to the aggregate/system.  Reservoirs currently only have one year.
	/// </summary>
	private int[] __collection_year = null;

	/// <summary>
	/// Construct and initialize data to reasonable defaults where appropriate.
	/// 
	/// </summary>
	public StateMod_Reservoir() : this(true)
	{
	}

	/// <summary>
	/// Construct and initialize data to reasonable defaults where appropriate. </summary>
	/// <param name="initialize_defaults"> If true, initialize data to reasonable defaults
	/// (e.g., zero dead storage) - this is suitable for defaults in the StateMod GUI.
	/// If false, don't initialize data - this is suitable for filling in StateDMI.
	///  </param>
	public StateMod_Reservoir(bool initialize_defaults) : base()
	{
		initialize(initialize_defaults);
	}

	/// <summary>
	/// Accepts any changes made inside of a GUI to this object.
	/// </summary>
	public virtual void acceptChanges()
	{
		_isClone = false;
		_original = null;
	}

	/// <summary>
	/// Add owner (account). </summary>
	/// <param name="owner"> StateMod_ReservoirAccount to add. </param>
	public virtual void addAccount(StateMod_ReservoirAccount owner)
	{
		if (owner != null)
		{
			_owners.Add(owner);
			setDirty(true);
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_RESERVOIR_STATIONS, true);
			}
		}
	}

	/// <summary>
	/// Add AreaCap. </summary>
	/// <param name="areacap"> StateMod_ReservoirAreaCap to add. </param>
	public virtual void addAreaCap(StateMod_ReservoirAreaCap areacap)
	{
		if (areacap != null)
		{
			_areacapvals.Add(areacap);
			setDirty(true);
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_RESERVOIR_STATIONS,true);
			}
		}
	}

	/// <summary>
	/// Add climate. </summary>
	/// <param name="climate"> StateMod_ReservoirClimate to add. </param>
	public virtual void addClimate(StateMod_ReservoirClimate climate)
	{
		if (climate != null)
		{
			_climate_Vector.Add(climate);
			setDirty(true);
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_RESERVOIR_STATIONS, true);
			}
		}
	}

	/// <summary>
	/// Adds a right to the rights linked list
	/// </summary>
	public virtual void addRight(StateMod_ReservoirRight right)
	{
		if (right != null)
		{
			_rights.Add(right);
		}
		// No need to set dirty because right is not stored in station file.
	}

	/// <summary>
	/// Compares this object with its original value (generated by createBackup() upon
	/// entering a GUI) to see if it has changed.
	/// </summary>
	public virtual bool changed()
	{
		if (_original == null)
		{
			return true;
		}
		if (compareTo(_original) == 0)
		{
			return false;
		}
		return true;
	}

	/// <summary>
	/// Clones the data object. </summary>
	/// <returns> a cloned object. </returns>
	public override object clone()
	{
		StateMod_Reservoir r = (StateMod_Reservoir)base.clone();
		r._isClone = true;

		// The following are not cloned because there is no need to.  
		// The cloned values are only used for comparing between the 
		// values that can be changed in a single GUI.  The following
		// Vectors' data have their changes committed in other GUIs.
		r._climate_Vector = _climate_Vector;
		r._areacapvals = _areacapvals;
		r._rights = _rights;
		return r;
	}

	/// <summary>
	/// Compares this object to another StateMod_Reservoir object. </summary>
	/// <param name="data"> the object to compare against. </param>
	/// <returns> 0 if they are the same, 1 if this object is greater than the other
	/// object, or -1 if it is less. </returns>
	public virtual int CompareTo(StateMod_Data data)
	{
		int res = base.CompareTo(data);
		if (res != 0)
		{
			return res;
		}

		StateMod_Reservoir r = (StateMod_Reservoir)data;

		if (_rdate < r._rdate)
		{
			return -1;
		}
		else if (_rdate > r._rdate)
		{
			return 1;
		}

		if (_volmin < r._volmin)
		{
			return -1;
		}
		else if (_volmin > r._volmin)
		{
			return 1;
		}

		if (_volmax < r._volmax)
		{
			return -1;
		}
		else if (_volmax > r._volmax)
		{
			return 1;
		}

		if (_flomax < r._flomax)
		{
			return -1;
		}
		else if (_flomax > r._flomax)
		{
			return 1;
		}

		if (_deadst < r._deadst)
		{
			return -1;
		}
		else if (_deadst > r._deadst)
		{
			return 1;
		}

		res = _cresdy.CompareTo(r._cresdy);
		if (res != 0)
		{
			return res;
		}

		return 0;
	}

	/// <summary>
	/// This set of routines don't actually add an element to an array.  They already
	/// exist as part of a list of StateMod_ReservoirRight.  We are just connecting pointers.
	/// </summary>
	public static void connectAllRights(IList<StateMod_Reservoir> reservoirs, IList<StateMod_ReservoirRight> rights)
	{
		if (reservoirs == null)
		{
			return;
		}
		int i, num_res = reservoirs.Count;

		StateMod_Reservoir res = null;
		for (i = 0; i < num_res; i++)
		{
			res = reservoirs[i];
			if (res == null)
			{
				continue;
			}
			res.connectRights(rights);
		}
		res = null;
	}

	/// <summary>
	/// Connect all reservoir-related time series to reservoir data objects. </summary>
	/// <param name="reservoirs"> List of StateMod_Reservoir. </param>
	/// <param name="content_MonthTS"> List of MonthTS containing end-of-month content. </param>
	/// <param name="content_DayTS"> List of DayTS containing end-of-day content. </param>
	/// <param name="target_MonthTS"> List of MonthTS containing minimum/maximum target time series pairs. </param>
	/// <param name="target_DayTS"> List of DayTS containing minimum/maximum target time series pairs. </param>
	public static void connectAllTS(IList<StateMod_Reservoir> reservoirs, IList<MonthTS> content_MonthTS, IList<DayTS> content_DayTS, IList<MonthTS> target_MonthTS, IList<DayTS> target_DayTS)
	{
		if (reservoirs == null)
		{
			return;
		}
		int numRes = reservoirs.Count;

		StateMod_Reservoir res = null;
		for (int i = 0; i < numRes; i++)
		{
			res = reservoirs[i];
			if (res == null)
			{
				continue;
			}
			res.connectContentMonthTS(content_MonthTS);
			res.connectContentDayTS(content_DayTS);
			res.connectTargetMonthTS(target_MonthTS);
			res.connectTargetDayTS(target_DayTS);
		}
	}

	/// <summary>
	/// Connect the end-of-day content time series to this reservoir, using the
	/// time series location and the reservoir identifier to make a match.
	/// The reservoir name is also set as the time series description. </summary>
	/// <param name="contentTS"> Vector of end-of-day content time series to search. </param>
	public virtual void connectContentDayTS(IList<DayTS> contentTS)
	{
		if (contentTS == null)
		{
			return;
		}
		int numTS = contentTS.Count;
		DayTS ts = null;

		for (int i = 0; i < numTS; i++)
		{
			ts = contentTS[i];
			if (ts == null)
			{
				continue;
			}
			if (_id.Equals(ts.getIdentifier().getLocation()))
			{
				setContentDayTS(ts);
				ts.setDescription(getName());
				break;
			}
		}
	}

	/// <summary>
	/// Connect the end-of-month content time series to this reservoir, using the
	/// time series location and the reservoir identifier to make a match.
	/// The reservoir name is also set as the time series description. </summary>
	/// <param name="contentTS"> Vector of end-of-month content time series to search. </param>
	public virtual void connectContentMonthTS(IList<MonthTS> contentTS)
	{
		if (contentTS == null)
		{
			return;
		}
		int numTS = contentTS.Count;
		MonthTS ts = null;

		for (int i = 0; i < numTS; i++)
		{
			ts = contentTS[i];
			if (ts == null)
			{
				continue;
			}
			if (_id.Equals(ts.getIdentifier().getLocation()))
			{
				setContentMonthTS(ts);
				ts.setDescription(getName());
				break;
			}
		}
	}

	/// <summary>
	/// Connect the minimum and maximum target time series (daily) to the reservoir,
	/// using the time series location and the reservoir identifier to make the match.
	/// The time series must be in the order of min, max, min, max, etc. </summary>
	/// <param name="targetTS"> The Vector of DayTS containing minimum and maximum targets. </param>
	public virtual void connectTargetDayTS(IList<DayTS> targetTS)
	{
		if (targetTS == null)
		{
			return;
		}
		int numTS = targetTS.Count;
		DayTS ts = null;

		for (int i = 0; i < numTS; i++)
		{
			ts = targetTS[i];
			if (ts == null)
			{
				continue;
			}
			if (_id.Equals(ts.getIdentifier().getLocation()))
			{
				setMinTargetDayTS(ts);
				ts.setDescription(getName());
				// now set max
				ts = targetTS[i + 1];
				if (ts == null)
				{
					continue;
				}
				if (_id.Equals(ts.getIdentifier().getLocation()))
				{
					setMaxTargetDayTS(ts);
					ts.setDescription(getName());
				}
				break;
			}
		}
	}

	/// <summary>
	/// Connect the minimum and maximum target time series (monthly) to the reservoir,
	/// using the time series location and the reservoir identifier to make the match.
	/// The time series must be in the order of min, max, min, max, etc. </summary>
	/// <param name="targetTS"> The Vector of MonthTS containing minimum and maximum targets. </param>
	public virtual void connectTargetMonthTS(IList<MonthTS> targetTS)
	{
		if (targetTS == null)
		{
			return;
		}
		int numTS = targetTS.Count;
		MonthTS ts = null, ts1 = null, ts2 = null;

		for (int i = 0; i < numTS; i++)
		{
			ts = targetTS[i];
			ts1 = null;
			ts2 = null;
			if (ts == null)
			{
				// Can't check the time series...
				continue;
			}
			if (_id.Equals(ts.getIdentifier().getLocation()))
			{
				// Found a matching identifier so this is the minimum
				// or maximum target part of the min/max time series
				// pair.  If only one time series is found, set it to
				// the maximum and set the minimum to null (interpreted
				// as zeros in other code like graphs).
				ts1 = ts;
				// Now set max, which should be the next time series.
				if ((i + 1) < numTS)
				{
					ts2 = targetTS[i + 1];
					if (!_id.Equals(ts2.getIdentifier().getLocation()))
					{
						// Time series is for a different reservoir so reset to null...
						ts2 = null;
					}
				}
				// Now link the time series...
				if ((ts1 == null) && (ts2 == null))
				{
					// Nothing to do...
				}
				else if ((ts2 == null) && (ts1 != null))
				{
					// Only one time series is specified so it is the maximum...
					setMaxTargetMonthTS(ts1);
					ts1.setDescription(getName());
				}
				else
				{
					// Have both time series...
					setMinTargetMonthTS(ts1);
					ts1.setDescription(getName());
					setMaxTargetMonthTS(ts2);
					ts2.setDescription(getName());
				}
				break;
			}
		}
	}

	/// <summary>
	/// Connect the rights to this instance of a reservoir. </summary>
	/// <param name="rights"> Vector of all reservoir rights. </param>
	public virtual void connectRights(IList<StateMod_ReservoirRight> rights)
	{
		if (rights == null)
		{
			return;
		}
		int i;
		int num_rights = rights.Count;

		StateMod_ReservoirRight right;
		for (i = 0; i < num_rights; i++)
		{
			right = rights[i];
			if (right == null)
			{
				continue;
			}
			if (_id.Equals(right.getCgoto()))
			{
				_rights.Add(right);
			}
		}
	}

	/// <summary>
	/// Creates a backup of the current data object and stores it in _original,
	/// for use in determining if an object was changed inside of a GUI.
	/// </summary>
	public virtual void createBackup()
	{
		_original = (StateMod_Reservoir)clone();
		((StateMod_Reservoir)_original)._isClone = false;
		_isClone = true;
	}

	/// <summary>
	/// Deletes the area capacity at the given index </summary>
	/// <param name="index"> of the area capacity to delete. </param>
	public virtual void deleteAreaCapAt(int index)
	{
		_areacapvals.RemoveAt(index);
		setDirty(true);
		if (!_isClone && _dataset != null)
		{
			_dataset.setDirty(StateMod_DataSet.COMP_RESERVOIR_STATIONS, true);
		}
	}

	/// <summary>
	/// Deletes the climate at the given index </summary>
	/// <param name="index"> of the climate to delete </param>
	public virtual void deleteClimateAt(int index)
	{
		_climate_Vector.RemoveAt(index);
		setDirty(true);
		if (!_isClone && _dataset != null)
		{
			_dataset.setDirty(StateMod_DataSet.COMP_RESERVOIR_STATIONS, true);
		}
	}

	/// <summary>
	/// Deletes the owner at the given index </summary>
	/// <param name="index"> of the owner to delete </param>
	public virtual void deleteAccountAt(int index)
	{
		_owners.RemoveAt(index);
		setDirty(true);
		if (!_isClone && _dataset != null)
		{
			_dataset.setDirty(StateMod_DataSet.COMP_RESERVOIR_STATIONS, true);
		}
	}

	// TODO - need to decide in the GUI if the right is actually removed from the main list.
	/// <summary>
	/// Remove right from list.  A comparison on the ID is made. </summary>
	/// <param name="right"> Right to remove.  Note that the right is only removed from the
	/// list for this reservoir and must also be removed from the main reservoir right list. </param>
	public virtual void disconnectRight(StateMod_ReservoirRight right)
	{
		if (right == null)
		{
			return;
		}
		int size = _rights.Count;
		StateMod_ReservoirRight right2;
		// Assume that more than on instance can exist, even though this is not allowed...
		for (int i = 0; i < size; i++)
		{
			right2 = _rights[i];
			if (right2.getID().Equals(right.getID(), StringComparison.OrdinalIgnoreCase))
			{
				_rights.RemoveAt(i);
			}
		}
	}

	/// <summary>
	/// Disconnect all rights.
	/// </summary>
	public virtual void disconnectRights()
	{
		_rights.Clear();
	}

	/// <summary>
	/// Clean up before garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~StateMod_Reservoir()
	{
		_owners = null;
		_climate_Vector = null;
		_areacapvals = null;
		_cresdy = null;
		_rights = null;
		_content_MonthTS = null;
		_content_DayTS = null;
		_mintarget_MonthTS = null;
		_maxtarget_MonthTS = null;
		_mintarget_DayTS = null;
		_maxtarget_DayTS = null;
		_georecord = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Retrieve the owner at a particular index.
	/// </summary>
	public virtual StateMod_ReservoirAccount getAccount(int index)
	{
		return (_owners[index]);
	}

	/// <summary>
	/// Get all owners.
	/// </summary>
	public virtual IList<StateMod_ReservoirAccount> getAccounts()
	{
		return _owners;
	}

	/// <summary>
	/// Return the area capacity at a particular index.
	/// </summary>
	public virtual StateMod_ReservoirAreaCap getAreaCap(int index)
	{
		return (_areacapvals[index]);
	}

	/// <summary>
	/// Return all the area capacity data.
	/// </summary>
	public virtual IList<StateMod_ReservoirAreaCap> getAreaCaps()
	{
		return _areacapvals;
	}

	/// <summary>
	/// Return the climate station at a particular index.
	/// </summary>
	public virtual StateMod_ReservoirClimate getClimate(int index)
	{
		if (_climate_Vector.Count == 0 || index >= _climate_Vector.Count)
		{
			return (new StateMod_ReservoirClimate());
		}
		return (_climate_Vector[index]);
	}

	/// <summary>
	/// Return the climate station assignments.
	/// </summary>
	public virtual IList<StateMod_ReservoirClimate> getClimates()
	{
		return _climate_Vector;
	}

	/// <summary>
	/// Return the collection part ID list for the specific year.  For reservoirs, only one
	/// aggregate/system list is currently supported so the same information is returned
	/// regardless of the year value. </summary>
	/// <returns> the list of collection part IDS, or null if not defined. </returns>
	public virtual IList<string> getCollectionPartIDs(int year)
	{
		if (__collection_Vector.Count == 0)
		{
			return null;
		}
		//if ( __collection_part_type.equalsIgnoreCase("Reservoir") ) {
			// The list of part IDs will be the first and only list...
			return __collection_Vector[0];
		//}
		/* Not supported
		else if ( __collection_part_type.equalsIgnoreCase("Parcel") ) {
			// The list of part IDs needs to match the year.
			for ( int i = 0; i < __collection_year.length; i++ ) {
				if ( year == __collection_year[i] ) {
					return (Vector)__collection_Vector.elementAt(i);
				}
			}
		}
		return null;
		*/
	}

	/// <summary>
	/// Returns the collection part type ("Reservoir"). </summary>
	/// <returns> the collection part type ("Reservoir"). </returns>
	public virtual string getCollectionPartType()
	{
		return __collection_part_type;
	}

	/// <summary>
	/// Return the collection type, "Aggregate" or "System". </summary>
	/// <returns> the collection type, "Aggregate" or "System". </returns>
	public virtual string getCollectionType()
	{
		return __collection_type;
	}

	/// <summary>
	/// Returns the collection years. </summary>
	/// <returns> the collection years. </returns>
	public virtual int[] getCollectionYears()
	{
		return __collection_year;
	}

	/// <summary>
	/// Return end-of-day content time series. </summary>
	/// <returns> end-of-day content time series. </returns>
	public virtual DayTS getContentDayTS()
	{
		return _content_DayTS;
	}

	/// <summary>
	/// Return end-of-month content time series. </summary>
	/// <returns> end-of-month content time series. </returns>
	public virtual MonthTS getContentMonthTS()
	{
		return _content_MonthTS;
	}

	/// <summary>
	/// Return cresdy
	/// </summary>
	public virtual string getCresdy()
	{
		return _cresdy;
	}

	/// <summary>
	/// Returns the data column header for the specifically checked data. </summary>
	/// <returns> Data column header. </returns>
	public static string[] getDataHeader()
	{
		// TODO KAT 2007-04-16 When specific checks are added to checkComponentData
		// return the header for that data here
		return new string[] {};
	}

	/// <summary>
	/// Return the dead storage in reservoir.
	/// </summary>
	public virtual double getDeadst()
	{
		return _deadst;
	}

	/// <summary>
	/// Return the maximum reservoir release.
	/// </summary>
	public virtual double getFlomax()
	{
		return _flomax;
	}

	/// <summary>
	/// Get the geographical data associated with the reservoir. </summary>
	/// <returns> the GeoRecord for the reservoir. </returns>
	public virtual GeoRecord getGeoRecord()
	{
		return _georecord;
	}

	/// <summary>
	/// Return a list of on/off switch option strings, for use in GUIs.
	/// The options are of the form "0" if include_notes is false and "0 - Off", if include_notes is true. </summary>
	/// <returns> a list of on/off switch option strings, for use in GUIs. </returns>
	/// <param name="include_notes"> Indicate whether notes should be added after the parameter values. </param>
	public static IList<string> getIresswChoices(bool include_notes)
	{
		IList<string> v = new List<string>();
		v.Add("0 - Off"); // Possible options are listed here.
		v.Add("1 - On, do not store above reservoir targets");
		v.Add("2 - 1 and adjust volume, etc. by dead storage");
		v.Add("3 - On, do store above reservoir targets");
		if (!include_notes)
		{
			// Remove the trailing notes...
			int size = v.Count;
			for (int i = 0; i < size; i++)
			{
				v[i] = StringUtil.getToken(v[i], " ", 0, 0);
			}
		}
		return v;
	}

	/// <summary>
	/// Return the default on/off switch choice.  This can be used by GUI code
	/// to pick a default for a new reservoir. </summary>
	/// <returns> the default reservoir replacement choice. </returns>
	public static string getIresswDefault(bool include_notes)
	{ // Make this aggree with the above method...
		if (include_notes)
		{
			return "1 - On, do not store above reservoir targets";
		}
		else
		{
			return "1";
		}
	}

	/// <summary>
	/// Get the last right associated with reservoir.
	/// </summary>
	public virtual StateMod_ReservoirRight getLastRight()
	{
		if ((_rights == null) || (_rights.Count == 0))
		{
			return null;
		}
		return _rights[_rights.Count - 1];
	}

	/// <summary>
	/// Get the maximum target time series (daily). </summary>
	/// <returns> the maximum target time series (daily). </returns>
	public virtual DayTS getMaxTargetDayTS()
	{
		return _maxtarget_DayTS;
	}

	/// <summary>
	/// Get the maximum target time series (monthly). </summary>
	/// <returns> the maximum target time series (monthly). </returns>
	public virtual MonthTS getMaxTargetMonthTS()
	{
		return _maxtarget_MonthTS;
	}

	/// <summary>
	/// Get the minimum target time series (daily). </summary>
	/// <returns> the minimum target time series (daily). </returns>
	public virtual DayTS getMinTargetDayTS()
	{
		return _mintarget_DayTS;
	}

	/// <summary>
	/// Get the minimum target time series (monthly). </summary>
	/// <returns> the minimum target time series (monthly). </returns>
	public virtual MonthTS getMinTargetMonthTS()
	{
		return _mintarget_MonthTS;
	}

	/// <summary>
	/// Return the number of owners.
	/// </summary>
	public virtual int getNowner()
	{
		return _owners.Count;
	}

	/// <summary>
	/// Return the number of area capacity values.
	/// </summary>
	public virtual int getNrange()
	{
		return _areacapvals.Count;
	}

	/// <summary>
	/// Return the number of evaporation time series for the reservoir. </summary>
	/// <param name="tslist"> The list of monthly evaporation data to check. </param>
	/// <param name="check_ts"> If true, get the count of non-null time series (the reservoir
	/// may reference evaporation station identifiers but the identifiers may not actually exist). </param>
	/// <returns> the number of evaporation time series for the reservoir. </returns>
	public virtual int getNumEvaporationMonthTS(IList<MonthTS> tslist, bool check_ts)
	{ // Loop through the evaporation data for this reservoir.  For each
		// referenced evaporation station, if a matching time series is found, increment the count.
		int nsta = _climate_Vector.Count;
		int nts = 0;
		TS ts;
		int pos = 0;
		StateMod_ReservoirClimate sta = null;
		for (int i = 0; i < nsta; i++)
		{
			sta = _climate_Vector[i];
			if (sta.getType() != StateMod_ReservoirClimate.CLIMATE_EVAP)
			{
				Message.printStatus(1, "", "SAMX climate " + sta.getID() + " is not evap");
				continue;
			}
			pos = TSUtil.IndexOf(tslist, sta.getID(), "Location", 1);
			//Message.printStatus ( 2, "", "SAMX climate " + sta.getID() + " pos is " + pos );
			if (pos >= 0)
			{
				if (check_ts)
				{
					// Make sure that the time series has data...
					ts = tslist[pos];
					if ((ts != null) && ts.hasData())
					{
						//Message.printStatus ( 2, "", "SAMX ts has data." );
						++nts;
					}
					else
					{
						//Message.printStatus ( 2, "", "SAMX ts has NO data." );
					}
				}
				else
				{
					// Just a count of the evaporation time series...
					++nts;
				}
			}
		}
		return nts;
	}

	/// <summary>
	/// Return the number of precipitation time series for the reservoir. </summary>
	/// <param name="tslist"> The list of monthly precipitation data to check. </param>
	/// <param name="check_ts"> If true, get the count of non-null time series (the reservoir
	/// may reference precipitation station identifiers but the identifiers may not actually exist). </param>
	/// <returns> the number of precipitation time series for the reservoir. </returns>
	public virtual int getNumPrecipitationMonthTS(IList<MonthTS> tslist, bool check_ts)
	{ // Loop through the precipitation data for this reservoir.  For each
		// referenced precipitation station, if a matching time series is found, then increment the count.
		int nsta = _climate_Vector.Count;
		int nts = 0;
		TS ts;
		int pos = 0;
		StateMod_ReservoirClimate sta = null;
		for (int i = 0; i < nsta; i++)
		{
			sta = _climate_Vector[i];
			if (sta.getType() != StateMod_ReservoirClimate.CLIMATE_PTPX)
			{
				continue;
			}
			pos = TSUtil.IndexOf(tslist, sta.getID(), "Location", 1);
			if (pos >= 0)
			{
				if (check_ts)
				{
					ts = tslist[pos];
					// Make sure that the time series has data...
					if ((ts != null) && ts.hasData())
					{
						++nts;
					}
				}
				else
				{
					// Just a count of the evaporation time series...
					++nts;
				}
			}
		}
		return nts;
	}

	/// <summary>
	/// Return the number of rights for this reservoir.
	/// </summary>
	public virtual int getNumrights()
	{
		return _rights.Count;
	}

	/// <summary>
	/// Return the date for one fill rule admin.
	/// </summary>
	public virtual double getRdate()
	{
		return _rdate;
	}

	/// <summary>
	/// Return a list of one fill rule (rdate) switch option strings, for use in GUIs.
	/// The options are of the form "-1" if include_notes is false and
	/// "-1 - Do not administer one fill rule", if include_notes is true. </summary>
	/// <returns> a list of on/off switch option strings, for use in GUIs. </returns>
	/// <param name="include_notes"> Indicate whether notes should be added after the parameter values. </param>
	public static IList<string> getRdateChoices(bool include_notes)
	{
		IList<string> v = new List<string>(2);
		v.Add("-1 - Do not administer the one fill rule");
		v.Add("1 - January"); // Possible options are listed here.
		v.Add("2 - February");
		v.Add("3 - March");
		v.Add("4 - April");
		v.Add("5 - May");
		v.Add("6 - June");
		v.Add("7 - July");
		v.Add("8 - August");
		v.Add("9 - September");
		v.Add("10 - October");
		v.Add("11 - November");
		v.Add("12 - December");
		if (!include_notes)
		{
			// Remove the trailing notes...
			int size = v.Count;
			for (int i = 0; i < size; i++)
			{
				v[i] = StringUtil.getToken(v[i], " ", 0, 0);
			}
		}
		return v;
	}

	/// <summary>
	/// Return the default one fill rule switch choice.  This can be used by GUI code
	/// to pick a default for a new reservoir. </summary>
	/// <returns> the default reservoir replacement choice. </returns>
	public static string getRdateDefault(bool include_notes)
	{ // Make this agree with the above method...
		if (include_notes)
		{
			return ("-1 - Do not administer the one fill rule");
		}
		else
		{
			return "-1";
		}
	}

	/// <summary>
	/// Return the right associated with the given index.  If index
	/// number of rights don't exist, null will be returned. </summary>
	/// <param name="index"> desired right index </param>
	public virtual StateMod_ReservoirRight getRight(int index)
	{
		if ((index < 0) || (index >= _rights.Count))
		{
			return null;
		}
		else
		{
			return _rights[index];
		}
	}
	/// <summary>
	/// get the rights
	/// </summary>
	public virtual IList<StateMod_ReservoirRight> getRights()
	{
		return _rights;
	}

	/// <summary>
	/// Return the maximum reservoir content.
	/// </summary>
	public virtual double getVolmax()
	{
		return _volmax;
	}

	/// <summary>
	/// Return the minimum reservoir content.
	/// </summary>
	public virtual double getVolmin()
	{
		return _volmin;
	}

	/// <summary>
	/// Initialize data members. </summary>
	/// <param name="initialize_defaults"> If true, initialize data to reasonable defaults
	/// (e.g., zero dead storage) - this is suitable for defaults in the StateMod GUI.
	/// If false, don't initialize data - this is suitable for filling in StateDMI. </param>
	private void initialize(bool initialize_defaults)
	{
		_smdata_type = StateMod_DataSet.COMP_RESERVOIR_STATIONS;
		_owners = new List<StateMod_ReservoirAccount>();
		_climate_Vector = new List<StateMod_ReservoirClimate>();
		_areacapvals = new List<StateMod_ReservoirAreaCap>();
		_rights = new List<StateMod_ReservoirRight>();
		_content_MonthTS = null;
		_content_DayTS = null;
		_mintarget_MonthTS = null;
		_maxtarget_MonthTS = null;
		_mintarget_DayTS = null;
		_maxtarget_DayTS = null;
		_georecord = null;
		if (initialize_defaults)
		{
			_cresdy = "0"; // Use monthly TS for daily
			_switch = 1; // In base class
			_rdate = -1;
			_volmin = 0;
			_volmax = 0;
			_flomax = 99999.0; // As per old SMGUI new reservoir
			_deadst = 0;
		}
		else
		{
			_cresdy = "";
			_rdate = StateMod_Util.MISSING_INT;
			_volmin = StateMod_Util.MISSING_DOUBLE;
			_volmax = StateMod_Util.MISSING_DOUBLE;
			_flomax = StateMod_Util.MISSING_DOUBLE;
			_deadst = StateMod_Util.MISSING_DOUBLE;
		}
	}

	/// <summary>
	/// Indicate whether the reservoir is a collection (an aggregate or system). </summary>
	/// <returns> true if the reservoir is an aggregate or system. </returns>
	public virtual bool isCollection()
	{
		if (__collection_Vector == null)
		{
			return false;
		}
		else
		{
			return true;
		}
	}

	/// <summary>
	/// Insert AreaCap
	/// </summary>
	public virtual void insertAreaCapAt(StateMod_ReservoirAreaCap areacap, int i)
	{
		if (areacap != null)
		{
			_areacapvals.Insert(i,areacap);
			setDirty(true);
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_RESERVOIR_STATIONS, true);
			}
		}
	}

	/// <summary>
	/// Read reservoir information in and store in a Vector. </summary>
	/// <param name="filename"> Name of file to read. </param>
	/// <exception cref="Exception"> if there is an error reading the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.List<StateMod_Reservoir> readStateModFile(String filename) throws Exception
	public static IList<StateMod_Reservoir> readStateModFile(string filename)
	{
		string routine = "StateMod_Reservoir.readStateModFile";
		IList<StateMod_Reservoir> theReservoirs = new List<StateMod_Reservoir>();
		string iline = null;
		IList<object> v = new List<object>(9);
		int[] format_0 = new int[] {StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_INTEGER, StringUtil.TYPE_DOUBLE, StringUtil.TYPE_SPACE, StringUtil.TYPE_STRING};
		int[] format_0w = new int[] {12, 24, 12, 8, 8, 1, 12};
		int[] format_1 = new int[] {StringUtil.TYPE_SPACE, StringUtil.TYPE_DOUBLE, StringUtil.TYPE_DOUBLE, StringUtil.TYPE_DOUBLE, StringUtil.TYPE_DOUBLE, StringUtil.TYPE_INTEGER, StringUtil.TYPE_INTEGER, StringUtil.TYPE_INTEGER, StringUtil.TYPE_INTEGER};
		int[] format_1w = new int[] {24, 8, 8, 8, 8, 8, 8, 8, 8};
		int[] format_2 = new int[] {StringUtil.TYPE_SPACE, StringUtil.TYPE_STRING, StringUtil.TYPE_DOUBLE, StringUtil.TYPE_DOUBLE, StringUtil.TYPE_DOUBLE, StringUtil.TYPE_INTEGER};
		int[] format_2w = new int[] {12, 12, 8, 8, 8, 8};
		int[] format_3 = new int[] {StringUtil.TYPE_SPACE, StringUtil.TYPE_STRING, StringUtil.TYPE_DOUBLE};
		int[] format_3w = new int[] {24, 12, 8};
		int[] format_4 = new int[] {StringUtil.TYPE_SPACE, StringUtil.TYPE_DOUBLE, StringUtil.TYPE_DOUBLE, StringUtil.TYPE_DOUBLE};
		int[] format_4w = new int[] {24, 8, 8, 8};
		StreamReader @in = null;
		StateMod_Reservoir aReservoir = null;
		StateMod_ReservoirAccount anAccount = null;
		StateMod_ReservoirClimate anEvap = null;
		StateMod_ReservoirClimate aPtpx = null;
		int i = 0;

		if (Message.isDebugOn)
		{
			Message.printDebug(10, routine, "in SMParseResFile reading file: " + filename);
		}
		int line_count = 0;
		try
		{
			@in = new StreamReader(IOUtil.getPathUsingWorkingDir(filename));
			while (!string.ReferenceEquals((iline = @in.ReadLine()), null))
			{
				++line_count;
				// check for comments
				if (iline.StartsWith("#", StringComparison.Ordinal) || iline.Trim().Length == 0)
				{
					continue;
				}

				// allocate new reservoir node
				aReservoir = new StateMod_Reservoir();

				// line 1
				if (Message.isDebugOn)
				{
					Message.printDebug(50, routine, "line 1: " + iline);
				}
				StringUtil.fixedRead(iline, format_0, format_0w, v);
				aReservoir.setID(((string)v[0]).Trim());
				aReservoir.setName(((string)v[1]).Trim());
				aReservoir.setCgoto(((string)v[2]).Trim());
				aReservoir.setSwitch((int?)v[3]);
				aReservoir.setRdate((double?)v[4]);
				aReservoir.setCresdy(((string)v[5]).Trim());

				// line 2
				iline = @in.ReadLine();
				++line_count;
				if (Message.isDebugOn)
				{
					Message.printDebug(50, routine, "line 2: " + iline);
				}
				StringUtil.fixedRead(iline, format_1, format_1w, v);
				aReservoir.setVolmin(((double?)v[0]));
				aReservoir.setVolmax(((double?)v[1]));
				aReservoir.setFlomax(((double?)v[2]));
				aReservoir.setDeadst(((double?)v[3]));
				int nowner = ((int?)v[4]).Value;
				int nevap = ((int?)v[5]).Value;
				int nptpx = ((int?)v[6]).Value;
				int nrange = ((int?)v[7]).Value;

				// get the owner's information
				if (Message.isDebugOn)
				{
					Message.printDebug(50, routine, "Number of owners: " + nowner);
				}
				for (i = 0; i < nowner; i++)
				{
					iline = @in.ReadLine();
					++line_count;
					StringUtil.fixedRead(iline, format_2,format_2w, v);
					anAccount = new StateMod_ReservoirAccount();
					// Account ID is set to the numerical count (StateMod uses the number)
					anAccount.setID("" + (i + 1));
					anAccount.setName(((string)v[0]).Trim());
					anAccount.setOwnmax(((double?)v[1]));
					anAccount.setCurown(((double?)v[2]));
					anAccount.setPcteva(((double?)v[3]));
					anAccount.setN2own(((int?)v[4]));
					aReservoir.addAccount(anAccount);
				}

				// get the evaporation information
				for (i = 0; i < nevap; i++)
				{
					iline = @in.ReadLine();
					++line_count;
					StringUtil.fixedRead(iline, format_3,format_3w, v);
					anEvap = new StateMod_ReservoirClimate();
					anEvap.setID(((string)v[0]).Trim());
					anEvap.setType(StateMod_ReservoirClimate.CLIMATE_EVAP);
					anEvap.setWeight(((double?)v[1]));
					aReservoir.addClimate(anEvap);
				}

				// get the precipitation information
				for (i = 0; i < nptpx; i++)
				{
					iline = @in.ReadLine();
					++line_count;
					StringUtil.fixedRead(iline, format_3, format_3w, v);
					aPtpx = new StateMod_ReservoirClimate();
					aPtpx.setID(((string)v[0]).Trim());
					aPtpx.setType(StateMod_ReservoirClimate.CLIMATE_PTPX);
					aPtpx.setWeight(((double?)v[1]));
					aReservoir.addClimate(aPtpx);
				}

				// get the area capacity information
				for (i = 0; i < nrange; i++)
				{
					iline = @in.ReadLine();
					++line_count;
					StringUtil.fixedRead(iline, format_4, format_4w, v);
					StateMod_ReservoirAreaCap anAreaCap = new StateMod_ReservoirAreaCap();
					anAreaCap.setConten(((double?)v[0]));
					anAreaCap.setSurarea(((double?)v[1]));
					anAreaCap.setSeepage(((double?)v[2]));
					aReservoir.addAreaCap(anAreaCap);
				}

				// add the reservoir to the vector of reservoirs
				theReservoirs.Add(aReservoir);
			}
		}
		catch (Exception e)
		{
			Message.printWarning(3, routine, "Error reading reservoir stations in line " + line_count);
			throw e;
		}
		finally
		{
			if (@in != null)
			{
				@in.Close();
			}
		}
		return theReservoirs;
	}

	/// <summary>
	/// Cancels any changes made to this object within a GUI since createBackup()
	/// was called and sets _original to null.
	/// </summary>
	public override void restoreOriginal()
	{
		StateMod_Reservoir r = (StateMod_Reservoir)_original;
		base.restoreOriginal();
		_rdate = r._rdate;
		_volmin = r._volmin;
		_volmax = r._volmax;
		_flomax = r._flomax;
		_deadst = r._deadst;
		_cresdy = r._cresdy;
		_isClone = false;
		_original = null;
	}

	/// <summary>
	/// Set owners (accounts).  The new list may have the same or different objects than
	/// the original list.  A comparison of objects is made to verify whether any data are dirty. </summary>
	/// <param name="owners"> Vector of StateMod_ReservoirAccount to set.  This should be a non-null Vector. </param>
	public virtual void setAccounts(IList<StateMod_ReservoirAccount> owners)
	{ // All of the following work id done to make sure the dirty flag on the
		// component and individual objects is correct.  We could just delete
		// all existing accounts and re-add, but we don't know for sure that
		// the dirty flags would be correct.
	/* TODO
		int size = owners.size();
		StateMod_ReservoirAccount account;
		// Array to track whether all old accounts are accounted for.  If the
		// number is not the same, then we need to set the component dirty,
		// regardless of whether any individual objects have changed.
		boolean [] old_matched = null;
		int old_size = _owners.size();
		if ( old_size > 0 ) {
			old_matched = new boolean[old_size];
			for ( int i = 0; i < old_size; i++ ) {
				old_match[i] = false;
			}
		}
		int pos;
		for ( int i = 0; i < size; i++ ) {
			account = (StateMod_ReservoirAccount)owners.elementAt(i);
			pos = StateMod_Util.indexOf ( _owners, account.getID() );
			// Mark that we have checked the old account...
			if ( pos >= 0 ) {
				old_matched[pos] = true;
			}
			// ARG - sam does not have time for this - REVISIT!!!
		}
	*/
		// Finally do the assignment...
		_owners = owners;
	}

	// TODO - need to check dirty flag
	/// <summary>
	/// Set the area capacity vector.
	/// </summary>
	public virtual void setAreaCaps(IList<StateMod_ReservoirAreaCap> areacapvals)
	{
		_areacapvals = areacapvals;
	}

	// TODO - need to check dirty flag
	/// <summary>
	/// Sets the climate station vector.
	/// </summary>
	public virtual void setClimates(IList<StateMod_ReservoirClimate> climates)
	{
		_climate_Vector = climates;
	}

	/// <summary>
	/// Set the collection list for an aggregate/system.  It is assumed that the
	/// collection applies to all years of data. </summary>
	/// <param name="ids"> The identifiers indicating the locations to collection. </param>
	public virtual void setCollectionPartIDs(IList<string> ids)
	{
		if (__collection_Vector == null)
		{
			__collection_Vector = new List<IList<string>> (1);
			__collection_year = new int[1];
		}
		else
		{
			// Remove the previous contents...
			__collection_Vector.Clear();
		}
		// Now assign...
		__collection_Vector.Add(ids);
		__collection_year[0] = 0;
	}

	/// <summary>
	/// Set the collection type. </summary>
	/// <param name="collection_type"> The collection type, either "Aggregate" or "System". </param>
	public virtual void setCollectionType(string collection_type)
	{
		__collection_type = collection_type;
	}

	/// <summary>
	/// Set the end-of-day content time series. </summary>
	/// <param name="ts"> the end-of-day content time series. </param>
	public virtual void setContentDayTS(DayTS ts)
	{
		_content_DayTS = ts;
	}

	/// <summary>
	/// Set the end-of-month content time series. </summary>
	/// <param name="ts"> the end-of-month content time series. </param>
	public virtual void setContentMonthTS(MonthTS ts)
	{
		_content_MonthTS = ts;
	}

	/// <summary>
	/// set cresdy
	/// </summary>
	public virtual void setCresdy(string cresdy)
	{
		if ((!string.ReferenceEquals(cresdy, null)) && !cresdy.Equals(_cresdy))
		{
			_cresdy = cresdy;
			setDirty(true);
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_RESERVOIR_STATIONS, true);
			}
		}
	}

	/// <summary>
	/// Set the dead storage in reservoir.
	/// </summary>
	public virtual void setDeadst(double deadst)
	{
		if (deadst != _deadst)
		{
			_deadst = deadst;
			setDirty(true);
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_RESERVOIR_STATIONS, true);
			}
		}
	}

	/// <summary>
	/// Set the dead storage in reservoir.
	/// </summary>
	public virtual void setDeadst(double? deadst)
	{
		setDeadst(deadst.Value);
	}

	/// <summary>
	/// Set the dead storage in reservoir.
	/// </summary>
	public virtual void setDeadst(string deadst)
	{
		if (!string.ReferenceEquals(deadst, null))
		{
			setDeadst(StringUtil.atod(deadst.Trim()));
		}
	}

	/// <summary>
	/// Set the maximum reservoir release.
	/// </summary>
	public virtual void setFlomax(double flomax)
	{
		if (flomax != _flomax)
		{
			_flomax = flomax;
			setDirty(true);
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_RESERVOIR_STATIONS, true);
			}
		}
	}

	/// <summary>
	/// Set the maximum reservoir release.
	/// </summary>
	public virtual void setFlomax(double? flomax)
	{
		setFlomax(flomax.Value);
	}

	/// <summary>
	/// Set the maximum reservoir release.
	/// </summary>
	public virtual void setFlomax(string flomax)
	{
		if (!string.ReferenceEquals(flomax, null))
		{
			setFlomax(StringUtil.atod(flomax.Trim()));
		}
	}

	/// <summary>
	/// Set the geographic information object associated with the reservoir. </summary>
	/// <param name="georecord"> Geographic record associated with the reservoir. </param>
	public virtual void setGeoRecord(GeoRecord georecord)
	{
		_georecord = georecord;
	}

	/// <summary>
	/// Set the maximum target time series (daily). </summary>
	/// <param name="ts"> maximum target time series (daily). </param>
	public virtual void setMaxTargetDayTS(DayTS ts)
	{
		_maxtarget_DayTS = ts;
	}

	/// <summary>
	/// Set the maximum target time series (monthly). </summary>
	/// <param name="ts"> maximum target time series (monthly). </param>
	public virtual void setMaxTargetMonthTS(MonthTS ts)
	{
		_maxtarget_MonthTS = ts;
	}

	/// <summary>
	/// Set the minimum target time series (daily). </summary>
	/// <param name="ts"> minimum target time series (daily). </param>
	public virtual void setMinTargetDayTS(DayTS ts)
	{
		_mintarget_DayTS = ts;
	}

	/// <summary>
	/// Set the minimum target time series (monthly). </summary>
	/// <param name="ts"> minimum target time series (monthly). </param>
	public virtual void setMinTargetMonthTS(MonthTS ts)
	{
		_mintarget_MonthTS = ts;
	}

	/// <summary>
	/// Set the date for one fill rule admin.
	/// The value 0 is meaning (??)- should be 1-12 or -1 to signify do not administer rule
	/// </summary>
	public virtual void setRdate(double rdate)
	{
		if (rdate == 0)
		{
			rdate = -1;
		}

		if (rdate != _rdate)
		{
			_rdate = rdate;
			setDirty(true);
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_RESERVOIR_STATIONS, true);
			}
		}
	}

	/// <summary>
	/// Set the date for the one fill rule.
	/// </summary>
	public virtual void setRdate(double? rdate)
	{
		setRdate(rdate.Value);
	}

	/// <summary>
	/// Set the date for the one fill rule.
	/// </summary>
	public virtual void setRdate(string rdate)
	{
		if (!string.ReferenceEquals(rdate, null))
		{
			setRdate(StringUtil.atod(rdate.Trim()));
		}
	}

	// TODO - need to check dirty correctly
	/// <summary>
	/// Set the rights
	/// </summary>
	public virtual void setRights(IList<StateMod_ReservoirRight> rights)
	{
		_rights = rights;
	}

	/// <summary>
	/// Set the maximum reservoir content.
	/// </summary>
	public virtual void setVolmax(double volmax)
	{
		if (volmax != _volmax)
		{
			_volmax = volmax;
			setDirty(true);
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_RESERVOIR_STATIONS, true);
			}
		}
	}

	/// <summary>
	/// Set the maximum reservoir content.
	/// </summary>
	public virtual void setVolmax(double? volmax)
	{
		setVolmax(volmax.Value);
	}

	/// <summary>
	/// Set the maximum reservoir content.
	/// </summary>
	public virtual void setVolmax(string volmax)
	{
		if (!string.ReferenceEquals(volmax, null))
		{
			setVolmax(StringUtil.atod(volmax.Trim()));
		}
	}

	/// <summary>
	/// Set the minimum reservoir content.
	/// </summary>
	public virtual void setVolmin(double volmin)
	{
		if (volmin != _volmin)
		{
			_volmin = volmin;
			setDirty(true);
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_RESERVOIR_STATIONS, true);
			}
		}
	}

	/// <summary>
	/// Set the minimum reservoir content.
	/// </summary>
	public virtual void setVolmin(double? volmin)
	{
		setVolmin(volmin.Value);
	}

	/// <summary>
	/// Set the minimum reservoir content.
	/// </summary>
	public virtual void setVolmin(string volmin)
	{
		if (!string.ReferenceEquals(volmin, null))
		{
			setVolmin(StringUtil.atod(volmin.Trim()));
		}
	}

	/// <param name="dataset"> StateMod dataset object. </param>
	/// <returns> validation results. </returns>
	public virtual StateMod_ComponentValidation validateComponent(StateMod_DataSet dataset)
	{
		StateMod_ComponentValidation validation = new StateMod_ComponentValidation();
		string id = getID();
		string name = getName();
		string riverID = getCgoto();
		int iressw = getSwitch();
		double rdate = getRdate();
		string dailyID = getCresdy();
		double volmin = getVolmin();
		double volmax = getVolmax();
		double flomax = getFlomax();
		double deadst = getDeadst();
		int nowner = getNowner();
		// Make sure that basic information is not empty
		if (StateMod_Util.isMissing(id))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Reservoir identifier is blank.", "Specify a reservoir station identifier."));
		}
		if (StateMod_Util.isMissing(name))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Reservoir \"" + id + "\" name is blank.", "Specify a reservoir name to clarify data."));
		}
		// Get the network list if available for checks below
		DataSetComponent comp = null;
		System.Collections.IList rinList = null;
		if (dataset != null)
		{
			comp = dataset.getComponentForComponentType(StateMod_DataSet.COMP_RIVER_NETWORK);
			rinList = (System.Collections.IList)comp.getData();
			if ((rinList != null) && (rinList.Count == 0))
			{
				// Set to null to simplify checks below
				rinList = null;
			}
		}
		if (StateMod_Util.isMissing(riverID))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Reservoir \"" + id + "\" river node ID is blank.", "Specify a river node ID to associate the reservoir with a river network node."));
		}
		else
		{
			// Verify that the river node is in the data set, if the network is available
			if (rinList != null)
			{
				if (StateMod_Util.IndexOf(rinList, riverID) < 0)
				{
					validation.add(new StateMod_ComponentValidationProblem(this,"Reservoir \"" + id + "\" river network ID (" + riverID + ") is not found in the list of river network nodes.", "Specify a valid river network ID to associate the reservoir with a river network node."));
				}
			}
		}
		IList<string> choices = getIresswChoices(false);
		if (StringUtil.indexOfIgnoreCase(choices,"" + iressw) < 0)
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Reservoir \"" + id + "\" switch (" + iressw + ") is invalid.", "Specify the switch as one of " + choices));
		}
		if (!((rdate >= -1.01) && (rdate <= 12.01)))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Reservoir \"" + id + "\" date for one fill rule administration (" + StringUtil.formatString(rdate,"%.0f") + ") is invalid.", "Specify the value as -1 to not administer, or 0 - 12 for the month for reoperation."));
		}
		// Verify that the daily ID is in the data set (daily ID is allowed to be missing)
		if ((dataset != null) && !StateMod_Util.isMissing(dailyID))
		{
			DataSetComponent comp2 = dataset.getComponentForComponentType(StateMod_DataSet.COMP_RESERVOIR_STATIONS);
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<StateMod_Reservoir> resList = (java.util.List<StateMod_Reservoir>)comp2.getData();
			IList<StateMod_Reservoir> resList = (IList<StateMod_Reservoir>)comp2.getData();
			if (dailyID.Equals("0") || dailyID.Equals("3") || dailyID.Equals("4") || dailyID.Equals("5"))
			{
				// OK
			}
			else if ((resList != null) && (resList.Count > 0))
			{
				// Check the reservoir station list
				if (StateMod_Util.IndexOf(resList, dailyID) < 0)
				{
					validation.add(new StateMod_ComponentValidationProblem(this,"Reservoir \"" + id + "\" daily ID (" + dailyID + ") is not 0, 3, 4, or 5 and is not found in the list of reservoir stations.", "Specify the daily ID as 0, 3, 4, 5, or a matching reservoir ID."));
				}
			}
		}
		if (!((volmin >= 0.0)))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Reservoir \"" + id + "\" minimum volume (" + StringUtil.formatString(volmin,"%.1f") + ") is invalid.", "Specify the value as >= 0."));
		}
		if (!((volmax >= 0.0)))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Reservoir \"" + id + "\" maximum volume (" + StringUtil.formatString(volmax,"%.1f") + ") is invalid.", "Specify the value as >= 0."));
		}
		if (!((volmax >= volmin)))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Reservoir \"" + id + "\" maximum volume (" + StringUtil.formatString(volmax,"%.1f") + ") is < the minimum volume (" + StringUtil.formatString(volmin,"%.1f") + ").", "Specify the maximum volumen >= the minimum volume."));
		}
		if (!((flomax >= 0.0)))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Reservoir \"" + id + "\" maximum release (" + StringUtil.formatString(flomax,"%.1f") + ") is invalid.", "Specify the value as >= 0."));
		}
		if (!((deadst >= 0.0)))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Reservoir \"" + id + "\" dead storage (" + StringUtil.formatString(deadst,"%.1f") + ") is invalid.", "Specify the value as >= 0."));
		}
		if (!(nowner >= 0))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Reservoir \"" + id + "\" number of owners (" + nowner + ") is invalid.", "Specify the number of owners as >= 0."));
		}
		else
		{
			// Check owner information
			IList<StateMod_ReservoirAccount> accounts = getAccounts();
			StateMod_ReservoirAccount account;
			string ownnam;
			double ownmax;
			double curown;
			double pcteva;
			int n2own;
			IList<string> n2ownChoices = StateMod_ReservoirAccount.getN2ownChoices(false);
			for (int i = 0; i < nowner; i++)
			{
				account = accounts[i];
				ownnam = account.getName();
				ownmax = account.getOwnmax();
				curown = account.getCurown();
				pcteva = account.getPcteva();
				n2own = account.getN2own();
				if (StateMod_Util.isMissing(ownnam))
				{
					validation.add(new StateMod_ComponentValidationProblem(this,"Reservoir \"" + id + "\" account " + (i + 1) + " name is blank.", "Specify a reservoir account name to clarify data."));
				}
				if (!((ownmax >= 0.0) && (ownmax <= volmax)))
				{
					validation.add(new StateMod_ComponentValidationProblem(this,"Reservoir \"" + id + "\" account " + (i + 1) + " maximum (" + StringUtil.formatString(ownmax,"%.2f") + ") is invalid.", "Specify the account maximum as >= 0 and less than the reservoir maximum (" + StringUtil.formatString(volmax,"%.2f") + ")."));
				}
				if (!((curown >= 0.0) && (curown <= volmax)))
				{
					validation.add(new StateMod_ComponentValidationProblem(this,"Reservoir \"" + id + "\" account " + (i + 1) + " initial value (" + StringUtil.formatString(curown,"%.2f") + ") is invalid.", "Specify the account initial value >= 0 and less than the reservoir maximum (" + StringUtil.formatString(volmax,"%.2f") + ")."));
				}
				if (!((curown <= ownmax)))
				{
					validation.add(new StateMod_ComponentValidationProblem(this,"Reservoir \"" + id + "\" account " + (i + 1) + " initial value (" + StringUtil.formatString(curown,"%.2f") + ") is greater than the account maximum volume (" + StringUtil.formatString(ownmax,"%.2f") + ").", "Specify the account initial value as less than the account maximum."));
				}
				if (!((pcteva >= -1.01) && (pcteva <= 100.0)))
				{
					validation.add(new StateMod_ComponentValidationProblem(this,"Reservoir \"" + id + "\" account " + (i + 1) + " evaporation distribution parameter (" + StringUtil.formatString(pcteva,"%.2f") + ") is invalid.", "Specify the evaporation distribution parameter as -1 or 0 to 100."));
				}
				if (StringUtil.indexOfIgnoreCase(choices,"" + n2own) < 0)
				{
					validation.add(new StateMod_ComponentValidationProblem(this,"Reservoir \"" + id + "\" owernership switch (" + n2own + ") is invalid.", "Specify as one of " + n2ownChoices));
				}
			}
		}
		// Check the climate information
		// TODO SAM 2009-06-01 Evaluate whether to cross check with time series identifiers
		IList<StateMod_ReservoirClimate> climatev = getClimates();
		StateMod_ReservoirClimate clmt = null;
		int nclmt = climatev.Count;
		double weight;
		string staType = null;
		for (int i = 0; i < nclmt; i++)
		{
			clmt = (StateMod_ReservoirClimate)climatev[i];
			if (clmt == null)
			{
				continue;
			}
			else if (clmt.getType() == StateMod_ReservoirClimate.CLIMATE_EVAP)
			{
				staType = "evaportation";
			}
			else if (clmt.getType() == StateMod_ReservoirClimate.CLIMATE_PTPX)
			{
				staType = "precipitation";
			}
			weight = clmt.getWeight();
			if (!((weight >= 0) && (weight <= 100.0)))
			{
				validation.add(new StateMod_ComponentValidationProblem(this,"Reservoir \"" + id + "\" " + staType + " station \"" + clmt.getID() + " weight (" + StringUtil.formatString(weight,"%.2f") + ") is invalid.", "Specify the weight as 0 to 100."));
			}
		}
		// Check the area capacity data.
		IList<StateMod_ReservoirAreaCap> areacapv = getAreaCaps();
		StateMod_ReservoirAreaCap ac = null;
		int nareacap = areacapv.Count;
		double content;
		double area;
		double seepage;
		double contentPrev = -1.0;
		double areaPrev = -1.0;
		double seepagePrev = -1.0;
		for (int i = 0; i < nareacap; i++)
		{
			ac = areacapv[i];
			if (ac == null)
			{
				continue;
			}
			content = ac.getConten();
			area = ac.getSurarea();
			seepage = ac.getSeepage();
			if (!((content >= 0.0)))
			{
				validation.add(new StateMod_ComponentValidationProblem(this,"Reservoir \"" + id + "\" area/cap/seepage " + (i + 1) + " content (" + StringUtil.formatString(content,"%.2f") + ") is invalid.", "Specify the content >= 0."));
			}
			if (!((content > contentPrev)))
			{
				validation.add(new StateMod_ComponentValidationProblem(this,"Reservoir \"" + id + "\" area/cap/seepage " + (i + 1) + " content (" + StringUtil.formatString(content,"%.2f") + ") is not increasing.", "Specify the content > the previous content value."));
			}
			if (!((area >= 0.0)))
			{
				validation.add(new StateMod_ComponentValidationProblem(this,"Reservoir \"" + id + "\" area/cap/seepage " + (i + 1) + " area (" + StringUtil.formatString(area,"%.2f") + ") is invalid.", "Specify the area >= 0."));
			}
			if (i == nareacap - 1)
			{
				// Last one allowed to be the same
				if (!((area >= areaPrev)))
				{
					validation.add(new StateMod_ComponentValidationProblem(this,"Reservoir \"" + id + "\" area/cap/seepage " + (i + 1) + " area (" + StringUtil.formatString(area,"%.2f") + ") is not increasing.", "Specify the area > the previous area value (= allowed on last point)."));
				}
			}
			else
			{
				// Must be increasing
				if (!((area > areaPrev)))
				{
					validation.add(new StateMod_ComponentValidationProblem(this,"Reservoir \"" + id + "\" area/cap/seepage " + (i + 1) + " area (" + StringUtil.formatString(area,"%.2f") + ") is not increasing.", "Specify the area > the previous area value."));
				}
			}
			if (!((seepage >= 0.0)))
			{
				validation.add(new StateMod_ComponentValidationProblem(this,"Reservoir \"" + id + "\" area/cap/seepage " + (i + 1) + " seepage (" + StringUtil.formatString(seepage,"%.2f") + ") is invalid.", "Specify the seepage >= 0."));
			}
			// Seepage might be zero so allow same as previous
			if (!((seepage >= seepagePrev)))
			{
				validation.add(new StateMod_ComponentValidationProblem(this,"Reservoir \"" + id + "\" area/cap/seepage " + (i + 1) + " seepage (" + StringUtil.formatString(seepage,"%.2f") + ") is decreasing.", "Specify the seepage >= the previous seepage value."));
			}
			contentPrev = content;
			areaPrev = area;
			seepagePrev = seepage;
		}
		// TODO SAM 2009-06-01) evaluate how to check rights (with getRights() or checking the rights data
		// set component).
		return validation;
	}

	/// <summary>
	/// Write reservoirs information to output.  History header information 
	/// is also maintained by calling this routine. </summary>
	/// <param name="infile"> input file from which previous history should be taken </param>
	/// <param name="outfile"> output file to which to write </param>
	/// <param name="theReservoirs"> vector of reservoirs to print </param>
	/// <param name="newComments"> addition comments which should be included in history </param>
	/// <exception cref="Exception"> if an error occurs. </exception>

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeStateModFile(String infile, String outfile, java.util.List<StateMod_Reservoir> theReservoirs, java.util.List<String> newComments) throws Exception
	public static void writeStateModFile(string infile, string outfile, IList<StateMod_Reservoir> theReservoirs, IList<string> newComments)
	{
		writeStateModFile(infile, outfile, theReservoirs, newComments, true);
	}

	/// <summary>
	/// Write reservoirs information to output.  History header information 
	/// is also maintained by calling this routine. </summary>
	/// <param name="infile"> input file from which previous history should be taken </param>
	/// <param name="outfile"> output file to which to write </param>
	/// <param name="theReservoirs"> list of reservoirs to print </param>
	/// <param name="newComments"> addition comments which should be included in history </param>
	/// <param name="useDailyData"> whether to use daily data </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeStateModFile(String infile, String outfile, java.util.List<StateMod_Reservoir> theReservoirs, java.util.List<String> newComments, boolean useDailyData) throws Exception
	public static void writeStateModFile(string infile, string outfile, IList<StateMod_Reservoir> theReservoirs, IList<string> newComments, bool useDailyData)
	{
		string routine = "StateMod_Reservoirs.writeStateModFile";
		IList<string> commentIndicators = new List<string>(1);
		commentIndicators.Add("#");
		IList<string> ignoredCommentIndicators = new List<string>(1);
		ignoredCommentIndicators.Add("#>");
		PrintWriter @out = null;

		if (Message.isDebugOn)
		{
			Message.printDebug(1, routine, "in writeStateModFile printing file: " + outfile);
		}

		try
		{
			@out = IOUtil.processFileHeaders(IOUtil.getPathUsingWorkingDir(infile), IOUtil.getPathUsingWorkingDir(outfile), newComments, commentIndicators, ignoredCommentIndicators, 0);

			string iline = null;
			string cmnt = "#>";
			string format_0 = null;
			if (useDailyData)
			{
				format_0 = "%-12.12s%-24.24s%-12.12s%8d%#8.0f %-12.12s";
			}
			else
			{
				format_0 = "%-12.12s%-24.24s%-12.12s%8d%#8.0f";
			}
			string format_1 = "                        %#8.0f%#8.0f%#8.0f%#8.0f%8d%8d%8d%8d";
			string format_2 = "            %-12.12s%#8.0f%#8.0f%8.0f%8d";
			// TODO SAM 2007-03-01 Evaluate use
			//String format_3 = "            %-12.12s%#8.0f%#8.0f%8.0f%8d";
			string format_4 = "            %-12.12s%-12.12s%#8.0f";
			//String format_5 = "            %-12.12s%#8.0f%8.0f%8.0f";
			string desc = null;
			StateMod_Reservoir res = null;
			StateMod_ReservoirAreaCap ac = null;
			StateMod_ReservoirAccount own = null;
			StateMod_ReservoirClimate clmt = null;
			string ch1 = null;
			IList<object> v = new List<object>(6);
			IList<StateMod_ReservoirAccount> ownv = null;
			IList<StateMod_ReservoirClimate> climatev = null;
			IList<StateMod_ReservoirAreaCap> areacapv = null;

			int i, j = 0;

			// print out header
			@out.println(cmnt);
			@out.println(cmnt + " *************************************************************");
			@out.println(cmnt + "  StateMod Reservoir Station file");
			@out.println(cmnt);
			@out.println(cmnt + "  Card 1   format:  (a12, a24, a12, i8, f8.0, 1x, a12)");
			@out.println(cmnt);
			@out.println(cmnt + "  ID       cresid:  Reservoir Id");
			@out.println(cmnt + "  Name     resnam:  Reservoir name");
			@out.println(cmnt + "  Riv ID    cgoto:  Node where Reservoir is located");
			@out.println(cmnt + "  On/Off   iressw:  Switch 0 = off");
			@out.println(cmnt + "                           1 = on, do not adjust for dead storage");
			@out.println(cmnt + "                               do not store above rervoir targets");
			@out.println(cmnt + "                           2 = do not store above rervoir targets");
			@out.println(cmnt + "                               adjust maximum ownership and initial");
			@out.println(cmnt + "                               storage of the last account by the");
			@out.println(cmnt + "                               dead storage volume");
			@out.println(cmnt + "                           3 = on, do not adjust for dead storage");
			@out.println(cmnt + "                               do store above rervoir targets");
			@out.println(cmnt + "                               (Note: in conjunction with an");
			@out.println(cmnt + "                               operational release for targets this");
			@out.println(cmnt + "                               results in a 'paper fill' activity.)");
			@out.println(cmnt + "  Admin #   rdate:  Administration date for 1 fill rule");
			@out.println(cmnt + "  Daily ID cresdy:  Identifier for daily time series.");
			@out.println(cmnt);
			@out.println(cmnt + "  Card 2 format:  (24x, 4f8.0, 4i8)");
			@out.println(cmnt);
			@out.println(cmnt + "  VolMin   volmin:  Min storage (ac-ft)");
			@out.println(cmnt + "  VolMax   volmax:  Max storage (ac-ft)");
			@out.println(cmnt + "  FloMax   flomax:  Max discharge (cfs)");
			@out.println(cmnt + "  DeadSt   deadst:  Dead storage (ac-ft)");
			@out.println(cmnt + "  NumOwner nowner:  Number of owners");
			@out.println(cmnt + "  NumEva   nevapo:  Number of evaporation stations");
			@out.println(cmnt + "  NumPre   nprecp:  Number of precipitation stations");
			@out.println(cmnt + "  NumTable nrange:  Number of area capacity values");
			@out.println(cmnt);
			@out.println(cmnt + "  Card 3 format:  (12x, a12, 3f8.0, i8)");
			@out.println(cmnt);
			@out.println(cmnt + "  OwnName  ownnam:  Owner name");
			@out.println(cmnt + "  OwnMax   ownmax:  Maximum storage for that owner (ac-ft)");
			@out.println(cmnt + "  Sto-1    curown:  Initial storage for that owner (ac-ft)");
			@out.println(cmnt + "  EvapTyp  pcteva:  Evaporation distribution");
			@out.println(cmnt + "  FillTyp   n2own:  Ownership type 1=First fill; 2=Second fill");
			@out.println(cmnt);
			@out.println(cmnt + "  Card 4  format:  (24x, a12, f8.0)");
			@out.println(cmnt);
			@out.println(cmnt + "  Evap ID  cevar:  Evaporation station");
			@out.println(cmnt + "  EvapWt  weigev:  Evaporation station weight (%%)");
			@out.println(cmnt);
			@out.println(cmnt + "  Card 5 format:  (24x, a12, f8.0)");
			@out.println(cmnt);
			@out.println(cmnt + "  Prec ID   cprer:  Precipitation station");
			@out.println(cmnt + "  PrecWt   weigpr:  Precipitation station weight (%%)");
			@out.println(cmnt);
			@out.println(cmnt + "  Card 6 format:  (24x, 3f8.0)");
			@out.println(cmnt);
			@out.println(cmnt + "  Cont     conten:  Content (ac-ft)");
			@out.println(cmnt + "  Area    surarea:  Area (ac)");
			@out.println(cmnt + "  Seep    seepage:  Seepage (ac-ft)");
			@out.println(cmnt);
			@out.println(cmnt + " *************************************************************");
			@out.println(cmnt);

			@out.println(cmnt + "    ID              Name              Node     On/Off  RDate       DailyID ");
			@out.println(cmnt + "---------eb----------------------eb----------eb------eb------exb----------e");
			@out.println(cmnt + "                       VolMin  VolMax  FloMax  DeadSt NumOwner NumEva  NumPre NumTable");
			@out.println(cmnt + "xxxxxxxxxxxxxxxxxxxxxxb------eb------eb------eb------eb------eb------eb------eb------e");
			@out.println(cmnt + "                         OwnName   OwnMax   Sto-1 EvapTyp FillTyp");
			@out.println(cmnt + "xxxxxxxxxxxxxxxxxxxxxxb----------eb------eb------eb------eb------e");
			@out.println(cmnt + "                        Evap Id    EvapWt ");
			@out.println(cmnt + "xxxxxxxxxxxxxxxxxxxxxxb----------eb------e");
			@out.println(cmnt + "                        Prec Id    PrecWt ");
			@out.println(cmnt + "xxxxxxxxxxxxxxxxxxxxxxb----------eb------e");
			@out.println(cmnt + "                        Cont    Area    Seep  ");
			@out.println(cmnt + "xxxxxxxxxxxxxxxxxxxxxxb------eb------eb------e");
			@out.println(cmnt + "EndHeader");
			@out.println(cmnt);

			int num = 0;
			if (theReservoirs != null)
			{
				num = theReservoirs.Count;
			}
			int nevap, nptpx, nareacap, nclmt, nowner;
			for (i = 0; i < num; i++)
			{
				res = theReservoirs[i];
				if (res == null)
				{
					continue;
				}

				v.Clear();
				v.Add(res.getID());
				v.Add(res.getName());
				v.Add(res.getCgoto());
				v.Add(new int?(res.getSwitch()));
				v.Add(new double?(res.getRdate()));
				if (useDailyData)
				{
					v.Add(res.getCresdy());
				}
				iline = StringUtil.formatString(v, format_0);
				@out.println(iline);

				// print reservoir statics: min, max, maxrelease, dead storage,
				// #owners, #evaps ...
				// count the number climate stations which are evap vs precip
				nevap = StateMod_ReservoirClimate.getNumEvap(res.getClimates());
				nptpx = StateMod_ReservoirClimate.getNumPrecip(res.getClimates());
				if (Message.isDebugOn)
				{
					Message.printDebug(50, routine, "nevap: " + nevap + " nptpx: " + nptpx);
				}
				v.Clear();
				v.Add(new double?(res.getVolmin()));
				v.Add(new double?(res.getVolmax()));
				v.Add(new double?(res.getFlomax()));
				v.Add(new double?(res.getDeadst()));
				v.Add(new int?(res.getNowner()));
				v.Add(new int?(nevap));
				v.Add(new int?(nptpx));
				v.Add(new int?(res.getNrange()));
				iline = StringUtil.formatString(v, format_1);
				@out.println(iline);

				// print the owner information 
				ownv = res.getAccounts();
				nowner = ownv.Count;
				for (j = 0; j < nowner; j++)
				{
					own = ownv[j];
					if (own == null)
					{
						@out.println();
						continue;
					}
					desc = own.getName();
					if (desc.Length == 0)
					{
						desc = "Account " + (j + 1);
					}
					v.Clear();
					v.Add(desc);
					v.Add(new double?(own.getOwnmax()));
					v.Add(new double?(own.getCurown()));
					v.Add(new double?(own.getPcteva()));
					v.Add(new int?(own.getN2own()));
					iline = StringUtil.formatString(v, format_2);
					@out.println(iline);
				}

				// print the evap information
				climatev = res.getClimates();
				nclmt = climatev.Count;
				for (j = 0; j < nclmt; j++)
				{
					clmt = climatev[j];
					if (clmt == null)
					{
						@out.println();
						continue;
					}
					if (clmt.getType() == StateMod_ReservoirClimate.CLIMATE_EVAP)
					{
						v.Clear();
						v.Add("Evaporation");
						v.Add(clmt.getID());
						v.Add(new double?(clmt.getWeight()));
						iline = StringUtil.formatString(v, format_4);
						@out.println(iline);
					}
				}

				// Print the precip information
				for (j = 0; j < nclmt; j++)
				{
					clmt = climatev[j];
					if (clmt == null)
					{
						@out.println();
						continue;
					}
					if (clmt.getType() == StateMod_ReservoirClimate.CLIMATE_PTPX)
					{
						v.Clear();
						v.Add("Precipitatn");
						v.Add(clmt.getID());
						v.Add(new double?(clmt.getWeight()));
						iline = StringUtil.formatString(v, format_4);
						@out.println(iline);
					}
				}

				// print the area capacity information
				areacapv = res.getAreaCaps();
				nareacap = areacapv.Count;
				for (j = 0; j < nareacap; j++)
				{
					ac = areacapv[j];
					if (ac == null)
					{
						@out.println();
						continue;
					}
					ch1 = "CAP-AREA" + StringUtil.formatString(j, "%3.3s");
					iline = StringUtil.formatString(ch1, "            %-12.12s");
					// Not very efficient but this file does not get written often...
					if (ac.getConten() < 100.0)
					{
						iline += StringUtil.formatString(ac.getConten(), "%8.2f");
					}
					else
					{
						iline += StringUtil.formatString(ac.getConten(), "%#8.0f");
					}
					if (ac.getSurarea() < 100.0)
					{
						iline += StringUtil.formatString(ac.getSurarea(), "%8.2f");
					}
					else
					{
						iline += StringUtil.formatString(ac.getSurarea(), "%8.0f");
					}
					iline += StringUtil.formatString(ac.getSeepage(),"%8.0f");
					@out.println(iline);
				}
			}
		}
		catch (Exception e)
		{
			Message.printWarning(3, routine, e);
			throw e;
		}
		finally
		{
			if (@out != null)
			{
				@out.flush();
				@out.close();
			}
		}
	}

	/// <summary>
	/// Writes a list of StateMod_Reservoir objects to a list file.  A header is 
	/// printed to the top of the file, containing the commands used to generate the 
	/// file.  Any strings in the body of the file that contain the field delimiter 
	/// will be wrapped in "...".  <para>This method also writes out Reservoir Area Cap,
	/// Account, Climate Data and Collections data to separate files.  If this method 
	/// is called with a filename parameter of "reservoirs.txt", six files will be generated:
	/// - reservoirs.txt
	/// - reservoirs_Accounts.txt
	/// - reservoirs_ContentAreaSeepage.txt
	/// - reservoirs_EvapStations.txt
	/// - reservoirs_PrecipStations.txt
	/// - reservoirs_Collections.txt
	/// </para>
	/// </summary>
	/// <param name="filename"> the name of the file to which the data will be written. </param>
	/// <param name="delimiter"> the delimiter to use for separating field values. </param>
	/// <param name="update"> whether to update an existing file, retaining the current 
	/// header (true) or to create a new file with a new header. </param>
	/// <param name="data"> the list of objects to write. </param>
	/// <param name="newComments"> comments to add to the top of the file. </param>
	/// <returns> a list of files that were actually written, because this method controls all the secondary
	/// filenames. </returns>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.List<java.io.File> writeListFile(String filename, String delimiter, boolean update, java.util.List<StateMod_Reservoir> data, java.util.List<String> newComments) throws Exception
	public static IList<File> writeListFile(string filename, string delimiter, bool update, IList<StateMod_Reservoir> data, IList<string> newComments)
	{
		string routine = "StateMod_Reservoir.writeListFile";
		int size = 0;
		if (data != null)
		{
			size = data.Count;
		}

		IList<string> fields = new List<string>();
		fields.Add("ID");
		fields.Add("Name");
		fields.Add("RiverNodeID");
		fields.Add("OnOff");
		fields.Add("OneFillRule");
		fields.Add("ContentMin");
		fields.Add("ContentMax");
		fields.Add("ReleaseMax");
		fields.Add("DeadStorage");
		fields.Add("DailyID");
		fields.Add("NumOwners");
		fields.Add("NumEvapStations");
		fields.Add("NumPrecipStations");
		fields.Add("NumCurveRows");
		int fieldCount = fields.Count;

		string[] names = new string[fieldCount];
		string[] formats = new string[fieldCount];
		int comp = StateMod_DataSet.COMP_RESERVOIR_STATIONS;
		string s = null;
		for (int i = 0; i < fieldCount; i++)
		{
			s = fields[i];
			names[i] = StateMod_Util.lookupPropValue(comp, "FieldName", s);
			formats[i] = StateMod_Util.lookupPropValue(comp, "Format", s);
		}

		string oldFile = null;
		if (update)
		{
			oldFile = IOUtil.getPathUsingWorkingDir(filename);
		}

		int j = 0;
		int nAreaCaps = 0;
		int size2 = 0;
		PrintWriter @out = null;
		StateMod_Reservoir res = null;
		StateMod_ReservoirAccount account = null;
		StateMod_ReservoirAreaCap areaCap = null;
		StateMod_ReservoirClimate climate = null;
		IList<string> commentIndicators = new List<string>(1);
		commentIndicators.Add("#");
		IList<string> ignoredCommentIndicators = new List<string>(1);
		ignoredCommentIndicators.Add("#>");
		string[] line = new string[fieldCount];
		StringBuilder buffer = new StringBuilder();
		IList<StateMod_ReservoirAccount> accounts = new List<StateMod_ReservoirAccount>();
		IList<StateMod_ReservoirAreaCap> areaCaps = new List<StateMod_ReservoirAreaCap>();
		IList<StateMod_ReservoirClimate> evapClimates = new List<StateMod_ReservoirClimate>();
		IList<StateMod_ReservoirClimate> precipClimates = new List<StateMod_ReservoirClimate>();
		IList<StateMod_ReservoirAccount> tempVAccounts = null;
		IList<StateMod_ReservoirAreaCap> tempVAreaCap = null;
		IList<StateMod_ReservoirClimate> tempClimates = null;

		try
		{
			// Add some basic comments at the top of the file.  Do this to a copy of the
			// incoming comments so that they are not modified in the calling code.
			IList<string> newComments2 = null;
			if (newComments == null)
			{
				newComments2 = new List<string>();
			}
			else
			{
				newComments2 = new List<string>(newComments);
			}
			newComments2.Insert(0,"");
			newComments2.Insert(1,"StateMod reservoir stations as a delimited list file.");
			newComments2.Insert(2,"See also the associated account, precipitation station, evaporation station,");
			newComments2.Insert(3,"content/area/seepage, and collection files.");
			newComments2.Insert(4,"");
			@out = IOUtil.processFileHeaders(oldFile, IOUtil.getPathUsingWorkingDir(filename), newComments2, commentIndicators, ignoredCommentIndicators, 0);

			for (int i = 0; i < fieldCount; i++)
			{
				buffer.Append("\"" + names[i] + "\"");
				if (i < (fieldCount - 1))
				{
					buffer.Append(delimiter);
				}
			}

			@out.println(buffer.ToString());

			for (int i = 0; i < size; i++)
			{
				res = data[i];

				line[0] = StringUtil.formatString(res.getID(),formats[0]).Trim();
				line[1] = StringUtil.formatString(res.getName(),formats[1]).Trim();
				line[2] = StringUtil.formatString(res.getCgoto(),formats[2]).Trim();
				line[3] = StringUtil.formatString(res.getSwitch(),formats[3]).Trim();
				line[4] = StringUtil.formatString(res.getRdate(), formats[4]).Trim();
				line[5] = StringUtil.formatString(res.getVolmin(),formats[5]).Trim();
				line[6] = StringUtil.formatString(res.getVolmax(),formats[6]).Trim();
				line[7] = StringUtil.formatString(res.getFlomax(),formats[7]).Trim();
				line[8] = StringUtil.formatString(res.getDeadst(),formats[8]).Trim();
				line[9] = StringUtil.formatString(res.getCresdy(),formats[9]).Trim();
				line[10] = StringUtil.formatString(res.getNowner(),formats[10]).Trim();
				line[11] = StringUtil.formatString(StateMod_ReservoirClimate.getNumEvap(res.getClimates()),formats[11]).Trim();
				line[12] = StringUtil.formatString(StateMod_ReservoirClimate.getNumPrecip(res.getClimates()),formats[12]).Trim();

				tempVAreaCap = res.getAreaCaps();
				 if (tempVAreaCap == null)
				 {
					nAreaCaps = 0;
				 }
				else
				{
					nAreaCaps = tempVAreaCap.Count;
				}
				line[13] = StringUtil.formatString(nAreaCaps,formats[13]).Trim();

				buffer = new StringBuilder();
				for (j = 0; j < fieldCount; j++)
				{
					if (j > 0)
					{
						buffer.Append(delimiter);
					}
					if (line[j].IndexOf(delimiter, StringComparison.Ordinal) > -1)
					{
						line[j] = "\"" + line[j] + "\"";
					}
					buffer.Append(line[j]);
				}

				tempVAccounts = res.getAccounts();
				size2 = tempVAccounts.Count;
				for (j = 0; j < size2; j++)
				{
					account = tempVAccounts[j];
					account.setCgoto(res.getID());
					accounts.Add(account);
				}

				tempVAreaCap = res.getAreaCaps();
				size2 = tempVAreaCap.Count;
				for (j = 0; j < size2; j++)
				{
					areaCap = tempVAreaCap[j];
					areaCap.setCgoto(res.getID());
					areaCaps.Add(areaCap);
				}

				tempClimates = res.getClimates();
				size2 = tempClimates.Count;
				for (j = 0; j < size2; j++)
				{
					climate = tempClimates[j];
					climate.setCgoto(res.getID());
					if (climate.getType() == StateMod_ReservoirClimate.CLIMATE_PTPX)
					{
						precipClimates.Add(climate);
					}
					else
					{
						evapClimates.Add(climate);
					}
				}

				@out.println(buffer.ToString());
			}
		}
		catch (Exception e)
		{
			Message.printWarning(3, routine, e);
			throw e;
		}
		finally
		{
			if (@out != null)
			{
				@out.flush();
				@out.close();
			}
		}

		int lastIndex = filename.LastIndexOf(".", StringComparison.Ordinal);
		string front = filename.Substring(0, lastIndex);
		string end = filename.Substring((lastIndex + 1), filename.Length - (lastIndex + 1));

		string accountFilename = front + "_Accounts." + end;
		StateMod_ReservoirAccount.writeListFile(accountFilename, delimiter, update, accounts, newComments);

		string areaCapFilename = front + "_ContentAreaSeepage." + end;
		StateMod_ReservoirAreaCap.writeListFile(areaCapFilename, delimiter, update, areaCaps, newComments);

		string evapClimateFilename = front + "_EvapStations." + end;
		StateMod_ReservoirClimate.writeListFile(evapClimateFilename, delimiter, update, evapClimates, newComments, StateMod_DataSet.COMP_RESERVOIR_STATION_EVAP_STATIONS);

		string precipClimateFilename = front + "_PrecipStations." + end;
		StateMod_ReservoirClimate.writeListFile(precipClimateFilename, delimiter, update, precipClimates, newComments, StateMod_DataSet.COMP_RESERVOIR_STATION_PRECIP_STATIONS);

		string collectionFilename = front + "_Collections." + end;
		writeCollectionListFile(collectionFilename, delimiter, update, data, newComments);

		IList<File> filesWritten = new List<File>();
		filesWritten.Add(new File(filename));
		filesWritten.Add(new File(accountFilename));
		filesWritten.Add(new File(areaCapFilename));
		filesWritten.Add(new File(precipClimateFilename));
		filesWritten.Add(new File(evapClimateFilename));
		filesWritten.Add(new File(collectionFilename));
		return filesWritten;
	}

	/// <summary>
	/// Writes the collection data from a list of StateMod_Reservoir objects to a 
	/// list file.  A header is printed to the top of the file, containing the commands 
	/// used to generate the file.  Any strings in the body of the file that contain 
	/// the field delimiter will be wrapped in "...". </summary>
	/// <param name="filename"> the name of the file to which the data will be written. </param>
	/// <param name="delimiter"> the delimiter to use for separating field values. </param>
	/// <param name="update"> whether to update an existing file, retaining the current 
	/// header (true) or to create a new file with a new header. </param>
	/// <param name="data"> the list of objects to write. </param>
	/// <param name="newComments"> list of comments to add to the top of the file. </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeCollectionListFile(String filename, String delimiter, boolean update, java.util.List<StateMod_Reservoir> data, java.util.List<String> newComments) throws Exception
	public static void writeCollectionListFile(string filename, string delimiter, bool update, IList<StateMod_Reservoir> data, IList<string> newComments)
	{
		string routine = "StateMod_Reservoir.writeCollectionListFile";
		int size = 0;
		if (data != null)
		{
			size = data.Count;
		}

		IList<string> fields = new List<string>();
		fields.Add("LocationID");
		fields.Add("Year");
		fields.Add("CollectionType");
		fields.Add("PartType");
		fields.Add("PartID");
		int fieldCount = fields.Count;

		string[] names = new string[fieldCount];
		string[] formats = new string[fieldCount];
		int comp = StateMod_DataSet.COMP_RESERVOIR_STATION_COLLECTIONS;
		string s = null;
		for (int i = 0; i < fieldCount; i++)
		{
			s = fields[i];
			names[i] = StateMod_Util.lookupPropValue(comp, "FieldName", s);
			formats[i] = StateMod_Util.lookupPropValue(comp, "Format", s);
		}

		string oldFile = null;
		if (update)
		{
			oldFile = IOUtil.getPathUsingWorkingDir(filename);
		}

		int[] years = null;
		int numYears = 0;
		StateMod_Reservoir res = null;
		IList<string> commentIndicators = new List<string>(1);
		commentIndicators.Add("#");
		IList<string> ignoredCommentIndicators = new List<string>(1);
		ignoredCommentIndicators.Add("#>");
		string[] line = new string[fieldCount];
		string colType = null;
		string id = null;
		string partType = null;
		StringBuilder buffer = new StringBuilder();
		PrintWriter @out = null;
		IList<string> ids = null;

		try
		{
			// Add some basic comments at the top of the file.  However, do this to a copy of the
			// incoming comments so that they are not modified in the calling code.
			IList<string> newComments2 = null;
			if (newComments == null)
			{
				newComments2 = new List<string>();
			}
			else
			{
				newComments2 = new List<string>(newComments);
			}
			newComments2.Insert(0,"");
			newComments2.Insert(1,"StateMod reservoir station collection information as delimited list file.");
			newComments2.Insert(2,"See also the associated reservoir station file.");
			newComments2.Insert(3,"");
			@out = IOUtil.processFileHeaders(oldFile, IOUtil.getPathUsingWorkingDir(filename), newComments2, commentIndicators, ignoredCommentIndicators, 0);

			for (int i = 0; i < fieldCount; i++)
			{
				if (i > 0)
				{
					buffer.Append(delimiter);
				}
				buffer.Append("\"" + names[i] + "\"");
			}

			@out.println(buffer.ToString());

			for (int i = 0; i < size; i++)
			{
				res = data[i];
				id = res.getID();
				years = res.getCollectionYears();
				if (years == null)
				{
					numYears = 0;
				}
				else
				{
					numYears = years.Length;
				}
				colType = res.getCollectionType();
				partType = res.getCollectionPartType();
				//Message.printStatus(2, routine, "For " + id + " numYears=" + numYears );
				// Loop through the number of years of collection data...
				for (int iyear = 0; iyear < numYears; iyear++)
				{
					ids = res.getCollectionPartIDs(years[iyear]);
					//Message.printStatus(2, routine, "Part ids for year " + years[iyear] + " = " + ids );
					// Loop through the identifiers for the specific year
					for (int k = 0; k < ids.Count; k++)
					{
						line[0] = StringUtil.formatString(id,formats[0]).Trim();
						line[1] = StringUtil.formatString(years[iyear],formats[1]).Trim();
						line[2] = StringUtil.formatString(colType,formats[2]).Trim();
						line[3] = StringUtil.formatString(partType,formats[3]).Trim();
						line[4] = StringUtil.formatString(((string)(ids[k])),formats[4]).Trim();

						buffer = new StringBuilder();
						for (int ifield = 0; ifield < fieldCount; ifield++)
						{
							if (ifield > 0)
							{
								buffer.Append(delimiter);
							}
							if (line[ifield].IndexOf(delimiter, StringComparison.Ordinal) > -1)
							{
								line[ifield] = "\"" + line[ifield] + "\"";
							}
							buffer.Append(line[ifield]);
						}
						@out.println(buffer.ToString());
					}
				}
			}
		}
		catch (Exception e)
		{
			Message.printWarning(3, routine, e);
			throw e;
		}
		finally
		{
			if (@out != null)
			{
				@out.flush();
				@out.close();
			}
		}
	}

	}

}