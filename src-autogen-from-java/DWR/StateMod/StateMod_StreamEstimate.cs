using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

// StateMod_StreamEstimate - class for stream estimate station

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
// StateMod_StreamEstimate - class derived from StateMod_Data.  Contains
//	information the stream estimate station file (part of old .ris or new
//	.ses?)
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
// 
// 2003-08-14	Steven A. Malers, RTi	Copy StateMod_RiverStation and modify
//					accordingly.  The two classes are
//					essentially identical because they are
//					being read from the same file.
//					However, these baseflow nodes do not
//					have historical data.
// 2003-08-28	SAM, RTi		* Call setDirty() on each object in
//					  addition to the data component.
//					* Clean up handling of time series.
// 2003-09-11	SAM, RTi		Rename class from
//					StateMod_BaseFlowStation to
//					StateMod_StreamEstimate and make
//					appropriate changes throughout.
// 2003-09-12	SAM, RTi		* Ray Bennett decided to keep one file
//					  for the baseflow time series so no
//					  need to split apart.
//					* Rename processRiverData() to
//					  processStreamData().
// 2004-07-06	SAM, RTi		* Fix bug where writing the file was not
//					  adjusting the path using the working
//					  directory.
//					* Add information to the header comments
//					  to better explain the file contents.
//					* Overload the constructor to allow
//					  initialization to default values or
//					  missing data.
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
// 2007-04-12	Kurt Tometich, RTi		Added checkComponentData() and
//									getDataHeader() methods for check
//									file and data check support.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.

//------------------------------------------------------------------------------
// EndHeader

namespace DWR.StateMod
{

	using GeoRecord = RTi.GIS.GeoView.GeoRecord;
	using HasGeoRecord = RTi.GIS.GeoView.HasGeoRecord;
	using DayTS = RTi.TS.DayTS;
	using MonthTS = RTi.TS.MonthTS;
	using TS = RTi.TS.TS;
	using DataSetComponent = RTi.Util.IO.DataSetComponent;
	using IOUtil = RTi.Util.IO.IOUtil;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;

	public class StateMod_StreamEstimate : StateMod_Data, ICloneable, IComparable<StateMod_Data>, HasGeoRecord, StateMod_ComponentValidator
	{

	/// <summary>
	/// Monthly base flow time series, for use with the stream estimate station
	/// file, read from the .xbm/.rim file.
	/// </summary>
	protected internal MonthTS _baseflow_MonthTS;

	/// <summary>
	/// Daily base flow time series, read from the .rid file
	/// </summary>
	protected internal DayTS _baseflow_DayTS;
	/// <summary>
	/// Used with .rbs (column 4) - daily stream station identifier.
	/// </summary>
	protected internal string _crunidy;
	/// <summary>
	/// Reference to spatial data for this stream estimate station.
	/// </summary>
	protected internal GeoRecord _georecord;

	/// <summary>
	/// The StateMod_DataSet component type for the node.  At some point the
	/// related object reference may also be added, but there are cases when this
	/// is not known (only the type is known, for example in StateDMI).
	/// </summary>
	protected internal int _related_smdata_type;

	/// <summary>
	/// Second related type.  This is only used for D&W node types and should
	/// be set to the well stations component type.
	/// </summary>
	protected internal int _related_smdata_type2;

	// TODO - should we connect the .rib data similar to how water rights are
	// connected?   The data are not used as much as water rights.

	/// <summary>
	/// Constructor for stream estimate station.
	/// The time series are set to null and other information is empty strings.
	/// </summary>
	public StateMod_StreamEstimate() : this(true)
	{
	}

	/// <summary>
	/// Constructor for stream estimate station. </summary>
	/// <param name="initialize_defaults"> If true, the time series are set to null and other
	/// information is empty strings - this is suitable for the StateMod GUI.  If false,
	/// the data are set to missing - this is suitable for StateDMI where data will be filled. </param>
	public StateMod_StreamEstimate(bool initialize_defaults) : base()
	{
		initialize(initialize_defaults);
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
	/// Clones the data object. </summary>
	/// <returns> a cloned object. </returns>
	public override object clone()
	{
		StateMod_StreamEstimate s = (StateMod_StreamEstimate)base.clone();
		s._isClone = true;
		return s;
	}

	/// <summary>
	/// Compares this object to another StateMod_StreamEstimate object. </summary>
	/// <param name="data"> the object to compare against. </param>
	/// <returns> 0 if they are the same, 1 if this object is greater than the other object, or -1 if it is less. </returns>
	public virtual int CompareTo(StateMod_Data data)
	{
		int res = base.CompareTo(data);
		if (res != 0)
		{
			return res;
		}

		StateMod_StreamEstimate s = (StateMod_StreamEstimate)data;
		res = _crunidy.CompareTo(s._crunidy);
		if (res != 0)
		{
			return res;
		}

		if (_related_smdata_type < s._related_smdata_type)
		{
			return -1;
		}
		else if (_related_smdata_type > s._related_smdata_type)
		{
			return 1;
		}

		if (_related_smdata_type2 < s._related_smdata_type2)
		{
			return -1;
		}
		else if (_related_smdata_type2 > s._related_smdata_type2)
		{
			return 1;
		}

		return 0;
	}

	/// <summary>
	/// Connect the time series pointers to the appropriate time series objects
	/// for all the elements in the Vector of StateMod_StreamEstimate objects. </summary>
	/// <param name="rivs"> list of StateMod_StreamEstimate (e.g., as read from StateMod .rbs file). </param>
	/// <param name="baseflow_MonthTS"> list of baseflow MonthTS (e.g., as read from StateMod
	/// .xbm or .rim file).  Pass as null to not connect. </param>
	/// <param name="baseflow_DayTS"> list of baseflow MonthTS (e.g., as read from StateMod
	/// .xbd? or .rid file).  Pass as null to not connect. </param>
	public static void connectAllTS(IList<StateMod_StreamEstimate> rivs, IList<MonthTS> baseflow_MonthTS, IList<DayTS> baseflow_DayTS)
	{
		if (rivs == null)
		{
			return;
		}
		StateMod_StreamEstimate riv;
		int size = rivs.Count;
		for (int i = 0; i < size; i++)
		{
			riv = rivs[i];
			if (baseflow_MonthTS != null)
			{
				riv.connectBaseflowMonthTS(baseflow_MonthTS);
			}
			if (baseflow_DayTS != null)
			{
				riv.connectBaseflowDayTS(baseflow_DayTS);
			}
		}
	}

	/// <summary>
	/// Connect the daily base streamflow TS pointer to the appropriate TS in the list.
	/// A connection is made if the node identifier matches the time series location. </summary>
	/// <param name="tslist"> list of DayTS. </param>
	private void connectBaseflowDayTS(IList<DayTS> tslist)
	{
		if (tslist == null)
		{
			return;
		}
		DayTS ts;
		int size = tslist.Count;
		_baseflow_DayTS = null;
		for (int i = 0; i < size; i++)
		{
			ts = tslist[i];
			if (ts == null)
			{
				continue;
			}
			if (_id.Equals(ts.getLocation(), StringComparison.OrdinalIgnoreCase))
			{
				// Set this because the original file does not have...
				ts.setDescription(getName());
				_baseflow_DayTS = ts;
				break;
			}
		}
	}

	/// <summary>
	/// Connect monthly baseflow time series. </summary>
	/// <param name="tslist"> monthly baseflow time series.  </param>
	public virtual void connectBaseflowMonthTS(IList<MonthTS> tslist)
	{
		if (tslist == null)
		{
			return;
		}
		int num_TS = tslist.Count;

		MonthTS ts = null;
		_baseflow_MonthTS = null;
		for (int i = 0; i < num_TS; i++)
		{
			ts = tslist[i];
			if (ts == null)
			{
				continue;
			}
			if (_id.Equals(ts.getLocation(), StringComparison.OrdinalIgnoreCase))
			{
				// Set this because the original file does not have...
				ts.setDescription(getName());
				_baseflow_MonthTS = ts;
				break;
			}
		}
	}

	/// <summary>
	/// Creates a copy of the object for later use in checking to see if it was changed in a GUI.
	/// </summary>
	public virtual void createBackup()
	{
		_original = (StateMod_StreamEstimate)clone();
		((StateMod_StreamEstimate)_original)._isClone = false;
		_isClone = true;
	}

	/// <summary>
	/// Finalize data for garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~StateMod_StreamEstimate()
	{
		_baseflow_MonthTS = null;
		_baseflow_DayTS = null;
		_crunidy = null;
		_georecord = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Return the daily baseflow file associated with the stream estimate node. </summary>
	/// <returns> the daily baseflow file associated with the stream estimate node.
	/// Return null if no time series is available. </returns>
	public virtual DayTS getBaseflowDayTS()
	{
		return _baseflow_DayTS;
	}

	/// <summary>
	/// Return the monthly baseflow file associated with the stream estimate node. </summary>
	/// <returns> the monthly baseflow file associated with the stream estimate node.
	/// Return null if no time series is available. </returns>
	public virtual MonthTS getBaseflowMonthTS()
	{
		return _baseflow_MonthTS;
	}

	/// <summary>
	/// Get the daily stream station identifier used with the stream estimate station. </summary>
	/// <returns> the daily stream station identifier. </returns>
	public virtual string getCrunidy()
	{
		return _crunidy;
	}

	/// <summary>
	/// Returns the data column header for the specifically checked data. </summary>
	/// <returns> Data column header. </returns>
	public static string[] getDataHeader()
	{
		// TODO KAT 2007-04-16 
		// When specific checks are added to checkComponentData
		// return the header for that data here
		return new string[] {};
	}

	/// <summary>
	/// Get the geographical data associated with the station. </summary>
	/// <returns> the GeoRecord for the station. </returns>
	public virtual GeoRecord getGeoRecord()
	{
		return _georecord;
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
	/// Initialize data. </summary>
	/// <param name="initialize_defaults"> If true, the time series are set to null and other
	/// information is empty strings - this is suitable for the StateMod GUI.  If false,
	/// the data are set to missing - this is suitable for StateDMI where data will be filled. </param>
	private void initialize(bool initialize_defaults)
	{
		_smdata_type = StateMod_DataSet.COMP_STREAMESTIMATE_STATIONS;
		_cgoto = "";
		_baseflow_MonthTS = null;
		_baseflow_DayTS = null;
		_related_smdata_type = StateMod_DataSet.COMP_UNKNOWN;
		if (initialize_defaults)
		{
			// Reasonable defaults...
			_crunidy = "0"; // Estimate average daily from monthly data.
		}
		else
		{
			// Missing...
			_crunidy = "";
		}
		_georecord = null;
	}

	/// <summary>
	/// This method can be called when an old-style *.ris file containing both stream
	/// gage station and stream estimate stations is read.  The following adjustments to the data occur:
	/// <ol>
	/// <li>	Objects in the ris that do not have matching identifiers in the rih are	removed from the ris.</li>
	/// <li>	Objects in the rbs that do not have matching identifiers in the rib are	removed from the rbs.</li>
	/// </ol> </summary>
	/// <param name="ris_Vector"> list of StateMod_StreamGage, after initial read. </param>
	/// <param name="rih_Vector"> list of historical MonthTS, after initial read. </param>
	/// <param name="rbs_Vector"> list of StateMod_StreamEstimate, after initial read. </param>
	/// <param name="rib_Vector"> list of StateMod_StreamEstimte_Coefficients, after initial
	/// read. </param>
	public static void processStreamData(IList<StateMod_StreamGage> ris_Vector, IList<TS> rih_Vector, IList<StateMod_StreamEstimate> rbs_Vector, IList<StateMod_StreamEstimate_Coefficients> rib_Vector)
	{
		int nris = 0;
		if (ris_Vector != null)
		{
			nris = ris_Vector.Count;
		}
		int nrih = 0;
		if (rih_Vector != null)
		{
			nrih = rih_Vector.Count;
		}
		int nrbs = 0;
		if (rbs_Vector != null)
		{
			nrbs = rbs_Vector.Count;
		}
		int nrib = 0;
		if (rib_Vector != null)
		{
			nrib = rib_Vector.Count;
		}

		int i, j;
		TS ts;
		StateMod_StreamGage ris;
		string id;
		bool found = false;
		for (i = 0; i < nris; i++)
		{
			ris = ris_Vector[i];
			id = ris.getID();
			found = false;
			for (j = 0; j < nrih; j++)
			{
				ts = rih_Vector[j];
				if (id.Equals(ts.getLocation(), StringComparison.OrdinalIgnoreCase))
				{
					found = true;
					break;
				}
			}
			if (!found)
			{
				ris_Vector.RemoveAt(i);
				--i; // So next item will be properly checked.
				--nris;
			}
		}
		StateMod_StreamEstimate rbs;
		StateMod_StreamEstimate_Coefficients rib;
		for (i = 0; i < nrbs; i++)
		{
			rbs = rbs_Vector[i];
			id = rbs.getID();
			found = false;
			for (j = 0; j < nrib; j++)
			{
				rib = rib_Vector[j];
				if (id.Equals(rib.getID(), StringComparison.OrdinalIgnoreCase))
				{
					found = true;
					break;
				}
			}
			if (!found)
			{
				rbs_Vector.RemoveAt(i);
				--i; // So next item will be properly checked.
				--nrbs;
			}
		}
	}

	/// <summary>
	/// Read river station file and store return a Vector of StateMod_StreamEstimate.
	/// Note that ALL stations are returned.  Call the processRiverData() method to
	/// remove instances that are not actually base flow nodes. </summary>
	/// <returns> a Vector of StateMod_BaseFlowStation. </returns>
	/// <param name="filename"> Name of file to read. </param>
	/// <exception cref="Exception"> if there is an error reading the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.List<StateMod_StreamEstimate> readStateModFile(String filename) throws Exception
	public static IList<StateMod_StreamEstimate> readStateModFile(string filename)
	{
		string rtn = "StateMod_StreamEstimate.readStateModFile";
		IList<StateMod_StreamEstimate> theRivs = new List<StateMod_StreamEstimate>();
		string iline;
		IList<object> v = new List<object>(5);
		int[] format_0;
		int[] format_0w;
		format_0 = new int[5];
		format_0[0] = StringUtil.TYPE_STRING;
		format_0[1] = StringUtil.TYPE_STRING;
		format_0[2] = StringUtil.TYPE_STRING;
		format_0[3] = StringUtil.TYPE_STRING;
		format_0[4] = StringUtil.TYPE_STRING;
		format_0w = new int [5];
		format_0w[0] = 12;
		format_0w[1] = 24;
		format_0w[2] = 12;
		format_0w[3] = 1;
		format_0w[4] = 12;
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

				// allocate new StateMod_BaseFlowStation node
				StateMod_StreamEstimate aRiverNode = new StateMod_StreamEstimate();

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
				aRiverNode.setCgoto(((string)v[2]).Trim());
				// Space
				aRiverNode.setCrunidy(((string)v[4]).Trim());

				// add the node to the vector of river nodes
				theRivs.Add(aRiverNode);
			}
		}
		catch (Exception e)
		{
			Message.printWarning(2, rtn, "Error reading \"" + filename + "\" at line " + linecount);
			throw e;
		}
		finally
		{
			if (@in != null)
			{
				@in.Close();
				@in = null;
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
		StateMod_StreamEstimate s = (StateMod_StreamEstimate)_original;
		base.restoreOriginal();
		_crunidy = s._crunidy;
		_related_smdata_type = s._related_smdata_type;
		_related_smdata_type2 = s._related_smdata_type2;
		_isClone = false;
		_original = null;
	}

	/// <summary>
	/// Set the daily baseflow TS. </summary>
	/// <param name="ts"> daily baseflow TS. </param>
	public virtual void setBaseflowDayTS(DayTS ts)
	{
		_baseflow_DayTS = ts;
	}

	/// <summary>
	/// Set the monthly baseflow TS. </summary>
	/// <param name="ts"> monthly baseflow TS. </param>
	public virtual void setBaseflowMonthTS(MonthTS ts)
	{
		_baseflow_MonthTS = ts;
	}

	/// <summary>
	/// Set the daily stream station for the node. </summary>
	/// <param name="crunidy"> Daily station identifier for node. </param>
	public virtual void setCrunidy(string crunidy)
	{
		if ((!string.ReferenceEquals(crunidy, null)) && !_crunidy.Equals(crunidy))
		{
			_crunidy = crunidy;
			setDirty(true);
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(_smdata_type, true);
			}
		}
	}

	/// <summary>
	/// Set the geographic information object associated with the station. </summary>
	/// <param name="georecord"> Geographic record associated with the station. </param>
	public virtual void setGeoRecord(GeoRecord georecord)
	{
		_georecord = georecord;
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
	/// <returns> validation results. </returns>
	public virtual StateMod_ComponentValidation validateComponent(StateMod_DataSet dataset)
	{
		StateMod_ComponentValidation validation = new StateMod_ComponentValidation();
		string id = getID();
		string name = getName();
		string riverID = getCgoto();
		string dailyID = getCrunidy();
		// Make sure that basic information is not empty
		if (StateMod_Util.isMissing(id))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Stream estimate identifier is blank.", "Specify a station identifier."));
		}
		if (StateMod_Util.isMissing(name))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Stream estimate \"" + id + "\" name is blank.", "Specify a station name to clarify data."));
		}
		if (StateMod_Util.isMissing(riverID))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Stream estimate \"" + id + "\" river ID is blank.", "Specify a river ID to associate the station with a river network node."));
		}
		else
		{
			// Verify that the river node is in the data set, if the network is available
			if (dataset != null)
			{
				DataSetComponent comp = dataset.getComponentForComponentType(StateMod_DataSet.COMP_RIVER_NETWORK);
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<StateMod_RiverNetworkNode> rinList = (java.util.List<StateMod_RiverNetworkNode>)comp.getData();
				IList<StateMod_RiverNetworkNode> rinList = (IList<StateMod_RiverNetworkNode>)comp.getData();
				if ((rinList != null) && (rinList.Count > 0))
				{
					if (StateMod_Util.IndexOf(rinList, riverID) < 0)
					{
						validation.add(new StateMod_ComponentValidationProblem(this,"Stream estimate \"" + id + "\" river network ID (" + riverID + ") is not found in the list of river network nodes.", "Specify a valid river network ID to associate the station with a river network node."));
					}
				}
			}
		}
		// Verify that the daily ID is in the data set
		if (!StateMod_Util.isMissing(dailyID))
		{
			if (dataset != null)
			{
				DataSetComponent comp = dataset.getComponentForComponentType(StateMod_DataSet.COMP_STREAMGAGE_STATIONS);
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<StateMod_StreamGage> risList = (java.util.List<StateMod_StreamGage>)comp.getData();
				IList<StateMod_StreamGage> risList = (IList<StateMod_StreamGage>)comp.getData();
				if ((risList != null) && (risList.Count > 0))
				{
					if (!dailyID.Equals("0") && !dailyID.Equals("3") && !dailyID.Equals("4") && (StateMod_Util.IndexOf(risList, dailyID) < 0))
					{
						validation.add(new StateMod_ComponentValidationProblem(this,"Stream estimate \"" + id + "\" daily ID (" + dailyID + ") is not 0, 3, or 4 and is not found in the list of stream gages.", "Specify the daily ID as 0, 3, 4, or that matches a stream gage ID."));
					}
				}
			}
		}
		return validation;
	}

	/// <summary>
	/// Write the new (updated) river baseflow stations file.  If an original file is
	/// specified, then the original header is carried into the new file. </summary>
	/// <param name="infile"> Name of old file or null if no old file to update. </param>
	/// <param name="outfile"> Name of new file to create (can be the same as the old file). </param>
	/// <param name="theRivs"> list of StateMod_StreamEstimate to write. </param>
	/// <param name="newcomments"> New comments to write in the file header. </param>
	/// <param name="do_daily"> Indicates whether daily modeling fields should be written. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeStateModFile(String infile, String outfile, java.util.List<StateMod_StreamEstimate> theRivs, java.util.List<String> newcomments, boolean do_daily) throws Exception
	public static void writeStateModFile(string infile, string outfile, IList<StateMod_StreamEstimate> theRivs, IList<string> newcomments, bool do_daily)
	{
		PrintWriter @out = null;
		IList<string> commentIndicators = new List<string>(1);
		commentIndicators.Add("#");
		IList<string> ignoredCommentIndicators = new List<string>(1);
		ignoredCommentIndicators.Add("#>");
		string routine = "StateMod_StreamEstimate.writeStateModFile";

		if (Message.isDebugOn)
		{
			Message.printDebug(2, routine, "Writing stream estimate stations to file \"" + outfile + "\" using \"" + infile + "\" header...");
		}

		try
		{
			// Process the header from the old file...
			@out = IOUtil.processFileHeaders(IOUtil.getPathUsingWorkingDir(infile), IOUtil.getPathUsingWorkingDir(outfile), newcomments, commentIndicators, ignoredCommentIndicators, 0);

			string cmnt = "#>";
			string iline = null;
			string format = null;
			StateMod_StreamEstimate riv = null;

			@out.println(cmnt + " *******************************************************");
			@out.println(cmnt + "  Stream Estimate Station File");
			@out.println(cmnt);
			@out.println(cmnt + "  This file contains a list of stations at which stream");
			@out.println(cmnt + "  natural flows are estimated.");
			@out.println(cmnt + "  The IDs for nodes will match the IDs in one of the following files:");
			@out.println(cmnt + "      Diversion stations");
			@out.println(cmnt + "      Reservoir stations");
			@out.println(cmnt + "      Instream flow stations");
			@out.println(cmnt + "      Well stations");
			@out.println(cmnt + "  Stream gages with historical data are in the stream gage station file.");
			@out.println(cmnt + "  \"Other\" nodes with baseflow data are only listed in the river network file.");
			@out.println(cmnt);
			@out.println(cmnt + "     format:  (a12, a24, a12, 1x, a12)");
			@out.println(cmnt);
			@out.println(cmnt + "  ID         crunid:  Station ID");
			@out.println(cmnt + "  Name       runnam:  Station name");
			@out.println(cmnt + "  River ID    cgoto:  River node with stream estimate station");
			@out.println(cmnt + "  Daily ID  crunidy:  Daily stream station ID.");
			@out.println(cmnt);
			@out.println(cmnt + "    ID              Name           River ID     Daily ID   ");
			@out.println(cmnt + "---------eb----------------------eb----------exb----------e");
			if (do_daily)
			{
				format = "%-12.12s%-24.24s%-12.12s %-12.12s";
			}
			else
			{
				format = "%-12.12s%-24.24s%-12.12s";
			}
			@out.println(cmnt);
			@out.println(cmnt + "EndHeader");
			@out.println(cmnt);

			int num = 0;
			if (theRivs != null)
			{
				num = theRivs.Count;
			}
			IList<object> v = new List<object> (5);
			for (int i = 0; i < num; i++)
			{
				riv = theRivs[i];
				v.Clear();
				v.Add(riv.getID());
				v.Add(riv.getName());
				v.Add(riv.getCgoto());
				if (do_daily)
				{
					v.Add(riv.getCrunidy());
				}
				iline = StringUtil.formatString(v, format);
				@out.println(iline);
			}
		}
		catch (Exception e)
		{
			Message.printWarning(2, routine, e);
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
	/// Writes a list of StateMod_StreamEstimate objects to a list file.  A header is 
	/// printed to the top of the file, containing the commands used to generate the 
	/// file.  Any strings in the body of the file that contain the field delimiter will be wrapped in "...". </summary>
	/// <param name="filename"> the name of the file to which the data will be written. </param>
	/// <param name="delimiter"> the delimiter to use for separating field values. </param>
	/// <param name="update"> whether to update an existing file, retaining the current 
	/// header (true) or to create a new file with a new header. </param>
	/// <param name="data"> the list of objects to write. </param>
	/// <param name="newComments"> list of new comments to add in the file header. </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeListFile(String filename, String delimiter, boolean update, java.util.List<StateMod_StreamEstimate> data, java.util.List<String> newComments) throws Exception
	public static void writeListFile(string filename, string delimiter, bool update, IList<StateMod_StreamEstimate> data, IList<string> newComments)
	{
		int size = 0;
		if (data != null)
		{
			size = data.Count;
		}

		IList<string> fields = new List<string>();
		fields.Add("ID");
		fields.Add("Name");
		fields.Add("RiverNodeID");
		fields.Add("DailyID");
		int fieldCount = fields.Count;

		string[] names = new string[fieldCount];
		string[] formats = new string[fieldCount];
		int comp = StateMod_DataSet.COMP_STREAMESTIMATE_STATIONS;

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
		StateMod_StreamEstimate se = null;
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
			newComments2.Insert(1,"StateMod stream estimate stations as a delimited list file.");
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
				se = (StateMod_StreamEstimate)data[i];

				line[0] = StringUtil.formatString(se.getID(), formats[0]).Trim();
				line[1] = StringUtil.formatString(se.getName(), formats[1]).Trim();
				line[2] = StringUtil.formatString(se.getCgoto(), formats[2]).Trim();
				line[3] = StringUtil.formatString(se.getCrunidy(), formats[3]).Trim();

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
			@out.flush();
			@out.close();
			@out = null;
		}
		catch (Exception e)
		{
			// TODO SAM 2009-01-05 Log it?
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