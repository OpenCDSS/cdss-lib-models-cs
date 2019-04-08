using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

// StateMod_Plan_WellAugmentation - This class stores Plan (Well Augmentation) data.

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

	using IOUtil = RTi.Util.IO.IOUtil;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// This class stores Plan (Well Augmentation) data.  The plan ID is stored in the StateMod_Data ID.
	/// </summary>
	public class StateMod_Plan_WellAugmentation : StateMod_Data, ICloneable, IComparable<StateMod_Data>
	{

	/// <summary>
	/// Well right ID.
	/// </summary>
	private string __cistatW;

	/// <summary>
	/// Well structure ID.
	/// </summary>
	private string __cistatS;

	/// <summary>
	/// Construct an instance.
	/// </summary>
	public StateMod_Plan_WellAugmentation() : base()
	{
		initialize();
	}

	/// <summary>
	/// Clones the data object. </summary>
	/// <returns> a cloned object. </returns>
	public override object clone()
	{
		StateMod_Plan_WellAugmentation rf = (StateMod_Plan_WellAugmentation)base.clone();
		rf._isClone = true;

		return rf;
	}

	/// <summary>
	/// Compares this object to another object based on the well structure ID and well right ID. </summary>
	/// <param name="data"> the object to compare against. </param>
	/// <returns> 0 if they are the same, 1 if this object is greater than the other object, or -1 if it is less. </returns>
	public virtual int CompareTo(StateMod_Data data)
	{
		int res = base.CompareTo(data);
		if (res != 0)
		{
			return res;
		}

		StateMod_Plan_WellAugmentation rf = (StateMod_Plan_WellAugmentation)data;

		// Strip off trailing "- name" - may be present if comparing in UI

		string cistatS = __cistatS;
		int index = cistatS.IndexOf(" - ", StringComparison.Ordinal);
		if (index > 0)
		{
			cistatS = cistatS.Substring(0, index).Trim();
		}

		string cistatS2 = rf.__cistatS;
		index = cistatS2.IndexOf(" - ", StringComparison.Ordinal);
		if (index > 0)
		{
			cistatS2 = cistatS2.Substring(0, index).Trim();
		}

		res = cistatS.CompareTo(cistatS2);
		if (res != 0)
		{
			return res;
		}

		string cistatW = __cistatW;
		index = cistatW.IndexOf(" - ", StringComparison.Ordinal);
		if (index > 0)
		{
			cistatW = cistatW.Substring(0, index).Trim();
		}

		string cistatW2 = rf.__cistatW;
		index = cistatW2.IndexOf(" - ", StringComparison.Ordinal);
		if (index > 0)
		{
			cistatW2 = cistatW2.Substring(0, index).Trim();
		}

		return cistatW.CompareTo(cistatW2);
	}

	/// <summary>
	/// Creates a backup of the current data object and stores it in _original,
	/// for use in determining if an object was changed inside of a GUI.
	/// </summary>
	public virtual void createBackup()
	{
		_original = (StateMod_Plan_WellAugmentation)clone();
		((StateMod_Plan_WellAugmentation)_original)._isClone = false;
		_isClone = true;
	}

	/// <summary>
	/// Compare two return flow lists and see if they are the same. </summary>
	/// <param name="v1"> the first list of Plan_WellAugmentation to check.  Cannot be null. </param>
	/// <param name="v2"> the second list of Plan_WellAugmentation to check.  Cannot be null. </param>
	/// <returns> true if they are the same, false if not. </returns>
	public static bool Equals(IList<StateMod_Plan_WellAugmentation> v1, IList<StateMod_Plan_WellAugmentation> v2)
	{
		string routine = "StateMod_Plan_WellAugmentation.equals";
		StateMod_Plan_WellAugmentation rf1;
		StateMod_Plan_WellAugmentation rf2;
		if (v1.Count != v2.Count)
		{
			Message.printStatus(2, routine, "Well augmentation lists are different sizes");
			return false;
		}
		else
		{
			// Sort the lists and compare item-by-item.  Any differences
			// and data will need to be saved back into the data set.
			int size = v1.Count;
			//Message.printStatus(2, routine, "Well augmentation lists are of size: " + size);
			IList<StateMod_Plan_WellAugmentation> v1Sort = StateMod_Util.sortStateMod_DataVector(v1);
			IList<StateMod_Plan_WellAugmentation> v2Sort = StateMod_Util.sortStateMod_DataVector(v2);
			//Message.printStatus(2, routine, "Well augmentation lists have been sorted");

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
	public virtual bool Equals(StateMod_Plan_WellAugmentation rf)
	{
		if (!base.Equals(rf))
		{
			 return false;
		}
		if (rf.__cistatW.Equals(__cistatW) && rf.__cistatS.Equals(__cistatS))
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
	~StateMod_Plan_WellAugmentation()
	{
		__cistatW = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Retrieve the structure ID.
	/// </summary>
	public virtual string getCistatS()
	{
		return __cistatS;
	}

	/// <summary>
	/// Return the cistatW.
	/// </summary>
	public virtual string getCistatW()
	{
		return __cistatW;
	}

	private void initialize()
	{
		_smdata_type = StateMod_DataSet.COMP_PLAN_WELL_AUGMENTATION;
		__cistatW = "";
		__cistatS = "";
	}

	/// <summary>
	/// Read return information in and store in a list. </summary>
	/// <param name="filename"> filename for data file to read </param>
	/// <exception cref="Exception"> if an error occurs </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.List<StateMod_Plan_WellAugmentation> readStateModFile(String filename) throws Exception
	public static IList<StateMod_Plan_WellAugmentation> readStateModFile(string filename)
	{
		string routine = "StateMod_Plan_WellAugmentation.readStateModFile";
		string iline = null;
		IList<string> v = new List<string>(9);
		IList<StateMod_Plan_WellAugmentation> theWellAugs = new List<StateMod_Plan_WellAugmentation>();
		int linecount = 0;

		StateMod_Plan_WellAugmentation aWellAug = null;
		StreamReader @in = null;

		Message.printStatus(2, routine, "Reading well augmentation plan file: " + filename);
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
				if (size < 3)
				{
					Message.printStatus(2, routine, "Ignoring line " + linecount + " not enough data values.  Have " + size + " expecting 3");
					++errorCount;
					continue;
				}
				// Uncomment if testing...
				//Message.printStatus ( 2, routine, "" + v );

				// Allocate new plan node and set the values
				aWellAug = new StateMod_Plan_WellAugmentation();
				aWellAug.setID(v[0].Trim());
				aWellAug.setName(v[0].Trim()); // Same as ID
				aWellAug.setCistatW(v[1].Trim());
				aWellAug.setCistatS(v[2].Trim());
				if (v.Count > 3)
				{
					aWellAug.setComment(v[3].Trim());
				}

				// Set the return to not dirty because it was just initialized...

				aWellAug.setDirty(false);

				// Add the return to the list of returns
				theWellAugs.Add(aWellAug);
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
		return theWellAugs;
	}

	/// <summary>
	/// Cancels any changes made to this object within a GUI since createBackup()
	/// was called and sets _original to null.
	/// </summary>
	public override void restoreOriginal()
	{
		StateMod_Plan_WellAugmentation rf = (StateMod_Plan_WellAugmentation)_original;
		base.restoreOriginal();

		__cistatW = rf.__cistatW;
		__cistatS = rf.__cistatS;
		_isClone = false;
		_original = null;
	}

	/// <summary>
	/// Set the cistatS.
	/// </summary>
	public virtual void setCistatS(string cistatS)
	{
		if (!string.ReferenceEquals(cistatS, null))
		{
			if (!cistatS.Equals(__cistatS))
			{
				setDirty(true);
				if (!_isClone && _dataset != null)
				{
					_dataset.setDirty(_smdata_type, true);
				}
				__cistatS = cistatS;
			}
		}
	}

	/// <summary>
	/// Set the cistatW.
	/// </summary>
	public virtual void setCistatW(string cistatW)
	{
		if (!string.ReferenceEquals(cistatW, null))
		{
			if (!cistatW.Equals(__cistatW))
			{
				setDirty(true);
				if (!_isClone && _dataset != null)
				{
					_dataset.setDirty(_smdata_type, true);
				}
				__cistatW = cistatW;
			}
		}
	}

	/// <summary>
	/// Returns a String representation of this object. </summary>
	/// <returns> a String representation of this object. </returns>
	public override string ToString()
	{
		return base.ToString() + ", " + __cistatW + ", " + __cistatS;
	}

	/// <summary>
	/// Writes a list of StateMod_Plan_WellAugmentation objects to a list file.  A header is 
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
//ORIGINAL LINE: public static void writeListFile(String filename, String delimiter, boolean update, java.util.List<StateMod_Plan_WellAugmentation> data, int componentType, java.util.List<String> newComments) throws Exception
	public static void writeListFile(string filename, string delimiter, bool update, IList<StateMod_Plan_WellAugmentation> data, int componentType, IList<string> newComments)
	{
		int size = 0;
		if (data != null)
		{
			size = data.Count;
		}

		IList<string> fields = new List<string>();
		fields.Add("PlanID");
		fields.Add("WellRightID");
		fields.Add("WellStructureID");
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
		StateMod_Plan_WellAugmentation wellAug = null;
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
			newComments2.Insert(1,"StateMod well augmentation plan data file.");
			newComments2.Insert(2,"See also the associated plan station file.");
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
				wellAug = data[i];

				line[0] = StringUtil.formatString(wellAug.getID(), formats[0]).Trim();
				line[1] = StringUtil.formatString(wellAug.getCistatW(), formats[1]).Trim();
				line[2] = StringUtil.formatString(wellAug.getCistatS(), formats[2]).Trim();

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
			@out.flush();
			@out.close();
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
	/// Write well augmentation data to a StateMod file.  History header information 
	/// is also maintained by calling this routine. </summary>
	/// <param name="instrfile"> input file from which previous history should be taken </param>
	/// <param name="outstrfile"> output file to which to write </param>
	/// <param name="wellAugList"> list of plans to write. </param>
	/// <param name="newComments"> addition comments which should be included in history </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeStateModFile(String instrfile, String outstrfile, java.util.List<StateMod_Plan_WellAugmentation> wellAugList, java.util.List<String> newComments) throws Exception
	public static void writeStateModFile(string instrfile, string outstrfile, IList<StateMod_Plan_WellAugmentation> wellAugList, IList<string> newComments)
	{
		string routine = "StateMod_Plan_WellAugmentation.writeStateModFile";
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
			string formatLine1 = "%-12.12s %-12.12s %-12.12s"; // Comment only written if not blank
			StateMod_Plan_WellAugmentation wellAug = null;
			IList<object> v = new List<object>(11); // Reuse for all output lines.

			@out.println(cmnt);
			@out.println(cmnt + "*************************************************");
			@out.println(cmnt + "  StateMod Well Augmentation Plan Data");
			@out.println(cmnt);
			@out.println(cmnt + "  Free format; however historical format based on StateMod");
			@out.println(cmnt + "               identifier string lengths is used for consistency.");
			@out.println(cmnt);
			@out.println(cmnt + "  Plan ID      cistatP :  Plan identifier");
			@out.println(cmnt + "  WellRightID  cistatW :  Well right identifier");
			@out.println(cmnt + "  Well ID      cistatS :  Well (structure) identifier");
			@out.println(cmnt + "  Comment              :  Optional comments");
			@out.println(cmnt + "                          Double quote to faciliate free-format processing.");
			@out.println(cmnt);
			@out.println(cmnt + " Plan ID    WellRightID    Well ID        Comment");
			@out.println(cmnt + "---------exb----------exb----------exb-------------------------------e");
			@out.println(cmnt + "EndHeader");

			int num = 0;
			if (wellAugList != null)
			{
				num = wellAugList.Count;
			}
			for (i = 0; i < num; i++)
			{
				wellAug = wellAugList[i];
				if (wellAug == null)
				{
					continue;
				}

				// line 1
				v.Clear();
				v.Add(wellAug.getID());
				v.Add(wellAug.getCistatW());
				v.Add(wellAug.getCistatS());
				comment = wellAug.getComment().Trim();
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