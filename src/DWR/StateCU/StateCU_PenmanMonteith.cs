using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

// StateCU_PenmanMonteith - class to hold StateCU Penman-Monteith crop data for StateCU/StateDMI, compatible

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
	/// Class to hold StateCU Penman-Monteith crop data for StateCU/StateDMI, compatible
	/// with the StateCU KPM file.
	/// </summary>
	public class StateCU_PenmanMonteith : StateCU_Data, StateCU_ComponentValidator
	{

	// List data in the same order as in the StateCU documentation...

	// Cropn (Crop name) is stored in the base class name.
	// id (crop number) is stored in the base class ID, if necessary (currently not used).

	/// <summary>
	/// Number of growth stages (number of 0, 10, ..., 90, 100 percent sequences in coefficients).
	/// This is implied during read from the crop name.
	/// </summary>
	private int __nGrowthStages = StateCU_Util.MISSING_INT;

	/// <summary>
	/// Time from start of growth to effective cover (%) or % of growth stage after effective cover.
	/// </summary>
	private double[][] __kcday = null;

	/// <summary>
	/// Crop coefficient for each stage.
	/// </summary>
	private double[][] __kcb = null;

	/// <summary>
	/// Construct a StateCU_PenmanMonteith instance and set to missing and empty data. </summary>
	/// <param name="nGrowthStage"> the number of growth stages. </param>
	public StateCU_PenmanMonteith(int nGrowthStage) : base()
	{
		__nGrowthStages = nGrowthStage;
		int ncpgs = getNCoefficientsPerGrowthStage();
		// Allocate the arrays based on the number of growth stages
		__kcday = new double[nGrowthStage][];
		__kcb = new double[nGrowthStage][];
		for (int igs = 0; igs < nGrowthStage; igs++)
		{
			__kcday[igs] = new double[ncpgs];
			__kcb[igs] = new double[ncpgs];
			// Default these to simplify setting in DMI and other code...
			for (int i = 0; i < ncpgs; i++)
			{
				__kcday[igs][i] = i * 100.0 / (ncpgs - 1);
				__kcb[igs][i] = StateCU_Util.MISSING_DOUBLE;
			}
		}
	}

	/// <summary>
	/// Creates a backup of the current data object and stores it in _original,
	/// for use in determining if an object was changed inside of a GUI.
	/// </summary>
	public virtual void createBackup()
	{
		_original = clone();
		((StateCU_PenmanMonteith)_original)._isClone = false;
		_isClone = true;

		StateCU_PenmanMonteith pm = (StateCU_PenmanMonteith)_original;

		__nGrowthStages = pm.__nGrowthStages;
		int ncpgs = getNCoefficientsPerGrowthStage();
		__kcday = new double[__nGrowthStages][];
		__kcb = new double[__nGrowthStages][];
		for (int igs = 0; igs < __nGrowthStages; igs++)
		{
			__kcday[igs] = new double[ncpgs];
			__kcb[igs] = new double[ncpgs];
			// Default these to simplify setting in DMI and other code...
			for (int i = 0; i < ncpgs; i++)
			{
				__kcday[igs][i] = pm.__kcday[igs][i];
				__kcb[igs][i] = pm.__kcb[igs][i];
			}
		}
	}

	/// <summary>
	/// Clean up for garbage collection. </summary>
	/// <exception cref="Throwable"> if there is an error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~StateCU_PenmanMonteith()
	{
		__kcb = null;
		__kcday = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Return the crop coefficients. </summary>
	/// <returns> the crop coefficients. </returns>
	public virtual double [][] getKcb()
	{
		return __kcb;
	}

	/// <summary>
	/// Return the crop coefficient for the growth stage and index. </summary>
	/// <param name="igs"> growth stage (0+ index) </param>
	/// <param name="index"> index in growth stage (0+) </param>
	/// <returns> the crop coefficient for the growth stage and index. </returns>
	public virtual double getKcb(int igs, int index)
	{
		return __kcb[igs][index];
	}

	/// <summary>
	/// Return the crop coefficients time percent. </summary>
	/// <returns> the crop coefficients time percent. </returns>
	public virtual double [][] getKcday()
	{
		return __kcday;
	}

	/// <summary>
	/// Return the crop coefficient time percent for the growth stage and index. </summary>
	/// <param name="igs"> growth stage (0+ index) </param>
	/// <param name="index"> index in growth stage (0+) </param>
	/// <returns> the crop coefficient time percent for the growth stage and index. </returns>
	public virtual double getKcday(int igs, int index)
	{
		return __kcday[igs][index];
	}

	/// <summary>
	/// Returns the data column header for the specifically checked data. </summary>
	/// <returns> Data column header. </returns>
	public static string[] getDataHeader()
	{
		// TODO KAT 2007-04-12 When specific checks are added to checkComponentData
		// return the header for that data here
		return new string[] {};
	}

	/// <summary>
	/// Return the number of coefficients per growth stages - currently always 11. </summary>
	/// <returns> the number of coefficients per growth stages. </returns>
	public static int getNCoefficientsPerGrowthStage()
	{
		return 11;
	}

	/// <summary>
	/// Return the number of growth stages. </summary>
	/// <returns> the number of growth stages. </returns>
	public virtual int getNGrowthStages()
	{
		return __nGrowthStages;
	}

	/// <summary>
	/// Return the number of growth stages (3 if the crop name contains ALFALFA, 1 if it
	/// contains GRASS and PASTURE, and 2 otherwise). </summary>
	/// <param name="cropName"> the crop name. </param>
	/// <returns> the number of growth stages for the crop. </returns>
	public static int getNGrowthStagesFromCropName(string cropName)
	{
		cropName = cropName.ToUpper();
		if (cropName.IndexOf("ALFALFA", StringComparison.Ordinal) >= 0)
		{
			return 3;
		}
		else if ((cropName.IndexOf("GRASS", StringComparison.Ordinal) >= 0) && (cropName.IndexOf("PASTURE", StringComparison.Ordinal) >= 0))
		{
			return 1;
		}
		else
		{
			return 2;
		}
	}

	/// <summary>
	/// Read the StateCU KPM file and return as a list of StateCU_PenmanMonteith. </summary>
	/// <param name="filename"> filename containing KPM records. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.List<StateCU_PenmanMonteith> readStateCUFile(String filename) throws java.io.IOException
	public static IList<StateCU_PenmanMonteith> readStateCUFile(string filename)
	{
		string rtn = "StateCU_PenmanMonteith.readStateCUFile";
		string iline = null;
		StateCU_PenmanMonteith kpm = null;
		IList<StateCU_PenmanMonteith> kpmList = new List<StateCU_PenmanMonteith>(25);
		StreamReader @in = null;
		int lineNum = 0;

		Message.printStatus(1, rtn, "Reading StateCU KPM file: " + filename);
		try
		{
			// The following throws an IOException if the file cannot be opened...
			@in = new StreamReader(filename);
			int nc = -1;
			string title = null; // The title is currently read but not stored since it is never really used for anything.
			while (!string.ReferenceEquals((iline = @in.ReadLine()), null))
			{
				++lineNum;
				// check for comments
				if (iline.StartsWith("#", StringComparison.Ordinal) || iline.Trim().Length == 0)
				{
					continue;
				}
				if (string.ReferenceEquals(title, null))
				{
					title = iline;
				}
				else if (nc < 0)
				{
					// Assume that the line contains the number of crops
					nc = int.Parse(iline.Trim());
					break;
				}
			}

			// Now loop through the number of curves...

			// TODO SAM 2010-03-30 Evaluate if needed
			//String id;
			string cropn;
			IList<string> tokens;
			int j = 0;
			// Read the number of curves (crops)
			for (int i = 0; i < nc; i++)
			{
				// Read a free format line...
				iline = @in.ReadLine();
				++lineNum;

				tokens = StringUtil.breakStringList(iline.Trim(), " \t", StringUtil.DELIM_SKIP_BLANKS);
				// TODO SAM 2007-02-18 Evaluate if needed
				//id = tokens.elementAt(0);
				cropn = tokens[1];

				// Allocate new StateCU_PenmanMonteith instance...

				int ngs = getNGrowthStagesFromCropName(cropn);
				kpm = new StateCU_PenmanMonteith(ngs);
				// Number of coefficients per growth stage...
				int ncpgs = StateCU_PenmanMonteith.getNCoefficientsPerGrowthStage();
				kpm.setName(cropn);
				// TODO SAM 2005-05-22 Ignore the old ID and use the crop name - this facilitates
				// sorting and other standard StateCU_Data features.
				//kbc.setID ( id );
				kpm.setID(cropn);

				// Read the coefficients...
				for (int igs = 0; igs < ngs; igs++)
				{
					for (j = 0; j < ncpgs; j++)
					{
						iline = @in.ReadLine();
						++lineNum;
						tokens = StringUtil.breakStringList(iline.Trim(), " \t", StringUtil.DELIM_SKIP_BLANKS);
						kpm.setCurvePosition(igs,j,double.Parse(tokens[0]));
						kpm.setCurveValue(igs,j,double.Parse(tokens[1]));
					}
				}

				// add the StateCU_PenmanMonteith to the list...
				kpmList.Add(kpm);
			}
		}
		catch (Exception e)
		{
			Message.printWarning(3, rtn, "Error reading file (" + e + ").");
			Message.printWarning(3, rtn, e);
			throw new IOException("Error reading file \"" + filename + "\" near line " + lineNum);
		}
		finally
		{
			if (@in != null)
			{
				@in.Close();
			}
		}
		return kpmList;
	}

	/// <summary>
	/// Cancels any changes made to this object within a GUI since createBackup()
	/// was called and sets _original to null.
	/// </summary>
	public override void restoreOriginal()
	{
		StateCU_PenmanMonteith bc = (StateCU_PenmanMonteith)_original;
		base.restoreOriginal();

		__kcb = bc.__kcb;
		__kcday = bc.__kcday;
		__nGrowthStages = bc.__nGrowthStages;
		_isClone = false;
		_original = null;
	}

	/// <summary>
	/// Set the curve value for the crop coefficient curve (coefficient only). </summary>
	/// <param name="igs"> growth stage index (zero-index). </param>
	/// <param name="i"> Index in the curve (zero-index).  For example 1 corresponds to 10%. </param>
	public virtual void setCurveValue(int igs, int i, double coeff)
	{
		__kcb[igs][i] = coeff;
	}

	/// <summary>
	/// Set the values for the crop coefficient curve (curve position and coefficient). </summary>
	/// <param name="igs"> growth stage index (zero-index). </param>
	/// <param name="i"> Index in the curve (zero-index).  For example 0 corresponds to 0% and 1 to 10%. </param>
	/// <param name="pos"> Position in the curve (percent). </param>
	/// <param name="coeff"> Value at the position. </param>
	public virtual void setCurveValues(int igs, int i, double pos, double coeff)
	{
		__kcday[igs][i] = pos;
		__kcb[igs][i] = coeff;
	}

	/// <summary>
	/// Sets the percent for a curve value. </summary>
	/// <param name="i"> the index in the curve (zero-index).
	/// For example 1 corresponds to 5% for a percent curve or day 15 for a day curve. </param>
	/// <param name="pos"> the new percent to change the index position to. </param>
	public virtual void setCurvePosition(int igs, int i, double pos)
	{
		__kcday[igs][i] = pos;
	}

	// TODO SAM 2009-05-08 Evaluate whether to allow passing in max coefficient value for check
	/// <summary>
	/// Performs specific data checks and returns a list of data that failed the data checks. </summary>
	/// <param name="count"> Index of the data vector currently being checked. </param>
	/// <param name="dataset"> StateCU dataset currently in memory. </param>
	/// <param name="props"> Extra properties to perform checks with. </param>
	/// <returns> List of invalid data. </returns>
	public virtual StateCU_ComponentValidation validateComponent(StateCU_DataSet dataset)
	{
		StateCU_ComponentValidation validation = new StateCU_ComponentValidation();
		string id = getName(); // Name is used for ID because ID used to be numeric
		double[][] kcday = getKcday();
		double[][] kcb = getKcb();
		// Percent of growth stage
		for (int igs = 0; igs < kcday.Length; igs++)
		{
			for (int ipos = 0; ipos < kcday[igs].Length; ipos++)
			{
				if ((kcday[igs][ipos] < 0.0) || (kcday[igs][ipos] > 100.0))
				{
					validation.add(new StateCU_ComponentValidationProblem(this,"Crop \"" + id + "\" percent of growth stage (" + kcday[igs][ipos] + ") is invalid.", "Specify as 0 to 100."));
				}
				if ((kcb[igs][ipos] < 0) || (kcb[igs][ipos] > 3.0))
				{
					validation.add(new StateCU_ComponentValidationProblem(this,"Crop \"" + id + "\" coefficient (" + kcb[igs][ipos] + ") is invalid.", "Specify as 0 to 3.0 (upper limit may vary by location)."));
				}
			}
		}

		return validation;
	}

	/// <summary>
	/// Write a list of StateCU_PenmanMonteith to a file.  The filename is adjusted to
	/// the working directory if necessary using IOUtil.getPathUsingWorkingDir(). </summary>
	/// <param name="filename_prev"> The name of the previous version of the file (for
	/// processing headers).  Specify as null if no previous file is available. </param>
	/// <param name="filename"> The name of the file to write. </param>
	/// <param name="data_Vector"> A list of StateCU_PenmanMonteith to write. </param>
	/// <param name="new_comments"> Comments to add to the top of the file.  Specify as null 
	/// if no comments are available. </param>
	/// <exception cref="IOException"> if there is an error writing the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeStateCUFile(String filename_prev, String filename, java.util.List<StateCU_PenmanMonteith> data_Vector, java.util.List<String> new_comments) throws java.io.IOException
	public static void writeStateCUFile(string filename_prev, string filename, IList<StateCU_PenmanMonteith> data_Vector, IList<string> new_comments)
	{
		writeStateCUFile(filename_prev, filename, data_Vector, new_comments, null);
	}

	/// <summary>
	/// Write a list of StateCU_PenmanMonteith to a file.  The filename is adjusted to
	/// the working directory if necessary using IOUtil.getPathUsingWorkingDir(). </summary>
	/// <param name="filename_prev"> The name of the previous version of the file (for
	/// processing headers).  Specify as null if no previous file is available. </param>
	/// <param name="filename"> The name of the file to write. </param>
	/// <param name="data_Vector"> A list of StateCU_PenmanMonteith to write. </param>
	/// <param name="new_comments"> Comments to add to the top of the file.  Specify as null 
	/// if no comments are available. </param>
	/// <param name="props"> Properties to control the output.  Currently only the
	/// optional Precision property can be set, indicating how many digits after the
	/// decimal should be printed (default is 3). </param>
	/// <exception cref="IOException"> if there is an error writing the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeStateCUFile(String filename_prev, String filename, java.util.List<StateCU_PenmanMonteith> data_Vector, java.util.List<String> new_comments, System.Nullable<int> precision) throws java.io.IOException
	public static void writeStateCUFile(string filename_prev, string filename, IList<StateCU_PenmanMonteith> data_Vector, IList<string> new_comments, int? precision)
	{
		IList<string> comment_str = new List<string>(1);
		comment_str.Add("#");
		IList<string> ignore_comment_str = new List<string>(1);
		ignore_comment_str.Add("#>");
		PrintWriter @out = null;
		string full_filename_prev = IOUtil.getPathUsingWorkingDir(filename_prev);
		string full_filename = IOUtil.getPathUsingWorkingDir(filename);
		@out = IOUtil.processFileHeaders(full_filename_prev, full_filename, new_comments, comment_str, ignore_comment_str, 0);
		if (@out == null)
		{
			throw new IOException("Error writing to \"" + full_filename + "\"");
		}
		writeVector(data_Vector, @out, precision);
		@out.flush();
		@out.close();
		@out = null;
	}

	/// <summary>
	/// Write a list of StateCU_PenmanMonteith to an opened file. </summary>
	/// <param name="data_Vector"> A list of StateCU_PenmanMonteith to write. </param>
	/// <param name="out"> output PrintWriter. </param>
	/// <param name="props"> Properties to control the output.  Currently only the
	/// optional Precision property can be set, indicating how many digits after the
	/// decimal should be printed (default is 3). </param>
	/// <exception cref="IOException"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static void writeVector(java.util.List<StateCU_PenmanMonteith> data_Vector, java.io.PrintWriter out, System.Nullable<int> precision) throws java.io.IOException
	private static void writeVector(IList<StateCU_PenmanMonteith> data_Vector, PrintWriter @out, int? precision)
	{
		string cmnt = "#>";
		// Missing data are handled by formatting all as strings (blank if necessary).
		if (precision == null)
		{
			precision = new int?(3); // Make this agree with the Blaney-Criddle default
		}
		int precision2 = precision.Value;

		@out.println(cmnt);
		@out.println(cmnt + " StateCU Penman-Monteith Crop Coefficient (KPM) File");
		@out.println(cmnt);
		@out.println(cmnt + " Record 1 format (a80)");
		@out.println(cmnt);
		@out.println(cmnt + "  Title     remark:  Title");
		@out.println(cmnt);
		@out.println(cmnt + " Record 2 format (free format)");
		@out.println(cmnt);
		@out.println(cmnt + "  NumCurves     nc:  Number of crop coefficient curves");
		@out.println(cmnt);
		@out.println(cmnt + " Record 3 format (free format)");
		@out.println(cmnt);
		@out.println(cmnt + "  ID            id:  Crop number (not used by StateCU)");
		@out.println(cmnt + "  CropName   cropn:  Crop name (e.g., ALFALFA)");
		@out.println(cmnt);
		@out.println(cmnt + " Record 4 format (free format)");
		@out.println(cmnt);
		@out.println(cmnt + "  Percent    kcday:  Time from start of growth to effective cover (%)");
		@out.println(cmnt + "                     or % of next growth stage.");
		@out.println(cmnt + "  Coeff        kcb:  Crop coefficient for alfalfa-based ET");
		@out.println(cmnt);
		@out.println(cmnt + "  33 day/crop coefficient pairs for alfalfa.");
		@out.println(cmnt + "  11 day/crop coefficient pairs for grass pasture.");
		@out.println(cmnt + "  22 day/crop coefficient pairs for all other crop types.");
		@out.println(cmnt);
		@out.println(cmnt + "Title");
		@out.println(cmnt + "NumCurves");
		@out.println(cmnt + "ID CropName");
		@out.println(cmnt + "Percent Coeff");
		@out.println(cmnt + "----------------------------");
		@out.println(cmnt + "EndHeader");
		// Default title
		@out.println("Crop Coefficient Curves for Penman-Monteith");

		int num = 0;
		if (data_Vector != null)
		{
			num = data_Vector.Count;
		}
		@out.println(num);
		// Width allows precision to be increased some...
		string value_format = "%8." + precision2 + "f";
		int i = 0;
		foreach (StateCU_PenmanMonteith kpm in data_Vector)
		{
			++i;
			if (kpm == null)
			{
				continue;
			}
			// Crop number (not used by StateCU) and name
			string name = kpm.getName();
			// Since free format, the ID must always have something.  If
			// we don't know, put -999...
			string id = "" + i; // Default to sequential number
			@out.println(id + " " + name);

			// Loop through the number of growth stages per crop
			int nGrowthStages = kpm.getNGrowthStages();
			int ncpgs = StateCU_PenmanMonteith.getNCoefficientsPerGrowthStage();
			for (int igs = 0; igs < nGrowthStages; igs++)
			{
				for (int j = 0; j < ncpgs; j++)
				{
					@out.println(StringUtil.formatString(kpm.getKcday(igs,j),"%3.0f") + StringUtil.formatString(kpm.getKcb(igs,j),value_format));
				}
			}
		}
	}

	/// <summary>
	/// Writes a list of StateCU_PenmanMonteith objects to a list file.  A header is 
	/// printed to the top of the file, containing the commands used to generate the file.
	/// Any strings in the body of the file that contain the field delimiter will be wrapped in "...". </summary>
	/// <param name="filename"> the name of the file to which the data will be written. </param>
	/// <param name="delimiter"> the delimiter to use for separating field values. </param>
	/// <param name="update"> whether to update an existing file, retaining the current 
	/// header (true) or to create a new file with a new header. </param>
	/// <param name="data"> the list of objects to write. </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeListFile(String filename, String delimiter, boolean update, java.util.List<StateCU_PenmanMonteith> data, java.util.List<String> outputComments) throws Exception
	public static void writeListFile(string filename, string delimiter, bool update, IList<StateCU_PenmanMonteith> data, IList<string> outputComments)
	{
		string routine = "StateCU_PenmanMonteith.writeListFile";
		int size = 0;
		if (data != null)
		{
			size = data.Count;
		}

		IList<string> fields = new List<string>();
		fields.Add("Name");
		fields.Add("GrowthStage");
		fields.Add("Percent");
		fields.Add("Coefficient");
		int fieldCount = fields.Count;

		string[] names = new string[fieldCount];
		string[] formats = new string[fieldCount];
		int comp = StateCU_DataSet.COMP_PENMAN_MONTEITH;
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
		int k = 0;
		PrintWriter @out = null;
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
			newComments2.Insert(1,"StateCU Penman-Monteith crop coefficients as a delimited list file.");
			newComments2.Insert(2,"");
			@out = IOUtil.processFileHeaders(oldFile, IOUtil.getPathUsingWorkingDir(filename), newComments2, commentString, ignoreCommentString, 0);

			for (int i = 0; i < fieldCount; i++)
			{
				if (i > 0)
				{
					buffer.Append(delimiter);
				}
				buffer.Append("\"" + names[i] + "\"");
			}

			@out.println(buffer.ToString());
			StateCU_PenmanMonteith kpm;
			for (int i = 0; i < size; i++)
			{
				kpm = data[i];
				int ngs = kpm.getNGrowthStages();
				int ncpgs = StateCU_PenmanMonteith.getNCoefficientsPerGrowthStage();
				for (int igs = 0; igs < ngs; igs++)
				{
					for (j = 0; j < ncpgs; j++)
					{
						line[0] = StringUtil.formatString(kpm.getName(), formats[0]).Trim();
						line[1] = StringUtil.formatString((igs + 1), formats[1]).Trim();
						line[2] = StringUtil.formatString(kpm.getKcday(igs,j), formats[2]).Trim();
						line[3] = StringUtil.formatString(kpm.getKcb(igs,j), formats[3]).Trim();

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
			@out = null;
		}
	}

	}

}