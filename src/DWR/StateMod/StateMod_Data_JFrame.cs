using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

// StateMod_Data_JFrame - abstract class from which all the JFrames that display tabular StateMod data are built.

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
// StateMod_Data_JFrame - An abstract class from which all the JFrames that
//	display tabular StateMod data are built.
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
// 
// 2005-01-13	J. Thomas Sapienza, RTi	Initial version.
// 2005-01-20	JTS, RTi		Following review:
//					* Updated comments.
//					* Made JScrollWorksheet private.
//					* Removed getDataType().
//					* GUI title is now passed in to this
//					  class.
//					* The JFrame now sets its icon.
// 2005-03-28	JTS, RTi		Overrode setSize() and setVisible().
// 2005-03-29	JTS, RTi		* formatOutput() puts quotes around the
//					  column names now.
//					* formatOutput() puts quotes around 
//					  String field values.
//					* The worksheet is now set up to show
//					  the row number header.
// 2005-03-30	JTS, RTi		Added the optional top panel.
// 2004-04-06	JTS, RTi		* Added variables that specify the 
//					  number of lines to be printed in 
//					  landscape and portrait modes.
//					* Added print() method.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace DWR.StateMod
{


	using JFileChooserFactory = RTi.Util.GUI.JFileChooserFactory;
	using JGUIUtil = RTi.Util.GUI.JGUIUtil;
	using JScrollWorksheet = RTi.Util.GUI.JScrollWorksheet;
	using JWorksheet = RTi.Util.GUI.JWorksheet;
	using JWorksheet_DefaultTableCellRenderer = RTi.Util.GUI.JWorksheet_DefaultTableCellRenderer;
	using ReportPrinter = RTi.Util.GUI.ReportPrinter;
	using SimpleFileFilter = RTi.Util.GUI.SimpleFileFilter;
	using SimpleJButton = RTi.Util.GUI.SimpleJButton;
	using IOUtil = RTi.Util.IO.IOUtil;
	using PropList = RTi.Util.IO.PropList;
	using SecurityCheck = RTi.Util.IO.SecurityCheck;
	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// This class is a JFrame that displays StateMod data in a worksheet.  The 
	/// worksheet data can be exported to a file, printed, or edited.  This class 
	/// cannot be instantiated because it is abstract, but it provides a framework 
	/// for the derived classes.  <para>
	/// 
	/// </para>
	/// <b>Derived Class Requirements</b><para>
	/// 
	/// </para>
	/// The derived classes have to implement the following methods:<para>
	/// <ul>
	/// <li>apply() -- called when changes to the data are committed.</li>
	/// <li>buildJScrollWorksheet() -- called by the code that sets up the GUI to
	///    create the worksheet that will be displayed in the GUI.</li>
	/// <li>cancel() -- called when changes to the data are discarded.</li>
	/// </ul>
	/// 
	/// </para>
	/// <para>
	/// 
	/// </para>
	/// <b>Data Available for Use by Derived Classes</b><para>
	/// 
	/// There are several protected data members in this class that derived classes are 
	/// </para>
	/// expected to use.  These are:<para>
	/// 
	/// <ul>
	/// <li>_editable -- contains whether the data in the worksheet can be edited or 
	///    not.  If false, the data cannot be edited and the Apply and Cancel buttons
	///    will not appear.  If true, the data can be edited and the Apply and Cancel
	///    buttons will appear on the GUI.</li>
	/// <li>_worksheet -- contains the worksheet created by the derived class.  This is
	///    assigned automatically in this class's setupGUI() method, but is protected
	///    so that derived classes have access to it.</li>
	/// <li>_props -- contains default properties that can be used to ensure all the
	///    derived classes' worksheets have uniform properties.</li>
	/// <li>_titleString -- contains the title of the GUI as passed in to the
	///    constructor of the super class.  It is protected so that it can be 
	///    accessed, for instance, to change the GUI title if data have been edited.
	///    </li>
	/// <li>_data -- contains the data stored in the worksheet.  This is assigned in 
	///    this class's constructor but is protected so that the data are available
	///    in the derived classes.</li/>
	/// </ul>
	/// 
	/// </para>
	/// <b>Issues</b><para>
	/// </para>
	/// REVISIT (2005-01-20)<para>
	/// Currently there is no window management being done for this class and its 
	/// derived classes.  This will be added in the future but for now multiple windows
	/// will be able to be opened, etc.
	/// </para>
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public abstract class StateMod_Data_JFrame extends javax.swing.JFrame implements java.awt.event.ActionListener, java.awt.event.WindowListener
	public abstract class StateMod_Data_JFrame : JFrame, ActionListener, WindowListener
	{

	/// <summary>
	/// Button labels.
	/// </summary>
	protected internal readonly string _BUTTON_APPLY = "Apply", _BUTTON_CANCEL = "Cancel", _BUTTON_EXPORT = "Export", _BUTTON_OK = "OK", _BUTTON_PRINT = "Print";

	/// <summary>
	/// Whether the data in the worksheet can be edited (true) or not (false).
	/// </summary>
	protected internal bool _editable = false;

	/// <summary>
	/// The number of lines to be printed in landscape and portrait modes.  Public
	/// so that all derived classes can access them.
	/// </summary>
	public int PRINT_LANDSCAPE_LINES = 44, PRINT_PORTRAIT_LINES = 66;

	/// <summary>
	/// The JScrollWorksheet that is created and placed in the GUI.  It is created
	/// in the derived classes in the method buildJScrollWorksheet().
	/// </summary>
	private JScrollWorksheet __scrollWorksheet = null;

	/// <summary>
	/// The worksheet that is created in the derived classes.
	/// </summary>
	protected internal JWorksheet _worksheet = null;

	/// <summary>
	/// Generic properties that can be used in any worksheet in the derived classes. 
	/// These are available so that all the derived classes' worksheets can be uniform
	/// and have the same properties.
	/// </summary>
	protected internal PropList _props = null;

	/// <summary>
	/// The title of the GUI.
	/// </summary>
	protected internal string _titleString = null;

	/// <summary>
	/// The data to display in the worksheet, assigned in the derived classes. Assigned
	/// by this class's constructor when the derived classes call super().  Must be 
	/// passed to this class via the constructor in order for the Apply|Cancel|OK 
	/// buttons to function properly.
	/// </summary>
	protected internal System.Collections.IList _data = null;

	/// <summary>
	/// Constructor.  This constructor should only be called if initialize() will be called later.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public StateMod_Data_JFrame() throws Exception
	public StateMod_Data_JFrame() : base()
	{
	}

	/// <summary>
	/// Constructor. </summary>
	/// <param name="data"> the data to display in the worksheet.  Can be null or empty, in
	/// which case an empty worksheet is shown. </param>
	/// <param name="titleString"> the String to display as the GUI's title. </param>
	/// <param name="editable"> whether the data in the JFrame can be edited or not.  If true
	/// the data can be edited, if false they can not. </param>
	/// <exception cref="Exception"> if there is an error building the worksheet. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public StateMod_Data_JFrame(java.util.List data, String titleString, boolean editable) throws Exception
	public StateMod_Data_JFrame(System.Collections.IList data, string titleString, bool editable) : base()
	{

		initialize(data, titleString, editable);
	}

	/// <summary>
	/// Responds to action events. </summary>
	/// <param name="event"> the ActionEvent that happened. </param>
	public virtual void actionPerformed(ActionEvent @event)
	{
		string routine = "StateMod_Data_JFrame.actionPerformed";

		string action = @event.getActionCommand();

		if (action.Equals(_BUTTON_APPLY))
		{
			_worksheet.stopEditing();
			apply();
		}
		else if (action.Equals(_BUTTON_CANCEL))
		{
			_worksheet.cancelEditing();
			cancel();
			closeJFrame();
		}
		else if (action.Equals(_BUTTON_EXPORT))
		{
			string[] s = getFilenameAndFormat();

			if (s == null)
			{
				// user cancelled
				return;
			}

			IList<string> v = formatOutput(s[1], true, true);
			try
			{
				export(s[0], v);
			}
			catch (Exception e)
			{
				Message.printWarning(1, routine, "Error exporting data.");
				Message.printWarning(2, routine, e);
			}
		}
		else if (action.Equals(_BUTTON_OK))
		{
			_worksheet.stopEditing();
			apply();
			closeJFrame();
		}
		else if (action.Equals(_BUTTON_PRINT))
		{
			print();
		}
	}

	/// <summary>
	/// Called when the Apply or OK button is pressed.  This method must commit all 
	/// changes that were made to the data objects.<para>
	/// Derived classes should implement the method similar to:<pre>
	/// protected void apply() {
	/// StateMod_Reservoir res = null;
	/// for (int i = 0; i < _data.size(); i++) {
	///		res = (StateMod_Reservoir)_data.elementAt(i);
	///		res.createBackup();
	/// }
	/// }
	/// </pre>
	/// </para>
	/// </summary>
	protected internal abstract void apply();

	/// <summary>
	/// Called by setupGUI, this builds the table model and cell renderer 
	/// necessary for a worksheet, then builds the JScrollWorksheet and returns it. </summary>
	/// <returns> the JScrollWorksheet that was made and which should be displayed in
	/// the GUI. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected abstract RTi.Util.GUI.JScrollWorksheet buildJScrollWorksheet() throws Exception;
	protected internal abstract JScrollWorksheet buildJScrollWorksheet();

	/// <summary>
	/// Called when the cancel button is pressed.  Throws away all changes that
	/// were made to data objects and restores them to their original state.<para>
	/// Derived classes should implement the method similar to:<pre>
	/// protected void cancel() {
	/// StateMod_Reservoir res = null;
	/// for (int i = 0; i < _data.size(); i++) {
	///		res = (StateMod_Reservoir)_data.elementAt(i);
	///		res.restoreOriginal();
	/// }
	/// }
	/// </pre>
	/// </para>
	/// </summary>
	protected internal abstract void cancel();

	/// <summary>
	/// Closes the JFrame, setting it not visible and disposing of it.
	/// </summary>
	protected internal virtual void closeJFrame()
	{
		setVisible(false);
		dispose();
	}

	/// <summary>
	/// Creates backup copies of all the data to be displayed in the worksheet, so that
	/// changes can later be cancelled.  Called from this superclass's constructor.<para>
	/// Derived classes should implement the method similar to:<pre>
	/// protected void createDataBackup() {
	/// StateMod_Reservoir res = null;
	/// for (int i = 0; i < _data.size(); i++) {
	///		res = (StateMod_Reservoir)_data.elementAt(i);
	///		res.createBackup();
	/// }
	/// }
	/// </pre> 
	/// </para>
	/// </summary>
	protected internal abstract void createDataBackup();

	/// <summary>
	/// Exports a list of strings to a file. </summary>
	/// <param name="filename"> the name of the file to write. </param>
	/// <param name="strings"> a non-null Vector of Strings, each element of which will be
	/// another line in the file. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void export(String filename, java.util.List<String> strings) throws Exception
	protected internal virtual void export(string filename, IList<string> strings)
	{
		string routine = "StateMod_Data_JFrame.export";
		// First see if we can write the file given the security
		// settings...
		if (!SecurityCheck.canWriteFile(filename))
		{
			Message.printWarning(1, routine, "Cannot save \"" + filename + "\".");
			throw new Exception("Security check failed - unable to write \"" + filename + "\"");
		}

		JGUIUtil.setWaitCursor(this, true);

		// Create a new FileOutputStream wrapped with a DataOutputStream
		// for writing to a file.
		PrintWriter oStream = null;
		try
		{
			oStream = new PrintWriter(new StreamWriter(filename));
		}
		catch (Exception)
		{
			JGUIUtil.setWaitCursor(this, false);
			throw new Exception("Error opening file \"" + filename + "\".");
		}

		try
		{
			// Write each element of the strings Vector to a file.
			// For some reason, when just using println in an
			// applet, the cr-nl pair is not output like it should
			// be on Windows95.  Java Bug???
			string linesep = System.getProperty("line.separator");
			int size = strings.Count;
			for (int i = 0; i < size; i++)
			{
				oStream.print(strings[i].ToString() + linesep);
			}
			oStream.flush();
			oStream.close();
		}
		catch (Exception)
		{
			JGUIUtil.setWaitCursor(this, false);
			throw new Exception("Error writing to file \"" + filename + "\".");
		}

		JGUIUtil.setWaitCursor(this, false);
	}

	/// <summary>
	/// Cleans up member variables. 
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~StateMod_Data_JFrame()
	{
		__scrollWorksheet = null;
		_worksheet = null;
		_data = null;
	}

	/// <summary>
	/// Formats the data in the worksheet into a list of Strings.  Each field in the
	/// worksheet will be separated from the next by the specified delimiter, with
	/// no trailing delimiter. </summary>
	/// <param name="delimiter"> the character String to use to separate worksheet fields. </param>
	/// <param name="quotes"> if true, then the column names will be surrounded by 
	/// quotes.  In addition, if the column data has the delimiter in it, it will be
	/// surrounded by quotes.  If false, nothing will be surrounded by quotes. </param>
	/// <returns> a list of delimited strings.  Each element in the list is one
	/// row in the worksheet. </returns>
	protected internal virtual IList<string> formatOutput(string delimiter, bool trimFieldValue, bool quotes)
	{
		IList<string> v = new List<string>();

		int rows = _worksheet.getRowCount();
		int cols = _worksheet.getColumnCount();

		StringBuilder buff = new StringBuilder();
		string quote = "\"";
		for (int i = 0; i < cols; i++)
		{
			if (quotes)
			{
				buff.Append(quote);
			}
			buff.Append(_worksheet.getColumnName(i, true));
			if (quotes)
			{
				buff.Append(quote);
			}
			if (i < cols - 1)
			{
				buff.Append(delimiter);
			}
		}

		v.Add(buff.ToString());

		int j = 0;
		string s = null;
		for (int i = 0; i < rows; i++)
		{
			buff.Length = 0;

			for (j = 0; j < cols; j++)
			{
				s = _worksheet.getValueAtAsString(i, j);
				if (trimFieldValue)
				{
					s = s.Trim();
				}
				if (s.IndexOf(delimiter, StringComparison.Ordinal) > -1 && quotes)
				{
					buff.Append("\"" + s + "\"");
				}
				else
				{
					buff.Append(s);
				}

				if (j < cols - 1)
				{
					// do not do for the last column.
					buff.Append(delimiter);
				}
			}

			v.Add(buff.ToString());
		}

		return v;
	}

	/// <summary>
	/// Returns the filename and format type of a file selected from a file chooser
	/// in order that the kind of delimiter for the file can be known when the data
	/// is formatted for output.  Currently the only kinds of files that the data
	/// can be exported to are delimited files.  No StateMod files are yet supported.<para>
	/// Also sets the last selected file dialog directory to whatever directory the
	/// file is located in, if the file selection was approved (i.e., Cancel was not
	/// pressed).
	/// </para>
	/// </summary>
	/// <param name="title"> the title of the file chooser. </param>
	/// <param name="formats"> a Vector of the valid formats for the file chooser. </param>
	/// <returns> a two-element String array where the first element is the name of the
	/// file and the second element is the delimiter selected. </returns>
	protected internal virtual string[] getFilenameAndFormat()
	{
		JGUIUtil.setWaitCursor(this, true);
		string dir = JGUIUtil.getLastFileDialogDirectory();
		JFileChooser fc = JFileChooserFactory.createJFileChooser(dir);
		fc.setDialogTitle("Select Export File");

		SimpleFileFilter tabFF = new SimpleFileFilter("txt", "Tab-delimited");
		SimpleFileFilter commaFF = new SimpleFileFilter("csv", "Comma-delimited");
		SimpleFileFilter semiFF = new SimpleFileFilter("txt", "Semicolon-delimited");
		SimpleFileFilter pipeFF = new SimpleFileFilter("txt", "Pipe-delimited");

		fc.addChoosableFileFilter(commaFF);
		fc.addChoosableFileFilter(pipeFF);
		fc.addChoosableFileFilter(semiFF);
		fc.addChoosableFileFilter(tabFF);

		fc.setAcceptAllFileFilterUsed(false);
		fc.setFileFilter(commaFF);
		fc.setDialogType(JFileChooser.SAVE_DIALOG);

		JGUIUtil.setWaitCursor(this, false);
		int returnVal = fc.showSaveDialog(this);
		if (returnVal == JFileChooser.APPROVE_OPTION)
		{
			string[] ret = new string[2];
			string filename = fc.getCurrentDirectory() + File.separator + fc.getSelectedFile().getName();
			JGUIUtil.setLastFileDialogDirectory("" + fc.getCurrentDirectory());
			SimpleFileFilter sff = (SimpleFileFilter)fc.getFileFilter();

			// this will always return a one-element vector
			IList<string> extensionV = sff.getFilters();

			string extension = extensionV[0];

			string desc = sff.getShortDescription();
			string delimiter = "\t";

			if (desc.Equals("Tab-delimited"))
			{
				delimiter = "\t";
			}
			else if (desc.Equals("Comma-delimited"))
			{
				delimiter = ",";
			}
			else if (desc.Equals("Semicolon-delimited"))
			{
				delimiter = ";";
			}
			else if (desc.Equals("Pipe-delimited"))
			{
				delimiter = "|";
			}

			ret[0] = IOUtil.enforceFileExtension(filename, extension);
			ret[1] = delimiter;

			return ret;
		}
		else
		{
			return null;
		}
	}

	/// <summary>
	/// Initializes the data and sets up the GUI, using values that were passed into
	/// the constructor.  Alternately, if other setup needs done prior to the table 
	/// models being built and the GUI set up, this can be called separately with 
	/// the same parameters that would be into the GUI. </summary>
	/// <param name="data"> the data to display in the worksheet.  Can be null or empty, in
	/// which case an empty worksheet is shown. </param>
	/// <param name="titleString"> the String to display as the GUI's title. </param>
	/// <param name="editable"> whether the data in the JFrame can be edited or not.  If true
	/// the data can be edited, if false they can not. </param>
	/// <exception cref="Exception"> if there is an error building the worksheet. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void initialize(java.util.List data, String titleString, boolean editable) throws Exception
	public virtual void initialize(System.Collections.IList data, string titleString, bool editable)
	{
		if (data == null)
		{
			_data = new List<object>();
		}
		else
		{
			_data = data;
		}

		createDataBackup();

		_props = new PropList("Worksheet Props");
		_props.add("JWorksheet.ShowPopupMenu=true");
		_props.add("JWorksheet.AllowCopy=true");
		_props.add("JWorksheet.ShowRowHeader=true");
		_props.add("JWorksheet.SelectionMode=MultipleDiscontinuousRowSelection");

		_titleString = titleString;
		_editable = editable;

		setupGUI();
	}

	/// <summary>
	/// Prints the data from the worksheet.
	/// </summary>
	public virtual void print()
	{
		string titleString = getTitle();
		int index = titleString.IndexOf("-", StringComparison.Ordinal);
		if (index > -1)
		{
			titleString = (titleString.Substring(index + 1)).Trim();
		}
		IList<string> v = formatOutput(" ", false, false);
		if (v.Count > 40)
		{
			for (int i = v.Count - 1; i > 40; i--)
			{
				v.RemoveAt(i);
			}
		}
		string s = (new RTi.Util.GUI.TextResponseJDialog(this, "Lines per page:")).response();
		if (RTi.Util.String.StringUtil.atoi(s) <= 0)
		{
			return;
		}
		int lines = RTi.Util.String.StringUtil.atoi(s);
		ReportPrinter.printText(v, lines, lines, titleString, false, null); // do not use a pre-defined PageFormat for
					// this print job
	}

	/// <summary>
	/// Sets the visibility of the GUI.  Overrides the base setVisible() method in 
	/// order to guarantee that the GUI is repainted properly and sized properly, which
	/// was proving an issue in StateDMI. </summary>
	/// <param name="visible"> whether the GUI should be made visible or not. </param>
	public virtual void setVisible(bool visible)
	{
		base.setVisible(visible);
		setSize(getWidth(), getHeight());
	}

	/// <summary>
	/// Sets the size of the GUI.  Overrides the base setSize() method because 
	/// the GUI was not repainting properly in StateDMI after a resize, and this 
	/// ensures that developers do not need to call setSize();validate();repaint() 
	/// everytime a resize is necessary. </summary>
	/// <param name="width"> the new width of the GUI window. </param>
	/// <param name="height"> the new height of the GUI window. </param>
	public virtual void setSize(int width, int height)
	{
		base.setSize(width, height);
		JGUIUtil.center(this);
		invalidate();
		validate();
		repaint();
	}

	/// <summary>
	/// Sets up the GUI. </summary>
	/// <exception cref="Exception"> if there is an error setting up the worksheet. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void setupGUI() throws Exception
	private void setupGUI()
	{
		addWindowListener(this);

		JPanel panel = new JPanel();
		panel.setLayout(new GridBagLayout());

		// buildJScrollWorksheet is defined in the derived classes (it is
		// abstract in this class).  It creates a JScrollWorksheet and returns it.  
		__scrollWorksheet = buildJScrollWorksheet();
		_worksheet = __scrollWorksheet.getJWorksheet();

		JGUIUtil.addComponent(panel, __scrollWorksheet, 0, 0, 1, 1, 1, 1, GridBagConstraints.BOTH, GridBagConstraints.CENTER);

		getContentPane().add("Center", panel);

		SimpleJButton exportButton = new SimpleJButton(_BUTTON_EXPORT, this);
		SimpleJButton printButton = new SimpleJButton(_BUTTON_PRINT, this);
		SimpleJButton okButton = new SimpleJButton(_BUTTON_OK, this);

		SimpleJButton applyButton = null;
		SimpleJButton cancelButton = null;
		if (_editable)
		{
			applyButton = new SimpleJButton(_BUTTON_APPLY, this);
			cancelButton = new SimpleJButton(_BUTTON_CANCEL, this);
		}

		JPanel bottomPanel = new JPanel();
		bottomPanel.setLayout(new FlowLayout(FlowLayout.RIGHT));
		bottomPanel.add(exportButton);
		bottomPanel.add(printButton);
		bottomPanel.add(okButton);

		if (_editable)
		{
			bottomPanel.add(applyButton);
			bottomPanel.add(cancelButton);
		}

		getContentPane().add("South", bottomPanel);

		JGUIUtil.setIcon(this, JGUIUtil.getIconImage());

		setTitle(_titleString);
		pack();

		setSize(800, 400);
		JGUIUtil.center(this);

		setVisible(true);

		JWorksheet_DefaultTableCellRenderer renderer = _worksheet.getCellRenderer();
		int[] widths = renderer.getColumnWidths();
		if (widths != null)
		{
			_worksheet.setColumnWidths(widths);
		}
	}

	public virtual void windowActivated(WindowEvent @event)
	{
	}
	public virtual void windowClosed(WindowEvent @event)
	{
	}
	public virtual void windowClosing(WindowEvent @event)
	{
		setVisible(false);
	}
	public virtual void windowDeactivated(WindowEvent @event)
	{
	}
	public virtual void windowDeiconified(WindowEvent @event)
	{
		Message.printStatus(1, "", "Size: " + getWidth() + ", " + getHeight());
	}
	public virtual void windowIconified(WindowEvent @event)
	{
	}
	public virtual void windowOpened(WindowEvent @event)
	{
	}

	}

}