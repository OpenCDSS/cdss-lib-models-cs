using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

// StateMod_InstreamFlowRight - class to store instream flow right

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
// StateMod_InstreamFlowRight - Derived from SMData class
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
// 
// 08 Sep 1997	Catherine E.		Created initial version of class.
//		Nutting-Lane, RTi
// 21 Dec 1998	CEN, RTi		Added throws IOException to
//					read/write routines.
// 15 Feb 2001	Steven A. Malers, RTi	Update header for current documentation.
//					Update IO to IOUtil.  Add finalize()and
//					make sure all data are initialized.  Add
//					more Javadoc.  Alphabetize methods.  Add
//					checks for null data to prevent errors.
//					Set unused variables to null.
// 02 Mar 2001	SAM, RTi		Ray says to use F16.0 for rights and
//					get rid of the 4x.
// 2001-12-27	SAM, RTi		Update to use new fixedRead()to
//					improve performance.
// 2002-09-19	SAM, RTi		Use isDirty()instead of setDirty()to
//					indicate edits.
//------------------------------------------------------------------------------
// 2003-06-04	J. Thomas Sapienza, RTI	Renamed from StateMod_InstreamFlowRight 
//					to StateMod_InstreamFlowRight
// 2003-06-10	JTS, RTi		* Folded dumpInstreaFlowRightsFile()
//					  into writeInstreamFlowRightsFile()
//					* Renamed parseInstreamFlowRightsFile()
//					  to readInstreamFlowRightsFile()
//					* Renamed writeInstreamFlowRightsFile()
//					  to writeStateModFile()
// 2003-06-26	JTS, RTi		Renamed readInstreamFlowRightsFile()
//					readStateModFile()
// 2003-08-03	SAM, RTi		Change isDirty() back to setDirty().
// 2003-08-28	SAM, RTi		* Do not use linked list since
//					  StateMod_InstreamFlow has a Vector of
//					  rights.
//					* Call setDirty() on individual objects
//					  in addition to the data set component.
//					* Clean up Javadoc and parameter names.
// 2003-10-13	JTS, RTi		* Implemented Cloneable.
//					* Added clone().
//					* Added equals().
//					* Implemented Comparable.
//					* Added compareTo().
// 					* Added equals(Vector, Vector)
// 2003-10-15	JTS, RTi		* Revised the clone() code.
//					* Added toString().
// 2004-07-08	SAM, RTi		* When writing, adjust paths using
//					  working directory.
//					* Overload the constructor to allow
//					  initializing to default values or
//					  missing.
// 2005-03-13	SAM, RTi		* Expand header to explain switch
//					  better.
// 2005-03-31	JTS, RTi		Added createBackup().
// 2005-04-18	JTS, RTi		Added writeListFile().
// 2007-04-12	Kurt Tometich, RTi		Added checkComponentData() and
//									getDataHeader() methods for check
//									file and data check support.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
// 2007-05-16	SAM, RTi		Implement StateMod_Right interface.
//------------------------------------------------------------------------------
// EndHeader

namespace DWR.StateMod
{

	using DataSetComponent = RTi.Util.IO.DataSetComponent;
	using IOUtil = RTi.Util.IO.IOUtil;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// This StateMod_InstreamFlowRight class holds information for StateMod instream flow rights.
	/// </summary>
	public class StateMod_InstreamFlowRight : StateMod_Data, ICloneable, IComparable<StateMod_Data>, StateMod_ComponentValidator, StateMod_Right
	{

	/// <summary>
	/// Administration number.  The value is stored as a string to allow exact
	/// representation of the administration number, without any roundoff or precision issues.
	/// </summary>
	protected internal string _irtem;

	/// <summary>
	/// Decreed amount
	/// </summary>
	protected internal double _dcrifr;

	/// <summary>
	/// Construct and initialize to default values.
	/// </summary>
	public StateMod_InstreamFlowRight() : this(true)
	{
	}

	/// <summary>
	/// Constructor. </summary>
	/// <param name="initialize_defaults"> If true, initialize to default values, suitable for
	/// creating instances in the StateMod GUI.  If false, initialize to missing,
	/// suitable for use with StateDMI. </param>
	public StateMod_InstreamFlowRight(bool initialize_defaults) : base()
	{
		initialize(initialize_defaults);
	}

	/// <summary>
	/// Clones the data object. </summary>
	/// <returns> a cloned object. </returns>
	public override object clone()
	{
		StateMod_InstreamFlowRight right = (StateMod_InstreamFlowRight)base.clone();
		right._isClone = true;
		return right;
	}

	/// <summary>
	/// Creates a copy of the object for later use in checking to see if it was changed in a GUI.
	/// </summary>
	public virtual void createBackup()
	{
		_original = (StateMod_InstreamFlowRight)clone();
		((StateMod_InstreamFlowRight)_original)._isClone = false;
		_isClone = true;
	}

	/// <summary>
	/// Compares this object to another StateMod_Data object based on the sorted
	/// order from the StateMod_Data variables, and then by irtem and dcrifr, in that order. </summary>
	/// <param name="data"> the object to compare against. </param>
	/// <returns> 0 if they are the same, 1 if this object is greater than the other object, or -1 if it is less. </returns>
	public virtual int CompareTo(StateMod_Data data)
	{
		int res = base.CompareTo(data);
		if (res != 0)
		{
			return res;
		}

		StateMod_InstreamFlowRight right = (StateMod_InstreamFlowRight)data;

		res = _irtem.CompareTo(right.getIrtem());
		if (res == 0)
		{
			double dcrifr = right.getDcrifr();
			if (dcrifr == _dcrifr)
			{
				return 0;
			}
			else if (_dcrifr < dcrifr)
			{
				return -1;
			}
			else
			{
				return 1;
			}
		}
		else
		{
			return res;
		}
	}

	/// <summary>
	/// Compare two rights lists and see if they are the same. </summary>
	/// <param name="v1"> the first list of StateMod_InstreamFlowRights to check.  Cannot be null. </param>
	/// <param name="v2"> the second list of StateMod_InstreamFlowRights to check.  Cannot be null. </param>
	/// <returns> true if they are the same, false if not. </returns>
	public static bool Equals(IList<StateMod_InstreamFlowRight> v1, IList<StateMod_InstreamFlowRight> v2)
	{
		string routine = "StateMod_InstreamFlowRight.equals(Vector, Vector)";
		StateMod_InstreamFlowRight r1;
		StateMod_InstreamFlowRight r2;
		if (v1.Count != v2.Count)
		{
			Message.printStatus(1, routine, "Lists are different sizes");
			return false;
		}
		else
		{
			// sort the Vectors and compare item-by-item.  Any differences
			// and data will need to be saved back into the dataset.
			int size = v1.Count;
			Message.printStatus(2, routine, "Lists are of size: " + size);
			IList<StateMod_InstreamFlowRight> v1Sort = StateMod_Util.sortStateMod_DataVector(v1);
			IList<StateMod_InstreamFlowRight> v2Sort = StateMod_Util.sortStateMod_DataVector(v2);
			Message.printStatus(2, routine, "Lists have been sorted");

			for (int i = 0; i < size; i++)
			{
				r1 = v1Sort[i];
				r2 = v2Sort[i];
				Message.printStatus(1, routine, r1.ToString());
				Message.printStatus(1, routine, r2.ToString());
				Message.printStatus(1, routine, "Element " + i + " comparison: " + r1.CompareTo(r2));
				if (r1.CompareTo(r2) != 0)
				{
					return false;
				}
			}
		}
		return true;
	}

	/// <summary>
	/// Tests to see if two instream flow rights are equal.  Strings are compared with case sensitivity. </summary>
	/// <param name="right"> the right to compare. </param>
	/// <returns> true if they are equal, false otherwise. </returns>
	public virtual bool Equals(StateMod_InstreamFlowRight right)
	{
		if (!base.Equals(right))
		{
			 return false;
		}
		if (right._irtem.Equals(_irtem) && right._dcrifr == _dcrifr)
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Free memory before garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~StateMod_InstreamFlowRight()
	{
		_irtem = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Return the administration number, as per the generic interface. </summary>
	/// <returns> the administration number, as a String to protect from roundoff. </returns>
	public virtual string getAdministrationNumber()
	{
		return getIrtem();
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
	/// Return the decreed amount.
	/// </summary>
	public virtual double getDcrifr()
	{
		return _dcrifr;
	}

	/// <summary>
	/// Return the decree, as per the generic interface. </summary>
	/// <returns> the decree, in the units of the data. </returns>
	public virtual double getDecree()
	{
		return getDcrifr();
	}

	// TODO SAM 2007-05-15 Need to evaluate whether should be hard-coded.
	/// <summary>
	/// Return the decree units. </summary>
	/// <returns> the decree units. </returns>
	public virtual string getDecreeUnits()
	{
		return "CFS";
	}

	/// <summary>
	/// Return the right identifier, as per the generic interface. </summary>
	/// <returns> the right identifier. </returns>
	public virtual string getIdentifier()
	{
		return getID();
	}

	/// <summary>
	/// Retrieve the administration number.
	/// </summary>
	public virtual string getIrtem()
	{
		return _irtem;
	}

	/// <summary>
	/// Return the right location identifier, as per the generic interface. </summary>
	/// <returns> the right location identifier (location where right applies). </returns>
	public virtual string getLocationIdentifier()
	{
		return getCgoto();
	}

	/// <summary>
	/// Initialize data. </summary>
	/// <param name="initialize_defaults"> If true, initialize to default values, suitable for
	/// creating instances in the StateMod GUI.  If false, initialize to missing, suitable for use with StateDMI. </param>
	private void initialize(bool initialize_defaults)
	{
		_smdata_type = StateMod_DataSet.COMP_INSTREAM_RIGHTS;
		_irtem = "";
		if (initialize_defaults)
		{
			_dcrifr = 0;
		}
		else
		{
			_dcrifr = StateMod_Util.MISSING_DOUBLE;
		}
	}

	/// <summary>
	/// Determine whether a file is an instream flow right file.  Currently true is returned
	/// if the file extension is ".ifr". </summary>
	/// <param name="filename"> Name of the file being checked. </param>
	/// <returns> true if the file is a StateMod instream flow right file. </returns>
	public static bool isInstreamFlowRightFile(string filename)
	{
		if (filename.ToUpper().EndsWith(".IFR", StringComparison.Ordinal))
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Read instream flow rights information in and store in a list. </summary>
	/// <param name="filename"> Name of file to read. </param>
	/// <exception cref="Exception"> if there is an error reading the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.List<StateMod_InstreamFlowRight> readStateModFile(String filename) throws Exception
	public static IList<StateMod_InstreamFlowRight> readStateModFile(string filename)
	{
		string routine = "StateMod_InstreamFlowRight.readStateModFile";
		IList<StateMod_InstreamFlowRight> theInsfRights = new List<StateMod_InstreamFlowRight>();
		int[] format_0 = new int[] {StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_DOUBLE, StringUtil.TYPE_INTEGER};
		int[] format_0w = new int[] {12, 24, 12, 16, 8, 8};
		string iline;
		StateMod_InstreamFlowRight aRight = null;
		IList<object> v = new List<object>(6);

		Message.printStatus(1, routine, "Reading Instream Flow Rights File: " + filename);
		StreamReader @in = null;
		try
		{
			@in = new StreamReader(filename);
			while (!string.ReferenceEquals((iline = @in.ReadLine()), null))
			{
				// check for comments
				if (iline.StartsWith("#", StringComparison.Ordinal) || iline.Trim().Length == 0)
				{
					continue;
				}

				aRight = new StateMod_InstreamFlowRight();

				StringUtil.fixedRead(iline, format_0, format_0w, v);
				aRight.setID(((string)v[0]).Trim());
				aRight.setName(((string)v[1]).Trim());
				aRight.setCgoto(((string)v[2]).Trim());
				aRight.setIrtem(((string)v[3]).Trim());
				aRight.setDcrifr((double?)v[4]);
				aRight.setSwitch((int?)v[5]);

				theInsfRights.Add(aRight);
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
		}
		return theInsfRights;
	}

	/// <summary>
	/// Set the decreed amount.
	/// </summary>
	public virtual void setDcrifr(double dcrifr)
	{
		if (dcrifr != _dcrifr)
		{
			_dcrifr = dcrifr;
			setDirty(true);
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_INSTREAM_RIGHTS, true);
			}
		}
	}

	/// <summary>
	/// Set the decreed amount.
	/// </summary>
	public virtual void setDcrifr(double? dcrifr)
	{
		setDcrifr(dcrifr.Value);
	}

	/// <summary>
	/// Set the decreed amount.
	/// </summary>
	public virtual void setDcrifr(string dcrifr)
	{
		if (string.ReferenceEquals(dcrifr, null))
		{
			return;
		}
		setDcrifr(StringUtil.atod(dcrifr.Trim()));
	}

	/// <summary>
	/// Set the decree, as per the generic interface. </summary>
	/// <param name="decree">, in the units of the data. </param>
	public virtual void setDecree(double decree)
	{
		setDcrifr(decree);
	}

	/// <summary>
	/// Set the administration number.
	/// </summary>
	public virtual void setIrtem(string irtem)
	{
		if (string.ReferenceEquals(irtem, null))
		{
			return;
		}
		if (!irtem.Equals(_irtem))
		{
			_irtem = irtem.Trim();
			setDirty(true);
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_INSTREAM_RIGHTS, true);
			}
		}
	}


	/// <summary>
	/// Returns a String representation of this object. </summary>
	/// <returns> a String representation of this object. </returns>
	public override string ToString()
	{
		return base.ToString() + ", " + _irtem + ", " + _dcrifr;
	}

	/// <param name="dataset"> StateMod dataset object. </param>
	/// <returns> validation results. </returns>
	public virtual StateMod_ComponentValidation validateComponent(StateMod_DataSet dataset)
	{
		StateMod_ComponentValidation validation = new StateMod_ComponentValidation();
		string id = getID();
		string name = getName();
		string cgoto = getCgoto();
		string irtem = getIrtem();
		double dcrifr = getDcrifr();
		// Make sure that basic information is not empty
		if (StateMod_Util.isMissing(id))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Instream flow right identifier is blank.", "Specify a instream flow right identifier."));
		}
		if (StateMod_Util.isMissing(name))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Instream flow right \"" + id + "\" name is blank.", "Specify an instream flow right name to clarify data."));
		}
		if (StateMod_Util.isMissing(cgoto))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Instream flow right \"" + id + "\" station ID is blank.", "Specify an instream flow station to associate with the instream flow right."));
		}
		else
		{
			// Verify that the instream flow station is in the data set, if the network is available
			DataSetComponent comp2 = dataset.getComponentForComponentType(StateMod_DataSet.COMP_INSTREAM_STATIONS);
			System.Collections.IList ifsList = (System.Collections.IList)comp2.getData();
			if ((ifsList != null) && (ifsList.Count > 0))
			{
				if (StateMod_Util.IndexOf(ifsList, cgoto) < 0)
				{
					validation.add(new StateMod_ComponentValidationProblem(this,"Instream flow right \"" + id + "\" associated instream flow (" + cgoto + ") is not found in the list of instream flow stations.", "Specify a valid instream flow station ID to associate with the instream flow right."));
				}
			}
		}
		if (StateMod_Util.isMissing(irtem))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Instream flow right \"" + id + "\" administration number is blank.", "Specify an administration number NNNNN.NNNNN."));
		}
		else if (!StringUtil.isDouble(irtem))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Instream flow right \"" + id + "\" administration number (" + irtem + ") is invalid.", "Specify an administration number NNNNN.NNNNN."));
		}
		else
		{
			double irtemd = double.Parse(irtem);
			if (irtemd < 0)
			{
				validation.add(new StateMod_ComponentValidationProblem(this,"Instream flow right \"" + id + "\" administration number (" + irtem + ") is invalid.", "Specify an administration number NNNNN.NNNNN."));
			}
		}
		if (!(dcrifr >= 0.0))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Instream flow right \"" + id + "\" decree (" + StringUtil.formatString(dcrifr,"%.2f") + ") is invalid.", "Specify the decree as a number >= 0."));
		}
		return validation;
	}

	/// <summary>
	/// Print instream flow rights information to output.  History header information 
	/// is also maintained by calling this routine. </summary>
	/// <param name="infile"> input file from which previous history should be taken </param>
	/// <param name="outfile"> output file to which to write </param>
	/// <param name="theInsfRights"> list of instream flow rights to print </param>
	/// <param name="newComments"> addition comments which should be included in history </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeStateModFile(String infile, String outfile, java.util.List<StateMod_InstreamFlowRight> theInsfRights, java.util.List<String> newComments) throws Exception
	public static void writeStateModFile(string infile, string outfile, IList<StateMod_InstreamFlowRight> theInsfRights, IList<string> newComments)
	{
		writeStateModFile(infile, outfile, theInsfRights, newComments, false);
	}

	/// <summary>
	/// Print instream flow rights information to output.  History header information 
	/// is also maintained by calling this routine. </summary>
	/// <param name="infile"> input file from which previous history should be taken </param>
	/// <param name="outfile"> output file to which to write </param>
	/// <param name="theInsfRights"> list of instream flow rights to print </param>
	/// <param name="newComments"> addition comments which should be included in history </param>
	/// <param name="oldAdminNumFormat"> whether to use the old admin num format or not </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeStateModFile(String infile, String outfile, java.util.List<StateMod_InstreamFlowRight> theInsfRights, java.util.List<String> newComments, boolean oldAdminNumFormat) throws Exception
	public static void writeStateModFile(string infile, string outfile, IList<StateMod_InstreamFlowRight> theInsfRights, IList<string> newComments, bool oldAdminNumFormat)
	{
		IList<string> commentIndicators = new List<string>(1);
		commentIndicators.Add("#");
		IList<string> ignoredCommentIndicators = new List<string>(1);
		ignoredCommentIndicators.Add("#>");
		PrintWriter @out = null;
		string routine = "StateMod_InstreamFlowRight.writeStateModFile";
		if (Message.isDebugOn)
		{
			Message.printDebug(2, routine, "Write instream flow rights to " + outfile);
		}

		try
		{
			@out = IOUtil.processFileHeaders(IOUtil.getPathUsingWorkingDir(infile), IOUtil.getPathUsingWorkingDir(outfile), newComments, commentIndicators, ignoredCommentIndicators, 0);

			string iline;
			string cmnt = "#>";
			StateMod_InstreamFlowRight right;
			IList<object> v = new List<object>(6);
			string format_0 = null;
			if (oldAdminNumFormat)
			{
				format_0 = "%-12.12s%-24.24s%-12.12s    %-12.12s%8.2F%8d";
			}
			else
			{
				format_0 = "%-12.12s%-24.24s%-12.12s%16.16s%8.2F%8d";
			}

			@out.println(cmnt);
			@out.println(cmnt + " *******************************************************");
			@out.println(cmnt + "  StateMod Instream Flow Right file ");
			@out.println(cmnt);
			@out.println(cmnt + "       format:  (a12, a24, a12, F16.5, f8.2, i8)");
			@out.println(cmnt);
			@out.println(cmnt + "  ID         cifrri:      Instream flow right ID");
			@out.println(cmnt + "  Name        namei:      Instream flow right name");
			@out.println(cmnt + "  Structure   cgoto:      Instream flow station associated with the right");
			@out.println(cmnt + "  Admin#      irtem:      Priority or Administration number");
			@out.println(cmnt + "                          (small is senior).");
			@out.println(cmnt + "  Decree     dcrifr:      Decreed amount (cfs)");
			@out.println(cmnt + "  On/Off     iifrsw:      Switch 0 = off, 1 = on");
			@out.println(cmnt + "                          YYYY = on for years >= YYYY");
			@out.println(cmnt + "                          -YYYY = off for years > YYYY");
			@out.println(cmnt);
			@out.println(cmnt + "   ID           Name               Structure        Admin#     Decree On/Off");
			@out.println(cmnt + "---------eb----------------------eb----------exxxxb----------eb------eb------e");
			@out.println(cmnt + "EndHeader");
			@out.println(cmnt);

			int num = 0;
			if (theInsfRights != null)
			{
				num = theInsfRights.Count;
			}
			for (int i = 0; i < num; i++)
			{
				right = (StateMod_InstreamFlowRight)theInsfRights[i];
				if (right == null)
				{
					continue;
				}
				v.Clear();
				v.Add(right.getID());
				v.Add(right.getName());
				v.Add(right.getCgoto());
				v.Add(right.getIrtem());
				v.Add(new double?(right.getDcrifr()));
				v.Add(new int?(right.getSwitch()));
				iline = StringUtil.formatString(v, format_0);
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

	/// <summary>
	/// Writes a list of StateMod_InstreamFlowRight objects to a list file.  A header 
	/// is printed to the top of the file, containing the commands used to generate the 
	/// file.  Any strings in the body of the file that contain the field delimiter will be wrapped in "...". </summary>
	/// <param name="filename"> the name of the file to which the data will be written. </param>
	/// <param name="delimiter"> the delimiter to use for separating field values. </param>
	/// <param name="update"> whether to update an existing file, retaining the current 
	/// header (true) or to create a new file with a new header. </param>
	/// <param name="data"> the list of objects to write. </param>
	/// <param name="newComments"> comments to add to the the file header. </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeListFile(String filename, String delimiter, boolean update, java.util.List<StateMod_InstreamFlowRight> data, java.util.List<String> newComments) throws Exception
	public static void writeListFile(string filename, string delimiter, bool update, IList<StateMod_InstreamFlowRight> data, IList<string> newComments)
	{
		string routine = "StateMod_IntreamFlowRight.writeListFile";
		int size = 0;
		if (data != null)
		{
			size = data.Count;
		}

		IList<string> fields = new List<string>();
		fields.Add("ID");
		fields.Add("Name");
		fields.Add("StationID");
		fields.Add("AdministrationNumber");
		fields.Add("Decree");
		fields.Add("OnOff");
		int fieldCount = fields.Count;

		string[] names = new string[fieldCount];
		string[] formats = new string[fieldCount];
		int comp = StateMod_DataSet.COMP_INSTREAM_RIGHTS;
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
		StateMod_InstreamFlowRight right = null;
		IList<string> commentIndicators = new List<string>(1);
		commentIndicators.Add("#");
		IList<string> ignoredCommentIndicators = new List<string>(1);
		ignoredCommentIndicators.Add("#>");
		string[] line = new string[fieldCount];
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
			newComments2.Insert(1,"StateMod instream flow rights as a delimited list file.");
			newComments2.Insert(2,"");
			@out = IOUtil.processFileHeaders(oldFile, IOUtil.getPathUsingWorkingDir(filename), newComments2, commentIndicators, ignoredCommentIndicators, 0);

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
				right = data[i];

				line[0] = StringUtil.formatString(right.getID(),formats[0]).Trim();
				line[1] = StringUtil.formatString(right.getName(),formats[1]).Trim();
				line[2] = StringUtil.formatString(right.getCgoto(),formats[2]).Trim();
				line[3] = StringUtil.formatString(right.getIrtem(),formats[3]).Trim();
				line[4] = StringUtil.formatString(right.getDcrifr(),formats[4]).Trim();
				line[5] = StringUtil.formatString(right.getSwitch(),formats[5]).Trim();

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