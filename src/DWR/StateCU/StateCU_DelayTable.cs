using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

// StateCU_DelayTable - contains delay table

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
// StateCU_DelayTable - Contains delay table I/O.
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
// 
// 2004-03-17	Steven A. Malers, RTi	Copy StateMod_DelayTable and update to
//					have only the static readStateCUFile()
//					and writeCUFile() methods.
// 2004-03-31	SAM, RTi		Fix bug where delay values were written
//					on one line instead of 12 per line.
// 2005-04-19	J. Thomas Sapienza, RTi	Enabled readStateCUFile().
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
//------------------------------------------------------------------------------

namespace DWR.StateCU
{

	using StateMod_DelayTable = DWR.StateMod.StateMod_DelayTable;
	using IOUtil = RTi.Util.IO.IOUtil;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// This class reads and writes StateCU delay table files.  The StateCU and StateMod
	/// file formats are similar.  Consequently, rather than redefining data members
	/// here, the StateMod delay tables should be used.  This class simply provides
	/// read and write methods for StateCU file formats.
	/// </summary>
	public abstract class StateCU_DelayTable
	{

	/// <summary>
	/// Read delay information from a StateCU file and store in a java vector.  The new
	/// delay entries are added to the end of the previously stored delays.  Returns the delay table information. </summary>
	/// <returns> a Vector of StateMod_DelayTable </returns>
	/// <param name="filename"> the filename to read from. </param>
	/// <param name="interv"> The control file interv parameter.  +n indicates the number of
	/// values in each delay pattern.  -1 indicates variable number of values with
	/// values as percent (0-100).  -100 indicates variable number of values with values as fraction (0-1). </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.List<DWR.StateMod.StateMod_DelayTable> readStateCUFile(String filename, int interv) throws Exception
	public static IList<StateMod_DelayTable> readStateCUFile(string filename, int interv)
	{
		bool is_monthly = true;
		string routine = "StateCU_DelayTable.readStateCUFile";
		string iline;
		IList<StateMod_DelayTable> theDelays = new List<StateMod_DelayTable>(1);
		StateMod_DelayTable aDelay = new StateMod_DelayTable(is_monthly);
		int num_read = 0, total_num_to_read = 0;
		bool reading = false;
		StreamReader @in = null;
		StringTokenizer split = null;

		if (Message.isDebugOn)
		{
			Message.printDebug(10, routine, "in readStateModFile reading file: " + filename);
		}
		try
		{
			@in = new StreamReader(filename);
			while (!string.ReferenceEquals((iline = @in.ReadLine()), null))
			{
				// check for comments
				iline = iline.Trim();
				if (iline.StartsWith("#", StringComparison.Ordinal) || iline.Length == 0)
				{
					continue;
				}

				split = new StringTokenizer(iline);
				if ((split == null) || (split.countTokens() == 0))
				{
					continue;
				}

				if (!reading)
				{
					// allocate new delay node
					aDelay = new StateMod_DelayTable(is_monthly);
					num_read = 0;
					reading = true;
					theDelays.Add(aDelay);
					aDelay.setTableID(split.nextToken());

					if (interv < 0)
					{
						aDelay.setNdly(split.nextToken());
					}
					else
					{
						aDelay.setNdly(interv);
					}
					total_num_to_read = aDelay.getNdly();
					// Set the delay table units(default is percent)...
					aDelay.setUnits("PCT");
					if (interv == -100)
					{
						aDelay.setUnits("FRACTION");
					}
				}

				while (split.hasMoreTokens())
				{
					aDelay.addRet_val(split.nextToken());
					num_read++;
				}
				if (num_read >= total_num_to_read)
				{
					reading = false;
				}
			}
		}
		catch (Exception e)
		{
			Message.printWarning(2, routine, e);
			throw e;
		}
		finally
		{
			if (@in != null)
			{
				@in.Close();
			}
		}
		return theDelays;
	}

	/// <summary>
	/// Write the new (updated) delay table file.  This routine writes the new delay
	/// table file.  If an original file is specified, then the original header is
	/// carried into the new file.  The writing of data is done by the dumpDelayFile
	/// routine which now does not mess with headers. </summary>
	/// <param name="inputFile"> old file (used as input) </param>
	/// <param name="outputFile"> new file to create </param>
	/// <param name="dly"> list of delays </param>
	/// <param name="newcomments"> new comments to save with the header of the file </param>
	/// <exception cref="Exception"> if an error occurs </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeStateCUFile(String inputFile, String outputFile, java.util.List<DWR.StateMod.StateMod_DelayTable> dly, java.util.List<String> newcomments) throws Exception
	public static void writeStateCUFile(string inputFile, string outputFile, IList<StateMod_DelayTable> dly, IList<string> newcomments)
	{
		PrintWriter @out = null;
		IList<string> commentStr = new List<string>(1);
		commentStr.Add("#");
		IList<string> ignoreCommentStr = new List<string>(1);
		ignoreCommentStr.Add("#>");
		string routine = "StateMod_DelayTable.writeStateCUFile";

		Message.printStatus(2, routine, "Writing new delay table to file \"" + outputFile + "\" using \"" + inputFile + "\" header...");

		try
		{
			// Process the header from the old file...
			@out = IOUtil.processFileHeaders(IOUtil.getPathUsingWorkingDir(inputFile), IOUtil.getPathUsingWorkingDir(outputFile), newcomments, commentStr, ignoreCommentStr, 0);

			// Now write the new data...
			string cmnt = "#>";
			string m_format = "%8.2f";
			StateMod_DelayTable delay = null;

			@out.println(cmnt);
			@out.println(cmnt + " *******************************************************");
			@out.println(cmnt + " StateCU Delay (Return flow) Table");
			@out.println(cmnt);
			@out.println(cmnt + "     Format (a8, i4, (12f8.2)");
			@out.println(cmnt);
			@out.println(cmnt + "   ID       idly: Delay table id");
			@out.println(cmnt + "   Ndly     ndly: Number of entries in delay table idly.");
			@out.println(cmnt + "   Ret  dlyrat(1-n,idl): Return for month n, station idl");
			@out.println(cmnt);
			@out.println(cmnt + " ID   Ndly  Ret1    Ret2    Ret3    Ret4    Ret5    Ret6  " + "  Ret7    Ret8    Ret9    Ret10   Ret11   Ret12...");
			@out.println(cmnt + "-----eb--eb------eb------eb------eb------eb------eb------e" + "b------eb------eb------eb------eb------eb------e...");
			@out.println(cmnt + "EndHeader");
			@out.println(cmnt);

			int ndly = 0;
			if (dly != null)
			{
				ndly = dly.Count;
			}
			if (Message.isDebugOn)
			{
				Message.printDebug(3, routine, "Printing " + ndly + " delay table entries.");
			}

			StringBuilder b = new StringBuilder();
			int j = 0; // Index for returns in a table
			int nvals = 0; // Number of returns in a table
			bool printed; // Indicates if a line of output was printed, to help handle 12 values or less per line
			for (int i = 0; i < ndly; i++)
			{
				delay = dly[i];
				b.Length = 0;
				b.Append(StringUtil.formatString(delay.getTableID(), "%8d"));
				b.Append(StringUtil.formatString(delay.getNdly(), "%4d"));
				nvals = delay.getNdly();
				printed = false;
				for (j = 0; j < nvals; j++)
				{
					b.Append(StringUtil.formatString(delay.getRet_val(j), m_format));
					printed = false;
					if (((j + 1) % 12) == 0)
					{
						// Print the output and initialize a new line...
						@out.println(b.ToString());
						b.Length = 0;
						b.Append("            ");
						printed = true;
					}
				}
				if (!printed)
				{
					// Print the last line of output...
					@out.println(b.ToString());
				}
			}
		}
		catch (Exception e)
		{
			Message.printWarning(2, routine, e);
			throw e;
		}
		finally
		{
			if (@out != null)
			{
				@out.close();
			}
		}
	}

	}

}