using System;
using System.Collections.Generic;
using System.IO;

// StateMod_DownstreamCall - this class stores downstream call data

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

namespace DWR.StateMod
{

	using DayTS = RTi.TS.DayTS;
	using TSUtil = RTi.TS.TSUtil;

	using IOUtil = RTi.Util.IO.IOUtil;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;
	using DateTime = RTi.Util.Time.DateTime;
	using TimeUtil = RTi.Util.Time.TimeUtil;

	/// <summary>
	/// This class stores downstream call data.  Currently the class provides a read method because a single daily
	/// time series is read from the file.  This is experimental data for operating rule type 23 and more enhancements
	/// may be needed in the future.
	/// </summary>
	public class StateMod_DownstreamCall //extends StateMod_Data
	{
	//implements Cloneable, Comparable

	/// <summary>
	/// Read return information in and store in a list. </summary>
	/// <param name="filename"> filename for data file to read </param>
	/// <exception cref="Exception"> if an error occurs </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static RTi.TS.DayTS readStateModFile(String filename) throws Exception
	public static DayTS readStateModFile(string filename)
	{
		string routine = "StateMod_DownstreamCall.readStateModFile";
		string iline = null;
		IList<string> vData = new List<string>(4);
		int linecount = 0;

		StreamReader @in = null;

		Message.printStatus(2, routine, "Reading downstream call file: " + filename);
		int size = 0;
		int errorCount = 0;
		int year, month, day;
		double adminNumber;
		DateTime date = new DateTime(DateTime.PRECISION_DAY);
		DayTS ts = null;
		try
		{
			@in = new StreamReader(IOUtil.getPathUsingWorkingDir(filename));
			bool headerRead = false;
			while (!string.ReferenceEquals((iline = @in.ReadLine()), null))
			{
				++linecount;
				// check for comments
				if (iline.StartsWith("#", StringComparison.Ordinal) || (iline.Trim().Length == 0))
				{
					// Special dynamic header comments written by software and blank lines - no need to keep
					continue;
				}
				if (Message.isDebugOn)
				{
					Message.printDebug(50, routine, "line: " + iline);
				}
				if (!headerRead)
				{
					// Read the header line and create the time series.
					// This code copied from StateMod_TS.readTimeSeriesList()
					string format = "i5x1i4x5i5x1i4s5s5";
					IList<object> v = StringUtil.fixedRead(iline, format);
					int m1 = ((int?)v[0]).Value;
					int y1 = ((int?)v[1]).Value;
					int m2 = ((int?)v[2]).Value;
					int y2 = ((int?)v[3]).Value;
					DateTime date1Header;
					date1Header = new DateTime(DateTime.PRECISION_DAY);
					date1Header.setYear(y1);
					date1Header.setMonth(m1);
					date1Header.setDay(1);
					DateTime date2Header;
					date2Header = new DateTime(DateTime.PRECISION_DAY);
					date2Header.setYear(y2);
					date2Header.setMonth(m2);
					date2Header.setDay(TimeUtil.numDaysInMonth(m2,y2));
					string units = ((string)v[4]).Trim();
					string yeartypes = ((string)v[5]).Trim();
					// TODO SAM 2011-01-02 Year type is not used since year month and day are all calendar
					/*
					int yeartype = StateMod_DataSet.SM_CYR;
					// Year type is used in one place to initialize the year when
					// transferring data.  However, it is assumed that m1 is always correct for the year type.
					if ( yeartypes.equalsIgnoreCase("WYR") ) {
						yeartype = StateMod_DataSet.SM_WYR;
					}
					else if ( yeartypes.equalsIgnoreCase("IYR") ) {
						yeartype = StateMod_DataSet.SM_IYR;
					}
					*/
					// year that are specified are used to set the period.
					if (Message.isDebugOn)
					{
						Message.printDebug(1, routine, "Parsed m1=" + m1 + " y1=" + y1 + " m2=" + m2 + " y2=" + y2 + " units=\"" + units + "\" yeartype=\"" + yeartypes + "\"");
					}
					// Now create the time series
					string tsidentString = "OprType23..DownstreamCall.Day";
					ts = (DayTS)TSUtil.newTimeSeries(tsidentString, true);
					ts.setDate1(date1Header);
					ts.setDate1Original(date1Header);
					ts.setDate2(date2Header);
					ts.setDate2Original(date2Header);
					ts.setDataUnits(units);
					ts.setDataUnitsOriginal(units);
					ts.allocateDataSpace();
				}
				else
				{
					// A time series data line
					// Break the line using whitespace, while allowing for quoted strings...
					try
					{
						vData = StringUtil.breakStringList(iline, " \t", StringUtil.DELIM_ALLOW_STRINGS | StringUtil.DELIM_SKIP_BLANKS);
						size = 0;
						if (vData != null)
						{
							size = vData.Count;
						}
						if (size < 3)
						{
							Message.printStatus(2, routine, "Ignoring line " + linecount + " not enough data values.  Have " + size + " expecting 3");
							++errorCount;
							continue;
						}
						// Uncomment if testing...
						//Message.printStatus ( 2, routine, "" + v );

						// Allocate new plan node and set the values
						year = int.Parse(vData[0].Trim());
						month = int.Parse(vData[1].Trim());
						day = int.Parse(vData[2].Trim());
						adminNumber = double.Parse(vData[3].Trim());
						date.setYear(year);
						date.setMonth(month);
						date.setDay(day);
						ts.setDataValue(date, adminNumber);
					}
					catch (Exception e2)
					{
						Message.printWarning(3, routine, "Error reading line " + linecount + " \"" + iline + "\" (" + e2 + ") - skipping line.");
						Message.printWarning(3, routine, e2);
					}
				}

				// Set the data to not dirty because it was just initialized...

				ts.setDirty(false);
			}
		}
		catch (Exception e)
		{
			Message.printWarning(3, routine, "Error reading line " + linecount + " \"" + iline + "\" uniquetempvar.");
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
		if (errorCount > 0)
		{
			throw new Exception("There were " + errorCount + " errors processing the data - refer to log file.");
		}
		// Return the single time series.
		return ts;
	}

	}

}