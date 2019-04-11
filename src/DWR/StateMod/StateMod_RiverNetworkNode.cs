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
    using Message = RTi.Util.Message.Message;
    using StringUtil = RTi.Util.String.StringUtil;

    /// <summary>
	/// This StateMod_RiverNetworkNode class manages a record of data from the StateMod
	/// river network (.rin) file.  It is derived from StateMod_Data similar to other
	/// StateMod data objects.  It should not be confused with network node objects
	/// (e.g., StateMod_Diversion_Node).   See the readStateModFile() method to read
	/// the .rin file into a true network.
	/// </summary>
	public class StateMod_RiverNetworkNode : StateMod_Data //, ICloneable, IComparable<StateMod_Data>, HasGeoRecord, StateMod_ComponentValidator
    {

        /// <summary>
        /// Downstream node identifier - third column of files.
        /// </summary>
        protected internal string _cstadn;

        /// <summary>
        /// Used with .rin (column 5) - ground water maximum recharge limit.
        /// </summary>
        protected internal double _gwmaxr;

        /// <summary>
        /// Constructor.  The time series are set to null and other information is empty strings.
        /// </summary>
        public StateMod_RiverNetworkNode() : base()
        {
            initialize();
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
            format_0w = new int[7];
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
                        aRiverNode.setGwmaxr(double.Parse(s).ToString());
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
        /// Set the maximum recharge limit for network file. </summary>
        /// <param name="gwmaxr"> Maximum recharge limit. </param>
        public virtual void setGwmaxr(string gwmaxr)
        {
            if (StringUtil.isDouble(gwmaxr))
            {
                setGwmaxr(double.Parse(gwmaxr));
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

        public override string ToString()
        {
            return "ID: " + _id + "    Downstream node: " + _cstadn;
        }
    }
}
