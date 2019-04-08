using System;
using System.Collections.Generic;

// StateCU_Util - utility classes for StateCU package

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

//-----------------------------------------------------------------------------
// StateCU_Util - utility classes for StateCU package
//-----------------------------------------------------------------------------
// History:
//
// 2003-06-04	Steven A. Malers, RTi	* Change name of class from
//					  StateCUUtil to StateCU_Util.
//					* Update to use DateTime instead of
//					  TSDate.
// 2004-02-28	SAM, RTi		* Move indexOf(), indexOfName(), match()
//					  from StateCU_Data.
// 2005-03-08	SAM, RTi		* Add sortStateCU_DataVector(), similar
//					  to the StateMod version.
// 2005-04-05	SAM, RTi		* Add lookupTimeSeriesGraphTitle().
// 2005-04-18	J. Thomas Sapienza, RTi	Added the lookup*() methods.
//-----------------------------------------------------------------------------

namespace DWR.StateCU
{

	using Validator = RTi.Util.IO.Validator;
	using Validators = RTi.Util.IO.Validators;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;
	using DateTime = RTi.Util.Time.DateTime;

	/// <summary>
	/// This StateCU_Util class contains static data and methods used in the StateCU package.
	/// </summary>
	public abstract class StateCU_Util
	{

	public static string MISSING_STRING = "";
	public static int MISSING_INT = -999;
	public static float MISSING_FLOAT = (float) - 999.0;
	public static double MISSING_DOUBLE = -999.0;
	private static double MISSING_DOUBLE_FLOOR = -999.1;
	private static double MISSING_DOUBLE_CEILING = -998.9;
	public static DateTime MISSING_DATE = null;

	public static void checkAndSet(int i, JTextField textField)
	{
		if (isMissing(i))
		{
			textField.setText("");
		}
		else
		{
			textField.setText("" + i);
		}
	}

	public static void checkAndSet(double d, JTextField textField)
	{
		if (isMissing(d))
		{
			textField.setText("");
		}
		else
		{
			textField.setText("" + d);
		}
	}

	/// <summary>
	/// Helper method to return validators to check an ID. </summary>
	/// <returns> List of Validators. </returns>
	public static Validator[] getIDValidators()
	{
		return new Validator[] {Validators.notBlankValidator(), Validators.regexValidator("^[0-9a-zA-Z\\._]+$")};
	}

	/// <summary>
	/// Determine the CU Location given a part identifier.  If the part identifier
	/// matches a full location, then the full location identifier is returned.  Only ditch
	/// identifiers can be matched (collections of parcels cannot). </summary>
	/// <param name="CULocation_List"> a Vector of StateCU_Location to be searched.  The
	/// collection information is assumed to have been defined for the locations. </param>
	/// <param name="part_id"> The identifier to be found in the list of locations. </param>
	/// <returns> the matching StateCU_Location, or null if a match cannot be found. </returns>
	public static StateCU_Location getLocationForPartID(IList<StateCU_Location> CULocation_List, string part_id)
	{
		// First try to match the main location.

		int pos = indexOf(CULocation_List, part_id);
		if (pos >= 0)
		{
			return CULocation_List[pos];
		}
		// If here, search the location collections...
		int size = 0;
		if (CULocation_List != null)
		{
			size = CULocation_List.Count;
		}
		StateCU_Location culoc;
		IList<string> part_ids;
		for (int i = 0; i < size; i++)
		{
			culoc = CULocation_List[i];
			// Only check aggregates/collections that are composed of ditches.
			if (!culoc.getCollectionPartType().Equals(StateCU_Location.COLLECTION_PART_TYPE_DITCH, StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}
			// Get the part identifiers...
			part_ids = culoc.getCollectionPartIDsForYear(-1); // Since ditches, year is irrelevant
			int size2 = part_ids.Count;
			for (int j = 0; j < size2; j++)
			{
				if (part_id.Equals((string)part_ids[j], StringComparison.OrdinalIgnoreCase))
				{
					return culoc;
				}
			}
		}
		return null;
	}

	/// <summary>
	/// Helper method to return general validators for numbers. </summary>
	/// <returns> List of Validators. </returns>
	public static Validator[] getNumberValidators()
	{
		//Validators.regexValidator( "^[0-9]+.*" ),
		return new Validator[] {Validators.notBlankValidator(), Validators.rangeValidator(0, 999999)};
	}

	/// <summary>
	/// Helper method to return general validators for an on/off switch. </summary>
	/// <returns> List of Validators. </returns>
	public static Validator[] getOnOffSwitchValidator()
	{
		Validator[] orValidator = new Validator[] {Validators.isEquals(new int?(0)), Validators.isEquals(new int?(1))};
		return new Validator[] {Validators.notBlankValidator(), Validators.or(orValidator)};
	}

	// FIXME SAM 2008-08-22 Need to review all this doc once the software is working
	/// <summary>
	/// Get the time series data types associated with a component.
	/// Currently this returns all possible data types but does not
	/// cut down the lists based on what is actually available. </summary>
	/// <param name="comp_type"> Component type for a station:  StateCU_DataSet.COMP_CU_LOCATIONS. </param>
	/// <param name="binary_filename"> name of the binary output file for which data types
	/// (parameters) are being returned, typically selected by the user with a file
	/// chooser.  The path to the file is not adjusted to a working directory so do that
	/// before calling, if necessary. </param>
	/// <param name="id"> If non-null, it will be used with the data set to limit returned
	/// choices to those appropriate for the dataset. </param>
	/// <param name="dataset"> If a non-null StateCU_DataSet is specified, it will be used with
	/// the id to check for valid time series data types.  For example, it can be used
	/// to return data types for estimated time series. </param>
	/// <param name="statecu_version"> StateCU version as a floating point number.  If this
	/// is greater than ??VERSION_11_00??, then binary file parameters are read from the file. </param>
	/// <param name="interval"> TimeInterval.DAY or TimeInterval.MONTH. </param>
	/// <param name="include_input"> If true, input time series including historic data from
	/// ASCII input files are returned with the
	/// list (suitable for StateMod GUI graphing tool). </param>
	/// <param name="include_input_estimated"> If true, input time series that are estimated are included. </param>
	/// <param name="include_output"> If true, output time series are included in the list (this
	/// is used by the graphing tool).  Note that some output time series are for
	/// internal model use and are not suitable for viewing (as per Ray Bennett) and
	/// are therefore not returned in this list. </param>
	/// <param name="check_availability"> If true, an input data type will only be added if it
	/// is available in the input data set.  Because it is difficult and somewhat time
	/// consuming to check for the validity of output time series, output time series
	/// are not checked.  This flag is currently not used. </param>
	/// <param name="add_group"> If true, a group is added to the front of the data type to
	/// allow grouping of the parameters.  Currently this should only be used for
	/// output parameters (e.g., in TSTool) because other data types have not been grouped. </param>
	/// <param name="add_note"> If true, the string " - Input", " - Output" will be added to the
	/// data types, to help identify input and output parameters.  This is particularly
	/// useful when retrieving time series. </param>
	/// <returns> a non-null list of data types.  The list will have zero size if no
	/// data types are requested or are valid. </returns>
	public static IList<string> getTimeSeriesDataTypes(string binary_filename, int comp_type, string id, StateCU_DataSet dataset, double statecu_version, int interval, bool include_input, bool include_input_estimated, bool include_output, bool check_availability, bool add_group, bool add_note)
	{
		string routine = "StateCU_Util.getTimeSeriesDataTypes";
		IList<string> data_types = new List<string>();

		StateCU_BTS bts = null;
		if (!string.ReferenceEquals(binary_filename, null))
		{
			try
			{
				bts = new StateCU_BTS(binary_filename);
			}
			catch (Exception e)
			{
				// Error reading the file.  Print a warning but go on
				// and just do not have a list of parameters...
				Message.printWarning(3, routine, "Error opening/reading binary file \"" + binary_filename + "\" to determine parameters.");
				Message.printWarning(3, routine, e);
				bts = null;
			}
			// Close the file below after getting information...
		}

		// Get the list of output data types based on the StateCU version.
		// These are then used below.

		//if ( statecu_version >= 0.0 ) {
			// The parameters come from the binary file header.
			// Close the file because it is no longer needed...
			string[] parameters = null;
			if (bts != null)
			{
				parameters = bts.getTimeSeriesParameters();
				// TODO SAM 2006-01-15
				// Remove when tested in production.
				Message.printStatus(2, routine, "Parameters from file:  " + StringUtil.toList(parameters));
				try
				{
					bts.close();
				}
				catch (Exception)
				{
					// Ignore - problem would have occurred at open.
				}
				bts = null;
			}
		//}

		data_types = new List<string>(parameters.Length);
		for (int i = 0; i < parameters.Length; i++)
		{
			data_types.Add(parameters[i]);
		}

		return data_types;
	}

	/// <summary>
	/// Find the position of a StateCU_Data object in the data list, using the
	/// identifier.  The position for the first match is returned. </summary>
	/// <returns> the position, or -1 if not found. </returns>
	/// <param name="data"> an object extended from StateCU_Data </param>
	/// <param name="id"> StateCU_Data identifier. </param>
	public static int indexOf<T1>(IList<T1> data, string id) where T1 : StateCU_Data
	{
		int size = 0;
		if (data != null)
		{
			size = data.Count;
		}
		StateCU_Data d = null;
		for (int i = 0; i < size; i++)
		{
			d = data[i];
			if (id.Equals(d._id, StringComparison.OrdinalIgnoreCase))
			{
				return i;
			}
		}
		return -1;
	}

	/// <summary>
	/// Find the position of a StateCU_Data object in the data list, using the name.
	/// The position for the first match is returned. </summary>
	/// <returns> the position, or -1 if not found. </returns>
	/// <param name="name"> StateCU_Data name. </param>
	public static int indexOfName<T1>(IList<T1> data, string name) where T1 : StateCU_Data
	{
		int size = 0;
		if (data != null)
		{
			size = data.Count;
		}
		StateCU_Data d = null;
		for (int i = 0; i < size; i++)
		{
			d = data[i];
			if (name.Equals(d._name, StringComparison.OrdinalIgnoreCase))
			{
				return i;
			}
		}
		return -1;
	}

	/// <summary>
	/// Indicate if a double value is missing. </summary>
	/// <param name="d"> Double precision value to check. </param>
	/// <returns> true if the value is missing, false, if not. </returns>
	public static bool isMissing(double d)
	{
		if ((d < MISSING_DOUBLE_CEILING) && (d > MISSING_DOUBLE_FLOOR))
		{
			return true;
		}
		else if (double.IsNaN(d))
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	/// <summary>
	/// Indicate if an integer value is missing. </summary>
	/// <param name="i"> Integer value to check. </param>
	/// <returns> true if the value is missing, false, if not. </returns>
	public static bool isMissing(int i)
	{
		if (i == MISSING_INT)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	/// <summary>
	/// Indicate if a String value is missing. </summary>
	/// <param name="s"> String value to check. </param>
	/// <returns> true if the value is missing, false, if not. </returns>
	public static bool isMissing(string s)
	{
		if ((string.ReferenceEquals(s, null)) || (s.Length == 0))
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	/// <summary>
	/// Look up a title to use for a time series graph, given the data set component.
	/// Currently this simply returns the component name, replacing " TS " with " Time Series ". </summary>
	/// <param name="comp_type"> StateCU component type. </param>
	public static string lookupTimeSeriesGraphTitle(int comp_type)
	{
		try
		{
			StateCU_DataSet dataset = new StateCU_DataSet();
			return dataset.lookupComponentName(comp_type).replaceAll(" TS ", " Time Series ");
		}
		catch (Exception)
		{
			// Should not happen.
			return "";
		}
	}

	/// <summary>
	/// Find a list of StateCU_Data in a list, using a regular expression to match identifiers. </summary>
	/// <param name="data_List"> a list of StateCU_Data to search. </param>
	/// <param name="pattern"> Regular expression pattern to use when finding </param>
	/// <returns> a list containing StateCU_Data from data_List that have an
	/// identifier that matches the requested pattern.  A non-null list will be
	/// returned but it may have zero elements.  Cast the result to the proper list of objects. </returns>
	public static IList<StateCU_Data> match<T1>(IList<T1> data_List, string pattern) where T1 : StateCU_Data
	{
		int size = 0;
		if (data_List != null)
		{
			size = data_List.Count;
		}
		StateCU_Data data = null;
		IList<StateCU_Data> matches_List = new List<StateCU_Data>();
		// Apparently if the pattern is "*", Java complains so do a specific check...
		bool return_all = false;
		if (pattern.Equals("*"))
		{
			return_all = true;
		}
		// Loop regardless (always return a new list).
		for (int i = 0; i < size; i++)
		{
			data = data_List[i];
			if (return_all || data.getID().matches(pattern))
			{
				matches_List.Add(data);
			}
		}
		return matches_List;
	}

	/// <summary>
	/// Sorts a list of StateCU_Data objects, depending on the compareTo() method for the specific object. </summary>
	/// <param name="data"> a list of StateCU_Data objects.  Can be null. </param>
	/// <returns> a new sorted list with references to the same data objects in the
	/// passed-in list.  If a null list is passed in, an empty list will be returned.
	/// Cast the result to the list type being sorted </returns>
	public static IList<StateCU_Data> sortStateCUDataList<T1>(IList<T1> data) where T1 : StateCU_Data
	{
		return sortStateCUDataList(data, true);
	}

	/// <summary>
	/// Sorts a list of StateCU_Data objects, depending on the compareTo() method for the specific object. </summary>
	/// <param name="data"> a list of StateMod_Data objects.  Cannot be null. </param>
	/// <param name="return_new"> If true, return a new list with references to the data.
	/// If false, return the original list, with sorted contents. </param>
	/// <returns> a sorted list with references to the same data objects in the
	/// passed-in list.  If a null list is passed in, an empty list will be returned. </returns>
	public static IList<StateCU_Data> sortStateCUDataList<T1>(IList<T1> data, bool return_new) where T1 : StateCU_Data
	{
		if (data == null)
		{
			return new List<StateCU_Data>();
		}
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<StateCU_Data> v = (java.util.List<StateCU_Data>)data;
		IList<StateCU_Data> v = (IList<StateCU_Data>)data;
		int size = data.Count;
		if (return_new)
		{
			if (size == 0)
			{
				return new List<StateCU_Data>();
			}
			v = new List<StateCU_Data>();
			for (int i = 0; i < size; i++)
			{
				v.Add(data[i]);
			}
		}

		if (size == 1)
		{
			return v;
		}

		v.Sort();
		return v;
	}

	/// <summary>
	/// Returns the property value for a component. </summary>
	/// <param name="componentType"> the kind of component to look up for. </param>
	/// <param name="propType"> the property to look up.  One of "FieldName", "FieldNameHeader", "ToolTip", or "Format". </param>
	/// <param name="field"> the field for which to return the property. </param>
	/// <returns> the property, or if it could not be found null will be returned. </returns>
	public static string lookupPropValue(int componentType, string propType, string field)
	{
		if (componentType == StateCU_DataSet.COMP_BLANEY_CRIDDLE)
		{
			return lookupBlaneyCriddlePropValue(propType, field);
		}
		else if (componentType == StateCU_DataSet.COMP_CLIMATE_STATIONS)
		{
			return lookupClimatePropValue(propType, field);
		}
		else if (componentType == StateCU_DataSet.COMP_CROP_CHARACTERISTICS)
		{
			return lookupCropCharacteristicsPropValue(propType, field);
		}
		else if (componentType == StateCU_DataSet.COMP_CU_LOCATIONS)
		{
			return lookupLocationPropValue(propType, field);
		}
		else if (componentType == StateCU_DataSet.COMP_CU_LOCATION_CLIMATE_STATIONS)
		{
			return lookupLocationClimateStationPropValue(propType, field);
		}
		else if (componentType == StateCU_DataSet.COMP_CU_LOCATION_COLLECTIONS)
		{
			return lookupLocationCollectionPropValue(propType, field);
		}
		else if (componentType == StateCU_DataSet.COMP_DELAY_TABLE_ASSIGNMENT_MONTHLY)
		{
			return lookupDelayTableAssignmentPropValue(propType, field);
		}
		if (componentType == StateCU_DataSet.COMP_PENMAN_MONTEITH)
		{
			return lookupPenmanMonteithPropValue(propType, field);
		}

		return null;
	}

	/// <summary>
	/// Indicate whether the StateCU version is at least some standard value.  This is
	/// useful when checking binary formats against a recognized version. </summary>
	/// <returns> true if the version is >= the known version that is being checked. </returns>
	/// <param name="version"> A version to check. </param>
	/// <param name="known_version"> A known version to check against (see VERSION_*). </param>
	public static bool isVersionAtLeast(double version, double known_version)
	{
		if (version >= known_version)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	/// <summary>
	/// Returns the property value for Blaney-Criddle data. </summary>
	/// <param name="propType"> the property to look up.  One of "FieldName", "FieldNameHeader", "ToolTip", or "Format". </param>
	/// <param name="field"> the field for which to return the property. </param>
	/// <returns> the property, or if it could not be found null will be returned. </returns>
	private static string lookupBlaneyCriddlePropValue(string propType, string field)
	{
		if (propType.Equals("FieldName", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "CROP NAME";
			}
			else if (field.Equals("CurveType", StringComparison.OrdinalIgnoreCase))
			{
				return "CURVE TYPE";
			}
			else if (field.Equals("DayPercent", StringComparison.OrdinalIgnoreCase))
			{
				return "DAY OR PERCENT";
			}
			else if (field.Equals("Coefficient", StringComparison.OrdinalIgnoreCase))
			{
				return "COEFFICIENT";
			}
		}
		else if (propType.Equals("FieldNameHeader", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "CROP\nNAME";
			}
			else if (field.Equals("CurveType", StringComparison.OrdinalIgnoreCase))
			{
				return "CURVE\nTYPE";
			}
			else if (field.Equals("DayPercent", StringComparison.OrdinalIgnoreCase))
			{
				return "DAY OR\nPERCENT";
			}
			else if (field.Equals("Coefficient", StringComparison.OrdinalIgnoreCase))
			{
				return "COEFFICIENT";
			}
		}
		else if (propType.Equals("ToolTip", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "Crop name";
			}
			else if (field.Equals("CurveType", StringComparison.OrdinalIgnoreCase))
			{
				return "<html>Curve type (Day or Percent).</html>";
			}
			else if (field.Equals("DayPercent", StringComparison.OrdinalIgnoreCase))
			{
				return "<html>Day of year if Perennial (start, middle, "
					+ "end of month).<br>Percent of year if annual (5% increments).</html>";
			}
			else if (field.Equals("Coefficient", StringComparison.OrdinalIgnoreCase))
			{
				return "Crop coefficient";
			}
		}
		else if (propType.Equals("Format", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "%-20.20s";
			}
			else if (field.Equals("CurveType", StringComparison.OrdinalIgnoreCase))
			{
				return "%-8.8s";
			}
			else if (field.Equals("DayPercent", StringComparison.OrdinalIgnoreCase))
			{
				return "%8d";
			}
			else if (field.Equals("Coefficient", StringComparison.OrdinalIgnoreCase))
			{
				return "%10.2f";
			}
		}

		return null;
	}

	/// <summary>
	/// Returns the property value for climate data. </summary>
	/// <param name="propType"> the property to look up.  One of "FieldName", 
	/// "FieldNameHeader", "ToolTip", or "Format". </param>
	/// <param name="field"> the field for which to return the property. </param>
	/// <returns> the property, or if it could not be found null will be returned. </returns>
	private static string lookupClimatePropValue(string propType, string field)
	{
		if (propType.Equals("FieldName", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "ID";
			}
			else if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "NAME";
			}
			else if (field.Equals("Elevation", StringComparison.OrdinalIgnoreCase))
			{
				return "ELEVATION (FT)";
			}
			else if (field.Equals("Latitude", StringComparison.OrdinalIgnoreCase))
			{
				return "LATITUDE (DEC. DEG.)";
			}
			else if (field.Equals("Region1", StringComparison.OrdinalIgnoreCase))
			{
				return "REGION1";
			}
			else if (field.Equals("Region2", StringComparison.OrdinalIgnoreCase))
			{
				return "REGION2";
			}
			else if (field.Equals("HeightHumidity", StringComparison.OrdinalIgnoreCase))
			{
				return "HEIGHT HUMIDITY/TEMPERATURE MEASUREMENT (FT)";
			}
			else if (field.Equals("HeightWind", StringComparison.OrdinalIgnoreCase))
			{
				return "HEIGHT WIND MEASUREMENT (FT)";
			}
		}
		else if (propType.Equals("FieldNameHeader", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "\nID";
			}
			else if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "\nNAME";
			}
			else if (field.Equals("Elevation", StringComparison.OrdinalIgnoreCase))
			{
				return "ELEVATION\n(FT)";
			}
			else if (field.Equals("Latitude", StringComparison.OrdinalIgnoreCase))
			{
				return "LATITUDE\n(DEC. DEG.)";
			}
			else if (field.Equals("Region1", StringComparison.OrdinalIgnoreCase))
			{
				return "\nREGION1";
			}
			else if (field.Equals("Region2", StringComparison.OrdinalIgnoreCase))
			{
				return "\nREGION2";
			}
			else if (field.Equals("HeightHumidity", StringComparison.OrdinalIgnoreCase))
			{
				return "HEIGHT HUMIDITY/TEMPERATURE\nMEASUREMENT (FT)";
			}
			else if (field.Equals("HeightWind", StringComparison.OrdinalIgnoreCase))
			{
				return "HEIGHT WIND\nMEASUREMENT (FT)";
			}
		}
		else if (propType.Equals("ToolTip", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("Elevation", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("Latitude", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("Region1", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("Region2", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("HeightHumidity", StringComparison.OrdinalIgnoreCase))
			{
				return "Height of humidity and temperature measurements (feet, daily analysis only)";
			}
			else if (field.Equals("HeightWind", StringComparison.OrdinalIgnoreCase))
			{
				return "Height of wind measurement (feet, daily analysis only)";
			}
		}
		else if (propType.Equals("Format", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-20.20s";
			}
			else if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "%-20.20s";
			}
			else if (field.Equals("Elevation", StringComparison.OrdinalIgnoreCase))
			{
				return "%10.2f";
			}
			else if (field.Equals("Latitude", StringComparison.OrdinalIgnoreCase))
			{
				return "%10.2f";
			}
			else if (field.Equals("Region1", StringComparison.OrdinalIgnoreCase))
			{
				return "%-20.20s";
			}
			else if (field.Equals("Region2", StringComparison.OrdinalIgnoreCase))
			{
				return "%-20.20s";
			}
			else if (field.Equals("HeightHumidity", StringComparison.OrdinalIgnoreCase))
			{
				return "%8.2f";
			}
			else if (field.Equals("HeightWind", StringComparison.OrdinalIgnoreCase))
			{
				return "%8.2f";
			}
		}
		return null;
	}

	/// <summary>
	/// Returns the property value for crop characteristics. </summary>
	/// <param name="propType"> the property to look up.  One of "FieldName", 
	/// "FieldNameHeader", "ToolTip", or "Format". </param>
	/// <param name="field"> the field for which to return the property. </param>
	/// <returns> the property, or if it could not be found null will be returned. </returns>
	private static string lookupCropCharacteristicsPropValue(string propType, string field)
	{
		if (propType.Equals("FieldName", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "NAME";
			}
			else if (field.Equals("PlantingMonth", StringComparison.OrdinalIgnoreCase))
			{
				return "PLANTING MONTH";
			}
			else if (field.Equals("PlantingDay", StringComparison.OrdinalIgnoreCase))
			{
				return "PLANTING DAY";
			}
			else if (field.Equals("HarvestMonth", StringComparison.OrdinalIgnoreCase))
			{
				return "HARVEST MONTH";
			}
			else if (field.Equals("HarvestDay", StringComparison.OrdinalIgnoreCase))
			{
				return "HARVEST DAY";
			}
			else if (field.Equals("DaysToCover", StringComparison.OrdinalIgnoreCase))
			{
				return "DAYS TO FULL COVER";
			}
			else if (field.Equals("SeasonLength", StringComparison.OrdinalIgnoreCase))
			{
				return "SEASON LENGTH";
			}
			else if (field.Equals("EarlyMoisture", StringComparison.OrdinalIgnoreCase))
			{
				return "TEMP EARLY MOISTURE (F)";
			}
			else if (field.Equals("LateMoisture", StringComparison.OrdinalIgnoreCase))
			{
				return "TEMP LATE MOISTURE (F)";
			}
			else if (field.Equals("DeficitLevel", StringComparison.OrdinalIgnoreCase))
			{
				return "MANAGEMENT ALLOWABLE DEFICIT LEVEL";
			}
			else if (field.Equals("InitialRootZone", StringComparison.OrdinalIgnoreCase))
			{
				return "INITIAL ROOT ZONE DEPTH (IN)";
			}
			else if (field.Equals("MaxRootZone", StringComparison.OrdinalIgnoreCase))
			{
				return "MAXIMUM ROOT ZONE DEPTH (IN)";
			}
			else if (field.Equals("AWC", StringComparison.OrdinalIgnoreCase))
			{
				return "AVAILABLE WATER HOLDING CAPACITY AWC (IN)";
			}
			else if (field.Equals("MAD", StringComparison.OrdinalIgnoreCase))
			{
				return "MAXIMUM APPLICATION DEPTH (IN)";
			}
			else if (field.Equals("SpringFrost", StringComparison.OrdinalIgnoreCase))
			{
				return "SPRING FROST FLAG";
			}
			else if (field.Equals("FallFrost", StringComparison.OrdinalIgnoreCase))
			{
				return "FALL FROST FLAG";
			}
			else if (field.Equals("DaysBetween1And2", StringComparison.OrdinalIgnoreCase))
			{
				return "DAYS BETWEEN 1ST AND 2ND CUT";
			}
			else if (field.Equals("DaysBetween2And3", StringComparison.OrdinalIgnoreCase))
			{
				return "DAYS BETWEEN 2ND AND 3RD CUT";
			}
		}
		else if (propType.Equals("FieldNameHeader", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "\n\n\nNAME";
			}
			else if (field.Equals("PlantingMonth", StringComparison.OrdinalIgnoreCase))
			{
				return "\n\nPLANTING\nMONTH";
			}
			else if (field.Equals("PlantingDay", StringComparison.OrdinalIgnoreCase))
			{
				return "\n\nPLANTING\nDAY";
			}
			else if (field.Equals("HarvestMonth", StringComparison.OrdinalIgnoreCase))
			{
				return "\n\nHARVEST\nMONTH";
			}
			else if (field.Equals("HarvestDay", StringComparison.OrdinalIgnoreCase))
			{
				return "\n\nHARVEST\nDAY";
			}
			else if (field.Equals("DaysToCover", StringComparison.OrdinalIgnoreCase))
			{
				return "\n\nDAYS TO\nFULL COVER";
			}
			else if (field.Equals("SeasonLength", StringComparison.OrdinalIgnoreCase))
			{
				return "\n\nSEASON\nLENGTH";
			}
			else if (field.Equals("EarlyMoisture", StringComparison.OrdinalIgnoreCase))
			{
				return "\n\nTEMP EARLY\nMOISTURE (F)";
			}
			else if (field.Equals("LateMoisture", StringComparison.OrdinalIgnoreCase))
			{
				return "\n\nTEMP LATE\nMOISTURE (F)";
			}
			else if (field.Equals("DeficitLevel", StringComparison.OrdinalIgnoreCase))
			{
				return "\nMANAGEMENT\nALLOWABLE\nDEFICIT LEVEL";
			}
			else if (field.Equals("InitialRootZone", StringComparison.OrdinalIgnoreCase))
			{
				return "\nINITIAL ROOT\nZONE DEPTH\n(IN)";
			}
			else if (field.Equals("MaxRootZone", StringComparison.OrdinalIgnoreCase))
			{
				return "\nMAXIMUM ROOT\nZONE DEPTH\n(IN)";
			}
			else if (field.Equals("AWC", StringComparison.OrdinalIgnoreCase))
			{
				return "AVAILABLE\nWATER HOLDING\nCAPACITY\nAWC (IN)";
			}
			else if (field.Equals("MAD", StringComparison.OrdinalIgnoreCase))
			{
				return "\nMAXIMUM\nAPPLICATION\nDEPTH (IN)";
			}
			else if (field.Equals("SpringFrost", StringComparison.OrdinalIgnoreCase))
			{
				return "\nSPRING\nFROST\nFLAG";
			}
			else if (field.Equals("FallFrost", StringComparison.OrdinalIgnoreCase))
			{
				return "\nFALL\nFROST\nFLAG";
			}
			else if (field.Equals("DaysBetween1And2", StringComparison.OrdinalIgnoreCase))
			{
				return "\nDAYS BETWEEN\n1ST AND 2ND\nCUT";
			}
			else if (field.Equals("DaysBetween2And3", StringComparison.OrdinalIgnoreCase))
			{
				return "\nDAYS BETWEEN\n2ND AND 3RD\nCUT";
			}
		}
		else if (propType.Equals("ToolTip", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("PlantingMonth", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("PlantingDay", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("HarvestMonth", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("HarvestDay", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("DaysToCover", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("SeasonLength", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("EarlyMoisture", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("LateMoisture", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("DeficitLevel", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("InitialRootZone", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("MaxRootZone", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("AWC", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("MAD", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("SpringFrost", StringComparison.OrdinalIgnoreCase))
			{
				return "<html>Spring frost date flag<br>0 = mean<br>1 = 28F<br>2 = 32F</html>";
			}
			else if (field.Equals("FallFrost", StringComparison.OrdinalIgnoreCase))
			{
				return "<html>Fall frost date flag<br>0 = mean<br>1 = 28F<br>2 = 32F</html>";
			}
			else if (field.Equals("DaysBetween1And2", StringComparison.OrdinalIgnoreCase))
			{
				return "<html>Days between 1st and 2nd cutting.<br>Alfalfa only.</html>";
			}
			else if (field.Equals("DaysBetween2And3", StringComparison.OrdinalIgnoreCase))
			{
				return "<html>Days between 2nd and 3rd cutting.<br>Alfalfa only.</html>";
			}
		}
		else if (propType.Equals("Format", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "%-20.20s";
			}
			else if (field.Equals("PlantingMonth", StringComparison.OrdinalIgnoreCase))
			{
				return "%8d";
			}
			else if (field.Equals("PlantingDay", StringComparison.OrdinalIgnoreCase))
			{
				return "%8d";
			}
			else if (field.Equals("HarvestMonth", StringComparison.OrdinalIgnoreCase))
			{
				return "%8d";
			}
			else if (field.Equals("HarvestDay", StringComparison.OrdinalIgnoreCase))
			{
				return "%8d";
			}
			else if (field.Equals("DaysToCover", StringComparison.OrdinalIgnoreCase))
			{
				return "%8d";
			}
			else if (field.Equals("SeasonLength", StringComparison.OrdinalIgnoreCase))
			{
				return "%8d";
			}
			else if (field.Equals("EarlyMoisture", StringComparison.OrdinalIgnoreCase))
			{
				return "%8.2f";
			}
			else if (field.Equals("LateMoisture", StringComparison.OrdinalIgnoreCase))
			{
				return "%8.2f";
			}
			else if (field.Equals("DeficitLevel", StringComparison.OrdinalIgnoreCase))
			{
				return "%8.2f";
			}
			else if (field.Equals("InitialRootZone", StringComparison.OrdinalIgnoreCase))
			{
				return "%8.2f";
			}
			else if (field.Equals("MaxRootZone", StringComparison.OrdinalIgnoreCase))
			{
				return "%8.2f";
			}
			else if (field.Equals("AWC", StringComparison.OrdinalIgnoreCase))
			{
				return "%8.2f";
			}
			else if (field.Equals("MAD", StringComparison.OrdinalIgnoreCase))
			{
				return "%8.2f";
			}
			else if (field.Equals("SpringFrost", StringComparison.OrdinalIgnoreCase))
			{
				return "%8d";
			}
			else if (field.Equals("FallFrost", StringComparison.OrdinalIgnoreCase))
			{
				return "%8d";
			}
			else if (field.Equals("DaysBetween1And2", StringComparison.OrdinalIgnoreCase))
			{
				return "%8d";
			}
			else if (field.Equals("DaysBetween2And3", StringComparison.OrdinalIgnoreCase))
			{
				return "%8d";
			}
		}

		return null;
	}

	/// <summary>
	/// Returns the property value for delay table assignments. </summary>
	/// <param name="propType"> the property to look up.  One of "FieldName", 
	/// "FieldNameHeader", "ToolTip", or "Format". </param>
	/// <param name="field"> the field for which to return the property. </param>
	/// <returns> the property, or if it could not be found null will be returned. </returns>
	private static string lookupDelayTableAssignmentPropValue(string propType, string field)
	{
		if (propType.Equals("FieldName", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "CU LOCATION ID";
			}
			else if (field.Equals("DelayTableID", StringComparison.OrdinalIgnoreCase))
			{
				return "DELAY TABLE ID";
			}
			else if (field.Equals("Percent", StringComparison.OrdinalIgnoreCase))
			{
				return "PERCENT";
			}
		}
		else if (propType.Equals("FieldNameHeader", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "CU\nLOCATION ID";
			}
			else if (field.Equals("DelayTableID", StringComparison.OrdinalIgnoreCase))
			{
				return "DELAY\nTABLE ID";
			}
			else if (field.Equals("Percent", StringComparison.OrdinalIgnoreCase))
			{
				return "\nPERCENT";
			}
		}
		else if (propType.Equals("ToolTip", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("DelayTableID", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("Percent", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
		}
		else if (propType.Equals("Format", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-20.20s";
			}
			else if (field.Equals("DelayTableID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-20.20s";
			}
			else if (field.Equals("Percent", StringComparison.OrdinalIgnoreCase))
			{
				return "%10.2f";
			}
		}

		return null;
	}

	/// <summary>
	/// Returns the property value for locations. </summary>
	/// <param name="propType"> the property to look up.  One of "FieldName", 
	/// "FieldNameHeader", "ToolTip", or "Format". </param>
	/// <param name="field"> the field for which to return the property. </param>
	/// <returns> the property, or if it could not be found null will be returned. </returns>
	private static string lookupLocationPropValue(string propType, string field)
	{
		if (propType.Equals("FieldName", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "ID";
			}
			else if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "NAME";
			}
			else if (field.Equals("Elevation", StringComparison.OrdinalIgnoreCase))
			{
				return "ELEVATION (FT)";
			}
			else if (field.Equals("Latitude", StringComparison.OrdinalIgnoreCase))
			{
				return "LATITUDE (DEC. DEG.)";
			}
			else if (field.Equals("Region1", StringComparison.OrdinalIgnoreCase))
			{
				return "REGION1";
			}
			else if (field.Equals("Region2", StringComparison.OrdinalIgnoreCase))
			{
				return "REGION2";
			}
			else if (field.Equals("NumClimateStations", StringComparison.OrdinalIgnoreCase))
			{
				return "NUMBER OF CLIMATE STATIONS";
			}
			else if (field.Equals("AWC", StringComparison.OrdinalIgnoreCase))
			{
				return "AVAILABLE WATER CONTENT, AWC (FRACTION)";
			}
		}
		else if (propType.Equals("FieldNameHeader", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "\n\n\n\nID";
			}
			else if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "\n\n\n\nNAME";
			}
			else if (field.Equals("Elevation", StringComparison.OrdinalIgnoreCase))
			{
				return "\n\n\nELEVATION\n(FT)";
			}
			else if (field.Equals("Latitude", StringComparison.OrdinalIgnoreCase))
			{
				return "\n\n\nLATITUDE\n(DEC. DEG.)";
			}
			else if (field.Equals("Region1", StringComparison.OrdinalIgnoreCase))
			{
				return "\n\n\n\nREGION1";
			}
			else if (field.Equals("Region2", StringComparison.OrdinalIgnoreCase))
			{
				return "\n\n\n\nREGION2";
			}
			else if (field.Equals("NumClimateStations", StringComparison.OrdinalIgnoreCase))
			{
				return "\n\nNUMBER OF\nCLIMATE\nSTATIONS";
			}
			else if (field.Equals("AWC", StringComparison.OrdinalIgnoreCase))
			{
				return "AVAILABLE\nWATER\nCONTENT\nAWC,\n(FRACTION)";
			}
		}
		else if (propType.Equals("ToolTip", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("Elevation", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("Latitude", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("Region1", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("Region2", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("NumClimateStations", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("AWC", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
		}
		else if (propType.Equals("Format", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("ID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-20.20s";
			}
			else if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "%-20.20s";
			}
			else if (field.Equals("Elevation", StringComparison.OrdinalIgnoreCase))
			{
				return "%8.2f";
			}
			else if (field.Equals("Latitude", StringComparison.OrdinalIgnoreCase))
			{
				return "%8.2f";
			}
			else if (field.Equals("Region1", StringComparison.OrdinalIgnoreCase))
			{
				return "%-20.20s";
			}
			else if (field.Equals("Region2", StringComparison.OrdinalIgnoreCase))
			{
				return "%-20.20s";
			}
			else if (field.Equals("NumClimateStations", StringComparison.OrdinalIgnoreCase))
			{
				return "%8d";
			}
			else if (field.Equals("AWC", StringComparison.OrdinalIgnoreCase))
			{
				return "%8.4f";
			}
		}

		return null;
	}

	/// <summary>
	/// Returns the property value for location climate stations. </summary>
	/// <param name="propType"> the property to look up.  One of "FieldName", 
	/// "FieldNameHeader", "ToolTip", or "Format". </param>
	/// <param name="field"> the field for which to return the property. </param>
	/// <returns> the property, or if it could not be found null will be returned. </returns>
	private static string lookupLocationClimateStationPropValue(string propType, string field)
	{
		if (propType.Equals("FieldName", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("LocationID", StringComparison.OrdinalIgnoreCase))
			{
				return "CU LOCATION ID";
			}
			else if (field.Equals("StationID", StringComparison.OrdinalIgnoreCase))
			{
				return "CLIMATE STATION ID";
			}
			else if (field.Equals("OrographicPrecipAdj", StringComparison.OrdinalIgnoreCase))
			{
				return "OROGRAPHIC PRECIPITATION ADJUSTMENT (FRACTION)";
			}
			else if (field.Equals("OrographicTempAdj", StringComparison.OrdinalIgnoreCase))
			{
				return "OROGRAPHIC TEMPERATURE ADJUSTMENT (DEGF/1000 FT)";
			}
			else if (field.Equals("PrecipWeight", StringComparison.OrdinalIgnoreCase))
			{
				return "PRECIPITATION STATION WEIGHT (FRACTION)";
			}
			else if (field.Equals("TempWeight", StringComparison.OrdinalIgnoreCase))
			{
				return "TEMPERATURE STATION WEIGHT (FRACTION)";
			}
		}
		else if (propType.Equals("FieldNameHeader", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("LocationID", StringComparison.OrdinalIgnoreCase))
			{
				return "\nCU\nLOCATION\nID";
			}
			else if (field.Equals("StationID", StringComparison.OrdinalIgnoreCase))
			{
				return "\nCLIMATE\nSTATION\nID";
			}
			else if (field.Equals("OrographicPrecipAdj", StringComparison.OrdinalIgnoreCase))
			{
				return "OROGRAPHIC\nPRECIPITATION\nADJUSTMENT\n(FRACTION)";
			}
			else if (field.Equals("OrographicTempAdj", StringComparison.OrdinalIgnoreCase))
			{
				return "OROGRAPHIC\nTEMPERATURE\nADJUSTMENT\n(DEGF/1000 FT)";
			}
			else if (field.Equals("PrecipWeight", StringComparison.OrdinalIgnoreCase))
			{
				return "PRECIPITATION\nSTATION\nWEIGHT\n(FRACTION)";
			}
			else if (field.Equals("TempWeight", StringComparison.OrdinalIgnoreCase))
			{
				return "TEMPERATURE\nSTATION\nWEIGHT\n(FRACTION)";
			}
		}
		else if (propType.Equals("ToolTip", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("LocationID", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("StationID", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("OrographicPrecipAdj", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("OrographicTempAdj", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("PrecipWeight", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
			else if (field.Equals("TempWeight", StringComparison.OrdinalIgnoreCase))
			{
				return "";
			}
		}
		else if (propType.Equals("Format", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("LocationID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-20.20s";
			}
			else if (field.Equals("StationID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-20.20s";
			}
			else if (field.Equals("OrographicPrecipAdj", StringComparison.OrdinalIgnoreCase))
			{
				return "%8.2f";
			}
			else if (field.Equals("OrographicTempAdj", StringComparison.OrdinalIgnoreCase))
			{
				return "%8.2f";
			}
			else if (field.Equals("PrecipWeight", StringComparison.OrdinalIgnoreCase))
			{
				return "%8.2f";
			}
			else if (field.Equals("TempWeight", StringComparison.OrdinalIgnoreCase))
			{
				return "%8.2f";
			}
		}

		return null;
	}

	/// <summary>
	/// Returns the property value for location collections. </summary>
	/// <param name="propType"> the property to look up.  One of "FieldName", 
	/// "FieldNameHeader", "ToolTip", or "Format". </param>
	/// <param name="field"> the field for which to return the property. </param>
	/// <returns> the property, or if it could not be found null will be returned. </returns>
	private static string lookupLocationCollectionPropValue(string propType, string field)
	{
		if (propType.Equals("FieldName", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("LocationID", StringComparison.OrdinalIgnoreCase))
			{
				return "CU LOCATION ID";
			}
			else if (field.Equals("Division", StringComparison.OrdinalIgnoreCase))
			{
				return "DIVISION";
			}
			else if (field.Equals("Year", StringComparison.OrdinalIgnoreCase))
			{
				return "YEAR";
			}
			else if (field.Equals("CollectionType", StringComparison.OrdinalIgnoreCase))
			{
				return "COLLECTION TYPE";
			}
			else if (field.Equals("PartType", StringComparison.OrdinalIgnoreCase))
			{
				return "PART TYPE";
			}
			else if (field.Equals("PartID", StringComparison.OrdinalIgnoreCase))
			{
				return "PART ID";
			}
			else if (field.Equals("PartIDType", StringComparison.OrdinalIgnoreCase))
			{
				return "PART ID TYPE";
			}
		}
		else if (propType.Equals("FieldNameHeader", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("LocationID", StringComparison.OrdinalIgnoreCase))
			{
				return "CU\nLOCATION\nID";
			}
			else if (field.Equals("Division", StringComparison.OrdinalIgnoreCase))
			{
				return "\n\nDIVISION";
			}
			else if (field.Equals("Year", StringComparison.OrdinalIgnoreCase))
			{
				return "\n\nYEAR";
			}
			else if (field.Equals("CollectionType", StringComparison.OrdinalIgnoreCase))
			{
				return "\nCOLLECTION\nTYPE";
			}
			else if (field.Equals("PartType", StringComparison.OrdinalIgnoreCase))
			{
				return "\nPART\nTYPE";
			}
			else if (field.Equals("PartID", StringComparison.OrdinalIgnoreCase))
			{
				return "\nPART\nID";
			}
			else if (field.Equals("PartIDType", StringComparison.OrdinalIgnoreCase))
			{
				return "\nPART\nID TYPE";
			}
		}
		else if (propType.Equals("ToolTip", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("LocationID", StringComparison.OrdinalIgnoreCase))
			{
				return "StateCU location ID for aggregate/system";
			}
			else if (field.Equals("Division", StringComparison.OrdinalIgnoreCase))
			{
				return "Water division for aggregate/system (used when aggregating using parcel IDs)";
			}
			else if (field.Equals("Year", StringComparison.OrdinalIgnoreCase))
			{
				return "Year for aggregate/system (used when aggregating parcels)";
			}
			else if (field.Equals("CollectionType", StringComparison.OrdinalIgnoreCase))
			{
				return "Aggregate (aggregate water rights) or system (consider water rights individually)";
			}
			else if (field.Equals("PartType", StringComparison.OrdinalIgnoreCase))
			{
				return "Ditch, Well, or Parcel identifiers are specified as parts of aggregate/system";
			}
			else if (field.Equals("PartID", StringComparison.OrdinalIgnoreCase))
			{
				return "The identifier for the aggregate/system parts";
			}
			else if (field.Equals("PartIDType", StringComparison.OrdinalIgnoreCase))
			{
				return "The identifier type for the aggregate/system, WDID or Receipt when applied to wells";
			}
		}
		else if (propType.Equals("Format", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("LocationID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-20.20s";
			}
			else if (field.Equals("Division", StringComparison.OrdinalIgnoreCase))
			{
				return "%8d";
			}
			else if (field.Equals("Year", StringComparison.OrdinalIgnoreCase))
			{
				return "%8d";
			}
			else if (field.Equals("CollectionType", StringComparison.OrdinalIgnoreCase))
			{
				return "%-20.20s";
			}
			else if (field.Equals("PartType", StringComparison.OrdinalIgnoreCase))
			{
				return "%-20.20s";
			}
			else if (field.Equals("PartID", StringComparison.OrdinalIgnoreCase))
			{
				return "%-20.20s";
			}
			else if (field.Equals("PartIDType", StringComparison.OrdinalIgnoreCase))
			{
				return "%-7.7s";
			}
		}

		return null;
	}

	/// <summary>
	/// Returns the property value for Penman-Monteith data. </summary>
	/// <param name="propType"> the property to look up.  One of "FieldName", "FieldNameHeader", "ToolTip", or "Format". </param>
	/// <param name="field"> the field for which to return the property. </param>
	/// <returns> the property, or if it could not be found null will be returned. </returns>
	private static string lookupPenmanMonteithPropValue(string propType, string field)
	{
		if (propType.Equals("FieldName", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "CROP NAME";
			}
			else if (field.Equals("GrowthStage", StringComparison.OrdinalIgnoreCase))
			{
				return "GROWTH STAGE";
			}
			else if (field.Equals("Percent", StringComparison.OrdinalIgnoreCase))
			{
				return "PERCENT";
			}
			else if (field.Equals("Coefficient", StringComparison.OrdinalIgnoreCase))
			{
				return "COEFFICIENT";
			}
		}
		else if (propType.Equals("FieldNameHeader", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "CROP\nNAME";
			}
			else if (field.Equals("GrowthStage", StringComparison.OrdinalIgnoreCase))
			{
				return "GROWTH\nSTAGE";
			}
			else if (field.Equals("Percent", StringComparison.OrdinalIgnoreCase))
			{
				return "\nPERCENT";
			}
			else if (field.Equals("Coefficient", StringComparison.OrdinalIgnoreCase))
			{
				return "\nCOEFFICIENT";
			}
		}
		else if (propType.Equals("ToolTip", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "Crop name";
			}
			else if (field.Equals("GrowthStage", StringComparison.OrdinalIgnoreCase))
			{
				return "<html>Growth stage, 1+.</html>";
			}
			else if (field.Equals("Percent", StringComparison.OrdinalIgnoreCase))
			{
				return "<html>Time within growth stage 0, 10, ..., 90, 100%.</html>";
			}
			else if (field.Equals("Coefficient", StringComparison.OrdinalIgnoreCase))
			{
				return "Crop coefficient";
			}
		}
		else if (propType.Equals("Format", StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return "%-20.20s";
			}
			else if (field.Equals("GrowthStage", StringComparison.OrdinalIgnoreCase))
			{
				return "%1d";
			}
			else if (field.Equals("Percent", StringComparison.OrdinalIgnoreCase))
			{
				return "%5.3f";
			}
			else if (field.Equals("Coefficient", StringComparison.OrdinalIgnoreCase))
			{
				return "%10.3f";
			}
		}
		return null;
	}

	}

}