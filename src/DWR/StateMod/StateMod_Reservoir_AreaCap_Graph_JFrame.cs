using System;
using System.Collections.Generic;

// StateMod_Reservoir_AreaCap_Graph_JFrame - frame to graph a reservoir's content/area/seepage curve

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
// StateMod_Reservoir_AreaCap_Graph_JFrame - frame to graph a reservoir's 
//	content/area/seepage curve
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
// 
// 2006-02-28	Steven A. Malers, RTi	Create class to use the JFreeChart
//					package.  Initial prototyping was
//					created by JTS.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
//------------------------------------------------------------------------------
// EndHeader

namespace DWR.StateMod
{


	using ChartFactory = org.jfree.chart.ChartFactory;
	using ChartPanel = org.jfree.chart.ChartPanel;
	using JFreeChart = org.jfree.chart.JFreeChart;
	using NumberAxis = org.jfree.chart.axis.NumberAxis;
	using PlotOrientation = org.jfree.chart.plot.PlotOrientation;
	using XYPlot = org.jfree.chart.plot.XYPlot;
	using TextTitle = org.jfree.chart.title.TextTitle;
	using DefaultTableXYDataset = org.jfree.data.xy.DefaultTableXYDataset;
	using XYSeries = org.jfree.data.xy.XYSeries;

	using JGUIUtil = RTi.Util.GUI.JGUIUtil;
	using Message = RTi.Util.Message.Message;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class StateMod_Reservoir_AreaCap_Graph_JFrame extends javax.swing.JFrame
	public class StateMod_Reservoir_AreaCap_Graph_JFrame : JFrame
	{

	private StateMod_Reservoir __res = null;
	private string __type = "Area";

	/// <summary>
	/// Display a graph of reservoir content versus area or content versus seepage. </summary>
	/// <param name="dataset"> The dataset including the reservoir. </param>
	/// <param name="res"> StateMod_Reservoir with data to graph. </param>
	/// <param name="type"> "Area" or "Seepage", indicating the data to graph. </param>
	/// <param name="editable"> Indicate whether the data are editable or not (currently
	/// ignored and treated as not editable through the graph). </param>
	public StateMod_Reservoir_AreaCap_Graph_JFrame(StateMod_DataSet dataset, StateMod_Reservoir res, string type, bool editable)
	{
		StateMod_GUIUtil.setTitle(this, dataset, res.getName() + " - Reservoir Content/" + type + " Curve", null);
		JGUIUtil.setIcon(this, JGUIUtil.getIconImage());

		__res = res;
		__type = type;

		DefaultTableXYDataset graph_dataset = createDataset();
		JFreeChart chart = createChart(graph_dataset);
		ChartPanel chartPanel = new ChartPanel(chart);
		chartPanel.setPreferredSize(new Dimension(500, 500));
		getContentPane().add("Center", chartPanel);
		pack();
		JGUIUtil.center(this);
		setVisible(true);
	}

	/// <summary>
	/// Create a dataset to display in the chart.  Extract the values from the
	/// StateMod_Reservoir curve data.
	/// </summary>
	private DefaultTableXYDataset createDataset()
	{
		string routine = "StateMod_Reservoir_AreaCap_Graph_JFrame.createDataSet";
		DefaultTableXYDataset dataset = new DefaultTableXYDataset();

		IList<StateMod_ReservoirAreaCap> v = __res.getAreaCaps();
		int size = 0;
		if (v != null)
		{
			size = v.Count;
		}
		XYSeries series = new XYSeries("Reservoir " + __res.getID() + " (" + __res.getName() + ") Content/" + __type + " Curve", false, false);
		StateMod_ReservoirAreaCap ac = null;
		// Speed up checks in loop...
		bool do_area = true;
		if (__type.Equals("Seepage", StringComparison.OrdinalIgnoreCase))
		{
			do_area = false;
		}
		double value_prev = -10000000.0, value = 0.0, value2 = 0.0;
		double content = 0.0;
		int match_count = 0;
		for (int i = 0; i < size; i++)
		{
			ac = (StateMod_ReservoirAreaCap)v[i];
			// Curves will often have a very large content to protect
			// against out of bounds for interpolation.  However, if this
			// point is graphed, it causes the other values to appear
			// miniscule.  Therefore, omit the last point if it is much
			// larger than the previous value.
			if ((size > 4) && (i == (size - 1)) && ac.getConten() > 9000000.0)
			{
				Message.printStatus(2, routine, "Skipping last point.  Seems to be very large bounding" + " value and might skew the graph.");
				continue;
			}
			// Add X first, then Y...
			if (do_area)
			{
				value = ac.getSurarea();
			}
			else
			{
				value = ac.getSeepage();
			}
			if (value == value_prev)
			{
				// This is needed because JFreeChart will not allow
				// adjacent X values to be the same.
				++match_count;
				value2 = value + match_count * .00001;
			}
			else
			{
				value2 = value;
			}
			content = ac.getConten();
			// REVISIT SAM 2006-08-20
			// Not sure if content needs to be checked the same way for
			// duplicates.
			Message.printStatus(2, routine, "X=" + value2 + " y=" + content);
			series.add(value2,content);
			value_prev = value;
		}
		dataset.addSeries(series);

		return dataset;
	}

	/// <summary>
	/// Create the chart using the data.
	/// </summary>
	private JFreeChart createChart(DefaultTableXYDataset dataset)
	{
		string xlabel = "Surface Area (ACRE)";
		if (__type.Equals("Seepage"))
		{
			xlabel = "Seepage (AF/M)";
		}
		JFreeChart chart = ChartFactory.createXYLineChart("Reservoir Content/" + __type + " Curve", xlabel, "Content (ACFT)", dataset, PlotOrientation.VERTICAL, false, true, false);

		chart.addSubtitle(new TextTitle(__res.getID() + " (" + __res.getName() + ")"));
		//TextTitle source = new TextTitle("Source: rgtTW.res");
		//source.setFont(new Font("SansSerif", Font.PLAIN, 10));
		//source.setPosition(RectangleEdge.BOTTOM);
		//source.setHorizontalAlignment(HorizontalAlignment.RIGHT);
		//chart.addSubtitle(source);

		chart.setBackgroundPaint(Color.WHITE);

		XYPlot plot = (XYPlot)chart.getPlot();
		plot.setBackgroundPaint(Color.white);
		plot.setRangeGridlinePaint(Color.lightGray);

		NumberAxis rangeAxis = (NumberAxis)plot.getRangeAxis();
		rangeAxis.setStandardTickUnits(NumberAxis.createIntegerTickUnits());

		return chart;
	}

	/// <summary>
	/// Create the panel to hold the chart.
	/// </summary>
	public virtual JPanel createDemoPanel()
	{
		JFreeChart chart = createChart(createDataset());
		return new ChartPanel(chart);
	}

	}

}