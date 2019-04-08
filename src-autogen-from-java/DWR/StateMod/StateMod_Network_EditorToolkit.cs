using System;
using System.Collections.Generic;

// StateMod_Network_EditorToolkit - this toolkit provides utility methods for processing
// network data in coordination with the network editor.

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

	using JFileChooserFactory = RTi.Util.GUI.JFileChooserFactory;
	using JGUIUtil = RTi.Util.GUI.JGUIUtil;
	using SimpleFileFilter = RTi.Util.GUI.SimpleFileFilter;
	using IOUtil = RTi.Util.IO.IOUtil;
	using Message = RTi.Util.Message.Message;

	using HydrologyNode = cdss.domain.hydrology.network.HydrologyNode;

	// TODO SAM 2011-07-07 These methods do not support the undo feature - need to enable
	/// <summary>
	/// This toolkit provides utility methods for processing network data in coordination with
	/// the network editor.  It is intended to remove some of the utilitarian code from the other objects, and
	/// coordinate where appropriate.
	/// </summary>
	public class StateMod_Network_EditorToolkit
	{

	/// <summary>
	/// Editor JFrame.  If null then dialog positioning may be off.
	/// </summary>
	private StateMod_Network_JFrame __editorJFrame = null;

	/// <summary>
	/// Editor JComponent.  May be null if node data are edited directly.
	/// </summary>
	private StateMod_Network_JComponent __editorJComponent = null;

	/// <summary>
	/// Network being edited.
	/// </summary>
	private StateMod_NodeNetwork __network = null;

	/// <summary>
	/// Constructor.  The major objects used in editing are passed in, but may not be required for all actions. </summary>
	/// <param name="editorJFrame"> the controlling editor window (e.g., to allow popup dialogs to position) </param>
	/// <param name="editorComponent"> the graphical editor component that renders the network, which does some internal
	/// optimized data management </param>
	/// <param name="network"> the network that will be manipulated, although subsets of the network may be passed in to
	/// specific methods </param>
	public StateMod_Network_EditorToolkit(StateMod_Network_JFrame editorJFrame, StateMod_Network_JComponent editorJComponent, StateMod_NodeNetwork network)
	{
		this.__editorJFrame = editorJFrame;
		this.__editorJComponent = editorJComponent;
		this.__network = network;
	}

	/// <summary>
	/// Position the nodes evenly between the end nodes. </summary>
	/// <returns> the number of nodes that were edited </returns>
	protected internal virtual int positionNodesEvenlyBetweenEndNodes(IList<HydrologyNode> nodeList)
	{
		if (nodeList.Count < 3)
		{
			return 0;
		}
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		string routine = this.GetType().FullName + "positionNodesEvenlyBetweenEndNodes";
		// First make a copy of the list so that the sort does not change the original list
		IList<HydrologyNode> nodeListSorted = null;
		// Sort the list so that it is in the order from upstream to downstream
		int editCount = 0;
		try
		{
			nodeListSorted = sortNodesSequential(nodeList);
		}
		catch (Exception e)
		{
			// Likely not in a line
			Message.printWarning(1, routine, "Error positioning nodes (" + e + ").  Make sure that nodes are in a single reach with no branches.", this.__editorJFrame);
			Message.printWarning(3, routine, e);
			return 0;
		}
		HydrologyNode firstNode = nodeListSorted[0];
		HydrologyNode lastNode = nodeListSorted[nodeListSorted.Count - 1];
		// Compute the delta between the first node and last node
		double dx = (lastNode.getX() - firstNode.getX()) / (nodeListSorted.Count - 1);
		double dy = (lastNode.getY() - firstNode.getY()) / (nodeListSorted.Count - 1);
		// Apply the delta to the middle nodes
		HydrologyNode node;
		for (int i = 1; i < nodeListSorted.Count - 1; i++)
		{
			node = nodeListSorted[i];
			node.setX(firstNode.getX() + dx * i);
			node.setY(firstNode.getY() + dy * i);
			node.setDirty(true);
			++editCount;
		}
		return editCount;
	}

	/// <summary>
	/// Adjust the X coordinate of all the given nodes to match the X coordinate of the confluence node.
	/// The operation will only be performed if there is only one confluence node. </summary>
	/// <returns> the number of nodes that were edited </returns>
	protected internal virtual int setNodeXToConfluenceX(IList<HydrologyNode> nodeList)
	{
		// First find the confluence node
		HydrologyNode confNode = null;
		int confNodeCount = 0;
		foreach (HydrologyNode node in nodeList)
		{
			if (node.getType() == HydrologyNode.NODE_TYPE_CONFLUENCE)
			{
				confNode = node;
				++confNodeCount;
			}
		}
		int editCount = 0;
		if (confNodeCount == 1)
		{
			foreach (HydrologyNode node in nodeList)
			{
				if (node != confNode)
				{
					// Set the coordinate...
					node.setX(confNode.getX());
					node.setDirty(true);
					++editCount;
				}
			}
		}
		return editCount;
	}

	/// <summary>
	/// Adjust the Y coordinate of all the given nodes to match the Y coordinate of the confluence node.
	/// The operation will only be performed if there is only one confluence node. </summary>
	/// <returns> the number of nodes that were edited </returns>
	protected internal virtual int setNodeYToConfluenceY(IList<HydrologyNode> nodeList)
	{
		// First find the confluence node
		HydrologyNode confNode = null;
		int confNodeCount = 0;
		foreach (HydrologyNode node in nodeList)
		{
			if (node.getType() == HydrologyNode.NODE_TYPE_CONFLUENCE)
			{
				confNode = node;
				++confNodeCount;
			}
		}
		int editCount = 0;
		if (confNodeCount == 1)
		{
			foreach (HydrologyNode node in nodeList)
			{
				if (node != confNode)
				{
					// Set the coordinate...
					node.setY(confNode.getY());
					node.setDirty(true);
					++editCount;
				}
			}
		}
		return editCount;
	}

	/// <summary>
	/// Sort a list of nodes to be sequential upstream to downstream.  This ensures that processing that requires
	/// a sequence will have expected data.  A new list is returned, but original node objects are not copied.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected java.util.List<cdss.domain.hydrology.network.HydrologyNode> sortNodesSequential(java.util.List<cdss.domain.hydrology.network.HydrologyNode> nodeListOrig) throws RuntimeException
	protected internal virtual IList<HydrologyNode> sortNodesSequential(IList<HydrologyNode> nodeListOrig)
	{
		IList<HydrologyNode> nodeList = new List<object>(nodeListOrig);
		IList<HydrologyNode> nodeListSorted = new List<object>();
		// First find a node that has only one of the other nodes downstream (no matching upstream).
		// This will be the upstream node.
		HydrologyNode upstreamNode = null;
		foreach (HydrologyNode node in nodeList)
		{
			IList<HydrologyNode> nodeUpstreamNodes = node.getUpstreamNodes();
			if ((nodeUpstreamNodes == null) || (nodeUpstreamNodes.Count == 0))
			{
				// No upstream nodes so this is the most upstream node
				upstreamNode = node;
			}
			else
			{
				bool foundUpstream = false;
				foreach (HydrologyNode node2 in nodeUpstreamNodes)
				{
					foreach (HydrologyNode node3 in nodeList)
					{
						if ((node3 != node) && (node2 == node3))
						{
							// Found an upstream node so this is not the upstream node in the list
							foundUpstream = true;
							break;
						}
					}
					if (foundUpstream)
					{
						// No need to keep searching
						break;
					}
				}
				if (!foundUpstream)
				{
					// The node has no upstream nodes in the list so this is the upstream node
					upstreamNode = node;
					break;
				}
			}
		}
		if (upstreamNode == null)
		{
			throw new Exception("Could not find node in list that has no upstream nodes.");
		}
		// Now go through the list and add in the order upstream to downstream.  Remove the upstream node
		// because it does not need to be considered
		nodeList.Remove(upstreamNode);
		nodeListSorted.Add(upstreamNode);
		HydrologyNode nodePrev = upstreamNode;
		while (true)
		{
			int nodeListSortedSizePrev = nodeListSorted.Count;
			foreach (HydrologyNode node in nodeList)
			{
				if (node == nodePrev.getDownstreamNode())
				{
					// Found the node downstream from the upstream so add it to the list
					nodeListSorted.Add(node);
					nodeList.Remove(node);
					nodePrev = node;
					// Start the search again
					break;
				}
			}
			if (nodeListSorted.Count == nodeListSortedSizePrev)
			{
				// No further matches found
				break;
			}
		}
		// If the number in the list is not the same as the original list there was a disconnect somewhere
		if (nodeListSorted.Count != nodeListOrig.Count)
		{
			throw new Exception("Sorted node list (" + nodeListSorted.Count + ") is not same length as original list (" + nodeListOrig.Count + ").");
		}
		return nodeListSorted;
	}

	/// <summary>
	/// Write list files for the main station lists.  These can then be used with
	/// list-based commands in StateDMI.
	/// The user is prompted for a list file name.
	/// </summary>
	protected internal virtual void writeListFiles()
	{
		string routine = "StateMod_Network_JComponent.writeListFiles";

		string lastDirectorySelected = JGUIUtil.getLastFileDialogDirectory();
		JFileChooser fc = JFileChooserFactory.createJFileChooser(lastDirectorySelected);
		fc.setDialogTitle("Select Base Filename for List Files");
		SimpleFileFilter tff = new SimpleFileFilter("txt", "Text Files");
		fc.addChoosableFileFilter(tff);
		SimpleFileFilter csv_ff = new SimpleFileFilter("csv", "Comma-separated Values");
		fc.addChoosableFileFilter(csv_ff);
		fc.setFileFilter(csv_ff);
		fc.setDialogType(JFileChooser.SAVE_DIALOG);

		int retVal = fc.showSaveDialog(this.__editorJComponent);
		if (retVal != JFileChooser.APPROVE_OPTION)
		{
			return;
		}

		string currDir = (fc.getCurrentDirectory()).ToString();

		if (!currDir.Equals(lastDirectorySelected, StringComparison.OrdinalIgnoreCase))
		{
			JGUIUtil.setLastFileDialogDirectory(currDir);
		}
		string filename = fc.getSelectedFile().getPath();

		// Station types...

		int[] types = new int[] {-1, HydrologyNode.NODE_TYPE_FLOW, HydrologyNode.NODE_TYPE_DIV, HydrologyNode.NODE_TYPE_DIV_AND_WELL, HydrologyNode.NODE_TYPE_PLAN, HydrologyNode.NODE_TYPE_RES, HydrologyNode.NODE_TYPE_ISF, HydrologyNode.NODE_TYPE_WELL, HydrologyNode.NODE_TYPE_OTHER};

		/* TODO SAM 2006-01-03 Just use node abbreviations from network
		// Suffix for output, to be added to file basename...
	
		String[] nodetype_string = {
			"All",
			"StreamGage",
			"Diversion",
			"DiversionAndWell",
			"Plan",
			"Reservoir",
			"InstreamFlow",
			"Well",
			// TODO SAM 2006-01-03 Evaluate similar to node type above.
			//"StreamEstimate",
			"Other"
		};
		*/

		// Put the extension on the file (user may or may not have added)...

		if (fc.getFileFilter() == tff)
		{
			filename = IOUtil.enforceFileExtension(filename, "txt");
		}
		else if (fc.getFileFilter() == csv_ff)
		{
			filename = IOUtil.enforceFileExtension(filename, "csv");
		}

		// Now get the base name and remaining extension so that the basename can be adjusted below...

		int lastIndex = filename.LastIndexOf(".", StringComparison.Ordinal);
		string front = filename.Substring(0, lastIndex);
		string end = filename.Substring((lastIndex + 1), filename.Length - (lastIndex + 1));

		string outputFilename = null;
		IList<HydrologyNode> v = null;

		string warning = "";
		string[] comments = null;
		for (int i = 0; i < types.Length; i++)
		{
			v = this.__editorJComponent.getNodesForType(types[i]);

			if (v != null && v.Count > 0)
			{

				comments = new string[1];
				if (types[i] == -1)
				{
					comments[0] = "The following list contains data for all node types.";
					outputFilename = front + "_All." + end;
				}
				else
				{
					comments[0] = "The following list contains data for the following node type:  " + HydrologyNode.getTypeString(types[i], HydrologyNode.ABBREVIATION) +
						" (" + HydrologyNode.getTypeString(types[i], HydrologyNode.FULL) + ")";
					outputFilename = front + "_" + HydrologyNode.getTypeString(types[i], HydrologyNode.ABBREVIATION) + "." + end;
				}

				try
				{
					StateMod_NodeNetwork.writeListFile(outputFilename, ",", false, v, comments, false);
				}
				catch (Exception e)
				{
					Message.printWarning(3, routine, e);
					warning += "\nUnable to create list file \"" + outputFilename + "\"";
				}
			}
		}
		// TODO SAM 2006-01-03 Write at level 1 since this is currently triggered from an
		// interactive action.  However, may need to change if executed in batch mode.
		if (warning.Length > 0)
		{
			Message.printWarning(1, routine, warning);
		}
	}

	}

}