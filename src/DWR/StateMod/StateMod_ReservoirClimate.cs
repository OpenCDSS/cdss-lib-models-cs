using System;
using System.Collections.Generic;
using System.Text;

// StateMod_ReservoirClimate - class to store reservoir climate information,
// including evaporation and precipitation stations

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
// StateMod_ReservoirClimate - class to store reservoir climate information,
//	including evaporation and precipitation stations
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
// 
// 02 Sep 1997	Catherine E.		Created initial version of class.
//		Nutting-Lane, RTi
// 11 Feb 1998	CEN, RTi		Add StateMod_DataSet.setDirty 
//					to all set
//					commands.
// 01 Apr 1998	CEN, RTi		Added javadoc comments.
// 21 Dec 1998	CEN, RTi		Added throws IOException to read/write
//					routines.
// 17 Feb 2001	Steven A. Malers, RTi	Code review.  Clean up javadoc.  Add
//					finalize().  Handle nulls and set unused
//					variables to null.  Alphabetize code.
// 2002-09-19	SAM, RTi		Use isDirty()instead of setDirty()to
//					indicate edits.
//------------------------------------------------------------------------------
// 2003-06-05	J. Thomas Sapienza 	Initial StateMod_ version.
// 2003-07-15	JTS, RTi		Changed code to use new DatSet design.
// 2003-08-03	SAM, RTi		Changed isDirty() back to setDirty().
// 2003-08-16	SAM, RTi		* Change name of class from
//					  StateMod_Climate to
//					  StateMod_ReservoirClimate because the
//					  data are really for assigning climate
//					  stations to reservoirs.
//					* Remove exceptions from getNumEvap()
//					  and getNumPrecip() since it was
//					  somewhat redundant.
//					* Use the base class identifier data
//					  member than the old cevap_cprer
//					  data member - one less data member to
//					  manage and the old name is ugly.
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
	/// This class stores reservoir climate information, including evaporation and 
	/// precipitation stations, which pertain to an individual reservoir.
	/// The actual time series data are stored as separate components and are shared among reservoirs.
	/// Any calls to "set" routines sets the StateMod_DataSet.COMP_RESERVOIR_STATIONS dirty.
	/// </summary>
	public class StateMod_ReservoirClimate : StateMod_Data //, ICloneable, IComparable<StateMod_Data>
	{

	/// <summary>
	/// Precipitation station type.
	/// </summary>
	public const int CLIMATE_PTPX = 0;
	/// <summary>
	/// Evaporation station type.
	/// </summary>
	public const int CLIMATE_EVAP = 1;

	/// <summary>
	/// CLIMATE_PTPX or CLIMATE_EVAP
	/// </summary>
	protected internal int _type;

	/// <summary>
	/// Percent of this station to use.
	/// </summary>
	protected internal double _weight;

	/// <summary>
	/// Constructor allocates a String for the station id, sets the weight to 0 and
	/// sets the type to an invalid value so developer must set to a valid value.
	/// </summary>
	public StateMod_ReservoirClimate() : base()
	{
		initialize();
	}

	/// <summary>
	/// Clones the data object. </summary>
	/// <returns> a cloned object. </returns>
	//public override object clone()
	//{
	//	StateMod_ReservoirClimate rc = (StateMod_ReservoirClimate)base.clone();
	//	rc._isClone = true;
	//	return rc;
	//}

	/// <summary>
	/// Compares this object to another StateMod_Data object based on the sorted
	/// order from the StateMod_Data variables, and then by _type and _weight, in that order. </summary>
	/// <param name="data"> the object to compare against. </param>
	/// <returns> 0 if they are the same, 1 if this object is greater than the other object, or -1 if it is less. </returns>
	//public virtual int CompareTo(StateMod_Data data)
	//{
	//	int res = base.CompareTo(data);
	//	if (res != 0)
	//	{
	//		return res;
	//	}

	//	StateMod_ReservoirClimate rc = (StateMod_ReservoirClimate)data;
	//	if (_type < rc._type)
	//	{
	//		return -1;
	//	}
	//	else if (_type > rc._type)
	//	{
	//		return 1;
	//	}

	//	if (_weight < rc._weight)
	//	{
	//		return -1;
	//	}
	//	else if (_weight > rc._weight)
	//	{
	//		return 1;
	//	}

	//	return 0;
	//}

	/// <summary>
	/// Creates a backup of the current data object and stores it in _original,
	/// for use in determining if an object was changed inside of a GUI.
	/// </summary>
	//public virtual void createBackup()
	//{
	//	_original = (StateMod_ReservoirClimate)clone();
	//	((StateMod_ReservoirClimate)_original)._isClone = false;
	//	_isClone = true;
	//}

	/// <summary>
	/// Compare two rights Vectors and see if they are the same. </summary>
	/// <param name="v1"> the first Vector of StateMod_ReservoirClimate s to check.  Cannot be null. </param>
	/// <param name="v2"> the second Vector of StateMod_ReservoirClimate s to check.  Cannot be null. </param>
	/// <returns> true if they are the same, false if not. </returns>
	//public static bool Equals(IList<StateMod_ReservoirClimate> v1, IList<StateMod_ReservoirClimate> v2)
	//{
	//	string routine = "StateMod_ReservoirClimate.equals(Vector, Vector)";
	//	StateMod_ReservoirClimate r1;
	//	StateMod_ReservoirClimate r2;
	//	if (v1.Count != v2.Count)
	//	{
	//		Message.printStatus(1, routine, "Lists are different sizes");
	//		return false;
	//	}
	//	else
	//	{
	//		// sort the Vectors and compare item-by-item.  Any differences
	//		// and data will need to be saved back into the dataset.
	//		int size = v1.Count;
	//		Message.printStatus(1, routine, "Lists are of size: " + size);
	//		IList<StateMod_ReservoirClimate> v1Sort = StateMod_Util.sortStateMod_DataVector(v1);
	//		IList<StateMod_ReservoirClimate> v2Sort = StateMod_Util.sortStateMod_DataVector(v2);
	//		Message.printStatus(1, routine, "Lists have been sorted");

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
	/// <param name="rc"> the rc to compare. </param>
	/// <returns> true if they are equal, false otherwise. </returns>
	public virtual bool Equals(StateMod_ReservoirClimate rc)
	{
		if (!base.Equals(rc))
		{
			 return false;
		}

		if (_type == rc._type && _weight == rc._weight)
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
	~StateMod_ReservoirClimate()
	{
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Returns the type </summary>
	/// <returns> _type </returns>
	public virtual int getType()
	{
		return _type;
	}

	/// <summary>
	/// Returns the weight </summary>
	/// <returns> _weight </returns>
	public virtual double getWeight()
	{
		return _weight;
	}

	/// <summary>
	/// Initializes member variables.
	/// </summary>
	private void initialize()
	{
		_type = 2; // not valid - forces us to set
		_weight = 0;
	}

	/// <summary>
	/// Cancels any changes made to this object within a GUI since createBackup()
	/// was called and sets _original to null.
	/// </summary>
	//public override void restoreOriginal()
	//{
	//	StateMod_ReservoirClimate clim = (StateMod_ReservoirClimate)_original;
	//	base.restoreOriginal();

	//	_type = clim._type;
	//	_weight = clim._weight;
	//	_isClone = false;
	//	_original = null;
	//}

	/// <summary>
	/// Set the type.  Use either CLIMATE_PTPX or CLIMATE_EVAP. </summary>
	/// <param name="i"> the type to set </param>
	public virtual void setType(int i)
	{
		if (i != _type)
		{
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_RESERVOIR_STATIONS,true);
			}
			_type = i;
		}
	}

	/// <summary>
	/// Set the weight. </summary>
	/// <param name="d"> weight to set </param>
	public virtual void setWeight(double d)
	{
		if (d != _weight)
		{
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_RESERVOIR_STATIONS,true);
			}
			_weight = d;
		}
	}

	/// <summary>
	/// Set the weight. </summary>
	/// <param name="d"> weight to set </param>
	public virtual void setWeight(double? d)
	{
		setWeight(d.Value);
	}

	/// <summary>
	/// Set the weight. </summary>
	/// <param name="str"> weight to set </param>
	public virtual void setWeight(string str)
	{
		if (!string.ReferenceEquals(str, null))
		{
			setWeight(double.Parse(str.Trim()));
		}
	}

	/// <summary>
	/// Calculates the number of StateMod_ReservoirClimate objects that are evaporation stations. </summary>
	/// <returns> the number of StateMod_ReservoirClimate objects that are evaporation stations. </returns>
	public static int getNumEvap(IList<StateMod_ReservoirClimate> climates)
	{
		if (climates == null)
		{
			return 0;
		}

		int nevap = 0;
		int num = climates.Count;

		for (int i = 0; i < num; i++)
		{
			if (climates[i].getType() == CLIMATE_EVAP)
			{
				nevap++;
			}
		}
		return nevap;
	}

	/// <summary>
	/// Calculates the number of StateMod_ReservoirClimate objects that are precipitation stations. </summary>
	/// <returns> the number of StateMod_ReservoirClimate objects that are precipitation stations. </returns>
	public static int getNumPrecip(IList<StateMod_ReservoirClimate> climates)
	{
		if (climates == null)
		{
			return 0;
		}

		int nptpx = 0;
		int num = climates.Count;

		for (int i = 0; i < num; i++)
		{
			if (climates[i].getType() == CLIMATE_PTPX)
			{
				nptpx++;
			}
		}
		return nptpx;
	}

	/// <summary>
	/// Returns a String representation of this object. </summary>
	/// <returns> a String representation of this object. </returns>
	public override string ToString()
	{
		return base.ToString() + ", " + _type + ", " + _weight;
	}

	/// <summary>
	/// Writes a list of StateMod_ReservoirClimate objects to a list file.  A header 
	/// is printed to the top of the file, containing the commands used to generate the 
	/// file.  Any strings in the body of the file that contain the field delimiter will be wrapped in "...". </summary>
	/// <param name="filename"> the name of the file to which the data will be written. </param>
	/// <param name="delimiter"> the delimiter to use for separating field values. </param>
	/// <param name="update"> whether to update an existing file, retaining the current 
	/// header (true) or to create a new file with a new header. </param>
	/// <param name="data"> the Vector of objects to write. </param>
	/// <param name="componentType"> one of either StateMod_DataSet.COMP_RESERVOIR_PRECIP_STATIONS or
	/// StateMod_DataSet.COMP_RESERVOIR_EVAP_STATIONS. </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeListFile(String filename, String delimiter, boolean update, java.util.List<StateMod_ReservoirClimate> data, java.util.List<String> newComments, int componentType) throws Exception
	//public static void writeListFile(string filename, string delimiter, bool update, IList<StateMod_ReservoirClimate> data, IList<string> newComments, int componentType)
	//{
	//	string routine = "StateMod_ReservoirClimate.writeListFile";
	//	int size = 0;
	//	if (data != null)
	//	{
	//		size = data.Count;
	//	}

	//	IList<string> fields = new List<string>();
	//	fields.Add("ReservoirID");
	//	fields.Add("StationID");
	//	fields.Add("PercentWeight");
	//	int fieldCount = fields.Count;

	//	string[] names = new string[fieldCount];
	//	string[] formats = new string[fieldCount];
	//	int comp = componentType;
	//	string s = null;
	//	for (int i = 0; i < fieldCount; i++)
	//	{
	//		s = fields[i];
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
	//	StateMod_ReservoirClimate cli = null;
	//	IList<string> commentIndicators = new List<string>(1);
	//	commentIndicators.Add("#");
	//	IList<string> ignoredCommentIndicators = new List<string>(1);
	//	ignoredCommentIndicators.Add("#>");
	//	string[] line = new string[fieldCount];
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
	//		if (componentType == StateMod_DataSet.COMP_RESERVOIR_STATION_EVAP_STATIONS)
	//		{
	//			newComments2.Insert(1,"StateMod reservoir evaporation station assignment as a delimited list file.");
	//			newComments2.Insert(2,"See also the associated station, account, precipitation station,");
	//		}
	//		else if (componentType == StateMod_DataSet.COMP_RESERVOIR_STATION_PRECIP_STATIONS)
	//		{
	//			newComments2.Insert(1,"StateMod reservoir precipitation station assignment as a delimited list file.");
	//			newComments2.Insert(2,"See also the associated station, account, evaporation station,");
	//		}
	//		newComments2.Insert(3,"content/area/seepage, and collection files.");
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
	//			cli = (StateMod_ReservoirClimate)data[i];

	//			line[0] = StringUtil.formatString(cli.getCgoto(),formats[0]).Trim();
	//			line[1] = StringUtil.formatString(cli.getID(),formats[1]).Trim();
	//			line[2] = StringUtil.formatString(cli.getWeight(),formats[2]).Trim();

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