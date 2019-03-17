﻿using System;
using System.Collections.Generic;
using System.IO;

// StateCU_TS - class to read/write StateCU format time series

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
// StateCU_TS - class to read/write StateCU format time series
// ----------------------------------------------------------------------------
// History:
//
// 2004-02-05	Steven A. Malers, RTi	Initial version to support reading
//					IWR and WSL files in TSTool.
//					Copy and modify StateMod_TS.
// 2004-02-16	SAM, RTi		* Finalize writeTimeSeriesList() to work
//					  on frost dates.
//					* Add readTimeSeriesListFromFrost
//					  DatesFile to support TSTool - just
//					  pull in code from HBFrostDatesYearTS
//					  and do minimal cleanup.
// 2004-04-05	SAM, RTi		* Add EndHeader at the end of the header
//					  to make it easier to search for the
//					  data section.
// 2004-07-11	SAM, RTi		* Optimize the readTimeSeriesList()
//					  method to read a list based on a
//					  TSID pattern.
// 2004-10-12	SAM, RTi		* Fix bug where frost dates file was not
//					  being read because isFrostDatesFile()
//					  was not handling missing data in the
//					  first data record.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
// ----------------------------------------------------------------------------
// EndHeader

namespace DWR.StateCU
{

	using MonthTS = RTi.TS.MonthTS;
	using TS = RTi.TS.TS;
	using TSIdent = RTi.TS.TSIdent;
	using TSLimits = RTi.TS.TSLimits;
	using TSUtil = RTi.TS.TSUtil;
	using YearTS = RTi.TS.YearTS;
	using IOUtil = RTi.Util.IO.IOUtil;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;
	using DateTime = RTi.Util.Time.DateTime;
	using TimeInterval = RTi.Util.Time.TimeInterval;
	using TimeUtil = RTi.Util.Time.TimeUtil;

	/// <summary>
	/// The StateCU_TS class provides static input/output methods and general utilities
	/// for StateCU time series.  Methods are provided to read/write from monthly
	/// report files.
	/// </summary>
	public class StateCU_TS
	{

	/// <summary>
	/// Determine whether a StateCU file is a frost dates file.
	/// The file is checked for whether the 2nd non-comment line has 6 columns and four
	/// instances of "/" + "-" - this allows that the first data line has 4 valid
	/// dates 4 missing values, or a combination.
	/// be no reason why a line with no data is read. </summary>
	/// <param name="filename"> Name of file to examine. </param>
	/// <returns> true if the file is a frost dates file. </returns>
	public static bool isFrostDatesFile(string filename)
	{
		string message = null, routine = "StateMod_TS.isFrostDatesFile";
		StreamReader ifp = null;
		string iline = null;
		string full_filename = IOUtil.getPathUsingWorkingDir(filename);
		try
		{
			ifp = new StreamReader(full_filename);
		}
		catch (Exception)
		{
			message = "Unable to open file \"" + full_filename +
				"\" to determine format.";
			Message.printWarning(1, routine, message);
			return false;
		}
		bool header_read = false;
		bool is_frost_dates = false;
		try
		{
			while (true)
			{
				iline = ifp.ReadLine();
				if (string.ReferenceEquals(iline, null))
				{
					break;
				}
				if (iline.StartsWith("#", StringComparison.Ordinal))
				{
					// Comment...
					continue;
				}
				else if (!header_read)
				{
					header_read = true;
				}
				else if (header_read)
				{
					// See if line has 6 tokens and 4 instances of
					// "/" - this assumes no missing...
					IList<string> v = StringUtil.breakStringList(iline," ",StringUtil.DELIM_SKIP_BLANKS);
					if ((v != null) && (v.Count == 6) && ((StringUtil.patternCount(iline,"/") + StringUtil.patternCount(iline,"-")) == 4))
					{
						is_frost_dates = true;
					}
					// Always break, for performance reasons...
					break;
				}
			}
		}
		catch (Exception)
		{
			// TODO sam 2017-03-17 need to handle
		}
		finally
		{
			try
			{
				ifp.Close();
			}
			catch (Exception)
			{
				// Ignore.
			}
		}
		return is_frost_dates;
	}

	/// <summary>
	/// Determine whether a StateCU file is an IWR or WSL report file.
	/// The file is checked for a line that begins with _ (indicating an identifier)
	/// and a line that contains "CROP IRRIGATION WATER REQUIREMENT" or
	/// "SUPPLY-LIMITED CONSUMPTIVE USE". </summary>
	/// <param name="filename"> Name of file to examine. </param>
	/// <returns> true if the file is an IWR or WSL report file. </returns>
	public static bool isReportFile(string filename)
	{
		string message = null, routine = "StateMod_TS.isReportFile";
		StreamReader ifp = null;
		string iline = null;
		string full_filename = IOUtil.getPathUsingWorkingDir(filename);
		try
		{
			ifp = new StreamReader(full_filename);
		}
		catch (Exception)
		{
			message = "Unable to open file \"" + full_filename +
				"\" to determine format.";
			Message.printWarning(1, routine, message);
			return false;
		}
		bool underline_found = false;
		bool title_found = false;
		try
		{
			while (true)
			{
				iline = ifp.ReadLine();
				if (string.ReferenceEquals(iline, null))
				{
					break;
				}
				if (!underline_found && iline.StartsWith("_", StringComparison.Ordinal))
				{
					underline_found = true;
				}
				if (!title_found && (StringUtil.indexOfIgnoreCase(iline, "CROP IRRIGATION WATER REQUIREMENT",0) > 0) || StringUtil.indexOfIgnoreCase(iline, "SUPPLY-LIMITED CONSUMPTIVE USE",0) > 0)
				{
					title_found = true;
				}
				if (underline_found && title_found)
				{
					return true;
				}
			}
		}
		catch (IOException)
		{
			// TODO sam 2017-03-16 need to handle
		}
		finally
		{
			try
			{
				ifp.Close();
			}
			catch (Exception)
			{
				// Ignore.
			}
		}
		return false;
	}

	/// <summary>
	/// Read a time series from a StateCU format file.  The TSID string is specified
	/// in addition to the path to the file.  It is expected that a TSID in the file
	/// matches the TSID (and the path to the file, if included in the TSID would not
	/// properly allow the TSID to be specified).  This method can be used with newer
	/// code where the I/O path is separate from the TSID that is used to identify the
	/// time series.
	/// The IOUtil.getPathUsingWorkingDir() method is applied to the filename. </summary>
	/// <returns> a pointer to a newly-allocated time series if successful, a NULL pointer
	/// if not. </returns>
	/// <param name="tsident_string"> The full identifier for the time series to
	/// read.  The location, data type, and interval will be used to match the time
	/// series. </param>
	/// <param name="filename"> The name of a file to read
	/// (in which case the tsident_string must match one of the TSID strings in the
	/// file). </param>
	/// <param name="date1"> Starting date to initialize period (null to read the entire time
	/// series). </param>
	/// <param name="date2"> Ending date to initialize period (null to read the entire time
	/// series). </param>
	/// <param name="units"> Units to convert to. </param>
	/// <param name="read_data"> Indicates whether data should be read (false=no, true=yes). </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static RTi.TS.TS readTimeSeries(String tsident_string, String filename, RTi.Util.Time.DateTime date1, RTi.Util.Time.DateTime date2, String units, boolean read_data) throws Exception
	public static TS readTimeSeries(string tsident_string, string filename, DateTime date1, DateTime date2, string units, bool read_data)
	{
		TS ts = null;
		string routine = "StateCU_TS.readTimeSeries";

		string full_fname = IOUtil.getPathUsingWorkingDir(filename);
		if (!IOUtil.fileReadable(full_fname))
		{
			Message.printWarning(2, routine, "Unable to determine file for \"" + filename + "\"");
			return ts;
		}
		StreamReader @in = null;
		try
		{ //data_interval = getFileDataInterval ( full_fname );
			@in = new StreamReader(IOUtil.getInputStream(full_fname));
		}
		catch (Exception)
		{
			Message.printWarning(2, routine, "Unable to open file \"" + full_fname + "\"");
			return ts;
		}
		IList<TS> v = readTimeSeriesList(tsident_string, @in, full_fname, date1, date2, units, read_data);
		@in.Close();
		if ((v == null) || (v.Count == 0))
		{
			// Did not find the time series...
			return null;
		}
		else
		{
			return (TS)v[0];
		}
	}

	/// <summary>
	/// Read one ore more time series from a StateCU format file.  The TSID string is
	/// specified in addition to the path to the file.  It is expected that at least
	/// one TSID in the file
	/// matches the TSID (and the path to the file, if included in the TSID would not
	/// properly allow the TSID to be specified).  This method can be used with newer
	/// code where the I/O path is separate from the TSID that is used to identify the
	/// time series.
	/// The IOUtil.getPathUsingWorkingDir() method is applied to the filename. </summary>
	/// <returns> a pointer to a newly-allocated time series if successful, a NULL pointer
	/// if not. </returns>
	/// <param name="tsident_string"> The full identifier for the time series to
	/// read.  If null, all time series will be read.
	/// If a specific TSID with no wildcards, only the single time series will be
	/// read.  Wildcards can be used in the location to match one or more time series. </param>
	/// <param name="filename"> The name of a file to read
	/// (in which case the tsident_string must match one of the TSID strings in the
	/// file). </param>
	/// <param name="date1"> Starting date to initialize period (null to read the entire time
	/// series). </param>
	/// <param name="date2"> Ending date to initialize period (null to read the entire time
	/// series). </param>
	/// <param name="units"> Units to convert to. </param>
	/// <param name="read_data"> Indicates whether data should be read (false=no, true=yes). </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.List<RTi.TS.TS> readTimeSeriesList(String tsident_string, String filename, RTi.Util.Time.DateTime date1, RTi.Util.Time.DateTime date2, String units, boolean read_data) throws Exception
	public static IList<TS> readTimeSeriesList(string tsident_string, string filename, DateTime date1, DateTime date2, string units, bool read_data)
	{
		string routine = "StateCU_TS.readTimeSeriesList";

		string input_name = filename;
		string full_fname = IOUtil.getPathUsingWorkingDir(filename);
		if (!IOUtil.fileReadable(full_fname))
		{
			Message.printWarning(2, routine, "Unable to determine file for \"" + filename + "\"");
			return null;
		}
		StreamReader @in = null;
		try
		{ //data_interval = getFileDataInterval ( full_fname );
			@in = new StreamReader(IOUtil.getInputStream(full_fname));
		}
		catch (Exception)
		{
			Message.printWarning(2, routine, "Unable to open file \"" + full_fname + "\"");
			return null;
		}
		IList<TS> tslist = readTimeSeriesList(tsident_string, @in, full_fname, date1, date2, units, read_data);
		@in.Close();
		TS ts;
		int nts = 0;
		if (tslist != null)
		{
			nts = tslist.Count;
		}
		for (int i = 0; i < nts; i++)
		{
			ts = (TS)tslist[i];
			if (ts != null)
			{
				ts.setInputName(full_fname);
				ts.getIdentifier().setInputName(input_name);
			}
		}
		return tslist;
	}

	/// <summary>
	/// Read all the time series from a StateCU format file.
	/// The IOUtil.getPathUsingWorkingDir() method is applied to the filename. </summary>
	/// <returns> a pointer to a newly-allocated Vector of time series if successful,
	/// a NULL pointer if not. </returns>
	/// <param name="fname"> Name of file to read. </param>
	/// <param name="date1"> Starting date to initialize period (NULL to read the entire time series). </param>
	/// <param name="date2"> Ending date to initialize period (NULL to read the entire time series). </param>
	/// <param name="units"> Units to convert to. </param>
	/// <param name="read_data"> Indicates whether data should be read. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.List<RTi.TS.TS> readTimeSeriesList(String fname, RTi.Util.Time.DateTime date1, RTi.Util.Time.DateTime date2, String units, boolean read_data) throws Exception
	public static IList<TS> readTimeSeriesList(string fname, DateTime date1, DateTime date2, string units, bool read_data)
	{
		return readTimeSeriesList(null, fname, date1, date2, units,read_data);
	}

	/// <summary>
	/// Read one or more time series from a StateCU format file. </summary>
	/// <returns> a Vector of time series if successful, null if not.  The calling code
	/// is responsible for freeing the memory for the time series. </returns>
	/// <param name="req_tsident_string"> Pointer to time series identifier to read.  If null,
	/// return all new time series in the vector. </param>
	/// <param name="in"> Reference to open input stream.
	/// @parm full_filename Full path to filename, used for messages. </param>
	/// <param name="req_date1"> Requested starting date to initialize period (or NULL to read
	/// the entire time series). </param>
	/// <param name="req_date2"> Requested ending date to initialize period (or NULL to read
	/// the entire time series). </param>
	/// <param name="units"> Units to convert to (currently ignored). </param>
	/// <param name="read_data"> Indicates whether data should be read. </param>
	/// <exception cref="Exception"> if there is an error reading the time series. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static java.util.List<RTi.TS.TS> readTimeSeriesList(String req_tsident_string, java.io.BufferedReader in, String full_filename, RTi.Util.Time.DateTime req_date1, RTi.Util.Time.DateTime req_date2, String req_units, boolean read_data) throws Exception
	private static IList<TS> readTimeSeriesList(string req_tsident_string, StreamReader @in, string full_filename, DateTime req_date1, DateTime req_date2, string req_units, bool read_data)
	{ // Split out into separate methods based on the file type...

		if (isFrostDatesFile(full_filename))
		{
			// Read data from a frost dates file...
			return readTimeSeriesListFromFrostDatesFile(req_tsident_string, @in, full_filename, req_date1, req_date2, req_units, read_data);
		}
		else
		{ // Assume a report file for now.  Later can check the file
			// contents...
			return readTimeSeriesListFromReportFile(req_tsident_string, @in, full_filename, req_date1, req_date2, req_units, read_data);
		}
	}

	/// <summary>
	/// Read one or more time series from a StateCU frost dates format file. </summary>
	/// <returns> a Vector of time series if successful, null if not.  The calling code
	/// is responsible for freeing the memory for the time series. </returns>
	/// <param name="req_tsident_string"> Pointer to time series identifier to read.  If null,
	/// return all new time series in the vector. </param>
	/// <param name="ifp"> Reference to open input stream.
	/// @parm full_filename Full path to filename, used for messages. </param>
	/// <param name="req_date1"> Requested starting date to initialize period (or NULL to read
	/// the entire time series). </param>
	/// <param name="req_date2"> Requested ending date to initialize period (or NULL to read
	/// the entire time series). </param>
	/// <param name="units"> Units to convert to (currently ignored). </param>
	/// <param name="read_data"> Indicates whether data should be read. </param>
	/// <exception cref="Exception"> if there is an error reading the time series. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static java.util.List<RTi.TS.TS> readTimeSeriesListFromFrostDatesFile(String req_tsident_string, java.io.BufferedReader ifp, String full_filename, RTi.Util.Time.DateTime req_date1, RTi.Util.Time.DateTime req_date2, String req_units, boolean read_data) throws Exception
	private static IList<TS> readTimeSeriesListFromFrostDatesFile(string req_tsident_string, StreamReader ifp, string full_filename, DateTime req_date1, DateTime req_date2, string req_units, bool read_data)
	{
		int dl = 30, y1, y2, year = 0, currentIDindex, current_year = 0, numids = 0, line_count = 0; // Count of lines that are read
		string chval, iline, message, rtn = "StateCU_TS.readTimeSeriesListFromFrostDatesFile";
		IList<object> v;
		IList<TS> tslist = new List<TS>(); // Time series data to return.
		DateTime date = new DateTime(DateTime.PRECISION_YEAR);
		TSIdent tsident = null; // Time series identifier used
							// when creating time series -
							// reused below.
							// Used for iteration.
		bool requested_id_found = false; // Indicates if we have found
							// the requested TS in the file.
		TSIdent req_tsident = null; // TSIdent for requested time
							// series.
		string requested_id = null;
		string requested_data_type = null;
		int requested_col = 0; // Requested column - 0 to 3
		if (!string.ReferenceEquals(req_tsident_string, null))
		{
			req_tsident = new TSIdent(req_tsident_string);
			requested_id = req_tsident.getLocation();
			requested_data_type = req_tsident.getType();
			requested_col = 0;
			if (requested_data_type.Equals("FrostDateL28S", StringComparison.OrdinalIgnoreCase))
			{
				requested_col = 0;
			}
			else if (requested_data_type.Equals("FrostDateL32S", StringComparison.OrdinalIgnoreCase))
			{
				requested_col = 1;
			}
			else if (requested_data_type.Equals("FrostDateF32F", StringComparison.OrdinalIgnoreCase))
			{
				requested_col = 2;
			}
			else if (requested_data_type.Equals("FrostDateF28F", StringComparison.OrdinalIgnoreCase))
			{
				requested_col = 3;
			}
			else
			{ // Data type is not recognized...
				message = "Requested data type in \"" + req_tsident_string + "\" is not recognized.";
				Message.printWarning(2, rtn, message);
				throw new Exception(message);
			}
		}

		try
		{ // Main try...
		iline = ifp.ReadLine();
		++line_count;
		if ((string.ReferenceEquals(iline, null)) || (iline.Trim().Length < 1))
		{
			ifp.Close();
			message = "Unexpected end of frost dates file.";
			Message.printWarning(2, rtn, message);
			throw new Exception(message);
		}

		// Read lines until no more comments are found.  The last line read will
		// need to be processed as the main header line...

		while (iline.StartsWith("#", StringComparison.Ordinal))
		{
			iline = ifp.ReadLine();
			++line_count;
		}

		if (Message.isDebugOn)
		{
			Message.printDebug(dl, rtn, "Done with comments");
		}

		// Process the main header line...

		// Standard header...

		string format_0 = "i5s1i4s8i2s1i4s5s5";

		if (Message.isDebugOn)
		{
			Message.printDebug(dl, rtn, "Parsing line for period: \"" + iline + "\"");
		}

		v = StringUtil.fixedRead(iline, format_0);
		y1 = ((int?)v[2]).Value;
		y2 = ((int?)v[6]).Value;
		string units = ((string)v[7]).Trim();
		if (Message.isDebugOn)
		{
			Message.printDebug(dl, rtn, "Retrieved: year1=" + y1 + " year2=" + y2 + " units=" + units);
		}

		// Read remaining data lines.  If in the first year, allocate memory
		// for each time series as a new station is encountered...

		YearTS currentTS0 = null, currentTS1 = null, currentTS2 = null, currentTS3 = null, year_ts0 = null, year_ts1 = null, year_ts2 = null, year_ts3 = null;
						// Used to fill data.
		string id = null; // Identifier for a row.
		DateTime tsdate = null; // Date for processing.
		string @string = null;

		// Sometimes, the time series files have empty lines at the
		// bottom, checking it's length seemed to solve the problem.

		string format_annual = "i5s12s8s8s8s8";
		bool first_line_read = false; // Indicate that initial year is
							// is being read.
		currentIDindex = 0;
		bool line_parsed = false;
		while (true)
		{
			// Read a data line to check the years.  Each line will have 4
			// dates.

			iline = ifp.ReadLine();
			++line_count;
			 if ((string.ReferenceEquals(iline, null)) || (iline.Length == 0))
			 {
				// End of file...
				break;
			 }
			line_parsed = false;
			if (Message.isDebugOn)
			{
				Message.printDebug(dl, rtn, "Parsing line: \"" + iline + "\"");
			}

			// To improve performance, only do the full parse here if we
			// know we are getting only the header or are reading the whole
			// file...

			if (!read_data || (string.ReferenceEquals(requested_id, null)))
			{
				v = StringUtil.fixedRead(iline, format_annual);
				line_parsed = true;
			}

			// Check to make sure that the initial data line's date agrees
			// with the header...

			if (!first_line_read)
			{
				if (!line_parsed)
				{
					v = StringUtil.fixedRead(iline,format_annual);
					line_parsed = true;
				}
				current_year = ((int?)v[0]).Value;
				if (current_year != y1)
				{
					message = "First data record does not match " +
						"start of period:  " + y1;
					Message.printWarning(2, rtn, message);
					throw new Exception(message);
				}
				first_line_read = true;
			}

			// The first thing that we do is get the time series identifier
			// so we can check against a requested identifier...

			if (!string.ReferenceEquals(requested_id, null))
			{
				// Get the ID from the input line.  We don't parse
				// out the remaining lines unless this line is a
				// match...
				chval = iline.Substring(5, 12);
				id = chval.Trim();

				if (!id.Equals(requested_id, StringComparison.OrdinalIgnoreCase))
				{
					// We are not interested in this
					// time series so don't process...
					//iline = ifp.readLine();
					if (Message.isDebugOn)
					{
						Message.printDebug(dl, rtn, "Not interested in " + id + ".  Continuing.");
					}
					continue;
				}
			}

			// Parse the data line because all time series are being read
			// OR a requested identifier has been found...

			if (!line_parsed)
			{
				v = StringUtil.fixedRead(iline, format_annual);
				line_parsed = true;
			}
			current_year = ((int?)v[0]).Value;
			if (Message.isDebugOn)
			{
				Message.printDebug(dl, rtn, "Found id!  Read annual format.");
			}

			if (string.ReferenceEquals(requested_id, null))
			{
				// Need to get because it is parsed above only when
				// searching for a requested ID...
				id = ((string)v[1]).Trim();
			}

			if (Message.isDebugOn)
			{
				Message.printDebug(dl, rtn, "Current year: " + current_year + ", year1: " + y1);
			}
			if (current_year == y1)
			{
				// We are still establishing the list of stations in
				// the file...
				if (string.ReferenceEquals(requested_id, null))
				{
					// Create a new time series for each column of
					// data for the identifier...
					if (Message.isDebugOn)
					{
						Message.printDebug(dl, rtn, "Creating new TS for list.");
					}
						year_ts0 = new YearTS();
						year_ts1 = new YearTS();
						year_ts2 = new YearTS();
						year_ts3 = new YearTS();
				}
				else if (id.Equals(requested_id, StringComparison.OrdinalIgnoreCase))
				{
					// We want the requested time series to
					// get filled in.  Only allocate one time
					// series...
					if (Message.isDebugOn)
					{
						Message.printDebug(dl, rtn, "Setting TS to requested TS for \"" + requested_id + "\".");
					}
					if (requested_col == 0)
					{
							year_ts0 = new YearTS();
					}
					else if (requested_col == 1)
					{
							year_ts1 = new YearTS();
					}
					else if (requested_col == 2)
					{
							year_ts2 = new YearTS();
					}
					else if (requested_col == 3)
					{
							year_ts3 = new YearTS();
					}
					requested_id_found = true;
					numids = 1;
				}

				// We are guaranteed to have a time series from the
				// above checks and would not get to here otherwise.
				// Checking for null should handle the requested TS
				// properly...

				if ((req_date1 != null) && (req_date2 != null))
				{
					// Allocate memory for the time series based on
					// the requested period.
					if (year_ts0 != null)
					{
						year_ts0.setDate1(req_date1);
						year_ts0.setDate2(req_date2);
					}
					if (year_ts1 != null)
					{
						year_ts1.setDate1(req_date1);
						year_ts1.setDate2(req_date2);
					}
					if (year_ts2 != null)
					{
						year_ts2.setDate1(req_date1);
						year_ts2.setDate2(req_date2);
					}
					if (year_ts3 != null)
					{
						year_ts3.setDate1(req_date1);
						year_ts3.setDate2(req_date2);
					}
				}
				else
				{ // Allocate memory for the time series based on
					// the file header....
					date.setYear(y1);
					if (year_ts0 != null)
					{
						year_ts0.setDate1(date);
					}
					if (year_ts1 != null)
					{
						year_ts1.setDate1(date);
					}
					if (year_ts2 != null)
					{
						year_ts2.setDate1(date);
					}
					if (year_ts3 != null)
					{
						year_ts3.setDate1(date);
					}
					date.setYear(y2);
					if (year_ts0 != null)
					{
						year_ts0.setDate2(date);
					}
					if (year_ts1 != null)
					{
						year_ts1.setDate2(date);
					}
					if (year_ts2 != null)
					{
						year_ts2.setDate2(date);
					}
					if (year_ts3 != null)
					{
						year_ts3.setDate2(date);
					}
				}

				if (read_data)
				{
					// We are reading data so allocate the
					// data space...
					if (year_ts0 != null)
					{
						year_ts0.allocateDataSpace();
					}
					if (year_ts1 != null)
					{
						year_ts1.allocateDataSpace();
					}
					if (year_ts2 != null)
					{
						year_ts2.allocateDataSpace();
					}
					if (year_ts3 != null)
					{
						year_ts3.allocateDataSpace();
					}
				}

				if (year_ts0 != null)
				{
					year_ts0.setDescription(id);
					year_ts0.setDataUnits(units);
					year_ts0.setDataUnitsOriginal(units);
					year_ts0.setInputName(full_filename);
					tsident = new TSIdent();
					tsident.setLocation(id);
					tsident.setSource("StateCU");
					tsident.setType("FrostDateL28S");
					tsident.setInterval("Year");
					tsident.setInputType("StateCU");
					year_ts0.setIdentifier(tsident);
					tslist.Add(year_ts0);
				}

				if (year_ts1 != null)
				{
					year_ts1.setDescription(id);
					year_ts1.setDataUnits(units);
					year_ts1.setDataUnitsOriginal(units);
					year_ts1.setInputName(full_filename);
					tsident = new TSIdent();
					tsident.setLocation(id);
					tsident.setSource("StateCU");
					tsident.setType("FrostDateL32S");
					tsident.setInterval("Year");
					tsident.setInputType("StateCU");
					year_ts1.setIdentifier(tsident);
					tslist.Add(year_ts1);
				}

				if (year_ts2 != null)
				{
					year_ts2.setDescription(id);
					year_ts2.setDataUnits(units);
					year_ts2.setDataUnitsOriginal(units);
					year_ts2.setInputName(full_filename);
					tsident = new TSIdent();
					tsident.setLocation(id);
					tsident.setSource("StateCU");
					tsident.setType("FrostDateF32F");
					tsident.setInterval("Year");
					tsident.setInputType("StateCU");
					year_ts2.setIdentifier(tsident);
					tslist.Add(year_ts2);
				}

				if (year_ts3 != null)
				{
					year_ts3.setDescription(id);
					year_ts3.setDataUnits(units);
					year_ts3.setDataUnitsOriginal(units);
					year_ts3.setInputName(full_filename);
					tsident = new TSIdent();
					tsident.setLocation(id);
					tsident.setSource("StateCU");
					tsident.setType("FrostDateF28F");
					tsident.setInterval("Year");
					tsident.setInputType("StateCU");
					year_ts3.setIdentifier(tsident);
					tslist.Add(year_ts3);
				}

				numids++;
			}
			else
			{ // Not in the first year of data.  If only reading the
				// header then done reading.  If reading all data,
				// parse the line and save data values...
				if (!read_data)
				{
					// Only reading the header so return;
					break;
				}
				if ((!string.ReferenceEquals(requested_id, null)) && !requested_id_found)
				{
					// Went through a year and did not find the
					// ID.  Problem.
					message = "Did not find time series for requeted " +
					"identifier \"" + requested_id + "\"";
					Message.printWarning(2, rtn, message);
					ifp.Close();
					throw new Exception(message);
				}
			}

			// If we are working through the first year, currentTS
			// will be = numids.  On the other hand, if we have already
			// established the list and are filling the rest of the
			// rows, currentTS should be reset to the head of the list
			// and the year should be increased.

			if (currentIDindex >= numids)
			{
				currentIDindex = 0;
				year++;
			}

			// Check the date once up front rather than after parsing to
			// improve performance.  If the dates in the file are not
			// correct, the read may break prematurely.  If data records are
			// read below, the year from the data line will always be used.

			date.setYear(y1 + year);
			if (req_date2 != null)
			{
				if (date.greaterThan(req_date2))
				{
					break;
				}
			}

			if (!requested_id_found)
			{
				// Reading all data...
				currentTS0 = (YearTS)tslist[currentIDindex * 4];
				currentTS1 = (YearTS)tslist[currentIDindex * 4 + 1];
				currentTS2 = (YearTS)tslist[currentIDindex * 4 + 2];
				currentTS3 = (YearTS)tslist[currentIDindex * 4 + 3];
			}
			else
			{ // The requested identifier has been found.  Point to
				// the specific time series - there will only be one
				// since only one time series can be returned...
				if (requested_col == 0)
				{
					currentTS0 = (YearTS)tslist[0];
				}
				else if (requested_col == 1)
				{
					currentTS1 = (YearTS)tslist[0];
				}
				else if (requested_col == 2)
				{
					currentTS2 = (YearTS)tslist[0];
				}
				else if (requested_col == 3)
				{
					currentTS3 = (YearTS)tslist[0];
				}
			}

			// Parse out the frost dates and save as julian days in the time
			// series data...

			try
			{ // Might return a null...
				// Need to save the data...
				current_year = ((int?)v[0]).Value;
				date.setYear(current_year);
				if (currentTS0 != null)
				{
					@string = ((string)v[2]).Trim();
					if (@string[0] != '-')
					{
						// Not missing...
						try
						{
							tsdate = DateTime.parse(@string);
							tsdate.setYear(current_year);
							currentTS0.setDataValue(date, TimeUtil.dayOfYear(tsdate));
						}
						catch (Exception)
						{
							Message.printWarning(2, rtn, "Error parsing date \"" + @string + "\" in line " + iline);
							// Leave missing...
						}
					}
				}
				if (currentTS1 != null)
				{
					@string = ((string)v[3]).Trim();
					if (@string[0] != '-')
					{
						try
						{
							tsdate = DateTime.parse(@string);
							tsdate.setYear(current_year);
							currentTS1.setDataValue(date, TimeUtil.dayOfYear(tsdate));
						}
						catch (Exception)
						{
							Message.printWarning(2, rtn, "Error parsing date \"" + @string + "\" in line " + iline);
							// Leave missing...
						}
					}
				}
				if (currentTS2 != null)
				{
					@string = ((string)v[4]).Trim();
					if (@string[0] != '-')
					{
						try
						{
							tsdate = DateTime.parse(@string);
							tsdate.setYear(current_year);
							currentTS2.setDataValue(date, TimeUtil.dayOfYear(tsdate));
						}
						catch (Exception)
						{
							Message.printWarning(2, rtn, "Error parsing date \"" + @string + "\" in line " + iline);
							// Leave missing...
						}
					}
				}
				if (currentTS3 != null)
				{
					@string = ((string)v[5]).Trim();
					if (@string[0] != '-')
					{
						try
						{
							tsdate = DateTime.parse(@string);
							tsdate.setYear(current_year);
							currentTS3.setDataValue(date, TimeUtil.dayOfYear(tsdate));
						}
						catch (Exception)
						{
							Message.printWarning(2, rtn, "Error parsing date \"" + @string + "\" in line " + iline);
							// Leave missing...
						}
					}
				}
			}
			catch (Exception e)
			{
				// Just catch if we have a formatting problem...
				Message.printWarning(2, rtn, "Error parsing data line " + line_count + ": \"" + iline + "\"");
				Message.printWarning(2, rtn, e);
			}

			// Increment the loop counter on the identifiers.  This will be
			// reset for each year that is read.
			currentIDindex++;
		}
		// Close the file...
		ifp.Close();
		// Return the vector of time series...
		return tslist;
		} // Main try around routine.
		catch (Exception e)
		{
			message = "Error reading frost dates file.";
			Message.printWarning(2, rtn, message);
			Message.printWarning(2, rtn, e);
			//throw new Exception ( message );
			return null;
		}
	}

	/// <summary>
	/// Read one or more time series from a StateCU report format file. </summary>
	/// <returns> a Vector of time series if successful, null if not.  The calling code
	/// is responsible for freeing the memory for the time series. </returns>
	/// <param name="req_tsident_string"> Pointer to time series identifier to read.  If null,
	/// return all new time series in the vector. </param>
	/// <param name="in"> Reference to open input stream.
	/// @parm full_filename Full path to filename, used for messages. </param>
	/// <param name="req_date1"> Requested starting date to initialize period (or NULL to read
	/// the entire time series). </param>
	/// <param name="req_date2"> Requested ending date to initialize period (or NULL to read
	/// the entire time series). </param>
	/// <param name="units"> Units to convert to (currently ignored). </param>
	/// <param name="read_data"> Indicates whether data should be read. </param>
	/// <exception cref="Exception"> if there is an error reading the time series. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static java.util.List<RTi.TS.TS> readTimeSeriesListFromReportFile(String req_tsident_string, java.io.BufferedReader in, String full_filename, RTi.Util.Time.DateTime req_date1, RTi.Util.Time.DateTime req_date2, String req_units, boolean read_data) throws Exception
	private static IList<TS> readTimeSeriesListFromReportFile(string req_tsident_string, StreamReader @in, string full_filename, DateTime req_date1, DateTime req_date2, string req_units, bool read_data)
	{
		string routine = "StateCU_TS.readTimeSeriesListFromReportFile";
		string req_id = null;
		string req_data_type = null;
		string req_interval = null;
		string req_id_pattern = null; // Need for wildcarding
		IList<TS> tslist = new List<TS>();
		int dl = 1;
		int line_count = 0;
		TSIdent req_tsident = null;
		bool req_id_has_wildcards = false;
		if (!string.ReferenceEquals(req_tsident_string, null))
		{
			req_tsident = new TSIdent(req_tsident_string);
			req_id = req_tsident.getLocation();
			req_id_pattern = StringUtil.replaceString(req_id,"*",".*");
			if (req_id.IndexOf('*') >= 0)
			{
				req_id_has_wildcards = true;
			}
			req_data_type = req_tsident.getType();
			req_interval = req_tsident.getInterval();
		}
		try
		{ // General error handler

		// Format for data lines...

		int[] format = new int[16];
		int[] format_w = new int[16];
		format[0] = StringUtil.TYPE_INTEGER;
		format[1] = StringUtil.TYPE_DOUBLE;
		format[2] = StringUtil.TYPE_DOUBLE;
		format[3] = StringUtil.TYPE_DOUBLE;
		format[4] = StringUtil.TYPE_DOUBLE;
		format[5] = StringUtil.TYPE_DOUBLE;
		format[6] = StringUtil.TYPE_DOUBLE;
		format[7] = StringUtil.TYPE_DOUBLE;
		format[8] = StringUtil.TYPE_DOUBLE;
		format[9] = StringUtil.TYPE_DOUBLE;
		format[10] = StringUtil.TYPE_DOUBLE;
		format[11] = StringUtil.TYPE_DOUBLE;
		format[12] = StringUtil.TYPE_DOUBLE;
		format[13] = StringUtil.TYPE_DOUBLE;
		format[14] = StringUtil.TYPE_DOUBLE;
		format[15] = StringUtil.TYPE_DOUBLE;

		format_w[0] = 5; // Year
		format_w[1] = 9; // Area
		format_w[2] = 6; // Jan
		format_w[3] = 7; // Feb
		format_w[4] = 7; // Mar
		format_w[5] = 8; // Apr
		format_w[6] = 8; // May
		format_w[7] = 8; // Jun
		format_w[8] = 8; // Jul
		format_w[9] = 8; // Aug
		format_w[10] = 8; // Sep
		format_w[11] = 8; // Oct
		format_w[12] = 7; // Nov
		format_w[13] = 6; // Dec
		format_w[14] = 10; // Annual
		format_w[15] = 7; // Depth

		// Read the data...

		bool req_id_found = false; // used when reading a single time
						// series
		int year1 = 0, year2 = 0, icol = 0, year = 0, pos = 0, size = 0, i = 0;
		string id, name; // ID and name in file.
		DateTime date1 = new DateTime(DateTime.PRECISION_MONTH);
		DateTime date2 = new DateTime(DateTime.PRECISION_MONTH);
						// Period to allocate time series.
		DateTime date = new DateTime(); // Used for data iteration.
		IList<string> ilines = new List<string>(); // Buffer to hold lines for a single
						// time series.
		TSIdent tsident = null; // Used to create new time series.
		YearTS area_ts = null, yts = null, depth_ts = null;
		MonthTS mts = null; // The time series to return.
		IList<object> v = null; // Used to parse
		string iline; // Line read from file.
		bool include_area_ts = true; // Indicate which time series to include
		bool include_mts = true;
		bool include_yts = true;
		bool include_depth_ts = true;
		bool file_type_found = false;
		string file_data_type = "IWR"; // Default - or "WSL"
		while (true)
		{
			iline = @in.ReadLine();
			if (string.ReferenceEquals(iline, null))
			{
				// No more data...
				break;
			}
			++line_count;

			if (Message.isDebugOn)
			{
				Message.printDebug(dl, routine, "Parsing line: \"" + iline + "\"");
			}

			if (!iline.StartsWith("_", StringComparison.Ordinal))
			{
				// Lines at the end of one location and before the next
				// _ID string... generally of no interest...
				continue;
			}

			// Else line starting with _ indicates new time series...

			if (Message.isDebugOn)
			{
				Message.printDebug(dl, routine,"Found new location.");
			}
			id = StringUtil.getToken(iline.Substring(1)," ",0,0);
			pos = iline.IndexOf(" ", StringComparison.Ordinal);
			name = iline.Substring(pos).Trim();
			// 5 header lines...
			iline = @in.ReadLine();
			++line_count;
			if (!file_type_found)
			{
				// See if the line has the data type...
				if (iline.IndexOf("SUPPLY-LIMITED CONSUMPTIVE USE", StringComparison.Ordinal) > 0)
				{
					file_data_type = "WSL";
					file_type_found = true;
				}
			}

			iline = @in.ReadLine();
			++line_count;
			iline = @in.ReadLine();
			++line_count;
			iline = @in.ReadLine();
			++line_count;
			iline = @in.ReadLine();
			++line_count;
			// Now read the data associated with the time series - one line
			// per year...
			ilines.Clear();
			while (true)
			{
				iline = @in.ReadLine();
				if (string.ReferenceEquals(iline, null))
				{
					// No more data...
					break;
				}
				++line_count;
				if (Message.isDebugOn)
				{
					Message.printDebug(dl, routine, "Parsing line: \"" + iline + "\"");
				}
				if (iline.StartsWith("-", StringComparison.Ordinal))
				{
					// End of this time series' data.
					// Process the lines that were previously read
					// and bufferred.
					if (((!string.ReferenceEquals(req_id, null)) && !id.matches(req_id_pattern)))
					{
						// No need to process...
						if (Message.isDebugOn)
						{
							Message.printDebug(dl, routine, "Ignoring - " + id + " does not match requested \"" + req_id + "\"");
						}
						break;
					}
					size = ilines.Count;
					if (Message.isDebugOn)
					{
						Message.printDebug(dl, routine, "Read " + size + " lines of data.  " + "Processing requested TS...");
					}
					// Else we are reading all time series or need
					// one of the time series for the ID...
					if (Message.isDebugOn)
					{
						Message.printDebug(1, routine, "Matched ID.  File data type is " + file_data_type);
					}
					if (year1 == 0)
					{
						// Need to determine period using the
						// first column from the first and last
						// lines for this location...
						v = StringUtil.fixedRead((string) ilines[0], format, format_w, v);
						year1 = ((int?) v[0]).Value;
						v = StringUtil.fixedRead((string) ilines[size - 1], format, format_w, v);
						year2 = ((int?) v[0]).Value;
						date1.setMonth(1);
						date1.setYear(year1);
						date2.setMonth(12);
						date2.setYear(year2);
					}
					// Else...assume that the period from the first
					// time series is the same as other time series.
					// If not data will either be missing or the
					// period will be truncated.

					// Determine the time series that should be
					// read and returned...

					include_area_ts = true;
					include_mts = true;
					include_yts = true;
					include_depth_ts = true;

					if (!string.ReferenceEquals(req_id, null))
					{
						// Check the specific time series to see
						// if they should be included...
						if ((req_data_type.Equals("CropArea-AllCrops", StringComparison.OrdinalIgnoreCase) || req_data_type.Equals("*")) && (req_interval.Equals("Year", StringComparison.OrdinalIgnoreCase) || req_interval.Equals("*")))
						{
							include_area_ts = true;
							if (Message.isDebugOn)
							{
								Message.printDebug(1, routine, "Matched area TS.");
							}
						}
						else
						{
							include_area_ts = false;
						}
						if ((req_data_type.Equals(file_data_type, StringComparison.OrdinalIgnoreCase) || req_data_type.Equals("*")) && (req_interval.Equals("MONTH", StringComparison.OrdinalIgnoreCase) || req_interval.Equals("*")))
						{
							include_mts = true;
							if (Message.isDebugOn)
							{
								Message.printDebug(1, routine, "Matched " + "monthly " + file_data_type + " TS.");
							}
						}
						else
						{
							include_mts = false;
						}
						if ((req_data_type.Equals(file_data_type, StringComparison.OrdinalIgnoreCase) || req_data_type.Equals("*")) && (req_interval.Equals("Year", StringComparison.OrdinalIgnoreCase) || req_interval.Equals("*")))
						{
							include_yts = true;
							if (Message.isDebugOn)
							{
								Message.printDebug(1, routine, "Matched " + "yearly " + file_data_type + " TS.");
							}
						}
						else
						{
							include_yts = false;
						}
						if ((req_data_type.Equals(file_data_type + "_Depth", StringComparison.OrdinalIgnoreCase) || req_data_type.Equals("*")) && (req_interval.Equals("Year", StringComparison.OrdinalIgnoreCase) || req_interval.Equals("*")))
						{
							include_depth_ts = true;
							if (Message.isDebugOn)
							{
								Message.printDebug(1, routine, "Matched " + "yearly depth.");
							}
						}
						else
						{
							include_depth_ts = false;
						}
					}

					// Allocate the time series for the location...

					// Acreage...

					if (include_area_ts)
					{
						area_ts = new YearTS();
						area_ts.setDate1(date1);
						area_ts.setDate2(date2);
						area_ts.setDate1Original(date1);
						area_ts.setDate2Original(date2);
						area_ts.setDataUnits("ACRE");
						area_ts.setDataUnitsOriginal("ACRE");
						area_ts.setDescription(name);
						tsident = new TSIdent();
						tsident.setLocation(id);
						tsident.setSource("StateCU");
						tsident.setInterval("Year");
						tsident.setType("CropArea-AllCrops");
						tsident.setInputType("StateCU");
						tsident.setInputName(full_filename);
						area_ts.setIdentifier(tsident);
						tslist.Add(area_ts);
					}

					// Monthly data...

					if (include_mts)
					{
						mts = new MonthTS();
						mts.setDate1(date1);
						mts.setDate2(date2);
						mts.setDate1Original(date1);
						mts.setDate2Original(date2);
						mts.setDataUnits("ACFT");
						mts.setDataUnitsOriginal("ACFT");
						mts.setDescription(name);
						tsident = new TSIdent();
						tsident.setLocation(id);
						tsident.setSource("StateCU");
						tsident.setInterval("MONTH");
						tsident.setType(file_data_type);
						tsident.setInputType("StateCU");
						tsident.setInputName(full_filename);
						mts.setIdentifier(tsident);
						tslist.Add(mts);
					}

					// Yearly data...

					if (include_yts)
					{
						yts = new YearTS();
						yts.setDate1(date1);
						yts.setDate2(date2);
						yts.setDate1Original(date1);
						yts.setDate2Original(date2);
						yts.setDataUnits("ACFT");
						yts.setDataUnitsOriginal("ACFT");
						yts.setDescription(name);
						tsident = new TSIdent();
						tsident.setLocation(id);
						tsident.setSource("StateCU");
						tsident.setInterval("Year");
						tsident.setType(file_data_type);
						tsident.setInputType("StateCU");
						tsident.setInputName(full_filename);
						yts.setIdentifier(tsident);
						tslist.Add(yts);
					}

					// Yearly depth...

					if (include_depth_ts)
					{
						depth_ts = new YearTS();
						depth_ts.setDate1(date1);
						depth_ts.setDate2(date2);
						depth_ts.setDate1Original(date1);
						depth_ts.setDate2Original(date2);
						depth_ts.setDataUnits("ACFT/AC");
						depth_ts.setDataUnitsOriginal("ACFT/AC");
						depth_ts.setDescription(name);
						tsident = new TSIdent();
						tsident.setLocation(id);
						tsident.setSource("StateCU");
						tsident.setInterval("Year");
						tsident.setType(file_data_type + "_Depth");
						tsident.setInputType("StateCU");
						tsident.setInputName(full_filename);
						depth_ts.setIdentifier(tsident);
						tslist.Add(depth_ts);
					}

					if (!include_area_ts && !include_mts && !include_yts && !include_depth_ts)
					{
						// Don't want any of the time series
						// so no need to process...
						break;
					}

					if (read_data)
					{
						// Allocate data space and
						// transfer the data...
						if (include_area_ts)
						{
							area_ts.allocateDataSpace();
						}
						if (include_mts)
						{
							mts.allocateDataSpace();
						}
						if (include_yts)
						{
							yts.allocateDataSpace();
						}
						if (include_depth_ts)
						{
							depth_ts.allocateDataSpace();
						}
						// Now loop through and parse
						// the data
						// for the time series.
						for (i = 0; i < size; i++)
						{
							v = StringUtil.fixedRead((string) ilines[i], format, format_w, v);
							year = ((int?)v[0]).Value;
							date.setYear(year);
							date.setMonth(1);
							if (include_area_ts)
							{
								area_ts.setDataValue(date, ((double?)v[1]).Value);
							}
							if (include_mts)
							{
								for (icol = 2; icol < 14; icol++, date.addMonth(1))
								{
									mts.setDataValue(date, ((double?)v[icol]).Value);
								}
								// Reset year...
								date.setYear(year);
							}
							if (include_yts)
							{
								yts.setDataValue(date, ((double?)v[14]).Value);
							}
							if (include_depth_ts)
							{
								depth_ts.setDataValue(date, ((double?)v[15]).Value);
							}
						}
					}
					// Done processing this time series so break out
					// and start the search for a new location
					// identifier...
					break;
				}
				else
				{ // Else add to data vector so it can be
					// processed when the end of this time series
					// is detected...
					ilines.Add(iline);
				}
			}
			if ((!string.ReferenceEquals(req_id, null)) && !req_id_has_wildcards && req_id_found)
			{
				// No need to read more because
				// requested time series was processed.
				break;
			}
		}

		} // Main try around routine.
		catch (Exception e)
		{
			Message.printWarning(2, routine, "Error reading file near line " + line_count);
			Message.printWarning(2, routine, e);
			throw new Exception("Error reading StateCU file");
		}
		return tslist;
	}

	/// <summary>
	/// Write frost dates time series to a StateCU file. </summary>
	/// <param name="tslist"> A Vector of frost dates time series, where the data are Julian
	/// days as double (Jan 1 = 1).  Each location (e.g.,
	/// station) should have four time series of data type "FrostDateL28S",
	/// "FrostDateL32S", "FrostDateF32F", and "FrostDateF28F".  The four time series
	/// will be grouped and output together. </param>
	/// <param name="output"> Output file to write. </param>
	/// <param name="comments"> Comments to put at the top of the output file. </param>
	/// <param name="req_date1"> Requested output start of period (only year is used). </param>
	/// <param name="req_date2"> Requested output end of period (only year is used). </param>
	/// <exception cref="Exception"> if an error occurs writing the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeFrostDatesFile(java.util.List<RTi.TS.TS> tslist, String outfile, String [] newcomments, RTi.Util.Time.DateTime req_date1, RTi.Util.Time.DateTime req_date2) throws Exception
	public static void writeFrostDatesFile(IList<TS> tslist, string outfile, string[] newcomments, DateTime req_date1, DateTime req_date2)
	{
		PrintWriter @out;
		string[] comment_str = new string[] {"#"};
		string[] ignore_str = new string[] {"#>"};
		string rtn = "StateCU_TS.writeFrostDatesFile";

		Message.printStatus(1, rtn, "Writing new time series to file \"" + outfile + "\"");

		// Process the header from the old file...

		@out = IOUtil.processFileHeaders((string)null, IOUtil.getPathUsingWorkingDir(outfile), newcomments, comment_str, ignore_str, 0);
		if (@out == null)
		{
			Message.printWarning(2, rtn, "Error writing time series to \"" + outfile + "\"");
			throw new Exception("Error writing time series to \"" + outfile + "\"");
		}

		// Get the contents of the file...

		IList<string> strings = formatFrostDatesOutput(tslist, req_date1, req_date2);
		// Now write the new data...

		int size = 0;
		if (strings != null)
		{
			size = strings.Count;
		}
		for (int i = 0; i < size; i++)
		{
			@out.println((string)strings[i]);
		}

		@out.flush();
		@out.close();
	}

	/// <summary>
	/// Format output for printing or display.  This formats the time series similar
	/// to a standard StateMod time series, but data values are dates. </summary>
	/// <param name="tslist"> Vector of time series to format, containing four time series per
	/// location (see the writeFrostDatesFile() method for more information). </param>
	/// <param name="req_date1"> Requested start year, or null to write all data. </param>
	/// <param name="req_date2"> Requested end year, or null to write all data. </param>
	/// <returns> Vector of String suitable for a report or file. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static java.util.List<String> formatFrostDatesOutput(java.util.List<RTi.TS.TS> tslist, RTi.Util.Time.DateTime req_date1, RTi.Util.Time.DateTime req_date2) throws Exception
	private static IList<string> formatFrostDatesOutput(IList<TS> tslist, DateTime req_date1, DateTime req_date2)
	{
		string cmnt = "#>"; // Non-permanent comment for header.
		string message = null;
		string rtn = "StateCU_TS.formatFrostDatesOutput";
		IList<string> strings = new List<string> (50, 50);
		IList<object> v = new List<object> (50);

		if (Message.isDebugOn)
		{
			Message.printStatus(1, rtn, "Creating frost dates output " + "for " + req_date1 + " to " + req_date2);
		}

		if (tslist == null)
		{
			Message.printWarning(2, rtn, "Null time series list");
			return strings;
		}

		// First get a list of the unique identifiers in the time series list.
		// This defines the locations.

		int size = tslist.Count;
		IList<string> ids = new List<string>(size / 4 + 4);
		TS ts;
		int ids_size = 0;
		bool found = false;
		string id;
		int j = 0; // Reused in loop below
		for (int i = 0; i < size; i++)
		{
			ts = tslist[i];
			if (ts == null)
			{
				continue;
			}
			found = false;
			id = ts.getLocation();
			ids_size = ids.Count;
			for (j = 0; j < ids_size; j++)
			{
				if (id.Equals(ids[j], StringComparison.OrdinalIgnoreCase))
				{
					found = true;
					break;
				}
			}
			if (!found)
			{
				// The location was not found so add it to the list...
				ids.Add(id);
			}
		}

		// Now allocate an array to organize the time series.  This might not
		// be needed if it was guaranteed that the time series are in the proper
		// order, but do not assume so.  The rows in the array are for the
		// location and the columns correspond to the time series for the day of
		// the year for the last spring 28F, last spring 32F, first fall 32F and
		// first fall.

		TS[][] ts_array = new TS[ids.Count][];
		ids_size = ids.Count; // Update for lass add.
		// Data types that are expected - might pass this in as an argument to
		// make it more flexible...
		string[] datatypes = new string[] {"FrostDateL28S", "FrostDateL32S", "FrostDateF32F", "FrostDateF28F"};
		int idatatype = 0; // Counter for data types to match.
		for (int i = 0; i < ids_size; i++)
		{
			// Allocate the time series...
			ts_array[i] = new TS[4];
			// Initialize to null to indicate missing time series...
			ts_array[i][0] = ts_array[i][1] = ts_array[i][2] = ts_array[i][3] = null;
			// Fill up the rows in the order of the ids, finding time series
			// of the proper data types for the columns...
			id = (string)ids[i];
			// Loop through the original full list of time series to find
			// ones that match the identifier and data type...
			for (idatatype = 0; idatatype < 4; idatatype++)
			{
				for (j = 0; j < size; j++)
				{
					ts = (TS)tslist[j];
					if (ts.getLocation().equalsIgnoreCase(id) && ts.getDataType().equalsIgnoreCase(datatypes[idatatype]))
					{
						ts_array[i][idatatype] = ts;
					}
				}
			}
		}

		// Now output the time series...

		try
		{

		string iline, iline_format;
		int i, year;

		int nseries = ids_size;

		// If the period of record of interest was not requested, find period of
		// record which covers all time series...

		int earliest_year = 0, latest_year = 0;
		if ((req_date1 == null) || (req_date2 == null))
		{
			// One of the requested dates is null so get the period from the
			// data...
			try
			{
				TSLimits valid_dates = TSUtil.getPeriodFromTS(tslist, TSUtil.MAX_POR);
				if (req_date1 == null)
				{
					earliest_year = valid_dates.getDate1().getYear();
				}
				else
				{
					earliest_year = req_date1.getYear();
				}
				if (req_date2 == null)
				{
					latest_year = valid_dates.getDate2().getYear();
				}
				else
				{
					latest_year = req_date2.getYear();
				}
			}
			catch (Exception)
			{
				Message.printWarning(2, rtn, "Unable to determine output period.");
				throw new Exception("Unable to determine output period.");
			}
		}
		else
		{ // The requested dates have been specified...
			if (req_date1 != null)
			{
				earliest_year = req_date1.getYear();
			}
			if (req_date2 != null)
			{
				latest_year = req_date2.getYear();
			}
		}

		// Print comments at the top of the file...

		strings.Add(cmnt);
		strings.Add(cmnt);
		strings.Add(cmnt + " StateCU frost dates time series");
		strings.Add(cmnt + " *******************************");
		strings.Add(cmnt);
		strings.Add(cmnt);

		// Always do calendar year...

		strings.Add(cmnt + " Years Shown = Calendar Years");

		strings.Add(cmnt + " The period of record for each time series may vary");
		strings.Add(cmnt + " because of the original input and data processing steps.");
		strings.Add(cmnt);
		strings.Add(cmnt);

		// Print each time series id, description, and type...

		strings.Add(cmnt + "     TS ID                    Type" + "   Source   Units  Period of Record    Location    Description");

		string empty_string = "-", tmpdesc, tmpid, tmplocation, tmpsource, tmptype, tmpunits;
		string format;
		for (i = 0; i < nseries; i++)
		{
			ts = (TS)tslist[i];

			tmpid = ts.getIdentifierString();
			if (tmpid.Length == 0)
			{
				tmpid = empty_string;
			}

			tmpdesc = ts.getDescription();
			if (tmpdesc.Length == 0)
			{
				tmpdesc = empty_string;
			}

			tmptype = ts.getIdentifier().getType();
			if (tmptype.Length == 0)
			{
				tmptype = empty_string;
			}

			tmpsource = ts.getIdentifier().getSource();
			if (tmpsource.Length == 0)
			{
				tmpsource = empty_string;
			}

			tmpunits = ts.getDataUnits();
			if (tmpunits.Length == 0)
			{
				tmpunits = empty_string;
			}

			tmplocation = ts.getIdentifier().getLocation();
			if (tmplocation.Length == 0)
			{
				tmplocation = empty_string;
			}

			format = "%s %3d %-24.24s %-6.6s %-8.8s %-6.6s     %d - "
				+ "%d     %-12.12s%-24.24s";
			v.Clear();
			v.Add(cmnt);
			v.Add(new int?(i + 1));
			v.Add(tmpid);
			v.Add(tmptype);
			v.Add(tmpsource);
			v.Add(tmpunits);
			v.Add(new int?(ts.getDate1().getYear()));
			v.Add(new int?(ts.getDate2().getYear()));
			v.Add(tmplocation);
			v.Add(tmpdesc);

			iline = StringUtil.formatString(v, format);
			strings.Add(iline);
		}
		strings.Add(cmnt);
		strings.Add(cmnt);
		strings.Add(cmnt);

		// Ready to print table
		//
		// check a few conditions which would end this routine...
		if (nseries == 0)
		{
			strings.Add("No time series data.");
			return strings;
		}
		for (i = 0; i < nseries; i++)
		{
			ts = (TS)tslist[i];
			if (ts.getDataIntervalBase() != TimeInterval.YEAR)
			{
				message = "A TS interval other than year detected:" + ts.getDataIntervalBase();
				Message.printWarning(2, rtn, message);
				throw new Exception(message);
			}
		}

		strings.Add(cmnt + "Temperatures are degrees F");
		strings.Add(cmnt);
		strings.Add(cmnt + "               Last    Last    First   First");
		strings.Add(cmnt + " Yr ID         Spr 28  Spr 32  Fall 32 Fall 28");

		strings.Add(cmnt + "-e-b----------eb------eb------eb------eb------e");
		strings.Add(cmnt + "EndHeader");

		// For now, always output in calendar year...

		strings.Add("    1/" + StringUtil.formatString(earliest_year,"%4d") + "  -     12/" + StringUtil.formatString(latest_year,"%4d") + " DATE  CYR");

		double days32fall, days28fall, days28spring, days32spring;
		double missing = -999.0;
		double? missing_Double = new double?(missing);

		IList<object> iline_v = new List<object>(30);
		DateTime date = new DateTime(DateTime.PRECISION_YEAR);
		date.setYear(earliest_year);
		int[] date_array; // Returned from TimeUtil below - reused.
		// Loop through the number of years...
		for (; date.getYear() <= latest_year ; date.addYear(1))
		{
			year = date.getYear();
			// Loop through the number of identifiers...
			for (j = 0; j < nseries; j++)
			{
				// First, clear this string out, then append to it
				iline_v.Clear();

				// This is always the same...

				iline_format = "%4d %-12.12s";
				iline_v.Add(new int?(year));
				iline_v.Add((string)ids[j]);

				// Get the data from the time series...

				if (ts_array[j][0] == null)
				{
					days28spring = missing;
				}
				else
				{
					days28spring = ts_array[j][0].getDataValue(date);
				}
				if (ts_array[j][1] == null)
				{
					days32spring = missing;
				}
				else
				{
					days32spring = ts_array[j][1].getDataValue(date);
				}
				if (ts_array[j][2] == null)
				{
					days32fall = missing;
				}
				else
				{
					days32fall = ts_array[j][2].getDataValue(date);
				}
				if (ts_array[j][3] == null)
				{
					days28fall = missing;
				}
				else
				{
					days28fall = ts_array[j][3].getDataValue(date);
				}

				// Now format for output...

				if (days28spring < 0.0)
				{
					// No date...
					iline_format += "%8d";
					iline_v.Add(missing_Double);
				}
				else
				{
					iline_format += "%8.8s";
					date_array = TimeUtil.getMonthAndDayFromDayOfYear(year, (int)(days28spring + .01));
					iline_v.Add(StringUtil.formatString(date_array[0],"%02d") + "/" + StringUtil.formatString(date_array[1],"%02d"));
				}
				if (days32spring < 0.0)
				{
					// No date...
					iline_format += "%8d";
					iline_v.Add(missing_Double);
				}
				else
				{
					iline_format += "%8.8s";
					date_array = TimeUtil.getMonthAndDayFromDayOfYear(year, (int)(days32spring + .01));
					iline_v.Add(StringUtil.formatString(date_array[0],"%02d") + "/" + StringUtil.formatString(date_array[1],"%02d"));
				}
				if (days32fall < 0.0)
				{
					// No date...
					iline_format += "%8d";
					iline_v.Add(missing_Double);
				}
				else
				{
					iline_format += "%8.8s";
					date_array = TimeUtil.getMonthAndDayFromDayOfYear(year, (int)(days32fall + .01));
					iline_v.Add(StringUtil.formatString(date_array[0],"%02d") + "/" + StringUtil.formatString(date_array[1],"%02d"));
				}
				if (days28fall < 0.0)
				{
					// No date...
					iline_format += "%8d";
					iline_v.Add(missing_Double);
				}
				else
				{
					iline_format += "%8.8s";
					date_array = TimeUtil.getMonthAndDayFromDayOfYear(year, (int)(days28fall + .01));
					iline_v.Add(StringUtil.formatString(date_array[0],"%02d") + "/" + StringUtil.formatString(date_array[1],"%02d"));
				}
				strings.Add(StringUtil.formatString(iline_v, iline_format));
			}

		}
		}
		catch (Exception)
		{
			message = "Unable to format frost dates output.";
			Message.printWarning(2, rtn, message);
			throw new Exception(message);
		}
		return strings;
	}

	} // End StateCU_TS

}