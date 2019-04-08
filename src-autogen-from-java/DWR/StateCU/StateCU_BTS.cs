using System;
using System.Collections.Generic;

// StateCU_BTS - provide an interface to a StateCU binary files

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

	/// <summary>
	/// Provide an interface to a StateCU binary files.  Binary data are organized as
	/// follows:
	/// <pre>
	/// Station Type     Monthly File     Daily File
	/// --------------------------------------------
	/// CU location      *.b1             *.b??
	/// ---------------------------------------------------------------------------
	/// </pre>
	/// The file and format is determined based on the file extension and/or the
	/// requested parameter and data interval.  Typically the readTimeSeriesList() methods
	/// is used, which open a file, read one or more time series, and close the file.
	/// All the methods in this class that use time series index numbers use 0 for the
	/// first time series.
	/// The format of the file is described in StateCU documentation.  Relevant
	/// notes:
	/// <ol>
	/// <li>	The structure list is used to look up a station identifier.</li>
	/// <li>	The time series are written in one month blocks, with time series
	/// within the block listed in the order of the structures.</li>
	/// </ol>
	/// </summary>
	public class StateCU_BTS
	{

	/// <summary>
	/// Types for variables - since internal, just use simple data (no external enumeration).
	/// </summary>
	private readonly string TYPE_REAL = "R";
	private readonly string TYPE_INT = "I";
	private readonly string TYPE_CHAR = "C";

	/// <summary>
	/// Recognized structure variables that have significance.
	/// </summary>
	private readonly string STRUCTURE_VAR_STRUCTURE_INDEX = "Structure Index";
	private readonly string STRUCTURE_VAR_STRUCTURE_ID = "Structure ID";
	private readonly string STRUCTURE_VAR_STRUCTURE_NAME = "Structure Name";

	/// <summary>
	/// Recognized time series variables that have significance.
	/// </summary>
	private readonly string TS_VAR_STRUCTURE_INDEX = "Structure Index";
	private readonly string TS_VAR_YEAR = "Year";
	private readonly string TS_VAR_MONTH_INDEX = "Month Index";

	//private String __version = "Unknown"; // File format version as a String (e.g., "9.62") or "Unknown".
	private double __versionDouble = -999.0; // File format version as a double
	//private String __headerProgram = "StateCU";		// Program that created the file.
	//private String __headerDateString = "Unknown";		// Date for the software version.

	// Data members...

	private string __tsfile; // Name of the binary file being operated on (may or may not be a full path).
	private string __full_tsfile; // Full path to binary file being operated on.  This is
							// used as the key in the __file_Hashtable.

	private EndianRandomAccessFile __fp; // Pointer to random access file (StateCU binary files
							// are assumed to be little endian since they are written
							// by Lahey FORTRAN code on a PC).  If necessary, the year
							// value can be examined to determine the file endianness.
	private static Dictionary<string, StateCU_BTS> __fileHashtable = new Dictionary<string, StateCU_BTS>();
							// A hashtable for the file pointers (instances of
							// StateCU_BTS).  This is used to increase performance.
	private int __headerLengthBytes = 0; // Length of the header in
							// bytes, including lists of
							// stations (everything before
							// the time series data).  This
							// is assigned after reading the
							// number of stations.
	private int __oneStructureOneTimestepAllVarsBytes = 0; // Number of bytes for one structure, one timestep, all vars
	private int __oneStructureAllTimestepsAllVarsBytes = 0; // Number of bytes for one structure, all timesteps, all vars
	private int __estimatedFileLength = 0; // Estimated size of the file,
							// calculated from the header
							// information - used for
							// debugging and to check for
							// premature end of file.

	private int __intervalBase = TimeInterval.MONTH;
							// Interval base for the binary file that is being read.
	private DateTime __date1 = null; // Contains the period for the
	private DateTime __date2 = null; // data, full calendar dates to the proper date precision.

	private int __compType = StateCU_DataSet.COMP_UNKNOWN;
							// Component type for the binary
							// file:
							// COMP_CU_LOCATIONS

	// Binary header information, according to the StateCU documentation.
	// Currently, only the B1 header information is listed.

	private int __varTypeLength = 1; // Length of structure variable (parameter) types
	private int __varNameLength = 24; // Length of parameter name (spaces on end)
	private int __varReportHeaderLength = 60; // Length of report header (spaces on end)

	private int __tsVarTypeLength = 1; // Length of variable (parameter) types
	private int __tsVarNameLength = 24; // Length of parameter name (spaces on end)
	private int __tsVarUnitsLength = 10; // Length of report header (spaces on end)

	private int __numStructures = 0;
	private int __numStructureVar = 0;
	private int __numTimeSteps = 0;
	private int __numTimeSeriesVar = 0;
	private int __numAnnualTimeSeriesSteps = 0;

	/// <summary>
	/// Raw structure metadata.
	/// </summary>
	private string[] __structureVarTypes = null;
	private int[] __structureVarLength = null;
	private string[] __structureVarNames = null;
	private int[] __structureVarInReport = null;
	private string[] __structureVarReportHeaders = null;
	private object[][] __structureVarValues = null;
	/// <summary>
	/// Order of structures in the time series data section.  For example, structure 1 in
	/// the metadata may actually be in slot 13 in the time series data, due to the header
	/// and time series data being written at different times.  The information in the
	/// array is as follows (all indices are 0+):
	/// <pre>
	/// __tsStructureOrder[indexFrom __structureVar*] = index in time series data space
	/// </pre>
	/// </summary>
	private int[] __tsStructureOrder;

	/// <summary>
	/// Processed structure metadata, in the order of the "Index" structure variable.
	/// </summary>
	private string[] __structureIds = null;
	private string[] __structureNames = null;

	/// <summary>
	/// Raw time series metadata.
	/// </summary>
	private string[] __tsVarTypes = null;
	private int[] __tsVarLength = null;
	private int[] __tsVarStartBytes = null;
	private string[] __tsVarNames = null;
	private int[] __tsVarInReport = null;
	private string[] __tsVarUnits = null;

	/// <summary>
	/// Open a binary StateCU binary time series file.  It is assumed that the file
	/// exists and should be opened as read-only because typically only StateCU writes
	/// to the file.  The header information is immediately read and is available for
	/// access by other methods.  After opening the file, the readTimeSeries*() methods
	/// can be called to read time series using time series identifiers. </summary>
	/// <param name="tsfile"> Name of binary file to write. </param>
	/// <exception cref="IOException"> if unable to open the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public StateCU_BTS(String tsfile) throws java.io.IOException
	public StateCU_BTS(string tsfile)
	{ // Initialize the file using the version in the header if available...
		initialize(tsfile, -999.0);
	}

	/// <summary>
	/// Open a binary StateCU binary time series file.  It is assumed that the file
	/// exists and should be opened as read-only because typically only StateCU writes
	/// to the file.  The header information is immediately read and is available for
	/// access by other methods.  After opening the file, the readTimeSeries*() methods
	/// can be called to read time series using time series identifiers. </summary>
	/// <param name="tsfile"> Name of binary file to write. </param>
	/// <param name="file_version"> Version of StateCU that wrote the file.
	/// This is used, for example, by some TSTool commands to read old file formats. </param>
	/// <exception cref="IOException"> if unable to open the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public StateCU_BTS(String tsfile, double file_version) throws java.io.IOException
	public StateCU_BTS(string tsfile, double file_version)
	{
		initialize(tsfile, file_version);
	}

	/// <summary>
	/// Calculate the file position in bytes for any data value.  This DOES NOT position
	/// the file pointer!  For example, use this method as follows:
	/// <pre>
	/// // Find the month of data for a station and parameter...
	/// long pos = calculateFilePosition ( date, iStructure, iparam );
	/// // Position the file...
	/// __fp.seek ( pos );
	/// // Read the data...
	/// param = __fp.readLittleEndianFloat ();
	/// </pre> </summary>
	/// <param name="dateAbsoluteMonth"> Date to find as absolute month.  The month and year are considered by using an
	/// absolute month offset (year*12 + month).  Absolute month is used to increase performance since all data
	/// are currently month. </param>
	/// <param name="date1AbsoluteMonth"> The data file's first month as absolute month. </param>
	/// <param name="date2AbsoluteMonth"> The data file's last month as absolute month. </param>
	/// <param name="iStructure"> Station index (0+). </param>
	/// <param name="iTimeSeriesVar"> Parameter to find (0+). </param>
	/// <returns> byte position in file for requested parameter or -1 if unable to calculate. </returns>
	private long calculateFilePosition(int dateAbsoluteMonth, int date1AbsoluteMonth, int date2AbsoluteMonth, int iStructure, int iTimeSeriesVar)
	{
		long pos = -1;
		if ((dateAbsoluteMonth < date1AbsoluteMonth) || (dateAbsoluteMonth > date2AbsoluteMonth))
		{
			// Requested date is not in the file.
			return -1;
		}
		// Assumes monthly interval, which is all that is supported at this time
		pos = __headerLengthBytes + iStructure * __oneStructureAllTimestepsAllVarsBytes + + (dateAbsoluteMonth - date1AbsoluteMonth) * __oneStructureOneTimestepAllVarsBytes + __tsVarStartBytes[iTimeSeriesVar];
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
		if (__fileHashtable.Contains(this))
		{
			__fileHashtable.Remove(__full_tsfile);
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

		IEnumerator<string> keysEnumeration = __fileHashtable.Keys.GetEnumerator();

		StateCU_BTS bts = null;
		string filename = null;

		while (keysEnumeration.MoveNext())
		{
			filename = keysEnumeration.Current;
			bts = __fileHashtable[filename];
			bts.close();
			__fileHashtable.Remove(filename);
		}
	}

	// FIXME SAM 2008-08-21 Fix this
	// TODO SAM 2006-01-15 If it becomes important to read versions before 9.69,
	// add logic to check the file size and estimate from that the record length
	// that was used, and hence the file version.
	/// <summary>
	/// Determine the StateCU binary file version.  For StateCU version 11.x+, the
	/// file version can be determined from the binary file header.  For older versions,
	/// version 9.69 is returned, since this version has been in use for some time and
	/// is likely. </summary>
	/// <param name="filename"> the path to the file to check.  No adjustments to the path
	/// are made; therefore a full path should be provided to prevent errors. </param>
	/// <exception cref="Exception"> if there is an error opening or reading the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static double determineFileVersion(String filename) throws Exception
	public static double determineFileVersion(string filename)
	{
		StateCU_BTS bts = new StateCU_BTS(filename);
		double version_double = bts.getVersion();
		bts.close();
		bts = null;
		if (version_double < 0.0)
		{
			// Default to common old version...
			// FIXME SAM 2008-08-21
			return version_double;
			//return StateCU_Util.VERSION_9_69;
		}
		else
		{ // Version is in the file...
			return version_double;
		}
	}

	/// <summary>
	/// Replace reserved characters in regular expressions with literals, needed because
	/// some data types have -/%() characters. </summary>
	/// <param name="s"> String to process. </param>
	/// <returns> String where special characters are escaped to literals. </returns>
	private string escapeRegExpressionChars(string s)
	{
		string specialChars = "-/%()[]!^<>=?+:,|{}";
		for (int i = 0; i < specialChars.Length; i++)
		{
			s = StringUtil.replaceString(s, "" + specialChars[i], "\\" + specialChars[i]);
		}
		return s;
	}

	/// <summary>
	/// Finalize before garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~StateCU_BTS()
	{
		__fp.close();
		__fp = null;
		__tsfile = null;
		__full_tsfile = null;
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
	/// Return the time series parameter list for the file, which is determined from the file
	/// header.  Only the public parameters are provided (not extra ones that may be used internally).
	/// Currently only the time series variables of type R (floats) are returned. </summary>
	/// <returns> the parameter list read from the file header. </returns>
	public virtual string [] getTimeSeriesParameters()
	{ // Create a temporary array for output
		string[] temp = new string[__numTimeSeriesVar];
		int paramCount = 0; // count of returned parameters
		for (int iTimeSeriesVar = 0; iTimeSeriesVar < __numTimeSeriesVar; ++iTimeSeriesVar)
		{
			if (__tsVarTypes[iTimeSeriesVar].Equals(TYPE_REAL))
			{
				temp[paramCount++] = __tsVarNames[iTimeSeriesVar];
			}
		}
		// Now create the final array sized to the correct size and copy from the temporary array
		string[] parameters = new string[paramCount];
		Array.Copy(temp, 0, parameters, 0, paramCount);
		return parameters;
	}

	// TODO SAM 2006-01-15
	// If resources allow, remember to estimate the file version by back-calculating from the file size.
	/// <summary>
	/// Return the version of the file (the StateCU version that wrote the file).
	/// This information is determined from the file header for version 11.x+ and is unknown otherwise. </summary>
	/// <returns> the file format version. </returns>
	public virtual double getVersion()
	{
		return __versionDouble;
	}

	/// <summary>
	/// Initialize the binary file.  The file is opened and the header is read. </summary>
	/// <param name="tsfile"> Name of binary file. </param>
	/// <param name="file_version"> Version of StateCU that wrote the file. </param>
	/// <exception cref="IOException"> If the file cannot be opened or read. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void initialize(String tsfile, double file_version) throws java.io.IOException
	private void initialize(string tsfile, double file_version)
	{
		string routine = "StateCU_BTS.initialize";
		__tsfile = tsfile;
		__versionDouble = file_version;
		int dl = 1;

		// Open the binary file as a random access endian file.  This allows
		// Big-endian Java to read the little-endian (Microsoft/Lahey) file...

		__full_tsfile = IOUtil.getPathUsingWorkingDir(tsfile);
		__fp = new EndianRandomAccessFile(__full_tsfile, "r");

		// Initialize important data...

		__fileHashtable[__full_tsfile] = this;

		__intervalBase = TimeInterval.MONTH; // Default
		string extension = IOUtil.getFileExtension(__tsfile);
		if (extension.Equals("bd1", StringComparison.OrdinalIgnoreCase))
		{
			// CU Locations...
			__intervalBase = TimeInterval.MONTH;
			__compType = StateCU_DataSet.COMP_CU_LOCATIONS;
		}
		else
		{
			throw new Exception("StateCU file extension \"" + extension + "\" is not understood.  Cannot read binary file.");
		}

		// Read the file header version...

		readHeaderVersion();

		// Read the file header...

		readHeader();

		// Set the dates, determined by reading the first data record.

		DateTime[] dates = readDates();
		__date1 = dates[0];
		__date2 = dates[1];

		__tsStructureOrder = readStructureOrderForTimeSeriesData(__numStructures);

		// Print out some useful debug information

		if (Message.isDebugOn)
		{
			for (int iStructure = 0; iStructure < __numStructures; iStructure++)
			{
				Message.printStatus(2, routine, "Structure[" + iStructure + "] \"" + __structureNames[iStructure] + "\" time series data is in position " + __tsStructureOrder[iStructure]);
			}
			for (int iTimeSeriesVar = 0; iTimeSeriesVar < __numTimeSeriesVar; iTimeSeriesVar++)
			{
				Message.printStatus(2, routine, "Time series var [" + iTimeSeriesVar + "] \"" + __tsVarNames[iTimeSeriesVar] + "\" starts at position " + __tsVarStartBytes[iTimeSeriesVar] + " in record.");
			}
		}

		// Estimated file length...

		__estimatedFileLength = __headerLengthBytes + __oneStructureOneTimestepAllVarsBytes * __numStructures * __numTimeSteps;

		if (Message.isDebugOn)
		{
			if (__intervalBase == TimeInterval.MONTH)
			{
				Message.printDebug(dl, "", "Reading monthly data.");
			}
			else
			{
				Message.printDebug(dl, "", "Reading daily data.");
			}
			Message.printDebug(dl, "", "Header length (bytes) = " + __headerLengthBytes);
			if (__compType == StateCU_DataSet.COMP_CU_LOCATIONS)
			{
				Message.printDebug(dl, "", "Number of structures in data set = " + __numStructures);
			}
			if (__intervalBase == TimeInterval.MONTH)
			{
				Message.printDebug(dl, "", "Length of one structure one timestep (month) all parameters (bytes) = " + __oneStructureOneTimestepAllVarsBytes);
			}
			else
			{
				// Daily
				Message.printDebug(dl, "", "Length of one complete day (bytes) = " + __oneStructureOneTimestepAllVarsBytes);
			}
			Message.printDebug(dl, "", "Length of one structure all timesteps all parameters (bytes) = " + __oneStructureAllTimestepsAllVarsBytes);
			Message.printDebug(dl, "", "Estimated file size (bytes) = " + __estimatedFileLength);
		}

		// Set the following to true for troubleshooting
		bool testing = false;
		if (testing)
		{
			// Read the data brute force to check values
			readTimeSeriesData();
		}
	}

	/// <summary>
	/// Look up the file pointer to use when opening a new file.  If the file is already
	/// open and is in the internal __file_HashTable, use it.  Otherwise, open the file
	/// and add it to the Hashtable.  The code to close the file must remove the file
	/// from the Hashtable. </summary>
	/// <param name="full_fname"> Full path to file to open. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static StateCU_BTS lookupStateCUBTS(String full_fname) throws Exception
	private static StateCU_BTS lookupStateCUBTS(string full_fname)
	{
		string routine = "StateCU_BTS.lookupStateCUBTS";
		object o = __fileHashtable[full_fname];
		if (o != null)
		{
			// Have a matching file pointer so assume that it can be used...
			Message.printStatus(2, routine, "Using existing binary file.");
			return (StateCU_BTS)o;
		}
		// Else create a new file...
		Message.printStatus(2, routine, "Opening new binary file.");
		StateCU_BTS bts = new StateCU_BTS(full_fname);
		// Add to the HashTable...
		__fileHashtable[full_fname] = bts;
		return bts;
	}

	/// <summary>
	/// Read the dates, which are taken from the first time series record.
	/// The time series variables "Year" and "Month Index" contain the calendar year
	/// and months 1-12 (Jan - Feb).
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private RTi.Util.Time.DateTime[] readDates() throws java.io.IOException
	private DateTime[] readDates()
	{
		DateTime[] dates = new DateTime[2];
		// Position after the header...
		__fp.seek(__headerLengthBytes);
		// Read until the year and month are known.
		// For now read intervening variables, although the logic could be changed to jump
		// directly to the correct position.
		int startYear = -1, startMonthIndex = -1;
		object dataValue;
		for (int iTimeSeriesVar = 0; iTimeSeriesVar < __numTimeSeriesVar; ++iTimeSeriesVar)
		{
			if (__tsVarTypes[iTimeSeriesVar].Equals(TYPE_INT))
			{
				dataValue = new int?(__fp.readLittleEndianInt());
				if (__tsVarNames[iTimeSeriesVar].Equals(TS_VAR_YEAR))
				{
					startYear = ((int?)dataValue).Value;
				}
				if (__tsVarNames[iTimeSeriesVar].Equals(TS_VAR_MONTH_INDEX))
				{
					startMonthIndex = ((int?)dataValue).Value;
				}
			}
			else if (__tsVarTypes[iTimeSeriesVar].Equals(TYPE_REAL))
			{
				dataValue = new float?(__fp.readLittleEndianFloat());
			}
			else if (__tsVarTypes[iTimeSeriesVar].Equals(TYPE_CHAR))
			{
				dataValue = __fp.readLittleEndianString1(__tsVarLength[iTimeSeriesVar]).Trim();
			}
			if ((startYear > 0) && (startMonthIndex > 0))
			{
				// Done looking
				break;
			}
		}
		if ((startYear < 0) || (startMonthIndex < 0))
		{
			throw new IOException("Start year and month cannot be determined - cannot determine data period.");
		}
		if (startMonthIndex != 1)
		{
			throw new IOException("Start month (" + startMonthIndex + ") is not 1.  Corrupt data or out of date software?");
		}
		if (__numTimeSteps % 12 != 0)
		{
			throw new IOException("Number of time steps (" + __numTimeSteps + ") is not divisible by 12.  Corrupt data or out of date software?");
		}
		DateTime date1 = new DateTime(DateTime.PRECISION_MONTH);
		date1.setMonth(startMonthIndex);
		date1.setYear(startYear);
		DateTime date2 = new DateTime(date1);
		date2.addMonth(__numTimeSteps - 1);
		dates[0] = date1;
		dates[1] = date2;
		return dates;
	}

	/// <summary>
	/// Read the header from the opened binary file and save the information in
	/// memory for fast lookups.  The header is the same for all of the binary output files. </summary>
	/// <exception cref="IOException"> if there is an error reading from the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void readHeader() throws java.io.IOException
	private void readHeader()
	{
		string routine = "StateCU_BTS.readHeader";
		int dl = 1;

		__headerLengthBytes = 0;
		// First record
		//int header_rec = 0;
		/*
		if ( StateCU_Util.isVersionAtLeast(__version_double, 11.0 ) ) { // FIXME StateCU_Util.VERSION_11_00) ) {
			// Need to skip the first record that has the file version information.
			// The period for the file is in record 2...
			header_rec = 1;
		}
		*/

		__fp.seek(0);
		__numStructures = __fp.readLittleEndianInt();
		__structureIds = new string[__numStructures];
		__structureNames = new string[__numStructures];
		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "numStructures=" + __numStructures);
		}
		__numTimeSteps = __fp.readLittleEndianInt();
		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "numTimeSteps=" + __numTimeSteps);
		}
		__numStructureVar = __fp.readLittleEndianInt();
		__structureVarTypes = new string[__numStructureVar];
		__structureVarLength = new int[__numStructureVar];
		__structureVarNames = new string[__numStructureVar];
		__structureVarInReport = new int[__numStructureVar];
		__structureVarReportHeaders = new string[__numStructureVar];
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: __structureVarValues = new object[__numStructures][__numStructureVar];
		__structureVarValues = RectangularArrays.RectangularObjectArray(__numStructures, __numStructureVar);
		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "numStructureVar=" + __numStructureVar);
		}
		__numTimeSeriesVar = __fp.readLittleEndianInt();
		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "numTimeSeriesVar=" + __numTimeSeriesVar);
		}
		__tsVarTypes = new string[__numTimeSeriesVar];
		__tsVarLength = new int[__numTimeSeriesVar];
		__tsVarStartBytes = new int[__numTimeSeriesVar];
		__tsVarNames = new string[__numTimeSeriesVar];
		__tsVarInReport = new int[__numTimeSeriesVar];
		__tsVarUnits = new string[__numTimeSeriesVar];
		__numAnnualTimeSeriesSteps = __fp.readLittleEndianInt();
		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "numAnnualTimeSeriesSteps=" + __numAnnualTimeSeriesSteps);
		}
		// Header length is 4 bytes for each of the 5 variables above.
		__headerLengthBytes += 20;

		// Read the structure main metadata records...

	   for (int iStructureVar = 0; iStructureVar < __numStructureVar; ++iStructureVar)
	   {
			__structureVarTypes[iStructureVar] = __fp.readLittleEndianString1(__varTypeLength).Trim();
			__headerLengthBytes += __varTypeLength;
			if (Message.isDebugOn)
			{
				Message.printDebug(dl, routine, "structureVar[" + iStructureVar + "] variable type = \"" + __structureVarTypes[iStructureVar] + "\"");
			}
			__structureVarLength[iStructureVar] = __fp.readLittleEndianInt();
			__headerLengthBytes += 4;
			if (Message.isDebugOn)
			{
				Message.printDebug(dl, routine, "structureVar[" + iStructureVar + "] variable length = \"" + __structureVarLength[iStructureVar] + "\"");
			}
			__structureVarNames[iStructureVar] = __fp.readLittleEndianString1(__varNameLength).Trim();
			__headerLengthBytes += __varNameLength;
			if (Message.isDebugOn)
			{
				Message.printDebug(dl, routine, "structureVar[" + iStructureVar + "] variable name = \"" + __structureVarNames[iStructureVar] + "\"");
			}
			__structureVarInReport[iStructureVar] = __fp.readLittleEndianInt();
			__headerLengthBytes += 4;
			if (Message.isDebugOn)
			{
				Message.printDebug(dl, routine, "structureVar[" + iStructureVar + "] variable in report = \"" + __structureVarInReport[iStructureVar] + "\"");
			}
			__structureVarReportHeaders[iStructureVar] = __fp.readLittleEndianString1(__varReportHeaderLength).Trim();
			__headerLengthBytes += __varReportHeaderLength;
			if (Message.isDebugOn)
			{
				Message.printDebug(dl, routine, "structureVar[" + iStructureVar + "] variable report Header = \"" + __structureVarReportHeaders[iStructureVar] + "\"");
			}
	   }

		// Time series metadata

	   __oneStructureOneTimestepAllVarsBytes = 0;
		for (int iTimeSeriesVar = 0; iTimeSeriesVar < __numTimeSeriesVar; ++iTimeSeriesVar)
		{
			__tsVarTypes[iTimeSeriesVar] = __fp.readLittleEndianString1(__tsVarTypeLength).Trim();
			__headerLengthBytes += __tsVarTypeLength;
			if (Message.isDebugOn)
			{
				Message.printDebug(dl, routine, "tsVar[" + iTimeSeriesVar + "] variable type = \"" + __tsVarTypes[iTimeSeriesVar] + "\"");
			}
			__tsVarLength[iTimeSeriesVar] = __fp.readLittleEndianInt();
			__oneStructureOneTimestepAllVarsBytes += __tsVarLength[iTimeSeriesVar];
			__headerLengthBytes += 4;
			// Increment the starting position of the variable in the time series data record.
			// The position of the first variable within the record will be 0.
			if (iTimeSeriesVar == 0)
			{
				__tsVarStartBytes[0] = 0;
			}
			else
			{
				__tsVarStartBytes[iTimeSeriesVar] = __tsVarStartBytes[iTimeSeriesVar - 1] + __tsVarLength[iTimeSeriesVar - 1];
			}
			if (Message.isDebugOn)
			{
				Message.printDebug(dl, routine, "tsVar[" + iTimeSeriesVar + "] variable length = \"" + __tsVarLength[iTimeSeriesVar] + "\"");
			}
			__tsVarNames[iTimeSeriesVar] = __fp.readLittleEndianString1(__tsVarNameLength).Trim();
			__headerLengthBytes += __tsVarNameLength;
			if (Message.isDebugOn)
			{
				Message.printDebug(dl, routine, "tsVar[" + iTimeSeriesVar + "] variable name = \"" + __tsVarNames[iTimeSeriesVar] + "\"");
			}
			__tsVarInReport[iTimeSeriesVar] = __fp.readLittleEndianInt();
			__headerLengthBytes += 4;
			if (Message.isDebugOn)
			{
				Message.printDebug(dl, routine, "tsVar[" + iTimeSeriesVar + "] variable in report = \"" + __tsVarInReport[iTimeSeriesVar] + "\"");
			}
			__tsVarUnits[iTimeSeriesVar] = __fp.readLittleEndianString1(__tsVarUnitsLength).Trim();
			__headerLengthBytes += __tsVarUnitsLength;
			if (Message.isDebugOn)
			{
				Message.printDebug(dl, routine, "tsVar[" + iTimeSeriesVar + "] units = \"" + __tsVarUnits[iTimeSeriesVar] + "\"");
			}
		}
		// Size of full time series block for a structure
		__oneStructureAllTimestepsAllVarsBytes = __oneStructureOneTimestepAllVarsBytes * __numTimeSteps;

		// Now read the structure records

		int structureIndex = -1; // index in processed arrays for IDs, Name
		string structureID; // ID for one structure
		string structureName; // Name for one structure
		for (int iStructure = 0; iStructure < __numStructures; ++iStructure)
		{
			for (int iStructureVar = 0; iStructureVar < __numStructureVar; ++iStructureVar)
			{
				if (__structureVarTypes[iStructureVar].Equals(TYPE_INT))
				{
					__structureVarValues[iStructure][iStructureVar] = new int?(__fp.readLittleEndianInt());
					__headerLengthBytes += 4;
				}
				else if (__structureVarTypes[iStructureVar].Equals(TYPE_REAL))
				{
					__structureVarValues[iStructure][iStructureVar] = new float?(__fp.readLittleEndianFloat());
					__headerLengthBytes += 4;
				}
				else if (__structureVarTypes[iStructureVar].Equals(TYPE_CHAR))
				{
					__structureVarValues[iStructure][iStructureVar] = __fp.readLittleEndianString1(__structureVarLength[iStructureVar]).Trim();
					__headerLengthBytes += __structureVarLength[iStructureVar];
				}
				else
				{
					throw new IOException("Structure variable type \"" + __structureVarTypes[iStructureVar] + "\" is not recognized.");
				}
				if (Message.isDebugOn)
				{
					Message.printDebug(dl, routine, "Structure[" + iStructure + "] Var[" + iStructureVar + "] " + __structureVarNames[iStructureVar] + " = " + __structureVarValues[iStructure][iStructureVar]);
				}
				// Now process into the ordered arrays.
				if (__structureVarNames[iStructureVar].Equals(STRUCTURE_VAR_STRUCTURE_INDEX))
				{
					// Save the index for assignment below.  The index property is always before the ID and name and
					// is 1+ so need to decrement to get zero-based indices for internal arrays
					structureIndex = ((int?)__structureVarValues[iStructure][iStructureVar]).Value - 1;
				}
				else if (__structureVarNames[iStructureVar].Equals(STRUCTURE_VAR_STRUCTURE_ID))
				{
					structureID = (string)__structureVarValues[iStructure][iStructureVar];
					__structureIds[structureIndex] = structureID;
				}
				else if (__structureVarNames[iStructureVar].Equals(STRUCTURE_VAR_STRUCTURE_NAME))
				{
					structureName = (string)__structureVarValues[iStructure][iStructureVar];
					__structureNames[structureIndex] = structureName;
				}
			}
		}
		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "Length of all header information is " + __headerLengthBytes + " bytes.");
		}
	}

	/// <summary>
	/// For an open binary file, determine the StateCU version from the first record.
	/// If the record does not contain the version (e.g., old file format), then try
	/// to determine the version from the current StateCU executable that is available. </summary>
	/// <exception cref="IOException"> if an error occurs reading the header (usually due to an
	/// empty file). </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void readHeaderVersion() throws java.io.IOException
	private void readHeaderVersion()
	{ // The initial StateCU binary file specification does not have a header with version.
		//__version = "Unknown";
		__versionDouble = -999.0;
		//__headerProgram = "StateCU";
		//__headerDateString = null;
	}

	/// <summary>
	/// Read a time series from a StateCU binary file.  The TSID string is specified
	/// in addition to the path to the file.  It is expected that a TSID in the file
	/// matches the TSID (and the path to the file, if included in the TSID would not
	/// properly allow the TSID to be specified).  This method can be used with newer
	/// code where the I/O path is separate from the TSID that is used to identify the
	/// time series.
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
	/// Read a time series from a StateCU binary file.  The TSID string is specified
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
	/// <tr><td>CloseWhenDone</td>	<td>Specifies whether to close the<br>
	///				file once the time series has been<br>
	///				read from it</td>
	///							<td>False<br>
	///							True is the only <br>
	///							other acceptable <br>
	///							value.</td>
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
			if (!string.ReferenceEquals(closeWhenTrue, null) && closeWhenTrue.Trim().Equals("true", StringComparison.OrdinalIgnoreCase))
			{
				closeFile = true;
			}
		}

		if (!IOUtil.fileReadable(full_fname))
		{
			Message.printWarning(2, "StateCU_BTS.readTimeSeries", "Unable to determine file for \"" + filename + "\"");
			return ts;
		}
		StateCU_BTS @in = null;
		try
		{
			@in = lookupStateCUBTS(full_fname);
		}
		catch (Exception)
		{
			Message.printWarning(2, "StateCU_BTS.readTimeSeries(String,...)", "Unable to open file \"" + full_fname + "\"");
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
			Message.printWarning(2, "StateCU_BTS.readTimeSeries(String,...)", "Unable to read time series for \"" + tsident_string + "\"");
			return ts;
		}
		return (TS)tslist[0];
	}

	/// <summary>
	/// Read a single time series.  This is currently not used because the reading is done
	/// in readTimeSeriesList().
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private RTi.TS.TS readTimeSeriesData() throws java.io.IOException
	private TS readTimeSeriesData()
	{
		string routine = "StateCU_BTS.readTimeSeries";
		int dl = 1; // Debug level
		// Read the time series data

		long pos = __headerLengthBytes;
		__fp.seek(pos);
		object dataValue;
		for (int iStructure = 0; iStructure < __numStructures; ++iStructure)
		{
			for (int iTimeStep = 0; iTimeStep < __numTimeSteps; ++iTimeStep)
			{
				for (int iTimeSeriesVar = 0; iTimeSeriesVar < __numTimeSeriesVar; ++iTimeSeriesVar)
				{
					if (__tsVarTypes[iTimeSeriesVar].Equals(TYPE_INT))
					{
						dataValue = new int?(__fp.readLittleEndianInt());
					}
					else if (__tsVarTypes[iTimeSeriesVar].Equals(TYPE_REAL))
					{
						dataValue = new float?(__fp.readLittleEndianFloat());
					}
					else if (__tsVarTypes[iTimeSeriesVar].Equals(TYPE_CHAR))
					{
						dataValue = __fp.readLittleEndianString1(__tsVarLength[iTimeSeriesVar]).Trim();
					}
					else
					{
						throw new IOException("Time series variable type \"" + __tsVarTypes[iTimeSeriesVar] + "\" is not recognized.");
					}
					// Increment position by length of variable, for debugging below
					pos += __tsVarLength[iTimeSeriesVar];
					if (Message.isDebugOn)
					{
						Message.printDebug(dl, routine, "Structure[" + iStructure + "] time step [" + iTimeStep + "] Var[" + iTimeSeriesVar + "] = " + dataValue + " starting at byte " + pos);
					}
				}
			}
		}

		return null;
	}

	/// <summary>
	/// Read a list of time series from the binary file.  A Vector of new time series is returned. </summary>
	/// <param name="tsidentPattern"> A regular expression for TSIdents to return.  For example
	/// or null returns all time series.  *.*.XXX.* returns only time series matching
	/// data type XXX.  Currently only location and data type (output parameter) are
	/// checked and only a
	/// wildcard can be specified, if used.  This is useful for TSTool in order to
	/// list all stations that have a data type.  For reservoirs, only the main location
	/// can be matched, and the returned list of time series will include time series
	/// for all accounts.  When matching a specific time series (no wildcards), the
	/// main location part is first matched and the the reservoir account is checked
	/// if the main location is matched. </param>
	/// <param name="reqDate1"> First date/time to read, or null to read the full period. </param>
	/// <param name="reqDate2"> Last date/time to read, or null to read the full period. </param>
	/// <param name="reqUnits"> Requested units for the time series (currently not
	/// implemented). </param>
	/// <param name="readData"> True if all data should be read or false to only read the headers. </param>
	/// <exception cref="IOException"> if the interval for the time series does not match that
	/// for the file or if a write error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public java.util.List<RTi.TS.TS> readTimeSeriesList(String tsidentPattern, RTi.Util.Time.DateTime reqDate1, RTi.Util.Time.DateTime reqDate2, String reqUnits, boolean readData) throws Exception
	public virtual IList<TS> readTimeSeriesList(string tsidentPattern, DateTime reqDate1, DateTime reqDate2, string reqUnits, bool readData)
	{
		string routine = "StateCUd_BTS.readTimeSeriesList";
		// Using previously read information, loop through each time series
		// identifier and see if it matches what we are searching for...

		// TODO (JTS - 2004-08-04)
		// there is no non-static readTimeSeries() method.  One should
		// be added for efficiency's sake.
		// SAM 2006-01-04 - non-static is used because there is a penalty
		// reading the header.  This needs to be considered.

		IList<TS> tslist = new List<TS>();
		if ((string.ReferenceEquals(tsidentPattern, null)) || (tsidentPattern.Length == 0))
		{
			// Match all parts when searching the data.
			tsidentPattern = "*.*.*.*.*";
		}
		TSIdent tsident_regexp = new TSIdent(tsidentPattern); // TSIdent containing the regular expression parts.
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
		// These fields really have no bearing on the filter, but if not
		// wildcarded, may cause a match to not be found...
		tsident_regexp.setSource("*");
		tsident_regexp.setInterval("*");
		tsident_regexp.setScenario("*");
		string tsident_regexp_loc = escapeRegExpressionChars(tsident_regexp.getLocation());
		string tsident_regexp_source = escapeRegExpressionChars(tsident_regexp.getSource());
		string tsident_regexp_type = escapeRegExpressionChars(tsident_regexp.getType());
		string tsident_regexp_interval = escapeRegExpressionChars(tsident_regexp.getInterval());
		string tsident_regexp_scenario = escapeRegExpressionChars(tsident_regexp.getScenario());
		bool station_has_wildcard = false; // Use to speed up
		bool datatype_has_wildcard = false; // loops.
		TS ts = null;
		float param = -999;
		long filepos;
		DateTime date;
		// Absolute months to improve performance, since only monthly data is supported
		int date1AbsoluteMonth = __date1.getAbsoluteMonth(), date2AbsoluteMonth = __date2.getAbsoluteMonth();
		int dl = 1;
		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "Reading time series for \"" + tsidentPattern + "\" __numStructures = " + __numStructures + " read data = " + readData);
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
			bool match_found = false; // Indicates if a match for the specific station is made.
			//int its = 0;		// Loop counter for time series.
			//
			TSIdent current_tsident = new TSIdent();
						// Used for the time series below, to compare to the pattern.
			TSIdent tsident = null; // Used when creating new time series.

			// Loop through the structure identifiers in the list...
			for (int iStructure = 0; iStructure < __numStructures; iStructure++)
			{
				if (Message.isDebugOn)
				{
					Message.printDebug(dl, routine, "Station[" + iStructure + "] = \"" + __structureNames[iStructure] + "\"");
				}
				// Loop through the parameters...
				for (int iparam = 0; iparam < __numTimeSeriesVar; iparam++)
				{
					// Check the station and parameter to see if
					// they match - all other fields are allowed to be wild-carded...
					if (Message.isDebugOn)
					{
						Message.printDebug(2, routine, "Parameter = \"" + __tsVarNames[iparam] + "\"");
					}
					// Need to match against each station ID list.
					// Set the information from the file into the working "tsident" to compare...
					current_tsident.setLocation(__structureIds[iStructure]);
					current_tsident.setType(__tsVarNames[iparam]);
					// Other TSID fields are left blank to match all.
					if (!current_tsident.matches(tsident_regexp_loc, tsident_regexp_source, tsident_regexp_type, tsident_regexp_interval, tsident_regexp_scenario, null, null, false))
					{
						// This time series does not match one that is requested.  Just need to
						// match the location and parameter since that is all that is in the file.
						if (Message.isDebugOn)
						{
							Message.printDebug(dl, routine, "Requested \"" + tsidentPattern + "\" does not match \"" + __structureIds[iStructure] + "\" \"" + __tsVarNames[iparam] + "\"");
						}
						continue;
					}
					if (!shouldTimeSeriesBeIncluded(iparam))
					{
						continue;
					}
					if (Message.isDebugOn)
					{
						Message.printDebug(2, routine, "Requested \"" + tsidentPattern + "\" does match \"" + __structureIds[iStructure] + "\" \"" + __tsVarNames[iparam] + "\"");
					}
					if (__intervalBase == TimeInterval.MONTH)
					{
						ts = new MonthTS();
						tsident = new TSIdent(__structureIds[iStructure], "StateCU", __tsVarNames[iparam], "Month","", "StateCUB", __tsfile);
					}
					else if (__intervalBase == TimeInterval.DAY)
					{
						ts = new DayTS();
						tsident = new TSIdent(__structureIds[iStructure], "StateCU", __tsVarNames[iparam], "Day","", "StateCUB", __tsfile);
					}
					// Set time series header information...
					ts.setIdentifier(tsident);
					ts.setInputName(__tsfile);
					ts.setDescription(__structureNames[iStructure]);
					ts.setDataType(__tsVarNames[iparam]);
					ts.setDataUnits(__tsVarUnits[iparam]);

					// Original dates from file header...
					ts.setDate1Original(new DateTime(__date1));
					ts.setDate2Original(new DateTime(__date2));
					// Time series dates from requested parameters or file...
					if (reqDate1 == null)
					{
						reqDate1 = new DateTime(__date1);
						ts.setDate1(reqDate1);
					}
					else
					{
						ts.setDate1(new DateTime(reqDate1));
					}
					if (reqDate2 == null)
					{
						reqDate2 = new DateTime(__date2);
						ts.setDate2(reqDate2);
					}
					else
					{
						ts.setDate2(new DateTime(reqDate2));
					}
					ts.addToGenesis("Read from \"" + __tsfile + " for " + reqDate1 + " to " + reqDate2);
					tslist.Add(ts);
					if (readData)
					{
						if (Message.isDebugOn)
						{
							Message.printDebug(2, routine, "Reading " + reqDate1 + " to " + reqDate2);
						}
						// Allocate the data space...
						if (ts.allocateDataSpace() != 0)
						{
							throw new Exception("Unable to allocate data space.");
						}
						// Read the data for the time series...
						// To optimize reading of values, use a couple of booleans to indicate the data type
						bool paramTypeReal = false;
						bool paramTypeInt = false;
						if (__tsVarTypes[iparam].Equals(TYPE_REAL))
						{
							paramTypeReal = true;
						}
						else if (__tsVarTypes[iparam].Equals(TYPE_INT))
						{
							paramTypeInt = true;
						}
						for (date = new DateTime(reqDate1); date.lessThanOrEqualTo(reqDate2); date.addInterval(__intervalBase, 1))
						{
							if ((date.getMonth() == 2) && (date.getDay() == 29))
							{
								// StateCU does not handle.
								// FIXME Check on StateCU
								continue;
							}
							filepos = calculateFilePosition(date.getAbsoluteMonth(), date1AbsoluteMonth, date2AbsoluteMonth, __tsStructureOrder[iStructure], iparam);
							if (filepos < 0)
							{
								// Leave missing in the result.
								continue;
							}
							// Move the file pointer to the read position
							__fp.seek(filepos);
							try
							{
								// Only real values should be attempted because only data types associated
								// with reals will be in the parameter list that is visible to external code.
								if (paramTypeReal)
								{
									param = __fp.readLittleEndianFloat();
								}
								else if (paramTypeInt)
								{
									param = (float)__fp.readLittleEndianInt();
								}
								if (Message.isDebugOn)
								{
									Message.printDebug(dl, routine, "Read value " + param + " for " + date + " iStructure=" + iStructure + " iparam=" + iparam + " filepos=" + filepos);
								}
							}
							catch (Exception)
							{
								// Assume end of file so break out of read.
								Message.printWarning(3, routine, "Unexpected error reading for date " + date + " structure[" + iStructure + "] param[" + iparam + "] filepos(byte)=" + filepos + " - stop reading data.  Expected file size =" + __estimatedFileLength);
								break;
							}
							// Convert units if requested...
							// FIXME SAM Need to enable units conversion
							ts.setDataValue(date,param);
						}
					}
					if (!datatype_has_wildcard && match_found)
					{
						// No need to keep searching...
						break;
					}
				} // End data type match_found
				if (!station_has_wildcard && match_found)
				{
					// No need to keep searching...
					break;
				}
			}
		}
		catch (Exception e)
		{
			Message.printWarning(3, "", e);
		}
		return tslist;
	}

	/// <summary>
	/// Read the the time series to structure mapping array.
	/// This requires reading the first time series record from each structure to determine
	/// the "Structure Index" value for the time series.  The structures themselves are
	/// listed in the order of the output but does not agree with the initial structure
	/// order. </summary>
	/// <param name="numStructures"> the number of structures. </param>
	/// <returns> the array indicating the position of the structures in the time series data
	/// block records for each of the structures in the metadata. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private int[] readStructureOrderForTimeSeriesData(int numStructures) throws java.io.IOException
	private int[] readStructureOrderForTimeSeriesData(int numStructures)
	{
		string routine = "StateCU_BTS.readStructureOrderForTimeSeriesData";
		int[] tsStructureOrder = new int[numStructures];
		// Read until the year and month are known.
		// For now read intervening variables, although the logic could be changed to jump
		// directly to the correct position.
		long pos;
		for (int iStructure = 0; iStructure < __numStructures; ++iStructure)
		{
			// Position the pointer at the start of the structures data
			pos = __headerLengthBytes + iStructure * __oneStructureAllTimestepsAllVarsBytes;
			__fp.seek(pos);
			// Looping through the first time step for the structure - no need to process
			// any other timesteps.
			// Brute force read through the variables for each time series and look for the
			// "Structure Index" variable (the first one according to code example from Jim Brannon
			// so it should be fast).
			int structurePosInHeader = -1;
			for (int iTimeSeriesVar = 0; iTimeSeriesVar < __numTimeSeriesVar; ++iTimeSeriesVar)
			{
				if (__tsVarTypes[iTimeSeriesVar].Equals(TYPE_INT))
				{
					int i = __fp.readLittleEndianInt();
					if (__tsVarNames[iTimeSeriesVar].Equals(TS_VAR_STRUCTURE_INDEX))
					{
						// Have the value of interest.
						structurePosInHeader = i - 1; // Data is 1+, but internally need 0+
						if (Message.isDebugOn)
						{
							Message.printDebug(1, routine, "Structure index in time series loop [" + iStructure + "] is [" + structurePosInHeader + "] read from byte " + pos);
						}
						// No need to keep reading variables
						break;
					}
				}
				else if (__tsVarTypes[iTimeSeriesVar].Equals(TYPE_REAL))
				{
					// Read and skip...
					__fp.readLittleEndianFloat();
				}
				else if (__tsVarTypes[iTimeSeriesVar].Equals(TYPE_CHAR))
				{
					// Read and skip...
					__fp.readLittleEndianString1(__tsVarLength[iTimeSeriesVar]);
				}
				// For debugging
				pos += __tsVarLength[iTimeSeriesVar];
			}
			if (structurePosInHeader < 0)
			{
				// Something is wrong - could not figure out the structure index for the structure.
				throw new IOException("Unable to find Structure Index for time series.  Corrupt data or out of date software?");
			}
			tsStructureOrder[structurePosInHeader] = iStructure;
		}
		return tsStructureOrder;
	}

	/// <summary>
	/// Return the number of time series in the file, including only public time series. </summary>
	/// <returns> the number of public time series in the file. </returns>
	public virtual int size()
	{
		return __numStructures * getTimeSeriesParameters().Length;
	}

	/// <summary>
	/// Determine whether time series should be included in output.  Only time series data types
	/// that have real data are included. </summary>
	/// <param name="iTimeSeriesVar"> time series parameter </param>
	private bool shouldTimeSeriesBeIncluded(int iTimeSeriesVar)
	{
		if (__tsVarTypes[iTimeSeriesVar].Equals(TYPE_REAL))
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	} // End StateCU_BTS

}