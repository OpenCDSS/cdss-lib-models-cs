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

    //using GeoRecord = RTi.GIS.GeoView.GeoRecord;
    //using HasGeoRecord = RTi.GIS.GeoView.HasGeoRecord;
    using DayTS = RTi.TS.DayTS;
    using MonthTS = RTi.TS.MonthTS;
    using TS = RTi.TS.TS;
    using DataSetComponent = RTi.Util.IO.DataSetComponent;
    using IOUtil = RTi.Util.IO.IOUtil;
    using Message = RTi.Util.Message.Message;
    using StringUtil = RTi.Util.String.StringUtil;

    public class StateMod_StreamEstimate : StateMod_Data //, ICloneable, IComparable<StateMod_Data>, HasGeoRecord, StateMod_ComponentValidator
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
        //protected internal GeoRecord _georecord;

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
            //_georecord = null;
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
    }
}
