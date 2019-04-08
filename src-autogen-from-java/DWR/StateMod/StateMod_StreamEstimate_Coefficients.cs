using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

// StateMod_StreamEstimate_Coefficients - class to store stream estimate station coefficient information

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
// StateMod_StreamEstimate_Coefficients - class to store stream estimate
//				station coefficient information
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
// 
// 02 Sep 1997	Catherine E.		Created initial version of class.
//		Nutting-Lane, RTi
// 11 Feb 1998	CEN, RTi		Added _dataset.setDirty
//					to all set
//					routines.
// 11 Mar 1998	CEN, RTi		Changed class to extend from Object
//					(default)to StateMod_Data.  Removed utm
//					abilities(use StateMod_Data's).
// 04 Aug 1998	Steven A. Malers, RTi	Overload some of the routines for use in
//					DMIs.  Add SMLocateBaseNode as per
//					legacy code.
// 21 Dec 1998	CEN, RTi		Added throws IOException to read/write
//					routines.
// 18 Feb 2001	SAM, RTi		Code review.  Clean up javadoc.  Handle
//					nulls.  Set unused variables to null.
//					Add finalize.  Alphabetize methods.
//					Change IO to IOUtil.  Change so Vectors
//					are initialize sized to 5 rather than
//					15.  Update SMLocateBaseNode to use
//					SMUtil for search.
// 02 May 2001	SAM, RTi		Track down problem with warning reading
//					baseflow.  Second line of read was
//					trimming first, which caused the fixed
//					read to have problems.  Not sure how
//					the problem got introduced.
// 2001-12-27	SAM, RTi		Update to use new fixedRead()to
//					improve performance.
// 2002-09-12	SAM, RTi		Remove baseflow time series code from
//					here and move to the SMRiverInfo class,
//					to handle the time series associated
//					with .ris nodes.
// 2002-09-19	SAM, RTi		Use isDirty() instead of setDirty() to
//					indicate edits.
//------------------------------------------------------------------------------
// 2003-06-04	J. Thomas Sapienza, RTI	Renamed from SMRivBaseflows
// 2003-06-10	JTS, RTi		* Folded dumpRiverBaseFlowsFile() into
//					  writeRiverBaseFlowsFile()
//					* Renamed parseRiverBaseFlowsFile() to
//					  readRiverBaseFlowsFile()
// 2003-06-23	JTS, RTi		Renamed writeRiverBaseFlowsFile() to
//					writeStateModFile()
// 2003-06-26	JTS, RTi		Renamed readRiverBaseFlowsFile() to
//					readStateModFile()
// 2003-07-15	JTS, RTi		Changed to use new dataset design.
// 2003-08-03	SAM, RTi		* Rename class from StateMod_BaseFlows
//					  to StateMod_BaseFlowCoefficients.
//					* Changed isDirty() back to setDirty().
//					* locateIndexFromID() is now in
//					  StateMod_Util.
// 2003-09-11	SAM, RTi		Rename class from
//					StateMod_BaseFlowCoefficents to
//					StateMod_StreamEstimate_Coefficients.
// 2003-10-16	SAM, RTi		Remove description since it is not
//					used in the file and the base class
//					name could be used if needed.
// 2004-03-15	JTS, RTi		Put description back in as it is now
//					used by the makenet code in 
//					HydroBase_NodeNetwork.
// 2004-07-12	JTS, RTi		Removed description one more time.
// 2004-07-14	JTS, RTi		* Added acceptChanges().
//					* Added changed().
//					* Added clone().
//					* Added compareTo().
//					* Added createBackup().
//					* Added restoreOriginal().
//					* Now implements Cloneable.
//					* Now implements Comparable.
//					* Clone status is checked via _isClone
//					  when the component is marked as dirty.
// 2004-08-13	SAM, RTi		* When writing, adjust file names for
//					  working directory.
//					* Handle null _dataset, for use with
//					  StateDMI.
//					* For setM() and setN(), if the size is
//					  set to zero, clear out the data
//					  vectors - this is used with the
//					  StateDMI set command.
// 2005-04-18	JTS, RTi		Added writeListFile().
// 2007-04-12	Kurt Tometich, RTi		Added checkComponentData() and
//									getDataHeader() methods for check
//									file and data check support.
//------------------------------------------------------------------------------
// EndHeader

namespace DWR.StateMod
{

	using DataSetComponent = RTi.Util.IO.DataSetComponent;
	using IOUtil = RTi.Util.IO.IOUtil;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;

	// FIXME SAM 2009-01-11 Need to remove N and M data members - they are redundant with the size of the
	// lists, which can lead to inconsistencies.

	public class StateMod_StreamEstimate_Coefficients : StateMod_Data, ICloneable, IComparable<StateMod_Data>, StateMod_ComponentValidator
	{

	// TODO SAM 2009-01-05 Why is this needed?  Apparently for the table model for setting data?
	public const int MAX_BASEFLOWS = 15;

	/// <summary>
	/// Node where flow is to be estimated.
	/// </summary>
	protected internal string _flowX;
	/// <summary>
	/// Number of stations upstream X.
	/// </summary>
	protected internal int _N;
	/// <summary>
	/// Factors to weight the gaged flow.
	/// </summary>
	protected internal IList<double?> _coefn;
	/// <summary>
	/// Station ids upstream X.
	/// </summary>
	protected internal IList<string> _upper;
	/// <summary>
	/// Factor to distribute the gain.
	/// </summary>
	protected internal double _proratnf;
	/// <summary>
	/// Number of stations used to calculate the gain.
	/// </summary>
	protected internal int _M;
	/// <summary>
	/// Factors to weight the flow for gain.
	/// </summary>
	protected internal IList<double?> _coefm;
	/// <summary>
	/// Station ids upstream X
	/// </summary>
	protected internal IList<string> _flowm;

	/// <summary>
	/// Constructor.
	/// </summary>
	public StateMod_StreamEstimate_Coefficients() : base()
	{
		initialize();
	}

	/// <summary>
	/// Accepts any changes made inside of a GUI to this object.
	/// </summary>
	public virtual void acceptChanges()
	{
		_isClone = false;
		_original = null;
	}

	public virtual void addCoefm(double d)
	{
		addCoefm(new double?(d));
	}

	public virtual void addCoefm(double? d)
	{
		_coefm.Add(d);
		int size = _coefm.Count;
		if (size > _M)
		{
			_M = size;
		}
		if (!_isClone)
		{
			if (_dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_STREAMESTIMATE_COEFFICIENTS, true);
			}
		}
	}

	public virtual void addCoefn(double d)
	{
		addCoefn(new double?(d));
	}

	public virtual void addCoefn(double? d)
	{
		_coefn.Add(d);
		int size = _coefn.Count;
		if (size > _N)
		{
			_N = size;
		}
		if (!_isClone)
		{
			if (_dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_STREAMESTIMATE_COEFFICIENTS, true);
			}
		}
	}

	public virtual void addUpper(string s)
	{
		if (!string.ReferenceEquals(s, null))
		{
			_upper.Add(s.Trim());
			int size = _upper.Count;
			if (size > _N)
			{
				_N = size;
			}
			if (!_isClone)
			{
				if (_dataset != null)
				{
					_dataset.setDirty(StateMod_DataSet.COMP_STREAMESTIMATE_COEFFICIENTS,true);
				}
			}
		}
	}

	/// <summary>
	/// Compares this object with its original value (generated by createBackup() upon
	/// entering a GUI) to see if it has changed.
	/// </summary>
	public virtual bool changed()
	{
		if (_original == null)
		{
			return true;
		}
		if (compareTo(_original) == 0)
		{
			return false;
		}
		return true;
	}

	/// <summary>
	/// Clones the data object. </summary>
	/// <returns> a cloned object. </returns>
	public override object clone()
	{
		StateMod_StreamEstimate_Coefficients c = (StateMod_StreamEstimate_Coefficients)base.clone();
		c._isClone = true;
		// Copy contents of lists...
		c._coefn = new List<double?>(_coefn.Count);
		for (int i = 0; i < _coefn.Count; i++)
		{
			c._coefn.Add(new double?(_coefn[i].Value));
		}
		c._upper = new List<string>(_upper.Count);
		for (int i = 0; i < _upper.Count; i++)
		{
			c._upper.Add(_upper[i]);
		}
		c._coefm = new List<double?>(_coefm.Count);
		for (int i = 0; i < _coefm.Count; i++)
		{
			c._coefm.Add(new double?(_coefm[i].Value));
		}
		c._flowm = new List<string>(_flowm.Count);
		for (int i = 0; i < _flowm.Count; i++)
		{
			c._flowm.Add(_flowm[i]);
		}
		return c;
	}

	/// <summary>
	/// Compares this object to another StateMod_StreamEstimate_Coefficients object. </summary>
	/// <param name="data"> the object to compare against. </param>
	/// <returns> 0 if they are the same, 1 if this object is greater than the other
	/// object, or -1 if it is less. </returns>
	public virtual int CompareTo(StateMod_Data data)
	{
		int res = base.CompareTo(data);
		if (res != 0)
		{
			return res;
		}

		StateMod_StreamEstimate_Coefficients c = (StateMod_StreamEstimate_Coefficients)data;

		res = _flowX.CompareTo(c._flowX);
		if (res != 0)
		{
			return res;
		}

		if (_N < c._N)
		{
			return -1;
		}
		else if (_N > c._N)
		{
			return 1;
		}

		if (_proratnf < c._proratnf)
		{
			return -1;
		}
		else if (_proratnf > c._proratnf)
		{
			return 1;
		}

		if (_M < c._M)
		{
			return -1;
		}
		else if (_M > c._M)
		{
			return 1;
		}

		int size1 = 0;
		int size2 = 0;
		double d1 = 0;
		double d2 = 0;
		string s1 = null;
		string s2 = null;

		if (_coefn == null && c._coefn != null)
		{
			return -1;
		}
		else if (_coefn != null && c._coefn == null)
		{
			return 1;
		}

		size1 = _coefn.Count;
		size2 = c._coefn.Count;

		if (size1 < size2)
		{
			return -1;
		}
		else if (size2 > size1)
		{
			return 1;
		}
		else
		{
			for (int i = 0; i < size1; i++)
			{
				d1 = ((double?)_coefn[i]).Value;
				d2 = ((double?)c._coefn[i]).Value;
				if (d1 < d2)
				{
					return -1;
				}
				else if (d1 > d2)
				{
					return 1;
				}
			}
		}

		if (_upper == null && c._upper != null)
		{
			return -1;
		}
		else if (_upper != null && c._upper == null)
		{
			return 1;
		}

		size1 = _upper.Count;
		size2 = c._upper.Count;

		if (size1 < size2)
		{
			return -1;
		}
		else if (size2 > size1)
		{
			return 1;
		}
		else
		{
			for (int i = 0; i < size1; i++)
			{
				s1 = (string)_upper[i];
				s2 = (string)c._upper[i];
				res = s1.CompareTo(s2);
				if (res != 0)
				{
					return res;
				}
			}
		}

		if (_coefm == null && c._coefm != null)
		{
			return -1;
		}
		else if (_coefm != null && c._coefm == null)
		{
			return 1;
		}

		size1 = _coefm.Count;
		size2 = c._coefm.Count;

		if (size1 < size2)
		{
			return -1;
		}
		else if (size2 > size1)
		{
			return 1;
		}
		else
		{
			for (int i = 0; i < size1; i++)
			{
				d1 = ((double?)_coefm[i]).Value;
				d2 = ((double?)c._coefm[i]).Value;
				if (d1 < d2)
				{
					return -1;
				}
				else if (d1 > d2)
				{
					return 1;
				}
			}
		}


		if (_flowm == null && c._flowm != null)
		{
			return -1;
		}
		else if (_flowm != null && c._flowm == null)
		{
			return 1;
		}

		size1 = _flowm.Count;
		size2 = c._flowm.Count;

		if (size1 < size2)
		{
			return -1;
		}
		else if (size2 > size1)
		{
			return 1;
		}
		else
		{
			for (int i = 0; i < size1; i++)
			{
				s1 = (string)_flowm[i];
				s2 = (string)c._flowm[i];
				res = s1.CompareTo(s2);
				if (res != 0)
				{
					return res;
				}
			}
		}

		return 0;
	}

	/// <summary>
	/// Creates a copy of the object for later use in checking to see if it was changed in a GUI.
	/// </summary>
	public virtual void createBackup()
	{
		_original = (StateMod_StreamEstimate_Coefficients)clone();
		((StateMod_StreamEstimate_Coefficients)_original)._isClone = false;
		_isClone = true;
	}

	/// <summary>
	/// Clean up before garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~StateMod_StreamEstimate_Coefficients()
	{
		_flowX = null;
		_coefn = null;
		_upper = null;
		_coefm = null;
		_flowm = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Retrieve the coefm corresponding to a particular index.
	/// </summary>
	public virtual double getCoefm(int index)
	{
		return ((double?)_coefm[index]).Value;
	}

	/// <summary>
	/// Retrieve the coefm corresponding to a particular index.
	/// </summary>
	public virtual IList<double?> getCoefm()
	{
		return _coefm;
	}

	/// <summary>
	/// Return the coefn corresponding to a particular index.
	/// </summary>
	public virtual double getCoefn(int index)
	{
		return _coefn[index].Value;
	}

	/// <summary>
	/// Return the coefn corresponding to a particular index.
	/// </summary>
	public virtual IList<double?> getCoefn()
	{
		return _coefn;
	}

	/// <summary>
	/// Returns the data column header for the specifically checked data. </summary>
	/// <returns> Data column header. </returns>
	public static string[] getDataHeader()
	{
		// TODO KAT 2007-04-16 When specific checks are added to checkComponentData
		// return the header for that data here
		return new string[] {};
	}

	/// <summary>
	/// Return the upper id corresponding to a particular index.
	/// </summary>
	public virtual string getFlowm(int index)
	{
		return (string)_flowm[index];
	}

	/// <summary>
	/// Return the upper id corresponding to a particular index.
	/// </summary>
	public virtual IList<string> getFlowm()
	{
		return _flowm;
	}

	/// <summary>
	/// Return the node where flow is to be estimated.
	/// </summary>
	public virtual string getFlowX()
	{
		return _flowX;
	}

	/// <summary>
	/// Return the number of stations used to calc the gain.
	/// </summary>
	public virtual int getM()
	{
		return _M;
	}

	/// <summary>
	/// Retrieve the number of stations upstream X.
	/// </summary>
	public virtual int getN()
	{
		return _N;
	}

	/// <summary>
	/// Return the factor to distribute the gain.
	/// </summary>
	public virtual double getProratnf()
	{
		return _proratnf;
	}

	/// <summary>
	/// Return the upper id corresponding to a particular index.
	/// </summary>
	public virtual string getUpper(int index)
	{
		return (string)_upper[index];
	}

	/// <summary>
	/// Return the upper id corresponding to a particular index.
	/// </summary>
	public virtual IList<string> getUpper()
	{
		return _upper;
	}

	/// <summary>
	/// Initialize member variables.
	/// </summary>
	private void initialize()
	{
		_smdata_type = StateMod_DataSet.COMP_STREAMESTIMATE_COEFFICIENTS;
		_flowX = "";
		_N = 0;
		_M = 0;
		_coefn = new List<double?>();
		_upper = new List<string>();
		_coefm = new List<double?>();
		_flowm = new List<string>();
		_proratnf = 0;
	}

	/// <summary>
	/// Cancels any changes made to this object within a GUI since createBackup()
	/// was called and sets _original to null.
	/// </summary>
	public override void restoreOriginal()
	{
		StateMod_StreamEstimate_Coefficients c = (StateMod_StreamEstimate_Coefficients)_original;
		base.restoreOriginal();
		_flowX = c._flowX;
		_N = c._N;
		_proratnf = c._proratnf;
		_M = c._M;
		_isClone = false;
		_original = null;
	}

	/// <summary>
	/// Add factor to vector of factors to weight the flow for gain.
	/// </summary>
	public virtual void setCoefm(int index, string str)
	{
		if (!string.ReferenceEquals(str, null))
		{
			setCoefm(index, StringUtil.atod(str.Trim()));
		}
	}

	/// <summary>
	/// Add factor to vector of factors to weight the flow for gain.
	/// </summary>
	public virtual void setCoefm(int index, double str)
	{
		if (_coefm.Count < index + 1)
		{
			addCoefm(new double?(str));
		}
		else
		{
			_coefm[index] = new double?(str);
			if (!_isClone)
			{
				if (_dataset != null)
				{
					_dataset.setDirty(StateMod_DataSet.COMP_STREAMESTIMATE_COEFFICIENTS,true);
				}
			}
		}
	}

	/// <summary>
	/// Add factor to vector of factors to weight the gaged flow to vector (coefn).
	/// </summary>
	public virtual void setCoefn(int index, string str)
	{
		if (!string.ReferenceEquals(str, null))
		{
			setCoefn(index, StringUtil.atod(str.Trim()));
		}
	}

	/// <summary>
	/// Main version.
	/// </summary>
	public virtual void setCoefn(int index, double str)
	{
		if (_coefn.Count < index + 1)
		{
			addCoefn(new double?(str));
		}
		else
		{
			_coefn[index] = new double?(str);
			if (!_isClone)
			{
				if (_dataset != null)
				{
					_dataset.setDirty(StateMod_DataSet.COMP_STREAMESTIMATE_COEFFICIENTS,true);
				}
			}
		}
	}

	/// <summary>
	/// Set the node where flow is to be estimated.
	/// </summary>
	public virtual void setFlowX(string s)
	{
		if (!string.ReferenceEquals(s, null))
		{
			if (!s.Equals(_flowX))
			{
				if (!_isClone)
				{
					if (_dataset != null)
					{
						_dataset.setDirty(StateMod_DataSet.COMP_STREAMESTIMATE_COEFFICIENTS,true);
					}
				}
				_flowX = s;
				setID(s);
			}
		}
	}

	/// <summary>
	/// Add id to vector of station ids upstream X.
	/// </summary>
	public virtual void setFlowm(int index, string str)
	{
		if (!string.ReferenceEquals(str, null))
		{
			if (_flowm.Count < index + 1)
			{
				addFlowm(str.Trim());
			}
			else
			{
				_flowm[index] = str.Trim();
				if (!_isClone)
				{
					if (_dataset != null)
					{
						_dataset.setDirty(StateMod_DataSet.COMP_STREAMESTIMATE_COEFFICIENTS,true);
					}
				}
			}
		}
	}

	/// <summary>
	/// Add id to vector of station ids upstream X.
	/// </summary>
	public virtual void addFlowm(string s)
	{
		if (!string.ReferenceEquals(s, null))
		{
			_flowm.Add(s.Trim());
			int size = _flowm.Count;
			if (size > _M)
			{
				_M = size;
			}
			if (!_isClone)
			{
				if (_dataset != null)
				{
					_dataset.setDirty(StateMod_DataSet.COMP_STREAMESTIMATE_COEFFICIENTS, true);
				}
			}
		}
	}

	/// <summary>
	/// Set the number of stations used to calc the gain.
	/// </summary>
	public virtual void setM(int i)
	{
		if (i != _M)
		{
			if (!_isClone)
			{
				if (_dataset != null)
				{
					_dataset.setDirty(StateMod_DataSet.COMP_STREAMESTIMATE_COEFFICIENTS, true);
				}
			}
			_M = i;
			if (i == 0)
			{
				// Clear vector...
				_coefm.Clear();
				_flowm.Clear();
			}
		}
	}

	/// <summary>
	/// Set the number of stations used to calculate the gain.
	/// </summary>
	public virtual void setM(int? i)
	{
		setM(i.Value);
	}

	/// <summary>
	/// Set the number of stations used to calculate the gain.
	/// </summary>
	public virtual void setM(string str)
	{
		if (!string.ReferenceEquals(str, null))
		{
			setM(StringUtil.atoi(str.Trim()));
		}
	}

	/// <summary>
	/// Set the number of stations upstream X.
	/// </summary>
	public virtual void setN(int i)
	{
		if (i != _N)
		{
			if (!_isClone)
			{
				if (_dataset != null)
				{
					_dataset.setDirty(StateMod_DataSet.COMP_STREAMESTIMATE_COEFFICIENTS, true);
				}
			}
			_N = i;
			if (i == 0)
			{
				// Clear vector...
				_coefn.Clear();
				_upper.Clear();
			}
		}
	}

	/// <summary>
	/// Set the number of stations upstream X.
	/// </summary>
	public virtual void setN(int? i)
	{
		setN(i.Value);
	}

	/// <summary>
	/// Set the number of stations upstream X.
	/// </summary>
	public virtual void setN(string str)
	{
		if (!string.ReferenceEquals(str, null))
		{
			setN(StringUtil.atoi(str.Trim()));
		}
	}

	/// <summary>
	/// Set the initial storage of owner.
	/// </summary>
	public virtual void setProratnf(double d)
	{
		if (d != _proratnf)
		{
			if (!_isClone)
			{
				if (_dataset != null)
				{
					_dataset.setDirty(StateMod_DataSet.COMP_STREAMESTIMATE_COEFFICIENTS,true);
				}
			}
			_proratnf = d;
		}
	}

	/// <summary>
	/// Set the initial storage of owner.
	/// </summary>
	public virtual void setProratnf(double? d)
	{
		setProratnf(d.Value);
	}

	/// <summary>
	/// Set the initial storage of owner.
	/// </summary>
	public virtual void setProratnf(string str)
	{
		if (!string.ReferenceEquals(str, null))
		{
			setProratnf(StringUtil.atod(str.Trim()));
		}
	}

	/// <summary>
	/// Add id to vector of station ids upstream X.
	/// </summary>
	public virtual void setUpper(int index, string str)
	{
		if (!string.ReferenceEquals(str, null))
		{
			if (_upper.Count < index + 1)
			{
				addUpper(str.Trim());
			}
			else
			{
				_upper[index] = str.Trim();
				if (!_isClone)
				{
					if (_dataset != null)
					{
						_dataset.setDirty(StateMod_DataSet.COMP_STREAMESTIMATE_COEFFICIENTS, true);
					}
				}
			}
		}
	}

	/// <returns> A in instance of StateMod_StreamEstimate_Coefficients from a vector of
	/// the same, or null if not found. </returns>
	/// <param name="baseflow"> Vector of StateMod_BaseFlowCoefficients data. </param>
	/// <param name="id"> Baseflow node identifier to locate. </param>
	public static StateMod_StreamEstimate_Coefficients locateBaseNode(IList<StateMod_StreamEstimate_Coefficients> baseflow, string id)
	{
		int index = StateMod_Util.locateIndexFromID(id, baseflow);
		if (index < 0)
		{
			return null;
		}
		return baseflow[index];
	}

	/// <summary>
	/// Read stream estimate coefficients and store in a list. </summary>
	/// <param name="filename"> Name of file to read. </param>
	/// <returns> list of streamflow estimate coefficients data </returns>
	/// <exception cref="Exception"> if there is an error reading the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.List<StateMod_StreamEstimate_Coefficients> readStateModFile(String filename) throws Exception
	public static IList<StateMod_StreamEstimate_Coefficients> readStateModFile(string filename)
	{
		string routine = "StateMod_StreamEstimate_Coefficients.readStateModFile";
		IList<StateMod_StreamEstimate_Coefficients> theBaseflows = new List<StateMod_StreamEstimate_Coefficients>();

		string iline = null;
		IList<object> v = new List<object>(2); // used to retrieve from fixedRead
		string adnl = null;
		int[] format_0 = new int[] {StringUtil.TYPE_STRING, StringUtil.TYPE_SPACE, StringUtil.TYPE_INTEGER};
		int[] format_0w = new int[] {12, 8, 8};
		int[] format_1 = new int[] {StringUtil.TYPE_DOUBLE, StringUtil.TYPE_SPACE, StringUtil.TYPE_STRING};
		int[] format_1w = new int[] {8, 1, 12};
		int[] format_2 = new int[] {StringUtil.TYPE_SPACE, StringUtil.TYPE_DOUBLE, StringUtil.TYPE_INTEGER};
		int[] format_2w = new int[] {12, 8, 8};
		int[] format_3 = new int[] {StringUtil.TYPE_DOUBLE, StringUtil.TYPE_SPACE, StringUtil.TYPE_STRING};
		int[] format_3w = new int[] {8, 1, 12};
		StreamReader @in = null;
		StateMod_StreamEstimate_Coefficients aBaseflow = null;
		int i = 0;
		int num_adnl;
		int begin_pos;
		int end_pos;
		int linecount = 0;

		if (Message.isDebugOn)
		{
			Message.printDebug(30, routine, "Reading file :" + filename);
		}

		try
		{
			@in = new StreamReader(filename);
			while (!string.ReferenceEquals((iline = @in.ReadLine()), null))
			{
				++linecount;
				// check for comments
				if (iline.StartsWith("#", StringComparison.Ordinal) || iline.Trim().Length == 0)
				{
					continue;
				}
				// allocate a new baseflow
				aBaseflow = new StateMod_StreamEstimate_Coefficients();

				// read in first of two lines for each baseflow
				StringUtil.fixedRead(iline, format_0, format_0w, v);
				aBaseflow.setFlowX(((string)v[0]).Trim());
				aBaseflow.setN((int?)v[1]);

				num_adnl = aBaseflow.getN();
				for (i = 0; i < num_adnl; i++)
				{
					// calculate begin_pos and end_pos
					//	 8(factor to weight the gaged flow)
					// +	 1(space)
					// +	12(station id upstream X)
					// --------
					//	21(for each set of num_adnl
					// +	28(array index after initial info)
					// +	 1(we should start on next position)
					// -	 1(our string starts with index 0)
					// 	
					begin_pos = 28 + (i * 21);
					end_pos = begin_pos + 21;
					adnl = iline.Substring(begin_pos, end_pos - begin_pos);
					StringUtil.fixedRead(adnl, format_1, format_1w, v);
					aBaseflow.addCoefn((double?)v[0]);
					aBaseflow.addUpper((string)v[1]);
				}

				// read in second of two lines for each baseflow
				iline = @in.ReadLine();
				StringUtil.fixedRead(iline, format_2, format_2w, v);
				aBaseflow.setProratnf((double?)v[0]);
				aBaseflow.setM((int?)v[1]);

				num_adnl = aBaseflow.getM();
				for (i = 0; i < num_adnl; i++)
				{
					begin_pos = 28 + (i * 21);
					end_pos = begin_pos + 21;
					adnl = iline.Substring(begin_pos, end_pos - begin_pos);
					StringUtil.fixedRead(adnl, format_3, format_3w, v);
					aBaseflow.addCoefm((double?)v[0]);
					aBaseflow.addFlowm((string)v[1]);
				}

				// add the baseflow to the vector of baseflows
				theBaseflows.Add(aBaseflow);
			}
		}
		catch (Exception e)
		{
			Message.printWarning(3, routine, "Error reading near line " + linecount);
			Message.printWarning(3, routine, e);
			throw e;
		}
		finally
		{
			if (@in != null)
			{
				@in.Close();
			}
			@in = null;
		}
		return theBaseflows;
	}

	/// <param name="dataset"> StateMod dataset object. </param>
	/// <returns> Validation results. </returns>
	public virtual StateMod_ComponentValidation validateComponent(StateMod_DataSet dataset)
	{
		StateMod_ComponentValidation validation = new StateMod_ComponentValidation();
		string id = getID();
		// Make sure that basic information is not empty
		if (StateMod_Util.isMissing(id))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Stream estimate station identifier is blank.", "Specify a station identifier."));
		}
		IList<StateMod_RiverNetworkNode> rinList = null;
		if (dataset != null)
		{
			DataSetComponent comp = dataset.getComponentForComponentType(StateMod_DataSet.COMP_RIVER_NETWORK);
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<StateMod_RiverNetworkNode> rinList0 = (java.util.List<StateMod_RiverNetworkNode>)comp.getData();
			IList<StateMod_RiverNetworkNode> rinList0 = (IList<StateMod_RiverNetworkNode>)comp.getData();
			rinList = rinList0;
		}

		double coefn;
		string upper;
		for (int j = 0; j < getN(); j++)
		{
			coefn = getCoefn(j);
			if (!((coefn >= 0.0) && (coefn <= 1.0)))
			{
				validation.add(new StateMod_ComponentValidationProblem(this,"Stream estimate \"" + id + "\" estimate coefficient (" + StringUtil.formatString(coefn,"%.3f") + ") is out of normal range 0 to 1.0 (limits may vary).", "Verify the area and precipitation information for subareas used in coefficient calculations."));
			}
			upper = getUpper(j);
			// Make sure that node is in the network.
			if ((rinList != null) && (rinList.Count > 0))
			{
				if (StateMod_Util.IndexOf(rinList, upper) < 0)
				{
					validation.add(new StateMod_ComponentValidationProblem(this,"Stream estimate station \"" + id + "\" estimate station ID (" + upper + ") is not found in the list of river network nodes.", "Verify that stream estimate coefficient file is consistent with network information."));
				}
			}
		}

		double proratnf = getProratnf();
		if (!((proratnf >= 0.0) && (proratnf <= 1.5)))
		{
			validation.add(new StateMod_ComponentValidationProblem(this,"Stream estimate \"" + id + "\" proration factor (" + StringUtil.formatString(proratnf,"%.3f") + ") is out of normal range 0 to 1.5 (limits may vary).", "Verify the area and precipitation information for subareas used in coefficient calculations."));
		}
		double coefm;
		string flowm;
		for (int j = 0; j < getM(); j++)
		{
			coefm = getCoefm(j);
			if (!((coefm >= -1.5) && (coefm <= 1.5)))
			{
				validation.add(new StateMod_ComponentValidationProblem(this,"Stream estimate \"" + id + "\" gain coefficient (" + StringUtil.formatString(coefm,"%.3f") + ") is out of normal range -1.5 to 1.5 (limits may vary).", "Verify the area and precipitation information for subareas used in coefficient calculations."));
			}
			flowm = getFlowm(j);
			// Make sure that node is in the network.
			if ((rinList != null) && (rinList.Count > 0))
			{
				if (StateMod_Util.IndexOf(rinList, flowm) < 0)
				{
					validation.add(new StateMod_ComponentValidationProblem(this,"Stream estimate station \"" + id + "\" gain station ID (" + flowm + ") is not found in the list of river network nodes.", "Verify that stream estimate coefficient file is consistent with network information."));
				}
			}
		}
		return validation;
	}

	/// <summary>
	/// Write stream estimate station coefficients to output.  History header
	/// information is also maintained by calling this routine. </summary>
	/// <param name="infile"> input file from which previous history should be taken </param>
	/// <param name="outfile"> output file to write </param>
	/// <param name="theBaseflows"> list of baseflow coefficients to print </param>
	/// <param name="newComments"> addition comments which should be included in history </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeStateModFile(String infile, String outfile, java.util.List<StateMod_StreamEstimate_Coefficients> theBaseflows, java.util.List<String> newComments) throws Exception
	public static void writeStateModFile(string infile, string outfile, IList<StateMod_StreamEstimate_Coefficients> theBaseflows, IList<string> newComments)
	{
		string routine = "StateMod_StreamEstimate_Coefficients.writeStateModFile";
		IList<string> commentIndicators = new List<string>(1);
		commentIndicators.Add("#");
		IList<string> ignoredCommentIndicators = new List<string>(1);
		ignoredCommentIndicators.Add("#>");
		PrintWriter @out = null;

		Message.printStatus(2, routine, "Writing stream estimate station coefficients to file \"" + outfile + "\" using \"" + infile + "\" header...");

		try
		{
			@out = IOUtil.processFileHeaders(IOUtil.getPathUsingWorkingDir(infile), IOUtil.getPathUsingWorkingDir(outfile), newComments, commentIndicators, ignoredCommentIndicators, 0);

			string cmnt = "#>";
			string iline = null;
			StateMod_StreamEstimate_Coefficients bf = null;
			string format_1 = "%-12.12s        %8d";
			string format_2 = "%8.3f %-12.12s";
			string format_3 = "            %8.3f%8d";
			IList<object> v = new List<object>(2);

			@out.println(cmnt + "---------------------------------------------------------------------------");
			@out.println(cmnt + "  StateMod Stream Estimate Station Coefficient Data");
			@out.println(cmnt);
			@out.println(cmnt + "  FlowX = (FlowB(1)*coefB(1) + FlowG(2)*coefB(2) + ...)+");
			@out.println(cmnt + "          pf * (FlowG(1)*coefG(1) + FlowG(2)*coefG(2) + ...)+");
			@out.println(cmnt);
			@out.println(cmnt + "  where:");
			@out.println(cmnt);
			@out.println(cmnt + "  FlowX = Flow at intermediate node to be estimated.");
			@out.println(cmnt + "  FlowB =   Estimate flow station(s).");
			@out.println(cmnt + "  FlowG =   Gain flow station(s).");
			@out.println(cmnt);
			@out.println(cmnt + "     pf = Proration factor for gain term.");
			@out.println(cmnt + "  coefB =   Estimate flow coefficient.");
			@out.println(cmnt + "  coefG =   Gain flow coefficient.");
			@out.println(cmnt);
			@out.println(cmnt + "  Card 1 format (a12, 8x, i8, 10(f8.3,1x,a12)");
			@out.println(cmnt);
			@out.println(cmnt + "       FlowX:  Node where flow is to be estimated");
			@out.println(cmnt + "       Mbase:  Number of base stations to follow");
			@out.println(cmnt + "       coefB:  Estimate flow coefficient");
			@out.println(cmnt + "       FlowB:  Estimate station ID");
			@out.println(cmnt);
			@out.println(cmnt + "  Card 2 format (12x, f8.2, i8, 10(f8.3,1x,a12)");
			@out.println(cmnt);
			@out.println(cmnt + "          pf:  Proration factor for gain term.");
			@out.println(cmnt + "       nbase:  Number of gain stations to follow");
			@out.println(cmnt + "       coefG:  Gain flow coefficient.");
			@out.println(cmnt + "       FlowG:  Gaged flow stations used to calculate gain");
			@out.println(cmnt);
			@out.println(cmnt + " FlowX              mbase   coefB1    FlowB1    coefB2    FlowB2    coefB3   FlowB3      coefB3    FlowB4     ...");
			@out.println(cmnt + "---------exxxxxxxxb------eb------exb----------eb------exb----------eb------exb----------eb------exb----------e ...");
			@out.println(cmnt + "             pf     nbase   coefG1   FlowG1     coefG2    FlowG2     coefG3    FlowG3     coefG4    FlowG4     ...");
			@out.println(cmnt + "xxxxxxxxxxb------eb------eb------exb----------eb------exb----------eb------exb----------eb------exb----------e ...");
			@out.println(cmnt);
			@out.println(cmnt + "EndHeader");
			@out.println(cmnt);

			int num = 0;
			if (theBaseflows != null)
			{
				num = theBaseflows.Count;
			}
			for (int i = 0; i < num; i++)
			{
				bf = theBaseflows[i];
				if (bf == null)
				{
					continue;
				}

				// 1st line
				v.Clear();
				v.Add(bf.getFlowX());
				v.Add(new int?(bf.getN()));
				iline = StringUtil.formatString(v, format_1);
				@out.print(iline);

				for (int j = 0; j < bf.getN(); j++)
				{
					v.Clear();
					v.Add(new double?(bf.getCoefn(j)));
					v.Add(bf.getUpper(j));
					iline = StringUtil.formatString(v, format_2);
					@out.print(iline);
				}
				@out.println();

				// 2nd line
				v.Clear();
				v.Add(new double?(bf.getProratnf()));
				v.Add(new int?(bf.getM()));
				iline = StringUtil.formatString(v, format_3);
				@out.print(iline);
				for (int j = 0; j < bf.getM(); j++)
				{
					v.Clear();
					v.Add(new double?(bf.getCoefm(j)));
					v.Add(bf.getFlowm(j));
					iline = StringUtil.formatString(v, format_2);
					@out.print(iline);
				}
				@out.println();
			}
		}
		catch (Exception e)
		{
			Message.printWarning(3, routine, e);
			throw e;
		}
		finally
		{
			if (@out != null)
			{
				@out.flush();
				@out.close();
				@out = null;
			}
		}
	}

	/// <summary>
	/// Writes a list of StateMod_StreamEstimate_Coefficients objects to a list file. 
	/// A header is printed to the top of the file, containing the commands used to 
	/// generate the file.  Any strings in the body of the file that contain the field 
	/// delimiter will be wrapped in "...". </summary>
	/// <param name="filename"> the name of the file to which the data will be written. </param>
	/// <param name="delimiter"> the delimiter to use for separating field values. </param>
	/// <param name="update"> whether to update an existing file, retaining the current 
	/// header (true) or to create a new file with a new header. </param>
	/// <param name="data"> the list of objects to write. </param>
	/// <param name="newComments"> list of new comments to add to the header. </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeListFile(String filename, String delimiter, boolean update, java.util.List<StateMod_StreamEstimate_Coefficients> data, java.util.List<String> newComments) throws Exception
	public static void writeListFile(string filename, string delimiter, bool update, IList<StateMod_StreamEstimate_Coefficients> data, IList<string> newComments)
	{
		int size = 0;
		if (data != null)
		{
			size = data.Count;
		}

		IList<string> fields = new List<string>();
		fields.Add("ID");
		fields.Add("Name");
		fields.Add("UpstreamGage");
		fields.Add("ProrationFactor");
		fields.Add("Weight");
		fields.Add("GageID");
		int fieldCount = fields.Count;

		string[] names = new string[fieldCount];
		string[] formats = new string[fieldCount];
		int comp = StateMod_DataSet.COMP_STREAMESTIMATE_COEFFICIENTS;
		string s = null;
		for (int i = 0; i < fieldCount; i++)
		{
			s = (string)fields[i];
			names[i] = StateMod_Util.lookupPropValue(comp, "FieldName", s);
			formats[i] = StateMod_Util.lookupPropValue(comp, "Format", s);
		}

		string oldFile = null;
		if (update)
		{
			oldFile = IOUtil.getPathUsingWorkingDir(filename);
		}

		int j = 0;
		int k = 0;
		int num = 0;
		int M = 0;
		int N = 0;
		PrintWriter @out = null;
		StateMod_StreamEstimate_Coefficients coeff = null;
		IList<string> commentIndicators = new List<string>(1);
		commentIndicators.Add("#");
		IList<string> ignoredCommentIndicators = new List<string>(1);
		ignoredCommentIndicators.Add("#>");
		string[] line = new string[fieldCount];
		string id = null;
		StringBuilder buffer = new StringBuilder();

		try
		{
			// Add some basic comments at the top of the file.  Do this to a copy of the
			// incoming comments so that they are not modified in the calling code.
			IList<string> newComments2 = null;
			if (newComments == null)
			{
				newComments2 = new List<string>();
			}
			else
			{
				newComments2 = new List<string>(newComments);
			}
			newComments2.Insert(0,"");
			newComments2.Insert(1,"StateMod stream estimate coefficients as a delimited list file.");
			newComments2.Insert(2,"");
			@out = IOUtil.processFileHeaders(oldFile, IOUtil.getPathUsingWorkingDir(filename), newComments2, commentIndicators, ignoredCommentIndicators, 0);

			for (int i = 0; i < fieldCount; i++)
			{
				if (i > 0)
				{
					buffer.Append(delimiter);
				}
				buffer.Append("\"" + names[i] + "\"");
			}

			@out.println(buffer.ToString());

			for (int i = 0; i < size; i++)
			{
				coeff = (StateMod_StreamEstimate_Coefficients)data[i];

				id = coeff.getID();

				M = coeff.getM();
				N = coeff.getN();

				num = M < N ? N : M;

				for (j = 0; j < num; j++)
				{
					line[0] = StringUtil.formatString(id,formats[0]).Trim();

					if (j < N)
					{
						line[1] = StringUtil.formatString(coeff.getCoefn(j),formats[1]).Trim();
						line[2] = StringUtil.formatString(coeff.getUpper(j),formats[2]).Trim();
					}
					else
					{
						line[1] = "";
						line[2] = "";
					}

					if (j < M)
					{
						line[3] = StringUtil.formatString(coeff.getProratnf(),formats[3]).Trim();
						line[4] = StringUtil.formatString(coeff.getCoefm(j),formats[4]).Trim();
						line[5] = StringUtil.formatString(coeff.getFlowm(j),formats[5]).Trim();
					}
					else
					{
						line[3] = "";
						line[4] = "";
						line[5] = "";
					}

					buffer = new StringBuilder();
					for (k = 0; k < fieldCount; k++)
					{
						if (k > 0)
						{
							buffer.Append(delimiter);
						}
						if (line[k].IndexOf(delimiter, StringComparison.Ordinal) > -1)
						{
							line[k] = "\"" + line[k] + "\"";
						}
						buffer.Append(line[k]);
					}

					@out.println(buffer.ToString());
				}
			}
			@out.flush();
			@out.close();
			@out = null;
		}
		catch (Exception e)
		{
			// TODO SAM 2009-01-05 Log?
			throw e;
		}
		finally
		{
			if (@out != null)
			{
				@out.flush();
				@out.close();
			}
			@out = null;
		}
	}

	}

}