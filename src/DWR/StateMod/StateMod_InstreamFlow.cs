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

    //using GeoRecord = RTi.GIS.GeoView.GeoRecord;
    //using HasGeoRecord = RTi.GIS.GeoView.HasGeoRecord;
    using DayTS = RTi.TS.DayTS;
    using MonthTS = RTi.TS.MonthTS;
    using DataSetComponent = RTi.Util.IO.DataSetComponent;
    using IOUtil = RTi.Util.IO.IOUtil;
    using Message = RTi.Util.Message.Message;
    using StringUtil = RTi.Util.String.StringUtil;

    public class StateMod_InstreamFlow : StateMod_Data //, IComparable<StateMod_Data>, HasGeoRecord, StateMod_ComponentValidator
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
        //protected internal GeoRecord _georecord = null;

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
            //_georecord = null;
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
            int[] format_0 = new int[] { StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_INTEGER, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING, StringUtil.TYPE_STRING };
            int[] format_0w = new int[] { 12, 24, 12, 8, 1, 12, 1, 12, 8 };

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
                    _dataset.setDirty(StateMod_DataSet.COMP_INSTREAM_STATIONS, true);
                }
            }
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
                    _dataset.setDirty(StateMod_DataSet.COMP_INSTREAM_STATIONS, true);
                }
            }
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
                    _dataset.setDirty(StateMod_DataSet.COMP_INSTREAM_STATIONS, true);
                }
            }
        }

        /// <summary>
        /// Set the value for iifcom for the StateMod instream flow station.
        /// </summary>
        public virtual void setIifcom(string iifcom)
        {
            setIifcom(int.Parse(iifcom));
        }
    }
}
