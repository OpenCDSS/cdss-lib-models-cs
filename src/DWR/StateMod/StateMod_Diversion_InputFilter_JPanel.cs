using System.Collections.Generic;

// StateMod_Diversion_InputFilter_JPanel - input filter panel for StateMod diversion data.

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

	using InputFilter = RTi.Util.GUI.InputFilter;
	using InputFilter_JPanel = RTi.Util.GUI.InputFilter_JPanel;
	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// Input filter panel for StateMod diversion data.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class StateMod_Diversion_InputFilter_JPanel extends RTi.Util.GUI.InputFilter_JPanel
	public class StateMod_Diversion_InputFilter_JPanel : InputFilter_JPanel
	{

	/// <summary>
	/// Create an InputFilter_JPanel for creating where clauses
	/// for StateMod diversion station queries.  This is used by the StateMod GUI. </summary>
	/// <param name="dataset"> StateMod_DataSet instance. </param>
	/// <returns> a JPanel containing InputFilter instances for StateMod_Diversion queries. </returns>
	/// <exception cref="Exception"> if there is an error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public StateMod_Diversion_InputFilter_JPanel(StateMod_DataSet dataset) throws Exception
	public StateMod_Diversion_InputFilter_JPanel(StateMod_DataSet dataset) : base()
	{
		IList<InputFilter> input_filters = new List<InputFilter>(15);
		InputFilter filter = null;
		input_filters.Add(new InputFilter("", "", StringUtil.TYPE_STRING, null, null, true)); // Blank to disable filter
		input_filters.Add(new InputFilter("ID", "ID", StringUtil.TYPE_STRING, null, null, true));
		input_filters.Add(new InputFilter("Name", "Name", StringUtil.TYPE_STRING, null, null, true));
		input_filters.Add(new InputFilter("River Node ID", "RiverNodeID", StringUtil.TYPE_STRING, null, null, true));
		filter = new InputFilter("On/Off Switch", "OnOff", StringUtil.TYPE_INTEGER, StateMod_Diversion.getIdivswChoices(true), StateMod_Diversion.getIdivswChoices(false), true);
		filter.setTokenInfo(" ", 0);
		input_filters.Add(filter);
		input_filters.Add(new InputFilter("Capacity", "Capacity", StringUtil.TYPE_DOUBLE, null, null, true));
		filter = new InputFilter("Replacement Reservoir Option", "ReplaceResOption", StringUtil.TYPE_INTEGER, StateMod_Diversion.getIreptypeChoices(true), StateMod_Diversion.getIreptypeChoices(false), true);
		filter.setTokenInfo(" ", 0);
		input_filters.Add(filter);
		input_filters.Add(new InputFilter("Daily ID", "DailyID", StringUtil.TYPE_STRING, null, null, true));
		input_filters.Add(new InputFilter("User Name", "UserName", StringUtil.TYPE_STRING, null, null, true));
		filter = new InputFilter("Demand Type", "DemandType", StringUtil.TYPE_INTEGER, StateMod_Diversion.getIdvcomChoices(true), StateMod_Diversion.getIdvcomChoices(false), true);
		filter.setTokenInfo(" ", 0);
		input_filters.Add(filter);
		/*
		input_filters.addElement ( new InputFilter (
			"Number of Returns", "NumReturns",
			StringUtil.TYPE_INTEGER,
			null, null, true ) );
		*/
		input_filters.Add(new InputFilter("Efficiency (Annual)", "EffAnnual", StringUtil.TYPE_DOUBLE, null, null, true));
		input_filters.Add(new InputFilter("Area (ACRE)", "Area", StringUtil.TYPE_DOUBLE, null, null, true));
		filter = new InputFilter("Use Type", "UseType", StringUtil.TYPE_INTEGER, StateMod_Diversion.getIrturnChoices(true), StateMod_Diversion.getIrturnChoices(false), true);
		filter.setTokenInfo(" ", 0);
		input_filters.Add(filter);
		filter = new InputFilter("Demand Source", "DemandSource", StringUtil.TYPE_INTEGER, StateMod_Diversion.getDemsrcChoices(true), StateMod_Diversion.getDemsrcChoices(false), true);
		filter.setTokenInfo(" ", 0);
		input_filters.Add(filter);
		// TODO SAM 2004-10-25 monthly efficiencies? Returns?

		setToolTipText("<html>HydroBase queries can be filtered<br>based on station data.</html>");
		setInputFilters(input_filters, 3, -1);
	}

	}

}