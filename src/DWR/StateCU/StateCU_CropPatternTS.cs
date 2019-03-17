using System;
using System.Collections.Generic;
using System.IO;

// StateCU_CropPatternTS.java - base class for StateCU crop pattern time series

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
// StateCU_CropPatternTS.java - base class for StateCU crop pattern time series
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
// 2002-09-26	J. Thomas Sapienza, RTi	Initial version from CUTimeSeries.java
// 2002-09-30	JTS, RTi		Added the output code, added code to
//					translate missing data to 0s for doing
//					totals and percents.
// 2003-02-23	Steven A. Malers, RTi	Rework for the official StateDMI
//					release.
// 2003-06-04	SAM, RTi		Change name of class from
//					CUCropPatternTS to
//					StateCU_CropPatternTS.
//					Update to use new TS package.
// 2003-07-07	SAM, RTi		Update to use new format as per Erin
//					Wilson.  This involves using a header
//					record, number of crops, and formatting
//					more suitable for a spreadsheet.  The
//					old code is commented out in case we
//					need to support reading the old files.
//					The units have been added to the
//					constructor since they are in the file
//					header.
// 2004-02-05	SAM, RTi		Update for use with TSTool.
//					* Add readTimeSeries() and
//					  readTimeSeriesList() methods to read
//					  time series only, which can be used by
//					  TSTool and other software for pure
//					  time series analysis and manipulation.
//					* Change so data types have "CropArea-"
//					  in front.
// 2004-02-28	SAM, RTi		* Move some utility code from
//					  StateCU_Data to StateCU_Util.
// 2004-02-29	SAM, RTi		* Fix writeStateCU to use the exact
//					  format (without extra formats) so that
//					  StringUtil.formatString() does not
//					  complain.
// 2004-03-02	SAM, RTi		* Add removeAllTS() to remove all time
//					  series for the location.  This is
//					  called before resetting the time
//					  series.
//					* Add translateCropName() to deal with
//					  StateDMI input not being of the
//					  correct crop type.
// 2004-03-03	SAM, RTi		* Add translateCropName() to support
//					  StateDMI.
//					* Add removeCropName() to support
//					  StateDMI.
// 2004-03-11	SAM, RTi		* Add option to write the crop acreage
//					  in addition to the fraction.
// 2004-03-31	SAM, RTi		* Erin Wilson agreed to add the acreage
//					  in addition to the percent so make it
//					  happen - also line up the columns.
// 2004-05-17	SAM, RTi		* Add setCropAreasToZero() to simplify
//					  processing in StateDMI.
// 2004-06-02	SAM, RTi		* Add a header to the output, similar
//					  to the irrigation practice time series
//					  file.
//					* Remove commented code for old format.
// 2004-06-24	SAM, RTi		* Update translateCropName() to combine
//					  the crops if the new crop name is the
//					  same as an existing crop name.
// 2005-01-19	SAM, RTi		* Add toTSVector() utility method to
//					  simplify getting a vector of all the
//					  time series.
// 2005-02-25	SAM, RTi		* Add setTotalArea() to allow data to be
//					  reset.
//					* Add setCropArea() to simplify data
//					  reset.
// 2005-03-31	SAM, RTi		* Add overload toTSVector() method to
//					  add total time series to the raw
//					  data Vector, for use by StateDMI and
//					  other applications.  In the future,
//					  it may make sense to have this method
//					  be optionally called when reading the
//					  IPY file in TSTool, etc.
// 2005-06-03	SAM, RTI		* Add WriteOnlyTotal and overload the
//					  writeStateCUFile to take a PropList.
// 2005-06-08	SAM, RTi		* Overload readStateCUFile() to take a
//					  PropList and support a Version
//					  property.
// 2005-07-28	SAM, RTi		* When writing the area for each crop,
//					  calculate to the precision of the
//					  fraction, so that the numbers agree.
// 2005-11-17	SAM, RTi		* The start/end months and year type
//					  were not previously included.
// 2005-11-30	SAM, RTi		* Fix so that trying to set data outside
//					  the period in memory results in no
//					  action.
//					* When reading time series, the
//					  requested period was being used to
//					  initialize memory but was not being
//					  used to optimize the read start/stop.
//					  Put in a REVISIT for now to note that
//					  an improvement can be made when there
//					  is time.
// 2007-01-09	Kurt Tometich RTi
//						Added a new method isVersion10() to detect
//						whether the file being read is Version10 or
//						newer.  
//						Updated the writeVector and readStateCUFile methods
//						for version10 and newer formats.
// 2007-01-29	SAM, RTi		Review KAT's code.
// 2007-03-02	SAM, RTi		Final cleanup before release.
// 2007-03-21	SAM, RTi		Change isVersion10() to be less fragile.
// 2007-04-13	SAM, RTi		Reenable support for really old format where no
//						period of record header was used.
// 2007-05-18	SAM, RTi		Add saving parcel data for each year of
//						observations to facilitate filling later.
// ----------------------------------------------------------------------------
// EndHeader

namespace DWR.StateCU
{

	using DateValueTS = RTi.TS.DateValueTS;
	using TS = RTi.TS.TS;
	using TSIdent = RTi.TS.TSIdent;
	using TSUtil = RTi.TS.TSUtil;
	using YearTS = RTi.TS.YearTS;
	using IOUtil = RTi.Util.IO.IOUtil;
	using PropList = RTi.Util.IO.PropList;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;
	using DateTime = RTi.Util.Time.DateTime;

	/// <summary>
	/// The StateCU_CropPatternTS class is used to hold crop time series data
	/// (acreage/area) used in StateDMI for CDS files.  Each instance has an identifier,
	/// which will match a StateCU_Location identifier, and a list of time series for
	/// various crops that are associated with the CU Location for a period of time.
	/// If an average annual analysis is done, the period may consist of one zero year.
	/// </summary>
	public class StateCU_CropPatternTS : StateCU_Data, StateCU_ComponentValidator
	{

	/// <summary>
	/// The file that is read, used to set the time series input name.
	/// </summary>
	private string __filename = "";

	/// <summary>
	/// The list of crop time series.  The data type for each time series is the crop type.
	/// </summary>
	private IList<TS> __tslist = new List<TS>();

	/// <summary>
	/// The list of StateCU_Parcel observations, as an archive of observations to use with data filling.
	/// These are read from HydroBase.
	/// </summary>
	private IList<StateCU_Parcel> __parcel_Vector = new List<StateCU_Parcel>();

	/// <summary>
	/// Total acres (total of all crops) for each year in the period.  This is computed
	/// from the time series in __tslist when crop values are set.
	/// </summary>
	private double[] __total_area = null;

	/// <summary>
	/// Dates for the period of record (for all time series).
	/// </summary>
	private DateTime __date1 = null;
	private DateTime __date2 = null;

	/// <summary>
	/// Units of the data in the file, typically acres.
	/// </summary>
	private string __units = "";

	/// <summary>
	/// Internal data that is used to set/get data so new DateTime objects don't need to
	/// be created each time.  Only the year is manipulated.
	/// </summary>
	private DateTime __temp_DateTime = new DateTime();

	/// <summary>
	/// List of crop types for the time series.  This is consistent with the data
	/// sub-types for the time series.  The list is maintained to simplify use at output.
	/// </summary>
	private IList<string> __crop_name_Vector = null;

	/// <summary>
	/// Construct a new StateCU_CropPatternTS object for the specified CU Location identifier. </summary>
	/// <param name="id"> StateCU_Location identifier. </param>
	/// <param name="date1"> Starting date of period.  Specify with year 0 if an average annual data set. </param>
	/// <param name="date2"> Ending date of period.  Specify with year 0 if an average annual data set. </param>
	/// <param name="units"> Data units for the acreage. </param>
	public StateCU_CropPatternTS(string id, DateTime date1, DateTime date2, string units) : this(id, date1, date2, units, null)
	{
	}

	/// <summary>
	/// Construct a new StateCU_CropPatternTS object for the specified CU Location
	/// identifier.  This is typically only called by readStateCUFile(). </summary>
	/// <param name="id"> StateCU_Location identifier. </param>
	/// <param name="date1"> Starting date of period.  Specify with year 0 if an average annual data set. </param>
	/// <param name="date2"> Ending date of period.  Specify with year 0 if an average annual data set. </param>
	/// <param name="units"> Data units for the acreage. </param>
	/// <param name="filename"> The name of the file that is being read, or null if created in memory. </param>
	public StateCU_CropPatternTS(string id, DateTime date1, DateTime date2, string units, string filename) : base()
	{
		_id = id;
		__tslist = new List<TS>();
		__crop_name_Vector = new List<string>();
		__units = units;
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
		__total_area = new double[__date2.getYear() - __date1.getYear() + 1];
		for (int i = 0; i < __total_area.Length; i++)
		{
			__total_area[i] = -999.0;
		}
	}

	/// <summary>
	/// Add a parcel containing observations.  This is used to store raw data (e.g., from HydroBase) so that
	/// later filling and checks can more easily be performed. </summary>
	/// <param name="parcel"> StateCU_Parcel to add. </param>
	public virtual void addParcel(StateCU_Parcel parcel)
	{
		__parcel_Vector.Add(parcel);
		string routine = "StateCU_CropPatternTS.addParcel";
		Message.printStatus(2, routine, "Adding parcel " + parcel.ToString());
	}

	/// <summary>
	/// Add another crop time series, using the period that was defined in the constructor. </summary>
	/// <param name="crop_name"> Name of crop for this time series (only include the crop name, not the leading "CropArea-"). </param>
	/// <param name="overwrite"> If false, the time series is only added if it does not already
	/// exist.  If true, the time series is added regardless and replaces the existing time series. </param>
	/// <returns> the position that the time series was added in the list (zero reference). </returns>
	/// <exception cref="Exception"> if there is an error adding the time series. </exception>
	public virtual YearTS addTS(string crop_name, bool overwrite)
	{
		int pos = indexOf(crop_name);
		YearTS yts = null;
		if ((pos < 0) || overwrite)
		{
			yts = new YearTS();
			try
			{
				TSIdent tsident = new TSIdent(_id, "StateCU", "CropArea-" + crop_name, "Year", "");
				yts.setIdentifier(tsident);
				yts.getIdentifier().setInputType("StateCU");
				if (!string.ReferenceEquals(__filename, null))
				{
					yts.getIdentifier().setInputName(__filename);
				}
			}
			catch (Exception)
			{
				// This should NOT happen because the TSID is being controlled...
				Message.printWarning(2, "StateCU_CropPatternTS.addTS", "Error adding time series for \"" + crop_name + "\"");
			}
			yts.setDataUnits(__units);
			yts.setDataUnitsOriginal(__units);
			yts.setDescription(_id + " " + crop_name + " crop area");
			yts.setDate1(new DateTime(__date1));
			yts.setDate2(new DateTime(__date2));
			yts.setDate1Original(new DateTime(__date1));
			yts.setDate2Original(new DateTime(__date2));
			yts.allocateDataSpace();
		}
		if (pos < 0)
		{
			pos = __tslist.Count;
			__tslist.Add(yts);
			__crop_name_Vector.Add(crop_name);
		}
		else
		{
			__tslist[pos] = yts;
			__crop_name_Vector[pos] = crop_name;
		}
		//Message.printStatus ( 1, "", "SAMX Adding time series for " + _id +
			//" " + crop_name + " for " + __date1 + " to " + __date2 +
			//" at " + pos );
		return yts;
	}

	/// <summary>
	/// Get the crop acreage for the given year. </summary>
	/// <returns> the crop acreage for the given year.  Return -999.0 if the crop is not
	/// found or the requested year is outside the data period. </returns>
	/// <param name="crop_name"> Name of the crop, only the crop name without the leading "CropArea-". </param>
	/// <param name="year"> Year to retrieve data. </param>
	/// <param name="return_fraction"> If true, return the acreage as a fraction (0.0 to 1.0) of
	/// the total.  If false, return the acreage. </param>
	public virtual double getCropArea(string crop_name, int year, bool return_fraction)
	{
		int pos = indexOf(crop_name);
		if (pos < 0)
		{
			return -999.0;
		}
		if ((year < __date1.getYear()) || (year > __date2.getYear()))
		{
			return -999.0;
		}
		YearTS yts = (YearTS)__tslist[pos];
		__temp_DateTime.setYear(year);
		if (return_fraction)
		{
			// Need to consider a total that is zero or missing..
			double total_area = __total_area[year - __date1.getYear()];
			if (total_area < 0.0)
			{
				// Missing...
				return -999.0;
			}
			else if (total_area == 0.0)
			{
				return 0.0;
			}
			else
			{
				// Total is an actual value so evaluate the specific value...
				double value = yts.getDataValue(__temp_DateTime);
				if (value < 0.0)
				{
					// Missing...
					return -999.0;
				}
				else
				{
					return (value / total_area);
				}
			}
		}
		else
		{
			// Will return missing value if that is what it is...
			return yts.getDataValue(__temp_DateTime);
		}
	}

	/// <summary>
	/// Return the list of crops for this CU Location.
	/// </summary>
	public virtual IList<string> getCropNames()
	{
		return __crop_name_Vector;
	}

	/// <summary>
	/// Return the list of distinct crop names for a list of StateCU_CropPatternTS.
	/// The list will be sorted alphabetically. </summary>
	/// <param name="dataList"> A list of StateCU_CropPatternTS to process. </param>
	/// <returns> a list of distinct crop names, determined from data_Vector.  A
	/// non-null list is guaranteed, but may be empty. </returns>
	public static IList<string> getCropNames(IList<StateCU_CropPatternTS> dataList)
	{
		IList<string> v = new List<string> (10);
		int size = 0;
		StateCU_CropPatternTS cds = null;
		if (dataList != null)
		{
			size = dataList.Count;
		}
		IList<string> crop_names = null;
		string crop_name;
		int vsize = 0;
		int ncrops, j, k;
		bool found;
		// Loop through StateCU_CropPatternTS instances...
		for (int i = 0; i < size; i++)
		{
			cds = dataList[i];
			if (cds == null)
			{
				continue;
			}
			// Get the crops from the StateCU_CropPatternTS instance...
			crop_names = cds.getCropNames();
			ncrops = 0;
			if (crop_names != null)
			{
				ncrops = crop_names.Count;
			}
			// Loop through each crop and add to the main Vector if it is not already in that list...
			for (j = 0; j < ncrops; j++)
			{
				crop_name = crop_names[j];
				vsize = v.Count;
				found = false;
				for (k = 0; k < vsize; k++)
				{
					if (crop_name.Equals(v[k], StringComparison.OrdinalIgnoreCase))
					{
						found = true;
						break;
					}
				}
				if (!found)
				{
					// Add the name...
					v.Add(crop_name);
				}
			}
		}

		// Alphabetize...

		if (v.Count == 0)
		{
			return new List<string>();
		}
		else
		{
			return (StringUtil.sortStringList(v));
		}
	}

	/// <summary>
	/// Return the time series for the matching crop. </summary>
	/// <returns> the time series for the matching crop, or null if the crop does not have a time series. </returns>
	/// <param name="crop_name"> The name of a crop to check for (just the crop name, without the leading "CropArea-"). </param>
	public virtual YearTS getCropPatternTS(string crop_name)
	{
		int pos = indexOf(crop_name);
		if (pos < 0)
		{
			return null;
		}
		else
		{
			return (YearTS)__tslist[pos];
		}
	}

	/// <summary>
	/// Returns __date1. </summary>
	/// <returns> __date1 </returns>
	public virtual DateTime getDate1()
	{
		return __date1;
	}

	/// <summary>
	/// Returns __date2. </summary>
	/// <returns> __date2. </returns>
	public virtual DateTime getDate2()
	{
		return __date2;
	}

	/// <summary>
	/// Return the parcels for a requested year and crop type.  These values can be used in data filling. </summary>
	/// <param name="year"> Parcel year of interest or <= number if all years should be returned. </param>
	/// <param name="crop"> Crop type of interest or null if all crops should be returned. </param>
	/// <returns> the list of StateCU_Parcel for a year </returns>
	public virtual IList<StateCU_Parcel> getParcelListForYearAndCropName(int year, string crop)
	{
		IList<StateCU_Parcel> parcels = new List<StateCU_Parcel>();
		int size = __parcel_Vector.Count;
		StateCU_Parcel parcel;
		for (int i = 0; i < size; i++)
		{
			parcel = __parcel_Vector[i];
			if ((year > 0) && (parcel.getYear() != year))
			{
				// Requested year does not match.
				continue;
			}
			if ((!string.ReferenceEquals(crop, null)) && !crop.Equals(parcel.getCrop(), StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}
			// Criteria are met
			parcels.Add(parcel);
		}
		return parcels;
	}

	/// <summary>
	/// Return the total area (acres) for a year. </summary>
	/// <returns> the total area (acres) for a year or -999.0 if the year is outside the period of record. </returns>
	public virtual double getTotalArea(int year)
	{
		if ((year < __date1.getYear()) || (year > __date2.getYear()))
		{
			return -999.0;
		}
		return __total_area[year - __date1.getYear()];
	}

	/// <summary>
	/// Return the data units for the time series. </summary>
	/// <returns> the data units for the time series. </returns>
	public virtual string getUnits()
	{
		return __units;
	}

	/// <summary>
	/// Determine the index of a crop time series within the list based on the crop name. </summary>
	/// <param name="crop_name"> Crop to search for (without leading "CropArea-"). </param>
	/// <returns> index within the list (zero referenced) or -1 if not found. </returns>
	public virtual int indexOf(string crop_name)
	{
		int size = __tslist.Count;
		YearTS yts = null;
		for (int i = 0; i < size; i++)
		{
			yts = (YearTS)__tslist[i];
			if (yts.getDataType().equalsIgnoreCase("CropArea-" + crop_name))
			{
				return i;
			}
		}
		return -1;
	}

	/// <summary>
	/// Determine whether a StateCU file is a crop pattern time series file.
	/// Currently the only check is to see if the file has a "cds" extension. </summary>
	/// <param name="filename"> Name of file to examine. </param>
	/// <returns> true if the file is crop pattern time series file, false otherwise. </returns>
	public static bool isCropPatternTSFile(string filename)
	{
		string ext = IOUtil.getFileExtension(filename);
		if (ext.Equals("cds", StringComparison.OrdinalIgnoreCase))
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
	/// Checks for version 10 by reading the file and checking the length of the first
	/// data record.  The total length for version 12+ is 55 characters and for version
	/// 10 is 45.  Therefore, a data record length of 50 is considered version 12 (not version 10). </summary>
	/// <param name="filename"> Name of file to check. </param>
	/// <returns> true if the file is version 10, false if not (version 12+). </returns>
	/// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static boolean isVersion_10(String filename) throws java.io.IOException
	private static bool isVersion_10(string filename)
	{
		string fname = filename;
		string line = "";
		StreamReader input = null;

		// Read the StateCU file.  Only read the first line 
		// This is enough to know if it is version 10
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
		// The last line read above should be the header line with the
		// period.  Ignore it because there is some inconsistency in formatting
		// and read another line.  For really old files, a header line will not be used.
		if (isPeriodInHeader(filename))
		{
			while (true)
			{
				line = input.ReadLine();
				if (string.ReferenceEquals(line, null))
				{
					line = ""; // No data
					break;
				}
				else if (line.StartsWith("#", StringComparison.Ordinal))
				{
					continue; // Have read a comment so read another line
				}
				else
				{
					break; // Will use the line below
				}
			}
		}

		bool version10 = false;
		if (line.Length < 50)
		{
			// Assume version 10
			version10 = true;
		}
		else
		{
			// Assume version 12+ (not version 10)
			version10 = false;
		}

		input.Close();
		return version10;
	}

	/// <summary>
	/// Read the StateCU CDS file and return as a Vector of StateCU_CropPattern. </summary>
	/// <param name="filename"> filename containing CDS records. </param>
	/// <param name="date1_req"> Requested start of period. </param>
	/// <param name="date2_req"> Requested end of period. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.List<StateCU_CropPatternTS> readStateCUFile(String filename, RTi.Util.Time.DateTime date1_req, RTi.Util.Time.DateTime date2_req) throws Exception
	public static IList<StateCU_CropPatternTS> readStateCUFile(string filename, DateTime date1_req, DateTime date2_req)
	{
		return readStateCUFile(filename, date1_req, date2_req, null);
	}

	/// <summary>
	/// Read the StateCU CDS file and return as a Vector of StateCU_CropPattern. </summary>
	/// <param name="filename"> filename containing CDS records. </param>
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
	/// most current format will be read.  This is used for backward compatibility.</td>
	/// <td>True</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>ReadDataFrom</b></td>
	/// <td>If "CropArea", crop area time series will be read from the individual acreage
	/// values - this should be used for newer software.  If "TotalAndCropFraction", the
	/// individual crops will be computed from the total multiplied by the fractions - this
	/// should only be used when processing older files.</td>
	/// <td>True</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>AutoAdjust</b></td>
	/// <td>If "True", automatically adjust the following information when reading the file:
	/// <ol>
	/// <li>	Crop data types with "." - replace with "-".</li>
	/// </ol>
	/// </td>
	/// <td>True</td>
	/// </tr>
	/// </table> </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.List<StateCU_CropPatternTS> readStateCUFile(String filename, RTi.Util.Time.DateTime date1_req, RTi.Util.Time.DateTime date2_req, RTi.Util.IO.PropList props) throws Exception
	public static IList<StateCU_CropPatternTS> readStateCUFile(string filename, DateTime date1_req, DateTime date2_req, PropList props)
	{
		string routine = "StateCU_CropPatternTS.readStateCUFile";
		string iline = null;
		IList<object> v = new List<object> (5);
		IList<StateCU_CropPatternTS> cupat_Vector = new List<StateCU_CropPatternTS> (100);

		string full_filename = IOUtil.getPathUsingWorkingDir(filename);
		Message.printStatus(2,routine, "Reading StateCU CDS file: " + full_filename);

		if (props == null)
		{
			props = new PropList("CDS");
		}
		// check the version
		string Version = props.getValue("Version");
		bool version10 = false;
		// TODO SAM 2007-02-18 Evaluate phasing out property - default the check
		if ((!string.ReferenceEquals(Version, null)) && Version.Equals("10", StringComparison.OrdinalIgnoreCase))
		{
			version10 = true;
		}
		else if (isVersion_10(full_filename))
		{ // Automatically check for the old format.
			Message.printStatus(2, routine, "Format of file was found to be" + " version 10.  Will use old format for reading.");
			version10 = true;
		}

		// If early versions (earlier than 10?), the period is not in the header
		// so determine by reading the first and last part of the file...
		// TODO SAM 2007-03-04 Need to figure out what version the addition
		// of period to the header occurred.  It seems to have been in version 10 and later?
		bool period_in_header = isPeriodInHeader(full_filename);
		string AutoAdjust = props.getValue("AutoAdjust");
		bool AutoAdjust_boolean = false;
		if ((!string.ReferenceEquals(AutoAdjust, null)) && AutoAdjust.Equals("True", StringComparison.OrdinalIgnoreCase))
		{
			AutoAdjust_boolean = true;
		}

		// Check how to read the data...
		string ReadDataFrom = props.getValue("ReadDataFrom");
		bool ReadDataFrom_CropArea_boolean = true; // Default
		if ((!string.ReferenceEquals(ReadDataFrom, null)) && ReadDataFrom.Equals("TotalAndCropFraction", StringComparison.OrdinalIgnoreCase))
		{
			ReadDataFrom_CropArea_boolean = false;
		}

		// If an older file version, automatically adjust to the old setting...
		if (version10 || !period_in_header)
		{
			if (ReadDataFrom_CropArea_boolean)
			{
				ReadDataFrom_CropArea_boolean = false;
			}
		}
		if (ReadDataFrom_CropArea_boolean)
		{
			Message.printStatus(2, routine, "Reading crop area time series from individual time series (NOT fraction).");
		}
		else
		{
			Message.printStatus(2, routine, "Reading crop area time series from total acres and fraction.");
		}

		// Format specifiers for defaults (version 12).
		int[] format_0 = new int[] {StringUtil.TYPE_STRING, StringUtil.TYPE_SPACE, StringUtil.TYPE_STRING, StringUtil.TYPE_SPACE, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING};

		int[] format_0w = new int[] {4, 1, 12, 18, 10, 10};

		if (!period_in_header)
		{
			// Use the even older format (what version?)...
				format_0 = new int [6];
				format_0[0] = StringUtil.TYPE_STRING;
				format_0[1] = StringUtil.TYPE_SPACE;
				format_0[2] = StringUtil.TYPE_STRING;
				format_0[3] = StringUtil.TYPE_SPACE;
				format_0[4] = StringUtil.TYPE_STRING;
				//format_0[5] = StringUtil.TYPE_STRING;

				format_0w = new int[6];
				format_0w[0] = 4; // Year
				format_0w[1] = 1; // Space
				format_0w[2] = 12; // CU Location ID
				format_0w[3] = 3; // Space
				format_0w[4] = 10; // Total area
				//format_0w[5] = 10;	// Number of crops (not in data)
		}
		else if (version10)
		{
			// Use the older format...
			format_0 = new int [6];
			format_0[0] = StringUtil.TYPE_STRING;
			format_0[1] = StringUtil.TYPE_SPACE;
			format_0[2] = StringUtil.TYPE_STRING;
			format_0[3] = StringUtil.TYPE_SPACE;
			format_0[4] = StringUtil.TYPE_STRING;
			format_0[5] = StringUtil.TYPE_STRING;

			format_0w = new int[6];
			format_0w[0] = 4; // Year
			format_0w[1] = 1; // Space
			format_0w[2] = 12; // CU Location ID
			format_0w[3] = 8; // Space
			format_0w[4] = 10; // Total area
			format_0w[5] = 10; // Number of crops
		}

		// Data records within a location.
		// Newest (version 12) format specifiers for crop record, including the crop fraction and area...
		int[] format_1 = new int[] {StringUtil.TYPE_SPACE, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING};
		int[] format_1w = new int[] {5, 30, 10, 10};
		if (!ReadDataFrom_CropArea_boolean)
		{
			// Read the crop area from the total and fraction (no need to read crop area).
			// This was the normal way of doing it until 2007-10-02
			format_1 = new int[3];
			format_1[0] = StringUtil.TYPE_SPACE;
			format_1[1] = StringUtil.TYPE_STRING; // Crop name
			format_1[2] = StringUtil.TYPE_STRING; // Crop fraction

			format_1w = new int[3];
			format_1w[0] = 5; // Spaces
			format_1w[1] = 30; // Crop name
			format_1w[2] = 10; // Crop fraction
		}

		if (!period_in_header)
		{
			// Even older format
			format_1w[0] = 4;
			format_1w[1] = 20;
			format_1w[2] = 10;
		}
		else if (version10)
		{
			// Use the older format...
			format_1w[0] = 5;
			format_1w[1] = 20;
			format_1w[2] = 10;
		}

		StateCU_CropPatternTS cupat = null;
		StreamReader @in = null;

		DateTime date1 = null, date2 = null;
		DateTime date1_file = null;
		DateTime date2_file = null;
		string units_file = "";
		int year1 = -1, year = 0; // Year being processed
		//TODO SAM 2007-02-18 Evaluate why requested years are not used.
		//	year1_req = -1,		// Requested first year to process
						// (null requested date = read all)
		//	year2_req = -1;		// Requested last year to process

		if (!period_in_header)
		{
			// Version 10 and older files do not have header information
			// with the period for the time series.  Therefore, grab a
			// reasonable amount of the start and end of the file - then
			// read lines (broken by line breaks) until the last data
			// line is encountered...

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

			Message.printStatus(2, routine, "No period in file header.  Period determined from data to be " + date1_file + " to " + date2_file);

			if (date1_req != null)
			{
				date1 = date1_req;
				//year1_req = date1_req.getYear();
			}
			else
			{
				date1 = date1_file;
				//year1_req = date1_file.getYear();
			}
			if (date2_req != null)
			{
				date2 = date2_req;
				//year2_req = date2_req.getYear();
			}
			else
			{
				date2 = date2_file;
				//year2_req = date2_file.getYear();
			}

			year1 = date1_file.getYear();
			units_file = "ACRE";
		}

		// The following throws an IOException if the file cannot be opened...
		@in = new StreamReader(full_filename);
		int ncrops = 0;
		string[] crop_names = new string[50]; // Should never exceed
		double[] crop_fractions = new double[50]; // Should never exceed
		double[] crop_areas = new double[50]; // Should never exceed
		string total_area = "";
		string culoc = "";
		int pos = 0;
		int linecount = 0;
		IList<object> tokens;
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
			// the first line is header with the period, etc.)...
			//if ( !version10 && date1_file == null ) {
			if (date1_file == null)
			{
				// Treat all as strings for initial read...
				// Header format is the following, although start and
				// end months and the year type are ignored...
				// "  Header format (i5,1x,i4,5x,i5,1x,i4,a5,a5)" );
				//"    M/YYYY        MM/YYYY UNIT  CYR"
				string format_header = "x6s4x11s4s5s5";
				tokens = StringUtil.fixedRead(iline, format_header);
				date1_file = new DateTime(DateTime.PRECISION_YEAR);
				date1_file.setYear(StringUtil.atoi(((string)tokens[0]).Trim()));
				year1 = date1_file.getYear();
				date2_file = new DateTime(DateTime.PRECISION_YEAR);
				date2_file.setYear(StringUtil.atoi(((string)tokens[1]).Trim()));

				if (iline.StartsWith("      ", StringComparison.Ordinal))
				{
					// this is the newest format - have startYear, endYear, units
					IList<string> toks = StringUtil.breakStringList(iline, " ", StringUtil.DELIM_SKIP_BLANKS);
					units_file = toks[2];
				}
				else
				{
					units_file = ((string)tokens[2]).Trim();
				}
				Message.printStatus(2, routine, "Units from file are \"" + units_file + "\"");
				// Year type is ignored - not used for anything - will be output as CYR when writing.

				// Set the dates for processing...
				if (date1_req != null)
				{
					date1 = date1_req;
					//year1_req = date1_req.getYear();
				}
				else
				{
					date1 = date1_file;
					//year1_req = date1_file.getYear();
				}
				if (date2_req != null)
				{
					date2 = date2_req;
					//year2_req = date2_req.getYear();
				}
				else
				{
					date2 = date2_file;
					//year2_req = date2_file.getYear();
				}
				continue;
			}

			if (iline[0] == ' ')
			{
				// Continuation of previous Year/CULocation, indicating another crop.
				StringUtil.fixedRead(iline, format_1, format_1w, v);
				if (AutoAdjust_boolean)
				{
					// Replace "." with "-" in the crop names.
					crop_names[ncrops] = ((string)v[0]).Trim().Replace('.','-');
				}
				else
				{
					// Just use the crop name as it is in the file...
					crop_names[ncrops] = ((string)v[0]).Trim();
				}
				crop_fractions[ncrops] = StringUtil.atod(((string)v[1]).Trim());
				if (ReadDataFrom_CropArea_boolean)
				{
					// Additionally, read the crop area...
					crop_areas[ncrops] = StringUtil.atod(((string)v[2]).Trim());
				}
				// Increment crop count.
				++ncrops;
			}
			else
			{
				// Assume a new year/CULocation...
				// If the number of crops from the previous record is
				// >= 0, set the crops (if the total is zero then the total will still be set)...
				if ((ncrops >= 0) && (cupat != null))
				{
					if (ReadDataFrom_CropArea_boolean)
					{
						cupat.setPatternUsingAreas(year, ncrops, crop_names, crop_areas);
					}
					else
					{
						cupat.setPatternUsingFractions(year, StringUtil.atod(total_area), ncrops, crop_names, crop_fractions);
					}
				}
				// Now process the new data...
				StringUtil.fixedRead(iline, format_0, format_0w, v);
				year = StringUtil.atoi(((string)v[0]).Trim());
				// TODO SAM 2005-11-30 Need to optimize code
				// Need to optimize code here to quit reading if
				// requested period is done or have not reached the
				// start of the requested period.   The problem is that
				// the code only saves data when a new year/crop is
				// detected so probably need to always try to process
				// one year before the requested period.
				culoc = ((string)v[1]).Trim();
				total_area = ((string)v[2]).Trim();
				// Important... ncrops from the file is not actually
				// used.  Instead the space at the beginning of lines is
				// used to indicate crops.  This works for Version 10
				// and newer formats.
				ncrops = 0;
				if (year == year1)
				{
					// Create an object for the CU Location.  It is
					// assumed that a structure is listed for each
					// year, even if it has zero crops for a year.
					pos = StateCU_Util.IndexOf(cupat_Vector, culoc);
					if (pos >= 0)
					{
						// Should not happen!  The CU Location is apparently listed twice in the
						// first year...
						Message.printWarning(1, routine, "CU Location \"" + culoc + "\" is listed more than once in the first year.");
						cupat = (StateCU_CropPatternTS)cupat_Vector[pos];
					}
					else
					{
						cupat = new StateCU_CropPatternTS(culoc, date1, date2, units_file, full_filename);
						cupat_Vector.Add(cupat);
						//Message.printStatus ( 1, "", "SAMX created new StateCU_CropPatternTS:" + cupat );
					}
				}
				else
				{
					// Find the object of interest for this CU Location so it can be used to set data
					// values...
					pos = StateCU_Util.IndexOf(cupat_Vector, culoc);
					if (pos < 0)
					{
						// Should not happen!  Apparently the CU Location was not listed in the first year...
						Message.printWarning(3, routine, "CU Location \"" + culoc + "\" found in year " + year + " but was not listed in the first year.");
						cupat = new StateCU_CropPatternTS(culoc, date1, date2, units_file, full_filename);
						cupat_Vector.Add(cupat);
						//Message.printStatus ( 1, "", "SAMX created new StateCU_CropPatternTS:" + cupat );
					}
					else
					{
						cupat = (StateCU_CropPatternTS)cupat_Vector[pos];
					}
				}
			}
		}
		// Process the data for the last CU Location that is read...
		if (ncrops >= 0)
		{
			if (ReadDataFrom_CropArea_boolean)
			{
				cupat.setPatternUsingAreas(year, ncrops, crop_names, crop_areas);
			}
			else
			{
				cupat.setPatternUsingFractions(year, StringUtil.atod(total_area), ncrops, crop_names, crop_fractions);
			}
		}
		}
		catch (Exception e)
		{
			Message.printWarning(2, routine, "Error processing near line " + linecount + ": " + iline);
			Message.printWarning(2, routine, e);
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
		return cupat_Vector;
	}

	/// <summary>
	/// Read a time series from a StateCU format file, using a time series identifier.
	/// The TSID string is specified in addition to the path to the file.  It is
	/// expected that a TSID in the file
	/// matches the TSID (and the path to the file, if included in the TSID would not
	/// properly allow the TSID to be specified).  This method can be used with newer
	/// code where the I/O path is separate from the TSID that is used to identify the time series.
	/// The IOUtil.getPathUsingWorkingDir() method is applied to the filename. </summary>
	/// <returns> a pointer to a newly-allocated time series if successful, a NULL pointer if not. </returns>
	/// <param name="tsident_string"> The full identifier for the time series to
	/// read.  This string can also be the alias for the time series in the file. </param>
	/// <param name="filename"> The name of a file to read
	/// (in which case the tsident_string must match one of the TSID strings in the file). </param>
	/// <param name="date1"> Starting date to initialize period (null to read the entire time
	/// series). </param>
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
	/// <returns> a pointer to a newly-allocated list of time series if successful, a NULL pointer if not. </returns>
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
	/// return all new time series in the vector.  If not null, return the matching time series. </param>
	/// <param name="full_filename"> Full path to filename, used for messages. </param>
	/// <param name="req_date1"> Requested starting date to initialize period (or NULL to read the entire time series). </param>
	/// <param name="req_date2"> Requested ending date to initialize period (or NULL to read the entire time series). </param>
	/// <param name="units"> Units to convert to (currently ignored). </param>
	/// <param name="read_data"> Indicates whether data should be read. </param>
	/// <exception cref="Exception"> if there is an error reading the time series. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static java.util.List<RTi.TS.TS> readTimeSeriesList(String req_tsident, String full_filename, RTi.Util.Time.DateTime req_date1, RTi.Util.Time.DateTime req_date2, String req_units, boolean read_data) throws Exception
	private static IList<TS> readTimeSeriesList(string req_tsident, string full_filename, DateTime req_date1, DateTime req_date2, string req_units, bool read_data)
	{ // TODO - can optimize this later to only read one time series...
		// First read the whole file...

		IList<StateCU_CropPatternTS> data_Vector = readStateCUFile(full_filename, req_date1, req_date2);
		// If all the time series are required, return all...
		int size = 0;
		if (data_Vector != null)
		{
			size = data_Vector.Count;
		}
		// Guess at non-zero size (assume 1.5 crops per structure)...
		IList<TS> tslist = new List<TS>((size + 1) * 3 / 2);
		StateCU_CropPatternTS cds;
		int nts = 0, j;
		TSIdent tsident = null;
		if (!string.ReferenceEquals(req_tsident, null))
		{
			tsident = new TSIdent(req_tsident);
		}
		TS ts;
		bool include_ts = true;
		for (int i = 0; i < size; i++)
		{
			include_ts = true;
			cds = data_Vector[i];
			if (!string.ReferenceEquals(req_tsident, null))
			{
				// Check to see if the location match...
				if (!cds.getID().Equals(tsident.getLocation(), StringComparison.OrdinalIgnoreCase))
				{
					include_ts = false;
				}
			}
			if (!include_ts)
			{
				continue;
			}
			nts = cds.__tslist.Count;
			for (j = 0; j < nts; j++)
			{
				// TODO - optimize this by evaluating when reading the file...
				ts = cds.__tslist[j];
				if (!string.ReferenceEquals(req_tsident, null))
				{
					// Check to see if the location and data type match...
					if (!tsident.getType().equalsIgnoreCase(ts.getDataType()))
					{
						continue;
					}
				}
				tslist.Add(ts);
			}
		}
		return tslist;
	}

	/// <summary>
	/// Recalculate the total values for the structure for each year of data.  This
	/// method should be called if individual time series values are manipulated outside
	/// of file read methods.  Missing will be assigned if all the component time series
	/// are missing.  If no time series are available, the total will remain the same as
	/// previous, as determined by other code.
	/// </summary>
	public virtual void refresh()
	{
		int year1 = __date1.getYear();
		int year2 = __date2.getYear();
		double total_area = 0.0;
		double area = 0.0;
		YearTS yts = null;
		int size = __tslist.Count;
		for (int year = year1; year <= year2; year++)
		{
			__temp_DateTime.setYear(year);
			total_area = -999.0;
			for (int i = 0; i < size; i++)
			{
				yts = (YearTS)__tslist[i];
				area = yts.getDataValue(__temp_DateTime);
				if (!yts.isDataMissing(area))
				{
					if (total_area < 0.0)
					{
						// Total is missing so assign...
						total_area = area;
					}
					else
					{
						// Total is not missing so increment.
						total_area += area;
					}
				}
			}
			if (size != 0)
			{
				__total_area[year - year1] = total_area;
			}
			// Otherwise leave the total as missing or zero as previous
		}
	}

	/// <summary>
	/// Remove all the time series from the object.  This can be used, for example, when
	/// resetting the time series to override a read.
	/// </summary>
	public virtual void removeAllTS()
	{
		if (__crop_name_Vector != null)
		{
			__crop_name_Vector.Clear();
		}
		if (__tslist != null)
		{
			__tslist.Clear();
		}
	}

	/// <summary>
	/// Remove a the time series for a crop name. </summary>
	/// <param name="crop_name"> Crop name to remove. </param>
	public virtual void removeCropName(string crop_name)
	{
		int size = 0;
		if (__crop_name_Vector != null)
		{
			size = __crop_name_Vector.Count;
		}
		// Remove from the crop names Vector...
		for (int i = 0; i < size; i++)
		{
			if (((string)__crop_name_Vector[i]).Equals(crop_name, StringComparison.OrdinalIgnoreCase))
			{
				// Remove the crop name...
				__crop_name_Vector.RemoveAt(i);
				--i;
				--size;
			}
		}
		// Remove from the time series Vector...
		size = 0;
		if (__tslist != null)
		{
			size = __tslist.Count;
		}
		TS ts;
		for (int i = 0; i < size; i++)
		{
			ts = (TS)__tslist[i];
			if (ts.getIdentifier().getSubType().equalsIgnoreCase(crop_name))
			{
				__tslist.RemoveAt(i);
				--i;
				--size;
			}
		}
	}

	/// <summary>
	/// Set the area for a crop and year.  This method will NOT add a new crop.
	/// Trying to set data outside the period will cause the value to be ignored (the
	/// period will not be extended).
	/// The total for all crops in the year will be reset to reflect the new value. </summary>
	/// <param name="crop_name"> Crop name. </param>
	/// <param name="year"> Year to set the area. </param>
	/// <param name="area"> Area for the crop, for the given year. </param>
	/// <exception cref="Exception"> if there is an error setting the data (e.g., the time
	/// series for the crop cannot be found). </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void setCropArea(String crop_name, int year, double area) throws Exception
	public virtual void setCropArea(string crop_name, int year, double area)
	{
		if ((year < __date1.getYear()) || (year > __date2.getYear()))
		{
			return;
		}
		// First find the time series...
		YearTS yts = getCropPatternTS(crop_name);
		if (yts == null)
		{
			throw new Exception("Unable to find time series for \"" + crop_name + "\"");
		}
		// Set the data value...
		__temp_DateTime.setYear(year);
		double old_total = getTotalArea(year);
		double old_value = yts.getDataValue(__temp_DateTime);
		yts.setDataValue(__temp_DateTime, area);
		// Reset the total by adjusting the old value (this performs better than
		// looping through the crop time series)...
		if (yts.isDataMissing(old_value) || yts.isDataMissing(area))
		{
			// Set to the new value...
			__total_area[year - __date1.getYear()] = area;
		}
		else
		{
			// Adjust the old value...
			__total_area[year - __date1.getYear()] = old_total - old_value + area;
		}
	}

	/// <summary>
	/// Set the areas for each crop to zero.  This is useful, for example, when crop
	/// patterns are being processed from individual records and any record in a year
	/// should cause other time series to be set to zero for the year.  A later reset
	/// of the zero can occur without issue.  However, leaving the value as -999 may
	/// result in unexpected filled values later. </summary>
	/// <param name="year"> The year to set data.  If negative, all years with non-missing values are processed. </param>
	/// <param name="set_all"> If true, then all defined time series are set to zero.  If false,
	/// then only missing values are set to zero. </param>
	public virtual void setCropAreasToZero(int year, bool set_all)
	{
		int size = 0;
		if (__tslist != null)
		{
			size = __tslist.Count;
		}
		YearTS yts = null;
		// Default to process one year
		if ((year < __date1.getYear()) || (year > __date2.getYear()))
		{
			// No need to process...
			return;
		}
		int year1 = year;
		int year2 = year;
		if (year < 0)
		{
			// Process all data
			year1 = __date1.getYear();
			year2 = __date2.getYear();
		}
		// Loop through the requested period.
		for (int iyear = year1; iyear <= year2; iyear++)
		{
			__temp_DateTime.setYear(iyear);
			if (size == 0)
			{
				// No time series so set the total to zero.
				__total_area[iyear - __date1.getYear()] = 0.0;
				Message.printStatus(2, "StateCU_CropPatternTS.setCropAreasToZero", "Setting " + _id + " " + iyear + " crop total to zero since no crops.");
			}
			else
			{ // Process each time series...
				for (int i = 0; i < size; i++)
				{
					yts = (YearTS)__tslist[i];
					if (set_all || yts.isDataMissing(yts.getDataValue(__temp_DateTime)))
					{
						yts.setDataValue(__temp_DateTime, 0.0);
						Message.printStatus(2, "StateCU_CropPatternTS.setCropAreasToZero", "Setting " + _id + " " + iyear + " crop " + yts.getDataType() + " to zero.");
					}
				}
			}
		}
	}

	/// <summary>
	/// Set the pattern for a crop for a given year by specifying the areas for each crop. </summary>
	/// <param name="year"> Year for the crop data. </param>
	/// <param name="ncrops"> Number of crops that should be processed. </param>
	/// <param name="crop_names"> List of crops that are planted. </param>
	/// <param name="crop_areas"> Areas planted for each crop, acres. </param>
	public virtual void setPatternUsingAreas(int year, int ncrops, string[] crop_names, double[] crop_areas)
	{
		if ((year < __date1.getYear()) || (year > __date2.getYear()))
		{
			return;
		}
		__temp_DateTime.setYear(year);
		int size = 0;
		if (crop_names != null)
		{
			size = crop_names.Length;
		}
		if (ncrops < size)
		{
			size = ncrops;
		}
		double total_area = -999.0;
		if (ncrops == 0)
		{
			// No crops.
			total_area = 0.0;
		}
		for (int i = 0; i < size; i++)
		{
			int pos = indexOf(crop_names[i]);
			YearTS yts = null;
			if (pos < 0)
			{
				// Add a new time series...
				yts = addTS(crop_names[i], true);
			}
			else if (pos >= 0)
			{
				yts = (YearTS)__tslist[pos];
			}
			yts.setDataValue(__temp_DateTime, crop_areas[i]);
			// FIXME SAM 2007-10-03 Need to consolidate into refresh(year) and reuse code
			if (total_area < 0.0)
			{
				// Set the value (ok even if crop area is missing)...
				total_area = crop_areas[i];
			}
			else
			{
				// Add the value (but only if not missing)...
				if (crop_areas[i] >= 0.0)
				{
					total_area += crop_areas[i];
				}
			}
		}
		__total_area[year - __date1.getYear()] = total_area;
	}

	/// <summary>
	/// Set the pattern for crops for a given year by specifying a total area and
	/// distribution in fractions.  If the year is outside the period, then no action occurs. </summary>
	/// <param name="year"> Year for the crop data. </param>
	/// <param name="ncrops"> Number of crops that should be processed. </param>
	/// <param name="total_area"> Total area that is cultivated (acres). </param>
	/// <param name="crop_names"> List of crops that are planted. </param>
	/// <param name="crop_fractions"> Fractions for each crop (0.0 to 1.0). </param>
	public virtual void setPatternUsingFractions(int year, double total_area, int ncrops, string[] crop_names, double[] crop_fractions)
	{
		__temp_DateTime.setYear(year);
		if ((year < __date1.getYear()) || (year > __date2.getYear()))
		{
			return;
		}
		int size = 0;
		if (crop_names != null)
		{
			size = crop_names.Length;
		}
		if (ncrops < size)
		{
			size = ncrops;
		}
		//Message.printStatus ( 1, "", "SAMX: Setting " + year + " to " + total_area + " date1=" + __date1 );
		__total_area[year - __date1.getYear()] = total_area;
		YearTS yts = null;
		for (int i = 0; i < size; i++)
		{
			int pos = indexOf(crop_names[i]);
			if (pos < 0)
			{
				// Add a new time series...
				yts = addTS(crop_names[i], true);
			}
			else if (pos >= 0)
			{
				yts = (YearTS)__tslist[pos];
			}
			yts.setDataValue(__temp_DateTime,total_area * crop_fractions[i]);
		}
	}

	/// <summary>
	/// Set the total crop area.  This will adjust the areas of each crop
	/// proportionally, using the initial fractions.  This method is usually only called
	/// when data are being reset based on some knowledge of the total acres.
	/// If the old total is zero, all the crops will be zero to zero area.  This method
	/// should NOT be called by other methods that result in a call to this method or
	/// an infinite loop will occur. </summary>
	/// <param name="year"> Year to set the total area. </param>
	/// <param name="total_area"> Total area for the CU location. </param>
	/// <exception cref="Exception"> If there is no initial crop data defined. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void setTotalArea(int year, double total_area) throws Exception
	public virtual void setTotalArea(int year, double total_area)
	{
		double old_total_area = getTotalArea(year);
		int ncrops = __crop_name_Vector.Count;
		if (old_total_area <= 0.0)
		{
			// TODO SAM 2007-06-20 Evaluate how to handle when all crops are zero or missing.
			if (ncrops > 0)
			{
				// Do not have crop data to prorate...
				Message.printWarning(2, "StateCU_CropPatternTS.setTotalArea", "No initial crop data for \"" + _id + "\".  Cannot prorate crops to new total " + StringUtil.formatString(total_area,"%.3f") + ".");
				throw new Exception("Unable to set total area for \"" + _id + "\"");
			}
			else
			{
				// No previous crops so just set the total (probably zero).
				__total_area[year - __date1.getYear()] = total_area;
			}
		}
		else
		{
			// Modify the existing areas by the factor...
			double factor = total_area / old_total_area;
			// Loop through the crops...
			string crop_name;
			double area;
			for (int i = 0; i < ncrops; i++)
			{
				crop_name = (string)__crop_name_Vector[i];
				area = getCropArea(crop_name, year, false);
				setCropArea(crop_name, year, area * factor);
			}
		}
	}

	/// <summary>
	/// Return a string representation of this object (the crop name list as a string). </summary>
	/// <returns> a string representation of this object. </returns>
	public override string ToString()
	{
		return _id + " " + __crop_name_Vector;
	}

	/// <summary>
	/// Return a list containing all the time series in the data list.  Only the raw
	/// time series are returned.  Use the overloaded version to also return total time series. </summary>
	/// <returns> a list containing all the time series in the data list. </returns>
	/// <param name="dataList"> A list of StateCU_CropPatternTS. </param>
	public static IList<TS> toTSList(IList<StateCU_CropPatternTS> dataList)
	{
		return toTSList(dataList, false, false, null, null);
	}

	/// <summary>
	/// Return a list containing all the time series in the data list.
	/// Optionally, process the time series in the instance and add total time series
	/// by location and for the entire data set.
	/// This is a performance hit but is useful for summarizing the data.  Any non-zero
	/// value in the individual time series will result in a value in the total.
	/// Missing for all time series will result in missing in the total.  The period for
	/// the totals is the overall period from all StateCU_CropPatternTS being processed. </summary>
	/// <returns> a list containing all the time series in the data list. </returns>
	/// <param name="dataList"> A list of CropPatternTS. </param>
	/// <param name="include_location_totals"> If true, include totals for each location, equal
	/// to the sum of the acreage for all crops. </param>
	/// <param name="include_dataset_totals"> If true, include totals for the entire data set,
	/// equal to the sum of the acreage for all locations, by crop, and in total. </param>
	/// <param name="dataset_location"> A string used as the location for the data set totals.
	/// If not specified, "DataSet" will be used.  A non-null value should be supplied,
	/// in particular, if the totals for different data sets will be graphed or manipulated. </param>
	/// <param name="dataset_datasource"> Data source to be used for the total time series.
	/// If not specified, "StateCU" will be used. </param>
	public static IList<TS> toTSList(IList<StateCU_CropPatternTS> dataList, bool include_location_totals, bool include_dataset_totals, string dataset_location, string dataset_datasource)
	{
		string routine = "StateCU_CropPatternTS.toTSList";
		IList<TS> tslist = new List<TS>();
		int size = 0;
		if (dataList != null)
		{
			size = dataList.Count;
		}
		StateCU_CropPatternTS cds = null;
		IList<string> distinct_crop_names = null; // For data set totals.
		string crop_name, crop_name2; // Single crop name.
		int ndistinct_crops = 0; // For data set totals.
		DateTime start_DateTime = null, end_DateTime = null, date; // To allocate new time series.
		int end_year = 0, start_year = 0, year; // For data set totals.
		YearTS yts = null, yts2; // For data set totals.
		string units = ""; // Units for new time series.
		IList<TS> dataset_ts_Vector = null; // List of data set total time series.
		int j, k, nts;
		if (include_dataset_totals)
		{
			// Get a list of unique crops in the time series list...
			distinct_crop_names = getCropNames(dataList);
			// Add for the total...
			distinct_crop_names.Add("AllCrops");
			ndistinct_crops = distinct_crop_names.Count;
			// Set the data set location if not provided...
			if ((string.ReferenceEquals(dataset_location, null)) || (dataset_location.Length == 0))
			{
				dataset_location = "DataSet";
			}
			if ((string.ReferenceEquals(dataset_datasource, null)) || (dataset_datasource.Length == 0))
			{
				dataset_datasource = "StateCU";
			}
			// Determine the period to use for new time series...
			for (int i = 0; i < size; i++)
			{
				cds = dataList[i];
				date = cds.getDate1();
				if ((start_DateTime == null) || date.lessThan(start_DateTime))
				{
					start_DateTime = new DateTime(date);
				}
				start_year = start_DateTime.getYear();
				date = cds.getDate2();
				if ((end_DateTime == null) || date.greaterThan(end_DateTime))
				{
					end_DateTime = new DateTime(date);
				}
				end_year = end_DateTime.getYear();
				if (!string.ReferenceEquals(cds.getUnits(), null))
				{
					units = cds.getUnits();
				}
			}
			// Add a time series for each distinct crop and for the data set total...
			dataset_ts_Vector = new List<TS>(ndistinct_crops);
			for (j = 0; j < ndistinct_crops; j++)
			{
				// Add a new time series for all the distinct crops...
				crop_name = (string)distinct_crop_names[j];
				yts = new YearTS();
				try
				{
					TSIdent tsident = new TSIdent(dataset_location, dataset_datasource, "CropArea-" + crop_name, "Year", "");
					yts.setIdentifier(tsident);
					yts.getIdentifier().setInputType("StateCU");
				}
				catch (Exception)
				{
					// This should NOT happen because the TSID is being controlled...
					Message.printWarning(3, routine, "Error adding time series for \"" + crop_name + "\"");
				}
				yts.setDataUnits(units);
				yts.setDataUnitsOriginal(units);
				yts.setDescription(dataset_location + " " + crop_name + " crop area");
				yts.setDate1(new DateTime(start_DateTime));
				yts.setDate2(new DateTime(end_DateTime));
				yts.setDate1Original(new DateTime(start_DateTime));
				yts.setDate2Original(new DateTime(end_DateTime));
				yts.allocateDataSpace();
				dataset_ts_Vector.Add(yts);
			}
		}
		for (int i = 0; i < size; i++)
		{
			cds = (StateCU_CropPatternTS)dataList[i];
			nts = cds.__tslist.Count;
			for (j = 0; j < nts; j++)
			{
				yts = (YearTS)cds.__tslist[j];
				tslist.Add(yts);
				crop_name = yts.getDataType();
				if (include_dataset_totals)
				{
					// Add to the data set total...
					for (k = 0; k < ndistinct_crops; k++)
					{
						// Need to concatenate to compare...
						crop_name2 = "CropArea-" + (string)distinct_crop_names[k];
						if (crop_name2.Equals(crop_name, StringComparison.OrdinalIgnoreCase))
						{
							// Matching crop name - add it...
							yts2 = (YearTS)dataset_ts_Vector[k];
							try
							{
								TSUtil.add(yts2, yts);
							}
							catch (Exception)
							{
								Message.printWarning(3, routine, "Error adding time series.");
							}
						}
					}
					// Add to the overall total...
					yts2 = (YearTS)dataset_ts_Vector[ndistinct_crops - 1];
					try
					{
						TSUtil.add(yts2, yts);
					}
					catch (Exception)
					{
						Message.printWarning(3, routine, "Error adding time series.");
					}
				}
			}
			if (include_location_totals)
			{
				// Insert a new time series with the total acreage for
				// the location.  Add after the time series for the location...
				crop_name = "AllCrops";
				yts = new YearTS();
				try
				{
					TSIdent tsident = new TSIdent(cds.getID(), dataset_datasource, "CropArea-" + crop_name, "Year", "");
					yts.setIdentifier(tsident);
					yts.getIdentifier().setInputType("StateCU");
				}
				catch (Exception)
				{
					// This should NOT happen because the TSID is being controlled...
					Message.printWarning(3, routine, "Error adding time series for \"" + crop_name + "\"");
				}
				yts.setDataUnits(units);
				yts.setDataUnitsOriginal(units);
				yts.setDescription(cds.getID() + " " + crop_name + " area");
				yts.setDate1(new DateTime(start_DateTime));
				yts.setDate2(new DateTime(end_DateTime));
				yts.setDate1Original(new DateTime(start_DateTime));
				yts.setDate2Original(new DateTime(end_DateTime));
				yts.allocateDataSpace();
				// No need to add because the totals are maintained with
				// the data.  Just assign the values.
				for (date = new DateTime(start_DateTime), year = date.getYear(); year <= end_year; date.addYear(1), year++)
				{
					yts.setDataValue(date, cds.__total_area[year - start_year]);
				}
				tslist.Add(yts);
			}
		}
		if (include_dataset_totals)
		{
			// Insert the time series with the total acreage for
			// the data set, by crop and overall total.  Do this after all
			// other time series have been added.  Also reset the description to be more concise.
			for (j = 0; j < ndistinct_crops; j++)
			{
				yts = (YearTS)dataset_ts_Vector[j];
				yts.setDescription(yts.getLocation() + " " + yts.getDataType());
				tslist.Add(yts);
			}
		}
		return tslist;
	}

	/// <summary>
	/// Translate the crop name from the current value to a new value.  The time series
	/// identifiers are adjusted.  This is used by StateDMI to correct crop names in
	/// input to those used in the DSS.  If a match of the old_crop_name is not found, then nothing occurs. </summary>
	/// <param name="old_crop_name"> Old crop name. </param>
	/// <param name="new_crop_name"> New crop name to be used. </param>
	/// <exception cref="Exception"> if there is an error (generally only if add fails when
	/// the new name is the same as an existing name). </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void translateCropName(String old_crop_name, String new_crop_name) throws Exception
	public virtual void translateCropName(string old_crop_name, string new_crop_name)
	{
		string routine = "StateCU_CropPatternTS.translateCropName";
		int size = 0;
		if (__crop_name_Vector != null)
		{
			size = __crop_name_Vector.Count;
		}

		// Check to see if the new crop name is the same as an old crop name...

		int existing_crop_pos = -1; // Position matching new crop name -
						// will be >= 0 if the new name already is in use.
		for (int i = 0; i < size; i++)
		{
			if (((string)__crop_name_Vector[i]).Equals(new_crop_name, StringComparison.OrdinalIgnoreCase))
			{
				existing_crop_pos = i;
				Message.printStatus(2, routine, getID() + " new crop name matches an existing crop name. " + "Crop data for \"" + old_crop_name + "\" will be added to existing \"" + new_crop_name + "\"");
				break;
			}
		}

		// Reset in the crop names list...

		int old_crop_pos = -1; // The position of the old crop time series, before changing its name.
		bool found = false;
		for (int i = 0; i < size; i++)
		{
			if (((string)__crop_name_Vector[i]).Equals(old_crop_name, StringComparison.OrdinalIgnoreCase))
			{
				// Reset the crop name...
				found = true;
				__crop_name_Vector[i] = new_crop_name;
				// This crop either needs to be renamed or merged with another crop of the same name...
				if (existing_crop_pos >= 0)
				{
					old_crop_pos = i;
					break;
				}
			}
		}

		if (!found)
		{
			// No need to continue...
			return;
		}

		// Reset in the time series list...

		size = 0;
		if (__tslist != null)
		{
			size = __tslist.Count;
		}
		TS ts = null;
		// Search for and modify the time series...
		for (int i = 0; i < size; i++)
		{
			ts = (TS)__tslist[i];
			if (ts.getIdentifier().getSubType().equalsIgnoreCase(old_crop_name))
			{
				// This crop needs to be renamed, but only do so if
				// not merging.  If merging keep the same name so the add comments make sense
				if (existing_crop_pos < 0)
				{
					ts.getIdentifier().setSubType(new_crop_name);
					ts.addToGenesis("Translated \"" + ts.getLocation() + "\" crop name from \"" + old_crop_name + "\" to \"" + new_crop_name + "\"");
					ts.setDescription(ts.getLocation() + " " + new_crop_name + " crop area");
				}
				// Only allow one rename at a time...
				break;
			}
		}

		if (existing_crop_pos >= 0)
		{
			// Need to add the translated crop to the existing crop and
			// remove the translated crop.  The overall crop totals will
			// remain the same so their is no need to recompute the totals.
			TS ts_existing = (TS)__tslist[existing_crop_pos];
			TSUtil.add(ts_existing, ts);
			__tslist.RemoveAt(old_crop_pos);
			__crop_name_Vector.RemoveAt(old_crop_pos);
		}
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
		string id = getID();
		// Check major issues
		int year1 = __date1.getYear();
		int year2 = __date2.getYear();
		bool problemFound = false;
		if ((year1 <= 0) || (year2 <= 0))
		{
			validation.add(new StateCU_ComponentValidationProblem(this, "Location \"" + id + "\" period for crop pattern time series is not set.", "Verify that the time series are properly defined."));
			problemFound = true;
		}
		if (!problemFound)
		{
			// Did not find a major problem above so can continue checking time series
			double areaTotal;
			double area;
			string crop;
			YearTS yts = null;
			int size = __tslist.Count;
			for (int year = year1; year <= year2; year++)
			{
				__temp_DateTime.setYear(year);
				areaTotal = getTotalArea(year);
				if (!(areaTotal >= 0.0))
				{
					validation.add(new StateCU_ComponentValidationProblem(this, "Location \"" + id + "\" year " + year + " total area (" + areaTotal + ") is invalid.", "Verify that crop areas are >= 0 for year."));
				}
				for (int i = 0; i < size; i++)
				{
					yts = (YearTS)__tslist[i];
					area = yts.getDataValue(__temp_DateTime);
					crop = getCropNames()[i];
					if (!(area >= 0.0))
					{
						validation.add(new StateCU_ComponentValidationProblem(this, "Location \"" + id + "\" crop \"" + crop + "\" year " + year + " area (" + area + ") is invalid.", "Verify that crop area is >= 0 for year."));
					}
				}
				// Don't check fraction since that is really an artifact for output
			}
		}
		// TODO SAM 2009-05-11 Evaluate whether need check for zero area/crops for whole period
		// Can non-agricultural crops be in file?
		return validation;
	}

	/// <summary>
	/// Write a list of StateCU_CropPatternTS to a DateValue file.  The filename is
	/// adjusted to the working directory if necessary using IOUtil.getPathUsingWorkingDir(). </summary>
	/// <param name="filename_prev"> The name of the previous version of the file (for
	/// processing headers).  Specify as null if no previous file is available. </param>
	/// <param name="filename"> The name of the file to write. </param>
	/// <param name="dataList"> list of StateCU_CropPatternTS to write. </param>
	/// <param name="new_comments"> Comments to add to the top of the file.  Specify as null if no comments are available. </param>
	/// <exception cref="IOException"> if there is an error writing the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeDateValueFile(String filename_prev, String filename, java.util.List<StateCU_CropPatternTS> dataList, java.util.List<String> new_comments) throws Exception
	public static void writeDateValueFile(string filename_prev, string filename, IList<StateCU_CropPatternTS> dataList, IList<string> new_comments)
	{ // For now ignore the previous file and new comments.
		// Create a new list with the time series data...
		IList<TS> tslist = toTSList(dataList);
		// Now write using a standard DateValueTS call...
		string full_filename = IOUtil.getPathUsingWorkingDir(filename);
		DateValueTS.writeTimeSeriesList(tslist, full_filename);
	}

	/// <summary>
	/// Write a List of StateCU_CropPatternTS to a CDS file.  The filename is
	/// adjusted to the working directory if necessary using IOUtil.getPathUsingWorkingDir(). </summary>
	/// <param name="filename_prev"> The name of the previous version of the file (for
	/// processing headers).  Specify as null if no previous file is available. </param>
	/// <param name="filename"> The name of the file to write. </param>
	/// <param name="dataList"> A list of StateCU_CropPatternTS to write. </param>
	/// <param name="new_comments"> Comments to add to the top of the file.  Specify as null
	/// if no comments are available. </param>
	/// <param name="write_crop_area"> If true, then the acreage for each crop is shown in addition to the fractions. </param>
	/// <exception cref="IOException"> if there is an error writing the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeStateCUFile(String filename_prev, String filename, java.util.List<StateCU_CropPatternTS> dataList, java.util.List<String> new_comments, boolean write_crop_area) throws java.io.IOException
	public static void writeStateCUFile(string filename_prev, string filename, IList<StateCU_CropPatternTS> dataList, IList<string> new_comments, bool write_crop_area)
	{
		PropList props = new PropList("writeStateCUFile");
		if (!write_crop_area)
		{
			// Default is true so only set to false if specified...
			props.set("WriteCropArea", "False");
		}
		writeStateCUFile(filename_prev, filename, dataList, new_comments, null, null, props);
	}

	/// <summary>
	/// Write a list of StateCU_CropPatternTS to a CDS file.  The filename is
	/// adjusted to the working directory if necessary using IOUtil.getPathUsingWorkingDir(). </summary>
	/// <param name="filename_prev"> The name of the previous version of the file (for
	/// processing headers).  Specify as null if no previous file is available. </param>
	/// <param name="filename"> The name of the file to write. </param>
	/// <param name="dataList"> A list of StateCU_CropPatternTS to write. </param>
	/// <param name="new_comments"> Comments to add to the top of the file.  Specify as null 
	/// if no comments are available. </param>
	/// <param name="write_crop_area"> If true, then the acreage for each crop is shown in addition to the fractions. </param>
	/// <param name="req_date1"> Requested output start date. </param>
	/// <param name="req_date2"> Requested output end date. </param>
	/// <param name="props"> Properties to control the write. </param>
	/// <exception cref="IOException"> if there is an error writing the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeStateCUFile(String filename_prev, String filename, java.util.List<StateCU_CropPatternTS> dataList, java.util.List<String> new_comments, RTi.Util.Time.DateTime req_date1, RTi.Util.Time.DateTime req_date2, RTi.Util.IO.PropList props) throws java.io.IOException
	public static void writeStateCUFile(string filename_prev, string filename, IList<StateCU_CropPatternTS> dataList, IList<string> new_comments, DateTime req_date1, DateTime req_date2, PropList props)
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
		writeStateCUFile(dataList, @out, req_date1, req_date2, props);
		@out.flush();
		@out.close();
		@out = null;
	}

	/// <summary>
	/// Write a list of StateCU_CropPatternTS to an opened file. </summary>
	/// <param name="dataList"> A list of StateCU_CropPatternTS to write. </param>
	/// <param name="out"> output PrintWriter. </param>
	/// <param name="add_part_acres"> If true, then the acreage for each crop is shown in addition to the fractions. </param>
	/// <param name="req_date1"> Requested output start date. </param>
	/// <param name="req_date2"> Requested output end date. </param>
	/// <param name="props"> Properties to control the write, as follows:
	/// <table width=100% cellpadding=10 cellspacing=0 border=2>
	/// <tr>
	/// <td><b>Property</b></td>	<td><b>Description</b></td>	<td><b>Default</b></td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>WriteCropArea</b></td>
	/// <td>If True, the crop area is written in output for each crop, in addition to
	/// the fraction of the total.  If False, only the faction is written.</td>
	/// <td>True</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>WriteOnlyTotal</b></td>
	/// <td>If True, the output for each crop is omitted, and only the total is written.
	/// This is useful when verifying output and only the total is being checked.
	/// </td>
	/// <td>False</td>
	/// </tr>
	/// 
	/// </table> </param>
	/// <exception cref="IOException"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static void writeStateCUFile(java.util.List<StateCU_CropPatternTS> dataList, java.io.PrintWriter out, RTi.Util.Time.DateTime req_date1, RTi.Util.Time.DateTime req_date2, RTi.Util.IO.PropList props) throws java.io.IOException
	private static void writeStateCUFile(IList<StateCU_CropPatternTS> dataList, PrintWriter @out, DateTime req_date1, DateTime req_date2, PropList props)
	{
		int i;
		string iline;
		string cmnt = "#>";
		string format0 = "%-4.4s %-12.12s                  %10.10s%10.10s";
		string format1 = "     %-30.30s%10.10s";
		string rec1_format = "  Record 1 format (i4,1x,a12,18x,f10.0,i10) - for each year/CULocation.";
		string rec2_format = "  Record 2 format (5x,a30,f10.3,f10.3) - for each crop for Record 1";

		if (props == null)
		{
			props = new PropList("writeVector");
		}

		bool WriteCropArea_boolean = true; // Default
		string propval = props.getValue("WriteCropArea");
		if ((!string.ReferenceEquals(propval, null)) && propval.Equals("False", StringComparison.OrdinalIgnoreCase))
		{
			WriteCropArea_boolean = false;
		}
		bool WriteOnlyTotal_boolean = false; // Default
		propval = props.getValue("WriteOnlyTotal");
		if ((!string.ReferenceEquals(propval, null)) && propval.Equals("True", StringComparison.OrdinalIgnoreCase))
		{
			WriteOnlyTotal_boolean = true;
		}

		if (WriteCropArea_boolean)
		{
			format1 = "     %-30.30s%10.10s%10.10s";
		}

		// setup to write as old version 10
		string Version = props.getValue("Version");
		bool version10 = false; // Use the most current format
		if ((!string.ReferenceEquals(Version, null)) && Version.Equals("10", StringComparison.OrdinalIgnoreCase))
		{
			version10 = true;
			format0 = "%-4.4s %-12.12s        %10.10s%10.10s";
			format1 = "     %-20.20s%10.10s";

			// set the record format strings
			rec1_format = "  Record 1 format (i4,1x,a12,8x,f10.3,i10) - for each year/CULocation.";
			if (WriteCropArea_boolean)
			{
				rec2_format = "  Record 2 format (5x,a20,f10.3,f10.3) - for each crop for Record 1";
			}
			else
			{
				rec2_format = "  Record 2 format (5x,a20,f10.3) - for each crop for Record 1";
			}

			if (WriteCropArea_boolean)
			{
				format1 = "     %-20.20s%10.10s%10.10s";
			}
		}

		IList<object> v = new List<object>(3); // Reuse for all output lines.

		@out.println(cmnt);
		@out.println(cmnt + "  StateCU Crop Patterns (CDS) File");
		@out.println(cmnt);
		if (version10)
		{
			@out.println(cmnt + "  Header format (i5,1x,i4,5x,i5,1x,i4,a5,a5)");
		}
		else
		{
			@out.println(cmnt + "  Header format (6x,i4,5x,6x,i4,a5,a5)");
		}
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
			@out.println(cmnt + "  units            :  Data units for crop areas.");
		}
		@out.println(cmnt + "  yeartype         :  Year type (always CYR for calendar).");
		@out.println(cmnt);
		@out.println(cmnt + rec1_format);
		@out.println(cmnt);
		@out.println(cmnt + "  Yr            tyr:  Year for data (calendar year).");
		@out.println(cmnt + "  CULocation    tid:  CU Location ID (e.g., structure/station).");
		@out.println(cmnt + "  TotalAcres ttacre:  Total acreage for the CU Location.");
		@out.println(cmnt + "  NCrop            :  Number of crops at location/year.");
		@out.println(cmnt);
		@out.println(cmnt + rec2_format);
		@out.println(cmnt);
		@out.println(cmnt + "  CropName    cropn:  Crop name (e.g., ALFALFA).");
		@out.println(cmnt + "  Fraction     tpct:  Decimal fraction of total acreage");
		@out.println(cmnt + "                      for the crop (0.0 to 1.0) - INFO ONLY.");
		@out.println(cmnt + "                      Equal to total/crop acres.");
		@out.println(cmnt + "                      Fractions should add to 1.0.");
		if (WriteCropArea_boolean)
		{
			@out.println(cmnt + "  Acres       acres:  Acreage for crop.");
			@out.println(cmnt + "                      Should sum to the total acres.");
		}
		@out.println(cmnt);
		if (version10)
		{
			// Old format...
			@out.println(cmnt + "Yr  CULocation     TotalArea       NCrop");
			@out.println(cmnt + "-exb----------exxxxxxxxb--------eb--------e");
			if (WriteCropArea_boolean)
			{
				@out.println(cmnt + "     CropName          Fraction    Acres");
				@out.println(cmnt + "xxxb------------------eb--------eb--------e");
			}
			else
			{
				@out.println(cmnt + "     CropName          Fraction");
				@out.println(cmnt + "xxxb------------------eb--------e");
			}
		}
		else
		{
			// Current format...
			@out.println(cmnt + "Yr  CULocation                   TotalArea   NCrop");
			@out.println(cmnt + "-exb----------exxxxxxxxxxxxxxxxxxb--------eb--------e");
			if (WriteCropArea_boolean)
			{
				@out.println(cmnt + "     CropName                    Fraction    Acres");
				@out.println(cmnt + "xxxb----------------------------eb--------eb--------e");
			}
			else
			{
				@out.println(cmnt + "     CropName                    Fraction");
				@out.println(cmnt + "xxxb----------------------------eb--------e");
			}
		}
		if (!WriteCropArea_boolean)
		{
			@out.println(cmnt + "   Writing crop areas has been disabled (only fractions are shown).");
		}
		if (WriteOnlyTotal_boolean)
		{
			@out.println(cmnt + "   Only totals for location are shown (area by crop has been disabled).");
		}
		@out.println(cmnt + "EndHeader");

		int num = 0;
		if (dataList != null)
		{
			num = dataList.Count;
		}
		if (num == 0)
		{
			return;
		}
		StateCU_CropPatternTS cds = null;
		// The dates are taken from the first object and are assumed to be consistent between objects...
		cds = dataList[0];
		DateTime date1 = cds.getDate1();
		if (req_date1 != null)
		{
			date1 = req_date1;
		}
		DateTime date2 = cds.getDate2();
		if (req_date2 != null)
		{
			date2 = req_date2;
		}
		string units = cds.getUnits();
		DateTime date = new DateTime(date1);
		int icrop = 0;
		int ncrops = 0;
		int year = 0;
		IList<string> crop_names = null;
		string crop_name = null;
		// Default is for current version...
		string row1_header = "      " + StringUtil.formatString(date1.getYear(),"%4d") +
			"           " + StringUtil.formatString(date2.getYear(),"%4d") + " " +
			StringUtil.formatString(units,"%-4.4s") + " " + StringUtil.formatString("CYR","%-4.5s");
		double total_area, area, fraction;
		// Print the header...
		if (version10)
		{
			row1_header = "    1/" + StringUtil.formatString(date1.getYear(),"%4d") + "        12/" + StringUtil.formatString(date2.getYear(),"%4d") + " " + StringUtil.formatString(units,"%4.4s") + StringUtil.formatString("CYR","%4.4s");
		}

		@out.println(row1_header);
		// Make sure that the time series are refreshed before writing.  The
		// totals are needed to calculate percentages.
		for (i = 0; i < num; i++)
		{
			cds = dataList[i];
			cds.refresh();
		}
		// Outer loop is for the time series period...
		for (; date.lessThanOrEqualTo(date2); date.addYear(1))
		{
			year = date.getYear();
			// Inner loop is for each StateCU_Location
			for (i = 0; i < num; i++)
			{
				cds = dataList[i];
				if (cds == null)
				{
					continue;
				}
				v.Clear();
				v.Add(StringUtil.formatString(date.getYear(),"%4d"));
				v.Add(cds._id);
				total_area = cds.getTotalArea(year);
				v.Add(StringUtil.formatString(total_area,"%10.3f"));
				crop_names = cds.getCropNames();
				ncrops = crop_names.Count;
				v.Add(StringUtil.formatString(ncrops,"%10d"));
				iline = StringUtil.formatString(v, format0);
				@out.println(iline);
				if (WriteOnlyTotal_boolean)
				{
					// Only writing the total so no need to do the following...
					continue;
				}
				// Now loop through the crops for the year and CULocation...
				//long area_sum_int = 0;
				//long fraction_sum_int = 0;
				for (icrop = 0; icrop < ncrops; icrop++)
				{
					v.Clear();
					crop_name = crop_names[icrop];
					v.Add(crop_name);
					// Write the fraction...
					fraction = cds.getCropArea(crop_name, year, true);
					/* Allow missing as of 2007-06-14
					if ( (fraction < 0.0) ) {
						fraction = 0.0;
					}
					*/
					v.Add(StringUtil.formatString(fraction,"%10.3f"));
					// Write the area...
					if (WriteCropArea_boolean)
					{
						// TODO SAM 2004-07-28 May need to evaluate whether there
						// needs to be an option of how to write the crop area.  For now, calculate to
						// 3 significant figures since that is what the fraction is...
						area = cds.getCropArea(crop_name, year, false);
						/* TODO SAM 2007-10-02 Old code - remove when tested out
						if ( (total_area < 0.0) || (fraction < 0.0) ) {
							area = -999.0;
						}
						else { area = total_area*
							StringUtil.atod(
							StringUtil.formatString(
							fraction,"%.3f"));
						}
						/ * Allow missing as of 2007-06-14...
						if ( (area < 0.0) ) {
							area = 0.0;
						}
						* /
						*/
						/* TODO SAM Need to make area and fraction agree to .3 precision.
						if ( icrop == (ncrops -1) ) {
							// Want to make sure that the value that is printed results in
							// a total that agrees with the overall total.  Round off the
							// last crop name acreage so that everything agrees.
							// Multiply by 1000 to get to the precision of output.
							long total_area_int = Math.round(total_area*1000.0);
							long total_area_sum = 0;
							for ( int i2 = 0; i2 < (ncrops - 1); i2++ ) {
								total_area_sum 
							}
						}
						*/
						v.Add(StringUtil.formatString(area,"%10.3f"));
					}
					iline = StringUtil.formatString(v, format1);
					@out.println(iline);
				}
			}
		}
	}

	}

}