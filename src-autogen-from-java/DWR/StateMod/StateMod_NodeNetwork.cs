using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

// StateMod_NodeNetwork - data used when reading the old Makenet file.

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

	// Before 2017-07-01 used Xerces but Java is now distributed with embedded Xerces
	//import org.apache.xerces.parsers.DOMParser;
	using Document = org.w3c.dom.Document;
	using NamedNodeMap = org.w3c.dom.NamedNodeMap;
	using Node = org.w3c.dom.Node;
	using NodeList = org.w3c.dom.NodeList;

	using DOMParser = com.sun.org.apache.xerces.@internal.parsers.DOMParser;

	using DMIUtil = RTi.DMI.DMIUtil;
	using GRLimits = RTi.GR.GRLimits;
	using GRText = RTi.GR.GRText;
	using PropList = RTi.Util.IO.PropList;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;
	using StopWatch = RTi.Util.Time.StopWatch;

	using HydrologyNode = cdss.domain.hydrology.network.HydrologyNode;
	using HydrologyNodeNetwork = cdss.domain.hydrology.network.HydrologyNodeNetwork;
	using RiverLine = cdss.domain.hydrology.network.RiverLine;

	public class StateMod_NodeNetwork : HydrologyNodeNetwork
	{
	/// <summary>
	/// Data used when reading the old Makenet file.
	/// FIXME SAM 2008-03-15 Need to evaluate whether this can be in read code only.
	/// </summary>

	/// <summary>
	/// Counter for the number of reaches closed for processing (to count matches for __openCount).
	/// </summary>
	private int __closeCount;

	/// <summary>
	/// The counter for the line in the input file.  
	/// </summary>
	private int __line;

	/// <summary>
	/// The number of reaches opened for processing.
	/// </summary>
	private int __openCount;

	/// <summary>
	/// The number of reaches processed.
	/// </summary>
	private int __reachCounter;

	// End Makenet data

	/// <summary>
	/// Whether to generate fancy node descriptions or not.
	/// </summary>
	private bool __createFancyDescription;

	/// <summary>
	/// Whether to create output files or not.
	/// </summary>
	private bool __createOutputFiles;

	/// <summary>
	/// Construct a StateMod_NodeNetwork but do not add an end node.
	/// </summary>
	public StateMod_NodeNetwork() : this(false)
	{
	}

	/// <summary>
	/// Construct a StateMod_NodeNetwork. </summary>
	/// <param name="addEndNode"> if true an end node will automatically be added at initialization. </param>
	public StateMod_NodeNetwork(bool addEndNode) : base(addEndNode)
	{
		initialize();
	}

	/// <summary>
	/// Append a network to this network.  The process taken is to reduce the data to a list of nodes and then
	/// recalculate the node connectivity.  This uses data and methods that mix HydrologyNode and ID representations
	/// of the network since it is a mix of processing raw data from existing networks. </summary>
	/// <param name="networkToAppend"> a network to append to this existing network </param>
	/// <param name="appendEndAs"> the identifier for a node in the current network, which will become the downstream
	/// node for the appended network </param>
	public virtual StateMod_NodeNetwork append(StateMod_NodeNetwork networkToAppend, StateMod_NodeNetwork_AppendHowType appendHowType, string existingDownstreamNodeID, string appendedUpstreamNodeID, double? scaleXY, double? shiftX, double? shiftY)
	{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		string routine = this.GetType().FullName + ".append";
		// The network will already have been read in and cleaned up.  Therefore just extract the
		// nodes and replace the end node with the node matching the "appendEndAs" identifier, which must
		// exist in the current network.

		// Find the new end node.
		HydrologyNode existingDownstreamNode = findNode(existingDownstreamNodeID);
		if (existingDownstreamNode == null)
		{
			throw new Exception("Unable to match node \"" + existingDownstreamNodeID + "\" in existing network to serve as downstream node for appended network.");
		}
		// Find the starting node on the network to be appended.
		HydrologyNode appendedUpstreamNode = networkToAppend.findNode(appendedUpstreamNodeID);
		if (appendedUpstreamNode == null)
		{
			throw new Exception("Unable to find appended upstream node \"" + appendedUpstreamNodeID + "\".");
		}
		// Verify that the existing node does not also exist in the other network.
		/*
		HydrologyNode endNode = networkToAppend.getNodeHead();
		if ( endNode == null ) {
			throw new RuntimeException ( "Unable to get downstream (end) node for appended network.");
		}
		*/

		// The following logic is similar to that in the readXMLNetworkFile() method.  Basically the lists of
		// nodes, labels, etc., are combined into one big list and then the internal network is reconstructed
		// to assign network navigation information.  The items being appended are also repositioned to align
		// with the joined network.  Finally, overall limits are reset.

		// Get the original node lists...

		IList<HydrologyNode> networkNodeList = this.getNodeList(); // List of all nodes read
		IList<PropList> networkLinkList = this.getLinkList(); // List of all links read (lines from one node to another)
		IList<PropList> networkLayoutList = this.getLayoutList(); // List of all layouts
		IList<HydrologyNode> networkAnnotationList = this.getAnnotationList(); // All annotations.

		// Get the lists to append...

		IList<HydrologyNode> appendNetworkNodeList = networkToAppend.getNodeList();
		IList<PropList> appendNetworkLinkList = networkToAppend.getLinkList();
		IList<PropList> appendNetworkLayoutList = networkToAppend.getLayoutList();
		IList<HydrologyNode> appendNetworkAnnotationList = networkToAppend.getAnnotationList();
		// Get the list of confluences, which have internally assigned identifiers like "CONFL_1"
		// (although the first one may just be "CONFL"
		// It is likely that there are duplicate identifiers so find the maximum confluence ID number
		// and change all the confluences in the network being appended to continue the series.
		int[] types = new int[] {HydrologyNode.NODE_TYPE_CONFLUENCE};
		IList<string> conflIDList = new List<object>();
		try
		{
			conflIDList = getNodeIdentifiersByType(types);
		}
		catch (Exception e)
		{
			throw new Exception(e);
		}
		int max = 1;
		foreach (string conflID in conflIDList)
		{
			int pos = conflID.IndexOf("_", StringComparison.Ordinal);
			if (pos > 0)
			{
				max = Math.Max(max,int.Parse(conflID.Substring(pos + 1)));
			}
		}
		// Get the confluence nodes in the network to append...
		IList<string> conflIDAppendList = new List<object>();
		try
		{
			conflIDAppendList = networkToAppend.getNodeIdentifiersByType(types);
		}
		catch (Exception e)
		{
			throw new Exception(e);
		}
		conflIDAppendList.Sort();
		// Create a hash and define the translation
		Hashtable confIDLookup = new Hashtable();
		int conflNum = max + 1;
		Message.printStatus(2,routine, "Renumbering confluence nodes in appended network to start with CONFL_" + conflNum);
		foreach (string conflIDAppend in conflIDAppendList)
		{
			// Remove the number and replace with another
			int pos = conflIDAppend.IndexOf("_", StringComparison.Ordinal);
			if (pos < 0)
			{
				// CONFL
				confIDLookup[conflIDAppend] = conflIDAppend + "_" + conflNum;
			}
			else
			{
				// CONFL_N
				confIDLookup[conflIDAppend] = conflIDAppend.Substring(0,pos) + "_" + conflNum;
			}
			++conflNum;
		}
		// Finally, loop through all the nodes and replace the confluence identifiers with non-conflicting values
		string id;
		object oHashValue;
		foreach (HydrologyNode nodeToAppend in appendNetworkNodeList)
		{
			id = nodeToAppend.getCommonID();
			oHashValue = confIDLookup[id];
			if (oHashValue != null)
			{
				// Have a match...
				nodeToAppend.setCommonID((string)oHashValue);
			}
			id = nodeToAppend.getNetID(); // Legacy...
			oHashValue = confIDLookup[id];
			if (oHashValue != null)
			{
				// Have a match...
				nodeToAppend.setNetID((string)oHashValue);
			}
			// Also process the node upstream and downstream IDs, which are maintained as strings during the
			// network build process...
			// Upstream...
			string[] upstreamIDs = nodeToAppend.getUpstreamNodeIDs();
			if ((upstreamIDs != null) && (upstreamIDs.Length > 0))
			{
				nodeToAppend.clearUpstreamNodeIDs();
				for (int i = 0; i < upstreamIDs.Length; i++)
				{
					id = upstreamIDs[i];
					if (!string.ReferenceEquals(id, null))
					{
						oHashValue = confIDLookup[id];
						if (oHashValue != null)
						{
							// Have a match...
							nodeToAppend.addUpstreamNodeID((string)oHashValue);
						}
					}
				}
			}
			// Downstream...
			id = nodeToAppend.getDownstreamNodeID();
			if (!string.ReferenceEquals(id, null))
			{
				oHashValue = confIDLookup[id];
				if (oHashValue != null)
				{
					// Have a match...
					nodeToAppend.setDownstreamNodeID((string)oHashValue);
				}
			}
		}

		// Calculate the coordinate offset such that the end node would exactly overly the node that will
		// replace it.  The offset can be added to the coordinates in the new network.
		// This aligns the appended network such that the appended node is at coordinate (0,0) of its space,
		// and it will subsequently be scaled to match the scaling of the existing network.
		double shiftXAlignNetworks1 = -appendedUpstreamNode.getX();
		double shiftYAlignNetworks1 = -appendedUpstreamNode.getY();
		double shiftXAlignNetworks2 = existingDownstreamNode.getX();
		double shiftYAlignNetworks2 = existingDownstreamNode.getY();
		double scale = 1.0; // TODO SAM 2011-01-04 Compute from average node spacing?
		if (scaleXY != null)
		{
			scale = scaleXY.Value;
		}
		double shiftXAdditional = 0.0; // Additional offset needed to position whole block of new nodes
		double shiftYAdditional = 0.0;
		if (shiftX != null)
		{
			shiftXAdditional = shiftX.Value;
		}
		if (shiftY != null)
		{
			shiftYAdditional = shiftY.Value;
		}
		Message.printStatus(2, routine, "Shift to set coordinates of append node to zero = " + shiftXAlignNetworks1 + "," + shiftYAlignNetworks1);
		Message.printStatus(2, routine, "Scale to apply to appended network = " + scale);
		Message.printStatus(2, routine, "Shift to set coordinates of append point to downstream = " + shiftXAlignNetworks2 + "," + shiftYAlignNetworks2);
		Message.printStatus(2, routine, "Additional shift to apply to appended network = " + shiftXAdditional + "," + shiftYAdditional);

		// Adjust the merge point nodes

		if (appendHowType == StateMod_NodeNetwork_AppendHowType.ADD_UPSTREAM_OF_DOWNSTREAM)
		{
			// Remove all nodes downstream in the appended network.  Because this code will rebuild the
			// network below, just do this brute force without properly recalculating the navigation data.
			// Start with the node downstream of the requested node...
			append_RemoveNodesDownsteamOfAppendNode(routine, appendedUpstreamNode, appendNetworkNodeList);
			appendedUpstreamNode.setDownstreamNodeID(null);
			// Now reset the downstream node of the append point
			appendedUpstreamNode.setDownstreamNode(existingDownstreamNode);
			appendedUpstreamNode.setDownstreamNodeID(existingDownstreamNode.getCommonID());
			Message.printStatus(2,routine,"Setting append node \"" + appendedUpstreamNode.getCommonID() + "\" downstream to: " + existingDownstreamNode.getCommonID());
			if (appendedUpstreamNode.getType() == HydrologyNode.NODE_TYPE_END)
			{
				// Change to an "Other" node
				Message.printStatus(2,routine,"Changing append node type from End to Other");
				appendedUpstreamNode.setType(HydrologyNode.NODE_TYPE_OTHER);
			}
			Message.printStatus(2,routine,"Adding to existing downstream node \"" + existingDownstreamNode.getCommonID() + "\" the append node as upstream: " + appendedUpstreamNode.getCommonID());
			existingDownstreamNode.addUpstreamNode(appendedUpstreamNode);
			existingDownstreamNode.addUpstreamNodeID(appendedUpstreamNode.getCommonID());
		}
		else if (appendHowType == StateMod_NodeNetwork_AppendHowType.REPLACE_UPSTREAM_OF_DOWNSTREAM)
		{
			// Remove all nodes downstream of the appended network.  Because this code will rebuild
			// the network below, just do this brute force without properly recalculating the navigation data.
			// Start with the node downstream of the requested node...
			append_RemoveNodesDownsteamOfAppendNode(routine, appendedUpstreamNode, appendNetworkNodeList);
			appendedUpstreamNode.setDownstreamNodeID(null);
			// Remove all the nodes upstream of the downstream node, on the reach
			append_RemoveNodesUpstreamOfDownstreamNode(routine, existingDownstreamNode.getUpstreamNodes(), networkNodeList);
			// Also need to manually remove because the above removes from the node list but does not reset
			// the pointers on the node.
			int nUp = existingDownstreamNode.getNumUpstreamNodes();
			for (int i = (nUp - 1); i >= 0; i--)
			{
				existingDownstreamNode.removeUpstreamNode(i);
			}
			existingDownstreamNode.clearUpstreamNodeIDs();
			// Set the downstream node for the append point
			appendedUpstreamNode.setDownstreamNode(existingDownstreamNode);
			appendedUpstreamNode.setDownstreamNodeID(existingDownstreamNode.getCommonID());
			Message.printStatus(2,routine,"Setting append node \"" + appendedUpstreamNode.getCommonID() + "\" downstream=\"" + existingDownstreamNode.getCommonID() + "\".");
			if (appendedUpstreamNode.getType() == HydrologyNode.NODE_TYPE_END)
			{
				// Change to an "Other" node
				Message.printStatus(2,routine,"Changing append node type from End to Other");
				appendedUpstreamNode.setType(HydrologyNode.NODE_TYPE_OTHER);
			}
			Message.printStatus(2,routine,"Adding to existing downstream node \"" + existingDownstreamNode.getCommonID() + "\" the append node as upstream: " + appendedUpstreamNode.getCommonID());
			// All upstream nodes were removed above so the following should be the only one.
			existingDownstreamNode.addUpstreamNode(appendedUpstreamNode);
			existingDownstreamNode.addUpstreamNodeID(appendedUpstreamNode.getCommonID());
		}
		else
		{
			throw new InvalidParameterException("AppendHowType is not supported: " + appendHowType);
		}

		// Check for duplicate identifiers.  If not removed, they will cause lots of problems with infinite loops
		// in network navigation.

		StringBuilder b = new StringBuilder();
		double xOld, yOld;
		foreach (HydrologyNode nodeToAppend in appendNetworkNodeList)
		{
			// Adjust the coordinates of appended node and check for duplicates
			xOld = nodeToAppend.getX();
			yOld = nodeToAppend.getY();
			nodeToAppend.setX((xOld + shiftXAlignNetworks1) * scale + shiftXAlignNetworks2 + shiftXAdditional);
			nodeToAppend.setY((yOld + shiftYAlignNetworks1) * scale + shiftYAlignNetworks2 + shiftYAdditional);
			Message.printStatus(2,routine,"For appended node \"" + nodeToAppend.getCommonID() + "\" oldXY=" + xOld + "," + yOld + " newXY=" + nodeToAppend.getX() + "," + nodeToAppend.getY());
			foreach (HydrologyNode nodeToCheck in networkNodeList)
			{
				if (nodeToCheck.getCommonID().equalsIgnoreCase(nodeToAppend.getCommonID()))
				{
					b.Append(" " + nodeToCheck.getCommonID());
				}
			}
			// Add the node to the list.
			networkNodeList.Add(nodeToAppend);
		}
		if (b.Length > 0)
		{
			throw new Exception("Network being appended has nodes with identifiers in the existing network:  " + b);
		}
		Message.printStatus(2,routine,"Merged network has " + networkNodeList.Count + " nodes (from simple list merge).");
		// TODO SAM 2011-01-05 Evaluate whether duplicate labels, etc. are an issue

		// Now merge the nodes from each network.

		foreach (PropList linkToAppend in appendNetworkLinkList)
		{
			// Links are just two identifiers so no need to adjust coordinates.
			networkLinkList.Add(linkToAppend);
		}
		foreach (PropList layoutToAppend in appendNetworkLayoutList)
		{
			// For now use the layout from the original network
			// TODO SAM 2011-01-04 Any need to adjust any coordinates?  Check layout consistency?
			//networkLayoutList.add(layoutToAppend);
		}
		foreach (HydrologyNode annotationToAppend in appendNetworkAnnotationList)
		{
			// Adjust the coordinates and add
			annotationToAppend.setX((annotationToAppend.getX() + shiftXAlignNetworks1) * scale + shiftXAlignNetworks2 + shiftXAdditional);
			annotationToAppend.setY((annotationToAppend.getY() + shiftYAlignNetworks1) * scale + shiftYAlignNetworks2 + shiftYAdditional);
			networkAnnotationList.Add(annotationToAppend);
		}

		StateMod_NodeNetwork mergedNetwork = new StateMod_NodeNetwork();
		bool debug = Message.isDebugOn;
		if (debug)
		{
			try
			{
				HydrologyNodeNetwork.writeListFile(getInputName() + ".beforeCalc", null, false, networkNodeList, null, true);
			}
			catch (Exception e)
			{
				Message.printWarning(3,routine,e);
			}
		}
		mergedNetwork.calculateNetworkNodeData(networkNodeList, false); // False means upstream first
		Message.printStatus(2,routine,"Merged network has " + mergedNetwork.getNodeCount() + " nodes (from complete network).");
		if (debug)
		{
			try
			{
				HydrologyNodeNetwork.writeListFile(getInputName() + ".afterCalc", null, false, networkNodeList, null, true);
			}
			catch (Exception e)
			{
				Message.printWarning(3,routine,e);
			}
		}
		// FIXME SAM 2011-01-05 Do not know why but the above call sometimes results in duplicate
		// upstream nodes in the list.  Fix that here by removing the duplicate.  Don't have time right
		// now to track down the root issue
		HydrologyNode node2, node3;
		foreach (HydrologyNode node in networkNodeList)
		{
			IList<HydrologyNode> upstreamNodeList = node.getUpstreamNodes();
			if (upstreamNodeList != null)
			{
				// Loop through each node in the list...
				for (int i = 0; i < upstreamNodeList.Count; i++)
				{
					node2 = upstreamNodeList[i];
					// Loop through the remaining items in the list..
					for (int i1 = (i + 1); i1 < upstreamNodeList.Count; i1++)
					{
						node3 = upstreamNodeList[i1];
						if (node2 == node3)
						{
							upstreamNodeList.RemoveAt(i1);
							--i1; // To ensure next node will also be compared.
						}
					}
				}
			}
		}
		if (debug)
		{
			try
			{
				HydrologyNodeNetwork.writeListFile(getInputName() + ".afterRemoveDuplicates", null, false, networkNodeList, null, true);
			}
			catch (Exception e)
			{
				Message.printWarning(3,routine,e);
			}
		}
		mergedNetwork.setAnnotationList(networkAnnotationList);
		mergedNetwork.setLayoutList(networkLayoutList);
		mergedNetwork.setLinkList(networkLinkList);

		// For lack of a better option, get the new data extents for the bounds.
		// TODO SAM 2011-01-04 this really needs to consider the page size, labels, etc.,
		// but do the following for a first cut

		GRLimits networkDataLimits = mergedNetwork.determineExtentFromNetworkData();
		//GRLimits networkDataLimits = determineExtentFromNetworkData ( networkNodeList );
		Message.printStatus(2,routine,"Limits of merged network data=" + networkDataLimits);
		mergedNetwork.setBounds(networkDataLimits.getLeftX(), networkDataLimits.getBottomY(), networkDataLimits.getRightX(), networkDataLimits.getTopY());
		// Use the old legend position
		mergedNetwork.setLegendPosition(this.getLegendX(), this.getLegendY());

		mergedNetwork.convertNodeTypes();
		mergedNetwork.finalCheck(networkDataLimits.getLeftX(), networkDataLimits.getBottomY(), networkDataLimits.getRightX(), networkDataLimits.getTopY(), false);

		return mergedNetwork;
	}

	/// <summary>
	/// Helper method to remove nodes downstream of append node (local data from append() method).
	/// </summary>
	private void append_RemoveNodesDownsteamOfAppendNode(string routine, HydrologyNode appendedUpstreamNode, IList<HydrologyNode> appendNetworkNodeList)
	{
		HydrologyNode node = appendedUpstreamNode.getDownstreamNode();
		if (node != null)
		{
			// Not at the bottom of the network so remove downstream nodes.
			IList<HydrologyNode> downstreamNodesToRemoveList = new List<object>();
			// Move to the bottom of the network and save node references in a list...
			while (true)
			{
				// This node needs to be removed.
				downstreamNodesToRemoveList.Add(node);
				// Get the next downstream...
				node = node.getDownstreamNode();
				if (node == null)
				{
					break;
				}
			}
			// Now have a list of nodes to remove
			// Delete the nodes from the network being appended...
			foreach (HydrologyNode node2 in downstreamNodesToRemoveList)
			{
				Message.printStatus(2,routine,"Deleting the following downstream node in " + "append network before appending: " + node2.getCommonID());
				appendNetworkNodeList.Remove(node2);
			}
		}
	}

	/// <summary>
	/// Helper method to remove nodes upstream of the existing downstream node (local data from append() method).
	/// </summary>
	private void append_RemoveNodesUpstreamOfDownstreamNode(string routine, IList<HydrologyNode> upstreamNodeList, IList<HydrologyNode> networkNodeList)
	{
		// Follow each reach upstream
		foreach (HydrologyNode upstreamNode in upstreamNodeList)
		{
			IList<HydrologyNode> upstreamNodeList2 = upstreamNode.getUpstreamNodes();
			if (upstreamNodeList2 != null)
			{
				// Call recursively.  This should result in a march up the network and deletes of each node
				// as the recursive calls back down.
				append_RemoveNodesUpstreamOfDownstreamNode(routine, upstreamNodeList2, networkNodeList);
			}
			// Now remove this current node.
			Message.printStatus(2,routine,"Deleting node \"" + upstreamNode.getCommonID() + "\" that is upstream of \"" + upstreamNode.getDownstreamNode().getCommonID() + "\" in the existing network.");
				networkNodeList.Remove(upstreamNode);
		}
	}

	/// <summary>
	/// Creates a HydroBase_NodeNetwork from a list of StateMod_RiverNetworkNodes. </summary>
	/// <param name="nodes"> the nodes from which to create a HydroBase_NodeNetwork. </param>
	/// <returns> the HydroBase_NodeNetwork that was built.
	/// TODO (JTS - 2004-07-03) should not be a static returning a method, I think ... </returns>
	public static StateMod_NodeNetwork createFromStateModVector(IList<StateMod_RiverNetworkNode> nodes)
	{
		int size = nodes.Count;

		HydrologyNode[] nodeArray = new HydrologyNode[size];
		StateMod_RiverNetworkNode rnn = null;
		for (int i = size - 1; i >= 0; i--)
		{
			rnn = nodes[i];
			nodeArray[i] = new HydrologyNode();
			nodeArray[i].setCommonID(rnn.getID().Trim());
			nodeArray[i].setDownstreamNodeID(rnn.getCstadn().Trim());
			nodeArray[i].setType(HydrologyNode.NODE_TYPE_UNKNOWN);
		}
		nodeArray[size - 1].setType(HydrologyNode.NODE_TYPE_END);

		string dsid = null;
		for (int i = size - 1; i >= 0; i--)
		{
			dsid = nodeArray[i].getDownstreamNodeID();
			if (!string.ReferenceEquals(dsid, null))
			{
				for (int j = 0; j < size; j++)
				{
					if (nodeArray[j].getCommonID().Equals(dsid))
					{
						nodeArray[j].addUpstreamNodeID(nodeArray[i].getCommonID());
					}
				}
			}
		}

		IList<HydrologyNode> v = new List<object>();
		for (int i = 0; i < size; i++)
		{
			v.Add(nodeArray[i]);
		}

		StateMod_NodeNetwork network = new StateMod_NodeNetwork();
		network.calculateNetworkNodeData(v, false);
		return network;
	}

	/// <summary>
	/// Creates a StateMod_RiverNodeNetwork from the nodes in the HydroBase_NodeNetwork.
	/// The output contains only actual nodes.  Therefore, confluence nodes are skipped. </summary>
	/// <returns> the list of StateMod_RiverNetworkNode nodes. </returns>
	public virtual IList<StateMod_RiverNetworkNode> createStateModRiverNetwork()
	{
		bool done = false;
		HydrologyNode holdNode = null;
		HydrologyNode node = getMostUpstreamNode();
		HydrologyNode dsNode = null;
		StateMod_RiverNetworkNode rnn = null;
		IList<StateMod_RiverNetworkNode> v = new List<object>();
		int node_type; // Type for current node.
		int dsNode_type; // Type for downstream node.
		HydrologyNode node_downstream = null; // Used to find a real
		HydrologyNode real_node_downstream = null; // downstream node.
		// Create a blank node used for disappearing streams.  The identifiers will be empty strings...
		HydrologyNode blankNode = new HydrologyNode();
		blankNode.setDescription("SURFACE WATER LOSS");
		blankNode.setUserDescription("SURFACE WATER LOSS");
		blankNode.setType(HydrologyNode.NODE_TYPE_UNKNOWN);
		while (!done)
		{
			node_type = node.getType();
			// TODO SAM 2004-07-11 - the following may fail if no valid
			// downstream node is found - error handling below is not used.
			if ((node_type == HydrologyNode.NODE_TYPE_CONFLUENCE) || (node_type == HydrologyNode.NODE_TYPE_XCONFLUENCE) || (node_type == HydrologyNode.NODE_TYPE_BLANK))
			{
				node = getDownstreamNode(node, POSITION_COMPUTATIONAL);
				continue;
			}

			// Create a new instance and set the identifier...

			rnn = new StateMod_RiverNetworkNode();
			rnn.setID(node.getCommonID());

			// Set the node type.  This can be used later when filling with
			// HydroBase, to format the name (remove this code if that convention is phased out).
			//
			// This code is the same when reading the stream estimate stations command.
			if (node.getType() == HydrologyNode.NODE_TYPE_DIV)
			{
				rnn.setRelatedSMDataType(StateMod_DataSet.COMP_DIVERSION_STATIONS);
			}
			else if (node.getType() == HydrologyNode.NODE_TYPE_DIV_AND_WELL)
			{
				rnn.setRelatedSMDataType(StateMod_DataSet.COMP_DIVERSION_STATIONS);
				rnn.setRelatedSMDataType2(StateMod_DataSet.COMP_WELL_STATIONS);
			}
			else if (node.getType() == HydrologyNode.NODE_TYPE_ISF)
			{
				rnn.setRelatedSMDataType(StateMod_DataSet.COMP_INSTREAM_STATIONS);
			}
			else if (node.getType() == HydrologyNode.NODE_TYPE_PLAN)
			{
				rnn.setRelatedSMDataType(StateMod_DataSet.COMP_PLANS);
			}
			else if (node.getType() == HydrologyNode.NODE_TYPE_RES)
			{
				rnn.setRelatedSMDataType(StateMod_DataSet.COMP_RESERVOIR_STATIONS);
			}
			else if (node.getType() == HydrologyNode.NODE_TYPE_WELL)
			{
				rnn.setRelatedSMDataType(StateMod_DataSet.COMP_WELL_STATIONS);
			}

			// Set the downstream node information...

			dsNode = getDownstreamNode(node, POSITION_COMPUTATIONAL);

			// Taken from old createRiverNetworkFile() code...

			//Message.printStatus ( 2, "", "Processing node: " + node  );
			if (node.getDownstreamNode() != null)
			{
				dsNode_type = node.getDownstreamNode().getType();
				if ((dsNode_type == HydrologyNode.NODE_TYPE_BLANK) || (dsNode_type == HydrologyNode.NODE_TYPE_XCONFLUENCE) || (dsNode_type == HydrologyNode.NODE_TYPE_CONFLUENCE))
				{
					// We want to always show a real node in the river network...
					real_node_downstream = findNextRealDownstreamNode(node);
					node_downstream = findNextRealOrXConfluenceDownstreamNode(node);
					//Message.printStatus ( 2, "", "real ds node=" + real_node_downstream.getCommonID() +
					//" real or X ds node=" + node_downstream.getCommonID() );
					// If the downstream node in the reach is an XCONFLUENCE (as opposed to a tributary
					// coming in) then this is the last real node in disappearing stream.
					// Use a blank for the downstream node...
					//
					// Cannot simply check for the downstream node to be an XCONFL because incoming tributaries
					// may be joined to the main stem with an XCONFL
					//
					// There may be cases where multiple XCONFL nodes are in a row (e.g., to bend lines).  In
					// these cases, it is necessary to check the downstream reach node type, rather than
					// make sure that the node is the same as the immediate downstream node (as was done in
					// software prior to 2005-06-13).  This has not been implemented.  The user should make sure
					// that multiple XCONFL nodes are not included on a tributary reach that joins another stream.
					//
					if (node_downstream.getType() == HydrologyNode.NODE_TYPE_XCONFLUENCE)
					{
						// Get node to check...
						/* Use for debugging
						HydroBase_Node temp_node;
						temp_node = getDownstreamNode(node, POSITION_REACH);
						Message.printStatus ( 2, "", "reach end =" + temp_node );
						if ( temp_node != null ) {
							Message.printStatus ( 2, "", "reach end ds =" + temp_node.getDownstreamNode() );
						}
						*/
						if (node_downstream == getDownstreamNode(node,POSITION_REACH).getDownstreamNode())
						{
							// This identifies XCONFL nodes at the ends of tributary reaches and works
							// because the nodes are clearly identified on a reach...
							node_downstream = blankNode;
						}
						else if (node_downstream.getNumUpstreamNodes() == 1)
						{
							// For some reason someone put an XCONFL in a reach but did not split out
							// a tributary.  Therefore the "main stem" goes dry but is computationally
							// connected to the next downstream node...
							node_downstream = blankNode;
						}
						else
						{
							// Picking up on a confluence from another trib so use what normally would have...
							node_downstream = real_node_downstream;
						}
					}
					else
					{
						// Normal node...
						node_downstream = real_node_downstream;
					}
				}
				else
				{
					node_downstream = node.getDownstreamNode();
				}
				rnn.setCstadn(node_downstream.getCommonID());
			}

			// Add the node to the list...

			v.Add(rnn);

			if (node.getType() == HydrologyNode.NODE_TYPE_END)
			{
				done = true;
			}
			// TODO -- eliminate the need for hold nodes -- they signify an error in the network.
			else if (node == holdNode)
			{
				done = true;
			}
			holdNode = node;
			node = dsNode;
		}
		return v;
	}

	/// <summary>
	/// Gets the extent of the nodes in the network in the form of GRLimits, in network plotting coordinates
	/// (NOT alternative coordinates).  This is different from the HydrologyNodeNetwork.getExtents() in that
	/// it processes the raw nodes before they have been assimilated into a network.  This method may need to be
	/// called if the XMin, Ymin, XMax, Ymax data are corrupted in the network metadata (happened because of a
	/// bug that used alternative coordinates for computing these limits). </summary>
	/// <param name="nodeList"> list of nodes to be processed. </param>
	/// <returns> the GRLimits that represent the bounds of the nodes in the network, or null if no nodes have
	/// coordinates. </returns>
	public static GRLimits determineExtentFromNetworkData(IList<HydrologyNode> nodeList)
	{
		double lx = Double.NaN;
		double rx = Double.NaN;
		double by = Double.NaN;
		double ty = Double.NaN;

		foreach (HydrologyNode node in nodeList)
		{
			if (double.IsNaN(lx) || (node.getX() < lx))
			{
				lx = node.getX();
			}
			if (double.IsNaN(rx) || (node.getX() > rx))
			{
				rx = node.getX();
			}
			if (double.IsNaN(by) || (node.getY() < by))
			{
				by = node.getY();
			}
			if (double.IsNaN(ty) || (node.getY() > ty))
			{
				ty = node.getY();
			}
		}


		if (double.IsNaN(lx) || double.IsNaN(rx) || double.IsNaN(by) || double.IsNaN(ty))
		{
			// Do not have all parts of the data limits
			return null;
		}
		else
		{
			return new GRLimits(lx, by, rx, ty);
		}
	}

	/// <summary>
	/// Fills the locations of the nodes in the network, interpolating if necessary, 
	/// and looking up from the database if possible. </summary>
	/// <param name="nodeDataProvider"> the dmi to use for talking to the database.  Should be open and non-null. </param>
	/// <param name="interpolate"> whether node locations should be interpolated, or just
	/// looked up from the database. </param>
	/// <param name="limits"> if interpolating, the limits to use as the far bounds of the network. </param>
	public virtual void fillLocations(StateMod_NodeDataProvider nodeDataProvider, bool interpolate, GRLimits limits)
	{
		double lx, rx, by, ty;
		if (limits == null)
		{
			limits = determineExtentFromNetworkData();
		}

		lx = limits.getLeftX();
		by = limits.getBottomY();
		rx = limits.getRightX();
		ty = limits.getTopY();

		// TODO -- eliminate the need for hold nodes -- they signify an error in the network.
		HydrologyNode holdNode = null;
		HydrologyNode node = getMostUpstreamNode();
		bool done = false;
		double[] loc = null;
		while (!done)
		{
			loc = nodeDataProvider.lookupNodeLocation(node.getCommonID());
			if (DMIUtil.isMissing(node.getX()))
			{
				node.setX(loc[0]);
				node.setDBX(loc[0]);
			}

			if (DMIUtil.isMissing(node.getY()))
			{
				node.setY(loc[1]);
				node.setDBY(loc[1]);
			}

			if (node.getType() == HydrologyNode.NODE_TYPE_END)
			{
				done = true;
			}
			else if (node == holdNode)
			{
				done = true;
			}

			holdNode = node;
			node = getDownstreamNode(node, POSITION_COMPUTATIONAL);
		}

		if (!interpolate)
		{
			return;
		}

		setLx(lx);
		setBy(by);

		if ((rx - lx) > (ty - by))
		{
			setNodeSpacing((ty - by) * 0.06);
		}
		else
		{
			setNodeSpacing((rx - lx) * 0.06);
		}

		// Fills in any missing locations for all the nodes on the main stream stem.
		fillMainStemLocations();

		// Fills in missing locations for any node upstream of the main stem.
		fillUpstreamLocations();

		finalCheck(lx, by, rx, ty);
	}

	/// <summary>
	/// Finalize before garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~StateMod_NodeNetwork()
	{
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Initialize data.
	/// </summary>
	private void initialize()
	{
		__closeCount = 0;
		__createOutputFiles = true;
		__createFancyDescription = false;
		//In base... __fontSize = 10.0;
		//In base... __labelType = LABEL_NODES_NETID;
		//__legendDX = 1.0;
		//__legendDY = 1.0;
		//__legendX = 0.0;
		//__legendY = 0.0;
		__line = 1;
		//In base...__nodes = new Vector();
		//In base...__plotCommands = new Vector();
		//In base...__nodeCount = 1;
		//In base...__nodeDiam = 10.0;
		//In base...__nodeHead = null;
		__openCount = 0;
		__reachCounter = 0;
		//In base... __title = "Node Network";
		//In base...__titleX = 0.0;
		//In base...__titleY = 0.0;
		//In base... __treatDryAsBaseflow = false;
	}

	/// <summary>
	/// Processes node information from a makenet file. This version initializes the 
	/// counters properly and then calls the version that has the full argument list. </summary>
	/// <param name="netfp"> the BufferedReader to use for reading from the file. </param>
	/// <param name="filename"> the name of the file being read. </param>
	/// <returns> a node filled with data from the makenet file. </returns>
	public virtual HydrologyNode processMakenetNodes(StreamReader netfp, string filename)
	{
		return processMakenetNodes(netfp, filename, false);
	}

	/// <summary>
	/// Processes node information from a makenet file. This version initializes the 
	/// counters properly and then calls the version that has the full argument list. </summary>
	/// <param name="netfp"> the BufferedReader to use for reading from the file. </param>
	/// <param name="filename"> the name of the file being read. </param>
	/// <param name="skipBlankNodes"> whether blank nodes should just be skipped when read in from the file. </param>
	/// <returns> a node filled with data from the makenet file. </returns>
	private HydrologyNode processMakenetNodes(StreamReader netfp, string filename, bool skipBlankNodes)
	{
		double dx = 1.0, dy = 1.0, x0 = 0.0, y0 = 0.0;
		int __closeCount = 0; // Number of closing }.
		int __openCount = 0; // Number of opening {.
		int _reachLevel = 1;
		string wd = "";

		return processMakenetNodes((HydrologyNode)null, netfp, filename, wd, x0, y0, dx, dy, __openCount, __closeCount, _reachLevel, skipBlankNodes);
	}

	/// <summary>
	/// Processes node information from a makenet file. This version is called 
	/// recursively.  A main program should call the other version. </summary>
	/// <param name="node"> the node processed prior to the current iteration. </param>
	/// <param name="netfp"> the BufferedReader to use for reading from the file. </param>
	/// <param name="filename"> the name of the file being read. </param>
	/// <param name="wd"> the water district in effect. </param>
	/// <param name="x0"> the starting X for the reach. </param>
	/// <param name="y0"> the starting Y for the reach. </param>
	/// <param name="dx"> the X-increment for drawing each node. </param>
	/// <param name="dy"> the Y-increment for drawing each node. </param>
	/// <param name="openCount"> the number of open {s. </param>
	/// <param name="closeCount"> the number of closed }s. </param>
	/// <param name="reachLevel"> the current reach level being iterated over. </param>
	/// <param name="skipBlankNodes"> whether to skip blank nodes when reading nodes in from the file. </param>
	/// <returns> a node filled with data from the makenet file. </returns>
	private HydrologyNode processMakenetNodes(HydrologyNode node, StreamReader netfp, string filename, string wd, double x0, double y0, double dx, double dy, int openCount, int closeCount, int reachLevel, bool skipBlankNodes)
	{
		string routine = "HydroBase_NodeNetwork.processMakenetNodes";
		double river_dx, river_dy, x, y;
		HydrologyNode nodePt;
		HydrologyNode tempNode;
		int dir, dl = 20, nlist, nodeInReach, numRiverNodes, numTokens, reachCounterSave;
		RiverLine river = new RiverLine();
		string message, nodeID, token0;
		IList<string> list;
		IList<string> tokens;

		__closeCount = 0; // Number of closing curly-braces.
		__openCount = 0; // Number of opening curly-braces.

		// Since some of the original C code used pointers, we use globals and
		// locals in conjunction to maintain the original C version logic.

		++reachLevel;
		++__reachCounter;
		reachCounterSave = __reachCounter;

		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "Starting new reach at (x0,y0) = " + x0 + "," + y0 + " dx,dy = " + dx + "," + dy + " reachLevel=" + reachLevel + " reachCounter=" + __reachCounter);
		}
		x = x0;
		y = y0;

		// If the downstream node was a BLANK, then we need to subtract a
		// dx,dy to get so the diagram will look right (in this case we are
		// using a BLANK instead of a CONFL or XCONFL)...

		// Reset the node counter for the reach...

		nodeInReach = 1;

		while (true)
		{
			numTokens = 0;
			try
			{
				tokens = readMakenetLineTokens(netfp);
			}
			catch (IOException)
			{
				// End of file...
				return node;
			}
			if (tokens == null)
			{
				// End of file?
				break;
			}
			else
			{
				numTokens = tokens.Count;
				if (numTokens == 0)
				{
					// Blank line...
					Message.printWarning(2, routine, "Skipping line " + __line + " in \"" + filename + "\"");
					continue;
				}
			}

			// ------------------------------------------------------------
			// Now evaluate node-level commands...
			// ------------------------------------------------------------
			token0 = (string)tokens[0];
			if (Message.isDebugOn)
			{
				Message.printDebug(dl, routine, "token[0]=\"" + token0 + "\"");
			}
			if (token0[0] == '#')
			{
				// Comment line...
				continue;
			}
			if (token0.Equals("DISTRICT", StringComparison.OrdinalIgnoreCase))
			{
				wd = tokens[1];
				Message.printStatus(2, routine, "Water district specified as " + wd);
				//HMPrintWarning(1, routine, "DISTRICT command is no longer recognized.  Use full ID");
				continue;
			}
			else if (token0.Equals("FONT", StringComparison.OrdinalIgnoreCase))
			{
				setFont(tokens[1], StringUtil.atod(tokens[2]));
				continue;
			}
			else if (token0.Equals("TEXT", StringComparison.OrdinalIgnoreCase))
			{
				message = "The TEXT command is obsolete.  Use FONT";
				Message.printWarning(3, routine, message);
				printCheck(routine, 'W', message);
				continue;
			}
			else if (token0.Equals("NODESIZE", StringComparison.OrdinalIgnoreCase))
			{
				message = "The NODESIZE command is obsolete.  Use NODEDIAM";
				Message.printWarning(3, routine, message);
				printCheck(routine, 'W', message);
				setNodeDiam(StringUtil.atod(tokens[1]));
				continue;
			}
			else if (token0.Equals("NODEDIAM", StringComparison.OrdinalIgnoreCase))
			{
				setNodeDiam(StringUtil.atod(tokens[1]));
				continue;
			}
			else if (token0.Equals("{", StringComparison.OrdinalIgnoreCase))
			{
				// put a closing } here to help editor!

				// We are starting a new stream reach so recursively
				// call this routine.  Note that the coordinates are
				// the ones for the current node...

				// If the current node(subsequent to the new reach)
				// was a blank, then do not increment the counter...
				if (node.getType() == HydrologyNode.NODE_TYPE_BLANK)
				{
					x -= dx;
					y -= dy;
				}
				++openCount;
				++__openCount;
				if (Message.isDebugOn)
				{
					Message.printDebug(dl, routine, "Adding a reach above \"" + node.getCommonID() + "\" nupstream=" + node.getNumUpstreamNodes());
				}
				HydrologyNode node_upstream = processMakenetNodes(node, netfp, filename, wd, x, y, dx, dy, openCount, closeCount, reachLevel, skipBlankNodes);
				if (node_upstream == null)
				{
					// We have bad troubles.  Return null here to
					// back out also (we want to end the program).
					message = "Major error processing nodes.";
					Message.printWarning(3, routine, message);
					printCheck(routine, 'W', message);
					return null;
				}
				// Else add the node...
				// This would have been done in the recurse!  
				// Don't need to add again...
				//node.addUpstreamNode(node_upstream);
				if (Message.isDebugOn)
				{
					Message.printDebug(dl, routine, "Added reach starting at \"" + node_upstream.getNetID() + "\" reachnum=" + node_upstream.getTributaryNumber() + " num=" + node_upstream.getSerial() + "(downstream is \"" + node.getNetID() + "\" [" + node.getSerial() + ", nupstream=" + node.getNumUpstreamNodes() + ")");
				}
				x += dx;
				y += dy;
				if (Message.isDebugOn)
				{
					Message.printDebug(dl, routine, "Now back at reach level " + reachLevel);
				}
			}
			else if (token0.Equals("STREAM", StringComparison.OrdinalIgnoreCase))
			{
				// We are starting a stream(this will generally only be called when recursing).

				// with graphing
				river.id = tokens[1];
				Message.printStatus(2, routine, "Starting stream \"" + river.id + "\".  Building network upstream...");
				numRiverNodes = StringUtil.atoi(tokens[2]);
				river_dx = StringUtil.atod(tokens[3]);
				river_dy = StringUtil.atod(tokens[4]);
				river.strx = x;
				river.stry = y;
				river.endx = x + river_dx;
				river.endy = y + river_dy;

				// Reset the dx and dy based on this reach...
				if (numRiverNodes == 1)
				{
					dx = river.endx - river.strx;
					dy = river.endy - river.stry;
				}
				else
				{
					dx = (river.endx - river.strx) / (numRiverNodes - 1);
					dy = (river.endy - river.stry) / (numRiverNodes - 1);
				}
				if (Message.isDebugOn)
				{
					Message.printDebug(1, routine, "Resetting dx,dy to " + dx + "," + dy);
				}
				// Save the reach label...
				storeLabel((string)tokens[1], x, y, (x + river_dx),(y + river_dy));
				// Plot the line for the river reach in the old plot
				// file.  The nodes will plot on top and clear the line under the node...

				// Because we are on a new reach, we want our first
				// node to be off the previous stem...
			}
			// Add { here to help editor with matching on the next line...
			else if (token0.Equals("}", StringComparison.OrdinalIgnoreCase))
			{
				// Make sure that we are not closing off more reaches than we have started...
				++closeCount;
				++__closeCount;
				if (closeCount > openCount)
				{
					message = "Line " + __line + ":  unmatched }(" + openCount + " {, " + closeCount + " })";
					Message.printWarning(3, routine, message);
					printCheck(routine, 'W', message);
					return null;
				}
				// We are done with the recursion on the stream reach
				// but need to return the lowest node in the reach...
				nodePt = getDownstreamNode(node, POSITION_REACH);
				if (Message.isDebugOn)
				{
					Message.printDebug(dl, routine, "Done with reach(top=\"" + node.getCommonID() + "\", bottom=\"" + nodePt.getCommonID() + "\")");
				}
				return nodePt;
			}
			else if (token0.Equals("STOP", StringComparison.OrdinalIgnoreCase))
			{
				// We are at the end of the system.  Just return the
				// node (which will be the upstream node).  This should
				// send us back to the main program...
				Message.printStatus(2, routine, "Detected STOP in net file.  Stop processing nodes.");
				return node;
			}
			else if (token0.Equals("BLANK", StringComparison.OrdinalIgnoreCase) && skipBlankNodes)
			{
				// skip it
				x += dx;
				y += dy;
			}
			else
			{
				// Node information...

				// Create a new node above another node (this is not
				// adding a reach - it is adding a node within a reach)...
				if (token0.Length > 12)
				{
					Message.printWarning(1, routine, "\"" + token0 + "\"(line " + __line + " is > 12 characters.  Truncating to 12.");
					token0 = token0.Substring(0, 12);
				}
				if (node == null)
				{
					// This is the first node in the network...
					node = new HydrologyNode();
					if (Message.isDebugOn)
					{
						Message.printDebug(dl, routine, "Adding first node");
					}
				}
				else
				{
					// This is a node...

					// First create the upstream node...
					if (Message.isDebugOn)
					{
						Message.printDebug(dl, routine, "Adding upstream node \"" + token0 + "\"(line " + __line + ")");
					}

					tempNode = new HydrologyNode();
					node.addUpstreamNode(tempNode);
					// Now it is safe to reset the current node to be the upstream node...
					node = node.getUpstreamNode(node.getNumUpstreamNodes() - 1);
					// Set the h_num to be the same as the number of nodes added above the downstream node...
					node.setTributaryNumber((node.getDownstreamNode()).getNumUpstreamNodes());
				}
				// Set the node information regardless whether the
				// first node or not.  By here, "node" points to the node that has just been added...

				// Set node in reach...
				node.setNodeInReachNumber(nodeInReach);

				// Use the saved reach counter so that we
				// do not keep incrementing the reach counter for the same reach...
				node.setReachCounter(reachCounterSave);
				node.setReachLevel(reachLevel);
				node.setSerial(getNodeCount());

				if (node.getDownstreamNode() == null)
				{
					// First node.  Do not print downstream...
					if (Message.isDebugOn)
					{
						Message.printDebug(dl, routine, "Added node \"" + token0 + "\" nodeInReach=" + node.getNodeInReachNumber() + " reachnum=" + node.getTributaryNumber() + " reachCounter=" + node.getReachCounter() + " num=" + node.getSerial() + " #up=" + node.getNumUpstreamNodes());
					}
				}
				else
				{
					if (Message.isDebugOn)
					{
						Message.printDebug(1, routine, "Added node \"" + tokens[0] + "\" nodeInReach=" + node.getNodeInReachNumber() + " reachnum=" + node.getTributaryNumber() + " reachCounter=" + node.getReachCounter() + " num=" + node.getSerial() + " #up=" + node.getNumUpstreamNodes() + "(downstream is \"" + (node.getDownstreamNode()).getNetID() + " nupstream=" + (node.getDownstreamNode()).getNumUpstreamNodes() + ")");
					}
				}
				++nodeInReach;
				setNodeCount(getNodeCount() + 1);
				if (token0.Equals("BLANK", StringComparison.OrdinalIgnoreCase))
				{
					// Allow blank nodes for spacing...
					node.setType(HydrologyNode.NODE_TYPE_BLANK);
					node.setNetID("BLANK");
					Message.printStatus(2, routine, "Processing node " + (getNodeCount() - 1) + ": BLANK");
				}
				else if (token0.Equals("CONFL", StringComparison.OrdinalIgnoreCase))
				{
					// Confluence...
					node.setType(HydrologyNode.NODE_TYPE_CONFLUENCE);
					node.setNetID("CONFL");
					Message.printStatus(2, routine, "Processing node " + (getNodeCount() - 1) + ": CONFL");
				}
				else if (token0.Equals("XCONFL", StringComparison.OrdinalIgnoreCase))
				{
					// Computational confluence for off-stream channel...
					node.setType(HydrologyNode.NODE_TYPE_XCONFLUENCE);
					node.setNetID("XCONFL");
					Message.printStatus(2, routine, "Processing node " + (getNodeCount() - 1) + ": XCONFL");
				}
				else if (token0.Equals("END", StringComparison.OrdinalIgnoreCase))
				{
					// End of system.  This node has no downstream node...
					node.setType(HydrologyNode.NODE_TYPE_END);
					node.setNetID(token0);
					Message.printStatus(2, routine, "Processing node " + (getNodeCount() - 1) + ": END");
				}
				else
				{
					// A valid node ID...
					node.setNetID(token0);
					Message.printStatus(2, routine, "Processing node " + (getNodeCount() - 1) + ": " + token0);
					// Get the node type. 
					string token1 = tokens[1];
					if (token1.Equals("BLANK", StringComparison.OrdinalIgnoreCase))
					{
						node.setType(HydrologyNode.NODE_TYPE_BLANK);
					}
					else if (token1.Equals("DIV", StringComparison.OrdinalIgnoreCase))
					{
						node.setType(HydrologyNode.NODE_TYPE_DIV);
						if (Message.isDebugOn)
						{
							Message.printDebug(dl, routine, "Node \"" + node.getNetID() + "\" type is DIV " + "from \"DIV\"");
						}
					}
					else if (token1.Equals("D&W", StringComparison.OrdinalIgnoreCase) || token1.Equals("DW", StringComparison.OrdinalIgnoreCase))
					{
						node.setType(HydrologyNode.NODE_TYPE_DIV_AND_WELL);
						if (Message.isDebugOn)
						{
							Message.printDebug(dl, routine, "Node \"" + node.getNetID() + "\" type is D&W from \"D&W\"");
						}
					}
					else if (token1.Equals("WELL", StringComparison.OrdinalIgnoreCase))
					{
						node.setType(HydrologyNode.NODE_TYPE_WELL);
					}
					else if (token1.Equals("FLOW", StringComparison.OrdinalIgnoreCase))
					{
						node.setType(HydrologyNode.NODE_TYPE_FLOW);
					}
					else if (token1.Equals("CONFL", StringComparison.OrdinalIgnoreCase))
					{
						node.setType(HydrologyNode.NODE_TYPE_CONFLUENCE);
					}
					else if (token1.Equals("XCONFL", StringComparison.OrdinalIgnoreCase))
					{
						node.setType(HydrologyNode.NODE_TYPE_XCONFLUENCE);
					}
					else if (token1.Equals("END", StringComparison.OrdinalIgnoreCase))
					{
						node.setType(HydrologyNode.NODE_TYPE_END);
					}
					else if (token1.Equals("ISF", StringComparison.OrdinalIgnoreCase))
					{
						node.setType(HydrologyNode.NODE_TYPE_ISF);
					}
					else if (token1.Equals("OTH", StringComparison.OrdinalIgnoreCase))
					{
						node.setType(HydrologyNode.NODE_TYPE_OTHER);
					}
					else if (token1.Equals("PLN", StringComparison.OrdinalIgnoreCase))
					{
						node.setType(HydrologyNode.NODE_TYPE_PLAN);
					}
					else if (token1.Equals("RES", StringComparison.OrdinalIgnoreCase))
					{
						node.setType(HydrologyNode.NODE_TYPE_RES);
					}
					else if (token1.Equals("IMPORT", StringComparison.OrdinalIgnoreCase))
					{
						node.setType(HydrologyNode.NODE_TYPE_IMPORT);
					}
					else if (token1.Equals("BFL", StringComparison.OrdinalIgnoreCase))
					{
						node.setType(HydrologyNode.NODE_TYPE_BASEFLOW);
					}
					else
					{
						// Assume number type...
						if (Message.isDebugOn)
						{
							message = "\"" + node.getNetID() + "\" Node type \""
								+ token1 + "\":  Node type is not recognized.";
							Message.printWarning(3, routine, message);
							printCheck(routine, 'W', message);
						}
						return null;
					}
					// Now process the area*precip information...
					if (!node.parseAreaPrecip(tokens[2]))
					{
						message = "Error processing area*precip info for \"" + node.getNetID() + "\"";
						Message.printWarning(3, routine,message);
						printCheck(routine, 'W', message);
					}
					if (numTokens >= 4)
					{
						dir = StringUtil.atoi(tokens[3]);
					}
					else
					{
						Message.printWarning(3, routine, "No direction for \"" + token0 + "\" symbol.  Assuming 1.");
						dir = 1;
					}
					node.setLabelDirection(dir);
					// Now process the description if specified (it is optional)...
					if (numTokens >= 5)
					{
						node.setDescription(tokens[4]);
						node.setUserDescription(tokens[4]);
					}
					// Else defaults are empty descriptions.
				}
				if (Message.isDebugOn)
				{
					Message.printDebug(dl, routine, "Set \"" + node.getNetID() + "\" node type to " + node.getType() + " area=" + node.getArea() + " precip=" + node.getPrecip() + " water=" + node.getWater() + " desc=\"" + node.getDescription() + "\"");
				}
				// Now get the common ID and description for
				// the node.  Only do this if we have not already
				// manually set the description. This is a little ugly
				// because some nodes types have multiple values being
				// set - we can't just check the description being
				// empty in an outside loop.
				if (node.getType() == HydrologyNode.NODE_TYPE_FLOW)
				{
					node.setCommonID(node.getNetID());
				}
				else if (node.getType() == HydrologyNode.NODE_TYPE_CONFLUENCE)
				{
					// Confluences...
					node.setCommonID("CONFL");
				}
				else if (node.getType() == HydrologyNode.NODE_TYPE_XCONFLUENCE)
				{
					// Confluences...
					node.setCommonID("XCONFL");
				}
				else if (node.getType() == HydrologyNode.NODE_TYPE_BLANK)
				{
					// Blank nodes...
					node.setCommonID("BLANK");
				}
				else if (node.getType() == HydrologyNode.NODE_TYPE_END)
				{
					// End node...
					node.setCommonID(node.getNetID());
				}
				else if (node.getType() == HydrologyNode.NODE_TYPE_BASEFLOW)
				{
					// Special baseflow node...
					node.setCommonID(node.getNetID());
				}
				else if (node.getType() == HydrologyNode.NODE_TYPE_RES)
				{
					// Reservoirs...
					node.setCommonID(node.getNetID());
				}
				else if (node.getType() == HydrologyNode.NODE_TYPE_ISF)
				{
					// Minimum streamflows.  Get the description from the water rights...  For these
					// we are allowed to abbreviate the identifier on the network.  For the
					// common ID we need to prepend the water district...

					// To support old-style ISF identifiers with periods, we need to have the following...
					list = StringUtil.breakStringList(node.getNetID(), ".", 0);
					nlist = list.Count;
					if (nlist >= 1)
					{
						nodeID = list[0];
					}
					else
					{
						nodeID = new string(node.getNetID());
					}
					string wdid = formatWDID(wd, nodeID, node.getType());
					node.setCommonID(wdid);
					node.setCommonID(node.getNetID());
				}
				else if (node.getType() == HydrologyNode.NODE_TYPE_WELL)
				{
					// Ground water well only.  Don't allow the abbreviation...
					node.setCommonID(node.getNetID());
				}
				else if (node.getType() == HydrologyNode.NODE_TYPE_PLAN)
				{
					// Plans...
					node.setCommonID(node.getNetID());
				}
				else
				{
					// Diversions, imports, and D&W.  For these we are allowed to abbreviate the
					// identifier on the network.  For the common ID we need to prepend the
					// water district...
					string wdid = formatWDID(wd, node.getNetID(), node.getType());
					node.setCommonID(wdid);
				}
				if (Message.isDebugOn)
				{
					Message.printDebug(dl, routine, "Set common ID for \"" + node.getNetID() + "\" to \"" + node.getCommonID() + "\"");
				}

				// Set the river node ID...
				// The new way is to just use the common ID.  The
				// old way is to put an extension on USGS IDs (the old code has been deleted)...

				// The new way...
				node.setRiverNodeID(node.getCommonID());
				if (Message.isDebugOn)
				{
					Message.printDebug(dl, routine, "Set \"" + node.getNetID() + "\" river node to \"" + node.getRiverNodeID() + "\"");
				}
				// Now plot the node...
				node.setX(x);
				node.setY(y);
				if ((node.getType() != HydrologyNode.NODE_TYPE_CONFLUENCE) && (node.getType() != HydrologyNode.NODE_TYPE_XCONFLUENCE))
				{
					// If a confluence we want the line to come in at the same point...
					x += dx;
					y += dy;
				}
			}
		}
		return node;
	}

	/// <summary>
	/// Read a line from the makenet network file and split into tokens.
	/// It is assumed that the line number is initialized to zero in the main program.
	/// Blank lines are ignored.  Comments are parsed and returned (can be ignored in calling code). </summary>
	/// <param name="netfp"> the reader reading the makenet net file. </param>
	/// <returns> the tokens from the line </returns>
	/// <exception cref="IOException"> if there is an error reading the line from the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private java.util.List<String> readMakenetLineTokens(java.io.BufferedReader netfp) throws java.io.IOException
	private IList<string> readMakenetLineTokens(StreamReader netfp)
	{
		string routine = "StateMod_NodeNetwork.readMakenetLineTokens";
		int commentIndex = 0, dl = 50, numTokens;
		string lineString;
		IList<string> tokens;

		while (true)
		{
			try
			{
				lineString = netfp.ReadLine();
				if (string.ReferenceEquals(lineString, null))
				{
					break;
				}
			}
			catch (IOException)
			{
				// End of file.
				throw new IOException("End of file");
			}
			++__line;
			lineString = StringUtil.removeNewline(lineString.Trim());
			if (Message.isDebugOn)
			{
				Message.printDebug(dl, routine, "Line " + __line + ":  \"" + lineString + "\"");
			}

			// Trim comment from end (if included)...
			commentIndex = lineString.IndexOf('#');
			if (commentIndex >= 0)
			{
				// Special check to allow _# because it is currently
				// used in some identifiers...
				if ((commentIndex > 0) && lineString[commentIndex - 1] == '_')
				{
					Message.printWarning(3, routine, "Need to remove # from ID on line " + __line + ": " + lineString);
				}
				else
				{
					// OK to reset the line to the the beginning of the line...
					lineString = lineString.Substring(0, commentIndex);
				}
			}

			// Now break into tokens...

			numTokens = 0;
			tokens = StringUtil.breakStringList(lineString, "\t ", StringUtil.DELIM_SKIP_BLANKS | StringUtil.DELIM_ALLOW_STRINGS);
			if (tokens != null)
			{
				numTokens = tokens.Count;
			}
			if (numTokens == 0)
			{
				// Blank line...
				continue;
			}
			return tokens;
		}
		return null;
	}

	/// <summary>
	/// Read an entire Makenet network file and save in memory. </summary>
	/// <param name="nodeDataProvider"> Object that fills in node data (e.g., from HydroBase). </param>
	/// <param name="filename"> Makenet .net file to read and process. </param>
	/// <returns> true if the network was read successfully, false if not. </returns>
	public virtual bool readMakenetNetworkFile(StateMod_NodeDataProvider nodeDataProvider, string filename)
	{
		return readMakenetNetworkFile(nodeDataProvider, filename, false);
	}

	/// <summary>
	/// Read an entire Makenet network file and save in memory. </summary>
	/// <param name="nodeDataProvider"> Object that fills in node data (e.g., from HydroBase). </param>
	/// <param name="filename"> Makenet .net file to read and process. </param>
	/// <param name="skipBlankNodes"> whether to skip blank nodes when reading nodes in. </param>
	/// <returns> true if the network was read successfully, false if not. </returns>
	public virtual bool readMakenetNetworkFile(StateMod_NodeDataProvider nodeDataProvider, string filename, bool skipBlankNodes)
	{
		string routine = "HydroBase_NodeNetwork.readMakenetNetworkFile";
		StreamReader @in;
		try
		{
			@in = new StreamReader(filename);
		}
		catch (IOException)
		{
			string message = "Error opening net file \"" + filename + "\"";
			Message.printWarning(3, routine, message);
			printCheck(routine, 'W', message);
			return false;
		}
		// Set the filename so that it can be selected by default, for example in save dialogs
		setInputName(filename);
		return readMakenetNetworkFile(nodeDataProvider, @in, filename, skipBlankNodes);
	}

	/// <summary>
	/// Read an entire Makenet network file and save in memory. </summary>
	/// <param name="nodeDataProvider"> Object that fills in node data (e.g., from HydroBase). </param>
	/// <param name="in"> the BufferedReader to use for reading from the file. </param>
	/// <param name="filename"> Makenet .net file to read and process. </param>
	/// <returns> true if the network was read successfully, false if not. </returns>
	public virtual bool readMakenetNetworkFile(StateMod_NodeDataProvider nodeDataProvider, StreamReader @in, string filename)
	{
		return readMakenetNetworkFile(nodeDataProvider, @in, filename, false);
	}

	/// <summary>
	/// Read an entire makenet network file and save in memory. </summary>
	/// <param name="nodeDataProvider"> Object that fills in node data (e.g., from HydroBase). </param>
	/// <param name="in"> the BufferedReader opened on the file to use for reading it. </param>
	/// <param name="filename"> the name of the file to be read. </param>
	/// <param name="skipBlankNodes"> whether to skip blank nodes when reading in from a file. </param>
	/// <returns> true if the network was read successfully, false if not. </returns>
	public virtual bool readMakenetNetworkFile(StateMod_NodeDataProvider nodeDataProvider, StreamReader @in, string filename, bool skipBlankNodes)
	{
		string routine = "HydroBase_NodeNetwork.readMakenetNetworkFile";
		double dx = 1.0, dy = 1.0, x0 = 0.0, x1 = 1.0, y0 = 0.0, y1 = 1.0;
		int dl = 30, numTokens, numnodes, reachLevel = 0;
		string token0;
		IList<string> tokens;

		// Create a blank node used for disappearing streams.  The identifiers will be empty strings...

		HydrologyNode blankNode = new HydrologyNode();
		blankNode.setDescription("SURFACE WATER LOSS");
		blankNode.setUserDescription("SURFACE WATER LOSS");
		blankNode.setType(HydrologyNode.NODE_TYPE_UNKNOWN);

		// Start at the top of the file and read until we get to the STOP command or the end of the file...

		while (true)
		{
			numTokens = 0;
			try
			{
				tokens = readMakenetLineTokens(@in);
			}
			catch (IOException)
			{
				// End of the file so break...
				break;
			}
			if (tokens == null)
			{
				continue;
			}
			numTokens = tokens.Count;
			if (numTokens == 0)
			{
				// Blank line...
				continue;
			}
			token0 = tokens[0];
			if (Message.isDebugOn)
			{
				Message.printDebug(10, routine, "token[0]=\"" + token0 + "\"");
			}
			if (token0.Equals("STOP", StringComparison.OrdinalIgnoreCase))
			{
				// End of run...
				break;
			}
			else if (token0[0] == '#')
			{
				// Comment...
				continue;
			}
			else if (token0.Equals("LEGEND", StringComparison.OrdinalIgnoreCase))
			{
				if (Message.isDebugOn)
				{
					Message.printDebug(dl, routine, "Found LEGEND command");
				}
				//TODO SAM 2007-02-18 Evaluate whether needed
				//__legendX = StringUtil.atod(
				//		 (String)tokens.elementAt(1));
				//__legendY = StringUtil.atod(
				//		 (String)tokens.elementAt(2));
				//__legendDX = StringUtil.atod(
				//		 (String)tokens.elementAt(3));
				//__legendDY = StringUtil.atod(
				//		 (String)tokens.elementAt(4));
				continue;
			}
			else if (token0.Equals("SCALE", StringComparison.OrdinalIgnoreCase))
			{
				if (Message.isDebugOn)
				{
					Message.printDebug(dl, routine,"Found SCALE command");
				}
				string message = "SCALE is obsolete!";
				Message.printWarning(3, routine, message);
				printCheck(routine, 'W', message);
				//sx = atof(tokens[1]);
				//sy = atof(tokens[2]);
				continue;
			}
			else if (token0.Equals("RIVER", StringComparison.OrdinalIgnoreCase))
			{
				// Line information for the river reach...
				if (Message.isDebugOn)
				{
					Message.printDebug(dl, routine,"Found RIVER command");
				}
				numnodes = StringUtil.atoi((string)tokens[2]);
				x0 = StringUtil.atod((string)tokens[3]);
				y0 = StringUtil.atod((string)tokens[4]);
				x1 = StringUtil.atod((string)tokens[5]);
				y1 = StringUtil.atod((string)tokens[6]);

				// Calculate the spacing of nodes on the main stem...
				dx = (x1 - x0) / (double)(numnodes - 1);
				dy = (y1 - y0) / (double)(numnodes - 1);

				// Break out of this loop and go to the next level (do not use a continue...
			}
			else if (token0.Equals("TITLE", StringComparison.OrdinalIgnoreCase))
			{
				if (Message.isDebugOn)
				{
					Message.printDebug(dl, routine, "Found TITLE command");
				}
				double fontSize = StringUtil.atod((string)tokens[1]);
				double titleX = StringUtil.atod((string)tokens[2]);
				double titleY = StringUtil.atod((string)tokens[3]);
				string title = (string)tokens[4];
				for (int i = 5; i < numTokens; i++)
				{
					title = title + " ";
					title = title + (string)tokens[i];
				}
				setFont(null, fontSize);
				setTitle(title);
				setTitleX(titleX);
				setTitleY(titleY);
				// TODO SAM 2008-03-16 Evaluate whether the title is special or just another label/annotation
				// Also add as a label
				addLabel(titleX, titleY, fontSize, GRText.LEFT | GRText.BOTTOM, title);
				continue;
			}

			// Else, recursively process the child reaches...

			StopWatch timer = new StopWatch();
			timer.start();
			setNodeHead(processMakenetNodes(getNodeHead(), @in, filename, "", x0, y0, dx, dy, __openCount, __closeCount, reachLevel, skipBlankNodes));
			timer.stop();
			Message.printStatus(2, routine, "Reading net file took " + (int)timer.getSeconds() + " seconds.");

			// Now set the descriptions, which are dependent on database information...

			try
			{
				setNodeDescriptions(nodeDataProvider);
			}
			catch (Exception e)
			{
				Message.printWarning(3, routine, e);
				convertNodeTypes();
				return false;
			}

			if (Message.isDebugOn)
			{
				Message.printDebug(dl, routine, "Done reading net file.");
				Message.printDebug(dl, routine, "Node is \"" + getNodeHead().getCommonID() + "\"");
			}
			resetComputationalOrder();
			convertNodeTypes();
			return true;
		}
		convertNodeTypes();
		return true;
	}

	/// <summary>
	/// Reads a StateMod network file in either Makenet or XML format and returns the network that was generated. </summary>
	/// <param name="filename"> the name of the file from which to read. </param>
	/// <param name="nodeDataProvider"> Object that fills in node data (e.g., from HydroBase).
	/// Can be null if reading from an XML file. </param>
	/// <param name="skipBlankNodes"> whether blank nodes should be read from the Makenet file
	/// or not.  Does not matter if reading from an XML file. </param>
	/// <returns> the network read from the file. </returns>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static StateMod_NodeNetwork readStateModNetworkFile(String filename, StateMod_NodeDataProvider nodeDataProvider, boolean skipBlankNodes) throws Exception
	public static StateMod_NodeNetwork readStateModNetworkFile(string filename, StateMod_NodeDataProvider nodeDataProvider, bool skipBlankNodes)
	{
		StateMod_NodeNetwork network = null;
		if (isXML(filename))
		{
			network = readXMLNetworkFile(filename);
		}
		else
		{
			network = new StateMod_NodeNetwork();
			network.readMakenetNetworkFile(nodeDataProvider, filename, skipBlankNodes);
		}
		return network;
	}

	/// <summary>
	/// Reads a HydroBase_NodeNetwork from an XML Network file. </summary>
	/// <param name="filename"> the name of the file to read. </param>
	/// <returns> the network read from the file. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static StateMod_NodeNetwork readXMLNetworkFile(String filename) throws Exception
	public static StateMod_NodeNetwork readXMLNetworkFile(string filename)
	{
		string routine = "StateMod_NodeNetwork.readXMLNetworkFile";
		IList<HydrologyNode> networkNodeList = new List<object>(); // List of all nodes read
		IList<PropList> networkLinkList = new List<object>(); // List of all links read (lines from one node to another)
		IList<PropList> networkLayoutList = new List<object>(); // List of all layouts
		IList<HydrologyNode> networkAnnotationList = new List<object>(); // List of all annotations read - these are built-in
														// as opposed to run-time annotations from the StateMod GUI

		DOMParser parser = null;
		try
		{
			parser = new DOMParser();
			parser.parse(filename);
		}
		catch (Exception e)
		{
			Message.printWarning(2, routine, "Error reading XML Network file \"" + filename + "\"");
			Message.printWarning(2, routine, e);
			throw new Exception("Error reading XML Network file \"" + filename + "\"");
		}

		// Now get information from the document.  For now don't hold the document as a data member...
		Document doc = parser.getDocument();

		// Loop through and process the document nodes, starting with the root node.
		// Pass data needed during processing but keep handing within static code

		// LeftX, LowerY, RightX, TopY, LegendX, LegendY - from network properties
		double?[] extentData = new double?[] {Double.NaN, Double.NaN, Double.NaN, Double.NaN, Double.NaN, Double.NaN};
		// LeftX, LowerY, RightX, TopY, from checking network coordinates
		double?[] extentDataFromNodes = new double?[] {Double.NaN, Double.NaN, Double.NaN, Double.NaN};

		// Left, right, top, bottom
		double?[] edgeBuffer = new double?[] {Double.NaN, Double.NaN, Double.NaN, Double.NaN};

		readXMLNetworkFile_ProcessDocumentNode(doc, networkNodeList, networkLinkList, networkLayoutList, networkAnnotationList, extentData, extentDataFromNodes, edgeBuffer);

		// Check the extent data for the network against the extents from the nodes...
		// Legacy code adjusts the data limits to page size so for example a wide network will have its Y
		// limits adjusted to be higher
		Message.printStatus(2, routine, "Extents from network properties:  (" + extentData[0] + "," + extentData[1] + ") to (" + extentData[2] + "," + extentData[3] + ")");
		// True data limits are what we really want so that the page can be properly sized as needed...
		Message.printStatus(2, routine, "Extents from node data:  (" + extentDataFromNodes[0] + "," + extentDataFromNodes[1] + ") to (" + extentDataFromNodes[2] + "," + extentDataFromNodes[3] + ")");
		// TODO SAM 2011-07-08 However, the issue is with a new network, where the node data would be limiting
		// Need to resolve this - need a network editor tool to "recenter" with an edge buffer.
		//extentData[0] = extentDataFromNodes[0];
		//extentData[1] = extentDataFromNodes[1];
		//extentData[2] = extentDataFromNodes[2];
		//extentData[3] = extentDataFromNodes[3];

		HydrologyNode node = networkNodeList[0];

		StateMod_NodeNetwork network = null;
		if (node.getComputationalOrder() == -1)
		{
			network = new StateMod_NodeNetwork();
			network.calculateNetworkNodeData(networkNodeList, false);
		}
		else
		{
			network = readXMLNetworkFile_BuildNetworkFromXMLNodes(networkNodeList);
		}
		// Set the filename so that it can be selected by default, for example in save dialogs
		// This is in the base class
		network.setInputName(filename);
		network.setAnnotationList(networkAnnotationList);
		network.setLayoutList(networkLayoutList);
		network.setLinkList(networkLinkList);

		network.setBounds(extentData[0], extentData[1], extentData[2], extentData[3]);
		if (!extentData[4].isNaN() && !extentData[5].isNaN())
		{
			network.setLegendPosition(extentData[4], extentData[5]);
		}

		if (network != null)
		{
			network.convertNodeTypes();
			network.finalCheck(extentData[0], extentData[1], extentData[2], extentData[3], false);
		}

		return network;
	}

	/// <summary>
	/// Builds all the network connections based on individual network nodes read in
	/// from an XML file and returns the network that was built. </summary>
	/// <returns> a HydroBase_NodeNetwork with all its connections built. </returns>
	private static StateMod_NodeNetwork readXMLNetworkFile_BuildNetworkFromXMLNodes(IList<HydrologyNode> networkNodeList)
	{
		// Put the nodes into an array for quicker iteration
		int size = networkNodeList.Count;

		// Add the nodes to an array for quicker traversal.  The nodes will be
		// looped through entirely size*3 times, so with a large Vector 
		// performance will be impacted by all the casts.  
		HydrologyNode[] nodes = new HydrologyNode[size];
		for (int i = 0; i < size; i++)
		{
			nodes[i] = networkNodeList[i];
		}

		string dsid = null;
		string[] usid = null;
		// Right now every node has a String that tells what its upstream
		// and downstream nodes are.  No connections.  Find the nodes that
		// match the upstream and downstream node IDs and make the connections.
		for (int i = 0; i < size; i++)
		{
			dsid = nodes[i].getDownstreamNodeID();
			usid = nodes[i].getUpstreamNodeIDs();

			if (!string.ReferenceEquals(dsid, null) && !dsid.Equals("") && !dsid.Equals("null", StringComparison.OrdinalIgnoreCase))
			{
				for (int j = 0; j < size; j++)
				{
					if (nodes[j].getCommonID().Equals(dsid))
					{
						nodes[i].setDownstreamNode(nodes[j]);
						j = size + 1;
					}
				}
			}

			for (int j = 0; j < usid.Length; j++)
			{
				for (int k = 0; k < size; k++)
				{
					if (nodes[k].getCommonID().Equals(usid[j]))
					{
						nodes[i].addUpstreamNode(nodes[k]);
						k = size + 1;
					}
				}
			}
		}

		// Put the nodes back in a list for placement back into the node network.
		IList<HydrologyNode> v = new List<object>();
		for (int i = 0; i < size; i++)
		{
			v.Add(nodes[i]);
		}

		StateMod_NodeNetwork network = new StateMod_NodeNetwork();
		network.setNetworkFromNodes(v);
		// TODO SAM 2011-07-08 Why does the shading in the network diagram not seem to work?
		// Also calculate secondary information like stream level.
		//network.calculateNetworkNodeData(networkNodeList, false);
		return network;
	}

	/// <summary>
	/// Processes an annotation node from an XML file and builds the annotation that will appear on the network. </summary>
	/// <param name="node"> the XML Node containing the annotation information. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static void readXMLNetworkFile_ProcessAnnotation(org.w3c.dom.Node node, java.util.List<cdss.domain.hydrology.network.HydrologyNode> networkAnnotationList) throws Exception
	private static void readXMLNetworkFile_ProcessAnnotation(Node node, IList<HydrologyNode> networkAnnotationList)
	{
		NamedNodeMap attributes = node.getAttributes();
		Node attributeNode;
		string name = null;
		string value = null;
		int nattributes = attributes.getLength();

		HydrologyNode hnode = new HydrologyNode();
		PropList p = new PropList("");
		for (int i = 0; i < nattributes; i++)
		{
			attributeNode = attributes.item(i);
			name = attributeNode.getNodeName();
			value = attributeNode.getNodeValue();
			if (name.Equals("FontSize", StringComparison.OrdinalIgnoreCase))
			{
				p.set("OriginalFontSize", value);
			}
			else
			{
				p.set(name, value);
			}
		}
		hnode.setAssociatedObject(p);

		networkAnnotationList.Add(hnode);
	}

	/// <summary>
	/// Processes a document node while reading from an XML file, the data lists and arrays will be populated during
	/// the read. </summary>
	/// <param name="node"> the node to process </param>
	/// <param name="networkNodeList"> empty list of nodes </param>
	/// <param name="networkLinkList"> empty list of link data </param>
	/// <param name="networkLayoutList"> empty list of page layout data </param>
	/// <param name="networkAnnotationList"> empty list of annotations, </param>
	/// <param name="extentData"> empty array of extent data (xmin, xmax, ymin, ymax, legendx, legendy) </param>
	/// <param name="edgeBuffer"> empty array of buffer values (left, right, top, bottom) </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static void readXMLNetworkFile_ProcessDocumentNode(org.w3c.dom.Node node, java.util.List<cdss.domain.hydrology.network.HydrologyNode> networkNodeList, java.util.List<RTi.Util.IO.PropList> networkLinkList, java.util.List<RTi.Util.IO.PropList> networkLayoutList, java.util.List<cdss.domain.hydrology.network.HydrologyNode> networkAnnotationList, Double[] extentData, Double[] extentDataFromNodes, Double [] edgeBuffer) throws Exception
	private static void readXMLNetworkFile_ProcessDocumentNode(Node node, IList<HydrologyNode> networkNodeList, IList<PropList> networkLinkList, IList<PropList> networkLayoutList, IList<HydrologyNode> networkAnnotationList, Double[] extentData, Double[] extentDataFromNodes, Double[] edgeBuffer)
	{
		NodeList children;
		switch (node.getNodeType())
		{
			case Node.DOCUMENT_NODE:
				// The main data set node.  Get the data set type, etc.
				readXMLNetworkFile_ProcessDocumentNode(((Document)node).getDocumentElement(), networkNodeList, networkLinkList, networkLayoutList, networkAnnotationList, extentData, extentDataFromNodes, edgeBuffer);
				children = node.getChildNodes();
				if (children != null)
				{
					readXMLNetworkFile_ProcessDocumentNode(children.item(0), networkNodeList, networkLinkList, networkLayoutList, networkAnnotationList, extentData, extentDataFromNodes, edgeBuffer);
				}
				break;
			case Node.ELEMENT_NODE:
				// Data set components.  Print the basic information...
				string elementName = node.getNodeName();
				if (elementName.Equals("StateMod_Network", StringComparison.OrdinalIgnoreCase))
				{
					readXMLNetworkFile_ProcessStateMod_NetworkNode(node, extentData, edgeBuffer);
					// The main document node will have a list 
					// of children but components will not.
					// Recursively process each node...
					children = node.getChildNodes();
					if (children != null)
					{
						int len = children.getLength();
						for (int i = 0; i < len; i++)
						{
							readXMLNetworkFile_ProcessDocumentNode(children.item(i),networkNodeList, networkLinkList, networkLayoutList, networkAnnotationList, extentData, extentDataFromNodes, edgeBuffer);
						}
					}
				}
				else if (elementName.Equals("PageLayout", StringComparison.OrdinalIgnoreCase))
				{
					readXMLNetworkFile_ProcessLayoutNode(node, networkLayoutList);
				}
				else if (elementName.Equals("Node", StringComparison.OrdinalIgnoreCase))
				{
					readXMLNetworkFile_ProcessNode(node, networkNodeList, extentData, extentDataFromNodes);
				}
				else if (elementName.Equals("Annotation", StringComparison.OrdinalIgnoreCase))
				{
					readXMLNetworkFile_ProcessAnnotation(node, networkAnnotationList);
				}
				else if (elementName.Equals("Link", StringComparison.OrdinalIgnoreCase))
				{
					readXMLNetworkFile_ProcessLink(node, networkLinkList);
				}
				break;
		}
	}

	/// <summary>
	/// Processes a "Downstream" node containing the ID of the downstream node from the Network node. </summary>
	/// <param name="hnode"> the HydroBase_Node being built. </param>
	/// <param name="node"> the XML node read from the file. </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static void readXMLNetworkFile_ProcessDownstreamNode(cdss.domain.hydrology.network.HydrologyNode hnode, org.w3c.dom.Node node) throws Exception
	private static void readXMLNetworkFile_ProcessDownstreamNode(HydrologyNode hnode, Node node)
	{
		NamedNodeMap attributes = node.getAttributes();
		Node attributeNode;
		int nattributes = attributes.getLength();
		string name = null;
		string value = null;

		for (int i = 0; i < nattributes; i++)
		{
			attributeNode = attributes.item(i);
			name = attributeNode.getNodeName();
			value = attributeNode.getNodeValue();
			if (name.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				hnode.setDownstreamNodeID(value);
			}
		}
	}

	/// <summary>
	/// Called by the readXML code when processing a Layout node. </summary>
	/// <param name="node"> the node being read. </param>
	private static void readXMLNetworkFile_ProcessLayoutNode(Node node, IList<PropList> networkLayoutList)
	{
		NamedNodeMap attributes;
		Node attributeNode;
		string name = null;
		string value = null;

		attributes = node.getAttributes();
		int nattributes = attributes.getLength();

		PropList p = new PropList("Layout");
		p.set("ID=\"Page Layout #" + (networkLayoutList.Count + 1) + "\"");
		p.set("PaperSize=\"" + DEFAULT_PAPER_SIZE + "\"");
		p.set("PageOrientation=\"" + DEFAULT_PAGE_ORIENTATION + "\"");
		p.set("NodeLabelFontSize=\"" + DEFAULT_FONT_SIZE + "\"");
		p.set("NodeSize=\"" + DEFAULT_NODE_SIZE + "\"");
		p.set("IsDefault=\"false\"");
		for (int i = 0; i < nattributes; i++)
		{
			attributeNode = attributes.item(i);
			name = attributeNode.getNodeName();
			value = attributeNode.getNodeValue();
			if (name.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				p.set("ID=\"" + value + "\"");
			}
			if (name.Equals("IsDefault", StringComparison.OrdinalIgnoreCase))
			{
				p.set("IsDefault=\"" + value + "\"");
			}
			if (name.Equals("PaperSize", StringComparison.OrdinalIgnoreCase))
			{
				p.set("PaperSize=\"" + value + "\"");
			}
			if (name.Equals("PageOrientation", StringComparison.OrdinalIgnoreCase))
			{
				p.set("PageOrientation=\"" + value + "\"");
			}
			if (name.Equals("NodeLabelFontSize", StringComparison.OrdinalIgnoreCase))
			{
				p.set("NodeLabelFontSize=\"" + value + "\"");
			}
			if (name.Equals("NodeSize", StringComparison.OrdinalIgnoreCase))
			{
				p.set("NodeSize=\"" + value + "\"");
			}
		}
		networkLayoutList.Add(p);
	}

	/// <summary>
	/// Processes a link node from an XML file and builds the link that will appear on the network. </summary>
	/// <param name="node"> the XML Node containing the link information. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static void readXMLNetworkFile_ProcessLink(org.w3c.dom.Node node, java.util.List<RTi.Util.IO.PropList> networkLinkList) throws Exception
	private static void readXMLNetworkFile_ProcessLink(Node node, IList<PropList> networkLinkList)
	{
		NamedNodeMap attributes = node.getAttributes();
		Node attributeNode;
		string name = null;
		string value = null;
		int nattributes = attributes.getLength();

		PropList p = new PropList("");
		for (int i = 0; i < nattributes; i++)
		{
			attributeNode = attributes.item(i);
			name = attributeNode.getNodeName();
			value = attributeNode.getNodeValue();
			p.set(name, value);
		}

		networkLinkList.Add(p);
	}

	/// <summary>
	/// Process the data attributes of a HydroBase_Node in the XML file. </summary>
	/// <param name="node"> the XML document node being processed </param>
	/// <param name="networkNodeList"> the list of nodes from the network </param>
	/// <param name="extentData"> the network extent (from network properties) </param>
	/// <param name="extentDataFromNodes"> the network extent from node coordinates </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static void readXMLNetworkFile_ProcessNode(org.w3c.dom.Node node, java.util.List<cdss.domain.hydrology.network.HydrologyNode> networkNodeList, Double [] extentData, Double [] extentDataFromNodes) throws Exception
	private static void readXMLNetworkFile_ProcessNode(Node node, IList<HydrologyNode> networkNodeList, Double[] extentData, Double[] extentDataFromNodes)
	{
		NamedNodeMap attributes = node.getAttributes();
		Node attributeNode;
		string area = null;
		string precip = null;
		string name = null;
		string value = null;
		int nattributes = attributes.getLength();

		HydrologyNode hnode = new HydrologyNode();
		for (int i = 0; i < nattributes; i++)
		{
			attributeNode = attributes.item(i);
			name = attributeNode.getNodeName();
			value = attributeNode.getNodeValue();
			if (name.Equals("AlternateX", StringComparison.OrdinalIgnoreCase))
			{
				hnode.setDBX((Convert.ToDouble(value)));
			}
			else if (name.Equals("AlternateY", StringComparison.OrdinalIgnoreCase))
			{
				hnode.setDBY((Convert.ToDouble(value)));
			}
			else if (name.Equals("Area", StringComparison.OrdinalIgnoreCase))
			{
				area = value;
				hnode.setArea((Convert.ToDouble(value)));
			}
			else if (name.Equals("ComputationalOrder", StringComparison.OrdinalIgnoreCase))
			{
				hnode.setComputationalOrder(Integer.decode(value).intValue());
			}
			else if (name.Equals("Description", StringComparison.OrdinalIgnoreCase))
			{
				hnode.setDescription(value);
			}
			else if (name.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				hnode.setCommonID(value);
			}
			// FIXME SAM 2008-12-10 Support both in the code: legacy "IsBaseflow" and new "IsNaturalFlow"
			else if (name.Equals("IsBaseflow", StringComparison.OrdinalIgnoreCase) || name.Equals("IsNaturalFlow", StringComparison.OrdinalIgnoreCase))
			{
				if (value.Equals("true", StringComparison.OrdinalIgnoreCase))
				{
					hnode.setIsNaturalFlow(true);
				}
				else
				{
					hnode.setIsNaturalFlow(false);
				}
			}
			else if (name.Equals("IsImport", StringComparison.OrdinalIgnoreCase))
			{
				if (value.Equals("true", StringComparison.OrdinalIgnoreCase))
				{
					hnode.setIsImport(true);
				}
				else
				{
					hnode.setIsImport(false);
				}
			}
			else if (name.Equals("LabelAngle", StringComparison.OrdinalIgnoreCase))
			{
				hnode.setLabelAngle((Convert.ToDouble(value)));
			}
			else if (name.Equals("LabelPosition", StringComparison.OrdinalIgnoreCase))
			{
				int div = hnode.getLabelDirection() / 10;
				if (value.Equals("AboveCenter", StringComparison.OrdinalIgnoreCase))
				{
					hnode.setLabelDirection((div * 10) + 1);
				}
				else if (value.Equals("UpperRight", StringComparison.OrdinalIgnoreCase))
				{
					hnode.setLabelDirection((div * 10) + 7);
				}
				else if (value.Equals("Right", StringComparison.OrdinalIgnoreCase))
				{
					hnode.setLabelDirection((div * 10) + 4);
				}
				else if (value.Equals("LowerRight", StringComparison.OrdinalIgnoreCase))
				{
					hnode.setLabelDirection((div * 10) + 8);
				}
				else if (value.Equals("BelowCenter", StringComparison.OrdinalIgnoreCase))
				{
					hnode.setLabelDirection((div * 10) + 2);
				}
				else if (value.Equals("LowerLeft", StringComparison.OrdinalIgnoreCase))
				{
					hnode.setLabelDirection((div * 10) + 5);
				}
				else if (value.Equals("Left", StringComparison.OrdinalIgnoreCase))
				{
					hnode.setLabelDirection((div * 10) + 3);
				}
				else if (value.Equals("UpperLeft", StringComparison.OrdinalIgnoreCase))
				{
					hnode.setLabelDirection((div * 10) + 6);
				}
				else if (value.Equals("Center", StringComparison.OrdinalIgnoreCase))
				{
					hnode.setLabelDirection((div * 10) + 1);
				}
				else
				{
					hnode.setLabelDirection((div * 10) + 1);
				}
			}
			else if (name.Equals("NetID", StringComparison.OrdinalIgnoreCase))
			{
				hnode.setNetID(value);
			}
			else if (name.Equals("NodeInReachNum", StringComparison.OrdinalIgnoreCase))
			{
				hnode.setNodeInReachNumber(Integer.decode(value).intValue());
			}
			else if (name.Equals("Precipitation", StringComparison.OrdinalIgnoreCase))
			{
				precip = value;
				hnode.setPrecip((Convert.ToDouble(value)));
			}
			else if (name.Equals("ReachCounter", StringComparison.OrdinalIgnoreCase))
			{
				hnode.setReachCounter(Integer.decode(value).intValue());
			}
			else if (name.Equals("ReservoirDir", StringComparison.OrdinalIgnoreCase))
			{
				int mod = hnode.getLabelDirection() % 10;
				if (value.Equals("Up", StringComparison.OrdinalIgnoreCase))
				{
					hnode.setLabelDirection(20 + mod);
				}
				else if (value.Equals("Down", StringComparison.OrdinalIgnoreCase))
				{
					hnode.setLabelDirection(10 + mod);
				}
				else if (value.Equals("Left", StringComparison.OrdinalIgnoreCase))
				{
					hnode.setLabelDirection(40 + mod);
				}
				else if (value.Equals("Right", StringComparison.OrdinalIgnoreCase))
				{
					hnode.setLabelDirection(30 + mod);
				}
				else
				{
					hnode.setLabelDirection(40 + mod);
				}
			}
			else if (name.Equals("Serial", StringComparison.OrdinalIgnoreCase))
			{
				hnode.setSerial(Integer.decode(value).intValue());
			}
			else if (name.Equals("TributaryNum", StringComparison.OrdinalIgnoreCase))
			{
				hnode.setTributaryNumber(Integer.decode(value).intValue());
			}
			else if (name.Equals("Type", StringComparison.OrdinalIgnoreCase))
			{
				hnode.setVerboseType(value);
			}
			else if (name.Equals("UpstreamOrder", StringComparison.OrdinalIgnoreCase))
			{
				hnode.setUpstreamOrder(Integer.decode(value).intValue());
			}
			else if (name.Equals("X", StringComparison.OrdinalIgnoreCase))
			{
				hnode.setX((Convert.ToDouble(value)));
				// Left...
				if (double.IsNaN(extentDataFromNodes[0]))
				{
					extentDataFromNodes[0] = hnode.getX();
				}
				else
				{
					extentDataFromNodes[0] = Math.Min(extentDataFromNodes[0], hnode.getX());
				}
				// Right...
				if (double.IsNaN(extentDataFromNodes[2]))
				{
					extentDataFromNodes[2] = hnode.getX();
				}
				else
				{
					extentDataFromNodes[2] = Math.Max(extentDataFromNodes[2], hnode.getX());
				}
			}
			else if (name.Equals("Y", StringComparison.OrdinalIgnoreCase))
			{
				hnode.setY((Convert.ToDouble(value)));
				// Bottom...
				if (double.IsNaN(extentDataFromNodes[1]))
				{
					extentDataFromNodes[1] = hnode.getY();
				}
				else
				{
					extentDataFromNodes[1] = Math.Min(extentDataFromNodes[1], hnode.getY());
				}
				// Top...
				if (double.IsNaN(extentDataFromNodes[3]))
				{
					extentDataFromNodes[3] = hnode.getY();
				}
				else
				{
					extentDataFromNodes[3] = Math.Max(extentDataFromNodes[3], hnode.getY());
				}
			}
		}

		if (!string.ReferenceEquals(area, null) && !string.ReferenceEquals(precip, null))
		{
			area = area.Trim();
			precip = precip.Trim();
			hnode.parseAreaPrecip(area + "*" + precip);
		}
		else if (!string.ReferenceEquals(area, null) && string.ReferenceEquals(precip, null))
		{
			hnode.parseAreaPrecip(area);
		}
		else
		{
			// do nothing
		}

		NodeList children = node.getChildNodes();

		if (children != null)
		{
			string elementName = null;
			int len = children.getLength();
			for (int i = 0; i < len; i++)
			{
				node = children.item(i);
				elementName = node.getNodeName();
				// Evaluate the nodes attributes...
				if (elementName.Equals("DownstreamNode", StringComparison.OrdinalIgnoreCase))
				{
					readXMLNetworkFile_ProcessDownstreamNode(hnode, node);
				}
				else if (elementName.Equals("UpstreamNode", StringComparison.OrdinalIgnoreCase))
				{
					readXMLNetworkFile_ProcessUpstreamNodes(hnode, node);
				}
				else
				{
				}
			}
		}

		networkNodeList.Add(hnode);
	}

	/// <summary>
	/// Called by the readXML code when processing a StateMod_Network node. </summary>
	/// <param name="node"> the XML node being read </param>
	/// <param name="extentData"> the maximum data coordinates of the network, considering node coordinates </param>
	/// <param name="edgeBuffer"> the additional edge buffer, in node coordinate units, that should be added to the
	/// edges of the network when rendering </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static void readXMLNetworkFile_ProcessStateMod_NetworkNode(org.w3c.dom.Node node, Double [] extentData, Double [] edgeBuffer) throws Exception
	private static void readXMLNetworkFile_ProcessStateMod_NetworkNode(Node node, Double[] extentData, Double[] edgeBuffer)
	{
		string routine = "StateMod_NodeNetwork.processStateMod_NetworkNode";
		NamedNodeMap attributes;
		Node attributeNode;
		string name = null;
		string value = null;

		attributes = node.getAttributes();
		int nattributes = attributes.getLength();

		for (int i = 0; i < nattributes; i++)
		{
			attributeNode = attributes.item(i);
			name = attributeNode.getNodeName();
			value = attributeNode.getNodeValue();
			if (name.Equals("XMin", StringComparison.OrdinalIgnoreCase))
			{
				extentData[0] = Convert.ToDouble(value);
				Message.printStatus(2, routine, "Read Xmin=" + extentData[0]);
			}
			else if (name.Equals("YMin", StringComparison.OrdinalIgnoreCase))
			{
				extentData[1] = Convert.ToDouble(value);
				Message.printStatus(2, routine, "Read Ymin=" + extentData[1]);
			}
			else if (name.Equals("XMax", StringComparison.OrdinalIgnoreCase))
			{
				extentData[2] = Convert.ToDouble(value);
				Message.printStatus(2, routine, "Read Xmax=" + extentData[2]);
			}
			else if (name.Equals("YMax", StringComparison.OrdinalIgnoreCase))
			{
				extentData[3] = Convert.ToDouble(value);
				Message.printStatus(2, routine, "Read Ymax=" + extentData[3]);
			}
			else if (name.Equals("EdgeBufferLeft", StringComparison.OrdinalIgnoreCase))
			{
				edgeBuffer[0] = Convert.ToDouble(value);
				Message.printStatus(2, routine, "Read EdgeBufferLeft=" + edgeBuffer[0]);
			}
			else if (name.Equals("EdgeBufferRight", StringComparison.OrdinalIgnoreCase))
			{
				edgeBuffer[1] = Convert.ToDouble(value);
				Message.printStatus(2, routine, "Read EdgeBufferRight=" + edgeBuffer[1]);
			}
			else if (name.Equals("EdgeBufferTop", StringComparison.OrdinalIgnoreCase))
			{
				edgeBuffer[2] = Convert.ToDouble(value);
				Message.printStatus(2, routine, "Read EdgeBufferTop=" + edgeBuffer[2]);
			}
			else if (name.Equals("EdgeBufferBottom", StringComparison.OrdinalIgnoreCase))
			{
				edgeBuffer[3] = Convert.ToDouble(value);
				Message.printStatus(2, routine, "Read EdgeBufferBottom=" + edgeBuffer[3]);
			}
			else if (name.Equals("LegendX", StringComparison.OrdinalIgnoreCase))
			{
				extentData[4] = Convert.ToDouble(value);
			}
			else if (name.Equals("LegendY", StringComparison.OrdinalIgnoreCase))
			{
				extentData[5] = Convert.ToDouble(value);
			}
		}
	}

	/// <summary>
	/// Processes an "Upstream" node containing the IDs of the upstream nodes from the Network node. </summary>
	/// <param name="hnode"> the HydroBase_Node being built. </param>
	/// <param name="node"> the XML node read from the file. </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static void readXMLNetworkFile_ProcessUpstreamNodes(cdss.domain.hydrology.network.HydrologyNode hnode, org.w3c.dom.Node node) throws Exception
	private static void readXMLNetworkFile_ProcessUpstreamNodes(HydrologyNode hnode, Node node)
	{
		NamedNodeMap attributes = node.getAttributes();
		Node attributeNode;
		int nattributes = attributes.getLength();
		string name = null;
		string value = null;

		for (int i = 0; i < nattributes; i++)
		{
			attributeNode = attributes.item(i);
			name = attributeNode.getNodeName();
			value = attributeNode.getNodeValue();
			if (name.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				hnode.addUpstreamNodeID(value);
			}
		}
	}

	/// <summary>
	/// Set whether fancy descriptions should be generated (true) or not (false). </summary>
	/// <param name="fancydesc"> true to turn on fancy descriptions. </param>
	public virtual void setFancyDesc(bool fancydesc)
	{
		__createFancyDescription = fancydesc;
	}

	/// <summary>
	/// Sets the node descriptions (names) of all nodes in the network, when reading a
	/// makenet network file.  Station names are read from HydroBase.
	/// If __createFancyDescription is true, then fancy descriptions will be set.  
	/// If the user description has been set, it will be used as is. </summary>
	/// <param name="nodeDataProvider"> Object that fills in node data (e.g., from HydroBase). </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void setNodeDescriptions(StateMod_NodeDataProvider nodeDataProvider) throws Exception
	public virtual void setNodeDescriptions(StateMod_NodeDataProvider nodeDataProvider)
	{
		if (nodeDataProvider != null)
		{
			nodeDataProvider.setNodeDescriptions(this, __createFancyDescription, __createOutputFiles);
		}
	}

	}

}