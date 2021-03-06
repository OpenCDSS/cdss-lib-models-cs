﻿using System;

// StateCU_DataSet_JTree - an object to display a StateCU data set in a JTree.

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

//-----------------------------------------------------------------------------
// StateCU_DataSet_JTree - an object to display a StateCU data set in a JTree.
//-----------------------------------------------------------------------------
// History:
//
// 2003-06-30	Steven A. Malers, RTi	Created class.
// 2003-07-13	SAM, RTi		Updated to handle
//					RTi.Util.IO.DataSetComponent being used
//					for components.
// 2003-07-16	J. Thomas Sapienza, RTi	Added call to setFastAdd() to speed
//					up tree performance.
//					Add ability to handle events on the
//					nodes.
// 2003-07-21	SAM, RTi		* Add call to setTreeTextEditable(false)
//					  to make the tree not be editable.
//					* Move back to the more generic
//					  SimpleJTree_Node to handle more
//					  specific issues with popups.
// 2003-08-04	JTS, RTi		Group components now display an icon
//					if they have no data.
// 2004-02-19	SAM, RTi		* Display the control group when not
//					  showing details - need a way to get
//					  to the properties.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
//-----------------------------------------------------------------------------

namespace DWR.StateCU
{


	using StateMod_Data = DWR.StateMod.StateMod_Data;
	using StateMod_DelayTable = DWR.StateMod.StateMod_DelayTable;
	using SimpleJMenuItem = RTi.Util.GUI.SimpleJMenuItem;
	using SimpleJTree = RTi.Util.GUI.SimpleJTree;
	using SimpleJTree_Node = RTi.Util.GUI.SimpleJTree_Node;
	using DataSetComponent = RTi.Util.IO.DataSetComponent;
	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// This StateCU_DataSet_JTree class displays a StateCU_DataSet and its components
	/// in a JTree.  It can be constructed to show all the data, or just the high-level
	/// objects.
	/// </summary>
	public class StateCU_DataSet_JTree : SimpleJTree, ActionListener, MouseListener
	{

	/// <summary>
	/// The icon that will be used for group components that have no data.
	/// </summary>
	private Icon __folderIcon;

	private SimpleJTree_Node __popup_Node; // The SimpleJTree_Node that was
							// selected for the popup menu.

	private JPopupMenu __popup_JPopupMenu; // A single popup menu that is
							// used to provide access to
							// other features from the tree.
							// The single menu has its
							// items added/removed as
							// necessary based on the state
							// of the tree.

	private StateCU_DataSet __dataset = null; // Data set to be displayed
	private bool __display_data_objects = false; // Indicates whether data
							// objects should be listed in
							// the tree (true) or only the
							// top-level components (false).

	/// <summary>
	/// If true then the component is being used with a full StateCU data set.  If false, it is being used
	/// with the StateMod GUI (CU Locations only).
	/// </summary>
	private bool __isStateCU = true;

	/// <summary>
	/// Construct a StateCU_DataSet_JFrame. </summary>
	/// <param name="parent"> JFrame from which this instance is constructed. </param>
	/// <param name="dataset"> StateCU_DataSet that is being displayed/managed. </param>
	/// <param name="display_data_objects"> If true, data objects are listed in the tree.  If
	/// false, only the top-level data set components are listed. </param>
	/// <param name="isStateCU"> indicates whether the display is being used for a full StateCU data set (true) or
	/// StateMod GUI (false). </param>
	public StateCU_DataSet_JTree(JFrame parent, StateCU_DataSet dataset, bool display_data_objects, bool isStateCU)
	{
		__dataset = dataset;
		__display_data_objects = display_data_objects;
		__isStateCU = isStateCU;
		__folderIcon = getClosedIcon();
		showRootHandles(true);
		addMouseListener(this);
		setLeafIcon(null);
		setTreeTextEditable(false);
		__popup_JPopupMenu = new JPopupMenu();
	}

	/// <summary>
	/// Construct a StateCU_DataSet_JFrame.  The data set should be set when available
	/// using setDataSet(). </summary>
	/// <param name="parent"> JFrame from which this instance is constructed. </param>
	/// <param name="display_data_objects"> If true, data objects are listed in the tree.  If
	/// false, only the top-level data set components are listed. </param>
	public StateCU_DataSet_JTree(JFrame parent, bool display_data_objects)
	{
		__display_data_objects = display_data_objects;
		__folderIcon = getClosedIcon();
		showRootHandles(true);
		addMouseListener(this);
		setLeafIcon(null);
		setTreeTextEditable(false);
		__popup_JPopupMenu = new JPopupMenu();
	}

	/// <summary>
	/// Set the data set to be displayed. </summary>
	/// <param name="dataset"> StateCU_DataSet that is being displayed/managed. </param>
	public virtual void setDataSet(StateCU_DataSet dataset)
	{
		__dataset = dataset;
		__popup_JPopupMenu = new JPopupMenu();
	}

	/// <summary>
	/// Clear all data from the tree.
	/// </summary>
	public virtual void clear()
	{
		string routine = "StateCU_DataSet_JTree.clear";
		SimpleJTree_Node node = getRoot();
		System.Collections.IList v = getChildrenList(node);
		int size = 0;
		if (v != null)
		{
			size = v.Count;
		}
		for (int i = 0; i < size; i++)
		{
			try
			{
				removeNode((SimpleJTree_Node)v[i], false);
			}
			catch (Exception e)
			{
				Message.printWarning(2, routine, "Cannot remove node " + node.ToString());
				Message.printWarning(2, routine, e);
			}
		}
	}

	/// <summary>
	/// Display all the information in the data set.  This can be called, for example,
	/// after a data set has been read.
	/// </summary>
	public virtual void displayDataSet()
	{
		string routine = "StateCU_DataSet_JTree.displayDataSet";
		System.Collections.IList v = __dataset.getComponentGroups();
		int size = 0;
		if (v != null)
		{
			size = v.Count;
		}
		SimpleJTree_Node node = null, node2 = null;
		DataSetComponent comp = null;
		string name = "";
		bool isGroup = false;
		bool hasData = false;
		int type;
		// Add each component group...
		setFastAdd(true);
		for (int i = 0; i < size; i++)
		{
			isGroup = false;
			hasData = false;
			comp = (DataSetComponent)v[i];
			if ((comp == null) || !comp.isVisible())
			{
				continue;
			}

			if (comp.isGroup())
			{
				isGroup = true;
			}

			type = comp.getComponentType();
			// Show the control data at the high level.
			if (type == StateCU_DataSet.COMP_GIS_GROUP)
			{
				// Don't want to list the GIS data...
				continue;
			}
			if ((type == StateCU_DataSet.COMP_GIS_GROUP) && __display_data_objects)
			{
				// Currently don't want to list GIS data in
				// results...
				continue;
			}
			node = new SimpleJTree_Node(comp.getComponentName());
			node.setData(comp);
			try
			{
				addNode(node);
			}
			catch (Exception e)
			{
				Message.printWarning(2, routine, "Error adding component group " + comp.getComponentName());
				Message.printWarning(2, routine, e);
				continue;
			}
			if (__display_data_objects)
			{
				// Display the primary object in each group
				int primary_type = __dataset.lookupPrimaryComponentTypeForComponentGroup(comp.getComponentType());
				if (primary_type >= 0)
				{
					comp = __dataset.getComponentForComponentType(primary_type);
				}
				if ((comp == null) || !comp.isVisible())
				{
					continue;
				}
				object data_Object = comp.getData();
				if (data_Object == null)
				{
					continue;
				}
				System.Collections.IList data = null;
				if (data_Object is System.Collections.IList)
				{
					data = (System.Collections.IList)comp.getData();
				}
				else
				{ // Continue (REVISIT - what components would
					// this happen for?)...
					Message.printWarning(2, routine, "Unexpected non-Vector for " + comp.getComponentName());
				}
				StateCU_Data cudata;
				StateMod_Data smdata;
				int dsize = 0;
				if (data != null)
				{
					dsize = data.Count;
				}
				for (int idata = 0; idata < dsize; idata++)
				{
					if (comp.getComponentType() == StateCU_DataSet.COMP_DELAY_TABLES_MONTHLY)
					{
						// StateMod data object so have to
						// handle separately because StateCU
						// uses the StateMod group...
						smdata = (StateMod_Data) data[idata];
						name = smdata.getName();
						node2 = new SimpleJTree_Node(name);
						node2.setData(smdata);
					}
					else
					{ // StateCU data object...
						cudata = (StateCU_Data) data[idata];
						name = cudata.getName();
						node2 = new SimpleJTree_Node(name);
						node2.setData(cudata);
					}
					try
					{
						addNode(node2, node);
					}
					catch (Exception e)
					{
						Message.printWarning(2, routine, "Error adding data " + name);
						Message.printWarning(2, routine, e);
						continue;
					}
				}
				if (dsize > 0)
				{
					hasData = true;
				}
				// Collapse the node because the lists are
				// usually pretty long...
				try
				{
					collapseNode(node);
				}
				catch (Exception)
				{
					// Ignore.
				}
			}
			else
			{ // Add the components in the group...
	Message.printStatus(1, "", "Not displaying data objects");
				System.Collections.IList v2 = (System.Collections.IList)comp.getData();
				int size2 = 0;
				if (v2 != null)
				{
					size2 = v2.Count;
				}
	Message.printStatus(1, "", "group has " + size2 + " subcomponents");
				for (int j = 0; j < size2; j++)
				{
					comp = (DataSetComponent)v2[j];
					if (!comp.isVisible())
					{
						continue;
					}
					node2 = new SimpleJTree_Node(comp.getComponentName());
					node2.setData(comp);
					try
					{
						addNode(node2, node);
					}
					catch (Exception e)
					{
						Message.printWarning(2, routine, "Error adding component " + comp.getComponentName());
						Message.printWarning(2, routine, e);
						continue;
					}
				}
				if (size2 > 0)
				{
					hasData = true;
				}
			}

			if (isGroup && !hasData)
			{
				node.setIcon(__folderIcon);
			}
		}
		setFastAdd(false);
	}

	/// <summary>
	/// Handle action events from the popup menu. </summary>
	/// <param name="e"> ActionEvent to handle. </param>
	public virtual void actionPerformed(ActionEvent e)
	{
		object data = __popup_Node.getData();
		bool editable = false;
		if (data is DataSetComponent)
		{
			DataSetComponent comp = (DataSetComponent)data;
			int comp_type = comp.getComponentType();
			if (comp_type == StateCU_DataSet.COMP_CLIMATE_STATIONS_GROUP)
			{
				new StateCU_ClimateStation_JFrame("Climate Stations", __dataset, editable);
			}
			else if (comp_type == StateCU_DataSet.COMP_CROP_CHARACTERISTICS_GROUP)
			{
				new StateCU_CropCharacteristics_JFrame(__dataset, editable);
			}
			else if (comp_type == StateCU_DataSet.COMP_DELAY_TABLES_GROUP)
			{
				DataSetComponent comp2 = __dataset.getComponentForComponentType(StateCU_DataSet.COMP_DELAY_TABLES_MONTHLY);
				if (comp2 != null)
				{
				/*
					REVISIT (JTS - 2003-10-06)
					old constructor
					new StateMod_DelayTable_JFrame(
					"Delay Tables", (Vector)comp2.getData(),
					true, editable);
				*/
				}
			}
			else if (comp_type == StateCU_DataSet.COMP_CU_LOCATIONS_GROUP)
			{
				// TODO SAM 2011-06-22 Need a way to pass a window manager
				new StateCU_Location_JFrame(__isStateCU,__dataset, null, editable);
			}
		}
		// Else, data are specific objects...
		else if (data is StateCU_ClimateStation)
		{
			new StateCU_ClimateStation_JFrame("Climate Stations", __dataset, (StateCU_ClimateStation)data, editable);
		}
		else if (data is StateCU_CropCharacteristics)
		{
			new StateCU_CropCharacteristics_JFrame(__dataset, (StateCU_CropCharacteristics)data, editable);
		}
		else if (data is StateMod_DelayTable)
		{
			DataSetComponent comp2 = __dataset.getComponentForComponentType(StateCU_DataSet.COMP_DELAY_TABLES_MONTHLY);
			if (comp2 != null)
			{
				/*
				REVISIT (JTS - 2003-10-06)			
				old constructor
				new StateMod_DelayTable_JFrame(
				"Delay Tables", (Vector)comp2.getData(),
				(StateMod_DelayTable)data, true, editable);
				*/
			}
		}
		else if (data is StateCU_Location)
		{
			new StateCU_Location_JFrame(false, __dataset, null, (StateCU_Location)data, editable);
		}
	}

	/// <summary>
	/// Responds to mouse clicked events; does nothing. </summary>
	/// <param name="event"> the MouseEvent that happened. </param>
	public virtual void mouseClicked(MouseEvent @event)
	{
	}

	/// <summary>
	/// Responds to mouse dragged events; does nothing. </summary>
	/// <param name="event"> the MouseEvent that happened. </param>
	public virtual void mouseDragged(MouseEvent @event)
	{
	}

	/// <summary>
	/// Responds to mouse entered events; does nothing. </summary>
	/// <param name="event"> the MouseEvent that happened. </param>
	public virtual void mouseEntered(MouseEvent @event)
	{
	}

	/// <summary>
	/// Responds to mouse exited events; does nothing. </summary>
	/// <param name="event"> the MouseEvent that happened. </param>
	public virtual void mouseExited(MouseEvent @event)
	{
	}

	/// <summary>
	/// Responds to mouse moved events; does nothing. </summary>
	/// <param name="event"> the MouseEvent that happened. </param>
	public virtual void mouseMoved(MouseEvent @event)
	{
	}

	/// <summary>
	/// Responds to mouse pressed events; does nothing. </summary>
	/// <param name="event"> the MouseEvent that happened. </param>
	public virtual void mousePressed(MouseEvent @event)
	{
	}

	/// <summary>
	/// Responds to mouse released events. </summary>
	/// <param name="event"> the MouseEvent that happened. </param>
	public virtual void mouseReleased(MouseEvent @event)
	{
		showPopupMenu(@event);
	}

	/// <summary>
	/// Checks to see if the mouse event would trigger display of the popup menu.
	/// The popup menu does not display if it is null. </summary>
	/// <param name="e"> the MouseEvent that happened. </param>
	private void showPopupMenu(MouseEvent e)
	{
		if (!e.isPopupTrigger() || !__display_data_objects)
		{
			// Do not do anything...
			return;
		}
		// Figure out which node is selected...
		TreePath path = getPathForLocation(e.getX(), e.getY());
		if (path == null)
		{
			return;
		}
		__popup_Node = (SimpleJTree_Node)path.getLastPathComponent();
		// First remove the menu items that are currently in the menu...
		__popup_JPopupMenu.removeAll();
		object data = null; // Data object associated with the node
		// Now reset the popup menu based on the selected node...
		if (__display_data_objects)
		{
			// Get the data for the node.  If the node is a data object,
			// the type can be checked to know what to display.
			// The tree is displaying data objects so the popup will show
			// specific JFrames for each data group.  If the group folder
			// was selected, then display the JFrame showing the first item
			// selected.  If a specific data item in the group was selected,
			// then show the specific data item.
			JMenuItem item;
			data = __popup_Node.getData();
			if (data is DataSetComponent)
			{
				// Specific checks need to be done to identify the
				// component...
				DataSetComponent comp = (DataSetComponent)data;
				int comp_type = comp.getComponentType();
				if (comp_type == StateCU_DataSet.COMP_CLIMATE_STATIONS_GROUP)
				{
					item = new SimpleJMenuItem("Climate Stations Properties", "Climate Stations Properties", this);
					__popup_JPopupMenu.add(item);
				}
				else if (comp_type == StateCU_DataSet.COMP_CROP_CHARACTERISTICS_GROUP)
				{
					item = new SimpleJMenuItem("Crop Properties", "Crop Properties", this);
					__popup_JPopupMenu.add(item);
				}
				else if (comp_type == StateCU_DataSet.COMP_DELAY_TABLES_GROUP)
				{
					item = new SimpleJMenuItem("Delay Tables Properties", "Delay Tables Properties", this);
					__popup_JPopupMenu.add(item);
				}
				else if (comp_type == StateCU_DataSet.COMP_CU_LOCATIONS_GROUP)
				{
					item = new SimpleJMenuItem("CU Locations Properties", "CU Locations Properties", this);
					__popup_JPopupMenu.add(item);
				}
			}
			// Check specific instances of the primary data object...
			else if (data is StateCU_ClimateStation)
			{
				item = new SimpleJMenuItem(__popup_Node.getText() + " Properties", __popup_Node.getText() + " Properties", this);
				__popup_JPopupMenu.add(item);
			}
			else if (data is StateCU_CropCharacteristics)
			{
				item = new SimpleJMenuItem(__popup_Node.getText() + " Properties", __popup_Node.getText() + " Properties", this);
				__popup_JPopupMenu.add(item);
			}
			else if (data is StateMod_DelayTable)
			{
				item = new SimpleJMenuItem(__popup_Node.getText() + " Properties", __popup_Node.getText() + " Properties", this);
				__popup_JPopupMenu.add(item);
			}
			else if (data is StateCU_Location)
			{
				item = new SimpleJMenuItem(__popup_Node.getText() + " Properties", __popup_Node.getText() + " Properties", this);
				__popup_JPopupMenu.add(item);
			}
			else
			{
				return;
			}
		}
		// Now display the popup so that the user can select the appropriate
		// menu item...
		__popup_JPopupMenu.show(e.getComponent(), e.getX(), e.getY());
	}

	} // End StateCU_DataSet_JTree

}