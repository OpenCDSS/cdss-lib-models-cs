// StateMod_ComponentValidationProblem - simple class to hold validation problem for a StateMod component object

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
	/// Simple class to hold validation problem for a StateMod component object, including the object reference,
	/// the problem description, and the recommendation to fix.
	/// </summary>
	public class StateMod_ComponentValidationProblem
	{

	/// <summary>
	/// Object that has the problem.
	/// </summary>
	private StateMod_ComponentValidator __data;

	/// <summary>
	/// Validation problem.
	/// </summary>
	private string __problem = "";

	/// <summary>
	/// Validation recommendation.
	/// </summary>
	private string __recommendation = "";

	/// <summary>
	/// Constructor.  Null parameters will be converted to empty strings.  Simple validation issues may not
	/// have a recommendation.  More complex problems, for example when cross-checking dataset components, may have
	/// a correspondingly more complex recommendation. </summary>
	/// <param name="data"> object that has the problem. </param>
	/// <param name="problem"> description of validation problem. </param>
	/// <param name="recommendation"> recommendation of how to correct the problem. </param>
	public StateMod_ComponentValidationProblem(StateMod_ComponentValidator data, string problem, string recommendation)
	{
		__problem = problem;
		if (string.ReferenceEquals(__problem, null))
		{
			__problem = "";
		}
		__data = data;
		__recommendation = recommendation;
		if (string.ReferenceEquals(__recommendation, null))
		{
			__recommendation = "";
		}
	}

	/// <summary>
	/// Return the data object that has the problem.
	/// </summary>
	public virtual StateMod_ComponentValidator getData()
	{
		return __data;
	}

	/// <summary>
	/// Return the problem description.
	/// </summary>
	public virtual string getProblem()
	{
		return __problem;
	}

	/// <summary>
	/// Return the recommendation description.
	/// </summary>
	public virtual string getRecommendation()
	{
		return __recommendation;
	}

	}

}