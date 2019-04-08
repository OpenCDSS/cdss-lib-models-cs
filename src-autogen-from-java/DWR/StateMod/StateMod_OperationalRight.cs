using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

// StateMod_OperationalRight - class for operational rights

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
// StateMod_OperationalRight - class derived from StateMod_Data.  Contains 
//	information read from the operational rights file.
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
// 
// 07 Jan 1998	Catherine E.		Created initial version of class
//		Nutting-Lane, RTi
// 23 Feb 1998	CEN, RTi		Added Write routines
// 21 Dec 1998	CEN, RTi		Added throws IOException to read/write
//					routines.
// 23 Nov 1999	CEN, RTi		Added comments for each 
//					StateMod_OperationalRight
//					instantiation.
// 07 Mar 2000	CEN, RTi		Modified read/write methods logic to
//					work off dumx variable to determine
//					additional lines for a rule rather than
//					using rule type.  Also, added rule types
//					15 and 16.
// 19 Feb 2001	Steven A. Malers, RTi	Code review.  Clean up javadoc.  Handle
//					nulls and set unused variables to null.
//					Add finalize.  Alphabetize methods.
//					Change IO to IOUtil.  Change some status
//					messages to debug and remove some debug
//					messages.
// 2001-12-27	SAM, RTi		Update to use new fixedRead() to
//					improve performance (are not using full
//					optimization here).
// 2002-09-19	SAM, RTi		Use isDirty() instead of setDirty() to
//					indicate edits.
//------------------------------------------------------------------------------
// 2003-06-04	J. Thomas Sapienza, RTi	Renamed from SMOprits to 
//					StateMod_OperationalRight
// 2003-06-10	JTS, RTi		* Folded dumpOperationalRightsFile()
//					  into writeOperationalRightsFile()
//					* Renamed parseOperationalRightsFile()
//					  into readOperationalRightsFile()
// 2003-06-23	JTS, RTi		Renamed writeOperationalRightsFile()
//					to writeStateModFile()
// 2003-06-26	JTS, RTi		Renamed readOperationalRightsFile()
//					to readStateModFile()
// 2003-07-15	JTS, RTi		Changed to use new dataset design.
// 2003-08-03	SAM, RTi		Changed isDirty() back to setDirty().
// 2003-08-25	SAM, RTi		Changed public oprightsOptions to
//					TYPES, consistent with other programming
//					standards.
// 2003-08-28	SAM, RTi		* Call setDirty() for each object and
//					  the data component.
//					* Clean up parameter names and javadoc.
// 2003-09-15	SAM, RTi		* Update to handle all new operations,
//					  up through number 23.
//					* Change some data types from numbers to
//					  String because of changes in how they
//					  are used in the FORTRAM (must be doing
//					  internal type casting in FORTRAN).
//					* Change StringTokenizer to
//					  breakStringList() - easier to check
//					  count of tokens.
// 2003-09-22	J. Thomas Sapienza, RTi	* Added hasImonsw().
//					* Added setupImonsw().
//					* Added getQdebt().
//					* Added getQdebtx().
//					* Added getSjmina().
//					* Added getSjrela().
// 2003-10-19	SAM, RTi		Change description of types 2 and 3 as
//					per Ray Bennett 2003-10-18 email.
// 2004-07-14	JTS, RTi		* Added acceptChanges().
//					* Added changed().
//					* Added clone().
//					* Added compareTo().
//					* Added createBackup().
//					* Added restoreOriginal().
//					* Now implements Cloneable.
//					* Now implements Comparable.
//					* Clone status is checked via _isClone
//					  when the component is marked as dirty.
// 2004-08-25	JTS, RTi		Revised the clone() code because of
//					null pointers being thrown if the data
//					arrays were null.
// 2004-08-26	JTS, RTi		The array values (_intern and _imonsw)
//					were not being handled in 
//					restoreOriginal() or compareTo(), so
//					they were added.
// 2006-08-16	SAM, RTi		* Add names of operational rights 24 to
//					  35.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
//------------------------------------------------------------------------------
// EndHeader

namespace DWR.StateMod
{

	using IOUtil = RTi.Util.IO.IOUtil;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// StateMod operational right (operating rule) data.
	/// Not all operational right types may be handled by the software.  See the the
	/// StateMod_OperationalRight_Metadata.getFullEditingSupported() for information.
	/// </summary>
	public class StateMod_OperationalRight : StateMod_Data, ICloneable, IComparable<StateMod_Data>
	{

	/// <summary>
	/// Administration number.
	/// </summary>
	private string _rtem;
	/// <summary>
	/// Typically the number of intervening structures or the number of monthly
	/// switches, depending on the right number
	/// </summary>
	private int _dumx;
	/// <summary>
	/// Typically the destination ID.
	/// </summary>
	private string _ciopde;
	/// <summary>
	/// Typically the destination account.
	/// </summary>
	private string _iopdes;
	/// <summary>
	/// Typically the supply ID.
	/// </summary>
	private string _ciopso1;
	/// <summary>
	/// Typically the supply account.
	/// </summary>
	private string _iopsou1;
	/// <summary>
	/// Definition varies by right type.
	/// </summary>
	private string _ciopso2;
	/// <summary>
	/// Definition varies by right type.
	/// </summary>
	private string _iopsou2;
	/// <summary>
	/// Definition varies by right type.
	/// </summary>
	private string _ciopso3;
	/// <summary>
	/// Definition varies by right type.
	/// </summary>
	private string _iopsou3;
	/// <summary>
	/// Used with type 17, 18.
	/// </summary>
	private string _ciopso4;
	/// <summary>
	/// Used with type 17, 18.
	/// </summary>
	private string _iopsou4;
	/// <summary>
	/// Used with type 17, 18.
	/// </summary>
	private string _ciopso5;
	/// <summary>
	/// Used with type 17, 18.
	/// </summary>
	private string _iopsou5;
	/// <summary>
	/// Operational right type > 1.
	/// </summary>
	private int __ityopr;
	/// <summary>
	/// Intervening structure IDs (up to 10 in StateMod doc but no limit here) - used by some rights, null if not used.
	/// </summary>
	private string[] _intern = null;
	/// <summary>
	/// Intervening structure carrier loss, %.
	/// </summary>
	private double[] __oprLossC = null;
	/// <summary>
	/// Intervening structure types, used when have loss.
	/// </summary>
	private string[] __internT = null;
	/// <summary>
	/// Monthly switch, for some rights, null if not used (months in order of data set control file).
	/// </summary>
	private int[] _imonsw = null;
	/// <summary>
	/// Comments provided by user - # comments before each right.  An empty (non-null) list is guaranteed.
	/// TODO SAM 2010-12-14 Evaluate whether this can be in StateMod_Data or will it bloat memory.
	/// </summary>
	private IList<string> __commentsBeforeData = new List<string>();
	/// <summary>
	/// Used with operational right 17, 18.
	/// </summary>
	private double _qdebt;
	/// <summary>
	/// used with operational right 17, 18.
	/// </summary>
	private double _qdebtx;
	/// <summary>
	/// Used with operational right 20.
	/// </summary>
	private double _sjmina;
	/// <summary>
	/// Used with operational right 20.
	/// </summary>
	private double _sjrela;
	/// <summary>
	/// Plan ID.
	/// </summary>
	private string __creuse;
	/// <summary>
	/// Diversion type.
	/// </summary>
	private string __cdivtyp;
	/// <summary>
	/// Conveyance loss.
	/// </summary>
	private double __oprLoss;
	/// <summary>
	/// Miscellaneous limits.
	/// </summary>
	private double __oprLimit;
	/// <summary>
	/// Beginning year of operation.
	/// </summary>
	private int __ioBeg;
	/// <summary>
	/// Ending year of operation.
	/// </summary>
	private int __ioEnd;

	/// <summary>
	/// Monthly efficiencies (12 values in order of data set control file),
	/// only used by some rights like 24.
	/// </summary>
	private double[] __oprEff;

	/// <summary>
	/// Monthly operational limits (12 values in order of data set control file plus annual at end),
	/// only used by some rights like 47.
	/// </summary>
	private double[] __oprMax;

	/// <summary>
	/// TODO SAM 2011-01-29 Phase out when operational rights as documented have been fully tested in code.
	/// A list of strings indicating errors at read.  This is checked to determine if the right should be edited
	/// as text (yes if any errors) or detailed (no if any errors).
	/// </summary>
	private IList<string> __readErrorList = new List<string>();

	/// <summary>
	/// The operational right as a list of strings (lines after right comments and prior to the comments for
	/// the next right.
	/// </summary>
	private IList<string> __rightStringsList = new List<string>();

	/// <summary>
	/// Used with monthly and annual limitation.
	/// </summary>
	private string __cx = "";

	// cidvri = ID is in base class identifier
	// nameo = Name is in base class name
	// ioprsw = on/off is in base class switch

	/// <summary>
	/// The metadata that corresponds to the operational right type, or null if the right type is not recognized.
	/// The metadata is set when the operational right type is set.
	/// </summary>
	private StateMod_OperationalRight_Metadata __metadata = null;

	/// <summary>
	/// Constructor.
	/// </summary>
	public StateMod_OperationalRight() : base()
	{
		initialize();
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
		return true;
	}

	/// <summary>
	/// Clones the data object. </summary>
	/// <returns> a cloned object. </returns>
	public override object clone()
	{
		StateMod_OperationalRight op = (StateMod_OperationalRight)base.clone();
		op._isClone = true;
		// Handle more complex types that will not automatically be cloned
		if (_intern != null)
		{
			op._intern = (string[])_intern.Clone();
		}
		else
		{
			op._intern = null;
		}

		if (__oprLossC != null)
		{
			op.__oprLossC = (double[])__oprLossC.Clone();
		}
		else
		{
			op.__oprLossC = null;
		}

		if (__internT != null)
		{
			op.__internT = (string[])__internT.Clone();
		}
		else
		{
			op.__internT = null;
		}

		if (_imonsw != null)
		{
			op._imonsw = (int[])_imonsw.Clone();
		}
		else
		{
			_imonsw = null;
		}

		if (__oprMax != null)
		{
			op.__oprMax = (double[])__oprMax.Clone();
		}
		else
		{
			__oprMax = null;
		}

		if (__oprEff != null)
		{
			op.__oprEff = (double[])__oprEff.Clone();
		}
		else
		{
			__oprEff = null;
		}

		op.__commentsBeforeData = new List<string>();
		foreach (string comment in __commentsBeforeData)
		{
			op.__commentsBeforeData.Add(comment);
		}
		op.__rightStringsList = new List<string>();
		foreach (string @string in __rightStringsList)
		{
			op.__rightStringsList.Add(@string);
		}

		return op;
	}

	/// <summary>
	/// Compares this object to another StateMod_OperationalRight object.  Because there is so much variability
	/// in the operational rights data, two instances that are of the same type should have the same non-used
	/// extra data.  Therefore, only significant information should be compared if the types are the same. </summary>
	/// <param name="data"> the object to compare against. </param>
	/// <returns> 0 if they are the same, 1 if this object is greater than the other object, or -1 if it is less. </returns>
	public virtual int CompareTo(StateMod_Data data)
	{
		int res; // result of compareTo calls
		StateMod_OperationalRight op = (StateMod_OperationalRight)data;
		if (!__metadata.getFullEditingSupported(_dataset))
		{
			// Only text is used so compare the text.
			if (__rightStringsList.Count < op.__rightStringsList.Count)
			{
				if (Message.isDebugOn)
				{
					Message.printDebug(1,"compareTo","op rights text size are different");
				}
				return -1;
			}
			else if (__rightStringsList.Count > op.__rightStringsList.Count)
			{
				if (Message.isDebugOn)
				{
					Message.printDebug(1,"compareTo","op rights text size are different");
				}
				return 1;
			}
			// Lists are the same size...
			for (int i = 0; i < __rightStringsList.Count; i++)
			{
				res = __rightStringsList[i].CompareTo(op.__rightStringsList[i]);
				if (res != 0)
				{
					if (Message.isDebugOn)
					{
						Message.printDebug(1,"compareTo","op rights text string is different");
					}
					return res;
				}
			}
		}
		// Else, compare the data members...
		res = base.CompareTo(data);
		if (res != 0)
		{
			if (Message.isDebugOn)
			{
				Message.printDebug(1,"compareTo","op rights parent data are different");
			}
			return res;
		}

		res = _rtem.CompareTo(op._rtem);
		if (res != 0)
		{
			if (Message.isDebugOn)
			{
				Message.printDebug(1,"compareTo","op rights rtem are different");
			}
			return res;
		}

		if (_dumx < op._dumx)
		{
			if (Message.isDebugOn)
			{
				Message.printDebug(1,"compareTo","op rights dumx are different");
			}
			return -1;
		}
		else if (_dumx > op._dumx)
		{
			if (Message.isDebugOn)
			{
				Message.printDebug(1,"compareTo","op rights dumx are different");
			}
			return 1;
		}

		res = _ciopde.CompareTo(op._ciopde);
		if (res != 0)
		{
			return res;
		}

		res = _iopdes.CompareTo(op._iopdes);
		if (res != 0)
		{
			return res;
		}

		res = _ciopso1.CompareTo(op._ciopso1);
		if (res != 0)
		{
			return res;
		}

		res = _iopsou1.CompareTo(op._iopsou1);
		if (res != 0)
		{
			return res;
		}

		res = _ciopso2.CompareTo(op._ciopso2);
		if (res != 0)
		{
			return res;
		}

		res = _iopsou2.CompareTo(op._iopsou2);
		if (res != 0)
		{
			return res;
		}

		res = _ciopso3.CompareTo(op._ciopso3);
		if (res != 0)
		{
			return res;
		}

		res = _iopsou3.CompareTo(op._iopsou3);
		if (res != 0)
		{
			return res;
		}

		res = _ciopso4.CompareTo(op._ciopso4);
		if (res != 0)
		{
			return res;
		}

		res = _iopsou4.CompareTo(op._iopsou4);
		if (res != 0)
		{
			return res;
		}

		res = _ciopso5.CompareTo(op._ciopso5);
		if (res != 0)
		{
			return res;
		}

		res = _iopsou5.CompareTo(op._iopsou5);
		if (res != 0)
		{
			return res;
		}

		if (_intern == null && op._intern == null)
		{
			// ok
		}
		else if (_intern == null)
		{
			return -1;
		}
		else if (op._intern == null)
		{
			return 1;
		}
		else
		{
			int size1 = _intern.Length;
			int size2 = op._intern.Length;
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
				res = _intern[i].CompareTo(op._intern[i]);
				if (res != 0)
				{
					return res;
				}
			}
		}

		if (_imonsw == null && op._imonsw == null)
		{
			// ok
		}
		else if (_imonsw == null)
		{
			return -1;
		}
		else if (op._imonsw == null)
		{
			return 1;
		}
		else
		{
			int size1 = _imonsw.Length;
			int size2 = op._imonsw.Length;
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
				if (_imonsw[i] < op._imonsw[i])
				{
					return -1;
				}
				else if (_imonsw[i] > op._imonsw[i])
				{
					return 1;
				}
			}
		}

		if (__ityopr < op.__ityopr)
		{
			return -1;
		}
		else if (__ityopr > op.__ityopr)
		{
			return 1;
		}

		if (_qdebt < op._qdebt)
		{
			return -1;
		}
		else if (_qdebt > op._qdebt)
		{
			return 1;
		}

		if (_qdebtx < op._qdebtx)
		{
			return -1;
		}
		else if (_qdebtx > op._qdebtx)
		{
			return 1;
		}

		if (_sjmina < op._sjmina)
		{
			return -1;
		}
		else if (_sjmina > op._sjmina)
		{
			return 1;
		}

		if (_sjrela < op._sjrela)
		{
			return -1;
		}
		else if (_sjrela > op._sjrela)
		{
			return 1;
		}

		res = __creuse.CompareTo(op.__creuse);
		if (res != 0)
		{
			return res;
		}

		res = __cdivtyp.CompareTo(op.__cdivtyp);
		if (res != 0)
		{
			return res;
		}

		if (__oprLoss < op.__oprLoss)
		{
			return -1;
		}
		else if (__oprLoss > op.__oprLoss)
		{
			return 1;
		}

		if (__oprLimit < op.__oprLimit)
		{
			return -1;
		}
		else if (__oprLimit > op.__oprLimit)
		{
			return 1;
		}

		if (__ioBeg < op.__ioBeg)
		{
			return -1;
		}
		else if (__ioBeg > op.__ioBeg)
		{
			return 1;
		}

		if (__ioEnd < op.__ioEnd)
		{
			return -1;
		}
		else if (__ioEnd > op.__ioEnd)
		{
			return 1;
		}

		// Some rules use this (otherwise will be equally missing)...

		res = __cx.CompareTo(op.__cx);
		if (res != 0)
		{
			return res;
		}

		// Some rules use this (otherwise will be equally missing)...

		if (__oprMax == null && op.__oprMax == null)
		{
			// ok
		}
		else if (__oprMax == null)
		{
			return -1;
		}
		else if (op.__oprMax == null)
		{
			return 1;
		}
		else
		{
			int size1 = __oprMax.Length;
			int size2 = op.__oprMax.Length;
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
				if (__oprMax[i] < op.__oprMax[i])
				{
					return -1;
				}
				else if (__oprMax[i] > op.__oprMax[i])
				{
					return 1;
				}
			}
		}

		// Some rules use this (otherwise will be equally missing)...

		if (__oprEff == null && op.__oprEff == null)
		{
			// ok
		}
		else if (__oprEff == null)
		{
			return -1;
		}
		else if (op.__oprEff == null)
		{
			return 1;
		}
		else
		{
			int size1 = __oprEff.Length;
			int size2 = op.__oprEff.Length;
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
				if (__oprEff[i] < op.__oprEff[i])
				{
					return -1;
				}
				else if (__oprEff[i] > op.__oprEff[i])
				{
					return 1;
				}
			}
		}

		// Some rules use this (otherwise will be equally missing)...

		if (__internT == null && op.__internT == null)
		{
			// ok
		}
		else if (__internT == null)
		{
			return -1;
		}
		else if (op.__internT == null)
		{
			return 1;
		}
		else
		{
			int size1 = __internT.Length;
			int size2 = op.__internT.Length;
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
				res = __internT[i].CompareTo(op.__internT[i]);
				if (res != 0)
				{
					return res;
				}
			}
		}

		// Some rules use this (otherwise will be equally missing)...

		if (__oprLossC == null && op.__oprLossC == null)
		{
			// ok
		}
		else if (__oprLossC == null)
		{
			return -1;
		}
		else if (op.__oprLossC == null)
		{
			return 1;
		}
		else
		{
			int size1 = __oprLossC.Length;
			int size2 = op.__oprLossC.Length;
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
				if (__oprLossC[i] < op.__oprLossC[i])
				{
					return -1;
				}
				else if (__oprLossC[i] > op.__oprLossC[i])
				{
					return 1;
				}
			}
		}

		if (__commentsBeforeData.Count < op.__commentsBeforeData.Count)
		{
			return -1;
		}
		else if (__commentsBeforeData.Count > op.__commentsBeforeData.Count)
		{
			return 1;
		}
		// Lists are the same size...
		int compareResult;
		for (int i = 0; i < __commentsBeforeData.Count; i++)
		{
			compareResult = __commentsBeforeData[i].CompareTo(op.__commentsBeforeData[i]);
			if (compareResult != 0)
			{
				return compareResult;
			}
		}

		// This is only relevant if text representation is used...
		if (__commentsBeforeData.Count < op.__commentsBeforeData.Count)
		{
			return -1;
		}
		else if (__commentsBeforeData.Count > op.__commentsBeforeData.Count)
		{
			return 1;
		}
		// Lists are the same size...
		for (int i = 0; i < __commentsBeforeData.Count; i++)
		{
			res = __commentsBeforeData[i].CompareTo(op.__commentsBeforeData[i]);
			if (res != 0)
			{
				return res;
			}
		}

		// Instances are the same

		return 0;
	}

	/// <summary>
	/// Creates a copy of the object for later use in checking to see if it was changed in a GUI.
	/// </summary>
	public virtual void createBackup()
	{
		_original = (StateMod_OperationalRight)clone();
		((StateMod_OperationalRight)_original)._isClone = false;
		_isClone = true;
	}

	// TODO SAM 2010-12-11 Should the default if not specified be version 2?
	/// <summary>
	/// Determine the StateMod operational right file version.  Version 1 is old and Version 2 was introduced
	/// in Version 12.0.  The version is determined by checking for the string "FileFormatVersion 2" in a comment
	/// line. </summary>
	/// <returns> 1 for the old version and 2 for version 2. </returns>
	private static int determineFileVersion(string filename)
	{
		StreamReader @in = null;
		int version = 1;
		try
		{
			@in = new StreamReader(IOUtil.getInputStream(filename));
			try
			{
				// Read lines and check for string that indicates a version 2 file.
				string @string = null;
				while (!string.ReferenceEquals((@string = @in.ReadLine()), null))
				{
					@string = @string.ToUpper();
					if (@string.StartsWith("#", StringComparison.Ordinal) && (@string.IndexOf("FILEFORMATVERSION 2", StringComparison.Ordinal) > 0))
					{
						version = 2;
						break;
					}
				}
			}
			finally
			{
				if (@in != null)
				{
					@in.Close();
				}
			}
			return version;
		}
		catch (Exception e)
		{
			Message.printWarning(3, "", e);
			return version;
		}
	}

	/// <summary>
	/// Clean up before garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~StateMod_OperationalRight()
	{
		_ciopde = null;
		_iopdes = null;
		_ciopso1 = null;
		_iopsou1 = null;
		_ciopso2 = null;
		_iopsou2 = null;
		_ciopso3 = null;
		_iopsou3 = null;
		_ciopso4 = null;
		_iopsou4 = null;
		_ciopso5 = null;
		_iopsou5 = null;
		_imonsw = null;
		_intern = null;
		__commentsBeforeData = null;
		__creuse = null;
		__cdivtyp = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Return the cdivtyp (diversion type).
	/// </summary>
	public virtual string getCdivtyp()
	{
		return __cdivtyp;
	}

	/// <summary>
	/// Return the ciopde.
	/// </summary>
	public virtual string getCiopde()
	{
		return _ciopde;
	}

	/// <summary>
	/// Return the ciopso1.
	/// </summary>
	public virtual string getCiopso1()
	{
		return _ciopso1;
	}

	/// <summary>
	/// Return the ciopso2.
	/// </summary>
	public virtual string getCiopso2()
	{
		return _ciopso2;
	}

	/// <summary>
	/// Return the ciopso3.
	/// </summary>
	public virtual string getCiopso3()
	{
		return _ciopso3;
	}

	/// <summary>
	/// Return the ciopso4.
	/// </summary>
	public virtual string getCiopso4()
	{
		return _ciopso4;
	}

	/// <summary>
	/// Return the ciopso5.
	/// </summary>
	public virtual string getCiopso5()
	{
		return _ciopso5;
	}

	/// <summary>
	/// Return the creuse (plan identifier).
	/// </summary>
	public virtual string getCreuse()
	{
		return __creuse;
	}

	/// <summary>
	/// Return the comments from the input file that immediate precede the data. </summary>
	/// <returns> the comments from the input file that immediate precede the data. </returns>
	public virtual IList<string> getCommentsBeforeData()
	{
		return __commentsBeforeData;
	}

	/// <summary>
	/// Return the cx (used with monthly and annual limitation).
	/// </summary>
	public virtual string getCx()
	{
		return __cx;
	}

	/// <summary>
	/// Retrieve dumx.
	/// </summary>
	public virtual int getDumx()
	{
		return _dumx;
	}

	/// <summary>
	/// Return the array of monthly switch.
	/// </summary>
	public virtual int [] getImonsw()
	{
		return _imonsw;
	}

	/// <summary>
	/// Return a monthly switch at an index. </summary>
	/// <param name="index"> month to get switch for (0-11), where the index is a position, not
	/// a month (actual month is controlled by the year type for the data set). </param>
	public virtual int getImonsw(int index)
	{
		return _imonsw[index];
	}

	/// <summary>
	/// Return the array of "intern".
	/// </summary>
	public virtual string [] getIntern()
	{
		return _intern;
	}

	/// <summary>
	/// Return the "intern" at an index, or blank if not set.
	/// </summary>
	public virtual string getIntern(int index)
	{
		if ((index < 0) || (index >= _intern.Length))
		{
			return "";
		}
		else
		{
			return _intern[index];
		}
	}

	/// <summary>
	/// Return the array of "internT".
	/// </summary>
	public virtual string [] getInternT()
	{
		return __internT;
	}

	/// <summary>
	/// Return the "internT" at an index, or blank if not set.
	/// </summary>
	public virtual string getInternT(int index)
	{
		if ((index < 0) || (index >= __internT.Length))
		{
			return "";
		}
		else
		{
			return __internT[index];
		}
	}

	/// <summary>
	/// Return the intervening structure identifiers, guaranteed to be non-null but may be empty.
	/// </summary>
	public virtual IList<string> getInterveningStructureIDs()
	{
		IList<string> structureIDList = new List<string>();
		if (__metadata == null)
		{
			return structureIDList;
		}
		else if (__metadata.getRightTypeUsesInterveningStructuresWithoutLoss())
		{
			string[] intern = getIntern();
			if ((intern == null) || (intern.Length == 0))
			{
				return structureIDList;
			}
			else
			{
				return Arrays.asList(intern);
			}
		}
		else
		{
			return structureIDList;
		}
	}

	/// <summary>
	/// Retrieve the ioBeg.
	/// </summary>
	public virtual int getIoBeg()
	{
		return __ioBeg;
	}

	/// <summary>
	/// Retrieve the ioEnd.
	/// </summary>
	public virtual int getIoEnd()
	{
		return __ioEnd;
	}

	/// <summary>
	/// Get the interns as a list. </summary>
	/// <returns> the intervening structure identifiers or an empty list. </returns>
	public virtual IList<string> getInternsVector()
	{
		IList<string> v = new List<string>();
		if (_intern != null)
		{
			for (int i = 0; i < _intern.Length; i++)
			{
				v.Add(getIntern(i));
			}
		}
		return v;
	}

	/// <summary>
	/// Return the iopdes.
	/// </summary>
	public virtual string getIopdes()
	{
		return _iopdes;
	}

	/// <summary>
	/// Return the iopsou.
	/// </summary>
	public virtual string getIopsou1()
	{
		return _iopsou1;
	}

	/// <summary>
	/// Return the iopsou2.
	/// </summary>
	public virtual string getIopsou2()
	{
		return _iopsou2;
	}

	/// <summary>
	/// Return the iopsou3.
	/// </summary>
	public virtual string getIopsou3()
	{
		return _iopsou3;
	}

	/// <summary>
	/// Return the iopsou4.
	/// </summary>
	public virtual string getIopsou4()
	{
		return _iopsou4;
	}

	/// <summary>
	/// Return the iopsou5.
	/// </summary>
	public virtual string getIopsou5()
	{
		return _iopsou5;
	}

	/// <summary>
	/// Retrieve the ityopr.
	/// </summary>
	public virtual int getItyopr()
	{
		return __ityopr;
	}

	/// <summary>
	/// Get the metadata for the right or null if the right type is not recognized.
	/// </summary>
	public virtual StateMod_OperationalRight_Metadata getMetadata()
	{
		return __metadata;
	}

	/// <summary>
	/// Return OprLimit. </summary>
	/// <returns> OprLimit. </returns>
	public virtual double getOprLimit()
	{
		return __oprLimit;
	}

	/// <summary>
	/// Return OprLoss. </summary>
	/// <returns> OprLoss. </returns>
	public virtual double getOprLoss()
	{
		return __oprLoss;
	}

	/// <summary>
	/// Return the array of "oprLossC".
	/// </summary>
	public virtual double [] getOprLossC()
	{
		return __oprLossC;
	}

	/// <summary>
	/// Return the "oprLossC" at an index, or missing if not set.
	/// </summary>
	public virtual double getOprLossC(int index)
	{
		if ((index < 0) || (index >= __oprLossC.Length))
		{
			return StateMod_Util.MISSING_DOUBLE;
		}
		else
		{
			return __oprLossC[index];
		}
	}

	/// <summary>
	/// Return the array of monthly efficiency values.
	/// </summary>
	public virtual double [] getOprEff()
	{
		return __oprEff;
	}

	/// <summary>
	/// Return a monthly efficiency at an index. </summary>
	/// <param name="index"> month to get efficiency for (0-11), where the index is a position, not
	/// a month (actual month is controlled by the year type for the data set). </param>
	public virtual double getOprEff(int index)
	{
		return __oprEff[index];
	}

	/// <summary>
	/// Return the array of monthly max limits.
	/// </summary>
	public virtual double [] getOprMax()
	{
		return __oprMax;
	}

	/// <summary>
	/// Return a monthly switch at an index. </summary>
	/// <param name="index"> month to get switch for (0-11), where the index is a position, not
	/// a month (actual month is controlled by the year type for the data set). </param>
	public virtual double getOprMax(int index)
	{
		return __oprMax[index];
	}

	/// <summary>
	/// Return the list of strings that contain read error messages, when the first line of the right is
	/// inconsistent with the documentation.  This may indicate that the documentation is wrong or the code is
	/// wrong, but may be that the data file is wrong and needs to be cleaner.  For example, a hand-edited
	/// operational right may be inaccurate and StateMod allows.  This right can be treated as text until the
	/// error in documentation/code/data are corrected. </summary>
	/// <returns> the list of strings that contain read error messages, when the first line of the right is
	/// inconsistent with the documentation. </returns>
	public virtual IList<string> getReadErrors()
	{
		return __readErrorList;
	}

	/// <returns> the list of strings that contain the operating rule data when the right is not understood. </returns>
	public virtual IList<string> getRightStrings()
	{
		return __rightStringsList;
	}

	/// <summary>
	/// Return rtem. </summary>
	/// <returns> rtem. </returns>
	public virtual string getRtem()
	{
		return _rtem;
	}

	public virtual double getQdebt()
	{
		return _qdebt;
	}

	public virtual double getQdebtx()
	{
		return _qdebtx;
	}

	public virtual double getSjrela()
	{
		return _sjrela;
	}

	public virtual double getSjmina()
	{
		return _sjmina;
	}

	/// <summary>
	/// Initializes member variables.
	/// </summary>
	private void initialize()
	{
		_smdata_type = StateMod_DataSet.COMP_OPERATION_RIGHTS;
		_rtem = "";
		_dumx = StateMod_Util.MISSING_INT;
		_ciopde = "";
		_iopdes = "";
		_ciopso1 = "";
		_iopsou1 = "";
		_ciopso2 = "";
		_iopsou2 = "";
		_ciopso3 = "";
		_iopsou3 = "";
		_ciopso4 = "";
		_iopsou4 = "";
		_ciopso5 = "";
		_iopsou5 = "";
		__ityopr = StateMod_Util.MISSING_INT;
		_imonsw = new int[12];
		for (int i = 0; i < 12; i++)
		{
			_imonsw[i] = StateMod_Util.MISSING_INT;
		}
		_intern = new string[10]; // Maximum defined by StateMod
		for (int i = 0; i < 10; i++)
		{
			_intern[i] = "";
		}
		__commentsBeforeData = new List<string>();
		_qdebt = StateMod_Util.MISSING_DOUBLE;
		_qdebtx = StateMod_Util.MISSING_DOUBLE;
		_sjmina = StateMod_Util.MISSING_DOUBLE;
		_sjrela = StateMod_Util.MISSING_DOUBLE;
		// Newer data - if not specified, the following should display OK and not trigger a dirty
		__creuse = "";
		__cdivtyp = "";
		__oprLoss = StateMod_Util.MISSING_DOUBLE;
		__oprLimit = StateMod_Util.MISSING_DOUBLE;
		__ioBeg = StateMod_Util.MISSING_INT;
		__ioEnd = StateMod_Util.MISSING_INT;
		// Only used by some rights, but setup data to avoid memory management checks
		__oprMax = new double[13];
		for (int i = 0; i < 13; i++)
		{
			__oprMax[i] = StateMod_Util.MISSING_DOUBLE;
		}
		// Only used by some rights, but setup data to avoid memory management checks
		__oprEff = new double[12];
		for (int i = 0; i < 12; i++)
		{
			__oprEff[i] = StateMod_Util.MISSING_DOUBLE;
		}
		__internT = new string[10]; // Maximum defined by StateMod
		for (int i = 0; i < 10; i++)
		{
			__internT[i] = "";
		}
		__oprLossC = new double[10]; // Maximum defined by StateMod
		for (int i = 0; i < 10; i++)
		{
			__oprLossC[i] = StateMod_Util.MISSING_DOUBLE;
		}
	}

	/// <summary>
	/// Indicate whether an operational right is known to the software.  If true, then the internal code should
	/// handle.  If false, the right should be treated as strings on read. </summary>
	/// <param name="rightTypeNumber"> the right type number </param>
	/// <param name="dataSet"> StateMod_DataSet, needed to check some relationships during the read (e.g., type 24). </param>
	public static bool isRightUnderstoodByCode(int rightTypeNumber, StateMod_DataSet dataSet)
	{
		StateMod_OperationalRight_Metadata metadata = StateMod_OperationalRight_Metadata.getMetadata(rightTypeNumber);
		if ((metadata == null) || !metadata.getFullEditingSupported(dataSet))
		{
			return false;
		}
		else
		{
			return true;
		}
	}

	/// <summary>
	/// Return the destination data set object by searching appropriate dataset lists.
	/// The destination identifier is matched up against station and right identifiers and the corresponding
	/// object returned. </summary>
	/// <param name="dataset"> the full dataset from which the destination should be extracted </param>
	public virtual StateMod_Data lookupDestinationDataObject(StateMod_DataSet dataset)
	{
		string destinationID = getCiopde();
		StateMod_OperationalRight_Metadata metadata = getMetadata();
		if (metadata == null)
		{
			throw new Exception("Unable to get operational right metadata for type " + getItyopr() + " - unable to get destination object.");
		}
		StateMod_OperationalRight_Metadata_SourceOrDestinationType[] destinationTypes = metadata.getDestinationTypes();
		if (destinationTypes == null)
		{
			return null;
		}
		IList<StateMod_Data> smdataList = new List<StateMod_Data>();
		for (int i = 0; i < destinationTypes.Length; i++)
		{
			 ((IList<StateMod_Data>)smdataList).AddRange(StateMod_Util.getDataList(destinationTypes[i], dataset, destinationID, true));
			if (smdataList.Count > 0)
			{
				break;
			}
		}
		if (smdataList.Count == 1)
		{
			return smdataList[0];
		}
		else if (smdataList.Count == 0)
		{
			return null;
		}
		else
		{
			throw new Exception("" + smdataList.Count + " data objects returned matching destination \"" + destinationID + "\" for operational right \"" + getID() + " - one is expected.");
		}
	}

	/// <summary>
	/// Return the source1 data set object(s) by searching appropriate dataset lists.
	/// The source identifier(s) are matched up against station and right identifiers and the corresponding
	/// object returned. </summary>
	/// <param name="dataset"> the full dataset from which the destination should be extracted </param>
	public virtual StateMod_Data lookupSource1DataObject(StateMod_DataSet dataset)
	{
		string sourceID = getCiopso1();
		StateMod_OperationalRight_Metadata metadata = getMetadata();
		if (metadata == null)
		{
			throw new Exception("Unable to get operational right metadata for type " + getItyopr() + " - unable to get source1 object.");
		}
		StateMod_OperationalRight_Metadata_SourceOrDestinationType[] sourceTypes = metadata.getSource1Types();
		if (sourceTypes == null)
		{
			return null;
		}
		else if ((sourceTypes.Length == 1) && (sourceTypes[0] == StateMod_OperationalRight_Metadata_SourceOrDestinationType.NA))
		{
			return null;
		}
		IList<StateMod_Data> smdataList = new List<StateMod_Data>();
		for (int i = 0; i < sourceTypes.Length; i++)
		{
			((IList<StateMod_Data>)smdataList).AddRange(StateMod_Util.getDataList(sourceTypes[i], dataset, sourceID, true));
			if (smdataList.Count > 0)
			{
				break;
			}
		}
		if (smdataList.Count == 1)
		{
			return smdataList[0];
		}
		else if (smdataList.Count == 0)
		{
			return null;
		}
		else
		{
			throw new Exception("" + smdataList.Count + " data objects returned matching source1 \"" + sourceID + "\" for \"" + getID() + "\" - one is expected.");
		}
	}

	/// <summary>
	/// Return the source2 data set object(s) by searching appropriate dataset lists.
	/// The source identifier(s) are matched up against station and right identifiers and the corresponding
	/// object returned. </summary>
	/// <param name="dataset"> the full dataset from which the destination should be extracted </param>
	public virtual StateMod_Data lookupSource2DataObject(StateMod_DataSet dataset)
	{
		string sourceID = getCiopso2();
		StateMod_OperationalRight_Metadata metadata = getMetadata();
		if (metadata == null)
		{
			throw new Exception("Unable to get operational right metadata for type " + getItyopr() + " - unable to get source2 object.");
		}
		StateMod_OperationalRight_Metadata_SourceOrDestinationType[] sourceTypes = metadata.getSource2Types();
		if (sourceTypes == null)
		{
			return null;
		}
		else if ((sourceTypes.Length == 1) && (sourceTypes[0] == StateMod_OperationalRight_Metadata_SourceOrDestinationType.NA))
		{
			return null;
		}
		IList<StateMod_Data> smdataList = new List<StateMod_Data>();
		for (int i = 0; i < sourceTypes.Length; i++)
		{
			((IList<StateMod_Data>)smdataList).AddRange(StateMod_Util.getDataList(sourceTypes[i], dataset, sourceID, true));
			if (smdataList.Count > 0)
			{
				break;
			}
		}
		if (smdataList.Count == 1)
		{
			return smdataList[0];
		}
		else if (smdataList.Count == 0)
		{
			return null;
		}
		else
		{
			throw new Exception("" + smdataList.Count + " data objects returned matching source2 \"" + sourceID + "\" for \"" + getID() + " - one is expected.");
		}
	}

	/// <summary>
	/// Read operational right information in and store in a list. </summary>
	/// <param name="filename"> Name of file to read. </param>
	/// <param name="dataSet"> StateMod_DataSet, needed to check some relationships during the read (e.g., type 24). </param>
	/// <exception cref="Exception"> if there is an error reading the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.List<StateMod_OperationalRight> readStateModFile(String filename, StateMod_DataSet dataSet) throws Exception
	public static IList<StateMod_OperationalRight> readStateModFile(string filename, StateMod_DataSet dataSet)
	{
		int version = determineFileVersion(filename);
		if (version == 1)
		{
			// TODO SAM 2010-12-27 Evaluate whether old format should be supported - too much work.
			throw new Exception("StateMod operating rules file format 1 is not supported.");
			//return readStateModFileVersion1(filename);
		}
		else if (version == 2)
		{
			return readStateModFileVersion2(filename, dataSet);
		}
		else
		{
			throw new Exception("Unable to determine StateMod file version to read operational rights.");
		}
	}

	/// <summary>
	/// Read the StateMod operational rights file associated operating rule. </summary>
	/// <param name="routine"> to use for logging. </param>
	/// <param name="linecount"> Line count (1+) before reading in this method. </param>
	/// <param name="in"> BufferedReader to read. </param>
	/// <param name="anOprit"> Operational right for which to read data. </param>
	/// <returns> the number of errors. </returns>
	/// <exception cref="IOException"> if there is an error reading the file </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static int readStateModFile_AssociatedOperatingRule(String routine, int linecount, java.io.BufferedReader in, StateMod_OperationalRight anOprit) throws java.io.IOException
	private static int readStateModFile_AssociatedOperatingRule(string routine, int linecount, StreamReader @in, StateMod_OperationalRight anOprit)
	{
		string iline = null;
		try
		{
			iline = @in.ReadLine();
			++linecount;
			Message.printStatus(2, routine, "Processing operating rule " + anOprit.getItyopr() + " associated operating rule " + (linecount + 1) + ": " + iline);
			anOprit.setCx(iline.Trim());
		}
		catch (Exception e)
		{
			// TODO SAM 2010-12-13 Need to handle errors and provide feedback
			Message.printWarning(3, routine, "Error reading associated operating rule at line " + (linecount + 1) + ": " + iline + " (" + e + ")");
			return 1;
		}
		return 0;
	}

	/// <summary>
	/// Read the StateMod operational rights file intervening structures, with loss. </summary>
	/// <param name="ninterv"> Intervening structures switch. </param>
	/// <param name="routine"> to use for logging. </param>
	/// <param name="linecount"> Line count (1+) before reading in this method. </param>
	/// <param name="in"> BufferedReader to read. </param>
	/// <param name="anOprit"> Operational right for which to read data. </param>
	/// <returns> the number of errors. </returns>
	/// <exception cref="IOException"> if there is an error reading the file </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static int readStateModFile_InterveningStructuresWithLoss(int ninterv, String routine, int linecount, java.io.BufferedReader in, StateMod_OperationalRight anOprit) throws java.io.IOException
	private static int readStateModFile_InterveningStructuresWithLoss(int ninterv, string routine, int linecount, StreamReader @in, StateMod_OperationalRight anOprit)
	{ // One line has up to 10 intervening structure identifiers
		int errorCount = 0;
		string iline = "";
		try
		{
			for (int i = 0; i < ninterv; i++)
			{
				iline = @in.ReadLine().Trim();
				++linecount;
				Message.printStatus(2, routine, "Processing operating rule " + anOprit.getItyopr() + " intervening structures with loss line " + (linecount + 1) + ": " + iline);
				IList<string> tokens = StringUtil.breakStringList(iline, " \t", StringUtil.DELIM_SKIP_BLANKS);
				int ntokens = 0;
				if (tokens != null)
				{
					ntokens = tokens.Count;
				}
				if (ntokens > 0)
				{
					anOprit.setIntern(i, tokens[0], false);
				}
				if (ntokens > 1)
				{
					if (StringUtil.isDouble(tokens[1]))
					{
						anOprit.setOprLossC(i, tokens[1]);
					}
					else
					{
						Message.printWarning(3,routine,"Intervening structure " + (i + 1) + " loss percent (" + tokens[1] + " is not a number.");
					}
				}
				if (ntokens > 2)
				{
					anOprit.setInternT(i, tokens[2]);
				}
			}
		}
		catch (Exception e)
		{
			// TODO SAM 2010-12-13 Need to handle errors and provide feedback
			Message.printWarning(3, routine, "Error reading intervening structures at line " + (linecount + 1) + ": " + iline + " (" + e + ")");
		}
		return errorCount;
	}

	/// <summary>
	/// Read the StateMod operational rights file intervening structures, without loss. </summary>
	/// <param name="ninterv"> Intervening structures switch. </param>
	/// <param name="routine"> to use for logging. </param>
	/// <param name="linecount"> Line count (1+) before reading in this method. </param>
	/// <param name="in"> BufferedReader to read. </param>
	/// <param name="anOprit"> Operational right for which to read data. </param>
	/// <returns> the number of errors. </returns>
	/// <exception cref="IOException"> if there is an error reading the file </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static int readStateModFile_InterveningStructuresWithoutLoss(int ninterv, String routine, int linecount, java.io.BufferedReader in, StateMod_OperationalRight anOprit) throws java.io.IOException
	private static int readStateModFile_InterveningStructuresWithoutLoss(int ninterv, string routine, int linecount, StreamReader @in, StateMod_OperationalRight anOprit)
	{ // One line has up to 10 intervening structure identifiers - format 10a12
		string iline = @in.ReadLine().Trim();
		string format = "x36a12a12a12a12a12a12a12a12a12a12";
		++linecount;
		int errorCount = 0;
		try
		{
			Message.printStatus(2, routine, "Processing operating rule " + anOprit.getItyopr() + " intervening structures without loss line " + (linecount + 1) + ": " + iline);
			IList<object> v = StringUtil.fixedRead(iline, format);
			for (int i = 0; i < ninterv; i++)
			{
				anOprit.setIntern(i, ((string)v[i]).Trim(), false);
			}
		}
		catch (Exception e)
		{
			// TODO SAM 2010-12-13 Need to handle errors and provide feedback
			Message.printWarning(3, routine, "Error reading intervening structures at line " + (linecount + 1) + ": " + iline + " (" + e + ")");
		}
		return errorCount;
	}

	/// <summary>
	/// Read the StateMod operational rights file intervening structures. </summary>
	/// <param name="routine"> to use for logging. </param>
	/// <param name="linecount"> Line count (1+) before reading in this method. </param>
	/// <param name="in"> BufferedReader to read. </param>
	/// <param name="anOprit"> Operational right for which to read data. </param>
	/// <returns> the number of errors. </returns>
	/// <exception cref="IOException"> if there is an error reading the file </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static int readStateModFile_MonthlyAndAnnualLimitationData(String routine, int linecount, java.io.BufferedReader in, StateMod_OperationalRight anOprit) throws java.io.IOException
	private static int readStateModFile_MonthlyAndAnnualLimitationData(string routine, int linecount, StreamReader @in, StateMod_OperationalRight anOprit)
	{ // One line has up to 10 intervening structure identifiers
		string iline = @in.ReadLine().Trim();
		int errorCount = 0;
		try
		{
			Message.printStatus(2, routine, "Processing operating rule monthly and annual limitation line " + (linecount + 1) + ": " + iline);
			IList<string> tokens = StringUtil.breakStringList(iline, " \t", StringUtil.DELIM_SKIP_BLANKS);
			int ntokens = 0;
			if (tokens != null)
			{
				ntokens = tokens.Count;
			}
			// Only one identifier
			if (ntokens > 0)
			{
				anOprit.setCx(tokens[0].Trim());
			}
		}
		catch (Exception e)
		{
			// TODO SAM 2010-12-13 Need to handle errors and provide feedback
			Message.printWarning(3, routine, "Error reading monthly and annual limitation at line " + (linecount + 1) + ": " + iline + " (" + e + ")");
		}
		return errorCount;
	}

	/// <summary>
	/// Read the StateMod operational rights file monthly efficiencies.  This method is only called if the
	/// data line needs to be read. </summary>
	/// <param name="routine"> to use for logging. </param>
	/// <param name="linecount"> Line count (1+) before reading in this method. </param>
	/// <param name="in"> BufferedReader to read. </param>
	/// <param name="anOprit"> Operational right for which to read data. </param>
	/// <returns> the number of errors. </returns>
	/// <exception cref="IOException"> if there is an error reading the file </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static int readStateModFile_MonthlyOprEff(String routine, int linecount, java.io.BufferedReader in, StateMod_OperationalRight anOprit) throws java.io.IOException
	private static int readStateModFile_MonthlyOprEff(string routine, int linecount, StreamReader @in, StateMod_OperationalRight anOprit)
	{
		string iline = null;
		try
		{
			iline = @in.ReadLine().Trim();
			++linecount;
			Message.printStatus(2, routine, "Processing operating rule " + anOprit.getItyopr() + " monthly operating limits " + (linecount + 1) + ": " + iline);
			// Limits are free format, but 13 are expected
			IList<string> tokens = StringUtil.breakStringList(iline, " \t", StringUtil.DELIM_SKIP_BLANKS);
			int ntokens = 0;
			if (tokens != null)
			{
				ntokens = tokens.Count;
			}
			if (ntokens > 12)
			{
				ntokens = 12;
			}
			for (int i = 0; i < ntokens; i++)
			{
				anOprit.setOprEff(i, tokens[i].Trim());
			}
		}
		catch (Exception e)
		{
			// TODO SAM 2010-12-13 Need to handle errors and provide feedback
			Message.printWarning(3, routine, "Error reading monthly operational limits at line " + (linecount + 1) + ": " + iline + " (" + e + ")");
			return 1;
		}
		return 0;
	}

	/// <summary>
	/// Read the StateMod operational rights file monthly operational limits. </summary>
	/// <param name="routine"> to use for logging. </param>
	/// <param name="linecount"> Line count (1+) before reading in this method. </param>
	/// <param name="in"> BufferedReader to read. </param>
	/// <param name="anOprit"> Operational right for which to read data. </param>
	/// <returns> the number of errors. </returns>
	/// <exception cref="IOException"> if there is an error reading the file </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static int readStateModFile_MonthlyOprMax(String routine, int linecount, java.io.BufferedReader in, StateMod_OperationalRight anOprit) throws java.io.IOException
	private static int readStateModFile_MonthlyOprMax(string routine, int linecount, StreamReader @in, StateMod_OperationalRight anOprit)
	{
		double oprLimits = anOprit.getOprLimit();
		int oprLimitsInt = 0;
		if (oprLimits > 0)
		{
			oprLimitsInt = (int)(oprLimits + .1);
		}
		else if (oprLimits < 0)
		{
			oprLimitsInt = (int)(oprLimits - .1);
		}
		string iline = null;
		try
		{
			if (oprLimitsInt == 1)
			{
				iline = @in.ReadLine().Trim();
				++linecount;
				Message.printStatus(2, routine, "Processing operating rule " + anOprit.getItyopr() + " monthly operating limits " + (linecount + 1) + ": " + iline);
				// Limits are free format, but 13 are expected
				IList<string> tokens = StringUtil.breakStringList(iline, " \t", StringUtil.DELIM_SKIP_BLANKS);
				int ntokens = 0;
				if (tokens != null)
				{
					ntokens = tokens.Count;
				}
				for (int i = 0; i < ntokens; i++)
				{
					anOprit.setOprMax(i, tokens[i].Trim());
				}
			}
		}
		catch (Exception e)
		{
			// TODO SAM 2010-12-13 Need to handle errors and provide feedback
			Message.printWarning(3, routine, "Error reading monthly operational limits at line " + (linecount + 1) + ": " + iline + " (" + e + ")");
			return 1;
		}
		return 0;
	}

	/// <summary>
	/// Read the StateMod operational rights file monthly switches. </summary>
	/// <param name="nmonsw"> Monthly switch </param>
	/// <param name="routine"> to use for logging. </param>
	/// <param name="linecount"> Line count (1+) before reading in this method. </param>
	/// <param name="in"> BufferedReader to read. </param>
	/// <param name="anOprit"> Operational right for which to read data. </param>
	/// <returns> the number of errors. </returns>
	/// <exception cref="IOException"> if there is an error reading the file </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static int readStateModFile_MonthlySwitches(int nmonsw, String routine, int linecount, java.io.BufferedReader in, StateMod_OperationalRight anOprit) throws java.io.IOException
	private static int readStateModFile_MonthlySwitches(int nmonsw, string routine, int linecount, StreamReader @in, StateMod_OperationalRight anOprit)
	{
		string iline = @in.ReadLine().Trim();
		++linecount;
		try
		{
			Message.printStatus(2, routine, "Processing operating rule " + anOprit.getItyopr() + " monthly switch line " + (linecount + 1) + ": " + iline);
			// Switches are free format
			IList<string> tokens = StringUtil.breakStringList(iline, " \t", StringUtil.DELIM_SKIP_BLANKS);
			int ntokens = 0;
			if (tokens != null)
			{
				ntokens = tokens.Count;
			}
			if (nmonsw > 0)
			{
				anOprit._imonsw = new int[nmonsw];
			}
			for (int i = 0; i < ntokens; i++)
			{
				anOprit.setImonsw(i, tokens[i].Trim());
			}
		}
		catch (Exception e)
		{
			// TODO SAM 2010-12-13 Need to handle errors and provide feedback
			Message.printWarning(3, routine, "Error reading monthly switches at line " + (linecount + 1) + ": " + iline + " (" + e + ")");
			return 1;
		}
		return 0;
	}

	/// <summary>
	/// Read the StateMod operational rights file Rio Grande data. </summary>
	/// <param name="routine"> to use for logging. </param>
	/// <param name="linecount"> Line count (1+) before reading in this method. </param>
	/// <param name="in"> BufferedReader to read. </param>
	/// <param name="anOprit"> Operational right for which to read data. </param>
	/// <returns> the number of errors. </returns>
	/// <exception cref="IOException"> if there is an error reading the file </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static int readStateModFile_RioGrande(String routine, int linecount, java.io.BufferedReader in, StateMod_OperationalRight anOprit) throws java.io.IOException
	private static int readStateModFile_RioGrande(string routine, int linecount, StreamReader @in, StateMod_OperationalRight anOprit)
	{
		int errorCount = 0;
		// Rio Grande additional data...
		// StateMod doc treats last part as numbers but treat as strings here consistent with source ID/account
		//String formatRioGrande = "x64f8f8x1a12i8x1a12i8x1a12i8";
		string formatRioGrande = "x64f8f8x1a12a8x1a12a8x1a12a8";
		IList<object> v = null;
		string iline = @in.ReadLine();
		++linecount;
		try
		{
			Message.printStatus(2, routine, "Processing operating rule " + anOprit.getItyopr() + " Rio Grande data line " + (linecount + 1) + ": " + iline);
			v = StringUtil.fixedRead(iline, formatRioGrande);
			anOprit.setQdebt((float?)v[0]);
			anOprit.setQdebtx((float?)v[1]);
			anOprit.setCiopso3(((string)v[2]).Trim());
			anOprit.setIopsou3(((string)v[3]).Trim());
			anOprit.setCiopso4(((string)v[4]).Trim());
			anOprit.setIopsou4(((string)v[5]).Trim());
			anOprit.setCiopso5(((string)v[6]).Trim());
			anOprit.setIopsou5(((string)v[7]).Trim());
		}
		catch (Exception e)
		{
			// TODO SAM 2010-12-13 Need to handle errors and provide feedback
			Message.printWarning(3, routine, "Error reading Rio Grande data at line " + (linecount + 1) + ": " + iline + " (" + e + ")");
		}
		return errorCount;
	}

	/// <summary>
	/// Read the StateMod operational rights file San Juan data. </summary>
	/// <param name="routine"> to use for logging. </param>
	/// <param name="linecount"> Line count (1+) before reading in this method. </param>
	/// <param name="in"> BufferedReader to read. </param>
	/// <param name="anOprit"> Operational right for which to read data. </param>
	/// <returns> the number of errors. </returns>
	/// <exception cref="IOException"> if there is an error reading the file </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static int readStateModFile_SanJuan(String routine, int linecount, java.io.BufferedReader in, StateMod_OperationalRight anOprit) throws java.io.IOException
	private static int readStateModFile_SanJuan(string routine, int linecount, StreamReader @in, StateMod_OperationalRight anOprit)
	{
		int errorCount = 0;
		// San Juan additional data...
		string formatSanJuan = "x64f8f8";
		IList<object> v = null;
		string iline = @in.ReadLine();
		++linecount;
		try
		{
			Message.printStatus(2, routine, "Processing operating rule " + anOprit.getItyopr() + " San Juan data line " + (linecount + 1) + ": " + iline);
			v = StringUtil.fixedRead(iline, formatSanJuan);
			anOprit.setSjmina((float?)v[0]);
			anOprit.setSjrela((float?)v[1]);
		}
		catch (Exception e)
		{
			// TODO SAM 2010-12-13 Need to handle errors and provide feedback
			Message.printWarning(3, routine, "Error reading San Juan data at line " + (linecount + 1) + ": " + iline + " (" + e + ")");
		}
		return errorCount;
	}

	/// <summary>
	/// Read operational right information in and store in a list. </summary>
	/// <param name="filename"> Name of file to read - the file should be the older "version 1" format. </param>
	/// <exception cref="Exception"> if there is an error reading the file. </exception>
	/*
	private static List<StateMod_OperationalRight> readStateModFileVersion1(String filename)
	throws Exception {
		String routine = "StateMod_OperationalRight.readStateModFileVersion1";
		String iline = null;
		List v = null;
		List<StateMod_OperationalRight> theOprits = new Vector();
		List<String> comment_vector = new Vector(1);	// Will be used prior to finding an operational right
		// Formats use strings for many variables because files may have extra
		// whitespace or be used for numeric and character data...
		// Consistent among all operational rights...
		// Before adding creuse, etc... (12 values)
		//String format_0 = "s12s24x16s12s8i8x1s12s8x1s12s8x1s12s8s8";
		// After adding creuse, etc.... (18 values)
		String format_0 = "s12s24x16s12s8i8x1s12s8x1s12s8x1s12s8s8x1a12x1s12x1s8s8s8s8";
		// Format for intervening structures...
		// TODO SAM 2007-03-01 Evaluate use
		//String format_interv = "x36s12s12s12s12s12s12s12s12s12s12";
		// Rio Grande additional data...
		String format_rg = "x64s8s8x1s12s8x1s12s8x1s12s8";
		String format_sj = "x64s8s8";
		BufferedReader in = null;
		StateMod_OperationalRight anOprit = null;
		int linecount = 0;
	
		int dumx, ninterv, nmonsw;
		int type = 0;
		int errorCount = 0;
	
		Message.printStatus(2, routine, "Reading operational rights file \"" + filename + "\"");
		try {
			boolean reading_unknown_right = false;
			List<String> right_strings_Vector = null;	// Operating rule as a list of strings
			in = new BufferedReader(new FileReader(filename));
			while ((iline = in.readLine()) != null) {
				++linecount;
				Message.printStatus ( 2, routine, "Processing operating rule line " + linecount + ": " + iline );
				// If was reading an unknown rule, turn off flag if done reading.
				if ( reading_unknown_right ) {
					if ( (iline.length() > 0) && (iline.charAt(0) != ' ') ) {
						// Done reading the unknown right.  Next are either comments before or data for
						// the next right.
						reading_unknown_right = false;
						// Add to the end of the list
						Message.printStatus ( 2, routine, "Adding unrecognized operational right \"" +
								anOprit.getID() + "\" as text");
						theOprits.add(anOprit);
						// Don't continue because the line that was just read needs to be handled.
					}
					else {
						// Blank at front of line so assume still reading the unknown right.
						// Add a string to the unknown right
						right_strings_Vector.add ( iline );
						continue;
					}
				}
				// check for comments
				// if a temporary comment line
				if ( iline.startsWith("#>") ) {
					continue;
				}
				// TODO SAM 2008-03-10 Evaluate whether needed
				//else if ((iline.startsWith("#") && !readingTmpComments)	|| iline.trim().length()==0) {
				//	// A general comment line not associated with an operational right...
				//	continue;
				//}
				else if (iline.startsWith("#")) { 
					// A comment line specific to an individual operational right...
					if (Message.isDebugOn) {
						Message.printDebug(10, routine, "Opright comments: " + iline);
					}
					comment_vector.add(iline.substring(1).trim());
					continue;
				}
	
				// Allocate new operational rights object
				anOprit = new StateMod_OperationalRight();
				if (Message.isDebugOn) {
					Message.printDebug(10, routine,	"Number of Opright comments: " + comment_vector.size());
				}
				if (comment_vector.size()> 0) {
					// Set comments that have been read previous to this line.
					anOprit.setCommentsBeforeData(comment_vector);
				}
				// Always clear out for next object...
				comment_vector = new Vector(1);
	
				// line 1
				if (Message.isDebugOn) {
					Message.printDebug(50, routine, "line 1: " + iline);
				}
				v = StringUtil.fixedRead(iline, format_0);
				if ( Message.isDebugOn ) {
					Message.printDebug ( 1, routine, v.toString() );
				}
				anOprit.setID(((String)v.get(0)).trim()); 
				anOprit.setName(((String)v.get(1)).trim()); 
				anOprit.setRtem(((String)v.get(2)).trim()); 
				anOprit.setDumx(((String)v.get(3)).trim());
				dumx = anOprit.getDumx();
				anOprit.setSwitch( (Integer)v.get(4) );
				// Should always be in file but may be zero...
				anOprit.setCiopde(((String)v.get(5)).trim());
				anOprit.setIopdes(((String)v.get(6)).trim());
				// Should always be in file but may be zero...
				anOprit.setCiopso1(((String)v.get(7)).trim());
				anOprit.setIopsou1(((String)v.get(8)).trim());
				// Should always be in file but may be zero...
				anOprit.setCiopso2(((String)v.get(9)).trim());
				anOprit.setIopsou2(((String)v.get(10)).trim());
				// Type is used to make additional decisions below...
				type = StringUtil.atoi(((String)v.get(11)).trim());
				anOprit.setItyopr(type);
				// Plan ID
				anOprit.setCreuse(((String)v.get(12)).trim());
				// Diversion type
				anOprit.setCdivtyp(((String)v.get(13)).trim());
				// Conveyance loss...
				double oprLoss = StringUtil.atod(((String)v.get(14)).trim());
				anOprit.setOprLoss ( oprLoss );
				// Miscellaneous limits...
				double oprLimit = StringUtil.atod(((String)v.get(15)).trim());
				anOprit.setOprLimit ( oprLimit );
				// Beginning year...
				int ioBeg = StringUtil.atoi(((String)v.get(16)).trim());
				anOprit.setIoBeg ( ioBeg );
				// Ending year...
				int ioEnd = StringUtil.atoi(((String)v.get(17)).trim());
				anOprit.setIoEnd ( ioEnd );
				Message.printStatus( 2, routine, "Reading operating rule type " + type +
						" starting at line " + linecount );
				
				StateMod_OperationalRight_Metadata metadata = StateMod_OperationalRight_Metadata.getMetadata(type);
				if ( (metadata == null) || !metadata.getFullEditingSupported() ) {
					// The type is not known so read in as strings and set the type to negative.
					// Most of the reading will occur at the top of the loop.
					reading_unknown_right = true;
					right_strings_Vector = new Vector();
					right_strings_Vector.add ( iline );
					// Add list and continue to add if more lines are read.  Since using a reference
					// this will ensure that all lines are set for the right.
					anOprit.setRightStrings ( right_strings_Vector );
					Message.printWarning ( 2, routine, "Unknown right type " + type + " at line " + linecount +
							".  Reading as text to continue reading file." );
					continue;
				}
	
				// Now read the additional lines of data.  Just do the
				// logic brute force since the order of data is not
				// a pattern that is common between many rights...
	
				nmonsw = 0;
				ninterv = 0;
				
				// May have monthly switch and intervening structures.  For now check the value.
				// FIXME SAM 2008-03-17 Will read in a file that indicates what is allowed so it is
				// easier to dynamically check.
				
				if ( dumx == 12 ) {
					// Only have monthly switches
					nmonsw = 12;
				}
				else if ( dumx >= 0 ) {
					// Only have intervening structures...
					ninterv = dumx;
				}
				else if ( dumx < 0 ){
					// Have monthly switches and intervening structures.
					// -12 of the total count toward the monthly switch and the remainder is
					// the number of intervening structures
					// Check the value because some rules like 17 - Rio Grande Compact use -8
					if ( dumx < -12 ) {
						ninterv = -1*(dumx + 12);
						nmonsw = 12;
					}
					else {
						ninterv = -1*dumx;
					}
				}
				// FIXME SAM 2008-03-17 Need some more checks for things like invalid -11 and + 13
				
				// Start reading additional information before monthly and intervening data)...
	
				if ( type == 17 ) {
					// Rio Grande compact data...
					iline = in.readLine().trim();
					++linecount;
					Message.printStatus ( 2, routine, "Processing operating rule " + type +
						" Rio Grande data line " + linecount + ": " + iline );
					v = StringUtil.fixedRead(iline, format_rg);
					anOprit.setQdebt( ((String)v.get(0)).trim() );
					anOprit.setQdebtx( ((String)v.get(1)).trim() );
					anOprit.setCiopso3(	((String)v.get(2)).trim());
					anOprit.setIopsou3(	((String)v.get(3)).trim());
					anOprit.setCiopso4(	((String)v.get(4)).trim());
					anOprit.setIopsou4(	((String)v.get(5)).trim());
					anOprit.setCiopso5(	((String)v.get(6)).trim());
					anOprit.setIopsou5(	((String)v.get(7)).trim());
				}
				else if ( type == 18 ) {
					// Rio Grande Compact - Conejos River
					iline = in.readLine().trim();
					++linecount;
					Message.printStatus ( 2, routine, "Processing operating rule line " + linecount + ": " + iline );
					v = StringUtil.fixedRead(iline, format_rg);
					anOprit.setQdebt( ((String)v.get(0)).trim() );
					anOprit.setQdebtx( ((String)v.get(1)).trim() );
					anOprit.setCiopso3(	((String)v.get(2)).trim());
					anOprit.setIopsou3(	((String)v.get(3)).trim());
					anOprit.setCiopso4(	((String)v.get(4)).trim());
					anOprit.setIopsou4(	((String)v.get(5)).trim());
					anOprit.setCiopso5(	((String)v.get(6)).trim());
					anOprit.setIopsou5(	((String)v.get(7)).trim());
				}
				else if ( type == 20 ) {
					// San Juan RIP...
					v = StringUtil.fixedRead(iline, format_sj);
					anOprit.setSjmina( ((String)v.get(0)).trim());
					anOprit.setSjrela( ((String)v.get(1)).trim());
				}
				
				// ...end reading additional data before monthly and intervening structure data
				
				// Start reading the monthly and intervening structure data...
				
				Message.printStatus ( 2, routine, "Number of intervening structures = " + ninterv +
						" month switch = " + nmonsw );
				if ( nmonsw > 0 ) {
					errorCount += readStateModFile_MonthlySwitches ( nmonsw, routine, linecount, in, anOprit );
					++linecount;
				}
				// Don't read for the Rio Grande types
				if ( (ninterv > 0) && (type != 17) && (type != 18) ) {
					errorCount += readStateModFile_InterveningStructures (
						ninterv, routine, linecount, in, anOprit );
					++linecount;
				}
				
				// ...end reading monthly and intervening structure data.
				
				// Start reading additional data after monthly and intervening structure data...
				
				// ...end reading additional data after monthly and intervening structure data
	
				// add the operational right to the vector of oprits
				Message.printStatus ( 2, routine, "Adding recognized operational right \"" +
						anOprit.getID() + "\" from full read.");
				theOprits.add(anOprit);
			}
			// All lines have been read.
			if ( reading_unknown_right ) {
				// Last line was part of the unknown right so need to add what there was.
				Message.printStatus ( 2, routine, "Adding unrecognized operational right \"" +
						anOprit.getID() + "\" as text.");
				theOprits.add(anOprit);
			}
		}
		catch (Exception e) {
			Message.printWarning(3, routine, "Error reading near line " + linecount + ": " + iline);
			Message.printWarning(3, routine, e);
			throw e;
		}
		finally {
			if (in != null) {
				in.close();
			}
		}
		return theOprits;
	}
	*/

	/// <summary>
	/// Read operational right information in and store in a list. </summary>
	/// <param name="filename"> Name of file to read - the file should be the older "version 2" format. </param>
	/// <param name="dataSet"> StateMod_DataSet, needed to check some relationships during the read (e.g., type 24). </param>
	/// <exception cref="Exception"> if there is an error reading the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static java.util.List<StateMod_OperationalRight> readStateModFileVersion2(String filename, StateMod_DataSet dataSet) throws Exception
	private static IList<StateMod_OperationalRight> readStateModFileVersion2(string filename, StateMod_DataSet dataSet)
	{
		string routine = "StateMod_OperationalRight.readStateModFileVersion2";
		string iline = null;
		IList<object> v = null;
		IList<StateMod_OperationalRight> theOprits = new List<StateMod_OperationalRight>();
		IList<string> commentsBeforeData = new List<string>(); // Will be used prior to finding an operational right
		// Formats use strings for many variables because files may have extra
		// whitespace or be used for numeric and character data...
		// Consistent among all operational rights...
		// Before adding creuse, etc... (12 values)
		//   String format_0 = "s12s24x16s12s8i8x1s12s8x1s12s8x1s12s8s8";
		// After adding creuse, etc.... (18 values)
		// Format to read line 1.  The following differ from the StateMod documentation (as of Nov 2008 doc):
		// - administration number is read as a string (not float) to prevent roundoff since this
		//   is an important number
		// - iopdes (destination account) is treated as a string (not integer) for flexibility
		// - creuse, cdivtyp, OprLoss, OprLimit, IoBeg, and IoEnd are read as strings and allowed to be
		//   missing, which will use StateMod internal defaults
		// 
		string formatLine1 = "a12a24x12x4a12f8i8x1a12a8x1a12a8x1a12a8i8x1a12x1a12x1a8a8a8a8";
		StateMod_OperationalRight anOprit = null;
		StreamReader @in = null;
		int linecount = 0;

		int dumxInt, ninterv, nmonsw;
		float dumxFloat;
		double oprLimit; // Internal value
		int rightType = 0;
		int errorCount = 0;

		Message.printStatus(2, routine, "Reading operational rights file \"" + filename + "\"");
		try
		{
			bool readingUnknownRight = false;
			IList<string> rightStringsList = null; // Operating rule as a list of strings
			@in = new StreamReader(filename);
			while (!string.ReferenceEquals((iline = @in.ReadLine()), null))
			{
				++linecount;
				Message.printStatus(2, routine, "Processing operating rule line " + linecount + ": " + iline);
				// If was reading an unknown rule, turn off flag if done reading.
				if (readingUnknownRight)
				{
					if ((iline.Length > 0) && (iline[0] != ' '))
					{
						// Done reading the unknown right.  Next are either comments before or data for
						// the next right.
						readingUnknownRight = false;
						// Add to the end of the list
						Message.printStatus(2, routine, "Adding unrecognized operational right \"" + anOprit.getID() + "\" as text from previous lines");
						theOprits.Add(anOprit);
						// Don't continue because the line that was just read needs to be handled.
					}
					else
					{
						// Blank at front of line so assume still reading the unknown right.
						// Add a string to the unknown right
						rightStringsList.Add(iline);
						continue;
					}
				}
				// check for comments
				// if a temporary comment line
				if (iline.StartsWith("#>", StringComparison.Ordinal))
				{
					continue;
				}
				/* TODO SAM 2008-03-10 Evaluate whether needed
				else if ((iline.startsWith("#") && !readingTmpComments)	|| iline.trim().length()==0) {
					// A general comment line not associated with an operational right...
					continue;
				}
				*/
				else if (iline.StartsWith("#", StringComparison.Ordinal))
				{
					// A comment line specific to an individual operational right...
					string comment = iline.Substring(1);
					Message.printStatus(2, routine, "Treating as comment before right: \"" + comment + "\"");
					// Don't trim because may want to compare output during testing
					// Do trim the initial #, which will get added on output.
					commentsBeforeData.Add(comment);
					continue;
				}

				// Allocate new operational rights object
				anOprit = new StateMod_OperationalRight();
				if (Message.isDebugOn)
				{
					Message.printDebug(10, routine, "Number of Opright comments: " + commentsBeforeData.Count);
				}
				if (commentsBeforeData.Count > 0)
				{
					// Set comments that have been read previous to this line.  First, attempt to discard
					// comments that do not below with the operational right.  For now, search backward for
					// "FileFormatVersion", "EndHeader", and "--e".  If found, discard the comments prior
					// to this because they are assumed to be file header comments, not comments for a specific right.
					string comment;
					for (int iComment = commentsBeforeData.Count - 1; iComment >= 0; --iComment)
					{
						comment = commentsBeforeData[iComment].ToUpper();
						if ((comment.IndexOf("FILEFORMATVERSION", StringComparison.Ordinal) >= 0) || (comment.IndexOf("ENDHEADER", StringComparison.Ordinal) >= 0))
						{ //|| (comment.indexOf("--E") >= 0) ) {
							// TODO SAM 2011-02-05 Problem --E often found intermingled in file to help users
							// Remove the comments above the position.
							while (iComment >= 0)
							{
								commentsBeforeData.RemoveAt(iComment--);
							}
							break;
						}
					}
					anOprit.setCommentsBeforeData(commentsBeforeData);
				}
				// Always clear out for next right...
				commentsBeforeData = new List<string>(1);

				// line 1
				if (Message.isDebugOn)
				{
					Message.printDebug(50, routine, "line 1: " + iline);
				}
				v = StringUtil.fixedRead(iline, formatLine1);
				if (Message.isDebugOn)
				{
					Message.printDebug(1, routine, v.ToString());
				}
				anOprit.setID(((string)v[0]).Trim());
				anOprit.setName(((string)v[1]).Trim());
				anOprit.setRtem(((string)v[2]).Trim());
				dumxFloat = (float?)v[3].Value;
				if (dumxFloat >= 0.0)
				{
					anOprit.setDumx((int)(dumxFloat + .1)); // Add .1 to make sure 11.9999 ends up as 12, etc.
				}
				else
				{
					anOprit.setDumx((int)(dumxFloat - .1)); // Subtract .1 to make sure 11.9999 ends up as 12, etc.
				}
				dumxInt = anOprit.getDumx();
				anOprit.setIoprsw((int?)v[4]);
				// Destination data - should always be in file but may be zero...
				anOprit.setCiopde(((string)v[5]).Trim());
				anOprit.setIopdes(((string)v[6]).Trim());
				// Supply data - should always be in file but may be zero...
				anOprit.setCiopso1(((string)v[7]).Trim());
				anOprit.setIopsou1(((string)v[8]).Trim());
				// Should always be in file but may be zero...
				anOprit.setCiopso2(((string)v[9]).Trim());
				anOprit.setIopsou2(((string)v[10]).Trim());
				// Type is used to make additional decisions below...
				anOprit.setItyopr((int?)v[11]);
				rightType = anOprit.getItyopr();
				Message.printStatus(2, routine, "rightType=" + rightType + " DumxF=" + dumxFloat + " DumxI=" + dumxInt);
				// Plan ID
				anOprit.setCreuse(((string)v[12]).Trim());
				// Diversion type
				anOprit.setCdivtyp(((string)v[13]).Trim());
				// Conveyance loss...
				string OprLoss = ((string)v[14]).Trim();
				if (StringUtil.isDouble(OprLoss))
				{
					anOprit.setOprLoss(OprLoss);
				}
				double oprLossDouble = anOprit.getOprLoss();

				// Miscellaneous limits...
				string OprLimit = ((string)v[15]).Trim();
				if (StringUtil.isDouble(OprLimit))
				{
					anOprit.setOprLimit(OprLimit);
				}
				oprLimit = anOprit.getOprLimit();
				// Beginning year...
				string IoBeg = ((string)v[16]).Trim();
				if (StringUtil.isInteger(IoBeg))
				{
					anOprit.setIoBeg(IoBeg);
				}
				// Ending year...
				string IoEnd = ((string)v[17]).Trim();
				if (StringUtil.isInteger(IoEnd))
				{
					anOprit.setIoEnd(IoEnd);
				}
				Message.printStatus(2, routine, "Reading operating rule type " + rightType + " starting at line " + linecount);

				bool rightUnderstoodByCode = isRightUnderstoodByCode(rightType,dataSet);

				if (!rightUnderstoodByCode)
				{
					// The type is not known so read in as strings and set the type to negative.
					// Most of the reading will occur at the top of the loop.
					readingUnknownRight = true;
					rightStringsList = new List<string>();
					rightStringsList.Add(iline);
					// Add list and continue to add if more lines are read.  Since using a reference
					// this will ensure that all lines are set for the right.
					anOprit.setRightStrings(rightStringsList);
					Message.printWarning(2, routine, "Unknown right type " + rightType + " at line " + linecount + ".  Reading as text to continue reading file.");
					// Add metadata so that code in the GUI for example will be able to list the right type, but
					// treat as text
					StateMod_OperationalRight_Metadata metadata = StateMod_OperationalRight_Metadata.getMetadata(rightType);
					if (metadata == null)
					{
						StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source1Array_1 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[0];
						StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source2Array_1 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[0];
						StateMod_OperationalRight_Metadata_SourceOrDestinationType[] destinationArray_1 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[0];
						StateMod_OperationalRight_Metadata_DestinationLocationType[] destinationLocationArray_1 = new StateMod_OperationalRight_Metadata_DestinationLocationType[0];
						StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] associatedPlanAllowedArray_1 = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[0];
						StateMod_OperationalRight_Metadata_DiversionType[] diversionTypeArray_1 = new StateMod_OperationalRight_Metadata_DiversionType[0];
						StateMod_OperationalRight_Metadata.getAllMetadata().Add(new StateMod_OperationalRight_Metadata(rightType, false, "Unknown Type", StateMod_OperationalRight_Metadata_RuleType.NA, source1Array_1, source2Array_1, StateMod_OperationalRight_Metadata_Source2AllowedType.NA, destinationArray_1, destinationLocationArray_1, StateMod_OperationalRight_Metadata_DeliveryMethodType.NA, StateMod_OperationalRight_Metadata_CarrierAllowedType.NA, false, false, associatedPlanAllowedArray_1, diversionTypeArray_1, StateMod_OperationalRight_Metadata_TransitAndConveyanceLossAllowedType.NA, ""));
					}
					continue;
				}

				// If here the operational right is understood and additional lines of data may be provided.

				// May have monthly switch and intervening structures.  For now check the value.
				StateMod_OperationalRight_Metadata metadata = StateMod_OperationalRight_Metadata.getMetadata(rightType);

				// FIXME SAM 2008-03-17 Need some more checks for things like invalid -11 and + 13

				// Start reading additional information before monthly and intervening structure data)...

				if (metadata.getRightTypeUsesRioGrande())
				{
					errorCount += readStateModFile_RioGrande(routine, linecount, @in, anOprit);
					++linecount; // Increment here because copy passed in to above call is local to that method
				}

				if (metadata.getRightTypeUsesSanJuan())
				{
					errorCount += readStateModFile_SanJuan(routine, linecount, @in, anOprit);
					++linecount; // Increment here because copy passed in to above call is local to that method
				}

				// ...end reading additional data before monthly and intervening structure data

				// Start reading the monthly and intervening structure data - first split the "dumx"
				// value into parts...
				nmonsw = 0;
				ninterv = 0;
				// Special case for type 17 and 18, where -8 means no monthly switches and -20 = use switches
				if ((rightType == 17) || (rightType == 18))
				{
					nmonsw = 0; // Default
					ninterv = 0;
					if (dumxInt == -20)
					{
						nmonsw = 12;
					}
				}
				else
				{
					// Normal interpretation of dumx
					if (dumxInt == 12)
					{
						// Only have monthly switches
						nmonsw = 12;
					}
					else if (dumxInt >= 0)
					{
						// Only have intervening structures...
						ninterv = dumxInt;
					}
					else if (dumxInt < 0)
					{
						// Have monthly switches and intervening structures.
						// -12 of the total count toward the monthly switch and the remainder is
						// the number of intervening structures
						if (dumxInt < -12)
						{
							ninterv = -1 * (dumxInt + 12);
							nmonsw = 12;
						}
						else
						{
							ninterv = -1 * dumxInt;
						}
					}
				}

				Message.printStatus(2, routine, "Dumx=" + dumxInt + ", number of intervening structures = " + ninterv + " month switch = " + nmonsw);

				if (metadata.getRightTypeUsesMonthlySwitch())
				{
					if (nmonsw > 0)
					{
						errorCount += readStateModFile_MonthlySwitches(nmonsw, routine, linecount, @in, anOprit);
						++linecount; // Increment here because copy passed in to above call is local to that method
					}
				}

				if (metadata.getRightTypeUsesAssociatedOperatingRule(oprLimit))
				{
					errorCount += readStateModFile_AssociatedOperatingRule(routine, linecount, @in, anOprit);
					++linecount; // Increment here because copy passed in to above call is local to that method
				}

				if (metadata.getRightTypeUsesInterveningStructuresWithoutLoss())
				{
					// Only read intervening structures if allowed (otherwise assume user error in input)
					if (ninterv > 0)
					{
						errorCount += readStateModFile_InterveningStructuresWithoutLoss(ninterv, routine, linecount, @in, anOprit);
						++linecount; // Increment here because copy passed in to above call is local to that method
					}
				}
				if (metadata.getRightTypeUsesInterveningStructuresWithLoss(oprLossDouble))
				{
					// Only read intervening structures if allowed (otherwise assume user error in input)
					if (ninterv > 0)
					{
						errorCount += readStateModFile_InterveningStructuresWithLoss(ninterv, routine, linecount, @in, anOprit);
						++linecount; // Increment here because copy passed in to above call is local to that method
					}
				}

				// ...end reading monthly and intervening structure data.
				// Start reading additional records after monthly and intervening structure...

				if (metadata.getRightTypeUsesMonthlyOprMax(oprLimit))
				{
					errorCount += readStateModFile_MonthlyOprMax(routine, linecount, @in, anOprit);
					++linecount; // Increment here because copy passed in to above call is local to that method
				}

				if (metadata.getRightTypeUsesMonthlyOprEff(dataSet, anOprit.getCiopso2(), anOprit.getIopsou2()))
				{
					errorCount += readStateModFile_MonthlyOprEff(routine, linecount, @in, anOprit);
					++linecount; // Increment here because copy passed in to above call is local to that method
				}

				// ...end reading additional data after monthly and intervening structure data

				// add the operational right to the vector of rights
				Message.printStatus(2, routine, "Adding recognized operational right type " + rightType + " \"" + anOprit.getID() + "\" from full read - " + anOprit.getCommentsBeforeData().Count + " comments before data");
				theOprits.Add(anOprit);
			}
			// All lines have been read.
			if (readingUnknownRight)
			{
				// Last line was part of the unknown right so need to add what there was.
				Message.printStatus(2, routine, "Adding unrecognized operational right type " + rightType + " \"" + anOprit.getID() + "\" as text.");
				theOprits.Add(anOprit);
			}
		}
		catch (Exception e)
		{
			Message.printWarning(3, routine, "Error reading near line " + linecount + ": " + iline);
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
		// If there were any errors, generate an exception
		if (errorCount > 0)
		{
			throw new Exception("There were " + errorCount + " errors reading the operational rights.");
		}
		return theOprits;
	}

	/// <summary>
	/// Cancels any changes made to this object within a GUI since createBackup()
	/// was called and sets _original to null.
	/// </summary>
	public override void restoreOriginal()
	{
		StateMod_OperationalRight op = (StateMod_OperationalRight)_original;
		base.restoreOriginal();
		_rtem = op._rtem;
		_dumx = op._dumx;
		_ciopde = op._ciopde;
		_iopdes = op._iopdes;
		_ciopso1 = op._ciopso1;
		_iopsou1 = op._iopsou1;
		_ciopso2 = op._ciopso2;
		_iopsou2 = op._iopsou2;
		_ciopso3 = op._ciopso3;
		_iopsou3 = op._iopsou3;
		_ciopso4 = op._ciopso4;
		_iopsou4 = op._iopsou4;
		_ciopso5 = op._ciopso5;
		_iopsou5 = op._iopsou5;
		__ityopr = op.__ityopr;
		_qdebt = op._qdebt;
		_qdebtx = op._qdebtx;
		_sjmina = op._sjmina;
		_sjrela = op._sjrela;
		_imonsw = op._imonsw;
		_intern = op._intern;
		// Newer data..
		__creuse = op.__creuse;
		__cdivtyp = op.__cdivtyp;
		__oprLoss = op.__oprLoss;
		__oprLimit = op.__oprLimit;
		__ioBeg = op.__ioBeg;
		__ioEnd = op.__ioEnd;
		// Additional new data after core attributes...
		__oprMax = op.__oprMax;
		// Comments, etc...
		__rightStringsList = op.__rightStringsList;
		__commentsBeforeData = op.__commentsBeforeData;
		__readErrorList = op.__readErrorList;
		// Controlling info...
		_isClone = false;
		_original = null;
	}

	/// <summary>
	/// Set the cdivtyp.
	/// </summary>
	public virtual void setCdivtyp(string cdivtyp)
	{
		if ((!string.ReferenceEquals(cdivtyp, null)) && !cdivtyp.Equals(__cdivtyp))
		{
			__cdivtyp = cdivtyp;
			setDirty(true);
			if (Message.isDebugOn)
			{
				Message.printDebug(1,"","Setting cdivtyp dirty");
			}
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS,true);
			}
		}
	}

	/// <summary>
	/// Set the user ciopde.
	/// </summary>
	public virtual void setCiopde(string ciopde)
	{
		if ((!string.ReferenceEquals(ciopde, null)) && !ciopde.Equals(_ciopde))
		{
			_ciopde = ciopde;
			setDirty(true);
			if (Message.isDebugOn)
			{
				Message.printDebug(1,"","Setting ciopde dirty");
			}
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS,true);
			}
		}
	}

	/// <summary>
	/// Set the user ciopso.
	/// </summary>
	public virtual void setCiopso1(string ciopso1)
	{
		if ((!string.ReferenceEquals(ciopso1, null)) && !ciopso1.Equals(_ciopso1))
		{
			_ciopso1 = ciopso1;
			setDirty(true);
			if (Message.isDebugOn)
			{
				Message.printDebug(1,"","Setting ciopso1 dirty");
			}
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
			}
		}
	}

	/// <summary>
	/// Set the user ciopso2.
	/// </summary>
	public virtual void setCiopso2(string ciopso2)
	{
		if ((!string.ReferenceEquals(ciopso2, null)) && !ciopso2.Equals(_ciopso2))
		{
			_ciopso2 = ciopso2;
			setDirty(true);
			if (Message.isDebugOn)
			{
				Message.printDebug(1,"","Setting ciopso2 dirty");
			}
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
			}
		}
	}

	/// <summary>
	/// Set the user ciopso3.
	/// </summary>
	public virtual void setCiopso3(string ciopso3)
	{
		if ((!string.ReferenceEquals(ciopso3, null)) && !ciopso3.Equals(_ciopso3))
		{
			_ciopso3 = ciopso3;
			setDirty(true);
			if (Message.isDebugOn)
			{
				Message.printDebug(1,"","Setting ciopso3 dirty");
			}
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
			}
		}
	}

	/// <summary>
	/// Set the user ciopso4.
	/// </summary>
	public virtual void setCiopso4(string ciopso4)
	{
		if ((!string.ReferenceEquals(ciopso4, null)) && !ciopso4.Equals(_ciopso4))
		{
			_ciopso4 = ciopso4;
			setDirty(true);
			if (Message.isDebugOn)
			{
				Message.printDebug(1,"","Setting ciopso4 dirty");
			}
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
			}
		}
	}

	/// <summary>
	/// Set the user ciopso5.
	/// </summary>
	public virtual void setCiopso5(string ciopso5)
	{
		if ((!string.ReferenceEquals(ciopso5, null)) && !ciopso5.Equals(_ciopso5))
		{
			_ciopso5 = ciopso5;
			setDirty(true);
			if (Message.isDebugOn)
			{
				Message.printDebug(1,"","Setting ciopso5 dirty");
			}
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
			}
		}
	}

	/// <summary>
	/// Set the comments before the data in the input file. </summary>
	/// <param name="commentsBeforeData"> comments before the data in the input file. </param>
	public virtual void setCommentsBeforeData(IList<string> commentsBeforeData)
	{
		bool dirty = false;
		int size = commentsBeforeData.Count;
		IList<string> commentsBeforeData0 = getCommentsBeforeData();
		if (size != commentsBeforeData0.Count)
		{
			dirty = true;
			if (Message.isDebugOn)
			{
				Message.printDebug(1,"","commentsBeforeData old size=" + commentsBeforeData0.Count + " new size=" + size);
			}
		}
		else
		{
			// Lists are the same size and there may not have been any changes
			// Need to check each string in the comments
			for (int i = 0; i < size; i++)
			{
				if (!commentsBeforeData[i].Equals(commentsBeforeData0[i]))
				{
					dirty = true;
					if (Message.isDebugOn)
					{
						Message.printDebug(1,"","commentsBeforeData old string \"" + commentsBeforeData0[i] + "\" is different from new string \"" + commentsBeforeData[i] + "\"");
					}
					break;
				}
			}
		}
		if (dirty)
		{
			// Something was different so set the comments and change the dirty flag
			__commentsBeforeData = commentsBeforeData;
			setDirty(true);
			if (Message.isDebugOn)
			{
				Message.printDebug(1,"","Setting commentsBeforeData dirty");
			}
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS,true);
			}
		}
	}

	/// <summary>
	/// Set the creuse.
	/// </summary>
	public virtual void setCreuse(string creuse)
	{
		if ((!string.ReferenceEquals(creuse, null)) && !creuse.Equals(__creuse))
		{
			__creuse = creuse;
			setDirty(true);
			if (Message.isDebugOn)
			{
				Message.printDebug(1,"","Setting creuse dirty");
			}
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS,true);
			}
		}
	}

	/// <summary>
	/// Set the cx.
	/// </summary>
	public virtual void setCx(string cx)
	{
		if ((!string.ReferenceEquals(cx, null)) && !cx.Equals(__cx))
		{
			__cx = cx;
			setDirty(true);
			if (Message.isDebugOn)
			{
				Message.printDebug(1,"","Setting cx dirty");
			}
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS,true);
			}
		}
	}

	/// <summary>
	/// Set dumx.  This method should only be called when reading the StateMod operational rights file and otherwise
	/// dumx is calculated internally. </summary>
	/// <param name="dumx"> monthly/intervening structures switch </param>
	public virtual void setDumx(int dumx)
	{
		if (dumx != _dumx)
		{
			_dumx = dumx;
			setDirty(true);
			if (Message.isDebugOn)
			{
				Message.printDebug(1,"","Setting dumx dirty");
			}
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
			}
		}
	}

	/// <summary>
	/// Set dumx.  This method should only be called when reading the StateMod operational rights file and otherwise
	/// dumx is calculated internally. </summary>
	/// <param name="dumx"> monthly/intervening structures switch </param>
	public virtual void setDumx(int? dumx)
	{
		setDumx(dumx.Value);
	}

	/// <summary>
	/// Set dumx.  Note that sometimes the integer has a . at the end.  To resolve this,
	/// convert to a double and then cast as an integer.
	/// This method should only be called when reading the StateMod operational rights file and otherwise
	/// dumx is calculated internally. </summary>
	/// <param name="dumx"> monthly/intervening structures switch </param>
	public virtual void setDumx(string dumx)
	{
		if (!string.ReferenceEquals(dumx, null))
		{
			double? d = (double.Parse(dumx.Trim()));
			setDumx((int)d.Value);
		}
	}

	/// <summary>
	/// Set dumx from the monthly switch and intervening structure values.  For example this is used when
	/// editing data in the GUI and dumx is not set directly.
	/// </summary>
	private void setDumxFromMonthlySwitchAndInterveningStructures()
	{
		// All of the monthly switches need to have values -31 to +31.  Otherwise, the monthly
		// switches are assumed to be not used for the right
		int nValidImonsw = 0;
		StateMod_OperationalRight_Metadata metadata = getMetadata();
		if (metadata.getRightTypeUsesMonthlySwitch())
		{
			int[] imonsw = getImonsw();
			if (imonsw != null)
			{
				for (int i = 0; i < imonsw.Length; i++)
				{
					if ((imonsw[i] >= -31) && (imonsw[i] <= 31))
					{
						++nValidImonsw;
					}
				}
			}
		}
		int nValidInterveningStructures = 0;
		if (metadata.getRightTypeUsesInterveningStructuresWithoutLoss())
		{
			string[] intern = getIntern();
			if (intern != null)
			{
				for (int i = 0; i < intern.Length; i++)
				{
					if (intern[i].Length > 0)
					{
						++nValidInterveningStructures;
					}
				}
			}
		}
		int dumx = 0;
		if (nValidImonsw == 12)
		{
			dumx = 12;
		}
		if (nValidInterveningStructures > 0)
		{
			if (dumx > 0)
			{
				// Have monthly switches so start with -12 value
				dumx = -12;
			}
			// Now subtract the number of intervening structures
			dumx -= nValidInterveningStructures;
		}
		// Finally set the value
		setDumx(dumx);
	}

	/// <summary>
	/// Set ioBeg
	/// </summary>
	public virtual void setIoBeg(int ioBeg)
	{
		if (ioBeg != __ioBeg)
		{
			__ioBeg = ioBeg;
			setDirty(true);
			if (Message.isDebugOn)
			{
				Message.printDebug(1,"","Setting ioBeg dirty");
			}
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
			}
		}
	}

	/// <summary>
	/// Set ioBeg
	/// </summary>
	public virtual void setIoBeg(int? ioBeg)
	{
		setIoBeg(ioBeg.Value);
	}

	/// <summary>
	/// Set ioBeg.
	/// </summary>
	public virtual void setIoBeg(string ioBeg)
	{
		if (!string.ReferenceEquals(ioBeg, null))
		{
			setIoBeg((int)(StringUtil.atoi(ioBeg.Trim())));
		}
	}

	/// <summary>
	/// Set ioEnd
	/// </summary>
	public virtual void setIoEnd(int ioEnd)
	{
		if (ioEnd != __ioEnd)
		{
			__ioEnd = ioEnd;
			setDirty(true);
			if (Message.isDebugOn)
			{
				Message.printDebug(1,"","Setting ioEnd dirty");
			}
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
			}
		}
	}

	/// <summary>
	/// Set ioEnd
	/// </summary>
	public virtual void setIoEnd(int? ioEnd)
	{
		setIoEnd(ioEnd.Value);
	}

	/// <summary>
	/// Set ioEnd.
	/// </summary>
	public virtual void setIoEnd(string ioEnd)
	{
		if (!string.ReferenceEquals(ioEnd, null))
		{
			setIoEnd((int)(StringUtil.atoi(ioEnd.Trim())));
		}
	}

	/// <summary>
	/// Set a monthly switch.
	/// </summary>
	public virtual void setImonsw(int index, int imonsw)
	{
		if (imonsw != _imonsw[index])
		{
			_imonsw[index] = imonsw;
			setDirty(true);
			if (Message.isDebugOn)
			{
				Message.printDebug(1,"","Setting imonsw[" + index + "] dirty");
			}
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
			}
			// Also reset the dumx
			setDumxFromMonthlySwitchAndInterveningStructures();
		}
	}

	/// <summary>
	/// Set a monthly switch.
	/// </summary>
	public virtual void setImonsw(int index, int? imonsw)
	{
		setImonsw(index, imonsw.Value);
	}

	/// <summary>
	/// Set a monthly switch.
	/// </summary>
	public virtual void setImonsw(int index, string imonsw)
	{
		if (!string.ReferenceEquals(imonsw, null))
		{
			setImonsw(index, int.Parse(imonsw.Trim()));
		}
	}

	/// <summary>
	/// Set an "intern". </summary>
	/// <param name="setDumx"> if true, reset the dumx value based on the monthly switches and intervening structures
	/// (typically done when setting the intervening structures from the GUI, since dumx is not edited directly).
	/// If false, just set the intervening ID but do not change dumx (typically done when reading the data file). </param>
	public virtual void setIntern(int index, string intern, bool setDumx)
	{
		if (string.ReferenceEquals(intern, null))
		{
			return;
		}
		if (!intern.Equals(_intern[index]))
		{
			// Only set if not already set - otherwise will trigger dirty flag
			_intern[index] = intern;
			setDirty(true);
			if (Message.isDebugOn)
			{
				Message.printDebug(1,"","Setting intern[" + index + "] dirty");
			}
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
			}
			if (Message.isDebugOn)
			{
				Message.printDebug(30, "StateMod_OperationalRight.setIntern", "Old Dumx: " + getDumx() + ", New Dumx: " + index + 1);
			}
			if (setDumx)
			{
				setDumxFromMonthlySwitchAndInterveningStructures();
			}
			if (Message.isDebugOn)
			{
				Message.printDebug(30, "StateMod_OperationalRight.setInter", "Dumx: " + getDumx());
			}
		}
	}

	/// <summary>
	/// Sets the interns from a list, for example when setting from the StateMod GUI.
	/// </summary>
	public virtual void setInterns(IList<string> v)
	{
		if (v != null)
		{
			for (int i = 0; i < v.Count; i++)
			{
				setIntern(i, v[i], false);
			}
		}
	}

	/// <summary>
	/// Set an "internT".
	/// </summary>
	public virtual void setInternT(int index, string internT)
	{
		if (string.ReferenceEquals(internT, null))
		{
			return;
		}
		if (!internT.Equals(__internT[index]))
		{
			// Only set if not already set - otherwise will trigger dirty flag
			__internT[index] = internT;
			setDirty(true);
			if (Message.isDebugOn)
			{
				Message.printDebug(1,"","Setting internT[" + index + "] dirty");
			}
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
			}
		}
	}

	/// <summary>
	/// Set the iopdes.
	/// </summary>
	public virtual void setIopdes(string iopdes)
	{
		if ((!string.ReferenceEquals(iopdes, null)) && !iopdes.Equals(_iopdes))
		{
			_iopdes = iopdes;
			setDirty(true);
			if (Message.isDebugOn)
			{
				Message.printDebug(1,"","Setting iopdes dirty");
			}
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
			}
		}
	}

	/// <summary>
	/// Set the ioprsw - this calls setSwitch() in the base class.
	/// </summary>
	public virtual void setIoprsw(int? ioprsw)
	{
		setSwitch(ioprsw.Value);
	}

	/// <summary>
	/// Set the iopsou1.
	/// </summary>
	public virtual void setIopsou1(string iopsou1)
	{
		if ((!string.ReferenceEquals(iopsou1, null)) && !iopsou1.Equals(_iopsou1))
		{
			_iopsou1 = iopsou1;
			setDirty(true);
			if (Message.isDebugOn)
			{
				Message.printDebug(1,"","Setting iopsou1 dirty");
			}
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
			}
		}
	}

	/// <summary>
	/// Set the iopsou2.
	/// </summary>
	public virtual void setIopsou2(string iopsou2)
	{
		if ((!string.ReferenceEquals(iopsou2, null)) && !iopsou2.Equals(_iopsou2))
		{
			_iopsou2 = iopsou2;
			setDirty(true);
			if (Message.isDebugOn)
			{
				Message.printDebug(1,"","Setting iopsou2 dirty");
			}
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
			}
		}
	}

	/// <summary>
	/// Set the iopsou3.
	/// </summary>
	public virtual void setIopsou3(string iopsou3)
	{
		if ((!string.ReferenceEquals(iopsou3, null)) && !iopsou3.Equals(_iopsou3))
		{
			_iopsou3 = iopsou3;
			setDirty(true);
			if (Message.isDebugOn)
			{
				Message.printDebug(1,"","Setting iopsou3 dirty");
			}
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
			}
		}
	}

	/// <summary>
	/// Set the iopsou4.
	/// </summary>
	public virtual void setIopsou4(string iopsou4)
	{
		if ((!string.ReferenceEquals(iopsou4, null)) && !iopsou4.Equals(_iopsou4))
		{
			_iopsou4 = iopsou4;
			setDirty(true);
			if (Message.isDebugOn)
			{
				Message.printDebug(1,"","Setting iopsou4 dirty");
			}
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
			}
		}
	}

	/// <summary>
	/// Set the iopsou5.
	/// </summary>
	public virtual void setIopsou5(string iopsou5)
	{
		if ((!string.ReferenceEquals(iopsou5, null)) && !iopsou5.Equals(_iopsou5))
		{
			_iopsou5 = iopsou5;
			setDirty(true);
			if (Message.isDebugOn)
			{
				Message.printDebug(1,"","Setting iopsou5 dirty");
			}
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
			}
		}
	}

	/// <summary>
	/// Set the ityopr
	/// </summary>
	public virtual void setItyopr(int ityopr)
	{
		if (ityopr != __ityopr)
		{
			setDirty(true);
			if (Message.isDebugOn)
			{
				Message.printDebug(1,"","Setting ityopr dirty, old=" + __ityopr + ", new =" + ityopr);
			}
			__ityopr = ityopr;
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
			}
		}
	}

	/// <summary>
	/// Set the ityopr
	/// </summary>
	public virtual void setItyopr(int? ityopr)
	{
		setItyopr(ityopr.Value);
		// Also set the metadata for the right
		__metadata = StateMod_OperationalRight_Metadata.getMetadata(ityopr.Value);
	}

	/// <summary>
	/// Set the ityopr
	/// </summary>
	public virtual void setItyopr(string ityopr)
	{
		if (!string.ReferenceEquals(ityopr, null))
		{
			setItyopr(int.Parse(ityopr.Trim()));
		}
	}

	/// <summary>
	/// Set the OprLimit 
	/// </summary>
	public virtual void setOprLimit(string oprLimit)
	{
		if ((!string.ReferenceEquals(oprLimit, null)) && !oprLimit.Equals(""))
		{
			setOprLimit(double.Parse(oprLimit.Trim()));
		}
	}

	/// <summary>
	/// Set oprLimit
	/// </summary>
	public virtual void setOprLimit(double oprLimit)
	{
		if (oprLimit != __oprLimit)
		{
			__oprLimit = oprLimit;
			setDirty(true);
			if (Message.isDebugOn)
			{
				Message.printDebug(1,"","Setting oprLimit dirty");
			}
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
			}
		}
	}

	/// <summary>
	/// Set the OprLoss 
	/// </summary>
	public virtual void setOprLoss(string oprLoss)
	{
		if ((!string.ReferenceEquals(oprLoss, null)) && !oprLoss.Equals(""))
		{
			setOprLoss(double.Parse(oprLoss.Trim()));
		}
	}

	/// <summary>
	/// Set oprLoss
	/// </summary>
	public virtual void setOprLoss(double oprLoss)
	{
		if (oprLoss != __oprLoss)
		{
			__oprLoss = oprLoss;
			setDirty(true);
			if (Message.isDebugOn)
			{
				Message.printDebug(1,"","Setting oprLoss dirty");
			}
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
			}
		}
	}

	/// <summary>
	/// Set an oprLossC 
	/// </summary>
	public virtual void setOprLossC(int index, string oprLossC)
	{
		if ((!string.ReferenceEquals(oprLossC, null)) && !oprLossC.Equals(""))
		{
			setOprLossC(index, double.Parse(oprLossC.Trim()));
		}
	}

	/// <summary>
	/// Set an "oprLossC".
	/// </summary>
	public virtual void setOprLossC(int index, double oprLossC)
	{
		if (oprLossC != __oprLossC[index])
		{
			__oprLossC[index] = oprLossC;
			setDirty(true);
			if (Message.isDebugOn)
			{
				Message.printDebug(1,"","Setting oprLossC dirty");
			}
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
			}
		}
	}

	/// <summary>
	/// Set a monthly operational limit.
	/// </summary>
	public virtual void setOprEff(int index, double oprEff)
	{
		if (oprEff != __oprEff[index])
		{
			__oprEff[index] = oprEff;
			setDirty(true);
			if (Message.isDebugOn)
			{
				Message.printDebug(1,"","Setting oprEff[" + index + "] dirty");
			}
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
			}
		}
	}

	/// <summary>
	/// Set a monthly operational limit.
	/// </summary>
	public virtual void setOprEff(int index, double? oprEff)
	{
		setOprEff(index, oprEff.Value);
	}

	/// <summary>
	/// Set a monthly operational limit.
	/// </summary>
	public virtual void setOprEff(int index, string oprEff)
	{
		if (!string.ReferenceEquals(oprEff, null))
		{
			setOprEff(index, double.Parse(oprEff.Trim()));
		}
	}

	/// <summary>
	/// Set a monthly operational limit.
	/// </summary>
	public virtual void setOprMax(int index, double oprMax)
	{
		if (oprMax != __oprMax[index])
		{
			__oprMax[index] = oprMax;
			setDirty(true);
			if (Message.isDebugOn)
			{
				Message.printDebug(1,"","Setting oprMax[" + index + "] dirty");
			}
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
			}
		}
	}

	/// <summary>
	/// Set a monthly operational limit.
	/// </summary>
	public virtual void setOprMax(int index, double? oprMax)
	{
		setOprMax(index, oprMax.Value);
	}

	/// <summary>
	/// Set a monthly operational limit.
	/// </summary>
	public virtual void setOprMax(int index, string oprMax)
	{
		if (!string.ReferenceEquals(oprMax, null))
		{
			setOprMax(index, double.Parse(oprMax.Trim()));
		}
	}

	/// <summary>
	/// Set qdebt
	/// </summary>
	public virtual void setQdebt(double qdebt)
	{
		if (qdebt != _qdebt)
		{
			_qdebt = qdebt;
			setDirty(true);
			if (Message.isDebugOn)
			{
				Message.printDebug(1,"","Setting qdebt dirty");
			}
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
			}
		}
	}

	/// <summary>
	/// Set qdebt
	/// </summary>
	public virtual void setQdebt(double? qdebt)
	{
		setQdebt(qdebt.Value);
	}

	/// <summary>
	/// Set qdebt
	/// </summary>
	public virtual void setQdebt(string qdebt)
	{
		if (!string.ReferenceEquals(qdebt, null))
		{
			setQdebt(double.Parse(qdebt.Trim()));
		}
	}

	/// <summary>
	/// Set qdebtx
	/// </summary>
	public virtual void setQdebtx(double qdebtx)
	{
		if (qdebtx != _qdebtx)
		{
			_qdebtx = qdebtx;
			setDirty(true);
			if (Message.isDebugOn)
			{
				Message.printDebug(1,"","Setting qdebtx dirty");
			}
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
			}
		}
	}

	/// <summary>
	/// Set qdebtx
	/// </summary>
	public virtual void setQdebtx(double? qdebtx)
	{
		setQdebtx(qdebtx.Value);
	}

	/// <summary>
	/// Set qdebtx
	/// </summary>
	public virtual void setQdebtx(string qdebtx)
	{
		if (!string.ReferenceEquals(qdebtx, null))
		{
			setQdebtx(double.Parse(qdebtx.Trim()));
		}
	}

	/// <summary>
	/// Set the operating rule strings, when read as text because an unknown right type.
	/// </summary>
	public virtual void setRightStrings(IList<string> rightStringList)
	{
		bool dirty = false;
		int size = rightStringList.Count;
		IList<string> rightStringList0 = getRightStrings();
		if (size != rightStringList0.Count)
		{
			dirty = true;
			if (Message.isDebugOn)
			{
				Message.printDebug(1,"","rightStringList old size=" + rightStringList0.Count + " new size=" + size);
			}
		}
		else
		{
			// Lists are the same size and there may not have been any changes
			// Need to check each string in the data
			for (int i = 0; i < size; i++)
			{
				if (!rightStringList[i].Equals(rightStringList0[i]))
				{
					dirty = true;
					if (Message.isDebugOn)
					{
						Message.printDebug(1,"","commentsBeforeData old string \"" + rightStringList0[i] + "\" is different from new string \"" + rightStringList[i] + "\"");
					}
					break;
				}
			}
		}
		if (dirty)
		{
			// Something was different so set the strings and change the dirty flag
			__rightStringsList = rightStringList;
			setDirty(true);
			if (Message.isDebugOn)
			{
				Message.printDebug(1,"","Setting rightStringList dirty");
			}
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS,true);
			}
		}
	}

	/// <summary>
	/// Set rtem
	/// </summary>
	public virtual void setRtem(string rtem)
	{
		if (!_rtem.Equals(rtem))
		{
			_rtem = rtem;
			setDirty(true);
			if (Message.isDebugOn)
			{
				Message.printDebug(1,"","Setting rtem dirty");
			}
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
			}
		}
	}

	/// <summary>
	/// Set sjmina
	/// </summary>
	public virtual void setSjmina(double sjmina)
	{
		if (sjmina != _sjmina)
		{
			_sjmina = sjmina;
			setDirty(true);
			if (Message.isDebugOn)
			{
				Message.printDebug(1,"","Setting sjmina dirty");
			}
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
			}
		}
	}

	/// <summary>
	/// Set sjmina
	/// </summary>
	public virtual void setSjmina(double? sjmina)
	{
		setSjmina(sjmina.Value);
	}

	/// <summary>
	/// Set sjmina
	/// </summary>
	public virtual void setSjmina(string sjmina)
	{
		if (!string.ReferenceEquals(sjmina, null))
		{
			setSjmina(double.Parse(sjmina.Trim()));
		}
	}

	/// <summary>
	/// Set sjrela
	/// </summary>
	public virtual void setSjrela(double sjrela)
	{
		if (sjrela != _sjrela)
		{
			_sjrela = sjrela;
			setDirty(true);
			if (Message.isDebugOn)
			{
				Message.printDebug(1,"","Setting sjrela dirty");
			}
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
			}
		}
	}

	/// <summary>
	/// Set sjrela
	/// </summary>
	public virtual void setSjrela(double? sjrela)
	{
		setSjrela(sjrela.Value);
	}

	/// <summary>
	/// Set sjrela
	/// </summary>
	public virtual void setSjrela(string sjrela)
	{
		if (!string.ReferenceEquals(sjrela, null))
		{
			setSjrela(double.Parse(sjrela.Trim()));
		}
	}


	public virtual void setupImonsw()
	{
		_imonsw = new int[12];
		for (int i = 0; i < 12; i++)
		{
			_imonsw[i] = 0;
		}
	}

	/// <summary>
	/// Write operational right information to output.  History header information 
	/// is also maintained by calling this routine. </summary>
	/// <param name="infile"> input file from which previous history should be taken </param>
	/// <param name="outfile"> output file to which to write </param>
	/// <param name="formatVersion"> the StateMod operational rights format version (1 or 2) </param>
	/// <param name="theOpr"> list of operational right to write </param>
	/// <param name="newComments"> addition comments that should be included at the top of the file </param>
	/// <param name="dataSet"> StateMod_DataSet, needed to check some relationships during the read (e.g., type 24). </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeStateModFile(String infile, String outfile, int formatVersion, java.util.List<StateMod_OperationalRight> theOpr, java.util.List<String> newComments, StateMod_DataSet dataSet) throws Exception
	public static void writeStateModFile(string infile, string outfile, int formatVersion, IList<StateMod_OperationalRight> theOpr, IList<string> newComments, StateMod_DataSet dataSet)
	{
		if (formatVersion == 1)
		{
			writeStateModFileVersion1(infile, outfile, theOpr, newComments);
		}
		else if (formatVersion == 2)
		{
			writeStateModFileVersion2(infile, outfile, theOpr, newComments, dataSet);
		}
	}

	/// <summary>
	/// Write operational right information to output for version 1 format file.  History header information 
	/// is also maintained by calling this routine. </summary>
	/// <param name="infile"> input file from which previous history should be taken </param>
	/// <param name="outfile"> output file to which to write </param>
	/// <param name="theOpr"> vector of operational right to print </param>
	/// <param name="newComments"> addition comments which should be included in history </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeStateModFileVersion1(String infile, String outfile, java.util.List<StateMod_OperationalRight> theOpr, java.util.List<String> newComments) throws Exception
	public static void writeStateModFileVersion1(string infile, string outfile, IList<StateMod_OperationalRight> theOpr, IList<string> newComments)
	{
		PrintWriter @out = null;
		IList<string> commentIndicators = new List<string>(1);
		commentIndicators.Add("#");
		IList<string> ignoredCommentIndicators = new List<string>(1);
		ignoredCommentIndicators.Add("#>");
		string routine = "StateMod_OperationalRight.writeStateModFileVersion1";

		Message.printStatus(1, routine, "Writing new operational rights to file \"" + outfile + "\" using \"" + infile + "\" header...");

		@out = IOUtil.processFileHeaders(infile, outfile, newComments, commentIndicators, ignoredCommentIndicators, 0);
		try
		{
			string cmnt = "#>";
			string iline = null;
			string format = "%-12.12s%-36.36s%16.16s%7d.%8d %-12.12s%8d %-12.12s%8d %-12.12s%8d%8d";
			string formatS = "         %d %d %d %d %d %d %d %d %d %d %d %d";
			string formatsp = "%36.36s";
			string formatI = "%-12.12s";
			StateMod_OperationalRight opr = null;
			IList<object> v = new List<object>(12);
			IList<object> vS = new List<object>(12);
			IList<object> vsp = new List<object>(1);
			IList<object> vI = new List<object>(1);
			IList<string> comments_vector = null;

			@out.println(cmnt);
			@out.println(cmnt + " *******************************************************");
			 @out.println(cmnt + " Operational Right File");
			 @out.println(cmnt);
			@out.println(cmnt + "     Card 1   Control");
			@out.println(cmnt + "     format:  (a12, a24, 12x, 2i4, i8, f8.0, i8, 3 (i8,a12), 20i8)");
			 @out.println(cmnt);
			@out.println(cmnt + "     ID       cidvri:          Operational Right ID");
			@out.println(cmnt + "     Name     nameo:           Operational Right name");
			@out.println(cmnt + "     AdminDat iodat (1-2,k):   Effective priority date");
			@out.println(cmnt + "     Admin #  irtem:           Priority Number (smaller is most senior)");
			@out.println(cmnt + "     # Str    dumx:            Number of intervenging structures ");
			@out.println(cmnt + "     On/Off   ioprsw (k):      Switch 0 = off,1 = on");
			@out.println(cmnt + "     Dest ID  ciopde:          Destination reservoir or structure ID");
			@out.println(cmnt + "     Dest Ac  iopdes (2,k):    Destination reservoir or structure account # (1 for a diversion)");
			@out.println(cmnt + "     Sou1 ID  ciopso (1)       Supply reservoir #1 or structure ID");
			@out.println(cmnt + "     Sou1 Ac  iopsou (2,k):    Supply reservoir #1 or structure account # (1 for a diversion)");
			@out.println(cmnt + "     Sou2 ID  ciopso (2):      Supply reservoir #2 ID");
			@out.println(cmnt + "     Sou1 Ac  iopsou (4,k):    Supply reservoir #2 account");
			@out.println(cmnt + "     Type     ityopr (k):      Switch");
			@out.println(cmnt + "              1 = Reservoir Release to an instream demand");
			@out.println(cmnt + "              2 = Reservoir Release to a direct diversion demand");
			@out.println(cmnt + "              3 = Reservoir Release to a direct diversion demand by a carrier");
			@out.println(cmnt + "              4 = Reservoir Release to a direct diversion demand by exchange");
			@out.println(cmnt + "              5 = Reservoir Release to a reservoir by exchange");
			@out.println(cmnt + "              6 = Reservoir to reservoir bookover");
			@out.println(cmnt + "              7 = Reservoir Release to a carrier exchange");
			@out.println(cmnt + "              8 = Out-of-Priority Reservoir Storage");
			@out.println(cmnt + "              9 = Reservoir Release for target contents");
			@out.println(cmnt + "              10 = General Replacement Reservoir");
			@out.println(cmnt + "              11 = Direct flow demand thru intervening structures");
			@out.println(cmnt + "              12 = Reoperate");
			@out.println(cmnt + "              13 = Index Flow");
			@out.println(cmnt + "              14 = Similar to 11 but diversions are constrained by demand at carrier structure");
			@out.println(cmnt + "              15 = Interruptible Supply");
			@out.println(cmnt + "              16 = Direct Flow Storage");

			@out.println(cmnt);
			@out.println(cmnt + " *************************************************************************");
			@out.println(cmnt + "     Card 2   Carrier Ditch data (include only if dumx > 0)");
			@out.println(cmnt + "     format:  (free)");
			@out.println(cmnt);
			@out.println(cmnt + "     Inter    itern (1,j)     intervening direct diversion structure id's");
			@out.println(cmnt + "                              Enter # Str values");
			@out.println(cmnt);
			@out.println(cmnt + " ID        Name                    " + "NA          AdminDat  Admin#   # Str  On/Off Dest " + "Id     Dest Ac  Sou1 Id     Sou1 Ac  Sou2 Id     Sou2 Ac     Type");
			@out.println(cmnt + "---------eb----------------------e" + "b----------eb------eb------eb------eb------e-b----------eb------e-b----------eb------e-b------" + "----eb------eb------e");
			@out.println(cmnt + "EndHeader");
			@out.println(cmnt);

			int num = 0;
			if (theOpr != null)
			{
				num = theOpr.Count;
			}
			int num_intern;
			int num_comments;
			int dumx;
			for (int i = 0; i < num ; i++)
			{
				opr = (StateMod_OperationalRight)theOpr[i];
				if (opr == null)
				{
					continue;
				}

				comments_vector = opr.getCommentsBeforeData();
				num_comments = comments_vector.Count;
				// Print the comments in front of the operational right
				for (int j = 0; j < num_comments; j++)
				{
					@out.println("# " + (string)comments_vector[j]);
				}
				// If the operational right was not understood at read, print the original contents
				// and go to the next right.
				IList<string> rightStringsList = opr.getRightStrings();
				if (rightStringsList != null)
				{
					for (int j = 0; j < rightStringsList.Count; j++)
					{
						@out.println(rightStringsList[j]);
					}
					continue;
				}

				v.Clear();
				v.Add(opr.getID());
				v.Add(opr.getName());
				v.Add(opr.getCgoto());
				v.Add(new int?(opr.getDumx()));
				v.Add(new int?(opr.getSwitch()));
				v.Add(opr.getCiopde());
				v.Add(Convert.ToInt32(opr.getIopdes()));
				v.Add(opr.getCiopso1());
				v.Add(Convert.ToInt32(opr.getIopsou1()));
				v.Add(opr.getCiopso2());
				v.Add(Convert.ToInt32(opr.getIopsou2()));
				v.Add(new int?(opr.getItyopr()));
				iline = StringUtil.formatString(v, format);
				@out.println(iline);
				dumx = opr.getDumx();

				if ((dumx == 12) || (dumx < -12))
				{
					if (Message.isDebugOn)
					{
						Message.printDebug(50, routine, "in area 1: getDumx = " + opr.getDumx() + "getItyopr = " + opr.getItyopr());
					}
					vS.Clear();
					for (int j = 0; j < 12; j++)
					{
						vS.Add(new int?(opr.getImonsw(j)));
					}
					iline = StringUtil.formatString(vS, formatS);
					@out.println(iline);
				}

				if (Message.isDebugOn)
				{
					Message.printDebug(50, routine, "in area 3 (" + opr.getID() + "): getDumx = " + opr.getDumx() + ", getItyopr = " + opr.getItyopr());
				}
				if ((dumx > 0 && dumx <= 10) || dumx < -12)
				{
					if (Message.isDebugOn)
					{
						Message.printDebug(50, routine, "in area 2: getDumx = " + opr.getDumx() + "getItyopr = " + opr.getItyopr());
					}
					vsp.Clear();
					vsp.Add(" ");
					iline = StringUtil.formatString(vsp, formatsp);
					@out.print(iline);

					num_intern = opr.getDumx();
					if (opr.getDumx() < -12)
					{
						num_intern = -12 - opr.getDumx();
					}
					for (int j = 0; j < num_intern; j++)
					{
						if (Message.isDebugOn)
						{
							Message.printDebug(50, routine, "in area 3: " + num_intern);
						}
						vI.Clear();
						vI.Add(opr.getIntern(j));
						iline = StringUtil.formatString(vI, formatI);
						@out.print(iline);
					}
					@out.println();
				}
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
				@out.flush();
				@out.close();
			}
		}
	}

	/// <summary>
	/// Write operational right information to output for version 2 format file.  History header information 
	/// is also maintained by calling this routine. </summary>
	/// <param name="infile"> input file from which previous history should be taken </param>
	/// <param name="outfile"> output file to which to write </param>
	/// <param name="theOpr"> vector of operational right to print </param>
	/// <param name="newComments"> addition comments which should be included in history </param>
	/// <param name="dataSet"> StateMod_DataSet, needed to check some relationships during the read (e.g., type 24). </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeStateModFileVersion2(String infile, String outfile, java.util.List<StateMod_OperationalRight> theOpr, java.util.List<String> newComments, StateMod_DataSet dataSet) throws Exception
	public static void writeStateModFileVersion2(string infile, string outfile, IList<StateMod_OperationalRight> theOpr, IList<string> newComments, StateMod_DataSet dataSet)
	{
		PrintWriter @out = null;
		IList<string> commentIndicators = new List<string>(1);
		commentIndicators.Add("#");
		IList<string> ignoredCommentIndicators = new List<string>(1);
		ignoredCommentIndicators.Add("#>");
		string routine = "StateMod_OperationalRight.writeStateModFileVersion2";

		Message.printStatus(1, routine, "Writing new operational rights to file \"" + outfile + "\" using \"" + infile + "\" header...");

		@out = IOUtil.processFileHeaders(infile, outfile, newComments, commentIndicators, ignoredCommentIndicators, 0);
		try
		{
			string cmnt = "#>";
			string iline = null;
			// Note that dumx is output as a string to force the trailing period
			string formatLine1 = "%-12.12s%-24.24s                %12.12s%8.8s%8d %-12.12s%8d %-12.12s%8d %-12.12s%8d" +
				"%8d %-12.12s %-12.12s %8.0f%8.0f%8d%8d";
			// The spaces in the following follow traditional file formatting and allow input and output to be compared
			string formatMonthlySwitches = "                                    %d %d %d %d %d %d %d %d %d %d %d %d";
			string formatsp = "%36.36s";
			string formatI = "%-12.12s";
			StateMod_OperationalRight opr = null;
			IList<object> v = new List<object>(18);
			IList<object> vMonthlySwitches = new List<object>(12);
			IList<object> vsp = new List<object>(1);
			IList<object> vI = new List<object>(1);
			IList<string> commentsBeforeData = null;

			@out.println(cmnt);
			@out.println(cmnt + " *******************************************************");
			 @out.println(cmnt + " Operational Right (Operating Rule) File");
			 @out.println(cmnt + "");
			/*
			 out.println(cmnt);
			out.println(cmnt + "     Card 1   Control");
			out.println(cmnt + "     format:  (a12, a24, 12x, 2i4, i8, f8.0, i8, 3 (i8,a12), 20i8)");
			 out.println(cmnt);
			out.println(cmnt + "     ID       cidvri:          Operational Right ID");
			out.println(cmnt + "     Name     nameo:           Operational Right name");
			out.println(cmnt + "     AdminDat iodat (1-2,k):   Effective priority date");
			out.println(cmnt + "     Admin #  irtem:           Priority Number (smaller is most senior)");
			out.println(cmnt + "     # Str    dumx:            Number of intervenging structures ");
			out.println(cmnt + "     On/Off   ioprsw (k):      Switch 0 = off,1 = on");
			out.println(cmnt + "     Dest ID  ciopde:          Destination reservoir or structure ID");
			out.println(cmnt + "     Dest Ac  iopdes (2,k):    Destination reservoir or structure account # (1 for a diversion)");
			out.println(cmnt + "     Sou1 ID  ciopso (1)       Supply reservoir #1 or structure ID");
			out.println(cmnt + "     Sou1 Ac  iopsou (2,k):    Supply reservoir #1 or structure account # (1 for a diversion)");
			out.println(cmnt + "     Sou2 ID  ciopso (2):      Supply reservoir #2 ID");
			out.println(cmnt + "     Sou1 Ac  iopsou (4,k):    Supply reservoir #2 account");
			out.println(cmnt + "     Type     ityopr (k):      Switch");
	
			out.println(cmnt);
			out.println(cmnt + " *************************************************************************");
			out.println(cmnt + "     Card 2   Carrier Ditch data (include only if dumx > 0)");
			out.println(cmnt + "     format:  (free)");
			out.println(cmnt);
			out.println(cmnt + "     Inter    itern (1,j)     intervening direct diversion structure id's");
			out.println(cmnt + "                              Enter # Str values");
			out.println(cmnt);
			out.println(cmnt + " ID        Name                    "
				+ "NA          AdminDat  Admin#   # Str  On/Off Dest "
				+ "Id     Dest Ac  Sou1 Id     Sou1 Ac  Sou2 Id     Sou2 Ac     Type");
			out.println(cmnt + "---------eb----------------------e"
				+ "b----------eb------eb------eb------eb------e-b----------eb------e-b----------eb------e-b------"
				+ "----eb------eb------e");
			out.println(cmnt + "EndHeader");
			out.println(cmnt);*/

			@out.println(cmnt);
			@out.println(cmnt + "           OPERATING RULE TYPES (types that are not fully handled are read as text by the software)");
			@out.println(cmnt + "  =======================================================================================================================");
			foreach (StateMod_OperationalRight_Metadata metadata in StateMod_OperationalRight_Metadata.getAllMetadata())
			{
				@out.println(cmnt + "	" + StringUtil.formatString(metadata.getRightTypeNumber(),"%2d") + "   " + metadata.getRightTypeName() + " (fully handled=" + (metadata.getFullEditingSupported(dataSet) ? "yes" : "no") + ")");
			}
			@out.println(cmnt + "");
			@out.println(cmnt + "            GUIDE TO COLUMN ENTRIES (see StateMod documentation for details, using variable names)");
			@out.println(cmnt + "  =======================================================================================================================");
			@out.println(cmnt + "   ID         cidvri     Unique identifier for the operating rule, used in output in the *.xop output file");
			@out.println(cmnt + "   Name       nameo      Name of operating rule - used for descriptive purposes only");
			@out.println(cmnt + "   Admin #    irtem      Administration number used to determine priority of operational water rights relative to other");
			@out.println(cmnt + "                         operations and direct diversion, reservoir, instream flow, and well rights");
			@out.println(cmnt + "                         (see tabulation in *.xwr output file)");
			@out.println(cmnt + "   # Str      dumx       Number of carrier structures, monthly on/off switches, or monthly volumetrics");
			@out.println(cmnt + "                         (flag telling StateMod program the number of entries on next lines)");
			@out.println(cmnt + "   On/Off     ioprsw     1 for ON and 0 for OFF");
			@out.println(cmnt + "   Dest ID    ciopde     Destination of operating rule whose demand is to be met by simulating the operating rule");
			@out.println(cmnt + "   Dest Ac    iopdes     Account at destination to be met by operating rule - typically 1 for a diversion structure");
			@out.println(cmnt + "                         and account number for reservoir destination");
			@out.println(cmnt + "   Sou1 ID    ciopso(1)  ID number of primary source of water under which water right is being diverted in operating rule - ");
			@out.println(cmnt + "                         typically a water right, reservoir, or Plan structure ID");
			@out.println(cmnt + "   Sou1 Ac    iopsou(1)  Account of Sou1 - typically 1 for a diversion structure and account number for reservoir source");
			@out.println(cmnt + "   Sou2 ID    ciopso(2)  ID of Plan where reusable storage water or reusable ditch credits is accounted");
			@out.println(cmnt + "   Sou2 Ac    iopsou(2)  Percentage of Plan supplies available for operation");
			@out.println(cmnt + "   Type       ityopr     Rule type corresponding with definitions in StateMod documentation (see list above");
			@out.println(cmnt + "                         Note that this data processing software may not explicitly understand all types.");
			@out.println(cmnt + "                         Other rule types are read as text assuming that each operating rule has comments above the data.");
			@out.println(cmnt + "   ReusePlan  creuse     ID of Plan where reusable return flows or diversions to storage are accounted");
			@out.println(cmnt + "   Div Type   cdivtyp    'Diversion' indicates pro-rata diversion of source water right priority or exchange of reusable credits to Dest1");
			@out.println(cmnt + "                         'Depletion' indicates pro-rata diversion of source water right priority consumptive use or augmentation of upstream diversions at Dest1");
			@out.println(cmnt + "   OprLoss    OprLoss    Percentage of simulated diversion lost in carrier ditch (only applies to certain rules - see StateMod documentation, Section 4.13)");
			@out.println(cmnt + "   Limit      OprLimit   Capacity limit for carrier structures different from capacity in .dds file (used to represent constricted conveyance capacity for winter deliveries to reservoirs)");
			@out.println(cmnt + "   Year1      IoBeg      First year the operating rule is on.");
			@out.println(cmnt + "   Year2      IoEnd      Last year the operating rule is on.");
			@out.println(cmnt + "");
			@out.println(cmnt + " Note - StateMod supports several *.opr file format versions.  The following string indicates the version for this file:");
			@out.println(cmnt + "");
			@out.println(cmnt + " FileFormatVersion 2");
			@out.println(cmnt + "");
			@out.println(cmnt + " If the format version indicator is not provided StateMod will try to determine the version.");
			@out.println(cmnt + "");
			@out.println(cmnt + " Card 1 format:  a12,a24,12x,4x,f12.5,f8.0,i8,3(1x,a12,i8),i8,1x,a12,1x,a12,1x,2f8.0,2i8");
			@out.println(cmnt + "");
			@out.println(cmnt + " ID        Name                    NA                    Admin#   # Str  On/Off Dest Id     Dest Ac  Sou1 Id     Sou1 Ac  Sou2 Id     Sou2 Ac     Type ReusePlan    Div Type      OprLoss   Limit   Year1   Year 2");
			@out.println(cmnt + " ---------eb----------------------eb----------exxxxb----------eb------eb------e-b----------eb------e-b----------eb------e-b----------eb------eb------exb----------exb----------exb------eb------exb------eb--------e");
			@out.println(cmnt + "EndHeader");
			@out.println(cmnt);

			int num = 0;
			if (theOpr != null)
			{
				num = theOpr.Count;
			}
			int num_intern;
			int numComments;
			int dumx;
			int rightType;
			double oprLimit;
			StateMod_OperationalRight_Metadata metadata;
			for (int i = 0; i < num ; i++)
			{
				opr = theOpr[i];
				if (opr == null)
				{
					continue;
				}
				rightType = opr.getItyopr();

				commentsBeforeData = opr.getCommentsBeforeData();
				numComments = commentsBeforeData.Count;
				// Print the comments in front of the operational right
				// The original comments were stripped of the leading # but otherwise are padded with whitespace
				// as per the original - when written they should exactly match the original
				for (int j = 0; j < numComments; j++)
				{
					@out.println("#" + commentsBeforeData[j]);
				}
				metadata = opr.getMetadata();
				if (!isRightUnderstoodByCode(opr.getItyopr(),dataSet) || (opr.getReadErrors().Count > 0))
				{
					// The operational right is not explicitly understood so print the original contents
					// and go to the next right
					IList<string> rightStringsList = opr.getRightStrings();
					if (rightStringsList != null)
					{
						for (int j = 0; j < rightStringsList.Count; j++)
						{
							@out.println(rightStringsList[j]);
						}
					}
				}
				else
				{
					// Print the rights details using the in-memory information
					v.Clear();
					v.Add(opr.getID());
					v.Add(opr.getName());
					v.Add(opr.getRtem());
					// Dumx is handled as a float even though it is documented as having integer values
					// Traditionally it has a period in the files (e.g., "12.").  Therefore, force the period here
					string dumxString = "" + opr.getDumx() + ".";
					v.Add(dumxString);
					v.Add(new int?(opr.getSwitch()));
					v.Add(opr.getCiopde());
					v.Add(opr.getIopdes());
					v.Add(opr.getCiopso1());
					v.Add(opr.getIopsou1());
					v.Add(opr.getCiopso2());
					v.Add(opr.getIopsou2());
					v.Add(new int?(rightType));
					v.Add(opr.getCreuse());
					v.Add(opr.getCdivtyp());
					v.Add(new double?(opr.getOprLoss()));
					oprLimit = opr.getOprLimit();
					v.Add(new double?(oprLimit));
					v.Add(new int?(opr.getIoBeg()));
					v.Add(new int?(opr.getIoEnd()));
					iline = StringUtil.formatString(v, formatLine1);
					@out.println(iline);
					dumx = opr.getDumx();

					// Write records before monthly switches and intervening structures

					if (metadata.getRightTypeUsesRioGrande())
					{
						@out.println("                                                                " + StringUtil.formatString(opr.getQdebt(), "%7.0f.") + StringUtil.formatString(opr.getQdebtx(), "%7.0f.") + " " + StringUtil.formatString(opr.getCiopso3(), "%-12.12s") + StringUtil.formatString(opr.getIopsou3(), "%8.8s") + " " + StringUtil.formatString(opr.getCiopso4(), "%-12.12s") + StringUtil.formatString(opr.getIopsou4(), "%8.8s") + " " + StringUtil.formatString(opr.getCiopso5(), "%-12.12s") + StringUtil.formatString(opr.getIopsou5(), "%8.8s"));
					}
					if (metadata.getRightTypeUsesSanJuan())
					{
						@out.println("                                                                " + StringUtil.formatString(opr.getSjmina(), "%8.0f") + StringUtil.formatString(opr.getSjrela(), "%8.0f"));
					}

					// This code is the same as the readStateModFileVersion2()...

					// Start reading the monthly and intervening structure data - first split the "dumx"
					// value into parts...
					int nmonsw = 0;
					int ninterv = 0;
					// Special case for type 17 and 18, where -8 means no monthly switches and -20 = use switches
					if ((rightType == 17) || (rightType == 18))
					{
						nmonsw = 0; // Default
						ninterv = 0;
						if (dumx == -20)
						{
							nmonsw = 12;
						}
					}
					else
					{
						// Normal interpretation of dumx
						if (dumx == 12)
						{
							// Only have monthly switches
							nmonsw = 12;
						}
						else if (dumx >= 0)
						{
							// Only have intervening structures...
							ninterv = dumx;
						}
						else if (dumx < 0)
						{
							// Have monthly switches and intervening structures.
							// -12 of the total count toward the monthly switch and the remainder is
							// the number of intervening structures
							if (dumx < -12)
							{
								ninterv = -1 * (dumx + 12);
								nmonsw = 12;
							}
							else
							{
								ninterv = -1 * dumx;
							}
						}
					}

					// Write the monthly switches if used

					if (metadata.getRightTypeUsesMonthlySwitch())
					{
						if (nmonsw == 12)
						{
							vMonthlySwitches.Clear();
							for (int j = 0; j < 12; j++)
							{
								vMonthlySwitches.Add(new int?(opr.getImonsw(j)));
							}
							iline = StringUtil.formatString(vMonthlySwitches, formatMonthlySwitches);
							@out.println(iline);
						}
					}

					// Write the intervening structures (without loss) if used

					if (metadata.getRightTypeUsesInterveningStructuresWithoutLoss())
					{
						if (ninterv > 0)
						{
							vsp.Clear();
							vsp.Add(" ");
							StringBuilder b = new StringBuilder();
							b.Append(StringUtil.formatString(vsp, formatsp));
							for (int j = 0; j < ninterv; j++)
							{
								vI.Clear();
								vI.Add(opr.getIntern(j));
								b.Append(StringUtil.formatString(vI, formatI));
							}
							@out.println(b.ToString());
						}
					}

					// Write associated operating rule if used

					if (metadata.getRightTypeUsesAssociatedOperatingRule(opr.getOprLimit()))
					{
						@out.println("                                    " + opr.getCx());
					}

					// Write the intervening structures (with loss) if used

					if (metadata.getRightTypeUsesInterveningStructuresWithLoss(opr.getOprLoss()))
					{
						StringBuilder b = new StringBuilder();
						for (int j = 0; j < ninterv; j++)
						{
							b.Length = 0;
							b.Append("                                   ");
							b.Append(StringUtil.formatString(opr.getIntern(i), "%-12.12s"));
							b.Append(" ");
							b.Append(StringUtil.formatString(opr.getOprLossC(i), "%8.0f"));
							b.Append(" ");
							b.Append(StringUtil.formatString(opr.getInternT(i), "%s"));
							@out.println(b.ToString());
						}
					}

					// Write operating limits if used

					if (metadata.getRightTypeUsesMonthlyOprMax(opr.getOprLimit()))
					{
						for (int iLim = 0; iLim < 13; iLim++)
						{
							@out.println(StringUtil.formatString(opr.getOprMax(iLim), "%8.0f"));
						}
					}

					// Write efficiency if used

					if (metadata.getRightTypeUsesMonthlyOprEff(dataSet, opr.getCiopso2(), opr.getIopsou2()))
					{
						for (int iEff = 0; iEff < 12; iEff++)
						{
							@out.println(StringUtil.formatString(opr.getOprEff(iEff), "%8.2f"));
						}
					}
				}
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
				@out.flush();
				@out.close();
			}
		}
	}

	/// <summary>
	/// WRite the StateMod operational rights file monthly and annual limitation data. </summary>
	/// <param name="routine"> to use for logging. </param>
	/// <param name="linecount"> Line count (1+) before writing in this method. </param>
	/// <param name="out"> BufferedWriter to write. </param>
	/// <param name="anOprit"> Operational right for which to write data. </param>
	/// <returns> the number of errors. </returns>
	/// <exception cref="IOException"> if there is an error writing the file </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static int writeStateModFile_MonthlyAndAnnualLimitationData(String routine, java.io.PrintWriter out, StateMod_OperationalRight anOprit) throws java.io.IOException
	private static int writeStateModFile_MonthlyAndAnnualLimitationData(string routine, PrintWriter @out, StateMod_OperationalRight anOprit)
	{ // Single identifier
		int errorCount = 0;
		try
		{
			@out.println("                                    " + anOprit.getCx().Trim());
		}
		catch (Exception)
		{
			// TODO SAM 2010-12-13 Need to handle errors and provide feedback
			Message.printWarning(3, routine, "Error writing monthly and annual limitation for right \"" + anOprit.getID() + "\"");
		}
		return errorCount;
	}

	}

}