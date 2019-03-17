using System;
using System.Collections.Generic;
using System.IO;

// StateCU_IrrigationPracticeTS - StateCU irrigation practice time series

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

// ----------------------------------------------------------------------------
// StateCU_IrrigationPracticeTS - StateCU irrigation practice time series
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
//
// 2003-02-23	Steven A. Malers, RTi	Copy CUCropPatternTS and update since
//					data objects are similar.
// 2003-06-04	SAM, RTi		Rename class from CUParameterTS to
//					StateCU_ParameterTS.  Use new TS
//					package.
// 2003-07-08	SAM, RTi		Rename class from StateCU_ParameterTS to
//					StateCU_IrrigationPracticeTS and update
//					to current file format description as
//					per Leonard Rice.
// 2003-10-14	SAM, RTi		Handle water year format - basically
//					the data are stored in the year type
//					and other code will need to handle.  In
//					most cases it is irrelevant.
// 2004-02-10	SAM, RTi		* Add readTimeSeries() method to
//					  facilitate use by TSTool.
//					* Change time series names to better
//					  integrate into TSTool.
// 2004-03-03	SAM, RTi		* Initialize time series to appropriate
//					  values.
//					* Add total acreage time series in
//					  writeVector().
// 2004-03-22	SAM, RTi		* Add getTimeSeriesDataTypes() to
//					  generically return the data type
//					  strings.
//					* Add getTimeSeries ( String ) to
//					  simplify generic use by other code.
// 2004-06-02	SAM, RTi		* Add a header to the output, similar
//					  to the crop pattern time series file.
//					* Remove some code to support the old
//					  TSP file since it is not being used.
// 2004-09-20	SAM, RTi		* Change file format to legacy format as
//					  per Ray Bennett 2004-08-25 email:
//					  i4,1x,a12,3f6.0,2f8.0,f12.0,f3.0,f8.0
// 2005-01-19	SAM, RTi		* Add toTSVector() to facilitate
//				  	  extracting the time series data.
// 2005-03-21	SAM, RTi		* Add addToGacre().
// 2005-04-01	SAM, RTi		* Overload toTSVector() to include the
//					  data set total.
// 2005-06-09	SAM, RTi		* Add isIrrigationPracticeTSFile().
//					* Overload readStateCUFile() to take a
//					  PropList and handle the Version
//					  property.
// 2005-11-17	SAM, RTi		* On output, write the beginning and
//					  ending month, units, and year type.
// 2007-01-11	Kurt Tometich, RTi	Adding support for 4 new land acreage
//							fields in the new version as well as support the
//							old format Version 10.
// 2007-02-18	SAM, RTi		Review KAT changes.
//					Clean code based on Eclipse feedback.
//					Overload getTimeSeriesDataTypes() to default to current
//					format but support old format.
// 2007-04-17	SAM, RTi		Confirm ability to read very old format without
//					the header to compare with newer formats.
// ----------------------------------------------------------------------------
// EndHeader

namespace DWR.StateCU
{

	using DateValueTS = RTi.TS.DateValueTS;
	using TS = RTi.TS.TS;
	using TSIdent = RTi.TS.TSIdent;
	using TSUtil = RTi.TS.TSUtil;
	using YearTS = RTi.TS.YearTS;
	using DataSetComponent = RTi.Util.IO.DataSetComponent;
	using IOUtil = RTi.Util.IO.IOUtil;
	using PropList = RTi.Util.IO.PropList;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;
	using DateTime = RTi.Util.Time.DateTime;

	/// <summary>
	/// The StateCU_IrrigationPracticeTS class is used to hold irrigation practice
	/// time series associated with structures.  Each instance has an identifier, which will
	/// match a CULocation identifier, and a list of time series for various parameters
	/// that are associated with the CU Location for a period of time.  If an average
	/// annual analysis is done, the period may consist of one zero year.
	/// </summary>
	public class StateCU_IrrigationPracticeTS : StateCU_Data, StateCU_ComponentValidator
	{

	/// <summary>
	/// Time series data types for use in choices, TSID strings.
	/// </summary>
	public const string TSTYPE_Eff_SurfaceMax = "Eff-SurfaceMax";
	public const string TSTYPE_Eff_FloodMax = "Eff-FloodMax";
	public const string TSTYPE_Eff_SprinklerMax = "Eff-SprinklerMax";

	public const string TSTYPE_CropArea_GroundWaterVersion10 = "CropArea-GroundWaterVersion10";
	public const string TSTYPE_CropArea_SprinklerVersion10 = "CropArea-SprinklerVersion10";

	public const string TSTYPE_CropArea_Total = "CropArea-Total";
	public const string TSTYPE_CropArea_GroundWater = "CropArea-GroundWater";
	public const string TSTYPE_CropArea_SurfaceWaterOnly = "CropArea-SurfaceWaterOnly";
	public const string TSTYPE_CropArea_SurfaceWaterOnlyFlood = "CropArea-SurfaceWaterOnlyFlood";
	public const string TSTYPE_CropArea_SurfaceWaterOnlySprinkler = "CropArea-SurfaceWaterOnlySprinkler";
	public const string TSTYPE_CropArea_GroundWaterFlood = "CropArea-GroundWaterFlood";
	public const string TSTYPE_CropArea_GroundWaterSprinkler = "CropArea-GroundWaterSprinkler";

	public const string TSTYPE_PumpingMax = "PumpingMax";
	public const string TSTYPE_GWUseMode = "GWUseMode";

	private YearTS __ceff_ts = null;
	private YearTS __feff_ts = null;
	private YearTS __seff_ts = null;
	//private YearTS __gacre_ts = null;	// Old Version 10 time series
	//private YearTS __sacre_ts = null;	// Old Version 10 time series
	private YearTS __mprate_ts = null; // Also referred to as Gwvol
	private YearTS __gmode_ts = null; // Also referred to as Gwm

	/// <summary>
	/// Total acres time series.
	/// </summary>
	private YearTS __tacre_ts = null;

	/// <summary>
	/// Acreage by supply type only (surface water only or ground water supplemented).
	/// Surface supply only acreage.
	/// </summary>
	private YearTS __acsw_ts = null;

	/// <summary>
	/// Groundwater supplemented supply acreage.
	/// </summary>
	private YearTS __acgw_ts = null;

	/// <summary>
	/// Acreage by supply type (surface water only or ground water supplemented)
	/// and irrigation method (sprinkler=high efficiency or flood=low efficiency).
	/// Surface water flood.
	/// </summary>
	private YearTS __acswfl_ts = null;

	/// <summary>
	/// Surface water only sprinkler.
	/// </summary>
	private YearTS __acswspr_ts = null;

	/// <summary>
	/// Ground water supplemented flood.
	/// </summary>
	private YearTS __acgwfl_ts = null;

	/// <summary>
	/// Ground water supplemented sprinkler.
	/// </summary>
	private YearTS __acgwspr_ts = null;

						// Integers are stored as double data
						// so make sure to convert to add .1
						// and cast to int when using data.

	/// <summary>
	/// Dates for the period of record (for all time series) - these will agree with
	/// the year type and therefore may not match the header year integer.
	/// </summary>
	private DateTime __date1 = null;
	private DateTime __date2 = null;

	// Year type CYR, WYR, IYR - currently always use CYR since yearly data.
	private string __year_type = "CYR";

	/// <summary>
	/// Internal data that is used to set/get data so new DateTime objects don't have to
	/// be created each time.  Only the year is manipulated.
	/// </summary>
	private DateTime __temp_DateTime = new DateTime();

	/// <summary>
	/// The file that is read, used to set the time series input name.
	/// </summary>
	private string __filename = "";

	/// <summary>
	/// The list of StateCU_Parcel observations, as an archive of observations to use with data filling.
	/// </summary>
	private IList<StateCU_Parcel> __parcel_List = new List<StateCU_Parcel>();

	/// <summary>
	/// Construct a new StateCU_IrrigationPracticeTS object for the specified CU
	/// Location identifier.  All time series for the current version (12) are created. </summary>
	/// <param name="id"> CU Location identifier. </param>
	/// <param name="date1"> Starting date of period, to year precision.
	/// Specify with year 0 if an average annual data set. </param>
	/// <param name="date2"> Ending date of period, to year precision.
	/// Specify with year 0 if an average annual data set. </param>
	/// <param name="year_type"> The years in the dates should correspond to the year type,
	/// which can be "CYR" (calendar), "WYR" (water), or "IYR" (irrigation) -
	/// currently "CYR" is assumed regardless. </param>
	/// <param name="filename"> The name of the file that is being read, or null if created in memory. </param>
	public StateCU_IrrigationPracticeTS(string id, DateTime date1, DateTime date2, string year_type, string filename) : this(id, date1, date2, year_type, filename, 12)
	{
	}

	/// <summary>
	/// Construct a new StateCU_IrrigationPracticeTS object for the specified CU Location identifier. </summary>
	/// <param name="id"> CU Location identifier. </param>
	/// <param name="date1"> Starting date of period, to year precision.
	/// Specify with year 0 if an average annual data set. </param>
	/// <param name="date2"> Ending date of period, to year precision.
	/// Specify with year 0 if an average annual data set. </param>
	/// <param name="year_type"> The years in the dates should correspond to the year type,
	/// which can be "CYR" (calendar), "WYR" (water), or "IYR" (irrigation) -
	/// currently "CYR" is assumed regardless. </param>
	/// <param name="filename"> The name of the file that is being read, or null if created in memory. </param>
	/// <param name="version"> StateCU version for file.  If < 12, groundwater and sprinkler
	/// acreage time series are created.  If 12+, separate time series for flood,
	/// sprinkler, surface, groundwater combinations are created. </param>
	public StateCU_IrrigationPracticeTS(string id, DateTime date1, DateTime date2, string year_type, string filename, int version) : base()
	{
		string routine = "IrrigationPracticeTS";
		_id = id;
		__filename = filename;
		if ((date1 == null) || (date2 == null))
		{
			// Assume an average condition and use year 0...
			__date1 = new DateTime();
			__date2 = new DateTime();
		}
		else
		{
			// Use the specified dates...
			__date1 = new DateTime(date1);
			__date2 = new DateTime(date2);
		}
		__year_type = year_type;
		__ceff_ts = new YearTS();
		TSIdent tsident = null;
		try
		{
			tsident = new TSIdent(_id, "StateCU", TSTYPE_Eff_SurfaceMax,"Year","");
			__ceff_ts.setIdentifier(tsident);
		}
		catch (Exception)
		{
			// Should not happen.
			Message.printWarning(3, routine, "Unable to create Eff-SurfaceMax time series.");
		}
		__ceff_ts.setDataUnits("FRACTION");
		__ceff_ts.setDescription(_id + " maximum efficiency for delivering surface water supply.");
		__ceff_ts.getIdentifier().setInputType("StateCU");
		if (!string.ReferenceEquals(__filename, null))
		{
			__ceff_ts.getIdentifier().setInputName(__filename);
		}
		__ceff_ts.setDate1(new DateTime(__date1));
		__ceff_ts.setDate2(new DateTime(__date2));
		// Initialize to reasonable default...
		__ceff_ts.allocateDataSpace(.8);
		__feff_ts = new YearTS();
		try
		{
			tsident = new TSIdent(_id, "StateCU", TSTYPE_Eff_FloodMax,"Year", "");
			__feff_ts.setIdentifier(tsident);
		}
		catch (Exception)
		{
			// Should not happen.
			Message.printWarning(3, routine, "Unable to create Eff-FloodMax time series.");
		}
		__feff_ts.setDataUnits("FRACTION");
		__feff_ts.setDescription(_id + " maximum application efficiency for flood irrigation.");
		__feff_ts.getIdentifier().setInputType("StateCU");
		if (!string.ReferenceEquals(__filename, null))
		{
			__feff_ts.getIdentifier().setInputName(__filename);
		}
		__feff_ts.setDate1(new DateTime(__date1));
		__feff_ts.setDate2(new DateTime(__date2));
		// Initialize to reasonable default...
		__feff_ts.allocateDataSpace(.7);
		__seff_ts = new YearTS();
		try
		{
			tsident = new TSIdent(_id, "StateCU", TSTYPE_Eff_SprinklerMax, "Year", "");
			__seff_ts.setIdentifier(tsident);
		}
		catch (Exception)
		{
			// Should not happen.
			Message.printWarning(3, routine, "Unable to create Eff-SprinklerMax time series.");
		}
		__seff_ts.setDataUnits("FRACTION");
		__seff_ts.setDescription(_id + " maximum application efficiency for sprinkler irrigation.");
		__seff_ts.getIdentifier().setInputType("StateCU");
		if (!string.ReferenceEquals(__filename, null))
		{
			__seff_ts.getIdentifier().setInputName(__filename);
		}
		__seff_ts.setDate1(new DateTime(__date1));
		__seff_ts.setDate2(new DateTime(__date2));
		// Initialize to reasonable default...
		__seff_ts.allocateDataSpace(.8);

		// Initialize new time series for V12 and newer

		// Acres surface water only
		if (version >= 12)
		{
			__acsw_ts = new YearTS();
			try
			{
				tsident = new TSIdent(_id, "StateCU", TSTYPE_CropArea_SurfaceWaterOnly, "Year", "");
				__acsw_ts.setIdentifier(tsident);
			}
			catch (Exception)
			{
				// Should not happen.
				Message.printWarning(3, routine, "Unable to create acre surface water time series.");
			}
			__acsw_ts.setDataUnits("ACRE");
			__acsw_ts.setDescription(_id + " acres surface water.");
			__acsw_ts.getIdentifier().setInputType("StateCU");
			if (!string.ReferenceEquals(__filename, null))
			{
				__acsw_ts.getIdentifier().setInputName(__filename);
			}
			__acsw_ts.setDate1(new DateTime(__date1));
			__acsw_ts.setDate2(new DateTime(__date2));
			// Initialize to missing - may fill later
			__acsw_ts.allocateDataSpace();

			// Acres groundwater supply

			__acgw_ts = new YearTS();
			try
			{
				tsident = new TSIdent(_id, "StateCU", TSTYPE_CropArea_GroundWater, "Year", "");
				__acgw_ts.setIdentifier(tsident);
			}
			catch (Exception)
			{
				// Should not happen.
				Message.printWarning(3, routine, "Unable to create acre ground water time series.");
			}
			__acgw_ts.setDataUnits("ACRE");
			__acgw_ts.setDescription(_id + " acres ground water.");
			__acgw_ts.getIdentifier().setInputType("StateCU");
			if (!string.ReferenceEquals(__filename, null))
			{
				__acgw_ts.getIdentifier().setInputName(__filename);
			}
			__acgw_ts.setDate1(new DateTime(__date1));
			__acgw_ts.setDate2(new DateTime(__date2));
			// Initialize to missing - may fill later
			__acgw_ts.allocateDataSpace();

			//Acres surface water flood

			__acswfl_ts = new YearTS();
			try
			{
				tsident = new TSIdent(_id, "StateCU", TSTYPE_CropArea_SurfaceWaterOnlyFlood, "Year", "");
				__acswfl_ts.setIdentifier(tsident);
			}
			catch (Exception)
			{
				// Should not happen.
				Message.printWarning(3, routine, "Unable to create acre surface water flood time series.");
			}
			__acswfl_ts.setDataUnits("ACRE");
			__acswfl_ts.setDescription(_id + " acres surface water flood.");
			__acswfl_ts.getIdentifier().setInputType("StateCU");
			if (!string.ReferenceEquals(__filename, null))
			{
				__acswfl_ts.getIdentifier().setInputName(__filename);
			}
			__acswfl_ts.setDate1(new DateTime(__date1));
			__acswfl_ts.setDate2(new DateTime(__date2));
			// Initialize to missing - may fill later
			__acswfl_ts.allocateDataSpace();

			// Acres surface water sprinkler
			__acswspr_ts = new YearTS();
			try
			{
				tsident = new TSIdent(_id, "StateCU", TSTYPE_CropArea_SurfaceWaterOnlySprinkler, "Year", "");
				__acswspr_ts.setIdentifier(tsident);
			}
			catch (Exception)
			{
				// Should not happen.
				Message.printWarning(3, routine, "Unable to create acre surface water sprinkler time series.");
			}
			__acswspr_ts.setDataUnits("ACRE");
			__acswspr_ts.setDescription(_id + " acres surface water sprinkler.");
			__acswspr_ts.getIdentifier().setInputType("StateCU");
			if (!string.ReferenceEquals(__filename, null))
			{
				__acswspr_ts.getIdentifier().setInputName(__filename);
			}
			__acswspr_ts.setDate1(new DateTime(__date1));
			__acswspr_ts.setDate2(new DateTime(__date2));
			// Initialize to missing - may fill later
			__acswspr_ts.allocateDataSpace();

			// Acres ground water flood
			__acgwfl_ts = new YearTS();
			try
			{
				tsident = new TSIdent(_id, "StateCU", TSTYPE_CropArea_GroundWaterFlood, "Year", "");
				__acgwfl_ts.setIdentifier(tsident);
			}
			catch (Exception)
			{
				// Should not happen.
				Message.printWarning(3, routine, "Unable to create acre ground water flood time series.");
			}
			__acgwfl_ts.setDataUnits("ACRE");
			__acgwfl_ts.setDescription(_id + " acres ground water flood.");
			__acgwfl_ts.getIdentifier().setInputType("StateCU");
			if (!string.ReferenceEquals(__filename, null))
			{
				__acgwfl_ts.getIdentifier().setInputName(__filename);
			}
			__acgwfl_ts.setDate1(new DateTime(__date1));
			__acgwfl_ts.setDate2(new DateTime(__date2));
			// Initialize to missing - may fill later
			__acgwfl_ts.allocateDataSpace();

			// Acres ground water sprinkler
			__acgwspr_ts = new YearTS();
			try
			{
				tsident = new TSIdent(_id, "StateCU", TSTYPE_CropArea_GroundWaterSprinkler, "Year", "");
				__acgwspr_ts.setIdentifier(tsident);
			}
			catch (Exception)
			{
				// Should not happen.
				Message.printWarning(3, routine, "Unable to create acre ground water sprinkler time series.");
			}
			__acgwspr_ts.setDataUnits("ACRE");
			__acgwspr_ts.setDescription(_id + " acres ground water sprinkler.");
			__acgwspr_ts.getIdentifier().setInputType("StateCU");
			if (!string.ReferenceEquals(__filename, null))
			{
				__acgwspr_ts.getIdentifier().setInputName(__filename);
			}
			__acgwspr_ts.setDate1(new DateTime(__date1));
			__acgwspr_ts.setDate2(new DateTime(__date2));
			// Initialize to missing - may fill later
			__acgwspr_ts.allocateDataSpace();
		}

		// Version 10 time series - keep them around also until no longer needed.
		/* FIXME SAM 2007-10-18 Comment out because they are confusing other code
		__gacre_ts = new YearTS ();
		try {	tsident = new TSIdent ( _id, "StateCU",
			TSTYPE_CropArea_GroundWaterVersion10, "Year", "");
			__gacre_ts.setIdentifier ( tsident );
		}
		catch ( Exception e ) {
			// Should not happen.
			Message.printWarning ( 2, routine,
			"Unable to create CropArea-Groundwater time series." );
		}
		__gacre_ts.setDataUnits ( "ACRE" );
		__gacre_ts.setDescription (_id+
			" acres with groundwater supply.");
		__gacre_ts.getIdentifier().setInputType ( "StateCU" );
		if ( __filename != null ) {
			__gacre_ts.getIdentifier().setInputName ( __filename );
		}
		__gacre_ts.setDate1(new DateTime(__date1));
		__gacre_ts.setDate2(new DateTime(__date2));
		// Initialize to missing - may fill later
		__gacre_ts.allocateDataSpace();
		__sacre_ts = new YearTS ();
		try {	tsident = new TSIdent ( _id, "StateCU",
				TSTYPE_CropArea_SprinklerVersion10, "Year",
				"");
			__sacre_ts.setIdentifier ( tsident );
		}
		catch ( Exception e ) {
			// Should not happen.
			Message.printWarning ( 2, routine,
			"Unable to create CropArea-Sprinkler time series." );
		}
		__sacre_ts.setDataUnits ( "ACRE" );
		__sacre_ts.setDescription (_id+" acres with sprinkler supply.");
		__sacre_ts.getIdentifier().setInputType ( "StateCU" );
		if ( __filename != null ) {
			__sacre_ts.getIdentifier().setInputName ( __filename );
		}
		__sacre_ts.setDate1(new DateTime(__date1));
		__sacre_ts.setDate2(new DateTime(__date2));
		// Initialize to missing - may fill later
		__sacre_ts.allocateDataSpace();
		*/

		__mprate_ts = new YearTS();
		try
		{
			tsident = new TSIdent(_id, "StateCU", TSTYPE_PumpingMax, "Year", "");
			__mprate_ts.setIdentifier(tsident);
		}
		catch (Exception)
		{
			// Should not happen.
			Message.printWarning(3, routine, "Unable to create PumpingMax time series.");
		}
		__mprate_ts.setDataUnits("ACFT");
		__mprate_ts.setDescription(_id + " Maximum monthly pumping.");
		__mprate_ts.getIdentifier().setInputType("StateCU");
		if (!string.ReferenceEquals(__filename, null))
		{
			__mprate_ts.getIdentifier().setInputName(__filename);
		}
		__mprate_ts.setDate1(new DateTime(__date1));
		__mprate_ts.setDate2(new DateTime(__date2));
		// Initialize to missing - may fill later
		__mprate_ts.allocateDataSpace();
		__gmode_ts = new YearTS();
		try
		{
			tsident = new TSIdent(_id, "StateCU", TSTYPE_GWUseMode, "Year", "");
			__gmode_ts.setIdentifier(tsident);
		}
		catch (Exception)
		{
			// Should not happen.
			Message.printWarning(3, routine, "Unable to create GWUseMode time series.");
		}
		__gmode_ts.setDataUnits("");
		__gmode_ts.setDescription(_id + " Groundwater use mode.");
		__gmode_ts.getIdentifier().setInputType("StateCU");
		if (!string.ReferenceEquals(__filename, null))
		{
			__gmode_ts.getIdentifier().setInputName(__filename);
		}
		__gmode_ts.setDate1(new DateTime(__date1));
		__gmode_ts.setDate2(new DateTime(__date2));
		// Initialize to reasonable default...
		__gmode_ts.allocateDataSpace(2.0);
		__tacre_ts = new YearTS();
		try
		{
			tsident = new TSIdent(_id, "StateCU", TSTYPE_CropArea_Total, "Year", "");
			__tacre_ts.setIdentifier(tsident);
		}
		catch (Exception)
		{
			// Should not happen.
			Message.printWarning(3, routine, "Unable to create CropArea-AllIrrigation time series.");
		}
		__tacre_ts.setDataUnits("ACRE");
		__tacre_ts.setDescription(_id + " Total acres");
		__tacre_ts.getIdentifier().setInputType("StateCU");
		if (!string.ReferenceEquals(__filename, null))
		{
			__tacre_ts.getIdentifier().setInputName(__filename);
		}
		__tacre_ts.setDate1(new DateTime(__date1));
		__tacre_ts.setDate2(new DateTime(__date2));
		// Initialize to missing - may fill later
		__tacre_ts.allocateDataSpace();
	}

	/// <summary>
	/// Add a parcel containing observations. </summary>
	/// <param name="parcel"> StateCU_Parcel to add. </param>
	public virtual void addParcel(StateCU_Parcel parcel)
	{
		__parcel_List.Add(parcel);
	}

	/// <summary>
	/// Add to Gacre.  If the initial value is missing, it will be set to the specified value. </summary>
	/// <param name="year"> Year for data. </param>
	/// <param name="gacre"> Gacre value to add. </param>
	/* FIXME SAM 2007-10-18 Remove later when tested out
	public void addToGacre ( int year, double gacre )
	{	__temp_DateTime.setYear ( year );
		if (	__gacre_ts.isDataMissing(
			__gacre_ts.getDataValue(__temp_DateTime)) ) {
			// Set the value...
			__gacre_ts.setDataValue ( __temp_DateTime, gacre );
		}
		else {	// Add to the value...
			__gacre_ts.setDataValue ( __temp_DateTime,
			(__gacre_ts.getDataValue ( __temp_DateTime ) + gacre) );
		}
	}
	*/

	/// <summary>
	/// Adjust the ground water acreage to the total acres if necessary.  The total acres must be set previously.
	/// This is typically done when processing the IPY file and setting the groundwater acreage
	/// first, causing a cascade to set the other values.
	/// It is required that both groundwater acreage parts (sprinkler and flood) are set.
	/// Otherwise no adjustments are made.
	/// Surface water acres are adjusted after the groundwater acres are adjusted. </summary>
	/// <param name="Date"> Date (year) being processed. </param>
	/// <param name="is_gw_only"> If true then the location only has groundwater acreage and the surface water
	/// supply cannot take up the slack the slack to meet the overall total. </param>
	public virtual void adjustGroundwaterAcresToTotalAcres(DateTime date, bool is_gw_only)
	{ // Format to integer as per output...
		double cds_total = __tacre_ts.getDataValue(date);
		if (cds_total < 0.0)
		{
			// Can't adjust to total because missing.
		}

		string routine = "StateCU_IrrigationPracticeTS.adjustGroundwaterAcresToTotalAcres";

		double Acgw_prev = __acgw_ts.getDataValue(date);
		double Acgwfl_prev = __acgwfl_ts.getDataValue(date);
		double Acgwspr_prev = __acgwspr_ts.getDataValue(date);

		int year = date.getYear(); // Used multiple times below

		// Make sure that the groundwater acres are up to date with the parts.
		// This should be OK because for groundwater processing the parts are used to
		// get the total.  If the parts are missing and only the total is set, then the
		// total will remain as before and won't get set to missing.

		// If for some reason the parts were read but the groundwater total has not been computec,
		// calculated it now...
		if ((Acgwfl_prev >= 0.0) && (Acgwspr_prev >= 0.0) && (Acgw_prev < 0.0))
		{
			refreshAcgw(year);
		}
		// Get new value...
		Acgw_prev = __acgw_ts.getDataValue(date);

		if (is_gw_only)
		{
			// Only have groundwater supply so adjust the groundwater up or down
			// to match the total acres.
			double diff = cds_total - Acgw_prev;
			if (Math.Abs(diff) > 0.1)
			{
				// Only print the message if there is a difference bigger than the 8.1 output
				// format would show...
				Message.printStatus(2, routine, "Location \"" + _id + "\" " + year + " GWacres (" + StringUtil.formatString(Acgw_prev,"%.1f") + ") != Total acres (" + StringUtil.formatString(cds_total,"%.1f") + ").  " + "  Attempting to adjust GWacres to Total acres.");
			}
			// Always make sure the GW acres agree with total below (may result in slight adjustments even
			// if above message is not printed).
			// And, always make sure that the groundwater acreage parts agree with the groundwater total.
			// This is needed because, for example, interpolation may be done on the total
			// and the parts need to be adjusted.

			__acgw_ts.setDataValue(date,cds_total);
			Acgw_prev = cds_total;
			Message.printStatus(2, routine, "Location \"" + _id + "\" " + year + " is ground water only.  Setting GWacres to Total acres (" + StringUtil.formatString(cds_total,"%.1f") + ").");
		}
		else
		{
			// Not groundwater only.
			// Only want to adjust down since surface water can take the extra
			if (Acgw_prev > cds_total)
			{
				__acgw_ts.setDataValue(date,cds_total);
				Acgw_prev = cds_total;
				Message.printStatus(2, routine, "Location \"" + _id + "\" " + year + " Adjusting GWacres down to Total acres (" + StringUtil.formatString(cds_total,"%.1f") + ").");
			}
			// Else GW is less than total and let SW take up slack below.
		}
		// Adjust the groundwater by irrigation method in any case...
		adjustGroundWaterIrrigationMethodAcres(date, Acgw_prev, Acgwfl_prev, Acgwspr_prev);

		// Also adjust the surface water acres to groundwater in response to what was
		// done above (in case surface water total changed).

		adjustSurfaceWaterAcresToGroundwaterAndTotalAcres(date, is_gw_only);
	}

	/// <summary>
	/// Adjust the groundwater acres parts to the total groundwater acres.  When called,
	/// the groundwater total must have been set, and the parts may also have been set.
	/// This methods brings the parts back into alignment.  Given that this method is called
	/// after one part is set, there should not be an issue with a data conflict. </summary>
	/// <param name="date"> Date (year) to process. </param>
	/// <param name="Acgw_prev"> New groundwater total acres to adjust to. </param>
	/// <param name="new_gw_total"> New groundwater total.  This is
	/// passed to improve performance, assuming it was also set/used in calling code. </param>
	/// <param name="Acgwfl_prev"> Acres of groundwater (flood) to give previous ratio.  This is
	/// passed to improve performance, assuming it was also set/used in calling code. </param>
	/// <param name="Acgwspr_prev"> Acres of groundwater (sprinkler) to give previous ratio.
	/// This is passed to improve performance, assuming it was also set/used in calling code. </param>
	private void adjustGroundWaterIrrigationMethodAcres(DateTime date, double Acgw_prev, double Acgwfl_prev, double Acgwspr_prev)
	{
		string routine = "StateCU_IrrigationPracticeTS.adjustGroundWaterIrrigationMethodAcres";
		int year = date.getYear();
		if (Acgw_prev < 0.0)
		{
			// Missing GWtotal so can't do anything
			Message.printStatus(2,routine, "Location \"" + _id + "\" " + year + ":  Acgw is missing.  Unable to adjust irrigation method terms.");
			return;
		}
		else if (Acgw_prev == 0.0)
		{
			// Set the irrigation method terms to zero...
			Message.printStatus(2,routine, "Location \"" + _id + "\" " + year + ":  Acgw is 0.  Setting irrigation method terms to zero.");
			__acgwfl_ts.setDataValue(date, 0.0);
			__acgwspr_ts.setDataValue(date, 0.0);
		}
		else if ((Acgwfl_prev < 0.0) && (Acgwspr_prev < 0.0))
		{
			// Both missing so can't adjust.
			Message.printStatus(2,routine, "Location \"" + _id + "\" " + year + ":  Acgw is known but irrigation method terms are missing.  Unable to adjust irrigation method terms.");
			return;
		}
		else if ((Acgwfl_prev >= 0.0) && (Acgwspr_prev >= 0.0))
		{
			// Both sprinkler and flood acres were specified before so need to
			// prorate both terms to agree with the total...
			double Acgw_parts_total = Acgwfl_prev + Acgwspr_prev;
			double Acgwfl_new = Acgw_prev * Acgwfl_prev / Acgw_parts_total;
			__acgwfl_ts.setDataValue(date,Acgwfl_new);
			Message.printStatus(2, routine, "For location " + _id + " " + year + " setting GWflood prorated to previous GWtotal (" + StringUtil.formatString(Acgwfl_new,"%.3f") + ") previous=" + StringUtil.formatString(Acgwfl_prev,"%.3f"));

			double Acgwspr_new = Acgw_prev * Acgwspr_prev / Acgw_parts_total;
			__acgwspr_ts.setDataValue(date,Acgwspr_new);
			Message.printStatus(2, routine, "For location " + _id + " " + year + " setting GWsprinkler prorated to previous GWtotal (" + StringUtil.formatString(Acgwspr_new,"%.3f") + ") previous=" + StringUtil.formatString(Acgwspr_prev,"%.3f"));
			// Refresh the total acres groundwater.
			refreshAcgw(year);
		}
		else if (Acgwfl_prev < 0.0)
		{
			// Only flood is missing so compute from the total minus the
			// sprinkler.  First adjust the sprinkler down to the total if necessary.
			if (Acgwspr_prev > Acgw_prev)
			{
				setAcgwspr(year,Acgw_prev);
				Acgwspr_prev = Acgw_prev;
				Message.printStatus(2,routine, "Location \"" + _id + "\" " + year + ":  Acgwspr reduced to Acgw = " + StringUtil.formatString(Acgwspr_prev,"%.3f"));
			}
			// Now compute the flood acres...
			setAcgwfl(year,(Acgw_prev - Acgwspr_prev));
			Message.printStatus(2,routine, "Location \"" + _id + "\" " + year + ":  Acgwfl computed as Acgw-Acgwspr=" + StringUtil.formatString(Acgw_prev,"%.3f") + " - " + StringUtil.formatString(Acgwspr_prev,"%.3f") + " = " + StringUtil.formatString(getAcgwfl(year),"%.3f"));
		}
		else if (Acgwspr_prev < 0.0)
		{
			// Only sprinkler is missing so compute from the total minus the
			// flood.  First adjust the flood down to the total if necessary.
			if (Acgwfl_prev > Acgw_prev)
			{
				setAcgwfl(year,Acgw_prev);
				Acgwfl_prev = Acgw_prev;
				Message.printStatus(2,routine, "Location \"" + _id + "\" " + year + ":  Acgwfl reduced to Acgw = " + StringUtil.formatString(Acgwfl_prev,"%.3f"));
			}
			// Now compute the sprinkler acres...
			setAcswspr(year,(Acgw_prev - Acgwfl_prev));
			Message.printStatus(2,routine, "Location \"" + _id + "\" " + year + ":  Acgwspr computed as Acgw-Acgwfl=" + StringUtil.formatString(Acgw_prev,"%.3f") + " - " + StringUtil.formatString(Acgwfl_prev,"%.3f") + " = " + StringUtil.formatString(getAcgwspr(year),"%.3f"));
		}
	}

	/// <summary>
	/// Adjust the surface water acres to the groundwater and total acres.  This assumes that
	/// the total and groundwater acres are set and non-missing. </summary>
	/// <param name="date"> Date (year) for adjustment. </param>
	/// <param name="is_gw_only"> If true, the location is groundwater only. </param>
	public virtual void adjustSurfaceWaterAcresToGroundwaterAndTotalAcres(DateTime date, bool is_gw_only)
	{
		string routine = "StateCU_IrrigationPracticeTS.adjustSurfaceWaterAcresToGroundwaterAndTotalAcres";
		if (is_gw_only)
		{
			// There should not any surface water only data.
			__acswfl_ts.setDataValue(date, 0.0);
			__acswspr_ts.setDataValue(date, 0.0);
			refreshAcsw(date.getYear());
			return;
		}

		// If here then possibly have non-zero surface water terms.
		// First make sure that surface water total acres are computed.

		int year = date.getYear();
		double total = __tacre_ts.getDataValue(date);
		double gw = __acgw_ts.getDataValue(date);

		if ((total < 0.0) || (gw < 0.0))
		{
			Message.printStatus(2, routine, "Location \"" + _id + "\" " + year + ":  Cannot compute target SW acres because total acres (" + StringUtil.formatString(total,"%.3f") + ") or GW acres (" + StringUtil.formatString(gw,"%.3f") + ") are missing.  Not adjusting surface water total acreage or parts.");
			return;
		}

		// If here need to check the surface water acreage.

		double sw_target = total - gw;
		if (Math.Abs(sw_target) < .001)
		{
			// Sometimes math roundoff results in really small values that should
			// just be treated as zero.
			sw_target = 0.0;
		}
		if (sw_target == 0.0)
		{
			// Just set it, regardless of whether missing...
			__acswfl_ts.setDataValue(date, 0.0);
			__acswspr_ts.setDataValue(date, 0.0);
			refreshAcsw(year);
			return;
		}

		// If here, have surface water total and possible zero, one, or two irrigation part terms.
		// Always set the surface water totals so it can be printed and used for other data filling, etc.
		__acsw_ts.setDataValue(date, sw_target);

		double Acswfl_prev = __acswfl_ts.getDataValue(date);
		double Acswspr_prev = __acswspr_ts.getDataValue(date);
		if ((Acswfl_prev < 0.0) && (Acswspr_prev < 0.0))
		{
			// TODO SAM 2007-10-17 comment out when done debugging
				Message.printWarning(3,routine, "Location \"" + _id + "\" " + year + ", cannot adjust surface water irrigation method acres (Acsw=" + StringUtil.formatString(sw_target,"%.3f") + ") because flood and sprinkler acres are both missing - use another command to fill one term.");
		}
		else
		{
			adjustSurfaceWaterIrrigationMethodAcres(date, sw_target, Acswfl_prev, Acswspr_prev);
		}
	}

	/// <summary>
	/// Prorate and set the surface water acres parts.  When called, the surface water total and
	/// parts may not be in agreement, but the surface water total should have been set to
	/// its target value.  Therefore, only the parts will be adjusted. </summary>
	/// <param name="date"> Date (year) to process. </param>
	/// <param name="Acsw_prev"> New surface water total acres to adjust to (already set in object). </param>
	/// <param name="Acswfl_prev"> Acres of groundwater (flood) to give previous ratio. </param>
	/// <param name="Acswspr_prev"> Acres of groundwater (sprinkler) to give previous ratio. </param>
	private void adjustSurfaceWaterIrrigationMethodAcres(DateTime date, double Acsw_prev, double Acswfl_prev, double Acswspr_prev)
	{
		string routine = "StateCU_CropPatternTS.ajdustSurfaceWaterIrrigationMethodAcres";
		int year = date.getYear();
		if (Acsw_prev < 0.0)
		{
			// Missing SWtotal so can't do anything
			Message.printStatus(2,routine, "Location \"" + _id + "\" " + year + ":  Acsw is missing.  Unable to adjust irrigation method terms.");
			return;
		}
		else if ((Acswfl_prev < 0.0) && (Acswspr_prev < 0.0))
		{
			// Both missing so can't adjust.
			Message.printStatus(2,routine, "Location \"" + _id + "\" " + year + ":  Acsw is known but irrigation method terms are missing.  Unable to adjust irrigation method terms.");
			return;
		}
		else if (Acsw_prev == 0.0)
		{
			// Set both terms to zero...
			Message.printStatus(2,routine, "Location \"" + _id + "\" " + year + ":  Acsw is 0.  Setting irrigation method terms to zero.");
			__acswfl_ts.setDataValue(date, 0.0);
			__acswspr_ts.setDataValue(date, 0.0);
		}
		else if ((Acswfl_prev >= 0.0) && (Acswspr_prev >= 0.0))
		{
			// Both are specified so prorate to add to the SWtotal...
			double Acsw_parts_total = Acswfl_prev + Acswspr_prev;
			double Acswfl_new = Acsw_prev * Acswfl_prev / Acsw_parts_total;
			double Acswspr_new = Acsw_prev * Acswspr_prev / Acsw_parts_total;
			// Make an adjustment if both were zero.  In this case, assign equally...
			if ((Acswfl_prev == 0.0) && (Acswspr_prev == 0.0))
			{
				Acswfl_new = Acsw_prev / 2.0;
				Acswspr_new = Acsw_prev / 2.0;
			}
			setAcswfl(year,Acswfl_new);
			setAcswspr(year,Acswspr_new);
			Message.printStatus(2,routine, "Location \"" + _id + "\" " + year + ":  Adjusted SW acres (" + StringUtil.formatString(Acsw_prev,"%.3f") + ") to Total-GW.  Prorated to get new Acswfl=" + StringUtil.formatString(Acswfl_new,"%.3f") + " Acswspr=" + StringUtil.formatString(Acswspr_new,"%.3f"));
		}
		else if (Acswfl_prev < 0.0)
		{
			// Only flood is missing so compute from the total minus the
			// sprinkler.  First adjust the sprinkler down to the total if necessary.
			if (Acswspr_prev > Acsw_prev)
			{
				setAcswspr(year,Acsw_prev);
				Acswspr_prev = Acsw_prev;
				Message.printStatus(2,routine, "Location \"" + _id + "\" " + year + ":  Acswspr reduced to Acsw = " + StringUtil.formatString(Acswspr_prev,"%.3f"));
			}
			// Now compute the flood acres...
			setAcswfl(year,(Acsw_prev - Acswspr_prev));
			Message.printStatus(2,routine, "Location \"" + _id + "\" " + year + ":  Acswfl computed as Acsw-Acswspr=" + StringUtil.formatString(getAcswfl(year),"%.3f"));
		}
		else if (Acswspr_prev < 0.0)
		{
			// Only sprinkler is missing so compute from the total minus the
			// flood.  First adjust the flood down to the total if necessary.
			if (Acswfl_prev > Acsw_prev)
			{
				setAcswfl(year,Acsw_prev);
				Acswfl_prev = Acsw_prev;
				Message.printStatus(2,routine, "Location \"" + _id + "\" " + year + ":  Acswfl reduced to Acsw = " + StringUtil.formatString(Acswfl_prev,"%.3f"));
			}
			// Now compute the sprinkler acres...
			setAcswspr(year,(Acsw_prev - Acswfl_prev));
			Message.printStatus(2,routine, "Location \"" + _id + "\" " + year + ":  Acswspr computed as Acsw-Acswfl=" + StringUtil.formatString(getAcswspr(year),"%.3f"));
		}
	}

	/// <summary>
	/// Returns acres ground water supplemented acres for the requested year. </summary>
	/// <param name="year"> Year to retrieve data. </param>
	/// <param name="Acres"> ground water supplemented acres. </param>
	public virtual double getAcgw(int year)
	{
		__temp_DateTime.setYear(year);
		return __acgw_ts.getDataValue(__temp_DateTime);
	}

	/// <summary>
	/// Returns the acre ground water supply time series. </summary>
	/// <returns> acre ground water supply TS. </returns>
	public virtual YearTS getAcgwTS()
	{
		return __acgw_ts;
	}

	/// <summary>
	/// Returns acres ground water flood </summary>
	/// <param name="year"> Year to retrieve data </param>
	/// <returns> Acres ground water flood </returns>
	public virtual double getAcgwfl(int year)
	{
		__temp_DateTime.setYear(year);
		return __acgwfl_ts.getDataValue(__temp_DateTime);
	}

	/// <summary>
	/// Returns the acre ground water flood time series. </summary>
	/// <returns> acre ground water flood TS </returns>
	public virtual YearTS getAcgwflTS()
	{
		return __acgwfl_ts;
	}

	/// <summary>
	/// Returns acres ground water sprinkler </summary>
	/// <param name="Year"> to retrieve data </param>
	/// <returns> Acres ground water sprinkler </returns>
	public virtual double getAcgwspr(int year)
	{
		__temp_DateTime.setYear(year);
		return __acgwspr_ts.getDataValue(__temp_DateTime);
	}

	/// <summary>
	/// Returns the acre surface water sprinkler time series. </summary>
	/// <returns> acre surface water sprinkler TS </returns>
	public virtual YearTS getAcgwsprTS()
	{
		return __acgwspr_ts;
	}

	/// <summary>
	/// Returns acres ground water supplemented acres for the requested year. </summary>
	/// <param name="year"> Year to retrieve data. </param>
	/// <param name="Acres"> ground water supplemented acres. </param>
	public virtual double getAcsw(int year)
	{
		__temp_DateTime.setYear(year);
		return __acsw_ts.getDataValue(__temp_DateTime);
	}

	/// <summary>
	/// Returns the acre surface water supply only time series. </summary>
	/// <returns> acre surface water supply TS. </returns>
	public virtual YearTS getAcswTS()
	{
		return __acsw_ts;
	}

	/// <summary>
	/// Returns acres surface water flood </summary>
	/// <param name="Year"> to retrieve data </param>
	/// <returns> Acres surface water flood </returns>
	public virtual double getAcswfl(int year)
	{
		__temp_DateTime.setYear(year);
		return __acswfl_ts.getDataValue(__temp_DateTime);
	}

	/// <summary>
	/// Returns the acre surface water flood time series. </summary>
	/// <returns> acre surface water flood TS </returns>
	public virtual YearTS getAcswflTS()
	{
		return __acswfl_ts;
	}

	/// <summary>
	/// Returns acres surface water sprinkler </summary>
	/// <param name="Year"> to retrieve data </param>
	/// <returns> Acres surface water sprinkler </returns>
	public virtual double getAcswspr(int year)
	{
		__temp_DateTime.setYear(year);
		return __acswspr_ts.getDataValue(__temp_DateTime);
	}

	/// <summary>
	/// Returns the acre surface water sprinkler time series. </summary>
	/// <returns> acre surface water sprinkler TS </returns>
	public virtual YearTS getAcswsprTS()
	{
		return __acswspr_ts;
	}

	/// <summary>
	/// Return Ceff for the given year. </summary>
	/// <returns> Ceff for the given year.  Return -999.0 for missing. </returns>
	/// <param name="year"> Year to retrieve data. </param>
	public virtual double getCeff(int year)
	{
		__temp_DateTime.setYear(year);
		return __ceff_ts.getDataValue(__temp_DateTime);
	}

	/// <summary>
	/// Return the time series for Ceff. </summary>
	/// <returns> the time series for Ceff. </returns>
	public virtual YearTS getCeffTS()
	{
		return __ceff_ts;
	}

	/// <summary>
	/// Return the start date for the time series. </summary>
	/// <returns> the start date for the time series. </returns>
	public virtual DateTime getDate1()
	{
		return __date1;
	}

	/// <summary>
	/// Return the end date for the time series. </summary>
	/// <returns> the end date for the time series. </returns>
	public virtual DateTime getDate2()
	{
		return __date2;
	}

	/// <summary>
	/// Return Feff for the given year. </summary>
	/// <returns> Feff for the given year.  Return -999.0 for missing. </returns>
	/// <param name="year"> Year to retrieve data. </param>
	public virtual double getFeff(int year)
	{
		__temp_DateTime.setYear(year);
		return __feff_ts.getDataValue(__temp_DateTime);
	}

	/// <summary>
	/// Return the time series for Feff. </summary>
	/// <returns> the time series for Feff. </returns>
	public virtual YearTS getFeffTS()
	{
		return __feff_ts;
	}

	/// <summary>
	/// Return Gacre for the given year. </summary>
	/// <returns> Gacre for the given year.  Return -999.0 for missing. </returns>
	/// <param name="year"> Year to retrieve data. </param>
	/* FIXME SAM 2007-10-18 Remove later when tested out
	public double getGacre ( int year ) 
	{	__temp_DateTime.setYear ( year );
		return __gacre_ts.getDataValue ( __temp_DateTime );
	}
	*/

	/// <summary>
	/// Return the time series for Gacre. </summary>
	/// <returns> the time series for Gacre. </returns>
	/* FIXME SAM 2007-10-18 Remove later when tested out
	public YearTS getGacreTS ()
	{	return __gacre_ts;
	}
	*/

	/// <summary>
	/// Return Gmode for the given year. </summary>
	/// <returns> Gmode for the given year.  Return -999.0 for missing. </returns>
	/// <param name="year"> Year to retrieve data. </param>
	public virtual int getGmode(int year)
	{
		__temp_DateTime.setYear(year);
		double gmode = __gmode_ts.getDataValue(__temp_DateTime);
		if (__gmode_ts.isDataMissing(gmode))
		{
			return -999;
		}
		else
		{
			return (int)(gmode + .1);
		}
	}

	/// <summary>
	/// Return the time series for Gmode. </summary>
	/// <returns> the time series for Gmode. </returns>
	public virtual YearTS getGmodeTS()
	{
		return __gmode_ts;
	}

	/// <summary>
	/// Return Mprate for the given year. </summary>
	/// <returns> Mprate for the given year.  Return -999.0 for missing. </returns>
	/// <param name="year"> Year to retrieve data. </param>
	public virtual double getMprate(int year)
	{
		__temp_DateTime.setYear(year);
		return __mprate_ts.getDataValue(__temp_DateTime);
	}

	/// <summary>
	/// Return the time series for Mprate. </summary>
	/// <returns> the time series for Mprate. </returns>
	public virtual YearTS getMprateTS()
	{
		return __mprate_ts;
	}

	/// <summary>
	/// Return the parcels for a requested year.  These values can be used in data filling. </summary>
	/// <param name="year"> Parcel year of interest or <= number if all years should be returned. </param>
	/// <returns> the list of StateCU_Parcel for a year </returns>
	public virtual IList<StateCU_Parcel> getParcelListForYear(int year)
	{
		IList<StateCU_Parcel> parcels = new List<StateCU_Parcel>();
		int size = __parcel_List.Count;
		StateCU_Parcel parcel;
		for (int i = 0; i < size; i++)
		{
			parcel = __parcel_List[i];
			if ((year > 0) && (parcel.getYear() != year))
			{
				// Requested year does not match.
				continue;
			}
			// Criteria are met
			parcels.Add(parcel);
		}
		return parcels;
	}

	/// <summary>
	/// Return Sacre for the given year. </summary>
	/// <returns> Sacre for the given year.  Return -999.0 for missing. </returns>
	/// <param name="year"> Year to retrieve data. </param>
	/* FIXME SAM 2007-10-18 Remove later when tested out
	public double getSacre ( int year ) 
	{	__temp_DateTime.setYear ( year );
		return __sacre_ts.getDataValue ( __temp_DateTime );
	}
	*/

	/// <summary>
	/// Return the time series for Sacre. </summary>
	/// <returns> the time series for Sacre. </returns>
	/* FIXME SAM 2007-10-18 Remove later when tested out
	public YearTS getSacreTS ()
	{	return __sacre_ts;
	}
	*/

	/// <summary>
	/// Return Seff for the given year. </summary>
	/// <returns> Seff for the given year.  Return -999.0 for missing. </returns>
	/// <param name="year"> Year to retrieve data. </param>
	public virtual double getSeff(int year)
	{
		__temp_DateTime.setYear(year);
		return __seff_ts.getDataValue(__temp_DateTime);
	}

	/// <summary>
	/// Return the time series for Seff. </summary>
	/// <returns> the time series for Seff. </returns>
	public virtual YearTS getSeffTS()
	{
		return __seff_ts;
	}

	/// <summary>
	/// Return the time series for Tacre. </summary>
	/// <returns> the time series for Tacre. </returns>
	public virtual YearTS getTacreTS()
	{
		return __tacre_ts;
	}

	/// <summary>
	/// Return Tacre for the given year. </summary>
	/// <returns> Tacre for the given year.  Return -999.0 for missing. </returns>
	/// <param name="year"> Year to retrieve data. </param>
	public virtual double getTacre(int year)
	{
		__temp_DateTime.setYear(year);
		return __tacre_ts.getDataValue(__temp_DateTime);
	}

	/// <summary>
	/// Return a time series, based on the data type. </summary>
	/// <returns> a time series, based on the data type, or null if the data type is not recognized. </returns>
	/// <param name="datatype"> The data type of the time series to return.  The data type
	/// should match one of the values returned by getTimeSeriesDataTypes(). </param>
	public virtual YearTS getTimeSeries(string datatype)
	{
		if (datatype.Equals(TSTYPE_Eff_SurfaceMax, StringComparison.OrdinalIgnoreCase))
		{
			return __ceff_ts;
		}
		else if (datatype.Equals(TSTYPE_Eff_FloodMax, StringComparison.OrdinalIgnoreCase))
		{
			return __feff_ts;
		}
		else if (datatype.Equals(TSTYPE_Eff_SprinklerMax, StringComparison.OrdinalIgnoreCase))
		{
			return __seff_ts;
		}
		/* FIXME SAM 2007-10-18 Remove later when tested out
		else if ( datatype.equalsIgnoreCase ( TSTYPE_CropArea_GroundWaterVersion10 ) ) {
			return __gacre_ts;
		}
		else if ( datatype.equalsIgnoreCase ( TSTYPE_CropArea_SprinklerVersion10 ) ) {
			return __sacre_ts;
		}
		*/
		else if (datatype.Equals(TSTYPE_CropArea_SurfaceWaterOnly, StringComparison.OrdinalIgnoreCase))
		{
			return __acsw_ts;
		}
		else if (datatype.Equals(TSTYPE_CropArea_SurfaceWaterOnlyFlood, StringComparison.OrdinalIgnoreCase))
		{
			return __acswfl_ts;
		}
		else if (datatype.Equals(TSTYPE_CropArea_SurfaceWaterOnlySprinkler, StringComparison.OrdinalIgnoreCase))
		{
			return __acswspr_ts;
		}
		else if (datatype.Equals(TSTYPE_CropArea_GroundWater, StringComparison.OrdinalIgnoreCase))
		{
			return __acgw_ts;
		}
		else if (datatype.Equals(TSTYPE_CropArea_GroundWaterFlood, StringComparison.OrdinalIgnoreCase))
		{
			return __acgwfl_ts;
		}
		else if (datatype.Equals(TSTYPE_CropArea_GroundWaterSprinkler, StringComparison.OrdinalIgnoreCase))
		{
			return __acgwspr_ts;
		}
		else if (datatype.Equals(TSTYPE_PumpingMax, StringComparison.OrdinalIgnoreCase))
		{
			return __mprate_ts;
		}
		else if (datatype.Equals(TSTYPE_GWUseMode, StringComparison.OrdinalIgnoreCase))
		{
			return __gmode_ts;
		}
		else if (datatype.Equals(TSTYPE_CropArea_Total, StringComparison.OrdinalIgnoreCase))
		{
			return __tacre_ts;
		}
		else
		{
			return null;
		}
	}

	/// <summary>
	/// Return the time series data types available in this class.  This can be called
	/// to more generically handle time series.  The data types are returned in the
	/// order of the file.  The default is to return the types based on the most current
	/// file format.  Call the overloaded method to get types based on an older format. </summary>
	/// <returns> a list of time series data types available for a StateCU_IrrigationPracticeTS. </returns>
	/// <param name="include_gwmode"> If true, include the GW mode time series data type.  This
	/// is a flag that is not suitable for numerical filling. </param>
	/// <param name="include_notes"> If true, include " - Note" notes after the data type. This is currently disabled. </param>
	public static IList<string> getTimeSeriesDataTypes(bool include_gwmode, bool include_notes)
	{
		return getTimeSeriesDataTypes(include_gwmode, include_notes, null);
	}

	/// <summary>
	/// Return the time series data types available in this class.  This can be called
	/// to more generically handle time series.  The data types are returned in the order of the file. </summary>
	/// <returns> a list of time series data types available for a StateCU_IrrigationPracticeTS. </returns>
	/// <param name="include_gwmode"> If true, include the GW mode time series data type.  This
	/// is flag that is not suitable for numerical filling. </param>
	/// <param name="include_notes"> If true, include " - Note" notes after the data type.  This is currently disabled. </param>
	/// <param name="version"> File version.  Use null or blank for the most recent version or
	/// use "10" for version 10 data types. </param>
	public static IList<string> getTimeSeriesDataTypes(bool include_gwmode, bool include_notes, string version)
	{
		IList<string> datatypes = new List<string> (12);

		datatypes.Add(TSTYPE_Eff_SurfaceMax);
		datatypes.Add(TSTYPE_Eff_FloodMax);
		datatypes.Add(TSTYPE_Eff_SprinklerMax);

		/* FIXME SAM 2007-10-18 Remove later when tested out
		if( (version != null) && version.equals("10") ) {
			datatypes.addElement ( TSTYPE_CropArea_GroundWaterVersion10 );
			datatypes.addElement ( TSTYPE_CropArea_SprinklerVersion10 );
		}
		else {*/
			//TODO SAM Determine whether old types should be totally removed
			// after generating data
			datatypes.Add(TSTYPE_CropArea_Total);
			datatypes.Add(TSTYPE_CropArea_SurfaceWaterOnly);
			datatypes.Add(TSTYPE_CropArea_GroundWater);
			datatypes.Add(TSTYPE_CropArea_SurfaceWaterOnlyFlood);
			datatypes.Add(TSTYPE_CropArea_SurfaceWaterOnlySprinkler);
			datatypes.Add(TSTYPE_CropArea_GroundWaterFlood);
			datatypes.Add(TSTYPE_CropArea_GroundWaterSprinkler);
		//}

		datatypes.Add(TSTYPE_PumpingMax);

		if (include_gwmode)
		{
			datatypes.Add(TSTYPE_GWUseMode);
		}

		return datatypes;
	}

	/// <summary>
	/// Return the year type ("CYR", "WYR", "IYR") for the time series. </summary>
	/// <returns> the year type for the time series. </returns>
	public virtual string getYearType()
	{
		return __year_type;
	}

	/// <summary>
	/// Indicate whether the location has groundwater supply.  This will be true if
	/// any of the StateCU_Supply associated with the parcel return isGroundWater as
	/// true.
	/// </summary>
	public virtual bool hasGroundWaterSupply()
	{
		int size = __parcel_List.Count;
		StateCU_Parcel parcel = null;
		for (int i = 0; i < size; i++)
		{
			parcel = (StateCU_Parcel)__parcel_List[i];
			if (parcel.hasGroundWaterSupply())
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Determine whether a StateCU file is an irrigation practice time series file.
	/// Currently the only check is to see if the file has a "ipy" or "tsp" extension. </summary>
	/// <param name="filename"> Name of file to examine. </param>
	/// <returns> true if the file is crop pattern time series file, false otherwise. </returns>
	public static bool isIrrigationPracticeTSFile(string filename)
	{
		string ext = IOUtil.getFileExtension(filename);
		if (ext.Equals("ipy", StringComparison.OrdinalIgnoreCase) || ext.Equals("tsp", StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Checks for the period in the header by reading the first non-comment line.
	/// If the first 2 characters are spaces, it is assumed that a period header is present. </summary>
	/// <param name="filename"> Absolute path to filename to check. </param>
	/// <returns> true if the file includes a period in the header, false if not. </returns>
	/// <exception cref="IOException"> if the file cannot be opened. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static boolean isPeriodInHeader(String filename) throws java.io.IOException
	private static bool isPeriodInHeader(string filename)
	{
		string fname = filename;
		string line = "";
		StreamReader input = null;

		// Read the StateCU file.  Only read the first non-comment line 
		input = new StreamReader(fname);
		while (!string.ReferenceEquals((line = input.ReadLine()), null))
		{
			// check for comments
			if (line.StartsWith("#", StringComparison.Ordinal) || line.Trim().Length == 0)
			{
				continue;
			}
			// Not a comment so break out of loop...
			break;
		}
		// The last line read above should be the header line with the period, or the first line of data.

		bool period_in_header = false;
		if ((line.Length > 2) && line.Substring(0,2).Equals("  "))
		{
			// Assume period in header
			period_in_header = true;
			string routine = "StateCU_IrrigationPracticeTS.isPeriodInHeader";
			Message.printStatus(2, routine, "File has period in the header (line=\"" + line + ").");
		}
		else
		{
			// Assume no period in header
			period_in_header = false;
		}

		input.Close();
		return period_in_header;
	}

	/// <summary>
	/// Checks for version 10 by reading the file and checking the field length </summary>
	/// <param name="filename"> </param>
	/// <returns> rVal </returns>
	/// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static boolean isVersion_10(String filename) throws java.io.IOException
	private static bool isVersion_10(string filename)
	{
		string fname = filename;
		string line = "";
		bool version10 = false;
		StreamReader input = null;
		string routine = "StateCU_Irrigation.isVersion10";

		// Read the StateCU file.  Only read the first line 
		// This is enough to know if it is version 10
		input = new StreamReader(fname);
		bool period_in_header = isPeriodInHeader(fname);
		while (!string.ReferenceEquals((line = input.ReadLine()), null))
		{
			// check for comments
			if (!line.StartsWith("#", StringComparison.Ordinal))
			{
				break;
			}
		}
		if (period_in_header)
		{
			// Header line will have been read so read another line to get data...
			line = input.ReadLine();
		}
		// Check the line of data...
		// Old IPY format has only 10 elements on lines after the header
		IList<string> tmp = StringUtil.breakStringList(line," ",StringUtil.DELIM_SKIP_BLANKS);
		if (tmp.Count < 12)
		{
			Message.printStatus(2, routine, "IPY file found to be in version 10 format: " + tmp.Count + " items in first data record (" + line + ").");
			version10 = true;
		}
		else
		{
			version10 = false;
		}
		input.Close();
		return version10;
	}

	/// <summary>
	/// Read the StateCU TSP file and return as a list of StateCU_IrrigationPracticeTS. </summary>
	/// <param name="filename"> filename containing irrigation practice time series records. </param>
	/// <param name="date1_req"> Requested start of period. </param>
	/// <param name="date2_req"> Requested end of period. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.List<StateCU_IrrigationPracticeTS> readStateCUFile(String filename, RTi.Util.Time.DateTime date1_req, RTi.Util.Time.DateTime date2_req) throws Exception
	public static IList<StateCU_IrrigationPracticeTS> readStateCUFile(string filename, DateTime date1_req, DateTime date2_req)
	{
		return readStateCUFile(filename, date1_req, date2_req, null);
	}

	/// <summary>
	/// Read the StateCU TSP file and return as a Vector of StateCU_IrrigationPracticeTS. </summary>
	/// <param name="filename"> filename containing irrigation practice time series records. </param>
	/// <param name="date1_req"> Requested start of period. </param>
	/// <param name="date2_req"> Requested end of period. </param>
	/// <param name="props"> properties to control the read, as follows:
	/// <table width=100% cellpadding=10 cellspacing=0 border=2>
	/// <tr>
	/// <td><b>Property</b></td>	<td><b>Description</b></td>	<td><b>Default</b></td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>Version</b></td>
	/// <td>If "10", the StateCU version 10 format file will be read.  Otherwise, the
	/// most current format will be read.</td>
	/// <td>True</td>
	/// </tr>
	/// 
	/// </table> </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.List<StateCU_IrrigationPracticeTS> readStateCUFile(String filename, RTi.Util.Time.DateTime date1_req, RTi.Util.Time.DateTime date2_req, RTi.Util.IO.PropList props) throws Exception
	public static IList<StateCU_IrrigationPracticeTS> readStateCUFile(string filename, DateTime date1_req, DateTime date2_req, PropList props)
	{
		string routine = "StateCU_IrrigationPracticeTS.readStateCUFile";
		string iline = null;
		IList<object> v = new List<object>();
		IList<StateCU_IrrigationPracticeTS> ipyts_Vector = new List<StateCU_IrrigationPracticeTS>();
		if (props == null)
		{
			props = new PropList("IPY");
		}
		string full_filename = IOUtil.getPathUsingWorkingDir(filename);
		Message.printStatus(2,routine,"Reading StateCU IPY file: " + full_filename);
		// If early versions (earlier than 10?), the period is not in the header
		// so determine by reading the first and last part of the file...
		// TODO SAM 2007-04-16 Need to figure out what version the addition
		// of period to the header occurred.  It seems to have been in version 10 and later?
		bool period_in_header = isPeriodInHeader(full_filename);
		string Version = props.getValue("Version");
		bool Version_10 = false;
		if ((!string.ReferenceEquals(Version, null)) && Version.Equals("10", StringComparison.OrdinalIgnoreCase))
		{
			// User has specified...
			Version_10 = true;
			Message.printStatus(2, routine, "Version 10 has been specified for file.");
		}
		else
		{
			// override the command if the version is found to be 10 
			// by checking field length of the second row
			Version_10 = isVersion_10(full_filename);
			//if ( Version_10 ) {
			//	Message.printStatus ( 2, routine, "File has been determined to be version 10.");
			//}
		}

		// Current format.

		int[] format_0 = new int[] {StringUtil.TYPE_STRING, StringUtil.TYPE_SPACE, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING};
		int[] format_0w = new int[] {4, 1, 12, 6, 6, 6, 8, 8, 8, 8, 12, 3, 8};

		if (!period_in_header || Version_10)
		{
			// Use the older format...
			if (Version_10)
			{
				format_0 = new int[11];
			}
			else
			{
				// No total...
				format_0 = new int[10];
			}
			format_0[0] = StringUtil.TYPE_STRING; // year
			format_0[1] = StringUtil.TYPE_SPACE;
			format_0[2] = StringUtil.TYPE_STRING; // CU Location ID
			format_0[3] = StringUtil.TYPE_STRING; // ceff
			format_0[4] = StringUtil.TYPE_STRING; // feff
			format_0[5] = StringUtil.TYPE_STRING; // seff
			format_0[6] = StringUtil.TYPE_STRING; // gacre
			format_0[7] = StringUtil.TYPE_STRING; // sacre
			format_0[8] = StringUtil.TYPE_STRING; // mprate
			format_0[9] = StringUtil.TYPE_STRING; // gmode
			if (Version_10)
			{
				// Do have the total...
				format_0[10] = StringUtil.TYPE_STRING; // tacre
			}
			if (Version_10)
			{
				format_0w = new int[11];
			}
			else
			{
				// No total...
				format_0w = new int[10];
			}
			format_0w[0] = 4; // Year
			format_0w[1] = 1; // Space
			format_0w[2] = 12; // CU Location ID
			format_0w[3] = 6; // ceff
			format_0w[4] = 6; // feff
			format_0w[5] = 6; // seff
			format_0w[6] = 8; // gacre
			format_0w[7] = 8; // gacre
			format_0w[8] = 12; // mprate
			format_0w[9] = 3; // gmode
			if (Version_10)
			{
				// Do have the total...
				format_0w[10] = 8; // tacre
			}
		}

		StateCU_IrrigationPracticeTS ipyts = null;
		StreamReader @in = null;

		// First get the period of record from the file.  The TSP file does not
		// currently have a header record with the period (this is being
		// considered).  Therefore, read the last records to get the end of the period...

		DateTime date1 = null, date2 = null;
		DateTime date1_file = null;
		DateTime date2_file = null;
		int year1 = -1, year = 0;

		if (!period_in_header)
		{
			// Older files do not have header information with the period for the time series.  Therefore,
			// grab a reasonable amount of the start and end of the file - then
			// read lines (broken by line breaks) until the last data line is encountered...

			RandomAccessFile ra = new RandomAccessFile(full_filename, "r");

			// Get the start of the file...

			sbyte[] b = new sbyte[5000];
			ra.read(b);
			string @string = null;
			string bs = StringHelper.NewString(b);
			IList<string> v2 = StringUtil.breakStringList(bs, "\n\r", StringUtil.DELIM_SKIP_BLANKS);
			// Loop through and figure out the first date.
			int size = v2.Count;
			string date1_string = null;
			IList<string> tokens = null;
			for (int i = 0; i < size; i++)
			{
				@string = v2[i].Trim();
				if ((@string.Length == 0) || (@string[0] == '#') || (@string[0] == ' '))
				{
					continue;
				}
				tokens = StringUtil.breakStringList(@string, " \t", StringUtil.DELIM_SKIP_BLANKS);
				date1_string = tokens[0];
				// Check for reasonable dates...
				if (StringUtil.isInteger(date1_string) && (StringUtil.atoi(date1_string) < 2050))
				{
					break;
				}
			}
			date1_file = DateTime.parse(date1_string);

			// Get the end of the file...
			long length = ra.length();
			// Skip to 5000 bytes from the end.  This should get some actual
			// data lines.  Save in a temporary array in memory.
			if (length >= 5000)
			{
				ra.seek(length - 5000);
			}
			ra.read(b);
			ra.close();
			ra = null;
			// Now break the bytes into records...
			bs = StringHelper.NewString(b);
			v2 = StringUtil.breakStringList(bs, "\n\r", StringUtil.DELIM_SKIP_BLANKS);
			// Loop through and figure out the last date.  Start at the
			// second record because it is likely that a complete record was not found...
			size = v2.Count;
			string date2_string = null;
			for (int i = 1; i < size; i++)
			{
				@string = v2[i].Trim();
				if ((@string.Length == 0) || (@string[0] == '#') || (@string[0] == ' '))
				{
					continue;
				}
				tokens = StringUtil.breakStringList(@string, " \t", StringUtil.DELIM_SKIP_BLANKS);
				@string = tokens[0];
				// Check for reasonable dates...
				if (StringUtil.isInteger(@string) && (StringUtil.atoi(@string) < 2050))
				{
					date2_string = @string;
				}
			}
			v2 = null;
			bs = null;
			date2_file = DateTime.parse(date2_string);

			Message.printStatus(2, routine, "No period in file header.  Period for file determined from data to be " + date1_file + " to " + date2_file);

			if (date1_req != null)
			{
				date1 = date1_req;
			}
			else
			{
				date1 = date1_file;
			}
			if (date2_req != null)
			{
				date2 = date2_req;
			}
			else
			{
				date2 = date2_file;
			}

			year1 = date1_file.getYear();
		}

		// The following throws an IOException if the file cannot be opened...
		@in = new StreamReader(full_filename);
		string culoc = "";
		int pos = 0;
		int linecount = 0;
		IList<object> tokens = null;
		string year_type = "CYR";
		int pos_error_count = 0; // Counter for error when the first year
						// of data does not contain the IDs.
						// Often this is because the year type
						// has not been considered when
						// formatting the data file.
		double acswfl = 0.0; // Acres for various time series.
		double acswspr = 0.0;
		double acgwfl = 0.0;
		double acgwspr = 0.0;
		//TS gacrets = null;		// Time series for groundwater and sprinkler acres
		//TS sacrets = null;
		try
		{
			while (!string.ReferenceEquals((iline = @in.ReadLine()), null))
			{
				++linecount;
				// check for comments
				if (iline.StartsWith("#", StringComparison.Ordinal) || iline.Trim().Length == 0)
				{
					continue;
				}

				// If the dates have not been determined, do so (assume that
				// the first line is header with the period, etc.).  Only read the years.
				if (date1_file == null)
				{
					// Treat all as strings for initial read...
					// The beginning and ending month, units, and year type
					// are ignored (assumed to be calendar and data type varies by column).
					string format_header = "x6s4x11s4";
					tokens = StringUtil.fixedRead(iline,format_header);
					date1_file = new DateTime(DateTime.PRECISION_YEAR);
					date1_file.setYear(StringUtil.atoi(((string)tokens[0]).Trim()));
					date2_file = new DateTime(DateTime.PRECISION_YEAR);
					date2_file.setYear(StringUtil.atoi(((string)tokens[1]).Trim()));
					// Year type is always assumed to be "CYR".
					year1 = date1_file.getYear(); // Expect the first line to match this.
					// Set the dates for processing...
					if (date1_req != null)
					{
						date1 = date1_req;
					}
					else
					{
						date1 = date1_file;
					}
					if (date2_req != null)
					{
						date2 = date2_req;
					}
					else
					{
						date2 = date2_file;
					}
					continue;
				}

				// We already read the header line and got the date above.  Need to skip this line. 
				//if( count == 1 && !period_in_header) {
					//continue;
				//}

				// Process the line of data...
				StringUtil.fixedRead(iline, format_0, format_0w, v);
				year = StringUtil.atoi(((string)v[0]).Trim());
				culoc = ((string)v[1]).Trim();

				if (year == year1)
				{
					// First year of data in the file.
					// Create an object for the CU Location.  It is assumed
					// that the structures are always listed in the first year at least (and probably every year).
					pos = StateCU_Util.IndexOf(ipyts_Vector, culoc);
					if (pos >= 0)
					{
						// Should not happen!  The CU Location is apparently listed twice in the first year...
						Message.printWarning(2, routine, "CU Location \"" + culoc + "\" is listed more than once in the first year.");
						ipyts = (StateCU_IrrigationPracticeTS)ipyts_Vector[pos];
					}
					else
					{
						if (Version_10)
						{
							ipyts = new StateCU_IrrigationPracticeTS(culoc, date1, date2, year_type, full_filename, 10);
						}
						else
						{
							ipyts = new StateCU_IrrigationPracticeTS(culoc, date1, date2, year_type, full_filename, 12);
						}
						ipyts_Vector.Add(ipyts);
					}
				}
				else
				{
					// Find the object of interest for this CU Location so it can be used to set data values...
					pos = StateCU_Util.IndexOf(ipyts_Vector, culoc);
					if (pos < 0)
					{
						// Should not happen!  Apparently the CU Location was not listed in the first year...
						++pos_error_count;
						Message.printWarning(3, routine, "CU Location \"" + culoc + "\" found in year " + year + " but was not listed in the first" + " year.");
						ipyts = new StateCU_IrrigationPracticeTS(culoc, date1, date2, year_type, full_filename);
						ipyts_Vector.Add(ipyts);
					}
					else
					{
						ipyts = ipyts_Vector[pos];
					}
				}
				// Now set the values...
				ipyts.setCeff(year, StringUtil.atof(((string)v[2]).Trim()));
				ipyts.setFeff(year, StringUtil.atof(((string)v[3]).Trim()));
				ipyts.setSeff(year, StringUtil.atof(((string)v[4]).Trim()));

				if (!period_in_header || Version_10)
				{
					// Have 2 acreage values and total acreage if version 10
					/* FIXME SAM 2007-10-18 Remove later when tested out
					ipyts.setGacre ( year, StringUtil.atof(((String)v.elementAt(5)).trim()) );
					ipyts.setSacre ( year, StringUtil.atof(((String)v.elementAt(6)).trim()) );
					*/
					ipyts.setMprate(year, StringUtil.atof(((string)v[7]).Trim()));
					ipyts.setGmode(year, StringUtil.atoi(((string)v[8]).Trim()));
					if (Version_10)
					{
						ipyts.setTacre(year, StringUtil.atof(((string)v[9]).Trim()));
					}
				}
				else
				{
					// New version 12 format
					acswfl = StringUtil.atof(((string)v[5]).Trim());
					ipyts.setAcswfl(year, acswfl);

					acswspr = StringUtil.atof(((string)v[6]).Trim());
					ipyts.setAcswspr(year, acswspr);

					acgwfl = StringUtil.atof(((string)v[7]).Trim());
					ipyts.setAcgwfl(year, acgwfl);

					acgwspr = StringUtil.atof(((string)v[8]).Trim());
					ipyts.setAcgwspr(year, acgwspr);

					// Refresh the total by supply type...

					ipyts.refreshAcsw(year);
					ipyts.refreshAcgw(year);

					ipyts.setMprate(year, StringUtil.atof(((string)v[9]).Trim()));
					ipyts.setGmode(year, StringUtil.atoi(((string)v[10]).Trim()));
					ipyts.setTacre(year, StringUtil.atof(((string)v[11]).Trim()));

					/* FIXME SAM 2007-10-18 Remove later when tested out
					// Set the Gacre and Sacre totals based on the sum
					// of the Gacre flood, sprinkler and the Sacre flood, sprinkler.
					// This way the code can still write old version files
					
					gacrets = ipyts.getGacreTS ();
					// Set the groundwater flood acres, whether missing or not...
					ipyts.setGacre ( year, acgwfl );
					// Add the groundwater sprinkler acres...
					if ( !gacrets.isDataMissing(acgwspr) ) {
						// Set/add the value
						if ( gacrets.isDataMissing(acgwfl)) {
							ipyts.setGacre ( year, acgwspr );
						}
						else {
							ipyts.setGacre ( year, acgwspr + acgwfl );
						}
					}
					sacrets = ipyts.getSacreTS ();
					// Set the groundwater sprinkler acres, whether missing or not...
					ipyts.setSacre ( year, acgwspr );
					// Add the surface water sprinkler acres...
					if ( !sacrets.isDataMissing(acswspr) ) {
						// Set/add the value
						if ( sacrets.isDataMissing(acgwspr)) {
							ipyts.setSacre ( year, acswspr );
						}
						else {
							ipyts.setSacre ( year, acgwspr + acswspr );
						}
					}
					*/
				}
			}
			if (pos_error_count > 0)
			{
				// FIXME SAM 2009-02-12 Evaluate how to handle
				Message.printWarning(1, routine, "One or more CU Location identifiers " + "were not listed in the first year of data.\n" + "The calendar type in the header may be incompatible with the data records.\n" + "See the log file for more information.");
			}
		}
		catch (Exception e)
		{
			Message.printWarning(3, routine, "Error processing near line " + linecount + ": " + iline + " uniquetempvar.");
			Message.printWarning(3, routine, e);
			// Now rethrow to calling code...
			throw (e);
		}
		finally
		{
			if (@in != null)
			{
				@in.Close();
			}
		}
		return ipyts_Vector;
	}

	/// <summary>
	/// Read a time series from a StateCU format file, using a time series identifier.
	/// The TSID string is specified in addition to the path to the file.  It is
	/// expected that a TSID in the file
	/// matches the TSID (and the path to the file, if included in the TSID would not
	/// properly allow the TSID to be specified).  This method can be used with newer
	/// code where the I/O path is separate from the TSID that is used to identify the time series.
	/// The IOUtil.getPathUsingWorkingDir() method is applied to the filename. </summary>
	/// <returns> a pointer to a newly-allocated time series if successful, null if not. </returns>
	/// <param name="tsident_string"> The full identifier for the time series to
	/// read.  This string can also be the alias for the time series in the file. </param>
	/// <param name="filename"> The name of a file to read
	/// (in which case the tsident_string must match one of the TSID strings in the file). </param>
	/// <param name="date1"> Starting date to initialize period (null to read the entire time series). </param>
	/// <param name="date2"> Ending date to initialize period (null to read the entire time series). </param>
	/// <param name="units"> Units to convert to. </param>
	/// <param name="read_data"> Indicates whether data should be read (false=no, true=yes). </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static RTi.TS.TS readTimeSeries(String tsident_string, String filename, RTi.Util.Time.DateTime date1, RTi.Util.Time.DateTime date2, String units, boolean read_data) throws Exception
	public static TS readTimeSeries(string tsident_string, string filename, DateTime date1, DateTime date2, string units, bool read_data)
	{
		TS ts = null;
		IList<TS> v = readTimeSeriesList(tsident_string, filename, date1, date2, units, read_data);
		if ((v != null) && (v.Count > 0))
		{
			ts = v[0];
		}
		return ts;
	}

	/// <summary>
	/// Read all the time series from a StateCU format file.
	/// The IOUtil.getPathUsingWorkingDir() method is applied to the filename. </summary>
	/// <returns> a pointer to a newly-allocated Vector of time series if successful, null if not. </returns>
	/// <param name="fname"> Name of file to read. </param>
	/// <param name="date1"> Starting date to initialize period (NULL to read the entire time series). </param>
	/// <param name="date2"> Ending date to initialize period (NULL to read the entire time series). </param>
	/// <param name="units"> Units to convert to. </param>
	/// <param name="read_data"> Indicates whether data should be read. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.List<RTi.TS.TS> readTimeSeriesList(String fname, RTi.Util.Time.DateTime date1, RTi.Util.Time.DateTime date2, String units, boolean read_data) throws Exception
	public static IList<TS> readTimeSeriesList(string fname, DateTime date1, DateTime date2, string units, bool read_data)
	{
		IList<TS> tslist = null;

		string full_fname = IOUtil.getPathUsingWorkingDir(fname);
		tslist = readTimeSeriesList(null, full_fname, date1, date2, units, read_data);
		// Return all the time series.
		return tslist;
	}

	/// <summary>
	/// Read one or more time series from a StateCU crop pattern time series format file. </summary>
	/// <returns> a list of time series if successful, null if not.  The calling code
	/// is responsible for freeing the memory for the time series. </returns>
	/// <param name="req_tsident"> Identifier for requested item series.  If null,
	/// return all new time series in the vector.  If not null, return the matching time series.
	/// @parm full_filename Full path to filename, used for messages. </param>
	/// <param name="req_date1"> Requested starting date to initialize period (or null to read the entire time series). </param>
	/// <param name="req_date2"> Requested ending date to initialize period (or null to read the entire time series). </param>
	/// <param name="units"> Units to convert to (currently ignored). </param>
	/// <param name="read_data"> Indicates whether data should be read. </param>
	/// <exception cref="Exception"> if there is an error reading the time series. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static java.util.List<RTi.TS.TS> readTimeSeriesList(String req_tsident, String full_filename, RTi.Util.Time.DateTime req_date1, RTi.Util.Time.DateTime req_date2, String req_units, boolean read_data) throws Exception
	private static IList<TS> readTimeSeriesList(string req_tsident, string full_filename, DateTime req_date1, DateTime req_date2, string req_units, bool read_data)
	{ // TODO - can optimize this later to only read one time series...
		// First read the whole file...

		IList<StateCU_IrrigationPracticeTS> data_Vector = readStateCUFile(full_filename, req_date1, req_date2);
		// If all the time series are required, return all...
		int size = 0;
		if (data_Vector != null)
		{
			size = data_Vector.Count;
		}
		IList<TS> tslist = new List<TS>(size * 8);
		StateCU_IrrigationPracticeTS ipy;
		TSIdent tsident = null;
		string req_data_type = null;
		if (!string.ReferenceEquals(req_tsident, null))
		{
			tsident = new TSIdent(req_tsident);
			req_data_type = tsident.getType();
		}
		bool include_ts = true;
		for (int i = 0; i < size; i++)
		{
			include_ts = true;
			ipy = data_Vector[i];
			if (!string.ReferenceEquals(req_tsident, null))
			{
				// Check to see if the location match...
				if (!ipy.getID().Equals(tsident.getLocation(), StringComparison.OrdinalIgnoreCase))
				{
					include_ts = false;
				}
			}
			if (!include_ts)
			{
				continue;
			}
			if ((string.ReferenceEquals(req_data_type, null)) || req_data_type.Equals(TSTYPE_Eff_SurfaceMax, StringComparison.OrdinalIgnoreCase))
			{
				tslist.Add(ipy.getCeffTS());
			}
			if ((string.ReferenceEquals(req_data_type, null)) || req_data_type.Equals(TSTYPE_Eff_FloodMax, StringComparison.OrdinalIgnoreCase))
			{
				tslist.Add(ipy.getFeffTS());
			}
			if ((string.ReferenceEquals(req_data_type, null)) || req_data_type.Equals(TSTYPE_Eff_SprinklerMax, StringComparison.OrdinalIgnoreCase))
			{
				tslist.Add(ipy.getSeffTS());
			}
			// Check for null time series below and only add the ones
			// that are not null.  This reflects the fact that version settings
			// may indicate that some time series should be ignored.
			/* FIXME SAM 2007-10-18 Remove later when tested out
			if ( (req_data_type == null) || req_data_type.equalsIgnoreCase(TSTYPE_CropArea_GroundWaterVersion10)){
				if ( ipy.getGacreTS() != null ) {
					tslist.addElement ( ipy.getGacreTS() );
				}
			}
			if ( (req_data_type == null) || req_data_type.equalsIgnoreCase(TSTYPE_CropArea_SprinklerVersion10)){
				if ( ipy.getSacreTS() != null ) {
					tslist.addElement ( ipy.getSacreTS() );
				}
			}
			*/
			if ((string.ReferenceEquals(req_data_type, null)) || req_data_type.Equals(TSTYPE_CropArea_SurfaceWaterOnly, StringComparison.OrdinalIgnoreCase))
			{
				if (ipy.getAcswTS() != null)
				{
					tslist.Add(ipy.getAcswTS());
				}
			}
			if ((string.ReferenceEquals(req_data_type, null)) || req_data_type.Equals(TSTYPE_CropArea_SurfaceWaterOnlyFlood, StringComparison.OrdinalIgnoreCase))
			{
				if (ipy.getAcswflTS() != null)
				{
					tslist.Add(ipy.getAcswflTS());
				}
			}
			if ((string.ReferenceEquals(req_data_type, null)) || req_data_type.Equals(TSTYPE_CropArea_SurfaceWaterOnlySprinkler, StringComparison.OrdinalIgnoreCase))
			{
				if (ipy.getAcswsprTS() != null)
				{
					tslist.Add(ipy.getAcswsprTS());
				}
			}
			if ((string.ReferenceEquals(req_data_type, null)) || req_data_type.Equals(TSTYPE_CropArea_GroundWater, StringComparison.OrdinalIgnoreCase))
			{
				if (ipy.getAcgwTS() != null)
				{
					tslist.Add(ipy.getAcgwTS());
				}
			}
			if ((string.ReferenceEquals(req_data_type, null)) || req_data_type.Equals(TSTYPE_CropArea_GroundWaterFlood, StringComparison.OrdinalIgnoreCase))
			{
				if (ipy.getAcgwflTS() != null)
				{
					tslist.Add(ipy.getAcgwflTS());
				}
			}
			if ((string.ReferenceEquals(req_data_type, null)) || req_data_type.Equals(TSTYPE_CropArea_GroundWaterSprinkler, StringComparison.OrdinalIgnoreCase))
			{
				if (ipy.getAcgwsprTS() != null)
				{
					tslist.Add(ipy.getAcgwsprTS());
				}
			}
			if ((string.ReferenceEquals(req_data_type, null)) || req_data_type.Equals(TSTYPE_PumpingMax, StringComparison.OrdinalIgnoreCase))
			{
				tslist.Add(ipy.getMprateTS());
			}
			if ((string.ReferenceEquals(req_data_type, null)) || req_data_type.Equals(TSTYPE_GWUseMode, StringComparison.OrdinalIgnoreCase))
			{
				tslist.Add(ipy.getGmodeTS());
			}
			if ((string.ReferenceEquals(req_data_type, null)) || req_data_type.Equals(TSTYPE_CropArea_Total, StringComparison.OrdinalIgnoreCase))
			{
				if (ipy.getTacreTS() != null)
				{
					tslist.Add(ipy.getTacreTS());
				}
			}
		}
		return tslist;
	}

	/// <summary>
	/// Refresh the groundwater acreage (total of flood and sprinkler terms).
	/// For example, call this method when the file is read or values are set from a
	/// database.  If any term is missing the result is missing.  If both are non-missing
	/// the result is the sum.  In years with
	/// actual observations, there should be no missing.  The handling of missing may need
	/// to be evaluated in other cases where data filling steps are occurring. </summary>
	/// <param name="year"> Year to refresh data. </param>
	public virtual void refreshAcgw(int year)
	{
		double acgwfl = getAcgwfl(year);
		double acgwspr = getAcgwspr(year);
		bool acgwfl_ismissing = __acgwfl_ts.isDataMissing(acgwfl);
		bool acgwspr_ismissing = __acgwspr_ts.isDataMissing(acgwspr);
		if (acgwfl_ismissing || acgwspr_ismissing)
		{
			// Set the total to missing...
			setAcgw(year, __acgw_ts.getMissing());
		}
		/* TODO SAM 2007-09-11 Evaluate use
		else if ( acgwfl_ismissing ) {
			// Set to non-missing sprinkler...
			setAcgw ( year, acgwspr );
		}
		else if ( acgwspr_ismissing ) {
			// Set to non-missing flood...
			setAcgw ( year, acgwfl );
		}
		*/
		else
		{
			// Set to the sum...
			setAcgw(year, (acgwfl + acgwspr));
		}
	}

	/// <summary>
	/// Refresh the surface water only acreage (total of flood and sprinkler terms).
	/// For example, call this method when the file is read or values are set from a
	/// database.  If either term is missing the result is missing.  If both are non-missing
	/// the result is the sum.  In years with
	/// actual observations, there should be no missing.  The handling of missing may need
	/// to be evaluated in other cases where data filling steps are occurring. </summary>
	/// <param name="year"> Year to refresh data. </param>
	public virtual void refreshAcsw(int year)
	{
		double acswfl = getAcswfl(year);
		double acswspr = getAcswspr(year);
		bool acswfl_ismissing = __acswfl_ts.isDataMissing(acswfl);
		bool acswspr_ismissing = __acswspr_ts.isDataMissing(acswspr);
		if (acswfl_ismissing || acswspr_ismissing)
		{
			// Set the total to missing...
			setAcsw(year, __acsw_ts.getMissing());
		}
		/* TODO SAM 2007-09-11 Evaluate use
		else if ( acswfl_ismissing ) {
			// Set to non-missing sprinkler...
			setAcsw ( year, acswspr );
		}
		else if ( acswspr_ismissing ) {
			// Set to non-missing flood...
			setAcsw ( year, acswfl );
		}
		*/
		else
		{
			// Set to the sum...
			setAcsw(year, (acswfl + acswspr));
		}
	}

	/// <summary>
	/// Set acres ground water supplemented. </summary>
	/// <param name="Year"> for data. </param>
	/// <param name="acgw"> value to set. </param>
	public virtual void setAcgw(int year, double acgw)
	{
		__temp_DateTime.setYear(year);
		__acgw_ts.setDataValue(__temp_DateTime, acgw);
	}

	/// <summary>
	/// Set acres ground water flood. </summary>
	/// <param name="Year"> for data. </param>
	/// <param name="acgwfl"> value to set. </param>
	public virtual void setAcgwfl(int year, double acgwfl)
	{
		__temp_DateTime.setYear(year);
		__acgwfl_ts.setDataValue(__temp_DateTime, acgwfl);
	}

	/// <summary>
	/// Set GWflood acres and set GWsprinkler to GWtotal - GWflood.  GWtotal must be
	/// set to do anything and its value will not be changed.
	/// Try to maintain the exact value set, but use GWtotal if GWflood > GWtotal. </summary>
	/// <param name="Year"> for data. </param>
	/// <param name="acgwfl"> value to set. </param>
	public virtual void setAcgwflAndAdjust(int year, double acgwfl)
	{
		__temp_DateTime.setYear(year);
		// If the groundwater total is not available, don't do it.
		double acgw = __acgw_ts.getDataValue(__temp_DateTime);
		if (acgw < 0.0)
		{
			return;
		}
		if (acgwfl > acgw)
		{
			// Just use the total...
			acgwfl = acgw;
		}
		__acgwfl_ts.setDataValue(__temp_DateTime, acgwfl);
		// Now set GWsprinkler...
		__acgwspr_ts.setDataValue(__temp_DateTime, (acgw - acgwfl));
	}

	/// <summary>
	/// Set acres ground water sprinkler. </summary>
	/// <param name="Year"> for data. </param>
	/// <param name="acgwspr"> value to set. </param>
	public virtual void setAcgwspr(int year, double acgwspr)
	{
		__temp_DateTime.setYear(year);
		__acgwspr_ts.setDataValue(__temp_DateTime, acgwspr);
	}

	/// <summary>
	/// Set acres GWsprinkler and adjust the resulting GWflood data to GWtotal - GWsprinkler.
	/// Try to maintain the exact value set, except when greater than GWtotal, in which case GWtotal will be used.
	/// Don't do anything if GWtotal is not set. </summary>
	/// <param name="Year"> for data. </param>
	/// <param name="acgwspr"> value to set. </param>
	public virtual void setAcgwsprAndAdjust(int year, double acgwspr)
	{
		__temp_DateTime.setYear(year);
		// If the groundwater total is not available, don't do it.
		double acgw = __acgw_ts.getDataValue(__temp_DateTime);
		if (acgw < 0.0)
		{
			return;
		}
		if (acgwspr > acgw)
		{
			// Just use the total...
			acgwspr = acgw;
		}
		__acgwspr_ts.setDataValue(__temp_DateTime, acgwspr);
		// Now set GWflood...
		__acgwfl_ts.setDataValue(__temp_DateTime, (acgw - acgwspr));
	}

	/// <summary>
	/// Set acres surface water only. </summary>
	/// <param name="Year"> for data. </param>
	/// <param name="acsw"> value to set. </param>
	public virtual void setAcsw(int year, double acsw)
	{
		__temp_DateTime.setYear(year);
		__acsw_ts.setDataValue(__temp_DateTime, acsw);
	}

	/// <summary>
	/// Set acres surface water flood. </summary>
	/// <param name="Year"> for data. </param>
	/// <param name="acswfl"> value to set. </param>
	public virtual void setAcswfl(int year, double acswfl)
	{
		__temp_DateTime.setYear(year);
		__acswfl_ts.setDataValue(__temp_DateTime, acswfl);
	}

	/// <summary>
	/// Set acres SWflood and adjust the resulting SWsprinkler data to SWtotal - SWflood.
	/// Try to maintain the exact value set, except when greater than SWtotal, in which case SWtotal will be used.
	/// Don't do anything if SWtotal is not set. </summary>
	/// <param name="Year"> for data. </param>
	/// <param name="acswfl"> value to set. </param>
	public virtual void setAcswflAndAdjust(int year, double acswfl)
	{
		__temp_DateTime.setYear(year);
		// If the surface water total is not available, don't do it.
		double acsw = __acsw_ts.getDataValue(__temp_DateTime);
		if (acsw < 0.0)
		{
			return;
		}
		if (acswfl > acsw)
		{
			// Just use the total...
			acswfl = acsw;
		}
		__acswfl_ts.setDataValue(__temp_DateTime, acswfl);
		// Now set SWsprinkler...
		__acswspr_ts.setDataValue(__temp_DateTime, (acsw - acswfl));
	}

	/// <summary>
	/// Set acres surface water sprinkler. </summary>
	/// <param name="Year"> for data. </param>
	/// <param name="acswspr"> value to set. </param>
	public virtual void setAcswspr(int year, double acswspr)
	{
		__temp_DateTime.setYear(year);
		__acswspr_ts.setDataValue(__temp_DateTime, acswspr);
	}


	/// <summary>
	/// Set acres SWsprinkler and adjust the resulting SWflood data to SWtotal - SWsprinkler.
	/// Try to maintain the exact value set, except ewhen greater than SWtotal, in which case SWtotal will be used.
	/// Don't do anything if SWtotal is not set. </summary>
	/// <param name="Year"> for data. </param>
	/// <param name="acswspr"> value to set. </param>
	public virtual void setAcswsprAndAdjust(int year, double acswspr)
	{
		string routine = "StateCU_IrrigationPracticeTS.setAcswspAndAdjust";
		__temp_DateTime.setYear(year);
		// If the surface water total is not available, don't do it.
		double acsw = __acsw_ts.getDataValue(__temp_DateTime);
		if (acsw < 0.0)
		{
			Message.printStatus(2, routine, "Location \"" + _id + "\" " + year + " SWtotal acres is not set.  Unable to set/adjust to SWsprinkler acres.");
			return;
		}
		if (acswspr > acsw)
		{
			// Just use the total...
			acswspr = acsw;
			Message.printStatus(2, routine, "Location \"" + _id + "\" " + year + " SWsprinkler acres adjusted to SWtotal acres (" + (long)Math.Round(acswspr, MidpointRounding.AwayFromZero) + ")");
		}
		__acswspr_ts.setDataValue(__temp_DateTime, acswspr);
		// Now set SWflood...
		double acswfl = acsw - acswspr;
		__acswfl_ts.setDataValue(__temp_DateTime, acswfl);
		Message.printStatus(2, routine, "Location \"" + _id + "\" " + year + " SWflood adjusted to SWtotal - SWsprinkler (" + (long)Math.Round(acswspr, MidpointRounding.AwayFromZero) + ")");
	}

	/// <summary>
	/// Set Ceff. </summary>
	/// <param name="year"> Year for data. </param>
	/// <param name="ceff"> Ceff value to set. </param>
	public virtual void setCeff(int year, double ceff)
	{
		__temp_DateTime.setYear(year);
		__ceff_ts.setDataValue(__temp_DateTime, ceff);
	}

	/// <summary>
	/// Set Feff. </summary>
	/// <param name="year"> Year for data. </param>
	/// <param name="feff"> Feff value to set. </param>
	public virtual void setFeff(int year, double feff)
	{
		__temp_DateTime.setYear(year);
		__feff_ts.setDataValue(__temp_DateTime, feff);
	}

	/// <summary>
	/// Set Gacre. </summary>
	/// <param name="year"> Year for data. </param>
	/// <param name="gacre"> Gacre value to set. </param>
	/* FIXME SAM 2007-10-18 Remove later when tested out
	public void setGacre ( int year, double gacre )
	{	__temp_DateTime.setYear ( year );
		__gacre_ts.setDataValue ( __temp_DateTime, gacre );
	}
	*/

	/// <summary>
	/// Set Gmode. </summary>
	/// <param name="year"> Year for data. </param>
	/// <param name="gmode"> Gmode value to set. </param>
	public virtual void setGmode(int year, int gmode)
	{
		__temp_DateTime.setYear(year);
		__gmode_ts.setDataValue(__temp_DateTime, (double)gmode);
	}

	/// <summary>
	/// Set Mprate. </summary>
	/// <param name="year"> Year for data. </param>
	/// <param name="mprate"> Mprate value to set. </param>
	public virtual void setMprate(int year, double mprate)
	{
		__temp_DateTime.setYear(year);
		__mprate_ts.setDataValue(__temp_DateTime, mprate);
	}

	/// <summary>
	/// Set Sacre. </summary>
	/// <param name="year"> Year for data. </param>
	/// <param name="sacre"> Sacre value to set. </param>
	/* FIXME SAM 2007-10-18 Remove later when tested out
	public void setSacre ( int year, double sacre )
	{	__temp_DateTime.setYear ( year );
		__sacre_ts.setDataValue ( __temp_DateTime, sacre );
	}
	*/

	/// <summary>
	/// Set Seff. </summary>
	/// <param name="year"> Year for data. </param>
	/// <param name="seff"> Seff value to set. </param>
	public virtual void setSeff(int year, double seff)
	{
		__temp_DateTime.setYear(year);
		__seff_ts.setDataValue(__temp_DateTime, seff);
	}

	/// <summary>
	/// Set Tacre. </summary>
	/// <param name="year"> Year for data. </param>
	/// <param name="tacre"> Tacre value to set. </param>
	public virtual void setTacre(int year, double tacre)
	{
		__temp_DateTime.setYear(year);
		__tacre_ts.setDataValue(__temp_DateTime, tacre);
	}

	/// <summary>
	/// Return a string representation of this object.
	/// </summary>
	public override string ToString()
	{
		return _id;
	}

	/// <summary>
	/// Return a List containing all the time series in the data list.  Only the raw
	/// time series are returned.  Use the overloaded version to also return total time series. </summary>
	/// <returns> a list containing all the time series in the data list. </returns>
	/// <param name="dataList"> A list of StateCU_IrrigationPracticeTS. </param>
	public static IList<TS> toTSList(IList<StateCU_IrrigationPracticeTS> dataList)
	{
		return toTSList(dataList, false, null, null);
	}

	/// <summary>
	/// Return a List containing all the time series in the data list.
	/// Optionally, process the time series in the instance and add total time series for the entire data set.
	/// This is a performance hit but is useful for summarizing the data.  Any non-zero
	/// value in the individual time series will result in a value in the total.
	/// Missing for all time series will result in missing in the total.  The period for
	/// the totals is the overall period from all StateCU_IrrigationPracticeTS being processed. </summary>
	/// <returns> a list containing all the time series in the data list. </returns>
	/// <param name="dataList"> A list of StateCU_IrrigationPracticeTS. </param>
	/// <param name="include_dataset_totals"> If true, include totals for the entire data set,
	/// including the groundwater, sprinkler, and total acreage, and the total of maximum monthly pumping. </param>
	/// <param name="dataset_location"> A string used as the location for the data set totals.
	/// If not specified, "DataSet" will be used.  A non-null value should be supplied,
	/// in particular, if the totals for different data sets will be graphed or manipulated. </param>
	public static IList<TS> toTSList(IList<StateCU_IrrigationPracticeTS> dataList, bool include_dataset_totals, string dataset_location, string dataset_datasource)
	{
		string routine = "StateCU_IrrigationPracticeTS.toTSVector";
		IList<TS> tslist = new List<TS>();
		int size = 0;
		if (dataList != null)
		{
			size = dataList.Count;
		}
		StateCU_IrrigationPracticeTS ipy = null;
		DateTime start_DateTime = null, end_DateTime = null, date; // To allocate new time series.
		YearTS yts_acgw = null, yts_acsw = null, yts_tot = null, yts_pump = null, yts_acswfl = null, yts_acswspr = null, yts_acgwfl = null, yts_acgwspr = null;

		if (include_dataset_totals)
		{
			if ((string.ReferenceEquals(dataset_location, null)) || (dataset_location.Length == 0))
			{
				dataset_location = "DataSet";
			}
			if ((string.ReferenceEquals(dataset_datasource, null)) || (dataset_datasource.Length == 0))
			{
				dataset_location = "StateCU";
			}
			// Determine the period to use for new time series...
			for (int i = 0; i < size; i++)
			{
				ipy = (StateCU_IrrigationPracticeTS)dataList[i];
				date = ipy.getDate1();
				if ((start_DateTime == null) || date.lessThan(start_DateTime))
				{
					start_DateTime = new DateTime(date);
				}
				date = ipy.getDate2();
				if ((end_DateTime == null) || date.greaterThan(end_DateTime))
				{
					end_DateTime = new DateTime(date);
				}
			}
			// Add a time series for each time series data set total...
			// The following were originally only used with Version 10
			// but are also carried around for newer versions as subtotals.

			/* TODO SAM Evaluate whether old is needed - focus on individual acreage terms
			// Version 10 sprinkler acres...
	
			yts_spr = new YearTS ();
			try {	TSIdent tsident = new TSIdent (
					dataset_location, dataset_datasource,
					"CropArea-Sprinkler", "Year", "" );
					yts_spr.setIdentifier ( tsident );
					yts_spr.getIdentifier().setInputType ( "StateCU" );
			}
			catch ( Exception e ) {
					// This should NOT happen because the TSID is
					// being controlled...
					Message.printWarning ( 3, routine,
					"Error adding time series for sprinkler total." );
			}
			yts_spr.setDataUnits ( "ACRE" );
			yts_spr.setDataUnitsOriginal ( "ACRE" );
			yts_spr.setDescription ( dataset_location +
			" Sprinkler-irrigated area" );
			yts_spr.setDate1(new DateTime(start_DateTime));
			yts_spr.setDate2(new DateTime(end_DateTime));
			yts_spr.setDate1Original(new DateTime(start_DateTime));
			yts_spr.setDate2Original(new DateTime(end_DateTime));
			yts_spr.allocateDataSpace();
			*/

			// Also allocate the newer time series.  These will be
			// unused if printed in version 10.

			// Surface water flood...

			yts_acswfl = new YearTS();
			try
			{
				TSIdent tsident = new TSIdent(dataset_location, dataset_datasource, TSTYPE_CropArea_SurfaceWaterOnlyFlood, "Year", "");
				yts_acswfl.setIdentifier(tsident);
				yts_acswfl.getIdentifier().setInputType("StateCU");
			}
			catch (Exception)
			{
				// This should NOT happen because the TSID is being controlled...
				Message.printWarning(3, routine, "Error adding time series for surface water flood.");
			}
			yts_acswfl.setDataUnits("ACRE");
			yts_acswfl.setDataUnitsOriginal("ACRE");
			yts_acswfl.setDescription(dataset_location + " SurfaceWaterOnlyFlood-irrigated area");
			yts_acswfl.setDate1(new DateTime(start_DateTime));
			yts_acswfl.setDate2(new DateTime(end_DateTime));
			yts_acswfl.setDate1Original(new DateTime(start_DateTime));
			yts_acswfl.setDate2Original(new DateTime(end_DateTime));
			yts_acswfl.allocateDataSpace();

			// Surface water sprinkler...

			yts_acswspr = new YearTS();
			try
			{
				TSIdent tsident = new TSIdent(dataset_location, dataset_datasource, TSTYPE_CropArea_SurfaceWaterOnlySprinkler, "Year", "");
				yts_acswspr.setIdentifier(tsident);
				yts_acswspr.getIdentifier().setInputType("StateCU");
			}
			catch (Exception)
			{
				// This should NOT happen because the TSID is being controlled...
				Message.printWarning(3, routine, "Error adding time series for surface water sprinkler.");
			}
			yts_acswspr.setDataUnits("ACRE");
			yts_acswspr.setDataUnitsOriginal("ACRE");
			yts_acswspr.setDescription(dataset_location + " SurfaceWaterOnlySprinkler-irrigated area");
			yts_acswspr.setDate1(new DateTime(start_DateTime));
			yts_acswspr.setDate2(new DateTime(end_DateTime));
			yts_acswspr.setDate1Original(new DateTime(start_DateTime));
			yts_acswspr.setDate2Original(new DateTime(end_DateTime));
			yts_acswspr.allocateDataSpace();

			// Groundwater flood...

			yts_acgwfl = new YearTS();
			try
			{
				TSIdent tsident = new TSIdent(dataset_location, dataset_datasource, TSTYPE_CropArea_GroundWaterFlood, "Year", "");
				yts_acgwfl.setIdentifier(tsident);
				yts_acgwfl.getIdentifier().setInputType("StateCU");
			}
			catch (Exception)
			{
				// This should NOT happen because the TSID is being controlled...
				Message.printWarning(3, routine, "Error adding time series for groundwater flood.");
			}
			yts_acgwfl.setDataUnits("ACRE");
			yts_acgwfl.setDataUnitsOriginal("ACRE");
			yts_acgwfl.setDescription(dataset_location + " GroundwaterFlood-irrigated area");
			yts_acgwfl.setDate1(new DateTime(start_DateTime));
			yts_acgwfl.setDate2(new DateTime(end_DateTime));
			yts_acgwfl.setDate1Original(new DateTime(start_DateTime));
			yts_acgwfl.setDate2Original(new DateTime(end_DateTime));
			yts_acgwfl.allocateDataSpace();

			// Groundwater sprinkler...

			yts_acgwspr = new YearTS();
			try
			{
				TSIdent tsident = new TSIdent(dataset_location, dataset_datasource, TSTYPE_CropArea_GroundWaterSprinkler, "Year", "");
				yts_acgwspr.setIdentifier(tsident);
				yts_acgwspr.getIdentifier().setInputType("StateCU");
			}
			catch (Exception)
			{
				// This should NOT happen because the TSID is being controlled...
				Message.printWarning(3, routine, "Error adding time series for groundwater sprinkler.");
			}
			yts_acgwspr.setDataUnits("ACRE");
			yts_acgwspr.setDataUnitsOriginal("ACRE");
			yts_acgwspr.setDescription(dataset_location + " GroundWaterSprinkler-irrigated area");
			yts_acgwspr.setDate1(new DateTime(start_DateTime));
			yts_acgwspr.setDate2(new DateTime(end_DateTime));
			yts_acgwspr.setDate1Original(new DateTime(start_DateTime));
			yts_acgwspr.setDate2Original(new DateTime(end_DateTime));
			yts_acgwspr.allocateDataSpace();

			// Surface water only (flood and sprinkler)...

			yts_acsw = new YearTS();
			try
			{
				TSIdent tsident = new TSIdent(dataset_location, dataset_datasource, TSTYPE_CropArea_SurfaceWaterOnly, "Year", "");
				yts_acsw.setIdentifier(tsident);
				yts_acsw.getIdentifier().setInputType("StateCU");
			}
			catch (Exception)
			{
				// This should NOT happen because the TSID is being controlled...
				Message.printWarning(3, routine, "Error adding time series for surface water only supply total.");
			}
			yts_acsw.setDataUnits("ACRE");
			yts_acsw.setDataUnitsOriginal("ACRE");
			yts_acsw.setDescription(dataset_location + " SurfaceWaterOnly-irrigated area");
			yts_acsw.setDate1(new DateTime(start_DateTime));
			yts_acsw.setDate2(new DateTime(end_DateTime));
			yts_acsw.setDate1Original(new DateTime(start_DateTime));
			yts_acsw.setDate2Original(new DateTime(end_DateTime));
			yts_acsw.allocateDataSpace();

			// Groundwater (flood and sprinkler)...

			yts_acgw = new YearTS();
			try
			{
				TSIdent tsident = new TSIdent(dataset_location, dataset_datasource, TSTYPE_CropArea_GroundWater, "Year", "");
				yts_acgw.setIdentifier(tsident);
				yts_acgw.getIdentifier().setInputType("StateCU");
			}
			catch (Exception)
			{
				// This should NOT happen because the TSID is being controlled...
				Message.printWarning(3, routine, "Error adding time series for groundwater total.");
			}
			yts_acgw.setDataUnits("ACRE");
			yts_acgw.setDataUnitsOriginal("ACRE");
			yts_acgw.setDescription(dataset_location + " Groundwater-irrigated area");
			yts_acgw.setDate1(new DateTime(start_DateTime));
			yts_acgw.setDate2(new DateTime(end_DateTime));
			yts_acgw.setDate1Original(new DateTime(start_DateTime));
			yts_acgw.setDate2Original(new DateTime(end_DateTime));
			yts_acgw.allocateDataSpace();

			// Total of all irrigation...

			yts_tot = new YearTS();
			try
			{
				TSIdent tsident = new TSIdent(dataset_location, dataset_datasource, TSTYPE_CropArea_Total, "Year", "");
				yts_tot.setIdentifier(tsident);
				yts_tot.getIdentifier().setInputType("StateCU");
			}
			catch (Exception)
			{
				// This should NOT happen because the TSID is being controlled...
				Message.printWarning(3, routine, "Error adding time series for total acres.");
			}
			yts_tot.setDataUnits("ACRE");
			yts_tot.setDataUnitsOriginal("ACRE");
			yts_tot.setDescription(dataset_location + " groundwater-irrigated area");
			yts_tot.setDate1(new DateTime(start_DateTime));
			yts_tot.setDate2(new DateTime(end_DateTime));
			yts_tot.setDate1Original(new DateTime(start_DateTime));
			yts_tot.setDate2Original(new DateTime(end_DateTime));
			yts_tot.allocateDataSpace();

			yts_pump = new YearTS();
			try
			{
				TSIdent tsident = new TSIdent(dataset_location, dataset_datasource, TSTYPE_PumpingMax, "Year", "");
				yts_pump.setIdentifier(tsident);
				yts_pump.getIdentifier().setInputType("StateCU");
			}
			catch (Exception)
			{
				// This should NOT happen because the TSID is being controlled...
				Message.printWarning(3, routine, "Error adding time series for total acres.");
			}
			yts_pump.setDataUnits("ACFT");
			yts_pump.setDataUnitsOriginal("ACFT");
			yts_pump.setDescription(dataset_location + " groundwater-irrigated area");
			yts_pump.setDate1(new DateTime(start_DateTime));
			yts_pump.setDate2(new DateTime(end_DateTime));
			yts_pump.setDate1Original(new DateTime(start_DateTime));
			yts_pump.setDate2Original(new DateTime(end_DateTime));
			yts_pump.allocateDataSpace();
		}
		for (int i = 0; i < size; i++)
		{
			ipy = (StateCU_IrrigationPracticeTS)dataList[i];
			tslist.Add(ipy.__ceff_ts);
			tslist.Add(ipy.__feff_ts);
			tslist.Add(ipy.__seff_ts);

			if (ipy.__tacre_ts != null)
			{
				tslist.Add(ipy.__tacre_ts);
			}
			if (ipy.__acsw_ts != null)
			{
				tslist.Add(ipy.__acsw_ts);
			}
			if (ipy.__acgw_ts != null)
			{
				tslist.Add(ipy.__acgw_ts);
			}
			if (ipy.__acswfl_ts != null)
			{
				tslist.Add(ipy.__acswfl_ts);
			}
			if (ipy.__acswspr_ts != null)
			{
				tslist.Add(ipy.__acswspr_ts);
			}
			if (ipy.__acgwfl_ts != null)
			{
				tslist.Add(ipy.__acgwfl_ts);
			}
			if (ipy.__acgwspr_ts != null)
			{
				tslist.Add(ipy.__acgwspr_ts);
			}

			tslist.Add(ipy.__mprate_ts);
			tslist.Add(ipy.__gmode_ts);

			if (include_dataset_totals)
			{
				// Totals for version 10+ format...
				if ((yts_acsw != null) && (ipy.__acsw_ts != null))
				{
					try
					{
						TSUtil.add(yts_acsw, ipy.__acsw_ts);
					}
					catch (Exception)
					{
						Message.printWarning(3, routine, "Error adding time series.");
					}
				}
				if ((yts_acgw != null) && (ipy.__acgw_ts != null))
				{
					try
					{
						TSUtil.add(yts_acgw, ipy.__acgw_ts);
					}
					catch (Exception)
					{
						Message.printWarning(3, routine, "Error adding time series.");
					}
				}
				// Totals for version 12+ format
				if ((yts_acswfl != null) && (ipy.__acswfl_ts != null))
				{
					try
					{
						TSUtil.add(yts_acswfl, ipy.__acswfl_ts);
					}
					catch (Exception)
					{
						Message.printWarning(3, routine, "Error adding time series.");
					}
				}
				if ((yts_acswspr != null) && (ipy.__acswspr_ts != null))
				{
					try
					{
						TSUtil.add(yts_acswspr, ipy.__acswspr_ts);
					}
					catch (Exception)
					{
						Message.printWarning(3, routine, "Error adding time series.");
					}
				}
				if ((yts_acgwfl != null) && (ipy.__acgwfl_ts != null))
				{
					try
					{
						TSUtil.add(yts_acgwfl, ipy.__acgwfl_ts);
					}
					catch (Exception)
					{
						Message.printWarning(3, routine, "Error adding time series.");
					}
				}
				if ((yts_acgwspr != null) && (ipy.__acgwspr_ts != null))
				{
					try
					{
						TSUtil.add(yts_acgwspr, ipy.__acgwspr_ts);
					}
					catch (Exception)
					{
						Message.printWarning(3, routine, "Error adding time series.");
					}
				}

				if ((yts_tot != null) && (ipy.__tacre_ts != null))
				{
					try
					{
						TSUtil.add(yts_tot, ipy.__tacre_ts);
					}
					catch (Exception)
					{
						Message.printWarning(3, routine, "Error adding total acres time series.");
					}
				}
				try
				{
					TSUtil.add(yts_pump, ipy.__mprate_ts);
				}
				catch (Exception)
				{
					Message.printWarning(3, routine, "Error adding max pumping time series.");
				}
			}
		}
		if (include_dataset_totals)
		{
			// Add the totals to the list...
			// And also do for the version 10+ time series...
			yts_acswfl.setDescription(dataset_location + " " + yts_acswfl.getDataType());
			tslist.Add(yts_acswfl);
			yts_acswspr.setDescription(dataset_location + " " + yts_acswspr.getDataType());
			tslist.Add(yts_acswspr);
			yts_acgwfl.setDescription(dataset_location + " " + yts_acgwfl.getDataType());
			tslist.Add(yts_acgwfl);
			yts_acgwspr.setDescription(dataset_location + " " + yts_acgwspr.getDataType());
			tslist.Add(yts_acgwspr);

			// Do for totals by supply type...
			yts_acsw.setDescription(dataset_location + " " + yts_acsw.getDataType());
			tslist.Add(yts_acsw);
			yts_acgw.setDescription(dataset_location + " " + yts_acgw.getDataType());
				tslist.Add(yts_acgw);

			yts_pump.setDescription(dataset_location + " " + yts_pump.getDataType());
			tslist.Add(yts_pump);
			yts_tot.setDescription(dataset_location + " " + yts_tot.getDataType());
			tslist.Add(yts_tot);
		}
		return tslist;
	}

	/// <summary>
	/// Performs specific data checks and returns a list of data that failed the data checks. </summary>
	/// <param name="count"> Index of the data vector currently being checked. </param>
	/// <param name="dataset"> StateCU dataset currently in memory. </param>
	/// <param name="props"> Extra properties to perform checks with. </param>
	/// <returns> List of invalid data. </returns>
	public virtual StateCU_ComponentValidation validateComponent(StateCU_DataSet dataset)
	{
		StateCU_ComponentValidation validation = new StateCU_ComponentValidation();
		// Get the CDS component if available that matches this IPY.
		// TODO SAM 2017-03-14 the following to get YearTS did not seem correct
		//YearTS cds_yts = null;
		StateCU_CropPatternTS cdsForId = null;
		if (dataset != null)
		{
			DataSetComponent comp = dataset.getComponentForComponentType(StateCU_DataSet.COMP_CROP_PATTERN_TS_YEARLY);
			if (comp != null)
			{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<StateCU_CropPatternTS> cdsList = (java.util.List<StateCU_CropPatternTS>)comp.getData();
				IList<StateCU_CropPatternTS> cdsList = (IList<StateCU_CropPatternTS>)comp.getData();
				if (cdsList != null)
				{
					int pos = StateCU_Util.IndexOf(cdsList, getID());
					if (pos >= 0)
					{
						// Get the CDS total acreage time series
						// TODO SAM 2017-03-14 the following to get YearTS did not seem correct
						//cds_yts = cdsList.get(pos).getTotal()???;
						cdsForId = cdsList[pos];
					}
				}
			}
		}
		string id = getID();
		// Check major issues
		int year1 = __date1.getYear();
		int year2 = __date2.getYear();
		bool problemFound = false;
		if ((year1 <= 0) || (year2 <= 0))
		{
			validation.add(new StateCU_ComponentValidationProblem(this, "Location \"" + id + "\" period for irrigation practice time series is not set.", "Verify that the time series are properly defined."));
			problemFound = true;
		}
		if (!problemFound)
		{
			// Did not find a major problem above so can continue checking time series
			double ceff, feff, seff, acswfl, acswspr, acgwfl, acgwspr, mprate, gmode, tacre, acSum, cds;
			YearTS ceff_yts = getCeffTS(), feff_yts = getFeffTS(), seff_yts = getSeffTS(), acswfl_yts = getAcswflTS(), acswspr_yts = getAcswsprTS(), acgwfl_yts = getAcgwflTS(), acgwspr_yts = getAcgwsprTS(), mprate_yts = getMprateTS(), gmode_yts = getGmodeTS(), tacre_yts = getTacreTS();
			string acSumFormatted, cdsFormatted, tacreFormatted = null;

			DateTime temp_DateTime = new DateTime();
			for (int year = year1; year <= year2; year++)
			{
				temp_DateTime.setYear(year);
				ceff = ceff_yts.getDataValue(temp_DateTime);
				feff = feff_yts.getDataValue(temp_DateTime);
				seff = seff_yts.getDataValue(temp_DateTime);
				acswfl = acswfl_yts.getDataValue(temp_DateTime);
				acswspr = acswspr_yts.getDataValue(temp_DateTime);
				acgwfl = acgwfl_yts.getDataValue(temp_DateTime);
				acgwspr = acgwspr_yts.getDataValue(temp_DateTime);
				mprate = mprate_yts.getDataValue(temp_DateTime);
				gmode = gmode_yts.getDataValue(temp_DateTime);
				int gmodeInt = (int)(gmode + .01);
				tacre = tacre_yts.getDataValue(temp_DateTime);
				if (!((ceff >= 0.0) && (ceff <= 1.0)))
				{
					validation.add(new StateCU_ComponentValidationProblem(this, "Location \"" + id + "\" year " + year + " maximum surface efficiency (" + StringUtil.formatString(ceff,"%.2f") + ") is invalid.", "Verify that the efficiency is in range 0 to 1."));
				}
				if (!((feff >= 0.0) && (feff <= 1.0)))
				{
					validation.add(new StateCU_ComponentValidationProblem(this, "Location \"" + id + "\" year " + year + " maximum flood efficiency (" + StringUtil.formatString(feff,"%.2f") + ") is invalid.", "Verify that the efficiency is in range 0 to 1."));
				}
				if (!((seff >= 0.0) && (seff <= 1.0)))
				{
					validation.add(new StateCU_ComponentValidationProblem(this, "Location \"" + id + "\" year " + year + " maximum sprinkler efficiency (" + StringUtil.formatString(seff,"%.2f") + ") is invalid.", "Verify that the efficiency is in range 0 to 1."));
				}
				if (!(acswfl >= 0.0))
				{
					validation.add(new StateCU_ComponentValidationProblem(this, "Location \"" + id + "\" year " + year + " acres surface flood (" + StringUtil.formatString(acswfl,"%.1f") + ") is invalid.", "Verify that the acres value is >= 0."));
					problemFound = true;
				}
				if (!(acswspr >= 0.0))
				{
					validation.add(new StateCU_ComponentValidationProblem(this, "Location \"" + id + "\" year " + year + " acres surface sprinkler (" + StringUtil.formatString(acswspr,"%.1f") + ") is invalid.", "Verify that the acres value is >= 0."));
					problemFound = true;
				}
				if (!(acgwfl >= 0.0))
				{
					validation.add(new StateCU_ComponentValidationProblem(this, "Location \"" + id + "\" year " + year + " acres groundwater flood (" + StringUtil.formatString(acgwfl,"%.1f") + ") is invalid.", "Verify that the acres value is >= 0."));
					problemFound = true;
				}
				if (!(acgwspr >= 0.0))
				{
					validation.add(new StateCU_ComponentValidationProblem(this, "Location \"" + id + "\" year " + year + " acres groundwater sprinkler (" + StringUtil.formatString(acgwspr,"%.1f") + ") is invalid.", "Verify that the acres value is >= 0."));
					problemFound = true;
				}
				if (!(mprate >= 0.0))
				{
					validation.add(new StateCU_ComponentValidationProblem(this, "Location \"" + id + "\" year " + year + " maximum pumping rate (" + StringUtil.formatString(mprate,"%.1f") + ") is invalid.", "Verify that the maximum pumping value is >= 0."));
				}
				if (!((gmodeInt >= 1) && (gmodeInt <= 3)))
				{
					validation.add(new StateCU_ComponentValidationProblem(this, "Location \"" + id + "\" year " + year + " groundwater mode (" + gmodeInt + ") is invalid.", "Verify that the groundwater mode is in range 1 to 3."));
				}
				if (!(tacre >= 0.0))
				{
					validation.add(new StateCU_ComponentValidationProblem(this, "Location \"" + id + "\" year " + year + " total acres (" + StringUtil.formatString(tacre,"%.1f") + ") is invalid.", "Verify that the total acres value is >= 0."));
				}
				//if ( !problemFound || (cds_yts != null) ) {
				if (!problemFound || (cdsForId != null))
				{
					// Need the following...
					tacreFormatted = StringUtil.formatString(tacre,"%.1f");
				}
				if (!problemFound)
				{
					// Verify that the acreage totals add to the overall total
					acSum = (acswfl + acswspr + acgwfl + acgwspr);
					acSumFormatted = StringUtil.formatString(acSum,"%.1f");
					if (!acSumFormatted.Equals(tacreFormatted))
					{
						validation.add(new StateCU_ComponentValidationProblem(this, "Location \"" + id + "\" year " + year + " total acres (" + tacreFormatted + ") does not match total of acreage parts (" + acSumFormatted + ").", "Verify that commands to set parts are consistent with total."));
					}
				}
				// Verify that the totals agree to .1 with the CDS total since .1 precision is what the
				// IPY file acreage is written to
				if (cdsForId != null)
				{
					// TODO SAM 2017-03-14 the following does not seem correct
					//cds = cds_yts.getDataValue(temp_DateTime);
					cds = cdsForId.getTotalArea(year);
					cdsFormatted = StringUtil.formatString(cds,"%.1f");
					if (!cdsFormatted.Equals(tacreFormatted))
					{
						validation.add(new StateCU_ComponentValidationProblem(this, "Location \"" + id + "\" year " + year + " total acres (" + tacreFormatted + ") does not match crop pattern time series total acreage (" + cdsFormatted + ").", "Verify that irrigation practice time series total acreage is set to the crop pattern " + "time series total with SetIrrigationPracticeTSTotalAcreateToCropPatternTSTotalAcreage() " + "command before other acreage commands."));
					}
				}
			}
		}
		// TODO SAM 2009-05-11 Evaluate whether need check for zero values for whole period
		return validation;
	}

	/// <summary>
	/// Write a list of StateCU_IrrigationPracticeTS to a StateCU file.  The filename
	/// is adjusted to the working directory if necessary using IOUtil.getPathUsingWorkingDir(). </summary>
	/// <param name="filename_prev"> The name of the previous version of the file (for
	/// processing headers).  Specify as null if no previous file is available. </param>
	/// <param name="filename"> The name of the file to write. </param>
	/// <param name="data_List"> A list of StateCU_IrrigationPracticeTS to write. </param>
	/// <param name="new_comments"> Comments to add to the top of the file.  Specify as null if no comments are available. </param>
	/// <exception cref="IOException"> if there is an error writing the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeStateCUFile(String filename_prev, String filename, java.util.List<StateCU_IrrigationPracticeTS> data_List, java.util.List<String> new_comments) throws java.io.IOException
	public static void writeStateCUFile(string filename_prev, string filename, IList<StateCU_IrrigationPracticeTS> data_List, IList<string> new_comments)
	{
		writeStateCUFile(filename_prev, filename, data_List, new_comments, null);
	}

	/// <summary>
	/// Write a list of StateCU_IrrigationPracticeTS to a StateCU file.  The filename
	/// is adjusted to the working directory if necessary using IOUtil.getPathUsingWorkingDir(). </summary>
	/// <param name="filename_prev"> The name of the previous version of the file (for
	/// processing headers).  Specify as null if no previous file is available. </param>
	/// <param name="filename"> The name of the file to write. </param>
	/// <param name="data_List"> A list of StateCU_IrrigationPracticeTS to write. </param>
	/// <param name="new_comments"> Comments to add to the top of the file.  Specify as null 
	/// if no comments are available. </param>
	/// <param name="start"> DateTime to start output. </param>
	/// <param name="end"> DateTime to end output. </param>
	/// <exception cref="IOException"> if there is an error writing the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeStateCUFile(String filename_prev, String filename, java.util.List<StateCU_IrrigationPracticeTS> data_List, java.util.List<String> new_comments, RTi.Util.Time.DateTime start, RTi.Util.Time.DateTime end, RTi.Util.IO.PropList props) throws java.io.IOException
	public static void writeStateCUFile(string filename_prev, string filename, IList<StateCU_IrrigationPracticeTS> data_List, IList<string> new_comments, DateTime start, DateTime end, PropList props)
	{
		IList<string> commentStr = new List<string>(1);
		commentStr.Add("#");
		IList<string> ignoreCommentStr = new List<string>(1);
		ignoreCommentStr.Add("#>");
		PrintWriter @out = null;
		string full_filename_prev = IOUtil.getPathUsingWorkingDir(filename_prev);
		string full_filename = IOUtil.getPathUsingWorkingDir(filename);
		@out = IOUtil.processFileHeaders(full_filename_prev, full_filename, new_comments, commentStr, ignoreCommentStr, 0);
		if (@out == null)
		{
			throw new IOException("Error writing to \"" + full_filename + "\"");
		}
		writeList(data_List, @out, start, end, props);
		@out.flush();
		@out.close();
	}

	/// <summary>
	/// Write a list of StateCU_IrrigationPracticeTS to a StateCU file.  The filename
	/// is adjusted to the working directory if necessary using IOUtil.getPathUsingWorkingDir(). </summary>
	/// <param name="filename_prev"> The name of the previous version of the file (for
	/// processing headers).  Specify as null if no previous file is available. </param>
	/// <param name="filename"> The name of the file to write. </param>
	/// <param name="data_List"> A list of StateCU_IrrigationPracticeTS to write. </param>
	/// <param name="new_comments"> Comments to add to the top of the file.  Specify as null if no comments are available. </param>
	/// <exception cref="IOException"> if there is an error writing the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeStateCUFile(String filename_prev, String filename, java.util.List<StateCU_IrrigationPracticeTS> data_List, java.util.List<String> new_comments, RTi.Util.IO.PropList props) throws java.io.IOException
	public static void writeStateCUFile(string filename_prev, string filename, IList<StateCU_IrrigationPracticeTS> data_List, IList<string> new_comments, PropList props)
	{
		writeStateCUFile(filename_prev, filename, data_List, new_comments, null, null, props);
	}

	/// <summary>
	/// Write a list of StateCU_IrrigationPracticeTS to a DateValue file.  The
	/// filename is adjusted to the working directory if necessary using IOUtil.getPathUsingWorkingDir(). </summary>
	/// <param name="filename_prev"> The name of the previous version of the file (for
	/// processing headers).  Specify as null if no previous file is available. </param>
	/// <param name="filename"> The name of the file to write. </param>
	/// <param name="dataList"> A list of StateCU_IrrigationPracticeTS to write. </param>
	/// <param name="new_comments"> Comments to add to the top of the file.  Specify as null if no comments are available. </param>
	/// <exception cref="IOException"> if there is an error writing the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeDateValueFile(String filename_prev, String filename, java.util.List<StateCU_IrrigationPracticeTS> dataList, java.util.List<String> new_comments) throws Exception
	public static void writeDateValueFile(string filename_prev, string filename, IList<StateCU_IrrigationPracticeTS> dataList, IList<string> new_comments)
	{ // For now ignore the previous file and new comments.
		// Create a new Vector with the time series data...
		IList<TS> tslist = toTSList(dataList);
		// Now write using a standard DateValueTS call...
		string full_filename = IOUtil.getPathUsingWorkingDir(filename);
		DateValueTS.writeTimeSeriesList(tslist, full_filename);
	}

	/// <summary>
	/// Write a list of StateCU_IrrigationPracticeTS to an opened file. </summary>
	/// <param name="data_List"> A list of StateCU_IrrigationPracticeTS to write. </param>
	/// <param name="out"> output PrintWriter. </param>
	/// <param name="start"> the DateTime for the start of output. </param>
	/// <param name="end"> the DateTime for the end of output. </param>
	/// <param name="props"> Properties that control output.  Version=10 will write the old
	/// format.  RecomputeVersion10Acreage=True|False indicates whether version 10 acreage
	/// should be recomputed from the parts (default=true).  PrecisionArea indicates the
	/// precision for area formatting. </param>
	/// <exception cref="IOException"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static void writeList(java.util.List<StateCU_IrrigationPracticeTS> data_List, java.io.PrintWriter out, RTi.Util.Time.DateTime start, RTi.Util.Time.DateTime end, RTi.Util.IO.PropList props) throws java.io.IOException
	private static void writeList(IList<StateCU_IrrigationPracticeTS> data_List, PrintWriter @out, DateTime start, DateTime end, PropList props)
	{
		int i;
		string iline;
		string cmnt = "#>";

		// Check proplist for version
		if (props == null)
		{
			props = new PropList("StateCU_IrrigationPracticeTS");
		}
		// Set the format to version 10 format
		string Version = props.getValue("Version");
		bool version10 = false;
		if ((!string.ReferenceEquals(Version, null)) && Version.Equals("10", StringComparison.OrdinalIgnoreCase))
		{
			version10 = true;
		}
		// Indicate whether version 10 groundwater and sprinkler acreage should
		// be computed from the more detailed Version 12+ acreage columns.
		//String RecomputeVersion10Acreage = props.getValue ( "RecomputeVersion10Acreage" );
		bool RecomputeVersion10Acreage_boolean = true;
		/* FIXME SAM 2007-10-18 Remove later when tested out
		 * Always recompute now
		if ( (RecomputeVersion10Acreage != null) && RecomputeVersion10Acreage.equalsIgnoreCase("False") ) {
			RecomputeVersion10Acreage_boolean = false;
		}
		*/
		// Indicate the precision to use for area output.
		string PrecisionForArea = props.getValue("PrecisionForArea");
		int PrecisionForArea_int = 0;
		if ((!string.ReferenceEquals(PrecisionForArea, null)) && (PrecisionForArea.Length != 0) && StringUtil.isInteger(PrecisionForArea))
		{
			PrecisionForArea_int = StringUtil.atoi(PrecisionForArea);
		}

		// Missing data handled by formatting all as strings...
		string format0 = "%4.4s %-12.12s%6.6s%6.6s%6.6s%8.8s%8.8s%8.8s%8.8s%12.12s%3.3s%8.8s%8.8s%8.8s";
		string format_MaxEfficiency = "%6.2f";
		string header_format = "(6x,i4,11x,i4,7x,a3)";
		string record_format = "(i4,1x,a12,3(f6.2),4(f8." + PrecisionForArea_int + "),f12.0,i3,f8." + PrecisionForArea_int + ",2(f8." + PrecisionForArea_int + "))";
		string area_format0 = "%8.0f"; // For big numbers, regardless of precision
		string area_format = "%8." + PrecisionForArea_int + "f";
		string header0 = "                 Max  Efficiency";
		string header1 = "Yr  CULocation   Surf Flood   Spr AcSWFl  AcSWSpr " +
			"AcGWFl  AcGWSpr PumpingMax GMode AcTot  AcSW    AcGW  ";
		// Update the formats if the version is 10
		if (version10)
		{
			format0 = "%4.4s %-12.12s  %4.4s  %4.4s  %4.4s%8.8s%8.8s%12.12s%3.3s%8.8s";
			format_MaxEfficiency = "%4.2f";
			record_format = "(i4,1x,a12,3(2x,f4.2),2(i8),i12,i3,f8.0)";
			header_format = "(i5,1x,i4,5x,i5,1x,i4,a5,a5)";
			header0 = "                 Max  Efficiency";
			header1 = "Yr  CULocation   Surf  Flood Spr  AcGW   AcSprnk PumpingMax GMode  AcTot";
		}

		IList<object> v = new List<object>(11); // Reuse for all output lines.

		@out.println(cmnt);
		@out.println(cmnt + "  StateCU Irrigation Practice Time Series (IPY) File");
		@out.println(cmnt);
		@out.println(cmnt + "  Header format " + header_format);
		@out.println(cmnt);
		if (version10)
		{
			@out.println(cmnt + "  month1           :  Beginning month of data (always 1).");
		}
		@out.println(cmnt + "  year1            :  Beginning year of data (calendar year).");
		if (version10)
		{
			@out.println(cmnt + "  month2           :  Ending month of data (always 12).");
		}
		@out.println(cmnt + "  year2            :  Ending year of data (calendar year).");
		if (version10)
		{
			@out.println(cmnt + "  units            :  Data units for acreages.");
		}
		@out.println(cmnt + "  yeartype         :  Year type (always CYR for calendar).");
		@out.println(cmnt);
		@out.println(cmnt + "  Record format " + record_format + " - each year/CULocation.");
		@out.println(cmnt);
		@out.println(cmnt + "  Yr             yr:  Year for data (calendar year).");
		@out.println(cmnt + "  CULocation  aspid:  CU Location ID (e.g., structure/station).");
		@out.println(cmnt + "  Surf         ceff:  Maximum efficiency for delivering");
		@out.println(cmnt + "                      surface water supply to the farm");
		@out.println(cmnt + "                      headgage (fraction).");
		@out.println(cmnt + "  Flood        feff:  Maximum application efficiency for");
		@out.println(cmnt + "                      flood irrigation (fraction).");
		@out.println(cmnt + "  Spr          seff:  Maximum application efficiency for");
		@out.println(cmnt + "                      sprinkler irrigation (fraction).");
		if (version10)
		{
			@out.println(cmnt + "  AcGW        gacre:  Acres with ground water supply.");
			@out.println(cmnt + "  AcSprnk     sacre:  Acres irrigated by sprinkler application.");
		}
		else
		{
			@out.println(cmnt + "  AcSwFl      acswfl: Acres with surface water only supply, flood.");
			@out.println(cmnt + "  AcSwSpr     acswspr:Acres with surface water only supply, sprinkler.");
			@out.println(cmnt + "  AcGwFl      acgwfl: Acres with groundwater supply, flood.");
			@out.println(cmnt + "  AcGwSpr     acgwspr:Acres with groundwater supply, sprinkler.");
		}
		@out.println(cmnt + "  PumpingMax mprate:  Maximum pumping volume (AF per month).");
		@out.println(cmnt + "  GMode       gmode:  Ground water use mode.");
		@out.println(cmnt + "                      1=surface and GW are used to maximize supply.");
		@out.println(cmnt + "                      2=surface water is used 1st on all");
		@out.println(cmnt + "                      acreage, and then GW.");
		@out.println(cmnt + "                      3=GW is used first on sprinkler");
		@out.println(cmnt + "                      acreage and surface water shares for");
		@out.println(cmnt + "                      the same acreage are available for recharge.");
		@out.println(cmnt + "  AcTot       tacre:  Total acres irrigated - agrees with");
		@out.println(cmnt + "                      crop pattern time series file (CDS).");
		if (!version10)
		{
			// Newer put the supply totals on the far right for information only.
			@out.println(cmnt + "  AcSW       gwacre:  Acres with surface water only supply (not read by StateCU).");
			@out.println(cmnt + "  AcGW       swacre:  Acres with ground water supply (not read by StateCU). ");
		}
		@out.println(cmnt);
		@out.println(cmnt + header0);
		@out.println(cmnt + header1);
		if (version10)
		{
			@out.println(cmnt + "-exb----------exxb--exxb--exxb--eb------eb------eb----------eb-eb------e");
		}
		else
		{
			// New format...
			@out.println(cmnt + "-exb----------eb----eb----eb----eb------eb------eb------eb------eb----------eb-eb------eb------eb------e");
		}
		@out.println(cmnt + "EndHeader");

		// Write the header...
		// The dates by default are taken from the first object and are assumed to be
		// consistent between objects...
		StateCU_IrrigationPracticeTS tsp = (StateCU_IrrigationPracticeTS)data_List[0];
		DateTime date1 = tsp.getDate1();
		DateTime date2 = tsp.getDate2();
		if (start != null)
		{
			date1 = new DateTime(start);
		}
		if (end != null)
		{
			date2 = new DateTime(end);
		}

		if (version10)
		{
			@out.println("    1/" + StringUtil.formatString(date1.getYear(),"%04d") + "        12/" + StringUtil.formatString(date2.getYear(),"%04d") + StringUtil.formatString("ACRE","%5.5s") + StringUtil.formatString("CYR","%5.5s"));
		}
		else
		{
			@out.println("      " + StringUtil.formatString(date1.getYear(),"%04d") + "           " + StringUtil.formatString(date2.getYear(),"%04d") + "       " + StringUtil.formatString("CYR","%3.3s"));
		}

		// Write the data...

		int num = 0;
		if (data_List != null)
		{
			num = data_List.Count;
		}
		if (num == 0)
		{
			return;
		}
		DateTime date = new DateTime(date1);
		DateTime temp_DateTime = new DateTime(); // Use for data access.
		int year = 0;
		double val, acgw_val, acgwfl_val, acgwspr_val, acsw_val, acswfl_val, acswspr_val;
		double area_big = 1000000.0; // Needs to use lower precision output
		YearTS ceff_yts, feff_yts, seff_yts, mprate_yts, gmode_yts, tacre_yts, acgw_yts, acgwfl_yts, acgwspr_yts, acsw_yts, acswfl_yts, acswspr_yts;
		// This is not real efficient but is relatively fast...
		// Outer loop is for the time series period...
		for (; date.lessThanOrEqualTo(date2); date.addYear(1))
		{
			year = date.getYear();
			temp_DateTime.setYear(year);
			// Inner loop is for each CULocation
			for (i = 0; i < num; i++)
			{
				tsp = data_List[i];
				if (tsp == null)
				{
					continue;
				}
				v.Clear();
				v.Add(StringUtil.formatString(date.getYear(),"%4d"));
				v.Add(tsp._id);
				ceff_yts = tsp.getCeffTS();
				val = ceff_yts.getDataValue(temp_DateTime);
				v.Add(StringUtil.formatString(val, format_MaxEfficiency));
				feff_yts = tsp.getFeffTS();
				val = feff_yts.getDataValue(temp_DateTime);
				v.Add(StringUtil.formatString(val, format_MaxEfficiency));
				seff_yts = tsp.getSeffTS();
				val = seff_yts.getDataValue(temp_DateTime);
				v.Add(StringUtil.formatString(val, format_MaxEfficiency));

				if (version10)
				{
					// FIXME SAM 2007-10-18 Remove later when tested out gacre_yts = tsp.getGacreTS();
					if (RecomputeVersion10Acreage_boolean)
					{
						acgwfl_yts = tsp.getAcgwflTS();
						acgwfl_val = acgwfl_yts.getDataValue(temp_DateTime);
						acgwspr_yts = tsp.getAcgwsprTS();
						acgwspr_val = acgwspr_yts.getDataValue(temp_DateTime);
						if ((acgwfl_val < 0.0) || (acgwspr_val < 0.0))
						{
							val = -999.0;
						}
						else
						{
							val = acgwfl_val + acgwspr_val;
						}
					}
					v.Add(StringUtil.formatString(val,area_format));
					if (RecomputeVersion10Acreage_boolean)
					{
						acswspr_yts = tsp.getAcswsprTS();
						acswspr_val = acswspr_yts.getDataValue(temp_DateTime);
						acgwspr_yts = tsp.getAcgwsprTS();
						acgwspr_val = acgwspr_yts.getDataValue(temp_DateTime);
						if ((acswspr_val < 0.0) || (acgwspr_val < 0.0))
						{
							val = -999.0;
						}
						else
						{
							val = acswspr_val + acgwspr_val;
						}
					}
					v.Add(StringUtil.formatString(val,area_format));
				}
				else
				{
					// add the new land acreage
					acswfl_yts = tsp.getAcswflTS();
					acswfl_val = acswfl_yts.getDataValue(temp_DateTime);
					if (acswfl_val >= area_big)
					{
						v.Add(StringUtil.formatString(acswfl_val,area_format0));
					}
					else
					{
						v.Add(StringUtil.formatString(acswfl_val,area_format));
					}

					acswspr_yts = tsp.getAcswsprTS();
					acswspr_val = acswspr_yts.getDataValue(temp_DateTime);
					if (acswspr_val >= area_big)
					{
						v.Add(StringUtil.formatString(acswspr_val,area_format0));
					}
					else
					{
						v.Add(StringUtil.formatString(acswspr_val,area_format));
					}

					acgwfl_yts = tsp.getAcgwflTS();
					acgwfl_val = acgwfl_yts.getDataValue(temp_DateTime);
					if (acgwfl_val >= area_big)
					{
						v.Add(StringUtil.formatString(acgwfl_val,area_format0));
					}
					else
					{
						v.Add(StringUtil.formatString(acgwfl_val,area_format));
					}

					acgwspr_yts = tsp.getAcgwsprTS();
					acgwspr_val = acgwspr_yts.getDataValue(temp_DateTime);
					if (acgwspr_val >= area_big)
					{
						v.Add(StringUtil.formatString(acgwspr_val,area_format0));
					}
					else
					{
						v.Add(StringUtil.formatString(acgwspr_val,area_format));
					}
				}
				mprate_yts = tsp.getMprateTS();
				val = mprate_yts.getDataValue(temp_DateTime);
				v.Add(StringUtil.formatString(val,"%12.0f"));
				gmode_yts = tsp.getGmodeTS();
				val = gmode_yts.getDataValue(temp_DateTime);
				v.Add(StringUtil.formatString((int)(val + .1),"%3d"));
				tacre_yts = tsp.getTacreTS();
				val = tacre_yts.getDataValue(temp_DateTime);
				if (val >= area_big)
				{
					v.Add(StringUtil.formatString(val,area_format0));
				}
				else
				{
					v.Add(StringUtil.formatString(val,area_format));
				}
				if (!version10)
				{
					// Add the new supply type acreage
					acsw_yts = tsp.getAcswTS();
					acsw_val = acsw_yts.getDataValue(temp_DateTime);
					if (acsw_val >= area_big)
					{
						v.Add(StringUtil.formatString(acsw_val,area_format0));
					}
					else
					{
						v.Add(StringUtil.formatString(acsw_val,area_format));
					}
					// Add the new supply type acreage
					acgw_yts = tsp.getAcgwTS();
					acgw_val = acgw_yts.getDataValue(temp_DateTime);
					if (acgw_val >= area_big)
					{
						v.Add(StringUtil.formatString(acgw_val,area_format0));
					}
					else
					{
						v.Add(StringUtil.formatString(acgw_val,area_format));
					}
				}
				iline = StringUtil.formatString(v, format0);
				@out.println(iline);
			}
		}
	}

	}

}