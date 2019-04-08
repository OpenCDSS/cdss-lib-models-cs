using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

// StateMod_Diversion - class for diversion station

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
// StateMod_Diversion - class derived from StateMod_Data.  Contains information
//			read from the diversion file.
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
// 
// 19 Aug 1997	Catherine E.
//		Nutting-Lane, RTi	Created initial version of class
// 27 Mar 1998	CEN, RTi		Added pointers to TS.
// 06 Apr 1998	CEN, RTi		Added java style documentation.
// 21 Dec 1998	CEN, RTi		Added throws IOException to read/write
//					routines.
// 25 Oct 1999	CEN, RTi		Added daily diversion id.
//
// 01 Dec 1999	Steven A. Malers, RTi	Change so that connectAllTS is
//					overloaded to work the old way(no daily
//					time series)and with daily time series.
// 15 Feb 2001	SAM, RTi		Add use_daily_data flag to write methods
//					to allow writing old format files.
//					Change IO to IOUtil.  Add finalize();
//					Alphabetize methods.  Set unused
//					variables to null.  Add more checks for
//					null.  Update file output header.
//					In dumpDiversionsFile, reuse the vector
//					for output, optimize output formats
//					(don't need to format blank strings),
//					and remove debug statements.
// 2001-12-27	SAM, RTi		Update to use new fixedRead()to
//					improve performance.
// 2002-09-09	SAM, RTi		Add GeoRecord reference to allow 2-way
//					connection between spatial and StateMod
//					data.
// 2002-09-19	SAM, RTi		Use isDirty()instead of setDirty()to
//					indicate edits.
//					dds file.
//------------------------------------------------------------------------------
// 2003-06-04	J. Thomas Sapienza, RTi	Renamed from SMDiversion to 
//					StateMod_Diversion
// 2003-06-10	JTS, RTi		* Folded dumpDiversionsFile() into
//					  writeDiversionsFile()
//					* Renamed parseDiversionsFile() to
//					  readDiversionsFile()
// 2003-06-23	JTS, RTi		Renamed writeDiversionsFile() to
//					writeStateModFile()
// 2003-06-26	JTS, RTi		Renamed readDiversionsFile() to
//					readStateModFile()
// 2003-08-03	SAM, RTi		Change isDirty() to setDirty().
// 2003-08-14	SAM, RTi		Change GeoRecordNoSwing to GeoRecord.
// 2003-08-27	SAM, RTi		* Change default for cdividy to "0".
//					* Rework time series data members and
//					  methods to have better names,
//					  consistent with the data set
//					  components.
//					* Change so water rights are stored in
//					  a Vector, not an internally-maintained
//					  linked-list.
//					* Add all diversion data for the current
//					  StateMod design so that nothing is
//					  left out.
//					* Change connectDivRights() to
//					  connectRights().
//					* Change connectAllDivRights() to
//					  connectAllRights().
//					* Allow case-independent searches for
//					  time series identifiers.
//					* In addition to calling setDirty() on
//					  the data set component, do so on the
//					  individual objects.
//					* Clean up Javadoc.
//					* Remove data members for size of
//					  Vectors - the size can be determined
//					  from the Vectors.  Nduser is no longer
//					  used so don't need in any case.  Still
//					  output to allow file comparisons.
// 2003-09-30	SAM, RTi		Pass component type to
//					StateMod_ReturnFlow constructor.
// 2003-10-07	SAM, RTi		* As per Ray Bennett, default the demand
//					  source to 0, Unknown.
//					* Similarly, default efficiency is 60.
// 2003-10-10	SAM, RTi		Add disconnectRights().
// 2003-10-14	SAM, RTi		* Add a copy constructor for use by the
//					  StateMod_Diversion_JFrame to track
//					  edits.
//					* Change IWR to CWR (irrigation to
//					  consumptive) as per Ray Bennett
//					  feedback.
//					* Set the diversion dirty to false after
//					  read or construction - it may have
//					  been marked dirty with set() methods.
// 2003-10-21	SAM, RTi		Change demand override average monthly
//					to demand average monthly - more
//					consistent with documentation.
// 2004-02-25	SAM, RTi		Add isStateModDiversionFile().
// 2004-03-31	SAM, RTi		Print the line number and line when an
//					error occurs reading the file.
// 2004-04-12	SAM, RTi		* Change so read and write methods
//					  convert file paths using working
//					  directory.
// 2004-06-05	SAM, RTi		* Add methods to handle collections,
//					  similar to StateCU locations.
//					* Define static values here, that are
//					  possible values for some data members.
//					  These values were previously defined
//					  in StateMod_Diversion_JFrame.
//					* Add methods to retrieve the option
//					  strings.
// 2004-06-14	SAM, RTi		* Define public final int's for
//					  important demsrc values to help with
//					  StateDMI.
//					* Overload the constructor to allow
//					  initialization as completely missing
//					  or with reasonable defaults.  The
//					  former is better for StateDMI, the
//					  latter for StateMod GUI.
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
// 2004-08-16	SAM, RTi		* Output old Nduser as 1 instead of 0
//					  since that is what old files have.
// 2004-09-01	SAM, RTi		* Add the following for use with
//					  StateDMI only - no need to check for
//					  dirty - only set/gets on the entire
//					  array are enabled:
//						__cwr_monthly
//						__ddh_monthly
//						__calculated_efficiencies
//						__calculated_efficiency_stddevs
//						__model_efficiecies
// 2004-09-06	SAM, RTi		* Add "MultiStruct" to the types of
//					  collection.
// 2005-03-30	JTS, RTi		* Added getCollectionPartType().
//					* Added getCollectionYears().
// 2005-04-14	JTS, RTi		Added writeListFile().
// 2005-04-19	JTS, RTi		Added writeCollectionListFile().
// 2005-15-16	SAM, RTi		Overload setDiveff() to accept a
//					parameter indicating the year type of
//					the diversion stations file, to simplify
//					adjustments for water year, etc.
// 2006-04-09	SAM, RTi		Add _parcels_Vector data member and
//					associated methods, to help with
//					StateDMI error handling.
// 2007-04-12	Kurt Tometich, RTi		Added checkComponentData() and
//									getDataHeader() methods for check
//									file and data check support.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
//------------------------------------------------------------------------------
// EndHeader
// REVISIT SAM 2006-04-09
// The _parcel_Vector has minimal support and is not yet considered in
// copy, clone, equals, etc.

namespace DWR.StateMod
{

	using StateCU_IrrigationPracticeTS = DWR.StateCU.StateCU_IrrigationPracticeTS;
	using GeoRecord = RTi.GIS.GeoView.GeoRecord;
	using HasGeoRecord = RTi.GIS.GeoView.HasGeoRecord;
	using DayTS = RTi.TS.DayTS;
	using MonthTS = RTi.TS.MonthTS;
	using DataSetComponent = RTi.Util.IO.DataSetComponent;
	using IOUtil = RTi.Util.IO.IOUtil;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;
	using TimeUtil = RTi.Util.Time.TimeUtil;
	using YearType = RTi.Util.Time.YearType;

	/// <summary>
	/// Object used to store diversion information.  All set routines set
	/// the COMP_DIVERSION_STATIONS flag dirty.  A new object will have empty non-null
	/// lists, null time series, and defaults for all other data.
	/// </summary>
	public class StateMod_Diversion : StateMod_Data, ICloneable, IComparable<StateMod_Data>, StateMod_ComponentValidator, HasGeoRecord
	{

	/// <summary>
	/// Demand source values used by other software.  Most interaction is expected to occur through GUIs.
	/// </summary>
	public const int DEMSRC_UNKNOWN = 0;
	public const int DEMSRC_GIS = 1;
	public const int DEMSRC_TIA = 2;
	public const int DEMSRC_GIS_PRIMARY = 3;
	public const int DEMSRC_TIA_PRIMARY = 4;
	public const int DEMSRC_GIS_SECONDARY = 5;
	public const int DEMSRC_MI_TRANSBASIN = 6;
	public const int DEMSRC_CARRIER = 7;
	public const int DEMSRC_USER = 8;

	/// <summary>
	/// Daily diversion ID.
	/// </summary>
	protected internal string _cdividy;

	/// <summary>
	/// Diversion capacity
	/// </summary>
	protected internal double _divcap;

	/// <summary>
	/// User name
	/// </summary>
	protected internal string _username;

	/// <summary>
	/// data type switch
	/// </summary>
	protected internal int _idvcom;

	/// <summary>
	/// System efficiency switch
	/// </summary>
	protected internal double _divefc;

	/// <summary>
	/// Efficiency % by month.  The efficiencies are in the order of the calendar for
	/// the data set.  Therefore, for proper display, the calendar type must be known.
	/// </summary>
	protected internal double[] _diveff;

	// The following are used only by StateDMI and do not needed to be handled in
	// comparison, initialization, etc.
	private double[] __calculated_efficiencies = null;
	private double[] __calculated_efficiency_stddevs = null;
	private double[] __model_efficiencies = null;

	/// <summary>
	/// irrigated acreage, future
	/// </summary>
	protected internal double _area;

	/// <summary>
	/// use type
	/// </summary>
	protected internal int _irturn;

	/// <summary>
	/// river nodes receiving return flow
	/// </summary>
	protected internal IList<StateMod_ReturnFlow> _rivret;

	/// <summary>
	/// Direct diversions rights
	/// </summary>
	protected internal IList<StateMod_DiversionRight> _rights;

	/// <summary>
	/// Acreage source
	/// </summary>
	protected internal int _demsrc;

	/// <summary>
	/// Replacement code
	/// </summary>
	protected internal int _ireptype;

	/// <summary>
	/// Pointer to monthly demand ts.
	/// </summary>
	protected internal MonthTS _demand_MonthTS;

	/// <summary>
	/// Pointer to monthly demand override ts.
	/// </summary>
	protected internal MonthTS _demand_override_MonthTS;

	/// <summary>
	/// Pointer to average monthly demand override ts.
	/// </summary>
	protected internal MonthTS _demand_average_MonthTS;

	/// <summary>
	/// Pointer to daily demand ts.
	/// </summary>
	protected internal DayTS _demand_DayTS;

	/// <summary>
	/// Pointer to historical monthly diversion ts
	/// </summary>
	protected internal MonthTS _diversion_MonthTS;

	/// <summary>
	/// 12 monthly and annual average over period, used by StateDMI.
	/// </summary>
	private double[] __ddh_monthly = null;

	/// <summary>
	/// Pointer to historical daily diversion ts
	/// </summary>
	protected internal DayTS _diversion_DayTS;

	/// <summary>
	/// Pointer to monthly consumptive water requirement ts.
	/// </summary>
	protected internal MonthTS _cwr_MonthTS;

	/// <summary>
	/// 12 monthly and annual average over period, used by StateDMI
	/// </summary>
	private double[] __cwr_monthly = null;

	/// <summary>
	/// Pointer to daily consumptive water requirement ts.
	/// </summary>
	protected internal DayTS _cwr_DayTS;

	/// <summary>
	/// Pointer to the StateCU_IrrigationPracticeTS.  This object actually contains
	/// other time series, which can be retrieved for displays.
	/// </summary>
	protected internal StateCU_IrrigationPracticeTS _ipy_YearTS;

	/// <summary>
	/// Soil available water content, from StateCU file.
	/// </summary>
	// FIXME SAM 2009-06-22 Does not seem to be used anymore - is not read or written to file - need to evaluate
	protected internal double _awc;

	/// <summary>
	/// Reference to spatial data for this diversion -- currently NOT cloned.  If null, then no spatial data
	/// are available.
	/// </summary>
	protected internal GeoRecord _georecord = null;

	/// <summary>
	/// List of parcel data, in particular to allow StateDMI to detect when a diversion had no data.
	/// </summary>
	protected internal IList<StateMod_Parcel> _parcel_Vector = new List<StateMod_Parcel>();

	// Collections are set up to be specified by year, although currently for
	// diversions collections are always the same for the full period.

	/// <summary>
	/// Types of collections.  An aggregate merges the water rights whereas
	/// a system keeps all the water rights but just has one ID.  See email from Erin
	/// Wilson 2004-09-01, to reiterate current modeling procedures:
	/// <pre>
	/// <ol>
	/// <li>Multistructure should be used to represent two or more structures
	/// that divert from DIFFERENT TRIBUTARIES to serve the same demand
	/// (irrigated acreage or M&I demand).  In the Historic model used to
	/// estimate Baseflows, the historic diversions need to be represented on
	/// the correct tributary, so all structures are in the network.  Average
	/// efficiencies need to be set for these structures, since IWR has been
	/// assigned to only one structure.  In Baseline and Calculated mode, the
	/// multistruct(x,x) command will assign all demand to the primary structure
	/// and zero out the demand for the secondary structures.  Water rights will
	/// continue to be assigned to each individual structure, and operating
	/// rules need to be included to allow the model to divert from the
	/// secondary structure location (under their water right) to meet the
	/// primary structure demand.</li>
	/// <li>Divsystems should be used to represents two or more structures with
	/// intermingled lands and/or diversions that divert from the SAME
	/// TRIBUTARY.  Only the primary structure should be included in the
	/// network.  The Divsystem(x,x) command will combine historic diversions,
	/// capacities, and acreages for use in the Historic model and to create
	/// Baseflows.  Water rights for all structures will be assigned explicitly
	/// to the primary structure.  No operating rules or set efficiency commands are required.</li>
	/// <li>Aggregates.  The only difference between Divsystems and Aggregates
	/// is that the water rights are not necessarily assigned explicitly, but
	/// are generally grouped into water rights classes.</li>
	/// </pre>
	/// </summary>
	public static string COLLECTION_TYPE_AGGREGATE = "Aggregate";
	public static string COLLECTION_TYPE_SYSTEM = "System";
	public static string COLLECTION_TYPE_MULTISTRUCT = "MultiStruct";

	private string __collection_type = StateMod_Util.MISSING_STRING;

	/// <summary>
	/// Used by DMI software - currently no options.
	/// </summary>
	private string __collection_part_type = "Ditch";

	/// <summary>
	/// The identifiers for data that are collected - null if not a collection location.
	/// This is a list of lists where the __collection_year is the first dimension.
	/// This is ugly but need to use the code to see if it can be made cleaner.
	/// </summary>
	private IList<IList<string>> __collection_Vector = null;

	/// <summary>
	/// An array of years that correspond to the aggregate/system.  Ditches currently only have one year.
	/// </summary>
	private int[] __collection_year = null;

	/// <summary>
	/// Construct a new diversion and assign data to reasonable defaults.
	/// </summary>
	public StateMod_Diversion() : base()
	{
		initialize(true);
	}

	/// <summary>
	/// Construct a new diversion. </summary>
	/// <param name="initialize_defaults"> If true, assign data to reasonable defaults.
	/// If false, all data are set to missing. </param>
	public StateMod_Diversion(bool initialize_defaults) : base()
	{
		initialize(initialize_defaults);
	}

	/// <summary>
	/// Copy constructor. </summary>
	/// <param name="deep_copy"> If true, make a deep copy including secondary vectors of data.
	/// Currently only false is recognized, in which primitive data are copied.  This is
	/// suitable to allow the StateMod_Diversion_JFrame class to know when changes have
	/// been made to data on the main screen. </param>
	public StateMod_Diversion(StateMod_Diversion div, bool deep_copy) : this()
	{
		// Base class...
		// TODO
		// Local data members...
		_cdividy = div._cdividy;
		_divcap = div._divcap;
		_username = div._username;
		_idvcom = div._idvcom;
		_divefc = div._divefc;
		for (int i = 0; i < 12; i++)
		{
			_diveff[i] = div._diveff[i];
		}
		_area = div._area;
		_irturn = div._irturn;
		_rivret = div._rivret;
		_rights = div._rights;
		_demsrc = div._demsrc;
		_ireptype = div._ireptype;
		// For time series, the references are pointed to the original but data are not copied.
		_demand_MonthTS = div._demand_MonthTS;
		_demand_override_MonthTS = div._demand_override_MonthTS;
		_demand_average_MonthTS = div._demand_average_MonthTS;
		_demand_DayTS = div._demand_DayTS;
		_diversion_MonthTS = div._diversion_MonthTS;
		_diversion_DayTS = div._diversion_DayTS;
		_cwr_MonthTS = div._cwr_MonthTS;
		_cwr_DayTS = div._cwr_DayTS;
		_ipy_YearTS = div._ipy_YearTS;
		_awc = div._awc;
		_georecord = div._georecord;
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
	/// Add return flow node to the vector of return flow nodes. </summary>
	/// <param name="rivret"> return flow </param>
	public virtual void addReturnFlow(StateMod_ReturnFlow rivret)
	{
		if (rivret == null)
		{
			return;
		}
		_rivret.Add(rivret);
		setDirty(true);
		if (!_isClone && _dataset != null)
		{
			_dataset.setDirty(StateMod_DataSet.COMP_DIVERSION_STATIONS, true);
		}
	}

	/// <summary>
	/// Adds a right for the diversion.
	/// </summary>
	public virtual void addRight(StateMod_DiversionRight right)
	{
		if (right != null)
		{
			_rights.Add(right);
		}
		// No need to set dirty because right is not saved in station file.
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
		StateMod_Diversion d = (StateMod_Diversion)base.clone();
		d._isClone = true;

		// The following are not cloned because there is no need to.  
		// The cloned values are only used for comparing between the 
		// values that can be changed in a single GUI.  The following
		// Vectors' data have their changes committed in other GUIs.	
		d._rivret = _rivret;
		d._rights = _rights;

		if (_diveff == null)
		{
			d._diveff = null;
		}
		else
		{
			d._diveff = (double[])_diveff.Clone();
		}
		return d;
	}

	/// <summary>
	/// Compares this object to another StateMod_Diversion object. </summary>
	/// <param name="o"> the object to compare against. </param>
	/// <returns> 0 if they are the same, 1 if this object is greater than the other
	/// object, or -1 if it is less. </returns>
	public virtual int CompareTo(StateMod_Data data)
	{
		int res = base.CompareTo(data);
		if (res != 0)
		{
			return res;
		}

		StateMod_Diversion d = (StateMod_Diversion)data;
		bool emptyCdividy = false;
		bool emptyDCdividy = false;
		if (_cdividy.Equals(""))
		{
			emptyCdividy = true;
		}
		if (d._cdividy.Equals(""))
		{
			emptyDCdividy = true;
		}

		if (emptyCdividy && emptyDCdividy)
		{
		}
		else if (emptyCdividy)
		{
			res = d._cdividy.CompareTo("0");
			if (res != 0)
			{
				return res;
			}
		}
		else if (emptyDCdividy)
		{
			res = _cdividy.CompareTo("0");
			if (res != 0)
			{
				return res;
			}
		}
		else
		{
			res = _cdividy.CompareTo(d._cdividy);
			if (res != 0)
			{
				return res;
			}
		}

		if (_divcap < d._divcap)
		{
			return -1;
		}
		else if (_divcap > d._divcap)
		{
			return 1;
		}

		res = _username.CompareTo(d._username);
		if (res != 0)
		{
			return res;
		}

		if (_idvcom < d._idvcom)
		{
			return -1;
		}
		else if (_idvcom > d._idvcom)
		{
			return 1;
		}

		if (_divefc < d._divefc)
		{
			return -1;
		}
		else if (_divefc > d._divefc)
		{
			return 1;
		}

		if (_area < d._area)
		{
			return -1;
		}
		else if (_area > d._area)
		{
			return 1;
		}

		if (_irturn < d._irturn)
		{
			return -1;
		}
		else if (_irturn > d._irturn)
		{
			return 1;
		}

		if (_demsrc < d._demsrc)
		{
			return -1;
		}
		else if (_demsrc > d._demsrc)
		{
			return 1;
		}

		if (_ireptype < d._ireptype)
		{
			return -1;
		}
		else if (_ireptype > d._ireptype)
		{
			return 1;
		}

		if (_awc < d._awc)
		{
			return -1;
		}
		else if (_awc > d._awc)
		{
			return 1;
		}

		if (_diveff == null && d._diveff == null)
		{
			return 0;
		}
		else if (_diveff == null && d._diveff != null)
		{
			return -1;
		}
		else if (_diveff != null && d._diveff == null)
		{
			return 1;
		}
		else
		{
			int size1 = _diveff.Length;
			int size2 = d._diveff.Length;
			if (size1 < size2)
			{
				return -1;
			}
			else if (size1 > size2)
			{
				return 1;
			}

			for (int i = 0; i < size1; i++)
			{
				if (_diveff[i] < d._diveff[i])
				{
					return -1;
				}
				else if (_diveff[i] > d._diveff[i])
				{
					return 1;
				}
			}
		}

		return 0;
	}

	/// <summary>
	/// Given a list containing all the diversions and another list containing
	/// all the rights, creates a system of pointers to link the diversions to their associated rights.
	/// This routines doesn't add an element to an array.  The array
	/// already exists, we are just connecting next and previous pointers. </summary>
	/// <param name="diversions"> all diversions </param>
	/// <param name="rights"> all rights </param>
	public static void connectAllRights(IList<StateMod_Diversion> diversions, IList<StateMod_DiversionRight> rights)
	{
		if ((diversions == null) || (rights == null))
		{
			return;
		}
		int num_divs = diversions.Count;

		StateMod_Diversion div;
		for (int i = 0; i < num_divs; i++)
		{
			div = diversions[i];
			if (div == null)
			{
				continue;
			}
			div.connectRights(rights);
		}
	}

	/// <summary>
	/// Given a vector containing all the diversion objects and lists of time series
	/// objects, sets pointers from the diversions to the associated time series. </summary>
	/// <param name="diversions"> All diversions. </param>
	/// <param name="diversion_MonthTS"> list of monthly historical diversion time series, or null. </param>
	/// <param name="diversion_DayTS"> list of daily historical diversion time series, or null. </param>
	/// <param name="demand_MonthTS"> list of monthly demand time series, or null. </param>
	/// <param name="demand_override_MonthTS"> list of monthly override demand time series, or null. </param>
	/// <param name="demand_average_MonthTS"> list of average monthly override
	/// demand time series, or null. </param>
	/// <param name="ipy_YearTS"> list of yearly irrigation practice objects, containing 
	/// time series of efficiencies, etc. (StateCU_IrrigationPracticeTS), or null </param>
	/// <param name="cwr_MonthTS"> list of monthly consumptive water requirement time series, or null. </param>
	/// <param name="cwr_DayTS"> list of daily consumptive water requirement time series, or null. </param>
	public static void connectAllTS(IList<StateMod_Diversion> diversions, IList<MonthTS> diversion_MonthTS, IList<DayTS> diversion_DayTS, IList<MonthTS> demand_MonthTS, IList<MonthTS> demand_override_MonthTS, IList<MonthTS> demand_average_MonthTS, IList<DayTS> demand_DayTS, IList<StateCU_IrrigationPracticeTS> ipy_YearTS, IList<MonthTS> cwr_MonthTS, IList<DayTS> cwr_DayTS)
	{
		if (diversions == null)
		{
			return;
		}
		int i;
		int num_divs = diversions.Count;

		StateMod_Diversion div;
		for (i = 0; i < num_divs; i++)
		{
			div = diversions[i];
			if (div == null)
			{
				continue;
			}
			if (diversion_MonthTS != null)
			{
				div.connectDiversionMonthTS(diversion_MonthTS);
			}
			if (diversion_DayTS != null)
			{
				div.connectDiversionDayTS(diversion_DayTS);
			}
			if (demand_MonthTS != null)
			{
				div.connectDemandMonthTS(demand_MonthTS);
			}
			if (demand_override_MonthTS != null)
			{
				div.connectDemandOverrideMonthTS(demand_override_MonthTS);
			}
			if (demand_average_MonthTS != null)
			{
				div.connectDemandAverageMonthTS(demand_average_MonthTS);
			}
			if (demand_DayTS != null)
			{
				div.connectDemandDayTS(demand_DayTS);
			}
			if (ipy_YearTS != null)
			{
				div.connectIrrigationPracticeYearTS(ipy_YearTS);
			}
			if (cwr_MonthTS != null)
			{
				div.connectCWRMonthTS(cwr_MonthTS);
			}
			if (cwr_DayTS != null)
			{
				div.connectCWRDayTS(cwr_DayTS);
			}
		}
	}

	/// <summary>
	/// Connect daily CWR series pointer.  The connection is made using the value of "cdividy" for the diversion. </summary>
	/// <param name="tslist"> demand time series </param>
	public virtual void connectCWRDayTS(IList<DayTS> tslist)
	{
		if (tslist == null)
		{
			return;
		}
		_cwr_DayTS = null;
		int num_TS = tslist.Count;

		DayTS ts;
		for (int i = 0; i < num_TS; i++)
		{
			ts = tslist[i];
			if (ts == null)
			{
				return;
			}
			if (_cdividy.Equals(ts.getLocation(), StringComparison.OrdinalIgnoreCase))
			{
				_cwr_DayTS = ts;
				ts.setDescription(getName());
				break;
			}
		}
	}

	/// <summary>
	/// Connect monthly CWR time series pointer.  The time series name is set to that of the diversion. </summary>
	/// <param name="tslist"> Time series list. </param>
	public virtual void connectCWRMonthTS(IList<MonthTS> tslist)
	{
		if (tslist == null)
		{
			return;
		}
		int num_TS = tslist.Count;

		MonthTS ts;
		_cwr_MonthTS = null;
		for (int i = 0; i < num_TS; i++)
		{
			ts = tslist[i];
			if (ts == null)
			{
				continue;
			}
			if (_id.Equals(ts.getLocation(), StringComparison.OrdinalIgnoreCase))
			{
				_cwr_MonthTS = ts;
				ts.setDescription(getName());
				break;
			}
		}
	}

	/// <summary>
	/// Connect average monthly demand time series pointer.  The time series
	/// name is set to that of the diversion. </summary>
	/// <param name="tslist"> Time series list. </param>
	public virtual void connectDemandAverageMonthTS(IList<MonthTS> tslist)
	{
		if (tslist == null)
		{
			return;
		}
		int num_TS = tslist.Count;

		MonthTS ts;
		_demand_average_MonthTS = null;
		for (int i = 0; i < num_TS; i++)
		{
			ts = tslist[i];
			if (ts == null)
			{
				continue;
			}
			if (_id.Equals(ts.getLocation(), StringComparison.OrdinalIgnoreCase))
			{
				_demand_average_MonthTS = ts;
				ts.setDescription(getName());
				break;
			}
		}
	}

	/// <summary>
	/// Connect daily demand time series pointer.  The connection is made using the
	/// value of "cdividy" for the diversion. </summary>
	/// <param name="tslist"> demand time series </param>
	public virtual void connectDemandDayTS(IList<DayTS> tslist)
	{
		if (tslist == null)
		{
			return;
		}
		_demand_DayTS = null;
		int num_TS = tslist.Count;

		DayTS ts;
		for (int i = 0; i < num_TS; i++)
		{
			ts = tslist[i];
			if (ts == null)
			{
				return;
			}
			if (_cdividy.Equals(ts.getLocation(), StringComparison.OrdinalIgnoreCase))
			{
				_demand_DayTS = ts;
				ts.setDescription(getName());
				break;
			}
		}
	}

	/// <summary>
	/// Connect monthly demand time series pointer.  The time series name is set to that of the diversion. </summary>
	/// <param name="tslist"> Time series list. </param>
	public virtual void connectDemandMonthTS(IList<MonthTS> tslist)
	{
		if (tslist == null)
		{
			return;
		}
		int num_TS = tslist.Count;

		MonthTS ts;
		_demand_MonthTS = null;
		for (int i = 0; i < num_TS; i++)
		{
			ts = tslist[i];
			if (ts == null)
			{
				continue;
			}
			if (_id.Equals(ts.getLocation(), StringComparison.OrdinalIgnoreCase))
			{
				_demand_MonthTS = ts;
				ts.setDescription(getName());
				break;
			}
		}
	}

	/// <summary>
	/// Connect monthly demand override time series pointer.  The time series name is
	/// set to that of the diversion. </summary>
	/// <param name="tslist"> Time series list. </param>
	public virtual void connectDemandOverrideMonthTS(IList<MonthTS> tslist)
	{
		if (tslist == null)
		{
			return;
		}
		int num_TS = tslist.Count;

		MonthTS ts;
		_demand_override_MonthTS = null;
		for (int i = 0; i < num_TS; i++)
		{
			ts = tslist[i];
			if (ts == null)
			{
				continue;
			}
			if (_id.Equals(ts.getLocation(), StringComparison.OrdinalIgnoreCase))
			{
				_demand_override_MonthTS = ts;
				ts.setDescription(getName());
				break;
			}
		}
	}

	/// <summary>
	/// Connect historical daily diversion time series pointer. </summary>
	/// <param name="tslist"> Vector of historical daily diversion time series. </param>
	public virtual void connectDiversionDayTS(IList<DayTS> tslist)
	{
		if (tslist == null)
		{
			return;
		}
		int num_TS = tslist.Count;

		DayTS ts;
		_diversion_DayTS = null;
		for (int i = 0; i < num_TS; i++)
		{
			ts = tslist[i];
			if (ts == null)
			{
				continue;
			}
			if (_id.Equals(ts.getLocation(), StringComparison.OrdinalIgnoreCase))
			{
				_diversion_DayTS = ts;
				ts.setDescription(getName());
				break;
			}
		}
	}

	/// <summary>
	/// Connect historical monthly time series pointer. </summary>
	/// <param name="tslist"> list of historical monthly time series. </param>
	public virtual void connectDiversionMonthTS(IList<MonthTS> tslist)
	{
		if (tslist == null)
		{
			return;
		}
		int num_TS = tslist.Count;

		MonthTS ts;
		_diversion_MonthTS = null;
		for (int i = 0; i < num_TS; i++)
		{
			ts = tslist[i];
			if (ts == null)
			{
				continue;
			}
			if (_id.Equals(ts.getLocation(), StringComparison.OrdinalIgnoreCase))
			{
				_diversion_MonthTS = ts;
				ts.setDescription(getName());
				break;
			}
		}
	}

	/// <summary>
	/// Connect the irrigation practice TS object. </summary>
	/// <param name="tslist"> Time series list. </param>
	public virtual void connectIrrigationPracticeYearTS(IList<StateCU_IrrigationPracticeTS> tslist)
	{
		if (tslist == null)
		{
			return;
		}
		int num_TS = tslist.Count;

		_ipy_YearTS = null;
		StateCU_IrrigationPracticeTS ipy_YearTS;
		for (int i = 0; i < num_TS; i++)
		{
			ipy_YearTS = tslist[i];
			if (ipy_YearTS == null)
			{
				continue;
			}
			if (_id.Equals(ipy_YearTS.getID(), StringComparison.OrdinalIgnoreCase))
			{
				_ipy_YearTS = ipy_YearTS;
				break;
			}
		}
	}

	/// <summary>
	/// Connect the diversion rights with this diversion, comparing the "cgoto" for the
	/// right to the diversion identifier.  Multiple rights may be associated with a diversion. </summary>
	/// <param name="rights"> all rights. </param>
	public virtual void connectRights(IList<StateMod_DiversionRight> rights)
	{
		if (rights == null)
		{
			return;
		}
		int num_rights = rights.Count;

		StateMod_DiversionRight right;
		for (int i = 0; i < num_rights; i++)
		{
			right = rights[i];
			if (right == null)
			{
				continue;
			}
			if (_id.Equals(right.getCgoto(), StringComparison.OrdinalIgnoreCase))
			{
				_rights.Add(right);
			}
		}
	}

	/// <summary>
	/// Creates a copy of the object for later use in checking to see if it was changed in a GUI.
	/// </summary>
	public virtual void createBackup()
	{
		_original = (StateMod_Diversion)clone();
		((StateMod_Diversion)_original)._isClone = false;
		_isClone = true;
	}

	/// <summary>
	/// Delete return flow node at a specified index. </summary>
	/// <param name="index"> index desired to delete </param>
	public virtual void deleteReturnFlowAt(int index)
	{
		_rivret.RemoveAt(index);
		setDirty(true);
		if (!_isClone && _dataset != null)
		{
			_dataset.setDirty(StateMod_DataSet.COMP_DIVERSION_STATIONS, true);
		}
	}

	// TODO - in the GUI need to decide if the right is actually removed from the main list
	/// <summary>
	/// Remove right from list.  A comparison on the ID is made. </summary>
	/// <param name="right"> Right to remove.  Note that the right is only removed from the
	/// list for this diversion and must also be removed from the main diversion right list. </param>
	public virtual void disconnectRight(StateMod_DiversionRight right)
	{
		if (right == null)
		{
			return;
		}
		int size = _rights.Count;
		StateMod_DiversionRight right2;
		// Assume that more than on instance can exist, even though this is not allowed...
		for (int i = 0; i < size; i++)
		{
			right2 = (StateMod_DiversionRight)_rights[i];
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
	/// Clean up for garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~StateMod_Diversion()
	{
		_cdividy = null;
		_username = null;
		_rivret = null;
		_rights = null;
		_demand_DayTS = null;
		_diversion_MonthTS = null;
		_demand_MonthTS = null;
		_georecord = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Return the irrigated acreage.
	/// </summary>
	public virtual double getArea()
	{
		return _area;
	}

	/// <summary>
	/// Return the average monthly CWR (12 monthly values + annual average), for the
	/// data set calendar type.  This is ONLY used by StateDMI and does not need
	/// to be considered in comparison code.
	/// </summary>
	public virtual double [] getAverageMonthlyCWR()
	{
		return __cwr_monthly;
	}

	/// <summary>
	/// Return the average monthly historical diversion (12 monthly values + annual
	/// average), for the data set calendar type.  This is ONLY used by StateDMI and
	/// does not need to be considered in comparison code.
	/// </summary>
	public virtual double [] getAverageMonthlyHistoricalDiversions()
	{
		return __ddh_monthly;
	}

	/// <summary>
	/// Return the AWC (available water capacity).
	/// </summary>
	public virtual double getAWC()
	{
		return _awc;
	}

	/// <summary>
	/// Return the average monthly efficiencies calculated from CWR and historical
	/// diversions (12 monthly values + annual average), for the
	/// data set calendar type.  This is ONLY used by StateDMI and does not need
	/// to be considered in comparison code.
	/// </summary>
	public virtual double [] getCalculatedEfficiencies()
	{
		return __calculated_efficiencies;
	}

	/// <summary>
	/// Return the standard deviation of monthly efficiencies calculated from CWR and
	/// historical diversions (12 monthly values + annual average), for the
	/// data set calendar type.  This is ONLY used by StateDMI and does not need
	/// to be considered in comparison code.
	/// </summary>
	public virtual double [] getCalculatedEfficiencyStddevs()
	{
		return __calculated_efficiency_stddevs;
	}

	/// <summary>
	/// Return the daily id.
	/// </summary>
	public virtual string getCdividy()
	{
		return _cdividy;
	}

	/// <summary>
	/// Return the collection part ID list for the specific year.  For ditches, only one
	/// aggregate/system list is currently supported so the same information is returned
	/// regardless of the year value. </summary>
	/// <returns> the list of collection part IDS, or null if not defined. </returns>
	public virtual IList<string> getCollectionPartIDs(int year)
	{
		if (__collection_Vector.Count == 0)
		{
			return null;
		}
		//if ( __collection_part_type.equalsIgnoreCase("Ditch") ) {
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
	/// Returns the collection part type ("Ditch"). </summary>
	/// <returns> the collection part type ("Ditch"). </returns>
	public virtual string getCollectionPartType()
	{
		return __collection_part_type;
	}

	/// <summary>
	/// Return the collection type, "Aggregate", "System", or "MultiStruct". </summary>
	/// <returns> the collection type, "Aggregate", "System", or "MultiStruct". </returns>
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
	/// Get daily consumptive water requirement time series.
	/// </summary>
	public virtual DayTS getConsumptiveWaterRequirementDayTS()
	{
		return _cwr_DayTS;
	}

	/// <summary>
	/// Get monthly consumptive water requirement time series.
	/// </summary>
	public virtual MonthTS getConsumptiveWaterRequirementMonthTS()
	{
		return _cwr_MonthTS;
	}

	/// <summary>
	/// Returns the column headers for the specific data checked. </summary>
	/// <returns> List of column headers. </returns>
	public static string[] getDataHeader()
	{
		return new string[] {"Num", "Diversion ID", "Diversion Name"};
	}

	/// <summary>
	/// Get average monthly demand time series.
	/// </summary>
	public virtual MonthTS getDemandAverageMonthTS()
	{
		return _demand_average_MonthTS;
	}

	/// <summary>
	/// Get daily demand time series.
	/// </summary>
	public virtual DayTS getDemandDayTS()
	{
		return _demand_DayTS;
	}

	/// <summary>
	/// Get monthly demand time series.
	/// </summary>
	public virtual MonthTS getDemandMonthTS()
	{
		return _demand_MonthTS;
	}

	/// <summary>
	/// Get monthly demand override time series.
	/// </summary>
	public virtual MonthTS getDemandOverrideMonthTS()
	{
		return _demand_override_MonthTS;
	}

	/// <summary>
	/// Return the demand source.
	/// </summary>
	public virtual int getDemsrc()
	{
		return _demsrc;
	}

	/// <summary>
	/// Return a list of demand source option strings, for use in GUIs.
	/// The options are of the form "1" if include_notes is false and
	/// "1 - Irrigated acres from GIS", if include_notes is true. </summary>
	/// <returns> a list of demand source option strings, for use in GUIs. </returns>
	/// <param name="include_notes"> Indicate whether notes should be included. </param>
	public static IList<string> getDemsrcChoices(bool include_notes)
	{
		IList<string> v = new List<string>(8);
		v.Add("0 - Irrigated acres source unknown");
		v.Add("1 - Irrigated acres from GIS");
		v.Add("2 - Irrigated acres from structure file (tia)");
		v.Add("3 - Irr. acr. from GIS, primary comp. served by mult. structs");
		v.Add("4 - Same as 3 but data from struct. file (tia)");
		v.Add("5 - Irr. acr. from GIS, secondary comp. served by mult. structs");
		v.Add("6 - Municipal, industrial, or transmountain structure");
		v.Add("7 - Carrier structure (no irrigated acres)");
		v.Add("8 - Irrigated acres provided by user");
		if (!include_notes)
		{
			// Remove the trailing notes...
			int size = v.Count;
			for (int i = 0; i < size; i++)
			{
				v[i] = StringUtil.getToken((string)v[i], " ", 0, 0);
			}
		}
		return v;
	}

	/// <summary>
	/// Return the default demand source choice.  This can be used by GUI code
	/// to pick a default for a new diversion. </summary>
	/// <returns> the default demand source choice. </returns>
	public static string getDemsrcDefault(bool include_notes)
	{
		if (include_notes)
		{
			return "0 - Irrigated acres source unknown";
		}
		else
		{
			return "0";
		}
	}

	/// <summary>
	/// Return the diversion capacity.
	/// </summary>
	public virtual double getDivcap()
	{
		return _divcap;
	}

	/// <summary>
	/// Return the system efficiency switch.
	/// </summary>
	public virtual double getDivefc()
	{
		return _divefc;
	}

	/// <summary>
	/// Return the system efficiency for the specified month index.
	/// The efficiencies are stored in the order of the year for the data set.  For
	/// example, if water years are used, the first efficiency will be for October.  For
	/// calendar year, the first efficiency will be for January. </summary>
	/// <param name="index"> 0 based monthly index </param>
	public virtual double getDiveff(int index)
	{
		return _diveff[index];
	}

	/// <summary>
	/// Return the system efficiency for the specified month index, where the month
	/// is always for calendar year (0=January). </summary>
	/// <param name="index"> 0-based monthly index (0=January). </param>
	/// <param name="yeartype"> The year type for the diversion stations file (consistent with
	/// the control file for a full data set). </param>
	public virtual double getDiveff(int index, YearType yeartype)
	{ // Adjust the index if necessary based on the year type...
		if (yeartype == null)
		{
			// Assume calendar.
		}
		else if (yeartype == YearType.WATER)
		{
			index = TimeUtil.convertCalendarMonthToCustomMonth((index + 1), 10) - 1;
		}
		else if (yeartype == YearType.NOV_TO_OCT)
		{
			index = TimeUtil.convertCalendarMonthToCustomMonth((index + 1), 11) - 1;
		}
		return _diveff[index];
	}

	/// <summary>
	/// Get daily historical diversion time series.
	/// </summary>
	public virtual DayTS getDiversionDayTS()
	{
		return _diversion_DayTS;
	}

	/// <summary>
	/// Get monthly historical diversion time series.
	/// </summary>
	public virtual MonthTS getDiversionMonthTS()
	{
		return _diversion_MonthTS;
	}

	/// <summary>
	/// Get the geographical data associated with the diversion. </summary>
	/// <returns> the GeoRecord for the diversion. </returns>
	public virtual GeoRecord getGeoRecord()
	{
		return _georecord;
	}

	/// <summary>
	/// Return a list of on/off switch option strings, for use in GUIs.
	/// The options are of the form "0" if include_notes is false and
	/// "0 - Off", if include_notes is true. </summary>
	/// <returns> a list of on/off switch option strings, for use in GUIs. </returns>
	/// <param name="include_notes"> Indicate whether notes should be added after the parameter values. </param>
	public static IList<string> getIdivswChoices(bool include_notes)
	{
		IList<string> v = new List<string>(2);
		v.Add("0 - Off"); // Possible options are listed here.
		v.Add("1 - On");
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
	/// to pick a default for a new diversion. </summary>
	/// <returns> the default reservoir replacement choice. </returns>
	public static string getIdivswDefault(bool include_notes)
	{ // Make this aggree with the above method...
		if (include_notes)
		{
			return ("1 - On");
		}
		else
		{
			return "1";
		}
	}

	/// <summary>
	/// Return the data type switch.
	/// </summary>
	public virtual int getIdvcom()
	{
		return _idvcom;
	}

	/// <summary>
	/// Return a list of monthly demand type option strings, for use in GUIs.
	/// The options are of the form "1" if include_notes is false and
	/// "1 - Monthly total demand", if include_notes is true. </summary>
	/// <returns> a list of monthly demand type option strings, for use in GUIs. </returns>
	/// <param name="include_notes"> Indicate whether notes should be included. </param>
	public static IList<string> getIdvcomChoices(bool include_notes)
	{
		IList<string> v = new List<string>(5);
		v.Add("1 - Monthly total demand");
		v.Add("2 - Annual total demand");
		v.Add("3 - Monthly irrigation water requirement");
		v.Add("4 - Annual irrigation water requirement");
		v.Add("5 - Estimate to be zero");
		if (!include_notes)
		{
			// Remove the trailing notes...
			int size = v.Count;
			for (int i = 0; i < size; i++)
			{
				v[i] = StringUtil.getToken((string)v[i], " ", 0, 0);
			}
		}
		return v;
	}

	/// <summary>
	/// Return the default monthly demand type choice.  This can be used by GUI code
	/// to pick a default for a new diversion. </summary>
	/// <returns> the default monthly demand type choice. </returns>
	public static string getIdvcomDefault(bool include_notes)
	{
		if (include_notes)
		{
			return "1 - Monthly total demand";
		}
		else
		{
			return "1";
		}
	}

	/// <summary>
	/// Return the replacement code.
	/// </summary>
	public virtual int getIreptype()
	{
		return _ireptype;
	}

	/// <summary>
	/// Return a list of reservoir replacement option strings, for use in GUIs.
	/// The options are of the form "0" if include_notes is false and
	/// "0 - Do not provide replacement res. benefits", if include_notes is true. </summary>
	/// <returns> a list of reservoir replacement option strings, for use in GUIs. </returns>
	/// <param name="include_notes"> Indicate whether notes should be included. </param>
	public static IList<string> getIreptypeChoices(bool include_notes)
	{
		IList<string> v = new List<string>(3);
		v.Add("0 - Do not provide replacement res. benefits");
		v.Add("1 - Provide 100% replacement");
		v.Add("-1 - Provide depletion replacement");
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
	/// Return the default reservoir replacement choice.  This can be used by GUI code
	/// to pick a default for a new diversion. </summary>
	/// <returns> the default reservoir replacement choice. </returns>
	public static string getIreptypeDefault(bool include_notes)
	{
		if (include_notes)
		{
			return "-1 - Provide depletion replacement";
		}
		else
		{
			return "-1";
		}
	}

	/// <summary>
	/// Get yearly irrigation practice time series.
	/// </summary>
	public virtual StateCU_IrrigationPracticeTS getIrrigationPracticeYearTS()
	{
		return _ipy_YearTS;
	}

	/// <summary>
	/// Return the use type.
	/// </summary>
	public virtual int getIrturn()
	{
		return _irturn;
	}

	/// <summary>
	/// Return a list of use type option strings, for use in GUIs.
	/// The options are of the form "1" if include_notes is false and
	/// "1 - Irrigation", if include_notes is true. </summary>
	/// <returns> a list of use type option strings, for use in GUIs. </returns>
	/// <param name="include_notes"> Indicate whether notes should be included. </param>
	public static IList<string> getIrturnChoices(bool include_notes)
	{
		IList<string> v = new List<string>(6);
		v.Add("0 - Storage");
		v.Add("1 - Irrigation");
		v.Add("2 - Municipal");
		v.Add("3 - Carrier");
		v.Add("4 - Transmountain");
		v.Add("5 - Other");
		if (!include_notes)
		{
			// Remove the trailing notes...
			int size = v.Count;
			for (int i = 0; i < size; i++)
			{
				v[i] = StringUtil.getToken((string)v[i], " ", 0, 0);
			}
		}
		return v;
	}

	/// <summary>
	/// Return the default use type choice.  This can be used by GUI code
	/// to pick a default for a new diversion. </summary>
	/// <returns> the default use type choice. </returns>
	public static string getIrturnDefault(bool include_notes)
	{
		if (include_notes)
		{
			return "1 - Irrigation";
		}
		else
		{
			return "1";
		}
	}

	/// <summary>
	/// Get the last right associated with diversion.
	/// </summary>
	public virtual StateMod_DiversionRight getLastRight()
	{
		if ((_rights == null) || (_rights.Count == 0))
		{
			return null;
		}
		return (StateMod_DiversionRight)_rights[_rights.Count - 1];
	}

	/// <summary>
	/// Return the average monthly efficiencies to be used for modeling (
	/// 12 monthly values + annual average), for the
	/// data set calendar type.  This is ONLY used by StateDMI and does not need
	/// to be considered in comparison code.
	/// </summary>
	public virtual double [] getModelEfficiencies()
	{
		return __model_efficiencies;
	}

	/// <summary>
	/// Return the number of return flow locations.
	/// </summary>
	public virtual int getNrtn()
	{
		return _rivret.Count;
	}

	/// <summary>
	/// Return the list of parcels. </summary>
	/// <returns> the list of parcels. </returns>
	public virtual IList<StateMod_Parcel> getParcels()
	{
		return _parcel_Vector;
	}

	/// <summary>
	/// Return the return flow at a particular index. </summary>
	/// <param name="index"> index desired to retrieve. </param>
	public virtual StateMod_ReturnFlow getReturnFlow(int index)
	{
		return _rivret[index];
	}

	/// <summary>
	/// Retrieve the return flow vector.
	/// </summary>
	public virtual IList<StateMod_ReturnFlow> getReturnFlows()
	{
		return _rivret;
	}

	/// <summary>
	/// Return the right associated with the given index.  If index
	/// number of rights don't exist, null will be returned. </summary>
	/// <param name="index"> desired right index </param>
	public virtual StateMod_DiversionRight getRight(int index)
	{
		if ((index < 0) || (index >= _rights.Count))
		{
			return null;
		}
		else
		{
			return (StateMod_DiversionRight)_rights[index];
		}
	}

	/// <summary>
	/// Return the rights list. </summary>
	/// <returns> the rights list. </returns>
	public virtual IList<StateMod_DiversionRight> getRights()
	{
		return _rights;
	}

	/// <summary>
	/// Return the user name.
	/// </summary>
	public virtual string getUsername()
	{
		return _username;
	}

	/// <summary>
	/// Initialize data.  Sets the smdata_type to _dataset.COMP_DIVERSION_STATIONS. </summary>
	/// <param name="initialize_defaults"> If true, assign data to reasonable defaults.
	/// If false, all data are set to missing. </param>
	private void initialize(bool initialize_defaults)
	{
		_smdata_type = StateMod_DataSet.COMP_DIVERSION_STATIONS;
		if (initialize_defaults)
		{
			_divefc = -60.0; // Ray Bennett, Ray Alvarado, 2003-10-07 progress mtg.
			_diveff = new double[12];
			for (int i = 0; i < 12; i++)
			{
				_diveff[i] = 60.0; // See above
			}
			_username = "";
			_cdividy = "0"; // Use the average monthly TS for daily TS
			_divcap = 0;
			_idvcom = 1;
			_area = 0;
			_irturn = 1;
			_demsrc = DEMSRC_UNKNOWN;
			_ireptype = -1; // Provide depletion replacement
		}
		else
		{
			_divefc = StateMod_Util.MISSING_DOUBLE;
			_diveff = new double[12];
			for (int i = 0; i < 12; i++)
			{
				_diveff[i] = StateMod_Util.MISSING_DOUBLE;
			}
			_username = StateMod_Util.MISSING_STRING;
			_cdividy = StateMod_Util.MISSING_STRING;
			_divcap = StateMod_Util.MISSING_INT;
			_idvcom = StateMod_Util.MISSING_INT;
			_area = StateMod_Util.MISSING_DOUBLE;
			_irturn = StateMod_Util.MISSING_INT;
			_demsrc = StateMod_Util.MISSING_INT;
			_ireptype = StateMod_Util.MISSING_INT;
		}
		_rivret = new List<StateMod_ReturnFlow>();
		_rights = new List<StateMod_DiversionRight>();
		_diversion_MonthTS = null;
		_diversion_DayTS = null;
		_demand_MonthTS = null;
		_demand_override_MonthTS = null;
		_demand_average_MonthTS = null;
		_demand_DayTS = null;
		_ipy_YearTS = null;
		_cwr_MonthTS = null;
		_cwr_DayTS = null;
		_georecord = null;
	}

	/// <summary>
	/// Indicate whether the diversion is a collection (an aggregate or system). </summary>
	/// <returns> true if the diversion is an aggregate or system. </returns>
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
	/// Indicate whether a file is a StateMod diversion file.  Currently the only
	/// check that is done is to see if the file name ends in "dds". </summary>
	/// <param name="filename"> File name. </param>
	/// <returns> true if the file appears to be a diversion file, false if not. </returns>
	public static bool isStateModDiversionFile(string filename)
	{
		if (StringUtil.endsWithIgnoreCase(filename,".dds"))
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Read diversion information in and store in a java vector.
	/// The new diversions are added to the end of the previously stored diversions. </summary>
	/// <param name="filename"> filename containing diversion information </param>
	/// <exception cref="Exception"> if an error occurs </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.List<StateMod_Diversion> readStateModFile(String filename) throws Exception
	public static IList<StateMod_Diversion> readStateModFile(string filename)
	{
		string routine = "StateMod_Diversion.readStateModFile";
		string iline = null;
		IList<object> v = new List<object>(9);
		IList<StateMod_Diversion> theDiversions = new List<StateMod_Diversion>();
		int i;
		int linecount = 0;
		string s = null;

		int[] format_0 = new int[] {StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_INTEGER, StringUtil.TYPE_DOUBLE, StringUtil.TYPE_INTEGER, StringUtil.TYPE_INTEGER, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING};
		int[] format_0w = new int[] {12, 24, 12, 8, 8, 8, 8, 1, 12};
		int[] format_1 = new int[] {StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_INTEGER, StringUtil.TYPE_INTEGER, StringUtil.TYPE_DOUBLE, StringUtil.TYPE_DOUBLE, StringUtil.TYPE_INTEGER, StringUtil.TYPE_INTEGER};
		int[] format_1w = new int[] {12, 24, 12, 8, 8, 8, 8, 8, 8};
		int[] format_2 = new int[] {StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_DOUBLE, StringUtil.TYPE_INTEGER};
		int[] format_2w = new int[] {36, 12, 8, 8};

		StateMod_Diversion aDiversion = null;
		StateMod_ReturnFlow aReturnNode = null;
		StreamReader @in = null;

		Message.printStatus(1, routine, "Reading diversion file: " + filename);
		try
		{
			@in = new StreamReader(IOUtil.getPathUsingWorkingDir(filename));
			while (!string.ReferenceEquals((iline = @in.ReadLine()), null))
			{
				++linecount;
				// check for comments
				if (iline.StartsWith("#", StringComparison.Ordinal) || iline.Trim().Length == 0)
				{
					continue;
				}

				// allocate new diversion node
				aDiversion = new StateMod_Diversion();

				// line 1
				if (Message.isDebugOn)
				{
					Message.printDebug(50, routine, "line 1: " + iline);
				}
				StringUtil.fixedRead(iline, format_0, format_0w, v);
				aDiversion.setID(((string)v[0]).Trim());
				aDiversion.setName(((string)v[1]).Trim());
				aDiversion.setCgoto(((string)v[2]).Trim());
				aDiversion.setSwitch((int?)v[3]);
				aDiversion.setDivcap((double?)v[4]);
				aDiversion.setIreptype(((int?)v[6]));
				aDiversion.setCdividy(((string)v[8]).Trim());

				// line 2
				iline = @in.ReadLine();
				++linecount;
				if (Message.isDebugOn)
				{
					Message.printDebug(50, routine, "line 2: " + iline);
				}
				StringUtil.fixedRead(iline, format_1, format_1w, v);
				aDiversion.setUsername(((string)v[1]).Trim());
				aDiversion.setIdvcom(((int?)v[3]));
				int nrtn = ((int?)v[4]).Value;
				aDiversion.setDivefc(((double?)v[5]));
				aDiversion.setArea(((double?)v[6]));
				aDiversion.setIrturn(((int?)v[7]));
				aDiversion.setDemsrc(((int?)v[8]));

				// get the efficiency information
				if (aDiversion.getDivefc() < 0)
				{
					// Negative value indicates monthly efficiencies will follow...
					iline = @in.ReadLine();
					++linecount;
					// Free format...
					StringTokenizer split = new StringTokenizer(iline);
					if (split != null && split.countTokens() == 12)
					{
						for (i = 0; i < 12; i++)
						{
							aDiversion.setDiveff(i, split.nextToken());
						}
					}
				}
				else
				{
					// Annual efficiency so set monthly efficiencies to the annual...
					aDiversion.setDiveff(0,aDiversion.getDivefc());
					aDiversion.setDiveff(1,aDiversion.getDivefc());
					aDiversion.setDiveff(2,aDiversion.getDivefc());
					aDiversion.setDiveff(3,aDiversion.getDivefc());
					aDiversion.setDiveff(4,aDiversion.getDivefc());
					aDiversion.setDiveff(5,aDiversion.getDivefc());
					aDiversion.setDiveff(6,aDiversion.getDivefc());
					aDiversion.setDiveff(7,aDiversion.getDivefc());
					aDiversion.setDiveff(8,aDiversion.getDivefc());
					aDiversion.setDiveff(9,aDiversion.getDivefc());
					aDiversion.setDiveff(10,aDiversion.getDivefc());
					aDiversion.setDiveff(11,aDiversion.getDivefc());
				}

				// get the return information
				for (i = 0; i < nrtn; i++)
				{
					iline = @in.ReadLine();
					++linecount;
					StringUtil.fixedRead(iline, format_2, format_2w, v);
					aReturnNode = new StateMod_ReturnFlow(StateMod_DataSet.COMP_DIVERSION_STATIONS);
					s = ((string)v[1]).Trim();
					if (s.Length <= 0)
					{
						aReturnNode.setCrtnid(((string)v[0]).Trim());
						Message.printWarning(3, routine, "Return node for structure \"" + aDiversion.getID() + "\" is blank. ");
					}
					else
					{
						aReturnNode.setCrtnid(s);
					}

					aReturnNode.setPcttot(((double?)v[2]));
					aReturnNode.setIrtndl(((int?)v[3]));
					aDiversion.addReturnFlow(aReturnNode);
				}

				// Set the diversion to not dirty because it was just initialized...

				aDiversion.setDirty(false);

				// add the diversion to the vector of diversions
				theDiversions.Add(aDiversion);
			}
		}
		catch (Exception e)
		{
			Message.printWarning(3, routine, "Error reading line " + linecount + " \"" + iline + "\"");
			Message.printWarning(3, routine, e);
			throw e;
		}
		finally
		{
			if (@in != null)
			{
				@in.Close();
			}
		}
		return theDiversions;
	}

	/// <summary>
	/// Set the irrigated acreage. </summary>
	/// <param name="area"> acreage. </param>
	public virtual void setArea(double area)
	{
		if (_area != area)
		{
			_area = area;
			setDirty(true);
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_DIVERSION_STATIONS, true);
			}
		}
	}

	/// <summary>
	/// Set the irrigated acreage. </summary>
	/// <param name="area"> acreage. </param>
	public virtual void setArea(double? area)
	{
		setArea(area.Value);
	}

	/// <summary>
	/// Set the irrigated acreage. </summary>
	/// <param name="area"> acreage. </param>
	public virtual void setArea(string area)
	{
		if (string.ReferenceEquals(area, null))
		{
			return;
		}
		setArea(StringUtil.atod(area.Trim()));
	}

	/// <summary>
	/// Set the average monthly CWR (12 monthly values + annual average), for the
	/// data set calendar type.  This is ONLY used by StateDMI and does not need
	/// to be considered in comparison code.
	/// </summary>
	public virtual void setAverageMonthlyCWR(double[] cwr_monthly)
	{
		__cwr_monthly = cwr_monthly;
	}

	/// <summary>
	/// Set the average monthly historical diversions (12 monthly values + annual
	/// average), for the data set calendar type.  This is ONLY used by StateDMI and
	/// does not need to be considered in comparison code.
	/// </summary>
	public virtual void setAverageMonthlyHistoricalDiversions(double[] ddh_monthly)
	{
		__ddh_monthly = ddh_monthly;
	}

	/// <summary>
	/// Set the available water capacity. </summary>
	/// <param name="awc"> available water capacity. </param>
	public virtual void setAWC(double awc)
	{
		if (_awc != awc)
		{
			_awc = awc;
			setDirty(true);
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_DIVERSION_STATIONS, true);
			}
		}
	}

	/// <summary>
	/// Set the available water capacity. </summary>
	/// <param name="awc"> available water capacity. </param>
	public virtual void setAWC(double? awc)
	{
		setAWC(awc.Value);
	}

	/// <summary>
	/// Set the available water capacity. </summary>
	/// <param name="awc"> available water capacity. </param>
	public virtual void setAWC(string awc)
	{
		if (string.ReferenceEquals(awc, null))
		{
			return;
		}
		setAWC(StringUtil.atod(awc.Trim()));
	}

	/// <summary>
	/// Set the average monthly efficiencies calculated from CWR and historical
	/// diversions (12 monthly values + annual average), for the
	/// data set calendar type.  This is ONLY used by StateDMI and does not need
	/// to be considered in comparison code.
	/// </summary>
	public virtual void setCalculatedEfficiencies(double[] calculated_efficiencies)
	{
		__calculated_efficiencies = calculated_efficiencies;
	}

	/// <summary>
	/// Set the standard deviation of monthly efficiencies calculated from CWR and
	/// historical diversions (12 monthly values + annual average), for the
	/// data set calendar type.  This is ONLY used by StateDMI and does not need
	/// to be considered in comparison code.
	/// </summary>
	public virtual void setCalculatedEfficiencyStddevs(double[] calculated_efficiency_stddevs)
	{
		__calculated_efficiency_stddevs = calculated_efficiency_stddevs;
	}

	/// <summary>
	/// Set the daily id. </summary>
	/// <param name="cdividy"> daily id. </param>
	public virtual void setCdividy(string cdividy)
	{
		if (string.ReferenceEquals(cdividy, null))
		{
			return;
		}
		if (!cdividy.Equals(_cdividy))
		{
			_cdividy = cdividy;
			setDirty(true);
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_DIVERSION_STATIONS, true);
			}
		}
	}

	/// <summary>
	/// Return a list of daily ID option strings, for use in GUIs.
	/// The options are of the form "3" if include_notes is false and
	/// "3 - Daily time series are supplied", if include_notes is true. </summary>
	/// <returns> a list of daily ID option strings, for use in GUIs. </returns>
	/// <param name="include_notes"> Indicate whether notes should be included. </param>
	public static IList<string> getCdividyChoices(bool include_notes)
	{
		IList<string> v = new List<string>(8);
		v.Add("0 - Use monthly time series to get average daily values");
		v.Add("3 - Daily time series are supplied");
		v.Add("4 - Daily time series interpolated from midpoints of monthly data");
		if (!include_notes)
		{
			// Remove the trailing notes...
			int size = v.Count;
			for (int i = 0; i < size; i++)
			{
				v[i] = StringUtil.getToken((string)v[i], " ", 0, 0);
			}
		}
		return v;
	}

	/// <summary>
	/// Return the default daily ID choice.  This can be used by GUI code
	/// to pick a default for a new diversion. </summary>
	/// <returns> the default daily ID choice. </returns>
	public static string getCdividyDefault(bool include_notes)
	{
		if (include_notes)
		{
			return "0 - Use monthly time series to get average daily values";
		}
		else
		{
			return "0";
		}
	}

	/// <summary>
	/// Cancels any changes made to this object within a GUI since createBackup()
	/// was called and sets _original to null.
	/// </summary>
	public override void restoreOriginal()
	{
		StateMod_Diversion d = (StateMod_Diversion)_original;
		base.restoreOriginal();

		_diveff = d._diveff;
		_cdividy = d._cdividy;
		_divcap = d._divcap;
		_username = d._username;
		_idvcom = d._idvcom;
		_divefc = d._divefc;
		_area = d._area;
		_irturn = d._irturn;
		_demsrc = d._demsrc;
		_ireptype = d._ireptype;
		_awc = d._awc;
		_isClone = false;
		_original = null;
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
	/// <param name="collection_type"> The collection type, either
	/// COLLECTION_TYPE_AGGREGATE, COLLECTION_TYPE_SYSTEM, or COLLECTION_TYPE_MULTISTRUCT. </param>
	public virtual void setCollectionType(string collection_type)
	{
		__collection_type = collection_type;
	}

	/// <summary>
	/// Set the consumptive water requirement daily time series for the diversion structure.
	/// </summary>
	public virtual void setConsumptiveWaterRequirementDayTS(DayTS cwr_DayTS)
	{
		_cwr_DayTS = cwr_DayTS;
	}

	/// <summary>
	/// Set the consumptive water requirement monthly time series for the diversion structure.
	/// </summary>
	public virtual void setConsumptiveWaterRequirementMonthTS(MonthTS cwr_MonthTS)
	{
		_cwr_MonthTS = cwr_MonthTS;
	}

	/// <summary>
	/// Set the average monthly demand time series for the diversion structure.
	/// </summary>
	public virtual void setDemandAverageMonthTS(MonthTS demand_average_MonthTS)
	{
		_demand_average_MonthTS = demand_average_MonthTS;
	}

	/// <summary>
	/// Set the daily demand time series for the diversion structure.
	/// </summary>
	public virtual void setDemandDayTS(DayTS demand_DayTS)
	{
		_demand_DayTS = demand_DayTS;
	}

	/// <summary>
	/// Set the monthly demand time series for the diversion structure.
	/// </summary>
	public virtual void setDemandMonthTS(MonthTS demand_MonthTS)
	{
		_demand_MonthTS = demand_MonthTS;
	}

	/// <summary>
	/// Set the monthly demand override time series for the diversion structure.
	/// </summary>
	public virtual void setDemandOverrideMonthTS(MonthTS demand_override_MonthTS)
	{
		_demand_override_MonthTS = demand_override_MonthTS;
	}

	/// <summary>
	/// Set the demand source. </summary>
	/// <param name="demsrc"> acreage source. </param>
	public virtual void setDemsrc(int demsrc)
	{
		if (demsrc != _demsrc)
		{
			_demsrc = demsrc;
			setDirty(true);
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_DIVERSION_STATIONS, true);
			}
		}
	}

	/// <summary>
	/// Set the demand source. </summary>
	/// <param name="demsrc"> acreage source. </param>
	public virtual void setDemsrc(int? demsrc)
	{
		setDemsrc(demsrc.Value);
	}

	/// <summary>
	/// Set the demand source. </summary>
	/// <param name="demsrc"> demand source. </param>
	public virtual void setDemsrc(string demsrc)
	{
		if (string.ReferenceEquals(demsrc, null))
		{
			return;
		}
		setDemsrc(StringUtil.atoi(demsrc.Trim()));
	}

	/// <summary>
	/// Set the diversion capacity. </summary>
	/// <param name="divcap"> diversion capacity. </param>
	public virtual void setDivcap(double divcap)
	{
		if (divcap != _divcap)
		{
			_divcap = divcap;
			setDirty(true);
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_DIVERSION_STATIONS, true);
			}
		}
	}

	/// <summary>
	/// Set the diversion capacity. </summary>
	/// <param name="divcap"> diversion capacity. </param>
	public virtual void setDivcap(double? divcap)
	{
		setDivcap(divcap.Value);
	}

	/// <summary>
	/// Set the diversion capacity. </summary>
	/// <param name="divcap"> diversion capacity. </param>
	public virtual void setDivcap(string divcap)
	{
		if (!string.ReferenceEquals(divcap, null))
		{
			setDivcap(StringUtil.atod(divcap.Trim()));
		}
	}

	/// <summary>
	/// Set the system efficiency switch. </summary>
	/// <param name="divefc"> efficiency. </param>
	public virtual void setDivefc(double divefc)
	{
		if (divefc != _divefc)
		{
			_divefc = divefc;
			setDirty(true);
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_DIVERSION_STATIONS, true);
			}
		}
	}

	/// <summary>
	/// Set the system efficiency switch. </summary>
	/// <param name="divefc"> efficiency. </param>
	public virtual void setDivefc(double? divefc)
	{
		setDivefc(divefc.Value);
	}

	/// <summary>
	/// Set the system efficiency switch. </summary>
	/// <param name="divefc"> efficiency. </param>
	public virtual void setDivefc(string divefc)
	{
		if (string.ReferenceEquals(divefc, null))
		{
			return;
		}
		setDivefc(StringUtil.atod(divefc.Trim()));
	}

	/// <summary>
	/// Set the system efficiency for a particular month.
	/// The efficiencies are stored in the order of the year for the data set.  For
	/// example, if water years are used, the first efficiency will be for October.  For
	/// calendar year, the first efficiency will be for January. </summary>
	/// <param name="index"> month index (0+) </param>
	/// <param name="diveff"> monthly efficiency </param>
	public virtual void setDiveff(int index, double diveff)
	{
		if (_diveff[index] != diveff)
		{
			_diveff[index] = diveff;
			setDirty(true);
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_DIVERSION_STATIONS, true);
			}
		}
	}

	/// <summary>
	/// Set the system efficiency for a particular month.
	/// The efficiencies are specified with month 0 being January. </summary>
	/// <param name="index"> month index (0=January). </param>
	/// <param name="diveff"> monthly efficiency </param>
	/// <param name="yeartype"> The year type for the diversion stations file (consistent with
	/// the control file for a full data set).  </param>
	public virtual void setDiveff(int index, double diveff, YearType yeartype)
	{ // Adjust the index if necessary based on the year type...
		if (yeartype == null)
		{
			// Assume calendar.
		}
		else if (yeartype == YearType.WATER)
		{
			index = TimeUtil.convertCalendarMonthToCustomMonth((index + 1), 10) - 1;
		}
		else if (yeartype == YearType.NOV_TO_OCT)
		{
			index = TimeUtil.convertCalendarMonthToCustomMonth((index + 1), 11) - 1;
		}
		if (_diveff[index] != diveff)
		{
			_diveff[index] = diveff;
			setDirty(true);
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_DIVERSION_STATIONS, true);
			}
		}
	}

	/// <summary>
	/// Set the system efficiency for a particular month.
	/// The efficiencies are stored in the order of the year for the data set.  For
	/// example, if water years are used, the first efficiency will be for October.  For
	/// calendar year, the first efficiency will be for January. </summary>
	/// <param name="index"> month index </param>
	/// <param name="diveff"> monthly efficiency </param>
	public virtual void setDiveff(int index, double? diveff)
	{
		setDiveff(index, diveff.Value);
	}

	/// <summary>
	/// Set the system efficiency for a particular month.
	/// The efficiencies are stored in the order of the year for the data set.  For
	/// example, if water years are used, the first efficiency will be for October.  For
	/// calendar year, the first efficiency will be for January. </summary>
	/// <param name="index"> month index </param>
	/// <param name="diveff"> monthly efficiency </param>
	public virtual void setDiveff(int index, string diveff)
	{
		if (string.ReferenceEquals(diveff, null))
		{
			return;
		}
		setDiveff(index, StringUtil.atod(diveff.Trim()));
	}

	/// <summary>
	/// Set the historical daily diversion time series.
	/// </summary>
	public virtual void setDiversionDayTS(DayTS diversion_DayTS)
	{
		_diversion_DayTS = diversion_DayTS;
	}

	/// <summary>
	/// Set the historical monthly diversion time series.
	/// </summary>
	public virtual void setDiversionMonthTS(MonthTS diversion_MonthTS)
	{
		_diversion_MonthTS = diversion_MonthTS;
	}

	/// <summary>
	/// Set the geographic information object associated with the diversion. </summary>
	/// <param name="georecord"> Geographic record associated with the diversion. </param>
	public virtual void setGeoRecord(GeoRecord georecord)
	{
		_georecord = georecord;
	}

	/// <summary>
	/// Set the data type switch. </summary>
	/// <param name="idvcom"> data type switch. </param>
	public virtual void setIdvcom(int idvcom)
	{
		if (idvcom != _idvcom)
		{
			_idvcom = idvcom;
			setDirty(true);
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_DIVERSION_STATIONS, true);
			}
		}
	}

	/// <summary>
	/// Set the data type switch. </summary>
	/// <param name="idvcom"> data type switch. </param>
	public virtual void setIdvcom(int? idvcom)
	{
		setIdvcom(idvcom.Value);
	}

	/// <summary>
	/// Set the data type switch. </summary>
	/// <param name="idvcom"> data type switch. </param>
	public virtual void setIdvcom(string idvcom)
	{
		if (string.ReferenceEquals(idvcom, null))
		{
			return;
		}
		setIdvcom(StringUtil.atoi(idvcom.Trim()));
	}

	/// <summary>
	/// Set the replacement code. </summary>
	/// <param name="ireptype"> replacement code. </param>
	public virtual void setIreptype(int ireptype)
	{
		if (ireptype != _ireptype)
		{
			_ireptype = ireptype;
			setDirty(true);
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_DIVERSION_STATIONS, true);
			}
		}
	}

	/// <summary>
	/// Set the replacement code. </summary>
	/// <param name="ireptype"> replacement code. </param>
	public virtual void setIreptype(int? ireptype)
	{
		setIreptype(ireptype.Value);
	}

	/// <summary>
	/// Set the replacement code. </summary>
	/// <param name="ireptype"> replacement code. </param>
	public virtual void setIreptype(string ireptype)
	{
		if (string.ReferenceEquals(ireptype, null))
		{
			return;
		}
		setIreptype(StringUtil.atoi(ireptype.Trim()));
	}

	/// <summary>
	/// Set the use type. </summary>
	/// <param name="irturn"> use type. </param>
	public virtual void setIrturn(int irturn)
	{
		if (irturn != _irturn)
		{
			_irturn = irturn;
			setDirty(true);
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_DIVERSION_STATIONS, true);
			}
		}
	}

	/// <summary>
	/// Set the use type. </summary>
	/// <param name="irturn"> use type. </param>
	public virtual void setIrturn(int? irturn)
	{
		setIrturn(irturn.Value);
	}

	/// <summary>
	/// Set the use type. </summary>
	/// <param name="irturn"> use type. </param>
	public virtual void setIrturn(string irturn)
	{
		if (string.ReferenceEquals(irturn, null))
		{
			return;
		}
		setIrturn(StringUtil.atoi(irturn.Trim()));
	}

	/// <summary>
	/// Set the average monthly efficiencies to be used for modeling (
	/// 12 monthly values + annual average), for the
	/// data set calendar type.  This is ONLY used by StateDMI and does not need
	/// to be considered in comparison code.
	/// </summary>
	public virtual void setModelEfficiencies(double[] model_efficiencies)
	{
		__model_efficiencies = model_efficiencies;
	}

	/// <summary>
	/// Set the parcel list. </summary>
	/// <param name="parcel_Vector"> the list of StateMod_Parcel to set for parcel data. </param>
	internal virtual void setParcels(IList<StateMod_Parcel> parcel_Vector)
	{
		_parcel_Vector = parcel_Vector;
	}

	/// <summary>
	/// Sets the Return flow vector.
	/// </summary>
	public virtual void setReturnFlow(IList<StateMod_ReturnFlow> rivret)
	{
		_rivret = rivret;
	}

	/// <summary>
	/// Set the rights from a list of rights.  The linked list will be set up, too, 
	/// according to the order in the list.
	/// </summary>
	public virtual void setRightsVector(IList<StateMod_DiversionRight> rights)
	{
		_rights = rights;
	}

	/// <summary>
	/// Set the user name. </summary>
	/// <param name="username"> user name. </param>
	public virtual void setUsername(string username)
	{
		if (string.ReferenceEquals(username, null))
		{
			return;
		}
		if (!username.Equals(_username))
		{
			_username = username;
			setDirty(true);
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_DIVERSION_STATIONS, true);
			}
		}
	}

	public virtual StateMod_ComponentValidation validateComponent(StateMod_DataSet dataset)
	{
		StateMod_ComponentValidation validation = new StateMod_ComponentValidation();
		string id = getID();
		string name = getName();
		string riverID = getCgoto();
		double capacity = getDivcap();
		int ireptyp = getIreptype();
		string dailyID = getCdividy();
		string userName = getUsername();
		int idvcom = getIdvcom();
		int nrtn = getNrtn();
		double divefc = getDivefc();
		double area = getArea();
		int irturn = getIrturn();
		int demsrc = getDemsrc();
		// Make sure that basic information is not empty
		if (StateMod_Util.isMissing(id))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Diversion identifier is blank.", "Specify a station identifier."));
		}
		if (StateMod_Util.isMissing(name))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Diversion \"" + id + "\" name is blank.", "Specify a diversion name to clarify data."));
		}
		// Get the network list if available for checks below
		DataSetComponent comp = null;
		IList<StateMod_RiverNetworkNode> rinList = null;
		if (dataset != null)
		{
			comp = dataset.getComponentForComponentType(StateMod_DataSet.COMP_RIVER_NETWORK);
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<StateMod_RiverNetworkNode> rinList0 = (java.util.List<StateMod_RiverNetworkNode>)comp.getData();
			IList<StateMod_RiverNetworkNode> rinList0 = (IList<StateMod_RiverNetworkNode>)comp.getData();
			rinList = rinList0;
			if ((rinList != null) && (rinList.Count == 0))
			{
				// Set to null to simplify checks below
				rinList = null;
			}
		}
		if (StateMod_Util.isMissing(riverID))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Diversion \"" + id + "\" river ID is blank.", "Specify a river ID to associate the diversion with a river network node."));
		}
		else
		{
			// Verify that the river node is in the data set, if the network is available
			if (rinList != null)
			{
				if (StateMod_Util.IndexOf(rinList, riverID) < 0)
				{
					validation.add(new StateMod_ComponentValidationProblem(this,"Diversion \"" + id + "\" river network ID (" + riverID + ") is not found in the list of river network nodes.", "Specify a valid river network ID to associate the diversion with a river network node."));
				}
			}
		}
		if (!(capacity >= 0.0))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Diversion \"" + id + "\" capacity (" + StringUtil.formatString(capacity,"%.2f") + ") is invalid.", "Specify the capacity as a number >= 0."));
		}
		IList<string> choices = getIreptypeChoices(false);
		if (StringUtil.indexOfIgnoreCase(choices,"" + ireptyp) < 0)
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Diversion \"" + id + "\" replacement reservoir option (" + ireptyp + ") is invalid.", "Specify the data type as one of " + choices));
		}
		// Verify that the daily ID is in the data set (daily ID is allowed to be missing)
		if ((dataset != null) && !StateMod_Util.isMissing(dailyID))
		{
			DataSetComponent comp2 = dataset.getComponentForComponentType(StateMod_DataSet.COMP_DIVERSION_STATIONS);
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<StateMod_Diversion> ddsList = (java.util.List<StateMod_Diversion>)comp2.getData();
			IList<StateMod_Diversion> ddsList = (IList<StateMod_Diversion>)comp2.getData();
			if (dailyID.Equals("0") || dailyID.Equals("3") || dailyID.Equals("4"))
			{
				// OK
			}
			else if ((ddsList != null) && (ddsList.Count > 0))
			{
				// Check the diversion station list
				if (StateMod_Util.IndexOf(ddsList, dailyID) < 0)
				{
					validation.add(new StateMod_ComponentValidationProblem(this,"Diversion \"" + id + "\" daily ID (" + dailyID + ") is not 0, 3, or 4 and is not found in the list of diversion stations.", "Specify the daily ID as 0, 3, 4, or a matching diversion ID."));
				}
			}
		}
		if (StateMod_Util.isMissing(userName))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Diversion \"" + id + "\" user name is blank.", "Specify a diversion user name to clarify data."));
		}
		choices = getIdvcomChoices(false);
		if (StringUtil.indexOfIgnoreCase(choices,"" + idvcom) < 0)
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Diversion \"" + id + "\" data type (" + idvcom + ") is invalid.", "Specify the data type as one of " + choices));
		}
		if (!(nrtn >= 0))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Diversion \"" + id + "\" number of return flow locations (" + nrtn + ") is invalid.", "Specify the number of return flow locations as >= 0."));
		}
		if (!((divefc >= -100.0) && (divefc <= 100.0)))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Diversion \"" + id + "\" annual system efficiency (" + StringUtil.formatString(divefc,"%.2f") + ") is invalid.", "Specify the efficiency as 0 to 100 for annual (negative if monthly values are provided)."));
		}
		else if (divefc < 0.0)
		{
			// Check that each monthly efficiency is in the range 0 to 100
			double diveff;
			for (int i = 0; i < 12; i++)
			{
				diveff = getDiveff(i);
				if (!((diveff >= 0.0) && (diveff <= 100.0)))
				{
					validation.add(new StateMod_ComponentValidationProblem(this,"Diversion \"" + id + "\" system efficiency for month " + (i + 1) + " (" + StringUtil.formatString(diveff,"%.2f") + ") is invalid.", "Specify the efficiency as 0 to 100."));
				}
			}
		}
		if (!(area >= 0.0))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Diversion \"" + id + "\" area (" + StringUtil.formatString(area,"%.2f") + ") is invalid.", "Specify the area as a number >= 0."));
		}
		choices = getIrturnChoices(false);
		if (StringUtil.indexOfIgnoreCase(choices,"" + irturn) < 0)
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Diversion \"" + id + "\" use type (" + irturn + ") is invalid.", "Specify the use type as one of " + choices));
		}
		choices = getDemsrcChoices(false);
		if (StringUtil.indexOfIgnoreCase(choices,"" + demsrc) < 0)
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Diversion \"" + id + "\" demand source (" + demsrc + ") is invalid.", "Specify the demand source as one of " + choices));
		}
		// Check return flow locations...
		StateMod_ReturnFlow ret;
		double pcttot;
		string crtnid;
		for (int i = 0; i < nrtn; i++)
		{
			ret = getReturnFlow(i);
			pcttot = ret.getPcttot();
			crtnid = ret.getCrtnid();
			if (rinList != null)
			{
				// Make sure that the return location is in the network list
				if (StateMod_Util.IndexOf(rinList, crtnid) < 0)
				{
					validation.add(new StateMod_ComponentValidationProblem(this,"Diversion \"" + id + "\" return " + (i + 1) + " location (" + crtnid + ") is not found in the list of river network nodes.", "Specify a valid river network ID to associate the return location with a river network node."));
				}
			}
			if (!((pcttot >= 0.0) && (pcttot <= 100.0)))
			{
				validation.add(new StateMod_ComponentValidationProblem(this,"Diversion \"" + id + "\" return " + (i + 1) + " percent (" + StringUtil.formatString(pcttot,"%.2f") + ") is invalid.", "Specify the return percent as a number 0 to 100."));
			}
		}
		// TODO SAM 2009-06-01) evaluate how to check rights (with getRights() or checking the rights data
		// set component).
		return validation;
	}

	/// <summary>
	/// Write diversion information to output.  History header information 
	/// is also maintained by calling this routine.  Daily data fields are written. </summary>
	/// <param name="instrfile"> input file from which previous history should be taken </param>
	/// <param name="outstrfile"> output file to which to write </param>
	/// <param name="theDiversions"> list of diversions to write. </param>
	/// <param name="newComments"> addition comments which should be included in history </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeStateModFile(String instrfile, String outstrfile, java.util.List<StateMod_Diversion> theDiversions, java.util.List<String> newComments) throws Exception
	public static void writeStateModFile(string instrfile, string outstrfile, IList<StateMod_Diversion> theDiversions, IList<string> newComments)
	{
		writeStateModFile(instrfile, outstrfile, theDiversions, newComments, true);
	}

	/// <summary>
	/// Write diversion information to output.  History header information 
	/// is also maintained by calling this routine. </summary>
	/// <param name="instrfile"> input file from which previous history should be taken </param>
	/// <param name="outstrfile"> output file to which to write </param>
	/// <param name="theDiversions"> list of diversions to write. </param>
	/// <param name="newComments"> addition comments which should be included in history </param>
	/// <param name="use_daily_data"> Indicates whether daily data should be written.  The data
	/// are only used if the control file indicates that a daily run is occurring. </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeStateModFile(String instrfile, String outstrfile, java.util.List<StateMod_Diversion> theDiversions, java.util.List<String> newComments, boolean use_daily_data) throws Exception
	public static void writeStateModFile(string instrfile, string outstrfile, IList<StateMod_Diversion> theDiversions, IList<string> newComments, bool use_daily_data)
	{
		string routine = "StateMod_Diversin.writeStateModFile";
		IList<string> commentIndicators = new List<string>(1);
		commentIndicators.Add("#");
		IList<string> ignoredCommentIndicators = new List<string>(1);
		ignoredCommentIndicators.Add("#>");
		PrintWriter @out = null;
		try
		{
			@out = IOUtil.processFileHeaders(IOUtil.getPathUsingWorkingDir(instrfile), IOUtil.getPathUsingWorkingDir(outstrfile), newComments, commentIndicators, ignoredCommentIndicators, 0);

			int i;
			int j;
			string iline;
			string cmnt = "#>";
			// With daily ID....
			string format_1 = "%-12.12s%-24.24s%-12.12s%8d%#8.2F%8d%8d %-12.12s";
			// Without daily ID...
			string format_1A = "%-12.12s%-24.24s%-12.12s%8d%#8.2F%8d%8d";
			string format_2 = "            %-24.24s            %8d%8d%#8.0F%#8.2F%8d%8d";
			string format_3 = "%1.1s%#5.0F";
			string format_4 = "                                    %-12.12s%8.2F%8d";
			StateMod_Diversion div = null;
			StateMod_ReturnFlow ret = null;
			IList<object> v = new List<object>(9); // Reuse for all output lines.
			IList<StateMod_ReturnFlow> v5 = null; // For return flows.

			@out.println(cmnt);
			@out.println(cmnt + "*************************************************");
			@out.println(cmnt + "  Direct Diversion Station File");
			@out.println(cmnt);
			@out.println(cmnt + "  Card 1 format (a12, a24, a12, i8, f8.2, 2i8, 1x, a12)");
			@out.println(cmnt);
			@out.println(cmnt + "  ID          cdivid:  Diversion station ID");
			@out.println(cmnt + "  Name        divnam:  Diversion name");
			@out.println(cmnt + "  Riv ID       cgoto:  River node for diversion");
			@out.println(cmnt + "  On/Off      idivsw:  Switch 0=off, 1=on");
			@out.println(cmnt + "  Capacity    divcap:  Diversion capacity (CFS)");
			@out.println(cmnt + "                dumx:  Not currently used");
			@out.println(cmnt + "  RepType    ireptyp:  Replacement reservoir option (see StateMod doc)");
			@out.println(cmnt + "  Daily ID   cdividy:  Daily diversion ID");
			@out.println(cmnt);
			@out.println(cmnt + "  Card 2 format (12x, a24, 12x, 2i8, f8.2, f8.0, 2i8)");
			@out.println(cmnt);
			@out.println(cmnt + "  User Name  usernam:  User name.");
			@out.println(cmnt + "  DemType     idvcom:  Demand data type switch (see StateMod doc)");
			@out.println(cmnt + "  #-Ret         nrtn:  Number of return flow table ref");
			@out.println(cmnt + "  Eff         divefc:  Annual system efficiency");
			@out.println(cmnt + "  Area          area:  Irrigated acreage");
			@out.println(cmnt + "  UseType     irturn:  Use type (see StateMod doc)");
			@out.println(cmnt + "  Demsrc      demsrc:  Demand source (see StateMod doc)");
			@out.println(cmnt);
			@out.println(cmnt + "  Card 3 format (free format)");
			@out.println(cmnt);
			@out.println(cmnt + "     diveff (12):  System efficiency % by month");
			@out.println(cmnt);
			@out.println(cmnt + "  Card 4 format (36x, a12, f8.2, i8)");
			@out.println(cmnt);
			@out.println(cmnt + "  Ret ID      crtnid:  River node receiving return flow");
			@out.println(cmnt + "  Ret %       pcttot:  Percent of return flow to this river node");
			@out.println(cmnt + "  Table #     irtndl:  Delay (return flow) table for this return flow.");
			@out.println(cmnt);

			@out.println(cmnt + " ID               Name             Riv ID     On/Off  Capacity        RepType   Daily ID");
			@out.println(cmnt + "---------eb----------------------eb----------eb------eb------eb------eb------exb----------e");
			@out.println(cmnt + "              User Name                       DemType   #-Ret   Eff %   Area  UseType DemSrc");
			@out.println(cmnt + "xxxxxxxxxxb----------------------exxxxxxxxxxxxb------eb------eb------eb------eb------eb------e");
			@out.println(cmnt + "          ... Monthly Efficiencies...");
			@out.println(cmnt + "b----------------------------------------------------------------------------e");
			@out.println(cmnt + "                                   Ret ID       Ret % Table #");
			@out.println(cmnt + "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxb----------eb------eb------e");
			@out.println(cmnt + "EndHeader");

			int num = 0;
			if (theDiversions != null)
			{
				num = theDiversions.Count;
			}
			for (i = 0; i < num; i++)
			{
				div = theDiversions[i];
				if (div == null)
				{
					continue;
				}

				// line 1
				v.Clear();
				v.Add(div.getID());
				v.Add(div.getName());
				v.Add(div.getCgoto());
				v.Add(new int?(div.getSwitch()));
				v.Add(new double?(div.getDivcap()));
				v.Add(new int?(1)); // Old nduser not used anymore.
				v.Add(new int?(div.getIreptype()));
				if (use_daily_data)
				{
					v.Add(div.getCdividy());
					iline = StringUtil.formatString(v, format_1);
				}
				else
				{
					iline = StringUtil.formatString(v, format_1A);
				}
				@out.println(iline);

				// line 2
				v.Clear();
				v.Add(div.getUsername());
				v.Add(new int?(div.getIdvcom()));
				v.Add(new int?(div.getNrtn()));
				v.Add(new double?(div.getDivefc()));
				v.Add(new double?(div.getArea()));
				v.Add(new int?(div.getIrturn()));
				v.Add(new int?(div.getDemsrc()));
				iline = StringUtil.formatString(v, format_2);
				@out.println(iline);

				// line 3 - diversion efficiency
				if (div.getDivefc() < 0)
				{
					for (j = 0; j < 12; j++)
					{
						v.Clear();
						v.Add("");
						v.Add(new double?(div.getDiveff(j)));
						iline = StringUtil.formatString(v, format_3);
						@out.print(iline);
					}

					@out.println();
				}

				// line 4 - return information
				int nrtn = div.getNrtn();
				v5 = div.getReturnFlows();
				for (j = 0; j < nrtn; j++)
				{
					v.Clear();
					ret = v5[j];
					v.Add(ret.getCrtnid());
					v.Add(new double?(ret.getPcttot()));
					v.Add(new int?(ret.getIrtndl()));
					iline = StringUtil.formatString(v, format_4);
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
			@out = null;
		}
	}

	/// <summary>
	/// Writes a list of StateMod_Diversion objects to a list file.  A header is 
	/// printed to the top of the file, containing the commands used to generate the 
	/// file.  Any strings in the body of the file that contain the field delimiter 
	/// will be wrapped in "...".  This method also prints Diversion Return Flows to
	/// filename[without extension]_ReturnFlows[extension], so if this method is called
	/// with a filename parameter of "diversions.txt", two files will be generated:
	/// - diversions.txt
	/// - diversions_ReturnFlows.txt </summary>
	/// <param name="filename"> the name of the file to which the data will be written. </param>
	/// <param name="delimiter"> the delimiter to use for separating field values. </param>
	/// <param name="update"> whether to update an existing file, retaining the current 
	/// header (true) or to create a new file with a new header. </param>
	/// <param name="data"> the list of objects to write. </param>
	/// <param name="newComments"> new comments to add to the header (command file, HydroBase version, etc.). </param>
	/// <returns> a list of files that were actually written, because this method controls all the secondary
	/// filenames. </returns>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.List<java.io.File> writeListFile(String filename, String delimiter, boolean update, java.util.List<StateMod_Diversion> data, java.util.List<String> newComments) throws Exception
	public static IList<File> writeListFile(string filename, string delimiter, bool update, IList<StateMod_Diversion> data, IList<string> newComments)
	{
		string routine = "StateMod_Diversion.writeListFile";
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
		fields.Add("Capacity");
		fields.Add("ReplaceResOption");
		fields.Add("DailyID");
		fields.Add("UserName");
		fields.Add("DemandType");
		fields.Add("EffAnnual");
		fields.Add("IrrigatedAcres");
		fields.Add("UseType");
		fields.Add("DemandSource");
		fields.Add("EffMonthly01");
		fields.Add("EffMonthly02");
		fields.Add("EffMonthly03");
		fields.Add("EffMonthly04");
		fields.Add("EffMonthly05");
		fields.Add("EffMonthly06");
		fields.Add("EffMonthly07");
		fields.Add("EffMonthly08");
		fields.Add("EffMonthly09");
		fields.Add("EffMonthly10");
		fields.Add("EffMonthly11");
		fields.Add("EffMonthly12");
		int fieldCount = fields.Count;

		string[] names = new string[fieldCount];
		string[] formats = new string[fieldCount];
		int comp = StateMod_DataSet.COMP_DIVERSION_STATIONS;
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
		int size2 = 0;
		PrintWriter @out = null;
		IList<string> commentIndicators = new List<string>(1);
		commentIndicators.Add("#");
		IList<string> ignoredCommentIndicators = new List<string>(1);
		ignoredCommentIndicators.Add("#>");
		string[] line = new string[fieldCount];
		string id = null;
		StringBuilder buffer = new StringBuilder();
		StateMod_Diversion div = null;
		StateMod_ReturnFlow rf = null;
		IList<StateMod_ReturnFlow> returnFlows = new List<StateMod_ReturnFlow>();
		IList<StateMod_ReturnFlow> temp = null;

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
			newComments2.Insert(1,"StateMod diversion stations as a delimited list file.");
			newComments2.Insert(2,"See also the associated return flow and collection files.");
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
				div = data[i];

				line[0] = StringUtil.formatString(div.getID(), formats[0]).Trim();
				line[1] = StringUtil.formatString(div.getName(), formats[1]).Trim();
				line[2] = StringUtil.formatString(div.getCgoto(), formats[2]).Trim();
				line[3] = StringUtil.formatString(div.getSwitch(), formats[3]).Trim();
				line[4] = StringUtil.formatString(div.getDivcap(), formats[4]).Trim();
				line[5] = StringUtil.formatString(div.getIreptype(), formats[5]).Trim();
				line[6] = StringUtil.formatString(div.getCdividy(), formats[6]).Trim();
				line[7] = StringUtil.formatString(div.getUsername(), formats[7]).Trim();
				line[8] = StringUtil.formatString(div.getIdvcom(), formats[8]).Trim();
				line[9] = StringUtil.formatString(div.getDivefc(), formats[9]).Trim();
				line[10] = StringUtil.formatString(div.getArea(), formats[10]).Trim();
				line[11] = StringUtil.formatString(div.getIrturn(), formats[11]).Trim();
				line[12] = StringUtil.formatString(div.getDemsrc(), formats[12]).Trim();
				line[13] = StringUtil.formatString(div.getDiveff(0), formats[13]).Trim();
				line[14] = StringUtil.formatString(div.getDiveff(1), formats[14]).Trim();
				line[15] = StringUtil.formatString(div.getDiveff(2), formats[15]).Trim();
				line[16] = StringUtil.formatString(div.getDiveff(3), formats[16]).Trim();
				line[17] = StringUtil.formatString(div.getDiveff(4), formats[17]).Trim();
				line[18] = StringUtil.formatString(div.getDiveff(5), formats[18]).Trim();
				line[19] = StringUtil.formatString(div.getDiveff(6), formats[19]).Trim();
				line[20] = StringUtil.formatString(div.getDiveff(7), formats[20]).Trim();
				line[21] = StringUtil.formatString(div.getDiveff(8), formats[21]).Trim();
				line[22] = StringUtil.formatString(div.getDiveff(9), formats[22]).Trim();
				line[23] = StringUtil.formatString(div.getDiveff(10), formats[23]).Trim();
				line[24] = StringUtil.formatString(div.getDiveff(11), formats[24]).Trim();

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

				@out.println(buffer.ToString());

				temp = div.getReturnFlows();
				size2 = temp.Count;
				id = div.getID();
				for (j = 0; j < size2; j++)
				{
					rf = temp[j];
					rf.setID(id);
					returnFlows.Add(rf);
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

		int lastIndex = filename.LastIndexOf(".", StringComparison.Ordinal);
		string front = filename.Substring(0, lastIndex);
		string end = filename.Substring((lastIndex + 1), filename.Length - (lastIndex + 1));

		string returnFlowFilename = front + "_ReturnFlows." + end;
		StateMod_ReturnFlow.writeListFile(returnFlowFilename, delimiter, update, returnFlows, StateMod_DataSet.COMP_DIVERSION_STATION_DELAY_TABLES, newComments);

		string collectionFilename = front + "_Collections." + end;
		writeCollectionListFile(collectionFilename, delimiter, update, data, newComments);

		IList<File> filesWritten = new List<File>();
		filesWritten.Add(new File(filename));
		filesWritten.Add(new File(returnFlowFilename));
		filesWritten.Add(new File(collectionFilename));
		return filesWritten;
	}

	/// <summary>
	/// Writes the collection data from a list of StateMod_Diversion objects to a 
	/// list file.  A header is printed to the top of the file, containing the commands 
	/// used to generate the file.  Any strings in the body of the file that contain 
	/// the field delimiter will be wrapped in "...". </summary>
	/// <param name="filename"> the name of the file to which the data will be written. </param>
	/// <param name="delimiter"> the delimiter to use for separating field values. </param>
	/// <param name="update"> whether to update an existing file, retaining the current 
	/// header (true) or to create a new file with a new header. </param>
	/// <param name="data"> the list of objects to write. </param>
	/// <param name="newComments"> new comments to add to header (command file, HydroBase version, etc.). </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeCollectionListFile(String filename, String delimiter, boolean update, java.util.List<StateMod_Diversion> data, java.util.List<String> newComments) throws Exception
	public static void writeCollectionListFile(string filename, string delimiter, bool update, IList<StateMod_Diversion> data, IList<string> newComments)
	{
		string routine = "StateMod_Diversion.writeCollectionListFile";
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
		int comp = StateMod_DataSet.COMP_DIVERSION_STATION_COLLECTIONS;
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
		int k = 0;
		int num = 0;
		PrintWriter @out = null;
		StateMod_Diversion div = null;
		IList<string> commentIndicators = new List<string>(1);
		commentIndicators.Add("#");
		IList<string> ignoredCommentIndicators = new List<string>(1);
		ignoredCommentIndicators.Add("#>");
		string[] line = new string[fieldCount];
		string colType = null;
		string id = null;
		string partType = null;
		StringBuilder buffer = new StringBuilder();
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
			newComments2.Insert(1,"StateMod diversion station collection information as delimited list file.");
			newComments2.Insert(2,"See also the associated diversion station file.");
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
				div = data[i];
				id = div.getID();
				years = div.getCollectionYears();
				if (years == null)
				{
					num = 0;
				}
				else
				{
					num = years.Length;
				}
				colType = div.getCollectionType();
				partType = div.getCollectionPartType();

				for (int iyear = 0; iyear < num; iyear++)
				{
					ids = div.getCollectionPartIDs(years[iyear]);
					// Loop through the identifiers for the specific year
					for (k = 0; k < ids.Count; k++)
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
			@out = null;
		}
	}

	}

}