using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

// StateMod_InstreamFlow - class instream flow station

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
// StateMod_InstreamFlow - class derived from SMData.  Contains information 
//	read from the instream flow file.
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
// 
// 08 Sep 1997	Catherine E.		Created initial version of class.
//		Nutting-Lane, RTi
// 11 Feb 1998	CEN, RTi		Added _dataset.setDirty
//					to all set
//					routines.
// 21 Dec 1998	CEN, RTi		Added throws IOException to read/write
//					routines.
// 25 Oct 1999	CEN, RTi		Added daily instream flow id.
// 03 Mar 2000	Steven A. Malers, RTi	Add iifcom(data type switch).  Javadoc
//					the constructor.  Add a finalize()
//					method.  Also Javadoc the I/O code.
// 15 Feb 2001	SAM, RTi		Add use_daily_data parameter to
//					writeInstreamFlowFile()method to 
//					allow comparison
//					with older files.  Clean up javadoc some
//					more.  Alphabetize methods.  Optimize
//					memory by setting unused variables to
//					null.  Handle null arguments better.
//					Update header information with current
//					variables.  Change IO to IOUtil.
// 2001-12-27	SAM, RTi		Update to use new fixedRead()to
//					improve performance.
// 2002-09-09	SAM, RTi		Add GeoRecord reference to allow 2-way
//					connection between spatial and StateMod
//					data.
// 2002-09-19	SAM, RTi		Use isDirty()instead of setDirty()to
//					indicate edits.
//------------------------------------------------------------------------------
// 2003-06-04	J. Thomas Sapienza, RTi	Renamed from SMInsflow to 
//					StateMod_InstreamFlow
// 2003-06-10	JTS, RTI		* Folded dumpInstreamFlowFile() into
//					  writeInstreamFlowFile()
// 					* Renamed parseInstreamFlowFile() to
//					  readInstreamFlowFile()
// 2003-06-23	JTS, RTi		Renamed writeInstreamFlowFile() to
//					writeStateModFile()
// 2003-06-26	JTS, RTi		Renamed readInstreamFlowFile() to
//					readStateModFile()
// 2003-07-15	JTS, RTi		Changed code to use new dataset design.
// 2003-08-03	SAM, RTi		Change isDirty() back to setDirty().
// 2003-08-15	SAM, RTi		Change GeoRecordNoSwing to GeoRecord.
// 2003-08-28	SAM, RTi		* Call setDirty() for each object and
//					  the component.
//					* Change the rights to a simple Vector
//					  (not a linked list) and remove the
//					  data member for the number of rights.
//					* Clean up Javadoc for parameters.
//					* Clean up handling of the time series.
//					* Clean up method names to not have
//					  "Insf" - this is redundant.
// 2003-10-10	SAM, RTi		Add disconnectRights().
// 2004-07-06	SAM, RTi		* Overload the constructor to allow data
//					  to be set to missing or be initialized
//					  to reasonable defaults.
//					* Add getIifcomChoices() and
//					  getDefaultIifcom().
//					* Fix output header - was not lined up
//					  correctly.
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
// 2006-03-06	SAM, RTi		Fix bug where all rights were being
//					connected, not just the ones associated
//					with this instream flow station/reach.
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
	using DataSetComponent = RTi.Util.IO.DataSetComponent;
	using IOUtil = RTi.Util.IO.IOUtil;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;

	public class StateMod_InstreamFlow : StateMod_Data, IComparable<StateMod_Data>, HasGeoRecord, StateMod_ComponentValidator
	{

	/// <summary>
	/// Daily instream flow id
	/// </summary>
	protected internal string _cifridy;

	/// <summary>
	/// Downstream river node, ins located
	/// </summary>
	protected internal string _ifrrdn;

	/// <summary>
	/// Instream flow rights
	/// </summary>
	protected internal IList<StateMod_InstreamFlowRight> _rights;

	/// <summary>
	/// Annual demand time series
	/// </summary>
	protected internal MonthTS _demand_average_MonthTS;

	/// <summary>
	/// Monthly demand time series
	/// </summary>
	protected internal MonthTS _demand_MonthTS;

	/// <summary>
	/// Daily demand time series
	/// </summary>
	protected internal DayTS _demand_DayTS;

	/// <summary>
	/// Data type switch
	/// </summary>
	protected internal int _iifcom;

	/// <summary>
	/// Link to spatial data.
	/// </summary>
	protected internal GeoRecord _georecord = null;

	/// <summary>
	/// Construct a new instance of a StateMod instream flow station.
	/// The initial data values are empty strings, no rights or time series, and ifcom=1.
	/// </summary>
	public StateMod_InstreamFlow() : this(true)
	{
	}

	/// <summary>
	/// Construct a new instance of a StateMod instream flow station. </summary>
	/// <param name="initialize_defaults"> If true, then data values are initialized to
	/// reasonable defaults - this is suitable for adding a new instance in the
	/// StateMod GUI.  If false, data values are initialized to missing - this is
	/// suitable for a new instance in StateDMI. </param>
	public StateMod_InstreamFlow(bool initialize_defaults) : base()
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
	/// Adds a right for the instream flow station.
	/// </summary>
	public virtual void addRight(StateMod_InstreamFlowRight right)
	{
		if (right == null)
		{
			return;
		}
		_rights.Add(right);
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
		StateMod_InstreamFlow i = (StateMod_InstreamFlow)base.clone();
		i._isClone = true;

		// The following are not cloned because there is no need to.  
		// The cloned values are only used for comparing between the 
		// values that can be changed in a single GUI.  The following
		// lists' data have their changes committed in other GUIs.	
		i._rights = _rights;
		return i;
	}

	/// <summary>
	/// Compares this object to another StateMod_InstreamFlow object. </summary>
	/// <param name="data"> the object to compare against. </param>
	/// <returns> 0 if they are the same, 1 if this object is greater than the other object, or -1 if it is less. </returns>
	public virtual int CompareTo(StateMod_Data data)
	{
		int res = base.CompareTo(data);
		if (res != 0)
		{
			return res;
		}

		StateMod_InstreamFlow i = (StateMod_InstreamFlow)data;

		res = _cifridy.CompareTo(i._cifridy);
		if (res != 0)
		{
			return res;
		}

		res = _ifrrdn.CompareTo(i._ifrrdn);
		if (res != 0)
		{
			return res;
		}

		if (_iifcom < i._iifcom)
		{
			return -1;
		}
		else if (_iifcom > i._iifcom)
		{
			return 1;
		}

		return 0;
	}

	/// <summary>
	/// Connect instream flow rights to stations. </summary>
	/// <param name="isfs"> list of instream flow stations </param>
	/// <param name="rights"> list of instream flow rights </param>
	public static void connectAllRights(IList<StateMod_InstreamFlow> isfs, IList<StateMod_InstreamFlowRight> rights)
	{
		if ((isfs == null) || (rights == null))
		{
			return;
		}
		int num_insf = isfs.Count;

		StateMod_InstreamFlow insf;
		for (int i = 0; i < num_insf; i++)
		{
			insf = isfs[i];
			if (insf == null)
			{
				continue;
			}
			insf.connectRights(rights);
		}
		insf = null;
	}

	/// <summary>
	/// Connect all instream flow time series to the instream flow objects.
	/// </summary>
	public static void connectAllTS(IList<StateMod_InstreamFlow> theIns, IList<MonthTS> demandMonthTS, IList<MonthTS> demandAverageMonthTS, IList<DayTS> demandDayTS)
	{
		if (theIns == null)
		{
			return;
		}
		int numInsf = theIns.Count;
		StateMod_InstreamFlow insflow;

		for (int i = 0; i < numInsf; i++)
		{
			insflow = theIns[i];
			if (insflow == null)
			{
				continue;
			}
			if (demandMonthTS != null)
			{
				insflow.connectDemandMonthTS(demandMonthTS);
			}
			if (demandAverageMonthTS != null)
			{
				insflow.connectDemandAverageMonthTS(demandAverageMonthTS);
			}
			if (demandDayTS != null)
			{
				insflow.connectDemandDayTS(demandDayTS);
			}
		}
	}

	/// <summary>
	/// Connect daily demand time series to the instream flow.
	/// The daily id "cifridy" must match the time series.
	/// The time series description is set to the station name.
	/// </summary>
	public virtual void connectDemandDayTS(IList<DayTS> tslist)
	{
		if (tslist == null)
		{
			return;
		}
		_demand_DayTS = null;
		int numTS = tslist.Count;
		DayTS ts;

		for (int i = 0; i < numTS; i++)
		{
			ts = tslist[i];
			if (ts == null)
			{
				continue;
			}
			if (_cifridy.Equals(ts.getLocation(), StringComparison.OrdinalIgnoreCase))
			{
				_demand_DayTS = ts;
				ts.setDescription(getName());
				break;
			}
		}
	}

	/// <summary>
	/// Connect average monthly demand time series to the instream flow.
	/// The time series description is set to the station name.
	/// </summary>
	public virtual void connectDemandAverageMonthTS(IList<MonthTS> tslist)
	{
		if (tslist == null)
		{
			return;
		}
		int numTS = tslist.Count;
		MonthTS ts;

		_demand_average_MonthTS = null;
		for (int i = 0; i < numTS; i++)
		{
			ts = tslist[i];
			if (ts == null)
			{
				continue;
			}
			if (_id.Equals(ts.getLocation()))
			{
				_demand_average_MonthTS = ts;
				ts.setDescription(getName());
				break;
			}
		}
	}

	/// <summary>
	/// Connect monthly demand time series to the instream flow.
	/// The time series description is set to the station name.
	/// </summary>
	public virtual void connectDemandMonthTS(IList<MonthTS> tslist)
	{
		if (tslist == null)
		{
			return;
		}
		int numTS = tslist.Count;
		MonthTS ts;

		_demand_MonthTS = null;
		for (int i = 0; i < numTS; i++)
		{
			ts = tslist[i];
			if (ts == null)
			{
				continue;
			}
			if (_id.Equals(ts.getLocation()))
			{
				_demand_MonthTS = ts;
				ts.setDescription(getName());
				break;
			}
		}
	}

	/// <summary>
	/// Connect the rights in the main rights file to this instream flow, using the instream flow ID.
	/// </summary>
	public virtual void connectRights(IList<StateMod_InstreamFlowRight> rights)
	{
		if (rights == null)
		{
			return;
		}
		int num_rights = rights.Count;

		StateMod_InstreamFlowRight right;
		for (int i = 0; i < num_rights; i++)
		{
			right = rights[i];
			if (right == null)
			{
				continue;
			}
			if (_id.Equals(right.getCgoto(), StringComparison.OrdinalIgnoreCase))
			{
				_rights.Add(right);
			}
		}
	}

	/// <summary>
	/// Creates a copy of the object for later use in checking to see if it was changed in a GUI.
	/// </summary>
	public virtual void createBackup()
	{
		_original = (StateMod_InstreamFlow)clone();
		((StateMod_InstreamFlow)_original)._isClone = false;
		_isClone = true;
	}

	// TODO - in the GUI need to decide if the right is actually removed from the main list
	/// <summary>
	/// Remove right from list.  A comparison on the ID is made. </summary>
	/// <param name="right"> Right to remove.  Note that the right is only removed from the
	/// list for this instream flow station and must also be removed from the main instream flow right list. </param>
	public virtual void disconnectRight(StateMod_InstreamFlowRight right)
	{
		if (right == null)
		{
			return;
		}
		int size = _rights.Count;
		StateMod_InstreamFlowRight right2;
		// Assume that more than on instance can exist, even though this is not allowed...
		for (int i = 0; i < size; i++)
		{
			right2 = _rights[i];
			if (right2.getID().Equals(right.getID(), StringComparison.OrdinalIgnoreCase))
			{
				_rights.RemoveAt(i);
			}
		}
	}

	/// <summary>
	/// Disconnect all rights.
	/// </summary>
	public virtual void disconnectRights()
	{
		_rights.Clear();
	}

	/// <summary>
	/// Free memory for garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~StateMod_InstreamFlow()
	{
		_ifrrdn = null;
		_cifridy = null;
		_rights = null;
		_demand_MonthTS = null;
		_demand_average_MonthTS = null;
		_demand_DayTS = null;
		_georecord = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Return Cifridy
	/// </summary>
	public virtual string getCifridy()
	{
		return _cifridy;
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
	/// Returns daily demand ts
	/// </summary>
	public virtual DayTS getDemandDayTS()
	{
		return _demand_DayTS;
	}

	/// <summary>
	/// Returns average monthly demand ts
	/// </summary>
	public virtual MonthTS getDemandAverageMonthTS()
	{
		return _demand_average_MonthTS;
	}

	/// <summary>
	/// Returns monthly demand ts
	/// </summary>
	public virtual MonthTS getDemandMonthTS()
	{
		return _demand_MonthTS;
	}

	/// <summary>
	/// Get the geographical data associated with the instream flow station. </summary>
	/// <returns> the GeoRecord for the instream flow station. </returns>
	public virtual GeoRecord getGeoRecord()
	{
		return _georecord;
	}

	/// <returns> the value for iifcom for the StateMod instream flow station. </returns>
	public virtual int getIifcom()
	{
		return _iifcom;
	}

	/// <summary>
	/// Return a list of demand type option strings, for use in GUIs.
	/// The options are of the form "1" if include_notes is false and
	/// "1 - Monthly demand", if include_notes is true. </summary>
	/// <returns> a list of demand type option strings, for use in GUIs. </returns>
	/// <param name="include_notes"> Indicate whether notes should be included. </param>
	public static IList<string> getIifcomChoices(bool include_notes)
	{
		IList<string> v = new List<string>(5);
		v.Add("1 - Monthly demand");
		v.Add("2 - Average monthly demand");
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
	/// Return the default demand type choice.  This can be used by GUI code
	/// to pick a default for a new instream flow. </summary>
	/// <returns> the default demand type choice. </returns>
	public static string getIifcomDefault(bool include_notes)
	{
		if (include_notes)
		{
			return "2 - Average monthly demand";
		}
		else
		{
			return "2";
		}
	}

	/// <summary>
	/// Return the downstream river node where instream is located.
	/// </summary>
	public virtual string getIfrrdn()
	{
		return _ifrrdn;
	}

	/// <summary>
	/// Get the last right associated with the instream flow.
	/// </summary>
	public virtual StateMod_InstreamFlowRight getLastRight()
	{
		if ((_rights == null) || (_rights.Count == 0))
		{
			return null;
		}
		return (StateMod_InstreamFlowRight)_rights[_rights.Count - 1];
	}

	/// <summary>
	/// Return the number of rights.
	/// </summary>
	public virtual int getNumrights()
	{
		return _rights.Count;
	}

	/// <summary>
	/// Return the right associated with the given index.  If index
	/// number of rights don't exist, null will be returned. </summary>
	/// <param name="index"> desired right index </param>
	public virtual StateMod_InstreamFlowRight getRight(int index)
	{
		if ((index < 0) || (index >= _rights.Count))
		{
			return null;
		}
		else
		{
			return (StateMod_InstreamFlowRight)_rights[index];
		}
	}

	/// <summary>
	/// Returns list of rights.
	/// </summary>
	public virtual IList<StateMod_InstreamFlowRight> getRights()
	{
		return _rights;
	}

	/// <summary>
	/// Initialize data. </summary>
	/// <param name="initializeDefaults"> If true, then data values are initialized to
	/// reasonable defaults - this is suitable for adding a new instance in the
	/// StateMod GUI.  If false, data values are initialized to missing - this is
	/// suitable for a new instance in StateDMI. </param>
	private void initialize(bool initializeDefaults)
	{
		_smdata_type = StateMod_DataSet.COMP_INSTREAM_STATIONS;
		_ifrrdn = "";
		if (initializeDefaults)
		{
			_cifridy = "0"; // Estimate average daily data from monthly
			_iifcom = 2; // Default to annual
		}
		else
		{
			_cifridy = "";
			_iifcom = StateMod_Util.MISSING_INT;
		}
		_rights = new List<StateMod_InstreamFlowRight>();
		_demand_DayTS = null;
		_demand_MonthTS = null;
		_demand_average_MonthTS = null;
		_georecord = null;
	}

	/// <summary>
	/// Return the downstream data set object by searching appropriate dataset lists. </summary>
	/// <param name="dataset"> the full dataset from which the destination should be extracted </param>
	public virtual StateMod_Data lookupDownstreamDataObject(StateMod_DataSet dataset)
	{
		string downstreamID = getIfrrdn();
		StateMod_OperationalRight_Metadata_SourceOrDestinationType[] downstreamTypes = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.RIVER_NODE};
		IList<StateMod_Data> smdataList = new List<StateMod_Data>();
		for (int i = 0; i < downstreamTypes.Length; i++)
		{
			 ((IList<StateMod_Data>)smdataList).AddRange(StateMod_Util.getDataList(downstreamTypes[i], dataset, downstreamID, true));
			if (smdataList.Count > 0)
			{
				break;
			}
		}
		if (smdataList.Count == 1)
		{
			return smdataList[0];
		}
		else if (smdataList.Count == 0)
		{
			return null;
		}
		else
		{
			throw new Exception("" + smdataList.Count + " data objects returned matching downstream \"" + downstreamID + "\" for instream flow \"" + getID() + " - one is expected.");
		}
	}

	/// <summary>
	/// Read instream flow information in and store in a list.  The new instream
	/// flows are added to the end of the previously stored instream flows. </summary>
	/// <param name="filename"> Name of file to read. </param>
	/// <exception cref="Exception"> if there is an error reading the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.List<StateMod_InstreamFlow> readStateModFile(String filename) throws Exception
	public static IList<StateMod_InstreamFlow> readStateModFile(string filename)
	{
		string routine = "StateMod_InstreamFlow.readStateModFile";
		string iline, s;
		IList<StateMod_InstreamFlow> theIns = new List<StateMod_InstreamFlow>();
		IList<object> v = new List<object>(9);
		int[] format_0 = new int[] {StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_INTEGER, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING};
		int[] format_0w = new int[] {12, 24, 12, 8, 1, 12, 1, 12, 8};

		if (Message.isDebugOn)
		{
			Message.printDebug(10, routine, "Reading file: " + filename);
		}
		StreamReader @in = null;
		try
		{
			@in = new StreamReader(filename);
			StateMod_InstreamFlow anIns;
			while (!string.ReferenceEquals((iline = @in.ReadLine()), null))
			{
				// check for comments
				if (iline.StartsWith("#", StringComparison.Ordinal) || iline.Trim().Length == 0)
				{
					continue;
				}

				// allocate new instream flow node
				anIns = new StateMod_InstreamFlow();

				// line 1
				if (Message.isDebugOn)
				{
					Message.printDebug(50, routine, "line 1: " + iline);
				}
				StringUtil.fixedRead(iline, format_0, format_0w, v);
				if (Message.isDebugOn)
				{
					Message.printDebug(50, routine, "Fixed read returned " + v.Count + " elements");
				}
				s = StringUtil.unpad((string)v[0], " ", StringUtil.PAD_FRONT_BACK);
				anIns.setID(s);
				s = StringUtil.unpad((string)v[1], " ", StringUtil.PAD_FRONT_BACK);
				anIns.setName(s);
				s = StringUtil.unpad((string)v[2], " ", StringUtil.PAD_FRONT_BACK);
				anIns.setCgoto(s);
				anIns.setSwitch((int?)v[3]);
				s = StringUtil.unpad((string)v[5], " ", StringUtil.PAD_FRONT_BACK);
				if (Message.isDebugOn)
				{
					Message.printDebug(50, routine, "Ifrrdn: " + s);
				}
				anIns.setIfrrdn(s);
				// daily id
				s = StringUtil.unpad((string)v[7], " ", StringUtil.PAD_FRONT_BACK);
				anIns.setCifridy(s);
				// Data type(read as string and convert to integer)...
				s = StringUtil.unpad((string)v[8], " ", StringUtil.PAD_FRONT_BACK);
				anIns.setIifcom(s);

				// add the instream flow to the vector of instream flows
				theIns.Add(anIns);
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
		return theIns;
	}

	/// <summary>
	/// Cancels any changes made to this object within a GUI since createBackup()
	/// was called and sets _original to null.
	/// </summary>
	public override void restoreOriginal()
	{
		StateMod_InstreamFlow i = (StateMod_InstreamFlow)_original;
		base.restoreOriginal();
		_cifridy = i._cifridy;
		_ifrrdn = i._ifrrdn;
		_iifcom = i._iifcom;
		_original = null;
		_isClone = false;
	}

	/// <summary>
	/// Sets cifridy
	/// </summary>
	public virtual void setCifridy(string cifridy)
	{
		if ((!string.ReferenceEquals(cifridy, null)) && !cifridy.Equals(_cifridy))
		{
			_cifridy = cifridy;
			setDirty(true);
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_INSTREAM_STATIONS,true);
			}
		}
	}

	/// <summary>
	/// Sets daily demand ts
	/// </summary>
	public virtual void setDemandDayTS(DayTS demand_DayTS)
	{
		_demand_DayTS = demand_DayTS;
	}

	/// <summary>
	/// Sets average monthly demand ts
	/// </summary>
	public virtual void setDemandAverageMonthTS(MonthTS demand_average_MonthTS)
	{
		_demand_average_MonthTS = demand_average_MonthTS;
	}

	/// <summary>
	/// Sets monthly demand ts
	/// </summary>
	public virtual void setDemandMonthTS(MonthTS demand_MonthTS)
	{
		_demand_MonthTS = demand_MonthTS;
	}

	/// <summary>
	/// Set the geographic information object associated with the instream flow station. </summary>
	/// <param name="georecord"> Geographic record associated with the instream flow station. </param>
	public virtual void setGeoRecord(GeoRecord georecord)
	{
		_georecord = georecord;
	}

	/// <summary>
	/// Set the value for iifcom for the StateMod instream flow station.
	/// </summary>
	public virtual void setIifcom(int iifcom)
	{
		// Only set if value has changed...
		if (iifcom != _iifcom)
		{
			_iifcom = iifcom;
			setDirty(true);
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_INSTREAM_STATIONS,true);
			}
		}
	}

	/// <summary>
	/// Set the value for iifcom for the StateMod instream flow station.
	/// </summary>
	public virtual void setIifcom(string iifcom)
	{
		setIifcom(StringUtil.atoi(iifcom));
	}

	/// <summary>
	/// Set the downstream river node where instream is located.
	/// </summary>
	public virtual void setIfrrdn(string ifrrdn)
	{
		if (!ifrrdn.Equals(_ifrrdn))
		{
			_ifrrdn = ifrrdn;
			setDirty(true);
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_INSTREAM_STATIONS,true);
			}
		}
	}

	// TODO - need to make sure dirty flag is handled.
	/// <summary>
	/// Set the rights list. </summary>
	/// <param name="rights"> list of rights to set - this should not be null. </param>
	public virtual void setRights(IList<StateMod_InstreamFlowRight> rights)
	{
		_rights = rights;
	}

	/// <param name="dataset"> StateMod dataset object. </param>
	/// <returns> List of data that failed specific checks. </returns>
	public virtual StateMod_ComponentValidation validateComponent(StateMod_DataSet dataset)
	{
		StateMod_ComponentValidation validation = new StateMod_ComponentValidation();
		string id = getID();
		string name = getName();
		string riverID = getCgoto();
		string downstreamRiverID = getIfrrdn();
		string dailyID = getCifridy();
		int iifcom = getIifcom();
		// Make sure that basic information is not empty
		if (StateMod_Util.isMissing(id))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Instream flow station identifier is blank.", "Specify a station identifier."));
		}
		if (StateMod_Util.isMissing(name))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Instream flow station \"" + id + "\" name is blank.", "Specify an instream flow station name to clarify data."));
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
		if (StateMod_Util.isMissing(riverID))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Instream flow station \"" + id + "\" river node ID is blank.", "Specify a river node ID to associate the instream flow station with a river network node."));
		}
		else
		{
			// Verify that the river node is in the data set, if the network is available
			if (rinList != null)
			{
				if (StateMod_Util.IndexOf(rinList, riverID) < 0)
				{
					validation.add(new StateMod_ComponentValidationProblem(this,"Instream flow station \"" + id + "\" river network ID (" + riverID + ") is not found in the list of river network nodes.", "Specify a valid river network ID to associate the instream flow station."));
				}
			}
		}
		if (StateMod_Util.isMissing(downstreamRiverID))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Instream flow station \"" + id + "\" downstream river node ID is blank.", "Specify a downstream river node ID to associate the instream flow station with a river network node."));
		}
		else
		{
			// Verify that the river node is in the data set, if the network is available
			if (rinList != null)
			{
				if (StateMod_Util.IndexOf(rinList, downstreamRiverID) < 0)
				{
					validation.add(new StateMod_ComponentValidationProblem(this,"Instream flow station \"" + id + "\" downstream river network ID (" + riverID + ") is not found in the list of river network nodes.", "Specify a valid river network ID to associate the instream flow station downstream node."));
				}
			}
		}
		// Verify that the daily ID is in the data set (daily ID is allowed to be missing)
		if ((dataset != null) && !StateMod_Util.isMissing(dailyID))
		{
			DataSetComponent comp2 = dataset.getComponentForComponentType(StateMod_DataSet.COMP_INSTREAM_STATIONS);
			System.Collections.IList ifsList = (System.Collections.IList)comp2.getData();
			if (dailyID.Equals("0") || dailyID.Equals("3") || dailyID.Equals("4"))
			{
				// OK
			}
			else if ((ifsList != null) && (ifsList.Count > 0))
			{
				// Check the instream flow station list
				if (StateMod_Util.IndexOf(ifsList, dailyID) < 0)
				{
					validation.add(new StateMod_ComponentValidationProblem(this,"Instream flow station \"" + id + "\" daily ID (" + dailyID + ") is not 0, 3, or 4 and is not found in the list of instream flow stations.", "Specify the daily ID as 0, 3, 4, or a matching instream flow station ID."));
				}
			}
		}
		IList<string> choices = getIifcomChoices(false);
		if (StringUtil.indexOfIgnoreCase(choices,"" + iifcom) < 0)
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Instream flow station \"" + id + "\" data type (" + iifcom + ") is invalid.", "Specify the data type as one of " + choices));
		}
		// TODO SAM 2009-06-01) evaluate how to check rights (with getRights() or checking the rights data
		// set component).
		return validation;
	}

	/// <summary>
	/// Write instream flow information to output.  History header information 
	/// is also maintained by calling this routine. </summary>
	/// <param name="infile"> input file from which previous history should be taken </param>
	/// <param name="outfile"> output file to which to write </param>
	/// <param name="theInsf"> list of instream flows to write </param>
	/// <param name="newcomments"> addition comments which should be included in history </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeStateModFile(String infile, String outfile, java.util.List<StateMod_InstreamFlow> theInsf, java.util.List<String> newcomments) throws Exception
	public static void writeStateModFile(string infile, string outfile, IList<StateMod_InstreamFlow> theInsf, IList<string> newcomments)
	{
		writeStateModFile(infile, outfile, theInsf, newcomments, true);
	}

	/// <summary>
	/// Write the instream flow objects to the StateMod file. </summary>
	/// <param name="infile"> input file(original file read from, can be null). </param>
	/// <param name="outfile"> output file(to create or update, can be same as input). </param>
	/// <param name="theInsf"> list of StateMod_InstreamFlow instances. </param>
	/// <param name="newcomments"> Comments to add at the top of the file. </param>
	/// <param name="useDailyData"> Indicates whether daily and extended data(cifridy, iifcom)should be used. </param>
	/// <exception cref="Exception"> if an error occurs </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeStateModFile(String infile, String outfile, java.util.List<StateMod_InstreamFlow> theInsf, java.util.List<String> newcomments, boolean useDailyData) throws Exception
	public static void writeStateModFile(string infile, string outfile, IList<StateMod_InstreamFlow> theInsf, IList<string> newcomments, bool useDailyData)
	{
		string routine = "StateMod_InstreamFlow.writeStateModFile";
		IList<string> commentIndicators = new List<string>(1);
		commentIndicators.Add("#");
		IList<string> ignoredCommentIndicators = new List<string>(1);
		ignoredCommentIndicators.Add("#>");
		PrintWriter @out = null;
		Message.printStatus(2, routine, "Writing instream flows to file \"" + outfile + "\" using \"" + infile + "\" header...");

		// Process the header from the old file...

		try
		{
			@out = IOUtil.processFileHeaders(IOUtil.getPathUsingWorkingDir(infile), IOUtil.getPathUsingWorkingDir(outfile), newcomments, commentIndicators, ignoredCommentIndicators, 0);

			int i;
			string iline;
			string cmnt = "#>";
			IList<object> v = new List<object>(7);
			StateMod_InstreamFlow insf = null;
			string format_0 = "%-12.12s%-24.24s%-12.12s%8d %-12.12s %-12.12s%8d";
			string format_1 = "%-12.12s%-24.24s%-12.12s%8d %-12.12s";

			@out.println(cmnt);
			@out.println(cmnt + " ******************************************************* ");
			@out.println(cmnt + "  StateMod Instream Flow Station File");
			@out.println(cmnt);
			@out.println(cmnt + "  Card format:  (a12,a24,a12,i8,1x,a12,1x,a12,i8)");
			@out.println(cmnt);
			@out.println(cmnt + "  ID           cifrid:  Instream Flow ID");
			@out.println(cmnt + "  Name         cfrnam:  Instream Flow Name");
			@out.println(cmnt + "  Riv ID        cgoto:  Upstream river ID where instream flow is located");
			@out.println(cmnt + "  On/Off       ifrrsw:  Switch; 0=off, 1=on");
			@out.println(cmnt + "  Downstream   ifrrdn:  Downstream river ID where instream flow is located");
			@out.println(cmnt + "                        (blank indicates downstream=upstream)");
			@out.println(cmnt + "  DailyID     cifridy:  Daily instream flow ID (see StateMod doc)");
			@out.println(cmnt + "  DemandType   iifcom:  Demand type switch (see StateMod doc)");
			@out.println(cmnt);
			@out.println(cmnt + " ID        Name                    Riv ID     On/Off   Downstream    DailyID    DemandType");
			@out.println(cmnt + "---------eb----------------------eb----------eb------e-b----------exb----------eb------e");
			@out.println(cmnt + "EndHeader");
			@out.println(cmnt);

			int num = 0;
			if (theInsf != null)
			{
				num = theInsf.Count;
			}
			for (i = 0; i < num; i++)
			{
				insf = theInsf[i];
				if (insf == null)
				{
					continue;
				}
				v.Clear();
				v.Add(insf.getID());
				v.Add(insf.getName());
				v.Add(insf.getCgoto());
				v.Add(new int?(insf.getSwitch()));
				v.Add(insf.getIfrrdn());
				if (useDailyData)
				{
					v.Add(insf.getCifridy());
					v.Add(new int?(insf.getIifcom()));
					iline = StringUtil.formatString(v, format_0);
				}
				else
				{
					iline = StringUtil.formatString(v, format_1);
				}
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
	/// Writes a list of StateMod_InstreamFlow objects to a list file.  A header is 
	/// printed to the top of the file, containing the commands used to generate the 
	/// file.  Any strings in the body of the file that contain the field delimiter will be wrapped in "...". </summary>
	/// <param name="filename"> the name of the file to which the data will be written. </param>
	/// <param name="delimiter"> the delimiter to use for separating field values. </param>
	/// <param name="update"> whether to update an existing file, retaining the current 
	/// header (true) or to create a new file with a new header. </param>
	/// <param name="data"> the list of objects to write. </param>
	/// <param name="newComments"> additional comments to write to the header. </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeListFile(String filename, String delimiter, boolean update, java.util.List<StateMod_InstreamFlow> data, java.util.List<String> newComments) throws Exception
	public static void writeListFile(string filename, string delimiter, bool update, IList<StateMod_InstreamFlow> data, IList<string> newComments)
	{
		int size = 0;
		if (data != null)
		{
			size = data.Count;
		}

		IList<string> fields = new List<string>();
		fields.Add("ID");
		fields.Add("Name");
		fields.Add("UpstreamRiverNodeID");
		fields.Add("OnOff");
		fields.Add("DownstreamRiverNodeID");
		fields.Add("DailyID");
		fields.Add("DemandType");
		int fieldCount = fields.Count;

		string[] names = new string[fieldCount];
		string[] formats = new string[fieldCount];
		int comp = StateMod_DataSet.COMP_INSTREAM_STATIONS;
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
		StateMod_InstreamFlow flo = null;
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
			newComments2.Insert(1,"StateMod instream flow stations as a delimited list file.");
			newComments2.Insert(2,"");
			@out = IOUtil.processFileHeaders(oldFile, IOUtil.getPathUsingWorkingDir(filename), newComments2, commentIndicators, ignoredCommentIndicators, 0);

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
				flo = (StateMod_InstreamFlow)data[i];

				line[0] = StringUtil.formatString(flo.getID(), formats[0]).Trim();
				line[1] = StringUtil.formatString(flo.getName(), formats[1]).Trim();
				line[2] = StringUtil.formatString(flo.getCgoto(), formats[2]).Trim();
				line[3] = StringUtil.formatString(flo.getSwitch(), formats[3]).Trim();
				line[4] = StringUtil.formatString(flo.getIfrrdn(), formats[4]).Trim();
				line[5] = StringUtil.formatString(flo.getCifridy(), formats[5]).Trim();
				line[6] = StringUtil.formatString(flo.getIifcom(), formats[6]).Trim();

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