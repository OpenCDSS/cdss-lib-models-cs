using System.Collections.Generic;

// StateMod_ComponentValidation - simple class to hold validation data for a StateMod component object

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

	/// <summary>
	/// Simple class to hold validation data for a StateMod component object.  An instance of this class is returned
	/// by the StateMod_Component.validateComponent() method.
	/// </summary>
	public class StateMod_ComponentValidation
	{

	/// <summary>
	/// List of component data validation items - specific problems.
	/// </summary>
	private IList<StateMod_ComponentValidationProblem> __validationProblems = new List<object>();

	/// <summary>
	/// Add a validation result. </summary>
	/// <param name="validationItem"> a validation problem. </param>
	public virtual void add(StateMod_ComponentValidationProblem item)
	{
		__validationProblems.Add(item);
	}

	/// <summary>
	/// Get a validation problem.
	/// </summary>
	public virtual StateMod_ComponentValidationProblem get(int i)
	{
		return __validationProblems[i];
	}

	/// <summary>
	/// Get all validation problems.
	/// </summary>
	public virtual IList<StateMod_ComponentValidationProblem> getAll()
	{
		return __validationProblems;
	}

	/// <summary>
	/// Return the number of validation problems for the object. </summary>
	/// <returns> the number of validation problems for the object. </returns>
	public virtual int size()
	{
		return __validationProblems.Count;
	}

	}

}