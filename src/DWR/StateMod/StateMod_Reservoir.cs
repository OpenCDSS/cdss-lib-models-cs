using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

// StateMod_Reservoir - class for reservoir station data

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
// StateMod_Reservoir - class derived from StateMod_Data.  Contains information 
//	read from the reservoir file.
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
// 
// 27 Aug 1997	Catherine E.		Created initial version of class.
//		Nutting-Lane, RTi
// 21 Dec 1998	CEN, RTi		Added throws IOException to read/write
//					routines.
// 25 Oct 1999	CEN, RTi		Adding daily id.
// 10 Nov 1999	CEN, RTi		Added time series pointers and set/get
//					routines for ts similar to diversions
// 19 Feb 2001	Steven A. Malers, RTi	Code review.  Clean up javadoc.  Add
//					finalize.  Handle nulls and set unused
//					variables to null.  Alphabetize methods.
//					Change IO to IOUtil.  Add a switch to
//					allow printing with or without daily
//					data.  Update output header.  Change
//					area capacity output to be printed with
//					numbers 1... rather than 0...
// 02 Mar 2001	SAM, RTi		Change reservoir curve number back to
//					start with zero but make sure the
//					zero record has zeros all the way
//					across to allow interpolation.
//					Add insertAreaCapAt()to allow
//					insertion.
// 22 Aug 2001	SAM, RTi		When writing the reservoir area/capacity
//					file, if the area or capacity is less
//					than 100, write to 2 decimal points.
//					Otherwise, write as before.  This
//					prevents StateMod from complaining.
//					Print counter for curve to 3 digits.
// 2001-12-27	SAM, RTi		Update to use new fixedRead()to
//					improve performance.
// 2002-09-09	SAM, RTi		Add GeoRecord reference to allow 2-way
//					connection between spatial and StateMod
//					data.
// 2002-09-19	SAM, RTi		Use isDirty() instead of setDirty() to
//					indicate edits.
// 2002-11-01	SAM, RTi		Minor revision to add description of
//					first line to the output header.
//------------------------------------------------------------------------------
// 2003-06-04	J. Thomas Sapienza	Renamed from SMReservoir to 
//					StateMod_Reservoir
// 2003-06-10	JTS, RTi		* Folded dumpReservoirFile() into
//					  writeReservoirFile()
//					* Renamed parseReservoirFile() to
//					  readReservoirFile()
// 2003-06-23	JTS, RTi		Renamed writeReservoirFile() to
//					writeStateModFile()
// 2003-06-26	JTS, RTi		Renamed readReservoirFile() to
//					readStateModFile()
// 2003-07-15	JTS, RTi		Changed to use new dataset design.
// 2003-08-03	SAM, RTi		Changed isDirty() back to setDirty().
// 2003-08-15	SAM, RTi		* Changed GeoRecordNoSwing() to
//					  GeoRecord.
//					* Change StateMod_Climate to
//					  StateMod_ReservoirClimate.
// 2003-08-18	SAM, RTi		* Clean up the data members and method
//					  names for time series - daily were
//					  not being properly handled.
//					* Add hasXXXTS() to indicate whether the
//					  reservoir has climate data time
//					  series.
//					* Add getXXXTS() to get climate time
//					  series.
//					* Change so StateMod_ReservoirClimate
//					  uses the StateMod_Data base class for
//					  the identifiers.
// 2003-08-28	SAM, RTi		* Change water rights to be a simple
//					  Vector, not a linked list.
//					* Call setDirty() for the individual
//					  objects as well as the component.
//					* Remove data members for numbers of
//					  items - these can be determined from
//					  the data Vectors as needed.
//					* Clean up Javadoc.
//					* Alphabetize the methods.
//					* Change getRes* setRes* to remove the
//					  "Res" - it is redundant.
// 2003-09-18	SAM, RTi		Change reservoir accounts to use the
//					base class for the name and assign
//					sequential integers for the account
//					ID.
// 2003-10-10	SAM, RTi		Add disconnectRights().
// 2003-10-15	SAM, RTi		Change some initial values to agree
//					with the old SMGUI for new instance.
// 2004-06-05	SAM, RTi		* Add methods to handle collections,
//					  similar to StateCU locations.
// 2004-07-02	SAM, RTi		* Overload the constructor to indicate
//					  whether reasonable defaults should be
//					  assigned.
//					* Add getRdateChoices() and
//					  getRdateDefault() to help provide
//					  information for GUIs.
//					* Add getIresswChoices() and
//					  getIresswDefault() to help provide
//					  information for GUIs.
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
// 2004-09-09	SAM, RTi		When reading and writing, adjust the
//					file paths using the working directory.
// 2004-11-12	SAM, RTi		Remove "Fill #" from second line in
//					output header.
// 2005-02-01	SAM, RTi		The writeStateModFile() method was
//					automatically adding a dead storage
//					account as the last account, if dead
//					storage was specified, and it was
//					decrementing the account numbers by the
//					dead storage value.  Remove this code
//					and include options in StateDMI -
//					handling in the write method is
//					confusing.
// 2005-03-30	JTS, RTi		* Added getCollectionPartType().
//					* Added getCollectionYears().
// 2005-04-18	JTS, RTi		Added writeListFile().
// 2005-04-19	JTS, RTi		Added writeCollectionListFile().
// 2005-05-06	SAM, RTi		Correct a couple of typos in StateMod
//					data set components for writing the
//					delimited files.
// 2006-06-13	SAM, RTi		Change the names of secondary list files
//					to be more appropriate.
// 2006-08-15	SAM, RTi		Fix so that if target time series file
//					has one time series for a reservoir,
//					then assign to the maximum target and
//					leave the minimum as null.
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
    using TS = RTi.TS.TS;
    using TSUtil = RTi.TS.TSUtil;
    using DataSetComponent = RTi.Util.IO.DataSetComponent;
    using IOUtil = RTi.Util.IO.IOUtil;
    using Message = RTi.Util.Message.Message;
    using StringUtil = RTi.Util.String.StringUtil;

    /// <summary>
    /// The Reservoir class holds data for entries in the StateMod reservoir station
    /// file.  Secondary data classes are used in cases where lists of data are used.
    /// </summary>
    public class StateMod_Reservoir : StateMod_Data //, ICloneable, IComparable<StateMod_Data>, HasGeoRecord, StateMod_ComponentValidator
    {
        /// <summary>
        /// date for one fill rule admin
        /// </summary>
        protected internal double _rdate;

        /// <summary>
        /// minimum reservoir content
        /// </summary>
        protected internal double _volmin;

        /// <summary>
        /// Maximum reservoir content
        /// </summary>
        protected internal double _volmax;

        /// <summary>
        /// Maximum reservoir release
        /// </summary>
        protected internal double _flomax;

        /// <summary>
        /// Dead storage in reservoir
        /// </summary>
        protected internal double _deadst;

        /// <summary>
        /// List of owners
        /// </summary>
        protected internal IList<StateMod_ReservoirAccount> _owners;

        /// <summary>
        /// Daily id
        /// </summary>
        protected internal string _cresdy;

        /// <summary>
        /// List of evap/precip stations
        /// </summary>
        protected internal IList<StateMod_ReservoirClimate> _climate_Vector;

        /// <summary>
        /// List of area capacity values
        /// </summary>
        protected internal IList<StateMod_ReservoirAreaCap> _areacapvals;

        /// <summary>
        /// List of reservoir rights
        /// </summary>
        protected internal IList<StateMod_ReservoirRight> _rights;

        /// <summary>
        /// End of month content time series.
        /// </summary>
        protected internal MonthTS _content_MonthTS;

        /// <summary>
        /// End of day content time series.
        /// </summary>
        protected internal DayTS _content_DayTS;

        /// <summary>
        /// Minimum target time series (Monthly).
        /// </summary>
        protected internal MonthTS _mintarget_MonthTS;

        /// <summary>
        /// Maximum target time series (Monthly).
        /// </summary>
        protected internal MonthTS _maxtarget_MonthTS;

        /// <summary>
        /// Minimum target time series (Daily).
        /// </summary>
        protected internal DayTS _mintarget_DayTS;

        /// <summary>
        /// Maximum target time series (Daily).
        /// </summary>
        protected internal DayTS _maxtarget_DayTS;

        /// <summary>
        /// Link to spatial data -- currently NOT cloned.
        /// </summary>
        //protected internal GeoRecord _georecord;

        // Collections are set up to be specified by year, although currently for
        // reservoir collections are always the same for the full period.

        /// <summary>
        /// Types of collections.  An aggregate merges the water rights whereas
        /// a system keeps all the water rights but just has one ID.
        /// </summary>
        public static string COLLECTION_TYPE_AGGREGATE = "Aggregate";
        public static string COLLECTION_TYPE_SYSTEM = "System";

        private string __collection_type = StateMod_Util.MISSING_STRING;

        /// <summary>
        /// Used by DMI software - currently no options.
        /// </summary>
        private string __collection_part_type = "Reservoir";

        /// <summary>
        /// The identifiers for data that are collected - null if not a collection location.
        /// This is a list of List where the __collection_year is the first dimension.
        /// This is ugly but need to use the code to see if it can be made cleaner.
        /// </summary>
        private IList<IList<string>> __collection_Vector = null;

        /// <summary>
        /// An array of years that correspond to the aggregate/system.  Reservoirs currently only have one year.
        /// </summary>
        private int[] __collection_year = null;

        /// <summary>
        /// Construct and initialize data to reasonable defaults where appropriate.
        /// 
        /// </summary>
        public StateMod_Reservoir() : this(true)
        {
        }

        /// <summary>
        /// Construct and initialize data to reasonable defaults where appropriate. </summary>
        /// <param name="initialize_defaults"> If true, initialize data to reasonable defaults
        /// (e.g., zero dead storage) - this is suitable for defaults in the StateMod GUI.
        /// If false, don't initialize data - this is suitable for filling in StateDMI.
        ///  </param>
        public StateMod_Reservoir(bool initialize_defaults) : base()
        {
            initialize(initialize_defaults);
        }

        /// <summary>
        /// Add owner (account). </summary>
        /// <param name="owner"> StateMod_ReservoirAccount to add. </param>
        public virtual void addAccount(StateMod_ReservoirAccount owner)
        {
            if (owner != null)
            {
                _owners.Add(owner);
                setDirty(true);
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_RESERVOIR_STATIONS, true);
                }
            }
        }

        /// <summary>
        /// Add AreaCap. </summary>
        /// <param name="areacap"> StateMod_ReservoirAreaCap to add. </param>
        public virtual void addAreaCap(StateMod_ReservoirAreaCap areacap)
        {
            if (areacap != null)
            {
                _areacapvals.Add(areacap);
                setDirty(true);
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_RESERVOIR_STATIONS, true);
                }
            }
        }

        /// <summary>
        /// Add climate. </summary>
        /// <param name="climate"> StateMod_ReservoirClimate to add. </param>
        public virtual void addClimate(StateMod_ReservoirClimate climate)
        {
            if (climate != null)
            {
                _climate_Vector.Add(climate);
                setDirty(true);
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_RESERVOIR_STATIONS, true);
                }
            }
        }

        /// <summary>
        /// This set of routines don't actually add an element to an array.  They already
        /// exist as part of a list of StateMod_ReservoirRight.  We are just connecting pointers.
        /// </summary>
        public static void connectAllRights(IList<StateMod_Reservoir> reservoirs, IList<StateMod_ReservoirRight> rights)
        {
            if (reservoirs == null)
            {
                return;
            }
            int i, num_res = reservoirs.Count;

            StateMod_Reservoir res = null;
            for (i = 0; i < num_res; i++)
            {
                res = reservoirs[i];
                if (res == null)
                {
                    continue;
                }
                res.connectRights(rights);
            }
            res = null;
        }

        /// <summary>
        /// Connect the rights to this instance of a reservoir. </summary>
        /// <param name="rights"> Vector of all reservoir rights. </param>
        public virtual void connectRights(IList<StateMod_ReservoirRight> rights)
        {
            if (rights == null)
            {
                return;
            }
            int i;
            int num_rights = rights.Count;

            StateMod_ReservoirRight right;
            for (i = 0; i < num_rights; i++)
            {
                right = rights[i];
                if (right == null)
                {
                    continue;
                }
                if (_id.Equals(right.getCgoto()))
                {
                    _rights.Add(right);
                }
            }
        }

        /// <summary>
        /// Initialize data members. </summary>
        /// <param name="initialize_defaults"> If true, initialize data to reasonable defaults
        /// (e.g., zero dead storage) - this is suitable for defaults in the StateMod GUI.
        /// If false, don't initialize data - this is suitable for filling in StateDMI. </param>
        private void initialize(bool initialize_defaults)
        {
            _smdata_type = StateMod_DataSet.COMP_RESERVOIR_STATIONS;
            _owners = new List<StateMod_ReservoirAccount>();
            _climate_Vector = new List<StateMod_ReservoirClimate>();
            _areacapvals = new List<StateMod_ReservoirAreaCap>();
            _rights = new List<StateMod_ReservoirRight>();
            _content_MonthTS = null;
            _content_DayTS = null;
            _mintarget_MonthTS = null;
            _maxtarget_MonthTS = null;
            _mintarget_DayTS = null;
            _maxtarget_DayTS = null;
            //_georecord = null;
            if (initialize_defaults)
            {
                _cresdy = "0"; // Use monthly TS for daily
                _switch = 1; // In base class
                _rdate = -1;
                _volmin = 0;
                _volmax = 0;
                _flomax = 99999.0; // As per old SMGUI new reservoir
                _deadst = 0;
            }
            else
            {
                _cresdy = "";
                _rdate = StateMod_Util.MISSING_INT;
                _volmin = StateMod_Util.MISSING_DOUBLE;
                _volmax = StateMod_Util.MISSING_DOUBLE;
                _flomax = StateMod_Util.MISSING_DOUBLE;
                _deadst = StateMod_Util.MISSING_DOUBLE;
            }
        }

        /// <summary>
        /// Read reservoir information in and store in a Vector. </summary>
        /// <param name="filename"> Name of file to read. </param>
        /// <exception cref="Exception"> if there is an error reading the file. </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public static java.util.List<StateMod_Reservoir> readStateModFile(String filename) throws Exception
        public static IList<StateMod_Reservoir> readStateModFile(string filename)
        {
            string routine = "StateMod_Reservoir.readStateModFile";
            IList<StateMod_Reservoir> theReservoirs = new List<StateMod_Reservoir>();
            string iline = null;
            IList<object> v = new List<object>(9);
            int[] format_0 = new int[] { StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_INTEGER, StringUtil.TYPE_DOUBLE, StringUtil.TYPE_SPACE, StringUtil.TYPE_STRING };
            int[] format_0w = new int[] { 12, 24, 12, 8, 8, 1, 12 };
            int[] format_1 = new int[] { StringUtil.TYPE_SPACE, StringUtil.TYPE_DOUBLE, StringUtil.TYPE_DOUBLE, StringUtil.TYPE_DOUBLE, StringUtil.TYPE_DOUBLE, StringUtil.TYPE_INTEGER, StringUtil.TYPE_INTEGER, StringUtil.TYPE_INTEGER, StringUtil.TYPE_INTEGER };
            int[] format_1w = new int[] { 24, 8, 8, 8, 8, 8, 8, 8, 8 };
            int[] format_2 = new int[] { StringUtil.TYPE_SPACE, StringUtil.TYPE_STRING, StringUtil.TYPE_DOUBLE, StringUtil.TYPE_DOUBLE, StringUtil.TYPE_DOUBLE, StringUtil.TYPE_INTEGER };
            int[] format_2w = new int[] { 12, 12, 8, 8, 8, 8 };
            int[] format_3 = new int[] { StringUtil.TYPE_SPACE, StringUtil.TYPE_STRING, StringUtil.TYPE_DOUBLE };
            int[] format_3w = new int[] { 24, 12, 8 };
            int[] format_4 = new int[] { StringUtil.TYPE_SPACE, StringUtil.TYPE_DOUBLE, StringUtil.TYPE_DOUBLE, StringUtil.TYPE_DOUBLE };
            int[] format_4w = new int[] { 24, 8, 8, 8 };
            StreamReader @in = null;
            StateMod_Reservoir aReservoir = null;
            StateMod_ReservoirAccount anAccount = null;
            StateMod_ReservoirClimate anEvap = null;
            StateMod_ReservoirClimate aPtpx = null;
            int i = 0;

            if (Message.isDebugOn)
            {
                Message.printDebug(10, routine, "in SMParseResFile reading file: " + filename);
            }
            int line_count = 0;
            try
            {
                @in = new StreamReader(IOUtil.getPathUsingWorkingDir(filename));
                while (!string.ReferenceEquals((iline = @in.ReadLine()), null))
                {
                    ++line_count;
                    // check for comments
                    if (iline.StartsWith("#", StringComparison.Ordinal) || iline.Trim().Length == 0)
                    {
                        continue;
                    }

                    // allocate new reservoir node
                    aReservoir = new StateMod_Reservoir();

                    // line 1
                    if (Message.isDebugOn)
                    {
                        Message.printDebug(50, routine, "line 1: " + iline);
                    }
                    StringUtil.fixedRead(iline, format_0, format_0w, v);
                    aReservoir.setID(((string)v[0]).Trim());
                    aReservoir.setName(((string)v[1]).Trim());
                    aReservoir.setCgoto(((string)v[2]).Trim());
                    aReservoir.setSwitch((int?)v[3]);
                    aReservoir.setRdate((double?)v[4]);
                    aReservoir.setCresdy(((string)v[5]).Trim());

                    // line 2
                    iline = @in.ReadLine();
                    ++line_count;
                    if (Message.isDebugOn)
                    {
                        Message.printDebug(50, routine, "line 2: " + iline);
                    }
                    StringUtil.fixedRead(iline, format_1, format_1w, v);
                    aReservoir.setVolmin(((double?)v[0]));
                    aReservoir.setVolmax(((double?)v[1]));
                    aReservoir.setFlomax(((double?)v[2]));
                    aReservoir.setDeadst(((double?)v[3]));
                    int nowner = ((int?)v[4]).Value;
                    int nevap = ((int?)v[5]).Value;
                    int nptpx = ((int?)v[6]).Value;
                    int nrange = ((int?)v[7]).Value;

                    // get the owner's information
                    if (Message.isDebugOn)
                    {
                        Message.printDebug(50, routine, "Number of owners: " + nowner);
                    }
                    for (i = 0; i < nowner; i++)
                    {
                        iline = @in.ReadLine();
                        ++line_count;
                        StringUtil.fixedRead(iline, format_2, format_2w, v);
                        anAccount = new StateMod_ReservoirAccount();
                        // Account ID is set to the numerical count (StateMod uses the number)
                        anAccount.setID("" + (i + 1));
                        anAccount.setName(((string)v[0]).Trim());
                        anAccount.setOwnmax(((double?)v[1]));
                        anAccount.setCurown(((double?)v[2]));
                        anAccount.setPcteva(((double?)v[3]));
                        anAccount.setN2own(((int?)v[4]));
                        aReservoir.addAccount(anAccount);
                    }

                    // get the evaporation information
                    for (i = 0; i < nevap; i++)
                    {
                        iline = @in.ReadLine();
                        ++line_count;
                        StringUtil.fixedRead(iline, format_3, format_3w, v);
                        anEvap = new StateMod_ReservoirClimate();
                        anEvap.setID(((string)v[0]).Trim());
                        anEvap.setType(StateMod_ReservoirClimate.CLIMATE_EVAP);
                        anEvap.setWeight(((double?)v[1]));
                        aReservoir.addClimate(anEvap);
                    }

                    // get the precipitation information
                    for (i = 0; i < nptpx; i++)
                    {
                        iline = @in.ReadLine();
                        ++line_count;
                        StringUtil.fixedRead(iline, format_3, format_3w, v);
                        aPtpx = new StateMod_ReservoirClimate();
                        aPtpx.setID(((string)v[0]).Trim());
                        aPtpx.setType(StateMod_ReservoirClimate.CLIMATE_PTPX);
                        aPtpx.setWeight(((double?)v[1]));
                        aReservoir.addClimate(aPtpx);
                    }

                    // get the area capacity information
                    for (i = 0; i < nrange; i++)
                    {
                        iline = @in.ReadLine();
                        ++line_count;
                        StringUtil.fixedRead(iline, format_4, format_4w, v);
                        StateMod_ReservoirAreaCap anAreaCap = new StateMod_ReservoirAreaCap();
                        anAreaCap.setConten(((double?)v[0]));
                        anAreaCap.setSurarea(((double?)v[1]));
                        anAreaCap.setSeepage(((double?)v[2]));
                        aReservoir.addAreaCap(anAreaCap);
                    }

                    // add the reservoir to the vector of reservoirs
                    theReservoirs.Add(aReservoir);
                }
            }
            catch (Exception e)
            {
                Message.printWarning(3, routine, "Error reading reservoir stations in line " + line_count);
                throw e;
            }
            finally
            {
                if (@in != null)
                {
                    @in.Close();
                }
            }
            return theReservoirs;
        }

        /// <summary>
        /// set cresdy
        /// </summary>
        public virtual void setCresdy(string cresdy)
        {
            if ((!string.ReferenceEquals(cresdy, null)) && !cresdy.Equals(_cresdy))
            {
                _cresdy = cresdy;
                setDirty(true);
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_RESERVOIR_STATIONS, true);
                }
            }
        }

        /// <summary>
        /// Set the dead storage in reservoir.
        /// </summary>
        public virtual void setDeadst(double deadst)
        {
            if (deadst != _deadst)
            {
                _deadst = deadst;
                setDirty(true);
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_RESERVOIR_STATIONS, true);
                }
            }
        }

        /// <summary>
        /// Set the dead storage in reservoir.
        /// </summary>
        public virtual void setDeadst(double? deadst)
        {
            setDeadst(deadst.Value);
        }

        /// <summary>
        /// Set the dead storage in reservoir.
        /// </summary>
        public virtual void setDeadst(string deadst)
        {
            if (!string.ReferenceEquals(deadst, null))
            {
                setDeadst(double.Parse(deadst.Trim()));
            }
        }

        /// <summary>
        /// Set the maximum reservoir release.
        /// </summary>
        public virtual void setFlomax(double flomax)
        {
            if (flomax != _flomax)
            {
                _flomax = flomax;
                setDirty(true);
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_RESERVOIR_STATIONS, true);
                }
            }
        }

        /// <summary>
        /// Set the maximum reservoir release.
        /// </summary>
        public virtual void setFlomax(double? flomax)
        {
            setFlomax(flomax.Value);
        }

        /// <summary>
        /// Set the maximum reservoir release.
        /// </summary>
        public virtual void setFlomax(string flomax)
        {
            if (!string.ReferenceEquals(flomax, null))
            {
                setFlomax(double.Parse(flomax.Trim()));
            }
        }

        /// <summary>
        /// Set the date for one fill rule admin.
        /// The value 0 is meaning (??)- should be 1-12 or -1 to signify do not administer rule
        /// </summary>
        public virtual void setRdate(double rdate)
        {
            if (rdate == 0)
            {
                rdate = -1;
            }

            if (rdate != _rdate)
            {
                _rdate = rdate;
                setDirty(true);
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_RESERVOIR_STATIONS, true);
                }
            }
        }

        /// <summary>
        /// Set the date for the one fill rule.
        /// </summary>
        public virtual void setRdate(double? rdate)
        {
            setRdate(rdate.Value);
        }

        /// <summary>
        /// Set the date for the one fill rule.
        /// </summary>
        public virtual void setRdate(string rdate)
        {
            if (!string.ReferenceEquals(rdate, null))
            {
                setRdate(double.Parse(rdate.Trim()));
            }
        }

        /// <summary>
        /// Set the maximum reservoir content.
        /// </summary>
        public virtual void setVolmax(double volmax)
        {
            if (volmax != _volmax)
            {
                _volmax = volmax;
                setDirty(true);
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_RESERVOIR_STATIONS, true);
                }
            }
        }

        /// <summary>
        /// Set the maximum reservoir content.
        /// </summary>
        public virtual void setVolmax(double? volmax)
        {
            setVolmax(volmax.Value);
        }

        /// <summary>
        /// Set the maximum reservoir content.
        /// </summary>
        public virtual void setVolmax(string volmax)
        {
            if (!string.ReferenceEquals(volmax, null))
            {
                setVolmax(double.Parse(volmax.Trim()));
            }
        }

        /// <summary>
        /// Set the minimum reservoir content.
        /// </summary>
        public virtual void setVolmin(double volmin)
        {
            if (volmin != _volmin)
            {
                _volmin = volmin;
                setDirty(true);
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_RESERVOIR_STATIONS, true);
                }
            }
        }

        /// <summary>
        /// Set the minimum reservoir content.
        /// </summary>
        public virtual void setVolmin(double? volmin)
        {
            setVolmin(volmin.Value);
        }

        /// <summary>
        /// Set the minimum reservoir content.
        /// </summary>
        public virtual void setVolmin(string volmin)
        {
            if (!string.ReferenceEquals(volmin, null))
            {
                setVolmin(double.Parse(volmin.Trim()));
            }
        }
    }
}
