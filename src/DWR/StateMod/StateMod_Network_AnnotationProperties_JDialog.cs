using System;
using System.Collections.Generic;

// StateMod_Network_AnnotationProperties_JDialog - class to display and edit annotation properties for a node in the diagram display.

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
// StateMod_Network_AnnotationProperties_JDialog - class to display and edit
// 	annotation properties for a node in the diagram display.
// ----------------------------------------------------------------------------
//  Copyright:	See the COPYRIGHT file.
// ----------------------------------------------------------------------------
// History:
// 2004-07-12	J. Thomas Sapienza, RTi	Initial version from
//					HydroBase_GUI_WISDiagramNodeProperties.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace DWR.StateMod
{


	using HydrologyNode = cdss.domain.hydrology.network.HydrologyNode;
	using GRLimits = RTi.GR.GRLimits;
	using GRText = RTi.GR.GRText;
	using JGUIUtil = RTi.Util.GUI.JGUIUtil;
	using SimpleJComboBox = RTi.Util.GUI.SimpleJComboBox;
	using PropList = RTi.Util.IO.PropList;
	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// GUI for displaying, editing, and validating node properties for normal nodes and annotations.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class StateMod_Network_AnnotationProperties_JDialog extends javax.swing.JFrame implements java.awt.event.ActionListener, java.awt.event.KeyListener, java.awt.event.WindowListener
	public class StateMod_Network_AnnotationProperties_JDialog : JFrame, ActionListener, KeyListener, WindowListener
	{

	/// <summary>
	/// Button labels.
	/// </summary>
	private readonly string __BUTTON_APPLY = "Apply", __BUTTON_CANCEL = "Cancel", __BUTTON_OK = "OK";

	/// <summary>
	/// Whether the node data are editable.
	/// </summary>
	private bool __editable = false;

	/// <summary>
	/// The default background color of editable text fields.
	/// </summary>
	private Color __textFieldBackground = null;

	/// <summary>
	/// The GUI that instantiated this GUI.
	/// </summary>
	private StateMod_Network_JComponent __parent;

	/// <summary>
	/// The node that is being edited.
	/// </summary>
	private HydrologyNode __node;

	/// <summary>
	/// The number of the node (for use in the parent GUI).
	/// </summary>
	private int __nodeNum = -1;

	/// <summary>
	/// GUI buttons
	/// </summary>
	private JButton __applyButton = null, __okButton = null;

	/// <summary>
	/// GUI text fields.
	/// </summary>
	private JTextField __fontSizeTextField, __textTextField, __xTextField, __yTextField;

	/// <summary>
	/// GUI combo boxes.
	/// </summary>
	private SimpleJComboBox __fontNameComboBox, __fontStyleComboBox, __textPositionComboBox;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="parent"> the parent GUI that instantiated this GUI. </param>
	/// <param name="node"> the node for which to edit properties. </param>
	/// <param name="nodeNum"> the number of the node in the parent GUI. </param>
	public StateMod_Network_AnnotationProperties_JDialog(StateMod_Network_JComponent parent, bool editable, HydrologyNode node, int nodeNum)
	{
		__parent = parent;
		__editable = editable;
		__node = node;
		__nodeNum = nodeNum;

		JGUIUtil.setIcon(this, JGUIUtil.getIconImage());
		setupGUI();
	}

	/// <summary>
	/// Responds to button presses. </summary>
	/// <param name="event"> the ActionEvent that happened. </param>
	public virtual void actionPerformed(ActionEvent @event)
	{
		string command = @event.getActionCommand();

		if (command.Equals(__BUTTON_APPLY))
		{
			applyChanges();
			__node = __parent.getAnnotationNode(__nodeNum);
			__applyButton.setEnabled(false);
			__okButton.setEnabled(false);
		}
		else if (command.Equals(__BUTTON_CANCEL))
		{
			closeWindow();
		}
		else if (command.Equals(__BUTTON_OK))
		{
			if (__editable)
			{
				applyChanges();
			}
			closeWindow();
		}
		else
		{
			// a combo box action triggered it.
			validateData();
		}
	}

	/// <summary>
	/// Saves the changes made in the GUI and applies them to the node in the parent GUI.
	/// </summary>
	private void applyChanges()
	{
		if (!validateData())
		{
			// if the data are not valid, don't close
			return;
		}

		HydrologyNode node = new HydrologyNode();
		PropList p = new PropList("");
		string temp = null;

		temp = __textTextField.getText().Trim();
		temp = StringUtil.replaceString(temp, "\"", "'");
		p.set("Text", temp);

		temp = __xTextField.getText().Trim() + "," + __yTextField.getText().Trim();
		p.set("Point", temp);

		temp = __textPositionComboBox.getSelected();
		p.set("TextPosition", temp);

		temp = __fontNameComboBox.getSelected().Trim();
		p.set("FontName", temp);

		temp = __fontSizeTextField.getText().Trim();
		p.set("OriginalFontSize", temp);

		temp = __fontStyleComboBox.getSelected().Trim();
		p.set("FontStyle", temp);
		node.setAssociatedObject(p);

		node.setDirty(true);

		__parent.updateAnnotation(__nodeNum, node);
		__applyButton.setEnabled(false);
		__okButton.setEnabled(false);
	}

	/// <summary>
	/// Closes the GUI.
	/// </summary>
	private void closeWindow()
	{
		setVisible(false);
		dispose();
	}

	/// <summary>
	/// Displays the annotation values from the annotation proplist in the components in the GUI.
	/// </summary>
	private void displayPropListValues()
	{
		PropList p = (PropList)__node.getAssociatedObject();

		string temp = null;
		string val = null;

		val = p.getValue("Text").Trim();
		__textTextField.setText(val);

		val = p.getValue("Point").Trim();
		temp = StringUtil.getToken(val, ",", 0, 0);
		__xTextField.setText(StringUtil.formatString(temp, "%20.6f").Trim());
		temp = StringUtil.getToken(val, ",", 0, 1);
		__yTextField.setText(StringUtil.formatString(temp, "%20.6f").Trim());

		val = p.getValue("TextPosition").Trim();
		__textPositionComboBox.select(val);

		val = p.getValue("FontName").Trim();
		__fontNameComboBox.select(val);

		val = p.getValue("OriginalFontSize").Trim();
		__fontSizeTextField.setText(val);

		val = p.getValue("FontStyle").Trim();
		__fontStyleComboBox.select(val);
	}

	/// <summary>
	/// Responds when users press the enter button in an edit field.  Saves the changes and closes the GUI. </summary>
	/// <param name="event"> that KeyEvent that happened. </param>
	public virtual void keyPressed(KeyEvent @event)
	{
		if (@event.getKeyCode() == KeyEvent.VK_ENTER)
		{
			if (__editable)
			{
				applyChanges();
			}
			closeWindow();
		}
	}

	/// <summary>
	/// Responds after users presses a key -- tries to validate the data that has been entered.
	/// </summary>
	public virtual void keyReleased(KeyEvent @event)
	{
		validateData();
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void keyTyped(KeyEvent @event)
	{
	}

	/// <summary>
	/// Sets up the GUI.
	/// </summary>
	private void setupGUI()
	{
		addWindowListener(this);

		JPanel panel = new JPanel();
		panel.setLayout(new GridBagLayout());

		int y = 0;

		JGUIUtil.addComponent(panel, new JLabel("Text:"), 0, y++, 1, 1, 1, 1, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(panel, new JLabel("X:"), 0, y++, 1, 1, 1, 1, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(panel, new JLabel("Y:"), 0, y++, 1, 1, 1, 1, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(panel, new JLabel("Text Position:"), 0, y++, 1, 1, 1, 1, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(panel, new JLabel("Font Name:"), 0, y++, 1, 1, 1, 1, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(panel, new JLabel("Font Size:"), 0, y++, 1, 1, 1, 1, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(panel, new JLabel("Font Style:"), 0, y++, 1, 1, 1, 1, GridBagConstraints.NONE, GridBagConstraints.EAST);

		__textTextField = new JTextField(30);
		__textFieldBackground = __textTextField.getBackground();
		__xTextField = new JTextField(15);
		__yTextField = new JTextField(15);

		__textPositionComboBox = new SimpleJComboBox(false);
		string[] positions = GRText.getTextPositions();
		__textPositionComboBox.setMaximumRowCount(positions.Length);
		IList<string> positionsChoices = new List<string>();
		for (int i = 0; i < positions.Length; i++)
		{
			positionsChoices.Add(positions[i]);
		}
		__textPositionComboBox.setData(positionsChoices);

		__fontNameComboBox = JGUIUtil.newFontNameJComboBox();
		__fontSizeTextField = new JTextField(7);
		__fontStyleComboBox = JGUIUtil.newFontStyleJComboBox();

		displayPropListValues();

		__textTextField.addKeyListener(this);
		__xTextField.addKeyListener(this);
		__yTextField.addKeyListener(this);
		__fontSizeTextField.addKeyListener(this);
		__textPositionComboBox.addActionListener(this);
		__fontNameComboBox.addActionListener(this);
		__fontStyleComboBox.addActionListener(this);

		y = 0;
		JGUIUtil.addComponent(panel, __textTextField, 1, y++, 1, 1, 1, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(panel, __xTextField, 1, y++, 1, 1, 1, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(panel, __yTextField, 1, y++, 1, 1, 1, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(panel, __textPositionComboBox, 1, y++, 1, 1, 1, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(panel, __fontNameComboBox, 1, y++, 1, 1, 1, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(panel, __fontSizeTextField, 1, y++, 1, 1, 1, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(panel, __fontStyleComboBox, 1, y++, 1, 1, 1, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);

		if (!__editable)
		{
			__textTextField.setEditable(false);
			__xTextField.setEditable(false);
			__yTextField.setEditable(false);
			__textPositionComboBox.setEnabled(false);
			__fontNameComboBox.setEnabled(false);
			__fontSizeTextField.setEditable(false);
			__fontStyleComboBox.setEnabled(false);
		}

		__applyButton = new JButton(__BUTTON_APPLY);
		if (__editable)
		{
			__applyButton.setToolTipText("Apply changes.");
			__applyButton.addActionListener(this);
		}
		__applyButton.setEnabled(false);

		JButton cancelButton = new JButton(__BUTTON_CANCEL);
		cancelButton.setToolTipText("Discard changes and return.");
		cancelButton.addActionListener(this);

		__okButton = new JButton(__BUTTON_OK);
		__okButton.setToolTipText("Accept changes and return.");
		__okButton.setEnabled(false);
		__okButton.addActionListener(this);

		JPanel southPanel = new JPanel();
		southPanel.setLayout(new GridBagLayout());

		if (__editable)
		{
			JGUIUtil.addComponent(southPanel, __applyButton, 0, 0, 1, 1, 1, 1, 0, 10, 0, 10, GridBagConstraints.NONE, GridBagConstraints.EAST);
		}

		int space = 0;
		if (!__editable)
		{
			space = 1;
		}

		JGUIUtil.addComponent(southPanel, __okButton, 1, 0, 1, 1, space, space, 0, 10, 0, 10, GridBagConstraints.NONE, GridBagConstraints.EAST);

		if (__editable)
		{
			JGUIUtil.addComponent(southPanel, cancelButton, 2, 0, 1, 1, 0, 0, 0, 10, 0, 10, GridBagConstraints.NONE, GridBagConstraints.EAST);
		}
		else
		{
			__okButton.setEnabled(true);
		}

		getContentPane().add("Center", panel);
		getContentPane().add("South", southPanel);

		string app = JGUIUtil.getAppNameForWindows();
		if (string.ReferenceEquals(app, null) || app.Trim().Equals(""))
		{
			app = "";
		}
		else
		{
			app += " - ";
		}
		setTitle(app + "Node Properties");

		pack();
		setSize(getWidth() + 100, getHeight());
		JGUIUtil.center(this);
		setVisible(true);
	}

	/// <summary>
	/// Validates data entered in the GUI.  If any values are invalid (non-numbers in
	/// the X and Y fields, blank label field), the OK button is disabled and the field is highlighted in red. </summary>
	/// <returns> true if all the text is valid.  False if not. </returns>
	private bool validateData()
	{
		string text = __textTextField.getText().Trim();
		double x = -1;
		bool badX = false;
		double y = -1;
		bool badY = false;

		GRLimits limits = __parent.getDataLimits();

		// make sure the X value is a double and that it is within the range
		// of the X values in the data limits
		try
		{
			x = (Convert.ToDouble(__xTextField.getText().Trim()));
			if (x < limits.getLeftX() || x > limits.getRightX())
			{
				badX = true;
			}
		}
		catch (Exception)
		{
			badX = true;
		}

		// if the X value is not valid, set the textfield red to show this. 
		// Otherwise, make sure the text has the proper background color.
		if (badX)
		{
			__xTextField.setBackground(Color.red);
		}
		else
		{
			__xTextField.setBackground(__textFieldBackground);
		}

		// make sure the Y value is a double and that it is within the range
		// of the Y values in the data limits
		try
		{
			y = (Convert.ToDouble(__yTextField.getText().Trim()));
			if (y < limits.getBottomY() || y > limits.getTopY())
			{
				badY = true;
			}
		}
		catch (Exception)
		{
			badY = true;
		}

		// if the Y value is not valid, set the textfield red to show this. 
		// Otherwise, make sure the text has the proper background color.
		if (badY)
		{
			__yTextField.setBackground(Color.red);
		}
		else
		{
			__yTextField.setBackground(__textFieldBackground);
		}

		// make sure that the text is not an empty string.  If it is, make
		// its textfield red.  Otherwise, the textfield will have the normal textfield color.
		bool badText = false;
		if (text.Trim().Equals(""))
		{
			badText = true;
			__textTextField.setBackground(Color.red);
		}
		else
		{
			__textTextField.setBackground(__textFieldBackground);
		}

		// make sure that the font size is an integer greater than 0.  If not,
		// set its textfield to red.  Otherwise the textfield will have a normal textfield color.
		bool badFontSize = false;
		int size = 0;
		try
		{
			size = (Convert.ToInt32(__fontSizeTextField.getText().Trim()));
		}
		catch (Exception)
		{
			badFontSize = true;
		}

		if (size <= 0)
		{
			badFontSize = true;
		}

		if (badFontSize)
		{
			__fontSizeTextField.setBackground(Color.red);
		}
		else
		{
			__fontSizeTextField.setBackground(__textFieldBackground);
		}

		if (!badText && !badX && !badY && !badFontSize)
		{
			// if all the data validated properly then mark whether the data are dirty or not.
			// OK is only active if the data are valid and something is dirty.
			bool dirty = false;
			PropList p = (PropList)__node.getAssociatedObject();

			string temp = p.getValue("Text").Trim();
			if (!temp.Equals(__textTextField.getText().Trim()))
			{
				dirty = true;
			}

			string val = p.getValue("Point").Trim();
			temp = StringUtil.getToken(val, ",", 0, 0);

			if (!temp.Equals(__xTextField.getText().Trim()))
			{
				dirty = true;
			}

			temp = StringUtil.getToken(val, ",", 0, 1);
			if (!temp.Equals(__yTextField.getText().Trim()))
			{
				dirty = true;
			}

			temp = p.getValue("OriginalFontSize").Trim();
			if (!temp.Equals(__fontSizeTextField.getText().Trim()))
			{
				dirty = true;
			}

			temp = p.getValue("FontName").Trim();
			if (!temp.Equals(__fontNameComboBox.getSelected().Trim()))
			{
				dirty = true;
			}

			temp = p.getValue("FontStyle").Trim();
			if (!temp.Equals(__fontStyleComboBox.getSelected().Trim()))
			{
				dirty = true;
			}

			temp = p.getValue("TextPosition").Trim();
			if (!temp.Equals(__textPositionComboBox.getSelected().Trim()))
			{
				dirty = true;
			}

			__applyButton.setEnabled(dirty);
			__okButton.setEnabled(dirty);
			return true;
		}
		else
		{
			// if the data aren't valid, the ok button is not enabled
			__applyButton.setEnabled(false);
			__okButton.setEnabled(false);
			return false;
		}
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void windowActivated(WindowEvent @event)
	{
	}

	/// <summary>
	/// Closes the GUI.
	/// </summary>
	public virtual void windowClosing(WindowEvent @event)
	{
		closeWindow();
	}
	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void windowClosed(WindowEvent @event)
	{
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void windowDeactivated(WindowEvent @event)
	{
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void windowDeiconified(WindowEvent @event)
	{
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void windowIconified(WindowEvent @event)
	{
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void windowOpened(WindowEvent @event)
	{
	}

	}

}