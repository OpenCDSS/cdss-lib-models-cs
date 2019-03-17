using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

// StateMod_WellRight - class to hold well right data

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
// StateMod_WellRight - Derived from StateMod_Data class
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
// 
// 01 Feb 1999	Catherine E.		Created initial version of class.
//		Nutting-Lane, RTi
// 19 Mar 2000	Steven A. Malers, RTi	Change some data members and methods to
//					agree with recent StateMod documentation
//						dcrwel -> dcrdivw
//						rtem -> irtem
// 18 Feb 2001	SAM, RTi		Code review.  Add finalize().  Handle
//					nulls and set unused variables to null.
//					Alphabetize methods.  Add ability to
//					print old style(left-justified)rights
//					but change default to right-justified.
//					Change IO to IOUtil.  Remove unneeded
//					debug messages.
// 02 Mar 2001	SAM, RTi		Ray says to use F16.5 for rights and
//					get rid of the 4x.
// 2001-12-27	SAM, RTi		Update to use new fixedRead()to
//					improve performance.
// 2002-09-19	SAM, RTi		Use isDirty()instead of setDirty()to
//					indicate edits.
//------------------------------------------------------------------------------
// 2003-06-04	J. Thomas Sapienza, RTi	Renamed from SMWellRights to 
//					StateMod_WellRight
// 2003-06-10	JTS, RTi		* Folded dumpWellRightsFile() into
//					  writeWellRightsFile()
//					* Renamed parseWellRightsFile() to
//					  readWellRightsFile()
// 2003-06-23	JTS, RTi		Renamed writeWellRightsFile() to
//					writeStateModFile()
// 2003-06-26	JTS, RTi		Renamed readWellRightsFile() to
//					readStateModFile()
// 2003-08-03	SAM, RTi		Changed isDirty() back to setDirty().
// 2003-08-28	SAM, RTi		Remove use of linked list since
//					StateMod_Well maintains a Vector of
//					rights.
// 2003-10-09	JTS, RTi		* Implemented Cloneable.
//					* Added clone().
//					* Added equals().
//					* Implemented Comparable.
//					* Added compareTo().
// 					* Added equals(Vector, Vector)
// 2004-09-16	SAM, RTi		* Change so that the read and write
//					  methods adjust the file path using the
//					  working directory.
// 2005-01-17	JTS, RTi		* Added createBackup().
//					* Added restoreOriginal().
// 2005-03-10	SAM, RTi		* Clarify the header some for admin #
//					  and switch.
// 2005-03-28	JTS, RTi		Corrected wrong class name in 
//					createBackup().
// 2005-04-18	JTS, RTi		Added writeListFile().
// 2007-04-12	Kurt Tometich, RTi		Added checkComponentData() and
//									getDataHeader() methods for check
//									file and data check support.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
// 2007-05-14	SAM, RTi		Implement the StateMod_Right interface to make
//					it easier to handle rights generically in other code.
// 2007-05-16	SAM, RTi		Add isWellRightFile() to help code like TSTool
//					generically handle reading.  Add optional comment to output.
//------------------------------------------------------------------------------
// EndHeader

namespace DWR.StateMod
{

	using CommandStatus = RTi.Util.IO.CommandStatus;
	using DataSetComponent = RTi.Util.IO.DataSetComponent;
	using IOUtil = RTi.Util.IO.IOUtil;
	using PropList = RTi.Util.IO.PropList;
	using Message = RTi.Util.Message.Message;
	using MessageUtil = RTi.Util.Message.MessageUtil;
	using StringUtil = RTi.Util.String.StringUtil;
	using DateTime = RTi.Util.Time.DateTime;
	using TimeZoneDefaultType = RTi.Util.Time.TimeZoneDefaultType;

	/// <summary>
	/// This class provides stores all the information associated with a well right.
	/// </summary>
	public class StateMod_WellRight : StateMod_Data, ICloneable, IComparable<StateMod_Data>, StateMod_ComponentValidator, StateMod_Right
	{

	/// <summary>
	/// Administration number.
	/// </summary>
	private string _irtem;
	/// <summary>
	/// Decreed amount.
	/// </summary>
	private double _dcrdivw;

	// TODO SAM 2016-05-18 Evaluate how to make this more generic, but is tough due to complexity.
	// The following data are not part of the official StateMod specification but are useful
	// to output in order to understand how well rights are determined.
	// The data are specific to the State of Colorado due to its complex data model.

	/// <summary>
	/// Year for parcels.
	/// </summary>
	private int __parcelYear;
	/// <summary>
	/// Identifier for parcel.
	/// </summary>
	private string __parcelId;
	/// <summary>
	/// Well to parcel matching "class".
	/// </summary>
	private int __parcelMatchClass;

	/// <summary>
	/// Collection type (if a collection), StateMod_Well.COLLECTION_TYPE_*.
	/// </summary>
	private string __collectionType = "";

	/// <summary>
	/// Collection part type (if a collection), StateMod_Well.COLLECTION_PART_TYPE_*.
	/// </summary>
	private string __collectionPartType = "";

	/// <summary>
	/// Collection part ID (if a collection), ID corresponding to the part type.
	/// </summary>
	private string __collectionPartId = "";

	/// <summary>
	/// Collection part ID type (if a collection), StateMod_Well.COLLECTION_WELL_PART_ID_TYPE_*.
	/// </summary>
	private string __collectionPartIdType = "";

	/// <summary>
	/// Well WDID.	
	/// </summary>
	private string __xWDID = "";

	/// <summary>
	/// Well permit receipt.	
	/// </summary>
	private string __xPermitReceipt = "";

	/// <summary>
	/// Well yield GPM
	/// </summary>
	private double __xYieldGPM = Double.NaN;

	/// <summary>
	/// Well yield alternate point/exchange (APEX) GPM
	/// </summary>
	private double __xYieldApexGPM = Double.NaN;

	/// <summary>
	/// Well permit date.
	/// </summary>
	private DateTime __xPermitDate = null;

	/// <summary>
	/// Well permit date as an administration number.
	/// </summary>
	private string __xPermitDateAdminNumber = "";

	/// <summary>
	/// Well right appropriation date.
	/// </summary>
	private DateTime __xApproDate = null;

	/// <summary>
	/// Well right appropriation date as an administration number.
	/// </summary>
	private string __xApproDateAdminNumber = "";

	/// <summary>
	/// Well right use corresponding to three-digit concatenated codes.
	/// </summary>
	private string __xUse = "";

	/// <summary>
	/// Prorated yield based on parcel area
	/// </summary>
	private double __xProratedYield = Double.NaN;

	/// <summary>
	/// Fraction of yield attributed to ditch (based on area of parcel served by ditch)
	/// </summary>
	private double __xDitchFraction = Double.NaN;

	/// <summary>
	/// Fraction of yield (percent_yield in HydroBase)
	/// </summary>
	private double __xFractionYield = Double.NaN;

	/// <summary>
	/// Constructor
	/// </summary>
	public StateMod_WellRight() : base()
	{
		initialize();
	}

	/// <summary>
	/// Clean up before garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize()throws Throwable
	~StateMod_WellRight()
	{
		_irtem = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Clones the data object. </summary>
	/// <returns> a cloned object. </returns>
	public override object clone()
	{
		StateMod_WellRight right = (StateMod_WellRight)base.clone();
		right._irtem = _irtem;
		right._dcrdivw = _dcrdivw;
		right._isClone = true;

		return right;
	}

	/// <summary>
	/// Compares this object to another StateMod_Data object based on the sorted
	/// order from the StateMod_Data variables, and then by rtem, dcrres, iresco,
	/// ityrstr, n2fill and copid, in that order. </summary>
	/// <param name="data"> the object to compare against. </param>
	/// <returns> 0 if they are the same, 1 if this object is greater than the other
	/// object, or -1 if it is less. </returns>
	public virtual int CompareTo(StateMod_Data data)
	{
		int res = base.CompareTo(data);
		if (res != 0)
		{
			return res;
		}

		StateMod_WellRight right = (StateMod_WellRight)data;

		res = _irtem.CompareTo(right._irtem);
		if (res != 0)
		{
			return res;
		}

		if (_dcrdivw < right._dcrdivw)
		{
			return -1;
		}
		else if (_dcrdivw > right._dcrdivw)
		{
			return 1;
		}

		return 0;
	}

	/// <summary>
	/// Creates a backup of the current data object and stores it in _original,
	/// for use in determining if an object was changed inside of a GUI.
	/// </summary>
	public virtual void createBackup()
	{
		_original = (StateMod_WellRight)clone();
		((StateMod_WellRight)_original)._isClone = false;
		_isClone = true;
	}

	/// <summary>
	/// Compare two rights lists and see if they are the same. </summary>
	/// <param name="v1"> the first list of StateMod_WellRight to check.  Cannot be null. </param>
	/// <param name="v2"> the second list of StateMod_WellRight to check.  Cannot be null. </param>
	/// <returns> true if they are the same, false if not. </returns>
	public static bool Equals(IList<StateMod_WellRight> v1, IList<StateMod_WellRight> v2)
	{
		string routine = "StateMod_WellRight.equals(Vector, Vector)";
		StateMod_WellRight r1;
		StateMod_WellRight r2;
		if (v1.Count != v2.Count)
		{
			Message.printStatus(1, routine, "Lists are different sizes");
			return false;
		}
		else
		{
			// sort the Vectors and compare item-by-item.  Any differences
			// and data will need to be saved back into the dataset.
			int size = v1.Count;
			Message.printStatus(2, routine, "Lists are of size: " + size);
			IList<StateMod_WellRight> v1Sort = StateMod_Util.sortStateMod_DataVector(v1);
			IList<StateMod_WellRight> v2Sort = StateMod_Util.sortStateMod_DataVector(v2);
			Message.printStatus(2, routine, "Vectors have been sorted");

			for (int i = 0; i < size; i++)
			{
				r1 = v1Sort[i];
				r2 = v2Sort[i];
				Message.printStatus(2, routine, r1.ToString());
				Message.printStatus(2, routine, r2.ToString());
				Message.printStatus(2, routine, "Element " + i + " comparison: " + r1.CompareTo(r2));
				if (r1.CompareTo(r2) != 0)
				{
					return false;
				}
			}
		}
		return true;
	}

	/// <summary>
	/// Tests to see if two diversion rights are equal.  Strings are compared with case sensitivity. </summary>
	/// <param name="right"> the right to compare. </param>
	/// <returns> true if they are equal, false otherwise. </returns>
	public virtual bool Equals(StateMod_WellRight right)
	{
		 if (!base.Equals(right))
		 {
			 return false;
		 }

		if (right._irtem.Equals(_irtem) && right._dcrdivw == _dcrdivw)
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Return the administration number, as per the generic interface. </summary>
	/// <returns> the administration number, as a String to protect from roundoff. </returns>
	public virtual string getAdministrationNumber()
	{
		return getIrtem();
	}

	/// <summary>
	/// Return the collection part ID. </summary>
	/// <returns> the collection part ID. </returns>
	public virtual string getCollectionPartId()
	{
		return __collectionPartId;
	}

	/// <summary>
	/// Return the collection part ID type. </summary>
	/// <returns> the collection part ID type. </returns>
	public virtual string getCollectionPartIdType()
	{
		return __collectionPartIdType;
	}

	/// <summary>
	/// Return the collection part type. </summary>
	/// <returns> the collection part type. </returns>
	public virtual string getCollectionPartType()
	{
		return __collectionPartType;
	}

	/// <summary>
	/// Return the collection type. </summary>
	/// <returns> the collection type. </returns>
	public virtual string getCollectionType()
	{
		return __collectionType;
	}

	/// <summary>
	/// Returns the table header for StateMod_WellRight data tables. </summary>
	/// <returns> String[] header - Array of header elements. </returns>
	public static string[] getDataHeader()
	{
		return new string[] {"Num", "Well Right ID", "Well Station ID"};
			//"Well Name" };
	}

	/// <returns> the decreed amount(cfs) </returns>
	public virtual double getDcrdivw()
	{
		return _dcrdivw;
	}

	/// <summary>
	/// Return the decree, as per the generic interface. </summary>
	/// <returns> the decree, in the units of the data. </returns>
	public virtual double getDecree()
	{
		return getDcrdivw();
	}

	// TODO SAM 2007-05-15 Need to evaluate whether should be hard-coded.
	/// <summary>
	/// Return the decree units. </summary>
	/// <returns> the decree units. </returns>
	public virtual string getDecreeUnits()
	{
		return "CFS";
	}

	/// <summary>
	/// Return the right identifier, as per the generic interface. </summary>
	/// <returns> the right identifier. </returns>
	public virtual string getIdentifier()
	{
		return getID();
	}

	/// <returns> the administration number </returns>
	public virtual string getIrtem()
	{
		return _irtem;
	}

	/// <summary>
	/// Return the right location identifier, as per the generic interface. </summary>
	/// <returns> the right location identifier (location where right applies). </returns>
	public virtual string getLocationIdentifier()
	{
		return getCgoto();
	}

	/// <returns> the parcel identifier </returns>
	public virtual string getParcelID()
	{
		return __parcelId;
	}

	/// <returns> the parcel match class. </returns>
	public virtual int getParcelMatchClass()
	{
		return __parcelMatchClass;
	}

	/// <returns> the parcel year. </returns>
	public virtual int getParcelYear()
	{
		return __parcelYear;
	}

	/// <summary>
	/// Create summary comments suitable to add to a file header.
	/// </summary>
	private static IList<string> getSummaryCommentList(IList<StateMod_WellRight> rightList)
	{
		IList<string> summaryList = new List<string>();
		int size = rightList.Count;
		IList<int?> parcelMatchClassList = new List<int?>();
		StateMod_WellRight right = null;
		int parcelMatchClass;
		// Determine the unique list of water right classes - ok to end up with one class of -999
		// if class match information is not available.
		for (int i = 0; i < size; i++)
		{
			right = rightList[i];
			parcelMatchClass = right.getParcelMatchClass();
			bool found = false;
			for (int j = 0; j < parcelMatchClassList.Count; j++)
			{
				if (parcelMatchClassList[j].Value == parcelMatchClass)
				{
					found = true;
					break;
				}
			}
			if (!found)
			{
				parcelMatchClassList.Add(new int?(parcelMatchClass));
			}
		}
		// Now summarize information by class
		//int parcelMatchClassListSize = parcelMatchClassList.size();
		//int [] countByClass = new int[parcelMatchClassListSize];
		return summaryList;
	}

	/// <summary>
	/// Return the well permit receipt.	
	/// </summary>
	public virtual string getXWDID()
	{
		return __xWDID;
	}

	/// <summary>
	/// Return the well right use, for example from HydroBase well right use.
	/// </summary>
	public virtual string getXUse()
	{
		return __xUse;
	}


	/// <summary>
	/// Return the well permit receipt.	
	/// </summary>
	public virtual string getXPermitReceipt()
	{
		return __xPermitReceipt;
	}

	/// <summary>
	/// Return the well yield GPM.
	/// </summary>
	public virtual double getXYieldGPM()
	{
		return __xYieldGPM;
	}

	/// <summary>
	/// Return the well yield alternate point/exchange (APEX) GPM
	/// </summary>
	public virtual double getXYieldApexGPM()
	{
		return __xYieldApexGPM;
	}

	/// <summary>
	/// Return the well permit date.
	/// </summary>
	public virtual DateTime getXPermitDate()
	{
		return __xPermitDate;
	}

	/// <summary>
	/// Return the well permit date as an administration number.
	/// </summary>
	public virtual string getXPermitDateAdminNumber()
	{
		return __xPermitDateAdminNumber;
	}

	/// <summary>
	/// Well right appropriation date.
	/// </summary>
	public virtual DateTime getXApproDate()
	{
		return __xApproDate;
	}

	/// <summary>
	/// Well right appropriation date as an administration number.
	/// </summary>
	public virtual string getXApproDateAdminNumber()
	{
		return __xApproDateAdminNumber;
	}

	/// <summary>
	/// Return the fraction of yield attributed to the ditch supply
	/// </summary>
	public virtual double getXDitchFraction()
	{
		return __xDitchFraction;
	}

	/// <summary>
	/// Return the fraction of yield (percent_yield in HydroBase)
	/// </summary>
	public virtual double getXFractionYield()
	{
		return __xFractionYield;
	}

	/// <summary>
	/// Return the prorated yield based on parcel area
	/// </summary>
	public virtual double getXProratedYield()
	{
		return __xProratedYield;
	}

	private void initialize()
	{
		_smdata_type = StateMod_DataSet.COMP_WELL_RIGHTS;
		_irtem = "99999";
		_dcrdivw = 0;
		// Parcel data...
		__parcelId = "";
		__parcelYear = StateMod_Util.MISSING_INT;
		__parcelMatchClass = StateMod_Util.MISSING_INT;
	}

	/// <summary>
	/// Determine whether the right is for an estimated well.  Estimated wells are those that are copies of
	/// real wells, as an estimate of water supply for parcels that are clearly groundwater irrigated but a supply
	/// well is not physically evident in remote sensing work. </summary>
	/// <returns> true if the well right is for an estimated well. </returns>
	public virtual bool isEstimatedWell()
	{
		int parcelMatchClass = getParcelMatchClass();
		if ((parcelMatchClass == 4) || (parcelMatchClass == 9))
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Determine whether a file is a well right file.  Currently true is returned if the file extension is ".wer". </summary>
	/// <param name="filename"> Name of the file being checked. </param>
	/// <returns> true if the file is a StateMod well right file. </returns>
	public static bool isWellRightFile(string filename)
	{
		if (filename.ToUpper().EndsWith(".WER", StringComparison.Ordinal))
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Cancels any changes made to this object within a GUI since createBackup()
	/// was called and sets _original to null.
	/// </summary>
	public override void restoreOriginal()
	{
		StateMod_WellRight right = (StateMod_WellRight)_original;
		base.restoreOriginal();

		_smdata_type = right._smdata_type;
		_irtem = right._irtem;
		_dcrdivw = right._dcrdivw;
		__parcelId = right.__parcelId;
		__parcelMatchClass = right.__parcelMatchClass;
		__parcelYear = right.__parcelYear;
		_isClone = false;
		_original = null;
	}

	/// <summary>
	/// Read the well rights file. </summary>
	/// <param name="filename"> name of file containing well rights </param>
	/// <returns> list of well rights </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.List<StateMod_WellRight> readStateModFile(String filename) throws Exception
	public static IList<StateMod_WellRight> readStateModFile(string filename)
	{
		string routine = "StateMod_WellRight.readStateModFile";
		IList<StateMod_WellRight> theWellRights = new List<StateMod_WellRight>();
		int[] format_0 = new int[] {StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_DOUBLE, StringUtil.TYPE_INTEGER, StringUtil.TYPE_INTEGER, StringUtil.TYPE_INTEGER, StringUtil.TYPE_STRING};
		// Extended, which includes parcel data and more
		// Same as above and additional columns
		int[] format_0Extended = new int[] {StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_DOUBLE, StringUtil.TYPE_INTEGER, StringUtil.TYPE_INTEGER, StringUtil.TYPE_INTEGER, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_DOUBLE, StringUtil.TYPE_DOUBLE, StringUtil.TYPE_DOUBLE, StringUtil.TYPE_DOUBLE, StringUtil.TYPE_DOUBLE, StringUtil.TYPE_DOUBLE, StringUtil.TYPE_DOUBLE};
		int[] format_0w = new int[] {12, 24, 12, 16, 8, 8, 5, 5, 7};
		// Extended, which includes parcel data and more
		// Same as above and additional columns
		// x column adds 1 to each column
		int[] format_0wExtended = new int[] {12, 24, 12, 16, 8, 8, 5, 5, 13, 15, 9, 21, 9, 9, 11, 12, 31, 9, 11, 12, 9, 9, 9, 9, 9, 9, 9};
		string iline = null;
		IList<object> v = new List<object>(10);
		StreamReader @in = null;
		StateMod_WellRight aRight = null;

		Message.printStatus(1, routine, "Reading well rights file: " + filename);

		try
		{
			@in = new StreamReader(IOUtil.getPathUsingWorkingDir(filename));
			bool formatSet = false;
			bool fileHasExtendedComments = false; // Does file have the extended comments (parcel/well/permit/right)?
			int lineCount = 0;
			while (!string.ReferenceEquals((iline = @in.ReadLine()), null))
			{
				// Check for extended comments
				++lineCount;
				if (iline.StartsWith("#", StringComparison.Ordinal) || iline.Trim().Length == 0)
				{
					// Check to see if extended comments which will be if the line starts with #>----- and is longer than 100
					if (iline.StartsWith("#>-----", StringComparison.Ordinal) && (iline.Length > 100))
					{
						fileHasExtendedComments = true;
					}
					continue;
				}
				if (!formatSet && fileHasExtendedComments)
				{
					format_0 = format_0Extended;
					format_0w = format_0wExtended;
					formatSet = true;
					Message.printStatus(1, routine, "Detected extended comments in well rights file.");
				}

				aRight = new StateMod_WellRight();

				if (Message.isDebugOn)
				{
					Message.printDebug(50, routine, "iline: " + iline);
				}
				StringUtil.fixedRead(iline, format_0, format_0w, v);
				aRight.setID(((string)v[0]).Trim());
				aRight.setName(((string)v[1]).Trim());
				aRight.setCgoto(((string)v[2]).Trim());
				aRight.setIrtem(((string)v[3]).Trim());
				aRight.setDcrdivw((double?)v[4]);
				aRight.setSwitch((int?)v[5]);
				/* If comments used
				if ( v.size() > 6 ) {
					// Save the comment at the end of the line
					comment = (String)v.get(6);
					if ( comment != null ) {
						aRight.setComment ( comment.trim() );
					}
				}
				*/
				// Evaluate handling parcel data... 
				aRight.setParcelYear((int?)v[6]);
				aRight.setParcelMatchClass((int?)v[7]);
				aRight.setParcelID(((string)v[8]).Trim());
				if (fileHasExtendedComments)
				{
					try
					{
						aRight.setCollectionType((string)v[9]);
						aRight.setCollectionPartType((string)v[10]);
						aRight.setCollectionPartId((string)v[11]);
						aRight.setCollectionPartIdType((string)v[12]);
						aRight.setXWDID((string)v[13]);
						try
						{
							DateTime dt = DateTime.parse((string)v[14]);
							aRight.setXApproDate(dt.getDate(TimeZoneDefaultType.LOCAL));
						}
						catch (Exception)
						{
						}
						aRight.setXApproDateAdminNumber((string)v[15]);
						aRight.setXUse((string)v[16]);
						aRight.setXPermitReceipt((string)v[17]);
						try
						{
							DateTime dt = DateTime.parse((string)v[18]);
							aRight.setXPermitDate(dt.getDate(TimeZoneDefaultType.LOCAL));
						}
						catch (Exception)
						{
						}
						aRight.setXPermitDateAdminNumber((string)v[19]);
						aRight.setXYieldGPM((double?)v[20].Value.Value);
						aRight.setXYieldApexGPM((double?)v[21].Value.Value);
						aRight.setXFractionYield((double?)v[22].Value.Value);
						aRight.setXDitchFraction((double?)v[23].Value.Value);
						aRight.setXProratedYield((double?)v[24].Value.Value);
					}
					catch (Exception e)
					{
						Message.printWarning(3,routine,"Error reading line " + lineCount + " (" + e + "): " + iline);
					}
				}
				// If extended comments
				theWellRights.Add(aRight);
			}
		}
		catch (Exception e)
		{
			Message.printWarning(3, routine, e);
			throw e;
		}
		finally
		{
			if (@in != null)
			{
				@in.Close();
			}
			@in = null;
		}
		return theWellRights;
	}

	/// <summary>
	/// Set the collection part ID. </summary>
	/// <param name="collectionPartId"> collection part ID. </param>
	public virtual void setCollectionPartId(string collectionPartId)
	{
		if (string.ReferenceEquals(collectionPartId, null))
		{
			return;
		}
		if (!collectionPartId.Equals(__collectionPartId))
		{
			__collectionPartId = collectionPartId.Trim();
			setDirty(true);
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_WELL_RIGHTS, true);
			}
		}
	}

	/// <summary>
	/// Set the collection part ID type. </summary>
	/// <param name="collectionPartId"> collection part ID type. </param>
	public virtual void setCollectionPartIdType(string collectionPartIdType)
	{
		if (string.ReferenceEquals(collectionPartIdType, null))
		{
			return;
		}
		if (!collectionPartIdType.Equals(__collectionPartIdType))
		{
			__collectionPartIdType = collectionPartIdType.Trim();
			setDirty(true);
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_WELL_RIGHTS, true);
			}
		}
	}

	/// <summary>
	/// Set the collection part type. </summary>
	/// <param name="collectionPartType"> collection part type. </param>
	public virtual void setCollectionPartType(string collectionPartType)
	{
		if (string.ReferenceEquals(collectionPartType, null))
		{
			return;
		}
		if (!collectionPartType.Equals(__collectionPartType))
		{
			__collectionPartType = collectionPartType.Trim();
			setDirty(true);
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_WELL_RIGHTS, true);
			}
		}
	}

	/// <summary>
	/// Set the collection type. </summary>
	/// <param name="collectionType"> collection part type. </param>
	public virtual void setCollectionType(string collectionType)
	{
		if (string.ReferenceEquals(collectionType, null))
		{
			return;
		}
		if (!collectionType.Equals(__collectionType))
		{
			__collectionType = collectionType.Trim();
			setDirty(true);
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_WELL_RIGHTS, true);
			}
		}
	}

	/// <summary>
	/// Set the decreed amount(cfs) </summary>
	/// <param name="dcrdivw"> decreed amount for this right </param>
	public virtual void setDcrdivw(double dcrdivw)
	{
		if (dcrdivw != _dcrdivw)
		{
			_dcrdivw = dcrdivw;
			setDirty(true);
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_WELL_RIGHTS, true);
			}
		}
	}

	/// <summary>
	/// Set the decreed amount(cfs) </summary>
	/// <param name="dcrdivw"> decreed amount for this right </param>
	public virtual void setDcrdivw(double? dcrdivw)
	{
		if (dcrdivw == null)
		{
			return;
		}
		setDcrdivw(dcrdivw.Value);
	}

	/// <summary>
	/// Set the decreed amount(cfs) </summary>
	/// <param name="dcrdivw"> decreed amount for this right </param>
	public virtual void setDcrdivw(string dcrdivw)
	{
		if (string.ReferenceEquals(dcrdivw, null))
		{
			return;
		}
		setDcrdivw(StringUtil.atod(dcrdivw.Trim()));
	}

	/// <summary>
	/// Set the decree, as per the generic interface. </summary>
	/// <param name="decree"> decree, in the units of the data. </param>
	public virtual void setDecree(double decree)
	{
		setDcrdivw(decree);
	}

	/// <summary>
	/// Set the administration number </summary>
	/// <param name="irtem"> admin number of right </param>
	public virtual void setIrtem(string irtem)
	{
		if (string.ReferenceEquals(irtem, null))
		{
			return;
		}
		if (!irtem.Equals(_irtem))
		{
			_irtem = irtem.Trim();
			setDirty(true);
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_WELL_RIGHTS, true);
			}
		}
	}

	/// <summary>
	/// Set the parcel identifier. </summary>
	/// <param name="parcel_id"> Parcel identifier. </param>
	public virtual void setParcelID(string parcel_id)
	{
		if (string.ReferenceEquals(parcel_id, null))
		{
			return;
		}
		if (!parcel_id.Equals(__parcelId))
		{
			__parcelId = parcel_id.Trim();
			setDirty(true);
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_WELL_RIGHTS, true);
			}
		}
	}

	/// <summary>
	/// Set the parcel match class. </summary>
	/// <param name="parcel_match_class"> Parcel to well match class. </param>
	public virtual void setParcelMatchClass(int? parcel_match_class)
	{
		if (parcel_match_class == null)
		{
			return;
		}
		setParcelMatchClass(parcel_match_class.Value);
	}

	/// <summary>
	/// Set the parcel match class, used to match wells to parcels. </summary>
	/// <param name="parcel_match_class"> Parcel match class. </param>
	public virtual void setParcelMatchClass(int parcel_match_class)
	{
		if (parcel_match_class != __parcelMatchClass)
		{
			__parcelMatchClass = parcel_match_class;
			setDirty(true);
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_WELL_RIGHTS, true);
			}
		}
	}

	/// <summary>
	/// Set the parcel year. </summary>
	/// <param name="parcel_year"> Parcel year. </param>
	public virtual void setParcelYear(int? parcel_year)
	{
		if (parcel_year == null)
		{
			return;
		}
		setParcelYear(parcel_year.Value);
	}

	/// <summary>
	/// Set the parcel year, used to match wells to parcels. </summary>
	/// <param name="parcel_year"> Parcel year. </param>
	public virtual void setParcelYear(int parcel_year)
	{
		if (parcel_year != __parcelYear)
		{
			__parcelYear = parcel_year;
			setDirty(true);
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_WELL_RIGHTS, true);
			}
		}
	}

	/// <summary>
	/// Set the well WDID.	
	/// </summary>
	public virtual void setXWDID(string xWDID)
	{
		__xWDID = xWDID;
	}

	/// <summary>
	/// Set the well right use.	
	/// </summary>
	public virtual void setXUse(string xUse)
	{
		__xUse = xUse;
	}

	/// <summary>
	/// Set the well permit receipt.	
	/// </summary>
	public virtual void setXPermitReceipt(string xPermitReceipt)
	{
		__xPermitReceipt = xPermitReceipt;
	}

	/// <summary>
	/// Set the well yield GPM.
	/// </summary>
	public virtual void setXYieldGPM(double xYieldGPM)
	{
		__xYieldGPM = xYieldGPM;
	}

	/// <summary>
	/// Set the well yield alternate point/exchange (APEX) GPM
	/// </summary>
	public virtual void setXYieldApexGPM(double xYieldApexGPM)
	{
		__xYieldApexGPM = xYieldApexGPM;
	}

	/// <summary>
	/// Set the well permit date.
	/// </summary>
	public virtual void setXPermitDate(DateTime xPermitDate)
	{
		__xPermitDate = xPermitDate;
	}

	/// <summary>
	/// Set the well permit date as an administration number.
	/// </summary>
	public virtual void setXPermitDateAdminNumber(string xPermitDateAdminNumber)
	{
		__xPermitDateAdminNumber = xPermitDateAdminNumber;
	}

	/// <summary>
	/// Set the well right appropriation date.
	/// </summary>
	public virtual void setXApproDate(DateTime xApproDate)
	{
		__xApproDate = xApproDate;
	}

	/// <summary>
	/// Set the well right appropriation date as an administration number.
	/// </summary>
	public virtual void setXApproDateAdminNumber(string xApproDateAdminNumber)
	{
		__xApproDateAdminNumber = xApproDateAdminNumber;
	}

	/// <summary>
	/// Set the prorated yield based on parcel area
	/// </summary>
	public virtual void setXProratedYield(double xProratedYield)
	{
		__xProratedYield = xProratedYield;
	}

	/// <summary>
	/// Set the fraction of yield attributed to parcel served by ditch
	/// </summary>
	public virtual void setXDitchFraction(double xDitchFraction)
	{
		__xDitchFraction = xDitchFraction;
	}

	/// <summary>
	/// Set the fraction of yield (percent_yield in HydroBase)
	/// </summary>
	public virtual void setXFractionYield(double xFractionYield)
	{
		__xFractionYield = xFractionYield;
	}

	/// <summary>
	/// Performs specific data checks for StateMod Well Rights. </summary>
	/// <param name="dataset"> StateMod dataset. </param>
	/// <returns> validation results. </returns>
	public virtual StateMod_ComponentValidation validateComponent(StateMod_DataSet dataset)
	{
		StateMod_ComponentValidation validation = new StateMod_ComponentValidation();
		string id = getID();
		string name = getName();
		string cgoto = getCgoto();
		string irtem = getIrtem();
		double dcrdivw = getDcrdivw();
		// Make sure that basic information is not empty
		if (StateMod_Util.isMissing(id))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Well right identifier is blank.", "Specify a well right identifier."));
		}
		if (StateMod_Util.isMissing(name))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Well right \"" + id + "\" name is blank.", "Specify a well right name to clarify data."));
		}
		if (StateMod_Util.isMissing(cgoto))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Well right \"" + id + "\" well station ID is blank.", "Specify a well station to associate the well right."));
		}
		else
		{
			// Verify that the well station is in the data set, if the network is available
			DataSetComponent comp2 = dataset.getComponentForComponentType(StateMod_DataSet.COMP_WELL_STATIONS);
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<StateMod_Well> wesList = (java.util.List<StateMod_Well>)comp2.getData();
			IList<StateMod_Well> wesList = (IList<StateMod_Well>)comp2.getData();
			if ((wesList != null) && (wesList.Count > 0))
			{
				if (StateMod_Util.IndexOf(wesList, cgoto) < 0)
				{
					validation.add(new StateMod_ComponentValidationProblem(this,"Well right \"" + id + "\" associated well (" + cgoto + ") is not found in the list of well stations.", "Specify a valid well station ID to associate with the well right."));
				}
			}
		}
		if (StateMod_Util.isMissing(irtem))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Well right \"" + id + "\" administration number is blank.", "Specify an administration number NNNNN.NNNNN."));
		}
		else if (!StringUtil.isDouble(irtem))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Well right \"" + id + "\" administration number (" + irtem + ") is invalid.", "Specify an administration number NNNNN.NNNNN."));
		}
		else
		{
			double irtemd = double.Parse(irtem);
			if (irtemd < 0)
			{
				validation.add(new StateMod_ComponentValidationProblem(this,"Well right \"" + id + "\" administration number (" + irtem + ") is invalid.", "Specify an administration number NNNNN.NNNNN."));
			}
		}
		if (!(dcrdivw >= 0.0))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Well right \"" + id + "\" decree (" + StringUtil.formatString(dcrdivw,"%.2f") + ") is invalid.", "Specify the decree as a number >= 0."));
		}

		return validation;
	}

	/// <summary>
	/// FIXME SAM 2009-06-03 Evaluate how to call from main validate method
	/// Check the well rights.
	/// </summary>
	private void validateComponent2(IList<StateMod_WellRight> werList, IList<StateMod_Well> wesList, string idpattern_Java, int warningCount, int warningLevel, string commandTag, CommandStatus status)
	{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		string routine = this.GetType().FullName + ".checkWellRights";
		string message;

		StateMod_Well wes_i = null;

		StateMod_Parcel parcel = null; // Parcel associated with a well station
		int wes_parcel_count = 0; // Parcel count for well station
		double wes_parcel_area = 0.0; // Area of parcels for well station
		int wes_well_parcel_count = 0; // Parcel (with wells) count for well station.
		double wes_well_parcel_area = 0.0; // Area of parcels with wells for well station.
		IList<StateMod_Parcel> parcel_Vector; // List of parcels for well station.
		int count = 0; // Count of well stations with potential problems.
		string id_i = null;
		System.Collections.IList rightList = null;
		int welListSize = wesList.Count;
		for (int i = 0; i < welListSize; i++)
		{
			wes_i = (StateMod_Well)wesList[i];
			if (wes_i == null)
			{
				continue;
			}
			id_i = wes_i.getID();
			rightList = StateMod_Util.getRightsForStation(id_i, werList);
			// TODO SAM 2007-01-02 Evaluate how to put this code in a separate method and share between rights and stations.
			if ((rightList == null) || (rightList.Count == 0))
			{
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
				// Parcels associated with the well station
				parcel_Vector = wes_i.getParcels();
				if (parcel_Vector != null)
				{
					// Number of parcels associated with the well station
					wes_parcel_count = parcel_Vector.Count;
					for (int j = 0; j < wes_parcel_count; j++)
					{
						parcel = parcel_Vector[j];
						// Increment parcel area associated with the well station
						if (parcel.getArea() > 0.0)
						{
							wes_parcel_area += parcel.getArea();
						}
						if (parcel.getWellCount() > 0)
						{
							// Count and area of parcels that have wells
							wes_well_parcel_count += parcel.getWellCount();
							wes_well_parcel_area += parcel.getArea();
						}
					}
				}
				message = "The following well station has no water rights (no irrigated parcels served by " +
					"wells) well station ID=" + id_i +
					", well name=" + wes_i.getName() +
					", collection type=" + wes_i.getCollectionType() +
					", parcels for well station=" + wes_parcel_count +
					", parcel area for well station (acres)=" + StringUtil.formatString(wes_parcel_area,"%.3f") +
					", count of wells on parcels=" + wes_well_parcel_count +
					", area of parcels with wells (acres)=" + StringUtil.formatString(wes_well_parcel_area,"%.3f");
				Message.printWarning(warningLevel, MessageUtil.formatMessageTag(commandTag, ++warningCount), routine, message);
				/*
				status.addToLog ( CommandPhaseType.RUN,
					new WellRightValidation(CommandStatusType.WARNING,
						message, "Data may be OK if the station has no wells.  " +
							"Parcel count and area in the following table are available " +
							"only if well rights are read from HydroBase." ) );
							*/
			}
		}

		// Since well rights are determined from parcel data, print a list of
		// well rights that do not have associated yield (decree)...

		int werListSize = werList.Count;
		int pos = 0; // Position in well station vector
		string wes_name = null; // Well station name
		string wes_id = null; // Well station ID
		string wer_id = null; // Well right identifier
		double decree = 0.0;
		StateMod_WellRight wer_i = null;
		int matchCount = 0;
		for (int i = 0; i < werListSize; i++)
		{
			wer_i = (StateMod_WellRight)werList[i];
			wer_id = wer_i.getID();
			if (!wer_id.matches(idpattern_Java))
			{
				continue;
			}
			++matchCount;
			// Format to two digits to match StateMod output...
			decree = StringUtil.atod(StringUtil.formatString(wer_i.getDcrdivw(),"%.2f"));
			if (decree <= 0.0)
			{
				// Find associated well station for output to print ID and name...
				pos = StateMod_Util.IndexOf(wesList,wer_i.getCgoto());
				wes_i = null;
				if (pos >= 0)
				{
					wes_i = (StateMod_Well)wesList[pos];
				}
				wes_name = "";
				if (wes_i != null)
				{
					wes_id = wes_i.getID();
					wes_name = wes_i.getName();
				}
				// Format suitable for output in a list that can be copied to a spreadsheet or table.
				message = "Well right \"" + wer_id + "\" (well station " + wes_id + " \"" + wes_name +
					"\") has decree (" + decree + ") <= 0.";
				Message.printWarning(warningLevel, MessageUtil.formatMessageTag(commandTag, ++warningCount), routine, message);
				/*
				status.addToLog ( CommandPhaseType.RUN,
					new WellRightValidation(CommandStatusType.FAILURE,
						message, "Verify that parcels are available for wells and check that well " +
							"right at 2-digit precision is > 0." ) );
							*/
			}
		}
		// Return values
		int[] retVals = new int[2];
		retVals[0] = matchCount;
		retVals[1] = warningCount;
		//return retVals;
	}

	/// <summary>
	/// Write the well rights file.  The comments from the previous
	/// rights file are transferred into the next one.  Also, a history is maintained
	/// and printed in the header for the file.  Additional header comments can be added
	/// through the new_comments parameter.
	/// Comments for each data item will be written if provided - these are being used
	/// for evaluation during development but are not a part of the standard file. </summary>
	/// <param name="infile"> name of file to retrieve previous comments and history from </param>
	/// <param name="outfile"> name of output file to write. </param>
	/// <param name="theRights"> list of rights to write. </param>
	/// <param name="newComments"> additional comments to print to the comment section </param>
	/// <param name="writeProps"> Properties to control the rights.  Currently only WriteDataComments=True/False
	/// and WriteExtendedDataComments are recognized </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeStateModFile(String infile, String outfile, java.util.List<StateMod_WellRight> theRights, java.util.List<String> newComments, RTi.Util.IO.PropList writeProps) throws Exception
	public static void writeStateModFile(string infile, string outfile, IList<StateMod_WellRight> theRights, IList<string> newComments, PropList writeProps)
	{
		PrintWriter @out = null;
		string routine = "StateMod_WellRight.writeStateModFile";

		if (writeProps == null)
		{
			// Create properties to check
			writeProps = new PropList("");
		}
		string WriteDataComments = writeProps.getValue("WriteDataComments");
		bool writeDataComments = false; // Default
		if ((!string.ReferenceEquals(WriteDataComments, null)) && WriteDataComments.Equals("True", StringComparison.OrdinalIgnoreCase))
		{
			writeDataComments = true;
		}
		string WriteExtendedDataComments = writeProps.getValue("WriteExtendedDataComments");
		bool writeExtendedDataComments = false; // Default
		if ((!string.ReferenceEquals(WriteExtendedDataComments, null)) && WriteExtendedDataComments.Equals("True", StringComparison.OrdinalIgnoreCase))
		{
			writeExtendedDataComments = true;
		}

		if (string.ReferenceEquals(outfile, null))
		{
			string msg = "Unable to write to null filename";
			Message.printWarning(3, routine, msg);
			throw new Exception(msg);
		}

		Message.printStatus(2, routine, "Writing well rights to: " + outfile);

		IList<string> commentIndicators = new List<string>(1);
		commentIndicators.Add("#");
		IList<string> ignoredCommentIndicators = new List<string>(1);
		ignoredCommentIndicators.Add("#>");
		try
		{
			@out = IOUtil.processFileHeaders(IOUtil.getPathUsingWorkingDir(infile), IOUtil.getPathUsingWorkingDir(outfile), newComments, commentIndicators, ignoredCommentIndicators, 0);

			string iline = null;
			string cmnt = "#>";
			string format_0 = "%-12.12s%-24.24s%-12.12s%16.16s%8.2F%8d";
			StateMod_WellRight right = null;
			IList<object> v = new List<object>(6);

			@out.println(cmnt);
			@out.println(cmnt + "***************************************************");
			@out.println(cmnt + "  StateMod Well Right File (" + theRights.Count + " rights)");
			@out.println(cmnt);
			IList<string> fileSummary = getSummaryCommentList(theRights);
			for (int i = 0; i < fileSummary.Count; i++)
			{
				@out.println(cmnt + fileSummary[i]);
			}
			@out.println(cmnt);
			string format_add = "";
			if (writeDataComments)
			{
				format_add = ", 1x, i4," // Parcel year
					+ " 1x, i4," // Parcel match class
					+ " 1x, a6"; // Parcel ID (note 6 characters for legacy behavior)
			}
			else if (writeExtendedDataComments)
			{
				// Longer parcel ID to allow WD + ID, plus other information that shows the
				// original source of the right
				format_add = ", 1x, i4," // Parcel year
					+ " 1x, i4," // Parcel match class
					+ " 1x, a12," // Parcel ID (note 12 characters for new behavior)
					+ " 1x, a14," // Collection type
					+ " 1x, a8," // Part type
					+ " 1x, a20," // Part ID
					+ " 1x, a8," // Part ID type
					+ " 1x, a8," // WDID (if available)
					+ " 1x, a10," // Appropriation date YYYY-MM-DD
					+ " 1x, a11," // Appropriation date as administration number
					+ " 1x, a30," // Well right use (30 characters was maximum length in HydroBase as of 2016-10-03)
					+ " 1x, a8," // Receipt (if available)
					+ " 1x, a10," // Receipt date
					+ " 1x, a11," // Receipt date as administration number
					+ " 1x, f8.2," // Yield (GPM)
					+ " 1x, f8.2," // Yield (CFS)
					+ " 1x, f8.2," // APEX (GPM)
					+ " 1x, f8.2," // APEX (CFS)
					+ " 1x, f8.2," // Well fraction
					+ " 1x, f8.2," // Ditch fraction
					+ " 1x, f8.2"; // Yield prorated (GPM)
			}
			@out.println(cmnt + "  Format:  (a12, a24, a12, f16.5, f8.2, i8" + format_add + ")");
			@out.println(cmnt);
			@out.println(cmnt + "     ID        cidvi:  Well right ID ");
			@out.println(cmnt + "     Name     cnamew:  Well right name");
			@out.println(cmnt + "     Struct    cgoto:  Well Structure ID associated with this right");
			@out.println(cmnt + "     Admin #   irtem:  Administration number");
			@out.println(cmnt + "                       (priority, small is senior)");
			@out.println(cmnt + "     Decree  dcrdivw:  Well right (cfs)");
			@out.println(cmnt + "     On/Off  idvrsww:  Switch 0 = off, 1 = on");
			@out.println(cmnt + "                       YYYY = on for years >= YYYY");
			@out.println(cmnt + "                       -YYYY = off for years > YYYY");
			string header1_add = "";
			string header2_add = "";
			string header3_add = "";
			if (writeDataComments || writeExtendedDataComments)
			{
				if (writeExtendedDataComments)
				{
					// Wider ParcelID since more recent includes WD in ID
					header1_add = "                       ";
					header2_add = " PYr--Cls----ParcelID  ";
					header3_add = "xb--exb--exb----------e";
				}
				else
				{
					header1_add = "                 ";
					header2_add = " PYr--Cls--PID   ";
					header3_add = "xb--exb--exb----e";
				}
				@out.println(cmnt);
				@out.println(cmnt + "The following data are NOT part of the standard StateMod file and StateMod will ignore.");
				@out.println(cmnt + "                 Pyr:  Parcel year used for parcel/well matching (-999 if data applies to full period)");
				@out.println(cmnt + "                 Cls:  Indicates how well was matched to parcel (see CDSS documentation).");
				@out.println(cmnt + "            ParcelID:  Parcel ID for year.");
			}
			if (writeExtendedDataComments)
			{
				header1_add = header1_add + "                                                                                                                                                             Ditch    Well   Prorated";
				header2_add = header2_add + " CollectionType-PartType--------PartID--------IDType     WDID----ApproDate-ApproDateAN---------------Use               Receipt-PermitDate-PermtDateAN-YieldGPM-YieldCFS-APEXGPM--APEXCFS  Fraction-Fraction-YieldGPM";
				header3_add = header3_add + "xb------------exb------exb------------------exb------exb------exb--------exb---------exb----------------------------exb------exb--------exb---------exb------exb------exb------exb------exb------exb------exb------e";
				@out.println(cmnt);
				@out.println(cmnt + "The following are output if extended data comments are requested and are useful for understanding parcel/well/ditch/right/permit.");
				@out.println(cmnt + "      CollectionType:  Aggregate, System, etc.");
				@out.println(cmnt + "            PartType:  Parcel ID for year.");
				@out.println(cmnt + "              PartID:  Part ID for collection (original source of water right).");
				@out.println(cmnt + "          PartIDType:  Part ID for PartID, if a well (WDID or Receipt).");
				@out.println(cmnt + "                WDID:  Well structure WDID (if available) - part IDType controls initial data lookup.");
				@out.println(cmnt + "           ApproDate:  Well right appropriation date (if available).");
				@out.println(cmnt + "         ApproDateAN:  Well right appropriation date as administration number.");
				@out.println(cmnt + "                 Use:  Well right decreed use.");
				@out.println(cmnt + "             Receipt:  Well permit receipt (if available) - part IDType controls initial data lookup.");
				@out.println(cmnt + "          PermitDate:  Permit date (if available).");
				@out.println(cmnt + "         PermtDateAN:  Permit date as administration number.");
				@out.println(cmnt + "            YieldGPM:  Well yield (GPM).");
				@out.println(cmnt + "            YieldCFS:  Well yield (CFS).");
				@out.println(cmnt + "             APEXGPM:  Alternate point/exchange yield, GPM, added to yield if requested during processing.");
				@out.println(cmnt + "             APEXCFS:  Alternate point/exchange yield, CFS.");
				@out.println(cmnt + "      Ditch Fraction:  Fraction of well yield to be used for this right (based on fraction of parcel served by ditch), 1.0 if no ditch.");
				@out.println(cmnt + "       Well Fraction:  Fraction of well yield to be used for this right (based on number of wells serving parcel).");
				@out.println(cmnt + "   Prorated YieldGPM:  Prorated yield (GPM, Yield*WellFraction*DitchFraction), may contain APEX depending on processing, equivalent to decree (CFS).");
			}
			@out.println(cmnt);
			@out.println(cmnt + "                                                                              " + header1_add);
			@out.println(cmnt + "   ID               Name             Struct          Admin #   Decree  On/Off " + header2_add);
			@out.println(cmnt + "---------eb----------------------eb----------eb--------------eb------eb------e" + header3_add);
			@out.println(cmnt);
			@out.println(cmnt + "EndHeader");
			@out.println(cmnt);

			int num = 0;
			if (theRights != null)
			{
				num = theRights.Count;
			}

			string comment = null; // Comment for data item
			for (int i = 0; i < num; i++)
			{
				right = theRights[i];
				if (right == null)
				{
					continue;
				}

				v.Clear();
				v.Add(right.getID());
				v.Add(right.getName());
				v.Add(right.getCgoto());
				v.Add(right.getIrtem());
				v.Add(new double?(right.getDcrdivw()));
				v.Add(new int?(right.getSwitch()));
				iline = StringUtil.formatString(v, format_0);
				if (writeDataComments || writeExtendedDataComments)
				{
					comment = right.getComment(); // TODO SAM 2016-05-18 Figure out how this is used
					string parcelYear = "-999"; // Need to use this because well merging expects -999
					string parcelMatchClass = "    ";
					string parcelID = "      ";
					if (writeExtendedDataComments)
					{
						parcelID = "            ";
					}
					if (right.getParcelYear() > 0)
					{
						parcelYear = StringUtil.formatString(right.getParcelYear(),"%4d");
					}
					if (right.getParcelMatchClass() >= 0)
					{
						// TODO SAM 2016-05-17 Apparently the class does not get set to -999 or zero, etc. and can be -2147483648
						parcelMatchClass = StringUtil.formatString(right.getParcelMatchClass(),"%4d");
					}
					if ((!string.ReferenceEquals(right.getParcelID(), null)) && !right.getParcelID().Equals("-999"))
					{
						if (writeExtendedDataComments)
						{
							parcelID = StringUtil.formatString(right.getParcelID(),"%12.12s");
						}
						else
						{
							parcelID = StringUtil.formatString(right.getParcelID(),"%6.6s");
						}
					}
					comment = parcelYear + " " + parcelMatchClass + " " + parcelID;
					iline = iline + " " + comment;
				}
				if (writeExtendedDataComments)
				{
					// Also add the additional properties
					string collectionType = right.getCollectionType();
					if (string.ReferenceEquals(collectionType, null))
					{
						collectionType = "";
					}
					collectionType = StringUtil.formatString(collectionType,"%-14.14s");
					string partType = right.getCollectionPartType();
					if (string.ReferenceEquals(partType, null))
					{
						partType = "";
					}
					partType = StringUtil.formatString(partType,"%-8.8s");
					string partId = right.getCollectionPartId();
					if (string.ReferenceEquals(partId, null))
					{
						partId = "";
					}
					partId = StringUtil.formatString(partId,"%-20.20s");
					string partIdType = right.getCollectionPartIdType();
					if (string.ReferenceEquals(partIdType, null))
					{
						partIdType = "        ";
					}
					else
					{
						partIdType = StringUtil.formatString(partIdType,"%-8.8s");
					}
					string wdid = right.getXWDID();
					if (string.ReferenceEquals(wdid, null))
					{
						wdid = "        ";
					}
					else
					{
						wdid = StringUtil.formatString(wdid,"%-8.8s");
					}
					DateTime approDate = right.getXApproDate();
					string approDateString = "";
					string approDateAdminNumberString = "";
					if (approDate == null)
					{
						approDateString = "          ";
						approDateAdminNumberString = "           ";
					}
					else
					{
						DateTime dt = new DateTime(approDate);
						approDateString = dt.ToString();
						approDateAdminNumberString = StringUtil.formatString(right.getXApproDateAdminNumber(), "%-11.11s");
					}
					string use = right.getXUse();
					if (string.ReferenceEquals(use, null))
					{
						use = "";
					}
					else
					{
						use = StringUtil.formatString(use,"%-30.30s");
					}
					string receipt = right.getXPermitReceipt();
					if (string.ReferenceEquals(receipt, null))
					{
						receipt = "        ";
					}
					else
					{
						receipt = StringUtil.formatString(receipt,"%-8.8s");
					}
					DateTime permitDate = right.getXPermitDate();
					string permitDateString = "";
					string permitDateAdminNumberString = "";
					if (permitDate == null)
					{
						permitDateString = "          ";
						permitDateAdminNumberString = "           ";
					}
					else
					{
						DateTime dt = new DateTime(permitDate);
						permitDateString = dt.ToString();
						permitDateAdminNumberString = StringUtil.formatString(right.getXPermitDateAdminNumber(), "%-11.11s");
					}
					double yieldGPM = right.getXYieldGPM();
					string yieldGPMString = "";
					string yieldCFSString = "";
					if (double.IsNaN(yieldGPM))
					{
						yieldGPMString = "        ";
						yieldCFSString = "        ";
					}
					else
					{
						yieldGPMString = string.Format("{0,8:F2}", yieldGPM);
						yieldCFSString = string.Format("{0,8:F2}", yieldGPM * .002228);
					}
					double apexGPM = right.getXYieldApexGPM();
					string apexGPMString = "";
					string apexCFSString = "";
					if (double.IsNaN(apexGPM))
					{
						apexGPMString = "        ";
						apexCFSString = "        ";
					}
					else
					{
						apexGPMString = string.Format("{0,8:F2}", apexGPM);
						apexCFSString = string.Format("{0,8:F2}", apexGPM * .002228);
					}
					double ditchFraction = right.getXDitchFraction();
					string ditchFractionString = "";
					if (double.IsNaN(ditchFraction))
					{
						ditchFractionString = "        ";
					}
					else
					{
						ditchFractionString = string.Format("{0,8:F2}", ditchFraction);
					}
					double wellFraction = right.getXFractionYield();
					string wellFractionString = "";
					if (double.IsNaN(wellFraction))
					{
						wellFractionString = "        ";
					}
					else
					{
						wellFractionString = string.Format("{0,8:F2}", wellFraction);
					}
					//double prorated = right.getXProratedYield();
					double proratedYield = right.getDcrdivw();
					string proratedYieldString = "";
					if (double.IsNaN(proratedYield))
					{
						proratedYieldString = "        ";
					}
					else
					{
						proratedYieldString = string.Format("{0,8:F2}", proratedYield / .002228); // Convert decree as CFS to GPM
					}
					iline = iline + " " + collectionType + " " + partType + " " + partId + " " + partIdType + " "
						+ wdid + " " + approDateString + " " + approDateAdminNumberString + " " + use + " " + receipt + " " + permitDateString + " " + permitDateAdminNumberString + " " + yieldGPMString + " " + yieldCFSString + " "
						+ apexGPMString + " " + apexCFSString + " " + ditchFractionString + " "
						+ wellFractionString + " " + proratedYieldString;
				}
				// Print the line to the file
				@out.println(iline);
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
				@out.close();
			}
			@out = null;
		}
	}

	/// <summary>
	/// Writes a list of StateMod_WellRight objects to a list file.  A header is 
	/// printed to the top of the file, containing the commands used to generate the 
	/// file.  Any strings in the body of the file that contain the field delimiter will be wrapped in "...". </summary>
	/// <param name="filename"> the name of the file to which the data will be written. </param>
	/// <param name="delimiter"> the delimiter to use for separating field values. </param>
	/// <param name="update"> whether to update an existing file, retaining the current 
	/// header (true) or to create a new file with a new header. </param>
	/// <param name="data"> the list of objects to write. </param>
	/// <param name="newComments"> the list of new comments to write to the header. </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeListFile(String filename, String delimiter, boolean update, java.util.List<StateMod_WellRight> data, java.util.List<String> newComments) throws Exception
	public static void writeListFile(string filename, string delimiter, bool update, IList<StateMod_WellRight> data, IList<string> newComments)
	{
		int size = 0;
		if (data != null)
		{
			size = data.Count;
		}

		IList<string> fields = new List<string>();
		fields.Add("ID");
		fields.Add("Name");
		fields.Add("StationID");
		fields.Add("AdministrationNumber");
		fields.Add("Decree");
		fields.Add("OnOff");
		int fieldCount = fields.Count;

		string[] names = new string[fieldCount];
		string[] formats = new string[fieldCount];
		int comp = StateMod_DataSet.COMP_WELL_RIGHTS;
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

		int j = 0;
		PrintWriter @out = null;
		StateMod_WellRight right = null;
		IList<string> commentIndicators = new List<string>(1);
		commentIndicators.Add("#");
		IList<string> ignoredCommentIndicators = new List<string>(1);
		ignoredCommentIndicators.Add("#>");
		string[] line = new string[fieldCount];
		StringBuilder buffer = new StringBuilder();

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
			newComments2.Insert(1,"StateMod well rights as a delimited list file.");
			newComments2.Insert(2,"");
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
				right = data[i];

				line[0] = StringUtil.formatString(right.getID(), formats[0]).Trim();
				line[1] = StringUtil.formatString(right.getName(), formats[1]).Trim();
				line[2] = StringUtil.formatString(right.getCgoto(), formats[2]).Trim();
				line[3] = StringUtil.formatString(right.getIrtem(), formats[3]).Trim();
				line[4] = StringUtil.formatString(right.getDcrdivw(), formats[4]).Trim();
				line[5] = StringUtil.formatString(right.getSwitch(), formats[5]).Trim();

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