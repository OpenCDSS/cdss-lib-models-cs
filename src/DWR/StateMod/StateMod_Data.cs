using System;

// StateMod_Data - super class for many of the StateModLib classes

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
// StateMod_Data - super class for many of the StateModLib classes
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// Notes:	(1)This class is abstract and cannot be directly 
//		instantiated.
//		(2)Derived classes MUST override the toString()function.
//------------------------------------------------------------------------------
// History:
// 
// 19 Aug 1997	Catherine E.		Created initial version of class.
//		Nutting-Lane, RTi
// 07 Jan 1998	CEN, RTi		Adding operational rights type.
// 11 Feb 1998	CEN, RTi		Adding SMFileData.setDirty to all set
//					routines.
// 06 Apr 1998	CEN, RTi		Adding java documentation style
//					comments.
// 17 Feb 2001	Steven A. Malers, RTi	Review code as part of upgrades.  Add
//					finalize.  Add some javadoc.  Set unused
//					variables to null.  Get rid of debugs
//					that are no longer necessary.
//					Alphabetize methods.  Handle null
//					arguments.  Deprecated some methods that
//					are now in SMUtil.
// 2002-09-09	SAM, RTi		Add a comment about the GeoRecord
//					reference in derived classes to allow
//					two-way connections between spatial and
//					StateMod data.
// 2002-09-19	SAM, RTi		Use isDirty()instead of setDirty()to
//					indicate edits.
//------------------------------------------------------------------------------
// 2003-06-05	J. Thomas Sapienza 	Initial StateMod_ version.
// 2003-06-12	JTS, RTi		Added MISSING_* data
// 2003-07-07	SAM, RTi		Handle null data set for cases where the
//					code is used outside a full StateMod
//					data set.	
// 2003-07-16	JTS, RTi		Added indexOf and indexOfName
// 2003-08-03	SAM, RTi		* Changed isDirty() back to setDirty().
//					* Remove isMissing(), indexOf(),
//					  lookup*() methods - they are now in
//					  StateMod_Util.
// 2003-10-09	JTS, RTi		* Now implements Cloneable.
//					* Added clone().
//					* Added equals().
//					* Added rudimentary toString().
//					* Now implements Comparable.
//					* Added compareTo().
// 2003-10-15	JTS, RTi		Revised the clone code.
// 2004-07-14	JTS, RTi		* Added _isClone.
//					* Added _original.
//					* Added acceptChanges().
//					* Added changed().
//					* Added setDataSet().
// 2005-04-13	JTS, RTi		Added writeToListFile(), which is used
//					by subclasses.
// 2007-04-27	Kurt Tometich, RTi		Fixed some warnings.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
// 2007-05-17	SAM, RTi		Add comment as data member to help with modeling
//					procedure development.
//------------------------------------------------------------------------------

namespace DWR.StateMod
{
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;
	using DateTime = RTi.Util.Time.DateTime;

	/// <summary>
	/// Abstract object from which all other StateMod objects are derived.  
	/// Each object can be identified by setting the smdata_type member.
	/// Possible values for this member come from the SMFileData class (RES_FILE, DIV_FILE, etc.)
	/// </summary>
	public class StateMod_Data : ICloneable, IComparable<StateMod_Data>
	{

	// TODO SAM 2010-12-20 Consider moving to NaN for missing floats, but what about integers?
	public static DateTime MISSING_DATE = null;
	public static double MISSING_DOUBLE = -999.0;
	public static float MISSING_FLOAT = (float) - 999.0;
	public static int MISSING_INT = -999;
	public static long MISSING_LONG = -999;
	public static string MISSING_STRING = "";

	/// <summary>
	/// Reference to the _dataset into which all the StateMod_* data objects are 
	/// being placed.  It is used statically because this way every object that extends
	/// StateMod_Data will have a reference to the same dataset for using the setDirty() method.
	/// </summary>
	protected internal static StateMod_DataSet _dataset = null;

	/// <summary>
	/// Whether the data is dirty or not.
	/// </summary>
	protected internal bool _isDirty = false;

	/// <summary>
	/// Whether this object is a clone (i.e. data that can be canceled out of).
	/// </summary>
	protected internal bool _isClone = false;

	/// <summary>
	/// Specific type of data.  This should be set by each derived class in its
	/// constructor.  The types agree with the StateMod_DataSet component types.
	/// </summary>
	protected internal int _smdata_type = -999;

	/// <summary>
	/// Station id.
	/// </summary>
	protected internal string _id;

	/// <summary>
	/// Station name.
	/// </summary>
	protected internal string _name;

	/// <summary>
	/// Comment for data object.
	/// </summary>
	protected internal string _comment;

	/// <summary>
	/// For stations, the river node where station is located.  For water rights, the
	/// station identifier where the right is located.
	/// </summary>
	protected internal string _cgoto;

	/// <summary>
	/// Switch on or off
	/// </summary>
	protected internal int _switch;

	/// <summary>
	/// UTM should be written to gis file.
	/// </summary>
	protected internal int _new_utm;

	/// <summary>
	/// For mapping, but see StateMod_GeoRecord interface.
	/// </summary>
	protected internal double _utm_x;

	/// <summary>
	/// For mapping, but see StateMod_GeoRecord interface.
	/// </summary>
	protected internal double _utm_y;

	/// <summary>
	/// Label used when display on map.
	/// </summary>
	protected internal string _mapLabel;
	protected internal bool _mapLabelDisplayID;
	protected internal bool _mapLabelDisplayName;

	/// <summary>
	/// For screens that can cancel changes, this stores the original values.
	/// </summary>
	protected internal StateMod_Data _original;

	/// <summary>
	/// Each GRShape has a pointer to the StateMod_Data which is its associated object.
	/// This variable whether this object's location was found.  We could
	/// have pointed back to the GRShape, but I was trying to avoid including GR in this package.
	/// Add a GeoRecord _georecord; object to derived classes that really have 
	/// location information.  Adding it here would bloat the code since 
	/// StateMod_Data is the base class for most other classes.
	/// </summary>
	public bool _shape_found;

	/// <summary>
	/// Constructor.
	/// </summary>
	public StateMod_Data() : base()
	{
		initialize();
	}

	/// <summary>
	/// Clones this object.  The _dataset object is not cloned -- the reference is 
	/// kept pointing to the same dataset. </summary>
	/// <returns> a clone of this object. </returns>
	public virtual object clone()
	{
		StateMod_Data data = null;
		try
		{
			data = (StateMod_Data)base.clone();
		}
		catch (CloneNotSupportedException)
		{
			// should never happen.
		}

		// dataset is not cloned -- the same reference is used.
		//StateMod_Data._dataset = _dataset;
		data._isClone = true;

		return data;
	}

	/// <summary>
	/// Compares this object to another StateMod_Data object based on _id, _name,
	/// _cgoto, _switch, _utm_x, _utm_y, in that order.  The comment is not compared. </summary>
	/// <param name="o"> the object to compare against. </param>
	/// <returns> 0 if they are the same, 1 if this object is greater than the other object, or -1 if it is less. </returns>
	public virtual int CompareTo(StateMod_Data data)
	{
		string name = data.getName();
		string id = data.getID();
		string cgoto = data.getCgoto();
		int swhich = data.getSwitch();
		double utm_x = data.getUTMx();
		double utm_y = data.getUTMy();

		int res = _id.CompareTo(id);
		// Set as needed for troubleshooting during development, especially StateMod GUI
		bool debug = false; //Message.isDebugOn;
		if (res != 0)
		{
			if (debug)
			{
				Message.printDebug(1,"compareTo","smdata ID are different");
			}
			return res;
		}

		res = _name.CompareTo(name);
		if (res != 0)
		{
			if (debug)
			{
				Message.printDebug(1,"compareTo","smdata name are different");
			}
			return res;
		}

		res = _cgoto.CompareTo(cgoto);
		if (res != 0)
		{
			if (debug)
			{
				Message.printDebug(1,"compareTo","smdata cgoto are different, old=\"" + _cgoto + "\", new=\"" + cgoto + "\"");
			}
			return res;
		}

		if (_switch < swhich)
		{
			if (debug)
			{
				Message.printDebug(1,"compareTo","smdata switch are different");
			}
			return -1;
		}
		else if (_switch > swhich)
		{
			if (debug)
			{
				Message.printDebug(1,"compareTo","smdata switch are different");
			}
			return 1;
		}

		if (_utm_x < utm_x)
		{
			if (debug)
			{
				Message.printDebug(1,"compareTo","smdata utmx are different");
			}
			return -1;
		}
		else if (_utm_x > utm_x)
		{
			if (debug)
			{
				Message.printDebug(1,"compareTo","smdata utmx are different");
			}
			return 1;
		}

		if (_utm_y < utm_y)
		{
			if (debug)
			{
				Message.printDebug(1,"compareTo","smdata utmy are different");
			}
			return -1;
		}
		else if (_utm_y > utm_y)
		{
			if (debug)
			{
				Message.printDebug(1,"compareTo","smdata utmy are different");
			}
			return 1;
		}

		return 0;
	}

	/// <summary>
	/// Checks to see if two StateMod_Data objects are equal.  The objects are equal
	/// if all the boolean, double and int variables are the same, the Strings match
	/// with case-sensitivity, and they both have are in the same _dataset object.
	/// The comment is not compared. </summary>
	/// <returns> true if they are equal, false if not. </returns>
	public virtual bool Equals(StateMod_Data data)
	{
		if (data._isDirty == _isDirty && data._utm_x == _utm_x && data._utm_y == _utm_y && data._switch == _switch && data._id.Equals(_id) && data._name.Equals(_name) && data._cgoto.Equals(_cgoto) && data._smdata_type == _smdata_type && data._mapLabel.Equals(_mapLabel) && data._mapLabelDisplayID == _mapLabelDisplayID && data._mapLabelDisplayName == _mapLabelDisplayName && StateMod_Data._dataset == _dataset)
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Clean up before garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~StateMod_Data()
	{
		_id = null;
		_name = null;
		_comment = null;
		_cgoto = null;
		_mapLabel = null;
	}

	/// <summary>
	/// Return the Cgoto.
	/// </summary>
	public virtual string getCgoto()
	{
		return _cgoto;
	}

	/// <summary>
	/// Return the comment.
	/// </summary>
	public virtual string getComment()
	{
		return _comment;
	}

	/// <summary>
	/// Return the ID.
	/// </summary>
	public virtual string getID()
	{
		return _id;
	}

	public virtual string getMapLabel()
	{
		return _mapLabel;
	}

	/// <summary>
	/// Return the name.
	/// </summary>
	public virtual string getName()
	{
		return _name;
	}

	/// <summary>
	/// Return the new_utm flag.
	/// </summary>
	public virtual int getNewUTM()
	{
		return _new_utm;
	}

	/// <summary>
	/// Return the StateMod_DataType.
	/// </summary>
	public virtual int getStateMod_DataType()
	{
		return (_smdata_type);
	}

	/// <summary>
	/// Return the switch.
	/// </summary>
	public virtual int getSwitch()
	{
		return _switch;
	}

	/// <summary>
	/// Return the UTM x coordinate.
	/// </summary>
	public virtual double getUTMx()
	{
		return _utm_x;
	}

	/// <summary>
	/// Retrieve the UTM y coordinate.
	/// </summary>
	public virtual double getUTMy()
	{
		return _utm_y;
	}

	/// <summary>
	/// Initialize data members.
	/// </summary>
	private void initialize()
	{
		_id = "";
		_name = "";
		_comment = "";
		_cgoto = "";
		_mapLabel = "";
		_mapLabelDisplayID = false;
		_mapLabelDisplayName = false;
		_shape_found = false;
		_switch = 1;
		_new_utm = 0;
		_utm_x = -999;
		_utm_y = -999;
	}

	/// <summary>
	/// Returns whether the data is dirty or not. </summary>
	/// <returns> whether the data is dirty or not. </returns>
	public virtual bool isDirty()
	{
		return _isDirty;
	}

	/// <summary>
	/// Resets the map label booleans to both false.
	/// </summary>
	public virtual void resetMapLabelBooleans()
	{
		_mapLabelDisplayID = false;
		_mapLabelDisplayName = false;
	}

	/// <summary>
	/// Restores the values from the _original object into the current object and 
	/// sets _original to null.
	/// </summary>
	public virtual void restoreOriginal()
	{
		StateMod_Data d = (StateMod_Data)_original;
		_utm_x = d._utm_x;
		_utm_y = d._utm_y;
		_new_utm = d._new_utm;
		_switch = d._switch;
		_id = d._id;
		_name = d._name;
		_comment = d._comment;
		_cgoto = d._cgoto;
		_smdata_type = d._smdata_type;
		_mapLabel = d._mapLabel;
		_mapLabelDisplayID = d._mapLabelDisplayID;
		_mapLabelDisplayName = d._mapLabelDisplayName;
	}

	/// <summary>
	/// Set the Cgoto. </summary>
	/// <param name="s"> the new Cgoto. </param>
	public virtual void setCgoto(string s)
	{
		if (string.ReferenceEquals(s, null))
		{
			return;
		}
		if (!s.Equals(_cgoto))
		{
			if (!_isClone && !_isClone && _dataset != null)
			{
				_dataset.setDirty(_smdata_type, true);
			}
			_cgoto = s;
		}
	}

	/// <summary>
	/// Set the Comment. </summary>
	/// <param name="s"> the new comment. </param>
	public virtual void setComment(string s)
	{
		if (string.ReferenceEquals(s, null))
		{
			return;
		}
		if (!s.Equals(_comment))
		{
			if (!_isClone && !_isClone && _dataset != null)
			{
				_dataset.setDirty(_smdata_type, true);
			}
			_comment = s;
		}
	}

	/// <summary>
	/// Sets the dataset that all StateMod_Data objects will share.
	/// </summary>
	public static void setDataSet(StateMod_DataSet dataset)
	{
		_dataset = dataset;
	}

	/// <summary>
	/// Sets whether the data is dirty or not. </summary>
	/// <returns> whether the data is dirty or not. </returns>
	public virtual void setDirty(bool dirty)
	{
		_isDirty = dirty;
	}

	/// <summary>
	/// Set the ID. </summary>
	/// <param name="s"> the new ID. </param>
	public virtual void setID(string s)
	{
		if ((!string.ReferenceEquals(s, null)) && (!s.Equals(_id)))
		{
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(_smdata_type, true);
			}
			_id = s;
		}
	}

	/// <summary>
	/// Set the ID </summary>
	/// <param name="d"> the new ID </param>
	public virtual void setID(double d)
	{
		double? D = new double?(d);
		setID(D.ToString());
	}

	/// <summary>
	/// Sets the map ID according to contents of aResponse.  However, because one 
	/// layer may turn the ID on and the next may turn it back off, a check is first 
	/// done to see if _mapLabel has already been set.  Therefore, before calling 
	/// setMapLabel in a loop, all _mapLabel members should be set to ""
	/// </summary>
	/*
	REVISIT(JTS - 2003-06-04)
	whenever SMGISResponse is converted ...
	public void setMapLabel(SMGISResponse aResponse) {
		String id = "";
	
		// ID - this seems like a waste to put into 2 steps, but previous 
		// calls to setMapLabel may have set to true
		if (aResponse.getMapDisplayID()) {
			_mapLabelDisplayID = true;
		}
		if (_mapLabelDisplayID) {
			id += _id;
		}
	
		if (aResponse.getMapDisplayName()) {
			_mapLabelDisplayName = true;
		}
		if (_mapLabelDisplayName) {
			id += " " + _name;
		}
	
		if (Message.isDebugOn) {
			Message.printDebug(50, "StateMod_Data.setMapLabel",
				"Setting map label for " + _id + " to " + id);
		}
		_mapLabel = id;
		id = null;
	}
	*/

	/// <summary>
	/// Set the StateMod_DataType </summary>
	/// <param name="type"> type of node - uses types in SMFileData. </param>
	public virtual void setStateMod_DataType(int type)
	{
		_smdata_type = type;
	}

	/// <summary>
	/// Set the name. </summary>
	/// <param name="s"> the new Name. </param>
	public virtual void setName(string s)
	{
		if (string.ReferenceEquals(s, null))
		{
			return;
		}
		if (!s.Equals(_name))
		{
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(_smdata_type, true);
			}
			_name = s;
		}
	}

	/// <summary>
	/// Set the new_utm flag.  If the flag is set to 1, the UTM value is new and
	/// the GIS file should be rewritten. </summary>
	/// <param name="i"> new utm flag </param>
	public virtual void setNewUTM(int i)
	{
			_new_utm = i;
	}

	/// <summary>
	/// Set the switch. </summary>
	/// <param name="i"> the new switch: 1 = on, 0 = off, or other values for some data types. </param>
	public virtual void setSwitch(int i)
	{
		if (i != _switch)
		{
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(_smdata_type, true);
			}
			_switch = i;
		}
	}

	/// <summary>
	/// Set the switch. </summary>
	/// <param name="i"> the new switch: 1 = on, 0 = off. </param>
	public virtual void setSwitch(int? i)
	{
		setSwitch(i.Value);
	}

	/// <summary>
	/// Set the switch </summary>
	/// <param name="str"> the new switch: 1 = on, 0 = off </param>
	public virtual void setSwitch(string str)
	{
		if (string.ReferenceEquals(str, null))
		{
			return;
		}
		setSwitch(StringUtil.atoi(str.Trim()));
	}

	/// <summary>
	/// Set the UTM x and y coordinate. </summary>
	/// <param name="x"> new x UTM </param>
	/// <param name="y"> new y UTM </param>
	public virtual void setUTM(double x, double y)
	{
		if (_utm_x != x || _utm_y != y)
		{
			_utm_x = x;
			_utm_y = y;
			if (!_isClone && _dataset != null)
			{
				_dataset.setDirty(StateMod_DataSet.COMP_GEOVIEW, true);
			}
		}
	}

	/// <summary>
	/// Set the UTM x and y coordinate. </summary>
	/// <param name="x"> new x UTM </param>
	/// <param name="y"> new y UTM </param>
	public virtual void setUTM(double? x, double? y)
	{
		setUTM(x.Value, y.Value);
	}

	/// <summary>
	/// Set the UTM x and y coordinate </summary>
	/// <param name="sx_orig"> new x UTM </param>
	/// <param name="sy_orig"> new y UTM </param>
	public virtual void setUTM(string sx_orig, string sy_orig)
	{
		if ((string.ReferenceEquals(sx_orig, null)) || (string.ReferenceEquals(sy_orig, null)))
		{
			return;
		}
		setUTM(StringUtil.atod(sx_orig.Trim()),StringUtil.atod(sy_orig.Trim()));
	}

	/// <summary>
	/// Returns a String representation of this object.  Omit comment. </summary>
	/// <returns> a String representation of this object. </returns>
	public override string ToString()
	{
		return _isDirty + ", " + _utm_x + ", " + _utm_y + ", " + _new_utm + ", " + _switch + ", " + _id + ", " + _name + ", " + _cgoto + ", " + _smdata_type + ", " + _mapLabel + ", "
			+ _mapLabelDisplayID + ", " + _mapLabelDisplayName;
	}

	}

}