﻿using System.Collections.Generic;

// StateMod_Network_AnnotationDataListJPanel - panel to hold a list of StateMod_Network_AnnotationData

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


	using JGUIUtil = RTi.Util.GUI.JGUIUtil;

	/// <summary>
	/// A panel to hold a list of StateMod_Network_AnnotationData, to allow interaction such as clearing the list
	/// of annotations.
	/// </summary>
	public class StateMod_Network_AnnotationDataListJPanel : JPanel, ActionListener
	{

	/// <summary>
	/// The JList that manages the list of annotations (labels).
	/// </summary>
	private JList __annotationJList = null;

	/// <summary>
	/// Data for the list.
	/// </summary>
	private DefaultListModel __annotationJListModel = new DefaultListModel();

	/// <summary>
	/// Indicate whether the component should be set invisible when the list is empty.
	/// </summary>
	private bool __hideIfEmpty = false;

	/// <summary>
	/// The list of annotations maintained in the GeoView.
	/// </summary>
	private IList<StateMod_Network_AnnotationData> __annotationDataList = null;

	/// <summary>
	/// The component that actually renders the annotations - need this if the popup menu changes the
	/// list of displayed annotations (such as clearing the list).
	/// </summary>
	private StateMod_Network_JComponent __networkJComponent = null;

	/// <summary>
	/// Menu items.
	/// </summary>
	private string __RemoveAllAnnotationsString = "Remove All Annotations";

	/// <summary>
	/// Constructor. </summary>
	/// <param name="annotationDataList"> list of annotation data, if available (can pass null and reset the list
	/// later by calling setAnnotationData()). </param>
	/// <param name="hideIfEmpty"> if true, set the panel to not visible if the list is empty - this may be appropriate
	/// if UI real estate is in short supply and annotations should only be shown if used </param>
	public StateMod_Network_AnnotationDataListJPanel(IList<StateMod_Network_AnnotationData> annotationDataList, StateMod_Network_JComponent networkJComponent, bool hideIfEmpty) : base()
	{
		// Set up the layout manager
		this.setLayout(new GridBagLayout());
		this.setBorder(BorderFactory.createTitledBorder("Annotations"));
		int y = 0;
		Insets insetsTLBR = new Insets(0, 0, 0, 0);
		__annotationJList = new JList();
		if (annotationDataList != null)
		{
			setAnnotationData(annotationDataList);
		}
		JGUIUtil.addComponent(this, new JScrollPane(__annotationJList), 0, y, 1, 1, 1.0, 1.0, insetsTLBR, GridBagConstraints.BOTH, GridBagConstraints.SOUTH);
		__hideIfEmpty = hideIfEmpty;
		__networkJComponent = networkJComponent;

		// Add popup for actions on annotations

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final javax.swing.JPopupMenu popupMenu = new javax.swing.JPopupMenu();
		JPopupMenu popupMenu = new JPopupMenu();
		JMenuItem removeAllAnnotationsJMenuItem = new JMenuItem(__RemoveAllAnnotationsString);
		removeAllAnnotationsJMenuItem.addActionListener(this);
		popupMenu.add(removeAllAnnotationsJMenuItem);
		__annotationJList.addMouseListener(new MouseAdapterAnonymousInnerClass(this, popupMenu));

		checkVisibility();
	}

	private class MouseAdapterAnonymousInnerClass : MouseAdapter
	{
		private readonly StateMod_Network_AnnotationDataListJPanel outerInstance;

		private JPopupMenu popupMenu;

		public MouseAdapterAnonymousInnerClass(StateMod_Network_AnnotationDataListJPanel outerInstance, JPopupMenu popupMenu)
		{
			this.outerInstance = outerInstance;
			this.popupMenu = popupMenu;
		}

		public void mouseClicked(MouseEvent me)
		{
			// if right mouse button clicked (or me.isPopupTrigger())
			if (SwingUtilities.isRightMouseButton(me))
			{
					popupMenu.show(outerInstance.__annotationJList, me.getX(), me.getY());
			}
		}
	}

	/// <summary>
	/// Handle action events.
	/// </summary>
	public virtual void actionPerformed(ActionEvent @event)
	{
		string action = @event.getActionCommand();
		if (action.Equals(__RemoveAllAnnotationsString))
		{
			// Remove from the list and the original data that was passed in
			__annotationJListModel.clear();
			if (__annotationDataList != null)
			{
				if (__networkJComponent != null)
				{
					// TODO SAM 2010-12-28 Enable
					__networkJComponent.clearAnnotations(); // This will modify __annotationDataList
				}
			}
			checkVisibility();
		}
	}

	/// <summary>
	/// Add an annotation to the list.
	/// </summary>
	public virtual void addAnnotation(StateMod_Network_AnnotationData annotationData)
	{
		// For now just add at the end...
		if (annotationData != null)
		{
			__annotationJListModel.addElement(annotationData.getLabel());
		}
		checkVisibility();
	}

	/// <summary>
	/// Check the annotation list visibility.  If hideIfEmpty=true, then set to not visible if the list is empty.
	/// </summary>
	private void checkVisibility()
	{
		if (__hideIfEmpty && __annotationJListModel.size() == 0)
		{
			setVisible(false);
		}
		else
		{
			setVisible(true);
		}
	}

	/// <summary>
	/// Set the annotation data and repopulate the list.
	/// </summary>
	public virtual void setAnnotationData(IList<StateMod_Network_AnnotationData> annotationDataList)
	{
		__annotationDataList = annotationDataList;
		IList<string> annotationLabelList = new List<string>(annotationDataList.Count);
		foreach (StateMod_Network_AnnotationData annotationData in annotationDataList)
		{
			annotationLabelList.Add(annotationData.getLabel());
		}
		// Sort the array before adding
		annotationLabelList.Sort();
		__annotationJListModel = new DefaultListModel();
		foreach (string annotationLabel in annotationLabelList)
		{
			__annotationJListModel.addElement(annotationLabel);
		}
		__annotationJList.setModel(__annotationJListModel);
		checkVisibility();
	}

	/// <summary>
	/// Set the GeoView that is rendering the map.
	/// </summary>
	public virtual void setNetworkJComponent(StateMod_Network_JComponent networkJComponent)
	{
		__networkJComponent = networkJComponent;
	}

	}

}