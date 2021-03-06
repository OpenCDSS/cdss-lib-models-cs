﻿using System;
using System.Collections.Generic;

// StateCU_DataSet_JFrame - an object to display a StateCU data set.

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
// StateCU_DataSet_JFrame - an object to display a StateCU data set.
//-----------------------------------------------------------------------------
// History:
//
// 2003-06-08	Steven A. Malers, RTi	Created class.
// 2003-07-07	SAM, RTi		Support component groups.
// 2003-07-13	SAM, RTi		Update to handle
//					RTi.Util.IO.DataSetComponent being used
//					for data set components now.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
//-----------------------------------------------------------------------------

namespace DWR.StateCU
{


	using JGUIUtil = RTi.Util.GUI.JGUIUtil;
	using DataSetComponent = RTi.Util.IO.DataSetComponent;

	/// <summary>
	/// This StateCU_DataSet_JFrame class displays a StateCU_DataSet and its components.
	/// Only one instance of this interface should be created.  The setVisible() method
	/// can be called to hide/show the interface.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class StateCU_DataSet_JFrame extends javax.swing.JFrame
	public class StateCU_DataSet_JFrame : JFrame
	{
	//implements ChangeListener

	private StateCU_DataSet __dataset = null;

	/// <summary>
	/// Construct a StateCU_DataSet_JFrame and optionally set visible. </summary>
	/// <param name="parent"> JFrame from which this instance is constructed. </param>
	/// <param name="dataset"> StateCU_DataSet that is being displayed/managed. </param>
	/// <param name="title"> Title to be displayed. </param>
	/// <param name="is_visible"> Indicates whether the display should be made visible at
	/// creation. </param>
	public StateCU_DataSet_JFrame(JFrame parent, StateCU_DataSet dataset, string title, bool is_visible)
	{
		__dataset = dataset;
		initialize(title, is_visible);
	}

	/// <summary>
	/// Initialize the interface. </summary>
	/// <param name="title"> Title to be displayed. </param>
	/// <param name="is_visible"> Indicates whether the display should be made visible at
	/// creation. </param>
	private void initialize(string title, bool is_visible)
	{
		GridBagLayout gbl = new GridBagLayout();
		Insets insetsTLBR = new Insets(2, 2, 2, 2); // space around text
								// area

		// Add a panel to hold the main components...

		JPanel display_JPanel = new JPanel();
		display_JPanel.setLayout(gbl);
		getContentPane().add(display_JPanel);

		JTabbedPane dataset_JTabbedPane = new JTabbedPane();
		//dataset_JTabbedPane.addChangeListener ( this );
		JGUIUtil.addComponent(display_JPanel, dataset_JTabbedPane, 0, 0, 10, 1, 0, 0, insetsTLBR, GridBagConstraints.BOTH, GridBagConstraints.CENTER);

		int y = 0; // Used for vertical positioning.

		//
		// Data Set Components...
		//
		// 1 row per component, each column width of 1
		//

		JPanel components_JPanel = new JPanel();
		components_JPanel.setLayout(gbl);
		dataset_JTabbedPane.addTab("Components", null, components_JPanel, "Components");
		// Add the headers...
		int x = 0;
		JGUIUtil.addComponent(components_JPanel, new JLabel("Component"), x, y, 1, 1, 0.0, 0.0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(components_JPanel, new JLabel("Created How"), ++x, y, 1, 1, 0.0, 0.0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(components_JPanel, new JLabel("Input File"), ++x, y, 1, 1, 0.0, 0.0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(components_JPanel, new JLabel("Count"), ++x, y, 1, 1, 0.0, 0.0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(components_JPanel, new JLabel("Incomplete"), ++x, y, 1, 1, 0.0, 0.0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(components_JPanel, new JLabel("Data File"), ++x, y, 1, 1, 0.0, 0.0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);

		// Now add the contents for each component...
		IList<DataSetComponent> components_Vector = __dataset.getComponents();
		int size = components_Vector.Count;
		DataSetComponent component;
		y = 0; // Incremented below.  True row 0 is used for headers above.
		IList<DataSetComponent> data = null;
		int data_size = 0;
		for (int i = 0; i < size; i++)
		{
			x = 0;
			component = components_Vector[i];
			JTextField component_JTextField = new JTextField(component.getComponentName(), 20);
			component_JTextField.setEditable(false);
			JGUIUtil.addComponent(components_JPanel, component_JTextField, x, ++y, 1, 1, 0.0, 0.0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);

			if (component.isGroup())
			{
				// Get each of the sub-component's information...
				if (component.getData() == null)
				{
					continue;
				}
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<RTi.Util.IO.DataSetComponent> data0 = (java.util.List<RTi.Util.IO.DataSetComponent>)component.getData();
				IList<DataSetComponent> data0 = (IList<DataSetComponent>)component.getData();
				data = data0;
				data_size = 0;
				if (data != null)
				{
					data_size = data.Count;
				}
				for (int j = 0; j < data_size; j++)
				{
					x = 0;
					component = (DataSetComponent)data[j];
					component_JTextField = new JTextField("    " + component.getComponentName(), 20);
					component_JTextField.setEditable(false);
					JGUIUtil.addComponent(components_JPanel, component_JTextField, x, ++y, 1, 1, 0.0, 0.0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);

					// REVISIT - need to track create method
					JTextField from_JTextField = new JTextField("?", 10);
						//from.getName(), 10 );
					from_JTextField.setEditable(false);
					JGUIUtil.addComponent(components_JPanel, from_JTextField, ++x, y, 1, 1, 0.0, 0.0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);

					// REVISIT - need to indicate list or commands
					// file
					JTextField inputfile_JTextField = new JTextField("?", 10);
						//from.getName(), 10 );
					inputfile_JTextField.setEditable(false);
					JGUIUtil.addComponent(components_JPanel, inputfile_JTextField, ++x, y, 1, 1, 0.0, 0.0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);

					int count = 0;
					try
					{
						count = ((System.Collections.IList)component.getData()).Count;
					}
					catch (Exception)
					{
						// REVISIT
						// Probably because not a Vector (GIS
						// and control) - need a more graceful
						// way to handle.
						count = -1;
					}
					JTextField object_JTextField = null;
					if (count >= 0)
					{
						object_JTextField = new JTextField("" + count, 5);
					}
					else
					{
						object_JTextField = new JTextField("-", 5);
					}
					object_JTextField.setEditable(false);
					JGUIUtil.addComponent(components_JPanel, object_JTextField, ++x, y, 1, 1, 0.0, 0.0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);

					// REVISIT - need to indicate how many are
					// incomplete

					JTextField incomplete_JTextField = new JTextField("?", 5);
						//from.getName(), 10 );
					incomplete_JTextField.setEditable(false);
					JGUIUtil.addComponent(components_JPanel, incomplete_JTextField, ++x, y, 1, 1, 0.0, 0.0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);

					JTextField datafile_JTextField = new JTextField(component.getDataFileName(), 15);
					datafile_JTextField.setEditable(false);
					JGUIUtil.addComponent(components_JPanel, datafile_JTextField, ++x, y, 1, 1, 0.0, 0.0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);
				}
			}
		}

		//
		// Data Set Properties...
		//
		// 1 grid for labels, 6 for text fields (resizable).
		//

		y = 0;
		JPanel properties_JPanel = new JPanel();
		properties_JPanel.setLayout(gbl);
		JGUIUtil.addComponent(properties_JPanel, new JLabel("Data Set Type:"), 0, y, 1, 1, 0.0, 0.0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JTextField dataset_type_JTextField = new JTextField(__dataset.getDataSetName(), 20);
		dataset_type_JTextField.setEditable(false);
		JGUIUtil.addComponent(properties_JPanel, dataset_type_JTextField, 1, y, 2, 1, 0.0, 1.0, insetsTLBR, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);

		JGUIUtil.addComponent(properties_JPanel, new JLabel("Data Set Base Name:"), 0, ++y, 1, 1, 0.0, 0.0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JTextField dataset_basename_JTextField = new JTextField(__dataset.getBaseName(), 20);
		dataset_basename_JTextField.setEditable(false);
		JGUIUtil.addComponent(properties_JPanel, dataset_basename_JTextField, 1, y, 2, 1, 0.0, 1.0, insetsTLBR, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);

		JGUIUtil.addComponent(properties_JPanel, new JLabel("Data Set Directory:"), 0, ++y, 1, 1, 0.0, 0.0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JTextField dataset_dir_JTextField = new JTextField(__dataset.getDataSetDirectory(), 40);
		dataset_dir_JTextField.setEditable(false);
		JGUIUtil.addComponent(properties_JPanel, dataset_dir_JTextField, 1, y, 6, 1, 0.0, 1.0, insetsTLBR, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);

		JGUIUtil.addComponent(properties_JPanel, new JLabel("Data Set File:"), 0, ++y, 1, 1, 0.0, 0.0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JTextField dataset_file_JTextField = new JTextField(__dataset.getDataSetFileName(), 20);
		dataset_file_JTextField.setEditable(false);
		JGUIUtil.addComponent(properties_JPanel, dataset_file_JTextField, 1, y, 2, 1, 0.0, 1.0, insetsTLBR, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);

		dataset_JTabbedPane.addTab("Properties", null, properties_JPanel, "Properties");

		// Show the interface...

		if ((string.ReferenceEquals(title, null)) || (title.Length == 0))
		{
			setTitle("Data Set Manager");
		}
		else
		{
			setTitle(title);
		}
		pack();
		JGUIUtil.center(this);
		setResizable(true);
		setVisible(is_visible);
	}

	} // End StateCU_DataSet_JFrame

}