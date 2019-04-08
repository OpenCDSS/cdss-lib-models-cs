// StateMod_RiverStation_Node - RiverStation node for the network

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
// StateMod_RiverStation_Node - RiverStation node for the network
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
// 
// 2003-07-30	Steven A. Malers, RTi	Initial version.
// 2003-09-11	SAM, RTi		Rename class from
//					StateMod_RiverStation_Node to
//					StateMod_StreamGage_Node.
//------------------------------------------------------------------------------

namespace DWR.StateMod
{
	using Node = RTi.Util.IO.Node;

	/// <summary>
	/// Network object for StateMod StreamGage node.
	/// </summary>
	public class StateMod_StreamGage_Node : Node
	{

	/// <summary>
	/// Constructor.
	/// </summary>
	public StateMod_StreamGage_Node(string id, string name) : base(id, name, "StreamGage")
	{
	}

	} // End StateMod_StreamGage_Node

}