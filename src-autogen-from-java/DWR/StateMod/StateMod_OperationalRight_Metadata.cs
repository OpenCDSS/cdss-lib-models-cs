﻿using System;
using System.Collections.Generic;

// StateMod_OperationalRight_Metadata - Class to hold operational right metadata

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

	using PropList = RTi.Util.IO.PropList;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;
	using DataTable = RTi.Util.Table.DataTable;

	/// <summary>
	/// Class to hold operational right metadata, which helps the software with error checks and visualization.
	/// The term "Operational Right" and "Operating Rule" are used interchangeably.  For example, by knowing the
	/// source types, software can present the proper lists of structures for editing and the proper node types
	/// can be search for when annotating the network diagram.
	/// </summary>
	public class StateMod_OperationalRight_Metadata
	{

	/// <summary>
	/// Maximum number of intervening structures.  This is an array dimension in StateMod.
	/// </summary>
	public static int MAXIMUM_INTERVENING_STRUCTURES = 10;

	/// <summary>
	/// Indicate whether the editing class supports the right (true) - if false, a text editor will
	/// be used for the operational right.
	/// </summary>
	private bool __fullEditingSupported = false;

	/// <summary>
	/// Operational right (operating rule) type - to group rules for editing.
	/// </summary>
	private StateMod_OperationalRight_Metadata_RuleType __ruleTypeCategory;

	/// <summary>
	/// Operational right type (number).
	/// </summary>
	private int __rightTypeNumber;

	/// <summary>
	/// Operational right name.
	/// </summary>
	private string __rightTypeName;

	/// <summary>
	/// Types of data that can be used as the destination for an operational right.
	/// </summary>
	private StateMod_OperationalRight_Metadata_SourceOrDestinationType[] __destinationTypes = null;

	/// <summary>
	/// Types of data that can be used as the source1 for an operational right.
	/// </summary>
	private StateMod_OperationalRight_Metadata_SourceOrDestinationType[] __source1Types = null;

	/// <summary>
	/// Types of data that can be used as the source2 for an operational right.
	/// </summary>
	private StateMod_OperationalRight_Metadata_SourceOrDestinationType[] __source2Types = null;

	/// <summary>
	/// Types of plans that that can be used as the associated plan for an operational right.
	/// </summary>
	private StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] __associatedPlanAllowedTypes = null;

	/// <summary>
	/// Types of diversion that that can be used for an operational right.
	/// </summary>
	private StateMod_OperationalRight_Metadata_DiversionType[] __diversionTypes = null;

	/// <summary>
	/// Whether transit and conveyance loss are allowed for an operational right.
	/// </summary>
	private StateMod_OperationalRight_Metadata_TransitAndConveyanceLossAllowedType __transitAndConveyanceLossAllowed = null;

	/// <summary>
	/// List of static global metadata, meant to be initialized once and shared within the application.
	/// </summary>
	private static IList<StateMod_OperationalRight_Metadata> __opRightsMetadataList = null;

	/// <summary>
	/// Whether the operational right uses intervening structures with loss.
	/// These are usually indicated in the dumx value
	/// and are provided in separate records.  This might be the same as whether the carrier is allowed
	/// but Ray Bennett was going to check on it.
	/// </summary>
	private bool __rightTypeUsesInterveningStructuresWithLoss = false;

	/// <summary>
	/// Whether the operational right uses intervening structures without loss.
	/// These are usually indicated in the dumx value
	/// and are provided in separate records.  This might be the same as whether the carrier is allowed
	/// but Ray Bennett was going to check on it.
	/// </summary>
	private bool __rightTypeUsesInterveningStructuresWithoutLoss = false;

	/// <summary>
	/// Constructor for metadata.
	/// </summary>
	public StateMod_OperationalRight_Metadata(int rightTypeNumber, bool fullEditingSupported, string rightName, StateMod_OperationalRight_Metadata_RuleType ruleTypeCategory, StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source1Types, StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source2Types, StateMod_OperationalRight_Metadata_Source2AllowedType source2AllowedType, StateMod_OperationalRight_Metadata_SourceOrDestinationType[] destinationTypes, StateMod_OperationalRight_Metadata_DestinationLocationType[] destinationLocationType, StateMod_OperationalRight_Metadata_DeliveryMethodType deliveryMethodType, StateMod_OperationalRight_Metadata_CarrierAllowedType carrierAllowedType, bool usesInterveningStructuresWithoutLoss, bool usesInterveningStructuresWithLoss, StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] associatedPlanAllowedTypes, StateMod_OperationalRight_Metadata_DiversionType[] diversionTypes, StateMod_OperationalRight_Metadata_TransitAndConveyanceLossAllowedType transitAndConveyanceLossAllowedType, string comment)
	{
		setRightTypeNumber(rightTypeNumber);
		setFullEditingSupported(fullEditingSupported);
		setRightTypeName(rightName);
		setRuleTypeCategory(ruleTypeCategory);
		setDestinationTypes(destinationTypes);
		setSource1Types(source1Types);
		setSource2Types(source2Types);
		setUsesInterveningStructuresWithoutLoss(usesInterveningStructuresWithoutLoss);
		setUsesInterveningStructuresWithLoss(usesInterveningStructuresWithLoss);
		setAssociatedPlanAllowedTypes(associatedPlanAllowedTypes);
		setDiversionTypes(diversionTypes);
		setTransitAndConveyanceLossAllowed(transitAndConveyanceLossAllowedType);
	}

	/// <summary>
	/// Return the list of all metadata.
	/// </summary>
	public static IList<StateMod_OperationalRight_Metadata> getAllMetadata()
	{
		initialize();
		return __opRightsMetadataList;
	}

	/// <summary>
	/// Return the allowed associated plan types for the right.
	/// </summary>
	public virtual StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType [] getAssociatedPlanAllowedTypes()
	{
		return __associatedPlanAllowedTypes;
	}

	//TODO SAM 2011-02-02 evaluate whether a choice can be used when all rights have been reviewed
	/// <summary>
	/// Return an explanation of the OprLoss fields.  This is used because sometimes StateMod uses fixed
	/// values and other times a number.  An editable JComboBox could be used but hold off until more review
	/// is done.
	/// </summary>
	public virtual string getConveyanceLossNotes()
	{
		int rightTypeNumber = getRightTypeNumber();
		if (rightTypeNumber == 24)
		{
			return "<html>0 - no transit loss<b>" +
					"+n - transit loss n%<b>" +
					"-1 - no transit loss</html>";
		}
		else
		{
			return "";
		}
	}

	/// <summary>
	/// TODO SAM 2011-01-31 Move to metadata if a pattern emerges - and are these additional or replacements
	/// For now interpret as additional (added to empty or non-empty list when used).
	/// Return the special destination account
	/// </summary>
	public virtual IList<string> getDestinationAccountSpecialChoices()
	{
		IList<string> specialChoices = new List<object>();
		int rightTypeNumber = getRightTypeNumber();
		if (rightTypeNumber == 17)
		{
			specialChoices.Add("1.0 - Coefficient");
		}
		else if (rightTypeNumber == 18)
		{
			specialChoices.Add("1.0 - Coefficient");
		}
		else if (rightTypeNumber == 48)
		{
			specialChoices.Add("0 - Not used");
		}
		else if (rightTypeNumber == 49)
		{
			specialChoices.Add("0 - Not used");
		}
		return specialChoices;
	}

	/// <summary>
	/// Return the allowed destination types for the right.
	/// </summary>
	public virtual StateMod_OperationalRight_Metadata_SourceOrDestinationType [] getDestinationTypes()
	{
		return __destinationTypes;
	}

	/// <summary>
	/// Return the diversion types for the right.
	/// </summary>
	public virtual StateMod_OperationalRight_Metadata_DiversionType [] getDiversionTypes()
	{
		return __diversionTypes;
	}

	/// <summary>
	/// Return whether full editing is supported for the right. </summary>
	/// <param name="dataSet"> StateMod_DataSet, needed to check some relationships during the read (e.g., type 24). </param>
	public virtual bool getFullEditingSupported(StateMod_DataSet dataSet)
	{
		int rightTypeNumber = getRightTypeNumber();
		int[] rightTypesThatUseDataSet = new int[] {24};
		for (int i = 0; i < rightTypesThatUseDataSet.Length; i++)
		{
			if (rightTypeNumber == rightTypesThatUseDataSet[i])
			{
				if (dataSet == null)
				{
					return false;
				}
				else
				{
					return __fullEditingSupported;
				}
			}
		}
		return __fullEditingSupported;
	}

	// TODO SAM 2011-02-02 evaluate whether a choice can be used when all rights have been reviewed
	/// <summary>
	/// Return an explanation of the OprLimits fields.  This is used because sometimes StateMod uses fixed
	/// values and other times a number.  An editable JComboBox could be used but hold off until more review
	/// is done.
	/// </summary>
	public virtual string getLimitsNotes()
	{
		int rightTypeNumber = getRightTypeNumber();
		if (rightTypeNumber == 11)
		{
			return "<html>0 = no carrier limitation<br>" +
					"+n = carrier limit (up to structure capacity)</html>";
		}
		else if (rightTypeNumber == 29)
		{
			return "<html>0 = do not adjust monthly and annual operational limits<br>" +
				"1 = adjust monthly and annual limits specified by associated operating rule</html>";
		}
		else if (rightTypeNumber == 34)
		{
			return "<html>0 = do not adjust monthly or annual operational limits<br>" +
				"2 = limit monthly or annual limits specified by associated operating rule</html>";
		}
		else if (rightTypeNumber == 45)
		{
			return "<html>0 = no carrier limitation<br>" +
					"-1 = water right is shared with another operating rule with the same administration number<br>" +
					"+n = water right is not shared with another operating rule with the same administration number</html>";
		}
		else if (rightTypeNumber == 46)
		{
			return "Number of destinations";
		}
		if (rightTypeNumber == 47)
		{
			return "<html>0 = do not include monthly and annual operational limits<br>" +
					"1 = monthly and annual diversion operational limits are provided</html>";
		}
		else
		{
			return "";
		}
	}

	/// <summary>
	/// Return the metadata given an operational right number. </summary>
	/// <param name="rightTypeNumber"> the operational right type number </param>
	public static StateMod_OperationalRight_Metadata getMetadata(int rightTypeNumber)
	{
		initialize();
		foreach (StateMod_OperationalRight_Metadata metadata in getAllMetadata())
		{
			if (metadata.getRightTypeNumber() == rightTypeNumber)
			{
				return metadata;
			}
		}
		return null;
	}

	/// <summary>
	/// Get the monthly OprLimits title (data have different application based on right type).
	/// </summary>
	public virtual string getMonthlyLimitsTitle()
	{
		int rightTypeNumber = getRightTypeNumber();
		if (rightTypeNumber == 24)
		{
			return "Monthly and annual exchange limits (ACFT)";
		}
		else
		{
			// Default (and for 47)...
			return "Monthly and annual operating limits (ACFT)";
		}
	}

	/// <summary>
	/// Return the operational right type name. </summary>
	/// <returns> the operational right type name. </returns>
	public virtual string getRightTypeName()
	{
		return __rightTypeName;
	}

	/// <summary>
	/// Return the operational right type name list (all names). </summary>
	/// <returns> the operational right type name (all names). </returns>
	public static IList<string> getRightTypeNameList()
	{
		initialize();
		IList<StateMod_OperationalRight_Metadata> metadataList = getAllMetadata();
		IList<string> names = new List<object>();
		foreach (StateMod_OperationalRight_Metadata metadata in metadataList)
		{
			names.Add(metadata.getRightTypeName());
		}
		return names;
	}

	/// <summary>
	/// Return the operational right type number. </summary>
	/// <returns> the operational right type number. </returns>
	public virtual int getRightTypeNumber()
	{
		return __rightTypeNumber;
	}

	/// <summary>
	/// Return the operating rule type. </summary>
	/// <returns> the operating rule type. </returns>
	public virtual StateMod_OperationalRight_Metadata_RuleType getRuleTypeCategory()
	{
		return __ruleTypeCategory;
	}

	/// <summary>
	/// Indicate whether the operational right uses an associated operating rule (cx).
	/// </summary>
	public virtual bool getRightTypeUsesAssociatedOperatingRule(double oprLimits)
	{
		int rightTypeNumber = getRightTypeNumber();
		int oprLimitsInt = 0;
		if (oprLimits > 0.0)
		{
			oprLimitsInt = (int)(oprLimits + .1);
		}
		else if (oprLimits < 0.0)
		{
			oprLimitsInt = (int)(oprLimits - .1);
		}
		int[] rightTypesThatUseMonthlyOprLimits = new int[] {29, 34};
		for (int i = 0; i < rightTypesThatUseMonthlyOprLimits.Length; i++)
		{
			if (rightTypeNumber == rightTypesThatUseMonthlyOprLimits[i])
			{
				// TODO SAM 2011-02-06 Need to confirm this since documentation is not consistent
				if ((rightTypeNumber == 29) && (oprLimitsInt == 1))
				{
					return true;
				}
				else if ((rightTypeNumber == 34) && (oprLimitsInt == 2))
				{
					return true;
				}
			}
		}
		return false;
	}

	/// <summary>
	/// Indicate whether the operational right uses an associated plan.
	/// </summary>
	public virtual bool getRightTypeUsesAssociatedPlan()
	{
		StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] associatedPlanAllowedTypes = getAssociatedPlanAllowedTypes();
		if ((associatedPlanAllowedTypes.Length == 0) || ((associatedPlanAllowedTypes.Length == 1) && (associatedPlanAllowedTypes[0] == StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.NA)))
		{
			return false;
		}
		else
		{
			return true;
		}
	}

	/// <summary>
	/// Indicate whether the operational right uses conveyance loss.
	/// </summary>
	public virtual bool getRightTypeUsesConveyanceLoss()
	{
		StateMod_OperationalRight_Metadata_TransitAndConveyanceLossAllowedType transitAndConveyanceLossAllowed = getTransitAndConveyanceLossAllowedType();
		if ((transitAndConveyanceLossAllowed == StateMod_OperationalRight_Metadata_TransitAndConveyanceLossAllowedType.NA))
		{
			return false;
		}
		else
		{
			return true;
		}
	}

	/// <summary>
	/// Indicate whether the operational right uses the destination.
	/// </summary>
	public virtual bool getRightTypeUsesDestination()
	{
		StateMod_OperationalRight_Metadata_SourceOrDestinationType[] allowedDestinationTypes = getDestinationTypes();
		if ((allowedDestinationTypes.Length == 0) || (allowedDestinationTypes[0] == StateMod_OperationalRight_Metadata_SourceOrDestinationType.NA))
		{
			return false;
		}
		else
		{
			return true;
		}
	}

	/// <summary>
	/// Indicate whether the operational right uses a diversion type.
	/// </summary>
	public virtual bool getRightTypeUsesDiversionType()
	{
		StateMod_OperationalRight_Metadata_DiversionType[] diversionTypes = getDiversionTypes();
		if ((diversionTypes.Length == 0) || (diversionTypes[0] == StateMod_OperationalRight_Metadata_DiversionType.NA))
		{
			return false;
		}
		else
		{
			return true;
		}
	}

	/// <summary>
	/// Indicate whether the operational right uses intervening structures with loss.
	/// For now hard code here but once list is known, may add to metadata. </summary>
	/// <param name="oprLoss"> the single value oprLoss from the first record, needed because some right types allow
	/// intervening structures with and without loss </param>
	public virtual bool getRightTypeUsesInterveningStructuresWithLoss(double oprLoss)
	{
		int rightTypeNumber = getRightTypeNumber();
		int[] rightTypesThatUseInterveningStructuresWithLossCheckOprLoss = new int[] {};
		int oprLossInt = 0; // Use integer for check to guard against roundoff
		if (oprLoss > 0.0)
		{
			oprLossInt = (int)(oprLoss + .1);
		}
		else if (oprLoss < 0.0)
		{
			oprLossInt = (int)(oprLoss - .1);
		}
		if (__rightTypeUsesInterveningStructuresWithLoss)
		{
			// Additionally check the OprLoss value (only return true if 1)...
			for (int i = 0; i < rightTypesThatUseInterveningStructuresWithLossCheckOprLoss.Length; i++)
			{
				if (rightTypeNumber == rightTypesThatUseInterveningStructuresWithLossCheckOprLoss[i])
				{
					if (oprLossInt != 0)
					{
						// See right type 24, for example
						return true;
					}
					else
					{
						return false;
					}
				}
			}
			return true;
		}
		return false;
	}

	/// <summary>
	/// FIXME SAM 2011-02-02 Can the "Carrier Allowed" information from the metadata be used?  These values
	/// were taken from the Operating Rule Summary but Ray was not clear on whether this is equivalent to the
	/// intervening structures.  For now handle separately.
	/// <para>
	/// Indicate whether the operational right uses intervening structures.  Intervening structures are not required
	/// but may be used.
	/// </para>
	/// </summary>
	public virtual bool getRightTypeUsesInterveningStructuresWithoutLoss()
	{
		return __rightTypeUsesInterveningStructuresWithoutLoss;
	}

	/// <summary>
	/// Indicate whether the operational right uses limits.
	/// </summary>
	public virtual bool getRightTypeUsesLimits()
	{
		int rightTypeNumber = getRightTypeNumber();
		int[] rightTypesThatUseLimits = new int[] {11, 29, 34, 45, 46, 47};
		for (int i = 0; i < rightTypesThatUseLimits.Length; i++)
		{
			if (rightTypeNumber == rightTypesThatUseLimits[i])
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Indicate whether the operational right uses monthly efficiencies.
	/// For now hard code here but once list is known, may add to metadata.
	/// </summary>
	public virtual bool getRightTypeUsesMonthlyOprEff(StateMod_DataSet dataSet, string source2, string sourceAccount2)
	{
		if (StringUtil.isInteger(sourceAccount2))
		{
			int sourceAccount2Int = Convert.ToInt32(sourceAccount2);
			if (sourceAccount2Int <= 0)
			{
				return false;
			}
		}
		IList<StateMod_Plan> planList = (IList<StateMod_Plan>)dataSet.getComponentForComponentType(StateMod_DataSet.COMP_PLANS).getData();
		int pos = StateMod_Util.IndexOf(planList, source2);
		if (pos >= 0)
		{
			StateMod_Plan plan = planList[pos];
			// Only used with T&C plan types...
			if (plan.getIPlnTyp() != 4)
			{
				return false;
			}
		}
		int rightTypeNumber = getRightTypeNumber();
		int[] rightTypesThatUseMonthlyOprEff = new int[] {24};
		for (int i = 0; i < rightTypesThatUseMonthlyOprEff.Length; i++)
		{
			if (rightTypeNumber == rightTypesThatUseMonthlyOprEff[i])
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Indicate whether the operational right uses monthly operational limits.
	/// For now hard code here but once list is known, may add to metadata.
	/// </summary>
	public virtual bool getRightTypeUsesMonthlyOprMax(double oprLimits)
	{
		int rightTypeNumber = getRightTypeNumber();
		int oprLimitsInt = 0;
		if (oprLimits > 0.0)
		{
			oprLimitsInt = (int)(oprLimits + .1);
		}
		else if (oprLimits < 0.0)
		{
			oprLimitsInt = (int)(oprLimits - .1);
		}
		int[] rightTypesThatUseMonthlyOprLimits = new int[] {24, 47};
		for (int i = 0; i < rightTypesThatUseMonthlyOprLimits.Length; i++)
		{
			if (rightTypeNumber == rightTypesThatUseMonthlyOprLimits[i])
			{
				if (rightTypeNumber == 24)
				{
					// Always use record
					return true;
				}
				else if ((rightTypeNumber == 47) && (oprLimitsInt == 1))
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}
		return false;
	}

	/// <summary>
	/// Indicate whether the operational right uses monthly switches.  Switches are not required, but may be used.
	/// </summary>
	public virtual bool getRightTypeUsesMonthlySwitch()
	{
		// TODO SAM 2011-01-29 For now always return true.  If any right types are enabled that do not
		// use the monthly switch, return false
		return true;
	}

	/// <summary>
	/// Indicate whether the operational right uses negative reservoir destination accounts.
	/// For now hard code here but once list is known, may add to metadata.
	/// </summary>
	public virtual bool getRightTypeUsesNegativeDestinationAccounts()
	{
		int rightTypeNumber = getRightTypeNumber();
		int[] rightTypesThatUseNegative = new int[] {2, 3, 5, 6, 7, 11, 14, 31, 32, 33, 34, 35, 38, 41, 45};
		for (int i = 0; i < rightTypesThatUseNegative.Length; i++)
		{
			if (rightTypeNumber == rightTypesThatUseNegative[i])
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Indicate whether the operational right uses Rio Grande data.
	/// For now hard code here but once list is known, may add to metadata.
	/// </summary>
	public virtual bool getRightTypeUsesRioGrande()
	{
		int rightTypeNumber = getRightTypeNumber();
		int[] rightTypesThatUseRioGrande = new int[] {17, 18};
		for (int i = 0; i < rightTypesThatUseRioGrande.Length; i++)
		{
			if (rightTypeNumber == rightTypesThatUseRioGrande[i])
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Indicate whether the operational right uses San Juan data.
	/// For now hard code here but once list is known, may add to metadata.
	/// </summary>
	public virtual bool getRightTypeUsesSanJuan()
	{
		int rightTypeNumber = getRightTypeNumber();
		int[] rightTypesThatUseSanJuan = new int[] {20};
		for (int i = 0; i < rightTypesThatUseSanJuan.Length; i++)
		{
			if (rightTypeNumber == rightTypesThatUseSanJuan[i])
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Indicate whether the operational right uses the source 1.
	/// </summary>
	public virtual bool getRightTypeUsesSource1()
	{
		StateMod_OperationalRight_Metadata_SourceOrDestinationType[] allowedSourceTypes = getSource1Types();
		if ((allowedSourceTypes.Length == 0) || (allowedSourceTypes[0] == StateMod_OperationalRight_Metadata_SourceOrDestinationType.NA))
		{
			return false;
		}
		else
		{
			return true;
		}
	}

	/// <summary>
	/// Indicate whether the operational right uses the source 2.
	/// </summary>
	public virtual bool getRightTypeUsesSource2()
	{
		StateMod_OperationalRight_Metadata_SourceOrDestinationType[] allowedSourceTypes = getSource2Types();
		// Special cases (dynamic data expected)
		int rightTypeNumber = getRightTypeNumber();
		int[] rightTypesThatUseDynamicSource2Data = new int[] {48, 49};
		for (int i = 0; i < rightTypesThatUseDynamicSource2Data.Length; i++)
		{
			if (rightTypeNumber == rightTypesThatUseDynamicSource2Data[i])
			{
				return true;
			}
		}
		if ((allowedSourceTypes.Length == 0) || (allowedSourceTypes[0] == StateMod_OperationalRight_Metadata_SourceOrDestinationType.NA))
		{
			return false;
		}
		else
		{
			return true;
		}
	}

	/// <summary>
	/// Indicate whether the operational right uses special destination account choices.
	/// </summary>
	public virtual bool getRightTypeUsesSpecialDestinationAccount()
	{
		int rightTypeNumber = getRightTypeNumber();
		int[] rightsThatUseSpecialDestinationAccount = new int[] {17, 18, 46, 48, 49};
		for (int i = 0; i < rightsThatUseSpecialDestinationAccount.Length; i++)
		{
			if (rightTypeNumber == rightsThatUseSpecialDestinationAccount[i])
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Indicate whether the operational right uses the special source 2 choices.
	/// </summary>
	public virtual bool getRightTypeUsesSpecialSource2()
	{
		int rightTypeNumber = getRightTypeNumber();
		int[] rightsThatUseSpecialSource2 = new int[] {6, 34};
		for (int i = 0; i < rightsThatUseSpecialSource2.Length; i++)
		{
			if (rightTypeNumber == rightsThatUseSpecialSource2[i])
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Indicate whether the operational right uses the special source account 1 choices.
	/// </summary>
	public virtual bool getRightTypeUsesSpecialSourceAccount1()
	{
		int rightTypeNumber = getRightTypeNumber();
		int[] rightsThatUseSpecialSourceAccount1 = new int[] {9, 11, 14, 15, 16, 17, 18, 20, 24, 39, 45};
		for (int i = 0; i < rightsThatUseSpecialSourceAccount1.Length; i++)
		{
			if (rightTypeNumber == rightsThatUseSpecialSourceAccount1[i])
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Indicate whether the operational right uses the special source account 2 choices.
	/// </summary>
	public virtual bool getRightTypeUsesSpecialSourceAccount2()
	{
		int rightTypeNumber = getRightTypeNumber();
		int[] rightsThatUseSpecialSourceAccount2 = new int[] {2, 3, 4, 6, 7, 14, 15, 16, 17, 18, 24, 32, 33, 39, 45};
		for (int i = 0; i < rightsThatUseSpecialSourceAccount2.Length; i++)
		{
			if (rightTypeNumber == rightsThatUseSpecialSourceAccount2[i])
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// TODO SAM 2011-01-31 Move to metadata if a pattern emerges - and are these additional or replacements
	/// For now interpret as additional (added to empty or non-empty list when used).
	/// Return the special source 1 choices
	/// </summary>
	public virtual IList<string> getSourceAccount1SpecialChoices()
	{
		IList<string> specialChoices = new List<object>();
		int rightTypeNumber = getRightTypeNumber();
		if (rightTypeNumber == 9)
		{
			specialChoices.Add("0 - Meet target by releasing from each account");
		}
		else if (rightTypeNumber == 11)
		{
			specialChoices.Add("0 - Source water right is left on");
			specialChoices.Add("1 - Source water right is turned off");
		}
		else if (rightTypeNumber == 13)
		{
			for (int i = 0; i <= 100; i++)
			{
				specialChoices.Add("" + i + " - Percent of index flow station available");
			}
		}
		else if (rightTypeNumber == 14)
		{
			specialChoices.Add("0 - Source water right is left on");
			specialChoices.Add("1 - Source water right is turned off");
		}
		else if (rightTypeNumber == 15)
		{
			specialChoices.Add("0 - Natural streamflow below which rule is used");
		}
		else if (rightTypeNumber == 16)
		{
			specialChoices.Add("0 - Source water right is left on");
			specialChoices.Add("1 - Source water right is turned off");
		}
		else if (rightTypeNumber == 17)
		{
			// TODO SAM 2011-02-05 Need to handle numbers better but for now put in reasonable variations
			specialChoices.Add("1 - Coefficient");
			specialChoices.Add("1. - Coefficient");
			specialChoices.Add("1.0 - Coefficient");
		}
		else if (rightTypeNumber == 18)
		{
			specialChoices.Add("1 - Coefficient");
			specialChoices.Add("1. - Coefficient");
			specialChoices.Add("1.0 - Coefficient");
		}
		else if (rightTypeNumber == 20)
		{
			specialChoices.Add("0 - Meet target by releasing from each account");
		}
		else if (rightTypeNumber == 24)
		{
			for (int i = 0; i <= 100; i++)
			{
				specialChoices.Add("" + i + " - Percent of source water right to exchange");
			}
		}
		else if (rightTypeNumber == 39)
		{
			specialChoices.Add("0 - Source water right is left on");
			specialChoices.Add("1 - Source water right is turned off");
		}
		else if (rightTypeNumber == 45)
		{
			specialChoices.Add("0 - Source water right is left on");
			specialChoices.Add("1 - Source water right is turned off");
		}
		return specialChoices;
	}

	/// <summary>
	/// TODO SAM 2011-01-31 Move to metadata if a pattern emerges
	/// Return the special source2 types for the right.
	/// </summary>
	public virtual IList<string> getSourceAccount2SpecialChoices()
	{
		IList<string> specialChoices = new List<object>();
		int rightTypeNumber = getRightTypeNumber();
		if (rightTypeNumber == 2)
		{
			specialChoices.Add("0 - Reservoir demand is not adjusted");
			specialChoices.Add("-1 - Provide depletion replacement");
			for (int i = 1; i <= 100; i++)
			{
				specialChoices.Add("" + i + " - Limit reservoir demand to CIR/n");
			}
		}
		else if (rightTypeNumber == 3)
		{
			specialChoices.Add("0 - Reservoir demand is not adjusted");
			for (int i = 1; i <= 100; i++)
			{
				specialChoices.Add("" + i + " - Limit reservoir demand to CIR/n");
			}
		}
		else if (rightTypeNumber == 4)
		{
			specialChoices.Add("0 - Provide 100% replacement");
			specialChoices.Add("-1 - Provide depletion replacement");
		}
		else if (rightTypeNumber == 5)
		{
			specialChoices.Add("0 - Reservoir demand is not adjusted");
			for (int i = 1; i <= 100; i++)
			{
				specialChoices.Add("" + i + " - Limit reservoir demand to CIR/n");
			}
		}
		else if (rightTypeNumber == 6)
		{
			specialChoices.Add("0 - specify if Source 2 is an operating rule");
			specialChoices.Add("99 - specify if Source 2 is a diversion");
		}
		else if (rightTypeNumber == 7)
		{
			specialChoices.Add("0 - Reservoir demand is not adjusted");
			for (int i = 1; i <= 100; i++)
			{
				specialChoices.Add("" + i + " - Limit reservoir demand to CIR/n");
			}
		}
		else if (rightTypeNumber == 14)
		{
			specialChoices.Add("1 - Monthly diversion limit is in direct diversion demand");
			specialChoices.Add("99 - Annual diversion limit (ACFT)");
			// TODO SAM 2011-02-04 Need to allow annual diversion limit as number
		}
		else if (rightTypeNumber == 15)
		{
			specialChoices.Add("0 - Allow 100% of the decree to be diverted");
			specialChoices.Add("-1 - Allow depletion (CU) to be diverted");
		}
		else if (rightTypeNumber == 16)
		{
			specialChoices.Add("99 - Maximum direct flow storage (%)");
			// TODO SAM 2011-02-04 Need to allow as number
		}
		else if (rightTypeNumber == 17)
		{
			// TODO SAM 2011-02-05 Need to handle numbers better but for now put in reasonable variations
			specialChoices.Add("-1 - Coefficient");
			specialChoices.Add("-1. - Coefficient");
			specialChoices.Add("-1.0 - Coefficient");
		}
		else if (rightTypeNumber == 18)
		{
			specialChoices.Add("1 - Coefficient");
			specialChoices.Add("1. - Coefficient");
			specialChoices.Add("1.0 - Coefficient");
		}
		else if (rightTypeNumber == 24)
		{
			specialChoices.Add("0 - if Source 2 is NA");
			specialChoices.Add("1 - for a stndard return pattern");
			specialChoices.Add("3 - for a mixed return pattern");
			specialChoices.Add("4 - for a default (source) return pattern");
			specialChoices.Add("-1 - the T&C will be calculated upon release");
		}
		else if (rightTypeNumber == 32)
		{
			specialChoices.Add("0 - Reservoir demand is not adjusted");
			for (int i = 1; i <= 100; i++)
			{
				specialChoices.Add("" + i + " - Limit reservoir demand to CIR/n");
			}
		}
		else if (rightTypeNumber == 33)
		{
			specialChoices.Add("0 - Reservoir demand is not adjusted");
			for (int i = 1; i <= 100; i++)
			{
				specialChoices.Add("" + i + " - Limit reservoir demand to CIR/n");
			}
		}
		else if (rightTypeNumber == 39)
		{
			specialChoices.Add("1 - Required");
		}
		else if (rightTypeNumber == 45)
		{
			for (int i = 1; i <= 100; i++)
			{
				specialChoices.Add("" + i + " - Percent of water right to use as source");
			}
		}
		return specialChoices;
	}

	/// <summary>
	/// Return the allowed source1 types for the right.
	/// </summary>
	public virtual StateMod_OperationalRight_Metadata_SourceOrDestinationType [] getSource1Types()
	{
		return __source1Types;
	}

	/// <summary>
	/// TODO SAM 2011-01-31 Move to metadata if a pattern emerges - and are these additional or replacements
	/// For now interpret as additional (added to empty or non-empty list when used).
	/// Return the special source 1 choices
	/// </summary>
	public virtual IList<string> getSource2SpecialChoices()
	{
		IList<string> specialChoices = new List<object>();
		int rightTypeNumber = getRightTypeNumber();
		if (rightTypeNumber == 6)
		{
			specialChoices.Add("0 - Not required");
		}
		else if (rightTypeNumber == 34)
		{
			specialChoices.Add("0 - Not required");
		}
		return specialChoices;
	}

	/// <summary>
	/// Return the allowed source2 types for the right.
	/// </summary>
	public virtual StateMod_OperationalRight_Metadata_SourceOrDestinationType [] getSource2Types()
	{
		return __source2Types;
	}

	/// <summary>
	/// Return whether transit and conveyance loss is allowed for the right type.
	/// </summary>
	public virtual StateMod_OperationalRight_Metadata_TransitAndConveyanceLossAllowedType getTransitAndConveyanceLossAllowedType()
	{
		return __transitAndConveyanceLossAllowed;
	}

	/// <summary>
	/// Initialize the singleton list of operational right metadata.  Do this rather than having a large amount
	/// of static data in memory.
	/// </summary>
	private static void initialize()
	{
		if (__opRightsMetadataList != null)
		{
			// No need to initialize.
			return;
		}
		__opRightsMetadataList = new List<object>();
		// Initialize the list of metadata
		StateMod_OperationalRight_Metadata metaData = null;
		// Loop through the number of known operational right types.  This should ideally be the
		// same number as StateMod_OperationalRight.MAX_HANDLED_TYPE but may be different during
		// development
		for (int i = 1; i <= 49; i++)
		{
			switch (i)
			{
				case 1:
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source1Array_1 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.RESERVOIR};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source2Array_1 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.NA};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] destinationArray_1 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.INSTREAM_FLOW};
					StateMod_OperationalRight_Metadata_DestinationLocationType[] destinationLocationArray_1 = new StateMod_OperationalRight_Metadata_DestinationLocationType[] {StateMod_OperationalRight_Metadata_DestinationLocationType.DOWNSTREAM};
					StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] associatedPlanAllowedArray_1 = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] {StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.NA};
					StateMod_OperationalRight_Metadata_DiversionType[] diversionTypeArray_1 = new StateMod_OperationalRight_Metadata_DiversionType[] {StateMod_OperationalRight_Metadata_DiversionType.NA};
						// Summary has...
						//{StateMod_OperationalRight_Metadata_DiversionType.DIVERSION};
					metaData = new StateMod_OperationalRight_Metadata(i, true, "Reservoir Release to an Instream Flow", StateMod_OperationalRight_Metadata_RuleType.SOURCE_RESERVOIR, source1Array_1, source2Array_1, StateMod_OperationalRight_Metadata_Source2AllowedType.NA, destinationArray_1, destinationLocationArray_1, StateMod_OperationalRight_Metadata_DeliveryMethodType.RIVER, StateMod_OperationalRight_Metadata_CarrierAllowedType.NA, false, false, associatedPlanAllowedArray_1, diversionTypeArray_1, StateMod_OperationalRight_Metadata_TransitAndConveyanceLossAllowedType.NA, "");
					break;
				case 2:
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source1Array_2 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.RESERVOIR};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source2Array_2 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.NA};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] destinationArray_2 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.DIVERSION, StateMod_OperationalRight_Metadata_SourceOrDestinationType.RESERVOIR};
					StateMod_OperationalRight_Metadata_DestinationLocationType[] destinationLocationArray_2 = new StateMod_OperationalRight_Metadata_DestinationLocationType[] {StateMod_OperationalRight_Metadata_DestinationLocationType.DOWNSTREAM};
					StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] associatedPlanAllowedArray_2 = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] {StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.NA};
					StateMod_OperationalRight_Metadata_DiversionType[] diversionTypeArray_2 = new StateMod_OperationalRight_Metadata_DiversionType[] {StateMod_OperationalRight_Metadata_DiversionType.NA};
						// Summary has the following...
						//{StateMod_OperationalRight_Metadata_DiversionType.DIVERSION};
					metaData = new StateMod_OperationalRight_Metadata(i, true, "Reservoir Release to a Diversion, Reservoir, or Carrier", StateMod_OperationalRight_Metadata_RuleType.SOURCE_RESERVOIR, source1Array_2, source2Array_2, StateMod_OperationalRight_Metadata_Source2AllowedType.NA, destinationArray_2, destinationLocationArray_2, StateMod_OperationalRight_Metadata_DeliveryMethodType.RIVER, StateMod_OperationalRight_Metadata_CarrierAllowedType.YES, true, false, associatedPlanAllowedArray_2, diversionTypeArray_2, StateMod_OperationalRight_Metadata_TransitAndConveyanceLossAllowedType.NA, "");
					break;
				case 3:
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source1Array_3 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.RESERVOIR};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source2Array_3 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.NA};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] destinationArray_3 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.DIVERSION, StateMod_OperationalRight_Metadata_SourceOrDestinationType.RESERVOIR};
					StateMod_OperationalRight_Metadata_DestinationLocationType[] destinationLocationArray_3 = new StateMod_OperationalRight_Metadata_DestinationLocationType[] {StateMod_OperationalRight_Metadata_DestinationLocationType.DOWNSTREAM};
					StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] associatedPlanAllowedArray_3 = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] {StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.NA};
					StateMod_OperationalRight_Metadata_DiversionType[] diversionTypeArray_3 = new StateMod_OperationalRight_Metadata_DiversionType[] {StateMod_OperationalRight_Metadata_DiversionType.DIVERSION};
					metaData = new StateMod_OperationalRight_Metadata(i, true, "Reservoir Release to a Direct Diversion or Reservoir by a Carrier", StateMod_OperationalRight_Metadata_RuleType.SOURCE_RESERVOIR, source1Array_3, source2Array_3, StateMod_OperationalRight_Metadata_Source2AllowedType.NA, destinationArray_3, destinationLocationArray_3, StateMod_OperationalRight_Metadata_DeliveryMethodType.RIVER, StateMod_OperationalRight_Metadata_CarrierAllowedType.YES, true, false, associatedPlanAllowedArray_3, diversionTypeArray_3, StateMod_OperationalRight_Metadata_TransitAndConveyanceLossAllowedType.NA, "");
					break;
				case 4:
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source1Array_4 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.RESERVOIR};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source2Array_4 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.NA};
					// TODO SAM 2011-02-02 summary does not include right
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] destinationArray_4 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.DIVERSION, StateMod_OperationalRight_Metadata_SourceOrDestinationType.DIVERSION_RIGHT};
					StateMod_OperationalRight_Metadata_DestinationLocationType[] destinationLocationArray_4 = new StateMod_OperationalRight_Metadata_DestinationLocationType[] {StateMod_OperationalRight_Metadata_DestinationLocationType.UPSTREAM};
					StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] associatedPlanAllowedArray_4 = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] {StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.NA};
					StateMod_OperationalRight_Metadata_DiversionType[] diversionTypeArray_4 = new StateMod_OperationalRight_Metadata_DiversionType[] {StateMod_OperationalRight_Metadata_DiversionType.NA};
					metaData = new StateMod_OperationalRight_Metadata(i, true, "Reservoir Release to a Direct Diversion by Exchange with the River", StateMod_OperationalRight_Metadata_RuleType.SOURCE_RESERVOIR, source1Array_4, source2Array_4, StateMod_OperationalRight_Metadata_Source2AllowedType.NA, destinationArray_4, destinationLocationArray_4, StateMod_OperationalRight_Metadata_DeliveryMethodType.RIVER, StateMod_OperationalRight_Metadata_CarrierAllowedType.NA, false, false, associatedPlanAllowedArray_4, diversionTypeArray_4, StateMod_OperationalRight_Metadata_TransitAndConveyanceLossAllowedType.NA, "");
					break;
				case 5:
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source1Array_5 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.RESERVOIR};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source2Array_5 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.NA};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] destinationArray_5 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.RESERVOIR, StateMod_OperationalRight_Metadata_SourceOrDestinationType.RESERVOIR_RIGHT};
					StateMod_OperationalRight_Metadata_DestinationLocationType[] destinationLocationArray_5 = new StateMod_OperationalRight_Metadata_DestinationLocationType[] {StateMod_OperationalRight_Metadata_DestinationLocationType.DOWNSTREAM};
					StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] associatedPlanAllowedArray_5 = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] {StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.NA};
					// TODO SAM 2011-02-02 Summary had Diversion
					StateMod_OperationalRight_Metadata_DiversionType[] diversionTypeArray_5 = new StateMod_OperationalRight_Metadata_DiversionType[] {StateMod_OperationalRight_Metadata_DiversionType.NA};
					metaData = new StateMod_OperationalRight_Metadata(i, true, "Reservoir Storage by Exchange", StateMod_OperationalRight_Metadata_RuleType.SOURCE_RESERVOIR, source1Array_5, source2Array_5, StateMod_OperationalRight_Metadata_Source2AllowedType.NA, destinationArray_5, destinationLocationArray_5, StateMod_OperationalRight_Metadata_DeliveryMethodType.RIVER, StateMod_OperationalRight_Metadata_CarrierAllowedType.NA, false, false, associatedPlanAllowedArray_5, diversionTypeArray_5, StateMod_OperationalRight_Metadata_TransitAndConveyanceLossAllowedType.NA, "");
					break;
				case 6:
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source1Array_6 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.RESERVOIR};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source2Array_6 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.OPERATIONAL_RIGHT, StateMod_OperationalRight_Metadata_SourceOrDestinationType.DIVERSION};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] destinationArray_6 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.RESERVOIR};
					StateMod_OperationalRight_Metadata_DestinationLocationType[] destinationLocationArray_6 = new StateMod_OperationalRight_Metadata_DestinationLocationType[] {StateMod_OperationalRight_Metadata_DestinationLocationType.SOURCE};
					StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] associatedPlanAllowedArray_6 = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] {StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.NA};
					// TODO SAM 2011-02-02 Summary had Diversion
					StateMod_OperationalRight_Metadata_DiversionType[] diversionTypeArray_6 = new StateMod_OperationalRight_Metadata_DiversionType[] {StateMod_OperationalRight_Metadata_DiversionType.NA};
					metaData = new StateMod_OperationalRight_Metadata(i, true, "Paper Exchange Between Reservoirs", StateMod_OperationalRight_Metadata_RuleType.SOURCE_RESERVOIR, source1Array_6, source2Array_6, StateMod_OperationalRight_Metadata_Source2AllowedType.NA, destinationArray_6, destinationLocationArray_6, StateMod_OperationalRight_Metadata_DeliveryMethodType.BOOKOVER, StateMod_OperationalRight_Metadata_CarrierAllowedType.NA, false, false, associatedPlanAllowedArray_6, diversionTypeArray_6, StateMod_OperationalRight_Metadata_TransitAndConveyanceLossAllowedType.NA, "");
					break;
				case 7:
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source1Array_7 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.RESERVOIR};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source2Array_7 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.NA};
					// TODO SAM 2011-02-02 Summary has Carrier
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] destinationArray_7 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.OPERATIONAL_RIGHT};
					StateMod_OperationalRight_Metadata_DestinationLocationType[] destinationLocationArray_7 = new StateMod_OperationalRight_Metadata_DestinationLocationType[] {StateMod_OperationalRight_Metadata_DestinationLocationType.UPSTREAM};
					StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] associatedPlanAllowedArray_7 = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] {StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.NA};
					// TODO SAM 2011-02-02 Summary has Diversion
					StateMod_OperationalRight_Metadata_DiversionType[] diversionTypeArray_7 = new StateMod_OperationalRight_Metadata_DiversionType[] {StateMod_OperationalRight_Metadata_DiversionType.NA};
					metaData = new StateMod_OperationalRight_Metadata(i, true, "Reservoir to a Carrier by Exchange", StateMod_OperationalRight_Metadata_RuleType.SOURCE_RESERVOIR, source1Array_7, source2Array_7, StateMod_OperationalRight_Metadata_Source2AllowedType.NA, destinationArray_7, destinationLocationArray_7, StateMod_OperationalRight_Metadata_DeliveryMethodType.RIVER, StateMod_OperationalRight_Metadata_CarrierAllowedType.YES, true, false, associatedPlanAllowedArray_7, diversionTypeArray_7, StateMod_OperationalRight_Metadata_TransitAndConveyanceLossAllowedType.NA, "");
					break;
				case 8:
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source1Array_8 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.RESERVOIR};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source2Array_8 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.NA};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] destinationArray_8 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.RESERVOIR};
					StateMod_OperationalRight_Metadata_DestinationLocationType[] destinationLocationArray_8 = new StateMod_OperationalRight_Metadata_DestinationLocationType[] {StateMod_OperationalRight_Metadata_DestinationLocationType.DOWNSTREAM};
					StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] associatedPlanAllowedArray_8 = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] {StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.NA};
					StateMod_OperationalRight_Metadata_DiversionType[] diversionTypeArray_8 = new StateMod_OperationalRight_Metadata_DiversionType[] {StateMod_OperationalRight_Metadata_DiversionType.DIVERSION};
					metaData = new StateMod_OperationalRight_Metadata(i, false, "Out of Priority Reservoir Bookover", StateMod_OperationalRight_Metadata_RuleType.SOURCE_RESERVOIR, source1Array_8, source2Array_8, StateMod_OperationalRight_Metadata_Source2AllowedType.NA, destinationArray_8, destinationLocationArray_8, StateMod_OperationalRight_Metadata_DeliveryMethodType.RIVER, StateMod_OperationalRight_Metadata_CarrierAllowedType.NA, false, false, associatedPlanAllowedArray_8, diversionTypeArray_8, StateMod_OperationalRight_Metadata_TransitAndConveyanceLossAllowedType.NA, "");
					break;
				case 9:
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source1Array_9 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.RESERVOIR};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source2Array_9 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.NA};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] destinationArray_9 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.NA};
					StateMod_OperationalRight_Metadata_DestinationLocationType[] destinationLocationArray_9 = new StateMod_OperationalRight_Metadata_DestinationLocationType[] {StateMod_OperationalRight_Metadata_DestinationLocationType.DOWNSTREAM};
					StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] associatedPlanAllowedArray_9 = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] {StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.NA};
					// TODO SAM 2011-02-02 summary had Diversion
					StateMod_OperationalRight_Metadata_DiversionType[] diversionTypeArray_9 = new StateMod_OperationalRight_Metadata_DiversionType[] {StateMod_OperationalRight_Metadata_DiversionType.NA};
					metaData = new StateMod_OperationalRight_Metadata(i, true, "Reservoir Release to Meet Target", StateMod_OperationalRight_Metadata_RuleType.SOURCE_RESERVOIR, source1Array_9, source2Array_9, StateMod_OperationalRight_Metadata_Source2AllowedType.NA, destinationArray_9, destinationLocationArray_9, StateMod_OperationalRight_Metadata_DeliveryMethodType.RIVER, StateMod_OperationalRight_Metadata_CarrierAllowedType.NA, false, false, associatedPlanAllowedArray_9, diversionTypeArray_9, StateMod_OperationalRight_Metadata_TransitAndConveyanceLossAllowedType.NA, "");
					break;
				case 10:
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source1Array_10 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.RESERVOIR};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source2Array_10 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.NA};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] destinationArray_10 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.DIVERSION};
					StateMod_OperationalRight_Metadata_DestinationLocationType[] destinationLocationArray_10 = new StateMod_OperationalRight_Metadata_DestinationLocationType[] {StateMod_OperationalRight_Metadata_DestinationLocationType.DOWNSTREAM};
					StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] associatedPlanAllowedArray_10 = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] {StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.NA};
					StateMod_OperationalRight_Metadata_DiversionType[] diversionTypeArray_10 = new StateMod_OperationalRight_Metadata_DiversionType[] {StateMod_OperationalRight_Metadata_DiversionType.DIVERSION, StateMod_OperationalRight_Metadata_DiversionType.DEPLETION};
					metaData = new StateMod_OperationalRight_Metadata(i, false, "General Replacement Reservoir to a Diversion by a Direct Release or Exchange", StateMod_OperationalRight_Metadata_RuleType.SOURCE_RESERVOIR, source1Array_10, source2Array_10, StateMod_OperationalRight_Metadata_Source2AllowedType.NA, destinationArray_10, destinationLocationArray_10, StateMod_OperationalRight_Metadata_DeliveryMethodType.RIVER, StateMod_OperationalRight_Metadata_CarrierAllowedType.NA, false, false, associatedPlanAllowedArray_10, diversionTypeArray_10, StateMod_OperationalRight_Metadata_TransitAndConveyanceLossAllowedType.NA, "StateMod determines if the supply is the river or by exchange");
					break;
				case 11:
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source1Array_11 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.DIVERSION_RIGHT, StateMod_OperationalRight_Metadata_SourceOrDestinationType.RESERVOIR_RIGHT};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source2Array_11 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.NA};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] destinationArray_11 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.DIVERSION, StateMod_OperationalRight_Metadata_SourceOrDestinationType.RESERVOIR};
					StateMod_OperationalRight_Metadata_DestinationLocationType[] destinationLocationArray_11 = new StateMod_OperationalRight_Metadata_DestinationLocationType[] {StateMod_OperationalRight_Metadata_DestinationLocationType.DOWNSTREAM};
					StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] associatedPlanAllowedArray_11 = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] {StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.NA, StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.PLAN_RECHARGE};
					// TODO SAM 2011-02-02 Summary had Diversion
					StateMod_OperationalRight_Metadata_DiversionType[] diversionTypeArray_11 = new StateMod_OperationalRight_Metadata_DiversionType[] {StateMod_OperationalRight_Metadata_DiversionType.NA};
					metaData = new StateMod_OperationalRight_Metadata(i, true, "Carrier to a Ditch or Reservoir", StateMod_OperationalRight_Metadata_RuleType.SOURCE_DIRECT_FLOW_RIGHT, source1Array_11, source2Array_11, StateMod_OperationalRight_Metadata_Source2AllowedType.NA, destinationArray_11, destinationLocationArray_11, StateMod_OperationalRight_Metadata_DeliveryMethodType.CARRIER, StateMod_OperationalRight_Metadata_CarrierAllowedType.YES, true, false, associatedPlanAllowedArray_11, diversionTypeArray_11, StateMod_OperationalRight_Metadata_TransitAndConveyanceLossAllowedType.NA, "Same as type 32 but does not allow reuse and loss");
					break;
				case 12:
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source1Array_12 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.NA};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source2Array_12 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.NA};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] destinationArray_12 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.NA};
					StateMod_OperationalRight_Metadata_DestinationLocationType[] destinationLocationArray_12 = new StateMod_OperationalRight_Metadata_DestinationLocationType[] {StateMod_OperationalRight_Metadata_DestinationLocationType.DOWNSTREAM};
					StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] associatedPlanAllowedArray_12 = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] {StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.NA};
					StateMod_OperationalRight_Metadata_DiversionType[] diversionTypeArray_12 = new StateMod_OperationalRight_Metadata_DiversionType[] {StateMod_OperationalRight_Metadata_DiversionType.NA};
					metaData = new StateMod_OperationalRight_Metadata(i, true, "Reoperate Water Rights", StateMod_OperationalRight_Metadata_RuleType.OTHER, source1Array_12, source2Array_12, StateMod_OperationalRight_Metadata_Source2AllowedType.NA, destinationArray_12, destinationLocationArray_12, StateMod_OperationalRight_Metadata_DeliveryMethodType.RIVER, StateMod_OperationalRight_Metadata_CarrierAllowedType.NA, false, false, associatedPlanAllowedArray_12, diversionTypeArray_12, StateMod_OperationalRight_Metadata_TransitAndConveyanceLossAllowedType.NA, "");
					break;
				case 13:
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source1Array_13 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.STREAM_GAGE};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source2Array_13 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.INSTREAM_FLOW_RIGHT};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] destinationArray_13 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.INSTREAM_FLOW};
					StateMod_OperationalRight_Metadata_DestinationLocationType[] destinationLocationArray_13 = new StateMod_OperationalRight_Metadata_DestinationLocationType[] {StateMod_OperationalRight_Metadata_DestinationLocationType.DOWNSTREAM};
					StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] associatedPlanAllowedArray_13 = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] {StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.NA};
					// TODO SAM 2011-02-04 Summary has Diversion
					StateMod_OperationalRight_Metadata_DiversionType[] diversionTypeArray_13 = new StateMod_OperationalRight_Metadata_DiversionType[] {StateMod_OperationalRight_Metadata_DiversionType.NA};
					metaData = new StateMod_OperationalRight_Metadata(i, true, "La Plata Compact (Index Flow Contraint on Stream Gage)", StateMod_OperationalRight_Metadata_RuleType.COMPACT, source1Array_13, source2Array_13, StateMod_OperationalRight_Metadata_Source2AllowedType.NA, destinationArray_13, destinationLocationArray_13, StateMod_OperationalRight_Metadata_DeliveryMethodType.RIVER, StateMod_OperationalRight_Metadata_CarrierAllowedType.NA, false, false, associatedPlanAllowedArray_13, diversionTypeArray_13, StateMod_OperationalRight_Metadata_TransitAndConveyanceLossAllowedType.NA, "");
					break;
				case 14:
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source1Array_14 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.DIVERSION_RIGHT};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source2Array_14 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.NA};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] destinationArray_14 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.DIVERSION, StateMod_OperationalRight_Metadata_SourceOrDestinationType.RESERVOIR};
					StateMod_OperationalRight_Metadata_DestinationLocationType[] destinationLocationArray_14 = new StateMod_OperationalRight_Metadata_DestinationLocationType[] {StateMod_OperationalRight_Metadata_DestinationLocationType.DOWNSTREAM};
					StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] associatedPlanAllowedArray_14 = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] {StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.NA};
					// TODO SAM 2011-02-04 summary has Diversion
					StateMod_OperationalRight_Metadata_DiversionType[] diversionTypeArray_14 = new StateMod_OperationalRight_Metadata_DiversionType[] {StateMod_OperationalRight_Metadata_DiversionType.NA};
					metaData = new StateMod_OperationalRight_Metadata(i, true, "Carrier Right with Constrained Demand", StateMod_OperationalRight_Metadata_RuleType.SOURCE_DIRECT_FLOW_RIGHT, source1Array_14, source2Array_14, StateMod_OperationalRight_Metadata_Source2AllowedType.NA, destinationArray_14, destinationLocationArray_14, StateMod_OperationalRight_Metadata_DeliveryMethodType.RIVER, StateMod_OperationalRight_Metadata_CarrierAllowedType.YES, true, false, associatedPlanAllowedArray_14, diversionTypeArray_14, StateMod_OperationalRight_Metadata_TransitAndConveyanceLossAllowedType.NA, "");
					break;
				case 15:
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source1Array_15 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.DIVERSION_RIGHT};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source2Array_15 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.STREAM_GAGE};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] destinationArray_15 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.INSTREAM_FLOW};
					StateMod_OperationalRight_Metadata_DestinationLocationType[] destinationLocationArray_15 = new StateMod_OperationalRight_Metadata_DestinationLocationType[] {StateMod_OperationalRight_Metadata_DestinationLocationType.DOWNSTREAM};
					StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] associatedPlanAllowedArray_15 = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] {StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.NA};
					// TODO SAM 2011-02-04 Summary has Diversion
					StateMod_OperationalRight_Metadata_DiversionType[] diversionTypeArray_15 = new StateMod_OperationalRight_Metadata_DiversionType[] {StateMod_OperationalRight_Metadata_DiversionType.NA};
					metaData = new StateMod_OperationalRight_Metadata(i, true, "Interruptible Supply", StateMod_OperationalRight_Metadata_RuleType.SOURCE_DIRECT_FLOW_RIGHT, source1Array_15, source2Array_15, StateMod_OperationalRight_Metadata_Source2AllowedType.NA, destinationArray_15, destinationLocationArray_15, StateMod_OperationalRight_Metadata_DeliveryMethodType.RIVER, StateMod_OperationalRight_Metadata_CarrierAllowedType.NA, true, false, associatedPlanAllowedArray_15, diversionTypeArray_15, StateMod_OperationalRight_Metadata_TransitAndConveyanceLossAllowedType.NA, "");
					break;
				case 16:
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source1Array_16 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.DIVERSION_RIGHT};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source2Array_16 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.NA};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] destinationArray_16 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.RESERVOIR};
					StateMod_OperationalRight_Metadata_DestinationLocationType[] destinationLocationArray_16 = new StateMod_OperationalRight_Metadata_DestinationLocationType[] {StateMod_OperationalRight_Metadata_DestinationLocationType.UPSTREAM};
					StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] associatedPlanAllowedArray_16 = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] {StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.NA};
					// TODO SAM 2011-02-04 Summary has Diversion
					StateMod_OperationalRight_Metadata_DiversionType[] diversionTypeArray_16 = new StateMod_OperationalRight_Metadata_DiversionType[] {StateMod_OperationalRight_Metadata_DiversionType.NA};
					metaData = new StateMod_OperationalRight_Metadata(i, true, "Direct Flow Storage", StateMod_OperationalRight_Metadata_RuleType.SOURCE_DIRECT_FLOW_RIGHT, source1Array_16, source2Array_16, StateMod_OperationalRight_Metadata_Source2AllowedType.NA, destinationArray_16, destinationLocationArray_16, StateMod_OperationalRight_Metadata_DeliveryMethodType.RIVER, StateMod_OperationalRight_Metadata_CarrierAllowedType.NA, true, false, associatedPlanAllowedArray_16, diversionTypeArray_16, StateMod_OperationalRight_Metadata_TransitAndConveyanceLossAllowedType.NA, "");
					break;
				case 17:
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source1Array_17 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.STREAM_GAGE};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source2Array_17 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.STREAM_GAGE};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] destinationArray_17 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.INSTREAM_FLOW};
					StateMod_OperationalRight_Metadata_DestinationLocationType[] destinationLocationArray_17 = new StateMod_OperationalRight_Metadata_DestinationLocationType[] {StateMod_OperationalRight_Metadata_DestinationLocationType.DOWNSTREAM};
					StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] associatedPlanAllowedArray_17 = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] {StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.NA};
					// TODO SAM 2011-02-05 summary has Diversion
					StateMod_OperationalRight_Metadata_DiversionType[] diversionTypeArray_17 = new StateMod_OperationalRight_Metadata_DiversionType[] {StateMod_OperationalRight_Metadata_DiversionType.NA};
					metaData = new StateMod_OperationalRight_Metadata(i, true, "Rio Grande Compact (Rio Grande)", StateMod_OperationalRight_Metadata_RuleType.COMPACT, source1Array_17, source2Array_17, StateMod_OperationalRight_Metadata_Source2AllowedType.REQUIRED, destinationArray_17, destinationLocationArray_17, StateMod_OperationalRight_Metadata_DeliveryMethodType.RIVER, StateMod_OperationalRight_Metadata_CarrierAllowedType.NA, false, false, associatedPlanAllowedArray_17, diversionTypeArray_17, StateMod_OperationalRight_Metadata_TransitAndConveyanceLossAllowedType.NA, "Compact Data");
					break;
				case 18:
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source1Array_18 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.STREAM_GAGE};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source2Array_18 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.STREAM_GAGE};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] destinationArray_18 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.INSTREAM_FLOW};
					StateMod_OperationalRight_Metadata_DestinationLocationType[] destinationLocationArray_18 = new StateMod_OperationalRight_Metadata_DestinationLocationType[] {StateMod_OperationalRight_Metadata_DestinationLocationType.DOWNSTREAM};
					StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] associatedPlanAllowedArray_18 = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] {StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.NA};
					// TODO SAM 2011-02-05 summary has Diversion
					StateMod_OperationalRight_Metadata_DiversionType[] diversionTypeArray_18 = new StateMod_OperationalRight_Metadata_DiversionType[] {StateMod_OperationalRight_Metadata_DiversionType.NA};
					metaData = new StateMod_OperationalRight_Metadata(i, true, "Rio Grande Compact (Conejos)", StateMod_OperationalRight_Metadata_RuleType.COMPACT, source1Array_18, source2Array_18, StateMod_OperationalRight_Metadata_Source2AllowedType.REQUIRED, destinationArray_18, destinationLocationArray_18, StateMod_OperationalRight_Metadata_DeliveryMethodType.RIVER, StateMod_OperationalRight_Metadata_CarrierAllowedType.NA, false, false, associatedPlanAllowedArray_18, diversionTypeArray_18, StateMod_OperationalRight_Metadata_TransitAndConveyanceLossAllowedType.NA, "Compact Data");
					break;
				case 19:
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source1Array_19 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.STREAM_GAGE};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source2Array_19 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.NA};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] destinationArray_19 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.DIVERSION};
					StateMod_OperationalRight_Metadata_DestinationLocationType[] destinationLocationArray_19 = new StateMod_OperationalRight_Metadata_DestinationLocationType[] {StateMod_OperationalRight_Metadata_DestinationLocationType.DOWNSTREAM};
					StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] associatedPlanAllowedArray_19 = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] {StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.NA};
					StateMod_OperationalRight_Metadata_DiversionType[] diversionTypeArray_19 = new StateMod_OperationalRight_Metadata_DiversionType[] {StateMod_OperationalRight_Metadata_DiversionType.DIVERSION};
					metaData = new StateMod_OperationalRight_Metadata(i, false, "Split Channel Operation", StateMod_OperationalRight_Metadata_RuleType.OTHER, source1Array_19, source2Array_19, StateMod_OperationalRight_Metadata_Source2AllowedType.NA, destinationArray_19, destinationLocationArray_19, StateMod_OperationalRight_Metadata_DeliveryMethodType.RIVER, StateMod_OperationalRight_Metadata_CarrierAllowedType.NA, false, false, associatedPlanAllowedArray_19, diversionTypeArray_19, StateMod_OperationalRight_Metadata_TransitAndConveyanceLossAllowedType.NA, "");
					break;
				case 20:
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source1Array_20 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.RESERVOIR};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source2Array_20 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.NA};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] destinationArray_20 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.NA};
					StateMod_OperationalRight_Metadata_DestinationLocationType[] destinationLocationArray_20 = new StateMod_OperationalRight_Metadata_DestinationLocationType[] {StateMod_OperationalRight_Metadata_DestinationLocationType.SOURCE};
					StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] associatedPlanAllowedArray_20 = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] {StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.NA};
					StateMod_OperationalRight_Metadata_DiversionType[] diversionTypeArray_20 = new StateMod_OperationalRight_Metadata_DiversionType[] {StateMod_OperationalRight_Metadata_DiversionType.NA};
					metaData = new StateMod_OperationalRight_Metadata(i, true, "San Juan Reservoir RIP Operation", StateMod_OperationalRight_Metadata_RuleType.COMPACT, source1Array_20, source2Array_20, StateMod_OperationalRight_Metadata_Source2AllowedType.NA, destinationArray_20, destinationLocationArray_20, StateMod_OperationalRight_Metadata_DeliveryMethodType.RIVER, StateMod_OperationalRight_Metadata_CarrierAllowedType.NA, false, false, associatedPlanAllowedArray_20, diversionTypeArray_20, StateMod_OperationalRight_Metadata_TransitAndConveyanceLossAllowedType.NA, "");
					break;
				case 21:
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source1Array_21 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.NA};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source2Array_21 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.NA};
					// TODO SAM 2011-02-05 Summary had Well
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] destinationArray_21 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.NA};
					StateMod_OperationalRight_Metadata_DestinationLocationType[] destinationLocationArray_21 = new StateMod_OperationalRight_Metadata_DestinationLocationType[] {StateMod_OperationalRight_Metadata_DestinationLocationType.SOURCE};
					StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] associatedPlanAllowedArray_21 = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] {StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.NA};
					StateMod_OperationalRight_Metadata_DiversionType[] diversionTypeArray_21 = new StateMod_OperationalRight_Metadata_DiversionType[] {StateMod_OperationalRight_Metadata_DiversionType.NA};
					metaData = new StateMod_OperationalRight_Metadata(i, true, "Wells with Sprinkler Use", StateMod_OperationalRight_Metadata_RuleType.OTHER, source1Array_21, source2Array_21, StateMod_OperationalRight_Metadata_Source2AllowedType.NA, destinationArray_21, destinationLocationArray_21, StateMod_OperationalRight_Metadata_DeliveryMethodType.RIVER, StateMod_OperationalRight_Metadata_CarrierAllowedType.NA, false, false, associatedPlanAllowedArray_21, diversionTypeArray_21, StateMod_OperationalRight_Metadata_TransitAndConveyanceLossAllowedType.NA, "");
					break;
				case 22:
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source1Array_22 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.NA};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source2Array_22 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.NA};
					// TODO SAM 2011-02-05 Summary has Diversion and Well
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] destinationArray_22 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.NA};
					StateMod_OperationalRight_Metadata_DestinationLocationType[] destinationLocationArray_22 = new StateMod_OperationalRight_Metadata_DestinationLocationType[] {StateMod_OperationalRight_Metadata_DestinationLocationType.SOURCE};
					StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] associatedPlanAllowedArray_22 = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] {StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.NA};
					StateMod_OperationalRight_Metadata_DiversionType[] diversionTypeArray_22 = new StateMod_OperationalRight_Metadata_DiversionType[] {StateMod_OperationalRight_Metadata_DiversionType.NA};
					metaData = new StateMod_OperationalRight_Metadata(i, true, "Soil Moisture Use", StateMod_OperationalRight_Metadata_RuleType.SOIL_MOISTURE, source1Array_22, source2Array_22, StateMod_OperationalRight_Metadata_Source2AllowedType.NA, destinationArray_22, destinationLocationArray_22, StateMod_OperationalRight_Metadata_DeliveryMethodType.RIVER, StateMod_OperationalRight_Metadata_CarrierAllowedType.NA, false, false, associatedPlanAllowedArray_22, diversionTypeArray_22, StateMod_OperationalRight_Metadata_TransitAndConveyanceLossAllowedType.NA, "");
					break;
				case 23:
					// TODO SAM 2011-02-05 Summary had Stream
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source1Array_23 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.NA};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source2Array_23 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.NA};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] destinationArray_23 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.NA};
					StateMod_OperationalRight_Metadata_DestinationLocationType[] destinationLocationArray_23 = new StateMod_OperationalRight_Metadata_DestinationLocationType[] {StateMod_OperationalRight_Metadata_DestinationLocationType.DOWNSTREAM};
					StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] associatedPlanAllowedArray_23 = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] {StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.NA};
					StateMod_OperationalRight_Metadata_DiversionType[] diversionTypeArray_23 = new StateMod_OperationalRight_Metadata_DiversionType[] {StateMod_OperationalRight_Metadata_DiversionType.NA};
					metaData = new StateMod_OperationalRight_Metadata(i, true, "Downstream Call", StateMod_OperationalRight_Metadata_RuleType.OTHER, source1Array_23, source2Array_23, StateMod_OperationalRight_Metadata_Source2AllowedType.NA, destinationArray_23, destinationLocationArray_23, StateMod_OperationalRight_Metadata_DeliveryMethodType.RIVER, StateMod_OperationalRight_Metadata_CarrierAllowedType.NA, false, false, associatedPlanAllowedArray_23, diversionTypeArray_23, StateMod_OperationalRight_Metadata_TransitAndConveyanceLossAllowedType.NA, "");
					break;
				case 24:
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source1Array_24 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.DIVERSION_RIGHT};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source2Array_24 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.NA, StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_TC};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] destinationArray_24 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.DIVERSION, StateMod_OperationalRight_Metadata_SourceOrDestinationType.RESERVOIR, StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_ACCOUNTING, StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_TC};
					StateMod_OperationalRight_Metadata_DestinationLocationType[] destinationLocationArray_24 = new StateMod_OperationalRight_Metadata_DestinationLocationType[] {StateMod_OperationalRight_Metadata_DestinationLocationType.UPSTREAM};
					StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] associatedPlanAllowedArray_24 = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] {StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.NA, StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.PLAN_REUSE_TO_DIVERSION, StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.PLAN_REUSE_TO_DIVERSION_FROM_TRANSMOUNTAIN, StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.PLAN_REUSE_TO_RESERVOIR, StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.PLAN_REUSE_TO_RESERVOIR_FROM_TRANSMOUNTAIN};
					StateMod_OperationalRight_Metadata_DiversionType[] diversionTypeArray_24 = new StateMod_OperationalRight_Metadata_DiversionType[] {StateMod_OperationalRight_Metadata_DiversionType.DEPLETION, StateMod_OperationalRight_Metadata_DiversionType.DIVERSION};
					metaData = new StateMod_OperationalRight_Metadata(i, false, "Direct Flow Exchange of a Pro-rata Water Right", StateMod_OperationalRight_Metadata_RuleType.SOURCE_DIRECT_FLOW_RIGHT, source1Array_24, source2Array_24, StateMod_OperationalRight_Metadata_Source2AllowedType.ALLOWED, destinationArray_24, destinationLocationArray_24, StateMod_OperationalRight_Metadata_DeliveryMethodType.RIVER, StateMod_OperationalRight_Metadata_CarrierAllowedType.YES, true, false, associatedPlanAllowedArray_24, diversionTypeArray_24, StateMod_OperationalRight_Metadata_TransitAndConveyanceLossAllowedType.YES, "Exchange Limit");
					break;
				case 25:
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source1Array_25 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.DIVERSION_RIGHT};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source2Array_25 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_TC};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] destinationArray_25 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.CARRIER, StateMod_OperationalRight_Metadata_SourceOrDestinationType.DIVERSION, StateMod_OperationalRight_Metadata_SourceOrDestinationType.RESERVOIR};
					StateMod_OperationalRight_Metadata_DestinationLocationType[] destinationLocationArray_25 = new StateMod_OperationalRight_Metadata_DestinationLocationType[] {StateMod_OperationalRight_Metadata_DestinationLocationType.DOWNSTREAM};
					StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] associatedPlanAllowedArray_25 = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] {StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.PLAN_TC};
					StateMod_OperationalRight_Metadata_DiversionType[] diversionTypeArray_25 = new StateMod_OperationalRight_Metadata_DiversionType[] {StateMod_OperationalRight_Metadata_DiversionType.DEPLETION, StateMod_OperationalRight_Metadata_DiversionType.DIVERSION};
					metaData = new StateMod_OperationalRight_Metadata(i, false, "Direct Flow Bypass of a Pro-rata Water Right", StateMod_OperationalRight_Metadata_RuleType.SOURCE_DIRECT_FLOW_RIGHT, source1Array_25, source2Array_25, StateMod_OperationalRight_Metadata_Source2AllowedType.ALLOWED, destinationArray_25, destinationLocationArray_25, StateMod_OperationalRight_Metadata_DeliveryMethodType.RIVER, StateMod_OperationalRight_Metadata_CarrierAllowedType.YES, true, false, associatedPlanAllowedArray_25, diversionTypeArray_25, StateMod_OperationalRight_Metadata_TransitAndConveyanceLossAllowedType.YES, "Exchange Limit");
					break;
				case 26:
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source1Array_26 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.NA};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source2Array_26 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.NA};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] destinationArray_26 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.NA};
					StateMod_OperationalRight_Metadata_DestinationLocationType[] destinationLocationArray_26 = new StateMod_OperationalRight_Metadata_DestinationLocationType[] {StateMod_OperationalRight_Metadata_DestinationLocationType.NA};
					StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] associatedPlanAllowedArray_26 = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] {StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.NA};
					StateMod_OperationalRight_Metadata_DiversionType[] diversionTypeArray_26 = new StateMod_OperationalRight_Metadata_DiversionType[] {StateMod_OperationalRight_Metadata_DiversionType.NA};
					metaData = new StateMod_OperationalRight_Metadata(i, false, "Not currently used - see type 48, 49", StateMod_OperationalRight_Metadata_RuleType.NA, source1Array_26, source2Array_26, StateMod_OperationalRight_Metadata_Source2AllowedType.NA, destinationArray_26, destinationLocationArray_26, StateMod_OperationalRight_Metadata_DeliveryMethodType.NA, StateMod_OperationalRight_Metadata_CarrierAllowedType.NA, false, false, associatedPlanAllowedArray_26, diversionTypeArray_26, StateMod_OperationalRight_Metadata_TransitAndConveyanceLossAllowedType.NA, "");
					break;
				case 27:
					// TODO SAM 2010-12-13 Need to check the following
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source1Array_27 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_ACCOUNTING, StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_REUSE_TO_DIVERSION, StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_REUSE_TO_DIVERSION_FROM_TRANSMOUNTAIN, StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_REUSE_TO_RESERVOIR, StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_REUSE_TO_RESERVOIR_FROM_TRANSMOUNTAIN};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source2Array_27 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_TC};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] destinationArray_27 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.CARRIER, StateMod_OperationalRight_Metadata_SourceOrDestinationType.DIVERSION, StateMod_OperationalRight_Metadata_SourceOrDestinationType.RESERVOIR};
					StateMod_OperationalRight_Metadata_DestinationLocationType[] destinationLocationArray_27 = new StateMod_OperationalRight_Metadata_DestinationLocationType[] {StateMod_OperationalRight_Metadata_DestinationLocationType.DOWNSTREAM};
					StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] associatedPlanAllowedArray_27 = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] {StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.PLAN_REUSE_TO_DIVERSION, StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.PLAN_REUSE_TO_DIVERSION_FROM_TRANSMOUNTAIN, StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.PLAN_REUSE_TO_RESERVOIR, StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.PLAN_REUSE_TO_RESERVOIR_FROM_TRANSMOUNTAIN};
					StateMod_OperationalRight_Metadata_DiversionType[] diversionTypeArray_27 = new StateMod_OperationalRight_Metadata_DiversionType[] {StateMod_OperationalRight_Metadata_DiversionType.DEPLETION, StateMod_OperationalRight_Metadata_DiversionType.DIVERSION};
					metaData = new StateMod_OperationalRight_Metadata(i, false, "Reservoir, Reuse, or Accounting Plan to a Diversion, Reservoir, or Carrier, with Reuse", StateMod_OperationalRight_Metadata_RuleType.SOURCE_PLAN_STRUCTURE, source1Array_27, source2Array_27, StateMod_OperationalRight_Metadata_Source2AllowedType.ALLOWED, destinationArray_27, destinationLocationArray_27, StateMod_OperationalRight_Metadata_DeliveryMethodType.RIVER, StateMod_OperationalRight_Metadata_CarrierAllowedType.YES, true, false, associatedPlanAllowedArray_27, diversionTypeArray_27, StateMod_OperationalRight_Metadata_TransitAndConveyanceLossAllowedType.YES, "");
					break;
				case 28:
					// TODO SAM 2010-12-13 Need to check the following
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source1Array_28 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_ACCOUNTING, StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_REUSE_TO_DIVERSION, StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_REUSE_TO_DIVERSION_FROM_TRANSMOUNTAIN, StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_REUSE_TO_RESERVOIR, StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_REUSE_TO_RESERVOIR_FROM_TRANSMOUNTAIN};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source2Array_28 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_TC};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] destinationArray_28 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.CARRIER, StateMod_OperationalRight_Metadata_SourceOrDestinationType.DIVERSION, StateMod_OperationalRight_Metadata_SourceOrDestinationType.RESERVOIR};
					StateMod_OperationalRight_Metadata_DestinationLocationType[] destinationLocationArray_28 = new StateMod_OperationalRight_Metadata_DestinationLocationType[] {StateMod_OperationalRight_Metadata_DestinationLocationType.UPSTREAM};
					StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] associatedPlanAllowedArray_28 = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] {StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.PLAN_REUSE_TO_DIVERSION, StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.PLAN_REUSE_TO_DIVERSION_FROM_TRANSMOUNTAIN, StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.PLAN_REUSE_TO_RESERVOIR, StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.PLAN_REUSE_TO_RESERVOIR_FROM_TRANSMOUNTAIN};
					StateMod_OperationalRight_Metadata_DiversionType[] diversionTypeArray_28 = new StateMod_OperationalRight_Metadata_DiversionType[] {StateMod_OperationalRight_Metadata_DiversionType.DEPLETION, StateMod_OperationalRight_Metadata_DiversionType.DIVERSION};
					metaData = new StateMod_OperationalRight_Metadata(i, false, "Reuse Plan to a Diversion or Reservoir by Exchange with or without Destination Reuse", StateMod_OperationalRight_Metadata_RuleType.SOURCE_PLAN_STRUCTURE, source1Array_28, source2Array_28, StateMod_OperationalRight_Metadata_Source2AllowedType.ALLOWED, destinationArray_28, destinationLocationArray_28, StateMod_OperationalRight_Metadata_DeliveryMethodType.RIVER, StateMod_OperationalRight_Metadata_CarrierAllowedType.YES, true, false, associatedPlanAllowedArray_28, diversionTypeArray_28, StateMod_OperationalRight_Metadata_TransitAndConveyanceLossAllowedType.YES, "");
					break;
				case 29:
					// TODO SAM 2010-12-13 Need to check the following
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source1Array_29 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.RESERVOIR, StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_REUSE_TO_DIVERSION, StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_REUSE_TO_DIVERSION_FROM_TRANSMOUNTAIN, StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_REUSE_TO_RESERVOIR, StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_REUSE_TO_RESERVOIR_FROM_TRANSMOUNTAIN};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source2Array_29 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_TC};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] destinationArray_29 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.NA};
					StateMod_OperationalRight_Metadata_DestinationLocationType[] destinationLocationArray_29 = new StateMod_OperationalRight_Metadata_DestinationLocationType[] {StateMod_OperationalRight_Metadata_DestinationLocationType.UPSTREAM};
					StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] associatedPlanAllowedArray_29 = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] {StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.NA};
					StateMod_OperationalRight_Metadata_DiversionType[] diversionTypeArray_29 = new StateMod_OperationalRight_Metadata_DiversionType[] {StateMod_OperationalRight_Metadata_DiversionType.NA};
					metaData = new StateMod_OperationalRight_Metadata(i, false, "Reuse or Accounting Plan Spill", StateMod_OperationalRight_Metadata_RuleType.SOURCE_PLAN_STRUCTURE, source1Array_29, source2Array_29, StateMod_OperationalRight_Metadata_Source2AllowedType.ALLOWED, destinationArray_29, destinationLocationArray_29, StateMod_OperationalRight_Metadata_DeliveryMethodType.RIVER, StateMod_OperationalRight_Metadata_CarrierAllowedType.NA, false, false, associatedPlanAllowedArray_29, diversionTypeArray_29, StateMod_OperationalRight_Metadata_TransitAndConveyanceLossAllowedType.NA, "");
					break;
				case 30:
					// TODO SAM 2010-12-13 Need to check the following
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source1Array_30 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.OPERATIONAL_RIGHT};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source2Array_30 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.NA};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] destinationArray_30 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.RESERVOIR};
					StateMod_OperationalRight_Metadata_DestinationLocationType[] destinationLocationArray_30 = new StateMod_OperationalRight_Metadata_DestinationLocationType[] {StateMod_OperationalRight_Metadata_DestinationLocationType.SOURCE};
					StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] associatedPlanAllowedArray_30 = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] {StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.NA};
					StateMod_OperationalRight_Metadata_DiversionType[] diversionTypeArray_30 = new StateMod_OperationalRight_Metadata_DiversionType[] {StateMod_OperationalRight_Metadata_DiversionType.NA};
					metaData = new StateMod_OperationalRight_Metadata(i, true, "Rservoir Re-diversion", StateMod_OperationalRight_Metadata_RuleType.OTHER, source1Array_30, source2Array_30, StateMod_OperationalRight_Metadata_Source2AllowedType.ALLOWED, destinationArray_30, destinationLocationArray_30, StateMod_OperationalRight_Metadata_DeliveryMethodType.RIVER, StateMod_OperationalRight_Metadata_CarrierAllowedType.NA, true, false, associatedPlanAllowedArray_30, diversionTypeArray_30, StateMod_OperationalRight_Metadata_TransitAndConveyanceLossAllowedType.NA, "");
					break;
				case 31:
					// TODO SAM 2010-12-13 Need to check the following
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source1Array_31 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.DIVERSION_RIGHT};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source2Array_31 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.NA};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] destinationArray_31 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.DIVERSION, StateMod_OperationalRight_Metadata_SourceOrDestinationType.RESERVOIR};
					StateMod_OperationalRight_Metadata_DestinationLocationType[] destinationLocationArray_31 = new StateMod_OperationalRight_Metadata_DestinationLocationType[] {StateMod_OperationalRight_Metadata_DestinationLocationType.DOWNSTREAM};
					StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] associatedPlanAllowedArray_31 = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] {StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.NA, StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.PLAN_REUSE_TO_DIVERSION, StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.PLAN_REUSE_TO_DIVERSION_FROM_TRANSMOUNTAIN, StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.PLAN_REUSE_TO_RESERVOIR, StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.PLAN_REUSE_TO_RESERVOIR_FROM_TRANSMOUNTAIN};
					StateMod_OperationalRight_Metadata_DiversionType[] diversionTypeArray_31 = new StateMod_OperationalRight_Metadata_DiversionType[] {StateMod_OperationalRight_Metadata_DiversionType.NA};
					metaData = new StateMod_OperationalRight_Metadata(i, true, "Carrier Right to a Ditch or Reservoir with Reuseable Return Flows", StateMod_OperationalRight_Metadata_RuleType.SOURCE_DIRECT_FLOW_RIGHT, source1Array_31, source2Array_31, StateMod_OperationalRight_Metadata_Source2AllowedType.ALLOWED, destinationArray_31, destinationLocationArray_31, StateMod_OperationalRight_Metadata_DeliveryMethodType.CARRIER, StateMod_OperationalRight_Metadata_CarrierAllowedType.YES, true, false, associatedPlanAllowedArray_31, diversionTypeArray_31, StateMod_OperationalRight_Metadata_TransitAndConveyanceLossAllowedType.YES, "");
					break;
				case 32:
					// TODO SAM 2010-12-13 Need to check the following
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source1Array_32 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.RESERVOIR};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source2Array_32 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_REUSE_TO_DIVERSION, StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_REUSE_TO_DIVERSION_FROM_TRANSMOUNTAIN, StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_REUSE_TO_RESERVOIR, StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_REUSE_TO_RESERVOIR_FROM_TRANSMOUNTAIN};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] destinationArray_32 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.CARRIER, StateMod_OperationalRight_Metadata_SourceOrDestinationType.DIVERSION, StateMod_OperationalRight_Metadata_SourceOrDestinationType.RESERVOIR};
					StateMod_OperationalRight_Metadata_DestinationLocationType[] destinationLocationArray_32 = new StateMod_OperationalRight_Metadata_DestinationLocationType[] {StateMod_OperationalRight_Metadata_DestinationLocationType.DOWNSTREAM};
					// TODO SAM 2010-12-13 Should reservoir account be added?
					StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] associatedPlanAllowedArray_32 = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] {StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.NA, StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.PLAN_REUSE_TO_DIVERSION, StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.PLAN_REUSE_TO_DIVERSION_FROM_TRANSMOUNTAIN, StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.PLAN_REUSE_TO_RESERVOIR, StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.PLAN_REUSE_TO_RESERVOIR_FROM_TRANSMOUNTAIN};
					StateMod_OperationalRight_Metadata_DiversionType[] diversionTypeArray_32 = new StateMod_OperationalRight_Metadata_DiversionType[] {StateMod_OperationalRight_Metadata_DiversionType.DEPLETION, StateMod_OperationalRight_Metadata_DiversionType.DIRECT, StateMod_OperationalRight_Metadata_DiversionType.DIVERSION};
					metaData = new StateMod_OperationalRight_Metadata(i, true, "Reservoir and Plan to a Direct Flow or Reservoir or Carrier Direct with or without Destination Reuse", StateMod_OperationalRight_Metadata_RuleType.SOURCE_PLAN_STRUCTURE, source1Array_32, source2Array_32, StateMod_OperationalRight_Metadata_Source2AllowedType.ALLOWED, destinationArray_32, destinationLocationArray_32, StateMod_OperationalRight_Metadata_DeliveryMethodType.RIVER, StateMod_OperationalRight_Metadata_CarrierAllowedType.YES, true, false, associatedPlanAllowedArray_32, diversionTypeArray_32, StateMod_OperationalRight_Metadata_TransitAndConveyanceLossAllowedType.YES, "Same as type 11 but allows reuse and loss.");
					break;
				case 33:
					// TODO SAM 2010-12-13 Need to check the following
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source1Array_33 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.RESERVOIR};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source2Array_33 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_REUSE_TO_DIVERSION, StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_REUSE_TO_DIVERSION_FROM_TRANSMOUNTAIN, StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_REUSE_TO_RESERVOIR, StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_REUSE_TO_RESERVOIR_FROM_TRANSMOUNTAIN};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] destinationArray_33 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.CARRIER, StateMod_OperationalRight_Metadata_SourceOrDestinationType.DIVERSION, StateMod_OperationalRight_Metadata_SourceOrDestinationType.RESERVOIR};
					StateMod_OperationalRight_Metadata_DestinationLocationType[] destinationLocationArray_33 = new StateMod_OperationalRight_Metadata_DestinationLocationType[] {StateMod_OperationalRight_Metadata_DestinationLocationType.UPSTREAM};
					// TODO SAM 2010-12-13 Should reservoir account be added?
					StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] associatedPlanAllowedArray_33 = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] {StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.NA, StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.PLAN_REUSE_TO_DIVERSION, StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.PLAN_REUSE_TO_DIVERSION_FROM_TRANSMOUNTAIN, StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.PLAN_REUSE_TO_RESERVOIR, StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.PLAN_REUSE_TO_RESERVOIR_FROM_TRANSMOUNTAIN};
					StateMod_OperationalRight_Metadata_DiversionType[] diversionTypeArray_33 = new StateMod_OperationalRight_Metadata_DiversionType[] {StateMod_OperationalRight_Metadata_DiversionType.DEPLETION, StateMod_OperationalRight_Metadata_DiversionType.DIVERSION};
					metaData = new StateMod_OperationalRight_Metadata(i, true, "Reservoir and Plan to a Direct Flow or Reservoir or Carrier by Exchange with or without Destination Reuse", StateMod_OperationalRight_Metadata_RuleType.SOURCE_RESERVOIR, source1Array_33, source2Array_33, StateMod_OperationalRight_Metadata_Source2AllowedType.ALLOWED, destinationArray_33, destinationLocationArray_33, StateMod_OperationalRight_Metadata_DeliveryMethodType.RIVER, StateMod_OperationalRight_Metadata_CarrierAllowedType.YES, true, false, associatedPlanAllowedArray_33, diversionTypeArray_33, StateMod_OperationalRight_Metadata_TransitAndConveyanceLossAllowedType.YES, "");
					break;
				case 34:
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source1Array_34 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.RESERVOIR};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source2Array_34 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.DIVERSION, StateMod_OperationalRight_Metadata_SourceOrDestinationType.OPERATIONAL_RIGHT, StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_OUT_OF_PRIORITY};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] destinationArray_34 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.RESERVOIR};
					StateMod_OperationalRight_Metadata_DestinationLocationType[] destinationLocationArray_34 = new StateMod_OperationalRight_Metadata_DestinationLocationType[] {StateMod_OperationalRight_Metadata_DestinationLocationType.SOURCE};
					// TODO SAM 2010-12-13 Should reservoir account be added?
					StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] associatedPlanAllowedArray_34 = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] {StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.NA, StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.PLAN_OUT_OF_PRIORITY, StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.PLAN_REUSE_TO_DIVERSION, StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.PLAN_REUSE_TO_DIVERSION_FROM_TRANSMOUNTAIN, StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.PLAN_REUSE_TO_RESERVOIR, StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.PLAN_REUSE_TO_RESERVOIR_FROM_TRANSMOUNTAIN};
					StateMod_OperationalRight_Metadata_DiversionType[] diversionTypeArray_34 = new StateMod_OperationalRight_Metadata_DiversionType[] {StateMod_OperationalRight_Metadata_DiversionType.NA};
					metaData = new StateMod_OperationalRight_Metadata(i, true, "Reservoir to Reservoir Transfer (Bookover) with Reuse", StateMod_OperationalRight_Metadata_RuleType.SOURCE_RESERVOIR, source1Array_34, source2Array_34, StateMod_OperationalRight_Metadata_Source2AllowedType.ALLOWED, destinationArray_34, destinationLocationArray_34, StateMod_OperationalRight_Metadata_DeliveryMethodType.BOOKOVER, StateMod_OperationalRight_Metadata_CarrierAllowedType.YES, true, false, associatedPlanAllowedArray_34, diversionTypeArray_34, StateMod_OperationalRight_Metadata_TransitAndConveyanceLossAllowedType.NA, "");
					break;
				case 35:
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source1Array_35 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.DIVERSION};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source2Array_35 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.NA};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] destinationArray_35 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.CARRIER, StateMod_OperationalRight_Metadata_SourceOrDestinationType.DIVERSION, StateMod_OperationalRight_Metadata_SourceOrDestinationType.RESERVOIR};
					StateMod_OperationalRight_Metadata_DestinationLocationType[] destinationLocationArray_35 = new StateMod_OperationalRight_Metadata_DestinationLocationType[] {StateMod_OperationalRight_Metadata_DestinationLocationType.DOWNSTREAM};
					StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] associatedPlanAllowedArray_35 = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] {StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.NA, StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.PLAN_REUSE_TO_DIVERSION, StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.PLAN_REUSE_TO_DIVERSION_FROM_TRANSMOUNTAIN, StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.PLAN_REUSE_TO_RESERVOIR, StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.PLAN_REUSE_TO_RESERVOIR_FROM_TRANSMOUNTAIN};
					StateMod_OperationalRight_Metadata_DiversionType[] diversionTypeArray_35 = new StateMod_OperationalRight_Metadata_DiversionType[] {StateMod_OperationalRight_Metadata_DiversionType.DIVERSION};
					metaData = new StateMod_OperationalRight_Metadata(i, true, "Import to a Diversion, Reservoir, or Carrier with or without Reuse", StateMod_OperationalRight_Metadata_RuleType.SOURCE_PLAN_STRUCTURE, source1Array_35, source2Array_35, StateMod_OperationalRight_Metadata_Source2AllowedType.ALLOWED, destinationArray_35, destinationLocationArray_35, StateMod_OperationalRight_Metadata_DeliveryMethodType.RIVER, StateMod_OperationalRight_Metadata_CarrierAllowedType.YES, true, false, associatedPlanAllowedArray_35, diversionTypeArray_35, StateMod_OperationalRight_Metadata_TransitAndConveyanceLossAllowedType.NA, "");
					break;
				case 36:
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source1Array_36 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.DIVERSION_RIGHT};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source2Array_36 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.NA};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] destinationArray_36 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.DIVERSION};
					StateMod_OperationalRight_Metadata_DestinationLocationType[] destinationLocationArray_36 = new StateMod_OperationalRight_Metadata_DestinationLocationType[] {StateMod_OperationalRight_Metadata_DestinationLocationType.SOURCE};
					StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] associatedPlanAllowedArray_36 = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] {StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.NA};
					StateMod_OperationalRight_Metadata_DiversionType[] diversionTypeArray_36 = new StateMod_OperationalRight_Metadata_DiversionType[] {StateMod_OperationalRight_Metadata_DiversionType.DIVERSION};
					metaData = new StateMod_OperationalRight_Metadata(i, true, "Seasonal (daily) On/Off Capability (e.g., Meadow Rights)", StateMod_OperationalRight_Metadata_RuleType.SOURCE_DIRECT_FLOW_RIGHT, source1Array_36, source2Array_36, StateMod_OperationalRight_Metadata_Source2AllowedType.ALLOWED, destinationArray_36, destinationLocationArray_36, StateMod_OperationalRight_Metadata_DeliveryMethodType.RIVER, StateMod_OperationalRight_Metadata_CarrierAllowedType.YES, true, false, associatedPlanAllowedArray_36, diversionTypeArray_36, StateMod_OperationalRight_Metadata_TransitAndConveyanceLossAllowedType.NA, "Limits a diversion to part of the year (season)");
					break;
				case 37:
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source1Array_37 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.WELL_RIGHT};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source2Array_37 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_WELL_AUGMENTATION};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] destinationArray_37 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_TC, StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_WELL_AUGMENTATION};
					StateMod_OperationalRight_Metadata_DestinationLocationType[] destinationLocationArray_37 = new StateMod_OperationalRight_Metadata_DestinationLocationType[] {StateMod_OperationalRight_Metadata_DestinationLocationType.DOWNSTREAM};
					StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] associatedPlanAllowedArray_37 = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] {StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.NA};
					StateMod_OperationalRight_Metadata_DiversionType[] diversionTypeArray_37 = new StateMod_OperationalRight_Metadata_DiversionType[] {StateMod_OperationalRight_Metadata_DiversionType.NA};
					metaData = new StateMod_OperationalRight_Metadata(i, true, "Augmentation Well", StateMod_OperationalRight_Metadata_RuleType.SOURCE_GROUNDWATER, source1Array_37, source2Array_37, StateMod_OperationalRight_Metadata_Source2AllowedType.ALLOWED, destinationArray_37, destinationLocationArray_37, StateMod_OperationalRight_Metadata_DeliveryMethodType.RIVER, StateMod_OperationalRight_Metadata_CarrierAllowedType.YES, true, false, associatedPlanAllowedArray_37, diversionTypeArray_37, StateMod_OperationalRight_Metadata_TransitAndConveyanceLossAllowedType.NA, "");
					break;
				case 38:
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source1Array_38 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.RESERVOIR_RIGHT};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source2Array_38 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.DIVERSION_RIGHT, StateMod_OperationalRight_Metadata_SourceOrDestinationType.RESERVOIR_RIGHT};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] destinationArray_38 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.DIVERSION, StateMod_OperationalRight_Metadata_SourceOrDestinationType.RESERVOIR};
					StateMod_OperationalRight_Metadata_DestinationLocationType[] destinationLocationArray_38 = new StateMod_OperationalRight_Metadata_DestinationLocationType[] {StateMod_OperationalRight_Metadata_DestinationLocationType.SOURCE};
					StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] associatedPlanAllowedArray_38 = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] {StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.PLAN_OUT_OF_PRIORITY};
						// TODO SAM 2011-02-06 Doc says "reuse plan" in one place and OOP plan in another
						//StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.PLAN_REUSE_TO_DIVERSION,
						//StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.PLAN_REUSE_TO_DIVERSION_FROM_TRANSMOUNTAIN,
						//StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.PLAN_REUSE_TO_RESERVOIR,
						//StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.PLAN_REUSE_TO_RESERVOIR_FROM_TRANSMOUNTAIN};
					StateMod_OperationalRight_Metadata_DiversionType[] diversionTypeArray_38 = new StateMod_OperationalRight_Metadata_DiversionType[] {StateMod_OperationalRight_Metadata_DiversionType.NA};
					metaData = new StateMod_OperationalRight_Metadata(i, true, "Out of Priority Diversion (addresses the upstream storage statute)", StateMod_OperationalRight_Metadata_RuleType.STORAGE_OPERATIONS, source1Array_38, source2Array_38, StateMod_OperationalRight_Metadata_Source2AllowedType.REQUIRED, destinationArray_38, destinationLocationArray_38, StateMod_OperationalRight_Metadata_DeliveryMethodType.RIVER, StateMod_OperationalRight_Metadata_CarrierAllowedType.YES, true, false, associatedPlanAllowedArray_38, diversionTypeArray_38, StateMod_OperationalRight_Metadata_TransitAndConveyanceLossAllowedType.NA, "");
					break;
				case 39:
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source1Array_39 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.DIVERSION_RIGHT};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source2Array_39 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.DIVERSION, StateMod_OperationalRight_Metadata_SourceOrDestinationType.WELL};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] destinationArray_39 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.DIVERSION};
					StateMod_OperationalRight_Metadata_DestinationLocationType[] destinationLocationArray_39 = new StateMod_OperationalRight_Metadata_DestinationLocationType[] {StateMod_OperationalRight_Metadata_DestinationLocationType.DOWNSTREAM, StateMod_OperationalRight_Metadata_DestinationLocationType.UPSTREAM};
					StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] associatedPlanAllowedArray_39 = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] {StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.NA};
					StateMod_OperationalRight_Metadata_DiversionType[] diversionTypeArray_39 = new StateMod_OperationalRight_Metadata_DiversionType[] {StateMod_OperationalRight_Metadata_DiversionType.DIVERSION};
					metaData = new StateMod_OperationalRight_Metadata(i, true, "Alternate Point Diversion", StateMod_OperationalRight_Metadata_RuleType.SOURCE_DIRECT_FLOW_RIGHT, source1Array_39, source2Array_39, StateMod_OperationalRight_Metadata_Source2AllowedType.REQUIRED, destinationArray_39, destinationLocationArray_39, StateMod_OperationalRight_Metadata_DeliveryMethodType.RIVER, StateMod_OperationalRight_Metadata_CarrierAllowedType.YES, true, false, associatedPlanAllowedArray_39, diversionTypeArray_39, StateMod_OperationalRight_Metadata_TransitAndConveyanceLossAllowedType.NA, "");
					break;
				case 40:
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source1Array_40 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.STREAM_GAGE};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source2Array_40 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.STREAM_GAGE};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] destinationArray_40 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.INSTREAM_FLOW};
					StateMod_OperationalRight_Metadata_DestinationLocationType[] destinationLocationArray_40 = new StateMod_OperationalRight_Metadata_DestinationLocationType[] {StateMod_OperationalRight_Metadata_DestinationLocationType.DOWNSTREAM};
					StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] associatedPlanAllowedArray_40 = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] {StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.NA};
					StateMod_OperationalRight_Metadata_DiversionType[] diversionTypeArray_40 = new StateMod_OperationalRight_Metadata_DiversionType[] {StateMod_OperationalRight_Metadata_DiversionType.DIVERSION};
					metaData = new StateMod_OperationalRight_Metadata(i, false, "South Platte Compact", StateMod_OperationalRight_Metadata_RuleType.COMPACT, source1Array_40, source2Array_40, StateMod_OperationalRight_Metadata_Source2AllowedType.REQUIRED, destinationArray_40, destinationLocationArray_40, StateMod_OperationalRight_Metadata_DeliveryMethodType.RIVER, StateMod_OperationalRight_Metadata_CarrierAllowedType.NA, true, false, associatedPlanAllowedArray_40, diversionTypeArray_40, StateMod_OperationalRight_Metadata_TransitAndConveyanceLossAllowedType.NA, "");
					break;
				case 41:
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source1Array_41 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.RESERVOIR_RIGHT};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source2Array_41 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.NA};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] destinationArray_41 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.RESERVOIR};
					StateMod_OperationalRight_Metadata_DestinationLocationType[] destinationLocationArray_41 = new StateMod_OperationalRight_Metadata_DestinationLocationType[] {StateMod_OperationalRight_Metadata_DestinationLocationType.SOURCE};
					StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] associatedPlanAllowedArray_41 = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] {StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.NA};
					StateMod_OperationalRight_Metadata_DiversionType[] diversionTypeArray_41 = new StateMod_OperationalRight_Metadata_DiversionType[] {StateMod_OperationalRight_Metadata_DiversionType.DIVERSION};
					metaData = new StateMod_OperationalRight_Metadata(i, true, "Reservoir Storage with Special Limits", StateMod_OperationalRight_Metadata_RuleType.STORAGE_OPERATIONS, source1Array_41, source2Array_41, StateMod_OperationalRight_Metadata_Source2AllowedType.NA, destinationArray_41, destinationLocationArray_41, StateMod_OperationalRight_Metadata_DeliveryMethodType.RIVER, StateMod_OperationalRight_Metadata_CarrierAllowedType.NA, true, false, associatedPlanAllowedArray_41, diversionTypeArray_41, StateMod_OperationalRight_Metadata_TransitAndConveyanceLossAllowedType.NA, "OOP Plan Limits");
					break;
				case 42:
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source1Array_42 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_OUT_OF_PRIORITY, StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_TC, StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_WELL_AUGMENTATION};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source2Array_42 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.NA};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] destinationArray_42 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.NA};
					StateMod_OperationalRight_Metadata_DestinationLocationType[] destinationLocationArray_42 = new StateMod_OperationalRight_Metadata_DestinationLocationType[] {StateMod_OperationalRight_Metadata_DestinationLocationType.SOURCE};
					StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] associatedPlanAllowedArray_42 = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] {StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.NA};
					StateMod_OperationalRight_Metadata_DiversionType[] diversionTypeArray_42 = new StateMod_OperationalRight_Metadata_DiversionType[] {StateMod_OperationalRight_Metadata_DiversionType.NA};
					metaData = new StateMod_OperationalRight_Metadata(i, true, "Plan Demand Reset", StateMod_OperationalRight_Metadata_RuleType.STORAGE_OPERATIONS, source1Array_42, source2Array_42, StateMod_OperationalRight_Metadata_Source2AllowedType.NA, destinationArray_42, destinationLocationArray_42, StateMod_OperationalRight_Metadata_DeliveryMethodType.RIVER, StateMod_OperationalRight_Metadata_CarrierAllowedType.NA, false, false, associatedPlanAllowedArray_42, diversionTypeArray_42, StateMod_OperationalRight_Metadata_TransitAndConveyanceLossAllowedType.NA, "");
					break;
				case 43:
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source1Array_43 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.NA};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source2Array_43 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.NA};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] destinationArray_43 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_TC, StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_WELL_AUGMENTATION};
					StateMod_OperationalRight_Metadata_DestinationLocationType[] destinationLocationArray_43 = new StateMod_OperationalRight_Metadata_DestinationLocationType[] {StateMod_OperationalRight_Metadata_DestinationLocationType.SOURCE};
					StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] associatedPlanAllowedArray_43 = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] {StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.NA};
					StateMod_OperationalRight_Metadata_DiversionType[] diversionTypeArray_43 = new StateMod_OperationalRight_Metadata_DiversionType[] {StateMod_OperationalRight_Metadata_DiversionType.NA};
					metaData = new StateMod_OperationalRight_Metadata(i, true, "In-Priority Well Depletion", StateMod_OperationalRight_Metadata_RuleType.SOURCE_PLAN_STRUCTURE, source1Array_43, source2Array_43, StateMod_OperationalRight_Metadata_Source2AllowedType.NA, destinationArray_43, destinationLocationArray_43, StateMod_OperationalRight_Metadata_DeliveryMethodType.RIVER, StateMod_OperationalRight_Metadata_CarrierAllowedType.NA, false, false, associatedPlanAllowedArray_43, diversionTypeArray_43, StateMod_OperationalRight_Metadata_TransitAndConveyanceLossAllowedType.NA, "");
					break;
				case 44:
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source1Array_44 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.WELL_RIGHT};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source2Array_44 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.NA};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] destinationArray_44 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.RESERVOIR};
					StateMod_OperationalRight_Metadata_DestinationLocationType[] destinationLocationArray_44 = new StateMod_OperationalRight_Metadata_DestinationLocationType[] {StateMod_OperationalRight_Metadata_DestinationLocationType.SOURCE};
					StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] associatedPlanAllowedArray_44 = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] {StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.NA, StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.PLAN_WELL_AUGMENTATION};
					// TODO SAM 2011-02-05 Summary has Diversion
					StateMod_OperationalRight_Metadata_DiversionType[] diversionTypeArray_44 = new StateMod_OperationalRight_Metadata_DiversionType[] {StateMod_OperationalRight_Metadata_DiversionType.NA};
					metaData = new StateMod_OperationalRight_Metadata(i, true, "Recharge Well", StateMod_OperationalRight_Metadata_RuleType.SOURCE_GROUNDWATER, source1Array_44, source2Array_44, StateMod_OperationalRight_Metadata_Source2AllowedType.NA, destinationArray_44, destinationLocationArray_44, StateMod_OperationalRight_Metadata_DeliveryMethodType.RIVER, StateMod_OperationalRight_Metadata_CarrierAllowedType.YES, true, false, associatedPlanAllowedArray_44, diversionTypeArray_44, StateMod_OperationalRight_Metadata_TransitAndConveyanceLossAllowedType.NA, "");
					break;
				case 45:
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source1Array_45 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.DIVERSION_RIGHT, StateMod_OperationalRight_Metadata_SourceOrDestinationType.RESERVOIR_RIGHT};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source2Array_45 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.NA};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] destinationArray_45 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.DIVERSION, StateMod_OperationalRight_Metadata_SourceOrDestinationType.RESERVOIR};
					StateMod_OperationalRight_Metadata_DestinationLocationType[] destinationLocationArray_45 = new StateMod_OperationalRight_Metadata_DestinationLocationType[] {StateMod_OperationalRight_Metadata_DestinationLocationType.SOURCE};
					StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] associatedPlanAllowedArray_45 = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] {StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.NA, StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.PLAN_RECHARGE};
					StateMod_OperationalRight_Metadata_DiversionType[] diversionTypeArray_45 = new StateMod_OperationalRight_Metadata_DiversionType[] {StateMod_OperationalRight_Metadata_DiversionType.DIVERSION};
					metaData = new StateMod_OperationalRight_Metadata(i, true, "Carrier with Transit Loss (allows multiple carriers and associated losses)", StateMod_OperationalRight_Metadata_RuleType.SOURCE_DIRECT_FLOW_RIGHT, source1Array_45, source2Array_45, StateMod_OperationalRight_Metadata_Source2AllowedType.NA, destinationArray_45, destinationLocationArray_45, StateMod_OperationalRight_Metadata_DeliveryMethodType.RIVER, StateMod_OperationalRight_Metadata_CarrierAllowedType.YES, false, true, associatedPlanAllowedArray_45, diversionTypeArray_45, StateMod_OperationalRight_Metadata_TransitAndConveyanceLossAllowedType.YES, "");
					break;
				case 46:
					// TODO SAM 2011-02-05 Need to enable - too many questions about documentation
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source1Array_46 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_ACCOUNTING};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source2Array_46 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.NA};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] destinationArray_46 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_ACCOUNTING};
					StateMod_OperationalRight_Metadata_DestinationLocationType[] destinationLocationArray_46 = new StateMod_OperationalRight_Metadata_DestinationLocationType[] {StateMod_OperationalRight_Metadata_DestinationLocationType.SOURCE};
					StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] associatedPlanAllowedArray_46 = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] {StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.NA};
					StateMod_OperationalRight_Metadata_DiversionType[] diversionTypeArray_46 = new StateMod_OperationalRight_Metadata_DiversionType[] {StateMod_OperationalRight_Metadata_DiversionType.DIVERSION};
					metaData = new StateMod_OperationalRight_Metadata(i, false, "Multiple Ownership Plans (distributes plan contents to multiple plans)", StateMod_OperationalRight_Metadata_RuleType.SOURCE_PLAN_STRUCTURE, source1Array_46, source2Array_46, StateMod_OperationalRight_Metadata_Source2AllowedType.NA, destinationArray_46, destinationLocationArray_46, StateMod_OperationalRight_Metadata_DeliveryMethodType.RIVER, StateMod_OperationalRight_Metadata_CarrierAllowedType.YES, false, false, associatedPlanAllowedArray_46, diversionTypeArray_46, StateMod_OperationalRight_Metadata_TransitAndConveyanceLossAllowedType.NA, "Multiple Ownership");
					break;
				case 47:
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source1Array_47 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_ACCOUNTING, StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_RELEASE_LIMIT};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source2Array_47 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.NA};
					// TODO SAM 2011-02-05 Summary had diversion, reservoir, or carrier
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] destinationArray_47 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.NA};
					StateMod_OperationalRight_Metadata_DestinationLocationType[] destinationLocationArray_47 = new StateMod_OperationalRight_Metadata_DestinationLocationType[] {StateMod_OperationalRight_Metadata_DestinationLocationType.SOURCE};
					StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] associatedPlanAllowedArray_47 = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] {StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.NA};
					StateMod_OperationalRight_Metadata_DiversionType[] diversionTypeArray_47 = new StateMod_OperationalRight_Metadata_DiversionType[] {StateMod_OperationalRight_Metadata_DiversionType.DIVERSION};
					metaData = new StateMod_OperationalRight_Metadata(i, true, "Administration Plan Limits", StateMod_OperationalRight_Metadata_RuleType.STORAGE_OPERATIONS, source1Array_47, source2Array_47, StateMod_OperationalRight_Metadata_Source2AllowedType.NA, destinationArray_47, destinationLocationArray_47, StateMod_OperationalRight_Metadata_DeliveryMethodType.RIVER, StateMod_OperationalRight_Metadata_CarrierAllowedType.YES, false, false, associatedPlanAllowedArray_47, diversionTypeArray_47, StateMod_OperationalRight_Metadata_TransitAndConveyanceLossAllowedType.NA, "");
					break;
				case 48:
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source1Array_48 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.RESERVOIR, StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_RECHARGE, StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_REUSE_TO_DIVERSION, StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_REUSE_TO_DIVERSION_FROM_TRANSMOUNTAIN, StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_REUSE_TO_RESERVOIR, StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_REUSE_TO_RESERVOIR_FROM_TRANSMOUNTAIN, StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_TRANSMOUNTAIN_IMPORT, StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_SPECIAL_WELL_AUGMENTATION};
					// TODO SAM 2011-02-05 Handled dynamically in code...
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source2Array_48 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.NA};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] destinationArray_48 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_TC, StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_SPECIAL_WELL_AUGMENTATION, StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_WELL_AUGMENTATION};
					StateMod_OperationalRight_Metadata_DestinationLocationType[] destinationLocationArray_48 = new StateMod_OperationalRight_Metadata_DestinationLocationType[] {StateMod_OperationalRight_Metadata_DestinationLocationType.DOWNSTREAM};
					// FIXME SAM 2011-01-31 the following was YES but need list of specific plans
					StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] associatedPlanAllowedArray_48 = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] {StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.NA};
					StateMod_OperationalRight_Metadata_DiversionType[] diversionTypeArray_48 = new StateMod_OperationalRight_Metadata_DiversionType[] {StateMod_OperationalRight_Metadata_DiversionType.DIVERSION};
					metaData = new StateMod_OperationalRight_Metadata(i, true, "Plan or Reservoir Reuse to a Plan Direct", StateMod_OperationalRight_Metadata_RuleType.SOURCE_PLAN_STRUCTURE, source1Array_48, source2Array_48, StateMod_OperationalRight_Metadata_Source2AllowedType.ALLOWED, destinationArray_48, destinationLocationArray_48, StateMod_OperationalRight_Metadata_DeliveryMethodType.RIVER, StateMod_OperationalRight_Metadata_CarrierAllowedType.NA, false, false, associatedPlanAllowedArray_48, diversionTypeArray_48, StateMod_OperationalRight_Metadata_TransitAndConveyanceLossAllowedType.NA, "");
					break;
				case 49:
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source1Array_49 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.RESERVOIR, StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_RECHARGE, StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_REUSE_TO_DIVERSION, StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_REUSE_TO_DIVERSION_FROM_TRANSMOUNTAIN, StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_REUSE_TO_RESERVOIR, StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_REUSE_TO_RESERVOIR_FROM_TRANSMOUNTAIN, StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_TRANSMOUNTAIN_IMPORT, StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_SPECIAL_WELL_AUGMENTATION};
					// TODO SAM 2011-02-05 This is handled dynamically based on source 1
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source2Array_49 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.NA};
					StateMod_OperationalRight_Metadata_SourceOrDestinationType[] destinationArray_49 = new StateMod_OperationalRight_Metadata_SourceOrDestinationType[] {StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_TC, StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_SPECIAL_WELL_AUGMENTATION, StateMod_OperationalRight_Metadata_SourceOrDestinationType.PLAN_WELL_AUGMENTATION};
					StateMod_OperationalRight_Metadata_DestinationLocationType[] destinationLocationArray_49 = new StateMod_OperationalRight_Metadata_DestinationLocationType[] {StateMod_OperationalRight_Metadata_DestinationLocationType.UPSTREAM};
					StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] associatedPlanAllowedArray_49 = new StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] {StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType.NA};
					StateMod_OperationalRight_Metadata_DiversionType[] diversionTypeArray_49 = new StateMod_OperationalRight_Metadata_DiversionType[] {StateMod_OperationalRight_Metadata_DiversionType.DIVERSION};
					metaData = new StateMod_OperationalRight_Metadata(i, true, "Plan or Reservoir Reuse to a Plan by Exchange", StateMod_OperationalRight_Metadata_RuleType.SOURCE_PLAN_STRUCTURE, source1Array_49, source2Array_49, StateMod_OperationalRight_Metadata_Source2AllowedType.ALLOWED, destinationArray_49, destinationLocationArray_49, StateMod_OperationalRight_Metadata_DeliveryMethodType.RIVER, StateMod_OperationalRight_Metadata_CarrierAllowedType.NA, false, false, associatedPlanAllowedArray_49, diversionTypeArray_49, StateMod_OperationalRight_Metadata_TransitAndConveyanceLossAllowedType.NA, "");
					break;
			}
			__opRightsMetadataList.Add(metaData);
		}
	}

	/// <summary>
	/// Read the global public metadata.  This is intended to supply metadata only for rights that are not
	/// understood by the code.  For example, it is possible that new rights will be added to the FORTRAN model
	/// but the Java will not be updated.  Consequently, the external file helps the software implement advanced
	/// features such as drawing the operational right on the network.
	/// Currently the only data read indicate whether full editing for the right is supported. </summary>
	/// <param name="filename"> name of operational rights metadata </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void readGlobalData(String filename) throws java.io.FileNotFoundException, java.io.IOException
	public static void readGlobalData(string filename)
	{
		string routine = "StateMod_OperationalRight_Metadata.readGlobalData";
		try
		{
			PropList props = new PropList("opr");
			props.set("CommentLineIndicator=#");
			props.set("Delimiter=,");
			props.set("ColumnDataTypes=Auto");
			props.set("TrimStrigs=true");
			DataTable table = DataTable.parseFile(filename, props);
			// Loop through the table rows and reset if any rights are not supported for editing
			int nrows = table.getNumberOfRecords();
			string fullEditingSupported;
			int? ruleType;
			for (int iRow = 0; iRow < nrows; iRow++)
			{
				ruleType = (int?)table.getFieldValue(iRow,"Type");
				fullEditingSupported = (string)table.getFieldValue(iRow, "EditingSupported");
				if ((!string.ReferenceEquals(fullEditingSupported, null)) && fullEditingSupported.Equals("no", StringComparison.OrdinalIgnoreCase))
				{
					Message.printStatus(2, routine, "Turning off full editing for operational right type " + ruleType + " because EditingSupported=no");
					StateMod_OperationalRight_Metadata.getMetadata(ruleType.Value).setFullEditingSupported(false);
				}
			}
		}
		catch (Exception e)
		{
			throw new IOException("Error reading operational right configuruation file \"" + filename + "\"", e);
		}
	}

	/// <summary>
	/// Read the contents of the operating rule definitions file. </summary>
	/// <param name="filename"> name of file to read (absolute path). </param>
	/// <returns> a list of StateMod_OperationalRights_Metadata from the definitions file. </returns>
	/*
	public static List readSpreadsheet ( String filename )
	throws FileNotFoundException, IOException
	{	String routine = "StateMod_OperationalRight_Metadata.readSpreadsheet";
		FileInputStream fis = null;
		List metadataList = new Vector();
		try {
			POIFSFileSystem fs = null;
			fs = new POIFSFileSystem(fis = new FileInputStream(filename) );
			// Create a workbook (why not just get it from the fs?)
			HSSFWorkbook wb = new HSSFWorkbook(fs);
			// Get the first sheet from the workbook
			HSSFSheet mySheet = wb.getSheetAt(0);
			// Iterate through the rows and cells
			Iterator rowIter = mySheet.rowIterator(); 
			while(rowIter.hasNext()){
				  HSSFRow myRow = (HSSFRow)rowIter.next();
				  Iterator cellIter = myRow.cellIterator();
				  Vector cellStoreVector=new Vector();
				  while(cellIter.hasNext()) {
					  HSSFCell myCell = (HSSFCell) cellIter.next();
					  cellStoreVector.addElement(myCell);
					  Message.printStatus(2,routine, "Row " + myRow.getRowNum() + " Cell " + myCell.getColumnIndex() +
						  " value=" + myCell );
				  }
			 }
		}
		finally {
			if ( fis != null ) {
				fis.close();
			}
		}
		return metadataList;
	}
	*/

	/// <summary>
	/// Set the allowed associated plan types (in agreement with StateMod documentation).
	/// Make private because objects should be immutable. </summary>
	/// <param name="associatedPlanAllowedTypes"> types of destinations allowed for the right </param>
	private void setAssociatedPlanAllowedTypes(StateMod_OperationalRight_Metadata_AssociatedPlanAllowedType[] associatedPlanAllowedTypes)
	{
		__associatedPlanAllowedTypes = associatedPlanAllowedTypes;
	}

	/// <summary>
	/// Set the destination types (in agreement with StateMod documentation).
	/// Make private because objects should be immutable. </summary>
	/// <param name="destinationTypes"> types of destinations allowed for the right </param>
	private void setDestinationTypes(StateMod_OperationalRight_Metadata_SourceOrDestinationType[] destinationTypes)
	{
		__destinationTypes = destinationTypes;
	}

	/// <summary>
	/// Set the allowed diversion types (in agreement with StateMod documentation).
	/// Make private because objects should be immutable. </summary>
	/// <param name="diversionTypes"> types of diversions allowed for the right </param>
	private void setDiversionTypes(StateMod_OperationalRight_Metadata_DiversionType[] diversionTypes)
	{
		__diversionTypes = diversionTypes;
	}

	/// <summary>
	/// Set whether full editing is supported. </summary>
	/// <param name="fullEditingSupported"> whether full editing is supported </param>
	private void setFullEditingSupported(bool fullEditingSupported)
	{
		__fullEditingSupported = fullEditingSupported;
	}

	/// <summary>
	/// Set the operational right name (in agreement with StateMod documentation).
	/// Make private because objects should be immutable. </summary>
	/// <param name="rightTypeName"> name of operational right </param>
	private void setRightTypeName(string rightTypeName)
	{
		__rightTypeName = rightTypeName;
	}

	/// <summary>
	/// Set the operational right type (1+ in agreement with StateMod documentation).
	/// Make private because objects should be immutable. </summary>
	/// <param name="rightTypeNumber"> type (number) of operational right </param>
	private void setRightTypeNumber(int rightTypeNumber)
	{
		__rightTypeNumber = rightTypeNumber;
	}

	/// <summary>
	/// Set the operating rule type, in agreement with StateMod documentation.
	/// Make private because objects should be immutable. </summary>
	/// <param name="ruleTypeCategory"> category of operating rule </param>
	private void setRuleTypeCategory(StateMod_OperationalRight_Metadata_RuleType ruleTypeCategory)
	{
		__ruleTypeCategory = ruleTypeCategory;
	}

	/// <summary>
	/// Set the source1 types (in agreement with StateMod documentation).
	/// Make private because objects should be immutable. </summary>
	/// <param name="sourceTypes"> types of destinations allowed for the right </param>
	private void setSource1Types(StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source1Types)
	{
		__source1Types = source1Types;
	}

	/// <summary>
	/// Set the source2 types (in agreement with StateMod documentation).
	/// Make private because objects should be immutable. </summary>
	/// <param name="sourceTypes"> types of destinations allowed for the right </param>
	private void setSource2Types(StateMod_OperationalRight_Metadata_SourceOrDestinationType[] source2Types)
	{
		__source2Types = source2Types;
	}

	/// <summary>
	/// Set whether transit and conveyance loss are allowed.
	/// Make private because objects should be immutable. </summary>
	/// <param name="diversionTypes"> types of diversions allowed for the right </param>
	private void setTransitAndConveyanceLossAllowed(StateMod_OperationalRight_Metadata_TransitAndConveyanceLossAllowedType transitAndConveyanceLossAllowed)
	{
		__transitAndConveyanceLossAllowed = transitAndConveyanceLossAllowed;
	}

	/// <summary>
	/// Set whether the operational right uses intervening structures with loss.
	/// Make private because objects should be immutable. </summary>
	/// <param name="usesInterveningStructuresWithoutLoss"> whether intervening structures are used </param>
	private void setUsesInterveningStructuresWithLoss(bool usesInterveningStructuresWithLoss)
	{
		__rightTypeUsesInterveningStructuresWithLoss = usesInterveningStructuresWithLoss;
	}

	/// <summary>
	/// Set whether the operational right uses intervening structures without loss.
	/// Make private because objects should be immutable. </summary>
	/// <param name="usesInterveningStructuresWithoutLoss"> whether intervening structures are used </param>
	private void setUsesInterveningStructuresWithoutLoss(bool usesInterveningStructuresWithoutLoss)
	{
		__rightTypeUsesInterveningStructuresWithoutLoss = usesInterveningStructuresWithoutLoss;
	}

	}

}