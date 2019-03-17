using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

// StateMod_Well - class to store well station

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
// StateMod_Well - Derived from StateMod_Data class
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
// 
// 01 Feb 1999	Catherine E.		Created initial version of class.
//		Nutting-Lane, RTi	
// 10 Feb 1999  CEN, RTi		Implemented changes due to SAM review
//					of code.
// 24 Feb 2000	Steven A. Malers, RTi	Added comments to the right of return
//					flows and depletions as per Ray Bennett.
// 13 Mar 2000	SAM, RTi		Change some variable names to reflect
//					more recent StateMod documentation:
//						cdividwx -> cdividyw
//						cgoto2 -> idvcow2
//						diveffw -> divefcw
// 10 Apr 2000	CEN, RTi		Added "primary" variable
// 09 Aug 2000	SAM, RTi		Change areaw to a double.
// 18 Feb 2001	SAM, RTi		Code review.  Clean up javadoc.  Handle
//					nulls and set unused variables to null.
//					Alphabetize methods.  Update output
//					header.
// 02 Mar 2001	SAM, RTi		Correct problem with 1.7 primary flag
//					being formatted 11.5 rather than 12.5
//					as documented.
// 2001-12-27	SAM, RTi		Update to use new fixedRead()to
//					improve performance.
// 2002-09-09	SAM, RTi		Add GeoRecord reference to allow 2-way
//					connection between spatial and StateMod
//					data.
// 2002-09-19	SAM, RTi		Use isDirty() instead of setDirty() to
//					indicate edits.
//------------------------------------------------------------------------------
// 2003-06-04	J. Thomas Sapienza, RTi	Renamed from SMWell to StateMod_Well
// 2003-06-10	JTS, RTi		* Folded dumpWellFile() into
//					  writeWellFile()
//					* Renamed parseWellFile() to 
//					  readWellFile()
// 2003-06-23	JTS, RTi		Renamed writeWellFile() to
//					writeStateModFile()
// 2003-06-26	JTS, RTi		Renamed readWellFile() to 
//					readStateModFile()
// 2003-07-15	JTS, RTi		Changed to use new dataset design.
// 2003-08-03	SAM, RTi		Change isDirty() back to setDirty().
// 2003-08-15	SAM, RTi		Change GeoRecordNoSwing to GeoRecord.
// 2003-08-28	SAM, RTi		* Change rights to use a simple Vector,
//					  not a linked list.
//					* Clean up parameter names and Javadoc.
//					* Call setDirty() on individual objects
//					  and the data set component.
//					* Remove unneeded "num" data - get from
//					  the Vector size.
//					* Remove redundant "Well" from some
//					  methods.
//					* Support all well-related time series
//					  and clean up names.
// 2003-10-10	SAM, RTi		Add disconnectRights().
// 2003-10-16	SAM, RTi		Change innitial efficiency to 60%.
// 2003-10-21	SAM, RTi		* Add CWR, similar to diversions.
//					* Default idvcow2 to "N/A".
// 2004-02-25	SAM, RTi		Add isStateModWellFile().
// 2004-06-05	SAM, RTi		* Add methods to handle collections,
//					  similar to StateCU locations.
// 2004-07-06	SAM, RTi		* Overload the constructor to allow
//					  initialization to defaults or missing.
// 2004-07-08	SAM, RTi		* Add getPrimaryChoices() and
//					  getPrimaryDefault().
// 2004-08-25	JTS, RTi		* Added acceptChanges().
//					* Added changed().
//					* Added clone().
//					* Added compareTo().
//					* Added createBackup().
//					* Added restoreOriginal().
//					* Now implements Cloneable.
//					* Now implements Comparable.
//					* Clone status is checked via _isClone
//					  when the component is marked as dirty.
// 2004-08-26	JTS, RTi		compareTo() now handles _idvcow2.
// 2004-09-16	SAM, RTi		* Change so read and write methods
//					  adjust the path relative to the
//					  working directory.
// 2004-09-29	SAM, RTi		* Add the following for use with
//					  StateDMI only - no need to check for
//					  dirty - only set/gets on the entire
//					  array are enabled:
//						__cwr_monthly
//						__ddh_monthly
//						__calculated_efficiencies
//						__calculated_efficiency_stddevs
//						__model_efficiecies
// 2004-10-07	SAM, RTi		* Add 6 as an option for idvcomw.
// 2005-04-18	JTS, RTi		* Added writeListFile().
//					* Added writeCollectionListFile().
// 2005-08-18	SAM, RTi		* Add static data for part types to
//					  minimize errors in string use.
// 2005-10-10	SAM, RTi		* Change some javadoc - was using
//					  "diversion" instead of "well".
// 2005-11-16	SAM, RTi		Overload set/get methods for monthly
//					efficiency to take a year type, to
//					facilitate handling of non-calendar
//					year type.
// 2006-01-30	SAM, RTi		* Add hasAssociatedDiversion() to
//					  facilitate processing.
// 2006-04-09	SAM, RTi		Add _parcels_Vector data member and
//					associated methods, to help with
//					StateDMI error handling.
// 2007-04-12	Kurt Tometich, RTi		Added checkComponentData() and
//									getDataHeader() methods for check
//									file and data check support.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
//------------------------------------------------------------------------------
// EndHeader
// TODO SAM 2006-04-09
// The _parcel_Vector has minimal support and is not yet considered in
// copy, clone, equals, etc.

namespace DWR.StateMod
{

	using StateCU_IrrigationPracticeTS = DWR.StateCU.StateCU_IrrigationPracticeTS;
	using GeoRecord = RTi.GIS.GeoView.GeoRecord;
	using HasGeoRecord = RTi.GIS.GeoView.HasGeoRecord;
	using DayTS = RTi.TS.DayTS;
	using MonthTS = RTi.TS.MonthTS;
	using CommandStatus = RTi.Util.IO.CommandStatus;
	using DataSetComponent = RTi.Util.IO.DataSetComponent;
	using IOUtil = RTi.Util.IO.IOUtil;
	using Message = RTi.Util.Message.Message;
	using MessageUtil = RTi.Util.Message.MessageUtil;
	using StringUtil = RTi.Util.String.StringUtil;
	using TimeUtil = RTi.Util.Time.TimeUtil;
	using YearType = RTi.Util.Time.YearType;

	/// <summary>
	/// This class stores all relevant data for a StateMod well.  
	/// </summary>

	public class StateMod_Well : StateMod_Data, ICloneable, IComparable<StateMod_Data>, HasGeoRecord, StateMod_ComponentValidator
	{
	/// <summary>
	/// Well id to use for daily data.
	/// </summary>
	protected internal string _cdividyw;
	/// <summary>
	/// Well capacity(cfs).
	/// </summary>
	private double _divcapw;
	/// <summary>
	/// Diversion this well is tied to ("N/A" if not tied to a diversion).
	/// </summary>
	private string _idvcow2;
	/// <summary>
	/// Demand code.
	/// </summary>
	private int _idvcomw;
	/// <summary>
	/// System efficiency(%).
	/// </summary>
	private double _divefcw;
	/// <summary>
	/// Irrigated area associated with well.
	/// </summary>
	private double _areaw;
	/// <summary>
	/// Use type.
	/// </summary>
	private int _irturnw;
	/// <summary>
	/// Irrigated acreage source.
	/// </summary>
	private int _demsrcw;
	/// <summary>
	/// 12 efficiency values. 
	/// </summary>
	private double[] _diveff;
	/// <summary>
	/// Return flow data.
	/// </summary>
	private IList<StateMod_ReturnFlow> _rivret;
	/// <summary>
	/// Depletion data.
	/// </summary>
	private IList<StateMod_ReturnFlow> _depl;
	/// <summary>
	/// Historical time series (monthly).
	/// </summary>
	private MonthTS _pumping_MonthTS;
	/// <summary>
	/// 12 monthly and annual average over period, used by StateDMI.
	/// </summary>
	private double[] __weh_monthly = null;
	/// <summary>
	/// Historical time series (daily).
	/// </summary>
	private DayTS _pumping_DayTS;
	/// <summary>
	/// Demand time series.
	/// </summary>
	private MonthTS _demand_MonthTS;
	/// <summary>
	/// Daily demand time series.
	/// </summary>
	private DayTS _demand_DayTS;
	/// <summary>
	/// Irrigation practice time series.
	/// </summary>
	private StateCU_IrrigationPracticeTS _ipy_YearTS;
	/// <summary>
	/// Consumptive water requirement.
	/// </summary>
	private MonthTS _cwr_MonthTS;
	/// <summary>
	/// 12 monthly and annual average over period, used by StateDMI.
	/// </summary>
	private double[] __cwr_monthly = null;
	/// <summary>
	/// Time series - only used when idvcow2 is "N/A".
	/// </summary>
	private DayTS _cwr_DayTS;
	/// <summary>
	/// Well rights.
	/// </summary>
	private IList<StateMod_WellRight> _rights;
	/// <summary>
	/// Priority switch.
	/// </summary>
	private double _primary;
	/// <summary>
	/// Link to spatial data.
	/// </summary>
	private GeoRecord _georecord;

	/// <summary>
	/// List of parcel data, in particular to allow StateDMI to detect when a well had no data.
	/// </summary>
	protected internal IList<StateMod_Parcel> _parcel_Vector = new List<StateMod_Parcel>();

	// Collections are set up to be specified by year for wells, using parcels as the parts.

	/// <summary>
	/// Types of collections.  An aggregate merges the water rights/permits whereas
	/// a system keeps all the water rights but just has one ID.
	/// </summary>
	public static string COLLECTION_TYPE_AGGREGATE = "Aggregate";
	public static string COLLECTION_TYPE_SYSTEM = "System";

	/// <summary>
	/// How aggregates are specified, as collection of ditches, parcels, or wells.
	/// If wells, see COLLECTION_WELL_PART_ID_TYPE_*.
	/// </summary>
	public static string COLLECTION_PART_TYPE_DITCH = "Ditch";
	public static string COLLECTION_PART_TYPE_PARCEL = "Parcel";
	public static string COLLECTION_PART_TYPE_WELL = "Well";

	/// <summary>
	/// Indicates the type of aggregate part ID when aggregating wells.
	/// </summary>
	public static string COLLECTION_WELL_PART_ID_TYPE_WDID = "WDID";
	public static string COLLECTION_WELL_PART_ID_TYPE_RECEIPT = "Receipt";

	private string __collection_type = StateMod_Util.MISSING_STRING;

	/// <summary>
	/// Used by DMI software - currently no options.
	/// </summary>
	private string __collection_part_type = COLLECTION_PART_TYPE_PARCEL;

	/// <summary>
	/// The identifiers for data that are collected - null if not a collection
	/// location.  This is a List of Lists corresponding to each __collectionYear element.
	/// If the list of identifiers is consistent for the entire period then the
	/// __collectionYear array will have a size of 0 and the __collectionIDList will be a single list.
	/// </summary>
	private IList<IList <string>> __collectionIDList = null;

	/// <summary>
	/// The identifiers types for data that are collected - null if not a collection
	/// location.  This is a List of Lists corresponding to each __collectionYear element.
	/// If the list of identifiers is consistent for the entire period then the
	/// __collectionYear array will have a size of 0 and the __collectionIDTypeList will be a single list.
	/// This list is only used for well collections that use well identifiers for the parts.
	/// </summary>
	private IList<IList <string>> __collectionIDTypeList = null;

	/// <summary>
	/// An array of years that correspond to the aggregate/system.  Well collections are defined by year.
	/// </summary>
	private int[] __collectionYear = null;

	/// <summary>
	/// The division that corresponds to the aggregate/system.  Currently it is expected that the same division
	/// number is assigned to all the data.
	/// </summary>
	private int __collection_div = StateMod_Util.MISSING_INT;

	/// <summary>
	/// The following are used only by StateDMI and do not needed to be handled in
	/// comparison, initialization, etc.
	/// </summary>
	private double[] __calculated_efficiencies = null;
	private double[] __calculated_efficiency_stddevs = null;
	private double[] __model_efficiencies = null;

	/// <summary>
	/// Constructor - initialize to default values.
	/// </summary>
	public StateMod_Well() : this(true)
	{
	}

	/// <summary>
	/// Constructor. </summary>
	/// <param name="initialize_defaults"> If true, initialize data to reasonable values.  If
	/// false, initialize to missing values. </param>
	public StateMod_Well(bool initialize_defaults) : base()
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
	/// Add depletion node to the vector of depletion nodes.  Updates the variable
	/// which tracks the number of depletion nodes for this well. </summary>
	/// <param name="depl"> depletion data object </param>
	public virtual void addDepletion(StateMod_ReturnFlow depl)
	{
		if (depl == null)
		{
			return;
		}
		_depl.Add(depl);
		setDirty(true);
		if (!_isClone && _dataset != null)
		{
			_dataset.setDirty(StateMod_DataSet.COMP_WELL_STATIONS, true);
		}
	}

	/// <summary>
	/// Add return flow node to the vector of return flow nodes.  Also, updates
	/// the number of return flow nodes variable. </summary>
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
			_dataset.setDirty(StateMod_DataSet.COMP_WELL_STATIONS, true);
		}
	}

	/// <summary>
	/// Adds a right to the rights linked list
	/// </summary>
	public virtual void addRight(StateMod_WellRight right)
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

	/*
		Message.printStatus(1, "", "'" + _cdividyw + "' : '" 
			+ ((StateMod_Well)_original)._cdividyw + "'");
		Message.printStatus(1, "", "'" + _divcapw + "' : '" 
			+ ((StateMod_Well)_original)._divcapw + "'");
		Message.printStatus(1, "", "'" + _idvcow2 + "' : '" 
			+ ((StateMod_Well)_original)._idvcow2 + "'");
		Message.printStatus(1, "", "'" + _idvcomw + "' : '" 
			+ ((StateMod_Well)_original)._idvcomw + "'");
		Message.printStatus(1, "", "'" + _divefcw + "' : '" 
			+ ((StateMod_Well)_original)._divefcw + "'");
		Message.printStatus(1, "", "'" + _areaw + "' : '" 
			+ ((StateMod_Well)_original)._areaw + "'");
		Message.printStatus(1, "", "'" + _irturnw + "' : '" 
			+ ((StateMod_Well)_original)._irturnw + "'");
		Message.printStatus(1, "", "'" + _demsrcw + "' : '" 
			+ ((StateMod_Well)_original)._demsrcw + "'");
		Message.printStatus(1, "", "'" + _primary + "' : '" 
			+ ((StateMod_Well)_original)._primary + "'");
		if (_diveff == null && ((StateMod_Well)_original)._diveff == null) {
			Message.printStatus(1, "", "BOTH NULL");
		}
		else if (_diveff == null) {
			Message.printStatus(1, "", "1 NULL");
		}
		else if (((StateMod_Well)_original)._diveff == null) {
			Message.printStatus(1, "", "2 NULL");
		}
		else {
			for (int i = 0; i < _diveff.length; i++) {
				Message.printStatus(1, "", "'" + _diveff[i] + "' : '" 
					+ ((StateMod_Well)_original)._diveff[i] 
					+ "' (" + i + ")");
			}
		}
	*/
		return true;
	}

	/// <summary>
	/// Performs data checks for the capacity portion of this component. </summary>
	/// <param name="wer_Vector"> List of water rights. </param>
	/// <returns> String[] array of data that has been checked.  Returns null if there were no problems found. </returns>
	public virtual string[] checkComponentData_Capacity(IList<StateMod_WellRight> wer_Vector, int count)
	{
		double decree;
		double decree_sum;
		int onoff = 0; // On/off switch for right
		int size_rights = 0;
		string id_i = null;
		System.Collections.IList rights = null;
		id_i = getID();
		StateMod_WellRight wer_i = null;
		rights = StateMod_Util.getRightsForStation(id_i, wer_Vector);
		size_rights = 0;
		if (rights != null)
		{
			size_rights = rights.Count;
		}
		if (size_rights == 0)
		{
			return null;
		}
		// Get the sum of the rights, assuming that all should be
		// compared against the capacity (i.e., sum of rights at the
		// end of the period will be compared with the current well capacity)...
		decree_sum = 0.0;
		for (int iright = 0; iright < size_rights; iright++)
		{
			wer_i = (StateMod_WellRight)rights[iright];
			decree = wer_i.getDcrdivw();
			onoff = getSwitch();
			if (decree < 0.0)
			{
				// Ignore - missing values will cause a bad sum.
				continue;
			}
			if (onoff <= 0)
			{
				// Subtract the decree...
				decree_sum -= decree;
			}
			else
			{
				// Add the decree...
				decree_sum += decree;
			}
		}
		// Compare to a whole number, which is the greatest precision for documented files.
		if (!StringUtil.formatString(decree_sum,"%.2f").Equals(StringUtil.formatString(getDivcapw(),"%.2f")))
		{
			// new format for check file
			string[] data_table = new string[] {StringUtil.formatString(++count,"%4d"), StringUtil.formatString(id_i,"%-12.12s"), getName(), StringUtil.formatString(getCollectionType(),"%-10.10s"), StringUtil.formatString(getDivcapw(),"%9.2f"), StringUtil.formatString(decree_sum,"%9.2f"), StringUtil.formatString(size_rights,"%8d")};
			return StateMod_Util.checkForMissingValues(data_table);
		}
		return null;
	}

	/// <summary>
	/// Clones the data object. </summary>
	/// <returns> a cloned object. </returns>
	public override object clone()
	{
		StateMod_Well w = (StateMod_Well)base.clone();
		w._isClone = true;

		// The following are not cloned because there is no need to.  
		// The cloned values are only used for comparing between the 
		// values that can be changed in a single GUI.  The following
		// Vectors' data have their changes committed in other GUIs.	
		w._rivret = _rivret;
		w._rights = _rights;
		w._depl = _depl;

		if (_diveff == null)
		{
			w._diveff = null;
		}
		else
		{
			w._diveff = (double[])_diveff.Clone();
		}
		return w;
	}

	/// <summary>
	/// Compares this object to another StateMod_Well object. </summary>
	/// <param name="data"> the object to compare against. </param>
	/// <returns> 0 if they are the same, 1 if this object is greater than the other object, or -1 if it is less. </returns>
	public virtual int CompareTo(StateMod_Data data)
	{
		int res = base.CompareTo(data);
		if (res != 0)
		{
			return res;
		}

		StateMod_Well w = (StateMod_Well)data;

		res = _cdividyw.CompareTo(w._cdividyw);
		if (res != 0)
		{
			return res;
		}

		if (_divcapw < w._divcapw)
		{
			return -1;
		}
		else if (_divcapw > w._divcapw)
		{
			return 1;
		}

		res = _idvcow2.CompareTo(w._idvcow2);
		if (res != 0)
		{
			return res;
		}

		if (_idvcomw < w._idvcomw)
		{
			return -1;
		}
		else if (_idvcomw > w._idvcomw)
		{
			return 1;
		}

		if (_divefcw < w._divefcw)
		{
			return -1;
		}
		else if (_divefcw > w._divefcw)
		{
			return 1;
		}

		if (_areaw < w._areaw)
		{
			return -1;
		}
		else if (_areaw > w._areaw)
		{
			return 1;
		}

		if (_irturnw < w._irturnw)
		{
			return -1;
		}
		else if (_irturnw > w._irturnw)
		{
			return 1;
		}

		if (_demsrcw < w._demsrcw)
		{
			return -1;
		}
		else if (_demsrcw > w._demsrcw)
		{
			return 1;
		}

		if (_primary < w._primary)
		{
			return -1;
		}
		else if (_primary > w._primary)
		{
			return 1;
		}

		if (_diveff == null && w._diveff == null)
		{
			return 0;
		}
		else if (_diveff == null && w._diveff != null)
		{
			return -1;
		}
		else if (_diveff != null && w._diveff == null)
		{
			return 1;
		}
		else
		{
			int size1 = _diveff.Length;
			int size2 = w._diveff.Length;
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
				if (_diveff[i] < w._diveff[i])
				{
					return -1;
				}
				else if (_diveff[i] > w._diveff[i])
				{
					return 1;
				}
			}
		}

		return 0;
	}

	/// <summary>
	/// Creates a copy of the object for later use in checking to see if it was changed in a GUI.
	/// </summary>
	public virtual void createBackup()
	{
		_original = (StateMod_Well)clone();
		((StateMod_Well)_original)._isClone = false;
		_isClone = true;
	}

	/// <summary>
	/// Connect all water rights to the corresponding wells.
	/// This routines doesn't add an element to an array - the array
	/// already exists. This method just connects next and previous pointers. </summary>
	/// <param name="wells"> all wells </param>
	/// <param name="rights"> all rights </param>
	public static void connectAllRights(IList<StateMod_Well> wells, IList<StateMod_WellRight> rights)
	{
		if ((wells == null) || (rights == null))
		{
			return;
		}
		int num_wells = wells.Count;

		StateMod_Well well = null;
		for (int i = 0; i < num_wells; i++)
		{
			well = wells[i];
			if (well == null)
			{
				continue;
			}
			well.connectRights(rights);
		}
	}

	/// <summary>
	/// Connect the wells time series to this instance. </summary>
	/// <param name="wells"> all wells </param>
	/// <param name="cwr_MonthTS"> list of monthly consumptive water requirement time series, or null. </param>
	/// <param name="cwr_DayTS"> list of daily consumptive water requirement time series, or null. </param>
	public static void connectAllTS(IList<StateMod_Well> wells, IList<MonthTS> pumping_MonthTS, IList<DayTS> pumping_DayTS, IList<MonthTS> demand_MonthTS, IList<DayTS> demand_DayTS, IList<StateCU_IrrigationPracticeTS> ipy_YearTS, IList<MonthTS> cwr_MonthTS, IList<DayTS> cwr_DayTS)
	{
		if (wells == null)
		{
			return;
		}

		int num_wells = wells.Count;

		StateMod_Well well = null;
		for (int i = 0; i < num_wells; i++)
		{
			well = wells[i];
			if (well == null)
			{
				continue;
			}
			if (pumping_MonthTS != null)
			{
				well.connectPumpingMonthTS(pumping_MonthTS);
			}
			if (pumping_DayTS != null)
			{
				well.connectPumpingDayTS(pumping_DayTS);
			}
			if (demand_MonthTS != null)
			{
				well.connectDemandMonthTS(demand_MonthTS);
			}
			if (demand_DayTS != null)
			{
				well.connectDemandDayTS(demand_DayTS);
			}
			if (ipy_YearTS != null)
			{
				well.connectIrrigationPracticeYearTS(ipy_YearTS);
			}
			if (cwr_MonthTS != null)
			{
				well.connectCWRMonthTS(cwr_MonthTS);
			}
			if (cwr_DayTS != null)
			{
				well.connectCWRDayTS(cwr_DayTS);
			}
		}
	}

	/// <summary>
	/// Connect daily CWR series pointer.  The connection is made using the value of "cdividyw" for the well. </summary>
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
			if (_cdividyw.Equals(ts.getLocation(), StringComparison.OrdinalIgnoreCase))
			{
				_cwr_DayTS = ts;
				ts.setDescription(getName());
				break;
			}
		}
	}

	/// <summary>
	/// Connect monthly CWR time series pointer.  The time series name is set to that of the well. </summary>
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
	/// Connect daily demand time series pointer to this object. </summary>
	/// <param name="tslist"> Daily demand time series. </param>
	public virtual void connectDemandDayTS(IList<DayTS> tslist)
	{
		if (tslist == null)
		{
			return;
		}
		_demand_DayTS = null;

		int num_TS = tslist.Count;

		DayTS ts = null;
		for (int i = 0; i < num_TS; i++)
		{
			ts = tslist[i];
			if (ts == null)
			{
				continue;
			}
			if (_cdividyw.Equals(ts.getLocation(), StringComparison.OrdinalIgnoreCase))
			{
				_demand_DayTS = ts;
				ts.setDescription(getName());
				break;
			}
		}
	}

	/// <summary>
	/// Connect monthly demand time series pointer to this object. </summary>
	/// <param name="tslist"> demand time series </param>
	public virtual void connectDemandMonthTS(IList<MonthTS> tslist)
	{
		if (tslist == null)
		{
			return;
		}
		_demand_MonthTS = null;
		int num_TS = tslist.Count;

		MonthTS ts = null;
		for (int i = 0; i < num_TS; i++)
		{
			ts = tslist[i];
			if (ts == null)
			{
				continue;
			}
			if (_id.Equals(ts.getLocation()))
			{
				_demand_MonthTS = ts;
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
	/// Connect daily pumping time series pointer to this object. </summary>
	/// <param name="tslist"> Daily pumping time series. </param>
	public virtual void connectPumpingDayTS(IList<DayTS> tslist)
	{
		if (tslist == null)
		{
			return;
		}
		_pumping_DayTS = null;

		int num_TS = tslist.Count;

		DayTS ts = null;
		for (int i = 0; i < num_TS; i++)
		{
			ts = tslist[i];
			if (ts == null)
			{
				continue;
			}
			if (_cdividyw.Equals(ts.getLocation(), StringComparison.OrdinalIgnoreCase))
			{
				_pumping_DayTS = ts;
				ts.setDescription(getName());
				break;
			}
		}
	}

	/// <summary>
	/// Connect monthly pumping time series pointer to this object. </summary>
	/// <param name="tslist"> monthly pumping time series </param>
	public virtual void connectPumpingMonthTS(IList<MonthTS> tslist)
	{
		if (tslist == null)
		{
			return;
		}
		_pumping_MonthTS = null;
		int num_TS = tslist.Count;

		MonthTS ts = null;
		for (int i = 0; i < num_TS; i++)
		{
			ts = tslist[i];
			if (ts == null)
			{
				continue;
			}
			if (_id.Equals(ts.getLocation()))
			{
				_pumping_MonthTS = ts;
				ts.setDescription(getName());
				break;
			}
		}
	}

	/// <summary>
	/// Create a list of references to rights for this well. </summary>
	/// <param name="rights"> all rights </param>
	public virtual void connectRights(IList<StateMod_WellRight> rights)
	{
		if (rights == null)
		{
			return;
		}

		int num_rights = rights.Count;

		StateMod_WellRight right = null;
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
	/// delete depletion table record at a specified index </summary>
	/// <param name="index"> index desired depletion data object to delete </param>
	/// <exception cref="ArrayIndexOutOfBounds"> throws exception if unable to remove the 
	/// specified depletion data object </exception>
	public virtual void deleteDepletionAt(int index)
	{
		_depl.RemoveAt(index);
		setDirty(true);
		if (!_isClone && _dataset != null)
		{
			_dataset.setDirty(StateMod_DataSet.COMP_WELL_STATIONS, true);
		}
	}

	/// <summary>
	/// Delete return flow node at a specified index.  Also, updates the number of return flow nodes variable. </summary>
	/// <param name="index"> index of return flow data object to delete </param>
	public virtual void deleteReturnFlowAt(int index)
	{
		_rivret.RemoveAt(index);
		setDirty(true);
		if (!_isClone && _dataset != null)
		{
			_dataset.setDirty(StateMod_DataSet.COMP_WELL_STATIONS, true);
		}
	}

	// TODO - in the GUI need to decide if the right is actually removed from the main list
	/// <summary>
	/// Remove right from list.  A comparison on the ID is made. </summary>
	/// <param name="right"> Right to remove.  Note that the right is only removed from the
	/// list for this well and must also be removed from the main well right list. </param>
	public virtual void disconnectRight(StateMod_WellRight right)
	{
		if (right == null)
		{
			return;
		}
		int size = _rights.Count;
		StateMod_WellRight right2;
		// Assume that more than on instance can exist, even though this is not allowed...
		for (int i = 0; i < size; i++)
		{
			right2 = (StateMod_WellRight)_rights[i];
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

	/// <returns> the area(This is currently not being used but is provided for consistency within this class). </returns>
	public virtual double getAreaw()
	{
		return _areaw;
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
	/// Return the average monthly historical pumping (12 monthly values + annual
	/// average), for the data set calendar type.  This is ONLY used by StateDMI and
	/// does not need to be considered in comparison code.
	/// </summary>
	public virtual double [] getAverageMonthlyHistoricalPumping()
	{
		return __weh_monthly;
	}

	/// <summary>
	/// Return the average monthly efficiencies calculated from CWR and historical
	/// pumping (12 monthly values + annual average), for the data set calendar type.
	/// This is ONLY used by StateDMI and does not need to be considered in comparison code.
	/// </summary>
	public virtual double [] getCalculatedEfficiencies()
	{
		return __calculated_efficiencies;
	}

	/// <summary>
	/// Return the standard deviation of monthly efficiencies calculated from CWR and
	/// historical pumping (12 monthly values + annual average), for the data set calendar type.
	/// This is ONLY used by StateDMI and does not need to be considered in comparison code.
	/// </summary>
	public virtual double [] getCalculatedEfficiencyStddevs()
	{
		return __calculated_efficiency_stddevs;
	}

	/// <returns> the well id to use for daily data </returns>
	public virtual string getCdividyw()
	{
		return _cdividyw;
	}

	/// <summary>
	/// Return the collection part division the specific year.  Currently it is
	/// expected that the user always uses the same division. </summary>
	/// <returns> the division for the collection, or 0. </returns>
	public virtual int getCollectionDiv()
	{
		return __collection_div;
	}

	/// <summary>
	/// Return the collection part ID list for the specific year.  For parcels, collections are by year. </summary>
	/// <returns> the list of collection part IDS, or null if not defined. </returns>
	public virtual IList<string> getCollectionPartIDs(int year)
	{
		if ((__collectionIDList == null) || (__collectionIDList.Count == 0))
		{
			return null;
		}
		if (__collection_part_type.Equals("Ditch", StringComparison.OrdinalIgnoreCase) || __collection_part_type.Equals("Well", StringComparison.OrdinalIgnoreCase))
		{
			// The list of part IDs will be the first and only list (year irrelevant)...
			return (IList<string>)__collectionIDList[0];
		}
		else if (__collection_part_type.Equals("Parcel", StringComparison.OrdinalIgnoreCase))
		{
			// The list of part IDs needs to match the year.
			for (int i = 0; i < __collectionYear.Length; i++)
			{
				if (year == __collectionYear[i])
				{
					return (IList<string>)__collectionIDList[i];
				}
			}
		}
		return null;
	}

	/// <summary>
	/// Return the collection part type (see COLLECTION_PART_TYPE_*).
	/// </summary>
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
		return __collectionYear;
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
	/// Returns the table header for StateMod_Well data tables. </summary>
	/// <returns> String[] header - Array of header elements. </returns>
	public static string[] getDataHeader()
	{
		return new string[] {"Num", "Well Station ID", "Well Station Name", "Collection Type", "# Parcels for Well Station", "Total Parcel Area", "# Parcels with Wells", "Parcels with Well Area (ACRE)"};
	}

	/// <returns> daily demand time series </returns>
	public virtual DayTS getDemandDayTS()
	{
		return _demand_DayTS;
	}

	/// <returns> monthly demand time series </returns>
	public virtual MonthTS getDemandMonthTS()
	{
		return _demand_MonthTS;
	}

	/// <returns> the irrigated acreage source </returns>
	public virtual int getDemsrcw()
	{
		return _demsrcw;
	}

	/// <summary>
	/// Return a list of demand source option strings, for use in GUIs.
	/// The options are of the form "1" if include_notes is false and
	/// "1 - Irrigated acres from GIS", if include_notes is true. </summary>
	/// <returns> a list of demand source option strings, for use in GUIs. </returns>
	/// <param name="includeNotes"> Indicate whether notes should be included. </param>
	public static IList<string> getDemsrcwChoices(bool includeNotes)
	{
		return StateMod_Diversion.getDemsrcChoices(includeNotes);
	}

	/// <returns> the depletion at a particular index </returns>
	/// <param name="index"> index desired to retrieve </param>
	/// <exception cref="ArrayIndexOutOfBounds"> throws exception if unable to retrieve the specified depletion node </exception>
	public virtual StateMod_ReturnFlow getDepletion(int index)
	{
		return (StateMod_ReturnFlow)_depl[index];
	}

	/// <returns> the depletion list </returns>
	public virtual IList<StateMod_ReturnFlow> getDepletions()
	{
		return _depl;
	}

	/// <returns> the well capacity </returns>
	public virtual double getDivcapw()
	{
		return _divcapw;
	}

	/*
	@return the system efficiency
	*/
	public virtual double getDivefcw()
	{
		return _divefcw;
	}

	/// <returns> the variable efficiency </returns>
	public virtual double getDiveff(int index)
	{
		return _diveff[index];
	}

	/// <summary>
	/// Return the system efficiency for the specified month index, where the month
	/// is always for calendar year (0=January). </summary>
	/// <param name="index"> 0-based monthly index (0=January). </param>
	/// <param name="yeartype"> The year type for the well stations file (consistent with
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
	/// Get the geographical data associated with the well. </summary>
	/// <returns> the GeoRecord for the well. </returns>
	public virtual GeoRecord getGeoRecord()
	{
		return _georecord;
	}

	/// <summary>
	/// Returns the table header for capacity data tables. </summary>
	/// <returns> String[] header - Array of header elements. </returns>
	public static string[] getCapacityHeader()
	{
		return new string[] {"Num", "Well Station ID", "Well Station Name", "Collection Type", "Well Capacity (CFS)", "Sum of Rights (CFS)", "Number of Rights (CFS)"};
	}

	/// <summary>
	/// Return the average monthly efficiencies to be used for modeling (12 monthly values + annual average),
	/// for the data set calendar type.  This is ONLY used by StateDMI and does not need
	/// to be considered in comparison code.
	/// </summary>
	public virtual double [] getModelEfficiencies()
	{
		return __model_efficiencies;
	}

	/// <returns> historical time series for this well. </returns>
	public virtual DayTS getPumpingDayTS()
	{
		return _pumping_DayTS;
	}

	/// <returns> historical time series for this well. </returns>
	public virtual MonthTS getPumpingMonthTS()
	{
		return _pumping_MonthTS;
	}

	/// <returns> the demand code(see StateMod documentation for acceptable values) </returns>
	public virtual int getIdvcomw()
	{
		return _idvcomw;
	}

	/// <summary>
	/// Return a list of monthly demand type option strings, for use in GUIs.
	/// The options are of the form "1" if include_notes is false and
	/// "1 - Monthly total demand", if include_notes is true. </summary>
	/// <returns> a list of monthly demand type option strings, for use in GUIs. </returns>
	/// <param name="include_notes"> Indicate whether notes should be included. </param>
	public static IList<string> getIdvcomwChoices(bool include_notes)
	{
		IList<string> v = new List<string>(6);
		v.Add("1 - Monthly total demand");
		v.Add("2 - Annual total demand");
		v.Add("3 - Monthly irrigation water requirement");
		v.Add("4 - Annual irrigation water requirement");
		v.Add("5 - Estimate to be zero");
		v.Add("6 - Diversion+well demand is with diversion");
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
	/// to pick a default for a new well. </summary>
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

	/// <returns> the diversion this well is tied to </returns>
	public virtual string getIdvcow2()
	{
		return _idvcow2;
	}

	/// <summary>
	/// Get yearly irrigation practice time series.
	/// </summary>
	public virtual StateCU_IrrigationPracticeTS getIrrigationPracticeYearTS()
	{
		return _ipy_YearTS;
	}

	/// <returns> the use type </returns>
	public virtual int getIrturnw()
	{
		return _irturnw;
	}

	/// <summary>
	/// Return a list of use type option strings, for use in GUIs.
	/// The options are of the form "1" if include_notes is false and
	/// "1 - Irrigation", if include_notes is true. </summary>
	/// <returns> a list of use type option strings, for use in GUIs. </returns>
	/// <param name="includeNotes"> Indicate whether notes should be included. </param>
	public static IList<string> getIrturnwChoices(bool includeNotes)
	{
		return StateMod_Diversion.getIrturnChoices(includeNotes);
	}

	/// <summary>
	/// Get the last right associated with the well.
	/// </summary>
	public virtual StateMod_WellRight getLastRight()
	{
		if ((_rights == null) || (_rights.Count == 0))
		{
			return null;
		}
		return (StateMod_WellRight)_rights[_rights.Count - 1];
	}

	/// <returns> the number of return flow locations.
	/// There is not a set function for this data because it is automatically
	/// calculated whenever a return flow is added or removed. </returns>
	public virtual int getNrtnw()
	{
		return _rivret.Count;
	}

	/// <returns> the number of depletion locations
	/// There is not a set function for this data because it is automatically
	/// calculated whenever a depletion location is added or removed. </returns>
	public virtual int getNrtnw2()
	{
		return _depl.Count;
	}

	/// <summary>
	/// Return the list of parcels. </summary>
	/// <returns> the list of parcels. </returns>
	public virtual IList<StateMod_Parcel> getParcels()
	{
		return _parcel_Vector;
	}

	/// <returns> the priority switch </returns>
	public virtual double getPrimary()
	{
		return _primary;
	}

	/// <summary>
	/// Return a list of primary option strings, for use in GUIs.
	/// The options are of the form "0" if include_notes is false and
	/// "0 - Off", if include_notes is true. </summary>
	/// <returns> a list primary switch option strings, for use in GUIs. </returns>
	/// <param name="include_notes"> Indicate whether notes should be added after the parameter values. </param>
	public static IList<string> getPrimaryChoices(bool include_notes)
	{
		IList<string> v = new List<string>(52);
		v.Add("0 - Use water right priorities");
		for (int i = 1000; i < 50000; i += 1000)
		{
			v.Add("" + i + " - Well water rights will be adjusted by " + i);
		}
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
	/// Return the default primary switch choice.  This can be used by GUI code to pick a default for a new well. </summary>
	/// <returns> the default primary choice. </returns>
	public static string getPrimaryDefault(bool include_notes)
	{ // Make this aggree with the above method...
		if (include_notes)
		{
			return ("0 - Use water right priorities");
		}
		else
		{
			return "0";
		}
	}

	/// <returns> the return flow at a particular index </returns>
	/// <param name="index"> index desired to retrieve </param>
	/// <exception cref="ArrayIndexOutOfBounds"> throws exception if unable to retrieve the specified return flow node </exception>
	public virtual StateMod_ReturnFlow getReturnFlow(int index)
	{
		return _rivret[index];
	}

	/// <returns> the return flow list </returns>
	public virtual IList<StateMod_ReturnFlow> getReturnFlows()
	{
		return _rivret;
	}

	/// <summary>
	/// Return the right associated with the given index.  If index
	/// number of rights don't exist, null will be returned. </summary>
	/// <param name="index"> desired right index </param>
	public virtual StateMod_WellRight getRight(int index)
	{
		if ((index < 0) || (index >= _rights.Count))
		{
			return null;
		}
		else
		{
			return (StateMod_WellRight)_rights[index];
		}
	}

	/// <returns> rights list </returns>
	public virtual IList<StateMod_WellRight> getRights()
	{
		return _rights;
	}

	/// <summary>
	/// Indicate if the well is a DivAndWell (D&W, DW) node, indicated by an associated diversion ID in idvcow2 </summary>
	/// <returns> true if "idvcow2" is not blank or "NA", false otherwise. </returns>
	public virtual bool hasAssociatedDiversion()
	{
		if ((!string.ReferenceEquals(_idvcow2, null)) && !_idvcow2.Equals("") && !_idvcow2.Equals("NA", StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Indicate whether the well has groundwater only supply.  This will
	/// be the case if the location is a collection with part type of "Parcel" or "Well".
	/// </summary>
	public virtual bool hasGroundwaterOnlySupply()
	{
		string collectionPartType = getCollectionPartType();
		if (isCollection() && (collectionPartType.Equals(COLLECTION_PART_TYPE_PARCEL, StringComparison.OrdinalIgnoreCase) || (collectionPartType.Equals(COLLECTION_PART_TYPE_WELL, StringComparison.OrdinalIgnoreCase))))
		{
			// TODO SAM 2007-05-11 Rectify part types with StateCU
			return true;
		}
		return false;
	}

	/// <summary>
	/// Indicate whether the well has surface water supply.  This will
	/// be the case if the location is NOT a groundwater only supply location.
	/// </summary>
	public virtual bool hasSurfaceWaterSupply()
	{
		if (hasGroundwaterOnlySupply())
		{
			return false;
		}
		return true;
	}

	/// <summary>
	/// Set default values for all arguments </summary>
	/// <param name="initialize_defaults"> If true, initialize data to reasonable values.  If
	/// false, initialize to missing values. </param>
	private void initialize(bool initialize_defaults)
	{
		_smdata_type = StateMod_DataSet.COMP_WELL_STATIONS;
		_rivret = new List<StateMod_ReturnFlow>(10);
		_depl = new List<StateMod_ReturnFlow>(10);
		_pumping_MonthTS = null;
		_pumping_DayTS = null;
		_demand_MonthTS = null;
		_demand_DayTS = null;
		_ipy_YearTS = null;
		_cwr_MonthTS = null;
		_cwr_DayTS = null;
		_rights = new List<StateMod_WellRight>();
		_georecord = null;

		if (initialize_defaults)
		{
			_cdividyw = "0"; // Estimate average daily from monthly data.
			_idvcow2 = "N/A";
			_diveff = new double[12];
			for (int i = 0; i < 12; i++)
			{
				_diveff[i] = 60.0;
			}
			_divcapw = 0;
			_idvcomw = 1;
			_divefcw = -60.0; // Indicate to use monthly efficiencies
			_areaw = 0.0;
			_irturnw = 0;
			_demsrcw = 1;
			_primary = 0;
		}
		else
		{
			_cdividyw = "";
			_idvcow2 = "";
			_diveff = new double[12];
			for (int i = 0; i < 12; i++)
			{
				_diveff[i] = StateMod_Util.MISSING_DOUBLE;
			}
			_divcapw = StateMod_Util.MISSING_DOUBLE;
			_idvcomw = StateMod_Util.MISSING_INT;
			_divefcw = StateMod_Util.MISSING_DOUBLE;
			_areaw = StateMod_Util.MISSING_DOUBLE;
			_irturnw = StateMod_Util.MISSING_INT;
			_demsrcw = StateMod_Util.MISSING_INT;
			_primary = StateMod_Util.MISSING_INT;
		}
	}

	/// <summary>
	/// Indicate whether the well is a collection (an aggregate or system). </summary>
	/// <returns> true if the well is an aggregate or system. </returns>
	public virtual bool isCollection()
	{
		if (__collectionIDList == null)
		{
			return false;
		}
		else
		{
			return true;
		}
	}

	/// <summary>
	/// Indicate whether a file is a StateMod well file.  Currently the only
	/// check that is done is to see if the file name ends in "wes". </summary>
	/// <param name="filename"> File name. </param>
	/// <returns> true if the file appears to be a well file, false if not. </returns>
	public static bool isStateModWellFile(string filename)
	{
		if (StringUtil.endsWithIgnoreCase(filename,".wes"))
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Read a well input file. </summary>
	/// <param name="filename"> name of file containing well information </param>
	/// <returns> status(always 0 since exception handling is now used) </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.List<StateMod_Well> readStateModFile(String filename) throws Exception
	public static IList<StateMod_Well> readStateModFile(string filename)
	{
		string routine = "StateMod_Well.readStateModFile";
		IList<StateMod_Well> theWellStations = new List<StateMod_Well>();
		int[] format_1 = new int[] {StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_INTEGER, StringUtil.TYPE_DOUBLE, StringUtil.TYPE_SPACE, StringUtil.TYPE_STRING, StringUtil.TYPE_SPACE, StringUtil.TYPE_DOUBLE};
		int[] format_1w = new int[] {12, 24, 12, 8, 8, 1, 12, 1, 12};
		int[] format_2 = new int[] {StringUtil.TYPE_SPACE, StringUtil.TYPE_STRING, StringUtil.TYPE_INTEGER, StringUtil.TYPE_INTEGER, StringUtil.TYPE_INTEGER, StringUtil.TYPE_DOUBLE, StringUtil.TYPE_DOUBLE, StringUtil.TYPE_INTEGER, StringUtil.TYPE_INTEGER};
		int[] format_2w = new int[] {36, 12, 8, 8, 8, 8, 8, 8, 8};
		int[] format_4 = new int[] {StringUtil.TYPE_SPACE, StringUtil.TYPE_STRING, StringUtil.TYPE_DOUBLE, StringUtil.TYPE_INTEGER};
		int[] format_4w = new int[] {36, 12, 8, 8};
		string iline = null;
		string s = null;
		IList<object> v = new List<object>(9);
		StreamReader @in = null;
		StateMod_Well aWell = null;
		StateMod_ReturnFlow aReturnNode = null;
		IList<string> effv = null;
		int nrtn, ndepl;

		Message.printStatus(1, routine, "Reading well file: " + filename);

		try
		{
			@in = new StreamReader(IOUtil.getPathUsingWorkingDir(filename));
			while (!string.ReferenceEquals((iline = @in.ReadLine()), null))
			{
				if (iline.StartsWith("#", StringComparison.Ordinal) || iline.Trim().Length == 0)
				{
					continue;
				}

				aWell = new StateMod_Well();

				StringUtil.fixedRead(iline, format_1, format_1w, v);
				if (Message.isDebugOn)
				{
					Message.printDebug(50, routine, "iline: " + iline);
				}
				aWell.setID(((string)v[0]).Trim());
				aWell.setName(((string)v[1]).Trim());
				aWell.setCgoto(((string)v[2]).Trim());
				aWell.setSwitch((int?)v[3]);
				aWell.setDivcapw((double?)v[4]);
				aWell.setCdividyw(((string)v[5]).Trim());
				aWell.setPrimary((double?)v[6]);

				// user data

				iline = @in.ReadLine();
				StringUtil.fixedRead(iline, format_2, format_2w, v);
				if (Message.isDebugOn)
				{
					Message.printDebug(50, routine, "iline: " + iline);
				}
				aWell.setIdvcow2(((string)v[0]).Trim());
				aWell.setIdvcomw((int?)v[1]);
				// Don't set the number of return flow data get(2) or depletion data get(3) because
				// those will be calculated
				nrtn = ((int?)v[2]).Value;
				ndepl = ((int?)v[3]).Value;
				aWell.setDivefcw((double?)v[4]);
				aWell.setAreaw((double?)v[5]);
				aWell.setIrturnw((int?)v[6]);
				aWell.setDemsrcw((int?)v[7]);
				if (aWell.getDivefcw() >= 0)
				{
					// Efficiency line won't be included - set each value to the average
					for (int i = 0; i < 12; i++)
					{
						aWell.setDiveff(i, aWell.getDivefcw());
					}
				}
				else
				{
					// 12 efficiency values
					iline = @in.ReadLine();
					effv = StringUtil.breakStringList(iline, " ", StringUtil.DELIM_SKIP_BLANKS);
					for (int i = 0; i < 12; i++)
					{
						aWell.setDiveff(i, (string)effv[i]);
					}
				}

				// return flow data

				if (Message.isDebugOn)
				{
					Message.printDebug(50, routine, "Number of return flows: " + nrtn);
				}
				for (int i = 0; i < nrtn; i++)
				{
					iline = @in.ReadLine();
					StringUtil.fixedRead(iline, format_4, format_4w, v);
					if (Message.isDebugOn)
					{
						Message.printDebug(50, routine, "Fixed read returned " + v.Count + " elements");
					}

					aReturnNode = new StateMod_ReturnFlow(StateMod_DataSet.COMP_WELL_STATIONS);
					s = ((string)v[0]).Trim();
					if (s.Length <= 0)
					{
						aReturnNode.setCrtnid(s);
						Message.printWarning(2, routine, "Return node for structure \"" + aWell.getID() + "\" is blank. ");
					}
					else
					{
						aReturnNode.setCrtnid(s);
					}

					aReturnNode.setPcttot(((double?)v[1]));
					aReturnNode.setIrtndl(((int?)v[2]));
					aWell.addReturnFlow(aReturnNode);
				}

				// depletion data

				for (int i = 0; i < ndepl; i++)
				{
					iline = @in.ReadLine();
					StringUtil.fixedRead(iline, format_4, format_4w, v);

					aReturnNode = new StateMod_ReturnFlow(StateMod_DataSet.COMP_WELL_STATIONS);
					s = ((string)v[0]).Trim();
					if (s.Length <= 0)
					{
						aReturnNode.setCrtnid(s);
						Message.printWarning(2, routine, "Return node for structure \"" + aWell.getID() + "\" is blank. ");
					}
					else
					{
						aReturnNode.setCrtnid(s);
					}

					aReturnNode.setPcttot(((double?)v[1]));
					aReturnNode.setIrtndl(((int?)v[2]));
					aWell.addDepletion(aReturnNode);
				}

				theWellStations.Add(aWell);
			}
		}
		catch (Exception e)
		{
			routine = null;
			format_1 = null;
			format_1w = null;
			format_2 = null;
			format_2w = null;
			format_4 = null;
			format_4w = null;
			s = null;
			v = null;
			if (@in != null)
			{
				@in.Close();
			}
			@in = null;
			aWell = null;
			aReturnNode = null;
			effv = null;
			Message.printWarning(2, routine, e);
			throw e;
		}
		routine = null;
		format_1 = null;
		format_1w = null;
		format_2 = null;
		format_2w = null;
		format_4 = null;
		format_4w = null;
		iline = null;
		s = null;
		v = null;
		if (@in != null)
		{
			@in.Close();
		}
		@in = null;
		aWell = null;
		aReturnNode = null;
		effv = null;
		return theWellStations;
	}

	/// <summary>
	/// Cancels any changes made to this object within a GUI since createBackup()
	/// was called and sets _original to null.
	/// </summary>
	public override void restoreOriginal()
	{
		StateMod_Well w = (StateMod_Well)_original;
		base.restoreOriginal();

		_cdividyw = w._cdividyw;
		_divcapw = w._divcapw;
		_idvcow2 = w._idvcow2;
		_idvcomw = w._idvcomw;
		_divefcw = w._divefcw;
		_areaw = w._areaw;
		_irturnw = w._irturnw;
		_demsrcw = w._demsrcw;
		_diveff = w._diveff;
		_primary = w._primary;
		_isClone = false;
		_original = null;
	}

	/// <summary>
	/// Set the area. </summary>
	/// <param name="area"> area </param>
	public virtual void setAreaw(double area)
	{
		if (area != _areaw)
		{
			_areaw = area;
			setDirty(true);
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_WELL_STATIONS, true);
			}
		}
	}

	/// <summary>
	/// Set the well area. </summary>
	/// <param name="area"> well area(AF) </param>
	public virtual void setAreaw(double? area)
	{
		if (area == null)
		{
			return;
		}
		setAreaw(area.Value);
	}

	/// <summary>
	/// Set the area(This is currently not being used but is provided for
	/// consistency within this class). </summary>
	/// <param name="area"> area </param>
	public virtual void setAreaw(string area)
	{
		if (string.ReferenceEquals(area, null))
		{
			return;
		}
		setAreaw(StringUtil.atod(area.Trim()));
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
	/// Set the average monthly historical historical pumping (12 monthly values +
	/// annual average), for the data set calendar type.  This is ONLY used by StateDMI
	/// and does not need to be considered in comparison code.
	/// </summary>
	public virtual void setAverageMonthlyHistoricalPumping(double[] weh_monthly)
	{
		__weh_monthly = weh_monthly;
	}

	/// <summary>
	/// Set the average monthly efficiencies calculated from CWR and historical
	/// pumping (12 monthly values + annual average), for the
	/// data set calendar type.  This is ONLY used by StateDMI and does not need
	/// to be considered in comparison code.
	/// </summary>
	public virtual void setCalculatedEfficiencies(double[] calculated_efficiencies)
	{
		__calculated_efficiencies = calculated_efficiencies;
	}

	/// <summary>
	/// Set the standard deviation of monthly efficiencies calculated from CWR and
	/// historical pumping (12 monthly values + annual average), for the
	/// data set calendar type.  This is ONLY used by StateDMI and does not need
	/// to be considered in comparison code.
	/// </summary>
	public virtual void setCalculatedEfficiencyStddevs(double[] calculated_efficiency_stddevs)
	{
		__calculated_efficiency_stddevs = calculated_efficiency_stddevs;
	}

	/// <summary>
	/// Set the well id to use for daily data </summary>
	/// <param name="cdividyw"> well id to use for daily data </param>
	public virtual void setCdividyw(string cdividyw)
	{
		if (string.ReferenceEquals(cdividyw, null))
		{
			return;
		}
		if (!cdividyw.Equals(_cdividyw))
		{
			_cdividyw = cdividyw.Trim();
			setDirty(true);
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_WELL_STATIONS, true);
			}
		}
	}

	/// <summary>
	/// Set the collection division.  This is needed to uniquely identify the parcels. </summary>
	/// <param name="collection_div"> The division for the collection. </param>
	public virtual void setCollectionDiv(int collection_div)
	{
		__collection_div = collection_div;
	}

	/// <summary>
	/// Set the collection list for an aggregate/system for the entire period, used when specifying well ID lists. </summary>
	/// <param name="partIdList"> The identifiers indicating the locations in the collection. </param>
	public virtual void setCollectionPartIDs(IList<string> partIdList, IList<string> partIdTypeList)
	{
		__collectionIDList = new List<IList<string>> (1);
			__collectionIDList.Add(partIdList);
			__collectionIDTypeList = new List<IList<string>> (1);
			__collectionIDTypeList.Add(partIdTypeList);
			__collectionYear = new int[1];
			__collectionYear[0] = 0;
	}

	/// <summary>
	/// Set the collection list for an aggregate/system for a specific year. </summary>
	/// <param name="year"> The year to which the collection applies. </param>
	/// <param name="partIdList"> The identifiers indicating the locations in the collection. </param>
	public virtual void setCollectionPartIDsForYear(int year, IList<string> partIdList)
	{
		int pos = -1; // Position of year in data lists.
		if (__collectionIDList == null)
		{
			// No previous data so create memory...
			__collectionIDList = new List<IList<string>> (1);
			__collectionIDList.Add(partIdList);
			__collectionYear = new int[1];
			__collectionYear[0] = year;
		}
		else
		{
			// See if the year matches any previous contents...
			for (int i = 0; i < __collectionYear.Length; i++)
			{
				if (year == __collectionYear[i])
				{
					pos = i;
					break;
				}
			}
			// Now assign...
			if (pos < 0)
			{
				// Need to add an item...
				pos = __collectionYear.Length;
				__collectionIDList.Add(partIdList);
				int[] temp = new int[__collectionYear.Length + 1];
				for (int i = 0; i < __collectionYear.Length; i++)
				{
					temp[i] = __collectionYear[i];
				}
				__collectionYear = temp;
				__collectionYear[pos] = year;
			}
			else
			{
				// Existing item...
				__collectionIDList[pos] = partIdList;
				__collectionYear[pos] = year;
			}
		}
	}

	/// <summary>
	/// Return the collection part ID type list.  This is used with well locations when aggregating
	/// by well identifiers (WDIDs and permit receipt numbers). </summary>
	/// <returns> the list of collection part ID types, or null if not defined. </returns>
	public virtual IList<string> getCollectionPartIDTypes()
	{
		if (__collectionIDTypeList == null)
		{
			return null;
		}
		else
		{
			return __collectionIDTypeList[0]; // Currently does not vary by year
		}
	}

	/// <summary>
	/// Set the collection part type. </summary>
	/// <param name="collection_part_type"> The collection part type (see COLLECTION_PART_TYPE_*). </param>
	public virtual void setCollectionPartType(string collection_part_type)
	{
		__collection_part_type = collection_part_type;
	}

	/// <summary>
	/// Set the collection type. </summary>
	/// <param name="collection_type"> The collection type, either "Aggregate" or "System". </param>
	public virtual void setCollectionType(string collection_type)
	{
		__collection_type = collection_type;
	}

	/// <summary>
	/// Set the consumptive water requirement daily time series for the well structure.
	/// </summary>
	public virtual void setConsumptiveWaterRequirementDayTS(DayTS cwr_DayTS)
	{
		_cwr_DayTS = cwr_DayTS;
	}

	/// <summary>
	/// Set the consumptive water requirement monthly time series for the well structure.
	/// </summary>
	public virtual void setConsumptiveWaterRequirementMonthTS(MonthTS cwr_MonthTS)
	{
		_cwr_MonthTS = cwr_MonthTS;
	}

	/// <summary>
	/// Set the demand time series "pointer" to the daily demand time series. </summary>
	/// <param name="demand_DayTS"> time series known to refer to this well. </param>
	public virtual void setDemandDayTS(DayTS demand_DayTS)
	{
		_demand_DayTS = demand_DayTS;
	}

	/// <summary>
	/// Set the demand time series "pointer" to the monthly demand time series. </summary>
	/// <param name="demand_MonthTS"> time series known to refer to this well. </param>
	public virtual void setDemandMonthTS(MonthTS demand_MonthTS)
	{
		_demand_MonthTS = demand_MonthTS;
	}

	/// <summary>
	/// Set the irrigated acreage source(see StateMod documentation for list of available sources). </summary>
	/// <param name="demsrcw"> source for irrigated acreage </param>
	public virtual void setDemsrcw(int demsrcw)
	{
		if (demsrcw != _demsrcw)
		{
			_demsrcw = demsrcw;
			setDirty(true);
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_WELL_STATIONS, true);
			}
		}
	}

	/// <summary>
	/// Set the irrigated acreage source(see StateMod documentation for list of available sources). </summary>
	/// <param name="demsrcw"> source for irrigated acreage </param>
	public virtual void setDemsrcw(int? demsrcw)
	{
		if (demsrcw == null)
		{
			return;
		}
		setDemsrcw(demsrcw.Value);
	}

	/// <summary>
	/// Set the irrigated acreage source(see StateMod documentation for list of available sources). </summary>
	/// <param name="demsrcw"> source for irrigated acreage </param>
	public virtual void setDemsrcw(string demsrcw)
	{
		if (string.ReferenceEquals(demsrcw, null))
		{
			return;
		}
		setDemsrcw(StringUtil.atoi(demsrcw.Trim()));
	}

	// TODO - need to handle dirty flag
	public virtual void setDepletions(IList<StateMod_ReturnFlow> depl)
	{
		_depl = depl;
	}

	/// <summary>
	/// Set the well capacity </summary>
	/// <param name="divcapw"> well capacity(cfs) </param>
	public virtual void setDivcapw(double divcapw)
	{
		if (divcapw != _divcapw)
		{
			_divcapw = divcapw;
			setDirty(true);
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_WELL_STATIONS, true);
			}
		}
	}

	/// <summary>
	/// Set the well capacity </summary>
	/// <param name="divcapw"> well capacity(cfs) </param>
	public virtual void setDivcapw(double? divcapw)
	{
		if (divcapw == null)
		{
			return;
		}
		setDivcapw(divcapw.Value);
	}

	/// <summary>
	/// Set the well capacity </summary>
	/// <param name="divcapw"> well capacity(cfs) </param>
	public virtual void setDivcapw(string divcapw)
	{
		if (string.ReferenceEquals(divcapw, null))
		{
			return;
		}
		setDivcapw(StringUtil.atod(divcapw.Trim()));
	}

	/// <summary>
	/// Set the system efficiency </summary>
	/// <param name="divefcw"> efficiency of the system.  If negative, 12 efficiency values will be used. </param>
	public virtual void setDivefcw(double divefcw)
	{
		if (divefcw != _divefcw)
		{
			_divefcw = divefcw;
			setDirty(true);
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_WELL_STATIONS, true);
			}
		}
	}

	/// <summary>
	/// Set the system efficiency </summary>
	/// <param name="divefcw"> efficiency of the system.  If negative, 12 efficiency values will be used. </param>
	/// <seealso cref= StateMod_Well#setDiveff </seealso>
	public virtual void setDivefcw(double? divefcw)
	{
		if (divefcw == null)
		{
			return;
		}
		setDivefcw(divefcw.Value);
	}

	/// <summary>
	/// Set the system efficiency </summary>
	/// <param name="divefcw"> efficiency of the system.  If negative, 12 efficiency values will be used. </param>
	/// <seealso cref= StateMod_Well#setDiveff </seealso>
	public virtual void setDivefcw(string divefcw)
	{
		if (string.ReferenceEquals(divefcw, null))
		{
			return;
		}
		setDivefcw(StringUtil.atod(divefcw.Trim()));
	}

	/// <summary>
	/// Set the variable efficiency </summary>
	/// <param name="index"> index of month for which to set efficiency(0-11) </param>
	/// <param name="diveff"> eff value(0-100) </param>
	public virtual void setDiveff(int index, double? diveff)
	{
		if (diveff == null)
		{
			return;
		}
		setDiveff(index, diveff.Value);
	}

	/// <summary>
	/// Set the system efficiency for a particular month.
	/// The efficiencies are specified with month 0 being January. </summary>
	/// <param name="index"> month index (0=January). </param>
	/// <param name="diveff"> monthly efficiency </param>
	/// <param name="yeartype"> The year type for the diversion stations file (consistent with
	/// the control file for a full data set). </param>
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
	/// Set the variable efficiency </summary>
	/// <param name="index"> index of month for which to set efficiency(0-11) </param>
	/// <param name="diveff"> eff value(0-100) </param>
	public virtual void setDiveff(int index, double diveff)
	{
		if (index < 0 || index > 11)
		{
			Message.printWarning(2, "setDiveff", "Unable to set efficiency for month index " + index);
			return;
		}
		if (diveff != _diveff[index])
		{
			_diveff[index] = diveff;
			setDirty(true);
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_WELL_STATIONS, true);
			}
		}
	}

	/// <summary>
	/// Set the variable efficiency </summary>
	/// <param name="index"> index of month for which to set efficiency(0-11) </param>
	/// <param name="diveff"> eff value(0-100) </param>
	public virtual void setDiveff(int index, string diveff)
	{
		if (string.ReferenceEquals(diveff, null))
		{
			return;
		}
		setDiveff(index, StringUtil.atod(diveff.Trim()));
	}

	/// <summary>
	/// Set the geographic information object associated with the well. </summary>
	/// <param name="georecord"> Geographic record associated with the well. </param>
	public virtual void setGeoRecord(GeoRecord georecord)
	{
		_georecord = georecord;
	}

	/// <summary>
	/// Set the historical daily pumping time series. </summary>
	/// <param name="pumping_DayTS"> time series known to refer to this well. </param>
	public virtual void setPumpingDayTS(DayTS pumping_DayTS)
	{
		_pumping_DayTS = pumping_DayTS;
	}

	/// <summary>
	/// Set the historical monthly pumping time series. </summary>
	/// <param name="pumping_MonthTS"> time series known to refer to this well. </param>
	public virtual void setPumpingMonthTS(MonthTS pumping_MonthTS)
	{
		_pumping_MonthTS = pumping_MonthTS;
	}

	/// <summary>
	/// Set the demand code </summary>
	/// <param name="idvcomw"> demand code to use </param>
	public virtual void setIdvcomw(int idvcomw)
	{
		if (idvcomw != _idvcomw)
		{
			_idvcomw = idvcomw;
			setDirty(true);
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_WELL_STATIONS, true);
			}
		}
	}

	/// <summary>
	/// Set the demand code </summary>
	/// <param name="idvcomw"> demand code to use </param>
	public virtual void setIdvcomw(int? idvcomw)
	{
		if (idvcomw == null)
		{
			return;
		}
		setIdvcomw(idvcomw.Value);
	}

	/// <summary>
	/// Set the demand code </summary>
	/// <param name="idvcomw"> demand code to use </param>
	public virtual void setIdvcomw(string idvcomw)
	{
		if (string.ReferenceEquals(idvcomw, null))
		{
			return;
		}
		setIdvcomw(StringUtil.atoi(idvcomw.Trim()));
	}

	/// <summary>
	/// Set the diversion this well is tied to </summary>
	/// <param name="idvcow2"> diversion this well is tied to </param>
	public virtual void setIdvcow2(string idvcow2)
	{
		if (string.ReferenceEquals(idvcow2, null))
		{
			return;
		}
		if (!idvcow2.Equals(_idvcow2))
		{
			_idvcow2 = idvcow2;
			setDirty(true);
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_WELL_STATIONS, true);
			}
		}
	}

	/// <summary>
	/// Set the use type </summary>
	/// <param name="irturnw"> use type </param>
	public virtual void setIrturnw(int irturnw)
	{
		if (irturnw != _irturnw)
		{
			_irturnw = irturnw;
			setDirty(true);
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_WELL_STATIONS, true);
			}
		}
	}

	/// <summary>
	/// Set the use type </summary>
	/// <param name="irturnw"> use type </param>
	public virtual void setIrturnw(int? irturnw)
	{
		if (irturnw == null)
		{
			return;
		}
		setIrturnw(irturnw.Value);
	}

	/// <summary>
	/// Set the use type </summary>
	/// <param name="irturnw"> use type </param>
	public virtual void setIrturnw(string irturnw)
	{
		if (string.ReferenceEquals(irturnw, null))
		{
			return;
		}
		setIrturnw(StringUtil.atoi(irturnw.Trim()));
	}

	/// <summary>
	/// Set the average monthly efficiencies to be used for modeling
	/// (12 monthly values + annual average), for the
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
	/// Set the Priority switch </summary>
	/// <param name="primary"> 0 = off; +n = on, adjust well rights by -n </param>
	public virtual void setPrimary(double primary)
	{
		if (primary != _primary)
		{
			_primary = primary;
			setDirty(true);
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_WELL_STATIONS, true);
			}
		}
	}

	/// <summary>
	/// Set the Priority switch </summary>
	/// <param name="primary">  0 = off; +n = on, adjust well rights by -n </param>
	public virtual void setPrimary(double? primary)
	{
		if (primary == null)
		{
			return;
		}
		setPrimary(primary.Value);
	}

	/// <summary>
	/// Set the Priority switch </summary>
	/// <param name="primary"> 0 = off; +n = on, adjust well rights by -n </param>
	public virtual void setPrimary(string primary)
	{
		if (string.ReferenceEquals(primary, null))
		{
			return;
		}
		setPrimary(StringUtil.atod(primary.Trim()));
	}

	// TODO - need to handle dirty flag
	public virtual void setReturnFlows(IList<StateMod_ReturnFlow> rivret)
	{
		_rivret = rivret;
	}

	// TODO - need to handle dirty flag
	public virtual void setRights(IList<StateMod_WellRight> rights)
	{
		_rights = rights;
	}

	/// <summary>
	/// Performs validation for this object within a StateMod data set. </summary>
	/// <param name="dataset"> a StateMod_DataSet that is managing this object. </param>
	/// <returns> validation results, from which information about problems can be extracted. </returns>
	public virtual StateMod_ComponentValidation validateComponent(StateMod_DataSet dataset)
	{
		StateMod_ComponentValidation validation = new StateMod_ComponentValidation();
		string id = getID();
		string name = getName();
		string riverID = getCgoto();
		double capacity = getDivcapw();
		string dailyID = getCdividyw();
		double primary = getPrimary();

		string idvcow2 = getIdvcow2();
		int idvcomw = getIdvcomw();
		int nrtnw = getNrtnw();
		int nrtnw2 = getNrtnw2();
		double divefcw = getDivefcw();
		double areaw = getAreaw();
		int irturnw = getIrturnw();
		int demsrcw = getDemsrcw();
		// Make sure that basic information is not empty
		if (StateMod_Util.isMissing(id))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Well identifier is blank.", "Specify a station identifier."));
		}
		if (StateMod_Util.isMissing(name))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Well \"" + id + "\" name is blank.", "Specify a well name to clarify data."));
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
			validation.add(new StateMod_ComponentValidationProblem(this,"Well \"" + id + "\" river ID is blank.", "Specify a river ID to associate the well with a river network node."));
		}
		else
		{
			// Verify that the river node is in the data set, if the network is available
			if (rinList != null)
			{
				if (StateMod_Util.IndexOf(rinList, riverID) < 0)
				{
					validation.add(new StateMod_ComponentValidationProblem(this,"Well \"" + id + "\" river network ID (" + riverID + ") is not found in the list of river network nodes.", "Specify a valid river network ID to associate the well with a river network node."));
				}
			}
		}
		if (!(capacity >= 0.0))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Well \"" + id + "\" capacity (" + StringUtil.formatString(capacity,"%.2f") + ") is invalid.", "Specify the capacity as a number >= 0."));
		}
		// Verify that the daily ID is in the data set (daily ID is allowed to be missing)
		if ((dataset != null) && !StateMod_Util.isMissing(dailyID))
		{
			DataSetComponent comp2 = dataset.getComponentForComponentType(StateMod_DataSet.COMP_WELL_STATIONS);
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<StateMod_Well> wesList = (java.util.List<StateMod_Well>)comp2.getData();
			IList<StateMod_Well> wesList = (IList<StateMod_Well>)comp2.getData();
			if (dailyID.Equals("0") || dailyID.Equals("3") || dailyID.Equals("4"))
			{
				// OK
			}
			else if ((wesList != null) && (wesList.Count > 0))
			{
				// Check the diversion station list
				if (StateMod_Util.IndexOf(wesList, dailyID) < 0)
				{
					validation.add(new StateMod_ComponentValidationProblem(this,"Well \"" + id + "\" daily ID (" + dailyID + ") is not 0, 3, or 4 and is not found in the list of well stations.", "Specify the daily ID as 0, 3, 4, or a matching well ID."));
				}
			}
		}
		if (!(primary >= 0.0))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Well \"" + id + "\" primary switch (" + StringUtil.formatString(primary,"%.2f") + ") is invalid.", "Specify the primary switch as >= 0."));
		}
		// Verify that the diversion ID is in the data set (diversion ID is allowed to be missing)
		if ((dataset != null) && !StateMod_Util.isMissing(idvcow2))
		{
			DataSetComponent comp2 = dataset.getComponentForComponentType(StateMod_DataSet.COMP_DIVERSION_STATIONS);
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<StateMod_Diversion> ddsList = (java.util.List<StateMod_Diversion>)comp2.getData();
			IList<StateMod_Diversion> ddsList = (IList<StateMod_Diversion>)comp2.getData();
			if (!dailyID.Equals("NA") && !dailyID.Equals("N/A"))
			{
				// OK
			}
			else if ((ddsList != null) && (ddsList.Count > 0))
			{
				// Check the diversion station list
				if (StateMod_Util.IndexOf(ddsList, idvcow2) < 0)
				{
					validation.add(new StateMod_ComponentValidationProblem(this,"Well \"" + id + "\" diversion ID (" + idvcow2 + ") is not \"NA\" and is not found in the list of diversion stations.", "Specify the diversion ID as \"NA\" or a matching diversion ID."));
				}
			}
		}
		IList<string> choices = getIdvcomwChoices(false);
		if (StringUtil.indexOfIgnoreCase(choices,"" + idvcomw) < 0)
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Well \"" + id + "\" data type (" + idvcomw + ") is invalid.", "Specify the data type as one of " + choices));
		}
		if (!(nrtnw >= 0))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Well \"" + id + "\" number of return flow locations (" + nrtnw + ") is invalid.", "Specify the number of return flow locations as >= 0."));
		}
		if (!((divefcw >= -100.0) && (divefcw <= 100.0)))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Well \"" + id + "\" annual system efficiency (" + StringUtil.formatString(divefcw,"%.2f") + ") is invalid.", "Specify the efficiency as 0 to 100 for annual (negative if monthly values are provided)."));
		}
		else if (divefcw < 0.0)
		{
			// Check that each monthly efficiency is in the range 0 to 100
			double diveffw;
			for (int i = 0; i < 12; i++)
			{
				diveffw = getDiveff(i);
				if (!((diveffw >= 0.0) && (diveffw <= 100.0)))
				{
					validation.add(new StateMod_ComponentValidationProblem(this,"Well \"" + id + "\" system efficiency for month " + (i + 1) + " (" + StringUtil.formatString(diveffw,"%.2f") + ") is invalid.", "Specify the efficiency as 0 to 100."));
				}
			}
		}
		if (!(areaw >= 0.0))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Well \"" + id + "\" area (" + StringUtil.formatString(areaw,"%.2f") + ") is invalid.", "Specify the area as a number >= 0."));
		}
		choices = getIrturnwChoices(false);
		if (StringUtil.indexOfIgnoreCase(choices,"" + irturnw) < 0)
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Well \"" + id + "\" use type (" + irturnw + ") is invalid.", "Specify the use type as one of " + choices));
		}
		choices = getDemsrcwChoices(false);
		if (StringUtil.indexOfIgnoreCase(choices,"" + demsrcw) < 0)
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Well \"" + id + "\" demand source (" + demsrcw + ") is invalid.", "Specify the demand source as one of " + choices));
		}
		// Check return flow locations...
		StateMod_ReturnFlow ret;
		double pcttot;
		string crtnid;
		for (int i = 0; i < nrtnw; i++)
		{
			ret = getReturnFlow(i);
			pcttot = ret.getPcttot();
			crtnid = ret.getCrtnid();
			if (rinList != null)
			{
				// Make sure that the return location is in the network list
				if (StateMod_Util.IndexOf(rinList, crtnid) < 0)
				{
					validation.add(new StateMod_ComponentValidationProblem(this,"Well \"" + id + "\" return " + (i + 1) + " location (" + crtnid + ") is not found in the list of river network nodes.", "Specify a valid river network ID to associate the return location with a river network node."));
				}
			}
			if (!((pcttot >= 0.0) && (pcttot <= 100.0)))
			{
				validation.add(new StateMod_ComponentValidationProblem(this,"Well \"" + id + "\" return " + (i + 1) + " percent (" + StringUtil.formatString(pcttot,"%.2f") + ") is invalid.", "Specify the return percent as a number 0 to 100."));
			}
		}
		// Check depletion locations...
		StateMod_ReturnFlow depl;
		double pcttot2;
		string crtnid2;
		for (int i = 0; i < nrtnw2; i++)
		{
			depl = getDepletion(i);
			pcttot2 = depl.getPcttot();
			crtnid2 = depl.getCrtnid();
			if (rinList != null)
			{
				// Make sure that the return location is in the network list
				if (StateMod_Util.IndexOf(rinList, crtnid2) < 0)
				{
					validation.add(new StateMod_ComponentValidationProblem(this,"Well \"" + id + "\" depletion " + (i + 1) + " location (" + crtnid2 + ") is not found in the list of river network nodes.", "Specify a valid river network ID to associate the depletion location with a river network node."));
				}
			}
			if (!((pcttot2 >= 0.0) && (pcttot2 <= 100.0)))
			{
				validation.add(new StateMod_ComponentValidationProblem(this,"Well \"" + id + "\" depletion " + (i + 1) + " percent (" + StringUtil.formatString(pcttot2,"%.2f") + ") is invalid.", "Specify the depletion percent as a number 0 to 100."));
			}
		}
		// TODO SAM 2009-06-01) evaluate how to check rights (with getRights() or checking the rights data
		// set component).
		bool checkRights = false;
		IList<StateMod_WellRight> wer_Vector = null;
		if (dataset != null)
		{
			DataSetComponent wer_comp = dataset.getComponentForComponentType(StateMod_DataSet.COMP_WELL_RIGHTS);
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<StateMod_WellRight> wer_Vector0 = (java.util.List<StateMod_WellRight>)wer_comp.getData();
			IList<StateMod_WellRight> wer_Vector0 = (IList<StateMod_WellRight>)wer_comp.getData();
			wer_Vector = wer_Vector0;
		}

		// Check to see if any parcels were associated with wells

		StateMod_Parcel parcel = null; // Parcel associated with a well station
		string id_i = null;
		int wes_parcel_count = 0; // Parcel count for well station
		double wes_parcel_area = 0.0; // Area of parcels for well station
		int wes_well_parcel_count = 0; // Parcel (with wells) count for well station.
		double wes_well_parcel_area = 0.0; // Area of parcels with wells for well station.
		IList<StateMod_Parcel> parcel_Vector; // List of parcels for well station.

		id_i = getID();
		if (getAreaw() <= 0.0)
		{
			if (checkRights)
			{
				System.Collections.IList rights = StateMod_Util.getRightsForStation(id_i, wer_Vector);
				if ((rights != null) && (rights.Count != 0))
				{
					return null;
				}
			}
			// Check for parcels...
			wes_parcel_count = 0;
			wes_parcel_area = 0.0;
			wes_well_parcel_count = 0;
			wes_well_parcel_area = 0.0;
			parcel_Vector = getParcels();
			if (parcel_Vector != null)
			{
				wes_parcel_count = parcel_Vector.Count;
				for (int j = 0; j < wes_parcel_count; j++)
				{
					parcel = parcel_Vector[j];
					if (parcel.getArea() > 0.0)
					{
						wes_parcel_area += parcel.getArea();
					}
					if (parcel.getWellCount() > 0)
					{
						wes_well_parcel_count += parcel.getWellCount();
						wes_well_parcel_area += parcel.getArea();
					}
				}
			}
			// new format for check file
			/*String [] data_table = {
				StringUtil.formatString(count,"%4d"),
				StringUtil.formatString(id_i,"%-12.12s"),
				getName(),
				StringUtil.formatString(getCollectionType(), "%-10.10s"),
				StringUtil.formatString(wes_parcel_count,"%9d"),
				StringUtil.formatString(wes_parcel_area,"%11.0f"),
				StringUtil.formatString(wes_well_parcel_count,"%9d"),
				StringUtil.formatString(wes_well_parcel_area,"%11.0f") };*/
			if ((capacity <= 0.0) && (wes_parcel_count == 0))
			{
				validation.add(new StateMod_ComponentValidationProblem(this,"Well \"" + id + "\" capacity is zero and no parcels are associated with well station.", "Verify well input data (this check only applies when data are processed from HydroBase)."));
			}

		}
		return validation;
	}

	/// <summary>
	/// FIXME SAM 2009-06-03 Evaluate how to call from above to add more specific checks.
	/// Check the well stations.
	/// </summary>
	private void validateComponent2(IList<StateMod_WellRight> werList, IList<StateMod_Well> wesList, string idpattern_Java, int warningCount, int warningLevel, string commandTag, CommandStatus status)
	{ //String routine = getClass().getSimpleName() + ".checkWellRights";
		//String message;
		int matchCount = 0;
		/*
		StateMod_Well wes_i = null;
		StateMod_Parcel parcel = null; // Parcel associated with a well station
		int wes_parcel_count = 0; // Parcel count for well station
		double wes_parcel_area = 0.0; // Area of parcels for well station
		int wes_well_parcel_count = 0; // Parcel (with wells) count for well station.
		double wes_well_parcel_area = 0.0; // Area of parcels with wells for well station.
		List parcel_Vector; // List of parcels for well station.
		int count = 0; // Count of well stations with potential problems.
		String id_i = null;
		List rightList = null;
		int welListSize = wesList.size();
		for ( int i = 0; i < welListSize; i++ ) {
			wes_i = (StateMod_Well)wesList.get(i);
			if ( wes_i == null ) {
				continue;
			}
			id_i = wes_i.getID();
			rightList = StateMod_Util.getRightsForStation ( id_i, werList );
			// TODO SAM 2007-01-02 Evaluate how to put this code in a separate method and share between rights and stations.
			if ( (rightList == null) || (rightList.size() == 0) ) {
				// The following is essentially a copy of code for well
				// stations. Keep the code consistent.  Note that the
				// following assumes that when reading well rights from
				// HydroBase that lists of parcels are saved with well
				// stations.  This will clobber any parcel data that
				// may have been saved at the time that well stations
				// were processed (if processed in the same commands file).
				++count;
				// Check for parcels...
				wes_parcel_count = 0;
				wes_parcel_area = 0.0;
				wes_well_parcel_count = 0;
				wes_well_parcel_area = 0.0;
				parcel_Vector = wes_i.getParcels();
				if ( parcel_Vector != null ) {
					wes_parcel_count = parcel_Vector.size();
					for ( int j = 0; j < wes_parcel_count; j++ ) {
						parcel = (StateMod_Parcel)parcel_Vector.get(j);
						if ( parcel.getArea() > 0.0 ) {
							wes_parcel_area += parcel.getArea();
						}
						if ( parcel.getWellCount() > 0 ) {
							wes_well_parcel_count += parcel.getWellCount();
							wes_well_parcel_area += parcel.getArea();
						}
					}
				}
				// Format suitable for output in a list that can be copied to a spreadsheet or table.
				message_list.add (
					StringUtil.formatString(count,"%4d") +
					", " +
					StringUtil.formatString(id_i,"%-12.12s") +
					", " +
					StringUtil.formatString(
					wes_i.getCollectionType(),"%-10.10s") +
					", " +
					StringUtil.formatString(wes_parcel_count,"%9d")+
					", " +
					StringUtil.formatString(
					wes_parcel_area,"%11.0f")+
					", " +
					StringUtil.formatString(
					wes_well_parcel_count,"%9d")+
					", " +
					StringUtil.formatString(
					wes_well_parcel_area,"%11.0f")
					+ ", \"" + wes_i.getName() + "\"" );
			}
		}
		if ( message_list.size() > 0 ) {
			int line = 0;		// Line number for output (zero index).
			// Prepend introduction to the specific warnings...
			message_list.add ( line++, "" );
			message_list.add ( line++,
			"The following well stations (" + count + " out of " + size +
			") have no water rights (no irrigated parcels served by " +
			"wells)." );
			message_list.add ( line++,
			"Data may be OK if the station has no wells." );
			message_list.add ( line++, "" );
			message_list.add ( line++,
			"Parcel count and area in the following table are available " +
			"only if well rights are read from HydroBase." );
			message_list.add ( line++, "" );
			message_list.add ( line++,
			"    ,             ,           , # PARCELS, TOTAL      , # PARCELS, PARCELS    , WELL" );
			message_list.add (line++,
			"    , WELL        , COLLECTION, FOR WELL , PARCEL     , WITH     , WITH WELLS , STATION" );
			message_list.add (line++,
			"NUM., STATION ID  , TYPE      , STATION  , AREA (ACRE), WELLS    , AREA (ACRE), NAME" );
			message_list.add (line++,
			"----,-------------,-----------,----------,------------,----------,------------,-------------------------" );
		}
	
		// Check to make sure the sum of well rights equals the well station capacity...
	
		checkComponentData_WellRights_Capacity ( message_list );
		*/

		// Return values
		int[] retVals = new int[2];
		retVals[0] = matchCount;
		retVals[1] = warningCount;
		//return retVals;
	}

	/// <summary>
	/// FIXME SAM 2009-06-03 Evaluate how to integrate into checks.
	/// Helper method to check that well rights sum to the well station capacity.  This
	/// is called by the well right and well station checks.  The check is performed
	/// by formatting the capacity and decree sum to .NN precision. </summary>
	/// <param name="message_list"> Vector of string to be printed to the check file, which will
	/// be added to in this method. </param>
	private int validateComponent_checkWellRights_SumToCapacity(IList<StateMod_Well> wesList, IList<StateMod_WellRight> werList, int warningCount, int warningLevel, string commandTag, CommandStatus status)
	{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		string routine = this.GetType().FullName + "checkWellRights_SumToCapacity";
		string message;
		StateMod_WellRight wer_i = null;
		StateMod_Well wes_i = null;
		int size = 0;
		if (wesList != null)
		{
			size = wesList.Count;
		}
		double decree;
		double decree_sum;
		int onoff = 0; // On/off switch for right
		int size_rights = 0;
		string id_i = null;
		System.Collections.IList rights = null;
		for (int i = 0; i < size; i++)
		{
			wes_i = (StateMod_Well)wesList[i];
			if (wes_i == null)
			{
				continue;
			}
			id_i = wes_i.getID();
			rights = StateMod_Util.getRightsForStation(id_i, werList);
			size_rights = 0;
			if (rights != null)
			{
				size_rights = rights.Count;
			}
			if (size_rights == 0)
			{
				continue;
			}
			// Get the sum of the rights, assuming that all should be
			// compared against the capacity (i.e., sum of rights at the
			// end of the period will be compared with the current well capacity)...
			decree_sum = 0.0;
			for (int iright = 0; iright < size_rights; iright++)
			{
				wer_i = (StateMod_WellRight)rights[iright];
				decree = wer_i.getDcrdivw();
				onoff = wer_i.getSwitch();
				if (decree < 0.0)
				{
					// Ignore - missing values will cause a bad sum.
					continue;
				}
				if (onoff <= 0)
				{
					// Subtract the decree...
					decree_sum -= decree;
				}
				else
				{
					// Add the decree...
					decree_sum += decree;
				}
			}
			// Compare to a whole number, which is the greatest precision for documented files.
			string decree_sum_formatted = StringUtil.formatString(decree_sum,"%.2f");
			string capacity_formatted = StringUtil.formatString(wes_i.getDivcapw(),"%.2f");
			if (!decree_sum_formatted.Equals(capacity_formatted))
			{
				message = "Well station \"" + id_i + " capacity (" + capacity_formatted +
				") does not sum to rights (" + decree_sum_formatted + ")";
				Message.printWarning(warningLevel, MessageUtil.formatMessageTag(commandTag, ++warningCount), routine, message);
				/*
				status.addToLog ( CommandPhaseType.RUN,
					new WellRightValidation(CommandStatusType.FAILURE,
						message, "Check that the StateDMI command parameters used to process " +
						"well stations and rights are consistent." ) );
						*/
			}
		}
		return warningCount;
	}

	/// <summary>
	/// Print well information to output.  History header information 
	/// is also maintained by calling this routine. </summary>
	/// <param name="instrfile"> input file from which previous history should be taken </param>
	/// <param name="outstrfile"> output file to which to write </param>
	/// <param name="theWellStations"> list of wells to print </param>
	/// <param name="newComments"> addition comments which should be included in history </param>
	/// <seealso cref= RTi.Util.IOUtil#processFileHeaders </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeStateModFile(String instrfile, String outstrfile, java.util.List<StateMod_Well> theWellStations, java.util.List<String> newComments) throws Exception
	public static void writeStateModFile(string instrfile, string outstrfile, IList<StateMod_Well> theWellStations, IList<string> newComments)
	{
		IList<string> commentIndicators = new List<string>(1);
		commentIndicators.Add("#");
		IList<string> ignoredCommentIndicators = new List<string>(1);
		ignoredCommentIndicators.Add("#>");
		PrintWriter @out = null;
		try
		{
			@out = IOUtil.processFileHeaders(IOUtil.getPathUsingWorkingDir(instrfile), IOUtil.getPathUsingWorkingDir(outstrfile), newComments, commentIndicators, ignoredCommentIndicators, 0);

			int i, j;
			string iline = null;
			string cmnt = "#>";
			string format_1 = "%-12.12s%-24.24s%-12.12s%8d%#8.2F %-12.12s %#12.5F";
			string format_2 = "                                    %-12.12s%8d%8d%8d%#8.0F%#8.0F%8d%8d";
			string format_4 = "                                    %-12.12s%8.2F%8d";
			StateMod_Well well = null;
			StateMod_ReturnFlow ret = null;
			IList<object> v = new List<object>(8);
			IList<StateMod_ReturnFlow> wellDepletion = null;
			IList<StateMod_ReturnFlow> wellReturnFlow = null;

			@out.println(cmnt);
			@out.println(cmnt + " *******************************************************");
			@out.println(cmnt + "  StateMod Well Station File");
			@out.println(cmnt);
			@out.println(cmnt + "  Row 1 format:  (a12, a24, a12,i8, f8.2, 1x, a12, 1x, f12.5)");
			@out.println(cmnt);
			@out.println(cmnt + "  ID        cdividw:  Well ID");
			@out.println(cmnt + "  Name      divnamw:  Well name");
			@out.println(cmnt + "  Riv ID    idvstaw:  River Node where well is located");
			@out.println(cmnt + "  On/Off    idivsww:  Switch 0=off; 1=on");
			@out.println(cmnt + "  Capacity  divcapw:  Well capacity (cfs)");
			@out.println(cmnt + "  Daily ID cdividyw:  Well ID to use for daily data");
			@out.println(cmnt + "  Primary   primary:  See StateMod documentation");
			@out.println(cmnt);
			@out.println(cmnt + "  Row 2 format:  (36x, a12, 3i8, f8.2, f8.0, i8, f8.0)");
			@out.println(cmnt);
			@out.println(cmnt + "  DivID     idvcow2:  Diversion this well is tied to (N/A if not tied to a diversion)");
			@out.println(cmnt + "  DataType  idvcomw:  Data type (see StateMod doc)");
			@out.println(cmnt + "  #-Ret       nrtnw:  Number of return flow locations");
			@out.println(cmnt + "  #-Dep      nrtnw2:  Number of depletion locations");
			@out.println(cmnt + "  Eff %     divefcw:  System efficiency (%) - if negative, include row 3");
			@out.println(cmnt + "  Area        areaw:  Area served.");
			@out.println(cmnt + "  UseType   irturnw:  Use type; 1-3=Inbasin; 4=Transmountain");
			@out.println(cmnt + "  Demsrc    demsrcw:  Irrig acreage source (1=GIS, 2=tia, 3=GIS-primary, 4=tia-primary,");
			@out.println(cmnt + "                       5=secondary, 6=M&I no acreage, 7=carrier no acreage, 8=user),");
			@out.println(cmnt);
			@out.println(cmnt + "  Row 3   Variable efficiency data, % (enter if divefcw < 0) - free format");
			@out.println(cmnt);
			@out.println(cmnt + "  Row 4  format:  (36x, a12, f8.0, i8)");
			@out.println(cmnt);
			@out.println(cmnt + "  Ret Id   crtnidw:  River ID receiving return flow");
			@out.println(cmnt + "  Ret %    pcttotw:  Percent of return flow to location ");
			@out.println(cmnt + "  Table #  irtndlw:  Return flow table id");
			@out.println(cmnt);
			@out.println(cmnt + "  Row 5  format:  (36x, a12, f8.0, i8)");
			@out.println(cmnt + "  Dep Id   crtnidw2:  River ID being depletion");
			@out.println(cmnt + "  Dep %    pcttotw2:  Percent of depletion to river node ");
			@out.println(cmnt + "  Table #  irtndlw2:  Return (depletion) table id");
			@out.println(cmnt);
			@out.println(cmnt + "   ID         Name                  Riv ID     On/Off Capacity  Daily ID     Primary    ");
			@out.println(cmnt + "---------eb----------------------eb----------eb------eb------exb----------exb----------e");
			@out.println(cmnt + "                                       DivID  DataCode #-Ret   #-Dep   Eff %   Area   UseType  Demsrc");
			@out.println(cmnt + "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxb----------eb------eb------eb------eb------eb------eb------eb------e");
			@out.println(cmnt + "  Eff %    Diveff Efficiency for month 1-12 where 1 is tied to year type");
			@out.println(cmnt + "eff(1)  eff(2)  eff(3)  eff(4)  eff(5)  eff(6)  eff(7)  eff(8)  eff(9) eff(10) eff(11) eff(12)");
			@out.println(cmnt + "-----eb------eb------eb------eb------eb------eb------eb------eb------eb------eb------eb------e");
			@out.println(cmnt + "                                    Ret ID     Ret %   Table #");
			@out.println(cmnt + "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxb----------eb------eb------e");
			@out.println(cmnt + "                                    Dep ID     Dep %   Table #");
			@out.println(cmnt + "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxb----------eb------eb------e");
			@out.println(cmnt);
			@out.println(cmnt + "EndHeader");
			@out.println(cmnt);

			int num = 0;
			if (theWellStations != null)
			{
				num = theWellStations.Count;
			}

			for (i = 0; i < num; i++)
			{
				well = theWellStations[i];
				if (well == null)
				{
					continue;
				}

				// line 1
				v.Clear();
				v.Add(well.getID());
				v.Add(well.getName());
				v.Add(well.getCgoto());
				v.Add(new int?(well.getSwitch()));
				v.Add(new double?(well.getDivcapw()));
				v.Add(well.getCdividyw());
				v.Add(new double?(well.getPrimary()));
				iline = StringUtil.formatString(v, format_1);
				@out.println(iline);

				// line 2
				v.Clear();
				v.Add(well.getIdvcow2());
				v.Add(new int?(well.getIdvcomw()));
				v.Add(new int?(well.getNrtnw()));
				v.Add(new int?(well.getNrtnw2()));
				v.Add(new double?(well.getDivefcw()));
				v.Add(new double?(well.getAreaw()));
				v.Add(new int?(well.getIrturnw()));
				v.Add(new int?(well.getDemsrcw()));
				iline = StringUtil.formatString(v, format_2);
				@out.println(iline);

				// line 3 - well efficiency
				if (well.getDivefcw() < 0)
				{
					for (j = 0; j < 12; j++)
					{
						v.Clear();
						v.Add(new double?(well.getDiveff(j)));
						iline = StringUtil.formatString(v, " %#5.0F");
						@out.print(iline);
					}

					@out.println();
				}

				// line 4 - return information
				int nrtn = well.getNrtnw();
				wellReturnFlow = well.getReturnFlows();
				for (j = 0; j < nrtn; j++)
				{
					v.Clear();
					ret = wellReturnFlow[j];
					v.Add(ret.getCrtnid());
					v.Add(new double?(ret.getPcttot()));
					v.Add(new int?(ret.getIrtndl()));
					// SAM changed on 2000-02-24 as per Ray Bennett...
					//iline = StringUtil.formatString(v, format_4);
					iline = StringUtil.formatString(v, format_4) + " Rtn" + StringUtil.formatString((j + 1),"%02d");
					@out.println(iline);
				}

				// line 5 - depletion information
				nrtn = well.getNrtnw2();
				wellDepletion = well.getDepletions();
				for (j = 0; j < nrtn; j++)
				{
					v.Clear();
					ret = wellDepletion[j];
					v.Add(ret.getCrtnid());
					v.Add(new double?(ret.getPcttot()));
					v.Add(new int?(ret.getIrtndl()));
					// SAM changed on 2000-02-24 as per Ray Bennett...
					//iline = StringUtil.formatString(v, format_4);
					iline = StringUtil.formatString(v, format_4) + " Dep" + StringUtil.formatString((j + 1),"%02d");
					@out.println(iline);
				}
			}

			@out.flush();
			@out.close();
			@out = null;
		}
		catch (Exception e)
		{
			@out = null;
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
	/// Writes a list of StateMod_Well objects to a list file.  A header is 
	/// printed to the top of the file, containing the commands used to generate the 
	/// file.  Any strings in the body of the file that contain the field delimiter will be wrapped in "...".  
	/// This method also writes well Return Flows to
	/// filename[without extension]_ReturnFlows[extension], so if this method is called
	/// with a filename parameter of "wells.txt", four files will be generated:
	/// - wells.txt
	/// - wells_Collections.txt
	/// - wells_DelayTableAssignments.txt
	/// - wells_ReturnFlows.txt </summary>
	/// <param name="filename"> the name of the file to which the data will be written. </param>
	/// <param name="delimiter"> the delimiter to use for separating field values. </param>
	/// <param name="update"> whether to update an existing file, retaining the current 
	/// header (true) or to create a new file with a new header. </param>
	/// <param name="data"> the list of objects to write. </param>
	/// <param name="newComments"> comments to add at the top of the file (e.g., command file, HydroBase version). </param>
	/// <returns> a list of files that were actually written, because this method controls all the secondary
	/// filenames. </returns>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.List<java.io.File> writeListFile(String filename, String delimiter, boolean update, java.util.List<StateMod_Well> data, java.util.List<String> newComments) throws Exception
	public static IList<File> writeListFile(string filename, string delimiter, bool update, IList<StateMod_Well> data, IList<string> newComments)
	{
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
		fields.Add("DailyID");
		fields.Add("Primary");
		fields.Add("DiversionID");
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
		int comp = StateMod_DataSet.COMP_WELL_STATIONS;
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
		StateMod_ReturnFlow rf = null;
		StateMod_Well well = null;
		IList<string> commentIndicators = new List<string>(1);
		commentIndicators.Add("#");
		IList<string> ignoredCommentIndicators = new List<string>(1);
		ignoredCommentIndicators.Add("#>");
		string[] line = new string[fieldCount];
		string id = null;
		StringBuilder buffer = new StringBuilder();
		IList<StateMod_ReturnFlow> depletions = new List<StateMod_ReturnFlow>();
		IList<StateMod_ReturnFlow> returnFlows = new List<StateMod_ReturnFlow>();
		IList<StateMod_ReturnFlow> temprf = null;
		IList<StateMod_ReturnFlow> tempdep = null;

		string filenameFull = IOUtil.getPathUsingWorkingDir(filename);
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
			newComments2.Insert(1,"StateMod well stations as a delimited list file.");
			newComments2.Insert(2,"See also the associated return flow, depletion, and collection files.");
			newComments2.Insert(3,"");
			@out = IOUtil.processFileHeaders(oldFile, IOUtil.getPathUsingWorkingDir(filenameFull), newComments2, commentIndicators, ignoredCommentIndicators, 0);

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
				well = (StateMod_Well)data[i];

				line[0] = StringUtil.formatString(well.getID(), formats[0]).Trim();
				line[1] = StringUtil.formatString(well.getName(), formats[1]).Trim();
				line[2] = StringUtil.formatString(well.getCgoto(), formats[2]).Trim();
				line[3] = StringUtil.formatString(well.getSwitch(), formats[3]).Trim();
				line[4] = StringUtil.formatString(well.getDivcapw(), formats[4]).Trim();
				line[5] = StringUtil.formatString(well.getCdividyw(), formats[5]).Trim();
				line[6] = StringUtil.formatString(well.getPrimary(), formats[6]).Trim();
				line[7] = StringUtil.formatString(well.getIdvcow2(), formats[7]).Trim();
				line[8] = StringUtil.formatString(well.getIdvcomw(), formats[8]).Trim();
				line[9] = StringUtil.formatString(well.getDivefcw(), formats[9]).Trim();
				line[10] = StringUtil.formatString(well.getAreaw(), formats[10]).Trim();
				line[11] = StringUtil.formatString(well.getIrturnw(), formats[11]).Trim();
				line[12] = StringUtil.formatString(well.getDemsrcw(), formats[12]).Trim();
				line[13] = StringUtil.formatString(well.getDiveff(0), formats[13]).Trim();
				line[14] = StringUtil.formatString(well.getDiveff(1), formats[14]).Trim();
				line[15] = StringUtil.formatString(well.getDiveff(2), formats[15]).Trim();
				line[16] = StringUtil.formatString(well.getDiveff(3), formats[16]).Trim();
				line[17] = StringUtil.formatString(well.getDiveff(4), formats[17]).Trim();
				line[18] = StringUtil.formatString(well.getDiveff(5), formats[18]).Trim();
				line[19] = StringUtil.formatString(well.getDiveff(6), formats[19]).Trim();
				line[20] = StringUtil.formatString(well.getDiveff(7), formats[20]).Trim();
				line[21] = StringUtil.formatString(well.getDiveff(8), formats[21]).Trim();
				line[22] = StringUtil.formatString(well.getDiveff(9), formats[22]).Trim();
				line[23] = StringUtil.formatString(well.getDiveff(10), formats[23]).Trim();
				line[24] = StringUtil.formatString(well.getDiveff(11), formats[24]).Trim();

				buffer = new StringBuilder();
				for (j = 0; j < fieldCount; j++)
				{
					if (line[j].IndexOf(delimiter, StringComparison.Ordinal) > -1)
					{
						line[j] = "\"" + line[j] + "\"";
					}
					buffer.Append(line[j]);
					if (j < (fieldCount - 1))
					{
						buffer.Append(delimiter);
					}
				}

				@out.println(buffer.ToString());

				temprf = well.getReturnFlows();
				size2 = temprf.Count;
				id = well.getID();
				for (j = 0; j < size2; j++)
				{
					rf = temprf[j];
					rf.setID(id);
					returnFlows.Add(rf);
				}

				tempdep = well.getDepletions();
				size2 = tempdep.Count;
				for (j = 0; j < size2; j++)
				{
					rf = tempdep[j];
					rf.setID(id);
					depletions.Add(rf);
				}
			}
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

		int lastIndex = filename.LastIndexOf(".", StringComparison.Ordinal);
		string front = filename.Substring(0, lastIndex);
		string end = filename.Substring((lastIndex + 1), filename.Length - (lastIndex + 1));

		string returnFlowFilename = front + "_ReturnFlows." + end;
		StateMod_ReturnFlow.writeListFile(returnFlowFilename, delimiter, update, returnFlows, StateMod_DataSet.COMP_WELL_STATION_DELAY_TABLES, newComments);

		string depletionFilename = front + "_Depletions." + end;
		StateMod_ReturnFlow.writeListFile(depletionFilename, delimiter, update, depletions, StateMod_DataSet.COMP_WELL_STATION_DEPLETION_TABLES, newComments);

		string collectionFilename = front + "_Collections." + end;
		writeCollectionListFile(collectionFilename, delimiter, update, data, newComments);

		IList<File> filesWritten = new List<File>();
		filesWritten.Add(new File(filenameFull));
		filesWritten.Add(new File(returnFlowFilename));
		filesWritten.Add(new File(depletionFilename));
		filesWritten.Add(new File(collectionFilename));
		return filesWritten;
	}

	/// <summary>
	/// Writes the collection data from a list of StateMod_Well objects to a 
	/// list file.  A header is printed to the top of the file, containing the commands 
	/// used to generate the file.  Any strings in the body of the file that contain 
	/// the field delimiter will be wrapped in "...". </summary>
	/// <param name="filename"> the name of the file to which the data will be written. </param>
	/// <param name="delimiter"> the delimiter to use for separating field values. </param>
	/// <param name="update"> whether to update an existing file, retaining the current 
	/// header (true) or to create a new file with a new header. </param>
	/// <param name="data"> the Vector of objects to write. </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeCollectionListFile(String filename, String delimiter, boolean update, java.util.List<StateMod_Well> data, java.util.List<String> newComments) throws Exception
	public static void writeCollectionListFile(string filename, string delimiter, bool update, IList<StateMod_Well> data, IList<string> newComments)
	{
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
		fields.Add("PartIDType");
		int fieldCount = fields.Count;

		string[] names = new string[fieldCount];
		string[] formats = new string[fieldCount];
		int comp = StateMod_DataSet.COMP_WELL_STATION_COLLECTIONS;
		string s = null;
		for (int i = 0; i < fieldCount; i++)
		{
			s = (string)fields[i];
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
		int numYears = 0;
		PrintWriter @out = null;
		StateMod_Well well = null;
		IList<string> commentIndicators = new List<string>(1);
		commentIndicators.Add("#");
		IList<string> ignoredCommentIndicators = new List<string>(1);
		ignoredCommentIndicators.Add("#>");
		string[] field = new string[fieldCount];
		string colType = null;
		string id = null;
		string partType = null;
		StringBuilder buffer = new StringBuilder();
		IList<string> ids = null;
		IList<string> idTypes = null;

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
			newComments2.Insert(1,"StateMod well station collection information as delimited list file.");
			newComments2.Insert(2,"See also the associated well station file.");
			newComments2.Insert(3,"Division and year are only used with well parcel aggregates/systems.");
			newComments2.Insert(4,"ParcelIdType are only used with well aggregates/systems where the part ID is Well.");
			newComments2.Insert(5,"");
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
				well = (StateMod_Well)data[i];
				id = well.getID();
				years = well.getCollectionYears();
				if (years == null)
				{
					numYears = 0; // By this point, collections that span the full period will use 1 year = 0
				}
				else
				{
					numYears = years.Length;
				}
				colType = well.getCollectionType();
				partType = well.getCollectionPartType();
				idTypes = well.getCollectionPartIDTypes(); // Currently crosses all years
				int numIdTypes = 0;
				if (idTypes != null)
				{
					numIdTypes = idTypes.Count;
				}
				// Loop through the number of years of collection data
				for (int iyear = 0; iyear < numYears; iyear++)
				{
					ids = well.getCollectionPartIDs(years[iyear]);
					// Loop through the identifiers for the specific year
					for (k = 0; k < ids.Count; k++)
					{
						field[0] = StringUtil.formatString(id, formats[0]).Trim();
						field[1] = StringUtil.formatString(years[iyear], formats[1]).Trim();
						field[2] = StringUtil.formatString(colType, formats[2]).Trim();
						field[3] = StringUtil.formatString(partType, formats[3]).Trim();
						field[4] = StringUtil.formatString(((string)(ids[k])), formats[4]).Trim();
						field[5] = "";
						if (numIdTypes > k)
						{
							// Have data to output
							field[5] = StringUtil.formatString(((string)(idTypes[k])),formats[5]).Trim();
						}

						buffer = new StringBuilder();
						for (int ifield = 0; ifield < fieldCount; ifield++)
						{
							if (ifield > 0)
							{
								buffer.Append(delimiter);
							}
							if (field[ifield].IndexOf(delimiter, StringComparison.Ordinal) > -1)
							{
								// Wrap delimiter in quoted field
								field[ifield] = "\"" + field[ifield] + "\"";
							}
							buffer.Append(field[ifield]);
						}

						@out.println(buffer.ToString());
					}
				}
			}
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