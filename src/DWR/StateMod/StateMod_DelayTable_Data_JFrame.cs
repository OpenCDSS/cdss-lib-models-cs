using System.Collections.Generic;

// StateMod_DelayTable_Data_JFrame - JFrame that displays DelayTable data in a tabular format.

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

// PRINTING CHECKED
// ----------------------------------------------------------------------------
// StateMod_DelayTable_Data_JFrame - This is a JFrame that displays DelayTable
//	data in a tabular format.
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
// 
// 2005-01-17	J. Thomas Sapienza, RTi	Initial version.
// 2005-01-20	JTS, RTi		Following review:
//					* Improved some loop performance.
//					* Removed getDataType().
//					* Title string is now passed to the
//					  super constructor.
//					* Editability of data in the worksheet
//					  is now passed in via the constructor.
// 2005-03-28	JTS, RTi		Adjusted GUI size.
// 2005-03-29	JTS, RTi		* Now uses the new Data Table Models and
//					  Cell Renderers.
//					* Values are only displayed in percents
//					  now, no fractions.
// 2005-03-31	JTS, RTi		Added the checkbox for displaying 
//					(or hiding) totals lines in the table.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace DWR.StateMod
{


	using JGUIUtil = RTi.Util.GUI.JGUIUtil;
	using JScrollWorksheet = RTi.Util.GUI.JScrollWorksheet;

	/// <summary>
	/// This class is a JFrame for displaying a Vector of StateMod_DelayTable data in
	/// a worksheet.  The worksheet data can be exported to a file or printed.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class StateMod_DelayTable_Data_JFrame extends StateMod_Data_JFrame
	public class StateMod_DelayTable_Data_JFrame : StateMod_Data_JFrame
	{

	/// <summary>
	/// Whether this JFrame is displaying monthly or daily delay table data.
	/// </summary>
	private bool __monthlyData = true;

	/// <summary>
	/// The checkbox for selecting whether to show rows with totals or not.
	/// </summary>
	private JCheckBox __checkBox = null;

	/// <summary>
	/// The table model for the worksheet in the GUI.
	/// </summary>
	private StateMod_DelayTable_Data_TableModel __tableModel = null;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="data"> the data to display in the worksheet.  Can be null or empty, in 
	/// which case an empty worksheet is shown. </param>
	/// <param name="titleString"> the String to display as the GUI title. </param>
	/// <param name="monthlyData"> if true, the delay tables are showing monthly data.  If
	/// false, they are showing daily data. </param>
	/// <param name="returnIsPercent"> whether the return amounts are in percents (true) or
	/// fractions (false). </param>
	/// <param name="editable"> whether the data in the JFrame can be edited or not.  If true
	/// the data can be edited, if false they can not. </param>
	/// <exception cref="Exception"> if there is an error building the worksheet. </exception>
	/// @deprecated use the other one without returnIsPercent. 
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public StateMod_DelayTable_Data_JFrame(java.util.List<StateMod_DelayTable_Data_TableModel> data, String titleString, boolean monthlyData, boolean returnIsPercent, boolean editable) throws Exception
	public StateMod_DelayTable_Data_JFrame(IList<StateMod_DelayTable_Data_TableModel> data, string titleString, bool monthlyData, bool returnIsPercent, bool editable) : this(data, titleString, monthlyData, editable)
	{
	}

	/// <summary>
	/// Constructor. </summary>
	/// <param name="data"> the data to display in the worksheet.  Can be null or empty, in 
	/// which case an empty worksheet is shown. </param>
	/// <param name="titleString"> the String to display as the GUI title. </param>
	/// <param name="monthlyData"> if true, the delay tables are showing monthly data.  If
	/// false, they are showing daily data. </param>
	/// <param name="editable"> whether the data in the JFrame can be edited or not.  If true
	/// the data can be edited, if false they can not. </param>
	/// <exception cref="Exception"> if there is an error building the worksheet. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public StateMod_DelayTable_Data_JFrame(java.util.List<StateMod_DelayTable_Data_TableModel> data, String titleString, boolean monthlyData, boolean editable) throws Exception
	public StateMod_DelayTable_Data_JFrame(IList<StateMod_DelayTable_Data_TableModel> data, string titleString, bool monthlyData, bool editable) : base()
	{
		__monthlyData = monthlyData;
		initialize(data, titleString, editable);

		JPanel panel = new JPanel();
		panel.setLayout(new GridBagLayout());
		JLabel label = new JLabel("Show totals: ");
		JGUIUtil.addComponent(panel, label, 0, 0, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		__checkBox = new JCheckBox((string)null, true);
		__checkBox.addActionListener(this);
		JGUIUtil.addComponent(panel, __checkBox, 1, 0, 1, 1, 1, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);
		getContentPane().add("North", panel);
		pack();

		__tableModel = (StateMod_DelayTable_Data_TableModel)_worksheet.getTableModel();
		__tableModel.setJWorksheet(_worksheet);

		setSize(300, 600);
	}

	/// <summary>
	/// Responds to action events. </summary>
	/// <param name="event"> the ActionEvent that occurred. </param>
	public override void actionPerformed(ActionEvent @event)
	{
		if (@event.getSource() == __checkBox)
		{
			__tableModel.setShowTotals(__checkBox.isSelected());
		}
		else
		{
			base.actionPerformed(@event);
		}
	}

	/// <summary>
	/// Called when the Apply button is pressed. This commits any changes to the data objects.
	/// </summary>
	protected internal override void apply()
	{
		StateMod_DelayTable delay = null;
		int size = _data.Count;
		for (int i = 0; i < size; i++)
		{
			delay = (StateMod_DelayTable)_data[i];
			delay.createBackup();
		}
	}

	/// <summary>
	/// Creates a JScrollWorksheet for the current data and returns it. </summary>
	/// <returns> a JScrollWorksheet containing the data Vector passed in to the constructor. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected RTi.Util.GUI.JScrollWorksheet buildJScrollWorksheet() throws Exception
	protected internal override JScrollWorksheet buildJScrollWorksheet()
	{
		StateMod_DelayTable_Data_TableModel tableModel = new StateMod_DelayTable_Data_TableModel(_data, __monthlyData, _editable);
		StateMod_DelayTable_Data_CellRenderer cellRenderer = new StateMod_DelayTable_Data_CellRenderer(tableModel);

		// Note (JTS - 2005-03-31)
		// while it would seem the right thing to do would be to here assign
		// the table model to __tableModel, JTS found that in practice for
		// some reason once the constructor was returned to (in order to finish
		// building the GUI with checkbox) that the table model was then 
		// null!  JTS didn't understand why it was happening but didn't
		// want to spend too long investigating.

		// _props is defined in the super class
		return new JScrollWorksheet(cellRenderer, tableModel, _props);
	}

	/// <summary>
	/// Called when the cancel button is pressed.  This discards any changes made to the data objects.
	/// </summary>
	protected internal override void cancel()
	{
		StateMod_DelayTable delay = null;
		int size = _data.Count;
		for (int i = 0; i < size; i++)
		{
			delay = (StateMod_DelayTable)_data[i];
			delay.restoreOriginal();
		}
	}

	/// <summary>
	/// Creates backups of all the data objects in the Vector so that changes can later be cancelled if necessary.
	/// </summary>
	protected internal override void createDataBackup()
	{
		StateMod_DelayTable delay = null;
		int size = _data.Count;
		for (int i = 0; i < size; i++)
		{
			delay = (StateMod_DelayTable)_data[i];
			delay.createBackup();
		}
	}

	}

}