using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

// StateMod_RiverNetworkNode - class to store network node data

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
// StateMod_RiverNetworkNode - class derived from StateMod_Data.  Contains
//	information read from the river network file
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
// 
// 02 Sep 1997	Catherine E.		Created initial version of class.
//		Nutting-Lane, RTi
// 23 Feb 1998	Catherine E.		Added write routines.
//		Nutting-Lane, RTi
// 21 Dec 1998	CEN, RTi		Added throws IOException to read/write
//					routines.
// 06 Feb 2001	Steven A. Malers, RTi	Update to handle new daily data.  Also,
//					Ray added a gwmaxr data item to the
//					.rin file.  Consequently, this
//					StateMod_RiverInfo class can not be 
//					shared as
//					transparently between .rin and .ris
//					files.  Probably need to make this a
//					base class and derive SMStation (or
//					similar) from it, but for now just put
//					specific .rin and .ris data here and use
//					a flag to indicate which is used.  Need
//					some help from Catherine to clean up at
//					some point.  Update javadoc as I go
//					through and figure things out.  Add
//					finalize method and set unused data to
//					null to help garbage collection.
//					Alphabetize methods.  Optimize loops so
//					size() is not called each iteration.
//					Check for null arguments.  Change some
//					low-level status messages to debug
//					messages to improve performance.
//					Optimize lookups by using _id rather
//					than calling getID().  There are still
//					places (like cases where strings are
//					manipulated without checking for null)
//					where error handling is not complete but
//					leave for now since it seems to be
//					working.  Use trim() instead of
//					StringUtil to simplify code.  Add line
//					cound to read routine to print in
//					error message.  Remove all "additional
//					string" code in favor of specific data
//					since Ray is beginning to add to files
//					in inconsistent ways.  Change IO to
//					IOUtil.  Add constructor to parse a
//					string and handle the setrin() syntax
//					used by makenet.  This allows the
//					StateMod_RiverInfo object to store set
//					information with not much more work.
//					Add applySetRinCommands() to apply
//					edits.
// 2001-12-27	SAM, RTi		Update to use new fixedRead() to
//					improve performance.
// 2002-09-12	SAM, RTi		Add the baseflow time series (.xbm or
//					.rim) to this class for the (.ris) file
//					display.  Remove the overloaded
//					connectAllTS() that only handled monthly
//					time series.  One version of the method
//					should be ok since the StateMod GUI is
//					the only thing that uses it.
//					Also add the daily baseflow time series
//					corresponding to the .rid file.
// 2002-09-19	SAM, RTi		Use isDirty() instead of setDirty() to
//					indicate edits.
// 2002-10-07	SAM, RTi		Add GeoRecord reference to allow 2-way
//					connection between spatial and StateMod
//					data.
//------------------------------------------------------------------------------
// 2003-06-04	J. Thomas Sapienza, RTi	Renamed from SMrivInfo
// 2003-06-10	JTS, RTi		* Folded dumpRiverInfoFile() into
//					  writeRiverInfoFile()
//					* Renamed parseRiverInfoFile() to
//					  readRiverInfoFile()
// 2003-06-23	JTS, RTi		Renamed writeRiverInfoFile() to
//					writeStateModFile()
// 2003-06-26	JTS, RTi		Renamed readRiverInfoFile() to
//					readStateModFile()
// 2003-07-30	SAM, RTi		* Change name of class from
//					  StateMod_RiverInfo to
//					  StateMod_RiverNetworkNode.
//					* Remove all code related to the RIS
//					  file, which is now in
//					  StateMod_RiverStation.
//					* Change isDirty() back to setDirty().
// 2003-08-28	SAM, RTi		* Call setDirty() on each object in
//					  addition to the data set component.
//					* Clean up javadoc and parameters.
// 2004-07-10	SAM, RTi		Add the _related_smdata_type and
//					_related_smdata_type2 data members.
//					This allows the node types to
//					be set when the list of stream estimate
//					stations is read from the network file.
//					This allows the node type to be properly
//					set for the last 3 characters in the
//					name, as has traditionally been done.
//					This change is made for stream gage and
//					stream estimate stations because in
//					order to support old data sets, the
//					stream estimate stations are combined
//					with stream gage stations.
// 2004-07-14	JTS, RTi		* Added acceptChanges().
//					* Added changed().
//					* Added clone().
//					* Added compareTo().
//					* Added createBackup().
//					* Added restoreOriginal().
//					* Now implements Cloneable.
//					* Now implements Comparable.
//					* Clone status is checked via _isClone
//					  when the component is marked as dirty.
// 2005-04-18	JTS, RTi		Added writeListFile().
// 2005-06-13	JTS, RTi		Made a new toString().
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
//------------------------------------------------------------------------------
// EndHeader

namespace DWR.StateMod
{

	using GeoRecord = RTi.GIS.GeoView.GeoRecord;
	using HasGeoRecord = RTi.GIS.GeoView.HasGeoRecord;
	using DataSetComponent = RTi.Util.IO.DataSetComponent;
	using IOUtil = RTi.Util.IO.IOUtil;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// This StateMod_RiverNetworkNode class manages a record of data from the StateMod
	/// river network (.rin) file.  It is derived from StateMod_Data similar to other
	/// StateMod data objects.  It should not be confused with network node objects
	/// (e.g., StateMod_Diversion_Node).   See the readStateModFile() method to read
	/// the .rin file into a true network.
	/// </summary>
	public class StateMod_RiverNetworkNode : StateMod_Data, ICloneable, IComparable<StateMod_Data>, HasGeoRecord, StateMod_ComponentValidator
	{

	/// <summary>
	/// Downstream node identifier - third column of files.
	/// </summary>
	protected internal string _cstadn;
	/// <summary>
	/// Used with .rin (column 4) - not really used anymore except by old watright code.
	/// </summary>
	protected internal new string _comment;
	/// <summary>
	/// Reference to spatial data for this diversion -- currently NOT cloned.  If null, then no spatial data
	/// are available.
	/// </summary>
	protected internal GeoRecord _georecord = null;
	/// <summary>
	/// Used with .rin (column 5) - ground water maximum recharge limit.
	/// </summary>
	protected internal double _gwmaxr;
	/// <summary>
	/// The StateMod_DataSet component type for the node.  At some point the related object reference
	/// may also be added, but there are cases when this is not known (only the type is
	/// known, for example in StateDMI).
	/// </summary>
	protected internal int _related_smdata_type;

	/// <summary>
	/// Second related type.  This is only used for D&W node types and should
	/// be set to the well stations component type.
	/// </summary>
	protected internal int _related_smdata_type2;

	/// <summary>
	/// Constructor.  The time series are set to null and other information is empty strings.
	/// </summary>
	public StateMod_RiverNetworkNode() : base()
	{
		initialize();
	}

	/// <summary>
	/// Accepts any changes made inside of a GUI to this object.
	/// </summary>
	public virtual void acceptChanges()
	{
		_isClone = false;
		_original = null;
	}

	/// <summary>
	/// Compares this object with its original value (generated by createBackup() upon
	/// entering a GUI) to see if it has changed.
	/// </summary>
	public virtual bool changed()
	{
		if (_original == null)
		{
			return true;
		}
		if (compareTo(_original) == 0)
		{
			return false;
		}
		return true;
	}

	/// <summary>
	/// Compares this object to another StateMod_RiverNetworkNode object. </summary>
	/// <param name="data"> the object to compare against. </param>
	/// <returns> 0 if they are the same, 1 if this object is greater than the other object, or -1 if it is less. </returns>
	public virtual int CompareTo(StateMod_Data data)
	{
		int res = base.CompareTo(data);
		if (res != 0)
		{
			return res;
		}

		StateMod_RiverNetworkNode r = (StateMod_RiverNetworkNode)data;

		res = _cstadn.CompareTo(r._cstadn);
		if (res != 0)
		{
			return res;
		}

		res = _comment.CompareTo(r._comment);
		if (res != 0)
		{
			return res;
		}

		if (_gwmaxr < r._gwmaxr)
		{
			return -1;
		}
		else if (_gwmaxr > r._gwmaxr)
		{
			return 1;
		}

		if (_related_smdata_type < r._related_smdata_type)
		{
			return -1;
		}
		else if (_related_smdata_type > r._related_smdata_type)
		{
			return 1;
		}

		if (_related_smdata_type2 < r._related_smdata_type2)
		{
			return -1;
		}
		else if (_related_smdata_type2 > r._related_smdata_type2)
		{
			return 1;
		}

		return 0;
	}

	/// <summary>
	/// Clones the data object. </summary>
	/// <returns> a cloned object. </returns>
	public override object clone()
	{
		StateMod_RiverNetworkNode r = (StateMod_RiverNetworkNode)base.clone();
		r._isClone = true;
		return r;
	}

	/// <summary>
	/// Creates a backup of the current data object and stores it in _original,
	/// for use in determining if an object was changed inside of a GUI.
	/// </summary>
	public virtual void createBackup()
	{
		_original = (StateMod_RiverNetworkNode)clone();
		((StateMod_RiverNetworkNode)_original)._isClone = false;
		_isClone = true;
	}

	/// <summary>
	/// Finalize data for garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~StateMod_RiverNetworkNode()
	{
		_cstadn = null;
		_comment = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Get the comment used with the network file. </summary>
	/// <returns> the comment. </returns>
	public override string getComment()
	{
		return _comment;
	}

	/// <summary>
	/// Get the downstream river node identifier. </summary>
	/// <returns> the downstream river node identifier. </returns>
	public virtual string getCstadn()
	{
		return _cstadn;
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
	/// Get the geographical data associated with the diversion. </summary>
	/// <returns> the GeoRecord for the diversion. </returns>
	public virtual GeoRecord getGeoRecord()
	{
		return _georecord;
	}

	/// <summary>
	/// Get the maximum recharge limit used with the network file. </summary>
	/// <returns> the maximum recharge limit. </returns>
	public virtual double getGwmaxr()
	{
		return _gwmaxr;
	}

	/// <summary>
	/// Get the StateMod_DataSet component type for the data for this node, or
	/// StateMod_DataSet.COMP_UNKNOWN if unknown.
	/// Get the StateMod_DataSet component type for the data for this node.
	/// </summary>
	public virtual int getRelatedSMDataType()
	{
		return _related_smdata_type;
	}

	/// <summary>
	/// Get the StateMod_DataSet component type for the data for this node, or
	/// StateMod_DataSet.COMP_UNKNOWN if unknown.
	/// This is only used for D&W nodes and should be set to the well component type.
	/// Get the StateMod_DataSet component type for the data for this node.
	/// </summary>
	public virtual int getRelatedSMDataType2()
	{
		return _related_smdata_type2;
	}

	/// <summary>
	/// Initialize data.
	/// </summary>
	private void initialize()
	{
		_cstadn = "";
		_comment = "";
		_gwmaxr = -999;
		_smdata_type = StateMod_DataSet.COMP_RIVER_NETWORK;
	}

	/// <summary>
	/// Read river network or stream gage information and return a list of StateMod_RiverNetworkNode. </summary>
	/// <param name="filename"> Name of file to read. </param>
	/// <exception cref="Exception"> if there is an error reading the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.List<StateMod_RiverNetworkNode> readStateModFile(String filename) throws Exception
	public static IList<StateMod_RiverNetworkNode> readStateModFile(string filename)
	{
		string rtn = "StateMod_RiverNetworkNode.readStateModFile";
		IList<StateMod_RiverNetworkNode> theRivs = new List<StateMod_RiverNetworkNode>();
		string iline, s;
		IList<object> v = new List<object>(7);
		int[] format_0;
		int[] format_0w;
		format_0 = new int[7];
		format_0[0] = StringUtil.TYPE_STRING;
		format_0[1] = StringUtil.TYPE_STRING;
		format_0[2] = StringUtil.TYPE_STRING;
		format_0[3] = StringUtil.TYPE_STRING;
		format_0[4] = StringUtil.TYPE_STRING;
		format_0[5] = StringUtil.TYPE_STRING;
		format_0[6] = StringUtil.TYPE_STRING;
		format_0w = new int [7];
		format_0w[0] = 12;
		format_0w[1] = 24;
		format_0w[2] = 12;
		format_0w[3] = 1;
		format_0w[4] = 12;
		format_0w[5] = 1;
		format_0w[6] = 8;

		int linecount = 0;

		if (Message.isDebugOn)
		{
			Message.printDebug(10, rtn, "in " + rtn + " reading file: " + filename);
		}
		StreamReader @in = null;
		try
		{
			@in = new StreamReader(filename);
			while (!string.ReferenceEquals((iline = @in.ReadLine()), null))
			{
				++linecount;
				// check for comments
				if (iline.StartsWith("#", StringComparison.Ordinal) || iline.Trim().Length == 0)
				{
					continue;
				}

				// allocate new StateMod_RiverNetworkNode
				StateMod_RiverNetworkNode aRiverNode = new StateMod_RiverNetworkNode();

				// line 1
				if (Message.isDebugOn)
				{
					Message.printDebug(50, rtn, "line 1: " + iline);
				}
				StringUtil.fixedRead(iline, format_0, format_0w, v);
				if (Message.isDebugOn)
				{
					Message.printDebug(50, rtn, "Fixed read returned " + v.Count + " elements");
				}
				aRiverNode.setID(((string)v[0]).Trim());
				aRiverNode.setName(((string)v[1]).Trim());
				aRiverNode.setCstadn(((string)v[2]).Trim());
				// 3 is whitespace
				// Expect that we also may have the comment and possibly the gwmaxr value...
				aRiverNode.setComment(((string)v[4]).Trim());
				// 5 is whitespace
				s = ((string)v[6]).Trim();
				if (s.Length > 0)
				{
					aRiverNode.setGwmaxr(StringUtil.atod(s));
				}

				// add the node to the vector of river nodes
				theRivs.Add(aRiverNode);
			}
		}
		catch (Exception e)
		{
			Message.printWarning(3, rtn, "Error reading \"" + filename + "\" at line " + linecount);
			throw e;
		}
		finally
		{
			if (@in != null)
			{
				@in.Close();
			}
		}
		return theRivs;
	}

	/// <summary>
	/// Cancels any changes made to this object within a GUI since createBackup()
	/// was called and sets _original to null.
	/// </summary>
	public override void restoreOriginal()
	{
		StateMod_RiverNetworkNode r = (StateMod_RiverNetworkNode)_original;
		base.restoreOriginal();
		_cstadn = r._cstadn;
		_comment = r._comment;
		_gwmaxr = r._gwmaxr;
		_related_smdata_type = r._related_smdata_type;
		_related_smdata_type2 = r._related_smdata_type2;
		_isClone = false;
		_original = null;
	}

	/// <summary>
	/// Set the comment for use with the network file. </summary>
	/// <param name="comment"> Comment for node. </param>
	public override void setComment(string comment)
	{
		if ((!string.ReferenceEquals(comment, null)) && !_comment.Equals(comment))
		{
			_comment = comment;
			setDirty(true);
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(_smdata_type, true);
			}
		}
	}

	/// <summary>
	/// Set the downstream river node identifier. </summary>
	/// <param name="cstadn"> Downstream river node identifier. </param>
	public virtual void setCstadn(string cstadn)
	{
		if ((!string.ReferenceEquals(cstadn, null)) && !cstadn.Equals(_cstadn))
		{
			_cstadn = cstadn;
			setDirty(true);
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(_smdata_type, true);
			}
		}
	}

	/// <summary>
	/// Set the geographic information object associated with the diversion. </summary>
	/// <param name="georecord"> Geographic record associated with the diversion. </param>
	public virtual void setGeoRecord(GeoRecord georecord)
	{
		_georecord = georecord;
	}

	/// <summary>
	/// Set the maximum recharge limit for network file. </summary>
	/// <param name="gwmaxr"> Maximum recharge limit. </param>
	public virtual void setGwmaxr(string gwmaxr)
	{
		if (StringUtil.isDouble(gwmaxr))
		{
			setGwmaxr(StringUtil.atod(gwmaxr));
		}
	}

	/// <summary>
	/// Set the maximum recharge limit for network file. </summary>
	/// <param name="gwmaxr"> Maximum recharge limit. </param>
	public virtual void setGwmaxr(double gwmaxr)
	{
		if (_gwmaxr != gwmaxr)
		{
			_gwmaxr = gwmaxr;
			setDirty(true);
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(_smdata_type, true);
			}
		}
	}

	/// <summary>
	/// Set the StateMod_DataSet component type for the data for this node. </summary>
	/// <param name="related_smdata_type"> The StateMod_DataSet component type for the data for this node. </param>
	public virtual void setRelatedSMDataType(int related_smdata_type)
	{
		_related_smdata_type = related_smdata_type;
	}

	/// <summary>
	/// Set the second StateMod_DataSet component type for the data for this node. </summary>
	/// <param name="related_smdata_type"> The second StateMod_DataSet component type for the data for this node.
	/// This is only used for D&W nodes and should be set to the well component type. </param>
	public virtual void setRelatedSMDataType2(int related_smdata_type2)
	{
		_related_smdata_type2 = related_smdata_type2;
	}

	/// <param name="dataset"> StateMod dataset object. </param>
	/// <returns> Validation results. </returns>
	public virtual StateMod_ComponentValidation validateComponent(StateMod_DataSet dataset)
	{
		StateMod_ComponentValidation validation = new StateMod_ComponentValidation();
		string id = getID();
		string name = getName();
		string downstreamRiverID = getCstadn();
		double gwmaxr = getGwmaxr();
		// Make sure that basic information is not empty
		if (StateMod_Util.isMissing(id))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"River node identifier is blank.", "Specify a river node identifier."));
		}
		if (StateMod_Util.isMissing(name))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"River node \"" + id + "\" name is blank.", "Specify a river node name to clarify data."));
		}
		// Get the network list if available for checks below
		DataSetComponent comp = null;
		System.Collections.IList rinList = null;
		if (dataset != null)
		{
			comp = dataset.getComponentForComponentType(StateMod_DataSet.COMP_RIVER_NETWORK);
			rinList = (System.Collections.IList)comp.getData();
			if ((rinList != null) && (rinList.Count == 0))
			{
				// Set to null to simplify checks below
				rinList = null;
			}
		}
		if (StateMod_Util.isMissing(downstreamRiverID) && !name.Equals("END", StringComparison.OrdinalIgnoreCase) && !name.EndsWith("_END", StringComparison.Ordinal))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"River node \"" + id + "\" downstream node ID is blank.", "Specify a downstream node ID."));
		}
		else
		{
			// Verify that the downstream river node is in the data set, if the network is available - skip this
			// check for the end node.
			if ((rinList != null) && !name.Equals("END", StringComparison.OrdinalIgnoreCase) && !name.EndsWith("_END", StringComparison.Ordinal))
			{
				if (StateMod_Util.IndexOf(rinList, downstreamRiverID) < 0)
				{
					validation.add(new StateMod_ComponentValidationProblem(this,"River node \"" + id + "\" downstream node ID (" + downstreamRiverID + ") is not found in the list of river network nodes.", "Specify a valid river network ID for the downstream node."));
				}
			}
		}
		if (!StateMod_Util.isMissing(gwmaxr) && !(gwmaxr >= 0.0))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"River node \"" + id + "\" maximum groundwater recharge (" + StringUtil.formatString(gwmaxr,"%.2f") + ") is invalid.", "Specify the maximum groundwater recharge as a number >= 0."));
		}
		return validation;
	}

	/// <summary>
	/// Write the new (updated) river network file to the StateMod river network
	/// file.  If an original file is specified, then the original header is carried into the new file. </summary>
	/// <param name="infile"> Name of old file or null if no old file to update. </param>
	/// <param name="outfile"> Name of new file to create (can be the same as the old file). </param>
	/// <param name="theRivs"> list of StateMod_RiverNetworkNode to write. </param>
	/// <param name="newcomments"> New comments to write in the file header. </param>
	/// <param name="doWell"> Indicates whether well modeling fields should be written. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeStateModFile(String infile, String outfile, java.util.List<StateMod_RiverNetworkNode> theRivs, java.util.List<String> newcomments, boolean doWell) throws Exception
	public static void writeStateModFile(string infile, string outfile, IList<StateMod_RiverNetworkNode> theRivs, IList<string> newcomments, bool doWell)
	{
		PrintWriter @out = null;
		IList<string> commentIndicators = new List<string>(1);
		commentIndicators.Add("#");
		IList<string> ignoredCommentIndicators = new List<string>(1);
		ignoredCommentIndicators.Add("#>");
		string routine = "StateMod_RiverNetworkNode.writeStateModFile";

		if (Message.isDebugOn)
		{
			Message.printDebug(2, routine, "Writing river network file \"" + outfile + "\" using \"" + infile + "\" header...");
		}

		try
		{
			// Process the header from the old file...
			@out = IOUtil.processFileHeaders(IOUtil.getPathUsingWorkingDir(infile), IOUtil.getPathUsingWorkingDir(outfile), newcomments, commentIndicators, ignoredCommentIndicators, 0);

			string cmnt = "#>";
			string iline = null;
			string format = null;
			StateMod_RiverNetworkNode riv = null;

			@out.println(cmnt + " *******************************************************");
			@out.println(cmnt + "  StateMod River Network File");
			@out.println(cmnt + "  WARNING - if .net file is available, it should be edited and the .rin");
			@out.println(cmnt + "  file should be created from the .net");
			@out.println(cmnt);
			@out.println(cmnt + "  format:  (a12, a24, a12, 1x, a12, 1x, f8.0)");
			@out.println(cmnt);
			@out.println(cmnt + "  ID           cstaid:  Station ID");
			@out.println(cmnt + "  Name         stanam:  Station name");
			@out.println(cmnt + "  Downstream   cstadn:  Downstream node ID");
			@out.println(cmnt + "  Comment     comment:  Alternate identifier/comment.");
			@out.println(cmnt + "  GWMax        gwmaxr:  Max recharge limit (cfs) - see iwell in control file.");
			@out.println(cmnt);
			@out.println(cmnt + "   ID                Name          DownStream     Comment    GWMax  ");
			@out.println(cmnt + "---------eb----------------------eb----------exb----------exb------e");
			if (doWell)
			{
				format = "%-12.12s%-24.24s%-12.12s %-12.12s %8.8s";
			}
			else
			{
				format = "%-12.12s%-24.24s%-12.12s %-12.12s";
			}
			@out.println(cmnt);
			@out.println(cmnt + "EndHeader");
			@out.println(cmnt);

			int num = 0;
			if (theRivs != null)
			{
				num = theRivs.Count;
			}
			IList<object> v = new List<object>(5);
			for (int i = 0; i < num; i++)
			{
				riv = theRivs[i];
				v.Clear();
				v.Add(riv.getID());
				v.Add(riv.getName());
				v.Add(riv.getCstadn());
				v.Add(riv.getComment());
				if (doWell)
				{
					// Format as string since main format uses string.
					v.Add(StringUtil.formatString(riv.getGwmaxr(), "%8.0f"));
				}
				iline = StringUtil.formatString(v, format);
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
	/// Writes a list of StateMod_RiverNetworkNode objects to a list file.  A header 
	/// is printed to the top of the file, containing the commands used to generate the
	/// file.  Any strings in the body of the file that contain the field delimiter will be wrapped in "...". </summary>
	/// <param name="filename"> the name of the file to which the data will be written. </param>
	/// <param name="delimiter"> the delimiter to use for separating field values. </param>
	/// <param name="update"> whether to update an existing file, retaining the current 
	/// header (true) or to create a new file with a new header. </param>
	/// <param name="data"> the list of objects to write. </param>
	/// <param name="newComments"> new comments to add to the file header. </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeListFile(String filename, String delimiter, boolean update, java.util.List<StateMod_RiverNetworkNode> data, java.util.List<String> newComments) throws Exception
	public static void writeListFile(string filename, string delimiter, bool update, IList<StateMod_RiverNetworkNode> data, IList<string> newComments)
	{
		string routine = "StateMod_RiverNetworkNode.writeListFile";
		int size = 0;
		if (data != null)
		{
			size = data.Count;
		}

		IList<string> fields = new List<string>();
		fields.Add("ID");
		fields.Add("Name");
		fields.Add("DownstreamID");
		fields.Add("Comment");
		fields.Add("GWMaxRecharge");
		int fieldCount = fields.Count;

		string[] names = new string[fieldCount];
		string[] formats = new string[fieldCount];
		int comp = StateMod_DataSet.COMP_RIVER_NETWORK;
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
		StateMod_RiverNetworkNode rnn = null;
		string[] line = new string[fieldCount];
		IList<string> commentIndicators = new List<string>(1);
		commentIndicators.Add("#");
		IList<string> ignoredCommentIndicators = new List<string>(1);
		ignoredCommentIndicators.Add("#>");
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
			newComments2.Insert(1,"StateMod river network as a delimited list file.");
			newComments2.Insert(2,"See also the generalized network file.");
			newComments2.Insert(3,"");
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
				rnn = (StateMod_RiverNetworkNode)data[i];

				line[0] = StringUtil.formatString(rnn.getID(), formats[0]).Trim();
				line[1] = StringUtil.formatString(rnn.getName(), formats[1]).Trim();
				line[2] = StringUtil.formatString(rnn.getCstadn(), formats[2]).Trim();
				line[3] = StringUtil.formatString(rnn.getComment(), formats[3]).Trim();
				line[4] = StringUtil.formatString(rnn.getGwmaxr(), formats[4]).Trim();

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

	public override string ToString()
	{
		return "ID: " + _id + "    Downstream node: " + _cstadn;
	}

	}

}