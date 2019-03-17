using System;
using System.Collections.Generic;
using System.Text;

// StateMod_ReservoirAccount - class to store reservoir owner information

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
// StateMod_ReservoirAccount - class to store reservoir owner information
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
// 
// 02 Sep 1997	Catherine E.		Created initial version of class
//		Nutting-Lane, RTi
// 17 Feb 2001	Steven A. Malers, RTi	Handled nulls everwhere.  Add
//					finalize().  Set unused variables to
//					null.  Update javadoc.
// 2002-09-19	SAM, RTi		Use isDirty()instead of setDirty()to
//					indicate edits.
// 2002-11-01	SAM, RTi		Add toString().
//------------------------------------------------------------------------------
// 2003-06-05	J. Thomas Sapienza 	Initial StateMod_ version.
// 2003-06-10	JTS, RTi		Renamed from StateMod_OwnerInfo
// 2003-08-03	SAM, RTi		Change isDirty() back to setDirty().
// 2003-08-19	SAM, RTi		Update code to make consistent with
//					StateMod documentation and use the
//					base class for name.
// 2003-10-09	JTS, RTi		* Implemented Cloneable.
//					* Added clone().
//					* Added equals().
//					* Implemented Comparable.
//					* Added compareTo().
// 					* Added equals(Vector, Vector)
// 2003-10-15	JTS, RTi		Revised the clone() code.
// 2004-07-02	SAM, RTi		Handle null _dataset.
// 2004-09-12	SAM, RTi		* Add getPctevaChoices().
//					* Add getN2ownChoices().
// 2004-10-29	SAM, RTi		Remove temporary data for table model.
// 2005-01-17	JTS, RTi		* Added createBackup().
//					* Added restoreOriginal().
// 2005-04-18	JTS, RTi		Added writeListFile().
//------------------------------------------------------------------------------
// EndHeader

namespace DWR.StateMod
{

	using IOUtil = RTi.Util.IO.IOUtil;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;

	public class StateMod_ReservoirAccount : StateMod_Data, ICloneable, IComparable<StateMod_Data>
	{

	/// <summary>
	/// Maximum storage of owner
	/// </summary>
	protected internal double _ownmax;
	/// <summary>
	/// Initial storage of owner
	/// </summary>
	protected internal double _curown;
	/// <summary>
	/// Prorate reservoir evaporation between account owners
	/// </summary>
	protected internal double _pcteva;
	/// <summary>
	/// Ownership is tied to n fill right
	/// </summary>
	protected internal int _n2own;

	// Base class _name is used for the owner name.
	// The ID should be sete when reading, adding, or deleting accounts - set to
	// an integer starting at 1.

	/// <summary>
	/// Constructor.
	/// </summary>
	public StateMod_ReservoirAccount() : base()
	{
		initialize();
	}

	/// <summary>
	/// Clones the data object. </summary>
	/// <returns> a cloned object. </returns>
	public override object clone()
	{
		StateMod_ReservoirAccount acct = (StateMod_ReservoirAccount)base.clone();
		acct._isClone = true;
		return acct;
	}

	/// <summary>
	/// Compares this object to another StateMod_Data object based on the sorted
	/// order from the StateMod_Data variables, and then by _ownmax, _curown, 
	/// _pcteva, and _n2own, in that order. </summary>
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

		StateMod_ReservoirAccount acct = (StateMod_ReservoirAccount)data;
		if (_ownmax < acct._ownmax)
		{
			return -1;
		}
		else if (_ownmax > acct._ownmax)
		{
			return 1;
		}

		if (_curown < acct._curown)
		{
			return -1;
		}
		else if (_curown > acct._curown)
		{
			return 1;
		}

		if (_pcteva < acct._pcteva)
		{
			return -1;
		}
		else if (_pcteva > acct._pcteva)
		{
			return 1;
		}

		if (_n2own < acct._n2own)
		{
			return -1;
		}
		else if (_n2own > acct._n2own)
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
		_original = (StateMod_ReservoirAccount)clone();
		((StateMod_ReservoirAccount)_original)._isClone = false;
		_isClone = true;
	}

	/// <summary>
	/// Compare two rights Vectors and see if they are the same. </summary>
	/// <param name="v1"> the first Vector of StateMod_ReservoirAccounts to check.  Cannot be null. </param>
	/// <param name="v2"> the second Vector of StateMod_ReservoirAccounts to check.  Cannot be null. </param>
	/// <returns> true if they are the same, false if not. </returns>
	public static bool Equals(IList<StateMod_ReservoirAccount> v1, IList<StateMod_ReservoirAccount> v2)
	{
		string routine = "StateMod_ReservoirAccount.equals(List,List)";
		StateMod_ReservoirAccount r1;
		StateMod_ReservoirAccount r2;
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
			Message.printStatus(1, routine, "Lists are of size: " + size);
			IList<StateMod_ReservoirAccount> v1Sort = StateMod_Util.sortStateMod_DataVector(v1);
			IList<StateMod_ReservoirAccount> v2Sort = StateMod_Util.sortStateMod_DataVector(v2);
			Message.printStatus(1, routine, "Lists have been sorted");

			for (int i = 0; i < size; i++)
			{
				r1 = (StateMod_ReservoirAccount)v1Sort[i];
				r2 = (StateMod_ReservoirAccount)v2Sort[i];
				Message.printStatus(1, routine, r1.ToString());
				Message.printStatus(1, routine, r2.ToString());
				Message.printStatus(1, routine, "Element " + i + " comparison: " + r1.CompareTo(r2));
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
	/// <param name="acct"> the account to compare. </param>
	/// <returns> true if they are equal, false otherwise. </returns>
	public virtual bool Equals(StateMod_ReservoirAccount acct)
	{
		if (!base.Equals(acct))
		{
			 return false;
		}

		if (_ownmax == acct._ownmax && _curown == acct._curown && _pcteva == acct._pcteva && _n2own == acct._n2own)
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Clean up memory before garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~StateMod_ReservoirAccount()
	{
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Return the initial storage of owner.
	/// </summary>
	public virtual double getCurown()
	{
		return _curown;
	}

	/// <summary>
	/// Return the maximum storage of owner.
	/// </summary>
	public virtual double getOwnmax()
	{
		return _ownmax;
	}

	/// <summary>
	/// Return the ownership tied to n fill right.
	/// </summary>
	public virtual int getN2own()
	{
		return _n2own;
	}

	/// <summary>
	/// Return a list of N2own option strings, for use in GUIs.
	/// The options are of the form "1" if include_notes is false and
	/// "1 - Ownership is tied to first fill right(s)", if include_notes is true. </summary>
	/// <returns> a list of N2own option strings, for use in GUIs. </returns>
	/// <param name="include_notes"> Indicate whether notes should be added after the parameter values. </param>
	public static IList<string> getN2ownChoices(bool include_notes)
	{
		IList<string> v = new List<string>(2);
		v.Add("1 - Ownership is tied to first fill right(s)");
		v.Add("2 - Ownership is tied to second fill right(s)");
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
	/// Return the default N2own choice.  This can be used by GUI code to pick a default for a new diversion. </summary>
	/// <returns> the default N2own choice. </returns>
	public static string getN2ownDefault(bool include_notes)
	{ // Make this aggree with the above method...
		if (include_notes)
		{
			return "1 - Ownership is tied to first fill right(s)";
		}
		else
		{
			return "1";
		}
	}

	/// <summary>
	/// Return the prorate res evap btwn accnt owners.
	/// </summary>
	public virtual double getPcteva()
	{
		return _pcteva;
	}

	/// <summary>
	/// Return a list of Pcteva option strings, for use in GUIs, with integer percents.
	/// The options are of the form "0" if include_notes is false and
	/// "0 - Prorate evaporation based on current storage", if include_notes is true. </summary>
	/// <returns> a list of Pcteva option strings, for use in GUIs. </returns>
	/// <param name="include_notes"> Indicate whether notes should be added after the parameter values. </param>
	public static IList<string> getPctevaChoices(bool include_notes)
	{
		IList<string> v = new List<string>(2);
		v.Add("0 - Prorate evaporation based on current storage");
		for (int i = 100; i >= 1; i--)
		{
			v.Add("" + i + " - Apply " + i + " % of evaporation to account");
		}
		v.Add("-1 - No evaporation for this account");
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
	/// Return the default Pcteva choice.  This can be used by GUI code to pick a default for a new diversion. </summary>
	/// <returns> the default Pcteva choice. </returns>
	public static string getPctevaDefault(bool include_notes)
	{ // Make this agree with the above method...
		if (include_notes)
		{
			return "0 - Prorate evaporation based on current storage";
		}
		else
		{
			return "0";
		}
	}

	/// <summary>
	/// Initialize member variables
	/// </summary>
	private void initialize()
	{
		_ownmax = 0;
		_curown = 0;
		_pcteva = 0;
		_n2own = 1;
	}

	/// <summary>
	/// Cancels any changes made to this object within a GUI since createBackup()
	/// was called and sets _original to null.
	/// </summary>
	public override void restoreOriginal()
	{
		StateMod_ReservoirAccount acct = (StateMod_ReservoirAccount)_original;
		base.restoreOriginal();

		_ownmax = acct._ownmax;
		_curown = acct._curown;
		_pcteva = acct._pcteva;
		_n2own = acct._n2own;
		_isClone = false;
		_original = null;
	}

	/// <summary>
	/// Set the initial storage of owner.
	/// </summary>
	public virtual void setCurown(double d)
	{
		if (d != _curown)
		{
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_RESERVOIR_STATIONS,true);
			}
			_curown = d;
		}
	}

	/// <summary>
	/// Set the initial storage of owner.
	/// </summary>
	public virtual void setCurown(double? d)
	{
		setCurown(d.Value);
	}

	/// <summary>
	/// Set the initial storage of owner.
	/// </summary>
	public virtual void setCurown(string str)
	{
		if (string.ReferenceEquals(str, null))
		{
			return;
		}
		setCurown(StringUtil.atod(str.Trim()));
	}

	/// <summary>
	/// Set the ownership tied to n fill right.
	/// </summary>
	public virtual void setN2own(int i)
	{
		if (i != _n2own)
		{
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_RESERVOIR_STATIONS,true);
			}
			_n2own = i;
		}
	}

	/// <summary>
	/// Set the ownership tied to n fill right.
	/// </summary>
	public virtual void setN2own(int? i)
	{
		setN2own(i.Value);
	}

	/// <summary>
	/// Set the ownership tied to n fill right.
	/// </summary>
	public virtual void setN2own(string str)
	{
		if (string.ReferenceEquals(str, null))
		{
			return;
		}
		setN2own(StringUtil.atoi(str.Trim()));
	}

	/// <summary>
	/// Set the maximum storage of owner.
	/// </summary>
	public virtual void setOwnmax(double d)
	{
		if (d != _ownmax)
		{
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_RESERVOIR_STATIONS, true);
			}
			_ownmax = d;
		}
	}

	/// <summary>
	/// Set the maximum storage of owner.
	/// </summary>
	public virtual void setOwnmax(double? d)
	{
		setOwnmax(d.Value);
	}

	/// <summary>
	/// Set the maximum storage of owner.
	/// </summary>
	public virtual void setOwnmax(string str)
	{
		if (string.ReferenceEquals(str, null))
		{
			return;
		}
		setOwnmax(StringUtil.atod(str.Trim()));
	}

	/// <summary>
	/// Set the prorate res evap btwn accnt owners.
	/// </summary>
	public virtual void setPcteva(double d)
	{
		if (d != _pcteva)
		{
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_RESERVOIR_STATIONS, true);
			}
			_pcteva = d;
		}
	}

	/// <summary>
	/// Set the prorate reservoir evaporation between account owners.
	/// </summary>
	public virtual void setPcteva(double? d)
	{
		setPcteva(d.Value);
	}

	/// <summary>
	/// Set the prorate reservoir evaporation between account owners.
	/// </summary>
	public virtual void setPcteva(string str)
	{
		if (string.ReferenceEquals(str, null))
		{
			return;
		}
		setPcteva(StringUtil.atod(str.Trim()));
	}

	/// <summary>
	/// Returns a String representation of this object. </summary>
	/// <returns> a String representation of this object. </returns>
	public override string ToString()
	{
		return base.ToString() + ", " + _ownmax + ", " + _curown + ", " + _pcteva + ", " + _n2own;
	}

	/// <summary>
	/// Writes a list of StateMod_ReservoirAccount objects to a list file.  A header 
	/// is printed to the top of the file, containing the commands used to generate the
	/// file.  Any strings in the body of the file that contain the field delimiter will be wrapped in "...". </summary>
	/// <param name="filename"> the name of the file to which the data will be written. </param>
	/// <param name="delimiter"> the delimiter to use for separating field values. </param>
	/// <param name="update"> whether to update an existing file, retaining the current 
	/// header (true) or to create a new file with a new header. </param>
	/// <param name="data"> the list of objects to write. </param>
	/// <param name="newComments"> new comments to add to the top of the file. </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeListFile(String filename, String delimiter, boolean update, java.util.List data, java.util.List newComments) throws Exception
	public static void writeListFile(string filename, string delimiter, bool update, System.Collections.IList data, System.Collections.IList newComments)
	{
		string routine = "StateMod_ReservoirAccount.writeListFile";
		int size = 0;
		if (data != null)
		{
			size = data.Count;
		}

		System.Collections.IList fields = new List<object>();
		fields.Add("ReservoirID");
		fields.Add("OwnerID");
		fields.Add("OwnerAccount");
		fields.Add("MaxStorage");
		fields.Add("InitialStorage");
		fields.Add("ProrateEvap");
		fields.Add("OwnershipTie");
		int fieldCount = fields.Count;

		string[] names = new string[fieldCount];
		string[] formats = new string[fieldCount];
		int comp = StateMod_DataSet.COMP_RESERVOIR_STATION_ACCOUNTS;
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
		StateMod_ReservoirAccount acct = null;
		System.Collections.IList commentIndicators = new List<object>(1);
		commentIndicators.Add("#");
		System.Collections.IList ignoredCommentIndicators = new List<object>(1);
		ignoredCommentIndicators.Add("#>");
		string[] line = new string[fieldCount];
		StringBuilder buffer = new StringBuilder();
		PrintWriter @out = null;

		try
		{
			// Add some basic comments at the top of the file.  Do this to a copy of the
			// incoming comments so that they are not modified in the calling code.
			System.Collections.IList newComments2 = null;
			if (newComments == null)
			{
				newComments2 = new List<object>();
			}
			else
			{
				newComments2 = new List<object>(newComments);
			}
			newComments2.Insert(0,"");
			newComments2.Insert(1,"StateMod reservoir station accounts as a delimited list file.");
			newComments2.Insert(2,"See also the associated station, precipitation station, evaporation station,");
			newComments2.Insert(3,"content/area/seepage, and collection files.");
			newComments2.Insert(4,"");
			@out = IOUtil.processFileHeaders(oldFile, IOUtil.getPathUsingWorkingDir(filename), newComments2, commentIndicators, ignoredCommentIndicators, 0);

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
				acct = (StateMod_ReservoirAccount)data[i];

				line[0] = StringUtil.formatString(acct.getCgoto(),formats[0]).Trim();
				line[1] = StringUtil.formatString(acct.getID(),formats[1]).Trim();
				line[2] = StringUtil.formatString(acct.getName(),formats[2]).Trim();
				line[3] = StringUtil.formatString(acct.getOwnmax(),formats[3]).Trim();
				line[4] = StringUtil.formatString(acct.getCurown(),formats[4]).Trim();
				line[5] = StringUtil.formatString(acct.getPcteva(),formats[5]).Trim();
				line[6] = StringUtil.formatString(acct.getN2own(),formats[6]).Trim();

				buffer = new StringBuilder();
				for (j = 0; j < fieldCount; j++)
				{
					if (line[j].IndexOf(delimiter, StringComparison.Ordinal) > -1)
					{
						line[j] = "\"" + line[j] + "\"";
					}
					buffer.Append(line[j]);
					if (j < (fieldCount - 1))
					{
						buffer.Append(delimiter);
					}
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