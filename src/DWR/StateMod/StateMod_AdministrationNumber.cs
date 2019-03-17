using System;

// StateMod_AdministrationNumber - class to store and manipulate administration number, as per HydroBase definition

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
// StateMod_AdministrationNumber - class to store and manipulate 
//				an administration number, as per HydroBase definition
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
//
// 22 Nov 1998	Steven A. Malers, RTi	Break code out of HBWaterRightNet
//					because we need a more full-featured
//					class for use with demandts.
// 26 Jan 1999	SAM, RTi		Overload constructor to take single
//					values, integers, etc.
// 10 Feb 1999	SAM, RTi		Update to have finalize().
// 24 May 2001	SAM, RTi		Add setAppropriationDate() to be used
//					when updating an administration number
//					without allocating a new one.
// 2003-02-05	J. Thomas Sapienza, RTi	Adapted from HBAdministrationNumber.
// 2003-04-08	JTS, RTi		Moved TSDates to DateTime.
// 2004-02-04	JTS, RTi		Now extends DMIDataObject.
// 2005-02-11	JTS, RTi		No longer extends DMIDataObject.
// 2007-02-26	SAM, RTi		Clean up code based on Eclipse feedback.
//------------------------------------------------------------------------------
// 2007-05-15	SAM, RTi		Copy HydroBase version to allow StateMod code to
//					have stand-alone version.
//------------------------------------------------------------------------------

namespace DWR.StateMod
{
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;
	using DateTime = RTi.Util.Time.DateTime;
	using TimeUtil = RTi.Util.Time.TimeUtil;

	/// <summary>
	/// Store and convert administration numbers.  The conversion logic is based on
	/// materials supplied by Ray Bennett, with the most accurate algorithm being a
	/// simple dBase procedure called HNUMBER.  Conversion occurs at construction and
	/// then member functions can be used to query data.  The administration number
	/// is stored internally as two integers representing the 5 digits before the
	/// decimal point and the 5 digits after the decimal point.  A floating point
	/// representation of the administration number is also stored; however, because
	/// of possible round-off and truncation, the integer representation is the most
	/// accurate.  Use the toString() method to generate an 11.5 format string for the
	/// administration number.  Use the equals(), lessThan(), and greaterThan() methods
	/// to compare administration numbers.  Note that reading an administration number
	/// as a floating-point number from a database may introduce a round-off error
	/// that will result in an error back-computing the dates.  Usually the error
	/// in this case is no more than one day.
	/// 
	/// This class may be updated in the future to provide alternate representations of
	/// a date, for use with StateMod water rights.
	/// </summary>
	public sealed class StateMod_AdministrationNumber
	{

	// Static data...

	public const int FORMAT_VERBOSE = 1; // Ways to format for toString()

	private static DateTime _admin_number_date_datum = null;
	private static int _admin_number_date_datum_days = 0;

	// Instance data...

	private double _admin_number; // 11.5 administration number
	private int _whole; // Whole part of the administration number
	private int _fraction; // Fractional part of the administration number
	private DateTime _appro_date; // Appropriation date.
	private DateTime _padj_date; // Prior adjudication date.

	/// <summary>
	/// Construct the administration number from the appropriation dates.  The
	/// prior adjudication date is assumed to be null. </summary>
	/// <param name="appro_date"> Appropriation date. </param>
	/// <exception cref="Exception"> if an error occurs </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public StateMod_AdministrationNumber(RTi.Util.Time.DateTime appro_date) throws Exception
	public StateMod_AdministrationNumber(DateTime appro_date)
	{
		// The following throws an exception...
		reset(appro_date, (DateTime)null);
	}

	/// <summary>
	/// Construct the administration number from the appropriation and prior
	/// adjudication dates. </summary>
	/// <param name="appro_date"> Appropriation date. </param>
	/// <param name="prior_adj_date"> Prior adjudication date (specify as null if no prior
	/// adjudication date applies). </param>
	/// <exception cref="Exception"> if an error occurs </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public StateMod_AdministrationNumber(RTi.Util.Time.DateTime appro_date, RTi.Util.Time.DateTime prior_adj_date) throws Exception
	public StateMod_AdministrationNumber(DateTime appro_date, DateTime prior_adj_date)
	{
		// The following throws an exception...
		reset(appro_date, prior_adj_date);
	}

	/// <summary>
	/// Construct from an administration number.  This is the inverse of
	/// of constructing with the dates.  If the number is an integer, the appropriation
	/// date is
	/// the integer part.  Otherwise, the remainder*100000 is the appropriation date.
	/// The prior adjudication date is assumed to be null. </summary>
	/// <param name="admin_number"> The administration number. </param>
	/// <exception cref="Exception"> if an error occurs </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public StateMod_AdministrationNumber(double admin_number) throws Exception
	public StateMod_AdministrationNumber(double admin_number)
	{
		// The following throws Exception...
		reset(admin_number, (DateTime)null);
	}

	/// <summary>
	/// Construct from a Julian apropriation date.  This is the inverse of
	/// of constructing with the dates.
	/// The prior adjudication date is assumed to be null. </summary>
	/// <param name="admin_number"> The administration number. </param>
	/// <exception cref="Exception"> if an error occurs </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public StateMod_AdministrationNumber(int admin_number) throws Exception
	public StateMod_AdministrationNumber(int admin_number)
	{
		// The following throws Exception...
		reset((double)admin_number, (DateTime)null);
	}

	/// <summary>
	/// Construct from an administration number and prior adjudication date.
	/// This is the inverse of of constructing with the dates.  If the number is an
	/// integer, the appropriation date is
	/// the integer part.  Otherwise, the remainder*100000 is the appropriation date. </summary>
	/// <param name="admin_number"> The administration number. </param>
	/// <param name="prior_adj_date"> If supplied it is used to determine the appropriation
	/// date when the administration number is not an integer.  
	/// If not supplied (use null), the determination of prior
	/// adjudication date cannot be computed if the administration number is not
	/// an integer. </param>
	/// <exception cref="Exception"> if an error occurs </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public StateMod_AdministrationNumber(double admin_number, RTi.Util.Time.DateTime prior_adj_date) throws Exception
	public StateMod_AdministrationNumber(double admin_number, DateTime prior_adj_date)
	{
		// The following throws Exception...
		reset(admin_number, prior_adj_date);
	}

	/// <returns> true if the administration numbers are equal, false if not.  The
	/// comparison is made on the integer parts of the administration number to avoid
	/// round-off problems that may have occurred in the floating point data. </returns>
	/// <param name="a"> Administration number to check. </param>
	public bool Equals(StateMod_AdministrationNumber a)
	{
		if ((a._fraction == _fraction) && (a._whole == _whole))
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Finalize before garbage collection. </summary>
	/// <exception cref="Throwable"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~StateMod_AdministrationNumber()
	{
		_appro_date = null;
		_padj_date = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <returns> the full floating-point administration number. </returns>
	public double getAdminNumber()
	{
		return _admin_number;
	}

	/// <returns> the appropriation date. </returns>
	public DateTime getAppropriationDate()
	{
		return _appro_date;
	}

	/// <returns> the fractional part of the administration number as an integer. </returns>
	public int getFraction()
	{
		return _fraction;
	}

	/// <returns> the Julian appropriation date as an integer.  If the fractional part of
	/// the administration number is zero, the whole part of the number is returned.
	/// If the fractional part is non-zero, the fractional part is returned. </returns>
	public int getJulianAppropriationDate()
	{
		if (_fraction != 0)
		{
			return _fraction;
		}
		else
		{
			return _whole;
		}
	}

	/// <returns> the prior adjudication date. </returns>
	public DateTime getPriorAdjudicationDate()
	{
		return _padj_date;
	}

	/// <returns> the whole number part of the administration number as an integer. </returns>
	public int getWhole()
	{
		return _whole;
	}

	/// <returns> true if the current instance is greater than "a".  The
	/// comparison is made on the integer parts of the administration number to avoid
	/// round-off problems that may have occurred in the floating point data. </returns>
	/// <param name="a"> Administration number to compare. </param>
	public bool greaterThan(StateMod_AdministrationNumber a)
	{
		if (_whole > a._whole)
		{
			// No need to check further
			return true;
		}
		else if (_whole == a._whole)
		{
			// Check the remainder...
			if (_fraction > a._fraction)
			{
				// No need to check further
				return true;
			}
		}
		// This instance is equal to or less than "a"...
		return false;
	}

	/// <summary>
	/// Initialize the instance data.
	/// </summary>
	private void initialize()
	{
		_admin_number = 0.0; // 11.5 administration number
		_whole = 0; // Whole part of the administration number
		_fraction = 0; // Fractional part of the administration number
		_appro_date = null; // Appropriation date.
		_padj_date = null; // Prior adjudication date.
	}

	/// <summary>
	/// Initialize the datum for the adiminstration number (December 31, 1849).
	/// This datum is used for subsequent conversions.
	/// </summary>
	private void initializeDatum()
	{
		_admin_number_date_datum = new DateTime(DateTime.DATE_ZERO | DateTime.PRECISION_DAY);
		_admin_number_date_datum.setMonth(12);
		_admin_number_date_datum.setDay(31);
		_admin_number_date_datum.setYear(1849);
		_admin_number_date_datum_days = _admin_number_date_datum.getAbsoluteDay();
	}

	/// <returns> true if the current instance is less than "a".  The
	/// comparison is made on the integer parts of the administration number to avoid
	/// round-off problems that may have occurred in the floating point data. </returns>
	/// <param name="a"> Administration number to check. </param>
	public bool lessThan(StateMod_AdministrationNumber a)
	{
		if (Equals(a))
		{
			return false;
		}
		if (greaterThan(a))
		{
			return false;
		}
		// Must be true...
		return true;
	}

	/// <summary>
	/// Reset the instance data administration number from the appropriation and prior
	/// adjudication dates.  This is called by the constructors. </summary>
	/// <param name="appro_date"> Appropriation date. </param>
	/// <param name="prior_adj_date"> Prior adjudication date (specify as null if no prior
	/// adjudication date applies). </param>
	/// <exception cref="Exception"> if there is an error resetting the information. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void reset(RTi.Util.Time.DateTime appro_date, RTi.Util.Time.DateTime prior_adj_date) throws Exception
	private void reset(DateTime appro_date, DateTime prior_adj_date)
	{
		string message;
		string routine = "HydroBase_AdministrationNumber.reset(DateTime,DateTime)";

		if (appro_date == null)
		{
			message = "Null appropriation date.  Cannot create " +
			"administration number";
			Message.printWarning(2, routine, message);
			throw new Exception(message);
		}

		// Initialize the instance data...

		initialize();
		if (appro_date != null)
		{
			_appro_date = new DateTime(appro_date);
		}
		if (prior_adj_date != null)
		{
			_padj_date = new DateTime(prior_adj_date);
		}

		// Set the datum to 1849-12-31...
		if (_admin_number_date_datum == null)
		{
			initializeDatum();
		}

		if (_padj_date == null)
		{
			// Case "a" in memo...
			// Integer math so no problem with roundoff...
			_whole = _appro_date.getAbsoluteDay() - _admin_number_date_datum_days;
			_fraction = 0;
			// Now save the floating point version...
			_admin_number = (double)_whole;
		}
		else
		{ // Case "a" in memo...
			// Whole number...
			if (_appro_date.greaterThan(_padj_date))
			{
				// Do the integer math first...
				_whole = _appro_date.getAbsoluteDay() - _admin_number_date_datum_days;
				// Now do the floating point math...
				_admin_number = (double)_whole + .00000001;
			}
			else
			{ // Case "b" in the memo...
				// Apparently, a +1 is not needed, although Will Burt's
				// 1987 memo mentions it.
				// Do the integer math first...
				_whole = _padj_date.getAbsoluteDay() - _admin_number_date_datum_days;
				// Now do the floating point math...
				_admin_number = (double)_whole + .00000001;
				// Fraction...
				// These commented lines appear to agree with Will
				// Burt's 1987 memo but do not give an answer that
				// matches the database...
				//fraction = (double)(appro_date.getAbsoluteDay() -
				//       datum_days)/admin_number;
				// This logic gives a value that matches the database...
				// Do integer math...
				_fraction = _appro_date.getAbsoluteDay() - _admin_number_date_datum_days;
				// Now do the floating point math...
				_admin_number += (double)_fraction / 100000.0;
			}
		}
		// As a final step, add a very small number to the floating point value
		// to help make sure that the value is not truncated to .99999
		// something.  This will likely not cause any problems, even if
		// admin numbers are averaged, etc.
		_admin_number += .0000001;
	}

	/// <summary>
	/// Reset the data given the administration number and prior adjudication date.
	/// If the number is an integer, the appropriation date is
	/// the integer part.  Otherwise, the remainder*100000 is the appropriation date. </summary>
	/// <param name="admin_number"> The administration number. </param>
	/// <param name="prior_adj_date"> If supplied it is used to determine the appropriation
	/// date when the administration number is not an integer.  
	/// If not supplied (use null), the determination of prior
	/// adjudication date cannot be computed if the administration number is not
	/// an integer. </param>
	/// <exception cref="Exception"> if there is an error resetting the information. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void reset(double admin_number, RTi.Util.Time.DateTime prior_adj_date) throws Exception
	private void reset(double admin_number, DateTime prior_adj_date)
	{
		if (_admin_number_date_datum == null)
		{
			initializeDatum();
		}

		// Initialize the instance data...

		initialize();

		// Initialize with incoming values...

		_admin_number = admin_number;
		if (prior_adj_date != null)
		{
			_padj_date = new DateTime(prior_adj_date, DateTime.PRECISION_DAY);
		}

		// Divide the admin number into whole number and fraction.  Use
		// an offset to make sure the truncation is ok.

		_whole = (int)_admin_number;
		_fraction = (int)((_admin_number - (double)_whole) * 100000.0 + .001);

		// A check of the DB on 1998-06-30 revealed that of 108066 net amount
		// records, 52109 had null prior adjudication dates.  We need to make
		// a decision to back-calculate the dates in the reverse of the
		// original logic.  Assume that if the fraction is zero that no
		// prior adjudication date was available.
		int appro_date_days = 0;
		int[] date_data;

		if (_fraction == 0)
		{
			// Case "a" in memo...
			// The integer is the appropriation date and the
			// previous adjudication date is indeterminate.
			appro_date_days = _whole + _admin_number_date_datum_days;
			date_data = TimeUtil.getYearMonthDayFromAbsoluteDay(appro_date_days);
			_appro_date = new DateTime(DateTime.DATE_ZERO | DateTime.PRECISION_DAY);
			_appro_date.setYear(date_data[0]);
			_appro_date.setMonth(date_data[1]);
			_appro_date.setDay(date_data[2]);
		}
		else
		{ // Case "a" in memo...
			// Fraction is the appropriation date...
			appro_date_days = _fraction + _admin_number_date_datum_days;
			date_data = TimeUtil.getYearMonthDayFromAbsoluteDay(appro_date_days);
			_appro_date = new DateTime(DateTime.DATE_ZERO | DateTime.PRECISION_DAY);
			_appro_date.setYear(date_data[0]);
			_appro_date.setMonth(date_data[1]);
			_appro_date.setDay(date_data[2]);
			// Whole number is the prior adjudication date...
			int computed_prior_adj_date_days = _whole + _admin_number_date_datum_days;
			date_data = TimeUtil.getYearMonthDayFromAbsoluteDay(computed_prior_adj_date_days);
			_padj_date = new DateTime(DateTime.DATE_ZERO | DateTime.PRECISION_DAY);
			_padj_date.setYear(date_data[0]);
			_padj_date.setMonth(date_data[1]);
			_padj_date.setDay(date_data[2]);
		}
	}

	/// <summary>
	/// Set the appropriation date and recompute the administration number.  The
	/// prior adjudication date is assumed to be null or the previous value set. </summary>
	/// <param name="appro_date"> Appropriation date. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void setAppropriationDate(RTi.Util.Time.DateTime appro_date) throws Exception
	public void setAppropriationDate(DateTime appro_date)
	{ // The following throws an exception...
		reset(appro_date, (DateTime)null);
	}

	/// <returns> a string representation of the administration number created by
	/// concatenating the integer whole and fractional parts of the number.  The
	/// whole number is NOT padded with zeros, but the result
	/// is guaranteed to have 5 digits after the decimal. </returns>
	public override string ToString()
	{
		return new string(StringUtil.formatString(_whole,"%5d") + "." + StringUtil.formatString(_fraction,"%05d"));
	}

	/// <returns> a formatted representation of the administration number. </returns>
	/// <param name="format"> Format to use for output.  Currently FORMAT_VERBOSE is the
	/// only option and is assumed. </param>
	public string ToString(int format)
	{
		if (_padj_date == null)
		{
			string @string = " AdminNum: " + _admin_number + " ApproDate: " + _appro_date.ToString(DateTime.FORMAT_YYYY_MM_DD) + " PadjDate:  null";
			return @string;
		}
		else
		{
			string @string = " AdminNum: " + _admin_number + " ApproDate: " + _appro_date.ToString(DateTime.FORMAT_YYYY_MM_DD) + " PadjDate: " + _padj_date.ToString(DateTime.FORMAT_YYYY_MM_DD);
			return @string;
		}
	}

	}

}