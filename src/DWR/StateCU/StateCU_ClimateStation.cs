using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

// StateCU_ClimateStation.java - class for the StateCU climate station objects, CLI file

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

// ----------------------------------------------------------------------------
// StateCU_ClimateStation.java - class for the StateCU climate station objects,
//				CLI file
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
// 2003-05-07	Steven A. Malers, RTi	Initial version copied from
//					CUClimateStationWeights.
// 2003-06-04	SAM, RTi		Rename class from CUClimateStation to
//					StateCU_ClimateStation.
// 2005-01-17	J. Thomas Sapienza, RTi	* Added createBackup().
//					* Added restoreOriginal().
// 2005-04-18	JTS, RTi		Added writeListFile().
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace DWR.StateCU
{

	using IOUtil = RTi.Util.IO.IOUtil;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// The StateCU_ClimateStation class holds data from one object (data line) in a
	/// StateCU climate station (CLI) file.  The method names do not exactly
	/// correspond to StateCU file variables because the content is general enough to
	/// describe with more general conventions.  The unique identifier for an object
	/// is the climate station identifier.
	/// </summary>
	public class StateCU_ClimateStation : StateCU_Data, StateCU_ComponentValidator
	{

	// List data in the same order as in the StateCU documentation.

	// StationID is stored in the base class ID.
	// Station name is stored in the base class name.

	private double __latitude = StateCU_Util.MISSING_DOUBLE;
	private double __elevation = StateCU_Util.MISSING_DOUBLE;

	private string __region1 = StateCU_Util.MISSING_STRING;
	private string __region2 = StateCU_Util.MISSING_STRING;

	private double __zh = StateCU_Util.MISSING_DOUBLE;
	private double __zm = StateCU_Util.MISSING_DOUBLE;

	/// <summary>
	/// Construct a StateCU_ClimateStation instance and set to missing and empty data.
	/// </summary>
	public StateCU_ClimateStation() : base()
	{
	}

	/// <summary>
	/// Creates a backup of the current data object and stores it in _original,
	/// for use in determining if an object was changed inside of a GUI.
	/// </summary>
	public virtual void createBackup()
	{
		_original = clone();
		((StateCU_ClimateStation)_original)._isClone = false;
		_isClone = true;
	}

	/// <summary>
	/// Clean up for garbage collection. </summary>
	/// <exception cref="Throwable"> if there is an error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~StateCU_ClimateStation()
	{
		__region1 = null;
		__region2 = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Returns the data column header for the specifically checked data. </summary>
	/// <returns> Data column header. </returns>
	public static string[] getDataHeader()
	{
		// TODO KAT 2007-04-12 
		// When specific checks are added to checkComponentData
		// return the header for that data here
		return new string[] {};
	}

	/// <summary>
	/// Return the station elevation, feet. </summary>
	/// <returns> the station elevation, feet. </returns>
	public virtual double getElevation()
	{
		return __elevation;
	}

	/// <summary>
	/// Return the station latitude, decimal degrees. </summary>
	/// <returns> the station latitude, decimal degrees. </returns>
	public virtual double getLatitude()
	{
		return __latitude;
	}

	/// <summary>
	/// Return Region 1. </summary>
	/// <returns> Region 1. </returns>
	public virtual string getRegion1()
	{
		return __region1;
	}

	/// <summary>
	/// Return Region 2. </summary>
	/// <returns> Region 2. </returns>
	public virtual string getRegion2()
	{
		return __region2;
	}

	/// <summary>
	/// Return the height of humidity and temperature measurements, feet. </summary>
	/// <returns> the height of humidity and temperature measurements, feet. </returns>
	public virtual double getZh()
	{
		return __zh;
	}

	/// <summary>
	/// Return the height of wind speed measurements, feet. </summary>
	/// <returns> the height of wind speed measurements, feet. </returns>
	public virtual double getZm()
	{
		return __zm;
	}

	/// <summary>
	/// Read the StateCU climate stations file and return as a list of CUClimateStation. </summary>
	/// <param name="filename"> filename containing CLI records. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.List<StateCU_ClimateStation> readStateCUFile(String filename) throws java.io.IOException
	public static IList<StateCU_ClimateStation> readStateCUFile(string filename)
	{
		string rtn = "StateCU_ClimateStation.readStateCUFile";
		string iline = null;
		IList<object> v = new List<object> (10);
		IList<StateCU_ClimateStation> sta_Vector = new List<StateCU_ClimateStation> (100); // Data to return.
		int[] format_0 = new int[] {StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_SPACE, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_SPACE, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING};

		int[] format_0w = new int[] {12, 6, 9, 2, 20, 8, 2, 24, 8, 8};
		StateCU_ClimateStation sta = null;
		StreamReader @in = null;

		try
		{
			Message.printStatus(2, rtn, "Reading StateCU climate station file: \"" + filename + "\"");
			// The following throws an IOException if the file cannot be opened...
			@in = new StreamReader(filename);
			string @string;
			while (!string.ReferenceEquals((iline = @in.ReadLine()), null))
			{
				// check for comments
				if (iline.StartsWith("#", StringComparison.Ordinal) || iline.Trim().Length == 0)
				{
					continue;
				}

				// allocate new StateCU_ClimateStation instance...
				sta = new StateCU_ClimateStation();

				StringUtil.fixedRead(iline, format_0, format_0w, v);
				sta.setID(((string)v[0]).Trim());
				@string = ((string)v[1]).Trim();
				if ((@string.Length != 0) && StringUtil.isDouble(@string))
				{
					sta.setLatitude(StringUtil.atod(@string));
				}
				@string = ((string)v[2]).Trim();
				if ((@string.Length != 0) && StringUtil.isDouble(@string))
				{
					sta.setElevation(StringUtil.atod(@string));
				}
				sta.setRegion1(((string)v[3]).Trim());
				sta.setRegion2(((string)v[4]).Trim());
				sta.setName(((string)v[5]).Trim());
				@string = ((string)v[6]).Trim();
				if ((@string.Length != 0) && StringUtil.isDouble(@string))
				{
					sta.setZh(double.Parse(@string));
				}
				@string = ((string)v[7]).Trim();
				if ((@string.Length != 0) && StringUtil.isDouble(@string))
				{
					sta.setZm(double.Parse(@string));
				}

				// add the StateCU_ClimateStation to the list...
				sta_Vector.Add(sta);
			}
		}
		finally
		{
			if (@in != null)
			{
				@in.Close();
			}
		}
		return sta_Vector;
	}

	/// <summary>
	/// Cancels any changes made to this object within a GUI since createBackup()
	/// was called and sets _original to null.
	/// </summary>
	public override void restoreOriginal()
	{
		StateCU_ClimateStation station = (StateCU_ClimateStation)_original;
		base.restoreOriginal();

		__latitude = station.__latitude;
		__elevation = station.__elevation;
		__region1 = station.__region1;
		__region2 = station.__region2;
		__zh = station.__zh;
		__zm = station.__zm;

		_isClone = false;
		_original = null;
	}

	/// <summary>
	/// Set the elevation, feet. </summary>
	/// <param name="elevation"> The elevation, feet. </param>
	public virtual void setElevation(double elevation)
	{
		__elevation = elevation;
	}

	/// <summary>
	/// Set the latitude, decimal degrees. </summary>
	/// <param name="latitude"> The latitude, decimal degrees. </param>
	public virtual void setLatitude(double latitude)
	{
		__latitude = latitude;
	}

	/// <summary>
	/// Set Region 1. </summary>
	/// <param name="region1"> Region 1. </param>
	public virtual void setRegion1(string region1)
	{
		__region1 = region1;
	}

	/// <summary>
	/// Set Region 2. </summary>
	/// <param name="region2"> Region 2. </param>
	public virtual void setRegion2(string region2)
	{
		__region2 = region2;
	}

	/// <summary>
	/// Set the height of humidity and temperature measurements. </summary>
	/// <param name="zh"> height of humidity and temperature measurements, feet </param>
	public virtual void setZh(double zh)
	{
		__zh = zh;
	}

	/// <summary>
	/// Set the height of wind speed measurement. </summary>
	/// <param name="zm"> height of wind speed measurement, feet </param>
	public virtual void setZm(double zm)
	{
		__zm = zm;
	}

	/// <summary>
	/// Performs specific data checks and returns a list of data that failed the data checks. </summary>
	/// <param name="dataset"> StateCU dataset currently in memory. </param>
	/// <returns> Validation results. </returns>
	public virtual StateCU_ComponentValidation validateComponent(StateCU_DataSet dataset)
	{
		StateCU_ComponentValidation validation = new StateCU_ComponentValidation();
		string id = getID();
		double latitude = getLatitude();
		if (!((latitude >= -90.0) && (latitude <= 90.0)))
		{
			validation.add(new StateCU_ComponentValidationProblem(this,"Climate station \"" + id + "\" latitude (" + latitude + ") is invalid.", "Specify a latitude -90 to 90."));
		}
		double elevation = getElevation();
		if (!((elevation >= 0.0) && (elevation <= 15000.00)))
		{
			validation.add(new StateCU_ComponentValidationProblem(this,"Climate station \"" + id + "\" elevation (" + elevation + ") is invalid.", "Specify an elevation 0 to 15000 FT (maximum varies by location)."));
		}
		string name = getName();
		if ((string.ReferenceEquals(name, null)) || name.Trim().Length == 0)
		{
			validation.add(new StateCU_ComponentValidationProblem(this,"Climate station \"" + id + "\" name is blank - may cause confusion.", "Specify the station name or use the ID for the name."));
		}
		string region1 = getRegion1();
		if ((string.ReferenceEquals(region1, null)) || region1.Trim().Length == 0)
		{
			validation.add(new StateCU_ComponentValidationProblem(this,"Climate station \"" + id + "\" region1 is blank - may cause region lookups to fail for other data.", "Specify as county or other region indicator."));
		}
		double zh = getZh();
		if (!(zh >= 0.0))
		{
			validation.add(new StateCU_ComponentValidationProblem(this,"Climate station \"" + id + "\" zh (" + zh + ") is invalid.", "Specify a zh >= 0."));
		}
		double zm = getZm();
		if (!(zm >= 0.0))
		{
			validation.add(new StateCU_ComponentValidationProblem(this,"Climate station \"" + id + "\" zm (" + zm + ") is invalid.", "Specify a zm >= 0."));
		}
		return validation;
	}

	/// <summary>
	/// Write a list of StateCU_ClimateStation to a file.  The filename is adjusted
	/// to the working directory if necessary using IOUtil.getPathUsingWorkingDir(). </summary>
	/// <param name="filenamePrev"> The name of the previous version of the file (for processing headers).
	/// Specify as null if no previous file is available. </param>
	/// <param name="filename"> The name of the file to write. </param>
	/// <param name="dataList"> A list of StateCU_ClimateStation to write. </param>
	/// <param name="newComments"> Comments to add to the top of the file.  Specify as null if no comments are available. </param>
	/// <exception cref="IOException"> if there is an error writing the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeStateCUFile(String filenamePrev, String filename, java.util.List<StateCU_ClimateStation> dataList, java.util.List<String> newComments) throws java.io.IOException
	public static void writeStateCUFile(string filenamePrev, string filename, IList<StateCU_ClimateStation> dataList, IList<string> newComments)
	{
		IList<string> comment_str = new List<string>(1);
		comment_str.Add("#");
		IList<string> ignore_comment_str = new List<string>(1);
		ignore_comment_str.Add("#>");
		PrintWriter @out = null;
		try
		{
			string full_filename_prev = IOUtil.getPathUsingWorkingDir(filenamePrev);
			string full_filename = IOUtil.getPathUsingWorkingDir(filename);
			@out = IOUtil.processFileHeaders(full_filename_prev, full_filename, newComments, comment_str, ignore_comment_str, 0);
			if (@out == null)
			{
				throw new IOException("Error writing to \"" + full_filename + "\"");
			}
			writeStateCUFile(dataList, @out);
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
	/// Write a list of StateCU_ClimateStation to an opened file. </summary>
	/// <param name="dataList"> A list of StateCU_ClimateStation to write. </param>
	/// <param name="out"> output PrintWriter. </param>
	/// <exception cref="IOException"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static void writeStateCUFile(java.util.List<StateCU_ClimateStation> dataList, java.io.PrintWriter out) throws java.io.IOException
	private static void writeStateCUFile(IList<StateCU_ClimateStation> dataList, PrintWriter @out)
	{
		int i;
		string iline;
		string cmnt = "#>";
		// Missing data handled by formatting all as strings...
		string format = "%-12.12s%6.6s%9.9s  %-20.20s%-8.8s  %-24.24s%8.8s%8.8s";
		IList<object> v = new List<object>(8); // Reuse for all output lines.

		@out.println(cmnt);
		@out.println(cmnt + "  StateCU Climate Stations File");
		@out.println(cmnt);
		@out.println(cmnt + "  Record format (a12,f6.2,f9.2,2x,a20,a8,2x,a24,8.2f,2.2f)");
		@out.println(cmnt);
		@out.println(cmnt + "  StationID:  Station identifier (e.g., 3951)");
		@out.println(cmnt + "        Lat:  Latitude (decimal degrees)");
		@out.println(cmnt + "       Elev:  Elevation (feet)");
		@out.println(cmnt + "    Region1:  Region1 (e.g., County)");
		@out.println(cmnt + "    Region2:  Region2 (e.g., Hydrologic Unit Code, HUC)");
		@out.println(cmnt + "StationName:  Station name");
		@out.println(cmnt + "     zHumid:  Height of humidity and temperature measurements (feet, daily analysis only)");
		@out.println(cmnt + "      zWind:  Height of wind speed measurement (feet, daily analysis only)");
		@out.println(cmnt);
		@out.println(cmnt + " StationID  Lat   Elev            Region1      Region2        StationName         zHumid  zWind");
		@out.println(cmnt + "---------eb----eb-------exxb------------------eb------exxb----------------------eb------eb------e");
		@out.println(cmnt + "EndHeader");

		int num = 0;
		if (dataList != null)
		{
			num = dataList.Count;
		}
		StateCU_ClimateStation sta = null;
		for (i = 0; i < num; i++)
		{
			sta = dataList[i];
			if (sta == null)
			{
				continue;
			}

			v.Clear();
			v.Add(sta._id);
			// Latitude...
			if (StateCU_Util.isMissing(sta.__latitude))
			{
				v.Add("");
			}
			else
			{
				v.Add(StringUtil.formatString(sta.__latitude,"%6.2f"));
			}
			// Elevation...
			if (StateCU_Util.isMissing(sta.__elevation))
			{
				v.Add("");
			}
			else
			{
				v.Add(StringUtil.formatString(sta.__elevation,"%9.2f"));
			}
			v.Add(sta.__region1);
			v.Add(sta.__region2);
			v.Add(sta._name);
			// zh...
			if (StateCU_Util.isMissing(sta.__zh))
			{
				v.Add("");
			}
			else
			{
				v.Add(StringUtil.formatString(sta.__zh,"%8.2f"));
			}
			// zm...
			if (StateCU_Util.isMissing(sta.__zm))
			{
				v.Add("");
			}
			else
			{
				v.Add(StringUtil.formatString(sta.__zm,"%8.2f"));
			}

			iline = StringUtil.formatString(v, format);
			@out.println(iline);
		}
	}

	/// <summary>
	/// Writes a list of StateCU_ClimateStation objects to a list file.  A header is 
	/// printed to the top of the file, containing the commands used to generate the 
	/// file.  Any strings in the body of the file that contain the field delimiter will be wrapped in "...". </summary>
	/// <param name="filename"> the name of the file to which the data will be written. </param>
	/// <param name="delimiter"> the delimiter to use for separating field values. </param>
	/// <param name="update"> whether to update an existing file, retaining the current </param>
	/// header (true) or to create a new file with a new header<param name="data"> the Vector of objects to write. </param>
	/// <param name="outputComments"> Comments to add to the header, usually the command file and database information. </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeListFile(String filename, String delimiter, boolean update, java.util.List<StateCU_ClimateStation> data, java.util.List<String> outputComments) throws Exception
	public static void writeListFile(string filename, string delimiter, bool update, IList<StateCU_ClimateStation> data, IList<string> outputComments)
	{
		int size = 0;
		if (data != null)
		{
			size = data.Count;
		}

		IList<string> fields = new List<string>();
		fields.Add("ID");
		fields.Add("Name");
		fields.Add("Latitude");
		fields.Add("Elevation");
		fields.Add("Region1");
		fields.Add("Region2");
		fields.Add("HeightHumidity");
		fields.Add("HeightWind");
		int fieldCount = fields.Count;

		string[] names = new string[fieldCount];
		string[] formats = new string[fieldCount];
		int comp = StateCU_DataSet.COMP_CLIMATE_STATIONS;
		string s = null;
		for (int i = 0; i < fieldCount; i++)
		{
			s = fields[i];
			names[i] = StateCU_Util.lookupPropValue(comp, "FieldName", s);
			formats[i] = StateCU_Util.lookupPropValue(comp, "Format", s);
		}

		string oldFile = null;
		if (update)
		{
			oldFile = IOUtil.getPathUsingWorkingDir(filename);
		}

		int j = 0;
		PrintWriter @out = null;
		StateCU_ClimateStation cli = null;
		IList<string> commentString = new List<string>(1);
		commentString.Add("#");
		IList<string> ignoreCommentString = new List<string>(1);
		ignoreCommentString.Add("#>");
		string[] line = new string[fieldCount];
		StringBuilder buffer = new StringBuilder();

		try
		{
			// Add some basic comments at the top of the file.  However, do this to a copy of the
			// incoming comments so that they are not modified in the calling code.
			IList<string> newComments2 = null;
			if (outputComments == null)
			{
				newComments2 = new List<string>();
			}
			else
			{
				newComments2 = new List<string>(outputComments);
			}
			newComments2.Insert(0,"");
			newComments2.Insert(1,"StateCU climate stations as a delimited list file.");
			newComments2.Insert(2,"");
			@out = IOUtil.processFileHeaders(oldFile, IOUtil.getPathUsingWorkingDir(filename), newComments2, commentString, ignoreCommentString, 0);

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
				cli = data[i];

				line[0] = StringUtil.formatString(cli.getID(), formats[0]).Trim();
				line[1] = StringUtil.formatString(cli.getName(), formats[1]).Trim();
				line[2] = StringUtil.formatString(cli.getLatitude(), formats[2]).Trim();
				line[3] = StringUtil.formatString(cli.getElevation(), formats[3]).Trim();
				line[4] = StringUtil.formatString(cli.getRegion1(), formats[4]).Trim();
				line[5] = StringUtil.formatString(cli.getRegion2(), formats[5]).Trim();
				line[6] = StringUtil.formatString(cli.getZh(), formats[6]).Trim();
				line[7] = StringUtil.formatString(cli.getZm(), formats[7]).Trim();

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

	}

}