using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

// StateMod_DelayTable - Contains information read from the delay table file.

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
// StateMod_DelayTable - Contains information read from the delay table file.
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
// 
// 03 Sep 1997	Catherine E.		Created initial version of class.
//		Nutting-Lane, RTi
// 24 Mar 1998	CEN, RTi		Added setRet_val.
// 21 Dec 1998	CEN, RTi		Added throws IOException to read/write
//					routines.
// 24 Jan 2000	CEN, RTi		Modified to accommodate Ray's new open
//					format(not necessarily 12 entries per
//					line).
// 14 Mar 2000	CEN, RTi		Extends from SMData now to utilize
//					search abilities in GUI(need to use ID
//					field).
// 17 Feb 2001	Steven A. Malers, RTi	Code review.  Change IO to IOUtil.  Add
//					finalize().  Handle nulls.  Set to null
//					when varialbles not used.  Update
//					javadoc.  Alphabetize methods.
// 13 Aug 2001	SAM, RTi		Add int to set the units as percent or
//					fraction.
// 2002-09-19	SAM, RTi		Use isDirty() instead of setDirty() to
//					indicate edits.
//------------------------------------------------------------------------------
// 2003-06-04	J. Thomas Sapienza, RTi	Renamed from SMDelayTbl to 
//					StateMod_DelayTable
// 2003-06-10	JTS, RTi		* Folded dumpDelayTableFile() into
//					  writeDelayTableFile()
// 					* Renamed parseDelayTableFile() to
//					  readDelayTableFile()
// 2003-06-23	JTS, RTi		Renamed writeDelayTableFile() to
//					writeStateModFile()
// 2003-06-26	JTS, RTi		Renamed readDelayTableFile() to
//					readStateModFile()
// 2003-07-07	SAM, RTi		* Javadoc data and parameters that were
//					  not documented.
//					* Remove MAX_DELAYS - not used anywhere.
//					* Remove _table_id since the base class
//					  _id can be used.
//					* Also set the base class name to the
//					  same as the ID.
//					* Check for null data set when reading
//					  data since when using with StateCU
//					  in StateDMI there is no StateMod data
//					  set.
// 2003-07-15	JTS, RTi		Changed code to use new dataset design.
// 2003-08-03	SAM, RTi		Change isDirty() back to setDirty().
// 2004-03-17	SAM, RTi		Add the scale() method to deal with
//					percent/fraction issues.
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
// 2005-04-18	JTS, RTi		Added writeListFile().
// 2007-04-12	Kurt Tometich, RTi		Added checkComponentData() and
//									getDataHeader() methods for check
//									file and data check support.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
//------------------------------------------------------------------------------

namespace DWR.StateMod
{

	using IOUtil = RTi.Util.IO.IOUtil;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;

	public class StateMod_DelayTable : StateMod_Data, ICloneable, IComparable<StateMod_Data>, StateMod_ComponentValidator
	{

	/// <summary>
	/// Number of return values.
	/// </summary>
	protected internal int _ndly;

	/// <summary>
	/// Double return values.
	/// </summary>
	protected internal IList<double?> _ret_val;

	/// <summary>
	/// Units for the data, as determined at read time.
	/// </summary>
	protected internal string _units;

	/// <summary>
	/// Indicate whether the delay table is for monthly or daily data.
	/// </summary>
	protected internal bool _isMonthly;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="isMonthly"> If true, the delay table contains monthly data.  If false, the
	/// delay table contains daily data. </param>
	public StateMod_DelayTable(bool isMonthly) : base()
	{
		initialize();
		_isMonthly = isMonthly;
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
	/// Add a delay.
	/// </summary>
	public virtual void addRet_val(double d)
	{
		addRet_val(new double?(d));
	}

	/// <summary>
	/// Add a delay
	/// </summary>
	public virtual void addRet_val(double? D)
	{
		_ret_val.Add(D);
		setNdly(_ret_val.Count);
		if (!_isClone && _dataset != null)
		{
			_dataset.setDirty(StateMod_DataSet.COMP_DELAY_TABLES_MONTHLY, true);
		}
	}

	/// <summary>
	/// Add a delay
	/// </summary>
	public virtual void addRet_val(string str)
	{
		if (!string.ReferenceEquals(str, null))
		{
			addRet_val(StringUtil.atod(str.Trim()));
		}
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
		StateMod_DelayTable d = (StateMod_DelayTable)base.clone();

		if (_ret_val == null)
		{
			d._ret_val = null;
		}
		else
		{
			d._ret_val = new List<double?>();
			int size = _ret_val.Count;
			for (int i = 0; i < size; i++)
			{
				d._ret_val.Add(new double?(_ret_val[i].Value));
			}
		}

		return d;
	}

	/// <summary>
	/// Compares this object to another StateMod_DelayTable object. </summary>
	/// <param name="data"> the object to compare against. </param>
	/// <returns> 0 if they are the same, 1 if this object is greater than the other object, or -1 if it is less. </returns>
	public virtual int CompareTo(StateMod_Data data)
	{
		int res = base.CompareTo(data);
		if (res != 0)
		{
			return res;
		}

	/* FIXME SAM 2009-01-11 Determine what to do with this code
		StateMod_DelayTable dt = (StateMod_DelayTable)_original;
		
		Message.printStatus(1, "", "'" + _ndly + "'  '" + dt._ndly + "'");
		Message.printStatus(1, "", "'" + _units + "'  '" + dt._units + "'");
		Message.printStatus(1, "", "'" + _is_monthly + "'   '" 
			+ dt._is_monthly + "'");
	
		if (_ret_val == null && dt._ret_val == null) {
			Message.printStatus(1, "", "Both are null");
		}
		else if (_ret_val == null) {
			Message.printStatus(1, "", "Mine is null");
		}
		else if (dt._ret_val == null) {
			Message.printStatus(1, "", "Hers is null");
		}
		else {
			int size1 = _ret_val.size();
			int size2 = dt._ret_val.size();
			Message.printStatus(1, "", "" + size1 + "   " + size2);
			if (size1 == size2) {
				for (int i = 0; i < size1; i++) {
					Message.printStatus(1, "", " " + (i + 1) 
						+ ") " + _ret_val.elementAt(i)
						+ "  " + dt._ret_val.elementAt(i));
				}
			}
		}
	*/

		StateMod_DelayTable d = (StateMod_DelayTable)data;

		if (_ndly < d._ndly)
		{
			return -1;
		}
		else if (_ndly > d._ndly)
		{
			return 1;
		}

		res = _units.CompareTo(d._units);
		if (res != 0)
		{
			return res;
		}

		if (_isMonthly != d._isMonthly)
		{
			return -1;
		}

		if (_ret_val == null && d._ret_val == null)
		{
			// ok
		}
		else if (_ret_val == null)
		{
			return -1;
		}
		else if (d._ret_val == null)
		{
			return 1;
		}
		else
		{
			double d1 = 0;
			double d2 = 0;
			double? D1 = null;
			double? D2 = null;

			int size1 = _ret_val.Count;
			int size2 = d._ret_val.Count;
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
				D1 = (double?)_ret_val[i];
				d1 = D1.Value;
				D2 = (double?)d._ret_val[i];
				d2 = D2.Value;

				if (d1 < d2)
				{
					return -1;
				}
				else if (d1 > d2)
				{
					return 1;
				}
			}
		}

		return 0;
	}

	/// <summary>
	/// Creates a copy of the object for later use in checking to see if it was changed in a GUI.
	/// </summary>
	public virtual void createBackup()
	{
		_original = (StateMod_DelayTable)clone();
		((StateMod_DelayTable)_original)._isClone = false;
		_isClone = true;
	}

	/// <summary>
	/// Clean up before garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~StateMod_DelayTable()
	{
		_ret_val = null;
		_units = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
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
	/// Get the number of return values.
	/// </summary>
	public virtual int getNdly()
	{
		return _ndly;
	}

	/// <summary>
	/// Get a delay corresponding to a particular index.
	/// </summary>
	public virtual double getRet_val(int index)
	{
		return (((double?)_ret_val[index]).Value);
	}

	/// <summary>
	/// Get a entire list of delays.
	/// </summary>
	public virtual IList<double?> getRet_val()
	{
		return _ret_val;
	}

	/// <summary>
	/// Return the delay table identifier. </summary>
	/// <returns> the delay table identifier. </returns>
	public virtual string getTableID()
	{
		return _id;
	}

	/// <summary>
	/// Return the delay table units. </summary>
	/// <returns> Units of delay table, consistent with time series data units, etc. </returns>
	public virtual string getUnits()
	{
		return _units;
	}

	/// <summary>
	/// Initialize data members.
	/// </summary>
	private void initialize()
	{
		_smdata_type = StateMod_DataSet.COMP_DELAY_TABLES_MONTHLY;
		_ndly = 0;
		_units = "PCT";
		_ret_val = new List<double?>(1);
	}

	/// <summary>
	/// Insert a delay - same as add but the index of where to insert can be given
	/// </summary>
	public virtual void insertRet_val(double d, int index)
	{
		double? D = new double?(d);
		insertRet_val(D, index);
	}

	/// <summary>
	/// Insert a delay - same as add but the index of where to insert can be given
	/// </summary>
	public virtual void insertRet_val(double? D, int index)
	{
		_ret_val.Insert(index, D);
		setNdly(_ret_val.Count);
		if (!_isClone && _dataset != null)
		{
			_dataset.setDirty(StateMod_DataSet.COMP_DELAY_TABLES_MONTHLY, true);
		}
	}

	/// <summary>
	/// Insert a delay - same as add but the index of where to insert can be given
	/// </summary>
	public virtual void insertRet_val(string str, int index)
	{
		if (!string.ReferenceEquals(str, null))
		{
			insertRet_val(StringUtil.atod(str.Trim()), index);
		}
	}

	/// <summary>
	/// Indicate whether the delay table contains monthly or daily data. </summary>
	/// <returns> true if the delay table contains monthly data, false if daily. </returns>
	public virtual bool isMonthly()
	{
		return _isMonthly;
	}

	/// <summary>
	/// Remove a delay. </summary>
	/// <param name="index"> the index of the delay to remove. </param>
	public virtual void removeRet_val(int index)
	{
		_ret_val.RemoveAt(index);
		setNdly(_ret_val.Count);
		if (!_isClone && _dataset != null)
		{
			_dataset.setDirty(StateMod_DataSet.COMP_DELAY_TABLES_MONTHLY, true);
		}
	}

	/// <summary>
	/// Cancels any changes made to this object within a GUI since createBackup()
	/// was caled and sets _original to null.
	/// </summary>
	public override void restoreOriginal()
	{
		StateMod_DelayTable d = (StateMod_DelayTable)_original;
		base.restoreOriginal();

		_ndly = d._ndly;
		_ret_val = d._ret_val;
		_units = d._units;
		_isMonthly = d._isMonthly;

		_isClone = false;
		_original = null;
	}

	/// <summary>
	/// Set the number of return values.
	/// </summary>
	public virtual void setNdly(int i)
	{
		if (i != _ndly)
		{
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_DELAY_TABLES_MONTHLY, true);
			}
			_ndly = i;
		}
	}

	/// <summary>
	/// Set the number of return values.
	/// </summary>
	public virtual void setNdly(int? i)
	{
		setNdly(i.Value);
	}

	/// <summary>
	/// Set the number of return values.
	/// </summary>
	public virtual void setNdly(string str)
	{
		if (!string.ReferenceEquals(str, null))
		{
			setNdly(StringUtil.atoi(str.Trim()));
		}
	}

	public virtual void setRet_val(IList<double?> v)
	{
		_ret_val = new List<double?>(v);
		_ndly = _ret_val.Count;
	}

	public virtual void setRet_val(int index, string str)
	{
		setRet_val(index, Convert.ToDouble(str.Trim()));
	}

	public virtual void setRet_val(int index, double d)
	{
		setRet_val(index, new double?(d));
	}

	public virtual void setRet_val(int index, double? d)
	{
		if (d != null)
		{
			if (getNdly() > index)
			{
				_ret_val[index] = d;
				if (!_isClone && _dataset != null)
				{
					_dataset.setDirty(StateMod_DataSet.COMP_DELAY_TABLES_MONTHLY, true);
				}
			}
			else
			{
				addRet_val(d);
			}
		}
	}

	/// <summary>
	/// Set the id.
	/// </summary>
	public virtual void setTableID(string str)
	{
		if (!str.Equals(_id))
		{
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_DELAY_TABLES_MONTHLY, true);
			}
			_id = str;
			// Set the name to the same as the ID...
			_name = str;
		}
	}

	/// <summary>
	/// Set the delay table units. </summary>
	/// <param name="units"> Units of delay table, consistent with time series data units, etc. </param>
	public virtual void setUnits(string units)
	{
		if (!units.Equals(_units))
		{
			if (!string.ReferenceEquals(units, null))
			{
				if (!_isClone && _dataset != null)
				{
					_dataset.setDirty(StateMod_DataSet.COMP_DELAY_TABLES_MONTHLY, true);
				}
				_units = units;
			}
		}
	}

	/// <summary>
	/// Returns the value for nrtn to be compared against the interv in the control file.  Either a value
	/// is returned (if every nrtn is the same) or a -1 is returned (variable values for nrtn).
	/// </summary>
	/* TODO SAM 2007-03-01 Evaluate use
	private int checkDelayInterv(Vector delaysVector) {
		int ndly = -999;
		if (delaysVector == null) {
			return ndly;
		}
	
		int ndelay = delaysVector.size();
		StateMod_DelayTable delayTable;
	
		for (int i = 0; i < ndelay; i++) {
			delayTable =(StateMod_DelayTable)delaysVector.elementAt(i);
			if (ndly == -999) {
				ndly = delayTable.getNdly();
			}
			if (ndly != delayTable.getNdly()) {
				delayTable = null;
				return(-1);
			}
		}
		delayTable = null;
		return ndly;
	}
	*/

	/// <summary>
	/// Read delay information in and store in a java vector.  The new delay entries are
	/// added to the end of the previously stored delays.  Returns the delay table information. </summary>
	/// <param name="filename"> the filename to read from. </param>
	/// <param name="isMonthly"> Set to true if the delay table contains monthly data, false if it contains daily data. </param>
	/// <param name="interv"> The control file interv parameter.  +n indicates the number of
	/// values in each delay pattern.  -1 indicates variable number of values with
	/// values as percent (0-100).  -100 indicates variable number of values with values as fraction (0-1). </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.List<StateMod_DelayTable> readStateModFile(String filename, boolean isMonthly, int interv) throws Exception
	public static IList<StateMod_DelayTable> readStateModFile(string filename, bool isMonthly, int interv)
	{
		string routine = "StateMod_DelayTable.readStateModFile";
		string iline;
		IList<StateMod_DelayTable> theDelays = new List<StateMod_DelayTable>(1);
		StateMod_DelayTable aDelay = new StateMod_DelayTable(isMonthly);
		int numRead = 0, totalNumToRead = 0;
		bool reading = false;
		StreamReader @in = null;
		StringTokenizer split = null;

		if (Message.isDebugOn)
		{
			Message.printDebug(10, routine, "in readStateModFile reading file: " + filename);
		}
		try
		{
			@in = new StreamReader(filename);
			while (!string.ReferenceEquals((iline = @in.ReadLine()), null))
			{
				// check for comments
				iline = iline.Trim();
				if (iline.StartsWith("#", StringComparison.Ordinal) || iline.Length == 0)
				{
					continue;
				}

				split = new StringTokenizer(iline);
				if ((split == null) || (split.countTokens() == 0))
				{
					continue;
				}

				if (!reading)
				{
					// allocate new delay node
					aDelay = new StateMod_DelayTable(isMonthly);
					numRead = 0;
					reading = true;
					theDelays.Add(aDelay);
					aDelay.setTableID(split.nextToken());

					if (interv < 0)
					{
						aDelay.setNdly(split.nextToken());
					}
					else
					{
						aDelay.setNdly(interv);
					}
					totalNumToRead = aDelay.getNdly();
					// Set the delay table units(default is percent)...
					aDelay.setUnits("PCT");
					if (interv == -100)
					{
						aDelay.setUnits("FRACTION");
					}
				}

				while (split.hasMoreTokens())
				{
					aDelay.addRet_val(split.nextToken());
					numRead++;
				}
				if (numRead >= totalNumToRead)
				{
					reading = false;
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
			if (@in != null)
			{
				@in.Close();
			}
			@in = null;
		}
		return theDelays;
	}

	/// <summary>
	/// Scale the delay table by the given value.  This is used to convert between percent and fraction. </summary>
	/// <param name="value"> Value by which to multiple the delay table values. </param>
	public virtual void scale(double value)
	{
		for (int i = 0; i < _ndly; i++)
		{
			setRet_val(i,getRet_val(i) * value);
		}
	}

	/// <param name="count"> Number of components checked. </param>
	/// <param name="dataset"> StateMod dataset object. </param>
	/// <returns> validation results. </returns>
	public virtual StateMod_ComponentValidation validateComponent(StateMod_DataSet dataset)
	{
		// TODO KAT 2007-04-16 add specific checks here
		return null;
	}

	/// <summary>
	/// Write the new (updated) delay table file.  This routine writes the new delay table file.
	/// If an original file is specified, then the original header is carried into the new file.
	/// The writing of data is done by the dumpDelayFile routine which now does not mess with headers. </summary>
	/// <param name="inputFile"> old file (used as input) </param>
	/// <param name="outputFile"> new file to create </param>
	/// <param name="dly"> list of delays </param>
	/// <param name="newcomments"> new comments to save with the header of the file </param>
	/// <param name="interv"> interv variable specified in control file (if negative, include the number
	/// of entries at the start of each record). </param>
	/// <param name="precision"> number of digits after the decimal to write.  If negative, 2 is assumed. </param>
	/// <exception cref="Exception"> if an error occurs </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeStateModFile(String inputFile, String outputFile, java.util.List<StateMod_DelayTable> dly, java.util.List<String> newcomments, int interv, int precision) throws Exception
	public static void writeStateModFile(string inputFile, string outputFile, IList<StateMod_DelayTable> dly, IList<string> newcomments, int interv, int precision)
	{
		PrintWriter @out = null;
		IList<string> commentIndicators = new List<string>(1);
		commentIndicators.Add("#");
		IList<string> ignoredCommentIndicators = new List<string>(1);
		ignoredCommentIndicators.Add("#>");
		string routine = "StateMod_DelayTable.writeStateModFile";
		if (precision < 0)
		{
			precision = 2;
		}

		Message.printStatus(2, routine, "Writing delay tables to file \"" + outputFile + "\" using \"" + inputFile + "\" header...");

		try
		{
			// Process the header from the old file...
			@out = IOUtil.processFileHeaders(inputFile, outputFile, newcomments, commentIndicators, ignoredCommentIndicators, 0);

			// Now write the new data...
			string delayVal = null;
			string cmnt = "#>";
			string idFormat = "%8.8s"; // Right justify because StateMod actually uses integers
			string delayValueFormat0 = "%8."; // no precision
			string delayValueFormat = "%8." + precision + "f";
			string countFormat = "%4d";

			@out.println(cmnt);
			@out.println(cmnt + " *******************************************************");
			@out.println(cmnt + " StateMod Delay (Return flow) Table");
			@out.println(cmnt);
			@out.println(cmnt + "     Format (a8, i4, (12f8.2)");
			@out.println(cmnt);
			@out.println(cmnt + "   ID       idly: Delay table id");
			@out.println(cmnt + "   Ndly     ndly: Number of entries in delay table idly.");
			@out.println(cmnt + "                  Include only if \"interv\" in the");
			@out.println(cmnt + "                  control file is equal to -1.");
			@out.println(cmnt + "                  interv = -1 = Variable number of entries");
			@out.println(cmnt + "                                as percent (0-100)");
			@out.println(cmnt + "                  interv = -100 = Variable number of entries");
			@out.println(cmnt + "                                as fraction (0-1)");
			@out.println(cmnt + "   Ret  dlyrat(1-n,idl): Return for month n, station idl");
			@out.println(cmnt);
			@out.println(cmnt + " ID   Ndly  Ret1    Ret2    Ret3    Ret4    Ret5    Ret6    Ret7    Ret8    Ret9    Ret10   Ret11   Ret12");
			@out.println(cmnt + "-----eb--eb------eb------eb------eb------eb------eb------eb------eb------eb------eb------eb------eb------e...next line");
			@out.println(cmnt + "EndHeader");
			@out.println(cmnt);

			int ndly = 0;
			if (dly != null)
			{
				ndly = dly.Count;
			}
			int numWritten, numToWrite;
			if (Message.isDebugOn)
			{
				Message.printDebug(3, routine, "Writing " + ndly + " delay table entries.");
			}
			StringBuilder iline = new StringBuilder();
			StateMod_DelayTable delay = null;
			for (int i = 0; i < ndly; i++)
			{
				delay = dly[i];
				// Clear out the string buffer for the new delay table
				iline.Length = 0;
				// Create one delay table entry with ID, Nvals, and 12 return values per line.
				if (interv < 0)
				{
					iline.Append(StringUtil.formatString(delay.getTableID(),idFormat));
					iline.Append(StringUtil.formatString(delay.getNdly(),countFormat));
				}
				else
				{
					iline.Append(StringUtil.formatString(delay.getTableID(),idFormat));
				}

				numWritten = 0; // Number of values written for this delay table
				int numDelayVals = delay.getNdly();
				while (true)
				{
					if (numWritten > 0)
					{
						// Clear lines 2+ before adding values
						iline.Length = 0;
						// Add spaces as per the record header info
						if (interv < 0)
						{
							iline.Append("            ");
						}
						else
						{
							iline.Append("        ");
						}
					}
					// Number of values remaining to write
					numToWrite = numDelayVals - numWritten;

					if (numToWrite > 12)
					{
						// Have more than 12 so only write 12 on this line
						numToWrite = 12;
					}

					for (int j = 0; j < numToWrite; j++)
					{
						delayVal = StringUtil.formatString(delay.getRet_val(numWritten + j),delayValueFormat);
						if (delayVal.IndexOf(' ') < 0)
						{
							// There are no spaces - this will be a problem because the file is free format.
							// Do a little more work here to reduce the precision until there is a leading
							// space.
							for (int iprecision = precision - 1; iprecision >= 0; iprecision--)
							{
								delayVal = StringUtil.formatString(delay.getRet_val(numWritten + j), delayValueFormat0 + iprecision + ".f");
								if (delayVal.IndexOf(' ') >= 0)
								{
									// Done
									break;
								}
							}
						}
						iline.Append(delayVal);
					}
					// Now output the line:

					@out.println(iline);
					numWritten += numToWrite;
					if (numWritten == numDelayVals)
					{
						// Done writing so break out
						break;
					}
				}
			}
		}
		catch (Exception e)
		{
			if (@out != null)
			{
				@out.close();
			}
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
	/// Writes a list of StateMod_DelayTable objects to a list file.  A header is 
	/// printed to the top of the file, containing the commands used to generate the 
	/// file.  Any strings in the body of the file that contain the field delimiter will be wrapped in "...". </summary>
	/// <param name="filename"> the name of the file to which the data will be written. </param>
	/// <param name="delimiter"> the delimiter to use for separating field values. </param>
	/// <param name="update"> whether to update an existing file, retaining the current 
	/// header (true) or to create a new file with a new header. </param>
	/// <param name="data"> the list of objects to write. </param>
	/// <param name="newComments"> the list of new comments to write to the header, or null if none. </param>
	/// <param name="comp"> the component type being written (to distinguish between daily and monthly delay tables),
	/// either StateMod_DataSet.COMP_DELAY_TABLES_DAILY or StateMod_DataSet.COMP_DELAY_TABLES_MONTHLY. </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeListFile(String filename, String delimiter, boolean update, java.util.List<StateMod_DelayTable> data, java.util.List<String> newComments, int comp) throws Exception
	public static void writeListFile(string filename, string delimiter, bool update, IList<StateMod_DelayTable> data, IList<string> newComments, int comp)
	{
		int size = 0;
		if (data != null)
		{
			size = data.Count;
		}

		IList<string> fields = new List<string>();
		fields.Add("DelayTableID");
		fields.Add("Date");
		fields.Add("ReturnAmount");

		int fieldCount = fields.Count;
		string[] names = new string[fieldCount];
		string[] formats = new string[fieldCount];
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
		int k = 0;
		int num = 0;
		PrintWriter @out = null;
		StateMod_DelayTable delay = null;
		IList<string> commentIndicators = new List<string>(1);
		commentIndicators.Add("#");
		IList<string> ignoredCommentIndicators = new List<string>(1);
		ignoredCommentIndicators.Add("#>");
		string[] line = new string[fieldCount];
		string id = null;
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
			if (comp == StateMod_DataSet.COMP_DELAY_TABLES_DAILY)
			{
				newComments2.Insert(1,"StateMod delay tables (daily) as a delimited list file.");
			}
			else
			{
				newComments2.Insert(1,"StateMod delay tables (monthly) as a delimited list file.");
			}
			newComments2.Insert(2,"");
			@out = IOUtil.processFileHeaders(IOUtil.getPathUsingWorkingDir(oldFile), IOUtil.getPathUsingWorkingDir(filename), newComments2, commentIndicators, ignoredCommentIndicators, 0);

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
				delay = (StateMod_DelayTable)data[i];

				id = delay.getID();
				num = delay.getNdly();
				for (j = 0; j < num; j++)
				{
					line[0] = StringUtil.formatString(id, formats[0]).Trim();
					line[1] = StringUtil.formatString((j + 1), formats[1]).Trim();
					line[2] = StringUtil.formatString(delay.getRet_val(j), formats[2]).Trim();

					buffer = new StringBuilder();
					for (k = 0; k < fieldCount; k++)
					{
						if (k > 0)
						{
							buffer.Append(delimiter);
						}
						if (line[k].IndexOf(delimiter, StringComparison.Ordinal) > -1)
						{
							line[k] = "\"" + line[k] + "\"";
						}
						buffer.Append(line[k]);
					}

					@out.println(buffer.ToString());
				}
			}
			@out.flush();
			@out.close();
			@out = null;
		}
		catch (Exception e)
		{
			// TODO SAM 2009-01-11 Log it?
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