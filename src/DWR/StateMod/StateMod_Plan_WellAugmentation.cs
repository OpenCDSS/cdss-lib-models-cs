using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

// StateMod_Plan_WellAugmentation - This class stores Plan (Well Augmentation) data.

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

    using IOUtil = RTi.Util.IO.IOUtil;
    using Message = RTi.Util.Message.Message;
    using StringUtil = RTi.Util.String.StringUtil;

    /// <summary>
    /// This class stores Plan (Well Augmentation) data.  The plan ID is stored in the StateMod_Data ID.
    /// </summary>
    public class StateMod_Plan_WellAugmentation : StateMod_Data //, ICloneable, IComparable<StateMod_Data>
    {

        /// <summary>
        /// Well right ID.
        /// </summary>
        private string __cistatW;

        /// <summary>
        /// Well structure ID.
        /// </summary>
        private string __cistatS;

        /// <summary>
        /// Construct an instance.
        /// </summary>
        public StateMod_Plan_WellAugmentation() : base()
        {
            initialize();
        }

        private void initialize()
        {
            _smdata_type = StateMod_DataSet.COMP_PLAN_WELL_AUGMENTATION;
            __cistatW = "";
            __cistatS = "";
        }

        /// <summary>
        /// Read return information in and store in a list. </summary>
        /// <param name="filename"> filename for data file to read </param>
        /// <exception cref="Exception"> if an error occurs </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public static java.util.List<StateMod_Plan_WellAugmentation> readStateModFile(String filename) throws Exception
        public static IList<StateMod_Plan_WellAugmentation> readStateModFile(string filename)
        {
            string routine = "StateMod_Plan_WellAugmentation.readStateModFile";
            string iline = null;
            IList<string> v = new List<string>(9);
            IList<StateMod_Plan_WellAugmentation> theWellAugs = new List<StateMod_Plan_WellAugmentation>();
            int linecount = 0;

            StateMod_Plan_WellAugmentation aWellAug = null;
            StreamReader @in = null;

            Message.printStatus(2, routine, "Reading well augmentation plan file: " + filename);
            int size = 0;
            int errorCount = 0;
            try
            {
                @in = new StreamReader(IOUtil.getPathUsingWorkingDir(filename));
                while (!string.ReferenceEquals((iline = @in.ReadLine()), null))
                {
                    ++linecount;
                    // check for comments
                    if (iline.StartsWith("#", StringComparison.Ordinal) || (iline.Trim().Length == 0))
                    {
                        // Special dynamic header comments written by software and blank lines - no need to keep
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
                    if (size < 3)
                    {
                        Message.printStatus(2, routine, "Ignoring line " + linecount + " not enough data values.  Have " + size + " expecting 3");
                        ++errorCount;
                        continue;
                    }
                    // Uncomment if testing...
                    //Message.printStatus ( 2, routine, "" + v );

                    // Allocate new plan node and set the values
                    aWellAug = new StateMod_Plan_WellAugmentation();
                    aWellAug.setID(v[0].Trim());
                    aWellAug.setName(v[0].Trim()); // Same as ID
                    aWellAug.setCistatW(v[1].Trim());
                    aWellAug.setCistatS(v[2].Trim());
                    if (v.Count > 3)
                    {
                        aWellAug.setComment(v[3].Trim());
                    }

                    // Set the return to not dirty because it was just initialized...

                    aWellAug.setDirty(false);

                    // Add the return to the list of returns
                    theWellAugs.Add(aWellAug);
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
            return theWellAugs;
        }


        /// <summary>
        /// Set the cistatS.
        /// </summary>
        public virtual void setCistatS(string cistatS)
        {
            if (!string.ReferenceEquals(cistatS, null))
            {
                if (!cistatS.Equals(__cistatS))
                {
                    setDirty(true);
                    if (!_isClone && _dataset != null)
                    {
                        _dataset.setDirty(_smdata_type, true);
                    }
                    __cistatS = cistatS;
                }
            }
        }

        /// <summary>
        /// Set the cistatW.
        /// </summary>
        public virtual void setCistatW(string cistatW)
        {
            if (!string.ReferenceEquals(cistatW, null))
            {
                if (!cistatW.Equals(__cistatW))
                {
                    setDirty(true);
                    if (!_isClone && _dataset != null)
                    {
                        _dataset.setDirty(_smdata_type, true);
                    }
                    __cistatW = cistatW;
                }
            }
        }
    }
}
