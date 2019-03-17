using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

// StateCU_BlaneyCriddle - class to hold StateCU Blaney-Criddle crop data, compatible with StateCU KBC file

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
// StateCU_BlaneyCriddle - class to hold StateCU Blaney-Criddle crop data,
//			compatible with StateCU KBC file
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
//
// 2002-12-01	Steven A. Malers, RTi	Copy CUCropCharacteristics class and
//					update for the KBC file contents.
// 2003-06-04	SAM, RTi		Rename class from CUBlaneyCriddle to
//					StateCU_BlaneyCriddle.
//					Change read/write methods to not use the
//					file extension in the method name.
// 2005-01-24	J. Thomas Sapienza, RTi	* Added createBackup().
//					* Added restoreOriginal().
// 2005-04-18	JTS, RTi		Added writeListFile().
// 2005-05-22	SAM, RTi		* When reading an existing file, set the
//					  crop ID to the name to facilitate
//					  sorting and other StateCU_Data
//					  features.  The crop number was
//					  previously used for the ID but is
//					  ignored in the model.
//					* Update the write method accordingly so
//					  that the resulting StateCU file has a
//					  -999 if the crop number is not an
//					  integer - this will hopefully allow
//					  the model to read the resulting file
//					  without error.
//					* Overload writeStateCUFile to take a
//					  PropList and add the Precision
//					  parameter.
// 2007-01-10	Kurt Tometich, RTi
//						Updated the readStateCUFile and writeVector
//						methods to support new and old versions.  The
//						newest version adds another field in the 3rd row
//						and adds 10 chars to the crop name.  There is also
//						a new field "ktsw" or "Blaney-Criddle Method."
// 2007-01-23	SAM, RTi		* Review KAT's code.
// 2007-03-04	SAM, RTi		Final cleanup before release.
// 2007-03-19	SAM, RTi		Write crop number as sequential integer.
//------------------------------------------------------------------------------
// EndHeader

namespace DWR.StateCU
{

	using IOUtil = RTi.Util.IO.IOUtil;
	using PropList = RTi.Util.IO.PropList;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// Class to hold StateCU Blaney-Criddle crop data for StateCU/StateDMI, compatible
	/// with the StateCU KBC file.
	/// </summary>
	public class StateCU_BlaneyCriddle : StateCU_Data, StateCU_ComponentValidator
	{

	// List data in the same order as in the StateCU documentation...

	// Cropn (Crop name) is stored in the base class name.
	// kcey (crop number) is stored in the base class ID, if necessary (currently not used).

	/// <summary>
	/// Growth curve type (Day=perennial crop, Percent=annual crop).
	/// </summary>
	private string __flag = StateCU_Util.MISSING_STRING;

	/// <summary>
	/// Day of year for annual crop types, null if perennial crop.
	/// </summary>
	private int[] __nckca = null;

	/// <summary>
	/// Crop coefficient for annual crop types, null if annual crop.
	/// </summary>
	private double[] __ckca = null;

	/// <summary>
	/// Percent of growing season for perennial crop types.
	/// </summary>
	private int[] __nckcp = null;

	/// <summary>
	/// Crop coefficient for perennial crop types.
	/// </summary>
	private double[] __ckcp = null;

	/// <summary>
	/// Flag for modified/original Blaney-Criddle
	/// </summary>
	private int __ktsw = StateCU_Util.MISSING_INT;

	/// <summary>
	/// Construct a StateCU_BlaneyCriddle instance and set to missing and empty data.
	/// Number of coefficients (as per StateCU this is currently always 25 for perennial
	/// crops and 21 for annual crops, but in the future the number may be made variable). </summary>
	/// <param name="curve_type"> "Day" for perennial crop (25 coefficients are assumed) or
	/// "Percent" for annual crop (21 coefficients are assumed). </param>
	public StateCU_BlaneyCriddle(string curve_type) : base()
	{
		setFlag(curve_type);
		if (__flag.Equals("Percent", StringComparison.OrdinalIgnoreCase))
		{
			// Annual crop
			__nckca = new int[21];
			__ckca = new double[21];
			// Default these to simplify setting in DMI and other code...
			for (int i = 0; i < 21; i++)
			{
				__nckca[i] = i * 5;
			}
		}
		else
		{ // Assume "Day" (perennial crop)
			__nckcp = new int[25];
			__ckcp = new double[25];
			// Default these to simplify setting in DMI and other code...
			__nckcp[0] = 1;
			__nckcp[1] = 15;
			__nckcp[2] = 32;
			__nckcp[3] = 46;
			__nckcp[4] = 60;
			__nckcp[5] = 74;
			__nckcp[6] = 91;
			__nckcp[7] = 105;
			__nckcp[8] = 121;
			__nckcp[9] = 135;
			__nckcp[10] = 152;
			__nckcp[11] = 166;
			__nckcp[12] = 182;
			__nckcp[13] = 196;
			__nckcp[14] = 213;
			__nckcp[15] = 227;
			__nckcp[16] = 244;
			__nckcp[17] = 258;
			__nckcp[18] = 274;
			__nckcp[19] = 288;
			__nckcp[20] = 305;
			__nckcp[21] = 319;
			__nckcp[22] = 335;
			__nckcp[23] = 349;
			__nckcp[24] = 366;
		}
	}

	/// <summary>
	/// Creates a backup of the current data object and stores it in _original,
	/// for use in determining if an object was changed inside of a GUI.
	/// </summary>
	public virtual void createBackup()
	{
		_original = clone();
		((StateCU_BlaneyCriddle)_original)._isClone = false;
		_isClone = true;

		StateCU_BlaneyCriddle bc = (StateCU_BlaneyCriddle)_original;

		if (bc.__ckca != null)
		{
			__ckca = new double[21];
			__nckca = new int[21];
			for (int i = 0; i < 21; i++)
			{
				__ckca[i] = bc.__ckca[i];
				__nckca[i] = bc.__nckca[i];
			}
		}
		else
		{
			__ckcp = new double[25];
			__nckcp = new int[25];
			for (int i = 0; i < 25; i++)
			{
				__ckcp[i] = bc.__ckcp[i];
				__nckcp[i] = bc.__nckcp[i];
			}
		}
	}

	/// <summary>
	/// Clean up for garbage collection. </summary>
	/// <exception cref="Throwable"> if there is an error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~StateCU_BlaneyCriddle()
	{
		__ckca = null;
		__ckcp = null;
		__nckca = null;
		__nckcp = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Return the crop coefficients for an annual crop. </summary>
	/// <returns> the crop coefficients for an annual crop. </returns>
	public virtual double [] getCkca()
	{
		return __ckca;
	}

	public virtual double getCkca(int index)
	{
		return __ckca[index];
	}

	/// <summary>
	/// Return the crop coefficients for a perennial crop. </summary>
	/// <returns> the crop coefficients for a perennial crop. </returns>
	public virtual double [] getCkcp()
	{
		return __ckcp;
	}

	public virtual double getCkcp(int index)
	{
		return __ckcp[index];
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
	/// Return the growth curve type flag. </summary>
	/// <returns> the growth curve type flag. </returns>
	public virtual string getFlag()
	{
		return __flag;
	}

	/// <summary>
	/// Return the Blaney-Criddle modified/original switch </summary>
	/// <returns> the Blaney-Criddle modified/original switch </returns>
	public virtual int getKtsw()
	{
		return __ktsw;
	}

	/// <summary>
	/// Return the day of year for an annual crop. </summary>
	/// <returns> the day of year for an annual crop. </returns>
	public virtual int [] getNckca()
	{
		return __nckca;
	}

	public virtual int getNckca(int index)
	{
		return __nckca[index];
	}

	/// <summary>
	/// Return the percent of growing season for a perennial crop. </summary>
	/// <returns> the percent of growing season for a perennial crop. </returns>
	public virtual int [] getNckcp()
	{
		return __nckcp;
	}

	public virtual int getNckcp(int index)
	{
		return __nckcp[index];
	}

	/// <summary>
	/// Indicate whether the crop is an annual crop (has coefficients specified for percent of growing season) or
	/// perennial crop (has coefficients specified for day of year). </summary>
	/// <returns> the day of year for an annual crop. </returns>
	public virtual bool isAnnualCrop()
	{
		if (__nckca != null)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	/// <summary>
	/// Checks for version 10 by reading the file
	/// and checking the number of fields.  Version 10 has 3 values in record 2.
	/// Version 11+ has 4 values in record 3.  It is assumed that values are separated by whitespace. </summary>
	/// <param name="filename"> Path to Blaney-Criddle file to check. </param>
	/// <returns> true if the file is for version 10, false if not. </returns>
	/// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static boolean isVersion_10(String filename) throws java.io.IOException
	private static bool isVersion_10(string filename)
	{
		bool rVal = false; // Not version 10
		string fname = filename;
		string line = "";
		StreamReader input = null;
		int count = 0; // Record count - interested in third record

		// Read the StateCU file.  Only read the first line 
		// This is enough to know if it is version 10
		input = new StreamReader(fname);
		while (!string.ReferenceEquals((line = input.ReadLine()), null))
		{
			// check for comments
			if (line.StartsWith("#", StringComparison.Ordinal) || line.Trim().Length == 0)
			{
				continue;
			}

			// Version 10 has 3 columns in the third row - the newest version has 4 columns
			if (count == 2)
			{
				IList<string> tmp = StringUtil.breakStringList(line, " \t", StringUtil.DELIM_SKIP_BLANKS);
				if (tmp.Count == 3)
				{
					rVal = true;
				}
				break;
			}
			count++;
		}
		input.Close();
		return rVal;
	}


	/// <summary>
	/// Read the StateCU KBC file and return as a Vector of StateCU_BlaneyCriddle. </summary>
	/// <param name="filename"> filename containing KBC records. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.List<StateCU_BlaneyCriddle> readStateCUFile(String filename) throws java.io.IOException
	public static IList<StateCU_BlaneyCriddle> readStateCUFile(string filename)
	{
		string rtn = "StateCU_BlaneyCriddle.readKBCFile";
		string iline = null;
		StateCU_BlaneyCriddle kbc = null;
		IList<StateCU_BlaneyCriddle> kbc_Vector = new List<StateCU_BlaneyCriddle>(25);
		StreamReader @in = null;
		bool version10 = isVersion_10(filename); // Is version 10 (old) format?

		Message.printStatus(1, rtn, "Reading StateCU KBC file: " + filename);
		// The following throws an IOException if the file cannot be opened...
		@in = new StreamReader(filename);
		int nc = -1;
		string title = null; // The title is currently read but not stored since it is never really used for anything.
		while (!string.ReferenceEquals((iline = @in.ReadLine()), null))
		{
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
				nc = StringUtil.atoi(iline.Trim());
				break;
			}
		}

		// Now loop through the number of curves...

		// TODO SAM 2007-02-18 Evaluate if needed
		//String id;
		string cropn, flag;
		int npts;
		int[] nckca = null;
		int[] nckcp = null;
		double[] ckc = null;
		string ktsw = null;
		IList<string> tokens;
		int j = 0;
		for (int i = 0; i < nc; i++)
		{
			nckca = null; // use to check whether annual or perennial below.
			// Read a free format line...

			iline = @in.ReadLine();

			tokens = StringUtil.breakStringList(iline.Trim(), " \t", StringUtil.DELIM_SKIP_BLANKS);
			// TODO SAM 2007-02-18 Evaluate if needed
			//id = (String)tokens.elementAt(0);
			cropn = (string)tokens[1];
			flag = (string)tokens[2];

			if (version10)
			{
				ktsw = "";
			}
			else
			{
				ktsw = (string)tokens[3];
			}
			// Allocate new StateCU_BlaneyCriddle instance...

			kbc = new StateCU_BlaneyCriddle(flag);
			kbc.setName(cropn);
			// TODO SAM 2005-05-22 Ignore the old ID and use the crop name - this facilitates
			// sorting and other standard StateCU_Data features.
			//kbc.setID ( id );
			kbc.setID(cropn);

			if (StringUtil.isInteger(ktsw))
			{
				kbc.setKtsw(StringUtil.atoi(ktsw));
			}

			// Read the coefficients...
			if (flag.Equals("Day", StringComparison.OrdinalIgnoreCase))
			{
				ckc = kbc.getCkcp();
				nckcp = kbc.getNckcp();
			}
			else
			{
				ckc = kbc.getCkca();
				nckca = kbc.getNckca();
			}
			npts = ckc.Length;

			for (j = 0; j < npts; j++)
			{
				iline = @in.ReadLine();

				tokens = StringUtil.breakStringList(iline.Trim(), " \t", StringUtil.DELIM_SKIP_BLANKS);
				if (nckca == null)
				{
					// Processing perennial crop...
					nckcp[j] = StringUtil.atoi((string)tokens[0]);
					ckc[j] = StringUtil.atod((string)tokens[1]);
				}
				else
				{
					// Processing annual crop...
					nckca[j] = StringUtil.atoi((string)tokens[0]);
					ckc[j] = StringUtil.atod((string)tokens[1]);
				}
			}

			// add the StateCU_BlaneyCriddle to the vector...
			kbc_Vector.Add(kbc);
		}
		if (@in != null)
		{
			@in.Close();
		}
		return kbc_Vector;
	}

	/// <summary>
	/// Cancels any changes made to this object within a GUI since createBackup()
	/// was called and sets _original to null.
	/// </summary>
	public override void restoreOriginal()
	{
		StateCU_BlaneyCriddle bc = (StateCU_BlaneyCriddle)_original;
		base.restoreOriginal();

		__flag = bc.__flag;
		__ckca = bc.__ckca;
		__nckca = bc.__nckca;
		__ckcp = bc.__ckcp;
		__nckcp = bc.__nckcp;

		_isClone = false;
		_original = null;
	}

	/// <summary>
	/// Set the curve value for the crop coefficient curve (coefficient only). </summary>
	/// <param name="i"> Index in the curve (zero-index).
	/// For example 1 corresponds to 5% for a percent curve or day 15 for a day curve. </param>
	/// <param name="coeff"> Value at the day/percent position. </param>
	public virtual void setCurveValue(int i, double coeff)
	{
		if (__flag.Equals("Percent", StringComparison.OrdinalIgnoreCase))
		{
			// Percent of growing season - Annual..
			__ckca[i] = coeff;
		}
		else
		{
			// Day of year - Perennial...
			__ckcp[i] = coeff;
		}
	}

	/// <summary>
	/// Set the values for the crop coefficient curve (curve position and coefficient). </summary>
	/// <param name="i"> Index in the curve (zero-index).
	/// For example 1 corresponds to 5% for a percent curve or day 15 for a day curve. </param>
	/// <param name="pos"> Position in the curve (day or percent depending on the curve type). </param>
	/// <param name="coeff"> Value at the position. </param>
	public virtual void setCurveValues(int i, int pos, double coeff)
	{
		if (__flag.Equals("Percent", StringComparison.OrdinalIgnoreCase))
		{
			// Annual..
			__nckca[i] = pos;
			__ckca[i] = coeff;
		}
		else
		{
			// Perennial...
			__nckcp[i] = pos;
			__ckcp[i] = coeff;
		}
	}

	/// <summary>
	/// Sets the day or percent for a curve value. </summary>
	/// <param name="i"> the index in the curve (zero-index).
	/// For example 1 corresponds to 5% for a percent curve or day 15 for a day curve. </param>
	/// <param name="pos"> the new day or percent to change the index position to. </param>
	public virtual void setCurvePosition(int i, int pos)
	{
		if (__flag.Equals("Percent", StringComparison.OrdinalIgnoreCase))
		{
			// Percent of growing season - Annual..
			__nckca[i] = pos;
		}
		else
		{
			// Day of year - Perennial...
			__nckcp[i] = pos;
		}
	}

	/// <summary>
	/// Set the growth curve type ("Day" for perennial crop or "Percent" for annual
	/// crop).  This should normally only be called by the constructor. </summary>
	/// <param name="flag"> Growth curve type. </param>
	public virtual void setFlag(string flag)
	{
		__flag = flag;
	}

	/// <summary>
	/// Sets the original/modified flag for Blaney-Criddle </summary>
	/// <param name="flag"> for ktsw 
	/// (0 = SCS Modified, 1 = Original, 2 = Modified with elevation, 
	/// 3 = Original with elevation, 4 = Estimating potential ET)  </param>
	public virtual void setKtsw(int ktsw)
	{
		__ktsw = ktsw;
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
		int[] nckca = getNckca();
		int[] nckcp = getNckcp();
		if (((nckca == null) || (nckca.Length == 0)) && ((nckcp == null) || (nckcp.Length == 0)))
		{
			validation.add(new StateCU_ComponentValidationProblem(this,"Crop \"" + id + "\" data are neither specified as day of year or percent of season.", "Specify coefficients for day of year OR percent of season."));
		}
		else if (((nckca != null) && (nckca.Length > 0)) && ((nckcp != null) && (nckcp.Length > 0)))
		{
			validation.add(new StateCU_ComponentValidationProblem(this,"Crop \"" + id + "\" data are specified as day of year and percent of season.", "Specify coefficients for day of year OR percent of season."));
		}
		else if (isAnnualCrop())
		{
			// Annual - percent of season
			double[] ckca = getCkca();
			for (int i = 0; i < nckca.Length; i++)
			{
				if ((nckca[i] < 0) || (nckca[i] > 100))
				{
					validation.add(new StateCU_ComponentValidationProblem(this,"Crop \"" + id + "\" percent of season (" + nckca[i] + ") is invalid.", "Specify as 0 to 100."));
				}
				if ((ckca[i] < 0) || (ckca[i] > 3.0))
				{
					validation.add(new StateCU_ComponentValidationProblem(this,"Crop \"" + id + "\" coefficient (" + ckca[i] + ") is invalid.", "Specify as 0 to 3.0 (upper limit may vary by location)."));
				}
			}
		}
		else
		{
			// Perennial - day of year
			double[] ckcp = getCkcp();
			for (int i = 0; i < nckcp.Length; i++)
			{
				if ((nckcp[i] < 1) || (nckcp[i] > 366))
				{
					validation.add(new StateCU_ComponentValidationProblem(this,"Crop \"" + id + "\" day of year (" + nckcp[i] + ") is invalid.", "Specify as 1 to 366."));
				}
				if ((ckcp[i] < 0) || (ckcp[i] > 3.0))
				{
					validation.add(new StateCU_ComponentValidationProblem(this,"Crop \"" + id + "\" coefficient (" + ckcp[i] + ") is invalid.", "Specify as 0 to 3.0 (upper limit may vary by location)."));
				}
			}
		}
		// Check method
		int ktsw = getKtsw();
		if ((ktsw < 0) || (ktsw > 4))
		{
			validation.add(new StateCU_ComponentValidationProblem(this,"Crop \"" + id + "\" Blaney-Criddle method (" + ktsw + ") is invalid.", "Specify as 0 to 4 (refer to StateCU documentation)."));
		}

		return validation;
	}

	/// <summary>
	/// Write a list of StateCU_BlaneyCriddle to a file.  The filename is adjusted to
	/// the working directory if necessary using IOUtil.getPathUsingWorkingDir(). </summary>
	/// <param name="filename_prev"> The name of the previous version of the file (for
	/// processing headers).  Specify as null if no previous file is available. </param>
	/// <param name="filename"> The name of the file to write. </param>
	/// <param name="data_Vector"> A list of StateCU_BlaneyCriddle to write. </param>
	/// <param name="new_comments"> Comments to add to the top of the file.  Specify as null 
	/// if no comments are available. </param>
	/// <exception cref="IOException"> if there is an error writing the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeStateCUFile(String filename_prev, String filename, java.util.List<StateCU_BlaneyCriddle> data_Vector, java.util.List<String> new_comments) throws java.io.IOException
	public static void writeStateCUFile(string filename_prev, string filename, IList<StateCU_BlaneyCriddle> data_Vector, IList<string> new_comments)
	{
		writeStateCUFile(filename_prev, filename, data_Vector, new_comments, null);
	}

	/// <summary>
	/// Write a list of StateCU_BlaneyCriddle to a file.  The filename is adjusted to
	/// the working directory if necessary using IOUtil.getPathUsingWorkingDir(). </summary>
	/// <param name="filename_prev"> The name of the previous version of the file (for
	/// processing headers).  Specify as null if no previous file is available. </param>
	/// <param name="filename"> The name of the file to write. </param>
	/// <param name="data_Vector"> A list of StateCU_BlaneyCriddle to write. </param>
	/// <param name="new_comments"> Comments to add to the top of the file.  Specify as null 
	/// if no comments are available. </param>
	/// <param name="props"> Properties to control the output.  Currently only the
	/// optional Precision property can be set, indicating how many digits after the
	/// decimal should be printed (default is 3). </param>
	/// <exception cref="IOException"> if there is an error writing the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeStateCUFile(String filename_prev, String filename, java.util.List<StateCU_BlaneyCriddle> data_Vector, java.util.List<String> new_comments, RTi.Util.IO.PropList props) throws java.io.IOException
	public static void writeStateCUFile(string filename_prev, string filename, IList<StateCU_BlaneyCriddle> data_Vector, IList<string> new_comments, PropList props)
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
		writeVector(data_Vector, @out, props);
		@out.flush();
		@out.close();
		@out = null;
	}

	/// <summary>
	/// Write a list of StateCU_BlaneyCriddle to an opened file. </summary>
	/// <param name="data_Vector"> A Vector of StateCU_BlaneyCriddle to write. </param>
	/// <param name="out"> output PrintWriter. </param>
	/// <param name="props"> Properties to control the output.  Currently only the
	/// optional Precision property can be set, indicating how many digits after the
	/// decimal should be printed (default is 3). </param>
	/// <exception cref="IOException"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static void writeVector(java.util.List<StateCU_BlaneyCriddle> data_Vector, java.io.PrintWriter out, RTi.Util.IO.PropList props) throws java.io.IOException
	private static void writeVector(IList<StateCU_BlaneyCriddle> data_Vector, PrintWriter @out, PropList props)
	{
		int i, j;
		string cmnt = "#>";
		// Missing data are handled by formatting all as strings (blank if necessary).
		bool version10 = false; // Indicate if old Version 10 format is written

		if (props == null)
		{
			props = new PropList("StateCU_BlaneyCriddle");
		}
		string Precision = props.getValue("Precision");
		string Version = props.getValue("Version");
		if (!string.ReferenceEquals(Version, null) && Version.Equals("10"))
		{
			// Version 10 is an older version.
			version10 = true;
		}

		int Precision_int = 3;
		if ((!string.ReferenceEquals(Precision, null)) && StringUtil.isInteger(Precision))
		{
			Precision_int = StringUtil.atoi(Precision);
		}

		@out.println(cmnt);
		@out.println(cmnt + "  StateCU Blaney-Criddle Crop Coefficient (KBC) File");
		@out.println(cmnt);
		@out.println(cmnt + "  Record 1 format (a80)");
		@out.println(cmnt);
		@out.println(cmnt + "  Title     remark:  Title");
		@out.println(cmnt);
		@out.println(cmnt + "  Record 2 format (free format)");
		@out.println(cmnt);
		@out.println(cmnt + "  NumCurves     nc:  Number of crop coefficient curves");
		@out.println(cmnt);
		@out.println(cmnt + "  Record 3 format (free format)");
		@out.println(cmnt);
		@out.println(cmnt + "  ID            id:  Crop number (not used by StateCU)");
		@out.println(cmnt + "  CropName   cropn:  Crop name (e.g., ALFALFA)");
		@out.println(cmnt + "  CurveType   flag:  Growth curve type");
		@out.println(cmnt + "                     Day = perennial; specify 25 values");
		@out.println(cmnt + "                           for start, middle, end of month");
		@out.println(cmnt + "                     Percent = annual; specify 21 values");
		@out.println(cmnt + "                           for 0, 5, ..., 100% of season");
		@out.println(cmnt);
		if (!version10)
		{
			// Include newer format information...
			@out.println(cmnt + "  BCMethod    ktsw:  Blaney-Criddle Method");
			@out.println(cmnt + "                     0 = SCS Modified Blaney-Criddle");
			@out.println(cmnt + "                     1 = Original Blaney-Criddle");
			@out.println(cmnt + "                     2 = Modifed Blaney-Criddle w/ Elev. Adj.");
			@out.println(cmnt + "                     3 = Original Blaney-Criddle w/ Elev. Adj.");
			@out.println(cmnt + "                     4 = Pochop");
			@out.println(cmnt);
		}
		@out.println(cmnt + "  Record 4 format (free format)");
		@out.println(cmnt);
		@out.println(cmnt + "Position     nckca:  Percent (0 to 100) of growing season for annual crop");
		@out.println(cmnt + "             nckcp:  Day of year (1 to 366) for perennial crop");
		@out.println(cmnt + "Coeff         ckca:  Crop coefficient for annual crop");
		@out.println(cmnt + "         OR   ckcp:  Crop coefficient for perennial crop");
		@out.println(cmnt);
		@out.println(cmnt + "Title...");
		@out.println(cmnt + "NumCurves");
		@out.println(cmnt + "ID CropName CurveType");
		@out.println(cmnt + "Position Coeff");
		@out.println(cmnt + "----------------------------");
		@out.println(cmnt + "EndHeader");
		@out.println("Crop Coefficient Curves for Blaney-Criddle");

		int num = 0;
		if (data_Vector != null)
		{
			num = data_Vector.Count;
		}
		@out.println(num);
		StateCU_BlaneyCriddle kbc = null;
		int[] nckca = null;
		int[] nckcp = null;
		double[] ckca = null;
		double[] ckcp = null;
		int size = 0;
		string value_format = "%9." + Precision_int + "f";
		for (i = 0; i < num; i++)
		{
			kbc = (StateCU_BlaneyCriddle)data_Vector[i];
			if (kbc == null)
			{
				continue;
			}

			// Just get all the data.  Null arrays are used as a check
			// below to know what data to output...
			nckca = kbc.getNckca();
			nckcp = kbc.getNckcp();
			ckca = kbc.getCkca();
			ckcp = kbc.getCkcp();

			// Do not truncate the name to 20 characters if version 10 because
			// doing so may result in arbitrary cut of the current crop names and
			// result in different output from old anyhow.
			string name = kbc.getName();
			// Since free format, the ID must always have something.  If
			// we don't know, put -999...
			string id = "" + (i + 1); // Default to sequential number
			if (version10)
			{
				// Previously used -999
				id = "-999";
			}
			if (!StateCU_Util.isMissing(kbc.getID()))
			{
				// Changes elsewhere impact this so also use -999 unless it is a number
				if (StringUtil.isInteger(kbc.getID()))
				{
					id = "" + kbc.getID();
				}
				else
				{
					id = "-999";
				}
				// Can't use the crop name because StateCU expects a number (?)
				//id = kbc.getID();
			}
			// Output based on the version because file comparisons may be done when verifying files.
			if (version10)
			{
				// No ktsw...
				@out.println(id + " " + name + " " + kbc.getFlag());
			}
			else
			{
				// With ktsw, but OK if blank.
				@out.println(id + " " + name + " " + kbc.getFlag() + " " + kbc.getKtsw());
			}

			if (nckca != null)
			{
				size = nckca.Length;
			}
			else
			{
				size = nckcp.Length;
			}
			for (j = 0; j < size; j++)
			{
				if (nckca != null)
				{
					// Print annual curve (Percent)...
					@out.println(StringUtil.formatString(nckca[j],"%-3d") + StringUtil.formatString(ckca[j],value_format));
				}
				else
				{
					// Print perennial curve (Day)...
					@out.println(StringUtil.formatString((int)nckcp[j],"%-3d") + StringUtil.formatString(ckcp[j],value_format));
				}
			}
		}
	}

	/// <summary>
	/// Writes a list of StateCU_BlaneyCriddle objects to a list file.  A header is 
	/// printed to the top of the file, containing the commands used to generate the 
	/// file.  Any strings in the body of the file that contain the field delimiter will be wrapped in "...". </summary>
	/// <param name="filename"> the name of the file to which the data will be written. </param>
	/// <param name="delimiter"> the delimiter to use for separating field values. </param>
	/// <param name="update"> whether to update an existing file, retaining the current 
	/// header (true) or to create a new file with a new header. </param>
	/// <param name="data"> the Vector of objects to write. </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeListFile(String filename, String delimiter, boolean update, java.util.List<StateCU_BlaneyCriddle> data, java.util.List<String> outputComments) throws Exception
	public static void writeListFile(string filename, string delimiter, bool update, IList<StateCU_BlaneyCriddle> data, IList<string> outputComments)
	{
		string routine = "StateCU_BlaneyCriddle.writeListFile";
		int size = 0;
		if (data != null)
		{
			size = data.Count;
		}

		IList<string> fields = new List<string>();
		fields.Add("Name");
		fields.Add("CurveType");
		fields.Add("DayPercent");
		fields.Add("Coefficient");
		int fieldCount = fields.Count;

		string[] names = new string[fieldCount];
		string[] formats = new string[fieldCount];
		int comp = StateCU_DataSet.COMP_BLANEY_CRIDDLE;
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
		PrintWriter @out = null;
		StateCU_BlaneyCriddle bc = null;
		IList<string> commentString = new List<string>(1);
		commentString.Add("#");
		IList<string> ignoreCommentString = new List<string>(1);
		ignoreCommentString.Add("#>");
		string[] line = new string[fieldCount];
		string flag = null;
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
			newComments2.Insert(1,"StateCU Blaney-Criddle crop coefficients as a delimited list file.");
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

			for (int i = 0; i < size; i++)
			{
				bc = (StateCU_BlaneyCriddle)data[i];
				flag = bc.getFlag();
				if (flag.Equals("Percent", StringComparison.OrdinalIgnoreCase))
				{
					for (j = 0; j < 21; j++)
					{
						line[0] = StringUtil.formatString(bc.getName(), formats[0]).Trim();
						line[1] = StringUtil.formatString(bc.getFlag(), formats[1]).Trim();
						line[2] = StringUtil.formatString(bc.getNckca(j), formats[2]).Trim();
						line[3] = StringUtil.formatString(bc.getCkca(j), formats[3]).Trim();

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
				else
				{
					for (j = 0; j < 25; j++)
					{
						line[0] = StringUtil.formatString(bc.getName(), formats[0]).Trim();
						line[1] = StringUtil.formatString(bc.getFlag(), formats[1]).Trim();
						line[2] = StringUtil.formatString(bc.getNckcp(j), formats[2]).Trim();
						line[3] = StringUtil.formatString(bc.getCkcp(j),formats[3]).Trim();

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