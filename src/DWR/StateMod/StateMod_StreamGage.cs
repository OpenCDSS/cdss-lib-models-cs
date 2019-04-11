using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

// StateMod_StreamGage - class to store stream gage data

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
// StateMod_StreamGage - class derived from StateMod_Data.  Contains
//	information the stream gage station file (.ris).
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
// 2003-06-04	J. Thomas Sapienza, RTi	Renamed from SMRiverInfo
// 2003-06-10	JTS, RTi		* Folded dumpRiverInfoFile() into
//					  writeRiverInfoFile()
//					* Renamed parseRiverInfoFile() to
//					  readRiverInfoFile()
// 2003-06-23	JTS, RTi		Renamed writeRiverInfoFile() to
//					writeStateModFile()
// 2003-06-26	JTS, RTi		Renamed readRiverInfoFile() to
//					readStateModFile()
// 2003-07-30	SAM, RTi		* Split river station code out of
//					  StateMod_RiverInfo into this
//					  StateMod_RiverStation class to make
//					  management of data cleaner.
//					* Change isDirty() back to setDirty().
// 2003-08-28	SAM, RTi		* Clean up time series data members and
//					  methods.
//					* Clean up parameter names.
//					* Call setDirty() on each object in
//					  addition to the data set component.
// 2003-09-11	SAM, RTi		Rename from StateMod_RiverStation to
//					StateMod_StreamGage and make
//					appropriate changes throughout.
// 2004-03-15	JTS, RTi		Added in some old member variables for
//					use with writing makenet files:
//					* _comment
//					* _node
//					* setNode()
//					* applySetRinCommands()
//					* applySetRisCommands()
//					* _gwmaxr_string
// 2004-07-06	SAM, RTi		* Overload the constructor to allow
//					  initialization to reasonable defaults
//					  or missing.
//					* Remove the above code from Tom since
//					  these features are either from Makenet
//					  (and now in StateDMI) and meant for
//					  the StateMod_RiverNetworkNode class.
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

    //using GeoRecord = RTi.GIS.GeoView.GeoRecord;
    //using HasGeoRecord = RTi.GIS.GeoView.HasGeoRecord;
    using DayTS = RTi.TS.DayTS;
    using MonthTS = RTi.TS.MonthTS;
    using DataSetComponent = RTi.Util.IO.DataSetComponent;
    using IOUtil = RTi.Util.IO.IOUtil;
    using Message = RTi.Util.Message.Message;
    using StringUtil = RTi.Util.String.StringUtil;

    public class StateMod_StreamGage : StateMod_Data //, ICloneable, IComparable<StateMod_Data>, HasGeoRecord, StateMod_ComponentValidator
    {

        //protected String 	_cgoto;		// River node for stream station.

        /// <summary>
        /// Monthly historical TS from the .rih file that is associated with the
        /// .ris station - only streamflow gages in the .ris have these data
        /// </summary>
        protected internal MonthTS _historical_MonthTS;

        /// <summary>
        /// Monthly base flow time series, for use with the river station (.ris)
        /// file, read from the .xbm/.rim file.
        /// </summary>
        protected internal MonthTS _baseflow_MonthTS;

        /// <summary>
        /// Daily base flow time series, read from the .rid file.
        /// </summary>
        protected internal DayTS _baseflow_DayTS;

        /// <summary>
        /// Daily historical TS from the .riy file that is associated with the .ris
        /// station - only streamflow gages in the .riy have these data
        /// </summary>
        protected internal DayTS _historical_DayTS;

        /// <summary>
        /// Used with .ris (column 4) - daily stream station identifier.
        /// </summary>
        protected internal string _crunidy;

        /// <summary>
        /// Reference to spatial data for this river station.  Currently not cloned.
        /// </summary>
        //protected internal GeoRecord _georecord;

        /// <summary>
        /// The StateMod_DataSet component type for the node.  At some point the related object reference
        /// may also be added, but there are cases when this is not known (only the type is
        /// known, for example in StateDMI).
        /// </summary>
        protected internal int _related_smdata_type;

        /// <summary>
        /// Second related type.  This is only used for D&W node types and should be set to the
        /// well stations component type.
        /// </summary>
        protected internal int _related_smdata_type2;

        /// <summary>
        /// Constructor for stream gage station.
        /// The time series are set to null and other information to empty strings or other reasonable defaults.
        /// </summary>
        public StateMod_StreamGage() : this(true)
        {
        }

        /// <summary>
        /// Constructor for stream gage station. </summary>
        /// <param name="initialize_defaults"> If true, the time series are set to null and other
        /// information to empty strings or other reasonable defaults - this is suitable
        /// for the StateMod GUI when creating new instances.  If false, the
        /// data values are set to missing - this is suitable for use with StateDMI, where
        /// data will be filled with commands. </param>
        public StateMod_StreamGage(bool initialize_defaults) : base()
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
        //public virtual bool changed()
        //{
        //    if (_original == null)
        //    {
        //        return true;
        //    }

        //    if (CompareTo(_original) == 0)
        //    {
        //        return false;
        //    }

        //    return true;
        //}

        /// <summary>
        /// Clones the data object. </summary>
        /// <returns> a cloned object. </returns>
        //public override object clone()
        //{
        //    StateMod_StreamGage s = (StateMod_StreamGage)base.clone();
        //    s._isClone = true;
        //    return s;
        //}

        /// <summary>
        /// Compares this object to another StateMod_StreamGage object. </summary>
        /// <param name="data"> the object to compare against. </param>
        /// <returns> 0 if they are the same, 1 if this object is greater than the other
        /// object, or -1 if it is less. </returns>
        //public virtual int CompareTo(StateMod_Data data)
        //{
        //    int res = base.CompareTo(data);
        //    if (res != 0)
        //    {
        //        return res;
        //    }

        //    StateMod_StreamGage s = (StateMod_StreamGage)data;

        //    res = _cgoto.CompareTo(s._cgoto);
        //    if (res != 0)
        //    {
        //        return res;
        //    }

        //    res = _crunidy.CompareTo(s._crunidy);
        //    if (res != 0)
        //    {
        //        return res;
        //    }

        //    if (_related_smdata_type < s._related_smdata_type)
        //    {
        //        return -1;
        //    }
        //    else if (_related_smdata_type > s._related_smdata_type)
        //    {
        //        return 1;
        //    }

        //    if (_related_smdata_type2 < s._related_smdata_type2)
        //    {
        //        return -1;
        //    }
        //    else if (_related_smdata_type2 > s._related_smdata_type2)
        //    {
        //        return 1;
        //    }

        //    return 0;
        //}

        /// <summary>
        /// Connect the historical monthly and daily TS pointers to the appropriate TS
        /// for all the elements in the list of StateMod_StreamGage objects. </summary>
        /// <param name="rivs"> list of StateMod_StreamGage (e.g., as read from StateMod .ris file). </param>
        //public static void connectAllTS(IList<StateMod_StreamGage> rivs, IList<MonthTS> historical_MonthTS, IList<DayTS> historical_DayTS, IList<MonthTS> baseflow_MonthTS, IList<DayTS> baseflow_DayTS)
        //{
        //    if (rivs == null)
        //    {
        //        return;
        //    }
        //    StateMod_StreamGage riv;
        //    int size = rivs.Count;
        //    for (int i = 0; i < size; i++)
        //    {
        //        riv = rivs[i];
        //        if (historical_MonthTS != null)
        //        {
        //            riv.connectHistoricalMonthTS(historical_MonthTS);
        //        }
        //        if (historical_DayTS != null)
        //        {
        //            riv.connectHistoricalDayTS(historical_DayTS);
        //        }
        //        if (baseflow_MonthTS != null)
        //        {
        //            riv.connectBaseflowMonthTS(baseflow_MonthTS);
        //        }
        //        if (baseflow_DayTS != null)
        //        {
        //            riv.connectBaseflowDayTS(baseflow_DayTS);
        //        }
        //    }
        //}

        /// <summary>
        /// Connect the daily base streamflow TS pointer to the appropriate TS in the list.
        /// A connection is made if the node identifier matches the time series location. </summary>
        /// <param name="tslist"> list of DayTS. </param>
        //private void connectBaseflowDayTS(IList<DayTS> tslist)
        //{
        //    if (tslist == null)
        //    {
        //        return;
        //    }
        //    DayTS ts;
        //    int size = tslist.Count;
        //    _baseflow_DayTS = null;
        //    for (int i = 0; i < size; i++)
        //    {
        //        ts = tslist[i];
        //        if (ts == null)
        //        {
        //            continue;
        //        }
        //        if (_id.Equals(ts.getLocation(), StringComparison.OrdinalIgnoreCase))
        //        {
        //            // Set this because the original file does not have...
        //            ts.setDescription(getName());
        //            _baseflow_DayTS = ts;
        //            break;
        //        }
        //    }
        //}

        /// <summary>
        /// Connect monthly baseflow time series. </summary>
        /// <param name="tslist"> baseflow time series.  </param>
        //public virtual void connectBaseflowMonthTS(IList<MonthTS> tslist)
        //{
        //    if (tslist == null)
        //    {
        //        return;
        //    }
        //    int num_TS = tslist.Count;

        //    _baseflow_MonthTS = null;
        //    MonthTS ts = null;
        //    for (int i = 0; i < num_TS; i++)
        //    {
        //        ts = tslist[i];
        //        if (ts == null)
        //        {
        //            continue;
        //        }
        //        if (_id.Equals(ts.getLocation(), StringComparison.OrdinalIgnoreCase))
        //        {
        //            // Set this because the original file does not have...
        //            ts.setDescription(getName());
        //            _baseflow_MonthTS = ts;
        //            break;
        //        }
        //    }
        //}

        /// <summary>
        /// Connect the historical daily TS pointer to the appropriate TS in the Vector.
        /// A connection is made if the node identifier matches the time series location. </summary>
        /// <param name="tslist"> Vector of DayTS. </param>
        //private void connectHistoricalDayTS(IList<DayTS> tslist)
        //{
        //    if (tslist == null)
        //    {
        //        return;
        //    }
        //    DayTS ts;
        //    _historical_DayTS = null;
        //    int size = tslist.Count;
        //    for (int i = 0; i < size; i++)
        //    {
        //        ts = tslist[i];
        //        if (ts == null)
        //        {
        //            continue;
        //        }
        //        if (_id.Equals(ts.getLocation(), StringComparison.OrdinalIgnoreCase))
        //        {
        //            // Set this because the original file does not have...
        //            ts.setDescription(getName());
        //            _historical_DayTS = ts;
        //            break;
        //        }
        //    }
        //}

        /// <summary>
        /// Connect the historical monthly TS pointer to the appropriate TS.
        /// A connection is made if the node identifier matches the time series location. </summary>
        /// <param name="tslist"> Vector of MonthTS. </param>
        //public virtual void connectHistoricalMonthTS(IList<MonthTS> tslist)
        //{
        //    if (tslist == null)
        //    {
        //        return;
        //    }
        //    MonthTS ts;
        //    _historical_MonthTS = null;
        //    int size = tslist.Count;
        //    for (int i = 0; i < size; i++)
        //    {
        //        ts = tslist[i];
        //        if (ts == null)
        //        {
        //            continue;
        //        }
        //        if (_id.Equals(ts.getLocation(), StringComparison.OrdinalIgnoreCase))
        //        {
        //            // The name is usually not set when reading the time series...
        //            ts.setDescription(getName());
        //            _historical_MonthTS = ts;
        //            break;
        //        }
        //    }
        //}

        /// <summary>
        /// Creates a copy of the object for later use in checking to see if it was changed in a GUI.
        /// </summary>
        //public virtual void createBackup()
        //{
        //    _original = (StateMod_StreamGage)clone();
        //    ((StateMod_StreamGage)_original)._isClone = false;
        //    _isClone = true;
        //}

        /// <summary>
        /// Finalize data for garbage collection.
        /// </summary>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: protected void finalize() throws Throwable
        ~StateMod_StreamGage()
        {
            _cgoto = null;
            _historical_MonthTS = null;
            _historical_DayTS = null;
            _baseflow_MonthTS = null;
            _baseflow_DayTS = null;
            _crunidy = null;
            //_georecord = null;
            //JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
            //		base.finalize();
        }

        /// <summary>
        /// Return the daily baseflow file associated with the *.ris node. </summary>
        /// <returns> the daily baseflow file associated with the *.ris node.
        /// Return null if no time series is available. </returns>
        public virtual DayTS getBaseflowDayTS()
        {
            return _baseflow_DayTS;
        }

        /// <summary>
        /// Return the monthly baseflow file associated with the *.ris node. </summary>
        /// <returns> the monthly baseflow file associated with the *.ris node.
        /// Return null if no time series is available. </returns>
        public virtual MonthTS getBaseflowMonthTS()
        {
            return _baseflow_MonthTS;
        }

        /// <summary>
        /// Get the river node identifier for the stream gage. </summary>
        /// <returns> the river node identifier. </returns>
        //public override string getCgoto()
        //{
        //    return _cgoto;
        //}

        /// <summary>
        /// Get the daily stream station identifier used with the stream gage station file. </summary>
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
            // TODO KAT 2007-04-16 When specific checks are added to checkComponentData
            // return the header for that data here
            return new string[] { };
        }

        /// <summary>
        /// Get the geographical data associated with the diversion. </summary>
        /// <returns> the GeoRecord for the diversion. </returns>
        //public virtual GeoRecord getGeoRecord()
        //{
        //    return _georecord;
        //}

        /// <summary>
        /// Get the daily TS pointer (typically only if storing .ris). </summary>
        /// <returns> the daily TS pointer. </returns>
        public virtual DayTS getHistoricalDayTS()
        {
            return _historical_DayTS;
        }

        /// <summary>
        /// Get the historical monthly TS pointer (typically only if storing .ris). </summary>
        /// <returns> the historical monthly TS pointer. </returns>
        public virtual MonthTS getHistoricalMonthTS()
        {
            return _historical_MonthTS;
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
        /// information to empty strings or other reasonable defaults - this is suitable
        /// for the StateMod GUI when creating new instances.  If false, the
        /// data values are set to missing - this is suitable for use with StateDMI, where
        /// data will be filled with commands. </param>
        private void initialize(bool initialize_defaults)
        {
            _smdata_type = StateMod_DataSet.COMP_STREAMGAGE_STATIONS;
            _cgoto = "";
            _historical_MonthTS = null;
            _historical_DayTS = null;
            _baseflow_MonthTS = null;
            _baseflow_DayTS = null;
            if (initialize_defaults)
            {
                // Set to reasonable defaults...
                _crunidy = "0"; // Use monthly data
            }
            else
            {
                // Initialize to missing
                _crunidy = "";
            }
            //_georecord = null;
        }

        /// <summary>
        /// Read the stream gage station file and store return a Vector of StateMod_StreamGage. </summary>
        /// <returns> a list of StateMod_StreamGage. </returns>
        /// <param name="filename"> Name of file to read. </param>
        /// <exception cref="Exception"> if there is an error reading the file. </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public static java.util.List<StateMod_StreamGage> readStateModFile(String filename) throws Exception
        public static IList<StateMod_StreamGage> readStateModFile(string filename)
        {
            string rtn = "StateMod_StreamGage.readStateModFile";
            IList<StateMod_StreamGage> theRivs = new List<StateMod_StreamGage>();
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
            format_0w = new int[5];
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

                    // allocate new StateMod_StreamGage node
                    StateMod_StreamGage aRiverNode = new StateMod_StreamGage();

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
                // Clean up...
                Message.printWarning(3, rtn, "Error reading \"" + filename + "\" at line " + linecount);
                throw e;
            }
            finally
            {
                // Clean up...
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
        //public override void restoreOriginal()
        //{
        //    StateMod_StreamGage s = (StateMod_StreamGage)_original;
        //    base.restoreOriginal();
        //    _related_smdata_type = s._related_smdata_type;
        //    _related_smdata_type2 = s._related_smdata_type2;
        //    _crunidy = s._crunidy;
        //    _isClone = false;
        //    _original = null;
        //}

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
        /// Set the river node identifier. </summary>
        /// <param name="cgoto"> River node identifier. </param>
        //public override void setCgoto(string cgoto)
        //{
        //    if ((!string.ReferenceEquals(cgoto, null)) && !cgoto.Equals(_cgoto))
        //    {
        //        _cgoto = cgoto;
        //        setDirty(true);
        //        if (!_isClone && _dataset != null)
        //        {
        //            _dataset.setDirty(_smdata_type, true);
        //        }
        //    }
        //}

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
        /// Set the geographic information object associated with the diversion. </summary>
        /// <param name="georecord"> Geographic record associated with the diversion. </param>
        //public virtual void setGeoRecord(GeoRecord georecord)
        //{
        //    _georecord = georecord;
        //}

        /// <summary>
        /// Set the daily historical TS pointer. </summary>
        /// <param name="ts"> Daily historical TS. </param>
        public virtual void setHistoricalDayTS(DayTS ts)
        {
            _historical_DayTS = ts;
        }

        /// <summary>
        /// Set the historical monthly TS pointer. </summary>
        /// <param name="ts"> historical monthly TS. </param>
        public virtual void setHistoricalMonthTS(MonthTS ts)
        {
            _historical_MonthTS = ts;
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
        //public virtual StateMod_ComponentValidation validateComponent(StateMod_DataSet dataset)
        //{
        //    StateMod_ComponentValidation validation = new StateMod_ComponentValidation();
        //    string id = getID();
        //    string name = getName();
        //    string riverID = getCgoto();
        //    string dailyID = getCrunidy();
        //    // Make sure that basic information is not empty
        //    if (StateMod_Util.isMissing(id))
        //    {
        //        validation.add(new StateMod_ComponentValidationProblem(this, "Stream gage identifier is blank.", "Specify a station identifier."));
        //    }
        //    if (StateMod_Util.isMissing(name))
        //    {
        //        validation.add(new StateMod_ComponentValidationProblem(this, "Stream gage \"" + id + "\" name is blank.", "Specify a station name to clarify data."));
        //    }
        //    if (StateMod_Util.isMissing(riverID))
        //    {
        //        validation.add(new StateMod_ComponentValidationProblem(this, "Stream gage \"" + id + "\" river ID is blank.", "Specify a river ID to associate the station with a river network node."));
        //    }
        //    else
        //    {
        //        // Verify that the river node is in the data set, if the network is available
        //        if (dataset != null)
        //        {
        //            DataSetComponent comp = dataset.getComponentForComponentType(StateMod_DataSet.COMP_RIVER_NETWORK);
        //            System.Collections.IList rinList = (System.Collections.IList)comp.getData();
        //            if ((rinList != null) && (rinList.Count > 0))
        //            {
        //                if (StateMod_Util.IndexOf(rinList, riverID) < 0)
        //                {
        //                    validation.add(new StateMod_ComponentValidationProblem(this, "Stream gage \"" + id + "\" river network ID (" + riverID + ") is not found in the list of river network nodes.", "Specify a valid river network ID to associate the station with a river network node."));
        //                }
        //            }
        //        }
        //    }
        //    // Verify that the daily ID is in the data set
        //    if (!StateMod_Util.isMissing(dailyID))
        //    {
        //        if (dataset != null)
        //        {
        //            DataSetComponent comp = dataset.getComponentForComponentType(StateMod_DataSet.COMP_STREAMGAGE_STATIONS);
        //            System.Collections.IList risList = (System.Collections.IList)comp.getData();
        //            if ((risList != null) && (risList.Count > 0))
        //            {
        //                if (!dailyID.Equals("0") && !dailyID.Equals("3") && !dailyID.Equals("4") && (StateMod_Util.IndexOf(risList, dailyID) < 0))
        //                {
        //                    validation.add(new StateMod_ComponentValidationProblem(this, "Stream gage \"" + id + "\" daily ID (" + dailyID + ") is not 0, 3, or 4 and is not found in the list of stream gages.", "Specify the daily ID as 0, 3, 4, or that matches a stream gage ID."));
        //                }
        //            }
        //        }
        //    }
        //    return validation;
        //}

        /// <summary>
        /// Write the new (updated) stream gage stations file.  If an original file is
        /// specified, then the original header is carried into the new file. </summary>
        /// <param name="infile"> Name of old file or null if no old file to update. </param>
        /// <param name="outfile"> Name of new file to create (can be the same as the old file). </param>
        /// <param name="theRivs"> list of StateMod_StreamGage to write. </param>
        /// <param name="newcomments"> New comments to write in the file header. </param>
        /// <param name="do_daily"> Indicates whether daily modeling fields should be written. </param>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public static void writeStateModFile(String infile, String outfile, java.util.List<StateMod_StreamGage> theRivs, java.util.List<String> newcomments, boolean do_daily) throws Exception
        public static void writeStateModFile(string infile, string outfile, IList<StateMod_StreamGage> theRivs, IList<string> newcomments, bool do_daily)
        {
            TextWriter @out = null;
            IList<string> commentIndicators = new List<string>(1);
            commentIndicators.Add("#");
            IList<string> ignoredCommentIndicators = new List<string>(1);
            ignoredCommentIndicators.Add("#>");
            string routine = "StateMod_StreamGage.writeStateModFile";

            if (Message.isDebugOn)
            {
                Message.printDebug(2, routine, "Writing stream gage stations to file \"" + outfile + "\" using \"" + infile + "\" header...");
            }

            try
            {
                // Process the header from the old file...
                @out = IOUtil.processFileHeaders(IOUtil.getPathUsingWorkingDir(infile), IOUtil.getPathUsingWorkingDir(outfile), newcomments, commentIndicators, ignoredCommentIndicators, 0);

                string cmnt = "#>";
                string iline = null;
                string format = null;
                StateMod_StreamGage riv = null;

                @out.WriteLine(cmnt + " *******************************************************");
                @out.WriteLine(cmnt + "  Stream Gage Station File");
                @out.WriteLine(cmnt);
                @out.WriteLine(cmnt + "     format:  (a12, a24, a12, 1x, a12)");
                @out.WriteLine(cmnt);
                @out.WriteLine(cmnt + "  ID         crunid:  Station ID");
                @out.WriteLine(cmnt + "  Name       runnam:  Station name");
                @out.WriteLine(cmnt + "  River ID    cgoto:  River node with stream gage");
                @out.WriteLine(cmnt + "  Daily ID  crunidy:  Daily stream station ID.");
                @out.WriteLine(cmnt);
                @out.WriteLine(cmnt + "    ID              Name           River ID     Daily ID   ");
                @out.WriteLine(cmnt + "---------eb----------------------eb----------exb----------e");
                if (do_daily)
                {
                    format = "%-12.12s%-24.24s%-12.12s %-12.12s";
                }
                else
                {
                    format = "%-12.12s%-24.24s%-12.12s";
                }
                @out.WriteLine(cmnt);
                @out.WriteLine(cmnt + "EndHeader");
                @out.WriteLine(cmnt);

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
                    v.Add(riv.getCgoto());
                    if (do_daily)
                    {
                        v.Add(riv.getCrunidy());
                    }
                    iline = StringUtil.formatString(v, format);
                    @out.WriteLine(iline);
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
                    @out.Flush();
                    @out.Close();
                }
            }
        }

        /// <summary>
        /// Writes a list of StateMod_StreamGage objects to a list file.  A header is 
        /// printed to the top of the file, containing the commands used to generate the 
        /// file.  Any strings in the body of the file that contain the field delimiter will be wrapped in "...". </summary>
        /// <param name="filename"> the name of the file to which the data will be written. </param>
        /// <param name="delimiter"> the delimiter to use for separating field values. </param>
        /// <param name="update"> whether to update an existing file, retaining the current 
        /// header (true) or to create a new file with a new header. </param>
        /// <param name="data"> the list of objects to write. </param>
        /// <param name="newComments"> list of comments to add at the top of the file. </param>
        /// <exception cref="Exception"> if an error occurs. </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public static void writeListFile(String filename, String delimiter, boolean update, java.util.List<StateMod_StreamGage> data, java.util.List<String> newComments) throws Exception
        //public static void writeListFile(string filename, string delimiter, bool update, IList<StateMod_StreamGage> data, IList<string> newComments)
        //{
        //    int size = 0;
        //    if (data != null)
        //    {
        //        size = data.Count;
        //    }

        //    IList<string> fields = new List<string>();
        //    fields.Add("ID");
        //    fields.Add("Name");
        //    fields.Add("NodeID");
        //    fields.Add("DailyID");
        //    int fieldCount = fields.Count;

        //    string[] names = new string[fieldCount];
        //    string[] formats = new string[fieldCount];
        //    int comp = StateMod_DataSet.COMP_STREAMGAGE_STATIONS;
        //    string s = null;
        //    for (int i = 0; i < fieldCount; i++)
        //    {
        //        s = fields[i];
        //        names[i] = StateMod_Util.lookupPropValue(comp, "FieldName", s);
        //        formats[i] = StateMod_Util.lookupPropValue(comp, "Format", s);
        //    }

        //    string oldFile = null;
        //    if (update)
        //    {
        //        oldFile = IOUtil.getPathUsingWorkingDir(filename);
        //    }

        //    int j = 0;
        //    PrintWriter @out = null;
        //    StateMod_StreamGage gage = null;
        //    IList<string> commentIndicators = new List<string>(1);
        //    commentIndicators.Add("#");
        //    IList<string> ignoredCommentIndicators = new List<string>(1);
        //    ignoredCommentIndicators.Add("#>");
        //    string[] line = new string[fieldCount];
        //    StringBuilder buffer = new StringBuilder();

        //    try
        //    {
        //        // Add some basic comments at the top of the file.  Do this to a copy of the
        //        // incoming comments so that they are not modified in the calling code.
        //        IList<string> newComments2 = null;
        //        if (newComments == null)
        //        {
        //            newComments2 = new List<string>();
        //        }
        //        else
        //        {
        //            newComments2 = new List<string>(newComments);
        //        }
        //        newComments2.Insert(0, "");
        //        newComments2.Insert(1, "StateMod stream gage stations as a delimited list file.");
        //        newComments2.Insert(2, "");
        //        @out = IOUtil.processFileHeaders(oldFile, IOUtil.getPathUsingWorkingDir(filename), newComments2, commentIndicators, ignoredCommentIndicators, 0);

        //        for (int i = 0; i < fieldCount; i++)
        //        {
        //            if (i > 0)
        //            {
        //                buffer.Append(delimiter);
        //            }
        //            buffer.Append("\"" + names[i] + "\"");
        //        }

        //        @out.println(buffer.ToString());

        //        for (int i = 0; i < size; i++)
        //        {
        //            gage = (StateMod_StreamGage)data[i];

        //            line[0] = StringUtil.formatString(gage.getID(), formats[0]).Trim();
        //            line[1] = StringUtil.formatString(gage.getName(), formats[1]).Trim();
        //            line[2] = StringUtil.formatString(gage.getCgoto(), formats[2]).Trim();
        //            line[3] = StringUtil.formatString(gage.getCrunidy(), formats[3]).Trim();

        //            buffer = new StringBuilder();
        //            for (j = 0; j < fieldCount; j++)
        //            {
        //                if (j > 0)
        //                {
        //                    buffer.Append(delimiter);
        //                }
        //                if (line[j].IndexOf(delimiter, StringComparison.Ordinal) > -1)
        //                {
        //                    line[j] = "\"" + line[j] + "\"";
        //                }
        //                buffer.Append(line[j]);
        //            }

        //            @out.println(buffer.ToString());
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        throw e;
        //    }
        //    finally
        //    {
        //        if (@out != null)
        //        {
        //            @out.flush();
        //            @out.close();
        //        }
        //    }
        //}

    }

}