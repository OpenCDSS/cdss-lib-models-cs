using System;
using System.Collections.Generic;
using System.Threading;

// StateMod_RunSmDelta_JFrame - dialog to create templates for graphing

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
// StateMod_RunSmDelta_JFrame - dialog to create templates for graphing
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
// 
// 24 Dec 1997	Catherine E.		Created initial version of class
//		Nutting-Lane, RTi
// 03 Jul 1998	CEN, RTi		Modify to work for graphing
//					or for the output control edit.
// 28 Sep 1998	SAM, RTi		Added ability to specify the
//					period when getting time series
//					so that graph performs better.
// 29 Sep 1998	CEN, RTi		Adding radio buttons to toggle
//					user entered id vs. generated lists.
// 20 Nov 1998	CEN, RTi		Adding big picture stuff.
// 21 Dec 1998	CEN, RTi		Added try/catch to IO routines
// 12 May 1999	CEN, RTi		Changed .att to .txt for big
//					picture output.
// 25 Oct 1999	CEN, RTi		Added _template_location
// 06 Nov 2000	CEN, RTi		Added auto-line copy checkbox preference
// 01 Apr 2001	Steven A. Malers, RTi	Change GUI to JGUIUtil.  Add finalize().
//					Remove import *.
// 04 May 2001	SAM, RTi		Verify TSView usage.  Enable all time
//					series defined in
//					TSGraph.GRAPH_TYPE_NAMES.  Remove limit
//					that only monthly time series can be
//					displayed.
// 19 Jul 2001	SAM, RTi		Change so the output file(s)are removed
//					before each run to make sure that new
//					data are used.
// 13 Aug 2001	SAM, RTi		Add wells(xwg).
// 17 Aug 2001	SAM, RTi		Add argument to RunSMOption to wait
//					after the run so that the output file is
//					created.  This may only be necessary on
//					fast machines.
// 23 Sep 2001	SAM, RTi		Change Table to DataTable.  Lengthen the
//					total time in waitForFile()calls to
//					60 seconds total.
// 2001-12-11	SAM, RTi		Update to use NoSwing GeoView classes so
//					parallel Swing development can occur.
// 2002-03-07	SAM, RTi		Use TSProduct to get the graph types.
// 2002-06-20	SAM, RTi		Update to pass well information to big
//					picture grid.
// 2002-07-26	SAM, RTi		Update to support the new GeoView for
//					the big picture plot.
// 2002-08-02	SAM, RTi		Make additional enhancements to
//					streamline running delplt.
//					Add __templateLocation to try to avoid
//					possible side-effects from multiple
//					windows being open at the same time.
// 2002-08-07	SAM, RTi		Figure out why the graph was not working
//					correctly - need to remove old files
//					each time.  Remove StringTokenizer - for
//					now do not handle reservoir accounts.
// 2002-08-26	SAM, RTi		Change constructor to take an integer
//					for the SMMainGUI interface type.
//					Change big picture constructor to take
//					a reference to SMMainGUI to pass
//					information.
// 2002-09-12	SAM, RTi		For graphing, add Baseflow as a data
//					type if the node is a baseflow node with
//					baseflow time series.
//					Also add a button to retrieve the time
//					series before graphing.  This allows
//					different graphs to be shown without
//					rereading.
// 2002-09-19	SAM, RTi		Use isDirty()instead of setDirty()to
//					indicate edits.  Add a JTextField at the
//					bottom of the window to indicate the
//					status of the GUI.
// 2002-10-11	SAM, RTi		Change ProcessManager* to
//					ProcessManager1* to be allow transition
//					to Java 1.4.x.
// 2002-10-16	SAM, RTi		Move back to ProcessManger since the
//					updated version seems to work well with
//					Java 1.18 and 1.4.0.  Use version that
//					takes command arguments rather than a
//					single string.
// 2003-06-26	SAM, RTi		* Change the call to editFile()to use
//					  local code and start the editor as a
//					  thread.
//					* Change graphTS()to extend the period
//					  by a month on each end if bar graphs
//					  are used to account for the overlap
//					  of the bars on the end.
//------------------------------------------------------------------------------
// 2003-08-21	J. Thomas Sapienza, RTi	Initial swing version.
// 2003-08-25	JTS, RTi		Continued work on initial swing version.
// 2003-09-11	SAM, RTi		Update due to some name changes in the
//					river station components.
// 2003-09-23	JTS, RTi		Uses new StateMod_GUIUtil code for
//					setting titles.
// 2004-01-22	JTS, RTi		Updated to use JScrollWorksheet and
//					the new row headers.
// 2006-03-06	JTS, RTi		Removed the help key from the process
//					manager dialog so that the help option
//					no longer displays.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
//------------------------------------------------------------------------------

namespace DWR.StateMod
{


	using JGUIUtil = RTi.Util.GUI.JGUIUtil;
	using JScrollWorksheet = RTi.Util.GUI.JScrollWorksheet;
	using JWorksheet = RTi.Util.GUI.JWorksheet;
	using ResponseJDialog = RTi.Util.GUI.ResponseJDialog;
	using SimpleFileFilter = RTi.Util.GUI.SimpleFileFilter;
	using SimpleJButton = RTi.Util.GUI.SimpleJButton;
	using SimpleJComboBox = RTi.Util.GUI.SimpleJComboBox;
	using HelpJDialog = RTi.Util.Help.HelpJDialog;
	using DataSetComponent = RTi.Util.IO.DataSetComponent;
	using IOUtil = RTi.Util.IO.IOUtil;
	using ProcessManager = RTi.Util.IO.ProcessManager;
	using ProcessManagerJDialog = RTi.Util.IO.ProcessManagerJDialog;
	using PropList = RTi.Util.IO.PropList;
	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// This GUI is a class for controlling the run of delta plots.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class StateMod_RunSmDelta_JFrame extends javax.swing.JFrame implements java.awt.event.ActionListener, java.awt.event.ItemListener, java.awt.event.KeyListener, java.awt.event.MouseListener, java.awt.event.WindowListener
	public class StateMod_RunSmDelta_JFrame : JFrame, ActionListener, ItemListener, KeyListener, MouseListener, WindowListener
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			__DELPLT_RUN_MODE_MERGE = "Merge - merge output from " + __SmDelta + " runs";
			__BUTTON_LOAD = "Load " + __SmDelta + " Input";
			__BUTTON_RUN = "Run " + __SmDelta;
			__BUTTON_SAVE = "Save " + __SmDelta + " Input";
			__defaultFilenameFilter = __SmDelta;
		}


	/// <summary>
	/// Reference the window manager's number for this window.
	/// </summary>
	public const int WINDOW_NUM = StateMod_DataSet_WindowManager.WINDOW_RUN_DELPLT;

	/// <summary>
	/// Filename option for the file browser.
	/// </summary>
	public const string OPTION_BROWSE = "Browse ...";

	/// <summary>
	/// Name of the program as known to users.
	/// </summary>
	private readonly string __SmDelta = "SmDelta";

	/// <summary>
	/// Run mode types.
	/// </summary>
	private string __DELPLT_RUN_MODE_SINGLE = "Single - One parameter (first given), 1+ stations", __DELPLT_RUN_MODE_MULTIPLE = "Multiple - 1+ parameter(s), 1+ station(s)", __DELPLT_RUN_MODE_DIFFERENCE = "Diff - 1 parameter difference between runs (assign zero if not found in both runs)", __DELPLT_RUN_MODE_DIFFX = "Diffx - 1 parameter difference between runs (ignore if not found in both runs)", __DELPLT_RUN_MODE_MERGE;

	/// <summary>
	/// Button labels.
	/// </summary>
	private string __BUTTON_ADD_ROW = "Add a Row (Append)", __BUTTON_DELETE_ROWS = "Delete Selected Row(s)", __BUTTON_REMOVE_ALL_ROWS = "Remove All Rows", __BUTTON_LOAD, __BUTTON_RUN, __BUTTON_CLOSE = "Close", __BUTTON_HELP = "Help", __BUTTON_SAVE;

	/// <summary>
	/// Whether the table is dirty or not.
	/// </summary>
	private bool __dirty = false;

	/// <summary>
	/// Specific dataset components that are kept handy.
	/// </summary>
	private DataSetComponent __reservoirComp = null, __diversionComp = null, __instreamFlowComp = null, __wellComp = null, __streamGageComp = null;

	/// <summary>
	/// GUI checkbox for whether to copy the above line when adding a new one.
	/// </summary>
	private JCheckBox __autoLineCopyJCheckBox;

	/// <summary>
	/// GUI status fields.
	/// </summary>
	private JTextField __messageTextField, __statusTextField;

	/// <summary>
	/// The worksheet in which data is displayed.
	/// </summary>
	private JWorksheet __worksheet;

	/// <summary>
	/// GUI buttons.
	/// </summary>
	private SimpleJButton __addRowButton, __deleteRowButton, __clearWorksheetButton, __selectTemplateButton, __saveTemplateButton, __helpButton, __closeButton, __runSmDeltaButton;

	/// <summary>
	/// GUI combo box for choosing the run mode.
	/// </summary>
	private SimpleJComboBox __smdeltaRunModeSimpleJComboBox;

	/// <summary>
	/// The dataset containing the StateMod data.
	/// </summary>
	private StateMod_DataSet __dataset;

	/// <summary>
	/// The worksheet table model.
	/// </summary>
	private StateMod_RunSmDelta_TableModel __tableModel;

	/// <summary>
	/// The default filename filter.
	/// </summary>
	private string __defaultFilenameFilter;

	/// <summary>
	/// The SmDelta response file (no leading path).
	/// </summary>
	private string __smdeltaFilename;

	/// <summary>
	/// The SmDelta response file folder.
	/// </summary>
	private string __smdeltaFolder;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="dataset"> the dataset for which to construct a delta plot run file. </param>
	public StateMod_RunSmDelta_JFrame(StateMod_DataSet dataset)
	{
			if (!InstanceFieldsInitialized)
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		StateMod_GUIUtil.setTitle(this, dataset, "Run " + __SmDelta, null);
		JGUIUtil.setIcon(this, JGUIUtil.getIconImage());
		// Get the StateMod data set basename
		string basename = dataset.getBaseName();

		__dataset = dataset;

		__reservoirComp = __dataset.getComponentForComponentType(StateMod_DataSet.COMP_RESERVOIR_STATIONS);
		__diversionComp = __dataset.getComponentForComponentType(StateMod_DataSet.COMP_DIVERSION_STATIONS);
		__instreamFlowComp = __dataset.getComponentForComponentType(StateMod_DataSet.COMP_INSTREAM_STATIONS);
		__wellComp = __dataset.getComponentForComponentType(StateMod_DataSet.COMP_WELL_STATIONS);
		__streamGageComp = __dataset.getComponentForComponentType(StateMod_DataSet.COMP_STREAMGAGE_STATIONS);

		if (!basename.Equals(""))
		{
			setTitle(getTitle() + " - " + basename);
		}

		setupGUI();
	}

	/// <summary>
	/// Responds to action events. </summary>
	/// <param name="event"> the ActionEvent that happened. </param>
	public virtual void actionPerformed(ActionEvent @event)
	{
		string routine = "StateMod_RunDeltaPlot_JFrame.actionPerformed";
		string action = @event.getActionCommand();

		if (action.Equals(__BUTTON_ADD_ROW))
		{
			if (!__tableModel.canAddRow())
			{
				setMessages("At least the first row must have all values filled out.", "Error");
				return;
			}
			int rows = __worksheet.getRowCount();

			if (rows == 0)
			{
				__worksheet.addRow(new StateMod_GraphNode());
				__tableModel.setCellAttributes(0, 0, true);
				__tableModel.setCellAttributes(0, 1, true);
				__tableModel.setCellAttributes(0, 2, true);
				__tableModel.setCellAttributes(0, 3, true);
				__tableModel.setCellAttributes(0, 4, true);
				__dirty = true;
				checkButtonStates();
				setMessages("Add parameters to analyze", "Ready");
				return;
			}

			StateMod_GraphNode gnp = (StateMod_GraphNode)__worksheet.getRowData(rows - 1);
			string id = gnp.getID().Trim();

			if (autoLineCopy() && id.Equals("0 (All)"))
			{
			// duplicate the values from the above row and copy them into the new row
				__worksheet.addRow(new StateMod_GraphNode());
				__worksheet.setValueAt(gnp.getFileName(), rows, 0);
				__worksheet.setValueAt(gnp.getType(), rows, 1);
				__tableModel.setParmIDComboBoxes(rows, gnp.getType());
				__worksheet.setValueAt(gnp.getDtype(), rows, 2);
				__worksheet.setValueAt(gnp.getYrAve(), rows, 3);
				__worksheet.setValueAt(gnp.getID(), rows, 4);
			}
			else
			{
			// normal row add
			// mark everything as a duplicate of the row above it, 
			// except the ID, which should have a red border
				__worksheet.addRow(new StateMod_GraphNode());
				__tableModel.copyDownComboBoxes();
				__tableModel.setLastRowAsBlank();
				__tableModel.setCellAttributes(rows, 4, true);
			}
			__dirty = true;
			checkButtonStates();
			setMessages("Add parameters to analyze", "Ready");
		}
		else if (action.Equals(__BUTTON_REMOVE_ALL_ROWS))
		{
			__worksheet.clear();
			__dirty = false;
			checkButtonStates();
		}
		else if (action.Equals(__BUTTON_CLOSE))
		{
			closeWindow();
		}
		else if (action.Equals(__BUTTON_DELETE_ROWS))
		{
			int[] rows = __worksheet.getSelectedRows();
			for (int i = rows.Length - 1; i >= 0; i--)
			{
				__tableModel.preDeleteRow(rows[i]);
	//			__worksheet.selectRow(rows[i] - 1);
	//			__worksheet.scrollToRow(rows[i] - 1);
			}
			checkButtonStates();
			__dirty = true;
			setMessages(__SmDelta + " input has changed.", "Ready");
		}
		else if (action.Equals(__BUTTON_HELP))
		{
			IList<string> helpVector = fillBPPrelimHelpGUIVector();
			PropList proplist = new PropList("HelpProps");
			proplist.setValue("HelpKey", "SMGUI.BigPicture");
			new HelpJDialog(this, helpVector, proplist);
		}
		else if (action.Equals(__BUTTON_RUN))
		{
			runSmDelta();
		}
		else if (action.Equals(__BUTTON_SAVE))
		{
			saveSmDeltaFile();
		}
		else if (action.Equals(__BUTTON_LOAD))
		{
			JGUIUtil.setWaitCursor(this, true);
			string directory = JGUIUtil.getLastFileDialogDirectory();

			JFileChooser fc = null;
			if (!string.ReferenceEquals(directory, null))
			{
				fc = new JFileChooser(directory);
			}
			else
			{
				fc = new JFileChooser();
			}

			fc.setDialogTitle("Select " + __SmDelta + " Input File");
			SimpleFileFilter tpl = new SimpleFileFilter(__defaultFilenameFilter, __SmDelta + " Input Files");
			fc.addChoosableFileFilter(tpl);
			fc.setAcceptAllFileFilterUsed(true);
			fc.setFileFilter(tpl);
			fc.setDialogType(JFileChooser.OPEN_DIALOG);

			JGUIUtil.setWaitCursor(this, false);
			int retVal = fc.showOpenDialog(this);
			if (retVal != JFileChooser.APPROVE_OPTION)
			{
				return;
			}

			string currDir = (fc.getCurrentDirectory()).ToString();

			__smdeltaFolder = currDir;

			if (!currDir.Equals(directory, StringComparison.OrdinalIgnoreCase))
			{
				JGUIUtil.setLastFileDialogDirectory(currDir);
			}
			string filename = fc.getSelectedFile().getName();

			__smdeltaFilename = filename;

			JGUIUtil.setWaitCursor(this, true);

			IList<StateMod_GraphNode> theGraphNodes = new List<StateMod_GraphNode>();
			try
			{
				StateMod_GraphNode.readStateModDelPltFile(theGraphNodes, currDir + File.separator + filename);
				IList<StateMod_GraphNode> nodes = __tableModel.formLoadData(theGraphNodes);
				StateMod_GraphNode gn = null;
				__worksheet.clear();
				for (int i = 0; i < nodes.Count; i++)
				{
					gn = nodes[i];
					__worksheet.addRow(new StateMod_GraphNode());
					__worksheet.setValueAt(gn.getFileName(), i, 0);
					__worksheet.setValueAt(gn.getType(), i, 1);
					__tableModel.setParmIDComboBoxes(i, gn.getType());
					__worksheet.setValueAt(gn.getDtype(), i, 2);
					__worksheet.setValueAt(gn.getYrAve(), i, 3);
					__worksheet.setValueAt(gn.getID(), i, 4);
				}

				__dirty = false;
			}
			catch (Exception e)
			{
				Message.printWarning(1, routine, "Error loading output control file\n\"" + currDir + File.separator + filename + "\"", this);
				Message.printWarning(2, routine, e);
			}

			if (theGraphNodes.Count > 0)
			{
				StateMod_GraphNode gn = (StateMod_GraphNode)theGraphNodes[0];
				setRunType(gn.getSwitch());
			}

			JGUIUtil.setWaitCursor(this, false);

			checkButtonStates();
		}
	}

	/// <summary>
	/// Checks to see if the previous line should be automatically copied down to the 
	/// next one when a new line is added. </summary>
	/// <returns> true if the line should be copied, false if not </returns>
	public virtual bool autoLineCopy()
	{
		if (__autoLineCopyJCheckBox.isSelected())
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	/// <summary>
	/// Checks the state of the GUI and sets the button states appropriately.
	/// </summary>
	private void checkButtonStates()
	{
		int selRows = __worksheet.getSelectedRowCount();

		if (selRows == 0)
		{
			__deleteRowButton.setEnabled(false);
		}
		else
		{
			__deleteRowButton.setEnabled(true);
		}

		int rows = __worksheet.getRowCount();
		if (rows == 0)
		{
			__clearWorksheetButton.setEnabled(false);
			__runSmDeltaButton.setEnabled(false);
			__saveTemplateButton.setEnabled(false);
		}
		else
		{
			__clearWorksheetButton.setEnabled(true);
			__runSmDeltaButton.setEnabled(true);
			__saveTemplateButton.setEnabled(true);
		}
	}

	/// <summary>
	/// Closes the window, prompting to save if the user has not already.
	/// </summary>
	protected internal virtual void closeWindow()
	{
		if (__dirty)
		{
			int x = (new ResponseJDialog(this, "Save " + __SmDelta + " Input", "You have not saved the " + __SmDelta + " input file.\n\n" + "Continue without saving?", ResponseJDialog.YES | ResponseJDialog.NO)).response();
			if (x == ResponseJDialog.NO)
			{
				setDefaultCloseOperation(WindowConstants.DO_NOTHING_ON_CLOSE);
				return;
			}
		}
		setVisible(false);
		setDefaultCloseOperation(WindowConstants.DISPOSE_ON_CLOSE);
		dispose();
	}


	/// <summary>
	/// Create help information for the help dialog.
	/// </summary>
	private IList<string> fillBPPrelimHelpGUIVector()
	{
		IList<string> helpVector = new List<string>(2);
		helpVector.Add("This tool helps you create an input file for the " + __SmDelta);
		helpVector.Add("program, which allows comparisons of different scenarios, years, ");
		helpVector.Add("data types, etc.  Each section in the input file (file, data type, ");
		helpVector.Add("parameters, ID list and year) is constructed in this tool.  Each ");
		helpVector.Add("time you specify a new file name, the entire row must be filled.");
		helpVector.Add("This is a new section in the input file.  If you wish to ");
		helpVector.Add("perform the analysis on every station, set the identifier to \"0\".");
		helpVector.Add("If you wish to specify a list of identifiers, fill in every column ");
		helpVector.Add("on the first row and only the identifier on subsequent rows.");
		helpVector.Add("");

		helpVector.Add("The file name should either be one of the binary files (.b43 or .b44");
		helpVector.Add("for example)or the .xdd file for diversions or .xre for ");
		helpVector.Add("reservoirs if full reports have been created.");

		helpVector.Add("StateMod determines which stations to include in the reports based");
		helpVector.Add("on the output control contents.  Therefore, if your results don't");
		helpVector.Add("include stations you wish to compare, look at your output control");
		helpVector.Add("file.  Remember that changes to that file will not be reflected in ");
		helpVector.Add("the BigPicture plot until another StateMod simulation has been run.");
		helpVector.Add("");

		helpVector.Add("For example, to perform difference comparisons between average");
		helpVector.Add("River_Outflow and River_Outflow in 1995 of 4 different stations, ");
		helpVector.Add("the table would look like the following:");
		helpVector.Add("");
		helpVector.Add("Stream       River_Outflow   09152500      Ave     whiteH.xdd");
		helpVector.Add("                             09144250");
		helpVector.Add("                             09128000");
		helpVector.Add("                             09149500");
		helpVector.Add("Stream       River_Outflow   09152500      1995    whiteH.xdd");
		helpVector.Add("                             09144250");
		helpVector.Add("                             09128000");
		helpVector.Add("                             09149500");
		helpVector.Add("");
		helpVector.Add("The result is a single value (the difference) for each station");

		helpVector.Add("");
		helpVector.Add("See additional help through the \"More help\" button below.");
		helpVector.Add("Note that the additional help may take some time to display.");
		return helpVector;
	}

	/// <summary>
	/// Clean up before garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~StateMod_RunSmDelta_JFrame()
	{
		// REVISIT
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// This method is called by the SMbigPictureGrid getData()method.
	/// REVISIT (JTS - 2003-08-25) 
	/// this should be used instead by whatever method saves the data to a file </summary>
	/// <returns> StateMod_GraphNode.RUNTYPE_*, indicating SmDelta run mode. </returns>
	public virtual int getRunType()
	{
		string selected = __smdeltaRunModeSimpleJComboBox.getSelected();
		if (selected.Equals(__DELPLT_RUN_MODE_SINGLE))
		{
			return StateMod_GraphNode.RUNTYPE_SINGLE;
		}
		else if (selected.Equals(__DELPLT_RUN_MODE_MULTIPLE))
		{
			return StateMod_GraphNode.RUNTYPE_MULTIPLE;
		}
		else if (selected.Equals(__DELPLT_RUN_MODE_DIFFERENCE))
		{
			return StateMod_GraphNode.RUNTYPE_DIFFERENCE;
		}
		else if (selected.Equals(__DELPLT_RUN_MODE_DIFFX))
		{
			return StateMod_GraphNode.RUNTYPE_DIFFX;
		}
		else
		{ // if (selected.equals(__DELPLT_RUN_MODE_MERGE)) {
			return StateMod_GraphNode.RUNTYPE_MERGE;
		}
	}

	/// <summary>
	/// Responds when item states have changed.
	/// </summary>
	public virtual void itemStateChanged(ItemEvent @event)
	{
		__dirty = true;
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void keyPressed(KeyEvent @event)
	{
	}

	/// <summary>
	/// Responds to key released events; calls 'processTableSelection' with the 
	/// newly-selected index in the table. </summary>
	/// <param name="event"> the KeyEvent that happened. </param>
	public virtual void keyReleased(KeyEvent @event)
	{
		checkButtonStates();
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void keyTyped(KeyEvent @event)
	{
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void mouseClicked(MouseEvent @event)
	{
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void mouseEntered(MouseEvent @event)
	{
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void mouseExited(MouseEvent @event)
	{
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void mousePressed(MouseEvent @event)
	{
	}

	/// <summary>
	/// Responds to mouse released events; calls 'processTableSelection' with the 
	/// newly-selected index in the table. </summary>
	/// <param name="event"> the MouseEvent that happened. </param>
	public virtual void mouseReleased(MouseEvent @event)
	{
		checkButtonStates();
	}

	/// <summary>
	/// Run SmDelta as follows:
	/// <ol>
	/// <li>	Run SmDelta.</li>
	/// <li>	Convert SmDelta .xgr output file to a more general .csv file format.</li>
	/// <li>	Display the .csv file contents as a layer on the map (if requested).</li>
	/// </ol>
	/// </summary>
	public virtual void runSmDelta()
	{
		string routine = "StateMod_RunSmDelta_JFrame.runSmDelta";

		if (__worksheet.getRowCount() == 0)
		{
			Message.printWarning(1, routine, "Input for " + __SmDelta + " has not been defined.");
			return;
		}

		(new ResponseJDialog(this, "Run " + __SmDelta, "The " + __SmDelta + " utility program will now be run to\n" + "prepare summary data that can be displayed on the map.", ResponseJDialog.OK)).response();

		// Check to see if anything in the template has changed since a template was last read in(if any).
		// If so, rewrite template file.

		if (__dirty)
		{
			// warn users 
			/* Until get OK from Rays, stick with *.in...
			if (IOUtil.testing()) {
				// Use *.delplt for file name...
				new ResponseJDialog(this, "Save Delplt Input",
				"Changes have been made to the input " +
				"file used by \"delplt\".\nYou will be asked to " +
				"select an input file name(convention is *.delplt).",
				ResponseJDialog.OK).response();
			}
			else {*/
			// Use *.in for file name...
			(new ResponseJDialog(this, "Save SmDelta Input", "Changes have been made to the input " + "file used by \"SmDelta\".\nYou will be asked " + "to select an input file name (convention is *.SmDelta).", ResponseJDialog.OK)).response();
			//}

			// If false is returned, the save was cancelled and no need to continue.
			if (!saveSmDeltaFile())
			{
				return;
			}
		}

		// Construct the(*.xgr)output file name created by SmDelta...

		int index = __smdeltaFilename.IndexOf(".", StringComparison.Ordinal);
		if (index == -1)
		{
			Message.printWarning(1, routine, "Don't know how to handle filename from output.  " + "You must include a \".\" in the filename.", this);
			return;
		}
		string xgrOutputFilename = __smdeltaFilename.Substring(0, index).ToUpper() + ".XGR";

		File xgrFile = new File(__smdeltaFolder + File.separator + xgrOutputFilename);

		// Delete the file if it exists so we don't get bogus output...

		if (xgrFile.exists())
		{
			Message.printStatus(1, routine, "Deleting existing " + __SmDelta + " output file \"" + __smdeltaFolder + File.separator + xgrOutputFilename + "\"");
			xgrFile.delete();
			xgrFile = null;
		}

		// Also delete the SmDelta log file so if there is an error we can display the correct one...

		// Start with the input file, and strip off the extension and replace with "log".
		string logfileString = __smdeltaFilename.Substring(0, index) + ".log";

		File logfile = new File(logfileString);
		if (logfile.exists())
		{
			Message.printStatus(1, routine, "Deleting existing SmDelta log file \"" + logfileString + "\"");
			logfile.delete();
			logfile = null;
		}

		// Run command using GUI...

		PropList props = new PropList("SmDelta");
		// This is modal...
		string[] command_array = new string[2];
		command_array[0] = StateMod_Util.getSmDeltaExecutable();
		command_array[1] = __smdeltaFolder + File.separator + __smdeltaFilename;
		ProcessManager pm = new ProcessManager(command_array);
		ProcessManagerJDialog pmgui = new ProcessManagerJDialog(this,"StateMod SmDelta", pm, props);

		int exitStatus = pmgui.getExitStatus();
		Message.printStatus(1, routine, "exitStatus is " + exitStatus);

		if (exitStatus != 0)
		{
			// Try checking for the log file.  If it does not exist, then SmDelta is probably not in the path...
			logfile = new File(logfileString);
			if (logfile.exists())
			{
				int x = (new ResponseJDialog(this, "SmDelta Unsuccessful", "SmDelta did not complete successfully.\n\n" + "View SmDelta log file?", ResponseJDialog.YES | ResponseJDialog.NO)).response();
				if (x == ResponseJDialog.YES)
				{
					try
					{
						string[] command_array2 = new string[2];
						command_array2[0] = StateMod_GUIUtil._editorPreference;
						command_array2[1] = logfileString;
						ProcessManager p = new ProcessManager(command_array2);
						// This will run as a thread until the process is shut down...
						Thread t = new Thread(p);
						t.Start();
					}
					catch (Exception)
					{
						Message.printWarning(1, routine, "Unable to view/edit file \"" + logfileString + "\"");
					}
				}
			}
			else
			{
				// Log file does not exist...
				(new ResponseJDialog(this, "SmDelta Unsuccessful", "SmDelta did not complete successfully.\n\n" + "No SmDelta log file is available.  Verify that SmDelta is in the PATH.", ResponseJDialog.OK)).response();
			}
			return;
		}
		else
		{
			// Successful execution so no reason to show the...
			// However, because the ProcessManagerJDialog is modal, we don't
			// get to here until the dialog is closed!  For now, require the
			// user to close...
			//pmgui.close();
		}

		// Now - if command was successful, there should be a <filename>.xgr
		// file in the directory corresponding to the SmDelta .rsp file
		// directory.  If so, ask user for a .csv filename to save converted
		// information to.  Note that this is different from the StateMod report
		// which puts the output in the directory of the response file for each
		// individual run.  For SmDelta, the output is always in the directory
		// where the SmDelta input file is.
		if (Message.isDebugOn)
		{
			Message.printDebug(10, routine, "Looking for xgr file: " + __smdeltaFolder + File.separator + xgrOutputFilename);
		}
		//File xgrOutput = new File(__templateLocation + xgrOutputFilename);
		// Reopen this file to see if output was created...
		xgrFile = new File(__smdeltaFolder + File.separator + xgrOutputFilename);

		// Sometimes need to wait for the file to be created...
		/* TODO take this out - the new ProcessManager seems to be better
			behaved so see if we can do without.
		if (TimeUtil.waitForFile(__templateLocation +
			xgrOutputFilename, 500, 120)) {}
		*/

		if (xgrFile.exists())
		{
			// Describe to users what is going to happen.  This is done with
			// a dialog because the JFileChooser does not have a way to add more instructions...
			int response = (new ResponseJDialog(this, "Save SmDelta Output as Text File", "SmDelta was successful.\n\nThe output will now " + "be converted from " + xgrOutputFilename + " format to a comma delimited format\n" + "that can be used directly by the StateMod GUI, GIS" + " applications and other software.\n" + "You will be asked to select an output filename " + "(convention is *.csv).\n\n" + "Select Cancel if no further action is desired.", ResponseJDialog.OK | ResponseJDialog.CANCEL)).response();
			if (response == ResponseJDialog.CANCEL)
			{
				pmgui.toFront();
				return;
			}

			JGUIUtil.setWaitCursor(this, true);
			string directory = JGUIUtil.getLastFileDialogDirectory();

			JFileChooser fc = null;
			if (!string.ReferenceEquals(directory, null))
			{
				fc = new JFileChooser(directory);
			}
			else
			{
				fc = new JFileChooser();
			}

			fc.setDialogTitle("Select Filename for SmDelta Text Output");
			SimpleFileFilter tpl = new SimpleFileFilter("csv","Comma Separated Value (CSV) Files");
			fc.addChoosableFileFilter(tpl);
			fc.setAcceptAllFileFilterUsed(true);
			fc.setFileFilter(tpl);
			fc.setDialogType(JFileChooser.SAVE_DIALOG);

			JGUIUtil.setWaitCursor(this, false);
			int retVal = fc.showSaveDialog(this);
			if (retVal != JFileChooser.APPROVE_OPTION)
			{
				return;
			}

			string currDir = (fc.getCurrentDirectory()).ToString();

			__smdeltaFolder = currDir;

			if (!currDir.Equals(directory, StringComparison.OrdinalIgnoreCase))
			{
				JGUIUtil.setLastFileDialogDirectory(currDir);
			}
			string filename = fc.getSelectedFile().getName();
			filename = IOUtil.enforceFileExtension(filename, "csv");

			StateMod_DeltaPlot smdeltaOutput = new StateMod_DeltaPlot();
			try
			{
				smdeltaOutput.readStateModDeltaOutputFile(__smdeltaFolder + File.separator + xgrOutputFilename);
				smdeltaOutput.writeArcViewFile(null, currDir + File.separator + filename, null);

				// Prompt the user if they want to add to the map.
				// Sometimes all they are trying to do is to run SmDelta
				// so they can look at the text output.

				response = (new ResponseJDialog(this, "Display " + __SmDelta + " Output on Map", "Do you want to display the " + __SmDelta + " output as " + "a map layer?\nYou can add to the map later if you answer No.", ResponseJDialog.YES | ResponseJDialog.NO)).response();
				if (response == ResponseJDialog.NO)
				{
					pmgui.toFront();
					return;
				}
		/*
		TODO (JTS - 2003-08-25)
		no 'addSummaryMapLayer' method in the main jframe
				// Create a layer and add it to the geoview...
				((StateModGUI_JFrame)StateMod_GUIUtil.getWindow(
					StateMod_GUIUtil.MAIN_WINDOW))
					.addSummaryMapLayer(currDir + File.separator 
					+ filename);
		*/
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				Console.Write(ex.StackTrace);
			}
		}
		else
		{
			Message.printWarning(1, routine, __smdeltaFolder + xgrOutputFilename + " file never created.", this);
		}
	}

	/// <summary>
	/// Saves the SmDelta plot file.
	/// </summary>
	private bool saveSmDeltaFile()
	{
		// TODO (JTS - 2003-08-27)
		// check for formatted cells and disallow saving if there are errors

		string routine = "StateMod_RunDeltaPlot_JFrame.saveSmDeltaFile";

		JGUIUtil.setWaitCursor(this, true);
		string directory = JGUIUtil.getLastFileDialogDirectory();

		JFileChooser fc = null;
		if (!string.ReferenceEquals(directory, null))
		{
			fc = new JFileChooser(directory);
		}
		else
		{
			fc = new JFileChooser();
		}

		fc.setDialogTitle("Select " + __SmDelta + " Input File to Save");
		SimpleFileFilter tpl = new SimpleFileFilter(__defaultFilenameFilter, __SmDelta + " Input Files");
		fc.addChoosableFileFilter(tpl);
		fc.setAcceptAllFileFilterUsed(true);
		fc.setFileFilter(tpl);
		fc.setDialogType(JFileChooser.SAVE_DIALOG);

		JGUIUtil.setWaitCursor(this, false);
		int retVal = fc.showSaveDialog(this);
		if (retVal != JFileChooser.APPROVE_OPTION)
		{
			return false;
		}

		string currDir = (fc.getCurrentDirectory()).ToString();

		__smdeltaFolder = currDir;

		if (!currDir.Equals(directory, StringComparison.OrdinalIgnoreCase))
		{
			JGUIUtil.setLastFileDialogDirectory(currDir);
		}
		string filename = fc.getSelectedFile().getName();
		filename = IOUtil.enforceFileExtension(filename, __SmDelta);

		__smdeltaFilename = filename;

		JGUIUtil.setWaitCursor(this, true);

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<StateMod_GraphNode> theGraphNodes = (java.util.List<StateMod_GraphNode>)__tableModel.formSaveData(__worksheet.getAllData());
		IList<StateMod_GraphNode> theGraphNodes = (IList<StateMod_GraphNode>)__tableModel.formSaveData(__worksheet.getAllData());

		StateMod_GraphNode gn = null;
		int runtype = getRunType();
		for (int i = 0; i < theGraphNodes.Count; i++)
		{
			gn = theGraphNodes[i];
			gn.setSwitch(runtype);
		}
		try
		{
			StateMod_GraphNode.writeStateModDelPltFile(null, currDir + File.separator + filename, theGraphNodes, null);
				__dirty = false;
		}
		catch (Exception e)
		{
			Message.printWarning(1, routine, "Error saving output control file\n" + "\"" + currDir + File.separator + filename + "\"", this);
			Message.printWarning(2, routine, e);
			JGUIUtil.setWaitCursor(this, false);
			return false;
		}
		JGUIUtil.setWaitCursor(this, false);
		__dirty = false;
		return true;
	}

	/// <summary>
	/// This method is called by the SMbigPictureGrid displayData()method.
	/// TODO (JTS - 2003-08-25) This should actually be called when the values are read in from a file. </summary>
	/// <param name="runType"> StateMod_GraphNode.RUNTYPE_* values. </param>
	public virtual void setRunType(int runType)
	{
		if (runType == StateMod_GraphNode.RUNTYPE_SINGLE)
		{
			__smdeltaRunModeSimpleJComboBox.select(__DELPLT_RUN_MODE_SINGLE);
		}
		else if (runType == StateMod_GraphNode.RUNTYPE_MULTIPLE)
		{
			__smdeltaRunModeSimpleJComboBox.select(__DELPLT_RUN_MODE_MULTIPLE);
		}
		else if (runType == StateMod_GraphNode.RUNTYPE_DIFFERENCE)
		{
			__smdeltaRunModeSimpleJComboBox.select(__DELPLT_RUN_MODE_DIFFERENCE);
		}
		else if (runType == StateMod_GraphNode.RUNTYPE_DIFFX)
		{
			__smdeltaRunModeSimpleJComboBox.select(__DELPLT_RUN_MODE_DIFFX);
		}
		else if (runType == StateMod_GraphNode.RUNTYPE_MERGE)
		{
			__smdeltaRunModeSimpleJComboBox.select(__DELPLT_RUN_MODE_MERGE);
		}
	}

	/// <summary>
	/// Set the messages that are visible in the bottom of the window. </summary>
	/// <param name="message"> General message string. </param>
	/// <param name="status"> Status string(e.g., "Ready", "Wait". </param>
	private void setMessages(string message, string status)
	{
		if (!string.ReferenceEquals(message, null))
		{
			__messageTextField.setText(message);
		}
		if (!string.ReferenceEquals(status, null))
		{
			__statusTextField.setText(status);
		}
	}

	/// <summary>
	/// Sets up the GUI.
	/// </summary>
	private void setupGUI()
	{
		string routine = "StateMod_RunDeltaPlot_JFrame.setupGUI";

		addWindowListener(this);

		__smdeltaFilename = "";

		__addRowButton = new SimpleJButton(__BUTTON_ADD_ROW, this);
		__deleteRowButton = new SimpleJButton(__BUTTON_DELETE_ROWS, this);
		__saveTemplateButton = new SimpleJButton(__BUTTON_SAVE, this);
		__clearWorksheetButton = new SimpleJButton(__BUTTON_REMOVE_ALL_ROWS, this);
		__selectTemplateButton = new SimpleJButton(__BUTTON_LOAD, this);

		__helpButton = new SimpleJButton(__BUTTON_HELP, this);
		__closeButton = new SimpleJButton(__BUTTON_CLOSE, this);
		__runSmDeltaButton = new SimpleJButton(__BUTTON_RUN, this);

		__smdeltaRunModeSimpleJComboBox = new SimpleJComboBox();
		__smdeltaRunModeSimpleJComboBox.add(__DELPLT_RUN_MODE_SINGLE);
		__smdeltaRunModeSimpleJComboBox.add(__DELPLT_RUN_MODE_MULTIPLE);
		__smdeltaRunModeSimpleJComboBox.add(__DELPLT_RUN_MODE_DIFFERENCE);
		__smdeltaRunModeSimpleJComboBox.add(__DELPLT_RUN_MODE_DIFFX);
		__smdeltaRunModeSimpleJComboBox.add(__DELPLT_RUN_MODE_MERGE);
		__smdeltaRunModeSimpleJComboBox.addItemListener(this);
		// Default is to select multiple mode...
		__smdeltaRunModeSimpleJComboBox.select(1);

		__autoLineCopyJCheckBox = new JCheckBox("Automatically fill initial line content on \"Add a Row\".", true);

		// Make a main panel to be the resizable body of the frame...
		JPanel mainPanel = new JPanel();
		GridBagLayout gb = new GridBagLayout();
		mainPanel.setLayout(gb);

		GridLayout gl = new GridLayout(2, 2, 2, 2);
		JPanel topPanel = new JPanel();
		topPanel.setLayout(gl);

		GridLayout gl2 = new GridLayout(1, 0, 2, 0);
		JPanel bottomPanel = new JPanel();
		bottomPanel.setLayout(gl2);

		FlowLayout fl = new FlowLayout(FlowLayout.CENTER);
		JPanel finalButtonPanel = new JPanel();
		finalButtonPanel.setLayout(fl);

		JPanel gridPanel = new JPanel();
		gridPanel.setLayout(gb);

		// add add a row, delete selected rows, clear spreadsheet,
		// select template, save template buttons
		topPanel.add(__addRowButton);
		topPanel.add(__deleteRowButton);
		topPanel.add(__clearWorksheetButton);
		topPanel.add(__selectTemplateButton);
		int y = 0;
		JGUIUtil.addComponent(mainPanel, topPanel, 0, y, 10, 3, 0, 0, 10, 10, 10, 10, GridBagConstraints.NONE, GridBagConstraints.NORTH);
		y += 3;
		JPanel runTypeJPanel = new JPanel(new FlowLayout());
		runTypeJPanel.add(new JLabel(__SmDelta + " run mode:"));
		runTypeJPanel.add(__smdeltaRunModeSimpleJComboBox);
		JGUIUtil.addComponent(mainPanel, runTypeJPanel, 0, y, 10, 1, 1, 0, 0, 0, 0, 0, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);
		JGUIUtil.addComponent(mainPanel, new JLabel("(1) File:  For reservoirs specify the ASCII " + ".xre or binary .b44 file."), 0, ++y, 10, 1, 1, 0, 0, 4, 0, 0, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);
		JGUIUtil.addComponent(mainPanel, new JLabel("         For other node types specify the " + "ASCII .xdd or binary .b43 file.  Or, specify" + " blank if continuing list of IDs."), 0, ++y, 10, 1, 1, 0, 0, 4, 0, 0, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);
		JGUIUtil.addComponent(mainPanel, new JLabel("(2) Station type:  diversion, instream, " + "reservoir, stream, streamID (0* gages)" + ", well, or blank if continuing list of IDs."), 0, ++y, 10, 1, 1, 0, 0, 4, 0, 0, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);
		JGUIUtil.addComponent(mainPanel, new JLabel("(3) Parameter:  parameters from StateMod " + "output, or blank if continuing list of IDs."), 0, ++y, 10, 1, 1, 0, 0, 4, 0, 0, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);
		JGUIUtil.addComponent(mainPanel, new JLabel("(4) Year/Ave:  Enter Ave, 4-digit year " + "(e.g., 1989) or year and month (e.g., " + "1989 NOV), or blank if continuing list of" + " IDs."), 0, ++y, 10, 1, 1, 0, 0, 4, 0, 0, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);
		JGUIUtil.addComponent(mainPanel, new JLabel("(5) ID:  0 (zero) for all or enter a specific " + "identifier."), 0, ++y, 10, 1, 1, 0, 0, 4, 0, 0, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);
		JGUIUtil.addComponent(mainPanel, __autoLineCopyJCheckBox, 0, ++y, 1, 1, 0, 0, 0, 10, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);

		PropList p = new PropList("StateMod_RunDeltaPlot_JFrame.JWorksheet");
		p.add("JWorksheet.ShowRowHeader=true");
		p.add("JWorksheet.AllowCopy=true");
		p.add("JWorksheet.RowColumnBackground=LightGray");
		p.add("JWorksheet.ShowPopupMenu=true");

		File file = new File(__dataset.getDataSetDirectory());
		__smdeltaFolder = __dataset.getDataSetDirectory();
		JGUIUtil.setLastFileDialogDirectory(__dataset.getDataSetDirectory());
		IList<string> filters = new List<string>();
		filters.Add("xre");
		filters.Add("b44");
		filters.Add("xdd");
		filters.Add("b43");
		SimpleFileFilter sff = new SimpleFileFilter(filters, "etc");
		File[] files = file.listFiles(sff);

		IList<string> filenames = new List<string>();
		filenames.Add("");
		filenames.Add(OPTION_BROWSE);
		for (int i = 0; i < files.Length; i++)
		{
			filenames.Add(files[i].getName());
			Message.printStatus(1, "", "File " + i + ": '" + files[i].getName() + "'");
		}

		int[] widths = null;
		JScrollWorksheet jsw = null;
		try
		{
			__tableModel = new StateMod_RunSmDelta_TableModel(this, new List<object>(), (IList<StateMod_Reservoir>)__reservoirComp.getData(), (IList<StateMod_Diversion>)__diversionComp.getData(), (IList<StateMod_InstreamFlow>)__instreamFlowComp.getData(), (IList<StateMod_StreamGage>)__streamGageComp.getData(), (IList<StateMod_Well>)__wellComp.getData());


			StateMod_RunSmDelta_CellRenderer crg = new StateMod_RunSmDelta_CellRenderer(__tableModel);

			jsw = new JScrollWorksheet(crg, __tableModel, p);
			__worksheet = jsw.getJWorksheet();

			__worksheet.setColumnJComboBoxValues(0, filenames, true);

			IList<string> vn = StateMod_Util.arrayToList(StateMod_GraphNode.node_types);

			int index = vn.IndexOf("Streamflow");
			vn.Insert(index + 1, "Stream ID (0* Gages)");

			vn.Add("");

			__worksheet.setColumnJComboBoxValues(1, vn, true);

			__worksheet.setCellSpecificJComboBoxColumn(2);
			__worksheet.setCellSpecificJComboBoxColumn(4, true);

			__tableModel.setWorksheet(__worksheet);

			widths = crg.getColumnWidths();
		}
		catch (Exception e)
		{
			Message.printWarning(1, routine, "Error building worksheet.");
			Message.printWarning(2, routine, e);
			jsw = new JScrollWorksheet(0, 0, p);
			__worksheet = jsw.getJWorksheet();
		}
		__worksheet.setPreferredScrollableViewportSize(null);
		__worksheet.setHourglassJFrame(this);
		__worksheet.addMouseListener(this);
		__worksheet.addKeyListener(this);

		JGUIUtil.addComponent(gridPanel, jsw, 0, 0, 1, 1, 1, 1, GridBagConstraints.BOTH, GridBagConstraints.CENTER);
		JGUIUtil.addComponent(mainPanel, gridPanel, 0, ++y, 10, 12, 1, 1, GridBagConstraints.BOTH, GridBagConstraints.CENTER);

		y += 11; // To account for grid height

		// Add bottom buttons - these are alphabetical so be
		// careful if more are added

		finalButtonPanel.add(__runSmDeltaButton);
		finalButtonPanel.add(__closeButton);
		finalButtonPanel.add(__helpButton);
		finalButtonPanel.add(__saveTemplateButton);

		// Add the final buttons on the bottom to the bottom panel...
		bottomPanel.add(finalButtonPanel);
		// Add the button panel to the frame...
		JGUIUtil.addComponent(mainPanel, bottomPanel, 0, ++y, 10, 1, 0, 0, GridBagConstraints.VERTICAL, GridBagConstraints.SOUTH);

		// Add the main panel as the resizable content...
		getContentPane().add("Center", mainPanel);

		// Add JTextFields for messages...
		JPanel message_JPanel = new JPanel();
		message_JPanel.setLayout(gb);
		__messageTextField = new JTextField();
		__messageTextField.setEditable(false);
		__statusTextField = new JTextField("             ");
		__statusTextField.setEditable(false);
		JGUIUtil.addComponent(message_JPanel, __messageTextField, 0, 0, 9, 1, 1, 0, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);
		JGUIUtil.addComponent(message_JPanel, __statusTextField, 9, 0, 1, 1, 0, 0, GridBagConstraints.HORIZONTAL, GridBagConstraints.SOUTH);

		getContentPane().add("South", message_JPanel);

		pack();
		setSize(950, 625);
		setMessages("Add parameters to analyze", "Ready");
		JGUIUtil.center(this);
		setVisible(true);

		checkButtonStates();

		if (widths != null)
		{
			__worksheet.setColumnWidths(widths);
		}
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void windowActivated(WindowEvent @event)
	{
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void windowClosed(WindowEvent @event)
	{
	}

	/// <summary>
	/// Responds to Window closing events; closes the window and marks it closed
	/// in StateMod_GUIUtil. </summary>
	/// <param name="event"> the WindowEvent that happened. </param>
	public virtual void windowClosing(WindowEvent @event)
	{
		closeWindow();
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

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void windowOpening(WindowEvent @event)
	{
	}

	}

}