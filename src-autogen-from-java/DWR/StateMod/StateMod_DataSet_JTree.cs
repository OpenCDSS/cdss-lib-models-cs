using System;
using System.Collections.Generic;

// StateMod_DataSet_JTree - an object to display a StateMod data set in a JTree.

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
// StateMod_DataSet_JTree - an object to display a StateMod data set in a JTree.
//-----------------------------------------------------------------------------
// History:
// 2003-07-15	J. Thomas Sapienza, RTi	Initial version from 
//					StateMod_DataSet_JTree
// 2003-07-16	JTS, RTi		Added code to open specific forms based
//					on node selections.
// 2003-07-30	Steven A. Malers, RTi	* Change COMP_STREAM_STATIONS to
//					  COMP_RIVER_STATIONS.
//					* Remove use of
//					  StateMod_DataSet_JTree_Node, as per
//					  the StateCU package - the same
//					  functionality can be achieved without
//					  the extra class.
//					* The reworked code reuses one popup
//					  rather than having a popup on each
//					  node - much less overhead.
//					* Change name of maybeShowPopup() to
//					  showPopupMenu().
// 2003-08-04	JTS, RTi		Folder icons now display for group
//					components even if they have no data.
// 2003-08-14	SAM, RTi		* Label the nodes with the identifier
//					  and description/name.
//					* For now assume monthly delay tables -
//					  need to handle daily delay tables.
// 2003-08-26	SAM, RTi		* Enable StateMod_DataSet_WindowManager.
//					* Enable river station, river baseflow
//					  station, and network.
//					* Add editable parameter to the
//					  constructor.
// 2003-08-29	SAM, RTi		Update for separate daily and monthly
//					delay table groups.
// 2003-09-11	SAM, RTi		Update due to changes in the naming
//					of river station components.
// 2003-09-18	SAM, RTi		Change so popup menus are not shown
//					if a group's primary component does
//					not have data.
// 2003-09-29	SAM, RTi		* Move formatNodeLabel() to
//					  StateMod_Util.formatDataLabel().
//					* Add refresh() and
//					  displayDataSetComponent() for use with
//					  adding and deleting data.
//					* Add to the popup a "Summarize use in
//					  data set" option to help
//					  users/developers understand the
//					  relationships between data.
// 2006-03-06	JTS, RTi		When diversions are summarized, the
//					main diversions window is no longer 
//					opened.
// 2006-03-07	JTS, RTi		When refreshing the tree, fast add is
//					used.
// 2006-08-22	SAM, RTi		Add plans.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
//-----------------------------------------------------------------------------
// EndHeader

namespace DWR.StateMod
{


	using StateCU_Data = DWR.StateCU.StateCU_Data;
	using TSViewJFrame = RTi.GRTS.TSViewJFrame;
	using MonthTS = RTi.TS.MonthTS;
	using TS = RTi.TS.TS;
	using JGUIUtil = RTi.Util.GUI.JGUIUtil;
	using ReportJFrame = RTi.Util.GUI.ReportJFrame;
	using SimpleJMenuItem = RTi.Util.GUI.SimpleJMenuItem;
	using SimpleJTree = RTi.Util.GUI.SimpleJTree;
	using SimpleJTree_Node = RTi.Util.GUI.SimpleJTree_Node;
	using DataSetComponent = RTi.Util.IO.DataSetComponent;
	using PropList = RTi.Util.IO.PropList;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// This StateMod_DataSet_JTree class displays a StateMod_DataSet and its components
	/// in a JTree.  It can be constructed to show all the data, or just the high-level objects.
	/// </summary>
	public class StateMod_DataSet_JTree : SimpleJTree, ActionListener, MouseListener
	{

	private readonly string __SUMMARIZE_HOW1 = "Summarize how ";
	private readonly string __SUMMARIZE_HOW2 = " is used";
						// String used in popup menus - checked
						// for in actionPerformed().
	private readonly string __PROPERTIES = " Properties";
						// String used in popup menus - checked
						// for in actionPerformed().

	/// <summary>
	/// Whether data objects should be listed in the tree (true) or only the 
	/// top-level components should be (false).
	/// </summary>
	private bool __display_data_objects = false;

	/// <summary>
	/// Whether the data in the tree are editable or not.  For example, StateDMI will
	/// set as false but StateMod GUI will set as true.
	/// </summary>
	private bool __editable = false;

	/// <summary>
	/// Stores the folder icon to be used for group components with no data.
	/// </summary>
	private Icon __folderIcon = null;

	private JPopupMenu __popup_JPopupMenu; // A single popup menu that is
							// used to provide access to
							// other features from the tree.
							// The single menu has its
							// items added/removed as
							// necessary based on the state
							// of the tree.

	/// <summary>
	/// The dataset to be displayed.
	/// </summary>
	private StateMod_DataSet __dataset = null;

	/// <summary>
	/// Data set window manager.
	/// </summary>
	private StateMod_DataSet_WindowManager __dataset_wm;

	/// <summary>
	/// The node that last opened a popup menu.
	/// </summary>
	private SimpleJTree_Node __popup_Node;

	/// <summary>
	/// Construct a StateMod_DataSet_JFrame. </summary>
	/// <param name="parent"> JFrame from which this instance is constructed. </param>
	/// <param name="dataset"> StateMod_DataSet that is being displayed/managed. </param>
	/// <param name="dataset_wm"> the dataset window manager or null if the data set windows are not being managed. </param>
	/// <param name="display_data_objects"> If true, data objects are listed in the tree.  If </param>
	/// <param name="editable"> If true, data objects can be edited.
	/// false, only the top-level data set components are listed. </param>
	public StateMod_DataSet_JTree(JFrame parent, StateMod_DataSet dataset, StateMod_DataSet_WindowManager dataset_wm, bool display_data_objects, bool editable)
	{
		__dataset = dataset;
		__dataset_wm = dataset_wm;
		__display_data_objects = display_data_objects;
		__editable = editable;

		__folderIcon = getClosedIcon();

		showRootHandles(true);
		addMouseListener(this);
		setLeafIcon(null);
		setTreeTextEditable(false);
		__popup_JPopupMenu = new JPopupMenu();
	}

	/// <summary>
	/// Construct a StateMod_DataSet_JFrame.  The data set should be set when available
	/// using setDataSet() and setDataSetWindowManager(). </summary>
	/// <param name="parent"> JFrame from which this instance is constructed. </param>
	/// <param name="display_data_objects"> If true, data objects are listed in the tree.  If
	/// false, only the top-level data set components are listed. </param>
	/// <param name="editable"> If true, data objects can be edited. </param>
	public StateMod_DataSet_JTree(JFrame parent, bool display_data_objects, bool editable)
	{
		__display_data_objects = display_data_objects;
		__editable = editable;
		__folderIcon = getClosedIcon();

		showRootHandles(true);
		addMouseListener(this);
		setLeafIcon(null);
		setTreeTextEditable(false);
		__popup_JPopupMenu = new JPopupMenu();
	}

	/// <summary>
	/// Set the data set to be displayed. </summary>
	/// <param name="dataset"> StateMod_DataSet that is being displayed/managed. </param>
	public virtual void setDataSet(StateMod_DataSet dataset)
	{
		__dataset = dataset;
	}

	/// <summary>
	/// Set the data set window manager. </summary>
	/// <param name="dataset_wm"> StateMod_DataSet_WindowManager that is being used to
	/// display/manage the data set. </param>
	public virtual void setDataSetWindowManager(StateMod_DataSet_WindowManager dataset_wm)
	{
		__dataset_wm = dataset_wm;
	}

	/// <summary>
	/// Responds to action performed events sent by popup menus of the tree nodes. </summary>
	/// <param name="event"> the ActionEvent that happened. </param>
	public virtual void actionPerformed(ActionEvent @event)
	{
		string action = @event.getActionCommand();
		string routine = "StateMod_DataSet_JTree.actionPerformed";

		object data = __popup_Node.getData();

		if (data is DataSetComponent)
		{
			DataSetComponent comp = (DataSetComponent)data;
			int comp_type = comp.getComponentType();
			if (comp_type == StateMod_DataSet.COMP_CONTROL_GROUP)
			{
				__dataset_wm.displayWindow(StateMod_DataSet_WindowManager.WINDOW_CONTROL, __editable);
			}
			else if (comp_type == StateMod_DataSet.COMP_STREAMGAGE_GROUP)
			{
				__dataset_wm.displayWindow(StateMod_DataSet_WindowManager.WINDOW_STREAMGAGE, __editable);
			}
			else if (comp_type == StateMod_DataSet.COMP_DELAY_TABLE_MONTHLY_GROUP)
			{
				__dataset_wm.displayWindow(StateMod_DataSet_WindowManager.WINDOW_DELAY_TABLE_MONTHLY, __editable);
			}
			else if (comp_type == StateMod_DataSet.COMP_DELAY_TABLE_DAILY_GROUP)
			{
				__dataset_wm.displayWindow(StateMod_DataSet_WindowManager.WINDOW_DELAY_TABLE_DAILY, __editable);
			}
			else if (comp_type == StateMod_DataSet.COMP_DIVERSION_GROUP)
			{
				__dataset_wm.displayWindow(StateMod_DataSet_WindowManager.WINDOW_DIVERSION, __editable);
			}
			else if (comp_type == StateMod_DataSet.COMP_PRECIPITATION_GROUP)
			{
				__dataset_wm.displayWindow(StateMod_DataSet_WindowManager.WINDOW_PRECIPITATION, __editable);
			}
			else if (comp_type == StateMod_DataSet.COMP_EVAPORATION_GROUP)
			{
				__dataset_wm.displayWindow(StateMod_DataSet_WindowManager.WINDOW_EVAPORATION, __editable);
			}
			else if (comp_type == StateMod_DataSet.COMP_RESERVOIR_GROUP)
			{
				__dataset_wm.displayWindow(StateMod_DataSet_WindowManager.WINDOW_RESERVOIR, __editable);
			}
			else if (comp_type == StateMod_DataSet.COMP_INSTREAM_GROUP)
			{
				__dataset_wm.displayWindow(StateMod_DataSet_WindowManager.WINDOW_INSTREAM, __editable);
			}
			else if (comp_type == StateMod_DataSet.COMP_WELL_GROUP)
			{
				__dataset_wm.displayWindow(StateMod_DataSet_WindowManager.WINDOW_WELL, __editable);
			}
			else if (comp_type == StateMod_DataSet.COMP_PLAN_GROUP)
			{
				__dataset_wm.displayWindow(StateMod_DataSet_WindowManager.WINDOW_PLAN, __editable);
			}
			else if (comp_type == StateMod_DataSet.COMP_STREAMESTIMATE_GROUP)
			{
				__dataset_wm.displayWindow(StateMod_DataSet_WindowManager.WINDOW_STREAMESTIMATE, __editable);
			}
			else if (comp_type == StateMod_DataSet.COMP_RIVER_NETWORK_GROUP)
			{
				__dataset_wm.displayWindow(StateMod_DataSet_WindowManager.WINDOW_RIVER_NETWORK, __editable);
			}
			else if (comp_type == StateMod_DataSet.COMP_OPERATION_GROUP)
			{
				__dataset_wm.displayWindow(StateMod_DataSet_WindowManager.WINDOW_OPERATIONAL_RIGHT, __editable);
			}
		}
		// Below here are specific instances of objects.  Similar to above,
		// display the main window but then also select the specific object...
		else if (data is StateMod_StreamGage)
		{
			__dataset_wm.displayWindow(StateMod_DataSet_WindowManager.WINDOW_STREAMGAGE, __editable);
			((StateMod_StreamGage_JFrame)__dataset_wm.getWindow(StateMod_DataSet_WindowManager.WINDOW_STREAMGAGE)).selectID(((StateMod_StreamGage)data).getID());
		}
		else if (data is StateMod_DelayTable)
		{
			StateMod_DelayTable dt = (StateMod_DelayTable)data;
			if (dt.isMonthly())
			{
				__dataset_wm.displayWindow(StateMod_DataSet_WindowManager.WINDOW_DELAY_TABLE_MONTHLY, __editable);
					((StateMod_DelayTable_JFrame)__dataset_wm.getWindow(StateMod_DataSet_WindowManager.WINDOW_DELAY_TABLE_MONTHLY)).selectID(dt.getID());
			}
			else
			{
				__dataset_wm.displayWindow(StateMod_DataSet_WindowManager.WINDOW_DELAY_TABLE_DAILY, __editable);
					((StateMod_DelayTable_JFrame)__dataset_wm.getWindow(StateMod_DataSet_WindowManager.WINDOW_DELAY_TABLE_DAILY)).selectID(dt.getID());
			}
		}
		else if (data is StateMod_Diversion)
		{
			if (action.IndexOf(__SUMMARIZE_HOW1, StringComparison.Ordinal) >= 0)
			{
				PropList props = new PropList("Diversion");
				props.set("Title=" + ((StateMod_Diversion)data).getID() + " Diversion use in Data Set");
				new ReportJFrame(__dataset.getDataObjectDetails(StateMod_DataSet.COMP_DIVERSION_STATIONS,((StateMod_Diversion)data).getID()), props);
			}
			else
			{
				// Assume properties...
				__dataset_wm.displayWindow(StateMod_DataSet_WindowManager.WINDOW_DIVERSION,__editable);
				((StateMod_Diversion_JFrame)__dataset_wm.getWindow(StateMod_DataSet_WindowManager.WINDOW_DIVERSION)).selectID(((StateMod_Diversion)data).getID());
			}
		}
		else if (data is TS)
		{
			// Might be precipitation or evaporation.  Check the data type to determine...
			TS ts = (TS)data;
			PropList props = new PropList("Precipitation/Evaporation");
			if (action.IndexOf(__SUMMARIZE_HOW1, StringComparison.Ordinal) >= 0)
			{
				if (StringUtil.startsWithIgnoreCase(ts.getDataType(),"e"))
				{
					props.set("Title=" + ts.getLocation() + " Evaporation TS use in Data Set");
					new ReportJFrame(__dataset.getDataObjectDetails(StateMod_DataSet.COMP_EVAPORATION_TS_MONTHLY, ts.getLocation()), props);
				}
				else if (StringUtil.startsWithIgnoreCase(ts.getDataType(),"p"))
				{
					props.set("Title=" + ts.getLocation() + " Precipitation TS use in Data Set");
					new ReportJFrame(__dataset.getDataObjectDetails(StateMod_DataSet.COMP_PRECIPITATION_TS_MONTHLY, ts.getLocation()), props);
				}
			}
			else if (action.IndexOf(__PROPERTIES, StringComparison.Ordinal) >= 0)
			{
				if (StringUtil.startsWithIgnoreCase(ts.getDataType(),"e"))
				{
					props.set("Title=Evaporation");
				}
				else if (StringUtil.startsWithIgnoreCase(ts.getDataType(),"p"))
				{
					props.set("Title=Precipitation");
				}
				props.set("InitialView=Graph");
				props.set("GraphType=Bar");
				System.Collections.IList tslist = new List<object>(1);
				tslist.Add(ts);
				try
				{
					new TSViewJFrame(tslist, props);
				}
				catch (Exception)
				{
					Message.printWarning(1, routine, "Error displaying data.");
				}
			}
		}
		else if (data is StateMod_Reservoir)
		{
			__dataset_wm.displayWindow(StateMod_DataSet_WindowManager.WINDOW_RESERVOIR, __editable);
			((StateMod_Reservoir_JFrame)__dataset_wm.getWindow(StateMod_DataSet_WindowManager.WINDOW_RESERVOIR)).selectID(((StateMod_Reservoir)data).getID());
		}
		else if (data is StateMod_InstreamFlow)
		{
			__dataset_wm.displayWindow(StateMod_DataSet_WindowManager.WINDOW_INSTREAM, __editable);
			((StateMod_InstreamFlow_JFrame)__dataset_wm.getWindow(StateMod_DataSet_WindowManager.WINDOW_INSTREAM)).selectID(((StateMod_InstreamFlow)data).getID());
		}
		else if (data is StateMod_Well)
		{
			__dataset_wm.displayWindow(StateMod_DataSet_WindowManager.WINDOW_WELL, __editable);
			((StateMod_Well_JFrame)__dataset_wm.getWindow(StateMod_DataSet_WindowManager.WINDOW_WELL)).selectID(((StateMod_Well)data).getID());
		}
		else if (data is StateMod_Plan)
		{
			__dataset_wm.displayWindow(StateMod_DataSet_WindowManager.WINDOW_PLAN, __editable);
			((StateMod_Plan_JFrame)__dataset_wm.getWindow(StateMod_DataSet_WindowManager.WINDOW_PLAN)).selectID(((StateMod_Plan)data).getID());
		}
		else if (data is StateMod_StreamEstimate)
		{
			__dataset_wm.displayWindow(StateMod_DataSet_WindowManager.WINDOW_STREAMESTIMATE, __editable);
			((StateMod_StreamEstimate_JFrame)__dataset_wm.getWindow(StateMod_DataSet_WindowManager.WINDOW_STREAMESTIMATE)).selectID(((StateMod_StreamEstimate)data).getID());
		}
		else if (data is StateMod_RiverNetworkNode)
		{
			__dataset_wm.displayWindow(StateMod_DataSet_WindowManager.WINDOW_RIVER_NETWORK, __editable);
			((StateMod_RiverNetworkNode_JFrame)__dataset_wm.getWindow(StateMod_DataSet_WindowManager.WINDOW_RIVER_NETWORK)).selectID(((StateMod_RiverNetworkNode)data).getID());
		}
		else if (data is StateMod_OperationalRight)
		{
			__dataset_wm.displayWindow(StateMod_DataSet_WindowManager.WINDOW_OPERATIONAL_RIGHT, __editable);
			((StateMod_OperationalRight_JFrame)__dataset_wm.getWindow(StateMod_DataSet_WindowManager.WINDOW_OPERATIONAL_RIGHT)).selectID(((StateMod_OperationalRight)data).getID());
		}
	}

	/// <summary>
	/// Clear all data from the tree.
	/// </summary>
	public virtual void clear()
	{
		string routine = "StateMod_DataSet_JTree.clear";
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
		string routine = "StateMod_DataSet_JTree.displayDataSet";
		System.Collections.IList v = __dataset.getComponentGroups();
		int size = 0;
		if (v != null)
		{
			size = v.Count;
		}
		SimpleJTree_Node node = null, node2 = null;
		DataSetComponent comp = null;
		bool hadData = false;
		bool isGroup = false;
		int type;
		// Add each component group...
		setFastAdd(true);
		Icon folder_Icon = getClosedIcon();
		for (int i = 0; i < size; i++)
		{
			hadData = false;
			isGroup = false;
			comp = (DataSetComponent)v[i];
			if ((comp == null) || !comp.isVisible())
			{
				continue;
			}
			type = comp.getComponentType();
			if (type == StateMod_DataSet.COMP_GEOVIEW_GROUP)
			{
				// Don't want to list the groups because there is no
				// way to display edit (or they are displayed elsewhere)...
				continue;
			}
			node = new SimpleJTree_Node(comp.getComponentName());
			node.setData(comp);

			if (comp.isGroup())
			{
				isGroup = true;
			}

			// To force groups to be folders, even if no data underneath...
			node.setIcon(folder_Icon);
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
				hadData = displayDataSetComponent(comp, node);
			}
			else
			{
				// Add the components in the group...
				System.Collections.IList v2 = (System.Collections.IList)comp.getData();
				int size2 = 0;
				if (v2 != null)
				{
					size2 = v2.Count;
				}
				for (int j = 0; j < size2; j++)
				{
					comp = (DataSetComponent)v2[j];
					if ((comp == null) || !comp.isVisible())
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
					hadData = true;
				}
			}
			if (isGroup && !hadData)
			{
				node.setIcon(__folderIcon);
			}
		}
		setFastAdd(false);
	}

	/// <summary>
	/// Display the primary data for a component.  This method is called when adding nodes under a group node. </summary>
	/// <param name="comp"> Component to display data. </param>
	/// <param name="node"> Parent node to display under. </param>
	private bool displayDataSetComponent(DataSetComponent comp, SimpleJTree_Node node)
	{
		string routine = "StateMod_DataSet_JTree.displayDataSetComponent";
		bool hadData = false; // No data for component...
		string label = "";
		int primary_type = __dataset.lookupPrimaryComponentTypeForComponentGroup(comp.getComponentType());
		if (primary_type >= 0)
		{
			comp = __dataset.getComponentForComponentType(primary_type);
		}
		// Revisit - later may enable even if a component does
		// not have data - for example have an "Add" popup...
		if ((comp == null) || !comp.isVisible() || !comp.hasData())
		{
			return hadData;
		}
		object data_Object = comp.getData();
		if (data_Object == null)
		{
			return hadData;
		}
		System.Collections.IList data = null;
		if (data_Object is System.Collections.IList)
		{
			data = (System.Collections.IList)comp.getData();
		}
		else
		{
			// Continue (REVISIT - what components would this happen for?)...
			Message.printWarning(2, routine, "Unexpected non-Vector for " + comp.getComponentName());
			return hadData;
		}
		StateCU_Data cudata;
		StateMod_Data smdata;
		SimpleJTree_Node node2 = null;
		TS tsdata;
		int dsize = 0;
		if (data != null)
		{
			dsize = data.Count;
		}
		for (int idata = 0; idata < dsize; idata++)
		{
			data_Object = data[idata];
			if (data_Object is StateMod_Data)
			{
				smdata = (StateMod_Data)data[idata];
				label = StateMod_Util.formatDataLabel(smdata.getID(), smdata.getName());
				node2 = new SimpleJTree_Node(label);
				node2.setData(smdata);
			}
			else if (data_Object is StateCU_Data)
			{
				cudata = (StateCU_Data)data[idata];
				label = StateMod_Util.formatDataLabel(cudata.getID(), cudata.getName());
				node2 = new SimpleJTree_Node(label);
				node2.setData(cudata);
			}
			else if (data_Object is TS)
			{
				tsdata = (TS)data[idata];
				label = StateMod_Util.formatDataLabel(tsdata.getLocation(), tsdata.getDescription());
				node2 = new SimpleJTree_Node(label);
				node2.setData(tsdata);
			}
			try
			{
				addNode(node2, node);
			}
			catch (Exception e)
			{
				Message.printWarning(2, routine, "Error adding data \"" + label + "\"");
				Message.printWarning(2, routine, e);
				continue;
			}
		}
		if (dsize > 0)
		{
			hadData = true;
		}
		// Collapse the node because the lists are usually pretty long...
		try
		{
			collapseNode(node);
		}
		catch (Exception)
		{
			// Ignore.
		}
		return hadData; // Needed in the calling code.
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
	/// Responds to mouse released events and possibly shows a popup menu. </summary>
	/// <param name="event"> the MouseEvent that happened. </param>
	public virtual void mouseReleased(MouseEvent @event)
	{
		showPopupMenu(@event);
	}

	/// <summary>
	/// Refresh a part of the JTree based on the component.  This method is only
	/// designed to work with the detailed display.  It is currently assumed that all
	/// components are represented in the tree, even if no data are listed below the group node. </summary>
	/// <param name="comp_type"> Component type being refreshed.  Use the component groups. </param>
	public virtual void refresh(int comp_type)
	{
		string routine = "StateMod_DataSet_JTree.refresh";
		if (!__display_data_objects)
		{
			return;
		}
		DataSetComponent comp = __dataset.getComponentForComponentType(comp_type);
		// Find the node...
		SimpleJTree_Node node = findNodeByName(comp.getComponentName());
		if (node == null)
		{
			return;
		}
		// Remove the sub-nodes...
		try
		{
			removeChildren(node);
		}
		catch (Exception)
		{
			Message.printWarning(2, routine, "Error removing old nodes - error should not occur.");
		}
		// Now redraw the data...
		setFastAdd(true);
		displayDataSetComponent(comp, node);
		setFastAdd(false);
	}

	/// <summary>
	/// Checks to see if the mouse event would trigger display of the popup menu.
	/// The popup menu does not display if it is null. </summary>
	/// <param name="e"> the MouseEvent that happened. </param>
	private void showPopupMenu(MouseEvent e)
	{
		string routine = "StateMod_DataSet_JTree.showPopupMenu";
		if (!e.isPopupTrigger() || !__display_data_objects)
		{
			// Do not do anything...
			return;
		}
		TreePath path = getPathForLocation(e.getX(), e.getY());
		if (path == null)
		{
			return;
		}
		__popup_Node = (SimpleJTree_Node)path.getLastPathComponent();
		// First remove the menu items that are currently in the menu...
		__popup_JPopupMenu.removeAll();
		object data = null; // Data object associated with the node
		DataSetComponent comp2; // Used to check components in groups.
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
				// Specific checks need to be done to identify the component group...

				DataSetComponent comp = (DataSetComponent)data;
				int comp_type = comp.getComponentType();

				if (comp_type == StateMod_DataSet.COMP_CONTROL_GROUP)
				{
					// For now display the control file information only...
					comp2 = __dataset.getComponentForComponentType(StateMod_DataSet.COMP_CONTROL);
					if (comp2.hasData())
					{
						item = new SimpleJMenuItem("Control Properties",this);
						__popup_JPopupMenu.add(item);
					}
				}

				else if (comp_type == StateMod_DataSet.COMP_STREAMGAGE_GROUP)
				{
					comp2 = __dataset.getComponentForComponentType(StateMod_DataSet.COMP_STREAMGAGE_STATIONS);
					if ((comp2 != null) && comp2.hasData())
					{
						item = new SimpleJMenuItem("Stream Gage Station Properties", this);
						__popup_JPopupMenu.add(item);
					}
				}

				else if (comp_type == StateMod_DataSet.COMP_DELAY_TABLE_MONTHLY_GROUP)
				{
					comp2 = __dataset.getComponentForComponentType(StateMod_DataSet.COMP_DELAY_TABLES_MONTHLY);
					if ((comp2 != null) && comp2.hasData())
					{
						item = new SimpleJMenuItem("Delay Table Properties", this);
						__popup_JPopupMenu.add(item);
					}
				}

				else if (comp_type == StateMod_DataSet.COMP_DELAY_TABLE_DAILY_GROUP)
				{
					comp2 = __dataset.getComponentForComponentType(StateMod_DataSet.COMP_DELAY_TABLES_MONTHLY);
					if ((comp2 != null) && comp2.hasData())
					{
						item = new SimpleJMenuItem("Delay Table Properties", this);
						__popup_JPopupMenu.add(item);
					}
				}

				else if (comp_type == StateMod_DataSet.COMP_DIVERSION_GROUP)
				{
					comp2 = __dataset.getComponentForComponentType(StateMod_DataSet.COMP_DIVERSION_STATIONS);
					if ((comp2 != null) && comp2.hasData())
					{
						item = new SimpleJMenuItem("Diversion Properties", this);
						__popup_JPopupMenu.add(item);
					}
				}

				else if ((comp_type == StateMod_DataSet.COMP_PRECIPITATION_GROUP))
				{
					comp2 = __dataset.getComponentForComponentType(StateMod_DataSet.COMP_PRECIPITATION_TS_MONTHLY);
					if ((comp2 != null) && comp2.hasData())
					{
						item = new SimpleJMenuItem("Precipitation Properties", this);
						__popup_JPopupMenu.add(item);
					}
				}

				else if (comp_type == StateMod_DataSet.COMP_EVAPORATION_GROUP)
				{
					comp2 = __dataset.getComponentForComponentType(StateMod_DataSet.COMP_EVAPORATION_TS_MONTHLY);
					if ((comp2 != null) && comp2.hasData())
					{
						item = new SimpleJMenuItem("Evaporation Properties", this);
						__popup_JPopupMenu.add(item);
					}
				}

				else if (comp_type == StateMod_DataSet.COMP_RESERVOIR_GROUP)
				{
					comp2 = __dataset.getComponentForComponentType(StateMod_DataSet.COMP_RESERVOIR_STATIONS);
					if ((comp2 != null) && comp2.hasData())
					{
						item = new SimpleJMenuItem("Reservoir Properties", this);
						__popup_JPopupMenu.add(item);
					}
				}

				else if (comp_type == StateMod_DataSet.COMP_INSTREAM_GROUP)
				{
					comp2 = __dataset.getComponentForComponentType(StateMod_DataSet.COMP_INSTREAM_STATIONS);
					if ((comp2 != null) && comp2.hasData())
					{
						item = new SimpleJMenuItem("Instream Flow Properties", this);
						__popup_JPopupMenu.add(item);
					}
				}
				else if (comp_type == StateMod_DataSet.COMP_WELL_GROUP)
				{
					comp2 = __dataset.getComponentForComponentType(StateMod_DataSet.COMP_WELL_STATIONS);
					if ((comp2 != null) && comp2.hasData())
					{
						item = new SimpleJMenuItem("Well Properties", this);
						__popup_JPopupMenu.add(item);
					}
				}
				else if (comp_type == StateMod_DataSet.COMP_PLAN_GROUP)
				{
					comp2 = __dataset.getComponentForComponentType(StateMod_DataSet.COMP_PLANS);
					if ((comp2 != null) && comp2.hasData())
					{
						item = new SimpleJMenuItem("Plan Properties", this);
						__popup_JPopupMenu.add(item);
					}
				}
				else if (comp_type == StateMod_DataSet.COMP_STREAMESTIMATE_GROUP)
				{
					comp2 = __dataset.getComponentForComponentType(StateMod_DataSet.COMP_STREAMESTIMATE_STATIONS);
					if ((comp2 != null) && comp2.hasData())
					{
						item = new SimpleJMenuItem("Stream Estimate Station Properties", this);
						__popup_JPopupMenu.add(item);
					}
				}
				else if (comp_type == StateMod_DataSet.COMP_RIVER_NETWORK_GROUP)
				{
					// Only add if data are available...
					comp2 = __dataset.getComponentForComponentType(StateMod_DataSet.COMP_RIVER_NETWORK);
					if ((comp2 != null) && comp2.hasData())
					{
						item = new SimpleJMenuItem("River Network Properties", this);
						__popup_JPopupMenu.add(item);
					}
				}

				else if (comp_type == StateMod_DataSet.COMP_OPERATION_GROUP)
				{
					comp2 = __dataset.getComponentForComponentType(StateMod_DataSet.COMP_OPERATION_RIGHTS);
					if ((comp2 != null) && comp2.hasData())
					{
						item = new SimpleJMenuItem("Operation Rights Properties", this);
						__popup_JPopupMenu.add(item);
					}
				}
			}
			// The data are a specific data instance so display the
			// properties for the specific item if a primary data item.

			// Control... nothing for now.

			else if (data is StateMod_StreamGage)
			{
				item = new SimpleJMenuItem(__popup_Node.getText() + " Properties", this);
				__popup_JPopupMenu.add(item);
			}
			else if (data is StateMod_DelayTable)
			{
				item = new SimpleJMenuItem(__popup_Node.getText() + " Properties", this);
				__popup_JPopupMenu.add(item);
			}
			else if (data is StateMod_Diversion)
			{
				item = new SimpleJMenuItem(__popup_Node.getText() + " Properties", this);
				__popup_JPopupMenu.add(item);
				__popup_JPopupMenu.add(new SimpleJMenuItem(__SUMMARIZE_HOW1 + "\"" + __popup_Node.getText() + "\"" + __SUMMARIZE_HOW2, this));
			}
			else if (data is MonthTS)
			{
				// Precipitation or evaporation time series...
				__popup_JPopupMenu.add(new SimpleJMenuItem(__popup_Node.getText() + __PROPERTIES, this));
				__popup_JPopupMenu.add(new SimpleJMenuItem(__SUMMARIZE_HOW1 + "\"" + __popup_Node.getText() + "\"" + __SUMMARIZE_HOW2, this));
			}
			else if (data is StateMod_Reservoir)
			{
				item = new SimpleJMenuItem(__popup_Node.getText() + " Properties", this);
				__popup_JPopupMenu.add(item);
			}
			else if (data is StateMod_InstreamFlow)
			{
				item = new SimpleJMenuItem(__popup_Node.getText() + " Properties", this);
				__popup_JPopupMenu.add(item);
			}
			else if (data is StateMod_Well)
			{
				item = new SimpleJMenuItem(__popup_Node.getText() + " Properties", this);
				__popup_JPopupMenu.add(item);
			}
			else if (data is StateMod_Plan)
			{
				item = new SimpleJMenuItem(__popup_Node.getText() + " Properties", this);
				__popup_JPopupMenu.add(item);
			}
			else if (data is StateMod_StreamEstimate)
			{
				item = new SimpleJMenuItem(__popup_Node.getText() + " Properties", this);
				__popup_JPopupMenu.add(item);
			}
			else if (data is StateMod_OperationalRight)
			{
				item = new SimpleJMenuItem(__popup_Node.getText() + " Properties", this);
				__popup_JPopupMenu.add(item);
			}
			else if (data is StateMod_RiverNetworkNode)
			{
				item = new SimpleJMenuItem(__popup_Node.getText() + " Properties", this);
				__popup_JPopupMenu.add(item);
			}
			// Others (e.g., San Juan Sediment) supported later....
			else
			{
				Message.printWarning(2, routine, "Node data is not recognized");
				return;
			}
		}
		// Now display the popup so that the user can select the appropriate menu item...
		Point pt = JGUIUtil.computeOptimalPosition(e.getPoint(), e.getComponent(), __popup_JPopupMenu);
		__popup_JPopupMenu.show(e.getComponent(), pt.x, pt.y);
	}

	}

}