using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

// StateCU_CropCharacteristics - class to hold CU crop characteristics data, compatible with StateCU CCH file

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
// StateCU_CropCharacteristics - class to hold CU crop characteristics data,
//			compatible with StateCU CCH file
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
//
// 2002-11-14	Steven A. Malers, RTi	Copy CULocation class and update for
//					the CCH file contents.
// 2003-02-19	SAM, RTi		Change so any missing data are printed
//					as blanks.
// 2003-06-04	SAM, RTi		Rename class from CUCropCharacteristics
//					to StateCU_CropCharacteristics.
//					Change read/write methods to not use
//					file extension in method name.
// 2003-07-02	SAM, RTi		Change so that the identifier and name
//					are the same.  The old crop number is
//					no longer used but there is no name
//					field.
// 2005-01-17	J. Thomas Sapienza, RTi	* Added createBackup().
//					* Added restoreOriginal().
// 2005-04-18	JTS, RTi		Added writeListFile().
// 2007-01-05   KAT, RTi		updates to the format
// 							Old Format ver. 10 ("x" is a space): 
//  						(a20,2(1x,i2,2x,i2),2x,i2,2x,2(i4,1x),3(f3,0,1x),
//  						4(f4.1,1x),2(i2,1x),i3,1x,i2)
// 							New Format (spaces are included):
//  						(a30,10(i6),4(f6.1),4(i5))
// 2007-01-29	SAM, RTi		Review KAT's code.  Clean code based on Eclipse
//					information.
// 2007-03-04	SAM, RTi		Some final code cleanup - formats were not quite
//							in agreement with StateCU documentation.
// 2007-03-19	SAM, RTi		Write the crop number as a sequential integer.
// 2007-04-22	SAM, RTi		Minor change to fix extra EndHeader comment in
//					output.  Add AutoAdjust to write method properties.
//------------------------------------------------------------------------------

namespace DWR.StateCU
{

	using IOUtil = RTi.Util.IO.IOUtil;
	using PropList = RTi.Util.IO.PropList;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;
	using TimeUtil = RTi.Util.Time.TimeUtil;

	/// <summary>
	/// Class to hold StateCU crop characteristics data for StateCU/StateDMI, compatible
	/// with the StateCU CCH file.  The method names correspond exactly to CCH variable
	/// names as of StateCU Version 10 documentation.
	/// </summary>
	public class StateCU_CropCharacteristics : StateCU_Data, StateCU_ComponentValidator
	{

	// List data in the same order as in the StateCU documentation...

	// Cropn (Crop name) is stored in the base class name.
	// kcey (crop number) is stored in the base class ID, if necessary (currently not used).

	/// <summary>
	/// Planting month.
	/// </summary>
	private int __gdate1 = StateCU_Util.MISSING_INT;

	/// <summary>
	/// Planting day.
	/// </summary>
	private int __gdate2 = StateCU_Util.MISSING_INT;

	/// <summary>
	/// Harvest month.
	/// </summary>
	private int __gdate3 = StateCU_Util.MISSING_INT;

	/// <summary>
	/// Harvest day.
	/// </summary>
	private int __gdate4 = StateCU_Util.MISSING_INT;

	/// <summary>
	/// Days to full cover.
	/// </summary>
	private int __gdate5 = StateCU_Util.MISSING_INT;

	/// <summary>
	/// Length of season.
	/// </summary>
	private int __gdates = StateCU_Util.MISSING_INT;

	/// <summary>
	/// Temperature early moisture (F).
	/// </summary>
	private double __tmois1 = StateCU_Util.MISSING_DOUBLE;

	/// <summary>
	/// Temperature late moisture (F).
	/// </summary>
	private double __tmois2 = StateCU_Util.MISSING_DOUBLE;

	/// <summary>
	/// Management allowable deficit level.
	/// </summary>
	private double __mad = StateCU_Util.MISSING_DOUBLE;

	/// <summary>
	/// Initial root zone depth (IN).
	/// </summary>
	private double __irx = StateCU_Util.MISSING_DOUBLE;

	/// <summary>
	/// Maximum root zone depth (IN).
	/// </summary>
	private double __frx = StateCU_Util.MISSING_DOUBLE;

	/// <summary>
	/// Available water holding capacity (IN?, IN/IN?).
	/// </summary>
	private double __awc = StateCU_Util.MISSING_DOUBLE;

	/// <summary>
	/// Maximum application depth (IN).
	/// </summary>
	private double __apd = StateCU_Util.MISSING_DOUBLE;

	/// <summary>
	/// Spring frost date flag (0=mean, 1=28F, 2=32F).
	/// </summary>
	private int __tflg1 = StateCU_Util.MISSING_INT;

	/// <summary>
	/// Fall frost date flag (0=mean, 1=28F, 2=32F).
	/// </summary>
	private int __tflg2 = StateCU_Util.MISSING_INT;

	/// <summary>
	/// Days between 1st and 2nd cut.
	/// </summary>
	private int __cut2 = StateCU_Util.MISSING_INT;

	/// <summary>
	/// Days between 2nd and 3rd cut.
	/// </summary>
	private int __cut3 = StateCU_Util.MISSING_INT;

	/// <summary>
	/// Construct a StateCU_CropCharacteristics instance and set to missing and empty data.
	/// </summary>
	public StateCU_CropCharacteristics() : base()
	{
	}

	/// <summary>
	/// Creates a backup of the current data object and stores it in _original,
	/// for use in determining if an object was changed inside of a GUI.
	/// </summary>
	public virtual void createBackup()
	{
		_original = clone();
		((StateCU_CropCharacteristics)_original)._isClone = false;
		_isClone = true;
	}

	/// <summary>
	/// Clean up for garbage collection. </summary>
	/// <exception cref="Throwable"> if there is an error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~StateCU_CropCharacteristics()
	{
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Return the maximum application depth (IN). </summary>
	/// <returns> the maximum application depth (IN). </returns>
	public virtual double getApd()
	{
		return __apd;
	}

	/// <summary>
	/// Return the available water holding capacity (AWC). </summary>
	/// <returns> the available water holding capacity (AWC). </returns>
	public virtual double getAwc()
	{
		return __awc;
	}

	/// <summary>
	/// Return the days between 1st and 2nd cuts for *ALFALFA*. </summary>
	/// <returns> the days between 1st and 2nd cuts for *ALFALFA*. </returns>
	public virtual int getCut2()
	{
		return __cut2;
	}

	/// <summary>
	/// Return the days between 2nd and 3rd cuts for *ALFALFA*. </summary>
	/// <returns> the days between 2nd and 3rd cuts for *ALFALFA*. </returns>
	public virtual int getCut3()
	{
		return __cut3;
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
	/// Return the maximum root zone depth (FRX). </summary>
	/// <returns> the maximum root zone depth (FRX). </returns>
	public virtual double getFrx()
	{
		return __frx;
	}

	/// <summary>
	/// Return the planting month. </summary>
	/// <returns> the planting month. </returns>
	public virtual int getGdate1()
	{
		return __gdate1;
	}

	/// <summary>
	/// Return the planting day. </summary>
	/// <returns> the planting day. </returns>
	public virtual int getGdate2()
	{
		return __gdate2;
	}

	/// <summary>
	/// Return the harvest month. </summary>
	/// <returns> the harvest month. </returns>
	public virtual int getGdate3()
	{
		return __gdate3;
	}

	/// <summary>
	/// Return the harvest day. </summary>
	/// <returns> the harvest day. </returns>
	public virtual int getGdate4()
	{
		return __gdate4;
	}

	/// <summary>
	/// Return the days to full cover. </summary>
	/// <returns> the days to full cover. </returns>
	public virtual int getGdate5()
	{
		return __gdate5;
	}

	/// <summary>
	/// Return the length of the season. </summary>
	/// <returns> the length of the season. </returns>
	public virtual int getGdates()
	{
		return __gdates;
	}

	/// <summary>
	/// Return the initial root zone depth (IRX). </summary>
	/// <returns> the initial root zone depth (IRX). </returns>
	public virtual double getIrx()
	{
		return __irx;
	}

	/// <summary>
	/// Return the management allowable deficit (MAD). </summary>
	/// <returns> the management allowable deficit (MAD). </returns>
	public virtual double getMad()
	{
		return __mad;
	}

	/// <summary>
	/// Return the spring frost date flag. </summary>
	/// <returns> the spring frost date flag. </returns>
	public virtual int getTflg1()
	{
		return __tflg1;
	}

	/// <summary>
	/// Return the fall frost date flag. </summary>
	/// <returns> the fall frost date flag. </returns>
	public virtual int getTflg2()
	{
		return __tflg2;
	}

	/// <summary>
	/// Return the temperature early moisture (F). </summary>
	/// <returns> the temperature early moisture (F). </returns>
	public virtual double getTmois1()
	{
		return __tmois1;
	}

	/// <summary>
	/// Return the temperature late moisture (F). </summary>
	/// <returns> the temperature late moisture (F). </returns>
	public virtual double getTmois2()
	{
		return __tmois2;
	}

	/// <summary>
	/// Checks for version 10 by reading the file and checking the record length. 
	/// Version 10 records are < 103 characters and version 11+ are >= 103.  This
	/// is actually a compromise given the old format was 94 and the new is 134,
	/// according to the StateCU documentation. </summary>
	/// <param name="filename"> File to check. </param>
	/// <returns> true if version 10 or false if version 11+. </returns>
	/// <exception cref="IOException">  </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static boolean isVersion_10(String filename) throws java.io.IOException
	private static bool isVersion_10(string filename)
	{
		bool rVal = false;
		string fname = filename;
		string line = "";
		StreamReader input = null;

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

			if (line.Length < 103)
			{
				rVal = true;
				break;
			}
		}
		input.Close();
		return rVal;
	}


	/// <summary>
	/// Read the StateCU CCH file and return as a list of StateCU_CropCharacteristics. </summary>
	/// <param name="filename"> filename containing CCH records. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.List<StateCU_CropCharacteristics> readStateCUFile(String filename) throws java.io.IOException
	public static IList<StateCU_CropCharacteristics> readStateCUFile(string filename)
	{
		string rtn = "StateCU_CropCharacteristics.readStateCUFile";
		string iline = null;
		IList<object> v = new List<object>(18);
		IList<StateCU_CropCharacteristics> cch_Vector = new List<StateCU_CropCharacteristics>(100);
		// Don't read the crop number...
		int[] v10_format_0 = new int[] {StringUtil.TYPE_STRING, StringUtil.TYPE_SPACE, StringUtil.TYPE_STRING, StringUtil.TYPE_SPACE, StringUtil.TYPE_STRING, StringUtil.TYPE_SPACE, StringUtil.TYPE_STRING, StringUtil.TYPE_SPACE, StringUtil.TYPE_STRING, StringUtil.TYPE_SPACE, StringUtil.TYPE_STRING, StringUtil.TYPE_SPACE, StringUtil.TYPE_STRING, StringUtil.TYPE_SPACE, StringUtil.TYPE_STRING, StringUtil.TYPE_SPACE, StringUtil.TYPE_STRING, StringUtil.TYPE_SPACE, StringUtil.TYPE_STRING, StringUtil.TYPE_SPACE, StringUtil.TYPE_STRING, StringUtil.TYPE_SPACE, StringUtil.TYPE_STRING, StringUtil.TYPE_SPACE, StringUtil.TYPE_STRING, StringUtil.TYPE_SPACE, StringUtil.TYPE_STRING, StringUtil.TYPE_SPACE, StringUtil.TYPE_STRING, StringUtil.TYPE_SPACE, StringUtil.TYPE_STRING, StringUtil.TYPE_SPACE, StringUtil.TYPE_STRING, StringUtil.TYPE_SPACE, StringUtil.TYPE_STRING};

		// New format for version 11+
		int[] format_0 = new int[] {StringUtil.TYPE_STRING, StringUtil.TYPE_SPACE, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING};

		int[] v10_format_Ow = new int[] {20, 5, 2, 1, 2, 2, 2, 2, 2, 2, 4, 1, 4, 1, 3, 1, 3, 1, 3, 1, 4, 1, 4, 1, 4, 1, 4, 1, 2, 1, 2, 1, 3, 1, 2};

		// New format for version 11+
		int[] format_0w = new int[] {30, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6};

		// set the version based on the length of this field
		// Version 10 uses 20 characters and newer versions use 30
		if (isVersion_10(filename))
		{
			Message.printStatus(2, rtn, "Format of file was found to be" + " version 10.  Will use old format for reading.");
			// Reset formats to old...
			format_0w = v10_format_Ow;
			format_0 = v10_format_0;
		}
		StateCU_CropCharacteristics cch = null;
		StreamReader @in = null;

		Message.printStatus(2, rtn, "Reading StateCU CCH file: " + filename);
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

			// allocate new StateCU_CropCharacteristics instance...
			cch = new StateCU_CropCharacteristics();
			StringUtil.fixedRead(iline, format_0, format_0w, v);
			cch.setName(((string)v[0]).Trim());
			// Set the ID the same as the name for data management purposes.
			// Maybe later a true name field will be added.
			cch.setID(((string)v[0]).Trim());
			// Crop key is not even read so continue with index 1...
			@string = ((string)v[1]).Trim();
			if ((@string.Length != 0) && StringUtil.isInteger(@string))
			{
				cch.setGdate1(StringUtil.atoi(@string));
			}
			@string = ((string)v[2]).Trim();
			if ((@string.Length != 0) && StringUtil.isInteger(@string))
			{
				cch.setGdate2(StringUtil.atoi(@string));
			}
			@string = ((string)v[3]).Trim();
			if ((@string.Length != 0) && StringUtil.isInteger(@string))
			{
				cch.setGdate3(StringUtil.atoi(@string));
			}
			@string = ((string)v[4]).Trim();
			if ((@string.Length != 0) && StringUtil.isInteger(@string))
			{
				cch.setGdate4(StringUtil.atoi(@string));
			}
			@string = ((string)v[5]).Trim();
			if ((@string.Length != 0) && StringUtil.isInteger(@string))
			{
				cch.setGdate5(StringUtil.atoi(@string));
			}
			@string = ((string)v[6]).Trim();
			if ((@string.Length != 0) && StringUtil.isInteger(@string))
			{
				cch.setGdates(StringUtil.atoi(@string));
			}
			@string = ((string)v[7]).Trim();
			if ((@string.Length != 0) && StringUtil.isDouble(@string))
			{
				cch.setTmois1(StringUtil.atod(@string));
			}
			@string = ((string)v[8]).Trim();
			if ((@string.Length != 0) && StringUtil.isDouble(@string))
			{
				cch.setTmois2(StringUtil.atod(@string));
			}
			@string = ((string)v[9]).Trim();
			if ((@string.Length != 0) && StringUtil.isDouble(@string))
			{
				cch.setMad(StringUtil.atod(@string));
			}
			@string = ((string)v[10]).Trim();
			if ((@string.Length != 0) && StringUtil.isDouble(@string))
			{
				cch.setIrx(StringUtil.atod(@string));
			}
			@string = ((string)v[11]).Trim();
			if ((@string.Length != 0) && StringUtil.isDouble(@string))
			{
				cch.setFrx(StringUtil.atod(@string));
			}
			@string = ((string)v[12]).Trim();
			if ((@string.Length != 0) && StringUtil.isDouble(@string))
			{
				cch.setAwc(StringUtil.atod(@string));
			}
			@string = ((string)v[13]).Trim();
			if ((@string.Length != 0) && StringUtil.isDouble(@string))
			{
				cch.setApd(StringUtil.atod(@string));
			}
			@string = ((string)v[14]).Trim();
			if ((@string.Length != 0) && StringUtil.isInteger(@string))
			{
				cch.setTflg1(StringUtil.atoi(@string));
			}
			@string = ((string)v[15]).Trim();
			if ((@string.Length != 0) && StringUtil.isInteger(@string))
			{
				cch.setTflg2(StringUtil.atoi(@string));
			}
			@string = ((string)v[16]).Trim();
			if ((@string.Length != 0) && StringUtil.isInteger(@string))
			{
				cch.setCut2(StringUtil.atoi(@string));
			}
			@string = ((string)v[17]).Trim();
			if ((@string.Length != 0) && StringUtil.isInteger(@string))
			{
				cch.setCut3(StringUtil.atoi(@string));
			}

			// add the StateCU_CropCharacteristics to the vector...
			cch_Vector.Add(cch);
		}
		if (@in != null)
		{
			@in.Close();
		}
		return cch_Vector;
	}

	/// <summary>
	/// Cancels any changes made to this object within a GUI since createBackup()
	/// was called and sets _original to null.
	/// </summary>
	public override void restoreOriginal()
	{
		StateCU_CropCharacteristics chars = (StateCU_CropCharacteristics)_original;
		base.restoreOriginal();

		__apd = chars.__apd;
		__awc = chars.__awc;
		__cut2 = chars.__cut2;
		__cut3 = chars.__cut3;
		__frx = chars.__frx;
		__gdate1 = chars.__gdate1;
		__gdate2 = chars.__gdate2;
		__gdate3 = chars.__gdate3;
		__gdate4 = chars.__gdate4;
		__gdate5 = chars.__gdate5;
		__gdates = chars.__gdates;
		__irx = chars.__irx;
		__mad = chars.__mad;
		__tflg1 = chars.__tflg1;
		__tflg2 = chars.__tflg2;
		__tmois1 = chars.__tmois1;
		__tmois2 = chars.__tmois2;

		_isClone = false;
		_original = null;
	}

	/// <summary>
	/// Set the maximum application depth (IN). </summary>
	/// <param name="apd"> maximum application depth (IN). </param>
	public virtual void setApd(double apd)
	{
		__apd = apd;
	}

	/// <summary>
	/// Set the available water holding capacity. </summary>
	/// <param name="awc"> available water holding capacity. </param>
	public virtual void setAwc(double awc)
	{
		__awc = awc;
	}

	/// <summary>
	/// Set the number of days between 1st and 2nd cut (*ALFALFA*). </summary>
	/// <param name="cut2"> number of days between 1st and 2nd cut (*ALFALFA*). </param>
	public virtual void setCut2(int cut2)
	{
		__cut2 = cut2;
	}

	/// <summary>
	/// Set the number of days between 2nd and 3rd cut (*ALFALFA*). </summary>
	/// <param name="cut3"> number of days between 2nd and 3rd cut (*ALFALFA*). </param>
	public virtual void setCut3(int cut3)
	{
		__cut3 = cut3;
	}

	/// <summary>
	/// Set the maximum root zone depth (IN). </summary>
	/// <param name="frx"> maximum root zone depth (IN). </param>
	public virtual void setFrx(double frx)
	{
		__frx = frx;
	}

	/// <summary>
	/// Set the planting month. </summary>
	/// <param name="gdate1"> Planting month (1-12). </param>
	public virtual void setGdate1(int gdate1)
	{
		__gdate1 = gdate1;
	}

	/// <summary>
	/// Set the planting day. </summary>
	/// <param name="gdate2"> Planting day (1-12). </param>
	public virtual void setGdate2(int gdate2)
	{
		__gdate2 = gdate2;
	}

	/// <summary>
	/// Set the harvest month. </summary>
	/// <param name="gdate3"> Harvest month (1-12). </param>
	public virtual void setGdate3(int gdate3)
	{
		__gdate3 = gdate3;
	}

	/// <summary>
	/// Set the harvest day. </summary>
	/// <param name="gdate4"> Harvest day (1-12). </param>
	public virtual void setGdate4(int gdate4)
	{
		__gdate4 = gdate4;
	}

	/// <summary>
	/// Set the days to full cover. </summary>
	/// <param name="gdate5"> Days to full cover. </param>
	public virtual void setGdate5(int gdate5)
	{
		__gdate5 = gdate5;
	}

	/// <summary>
	/// Set the length of season. </summary>
	/// <param name="gdates"> Length of season (days). </param>
	public virtual void setGdates(int gdates)
	{
		__gdates = gdates;
	}

	/// <summary>
	/// Set the initial root zone depth (IN). </summary>
	/// <param name="irx"> initial root zone depth (IN). </param>
	public virtual void setIrx(double irx)
	{
		__irx = irx;
	}

	/// <summary>
	/// Set the management allowable deficit (MAD). </summary>
	/// <param name="mad"> management allowable deficit (MAD). </param>
	public virtual void setMad(double mad)
	{
		__mad = mad;
	}

	/// <summary>
	/// Set the spring frost date flag (0=mean, 1=28F, 2=32F). </summary>
	/// <param name="tflg1"> spring frost date flag (0=mean, 1=28F, 2=32F). </param>
	public virtual void setTflg1(int tflg1)
	{
		__tflg1 = tflg1;
	}

	/// <summary>
	/// Set the fall frost date flag (0=mean, 1=28F, 2=32F). </summary>
	/// <param name="tflg2"> fall frost date flag (0=mean, 1=28F, 2=32F). </param>
	public virtual void setTflg2(int tflg2)
	{
		__tflg2 = tflg2;
	}

	/// <summary>
	/// Set the temperature early moisture. </summary>
	/// <param name="tmois1"> Temperature early moisture (F). </param>
	public virtual void setTmois1(double tmois1)
	{
		__tmois1 = tmois1;
	}

	/// <summary>
	/// Set the temperature late moisture. </summary>
	/// <param name="tmois2"> Temperature late moisture (F). </param>
	public virtual void setTmois2(double tmois2)
	{
		__tmois2 = tmois2;
	}

	/// <summary>
	/// Performs specific data checks and returns a list of data that failed the data checks.
	/// For now don't check for missing data individually - just check for invalid data. </summary>
	/// <param name="dataset"> StateCU dataset currently in memory. </param>
	/// <returns> Validation results. </returns>
	public virtual StateCU_ComponentValidation validateComponent(StateCU_DataSet dataset)
	{
		StateCU_ComponentValidation validation = new StateCU_ComponentValidation();
		string id = getName(); // Name is used for ID because ID used to be numeric
		// Crop number (not used by StateCU - not checked)
		int gdate1 = getGdate1();
		int gdate2 = getGdate2();
		int gdate3 = getGdate3();
		int gdate4 = getGdate4();
		int gdate5 = getGdate5();
		int gdates = getGdates();
		// TODO SAM 2009-05-05 Evaluate whether day check should use month
		if (!TimeUtil.isValidMonth(gdate1))
		{
			validation.add(new StateCU_ComponentValidationProblem(this,"Crop \"" + id + "\" planting month (" + gdate1 + ") is invalid.", "Specify a month 1-12."));
		}
		if (!TimeUtil.isValidDay(gdate2))
		{
			validation.add(new StateCU_ComponentValidationProblem(this,"Crop \"" + id + "\" planting day (" + gdate2 + ") is invalid.", "Specify a day 1-31."));
		}
		if (!TimeUtil.isValidMonth(gdate3))
		{
			validation.add(new StateCU_ComponentValidationProblem(this,"Crop \"" + id + "\" harvest month (" + gdate3 + ") is invalid.", "Specify a month 1-12."));
		}
		if (!TimeUtil.isValidDay(gdate4))
		{
			validation.add(new StateCU_ComponentValidationProblem(this,"Crop \"" + id + "\" planting day (" + gdate4 + ") is invalid.", "Specify a day 1-31."));
		}
		if ((gdate5 < 0) || (gdate5 > 365))
		{
			validation.add(new StateCU_ComponentValidationProblem(this,"Crop \"" + id + "\" days to full cover (" + gdate5 + ") is invalid.", "Specify a day 0 - 365."));
		}
		if ((gdates <= 0) || (gdate5 > 365))
		{
			validation.add(new StateCU_ComponentValidationProblem(this,"Crop \"" + id + "\" days in season (" + gdates + ") is invalid.", "Specify days 1 - 365."));
		}
		double tmois1 = getTmois1();
		double tmois2 = getTmois2();
		// Somewhat arbitrary
		if ((tmois1 < 0) || (tmois1 > 100))
		{
			validation.add(new StateCU_ComponentValidationProblem(this,"Crop \"" + id + "\" temperature early moisture (" + tmois1 + ") is invalid.", "Specify degrees F."));
		}
		if ((tmois2 < 0) || (tmois2 > 100))
		{
			validation.add(new StateCU_ComponentValidationProblem(this,"Crop \"" + id + "\" temperature late moisture (" + tmois2 + ") is invalid.", "Specify degrees F."));
		}
		// Management allowable deficit (not used by StateCU - not checked)
		// Initial root zone depth (not used by StateCU - not checked)
		double frx = getFrx();
		if (frx <= 0)
		{
			validation.add(new StateCU_ComponentValidationProblem(this,"Crop \"" + id + "\" maximum root zone depth (" + frx + ") is invalid.", "Specify inches > 0."));
		}
		// AWC often missing so don't check
		// Application depth
		double apd = getApd();
		// Somewhat arbitrary
		if ((apd < 0) || (apd > 100))
		{
			validation.add(new StateCU_ComponentValidationProblem(this,"Crop \"" + id + "\" maximum application depth (" + apd + ") is invalid.", "Specify inches > 0."));
		}
		int tflg1 = getTflg1();
		int tflg2 = getTflg2();
		if ((tflg1 < 0) || (tflg1 > 2))
		{
			validation.add(new StateCU_ComponentValidationProblem(this,"Crop \"" + id + "\" spring frost flag (" + tflg1 + ") is invalid.", "Specify 0, 1, 2."));
		}
		if ((tflg2 < 0) || (tflg2 > 2))
		{
			validation.add(new StateCU_ComponentValidationProblem(this,"Crop \"" + id + "\" fall frost flag (" + tflg2 + ") is invalid.", "Specify 0, 1, 2."));
		}
		int cut2 = getCut2();
		int cut3 = getCut3();
		if (cut2 > 365)
		{
			validation.add(new StateCU_ComponentValidationProblem(this,"Crop \"" + id + "\" days to 2nd cut (" + cut2 + ") is invalid.", "Specify days < 365."));
		}
		if (cut3 > 365)
		{
			validation.add(new StateCU_ComponentValidationProblem(this,"Crop \"" + id + "\" days to 3rd cut (" + cut3 + ") is invalid.", "Specify days < 365."));
		}
		return validation;
	}

	/// <summary>
	/// Write a list of StateCU_CropCharacteristics to a file.  The filename is
	/// adjusted to the working directory if necessary using IOUtil.getPathUsingWorkingDir() </summary>
	/// <param name="filename_prev"> </param>
	/// <param name="filename"> </param>
	/// <param name="data_Vector"> </param>
	/// <param name="new_comments"> </param>
	/// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeStateCUFile(String filename_prev, String filename, java.util.List<StateCU_CropCharacteristics> data_Vector, java.util.List<String> new_comments) throws java.io.IOException
	public static void writeStateCUFile(string filename_prev, string filename, IList<StateCU_CropCharacteristics> data_Vector, IList<string> new_comments)
	{
		writeStateCUFile(filename_prev, filename, data_Vector, new_comments, null);
	}

	/// <summary>
	/// Write a list of StateCU_CropCharacteristics to a file.  The filename is adjusted to
	/// the working directory if necessary using IOUtil.getPathUsingWorkingDir(). </summary>
	/// <param name="filename_prev"> The name of the previous version of the file (for
	/// processing headers).  Specify as null if no previous file is available. </param>
	/// <param name="filename"> The name of the file to write. </param>
	/// <param name="data_Vector"> A list of StateCU_CropCharacteristics to write. </param>
	/// <param name="new_comments"> Comments to add to the top of the file.  Specify as null 
	/// if no comments are available. </param>
	/// <param name="props"> Properties to control output.
	/// <table width=100% cellpadding=10 cellspacing=0 border=2>
	/// <tr>
	/// <td><b>Property</b></td>        <td><b>Description</b></td>     <td><b>Default</
	/// b></td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>AutoAdjust</b></td>
	/// <td><b>If "true", then if Version 10 format, strip off "." and trailing text from crop names.</b>
	/// <td>None - write current format.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>Version</b></td>
	/// <td><b>If "10", write StateCU Version 10 format file.  Otherwise, write the current format.</b>
	/// <td>None - write current format.</td>
	/// </tr>
	/// </table> </param>
	/// <exception cref="IOException"> if there is an error writing the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeStateCUFile(String filename_prev, String filename, java.util.List<StateCU_CropCharacteristics> data_Vector, java.util.List<String> new_comments, RTi.Util.IO.PropList props) throws java.io.IOException
	public static void writeStateCUFile(string filename_prev, string filename, IList<StateCU_CropCharacteristics> data_Vector, IList<string> new_comments, PropList props)
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
	/// Write a list of StateCU_CropCharacteristics to an opened file. </summary>
	/// <param name="data_Vector"> A list of StateCU_CropCharacteristics to write. </param>
	/// <param name="out"> output PrintWriter. </param>
	/// <param name="props"> Properties to control output (see writeStateCUFile). </param>
	/// <exception cref="IOException"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static void writeVector(java.util.List<StateCU_CropCharacteristics> data_Vector, java.io.PrintWriter out, RTi.Util.IO.PropList props) throws java.io.IOException
	private static void writeVector(IList<StateCU_CropCharacteristics> data_Vector, PrintWriter @out, PropList props)
	{
		int i;
		string iline;
		string rtn = "StateCU_CropCharacterstics.writeVector";
		string cmnt = "#>";
		// For header comments...
		string rec_format = "  Record format (a30,10(i6),4(f6.1),4(i5))";
		// format used to write the file
		string format = "%-30.30s%6.6s%6.6s%6.6s%6.6s%6.6s" +
			"%6.6s%6.6s%6.6s%6.6s%6.6s" +
			"%6.6s%6.6s%6.6s%6.6s%5.5s%5.5s%5.5s%5.5s";
		string crop_num_format = "%6d";
		string date_format = "%6d";
		string date_format2 = "%6d";
		string temp_format = "%6d";
		string float_format = "%6.1f";
		string last_format = "%5d";

		// check proplist for version
		bool version_10 = false; // Default is to use current 11+ version format
		if (props == null)
		{
			props = new PropList("StateCU_CropCharacteristics");
		}
		string Version = props.getValue("Version");
		// set the format to version 10 format
		if ((!string.ReferenceEquals(Version, null)) && Version.Equals("10", StringComparison.OrdinalIgnoreCase))
		{
			version_10 = true;
		}
		string AutoAdjust = props.getValue("AutoAdjust");
		// AutoAdjust output based on version
		bool AutoAdjust_boolean = false;
		if ((!string.ReferenceEquals(AutoAdjust, null)) && AutoAdjust.Equals("true", StringComparison.OrdinalIgnoreCase))
		{
			AutoAdjust_boolean = true;
		}
		if (version_10)
		{
			// format written in the header (record format)
			rec_format = "  Record format " +
			"(a20,2(1x,i2,2x,i2),2x,i2,2x,2(i4,1x)," +
			"3(f3.0,1x),4(f4.1,1x),2(i2,1x),i3,1x,i2)";
			// format for writing the file
			format = "%-20.20s %2.2s  %2.2s %2.2s  %2.2s  %2.2s  " +
			"%4.4s %4.4s %3.3s %3.3s %3.3s " +
			"%4.4s %4.4s %4.4s %4.4s %2.2s %2.2s %3.3s %2.2s";
			date_format = "%2d";
			date_format2 = "%4d";
			temp_format = "%3.0f";
			float_format = "%4.1f";
			last_format = "%2d";
			Message.printStatus(2, rtn, "Writing file in Version 10 format.");
		}

		IList<object> v = new List<object>(19); // Reuse for all output lines.
		@out.println(cmnt);
		@out.println(cmnt + "  StateCU Crop Characteristics (CCH) File");
		@out.println(cmnt);
		@out.println(cmnt + rec_format);
		@out.println(cmnt);
		@out.println(cmnt + "  CropName   cropn:  Crop name (e.g., ALFALFA)");
		@out.println(cmnt + "  CropNum     ckey:  Crop number (not used in StateCU - written as sequential number)");
		@out.println(cmnt + "Plant MM    gdate1:  Planting month (1-12)");
		@out.println(cmnt + "Plant DD    gdate2:  Planting day (e.g., 1-31)");
		@out.println(cmnt + "Harvest MM  gdate3:  Harvest month (1-12)");
		@out.println(cmnt + "Harvest DD  gdate4:  Harvest day (1-31)");
		@out.println(cmnt + "To Full     gdate5:  Days to full cover");
		@out.println(cmnt + "Days Seas   gdates:  Length of season (days)");
		@out.println(cmnt + "Ear Tem     tmois1:  Temperature early moisture (F)");
		@out.println(cmnt + "Lat Tem     tmois2:  Temperature late moisture (F)");
		@out.println(cmnt + "MAD            mad:  Mananagement allowable deficit (not used)");
		@out.println(cmnt + "Init Dep       irx:  Initial root zone depth (in, not used)");
		@out.println(cmnt + "Max Dep        frx:  Maximum root zone depth (in)");
		@out.println(cmnt + "AWC            awc:  Available water holding capacity (in)");
		@out.println(cmnt + "                     OVERWRITTEN IF .par IS SUPPLIED");
		@out.println(cmnt + "App Dep        apd:  Maximum application depth (in)");
		@out.println(cmnt + "Frost Sp     tflg1:  Spring frost date flag");
		@out.println(cmnt + "                     0 = mean, 1 = 28F, 2 = 32F");
		@out.println(cmnt + "Frost Fa     tflg2:  Fall frost date flag");
		@out.println(cmnt + "                     0 = mean, 1 = 28F, 2 = 32F");
		@out.println(cmnt + "Days to 2nd   cut2:  Days between 1st and 2nd cuttings");
		@out.println(cmnt + "                     cropn = *ALFALFA* only");
		@out.println(cmnt + "Days to 3rd   cut3:  Days between 2nd and 3rd cuttings");
		@out.println(cmnt + "                     cropn = *ALFALFA* only");
		@out.println(cmnt);

		if (version_10)
		{
			@out.println(cmnt + "                       Plant  Harvest To   Days Ear Lat     Init Max       App  Frost Days to");
			@out.println(cmnt + "     CropName          MM DD  MM  DD  Full Seas Tem Tem MAD Dep  Dep  AWC  Dep  Sp Fa 2nd 3rd");
			@out.println(cmnt + "-----------------exbexxbexbexxbexxbexxb--exb--exb-exb-exb-exb--exb--exb--exb--exbexbexb-exbe");
		}
		else
		{
			@out.println(cmnt + "                             Crop     Plant      Harvest    To   Days   Ear   Lat         Init  Max         App    Frost   Days to");
			@out.println(cmnt + "            CropName         Num    MM    DD    MM    DD   Full  Seas   Tem   Tem   MAD   Dep   Dep   AWC   Dep   Sp   Fa  2nd  3rd");
			@out.println(cmnt + "---------------------------eb----eb----eb----eb----eb----eb----eb----eb----eb----eb----eb----eb----eb----eb----eb---eb---eb---eb---e");
		}
		@out.println(cmnt + "EndHeader");

		int num = 0;
		if (data_Vector != null)
		{
			num = data_Vector.Count;
		}
		StateCU_CropCharacteristics cch = null;
		string name = null; // Crop name
		for (i = 0; i < num; i++)
		{
			cch = data_Vector[i];
			if (cch == null)
			{
				continue;
			}

			v.Clear();
			int pos = 0; // Position of object
			name = cch.getName();
			if (version_10 && AutoAdjust_boolean)
			{
				pos = name.IndexOf(".", StringComparison.Ordinal);
				if (pos > 0)
				{
					name = name.Substring(0,pos);
				}
			}
			v.Add(name);
			// Crop number is not used by model
			// Write as sequential integer to facilitate free-format reading.
			v.Add(StringUtil.formatString((i + 1), crop_num_format));

			// Planting...
			if (StateCU_Util.isMissing(cch.__gdate1))
			{
				v.Add("-999");
			}
			else
			{
				v.Add(StringUtil.formatString(cch.__gdate1, date_format));
			}
			if (StateCU_Util.isMissing(cch.__gdate2))
			{
				v.Add("-999");
			}
			else
			{
				v.Add(StringUtil.formatString(cch.__gdate2, date_format));
			}
			// Harvest...
			if (StateCU_Util.isMissing(cch.__gdate3))
			{
				v.Add("-999");
			}
			else
			{
				v.Add(StringUtil.formatString(cch.__gdate3, date_format));
			}
			if (StateCU_Util.isMissing(cch.__gdate4))
			{
				v.Add("-999");
			}
			else
			{
				v.Add(StringUtil.formatString(cch.__gdate4, date_format));
			}
			// Days to cover...
			if (StateCU_Util.isMissing(cch.__gdate5))
			{
				v.Add("-999");
			}
			else
			{
				v.Add(StringUtil.formatString(cch.__gdate5, date_format2));
			}
			// Length of season...
			if (StateCU_Util.isMissing(cch.__gdates))
			{
				v.Add("-999");
			}
			else
			{
				v.Add(StringUtil.formatString(cch.__gdates, date_format2));
			}
			// Moisture...
			if (StateCU_Util.isMissing(cch.__tmois1))
			{
				v.Add("-999");
			}
			else
			{
				v.Add(StringUtil.formatString(cch.__tmois1, temp_format));
			}
			if (StateCU_Util.isMissing(cch.__tmois2))
			{
				v.Add("-999");
			}
			else
			{
				v.Add(StringUtil.formatString(cch.__tmois2, temp_format));
			}
			// MAD...
			if (StateCU_Util.isMissing(cch.__mad))
			{
				v.Add("-999");
			}
			else
			{
				v.Add(StringUtil.formatString(cch.__mad, temp_format));
			}
			// Root depths...
			if (StateCU_Util.isMissing(cch.__irx))
			{
				v.Add("-999");
			}
			else
			{
				v.Add(StringUtil.formatString(cch.__irx, float_format));
			}
			if (StateCU_Util.isMissing(cch.__frx))
			{
				v.Add("-999");
			}
			else
			{
				v.Add(StringUtil.formatString(cch.__frx, float_format));
			}
			if (StateCU_Util.isMissing(cch.__awc))
			{
				v.Add("-999");
			}
			else
			{
				v.Add(StringUtil.formatString(cch.__awc, float_format));
			}
			if (StateCU_Util.isMissing(cch.__apd))
			{
				v.Add("-999");
			}
			else
			{
				v.Add(StringUtil.formatString(cch.__apd, float_format));
			}
			// Frost date flags (check for -90 because the original data
			// may be read in as -99 because it does not strictly comply with formatting)...
			if (StateCU_Util.isMissing(cch.__tflg1) || (cch.__tflg1 < -90.0))
			{
				v.Add("-999");
			}
			else
			{
				v.Add(StringUtil.formatString(cch.__tflg1, last_format));
			}
			if (StateCU_Util.isMissing(cch.__tflg2) || (cch.__tflg2 < -90.0))
			{
				v.Add("-999");
			}
			else
			{
				v.Add(StringUtil.formatString(cch.__tflg2, last_format));
			}
			// Additional cuttings (ALFALFA only)...
			if (StateCU_Util.isMissing(cch.__cut2) || (StringUtil.indexOfIgnoreCase(cch._name,"ALFALFA",0) < 0))
			{
				v.Add("");
			}
			else
			{
				v.Add(StringUtil.formatString(cch.__cut2, last_format));
			}
			if (StateCU_Util.isMissing(cch.__cut3) || (StringUtil.indexOfIgnoreCase(cch._name,"ALFALFA",0) < 0))
			{
				v.Add("");
			}
			else
			{
				v.Add(StringUtil.formatString(cch.__cut3, last_format));
			}
			iline = StringUtil.formatString(v, format);
			@out.println(iline);
		}
	}

	/// <summary>
	/// Writes a list of StateCU_CropCharacteristics objects to a list file.  A header
	/// is printed to the top of the file, containing the commands used to generate the 
	/// file.  Any strings in the body of the file that contain the field delimiter will be wrapped in "...". </summary>
	/// <param name="filename"> the name of the file to which the data will be written. </param>
	/// <param name="delimiter"> the delimiter to use for separating field values. </param>
	/// <param name="update"> whether to update an existing file, retaining the current 
	/// header (true) or to create a new file with a new header. </param>
	/// <param name="data"> the list of objects to write. </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeListFile(String filename, String delimiter, boolean update, java.util.List<StateCU_CropCharacteristics> data, java.util.List<String> outputComments) throws Exception
	public static void writeListFile(string filename, string delimiter, bool update, IList<StateCU_CropCharacteristics> data, IList<string> outputComments)
	{
		string routine = "StateCU_CropCharacteristics.writeListFile";
		int size = 0;
		if (data != null)
		{
			size = data.Count;
		}

		IList<string> fields = new List<string>();
		fields.Add("Name");
		fields.Add("PlantingMonth");
		fields.Add("PlantingDay");
		fields.Add("HarvestMonth");
		fields.Add("HarvestDay");
		fields.Add("DaysToCover");
		fields.Add("SeasonLength");
		fields.Add("EarlyMoisture");
		fields.Add("LateMoisture");
		fields.Add("DeficitLevel");
		fields.Add("InitialRootZone");
		fields.Add("MaxRootZone");
		fields.Add("AWC");
		fields.Add("MAD");
		fields.Add("SpringFrost");
		fields.Add("FallFrost");
		fields.Add("DaysBetween1And2");
		fields.Add("DaysBetween2And3");
		int fieldCount = fields.Count;

		string[] names = new string[fieldCount];
		string[] formats = new string[fieldCount];
		int comp = StateCU_DataSet.COMP_CROP_CHARACTERISTICS;
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
		StateCU_CropCharacteristics cc = null;
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
			newComments2.Insert(1,"StateCU crop characteristics as a delimited list file.");
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
				cc = data[i];

				line[0] = StringUtil.formatString(cc.getName(),formats[0]).Trim();
				line[1] = StringUtil.formatString(cc.getGdate1(),formats[1]).Trim();
				line[2] = StringUtil.formatString(cc.getGdate2(),formats[2]).Trim();
				line[3] = StringUtil.formatString(cc.getGdate3(),formats[3]).Trim();
				line[4] = StringUtil.formatString(cc.getGdate4(),formats[4]).Trim();
				line[5] = StringUtil.formatString(cc.getGdate5(),formats[5]).Trim();
				line[6] = StringUtil.formatString(cc.getGdates(),formats[6]).Trim();
				line[7] = StringUtil.formatString(cc.getTmois1(),formats[7]).Trim();
				line[8] = StringUtil.formatString(cc.getTmois2(),formats[8]).Trim();
				line[9] = StringUtil.formatString(cc.getMad(),formats[9]).Trim();
				line[10] = StringUtil.formatString(cc.getIrx(),formats[10]).Trim();
				line[11] = StringUtil.formatString(cc.getFrx(),formats[11]).Trim();
				line[12] = StringUtil.formatString(cc.getAwc(),formats[12]).Trim();
				line[13] = StringUtil.formatString(cc.getApd(),formats[13]).Trim();
				line[14] = StringUtil.formatString(cc.getTflg1(),formats[14]).Trim();
				line[15] = StringUtil.formatString(cc.getTflg2(),formats[15]).Trim();
				line[16] = StringUtil.formatString(cc.getCut2(),formats[16]).Trim();
				line[17] = StringUtil.formatString(cc.getCut3(),formats[17]).Trim();

				buffer = new StringBuilder();
				for (j = 0; j < fieldCount; j++)
				{
					if (j > 0)
					{
						buffer.Append(delimiter);
					}
					if (line[j].IndexOf(delimiter, StringComparison.Ordinal) > -1)
					{
						line[j] = "\"" + line[j] + "\"";
					}
					buffer.Append(line[j]);
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