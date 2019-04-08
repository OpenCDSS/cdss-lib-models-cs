using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

// StateMod_DiversionRight - class to store diversion right

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
// StateMod_DiversionRight - Derived from SMData class
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
// 
// 27 Aug 1997	Catherine E.		Created initial version of class
//		Nutting-Lane, RTi
// 11 Feb 1998	Catherine E.		Added SMFileData.setDirty to all set
//		Nutting-Lane, RTi	routines.  Added throws IOException to 
//					read/write routines
// 16 Feb 2001	Steven A. Malers, RTi	Update output header to be consistent
//					with new documentation.  Add finalize().
//					Alphabetize methods.  Set unused
//					variables to null.  Handle null
//					arguments.  Change IO to IOUtil.  Get
//					rid of low-level debugs that are not
//					needed.
// 02 Mar 2001	SAM, RTi		Ray says to use F16.0 for rights and
//					get rid of the 4x.
// 2001-12-27	SAM, RTi		Update to use new fixedRead()to
//					improve performance.
// 2002-09-19	SAM, RTi		Use isDirty()instead of setDirty()to
//					indicate edits.
//------------------------------------------------------------------------------
// 2003-06-04	J. Thomas Sapienza, RTi	Renamed from SMDivRights to 
//					StateMod_DiversionRight
// 2003-06-10	JTS, RTi		* Folded dumpDiversionRightsFile() into
//					  writeDiversionRightsFile()
//					* Renamed parseDiversionRightsFile() to
//					  readDiversionRightsFile()
// 2003-06-23	JTS, RTi		Renamed writeDiversionRightsFile() to
//					writeStateModFile()
// 2003-06-26	JTS, RTi		Renamed readDiversionRightsFile() to
//					readStateModFile()
// 2003-07-07	SAM, RTi		Check for null data set to allow the
//					code to be used outside of a full
//					StateMod data set implementation.
// 2003-07-15	JTS, RTi		Changed code to use new dataset design.
// 2003-08-03	SAM, RTi		Changed isDirty() back to setDirty().
// 2003-08-27	SAM, RTi		Change default value of irtem to
//					99999.
// 2003-08-28	SAM, RTi		* Remove linked list logic since a
//					  Vector of rights is now maintained in
//					  StateMod_Diversion.
//					* Call setDirty() on the individual
//					  objects as well as the component.
//					* Clean up Javadoc for parameters to
//					  make more readable.
// 2003-10-09	JTS, RTi		* Implemented Cloneable.
//					* Added clone().
//					* Added equals().
//					* Implemented Comparable.
//					* Added compareTo().
// 2003-10-10	JTS, RTI		Added equals(Vector, Vector)
// 2003-10-14	JTS, RTi		* Make sure diversion right is marked
//					  not dirty after initial read and
//					  construction.
// 2003-10-15	JTS, RTi		Revised the clone() code.
// 2004-10-28	SAM, RTi		Add getIdvrswChoices() and
//					getIdvrswDefault().
// 2005-01-13	JTS, RTi		* Added createBackup().
// 					* Added restoreOriginal().
// 2005-03-13	SAM, RTi		* Clean up output header information for
//					  switch.
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

	public class StateMod_DiversionRight : StateMod_Data, ICloneable, IComparable<StateMod_Data>, StateMod_ComponentValidator, StateMod_Right
	{

	/// <summary>
	/// Administration number.
	/// </summary>
	protected internal string _irtem;

	/// <summary>
	/// Decreed amount
	/// </summary>
	protected internal double _dcrdiv;

	// ID, Name, and Cgoto are in the base class.

	/// <summary>
	/// Constructor.
	/// </summary>
	public StateMod_DiversionRight() : base()
	{
		initialize();
	}

	/// <summary>
	/// Clones the data object. </summary>
	/// <returns> a cloned object. </returns>
	public override object clone()
	{
		StateMod_DiversionRight right = (StateMod_DiversionRight)base.clone();
		right._irtem = _irtem;
		right._dcrdiv = _dcrdiv;
		right._isClone = true;

		return right;
	}

	/// <summary>
	/// Compares this object to another StateMod_Data object based on the sorted
	/// order from the StateMod_Data variables, and then by irtem and dcrdiv, in that order. </summary>
	/// <param name="data"> the object to compare against. </param>
	/// <returns> 0 if they are the same, 1 if this object is greater than the other object, or -1 if it is less. </returns>
	public virtual int CompareTo(StateMod_Data data)
	{
		int res = base.CompareTo(data);
		if (res != 0)
		{
			return res;
		}

		StateMod_DiversionRight right = (StateMod_DiversionRight)data;

		res = _irtem.CompareTo(right.getIrtem());
		if (res == 0)
		{
			double dcrdiv = right.getDcrdiv();
			if (dcrdiv == _dcrdiv)
			{
				return 0;
			}
			else if (_dcrdiv < dcrdiv)
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
	/// Creates a copy of the object for later use in checking to see if it was changed in a GUI.
	/// </summary>
	public virtual void createBackup()
	{
		_original = (StateMod_DiversionRight)clone();
		((StateMod_DiversionRight)_original)._isClone = false;
		_isClone = true;
	}

	/// <summary>
	/// Compare two rights list and see if they are the same. </summary>
	/// <param name="v1"> the first list of StateMod_DiversionRight to check.  Cannot be null. </param>
	/// <param name="v2"> the second list of StateMod_DiversionRight to check.  Cannot be null. </param>
	/// <returns> true if they are the same, false if not. </returns>
	public static bool Equals(IList<StateMod_DiversionRight> v1, IList<StateMod_DiversionRight> v2)
	{
		string routine = "StateMod_DiversionRight.equals(Vector, Vector)";
		StateMod_DiversionRight r1;
		StateMod_DiversionRight r2;
		if (v1.Count != v2.Count)
		{
			Message.printStatus(1, routine, "Lists are different sizes");
			return false;
		}
		else
		{
			// Sort the lists and compare item-by-item.  Any differences
			// and data will need to be saved back into the dataset.
			int size = v1.Count;
			//Message.printStatus(2, routine, "Lists are of size: " + size);
			IList<StateMod_DiversionRight> v1Sort = StateMod_Util.sortStateMod_DataVector(v1);
			IList<StateMod_DiversionRight> v2Sort = StateMod_Util.sortStateMod_DataVector(v2);
			//Message.printStatus(2, routine, "Lists have been sorted");

			for (int i = 0; i < size; i++)
			{
				r1 = v1Sort[i];
				r2 = v2Sort[i];
				//Message.printStatus(2, routine, r1.toString());
				//Message.printStatus(2, routine, r2.toString());
				//Message.printStatus(2, routine, "Element " + i + " comparison: " + r1.compareTo(r2));
				if (r1.CompareTo(r2) != 0)
				{
					return false;
				}
			}
		}
		return true;
	}

	/// <summary>
	/// Tests to see if two diversion rights are equal.  Strings are compared with case sensitivity. </summary>
	/// <param name="right"> the right to compare. </param>
	/// <returns> true if they are equal, false otherwise. </returns>
	public virtual bool Equals(StateMod_DiversionRight right)
	{
		if (!base.Equals(right))
		{
			 return false;
		}
		if (right._irtem.Equals(_irtem) && right._dcrdiv == _dcrdiv)
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
	~StateMod_DiversionRight()
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
	/// Returns the column headers for the specific data checked. </summary>
	/// <returns> List of column headers. </returns>
	public static string[] getDataHeader()
	{
		return new string[] {"Num", "Right ID", "Right Name"};
	}

	/// <summary>
	/// Return the decree, as per the generic interface. </summary>
	/// <returns> the decree, in the units of the data. </returns>
	public virtual double getDecree()
	{
		return getDcrdiv();
	}

	//TODO SAM 2007-05-16 Need to evaluate whether should be hard-coded.
	/// <summary>
	/// Return the decree units. </summary>
	/// <returns> the decree units. </returns>
	public virtual string getDecreeUnits()
	{
		return "CFS";
	}

	/// <summary>
	/// Return the decreed amount.
	/// </summary>
	public virtual double getDcrdiv()
	{
		return _dcrdiv;
	}

	/// <summary>
	/// Return the right identifier, as per the generic interface. </summary>
	/// <returns> the right identifier. </returns>
	public virtual string getIdentifier()
	{
		return getID();
	}

	/// <summary>
	/// Return a list of on/off switch option strings, for use in GUIs.
	/// The options are of the form "0" if include_notes is false and "0 - Off", if include_notes is true. </summary>
	/// <returns> a list of on/off switch option strings, for use in GUIs. </returns>
	/// <param name="include_notes"> Indicate whether notes should be added after the parameter values. </param>
	public static IList<string> getIdvrswChoices(bool include_notes)
	{
		IList<string> v = new List<string>(2);
		v.Add("0 - Off"); // Possible options are listed here.
		v.Add("1 - On");
		if (!include_notes)
		{
			// Remove the trailing notes...
			int size = v.Count;
			for (int i = 0; i < size; i++)
			{
				v[i] = StringUtil.getToken(v[i], " ", 0, 0);
			}
		}
		return v;
	}

	/// <summary>
	/// Return the default on/off switch choice.  This can be used by GUI code
	/// to pick a default for a new diversion. </summary>
	/// <returns> the default reservoir replacement choice. </returns>
	public static string getIdvrswDefault(bool include_notes)
	{ // Make this agree with the above method...
		if (include_notes)
		{
			return ("1 - On");
		}
		else
		{
			return "1";
		}
	}

	/// <summary>
	/// Return the administration number.
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
	/// Initializes data members.
	/// </summary>
	private void initialize()
	{
		_smdata_type = StateMod_DataSet.COMP_DIVERSION_RIGHTS;
		_irtem = "99999";
		_dcrdiv = 0;
	}


	/// <summary>
	/// Determine whether a file is a diversion right file.  Currently true is returned
	/// if the file extension is ".ddr". </summary>
	/// <param name="filename"> Name of the file being checked. </param>
	/// <returns> true if the file is a StateMod diversion right file. </returns>
	public static bool isDiversionRightFile(string filename)
	{
		if (filename.ToUpper().EndsWith(".DDR", StringComparison.Ordinal))
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Parses the diversion rights file and returns a Vector of StateMod_DiversionRight objects. </summary>
	/// <param name="filename"> the diversion rights file to parse. </param>
	/// <returns> a Vector of StateMod_DiversionRight objects. </returns>
	/// <exception cref="Exception"> if an error occurs </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.List<StateMod_DiversionRight> readStateModFile(String filename) throws Exception
	public static IList<StateMod_DiversionRight> readStateModFile(string filename)
	{
		string routine = "StateMod_DiversionRight.readStateModFile";
		IList<StateMod_DiversionRight> theDivRights = new List<StateMod_DiversionRight> ();

		int[] format_0 = new int[] {StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_DOUBLE, StringUtil.TYPE_INTEGER};
		int[] format_0w = new int[] {12, 24, 12, 16, 8, 8};
		string iline = null;
		IList<object> v = new List<object>(6);
		StreamReader @in = null;
		StateMod_DiversionRight aRight = null;

		Message.printStatus(2, routine, "Reading diversion rights file: " + filename);

		try
		{
			@in = new StreamReader(IOUtil.getPathUsingWorkingDir(filename));
			while (!string.ReferenceEquals((iline = @in.ReadLine()), null))
			{
				// check for comments
				if (iline.StartsWith("#", StringComparison.Ordinal) || iline.Trim().Length == 0)
				{
					continue;
				}

				aRight = new StateMod_DiversionRight();

				if (Message.isDebugOn)
				{
					Message.printDebug(50, routine, "iline: " + iline);
				}
				StringUtil.fixedRead(iline, format_0, format_0w, v);
				aRight.setID(((string)v[0]).Trim());
				aRight.setName(((string)v[1]).Trim());
				aRight.setCgoto(((string)v[2]).Trim());
				aRight.setIrtem(((string)v[3]).Trim());
				aRight.setDcrdiv((double?)v[4]);
				aRight.setSwitch((int?)v[5]);
				// Mark as clean because set methods may have marked dirty...
				aRight.setDirty(false);
				theDivRights.Add(aRight);
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
		return theDivRights;
	}

	/// <summary>
	/// Cancels any changes made to this object within a GUI since createBackup()
	/// was called and sets _original to null.
	/// </summary>
	public override void restoreOriginal()
	{
		StateMod_DiversionRight d = (StateMod_DiversionRight)_original;
		base.restoreOriginal();

		_irtem = d._irtem;
		_dcrdiv = d._dcrdiv;

		_isClone = false;
		_original = null;
	}

	/// <summary>
	/// Set the decreed amount.
	/// </summary>
	public virtual void setDcrdiv(double dcrdiv)
	{
		if (dcrdiv != _dcrdiv)
		{
			_dcrdiv = dcrdiv;
			setDirty(true);
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_DIVERSION_RIGHTS, true);
			}
		}
	}

	/// <summary>
	/// Set the decreed amount.
	/// </summary>
	public virtual void setDcrdiv(double? dcrdiv)
	{
		setDcrdiv(dcrdiv.Value);
	}

	/// <summary>
	/// Set the decreed amount.
	/// </summary>
	public virtual void setDcrdiv(string dcrdiv)
	{
		if (string.ReferenceEquals(dcrdiv, null))
		{
			return;
		}
		setDcrdiv(StringUtil.atod(dcrdiv.Trim()));
	}

	/// <summary>
	/// Set the decree, as per the generic interface. </summary>
	/// <param name="decree"> decree, in the units of the data. </param>
	public virtual void setDecree(double decree)
	{
		setDcrdiv(decree);
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
				_dataset.setDirty(StateMod_DataSet.COMP_DIVERSION_RIGHTS, true);
			}
		}
	}

	/// <summary>
	/// Returns a String representation of this object. </summary>
	/// <returns> a String representation of this object. </returns>
	public override string ToString()
	{
		return base.ToString() + ", " + _irtem + ", " + _dcrdiv;
	}

	public virtual StateMod_ComponentValidation validateComponent(StateMod_DataSet dataset)
	{
		StateMod_ComponentValidation validation = new StateMod_ComponentValidation();
		string id = getID();
		string name = getName();
		string cgoto = getCgoto();
		string irtem = getIrtem();
		double dcrdiv = getDcrdiv();
		// Make sure that basic information is not empty
		if (StateMod_Util.isMissing(id))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Diversion right identifier is blank.", "Specify a diversion right identifier."));
		}
		if (StateMod_Util.isMissing(name))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Diversion right \"" + id + "\" name is blank.", "Specify a diversion right name to clarify data."));
		}
		if (StateMod_Util.isMissing(cgoto))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Diversion right \"" + id + "\" diversion station ID is blank.", "Specify a diversion station to associate the diversion right."));
		}
		else
		{
			// Verify that the diversion station is in the data set, if the network is available
			DataSetComponent comp2 = dataset.getComponentForComponentType(StateMod_DataSet.COMP_DIVERSION_STATIONS);
			System.Collections.IList ddsList = (System.Collections.IList)comp2.getData();
			if ((ddsList != null) && (ddsList.Count > 0))
			{
				if (StateMod_Util.IndexOf(ddsList, cgoto) < 0)
				{
					validation.add(new StateMod_ComponentValidationProblem(this,"Diversion right \"" + id + "\" associated diversion (" + cgoto + ") is not found in the list of diversion stations.", "Specify a valid diversion station ID to associate the diversion right."));
				}
			}
		}
		if (StateMod_Util.isMissing(irtem))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Diversion right \"" + id + "\" administration number is blank.", "Specify an administration number NNNNN.NNNNN."));
		}
		else if (!StringUtil.isDouble(irtem))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Diversion right \"" + id + "\" administration number (" + irtem + ") is invalid.", "Specify an administration number NNNNN.NNNNN."));
		}
		else
		{
			double irtemd = double.Parse(irtem);
			if (irtemd < 0)
			{
				validation.add(new StateMod_ComponentValidationProblem(this,"Diversion right \"" + id + "\" administration number (" + irtem + ") is invalid.", "Specify an administration number NNNNN.NNNNN."));
			}
		}
		if (!(dcrdiv >= 0.0))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Diversion right \"" + id + "\" decree (" + StringUtil.formatString(dcrdiv,"%.2f") + ") is invalid.", "Specify the decree as a number >= 0."));
		}
		return validation;
	}

	/// <summary>
	/// Writes a diversion rights file. </summary>
	/// <param name="infile"> the original file </param>
	/// <param name="outfile"> the new file to write </param>
	/// <param name="theRights"> a list of StateMod_DiversionRight objects to right </param>
	/// <param name="newComments"> new comments to add to the header </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeStateModFile(String infile, String outfile, java.util.List<StateMod_DiversionRight> theRights, java.util.List<String> newComments) throws Exception
	public static void writeStateModFile(string infile, string outfile, IList<StateMod_DiversionRight> theRights, IList<string> newComments)
	{
		writeStateModFile(infile, outfile, theRights, newComments, false);
	}

	/// <summary>
	/// Writes a diversion rights file. </summary>
	/// <param name="infile"> the original file </param>
	/// <param name="outfile"> the new file to write </param>
	/// <param name="theRights"> a Vector of StateMod_DiversionRight objects to right </param>
	/// <param name="newComments"> new comments to add to the header </param>
	/// <param name="useOldAdminNumFormat"> whether to use the old admin num format or not </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeStateModFile(String infile, String outfile, java.util.List<StateMod_DiversionRight> theRights, java.util.List<String> newComments, boolean useOldAdminNumFormat) throws Exception
	public static void writeStateModFile(string infile, string outfile, IList<StateMod_DiversionRight> theRights, IList<string> newComments, bool useOldAdminNumFormat)
	{
		IList<string> commentIndicators = new List<string>(1);
		commentIndicators.Add("#");
		IList<string> ignoredCommentIndicators = new List<string>(1);
		ignoredCommentIndicators.Add("#>");
		PrintWriter @out = null;
		string routine = "StateMod_DiversionRight.writeStateModFile";
		Message.printStatus(2, routine, "Writing diversion rights to: " + outfile);

		try
		{
			@out = IOUtil.processFileHeaders(IOUtil.getPathUsingWorkingDir(infile), IOUtil.getPathUsingWorkingDir(outfile), newComments, commentIndicators, ignoredCommentIndicators, 0);

			string iline;
			string cmnt = "#>";
			string format_0 = null;
			if (useOldAdminNumFormat)
			{
				format_0 = "%-12.12s%-24.24s%-12.12s%-12.12s    %8.2F%8d";
			}
			else
			{
				format_0 = "%-12.12s%-24.24s%-12.12s%16.16s%8.2F%8d";
			}
			StateMod_DiversionRight right = null;
			IList<object> v = new List<object>(6);

			// print out the non-permanent header
			@out.println(cmnt);
			@out.println(cmnt + "***************************************************");
			@out.println(cmnt + " StateMod Direct Diversion Rights File");
			@out.println(cmnt);
			@out.println(cmnt + "     format:  (a12, a24, a12, f16.5, f8.2, i8)");
			@out.println(cmnt);
			@out.println(cmnt + "     ID       cidvri:  Diversion right ID ");
			@out.println(cmnt + "     Name      named:  Diversion right name");
			@out.println(cmnt + "     Struct    cgoto:  Direct Diversion Structure ID associated with this right");
			@out.println(cmnt + "     Admin #   irtem:  Administration number");
			@out.println(cmnt + "                       (small is senior).");
			@out.println(cmnt + "     Decree   dcrdiv:  Decreed amount (cfs)");
			@out.println(cmnt + "     On/Off   idvrsw:  Switch 0 = off, 1 = on");
			@out.println(cmnt + "                       YYYY = on for years >= YYYY.");
			@out.println(cmnt + "                       -YYYY = off for years > YYYY.");
			@out.println(cmnt);
			@out.println(cmnt + "   ID            Name              Struct            Admin #   Decree  On/Off");
			@out.println(cmnt + "EndHeader");
			@out.println(cmnt + "---------eb----------------------eb----------eb--------------eb------eb------e");

			int num = 0;
			if (theRights != null)
			{
				num = theRights.Count;
			}
			for (int i = 0; i < num; i++)
			{
				right = (StateMod_DiversionRight)theRights[i];
				if (right == null)
				{
					continue;
				}
				v.Clear();
				v.Add(right.getID());
				v.Add(right.getName());
				v.Add(right.getCgoto());
				v.Add(right.getIrtem());
				v.Add(new double?(right.getDcrdiv()));
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
	/// Writes a list of StateMod_Diversion objects to a list file.  A header is 
	/// printed to the top of the file, containing the commands used to generate the 
	/// file.  Any strings in the body of the file that contain the field delimiter will be wrapped in "...". </summary>
	/// <param name="filename"> the name of the file to which the data will be written. </param>
	/// <param name="delimiter"> the delimiter to use for separating field values. </param>
	/// <param name="update"> whether to update an existing file, retaining the current 
	/// header (true) or to create a new file with a new header. </param>
	/// <param name="data"> the list of objects to write. </param>
	/// <param name="newComments"> comments to add at the top of the file (e.g., command file, HydroBase version). </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeListFile(String filename, String delimiter, boolean update, java.util.List<StateMod_DiversionRight> data, java.util.List<String> newComments) throws Exception
	public static void writeListFile(string filename, string delimiter, bool update, IList<StateMod_DiversionRight> data, IList<string> newComments)
	{
		string routine = "StateMod_DiversionRight.writeListFile";
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
		int comp = StateMod_DataSet.COMP_DIVERSION_RIGHTS;
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
		StateMod_DiversionRight right = null;
		IList<string> commentIndicators = new List<string>(1);
		commentIndicators.Add("#");
		IList<string> ignoredCommentIndicators = new List<string>(1);
		ignoredCommentIndicators.Add("#>");
		string[] line = new string[fieldCount];
		StringBuilder buffer = new StringBuilder();
		PrintWriter @out = null;

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
			newComments2.Insert(1,"StateMod diversion rights as a delimited list file.");
			newComments2.Insert(2,"");
			@out = IOUtil.processFileHeaders(oldFile,IOUtil.getPathUsingWorkingDir(filename), newComments2, commentIndicators, ignoredCommentIndicators, 0);

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
				line[4] = StringUtil.formatString(right.getDcrdiv(),formats[4]).Trim();
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