﻿using System;
using System.Collections.Generic;

// StateMod_Network_NodeProperties_JDialog - dialog for editing network node properties

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
// StateMod_Network_NodeProperties_JDialog -
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
//
// 2004-04-15	J. Thomas Sapienza, RTi	Initial version.  
// 2004-07-09	JTS, RTi		Removed Baseflow and Import type nodes
//					as they will be specified with the 
//					import and baseflow properties now.
// 2004-07-12	JTS, RTi		Added support for XConfluence nodes.
// 2004-12-20	JTS, RTi		Changed how node labels are numbered
//					so that Makenet networks display
//					properly.
// 2005-06-13	JTS, RTi		Properties now prints out the IDs
//					of upstream and downstream nodes.
// 2005-12-07	JTS, RTi		* When showing the properties for
//					  reservoir nodes, the Reservoir 
//					  Direction combo box and label was 
//					  being overwritten by the first 
//					  Upstream Node line.  Corrected.
//					* Corrected a bug causing null pointer
//					  errors when showing the downstream 
//					  nodes for the End node.
// 2005-12-20	JTS, RTi		Reservoir direction label problem from
//					2005-12-07 was only half solved -- nodes
//					that were not originally reservoirs when
//					the properties dialog was opened still
//					had problems.
// 2006-02-21	JTS, RTi		Fixed a bug that was resulting in 
//					baseflow and import booleans always 
//					being set to false if "Cancel" was
//					pressed.
// 2006-03-07	JTS, RTi		* If running in StateModGUI, important
//					  fields are now noneditable.
//					* Added finalize().
// 2006-04-18	JTS, RTi		__nodes is no longer finalized in this
//					class.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace DWR.StateMod
{


	using HydrologyNode = cdss.domain.hydrology.network.HydrologyNode;

	using JGUIUtil = RTi.Util.GUI.JGUIUtil;
	using SimpleJComboBox = RTi.Util.GUI.SimpleJComboBox;
	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// This class draws the network that can be printed, viewed and altered.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class StateMod_Network_NodeProperties_JDialog extends javax.swing.JDialog implements java.awt.event.ActionListener, java.awt.event.KeyListener, java.awt.event.WindowListener
	public class StateMod_Network_NodeProperties_JDialog : JDialog, ActionListener, KeyListener, WindowListener
	{

	/// <summary>
	/// Possible node types to appear in the type combo box.
	/// </summary>
	private readonly string __NODE_CONFLUENCE = "CONFL - Confluence", __NODE_DIVERSION = "DIV - Diversion", __NODE_DIVERSION_AND_WELL = "D&W - Diversion and Well", __NODE_END = "END - End Node", __NODE_INSTREAM_FLOW = "ISF - Instream Flow", __NODE_OTHER = "OTH - Other", __NODE_PLAN = "PLN - Plan", __NODE_RESERVOIR = "RES - Reservoir", __NODE_STREAMFLOW = "FLOW - Streamflow", __NODE_WELL = "WELL - Well", __NODE_XCONFLUENCE = "XCONFL - XConfluence";

	/// <summary>
	/// Reservoir directions.
	/// </summary>
	private readonly string __ABOVE_CENTER = "AboveCenter", __UPPER_RIGHT = "UpperRight", __RIGHT = "Right", __LOWER_RIGHT = "LowerRight", __BELOW_CENTER = "BelowCenter", __BOTTOM = "Bottom", __LOWER_LEFT = "LowerLeft", __LEFT = "Left", __UPPER_LEFT = "UpperLeft", __CENTER = "Center", __TOP = "Top";

	/// <summary>
	/// Button labels.
	/// </summary>
	private readonly string __BUTTON_APPLY = "Apply", __BUTTON_CANCEL = "Cancel", __BUTTON_OK = "OK";

	private bool __ignoreEvents;
	private bool __origNaturalFlow;
	private bool __origDirty = false;
	private bool __origImport;

	/// <summary>
	/// Array of nodes from which one will be displayed and edited.
	/// </summary>
	private HydrologyNode[] __nodes;

	/// <summary>
	/// The number of the node being displayed in the __nodes array.
	/// </summary>
	private int __nodeNum = -1;

	/// <summary>
	/// Original node int values.
	/// </summary>
	private int __origResIDir, __origIDir, __origIType;

	/// <summary>
	/// GUI buttons.
	/// </summary>
	private JButton __applyButton = null, __okButton = null;

	/// <summary>
	/// Check box for specifying if the node is a natural flow node.
	/// </summary>
	private JCheckBox __isNaturalFlowCheckBox;

	/// <summary>
	/// Check box for specifying if the node is an import node or not.
	/// </summary>
	private JCheckBox __isImportCheckBox;

	/// <summary>
	/// Reservoir direction JLabel.
	/// </summary>
	private JLabel __reservoirDirectionLabel;

	/// <summary>
	/// Text fields for holding node values.
	/// </summary>
	private JTextField __descriptionTextField, __idTextField, __xTextField, __yTextField, __areaTextField, __precipitationTextField;

	/// <summary>
	/// GUI combo boxes.
	/// </summary>
	private SimpleJComboBox __labelPositionComboBox, __reservoirDirectionComboBox, __typeComboBox;

	/// <summary>
	/// The parent JFrame on which this dialog appears.
	/// </summary>
	private StateMod_Network_JFrame __parent = null;

	/// <summary>
	/// Original node String values.
	/// </summary>
	private string __origArea, __origDesc, __origDir, __origID, __origPrecipitation, __origResDir, __origType, __origX, __origY;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="parent"> the parent JFrame on which this dialog appears. </param>
	/// <param name="nodes"> array of nodes, from which one will be displayed. </param>
	/// <param name="nodeNum"> the number of the node being displayed from the array. </param>
	public StateMod_Network_NodeProperties_JDialog(StateMod_Network_JFrame parent, HydrologyNode[] nodes, int nodeNum) : base(parent, "Node Properties - " + nodes[nodeNum].getCommonID(), true)
	{
		__parent = parent;
		__nodes = nodes;
		__nodeNum = nodeNum;
		setupGUI();
	}

	/// <summary>
	/// Responds to action events. </summary>
	/// <param name="event"> the ActionEvent that happened. </param>
	public virtual void actionPerformed(ActionEvent @event)
	{
		if (__ignoreEvents)
		{
			return;
		}
		string action = @event.getActionCommand();
		object source = @event.getSource();

		if (action.Equals(__BUTTON_CANCEL))
		{
			cancelClicked();
		}
		else if (action.Equals(__BUTTON_OK) || action.Equals(__BUTTON_APPLY))
		{
				  applyClicked();
			if (action.Equals(__BUTTON_OK))
			{
				setVisible(false);
				dispose();
			}
		}
		else if (source == __typeComboBox)
		{
			string type = __typeComboBox.getSelected();
			if (type.Equals(__NODE_RESERVOIR))
			{
				__reservoirDirectionLabel.setVisible(true);
				__reservoirDirectionComboBox.setVisible(true);
			}
			else
			{
				__reservoirDirectionLabel.setVisible(false);
				__reservoirDirectionComboBox.setVisible(false);
			}
		}
		else if (source == __isNaturalFlowCheckBox)
		{
			if (__isNaturalFlowCheckBox.isSelected())
			{
				__areaTextField.setEnabled(true);
				__precipitationTextField.setEnabled(true);
			}
			else
			{
				__areaTextField.setEnabled(false);
				__precipitationTextField.setEnabled(false);
			}
		}
		else if (source == __isImportCheckBox)
		{
			if (__isImportCheckBox.isSelected())
			{
			}
			else
			{
			}
		}
	}

	/// <summary>
	/// Adds the IDs of the downstream nodes to the main JPanel. </summary>
	/// <param name="panel"> the panel to which to add the node IDs. </param>
	/// <param name="y"> the y coordinate at which to start adding them. </param>
	private void addDownstreamNodeToPanel(JPanel panel, int y)
	{
		HydrologyNode node = StateMod_NodeNetwork.findNextRealDownstreamNode(__nodes[__nodeNum]);
		JGUIUtil.addComponent(panel, new JLabel("Downstream node: "), 0, y, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(panel, new JLabel("" + node.getCommonID() + " (model node)"), 1, y, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);

		y++;

		node = __nodes[__nodeNum].getDownstreamNode();
		if (node != null)
		{
			JGUIUtil.addComponent(panel, new JLabel("" + node.getCommonID() + " (diagram node)"), 1, y, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		}
		else
		{
			JGUIUtil.addComponent(panel,new JLabel("[None] (diagram node)"), 1, y, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		}
	}

	/// <summary>
	/// Adds the IDs of the upstream nodes to the main JPanel. </summary>
	/// <param name="panel"> the panel to which to add the node IDs. </param>
	/// <param name="y"> the y coordinate at which to start adding them. </param>
	private int addUpstreamNodesToPanel(JPanel panel, int y)
	{
		IList<HydrologyNode> upstreams = __nodes[__nodeNum].getUpstreamNodes();
		if (upstreams == null || upstreams.Count == 0)
		{
			return y;
		}

		int size = upstreams.Count;
		string plural = "s";
		if (size == 1)
		{
			plural = "";
		}

		for (int i = 0; i < size; i++)
		{
			if (i == 0)
			{
				JGUIUtil.addComponent(panel, new JLabel("Upstream node" + plural + ": "), 0, y, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
			}
			JGUIUtil.addComponent(panel, new JLabel("" + ((HydrologyNode)upstreams[i]).getCommonID()), 1, y, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
			y++;
		}

		return y;
	}

	/// <summary>
	/// Called when Apply or OK is clicked.  Commits any changes to the node.
	/// </summary>
	private void applyClicked()
	{
		//TODO SAM 2007-03-01 Evaluate use
		bool dirty = false;
		string id = __idTextField.getText();
		string x = __xTextField.getText();
		string y = __yTextField.getText();
		string desc = __descriptionTextField.getText();
		bool isNaturalFlow = __isNaturalFlowCheckBox.isSelected();
		bool mport = __isImportCheckBox.isSelected();
		string type = __typeComboBox.getSelected();
		string dir = __labelPositionComboBox.getSelected();
		string resDir = __reservoirDirectionComboBox.getSelected();
		string area = __areaTextField.getText();
		string precip = __precipitationTextField.getText();

		__nodes[__nodeNum].setCommonID(id);
		if (!id.Equals(__origID))
		{
			dirty = true;
		}

		__nodes[__nodeNum].setX((Convert.ToDouble(x)));
		if (!x.Equals(__origX))
		{
			dirty = true;
		}

		__nodes[__nodeNum].setY((Convert.ToDouble(y)));
		if (!y.Equals(__origY))
		{
			dirty = true;
		}

		__nodes[__nodeNum].setDescription(desc);
		if (!desc.Equals(__origDesc))
		{
			dirty = true;
		}

		__nodes[__nodeNum].setIsNaturalFlow(isNaturalFlow);
		if (isNaturalFlow != __origNaturalFlow)
		{
			dirty = true;
		}

		__nodes[__nodeNum].setIsImport(mport);
		if (mport != __origImport)
		{
			dirty = true;
		}

		__nodes[__nodeNum].setPrecip((Convert.ToDouble(precip)));
		if (!precip.Equals(__origPrecipitation))
		{
			dirty = true;
		}

		__nodes[__nodeNum].setArea((Convert.ToDouble(area)));
		if (!area.Equals(__origArea))
		{
			dirty = true;
		}

		int itype = -1;
		bool res = false;
		if (type.Equals(__NODE_CONFLUENCE))
		{
			itype = HydrologyNode.NODE_TYPE_CONFLUENCE;
		}
		else if (type.Equals(__NODE_DIVERSION))
		{
			itype = HydrologyNode.NODE_TYPE_DIV;
		}
		else if (type.Equals(__NODE_DIVERSION_AND_WELL))
		{
			itype = HydrologyNode.NODE_TYPE_DIV_AND_WELL;
		}
		else if (type.Equals(__NODE_END))
		{
			itype = HydrologyNode.NODE_TYPE_END;
		}
		else if (type.Equals(__NODE_INSTREAM_FLOW))
		{
			itype = HydrologyNode.NODE_TYPE_ISF;
		}
		else if (type.Equals(__NODE_OTHER))
		{
			itype = HydrologyNode.NODE_TYPE_OTHER;
		}
		else if (type.Equals(__NODE_PLAN))
		{
			itype = HydrologyNode.NODE_TYPE_PLAN;
		}
		else if (type.Equals(__NODE_RESERVOIR))
		{
			itype = HydrologyNode.NODE_TYPE_RES;
			res = true;
		}
		else if (type.Equals(__NODE_STREAMFLOW))
		{
			itype = HydrologyNode.NODE_TYPE_FLOW;
		}
		else if (type.Equals(__NODE_WELL))
		{
			itype = HydrologyNode.NODE_TYPE_WELL;
		}
		else if (type.Equals(__NODE_XCONFLUENCE))
		{
			itype = HydrologyNode.NODE_TYPE_XCONFLUENCE;
		}
		else
		{
			int index = type.IndexOf(":", StringComparison.Ordinal);
			type = type.Substring(index + 1).Trim();
			itype = Integer.decode(type).intValue();
		}
		__nodes[__nodeNum].setType(itype);
		if (!type.Equals(__origType))
		{
			dirty = true;
		}

		int idir = -1;
		if (dir.Equals(__ABOVE_CENTER))
		{
			idir = 1;
		}
		else if (dir.Equals(__UPPER_RIGHT))
		{
			idir = 7;
		}
		else if (dir.Equals(__RIGHT))
		{
			idir = 4;
		}
		else if (dir.Equals(__LOWER_RIGHT))
		{
			idir = 8;
		}
		else if (dir.Equals(__BELOW_CENTER))
		{
			idir = 2;
		}
		else if (dir.Equals(__LOWER_LEFT))
		{
			idir = 5;
		}
		else if (dir.Equals(__LEFT))
		{
			idir = 3;
		}
		else if (dir.Equals(__UPPER_LEFT))
		{
			idir = 6;
		}
		else if (dir.Equals(__CENTER))
		{
			idir = 9;
		}
		else
		{
			int index = dir.IndexOf(":", StringComparison.Ordinal);
			dir = dir.Substring(index + 1).Trim();
			idir = Integer.decode(dir).intValue();
		}
		int iresdir = 0;
		if (res)
		{
			if (resDir.Equals(__TOP))
			{
				iresdir = 2;
			}
			else if (resDir.Equals(__BOTTOM))
			{
				iresdir = 1;
			}
			else if (resDir.Equals(__LEFT))
			{
				iresdir = 4;
			}
			else if (resDir.Equals(__RIGHT))
			{
				iresdir = 3;
			}
			else
			{
				int index = resDir.IndexOf(":", StringComparison.Ordinal);
				resDir = resDir.Substring(index + 1).Trim();
				iresdir = Integer.decode(resDir).intValue();
			}
		}

		__nodes[__nodeNum].setLabelDirection((iresdir * 10) + idir);
		if (!resDir.Equals(__origResDir) || !dir.Equals(__origDir))
		{
			dirty = true;
		}

		// Only set the dirty flag if it was false previously because a previous edit may have marked
		// dirty and this edit may not have introduced additional changes.
		if (!__nodes[__nodeNum].isDirty())
		{
			__nodes[__nodeNum].setDirty(dirty);
		}
		__nodes[__nodeNum].setBoundsCalculated(false);

		__parent.nodePropertiesChanged();
	}

	/// <summary>
	/// Called when cancel is pressed -- reverts any changes made to the node.
	/// </summary>
	private void cancelClicked()
	{
		__nodes[__nodeNum].setCommonID(__origID);
		__nodes[__nodeNum].setX((Convert.ToDouble(__origX)));
		__nodes[__nodeNum].setY((Convert.ToDouble(__origY)));
		__nodes[__nodeNum].setDescription(__origDesc);
		__nodes[__nodeNum].setIsNaturalFlow(__origNaturalFlow);
		__nodes[__nodeNum].setType(__origIType);
		__nodes[__nodeNum].setLabelDirection(__origResIDir * 10 + __origIDir);
		__nodes[__nodeNum].setDirty(__origDirty);
		__parent.nodePropertiesChanged();
		setVisible(false);
		dispose();
	}

	/// <summary>
	/// Checks an id to make sure it's unique within the network. </summary>
	/// <param name="id"> the id to check </param>
	/// <returns> true if the ID is unique, false if not. </returns>
	private bool checkUniqueID(string id)
	{
		for (int i = 0; i < __nodes.Length; i++)
		{
			if (i != __nodeNum)
			{
				if (__nodes[i].getCommonID().Equals(id))
				{
					return false;
				}
			}
		}

		return true;
	}

	/// <summary>
	/// Checks the current values for the node's data to make sure they are valid.  If
	/// any are invalid their textfields are colored red and the OK button is 
	/// disabled.
	/// </summary>
	private void checkValidity()
	{
		string id = __idTextField.getText();
		bool valid = true;

		if (!__parent.inStateModGUI())
		{
			if (id.Trim().Equals(""))
			{
				__idTextField.setBackground(Color.red);
				valid = false;
			}
			else if (!checkUniqueID(id))
			{
				__idTextField.setBackground(Color.red);
				valid = false;
			}
			else
			{
				__idTextField.setBackground(Color.white);
			}
		}

		string xs = __xTextField.getText();
		if (xs.Trim().Equals(""))
		{
			__xTextField.setBackground(Color.red);
			valid = false;
		}
		else
		{
			try
			{
				__xTextField.setBackground(Color.white);
			}
			catch (Exception)
			{
				__xTextField.setBackground(Color.red);
				valid = false;
			}
		}

		string ys = __yTextField.getText();
		if (ys.Trim().Equals(""))
		{
			__yTextField.setBackground(Color.red);
			valid = false;
		}
		else
		{
			try
			{
				__yTextField.setBackground(Color.white);
			}
			catch (Exception)
			{
				__yTextField.setBackground(Color.red);
				valid = false;
			}
		}

		if (!__parent.inStateModGUI())
		{
			string area = __areaTextField.getText();
			if (area.Trim().Equals(""))
			{
				__areaTextField.setBackground(Color.red);
				valid = false;
			}
			else
			{
				try
				{
					__areaTextField.setBackground(Color.white);
				}
				catch (Exception)
				{
					__areaTextField.setBackground(Color.red);
					valid = false;
				}
			}
		}

		if (!__parent.inStateModGUI())
		{
			string precipitation = __precipitationTextField.getText();
			if (precipitation.Trim().Equals(""))
			{
				__precipitationTextField.setBackground(Color.red);
				valid = false;
			}
			else
			{
				try
				{
					__precipitationTextField.setBackground(Color.white);
				}
				catch (Exception)
				{
					__precipitationTextField.setBackground(Color.red);
					valid = false;
				}
			}
		}

		if (!valid)
		{
			__okButton.setEnabled(false);
			__applyButton.setEnabled(false);
		}
		else
		{
			__okButton.setEnabled(true);
			__applyButton.setEnabled(true);
		}
	}

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~StateMod_Network_NodeProperties_JDialog()
	{
		// DO NOT DO THE FOLLOWING:
		// RTi.Util.IO.IOUtil.nullArray(__nodes);
		// That will result in the array being nulled in the calling code.
		__applyButton = null;
		__okButton = null;
		__isNaturalFlowCheckBox = null;
		__isImportCheckBox = null;
		__reservoirDirectionLabel = null;
		__descriptionTextField = null;
		__idTextField = null;
		__xTextField = null;
		__yTextField = null;
		__areaTextField = null;
		__precipitationTextField = null;
		__labelPositionComboBox = null;
		__reservoirDirectionComboBox = null;
		__typeComboBox = null;
		__parent = null;
		__origArea = null;
		__origDesc = null;
		__origDir = null;
		__origID = null;
		__origPrecipitation = null;
		__origResDir = null;
		__origType = null;
		__origX = null;
		__origY = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Responds to key press events and calls checkValidity(). </summary>
	/// <param name="event"> the KeyEvent that happened. </param>
	public virtual void keyPressed(KeyEvent @event)
	{
		checkValidity();
	}

	/// <summary>
	/// Responds to key releases and calls checkValidity(). </summary>
	/// <param name="event"> the KeyEvent that happened. </param>
	public virtual void keyReleased(KeyEvent @event)
	{
		checkValidity();
	}

	/// <summary>
	/// Responds to key types events and calls checkValidity(). </summary>
	/// <param name="event"> the KeyEvent that happened. </param>
	public virtual void keyTyped(KeyEvent @event)
	{
		checkValidity();
	}

	/// <summary>
	/// Sets up the GUI.
	/// </summary>
	private void setupGUI()
	{
		addWindowListener(this);

		__descriptionTextField = new JTextField(25);
		__origDesc = __nodes[__nodeNum].getDescription();
		__descriptionTextField.setText(__origDesc);
		__idTextField = new JTextField(10);
		__origID = __nodes[__nodeNum].getCommonID();
		__idTextField.setText(__origID);
		__idTextField.addKeyListener(this);
		__xTextField = new JTextField(10);
		__origX = StringUtil.formatString(__nodes[__nodeNum].getX(), "%13.6f").Trim();
		__xTextField.setText(__origX);
		__xTextField.addKeyListener(this);
		__yTextField = new JTextField(10);
		__origY = StringUtil.formatString(__nodes[__nodeNum].getY(), "%13.6f").Trim();
		__yTextField.setText(__origY);
		__yTextField.addKeyListener(this);
		__areaTextField = new JTextField(10);
		__origArea = StringUtil.formatString(__nodes[__nodeNum].getArea(), "%13.6f").Trim();
		__areaTextField.setText(__origArea);
		__areaTextField.addKeyListener(this);
		__precipitationTextField = new JTextField(10);
		__origPrecipitation = StringUtil.formatString(__nodes[__nodeNum].getPrecip(), "%13.6f").Trim();
		__precipitationTextField.setText(__origPrecipitation);
		__precipitationTextField.addKeyListener(this);

		__typeComboBox = new SimpleJComboBox(false);
		__typeComboBox.add(__NODE_DIVERSION);
		__typeComboBox.add(__NODE_DIVERSION_AND_WELL);
		__typeComboBox.add(__NODE_INSTREAM_FLOW);
		__typeComboBox.add(__NODE_OTHER);
		__typeComboBox.add(__NODE_PLAN);
		__typeComboBox.add(__NODE_RESERVOIR);
		__typeComboBox.add(__NODE_STREAMFLOW);
		__typeComboBox.add(__NODE_WELL);

		int type = __nodes[__nodeNum].getType();
		if (type == HydrologyNode.NODE_TYPE_CONFLUENCE)
		{
			__typeComboBox.select(__NODE_CONFLUENCE);
		}
		else if (type == HydrologyNode.NODE_TYPE_DIV)
		{
			__typeComboBox.select(__NODE_DIVERSION);
		}
		else if (type == HydrologyNode.NODE_TYPE_DIV_AND_WELL)
		{
			__typeComboBox.select(__NODE_DIVERSION_AND_WELL);
		}
		else if (type == HydrologyNode.NODE_TYPE_END)
		{
			__typeComboBox.removeAll();
			__typeComboBox.add(__NODE_END);
		}
		else if (type == HydrologyNode.NODE_TYPE_ISF)
		{
			__typeComboBox.select(__NODE_INSTREAM_FLOW);
		}
		else if (type == HydrologyNode.NODE_TYPE_OTHER)
		{
			__typeComboBox.select(__NODE_OTHER);
		}
		else if (type == HydrologyNode.NODE_TYPE_PLAN)
		{
			__typeComboBox.select(__NODE_PLAN);
		}
		else if (type == HydrologyNode.NODE_TYPE_RES)
		{
			__typeComboBox.select(__NODE_RESERVOIR);
		}
		else if (type == HydrologyNode.NODE_TYPE_FLOW)
		{
			__typeComboBox.select(__NODE_STREAMFLOW);
		}
		else if (type == HydrologyNode.NODE_TYPE_WELL)
		{
			__typeComboBox.select(__NODE_WELL);
		}
		else if (type == HydrologyNode.NODE_TYPE_XCONFLUENCE)
		{
			__typeComboBox.select(__NODE_XCONFLUENCE);
		}
		else
		{
			__typeComboBox.removeAll();
			__typeComboBox.add("Unknown Type: " + type);
		}

		__typeComboBox.setMaximumRowCount(__typeComboBox.getItemCount());

		__origIType = type;
		__origType = __typeComboBox.getSelected();

		__labelPositionComboBox = new SimpleJComboBox(false);
		__labelPositionComboBox.add(__ABOVE_CENTER);
		__labelPositionComboBox.add(__UPPER_RIGHT);
		__labelPositionComboBox.add(__RIGHT);
		__labelPositionComboBox.add(__LOWER_RIGHT);
		__labelPositionComboBox.add(__BELOW_CENTER);
		__labelPositionComboBox.add(__LOWER_LEFT);
		__labelPositionComboBox.add(__LEFT);
		__labelPositionComboBox.add(__UPPER_LEFT);
		__labelPositionComboBox.add(__CENTER);
		__labelPositionComboBox.setMaximumRowCount(__labelPositionComboBox.getItemCount());

		int dir = __nodes[__nodeNum].getLabelDirection() % 10;
		if (dir == 2)
		{
			__labelPositionComboBox.select(__BELOW_CENTER);
		}
		else if (dir == 1)
		{
			__labelPositionComboBox.select(__ABOVE_CENTER);
		}
		else if (dir == 4)
		{
			__labelPositionComboBox.select(__RIGHT);
		}
		else if (dir == 3)
		{
			__labelPositionComboBox.select(__LEFT);
		}
		else if (dir == 7)
		{
			__labelPositionComboBox.select(__UPPER_RIGHT);
		}
		else if (dir == 8)
		{
			__labelPositionComboBox.select(__LOWER_RIGHT);
		}
		else if (dir == 5)
		{
			__labelPositionComboBox.select(__LOWER_LEFT);
		}
		else if (dir == 6)
		{
			__labelPositionComboBox.select(__UPPER_LEFT);
		}
		else if (dir == 9)
		{
			__labelPositionComboBox.select(__CENTER);
		}
		else
		{
			__labelPositionComboBox.removeAll();
			__labelPositionComboBox.add("Unknown Position: " + dir);
		}

		__origIDir = dir;
		__origDir = __labelPositionComboBox.getSelected();

		__isNaturalFlowCheckBox = new JCheckBox();
		__isImportCheckBox = new JCheckBox();

		__origDirty = __nodes[__nodeNum].isDirty();

		__reservoirDirectionLabel = new JLabel("Reservoir Direction: ");
		__reservoirDirectionComboBox = new SimpleJComboBox(false);
		__reservoirDirectionComboBox.setToolTipText("Reservoir body is to the indicated direction " + "(downstream follows the arrow).");
		__reservoirDirectionComboBox.add(__TOP);
		__reservoirDirectionComboBox.add(__BOTTOM);
		__reservoirDirectionComboBox.add(__LEFT);
		__reservoirDirectionComboBox.add(__RIGHT);

		int resDir = __nodes[__nodeNum].getLabelDirection() / 10;
		if (resDir == 1)
		{
			__reservoirDirectionComboBox.select(__BOTTOM);
		}
		else if (resDir == 2)
		{
			__reservoirDirectionComboBox.select(__TOP);
		}
		else if (resDir == 3)
		{
			__reservoirDirectionComboBox.select(__RIGHT);
		}
		else if (resDir == 4)
		{
			__reservoirDirectionComboBox.select(__LEFT);
		}
		else if (resDir == 0)
		{
		}
		else
		{
			__reservoirDirectionComboBox.removeAll();
			__reservoirDirectionComboBox.add("Unknown direction: " + resDir);
		}

		__origResIDir = resDir;
		__origResDir = __reservoirDirectionComboBox.getSelected();

		JPanel panel = new JPanel();
		panel.setLayout(new GridBagLayout());

		int y = 0;
		JGUIUtil.addComponent(panel, new JLabel("ID: "), 0, y, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(panel, __idTextField, 1, y, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);

		y++;
		JGUIUtil.addComponent(panel, new JLabel("Type: "), 0, y, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(panel, __typeComboBox, 1, y, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);

		y++;
		JGUIUtil.addComponent(panel, new JLabel("Description: "), 0, y, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(panel, __descriptionTextField, 1, y, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);

		y++;
		JGUIUtil.addComponent(panel, new JLabel("X: "), 0, y, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(panel, __xTextField, 1, y, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);

		y++;
		JGUIUtil.addComponent(panel, new JLabel("Y: "), 0, y, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(panel, __yTextField, 1, y, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);

		y++;
		JGUIUtil.addComponent(panel, new JLabel("Is natural flow?: "), 0, y, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(panel, __isNaturalFlowCheckBox, 1, y, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);

		y++;
		JGUIUtil.addComponent(panel, new JLabel("Is import?: "), 0, y, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(panel, __isImportCheckBox, 1, y, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);

		y++;
		JGUIUtil.addComponent(panel, new JLabel("Area: "), 0, y, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(panel, __areaTextField, 1, y, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		y++;
		JGUIUtil.addComponent(panel, new JLabel("Precipitation: "), 0, y, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(panel, __precipitationTextField, 1, y, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		if (__nodes[__nodeNum].getIsNaturalFlow())
		{
			__isNaturalFlowCheckBox.setSelected(true);
		}
		else
		{
			__isNaturalFlowCheckBox.setSelected(false);
			__areaTextField.setEnabled(false);
			__precipitationTextField.setEnabled(false);
		}
		__isNaturalFlowCheckBox.addActionListener(this);

		if (__nodes[__nodeNum].getIsImport())
		{
			__isImportCheckBox.setSelected(true);
		}
		else
		{
			__isImportCheckBox.setSelected(false);
		}
		__isImportCheckBox.addActionListener(this);

		__origNaturalFlow = __isNaturalFlowCheckBox.isSelected();
		__origImport = __isImportCheckBox.isSelected();

		y++;
		JGUIUtil.addComponent(panel, new JLabel("Label Position: "), 0, y, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(panel, __labelPositionComboBox, 1, y, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);

		y++;
		JGUIUtil.addComponent(panel, __reservoirDirectionLabel, 0, y, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(panel, __reservoirDirectionComboBox, 1, y, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);

		y++;

		y = addUpstreamNodesToPanel(panel, y);
		addDownstreamNodeToPanel(panel, y);

		getContentPane().add(panel);

		JPanel southPanel = new JPanel();
		southPanel.setLayout(new GridBagLayout());

		__applyButton = new JButton(__BUTTON_APPLY);
		__applyButton.addActionListener(this);
		__okButton = new JButton(__BUTTON_OK);
		__okButton.addActionListener(this);
		JButton cancelButton = new JButton(__BUTTON_CANCEL);
		cancelButton.addActionListener(this);

		JGUIUtil.addComponent(southPanel, __applyButton, 0, 0, 1, 1, 1, 1, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(southPanel, __okButton, 2, 0, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(southPanel, cancelButton, 3, 0, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);

		getContentPane().add(southPanel, "South");

		pack();

		if (__origIType != HydrologyNode.NODE_TYPE_RES)
		{
			__reservoirDirectionLabel.setVisible(false);
			__reservoirDirectionComboBox.setVisible(false);
		}

		__typeComboBox.addActionListener(this);

		if (__parent.inStateModGUI())
		{
			__typeComboBox.setEnabled(false);
			__precipitationTextField.setEditable(false);
			__precipitationTextField.removeKeyListener(this);
			__descriptionTextField.setEditable(false);
			__descriptionTextField.removeKeyListener(this);
			__areaTextField.removeKeyListener(this);
			__areaTextField.setEditable(false);
			__idTextField.removeKeyListener(this);
			__idTextField.setEditable(false);
			__isNaturalFlowCheckBox.setEnabled(false);
			__isImportCheckBox.setEnabled(false);
		}

		JGUIUtil.center(this);
		setVisible(true);
	}

	public virtual void windowActivated(WindowEvent @event)
	{
	}
	public virtual void windowDeactivated(WindowEvent @event)
	{
	}
	public virtual void windowIconified(WindowEvent @event)
	{
	}
	public virtual void windowDeiconified(WindowEvent @event)
	{
	}
	public virtual void windowOpened(WindowEvent @event)
	{
	}

	public virtual void windowClosing(WindowEvent @event)
	{
		cancelClicked();
	}

	public virtual void windowClosed(WindowEvent @event)
	{
	}

	}

}