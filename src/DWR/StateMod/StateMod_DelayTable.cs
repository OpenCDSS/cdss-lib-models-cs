using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.IO;

// StateMod_DelayTable - Contains information read from the delay table file.

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
// StateMod_DelayTable - Contains information read from the delay table file.
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
// 
// 03 Sep 1997	Catherine E.		Created initial version of class.
//		Nutting-Lane, RTi
// 24 Mar 1998	CEN, RTi		Added setRet_val.
// 21 Dec 1998	CEN, RTi		Added throws IOException to read/write
//					routines.
// 24 Jan 2000	CEN, RTi		Modified to accommodate Ray's new open
//					format(not necessarily 12 entries per
//					line).
// 14 Mar 2000	CEN, RTi		Extends from SMData now to utilize
//					search abilities in GUI(need to use ID
//					field).
// 17 Feb 2001	Steven A. Malers, RTi	Code review.  Change IO to IOUtil.  Add
//					finalize().  Handle nulls.  Set to null
//					when varialbles not used.  Update
//					javadoc.  Alphabetize methods.
// 13 Aug 2001	SAM, RTi		Add int to set the units as percent or
//					fraction.
// 2002-09-19	SAM, RTi		Use isDirty() instead of setDirty() to
//					indicate edits.
//------------------------------------------------------------------------------
// 2003-06-04	J. Thomas Sapienza, RTi	Renamed from SMDelayTbl to 
//					StateMod_DelayTable
// 2003-06-10	JTS, RTi		* Folded dumpDelayTableFile() into
//					  writeDelayTableFile()
// 					* Renamed parseDelayTableFile() to
//					  readDelayTableFile()
// 2003-06-23	JTS, RTi		Renamed writeDelayTableFile() to
//					writeStateModFile()
// 2003-06-26	JTS, RTi		Renamed readDelayTableFile() to
//					readStateModFile()
// 2003-07-07	SAM, RTi		* Javadoc data and parameters that were
//					  not documented.
//					* Remove MAX_DELAYS - not used anywhere.
//					* Remove _table_id since the base class
//					  _id can be used.
//					* Also set the base class name to the
//					  same as the ID.
//					* Check for null data set when reading
//					  data since when using with StateCU
//					  in StateDMI there is no StateMod data
//					  set.
// 2003-07-15	JTS, RTi		Changed code to use new dataset design.
// 2003-08-03	SAM, RTi		Change isDirty() back to setDirty().
// 2004-03-17	SAM, RTi		Add the scale() method to deal with
//					percent/fraction issues.
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

namespace DWR.StateMod
{

    using IOUtil = RTi.Util.IO.IOUtil;
    using Message = RTi.Util.Message.Message;
    using StringUtil = RTi.Util.String.StringUtil;

    public class StateMod_DelayTable : StateMod_Data //, ICloneable, IComparable<StateMod_Data>, StateMod_ComponentValidator
    {

        /// <summary>
        /// Number of return values.
        /// </summary>
        protected internal int _ndly;

        /// <summary>
        /// Double return values.
        /// </summary>
        protected internal IList<double?> _ret_val;

        /// <summary>
        /// Units for the data, as determined at read time.
        /// </summary>
        protected internal string _units;

        /// <summary>
        /// Indicate whether the delay table is for monthly or daily data.
        /// </summary>
        protected internal bool _isMonthly;

        /// <summary>
        /// Constructor. </summary>
        /// <param name="isMonthly"> If true, the delay table contains monthly data.  If false, the
        /// delay table contains daily data. </param>
        public StateMod_DelayTable(bool isMonthly) : base()
        {
            initialize();
            _isMonthly = isMonthly;
        }

        /// <summary>
        /// Add a delay.
        /// </summary>
        public virtual void addRet_val(double d)
        {
            addRet_val(new double?(d));
        }

        /// <summary>
        /// Add a delay
        /// </summary>
        public virtual void addRet_val(double? D)
        {
            _ret_val.Add(D);
            setNdly(_ret_val.Count);
            if (!_isClone && _dataset != null)
            {
                _dataset.setDirty(StateMod_DataSet.COMP_DELAY_TABLES_MONTHLY, true);
            }
        }

        /// <summary>
        /// Add a delay
        /// </summary>
        public virtual void addRet_val(string str)
        {
            if (!string.ReferenceEquals(str, null))
            {
                addRet_val(double.Parse(str.Trim()));
            }
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
        /// Get the number of return values.
        /// </summary>
        public virtual int getNdly()
        {
            return _ndly;
        }

        /// <summary>
        /// Get a delay corresponding to a particular index.
        /// </summary>
        public virtual double getRet_val(int index)
        {
            return (((double?)_ret_val[index]).Value);
        }

        /// <summary>
        /// Get a entire list of delays.
        /// </summary>
        public virtual IList<double?> getRet_val()
        {
            return _ret_val;
        }

        /// <summary>
        /// Return the delay table identifier. </summary>
        /// <returns> the delay table identifier. </returns>
        public virtual string getTableID()
        {
            return _id;
        }

        /// <summary>
        /// Return the delay table units. </summary>
        /// <returns> Units of delay table, consistent with time series data units, etc. </returns>
        public virtual string getUnits()
        {
            return _units;
        }

        /// <summary>
        /// Initialize data members.
        /// </summary>
        private void initialize()
        {
            _smdata_type = StateMod_DataSet.COMP_DELAY_TABLES_MONTHLY;
            _ndly = 0;
            _units = "PCT";
            _ret_val = new List<double?>(1);
        }

        /// <summary>
        /// Read delay information in and store in a java vector.  The new delay entries are
        /// added to the end of the previously stored delays.  Returns the delay table information. </summary>
        /// <param name="filename"> the filename to read from. </param>
        /// <param name="isMonthly"> Set to true if the delay table contains monthly data, false if it contains daily data. </param>
        /// <param name="interv"> The control file interv parameter.  +n indicates the number of
        /// values in each delay pattern.  -1 indicates variable number of values with
        /// values as percent (0-100).  -100 indicates variable number of values with values as fraction (0-1). </param>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public static java.util.List<StateMod_DelayTable> readStateModFile(String filename, boolean isMonthly, int interv) throws Exception
        public static IList<StateMod_DelayTable> readStateModFile(string filename, bool isMonthly, int interv)
        {
            string routine = "StateMod_DelayTable.readStateModFile";
            string iline;
            IList<StateMod_DelayTable> theDelays = new List<StateMod_DelayTable>(1);
            StateMod_DelayTable aDelay = new StateMod_DelayTable(isMonthly);
            int numRead = 0, totalNumToRead = 0;
            bool reading = false;
            StreamReader @in = null;

            if (Message.isDebugOn)
            {
                Message.printDebug(10, routine, "in readStateModFile reading file: " + filename);
            }
            try
            {
                @in = new StreamReader(filename);
                while (!string.ReferenceEquals((iline = @in.ReadLine()), null))
                {
                    // check for comments
                    iline = iline.Trim();
                    Debug.WriteLine(iline);
                    if (iline.StartsWith("#", StringComparison.Ordinal) || iline.Length == 0)
                    {
                        continue;
                    }

                    char[] splitOn = { ' ', '\t', '\r', '\n', '\f' };
                    string[] split = iline.Split(splitOn);
                    Debug.WriteLine(split.ToString());
                    if ((split == null) || (split.Length == 0))
                    {
                        continue;
                    }

                    int splitSize = split.Length;
                    if (!reading)
                    {
                        // allocate new delay node
                        aDelay = new StateMod_DelayTable(isMonthly);
                        numRead = 0;
                        reading = true;
                        theDelays.Add(aDelay);
                        aDelay.setTableID(split[0]);

                        if (interv < 0)
                        {
                            aDelay.setNdly(split[1]);
                        }
                        else
                        {
                            aDelay.setNdly(interv);
                        }
                        totalNumToRead = aDelay.getNdly();
                        // Set the delay table units(default is percent)...
                        aDelay.setUnits("PCT");
                        if (interv == -100)
                        {
                            aDelay.setUnits("FRACTION");
                        }
                    }

                    for (int i = 2; i < splitSize; i++)
                    {
                        aDelay.addRet_val(split[i]);
                        numRead++;
                    }
                    if (numRead >= totalNumToRead)
                    {
                        reading = false;
                    }
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
                @in = null;
            }
            return theDelays;
        }

        /// <summary>
        /// Set the number of return values.
        /// </summary>
        public virtual void setNdly(int i)
        {
            if (i != _ndly)
            {
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_DELAY_TABLES_MONTHLY, true);
                }
                _ndly = i;
            }
        }

        /// <summary>
        /// Set the number of return values.
        /// </summary>
        public virtual void setNdly(int? i)
        {
            setNdly(i.Value);
        }

        /// <summary>
        /// Set the number of return values.
        /// </summary>
        public virtual void setNdly(string str)
        {
            if (!string.ReferenceEquals(str, null))
            {
                setNdly(int.Parse(str.Trim()));
            }
        }

        public virtual void setRet_val(IList<double?> v)
        {
            _ret_val = new List<double?>(v);
            _ndly = _ret_val.Count;
        }

        public virtual void setRet_val(int index, string str)
        {
            setRet_val(index, Convert.ToDouble(str.Trim()));
        }

        public virtual void setRet_val(int index, double d)
        {
            setRet_val(index, new double?(d));
        }

        public virtual void setRet_val(int index, double? d)
        {
            if (d != null)
            {
                if (getNdly() > index)
                {
                    _ret_val[index] = d;
                    if (!_isClone && _dataset != null)
                    {
                        _dataset.setDirty(StateMod_DataSet.COMP_DELAY_TABLES_MONTHLY, true);
                    }
                }
                else
                {
                    addRet_val(d);
                }
            }
        }

        /// <summary>
        /// Set the id.
        /// </summary>
        public virtual void setTableID(string str)
        {
            if (!str.Equals(_id))
            {
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_DELAY_TABLES_MONTHLY, true);
                }
                _id = str;
                // Set the name to the same as the ID...
                _name = str;
            }
        }

        /// <summary>
        /// Set the delay table units. </summary>
        /// <param name="units"> Units of delay table, consistent with time series data units, etc. </param>
        public virtual void setUnits(string units)
        {
            if (!units.Equals(_units))
            {
                if (!string.ReferenceEquals(units, null))
                {
                    if (!_isClone && _dataset != null)
                    {
                        _dataset.setDirty(StateMod_DataSet.COMP_DELAY_TABLES_MONTHLY, true);
                    }
                    _units = units;
                }
            }
        }
    }
}
