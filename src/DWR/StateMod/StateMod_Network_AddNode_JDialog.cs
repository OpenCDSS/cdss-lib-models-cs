// StateMod_Network_AddNode_JDialog - class for adding nodes interactively to the network.

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
// StateMod_Network_AddNode_JDialog - class for adding nodes interactively to
//	the network.
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
//
// 2004-03-16	J. Thomas Sapienza, RTi	Initial version.  
// 2004-07-07	JTS, RTi		* Confluence nodes can be added now.
//					* Grouped related items on the panels.
// 2004-07-12	JTS, RTi		Added support for XConfluence nodes.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace DWR.StateMod
{


	using HydrologyNode = cdss.domain.hydrology.network.HydrologyNode;
	using JGUIUtil = RTi.Util.GUI.JGUIUtil;
	using SimpleJComboBox = RTi.Util.GUI.SimpleJComboBox;

	/// <summary>
	/// Dialog for adding nodes interactively to the network.
	/// </summary>
	public class StateMod_Network_AddNode_JDialog : JDialog, ActionListener, KeyListener
	{

	/// <summary>
	/// Button labels.
	/// </summary>
	private readonly string __BUTTON_CANCEL = "Cancel", __BUTTON_OK = "OK";

	/// <summary>
	/// Node types for display in a combo box.
	/// </summary>
	private readonly string __NODE_CONFLUENCE = "CONFL - Confluence", __NODE_DIVERSION = "DIV - Diversion", __NODE_DIVERSION_AND_WELL = "D&W - Diversion and Well", __NODE_END = "END - End Node", __NODE_INSTREAM_FLOW = "ISF - Instream Flow", __NODE_OTHER = "OTH - Other", __NODE_RESERVOIR = "RES - Reservoir", __NODE_STREAMFLOW = "FLOW - Streamflow", __NODE_WELL = "WELL - Well", __NODE_XCONFLUENCE = "XCONFL - XConfluence", __NODE_PLAN = "PLAN - Plan";

	/// <summary>
	/// The node downstream of the node to be added.
	/// </summary>
	private HydrologyNode __ds = null;

	/// <summary>
	/// Checkbox to mark whether the node is a natural flow node.
	/// </summary>
	private JCheckBox __naturalFlowJCheckBox;

	/// <summary>
	/// Checkbox to mark whether the node is an import node.
	/// </summary>
	private JCheckBox __importJCheckBox;

	/// <summary>
	/// Ok button to accept entered values.
	/// </summary>
	private JButton __okJButton = null;

	/// <summary>
	/// Dialog text fields.
	/// </summary>
	private JTextField __downstreamIDJTextField, __nodeNameJTextField;

	/// <summary>
	/// Dialog combo boxes.
	/// </summary>
	private SimpleJComboBox __nodeTypeComboBox, __upstreamIDComboBox;

	/// <summary>
	/// The parent window on which this dialog is being displayed.
	/// </summary>
	private StateMod_Network_JFrame __parent = null;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="parent"> the JFrame on which this dialog will be shown. </param>
	/// <param name="ds"> the Downstream node from where the ndoe should be added. </param>
	public StateMod_Network_AddNode_JDialog(StateMod_Network_JFrame parent, HydrologyNode ds) : base(parent, "Add Node", true)
	{
		__parent = parent;
		__ds = ds;

		setupGUI();
	}

	/// <summary>
	/// Responds to action events. </summary>
	/// <param name="event"> the ActionEvent that happened. </param>
	public virtual void actionPerformed(ActionEvent @event)
	{
		string action = @event.getActionCommand();
		object o = @event.getSource();

		if (action.Equals(__BUTTON_CANCEL))
		{
			dispose();
		}
		else if (action.Equals(__BUTTON_OK))
		{
			int type = -1;
			string s = __nodeTypeComboBox.getSelected();
			if (s.Equals(__NODE_CONFLUENCE))
			{
				type = HydrologyNode.NODE_TYPE_CONFLUENCE;
			}
			else if (s.Equals(__NODE_DIVERSION))
			{
				type = HydrologyNode.NODE_TYPE_DIV;
			}
			else if (s.Equals(__NODE_DIVERSION_AND_WELL))
			{
				type = HydrologyNode.NODE_TYPE_DIV_AND_WELL;
			}
			else if (s.Equals(__NODE_END))
			{
				type = HydrologyNode.NODE_TYPE_END;
			}
			else if (s.Equals(__NODE_INSTREAM_FLOW))
			{
				type = HydrologyNode.NODE_TYPE_ISF;
			}
			else if (s.Equals(__NODE_OTHER))
			{
				type = HydrologyNode.NODE_TYPE_OTHER;
			}
			else if (s.Equals(__NODE_PLAN))
			{
				type = HydrologyNode.NODE_TYPE_PLAN;
			}
			else if (s.Equals(__NODE_RESERVOIR))
			{
				type = HydrologyNode.NODE_TYPE_RES;
			}
			else if (s.Equals(__NODE_STREAMFLOW))
			{
				type = HydrologyNode.NODE_TYPE_FLOW;
			}
			else if (s.Equals(__NODE_WELL))
			{
				type = HydrologyNode.NODE_TYPE_WELL;
			}
			else if (s.Equals(__NODE_XCONFLUENCE))
			{
				type = HydrologyNode.NODE_TYPE_XCONFLUENCE;
			}

			StateMod_NodeNetwork network = __parent.getNetwork();

			string up = __upstreamIDComboBox.getSelected().Trim();
			if (up.Equals("[none]"))
			{
				up = null;
			}
			network.addNode(__nodeNameJTextField.getText().Trim(), type, up, __downstreamIDJTextField.getText().Trim(), __naturalFlowJCheckBox.isSelected(), __importJCheckBox.isSelected());
			__parent.setNetwork(network, true, true);
	//		__parent.resetNodeSize();
			__parent.endAddNode();
			dispose();
		}
		else if (o == __nodeTypeComboBox)
		{
			string selected = __nodeTypeComboBox.getSelected();

			if (!selected.Equals(__NODE_END))
			{
				__naturalFlowJCheckBox.setEnabled(true);
				__importJCheckBox.setEnabled(true);
			}
		}
	}

	/// <summary>
	/// Checks to make sure that the name entered in the node name field is valid.
	/// </summary>
	private void checkValidity()
	{
		if (!__nodeNameJTextField.getText().Trim().Equals(""))
		{
			__okJButton.setEnabled(true);
		}
		else
		{
			__okJButton.setEnabled(false);
		}
	}

	/// <summary>
	/// Called when the user types something in the node name field -- checks to make 
	/// sure that the entry is valid. </summary>
	/// <param name="event"> the KeyEvent that happened. </param>
	public virtual void keyPressed(KeyEvent @event)
	{
		checkValidity();
	}

	/// <summary>
	/// Called when the user types something in the node name field -- checks to make 
	/// sure that the entry is valid. </summary>
	/// <param name="event"> the KeyEvent that happened. </param>
	public virtual void keyReleased(KeyEvent @event)
	{
		checkValidity();
	}

	/// <summary>
	/// Called when the user types something in the node name field -- checks to make 
	/// sure that the entry is valid. </summary>
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
		addWindowListener(__parent);

		JPanel panel = new JPanel();
		panel.setLayout(new GridBagLayout());

		int y = 0;

		__downstreamIDJTextField = new JTextField(10);
		__downstreamIDJTextField.setEditable(false);
		__downstreamIDJTextField.setText(__ds.getCommonID());
		__upstreamIDComboBox = new SimpleJComboBox(false);
		__upstreamIDComboBox.setPrototypeDisplayValue("[none] - Start a new TributaryXX");
		__nodeNameJTextField = new JTextField(10);
		__nodeNameJTextField.addKeyListener(this);

		string[] usid = __ds.getUpstreamNodesIDs();
		for (int i = 0; i < usid.Length; i++)
		{
			__upstreamIDComboBox.add(usid[i]);
		}
		__upstreamIDComboBox.add("[none] - Start a new Tributary");

		__nodeTypeComboBox = new SimpleJComboBox();

		__nodeTypeComboBox.add(__NODE_CONFLUENCE);
		__nodeTypeComboBox.add(__NODE_DIVERSION);
		__nodeTypeComboBox.add(__NODE_DIVERSION_AND_WELL);
		__nodeTypeComboBox.add(__NODE_INSTREAM_FLOW);
		__nodeTypeComboBox.add(__NODE_OTHER);
		__nodeTypeComboBox.add(__NODE_PLAN);
		__nodeTypeComboBox.add(__NODE_RESERVOIR);
		__nodeTypeComboBox.add(__NODE_STREAMFLOW);
		__nodeTypeComboBox.add(__NODE_WELL);
		__nodeTypeComboBox.add(__NODE_XCONFLUENCE);
		__nodeTypeComboBox.select(__NODE_STREAMFLOW);
		__nodeTypeComboBox.setMaximumRowCount(__nodeTypeComboBox.getItemCount());
		__nodeTypeComboBox.addActionListener(this);

		__naturalFlowJCheckBox = new JCheckBox();
		__naturalFlowJCheckBox.addActionListener(this);

		__importJCheckBox = new JCheckBox();
		__importJCheckBox.addActionListener(this);

		JPanel top = new JPanel();
		top.setLayout(new GridBagLayout());
		top.setBorder(BorderFactory.createTitledBorder("Existing nodes"));

		JPanel bottom = new JPanel();
		bottom.setLayout(new GridBagLayout());
		bottom.setBorder(BorderFactory.createTitledBorder("New Node Data"));

		JGUIUtil.addComponent(panel, top, 0, 0, 1, 1, 1, 1, GridBagConstraints.BOTH, GridBagConstraints.WEST);
		JGUIUtil.addComponent(panel, bottom, 0, 1, 1, 1, 1, 1, GridBagConstraints.BOTH, GridBagConstraints.WEST);

		y = 0;
		JGUIUtil.addComponent(top, new JLabel("Downstream node: "), 0, y, 1, 1, 1, 1, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(top, __downstreamIDJTextField, 1, y, 1, 1, 1, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);
		y++;

		JGUIUtil.addComponent(top, new JLabel("Upstream node: "), 0, y, 1, 1, 1, 1, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(top, __upstreamIDComboBox, 1, y, 1, 1, 1, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);
		y++;

		y = 0;
		JGUIUtil.addComponent(bottom, new JLabel("Node ID: "), 0, y, 1, 1, 1, 1, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(bottom, __nodeNameJTextField, 1, y, 1, 1, 1, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);
		y++;

		JGUIUtil.addComponent(bottom, new JLabel("Node type: "), 0, y, 1, 1, 1, 1, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(bottom, __nodeTypeComboBox, 1, y, 1, 1, 1, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);
		y++;

		JGUIUtil.addComponent(bottom, new JLabel("Is natural flow?: "), 0, y, 1, 1, 1, 1, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(bottom, __naturalFlowJCheckBox, 1, y, 1, 1, 1, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);
		y++;

		JGUIUtil.addComponent(bottom, new JLabel("Is import?: "), 0, y, 1, 1, 1, 1, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(bottom, __importJCheckBox, 1, y, 1, 1, 1, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);

		JPanel southPanel = new JPanel();
		southPanel.setLayout(new GridBagLayout());

		__okJButton = new JButton(__BUTTON_OK);
		__okJButton.addActionListener(this);
		__okJButton.setEnabled(false);
		JButton cancelButton = new JButton(__BUTTON_CANCEL);
		cancelButton.addActionListener(this);

		JGUIUtil.addComponent(southPanel, __okJButton, 0, 0, 1, 1, 1, 1, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(southPanel, cancelButton, 1, 0, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);

		getContentPane().add(panel);
		getContentPane().add(southPanel, "South");

		pack();
		JGUIUtil.center(this);
		setVisible(true);
	}

	}

}