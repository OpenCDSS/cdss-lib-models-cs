using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.IO;

// StateMod_Diversion - class for diversion station

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
// StateMod_Diversion - class derived from StateMod_Data.  Contains information
//			read from the diversion file.
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
// 
// 19 Aug 1997	Catherine E.
//		Nutting-Lane, RTi	Created initial version of class
// 27 Mar 1998	CEN, RTi		Added pointers to TS.
// 06 Apr 1998	CEN, RTi		Added java style documentation.
// 21 Dec 1998	CEN, RTi		Added throws IOException to read/write
//					routines.
// 25 Oct 1999	CEN, RTi		Added daily diversion id.
//
// 01 Dec 1999	Steven A. Malers, RTi	Change so that connectAllTS is
//					overloaded to work the old way(no daily
//					time series)and with daily time series.
// 15 Feb 2001	SAM, RTi		Add use_daily_data flag to write methods
//					to allow writing old format files.
//					Change IO to IOUtil.  Add finalize();
//					Alphabetize methods.  Set unused
//					variables to null.  Add more checks for
//					null.  Update file output header.
//					In dumpDiversionsFile, reuse the vector
//					for output, optimize output formats
//					(don't need to format blank strings),
//					and remove debug statements.
// 2001-12-27	SAM, RTi		Update to use new fixedRead()to
//					improve performance.
// 2002-09-09	SAM, RTi		Add GeoRecord reference to allow 2-way
//					connection between spatial and StateMod
//					data.
// 2002-09-19	SAM, RTi		Use isDirty()instead of setDirty()to
//					indicate edits.
//					dds file.
//------------------------------------------------------------------------------
// 2003-06-04	J. Thomas Sapienza, RTi	Renamed from SMDiversion to 
//					StateMod_Diversion
// 2003-06-10	JTS, RTi		* Folded dumpDiversionsFile() into
//					  writeDiversionsFile()
//					* Renamed parseDiversionsFile() to
//					  readDiversionsFile()
// 2003-06-23	JTS, RTi		Renamed writeDiversionsFile() to
//					writeStateModFile()
// 2003-06-26	JTS, RTi		Renamed readDiversionsFile() to
//					readStateModFile()
// 2003-08-03	SAM, RTi		Change isDirty() to setDirty().
// 2003-08-14	SAM, RTi		Change GeoRecordNoSwing to GeoRecord.
// 2003-08-27	SAM, RTi		* Change default for cdividy to "0".
//					* Rework time series data members and
//					  methods to have better names,
//					  consistent with the data set
//					  components.
//					* Change so water rights are stored in
//					  a Vector, not an internally-maintained
//					  linked-list.
//					* Add all diversion data for the current
//					  StateMod design so that nothing is
//					  left out.
//					* Change connectDivRights() to
//					  connectRights().
//					* Change connectAllDivRights() to
//					  connectAllRights().
//					* Allow case-independent searches for
//					  time series identifiers.
//					* In addition to calling setDirty() on
//					  the data set component, do so on the
//					  individual objects.
//					* Clean up Javadoc.
//					* Remove data members for size of
//					  Vectors - the size can be determined
//					  from the Vectors.  Nduser is no longer
//					  used so don't need in any case.  Still
//					  output to allow file comparisons.
// 2003-09-30	SAM, RTi		Pass component type to
//					StateMod_ReturnFlow constructor.
// 2003-10-07	SAM, RTi		* As per Ray Bennett, default the demand
//					  source to 0, Unknown.
//					* Similarly, default efficiency is 60.
// 2003-10-10	SAM, RTi		Add disconnectRights().
// 2003-10-14	SAM, RTi		* Add a copy constructor for use by the
//					  StateMod_Diversion_JFrame to track
//					  edits.
//					* Change IWR to CWR (irrigation to
//					  consumptive) as per Ray Bennett
//					  feedback.
//					* Set the diversion dirty to false after
//					  read or construction - it may have
//					  been marked dirty with set() methods.
// 2003-10-21	SAM, RTi		Change demand override average monthly
//					to demand average monthly - more
//					consistent with documentation.
// 2004-02-25	SAM, RTi		Add isStateModDiversionFile().
// 2004-03-31	SAM, RTi		Print the line number and line when an
//					error occurs reading the file.
// 2004-04-12	SAM, RTi		* Change so read and write methods
//					  convert file paths using working
//					  directory.
// 2004-06-05	SAM, RTi		* Add methods to handle collections,
//					  similar to StateCU locations.
//					* Define static values here, that are
//					  possible values for some data members.
//					  These values were previously defined
//					  in StateMod_Diversion_JFrame.
//					* Add methods to retrieve the option
//					  strings.
// 2004-06-14	SAM, RTi		* Define public final int's for
//					  important demsrc values to help with
//					  StateDMI.
//					* Overload the constructor to allow
//					  initialization as completely missing
//					  or with reasonable defaults.  The
//					  former is better for StateDMI, the
//					  latter for StateMod GUI.
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
// 2004-08-16	SAM, RTi		* Output old Nduser as 1 instead of 0
//					  since that is what old files have.
// 2004-09-01	SAM, RTi		* Add the following for use with
//					  StateDMI only - no need to check for
//					  dirty - only set/gets on the entire
//					  array are enabled:
//						__cwr_monthly
//						__ddh_monthly
//						__calculated_efficiencies
//						__calculated_efficiency_stddevs
//						__model_efficiecies
// 2004-09-06	SAM, RTi		* Add "MultiStruct" to the types of
//					  collection.
// 2005-03-30	JTS, RTi		* Added getCollectionPartType().
//					* Added getCollectionYears().
// 2005-04-14	JTS, RTi		Added writeListFile().
// 2005-04-19	JTS, RTi		Added writeCollectionListFile().
// 2005-15-16	SAM, RTi		Overload setDiveff() to accept a
//					parameter indicating the year type of
//					the diversion stations file, to simplify
//					adjustments for water year, etc.
// 2006-04-09	SAM, RTi		Add _parcels_Vector data member and
//					associated methods, to help with
//					StateDMI error handling.
// 2007-04-12	Kurt Tometich, RTi		Added checkComponentData() and
//									getDataHeader() methods for check
//									file and data check support.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
//------------------------------------------------------------------------------
// EndHeader
// REVISIT SAM 2006-04-09
// The _parcel_Vector has minimal support and is not yet considered in
// copy, clone, equals, etc.

namespace DWR.StateMod
{

    //using StateCU_IrrigationPracticeTS = DWR.StateCU.StateCU_IrrigationPracticeTS;
    //using GeoRecord = RTi.GIS.GeoView.GeoRecord;
    //using HasGeoRecord = RTi.GIS.GeoView.HasGeoRecord;
    using DayTS = RTi.TS.DayTS;
    using MonthTS = RTi.TS.MonthTS;
    using DataSetComponent = RTi.Util.IO.DataSetComponent;
    using IOUtil = RTi.Util.IO.IOUtil;
    using Message = RTi.Util.Message.Message;
    using StringUtil = RTi.Util.String.StringUtil;
    using TimeUtil = RTi.Util.Time.TimeUtil;
    using YearType = RTi.Util.Time.YearType;

    /// <summary>
    /// Object used to store diversion information.  All set routines set
    /// the COMP_DIVERSION_STATIONS flag dirty.  A new object will have empty non-null
    /// lists, null time series, and defaults for all other data.
    /// </summary>
    public class StateMod_Diversion : StateMod_Data //, ICloneable, IComparable<StateMod_Data>, StateMod_ComponentValidator, HasGeoRecord
    {

        /// <summary>
        /// Demand source values used by other software.  Most interaction is expected to occur through GUIs.
        /// </summary>
        public const int DEMSRC_UNKNOWN = 0;
        public const int DEMSRC_GIS = 1;
        public const int DEMSRC_TIA = 2;
        public const int DEMSRC_GIS_PRIMARY = 3;
        public const int DEMSRC_TIA_PRIMARY = 4;
        public const int DEMSRC_GIS_SECONDARY = 5;
        public const int DEMSRC_MI_TRANSBASIN = 6;
        public const int DEMSRC_CARRIER = 7;
        public const int DEMSRC_USER = 8;

        /// <summary>
        /// Daily diversion ID.
        /// </summary>
        protected internal string _cdividy;

        /// <summary>
        /// Diversion capacity
        /// </summary>
        protected internal double _divcap;

        /// <summary>
        /// User name
        /// </summary>
        protected internal string _username;

        /// <summary>
        /// data type switch
        /// </summary>
        protected internal int _idvcom;

        /// <summary>
        /// System efficiency switch
        /// </summary>
        protected internal double _divefc;

        /// <summary>
        /// Efficiency % by month.  The efficiencies are in the order of the calendar for
        /// the data set.  Therefore, for proper display, the calendar type must be known.
        /// </summary>
        protected internal double[] _diveff;

        // The following are used only by StateDMI and do not needed to be handled in
        // comparison, initialization, etc.
        private double[] __calculated_efficiencies = null;
        private double[] __calculated_efficiency_stddevs = null;
        private double[] __model_efficiencies = null;

        /// <summary>
        /// irrigated acreage, future
        /// </summary>
        protected internal double _area;

        /// <summary>
        /// use type
        /// </summary>
        protected internal int _irturn;

        /// <summary>
        /// river nodes receiving return flow
        /// </summary>
        protected internal IList<StateMod_ReturnFlow> _rivret;

        /// <summary>
        /// Direct diversions rights
        /// </summary>
        protected internal IList<StateMod_DiversionRight> _rights;

        /// <summary>
        /// Acreage source
        /// </summary>
        protected internal int _demsrc;

        /// <summary>
        /// Replacement code
        /// </summary>
        protected internal int _ireptype;

        /// <summary>
        /// Pointer to monthly demand ts.
        /// </summary>
        protected internal MonthTS _demand_MonthTS;

        /// <summary>
        /// Pointer to monthly demand override ts.
        /// </summary>
        protected internal MonthTS _demand_override_MonthTS;

        /// <summary>
        /// Pointer to average monthly demand override ts.
        /// </summary>
        protected internal MonthTS _demand_average_MonthTS;

        /// <summary>
        /// Pointer to daily demand ts.
        /// </summary>
        protected internal DayTS _demand_DayTS;

        /// <summary>
        /// Pointer to historical monthly diversion ts
        /// </summary>
        protected internal MonthTS _diversion_MonthTS;

        /// <summary>
        /// 12 monthly and annual average over period, used by StateDMI.
        /// </summary>
        private double[] __ddh_monthly = null;

        /// <summary>
        /// Pointer to historical daily diversion ts
        /// </summary>
        protected internal DayTS _diversion_DayTS;

        /// <summary>
        /// Pointer to monthly consumptive water requirement ts.
        /// </summary>
        protected internal MonthTS _cwr_MonthTS;

        /// <summary>
        /// 12 monthly and annual average over period, used by StateDMI
        /// </summary>
        private double[] __cwr_monthly = null;

        /// <summary>
        /// Pointer to daily consumptive water requirement ts.
        /// </summary>
        protected internal DayTS _cwr_DayTS;

        /// <summary>
        /// Pointer to the StateCU_IrrigationPracticeTS.  This object actually contains
        /// other time series, which can be retrieved for displays.
        /// </summary>
        //protected internal StateCU_IrrigationPracticeTS _ipy_YearTS;

        /// <summary>
        /// Soil available water content, from StateCU file.
        /// </summary>
        // FIXME SAM 2009-06-22 Does not seem to be used anymore - is not read or written to file - need to evaluate
        protected internal double _awc;

        /// <summary>
        /// Reference to spatial data for this diversion -- currently NOT cloned.  If null, then no spatial data
        /// are available.
        /// </summary>
        //protected internal GeoRecord _georecord = null;

        /// <summary>
        /// List of parcel data, in particular to allow StateDMI to detect when a diversion had no data.
        /// </summary>
        protected internal IList<StateMod_Parcel> _parcel_Vector = new List<StateMod_Parcel>();

        // Collections are set up to be specified by year, although currently for
        // diversions collections are always the same for the full period.

        /// <summary>
        /// Types of collections.  An aggregate merges the water rights whereas
        /// a system keeps all the water rights but just has one ID.  See email from Erin
        /// Wilson 2004-09-01, to reiterate current modeling procedures:
        /// <pre>
        /// <ol>
        /// <li>Multistructure should be used to represent two or more structures
        /// that divert from DIFFERENT TRIBUTARIES to serve the same demand
        /// (irrigated acreage or M&I demand).  In the Historic model used to
        /// estimate Baseflows, the historic diversions need to be represented on
        /// the correct tributary, so all structures are in the network.  Average
        /// efficiencies need to be set for these structures, since IWR has been
        /// assigned to only one structure.  In Baseline and Calculated mode, the
        /// multistruct(x,x) command will assign all demand to the primary structure
        /// and zero out the demand for the secondary structures.  Water rights will
        /// continue to be assigned to each individual structure, and operating
        /// rules need to be included to allow the model to divert from the
        /// secondary structure location (under their water right) to meet the
        /// primary structure demand.</li>
        /// <li>Divsystems should be used to represents two or more structures with
        /// intermingled lands and/or diversions that divert from the SAME
        /// TRIBUTARY.  Only the primary structure should be included in the
        /// network.  The Divsystem(x,x) command will combine historic diversions,
        /// capacities, and acreages for use in the Historic model and to create
        /// Baseflows.  Water rights for all structures will be assigned explicitly
        /// to the primary structure.  No operating rules or set efficiency commands are required.</li>
        /// <li>Aggregates.  The only difference between Divsystems and Aggregates
        /// is that the water rights are not necessarily assigned explicitly, but
        /// are generally grouped into water rights classes.</li>
        /// </pre>
        /// </summary>
        public static string COLLECTION_TYPE_AGGREGATE = "Aggregate";
        public static string COLLECTION_TYPE_SYSTEM = "System";
        public static string COLLECTION_TYPE_MULTISTRUCT = "MultiStruct";

        private string __collection_type = StateMod_Util.MISSING_STRING;

        /// <summary>
        /// Used by DMI software - currently no options.
        /// </summary>
        private string __collection_part_type = "Ditch";

        /// <summary>
        /// The identifiers for data that are collected - null if not a collection location.
        /// This is a list of lists where the __collection_year is the first dimension.
        /// This is ugly but need to use the code to see if it can be made cleaner.
        /// </summary>
        private IList<IList<string>> __collection_Vector = null;

        /// <summary>
        /// An array of years that correspond to the aggregate/system.  Ditches currently only have one year.
        /// </summary>
        private int[] __collection_year = null;

        /// <summary>
        /// Construct a new diversion and assign data to reasonable defaults.
        /// </summary>
        public StateMod_Diversion() : base()
        {
            initialize(true);
        }

        /// <summary>
        /// Construct a new diversion. </summary>
        /// <param name="initialize_defaults"> If true, assign data to reasonable defaults.
        /// If false, all data are set to missing. </param>
        public StateMod_Diversion(bool initialize_defaults) : base()
        {
            initialize(initialize_defaults);
        }

        /// <summary>
        /// Copy constructor. </summary>
        /// <param name="deep_copy"> If true, make a deep copy including secondary vectors of data.
        /// Currently only false is recognized, in which primitive data are copied.  This is
        /// suitable to allow the StateMod_Diversion_JFrame class to know when changes have
        /// been made to data on the main screen. </param>
        public StateMod_Diversion(StateMod_Diversion div, bool deep_copy) : this()
        {
            // Base class...
            // TODO
            // Local data members...
            _cdividy = div._cdividy;
            _divcap = div._divcap;
            _username = div._username;
            _idvcom = div._idvcom;
            _divefc = div._divefc;
            for (int i = 0; i < 12; i++)
            {
                _diveff[i] = div._diveff[i];
            }
            _area = div._area;
            _irturn = div._irturn;
            _rivret = div._rivret;
            _rights = div._rights;
            _demsrc = div._demsrc;
            _ireptype = div._ireptype;
            // For time series, the references are pointed to the original but data are not copied.
            _demand_MonthTS = div._demand_MonthTS;
            _demand_override_MonthTS = div._demand_override_MonthTS;
            _demand_average_MonthTS = div._demand_average_MonthTS;
            _demand_DayTS = div._demand_DayTS;
            _diversion_MonthTS = div._diversion_MonthTS;
            _diversion_DayTS = div._diversion_DayTS;
            _cwr_MonthTS = div._cwr_MonthTS;
            _cwr_DayTS = div._cwr_DayTS;
            //_ipy_YearTS = div._ipy_YearTS;
            _awc = div._awc;
            //_georecord = div._georecord;
        }

        /// <summary>
        /// Add return flow node to the vector of return flow nodes. </summary>
        /// <param name="rivret"> return flow </param>
        public virtual void addReturnFlow(StateMod_ReturnFlow rivret)
        {
            if (rivret == null)
            {
                return;
            }
            _rivret.Add(rivret);
            setDirty(true);
            if (!_isClone && _dataset != null)
            {
                _dataset.setDirty(StateMod_DataSet.COMP_DIVERSION_STATIONS, true);
            }
        }

        /// <summary>
        /// Given a list containing all the diversions and another list containing
        /// all the rights, creates a system of pointers to link the diversions to their associated rights.
        /// This routines doesn't add an element to an array.  The array
        /// already exists, we are just connecting next and previous pointers. </summary>
        /// <param name="diversions"> all diversions </param>
        /// <param name="rights"> all rights </param>
        public static void connectAllRights(IList<StateMod_Diversion> diversions, IList<StateMod_DiversionRight> rights)
        {
            if ((diversions == null) || (rights == null))
            {
                return;
            }
            int num_divs = diversions.Count;

            StateMod_Diversion div;
            for (int i = 0; i < num_divs; i++)
            {
                div = diversions[i];
                if (div == null)
                {
                    continue;
                }
                div.connectRights(rights);
            }
        }

        /// <summary>
        /// Connect the diversion rights with this diversion, comparing the "cgoto" for the
        /// right to the diversion identifier.  Multiple rights may be associated with a diversion. </summary>
        /// <param name="rights"> all rights. </param>
        public virtual void connectRights(IList<StateMod_DiversionRight> rights)
        {
            if (rights == null)
            {
                return;
            }
            int num_rights = rights.Count;

            StateMod_DiversionRight right;
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
        /// Return the system efficiency switch.
        /// </summary>
        public virtual double getDivefc()
        {
            return _divefc;
        }

        /// <summary>
        /// Initialize data.  Sets the smdata_type to _dataset.COMP_DIVERSION_STATIONS. </summary>
        /// <param name="initialize_defaults"> If true, assign data to reasonable defaults.
        /// If false, all data are set to missing. </param>
        private void initialize(bool initialize_defaults)
        {
            _smdata_type = StateMod_DataSet.COMP_DIVERSION_STATIONS;
            if (initialize_defaults)
            {
                _divefc = -60.0; // Ray Bennett, Ray Alvarado, 2003-10-07 progress mtg.
                _diveff = new double[12];
                for (int i = 0; i < 12; i++)
                {
                    _diveff[i] = 60.0; // See above
                }
                _username = "";
                _cdividy = "0"; // Use the average monthly TS for daily TS
                _divcap = 0;
                _idvcom = 1;
                _area = 0;
                _irturn = 1;
                _demsrc = DEMSRC_UNKNOWN;
                _ireptype = -1; // Provide depletion replacement
            }
            else
            {
                _divefc = StateMod_Util.MISSING_DOUBLE;
                _diveff = new double[12];
                for (int i = 0; i < 12; i++)
                {
                    _diveff[i] = StateMod_Util.MISSING_DOUBLE;
                }
                _username = StateMod_Util.MISSING_STRING;
                _cdividy = StateMod_Util.MISSING_STRING;
                _divcap = StateMod_Util.MISSING_INT;
                _idvcom = StateMod_Util.MISSING_INT;
                _area = StateMod_Util.MISSING_DOUBLE;
                _irturn = StateMod_Util.MISSING_INT;
                _demsrc = StateMod_Util.MISSING_INT;
                _ireptype = StateMod_Util.MISSING_INT;
            }
            _rivret = new List<StateMod_ReturnFlow>();
            _rights = new List<StateMod_DiversionRight>();
            _diversion_MonthTS = null;
            _diversion_DayTS = null;
            _demand_MonthTS = null;
            _demand_override_MonthTS = null;
            _demand_average_MonthTS = null;
            _demand_DayTS = null;
            //_ipy_YearTS = null;
            _cwr_MonthTS = null;
            _cwr_DayTS = null;
            //_georecord = null;
        }

        /// <summary>
        /// Read diversion information in and store in a java vector.
        /// The new diversions are added to the end of the previously stored diversions. </summary>
        /// <param name="filename"> filename containing diversion information </param>
        /// <exception cref="Exception"> if an error occurs </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public static java.util.List<StateMod_Diversion> readStateModFile(String filename) throws Exception
        public static IList<StateMod_Diversion> readStateModFile(string filename)
        {
            string routine = "StateMod_Diversion.readStateModFile";
            string iline = null;
            IList<object> v = new List<object>(9);
            IList<StateMod_Diversion> theDiversions = new List<StateMod_Diversion>();
            int i;
            int linecount = 0;
            string s = null;

            int[] format_0 = new int[] { StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_INTEGER, StringUtil.TYPE_DOUBLE, StringUtil.TYPE_INTEGER, StringUtil.TYPE_INTEGER, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING };
            int[] format_0w = new int[] { 12, 24, 12, 8, 8, 8, 8, 1, 12 };
            int[] format_1 = new int[] { StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_INTEGER, StringUtil.TYPE_INTEGER, StringUtil.TYPE_DOUBLE, StringUtil.TYPE_DOUBLE, StringUtil.TYPE_INTEGER, StringUtil.TYPE_INTEGER };
            int[] format_1w = new int[] { 12, 24, 12, 8, 8, 8, 8, 8, 8 };
            int[] format_2 = new int[] { StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_DOUBLE, StringUtil.TYPE_INTEGER };
            int[] format_2w = new int[] { 36, 12, 8, 8 };

            StateMod_Diversion aDiversion = null;
            StateMod_ReturnFlow aReturnNode = null;
            StreamReader @in = null;

            Message.printStatus(1, routine, "Reading diversion file: " + filename);
            try
            {
                @in = new StreamReader(IOUtil.getPathUsingWorkingDir(filename));
                while (!string.ReferenceEquals((iline = @in.ReadLine()), null))
                {
                    ++linecount;
                    // check for comments
                    if (iline.StartsWith("#", StringComparison.Ordinal) || iline.Trim().Length == 0)
                    {
                        continue;
                    }

                    // allocate new diversion node
                    aDiversion = new StateMod_Diversion();

                    // line 1
                    if (Message.isDebugOn)
                    {
                        Message.printDebug(50, routine, "line 1: " + iline);
                    }
                    StringUtil.fixedRead(iline, format_0, format_0w, v);
                    aDiversion.setID(((string)v[0]).Trim());
                    aDiversion.setName(((string)v[1]).Trim());
                    aDiversion.setCgoto(((string)v[2]).Trim());
                    aDiversion.setSwitch((int?)v[3]);
                    aDiversion.setDivcap((double?)v[4]);
                    aDiversion.setIreptype(((int?)v[6]));
                    aDiversion.setCdividy(((string)v[8]).Trim());

                    // line 2
                    iline = @in.ReadLine();
                    ++linecount;
                    if (Message.isDebugOn)
                    {
                        Message.printDebug(50, routine, "line 2: " + iline);
                    }
                    StringUtil.fixedRead(iline, format_1, format_1w, v);
                    aDiversion.setUsername(((string)v[1]).Trim());
                    aDiversion.setIdvcom(((int?)v[3]));
                    int nrtn = ((int?)v[4]).Value;
                    aDiversion.setDivefc(((double?)v[5]));
                    aDiversion.setArea(((double?)v[6]));
                    aDiversion.setIrturn(((int?)v[7]));
                    aDiversion.setDemsrc(((int?)v[8]));

                    // get the efficiency information
                    if (aDiversion.getDivefc() < 0)
                    {
                        // Negative value indicates monthly efficiencies will follow...
                        iline = @in.ReadLine();
                        ++linecount;
                        // Free format...
                        char[] seps = { ' ', '\t', '\n', '\r', '\f' };
                        string[] split = iline.Split(seps);
                        if (split != null && split.Length == 12)
                        {
                            for (i = 0; i < 12; i++)
                            {
                                aDiversion.setDiveff(i, split[0]);
                            }
                        }
                    }
                    else
                    {
                        // Annual efficiency so set monthly efficiencies to the annual...
                        aDiversion.setDiveff(0, aDiversion.getDivefc());
                        aDiversion.setDiveff(1, aDiversion.getDivefc());
                        aDiversion.setDiveff(2, aDiversion.getDivefc());
                        aDiversion.setDiveff(3, aDiversion.getDivefc());
                        aDiversion.setDiveff(4, aDiversion.getDivefc());
                        aDiversion.setDiveff(5, aDiversion.getDivefc());
                        aDiversion.setDiveff(6, aDiversion.getDivefc());
                        aDiversion.setDiveff(7, aDiversion.getDivefc());
                        aDiversion.setDiveff(8, aDiversion.getDivefc());
                        aDiversion.setDiveff(9, aDiversion.getDivefc());
                        aDiversion.setDiveff(10, aDiversion.getDivefc());
                        aDiversion.setDiveff(11, aDiversion.getDivefc());
                    }

                    // get the return information
                    for (i = 0; i < nrtn; i++)
                    {
                        iline = @in.ReadLine();
                        ++linecount;
                        StringUtil.fixedRead(iline, format_2, format_2w, v);
                        aReturnNode = new StateMod_ReturnFlow(StateMod_DataSet.COMP_DIVERSION_STATIONS);
                        s = ((string)v[1]).Trim();
                        if (s.Length <= 0)
                        {
                            aReturnNode.setCrtnid(((string)v[0]).Trim());
                            Message.printWarning(3, routine, "Return node for structure \"" + aDiversion.getID() + "\" is blank. ");
                        }
                        else
                        {
                            aReturnNode.setCrtnid(s);
                        }

                        aReturnNode.setPcttot(((double?)v[2]));
                        aReturnNode.setIrtndl(((int?)v[3]));
                        aDiversion.addReturnFlow(aReturnNode);
                    }

                    // Set the diversion to not dirty because it was just initialized...

                    aDiversion.setDirty(false);

                    // add the diversion to the vector of diversions
                    theDiversions.Add(aDiversion);
                }
            }
            catch (Exception e)
            {
                Message.printWarning(3, routine, "Error reading line " + linecount + " \"" + iline + "\"");
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
            return theDiversions;
        }

        /// <summary>
        /// Set the irrigated acreage. </summary>
        /// <param name="area"> acreage. </param>
        public virtual void setArea(double area)
        {
            if (_area != area)
            {
                _area = area;
                setDirty(true);
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_DIVERSION_STATIONS, true);
                }
            }
        }

        /// <summary>
        /// Set the irrigated acreage. </summary>
        /// <param name="area"> acreage. </param>
        public virtual void setArea(double? area)
        {
            setArea(area.Value);
        }

        /// <summary>
        /// Set the irrigated acreage. </summary>
        /// <param name="area"> acreage. </param>
        public virtual void setArea(string area)
        {
            if (string.ReferenceEquals(area, null))
            {
                return;
            }
            setArea(double.Parse(area.Trim()));
        }

        /// <summary>
        /// Set the daily id. </summary>
        /// <param name="cdividy"> daily id. </param>
        public virtual void setCdividy(string cdividy)
        {
            if (string.ReferenceEquals(cdividy, null))
            {
                return;
            }
            if (!cdividy.Equals(_cdividy))
            {
                _cdividy = cdividy;
                setDirty(true);
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_DIVERSION_STATIONS, true);
                }
            }
        }

        /// <summary>
        /// Set the demand source. </summary>
        /// <param name="demsrc"> acreage source. </param>
        public virtual void setDemsrc(int demsrc)
        {
            if (demsrc != _demsrc)
            {
                _demsrc = demsrc;
                setDirty(true);
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_DIVERSION_STATIONS, true);
                }
            }
        }

        /// <summary>
        /// Set the demand source. </summary>
        /// <param name="demsrc"> acreage source. </param>
        public virtual void setDemsrc(int? demsrc)
        {
            setDemsrc(demsrc.Value);
        }

        /// <summary>
        /// Set the demand source. </summary>
        /// <param name="demsrc"> demand source. </param>
        public virtual void setDemsrc(string demsrc)
        {
            if (string.ReferenceEquals(demsrc, null))
            {
                return;
            }
            setDemsrc(int.Parse(demsrc.Trim()));
        }

        /// <summary>
        /// Set the diversion capacity. </summary>
        /// <param name="divcap"> diversion capacity. </param>
        public virtual void setDivcap(double divcap)
        {
            if (divcap != _divcap)
            {
                _divcap = divcap;
                setDirty(true);
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_DIVERSION_STATIONS, true);
                }
            }
        }

        /// <summary>
        /// Set the diversion capacity. </summary>
        /// <param name="divcap"> diversion capacity. </param>
        public virtual void setDivcap(double? divcap)
        {
            setDivcap(divcap.Value);
        }

        /// <summary>
        /// Set the diversion capacity. </summary>
        /// <param name="divcap"> diversion capacity. </param>
        public virtual void setDivcap(string divcap)
        {
            if (!string.ReferenceEquals(divcap, null))
            {
                setDivcap(double.Parse(divcap.Trim()));
            }
        }

        /// <summary>
        /// Set the system efficiency switch. </summary>
        /// <param name="divefc"> efficiency. </param>
        public virtual void setDivefc(double divefc)
        {
            if (divefc != _divefc)
            {
                _divefc = divefc;
                setDirty(true);
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_DIVERSION_STATIONS, true);
                }
            }
        }

        /// <summary>
        /// Set the system efficiency switch. </summary>
        /// <param name="divefc"> efficiency. </param>
        public virtual void setDivefc(double? divefc)
        {
            setDivefc(divefc.Value);
        }

        /// <summary>
        /// Set the system efficiency switch. </summary>
        /// <param name="divefc"> efficiency. </param>
        public virtual void setDivefc(string divefc)
        {
            if (string.ReferenceEquals(divefc, null))
            {
                return;
            }
            setDivefc(double.Parse(divefc.Trim()));
        }

        /// <summary>
        /// Set the system efficiency for a particular month.
        /// The efficiencies are stored in the order of the year for the data set.  For
        /// example, if water years are used, the first efficiency will be for October.  For
        /// calendar year, the first efficiency will be for January. </summary>
        /// <param name="index"> month index (0+) </param>
        /// <param name="diveff"> monthly efficiency </param>
        public virtual void setDiveff(int index, double diveff)
        {
            if (_diveff[index] != diveff)
            {
                _diveff[index] = diveff;
                setDirty(true);
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_DIVERSION_STATIONS, true);
                }
            }
        }

        /// <summary>
        /// Set the system efficiency for a particular month.
        /// The efficiencies are specified with month 0 being January. </summary>
        /// <param name="index"> month index (0=January). </param>
        /// <param name="diveff"> monthly efficiency </param>
        /// <param name="yeartype"> The year type for the diversion stations file (consistent with
        /// the control file for a full data set).  </param>
        //public virtual void setDiveff(int index, double diveff, YearType yeartype)
        //{ // Adjust the index if necessary based on the year type...
        //    if (yeartype == null)
        //    {
        //        // Assume calendar.
        //    }
        //    else if (yeartype == YearType.WATER)
        //    {
        //        index = TimeUtil.convertCalendarMonthToCustomMonth((index + 1), 10) - 1;
        //    }
        //    else if (yeartype == YearType.NOV_TO_OCT)
        //    {
        //        index = TimeUtil.convertCalendarMonthToCustomMonth((index + 1), 11) - 1;
        //    }
        //    if (_diveff[index] != diveff)
        //    {
        //        _diveff[index] = diveff;
        //        setDirty(true);
        //        if (!_isClone && _dataset != null)
        //        {
        //            _dataset.setDirty(StateMod_DataSet.COMP_DIVERSION_STATIONS, true);
        //        }
        //    }
        //}

        /// <summary>
        /// Set the system efficiency for a particular month.
        /// The efficiencies are stored in the order of the year for the data set.  For
        /// example, if water years are used, the first efficiency will be for October.  For
        /// calendar year, the first efficiency will be for January. </summary>
        /// <param name="index"> month index </param>
        /// <param name="diveff"> monthly efficiency </param>
        public virtual void setDiveff(int index, double? diveff)
        {
            setDiveff(index, diveff.Value);
        }

        /// <summary>
        /// Set the system efficiency for a particular month.
        /// The efficiencies are stored in the order of the year for the data set.  For
        /// example, if water years are used, the first efficiency will be for October.  For
        /// calendar year, the first efficiency will be for January. </summary>
        /// <param name="index"> month index </param>
        /// <param name="diveff"> monthly efficiency </param>
        public virtual void setDiveff(int index, string diveff)
        {
            if (string.ReferenceEquals(diveff, null))
            {
                return;
            }
            setDiveff(index, double.Parse(diveff.Trim()));
        }

        /// <summary>
        /// Set the data type switch. </summary>
        /// <param name="idvcom"> data type switch. </param>
        public virtual void setIdvcom(int idvcom)
        {
            if (idvcom != _idvcom)
            {
                _idvcom = idvcom;
                setDirty(true);
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_DIVERSION_STATIONS, true);
                }
            }
        }

        /// <summary>
        /// Set the data type switch. </summary>
        /// <param name="idvcom"> data type switch. </param>
        public virtual void setIdvcom(int? idvcom)
        {
            setIdvcom(idvcom.Value);
        }

        /// <summary>
        /// Set the data type switch. </summary>
        /// <param name="idvcom"> data type switch. </param>
        public virtual void setIdvcom(string idvcom)
        {
            if (string.ReferenceEquals(idvcom, null))
            {
                return;
            }
            setIdvcom(int.Parse(idvcom.Trim()));
        }

        /// <summary>
        /// Set the replacement code. </summary>
        /// <param name="ireptype"> replacement code. </param>
        public virtual void setIreptype(int ireptype)
        {
            if (ireptype != _ireptype)
            {
                _ireptype = ireptype;
                setDirty(true);
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_DIVERSION_STATIONS, true);
                }
            }
        }

        /// <summary>
        /// Set the replacement code. </summary>
        /// <param name="ireptype"> replacement code. </param>
        public virtual void setIreptype(int? ireptype)
        {
            setIreptype(ireptype.Value);
        }

        /// <summary>
        /// Set the replacement code. </summary>
        /// <param name="ireptype"> replacement code. </param>
        public virtual void setIreptype(string ireptype)
        {
            if (string.ReferenceEquals(ireptype, null))
            {
                return;
            }
            setIreptype(int.Parse(ireptype.Trim()));
        }

        /// <summary>
        /// Set the use type. </summary>
        /// <param name="irturn"> use type. </param>
        public virtual void setIrturn(int irturn)
        {
            if (irturn != _irturn)
            {
                _irturn = irturn;
                setDirty(true);
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_DIVERSION_STATIONS, true);
                }
            }
        }

        /// <summary>
        /// Set the use type. </summary>
        /// <param name="irturn"> use type. </param>
        public virtual void setIrturn(int? irturn)
        {
            setIrturn(irturn.Value);
        }

        /// <summary>
        /// Set the use type. </summary>
        /// <param name="irturn"> use type. </param>
        public virtual void setIrturn(string irturn)
        {
            if (string.ReferenceEquals(irturn, null))
            {
                return;
            }
            setIrturn(int.Parse(irturn.Trim()));
        }

        /// <summary>
        /// Set the user name. </summary>
        /// <param name="username"> user name. </param>
        public virtual void setUsername(string username)
        {
            if (string.ReferenceEquals(username, null))
            {
                return;
            }
            if (!username.Equals(_username))
            {
                _username = username;
                setDirty(true);
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_DIVERSION_STATIONS, true);
                }
            }
        }
    }
}
