using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

// StateCU_Location - class to hold StateCU Location data, compatible with StateCU STR file

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
// StateCU_Location - class to hold StateCU Location data, compatible with
//			StateCU STR file
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
//
// 2002-09-16	J. Thomas Sapienza, RTi	Initial version. 
// 2002-09-19	JTS, RTi		Region2 changed from int to String
// 2002-09-23	JTS, RTi		Added toStringForSTRFile()
// 2002-10-08	JTS, RTi		Added "filled"
// 2002-10-09	JTS, RTi		Formatting was SLIGHTLY off.
// 2002-11-06	Steven A. Malers, RTi	Review code for official software
//					release:
//					* simplify names of data members.
//					* rely on CUData base class for some
//					  data and behavior.
//					* remove code related to "fill".
//					* add writeSTRFile to streamline output.
// 2002-05-12	SAM, RTi		Change STR to StateCU in read/write
//					methods.	
// 2003-06-04	SAM, RTi		Change name of class from CULocation to
//					StateCU_Location.
// 2003-07-01	SAM, RTi		* Support the new format.  The format in
//					  the old documentation will not be
//					  supported.
// 2004-02-25	SAM, RTi		* Finalize new format based on StateCU
//					  4.35 release.
//					* Add a Vector to store aggregate
//					  information.  For now only store the
//					  identifiers but it may make sense in
//					  the future to store objects.
//					* Allow climate station data to be
//					  set dynamically because of resets.
// 2004-02-27	SAM, RTi		* Fix read code to handle climate
//					  stations on 2nd+ lines.
// 2004-02-29	SAM, RTi		* Change so setting the aggregate list
//					  also sets a date.
//					* Add isCollection().
// 2004-03-01	SAM, RTi		* Change "aggregate" to "collection"
//					  since aggregate and system are the
//					  collection types.  Using aggregate was
//					  becoming confusing.
//					* Parcel identifiers are not unique
//					  unless the division is included so add
//					  the division for the collection.  It
//					  should not vary by year.
// 2004-04-04	SAM, RTi		* Fix some justification problems in the
//					  output of numerical fields.
// 2005-01-17	J. Thomas Sapienza, RTi	* Added createBackup().
//					* Added restoreOriginal().
// 2005-03-07	SAM, RTi		* Add getCollectionType().
// 2005-03-29	SAM, RTi		* Add Elevation label to header - must
//					  have been an oversight.
// 2005-04-19	JTS, RTi		* Added writeListFile().
//   					* Added writeClimateStationListFile().
// 2005-05-24	SAM, RTi		* Update writeStateCUFile() to include
//					  properties, to allow old and new
//					  versions to be written.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
// 2007-05-11	SAM, RTi		Add hasGroundwaterOnlySupply() and
//					  hasSurfaceWaterSupply().
//------------------------------------------------------------------------------
// EndHeader

namespace DWR.StateCU
{

	using IOUtil = RTi.Util.IO.IOUtil;
	using PropList = RTi.Util.IO.PropList;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// Class to hold StateCU Location data for StateCU/StateDMI, compatible with the StateCU STR file.
	/// </summary>
	public class StateCU_Location : StateCU_Data, StateCU_ComponentValidator
	{

	/// <summary>
	/// Types of collections.  An aggregate merges the water rights whereas
	/// a system keeps all the water rights but just has one ID.
	/// </summary>
	public static string COLLECTION_TYPE_AGGREGATE = "Aggregate";
	public static string COLLECTION_TYPE_SYSTEM = "System";

	/// <summary>
	/// Types of collection parts, either Ditch or Parcel
	/// </summary>
	public static string COLLECTION_PART_TYPE_DITCH = "Ditch";
	public static string COLLECTION_PART_TYPE_PARCEL = "Parcel";
	public static string COLLECTION_PART_TYPE_WELL = "WEll";

	private string __collection_type = StateCU_Util.MISSING_STRING;

	/// <summary>
	/// Collection part type (see COLLECTION_PART_TYPE_*), used by DMI software.
	/// </summary>
	private string __collection_part_type = StateCU_Util.MISSING_STRING;

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
	/// An array of years that correspond to the aggregate/system.  Parcel
	/// collections can have multiple years but ditches currently only have one year.
	/// </summary>
	private int[] __collectionYear = null;

	/// <summary>
	/// The division that corresponds to the aggregate/system.  Currently
	/// it is expected that the same division number is assigned to all the data.
	/// </summary>
	private int __collection_div = StateCU_Util.MISSING_INT;


	/// <summary>
	/// CULocation Elevation.
	/// </summary>
	private double __elevation = StateCU_Util.MISSING_DOUBLE;

	/// <summary>
	/// CULocation Latitude.
	/// </summary>
	private double __latitude = StateCU_Util.MISSING_DOUBLE;

	/// <summary>
	/// Region 1 (e.g., County).
	/// </summary>
	private string __region1 = StateCU_Util.MISSING_STRING;

	/// <summary>
	/// Region 2 (e.g., HUC).
	/// </summary>
	private string __region2 = StateCU_Util.MISSING_STRING;

	/// <summary>
	/// Available water content (AWC).
	/// </summary>
	private double __awc = StateCU_Util.MISSING_DOUBLE;

	/// <summary>
	/// Orographic temperature adjustment (DEGF/1000 FT) - set to 0.0 because this
	/// means no adjustment.  The data will be reset (not filled) if needed.  The size
	/// is the number of climate stations.
	/// </summary>
	private double[] __ota;

	/// <summary>
	/// Orographic precipitation adjustment (fraction) - set to 1.0 because this
	/// means no adjustment.  The data will be reset (not filled) if needed.  The size
	/// is the number of climate stations.
	/// </summary>
	private double[] __opa;

	/// <summary>
	/// Number of stations.
	/// </summary>
	private string[] __climate_station_ids = null;
	private double[] __precipitation_station_weights = null;
	private double[] __temperature_station_weights = null;

	/// <summary>
	/// Construct a StateCU_Location instance and set to missing and empty data.
	/// </summary>
	public StateCU_Location() : base()
	{
	}

	/// <summary>
	/// Creates a backup of the current data object and stores it in _original,
	/// for use in determining if an object was changed inside of a GUI.
	/// </summary>
	public virtual void createBackup()
	{
		_original = clone();
		((StateCU_Location)_original)._isClone = false;
		_isClone = true;
	}

	/// <summary>
	/// Return the AWC. </summary>
	/// <returns> the AWC. </returns>
	public virtual double getAwc()
	{
		return __awc;
	}

	/// <summary>
	/// Return the AWC. </summary>
	/// <returns> the AWC. </returns>
	public virtual double getAWC()
	{
		return __awc;
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
	/// Return the collection part ID list for the specific year.  For ditches, only one
	/// aggregate/system list is currently supported so the same information is returned
	/// regardless of the year value.  For wells, the collection is done for a specific year. </summary>
	/// <param name="year"> The year of interest, only used for well identifiers when collection is specified with parcels. </param>
	/// <returns> the list of collection part IDS, or null if not defined. </returns>
	public virtual IList<string> getCollectionPartIDsForYear(int year)
	{
		if ((__collectionIDList == null) || (__collectionIDList.Count == 0))
		{
			return null;
		}
		if (__collection_part_type.Equals(COLLECTION_PART_TYPE_DITCH, StringComparison.OrdinalIgnoreCase))
		{
			// The list of part IDs will be the first and only list (same for all years)...
			return __collectionIDList[0];
		}
		else if (__collection_part_type.Equals(COLLECTION_PART_TYPE_WELL, StringComparison.OrdinalIgnoreCase))
		{
			// The list of part IDs will be the first and only list (same for all years)...
			return __collectionIDList[0];
		}
		else if (__collection_part_type.Equals(COLLECTION_PART_TYPE_PARCEL, StringComparison.OrdinalIgnoreCase))
		{
			// The list of part IDs needs to match the year.
			for (int i = 0; i < __collectionYear.Length; i++)
			{
				if (year == __collectionYear[i])
				{
					return __collectionIDList[i];
				}
			}
		}
		return null;
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
	/// Return the collection part type, COLLECTION_PART_TYPE_DITCH or COLLECTION_PART_TYPE_PARCEL.
	/// </summary>
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
	/// Return the array of years for the defined collections. </summary>
	/// <returns> the array of years for the defined collections. </returns>
	public virtual int [] getCollectionYears()
	{
		return __collectionYear;
	}

	/// <summary>
	/// Returns the data column header for the specifically checked data. </summary>
	/// <returns> Data column header. </returns>
	public static string[] getDataHeader()
	{
		// TODO KAT 2007-04-12 
		// When specific checks are added to checkComponentData
		// return the header for that data here
		return new string[] {};
	}

	/// <summary>
	/// Get the climate station identifier. </summary>
	/// <returns> Climate station identifier or "" if not available. </returns>
	/// <param name="pos"> Station index (relative to zero). </param>
	public virtual string getClimateStationID(int pos)
	{
		if (__climate_station_ids == null)
		{
			return "";
		}
		if ((pos >= 0) && (pos < __climate_station_ids.Length))
		{
			return __climate_station_ids[pos];
		}
		else
		{
			return "";
		}
	}

	/// <summary>
	/// Return the elevation. </summary>
	/// <returns> the elevation. </returns>
	public virtual double getElevation()
	{
		return __elevation;
	}

	/// <summary>
	/// Return the latitude. </summary>
	/// <returns> the latitude. </returns>
	public virtual double getLatitude()
	{
		return __latitude;
	}

	/// <summary>
	/// Return the number of climate stations. </summary>
	/// <returns> the number of climate stations. </returns>
	public virtual int getNumClimateStations()
	{
		if (__climate_station_ids == null)
		{
			return 0;
		}
		else
		{
			return __climate_station_ids.Length;
		}
	}

	/// <summary>
	/// Return the orographic precipitation adjustment factor. </summary>
	/// <param name="pos"> Index (0+) for climate (precipitation) station. </param>
	/// <returns> the orographic precipitation adjustment factor. </returns>
	public virtual double getOrographicPrecipitationAdjustment(int pos)
	{
		if ((__opa == null) || (pos >= __opa.Length))
		{
			return StateCU_Util.MISSING_DOUBLE;
		}
		else
		{
			return __opa[pos];
		}
	}

	/// <summary>
	/// Return the orographic temperature adjustment factor. </summary>
	/// <param name="pos"> Index (0+) for climate (temperature) station. </param>
	/// <returns> the orographic temperature adjustment factor. </returns>
	public virtual double getOrographicTemperatureAdjustment(int pos)
	{
		if ((__ota == null) || (pos >= __ota.Length))
		{
			return StateCU_Util.MISSING_DOUBLE;
		}
		else
		{
			return __ota[pos];
		}
	}

	/// <summary>
	/// Return the precipitation station weight. </summary>
	/// <param name="pos"> Index (0+) for climate (precipitation) station. </param>
	/// <returns> the precipitation station weight. </returns>
	public virtual double getPrecipitationStationWeight(int pos)
	{
		if ((__precipitation_station_weights == null) || (pos >= __precipitation_station_weights.Length))
		{
			return StateCU_Util.MISSING_DOUBLE;
		}
		else
		{
			return __precipitation_station_weights[pos];
		}
	}

	/// <summary>
	/// Return the temperature station weight. </summary>
	/// <param name="pos"> Index (0+) for climate (temperature) station. </param>
	/// <returns> the temperature station weight. </returns>
	public virtual double getTemperatureStationWeight(int pos)
	{
		if ((__temperature_station_weights == null) || (pos >= __temperature_station_weights.Length))
		{
			return StateCU_Util.MISSING_DOUBLE;
		}
		else
		{
			return __temperature_station_weights[pos];
		}
	}

	/// <summary>
	/// Return region 1. </summary>
	/// <returns> region 1. </returns>
	public virtual string getRegion1()
	{
		return __region1;
	}

	/// <summary>
	/// Return region 2. </summary>
	/// <returns> region 2. </returns>
	public virtual string getRegion2()
	{
		return __region2;
	}

	public virtual IList<IList<string>> getTemp()
	{
		return __collectionIDList;
	}

	/// <summary>
	/// Indicate whether the CU Location has groundwater only supply.  This will
	/// be the case if the location is a collection with part type of "Parcel".
	/// </summary>
	public virtual bool hasGroundwaterOnlySupply()
	{
		string collectionPartType = getCollectionPartType();
		if (isCollection() && (collectionPartType.Equals(COLLECTION_PART_TYPE_PARCEL, StringComparison.OrdinalIgnoreCase) || collectionPartType.Equals(COLLECTION_PART_TYPE_WELL, StringComparison.OrdinalIgnoreCase)))
		{
			// TODO SAM 2007-05-11 Rectify part types with StateMod
			return true;
		}
		return false;
	}

	/// <summary>
	/// Indicate whether the CU Location has surface water supply.  This will
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
	/// Indicate whether the CU Location is a collection (an aggregate or system). </summary>
	/// <returns> true if the CU Location is an aggregate or system. </returns>
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
	/// Read the StateCU STR file and return as a Vector of StateCU_Location. </summary>
	/// <param name="filename"> filename containing STR data. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.List<StateCU_Location> readStateCUFile(String filename) throws java.io.IOException
	public static IList<StateCU_Location> readStateCUFile(string filename)
	{
		string rtn = "StateCU_Location.readStateCUFile";
		string iline = null;
		IList<object> v = new List<object>(8);
		IList<StateCU_Location> culoc_List = new List<StateCU_Location>();
		int i;
		int[] format_0 = new int[] {StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_SPACE, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_SPACE, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING};
		int[] format_0w = new int[] {12, 6, 9, 2, 20, 8, 2, 24, 4, 8};
		int[] format_1 = new int[] {StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING};
		int[] format_1w = new int[] {12, 6, 9, 9, 9};

		StateCU_Location culoc = null;
		StreamReader @in = null;
		Message.printStatus(1, rtn, "Reading StateCU Locations file: " + filename);

		// The following throws an IOException if the file cannot be opened...
		@in = new StreamReader(filename);
		string latitude, elevation, num_climate_stations, awc, weight, opa, ota;
		int ncli = 0;
		int vsize; // Size of parsed token list
		while (!string.ReferenceEquals((iline = @in.ReadLine()), null))
		{
			// check for comments
			if (iline.StartsWith("#", StringComparison.Ordinal) || iline.Trim().Length == 0)
			{
				continue;
			}

			// Allocate new CULocation instance...
			culoc = new StateCU_Location();

			StringUtil.fixedRead(iline, format_0, format_0w, v);
			culoc.setID(((string)v[0]).Trim());
			latitude = (((string)v[1]).Trim());
			if ((latitude.Length != 0) && StringUtil.isDouble(latitude))
			{
				culoc.setLatitude(StringUtil.atod(latitude));
			}
			elevation = ((string)v[2]).Trim();
			if ((elevation.Length != 0) && StringUtil.isDouble(elevation))
			{
				culoc.setElevation(StringUtil.atod(elevation));
			}
			culoc.setRegion1(((string)v[3]).Trim());
			culoc.setRegion2(((string)v[4]).Trim());
			culoc.setName(((string)v[5]).Trim());
			num_climate_stations = (((string)v[6]).Trim());
			if ((num_climate_stations.Length != 0) && StringUtil.isInteger(num_climate_stations))
			{
				culoc.setNumClimateStations(StringUtil.atoi(num_climate_stations));
			}
			awc = ((string)v[7]).Trim();
			if ((awc.Length != 0) && StringUtil.isDouble(awc))
			{
				culoc.setAwc(StringUtil.atod(awc));
			}
			ncli = culoc.getNumClimateStations();
			for (i = 0; i < ncli; i++)
			{
				iline = @in.ReadLine();
				if (string.ReferenceEquals(iline, null))
				{
					break;
				}
				StringUtil.fixedRead(iline, format_1, format_1w, v);
				vsize = v.Count;
				culoc.setClimateStationID(((string)v[0]).Trim(), i);
				weight = ((string)v[1]).Trim();
				if ((weight.Length != 0) && StringUtil.isDouble(weight))
				{
					culoc.setTemperatureStationWeight(StringUtil.atod(weight), i);
				}
				weight = ((string)v[2]).Trim();
				if ((weight.Length != 0) && StringUtil.isDouble(weight))
				{
					culoc.setPrecipitationStationWeight(StringUtil.atod(weight), i);
				}
				if (vsize > 3)
				{
					ota = ((string)v[3]).Trim();
					if ((ota.Length != 0) && StringUtil.isDouble(ota))
					{
						culoc.setOrographicTemperatureAdjustment(StringUtil.atod(ota), i);
					}
				}
				if (vsize > 4)
				{
					opa = ((string)v[4]).Trim();
					if ((opa.Length != 0) && StringUtil.isDouble(opa))
					{
						culoc.setOrographicPrecipitationAdjustment(StringUtil.atod(opa), i);
					}
				}
			}

			// Add the StateCU_Location to the vector...
			culoc_List.Add(culoc);
		}
		if (@in != null)
		{
			@in.Close();
		}
		return culoc_List;
	}

	/// <summary>
	/// Cancels any changes made to this object within a GUI since createBackup()
	/// was called and sets _original to null.
	/// </summary>
	public override void restoreOriginal()
	{
		StateCU_Location loc = (StateCU_Location)_original;
		base.restoreOriginal();

		__awc = loc.__awc;
		__collection_div = loc.__collection_div;
		__collection_part_type = loc.__collection_part_type;
		__collection_type = loc.__collection_type;
		__elevation = loc.__elevation;
		__latitude = loc.__latitude;
		__region1 = loc.__region1;
		__region2 = loc.__region2;

		_isClone = false;
		_original = null;
	}

	/// <summary>
	/// Set the AWC. </summary>
	/// <param name="awc"> awc, fraction. </param>
	public virtual void setAwc(double awc)
	{
		__awc = awc;
	}

	/// <summary>
	/// Set the climate station identifier. </summary>
	/// <param name="id"> Climate station identifier. </param>
	/// <param name="pos"> Station index (relative to zero). </param>
	public virtual void setClimateStationID(string id, int pos)
	{
		if (__climate_station_ids == null)
		{
			__climate_station_ids = new string[pos + 1];
		}
		else if (pos >= __climate_station_ids.Length)
		{
			// Resize the array...
			string[] temp = new string[pos + 1];
			for (int i = 0; i < __climate_station_ids.Length; i++)
			{
				temp[i] = __climate_station_ids[i];
			}
			__climate_station_ids = temp;
		}
		// Finally, assign...
		__climate_station_ids[pos] = id;
	}

	/// <summary>
	/// Set the collection division.  This is needed to uniquely identify the parcels. </summary>
	/// <param name="collection_div"> The division for the collection. </param>
	public virtual void setCollectionDiv(int collection_div)
	{
		__collection_div = collection_div;
	}

	/// <summary>
	/// Set the collection list for an aggregate/system.  It is assumed that the
	/// collection applies to all years of data. </summary>
	/// <param name="ids"> The identifiers indicating the locations to collection. </param>
	public virtual void setCollectionPartIDs(IList<string> ids)
	{
		if (__collectionIDList == null)
		{
			__collectionIDList = new List<IList<string>>();
			__collectionYear = new int[1];
		}
		else
		{
			// Remove the previous contents...
			__collectionIDList.Clear();
		}
		// Now assign...
		__collectionIDList.Add(ids);
		__collectionYear[0] = 0;
	}

	/// <summary>
	/// Set the collection list for an aggregate/system.  It is assumed that the
	/// collection applies to all years of data. </summary>
	/// <param name="ids"> The identifiers indicating the locations to collection. </param>
	public virtual void setCollectionPartIDs(IList<string> ids, IList<string> idTypes)
	{
		__collectionIDList = new List<IList<string>>();
		__collectionIDTypeList = new List<IList<string>>();
		__collectionYear = new int[1];

		// Now assign...
		__collectionIDList.Add(ids);
		__collectionIDTypeList.Add(idTypes);
		__collectionYear[0] = 0;
	}

	/// <summary>
	/// Set the collection list for an aggregate/system for a specific year.  It is
	/// assumed that the collection applies to all years of data. </summary>
	/// <param name="year"> The year to which the collection applies. </param>
	/// <param name="ids"> The identifiers indicating the locations in the collection. </param>
	public virtual void setCollectionPartIDsForYear(int year, IList<string> ids)
	{
		int pos = -1; // Position of year in data lists.
		if (__collectionIDList == null)
		{
			// No previous data so create memory...
			__collectionIDList = new List<IList<string>>();
			__collectionIDList.Add(ids);
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
				__collectionIDList.Add(ids);
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
				__collectionIDList[pos] = ids;
				__collectionYear[pos] = year;
			}
		}
	}

	/// <summary>
	/// Set the collection part type. </summary>
	/// <param name="collection_part_type"> The collection part type,
	/// either COLLECTION_PART_TYPE_DITCH or COLLECTION_PART_TYPE_PARCEL. </param>
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
	/// Set the elevation. </summary>
	/// <param name="elevation"> Elevation, feet. </param>
	public virtual void setElevation(double elevation)
	{
		__elevation = elevation;
	}

	/// <summary>
	/// Set the latitude. </summary>
	/// <param name="latitude"> Latitude, decimal degrees. </param>
	public virtual void setLatitude(double latitude)
	{
		__latitude = latitude;
	}

	/// <summary>
	/// Set the number of climate stations. </summary>
	/// <param name="num_climate_stations"> Number of climate stations. </param>
	public virtual void setNumClimateStations(int num_climate_stations)
	{
		if (num_climate_stations == 0)
		{
			// Clear the arrays...
			__climate_station_ids = null;
			__precipitation_station_weights = null;
			__temperature_station_weights = null;
			__ota = null;
			__opa = null;
		}
		else
		{
			__climate_station_ids = new string[num_climate_stations];
			__precipitation_station_weights = new double[num_climate_stations];
			__temperature_station_weights = new double[num_climate_stations];
			__ota = new double[num_climate_stations];
			__opa = new double[num_climate_stations];
		}
	}

	/// <summary>
	/// Set the orographic precipitation adjustment for a station. </summary>
	/// <param name="opa"> orographic precipitation adjustment. </param>
	/// <param name="pos"> Station index (relative to zero). </param>
	public virtual void setOrographicPrecipitationAdjustment(double opa, int pos)
	{
		if (__opa == null)
		{
			__opa = new double[pos + 1];
		}
		else if (pos >= __opa.Length)
		{
			// Resize the array...
			double[] temp = new double[pos + 1];
			for (int i = 0; i < __opa.Length; i++)
			{
				temp[i] = __opa[i];
			}
			__opa = temp;
		}
		// Finally, assign...
		__opa[pos] = opa;
	}

	/// <summary>
	/// Set the orographic temperature adjustment for a station. </summary>
	/// <param name="ota"> orographic temperature adjustment. </param>
	/// <param name="pos"> Station index (relative to zero). </param>
	public virtual void setOrographicTemperatureAdjustment(double ota, int pos)
	{
		if (__ota == null)
		{
			__ota = new double[pos + 1];
		}
		else if (pos >= __ota.Length)
		{
			// Resize the array...
			double[] temp = new double[pos + 1];
			for (int i = 0; i < __ota.Length; i++)
			{
				temp[i] = __ota[i];
			}
			__ota = temp;
		}
		// Finally, assign...
		__ota[pos] = ota;
	}

	/// <summary>
	/// Set the precipitation station weight. </summary>
	/// <param name="wt"> precipitation station weight. </param>
	/// <param name="pos"> Station index (relative to zero). </param>
	public virtual void setPrecipitationStationWeight(double wt, int pos)
	{
		if (__precipitation_station_weights == null)
		{
			__precipitation_station_weights = new double[pos + 1];
		}
		else if (pos >= __precipitation_station_weights.Length)
		{
			// Resize the array...
			double[] temp = new double[pos + 1];
			for (int i = 0; i < __precipitation_station_weights.Length; i++)
			{
				temp[i] = __precipitation_station_weights[i];
			}
			__precipitation_station_weights = temp;
		}
		// Finally, assign...
		__precipitation_station_weights[pos] = wt;
	}

	/// <summary>
	/// Set region 1. </summary>
	/// <param name="region1"> Region 1 (e.g., the name of a county). </param>
	public virtual void setRegion1(string region1)
	{
		__region1 = region1;
	}

	/// <summary>
	/// Set region 2. </summary>
	/// <param name="region2"> Region 1 (e.g., the name of a HUC). </param>
	public virtual void setRegion2(string region2)
	{
		__region2 = region2;
	}

	/// <summary>
	/// Set the temperature station weight. </summary>
	/// <param name="wt"> temperature station weight. </param>
	/// <param name="pos"> Station index (relative to zero). </param>
	public virtual void setTemperatureStationWeight(double wt, int pos)
	{
		if (__temperature_station_weights == null)
		{
			__temperature_station_weights = new double[pos + 1];
		}
		else if (pos >= __temperature_station_weights.Length)
		{
			// Resize the array...
			double[] temp = new double[pos + 1];
			for (int i = 0; i < __temperature_station_weights.Length; i++)
			{
				temp[i] = __temperature_station_weights[i];
			}
			__temperature_station_weights = temp;
		}
		// Finally, assign...
		__temperature_station_weights[pos] = wt;
	}

	/// <summary>
	/// Performs specific data checks and returns a list of data that failed the data checks. </summary>
	/// <param name="dataset"> StateCU dataset currently in memory. </param>
	/// <returns> Validation results. </returns>
	public virtual StateCU_ComponentValidation validateComponent(StateCU_DataSet dataset)
	{
		StateCU_ComponentValidation validation = new StateCU_ComponentValidation();
		string id = getID();
		double latitude = getLatitude();
		if (!((latitude >= -90.0) && (latitude <= 90.0)))
		{
			validation.add(new StateCU_ComponentValidationProblem(this,"CU location \"" + id + "\" latitude (" + latitude + ") is invalid.", "Specify a latitude -90 to 90."));
		}
		double elevation = getElevation();
		if (!((elevation >= 0.0) && (elevation <= 15000.00)))
		{
			validation.add(new StateCU_ComponentValidationProblem(this,"CU location \"" + id + "\" elevation (" + elevation + ") is invalid.", "Specify an elevation 0 to 15000 FT (maximum varies by location)."));
		}
		string name = getName();
		if ((string.ReferenceEquals(name, null)) || name.Trim().Length == 0)
		{
			validation.add(new StateCU_ComponentValidationProblem(this,"CU location \"" + id + "\" name is blank - may cause confusion.", "Specify the station name or use the ID for the name."));
		}
		string region1 = getRegion1();
		if ((string.ReferenceEquals(region1, null)) || region1.Trim().Length == 0)
		{
			validation.add(new StateCU_ComponentValidationProblem(this,"CU location \"" + id + "\" region1 is blank - may cause region lookups to fail for other data.", "Specify as county or other region indicator."));
		}
		int ncli = getNumClimateStations();
		if (!(ncli >= 0))
		{
			validation.add(new StateCU_ComponentValidationProblem(this,"CU location \"" + id + "\" number of climate stations (" + ncli + ") is invalid.", "Specify as >= 0."));
		}
		if (ncli > 0)
		{
			// Check each climate station and the total...
			double totalPrecipWeight = 0.0;
			double totalTempWeight = 0.0;
			for (int i = 0; i < ncli; i++)
			{
				string cliStationID = getClimateStationID(i);
				if (cliStationID.Trim().Length == 0)
				{
					validation.add(new StateCU_ComponentValidationProblem(this,"CU location \"" + id + "\" climate station ID is invalid (blank).", "Specify the climate station ID."));
				}
				double tempWeight = getTemperatureStationWeight(i);
				if (!((tempWeight >= 0.0) && (tempWeight <= 1.0)))
				{
					validation.add(new StateCU_ComponentValidationProblem(this,"CU location \"" + id + "\" climate station \"" + cliStationID + "\" temperature station weight (" + tempWeight + ") is invalid.", "Specify as fraction 0 - 1."));
				}
				double precipWeight = getPrecipitationStationWeight(i);
				if (!((precipWeight >= 0.0) && (precipWeight <= 1.0)))
				{
					validation.add(new StateCU_ComponentValidationProblem(this,"CU location \"" + id + "\" climate station \"" + cliStationID + "\" precipitation station weight (" + precipWeight + ") is invalid.", "Specify as fraction 0 - 1."));
				}
				totalPrecipWeight += precipWeight;
				totalTempWeight += tempWeight;
				double oroTemppAdj = getOrographicTemperatureAdjustment(i);
				// Adjustment is always positive
				if (!((oroTemppAdj >= 0.0) && (oroTemppAdj <= 5.0)))
				{
					validation.add(new StateCU_ComponentValidationProblem(this,"CU location \"" + id + "\" climate station \"" + cliStationID + "\" orographic temperature adjustment (" + oroTemppAdj + ") is invalid.  May default on output but should be explicitly set.", "Specify as DEGF/1000 FT 0 to 5."));
				}
				double oroPrecipAdj = getOrographicPrecipitationAdjustment(i);
				if (!((oroPrecipAdj >= 0.0) && (oroPrecipAdj <= 1.0)))
				{
					validation.add(new StateCU_ComponentValidationProblem(this,"CU location \"" + id + "\" climate station \"" + cliStationID + "\" orographic precipitation adjustment (" + oroPrecipAdj + ") is invalid.  May default on output but should be explicitly set.", "Specify as fraction 0 - 1."));
				}
			}
			if (!((totalTempWeight >= .9999999) && (totalTempWeight <= 1.0000001)))
			{
				validation.add(new StateCU_ComponentValidationProblem(this,"CU location \"" + id + "\" total of temperature station weights (" + totalTempWeight + ") is invalid.", "Weights should add up to 1."));
			}
			if (!((totalPrecipWeight >= .9999999) && (totalPrecipWeight <= 1.0000001)))
			{
				validation.add(new StateCU_ComponentValidationProblem(this,"CU location \"" + id + "\" total of precipitation station weights (" + totalPrecipWeight + ") is invalid.", "Weights should add up to 1."));
			}
		}
		double awc = getAwc();
		if (!((awc >= 0.0) && (awc <= 1.0)))
		{
			validation.add(new StateCU_ComponentValidationProblem(this,"CU location \"" + id + "\" available water content (AWC) (" + awc + ") is invalid.", "Specify as fraction 0 - 1."));
		}
		return validation;
	}

	/// <summary>
	/// Write a list of StateCU_Location to a file using default properties.  The filename is adjusted to the
	/// working directory if necessary using IOUtil.getPathUsingWorkingDir(). </summary>
	/// <param name="filenamePrev"> The name of the previous version of the file (for
	/// processing headers).  Specify as null if no previous file is available. </param>
	/// <param name="filename"> The name of the file to write. </param>
	/// <param name="dataList"> A list of StateCU_Location to write. </param>
	/// <param name="newComments"> Comments to add to the top of the file.  Specify as null if no comments are available. </param>
	/// <exception cref="IOException"> if there is an error writing the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeStateCUFile(String filenamePrev, String filename, java.util.List<StateCU_Location> dataList, java.util.List<String> newComments) throws java.io.IOException
	public static void writeStateCUFile(string filenamePrev, string filename, IList<StateCU_Location> dataList, IList<string> newComments)
	{
		writeStateCUFile(filenamePrev, filename, dataList, newComments, null);
	}

	/// <summary>
	/// Write a list of StateCU_Location to a file.  The filename is adjusted to the
	/// working directory if necessary using IOUtil.getPathUsingWorkingDir(). </summary>
	/// <param name="filenamePrev"> The name of the previous version of the file (for
	/// processing headers).  Specify as null if no previous file is available. </param>
	/// <param name="filename"> The name of the file to write. </param>
	/// <param name="dataList"> A list of StateCU_Location to write. </param>
	/// <param name="newComments"> Comments to add to the top of the file.  Specify as null if no comments are available. </param>
	/// <param name="props"> Properties to control the write.  Currently only the following
	/// property is supported:  Version=True|False.  If the version is "10", then the
	/// file format will match that for version 10.  Otherwise, the newest format is
	/// used.  This is useful for comparing with or regenerating old data sets. </param>
	/// <exception cref="IOException"> if there is an error writing the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeStateCUFile(String filenamePrev, String filename, java.util.List<StateCU_Location> dataList, java.util.List<String> newComments, RTi.Util.IO.PropList props) throws java.io.IOException
	public static void writeStateCUFile(string filenamePrev, string filename, IList<StateCU_Location> dataList, IList<string> newComments, PropList props)
	{
		IList<string> commentStr = new List<string>(1);
		commentStr.Add("#");
		IList<string> ignoreCommentStr = new List<string>(1);
		ignoreCommentStr.Add("#>");
		PrintWriter @out = null;
		string fullFilenamePrev = IOUtil.getPathUsingWorkingDir(filenamePrev);
		string fullFilename = IOUtil.getPathUsingWorkingDir(filename);
		@out = IOUtil.processFileHeaders(fullFilenamePrev, fullFilename, newComments, commentStr, ignoreCommentStr, 0);
		if (@out == null)
		{
			throw new IOException("Error writing to \"" + fullFilename + "\"");
		}
		writeStateCUFile(dataList, @out, props);
		@out.flush();
		@out.close();
	}

	/// <summary>
	/// Write a Vector of StateCU_Location to an opened file. </summary>
	/// <param name="dataList"> A Vector of StateCU_Location to write. </param>
	/// <param name="out"> output PrintWriter. </param>
	/// <param name="props"> Properties to control the write.  See the writeStateCUFile() method for a description. </param>
	/// <exception cref="IOException"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static void writeStateCUFile(java.util.List<StateCU_Location> dataList, java.io.PrintWriter out, RTi.Util.IO.PropList props) throws java.io.IOException
	private static void writeStateCUFile(IList<StateCU_Location> dataList, PrintWriter @out, PropList props)
	{
		int i, j;
		string cmnt = "#>";
		// Missing data handled by formatting as a string...
		// The following format works for both lines.
		string format = "%-12.12s%6.6s%9.9s  %-20.20s%-8.8s  %-24.24s%4.4s%8.8s";
		string format_version10 = "%-12.12s%6.6s%9.9s  %-20.20s%-8.8s  %-24.24s";
		string format2 = "%-12.12s%6.6s%9.9s%9.9s%9.9s";
		// Not used but indicates format before orographic adjustments were added
		//String format2_version10 = "%-12.12s%6.6s%9.9s";
		StateCU_Location cu_loc = null;
		IList<object> v = new List<object>(8); // Reuse for all output lines.

		if (props == null)
		{
			props = new PropList("StateCU_Location");
		}
		string Version = props.getValue("Version");
		bool Version_10 = false;
		if ((!string.ReferenceEquals(Version, null)) && Version.Equals("10", StringComparison.OrdinalIgnoreCase))
		{
			Version_10 = true;
		}

		@out.println(cmnt);
		@out.println(cmnt + "  StateCU CU Locations (STR) File");
		@out.println(cmnt);
		@out.println(cmnt + "  Record 1 format (a12,f6.2,11x,a10,10x,i8,2x,a24,i4,f8.4)");
		@out.println(cmnt);
		@out.println(cmnt + "  ID       base_id:  CU Location identifier");
		@out.println(cmnt + "  Latitude    blat:  Latitude (decimal degrees)");
		@out.println(cmnt + "  Elevation   elev:  Elevation (feet)");
		@out.println(cmnt + "  Region1  ttcount:  Region1 (e.g., County)");
		@out.println(cmnt + "  Region2    tthuc:  Region2 (e.g., Hydrologic Unit)");
		@out.println(cmnt + "                     Optional");
		@out.println(cmnt + "  Name     base_id:  CU Location name");
		if (!Version_10)
		{
			@out.println(cmnt + "  NCli            :  Number of climate stations");
			@out.println(cmnt + "  AWC             :  Available water content (fraction)");
			@out.println(cmnt);
			@out.println(cmnt + "  Record 2+ format (a12,f6.2,3f9.2)");
			@out.println(cmnt);
			@out.println(cmnt + "  ClimID          :  Climate station identifier");
			@out.println(cmnt + "  TmpWT           :  Temperature station weight (fraction)");
			@out.println(cmnt + "  PptWT           :  Precipitation station weight (fraction)");
			@out.println(cmnt + "                     Weights for each type should add to 1.0");
			@out.println(cmnt + "  OroTmpAdj       :  Orographic temperature station adjustment (DEGF/1000 FT)");
			@out.println(cmnt + "  OroPptAdj       :  Orographic precipitation station adjustment (fraction)");
			@out.println(cmnt);
		}
		if (Version_10)
		{
			@out.println(cmnt + "    ID     Lat  Elevation   Region1             Region2       Name");
			@out.println(cmnt + "---------eb----eb-------bxxb--------exxxxxxxxxxb------exxb----------------------e");
		}
		else
		{
			// Full output...
			@out.println(cmnt + "    ID     Lat  Elevation   Region1             Region2       Name               NCli  AWC");
			@out.println(cmnt + "---------eb----eb-------bxxb--------exxxxxxxxxxb------exxb----------------------eb--eb------e");
			@out.println(cmnt);
			@out.println(cmnt + " ClimID    TmpWT  PptWt  OroTmpAdj OroPptAdj");
			@out.println(cmnt + "---------eb----eb-------eb-------eb-------e");
		}
		@out.println(cmnt + "EndHeader");

		int num = 0;
		if (dataList != null)
		{
			num = dataList.Count;
		}
		int numclimate = 0;
		double val; // Generic value
		for (i = 0; i < num; i++)
		{
			cu_loc = dataList[i];
			if (cu_loc == null)
			{
				continue;
			}

			v.Clear();
			v.Add(cu_loc._id);
			if (StateCU_Util.isMissing(cu_loc.__latitude))
			{
				v.Add("");
			}
			else
			{
				v.Add(StringUtil.formatString(cu_loc.__latitude,"%6.2f"));
			}
			if (StateCU_Util.isMissing(cu_loc.__elevation))
			{
				v.Add("");
			}
			else
			{
				v.Add(StringUtil.formatString(cu_loc.__elevation,"%9.2f"));
			}
			v.Add(cu_loc.__region1);
			v.Add(cu_loc.__region2);
			v.Add(cu_loc._name);
			if (!Version_10)
			{
				numclimate = cu_loc.getNumClimateStations();
				v.Add(StringUtil.formatString(numclimate,"%4d"));
				if (StateCU_Util.isMissing(cu_loc.__awc))
				{
					v.Add("");
				}
				else
				{
					v.Add(StringUtil.formatString(cu_loc.__awc,"%8.4f"));
				}
			}

			if (Version_10)
			{
				@out.println(StringUtil.formatString(v, format_version10));
			}
			else
			{
				@out.println(StringUtil.formatString(v, format));
			}
			if (!Version_10)
			{
				// Print the climate station weights.
				// If values are missing, assign reasonable defaults.
				for (j = 0; j < numclimate; j++)
				{
					v.Clear();
					v.Add(StringUtil.formatString(cu_loc.getClimateStationID(j),"%-12.12s"));
					val = cu_loc.getTemperatureStationWeight(j);
					if (StateCU_Util.isMissing(val))
					{
						val = 0.0;
					}
					v.Add(StringUtil.formatString(val,"%.2f"));
					val = cu_loc.getPrecipitationStationWeight(j);
					if (StateCU_Util.isMissing(val))
					{
						val = 0.0;
					}
					v.Add(StringUtil.formatString(val,"%.2f"));
					val = cu_loc.getOrographicTemperatureAdjustment(j);
					if (StateCU_Util.isMissing(val))
					{
						val = 0.0;
					}
					v.Add(StringUtil.formatString(val,"%.2f"));
					val = cu_loc.getOrographicPrecipitationAdjustment(j);
					if (StateCU_Util.isMissing(val))
					{
						val = 1.0;
					}
					v.Add(StringUtil.formatString(val,"%.2f"));
					@out.println(StringUtil.formatString(v, format2));
				}
			}
		}
	}

	/// <summary>
	/// Writes a Vector of StateCU_Location objects to a list file.  A header is 
	/// printed to the top of the file, containing the commands used to generate the 
	/// file.  Any strings in the body of the file that contain the field delimiter 
	/// will be wrapped in "...".  <para>
	/// This method also writes Climate Station and Collection data to
	/// so if this method is called with a filename parameter of "locations.txt", 
	/// three files may be generated:
	/// - locations.txt
	/// - locations_ClimateStations.txt
	/// - locations_Collections.txt
	/// </para>
	/// </summary>
	/// <param name="filename"> the name of the file to which the data will be written. </param>
	/// <param name="delimiter"> the delimiter to use for separating field values. </param>
	/// <param name="update"> whether to update an existing file, retaining the current 
	/// header (true) or to create a new file with a new header. </param>
	/// <returns> a list of files that were actually written, because this method controls all the secondary
	/// filenames. </returns>
	/// <param name="data"> the list of objects to write. </param>
	/// <param name="newComments"> comments to add to the top of the file (e.g., command file and HydroBase version). </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.List<java.io.File> writeListFile(String filename, String delimiter, boolean update, java.util.List<StateCU_Location> data, java.util.List<String> newComments) throws Exception
	public static IList<File> writeListFile(string filename, string delimiter, bool update, IList<StateCU_Location> data, IList<string> newComments)
	{
		int size = 0;
		if (data != null)
		{
			size = data.Count;
		}

		IList<string> fields = new List<string>();
		fields.Add("ID");
		fields.Add("Name");
		fields.Add("Latitude");
		fields.Add("Elevation");
		fields.Add("Region1");
		fields.Add("Region2");
		fields.Add("NumClimateStations");
		fields.Add("AWC");
		int fieldCount = fields.Count;

		IList<string> names = new List<string>(fieldCount);
		IList<string> formats = new List<string>(fieldCount);
		int comp = StateCU_DataSet.COMP_CU_LOCATIONS;
		string s = null;
		for (int i = 0; i < fieldCount; i++)
		{
			s = fields[i];
			names.Add(StateCU_Util.lookupPropValue(comp, "FieldName", s));
			formats.Add(StateCU_Util.lookupPropValue(comp, "Format", s));
		}

		string oldFile = null;
		if (update)
		{
			oldFile = IOUtil.getPathUsingWorkingDir(filename);
		}

		int j = 0;
		PrintWriter @out = null;
		StateCU_Location loc = null;
		IList<string> commentString = new List<string>(1);
		commentString.Add("#");
		IList<string> ignoreCommentString = new List<string>(1);
		ignoreCommentString.Add("#>");
		string[] line = new string[fieldCount];
		StringBuilder buffer = new StringBuilder();

		string filenameFull = IOUtil.getPathUsingWorkingDir(filename);
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
			newComments2.Insert(1,"StateCU location information as delimited list file.");
			newComments2.Insert(2,"See also the associated climate station assignment and collection files.");
			newComments2.Insert(3,"");
			@out = IOUtil.processFileHeaders(oldFile, filenameFull, newComments2, commentString, ignoreCommentString, 0);

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
				loc = data[i];

				line[0] = StringUtil.formatString(loc.getID(), ((string)formats[0])).Trim();
				line[1] = StringUtil.formatString(loc.getName(), ((string)formats[1])).Trim();
				line[2] = StringUtil.formatString(loc.getLatitude(), ((string)formats[2])).Trim();
				line[3] = StringUtil.formatString(loc.getElevation(), ((string)formats[3])).Trim();
				line[4] = StringUtil.formatString(loc.getRegion1(), ((string)formats[4])).Trim();
				line[5] = StringUtil.formatString(loc.getRegion2(), ((string)formats[5])).Trim();
				line[6] = StringUtil.formatString(loc.getNumClimateStations(), ((string)formats[6])).Trim();
				line[7] = StringUtil.formatString(loc.getAwc(), ((string)formats[7])).Trim();

				buffer = new StringBuilder();
				for (j = 0; j < fieldCount; j++)
				{
					if (line[j].IndexOf(delimiter, StringComparison.Ordinal) > -1)
					{
						line[j] = "\"" + line[j] + "\"";
					}
					if (j > 0)
					{
						buffer.Append(delimiter);
					}
					buffer.Append(line[j]);
				}

				@out.println(buffer.ToString());
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

		string climateFilename = front + "_ClimateStations." + end;
		writeClimateStationListFile(climateFilename, delimiter, update, data, newComments);

		string collectionFilename = front + "_Collections." + end;
		writeCollectionListFile(collectionFilename, delimiter, update, data, newComments);

		IList<File> filesWritten = new List<File>();
		filesWritten.Add(new File(filenameFull));
		filesWritten.Add(new File(climateFilename));
		filesWritten.Add(new File(collectionFilename));
		return filesWritten;
	}

	/// <summary>
	/// Writes the climate station data from a list of StateCU_Location objects to a 
	/// list file.  A header is printed to the top of the file, containing the commands 
	/// used to generate the file.  Any strings in the body of the file that contain 
	/// the field delimiter will be wrapped in "...". </summary>
	/// <param name="filename"> the name of the file to which the data will be written. </param>
	/// <param name="delimiter"> the delimiter to use for separating field values. </param>
	/// <param name="update"> whether to update an existing file, retaining the current 
	/// header (true) or to create a new file with a new header. </param>
	/// <param name="data"> the list of StateCU_Location objects to write (climate station assignments will
	/// be extracted). </param>
	/// <param name="newComments"> comments to add at the top of the file (e.g., commands, HydroBase information). </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeClimateStationListFile(String filename, String delimiter, boolean update, java.util.List<StateCU_Location> data, java.util.List<String> newComments) throws Exception
	public static void writeClimateStationListFile(string filename, string delimiter, bool update, IList<StateCU_Location> data, IList<string> newComments)
	{
		int size = 0;
		if (data != null)
		{
			size = data.Count;
		}

		IList<string> fields = new List<string>();
		fields.Add("LocationID");
		fields.Add("StationID");
		fields.Add("TempWeight");
		fields.Add("PrecipWeight");
		fields.Add("OrographicTempAdj");
		fields.Add("OrographicPrecipAdj");
		int fieldCount = fields.Count;

		IList<string> names = new List<string>(fieldCount);
		IList<string> formats = new List<string>(fieldCount);
		int comp = StateCU_DataSet.COMP_CU_LOCATION_CLIMATE_STATIONS;
		string s = null;
		for (int i = 0; i < fieldCount; i++)
		{
			s = fields[i];
			names.Add(StateCU_Util.lookupPropValue(comp, "FieldName", s));
			formats.Add(StateCU_Util.lookupPropValue(comp, "Format", s));
		}

		string oldFile = null;
		if (update)
		{
			oldFile = IOUtil.getPathUsingWorkingDir(filename);
		}

		int j = 0;
		int k = 0;
		int num = 0;
		PrintWriter @out = null;
		StateCU_Location loc = null;
		IList<string> commentString = new List<string>(1);
		commentString.Add("#");
		IList<string> ignoreCommentString = new List<string>(1);
		ignoreCommentString.Add("#>");
		string[] line = new string[fieldCount];
		string id = null;
		StringBuilder buffer = new StringBuilder();

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
			newComments2.Insert(1,"StateCU location climate station assignment information as delimited list file.");
			newComments2.Insert(2,"See also the associated location and collection files.");
			newComments2.Insert(3,"");
			@out = IOUtil.processFileHeaders(oldFile, IOUtil.getPathUsingWorkingDir(filename), newComments2, commentString, ignoreCommentString, 0);

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
				loc = (StateCU_Location)data[i];
				id = loc.getID();
				num = loc.getNumClimateStations();

				for (j = 0; j < num; j++)
				{
					line[0] = StringUtil.formatString(id, ((string)formats[0])).Trim();
					line[1] = StringUtil.formatString(loc.getClimateStationID(j), ((string)formats[1])).Trim();
					line[2] = StringUtil.formatString(loc.getTemperatureStationWeight(j), ((string)formats[2])).Trim();
					line[3] = StringUtil.formatString(loc.getPrecipitationStationWeight(j), ((string)formats[3])).Trim();
					line[4] = StringUtil.formatString(loc.getOrographicTemperatureAdjustment(j), ((string)formats[4])).Trim();
					line[5] = StringUtil.formatString(loc.getOrographicPrecipitationAdjustment(j), ((string)formats[5])).Trim();

					buffer = new StringBuilder();
					for (k = 0; k < fieldCount; k++)
					{
						if (line[k].IndexOf(delimiter, StringComparison.Ordinal) > -1)
						{
							line[k] = "\"" + line[k] + "\"";
						}
						buffer.Append(line[k]);
						if (k < (fieldCount - 1))
						{
							buffer.Append(delimiter);
						}
					}

					@out.println(buffer.ToString());
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
		}
	}

	/// <summary>
	/// Writes the collection data from a list of StateCU_Location objects to a 
	/// list file.  A header is printed to the top of the file, containing the commands 
	/// used to generate the file.  Any strings in the body of the file that contain 
	/// the field delimiter will be wrapped in "...". </summary>
	/// <param name="filename"> the name of the file to which the data will be written. </param>
	/// <param name="delimiter"> the delimiter to use for separating field values. </param>
	/// <param name="update"> whether to update an existing file, retaining the current 
	/// header (true) or to create a new file with a new header. </param>
	/// <param name="data"> the list of StateCU_Location objects to write, from which collection information will
	/// be extracted. </param>
	/// <param name="newComments"> comments to add at the top of the file (e.g., commands, HydroBase information). </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeCollectionListFile(String filename, String delimiter, boolean update, java.util.List<StateCU_Location> data, java.util.List<String> newComments) throws Exception
	public static void writeCollectionListFile(string filename, string delimiter, bool update, IList<StateCU_Location> data, IList<string> newComments)
	{
		int size = 0;
		if (data != null)
		{
			size = data.Count;
		}

		IList<string> fields = new List<string>();
		fields.Add("LocationID");
		fields.Add("Division");
		fields.Add("Year");
		fields.Add("CollectionType");
		fields.Add("PartType");
		fields.Add("PartID");
		fields.Add("PartIDType");
		int fieldCount = fields.Count;

		string[] names = new string[fieldCount];
		string[] formats = new string[fieldCount];
		int comp = StateCU_DataSet.COMP_CU_LOCATION_COLLECTIONS;

		string s = null;
		for (int i = 0; i < fieldCount; i++)
		{
			s = fields[i];
			names[i] = StateCU_Util.lookupPropValue(comp, "FieldName", s);
			formats[i] = StateCU_Util.lookupPropValue(comp, "Format", s);
		}

		string oldFile = null;
		if (update)
		{
			oldFile = IOUtil.getPathUsingWorkingDir(filename);
		}

		int[] years = null;
		int div = 0;
		int j = 0;
		int k = 0;
		int numYears = 0;
		PrintWriter @out = null;
		StateCU_Location loc = null;
		IList<string> commentString = new List<string>(1);
		commentString.Add("#");
		IList<string> ignoreCommentString = new List<string>(1);
		ignoreCommentString.Add("#>");
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
			newComments2.Insert(1,"StateCU location collection information as delimited list file.");
			newComments2.Insert(2,"See also the associated location and climate station assignment files.");
			newComments2.Insert(3,"Division and year are only used with well parcel aggregates/systems.");
			newComments2.Insert(4,"ParcelIdType are only used with well aggregates/systems where the part ID is Well.");
			newComments2.Insert(5,"");
			@out = IOUtil.processFileHeaders(oldFile, IOUtil.getPathUsingWorkingDir(filename), newComments2, commentString, ignoreCommentString, 0);

			for (int i = 0; i < fieldCount; i++)
			{
				if (i != 0)
				{
					buffer.Append(delimiter);
				}
				buffer.Append("\"" + names[i] + "\"");
			}

			@out.println(buffer.ToString());

			for (int i = 0; i < size; i++)
			{
				loc = data[i];
				id = loc.getID();
				div = loc.getCollectionDiv();
				years = loc.getCollectionYears();
				if (years == null)
				{
					numYears = 0; // By this point, collections that span the full period will use 1 year = 0
				}
				else
				{
					numYears = years.Length;
				}
				colType = loc.getCollectionType();
				partType = loc.getCollectionPartType();
				// Loop through the number of years of collection data
				idTypes = loc.getCollectionPartIDTypes(); // Currently crosses all years
				int numIdTypes = 0;
				if (idTypes != null)
				{
					numIdTypes = idTypes.Count;
				}
				for (j = 0; j < numYears; j++)
				{
					ids = loc.getCollectionPartIDsForYear(years[j]);
					// Loop through the identifiers for the specific year
					for (k = 0; k < ids.Count; k++)
					{
						field[0] = StringUtil.formatString(id,formats[0]).Trim();
						field[1] = StringUtil.formatString(div,formats[1]).Trim();
						field[2] = StringUtil.formatString(years[j],formats[2]).Trim();
						field[3] = StringUtil.formatString(colType,formats[3]).Trim();
						field[4] = StringUtil.formatString(partType,formats[4]).Trim();
						field[5] = StringUtil.formatString(ids[k],formats[5]).Trim();
						field[6] = "";
						if (numIdTypes > k)
						{
							// Have data to output
							field[6] = StringUtil.formatString(idTypes[k],formats[6]).Trim();
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
		}
	}

	}

}