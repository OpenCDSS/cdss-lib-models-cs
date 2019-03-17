using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

// StateMod_ReturnFlow - store and manipulate return flow assignments

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
// StateMod_ReturnFlow - store and manipulate return flow assignments
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
// 
// 27 Aug 1997	Catherine E.		Created initial version of class.
//		Nutting-Lane, RTi
// 18 Oct 1999	CEN, RTi		Because this is now being used for both
//					diversions and wells, I am adding a
//					constructor that indicates that any
//					changes affects a particular file
//					(default remains StateMod_DataSet.
//					COMP_DIVERSION_STATIONS).
// 17 Feb 2001	Steven A. Malers, RTi	Code review.  Clean up javadoc.  Add
//					finalize().  Alphabetize methods.
//					Handle nulls and set unused variables
//					to null.
// 2002-09-19	SAM, RTi		Use isDirty()instead of setDirty()to
//					indicate edits.
// 2003-08-03	SAM, RTi		Changed isDirty() back to setDirty().
// 2003-09-30	SAM, RTi		* Fix bug where initialize was not
//					  getting called.
//					* Change _dirtyFlag to use the base
//					  class _smdata_type.
// 2003-10-09	J. Thomas Sapienza, RTi	* Implemented Cloneable.
//					* Added clone().
//					* Added equals().
//					* Implemented Comparable.
//					* Added compareTo().
//					* Added equals(Vector, Vector).
//					* Added isMonthly_data().
// 2003-10-15	JTS, RTi		Revised the clone() code.
// 2004-07-14	JTS, RTi		Changed compareTo to account for
//					crtnids that have descriptions, too.
// 2005-01-17	JTS, RTi		* Added createBackup().
//					* Added restoreOriginal().
// 2005-04-15	JTS, RTi		Added writeListFile().
//------------------------------------------------------------------------------

namespace DWR.StateMod
{

	using IOUtil = RTi.Util.IO.IOUtil;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// <para>
	/// This class stores return flow assignments.  A list of instances is maintained for each StateMod_Diversion
	/// and StateMod_Well (included in station files) and separate files for reservoirs and plans.
	/// Each instance indicates the river node receiving the return flow, percent of the flow going to the node, and
	/// the delay table identifier to use for the time distribution of the flow.
	/// </para>
	/// <para>
	/// The StateMod_Data ID is the station for which the return flow applies.
	/// </para>
	/// </summary>
	public class StateMod_ReturnFlow : StateMod_Data, ICloneable, IComparable<StateMod_Data>
	{

	/// <summary>
	/// River node receiving the return flow.
	/// </summary>
	private string __crtnid;
	/// <summary>
	/// % of return flow to this river node.
	/// </summary>
	private double __pcttot;
	/// <summary>
	/// Delay (return q) table for return.
	/// </summary>
	private int __irtndl;
	/// <summary>
	/// Indicates whether the returns are for daily (false) or monthly (true) data.
	/// </summary>
	private bool __isMonthlyData;

	/// <summary>
	/// Construct an instance of StateMod_ReturnFlow. </summary>
	/// <param name="smdataType"> Either StateMod_DataSet.COMP_DIVERSION_STATIONS,
	/// StateMod_DataSet.COMP_WELL_STATIONS, StateMod_DataSet.COMP_RESERVOIR_RETURN, or
	/// StateMod_DataSet.COMP_PLAN_RETURN.  Return flow assignments are associated
	/// with these data components.  Therefore, when a change is made, the appropriate
	/// component must be marked dirty. </param>
	public StateMod_ReturnFlow(int smdataType) : base()
	{
		_smdata_type = smdataType;
		initialize();
	}

	/// <summary>
	/// Clones the data object. </summary>
	/// <returns> a cloned object. </returns>
	public override object clone()
	{
		StateMod_ReturnFlow rf = (StateMod_ReturnFlow)base.clone();
		rf._isClone = true;

		return rf;
	}

	/// <summary>
	/// Compares this object to another StateMod_ReturnFlow object based on the sorted
	/// order from the StateMod_ReturnFlow variables, and then by crtnid, pcttot, and irtndl, in that order. </summary>
	/// <param name="data"> the object to compare against. </param>
	/// <returns> 0 if they are the same, 1 if this object is greater than the other object, or -1 if it is less. </returns>
	public virtual int CompareTo(StateMod_Data data)
	{
		int res = base.CompareTo(data);
		if (res != 0)
		{
			return res;
		}

		StateMod_ReturnFlow rf = (StateMod_ReturnFlow)data;

		int index = -1;
		string crtnid1 = __crtnid;
		index = crtnid1.IndexOf(" - ", StringComparison.Ordinal);
		if (index > 0)
		{
			crtnid1 = crtnid1.Substring(0, index).Trim();
		}

		string crtnid2 = rf.__crtnid;
		index = crtnid2.IndexOf(" - ", StringComparison.Ordinal);
		if (index > 0)
		{
			crtnid2 = crtnid2.Substring(0, index).Trim();
		}

		res = crtnid1.CompareTo(crtnid2);
		if (res != 0)
		{
			return res;
		}

		if (__pcttot < rf.__pcttot)
		{
			return -1;
		}
		else if (__pcttot > rf.__pcttot)
		{
			return 1;
		}

		if (__irtndl < rf.__irtndl)
		{
			return -1;
		}
		else if (__irtndl > rf.__irtndl)
		{
			return 1;
		}

		// sort false before true
		if (__isMonthlyData == false)
		{
			if (rf.__isMonthlyData == true)
			{
				return -1;
			}
			return 0;
		}
		else
		{
			if (rf.__isMonthlyData == true)
			{
				return 0;
			}
			return 1;
		}
	}

	/// <summary>
	/// Creates a backup of the current data object and stores it in _original,
	/// for use in determining if an object was changed inside of a GUI.
	/// </summary>
	public virtual void createBackup()
	{
		_original = (StateMod_ReturnFlow)clone();
		((StateMod_ReturnFlow)_original)._isClone = false;
		_isClone = true;
	}

	/// <summary>
	/// Compare two return flow lists and see if they are the same. </summary>
	/// <param name="v1"> the first list of StateMod_ReturnFlow to check.  Cannot be null. </param>
	/// <param name="v2"> the second list of StateMod_ReturnFlow to check.  Cannot be null. </param>
	/// <returns> true if they are the same, false if not. </returns>
	public static bool Equals(IList<StateMod_ReturnFlow> v1, IList<StateMod_ReturnFlow> v2)
	{
		string routine = "StateMod_ReturnFlow.equals";
		StateMod_ReturnFlow rf1;
		StateMod_ReturnFlow rf2;
		if (v1.Count != v2.Count)
		{
			Message.printStatus(2, routine, "Return flow lists are different sizes");
			return false;
		}
		else
		{
			// Sort the lists and compare item-by-item.  Any differences
			// and data will need to be saved back into the data set.
			int size = v1.Count;
			//Message.printStatus(2, routine, "Return flow lists are of size: " + size);
			IList<StateMod_ReturnFlow> v1Sort = StateMod_Util.sortStateMod_DataVector(v1);
			IList<StateMod_ReturnFlow> v2Sort = StateMod_Util.sortStateMod_DataVector(v2);
			//Message.printStatus(2, routine, "Return flow lists have been sorted");

			for (int i = 0; i < size; i++)
			{
				rf1 = v1Sort[i];
				rf2 = v2Sort[i];
				Message.printStatus(2, routine, rf1.ToString());
				Message.printStatus(2, routine, rf2.ToString());
				//Message.printStatus(2, routine, "Element " + i + " comparison: " + rf1.compareTo(rf2));
				if (rf1.CompareTo(rf2) != 0)
				{
					return false;
				}
			}
		}
		return true;
	}

	/// <summary>
	/// Tests to see if two return flows are equal.  Strings are compared with case sensitivity. </summary>
	/// <param name="rf"> the return flow to compare. </param>
	/// <returns> true if they are equal, false otherwise. </returns>
	public virtual bool Equals(StateMod_ReturnFlow rf)
	{
		if (!base.Equals(rf))
		{
			 return false;
		}
		if (rf.__crtnid.Equals(__crtnid) && rf.__pcttot == __pcttot && rf.__irtndl == __irtndl && rf.__isMonthlyData == __isMonthlyData)
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
	~StateMod_ReturnFlow()
	{
		__crtnid = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Return the crtnid.
	/// </summary>
	public virtual string getCrtnid()
	{
		return __crtnid;
	}

	/// <summary>
	/// Retrieve the delay table for return.
	/// </summary>
	public virtual int getIrtndl()
	{
		return __irtndl;
	}

	/// <summary>
	/// Return the % of return flow to this river node.
	/// </summary>
	public virtual double getPcttot()
	{
		return __pcttot;
	}

	private void initialize()
	{
		__crtnid = "";
		__pcttot = 100;
		__irtndl = 1;
	}

	public virtual bool isMonthly_data()
	{
		return __isMonthlyData;
	}

	/// <summary>
	/// Read return information in and store in a list. </summary>
	/// <param name="filename"> filename containing return flow information </param>
	/// <param name="smdataCompType"> the StateMod_DataSet component type, passed to the constructor of StateMod_ReturnFlow
	/// objects. </param>
	/// <exception cref="Exception"> if an error occurs </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.List<StateMod_ReturnFlow> readStateModFile(String filename, int smdataCompType) throws Exception
	public static IList<StateMod_ReturnFlow> readStateModFile(string filename, int smdataCompType)
	{
		string routine = "StateMod_ReturnFlow.readStateModFile";
		string iline = null;
		IList<string> v;
		IList<StateMod_ReturnFlow> theReturns = new List<StateMod_ReturnFlow>();
		int linecount = 0;

		StateMod_ReturnFlow aReturn = null;
		StreamReader @in = null;

		Message.printStatus(2, routine, "Reading return file: " + filename);
		int size = 0;
		int errorCount = 0;
		try
		{
			@in = new StreamReader(IOUtil.getPathUsingWorkingDir(filename));
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
				// Break the line using whitespace, while allowing for quoted strings...
				v = StringUtil.breakStringList(iline, " \t", StringUtil.DELIM_ALLOW_STRINGS | StringUtil.DELIM_SKIP_BLANKS);
				size = 0;
				if (v != null)
				{
					size = v.Count;
				}
				if (size < 4)
				{
					Message.printStatus(2, routine, "Ignoring line " + linecount + " not enough data values.  Have " + size + " expecting 4+");
					++errorCount;
					continue;
				}
				// Uncomment if testing...
				//Message.printStatus ( 2, routine, "" + v );

				// Allocate new plan node and set the values
				aReturn = new StateMod_ReturnFlow(smdataCompType);
				aReturn.setID(v[0].Trim());
				aReturn.setName(v[0].Trim()); // Same as ID
				aReturn.setCrtnid(v[1].Trim());
				aReturn.setCgoto(v[1].Trim()); // Redundant
				aReturn.setPcttot(v[2].Trim());
				aReturn.setIrtndl(v[3].Trim());
				if (v.Count > 4)
				{
					aReturn.setComment(v[4].Trim());
				}

				// Set the return to not dirty because it was just initialized...

				aReturn.setDirty(false);

				// Add the return to the list of returns
				theReturns.Add(aReturn);
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
		return theReturns;
	}

	/// <summary>
	/// Cancels any changes made to this object within a GUI since createBackup()
	/// was called and sets _original to null.
	/// </summary>
	public override void restoreOriginal()
	{
		StateMod_ReturnFlow rf = (StateMod_ReturnFlow)_original;
		base.restoreOriginal();

		__crtnid = rf.__crtnid;
		__pcttot = rf.__pcttot;
		__irtndl = rf.__irtndl;
		_isClone = false;
		_original = null;
	}

	/// <summary>
	/// Set the crtnid.
	/// </summary>
	public virtual void setCrtnid(string s)
	{
		if (!string.ReferenceEquals(s, null))
		{
			if (!s.Equals(__crtnid))
			{
				setDirty(true);
				if (!_isClone && _dataset != null)
				{
					_dataset.setDirty(_smdata_type, true);
				}
				__crtnid = s;
			}
		}
	}

	/// <summary>
	/// Set the delay table for return.
	/// </summary>
	public virtual void setIrtndl(int i)
	{
		if (i != __irtndl)
		{
			setDirty(true);
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(_smdata_type, true);
			}
			__irtndl = i;
		}
	}

	public virtual void setIrtndl(int? i)
	{
		setIrtndl(i.Value);
	}

	public virtual void setIrtndl(string str)
	{
		if (!string.ReferenceEquals(str, null))
		{
			setIrtndl(StringUtil.atoi(str.Trim()));
		}
	}

	/// <summary>
	/// Set the % of return flow to this river node.
	/// </summary>
	public virtual void setPcttot(double d)
	{
		if (d != __pcttot)
		{
			setDirty(true);
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(_smdata_type, true);
			}
			__pcttot = d;
		}
	}

	public virtual void setPcttot(double? d)
	{
		setPcttot(d.Value);
	}

	public virtual void setPcttot(string str)
	{
		if (!string.ReferenceEquals(str, null))
		{
			setPcttot(StringUtil.atod(str.Trim()));
		}
	}

	/// <summary>
	/// Returns a String representation of this object. </summary>
	/// <returns> a String representation of this object. </returns>
	public override string ToString()
	{
		return base.ToString() + ", " + __crtnid + ", " + __pcttot + ", " + __irtndl + ", " + __isMonthlyData;
	}

	/// <summary>
	/// Writes a list of StateMod_ReturnFlow objects to a list file.  A header is 
	/// printed to the top of the file, containing the commands used to generate the 
	/// file.  Any strings in the body of the file that contain the field delimiter will be wrapped in "...". </summary>
	/// <param name="filename"> the name of the file to which the data will be written. </param>
	/// <param name="delimiter"> the delimiter to use for separating field values. </param>
	/// <param name="update"> whether to update an existing file, retaining the current 
	/// header (true) or to create a new file with a new header. </param>
	/// <param name="data"> the list of objects to write. </param>
	/// <param name="newComments"> new comments to add to the header (e.g., command file, HydroBase version). </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeListFile(String filename, String delimiter, boolean update, java.util.List<StateMod_ReturnFlow> data, int componentType, java.util.List<String> newComments) throws Exception
	public static void writeListFile(string filename, string delimiter, bool update, IList<StateMod_ReturnFlow> data, int componentType, IList<string> newComments)
	{
		int size = 0;
		if (data != null)
		{
			size = data.Count;
		}

		IList<string> fields = new List<string>();
		fields.Add("ID");
		fields.Add("RiverNodeID");
		if (componentType == StateMod_DataSet.COMP_WELL_STATION_DEPLETION_TABLES)
		{
			   fields.Add("DepletionPercent");
		}
		else
		{
			fields.Add("ReturnPercent");
		}
		fields.Add("DelayTableID");
		fields.Add("Comment");
		int fieldCount = fields.Count;

		string[] names = new string[fieldCount];
		string[] formats = new string[fieldCount];
		int comp = componentType;
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
		PrintWriter @out = null;
		StateMod_ReturnFlow rf = null;
		IList<string> commentIndicators = new List<string>(1);
		commentIndicators.Add("#");
		IList<string> ignoredCommentIndicators = new List<string>(1);
		ignoredCommentIndicators.Add("#>");
		string[] line = new string[fieldCount];
		StringBuilder buffer = new StringBuilder();

		try
		{
			// Add some basic comments at the top of the file.  However, do this to a copy of the
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
			if (componentType == StateMod_DataSet.COMP_DIVERSION_STATION_DELAY_TABLES)
			{
				newComments2.Insert(1,"StateMod diversion delay (return) file.");
				newComments2.Insert(2,"See also the associated diversion station file.");
			}
			else if (componentType == StateMod_DataSet.COMP_WELL_STATION_DEPLETION_TABLES)
			{
				newComments2.Insert(1,"StateMod well depletion file.");
				newComments2.Insert(2,"See also the associated well station and return files.");
			}
			else if (componentType == StateMod_DataSet.COMP_WELL_STATION_DELAY_TABLES)
			{
				newComments2.Insert(1,"StateMod well delay (return) file.");
				newComments2.Insert(2,"See also the associated well station and depletion files.");
			}
			newComments2.Insert(3,"");
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
				rf = data[i];

				line[0] = StringUtil.formatString(rf.getID(), formats[0]).Trim();
				line[1] = StringUtil.formatString(rf.getCrtnid(), formats[1]).Trim();
				line[2] = StringUtil.formatString(rf.getPcttot(), formats[2]).Trim();
				line[3] = StringUtil.formatString(rf.getIrtndl(), formats[3]).Trim();
				line[4] = StringUtil.formatString(rf.getComment(), formats[4]).Trim();

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
	/// Write return flow information to a StateMod file.  History header information 
	/// is also maintained by calling this routine. </summary>
	/// <param name="instrfile"> input file from which previous history should be taken </param>
	/// <param name="outstrfile"> output file to which to write </param>
	/// <param name="stationType"> the station type, for the file header (e.g., "Plan", "Reservoir"). </param>
	/// <param name="theReturns"> list of plans to write. </param>
	/// <param name="newComments"> addition comments which should be included in history </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeStateModFile(String instrfile, String outstrfile, String stationType, java.util.List<StateMod_ReturnFlow> theReturns, java.util.List<String> newComments) throws Exception
	public static void writeStateModFile(string instrfile, string outstrfile, string stationType, IList<StateMod_ReturnFlow> theReturns, IList<string> newComments)
	{
		string routine = "StateMod_ReturnFlow.writeStateModFile";
		IList<string> commentIndicators = new List<string>(1);
		commentIndicators.Add("#");
		IList<string> ignoredCommentIndicators = new List<string>(1);
		ignoredCommentIndicators.Add("#>");
		PrintWriter @out = null;
		string comment;
		try
		{
			@out = IOUtil.processFileHeaders(IOUtil.getPathUsingWorkingDir(instrfile), IOUtil.getPathUsingWorkingDir(outstrfile), newComments, commentIndicators, ignoredCommentIndicators, 0);

			int i;
			string iline;
			string cmnt = "#>";
			// This format follows historical conventions found in example files, limited by StateMod ID lengths
			string formatLine1 = "%-12.12s %-12.12s %8.2f %-12.12s"; // Comment only written if not blank
			StateMod_ReturnFlow rf = null;
			IList<object> v = new List<object>(11); // Reuse for all output lines.

			@out.println(cmnt);
			@out.println(cmnt + "*************************************************");
			@out.println(cmnt + "  StateMod " + stationType + " Return Flows");
			@out.println(cmnt);
			@out.println(cmnt + "  Free format; however historical format based on StateMod");
			@out.println(cmnt + "               identifier string lengths is used for consistency.");
			@out.println(cmnt);
			@out.println(cmnt + "  ID                   :  " + stationType + " ID");
			@out.println(cmnt + "  River Node     crtnid:  River node identifier receiving return flow");
			@out.println(cmnt + "  Ret %         pcttot*:  Percent of return flow the the river node");
			@out.println(cmnt + "  Table ID      irtndl*:  Delay (return flow) table identifier for return");
			@out.println(cmnt + "  Comment              :  Optional (e.g., return type, name)");
			@out.println(cmnt + "                          Double quote to faciliate free-format processing.");
			@out.println(cmnt);
			@out.println(cmnt + " ID         River Node    Ret %    Table ID       Comment");
			@out.println(cmnt + "---------exb----------exb------exb----------exb-------------------------------e");
			@out.println(cmnt + "EndHeader");

			int num = 0;
			if (theReturns != null)
			{
				num = theReturns.Count;
			}
			for (i = 0; i < num; i++)
			{
				rf = theReturns[i];
				if (rf == null)
				{
					continue;
				}

				// line 1
				v.Clear();
				v.Add(rf.getID());
				v.Add(rf.getCrtnid());
				v.Add(rf.getPcttot());
				v.Add("" + rf.getIrtndl()); // Format as string
				comment = rf.getComment().Trim();
				if (comment.Length > 0)
				{
					comment = " \"" + comment + "\"";

				}
				iline = StringUtil.formatString(v, formatLine1) + comment;
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

	}

}