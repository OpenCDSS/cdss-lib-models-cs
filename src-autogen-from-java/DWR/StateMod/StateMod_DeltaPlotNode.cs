using System.Collections.Generic;

// StateMod_DeltaPlotNode - Stores information about a station to be graphed.

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
// StateMod_DeltaPlotNode - Stores information about a station to be graphed.  
//	Used in conjunction with StateMod_DeltaPlot.
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
// 
// 30 Nov 1998	Catherine E.		Created initial version of class.
//		Nutting-Lane, RTi
// 17 Feb 2001	Steven A. Malers, RTi	Review code.  Clean up javadoc.  Add
//					finalize().  Handle nulls.  Alphabetize
//					methods.  Set unused variables to null.
//					Remove unneeded imports.
//------------------------------------------------------------------------------
// 2003-08-26	J. Thomas Sapienza, RTi	Initial StateMod version from 
//					SMBigPictNode.
//------------------------------------------------------------------------------

namespace DWR.StateMod
{

	using StringUtil = RTi.Util.String.StringUtil;

	public class StateMod_DeltaPlotNode : StateMod_Data
	{

	protected internal new string _id;
	protected internal new string _name;
	protected internal double _x;
	protected internal double _y;
	protected internal IList<double?> _z;

	/// <summary>
	/// Constructor.
	/// </summary>
	public StateMod_DeltaPlotNode() : base()
	{
		initialize();
	}

	public virtual void addZ(double d)
	{
		addZ(new double?(d));
	}

	public virtual void addZ(double? d)
	{
		_z.Add(d);
	}

	public virtual void addZ(string str)
	{
		if (string.ReferenceEquals(str, null))
		{
			return;
		}
		addZ(StringUtil.atod(str.Trim()));
	}

	/// <summary>
	/// Clean up before garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~StateMod_DeltaPlotNode()
	{
		_id = null;
		_name = null;
		_x = -999;
		_y = -999;
		_z = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Get id.
	/// </summary>
	public override string getID()
	{
		return _id;
	}

	/// <summary>
	/// Get name.
	/// </summary>
	public override string getName()
	{
		return _name;
	}

	/// <summary>
	/// Get x coordinate.
	/// </summary>
	public virtual double getX()
	{
		return _x;
	}

	/// <summary>
	/// Get y coordinate.
	/// </summary>
	public virtual double getY()
	{
		return _y;
	}

	public virtual double getZ(int i)
	{
		double? d = (double?)_z[i];
		return d.Value;
	}

	public virtual int getZsize()
	{
		return _z.Count;
	}

	private void initialize()
	{
		_id = "";
		_name = "";
		_x = -999;
		_y = -999;
		_z = new List<double?>(10,10);
	}

	public override void setID(string s)
	{
		if (!string.ReferenceEquals(s, null))
		{
			_id = s;
		}
	}

	public override void setName(string s)
	{
		if (!string.ReferenceEquals(s, null))
		{
			_name = s;
		}
	}

	public virtual void setX(double d)
	{
		_x = d;
	}

	public virtual void setX(string str)
	{
		if (!string.ReferenceEquals(str, null))
		{
			setX(StringUtil.atod(str.Trim()));
		}
	}

	public virtual void setY(double d)
	{
		_y = d;
	}

	public virtual void setY(string str)
	{
		if (string.ReferenceEquals(str, null))
		{
			return;
		}
		setY(StringUtil.atod(str.Trim()));
	}

	}

}