using System;
using System.Collections.Generic;

// StateMod_PrfGageData - proration factor information, used by StateDMI when
// processing StateMod_StreamEstimate_Coefficients

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

// ----------------------------------------------------------------------------
// StateMod_PrfGageData - proration factor information, used by StateDMI when
//				processing StateMod_StreamEstimate_Coefficients
// ----------------------------------------------------------------------------
// History:
//
// 11 Feb 1999	Steven A. Malers	Code sweep.
//		Riverside Technology,
//		inc.
// ----------------------------------------------------------------------------
// 2003-10-08	J. Thomas Sapienza, RTi	Upgraded to HydroBaseDMI.
// 2004-02-04	JTS, RTi		Now extends DMIDataObject.
// 2004-08-13	SAM, RTi		* Move from HydroBaseDMI package to
//					  StateMod to support more specific use.
//					* Move the isSetprfSource() and
//					  isSetprfTarget() methods from
//					  HydroBase_NodeNetwork to static
//					  methods here.  Keep the method names
//					  the same for now but might rename
//					  later as code is cleaned up further.
// ----------------------------------------------------------------------------

namespace DWR.StateMod
{

	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// This class stores proration factor data for use by StateDMI when processing
	/// StateMod_StreamEstimate_Coeffients data.
	/// </summary>
	public class StateMod_PrfGageData : StateMod_Data
	{

	/// <summary>
	/// Name gage to use for data.
	/// </summary>
	private string __gageID = StateMod_Data.MISSING_STRING;

	/// <summary>
	/// Name ID of node to reset.
	/// </summary>
	private string __nodeID = StateMod_Data.MISSING_STRING;

	/// <summary>
	/// Pointer to gage node, cast as HydroBase_Node, when processed by
	/// HydroBase_NodeNetwork code.
	/// </summary>
	private object __gageNode = null;

	/// <summary>
	/// Pointer to node, cast as HydroBase_Node, when processed by
	/// HydroBase_NodeNetwork code.
	/// </summary>
	private object __node = null;

	/// <summary>
	/// Construct and initialize with empty identifiers, and null nodes.
	/// </summary>
	public StateMod_PrfGageData()
	{
		__nodeID = "";
		setID("");
		__gageID = "";
		__node = null;
		__gageNode = null;
	}

	/// <summary>
	/// Construct and initialize with empty identifiers, and null nodes. </summary>
	/// <param name="nodeID"> Identifier for the node. </param>
	/// <param name="gageID"> Identifier for the gage to supply data. </param>
	public StateMod_PrfGageData(string nodeID, string gageID)
	{
		__nodeID = nodeID;
		setID(nodeID);
		__gageID = gageID;
		__node = null;
		__gageNode = null;
	}

	/// <summary>
	/// Finalize before garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~StateMod_PrfGageData()
	{
		__nodeID = null;
		__gageID = null;
		__node = null;
		__gageNode = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Returns the gage identifier. </summary>
	/// <returns> the gage identifier. </returns>
	public virtual string getGageID()
	{
		return __gageID;
	}

	/// <summary>
	/// Returns the gage node. </summary>
	/// <returns> the gage node. </returns>
	public virtual object getGageNode()
	{
		return __gageNode;
	}

	/// <summary>
	/// Returns the node. </summary>
	/// <returns> the node. </returns>
	public virtual object getNode()
	{
		return __node;
	}

	/// <returns> the node identifier. </returns>
	public virtual string getNodeID()
	{
		return __nodeID;
	}

	/// <summary>
	/// Checks whether the node is a gage that is to supply proration data. </summary>
	/// <param name="id"> the identifier for the station/node to check. </param>
	/// <param name="prfGageData"> vector of prf gage data </param>
	/// <returns> true if the node is gage that supplies proration data, false if not. </returns>
	public static bool isSetprfSource(string id, IList<StateMod_PrfGageData> prfGageData)
	{
		string routine = "StateMod_PrfGageData.isSetprfSource";
		int dl = 10;

		int numPrfGageData = 0;
		if (prfGageData != null)
		{
			numPrfGageData = prfGageData.Count;
		}
		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "Looking through " + numPrfGageData + " prfGageData's for " + id);
		}

		StateMod_PrfGageData prfGageData_j;
		for (int j = 0; j < numPrfGageData; j++)
		{
			prfGageData_j = prfGageData[j];
			if (prfGageData_j.getGageID().Equals(id, StringComparison.OrdinalIgnoreCase))
			{
				if (Message.isDebugOn)
				{
					Message.printDebug(dl, routine, "Found a prfGageData gage \"" + id + "\".");
				}
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Checks to see if the requested node is the target of a "set proration factor gage" (see StateDMI
	/// SetStreamEstimateCoefficientsPFGage() command). </summary>
	/// <param name="commonID"> the id of the node to check </param>
	/// <param name="prfGageData"> list of proration gage data </param>
	/// <returns> the index of in the PrfGageData that has a node ID that matches the specified commonID, or -1 if
	/// the node wasn't found. </returns>
	public static int isSetprfTarget(string commonID, IList<StateMod_PrfGageData> prfGageData)
	{
		string routine = "StateMod_NodeNetwork.isSetprfTarget";
		int dl = 10;

		int numPrfGageData = 0;
		if (prfGageData != null)
		{
			numPrfGageData = prfGageData.Count;
		}
		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "Looking through " + numPrfGageData + " prfGageData's for " + commonID);
		}

		StateMod_PrfGageData prfGageData_j = null;
		for (int j = 0; j < numPrfGageData; j++)
		{
			prfGageData_j = prfGageData[j];
			if (prfGageData_j.getNodeID().Equals(commonID, StringComparison.OrdinalIgnoreCase))
			{
				if (Message.isDebugOn)
				{
					Message.printDebug(dl, routine, "Found a prfGageData target structure \"" + commonID + "\".");
				}
				return j;
			}
		}
		return -1;
	}

	/// <summary>
	/// Set the gage identifier. </summary>
	/// <param name="gageid"> Gage identifier. </param>
	public virtual void setGageID(string gageid)
	{
		if (!string.ReferenceEquals(gageid, null))
		{
			__gageID = gageid;
		}
	}

	/// <summary>
	/// Set the gage node. </summary>
	/// <param name="gagenode"> Gage node. </param>
	public virtual void setGageNode(object gagenode)
	{
		__gageNode = gagenode;
	}

	/// <summary>
	/// Set the node. </summary>
	/// <param name="node"> Node. </param>
	public virtual void setNode(object node)
	{
		__node = node;
	}

	/// <summary>
	/// Set the node identifier. </summary>
	/// <param name="nodeid"> Node identifier. </param>
	public virtual void setNodeID(string nodeid)
	{
		if (!string.ReferenceEquals(nodeid, null))
		{
			__nodeID = nodeid;
			setID(nodeid);
		}
	}

	}

}