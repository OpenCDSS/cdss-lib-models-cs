using System;
using System.Collections.Generic;

// StateMod_Reservoir_Return_JFrame - GUI for displaying/editing the return flow assignments for a reservoir.

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
	using JScrollWorksheet = RTi.Util.GUI.JScrollWorksheet;
	using JWorksheet = RTi.Util.GUI.JWorksheet;
	using ResponseJDialog = RTi.Util.GUI.ResponseJDialog;
	using PropList = RTi.Util.IO.PropList;
	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// GUI for displaying/editing the return flow assignments for a reservoir.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class StateMod_Reservoir_Return_JFrame extends javax.swing.JFrame implements java.awt.event.ActionListener, java.awt.event.KeyListener, java.awt.event.MouseListener, java.awt.event.WindowListener
	public class StateMod_Reservoir_Return_JFrame : JFrame, ActionListener, KeyListener, MouseListener, WindowListener
	{

	/// <summary>
	/// Labels for the buttons.
	/// </summary>
	private const string __BUTTON_ADD_RETURN = "Add Return", __BUTTON_APPLY = "Apply", __BUTTON_CANCEL = "Cancel", __BUTTON_DEL_RETURN = "Delete Return", __BUTTON_HELP = "Help", __BUTTON_CLOSE = "Close";

	/// <summary>
	/// Whether the gui data is editable or not.
	/// </summary>
	private bool __editable = false;

	/// <summary>
	/// GUI buttons.
	/// </summary>
	private JButton __addReturn_JButton, __close_JButton, __deleteReturn_JButton, __help_JButton;

	/// <summary>
	/// Status bar textfields 
	/// </summary>
	private JTextField __messageJTextField, __statusJTextField;

	/// <summary>
	/// Worksheet in which the rights are shown.
	/// </summary>
	private JWorksheet __worksheet;

	/// <summary>
	/// Dataset that contains the data.
	/// </summary>
	//private StateMod_DataSet __dataset;

	/// <summary>
	/// The current reservoir for which returns are being shown.
	/// </summary>
	private StateMod_Reservoir __currentRes;

	/// <summary>
	/// The list of return data to view.  These returns are maintained as a separate data component and not
	/// currently linked to the reservoir.  This is a different approach than diversion/well returns, mainly
	/// because effort has not been put into the full editing features and "dirty" data need to be separate.
	/// </summary>
	private IList<StateMod_ReturnFlow> __currentResReturnList = new List<StateMod_ReturnFlow>();

	/// <summary>
	/// Constructor. </summary>
	/// <param name="dataset"> the dataset in which the data is contained. </param>
	/// <param name="res"> the Reservoir for which to display return information. </param>
	/// <param name="editable"> whether the gui data is editable or not. </param>
	public StateMod_Reservoir_Return_JFrame(StateMod_DataSet dataset, StateMod_Reservoir res, bool editable)
	{
		StateMod_GUIUtil.setTitle(this, dataset, res.getName() + " - Reservoir Return Flow Table Assignment", null);
		JGUIUtil.setIcon(this, JGUIUtil.getIconImage());
		__currentRes = res;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<StateMod_ReturnFlow> allReturns = (java.util.List<StateMod_ReturnFlow>)dataset.getComponentForComponentType(StateMod_DataSet.COMP_RESERVOIR_RETURN).getData();
		IList<StateMod_ReturnFlow> allReturns = (IList<StateMod_ReturnFlow>)dataset.getComponentForComponentType(StateMod_DataSet.COMP_RESERVOIR_RETURN).getData();
		__currentResReturnList = (IList<StateMod_ReturnFlow>)StateMod_Util.getDataList(allReturns,res.getID());
		Message.printStatus(2,"","Have " + __currentResReturnList.Count + " return records for reservoir \"" + __currentRes.getID() + "\" uniquetempvar.");
		//__dataset = dataset;
		// TODO SAM 2011-01-02 For now editing is disabled...
		editable = false;
		__editable = editable;
		setupGUI();
	}

	/// <summary>
	/// Responds to action performed events. </summary>
	/// <param name="e"> the ActionEvent that happened. </param>
	public virtual void actionPerformed(ActionEvent e)
	{
		string routine = "StateMod_Reservoir_Return_JFrame::actionPerformed";

		string action = e.getActionCommand();

		if (action.Equals(__BUTTON_ADD_RETURN))
		{
			StateMod_ReturnFlow aReturn = new StateMod_ReturnFlow(StateMod_DataSet.COMP_RESERVOIR_RETURN);
			aReturn._isClone = true;
			StateMod_ReturnFlow last = (StateMod_ReturnFlow)__worksheet.getLastRowData();

			if (last == null)
			{
				aReturn.setID(StateMod_Util.createNewID(__currentRes.getID()));
				aReturn.setCgoto(__currentRes.getID());
			}
			else
			{
				aReturn.setID(StateMod_Util.createNewID(last.getID()));
				aReturn.setCgoto(last.getCgoto());
			}
			__worksheet.scrollToLastRow();
			__worksheet.addRow(aReturn);
			__worksheet.selectLastRow();
			__deleteReturn_JButton.setEnabled(true);
		}
		else if (action.Equals(__BUTTON_DEL_RETURN))
		{
			int row = __worksheet.getSelectedRow();
			if (row != -1)
			{
				int x = (new ResponseJDialog(this, "Delete return", "Delete reservoir return?", ResponseJDialog.YES | ResponseJDialog.NO)).response();
				if (x == ResponseJDialog.NO)
				{
					return;
				}

				__worksheet.cancelEditing();
				__worksheet.deleteRow(row);
				__deleteReturn_JButton.setEnabled(false);
			}
			else
			{
				Message.printWarning(1, routine, "Must select desired return to delete.");
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
		else if (e.getSource() == __help_JButton)
		{
			// TODO HELP (JTS - 2003-06-09)
		}
	}

	/// <summary>
	/// Checks the data to make sure that all the data are valid. </summary>
	/// <returns> 0 if the data are valid, 1 if errors exist and -1 if non-fatal errors exist. </returns>
	private int checkInput()
	{
		string routine = "StateMod_Reservoir_Return_JFrame.checkInput";
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<StateMod_ReturnFlow> v = (java.util.List<StateMod_ReturnFlow>)__worksheet.getAllData();
		IList<StateMod_ReturnFlow> v = (IList<StateMod_ReturnFlow>)__worksheet.getAllData();

		int size = v.Count;
		StateMod_ReturnFlow aReturn = null;
		string warning = "";
		string id;
		string riverNodeID;
		//double percent;
		//String tableID;
		int fatalCount = 0;
		//String comment;
		for (int i = 0; i < size; i++)
		{
			aReturn = v[i];

			id = aReturn.getID();
			riverNodeID = aReturn.getCrtnid();
			//percent = aReturn.getPcttot();
			//tableID = "" + aReturn.getIrtndl();
			//comment = aReturn.getComment();

			// TODO SAM 2011-01-02 Need to implement validators
			if (id.Length > 12)
			{
				warning += "\nReservoir ID (" + id + ") is longer than 12 characters.";
				fatalCount++;
			}

			if (id.IndexOf(" ", StringComparison.Ordinal) > -1 || id.IndexOf("-", StringComparison.Ordinal) > -1)
			{
				warning += "\nReservoir ID (" + id + ") cannot contain spaces or dashes.";
				fatalCount++;
			}

			if (riverNodeID.Length > 12)
			{
				warning += "River node ID (" + riverNodeID + ") is longer than 12 characters.";
			}
		}

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
			{
				// Nonfatal errors...
				Message.printStatus(1, routine, "Returning -1 from checkInput()");
				return -1;
			}
		}
		else
		{
			// No errors...
			Message.printStatus(2, routine, "Returning 0 from checkInput()");
			return 0;
		}
	}

	/// <summary>
	/// Saves the input back into the dataset. </summary>
	/// <returns> true if the data was saved successfully.  False if not. </returns>
	private bool saveData()
	{
		//String routine = "StateMod_Reservoir_Return_JFrame.saveData";
		/* TODO SAM 2011-01-02 Enable - for now no editing is allowed
		if (!__worksheet.stopEditing()) {
			// don't save if there are errors.
			Message.printWarning(1, routine, "There are errors in the data "
				+ "that must be corrected before data can be saved.",
				this);
			return false;
		}
		
		if (checkInput() > 0) {
			return false;
		}
	
		// now only save data if any are different.
		boolean needToSave = false;
	
		// if the lists are differently-sized, they're different
		List wv = __worksheet.getAllData(); // w for worksheet
		List rv = __currentRes.getRights();	// i for instream flow
	
		needToSave = !(StateMod_ReservoirRight.equals(wv, rv));
	
		Message.printStatus(1, routine, "Saving? .........[" + needToSave +"]");
	
		if (!needToSave) {
			// there's nothing different -- users may even have deleted
			// some rights and added back in identical values
			return true;
		}
	
		// At this point, remove the old diversion rights from the original component list
		List reservoirRights =(List)(__dataset.getComponentForComponentType(
			StateMod_DataSet.COMP_RESERVOIR_RIGHTS)).getData();
		int size = rv.size();
		StateMod_ReservoirRight ir;
		for (int i = 0; i < size; i++) {
			ir = (StateMod_ReservoirRight)rv.get(i);
			StateMod_Util.removeFromVector(reservoirRights, ir);
		}
	
		// Now add the elements from the new list to the reservoirReturns list.
		size = wv.size();
		StateMod_ReservoirRight cdr = null;
	
		for (int i = 0; i < size; i++) {
			ir = (StateMod_ReservoirRight)wv.get(i);
			cdr = (StateMod_ReservoirRight)ir.clone();
			cdr._isClone = false;
			reservoirRights.add(cdr);
		}
	
		// sort the reservoirRights Vector
		// REVISIT (JTS - 2003-10-10)
		// here we are sorting the full data array -- may be a performance
		// issue
		List sorted=StateMod_Util.sortStateMod_DataVector(reservoirRights);
		__dataset.getComponentForComponentType(StateMod_DataSet.COMP_RESERVOIR_RIGHTS)
			.setData(sorted);
		__currentRes.disconnectRights();
		__currentRes.connectRights(sorted);
		__dataset.setDirty(StateMod_DataSet.COMP_RESERVOIR_RIGHTS, true);
		*/
		return true;
	}

	/// <summary>
	/// Checks to see if the __deleteReturn button should be enabled or not.
	/// </summary>
	private void checkDeleteReturnButton()
	{
		int row = __worksheet.getSelectedRow();
		if (__editable)
		{
			if (row == -1)
			{
				__deleteReturn_JButton.setEnabled(false);
			}
			else
			{
				__deleteReturn_JButton.setEnabled(true);
			}
		}
		else
		{
			__deleteReturn_JButton.setEnabled(false);
		}
	}

	/// <summary>
	/// Clean up before garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~StateMod_Reservoir_Return_JFrame()
	{
		__currentRes = null;
		__worksheet = null;
		__addReturn_JButton = null;
		__deleteReturn_JButton = null;
		__help_JButton = null;
		__close_JButton = null;
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
	/// Responds to key released events; checks if the __deleteRight button should be enabled. </summary>
	/// <param name="e"> the KeyEvent that happened. </param>
	public virtual void keyReleased(KeyEvent e)
	{
		checkDeleteReturnButton();
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
	/// Responds to mouse released events; checks to see if the __deleteRight button should be enabled or not. </summary>
	/// <param name="e"> the MouseEvent that happened. </param>
	public virtual void mouseReleased(MouseEvent e)
	{
		checkDeleteReturnButton();
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

		PropList p = new PropList("StateMod_Reservoir_Return_JFrame.JWorksheet");
		p.add("JWorksheet.AllowCopy=true");
		p.add("JWorksheet.CellFont=Courier");
		p.add("JWorksheet.CellStyle=Plain");
		p.add("JWorksheet.CellSize=11");
		p.add("JWorksheet.HeaderFont=Arial");
		p.add("JWorksheet.HeaderStyle=Plain");
		p.add("JWorksheet.HeaderSize=11");
		p.add("JWorksheet.HeaderBackground=LightGray");
		p.add("JWorksheet.RowColumnPresent=false");
		p.add("JWorksheet.ShowPopupMenu=true");
		p.add("JWorksheet.SelectionMode=SingleRowSelection");

		int[] widths = null;
		JScrollWorksheet jsw = null;
		try
		{
			/* TODO SAM 2011-01-02 Comment out - might allow something similar if editing is enabled
			 * and choices of IDs are provided
			List accounts = __currentRes.getAccounts();
			List v3 = new Vector();
			int size = accounts.size();
			StateMod_ReservoirAccount ra = null;
			for (int i = 0; i < size; i++) {
				ra = (StateMod_ReservoirAccount)accounts.get(i);
				v3.add("" + ra.getID() + " - " + ra.getName());
			}
			for (int i = 1; i < size; i++) {
				v3.add("-" + (i + 1) + " - Fill first " + (i + 1) 
					+ " accounts");
			}
	
			List v = new Vector();
			List v2 = __currentRes.getRights();
			StateMod_ReservoirRight rr;
			for (int i = 0; i < v2.size(); i++) {
				rr = (StateMod_ReservoirRight)
					((StateMod_ReservoirRight)v2.get(i))
					.clone();
				v.add(rr);
			}
			*/
			// Get the list of all returns and filter for this reservoir
			// TODO SAM 2011-01-02 The code needs to use a table model with lists if editing is enabled
			StateMod_Reservoir_Return_Data_TableModel tmr = new StateMod_Reservoir_Return_Data_TableModel(__currentResReturnList, __editable);
			StateMod_Reservoir_Return_Data_CellRenderer crr = new StateMod_Reservoir_Return_Data_CellRenderer(tmr);

			jsw = new JScrollWorksheet(crr, tmr, p);
			__worksheet = jsw.getJWorksheet();

			/*
			List onOff = StateMod_ReservoirRight.getIrsrswChoices(true);
			__worksheet.setColumnJComboBoxValues(
				StateMod_ReservoirRight_TableModel.COL_ON_OFF, onOff,
				false);
			__worksheet.setColumnJComboBoxValues(
				StateMod_ReservoirRight_TableModel.COL_ACCOUNT_DIST,
				v3, false);		
			List rightTypes =
				StateMod_ReservoirRight.getItyrsrChoices(true);
			__worksheet.setColumnJComboBoxValues(
				StateMod_ReservoirRight_TableModel.COL_RIGHT_TYPE,
				rightTypes, false);
			List fillTypes=StateMod_ReservoirRight.getN2fillChoices(true);
			__worksheet.setColumnJComboBoxValues(
				StateMod_ReservoirRight_TableModel.COL_FILL_TYPE,
				fillTypes, false);
				*/

			widths = crr.getColumnWidths();
		}
		catch (Exception e)
		{
			Message.printWarning(2, routine, e);
			jsw = new JScrollWorksheet(0, 0, p);
			__worksheet = jsw.getJWorksheet();
		}
		__worksheet.setPreferredScrollableViewportSize(null);

		__worksheet.setHourglassJFrame(this);
		__worksheet.addMouseListener(this);
		__worksheet.addKeyListener(this);

		__addReturn_JButton = new JButton(__BUTTON_ADD_RETURN);
		__deleteReturn_JButton = new JButton(__BUTTON_DEL_RETURN);
		__deleteReturn_JButton.setEnabled(false);
		__help_JButton = new JButton(__BUTTON_HELP);
		__help_JButton.setEnabled(false);
		__close_JButton = new JButton(__BUTTON_CLOSE);
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
			p1.add(__addReturn_JButton);
			p1.add(__deleteReturn_JButton);
		}
		p1.add(applyJButton);
		p1.add(cancelJButton);
	//	p1.add(__helpJButton);
		p1.add(__close_JButton);
		if (!__editable)
		{
			applyJButton.setEnabled(false);
			applyJButton.setToolTipText("Editing reservoir return data is not implemented.");
			__close_JButton.setEnabled(false);
			__close_JButton.setToolTipText("Editing reservoir return data is not implemented.");
		}

		main_panel.add(jsw, "Center");
		main_panel.add(p1, "South");

		JGUIUtil.addComponent(bigPanel, info_panel, 0, 0, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.NORTHWEST);
		JGUIUtil.addComponent(bigPanel, main_panel, 0, 1, 10, 10, 1, 1, GridBagConstraints.BOTH, GridBagConstraints.SOUTH);
		__addReturn_JButton.addActionListener(this);
		__deleteReturn_JButton.addActionListener(this);
		__help_JButton.addActionListener(this);
		__close_JButton.addActionListener(this);
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
		setSize(760, 400);
		JGUIUtil.center(this);
		setVisible(true);
		if (widths != null)
		{
			__worksheet.setColumnWidths(widths);
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

	}

}