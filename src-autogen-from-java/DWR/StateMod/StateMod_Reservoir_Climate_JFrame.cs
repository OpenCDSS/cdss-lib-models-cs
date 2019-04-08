using System;
using System.Collections.Generic;

// StateMod_Reservoir_Climate_JFrame - dialog to edit a reservoir's climate information.

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

//------------------------------------------------------------------------------
// StateMod_Reservoir_Climate_JFrame - dialog to edit a reservoir's climate 
//	information.  This information includes both evaporation/precipitation 
//	information.
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
// 
// 24 Dec 1997	Catherine E.		Created initial version of class
//		Nutting-Lane, RTi
// 25 Feb 1998	CEN, RTi		Added header information
// 01 Apr 2001	Steven A. Malers, RTi	Change GUI to JGUIUtil.  Add finalize().
//					Remove import *.
//------------------------------------------------------------------------------
// 2003-06-09	J. Thomas Sapienza, RTi	Initial swing version from 
//					SMresClimateFrame
// 2003-06-16	JTS, RTi		Javadoc'd.
// 2003-07-15	JTS, RTi		* Added status bar.
//					* Changed to use new dataset design.
// 2003-07-17	JTS, RTI		Change so that constructor takes a 
//					boolean that says whether the form's
//					data can be modified.
// 2003-07-23	JTS, RTi		Updated JWorksheet code following
//					JWorksheet revisions.
// 2003-08-03	SAM, RTi		Changed isDirty() back to setDirty().
// 2003-08-16	SAM, RTi		Update to reflect name changes in
//					StateMod_ReservoirClimate.
// 2003-08-25	JTS, RTi		* Added in a second worksheet.
//					* Changed the table model to use a 
//					  combo box for selecting the station
//					  id.
// 2003-08-29	SAM, RTi		Update due to changes in
//					StateMod_Reservoir.
// 2003-09-23	JTS, RTi		Uses new StateMod_GUIUtil code for
//					setting titles.
// 2003-10-13	JTS, RTi		* Worksheets now use multiple-line
//					  headers.
// 					* Added saveData().
//					* Added checkInput().
//					* Added apply and cancel buttons.
// 2004-01-21	JTS, RTi		Updated to use JScrollWorksheet and
//					the new row headers.
// 2004-07-15	JTS, RTi		* For data changes, enabled the
//					  Apply and Cancel buttons through new
//					  methods in the data classes.
//					* Changed layout of buttons to be
//					  aligned in the lower-right.
// 2004-10-28	SAM, RTi		Use table model specific to the climate
//					data.
// 2005-01-21	JTS, RTi		Table model constructor changed.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
//------------------------------------------------------------------------------
// EndHeader

namespace DWR.StateMod
{


	using MonthTS = RTi.TS.MonthTS;
	using TS = RTi.TS.TS;
	using JGUIUtil = RTi.Util.GUI.JGUIUtil;
	using JScrollWorksheet = RTi.Util.GUI.JScrollWorksheet;
	using JWorksheet = RTi.Util.GUI.JWorksheet;
	using ResponseJDialog = RTi.Util.GUI.ResponseJDialog;
	using PropList = RTi.Util.IO.PropList;
	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// This class is a gui for displaying the climate stations associated with 
	/// a reservoir, and adding and deleting stations from the reservoir.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class StateMod_Reservoir_Climate_JFrame extends javax.swing.JFrame implements java.awt.event.ActionListener, java.awt.event.KeyListener, java.awt.event.MouseListener, java.awt.event.WindowListener
	public class StateMod_Reservoir_Climate_JFrame : JFrame, ActionListener, KeyListener, MouseListener, WindowListener
	{

	/// <summary>
	/// Text to appear on the GUI buttons.
	/// </summary>
	private readonly string __BUTTON_ADD_PRECIPITATION_STATION = "Add precipitation station", __BUTTON_ADD_EVAPORATION_STATION = "Add evaporation station", __BUTTON_APPLY = "Apply", __BUTTON_CANCEL = "Cancel", __BUTTON_CLOSE = "Close", __BUTTON_DELETE_PRECIPITATION_STATION = "Delete Station", __BUTTON_HELP = "Help";

	/// <summary>
	/// Whether the gui data is editable or not.
	/// </summary>
	private bool __editable = false;

	/// <summary>
	/// GUI Buttons.
	/// </summary>
	private JButton __addEvap, __addPrecip, __closeJButton, __deleteStation, __helpJButton;

	/// <summary>
	/// Status bar textfields 
	/// </summary>
	private JTextField __messageJTextField, __statusJTextField;

	/// <summary>
	/// worksheet in which the list of precipitation stations will be displayed.
	/// </summary>
	private JWorksheet __worksheetP;

	/// <summary>
	/// Worksheet in which the list of evaporation stations will be displayed.
	/// </summary>
	private JWorksheet __worksheetE;

	/// <summary>
	/// The reservoir for which to display station data.
	/// </summary>
	private StateMod_Reservoir __currentRes;

	internal StateMod_ReservoirClimate_TableModel __tableModelP;
	internal StateMod_ReservoirClimate_TableModel __tableModelE;

	internal StateMod_DataSet __dataset;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="res"> the reservoir for which to display climate information. </param>
	/// <param name="editable"> whether the gui data is editable or not. </param>
	public StateMod_Reservoir_Climate_JFrame(StateMod_DataSet dataset, StateMod_Reservoir res, bool editable)
	{
		StateMod_GUIUtil.setTitle(this, dataset, res.getName() + " - Reservoir Climate Stations", null);
		JGUIUtil.setIcon(this, JGUIUtil.getIconImage());
		__dataset = dataset;

		__currentRes = res;

		__editable = editable;

		setupGUI();
	}

	private IList<StateMod_ReservoirClimate> getPrecipitationStations(IList<StateMod_ReservoirClimate> stations)
	{
		IList<StateMod_ReservoirClimate> v = new List<StateMod_ReservoirClimate>();
		StateMod_ReservoirClimate s = null;
		for (int i = 0; i < stations.Count; i++)
		{
			s = stations[i];
			if (s.getType() == StateMod_ReservoirClimate.CLIMATE_PTPX)
			{
				v.Add(s);
			}
		}
		return v;
	}

	private IList<StateMod_ReservoirClimate> getEvaporationStations(IList<StateMod_ReservoirClimate> stations)
	{
		IList<StateMod_ReservoirClimate> v = new List<StateMod_ReservoirClimate>();
		StateMod_ReservoirClimate s = null;
		for (int i = 0; i < stations.Count; i++)
		{
			s = stations[i];
			if (s.getType() == StateMod_ReservoirClimate.CLIMATE_EVAP)
			{
				v.Add(s);
			}
		}
		return v;
	}

	/// <summary>
	/// Reponds to action performed events. </summary>
	/// <param name="e"> the ActionEvent that happened. </param>
	public virtual void actionPerformed(ActionEvent e)
	{
		string action = e.getActionCommand();

		if (action.Equals(__BUTTON_ADD_PRECIPITATION_STATION))
		{
			StateMod_ReservoirClimate aClimateNode = new StateMod_ReservoirClimate();
			aClimateNode._isClone = true;
			aClimateNode.setType(StateMod_ReservoirClimate.CLIMATE_PTPX);
			__worksheetP.addRow(aClimateNode);
			__worksheetP.scrollToLastRow();
			__worksheetP.selectLastRow();
			checkDeleteStationButton();
		}
		else if (action.Equals(__BUTTON_ADD_EVAPORATION_STATION))
		{
			StateMod_ReservoirClimate aClimateNode = new StateMod_ReservoirClimate();
			aClimateNode._isClone = true;
			aClimateNode.setType(StateMod_ReservoirClimate.CLIMATE_EVAP);
			__worksheetE.addRow(aClimateNode);
			__worksheetE.scrollToLastRow();
			__worksheetE.selectLastRow();
			checkDeleteStationButton();
		}
		else if (action.Equals(__BUTTON_DELETE_PRECIPITATION_STATION))
		{
			int rowP = __worksheetP.getSelectedRow();
			int rowE = __worksheetE.getSelectedRow();
			int count = 0;
			if (rowP > -1)
			{
				count++;
			}
			if (rowE > -1)
			{
				count++;
			}
			if (count > 0)
			{
				string plural = "s";
				if (count == 1)
				{
					plural = "";
				}
				int x = (new ResponseJDialog(this, "Delete climate station" + plural, "Delete climate station" + plural + "?", ResponseJDialog.YES | ResponseJDialog.NO)).response();
				if (x == ResponseJDialog.NO)
				{
					return;
				}
				if (rowP > -1)
				{
					__worksheetP.deleteRow(rowP);
					__deleteStation.setEnabled(false);
					__worksheetP.scrollToLastRow();
				}
				if (rowE > -1)
				{
					__worksheetE.deleteRow(rowE);
					__deleteStation.setEnabled(false);
					__worksheetE.scrollToLastRow();
				}
			}
		}
		else if (action.Equals(__BUTTON_CLOSE))
		{
			if (saveData())
			{
				setVisible(false);
				dispose();
			}
		}
		else if (action.Equals(__BUTTON_APPLY))
		{
			saveData();
		}
		else if (action.Equals(__BUTTON_CANCEL))
		{
			setVisible(false);
			dispose();
		}
		else if (action.Equals(__BUTTON_HELP))
		{
			// REVISIT HELP (JTS - 2003-06-09)
		}
	}

	/// <summary>
	/// Checks the data to make sure that all the data are valid. </summary>
	/// <returns> 0 if the data are valid, 1 if errors exist and -1 if non-fatal errors
	/// exist. </returns>
	private int checkInput(JWorksheet worksheet, string name)
	{
		string routine = "StateMod_Reservoir_Climate_JFrame.checkInput";
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<StateMod_ReservoirClimate> v = (java.util.List<StateMod_ReservoirClimate>)worksheet.getAllData();
		IList<StateMod_ReservoirClimate> v = (IList<StateMod_ReservoirClimate>)worksheet.getAllData();

		int size = v.Count;
		StateMod_ReservoirClimate acct = null;
		string warning = "";
		string id;
		int fatalCount = 0;

		for (int i = 0; i < size; i++)
		{
			acct = (v[i]);

			id = acct.getID();

			if (id.Length > 12)
			{
				warning += "\n" + name + " reservoir climate ID ("
					+ id + ") is "
					+ "longer than 12 characters.";
				fatalCount++;
			}

			if (id.IndexOf(" ", StringComparison.Ordinal) > -1 || id.IndexOf("-", StringComparison.Ordinal) > -1)
			{
				warning += "\n" + name + " reservoir climate ID ("
					+ id + ") cannot "
					+ "contain spaces or dashes.";
				fatalCount++;
			}

			// the rest are handled automatically by the worksheet
		}
		// REVISIT - if daily time series are supplied, check for time series
		// and allow creation if not available.
		if (warning.Length > 0)
		{
			warning += "\nCorrect or Cancel.";
			Message.printWarning(1, routine, warning, this);
			if (fatalCount > 0)
			{
				// Fatal errors...
				Message.printStatus(1, routine, "Returning 1 from checkInput()");
				return 1;
			}
			else
			{ // Nonfatal errors...
				Message.printStatus(1, routine, "Returning -1 from checkInput()");
				return -1;
			}
		}
		else
		{ // No errors...
			Message.printStatus(1, routine, "Returning 0 from checkInput()");
			return 0;
		}
	}

	/// <summary>
	/// Saves the input back into the dataset. </summary>
	/// <returns> true if the data was saved successfuly.  False if not. </returns>
	private bool saveData()
	{
		string routine = "StateMod_Reservoir_Climate_JFrame.saveData";
		if (!__worksheetP.stopEditing())
		{
			// don't save if there are errors.
			Message.printWarning(1, routine, "There are errors in the " + "precipitation data " + "that must be corrected before data can be saved.", this);
			return false;
		}
		if (!__worksheetE.stopEditing())
		{
			// don't save if there are errors.
			Message.printWarning(1, routine, "There are errors in the " + "evaporation data " + "that must be corrected before data can be saved.", this);
			return false;
		}

		if (checkInput(__worksheetP, "Precipitation") > 0)
		{
			return false;
		}
		if (checkInput(__worksheetE, "Evaporation") > 0)
		{
			return false;
		}

		// if the Vectors are differently-sized, they're different
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<StateMod_ReservoirClimate> wv1 = (java.util.List<StateMod_ReservoirClimate>)__worksheetP.getAllData();
		IList<StateMod_ReservoirClimate> wv1 = (IList<StateMod_ReservoirClimate>)__worksheetP.getAllData(); // w for worksheet
		IList<StateMod_ReservoirClimate> rv1 = getPrecipitationStations(__currentRes.getClimates());
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<StateMod_ReservoirClimate> wv2 = (java.util.List<StateMod_ReservoirClimate>)__worksheetE.getAllData();
		IList<StateMod_ReservoirClimate> wv2 = (IList<StateMod_ReservoirClimate>)__worksheetE.getAllData(); // w for worksheet
		IList<StateMod_ReservoirClimate> rv2 = getEvaporationStations(__currentRes.getClimates());

		bool needToSave1 = !(StateMod_ReservoirClimate.Equals(wv1, rv1));
		bool needToSave2 = !(StateMod_ReservoirClimate.Equals(wv2, rv2));

		Message.printStatus(1, routine, "Saving Precip? .......[" + needToSave1 + "]");
		Message.printStatus(1, routine, "Saving Evap? .........[" + needToSave2 + "]");

		if (!needToSave1 && !needToSave2)
		{
			// there's nothing different -- users may even have deleted
			// some rights and added back in identical values
			return true;
		}

		int size = wv1.Count;
		IList<StateMod_ReservoirClimate> clone = new List<StateMod_ReservoirClimate>();
		StateMod_ReservoirClimate r = null;
		StateMod_ReservoirClimate cr = null;
		for (int i = 0; i < size; i++)
		{
			r = wv1[i];
			cr = (StateMod_ReservoirClimate)r.clone();
			cr._isClone = false;
			clone.Add(cr);
		}

		size = wv2.Count;
		for (int i = 0; i < size; i++)
		{
			r = wv2[i];
			cr = (StateMod_ReservoirClimate)r.clone();
			cr._isClone = false;
			clone.Add(cr);
		}

		__currentRes.setClimates(clone);
		__dataset.setDirty(StateMod_DataSet.COMP_DIVERSION_STATIONS, true);
		return true;
	}

	/// <summary>
	/// Checks to see if the __deleteStation button should be enabled or not.
	/// </summary>
	private void checkDeleteStationButton()
	{
		int rowP = __worksheetP.getSelectedRow();
		int rowE = __worksheetE.getSelectedRow();
		if (rowP == -1 && rowE == -1)
		{
			__deleteStation.setEnabled(false);
		}
		else
		{
			__deleteStation.setEnabled(true);
		}
	}

	/// <summary>
	/// Clean up before garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~StateMod_Reservoir_Climate_JFrame()
	{
		__worksheetP = null;
		__currentRes = null;
		__addPrecip = null;
		__addEvap = null;
		__deleteStation = null;
		__helpJButton = null;
		__closeJButton = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Responds to key pressed events; does nothing. </summary>
	/// <param name="e"> the KeyEvent that happened. </param>
	public virtual void keyPressed(KeyEvent e)
	{
	}

	/// <summary>
	/// Responds to key released events; checks if the __deleteStation button should
	/// be enabled. </summary>
	/// <param name="e"> the KeyEvent that happened. </param>
	public virtual void keyReleased(KeyEvent e)
	{
		checkDeleteStationButton();
	}

	/// <summary>
	/// Responds to key typed events; does nothing. </summary>
	/// <param name="e"> the KeyEvent that happened. </param>
	public virtual void keyTyped(KeyEvent e)
	{
	}

	/// <summary>
	/// Responds to mouse clicked events; does nothing. </summary>
	/// <param name="e"> the MouseEvent that happened. </param>
	public virtual void mouseClicked(MouseEvent e)
	{
	}

	/// <summary>
	/// Responds to mouse pressed events; does nothing. </summary>
	/// <param name="e"> the MouseEvent that happened. </param>
	public virtual void mousePressed(MouseEvent e)
	{
	}

	/// <summary>
	/// Responds to mouse released events; checks to see if the __deleteStation button
	/// should be enabled or not. </summary>
	/// <param name="e"> the MouseEvent that happened. </param>
	public virtual void mouseReleased(MouseEvent e)
	{
		checkDeleteStationButton();
	}

	/// <summary>
	/// Responds to mouse entered events; does nothing. </summary>
	/// <param name="e"> the MouseEvent that happened. </param>
	public virtual void mouseEntered(MouseEvent e)
	{
	}

	/// <summary>
	/// Responds to mouse exited events; does nothing. </summary>
	/// <param name="e"> the MouseEvent that happened. </param>
	public virtual void mouseExited(MouseEvent e)
	{
	}

	/// <summary>
	/// Sets up the GUI.
	/// </summary>
	public virtual void setupGUI()
	{
		string routine = "setupGUI";

		addWindowListener(this);

		__addPrecip = new JButton(__BUTTON_ADD_PRECIPITATION_STATION);
		__addEvap = new JButton(__BUTTON_ADD_EVAPORATION_STATION);
		__deleteStation = new JButton(__BUTTON_DELETE_PRECIPITATION_STATION);
		__deleteStation.setEnabled(false);
		__helpJButton = new JButton(__BUTTON_HELP);
		__helpJButton.setEnabled(false);
		__closeJButton = new JButton(__BUTTON_CLOSE);
		JButton cancelJButton = new JButton(__BUTTON_CANCEL);
		JButton applyJButton = new JButton(__BUTTON_APPLY);

		GridBagLayout gb = new GridBagLayout();
		JPanel bigPanel = new JPanel();
		bigPanel.setLayout(gb);

		FlowLayout fl = new FlowLayout(FlowLayout.RIGHT);
		JPanel p1 = new JPanel();
		p1.setLayout(fl);

		GridLayout gl = new GridLayout(2, 2, 1, 1);
		JPanel info_panel = new JPanel();
		info_panel.setLayout(gl);

		JPanel main_panel = new JPanel();
		main_panel.setLayout(new BorderLayout());

		info_panel.add(new JLabel("Reservoir:"));
		info_panel.add(new JLabel(__currentRes.getID()));
		info_panel.add(new JLabel("Reservoir name:"));
		info_panel.add(new JLabel(__currentRes.getName()));

		if (__editable)
		{
			p1.add(__addPrecip);
			p1.add(__addEvap);
			p1.add(__deleteStation);
		}
		p1.add(applyJButton);
		p1.add(cancelJButton);
	//	p1.add(__helpJButton);
		p1.add(__closeJButton);

		PropList p = new PropList("StateMod_Reservoir_Climate_JFrame.JWorksheet");
		p.add("JWorksheet.ShowPopupMenu=true");
		p.add("JWorksheet.AllowCopy=true");
		p.add("JWorksheet.SelectionMode=SingleRowSelection");

		int[] widthsP = null;
		JScrollWorksheet jswP = null;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<String> stations = StateMod_Util.createIdentifierListFromTS(combineData((java.util.List<RTi.TS.MonthTS>)__dataset.getComponentForComponentType(StateMod_DataSet.COMP_PRECIPITATION_TS_MONTHLY).getData(), (java.util.List<RTi.TS.MonthTS>)__dataset.getComponentForComponentType(StateMod_DataSet.COMP_EVAPORATION_TS_MONTHLY).getData()), true, null);
		IList<string> stations = StateMod_Util.createIdentifierListFromTS(combineData((IList<MonthTS>)__dataset.getComponentForComponentType(StateMod_DataSet.COMP_PRECIPITATION_TS_MONTHLY).getData(), (IList<MonthTS>)__dataset.getComponentForComponentType(StateMod_DataSet.COMP_EVAPORATION_TS_MONTHLY).getData()), true, null);

		try
		{
			IList<StateMod_ReservoirClimate> temp = getPrecipitationStations(__currentRes.getClimates());
			IList<StateMod_ReservoirClimate> clones = new List<StateMod_ReservoirClimate>();
			StateMod_ReservoirClimate r = null;
			int size = temp.Count;
			for (int i = 0; i < size; i++)
			{
				r = temp[i];
				clones.Add((StateMod_ReservoirClimate)r.clone());
			}

			__tableModelP = new StateMod_ReservoirClimate_TableModel(clones, __editable, true);
			StateMod_ReservoirClimate_CellRenderer crr = new StateMod_ReservoirClimate_CellRenderer(__tableModelP);

			jswP = new JScrollWorksheet(crr, __tableModelP, p);
			__worksheetP = jswP.getJWorksheet();

			__worksheetP.setColumnJComboBoxValues(StateMod_ReservoirClimate_TableModel.COL_STATION, stations, true);

			widthsP = crr.getColumnWidths();
		}
		catch (Exception e)
		{
			Message.printWarning(1, routine, "Error building worksheet.", this);
			Message.printWarning(2, routine, e);
			jswP = new JScrollWorksheet(0, 0, p);
			__worksheetP = jswP.getJWorksheet();
		}
		__worksheetP.setPreferredScrollableViewportSize(null);

		__worksheetP.setHourglassJFrame(this);
		__worksheetP.addMouseListener(this);
		__worksheetP.addKeyListener(this);

		int[] widthsE = null;
		JScrollWorksheet jswE = null;
		try
		{
			IList<StateMod_ReservoirClimate> temp = getEvaporationStations(__currentRes.getClimates());
			IList<StateMod_ReservoirClimate> clones = new List<StateMod_ReservoirClimate>();
			StateMod_ReservoirClimate r = null;
			int size = temp.Count;
			for (int i = 0; i < size; i++)
			{
				r = temp[i];
				clones.Add((StateMod_ReservoirClimate)r.clone());
			}

			__tableModelE = new StateMod_ReservoirClimate_TableModel(clones, __editable, true);
			StateMod_ReservoirClimate_CellRenderer crr = new StateMod_ReservoirClimate_CellRenderer(__tableModelE);

			jswE = new JScrollWorksheet(crr, __tableModelE, p);
			__worksheetE = jswE.getJWorksheet();

			__worksheetE.setColumnJComboBoxValues(StateMod_ReservoirClimate_TableModel.COL_STATION, stations, true);
			widthsE = crr.getColumnWidths();
		}
		catch (Exception e)
		{
			Message.printWarning(1, routine, "Error building worksheet.", this);
			Message.printWarning(2, routine, e);
			jswE = new JScrollWorksheet(0, 0, p);
			__worksheetE = jswE.getJWorksheet();
		}
		__worksheetE.setPreferredScrollableViewportSize(null);

		__worksheetE.setHourglassJFrame(this);
		__worksheetE.addMouseListener(this);
		__worksheetE.addKeyListener(this);

		JPanel worksheets = new JPanel();
		worksheets.setLayout(gb);

		JPanel panelP = new JPanel();
		panelP.setLayout(gb);
		panelP.setBorder(BorderFactory.createTitledBorder("Precipitation Stations"));
		JGUIUtil.addComponent(panelP, jswP, 0, 0, 1, 1, 1, 1, GridBagConstraints.BOTH, GridBagConstraints.NORTHWEST);

		JPanel panelE = new JPanel();
		panelE.setLayout(gb);
		panelE.setBorder(BorderFactory.createTitledBorder("Evaporation Stations"));
		JGUIUtil.addComponent(panelE, jswE, 0, 0, 1, 1, 1, 1, GridBagConstraints.BOTH, GridBagConstraints.NORTHWEST);

		JGUIUtil.addComponent(worksheets, panelP, 0, 0, 1, 1, 1, 1, GridBagConstraints.BOTH, GridBagConstraints.NORTHWEST);
		JGUIUtil.addComponent(worksheets, panelE, 0, 1, 1, 1, 1, 1, GridBagConstraints.BOTH, GridBagConstraints.NORTHWEST);

		main_panel.add(worksheets, "Center");
		main_panel.add(p1, "South");

		// assemble parts
		JGUIUtil.addComponent(bigPanel, info_panel, 0, 0, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.NORTHWEST);

		JGUIUtil.addComponent(bigPanel, main_panel, 0, 1, 10, 10, 1, 1, GridBagConstraints.BOTH, GridBagConstraints.SOUTH);
		__addEvap.addActionListener(this);
		__addPrecip.addActionListener(this);
		__deleteStation.addActionListener(this);
		__helpJButton.addActionListener(this);
		__closeJButton.addActionListener(this);
		applyJButton.addActionListener(this);
		cancelJButton.addActionListener(this);

		getContentPane().add(bigPanel);

		JPanel bottomJPanel = new JPanel();
		bottomJPanel.setLayout(gb);
		__messageJTextField = new JTextField();
		__messageJTextField.setEditable(false);
		JGUIUtil.addComponent(bottomJPanel, __messageJTextField, 0, 0, 7, 1, 1.0, 0.0, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);
		__statusJTextField = new JTextField(5);
		__statusJTextField.setEditable(false);
		JGUIUtil.addComponent(bottomJPanel, __statusJTextField, 7, 0, 1, 1, 0.0, 0.0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		getContentPane().add("South", bottomJPanel);

		pack();
		setSize(650, 400);
		JGUIUtil.center(this);
		setVisible(true);

		if (widthsP != null)
		{
			__worksheetP.setColumnWidths(widthsP);
		}
		if (widthsE != null)
		{
			__worksheetE.setColumnWidths(widthsE);
		}
	}

	/// <summary>
	/// Responds to window activated events; does nothing. </summary>
	/// <param name="e"> the WindowEvent that happened. </param>
	public virtual void windowActivated(WindowEvent e)
	{
	}

	/// <summary>
	/// Responds to window closed events; does nothing. </summary>
	/// <param name="e"> the WindowEvent that happened. </param>
	public virtual void windowClosed(WindowEvent e)
	{
	}

	/// <summary>
	/// Responds to window closing events; closes up the window properly. </summary>
	/// <param name="e"> the WindowEvent that happened. </param>
	public virtual void windowClosing(WindowEvent e)
	{
		if (saveData())
		{
			setDefaultCloseOperation(WindowConstants.DISPOSE_ON_CLOSE);
			setVisible(false);
			dispose();
		}
		else
		{
			setDefaultCloseOperation(WindowConstants.DO_NOTHING_ON_CLOSE);
		}
	}

	/// <summary>
	/// Responds to window deactivated events; does nothing. </summary>
	/// <param name="e"> the WindowEvent that happened. </param>
	public virtual void windowDeactivated(WindowEvent e)
	{
	}

	/// <summary>
	/// Responds to window deiconified events; does nothing. </summary>
	/// <param name="e"> the WindowEvent that happened. </param>
	public virtual void windowDeiconified(WindowEvent e)
	{
	}

	/// <summary>
	/// Responds to window iconified events; does nothing. </summary>
	/// <param name="e"> the WindowEvent that happened. </param>
	public virtual void windowIconified(WindowEvent e)
	{
	}

	/// <summary>
	/// Responds to window opened events; does nothing. </summary>
	/// <param name="e"> the WindowEvent that happened. </param>
	public virtual void windowOpened(WindowEvent e)
	{
	}

	/// <summary>
	/// Responds to window opening events; does nothing. </summary>
	/// <param name="e"> the WindowEvent that happened. </param>
	public virtual void windowOpening(WindowEvent e)
	{
	}

	/// <summary>
	/// Combines the values in two lists into a single lists. </summary>
	/// <param name="data1"> the first non-null list from which to add values. </param>
	/// <param name="data2"> the second non-null list from which to add values. </param>
	/// <returns> a list containing all the values of data1 and data2 </returns>
	public static IList<TS> combineData<T1, T2>(IList<T1> data1, IList<T2> data2) where T1 : RTi.TS.TS where T2 : RTi.TS.TS
	{
		IList<TS> v = new List<TS>();
		for (int i = 0; i < data1.Count; i++)
		{
			v.Add(data1[i]);
		}
		for (int i = 0; i < data2.Count; i++)
		{
			v.Add(data2[i]);
		}
		return v;
	}

	}

}