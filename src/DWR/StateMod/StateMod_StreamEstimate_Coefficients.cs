using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

// StateMod_StreamEstimate_Coefficients - class to store stream estimate station coefficient information

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
// StateMod_StreamEstimate_Coefficients - class to store stream estimate
//				station coefficient information
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
// 
// 02 Sep 1997	Catherine E.		Created initial version of class.
//		Nutting-Lane, RTi
// 11 Feb 1998	CEN, RTi		Added _dataset.setDirty
//					to all set
//					routines.
// 11 Mar 1998	CEN, RTi		Changed class to extend from Object
//					(default)to StateMod_Data.  Removed utm
//					abilities(use StateMod_Data's).
// 04 Aug 1998	Steven A. Malers, RTi	Overload some of the routines for use in
//					DMIs.  Add SMLocateBaseNode as per
//					legacy code.
// 21 Dec 1998	CEN, RTi		Added throws IOException to read/write
//					routines.
// 18 Feb 2001	SAM, RTi		Code review.  Clean up javadoc.  Handle
//					nulls.  Set unused variables to null.
//					Add finalize.  Alphabetize methods.
//					Change IO to IOUtil.  Change so Vectors
//					are initialize sized to 5 rather than
//					15.  Update SMLocateBaseNode to use
//					SMUtil for search.
// 02 May 2001	SAM, RTi		Track down problem with warning reading
//					baseflow.  Second line of read was
//					trimming first, which caused the fixed
//					read to have problems.  Not sure how
//					the problem got introduced.
// 2001-12-27	SAM, RTi		Update to use new fixedRead()to
//					improve performance.
// 2002-09-12	SAM, RTi		Remove baseflow time series code from
//					here and move to the SMRiverInfo class,
//					to handle the time series associated
//					with .ris nodes.
// 2002-09-19	SAM, RTi		Use isDirty() instead of setDirty() to
//					indicate edits.
//------------------------------------------------------------------------------
// 2003-06-04	J. Thomas Sapienza, RTI	Renamed from SMRivBaseflows
// 2003-06-10	JTS, RTi		* Folded dumpRiverBaseFlowsFile() into
//					  writeRiverBaseFlowsFile()
//					* Renamed parseRiverBaseFlowsFile() to
//					  readRiverBaseFlowsFile()
// 2003-06-23	JTS, RTi		Renamed writeRiverBaseFlowsFile() to
//					writeStateModFile()
// 2003-06-26	JTS, RTi		Renamed readRiverBaseFlowsFile() to
//					readStateModFile()
// 2003-07-15	JTS, RTi		Changed to use new dataset design.
// 2003-08-03	SAM, RTi		* Rename class from StateMod_BaseFlows
//					  to StateMod_BaseFlowCoefficients.
//					* Changed isDirty() back to setDirty().
//					* locateIndexFromID() is now in
//					  StateMod_Util.
// 2003-09-11	SAM, RTi		Rename class from
//					StateMod_BaseFlowCoefficents to
//					StateMod_StreamEstimate_Coefficients.
// 2003-10-16	SAM, RTi		Remove description since it is not
//					used in the file and the base class
//					name could be used if needed.
// 2004-03-15	JTS, RTi		Put description back in as it is now
//					used by the makenet code in 
//					HydroBase_NodeNetwork.
// 2004-07-12	JTS, RTi		Removed description one more time.
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
// 2004-08-13	SAM, RTi		* When writing, adjust file names for
//					  working directory.
//					* Handle null _dataset, for use with
//					  StateDMI.
//					* For setM() and setN(), if the size is
//					  set to zero, clear out the data
//					  vectors - this is used with the
//					  StateDMI set command.
// 2005-04-18	JTS, RTi		Added writeListFile().
// 2007-04-12	Kurt Tometich, RTi		Added checkComponentData() and
//									getDataHeader() methods for check
//									file and data check support.
//------------------------------------------------------------------------------
// EndHeader

namespace DWR.StateMod
{

    using DataSetComponent = RTi.Util.IO.DataSetComponent;
    using IOUtil = RTi.Util.IO.IOUtil;
    using Message = RTi.Util.Message.Message;
    using StringUtil = RTi.Util.String.StringUtil;

    // FIXME SAM 2009-01-11 Need to remove N and M data members - they are redundant with the size of the
    // lists, which can lead to inconsistencies.

    public class StateMod_StreamEstimate_Coefficients : StateMod_Data //, ICloneable, IComparable<StateMod_Data>, StateMod_ComponentValidator
    {

        // TODO SAM 2009-01-05 Why is this needed?  Apparently for the table model for setting data?
        public const int MAX_BASEFLOWS = 15;

        /// <summary>
        /// Node where flow is to be estimated.
        /// </summary>
        protected internal string _flowX;
        /// <summary>
        /// Number of stations upstream X.
        /// </summary>
        protected internal int _N;
        /// <summary>
        /// Factors to weight the gaged flow.
        /// </summary>
        protected internal IList<double?> _coefn;
        /// <summary>
        /// Station ids upstream X.
        /// </summary>
        protected internal IList<string> _upper;
        /// <summary>
        /// Factor to distribute the gain.
        /// </summary>
        protected internal double _proratnf;
        /// <summary>
        /// Number of stations used to calculate the gain.
        /// </summary>
        protected internal int _M;
        /// <summary>
        /// Factors to weight the flow for gain.
        /// </summary>
        protected internal IList<double?> _coefm;
        /// <summary>
        /// Station ids upstream X
        /// </summary>
        protected internal IList<string> _flowm;

        /// <summary>
        /// Constructor.
        /// </summary>
        public StateMod_StreamEstimate_Coefficients() : base()
        {
            initialize();
        }

        public virtual void addCoefm(double d)
        {
            addCoefm(new double?(d));
        }

        public virtual void addCoefm(double? d)
        {
            _coefm.Add(d);
            int size = _coefm.Count;
            if (size > _M)
            {
                _M = size;
            }
            if (!_isClone)
            {
                if (_dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_STREAMESTIMATE_COEFFICIENTS, true);
                }
            }
        }

        public virtual void addCoefn(double d)
        {
            addCoefn(new double?(d));
        }

        public virtual void addCoefn(double? d)
        {
            _coefn.Add(d);
            int size = _coefn.Count;
            if (size > _N)
            {
                _N = size;
            }
            if (!_isClone)
            {
                if (_dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_STREAMESTIMATE_COEFFICIENTS, true);
                }
            }
        }

        public virtual void addUpper(string s)
        {
            if (!string.ReferenceEquals(s, null))
            {
                _upper.Add(s.Trim());
                int size = _upper.Count;
                if (size > _N)
                {
                    _N = size;
                }
                if (!_isClone)
                {
                    if (_dataset != null)
                    {
                        _dataset.setDirty(StateMod_DataSet.COMP_STREAMESTIMATE_COEFFICIENTS, true);
                    }
                }
            }
        }

        /// <summary>
        /// Retrieve the coefm corresponding to a particular index.
        /// </summary>
        public virtual double getCoefm(int index)
        {
            return ((double?)_coefm[index]).Value;
        }

        /// <summary>
        /// Retrieve the coefm corresponding to a particular index.
        /// </summary>
        public virtual IList<double?> getCoefm()
        {
            return _coefm;
        }

        /// <summary>
        /// Return the coefn corresponding to a particular index.
        /// </summary>
        public virtual double getCoefn(int index)
        {
            return _coefn[index].Value;
        }

        /// <summary>
        /// Return the coefn corresponding to a particular index.
        /// </summary>
        public virtual IList<double?> getCoefn()
        {
            return _coefn;
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
        /// Return the upper id corresponding to a particular index.
        /// </summary>
        public virtual string getFlowm(int index)
        {
            return (string)_flowm[index];
        }

        /// <summary>
        /// Return the upper id corresponding to a particular index.
        /// </summary>
        public virtual IList<string> getFlowm()
        {
            return _flowm;
        }

        /// <summary>
        /// Return the node where flow is to be estimated.
        /// </summary>
        public virtual string getFlowX()
        {
            return _flowX;
        }

        /// <summary>
        /// Return the number of stations used to calc the gain.
        /// </summary>
        public virtual int getM()
        {
            return _M;
        }

        /// <summary>
        /// Retrieve the number of stations upstream X.
        /// </summary>
        public virtual int getN()
        {
            return _N;
        }

        /// <summary>
        /// Return the factor to distribute the gain.
        /// </summary>
        public virtual double getProratnf()
        {
            return _proratnf;
        }

        /// <summary>
        /// Return the upper id corresponding to a particular index.
        /// </summary>
        public virtual string getUpper(int index)
        {
            return (string)_upper[index];
        }

        /// <summary>
        /// Return the upper id corresponding to a particular index.
        /// </summary>
        public virtual IList<string> getUpper()
        {
            return _upper;
        }

        /// <summary>
        /// Initialize member variables.
        /// </summary>
        private void initialize()
        {
            _smdata_type = StateMod_DataSet.COMP_STREAMESTIMATE_COEFFICIENTS;
            _flowX = "";
            _N = 0;
            _M = 0;
            _coefn = new List<double?>();
            _upper = new List<string>();
            _coefm = new List<double?>();
            _flowm = new List<string>();
            _proratnf = 0;
        }

        /// <summary>
        /// Read stream estimate coefficients and store in a list. </summary>
        /// <param name="filename"> Name of file to read. </param>
        /// <returns> list of streamflow estimate coefficients data </returns>
        /// <exception cref="Exception"> if there is an error reading the file. </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public static java.util.List<StateMod_StreamEstimate_Coefficients> readStateModFile(String filename) throws Exception
        public static IList<StateMod_StreamEstimate_Coefficients> readStateModFile(string filename)
        {
            string routine = "StateMod_StreamEstimate_Coefficients.readStateModFile";
            IList<StateMod_StreamEstimate_Coefficients> theBaseflows = new List<StateMod_StreamEstimate_Coefficients>();

            string iline = null;
            IList<object> v = new List<object>(2); // used to retrieve from fixedRead
            string adnl = null;
            int[] format_0 = new int[] { StringUtil.TYPE_STRING, StringUtil.TYPE_SPACE, StringUtil.TYPE_INTEGER };
            int[] format_0w = new int[] { 12, 8, 8 };
            int[] format_1 = new int[] { StringUtil.TYPE_DOUBLE, StringUtil.TYPE_SPACE, StringUtil.TYPE_STRING };
            int[] format_1w = new int[] { 8, 1, 12 };
            int[] format_2 = new int[] { StringUtil.TYPE_SPACE, StringUtil.TYPE_DOUBLE, StringUtil.TYPE_INTEGER };
            int[] format_2w = new int[] { 12, 8, 8 };
            int[] format_3 = new int[] { StringUtil.TYPE_DOUBLE, StringUtil.TYPE_SPACE, StringUtil.TYPE_STRING };
            int[] format_3w = new int[] { 8, 1, 12 };
            StreamReader @in = null;
            StateMod_StreamEstimate_Coefficients aBaseflow = null;
            int i = 0;
            int num_adnl;
            int begin_pos;
            int end_pos;
            int linecount = 0;

            if (Message.isDebugOn)
            {
                Message.printDebug(30, routine, "Reading file :" + filename);
            }

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
                    // allocate a new baseflow
                    aBaseflow = new StateMod_StreamEstimate_Coefficients();

                    // read in first of two lines for each baseflow
                    StringUtil.fixedRead(iline, format_0, format_0w, v);
                    aBaseflow.setFlowX(((string)v[0]).Trim());
                    aBaseflow.setN((int?)v[1]);

                    num_adnl = aBaseflow.getN();
                    for (i = 0; i < num_adnl; i++)
                    {
                        // calculate begin_pos and end_pos
                        //	 8(factor to weight the gaged flow)
                        // +	 1(space)
                        // +	12(station id upstream X)
                        // --------
                        //	21(for each set of num_adnl
                        // +	28(array index after initial info)
                        // +	 1(we should start on next position)
                        // -	 1(our string starts with index 0)
                        // 	
                        begin_pos = 28 + (i * 21);
                        end_pos = begin_pos + 21;
                        adnl = iline.Substring(begin_pos, end_pos - begin_pos);
                        StringUtil.fixedRead(adnl, format_1, format_1w, v);
                        aBaseflow.addCoefn((double?)v[0]);
                        aBaseflow.addUpper((string)v[1]);
                    }

                    // read in second of two lines for each baseflow
                    iline = @in.ReadLine();
                    StringUtil.fixedRead(iline, format_2, format_2w, v);
                    aBaseflow.setProratnf((double?)v[0]);
                    aBaseflow.setM((int?)v[1]);

                    num_adnl = aBaseflow.getM();
                    for (i = 0; i < num_adnl; i++)
                    {
                        begin_pos = 28 + (i * 21);
                        end_pos = begin_pos + 21;
                        adnl = iline.Substring(begin_pos, end_pos - begin_pos);
                        StringUtil.fixedRead(adnl, format_3, format_3w, v);
                        aBaseflow.addCoefm((double?)v[0]);
                        aBaseflow.addFlowm((string)v[1]);
                    }

                    // add the baseflow to the vector of baseflows
                    theBaseflows.Add(aBaseflow);
                }
            }
            catch (Exception e)
            {
                Message.printWarning(3, routine, "Error reading near line " + linecount);
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
            return theBaseflows;
        }

        /// <summary>
        /// Add factor to vector of factors to weight the flow for gain.
        /// </summary>
        public virtual void setCoefm(int index, string str)
        {
            if (!string.ReferenceEquals(str, null))
            {
                setCoefm(index, double.Parse(str.Trim()));
            }
        }

        /// <summary>
        /// Add factor to vector of factors to weight the flow for gain.
        /// </summary>
        public virtual void setCoefm(int index, double str)
        {
            if (_coefm.Count < index + 1)
            {
                addCoefm(new double?(str));
            }
            else
            {
                _coefm[index] = new double?(str);
                if (!_isClone)
                {
                    if (_dataset != null)
                    {
                        _dataset.setDirty(StateMod_DataSet.COMP_STREAMESTIMATE_COEFFICIENTS, true);
                    }
                }
            }
        }

        /// <summary>
        /// Add factor to vector of factors to weight the gaged flow to vector (coefn).
        /// </summary>
        public virtual void setCoefn(int index, string str)
        {
            if (!string.ReferenceEquals(str, null))
            {
                setCoefn(index, double.Parse(str.Trim()));
            }
        }

        /// <summary>
        /// Main version.
        /// </summary>
        public virtual void setCoefn(int index, double str)
        {
            if (_coefn.Count < index + 1)
            {
                addCoefn(new double?(str));
            }
            else
            {
                _coefn[index] = new double?(str);
                if (!_isClone)
                {
                    if (_dataset != null)
                    {
                        _dataset.setDirty(StateMod_DataSet.COMP_STREAMESTIMATE_COEFFICIENTS, true);
                    }
                }
            }
        }

        /// <summary>
        /// Set the node where flow is to be estimated.
        /// </summary>
        public virtual void setFlowX(string s)
        {
            if (!string.ReferenceEquals(s, null))
            {
                if (!s.Equals(_flowX))
                {
                    if (!_isClone)
                    {
                        if (_dataset != null)
                        {
                            _dataset.setDirty(StateMod_DataSet.COMP_STREAMESTIMATE_COEFFICIENTS, true);
                        }
                    }
                    _flowX = s;
                    setID(s);
                }
            }
        }

        /// <summary>
        /// Add id to vector of station ids upstream X.
        /// </summary>
        public virtual void setFlowm(int index, string str)
        {
            if (!string.ReferenceEquals(str, null))
            {
                if (_flowm.Count < index + 1)
                {
                    addFlowm(str.Trim());
                }
                else
                {
                    _flowm[index] = str.Trim();
                    if (!_isClone)
                    {
                        if (_dataset != null)
                        {
                            _dataset.setDirty(StateMod_DataSet.COMP_STREAMESTIMATE_COEFFICIENTS, true);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Add id to vector of station ids upstream X.
        /// </summary>
        public virtual void addFlowm(string s)
        {
            if (!string.ReferenceEquals(s, null))
            {
                _flowm.Add(s.Trim());
                int size = _flowm.Count;
                if (size > _M)
                {
                    _M = size;
                }
                if (!_isClone)
                {
                    if (_dataset != null)
                    {
                        _dataset.setDirty(StateMod_DataSet.COMP_STREAMESTIMATE_COEFFICIENTS, true);
                    }
                }
            }
        }

        /// <summary>
        /// Set the number of stations used to calc the gain.
        /// </summary>
        public virtual void setM(int i)
        {
            if (i != _M)
            {
                if (!_isClone)
                {
                    if (_dataset != null)
                    {
                        _dataset.setDirty(StateMod_DataSet.COMP_STREAMESTIMATE_COEFFICIENTS, true);
                    }
                }
                _M = i;
                if (i == 0)
                {
                    // Clear vector...
                    _coefm.Clear();
                    _flowm.Clear();
                }
            }
        }

        /// <summary>
        /// Set the number of stations used to calculate the gain.
        /// </summary>
        public virtual void setM(int? i)
        {
            setM(i.Value);
        }

        /// <summary>
        /// Set the number of stations used to calculate the gain.
        /// </summary>
        public virtual void setM(string str)
        {
            if (!string.ReferenceEquals(str, null))
            {
                setM(int.Parse(str.Trim()));
            }
        }

        /// <summary>
        /// Set the number of stations upstream X.
        /// </summary>
        public virtual void setN(int i)
        {
            if (i != _N)
            {
                if (!_isClone)
                {
                    if (_dataset != null)
                    {
                        _dataset.setDirty(StateMod_DataSet.COMP_STREAMESTIMATE_COEFFICIENTS, true);
                    }
                }
                _N = i;
                if (i == 0)
                {
                    // Clear vector...
                    _coefn.Clear();
                    _upper.Clear();
                }
            }
        }

        /// <summary>
        /// Set the number of stations upstream X.
        /// </summary>
        public virtual void setN(int? i)
        {
            setN(i.Value);
        }

        /// <summary>
        /// Set the number of stations upstream X.
        /// </summary>
        public virtual void setN(string str)
        {
            if (!string.ReferenceEquals(str, null))
            {
                setN(int.Parse(str.Trim()));
            }
        }

        /// <summary>
        /// Set the initial storage of owner.
        /// </summary>
        public virtual void setProratnf(double d)
        {
            if (d != _proratnf)
            {
                if (!_isClone)
                {
                    if (_dataset != null)
                    {
                        _dataset.setDirty(StateMod_DataSet.COMP_STREAMESTIMATE_COEFFICIENTS, true);
                    }
                }
                _proratnf = d;
            }
        }

        /// <summary>
        /// Set the initial storage of owner.
        /// </summary>
        public virtual void setProratnf(double? d)
        {
            setProratnf(d.Value);
        }

        /// <summary>
        /// Set the initial storage of owner.
        /// </summary>
        public virtual void setProratnf(string str)
        {
            if (!string.ReferenceEquals(str, null))
            {
                setProratnf(int.Parse(str.Trim()));
            }
        }

        /// <summary>
        /// Add id to vector of station ids upstream X.
        /// </summary>
        public virtual void setUpper(int index, string str)
        {
            if (!string.ReferenceEquals(str, null))
            {
                if (_upper.Count < index + 1)
                {
                    addUpper(str.Trim());
                }
                else
                {
                    _upper[index] = str.Trim();
                    if (!_isClone)
                    {
                        if (_dataset != null)
                        {
                            _dataset.setDirty(StateMod_DataSet.COMP_STREAMESTIMATE_COEFFICIENTS, true);
                        }
                    }
                }
            }
        }
    }
}
