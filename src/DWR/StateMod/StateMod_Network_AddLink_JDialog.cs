using System.Collections.Generic;

// StateMod_Network_AddLink_JDialog - dialog for adding nodes interactively to the network.

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


	using HydrologyNode = cdss.domain.hydrology.network.HydrologyNode;
	using GRArrowStyleType = RTi.GR.GRArrowStyleType;
	using GRLineStyleType = RTi.GR.GRLineStyleType;
	using JGUIUtil = RTi.Util.GUI.JGUIUtil;
	using SimpleJComboBox = RTi.Util.GUI.SimpleJComboBox;

	/// <summary>
	/// Dialog for adding nodes interactively to the network.
	/// </summary>
	public class StateMod_Network_AddLink_JDialog : JDialog, ActionListener, ItemListener
	{

	/// <summary>
	/// Button labels.
	/// </summary>
	private readonly string __BUTTON_CANCEL = "Cancel", __BUTTON_OK = "OK";

	private HydrologyNode[] __nodes = null;

	/// <summary>
	/// Ok button to accept entered values.
	/// </summary>
	private JButton __okJButton = null;

	/// <summary>
	/// Dialog combo boxes.
	/// </summary>
	private SimpleJComboBox __node1ComboBox, __node2ComboBox, __lineStyleComboBox, __fromArrowStyleComboBox, __toArrowStyleComboBox;

	private JTextField __linkId_JTextField;

	private StateMod_Network_JComponent __device = null;

	/// <summary>
	/// The parent window on which this dialog is being displayed.
	/// </summary>
	private StateMod_Network_JFrame __parent = null;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="parent"> the JFrame on which this dialog will be shown. </param>
	/// <param name="ds"> the Downstream node from where the node should be added. </param>
	public StateMod_Network_AddLink_JDialog(StateMod_Network_JFrame parent, StateMod_Network_JComponent device, HydrologyNode[] nodes) : base(parent, "Add Link", true)
	{
		__parent = parent;
		__device = device;
		__nodes = nodes;

		setupGUI();
	}

	/// <summary>
	/// Responds to action events. </summary>
	/// <param name="event"> the ActionEvent that happened. </param>
	public virtual void actionPerformed(ActionEvent @event)
	{
		string action = @event.getActionCommand();

		if (action.Equals(__BUTTON_CANCEL))
		{
			dispose();
		}
		else if (action.Equals(__BUTTON_OK))
		{
			__device.addLink(__node1ComboBox.getSelected(), __node2ComboBox.getSelected(), __linkId_JTextField.getText(), __lineStyleComboBox.getSelected(), __fromArrowStyleComboBox.getSelected(), __toArrowStyleComboBox.getSelected());
			dispose();
		}
		else
		{
			checkValidity();
		}
	}

	private void checkValidity()
	{
		if (!__node1ComboBox.getSelected().Equals(__node2ComboBox.getSelected()))
		{
			__okJButton.setEnabled(true);
		}
		else
		{
			__okJButton.setEnabled(false);
		}
	}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~StateMod_Network_AddLink_JDialog()
	{
		// DO NOT FINALIZE __nodes IN HERE.
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	public virtual void itemStateChanged(ItemEvent @event)
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

		IList<string> nodeIds = new List<object>(__nodes.Length);
		for (int i = 0; i < __nodes.Length; i++)
		{
			nodeIds.Add(__nodes[i].getCommonID());
		}
		nodeIds.Sort();

		JPanel top = new JPanel();
		top.setLayout(new GridBagLayout());

		JGUIUtil.addComponent(panel, top, 0, 0, 1, 1, 1, 1, GridBagConstraints.BOTH, GridBagConstraints.WEST);

		y = 0;
		__node1ComboBox = new SimpleJComboBox(nodeIds);
		__node1ComboBox.addItemListener(this);
		__node1ComboBox.addActionListener(this);
		JGUIUtil.addComponent(top, new JLabel("From node: "), 0, y, 1, 1, 1, 1, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(top, __node1ComboBox, 1, y, 1, 1, 1, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);

		__node2ComboBox = new SimpleJComboBox(nodeIds);
		__node2ComboBox.addItemListener(this);
		__node2ComboBox.addActionListener(this);
		JGUIUtil.addComponent(top, new JLabel("To node: "), 0, ++y, 1, 1, 1, 1, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(top, __node2ComboBox, 1, y, 1, 1, 1, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);

		__linkId_JTextField = new JTextField(10);
		JGUIUtil.addComponent(top, new JLabel("Link ID: "), 0, ++y, 1, 1, 1, 1, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(top, __linkId_JTextField, 1, y, 1, 1, 1, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);

		IList<string> lineStyleChoices = new List<object>();
		lineStyleChoices.Add("" + GRLineStyleType.DASHED);
		lineStyleChoices.Add("" + GRLineStyleType.SOLID);
		__lineStyleComboBox = new SimpleJComboBox(lineStyleChoices);
		__lineStyleComboBox.addItemListener(this);
		__lineStyleComboBox.addActionListener(this);
		JGUIUtil.addComponent(top, new JLabel("Line style: "), 0, ++y, 1, 1, 1, 1, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(top, __lineStyleComboBox, 1, y, 1, 1, 1, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);

		IList<string> arrowEndChoices = new List<object>();
		arrowEndChoices.Add("" + GRArrowStyleType.NONE);
		arrowEndChoices.Add("" + GRArrowStyleType.SOLID);
		__fromArrowStyleComboBox = new SimpleJComboBox(arrowEndChoices);
		__fromArrowStyleComboBox.addItemListener(this);
		__fromArrowStyleComboBox.addActionListener(this);
		JGUIUtil.addComponent(top, new JLabel("Arrow (from) style: "), 0, ++y, 1, 1, 1, 1, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(top, __fromArrowStyleComboBox, 1, y, 1, 1, 1, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);

		__toArrowStyleComboBox = new SimpleJComboBox(arrowEndChoices);
		__toArrowStyleComboBox.addItemListener(this);
		__toArrowStyleComboBox.addActionListener(this);
		JGUIUtil.addComponent(top, new JLabel("Arrow (to) style: "), 0, ++y, 1, 1, 1, 1, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(top, __toArrowStyleComboBox, 1, y, 1, 1, 1, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);

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