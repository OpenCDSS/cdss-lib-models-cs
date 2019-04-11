using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

// StateMod_OperationalRight - class for operational rights

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
// StateMod_OperationalRight - class derived from StateMod_Data.  Contains 
//	information read from the operational rights file.
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
// 
// 07 Jan 1998	Catherine E.		Created initial version of class
//		Nutting-Lane, RTi
// 23 Feb 1998	CEN, RTi		Added Write routines
// 21 Dec 1998	CEN, RTi		Added throws IOException to read/write
//					routines.
// 23 Nov 1999	CEN, RTi		Added comments for each 
//					StateMod_OperationalRight
//					instantiation.
// 07 Mar 2000	CEN, RTi		Modified read/write methods logic to
//					work off dumx variable to determine
//					additional lines for a rule rather than
//					using rule type.  Also, added rule types
//					15 and 16.
// 19 Feb 2001	Steven A. Malers, RTi	Code review.  Clean up javadoc.  Handle
//					nulls and set unused variables to null.
//					Add finalize.  Alphabetize methods.
//					Change IO to IOUtil.  Change some status
//					messages to debug and remove some debug
//					messages.
// 2001-12-27	SAM, RTi		Update to use new fixedRead() to
//					improve performance (are not using full
//					optimization here).
// 2002-09-19	SAM, RTi		Use isDirty() instead of setDirty() to
//					indicate edits.
//------------------------------------------------------------------------------
// 2003-06-04	J. Thomas Sapienza, RTi	Renamed from SMOprits to 
//					StateMod_OperationalRight
// 2003-06-10	JTS, RTi		* Folded dumpOperationalRightsFile()
//					  into writeOperationalRightsFile()
//					* Renamed parseOperationalRightsFile()
//					  into readOperationalRightsFile()
// 2003-06-23	JTS, RTi		Renamed writeOperationalRightsFile()
//					to writeStateModFile()
// 2003-06-26	JTS, RTi		Renamed readOperationalRightsFile()
//					to readStateModFile()
// 2003-07-15	JTS, RTi		Changed to use new dataset design.
// 2003-08-03	SAM, RTi		Changed isDirty() back to setDirty().
// 2003-08-25	SAM, RTi		Changed public oprightsOptions to
//					TYPES, consistent with other programming
//					standards.
// 2003-08-28	SAM, RTi		* Call setDirty() for each object and
//					  the data component.
//					* Clean up parameter names and javadoc.
// 2003-09-15	SAM, RTi		* Update to handle all new operations,
//					  up through number 23.
//					* Change some data types from numbers to
//					  String because of changes in how they
//					  are used in the FORTRAM (must be doing
//					  internal type casting in FORTRAN).
//					* Change StringTokenizer to
//					  breakStringList() - easier to check
//					  count of tokens.
// 2003-09-22	J. Thomas Sapienza, RTi	* Added hasImonsw().
//					* Added setupImonsw().
//					* Added getQdebt().
//					* Added getQdebtx().
//					* Added getSjmina().
//					* Added getSjrela().
// 2003-10-19	SAM, RTi		Change description of types 2 and 3 as
//					per Ray Bennett 2003-10-18 email.
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
// 2004-08-25	JTS, RTi		Revised the clone() code because of
//					null pointers being thrown if the data
//					arrays were null.
// 2004-08-26	JTS, RTi		The array values (_intern and _imonsw)
//					were not being handled in 
//					restoreOriginal() or compareTo(), so
//					they were added.
// 2006-08-16	SAM, RTi		* Add names of operational rights 24 to
//					  35.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
//------------------------------------------------------------------------------
// EndHeader

namespace DWR.StateMod
{

    using IOUtil = RTi.Util.IO.IOUtil;
    using Message = RTi.Util.Message.Message;
    using StringUtil = RTi.Util.String.StringUtil;

    /// <summary>
    /// StateMod operational right (operating rule) data.
    /// Not all operational right types may be handled by the software.  See the the
    /// StateMod_OperationalRight_Metadata.getFullEditingSupported() for information.
    /// </summary>
    public class StateMod_OperationalRight : StateMod_Data //, ICloneable, IComparable<StateMod_Data>
    {

        /// <summary>
        /// Administration number.
        /// </summary>
        private string _rtem;
        /// <summary>
        /// Typically the number of intervening structures or the number of monthly
        /// switches, depending on the right number
        /// </summary>
        private int _dumx;
        /// <summary>
        /// Typically the destination ID.
        /// </summary>
        private string _ciopde;
        /// <summary>
        /// Typically the destination account.
        /// </summary>
        private string _iopdes;
        /// <summary>
        /// Typically the supply ID.
        /// </summary>
        private string _ciopso1;
        /// <summary>
        /// Typically the supply account.
        /// </summary>
        private string _iopsou1;
        /// <summary>
        /// Definition varies by right type.
        /// </summary>
        private string _ciopso2;
        /// <summary>
        /// Definition varies by right type.
        /// </summary>
        private string _iopsou2;
        /// <summary>
        /// Definition varies by right type.
        /// </summary>
        private string _ciopso3;
        /// <summary>
        /// Definition varies by right type.
        /// </summary>
        private string _iopsou3;
        /// <summary>
        /// Used with type 17, 18.
        /// </summary>
        private string _ciopso4;
        /// <summary>
        /// Used with type 17, 18.
        /// </summary>
        private string _iopsou4;
        /// <summary>
        /// Used with type 17, 18.
        /// </summary>
        private string _ciopso5;
        /// <summary>
        /// Used with type 17, 18.
        /// </summary>
        private string _iopsou5;
        /// <summary>
        /// Operational right type > 1.
        /// </summary>
        private int __ityopr;
        /// <summary>
        /// Intervening structure IDs (up to 10 in StateMod doc but no limit here) - used by some rights, null if not used.
        /// </summary>
        private string[] _intern = null;
        /// <summary>
        /// Intervening structure carrier loss, %.
        /// </summary>
        private double[] __oprLossC = null;
        /// <summary>
        /// Intervening structure types, used when have loss.
        /// </summary>
        private string[] __internT = null;
        /// <summary>
        /// Monthly switch, for some rights, null if not used (months in order of data set control file).
        /// </summary>
        private int[] _imonsw = null;
        /// <summary>
        /// Comments provided by user - # comments before each right.  An empty (non-null) list is guaranteed.
        /// TODO SAM 2010-12-14 Evaluate whether this can be in StateMod_Data or will it bloat memory.
        /// </summary>
        private IList<string> __commentsBeforeData = new List<string>();
        /// <summary>
        /// Used with operational right 17, 18.
        /// </summary>
        private double _qdebt;
        /// <summary>
        /// used with operational right 17, 18.
        /// </summary>
        private double _qdebtx;
        /// <summary>
        /// Used with operational right 20.
        /// </summary>
        private double _sjmina;
        /// <summary>
        /// Used with operational right 20.
        /// </summary>
        private double _sjrela;
        /// <summary>
        /// Plan ID.
        /// </summary>
        private string __creuse;
        /// <summary>
        /// Diversion type.
        /// </summary>
        private string __cdivtyp;
        /// <summary>
        /// Conveyance loss.
        /// </summary>
        private double __oprLoss;
        /// <summary>
        /// Miscellaneous limits.
        /// </summary>
        private double __oprLimit;
        /// <summary>
        /// Beginning year of operation.
        /// </summary>
        private int __ioBeg;
        /// <summary>
        /// Ending year of operation.
        /// </summary>
        private int __ioEnd;

        /// <summary>
        /// Monthly efficiencies (12 values in order of data set control file),
        /// only used by some rights like 24.
        /// </summary>
        private double[] __oprEff;

        /// <summary>
        /// Monthly operational limits (12 values in order of data set control file plus annual at end),
        /// only used by some rights like 47.
        /// </summary>
        private double[] __oprMax;

        /// <summary>
        /// TODO SAM 2011-01-29 Phase out when operational rights as documented have been fully tested in code.
        /// A list of strings indicating errors at read.  This is checked to determine if the right should be edited
        /// as text (yes if any errors) or detailed (no if any errors).
        /// </summary>
        private IList<string> __readErrorList = new List<string>();

        /// <summary>
        /// The operational right as a list of strings (lines after right comments and prior to the comments for
        /// the next right.
        /// </summary>
        private IList<string> __rightStringsList = new List<string>();

        /// <summary>
        /// Used with monthly and annual limitation.
        /// </summary>
        private string __cx = "";

        // cidvri = ID is in base class identifier
        // nameo = Name is in base class name
        // ioprsw = on/off is in base class switch

        /// <summary>
        /// The metadata that corresponds to the operational right type, or null if the right type is not recognized.
        /// The metadata is set when the operational right type is set.
        /// </summary>
        private StateMod_OperationalRight_Metadata __metadata = null;

        /// <summary>
        /// Constructor.
        /// </summary>
        public StateMod_OperationalRight() : base()
        {
            initialize();
        }

        // TODO SAM 2010-12-11 Should the default if not specified be version 2?
        /// <summary>
        /// Determine the StateMod operational right file version.  Version 1 is old and Version 2 was introduced
        /// in Version 12.0.  The version is determined by checking for the string "FileFormatVersion 2" in a comment
        /// line. </summary>
        /// <returns> 1 for the old version and 2 for version 2. </returns>
        private static int determineFileVersion(string filename)
        {
            StreamReader @in = null;
            int version = 1;
            try
            {
                @in = new StreamReader(filename);
                try
                {
                    // Read lines and check for string that indicates a version 2 file.
                    string @string = null;
                    while (!string.ReferenceEquals((@string = @in.ReadLine()), null))
                    {
                        @string = @string.ToUpper();
                        if (@string.StartsWith("#", StringComparison.Ordinal) && (@string.IndexOf("FILEFORMATVERSION 2", StringComparison.Ordinal) > 0))
                        {
                            version = 2;
                            break;
                        }
                    }
                }
                finally
                {
                    if (@in != null)
                    {
                        @in.Close();
                    }
                }
                return version;
            }
            catch (Exception e)
            {
                Message.printWarning(3, "", e);
                return version;
            }
        }

        /// <summary>
        /// Return the cdivtyp (diversion type).
        /// </summary>
        public virtual string getCdivtyp()
        {
            return __cdivtyp;
        }

        /// <summary>
        /// Return the ciopde.
        /// </summary>
        public virtual string getCiopde()
        {
            return _ciopde;
        }

        /// <summary>
        /// Return the ciopso1.
        /// </summary>
        public virtual string getCiopso1()
        {
            return _ciopso1;
        }

        /// <summary>
        /// Return the ciopso2.
        /// </summary>
        public virtual string getCiopso2()
        {
            return _ciopso2;
        }

        /// <summary>
        /// Return the ciopso3.
        /// </summary>
        public virtual string getCiopso3()
        {
            return _ciopso3;
        }

        /// <summary>
        /// Return the ciopso4.
        /// </summary>
        public virtual string getCiopso4()
        {
            return _ciopso4;
        }

        /// <summary>
        /// Return the ciopso5.
        /// </summary>
        public virtual string getCiopso5()
        {
            return _ciopso5;
        }

        /// <summary>
        /// Return the creuse (plan identifier).
        /// </summary>
        public virtual string getCreuse()
        {
            return __creuse;
        }

        /// <summary>
        /// Return the comments from the input file that immediate precede the data. </summary>
        /// <returns> the comments from the input file that immediate precede the data. </returns>
        public virtual IList<string> getCommentsBeforeData()
        {
            return __commentsBeforeData;
        }

        /// <summary>
        /// Return the cx (used with monthly and annual limitation).
        /// </summary>
        public virtual string getCx()
        {
            return __cx;
        }

        /// <summary>
        /// Retrieve dumx.
        /// </summary>
        public virtual int getDumx()
        {
            return _dumx;
        }

        /// <summary>
        /// Return the array of monthly switch.
        /// </summary>
        public virtual int[] getImonsw()
        {
            return _imonsw;
        }

        /// <summary>
        /// Return a monthly switch at an index. </summary>
        /// <param name="index"> month to get switch for (0-11), where the index is a position, not
        /// a month (actual month is controlled by the year type for the data set). </param>
        public virtual int getImonsw(int index)
        {
            return _imonsw[index];
        }

        /// <summary>
        /// Return the array of "intern".
        /// </summary>
        public virtual string[] getIntern()
        {
            return _intern;
        }

        /// <summary>
        /// Return the "intern" at an index, or blank if not set.
        /// </summary>
        public virtual string getIntern(int index)
        {
            if ((index < 0) || (index >= _intern.Length))
            {
                return "";
            }
            else
            {
                return _intern[index];
            }
        }

        /// <summary>
        /// Return the array of "internT".
        /// </summary>
        public virtual string[] getInternT()
        {
            return __internT;
        }

        /// <summary>
        /// Return the "internT" at an index, or blank if not set.
        /// </summary>
        public virtual string getInternT(int index)
        {
            if ((index < 0) || (index >= __internT.Length))
            {
                return "";
            }
            else
            {
                return __internT[index];
            }
        }

        /// <summary>
        /// Return the intervening structure identifiers, guaranteed to be non-null but may be empty.
        /// </summary>
        //public virtual IList<string> getInterveningStructureIDs()
        //{
        //    IList<string> structureIDList = new List<string>();
        //    if (__metadata == null)
        //    {
        //        return structureIDList;
        //    }
        //    else if (__metadata.getRightTypeUsesInterveningStructuresWithoutLoss())
        //    {
        //        string[] intern = getIntern();
        //        if ((intern == null) || (intern.Length == 0))
        //        {
        //            return structureIDList;
        //        }
        //        else
        //        {
        //            return Arrays.asList(intern);
        //        }
        //    }
        //    else
        //    {
        //        return structureIDList;
        //    }
        //}

        /// <summary>
        /// Retrieve the ioBeg.
        /// </summary>
        public virtual int getIoBeg()
        {
            return __ioBeg;
        }

        /// <summary>
        /// Retrieve the ioEnd.
        /// </summary>
        public virtual int getIoEnd()
        {
            return __ioEnd;
        }

        /// <summary>
        /// Get the interns as a list. </summary>
        /// <returns> the intervening structure identifiers or an empty list. </returns>
        public virtual IList<string> getInternsVector()
        {
            IList<string> v = new List<string>();
            if (_intern != null)
            {
                for (int i = 0; i < _intern.Length; i++)
                {
                    v.Add(getIntern(i));
                }
            }
            return v;
        }

        /// <summary>
        /// Return the iopdes.
        /// </summary>
        public virtual string getIopdes()
        {
            return _iopdes;
        }

        /// <summary>
        /// Return the iopsou.
        /// </summary>
        public virtual string getIopsou1()
        {
            return _iopsou1;
        }

        /// <summary>
        /// Return the iopsou2.
        /// </summary>
        public virtual string getIopsou2()
        {
            return _iopsou2;
        }

        /// <summary>
        /// Return the iopsou3.
        /// </summary>
        public virtual string getIopsou3()
        {
            return _iopsou3;
        }

        /// <summary>
        /// Return the iopsou4.
        /// </summary>
        public virtual string getIopsou4()
        {
            return _iopsou4;
        }

        /// <summary>
        /// Return the iopsou5.
        /// </summary>
        public virtual string getIopsou5()
        {
            return _iopsou5;
        }

        /// <summary>
        /// Retrieve the ityopr.
        /// </summary>
        public virtual int getItyopr()
        {
            return __ityopr;
        }

        /// <summary>
        /// Get the metadata for the right or null if the right type is not recognized.
        /// </summary>
        public virtual StateMod_OperationalRight_Metadata getMetadata()
        {
            return __metadata;
        }

        /// <summary>
        /// Return OprLimit. </summary>
        /// <returns> OprLimit. </returns>
        public virtual double getOprLimit()
        {
            return __oprLimit;
        }

        /// <summary>
        /// Return OprLoss. </summary>
        /// <returns> OprLoss. </returns>
        public virtual double getOprLoss()
        {
            return __oprLoss;
        }

        /// <summary>
        /// Return the array of "oprLossC".
        /// </summary>
        public virtual double[] getOprLossC()
        {
            return __oprLossC;
        }

        /// <summary>
        /// Return the "oprLossC" at an index, or missing if not set.
        /// </summary>
        public virtual double getOprLossC(int index)
        {
            if ((index < 0) || (index >= __oprLossC.Length))
            {
                return StateMod_Util.MISSING_DOUBLE;
            }
            else
            {
                return __oprLossC[index];
            }
        }

        /// <summary>
        /// Return the array of monthly efficiency values.
        /// </summary>
        public virtual double[] getOprEff()
        {
            return __oprEff;
        }

        /// <summary>
        /// Return a monthly efficiency at an index. </summary>
        /// <param name="index"> month to get efficiency for (0-11), where the index is a position, not
        /// a month (actual month is controlled by the year type for the data set). </param>
        public virtual double getOprEff(int index)
        {
            return __oprEff[index];
        }

        /// <summary>
        /// Return the array of monthly max limits.
        /// </summary>
        public virtual double[] getOprMax()
        {
            return __oprMax;
        }

        /// <summary>
        /// Return a monthly switch at an index. </summary>
        /// <param name="index"> month to get switch for (0-11), where the index is a position, not
        /// a month (actual month is controlled by the year type for the data set). </param>
        public virtual double getOprMax(int index)
        {
            return __oprMax[index];
        }

        /// <summary>
        /// Return the list of strings that contain read error messages, when the first line of the right is
        /// inconsistent with the documentation.  This may indicate that the documentation is wrong or the code is
        /// wrong, but may be that the data file is wrong and needs to be cleaner.  For example, a hand-edited
        /// operational right may be inaccurate and StateMod allows.  This right can be treated as text until the
        /// error in documentation/code/data are corrected. </summary>
        /// <returns> the list of strings that contain read error messages, when the first line of the right is
        /// inconsistent with the documentation. </returns>
        public virtual IList<string> getReadErrors()
        {
            return __readErrorList;
        }

        /// <returns> the list of strings that contain the operating rule data when the right is not understood. </returns>
        public virtual IList<string> getRightStrings()
        {
            return __rightStringsList;
        }

        /// <summary>
        /// Return rtem. </summary>
        /// <returns> rtem. </returns>
        public virtual string getRtem()
        {
            return _rtem;
        }

        public virtual double getQdebt()
        {
            return _qdebt;
        }

        public virtual double getQdebtx()
        {
            return _qdebtx;
        }

        public virtual double getSjrela()
        {
            return _sjrela;
        }

        public virtual double getSjmina()
        {
            return _sjmina;
        }

        /// <summary>
        /// Initializes member variables.
        /// </summary>
        private void initialize()
        {
            _smdata_type = StateMod_DataSet.COMP_OPERATION_RIGHTS;
            _rtem = "";
            _dumx = StateMod_Util.MISSING_INT;
            _ciopde = "";
            _iopdes = "";
            _ciopso1 = "";
            _iopsou1 = "";
            _ciopso2 = "";
            _iopsou2 = "";
            _ciopso3 = "";
            _iopsou3 = "";
            _ciopso4 = "";
            _iopsou4 = "";
            _ciopso5 = "";
            _iopsou5 = "";
            __ityopr = StateMod_Util.MISSING_INT;
            _imonsw = new int[12];
            for (int i = 0; i < 12; i++)
            {
                _imonsw[i] = StateMod_Util.MISSING_INT;
            }
            _intern = new string[10]; // Maximum defined by StateMod
            for (int i = 0; i < 10; i++)
            {
                _intern[i] = "";
            }
            __commentsBeforeData = new List<string>();
            _qdebt = StateMod_Util.MISSING_DOUBLE;
            _qdebtx = StateMod_Util.MISSING_DOUBLE;
            _sjmina = StateMod_Util.MISSING_DOUBLE;
            _sjrela = StateMod_Util.MISSING_DOUBLE;
            // Newer data - if not specified, the following should display OK and not trigger a dirty
            __creuse = "";
            __cdivtyp = "";
            __oprLoss = StateMod_Util.MISSING_DOUBLE;
            __oprLimit = StateMod_Util.MISSING_DOUBLE;
            __ioBeg = StateMod_Util.MISSING_INT;
            __ioEnd = StateMod_Util.MISSING_INT;
            // Only used by some rights, but setup data to avoid memory management checks
            __oprMax = new double[13];
            for (int i = 0; i < 13; i++)
            {
                __oprMax[i] = StateMod_Util.MISSING_DOUBLE;
            }
            // Only used by some rights, but setup data to avoid memory management checks
            __oprEff = new double[12];
            for (int i = 0; i < 12; i++)
            {
                __oprEff[i] = StateMod_Util.MISSING_DOUBLE;
            }
            __internT = new string[10]; // Maximum defined by StateMod
            for (int i = 0; i < 10; i++)
            {
                __internT[i] = "";
            }
            __oprLossC = new double[10]; // Maximum defined by StateMod
            for (int i = 0; i < 10; i++)
            {
                __oprLossC[i] = StateMod_Util.MISSING_DOUBLE;
            }
        }

        /// <summary>
        /// Indicate whether an operational right is known to the software.  If true, then the internal code should
        /// handle.  If false, the right should be treated as strings on read. </summary>
        /// <param name="rightTypeNumber"> the right type number </param>
        /// <param name="dataSet"> StateMod_DataSet, needed to check some relationships during the read (e.g., type 24). </param>
        public static bool isRightUnderstoodByCode(int rightTypeNumber, StateMod_DataSet dataSet)
        {
            StateMod_OperationalRight_Metadata metadata = StateMod_OperationalRight_Metadata.getMetadata(rightTypeNumber);
            if ((metadata == null) || !metadata.getFullEditingSupported(dataSet))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Read operational right information in and store in a list. </summary>
        /// <param name="filename"> Name of file to read. </param>
        /// <param name="dataSet"> StateMod_DataSet, needed to check some relationships during the read (e.g., type 24). </param>
        /// <exception cref="Exception"> if there is an error reading the file. </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public static java.util.List<StateMod_OperationalRight> readStateModFile(String filename, StateMod_DataSet dataSet) throws Exception
        public static IList<StateMod_OperationalRight> readStateModFile(string filename, StateMod_DataSet dataSet)
        {
            int version = determineFileVersion(filename);
            if (version == 1)
            {
                // TODO SAM 2010-12-27 Evaluate whether old format should be supported - too much work.
                throw new Exception("StateMod operating rules file format 1 is not supported.");
                //return readStateModFileVersion1(filename);
            }
            else if (version == 2)
            {
                return readStateModFileVersion2(filename, dataSet);
            }
            else
            {
                throw new Exception("Unable to determine StateMod file version to read operational rights.");
            }
        }

        /// <summary>
        /// Read the StateMod operational rights file associated operating rule. </summary>
        /// <param name="routine"> to use for logging. </param>
        /// <param name="linecount"> Line count (1+) before reading in this method. </param>
        /// <param name="in"> BufferedReader to read. </param>
        /// <param name="anOprit"> Operational right for which to read data. </param>
        /// <returns> the number of errors. </returns>
        /// <exception cref="IOException"> if there is an error reading the file </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: private static int readStateModFile_AssociatedOperatingRule(String routine, int linecount, java.io.BufferedReader in, StateMod_OperationalRight anOprit) throws java.io.IOException
        private static int readStateModFile_AssociatedOperatingRule(string routine, int linecount, StreamReader @in, StateMod_OperationalRight anOprit)
        {
            string iline = null;
            try
            {
                iline = @in.ReadLine();
                ++linecount;
                Message.printStatus(2, routine, "Processing operating rule " + anOprit.getItyopr() + " associated operating rule " + (linecount + 1) + ": " + iline);
                anOprit.setCx(iline.Trim());
            }
            catch (Exception e)
            {
                // TODO SAM 2010-12-13 Need to handle errors and provide feedback
                Message.printWarning(3, routine, "Error reading associated operating rule at line " + (linecount + 1) + ": " + iline + " (" + e + ")");
                return 1;
            }
            return 0;
        }

        /// <summary>
        /// Read the StateMod operational rights file intervening structures, with loss. </summary>
        /// <param name="ninterv"> Intervening structures switch. </param>
        /// <param name="routine"> to use for logging. </param>
        /// <param name="linecount"> Line count (1+) before reading in this method. </param>
        /// <param name="in"> BufferedReader to read. </param>
        /// <param name="anOprit"> Operational right for which to read data. </param>
        /// <returns> the number of errors. </returns>
        /// <exception cref="IOException"> if there is an error reading the file </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: private static int readStateModFile_InterveningStructuresWithLoss(int ninterv, String routine, int linecount, java.io.BufferedReader in, StateMod_OperationalRight anOprit) throws java.io.IOException
        private static int readStateModFile_InterveningStructuresWithLoss(int ninterv, string routine, int linecount, StreamReader @in, StateMod_OperationalRight anOprit)
        { // One line has up to 10 intervening structure identifiers
            int errorCount = 0;
            string iline = "";
            try
            {
                for (int i = 0; i < ninterv; i++)
                {
                    iline = @in.ReadLine().Trim();
                    ++linecount;
                    Message.printStatus(2, routine, "Processing operating rule " + anOprit.getItyopr() + " intervening structures with loss line " + (linecount + 1) + ": " + iline);
                    IList<string> tokens = StringUtil.breakStringList(iline, " \t", StringUtil.DELIM_SKIP_BLANKS);
                    int ntokens = 0;
                    if (tokens != null)
                    {
                        ntokens = tokens.Count;
                    }
                    if (ntokens > 0)
                    {
                        anOprit.setIntern(i, tokens[0], false);
                    }
                    if (ntokens > 1)
                    {
                        if (StringUtil.isDouble(tokens[1]))
                        {
                            anOprit.setOprLossC(i, tokens[1]);
                        }
                        else
                        {
                            Message.printWarning(3, routine, "Intervening structure " + (i + 1) + " loss percent (" + tokens[1] + " is not a number.");
                        }
                    }
                    if (ntokens > 2)
                    {
                        anOprit.setInternT(i, tokens[2]);
                    }
                }
            }
            catch (Exception e)
            {
                // TODO SAM 2010-12-13 Need to handle errors and provide feedback
                Message.printWarning(3, routine, "Error reading intervening structures at line " + (linecount + 1) + ": " + iline + " (" + e + ")");
            }
            return errorCount;
        }

        /// <summary>
        /// Read the StateMod operational rights file intervening structures, without loss. </summary>
        /// <param name="ninterv"> Intervening structures switch. </param>
        /// <param name="routine"> to use for logging. </param>
        /// <param name="linecount"> Line count (1+) before reading in this method. </param>
        /// <param name="in"> BufferedReader to read. </param>
        /// <param name="anOprit"> Operational right for which to read data. </param>
        /// <returns> the number of errors. </returns>
        /// <exception cref="IOException"> if there is an error reading the file </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: private static int readStateModFile_InterveningStructuresWithoutLoss(int ninterv, String routine, int linecount, java.io.BufferedReader in, StateMod_OperationalRight anOprit) throws java.io.IOException
        private static int readStateModFile_InterveningStructuresWithoutLoss(int ninterv, string routine, int linecount, StreamReader @in, StateMod_OperationalRight anOprit)
        { // One line has up to 10 intervening structure identifiers - format 10a12
            string iline = @in.ReadLine().Trim();
            string format = "x36a12a12a12a12a12a12a12a12a12a12";
            ++linecount;
            int errorCount = 0;
            try
            {
                Message.printStatus(2, routine, "Processing operating rule " + anOprit.getItyopr() + " intervening structures without loss line " + (linecount + 1) + ": " + iline);
                IList<object> v = StringUtil.fixedRead(iline, format);
                for (int i = 0; i < ninterv; i++)
                {
                    anOprit.setIntern(i, ((string)v[i]).Trim(), false);
                }
            }
            catch (Exception e)
            {
                // TODO SAM 2010-12-13 Need to handle errors and provide feedback
                Message.printWarning(3, routine, "Error reading intervening structures at line " + (linecount + 1) + ": " + iline + " (" + e + ")");
            }
            return errorCount;
        }

        /// <summary>
        /// Read the StateMod operational rights file intervening structures. </summary>
        /// <param name="routine"> to use for logging. </param>
        /// <param name="linecount"> Line count (1+) before reading in this method. </param>
        /// <param name="in"> BufferedReader to read. </param>
        /// <param name="anOprit"> Operational right for which to read data. </param>
        /// <returns> the number of errors. </returns>
        /// <exception cref="IOException"> if there is an error reading the file </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: private static int readStateModFile_MonthlyAndAnnualLimitationData(String routine, int linecount, java.io.BufferedReader in, StateMod_OperationalRight anOprit) throws java.io.IOException
        private static int readStateModFile_MonthlyAndAnnualLimitationData(string routine, int linecount, StreamReader @in, StateMod_OperationalRight anOprit)
        { // One line has up to 10 intervening structure identifiers
            string iline = @in.ReadLine().Trim();
            int errorCount = 0;
            try
            {
                Message.printStatus(2, routine, "Processing operating rule monthly and annual limitation line " + (linecount + 1) + ": " + iline);
                IList<string> tokens = StringUtil.breakStringList(iline, " \t", StringUtil.DELIM_SKIP_BLANKS);
                int ntokens = 0;
                if (tokens != null)
                {
                    ntokens = tokens.Count;
                }
                // Only one identifier
                if (ntokens > 0)
                {
                    anOprit.setCx(tokens[0].Trim());
                }
            }
            catch (Exception e)
            {
                // TODO SAM 2010-12-13 Need to handle errors and provide feedback
                Message.printWarning(3, routine, "Error reading monthly and annual limitation at line " + (linecount + 1) + ": " + iline + " (" + e + ")");
            }
            return errorCount;
        }

        /// <summary>
        /// Read the StateMod operational rights file monthly efficiencies.  This method is only called if the
        /// data line needs to be read. </summary>
        /// <param name="routine"> to use for logging. </param>
        /// <param name="linecount"> Line count (1+) before reading in this method. </param>
        /// <param name="in"> BufferedReader to read. </param>
        /// <param name="anOprit"> Operational right for which to read data. </param>
        /// <returns> the number of errors. </returns>
        /// <exception cref="IOException"> if there is an error reading the file </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: private static int readStateModFile_MonthlyOprEff(String routine, int linecount, java.io.BufferedReader in, StateMod_OperationalRight anOprit) throws java.io.IOException
        private static int readStateModFile_MonthlyOprEff(string routine, int linecount, StreamReader @in, StateMod_OperationalRight anOprit)
        {
            string iline = null;
            try
            {
                iline = @in.ReadLine().Trim();
                ++linecount;
                Message.printStatus(2, routine, "Processing operating rule " + anOprit.getItyopr() + " monthly operating limits " + (linecount + 1) + ": " + iline);
                // Limits are free format, but 13 are expected
                IList<string> tokens = StringUtil.breakStringList(iline, " \t", StringUtil.DELIM_SKIP_BLANKS);
                int ntokens = 0;
                if (tokens != null)
                {
                    ntokens = tokens.Count;
                }
                if (ntokens > 12)
                {
                    ntokens = 12;
                }
                for (int i = 0; i < ntokens; i++)
                {
                    anOprit.setOprEff(i, tokens[i].Trim());
                }
            }
            catch (Exception e)
            {
                // TODO SAM 2010-12-13 Need to handle errors and provide feedback
                Message.printWarning(3, routine, "Error reading monthly operational limits at line " + (linecount + 1) + ": " + iline + " (" + e + ")");
                return 1;
            }
            return 0;
        }

        /// <summary>
        /// Read the StateMod operational rights file monthly operational limits. </summary>
        /// <param name="routine"> to use for logging. </param>
        /// <param name="linecount"> Line count (1+) before reading in this method. </param>
        /// <param name="in"> BufferedReader to read. </param>
        /// <param name="anOprit"> Operational right for which to read data. </param>
        /// <returns> the number of errors. </returns>
        /// <exception cref="IOException"> if there is an error reading the file </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: private static int readStateModFile_MonthlyOprMax(String routine, int linecount, java.io.BufferedReader in, StateMod_OperationalRight anOprit) throws java.io.IOException
        private static int readStateModFile_MonthlyOprMax(string routine, int linecount, StreamReader @in, StateMod_OperationalRight anOprit)
        {
            double oprLimits = anOprit.getOprLimit();
            int oprLimitsInt = 0;
            if (oprLimits > 0)
            {
                oprLimitsInt = (int)(oprLimits + .1);
            }
            else if (oprLimits < 0)
            {
                oprLimitsInt = (int)(oprLimits - .1);
            }
            string iline = null;
            try
            {
                if (oprLimitsInt == 1)
                {
                    iline = @in.ReadLine().Trim();
                    ++linecount;
                    Message.printStatus(2, routine, "Processing operating rule " + anOprit.getItyopr() + " monthly operating limits " + (linecount + 1) + ": " + iline);
                    // Limits are free format, but 13 are expected
                    IList<string> tokens = StringUtil.breakStringList(iline, " \t", StringUtil.DELIM_SKIP_BLANKS);
                    int ntokens = 0;
                    if (tokens != null)
                    {
                        ntokens = tokens.Count;
                    }
                    for (int i = 0; i < ntokens; i++)
                    {
                        anOprit.setOprMax(i, tokens[i].Trim());
                    }
                }
            }
            catch (Exception e)
            {
                // TODO SAM 2010-12-13 Need to handle errors and provide feedback
                Message.printWarning(3, routine, "Error reading monthly operational limits at line " + (linecount + 1) + ": " + iline + " (" + e + ")");
                return 1;
            }
            return 0;
        }

        /// <summary>
        /// Read the StateMod operational rights file monthly switches. </summary>
        /// <param name="nmonsw"> Monthly switch </param>
        /// <param name="routine"> to use for logging. </param>
        /// <param name="linecount"> Line count (1+) before reading in this method. </param>
        /// <param name="in"> BufferedReader to read. </param>
        /// <param name="anOprit"> Operational right for which to read data. </param>
        /// <returns> the number of errors. </returns>
        /// <exception cref="IOException"> if there is an error reading the file </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: private static int readStateModFile_MonthlySwitches(int nmonsw, String routine, int linecount, java.io.BufferedReader in, StateMod_OperationalRight anOprit) throws java.io.IOException
        private static int readStateModFile_MonthlySwitches(int nmonsw, string routine, int linecount, StreamReader @in, StateMod_OperationalRight anOprit)
        {
            string iline = @in.ReadLine().Trim();
            ++linecount;
            try
            {
                Message.printStatus(2, routine, "Processing operating rule " + anOprit.getItyopr() + " monthly switch line " + (linecount + 1) + ": " + iline);
                // Switches are free format
                IList<string> tokens = StringUtil.breakStringList(iline, " \t", StringUtil.DELIM_SKIP_BLANKS);
                int ntokens = 0;
                if (tokens != null)
                {
                    ntokens = tokens.Count;
                }
                if (nmonsw > 0)
                {
                    anOprit._imonsw = new int[nmonsw];
                }
                for (int i = 0; i < ntokens; i++)
                {
                    anOprit.setImonsw(i, tokens[i].Trim());
                }
            }
            catch (Exception e)
            {
                // TODO SAM 2010-12-13 Need to handle errors and provide feedback
                Message.printWarning(3, routine, "Error reading monthly switches at line " + (linecount + 1) + ": " + iline + " (" + e + ")");
                return 1;
            }
            return 0;
        }

        /// <summary>
        /// Read the StateMod operational rights file Rio Grande data. </summary>
        /// <param name="routine"> to use for logging. </param>
        /// <param name="linecount"> Line count (1+) before reading in this method. </param>
        /// <param name="in"> BufferedReader to read. </param>
        /// <param name="anOprit"> Operational right for which to read data. </param>
        /// <returns> the number of errors. </returns>
        /// <exception cref="IOException"> if there is an error reading the file </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: private static int readStateModFile_RioGrande(String routine, int linecount, java.io.BufferedReader in, StateMod_OperationalRight anOprit) throws java.io.IOException
        private static int readStateModFile_RioGrande(string routine, int linecount, StreamReader @in, StateMod_OperationalRight anOprit)
        {
            int errorCount = 0;
            // Rio Grande additional data...
            // StateMod doc treats last part as numbers but treat as strings here consistent with source ID/account
            //String formatRioGrande = "x64f8f8x1a12i8x1a12i8x1a12i8";
            string formatRioGrande = "x64f8f8x1a12a8x1a12a8x1a12a8";
            IList<object> v = null;
            string iline = @in.ReadLine();
            ++linecount;
            try
            {
                Message.printStatus(2, routine, "Processing operating rule " + anOprit.getItyopr() + " Rio Grande data line " + (linecount + 1) + ": " + iline);
                v = StringUtil.fixedRead(iline, formatRioGrande);
                anOprit.setQdebt((float?)v[0]);
                anOprit.setQdebtx((float?)v[1]);
                anOprit.setCiopso3(((string)v[2]).Trim());
                anOprit.setIopsou3(((string)v[3]).Trim());
                anOprit.setCiopso4(((string)v[4]).Trim());
                anOprit.setIopsou4(((string)v[5]).Trim());
                anOprit.setCiopso5(((string)v[6]).Trim());
                anOprit.setIopsou5(((string)v[7]).Trim());
            }
            catch (Exception e)
            {
                // TODO SAM 2010-12-13 Need to handle errors and provide feedback
                Message.printWarning(3, routine, "Error reading Rio Grande data at line " + (linecount + 1) + ": " + iline + " (" + e + ")");
            }
            return errorCount;
        }

        /// <summary>
        /// Read the StateMod operational rights file San Juan data. </summary>
        /// <param name="routine"> to use for logging. </param>
        /// <param name="linecount"> Line count (1+) before reading in this method. </param>
        /// <param name="in"> BufferedReader to read. </param>
        /// <param name="anOprit"> Operational right for which to read data. </param>
        /// <returns> the number of errors. </returns>
        /// <exception cref="IOException"> if there is an error reading the file </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: private static int readStateModFile_SanJuan(String routine, int linecount, java.io.BufferedReader in, StateMod_OperationalRight anOprit) throws java.io.IOException
        private static int readStateModFile_SanJuan(string routine, int linecount, StreamReader @in, StateMod_OperationalRight anOprit)
        {
            int errorCount = 0;
            // San Juan additional data...
            string formatSanJuan = "x64f8f8";
            IList<object> v = null;
            string iline = @in.ReadLine();
            ++linecount;
            try
            {
                Message.printStatus(2, routine, "Processing operating rule " + anOprit.getItyopr() + " San Juan data line " + (linecount + 1) + ": " + iline);
                v = StringUtil.fixedRead(iline, formatSanJuan);
                anOprit.setSjmina((float?)v[0]);
                anOprit.setSjrela((float?)v[1]);
            }
            catch (Exception e)
            {
                // TODO SAM 2010-12-13 Need to handle errors and provide feedback
                Message.printWarning(3, routine, "Error reading San Juan data at line " + (linecount + 1) + ": " + iline + " (" + e + ")");
            }
            return errorCount;
        }

        /// <summary>
        /// Read operational right information in and store in a list. </summary>
        /// <param name="filename"> Name of file to read - the file should be the older "version 2" format. </param>
        /// <param name="dataSet"> StateMod_DataSet, needed to check some relationships during the read (e.g., type 24). </param>
        /// <exception cref="Exception"> if there is an error reading the file. </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: private static java.util.List<StateMod_OperationalRight> readStateModFileVersion2(String filename, StateMod_DataSet dataSet) throws Exception
        private static IList<StateMod_OperationalRight> readStateModFileVersion2(string filename, StateMod_DataSet dataSet)
        {
            string routine = "StateMod_OperationalRight.readStateModFileVersion2";
            string iline = null;
            IList<object> v = null;
            IList<StateMod_OperationalRight> theOprits = new List<StateMod_OperationalRight>();
            IList<string> commentsBeforeData = new List<string>(); // Will be used prior to finding an operational right
                                                                   // Formats use strings for many variables because files may have extra
                                                                   // whitespace or be used for numeric and character data...
                                                                   // Consistent among all operational rights...
                                                                   // Before adding creuse, etc... (12 values)
                                                                   //   String format_0 = "s12s24x16s12s8i8x1s12s8x1s12s8x1s12s8s8";
                                                                   // After adding creuse, etc.... (18 values)
                                                                   // Format to read line 1.  The following differ from the StateMod documentation (as of Nov 2008 doc):
                                                                   // - administration number is read as a string (not float) to prevent roundoff since this
                                                                   //   is an important number
                                                                   // - iopdes (destination account) is treated as a string (not integer) for flexibility
                                                                   // - creuse, cdivtyp, OprLoss, OprLimit, IoBeg, and IoEnd are read as strings and allowed to be
                                                                   //   missing, which will use StateMod internal defaults
                                                                   // 
            string formatLine1 = "a12a24x12x4a12f8i8x1a12a8x1a12a8x1a12a8i8x1a12x1a12x1a8a8a8a8";
            StateMod_OperationalRight anOprit = null;
            StreamReader @in = null;
            int linecount = 0;

            int dumxInt, ninterv, nmonsw;
            float dumxFloat;
            double oprLimit; // Internal value
            int rightType = 0;
            int errorCount = 0;

            Message.printStatus(2, routine, "Reading operational rights file \"" + filename + "\"");
            try
            {
                bool readingUnknownRight = false;
                IList<string> rightStringsList = null; // Operating rule as a list of strings
                @in = new StreamReader(filename);
                while (!string.ReferenceEquals((iline = @in.ReadLine()), null))
                {
                    ++linecount;
                    Message.printStatus(2, routine, "Processing operating rule line " + linecount + ": " + iline);
                    // If was reading an unknown rule, turn off flag if done reading.
                    if (readingUnknownRight)
                    {
                        if ((iline.Length > 0) && (iline[0] != ' '))
                        {
                            // Done reading the unknown right.  Next are either comments before or data for
                            // the next right.
                            readingUnknownRight = false;
                            // Add to the end of the list
                            Message.printStatus(2, routine, "Adding unrecognized operational right \"" + anOprit.getID() + "\" as text from previous lines");
                            theOprits.Add(anOprit);
                            // Don't continue because the line that was just read needs to be handled.
                        }
                        else
                        {
                            // Blank at front of line so assume still reading the unknown right.
                            // Add a string to the unknown right
                            rightStringsList.Add(iline);
                            continue;
                        }
                    }
                    // check for comments
                    // if a temporary comment line
                    if (iline.StartsWith("#>", StringComparison.Ordinal))
                    {
                        continue;
                    }
                    /* TODO SAM 2008-03-10 Evaluate whether needed
                    else if ((iline.startsWith("#") && !readingTmpComments)	|| iline.trim().length()==0) {
                        // A general comment line not associated with an operational right...
                        continue;
                    }
                    */
                    else if (iline.StartsWith("#", StringComparison.Ordinal))
                    {
                        // A comment line specific to an individual operational right...
                        string comment = iline.Substring(1);
                        Message.printStatus(2, routine, "Treating as comment before right: \"" + comment + "\"");
                        // Don't trim because may want to compare output during testing
                        // Do trim the initial #, which will get added on output.
                        commentsBeforeData.Add(comment);
                        continue;
                    }

                    // Allocate new operational rights object
                    anOprit = new StateMod_OperationalRight();
                    if (Message.isDebugOn)
                    {
                        Message.printDebug(10, routine, "Number of Opright comments: " + commentsBeforeData.Count);
                    }
                    if (commentsBeforeData.Count > 0)
                    {
                        // Set comments that have been read previous to this line.  First, attempt to discard
                        // comments that do not below with the operational right.  For now, search backward for
                        // "FileFormatVersion", "EndHeader", and "--e".  If found, discard the comments prior
                        // to this because they are assumed to be file header comments, not comments for a specific right.
                        string comment;
                        for (int iComment = commentsBeforeData.Count - 1; iComment >= 0; --iComment)
                        {
                            comment = commentsBeforeData[iComment].ToUpper();
                            if ((comment.IndexOf("FILEFORMATVERSION", StringComparison.Ordinal) >= 0) || (comment.IndexOf("ENDHEADER", StringComparison.Ordinal) >= 0))
                            { //|| (comment.indexOf("--E") >= 0) ) {
                              // TODO SAM 2011-02-05 Problem --E often found intermingled in file to help users
                              // Remove the comments above the position.
                                while (iComment >= 0)
                                {
                                    commentsBeforeData.RemoveAt(iComment--);
                                }
                                break;
                            }
                        }
                        anOprit.setCommentsBeforeData(commentsBeforeData);
                    }
                    // Always clear out for next right...
                    commentsBeforeData = new List<string>(1);

                    // line 1
                    if (Message.isDebugOn)
                    {
                        Message.printDebug(50, routine, "line 1: " + iline);
                    }
                    v = StringUtil.fixedRead(iline, formatLine1);
                    if (Message.isDebugOn)
                    {
                        Message.printDebug(1, routine, v.ToString());
                    }
                    anOprit.setID(((string)v[0]).Trim());
                    anOprit.setName(((string)v[1]).Trim());
                    anOprit.setRtem(((string)v[2]).Trim());
                    dumxFloat = (float)v[3];
                    if (dumxFloat >= 0.0)
                    {
                        anOprit.setDumx((int)(dumxFloat + .1)); // Add .1 to make sure 11.9999 ends up as 12, etc.
                    }
                    else
                    {
                        anOprit.setDumx((int)(dumxFloat - .1)); // Subtract .1 to make sure 11.9999 ends up as 12, etc.
                    }
                    dumxInt = anOprit.getDumx();
                    anOprit.setIoprsw((int?)v[4]);
                    // Destination data - should always be in file but may be zero...
                    anOprit.setCiopde(((string)v[5]).Trim());
                    anOprit.setIopdes(((string)v[6]).Trim());
                    // Supply data - should always be in file but may be zero...
                    anOprit.setCiopso1(((string)v[7]).Trim());
                    anOprit.setIopsou1(((string)v[8]).Trim());
                    // Should always be in file but may be zero...
                    anOprit.setCiopso2(((string)v[9]).Trim());
                    anOprit.setIopsou2(((string)v[10]).Trim());
                    // Type is used to make additional decisions below...
                    anOprit.setItyopr((int?)v[11]);
                    rightType = anOprit.getItyopr();
                    Message.printStatus(2, routine, "rightType=" + rightType + " DumxF=" + dumxFloat + " DumxI=" + dumxInt);
                    // Plan ID
                    anOprit.setCreuse(((string)v[12]).Trim());
                    // Diversion type
                    anOprit.setCdivtyp(((string)v[13]).Trim());
                    // Conveyance loss...
                    string OprLoss = ((string)v[14]).Trim();
                    if (StringUtil.isDouble(OprLoss))
                    {
                        anOprit.setOprLoss(OprLoss);
                    }
                    double oprLossDouble = anOprit.getOprLoss();

                    // Miscellaneous limits...
                    string OprLimit = ((string)v[15]).Trim();
                    if (StringUtil.isDouble(OprLimit))
                    {
                        anOprit.setOprLimit(OprLimit);
                    }
                    oprLimit = anOprit.getOprLimit();
                    // Beginning year...
                    string IoBeg = ((string)v[16]).Trim();
                    if (StringUtil.isInteger(IoBeg))
                    {
                        anOprit.setIoBeg(IoBeg);
                    }
                    // Ending year...
                    string IoEnd = ((string)v[17]).Trim();
                    if (StringUtil.isInteger(IoEnd))
                    {
                        anOprit.setIoEnd(IoEnd);
                    }
                    Message.printStatus(2, routine, "Reading operating rule type " + rightType + " starting at line " + linecount);

                    bool rightUnderstoodByCode = isRightUnderstoodByCode(rightType, dataSet);

                    if (!rightUnderstoodByCode)
                    {
                        // The type is not known so read in as strings and set the type to negative.
                        // Most of the reading will occur at the top of the loop.
                        readingUnknownRight = true;
                        rightStringsList = new List<string>();
                        rightStringsList.Add(iline);
                        // Add list and continue to add if more lines are read.  Since using a reference
                        // this will ensure that all lines are set for the right.
                        anOprit.setRightStrings(rightStringsList);
                        Message.printWarning(2, routine, "Unknown right type " + rightType + " at line " + linecount + ".  Reading as text to continue reading file.");
                        // Add metadata so that code in the GUI for example will be able to list the right type, but
                        // treat as text
                        StateMod_OperationalRight_Metadata Metadata = StateMod_OperationalRight_Metadata.getMetadata(rightType);
                        if (Metadata == null)
                        {
                            StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source1Array_1 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[0];
                            StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source2Array_1 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[0];
                            StateMod_OperationalRight_Metadata_SourceOrDestinationType[] destinationArray_1 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[0];
                            StateMod_OperationalRight_Metadata_DestinationLocationType[] destinationLocationArray_1 = new StateMod_OperationalRight_Metadata_DestinationLocationType[0];
                            StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] associatedPlanAllowedArray_1 = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[0];
                            StateMod_OperationalRight_Metadata_DiversionType[] diversionTypeArray_1 = new StateMod_OperationalRight_Metadata_DiversionType[0];
                            StateMod_OperationalRight_Metadata.getAllMetadata().Add(new StateMod_OperationalRight_Metadata(rightType, false, "Unknown Type", StateMod_OperationalRight_Metadata_RuleType.NA, source1Array_1, source2Array_1, StateMod_OperationalRight_Metadata_Source2AllowedType.NA, destinationArray_1, destinationLocationArray_1, StateMod_OperationalRight_Metadata_DeliveryMethodType.NA, StateMod_OperationalRight_Metadata_CarrierAllowedType.NA, false, false, associatedPlanAllowedArray_1, diversionTypeArray_1, StateMod_OperationalRight_Metadata_TransitAndConveyanceLossAllowedType.NA, ""));
                        }
                        continue;
                    }

                    // If here the operational right is understood and additional lines of data may be provided.

                    // May have monthly switch and intervening structures.  For now check the value.
                    StateMod_OperationalRight_Metadata metadata = StateMod_OperationalRight_Metadata.getMetadata(rightType);

                    // FIXME SAM 2008-03-17 Need some more checks for things like invalid -11 and + 13

                    // Start reading additional information before monthly and intervening structure data)...

                    if (metadata.getRightTypeUsesRioGrande())
                    {
                        errorCount += readStateModFile_RioGrande(routine, linecount, @in, anOprit);
                        ++linecount; // Increment here because copy passed in to above call is local to that method
                    }

                    if (metadata.getRightTypeUsesSanJuan())
                    {
                        errorCount += readStateModFile_SanJuan(routine, linecount, @in, anOprit);
                        ++linecount; // Increment here because copy passed in to above call is local to that method
                    }

                    // ...end reading additional data before monthly and intervening structure data

                    // Start reading the monthly and intervening structure data - first split the "dumx"
                    // value into parts...
                    nmonsw = 0;
                    ninterv = 0;
                    // Special case for type 17 and 18, where -8 means no monthly switches and -20 = use switches
                    if ((rightType == 17) || (rightType == 18))
                    {
                        nmonsw = 0; // Default
                        ninterv = 0;
                        if (dumxInt == -20)
                        {
                            nmonsw = 12;
                        }
                    }
                    else
                    {
                        // Normal interpretation of dumx
                        if (dumxInt == 12)
                        {
                            // Only have monthly switches
                            nmonsw = 12;
                        }
                        else if (dumxInt >= 0)
                        {
                            // Only have intervening structures...
                            ninterv = dumxInt;
                        }
                        else if (dumxInt < 0)
                        {
                            // Have monthly switches and intervening structures.
                            // -12 of the total count toward the monthly switch and the remainder is
                            // the number of intervening structures
                            if (dumxInt < -12)
                            {
                                ninterv = -1 * (dumxInt + 12);
                                nmonsw = 12;
                            }
                            else
                            {
                                ninterv = -1 * dumxInt;
                            }
                        }
                    }

                    Message.printStatus(2, routine, "Dumx=" + dumxInt + ", number of intervening structures = " + ninterv + " month switch = " + nmonsw);

                    if (metadata.getRightTypeUsesMonthlySwitch())
                    {
                        if (nmonsw > 0)
                        {
                            errorCount += readStateModFile_MonthlySwitches(nmonsw, routine, linecount, @in, anOprit);
                            ++linecount; // Increment here because copy passed in to above call is local to that method
                        }
                    }

                    if (metadata.getRightTypeUsesAssociatedOperatingRule(oprLimit))
                    {
                        errorCount += readStateModFile_AssociatedOperatingRule(routine, linecount, @in, anOprit);
                        ++linecount; // Increment here because copy passed in to above call is local to that method
                    }

                    if (metadata.getRightTypeUsesInterveningStructuresWithoutLoss())
                    {
                        // Only read intervening structures if allowed (otherwise assume user error in input)
                        if (ninterv > 0)
                        {
                            errorCount += readStateModFile_InterveningStructuresWithoutLoss(ninterv, routine, linecount, @in, anOprit);
                            ++linecount; // Increment here because copy passed in to above call is local to that method
                        }
                    }
                    if (metadata.getRightTypeUsesInterveningStructuresWithLoss(oprLossDouble))
                    {
                        // Only read intervening structures if allowed (otherwise assume user error in input)
                        if (ninterv > 0)
                        {
                            errorCount += readStateModFile_InterveningStructuresWithLoss(ninterv, routine, linecount, @in, anOprit);
                            ++linecount; // Increment here because copy passed in to above call is local to that method
                        }
                    }

                    // ...end reading monthly and intervening structure data.
                    // Start reading additional records after monthly and intervening structure...

                    if (metadata.getRightTypeUsesMonthlyOprMax(oprLimit))
                    {
                        errorCount += readStateModFile_MonthlyOprMax(routine, linecount, @in, anOprit);
                        ++linecount; // Increment here because copy passed in to above call is local to that method
                    }

                    if (metadata.getRightTypeUsesMonthlyOprEff(dataSet, anOprit.getCiopso2(), anOprit.getIopsou2()))
                    {
                        errorCount += readStateModFile_MonthlyOprEff(routine, linecount, @in, anOprit);
                        ++linecount; // Increment here because copy passed in to above call is local to that method
                    }

                    // ...end reading additional data after monthly and intervening structure data

                    // add the operational right to the vector of rights
                    Message.printStatus(2, routine, "Adding recognized operational right type " + rightType + " \"" + anOprit.getID() + "\" from full read - " + anOprit.getCommentsBeforeData().Count + " comments before data");
                    theOprits.Add(anOprit);
                }
                // All lines have been read.
                if (readingUnknownRight)
                {
                    // Last line was part of the unknown right so need to add what there was.
                    Message.printStatus(2, routine, "Adding unrecognized operational right type " + rightType + " \"" + anOprit.getID() + "\" as text.");
                    theOprits.Add(anOprit);
                }
            }
            catch (Exception e)
            {
                Message.printWarning(3, routine, "Error reading near line " + linecount + ": " + iline);
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
            // If there were any errors, generate an exception
            if (errorCount > 0)
            {
                throw new Exception("There were " + errorCount + " errors reading the operational rights.");
            }
            return theOprits;
        }

        /// <summary>
        /// Set the cdivtyp.
        /// </summary>
        public virtual void setCdivtyp(string cdivtyp)
        {
            if ((!string.ReferenceEquals(cdivtyp, null)) && !cdivtyp.Equals(__cdivtyp))
            {
                __cdivtyp = cdivtyp;
                setDirty(true);
                if (Message.isDebugOn)
                {
                    Message.printDebug(1, "", "Setting cdivtyp dirty");
                }
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
                }
            }
        }

        /// <summary>
        /// Set the user ciopde.
        /// </summary>
        public virtual void setCiopde(string ciopde)
        {
            if ((!string.ReferenceEquals(ciopde, null)) && !ciopde.Equals(_ciopde))
            {
                _ciopde = ciopde;
                setDirty(true);
                if (Message.isDebugOn)
                {
                    Message.printDebug(1, "", "Setting ciopde dirty");
                }
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
                }
            }
        }

        /// <summary>
        /// Set the user ciopso.
        /// </summary>
        public virtual void setCiopso1(string ciopso1)
        {
            if ((!string.ReferenceEquals(ciopso1, null)) && !ciopso1.Equals(_ciopso1))
            {
                _ciopso1 = ciopso1;
                setDirty(true);
                if (Message.isDebugOn)
                {
                    Message.printDebug(1, "", "Setting ciopso1 dirty");
                }
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
                }
            }
        }

        /// <summary>
        /// Set the user ciopso2.
        /// </summary>
        public virtual void setCiopso2(string ciopso2)
        {
            if ((!string.ReferenceEquals(ciopso2, null)) && !ciopso2.Equals(_ciopso2))
            {
                _ciopso2 = ciopso2;
                setDirty(true);
                if (Message.isDebugOn)
                {
                    Message.printDebug(1, "", "Setting ciopso2 dirty");
                }
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
                }
            }
        }

        /// <summary>
        /// Set the user ciopso3.
        /// </summary>
        public virtual void setCiopso3(string ciopso3)
        {
            if ((!string.ReferenceEquals(ciopso3, null)) && !ciopso3.Equals(_ciopso3))
            {
                _ciopso3 = ciopso3;
                setDirty(true);
                if (Message.isDebugOn)
                {
                    Message.printDebug(1, "", "Setting ciopso3 dirty");
                }
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
                }
            }
        }

        /// <summary>
        /// Set the user ciopso4.
        /// </summary>
        public virtual void setCiopso4(string ciopso4)
        {
            if ((!string.ReferenceEquals(ciopso4, null)) && !ciopso4.Equals(_ciopso4))
            {
                _ciopso4 = ciopso4;
                setDirty(true);
                if (Message.isDebugOn)
                {
                    Message.printDebug(1, "", "Setting ciopso4 dirty");
                }
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
                }
            }
        }

        /// <summary>
        /// Set the user ciopso5.
        /// </summary>
        public virtual void setCiopso5(string ciopso5)
        {
            if ((!string.ReferenceEquals(ciopso5, null)) && !ciopso5.Equals(_ciopso5))
            {
                _ciopso5 = ciopso5;
                setDirty(true);
                if (Message.isDebugOn)
                {
                    Message.printDebug(1, "", "Setting ciopso5 dirty");
                }
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
                }
            }
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
                if (Message.isDebugOn)
                {
                    Message.printDebug(1, "", "commentsBeforeData old size=" + commentsBeforeData0.Count + " new size=" + size);
                }
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
                        if (Message.isDebugOn)
                        {
                            Message.printDebug(1, "", "commentsBeforeData old string \"" + commentsBeforeData0[i] + "\" is different from new string \"" + commentsBeforeData[i] + "\"");
                        }
                        break;
                    }
                }
            }
            if (dirty)
            {
                // Something was different so set the comments and change the dirty flag
                __commentsBeforeData = commentsBeforeData;
                setDirty(true);
                if (Message.isDebugOn)
                {
                    Message.printDebug(1, "", "Setting commentsBeforeData dirty");
                }
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
                }
            }
        }

        /// <summary>
        /// Set the creuse.
        /// </summary>
        public virtual void setCreuse(string creuse)
        {
            if ((!string.ReferenceEquals(creuse, null)) && !creuse.Equals(__creuse))
            {
                __creuse = creuse;
                setDirty(true);
                if (Message.isDebugOn)
                {
                    Message.printDebug(1, "", "Setting creuse dirty");
                }
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
                }
            }
        }

        /// <summary>
        /// Set the cx.
        /// </summary>
        public virtual void setCx(string cx)
        {
            if ((!string.ReferenceEquals(cx, null)) && !cx.Equals(__cx))
            {
                __cx = cx;
                setDirty(true);
                if (Message.isDebugOn)
                {
                    Message.printDebug(1, "", "Setting cx dirty");
                }
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
                }
            }
        }

        /// <summary>
        /// Set dumx.  This method should only be called when reading the StateMod operational rights file and otherwise
        /// dumx is calculated internally. </summary>
        /// <param name="dumx"> monthly/intervening structures switch </param>
        public virtual void setDumx(int dumx)
        {
            if (dumx != _dumx)
            {
                _dumx = dumx;
                setDirty(true);
                if (Message.isDebugOn)
                {
                    Message.printDebug(1, "", "Setting dumx dirty");
                }
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
                }
            }
        }

        /// <summary>
        /// Set dumx.  This method should only be called when reading the StateMod operational rights file and otherwise
        /// dumx is calculated internally. </summary>
        /// <param name="dumx"> monthly/intervening structures switch </param>
        public virtual void setDumx(int? dumx)
        {
            setDumx(dumx.Value);
        }

        /// <summary>
        /// Set dumx.  Note that sometimes the integer has a . at the end.  To resolve this,
        /// convert to a double and then cast as an integer.
        /// This method should only be called when reading the StateMod operational rights file and otherwise
        /// dumx is calculated internally. </summary>
        /// <param name="dumx"> monthly/intervening structures switch </param>
        public virtual void setDumx(string dumx)
        {
            if (!string.ReferenceEquals(dumx, null))
            {
                double? d = (double.Parse(dumx.Trim()));
                setDumx((int)d.Value);
            }
        }

        /// <summary>
        /// Set dumx from the monthly switch and intervening structure values.  For example this is used when
        /// editing data in the GUI and dumx is not set directly.
        /// </summary>
        private void setDumxFromMonthlySwitchAndInterveningStructures()
        {
            // All of the monthly switches need to have values -31 to +31.  Otherwise, the monthly
            // switches are assumed to be not used for the right
            int nValidImonsw = 0;
            StateMod_OperationalRight_Metadata metadata = getMetadata();
            if (metadata.getRightTypeUsesMonthlySwitch())
            {
                int[] imonsw = getImonsw();
                if (imonsw != null)
                {
                    for (int i = 0; i < imonsw.Length; i++)
                    {
                        if ((imonsw[i] >= -31) && (imonsw[i] <= 31))
                        {
                            ++nValidImonsw;
                        }
                    }
                }
            }
            int nValidInterveningStructures = 0;
            if (metadata.getRightTypeUsesInterveningStructuresWithoutLoss())
            {
                string[] intern = getIntern();
                if (intern != null)
                {
                    for (int i = 0; i < intern.Length; i++)
                    {
                        if (intern[i].Length > 0)
                        {
                            ++nValidInterveningStructures;
                        }
                    }
                }
            }
            int dumx = 0;
            if (nValidImonsw == 12)
            {
                dumx = 12;
            }
            if (nValidInterveningStructures > 0)
            {
                if (dumx > 0)
                {
                    // Have monthly switches so start with -12 value
                    dumx = -12;
                }
                // Now subtract the number of intervening structures
                dumx -= nValidInterveningStructures;
            }
            // Finally set the value
            setDumx(dumx);
        }

        /// <summary>
        /// Set ioBeg
        /// </summary>
        public virtual void setIoBeg(int ioBeg)
        {
            if (ioBeg != __ioBeg)
            {
                __ioBeg = ioBeg;
                setDirty(true);
                if (Message.isDebugOn)
                {
                    Message.printDebug(1, "", "Setting ioBeg dirty");
                }
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
                }
            }
        }

        /// <summary>
        /// Set ioBeg
        /// </summary>
        public virtual void setIoBeg(int? ioBeg)
        {
            setIoBeg(ioBeg.Value);
        }

        /// <summary>
        /// Set ioBeg.
        /// </summary>
        public virtual void setIoBeg(string ioBeg)
        {
            if (!string.ReferenceEquals(ioBeg, null))
            {
                setIoBeg((int)(int.Parse(ioBeg.Trim())));
            }
        }

        /// <summary>
        /// Set ioEnd
        /// </summary>
        public virtual void setIoEnd(int ioEnd)
        {
            if (ioEnd != __ioEnd)
            {
                __ioEnd = ioEnd;
                setDirty(true);
                if (Message.isDebugOn)
                {
                    Message.printDebug(1, "", "Setting ioEnd dirty");
                }
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
                }
            }
        }

        /// <summary>
        /// Set ioEnd
        /// </summary>
        public virtual void setIoEnd(int? ioEnd)
        {
            setIoEnd(ioEnd.Value);
        }

        /// <summary>
        /// Set ioEnd.
        /// </summary>
        public virtual void setIoEnd(string ioEnd)
        {
            if (!string.ReferenceEquals(ioEnd, null))
            {
                setIoEnd((int)(int.Parse(ioEnd.Trim())));
            }
        }

        /// <summary>
        /// Set a monthly switch.
        /// </summary>
        public virtual void setImonsw(int index, int imonsw)
        {
            if (imonsw != _imonsw[index])
            {
                _imonsw[index] = imonsw;
                setDirty(true);
                if (Message.isDebugOn)
                {
                    Message.printDebug(1, "", "Setting imonsw[" + index + "] dirty");
                }
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
                }
                // Also reset the dumx
                setDumxFromMonthlySwitchAndInterveningStructures();
            }
        }

        /// <summary>
        /// Set a monthly switch.
        /// </summary>
        public virtual void setImonsw(int index, int? imonsw)
        {
            setImonsw(index, imonsw.Value);
        }

        /// <summary>
        /// Set a monthly switch.
        /// </summary>
        public virtual void setImonsw(int index, string imonsw)
        {
            if (!string.ReferenceEquals(imonsw, null))
            {
                setImonsw(index, int.Parse(imonsw.Trim()));
            }
        }

        /// <summary>
        /// Set an "intern". </summary>
        /// <param name="setDumx"> if true, reset the dumx value based on the monthly switches and intervening structures
        /// (typically done when setting the intervening structures from the GUI, since dumx is not edited directly).
        /// If false, just set the intervening ID but do not change dumx (typically done when reading the data file). </param>
        public virtual void setIntern(int index, string intern, bool setDumx)
        {
            if (string.ReferenceEquals(intern, null))
            {
                return;
            }
            if (!intern.Equals(_intern[index]))
            {
                // Only set if not already set - otherwise will trigger dirty flag
                _intern[index] = intern;
                setDirty(true);
                if (Message.isDebugOn)
                {
                    Message.printDebug(1, "", "Setting intern[" + index + "] dirty");
                }
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
                }
                if (Message.isDebugOn)
                {
                    Message.printDebug(30, "StateMod_OperationalRight.setIntern", "Old Dumx: " + getDumx() + ", New Dumx: " + index + 1);
                }
                if (setDumx)
                {
                    setDumxFromMonthlySwitchAndInterveningStructures();
                }
                if (Message.isDebugOn)
                {
                    Message.printDebug(30, "StateMod_OperationalRight.setInter", "Dumx: " + getDumx());
                }
            }
        }

        /// <summary>
        /// Sets the interns from a list, for example when setting from the StateMod GUI.
        /// </summary>
        public virtual void setInterns(IList<string> v)
        {
            if (v != null)
            {
                for (int i = 0; i < v.Count; i++)
                {
                    setIntern(i, v[i], false);
                }
            }
        }

        /// <summary>
        /// Set an "internT".
        /// </summary>
        public virtual void setInternT(int index, string internT)
        {
            if (string.ReferenceEquals(internT, null))
            {
                return;
            }
            if (!internT.Equals(__internT[index]))
            {
                // Only set if not already set - otherwise will trigger dirty flag
                __internT[index] = internT;
                setDirty(true);
                if (Message.isDebugOn)
                {
                    Message.printDebug(1, "", "Setting internT[" + index + "] dirty");
                }
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
                }
            }
        }

        /// <summary>
        /// Set the iopdes.
        /// </summary>
        public virtual void setIopdes(string iopdes)
        {
            if ((!string.ReferenceEquals(iopdes, null)) && !iopdes.Equals(_iopdes))
            {
                _iopdes = iopdes;
                setDirty(true);
                if (Message.isDebugOn)
                {
                    Message.printDebug(1, "", "Setting iopdes dirty");
                }
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
                }
            }
        }

        /// <summary>
        /// Set the ioprsw - this calls setSwitch() in the base class.
        /// </summary>
        public virtual void setIoprsw(int? ioprsw)
        {
            setSwitch(ioprsw.Value);
        }

        /// <summary>
        /// Set the iopsou1.
        /// </summary>
        public virtual void setIopsou1(string iopsou1)
        {
            if ((!string.ReferenceEquals(iopsou1, null)) && !iopsou1.Equals(_iopsou1))
            {
                _iopsou1 = iopsou1;
                setDirty(true);
                if (Message.isDebugOn)
                {
                    Message.printDebug(1, "", "Setting iopsou1 dirty");
                }
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
                }
            }
        }

        /// <summary>
        /// Set the iopsou2.
        /// </summary>
        public virtual void setIopsou2(string iopsou2)
        {
            if ((!string.ReferenceEquals(iopsou2, null)) && !iopsou2.Equals(_iopsou2))
            {
                _iopsou2 = iopsou2;
                setDirty(true);
                if (Message.isDebugOn)
                {
                    Message.printDebug(1, "", "Setting iopsou2 dirty");
                }
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
                }
            }
        }

        /// <summary>
        /// Set the iopsou3.
        /// </summary>
        public virtual void setIopsou3(string iopsou3)
        {
            if ((!string.ReferenceEquals(iopsou3, null)) && !iopsou3.Equals(_iopsou3))
            {
                _iopsou3 = iopsou3;
                setDirty(true);
                if (Message.isDebugOn)
                {
                    Message.printDebug(1, "", "Setting iopsou3 dirty");
                }
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
                }
            }
        }

        /// <summary>
        /// Set the iopsou4.
        /// </summary>
        public virtual void setIopsou4(string iopsou4)
        {
            if ((!string.ReferenceEquals(iopsou4, null)) && !iopsou4.Equals(_iopsou4))
            {
                _iopsou4 = iopsou4;
                setDirty(true);
                if (Message.isDebugOn)
                {
                    Message.printDebug(1, "", "Setting iopsou4 dirty");
                }
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
                }
            }
        }

        /// <summary>
        /// Set the iopsou5.
        /// </summary>
        public virtual void setIopsou5(string iopsou5)
        {
            if ((!string.ReferenceEquals(iopsou5, null)) && !iopsou5.Equals(_iopsou5))
            {
                _iopsou5 = iopsou5;
                setDirty(true);
                if (Message.isDebugOn)
                {
                    Message.printDebug(1, "", "Setting iopsou5 dirty");
                }
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
                }
            }
        }

        /// <summary>
        /// Set the ityopr
        /// </summary>
        public virtual void setItyopr(int ityopr)
        {
            if (ityopr != __ityopr)
            {
                setDirty(true);
                if (Message.isDebugOn)
                {
                    Message.printDebug(1, "", "Setting ityopr dirty, old=" + __ityopr + ", new =" + ityopr);
                }
                __ityopr = ityopr;
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
                }
            }
        }

        /// <summary>
        /// Set the ityopr
        /// </summary>
        public virtual void setItyopr(int? ityopr)
        {
            setItyopr(ityopr.Value);
            // Also set the metadata for the right
            __metadata = StateMod_OperationalRight_Metadata.getMetadata(ityopr.Value);
        }

        /// <summary>
        /// Set the ityopr
        /// </summary>
        public virtual void setItyopr(string ityopr)
        {
            if (!string.ReferenceEquals(ityopr, null))
            {
                setItyopr(int.Parse(ityopr.Trim()));
            }
        }

        /// <summary>
        /// Set the OprLimit 
        /// </summary>
        public virtual void setOprLimit(string oprLimit)
        {
            if ((!string.ReferenceEquals(oprLimit, null)) && !oprLimit.Equals(""))
            {
                setOprLimit(double.Parse(oprLimit.Trim()));
            }
        }

        /// <summary>
        /// Set oprLimit
        /// </summary>
        public virtual void setOprLimit(double oprLimit)
        {
            if (oprLimit != __oprLimit)
            {
                __oprLimit = oprLimit;
                setDirty(true);
                if (Message.isDebugOn)
                {
                    Message.printDebug(1, "", "Setting oprLimit dirty");
                }
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
                }
            }
        }

        /// <summary>
        /// Set the OprLoss 
        /// </summary>
        public virtual void setOprLoss(string oprLoss)
        {
            if ((!string.ReferenceEquals(oprLoss, null)) && !oprLoss.Equals(""))
            {
                setOprLoss(double.Parse(oprLoss.Trim()));
            }
        }

        /// <summary>
        /// Set oprLoss
        /// </summary>
        public virtual void setOprLoss(double oprLoss)
        {
            if (oprLoss != __oprLoss)
            {
                __oprLoss = oprLoss;
                setDirty(true);
                if (Message.isDebugOn)
                {
                    Message.printDebug(1, "", "Setting oprLoss dirty");
                }
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
                }
            }
        }

        /// <summary>
        /// Set an oprLossC 
        /// </summary>
        public virtual void setOprLossC(int index, string oprLossC)
        {
            if ((!string.ReferenceEquals(oprLossC, null)) && !oprLossC.Equals(""))
            {
                setOprLossC(index, double.Parse(oprLossC.Trim()));
            }
        }

        /// <summary>
        /// Set an "oprLossC".
        /// </summary>
        public virtual void setOprLossC(int index, double oprLossC)
        {
            if (oprLossC != __oprLossC[index])
            {
                __oprLossC[index] = oprLossC;
                setDirty(true);
                if (Message.isDebugOn)
                {
                    Message.printDebug(1, "", "Setting oprLossC dirty");
                }
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
                }
            }
        }

        /// <summary>
        /// Set a monthly operational limit.
        /// </summary>
        public virtual void setOprEff(int index, double oprEff)
        {
            if (oprEff != __oprEff[index])
            {
                __oprEff[index] = oprEff;
                setDirty(true);
                if (Message.isDebugOn)
                {
                    Message.printDebug(1, "", "Setting oprEff[" + index + "] dirty");
                }
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
                }
            }
        }

        /// <summary>
        /// Set a monthly operational limit.
        /// </summary>
        public virtual void setOprEff(int index, double? oprEff)
        {
            setOprEff(index, oprEff.Value);
        }

        /// <summary>
        /// Set a monthly operational limit.
        /// </summary>
        public virtual void setOprEff(int index, string oprEff)
        {
            if (!string.ReferenceEquals(oprEff, null))
            {
                setOprEff(index, double.Parse(oprEff.Trim()));
            }
        }

        /// <summary>
        /// Set a monthly operational limit.
        /// </summary>
        public virtual void setOprMax(int index, double oprMax)
        {
            if (oprMax != __oprMax[index])
            {
                __oprMax[index] = oprMax;
                setDirty(true);
                if (Message.isDebugOn)
                {
                    Message.printDebug(1, "", "Setting oprMax[" + index + "] dirty");
                }
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
                }
            }
        }

        /// <summary>
        /// Set a monthly operational limit.
        /// </summary>
        public virtual void setOprMax(int index, double? oprMax)
        {
            setOprMax(index, oprMax.Value);
        }

        /// <summary>
        /// Set a monthly operational limit.
        /// </summary>
        public virtual void setOprMax(int index, string oprMax)
        {
            if (!string.ReferenceEquals(oprMax, null))
            {
                setOprMax(index, double.Parse(oprMax.Trim()));
            }
        }

        /// <summary>
        /// Set qdebt
        /// </summary>
        public virtual void setQdebt(double qdebt)
        {
            if (qdebt != _qdebt)
            {
                _qdebt = qdebt;
                setDirty(true);
                if (Message.isDebugOn)
                {
                    Message.printDebug(1, "", "Setting qdebt dirty");
                }
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
                }
            }
        }

        /// <summary>
        /// Set qdebt
        /// </summary>
        public virtual void setQdebt(double? qdebt)
        {
            setQdebt(qdebt.Value);
        }

        /// <summary>
        /// Set qdebt
        /// </summary>
        public virtual void setQdebt(string qdebt)
        {
            if (!string.ReferenceEquals(qdebt, null))
            {
                setQdebt(double.Parse(qdebt.Trim()));
            }
        }

        /// <summary>
        /// Set qdebtx
        /// </summary>
        public virtual void setQdebtx(double qdebtx)
        {
            if (qdebtx != _qdebtx)
            {
                _qdebtx = qdebtx;
                setDirty(true);
                if (Message.isDebugOn)
                {
                    Message.printDebug(1, "", "Setting qdebtx dirty");
                }
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
                }
            }
        }

        /// <summary>
        /// Set qdebtx
        /// </summary>
        public virtual void setQdebtx(double? qdebtx)
        {
            setQdebtx(qdebtx.Value);
        }

        /// <summary>
        /// Set qdebtx
        /// </summary>
        public virtual void setQdebtx(string qdebtx)
        {
            if (!string.ReferenceEquals(qdebtx, null))
            {
                setQdebtx(double.Parse(qdebtx.Trim()));
            }
        }

        /// <summary>
        /// Set the operating rule strings, when read as text because an unknown right type.
        /// </summary>
        public virtual void setRightStrings(IList<string> rightStringList)
        {
            bool dirty = false;
            int size = rightStringList.Count;
            IList<string> rightStringList0 = getRightStrings();
            if (size != rightStringList0.Count)
            {
                dirty = true;
                if (Message.isDebugOn)
                {
                    Message.printDebug(1, "", "rightStringList old size=" + rightStringList0.Count + " new size=" + size);
                }
            }
            else
            {
                // Lists are the same size and there may not have been any changes
                // Need to check each string in the data
                for (int i = 0; i < size; i++)
                {
                    if (!rightStringList[i].Equals(rightStringList0[i]))
                    {
                        dirty = true;
                        if (Message.isDebugOn)
                        {
                            Message.printDebug(1, "", "commentsBeforeData old string \"" + rightStringList0[i] + "\" is different from new string \"" + rightStringList[i] + "\"");
                        }
                        break;
                    }
                }
            }
            if (dirty)
            {
                // Something was different so set the strings and change the dirty flag
                __rightStringsList = rightStringList;
                setDirty(true);
                if (Message.isDebugOn)
                {
                    Message.printDebug(1, "", "Setting rightStringList dirty");
                }
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
                }
            }
        }

        /// <summary>
        /// Set rtem
        /// </summary>
        public virtual void setRtem(string rtem)
        {
            if (!_rtem.Equals(rtem))
            {
                _rtem = rtem;
                setDirty(true);
                if (Message.isDebugOn)
                {
                    Message.printDebug(1, "", "Setting rtem dirty");
                }
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
                }
            }
        }

        /// <summary>
        /// Set sjmina
        /// </summary>
        public virtual void setSjmina(double sjmina)
        {
            if (sjmina != _sjmina)
            {
                _sjmina = sjmina;
                setDirty(true);
                if (Message.isDebugOn)
                {
                    Message.printDebug(1, "", "Setting sjmina dirty");
                }
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
                }
            }
        }

        /// <summary>
        /// Set sjmina
        /// </summary>
        public virtual void setSjmina(double? sjmina)
        {
            setSjmina(sjmina.Value);
        }

        /// <summary>
        /// Set sjmina
        /// </summary>
        public virtual void setSjmina(string sjmina)
        {
            if (!string.ReferenceEquals(sjmina, null))
            {
                setSjmina(double.Parse(sjmina.Trim()));
            }
        }

        /// <summary>
        /// Set sjrela
        /// </summary>
        public virtual void setSjrela(double sjrela)
        {
            if (sjrela != _sjrela)
            {
                _sjrela = sjrela;
                setDirty(true);
                if (Message.isDebugOn)
                {
                    Message.printDebug(1, "", "Setting sjrela dirty");
                }
                if (!_isClone && _dataset != null)
                {
                    _dataset.setDirty(StateMod_DataSet.COMP_OPERATION_RIGHTS, true);
                }
            }
        }

        /// <summary>
        /// Set sjrela
        /// </summary>
        public virtual void setSjrela(double? sjrela)
        {
            setSjrela(sjrela.Value);
        }

        /// <summary>
        /// Set sjrela
        /// </summary>
        public virtual void setSjrela(string sjrela)
        {
            if (!string.ReferenceEquals(sjrela, null))
            {
                setSjrela(double.Parse(sjrela.Trim()));
            }
        }


        public virtual void setupImonsw()
        {
            _imonsw = new int[12];
            for (int i = 0; i < 12; i++)
            {
                _imonsw[i] = 0;
            }
        }
    }
}
