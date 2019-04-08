using System;
using System.Collections.Generic;
using System.Threading;

// StateMod_GUIUtil - GUI-related utility functions

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
// StateMod_GUIUtil - GUI-related utility functions
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
//
// 2003-06-09	J. Thomas Sapienza, RTi	Initial version.
// 2003-07-07	JTS, RTi		Added code for displaying graphs.
// 2003-07-17	JTS, RTi		Added nothingSelected() and 
//					somethingSelected()
// 2003-08-03	Steven A. Malers, RTi	* Constants that were in
//					  StateMod_Control are now in
//					  StateMod_DataSet.
//					* Add READ_START for process listener.
// 2003-08-13	SAM, RTi		* Change ProcessListeners to be more
//					  consistent with StateDMI command
//					  processing and verify that process
//					  listener is working.
//					* Remove map layers window.
//					* Handle basin summary like other
//					  windows.
//					* Remove diagnostics window from the
//					  list of managed windows - it manages
//					  itself.
//					* Clean up the names of the windows to
//					  be consistent with data set
//					  components.
// 2003-08-26	SAM, RTi		Split window manager code into
//					StateMod_DataSet_WindowManager and
//					remove from here.
// 2003-09-24	SAM, RTi		Overload setTitle() to take a JDialog.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
//------------------------------------------------------------------------------

namespace DWR.StateMod
{
    public class StateMod_GUIUtil
    {
        /// <summary>
        /// Settings for use in relaying data back to the calling application via ProcessListener calls.
        /// </summary>
        public const int STATUS_READ_START = 20, STATUS_READ_COMPLETE = 22, STATUS_READ_GVP_START = 50, STATUS_READ_GVP_END = 51; // End reading the GVP file;
    }
}
