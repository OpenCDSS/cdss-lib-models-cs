using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

// StateMod_ReservoirRight - class to store reservoir rights

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
// StateMod_ReservoirRight - Derived from StateMod_Data class
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
// 
// 02 Sep 1997	Catherine E.		Created initial version of class.
//		Nutting-Lane, RTi
// 21 Dec 1998	CEN, RTi		Added throws IOException to read/write
//					routines.
// 25 Jun 2000	Steven A. Malers, RTi	Make so that res rights < 1.0 are
//					printed with 8.2 precision.  This is an
//					issue in the Rio Grande for very small
//					rights(e.g., stock ponds).
// 17 Feb 2001	SAM, RTi		Code review.  Add finalize().  Clean up
//					javadoc.  Handle nulls and set unused
//					variables to null.  Alphabetize methods.
//					Change so right admin numbers are right
//					justified but add option to print old
//					style.  Change IO to IOUtil.
// 02 Mar 2001	SAM, RTi		Ray says to use F16.5 for water rights
//					and get rid of the 4X.
// 2001-12-27	SAM, RTi		Update to use new fixedRead()to
//					improve performance.
// 2002-09-19	SAM, RTi		Use isDirty()instead of setDirty()to
//					indicate edits.
//------------------------------------------------------------------------------
// 2003-06-04	J. Thomas Sapienza, Rti	Renamed from SMResRights to 
//					StateMod_ReservoirRight
// 2003-06-10	JTS, RTi		* Folded dumpReservoirRightsFile() into
//					  writeReservoirRightsFile()
//					* Renamed parseReservoirRightsFile() to
//					  readReservoirRightsFile()
// 2003-06-23	JTS, RTi		Renamed writeReservoirRightsFile() to
//					writeStateModFile()
// 2003-06-26	JTS, RTi		Renamed readReservoirRightsFile() to
//					readStateModFile()
// 2003-07-15	JTS, RTi		Changed to use new dataset design.
// 2003-08-03	SAM, RTi		Change isDirty() back to setDirty().
// 2003-08-28	SAM, RTi		* Remove linked list data since a
//					  Vector of rights is maintained in the
//					  StateMod_Reservoir.
//					* Clean up parameters to methods to be
//					  clearer.
//					* Alphabetize methods.
// 2003-10-09	JTS, RTi		* Implemented Cloneable.
//					* Added clone().
//					* Added equals().
//					* Implemented Comparable.
//					* Added compareTo().
// 					* Added equals(Vector, Vector)
// 2003-10-15	JTS, RTi		* Revised the clone() code.
//					* Added toString().
// 2003-10-15	SAM, RTi		Changed some initial values to agree
//					with the old GUI for new instances.
// 2004-07-08	SAM, RTi		* Add getIrescoChoices() and
//					  getIrescoDefault() for use by GUIs.
//					* Add getItyrsrChoices() and
//					  getItyrsrDefault().
//					* Add getN2fillChoices() and
//					  getN2fillDefault().
// 2004-09-14	SAM, RTi		Open files considering the working
//					directory.
// 2004-10-28	SAM, RTi		Add getIrsrswChoices() and
//					getIrsrswDefault().
// 2004-10-29	SAM, RTi		Remove temporary data members used with
//					table model.  Displaying the expanded
//					strings only in the dropdown is OK and
//					makes the code simpler.
// 2004-11-11	SAM, RTi		Fix getIrescoChoices() - was showing
//					a negative number in the note.
// 2005-01-17	JTS, RTi		* Added createBackup().
//					* Added restoreOriginal().
// 2005-03-14	SAM, RTi		Clarify output header for switch.
// 2005-04-18	JTS, RTi		Added writeListFile().
// 2007-04-12	Kurt Tometich, RTi		Added checkComponentData() and
//									getDataHeader() methods for check
//									file and data check support.
// 2007-05-16	SAM, RTi		Implement StateMod_Right interface.
//------------------------------------------------------------------------------
// EndHeader

namespace DWR.StateMod
{

	using DataSetComponent = RTi.Util.IO.DataSetComponent;
	using IOUtil = RTi.Util.IO.IOUtil;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;

	public class StateMod_ReservoirRight : StateMod_Data, ICloneable, IComparable<StateMod_Data>, StateMod_ComponentValidator, StateMod_Right
	{
	/// <summary>
	/// Administration number
	/// </summary>
	protected internal string _rtem;
	/// <summary>
	/// Decreed amount
	/// </summary>
	protected internal double _dcrres;
	/// <summary>
	/// Filling ratio
	/// </summary>
	protected internal int _iresco;
	/// <summary>
	/// Reservoir type
	/// </summary>
	protected internal int _ityrstr;
	/// <summary>
	/// Reservoir right type
	/// </summary>
	protected internal int _n2fill;
	/// <summary>
	/// out of priority associated op right
	/// </summary>
	protected internal string _copid;

	/// <summary>
	/// Constructor.
	/// </summary>
	public StateMod_ReservoirRight() : base()
	{
		initialize();
	}

	/// <summary>
	/// Clones the data object. </summary>
	/// <returns> a cloned object. </returns>
	public override object clone()
	{
		StateMod_ReservoirRight right = (StateMod_ReservoirRight)base.clone();
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

		StateMod_ReservoirRight right = (StateMod_ReservoirRight)data;

		res = _rtem.CompareTo(right.getRtem());
		if (res != 0)
		{
			return res;
		}

		if (_dcrres < right._dcrres)
		{
			return -1;
		}
		else if (_dcrres > right._dcrres)
		{
			return 1;
		}

		if (_iresco < right._iresco)
		{
			return -1;
		}
		else if (_iresco > right._iresco)
		{
			return 1;
		}

		if (_ityrstr < right._ityrstr)
		{
			return -1;
		}
		else if (_ityrstr > right._ityrstr)
		{
			return 1;
		}

		if (_n2fill < right._n2fill)
		{
			return -1;
		}
		else if (_n2fill > right._n2fill)
		{
			return 1;
		}

		res = _copid.CompareTo(right._copid);
		return res;
	}

	/// <summary>
	/// Creates a backup of the current data object and stores it in _original,
	/// for use in determining if an object was changed inside of a GUI.
	/// </summary>
	public virtual void createBackup()
	{
		_original = (StateMod_ReservoirRight)clone();
		((StateMod_ReservoirRight)_original)._isClone = false;
		_isClone = true;
	}

	/// <summary>
	/// Compare two rights lists and see if they are the same. </summary>
	/// <param name="v1"> the first list of StateMod_ReservoirRight s to check.  Cannot be null. </param>
	/// <param name="v2"> the second list of StateMod_ReservoirRight s to check.  Cannot be null. </param>
	/// <returns> true if they are the same, false if not. </returns>
	public static bool Equals(IList<StateMod_ReservoirRight> v1, IList<StateMod_ReservoirRight> v2)
	{
		StateMod_ReservoirRight r1;
		StateMod_ReservoirRight r2;
		if (v1.Count != v2.Count)
		{
			//Message.printStatus(2, routine, "Lists are different sizes");
			return false;
		}
		else
		{
			// sort the lists and compare item-by-item.  Any differences
			// and data will need to be saved back into the dataset.
			int size = v1.Count;
			//Message.printStatus(2, routine, "Lists are of size: " + size);
			IList<StateMod_ReservoirRight> v1Sort = StateMod_Util.sortStateMod_DataVector(v1);
			IList<StateMod_ReservoirRight> v2Sort = StateMod_Util.sortStateMod_DataVector(v2);
			//Message.printStatus(2, routine, "Lists have been sorted");

			for (int i = 0; i < size; i++)
			{
				r1 = v1Sort[i];
				r2 = v2Sort[i];
				//Message.printStatus(2, routine, r1.toString());
				//Message.printStatus(2, routine, r2.toString());
				//Message.printStatus(2, routine, "Element " + i + " comparison: " + r1.compareTo(r2));
				if (r1.CompareTo(r2) != 0)
				{
					return false;
				}
			}
		}
		return true;
	}

	/// <summary>
	/// Tests to see if two reservoir rights are equal.  Strings are compared with case sensitivity. </summary>
	/// <param name="right"> the right to compare. </param>
	/// <returns> true if they are equal, false otherwise. </returns>
	public virtual bool Equals(StateMod_ReservoirRight right)
	{
		 if (!base.Equals(right))
		 {
			 return false;
		 }

		if (right._rtem.Equals(_rtem) && right._dcrres == _dcrres && right._iresco == _iresco && right._ityrstr == _ityrstr && right._n2fill == _n2fill && right._copid.Equals(_copid))
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Clean up before garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~StateMod_ReservoirRight()
	{
		_rtem = null;
		_copid = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Return the administration number, as per the generic interface. </summary>
	/// <returns> the administration number, as a String to protect from roundoff. </returns>
	public virtual string getAdministrationNumber()
	{
		return getRtem();
	}

	/// <summary>
	/// Return the out-of-priority associated op right.
	/// </summary>
	public virtual string getCopid()
	{
		return _copid;
	}

	/// <summary>
	/// Returns the data column header for the specifically checked data. </summary>
	/// <returns> Data column header. </returns>
	public static string[] getDataHeader()
	{
		// TODO KAT 2007-04-16 When specific checks are added to checkComponentData
		// return the header for that data here
		return new string[] {};
	}

	/// <summary>
	/// Return the decreed amount.
	/// </summary>
	public virtual double getDcrres()
	{
		return _dcrres;
	}

	/// <summary>
	/// Return the decree, as per the generic interface. </summary>
	/// <returns> the decree, in the units of the data. </returns>
	public virtual double getDecree()
	{
		return getDcrres();
	}

	// TODO SAM 2007-05-15 Need to evaluate whether should be hard-coded.
	/// <summary>
	/// Return the decree units. </summary>
	/// <returns> the decree units. </returns>
	public virtual string getDecreeUnits()
	{
		return "ACFT";
	}

	/// <summary>
	/// Return the right identifier, as per the generic interface. </summary>
	/// <returns> the right identifier. </returns>
	public virtual string getIdentifier()
	{
		return getID();
	}

	/// <summary>
	/// Return the filling ratio.
	/// </summary>
	public virtual int getIresco()
	{
		return _iresco;
	}

	/// <summary>
	/// Return a list of account distribution switch option strings, for use in GUIs.
	/// The options are of the form "1" if include_notes is false and
	/// "1 - Account to be served by right", if include_notes is true. </summary>
	/// <returns> a list of on/off switch option strings, for use in GUIs. </returns>
	/// <param name="include_notes"> Indicate whether notes should be added after the parameter values. </param>
	public static IList<string> getIrescoChoices(bool include_notes)
	{
		IList<string> v = new List<string>(102); // Allow for one blank in StateDMI
		for (int i = 1; i <= 50; i++)
		{
			v.Add("" + i + " - Account served by right");
		}
		for (int i = -1; i >= -50; i--)
		{
			v.Add("" + i + " - Fill first " + (-i) + " accounts");
		}
		if (!include_notes)
		{
			// Remove the trailing notes...
			int size = v.Count;
			for (int i = 0; i < size; i++)
			{
				v[i] = StringUtil.getToken(v[i], " ", 0, 0);
			}
		}
		return v;
	}

	/// <summary>
	/// Return the default account distribution switch choice.  This can be used by GUI
	/// code to pick a default for a new reservoir. </summary>
	/// <returns> the default reservoir account distribution. </returns>
	public static string getIrescoDefault(bool include_notes)
	{ // Make this aggree with the above method...
		if (include_notes)
		{
			return ("1 - Account served by right");
		}
		else
		{
			return "1";
		}
	}

	/// <summary>
	/// Return a list of on/off switch option strings, for use in GUIs.
	/// The options are of the form "0" if include_notes is false and "0 - Off", if include_notes is true. </summary>
	/// <returns> a list of on/off switch option strings, for use in GUIs. </returns>
	/// <param name="include_notes"> Indicate whether notes should be added after the parameter values. </param>
	public static IList<string> getIrsrswChoices(bool include_notes)
	{
		return StateMod_DiversionRight.getIdvrswChoices(include_notes);
	}

	/// <summary>
	/// Return the default on/off switch choice.  This can be used by GUI code
	/// to pick a default for a new reservoir. </summary>
	/// <returns> the default reservoir on/off choice. </returns>
	public static string getIrsrswDefault(bool include_notes)
	{
		return StateMod_DiversionRight.getIdvrswDefault(include_notes);
	}

	/// <summary>
	/// Return the reservoir type.
	/// </summary>
	public virtual int getItyrstr()
	{
		return _ityrstr;
	}

	/// <summary>
	/// Return a list of right type option strings, for use in GUIs.
	/// The options are of the form "1" if include_notes is false and
	/// "1 - Standard", if include_notes is true. </summary>
	/// <returns> a list of fill switch option strings, for use in GUIs. </returns>
	/// <param name="include_notes"> Indicate whether notes should be added after the parameter values. </param>
	public static IList<string> getItyrsrChoices(bool include_notes)
	{
		IList<string> v = new List<string>(2);
		v.Add("1 - Standard"); // Possible options are listed here.
		v.Add("-1 - Out of priority water right");
		if (!include_notes)
		{
			// Remove the trailing notes...
			int size = v.Count;
			for (int i = 0; i < size; i++)
			{
				v[i] = StringUtil.getToken(v[i], " ", 0, 0);
			}
		}
		return v;
	}

	/// <summary>
	/// Return the default right type choice.  This can be used by GUI code
	/// to pick a default for a new reservoir. </summary>
	/// <returns> the default right type choice. </returns>
	public static string getItyrsrDefault(bool include_notes)
	{ // Make this agree with the above method...
		if (include_notes)
		{
			return ("1 - Standard");
		}
		else
		{
			return "1";
		}
	}

	/// <summary>
	/// Return the right location identifier, as per the generic interface. </summary>
	/// <returns> the right location identifier (location where right applies). </returns>
	public virtual string getLocationIdentifier()
	{
		return getCgoto();
	}

	/// <summary>
	/// Retrieve the reservoir right type.
	/// </summary>
	public virtual int getN2fill()
	{
		return _n2fill;
	}

	/// <summary>
	/// Return a list of fill switch option strings, for use in GUIs.
	/// The options are of the form "1" if include_notes is false and
	/// "1 - First fill", if include_notes is true. </summary>
	/// <returns> a list of fill switch option strings, for use in GUIs. </returns>
	/// <param name="include_notes"> Indicate whether notes should be added after the parameter values. </param>
	public static IList<string> getN2fillChoices(bool include_notes)
	{
		IList<string> v = new List<string>(2);
		v.Add("1 - First fill"); // Possible options are listed here.
		v.Add("2 - Second fill");
		if (!include_notes)
		{
			// Remove the trailing notes...
			int size = v.Count;
			for (int i = 0; i < size; i++)
			{
				v[i] = StringUtil.getToken(v[i], " ", 0, 0);
			}
		}
		return v;
	}

	/// <summary>
	/// Return the default fill type choice.  This can be used by GUI code to pick a default for a new reservoir. </summary>
	/// <returns> the default fill type choice. </returns>
	public static string getN2fillDefault(bool include_notes)
	{ // Make this agree with the above method...
		if (include_notes)
		{
			return ("1 - First fill");
		}
		else
		{
			return "1";
		}
	}

	/// <summary>
	/// Return the administration number.
	/// </summary>
	public virtual string getRtem()
	{
		return _rtem;
	}

	/// <summary>
	/// INitialize data members
	/// </summary>
	private void initialize()
	{
		_smdata_type = StateMod_DataSet.COMP_RESERVOIR_RIGHTS;
		_rtem = "99999"; // Default as per old SMGUI.
		_copid = "";
		_dcrres = 0;
		_iresco = 1; // Server first account, as per old SMGUI default
		_ityrstr = 1;
		_n2fill = 1;
	}


	/// <summary>
	/// Determine whether a file is a reservoir right file.  Currently true is returned
	/// if the file extension is ".rer". </summary>
	/// <param name="filename"> Name of the file being checked. </param>
	/// <returns> true if the file is a StateMod reservoir right file. </returns>
	public static bool isReservoirRightFile(string filename)
	{
		if (filename.ToUpper().EndsWith(".RER", StringComparison.Ordinal))
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Read reservoir right information in and store in a list. </summary>
	/// <param name="filename"> Name of file to read. </param>
	/// <returns> list of reservoir right data </returns>
	/// <exception cref="Exception"> if there is an error reading the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.List<StateMod_ReservoirRight> readStateModFile(String filename) throws Exception
	public static IList<StateMod_ReservoirRight> readStateModFile(string filename)
	{
		string routine = "StateMod_ReservoirRight.readStateModFile";
		IList<StateMod_ReservoirRight> theRights = new List<StateMod_ReservoirRight>();
		int[] format_0 = new int[] {StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_DOUBLE, StringUtil.TYPE_INTEGER, StringUtil.TYPE_INTEGER, StringUtil.TYPE_INTEGER, StringUtil.TYPE_INTEGER, StringUtil.TYPE_STRING};
		int[] format_0w = new int[] {12, 24, 12, 16, 8, 8, 8, 8, 8, 12};
		string iline = null;
		IList<object> v = new List<object>(10);
		StreamReader @in = null;
		StateMod_ReservoirRight aRight = null;

		Message.printStatus(2, routine, "Reading reservoir rights file: " + filename);

		try
		{
			@in = new StreamReader(IOUtil.getPathUsingWorkingDir(filename));
			while (!string.ReferenceEquals((iline = @in.ReadLine()), null))
			{
				if (iline.StartsWith("#", StringComparison.Ordinal) || iline.Trim().Length == 0)
				{
					continue;
				}

				aRight = new StateMod_ReservoirRight();

				StringUtil.fixedRead(iline, format_0, format_0w, v);
				if (Message.isDebugOn)
				{
					Message.printDebug(50, routine, "iline: " + iline);
				}
				aRight.setID(((string)v[0]).Trim());
				aRight.setName(((string)v[1]).Trim());
				aRight.setCgoto(((string)v[2]).Trim());
				aRight.setRtem(((string)v[3]).Trim());
				aRight.setDcrres((double?)v[4]);
				aRight.setSwitch((int?)v[5]);
				aRight.setIresco((int?)v[6]);
				aRight.setItyrstr((int?)v[7]);
				aRight.setN2fill((int?)v[8]);
				aRight.setCopid(((string)v[9]).Trim());
				theRights.Add(aRight);
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
		}
		return theRights;
	}

	/// <summary>
	/// Cancels any changes made to this object within a GUI since createBackup()
	/// was called and sets _original to null.
	/// </summary>
	public override void restoreOriginal()
	{
		StateMod_ReservoirRight r = (StateMod_ReservoirRight)_original;
		base.restoreOriginal();
		_rtem = r._rtem;
		_dcrres = r._dcrres;
		_iresco = r._iresco;
		_ityrstr = r._ityrstr;
		_n2fill = r._n2fill;
		_copid = r._copid;
		_isClone = false;
		_original = null;
	}

	/// <summary>
	/// Set the out-of-priority associated op right.
	/// </summary>
	public virtual void setCopid(string copid)
	{
		if ((!string.ReferenceEquals(copid, null)) && !copid.Equals(_copid))
		{
			_copid = copid;
			setDirty(true);
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_RESERVOIR_RIGHTS, true);
			}
		}
	}

	/// <summary>
	/// Set the decreed amount.
	/// </summary>
	public virtual void setDcrres(double dcrres)
	{
		if (dcrres != _dcrres)
		{
			_dcrres = dcrres;
			setDirty(true);
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_RESERVOIR_RIGHTS, true);
			}
		}
	}

	/// <summary>
	/// Set the decreed amount.
	/// </summary>
	public virtual void setDcrres(double? dcrres)
	{
		setDcrres(dcrres.Value);
	}

	/// <summary>
	/// Set the decreed amount.
	/// </summary>
	public virtual void setDcrres(string dcrres)
	{
		if (!string.ReferenceEquals(dcrres, null))
		{
			setDcrres(StringUtil.atod(dcrres.Trim()));
		}
	}

	/// <summary>
	/// Set the decree, as per the generic interface. </summary>
	/// <param name="decree"> decree, in the units of the data. </param>
	public virtual void setDecree(double decree)
	{
		setDcrres(decree);
	}

	/// <summary>
	/// Set the filling ratio.
	/// </summary>
	public virtual void setIresco(int iresco)
	{
		if (iresco != _iresco)
		{
			_iresco = iresco;
			setDirty(true);
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_RESERVOIR_RIGHTS, true);
			}
		}
	}

	/// <summary>
	/// Set the filling ratio.
	/// </summary>
	public virtual void setIresco(int? iresco)
	{
		setIresco(iresco.Value);
	}

	/// <summary>
	/// Set the filling ratio.
	/// </summary>
	public virtual void setIresco(string iresco)
	{
		if (!string.ReferenceEquals(iresco, null))
		{
			setIresco(StringUtil.atoi(iresco.Trim()));
		}
	}

	/// <summary>
	/// Set the reservoir type.
	/// </summary>
	public virtual void setItyrstr(int ityrstr)
	{
		if (ityrstr != _ityrstr)
		{
			_ityrstr = ityrstr;
			setDirty(true);
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_RESERVOIR_RIGHTS, true);
			}
		}
	}

	/// <summary>
	/// Set the reservoir type.
	/// </summary>
	public virtual void setItyrstr(int? ityrstr)
	{
		setItyrstr(ityrstr.Value);
	}

	/// <summary>
	/// Set the reservoir type.
	/// </summary>
	public virtual void setItyrstr(string ityrstr)
	{
		if (!string.ReferenceEquals(ityrstr, null))
		{
			setItyrstr(StringUtil.atoi(ityrstr.Trim()));
		}
	}

	/// <summary>
	/// Set the reservoir right type.
	/// </summary>
	public virtual void setN2fill(int n2fill)
	{
		if (n2fill != _n2fill)
		{
			_n2fill = n2fill;
			setDirty(true);
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_RESERVOIR_RIGHTS, true);
			}
		}
	}

	/// <summary>
	/// Set the reservoir right type.
	/// </summary>
	public virtual void setN2fill(int? n2fill)
	{
		setN2fill(n2fill.Value);
	}

	/// <summary>
	/// Set the reservoir right type.
	/// </summary>
	public virtual void setN2fill(string n2fill)
	{
		if (!string.ReferenceEquals(n2fill, null))
		{
			setN2fill(StringUtil.atoi(n2fill.Trim()));
		}
	}

	/// <summary>
	/// Set the administration number.
	/// </summary>
	public virtual void setRtem(string rtem)
	{
		if ((!string.ReferenceEquals(rtem, null)) && !rtem.Equals(_rtem))
		{
			_rtem = rtem;
			setDirty(true);
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_RESERVOIR_RIGHTS, true);
			}
		}
	}

	/// <param name="dataset"> StateMod dataset object. </param>
	/// <returns> validation results. </returns>
	public virtual StateMod_ComponentValidation validateComponent(StateMod_DataSet dataset)
	{
		StateMod_ComponentValidation validation = new StateMod_ComponentValidation();
		string id = getID();
		string name = getName();
		string cgoto = getCgoto();
		string irtem = getRtem();
		double dcrres = getDcrres();
		int iresco = getIresco();
		int ityrsr = getItyrstr();
		int n2fill = getN2fill();
		string copid = getCopid();
		// Make sure that basic information is not empty
		if (StateMod_Util.isMissing(id))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Reservoir right identifier is blank.", "Specify a reservoir right identifier."));
		}
		if (StateMod_Util.isMissing(name))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Reservoir right \"" + id + "\" name is blank.", "Specify a reservoir right name to clarify data."));
		}
		if (StateMod_Util.isMissing(cgoto))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Reservoir right \"" + id + "\" reservoir station ID is blank.", "Specify a reservoir station to associate with the reservoir right."));
		}
		else
		{
			// Verify that the reservoir station is in the data set, if the network is available
			DataSetComponent comp2 = dataset.getComponentForComponentType(StateMod_DataSet.COMP_RESERVOIR_STATIONS);
			System.Collections.IList resList = (System.Collections.IList)comp2.getData();
			if ((resList != null) && (resList.Count > 0))
			{
				if (StateMod_Util.IndexOf(resList, cgoto) < 0)
				{
					validation.add(new StateMod_ComponentValidationProblem(this,"Reservoir right \"" + id + "\" associated reservoir (" + cgoto + ") is not found in the list of reservoir stations.", "Specify a valid reservoir station ID to associate with the reservoir right."));
				}
			}
		}
		if (StateMod_Util.isMissing(irtem))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Reservoir right \"" + id + "\" administration number is blank.", "Specify an administration number NNNNN.NNNNN."));
		}
		else if (!StringUtil.isDouble(irtem))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Reservoir right \"" + id + "\" administration number (" + irtem + ") is invalid.", "Specify an administration number NNNNN.NNNNN."));
		}
		else
		{
			double irtemd = double.Parse(irtem);
			if (irtemd < 0)
			{
				validation.add(new StateMod_ComponentValidationProblem(this,"Reservoir right \"" + id + "\" administration number (" + irtem + ") is invalid.", "Specify an administration number NNNNN.NNNNN."));
			}
		}
		if (!(dcrres >= 0.0))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Reservoir right \"" + id + "\" decree (" + StringUtil.formatString(dcrres,"%.2f") + ") is invalid.", "Specify the decree as a number >= 0."));
		}
		IList<string> choices = getIrescoChoices(false);
		if (StringUtil.indexOfIgnoreCase(choices,"" + iresco) < 0)
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Reservoir \"" + id + "\" ownership code (" + iresco + ") is invalid.", "Specify the ownership code as one of " + choices));
		}
		choices = getItyrsrChoices(false);
		if (StringUtil.indexOfIgnoreCase(choices,"" + ityrsr) < 0)
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Reservoir \"" + id + "\" right type (" + ityrsr + ") is invalid.", "Specify the right type as one of " + choices));
		}
		choices = getN2fillChoices(false);
		if (StringUtil.indexOfIgnoreCase(choices,"" + n2fill) < 0)
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Reservoir \"" + id + "\" first fill type (" + n2fill + ") is invalid.", "Specify the first fill type as one of " + choices));
		}
		if (ityrsr == -1)
		{
			if (StateMod_Util.isMissing(copid))
			{
				validation.add(new StateMod_ComponentValidationProblem(this, "Reservoir right out-of-priority associated operational right identifier is blank.", "Specify an operational right identifier."));
			}
		}
		return validation;
	}

	/// <summary>
	/// Write reservoir right information to output.  History header information 
	/// is also maintained by calling this routine. </summary>
	/// <param name="infile"> input file from which previous history should be taken </param>
	/// <param name="outfile"> output file to which to write </param>
	/// <param name="theRights"> vector of reservoir right to print </param>
	/// <param name="newComments"> addition comments which should be included in history </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeStateModFile(String infile, String outfile, java.util.List<StateMod_ReservoirRight> theRights, java.util.List<String> newComments) throws Exception
	public static void writeStateModFile(string infile, string outfile, IList<StateMod_ReservoirRight> theRights, IList<string> newComments)
	{
		writeStateModFile(infile, outfile, theRights, newComments, false);
	}

	/// <summary>
	/// Write reservoir right information to output.  History header information 
	/// is also maintained by calling this routine. </summary>
	/// <param name="infile"> input file from which previous history should be taken </param>
	/// <param name="outfile"> output file to which to write </param>
	/// <param name="theRights"> list of reservoir right to print </param>
	/// <param name="newComments"> addition comments which should be included in history </param>
	/// <param name="oldAdminNumFormat"> whether to use the old admin num format or not </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeStateModFile(String infile, String outfile, java.util.List<StateMod_ReservoirRight> theRights, java.util.List<String> newComments, boolean oldAdminNumFormat) throws Exception
	public static void writeStateModFile(string infile, string outfile, IList<StateMod_ReservoirRight> theRights, IList<string> newComments, bool oldAdminNumFormat)
	{
		IList<string> commentIndicators = new List<string>(1);
		commentIndicators.Add("#");
		IList<string> ignoredCommentIndicators = new List<string>(1);
		ignoredCommentIndicators.Add("#>");
		string routine = "StateMod_ReservoirRight.writeStateModFile";
		PrintWriter @out = null;

		if (Message.isDebugOn)
		{
			Message.printDebug(2, routine, "Writing reservoir rights to file: " + outfile);
		}
		try
		{
			@out = IOUtil.processFileHeaders(IOUtil.getPathUsingWorkingDir(infile), IOUtil.getPathUsingWorkingDir(outfile), newComments, commentIndicators, ignoredCommentIndicators, 0);

			string iline = null;
			string cmnt = "#>";
			StateMod_ReservoirRight right = null;
			IList<object> v = new List<object>(10);
			string format_0 = null;
			string format_1 = null;
			if (oldAdminNumFormat)
			{
				// Left justify...
				format_0 = "%-12.12s%-24.24s%-12.12s    %-12.12s%8.0f%8d%8d%8d%8d%-12.12s";
				format_1 = "%-12.12s%-24.24s%-12.12s    %-12.12s%8.2f%8d%8d%8d%8d%-12.12s";
			}
			else
			{
				// Right justify...
				format_0 = "%-12.12s%-24.24s%-12.12s    %12.12s%8.0f%8d%8d%8d%8d%-12.12s";
				format_1 = "%-12.12s%-24.24s%-12.12s    %12.12s%8.2f%8d%8d%8d%8d%-12.12s";
			}

			@out.println(cmnt);
			@out.println(cmnt + " *******************************************************");
			@out.println(cmnt + "  StateMod Reservoir Right File");
			@out.println(cmnt);
			@out.println(cmnt + "  format:  (a12, a24, a12, F16.5, f8.0, 4i8, a12)");
			@out.println(cmnt);
			@out.println(cmnt + "  ID       cirsid:  Reservoir right ID");
			@out.println(cmnt + "  Name      namer:  Reservoir name");
			@out.println(cmnt + "  Res ID    cgoto:  Reservoir ID tied to this right");
			@out.println(cmnt + "  Admin #    rtem:  Administration number");
			@out.println(cmnt + "                    (small is senior).");
			@out.println(cmnt + "  Decree   dcrres:  Decreed amount (af)");
			@out.println(cmnt + "  On/Off   irsrsw:  Switch 0 = off,1 = on");
			@out.println(cmnt + "                    YYYY = on for years >= YYYY");
			@out.println(cmnt + "                    -YYYY = off for years > YYYY");
			@out.println(cmnt + "  Owner    iresco:  Ownership code");
			@out.println(cmnt + "                      >0, account to be filled");
			@out.println(cmnt + "                      <0, ownership go to 1st (n) accounts");
			@out.println(cmnt + "  Type     ityrsr:  Reservoir type");
			@out.println(cmnt + "                      1=Standard");
			@out.println(cmnt + "                      2=Out of priority water right");
			@out.println(cmnt + "  Fill #   n2fill:  Right type 1=1st fill, 2=2nd fill");
			@out.println(cmnt + "  Out ID    copid:  Out of priority associated operational ");
			@out.println(cmnt + "                      right  (when ityrsr=-1)");
			@out.println(cmnt);
			@out.println(cmnt + "    ID     Name                    Res ID            Admin #   Decree  On/Off  Owner   Type    Fill #  Out ID     ");
			@out.println(cmnt + "---------eb----------------------eb----------eb--------------eb------eb------eb------eb------eb------eb----------e");
			@out.println(cmnt);
			@out.println(cmnt + "EndHeader");
			@out.println(cmnt);

			int num = 0;
			if (theRights != null)
			{
				num = theRights.Count;
			}
			for (int i = 0; i < num; i++)
			{
				right = (StateMod_ReservoirRight)theRights[i];
				if (right == null)
				{
					continue;
				}
				v.Clear();
				v.Add(right.getID());
				v.Add(right.getName());
				v.Add(right.getCgoto());
				v.Add(right.getRtem());
				v.Add(new double?(right.getDcrres()));
				v.Add(new int?(right.getSwitch()));
				v.Add(new int?(right.getIresco()));
				v.Add(new int?(right.getItyrstr()));
				v.Add(new int?(right.getN2fill()));
				v.Add(right.getCopid());
				if (right.getDcrres() < 1.0)
				{
					// Use the format for a small right(8.2)...
					iline = StringUtil.formatString(v, format_1);
				}
				else
				{
					// Default format 8.0...
					iline = StringUtil.formatString(v, format_0);
				}
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
				@out.flush();
				@out.close();
			}
		}
	}

	/// <summary>
	/// Writes a list of StateMod_ReservoirRight objects to a list file.  A header is 
	/// printed to the top of the file, containing the commands used to generate the 
	/// file.  Any strings in the body of the file that contain the field delimiter will be wrapped in "...". </summary>
	/// <param name="filename"> the name of the file to which the data will be written. </param>
	/// <param name="delimiter"> the delimiter to use for separating field values. </param>
	/// <param name="update"> whether to update an existing file, retaining the current 
	/// header (true) or to create a new file with a new header. </param>
	/// <param name="data"> the list of objects to write. </param>
	/// <param name="newComments"> comments to write to the top of the file. </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeListFile(String filename, String delimiter, boolean update, java.util.List<StateMod_ReservoirRight> data, java.util.List<String> newComments) throws Exception
	public static void writeListFile(string filename, string delimiter, bool update, IList<StateMod_ReservoirRight> data, IList<string> newComments)
	{
		string routine = "StateMod_ReservoirRight.writeListFile";
		int size = 0;
		if (data != null)
		{
			size = data.Count;
		}

		IList<string> fields = new List<string>();
		fields.Add("ID");
		fields.Add("Name");
		fields.Add("StructureID");
		fields.Add("AdministrationNumber");
		fields.Add("DecreedAmount");
		fields.Add("OnOff");
		fields.Add("AccountDistribution");
		fields.Add("Type");
		fields.Add("FillType");
		fields.Add("OopRight");
		int fieldCount = fields.Count;

		string[] names = new string[fieldCount];
		string[] formats = new string[fieldCount];
		int comp = StateMod_DataSet.COMP_RESERVOIR_RIGHTS;
		string s = null;
		for (int i = 0; i < fieldCount; i++)
		{
			s = fields[i];
			names[i] = StateMod_Util.lookupPropValue(comp, "FieldName", s);
			formats[i] = StateMod_Util.lookupPropValue(comp, "Format", s);
		}

		string oldFile = null;
		if (update)
		{
			oldFile = IOUtil.getPathUsingWorkingDir(filename);
		}

		int j = 0;
		StateMod_ReservoirRight right = null;
		IList<string> commentIndicators = new List<string>(1);
		commentIndicators.Add("#");
		IList<string> ignoredCommentIndicators = new List<string>(1);
		ignoredCommentIndicators.Add("#>");
		string[] line = new string[fieldCount];
		StringBuilder buffer = new StringBuilder();
		PrintWriter @out = null;

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
			newComments2.Insert(1,"StateMod reservoir rights as a delimited list file.");
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
				right = (StateMod_ReservoirRight)data[i];

				line[0] = StringUtil.formatString(right.getID(),formats[0]).Trim();
				line[1] = StringUtil.formatString(right.getName(),formats[1]).Trim();
				line[2] = StringUtil.formatString(right.getCgoto(),formats[2]).Trim();
				line[3] = StringUtil.formatString(right.getRtem(),formats[3]).Trim();
				line[4] = StringUtil.formatString(right.getDcrres(),formats[4]).Trim();
				line[5] = StringUtil.formatString(right.getSwitch(),formats[5]).Trim();
				line[6] = StringUtil.formatString(right.getIresco(),formats[6]).Trim();
				line[7] = StringUtil.formatString(right.getItyrstr(),formats[7]).Trim();
				line[8] = StringUtil.formatString(right.getN2fill(),formats[8]).Trim();
				line[9] = StringUtil.formatString(right.getCopid(),formats[9]).Trim();

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

	}

}