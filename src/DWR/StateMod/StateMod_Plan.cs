using System;
using System.Collections.Generic;
using System.IO;

// StateMod_Plan - class to store plan information

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

namespace DWR.StateMod
{

    //using GeoRecord = RTi.GIS.GeoView.GeoRecord;
    //using HasGeoRecord = RTi.GIS.GeoView.HasGeoRecord;
    using IOUtil = RTi.Util.IO.IOUtil;
    using Message = RTi.Util.Message.Message;
    using StringUtil = RTi.Util.String.StringUtil;

    /// <summary>
    /// Object used to store plan information.  All set routines set
    /// the COMP_PLANS flag dirty.  A new object will have empty non-null
    /// lists, null time series, and defaults for all other data.
    /// </summary>
    public class StateMod_Plan : StateMod_Data //, ICloneable, IComparable<StateMod_Data>, HasGeoRecord
    {

        // ID, name, river node (cgoto), and switch are in the base class.

        /// <summary>
        /// Plan type.
        /// </summary>
        protected internal int _iPlnTyp;

        /// <summary>
        /// Plan efficiency flag.
        /// </summary>
        protected internal int _PeffFlag;

        /// <summary>
        /// Plan efficiency.
        /// </summary>
        protected internal double[] _Peff = new double[12]; // only used if _PeffFlag = 1, but set aside memory

        /// <summary>
        /// Return flow table.
        /// </summary>
        protected internal int _iPrf;

        /// <summary>
        /// Plan failure switch.
        /// </summary>
        protected internal int _iPfail;

        /// <summary>
        /// Initial plan storage value (AF).
        /// </summary>
        protected internal double _Psto1;

        /// <summary>
        /// Source ID of structure where reuse water became available or a T&C condition
        /// originated (for type 8).
        /// </summary>
        protected internal string _Psource;

        /// <summary>
        /// Source account of structure where reuse water became available or a T&C condition
        /// originated (for type 8).  Treat as a string but is an integer in StateMod so right-justify output.
        /// </summary>
        protected internal string _iPAcc;

        /// <summary>
        /// Reference to spatial data for this plan -- currently NOT cloned.
        /// </summary>
        //protected internal GeoRecord _georecord;

        /// <summary>
        /// Comments provided by user - # comments before each plan.  An empty (non-null) list is guaranteed.
        /// TODO SAM 2010-12-14 Evaluate whether this can be in StateMod_Data or will it bloat memory.
        /// </summary>
        protected internal IList<string> __commentsBeforeData = new List<string>();

        /// <summary>
        /// Construct a new plan and assign data to reasonable defaults.
        /// </summary>
        public StateMod_Plan() : base()
        {
            initialize(true);
        }

        /// <summary>
        /// Construct a new plan. </summary>
        /// <param name="initialize_defaults"> If true, assign data to reasonable defaults.
        /// If false, all data are set to missing. </param>
        public StateMod_Plan(bool initialize_defaults) : base()
        {
            initialize(initialize_defaults);
        }

        /// <summary>
        /// Copy constructor. </summary>
        /// <param name="deep_copy"> If true, make a deep copy including secondary vectors of data.
        /// Currently only false is recognized, in which primitive data are copied.  This is
        /// suitable to allow the StateMod_Plan_JFrame class to know when changes have
        /// been made to data on the main screen. </param>
        public StateMod_Plan(StateMod_Plan plan, bool deep_copy) : this()
        {
            // Base class...
            // TODO
            // Local data members...
            _iPlnTyp = plan._iPlnTyp;
            _PeffFlag = plan._PeffFlag;
            _iPrf = plan._iPrf;
            _iPfail = plan._iPfail;
            _Psto1 = plan._Psto1;
            _Psource = plan._Psource;
            //_georecord = plan._georecord;
        }

        /// <summary>
        /// Return the comments from the input file that immediate precede the data. </summary>
        /// <returns> the comments from the input file that immediate precede the data. </returns>
        public virtual IList<string> getCommentsBeforeData()
        {
            return __commentsBeforeData;
        }

        /// <summary>
        /// Return the plan efficiency flag.
        /// </summary>
        public virtual int getPeffFlag()
        {
            return _PeffFlag;
        }

        /// <summary>
        /// Return the plan type.
        /// </summary>
        public virtual int getIPlnTyp()
        {
            return _iPlnTyp;
        }

        /// <summary>
        /// Initialize data.
        /// Sets the smdata_type to _dataset.COMP_PLANS. </summary>
        /// <param name="initialize_defaults"> If true, assign data to reasonable defaults.
        /// If false, all data are set to missing. </param>
        private void initialize(bool initialize_defaults)
        {
            _smdata_type = StateMod_DataSet.COMP_PLANS;
            if (initialize_defaults)
            {
                _iPlnTyp = 1;
                _PeffFlag = 999;
                _iPrf = 999;
                _iPfail = 0;
                _Psto1 = 0.0;
                _Psource = "";
                _iPAcc = "";
            }
            else
            {
                // Use missing data...
                _iPlnTyp = StateMod_Util.MISSING_INT;
                _PeffFlag = StateMod_Util.MISSING_INT;
                _iPrf = StateMod_Util.MISSING_INT;
                _iPfail = StateMod_Util.MISSING_INT;
                _Psto1 = StateMod_Util.MISSING_DOUBLE;
                _Psource = StateMod_Util.MISSING_STRING;
                _iPAcc = StateMod_Util.MISSING_STRING;
            }
            //_georecord = null;
        }

        /// <summary>
        /// Read plan information in and store in a list. </summary>
        /// <param name="filename"> filename containing plan information </param>
        /// <exception cref="Exception"> if an error occurs </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public static java.util.List<StateMod_Plan> readStateModFile(String filename) throws Exception
        public static IList<StateMod_Plan> readStateModFile(string filename)
        {
            string routine = "StateMod_Plan.readStateModFile";
            string iline = null;
            IList<string> v = new List<string>(9);
            IList<StateMod_Plan> thePlans = new List<StateMod_Plan>();
            int linecount = 0;

            StateMod_Plan aPlan = null;
            StreamReader @in = null;

            Message.printStatus(2, routine, "Reading plan file: " + filename);
            int size = 0;
            int errorCount = 0;
            try
            {
                @in = new StreamReader(IOUtil.getPathUsingWorkingDir(filename));
                IList<string> commentsBeforeData = new List<string>();
                while (!string.ReferenceEquals((iline = @in.ReadLine()), null))
                {
                    ++linecount;
                    // check for comments
                    if (iline.StartsWith("#>", StringComparison.Ordinal) || (iline.Trim().Length == 0))
                    {
                        // Special dynamic header comments written by software and blank lines - no need to keep
                        continue;
                    }
                    else if (iline.StartsWith("#", StringComparison.Ordinal))
                    {
                        // Comment prior to a plan - do not trim so that input/output comparisons can be made but
                        // do remove the initial comment character
                        commentsBeforeData.Add(iline.Substring(1));
                        continue;
                    }

                    if (Message.isDebugOn)
                    {
                        Message.printDebug(50, routine, "line: " + iline);
                    }
                    // Break the line using whitespace, while allowing for quoted strings...
                    v = StringUtil.breakStringList(iline, " \t", StringUtil.DELIM_ALLOW_STRINGS | StringUtil.DELIM_SKIP_BLANKS);
                    size = 0;
                    if (v != null)
                    {
                        size = v.Count;
                    }
                    if (size < 11)
                    {
                        Message.printStatus(2, routine, "Ignoring line " + linecount + " not enough data values.  Have " + size + " expecting " + 11);
                        ++errorCount;
                        continue;
                    }
                    // Uncomment if testing...
                    //Message.printStatus ( 2, routine, "" + v );

                    // Allocate new plan node and set the values
                    aPlan = new StateMod_Plan();
                    aPlan.setID(v[0].Trim());
                    aPlan.setName(v[1].Trim());
                    aPlan.setCgoto(v[2].Trim());
                    aPlan.setSwitch(v[3].Trim());
                    aPlan.setIPlnTyp(v[4].Trim());
                    aPlan.setPeffFlag(v[5].Trim());
                    int peffFlag = aPlan.getPeffFlag();
                    aPlan.setIPrf(v[6].Trim());
                    aPlan.setIPfail(v[7].Trim());
                    aPlan.setPsto1(v[8].Trim());
                    aPlan.setPsource(v[9].Trim());
                    aPlan.setIPAcc(v[10].Trim());

                    // Read the efficiencies...

                    if (peffFlag == 1)
                    {
                        iline = @in.ReadLine();
                        ++linecount;
                        if (string.ReferenceEquals(iline, null))
                        {
                            throw new IOException("Unexpected end of file after line " + linecount + " - expecting 12 efficiency values.");
                        }
                        v = StringUtil.breakStringList(iline, " \t", StringUtil.DELIM_ALLOW_STRINGS | StringUtil.DELIM_SKIP_BLANKS);
                        size = 0;
                        if (v != null)
                        {
                            size = v.Count;
                        }
                        if (size != 12)
                        {
                            Message.printStatus(2, routine, "Ignoring line " + linecount + " not enough data values.  Have " + size + " expecting " + 12);
                            ++errorCount;
                        }
                        else
                        {
                            for (int iEff = 0; iEff < 12; iEff++)
                            {
                                string val = v[0].Trim();
                                try
                                {
                                    aPlan.setPeff(iEff, double.Parse(val));
                                }
                                catch (Exception)
                                {
                                    Message.printStatus(2, routine, "Efficiencies on line " + linecount + " value \"" + val + "\" is not a number.");
                                    ++errorCount;
                                }
                            }
                        }
                    }

                    // Set the comments

                    if (commentsBeforeData.Count > 0)
                    {
                        // Set comments that have been read previous to this line.  First, attempt to discard
                        // comments that do not below with the operational right.  For now, search backward for
                        // "EndHeader" and "--e" which indicate the end of the header.  If found, discard the comments prior
                        // to this because they are assumed to be file header comments, not comments for a specific right.
                        // Only do this for the first right because the user may actually want to include the header
                        // information in their file periodically to help with formatting
                        string comment;
                        if (thePlans.Count == 0)
                        {
                            for (int iComment = commentsBeforeData.Count - 1; iComment >= 0; --iComment)
                            {
                                comment = commentsBeforeData[iComment].ToUpper();
                                if ((comment.IndexOf("ENDHEADER", StringComparison.Ordinal) >= 0) || (comment.IndexOf("--E", StringComparison.Ordinal) >= 0))
                                {
                                    // Remove the comments above the position.
                                    while (iComment >= 0)
                                    {
                                        commentsBeforeData.RemoveAt(iComment--);
                                    }
                                    break;
                                }
                            }
                        }
                        aPlan.setCommentsBeforeData(commentsBeforeData);
                    }
                    // Always clear out for next right...
                    commentsBeforeData = new List<string>(1);

                    // Set the plan to not dirty because it was just initialized...

                    aPlan.setDirty(false);

                    // Add the plan to the vector of plans
                    thePlans.Add(aPlan);
                }
            }
            catch (Exception e)
            {
                Message.printWarning(3, routine, "Error reading line " + linecount + " \"" + iline + "\" uniquetempvar.");
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
            if (errorCount > 0)
            {
                throw new Exception("There were " + errorCount + " errors processing the data - refer to log file.");
            }
            return thePlans;
        }

        /// <summary>
        /// Set the comments before the data in the input file. </summary>
        /// <param name="commentsBeforeData"> comments before the data in the input file. </param>
        public virtual void setCommentsBeforeData(IList<string> commentsBeforeData)
        {
            bool dirty = false;
            int size = commentsBeforeData.Count;
            IList<string> commentsBeforeData0 = getCommentsBeforeData();
            if (size != commentsBeforeData0.Count)
            {
                dirty = true;
            }
            else
            {
                // Lists are the same size and there may not have been any changes
                // Need to check each string in the comments
                for (int i = 0; i < size; i++)
                {
                    if (!commentsBeforeData[i].Equals(commentsBeforeData0[i]))
                    {
                        dirty = true;
                        break;
                    }
                }
            }
            if (dirty)
            {
                // Something was different so set the comments and change the dirty flag
                __commentsBeforeData = commentsBeforeData;
                setDirty(true);
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_PLANS, true);
                }
            }
        }

        /// <summary>
        /// Set the plan failure flag. </summary>
        /// <param name="iPfail"> Plan failure flag. </param>
        public virtual void setIPfail(int iPfail)
        {
            if (iPfail != _iPfail)
            {
                _iPfail = iPfail;
                setDirty(true);
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_PLANS, true);
                }
            }
        }

        /// <summary>
        /// Set the plan failure flag. </summary>
        /// <param name="iPfail"> plan failure flag. </param>
        public virtual void setIPfail(int? iPfail)
        {
            setIPfail(iPfail.Value);
        }

        /// <summary>
        /// Set the plan failure flag. </summary>
        /// <param name="iPfail"> plan failure flag. </param>
        public virtual void setIPfail(string iPfail)
        {
            if (string.ReferenceEquals(iPfail, null))
            {
                return;
            }
            setIPfail(int.Parse(iPfail.Trim()));
        }

        /// <summary>
        /// Set the plan return flow table. </summary>
        /// <param name="iPrf"> Plan return flow table. </param>
        public virtual void setIPrf(int iPrf)
        {
            if (iPrf != _iPrf)
            {
                _iPrf = iPrf;
                setDirty(true);
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_PLANS, true);
                }
            }
        }

        /// <summary>
        /// Set the plan return flow table. </summary>
        /// <param name="iPrf"> plan return flow table. </param>
        public virtual void setIPrf(int? iPrf)
        {
            setIPrf(iPrf.Value);
        }

        /// <summary>
        /// Set the plan return flow table. </summary>
        /// <param name="iPrf"> plan return flow table. </param>
        public virtual void setIPrf(string iPrf)
        {
            if (string.ReferenceEquals(iPrf, null))
            {
                return;
            }
            setIPrf(int.Parse(iPrf.Trim()));
        }

        /// <summary>
        /// Set the plan type. </summary>
        /// <param name="iPlnTyp"> Plan type. </param>
        public virtual void setIPlnTyp(int iPlnTyp)
        {
            if (iPlnTyp != _iPlnTyp)
            {
                _iPlnTyp = iPlnTyp;
                setDirty(true);
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_PLANS, true);
                }
            }
        }

        /// <summary>
        /// Set the plan type. </summary>
        /// <param name="iPlnTyp"> plan type. </param>
        public virtual void setIPlnTyp(int? iPlnTyp)
        {
            setIPlnTyp(iPlnTyp.Value);
        }

        /// <summary>
        /// Set the plan type. </summary>
        /// <param name="iPlnTyp"> plan type. </param>
        public virtual void setIPlnTyp(string iPlnTyp)
        {
            if (string.ReferenceEquals(iPlnTyp, null))
            {
                return;
            }
            setIPlnTyp(int.Parse(iPlnTyp.Trim()));
        }

        /// <summary>
        /// Set the source account. </summary>
        /// <param name="iPAcc"> source account. </param>
        public virtual void setIPAcc(string iPAcc)
        {
            if (string.ReferenceEquals(iPAcc, null))
            {
                return;
            }
            if (!iPAcc.Equals(_iPAcc))
            {
                _iPAcc = iPAcc;
                setDirty(true);
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_PLANS, true);
                }
            }
        }

        /// <summary>
        /// Set the plan efficiency for a particular month.
        /// The efficiencies are stored in the order of the year for the data set.  For
        /// example, if water years are used, the first efficiency will be for October.  For
        /// calendar year, the first efficiency will be for January. </summary>
        /// <param name="index"> month index (0+) </param>
        /// <param name="peff"> monthly efficiency </param>
        public virtual void setPeff(int index, double peff)
        {
            if (_Peff[index] != peff)
            {
                _Peff[index] = peff;
                setDirty(true);
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_PLANS, true);
                }
            }
        }

        /// <summary>
        /// Set the plan efficiency flag. </summary>
        /// <param name="peff"> plan efficiency flag. </param>
        public virtual void setPeffFlag(int PeffFlag)
        {
            if (_PeffFlag != PeffFlag)
            {
                _PeffFlag = PeffFlag;
                setDirty(true);
                // TODO SAM 2006-08-22 Take out after initial troubleshooting is complete
                //Message.printStatus ( 2, "", "Setting object dirty = true" );
                //String s = "not null";
                //if ( _dataset == null ) {
                //	s = "null";
                //}
                //Message.printStatus ( 2, "", "_isClone=" + _isClone + " _dataset="+s );
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_PLANS, true);
                    //Message.printStatus ( 2, "", "Is data set dirt?"  + _dataset.isDirty() );
                }
            }
        }

        /// <summary>
        /// Set the plan efficiency flag. </summary>
        /// <param name="PeffFlag"> plan efficiency flag. </param>
        public virtual void setPeffFlag(int? PeffFlag)
        {
            setPeffFlag(PeffFlag.Value);
        }

        /// <summary>
        /// Set the plan efficiency. </summary>
        /// <param name="PeffFlag"> Plan efficiency. </param>
        public virtual void setPeffFlag(string PeffFlag)
        {
            if (string.ReferenceEquals(PeffFlag, null))
            {
                return;
            }
            setPeffFlag(int.Parse(PeffFlag.Trim()));
        }

        /// <summary>
        /// Set the source id. </summary>
        /// <param name="Psource"> source id. </param>
        public virtual void setPsource(string Psource)
        {
            if (string.ReferenceEquals(Psource, null))
            {
                return;
            }
            if (!Psource.Equals(_Psource))
            {
                _Psource = Psource;
                setDirty(true);
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_PLANS, true);
                }
            }
        }

        /// <summary>
        /// Set the plan initial storage. </summary>
        /// <param name="peff"> plan initial storage. </param>
        public virtual void setPsto1(double Psto1)
        {
            if (_Psto1 != Psto1)
            {
                _Psto1 = Psto1;
                setDirty(true);
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_PLANS, true);
                }
            }
        }

        /// <summary>
        /// Set the plan initial storage. </summary>
        /// <param name="Psto1"> plan initial storage. </param>
        public virtual void setPsto1(double? Psto1)
        {
            setPsto1(Psto1.Value);
        }

        /// <summary>
        /// Set the plan initial storage. </summary>
        /// <param name="Peff"> Plan initial storage. </param>
        public virtual void setPsto1(string Psto1)
        {
            if (string.ReferenceEquals(Psto1, null))
            {
                return;
            }
            setPsto1(double.Parse(Psto1.Trim()));
        }
    }
}
