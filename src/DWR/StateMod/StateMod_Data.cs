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
    public class StateMod_Data
    {

        // TODO SAM 2010-12-20 Consider moving to NaN for missing floats, but what about integers?
        //public static DateTime MISSING_DATE = null;
        public static double MISSING_DOUBLE = -999.0;
        public static float MISSING_FLOAT = (float)-999.0;
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
        /// Sets whether the data is dirty or not. </summary>
        /// <returns> whether the data is dirty or not. </returns>
        public virtual void setDirty(bool dirty)
        {
            _isDirty = dirty;
        }

        /// <summary>
        /// Sets the dataset that all StateMod_Data objects will share.
        /// </summary>
        public static void setDataSet(StateMod_DataSet dataset)
        {
            _dataset = dataset;
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
    }
}
