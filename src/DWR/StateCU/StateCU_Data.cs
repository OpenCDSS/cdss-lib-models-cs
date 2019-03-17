using System;

// StateCU_Data - this class can be used as a base class for StateCU data objects.

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

namespace DWR.StateCU
{
	/// <summary>
	/// This StateCU_Data class can be used as a base class for StateCU data objects.
	/// Common data like identifier and name is maintained here to simplify access and remove redundant code.
	/// </summary>
	public abstract class StateCU_Data : ICloneable, IComparable<StateCU_Data>
	{

	protected internal string _id = StateCU_Util.MISSING_STRING;
	protected internal string _name = StateCU_Util.MISSING_STRING;
	protected internal bool _is_dirty = false;

	/// <summary>
	/// Whether this object is a clone (i.e., data that can be cancelled out of).
	/// </summary>
	protected internal bool _isClone = false;

	/// <summary>
	/// For screens that can cancel changes, this stores the original values.
	/// </summary>
	protected internal object _original;

	/// <summary>
	/// Construct the object and set values to empty strings.
	/// </summary>
	public StateCU_Data()
	{
	}

	/// <summary>
	/// Clones this object. </summary>
	/// <returns> a clone of this object. </returns>
	public virtual object clone()
	{
		StateCU_Data data = null;
		try
		{
			data = (StateCU_Data)base.clone();
		}
		catch (CloneNotSupportedException)
		{
			// should never happen.
		}

		// dataset is not cloned -- the same reference is used.
		data._isClone = true;

		return data;
	}

	/// <summary>
	/// Compares this object to another StateCU_Data object based on _id and _name, in that order. </summary>
	/// <param name="o"> the object to compare against. </param>
	/// <returns> 0 if they are the same, 1 if this object is greater than the other object, or -1 if it is less. </returns>
	public virtual int CompareTo(StateCU_Data data)
	{
		string name = data.getName();
		string id = data.getID();

		int res = _id.CompareTo(id);
		if (res != 0)
		{
			return res;
		}

		res = _name.CompareTo(name);
		if (res != 0)
		{
			return res;
		}
		// The same...
		return 0;
	}

	/// <summary>
	/// Finalize before garbage collection. </summary>
	/// <exception cref="Throwable"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~StateCU_Data()
	{
		_id = null;
		_name = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Return the identifier. </summary>
	/// <returns> the identifier. </returns>
	public virtual string getID()
	{
		return _id;
	}

	/// <summary>
	/// Return the name. </summary>
	/// <returns> the name. </returns>
	public virtual string getName()
	{
		return _name;
	}

	/// <summary>
	/// Indicates whether the data object is dirty (has been modified). </summary>
	/// <returns> true if the data object is dirty (has been modified). </returns>
	public virtual bool isDirty()
	{
		return _is_dirty;
	}

	/// <summary>
	/// Set whether the data object is dirty (has been modified). </summary>
	/// <param name="is_dirty"> true if the object is being marked as dirty. </param>
	/// <returns> true if the data object is dirty (has been modified). </returns>
	public virtual bool isDirty(bool is_dirty)
	{
		_is_dirty = is_dirty;
		return _is_dirty;
	}

	/// <summary>
	/// Restores the values from the _original object into the current object and sets _original to null.
	/// </summary>
	public virtual void restoreOriginal()
	{
		StateCU_Data d = (StateCU_Data)_original;
		_id = d._id;
		_name = d._name;
	}

	/// <summary>
	/// Set the identifier. </summary>
	/// <param name="id"> Identifier for object. </param>
	public virtual void setID(string id)
	{
		_id = id;
	}

	/// <summary>
	/// Set the name. </summary>
	/// <param name="name"> Name for object. </param>
	public virtual void setName(string name)
	{
		_name = name;
	}

	/// <summary>
	/// Print information about the object.
	/// </summary>
	public override string ToString()
	{
		return _id + "," + _name;
	}

	}

}