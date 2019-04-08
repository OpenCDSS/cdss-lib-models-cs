﻿using System;
using System.Collections.Generic;
using System.Text;

// StateMod_BTS - read/write time series from StateMod binary file(s)

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
// StateMod_BTS - read/write time series from StateMod binary file(s)
// ----------------------------------------------------------------------------
// History:
//
// 2003-03-12	Steven A. Malers, RTi	Initial version.  Copy RTi.TS.BinaryTS
//					and update for the StateMod binary file
//					format (.b43).
// 2003-10-20	SAM, RTi		* Rename StateModXTS to Statemod_BTS.
//					* Change TSDate to DateTime.
//					* Change TS.INTERVAL* to TimeInterval.
// 2003-10-28	SAM, RTi		* Convert some messages to debugs to
//					  improve performance.
//					* Keep around lists of the various
//					  station IDs and names from the header
//					  to use when creating the time series.
//				 	* Also, since the data are in the order
//					  of river nodes but the requested TSID
//					  uses the station ID, the header data
//					  must be examined to find a match using
//					  the determineRiverStatino() method.
// 2003-11-03	SAM, RTi		* Update to new StateMod 10.43 binary
//					  file format - Ray Bennett has added
//					  the river node ID next to the station
//					  ID!  This allows easy lookup of the
//					  river node to match the station.
//					  Still keep the header information in
//					  memory for now but later may optimize
//					  to jump through the header as needed.
//					* Use xfrnam instead of ifrnam because
//					  Ray has indicated the former in the
//					  documentation.
//					* Rename some private methods to be
//					  specific to B43, in anticipation of
//					  needing to read more files.
//					* Convert all B43 parameters from CFS to
//					  ACFT for output.
//					* Figured out how to read names - just
//					  read little endian char 1!
// 2003-11-14	SAM, RTi		* Add support for binary reservoir and
//					  well files and also all binary daily
//					  files.
//					* Get the list of parameters from the
//					  StateMod_Util class rather than
//					  duplicating the list here.
// 2003-11-26	SAM, RTi		* Finalize support for other data types.
//					* When initializing the object, set the
//					  __comp_type member data - then when
//					  processing data, only search arrays
//					  for the appropriate type of data in
//					  the file.  For example, for the well
//					  file, there is no reason to search
//					  diversion and reservoir identifiers.
// 2004-02-04	SAM, RTi		* Fix bug where duplicate time series
//					  were being matched in readTimeSeries()
//					  because baseflow station IDs are
//					  sometimes the same as other station
//					  IDs.
// 2004-03-15	SAM, RTi		* Add getFileFilter - REVISIT.
//					* Allow a null TSID pattern to be passed
//					  to readTimeSeriesList() - treat null
//					  as "*".
//					* Fix bug where only one data type could
//					  be read in readTimeSeriesList().
// 2004-08-23	SAM, RTi		* Overload readTimeSeries() to have a
//					  PropList to allow additional
//					  customization of reads - however, go
//					  ahead and implement a Hashtable for
//					  file management to use as the default.
//					  There does not seem to be a downside
//					  to this.
// 2005-12-21	SAM, RTi		* Include support for StateMod 11.0
//					  file format updates.
//					* Troubleshoot why reservoir time series
//					  are not being read properly.
//					* Remove setVersion() since the version
//					  really needs to be set in the
//					  constructor.
// 2005-12-22	SAM, RTi		* Add __nowner2 that is the number of
//					  accounts, not cumulative, and
//					  __nowner2_cum, which is cumulative.
// 2006-01-03	SAM, RTi		* Resolve final issues with
//					  new parameter lists and binary
//					  reservoirs.
// 2006-01-04	SAM, RTi		* Finalize recent reservoir file
//					  changes.
// 2006-01-05	SAM, RTi		* One last change because inactive
//					  accounts are at the end of the block.
// 2006-01-15	SAM, RTi		* Add determineFileVersion() to get
//					  the StateMod version from the file,
//					  in order to make other decisions.
//					  This is mainly intented for use
//					  during transition to the new format
//					  where more information is in the
//					  file header.
//					* Add getParameters() to return the
//					  list of parameter names, for use in
//					  displays, etc.
// 2006-07-06	SAM, RTi		* Fix bug where month data were not
//					  being properly initialized for
//					  irrigation year data (Nov-Oct).
// 2007-01-17	SAM, RTi		* Fix bug where determining the file
//					  version needed to use different bytes
//					  because the previous test was failing.
// 2007-01-18	SAM, RTi		* Allow lookup of station that is of
//					  type "other" (only in river network
//					  file *rin) to be looked up.
//					* Fix so dash in identifier is allowed
//					  for other than reservoirs (and
//					  reservoirs use for account).
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
// ----------------------------------------------------------------------------
// EndHeader

namespace DWR.StateMod
{

	using DayTS = RTi.TS.DayTS;
	using MonthTS = RTi.TS.MonthTS;
	using TS = RTi.TS.TS;
	using TSIdent = RTi.TS.TSIdent;
	using EndianRandomAccessFile = RTi.Util.IO.EndianRandomAccessFile;
	using IOUtil = RTi.Util.IO.IOUtil;
	using PropList = RTi.Util.IO.PropList;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;
	using DateTime = RTi.Util.Time.DateTime;
	using TimeInterval = RTi.Util.Time.TimeInterval;
	using TimeUtil = RTi.Util.Time.TimeUtil;

	/// <summary>
	/// Provide an interface to a StateMod binary files.  Binary data are organized as follows:
	/// <pre>
	/// Station Type     Monthly File     Daily File
	/// --------------------------------------------
	/// diversion        *.b43            *.b49
	/// instream flow    *.b43            *.b49
	/// reservoir        *.b44            *.b50
	/// stream gage/est  *.b43            *.b49
	/// well             *.b42            *.b65
	/// ---------------------------------------------------------------------------
	/// </pre>
	/// The file and format is determined based on the file extension and/or the
	/// requested parameter and data interval.  Typically the readTimeSeries() or
	/// readTimeSeriesList() methods are used, which open a file, read one or more
	/// time series, and close the file.
	/// All the methods in this class that use time series index numbers use 0 for the first time series.
	/// The format of the file is described in StateMod documentation.  Each file has
	/// essentially the same header information, followed by data records.  Relevant notes:
	/// <ol>
	/// <li>	The individual station lists are used to look up a station identifier.
	/// The river node number is then used to find the position in the main river node list.</li>
	/// <li>	The time series are written in one month blocks, with time series
	/// within the block listed in the order of the river nodes for stream/
	/// diversion/ISF and in the order of the specific list for reservoirs and wells.</li>
	/// <li>	For reservoirs, a station's time series consist of a total, and one for
	/// each account (therefore the reservoir file has a total number of time series that is the
	/// reservoir count (for totals) plus the number of accounts.</li>
	/// </ol>
	/// </summary>
	public class StateMod_BTS
	{

	/// <summary>
	/// Conversion factor from CFSto ACFT - also need to multiply by days in month.
	/// </summary>
	private readonly float CFS_TO_ACFT = (float)1.9835;

	/// <summary>
	/// File format version as a String (e.g., "9.62").  Before version 11.0 there was no version in the binary file
	/// so the code just reflects the documented format for the 9.62 version, especially
	/// since no documentation exists for earlier versions of the binary files.
	/// </summary>
	private string __version = null;

	/// <summary>
	/// Program that created the file, typically "StateMod".
	/// </summary>
	private string __headerProgram = "";
	/// <summary>
	/// Date for the software version, in format YYYY/MM/DD.
	/// </summary>
	private string __headerDate = "";

	// Data members...

	/// <summary>
	/// Name of the binary file being operated on (may or may not be an absolute path).
	/// </summary>
	private string __tsfile;
	/// <summary>
	/// Full path to binary file being operated on.  This is used as the key in the __file_Hashtable.
	/// </summary>
	private string __tsfileFull;
	/// <summary>
	/// Pointer to random access file (StateMod binary files are assumed to be little endian since they are written
	/// by Lahey FORTRAN code on a PC).  If necessary, the year value can be examined to determine the file endian-ness.
	/// </summary>
	private EndianRandomAccessFile __fp;
	/// <summary>
	/// A hashtable for the file pointers (instances of StateMod_BTS).  This is used to increase performance.
	/// </summary>
	private static Dictionary<string, StateMod_BTS> __file_Hashtable = new Dictionary<string, StateMod_BTS>();
	/// <summary>
	/// Direct access file record length, bytes.  140 is the B43 for 9.62, but this is reset below.
	/// </summary>
	private int __recordLength = 140;
	/// <summary>
	/// Length of the header in bytes, including lists of stations (everything before the time series data).  This
	/// is assigned after reading the number of stations.  The first record format various between versions but
	/// always fits into one record.
	/// </summary>
	private int __headerLength = 0;
	/// <summary>
	/// Number of bytes for one full interval (month or day) of data for all stations, to
	/// simplify iterations.  This is assigned after reading the number of stations.
	/// </summary>
	private int __intervalBytes = 0;
	/// <summary>
	/// Estimated size of the file, calculated from the header information - used for
	/// debugging and to check for premature end of file.
	/// </summary>
	private int __estimatedFileLengthBytes = 0;
	/// <summary>
	/// Interval base for the binary file that is being read.
	/// </summary>
	private int __intervalBase = TimeInterval.MONTH;
	/// <summary>
	/// Start of the data, in calendar year, to proper date precision.
	/// </summary>
	private DateTime __date1 = null;
	/// <summary>
	/// End of the data, in calendar year, to proper date precision.
	/// </summary>
	private DateTime __date2 = null;
	/// <summary>
	/// Number of parameters for each data record, for the current file.  Set below depending on
	/// file contents and version.  This will be set equal to one of __ndivO, __nresO, __nwelO.
	/// </summary>
	private int __numparm = 0;
	/// <summary>
	/// Maximum length of parameter list, for all files.
	/// </summary>
	private int __maxparm = 0;
	/// <summary>
	/// Number of parameters specific to the diversion file.
	/// </summary>
	private int __ndivO = 0;
	/// <summary>
	/// Number of parameters specific to the reservoir file.
	/// </summary>
	private int __nresO = 0;
	/// <summary>
	/// Number of parameters specific to the well file.
	/// </summary>
	private int __nwelO = 0;
	/// <summary>
	/// List of the official parameter names.
	/// </summary>
	private string[] __parameters = null;
	/// <summary>
	/// Units for each parameter.
	/// </summary>
	private string[] __unit = null;
	/// <summary>
	/// Component type for the binary file: COMP_DIVERSION_STATIONS, COMP_RESERVOIR_STATIONS, or COMP_WELL_STATIONS.
	/// </summary>
	private int __comp_type = StateMod_DataSet.COMP_UNKNOWN;

	// Binary header information, according to the StateMod documentation.
	// Currently, only the B43 header information is listed.

	/// <summary>
	/// Beginning year of simulation.
	/// </summary>
	private int __iystr0 = 0;
	/// <summary>
	/// Ending year of simulation.
	/// </summary>
	private int __iyend0 = 0;
	/// <summary>
	/// Number of river nodes.
	/// </summary>
	private int __numsta = 0;
	/// <summary>
	/// Number of diversions.
	/// </summary>
	private int __numdiv = 0;
	/// <summary>
	/// Number of instream flow locations.
	/// </summary>
	private int __numifr = 0;
	/// <summary>
	/// Number of reservoirs.
	/// </summary>
	private int __numres = 0;
	/// <summary>
	/// Number of reservoir owners.
	/// </summary>
	private int __numown = 0;
	/// <summary>
	/// Number of active reservoirs.
	/// </summary>
	private int __nrsact = 0;
	/// <summary>
	/// Number of baseflow (stream gage + stream estimate)
	/// </summary>
	private int __numrun = 0;
	/// <summary>
	/// Number of wells.
	/// </summary>
	private int __numdivw = 0;
	/// <summary>
	/// Number of ?
	/// </summary>
	private int __numdxw = 0;
	/// <summary>
	/// List of month names, used to determine whether the data are water or calendar year.
	/// </summary>
	private string[] __xmonam = null;
	/// <summary>
	/// Number of days per month, corresponding to __xmonam.  This is used to convert CFS
	/// to ACFT.  Note February always has 28 days.
	/// </summary>
	private int[] __mthday = null;
	/// <summary>
	/// __mthday, always in calendar order.
	/// </summary>
	private int[] __mthdayCalendar = null;
	/// <summary>
	/// List of river node IDs.  The data records are in this order.
	/// </summary>
	private string[] __cstaid = null;
	/// <summary>
	/// Station names for river nodes.
	/// </summary>
	private string[] __stanam = null;
	/// <summary>
	/// List of diversion IDs.
	/// </summary>
	private string[] __cdivid = null;
	/// <summary>
	/// Diversion names.
	/// </summary>
	private string[] __divnam = null;
	/// <summary>
	/// River node position for diversion (1+).
	/// </summary>
	private int[] __idvsta = null;
	/// <summary>
	/// List of instream flow IDs.
	/// </summary>
	private string[] __cifrid = null;
	/// <summary>
	/// Instream flow names.
	/// </summary>
	private string[] __xfrnam = null;
	/// <summary>
	/// River node position for instream flow (1+).
	/// </summary>
	private int[] __ifrsta = null;
	/// <summary>
	/// List of reservoir IDs.
	/// </summary>
	private string[] __cresid = null;
	/// <summary>
	/// Reservoir names.
	/// </summary>
	private string[] __resnam = null;
	/// <summary>
	/// River node position for reservoir (1+).
	/// </summary>
	private int[] __irssta = null;
	/// <summary>
	/// Indicates whether reservoir is on or off.  Reservoirs that are off do not have output records.
	/// </summary>
	private int[] __iressw = null;
	/// <summary>
	/// Number of owners (accounts) for each reservoir, cumulative, and does not include, totals, which are
	/// stored as account 0 for each reservoir.
	/// </summary>
	private int[] __nowner = null;
	/// <summary>
	/// Number of owners (accounts) for each reservoir (not cumulative like __nowner).
	/// This DOES include the total account, which is account 0.
	/// </summary>
	private int[] __nowner2 = null;
	/// <summary>
	/// Number of owners (accounts) for each reservoir, cumulative, including the current reservoir.  This includes the total
	/// and is only for active reservoirs.  This is used when figuring out how many records to skip for previous stations.
	/// </summary>
	private int[] __nowner2_cum = null;
	/// <summary>
	/// Number of owners (accounts) for each reservoir, cumulative, taking into account that inactive reservoirs are at
	/// the end of the list of time series and can be ignored.
	/// </summary>
	private int[] __nowner2_cum2 = null;
	/// <summary>
	/// List of stream gage and stream estimate IDs (nodes that have baseflows).
	/// </summary>
	private string[] __crunid = null;
	/// <summary>
	/// Stream gage and stream estimate names.
	/// </summary>
	private string[] __runnam = null;
	/// <summary>
	/// River node position for station (1+).
	/// </summary>
	private int[] __irusta = null;
	/// <summary>
	/// List of well IDs.
	/// </summary>
	private string[] __cdividw = null;
	/// <summary>
	/// Well names.
	/// </summary>
	private string[] __divnamw = null;
	/// <summary>
	/// River node position for well (1+).
	/// </summary>
	private int[] __idvstw = null;

	/// <summary>
	/// Open a binary StateMod binary time series file.  It is assumed that the file
	/// exists and should be opened as read-only because typically only StateMod writes
	/// to the file.  The header information is immediately read and is available for
	/// access by other methods.  After opening the file, the readTimeSeries*() methods
	/// can be called to read time series using time series identifiers. </summary>
	/// <param name="tsfile"> Name of binary file to write. </param>
	/// <exception cref="IOException"> if unable to open the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public StateMod_BTS(String tsfile) throws java.io.IOException
	public StateMod_BTS(string tsfile)
	{ // Initialize the file using the version in the header if available...
		initialize(tsfile, "");
	}

	/// <summary>
	/// Open a binary StateMod binary time series file.  It is assumed that the file
	/// exists and should be opened as read-only because typically only StateMod writes
	/// to the file.  The header information is immediately read and is available for
	/// access by other methods.  After opening the file, the readTimeSeries*() methods
	/// can be called to read time series using time series identifiers. </summary>
	/// <param name="tsfile"> Name of binary file to write. </param>
	/// <param name="fileVersion"> Version of StateMod that wrote the file.
	/// This is used, for example, by some TSTool commands to read old file formats. </param>
	/// <exception cref="IOException"> if unable to open the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public StateMod_BTS(String tsfile, String fileVersion) throws java.io.IOException
	public StateMod_BTS(string tsfile, string fileVersion)
	{
		initialize(tsfile, fileVersion);
	}

	/// <summary>
	/// Calculate the file position in bytes for any data value.  This DOES NOT position
	/// the file pointer!  For example, use this method as follows:
	/// <pre>
	/// // Find the month of data for a station and parameter...
	/// long pos = calculateFilePosition ( date, ista, its, iparam );
	/// // Position the file...
	/// __fp.seek ( pos );
	/// // Read the data...
	/// param = __fp.readLittleEndianFloat ();
	/// </pre> </summary>
	/// <param name="date"> Date to find.  The month and year are considered by using an
	/// absolute month offset (year*12 + month). </param>
	/// <param name="ista"> Station index (0+).  For streamflow/diversion/ISF, this is the
	/// position in the river node list.  For reservoirs and wells, it is the position
	/// in the specific list (which is in a different order than the river nodes). </param>
	/// <param name="its"> Time series for a location/parameter combination.  Normally this will
	/// be zero.  For reservoirs, it will be zero for the total time series and 1+ for
	/// the owner/accounts for a reservoir. </param>
	/// <param name="iparam"> Parameter to find (0+). </param>
	/// <returns> byte position in file for requested parameter or -1 if unable to calculate. </returns>
	private long calculateFilePosition(DateTime date, int ista, int its, int iparam)
	{
		long pos = -1;
		if (__intervalBase == TimeInterval.MONTH)
		{
			if (__comp_type == StateMod_DataSet.COMP_RESERVOIR_STATIONS)
			{
				int nowner2_cum_prev = 0;
				if (ista > 0)
				{
					nowner2_cum_prev = __nowner2_cum2[ista - 1];
				}
				pos = __headerLength + (date.getAbsoluteMonth() - __date1.getAbsoluteMonth()) * __intervalBytes + nowner2_cum_prev * __recordLength + its * __recordLength + iparam * 4;
			}
			else
			{
				// Non-reservoirs have only one time series per station.
				pos = __headerLength + (date.getAbsoluteMonth() - __date1.getAbsoluteMonth()) * __intervalBytes + ista * __recordLength + iparam * 4; // Previous parameters for this station (each value is a 4-byte float)...
			}
		}
		else
		{
			// Daily data...
			if (__comp_type == StateMod_DataSet.COMP_RESERVOIR_STATIONS)
			{
				int nowner2_cum_prev = 0;
				if (ista > 0)
				{
					nowner2_cum_prev = __nowner2_cum2[ista - 1];
				}
				pos = __headerLength + (date.getAbsoluteMonth() - __date1.getAbsoluteMonth()) * 31 * __intervalBytes + (date.getDay() - 1) * __intervalBytes + nowner2_cum_prev * __recordLength + its * __recordLength + + iparam * 4;
			}
			else
			{
				// Non-reservoirs have only one time series per station.
				pos = __headerLength + (date.getAbsoluteMonth() - __date1.getAbsoluteMonth()) * 31 * __intervalBytes + (date.getDay() - 1) * __intervalBytes + ista * __recordLength + iparam * 4;
			}
		}
		return pos;
	}

	/// <summary>
	/// Close the binary time series file. </summary>
	/// <exception cref="IOException"> if there is an error closing the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void close() throws java.io.IOException
	public virtual void close()
	{
		__fp.close();
		// Remove from the Hashtable...
		if (__file_Hashtable.Contains(this))
		{
			__file_Hashtable.Remove(__tsfileFull);
		}
	}

	/// <summary>
	/// Close all the binary time series files that may have been opened. </summary>
	/// <exception cref="IOException"> if there is an error closing any file (all closes are
	/// attempted and an Exception is thrown if any failed). </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void closeAll() throws java.io.IOException
	public static void closeAll()
	{ // TODO SAM 2004-08-23
		// Loop through the Hashtable and remove all entries...
		// Remove from the Hashtable...

		IEnumerator<string> keysEnumeration = __file_Hashtable.Keys.GetEnumerator();

		StateMod_BTS bts = null;
		string filename = null;

		while (keysEnumeration.MoveNext())
		{
			filename = keysEnumeration.Current;
			bts = __file_Hashtable[filename];
			bts.close();
			__file_Hashtable.Remove(filename);
		}
	}

	// TODO SAM 2006-01-15 If it becomes important to read versions before 9.69,
	// add logic to check the file size and estimate from that the record length
	// that was used, and hence the file version.
	/// <summary>
	/// Determine the StateMod binary file version.  For StateMod version 11.x+, the
	/// file version can be determined from the binary file header.  For older versions,
	/// version 9.69 is returned, since this version has been in use for some time and is likely. </summary>
	/// <param name="filename"> the path to the file to check.  No adjustments to the path
	/// are made; therefore a full path should be provided to prevent errors. </param>
	/// <exception cref="Exception"> if there is an error opening or reading the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static String determineFileVersion(String filename) throws Exception
	public static string determineFileVersion(string filename)
	{
		StateMod_BTS bts = new StateMod_BTS(filename);
		string version = bts.getVersion();
		bts.close();
		bts = null;
		return version;
	}

	/// <summary>
	/// Finalize before garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~StateMod_BTS()
	{
		__fp.close();
		__fp = null;
		__tsfile = null;
		__tsfileFull = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Return the interval base (TimeInterval.MONTH or TimeInterval.DAY). </summary>
	/// <returns> the data interval base. </returns>
	public virtual int getDataIntervalBase()
	{
		return __intervalBase;
	}

	/// <summary>
	/// Return the first date in the period. </summary>
	/// <returns> the first date in the period. </returns>
	public virtual DateTime getDate1()
	{
		return __date1;
	}

	/// <summary>
	/// Return the last date in the period. </summary>
	/// <returns> the last date in the period. </returns>
	public virtual DateTime getDate2()
	{
		return __date2;
	}

	/// <summary>
	/// Return the parameter list for the file, which is determined from the file
	/// header for version 11.x+ and is unknown otherwise.   Only the public parameters
	/// are provided (not extra ones that may be used internally). </summary>
	/// <returns> the parameter list read from the file header, or null if it cannot be determined from the file. </returns>
	public virtual string [] getParameters()
	{ // Return a copy of the array for only the appropriate parameters.
		// The array by default will have __maxparm items, but not all of these
		// are appropriate for other applications.
		if ((__numparm == 0) || (__parameters == null))
		{
			return null;
		}
		string[] parameter = new string[__numparm];
		for (int i = 0; i < __numparm; i++)
		{
			parameter[i] = __parameters[i];
		}
		return parameter;
	}

	// TODO SAM 2006-01-15 If resources allow, remember to estimate the file version by back-calculating
	// from the file size.
	/// <summary>
	/// Return the version of the file (the StateMod version that wrote the file).
	/// This information is determined from the file header for version 11.x+ and is unknown otherwise. </summary>
	/// <returns> the file format version. </returns>
	public virtual string getVersion()
	{
		return __version;
	}

	/// <summary>
	/// Initialize the binary file.  The file is opened and the header is read. </summary>
	/// <param name="tsfile"> Name of binary file. </param>
	/// <param name="fileVersion"> Version of StateMod that wrote the file.  Used to be a double like "9.01" but can now
	/// be a three-part version like "10.01.01". </param>
	/// <exception cref="IOException"> If the file cannot be opened or read. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void initialize(String tsfile, String fileVersion) throws java.io.IOException
	private void initialize(string tsfile, string fileVersion)
	{
		string routine = "StateMod_BTS.initialize";
		__tsfile = tsfile;
		__version = "";
		if ((!string.ReferenceEquals(fileVersion, null)) && !fileVersion.Equals(""))
		{
			__version = fileVersion;
			if (__version.StartsWith("9.", StringComparison.Ordinal))
			{
				// Add a leading 0 so that version string comparisons work
				__version = "0" + __version;
			}
		}

		// TODO SAM 2003? - for different file extensions, change the
		// interval to TimeInterval.DAY if necessary.

		// Open the binary file as a random access endian file.  This allows
		// Big-endian Java to read the little-endian (Microsoft/Lahey) file...

		__tsfileFull = IOUtil.getPathUsingWorkingDir(tsfile);
		__fp = new EndianRandomAccessFile(__tsfileFull, "r");

		// Initialize important data...

		__file_Hashtable[__tsfileFull] = this;

		__intervalBase = TimeInterval.MONTH; // Default
		string extension = IOUtil.getFileExtension(__tsfile);

		// Read the file header version...

		if (__version.Equals(""))
		{
			readHeaderVersion();
		}

		if (extension.Equals("b43", StringComparison.OrdinalIgnoreCase))
		{
			// Diversions, instream flow, stream (monthly)...
			// Use the diversion parameter list since it is the full list.
			__comp_type = StateMod_DataSet.COMP_DIVERSION_STATIONS;
			if (StateMod_Util.isVersionAtLeast(__version, StateMod_Util.VERSION_11_00))
			{
				__recordLength = 160;
			}
			else
			{
				__recordLength = 140;
			}
		}
		else if (extension.Equals("b49", StringComparison.OrdinalIgnoreCase))
		{
			// Diversions, instream flow, stream (daily)...
			// Use the diversion parameter list since it is the full list.
			__intervalBase = TimeInterval.DAY;
			__comp_type = StateMod_DataSet.COMP_DIVERSION_STATIONS;
			if (StateMod_Util.isVersionAtLeast(__version, StateMod_Util.VERSION_11_00))
			{
				__recordLength = 160;
			}
			else
			{
				__recordLength = 144;
			}
		}
		else if (extension.Equals("b44", StringComparison.OrdinalIgnoreCase))
		{
			// Reservoirs (monthly)...
			__comp_type = StateMod_DataSet.COMP_RESERVOIR_STATIONS;
			if (StateMod_Util.isVersionAtLeast(__version, StateMod_Util.VERSION_11_00))
			{
				__recordLength = 160;
			}
			else
			{
				__recordLength = 96;
			}
		}
		else if (extension.Equals("b50", StringComparison.OrdinalIgnoreCase))
		{
			// Reservoirs (daily)...
			__intervalBase = TimeInterval.DAY;
			__comp_type = StateMod_DataSet.COMP_RESERVOIR_STATIONS;
			if (StateMod_Util.isVersionAtLeast(__version, StateMod_Util.VERSION_11_00))
			{
				__recordLength = 160;
			}
			else
			{
				__recordLength = 96;
			}
		}
		else if (extension.Equals("b42", StringComparison.OrdinalIgnoreCase))
		{
			// Well (monthly)...
			__comp_type = StateMod_DataSet.COMP_WELL_STATIONS;
			// Same for all versions...
			__recordLength = 92;
		}
		else if (extension.Equals("b65", StringComparison.OrdinalIgnoreCase))
		{
			// Well (daily)...
			__intervalBase = TimeInterval.DAY;
			__comp_type = StateMod_DataSet.COMP_WELL_STATIONS;
			// Same for all versions...
			__recordLength = 92;
		}

		// Read the file header...

		readHeader();

		if (Message.isDebugOn)
		{
			Message.printDebug(1, routine, "Parameters are as follows, where " + "the number equals iparam in following messages.");
			for (int iparam = 0; iparam < __numparm; iparam++)
			{
				Message.printDebug(1, "", "Parameter [" + iparam + "]=\"" + __parameters[iparam] + "\"");
			}
		}

		// Set the dates...

		if (__intervalBase == TimeInterval.MONTH)
		{
			__date1 = new DateTime(DateTime.PRECISION_MONTH);
			__date2 = new DateTime(DateTime.PRECISION_MONTH);
		}
		else
		{
			__date1 = new DateTime(DateTime.PRECISION_DAY);
			__date2 = new DateTime(DateTime.PRECISION_DAY);
		}

		// Set the day in all cases.  It will be ignored with monthly data...

		if (__xmonam[0].Equals("JAN", StringComparison.OrdinalIgnoreCase))
		{
			// Calendar...
			__date1.setYear(__iystr0);
			__date1.setMonth(1);
			__date2.setYear(__iyend0);
			__date2.setMonth(12);
		}
		else if (__xmonam[0].Equals("OCT", StringComparison.OrdinalIgnoreCase))
		{
			// Water year...
			__date1.setYear(__iystr0 - 1);
			__date1.setMonth(10);
			__date2.setYear(__iyend0);
			__date2.setMonth(9);
		}
		else if (__xmonam[0].Equals("NOV", StringComparison.OrdinalIgnoreCase))
		{
			// Irrigation year...
			__date1.setYear(__iystr0 - 1);
			__date1.setMonth(11);
			__date2.setYear(__iyend0);
			__date2.setMonth(10);
		}
		__date1.setDay(1);
		__date2.setDay(TimeUtil.numDaysInMonth(__date2));

		// Header length, used to position for data records...
		int offset = 0;
		if (StateMod_Util.isVersionAtLeast(__version, StateMod_Util.VERSION_11_00))
		{
			// Offset in addition to header in older format files...
			offset = 1 + __maxparm * 3 + 1; // Unit
		}
		int resTotalAccountRec = 1;
		__headerLength = __recordLength * (offset + 4 + __numsta + __numdiv + __numifr + (__numres + resTotalAccountRec) + __numrun + __numdivw); // Number of D&W nodes
		// Number of bytes for one interval (one month or one day) of data for all stations...
		if (__comp_type == StateMod_DataSet.COMP_DIVERSION_STATIONS)
		{
			__intervalBytes = __recordLength * __numsta;
		}
		else if (__comp_type == StateMod_DataSet.COMP_RESERVOIR_STATIONS)
		{
			// Include owner/account records in addition to the 1 record
			// for each reservoir for the total account.  Inactive
			// reservoirs have the accounts included at the end, but no
			// total (__nowner2_cum is used for the block size but
			// __nowner2_cum2 is used to locate specific time series).
			__intervalBytes = __recordLength * __nowner2_cum[__numres - 1];
		}
		else if (__comp_type == StateMod_DataSet.COMP_WELL_STATIONS)
		{
			__intervalBytes = __recordLength * __numdivw;
		}

		// Estimated file length...

		if (__intervalBase == TimeInterval.MONTH)
		{
			__estimatedFileLengthBytes = __headerLength + __intervalBytes * 12 * (__iyend0 - __iystr0 + 1);
		}
		else
		{
			// One set of parameters per station...
			__estimatedFileLengthBytes = __headerLength + __intervalBytes * 12 * 31 * (__iyend0 - __iystr0 + 1);
		}
		if (Message.isDebugOn)
		{
			if (__intervalBase == TimeInterval.MONTH)
			{
				Message.printDebug(1, routine, "Reading monthly data.");
			}
			else
			{
				Message.printDebug(1, routine, "Reading daily data.");
			}
			Message.printDebug(1, routine, "Length of 1 record (bytes) = " + __recordLength);
			Message.printDebug(1, routine, "Header length (bytes) = " + __headerLength);
			if (__comp_type == StateMod_DataSet.COMP_DIVERSION_STATIONS)
			{
				Message.printDebug(1, "", "Number of stations in data set = " + __numsta);
			}
			else if (__comp_type == StateMod_DataSet.COMP_RESERVOIR_STATIONS)
			{
				Message.printDebug(1, routine, "Number of reservoirs in data set = " + __numres);
				Message.printDebug(1, routine, "Total number of reservoir accounts (does not include " + "total accounts, does include inactive) = " + __numown);
			}
			else if (__comp_type == StateMod_DataSet.COMP_WELL_STATIONS)
			{
				Message.printDebug(1, routine, "Number of wells in data set = " + __numdivw);
			}
			if (__intervalBase == TimeInterval.MONTH)
			{
				Message.printDebug(1, routine, "Length of 1 complete month (bytes) = " + __intervalBytes);
			}
			else
			{
				// Daily
				Message.printDebug(1, routine, "Length of 1 complete day (bytes) = " + __intervalBytes);
			}
			Message.printDebug(1, routine, "Estimated file size (bytes) = " + __estimatedFileLengthBytes);
		}

		if (IOUtil.testing())
		{
			try
			{
				printRecords0();
				//printRecords ( 10 );
			}
			catch (Exception e)
			{
				Message.printWarning(3, routine, "Error printing records.");
				Message.printWarning(3, routine, e);
			}
		}
	}

	/// <summary>
	/// Look up the file pointer to use when opening a new file.  If the file is already
	/// open and is in the internal __file_HashTable, use it.  Otherwise, open the file
	/// and add it to the Hashtable.  The code to close the file must remove the file from the Hashtable. </summary>
	/// <param name="full_fname"> Full path to file to open. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static StateMod_BTS lookupStateModBTS(String full_fname) throws Exception
	private static StateMod_BTS lookupStateModBTS(string full_fname)
	{
		string routine = "StateMod_BTS.lookupStateModBTS";
		object o = __file_Hashtable[full_fname];
		if (o != null)
		{
			// Have a matching file pointer so assume that it can be used...
			Message.printStatus(2, routine, "Using existing binary file.");
			return (StateMod_BTS)o;
		}
		// Else create a new file...
		Message.printStatus(2, routine, "Opening new binary file.");
		StateMod_BTS bts = new StateMod_BTS(full_fname);
		// Add to the HashTable...
		__file_Hashtable[full_fname] = bts;
		return bts;
	}

	/// <summary>
	/// Test code to print records, brute force until data runs out. </summary>
	/// <param name="max_stations"> Indicate the maximum number of stations to print. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void printRecords0() throws Exception
	private void printRecords0()
	{
		string routine = "StateMod_BTS.printRecords0";
		StringBuilder b = new StringBuilder();
		double value = 0.0;
		int iparm;
		for (int irec = 0; irec >= 0; irec++)
		{
			try
			{
				__fp.seek(__headerLength + irec * __recordLength);
			}
			catch (Exception)
			{
				// End of data...
				break;
			}
			b.Length = 0;
			b.Append(StringUtil.formatString(irec,"%4d") + " ");
			for (iparm = 0; iparm < __numparm; iparm++)
			{
				try
				{
					value = __fp.readLittleEndianFloat();
				}
				catch (Exception)
				{
					// End of data...
					irec = -2;
					break;
				}
				b.Append(" " + StringUtil.formatString(value,"%#7.0f"));
			}
			Message.printStatus(2, routine, b.ToString());
		}
	}

	/// <summary>
	/// Test code to print records, trying to do so intelligently.  The output in the log file can be sorted and
	/// compared against the standard *.xdd, *.xre, etc. reports. </summary>
	/// <param name="max_stations"> Indicate the maximum number of stations to print. </param>
	/* TODO SAM Evaluate use
	private void printRecords ( int max_stations )
	throws Exception
	{
		__fp.seek ( __header_length );
		StringBuffer b = new StringBuffer();
		float value;
		int irec = 0;
		int iaccount = 0;
		int naccount = 1;	// For non-reservoirs...
		int iy, im, ista, iparm, year, month;
		boolean do_res = false;
		int numsta = __numsta;	// Streamflow, diversion, ISF
		if (	StringUtil.endsWithIgnoreCase(__tsfile,"b44") ||
			StringUtil.endsWithIgnoreCase(__tsfile,"b50") ) {
			do_res = true;
			max_stations = __numres;	// Show them all
			numsta = __numres;
		}
		boolean do_well = false;
		if (	StringUtil.endsWithIgnoreCase(__tsfile,"b42") ||
			StringUtil.endsWithIgnoreCase(__tsfile,"b65") ) {
			do_well = true;
			numsta = __numdivw;
		}
		boolean do_water = false;
		if ( __xmonam[0].equalsIgnoreCase("Oct") ) {
			do_water = true;
		}
		boolean do_month = true;
		if (	StringUtil.endsWithIgnoreCase(__tsfile,"b49") ||
			StringUtil.endsWithIgnoreCase(__tsfile,"b50") ||
			StringUtil.endsWithIgnoreCase(__tsfile,"b65") ) {
			do_month = false;
		}
		// Read the rest of the file and just print out the values...
		// For stream/diversion/ISF, loop through river station list.
		// For reservoirs and wells, loop through the specific lists.
		String [] id_array = __cstaid;
		for ( iy = __iystr0; iy <= __iyend0; iy++ ) {
			for ( im = 0; im < 12; im++ ) {
			for ( ista = 0; ista < numsta; ista++ ) {
				// TODO SAM 2005-12-22 need special handling of other nodes as well.
				if ( do_res ) {
					naccount = __nowner2[ista];
					id_array = __cresid;
				}
				else if ( do_well ) {
					id_array = __cdividw;
				}
				// Else in the diversion binary files, all river nodes are listed.
				for (	iaccount=0;
					iaccount<naccount;
					iaccount++, irec++ ){
					if ( ista < max_stations ) {
					// Read and print the data (otherwise just
					// rely on the record counter to increment in
					// the "for" statement)...
					__fp.seek ( __header_length +
							irec*__record_length );
					b.setLength(0);
					year = iy;
					month = im + 1;
					if ( do_water ) {
						if ( im < 3 ) {
							month = im + 10;
							year = iy - 1;
						}
						else {	month = im - 2;
						}
					}
					// List this way so that output can be sorted
					// by station and then time...
					b.append ( StringUtil.formatString(
						id_array[ista],"%-12.12s") + " " +
						StringUtil.formatString(iaccount,
						"%2d"));
					b.append ( " " + year + " " +
						StringUtil.formatString(month,"%02d")
						+ " " + __xmonam[im]);
					for (	iparm = 0; iparm < __numparm;
						iparm++ ) {
						value = __fp.readLittleEndianFloat();
						if ( do_month ) {
							// Convert CFS to ACFT
							value = value*
							CFS_TO_ACFT* (float)
							__mthday2[month - 1];
						}
						b.append ( " " +StringUtil.formatString(
						value,"%#7.0f") );
					}
					Message.printStatus ( 2, routine, b.toString() );
					}
				}
			}
			}
		}
	}
	*/

	/// <summary>
	/// Read the header from the opened binary file and save the information in
	/// memory for fast lookups.  The header is the same for all of the binary output files. </summary>
	/// <exception cref="IOException"> if there is an error reading from the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void readHeader() throws java.io.IOException
	private void readHeader()
	{
		string routine = "StateMod_BTS.readHeader";
		int dl = 1;

		int header_rec = 0;
		if (StateMod_Util.isVersionAtLeast(__version, StateMod_Util.VERSION_11_00))
		{
			// Need to skip the first record that has the file version information.
			// The period for the file is in record 2...
			header_rec = 1;
		}
		else
		{
			// Old format that has period for the file in record 1...
			header_rec = 0;
			// Below we refer to record 2 since this is the newest format.
		}
		__fp.seek(header_rec * __recordLength);

		// Record 2 - start and end year - check the months in record 3 to determine the year type...

		__iystr0 = __fp.readLittleEndianInt();
		__iyend0 = __fp.readLittleEndianInt();
		if (Message.isDebugOn)
		{
			Message.printDebug(dl,routine,"Reading binary file header...");
			Message.printDebug(dl, routine, "iystr0=" + __iystr0);
			Message.printDebug(dl, routine, "iyend0=" + __iyend0);
		}

		// Record 3 - numbers of various stations...

		__fp.seek((header_rec + 1) * __recordLength);
		__numsta = __fp.readLittleEndianInt();
		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "numsta=" + __numsta);
		}
		__numdiv = __fp.readLittleEndianInt();
		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "numdiv=" + __numdiv);
		}
		__numifr = __fp.readLittleEndianInt();
		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "numifr=" + __numifr);
		}
		__numres = __fp.readLittleEndianInt();
		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "numres=" + __numres);
		}
		__numown = __fp.readLittleEndianInt();
		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "numown=" + __numown);
		}
		__nrsact = __fp.readLittleEndianInt();
		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "nrsact=" + __nrsact);
		}
		__numrun = __fp.readLittleEndianInt();
		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "numrun=" + __numrun);
		}
		__numdivw = __fp.readLittleEndianInt();
		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "numdivw=" + __numdivw);
		}
		__numdxw = __fp.readLittleEndianInt();
		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "numdxw=" + __numdxw);
		}
		if (StateMod_Util.isVersionAtLeast(__version, StateMod_Util.VERSION_11_00))
		{
			__maxparm = __fp.readLittleEndianInt();
			if (Message.isDebugOn)
			{
				Message.printDebug(dl, routine, "maxparm=" + __maxparm);
			}
			__ndivO = __fp.readLittleEndianInt();
			if (Message.isDebugOn)
			{
				Message.printDebug(dl, routine, "ndivO=" + __ndivO);
			}
			__nresO = __fp.readLittleEndianInt();
			if (Message.isDebugOn)
			{
				Message.printDebug(dl, routine, "nresO=" + __nresO);
			}
			__nwelO = __fp.readLittleEndianInt();
			if (Message.isDebugOn)
			{
				Message.printDebug(dl, routine, "nwelO=" + __nwelO);
			}
		}

		// Record 4 - month names...

		__fp.seek((header_rec + 2) * __recordLength);
		__xmonam = new string[14];
		char[] xmonam = new char[3];
		int j = 0;
		for (int i = 0; i < 14; i++)
		{
			// The months are written as 4-character strings but only need the first 3...
			for (j = 0; j < 3; j++)
			{
				xmonam[j] = __fp.readLittleEndianChar1();
			}
			__fp.readLittleEndianChar1();
			__xmonam[i] = new string(xmonam);
			if (Message.isDebugOn)
			{
				Message.printDebug(dl, routine, "xmonam[" + i + "]=" + __xmonam[i]);
			}
		}

		// Record 5 - number of days per month

		__fp.seek((header_rec + 3) * __recordLength);
		__mthday = new int[12];
		for (int i = 0; i < 12; i++)
		{
			__mthday[i] = __fp.readLittleEndianInt();
			if (Message.isDebugOn)
			{
				Message.printDebug(dl, routine, "mthday[" + i + "]=" + __mthday[i]);
			}
		}
		// Create a new array that is always in calendar order.  Therefore,
		// __mthday2[0] always has the number of days for January.
		__mthdayCalendar = new int[12];
		if (__xmonam[0].Equals("OCT", StringComparison.OrdinalIgnoreCase))
		{
			// Water year...
			__mthdayCalendar[9] = __mthday[0]; // Oct...
			__mthdayCalendar[10] = __mthday[1];
			__mthdayCalendar[11] = __mthday[2];
			__mthdayCalendar[0] = __mthday[3]; // Jan...
			__mthdayCalendar[1] = __mthday[4];
			__mthdayCalendar[2] = __mthday[5];
			__mthdayCalendar[3] = __mthday[6];
			__mthdayCalendar[4] = __mthday[7];
			__mthdayCalendar[5] = __mthday[8];
			__mthdayCalendar[6] = __mthday[9];
			__mthdayCalendar[7] = __mthday[10];
			__mthdayCalendar[8] = __mthday[11];
		}
		else if (__xmonam[0].Equals("NOV", StringComparison.OrdinalIgnoreCase))
		{
			// Irrigation year...
			__mthdayCalendar[10] = __mthday[0]; // Nov...
			__mthdayCalendar[11] = __mthday[1];
			__mthdayCalendar[0] = __mthday[2]; // Jan...
			__mthdayCalendar[1] = __mthday[3];
			__mthdayCalendar[2] = __mthday[4];
			__mthdayCalendar[3] = __mthday[5];
			__mthdayCalendar[4] = __mthday[6];
			__mthdayCalendar[5] = __mthday[7];
			__mthdayCalendar[6] = __mthday[8];
			__mthdayCalendar[7] = __mthday[9];
			__mthdayCalendar[8] = __mthday[10];
			__mthdayCalendar[9] = __mthday[11];
		}
		else
		{
			// Calendar...
			__mthdayCalendar = __mthday;
		}

		// Record 6 - river stations...

		int offset2 = (header_rec + 4) * __recordLength;
		__cstaid = new string[__numsta];
		__stanam = new string[__numsta];
		int counter = 0;
		for (int i = 0; i < __numsta; i++)
		{
			__fp.seek(offset2 + i * __recordLength);
			// Counter...
			counter = __fp.readLittleEndianInt();
			// Identifier as 12 character string...
			__cstaid[i] = __fp.readLittleEndianString1(12).Trim();
			// Station name as 24 characters, written as 6 reals...
			__stanam[i] = __fp.readLittleEndianString1(24).Trim();
			if (Message.isDebugOn)
			{
				Message.printDebug(dl, routine, "" + counter + " Riv = \"" + __cstaid[i] + "\" \"" + __stanam[i] + "\"");
			}
		}

		// Read the station ID/name lists.  The station IDs are matched against
		// the requested TSID to find the position in the data array.

		// Record 7 - diversion stations...

		offset2 = (header_rec + 4 + __numsta) * __recordLength;
		if (__numdiv > 0)
		{
			__cdivid = new string[__numdiv];
			__divnam = new string[__numdiv];
			__idvsta = new int[__numdiv];
		}
		for (int i = 0; i < __numdiv; i++)
		{
			__fp.seek(offset2 + i * __recordLength);
			// Counter...
			counter = __fp.readLittleEndianInt();
			// Identifier as 12 character string...
			__cdivid[i] = __fp.readLittleEndianString1(12).Trim();
			// Station name as 24 characters, written as 6 reals...
			__divnam[i] = __fp.readLittleEndianString1(24).Trim();
			__idvsta[i] = __fp.readLittleEndianInt();
			if (Message.isDebugOn)
			{
				Message.printDebug(dl, routine, "" + counter + " Div = \"" + __cdivid[i] + "\" \"" + __divnam[i] + "\" idvsta = " + __idvsta[i]);
			}
		}

		// Record 8 - instream flow stations...

		offset2 = (header_rec + 4 + __numsta + __numdiv) * __recordLength;
		if (__numifr > 0)
		{
			__cifrid = new string[__numifr];
			__xfrnam = new string[__numifr];
			__ifrsta = new int[__numifr];
		}
		for (int i = 0; i < __numifr; i++)
		{
			__fp.seek(offset2 + i * __recordLength);
			// Counter...
			counter = __fp.readLittleEndianInt();
			// Identifier as 12 character string...
			__cifrid[i] = __fp.readLittleEndianString1(12).Trim();
			// Station name as 24 characters, written as 6 reals...
			__xfrnam[i] = __fp.readLittleEndianString1(24).Trim();
			__ifrsta[i] = __fp.readLittleEndianInt();
			if (Message.isDebugOn)
			{
				Message.printDebug(dl, routine, "" + counter + " Ifr = \"" + __cifrid[i] + "\" \"" + __xfrnam[i] + "\" ifrsta = " + __ifrsta[i]);
			}
		}

		// Record 9 - reservoir stations...

		offset2 = (header_rec + 4 + __numsta + __numdiv + __numifr) * __recordLength;
		// The value of __nowner is the record number (1+) of the first owner
		// for the current reservoir.  Therefore, the number of owners is
		// calculated for the current reservoir by taking the number of owners
		// in record (i + 1) minus the value in the current record.  Thefore the
		// last record is necessary to calculate the number of owners for the
		// last reservoir.  For example:
		//
		// __nowner[0] = 1
		// __nowner[1] = 5
		//
		// Indicates that the first reservoir has 5 - 1 = 4 accounts not
		// counting the total.  Therefore, for this example the accounts would
		// be:
		//
		// 0 - total
		// 1 - Account 1
		// 2 - Account 2
		// 3 - Account 3
		// 4 - Account 4
		//
		// Below, allocate the reservoir arrays one more than the actual number
		// of reservoirs to capture the extra data, but __numres reflects only
		// the actual reservoirs, for later processing.
		int iend = __numres + 1;
		if (iend > 0)
		{
			__cresid = new string[iend];
			__resnam = new string[iend];
			__irssta = new int[iend];
			__iressw = new int[iend];
			__nowner = new int[iend];
		}
		if (__numres > 0)
		{
			__nowner2 = new int[__numres];
			__nowner2_cum = new int[__numres];
			__nowner2_cum2 = new int[__numres];
		}
		for (int i = 0; i < iend; i++)
		{
			__fp.seek(offset2 + i * __recordLength);
			// Counter...
			counter = __fp.readLittleEndianInt();
			// Identifier as 12 character string...
			__cresid[i] = __fp.readLittleEndianString1(12).Trim();
			// Station name as 24 characters, written as 6 reals...
			__resnam[i] = __fp.readLittleEndianString1(24).Trim();
			__irssta[i] = __fp.readLittleEndianInt();
			__iressw[i] = __fp.readLittleEndianInt();
			__nowner[i] = __fp.readLittleEndianInt();
			if (Message.isDebugOn)
			{
				Message.printDebug(dl, routine, "" + counter + " Res = \"" + __cresid[i] + "\" \"" + __resnam[i] + "\" irssta = " + __irssta[i] + " iressw=" + __iressw[i] + " nowner = " + __nowner[i]);
			}
		}
		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "The last reservoir record is used to compute the number of accounts.");
			Message.printDebug(dl, routine, "In the following, nowner2 is active accounts, including total.");
			Message.printDebug(dl, routine, "nowner2_cum is cumulative, including the current reservoir.");
			Message.printDebug(dl, routine, "nowner2_cum2 is cumulative, including the current reservoir," + " with inactive reservoir accounts at the end.");
		}
		// Compute the actual number of accounts for each reservoir (with
		// the total).  Only active reservoirs are counted.
		for (int i = 0; i < __numres; i++)
		{
			// The total number of accounts for reservoirs (from StateMod
			// documentation) is Nrsactx = nrsact + numown
			// where the "nrsact" accounts for the "total" time series and
			// "numown" accounts for time series for each account, whether
			// active or not.  For each reservoir, the individual accounts
			// are included (whether the reservoir is active or not), but
			// the total account is only included if the reservoir is active
			// (see the "else" below).
			__nowner2[i] = __nowner[i + 1] - __nowner[i]; // Accounts but no total
			if (__iressw[i] != 0)
			{
				// Reservoir is active so add the total...
				__nowner2[i] += 1;
			}
			// Cumulative accounts (including totals), inclusive of the current reservoir station.
			if (i == 0)
			{
				// Initialize...
				__nowner2_cum[i] = __nowner2[i];
				if (__iressw[i] == 0)
				{
					// Position for reading is zero (thistime series will never be read but the array
					// is used to increment later elements)...
					__nowner2_cum2[i] = 0;
				}
				else
				{
					__nowner2_cum2[i] = __nowner2[i];
				}
			}
			else
			{
				// Add the current accounts to the previous cumulative value...
				__nowner2_cum[i] = __nowner2_cum[i - 1] + __nowner2[i];
				if (__iressw[i] == 0)
				{
					// Position for reading stays the same (this time series will never be read but the array
					// is used to increment later elements)...
					__nowner2_cum2[i] = __nowner2_cum2[i - 1];
				}
				else
				{
					// Increment the counter...
					__nowner2_cum2[i] = __nowner2_cum2[i - 1] + __nowner2[i];
				}
			}
			Message.printDebug(dl, routine, " Res = \"" + __cresid[i] + "\" \"" + __resnam[i] + "\" nowner2 = " + __nowner2[i] + " nowner2_cum = " + __nowner2_cum[i] + " nowner2_cum2 = " + __nowner2_cum2[i]);
		}

		// A single record after reservoirs contains cumulative account
		// information for reservoirs.  Just ignore the record and add a +1
		// below when computing the position...

		// Record 10 - base flow stations...

		int resTotalAccountRec = 1;
		offset2 = (header_rec + 4 + __numsta + __numdiv + __numifr + __numres + resTotalAccountRec) * __recordLength;
		if (__numrun > 0)
		{
			__crunid = new string[__numrun];
			__runnam = new string[__numrun];
			__irusta = new int[__numrun];
		}
		for (int i = 0; i < __numrun; i++)
		{
			__fp.seek(offset2 + i * __recordLength);
			// Counter...
			counter = __fp.readLittleEndianInt();
			// Identifier as 12 character string...
			__crunid[i] = __fp.readLittleEndianString1(12).Trim();
			// Station name as 24 characters, written as 6 reals...
			__runnam[i] = __fp.readLittleEndianString1(24).Trim();
			__irusta[i] = __fp.readLittleEndianInt();
			if (Message.isDebugOn)
			{
				Message.printDebug(dl, routine, "" + counter + " Baseflow = \"" + __crunid[i] + "\" \"" + __runnam[i] + "\" irusta = " + __irusta[i]);
			}
		}

		// Record 11 - well stations...

		offset2 = (header_rec + 4 + __numsta + __numdiv + __numifr + __numres + resTotalAccountRec + __numrun) * __recordLength;
		if (__numdivw > 0)
		{
			__cdividw = new string[__numdivw];
			__divnamw = new string[__numdivw];
			__idvstw = new int[__numdivw];
		}
		for (int i = 0; i < __numdivw; i++)
		{
			__fp.seek(offset2 + i * __recordLength);
			// Counter...
			counter = __fp.readLittleEndianInt();
			// Identifier as 12 character string...
			__cdividw[i] = __fp.readLittleEndianString1(12).Trim();
			// Station name as 24 characters, written as 6 reals...
			__divnamw[i] = __fp.readLittleEndianString1(24).Trim();
			__idvstw[i] = __fp.readLittleEndianInt();
			if (Message.isDebugOn)
			{
				Message.printDebug(dl, routine, "" + counter + " Well = \"" + __cdividw[i] + "\" \"" + __divnamw[i] + "\" idvstw = " + __idvstw[i]);
			}
		}

		// Get the parameters that are expected in the file.  Note that this is
		// the full list in the file, not the list that may be appropriate for
		// the station type.  It is assumed that code that the calling code is
		// requesting an appropriate parameter.  For example, the StateMod GUI
		// graphing tool should have already filtered the parameters based on station type.

		if (StateMod_Util.isVersionAtLeast(__version, StateMod_Util.VERSION_11_00))
		{
			if (Message.isDebugOn)
			{
				Message.printDebug(dl, routine, "Reading parameters from file");
			}
			// Read the parameter lists and units from the file.  The
			// parameters that are available are the same for streamflow, diversions, and wells...
			string[] parameters = null; // Temporary list - will only save the one that is needed for this file
			// TODO SAM 2005-12-23
			// For now read through but later can just read the set of
			// parameters that are actually needed for this file.
			for (int ip = 0; ip < 3; ip++)
			{
				// Parameter lists for div, res, well binary files.
				offset2 = (header_rec + 4 + __numsta + __numdiv + __numifr + (__numres + resTotalAccountRec) + __numrun + __numdivw + ip * __maxparm) * __recordLength;
				if (__maxparm > 0)
				{
					// Reallocate each time so as to not step on the saved array below...
					parameters = new string[__maxparm];
				}
				for (int i = 0; i < __maxparm; i++)
				{
					// 4 is to skip the counter at the beginning of the line...
					__fp.seek(offset2 + i * __recordLength + 4);
					parameters[i] = __fp.readLittleEndianString1(24).Trim();
					//* Use during development...
					if (Message.isDebugOn)
					{
						Message.printDebug(dl, routine, "Parameter from file...ip=" + ip + " Parameter[" + i + "] = \"" + parameters[i] + "\"");
					}
					//*/
				}
				// Because __maxparm only lists the maximum, reduce the list by decrementing the count if "NA" is at the
				// end.  Also save the information if for the proper file.
				if ((ip == 0) && (__comp_type == StateMod_DataSet.COMP_DIVERSION_STATIONS))
				{
					__parameters = parameters;
					__numparm = __ndivO;
					Message.printStatus(2, routine, "Saving diversion parameters list.");
				}
				else if ((ip == 1) && (__comp_type == StateMod_DataSet.COMP_RESERVOIR_STATIONS))
				{
					__parameters = parameters;
					__numparm = __nresO;
					Message.printStatus(2, routine, "Saving reservoir parameters list.");
				}
				else if ((ip == 2) && (__comp_type == StateMod_DataSet.COMP_WELL_STATIONS))
				{
					__parameters = parameters;
					__numparm = __nwelO;
					Message.printStatus(2, routine, "Saving well parameters list.");
				}
			}
		}
		else
		{
			// Get the parameter list from hard-coded lists...
			Message.printDebug(dl, routine, "Getting parameters from hard-coded lists for older file version.");
			__parameters = StringUtil.toArray(StateMod_Util.getTimeSeriesDataTypes(__comp_type, null, null, __version, __intervalBase, false, false, true, false, false, false)); // No note - not needed here
				__numparm = __parameters.Length;
		}

		// Parameter names...

		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "Parameters for the file (size is " + __numparm + "):");
			for (int i = 0; i < __numparm; i++)
			{
				Message.printDebug(dl, routine, "Parameter[" + i + "] = \"" + __parameters[i] + "\"");
			}
		}

		if (StateMod_Util.isVersionAtLeast(__version, StateMod_Util.VERSION_11_00))
		{
			offset2 = (header_rec + 4 + __numsta + __numdiv + __numifr + (__numres + resTotalAccountRec) + __numrun + __numdivw + __maxparm * 3) * __recordLength;
			__fp.seek(offset2);
			if (__numparm > 0)
			{
				__unit = new string[__numparm];
			}
			for (int i = 0; i < __numparm; i++)
			{
				__unit[i] = __fp.readLittleEndianString1(4).Trim();
				if (Message.isDebugOn)
				{
					Message.printDebug(dl, routine, "For parameter \"" + __parameters[i] + "\" unit= \"" + __unit[i] + "\"");
				}
			}
		}
	}

	/// <summary>
	/// For an open binary file, determine the StateMod version from the first record.
	/// If the record does not contain the version (e.g., old file format), then try
	/// to determine the version from the current StateMod executable that is available. </summary>
	/// <exception cref="IOException"> if an error occurs reading the header (usually due to an empty file). </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void readHeaderVersion() throws java.io.IOException
	private void readHeaderVersion()
	{
		string routine = "StateMod_BTS.readHeaderVersion";
		int dl = 1;
		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "The file version before reading is \"" + __version + "\"");
		}
		bool pre11Format = false; // Indicates old format files
		bool newestFormat = false; // Indicates new format files
		// Files >= 12.29 use the following (from Ray Bennett 2008-10-27):
		// write(nf,rec=j1) CodeName, ver, vdate 
		// where codename is character*8, 
		//    ver is character *8, and 
		//    vdate is character *10
		// Therefore, check position [10] for a '.' which indicates the newer header format.
		__fp.seek(10);
		char test_char = __fp.readLittleEndianChar1();
		if (test_char == '.')
		{
			// Have the latest header format
			newestFormat = true;
		}
		else
		{
			// Files before version 11 have year start and year end (2 integers)
			// in the first record.  Therefore, check the characters used for the
			// date and see if at least two are non-null.  If non-null, assume the
			// new 11+ format.  If null, assume the old format.
			__fp.seek(16); // First '/' in date if YYYY/MM/DD
			test_char = __fp.readLittleEndianChar1();
			if (test_char == '\0')
			{
				pre11Format = true;
				__fp.seek(19); // Second '/' in date if YYYY/MM/DD
				test_char = __fp.readLittleEndianChar1();
				if (test_char == '\0')
				{
					pre11Format = true;
				}
			} // Else both characters were non-null so pretty sure it is 11+
		}
		if (newestFormat)
		{
			Message.printStatus(2, routine, "The file version detected from header is >= 12.29.");
		}
		else if (pre11Format)
		{
			Message.printStatus(2, routine, "The file version detected from header is < 11.x.");
		}
		else
		{
			Message.printStatus(2, routine, "The file version detected from header is >= 11.x. and < 12.29");
		}
		if (pre11Format)
		{
			// No version can be determined from the file...
			__version = "";
		}
		else
		{
			// Version 11+ format
			// Reposition and read the header...
			__fp.seek(0);
			// 8 characters for the program name...
			__headerProgram = __fp.readLittleEndianString1(8).Trim();
			// Program version...
			if (newestFormat)
			{
				// NN.NN.NN but OK to have remainder
				__version = __fp.readLittleEndianString1(8).Trim();
			}
			else
			{
				// Version 11+ to < 12.29 used NN.NN format as float
				double versionDouble = (double)__fp.readLittleEndianFloat();
				__version = "" + StringUtil.formatString(versionDouble,"%.2f"); // Format to avoid remainder
			}
			// Date as 10 characters...
			__headerDate = __fp.readLittleEndianString1(10).Trim();
			if (Message.isDebugOn)
			{
				Message.printDebug(dl, routine, "Creator program=\"" + __headerProgram + "\" file version=" + __version + " software date=\"" + __headerDate + "\"");
			}
		}

		// Set the version information so that it can be used elsewhere in the
		// class (for example to determine the parameters)...

		if (__version.Equals(""))
		{
			// If pre 11.0, then assume that it is the one before 11.0...
			__version = "9.69";
			Message.printStatus(2, routine, "Appears to be old file format - assuming that the StateMod file version is " + __version);
		}
		else
		{
			Message.printStatus(2, routine, "Read StateMod file version from header: " + __version);
		}
	}

	/// <summary>
	/// Read a time series from a StateMod binary file.  The TSID string is specified
	/// in addition to the path to the file.  It is expected that a TSID in the file
	/// matches the TSID (and the path to the file, if included in the TSID would not
	/// properly allow the TSID to be specified).  This method can be used with newer
	/// code where the I/O path is separate from the TSID that is used to identify the time series.
	/// The IOUtil.getPathUsingWorkingDir() method is applied to the filename.
	/// When a pattern is supplied, the duplicate time series are ignored (only the
	/// first occurance is kept).  This most often filters out baseflow nodes at
	/// diversions, etc.  Returning one instance ensures that the time series will not
	/// be double-counted in lists and subsequent analysis. </summary>
	/// <returns> a pointer to a newly-allocated time series if successful, a NULL pointer if not. </returns>
	/// <param name="tsident_string"> The full identifier for the time series to
	/// read (where the scenario is NOT the file name). </param>
	/// <param name="filename"> The name of a file to read
	/// (in which case the tsident_string must match one of the TSID strings in the file). </param>
	/// <param name="date1"> Starting date to initialize period (NULL to read the entire time series). </param>
	/// <param name="date2"> Ending date to initialize period (NULL to read the entire time series). </param>
	/// <param name="units"> Units to convert to (currently not supported). </param>
	/// <param name="read_data"> Indicates whether data should be read (false=no, true=yes). </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static RTi.TS.TS readTimeSeries(String tsident_string, String filename, RTi.Util.Time.DateTime date1, RTi.Util.Time.DateTime date2, String units, boolean read_data) throws Exception
	public static TS readTimeSeries(string tsident_string, string filename, DateTime date1, DateTime date2, string units, bool read_data)
	{
		return readTimeSeries(tsident_string, filename, date1, date2, units, read_data, (PropList)null);
	}

	/// <summary>
	/// Read a time series from a StateMod binary file.  The TSID string is specified
	/// in addition to the path to the file.  It is expected that a TSID in the file
	/// matches the TSID (and the path to the file, if included in the TSID would not
	/// properly allow the TSID to be specified).  This method can be used with newer
	/// code where the I/O path is separate from the TSID that is used to identify the time series.
	/// The IOUtil.getPathUsingWorkingDir() method is applied to the filename.
	/// When a pattern is supplied, the duplicate time series are ignored (only the
	/// first occurance is kept).  This most often filters out baseflow nodes at
	/// diversions, etc.  Returning one instance ensures that the time series will not
	/// be double-counted in lists and subsequent analysis. </summary>
	/// <returns> a pointer to a newly-allocated time series if successful, a NULL pointer if not. </returns>
	/// <param name="tsident_string"> The full identifier for the time series to
	/// read (where the scenario is NOT the file name). </param>
	/// <param name="filename"> The name of a file to read
	/// (in which case the tsident_string must match one of the TSID strings in the file). </param>
	/// <param name="date1"> Starting date to initialize period (NULL to read the entire time series). </param>
	/// <param name="date2"> Ending date to initialize period (NULL to read the entire time series). </param>
	/// <param name="units"> Units to convert to (currently not supported). </param>
	/// <param name="read_data"> Indicates whether data should be read (false=no, true=yes). </param>
	/// <param name="props"> A PropList containing information to control the read.  Recognized properties include:
	/// <table width=100% cellpadding=10 cellspacing=0 border=2>
	/// 
	/// <tr>
	/// <td><b>Property</b></td>   <td><b>Description</b></td>   <td><b>Default</b></td>
	/// </tr>
	/// <tr><td>CloseWhenDone</td>
	/// <td>Specifies whether to close the file once the time series has been read from it (False or True).</td>
	/// <td>False.</td>
	/// </tr>
	/// </table> </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static RTi.TS.TS readTimeSeries(String tsident_string, String filename, RTi.Util.Time.DateTime date1, RTi.Util.Time.DateTime date2, String units, boolean read_data, RTi.Util.IO.PropList props) throws Exception
	public static TS readTimeSeries(string tsident_string, string filename, DateTime date1, DateTime date2, string units, bool read_data, PropList props)
	{
		TS ts = null;
		string full_fname = IOUtil.getPathUsingWorkingDir(filename);

		bool closeFile = false;

		if (props != null)
		{
			string closeWhenTrue = props.getValue("CloseWhenDone");
			if ((!string.ReferenceEquals(closeWhenTrue, null)) && closeWhenTrue.Trim().Equals("true", StringComparison.OrdinalIgnoreCase))
			{
				closeFile = true;
			}
		}

		if (!IOUtil.fileReadable(full_fname))
		{
			Message.printWarning(2, "StateMod_BTS.readTimeSeries", "Unable to determine file for \"" + filename + "\"");
			return ts;
		}
		StateMod_BTS @in = null;
		try
		{
			@in = lookupStateModBTS(full_fname);
		}
		catch (Exception)
		{
			Message.printWarning(2, "StateMod_BTS.readTimeSeries(String,...)", "Unable to open file \"" + full_fname + "\"");
			return ts;
		}
		// Call the fully-loaded method...
		// Pass the file pointer and an empty time series, which
		// will be used to locate the time series in the file.
		IList<TS> tslist = @in.readTimeSeriesList(tsident_string, date1, date2, units, read_data);

		if (closeFile)
		{
			@in.close();
		}

		if ((tslist == null) || (tslist.Count <= 0))
		{
			Message.printWarning(2, "StateMod_BTS.readTimeSeries(String,...)", "Unable to read time series for \"" + tsident_string + "\"");
			return ts;
		}
		return (TS)tslist[0];
	}

	/// <summary>
	/// Read a list of time series from the binary file.  A Vector of new time series is returned. </summary>
	/// <param name="tsident_pattern"> A regular expression for TSIdents to return.  For example
	/// or null returns all time series.  *.*.XXX.* returns only time series matching
	/// data type XXX.  Currently only location and data type (output parameter) are checked and only a
	/// wildcard can be specified, if used.  This is useful for TSTool in order to
	/// list all stations that have a data type.  For reservoirs, only the main location
	/// can be matched, and the returned list of time series will include time series
	/// for all accounts.  When matching a specific time series (no wildcards), the
	/// main location part is first matched and the the reservoir account is checked if the main location is matched. </param>
	/// <param name="date1"> First date/time to read, or null to read the full period. </param>
	/// <param name="date2"> Last date/time to read, or null to read the full period. </param>
	/// <param name="req_units"> Requested units for the time series (currently not implemented). </param>
	/// <param name="read_data"> True if all data should be read or false to only read the headers. </param>
	/// <exception cref="IOException"> if the interval for the time series does not match that
	/// for the file or if a write error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public java.util.List<RTi.TS.TS> readTimeSeriesList(String tsident_pattern, RTi.Util.Time.DateTime date1, RTi.Util.Time.DateTime date2, String req_units, boolean read_data) throws Exception
	public virtual IList<TS> readTimeSeriesList(string tsident_pattern, DateTime date1, DateTime date2, string req_units, bool read_data)
	{
		string routine = "StateMod_BTS.readTimeSeriesList";
		// Using previously read information, loop through each time series
		// identifier and see if it matches what we are searching for...

		// TODO (JTS - 2004-08-04)
		// there is no non-static readTimeSeries() method.  One should be added for efficiency's sake.
		// SAM 2006-01-04 - non-static is used because there is a penalty
		// reading the header.  This needs to be considered.

		int iparam = 0;
		IList<TS> tslist = new List<TS>();
		if ((string.ReferenceEquals(tsident_pattern, null)) || (tsident_pattern.Length == 0))
		{
			tsident_pattern = "*.*.*.*.*";
		}
		TSIdent tsident_regexp = new TSIdent(tsident_pattern);
						// TSIdent containing the regular expression parts.
		// Make sure that parts have wildcards if not specified...
		if (tsident_regexp.getLocation().length() == 0)
		{
			tsident_regexp.setLocation("*");
		}
		if (tsident_regexp.getSource().length() == 0)
		{
			tsident_regexp.setSource("*");
		}
		if (tsident_regexp.getType().length() == 0)
		{
			tsident_regexp.setType("*");
		}
		if (tsident_regexp.getInterval().length() == 0)
		{
			tsident_regexp.setInterval("*");
		}
		if (tsident_regexp.getScenario().length() == 0)
		{
			tsident_regexp.setScenario("*");
		}
		// These fields really have no bearing on the filter, but if not wildcarded, may cause a match to not be found...
		tsident_regexp.setSource("*");
		tsident_regexp.setInterval("*");
		tsident_regexp.setScenario("*");
		string tsident_regexp_loc = tsident_regexp.getLocation();
		string tsident_regexp_source = tsident_regexp.getSource();
		string tsident_regexp_type = tsident_regexp.getType();
		string tsident_regexp_interval = tsident_regexp.getInterval();
		string tsident_regexp_scenario = tsident_regexp.getScenario();
		bool station_has_wildcard = false; // Use to speed up
		bool datatype_has_wildcard = false; // loops.
		TS ts = null;
		float param;
		long filepos;
		DateTime date;
		int dl = 1;
		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "Reading time series for \"" + tsident_pattern + "\" __numsta = " + __numsta);
		}
		// This is used to track matches to ensure that the same
		// station/datatype combination is only included once.  It is possible
		// that baseflow stations are listed in more than one list of stations.
		// Even if there are 1000 nodes and 30 data types, this will only take
		// 30K of memory, which is relatively small.
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: bool[][] sta_matched = new bool[__numsta][__numparm];
		bool[][] sta_matched = RectangularArrays.RectangularBoolArray(__numsta, __numparm);
		int j;
		for (int i = 0; i < __numsta; i++)
		{
			for (j = 0; j < __numparm; j++)
			{
				sta_matched[i][j] = false;
			}
		}
		try
		{
		if (tsident_regexp_loc.IndexOf("*", StringComparison.Ordinal) >= 0)
		{
			station_has_wildcard = true;
		}
		if (tsident_regexp_type.IndexOf("*", StringComparison.Ordinal) >= 0)
		{
			datatype_has_wildcard = true;
		}
		// If the location contains a dash ("-"), it indicates a reservoir and
		// account.  Since the StateMod binary file does not merge these fields,
		// replace the identifier with only the main identifier...
		string req_account = null;
		if ((tsident_regexp_loc.IndexOf("-", StringComparison.Ordinal) >= 0) && (__comp_type == StateMod_DataSet.COMP_RESERVOIR_STATIONS))
		{
			req_account = StringUtil.getToken(tsident_regexp_loc,"-",0,1);
			tsident_regexp.setLocation(StringUtil.getToken(tsident_regexp_loc,"-",0,0));
			tsident_regexp_loc = tsident_regexp.getLocation();
		}
		bool match_found = false;
					// Indicates if a match for the specific station is made.
					// TODO SAM 2006-01-04.  This seems to be a remnant of previous code.  It is used but
					// many checks result in "continue" or "break" out of the loops.
		string[] ids = null; // Used to point to each list of station IDs.
		string[] names = null; // Used to point to each list of names.
		int numids = 0; // Used to point to size of each list.
		int ista = 0; // River node position matching the ID.
		int ista2 = 0; // Station in data record portion of file to
					// locate.  For diversion/instream/stream it is
					// ista.  For reservoirs and wells it is the
					// position of the reservoir or well in the file.
		int nts = 0; // Number of time series per location/parameter
					// combination (used with reservoir accounts).
		int its = 0; // Loop counter for time series.
		string owner = ""; // Used to append a reservoir owner/account to the location, blank normally.
		bool convert_cfs_to_acft = true;
					// Indicates whether a parameter's data should be converted from CFS to ACFT.
		// Loop through the lists of stations:
		//
		int DIV = 0; // Diversion stations
		int ISF = 1; // Instream flow stations
		int RES = 2; // Reservoir stations
		int BF = 3; // Baseflow stations
		int WEL = 4; // Wells
		int RIV = 5; // River nodes (to find nodes only in RIN file)
		//
		// The original file that was opened indicated what
		// type of data the file contains and inappropriate types are skipped
		// below.  Diversions, instream flow, and stream stations are stored
		// in the same binary file so multiple lists are checked for that file.
		TSIdent current_tsident = new TSIdent();
					// Used for the time series below, to compare to the pattern.
		TSIdent tsident = null; // Used when creating new time series.

		for (int istatype = 0; istatype < 6; istatype++)
		{
			// First check to see if we even need to search the station list for this loop index...
			// Diversion stations file has div, isf, baseflow, other (river
			// nodes that are not another station type)
			if ((__comp_type == StateMod_DataSet.COMP_DIVERSION_STATIONS) && (istatype != DIV) && (istatype != ISF) && (istatype != BF) && (istatype != RIV))
			{
				continue;
			}
			else if ((__comp_type == StateMod_DataSet.COMP_RESERVOIR_STATIONS) && (istatype != RES))
			{
				continue;
			}
			else if ((__comp_type == StateMod_DataSet.COMP_WELL_STATIONS) && (istatype != WEL))
			{
				continue;
			}
			// If here then the correct station list type has been
			// determined.  Assign the references to the arrays to search...
			if (istatype == DIV)
			{
				// Search diversions...
				ids = __cdivid;
				names = __divnam;
				numids = __numdiv;
			}
			else if (istatype == ISF)
			{
				// Search instream flows...
				ids = __cifrid;
				names = __xfrnam;
				numids = __numifr;
			}
			else if (istatype == RES)
			{
				// Search reservoirs...
				ids = __cresid;
				names = __resnam;
				numids = __numres;
			}
			else if (istatype == BF)
			{
				// Baseflows (stream gage + stream estimate)...
				ids = __crunid;
				names = __runnam;
				numids = __numrun;
			}
			else if (istatype == WEL)
			{
				// Wells...
				ids = __cdividw;
				names = __divnamw;
				numids = __numdivw;
			}
			else if (istatype == RIV)
			{
				// River nodes (nodes not other station will be found)...
				ids = __cstaid;
				names = __stanam;
				numids = __numsta;
			}
			// Loop through the ids in the list...
			for (int iid = 0; iid < numids; iid++)
			{
				if (Message.isDebugOn)
				{
					Message.printDebug(dl, routine, "Station[" + iid + "] = " + ids[iid]);
				}
				// Loop through the parameters...
				for (iparam = 0; iparam < __numparm; iparam++)
				{
					// Check the station and parameter to see if
					// they match - all other fields are allowed to be wildcarded...
					if (Message.isDebugOn)
					{
						Message.printDebug(2, routine, "Parameter = " + __parameters[iparam]);
					}
					// Need to match against each station ID list.
					// Set the information from the file into the working "tsident" to compare...
					current_tsident.setLocation(ids[iid]);
					current_tsident.setType(__parameters[iparam]);
					// Other TSID fields are left blank to match all.
					if (!current_tsident.matches(tsident_regexp_loc, tsident_regexp_source, tsident_regexp_type, tsident_regexp_interval, tsident_regexp_scenario, null, null, false))
					{
						// This time series does not match one that is requested.  Just need to
						// match the location and parameter since that is all that is in the file.
						//Message.printStatus ( 1, routine,"Requested \"" + tsident_pattern +
						//"\" does not match \"" +ids[iid] + "\" \""+__parameter[iparam]+ "\"" );
						continue;
					}
					if (Message.isDebugOn)
					{
						Message.printDebug(2, routine, "Requested \"" + tsident_pattern + "\" does match \"" + ids[iid] + "\" \"" + __parameters[iparam] + "\"");
					}
					// Figure out the river station from the match.  The original river node positions
					// start at 1 so subtract 1 to get the in-memory positions.  The river node id for stations is
					// only available in StateMod 10.34 or later. If the following results in a value of ista
					// <= 0, then try to match the river node ID directly - this will work with data sets
					// where the station and river node IDs are the same.
					ista = -1; // To allow check below.
					if (istatype == DIV)
					{
						// Diversions...
						ista = __idvsta[iid] - 1;
						ista2 = ista;
						match_found = true;
					}
					else if (istatype == ISF)
					{
						// Instream flow...
						ista = __ifrsta[iid] - 1;
						ista2 = ista;
						match_found = true;
					}
					else if (istatype == RES)
					{
						// Reservoirs...
						ista = __irssta[iid] - 1;
						ista2 = iid;
						match_found = true;
						// Ignore inactive reservoirs because data will be junk...
						if (__iressw[iid] == 0)
						{
							// Reservoir is not active so do not include the time
							// series.  The values will be meaningless (zero) and
							// inactive reservoirs do not have a total, which would
							// require special handling below.
							match_found = false;
						}
						else
						{
							// Make sure that the matched reservoir has the requested account...
							if (!string.ReferenceEquals(req_account, null))
							{
								// Have an account (not just the total)...
								int naccounts = __nowner2[iid] - 1; // Ignore total
								int ireq_account = StringUtil.atoi(req_account);
								if ((ireq_account < 1) || (ireq_account > naccounts))
								{
									match_found = false;
								}
							}
						}
					}
					else if (istatype == BF)
					{
						// Baseflows (stream gage and estimate)...
						ista = __irusta[iid] - 1;
						ista2 = ista;
						match_found = true;
					}
					else if (istatype == WEL)
					{
						// Wells...
						ista = __idvstw[iid] - 1;
						ista2 = iid;
						match_found = true;
					}
					else if (istatype == RIV)
					{
						// Already a river node...
						ista = iid;
						ista2 = iid;
						match_found = true;
					}
					if (match_found && sta_matched[ista][iparam])
					{
						// Already matched this station for the current parameter so ignore.  It is
						// possible that a baseflow node is also another node type but we only want
						// one instance of the time series. This will also ensure that river
						// nodes are not counted twice.
						match_found = false;
					}
					if (match_found)
					{ // Don't just continue because a check to break occurs below.
					sta_matched[ista][iparam] = true;
					convert_cfs_to_acft = true;
					if (__intervalBase == TimeInterval.DAY)
					{
						convert_cfs_to_acft = false;
					}
					if (istatype == RES)
					{
						// For each reservoir, have a total and a time series for each account.
						if (station_has_wildcard)
						{
							// Requesting all available time series...
							nts = __nowner2[iid];
						}
						else
						{
							// Requesting a single total or account...
							nts = 1;
						}
					}
					else
					{
						// Other than reservoirs, only one time series per location/parameter...
						nts = 1;
					}
					for (its = 0; its < nts; its++)
					{
						if ((istatype == RES) && (its > 0))
						{
							// Getting all reservoir time series for the accounts
							// - an owner account is added as a sublocation (1, 2, etc.)...
							owner = "-" + its;
						}
						else if ((istatype == RES) && (!string.ReferenceEquals(req_account, null)))
						{
							// A requested reservoir account...
							owner = "-" + req_account;
							// Set "its" to the specific account.  This will be OK
							// because it will cause the loop to exit when done with
							// the single time series.  Total is 0, so no need to offset...
							its = StringUtil.atoi(req_account);
						}
						else
						{
							// A reservoir total (no sub-location) or other station time series...
							owner = "";
						}
						if (__intervalBase == TimeInterval.MONTH)
						{
							ts = new MonthTS();
							tsident = new TSIdent(ids[iid] + owner, "StateMod", __parameters[iparam], "Month","", "StateModB", __tsfile);
						}
						else if (__intervalBase == TimeInterval.DAY)
						{
							ts = new DayTS();
							tsident = new TSIdent(ids[iid] + owner, "StateMod", __parameters[iparam], "Day","", "StateModB", __tsfile);
						}
						// Set time series header information...
						ts.setIdentifier(tsident);
						ts.setInputName(__tsfile);
						if ((istatype == RES) && owner.Length > 0)
						{
							// Put the owner in the description...
							ts.setDescription(names[iid] + " - Account " + owner.Substring(1));
						}
						else
						{
							ts.setDescription(names[iid]);
						}
						ts.setDataType(__parameters[iparam]);
						// Data in file are CFS but we convert to ACFT
						if (convert_cfs_to_acft)
						{
							ts.setDataUnits("ACFT");
						}
						else
						{
							ts.setDataUnits("CFS");
						}
						// Original dates from file header...
						ts.setDate1Original(new DateTime(__date1));
						ts.setDate2Original(new DateTime(__date1));
						// Time series dates from requested parameters or file...
						if (date1 == null)
						{
							date1 = new DateTime(__date1);
							ts.setDate1(date1);
						}
						else
						{
							ts.setDate1(new DateTime(date1));
						}
						if (date2 == null)
						{
							date2 = new DateTime(__date2);
							ts.setDate2(date2);
						}
						else
						{
							ts.setDate2(new DateTime(date2));
						}
						ts.addToGenesis("Read from \"" + __tsfile + " for " + date1 + " to " + date2);
						tslist.Add(ts);
						if (read_data)
						{
							if (Message.isDebugOn)
							{
								Message.printDebug(2, routine, "Reading " + date1 + " to " + date2);
							}
							// Allocate the data space...
							if (ts.allocateDataSpace() != 0)
							{
								throw new Exception("Unable to allocate data space.");
							}
							// Read the data for the time series...
							for (date = new DateTime(date1); date.lessThanOrEqualTo(date2); date.addInterval(__intervalBase, 1))
							{
								if ((date.getMonth() == 2) && (date.getDay() == 29))
								{
									// StateMod does not handle.
									continue;
								}
								filepos = calculateFilePosition(date, ista2,its,iparam);
								if (Message.isDebugOn)
								{
									Message.printDebug(2, routine, "Reading for " + date + " ista2=" + ista2 + " iparam=" + iparam + " its=" + its + " filepos=" + filepos);
								}
								if (filepos < 0)
								{
									continue;
								}
								__fp.seek(filepos);
								// Convert CFS to ACFT so output is monthly volume...
								try
								{
									param = __fp.readLittleEndianFloat();
								}
								catch (Exception e)
								{
									// Assume end of file so break out of read.
									Message.printWarning(3, routine, "Unexpected error reading byte " + filepos + " - stop reading data.  Expected file size =" + __estimatedFileLengthBytes);
									if (Message.isDebugOn)
									{
										Message.printWarning(3, routine, e);
									}
									break;
								}
								// Convert to ACFT if necessary...
								if (convert_cfs_to_acft)
								{
									param = param * CFS_TO_ACFT * (float)__mthdayCalendar[date.getMonth() - 1];
								}
								if (Message.isDebugOn)
								{
									Message.printDebug(2, routine, "Parameter value (AF) is " + param);
								}
								ts.setDataValue(date,param);
							}
						}
					}
					} // End match_found
					if (!datatype_has_wildcard)
					{
						// No need to keep searching...
						break;
					}
				}
				if (!datatype_has_wildcard && !station_has_wildcard && match_found)
				{
					// No need to keep searching...
					break;
				}
			}
		}
		}
		catch (Exception e)
		{
			Message.printWarning(3, "", e);
		}
		// TODO 2007-01-18 old comment - Might return null if match_found == false????
		return tslist;
	}

	/// <summary>
	/// Return the number of time series in the file. </summary>
	/// <returns> the number of time series in the file. </returns>
	public virtual int size()
	{
		return __numsta * __numparm;
	}

	} // End StateMod_BTS

}