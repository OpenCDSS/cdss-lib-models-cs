using System;
using System.Collections.Generic;
using System.IO;

// StateCU_DataSet - an object to manage data in a StateCU data set.

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
// StateCU_DataSet - an object to manage data in a StateCU data set.
//-----------------------------------------------------------------------------
// History:
//
// 2003-05-05	Steven A. Malers, RTi	Created class.
// 2003-06-04	SAM, RTi		* Rename class from CUDataSet to
//					  StateCU_DataSet.
//					* Add getTypeName().
//					* Add readXMLFile() method.
//					* Add writeXMLFile() method.
//					* Add the response file as a component.
// 2003-06-30	SAM, RTi		* Add support for component groups and
//					  group data components as per the
//					  StateCU documentation.
//					* Fold the StateCU_Control class into
//					  this class since the response and
//					  control file always go hand in hand.
//					  Separate read/write methods are still
//					  offered because the files are
//					  separate.
//					* Include iprtysm control file parameter
//					  as per Erin May 15 email.
//					* Remove obcout as per Erin July 1
//					  email.
//					* Add support for time series and other
//					  files to enable full support of data
//					  sets.
//					* Change isDirty(boolean) to setDirty(),
//					  consistent with other Java conventions
//					  (e.g., setEnabled()).
// 2003-07-13	SAM, RTi		* Extend the class from
//					  RTi.Util.IO.DataSet to allow more
//					  flexibility.
// 2004-02-19	SAM, RTi		* Update based on StateCU 4.35 data set.
//					* Use new file tags.
// 2004-03-18	SAM, RTi		* Add well pumping file, similar to
//					  historical diversions.
// 2005-01-17	JTS, RTi		Commented out StateCU_FrostTS call
//					around lines 1550 as it was causing
//					compile-time errors.
// 2005-03-29	SAM, RTi		Add sub-components for display only, and
//					handle specifically in the lookup
//					methods.  This is done similar to the
//					StateMod features.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
//-----------------------------------------------------------------------------
// EndHeader

// FIXME SAM 2009-04-29 Need to remove code related to delay tables since not used by StateCU

namespace DWR.StateCU
{

	using StateMod_DelayTable = DWR.StateMod.StateMod_DelayTable;
	using StateMod_DiversionRight = DWR.StateMod.StateMod_DiversionRight;
	using StateMod_TS = DWR.StateMod.StateMod_TS;
	using CheckFile = RTi.Util.IO.CheckFile;
	using DataSet = RTi.Util.IO.DataSet;
	using DataSetComponent = RTi.Util.IO.DataSetComponent;
	using IOUtil = RTi.Util.IO.IOUtil;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// This StateCU_DataSet class manages data components in a StateCU data set,
	/// essentially managing the list of components from the response file.  Control
	/// data are also managed in this class.
	/// Typically, each component corresponds to a file.  A list of components is
	/// maintained and is displayed by StateCU_DataSet_JTree and StateCU_DataSet_JFrame.
	/// </summary>
	public class StateCU_DataSet : DataSet
	{

	/// <summary>
	/// The StateCU data set type is unknown.
	/// </summary>
	public const int TYPE_UNKNOWN = 0;
	private static string NAME_UNKNOWN = "Unknown";

	/// <summary>
	/// The StateCU data set is for climate stations (level 1).
	/// </summary>
	public const int TYPE_CLIMATE_STATIONS = 1;
	private static string NAME_CLIMATE_STATIONS = "Climate Stations";

	/// <summary>
	/// The StateCU data set is for structures (level 2).
	/// </summary>
	public const int TYPE_STRUCTURES = 2;
	private static string NAME_STRUCTURES = "Structures";

	/// <summary>
	/// The StateCU data set is for water supply limited (level 3).
	/// </summary>
	public const int TYPE_WATER_SUPPLY_LIMITED = 3;
	private static string NAME_WATER_SUPPLY_LIMITED = "Structures (Water Supply Limited)";

	/// <summary>
	/// The StateCU data set is for water supply limited by water rights (level 4).
	/// </summary>
	public const int TYPE_WATER_SUPPLY_LIMITED_BY_WATER_RIGHTS = 4;
	private static string NAME_WATER_SUPPLY_LIMITED_BY_WATER_RIGHTS = "Structures (Water Supply Limited by Water Rights)";

	/// <summary>
	/// The StateCU data set is for river depletion (level 5).
	/// </summary>
	public const int TYPE_RIVER_DEPLETION = 5;
	private static string NAME_RIVER_DEPLETION = "Structures (River Depletion)";

	/// <summary>
	/// The StateCU data set is for other uses (use -100 to identify).
	/// </summary>
	public const int TYPE_OTHER_USES = -100;
	private static string NAME_OTHER_USES = "Other Uses";

	/// <summary>
	/// StateCU data set component types, including groups.
	/// </summary>
	public const int COMP_UNKNOWN = -1, COMP_CONTROL_GROUP = 0, COMP_RESPONSE = 1, COMP_CONTROL = 2, COMP_CLIMATE_STATIONS_GROUP = 3, COMP_CLIMATE_STATIONS = 4, COMP_TEMPERATURE_TS_MONTHLY_AVERAGE = 5, COMP_FROST_DATES_TS_YEARLY = 6, COMP_PRECIPITATION_TS_MONTHLY = 7, COMP_CROP_CHARACTERISTICS_GROUP = 8, COMP_CROP_CHARACTERISTICS = 9, COMP_BLANEY_CRIDDLE = 10, COMP_PENMAN_MONTEITH = 31, COMP_DELAY_TABLES_GROUP = 11, COMP_DELAY_TABLES_MONTHLY = 12, COMP_CU_LOCATIONS_GROUP = 13, COMP_CU_LOCATIONS = 14, COMP_CU_LOCATION_CLIMATE_STATIONS = 29, COMP_CU_LOCATION_COLLECTIONS = 30, COMP_CROP_PATTERN_TS_YEARLY = 15, COMP_IRRIGATION_PRACTICE_TS_YEARLY = 16, COMP_DIVERSION_TS_MONTHLY = 17, COMP_WELL_PUMPING_TS_MONTHLY = 18, COMP_DIVERSION_RIGHTS = 19, COMP_DELAY_TABLE_ASSIGNMENT_MONTHLY = 20, COMP_GIS_GROUP = 21, COMP_GIS_STATE = 22, COMP_GIS_DIVISIONS = 23, COMP_GIS_RIVERS = 24, COMP_GIS_CLIMATE_STATIONS = 25, COMP_GIS_STRUCTURES = 26, COMP_OTHER_GROUP = 27, COMP_OTHER = 28; // Other - no files below

	/// <summary>
	/// Component types array for the above numbers.
	/// </summary>
	private static int[] __component_types = new int[] {COMP_CONTROL_GROUP, COMP_RESPONSE, COMP_CONTROL, COMP_CLIMATE_STATIONS_GROUP, COMP_CLIMATE_STATIONS, COMP_TEMPERATURE_TS_MONTHLY_AVERAGE, COMP_FROST_DATES_TS_YEARLY, COMP_PRECIPITATION_TS_MONTHLY, COMP_CROP_CHARACTERISTICS_GROUP, COMP_CROP_CHARACTERISTICS, COMP_BLANEY_CRIDDLE, COMP_PENMAN_MONTEITH, COMP_DELAY_TABLES_GROUP, COMP_DELAY_TABLES_MONTHLY, COMP_CU_LOCATIONS_GROUP, COMP_CU_LOCATIONS, COMP_CROP_PATTERN_TS_YEARLY, COMP_IRRIGATION_PRACTICE_TS_YEARLY, COMP_DIVERSION_TS_MONTHLY, COMP_WELL_PUMPING_TS_MONTHLY, COMP_DIVERSION_RIGHTS, COMP_DELAY_TABLE_ASSIGNMENT_MONTHLY, COMP_GIS_GROUP, COMP_GIS_STATE, COMP_GIS_DIVISIONS, COMP_GIS_RIVERS, COMP_GIS_CLIMATE_STATIONS, COMP_GIS_STRUCTURES, COMP_OTHER_GROUP, COMP_OTHER, COMP_CU_LOCATION_CLIMATE_STATIONS, COMP_CU_LOCATION_COLLECTIONS};

	/// <summary>
	/// Array indicating which components are groups.
	/// </summary>
	private static int[] __component_groups = new int[] {COMP_CONTROL_GROUP, COMP_CLIMATE_STATIONS_GROUP, COMP_CROP_CHARACTERISTICS_GROUP, COMP_DELAY_TABLES_GROUP, COMP_CU_LOCATIONS_GROUP, COMP_GIS_GROUP, COMP_OTHER_GROUP};

	/// <summary>
	/// Array indicating the primary components within each component group.  The
	/// primary components are used to get the list of identifiers for displays and
	/// processing.  The number of values should agree with the list above.
	/// </summary>
	private static int[] __component_group_primaries = new int[] {COMP_RESPONSE, COMP_CLIMATE_STATIONS, COMP_CROP_CHARACTERISTICS, COMP_DELAY_TABLES_MONTHLY, COMP_CU_LOCATIONS, -1, -1};

	/// <summary>
	/// Array indicating the groups for components.
	/// </summary>
	private static int[] __component_group_assignments = new int[] {COMP_CONTROL_GROUP, COMP_CONTROL_GROUP, COMP_CONTROL_GROUP, COMP_CLIMATE_STATIONS_GROUP, COMP_CLIMATE_STATIONS_GROUP, COMP_CLIMATE_STATIONS_GROUP, COMP_CLIMATE_STATIONS_GROUP, COMP_CLIMATE_STATIONS_GROUP, COMP_CROP_CHARACTERISTICS_GROUP, COMP_CROP_CHARACTERISTICS_GROUP, COMP_CROP_CHARACTERISTICS_GROUP, COMP_CROP_CHARACTERISTICS_GROUP, COMP_DELAY_TABLES_GROUP, COMP_DELAY_TABLES_GROUP, COMP_CU_LOCATIONS_GROUP, COMP_CU_LOCATIONS_GROUP, COMP_CU_LOCATIONS_GROUP, COMP_CU_LOCATIONS_GROUP, COMP_CU_LOCATIONS_GROUP, COMP_CU_LOCATIONS_GROUP, COMP_CU_LOCATIONS_GROUP, COMP_CU_LOCATIONS_GROUP, COMP_GIS_GROUP, COMP_GIS_GROUP, COMP_GIS_GROUP, COMP_GIS_GROUP, COMP_GIS_GROUP, COMP_GIS_GROUP, COMP_OTHER_GROUP, COMP_OTHER_GROUP};

	/// <summary>
	/// Component names matching the above numbers.
	/// </summary>
	private static string[] __component_names = new string[] {"Control Data", "Response", "Control", "Climate Station Data", "Climate Stations", "Temperature TS (Monthly Average)", "Frost Dates TS (Yearly)", "Precipitation TS (Monthly)", "Crop Characteristics/Coefficient Data", "Crop Characteristics", "Blaney-Criddle Crop Coefficients", "Penman-Monteith Crop Coefficients", "Delay Table Data", "Delay Tables", "CU Location Data and Crops", "CU Locations", "Crop Pattern TS (Yearly)", "Irrigation Practice TS (Yearly)", "Diversion TS (Monthly)", "Well Pumping TS (Monthly)", "Diversion Water Rights", "Delay Assignment", "Spatial Data (GIS)", "GIS - State Boundary", "GIS - Water Divisions", "GIS - Rivers", "GIS - Climate Stations", "GIS - Structures", "Other", "Other"};

	// Subcomponent names used with lookupComponentName().  These are special
	// cases for labels and displays but the data are managed with a component
	// listed above.  Make private to force handling through lookup methods.
	private const string __COMPNAME_CU_LOCATION_CLIMATE_STATIONS = "CU Location Climate Station Assignment", __COMPNAME_CU_LOCATION_COLLECTIONS = "CU Location Collection Definitions";

	/// <summary>
	/// Component names matching the above numbers, to use for XML tags.
	/// </summary>
	/* TODO SAM 2007-03-01 Evaluate use
	private static String [] __xml_component_names = {
					"ControlData",
					  "Response",
					  "Control",
					"ClimateStationData",
					  "ClimateStations",
					  "TemperatureTS",
					  "FrostDatesTS",
					  "PrecipitationTS",
					"CropCharacteristicsData",
					  "CropCharacteristics",
					  "Blaney-CriddleCropCoefficients",
					  "Penman-MonteithCropCoefficients",
					"DelayTables",
					  "DelayTables",
					"CULocations",
					  "CULocations",
					  "CropPatternTS",
					  "IrrigationPracticeTS",
					  "DiversionTS",
					  "WellPumpingTS",
					  "DiversionWaterRights",
					  "DelayAssignment",
					"SpatialData",
					  "StateBoundary",
					  "WaterDivisions",
					  "Rivers",
					  "ClimateStations",
					  "Structures",
					"Other",
					  "Other" };
					  */
	/// <summary>
	/// Component type indicators (from response file) - group tags should never be used.
	/// </summary>
	private static string[] __component_tags = new string[] {"Control", "rcu", "ccu", "Climate Stations", "cli", "tmp", "fd", "ppt", "Crop Characteristics", "cch", "kbc", "kpm", "Delay Tables", "dly", "CU Locations", "str", "cds", "ipy", "ddh", "pvh", "ddr", "dla", "Spatial Data", "stategis", "divgis", "hydrogis", "climgis", "strucgis", "Other", "oth"};

	/// <summary>
	/// Component preferred extensions matching the above numbers.
	/// </summary>
	/* TODO SAM 2007-03-01 Evaluate use
	private static String [] __component_ext = {
					"Control",
					  "rcu",
					  "ccu",
					"Climate Stations",
					  "cli",
					  "stm",
					  "stm",
					  "stm",
					"Crop Characteristics",
					  "cch",
					  "kbc",
					"Delay Tables",
					  "dly",
					"CU Locations",
					  "str",
					  "cds",
					  "ipy",
					  "ddh",
					  "pvh",
					  "ddr",
					  "dla",
					"Spatial Data",
					  "shp",
					  "shp",
					  "shp",
					  "shp",
					  "shp",
					"Other",
					  "oth" };
					  */

	// Control file data specific to StateCU...

	private string[] __comments = null;
	private int __nyr1 = StateCU_Util.MISSING_INT;
	private int __nyr2 = StateCU_Util.MISSING_INT;
	private int __flag1 = StateCU_Util.MISSING_INT;
	private int __rn_xco = StateCU_Util.MISSING_INT;
	private int __iclim = StateCU_Util.MISSING_INT;
	private int __isupply = StateCU_Util.MISSING_INT;
	private int __sout = StateCU_Util.MISSING_INT;
	private int __ism = StateCU_Util.MISSING_INT;
	private double __pjunmo = StateCU_Util.MISSING_DOUBLE;
	private double __pothmo = StateCU_Util.MISSING_DOUBLE;
	private double __psenmo = StateCU_Util.MISSING_DOUBLE;
	private int __iprtysm = StateCU_Util.MISSING_INT;
	private int __typout = StateCU_Util.MISSING_INT;
	private int __iflood = StateCU_Util.MISSING_INT;
	private int __ddcsw = StateCU_Util.MISSING_INT;
	private int __idaily = StateCU_Util.MISSING_INT;
	private double __admin_num = StateCU_Util.MISSING_DOUBLE;

	/// <summary>
	/// Construct a blank data set.  It is expected that other information will be set
	/// during further processing.  Component groups are not initialized until a data set type is set.
	/// </summary>
	public StateCU_DataSet() : base(__component_types, __component_names, __component_groups, __component_group_assignments, __component_group_primaries)
	{ // Pass the arrays of information about the data set to the base class
		// so general methods will work...


		// Initialize data specific to the StateCU data set...
		__comments = new string[3];
		__comments[0] = StateCU_Util.MISSING_STRING;
		__comments[1] = StateCU_Util.MISSING_STRING;
		__comments[2] = StateCU_Util.MISSING_STRING;

		initializeComponentGroups();
	}

	/// <summary>
	/// Construct a blank data set.  Specific output files, by default, will use the
	/// output directory and base file name in output file names. </summary>
	/// <param name="type"> Data set type. </param>
	/// <param name="dataset_dir"> Data set directory. </param>
	/// <param name="basename"> Basename for files (no directory). </param>
	/// <exception cref="Exception"> if there is an error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public StateCU_DataSet(int type, String dataset_dir, String basename) throws Exception
	public StateCU_DataSet(int type, string dataset_dir, string basename) : base(__component_types, __component_names, __component_groups, __component_group_assignments, __component_group_primaries)
	{ // Pass the arrays of information about the data set to the base class
		// so general methods will work...


		string routine = "StateCU_DataSet";
		setDataSetType(type, true);
		setDataSetDirectory(dataset_dir);
		setBaseName(basename);

		__comments = new string[3];
		__comments[0] = StateCU_Util.MISSING_STRING;
		__comments[1] = StateCU_Util.MISSING_STRING;
		__comments[2] = StateCU_Util.MISSING_STRING;

		// Every data set type gets a response and control file...

		try
		{
			initializeComponentGroups();

			getComponentForComponentType(COMP_CONTROL_GROUP).addComponent(new DataSetComponent(this, COMP_RESPONSE));
			getComponentForComponentType(COMP_CONTROL_GROUP).addComponent(new DataSetComponent(this, COMP_CONTROL));
		}
		catch (Exception e)
		{
			// Should not happen...
			Message.printWarning(2, routine, e);
		}

		// Other uses

		if (type == TYPE_OTHER_USES)
		{
			// TODO - add later.
		}
		else
		{
			// Every data set has...
			try
			{
				getComponentForComponentType(COMP_CLIMATE_STATIONS_GROUP).addComponent(new DataSetComponent(this, COMP_CLIMATE_STATIONS));
				getComponentForComponentType(COMP_CLIMATE_STATIONS_GROUP).addComponent(new DataSetComponent(this, COMP_TEMPERATURE_TS_MONTHLY_AVERAGE));
				getComponentForComponentType(COMP_CLIMATE_STATIONS_GROUP).addComponent(new DataSetComponent(this, COMP_FROST_DATES_TS_YEARLY));
				getComponentForComponentType(COMP_CLIMATE_STATIONS_GROUP).addComponent(new DataSetComponent(this, COMP_PRECIPITATION_TS_MONTHLY));
				getComponentForComponentType(COMP_CROP_CHARACTERISTICS_GROUP).addComponent(new DataSetComponent(this, COMP_CROP_CHARACTERISTICS));
				getComponentForComponentType(COMP_CROP_CHARACTERISTICS_GROUP).addComponent(new DataSetComponent(this, COMP_BLANEY_CRIDDLE));
				getComponentForComponentType(COMP_CROP_CHARACTERISTICS_GROUP).addComponent(new DataSetComponent(this, COMP_PENMAN_MONTEITH));
			}
			catch (Exception e)
			{
				// Should not happen...
				Message.printWarning(2, routine, e);
			}
		}

		if (type == TYPE_RIVER_DEPLETION)
		{
			// Want this before CU Locations...
			try
			{
				getComponentForComponentType(COMP_DELAY_TABLES_GROUP).addComponent(new DataSetComponent(this, COMP_DELAY_TABLES_MONTHLY));
			}
			catch (Exception e)
			{
				// Should not happen...
				Message.printWarning(2, routine, e);
			}
		}

		if (type != TYPE_OTHER_USES)
		{
			// Add CU Locations...
			try
			{
				getComponentForComponentType(COMP_CU_LOCATIONS_GROUP).addComponent(new DataSetComponent(this, COMP_CU_LOCATIONS));
			}
			catch (Exception e)
			{
				// Should not happen...
				Message.printWarning(2, routine, e);
			}
		}

		if (type >= TYPE_STRUCTURES)
		{
			try
			{
				getComponentForComponentType(COMP_CU_LOCATIONS_GROUP).addComponent(new DataSetComponent(this, COMP_CROP_PATTERN_TS_YEARLY));
			}
			catch (Exception e)
			{
				// Should not happen...
				Message.printWarning(2, routine, e);
			}
		}
		if (type >= TYPE_WATER_SUPPLY_LIMITED)
		{
			try
			{
				getComponentForComponentType(COMP_CU_LOCATIONS_GROUP).addComponent(new DataSetComponent(this, COMP_IRRIGATION_PRACTICE_TS_YEARLY));
				getComponentForComponentType(COMP_CU_LOCATIONS_GROUP).addComponent(new DataSetComponent(this, COMP_DIVERSION_TS_MONTHLY));
				getComponentForComponentType(COMP_CU_LOCATIONS_GROUP).addComponent(new DataSetComponent(this, COMP_WELL_PUMPING_TS_MONTHLY));
			}
			catch (Exception e)
			{
				// Should not happen...
				Message.printWarning(2, routine, e);
			}
		}
		if (type >= TYPE_WATER_SUPPLY_LIMITED_BY_WATER_RIGHTS)
		{
			try
			{
				getComponentForComponentType(COMP_CU_LOCATIONS_GROUP).addComponent(new DataSetComponent(this, COMP_DIVERSION_RIGHTS));
			}
			catch (Exception e)
			{
				// Should not happen...
				Message.printWarning(2, routine, e);
			}
		}
		if (type == TYPE_RIVER_DEPLETION)
		{
			try
			{
				getComponentForComponentType(COMP_DELAY_TABLES_GROUP).addComponent(new DataSetComponent(this, COMP_DELAY_TABLE_ASSIGNMENT_MONTHLY));
			}
			catch (Exception e)
			{
				// Should not happen...
				Message.printWarning(2, routine, e);
			}
		}
	}

	/// <summary>
	/// Finalize before garbage collection. </summary>
	/// <exception cref="Throwable"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~StateCU_DataSet()
	{
		__comments = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Return the administration number for CU by priority. </summary>
	/// <returns> the administration number for CU by priority. </returns>
	public virtual double getAdminNumForCUByPriority()
	{
		return __admin_num;
	}

	/// <summary>
	/// Return a comment for the data set. </summary>
	/// <returns> a comment for the data set. </returns>
	/// <param name="pos"> Comment index (0 - 2). </param>
	public virtual string getComment(int pos)
	{
		if ((pos < 0) || (pos > 2))
		{
			return "";
		}
		return __comments[pos];
	}

	/// <summary>
	/// Return the data set type name.  This method calls lookupDataSetName() for the instance. </summary>
	/// <returns> the data set type name. </returns>
	public virtual string getDataSetName()
	{
		return lookupDataSetName(getDataSetType());
	}

	/// <summary>
	/// Return whether to create StateMod output. </summary>
	/// <returns> whether to create StateMod output. </returns>
	public virtual int getDdcsw()
	{
		return __ddcsw;
	}

	/// <summary>
	/// Return the value of flag1 (CU method). </summary>
	/// <returns> the value of flag1. </returns>
	public virtual int getFlag1()
	{
		return __flag1;
	}

	/// <summary>
	/// Return the value of iclim (climate stations or structures data set type). </summary>
	/// <returns> the value of iclim. </returns>
	public virtual int getIclim()
	{
		return __iclim;
	}

	/// <summary>
	/// Return the value of idaily (daily/monthly switch). </summary>
	/// <returns> the value of idaily. </returns>
	public virtual int getIdaily()
	{
		return __idaily;
	}

	/// <summary>
	/// Return the value of iflood (groundwater use switch). </summary>
	/// <returns> the value of iflood. </returns>
	public virtual int getIflood()
	{
		return __iflood;
	}

	/// <summary>
	/// Return the value of iprtysm (operate soil moisture by proration). </summary>
	/// <returns> the value of iprtysm. </returns>
	public virtual int getIprtysm()
	{
		return __iprtysm;
	}

	/// <summary>
	/// Return the value of ism (soil moisture flag). </summary>
	/// <returns> the value of ism. </returns>
	public virtual int getIsm()
	{
		return __ism;
	}

	/// <summary>
	/// Return the value of isupply (water supply option). </summary>
	/// <returns> the value of isupply. </returns>
	public virtual int getIsupply()
	{
		return __isupply;
	}

	/// <summary>
	/// Return the starting year for the data set. </summary>
	/// <returns> the starting year for the data set. </returns>
	public virtual int getNyr1()
	{
		return __nyr1;
	}

	/// <summary>
	/// Return the ending year for the data set. </summary>
	/// <returns> the ending year for the data set. </returns>
	public virtual int getNyr2()
	{
		return __nyr2;
	}

	/// <summary>
	/// Return the initial soil moisture content for junior parcels (fraction of capacity). </summary>
	/// <returns> the initial soil moisture content for junior parcels. </returns>
	public virtual double getPjunmo()
	{
		return __pjunmo;
	}

	/// <summary>
	/// Return the initial soil moisture content for other parcels (fraction of capacity). </summary>
	/// <returns> the initial soil moisture content for other parcels. </returns>
	public virtual double getPothmo()
	{
		return __pothmo;
	}

	/// <summary>
	/// Return the initial soil moisture content for senior parcels (fraction of capacity). </summary>
	/// <returns> the initial soil moisture content for senior parcels. </returns>
	public virtual double getPsenmo()
	{
		return __psenmo;
	}

	/// <summary>
	/// Return the value of rn_xco (monthly precipitation method). </summary>
	/// <returns> the value of rn_xco. </returns>
	public virtual int getRn_xco()
	{
		return __rn_xco;
	}

	/// <summary>
	/// Return the value of sout (input summary type). </summary>
	/// <returns> the value of sout. </returns>
	public virtual int getSout()
	{
		return __sout;
	}

	/// <summary>
	/// Return the value of typout (output summary detail). </summary>
	/// <returns> the value of typout. </returns>
	public virtual int getTypout()
	{
		return __typout;
	}

	/// <summary>
	/// Initialize the component groups.  This is usually done immediately after
	/// the data set type is known (the data set type must be set).
	/// The list source for each group is set to DataSetComponent.LIST_SOURCE_PRIMARY_COMPONENT. </summary>
	/// <exception cref="Exception"> if there is an error initializing the component groups. </exception>
	private void initializeComponentGroups()
	{
		string routine = this.GetType().Name + ".initializeComponentGroups";
		// Always add the control group...
		DataSetComponent comp, subcomp = null;
		try
		{
			comp = new DataSetComponent(this, COMP_CONTROL_GROUP);

			comp.setListSource(DataSetComponent.LIST_SOURCE_PRIMARY_COMPONENT);
			addComponent(comp);
			int type = getDataSetType();
			if (type == TYPE_OTHER_USES)
			{
				comp = new DataSetComponent(this, COMP_OTHER_GROUP);
				comp.setListSource(DataSetComponent.LIST_SOURCE_PRIMARY_COMPONENT);
				addComponent(comp);
			}
			else
			{
				// TODO KAT 2007-04-12
				// Need to evaluate whether these are subgroups or not
				comp = new DataSetComponent(this, COMP_BLANEY_CRIDDLE);
				comp.setListSource(DataSetComponent.LIST_SOURCE_PRIMARY_COMPONENT);
				addComponent(comp);
				comp = new DataSetComponent(this, COMP_PENMAN_MONTEITH);
				comp.setListSource(DataSetComponent.LIST_SOURCE_PRIMARY_COMPONENT);
				addComponent(comp);
				comp = new DataSetComponent(this, COMP_CROP_PATTERN_TS_YEARLY);
				comp.setListSource(DataSetComponent.LIST_SOURCE_PRIMARY_COMPONENT);
				addComponent(comp);
				comp = new DataSetComponent(this, COMP_IRRIGATION_PRACTICE_TS_YEARLY);
				comp.setListSource(DataSetComponent.LIST_SOURCE_PRIMARY_COMPONENT);
				addComponent(comp);
				comp = new DataSetComponent(this, COMP_DELAY_TABLES_MONTHLY);
				comp.setListSource(DataSetComponent.LIST_SOURCE_PRIMARY_COMPONENT);
				addComponent(comp);
				comp = new DataSetComponent(this, COMP_DELAY_TABLE_ASSIGNMENT_MONTHLY);
				comp.setListSource(DataSetComponent.LIST_SOURCE_PRIMARY_COMPONENT);
				addComponent(comp);
				/////////////////

				comp = new DataSetComponent(this,COMP_CLIMATE_STATIONS_GROUP);
				comp.setListSource(DataSetComponent.LIST_SOURCE_PRIMARY_COMPONENT);
				addComponent(comp);
				subcomp = new DataSetComponent(this, COMP_CLIMATE_STATIONS);
				subcomp.setData(new List<StateCU_ClimateStation>());
				comp.addComponent(subcomp);
				// TODO KAT 2007-04-12 
				// add extra subcomponents for climate stations here
				// need to figure out all which components go here

				comp = new DataSetComponent(this, COMP_CROP_CHARACTERISTICS_GROUP);
				comp.setListSource(DataSetComponent.LIST_SOURCE_PRIMARY_COMPONENT);
				addComponent(comp);
				addComponent(comp);
				subcomp = new DataSetComponent(this, COMP_CROP_CHARACTERISTICS);
				subcomp.setData(new List<StateCU_CropCharacteristics>());
				comp.addComponent(subcomp);
				// TODO KAT 2007-04-12 need to figure out all subcomponents which go here

				if (type == TYPE_RIVER_DEPLETION)
				{
					comp = new DataSetComponent(this, COMP_DELAY_TABLES_GROUP);
					comp.setListSource(DataSetComponent.LIST_SOURCE_PRIMARY_COMPONENT);
					addComponent(comp);
					subcomp = new DataSetComponent(this, COMP_DELAY_TABLES_MONTHLY);
					subcomp.setData(new List<StateCU_DelayTable>());
					comp.addComponent(subcomp);
					subcomp = new DataSetComponent(this, COMP_DELAY_TABLE_ASSIGNMENT_MONTHLY);
					subcomp.setData(new List<StateCU_DelayTableAssignment>());
					comp.addComponent(subcomp);
				}
				comp = new DataSetComponent(this, COMP_CU_LOCATIONS_GROUP);
				comp.setListSource(DataSetComponent.LIST_SOURCE_PRIMARY_COMPONENT);
				addComponent(comp);
				subcomp = new DataSetComponent(this, COMP_CU_LOCATIONS);
				subcomp.setData(new List<StateCU_Location>());
				comp.addComponent(subcomp);
				subcomp = new DataSetComponent(this, COMP_CU_LOCATION_CLIMATE_STATIONS);
				subcomp.setData(new List<>());
				comp.addComponent(subcomp);
				subcomp = new DataSetComponent(this, COMP_CU_LOCATION_COLLECTIONS);
				subcomp.setData(new List<>());
				comp.addComponent(subcomp);
			}
			// Always add GIS a separate group...
			comp = new DataSetComponent(this, COMP_GIS_GROUP);
			comp.setListSource(DataSetComponent.LIST_SOURCE_PRIMARY_COMPONENT);
			addComponent(comp);
		}
		catch (Exception e)
		{
			Message.printWarning(3, routine, e);
		}
	}

	/// <summary>
	/// Returns the name of the specified component.  Subcomponents (e.g., CU location
	/// climate station assignment) are specifically checked and then the base class method is called. </summary>
	/// <param name="comp_type"> the component type integer. </param>
	/// <returns> the name of the specified component. </returns>
	public virtual string lookupComponentName(int comp_type)
	{
		if (comp_type == COMP_CU_LOCATION_CLIMATE_STATIONS)
		{
			return __COMPNAME_CU_LOCATION_CLIMATE_STATIONS;
		}
		else if (comp_type == COMP_CU_LOCATION_COLLECTIONS)
		{
			return __COMPNAME_CU_LOCATION_COLLECTIONS;
		}
		else
		{
			return base.lookupComponentName(comp_type);
		}
	}

	/// <summary>
	/// Determine the data set type from a string. </summary>
	/// <param name="type"> Data set type as a string. </param>
	/// <returns> the data set type as an integer or -1 if not found. </returns>
	public static int lookupDataSetType(string type)
	{
		if (type.Equals(NAME_UNKNOWN, StringComparison.OrdinalIgnoreCase))
		{
			return TYPE_UNKNOWN;
		}
		else if (type.Equals(NAME_CLIMATE_STATIONS, StringComparison.OrdinalIgnoreCase))
		{
			return TYPE_CLIMATE_STATIONS;
		}
		else if (type.Equals(NAME_STRUCTURES, StringComparison.OrdinalIgnoreCase))
		{
			return TYPE_STRUCTURES;
		}
		else if (type.Equals(NAME_WATER_SUPPLY_LIMITED, StringComparison.OrdinalIgnoreCase))
		{
			return TYPE_WATER_SUPPLY_LIMITED;
		}
		else if (type.Equals(NAME_WATER_SUPPLY_LIMITED_BY_WATER_RIGHTS, StringComparison.OrdinalIgnoreCase))
		{
			return TYPE_WATER_SUPPLY_LIMITED_BY_WATER_RIGHTS;
		}
		else if (type.Equals(NAME_RIVER_DEPLETION, StringComparison.OrdinalIgnoreCase))
		{
			return TYPE_RIVER_DEPLETION;
		}
		else if (type.Equals(NAME_OTHER_USES, StringComparison.OrdinalIgnoreCase))
		{
			return TYPE_OTHER_USES;
		}
		else
		{
			return -1;
		}
	}

	/// <summary>
	/// Return the data set type name.  This is suitable for warning messages and simple output. </summary>
	/// <param name="dataset_type"> Data set type (see TYPE_*). </param>
	/// <returns> the data set type name. </returns>
	public static string lookupDataSetName(int dataset_type)
	{
		if (dataset_type == TYPE_UNKNOWN)
		{
			return NAME_UNKNOWN;
		}
		else if (dataset_type == TYPE_CLIMATE_STATIONS)
		{
			return NAME_CLIMATE_STATIONS;
		}
		else if (dataset_type == TYPE_STRUCTURES)
		{
			return NAME_STRUCTURES;
		}
		else if (dataset_type == TYPE_WATER_SUPPLY_LIMITED)
		{
			return NAME_WATER_SUPPLY_LIMITED;
		}
		else if (dataset_type == TYPE_WATER_SUPPLY_LIMITED_BY_WATER_RIGHTS)
		{
			return NAME_WATER_SUPPLY_LIMITED_BY_WATER_RIGHTS;
		}
		else if (dataset_type == TYPE_RIVER_DEPLETION)
		{
			return NAME_RIVER_DEPLETION;
		}
		else if (dataset_type == TYPE_OTHER_USES)
		{
			return NAME_OTHER_USES;
		}
		else
		{
			return "";
		}
	}

	/// <summary>
	/// Return the numeric component type given its string tag, from the StateCU
	/// response file.  This method is called when reading the StateCU response file. </summary>
	/// <returns> the numeric component type given its string type, or -1 if not found. </returns>
	/// <param name="component_tag"> the component tag from the response file. </param>
	public static int lookupComponentTypeFromTag(string component_tag)
	{
		for (int i = 0; i < __component_tags.Length; i++)
		{
			if (__component_tags[i].Equals(component_tag, StringComparison.OrdinalIgnoreCase))
			{
				return i;
			}
		}
		return -1;
	}

	/// <summary>
	/// Construct a DataSetComponent object from a response file string.  The string should be of the form:
	/// <pre>
	/// file.ext, type
	/// </pre> </summary>
	/// <param name="dataset"> The StateCU_DataSet for the component. </param>
	/// <param name="line"> Non-comment data line from response file. </param>
	/// <exception cref="Exception"> if there is an error creating the object. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static RTi.Util.IO.DataSetComponent parseDataSetComponent(StateCU_DataSet dataset, String line) throws Exception
	public static DataSetComponent parseDataSetComponent(StateCU_DataSet dataset, string line)
	{
		IList<string> v = StringUtil.breakStringList(line, ", \t", StringUtil.DELIM_SKIP_BLANKS);
		if ((v == null) || (v.Count < 2))
		{
			throw new Exception("Bad StateCU data (\"" + line + "\"");
		}
		string type_string = (string)v[1];
		int comptype = lookupComponentTypeFromTag(type_string);
		if (comptype < 0)
		{
			throw new Exception("Unrecognized type in (\"" + line + "\"");
		}
		DataSetComponent comp = new DataSetComponent(dataset, comptype);
		comp.setDataFileName(v[0]);
		return comp;
	}

	/// <summary>
	/// Process an XML Document node during the read process. </summary>
	/// <param name="dataset"> StateCU_DataSet that is being read. </param>
	/// <param name="node"> an XML document node, which may have children. </param>
	/// <exception cref="Exception"> if there is an error processing the node. </exception>
	/* TODO SAM 2007-03-01 Evaluate if in the base class
	private static void processDocumentNodeForRead ( StateCU_DataSet dataset, Node node )
	throws Exception
	{	String routine = "StateCU_DataSet.processDocumentNodeForRead";
		/ * REVISIT - need to figure out if this is in the base class
		switch ( node.getNodeType() ) {
			case Node.DOCUMENT_NODE:
				// The main data set node.  Get the data set type, etc.
				processDocumentNodeForRead( dataset, ((Document)node).getDocumentElement() );
				break;
			case Node.ELEMENT_NODE:
				// Data set components.  Print the basic information...
				String element_name = node.getNodeName();
				Message.printStatus ( 1, routine, "Element name: " + element_name );
				NamedNodeMap attributes;
				Node attribute_Node;
				String attribute_name, attribute_value;
				// Evaluate the nodes attributes...
				if ( element_name.equalsIgnoreCase("StateCU_DataSet") ){
					attributes = node.getAttributes();
					int nattributes = attributes.getLength();
					for ( int i = 0; i < nattributes; i++ ) {
						attribute_Node = attributes.item(i);
						attribute_name = attribute_Node.getNodeName();
						if ( attribute_name.equalsIgnoreCase( "Type" ) ) {
							try {
								dataset.setType ( attribute_Node.getNodeValue(), true );
							}
							catch ( Exception e ) {
								Message.printWarning ( 2, routine, "Data set type \"" + attribute_name + "\" is"
								+ " not recognized." );
								throw new Exception ( "Error processing data set" );
							}
						}
						else if (
							attribute_name.equalsIgnoreCase( "BaseName" ) ) {
							dataset.setBaseName ( attribute_Node.getNodeValue() );
						}
					}
				}
				else if ( element_name.equalsIgnoreCase("DataSetComponent") ) {
					attributes = node.getAttributes();
					int nattributes = attributes.getLength();
					String comptype = "", compdatafile = "", complistfile = "", compcommandsfile ="";
					for ( int i = 0; i < nattributes; i++ ) {
						attribute_Node = attributes.item(i);
						attribute_name = attribute_Node.getNodeName();
						attribute_value = attribute_Node.getNodeValue();
						if ( attribute_name.equalsIgnoreCase( "Type" ) ) {
							comptype = attribute_value;
						}
						else if(attribute_name.equalsIgnoreCase( "DataFile" ) ) {
							compdatafile = attribute_value;
						}
						else if(attribute_name.equalsIgnoreCase( "ListFile" ) ) {
							complistfile = attribute_value;
						}
						else if(attribute_name.equalsIgnoreCase( "CommandsFile" ) ) {
							compcommandsfile = attribute_value;
						}
						else {
							Message.printWarning ( 2, routine, "Unrecognized " +
							"attribute \"" + attribute_name + " for \"" + element_name +"\"");
						}
					}
					int component_type = DataSetComponent.lookupComponentType( comptype );
					if ( component_type < 0 ) {
						Message.printWarning ( 2, routine,
						"Unrecognized data set component \"" + comptype + "\".  Skipping." );
						return;
					}
					// Add the component...
					DataSetComponent comp = new DataSetComponent ( this, component_type );
					comp.setDataFileName ( compdatafile );
					comp.setListFileName ( complistfile );
					comp.setCommandsFileName ( compcommandsfile );
					Message.printStatus ( 1, routine, "Adding new component for data \"" + compdatafile + "\" \"" );
					dataset.addComponent ( comp );
				}
				// The main document node will have a list of children
				// (data set components) but components will not.
				// Recursively process each node...
				NodeList children = node.getChildNodes();
				if ( children != null ) {
					int len = children.getLength();
					for ( int i = 0; i < len; i++ ) {
						processDocumentNodeForRead ( dataset, children.item(i) );
					}
				}
				break;
		}
	}
	*/

	/// <summary>
	/// Read the StateCU control file and handle data in the StateCU_DataSet instance.
	/// This method is usually only called by readStateCUFile(), which reads the StateCU response file. </summary>
	/// <param name="filename"> filename containing StateCU control data. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void readStateCUControlFile(String filename) throws java.io.IOException
	private void readStateCUControlFile(string filename)
	{
		string rtn = "StateCU_DataSet.readStateCUControlFile";
		string iline = null;
		StreamReader @in = null;

		Message.printStatus(1, rtn, "Reading StateCU control file: \"" + filename + "\"");
		// The following throws an IOException if the file cannot be opened...
		@in = new StreamReader(filename);
		string @string;
		string token1; // First token on a line
		int datarec = -1; // Counter for data records (not comments).
		IList<object> v = null;
		// Use a while to check for comments.
		while (!string.ReferenceEquals((iline = @in.ReadLine()), null))
		{
			// check for comments
			if (iline.StartsWith("#", StringComparison.Ordinal) || iline.Trim().Length == 0)
			{
				continue;
			}
			++datarec;
			@string = iline.Trim();
			token1 = StringUtil.getToken(@string, " \t", StringUtil.DELIM_SKIP_BLANKS, 0);
			if ((datarec >= 0) && (datarec <= 2))
			{
				setComment(@string, datarec);
			}
			else if (datarec == 3)
			{
				IList<string> v2 = StringUtil.breakStringList(@string, " \t", StringUtil.DELIM_SKIP_BLANKS);
				if (v2 != null)
				{
					if ((v2.Count >= 1) && StringUtil.isInteger(v2[0]))
					{
						setNyr1(StringUtil.atoi(v2[0]));
					}
					if ((v2.Count >= 2) && StringUtil.isInteger(v2[1]))
					{
						setNyr2(StringUtil.atoi(v2[1]));
					}
				}
			}
			else if (datarec == 4)
			{
				setFlag1(StringUtil.atoi(token1));
			}
			else if (datarec == 5)
			{
				setRn_xco(StringUtil.atoi(token1));
			}
			else if (datarec == 6)
			{
				setIclim(StringUtil.atoi(token1));
			}
			else if (datarec == 7)
			{
				setIsupply(StringUtil.atoi(token1));
			}
			else if (datarec == 8)
			{
				setSout(StringUtil.atoi(token1));
			}
			else if (datarec == 9)
			{
				setIsm(StringUtil.atoi(token1));
			}
			else if (datarec == 10)
			{
				v = StringUtil.fixedRead(iline, "s5s5s5");
				if (v != null)
				{
					if ((v.Count >= 1) && StringUtil.isDouble(((string)v[0]).Trim()))
					{
						setPsenmo(StringUtil.atod(((string)v[0]).Trim()));
					}
					if ((v.Count >= 2) && StringUtil.isDouble(((string)v[1]).Trim()))
					{
						setPjunmo(StringUtil.atod(((string)v[1]).Trim()));
					}
					if ((v.Count >= 3) && StringUtil.isDouble(((string)v[2]).Trim()))
					{
						setPothmo(StringUtil.atod(((string)v[2]).Trim()));
					}
				}
			}
			else if (datarec == 11)
			{
				setIprtysm(StringUtil.atoi(token1));
			}
			else if (datarec == 12)
			{
				setTypout(StringUtil.atoi(token1));
			}
			else if (datarec == 13)
			{
				setIflood(StringUtil.atoi(token1));
			}
			else if (datarec == 14)
			{
				setDdcsw(StringUtil.atoi(token1));
			}
			else if (datarec == 15)
			{
				setIdaily(StringUtil.atoi(token1));
			}
			else if (datarec == 16)
			{
				setAdminNumForCUByPriority(StringUtil.atod(token1));
			}
		}
		if (@in != null)
		{
			@in.Close();
		}
	}

	/// <summary>
	/// Read the StateCU response file and return a StateCU_DataSet object. </summary>
	/// <param name="filename"> StateCU response file. </param>
	/// <param name="read_all"> If true, all the data files mentioned in the response file will
	/// be read into memory, providing a complete data set for viewing and manipulation. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static StateCU_DataSet readStateCUFile(String filename,boolean read_all) throws Exception
	public static StateCU_DataSet readStateCUFile(string filename, bool read_all)
	{
		string routine = "StateCU_DataSet.readStateCUFile";
		string iline = null;
		StreamReader @in = null;
		Message.printStatus(1, routine, "Reading StateCU response file: " + filename);

		// Set the data set directory to be used when opening the component files...

		string full_filename = IOUtil.getPathUsingWorkingDir(filename);
		File f = new File(full_filename);

		// Declare the data set of the appropriate type (the data set type is
		// handled below after reading the control file)...  

		StateCU_DataSet dataset = new StateCU_DataSet();
		dataset.setDataSetDirectory(f.getParent());
		dataset.setDataSetFileName(f.getName());

		// Add a data set component for the response and control files...

		DataSetComponent response_comp = new DataSetComponent(dataset, COMP_RESPONSE);
		response_comp.setDataFileName(f.getName());

		// The following throws an IOException if the file cannot be opened...
		@in = new StreamReader(filename);

		DataSetComponent comp = null;
		int comptype; // Component type
		string compfile; // Data file for component
		int iclim = 0;
		int isupply = 0;
		string read_warning = "";
		string unneeded_warning = "";

		// First read in the entire response file.  This allows the control
		// file to be determined and read at once, which then allows the data
		// set to be initialized properly (groups, etc.).  This is also necessary
		// because the control file is not guaranteed to be the first file specified.

		IList<string> rcu_strings = new List<string>();
		while (!string.ReferenceEquals((iline = @in.ReadLine()), null))
		{
			// check for comments
			iline = iline.Trim();
			if (iline.StartsWith("#", StringComparison.Ordinal) || (iline.Length == 0))
			{
				continue;
			}
			rcu_strings.Add(iline);
		}

		// Read the control file...

		int size = rcu_strings.Count;
		int dataset_type = TYPE_UNKNOWN;
		DataSetComponent control_comp = null;
		for (int i = 0; i < size; i++)
		{
			try
			{
				comp = parseDataSetComponent(dataset, rcu_strings[i]);
				// The following are set in the above parse method...
				comptype = comp.getComponentType();
				compfile = comp.getDataFileName();
				if (comptype != COMP_CONTROL)
				{
					continue;
				}
				// Else, read the control file...
				// Save to add to the data set below...
				control_comp = comp;
				// Always read this because it specifies configuration information...
				f = new File(compfile);
				if (!f.isAbsolute())
				{
					compfile = dataset.getDataSetDirectory() + File.separator + compfile;
				}
				try
				{
					// Previously, the StateCU_Control object was used to store the control
					// data but now the data are in the StateCU_DataSet object itself so declare
					// the dataset object here...

					// Now read the control file.
					comp.setData(dataset);
					dataset.readStateCUControlFile(compfile);
					iclim = dataset.getIclim();
					isupply = dataset.getIsupply();
					Message.printStatus(1, routine, "iclim=" + iclim + " isupply=" + isupply);
					if (iclim == 0)
					{
						dataset_type = TYPE_CLIMATE_STATIONS;
						Message.printStatus(1, routine, "Reading climate stations data set");
					}
					// A structures data set - check the supply flag to know what type of data set...
					else if (isupply == 0)
					{
						dataset_type = TYPE_STRUCTURES;
						Message.printStatus(1, routine, "Reading structures data set");
					}
					else if (isupply == 1)
					{
						dataset_type = TYPE_WATER_SUPPLY_LIMITED;
						Message.printStatus(1, routine, "Reading structures (water supply limited) data set");
					}
					else if (isupply == 2)
					{
						dataset_type = TYPE_WATER_SUPPLY_LIMITED_BY_WATER_RIGHTS;
						Message.printStatus(1, routine, "Reading structures (water supply limited by water rights) data set");
					}
					else if (isupply == 3)
					{
						dataset_type = TYPE_RIVER_DEPLETION;
						Message.printStatus(1, routine, "Reading structures (river depletion) data set");
					}
				}
				catch (Exception e2)
				{
					Message.printWarning(1, routine, "Error reading data for:\n" + "\"" + compfile + "\"");
					Message.printWarning(2, routine, e2);
				}
			}
			catch (Exception)
			{
				// Ignore bad components...
				continue;
			}
		}

		// Initialize the data set components using the data set type...

		dataset.setDataSetType(dataset_type, true);
		// Reinitialize the groups now that we know the data set type...
		dataset.initializeComponentGroups();

		// Add the response and control file components...

		dataset.getComponentForComponentType(COMP_CONTROL_GROUP).addComponent(response_comp);
		dataset.getComponentForComponentType(COMP_CONTROL_GROUP).addComponent(control_comp);

		// Now loop through the remaining components...

		for (int i = 0; i < size; i++)
		{
			iline = (string)rcu_strings[i];
			// Allocate new DataSetComponent instance...
			try
			{
				try
				{
					comp = parseDataSetComponent(dataset,iline);
				}
				catch (Exception e2)
				{
					Message.printWarning(1, routine, "Unrecognized StateCU data component for:\n\"" + iline + "\"");
					Message.printWarning(2, routine, e2);
					continue;
				}
				// The following are set in the constructor for DataSetComponent...
				comptype = comp.getComponentType();
				compfile = comp.getDataFileName();
				// Checks to not read in unneeded data (TODO - take
				// this out or keep the code so that unneeded data are filtered out)?
				if (comptype == COMP_CONTROL)
				{
					// Already read it above...
					continue;
				}
				if ((dataset.getDataSetType() == TYPE_OTHER_USES) && (comptype != COMP_OTHER))
				{
					unneeded_warning += "\n" + iline;
					comp.setVisible(false);
				}
				else if ((dataset.getDataSetType() == TYPE_CLIMATE_STATIONS) && (comptype != COMP_CU_LOCATIONS) && (comptype != COMP_BLANEY_CRIDDLE) && (comptype != COMP_PENMAN_MONTEITH) && (comptype != COMP_CROP_CHARACTERISTICS) && (comptype != COMP_CLIMATE_STATIONS) && (comptype != COMP_TEMPERATURE_TS_MONTHLY_AVERAGE) && (comptype != COMP_FROST_DATES_TS_YEARLY) && (comptype != COMP_PRECIPITATION_TS_MONTHLY) && (comptype != COMP_GIS_STATE) && (comptype != COMP_GIS_DIVISIONS) && (comptype != COMP_GIS_RIVERS) && (comptype != COMP_GIS_CLIMATE_STATIONS))
				{
					unneeded_warning += "\n" + iline;
					comp.setVisible(false);
				}
				else if ((dataset.getDataSetType() == TYPE_STRUCTURES) && (comptype != COMP_CU_LOCATIONS) && (comptype != COMP_BLANEY_CRIDDLE) && (comptype != COMP_PENMAN_MONTEITH) && (comptype != COMP_CROP_CHARACTERISTICS) && (comptype != COMP_CLIMATE_STATIONS) && (comptype != COMP_TEMPERATURE_TS_MONTHLY_AVERAGE) && (comptype != COMP_FROST_DATES_TS_YEARLY) && (comptype != COMP_PRECIPITATION_TS_MONTHLY) && (comptype != COMP_GIS_STATE) && (comptype != COMP_GIS_DIVISIONS) && (comptype != COMP_GIS_RIVERS) && (comptype != COMP_GIS_CLIMATE_STATIONS) && (comptype != COMP_GIS_STRUCTURES) && (comptype != COMP_CROP_PATTERN_TS_YEARLY))
				{
					unneeded_warning += "\n" + iline;
					comp.setVisible(false);
				}
				else if ((dataset.getDataSetType() == TYPE_WATER_SUPPLY_LIMITED) && (comptype != COMP_CU_LOCATIONS) && (comptype != COMP_CROP_CHARACTERISTICS) && (comptype != COMP_BLANEY_CRIDDLE) && (comptype != COMP_PENMAN_MONTEITH) && (comptype != COMP_CLIMATE_STATIONS) && (comptype != COMP_TEMPERATURE_TS_MONTHLY_AVERAGE) && (comptype != COMP_FROST_DATES_TS_YEARLY) && (comptype != COMP_PRECIPITATION_TS_MONTHLY) && (comptype != COMP_GIS_STATE) && (comptype != COMP_GIS_DIVISIONS) && (comptype != COMP_GIS_RIVERS) && (comptype != COMP_GIS_CLIMATE_STATIONS) && (comptype != COMP_GIS_STRUCTURES) && (comptype != COMP_CROP_PATTERN_TS_YEARLY) && (comptype != COMP_DIVERSION_TS_MONTHLY) && (comptype != COMP_WELL_PUMPING_TS_MONTHLY) && (comptype != COMP_IRRIGATION_PRACTICE_TS_YEARLY))
				{
					unneeded_warning += "\n" + iline;
					comp.setVisible(false);
				}
				else if ((dataset.getDataSetType() == TYPE_WATER_SUPPLY_LIMITED_BY_WATER_RIGHTS) && (comptype != COMP_CU_LOCATIONS) && (comptype != COMP_CROP_CHARACTERISTICS) && (comptype != COMP_BLANEY_CRIDDLE) && (comptype != COMP_PENMAN_MONTEITH) && (comptype != COMP_CLIMATE_STATIONS) && (comptype != COMP_TEMPERATURE_TS_MONTHLY_AVERAGE) && (comptype != COMP_FROST_DATES_TS_YEARLY) && (comptype != COMP_PRECIPITATION_TS_MONTHLY) && (comptype != COMP_GIS_STATE) && (comptype != COMP_GIS_DIVISIONS) && (comptype != COMP_GIS_RIVERS) && (comptype != COMP_GIS_CLIMATE_STATIONS) && (comptype != COMP_GIS_STRUCTURES) && (comptype != COMP_CROP_PATTERN_TS_YEARLY) && (comptype != COMP_DIVERSION_TS_MONTHLY) && (comptype != COMP_WELL_PUMPING_TS_MONTHLY) && (comptype != COMP_IRRIGATION_PRACTICE_TS_YEARLY) && (comptype != COMP_DIVERSION_RIGHTS))
				{
					unneeded_warning += "\n" + iline;
					comp.setVisible(false);
				}
				else if ((dataset.getDataSetType() == TYPE_RIVER_DEPLETION) && (comptype != COMP_CU_LOCATIONS) && (comptype != COMP_CROP_CHARACTERISTICS) && (comptype != COMP_BLANEY_CRIDDLE) && (comptype != COMP_PENMAN_MONTEITH) && (comptype != COMP_CLIMATE_STATIONS) && (comptype != COMP_TEMPERATURE_TS_MONTHLY_AVERAGE) && (comptype != COMP_FROST_DATES_TS_YEARLY) && (comptype != COMP_PRECIPITATION_TS_MONTHLY) && (comptype != COMP_GIS_STATE) && (comptype != COMP_GIS_DIVISIONS) && (comptype != COMP_GIS_RIVERS) && (comptype != COMP_GIS_CLIMATE_STATIONS) && (comptype != COMP_GIS_STRUCTURES) && (comptype != COMP_CROP_PATTERN_TS_YEARLY) && (comptype != COMP_DIVERSION_TS_MONTHLY) && (comptype != COMP_WELL_PUMPING_TS_MONTHLY) && (comptype != COMP_IRRIGATION_PRACTICE_TS_YEARLY) && (comptype != COMP_DIVERSION_RIGHTS) && (comptype != COMP_DELAY_TABLES_MONTHLY) && (comptype != COMP_DELAY_TABLE_ASSIGNMENT_MONTHLY))
				{
					unneeded_warning += "\n" + iline;
					comp.setVisible(false);
				}
				// Evaluate each data component...
				if (!read_all || !comp.isVisible())
				{
					// Not reading the actual data so skip...
					continue;
				}
				// Read the data for the object.  Some files are
				// actually not read here (e.g., spatial data) but the
				// components are added and the data can be read later.
				f = new File(compfile);
				if (!f.isAbsolute())
				{
					compfile = dataset.getDataSetDirectory() + File.separator + compfile;
				}
				// List these in the order that they are normally
				// processed/listed in StateDMI and other software...
				//
				// Climate Stations...
				//
				if (comptype == COMP_CLIMATE_STATIONS)
				{
					try
					{
						comp.setData(StateCU_ClimateStation.readStateCUFile(compfile));
						dataset.getComponentForComponentType(COMP_CLIMATE_STATIONS_GROUP).addComponent(comp);
					}
					catch (Exception e2)
					{
						read_warning += "\n" + iline;
						Message.printWarning(2, routine, e2);
					}
				}
				else if ((comptype == COMP_TEMPERATURE_TS_MONTHLY_AVERAGE) || (comptype == COMP_PRECIPITATION_TS_MONTHLY))
				{
					try
					{
						comp.setData(StateMod_TS.readTimeSeriesList(compfile, null, null, null, true));
						dataset.getComponentForComponentType(COMP_CLIMATE_STATIONS_GROUP).addComponent(comp);
					}
					catch (Exception e2)
					{
						read_warning += "\n" + iline;
						Message.printWarning(2, routine, e2);
					}
				}
				else if (comptype == COMP_FROST_DATES_TS_YEARLY)
				{
					try
					{
	// TODO (JTS - 2005-01-17) throwing compile errors
	//					comp.setData (
	//					StateCU_FrostDatesTS.readTimeSeriesList
	//						( compfile, null, null,
	//						null, null, true ) );
						dataset.getComponentForComponentType(COMP_CLIMATE_STATIONS_GROUP).addComponent(comp);
					}
					catch (Exception e2)
					{
						read_warning += "\n" + iline;
						Message.printWarning(2, routine, e2);
					}
				}
				//
				// Crop Characteristics...
				//
				else if (comptype == COMP_CROP_CHARACTERISTICS)
				{
					try
					{
						comp.setData(StateCU_CropCharacteristics.readStateCUFile(compfile));
						dataset.getComponentForComponentType(COMP_CROP_CHARACTERISTICS_GROUP).addComponent(comp);
					}
					catch (Exception e2)
					{
						read_warning += "\n" + iline;
						Message.printWarning(2, routine, e2);
					}
				}
				else if (comptype == COMP_BLANEY_CRIDDLE)
				{
					try
					{
						comp.setData(StateCU_BlaneyCriddle.readStateCUFile(compfile));
						dataset.getComponentForComponentType(COMP_CROP_CHARACTERISTICS_GROUP).addComponent(comp);
					}
					catch (Exception e2)
					{
						read_warning += "\n" + iline;
						Message.printWarning(2, routine, e2);
					}
				}
				else if (comptype == COMP_PENMAN_MONTEITH)
				{
					try
					{
						comp.setData(StateCU_PenmanMonteith.readStateCUFile(compfile));
						dataset.getComponentForComponentType(COMP_CROP_CHARACTERISTICS_GROUP).addComponent(comp);
					}
					catch (Exception e2)
					{
						read_warning += "\n" + iline;
						Message.printWarning(2, routine, e2);
					}
				}
				//
				// Delay tables...
				//
				else if (comptype == COMP_DELAY_TABLES_MONTHLY)
				{
					try
					{
						// StateCU assumes percent (0-100) for values, which is indicated by the -1 flag...
						comp.setData(StateMod_DelayTable.readStateModFile(compfile, true, -1));
						dataset.getComponentForComponentType(COMP_DELAY_TABLES_GROUP).addComponent(comp);
					}
					catch (Exception e2)
					{
						read_warning += "\n" + iline;
						Message.printWarning(2, routine, e2);
					}
				}
				//
				// CU Locations...
				//
				else if (comptype == COMP_CU_LOCATIONS)
				{
					try
					{
						comp.setData(StateCU_Location.readStateCUFile(compfile));
						dataset.getComponentForComponentType(COMP_CU_LOCATIONS_GROUP).addComponent(comp);
					}
					catch (Exception e2)
					{
						read_warning += "\n" + iline;
						Message.printWarning(2, routine, e2);
					}
				}
				else if (comptype == COMP_CROP_PATTERN_TS_YEARLY)
				{
					try
					{
						comp.setData(StateCU_CropPatternTS.readStateCUFile(compfile, null, null));
						dataset.getComponentForComponentType(COMP_CU_LOCATIONS_GROUP).addComponent(comp);
					}
					catch (Exception e2)
					{
						read_warning += "\n" + iline;
						Message.printWarning(2, routine, e2);
					}
				}
				else if (comptype == COMP_IRRIGATION_PRACTICE_TS_YEARLY)
				{
					try
					{
						comp.setData(StateCU_IrrigationPracticeTS.readStateCUFile(compfile, null, null));
						dataset.getComponentForComponentType(COMP_CU_LOCATIONS_GROUP).addComponent(comp);
					}
					catch (Exception e2)
					{
						read_warning += "\n" + iline;
						Message.printWarning(2, routine, e2);
					}
				}
				else if (comptype == COMP_DIVERSION_TS_MONTHLY)
				{
					try
					{
						comp.setData(StateMod_TS.readTimeSeriesList(compfile, null, null, null, true));
						dataset.getComponentForComponentType(COMP_CU_LOCATIONS_GROUP).addComponent(comp);
					}
					catch (Exception e2)
					{
						read_warning += "\n" + iline;
						Message.printWarning(2, routine, e2);
					}
				}
				else if (comptype == COMP_WELL_PUMPING_TS_MONTHLY)
				{
					try
					{
						comp.setData(StateMod_TS.readTimeSeriesList(compfile, null, null, null, true));
						dataset.getComponentForComponentType(COMP_CU_LOCATIONS_GROUP).addComponent(comp);
					}
					catch (Exception e2)
					{
						read_warning += "\n" + iline;
						Message.printWarning(2, routine, e2);
					}
				}
				else if (comptype == COMP_DIVERSION_RIGHTS)
				{
					try
					{
						comp.setData(StateMod_DiversionRight.readStateModFile(compfile));
						dataset.getComponentForComponentType(COMP_CU_LOCATIONS_GROUP).addComponent(comp);
					}
					catch (Exception e2)
					{
						read_warning += "\n" + iline;
						Message.printWarning(2, routine, e2);
					}
				}
				else if (comptype == COMP_DELAY_TABLE_ASSIGNMENT_MONTHLY)
				{
					try
					{
						comp.setData(StateCU_DelayTableAssignment.readStateCUFile(compfile));
						dataset.getComponentForComponentType(COMP_CU_LOCATIONS_GROUP).addComponent(comp);
					}
					catch (Exception e2)
					{
						read_warning += "\n" + iline;
						Message.printWarning(2, routine, e2);
					}
				}
				// Files not specifically handled (e.g., GIS)...
				else
				{
					// Add to the component group but don't read in the data...
					int gtype = dataset.lookupComponentGroupTypeForComponent(comptype);
					if (gtype < 0)
					{
						Message.printWarning(2, routine, "Group for component is unknown.  Not adding: " + comp.getComponentName());
					}
					dataset.getComponentForComponentType(gtype).addComponent(comp);
				}
			}
			catch (Exception e)
			{
				Message.printWarning(1, routine, "Unexpected error for:\n\"" + iline + "\"");
				Message.printWarning(2, routine, e);
			}
		}
		if (unneeded_warning.Length > 0)
		{
			Message.printWarning(2, routine, lookupDataSetName(dataset.getDataSetType()) + " data set.  Unnecessary data files will not be visible:" + unneeded_warning);
		}
		if (read_warning.Length > 0)
		{
			Message.printWarning(1, routine, "Error reading data files:" + read_warning);
		}
		if (@in != null)
		{
			@in.Close();
		}
		return dataset;
	}

	/// <summary>
	/// Read a complete StateCU data set from an XML data set file. </summary>
	/// <param name="filename"> XML data set file to read. </param>
	/// <param name="read_all"> If true, all the data files mentioned in the response file will
	/// be read into memory, providing a complete data set for viewing and manipulation. </param>
	/// <exception cref="Exception"> if there is an error reading the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static StateCU_DataSet readXMLFile(String filename, boolean read_all) throws Exception
	public static StateCU_DataSet readXMLFile(string filename, bool read_all)

	{
	/* TODO SAM 2007-03-01
		String routine = "StateCU_DataSet.readXMLFile";
		String full_filename = IOUtil.getPathUsingWorkingDir ( filename );
		
		need to figure out if this is in the base class.
	
		DOMParser parser = null;
		try {
			parser = new DOMParser();
			parser.parse ( full_filename );
		}
		catch ( Exception e ) {
			Message.printWarning ( 2, routine, "Error reading StateCU Data set \"" + filename + "\"" );
			Message.printWarning ( 2, routine, e );
			throw new Exception ( "Error reading StateCU Data set \"" + filename + "\"" );
		}
	
		// Create a new data set object...
	
		StateCU_DataSet dataset = new StateCU_DataSet();
		File f = new File ( full_filename );
		dataset.setDirectory ( f.getParent() );
		dataset.setDataSetFileName ( f.getName() );
	
		// Now get information from the document.  For now don't hold the
		// document as a data member...
	
		Document doc = parser.getDocument();
	
		// Loop through and process the document nodes, starting with the root node...
	
		processDocumentNodeForRead ( dataset, doc );
	
		// Synchronize the response file with the control file (for now just
		// check - need to decide how to make bulletproof)...
	
		// TODO
		//DataSetComponent comp = dataset.getComponentForComponentType ( COMP_RESPONSE );
		//if ( comp != null ) {
		//	StateCU_DataSet ds2 = readStateCUFile ( comp.getDataFile(), false );
		//}
	
		// Compare components and response file.  Need to REVISIT this.
	
		// Now just read the components - the assumption is that the data set
		// components are correct for the data set but need to tighten this down
	
		String read_warning = "";
		if ( read_all ) {
			Vector components = dataset.getComponents();
			int size = dataset.__components.size();
			String datafile = "";
			DataSetComponent comp;
			for ( int i = 0; i < size; i++ ) {
				comp = (DataSetComponent)components.elementAt(i);
				try {
					datafile = comp.getDataFileName();
					f = new File(datafile);
					if ( !f.isAbsolute() ) {
						datafile =	dataset.getDirectory() + File.separator + datafile;
					}
					if ( comp.getComponentType() == COMP_CU_LOCATIONS ) {
						comp.setData (StateCU_Location.readStateCUFile(datafile));
					}
					else if (comp.getType() == COMP_CROP_CHARACTERISTICS) {
						comp.setData ( StateCU_CropCharacteristics.readStateCUFile( datafile));
					}
					else if (comp.getType() == COMP_BLANEY_CRIDDLE) {
						comp.setData ( StateCU_BlaneyCriddle.readStateCUFile(datafile));
					}
					else if (comp.getType() == COMP_PENMAN_MONTEITH) {
						comp.setData ( StateCU_PenmanMonteith.readStateCUFile(datafile));
					}
					else if (comp.getType() == COMP_CLIMATE_STATIONS) {
						comp.setData ( StateCU_ClimateStation.readStateCUFile(datafile));
					}
				}
				catch ( Exception e ) {
					read_warning += "\n" + datafile;
					Message.printWarning ( 2, routine, e );
				}
			}
		}
		else {
			// Read the control file???
		}
		if ( read_warning.length() > 0 ) {
			Message.printWarning ( 1, routine, "Error reading data files:" + read_warning );
		}
	
		return dataset;
	*/
		return null;
	}

	/// <summary>
	/// Performs the check file setup and calls code to check component.  Also sets
	/// the check file to the list in the GUI.  If problems are encountered when
	/// running data checks are added to the check file. </summary>
	/// <param name="int"> type - StateModComponent type. </param>
	public virtual string runComponentChecks(int type, string fname, string commands, string header)
	{
		string check_file = "";
		CheckFile chk = new CheckFile(fname, commands);
		chk.addToHeader(header);
		StateCU_ComponentDataCheck check = new StateCU_ComponentDataCheck(type, chk, this);
		// Run the data checks for the component and retrieve the finalized check file
		CheckFile final_check = check.checkComponentType(null);
		try
		{
			final_check.finalizeCheckFile();
			check_file = final_check.ToString();
		}
		catch (Exception e)
		{
			Message.printWarning(2, "StateDMI_Processor.runComponentChecks", "Check file: " + final_check.ToString() + " couldn't be finalized.");
			Message.printWarning(3, "StateDMI_Processor.runComponentChecks", e);
		}
		return check_file;
	}

	/// <summary>
	/// Set the administration number for CU by priority. </summary>
	/// <param name="admin_num"> the administration number for CU by priority. </param>
	public virtual void setAdminNumForCUByPriority(double admin_num)
	{
		__admin_num = admin_num;
	}

	/// <summary>
	/// Set a comment. </summary>
	/// <param name="comment"> Comment to set. </param>
	/// <param name="pos"> Comment index (0 - 2). </param>
	public virtual void setComment(string comment, int pos)
	{
		if ((pos < 0) || (pos > 2))
		{
			return;
		}
		__comments[pos] = comment;
	}

	/// <summary>
	/// Set whether to create StateMod output. </summary>
	/// <param name="ddcsw"> Indicate whether to create StateMod output. </param>
	public virtual void setDdcsw(int ddcsw)
	{
		__ddcsw = ddcsw;
	}

	/// <summary>
	/// Set the value of flag1 (CU method). </summary>
	/// <param name="flag1"> the value of flag1. </param>
	public virtual void setFlag1(int flag1)
	{
		__flag1 = flag1;
	}

	/// <summary>
	/// Set the value of iclim (climate stations or structures data set). </summary>
	/// <param name="iclim"> the value of iclim. </param>
	public virtual void setIclim(int iclim)
	{
		__iclim = iclim;
	}

	/// <summary>
	/// Set the value of idaily (daily/monthy switch). </summary>
	/// <param name="idaily"> the value of idaily. </param>
	public virtual void setIdaily(int idaily)
	{
		__idaily = idaily;
	}

	/// <summary>
	/// Set the value of iflood (handle groundwater use). </summary>
	/// <param name="iflood"> the value of iflood. </param>
	public virtual void setIflood(int iflood)
	{
		__iflood = iflood;
	}

	/// <summary>
	/// Set the iprtysm flag (operate soil moisture by proration). </summary>
	/// <param name="iprtysm"> the iprtysm flag. </param>
	public virtual void setIprtysm(int iprtysm)
	{
		__iprtysm = iprtysm;
	}

	/// <summary>
	/// Set the value of ism (consider soil moisture?). </summary>
	/// <param name="ism"> the value of ism. </param>
	public virtual void setIsm(int ism)
	{
		__ism = ism;
	}

	/// <summary>
	/// Set the value of isupply (water supply option). </summary>
	/// <param name="isupply"> the value of isupply. </param>
	public virtual void setIsupply(int isupply)
	{
		__isupply = isupply;
	}

	/// <summary>
	/// Set the starting year for the data set. </summary>
	/// <param name="nyr1"> the starting year for the data set. </param>
	public virtual void setNyr1(int nyr1)
	{
		__nyr1 = nyr1;
	}

	/// <summary>
	/// Set the ending year for the data set. </summary>
	/// <param name="nyr2"> the ending year for the data set. </param>
	public virtual void setNyr2(int nyr2)
	{
		__nyr2 = nyr2;
	}

	/// <summary>
	/// Set the initial soil moisture content for junior parcels (fraction of capacity). </summary>
	/// <param name="pjunmo"> the initial soil moisture content for junior parcels. </param>
	public virtual void setPjunmo(double pjunmo)
	{
		__pjunmo = pjunmo;
	}

	/// <summary>
	/// Set the initial soil moisture content for senior parcels (fraction of capacity). </summary>
	/// <param name="pothmo"> the initial soil moisture content for senior parcels. </param>
	public virtual void setPothmo(double pothmo)
	{
		__pothmo = pothmo;
	}

	/// <summary>
	/// Set the initial soil moisture content for other parcels (fraction of capacity). </summary>
	/// <param name="psenmo"> the initial soil moisture content for other parcels. </param>
	public virtual void setPsenmo(double psenmo)
	{
		__psenmo = psenmo;
	}

	/// <summary>
	/// Set the rn_xco flag (monthly precipitation method). </summary>
	/// <param name="rn_xco"> the rn_xco flag. </param>
	public virtual void setRn_xco(int rn_xco)
	{
		__rn_xco = rn_xco;
	}

	/// <summary>
	/// Set the sout flag (input summary type). </summary>
	/// <param name="sout"> the sout flag. </param>
	public virtual void setSout(int sout)
	{
		__sout = sout;
	}

	/// <summary>
	/// Set the typout flag (output summary type). </summary>
	/// <param name="typout"> the typout flag. </param>
	public virtual void setTypout(int typout)
	{
		__typout = typout;
	}

	/// <summary>
	/// Write a StateCU_Control object to a file.  The filename is adjusted
	/// to the working directory if necessary using IOUtil.getPathUsingWorkingDir(). </summary>
	/// <param name="filename_prev"> The name of the previous version of the file (for
	/// processing headers).  Specify as null if no previous file is available. </param>
	/// <param name="filename"> The name of the file to write. </param>
	/// <param name="control"> A StateCU_Control object to write.
	/// @new_comments Comments to add to the top of the file.  Specify as null if no
	/// comments are available. </param>
	/// <exception cref="IOException"> if there is an error writing the file. </exception>
	/* TODO SAM Evaluate whether needed
	private void writeStateCUControlFile ( String filename_prev, String filename, String [] new_comments )
	throws IOException
	{	String [] comment_str = { "#" };
		String [] ignore_comment_str = { "#>" };
		PrintWriter out = null;
		String full_filename_prev = IOUtil.getPathUsingWorkingDir ( filename_prev );
		String full_filename = IOUtil.getPathUsingWorkingDir ( filename );
		out = IOUtil.processFileHeaders ( full_filename_prev, full_filename, 
			new_comments, comment_str, ignore_comment_str, 0 );
		if ( out == null ) {
			throw new IOException ( "Error writing to \"" + full_filename + "\"" );
		}
		String cmnt = "#>";
		// Missing data handled by formatting all as strings...
	
		out.println ( cmnt );
		out.println ( cmnt + "  StateCU Control File" );
		out.println ( cmnt );
		out.println ( cmnt + "  Values are free-format except where noted." );
		out.println ( cmnt );
		out.println ( cmnt + "  Notes       comment:  First three non-comment lines are general notes");
		out.println ( cmnt + "  Begin/End nyr1,nyr2:  4-digit years for simulation period.");
		out.println ( cmnt + "                        The time series files can be longer.");
		out.println ( cmnt + "  CUMethod      flag1:  CU Method" );
		out.println ( cmnt + "                        1 = Blaney-Criddle" );
		out.println ( cmnt + "                        2 = Other uses (non-agriculture)" );
		out.println ( cmnt + "                        3 = Penman-Monteith" );
		out.println ( cmnt + "                        4 = Hargreaves" );
		out.println ( cmnt + "  PrecipMeth   RN_XCO:  Monthly precipitation method." );
		out.println ( cmnt + "                        1 = Soil Conservation Service" );
		out.println ( cmnt + "                        2 = United States Bureau of Rec." );
		out.println ( cmnt + "  DataSetType   iclim:  Data set type." );
		out.println ( cmnt + "                        0 = CU at climate stations (unit of area)" );
		out.println ( cmnt + "                        1 = CU at structures (ditches/wells)");
		out.println ( cmnt + "  WaterSupply isupply:  Water supply option (levels are incremental)." );
		out.println ( cmnt + "                        0 = none");
		out.println ( cmnt + "                        1 = supply limited");
		out.println ( cmnt + "                        2 = water rights considered");
		out.println ( cmnt + "                        3 = return flows considered");
		out.println ( cmnt + "                        4 = groundwater considered");
		out.println ( cmnt + "  InputSummary   sout:  Input summary flag." );
		out.println ( cmnt + "                        0 = output basic summary" );
		out.println ( cmnt + "                        1 = output detailed summary" );
		out.println ( cmnt + "  SoilMoisture   isim:  Soil moisture flag." );
		out.println ( cmnt + "                        0 = do not consider soil moisture" );
		out.println ( cmnt + "                        1 = consider user-initialized soil moisture" );
		out.println ( cmnt + "                        2 = consider run presimulation to initialize" );
		out.println ( cmnt + "  SoilMoist0   p***mo:  Initial soil moisture content for." );
		out.println ( cmnt + "                        senior, junior, other parcels" );
		out.println ( cmnt + "                        (fraction of capacity), format 3f5.0.");
		out.println ( cmnt + "  SMProrate   iprtysm:  0 = operate soil moisture by proration");
		out.println ( cmnt + "                        1 = operate by priority" );
		out.println ( cmnt + "                        sprinkler separately." );
		out.println ( cmnt + "                        (REVISIT possible values?)" );
		out.println ( cmnt + " Output        typout:  Output summary flag (format i5).");
		out.println ( cmnt + "                        0 = output basic summary" );
		out.println ( cmnt + "                        1 = +irrigation water requirement" );
		out.println ( cmnt + "                            +water supply limited" );
		out.println ( cmnt + "                        2 = +water budget" );
		out.println ( cmnt + "                        3 = +water budget by structure" );
		out.println ( cmnt + "  IrrigMethod  iflood:  Output groundwater use by flood and" );
		out.println ( cmnt + "                        sprinkler separately." );
		out.println ( cmnt + "                        (REVISIT possible values?)" );
		out.println ( cmnt + "  StateMod      ddcsw:  StateMod output format switch" );
		out.println ( cmnt + "                        0 = no" );
		out.println ( cmnt + "                        1 = yes" );
		out.println ( cmnt + "  Daily        idaily:  Daily data switch" );
		out.println ( cmnt + "                        0 = (REVISIT what is zero?)" );
		out.println ( cmnt + "                        1 = daily diversions with daily admin" );
		out.println ( cmnt + "                        2 = daily diversions with monthly admin" );
		out.println ( cmnt + "                        3 = daily diversions with single admin" );
		out.println ( cmnt + "                        4 = monthly diversions with monthly admin" );
		out.println ( cmnt + "                        5 = monthly diversions with single admin" );
		out.println ( cmnt + "  AdminNum           :  Administration number for CU by priority." );
		out.println ( cmnt );
		out.println ( cmnt + " StationID  Lat   Elev            Region1      Region2        StationName" );
		out.println ( cmnt + "----------------------------------------------------------------------------------------" );
		out.println ( cmnt + "EndHeader" );
	
		out.println ( getComment(0) );
		out.println ( getComment(1) );
		out.println ( getComment(2) );
		// Just print all, even if missing...
		out.println ( StringUtil.formatString( getNyr1(),"%4d") + " " +
			StringUtil.formatString(getNyr2(),"%4d") );
		out.println ( getFlag1() );
		out.println ( getRn_xco() );
		out.println ( getIclim() );
		out.println ( getIsupply() );
		out.println ( getSout() );
		out.println ( getIsm() );
		out.println ( StringUtil.formatString( getPsenmo(),"%5.0f") +
				StringUtil.formatString( getPjunmo(),"%5.0f") +
				StringUtil.formatString( getPothmo(),"%5.0f") );
		out.println ( getTypout() );
		out.println ( getIprtysm() );
		out.println ( getIflood() );
		out.println ( getDdcsw() );
		out.println ( getIdaily() );
		out.println ( StringUtil.formatString(getAdminNumForCUByPriority(),"%11.0f"));
		out.flush();
		out.close();
		out = null;
	}
	*/

	/*
	Write the data set to an XML file.  The filename is adjusted to the
	working directory if necessary using IOUtil.getPathUsingWorkingDir().
	@param filename_prev The name of the previous version of the file (for
	processing headers).  Specify as null if no previous file is available.
	@param filename The name of the file to write.
	@param data_Vector A Vector of StateCU_Location to write.
	@new_comments Comments to add to the top of the file.  Specify as null if no
	comments are available.
	@exception IOException if there is an error writing the file.
	*/
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeXMLFile(String filename_prev, String filename, StateCU_DataSet dataset, String [] new_comments) throws java.io.IOException
	public static void writeXMLFile(string filename_prev, string filename, StateCU_DataSet dataset, string[] new_comments)

	{
	/* TODO - need to figure out if this is in the base class
		PrintWriter out = null;
		String full_filename_prev = IOUtil.getPathUsingWorkingDir ( filename_prev );
		if ( !StringUtil.endsWithIgnoreCase(filename,".xml") ) {
			filename = filename + ".xml";
		}
		String full_filename = IOUtil.getPathUsingWorkingDir ( filename );
		out = IOUtil.processFileHeaders ( full_filename_prev, full_filename, 
			new_comments, comment_str, ignore_comment_str, 0 );
		if ( out == null ) {
			throw new IOException ( "Error writing to \"" + full_filename + "\"" );
		}
		writeDataSetToXMLFile ( dataset, out );
		out.flush();
		out.close();
		out = null;
		*/
	}

	/// <summary>
	/// Write a data set to an opened XML file. </summary>
	/// <param name="data"> A StateCU_DataSet to write. </param>
	/// <param name="out"> output PrintWriter.
	/// @exceptoin IOException if an error occurs. </param>
	/* TODO SAM 2007-03-01 Evaluate whether needed
	private static void writeDataSetToXMLFile (	StateCU_DataSet dataset, PrintWriter out )
	throws IOException
	{	// Start XML tag...
		out.println ("<!--" );
		out.println ( cmnt );
		out.println ( cmnt + "  StateCU Data Set (XML) File" );
		out.println ( cmnt );
		out.println ( cmnt + "EndHeader" );
		out.println ("-->" );
	
		out.println ("<StateCU_DataSet " +
			"Type=\"" + lookupTypeName(dataset.getDataSetType()) + "\"" +
			"BaseName=\"" + dataset.getBaseName() + "\"" + ">" );
	
		int num = 0;
		Vector data_Vector = dataset.getComponents();
		if ( data_Vector != null ) {
			num = data_Vector.size();
		}
		String indent1 = "  ";
		String indent2 = indent1 + indent1;
		for ( int i = 0; i < num; i++ ) {
			comp = (DataSetComponent)data_Vector.elementAt(i);
			if ( comp == null ) {
				continue;
			}
			out.println ( indent1 + "<DataSetComponent" );
	
			out.println ( indent2 + "Type=\"" + DataSetComponent.lookupComponentName(comp.getType()) + "\"" );
			out.println ( indent2 + "DataFile=\"" + comp.getDataFileName() + "\"" );
			out.println ( indent2 + "ListFile=\"" + comp.getListFileName() + "\"" );
			out.println ( indent2 + "CommandsFile=\"" + comp.getCommandsFileName() + "\"" );
			out.println ( indent2 + ">" );
	
			out.println ( indent1 + "</DataSetComponent>");
		}
		out.println ("</StateCU_DataSet>" );
	}
	*/

	}

}