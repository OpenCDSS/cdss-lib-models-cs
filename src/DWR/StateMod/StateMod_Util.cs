using System;
using System.Collections.Generic;
using System.Text;

// StateMod_Util - Utility functions for StateMod operation

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
// StateMod_Util - Utility functions for StateMod operation
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
//
// 2003-07-02	J. Thomas Sapienza, RTi	Initial version.
// 2003-07-30	Steven A. Malers, RTi	* Remove import for
//					  StateMod_DataSetComponnent.
//					* Remove static __basinName, which is
//					  the response file name without the
//					  .rsp.  StateMod now can take the name
//					  with or without the .rsp so just pass
//					  the response file name to the
//					  runStateMod() method.
//					* Change runStateModOption() to
//					  runStateMod() and pass the data set
//					  to the method.
//					* Make __statemod_version and
//					  __statemod_executable private and
//					  add set/get methods.
//					* Move remaining static methods from
//					  StateMod_Data.
//					* Alphabetize methods.
// 2003-08-21	SAM, RTi		* Add lookupTimeSeries() to simplify
//					  finding time series for components.
//					* Add createDataList() to help with
//					  choices, etc.
// 2003-08-25	SAM, RTi		Add getUpstreamNetworkNodes() from
//					old SMRiverInfo.retrieveUpstreams().
//					Change it to return data objects, not
//					strings.
// 2003-09-11	SAM, RTi		Update due to changes in the river
//					station component names.
// 2003-09-19	JTS, RTi		Added createCgotoDataList().
// 2003-09-24	SAM, RTi		* Change findEarliestPOR() to
//					  findEarliestDateInPOR().
// 					* Change findLatestPOR() to
//					  findLatestDateInPOR().
//					* Change the above methods to return
//					  null if no date can be found (e.g.,
//					  for a new data set).
// 2003-09-29	SAM, RTi		Add formatDataLabel().
// 2003-10-09	JTS, RTi		* Added removeFromVector().
//					* Added sortStateMod_DataVector().
// 2003-10-10	SAM, RTi		Add estimateDayTS ().
// 2003-10-24	SAM, RTi		Overload runStateMod() to take a 
//					StateMod_DataSet, so the response file
//					can be determined.
// 2003-10-29	SAM, RTi		* Change estimateDailyTS() to
//					  createDailyEstimateTS().
//					* Add createWaterRightTS().
// 2003-11-03	SAM, RTi		Change From_Well parameter to
//					From_River_By_Well.
// 2003-11-05	SAM, RTi		Got clarification from Ray Bennett on
//					which parameters should be listed for
//					output.
// 2003-11-14	SAM, RTi		Ray Bennett provided documentation for
//					the reservoir and well monthly binary
//					files as well as all the daily binary
//					files.  Therefore update the data type
//					lists, etc.
// 2003-11-29	SAM, RTi		In getTimeSeriesDataTypes(),
//					automatically turn off input types if
//					the request is for reservoirs and
//					the identifier has an account part.
// 2004-06-01	SAM, RTi		Update getTimeSeriesDataTypes() to have
//					a flag for data groups and use Ray
//					Bennett feedback for the groups.
// 2004-07-02	SAM, RTi		Add indexOfRiverNodeID().
// 2004-07-06	SAM, RTi		Overload sortStateMod_DataVector() to
//					allow option of creating new or using
//					existing data Vector.
// 2004-08-12	JTS, RTi		Added calculateTimeSeriesDifference().
// 2004-08-25	JTS, RTi		Removed the property that defined a
//					"HelpKey" for the dialog that runs 
//					StateMod.
// 2004-09-07	SAM, RTi		* Reordered some methods to be
//					  alphabetical.
//					* Add findWaterRightInsertPosition().
// 2004-09-14	SAM, RTi		For findWaterRightInsertPosition(), just
//					insert based on the right ID.
// 2004-10-05	SAM, RTi		* Add data type notes as per recent
//					  documentation (? are removed).
//					* Add River_Outflow for reservoir
//					  station output parameters.
// 2005-03-03	SAM, RTi		* Add compareFiles() to help with
//					  testing.
// 2005-04-01	SAM, RTi		* Add createTotalTimeSeries() method to
//					  facilitate summarizing information.
// 2005-04-05	SAM, RTi		* Add lookupTimeSeriesGraphTitle() to
//					  provide default titles based on the
//					  component type.
// 2005-04-18	JTS, RTi		Added the lookup methods.
// 2005-04-19	JTS, RTi		Removed testDirty().
// 2005-05-06	SAM, RTi		Correct a couple of typos in reservoir
//					subcomponent IDs in lookupPropValue().
// 2005-08-30	SAM, RTi		Add getTimeSeriesOutputPrecision().
// 2005-10-05	SAM, RTi		Handle well historical pumping time
//					series in createTotalTS().
// 2005-12-20	SAM, RTi		Add VERSION_XXX and isVersionAtLeast()
//					to help with binary file format
//					versions.
// 2006-01-15	SAM, RTi		Overload getTimeSeriesDataTypes() to
//					take the file name, to facilitate
//					reading the parameters from the newer
//					binary files.
// 2006-03-05	SAM, RTi		calculateTimeSeriesDifference() was
//					resulting in a division by zero, with
//					infinity values being returned.
// 2006-04-10	SAM, RTi		Add getRightsForStation(), which
//					extracts rights for an identifier.
// 2006-06-13	SAM, RTi		Add properties for downstream ID for
//					river network file.
// 2006-08-20	SAM, RTi		Move code to check for edits before
//					running to StateModGUI_JFrame.
// 2007-04-15	Kurt Tometich, RTi		Added some helper methods that
//								return validators for data checks.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
//------------------------------------------------------------------------------
// EndHeader

namespace DWR.StateMod
{
    public class StateMod_Util
    {

        public static string MISSING_STRING = "";
        public static int MISSING_INT = -999;
        public static float MISSING_FLOAT = (float)-999.0;
        public static double MISSING_DOUBLE = -999.0;
        private static double MISSING_DOUBLE_FLOOR = -999.1;
        private static double MISSING_DOUBLE_CEILING = -998.9;
        //public static DateTime MISSING_DATE = null;

        /// <summary>
        /// Determine whether an integer value is missing. </summary>
        /// <param name="i"> Integer value to check. </param>
        /// <returns> true if the value is missing, false, if not. </returns>
        public static bool isMissing(int i)
        {
            if (i == MISSING_INT)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Find the position of a StateMod_Data object in the data Vector, using the
        /// identifier.  The position for the first match is returned. </summary>
        /// <returns> the position, or -1 if not found. </returns>
        /// <param name="id"> StateMod_Data identifier. </param>
        public static int indexOf(System.Collections.IList data, string id)
        {
            int size = 0;
            if (string.ReferenceEquals(id, null))
            {
                return -1;
            }
            if (data != null)
            {
                size = data.Count;
            }
            StateMod_Data d = null;
            for (int i = 0; i < size; i++)
            {
                d = (StateMod_Data)data[i];
                if (id.Equals(d._id, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }
            return -1;
        }

    }
}
