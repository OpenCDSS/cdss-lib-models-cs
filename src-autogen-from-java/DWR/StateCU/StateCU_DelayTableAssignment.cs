using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

// StateCU_DelayTableAssignment - class to hold StateCU delay table assignment data for StateCU,
// compatible with the StateCU DLA file.

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

namespace DWR.StateCU
{

	using IOUtil = RTi.Util.IO.IOUtil;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// Class to hold StateCU delay table assignment data for StateCU, compatible with the StateCU DLA file.
	/// </summary>
	public class StateCU_DelayTableAssignment : StateCU_Data, StateCU_ComponentValidator
	{

	/// <summary>
	/// Number of stations.
	/// </summary>
	private string[] __delay_table_ids = null;
	private double[] __delay_table_percents = null;

	/// <summary>
	/// Construct a StateCU_DelayTableAssignment instance and set to missing and empty data.
	/// </summary>
	public StateCU_DelayTableAssignment() : base()
	{
	}

	/// <summary>
	/// Creates a backup of the current data object and stores it in _original,
	/// for use in determining if an object was changed inside of a GUI.
	/// </summary>
	public virtual void createBackup()
	{
		_original = clone();
		((StateCU_DelayTableAssignment)_original)._isClone = false;
		_isClone = true;

		int num = ((StateCU_DelayTableAssignment)_original).getNumDelayTables();

		StateCU_DelayTableAssignment dta = (StateCU_DelayTableAssignment)_original;

		if (dta.__delay_table_ids != null)
		{
			__delay_table_ids = new string[num];
			for (int i = 0; i < num; i++)
			{
				__delay_table_ids[i] = dta.getDelayTableID(i);
			}
		}

		if (dta.__delay_table_percents != null)
		{
			__delay_table_percents = new double[num];
			for (int i = 0; i < num; i++)
			{
				__delay_table_percents[i] = dta.getDelayTablePercent(i);
			}
		}
	}

	/// <summary>
	/// Clean up for garbage collection. </summary>
	/// <exception cref="Throwable"> if there is an error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~StateCU_DelayTableAssignment()
	{
		__delay_table_ids = null;
		__delay_table_percents = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Returns the data column header for the specifically checked data. </summary>
	/// <returns> Data column header. </returns>
	public static string[] getDataHeader()
	{
		// TODO KAT 2007-04-12 
		// When specific checks are added to checkComponentData return the header for that data here
		return new string[] {};
	}

	/// <summary>
	/// Return the delay table identifier. </summary>
	/// <returns> the delay table identifier or an empty string if the position is invalid. </returns>
	/// <param name="pos"> Delay table index (relative to zero). </param>
	public virtual string getDelayTableID(int pos)
	{
		if (__delay_table_ids == null)
		{
			return "";
		}
		if ((pos >= 0) && (pos < __delay_table_ids.Length))
		{
			return __delay_table_ids[pos];
		}
		else
		{
			return "";
		}
	}

	/// <summary>
	/// Return the delay table percent. </summary>
	/// <returns> the delay table percent or 0.0 if the position is invalid. </returns>
	/// <param name="pos"> Delay table index (relative to zero). </param>
	public virtual double getDelayTablePercent(int pos)
	{
		if (__delay_table_percents == null)
		{
			return 0.0;
		}
		if ((pos >= 0) && (pos < __delay_table_percents.Length))
		{
			return __delay_table_percents[pos];
		}
		else
		{
			return 0.0;
		}
	}

	/// <summary>
	/// Return the number of delay tables that are used. </summary>
	/// <returns> the number of delay tables that are used. </returns>
	public virtual int getNumDelayTables()
	{
		if (__delay_table_ids == null)
		{
			return 0;
		}
		else
		{
			return __delay_table_ids.Length;
		}
	}

	/// <summary>
	/// Read the StateCU delay table assignment file and return as a list of StateCU_DelayTableAssignment. </summary>
	/// <param name="filename"> filename containing delay table assignment records. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.List<StateCU_DelayTableAssignment> readStateCUFile(String filename) throws java.io.IOException
	public static IList<StateCU_DelayTableAssignment> readStateCUFile(string filename)
	{
		string rtn = "StateCU_DelayTableAssignment.readStateCUFile";
		string iline = null;
		IList<object> v = new List<object> (8);
		IList<StateCU_DelayTableAssignment> data_Vector = new List<StateCU_DelayTableAssignment> (100); // Data to return.
		int i;
		int[] format_0 = new int[] {StringUtil.TYPE_STRING, StringUtil.TYPE_STRING};
		int[] format_0w = new int[] {12, 2};
		// The following used to iteratively read the end of each record.
		int[] format_1 = new int[] {StringUtil.TYPE_STRING, StringUtil.TYPE_STRING};
		int[] format_1w = new int[] {8, 8};

		StateCU_DelayTableAssignment data = null;
		StreamReader @in = null;
		Message.printStatus(1, rtn, "Reading StateCU delay table assignment file: " + filename);

		// The following throws an IOException if the file cannot be opened...
		@in = new StreamReader(filename);
		string num_delay_tables, percent;
		int ndt = 0;
		while (!string.ReferenceEquals((iline = @in.ReadLine()), null))
		{
			// check for comments
			if (iline.StartsWith("#", StringComparison.Ordinal) || iline.Trim().Length == 0)
			{
				continue;
			}

			// Allocate new DelayTableAssignment instance...
			data = new StateCU_DelayTableAssignment();

			StringUtil.fixedRead(iline, format_0, format_0w, v);
			data.setID(((string)v[0]).Trim());
			num_delay_tables = ((string)v[1]).Trim();
			if ((num_delay_tables.Length != 0) && StringUtil.isInteger(num_delay_tables))
			{
				data.setNumDelayTables(StringUtil.atoi(num_delay_tables));
			}
			ndt = data.getNumDelayTables();
			for (i = 0; i < ndt; i++)
			{
				StringUtil.fixedRead(iline.Substring(14 + i * 16), format_1, format_1w, v);
				percent = ((string)v[0]).Trim();
				if ((percent.Length != 0) && StringUtil.isDouble(percent))
				{
					data.setDelayTablePercent(StringUtil.atod(percent), i);
				}
				data.setDelayTableID(((string)v[1]).Trim(), i);
			}

			// Add the StateCU_DelayTableAssignment to the vector...
			data_Vector.Add(data);
		}
		if (@in != null)
		{
			@in.Close();
		}
		return data_Vector;
	}

	/// <summary>
	/// Cancels any changes made to this object within a GUI since createBackup()
	/// was called and sets _original to null.
	/// </summary>
	public override void restoreOriginal()
	{
		StateCU_DelayTableAssignment dta = (StateCU_DelayTableAssignment)_original;
		base.restoreOriginal();

		__delay_table_percents = dta.__delay_table_percents;
		__delay_table_ids = dta.__delay_table_ids;

		_isClone = false;
		_original = null;
	}

	/// <summary>
	/// Set the delay table identifier. </summary>
	/// <param name="id"> Delay table identifier. </param>
	/// <param name="pos"> Delay table index (relative to zero). </param>
	public virtual void setDelayTableID(string id, int pos)
	{
		__delay_table_ids[pos] = id;
	}

	/// <summary>
	/// Set the number of delay tables.  The data arrays are reallocated. </summary>
	/// <param name="num_delay_tables"> Number of delay tables. </param>
	public virtual void setNumDelayTables(int num_delay_tables)
	{
		__delay_table_ids = new string[num_delay_tables];
		__delay_table_percents = new double[num_delay_tables];
	}

	/// <summary>
	/// Set the delay table percentage. </summary>
	/// <param name="percent"> delay table percentage. </param>
	/// <param name="pos"> Station index (relative to zero). </param>
	public virtual void setDelayTablePercent(double percent, int pos)
	{
		__delay_table_percents[pos] = percent;
	}

	/// <summary>
	/// Performs specific data checks and returns a list of data that failed the data checks. </summary>
	/// <param name="dataset"> StateCU dataset currently in memory. </param>
	/// <returns> validation results. </returns>
	public virtual StateCU_ComponentValidation validateComponent(StateCU_DataSet dataset)
	{
		// TODO KAT 2007-04-12 Add specific checks here ...
		return null;
	}

	/// <summary>
	/// Write a list of StateCU_DelayTableAssignment to a file.  The filename is
	/// adjusted to the working directory if necessary using IOUtil.getPathUsingWorkingDir(). </summary>
	/// <param name="filename_prev"> The name of the previous version of the file (for
	/// processing headers).  Specify as null if no previous file is available. </param>
	/// <param name="filename"> The name of the file to write. </param>
	/// <param name="data_Vector"> A Vector of StateCU_DelayTableAssignment to write. </param>
	/// <param name="newComments"> Comments to add to the top of the file.  Specify as null if no comments are available. </param>
	/// <exception cref="IOException"> if there is an error writing the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeStateCUFile(String filename_prev, String filename, java.util.List<StateCU_DelayTableAssignment> data_Vector, java.util.List<String> newComments) throws java.io.IOException
	public static void writeStateCUFile(string filename_prev, string filename, IList<StateCU_DelayTableAssignment> data_Vector, IList<string> newComments)
	{
		IList<string> commentStr = new List<string>(1);
		commentStr.Add("#");
		IList<string> ignoreCommentStr = new List<string>(1);
		ignoreCommentStr.Add("#>");
		PrintWriter @out = null;
		string full_filename_prev = IOUtil.getPathUsingWorkingDir(filename_prev);
		string full_filename = IOUtil.getPathUsingWorkingDir(filename);
		@out = IOUtil.processFileHeaders(full_filename_prev, full_filename, newComments, commentStr, ignoreCommentStr, 0);
		if (@out == null)
		{
			throw new IOException("Error writing to \"" + full_filename + "\"");
		}
		writeVector(data_Vector, @out);
		@out.flush();
		@out.close();
		@out = null;
	}

	/// <summary>
	/// Write a list of StateCU_DelayTableAssignment to an opened file. </summary>
	/// <param name="data_Vector"> A list of StateCU_DelayTableAssignment to write. </param>
	/// <param name="out"> output PrintWriter. </param>
	/// <exception cref="IOException"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static void writeVector(java.util.List<StateCU_DelayTableAssignment> data_Vector, java.io.PrintWriter out) throws java.io.IOException
	private static void writeVector(IList<StateCU_DelayTableAssignment> data_Vector, PrintWriter @out)
	{
		int i, j;
		string cmnt = "#>";
		// Missing data handled by formatting as a string...
		StateCU_DelayTableAssignment data = null;

		@out.println(cmnt);
		@out.println(cmnt + "  StateCU Delay Table Assignment (DLA) File");
		@out.println(cmnt);
		@out.println(cmnt + "  Record format (a12,i2,20(f8.2,i8))");
		@out.println(cmnt);
		@out.println(cmnt + "  ID              :  CU Location identifier");
		@out.println(cmnt + "  ND              :  Number of delay tables");
		@out.println(cmnt + "  Pct             :  Percent of flow that uses the delay table");
		@out.println(cmnt + "  DTID            :  Delay table identifier");
		@out.println(cmnt);
		@out.println(cmnt + "    ID    ND   Pct   DTID");
		@out.println(cmnt + "---------ebeb------eb------eb------eb------eb------e...");
		@out.println(cmnt + "EndHeader");

		int num = 0;
		if (data_Vector != null)
		{
			num = data_Vector.Count;
		}
		int ndt = 0;
		StringBuilder b = new StringBuilder();
		for (i = 0; i < num; i++)
		{
			data = data_Vector[i];
			if (data == null)
			{
				continue;
			}

			b.Length = 0;
			b.Append(StringUtil.formatString(data.getID(),"%-12.12s"));
			ndt = data.getNumDelayTables();
			b.Append(StringUtil.formatString(ndt,"%2d"));
			for (j = 0; j < ndt; j++)
			{
				b.Append(StringUtil.formatString(data.getDelayTablePercent(j), "%8.2f"));
				b.Append(StringUtil.formatString(StringUtil.atoi(data.getDelayTableID(j)),"%8d"));
			}
			@out.println(b.ToString());
		}
	}

	/// <summary>
	/// Writes a list of StateCU_DelayTableAssignment objects to a list file.  A 
	/// header is printed to the top of the file, containing the commands used to 
	/// generate the file.  Any strings in the body of the file that contain the field 
	/// delimiter will be wrapped in "...". </summary>
	/// <param name="filename"> the name of the file to which the data will be written. </param>
	/// <param name="delimiter"> the delimiter to use for separating field values. </param>
	/// <param name="update"> whether to update an existing file, retaining the current 
	/// header (true) or to create a new file with a new header. </param>
	/// <param name="data"> the Vector of objects to write. </param>
	/// <param name="newComments"> comments to add to the top of the file (e.g., command file and HydroBase version). </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeListFile(String filename, String delimiter, boolean update, java.util.List<StateCU_DelayTableAssignment> data, java.util.List<String> newComments) throws Exception
	public static void writeListFile(string filename, string delimiter, bool update, IList<StateCU_DelayTableAssignment> data, IList<string> newComments)
	{
		string routine = "StateCU_DelayTableAssignment.writeListFile";
		int size = 0;
		if (data != null)
		{
			size = data.Count;
		}

		IList<string> fields = new List<string>();
		fields.Add("ID");
		fields.Add("DelayTableID");
		fields.Add("Percent");
		int fieldCount = fields.Count;

		string[] names = new string[fieldCount];
		string[] formats = new string[fieldCount];
		int comp = StateCU_DataSet.COMP_DELAY_TABLE_ASSIGNMENT_MONTHLY;
		string s = null;
		for (int i = 0; i < fieldCount; i++)
		{
			s = (string)fields[i];
			names[i] = StateCU_Util.lookupPropValue(comp, "FieldName", s);
			formats[i] = StateCU_Util.lookupPropValue(comp, "Format", s);
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
		StateCU_DelayTableAssignment dly = null;
		IList<string> commentString = new List<string>(1);
		commentString.Add("#");
		IList<string> ignoreCommentString = new List<string>(1);
		ignoreCommentString.Add("#>");
		string[] line = new string[fieldCount];
		string id = null;
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
			newComments2.Insert(1,"StateCU location delay table assignment information as a delimited list file.");
			newComments2.Insert(2,"See also the associated CU location file.");
			newComments2.Insert(3,"");
			@out = IOUtil.processFileHeaders(oldFile, IOUtil.getPathUsingWorkingDir(filename), newComments, commentString, ignoreCommentString, 0);

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
				dly = (StateCU_DelayTableAssignment)data[i];

				num = dly.getNumDelayTables();
				id = dly.getID();

				for (j = 0; j < num; j++)
				{
					line[0] = StringUtil.formatString(id,formats[0]).Trim();
					line[1] = StringUtil.formatString(dly.getDelayTableID(j),formats[1]).Trim();
					line[2] = StringUtil.formatString(dly.getDelayTablePercent(j),formats[2]).Trim();

					buffer = new StringBuilder();
					for (k = 0; k < fieldCount; k++)
					{
						if (line[k].IndexOf(delimiter, StringComparison.Ordinal) > -1)
						{
							line[k] = "\"" + line[k] + "\"";
						}
						if (k > 0)
						{
							buffer.Append(delimiter);
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