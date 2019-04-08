// StateMod_NetworkReference_JComponent - class to control drawing of the network reference window

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

// ----------------------------------------------------------------------------
// StateMod_NetworkReference_JComponent - class to control drawing of 
//	the network reference window
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
//
// 2004-03-18	J. Thomas Sapienza, RTi	Initial version.  
// 2004-03-19 - 2004-03-22	JTS,RTi	Much more work getting a cleaner-
//					working version. 
// 2004-03-23	JTS, RTi		Javadoc'd.
// 2004-10-20	JTS, RTi		A black separator line is now drawn
//					around the component.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace DWR.StateMod
{

	using HydrologyNode = cdss.domain.hydrology.network.HydrologyNode;

	using GRColor = RTi.GR.GRColor;
	using GRDrawingAreaUtil = RTi.GR.GRDrawingAreaUtil;
	using GRJComponentDevice = RTi.GR.GRJComponentDevice;
	using GRJComponentDrawingArea = RTi.GR.GRJComponentDrawingArea;
	using GRLimits = RTi.GR.GRLimits;
	using GRText = RTi.GR.GRText;

	/// <summary>
	/// This class draws the reference window for the network drawing code.  The 
	/// reference window and the main window interact in ways, such as by allowing
	/// positioning by clicking on the reference window, and responding to network
	/// changes from the main window.
	/// </summary>
	public class StateMod_NetworkReference_JComponent : GRJComponentDevice, MouseListener, MouseMotionListener
	{

	/// <summary>
	/// Whether to draw the grid or not.
	/// </summary>
	private bool __drawInchGrid = false;

	/// <summary>
	/// Whether to draw the printable area margin or not.
	/// </summary>
	private bool __drawMargin = false;

	/// <summary>
	/// Whether to force paint() to refresh the entire drawing area or not.
	/// </summary>
	private bool __forceRefresh = true;

	/// <summary>
	/// Whether the box representing the current network view is being dragged around the display.
	/// </summary>
	private bool __inDrag = false;

	/// <summary>
	/// Whether drawing settings need to be initialized because it is the first
	/// time paint() has been called.
	/// </summary>
	private bool __initialized = false;

	/// <summary>
	/// The printing scale factor of the drawing.  This is the amount by which the
	/// 72 dpi printable pixels are scaled.  A printing scale value of 1 means that
	/// the ER diagram will be printed at 72 pixels per inch (ppi), which is the 
	/// java standard.   A scale factor of .5 means that the ER Diagram will be 
	/// printed at 144 ppi.  A scale factor of 3 means that the ER Diagram will be printed at 24 ppi.
	/// </summary>
	// TODO SAM 2007-03-01 Evaluate use
	//private double __printScale = 1;

	/// <summary>
	/// The height of the drawing area, in pixels.
	/// </summary>
	private double __height = 0;

	/// <summary>
	/// The width of the drawing area, in pixels.
	/// </summary>
	private double __width = 0;

	/// <summary>
	/// A dash pattern with large dash spacing
	/// </summary>
	private float[] __bigDashes = new float[] {3f, 5f};

	/// <summary>
	/// A dash pattern with smaller dash spacing.
	/// </summary>
	private float[] __smallDashes = new float[] {2f, 2f};

	/// <summary>
	/// The drawing area on which the main network is drawn.
	/// </summary>
	private GRJComponentDrawingArea __drawingArea;

	/// <summary>
	/// The node network read in from a makenet file.
	/// </summary>
	private StateMod_NodeNetwork __network;

	/// <summary>
	/// The difference between where the user clicked in the box that represents the
	/// currently-viewed area of the network and the far bottom left point of the box.
	/// </summary>
	private int __xAdjust, __yAdjust;

	/// <summary>
	/// The far bounds of the network, in terms of the reference window pixels.
	/// </summary>
	private int __leftX = 0, __bottomY = 0, __totalHeight = 0, __totalWidth = 0;

	/// <summary>
	/// The class that draws the full network, and which interacts with the reference display.
	/// </summary>
	private StateMod_Network_JComponent __networkJComponent;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="parent"> the JFrame on which this appears. </param>
	public StateMod_NetworkReference_JComponent(JFrame parent) : base("StateMod_NetworkReference_JComponent")
	{
		addMouseListener(this);
		addMouseMotionListener(this);
	}

	/// <summary>
	/// Clears the screen; fills with white.
	/// </summary>
	public virtual void clear()
	{
		__drawingArea.clear(GRColor.gray);
		GRDrawingAreaUtil.setColor(__drawingArea, GRColor.white);
		__drawingArea.setScaleData(false);
		GRDrawingAreaUtil.fillRectangle(__drawingArea, __leftX, __bottomY, __totalWidth, __totalHeight);
		__drawingArea.setScaleData(true);
	}

	/// <summary>
	/// Converts an X value from being scaled for drawing units to be scaled for data units. </summary>
	/// <param name="x"> the x value to scale. </param>
	/// <returns> the x value scaled to fit in the data units. </returns>
	public virtual double convertX(double x)
	{
		GRLimits data = __drawingArea.getDataLimits();
		GRLimits draw = __drawingArea.getDrawingLimits();
		double newX = (data.getLeftX() + (x / draw.getWidth()) * data.getWidth());
		return newX;
	}

	/// <summary>
	/// Converts an Y value from being scaled for drawing units to be scaled for data units. </summary>
	/// <param name="y"> the y value to scale. </param>
	/// <returns> the y value scaled to fit in the data units. </returns>
	public virtual double convertY(double y)
	{
		GRLimits data = __drawingArea.getDataLimits();
		GRLimits draw = __drawingArea.getDrawingLimits();
		double newY = (data.getBottomY() + (y / draw.getHeight()) * data.getHeight());
		return newY;
	}

	/// <summary>
	/// Draws the bounds that represent the currently-visible area of the network.
	/// </summary>
	private void drawBounds()
	{
		GRLimits vl = __networkJComponent.getVisibleDataLimits();
		if (vl == null)
		{
			return;
		}
		GRDrawingAreaUtil.setColor(__drawingArea, GRColor.green);
		double lx = vl.getLeftX();
		double by = vl.getBottomY();
		double rx = vl.getRightX();
		double ty = vl.getTopY();
		GRDrawingAreaUtil.drawLine(__drawingArea, lx, by, rx, by);
		GRDrawingAreaUtil.drawLine(__drawingArea, lx, ty, rx, ty);
		GRDrawingAreaUtil.drawLine(__drawingArea, lx, by, lx, ty);
		GRDrawingAreaUtil.drawLine(__drawingArea, rx, by, rx, ty);

	}

	/// <summary>
	/// Draws the outline of the legend.
	/// </summary>
	public virtual void drawLegend()
	{
		GRLimits l = __networkJComponent.getLegendDataLimits();
		if (l == null)
		{
			return;
		}
		GRDrawingAreaUtil.setColor(__drawingArea, GRColor.white);
		double lx = l.getLeftX();
		double by = l.getBottomY();
		double rx = l.getRightX();
		double ty = l.getTopY();
		GRDrawingAreaUtil.fillRectangle(__drawingArea, lx, by, l.getWidth(), l.getHeight());
		GRDrawingAreaUtil.setColor(__drawingArea, GRColor.black);
		GRDrawingAreaUtil.drawLine(__drawingArea, lx, by, rx, by);
		GRDrawingAreaUtil.drawLine(__drawingArea, lx, ty, rx, ty);
		GRDrawingAreaUtil.drawLine(__drawingArea, lx, by, lx, ty);
		GRDrawingAreaUtil.drawLine(__drawingArea, rx, by, rx, ty);
	}

	/// <summary>
	/// Draws the network between all the nodes.
	/// </summary>
	private void drawNetworkLines()
	{
		bool dash = false;
		float[] dashes = new float[] {5f, 4f};
		float offset = 0;
		double[] x = new double[2];
		double[] y = new double[2];
		HydrologyNode ds = null;
		HydrologyNode dsRealNode = null;
		HydrologyNode holdNode = null;
		HydrologyNode holdNode2 = null;
		HydrologyNode node = null;
		HydrologyNode nodeTop = __network.getMostUpstreamNode();

		GRDrawingAreaUtil.setLineWidth(__drawingArea, 1);

		for (node = nodeTop; node != null; node = StateMod_NodeNetwork.getDownstreamNode(node, StateMod_NodeNetwork.POSITION_COMPUTATIONAL))
		{
				// move ahead and skip and blank or unknown nodes (which won't
			// be drawn, anyways -- check buildNodeArray()), so that 
			// connections are only between visible nodes
			if (holdNode == node)
			{
				GRDrawingAreaUtil.setLineWidth(__drawingArea, 1);
				return;
			}
			holdNode2 = node;
			while (node.getType() == HydrologyNode.NODE_TYPE_UNKNOWN)
			{
				node = StateMod_NodeNetwork.getDownstreamNode(node, StateMod_NodeNetwork.POSITION_COMPUTATIONAL);
				if (node == null || node == holdNode2)
				{
					GRDrawingAreaUtil.setLineWidth(__drawingArea,1);
					return;
				}
			}

			ds = node.getDownstreamNode();
			if (ds == null || node.getType() == HydrologyNode.NODE_TYPE_END)
			{
				GRDrawingAreaUtil.setLineWidth(__drawingArea, 1);
				return;
			}

			dsRealNode = StateMod_NodeNetwork.findNextRealOrXConfluenceDownstreamNode(node);

			// if the confluence of the reach (as opposed to a trib coming
			// in) then this is the last real node in disappearing stream.
			// Use the end node for the downstream node.
			dash = false;
			if (dsRealNode == StateMod_NodeNetwork.getDownstreamNode(node, StateMod_NodeNetwork.POSITION_REACH))
			{
				dash = true;
			}

			// move ahead and skip and blank or unknown nodes (which won't
			// be drawn, anyways -- check buildNodeArray()), so that 
			// connections are only between visible nodes
			holdNode2 = ds;
			while (ds.getType() == HydrologyNode.NODE_TYPE_UNKNOWN)
			{
				ds = ds.getDownstreamNode();
				if (ds == null || ds == holdNode2)
				{
					GRDrawingAreaUtil.setLineWidth(__drawingArea,1);
					return;
				}
			}

			x[0] = node.getX();
			y[0] = node.getY();
			x[1] = ds.getX();
			y[1] = ds.getY();

			GRDrawingAreaUtil.setColor(__drawingArea, GRColor.black);
			GRDrawingAreaUtil.setLineWidth(__drawingArea, 1);

			if (dash)
			{
				__drawingArea.setFloatLineDash(dashes, offset);
				GRDrawingAreaUtil.drawLine(__drawingArea, x, y);
				__drawingArea.setFloatLineDash(null, (float)0);
			}
			else
			{
				GRDrawingAreaUtil.drawLine(__drawingArea, x, y);
			}
			holdNode = node;
		}
		GRDrawingAreaUtil.setLineWidth(__drawingArea, 1);
	}

	/// <summary>
	/// Forces the display to completely repaint itself immediately.
	/// </summary>
	public virtual void forceRepaint()
	{
		__forceRefresh = true;
		repaint();
	}

	/// <summary>
	/// Returns the height of the drawing area (in pixels). </summary>
	/// <returns> the height of the drawing area (in pixels). </returns>
	public virtual double getDrawingHeight()
	{
		return __height;
	}

	/// <summary>
	/// Returns the width of the drawing area (in pixels). </summary>
	/// <returns> the width of the drawing area (in pixels). </returns>
	public virtual double getDrawingWidth()
	{
		return __width;
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void mouseClicked(MouseEvent @event)
	{
	}

	/// <summary>
	/// Responds to mouse dragged events by moving around the box representing 
	/// the viewable network area, and also making the large network display change to show that area. </summary>
	/// <param name="event"> the MouseEvent that happened. </param>
	public virtual void mouseDragged(MouseEvent @event)
	{
		if (__inDrag)
		{
			if (@event.getX() < __leftX || @event.getX() > __leftX + __totalWidth)
			{
				return;
			}
			if (@event.getY() < __bottomY || @event.getY() > __bottomY + __totalHeight)
			{
				return;
			}

			__inDrag = true;
			GRLimits data = __networkJComponent.getTotalDataLimits();

			int width = __totalWidth;
			double pct = (double)(@event.getX() - __leftX) / (double)width;
			int nw = (int)data.getWidth();
			int xPoint = (int)((nw * pct) + data.getLeftX());

			int height = __totalHeight;
			pct = (double)(height - @event.getY() + __bottomY) / (double)height;
			int nh = (int)data.getHeight();
			int yPoint = (int)((nh * pct) + data.getBottomY());

			int x = xPoint - __xAdjust;
			int y = yPoint - __yAdjust;
			__networkJComponent.setViewPosition(x, y);
		}
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void mouseEntered(MouseEvent @event)
	{
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void mouseExited(MouseEvent @event)
	{
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void mouseMoved(MouseEvent @event)
	{
	}

	/// <summary>
	/// Responds to mouse pressed events.  If the mouse was clicked within the box that
	/// represents the viewable network area, then that view can be dragged around and
	/// will be represented in the large display.  Otherwise, the view is re-centered
	/// around the mouse click point. </summary>
	/// <param name="event"> the MouseEvent that happened. </param>
	public virtual void mousePressed(MouseEvent @event)
	{
		if (@event.getX() < __leftX || @event.getX() > __leftX + __totalWidth)
		{
			return;
		}
		if (@event.getY() < __bottomY || @event.getY() > __bottomY + __totalHeight)
		{
			return;
		}
		__inDrag = true;

		GRLimits data = __networkJComponent.getTotalDataLimits();
		int width = __totalWidth;
		double pct = (double)(@event.getX() - __leftX) / (double)width;
		int nw = (int)data.getWidth();
		int xPoint = (int)((nw * pct) + data.getLeftX());

		int height = __totalHeight;
		pct = (double)(height - @event.getY() + __bottomY) / (double)height;
		int nh = (int)data.getHeight();
		int yPoint = (int)((nh * pct) + data.getBottomY());
		GRLimits limits = __networkJComponent.getVisibleDataLimits();

		int lx = (int)limits.getLeftX();
		int by = (int)limits.getBottomY();
		int rx = (int)limits.getRightX();
		int ty = (int)limits.getTopY();
		int w = rx - lx;
		int h = ty - by;

		// If the mouse was clicked in a point outside of the display box, 
		// the box is re-centered on the point and the network display is updated to show that area
		__xAdjust = (w / 2);
		__yAdjust = (h / 2);
		int x = xPoint - __xAdjust;
		int y = yPoint - __yAdjust;
		__networkJComponent.setViewPosition(x, y);
	}

	/// <summary>
	/// Responds to mouse released events -- ends any dragging taking place. </summary>
	/// <param name="event"> the MouseEvent that happened. </param>
	public virtual void mouseReleased(MouseEvent @event)
	{
		__inDrag = false;
	}

	/// <summary>
	/// Paints the screen. </summary>
	/// <param name="g"> the Graphics context to use for painting. </param>
	public virtual void paint(Graphics g)
	{
		// sets the graphics in the base class appropriately (double-buffered
		// if doing double-buffered drawing, single-buffered if not)
		setGraphics(g);

		// Set up drawing limits based on current window size...
		setLimits(getLimits(true));

		// first time through, do the following ...
		if (!__initialized)
		{
			// one time ONLY, do the following.
			__height = getBounds().height;
			__width = getBounds().width;
			__initialized = true;
			setupDoubleBuffer(0, 0, getBounds().width, getBounds().height);

			repaint();
			__forceRefresh = true;
		}

		// only do the following if explicitly instructed to ...
		if (__forceRefresh)
		{
			clear();

			GRDrawingAreaUtil.setColor(__drawingArea, GRColor.black);

			setAntiAlias(true);
			drawNetworkLines();

			setAntiAlias(true);
			drawLegend();

			setAntiAlias(true);
			if (__drawMargin)
			{
				GRLimits margins = __networkJComponent.getMarginLimits();
				int lx = (int)margins.getLeftX();
				int rx = (int)margins.getRightX();
				int by = (int)margins.getBottomY();
				int ty = (int)margins.getTopY();

				__drawingArea.setFloatLineDash(__bigDashes, 0);
				GRDrawingAreaUtil.setColor(__drawingArea, GRColor.cyan);
				GRDrawingAreaUtil.drawLine(__drawingArea, lx, by, rx, by);
				GRDrawingAreaUtil.drawLine(__drawingArea, lx, ty, rx, ty);
				GRDrawingAreaUtil.drawLine(__drawingArea, lx, by, lx, ty);
				GRDrawingAreaUtil.drawLine(__drawingArea, rx, by, rx, ty);
				__drawingArea.setFloatLineDash(null, 0);
			}

			setAntiAlias(true);
			if (__drawInchGrid)
			{
				__drawingArea.setFloatLineDash(__smallDashes, 0);
				double maxX = __networkJComponent.getTotalWidth();
				double maxY = __networkJComponent.getTotalHeight();
				double leftX = __networkJComponent.getDataLeftX();
				double bottomY = __networkJComponent.getDataBottomY();
				double printScale = __networkJComponent.getPrintScale();

				GRDrawingAreaUtil.setColor(__drawingArea, GRColor.red);
				int j = 0;
				int minY = (int)(__networkJComponent.convertAbsY(0) + bottomY);
				int tempMaxY = (int)(__networkJComponent.convertAbsY(maxY) + bottomY);
				for (int i = 0; i < maxX; i += ((72 / printScale) / 2))
				{
					j = (int)(__networkJComponent.convertAbsX(i) + leftX);
					GRDrawingAreaUtil.drawLine(__drawingArea, j, minY, j, tempMaxY);
					GRDrawingAreaUtil.drawText(__drawingArea, "" + ((double)i / (72 / printScale)),j, minY, 0, GRText.CENTER_X | GRText.BOTTOM);
				}

				int minX = (int)(__networkJComponent.convertAbsX(0) + leftX);
				int tempMaxX = (int)(__networkJComponent.convertAbsX(maxX) + leftX);
				for (int i = 0; i < maxY; i += ((72 / printScale) / 2))
				{
					j = (int)(__networkJComponent.convertAbsY(i) + bottomY);
					GRDrawingAreaUtil.drawLine(__drawingArea, minX, j, tempMaxX, j);
					GRDrawingAreaUtil.drawText(__drawingArea, "" + ((double)i / (72 / printScale)),minX, j, 0, GRText.CENTER_Y | GRText.LEFT);
				}
				__drawingArea.setFloatLineDash(null, 0);
			}

			GRDrawingAreaUtil.setColor(__drawingArea, GRColor.black);
			GRLimits drawingLimits = __networkJComponent.getTotalDataLimits();
			GRDrawingAreaUtil.drawLine(__drawingArea, drawingLimits.getLeftX(), drawingLimits.getBottomY(), drawingLimits.getRightX(), drawingLimits.getBottomY());
			GRDrawingAreaUtil.drawLine(__drawingArea, drawingLimits.getLeftX(), drawingLimits.getTopY(), drawingLimits.getRightX(), drawingLimits.getTopY());
			GRDrawingAreaUtil.drawLine(__drawingArea, drawingLimits.getLeftX(), drawingLimits.getTopY(), drawingLimits.getLeftX(), drawingLimits.getBottomY());
			GRDrawingAreaUtil.drawLine(__drawingArea, drawingLimits.getRightX(), drawingLimits.getTopY(), drawingLimits.getRightX(), drawingLimits.getBottomY());

			setAntiAlias(true);
			drawBounds();
		}

		// displays the graphics
		showDoubleBuffer(g);
	}

	/// <summary>
	/// Sets whether the inch grid should be drawn.  The reference display will be instantly redrawn. </summary>
	/// <param name="draw"> whether the inch grid should be drawn. </param>
	public virtual void setDrawInchGrid(bool draw)
	{
		__drawInchGrid = draw;
		forceRepaint();
	}

	/// <summary>
	/// Sets the drawing area to be used with this device. </summary>
	/// <param name="drawingArea"> the drawingArea to use with this device. </param>
	public virtual void setDrawingArea(GRJComponentDrawingArea drawingArea)
	{
		__drawingArea = drawingArea;

		GRLimits data = __drawingArea.getDataLimits();
		double dw = data.getRightX() - data.getLeftX();
		double dh = data.getTopY() - data.getBottomY();

		double ratio = 0;
		if (dw > dh)
		{
			ratio = 200 / dw;
			__totalWidth = 200;
			__leftX = 0;
			__totalHeight = (int)(ratio * dh);
			__bottomY = (200 - __totalHeight) / 2;
		}
		else
		{
			ratio = 200 / dh;
			__totalHeight = 200;
			__bottomY = 0;
			__totalWidth = (int)(ratio * dw);
			__leftX = (200 - __totalWidth) / 2;
		}
	}

	/// <summary>
	/// Sets whether the margin should be drawn.  The reference display will be instantly redrawn. </summary>
	/// <param name="draw"> whether the margin should be drawn. </param>
	public virtual void setDrawMargin(bool draw)
	{
		__drawMargin = draw;
		forceRepaint();
	}

	/// <summary>
	/// Sets the HydrologyNodeNetwork to use. </summary>
	/// <param name="network"> the network to be drawn. </param>
	public virtual void setNetwork(StateMod_NodeNetwork network)
	{
		__network = network;
	}

	/// <summary>
	/// Sets the component that stores the full-view network display. </summary>
	/// <param name="net"> the component with the full-view network display. </param>
	public virtual void setNetworkJComponent(StateMod_Network_JComponent net)
	{
		__networkJComponent = net;
	}

	/// <summary>
	/// Sets new data limits for the network reference window.  Called when, 
	/// for instance, the paper orientation changes. </summary>
	/// <param name="data"> the GRLimits of the data. </param>
	public virtual void setNewDataLimits(GRLimits data)
	{
		double dw = data.getRightX() - data.getLeftX();
		double dh = data.getTopY() - data.getBottomY();

		__drawingArea.setDataLimits(data);
	/*
	System.out.println(">> W/H: " + __totalWidth + "  " + __totalHeight);
	System.out.println(">> L/B: " + __leftX + "  " + __bottomY);
	*/
		double ratio = 0;
		if (dw > dh)
		{
			ratio = 200 / dw;
			__totalWidth = 200;
			__leftX = 0;
			__totalHeight = (int)(ratio * dh);
			__bottomY = (200 - __totalHeight) / 2;
		}
		else
		{
			ratio = 200 / dh;
			__totalHeight = 200;
			__bottomY = 0;
			__totalWidth = (int)(ratio * dw);
			__leftX = (200 - __totalWidth) / 2;
		}
	/*
	System.out.println("<< W/H: " + __totalWidth + "  " + __totalHeight);
	System.out.println("<< L/B: " + __leftX + "  " + __bottomY);	
	*/
	}

	/// <summary>
	/// Sets the nodes array to use. </summary>
	/// <param name="nodes"> the nodes array to use. </param>
	public virtual void setNodesArray(HydrologyNode[] nodes)
	{
		//TODO SAM 2007-03-01 Evaluate use
		//__nodes = nodes;
	}

	/// <summary>
	/// Sets up the double buffer to be used when drawing.
	/// </summary>
	public virtual void setupDoubleBuffering()
	{
		setupDoubleBuffer(0, 0, getBounds().width, getBounds().height);
		forceRepaint();
	}

	}

}