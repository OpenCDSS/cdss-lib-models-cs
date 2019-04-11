using System;
using System.Collections.Generic;
using System.Text;

// StateMod_ReservoirAreaCap - class to store area capacity values for reservoir

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
// StateMod_ReservoirAreaCap - class to store area capacity values for reservoir
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
// 
// 02 Sep 1997	Catherine E.		Created initial version of class.
//		Nutting-Lane, RTi
// 11 Feb 1998	CEN, RTi		Added _dataset.setDirty
//					to all set
//					commands.
// 17 Feb 2001	Steven A. Malers, RTi	Add finalize().  Clean up javadoc.
//					Handle nulls.  Set variables to null
//					when done.  Alphabetize methods.
// 2002-09-19	SAM, RTi		Use isDirty()instead of setDirty()to
//					indicate edits.
//------------------------------------------------------------------------------
// 2003-06-05	J. Thomas Sapienza 	Initial StateMod_ version.
// 2003-06-10	JTS, RTi		Renamed from StateMod_AreaCap
// 2003-07-15	JTS, RTi		Changed to use new dataset design.
// 2003-08-03	SAM, RTi		Change isDirty() to setDirty().
// 2003-10-09	JTS, RTi		* Implemented Cloneable.
//					* Added clone().
//					* Added equals().
//					* Implemented Comparable.
//					* Added compareTo().
// 					* Added equals(Vector, Vector)
// 2003-10-15	JTS, RTi		* Revised the clone() code.
//					* Added toString().
// 2004-07-02	SAM, RTi		Handle null _dataset.
// 2005-01-17	JTS, RTi		* Added createBackup().
//					* Added restoreOriginal().
// 2005-03-30	JTS, RTi		Corrected class mis-type in 
//					createBackup().
// 2005-04-18	JTS, RTi		Added writeListFile().
//------------------------------------------------------------------------------
// EndHeader

namespace DWR.StateMod
{

	using IOUtil = RTi.Util.IO.IOUtil;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// Object used to store reservoir area capacity information in.
	/// Any calls to "set" routines sets the _dataset.COMP_RESERVOIR_STATIONS flag dirty.
	/// </summary>
	public class StateMod_ReservoirAreaCap : StateMod_Data //, ICloneable, IComparable<StateMod_Data>
	{

	/// <summary>
	/// Content in area cap table.
	/// </summary>
	protected internal double _conten;
	/// <summary>
	/// Area associated with the content.
	/// </summary>
	protected internal double _surarea;
	/// <summary>
	/// Seepage associated with the content.
	/// </summary>
	protected internal double _seepage;

	/// <summary>
	/// Constructor.
	/// </summary>
	public StateMod_ReservoirAreaCap() : base()
	{
		initialize();
	}

	/// <summary>
	/// Clones the data object. </summary>
	/// <returns> a cloned object. </returns>
	//public override object clone()
	//{
	//	StateMod_ReservoirAreaCap ac = (StateMod_ReservoirAreaCap)base.clone();
	//	ac._isClone = true;
	//	return ac;
	//}

	/// <summary>
	/// Compares this object to another StateMod_Data object based on the sorted
	/// order from the StateMod_Data variables, and then by conten, surarea and seepage, in that order. </summary>
	/// <param name="data"> the object to compare against. </param>
	/// <returns> 0 if they are the same, 1 if this object is greater than the other
	/// object, or -1 if it is less. </returns>
	//public virtual int CompareTo(StateMod_Data data)
	//{
	//	int res = base.CompareTo(data);
	//	if (res != 0)
	//	{
	//		return res;
	//	}

	//	StateMod_ReservoirAreaCap ac = (StateMod_ReservoirAreaCap)data;

	//	if (_conten < ac._conten)
	//	{
	//		return -1;
	//	}
	//	else if (_conten > ac._conten)
	//	{
	//		return 1;
	//	}

	//	if (_surarea < ac._surarea)
	//	{
	//		return -1;
	//	}
	//	else if (_surarea > ac._surarea)
	//	{
	//		return 1;
	//	}

	//	if (_seepage < ac._seepage)
	//	{
	//		return -1;
	//	}
	//	else if (_seepage > ac._seepage)
	//	{
	//		return 1;
	//	}

	//	return 0;
	//}

	///// <summary>
	///// Creates a backup of the current data object and stores it in _original,
	///// for use in determining if an object was changed inside of a GUI.
	///// </summary>
	//public virtual void createBackup()
	//{
	//	_original = (StateMod_ReservoirAreaCap)clone();
	//	((StateMod_ReservoirAreaCap)_original)._isClone = false;
	//	_isClone = true;
	//}

	///// <summary>
	///// Compare two lists and see if they are the same. </summary>
	///// <param name="v1"> the first list of StateMod_ReservoirAreaCap to check.  Cannot be null. </param>
	///// <param name="v2"> the second list of StateMod_ReservoirAreaCap to check.  Cannot be null. </param>
	///// <returns> true if they are the same, false if not. </returns>
	//public static bool Equals(IList<StateMod_ReservoirAreaCap> v1, IList<StateMod_ReservoirAreaCap> v2)
	//{
	//	string routine = "StateMod_ReservoirAreaCap.equals";
	//	StateMod_ReservoirAreaCap r1;
	//	StateMod_ReservoirAreaCap r2;
	//	if (v1.Count != v2.Count)
	//	{
	//		Message.printStatus(2, routine, "Lists are different sizes");
	//		return false;
	//	}
	//	else
	//	{
	//		// sort the lists and compare item-by-item.  Any differences
	//		// and data will need to be saved back into the dataset.
	//		int size = v1.Count;
	//		Message.printStatus(1, routine, "Lists are of size: " + size);
	//		IList<StateMod_ReservoirAreaCap> v1Sort = StateMod_Util.sortStateMod_DataVector(v1);
	//		IList<StateMod_ReservoirAreaCap> v2Sort = StateMod_Util.sortStateMod_DataVector(v2);
	//		Message.printStatus(2, routine, "Vectors have been sorted");

	//		for (int i = 0; i < size; i++)
	//		{
	//			r1 = v1Sort[i];
	//			r2 = v2Sort[i];
	//			Message.printStatus(1, routine, r1.ToString());
	//			Message.printStatus(1, routine, r2.ToString());
	//			Message.printStatus(1, routine, "Element " + i + " comparison: " + r1.CompareTo(r2));
	//			if (r1.CompareTo(r2) != 0)
	//			{
	//				return false;
	//			}
	//		}
	//	}
	//	return true;
	//}

	/// <summary>
	/// Tests to see if two diversion rights are equal.  Strings are compared with case sensitivity. </summary>
	/// <param name="ac"> the ac to compare. </param>
	/// <returns> true if they are equal, false otherwise. </returns>
	public virtual bool Equals(StateMod_ReservoirAreaCap ac)
	{
		if (!base.Equals(ac))
		{
			 return false;
		}

		if (_conten == ac._conten && _surarea == ac._surarea && _seepage == ac._seepage)
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
	~StateMod_ReservoirAreaCap()
	{
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Returns the content in area capacity table. </summary>
	/// <returns> the content in area capacity table. </returns>
	public virtual double getConten()
	{
		return _conten;
	}

	/// <summary>
	/// Returns the seepage associated with the content. </summary>
	/// <returns> the seepage associated with the content. </returns>
	public virtual double getSeepage()
	{
		return _seepage;
	}

	/// <summary>
	/// Returns the area associated with the content. </summary>
	/// <returns> the area associated with the content. </returns>
	public virtual double getSurarea()
	{
		return _surarea;
	}

	/// <summary>
	/// Initializes member variables.
	/// </summary>
	private void initialize()
	{
		_conten = 0;
		_surarea = 0;
		_seepage = 0;
	}

	/// <summary>
	/// Cancels any changes made to this object within a GUI since createBackup()
	/// was called and sets _original to null.
	/// </summary>
	//public override void restoreOriginal()
	//{
	//	StateMod_ReservoirAreaCap ac = (StateMod_ReservoirAreaCap)_original;
	//	base.restoreOriginal();

	//	_conten = ac._conten;
	//	_surarea = ac._surarea;
	//	_seepage = ac._seepage;
	//	_isClone = false;
	//	_original = null;
	//}

	/// <summary>
	/// Set the content in area capacity table. </summary>
	/// <param name="d"> content to set </param>
	public virtual void setConten(double d)
	{
		if (d != _conten)
		{
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_RESERVOIR_STATIONS,true);
			}
			_conten = d;
		}
	}

	/// <summary>
	/// Set the content in area capacity table. </summary>
	/// <param name="d"> content to set </param>
	public virtual void setConten(double? d)
	{
		setConten(d.Value);
	}

	/// <summary>
	/// Set the content in area capacity table. </summary>
	/// <param name="str"> content to set </param>
	public virtual void setConten(string str)
	{
		if (string.ReferenceEquals(str, null))
		{
			return;
		}
		setConten(double.Parse(str.Trim()));
	}

	/// <summary>
	/// Set the seepage associated with the content. </summary>
	/// <param name="d"> seepage to set </param>
	public virtual void setSeepage(double d)
	{
		if (d != _seepage)
		{
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_RESERVOIR_STATIONS,true);
			}
			_seepage = d;
		}
	}

	/// <summary>
	/// Set the seepage associated with the content. </summary>
	/// <param name="d"> seepage to set </param>
	public virtual void setSeepage(double? d)
	{
		setSeepage(d.Value);
	}

	/// <summary>
	/// Set the seepage associated with the content. </summary>
	/// <param name="str"> seepage to set </param>
	public virtual void setSeepage(string str)
	{
		if (string.ReferenceEquals(str, null))
		{
			return;
		}
		setSeepage(double.Parse(str.Trim()));
	}

	/// <summary>
	/// Set the area associated with the content. </summary>
	/// <param name="d"> content to set </param>
	public virtual void setSurarea(double d)
	{
		if (d != _surarea)
		{
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_RESERVOIR_STATIONS,true);
			}
			_surarea = d;
		}
	}

	/// <summary>
	/// Set the area associated with the content. </summary>
	/// <param name="d"> content to set </param>
	public virtual void setSurarea(double? d)
	{
		setSurarea(d.Value);
	}

	/// <summary>
	/// Set the area associated with the content. </summary>
	/// <param name="str"> content to set </param>
	public virtual void setSurarea(string str)
	{
		if (string.ReferenceEquals(str, null))
		{
			return;
		}
		setSurarea(double.Parse(str.Trim()));
	}

	/// <summary>
	/// Returns a String representation of this object. </summary>
	/// <returns> a String representation of this object. </returns>
	public override string ToString()
	{
		return base.ToString() + ", " + _conten + ", " + _surarea + ", " + _seepage;
	}

	/// <summary>
	/// Writes a list of StateMod_ReservoirAreaCap objects to a list file.  A header 
	/// is printed to the top of the file, containing the commands used to generate the 
	/// file.  Any strings in the body of the file that contain the field delimiter will be wrapped in "...". </summary>
	/// <param name="filename"> the name of the file to which the data will be written. </param>
	/// <param name="delimiter"> the delimiter to use for separating field values. </param>
	/// <param name="update"> whether to update an existing file, retaining the current 
	/// header (true) or to create a new file with a new header. </param>
	/// <param name="data"> the list of objects to write. </param>
	/// <param name="newComments"> new comments to add at the top of the file. </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeListFile(String filename, String delimiter, boolean update, java.util.List<StateMod_ReservoirAreaCap> data, java.util.List<String> newComments) throws Exception
	//public static void writeListFile(string filename, string delimiter, bool update, IList<StateMod_ReservoirAreaCap> data, IList<string> newComments)
	//{
	//	string routine = "StateMod_ReservoirAreaCap.writeListFile";
	//	int size = 0;
	//	if (data != null)
	//	{
	//		size = data.Count;
	//	}

	//	IList<string> fields = new List<string>();
	//	fields.Add("ReservoirID");
	//	fields.Add("Content");
	//	fields.Add("Area");
	//	fields.Add("Seepage");
	//	int fieldCount = fields.Count;

	//	string[] names = new string[fieldCount];
	//	string[] formats = new string[fieldCount];
	//	int comp = StateMod_Util.COMP_RESERVOIR_AREA_CAP;
	//	string s = null;
	//	for (int i = 0; i < fieldCount; i++)
	//	{
	//		s = (string)fields[i];
	//		names[i] = StateMod_Util.lookupPropValue(comp, "FieldName", s);
	//		formats[i] = StateMod_Util.lookupPropValue(comp, "Format", s);
	//	}

	//	string oldFile = null;
	//	if (update)
	//	{
	//		oldFile = IOUtil.getPathUsingWorkingDir(filename);
	//	}

	//	int j = 0;
	//	PrintWriter @out = null;
	//	StateMod_ReservoirAreaCap area = null;
	//	string[] line = new string[fieldCount];
	//	IList<string> commentIndicators = new List<string>(1);
	//	commentIndicators.Add("#");
	//	IList<string> ignoredCommentIndicators = new List<string>(1);
	//	ignoredCommentIndicators.Add("#>");
	//	StringBuilder buffer = new StringBuilder();

	//	try
	//	{
	//		// Add some basic comments at the top of the file.  Do this to a copy of the
	//		// incoming comments so that they are not modified in the calling code.
	//		IList<string> newComments2 = null;
	//		if (newComments == null)
	//		{
	//			newComments2 = new List<string>();
	//		}
	//		else
	//		{
	//			newComments2 = new List<string>(newComments);
	//		}
	//		newComments2.Insert(0,"");
	//		newComments2.Insert(1,"StateMod reservoir content/area/seepage data as a delimited list file.");
	//		newComments2.Insert(2,"See also the associated station, account, precipitation station,");
	//		newComments2.Insert(3,"evaporation station, and collection files.");
	//		newComments2.Insert(4,"");
	//		@out = IOUtil.processFileHeaders(oldFile, IOUtil.getPathUsingWorkingDir(filename), newComments2, commentIndicators, ignoredCommentIndicators, 0);

	//		for (int i = 0; i < fieldCount; i++)
	//		{
	//			if (i > 0)
	//			{
	//				buffer.Append(delimiter);
	//			}
	//			buffer.Append("\"" + names[i] + "\"");
	//		}

	//		@out.println(buffer.ToString());

	//		for (int i = 0; i < size; i++)
	//		{
	//			area = (StateMod_ReservoirAreaCap)data[i];

	//			line[0] = StringUtil.formatString(area.getCgoto(),formats[0]).Trim();
	//			line[1] = StringUtil.formatString(area.getConten(),formats[1]).Trim();
	//			line[2] = StringUtil.formatString(area.getSurarea(),formats[2]).Trim();
	//			line[3] = StringUtil.formatString(area.getSeepage(),formats[3]).Trim();

	//			buffer = new StringBuilder();
	//			for (j = 0; j < fieldCount; j++)
	//			{
	//				if (j > 0)
	//				{
	//					buffer.Append(delimiter);
	//				}
	//				if (line[j].IndexOf(delimiter, StringComparison.Ordinal) > -1)
	//				{
	//					line[j] = "\"" + line[j] + "\"";
	//				}
	//				buffer.Append(line[j]);
	//			}

	//			@out.println(buffer.ToString());
	//		}
	//	}
	//	catch (Exception e)
	//	{
	//		Message.printWarning(3, routine, e);
	//		throw e;
	//	}
	//	finally
	//	{
	//		if (@out != null)
	//		{
	//			@out.flush();
	//			@out.close();
	//		}
	//	}
	//}

	}

}