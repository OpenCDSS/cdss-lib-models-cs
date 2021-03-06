﻿// StateMod_GUIUpdatable - interface to allow the StateMod_DataSet_WindowManager
// to call StateModGUI_JFrame.updateWindowStatus() 

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
// StateMod_GUIUpdatable - interface to allow the StateMod_DataSet_WindowManager
//				to call StateModGUI_JFrame.updateWindowStatus() 
//				without StateModGUI_JFrame being known to the
//				StateMod package.
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
// 
// 2003-10-14	Steven A. Malers, RTi	Created interface.
//------------------------------------------------------------------------------

namespace DWR.StateMod
{
	public interface StateMod_GUIUpdatable
	{

	/// <summary>
	/// When called, this method will trigger the window to refresh its state, including
	/// the title bar and menu states, based on current conditions of the StateMod
	/// data set.
	/// </summary>
	void updateWindowStatus();

	}

}