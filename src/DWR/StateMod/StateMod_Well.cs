using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

// StateMod_Well - class to store well station

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
// StateMod_Well - Derived from StateMod_Data class
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
// 
// 01 Feb 1999	Catherine E.		Created initial version of class.
//		Nutting-Lane, RTi	
// 10 Feb 1999  CEN, RTi		Implemented changes due to SAM review
//					of code.
// 24 Feb 2000	Steven A. Malers, RTi	Added comments to the right of return
//					flows and depletions as per Ray Bennett.
// 13 Mar 2000	SAM, RTi		Change some variable names to reflect
//					more recent StateMod documentation:
//						cdividwx -> cdividyw
//						cgoto2 -> idvcow2
//						diveffw -> divefcw
// 10 Apr 2000	CEN, RTi		Added "primary" variable
// 09 Aug 2000	SAM, RTi		Change areaw to a double.
// 18 Feb 2001	SAM, RTi		Code review.  Clean up javadoc.  Handle
//					nulls and set unused variables to null.
//					Alphabetize methods.  Update output
//					header.
// 02 Mar 2001	SAM, RTi		Correct problem with 1.7 primary flag
//					being formatted 11.5 rather than 12.5
//					as documented.
// 2001-12-27	SAM, RTi		Update to use new fixedRead()to
//					improve performance.
// 2002-09-09	SAM, RTi		Add GeoRecord reference to allow 2-way
//					connection between spatial and StateMod
//					data.
// 2002-09-19	SAM, RTi		Use isDirty() instead of setDirty() to
//					indicate edits.
//------------------------------------------------------------------------------
// 2003-06-04	J. Thomas Sapienza, RTi	Renamed from SMWell to StateMod_Well
// 2003-06-10	JTS, RTi		* Folded dumpWellFile() into
//					  writeWellFile()
//					* Renamed parseWellFile() to 
//					  readWellFile()
// 2003-06-23	JTS, RTi		Renamed writeWellFile() to
//					writeStateModFile()
// 2003-06-26	JTS, RTi		Renamed readWellFile() to 
//					readStateModFile()
// 2003-07-15	JTS, RTi		Changed to use new dataset design.
// 2003-08-03	SAM, RTi		Change isDirty() back to setDirty().
// 2003-08-15	SAM, RTi		Change GeoRecordNoSwing to GeoRecord.
// 2003-08-28	SAM, RTi		* Change rights to use a simple Vector,
//					  not a linked list.
//					* Clean up parameter names and Javadoc.
//					* Call setDirty() on individual objects
//					  and the data set component.
//					* Remove unneeded "num" data - get from
//					  the Vector size.
//					* Remove redundant "Well" from some
//					  methods.
//					* Support all well-related time series
//					  and clean up names.
// 2003-10-10	SAM, RTi		Add disconnectRights().
// 2003-10-16	SAM, RTi		Change innitial efficiency to 60%.
// 2003-10-21	SAM, RTi		* Add CWR, similar to diversions.
//					* Default idvcow2 to "N/A".
// 2004-02-25	SAM, RTi		Add isStateModWellFile().
// 2004-06-05	SAM, RTi		* Add methods to handle collections,
//					  similar to StateCU locations.
// 2004-07-06	SAM, RTi		* Overload the constructor to allow
//					  initialization to defaults or missing.
// 2004-07-08	SAM, RTi		* Add getPrimaryChoices() and
//					  getPrimaryDefault().
// 2004-08-25	JTS, RTi		* Added acceptChanges().
//					* Added changed().
//					* Added clone().
//					* Added compareTo().
//					* Added createBackup().
//					* Added restoreOriginal().
//					* Now implements Cloneable.
//					* Now implements Comparable.
//					* Clone status is checked via _isClone
//					  when the component is marked as dirty.
// 2004-08-26	JTS, RTi		compareTo() now handles _idvcow2.
// 2004-09-16	SAM, RTi		* Change so read and write methods
//					  adjust the path relative to the
//					  working directory.
// 2004-09-29	SAM, RTi		* Add the following for use with
//					  StateDMI only - no need to check for
//					  dirty - only set/gets on the entire
//					  array are enabled:
//						__cwr_monthly
//						__ddh_monthly
//						__calculated_efficiencies
//						__calculated_efficiency_stddevs
//						__model_efficiecies
// 2004-10-07	SAM, RTi		* Add 6 as an option for idvcomw.
// 2005-04-18	JTS, RTi		* Added writeListFile().
//					* Added writeCollectionListFile().
// 2005-08-18	SAM, RTi		* Add static data for part types to
//					  minimize errors in string use.
// 2005-10-10	SAM, RTi		* Change some javadoc - was using
//					  "diversion" instead of "well".
// 2005-11-16	SAM, RTi		Overload set/get methods for monthly
//					efficiency to take a year type, to
//					facilitate handling of non-calendar
//					year type.
// 2006-01-30	SAM, RTi		* Add hasAssociatedDiversion() to
//					  facilitate processing.
// 2006-04-09	SAM, RTi		Add _parcels_Vector data member and
//					associated methods, to help with
//					StateDMI error handling.
// 2007-04-12	Kurt Tometich, RTi		Added checkComponentData() and
//									getDataHeader() methods for check
//									file and data check support.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
//------------------------------------------------------------------------------
// EndHeader
// TODO SAM 2006-04-09
// The _parcel_Vector has minimal support and is not yet considered in
// copy, clone, equals, etc.

namespace DWR.StateMod
{

    //using StateCU_IrrigationPracticeTS = DWR.StateCU.StateCU_IrrigationPracticeTS;
    //using GeoRecord = RTi.GIS.GeoView.GeoRecord;
    //using HasGeoRecord = RTi.GIS.GeoView.HasGeoRecord;
    using DayTS = RTi.TS.DayTS;
    using MonthTS = RTi.TS.MonthTS;
    //using CommandStatus = RTi.Util.IO.CommandStatus;
    using DataSetComponent = RTi.Util.IO.DataSetComponent;
    using IOUtil = RTi.Util.IO.IOUtil;
    using Message = RTi.Util.Message.Message;
    //using MessageUtil = RTi.Util.Message.MessageUtil;
    using StringUtil = RTi.Util.String.StringUtil;
    using TimeUtil = RTi.Util.Time.TimeUtil;
    using YearType = RTi.Util.Time.YearType;

    /// <summary>
    /// This class stores all relevant data for a StateMod well.  
    /// </summary>

    public class StateMod_Well : StateMod_Data //, ICloneable, IComparable<StateMod_Data>, HasGeoRecord, StateMod_ComponentValidator
    {
        /// <summary>
        /// Well id to use for daily data.
        /// </summary>
        protected internal string _cdividyw;
        /// <summary>
        /// Well capacity(cfs).
        /// </summary>
        private double _divcapw;
        /// <summary>
        /// Diversion this well is tied to ("N/A" if not tied to a diversion).
        /// </summary>
        private string _idvcow2;
        /// <summary>
        /// Demand code.
        /// </summary>
        private int _idvcomw;
        /// <summary>
        /// System efficiency(%).
        /// </summary>
        private double _divefcw;
        /// <summary>
        /// Irrigated area associated with well.
        /// </summary>
        private double _areaw;
        /// <summary>
        /// Use type.
        /// </summary>
        private int _irturnw;
        /// <summary>
        /// Irrigated acreage source.
        /// </summary>
        private int _demsrcw;
        /// <summary>
        /// 12 efficiency values. 
        /// </summary>
        private double[] _diveff;
        /// <summary>
        /// Return flow data.
        /// </summary>
        private IList<StateMod_ReturnFlow> _rivret;
        /// <summary>
        /// Depletion data.
        /// </summary>
        private IList<StateMod_ReturnFlow> _depl;
        /// <summary>
        /// Historical time series (monthly).
        /// </summary>
        private MonthTS _pumping_MonthTS;
        /// <summary>
        /// 12 monthly and annual average over period, used by StateDMI.
        /// </summary>
        private double[] __weh_monthly = null;
        /// <summary>
        /// Historical time series (daily).
        /// </summary>
        private DayTS _pumping_DayTS;
        /// <summary>
        /// Demand time series.
        /// </summary>
        private MonthTS _demand_MonthTS;
        /// <summary>
        /// Daily demand time series.
        /// </summary>
        private DayTS _demand_DayTS;
        /// <summary>
        /// Irrigation practice time series.
        /// </summary>
        //private StateCU_IrrigationPracticeTS _ipy_YearTS;
        /// <summary>
        /// Consumptive water requirement.
        /// </summary>
        private MonthTS _cwr_MonthTS;
        /// <summary>
        /// 12 monthly and annual average over period, used by StateDMI.
        /// </summary>
        private double[] __cwr_monthly = null;
        /// <summary>
        /// Time series - only used when idvcow2 is "N/A".
        /// </summary>
        private DayTS _cwr_DayTS;
        /// <summary>
        /// Well rights.
        /// </summary>
        private IList<StateMod_WellRight> _rights;
        /// <summary>
        /// Priority switch.
        /// </summary>
        private double _primary;
        /// <summary>
        /// Link to spatial data.
        /// </summary>
        //private GeoRecord _georecord;

        /// <summary>
        /// List of parcel data, in particular to allow StateDMI to detect when a well had no data.
        /// </summary>
        protected internal IList<StateMod_Parcel> _parcel_Vector = new List<StateMod_Parcel>();

        // Collections are set up to be specified by year for wells, using parcels as the parts.

        /// <summary>
        /// Types of collections.  An aggregate merges the water rights/permits whereas
        /// a system keeps all the water rights but just has one ID.
        /// </summary>
        public static string COLLECTION_TYPE_AGGREGATE = "Aggregate";
        public static string COLLECTION_TYPE_SYSTEM = "System";

        /// <summary>
        /// How aggregates are specified, as collection of ditches, parcels, or wells.
        /// If wells, see COLLECTION_WELL_PART_ID_TYPE_*.
        /// </summary>
        public static string COLLECTION_PART_TYPE_DITCH = "Ditch";
        public static string COLLECTION_PART_TYPE_PARCEL = "Parcel";
        public static string COLLECTION_PART_TYPE_WELL = "Well";

        /// <summary>
        /// Indicates the type of aggregate part ID when aggregating wells.
        /// </summary>
        public static string COLLECTION_WELL_PART_ID_TYPE_WDID = "WDID";
        public static string COLLECTION_WELL_PART_ID_TYPE_RECEIPT = "Receipt";

        private string __collection_type = StateMod_Util.MISSING_STRING;

        /// <summary>
        /// Used by DMI software - currently no options.
        /// </summary>
        private string __collection_part_type = COLLECTION_PART_TYPE_PARCEL;

        /// <summary>
        /// The identifiers for data that are collected - null if not a collection
        /// location.  This is a List of Lists corresponding to each __collectionYear element.
        /// If the list of identifiers is consistent for the entire period then the
        /// __collectionYear array will have a size of 0 and the __collectionIDList will be a single list.
        /// </summary>
        private IList<IList<string>> __collectionIDList = null;

        /// <summary>
        /// The identifiers types for data that are collected - null if not a collection
        /// location.  This is a List of Lists corresponding to each __collectionYear element.
        /// If the list of identifiers is consistent for the entire period then the
        /// __collectionYear array will have a size of 0 and the __collectionIDTypeList will be a single list.
        /// This list is only used for well collections that use well identifiers for the parts.
        /// </summary>
        private IList<IList<string>> __collectionIDTypeList = null;

        /// <summary>
        /// An array of years that correspond to the aggregate/system.  Well collections are defined by year.
        /// </summary>
        private int[] __collectionYear = null;

        /// <summary>
        /// The division that corresponds to the aggregate/system.  Currently it is expected that the same division
        /// number is assigned to all the data.
        /// </summary>
        private int __collection_div = StateMod_Util.MISSING_INT;

        /// <summary>
        /// The following are used only by StateDMI and do not needed to be handled in
        /// comparison, initialization, etc.
        /// </summary>
        private double[] __calculated_efficiencies = null;
        private double[] __calculated_efficiency_stddevs = null;
        private double[] __model_efficiencies = null;

        /// <summary>
        /// Constructor - initialize to default values.
        /// </summary>
        public StateMod_Well() : this(true)
        {
        }

        /// <summary>
        /// Add return flow node to the vector of return flow nodes.  Also, updates
        /// the number of return flow nodes variable. </summary>
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
                _dataset.setDirty(StateMod_DataSet.COMP_WELL_STATIONS, true);
            }
        }

        /// <summary>
        /// Constructor. </summary>
        /// <param name="initialize_defaults"> If true, initialize data to reasonable values.  If
        /// false, initialize to missing values. </param>
        public StateMod_Well(bool initialize_defaults) : base()
        {
            initialize(initialize_defaults);
        }

        /// <summary>
        /// Add depletion node to the vector of depletion nodes.  Updates the variable
        /// which tracks the number of depletion nodes for this well. </summary>
        /// <param name="depl"> depletion data object </param>
        public virtual void addDepletion(StateMod_ReturnFlow depl)
        {
            if (depl == null)
            {
                return;
            }
            _depl.Add(depl);
            setDirty(true);
            if (!_isClone && _dataset != null)
            {
                _dataset.setDirty(StateMod_DataSet.COMP_WELL_STATIONS, true);
            }
        }

        /// <summary>
        /// Connect all water rights to the corresponding wells.
        /// This routines doesn't add an element to an array - the array
        /// already exists. This method just connects next and previous pointers. </summary>
        /// <param name="wells"> all wells </param>
        /// <param name="rights"> all rights </param>
        public static void connectAllRights(IList<StateMod_Well> wells, IList<StateMod_WellRight> rights)
        {
            if ((wells == null) || (rights == null))
            {
                return;
            }
            int num_wells = wells.Count;

            StateMod_Well well = null;
            for (int i = 0; i < num_wells; i++)
            {
                well = wells[i];
                if (well == null)
                {
                    continue;
                }
                well.connectRights(rights);
            }
        }

        /// <summary>
        /// Create a list of references to rights for this well. </summary>
        /// <param name="rights"> all rights </param>
        public virtual void connectRights(IList<StateMod_WellRight> rights)
        {
            if (rights == null)
            {
                return;
            }

            int num_rights = rights.Count;

            StateMod_WellRight right = null;
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

        /*
	    @return the system efficiency
	    */
        public virtual double getDivefcw()
        {
            return _divefcw;
        }

        /// <summary>
        /// Set default values for all arguments </summary>
        /// <param name="initialize_defaults"> If true, initialize data to reasonable values.  If
        /// false, initialize to missing values. </param>
        private void initialize(bool initialize_defaults)
        {
            _smdata_type = StateMod_DataSet.COMP_WELL_STATIONS;
            _rivret = new List<StateMod_ReturnFlow>(10);
            _depl = new List<StateMod_ReturnFlow>(10);
            _pumping_MonthTS = null;
            _pumping_DayTS = null;
            _demand_MonthTS = null;
            _demand_DayTS = null;
            //_ipy_YearTS = null;
            _cwr_MonthTS = null;
            _cwr_DayTS = null;
            _rights = new List<StateMod_WellRight>();
            //_georecord = null;

            if (initialize_defaults)
            {
                _cdividyw = "0"; // Estimate average daily from monthly data.
                _idvcow2 = "N/A";
                _diveff = new double[12];
                for (int i = 0; i < 12; i++)
                {
                    _diveff[i] = 60.0;
                }
                _divcapw = 0;
                _idvcomw = 1;
                _divefcw = -60.0; // Indicate to use monthly efficiencies
                _areaw = 0.0;
                _irturnw = 0;
                _demsrcw = 1;
                _primary = 0;
            }
            else
            {
                _cdividyw = "";
                _idvcow2 = "";
                _diveff = new double[12];
                for (int i = 0; i < 12; i++)
                {
                    _diveff[i] = StateMod_Util.MISSING_DOUBLE;
                }
                _divcapw = StateMod_Util.MISSING_DOUBLE;
                _idvcomw = StateMod_Util.MISSING_INT;
                _divefcw = StateMod_Util.MISSING_DOUBLE;
                _areaw = StateMod_Util.MISSING_DOUBLE;
                _irturnw = StateMod_Util.MISSING_INT;
                _demsrcw = StateMod_Util.MISSING_INT;
                _primary = StateMod_Util.MISSING_INT;
            }
        }

        /// <summary>
        /// Read a well input file. </summary>
        /// <param name="filename"> name of file containing well information </param>
        /// <returns> status(always 0 since exception handling is now used) </returns>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public static java.util.List<StateMod_Well> readStateModFile(String filename) throws Exception
        public static IList<StateMod_Well> readStateModFile(string filename)
        {
            string routine = "StateMod_Well.readStateModFile";
            IList<StateMod_Well> theWellStations = new List<StateMod_Well>();
            int[] format_1 = new int[] { StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_INTEGER, StringUtil.TYPE_DOUBLE, StringUtil.TYPE_SPACE, StringUtil.TYPE_STRING, StringUtil.TYPE_SPACE, StringUtil.TYPE_DOUBLE };
            int[] format_1w = new int[] { 12, 24, 12, 8, 8, 1, 12, 1, 12 };
            int[] format_2 = new int[] { StringUtil.TYPE_SPACE, StringUtil.TYPE_STRING, StringUtil.TYPE_INTEGER, StringUtil.TYPE_INTEGER, StringUtil.TYPE_INTEGER, StringUtil.TYPE_DOUBLE, StringUtil.TYPE_DOUBLE, StringUtil.TYPE_INTEGER, StringUtil.TYPE_INTEGER };
            int[] format_2w = new int[] { 36, 12, 8, 8, 8, 8, 8, 8, 8 };
            int[] format_4 = new int[] { StringUtil.TYPE_SPACE, StringUtil.TYPE_STRING, StringUtil.TYPE_DOUBLE, StringUtil.TYPE_INTEGER };
            int[] format_4w = new int[] { 36, 12, 8, 8 };
            string iline = null;
            string s = null;
            IList<object> v = new List<object>(9);
            StreamReader @in = null;
            StateMod_Well aWell = null;
            StateMod_ReturnFlow aReturnNode = null;
            IList<string> effv = null;
            int nrtn, ndepl;

            Message.printStatus(1, routine, "Reading well file: " + filename);

            try
            {
                @in = new StreamReader(IOUtil.getPathUsingWorkingDir(filename));
                while (!string.ReferenceEquals((iline = @in.ReadLine()), null))
                {
                    if (iline.StartsWith("#", StringComparison.Ordinal) || iline.Trim().Length == 0)
                    {
                        continue;
                    }

                    aWell = new StateMod_Well();

                    StringUtil.fixedRead(iline, format_1, format_1w, v);
                    if (Message.isDebugOn)
                    {
                        Message.printDebug(50, routine, "iline: " + iline);
                    }
                    aWell.setID(((string)v[0]).Trim());
                    aWell.setName(((string)v[1]).Trim());
                    aWell.setCgoto(((string)v[2]).Trim());
                    aWell.setSwitch((int?)v[3]);
                    aWell.setDivcapw((double?)v[4]);
                    aWell.setCdividyw(((string)v[5]).Trim());
                    aWell.setPrimary((double?)v[6]);

                    // user data

                    iline = @in.ReadLine();
                    StringUtil.fixedRead(iline, format_2, format_2w, v);
                    if (Message.isDebugOn)
                    {
                        Message.printDebug(50, routine, "iline: " + iline);
                    }
                    aWell.setIdvcow2(((string)v[0]).Trim());
                    aWell.setIdvcomw((int?)v[1]);
                    // Don't set the number of return flow data get(2) or depletion data get(3) because
                    // those will be calculated
                    nrtn = ((int?)v[2]).Value;
                    ndepl = ((int?)v[3]).Value;
                    aWell.setDivefcw((double?)v[4]);
                    aWell.setAreaw((double?)v[5]);
                    aWell.setIrturnw((int?)v[6]);
                    aWell.setDemsrcw((int?)v[7]);
                    if (aWell.getDivefcw() >= 0)
                    {
                        // Efficiency line won't be included - set each value to the average
                        for (int i = 0; i < 12; i++)
                        {
                            aWell.setDiveff(i, aWell.getDivefcw());
                        }
                    }
                    else
                    {
                        // 12 efficiency values
                        iline = @in.ReadLine();
                        effv = StringUtil.breakStringList(iline, " ", StringUtil.DELIM_SKIP_BLANKS);
                        for (int i = 0; i < 12; i++)
                        {
                            aWell.setDiveff(i, (string)effv[i]);
                        }
                    }

                    // return flow data

                    if (Message.isDebugOn)
                    {
                        Message.printDebug(50, routine, "Number of return flows: " + nrtn);
                    }
                    for (int i = 0; i < nrtn; i++)
                    {
                        iline = @in.ReadLine();
                        StringUtil.fixedRead(iline, format_4, format_4w, v);
                        if (Message.isDebugOn)
                        {
                            Message.printDebug(50, routine, "Fixed read returned " + v.Count + " elements");
                        }

                        aReturnNode = new StateMod_ReturnFlow(StateMod_DataSet.COMP_WELL_STATIONS);
                        s = ((string)v[0]).Trim();
                        if (s.Length <= 0)
                        {
                            aReturnNode.setCrtnid(s);
                            Message.printWarning(2, routine, "Return node for structure \"" + aWell.getID() + "\" is blank. ");
                        }
                        else
                        {
                            aReturnNode.setCrtnid(s);
                        }

                        aReturnNode.setPcttot(((double?)v[1]));
                        aReturnNode.setIrtndl(((int?)v[2]));
                        aWell.addReturnFlow(aReturnNode);
                    }

                    // depletion data

                    for (int i = 0; i < ndepl; i++)
                    {
                        iline = @in.ReadLine();
                        StringUtil.fixedRead(iline, format_4, format_4w, v);

                        aReturnNode = new StateMod_ReturnFlow(StateMod_DataSet.COMP_WELL_STATIONS);
                        s = ((string)v[0]).Trim();
                        if (s.Length <= 0)
                        {
                            aReturnNode.setCrtnid(s);
                            Message.printWarning(2, routine, "Return node for structure \"" + aWell.getID() + "\" is blank. ");
                        }
                        else
                        {
                            aReturnNode.setCrtnid(s);
                        }

                        aReturnNode.setPcttot(((double?)v[1]));
                        aReturnNode.setIrtndl(((int?)v[2]));
                        aWell.addDepletion(aReturnNode);
                    }

                    theWellStations.Add(aWell);
                }
            }
            catch (Exception e)
            {
                routine = null;
                format_1 = null;
                format_1w = null;
                format_2 = null;
                format_2w = null;
                format_4 = null;
                format_4w = null;
                s = null;
                v = null;
                if (@in != null)
                {
                    @in.Close();
                }
                @in = null;
                aWell = null;
                aReturnNode = null;
                effv = null;
                Message.printWarning(2, routine, e);
                throw e;
            }
            routine = null;
            format_1 = null;
            format_1w = null;
            format_2 = null;
            format_2w = null;
            format_4 = null;
            format_4w = null;
            iline = null;
            s = null;
            v = null;
            if (@in != null)
            {
                @in.Close();
            }
            @in = null;
            aWell = null;
            aReturnNode = null;
            effv = null;
            return theWellStations;
        }

        /// <summary>
        /// Set the area. </summary>
        /// <param name="area"> area </param>
        public virtual void setAreaw(double area)
        {
            if (area != _areaw)
            {
                _areaw = area;
                setDirty(true);
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_WELL_STATIONS, true);
                }
            }
        }

        /// <summary>
        /// Set the well area. </summary>
        /// <param name="area"> well area(AF) </param>
        public virtual void setAreaw(double? area)
        {
            if (area == null)
            {
                return;
            }
            setAreaw(area.Value);
        }

        /// <summary>
        /// Set the area(This is currently not being used but is provided for
        /// consistency within this class). </summary>
        /// <param name="area"> area </param>
        public virtual void setAreaw(string area)
        {
            if (string.ReferenceEquals(area, null))
            {
                return;
            }
            setAreaw(double.Parse(area.Trim()));
        }

        /// <summary>
        /// Set the well id to use for daily data </summary>
        /// <param name="cdividyw"> well id to use for daily data </param>
        public virtual void setCdividyw(string cdividyw)
        {
            if (string.ReferenceEquals(cdividyw, null))
            {
                return;
            }
            if (!cdividyw.Equals(_cdividyw))
            {
                _cdividyw = cdividyw.Trim();
                setDirty(true);
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_WELL_STATIONS, true);
                }
            }
        }

        /// <summary>
        /// Set the irrigated acreage source(see StateMod documentation for list of available sources). </summary>
        /// <param name="demsrcw"> source for irrigated acreage </param>
        public virtual void setDemsrcw(int demsrcw)
        {
            if (demsrcw != _demsrcw)
            {
                _demsrcw = demsrcw;
                setDirty(true);
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_WELL_STATIONS, true);
                }
            }
        }

        /// <summary>
        /// Set the irrigated acreage source(see StateMod documentation for list of available sources). </summary>
        /// <param name="demsrcw"> source for irrigated acreage </param>
        public virtual void setDemsrcw(int? demsrcw)
        {
            if (demsrcw == null)
            {
                return;
            }
            setDemsrcw(demsrcw.Value);
        }

        /// <summary>
        /// Set the irrigated acreage source(see StateMod documentation for list of available sources). </summary>
        /// <param name="demsrcw"> source for irrigated acreage </param>
        public virtual void setDemsrcw(string demsrcw)
        {
            if (string.ReferenceEquals(demsrcw, null))
            {
                return;
            }
            setDemsrcw(int.Parse(demsrcw.Trim()));
        }

        /// <summary>
        /// Set the well capacity </summary>
        /// <param name="divcapw"> well capacity(cfs) </param>
        public virtual void setDivcapw(double divcapw)
        {
            if (divcapw != _divcapw)
            {
                _divcapw = divcapw;
                setDirty(true);
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_WELL_STATIONS, true);
                }
            }
        }

        /// <summary>
        /// Set the well capacity </summary>
        /// <param name="divcapw"> well capacity(cfs) </param>
        public virtual void setDivcapw(double? divcapw)
        {
            if (divcapw == null)
            {
                return;
            }
            setDivcapw(divcapw.Value);
        }

        /// <summary>
        /// Set the well capacity </summary>
        /// <param name="divcapw"> well capacity(cfs) </param>
        public virtual void setDivcapw(string divcapw)
        {
            if (string.ReferenceEquals(divcapw, null))
            {
                return;
            }
            setDivcapw(double.Parse(divcapw.Trim()));
        }

        /// <summary>
        /// Set the system efficiency </summary>
        /// <param name="divefcw"> efficiency of the system.  If negative, 12 efficiency values will be used. </param>
        public virtual void setDivefcw(double divefcw)
        {
            if (divefcw != _divefcw)
            {
                _divefcw = divefcw;
                setDirty(true);
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_WELL_STATIONS, true);
                }
            }
        }

        /// <summary>
        /// Set the system efficiency </summary>
        /// <param name="divefcw"> efficiency of the system.  If negative, 12 efficiency values will be used. </param>
        /// <seealso cref= StateMod_Well#setDiveff </seealso>
        public virtual void setDivefcw(double? divefcw)
        {
            if (divefcw == null)
            {
                return;
            }
            setDivefcw(divefcw.Value);
        }

        /// <summary>
        /// Set the system efficiency </summary>
        /// <param name="divefcw"> efficiency of the system.  If negative, 12 efficiency values will be used. </param>
        /// <seealso cref= StateMod_Well#setDiveff </seealso>
        public virtual void setDivefcw(string divefcw)
        {
            if (string.ReferenceEquals(divefcw, null))
            {
                return;
            }
            setDivefcw(double.Parse(divefcw.Trim()));
        }

        /// <summary>
        /// Set the variable efficiency </summary>
        /// <param name="index"> index of month for which to set efficiency(0-11) </param>
        /// <param name="diveff"> eff value(0-100) </param>
        public virtual void setDiveff(int index, double? diveff)
        {
            if (diveff == null)
            {
                return;
            }
            setDiveff(index, diveff.Value);
        }

        /// <summary>
        /// Set the system efficiency for a particular month.
        /// The efficiencies are specified with month 0 being January. </summary>
        /// <param name="index"> month index (0=January). </param>
        /// <param name="diveff"> monthly efficiency </param>
        /// <param name="yeartype"> The year type for the diversion stations file (consistent with
        /// the control file for a full data set). </param>
        public virtual void setDiveff(int index, double diveff, YearType yeartype)
        { // Adjust the index if necessary based on the year type...
            if (yeartype == null)
            {
                // Assume calendar.
            }
            else if (yeartype == YearType.WATER)
            {
                index = TimeUtil.convertCalendarMonthToCustomMonth((index + 1), 10) - 1;
            }
            else if (yeartype == YearType.NOV_TO_OCT)
            {
                index = TimeUtil.convertCalendarMonthToCustomMonth((index + 1), 11) - 1;
            }
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
        /// Set the variable efficiency </summary>
        /// <param name="index"> index of month for which to set efficiency(0-11) </param>
        /// <param name="diveff"> eff value(0-100) </param>
        public virtual void setDiveff(int index, double diveff)
        {
            if (index < 0 || index > 11)
            {
                Message.printWarning(2, "setDiveff", "Unable to set efficiency for month index " + index);
                return;
            }
            if (diveff != _diveff[index])
            {
                _diveff[index] = diveff;
                setDirty(true);
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_WELL_STATIONS, true);
                }
            }
        }

        /// <summary>
        /// Set the variable efficiency </summary>
        /// <param name="index"> index of month for which to set efficiency(0-11) </param>
        /// <param name="diveff"> eff value(0-100) </param>
        public virtual void setDiveff(int index, string diveff)
        {
            if (string.ReferenceEquals(diveff, null))
            {
                return;
            }
            setDiveff(index, double.Parse(diveff.Trim()));
        }

        /// <summary>
        /// Set the demand code </summary>
        /// <param name="idvcomw"> demand code to use </param>
        public virtual void setIdvcomw(int idvcomw)
        {
            if (idvcomw != _idvcomw)
            {
                _idvcomw = idvcomw;
                setDirty(true);
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_WELL_STATIONS, true);
                }
            }
        }

        /// <summary>
        /// Set the demand code </summary>
        /// <param name="idvcomw"> demand code to use </param>
        public virtual void setIdvcomw(int? idvcomw)
        {
            if (idvcomw == null)
            {
                return;
            }
            setIdvcomw(idvcomw.Value);
        }

        /// <summary>
        /// Set the demand code </summary>
        /// <param name="idvcomw"> demand code to use </param>
        public virtual void setIdvcomw(string idvcomw)
        {
            if (string.ReferenceEquals(idvcomw, null))
            {
                return;
            }
            setIdvcomw(int.Parse(idvcomw.Trim()));
        }

        /// <summary>
        /// Set the diversion this well is tied to </summary>
        /// <param name="idvcow2"> diversion this well is tied to </param>
        public virtual void setIdvcow2(string idvcow2)
        {
            if (string.ReferenceEquals(idvcow2, null))
            {
                return;
            }
            if (!idvcow2.Equals(_idvcow2))
            {
                _idvcow2 = idvcow2;
                setDirty(true);
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_WELL_STATIONS, true);
                }
            }
        }

        /// <summary>
        /// Set the use type </summary>
        /// <param name="irturnw"> use type </param>
        public virtual void setIrturnw(int irturnw)
        {
            if (irturnw != _irturnw)
            {
                _irturnw = irturnw;
                setDirty(true);
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_WELL_STATIONS, true);
                }
            }
        }

        /// <summary>
        /// Set the use type </summary>
        /// <param name="irturnw"> use type </param>
        public virtual void setIrturnw(int? irturnw)
        {
            if (irturnw == null)
            {
                return;
            }
            setIrturnw(irturnw.Value);
        }

        /// <summary>
        /// Set the use type </summary>
        /// <param name="irturnw"> use type </param>
        public virtual void setIrturnw(string irturnw)
        {
            if (string.ReferenceEquals(irturnw, null))
            {
                return;
            }
            setIrturnw(int.Parse(irturnw.Trim()));
        }

        /// <summary>
        /// Set the Priority switch </summary>
        /// <param name="primary"> 0 = off; +n = on, adjust well rights by -n </param>
        public virtual void setPrimary(double primary)
        {
            if (primary != _primary)
            {
                _primary = primary;
                setDirty(true);
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_WELL_STATIONS, true);
                }
            }
        }

        /// <summary>
        /// Set the Priority switch </summary>
        /// <param name="primary">  0 = off; +n = on, adjust well rights by -n </param>
        public virtual void setPrimary(double? primary)
        {
            if (primary == null)
            {
                return;
            }
            setPrimary(primary.Value);
        }

        /// <summary>
        /// Set the Priority switch </summary>
        /// <param name="primary"> 0 = off; +n = on, adjust well rights by -n </param>
        public virtual void setPrimary(string primary)
        {
            if (string.ReferenceEquals(primary, null))
            {
                return;
            }
            setPrimary(double.Parse(primary.Trim()));
        }
    }
}
