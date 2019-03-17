using System;

// StateCU_CropCharacteristics_TableModel - table model for displaying data for crop char tables

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
// StateCU_CropCharacteristics_TableModel - Table model for displaying data for 
//	crop char tables
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
// 2003-07-14	J. Thomas Sapienza, RTi	Initial version.
// 2005-01-21	JTS, RTi		Added the editable flag.
// 2005-01-24	JTS, RTi		* Removed the row count column because
//					  worksheets now handle that.
//					* Added column reference variables.
// 2005-03-28	JTS, RTi		* Adjusted column sizes.
//					* Removed the ID column.
//					* Added tool tips.
// 2007-01-10   Kurt Tometich, RTi
// 								Fixed the format for the cropName to 
//								30 chars instead of 20.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace DWR.StateCU
{

	using JWorksheet_AbstractRowTableModel = RTi.Util.GUI.JWorksheet_AbstractRowTableModel;
	using Validator = RTi.Util.IO.Validator;
	using Validators = RTi.Util.IO.Validators;

	/// <summary>
	/// This class is a table model for displaying crop char data.
	/// </summary>
	public class StateCU_CropCharacteristics_TableModel : JWorksheet_AbstractRowTableModel, StateCU_Data_TableModel
	{

	/// <summary>
	/// Number of columns in the table model.
	/// </summary>
	private int __COLUMNS = 3;

	/// <summary>
	/// Column references.
	/// </summary>
	private readonly int __COL_NAME = 0, __COL_DAY_PCT = 1, __COL_VALUE = 2;

	/// <summary>
	/// Column references.
	/// </summary>
	private readonly int __COL_GDATE1 = 1, __COL_GDATE2 = 2, __COL_GDATE3 = 3, __COL_GDATE4 = 4, __COL_GDATE5 = 5, __COL_GDATES = 6, __COL_TMOIS1 = 7, __COL_TMOIS2 = 8, __COL_MAD = 9, __COL_IRX = 10, __COL_FRX = 11, __COL_AWC = 12, __COL_APD = 13, __COL_TFLAG1 = 14, __COL_TFLAG2 = 15, __COL_CUT2 = 16, __COL_CUT3 = 17;


	private bool __dayNotPercent = true;

	/// <summary>
	/// Whether the data are editable or not.
	/// </summary>
	private bool __editable = true;

	/// <summary>
	/// Whether the table model is set up to show a single crop or many crops.
	/// </summary>
	private bool __singleCrop = true;

	/// <summary>
	/// The parent crop for which subdata is displayed.
	/// </summary>
	// TODO 2007-03-01 Evaluate use
	//private StateCU_CropCharacteristics __parentCrop;

	private StateCU_BlaneyCriddle __blaneyCriddle;


	/// <summary>
	/// Constructor.  This builds the Model for displaying crop char data </summary>
	/// <param name="data"> the data that will be displayed in the table. </param>
	/// <exception cref="Exception"> if an invalid data or dmi was passed in. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public StateCU_CropCharacteristics_TableModel(java.util.List data) throws Exception
	public StateCU_CropCharacteristics_TableModel(System.Collections.IList data) : this(data, true, true)
	{
	}

	/// <summary>
	/// Constructor.  This builds the Model for displaying crop char data </summary>
	/// <param name="data"> the data that will be displayed in the table. </param>
	/// <param name="editable"> whether the data are editable or not. </param>
	/// <param name="singleCrop"> whether a single crop's characteristics are shown (true) or
	/// the characteristics for many crops are shown. </param>
	/// <exception cref="Exception"> if an invalid data or dmi was passed in. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public StateCU_CropCharacteristics_TableModel(java.util.List data, boolean editable, boolean singleCrop) throws Exception
	public StateCU_CropCharacteristics_TableModel(System.Collections.IList data, bool editable, bool singleCrop)
	{
		if (data == null)
		{
			throw new Exception("Invalid data Vector passed to " + "StateCU_CropCharacteristics_TableModel " + "constructor.");
		}
		_rows = data.Count;
		_data = data;
		__editable = editable;
		__singleCrop = singleCrop;

		if (singleCrop)
		{
			__COLUMNS = 4;
		}
		else
		{
			__COLUMNS = 19;
		}
	}

	/// <summary>
	/// Returns the class of the data stored in a given column. </summary>
	/// <param name="columnIndex"> the column for which to return the data class. </param>
	public virtual Type getColumnClass(int columnIndex)
	{
		if (__singleCrop)
		{
			switch (columnIndex)
			{
				case __COL_NAME:
					return typeof(string);
				case __COL_DAY_PCT:
					return typeof(Integer);
				case __COL_VALUE:
					return typeof(Double);
			}
		}
		else
		{
			switch (columnIndex)
			{
				case __COL_NAME:
					return typeof(string);
				case __COL_GDATE1:
					return typeof(Integer);
				case __COL_GDATE2:
					return typeof(Integer);
				case __COL_GDATE3:
					return typeof(Integer);
				case __COL_GDATE4:
					return typeof(Integer);
				case __COL_GDATE5:
					return typeof(Integer);
				case __COL_GDATES:
					return typeof(Integer);
				case __COL_TMOIS1:
					return typeof(Double);
				case __COL_TMOIS2:
					return typeof(Double);
				case __COL_MAD:
					return typeof(Double);
				case __COL_IRX:
					return typeof(Double);
				case __COL_FRX:
					return typeof(Double);
				case __COL_AWC:
					return typeof(Double);
				case __COL_APD:
					return typeof(Double);
				case __COL_TFLAG1:
					return typeof(Integer);
				case __COL_TFLAG2:
					return typeof(Integer);
				case __COL_CUT2:
					return typeof(Integer);
				case __COL_CUT3:
					return typeof(Integer);
			}
		}
		return typeof(string);
	}

	/// <summary>
	/// Returns the number of columns of data. </summary>
	/// <returns> the number of columns of data. </returns>
	public virtual int getColumnCount()
	{
		return __COLUMNS;
	}

	/// <summary>
	/// Returns the name of the column at the given position. </summary>
	/// <returns> the name of the column at the given position. </returns>
	public virtual string getColumnName(int columnIndex)
	{
		if (__singleCrop)
		{
			switch (columnIndex)
			{
				case __COL_NAME:
					return "\nNAME";
				case __COL_DAY_PCT:
					return "\nDAY/PCT";
				case __COL_VALUE:
					return "CROP\nCOEFFICIENT";
			}
		}
		else
		{
			switch (columnIndex)
			{
				case __COL_NAME:
					return "\n\n\nNAME";
				case __COL_GDATE1:
					return "\n\nPLANTING\nMONTH";
				case __COL_GDATE2:
					return "\n\nPLANTING\nDAY";
				case __COL_GDATE3:
					return "\n\nHARVEST\nMONTH";
				case __COL_GDATE4:
					return "\n\nHARVEST\nDAY";
				case __COL_GDATE5:
					return "\n\nDAYS TO\nFULL COVER";
				case __COL_GDATES:
					return "\n\nSEASON\nLENGTH";
				case __COL_TMOIS1:
					return "\n\nTEMP EARLY\nMOISTURE (F)";
				case __COL_TMOIS2:
					return "\n\nTEMP LATE\nMOISTURE (F)";
				case __COL_MAD:
					return "\nMANAGEMENT\nALLOWABLE\nDEFICIT LEVEL";
				case __COL_IRX:
					return "\nINITIAL ROOT\nZONE DEPTH\n(IN)";
				case __COL_FRX:
					return "\nMAXIMUM ROOT\nZONE DEPTH\n(IN)";
				case __COL_AWC:
					return "AVAILABLE\nWATER HOLDING\nCAPACITY"
						+ "\nAWC (IN)";
					goto case __COL_APD;
				case __COL_APD:
					return "\nMAXIMUM\nAPPLICATION\nDEPTH (IN)";
				case __COL_TFLAG1:
					return "\nSPRING\nFROST\nFLAG";
				case __COL_TFLAG2:
					return "\nFALL\nFROST\nFLAG";
				case __COL_CUT2:
					return "\nDAYS BETWEEN\n1ST AND 2ND\nCUT";
				case __COL_CUT3:
					return "\nDAYS BETWEEN\n2ND AND 3RD\nCUT";
			}
		}
		return "";
	}

	/// <summary>
	/// Returns the tool tips for the columns. </summary>
	/// <returns> the tool tips for the columns. </returns>
	public virtual string[] getColumnToolTips()
	{
		string[] tips = new string[__COLUMNS];
		for (int i = 0; i < __COLUMNS; i++)
		{
			tips[i] = null;
		}

		if (__singleCrop)
		{
			tips[__COL_NAME] = null;
			tips[__COL_DAY_PCT] = null;
			tips[__COL_VALUE] = null;
		}
		else
		{
			tips[__COL_NAME] = null;
			tips[__COL_GDATE1] = null;
			tips[__COL_GDATE2] = null;
			tips[__COL_GDATE3] = null;
			tips[__COL_GDATE4] = null;
			tips[__COL_GDATE5] = null;
			tips[__COL_GDATES] = null;
			tips[__COL_TMOIS1] = null;
			tips[__COL_TMOIS2] = null;
			tips[__COL_MAD] = null;
			tips[__COL_IRX] = null;
			tips[__COL_FRX] = null;
			tips[__COL_AWC] = null;
			tips[__COL_APD] = null;
			tips[__COL_TFLAG1] = "<html>Spring frost date flag"
				+ "<br>0 = mean<br>1 = 28F<br>2 = 32F</html>";
			tips[__COL_TFLAG2] = "<html>Fall frost date flag"
				+ "<br>0 = mean<br>1 = 28F<br>2 = 32F</html>";
			tips[__COL_CUT2] = "<html>Days between 1st and 2nd cutting."
				+ "<br>Alfalfa only.</html>";
			tips[__COL_CUT3] = "<html>Days between 2nd and 3rd cutting."
				+ "<br>Alfalfa only.</html>";
		}

		return tips;
	}

	/// <summary>
	/// Returns the format that the specified column should be displayed in when
	/// the table is being displayed in the given table format. </summary>
	/// <param name="column"> column for which to return the format. </param>
	/// <returns> the format (as used by StringUtil.formatString() in which to display the
	/// column. </returns>
	public virtual string getFormat(int column)
	{
		if (__singleCrop)
		{
			switch (column)
			{
				case __COL_NAME:
					return "%-40s";
				case __COL_DAY_PCT:
					return "%8d";
				case __COL_VALUE:
					return "%8.2f";
			}
		}
		else
		{
			switch (column)
			{
				case __COL_NAME:
					return "%-30.30s";
				case __COL_GDATE1:
					return "%8d";
				case __COL_GDATE2:
					return "%8d";
				case __COL_GDATE3:
					return "%8d";
				case __COL_GDATE4:
					return "%8d";
				case __COL_GDATE5:
					return "%8d";
				case __COL_GDATES:
					return "%8d";
				case __COL_TMOIS1:
					return "%8.2f";
				case __COL_TMOIS2:
					return "%8.2f";
				case __COL_MAD:
					return "%8.2f";
				case __COL_IRX:
					return "%8.2f";
				case __COL_FRX:
					return "%8.2f";
				case __COL_AWC:
					return "%8.2f";
				case __COL_APD:
					return "%8.2f";
				case __COL_TFLAG1:
					return "%8d";
				case __COL_TFLAG2:
					return "%8d";
				case __COL_CUT2:
					return "%8d";
				case __COL_CUT3:
					return "%8d";
			}
		}
		return "%-8s";
	}

	/// <summary>
	/// Returns the number of rows of data in the table.
	/// </summary>
	public virtual int getRowCount()
	{
		return _rows;
	}

	/// <summary>
	/// Returns general validators based on column of data being checked. </summary>
	/// <param name="col"> Column of data to check. </param>
	/// <returns> List of validators for a column of data. </returns>
	public virtual Validator[] getValidators(int col)
	{
		Validator[] no_checks = new Validator[] {};
		// Numbers for months must be greater than 0 and less than 13
		Validator[] month = new Validator[] {Validators.notBlankValidator(), Validators.rangeValidator(0, 13)};
		// Numbers for days must be greater than 0 and less than 32
		Validator[] day = new Validator[] {Validators.notBlankValidator(), Validators.rangeValidator(0, 32)};
		// farenheit temperatures must be greater than -1 and less
		// than  101
		Validator[] temp = new Validator[] {Validators.notBlankValidator(), Validators.rangeValidator(-1, 101)};
		// The frost date flag must be 0,1 or 2
		Validator[] frostFlag = new Validator[] {Validators.isEquals(new int?(0)), Validators.isEquals(new int?(1)), Validators.isEquals(new int?(2))};
		Validator[] frostFlagValidators = new Validator[] {Validators.notBlankValidator(), Validators.or(frostFlag)};

		if (__singleCrop)
		{
			switch (col)
			{
				case __COL_NAME:
					return StateCU_Data_TableModel_Fields.blank;
				case __COL_DAY_PCT:
					if (__blaneyCriddle == null)
					{
						return no_checks;
					}
					// TODO KAT 2007-04-12 Need to find out
					// if a different check needs to happen
					// if dayNotPercent is true or false
					if (__dayNotPercent)
					{
						return StateCU_Data_TableModel_Fields.nums;
					}
					else
					{
						return StateCU_Data_TableModel_Fields.nums;
					}
				case __COL_VALUE:
					if (__blaneyCriddle == null)
					{
						return no_checks;
					}
					// TODO KAT 2007-04-12 Need to find out
					// if a different check needs to happen
					// if dayNotPercent is true or false
					if (__dayNotPercent)
					{
						return StateCU_Data_TableModel_Fields.nums;
					}
					else
					{
						return StateCU_Data_TableModel_Fields.nums;
					}
				default:
					return no_checks;
			}
		}
		else
		{
			switch (col)
			{
				case __COL_NAME:
					return StateCU_Data_TableModel_Fields.blank;
				// TODO KAT 2007-04-12 Need to add checks for dates
				// and test date checks.  For now just check for blank.
				case __COL_GDATE1:
					return month;
				case __COL_GDATE2:
					return day;
				case __COL_GDATE3:
					return month;
				case __COL_GDATE4:
					return day;
				case __COL_GDATE5:
					return day;
				case __COL_GDATES:
					return StateCU_Data_TableModel_Fields.nums;
				case __COL_TMOIS1:
					return temp;
				case __COL_TMOIS2:
					return temp;
				case __COL_MAD:
					return StateCU_Data_TableModel_Fields.nums;
				case __COL_IRX:
					return StateCU_Data_TableModel_Fields.nums;
				case __COL_FRX:
					return StateCU_Data_TableModel_Fields.nums;
				case __COL_AWC:
					return StateCU_Data_TableModel_Fields.nums;
				case __COL_APD:
					return StateCU_Data_TableModel_Fields.nums;
				case __COL_TFLAG1:
					return frostFlagValidators;
				case __COL_TFLAG2:
					return frostFlagValidators;
				case __COL_CUT2:
					return StateCU_Data_TableModel_Fields.nums;
				case __COL_CUT3:
					return StateCU_Data_TableModel_Fields.nums;
				default:
					return no_checks;
			}
		}
	}

	/// <summary>
	/// Returns the data that should be placed in the JTable
	/// at the given row and column. </summary>
	/// <param name="row"> the row for which to return data. </param>
	/// <param name="col"> the column for which to return data. </param>
	/// <returns> the data that should be placed in the JTable at the given row and col. </returns>
	public virtual object getValueAt(int row, int col)
	{
		if (_sortOrder != null)
		{
			row = _sortOrder[row];
		}

		StateCU_CropCharacteristics crop = (StateCU_CropCharacteristics)_data.get(row);

		if (__singleCrop)
		{
			switch (col)
			{
				case __COL_NAME:
					return crop.getName();
				case __COL_DAY_PCT:
					if (__blaneyCriddle == null)
					{
						return new int?(0);
					}
					if (__dayNotPercent)
					{
						return new int?(__blaneyCriddle.getNckcp(row));
					}
					else
					{
						return new int?(__blaneyCriddle.getNckca(row));
					}
				case __COL_VALUE:
					if (__blaneyCriddle == null)
					{
						return new double?(0);
					}

					if (__dayNotPercent)
					{
						return new double?(__blaneyCriddle.getCkcp(row));
					}
					else
					{
						return new double?(__blaneyCriddle.getCkca(row));

					}
			}
		}
		else
		{
			switch (col)
			{
				case __COL_NAME:
					return crop.getName();
				case __COL_GDATE1:
					return new int?(crop.getGdate1());
				case __COL_GDATE2:
					return new int?(crop.getGdate2());
				case __COL_GDATE3:
					return new int?(crop.getGdate3());
				case __COL_GDATE4:
					return new int?(crop.getGdate4());
				case __COL_GDATE5:
					return new int?(crop.getGdate5());
				case __COL_GDATES:
					return new int?(crop.getGdates());
				case __COL_TMOIS1:
					return new double?(crop.getTmois1());
				case __COL_TMOIS2:
					return new double?(crop.getTmois2());
				case __COL_MAD:
					return new double?(crop.getMad());
				case __COL_IRX:
					return new double?(crop.getIrx());
				case __COL_FRX:
					return new double?(crop.getFrx());
				case __COL_AWC:
					return new double?(crop.getAwc());
				case __COL_APD:
					return new double?(crop.getApd());
				case __COL_TFLAG1:
					return new int?(crop.getTflg1());
				case __COL_TFLAG2:
					return new int?(crop.getTflg2());
				case __COL_CUT2:
					return new int?(crop.getCut2());
				case __COL_CUT3:
					return new int?(crop.getCut3());
			}
		}
		return "";
	}

	/// <summary>
	/// Returns an array containing the widths (in number of characters) that the 
	/// fields in the table should be sized to. </summary>
	/// <returns> an integer array containing the widths for each field. </returns>
	public virtual int[] getColumnWidths()
	{
		int[] widths = new int[__COLUMNS];
		for (int i = 0; i < __COLUMNS; i++)
		{
			widths[i] = 0;
		}

		if (__singleCrop)
		{
			widths[__COL_NAME] = 20;
			widths[__COL_DAY_PCT] = 11;
			widths[__COL_VALUE] = 16;
		}
		else
		{
			widths[__COL_NAME] = 15;
			widths[__COL_GDATE1] = 6;
			widths[__COL_GDATE2] = 6;
			widths[__COL_GDATE3] = 6;
			widths[__COL_GDATE4] = 6;
			widths[__COL_GDATE5] = 8;
			widths[__COL_GDATES] = 6;
			widths[__COL_TMOIS1] = 9;
			widths[__COL_TMOIS2] = 9;
			widths[__COL_MAD] = 9;
			widths[__COL_IRX] = 9;
			widths[__COL_FRX] = 11;
			widths[__COL_AWC] = 11;
			widths[__COL_APD] = 9;
			widths[__COL_TFLAG1] = 6;
			widths[__COL_TFLAG2] = 4;
			widths[__COL_CUT2] = 10;
			widths[__COL_CUT3] = 10;
		}

		return widths;
	}

	/// <summary>
	/// Returns whether the cell is editable or not.  In this model, all the cells in
	/// columns 3 and greater are editable. </summary>
	/// <param name="rowIndex"> unused. </param>
	/// <param name="columnIndex"> the index of the column to check whether it is editable
	/// or not. </param>
	/// <returns> whether the cell is editable or not. </returns>
	public virtual bool isCellEditable(int rowIndex, int columnIndex)
	{
		if (!__editable)
		{
			return false;
		}
		return true;
	}

	/// <summary>
	/// Inserts the specified value into the table at the given position. </summary>
	/// <param name="value"> the object to store in the table cell. </param>
	/// <param name="row"> the row of the cell in which to place the object. </param>
	/// <param name="col"> the column of the cell in which to place the object. </param>
	public virtual void setValueAt(object value, int row, int col)
	{

		switch (col)
		{
		}

		base.setValueAt(value, row, col);
	}

	/// <summary>
	/// Sets the parent well under which the right and return flow data is stored. </summary>
	/// <param name="parent"> the parent well. </param>
	public virtual void setParentCropCharacteristics(StateCU_CropCharacteristics parent)
	{
		// TODO SAM 2007-03-01 Evaluate use
		//__parentCrop = parent;
	}

	public virtual void setBlaneyCriddle(StateCU_BlaneyCriddle bc)
	{
		__blaneyCriddle = bc;

		if (bc == null)
		{
			_rows = 0;
			fireTableDataChanged();
			return;
		}

		string flag = bc.getFlag();
		if (flag.Equals("Day", StringComparison.OrdinalIgnoreCase))
		{
			__dayNotPercent = true;
			_rows = 25;
		}
		else
		{
			__dayNotPercent = false;
			_rows = 21;
		}
		fireTableDataChanged();
	}

	}

}