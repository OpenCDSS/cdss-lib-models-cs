using System;
using System.Collections.Generic;

// StateMod_Network_JComponent - this class draws the network that can be printed, viewed and altered.

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

// TODO SAM 2011-07-07 This class is too complicated
// Need to isolate UI interactions in a separate class or at least make less intermingled here
// Need to have all setup calculations done in one place (e.g., height/width/scale) rather than
// throughout the code.  This will allow the logic to be simpler and likely also will reduce the amount
// of code.  Changes have occurred recently to ensure that printing and saving images occurs without interfering
// with the network objects in the interactive editor.  Current behavior is as follows:
// * Interactive drawing uses legacy code, but the code has checks for printing etc.  This is because the
//   previous design did all the work in-line whereas the current design splits printing into a setup step
//   with calls to the basic rendering methods.
// * Printing the entire network - occurs with a call to printNetwork() which for new printing calls print() -
//   a simpler sequence of steps occurs, bypassing the interactive code in paint().
// * Printing the partial network - currently is not supported (may or may not work) - this needs revisited.
//   Ideally this could work like printing the full network, but set the imageable area to the selected paper
//   size and the data limits to a subset of the network.
// * Saving the entire network to an image file - to be implemented ASAP - do similar to printing the entire network
//   but render to a buffer and then save the buffer
// * Saving the visible network to an image file - need to do something similar to TSTool where the active buffer
//   is saved - this may need rework in the future if batch image saving is implemented in StateDMI and a visible
//   window/buffer cannot be dumped
//
// TODO SAM 2011-07-09 The font size and node size are documented to be in points, but in the code they
// are sometimes treated as pixels.  Since 72 points = 1 inch and many screens are 72+ DPI, this is not a
// horrible error but does lead to the screen output looking different that printed.  Need to scrub the code
// and make sure that configuration information in points is converted accurately to pixels.  The configuration
// should be in absolute coordinates (points) rather than pixels, which can vary between devices.  Making the
// change may require changing data sets to use a different font and node size... so wait for now.
//
// TODO SAM 2011-07-09 Need to figure out how to set the overall network limits... can't just use node
// data because a new network with one node is a singularity.
// ----------------------------------------------------------------------------
// StateMod_Network_JComponent - class to control drawing of the network
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
//
// 2004-03-16	J. Thomas Sapienza, RTi	Initial version.  
// 2004-03-17 - 2004-03-22	JTS,RTi	Much more work getting a cleaner-
//					working version. 
// 2004-03-23	JTS, RTi		Javadoc'd.
// 2004-06-30	JTS, RTi		Corrected Java bug caused by zooming
//					out really far with antialiasing on.
//					Antialiasing is now only in effect
//					if the zoom is 100% or greater.
// 2004-07-07	JTS, RTi		Added printNetworkInfo().
// 2004-07-12	JTS, RTi		* Added annotations.
//					* Added links.
//					* Added capability to find nodes.
// 2004-10-20	JTS, RTi		Added a black border that is drawn
//					around the network in the GUI window.
// 2004-10-21	JTS, RTi		* Added __legendLimitsDetermined
//					  in order to know when the legend
//					  limits have been calculated the first
//					  time.
//					* The legend is now positioned initially
//					  (if it has never been positioned in
//					  a network before) 5% of the total
//					  network width from the left and 5% of
//					  the total network height from the
//					  bottom.
// 2004-11-11	JTS, RTi		* The margin is now drawn by default and
//					  automatically turned off when 
//					  printing, unless in testing mode.
//					* Antialiasing is turned on in printing.
//					* Corrected a bug that was causing 
//					  added nodes to not be able to
//					  be clicked on in order to select them.
// 2004-11-15	JTS, RTi		* When zooming out fully, the area 
//					  outside the network is now drawn in 
//					  grey.
//					* Downstream xconfluence nodes are
//					  now connected to their upstream nodes
//					  with dotted lines.
// 2005-04-08	JTS, RTi		saveXML() now takes a parameter that 
//					will be used to fill in the JFileChooser
//					filename, if not null.
// 2005-04-19	JTS, RTi		Added ability to save the network as
//					list files.
// 2005-05-23	JTS, RTi		Modified the line-dashing for 
//					XConfluence nodes so that even as line
//					widths are scaled up for various zoom
//					levels, the dashing remains looking
//					good.
// 2005-06-01	JTS, RTi		Added the ability to drag multiple nodes
//					simultaneously.
// 2005-11-21	JTS, RTi		Renaming a node connected to by a link
//					was throwing an exception, so the link
//					information was adjusted to keep up
//					with nodes that are renamed.
// 2005-12-20	JTS, RTi		The above fix introduced a new bug
//					where __links was not being checked to
//					make sure it is non-null.  This variable
//					is now checked in all methods for null.
// 2006-01-03	SAM, RTi		Fix problem with writing lists from the
//					network.
// 2006-01-04	JTS, RTi		Added separator to popup menu after
//					"Find Node".
// 2006-03-07	JTS, RTi		* Added finalize().
//					* Add and delete node popup menu items
//					  now are disabled if running in
//					  StateModGUI.
// 2007-03-01	SAM, RTi		Clean up code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace DWR.StateMod
{


	using HydrologyNode = cdss.domain.hydrology.network.HydrologyNode;

	using GRArrowStyleType = RTi.GR.GRArrowStyleType;
	using GRAspect = RTi.GR.GRAspect;
	using GRColor = RTi.GR.GRColor;
	using GRDrawingAreaUtil = RTi.GR.GRDrawingAreaUtil;
	using GRJComponentDevice = RTi.GR.GRJComponentDevice;
	using GRJComponentDrawingArea = RTi.GR.GRJComponentDrawingArea;
	using GRLimits = RTi.GR.GRLimits;
	using GRText = RTi.GR.GRText;
	using GRUnits = RTi.GR.GRUnits;
	using GRLineStyleType = RTi.GR.GRLineStyleType;
	using JComboBoxResponseJDialog = RTi.Util.GUI.JComboBoxResponseJDialog;
	using JFileChooserFactory = RTi.Util.GUI.JFileChooserFactory;
	using JGUIUtil = RTi.Util.GUI.JGUIUtil;
	using ResponseJDialog = RTi.Util.GUI.ResponseJDialog;
	using SimpleFileFilter = RTi.Util.GUI.SimpleFileFilter;
	using TextResponseJDialog = RTi.Util.GUI.TextResponseJDialog;
	using GraphicsPrinterJob = RTi.Util.IO.GraphicsPrinterJob;
	using IOUtil = RTi.Util.IO.IOUtil;
	using PrintUtil = RTi.Util.IO.PrintUtil;
	using PropList = RTi.Util.IO.PropList;
	using MathUtil = RTi.Util.Math.MathUtil;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// This class draws the network that can be printed, viewed and altered.
	/// There are 3 main ways of drawing the network:
	/// <ol>
	/// <li>	Drawing to the screen - uses double buffering for good visual.</li>
	/// <li>	Printing - prints to a graphics object from the printer.</li>
	/// <li>	Saving an image file - specify the image size and area will be scaled to fit.</li>
	/// </ol>
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class StateMod_Network_JComponent extends RTi.GR.GRJComponentDevice implements java.awt.event.ActionListener, java.awt.event.KeyListener, java.awt.event.MouseListener, java.awt.event.MouseMotionListener, java.awt.print.Printable
	public class StateMod_Network_JComponent : GRJComponentDevice, ActionListener, KeyListener, MouseListener, MouseMotionListener, Printable
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			__antiAlias = __antiAliasSetting;
		}


	/// <summary>
	/// Strings for menu items.
	/// </summary>
	private readonly string __MENU_ADD_ANNOTATION = "Add Annotation", __MENU_ADD_LINK = "Add Link", __MENU_ADD_NODE = "Add Upstream Node", __MENU_DELETE_ANNOTATION = "Delete Annotation", __MENU_DELETE_LINK = "Delete Link for Node", __MENU_DELETE_NODE = "Delete Node", __MENU_DRAW_NODE_LABELS = "Draw Text", __MENU_EDITABLE = "Editable", __MENU_FIND_NODE = "Find Node", __MENU_FIND_ANNOTATION = "Find Annotation", __MENU_INCH_GRID = "Show Half-Inch Grid", __MENU_MARGIN = "Show Margins", __MENU_PIXEL_GRID = "Show 25 Pixel Grid", __MENU_PRINT_NETWORK = "Print Entire Network", __MENU_PRINT_SCREEN = "Print Screen", __MENU_PROPERTIES = "Properties", __MENU_REFRESH = "Refresh", __MENU_SAVE_NETWORK = "Save Network as Image", __MENU_SAVE_SCREEN = "Save Screen as Image", __MENU_SAVE_XML = "Save XML Network File", __MENU_SHADED_RIVERS = "Shaded Rivers", __MENU_SNAP_TO_GRID = "Snap to Grid";

	/// <summary>
	/// Modes that the network can be placed in for responding to mouse presses.
	/// </summary>
	public const int MODE_PAN = 0, MODE_SELECT = 1;

	/// <summary>
	/// Whether legacy printing code should be used.  This allows old code to be kept in-line until new code is tested.
	/// </summary>
	private bool __useOldPrinting = false;

	/// <summary>
	/// Whether the annotations read in from an XML file were processed yet.
	/// </summary>
	private bool __annotationsProcessed = false;

	/// <summary>
	/// The default setting for anti-aliasing.  
	/// </summary>
	private bool __antiAliasSetting = true;

	/// <summary>
	/// Whether the drawing code should be anti-aliased or not.
	/// </summary>
	private bool __antiAlias;

	/// <summary>
	/// Whether to draw the grid or not.
	/// </summary>
	private bool __drawInchGrid = false;

	/// <summary>
	/// Whether the mouse drag is currently drawing a box around nodes or not.
	/// </summary>
	private bool __drawingBox = false;

	/// <summary>
	/// Whether to draw the printable area margin or not.
	/// </summary>
	private bool __drawMargin = true;

	/// <summary>
	/// Whether to draw the labels on the nodes in the network.
	/// </summary>
	private bool __drawNodeLabels = true;

	/// <summary>
	/// Whether to draw a 50-pixel grid or not.
	/// </summary>
	private bool __drawPixelGrid = false;

	/// <summary>
	/// Whether anything on the network can be changed or not.
	/// </summary>
	private bool __editable = true;

	/// <summary>
	/// Whether the legend should not be drawn on the next draw (because it is being dragged).
	/// </summary>
	private bool __eraseLegend = false;

	/// <summary>
	/// Whether the width was fit to be the max dimension when viewing the network
	/// initially.  There is no matching "fitHeight" variable -- it is assumed that
	/// if __fitWidth is false, then the network was fit to the paper height, instead.
	/// </summary>
	private bool __fitWidth = false;

	/// <summary>
	/// Whether to force paint() to refresh the entire drawing area or not.  Must be
	/// initially 'true' when the class is created.
	/// </summary>
	private bool __forceRefresh = true;

	/// <summary>
	/// If true, then any calls to repaint the canvas will be ignored.
	/// </summary>
	private bool __ignoreRepaint = false;

	/// <summary>
	/// Whether drawing settings need to be initialized because it is the first
	/// time paint() has been called.  This should be false when the class is instantiated.
	/// </summary>
	private bool __initialized = false;

	/// <summary>
	/// Whether the last thing clicked on in the network was an annotation or not.
	/// If false, then a node was the last object selected.
	/// </summary>
	private bool __isLastSelectedAnAnnotation = false;

	/// <summary>
	/// Whether the legend is being dragged.
	/// </summary>
	private bool __legendDrag = false;

	/// <summary>
	/// Whether or not the legend limits have been calculated for the first time.
	/// </summary>
	private bool __legendLimitsDetermined = false;

	/// <summary>
	/// Keeps track of whether the network has been changed by adding or deleting
	/// a node or changing network properties since the last save or file open.  Use getIsDirty()
	/// to determine if the main network or any of the node properties have changed.
	/// TODO SAM 2008-12-11 Why isn't this information stored in the network object instead of this component?
	/// </summary>
	private bool __networkChanged = false;

	/// <summary>
	/// Whether a node is currently being dragged on the screen.  This is true also
	/// if an annotation node is being dragged on the screen.
	/// </summary>
	private bool __nodeDrag = false;

	/// <summary>
	/// Whether the entire network is currently being printed.
	/// </summary>
	private bool __printingNetwork = false;

	/// <summary>
	/// Whether only the current screen is currently being printed.
	/// </summary>
	private bool __printingScreen = false;

	/// <summary>
	/// Whether annotations were read from a file and will need processed before being drawn.
	/// </summary>
	private bool __processAnnotations = false;

	/// <summary>
	/// Whether the entire network image is currently being saved.
	/// </summary>
	private bool __savingNetwork = false;

	/// <summary>
	/// Whether only the current screen image is currently being saved.
	/// </summary>
	private bool __savingScreen = false;

	/// <summary>
	/// Whether the screen position is currently being dragged.
	/// </summary>
	private bool __screenDrag = false;

	/// <summary>
	/// Whether rivers should be shaded.
	/// </summary>
	private bool __shadedRivers = false;

	/// <summary>
	/// Whether to draw a red bounding box around annotation text.  Useful for debugging.
	/// </summary>
	private bool __showAnnotationBoundingBox = false;

	/// <summary>
	/// Whether to snap node and legend movements to a grid.
	/// </summary>
	private bool __snapToGrid = false;

	/// <summary>
	/// A buffered image to use for rendering what will be printed or saved to an image file.
	/// </summary>
	private BufferedImage __tempBuffer = null;

	/// <summary>
	/// The array of X and Y positions for nodes being dragged.
	/// </summary>
	private double[] __draggedNodesXs = null, __draggedNodesYs = null;

	/// <summary>
	/// The current node size (as drawn for the current scale), in data units.
	/// </summary>
	private double __currNodeSize = 20;

	// TODO SAM 2010-12-29 Evaluate how this can be rectified with __dataLimitsMax
	/// <summary>
	/// The absolute Bottom Y of the data limits.
	/// TODO SAM 2011-07-09 Why not use getDrawingArea().getDataLimits().getBottomY() to avoid redundant data?
	/// </summary>
	private double __dataBottomY;

	/// <summary>
	/// The absolute Left X of the data limits.
	/// TODO SAM 2011-07-09 Why not use getDrawingArea().getDataLimits().getLeftX() to avoid redundant data?
	/// </summary>
	private double __dataLeftX;

	/// <summary>
	/// Values to keep track of where a mouse drag started and where it is now
	/// (TODO SAM 2011-07-09, in screen coordinates?).
	/// </summary>
	private double __currDragX, __currDragY, __dragStartX, __dragStartY;

	/// <summary>
	/// The amount to adjust the x and y values by when panning.  It is calculated
	/// whenever the zoom level changes, and then applied to the actual amount of 
	/// mouse dx and dy to see how far the screen should move during a pan.
	/// </summary>
	private double __DX = 1, __DY = 1;

	/// <summary>
	/// The size of the grid that is both drawn and snapped-to.
	/// </summary>
	private double __gridStep = 25;

	/// <summary>
	/// The height of the drawing area, in pixels.  Used for determining when the screen has been resized.
	/// </summary>
	private double __drawingAreaHeight = 0;

	/// <summary>
	/// The width of the drawing area, in pixels.  Used for determining when the screen has been resized.
	/// </summary>
	private double __drawingAreaWidth = 0;

	/// <summary>
	/// Used when reading in an XML network to hold the print node size so it can be
	/// applied after the graphics have been initialized.
	/// </summary>
	private double __holdPrintNodeSize = -1;

	/// <summary>
	/// The size of the nodes drawn in the legend, in device units (e.g., pixels for screen drawing).
	/// </summary>
	private double __legendNodeDiameter = 20;

	/// <summary>
	/// The maximum reach level in the network.
	/// </summary>
	private double __maxReachLevel = 0;

	/// <summary>
	/// The X and Y location of the last mouse press in data values. 
	/// </summary>
	private double __mouseDataX, __mouseDataY;

	/// <summary>
	/// The X and Y location of the last mouse press in device values.
	/// </summary>
	private double __mouseDownX = 0, __mouseDownY = 0;

	/// <summary>
	/// The X and Y locations where the last popup menu was opened, in device terms.
	/// </summary>
	private double __popupX = 0, __popupY = 0;

	/// <summary>
	/// The printing scale factor of the drawing.  This is the amount by which the
	/// 72 dpi printable pixels are scaled.  A printing scale value of 1 means that
	/// the network will be printed at 72 pixels per inch (ppi), which is the 
	/// Java standard.  A scale factor of .5 means that the network will be 
	/// printed at 144 ppi.  A scale factor of 3 means that the network will be printed at 24 ppi.
	/// </summary>
	private double __printScale = 1;

	/// <summary>
	/// The Y value (in data units) of the bottom of the screen.
	/// </summary>
	private double __screenBottomY;

	/// <summary>
	/// The current height of the data limits displayed on screen.  May be much larger
	/// or smaller than the actual data limits of the network, due to zooming.
	/// </summary>
	private double __screenDataHeight;

	/// <summary>
	/// The X value (in data units) of the left of the screen.
	/// </summary>
	private double __screenLeftX;

	/// <summary>
	/// The current width of the data limits displayed on screen.  May be much larger
	/// or smaller than the actual data limits of the network, due to zooming.
	/// </summary>
	private double __screenDataWidth;

	/// <summary>
	/// The total data height necessary to draw the entire network on the paper.  If
	/// the network is taller than the paper, then the height is the height of the entire network.
	/// TODO SAM 2011-07-09 Why not use getDrawingArea().getDataLimits().getHeight() to avoid redundant data?
	/// </summary>
	private double __totalDataHeight;

	/// <summary>
	/// The total data width necessary to draw the entire network on the paper.  If
	/// the network is wider than the paper, then the width is the width of the entire network.
	/// TODO SAM 2011-07-09 Why not use getDrawingArea().getDataLimits().getWidth() to avoid redundant data?
	/// </summary>
	private double __totalDataWidth;

	/// <summary>
	/// The difference between where the mouse was pressed on a node or legend to 
	/// drag it, and the far bottom left corner of the node or legend.
	/// </summary>
	private double __xAdjust, __yAdjust;

	/// <summary>
	/// The amount that the current zoom level is at.  100 is equal to a 1:1 zoom, where
	/// things are sized the same on paper that they are on screen.
	/// </summary>
	private double __zoomPercentage = 0;

	/// <summary>
	/// The dash pattern that is used when drawing grids and debugging lines on the screen.
	/// </summary>
	private float[] __dashes = new float[] {3, 5f};

	/// <summary>
	/// The dash pattern that is used when drawing grids and debugging lines on the screen.
	/// </summary>
	private float[] __dots = new float[] {3, 3f};

	/// <summary>
	/// The graphics context that should be used for drawing to the temporary BufferedImage for printing.
	/// </summary>
	private Graphics2D __bufferGraphics = null;

	/// <summary>
	/// The drawing area on which the network is drawn, which corresponds to the chosen media (or visible screen area).
	/// </summary>
	private GRJComponentDrawingArea __drawingArea = null;

	/// <summary>
	/// The drawing area used if rendering were to the full scale media.  This is used only to store the drawing and
	/// data limits at full scale so that the current rendering scale can be computed.
	/// </summary>
	private GRJComponentDrawingArea __drawingAreaFullScale = null;

	/// <summary>
	/// The array of limits of the nodes being dragged.
	/// </summary>
	private GRLimits[] __dragNodesLimits = null;

	// TODO SAM 2011-07-05 This should not be needed but there are some circular references that need to be unwound
	// Is this the data limits of actual data before calculateDataLimits() adjusts to what should be used in the
	// drawing area?  One is data and one is media?
	/// <summary>
	/// Backup of the data limits for the visible network.  Stored here so as to 
	/// avoid lots of calls to __drawingArea.getDataLimits();
	/// </summary>
	private GRLimits __dataLimits;

	/// <summary>
	/// Backup of the data limits for the network as initially opened, for zoom out.
	/// </summary>
	private GRLimits __dataLimitsMax;

	/// <summary>
	/// The limits of the node being dragged.
	/// </summary>
	private GRLimits __draggedNodeLimits;

	/// <summary>
	/// Temporary limits used while printing.  Declared here so as to avoid creating
	/// one object every time paint() is called, even when not printing.  Limits used
	/// to store the data limits during a drag when they're changed.
	/// </summary>
	private GRLimits __holdLimits;

	/// <summary>
	/// The limits of the legend, in data units.
	/// </summary>
	private GRLimits __legendDataLimits = new GRLimits(0, 0, 0, 0);

	/// <summary>
	/// Array of all the nodes in the node network.  Stored here for quick access,
	/// rather than iterating through the network.  This array does NOT contain
	/// annotations, even though some methods refer to nodes generically and process
	/// network nodes and annotations.
	/// </summary>
	private HydrologyNode[] __nodes;

	/// <summary>
	/// The node network read in from a makenet file.
	/// </summary>
	private StateMod_NodeNetwork __network;

	/// <summary>
	/// The array of nodes being dragged.  This will never be null.
	/// </summary>
	private int[] __draggedNodes = new int[0];

	/// <summary>
	/// The node that was last clicked on.
	/// </summary>
	private int __clickedNodeNum = -1;

	/// <summary>
	/// The dpi of the screen or paper.  System-dependent.
	/// </summary>
	private int __dpi = 0;

	/// <summary>
	/// The height in pixels that the node label font should be on the screen, for the current scale, using __dpi
	/// to convert between pixels and points.
	/// </summary>
	private int __fontSizePixels = 10;

	/// <summary>
	/// The height in points that the font should be on the screen, for the current scale.
	/// </summary>
	private int __fontSizePoints;

	/// <summary>
	/// Used when reading in an XML network to hold certain values so they can be
	/// applied after the graphics have been initialized.
	/// </summary>
	private int __holdPrintFontSize = -1;

	/// <summary>
	/// The thickness to scale 1-pixel-wide lines to during zooming in.
	/// </summary>
	private int __lineThickness = 1;

	/// <summary>
	/// The last mouse click positions in pixels.
	/// </summary>
	private int __mouseDeviceX = 0, __mouseDeviceY = 0;

	/// <summary>
	/// The mode the component is in in regard to how it responds to mouse presses.
	/// </summary>
	private int __networkMouseMode = MODE_PAN;

	/// <summary>
	/// The node size as set by the GUI at 1:1 zoom, in pixels.
	/// </summary>
	private int __nodeSizeFullScale = 20;

	/// <summary>
	/// The number of the node that had a popup menu opened on it.
	/// </summary>
	private int __popupNodeNum = -1;

	/// <summary>
	/// The number of times print(Graphics...) has been called so far per print.  
	/// It gets called three times per print (a Java foible), and 
	/// redrawing the network for the print each time is inefficient.
	/// </summary>
	private int __printCount = 0;

	/// <summary>
	/// The size in pixels that the printed font should be, for full scale layout paper size.
	/// The __printFontSizePoints is converted to pixels using the screen __dpi.
	/// </summary>
	private int __printFontSizePixels = 10;

	/// <summary>
	/// The size in points that the printed font should be, for full scale layout paper size, determined
	/// from the layout "NodeLabelFontSize".
	/// </summary>
	private int __printFontSizePoints;

	/// <summary>
	/// The thickness that lines should be printed at.
	/// </summary>
	private int __printLineThickness = 1;

	/// <summary>
	/// TODO SAM 2011-07-09 Evaluate whether to make the properties a class.
	/// Selected page layout properties (either from network being edited or batch job).
	/// </summary>
	private PropList __selectedPageLayoutPropList = null;

	/// <summary>
	/// The total height of the screen buffer.
	/// </summary>
	private int __totalBufferHeight;

	/// <summary>
	/// The total width of the screen buffer.
	/// </summary>
	private int __totalBufferWidth;

	/// <summary>
	/// The current position within the undo Vector.  The change operation at 
	/// position __undoPos - 1 is the one that last happened.  If __undoPos is 
	/// less than __undoOperations.size(), then redo can be done.  If __undoPos is 0 then no undo can be done.
	/// </summary>
	private int __undoPos = 0;

	/// <summary>
	/// Popup JMenuItems
	/// </summary>
	private JMenuItem __deleteLinkMenuItem = null, __addNodeMenuItem = null, __deleteNodeMenuItem = null;

	/// <summary>
	/// The popup menu that appears when an annotation is right-clicked on.
	/// </summary>
	private JPopupMenu __annotationPopup = null;

	/// <summary>
	/// The popup menu that appears when a node is right-clicked on.
	/// </summary>
	private JPopupMenu __nodePopup = null;

	/// <summary>
	/// The popup menu that appears when the pane is right-clicked on.
	/// </summary>
	private JPopupMenu __networkPopup;

	/// <summary>
	/// The pageformat to use for printing the network.
	/// </summary>
	private PageFormat __pageFormat;

	/// <summary>
	/// The parent JFrame in which this is displayed.
	/// </summary>
	private StateMod_Network_JFrame __parent;

	/// <summary>
	/// The reference window that is displayed along with this display.
	/// </summary>
	private StateMod_NetworkReference_JComponent __referenceJComponent;

	/// <summary>
	/// Used when reading in an XML network to hold certain values so they can be
	/// applied after the graphics have been initialized.
	/// </summary>
	private string __holdPaperOrientation = null, __holdPaperSize = null;

	// FIXME SAM 2008-01-25 Need to change so annotations are not same object type normal nodes.
	/// <summary>
	/// List of all the annotations displayed on the network.  Note that internally
	/// annotations are managed as a list of HydrologyNode.
	/// </summary>
	private IList<HydrologyNode> __annotations = new List<HydrologyNode>();

	/// <summary>
	/// StateMod_Network_AnnotationRenderers to display extra information as annotations on the map.
	/// Note that these are complex annotations whereas the __annotations list contains simple lines and shapes.
	/// </summary>
	private IList<StateMod_Network_AnnotationData> __annotationDataList = new List<StateMod_Network_AnnotationData>();

	/// <summary>
	/// List of all the links drawn on the network.  Each link is managed as a PropList with link properties.
	/// See the HydrologyNodeNetwork.writeXML() method for supported properties.
	/// </summary>
	private IList<PropList> __links = new List<PropList>();

	/// <summary>
	/// List to hold change operations.
	/// </summary>
	private IList<StateMod_Network_UndoData> __undoOperations = new List<StateMod_Network_UndoData>();

	/// <summary>
	/// Constructor used for headless operations, in particular printing.
	/// No wrapping JFrame interactions are supported.
	/// </summary>
	public StateMod_Network_JComponent(StateMod_NodeNetwork net, string requestedPageLayout) : base("StateMod_Network_JComponent")
	{
			if (!InstanceFieldsInitialized)
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		// Set the network for use elsewhere in class (calculate
		setNetwork(net, false, true); // determine secondary lists such as a linear array of nodes, to streamline processing

		// A scale of anything less than 1 seems to make it illegible.
		// __printScale = scale;
		__printScale = 1;

		// Determine the system-dependent DPI for the monitor
		Toolkit t = Toolkit.getDefaultToolkit();
		__dpi = t.getScreenResolution();

		// Setup the initial drawing areas and other information based on the layout.  Final setup will
		// occur in the print() method based on the chosen printer and paper size
		// First tell the class that printing is occurring and set some data
		initializeForPrinting();
		// Set the page size, etc. from the requested layout (defaults for printing/layout
		// but may be modified when print() gets called from the printJob
		initializeForNetworkPageLayout(requestedPageLayout);
	}

	/// <summary>
	/// Constructor. </summary>
	/// <param name="parent"> the JFrame in which this component appears. </param>
	/// <param name="scale"> the scale to use for determining how the network should be drawn
	/// for printing.  Printing is done at 72 dpi by default.  A printing scale of
	/// .5 would mean that printed output would be rendered at 144 dpi, and .25 would
	/// mean at 288 dpi.  Since each node is 20 pixels across, a scale of .25 seems to work best. </param>
	public StateMod_Network_JComponent(StateMod_Network_JFrame parent, double scale) : base("StateMod_Network_JComponent")
	{
			if (!InstanceFieldsInitialized)
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}

		__parent = parent;
		// A scale of anything less than 1 seems to make it illegible.
		// __printScale = scale;
		__printScale = 1;

		// determine the system-dependent DPI for the monitor
		Toolkit t = Toolkit.getDefaultToolkit();
		__dpi = t.getScreenResolution();

		// make sure this class listens to itself for certain events
		addKeyListener(this);
		addMouseListener(this);
		addMouseMotionListener(this);

		buildPopupMenus();

		// Set the default print font size for when the network is first displayed.
		// TODO (JTS - 2004-07-13) remove this call?
		setPrintFontSize(10);

		__undoOperations = new List<StateMod_Network_UndoData>();
	}

	/// <summary>
	/// Responds to action events. </summary>
	/// <param name="event"> the ActionEvent that happened. </param>
	public virtual void actionPerformed(ActionEvent @event)
	{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		string routine = this.GetType().FullName + "actionPerformed";
		string action = @event.getActionCommand();

		if (action.Equals(__MENU_ADD_ANNOTATION))
		{
			if (!__editable)
			{
				return;
			}
			addAnnotation(__popupX, __popupY);
			setNetworkChanged(true);
		}
		else if (action.Equals(__MENU_ADD_LINK))
		{
			new StateMod_Network_AddLink_JDialog(__parent, this, __nodes);
			setNetworkChanged(true);
		}
		else if (action.Equals(__MENU_ADD_NODE))
		{
			if (!__editable)
			{
				return;
			}
			new StateMod_Network_AddNode_JDialog(__parent, __nodes[__popupNodeNum]);
			// TODO (JTS - 2004-07-13) need a way to make sure that a node was really added 
			// and mark changed appropriately
			setNetworkChanged(true);
		}
		else if (action.Equals(__MENU_DELETE_ANNOTATION))
		{
			__annotations.RemoveAt(__popupNodeNum);
			setNetworkChanged(true);
			forceRepaint();
		}
		else if (action.Equals(__MENU_DELETE_LINK))
		{
			deleteLink();
		}
		else if (action.Equals(__MENU_DELETE_NODE))
		{
			if (!__editable)
			{
				return;
			}
			if (__popupNodeNum == -1)
			{
				return;
			}
			if (__isLastSelectedAnAnnotation)
			{
				__annotations.RemoveAt(__clickedNodeNum);
			}
			else
			{
				string id = __nodes[__popupNodeNum].getCommonID();
				removeIDFromLinks(id);
				__network.deleteNode(id);
				buildNodeArray();
				findMaxReachLevel();
				__undoOperations = new List<StateMod_Network_UndoData>();
				__parent.setUndo(false);
				__parent.setRedo(false);
			}
			setNetworkChanged(true);
			forceRepaint();
		}
		else if (action.Equals(__MENU_DRAW_NODE_LABELS))
		{
			if (__drawNodeLabels)
			{
				__drawNodeLabels = false;
			}
			else
			{
				__drawNodeLabels = true;
			}
			HydrologyNode.setDrawText(__drawNodeLabels);
			forceRepaint();
		}
		else if (action.Equals(__MENU_EDITABLE))
		{
			if (__editable)
			{
				__editable = false;
			}
			else
			{
				__editable = true;
			}
		}
		else if (action.Equals(__MENU_FIND_ANNOTATION))
		{
			findAnnotation();
		}
		else if (action.Equals(__MENU_FIND_NODE))
		{
			findNode();
		}
		else if (action.Equals(__MENU_INCH_GRID))
		{
			if (__drawInchGrid)
			{
				__drawInchGrid = false;
			}
			else
			{
				__drawInchGrid = true;
			}
			forceRepaint();
			__referenceJComponent.setDrawInchGrid(__drawInchGrid);
		}
		else if (action.Equals(__MENU_MARGIN))
		{
			if (__drawMargin)
			{
				__drawMargin = false;
			}
			else
			{
				__drawMargin = true;
			}
			forceRepaint();
			__referenceJComponent.setDrawMargin(__drawMargin);
		}
		else if (action.Equals(__MENU_PIXEL_GRID))
		{
			if (__drawPixelGrid)
			{
				__drawPixelGrid = false;
			}
			else
			{
				__drawPixelGrid = true;
			}
			forceRepaint();
		}
		else if (action.Equals(__MENU_PRINT_NETWORK))
		{
			printNetwork();
		}
		else if (action.Equals(__MENU_PRINT_SCREEN))
		{
			printScreen();
		}
		else if (action.Equals(__MENU_PROPERTIES))
		{
			if (__isLastSelectedAnAnnotation)
			{
				HydrologyNode node = __annotations[__popupNodeNum];
				new StateMod_Network_AnnotationProperties_JDialog(this, __editable, node, __popupNodeNum);
			}
			else
			{
				string idPre = __nodes[__popupNodeNum].getCommonID();
				new StateMod_Network_NodeProperties_JDialog(__parent, __nodes, __popupNodeNum);
				string idPost = __nodes[__popupNodeNum].getCommonID();
				if (!idPre.Equals(idPost))
				{
					adjustLinksForNodeRename(idPre, idPost);
				}
			}
		}
		else if (action.Equals(__MENU_REFRESH))
		{
			forceRepaint();
		}
		else if (action.Equals(__MENU_SAVE_NETWORK))
		{
			saveNetworkAsImage();
	//		Message.printStatus(1, "", "Save: setDirty(false)");
			setDirty(false);
		}
		else if (action.Equals(__MENU_SAVE_SCREEN))
		{
			try
			{
				saveScreenAsImage();
			}
			catch (Exception e)
			{
				Message.printWarning(1,routine,"Error saving image file (" + e + ").");
			}
		}
		else if (action.Equals(__MENU_SAVE_XML))
		{
			saveXML(__parent.getFilename());
	//		Message.printStatus(1, "", "Save: setDirty(false)");
			setDirty(false);
		}
		else if (action.Equals(__MENU_SHADED_RIVERS))
		{
			if (__shadedRivers)
			{
				__shadedRivers = false;
			}
			else
			{
				__shadedRivers = true;
			}
			forceRepaint();
		}
		else if (action.Equals(__MENU_SNAP_TO_GRID))
		{
			if (__snapToGrid)
			{
				__snapToGrid = false;
			}
			else
			{
				__snapToGrid = true;
			}
		}
	}

	/// <summary>
	/// Adds an annotation node to the drawing area at the specified point. </summary>
	/// <param name="x"> the x location of the annotation </param>
	/// <param name="y"> the y location of the annotation </param>
	protected internal virtual void addAnnotation(double x, double y)
	{
		string text = (new TextResponseJDialog(__parent, "Enter the annotation text", "Enter the annotation text:", ResponseJDialog.OK | ResponseJDialog.CANCEL)).response();

		if (string.ReferenceEquals(text, null))
		{
			// text will be null if the user pressed cancel
			return;
		}

		// Escape all instances of " -- they can cause problems when put into a PropList.
		text = StringUtil.replaceString(text, "\"", "'");

		// Round off the X and Y values to 6 digits after the decimal point
		y = toSixDigits(y);
		x = toSixDigits(x);

		// Default most of the information in the node
		HydrologyNode node = new HydrologyNode();
		string position = "Center";
		string props = "ShapeType=Text;"
			+ "FontSize=11;"
			+ "OriginalFontSize=11;"
			+ "FontStyle=Plain;"
			+ "FontName=Helvetica;"
			+ "Point=" + x + ", " + y + ";"
			+ "Text=\"" + text + "\";"
			+ "TextPosition=" + position;
		PropList p = PropList.parse(props, "", ";");
		node.setAssociatedObject(p);
		GRLimits limits = GRDrawingAreaUtil.getTextExtents(__drawingArea, text, GRUnits.DEVICE);
		double w = convertDrawingXToDataX(limits.getWidth());
		double h = convertDrawingYToDataY(limits.getHeight());

		// Calculate the actual limits for the from the lower-left corner to the upper-right,
		// in order to know when the text has been clicked on (for dragging, or popup menus).

		if (position.Equals("UpperRight", StringComparison.OrdinalIgnoreCase))
		{
			node.setPosition(x, y, w, h);
		}
		else if (position.Equals("Right", StringComparison.OrdinalIgnoreCase))
		{
			node.setPosition(x, y - (h / 2), w, h);
		}
		else if (position.Equals("LowerRight", StringComparison.OrdinalIgnoreCase))
		{
			node.setPosition(x, y - h, w, h);
		}
		else if (position.Equals("Below", StringComparison.OrdinalIgnoreCase) || position.Equals("BelowCenter", StringComparison.OrdinalIgnoreCase))
		{
			node.setPosition(x - (w / 2), y - h, w, h);
		}
		else if (position.Equals("LowerLeft", StringComparison.OrdinalIgnoreCase))
		{
			node.setPosition(x - w, y - h, w, h);
		}
		else if (position.Equals("Left", StringComparison.OrdinalIgnoreCase))
		{
			node.setPosition(x - w, y - (h / 2), w, h);
		}
		else if (position.Equals("UpperLeft", StringComparison.OrdinalIgnoreCase))
		{
			node.setPosition(x - w, y, w, h);
		}
		else if (position.Equals("Above", StringComparison.OrdinalIgnoreCase) || position.Equals("AboveCenter", StringComparison.OrdinalIgnoreCase))
		{
			node.setPosition(x - (w / 2), y, w, h);
		}
		else if (position.Equals("Center", StringComparison.OrdinalIgnoreCase))
		{
			node.setPosition(x - (w / 2), y - (h / 2), w, h);
		}

		node.setDirty(true);
		__annotations.Add(node);
		forceRepaint();
	}

	/// <summary>
	/// Add an annotation renderer.  This allows generic objects to be drawn on top of the map, allowing
	/// rendering to occur by external code that is familiar with domain issues.  The StateMod_Network_JComponent
	/// is passed back to the renderer to allow full access to layer information, symbols, etc. </summary>
	/// <param name="renderer"> the renderer that will be called when it is time to draw the object </param>
	/// <param name="objectToRender"> the object to render (will be passed back to the renderer) </param>
	/// <param name="objectLabel"> label for the object, to list in the GeoViewJPanel </param>
	/// <param name="scrollToAnnotation"> if true, scroll to the annotation (without changing scale) </param>
	public virtual StateMod_Network_AnnotationData addAnnotationRenderer(StateMod_Network_AnnotationRenderer renderer, object objectToRender, string objectLabel, GRLimits limits)
	{
		// Only add if the annotation is not already in the list
		foreach (StateMod_Network_AnnotationData annotationData in __annotationDataList)
		{
			if ((annotationData.getObject() == objectToRender) && annotationData.getLabel().Equals(objectLabel, StringComparison.OrdinalIgnoreCase))
			{
				// Don't add again.
				return null;
			}
		}
		StateMod_Network_AnnotationData annotationData = new StateMod_Network_AnnotationData(renderer,objectToRender,objectLabel,limits);
		__annotationDataList.Add(annotationData);
		repaint();
		return annotationData;
	}

	/// <summary>
	/// Adds a link to the network. </summary>
	/// <param name="fromNodeId"> the ID of the node from which the link is drawn. </param>
	/// <param name="toNodeId"> the ID of the node to which the link is drawn. </param>
	protected internal virtual void addLink(string fromNodeId, string toNodeId, string linkId, string lineStyle, string fromArrowStyle, string toArrowStyle)
	{
		PropList p = new PropList("");
		p.set("ID", linkId);
		p.set("ShapeType", "Link");
		p.set("FromNodeID", fromNodeId);
		p.set("ToNodeID", toNodeId);
		p.set("LineStyle", lineStyle);
		p.set("FromArrowStyle", fromArrowStyle);
		p.set("ToArrowStyle", toArrowStyle);
		__links.Add(p);
		forceRepaint();
	}

	/// <summary>
	/// Adds a node to the network. </summary>
	/// <param name="name"> the name of the node. </param>
	/// <param name="type"> the type of the node. </param>
	/// <param name="upID"> the ID of the node immediately upstream. </param>
	/// <param name="downID"> the ID of the node immediately downstream. </param>
	/// <param name="isNaturalFlow"> whether the node is a natural flow node. </param>
	protected internal virtual void addNode(string name, int type, string upID, string downID, bool isNaturalFlow, bool isImport)
	{
		if (string.ReferenceEquals(upID, null) || upID.Equals("None"))
		{
			upID = null;
		}
		StateMod_NodeNetwork network = getNetwork();

		network.addNode(name, type, upID, downID, isNaturalFlow, isImport);
		// TODO (JTS - 2004-07-13) is there a more efficient way??
		setNetworkChanged(true);
		__parent.setNetwork(network, true, true);
		buildNodeArray();
		findMaxReachLevel();
		forceRepaint();
	}

	/// <summary>
	/// Adds a node change operation to the undo list. </summary>
	/// <param name="nodeNum"> the node that was moved. </param>
	/// <param name="x"> the new node X position. </param>
	/// <param name="y"> the new node Y position. </param>
	private void addNodeChangeOperation(int nodeNum, double x, double y)
	{
		// Create a new UndoData object to store the location of the node before and 
		StateMod_Network_UndoData data = new StateMod_Network_UndoData();
		data.nodeNum = nodeNum;
		if (__isLastSelectedAnAnnotation)
		{
			// TODO SAM 2008-01-25 Figure out if undo applies to annotations also
			//data.oldX = getAnnotationNode(nodeNum).getX();
			//data.oldY = getAnnotationNode(nodeNum).getY();
			return;
		}
		else
		{
			data.oldX = __nodes[nodeNum].getX();
			data.oldY = __nodes[nodeNum].getY();
		}
		data.newX = x;
		data.newY = y;

		addNodeChangeOperation(data);
	}

	/// <summary>
	/// Adds a change operation to the undo data.  This is called when a node is dragged
	/// so that the operation can be undone. </summary>
	/// <param name="data"> the UndoData detailing what happened in the operation. </param>
	private void addNodeChangeOperation(StateMod_Network_UndoData data)
	{
		// undoPos tracks the current position within the undo list and
		// is used to allow undos and redos.  If a user has made changes,
		// and then undoes them, and then makes a new change, the previous
		// undos are made unavailable and are lost.  The following gets
		// rid of undos that cannot be undone
		if (__undoPos < __undoOperations.Count)
		{
			while (__undoPos < __undoOperations.Count)
			{
				__undoOperations.RemoveAt(__undoPos);
			}
		}
		__undoOperations.Add(data);
		__undoPos = __undoOperations.Count;

		// If a new undo is added, then it is the last undo in the list.
		__parent.setUndo(true);
		__parent.setRedo(false);
	}

	/// <summary>
	/// Adjusts the ID values stored in the links Vector after a node has been renamed
	/// so that the links point to the same node (though it has a different ID). </summary>
	/// <param name="idPre"> the ID of the node before it was renamed. </param>
	/// <param name="idPost"> the ID of the node after it was renamed. </param>
	private void adjustLinksForNodeRename(string idPre, string idPost)
	{
		if (__links == null)
		{
			return;
		}

		int size = __links.Count;
		PropList p = null;
		string id1 = null;
		string id2 = null;
		for (int i = 0; i < size; i++)
		{
			p = (PropList)__links[i];
			id1 = p.getValue("FromNodeID");
			id2 = p.getValue("ToNodeID");

			if (id1.Equals(idPre))
			{
				p.setValue("FromNodeID", idPost);
			}
			else if (id2.Equals(idPre))
			{
				p.setValue("ToNodeID", idPost);
			}
		}
	}

	/// <summary>
	/// Adjust the current data height and width when the GUI screen size is changed, called from paint()
	/// when a resize has been detected.
	/// </summary>
	private void adjustForResize()
	{
		int width = getBounds().width;
		int height = getBounds().height;

		double wpct = (double)width / (double)__drawingAreaWidth;
		double hpct = (double)height / (double)__drawingAreaHeight;

		__screenDataWidth *= wpct;
		__screenDataHeight *= hpct;
	}

	/// <summary>
	/// From the nodes stored in the node network, this builds the array of nodes.
	/// Nodes are stored in an array for quicker traversal during node operations.
	/// </summary>
	private void buildNodeArray()
	{
		bool done = false;
		HydrologyNode node = __network.getMostUpstreamNode();
		HydrologyNode holdNode = null;
		IList<HydrologyNode> nodes = new List<HydrologyNode>();

		while (!done)
		{
			if (node == null)
			{
				done = true;
			}
			else
			{
				if (node.getType() == HydrologyNode.NODE_TYPE_END)
				{
					done = true;
				}
				if (node.getType() != HydrologyNode.NODE_TYPE_UNKNOWN)
				{
					nodes.Add(node);
				}
				holdNode = node;
				node = StateMod_NodeNetwork.getDownstreamNode(node, StateMod_NodeNetwork.POSITION_COMPUTATIONAL);

				if (node == holdNode)
				{
					done = true;
				}
			}
		}

		int size = nodes.Count;

		// Move the nodes from the list into an array for quicker traversal.

		__nodes = new HydrologyNode[size];

		for (int i = 0; i < size; i++)
		{
			__nodes[i] = (HydrologyNode)nodes[i];
			// FIXME SAM 2008-03-16 Need to remove WIS code
			//__nodes[i].setInWis(false);
			__nodes[i].setBoundsCalculated(false);

			double diam = 0;
			if (__fitWidth)
			{
				diam = convertDrawingXToDataX(__legendNodeDiameter);
			}
			else
			{
				diam = convertDrawingYToDataY(__legendNodeDiameter);
			}
			__nodes[i].setSymbol(null);
			__nodes[i].setBoundsCalculated(false);
			__nodes[i].setDataDiameter(diam);
			__nodes[i].calculateExtents(__drawingArea);
		}

		// Make sure that all the nodes have unique IDs.  If they don't,
		// checkUniqueID will generate a unique ID for the offending nodes.
		for (int i = size - 1; i > -1; i--)
		{
			checkUniqueID(i);
		}
	}

	/// <summary>
	/// Builds the GUI's popup menus.
	/// </summary>
	private void buildPopupMenus()
	{
		JMenuItem mi = null;
		JCheckBoxMenuItem jcbmi = null;
		__annotationPopup = new JPopupMenu();
		__networkPopup = new JPopupMenu();
		__nodePopup = new JPopupMenu();

		// Popup menu for when an annotation is clicked on
		__annotationPopup = new JPopupMenu();
		mi = new JMenuItem(__MENU_PROPERTIES);
		mi.addActionListener(this);
		__annotationPopup.add(mi);
		mi = new JMenuItem(__MENU_DELETE_ANNOTATION);
		mi.addActionListener(this);
		__annotationPopup.add(mi);

		// Popup menu for when the network window background is clicked on
		mi = new JMenuItem(__MENU_ADD_ANNOTATION);
		mi.addActionListener(this);
		__networkPopup.add(mi);
		mi = new JMenuItem(__MENU_ADD_LINK);
		mi.addActionListener(this);
		__networkPopup.add(mi);
		__networkPopup.addSeparator();
		// -------------------------
		mi = new JMenuItem(__MENU_FIND_NODE);
		mi.addActionListener(this);
		__networkPopup.add(mi);
		mi = new JMenuItem(__MENU_FIND_ANNOTATION);
		mi.addActionListener(this);
		__networkPopup.add(mi);
		__networkPopup.addSeparator();
		// -------------------------
		jcbmi = new JCheckBoxMenuItem(__MENU_SHADED_RIVERS);
		jcbmi.addActionListener(this);
		__networkPopup.add(jcbmi);
		jcbmi = new JCheckBoxMenuItem(__MENU_DRAW_NODE_LABELS, true);
		jcbmi.addActionListener(this);
		__networkPopup.add(jcbmi);
		jcbmi = new JCheckBoxMenuItem(__MENU_EDITABLE, true);
		jcbmi.addActionListener(this);
		__networkPopup.add(jcbmi);
		__networkPopup.addSeparator();
		// -------------------------
		jcbmi = new JCheckBoxMenuItem(__MENU_MARGIN);
		jcbmi.setSelected(true);
		jcbmi.addActionListener(this);
		__networkPopup.add(jcbmi);
		jcbmi = new JCheckBoxMenuItem(__MENU_INCH_GRID);
		jcbmi.addActionListener(this);
		__networkPopup.add(jcbmi);
		jcbmi = new JCheckBoxMenuItem(__MENU_SNAP_TO_GRID);
		jcbmi.addActionListener(this);
		__networkPopup.add(jcbmi);

		// Popup menu for when a node is clicked on
		__addNodeMenuItem = new JMenuItem(__MENU_ADD_NODE);
		__addNodeMenuItem.addActionListener(this);
		__nodePopup.add(__addNodeMenuItem);
		__deleteNodeMenuItem = new JMenuItem(__MENU_DELETE_NODE);
		__deleteNodeMenuItem.addActionListener(this);
		__nodePopup.add(__deleteNodeMenuItem);
		__deleteLinkMenuItem = new JMenuItem(__MENU_DELETE_LINK);
		__deleteLinkMenuItem.addActionListener(this);
		__nodePopup.add(__deleteLinkMenuItem);
		__deleteLinkMenuItem.setEnabled(false);
		__nodePopup.addSeparator();
		// -------------------------
		mi = new JMenuItem(__MENU_PROPERTIES);
		mi.addActionListener(this);
		__nodePopup.add(mi);
		__nodePopup.addSeparator();
		// -------------------------
		mi = new JMenuItem(__MENU_FIND_NODE);
		mi.addActionListener(this);
		__nodePopup.add(mi);
		mi = new JMenuItem(__MENU_FIND_ANNOTATION);
		mi.addActionListener(this);
		__nodePopup.add(mi);
		__nodePopup.addSeparator();
		jcbmi = new JCheckBoxMenuItem(__MENU_SHADED_RIVERS);
		jcbmi.addActionListener(this);
		__nodePopup.add(jcbmi);
		jcbmi = new JCheckBoxMenuItem(__MENU_DRAW_NODE_LABELS, true);
		jcbmi.addActionListener(this);
		__nodePopup.add(jcbmi);
		jcbmi = new JCheckBoxMenuItem(__MENU_EDITABLE, true);
		jcbmi.addActionListener(this);
		__nodePopup.add(jcbmi);
		__nodePopup.addSeparator();
		// -------------------------
		jcbmi = new JCheckBoxMenuItem(__MENU_MARGIN);
		jcbmi.setSelected(true);
		jcbmi.addActionListener(this);
		__nodePopup.add(jcbmi);
		jcbmi = new JCheckBoxMenuItem(__MENU_INCH_GRID);
		jcbmi.addActionListener(this);
		__nodePopup.add(jcbmi);
		jcbmi = new JCheckBoxMenuItem(__MENU_SNAP_TO_GRID);
		jcbmi.addActionListener(this);
		__nodePopup.add(jcbmi);
	}

	/// <summary>
	/// Builds limits and sets up the starting position for a drag for multiple nodes. 
	/// This is done prior to dragging starting.
	/// </summary>
	private void buildSelectedNodesLimits()
	{
		IList<int?> v = new List<int?>();

		// First get a list comprising the indices of the nodes in the
		// __nodes array that are being dragged.  This method is only called
		// if at least 1 node is selected, so the list will never be 0-size.
		for (int i = 0; i < __nodes.Length; i++)
		{
			if (!__nodes[i].isSelected())
			{
				continue;
			}

			if (i == __clickedNodeNum)
			{
				continue;
			}

			v.Add(new int?(i));
		}

		int? I = null;
		int num = 0;
		int size = v.Count;

		__dragNodesLimits = new GRLimits[size];
		__draggedNodes = new int[size];
		__draggedNodesXs = new double[size];
		__draggedNodesYs = new double[size];

		double[] p = null;

		// Go through the nodes that are selected and populate the arrays
		// created above with the respective nodes' limits and starting X and Y positions.  

		for (int i = 0; i < size; i++)
		{
			I = v[i];
			num = I.Value;

			__dragNodesLimits[i] = __nodes[num].getLimits();
			__draggedNodesXs[i] = __nodes[num].getX();
			__draggedNodesYs[i] = __nodes[num].getY();
			__draggedNodes[i] = num;

			// If snap to grid is on, the positions of the nodes to be 
			// dragged are shifted to the nearest grid points prior to dragging.
			if (__snapToGrid)
			{
				p = findNearestGridXY(__nodes[num].getX(), __nodes[num].getY());
				__draggedNodesXs[i] = p[0];
				__draggedNodesYs[i] = p[1];
			}
		}
	}

	/// <summary>
	/// Calculates the data limits necessary to make the entire network visible at
	/// first.  This method should be called after the drawing area is set in this 
	/// device, and before the data limits are set in the reference drawing area.
	/// It is also called when the paper orientation changes.
	/// </summary>
	protected internal virtual void calculateDataLimits()
	{
		GRLimits data = __drawingArea.getDataLimits();
		if (__dataLimits == null)
		{
			__dataLimits = data;
		}

		// Get the height and width of the data as set in the main JFrame
		// from what was read from the network.
		double dWidth = __dataLimits.getRightX() - __dataLimits.getLeftX();
		double dHeight = __dataLimits.getTopY() - __dataLimits.getBottomY();

		GRLimits setLimits = new GRLimits(__dataLimits);

		// Next, the ratio of the width to the height of the paper will be
		// checked and if the ratio is not sufficient to allow the data
		// to be all printed on the page (no compression!), the data limits
		// will be reset so that there is extra room (off paper) in the drawing area
		if (dWidth >= dHeight)
		{
			__fitWidth = true;
			__totalDataWidth = dWidth;
			__dataLeftX = __dataLimits.getLeftX();
			// size __totalDataHeight to be proportional to __totalDataWidth, given the screen proportions
			__totalDataHeight = ((double)__totalBufferHeight / (double)__totalBufferWidth) * __totalDataWidth;

			if (__dataLimits.getHeight() > __totalDataHeight)
			{
				// If the data limits are greater than the total
				// data height that can fit on the paper, then 
				// reset the total data height to be the height of the data limits.  
				__dataBottomY = __dataLimits.getBottomY();
				__totalDataHeight = __dataLimits.getHeight();
			}
			else
			{
				// Otherwise, if the height of the network can 
				// fit within the paper entirely, center the network vertically on the paper
				double diff = __totalDataHeight - __dataLimits.getHeight();
				__dataBottomY = __dataLimits.getBottomY() - (diff / 2);
				setLimits.setBottomY(__dataBottomY);
				setLimits.setTopY(setLimits.getBottomY() + __totalDataHeight);
			}
		}
		else
		{
			__fitWidth = false;
			__totalDataHeight = dHeight;
			__dataBottomY = __dataLimits.getBottomY();
			// size __totalDataWidth to be proportional to __totalDataHeight, given the screen proportions		
			__totalDataWidth = ((double)__totalBufferWidth / (double)__totalBufferHeight) * __totalDataHeight;
			if (__dataLimits.getWidth() > __totalDataWidth)
			{
				// if the data limits are greater than the total
				// data width that can fit on the paper, then 
				// reset the total data width to be the width of the data limits. 
				__dataLeftX = __dataLimits.getLeftX();
				__totalDataWidth = __dataLimits.getWidth();
			}
			else
			{
				// otherwise, if the width of the network can
				// fit within the paper entirely, center the network
				// horizontally on the paper.  This is when the network is zoomed out.
				double diff = __totalDataWidth - __dataLimits.getWidth();
				__dataLeftX = __dataLimits.getLeftX() - (diff / 2);
				setLimits.setLeftX(__dataLeftX);
				setLimits.setRightX(__dataLeftX + __totalDataWidth);
			}
		}
		__drawingArea.setDataLimits(setLimits);
		// TODO (JTS - 2004-07-13) why are the __dataLimits not reset here??
	}

	/// <summary>
	/// Calculates the data limits necessary to line up the corners of the drawing area with data values.
	/// This ensures that the requested "view window" is drawn with 1:1 aspect for the given media size.
	/// For example, for printing, this ensures that the network is centered and fits on the page.
	/// This method should be called after the drawing area is set in this device (the drawing limits determine the
	/// aspect ratio).
	/// TODO SAM 2011-07-08 This method was adapted from calculateDataLimits() for printing, but may be used
	/// for other drawing modes in the future. </summary>
	/// <param name="drawingLimits"> the drawing limits of the media (imageable area on paper or screen drawing area). </param>
	/// <param name="dataLimitsOrig"> the data limits of the network data, based on the nodes and legend (typically taken from
	/// the XMin,Ymin,Xmax,Ymax coordinates from the network. </param>
	/// <param name="buffer"> an additional buffer to be added to the data limits, for example to allow for labels, in data units.
	/// These values will be used to adjust the dataLimits before computations occur.  The buffer should not be confused
	/// with the margin, which is a non-printable area around the edge of the page.  When printing, the margin is
	/// taken out of the page size, resulting in the imageable area.  The buffer is an additional edge inside the
	/// printable area.  The order of the buffer values is left, right, top, bottom.  If the length of the array is 1,
	/// then the same value is added to all sides. </param>
	/// <param name="setInternalData"> if true, then internal variables used for rendering are set; if false, the calculations
	/// occur but are not set in the class; therefore false does not interfere with rendering </param>
	/// <returns> the new data limits considering the media - the data limits will align with the edges of the media. </returns>
	protected internal virtual GRLimits calculateDataLimitsForMedia(GRLimits drawingLimits, GRLimits dataLimitsOrig, double[] buffer, bool setInternalData)
	{ // Copy the data limits so as to not interfere with calling code
		GRLimits dataLimits = new GRLimits(dataLimitsOrig);
		// Add to the limits
		if (buffer != null)
		{
			if (buffer.Length == 1)
			{
				dataLimits.setLeftX(dataLimits.getLeftX() - buffer[0]);
				dataLimits.setRightX(dataLimits.getRightX() + buffer[0]);
				dataLimits.setTopY(dataLimits.getTopY() + buffer[0]);
				dataLimits.setBottomY(dataLimits.getBottomY() - buffer[0]);
			}
			else if (buffer.Length == 4)
			{
				dataLimits.setLeftX(dataLimits.getLeftX() - buffer[0]);
				dataLimits.setRightX(dataLimits.getRightX() + buffer[1]);
				dataLimits.setTopY(dataLimits.getTopY() + buffer[2]);
				dataLimits.setBottomY(dataLimits.getBottomY() - buffer[3]);
			}
		}
		GRLimits dataLimitsMedia = new GRLimits(dataLimits); // Copy because only one dimension will be adjusted

		// Next, the ratio of the width to the height of the media is
		// checked and if the ratio is not sufficient to allow the data
		// to be all printed on the page without distortion, the data limits
		// will be reset so that there is extra room in the drawing area for the "skinny" dimension
		bool fitWidth = false;
		double totalDataWidth = 0.0;
		double totalDataHeight = 0.0;
		double dataLeftX = 0.0;
		double dataBottomY = 0.0;
		if (dataLimits.getWidth() >= dataLimits.getHeight())
		{
			fitWidth = true;
			totalDataWidth = dataLimits.getWidth();
			dataLeftX = dataLimits.getLeftX();
			// size __totalDataHeight to be proportional to __totalDataWidth, given the media proportions
			totalDataHeight = ((double)drawingLimits.getHeight() / (double)drawingLimits.getWidth()) * totalDataWidth;

			if (dataLimits.getHeight() > totalDataHeight)
			{
				// If the data limits are greater than the total data height that can fit on the paper, then 
				// reset the total data height to be the height of the data limits.  
				dataBottomY = dataLimits.getBottomY();
				totalDataHeight = dataLimits.getHeight();
			}
			else
			{
				// Otherwise, if the height of the network can 
				// fit within the paper entirely, center the network vertically on the paper
				double diff = totalDataHeight - dataLimits.getHeight();
				dataBottomY = dataLimits.getBottomY() - (diff / 2);
				dataLimitsMedia.setBottomY(dataBottomY);
				dataLimitsMedia.setTopY(dataLimitsMedia.getBottomY() + totalDataHeight);
			}
		}
		else
		{
			fitWidth = false;
			totalDataHeight = dataLimits.getHeight();
			dataBottomY = dataLimits.getBottomY();
			// size __totalDataWidth to be proportional to __totalDataHeight, given the screen proportions		
			totalDataWidth = ((double)drawingLimits.getWidth() / (double)drawingLimits.getHeight()) * totalDataHeight;
			if (dataLimits.getWidth() > totalDataWidth)
			{
				// If the data limits are greater than the total data width that can fit on the paper, then 
				// reset the total data width to be the width of the data limits. 
				dataLeftX = dataLimits.getLeftX();
				totalDataWidth = dataLimits.getWidth();
			}
			else
			{
				// otherwise, if the width of the network can fit within the paper entirely, center the network
				// horizontally on the paper.  This is when the network is zoomed out.
				double diff = totalDataWidth - dataLimits.getWidth();
				dataLeftX = dataLimits.getLeftX() - (diff / 2);
				dataLimitsMedia.setLeftX(dataLeftX);
				dataLimitsMedia.setRightX(dataLeftX + totalDataWidth);
			}
		}
		if (setInternalData)
		{
			this.__fitWidth = fitWidth;
			this.__totalDataWidth = totalDataWidth;
			this.__totalDataHeight = totalDataHeight;
			this.__dataLeftX = dataLeftX;
			this.__dataBottomY = dataBottomY;
		}
		return dataLimitsMedia;
	}

	/// <summary>
	/// Calculates the scaled font size that will equal the font size passed in.
	/// This is to handle the discrepancy between how fonts are drawn in points by
	/// the Java code and how they are handled in the network.  The network font sizes
	/// do not match up to point font sizes, because of the need to scale them. </summary>
	/// <param name="name"> the name of the font to calculate for. </param>
	/// <param name="style"> the style of the font to calculate for. </param>
	/// <param name="size"> the network size of the font. </param>
	/// <param name="isPrinting"> whether the network is currently being printed or not. </param>
	/// <returns> the equivalent font size in points. </returns>
	private int calculateScaledFont(string name, string style, int size, bool isPrinting)
	{
		double scale = 0;
		if (__fitWidth)
		{
			double pixels = __dpi * (int)(__pageFormat.getWidth() / 72);
			double pct = (getBounds().width / pixels);
			double width = __totalDataWidth * pct;
			scale = width / __screenDataWidth;
		}
		else
		{
			double pixels = __dpi * (int)(__pageFormat.getHeight() / 72);
			double pct = (getBounds().height / pixels);
			double height = __totalDataHeight * pct;
			scale = height / __screenDataHeight;
		}
		int fontPixelSize = (int)(size * scale);
		__drawingArea.setFont(name, style, size);
		int fontSize = __drawingArea.calculateFontSize(fontPixelSize);
		if (!isPrinting)
		{
			return fontSize;
		}
		double temp = (double)fontSize * ((double)__dpi / 72.0);
		temp += 0.5;
		int printFontSize = (int)temp + 2;
		return printFontSize;
	}

	/// <summary>
	/// Checks the id of the specified node to see if it is unique in the network.
	/// If not, the string _X (where X is the number of instances of this node's ID)
	/// will be appended to the node's ID.   The node is guaranteed to have a unique id by the end of this method. </summary>
	/// <param name="pos"> the position of the node in the node array. </param>
	private void checkUniqueID(int pos)
	{
		int count = 0;
		string id = __nodes[pos].getCommonID();

		for (int i = 0; i < __nodes.Length; i++)
		{
			if (i != pos)
			{
				if (__nodes[i].getCommonID().Equals(id))
				{
					count++;
				}
			}
		}

		if (count > 0)
		{
			__nodes[pos].setCommonID(id + "_" + count);
		}
	}

	/// <summary>
	/// Clears the screen; fills with white.
	/// </summary>
	public virtual void clear()
	{
		// drawn the area outside the network with grey first
		GRDrawingAreaUtil.setColor(__drawingArea, GRColor.gray);
		GRDrawingAreaUtil.fillRectangle(__drawingArea, __screenLeftX, __screenBottomY, __screenDataWidth, __screenDataHeight);

		// the network area is filled with white
		GRDrawingAreaUtil.setColor(__drawingArea, GRColor.white);
		GRDrawingAreaUtil.fillRectangle(__drawingArea, __dataLeftX, __dataBottomY, __totalDataWidth, __totalDataHeight);
	}

	/// <summary>
	/// Clear annotations and redraw.
	/// </summary>
	public virtual void clearAnnotations()
	{
		IList<StateMod_Network_AnnotationData> annotationDataList = getAnnotationData();
		int size = annotationDataList.Count;
		annotationDataList.Clear();
		// Also redraw
		if (size > 0)
		{
			// Previously had some annotations and now do not so redraw
			forceRepaint();
		}
	}

	// TODO evaluate how to change the visible screen extent to ensure that the data extent is fully visible.
	/// <summary>
	/// Center on the given extent.  This code is similar to findNode() except that the data extent is
	/// already found and provides the coordinates.  If the limits do not fit in the viewpoint, zoom out.
	/// If the limits do fit, keep the current zoom to minimize disruption to the user. </summary>
	/// <param name="limits"> the limits of the data extents to center on. </param>
	public virtual void centerOn(GRLimits limits)
	{
		// Point on which to center...

		double x = limits.getCenterX();
		double y = limits.getCenterY();
		double buffer = 1.1;
		double width = limits.getWidth() * buffer;
		double width2 = width / 2.0;
		double height = limits.getHeight() * buffer;
		double height2 = height / 2.0;

		// Recompute the view position in data units...
		__screenLeftX = x - (__screenDataWidth / 2);
		__screenBottomY = y - (__screenDataHeight / 2);

		// Loop until the network fits (see how this looks visually - maybe cool?)
		while (((x - width2) < __screenLeftX) || ((x + width2) > (__screenLeftX + __screenDataWidth)) || ((y - height2) < __screenBottomY) || ((y + height2) > (__screenBottomY + __screenDataHeight)))
		{
			forceRepaint();
			zoomOut();
		}
		forceRepaint();
	}

	/// <summary>
	/// Converts a value that runs from 0 to __totalBufferWidth (the number of 
	/// pixels wide the screen buffer is) to a value from 0 to __totalDataWidth.  
	/// Add the value to __dataLeftX to get the data value. </summary>
	/// <param name="x"> the value to convert to data limits. </param>
	/// <returns> a value that runs from 0 to __totalDataWidth. </returns>
	protected internal virtual double convertAbsX(double x)
	{
		return (x / __totalBufferWidth) * __totalDataWidth;
	}

	/// <summary>
	/// Converts a value that runs from 0 to __totalBufferHeight (the number of pixels 
	/// high the screen buffer is) to a value from 0 to __totalDataHeight.
	/// Add the value to __dataBottomY to get the data value. </summary>
	/// <param name="y"> the value to convert to data limits. </param>
	/// <returns> a value that runs from 0 to __totalDataHeight. </returns>
	protected internal virtual double convertAbsY(double y)
	{
		return (y / __totalBufferHeight) * __totalDataHeight;
	}

	// TODO SAM 2011-07-08 Shouldn't this be operating on width?
	/// <summary>
	/// Converts an X value from being scaled for drawing units to be scaled for 
	/// data units.  The X value passed in to this method will run from 0 to 
	/// the number of pixels across the GUI screen.  This is used for scaling fonts and node sizes. </summary>
	/// <param name="x"> the x value to scale. </param>
	/// <returns> the x value scaled to fit in the data units. </returns>
	private double convertDrawingXToDataX(double x)
	{
		if (__printingNetwork)
		{
			// In-memory component is not used for drawing
			return x * (__screenDataHeight / getDrawingArea().getDrawingLimits().getHeight());
		}
		else
		{
			return x * (__screenDataHeight / getBounds().height);
		}
	}

	//TODO SAM 2011-07-08 Shouldn't this be operating on height?
	/// <summary>
	/// Converts an Y value from being scaled for drawing units to be scaled for 
	/// data units.  The Y value passed in to this method will run from 0 to 
	/// the number of pixels up the GUI screen.  This is used for scaling fonts and node sizes. </summary>
	/// <param name="y"> the y value to scale. </param>
	/// <returns> the y value scaled to fit in the data units. </returns>
	private double convertDrawingYToDataY(double y)
	{
		if (__printingNetwork)
		{
			// In-memory component is not used for drawing
			return y * (__screenDataWidth / getDrawingArea().getDrawingLimits().getWidth());
		}
		else
		{
			return y * (__screenDataWidth / getBounds().width);
		}
	}

	/// <summary>
	/// Create a drawing area representing the full-scale rendering of the network.  This is used to calculate the
	/// scale when rendering occurs for different media than the layout, or a part of the full layout. </summary>
	/// <param name="network"> the StateMod network that is being rendered </param>
	/// <param name="pageLayoutPropList"> page layout properties </param>
	/// <returns> the drawing area that is configured for full scale rendering - the drawing and data limits from the
	/// drawing area can be used to compute the scale when compared to the current rendering drawing area </returns>
	private GRJComponentDrawingArea createDrawingAreaFullScale(StateMod_NodeNetwork network, PropList pageLayoutPropList)
	{
		// Determine the full drawing limits for the page layout media (portrait mode).
		string paperSize = pageLayoutPropList.getValue("PaperSize");
		PageFormat pageFormat = PrintUtil.getPageFormat(paperSize); // This is portrait
		// Initialize a device using the full page size (margins=0).  Setting the limits will not actually
		// size the JComponent, just keep the limits in memory for calculations
		GRLimits deviceLimits = new GRLimits(0, 0, pageFormat.getWidth(), pageFormat.getHeight());
		if (pageLayoutPropList.getValue("PageOrientation").equalsIgnoreCase("Landscape"))
		{
			// Swap the dimensions
			deviceLimits = new GRLimits(0, 0, pageFormat.getHeight(), pageFormat.getWidth());
		}
		GRJComponentDevice dev = new GRJComponentDevice("Virtual device for full scale limits.");
		dev.setLimits(deviceLimits);
		// Initialize the drawing area using the full page but subtract the margin from the imageable area
		// TODO SAM 2011-07-09 does the actual margin need to be used?  For example, when printing
		// the user may specify a different margin than the defaults - for now it is a minor error
		double margin = getMargin();
		GRLimits drawingLimits = new GRLimits(deviceLimits.getLeftX() + margin, deviceLimits.getBottomY() + margin, deviceLimits.getRightX() - margin, deviceLimits.getTopY() - margin);
		// Determine the data limits for the media
		GRLimits dataLimitsOrig = new GRLimits(network.getLX(),network.getBY(),network.getRX(),network.getTY());
		double[] buffer = network.getEdgeBuffer();
		// Calculate the data limits required to fill the media
		GRLimits dataLimits = calculateDataLimitsForMedia(drawingLimits, dataLimitsOrig, buffer, false);
		// Create a drawing area that matches the drawing (media) and data (fill media at full scale) limits
		GRJComponentDrawingArea drawingAreaFullScale = new GRJComponentDrawingArea(this, "StateMod_Network DrawingArea full scale", GRAspect.TRUE, drawingLimits, GRUnits.DEVICE, GRLimits.DEVICE, dataLimits);
		return drawingAreaFullScale;
	}

	/// <summary>
	/// Creates a node change operation (for undo/redo) that encapsulates a move 
	/// in which more than one node was dragged around the network.
	/// </summary>
	private void createMultiNodeChangeOperation()
	{
		double mainX = __mouseDataX - __xAdjust;
		double mainY = __mouseDataY - __yAdjust;
		double dx = mainX - __mouseDownX;
		double dy = mainY - __mouseDownY;

		StateMod_Network_UndoData data = new StateMod_Network_UndoData();
		data.nodeNum = __clickedNodeNum;
		data.oldX = __nodes[__clickedNodeNum].getX();
		data.oldY = __nodes[__clickedNodeNum].getY();
		data.newX = __mouseDataX;
		data.newY = __mouseDataY;

		data.otherNodes = new int[__draggedNodes.Length];
		data.oldXs = new double[__draggedNodes.Length];
		data.oldYs = new double[__draggedNodes.Length];
		data.newXs = new double[__draggedNodes.Length];
		data.newYs = new double[__draggedNodes.Length];

		for (int i = 0; i < __draggedNodes.Length; i++)
		{
			data.otherNodes[i] = __draggedNodes[i];
			data.oldXs[i] = __nodes[__draggedNodes[i]].getX();
			data.oldYs[i] = __nodes[__draggedNodes[i]].getY();
			data.newXs[i] = data.oldXs[i] + dx;
			data.newYs[i] = data.oldYs[i] + dy;
		}

		addNodeChangeOperation(data);
	}

	/// <summary>
	/// Deletes a link from a node.  If the node has multiple links, they are
	/// displayed in a dialog and the user can choose the one to delete.
	/// </summary>
	private void deleteLink()
	{
		if (__links == null)
		{
			return;
		}

		int size = __links.Count;
		PropList p = null;
		string from = null;
		string id = __nodes[__popupNodeNum].getCommonID();
		string to = null;
		IList<string> links = new List<string>();
		IList<int?> nums = new List<int?>();

		// Gather all the links in the network that reference the node
		// that the popup menu was opened in
		for (int i = 0; i < size; i++)
		{
			p = __links[i];
			from = p.getValue("FromNodeID");
			to = p.getValue("ToNodeID");
			if (from.Equals(id) || to.Equals(id))
			{
				links.Add("" + from + " -> " + to);
				nums.Add(new int?(i));
			}
		}

		size = links.Count;
		if (size == 1)
		{
			// If there is only one link involving the clicked-on node, then delete it outright.
			int i = nums[0].Value;
			__links.RemoveAt(i);
			setNetworkChanged(true);
			forceRepaint();
			return;
		}

		// Prompt the user for the link to delete
		JComboBoxResponseJDialog d = new JComboBoxResponseJDialog(__parent, "Select the link to delete", "Select the link to delete", links, ResponseJDialog.OK | ResponseJDialog.CANCEL);

		string s = d.response();
		if (string.ReferenceEquals(s, null))
		{
			// If no node was selected, don't delete a node.
			return;
		}

		// Find the node to delete and delete it
		string link = null;
		for (int i = 0; i < size; i++)
		{
			link = links[i];
			if (link.Equals(s))
			{
				int j = nums[i].Value;
				__links.RemoveAt(j);
				forceRepaint();
				setNetworkChanged(true);
				return;
			}
		}
	}

	/// <summary>
	/// Deletes a node from the network and rebuilds the network connectivity. </summary>
	/// <param name="id"> the id of the node to delete. </param>
	public virtual void deleteNode(string id)
	{
		removeIDFromLinks(id);
		__network.deleteNode(id);
		buildNodeArray();
		findMaxReachLevel();
		forceRepaint();
		__undoOperations = new List<StateMod_Network_UndoData>();
		__parent.setUndo(false);
		__parent.setRedo(false);
	}

	/// <summary>
	/// Draws annotations on the network.  These are the simple line annotations.  There are also annotations for
	/// more complex StateMod data, drawn by the annotation renderers.
	/// </summary>
	private void drawAnnotations()
	{
		// If annotations were read from an XML file, then they will
		// need an initial process to fill out the bounding box data
		// in the node objects that hold each one
		if (!__annotationsProcessed && __processAnnotations)
		{
			processAnnotationsFromNetwork();
			__annotationsProcessed = true;
		}

		GRDrawingAreaUtil.setColor(__drawingArea, GRColor.black);
		Rectangle bounds = null;
		if (__printingNetwork && !__useOldPrinting)
		{
			bounds = new Rectangle((int)this.getLimits().getWidth(), (int)this.getLimits().getHeight());
		}
		else
		{
			bounds = getBounds();
		}
		double scale = 0;
		if (__fitWidth)
		{
			double pixels = __dpi * (int)(__pageFormat.getWidth() / 72);
			double pct = (bounds.width / pixels);
			double width = __totalDataWidth * pct;
			scale = width / __screenDataWidth;
		}
		else
		{
			double pixels = __dpi * (int)(__pageFormat.getHeight() / 72);
			double pct = (bounds.height / pixels);
			double height = __totalDataHeight * pct;
			scale = height / __screenDataHeight;
		}
		if (__printingNetwork)
		{
			if (!__useOldPrinting)
			{
				scale = getScale();
			}
		}

		double fontSize = -1;
		double temp = -1;
		HydrologyNode node = null;
		int fontPixelSize = -1;
		int origFontSizeInt = -1;
		int printFontSize = -1;
		int size = __annotations.Count;
		PropList p = null;
		string origFontSize = null;
		for (int i = 0; i < size; i++)
		{
			if (i == __clickedNodeNum && __isLastSelectedAnAnnotation)
			{
				// skip it -- outline being drawn for drag
				continue;
			}
			node = __annotations[i];
			p = (PropList)node.getAssociatedObject();
			string fname = p.getValue("FontName");
			string style = p.getValue("FontStyle");
			GRDrawingAreaUtil.setFont(__drawingArea, fname, style, 7);
			origFontSize = p.getValue("OriginalFontSize");
			origFontSizeInt = Integer.decode(origFontSize).intValue();
			fontPixelSize = (int)(origFontSizeInt * scale);
			fontSize = __drawingArea.calculateFontSize(fontPixelSize);
			if (__printingNetwork)
			{
				if (__useOldPrinting)
				{
					temp = (double)fontSize * ((double)__dpi / 72.0);
					temp += 0.5;
					printFontSize = (int)temp + 2;
				}
				else
				{
					// For now do not scale
					printFontSize = (int)(origFontSizeInt * scale);
				}
				p.set("FontSize", "" + printFontSize);
			}
			else
			{
				p.set("FontSize", "" + (int)fontSize);
			}
			p.set("DrawOutOfBounds", "true");
			node.setAssociatedObject(p);

			GRDrawingAreaUtil.drawAnnotation(__drawingArea, (PropList)node.getAssociatedObject());

			if (__showAnnotationBoundingBox)
			{
				GRDrawingAreaUtil.setColor(__drawingArea, GRColor.red);
				GRDrawingAreaUtil.drawRectangle(__drawingArea, node.getX(), node.getY(), node.getWidth(), node.getHeight());
			}
		}
	}

	/// <summary>
	/// Draws the outline of a node being dragged. </summary>
	/// <param name="num"> the index of the node being dragged in the __nodes array. </param>
	private void drawDraggedNodeOutline(int num)
	{
		double mainX = __mouseDataX - __xAdjust;
		double mainY = __mouseDataY - __yAdjust;

		double dx = mainX - __mouseDownX;
		double dy = mainY - __mouseDownY;

		double nodeX = __draggedNodesXs[num] + dx;
		double nodeY = __draggedNodesYs[num] + dy;

		double w = __dragNodesLimits[num].getWidth();
		double h = __dragNodesLimits[num].getHeight();

		GRDrawingAreaUtil.drawLine(__drawingArea, nodeX, nodeY, nodeX + w, nodeY);
		GRDrawingAreaUtil.drawLine(__drawingArea, nodeX, nodeY + h, nodeX + w, nodeY + h);
		GRDrawingAreaUtil.drawLine(__drawingArea, nodeX, nodeY, nodeX, nodeY + h);
		GRDrawingAreaUtil.drawLine(__drawingArea, nodeX + w, nodeY, nodeX + w, nodeY + h);
	}

	/// <summary>
	/// Draws the legend.  A single node is created, its properties are set, and then it is drawn
	/// directly on the network.
	/// </summary>
	private void drawLegend()
	{
		/*
		Legends look generally like this, where the left column has normal nodes and
		the right column has nodes decorated to indicate natural flow,
		which are typically the same as the normal node but with an extra outer outline.
	
		+-----------------------------------------------------+
		|   Legend                                            |
		+-----------------------------------------------------+
		|                                                     |
		|   [Icon]  Node Text         [Icon] Node Text        |
		|                                                     |
		|                  .............................      |
		|                                                     |
		|   [Icon]  Node Text         [Icon] Node Text        |
		+-----------------------------------------------------+
		*/

		GRLimits limits = null;

		int by = 0;
		int col2x = 0; // the X value of the point where the second column begins
		int dividerY = 0; // the Y value of the line dividing the legend title from the rest
		int height = 0; // running total of the height of the legend
		int tempW = 0;
		int lx = 0;

		int width = 0;

		////////////////////////////////////////////////////////////////
		// Spacing parameters
		////////////////////////////////////////////////////////////////
		double colsp = convertDrawingXToDataX(15); // spacing between columns 1 and 2
		double edge = convertDrawingXToDataX(2); // spacing between the edge lines and the interior boundary line width
		int line = 1;
		double tsp = convertDrawingXToDataX(4); // spacing between nodes and node text
		double vsp = convertDrawingYToDataY(4); // vertical spacing between lines

		string text = "Legend";
		limits = GRDrawingAreaUtil.getTextExtents(__drawingArea, text, GRUnits.DATA);

		// Add the height of the text, plus space around the text before the boundary lines
		height += (int)((int)limits.getHeight() + (edge * 2));
		// Add the height of the line that divides the title from the rest
		height += line;
		// Hold this point for calculating the exact divider Y point later
		dividerY = height;

		double id = 0; // Inside diameter
		double bd = 0;
		if (__fitWidth)
		{
			id = convertDrawingXToDataX(__legendNodeDiameter);
			int third = (int)(__legendNodeDiameter / 3);
			if ((third % 2) == 1)
			{
				third++;
			}
			bd = convertDrawingXToDataX(third);

			if (id < limits.getHeight())
			{
				id = limits.getHeight() * 1.2;
			}
		}
		else
		{
			// Get the icon size from the first node (there will always be an end node)
			id = convertDrawingYToDataY(__nodes[0].getIconDiameter());
			int third = (int)(__legendNodeDiameter / 3);
			if ((third % 2) == 1)
			{
				third++;
			}
			bd = convertDrawingYToDataY(third);

			if (id < limits.getHeight())
			{
				id = limits.getHeight() * 1.2;
			}
		}

		// Add enough room to accommodate the number of node icons vertically, plus vertical
		// space between them, plus space around them between the lines.
		int numIcons = 10;
		height += (int)((numIcons * (id + bd + vsp)) + (edge * 2) - vsp);

		// Determine the width of the legend by taking the widths of the
		// two longest node labels and using them to determine the width required 
		text = "Most Downstream Node";
		limits = GRDrawingAreaUtil.getTextExtents(__drawingArea, text, GRUnits.DATA);
		tempW = (int)limits.getWidth();

		// Store the X point at which the second column begins
		col2x = (int)(edge + id + bd + tsp + tempW + colsp + ((id + bd) / 2));

		text = "Instream (Minimum) Flow / Natural Flow";
		limits = GRDrawingAreaUtil.getTextExtents(__drawingArea, text, GRUnits.DATA);

		tempW = (int)limits.getWidth();

		// calculate the overall width of the legend
		width = (int)(col2x + ((id + bd) / 2) + tsp + tempW + edge);

		// the legend's limits will be not null if the legend has already been drawn once.
		if (__legendLimitsDetermined)
		{
			lx = (int)__legendDataLimits.getLeftX();
			by = (int)__legendDataLimits.getBottomY();
		}
		// they will be null if the legend has not been drawn yet, or if the paper size has changed
		else
		{
	/*	
			lx = (int)convertX((int)(__pageFormat.getImageableX() / __printScale)) + 50;	
			by = (int)convertY((int)((__pageFormat.getHeight() 
				- (__pageFormat.getImageableHeight() + __pageFormat.getImageableY())) / __printScale)) + 50;
	*/		
			if (__network.isLegendPositionSet())
			{
				lx = (int)__network.getLegendX();
				by = (int)__network.getLegendY();
			}
			else
			{
				lx = (int)(__dataLimits.getLeftX() + (0.05 * __dataLimits.getWidth()));
				by = (int)(__dataLimits.getBottomY() + (0.05 * __dataLimits.getHeight()));
			}
			__legendLimitsDetermined = true;
		}
		__legendDataLimits = new GRLimits(lx, by, lx + width, by + height);

		// block out with white the area over which the legend will be drawn.
		// This means no nodes can be drawn over the top of the legend.
		GRDrawingAreaUtil.setColor(__drawingArea, GRColor.white);
		GRDrawingAreaUtil.fillRectangle(__drawingArea, lx, by, width, height);

		// determine the absolute point of the divider line.
		dividerY = by + height - dividerY;

		// move the second column X position to take into account the new left X
		col2x += lx;
		// determine the point at which the first column will go.
		int col1x = (int)(lx + edge + ((id + bd) / 2));

		// NOTE:
		// since the nodes are drawn centered on points, notice that both the
		// first and second column X values are for the center of the node.

		// draw the outline of the legend
		GRDrawingAreaUtil.setColor(__drawingArea, GRColor.black);
		GRDrawingAreaUtil.drawLine(__drawingArea, lx, by, lx + width, by);
		GRDrawingAreaUtil.drawLine(__drawingArea, lx, by + height, lx + width, by + height);
		GRDrawingAreaUtil.drawLine(__drawingArea, lx, by, lx, by + height);
		GRDrawingAreaUtil.drawLine(__drawingArea, lx + width, by, lx + width, by + height);
		GRDrawingAreaUtil.drawLine(__drawingArea, lx, dividerY, lx + width, dividerY);

		// determine the amounts to increment X and Y values after drawing nodes and rows of nodes.
		int yInc = (int)((id + bd) + vsp);
		int xInc = (int)((id + bd) / 2 + tsp);

		// determine the X positions for drawing the first and second column's text
		int text1x = col1x + xInc;
		int text2x = col2x + xInc;

		// draw the title
		GRDrawingAreaUtil.drawText(__drawingArea, "Legend", col1x, dividerY + edge, 0, 0);

		// draw the nodes.  
		// For the most part, if there are two columns of nodes, the second
		// column is the same type as the first but displays the natural flow representation, with
		// " / Natural Flow" appended to the node label.  
		// So between drawing column 1 and column 2, just change the X 
		// value and set isNaturalFlow to true.  When moving down a row to a
		// new node type, call resetNode() to reset node properties.
		// The node graphic is drawn using the single node and its properties.
		// The text for node labels is drawn separate from the nodes, not as the standard node label.

		// End of network
		HydrologyNode node = new HydrologyNode();
		node.setIconDiameter(__nodes[0].getIconDiameter());
		node.setDecoratorDiameter(__nodes[0].getDecoratorDiameter());
		// FIXME SAM 2003-08-15 Need to remove WIS code
		//node.setInWis(false);
		node.setX(col1x);
		node.setY(dividerY - edge - ((id + bd) / 2));
		node.setType(HydrologyNode.NODE_TYPE_END);
		node.draw(__drawingArea);
		text = "Most Downstream Node";
		GRDrawingAreaUtil.drawText(__drawingArea, text, text1x, node.getY(), 0, GRText.CENTER_Y);

		// Diversion
		node.setY(node.getY() - yInc);
		node.setX(col1x);
		node.resetNode(HydrologyNode.NODE_TYPE_DIV, false, false);
		node.draw(__drawingArea);
		text = "Diversion";
		GRDrawingAreaUtil.drawText(__drawingArea, text, text1x, node.getY(), 0, GRText.CENTER_Y);
		node.setX(col2x);
		node.resetNode(HydrologyNode.NODE_TYPE_DIV, true, false);
		node.draw(__drawingArea);
		text += " / Natural Flow";
		GRDrawingAreaUtil.drawText(__drawingArea, text, text2x, node.getY(), 0, GRText.CENTER_Y);

		// Diversion + Wells
		node.setY(node.getY() - yInc);
		node.setX(col1x);
		node.resetNode(HydrologyNode.NODE_TYPE_DIV_AND_WELL, false, false);
		node.draw(__drawingArea);
		text = "Diversion + Well(s)";
		GRDrawingAreaUtil.drawText(__drawingArea, text, text1x, node.getY(), 0, GRText.CENTER_Y);
		node.setX(col2x);
		node.resetNode(HydrologyNode.NODE_TYPE_DIV_AND_WELL, true, false);
		node.draw(__drawingArea);
		text += " / Natural Flow";
		GRDrawingAreaUtil.drawText(__drawingArea, text, text2x, node.getY(), 0, GRText.CENTER_Y);

		// Instream flow
		node.setY(node.getY() - yInc);
		node.setX(col1x);
		node.resetNode(HydrologyNode.NODE_TYPE_ISF, false, false);
		node.draw(__drawingArea);
		text = "Instream (Minimum) Flow";
		GRDrawingAreaUtil.drawText(__drawingArea, text, text1x, node.getY(), 0, GRText.CENTER_Y);
		node.setX(col2x);
		node.resetNode(HydrologyNode.NODE_TYPE_ISF, true, false);
		node.draw(__drawingArea);
		text += " / Natural Flow";
		GRDrawingAreaUtil.drawText(__drawingArea, text, text2x, node.getY(), 0, GRText.CENTER_Y);

		// Other
		node.setY(node.getY() - yInc);
		node.setX(col1x);
		node.resetNode(HydrologyNode.NODE_TYPE_OTHER, false, false);
		node.draw(__drawingArea);
		text = "Other";
		GRDrawingAreaUtil.drawText(__drawingArea, text, text1x, node.getY(), 0, GRText.CENTER_Y);
		node.setX(col2x);
		node.resetNode(HydrologyNode.NODE_TYPE_OTHER, true, false);
		node.draw(__drawingArea);
		text += " / Natural Flow";
		GRDrawingAreaUtil.drawText(__drawingArea, text, text2x, node.getY(), 0, GRText.CENTER_Y);

		// Plan
		node.setY(node.getY() - yInc);
		node.setX(col1x);
		node.resetNode(HydrologyNode.NODE_TYPE_PLAN, false, false);
		node.draw(__drawingArea);
		text = "Plan";
		GRDrawingAreaUtil.drawText(__drawingArea, text, text1x, node.getY(), 0, GRText.CENTER_Y);
		node.setX(col2x);
		node.resetNode(HydrologyNode.NODE_TYPE_PLAN, true, false);
		node.draw(__drawingArea);
		text += " / Natural Flow";
		GRDrawingAreaUtil.drawText(__drawingArea, text, text2x, node.getY(), 0, GRText.CENTER_Y);

		// Reservoir
		node.setY(node.getY() - yInc);
		node.setX(col1x);
		node.resetNode(HydrologyNode.NODE_TYPE_RES, false, false);
		node.draw(__drawingArea);
		text = "Reservoir";
		GRDrawingAreaUtil.drawText(__drawingArea, text, text1x, node.getY(), 0, GRText.CENTER_Y);
		node.setX(col2x);
		node.resetNode(HydrologyNode.NODE_TYPE_RES, true, false);
		node.draw(__drawingArea);
		text += " / Natural Flow";
		GRDrawingAreaUtil.drawText(__drawingArea, text, text2x, node.getY(), 0, GRText.CENTER_Y);

		// Streamflow
		node.setY(node.getY() - yInc);
		node.setX(col1x);
		node.resetNode(HydrologyNode.NODE_TYPE_FLOW, false, false);
		node.draw(__drawingArea);
		text = "Streamflow Gage";
		GRDrawingAreaUtil.drawText(__drawingArea, text, text1x, node.getY(), 0, GRText.CENTER_Y);
		node.setX(col2x);
		node.resetNode(HydrologyNode.NODE_TYPE_FLOW, true, false);
		node.draw(__drawingArea);
		text += " / Natural Flow";
		GRDrawingAreaUtil.drawText(__drawingArea, text, text2x, node.getY(), 0, GRText.CENTER_Y);

		// Well
		node.setY(node.getY() - yInc);
		node.setX(col1x);
		node.resetNode(HydrologyNode.NODE_TYPE_WELL, false, false);
		node.draw(__drawingArea);
		text = "Well(s)";
		GRDrawingAreaUtil.drawText(__drawingArea, text, text1x, node.getY(), 0, GRText.CENTER_Y);
		node.setX(col2x);
		node.resetNode(HydrologyNode.NODE_TYPE_WELL, true, false);
		node.draw(__drawingArea);
		text += " / Natural Flow";
		GRDrawingAreaUtil.drawText(__drawingArea, text, text2x, node.getY(), 0, GRText.CENTER_Y);

		// Other
		node.setY(node.getY() - yInc);
		node.setX(col1x);
		node.resetNode(HydrologyNode.NODE_TYPE_OTHER, false, true);
		node.draw(__drawingArea);
		text = "Import Indicator";
		GRDrawingAreaUtil.drawText(__drawingArea, text, text1x, node.getY(), 0, GRText.CENTER_Y);
		/* TODO SAM 2008-12-09 - export
		node.setX(col2x);
		node.resetNode(HydrologyNode.NODE_TYPE_OTHER, true, false );
		node.draw(__drawingArea);
		text += " / Natural Flow";
		GRDrawingAreaUtil.drawText(__drawingArea, text, text2x, node.getY(), 0, GRText.CENTER_Y);
		*/
	}

	/// <summary>
	/// Draws links between nodes.
	/// TODO (JTS - 2004-07-13) This would be faster if the nodes were not looked up every time.  Store the
	/// position of the nodes within the node array.  The positions would have to be
	/// recomputed every time a node is added or deleted.
	/// </summary>
	private void drawLinks(GRJComponentDrawingArea drawingArea, IList<PropList> links, HydrologyNode[] nodes, float[] dashes)
	{
		if (links == null)
		{
			return;
		}
		float offset = 0;
		HydrologyNode node1 = null;
		HydrologyNode node2 = null;
		string fromNodeId = null;
		string toNodeId = null;
		string propValue = null;
		double[] dashPattern = new double[dashes.Length];
		for (int i = 0; i < dashes.Length; i++)
		{
			dashPattern[i] = dashes[i];
		}
		double[] linePattern = null;
		GRLineStyleType lineStyle = GRLineStyleType.DASHED; // Default
		GRArrowStyleType fromArrowStyle = GRArrowStyleType.NONE;
		GRArrowStyleType toArrowStyle = GRArrowStyleType.NONE;
		double arrowWidth = 0; // Arrow head width and length
		double arrowLength = 0;
		foreach (PropList p in links)
		{
			fromNodeId = p.getValue("FromNodeID");
			toNodeId = p.getValue("ToNodeID");
			propValue = p.getValue("LineStyle");
			lineStyle = GRLineStyleType.valueOfIgnoreCase(propValue);
			if (lineStyle == null)
			{
				lineStyle = GRLineStyleType.DASHED;
				linePattern = dashPattern;
			}
			propValue = p.getValue("FromArrowStyle");
			fromArrowStyle = GRArrowStyleType.valueOfIgnoreCase(propValue);
			if (fromArrowStyle == null)
			{
				fromArrowStyle = GRArrowStyleType.NONE;
			}
			propValue = p.getValue("ToArrowStyle");
			toArrowStyle = GRArrowStyleType.valueOfIgnoreCase(propValue);
			if (toArrowStyle == null)
			{
				toArrowStyle = GRArrowStyleType.NONE;
			}
			node1 = null;
			node2 = null;
			for (int j = 0; j < nodes.Length; j++)
			{
				if (nodes[j].getCommonID().Equals(fromNodeId))
				{
					node1 = nodes[j];
				}
				if (nodes[j].getCommonID().Equals(toNodeId))
				{
					node2 = nodes[j];
				}
				if (node1 != null && node2 != null)
				{
					j = nodes.Length + 1;
				}
			}
			if (lineStyle == GRLineStyleType.DASHED)
			{
				// Set line style to dashes
				linePattern = dashPattern;
			}
			else
			{
				// Draw line solid
				linePattern = null;
			}
			drawingArea.setLineDash(linePattern, offset);
			if ((fromArrowStyle == GRArrowStyleType.NONE) && (toArrowStyle == GRArrowStyleType.NONE))
			{
				// Connect nodes with a line...
				GRDrawingAreaUtil.drawLine(drawingArea, node1.getX(), node1.getY(), node2.getX(), node2.getY());
			}
			else
			{
				// Connect nodes with an arrow.  Because the arrow may obscure or be obscured by the
				// node symbol, back off the length of the line to ensure that the arrow will only point
				// to the symbol.  Make the size of the arrow the length of the symbol but only 2/3 the width
				double x1 = node1.getX();
				double y1 = node1.getY();
				double x2 = node2.getX();
				double y2 = node2.getY();
				double diamData = 0.0; // Diameter if node icon in data units
				if (fromArrowStyle != GRArrowStyleType.NONE)
				{
					// Adjust the "from" node coordinates.
					double diamDrawing = node1.getIconDiameter(); // Drawing units
					diamData = GRDrawingAreaUtil.getDataExtents(drawingArea, new GRLimits(0,0,diamDrawing,diamDrawing), 0).getWidth();
					double xDiff = node2.getX() - node1.getX();
					double yDiff = node2.getY() - node1.getY();
					double lineLength = Math.Sqrt(xDiff + xDiff + yDiff * yDiff);
					double xAdjust = MathUtil.interpolate(diamData / 2, 0, lineLength, 0, Math.Abs(xDiff));
					double yAdjust = MathUtil.interpolate(diamData / 2, 0, lineLength, 0, Math.Abs(yDiff));
					if (xDiff >= 0)
					{
						x1 += xAdjust;
					}
					else
					{
						x1 -= xAdjust;
					}
					if (yDiff >= 0)
					{
						y1 += yAdjust;
					}
					else
					{
						y1 -= yAdjust;
					}
				}
				if (toArrowStyle != GRArrowStyleType.NONE)
				{
					// Adjust the "to" node coordinates.
					double diamDrawing = node2.getIconDiameter(); // Drawing units
					diamData = GRDrawingAreaUtil.getDataExtents(drawingArea, new GRLimits(0,0,diamDrawing,diamDrawing), 0).getWidth();
					double xDiff = node1.getX() - node2.getX();
					double yDiff = node1.getY() - node2.getY();
					double lineLength = Math.Sqrt(xDiff + xDiff + yDiff * yDiff);
					double xAdjust = MathUtil.interpolate(diamData / 2, 0, lineLength, 0, Math.Abs(xDiff));
					double yAdjust = MathUtil.interpolate(diamData / 2, 0, lineLength, 0, Math.Abs(yDiff));
					if (xDiff >= 0)
					{
						x2 += xAdjust;
					}
					else
					{
						x2 -= xAdjust;
					}
					if (yDiff >= 0)
					{
						y2 += yAdjust;
					}
					else
					{
						y2 -= yAdjust;
					}
				}
				arrowLength = diamData;
				arrowWidth = diamData / 2;
				// Exaggerate to troubleshoot
				//arrowLength = diamData*2;
				//arrowWidth = diamData;
				GRDrawingAreaUtil.drawArrow(drawingArea, x1, y1, x2, y2, lineStyle, linePattern, 0.0, fromArrowStyle, toArrowStyle, arrowWidth, arrowLength); // Arrow properties
			}
		}
		// Set back to solid line for drawing area
		drawingArea.setFloatLineDash(null, (float)0);
	}

	/// <summary>
	/// Draws the lines between all the nodes.
	/// </summary>
	private void drawNetworkLines()
	{
		bool dots = false;
		float offset = 0;
		double[] x = new double[2];
		double[] y = new double[2];
		HydrologyNode ds = null;
		HydrologyNode dsRealNode = null;
		HydrologyNode holdNode = null;
		HydrologyNode holdNode2 = null;
		HydrologyNode node = null;
		HydrologyNode nodeTop = __network.getMostUpstreamNode();

		GRDrawingAreaUtil.setLineWidth(__drawingArea, __lineThickness);

		float[] tempDots = new float[2];
		if (__lineThickness >= 1)
		{
			tempDots[0] = (float)(__dots[0] * __lineThickness);
			tempDots[1] = (float)(__dots[1] * __lineThickness);
		}
		else
		{
			tempDots[0] = (float)(__dots[0]);
			tempDots[1] = (float)(__dots[1]);
		}

		for (node = nodeTop; node != null; node = StateMod_NodeNetwork.getDownstreamNode(node, StateMod_NodeNetwork.POSITION_COMPUTATIONAL))
		{
			// Move ahead and skip and blank or unknown nodes (which won't
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

			dsRealNode = StateMod_NodeNetwork.getDownstreamNode(node, StateMod_NodeNetwork.POSITION_COMPUTATIONAL);

			dots = false;
			if (dsRealNode != null && dsRealNode.getType() == HydrologyNode.NODE_TYPE_XCONFLUENCE)
			{
				dots = true;
			}

			   // Move ahead and skip and blank or unknown nodes (which won't
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

			if (__shadedRivers)
			{
				// Show a shaded wide line for the rivers
				GRDrawingAreaUtil.setColor(__drawingArea, GRColor.gray);
				GRDrawingAreaUtil.setLineWidth(__drawingArea, (double)(__maxReachLevel - node.getReachLevel() + 1) * __lineThickness);
				//Message.printStatus(1, "", "MaxReachLevel: " + __maxReachLevel
				//	+ "   NodeReachLevel: " + node.getReachLevel() + ", line width multiplier = " 
				//	+ ((__maxReachLevel - node.getReachLevel() + 1)) );
				GRDrawingAreaUtil.drawLine(__drawingArea, x, y);
			}

			GRDrawingAreaUtil.setColor(__drawingArea, GRColor.black);
			GRDrawingAreaUtil.setLineWidth(__drawingArea, __lineThickness);

			if (dots)
			{
				__drawingArea.setFloatLineDash(tempDots, offset);
				GRDrawingAreaUtil.drawLine(__drawingArea, x, y);
				__drawingArea.setFloatLineDash(null, (float)0);
			}
			else
			{
				//Message.printStatus(2,"drawNodes","Drawing lines between nodes " + x[0] + "," + y[0] + " to " +
				//	x[1] + "," + y[1] );
				GRDrawingAreaUtil.drawLine(__drawingArea, x, y);
			}
			holdNode = node;
		}
		GRDrawingAreaUtil.setLineWidth(__drawingArea, 1);
	}

	/// <summary>
	/// Draws the nodes on the screen.
	/// </summary>
	private void drawNodes(GRJComponentDrawingArea drawingArea, HydrologyNode[] nodes)
	{
		for (int i = 0; i < nodes.Length; i++)
		{
			nodes[i].draw(drawingArea);
		}
	}

	/// <summary>
	/// Draws the outline of a dragged node on the screen while it is being dragged. </summary>
	/// <param name="g"> the Graphics context to use for dragging the node. </param>
	private void drawNodesOutlines(Graphics g)
	{
		// Force the graphics context to be the on-screen one, not the double-buffered one
		forceGraphics(g);
		GRDrawingAreaUtil.setColor(__drawingArea, GRColor.black);

		GRDrawingAreaUtil.drawLine(__drawingArea, __mouseDataX - __xAdjust, __mouseDataY - __yAdjust, __draggedNodeLimits.getWidth() + __mouseDataX - __xAdjust, __mouseDataY - __yAdjust);

		GRDrawingAreaUtil.drawLine(__drawingArea, __mouseDataX - __xAdjust, __mouseDataY + __draggedNodeLimits.getHeight() - __yAdjust, __draggedNodeLimits.getWidth() + __mouseDataX - __xAdjust, __mouseDataY + __draggedNodeLimits.getHeight() - __yAdjust);
		GRDrawingAreaUtil.drawLine(__drawingArea, __mouseDataX - __xAdjust, __mouseDataY - __yAdjust, __mouseDataX - __xAdjust, __mouseDataY + __draggedNodeLimits.getHeight() - __yAdjust);
		GRDrawingAreaUtil.drawLine(__drawingArea, __mouseDataX + __draggedNodeLimits.getWidth() - __xAdjust, __mouseDataY - __yAdjust, __mouseDataX + __draggedNodeLimits.getWidth() - __xAdjust, __mouseDataY + __draggedNodeLimits.getHeight() - __yAdjust);

		for (int i = 0; i < __draggedNodes.Length; i++)
		{
			drawDraggedNodeOutline(i);
		}
	}

	/// <summary>
	/// Called when a user presses OK on an Add Node dialog.
	/// </summary>
	protected internal virtual void endAddNode()
	{
		setNetworkChanged(true);
		forceRepaint();
	}

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~StateMod_Network_JComponent()
	{
		__dashes = null;
		__dots = null;
		__bufferGraphics = null;
		__drawingArea = null;
		IOUtil.nullArray(__dragNodesLimits);
		__dataLimits = null;
		__draggedNodeLimits = null;
		__holdLimits = null;
		__legendDataLimits = null;
		IOUtil.nullArray(__nodes);
		__network = null;
		__draggedNodes = null;
		__deleteLinkMenuItem = null;
		__addNodeMenuItem = null;
		__deleteNodeMenuItem = null;
		__annotationPopup = null;
		__nodePopup = null;
		__networkPopup = null;
		__pageFormat = null;
		__parent = null;
		__referenceJComponent = null;
		__holdPaperOrientation = null;
		__holdPaperSize = null;
		__annotations = null;
		__links = null;
		__undoOperations = null;
	}

	/// <summary>
	/// Finds the highest reach level in the entire network.  This is used for shading rivers. </summary>
	/// <returns> the highest reach level in the entire network. </returns>
	private int findMaxReachLevel()
	{
		int reach = 0;
		for (int i = 0; i < __nodes.Length; i++)
		{
			if (__nodes[i].getReachLevel() > reach)
			{
				reach = __nodes[i].getReachLevel();
			}
		}
		return reach;
	}

	/// <summary>
	/// Determines the nearest grid point in data units relative to the data unit points passed in. </summary>
	/// <param name="px"> an X point, in data units </param>
	/// <param name="py"> a Y point, in data units </param>
	/// <returns> a two-element integer array.  Element one stores the X location of
	/// the nearest grid point and element two stores the Y location.  Both are in data units. </returns>
	private double[] findNearestGridXY(int px, int py)
	{
		double[] p = new double[2];

		// determine the amount that the point is away from a grid location
		double lx = Math.Abs(__dataLeftX) % __gridStep;
		double by = Math.Abs(__dataBottomY) % __gridStep;

		double posX = (px % __gridStep) - lx;
		double posY = (py % __gridStep) - by;

		// then determine which gridstep it is closer to -- for X, the left one or the right one
		if (posX > (__gridStep / 2))
		{
			p[0] = (__gridStep - posX);
		}
		else
		{
			p[0] = -1 * posX;
		}

		// for Y the top one or the bottom one
		if (posY > (__gridStep / 2))
		{
			p[1] = (__gridStep - posY);
		}
		else
		{
			p[1] = -1 * posY;
		}
		return p;
	}

	/// <summary>
	/// Determines the nearest grid point in data units relative to the point stored in the MouseEvent. </summary>
	/// <param name="event"> the MouseEvent for which to find the nearest grid point. </param>
	/// <returns> a two-element integer array.  Element one stores the X location of
	/// the nearest grid point and element two stores the Y location.  Both are in data units. </returns>
	private double[] findNearestGridXY(MouseEvent @event)
	{
		double[] p = new double[2];

		// Turn both the X and Y mouse locations into data units
		double x = convertDrawingXToDataX(@event.getX()) + __screenLeftX;
		double y = (int)convertDrawingYToDataY(invertY(@event.getY())) + __screenBottomY;

		// Determine how far off the mouse position is from a grid step

		double lx = 0;
		if (__dataLeftX < 0)
		{
			lx = Math.Abs(__dataLeftX) % __gridStep;
		}
		else
		{
			lx = -1 * (__dataLeftX % __gridStep);
		}
		double by = 0;
		if (__dataBottomY < 0)
		{
			by = Math.Abs(__dataBottomY) % __gridStep;
		}
		else
		{
			by = -1 * (__dataBottomY % __gridStep);
		}

		double posX = (x % __gridStep) + lx;
		double posY = (y % __gridStep) + by;
		// Determine which gridstep it is closer to -- for X, the left one or the right one
		if (posX > (__gridStep / 2))
		{
			p[0] = x + (__gridStep - posX);
		}
		else
		{
			p[0] = x - posX;
		}

		// For Y the top one or the bottom one
		if (posY > (__gridStep / 2))
		{
			p[1] = y + (__gridStep - posY);
		}
		else
		{
			p[1] = y - posY;

		}

		return p;
	}

	/// <summary>
	/// Determines the nearest grid point in data units relative to the point stored in the MouseEvent. </summary>
	/// <param name="event"> the MouseEvent for which to find the nearest grid point. </param>
	/// <returns> a two-element integer array.  Element one stores the X location of
	/// the nearest grid point and element two stores the Y location.  Both are in data units. </returns>
	private double[] findNearestGridXY(double x, double y)
	{
		double[] p = new double[2];

		// determine how far off the mouse position is from a grid step

		double lx = 0;
		if (__dataLeftX < 0)
		{
			lx = Math.Abs(__dataLeftX) % __gridStep;
		}
		else
		{
			lx = -1 * (__dataLeftX % __gridStep);
		}
		double by = 0;
		if (__dataBottomY < 0)
		{
			by = Math.Abs(__dataBottomY) % __gridStep;
		}
		else
		{
			by = -1 * (__dataBottomY % __gridStep);
		}

		double posX = (x % __gridStep) + lx;
		double posY = (y % __gridStep) + by;
		// Determine which gridstep it is closer to -- for X, the left one or the right one
		if (posX > (__gridStep / 2))
		{
			p[0] = x + (__gridStep - posX);
		}
		else
		{
			p[0] = x - posX;
		}

		// For Y the top one or the bottom one
		if (posY > (__gridStep / 2))
		{
			p[1] = y + (__gridStep - posY);
		}
		else
		{
			p[1] = y - posY;

		}

		return p;
	}

	/// <summary>
	/// Displays a dialog containing all the annotations on the network; the user can 
	/// select one and the annotation will be highlighted and zoomed to.
	/// </summary>
	public virtual HydrologyNode findAnnotation()
	{
		// Compile a list of all the annotations in the network
		IList<string> annotationListTextSorted = new List<string>();
		PropList props = null;
		string propValue;
		foreach (HydrologyNode annotation in getNetwork().getAnnotationList())
		{
			props = (PropList)annotation.getAssociatedObject();
			if (props != null)
			{
				propValue = props.getValue("Text");
				if (!string.ReferenceEquals(propValue, null))
				{
					annotationListTextSorted.Add(propValue);
				}
			}
		}

		// Sort to ascending String order
		annotationListTextSorted.Sort(string.CASE_INSENSITIVE_ORDER);

		// Display a dialog from which the user can choose the annotation to find
		JComboBoxResponseJDialog j = new JComboBoxResponseJDialog(__parent, "Select the Annotation to Find", "Select the network annotation to find", annotationListTextSorted, ResponseJDialog.OK | ResponseJDialog.CANCEL, true);

		string s = j.response();
		if (string.ReferenceEquals(s, null))
		{
			// If s is null, then the user pressed CANCEL
			return null;
		}

		// Find the annotation in the network and center the display window around it.
		HydrologyNode foundAnnotation = findAnnotation(s, true, true);

		if (foundAnnotation == null)
		{
			(new ResponseJDialog(__parent, "Annotation '" + s + "' not found", "The annotation with text '" + s + "' could not be found.\n" + "The annotation text must match exactly, including case\n" + "sensitivity.", ResponseJDialog.OK)).response();
			return null;
		}

		return foundAnnotation;
	}

	/// <summary>
	/// Find an annotation given its common identifier. </summary>
	/// <param name="changeSelection"> if true, then change the selection of the annotation as the identifier is checked </param>
	/// <param name="center"> if true, center on the found node; if false do not change position </param>
	/// <returns> the found node, or null if not found </returns>
	public virtual HydrologyNode findAnnotation(string s, bool changeSelection, bool center)
	{
		double x = 0;
		double y = 0;
		HydrologyNode foundAnnotation = null;
		PropList props = null;
		string propValue = null;
		foreach (HydrologyNode annotation in getNetwork().getAnnotationList())
		{
			props = (PropList)annotation.getAssociatedObject();
			if (props != null)
			{
				propValue = props.getValue("Text");
				if (!string.ReferenceEquals(propValue, null))
				{
					if (propValue.Equals(s))
					{
						x = annotation.getX();
						y = annotation.getY();
						if (changeSelection)
						{
							annotation.setSelected(true);
						}
						foundAnnotation = annotation;
					}
				}
			}
			else
			{
				if (changeSelection)
				{
					annotation.setSelected(false);
				}
			}
		}

		if (center)
		{
			// center the screen around the node
			__screenLeftX = x - (__screenDataWidth / 2);
			__screenBottomY = y - (__screenDataHeight / 2);
			forceRepaint();
		}

		return foundAnnotation;
	}

	/// <summary>
	/// Displays a dialog containing all the nodes on the network; the user can 
	/// select one and the node will be highlighted and zoomed to.
	/// </summary>
	public virtual HydrologyNode findNode()
	{
		// compile a list of all the node IDs in the network
		int size = __nodes.Length;
		IList<string> v = new List<string>(size);
		for (int i = 0; i < size; i++)
		{
			v.Add(__nodes[i].getCommonID());
		}

		// sort to ascending String order
		v.Sort(string.CASE_INSENSITIVE_ORDER);

		// display a dialog from which the user can choose the node to find
		JComboBoxResponseJDialog j = new JComboBoxResponseJDialog(__parent, "Select the Node to Find", "Select the node to find in the network", v, ResponseJDialog.OK | ResponseJDialog.CANCEL, true);

		string s = j.response();
		if (string.ReferenceEquals(s, null))
		{
			// if s is null, then the user pressed CANCEL
			return null;
		}

		// find the node in the network and center the display window around it.
		HydrologyNode foundNode = findNode(s, true, true);

		if (foundNode == null)
		{
			(new ResponseJDialog(__parent, "Node '" + s + "' not found", "The node with ID '" + s + "' could not be found.\n" + "The node ID must match exactly, including case\n" + "sensitivity.", ResponseJDialog.OK)).response();
			return null;
		}

		return foundNode;
	}

	/// <summary>
	/// Find a node given its common identifier. </summary>
	/// <param name="changeSelection"> if true, then change the selection of the node as the identifier is checked </param>
	/// <param name="center"> if true, center on the found node; if false do not change position </param>
	/// <returns> the found node, or null if not found </returns>
	public virtual HydrologyNode findNode(string s, bool changeSelection, bool center)
	{
		double x = 0;
		double y = 0;
		HydrologyNode foundNode = null;
		int size = __nodes.Length;
		for (int i = 0; i < size; i++)
		{
			if (__nodes[i].getCommonID().Equals(s))
			{
				x = __nodes[i].getX();
				y = __nodes[i].getY();
				if (changeSelection)
				{
					__nodes[i].setSelected(true);
				}
				foundNode = __nodes[i];
			}
			else
			{
				if (changeSelection)
				{
					__nodes[i].setSelected(false);
				}
			}
		}

		if (center)
		{
			// center the screen around the node
			__screenLeftX = x - (__screenDataWidth / 2);
			__screenBottomY = y - (__screenDataHeight / 2);
			forceRepaint();
		}

		return foundNode;
	}

	/// <summary>
	/// Finds the number of the node at the specified click. </summary>
	/// <param name="x"> the x location of the click on the screen </param>
	/// <param name="y"> the y location of the click on the screen, inverted for RTi Y style. </param>
	/// <returns> the number of the node at the specified click, or -1 if no node was clicked on. </returns>
	private int findNodeOrAnnotationAtXY(double x, double y)
	{
		string routine = "Statemod_Network_JComponent.findNodeAtXY";
		double newX = x;
		double newY = y;
		if (Message.isDebugOn)
		{
			Message.printDebug(1, routine, "Trying to find node or annotation at X: " + x + "   Y: " + y);
		}
		for (int i = (__nodes.Length - 1); i >= 0; i--)
		{
	//		Message.printStatus(1, "", "" + __nodes[i].getCommonID() + "  " 
	//			+ __nodes[i].getX() + ", " + __nodes[i].getY()
	//			+ "   [" + __nodes[i].getWidth() + "/" 
	//			+ __nodes[i].getHeight() + "]  "
	//			+ __nodes[i].isVisible());
			if (__nodes[i].contains(newX, newY) && __nodes[i].isVisible())
			{
				__isLastSelectedAnAnnotation = false;
				if (Message.isDebugOn)
				{
					Message.printDebug(1, routine, "Found node [" + i + "] at X: " + x + "   Y: " + y);
				}
				return i;
			}
		}

		GRLimits limits = null;
		HydrologyNode node = null;
		int size = __annotations.Count;
		for (int i = 0; i < size; i++)
		{
			node = __annotations[i];
			limits = new GRLimits(node.getX(), node.getY(), node.getX() + node.getWidth(), node.getY() + node.getHeight());
			if (limits.contains(x, y) && node.isVisible())
			{
				__isLastSelectedAnAnnotation = true;
				if (Message.isDebugOn)
				{
					Message.printDebug(1, routine, "Found annotation [" + i + "] at X: " + x + "   Y: " + y);
				}
				return i;
			}
		}
		return -1;
	}

	/// <summary>
	/// Forces the display to be completely repainted.
	/// </summary>
	public virtual void forceRepaint()
	{
		__forceRefresh = true;
		repaint();
	}

	/// <summary>
	/// Return the list of StateMod_Network_AnnotationData to be processed when rendering the map.
	/// </summary>
	protected internal virtual IList<StateMod_Network_AnnotationData> getAnnotationData()
	{
		return __annotationDataList;
	}

	/// <summary>
	/// Returns the annotation node held in the annotations list at the specified position. </summary>
	/// <returns> the annotation node held in the annotations list at the specified position. </returns>
	protected internal virtual HydrologyNode getAnnotationNode(int nodeNum)
	{
		return __annotations[nodeNum];
	}

	/// <summary>
	/// Return the dash pattern used for dashed lines.
	/// </summary>
	protected internal virtual float [] getDashPattern()
	{
		return __dashes;
	}

	/// <summary>
	/// Returns the bottom Y value of the data. </summary>
	/// <returns> the bottom Y value of the data. </returns>
	protected internal virtual double getDataBottomY()
	{
		return __dataBottomY;
	}

	/// <summary>
	/// Returns the left X value of the data. </summary>
	/// <returns> the left X value of the data. </returns>
	protected internal virtual double getDataLeftX()
	{
		return __dataLeftX;
	}

	/// <summary>
	/// Returns the data limits for the entire network. </summary>
	/// <returns> the data limits for the entire network. </returns>
	protected internal virtual GRLimits getDataLimits()
	{
		return __dataLimits;
	}

	/// <summary>
	/// Returns the data limits for the entire network. </summary>
	/// <returns> the data limits for the entire network. </returns>
	protected internal virtual GRLimits getDataLimitsMax()
	{
		return __dataLimitsMax;
	}

	/// <summary>
	/// Return the GRDrawingArea used for drawing.  This allows external code to draw on the drawing area. </summary>
	/// <returns> the GRDrawingArea used for drawing. </returns>
	public virtual GRJComponentDrawingArea getDrawingArea()
	{
		return __drawingArea;
	}

	/// <summary>
	/// Returns the limits of the legend, which are in data units. </summary>
	/// <returns> the limits of the legend. </returns>
	protected internal virtual GRLimits getLegendDataLimits()
	{
		return __legendDataLimits;
	}

	/// <summary>
	/// Return the link list.
	/// </summary>
	protected internal virtual IList<PropList> getLinkList()
	{
		return __links;
	}

	/// <summary>
	/// Return the margin in inches (all sides are the same).
	/// </summary>
	public virtual double getMargin()
	{
		return.75;
	}

	/// <summary>
	/// Returns GRLImits with points representing the bounds of the printing margins.  
	/// Called by the reference display.
	/// Returns GRLImits with points representing the bounds of the printing margins.  
	/// </summary>
	public virtual GRLimits getMarginLimits()
	{
		double leftX = __pageFormat.getImageableX() / __printScale;
		double topY = (__pageFormat.getHeight() - __pageFormat.getImageableY()) / __printScale;
		double rightX = (leftX + __pageFormat.getImageableWidth() / __printScale) - 1;
		double bottomY = ((__pageFormat.getHeight() - (__pageFormat.getImageableY() + __pageFormat.getImageableHeight())) / __printScale) + 1;
		leftX = convertAbsX(leftX) + __dataLeftX;
		topY = convertAbsY(topY) + __dataBottomY;
		rightX = convertAbsX(rightX) + __dataLeftX;
		bottomY = convertAbsY(bottomY) + __dataBottomY;

		return new GRLimits(leftX, bottomY, rightX, topY);
	}

	/// <summary>
	/// Returns the node network. </summary>
	/// <returns> the node network. </returns>
	protected internal virtual StateMod_NodeNetwork getNetwork()
	{
		return __network;
	}

	/// <summary>
	/// Indicate whether any of the main network or its node properties have changed since the last load/save.
	/// </summary>
	public virtual bool getNetworkChanged()
	{
		return __networkChanged;
	}

	/// <summary>
	/// Returns the nodes array. </summary>
	/// <returns> the nodes array. </returns>
	protected internal virtual HydrologyNode[] getNodesArray()
	{
		return __nodes;
	}


	/// <summary>
	/// Returns a list of all the nodes in the node array that are a given type. </summary>
	/// <param name="type"> the type of nodes (as defined in HydrologyNode.NODE_*) to return. </param>
	/// <returns> a list of all the nodes that are the specified type.  The list is guaranteed to be non-null. </returns>
	public virtual IList<HydrologyNode> getNodesForType(int type)
	{
		IList<HydrologyNode> v = new List<HydrologyNode>();

		for (int i = 0; i < __nodes.Length; i++)
		{
			if (__nodes[i].getType() == type)
			{
				v.Add(__nodes[i]);
			}
		}

		return v;
	}

	/// <summary>
	/// Returns the print scale for the network. </summary>
	/// <returns> the print scale for the network. </returns>
	protected internal virtual double getPrintScale()
	{
		return __printScale;
	}

	/// <summary>
	/// Return the list of selected nodes, presumably so that specific actions can be taken in coordination
	/// with editing.  Major adjustments to the nodes will require rebuilding the node list by calling
	/// buildNodeArray() and possibly refreshing the network display. </summary>
	/// <returns> the list of selected nodes </returns>
	protected internal virtual IList<HydrologyNode> getSelectedNodes()
	{
		HydrologyNode[] nodeArray = getNodesArray();
		IList<HydrologyNode> nodeList = new List<HydrologyNode>();
		for (int i = 0; i < nodeArray.Length; i++)
		{
			if (nodeArray[i].isSelected())
			{
				nodeList.Add(nodeArray[i]);
			}
		}
		return nodeList;
	}

	/// <summary>
	/// Returns the data limits from the absolute bottom left to the total data width
	/// and total data height (which may or may not match up with the size of the paper)
	/// due to network size requirements.
	/// </summary>
	protected internal virtual GRLimits getTotalDataLimits()
	{
		return new GRLimits(__dataLeftX, __dataBottomY, __dataLeftX + __totalDataWidth, __dataBottomY + __totalDataHeight);
	}

	/// <summary>
	/// Return the scale, relative to a full 1:1 rendering for the page layout.
	/// A value of 2 means that the rendered graphics are twice as big as the full-size rendering.
	/// A value of .5 means that the rendered graphics are 1/2 as big as the full-size rendering. </summary>
	/// <returns> the scale, relative to a full 1:1 rendering for the page layout </returns>
	public virtual double getScale()
	{ //String routine = getClass().getName() + ".getScale";
		double scale = Double.NaN;
		// The scale is determined by mapping the current drawing area to the full scale drawing area
		// The resulting data range is then compared to the full scale data range.
		// In other words, how big would the full-scale drawing area be if the current rendering settings were
		// used to do a full rendering?  The scale is then the ratio of the full scale drawing area height to
		// the drawing area that would result from the full rendering at current settings.
		double drawingAreaHeightRendered = this.getDrawingArea().getDrawingLimits().getHeight() * (this.getDrawingArea().getDataLimits().getHeight() / this.__drawingAreaFullScale.getDataLimits().getHeight());
		double drawingAreaHightFullScale = this.__drawingAreaFullScale.getDrawingLimits().getHeight();
		scale = drawingAreaHeightRendered / drawingAreaHightFullScale;
		Message.printStatus(2, "", "Scale is: " + scale);
		return scale;
	}

	/// <summary>
	/// Return the selected network page layout properties to use for rendering. </summary>
	/// <returns> the selected network page layout properties to use for rendering </returns>
	public virtual PropList getSelectedPageLayoutPropList()
	{
		return this.__selectedPageLayoutPropList;
	}

	/// <summary>
	/// Returns the total height of the entire network. </summary>
	/// <returns> the total height of the entire network. </returns>
	protected internal virtual int getTotalHeight()
	{
		return __totalBufferHeight;
	}

	/// <summary>
	/// Returns the total width of the entire network. </summary>
	/// <returns> the total width of the entire network. </returns>
	protected internal virtual int getTotalWidth()
	{
		return __totalBufferWidth;
	}

	/// <summary>
	/// Returns the visible limits of the screen in data units. </summary>
	/// <returns> the visible limits of the screen in data units. </returns>
	protected internal virtual GRLimits getVisibleDataLimits()
	{
		return new GRLimits(__screenLeftX, __screenBottomY, __screenLeftX + __screenDataWidth, __screenBottomY + __screenDataHeight);
	}

	/// <summary>
	/// Initialize the network component for the network layout.
	/// This method is called in the constructor used for printing.
	/// Extract basic settings from the network page layout and set in this object
	/// This is necessary because these properties are not part of the printer job
	/// Printer job properties are controlled by the GraphicsPrinterJob that is used to
	/// initiate printing.  The print() and paint() methods will adjust to the printer job settings
	/// and imageable area.
	/// </summary>
	protected internal virtual void initializeForNetworkPageLayout(string pageLayout)
	{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		string routine = this.GetType().FullName + ".initializeForNetworkPageLayout";
		StateMod_NodeNetwork net = getNetwork();
		//String orientation = null;
		//String paperSize = null;
		double nodeSize = 10;
		int nodeFontSize = 10;
		IList<PropList> layouts = net.getLayoutList();
		bool found = false;
		foreach (PropList p in layouts)
		{
			string id = p.getValue("ID");
			if (id.Equals(pageLayout, StringComparison.OrdinalIgnoreCase))
			{
				setSelectedPageLayoutPropList(p);
				//orientation = p.getValue("PageOrientation");
				//paperSize = p.getValue("PaperSize");
				nodeSize = double.Parse(p.getValue("NodeSize"));
				Message.printStatus(2,routine,"Node size from layout is " + nodeSize + " points.");
				// This does not work here because it needs graphics from print/paint time
				//setPrintNodeSize(nodeSize);
				nodeFontSize = int.Parse(p.getValue("NodeLabelFontSize"));
				setPrintFontSize(nodeFontSize);
				found = true;
				break;
			}
		}
		if (!found)
		{
			throw new Exception("Layout \"" + pageLayout + "\" was not found in network.  Cannot initialize network.");
		}
		setDataLimits(new GRLimits(net.getLX(), net.getBY(), net.getRX(), net.getTY()));
		//setPaperSize(paperSize);
		//setOrientation(orientation);
		//setPrintNodeSize(nodeSize);
		//setPrintFontSize(nodeFontSize);
		//forceRepaint();
	}

	/// <summary>
	/// Initialize internal settings for printing.
	/// </summary>
	protected internal virtual void initializeForPrinting()
	{
		__drawMargin = false; // Margin is non-imageable area
		__antiAlias = true; // On for printing, to look better
		__printCount = 0;
		__dpi = 72; // for full-scale printing, used to scale fonts, etc.
		// FIXME SAM 2011-07-05 All of this seems non needed when printing - just use print graphics
		//__tempBuffer = new BufferedImage(__totalBufferWidth, __totalBufferHeight, BufferedImage.TYPE_4BYTE_ABGR);
		//__bufferGraphics = (Graphics2D)(__tempBuffer.createGraphics());
		// TODO SAM 2011-07-04 Had to put the following in for batch processing
		// because graphics is used for checking font size, etc.
		//if ( _graphics == null ) {
		//	_graphics = __bufferGraphics;
		//}
		__printingNetwork = true;
		// FIXME SAM 2011-07-05 Not sure if this is needed
		//zoomOneToOne();
	}

	/// <summary>
	/// Inverts the value of Y so that Y runs from 0 at the bottom to MAX at the top.
	/// This method typically is only called by interactive events such as mouse drags since
	/// other drawing is done using drawing limits that handle the inverted Y axis. </summary>
	/// <param name="y"> the value of Y to invert. </param>
	/// <returns> the inverted value of Y. </returns>
	private double invertY(double y)
	{
		return _devy2 - y;
	}

	/// <summary>
	/// Checks to see if the network is dirty. </summary>
	/// <returns> true if the network is dirty, false if not. </returns>
	public virtual bool isDirty()
	{
		if (getNetworkChanged())
		{
	//		Message.printStatus(1, "", "isDirty: NetworkChanged: true");
			return true;
		}
		for (int i = 0; i < __nodes.Length; i++)
		{
			if (__nodes[i].isDirty())
			{
	//			Message.printStatus(1, "", "isDirty: Node[" + i + "]: dirty");
				return true;
			}
		}
		int size = __annotations.Count;
		HydrologyNode node = null;
		for (int i = 0; i < size; i++)
		{
			node = __annotations[i];
			if (node.isDirty())
			{
	//			Message.printStatus(1, "", "isDirty: Annotation[" + i + "]: dirty");
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Listens for key presses events and cancels drags if Escape is pushed. </summary>
	/// <param name="event"> the KeyEvent that happened. </param>
	public virtual void keyPressed(KeyEvent @event)
	{
		if (__legendDrag || __nodeDrag)
		{
			// only worry about escape keypresses
			if (@event.getKeyCode() == KeyEvent.VK_ESCAPE)
			{
				// and only worry about them if something is currently being dragged
				if (__legendDrag)
				{
					__legendDrag = false;
				}
				else if (__nodeDrag)
				{
					if (__isLastSelectedAnAnnotation)
					{
						HydrologyNode node = __annotations[__clickedNodeNum];
						node.setVisible(true);
					}
					else
					{
						__parent.displayNodeXY(__nodes[__clickedNodeNum].getX(), __nodes[__clickedNodeNum].getY());
						__nodes[__clickedNodeNum].setVisible(true);
					}
					__clickedNodeNum = -1;
					__nodeDrag = false;
				}
				forceRepaint();
			}
		}
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void keyReleased(KeyEvent @event)
	{
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void keyTyped(KeyEvent @event)
	{
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void mouseClicked(MouseEvent @event)
	{
	}

	// TODO SAM 2011-07-07 The drag of multiple objects distorts the placement of the objects in the group
	// and it gets worse as multiple drags occur
	/// <summary>
	/// Responds to mouse dragged events and moves around the legend, a node, or 
	/// the screen, depending on what is being dragged. </summary>
	/// <param name="event"> the MouseEvent that happened. </param>
	public virtual void mouseDragged(MouseEvent @event)
	{
		double xx = convertDrawingXToDataX(@event.getX()) + __screenLeftX;
		double yy = convertDrawingYToDataY(invertY(@event.getY())) + __screenBottomY;
		__parent.setLocation(xx, yy);
		if (!__editable && !__screenDrag)
		{
			return;
		}
		if (__nodeDrag)
		{
			if (__snapToGrid)
			{
				// this version of findNearestGridXY does 
				// the conversion to data units from mouse-click units 
				double[] p = findNearestGridXY(@event);

				// if the nearest grid position is not where the node currently is located ...
				if (p[0] != (int)__mouseDataX || p[1] != (int)__mouseDataY)
				{
					// move the node to be situated on that grid point
					__mouseDataX = p[0];
					__mouseDataY = p[1];
					repaint();
				}
			}
			else
			{
				// convert the mouse click to data units so that the
				// node bounding box can be drawn on the screen
				__mouseDataX = convertDrawingXToDataX(@event.getX()) + __screenLeftX;
				__mouseDataY = convertDrawingYToDataY(invertY(@event.getY())) + __screenBottomY;
				repaint();
			}
			__parent.displayNodeXY(__mouseDataX, __mouseDataY);
		}
		else if (__legendDrag)
		{
			if (__snapToGrid)
			{
				// this version of findNearestGridXY assumes already-
				// converted X and Y values are passed in.  Use them
				double cx = convertDrawingXToDataX(@event.getX()) + __screenLeftX;
				double cy = convertDrawingYToDataY(invertY(@event.getY())) + __screenBottomY;
				double[] p = findNearestGridXY((int)cx - (int)__xAdjust, (int)cy - (int)__yAdjust);

				// if the nearest grid location is different from where the legend was last ...
				if (__mouseDataX != ((int)cx + p[0]) || __mouseDataY != ((int)cy + p[1]))
				{
					// move the legend so that it is at that grid location
					__mouseDataX = (int)cx + p[0];
					__mouseDataY = (int)cy + p[1];
				}
				repaint();
			}
			else
			{
				__mouseDataX = convertDrawingXToDataX(@event.getX()) + __screenLeftX;
				__mouseDataY = convertDrawingYToDataY(invertY(@event.getY())) + __screenBottomY;
				repaint();
			}
		}
		else if (__screenDrag)
		{
			double mouseX = convertDrawingXToDataX(@event.getX()) + __screenLeftX;
			double mouseY = convertDrawingYToDataY(invertY(@event.getY())) + __screenBottomY;

			double dx = __mouseDataX - mouseX;
			double dy = __mouseDataY - mouseY;

			__mouseDataX = mouseX;
			__mouseDataY = mouseY;

			if (__screenDataWidth >= __totalDataWidth)
			{
				// If zoomed out so that the entire network and paper is smaller than the screen ...
				// only allow the screen to be moved to the left, so that left edge of the paper can be aligned
				// with the left edge of the screen
				if (dx > 0)
				{
				}
				else
				{
					__mouseDataX -= __screenLeftX;
					__screenLeftX += (dx * __DX);
					if (__screenLeftX < __dataLeftX)
					{
						__screenLeftX = __dataLeftX;
					}

					__mouseDataX += __screenLeftX;
				}
			}
			else
			{
				// If zoomed in so not all the network or paper can
				// be seen at once, then determine how far the 
				// screen was panned and adjust accordingly
				__mouseDataX -= __screenLeftX;
				__screenLeftX += (dx * __DX);

				// Don't allow the screen to be moved too far right or left.
				if (__screenLeftX > (__dataLeftX + __totalDataWidth) - convertDrawingXToDataX(getBounds().width))
				{
					__screenLeftX = (__dataLeftX + __totalDataWidth) - convertDrawingXToDataX(getBounds().width);
				}

				if (__screenLeftX < __dataLeftX)
				{
					__screenLeftX = __dataLeftX;
				}

				__mouseDataX += __screenLeftX;
			}

			if (__screenDataHeight >= __totalDataHeight)
			{
				// If zoomed out so that the entire network and paper is smaller than the screen ...
				// only allow the screen to be moved down, so that bottom edge of the paper can be aligned
				// with the bottom edge of the screen		
				if (dy > 0)
				{
				}
				else
				{
					__mouseDataY -= __screenBottomY;
					__screenBottomY += (dy * __DY);

					if (__screenBottomY < __dataBottomY)
					{
						__screenBottomY = __dataBottomY;
					}

					__mouseDataY += __screenBottomY;
				}
			}
			else
			{
				// if zoomed in so not all the network or paper can
				// be seen at once, then determine how far the 
				// screen was panned and adjust accordingly		
				__mouseDataY -= __screenBottomY;
				__screenBottomY += (dy * __DY);

				// don't allow the screen to be moved too far up or down.
				if (__screenBottomY > (__dataBottomY + __totalDataHeight) - convertDrawingYToDataY(getBounds().height))
				{
					__screenBottomY = (__dataBottomY + __totalDataHeight) - convertDrawingYToDataY(getBounds().height);
				}

				if (__screenBottomY < __dataBottomY)
				{
					__screenBottomY = __dataBottomY;
				}

				__mouseDataY += __screenBottomY;
			}

			forceRepaint();
		}
		else if (__drawingBox)
		{
			__currDragX = convertDrawingXToDataX(@event.getX()) + __screenLeftX;
			__currDragY = convertDrawingYToDataY(invertY(@event.getY())) + __screenBottomY;
			repaint();
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
	/// Updates the current mouse location in the parent frame's status bar. </summary>
	/// <param name="event"> the MouseEvent that happened. </param>
	public virtual void mouseMoved(MouseEvent @event)
	{
		double xx = convertDrawingXToDataX(@event.getX()) + __screenLeftX;
		double yy = convertDrawingYToDataY(invertY(@event.getY())) + __screenBottomY;
		__parent.setLocation(xx, yy);
	}

	/// <summary>
	/// Responds to mouse pressed events by determining whether a node, the legend or
	/// the screen was clicked on. </summary>
	/// <param name="event"> the MouseEvent that happened. </param>
	public virtual void mousePressed(MouseEvent @event)
	{
		// do not respond to popup events, so make sure that it was the left
		// mouse button that was pressed.
		if (@event.getButton() != MouseEvent.BUTTON1)
		{
			return;
		}

		// determine first whether a node was clicked on
		__clickedNodeNum = findNodeOrAnnotationAtXY(convertDrawingXToDataX(@event.getX()) + __screenLeftX, convertDrawingYToDataY(invertY(@event.getY())) + __screenBottomY);

		// if a node was clicked on ...
		if (__clickedNodeNum > -1)
		{
			__mouseDeviceX = @event.getX();
			__mouseDeviceY = @event.getY();
			if (__isLastSelectedAnAnnotation)
			{
				__parent.displayNode(getAnnotationNode(__clickedNodeNum));
			}
			else
			{
				__parent.displayNode(__nodes[__clickedNodeNum]);
			}
			// ... then set everything up so that the node can be dragged around the screen.  
			if (!__editable)
			{
				return;
			}

			if (__isLastSelectedAnAnnotation)
			{
				HydrologyNode node = __annotations[__clickedNodeNum];
				__draggedNodeLimits = new GRLimits(node.getX(), node.getY(), node.getX() + node.getWidth(), node.getY() + node.getHeight());
				if (__snapToGrid)
				{
					double[] p = findNearestGridXY(@event);
					__mouseDataX = p[0];
					__mouseDataY = p[1];
				}
				else
				{
					__mouseDataX = convertDrawingXToDataX(@event.getX()) + __screenLeftX;
					__mouseDataY = convertDrawingYToDataY(invertY(@event.getY())) + __screenBottomY;
				}
				__xAdjust = __mouseDataX - __draggedNodeLimits.getMinX();
				__yAdjust = __mouseDataY - __draggedNodeLimits.getMinY();

				__draggedNodes = new int[0];
			}
			else
			{
				if (__snapToGrid)
				{
					double[] p = findNearestGridXY(@event);
					__mouseDataX = p[0];
					__mouseDownX = __mouseDataX;
					__mouseDataY = p[1];
					__mouseDownY = __mouseDataY;
					__draggedNodeLimits = __nodes[__clickedNodeNum].getLimits();
					__xAdjust = __nodes[__clickedNodeNum].getWidth() / 2;
					__yAdjust = __nodes[__clickedNodeNum].getHeight() / 2;
				}
				else
				{
					__mouseDataX = convertDrawingXToDataX(@event.getX()) + __screenLeftX;
					__mouseDownX = __mouseDataX;
					__mouseDataY = convertDrawingYToDataY(invertY(@event.getY())) + __screenBottomY;
					__mouseDownY = __mouseDataY;
					__draggedNodeLimits = __nodes[__clickedNodeNum].getLimits();
					__xAdjust = convertDrawingXToDataX(@event.getX()) + __screenLeftX - __draggedNodeLimits.getMinX();
					__yAdjust = convertDrawingYToDataY(invertY(@event.getY())) + __screenBottomY - __draggedNodeLimits.getMinY();
				}

				if (__nodes[__clickedNodeNum].isSelected())
				{
					// if the node is already selected, retain the existing node selection.
				}
				else
				{
					// otherwise, the node is the only selected one
					for (int i = 0; i < __nodes.Length; i++)
					{
						__nodes[i].setSelected(false);
					}
					__nodes[__clickedNodeNum].setSelected(true);
				}

				buildSelectedNodesLimits();
			}
			__nodeDrag = true;
			forceRepaint();
		}
		else
		{
			// otherwise, check to ese if the legend was clicked on
			double x = convertDrawingXToDataX(@event.getX()) + __screenLeftX;
			double y = convertDrawingYToDataY(invertY(@event.getY())) + __screenBottomY;
			if (__legendDataLimits.contains(x, y))
			{
				if (!__editable)
				{
					return;
				}
				// ... if so, set everything up so it can be dragged.
				__legendDrag = true;
				__mouseDataX = convertDrawingXToDataX(@event.getX()) + __screenLeftX;
				__mouseDataY = convertDrawingYToDataY(invertY(@event.getY())) + __screenBottomY;
				__xAdjust = convertDrawingXToDataX(@event.getX()) + __screenLeftX - __legendDataLimits.getMinX();
				__yAdjust = convertDrawingYToDataY(invertY(@event.getY())) + __screenBottomY - __legendDataLimits.getMinY();
				__eraseLegend = true;
				forceRepaint();
			}
			else
			{
				if (__networkMouseMode == MODE_PAN)
				{
					// otherwise, prepare the screen to be moved around
					__screenDrag = true;
					__mouseDataX = convertDrawingXToDataX(@event.getX()) + __screenLeftX;
					__mouseDataY = convertDrawingYToDataY(invertY(@event.getY())) + __screenBottomY;
					repaint();
				}
				else
				{
					__drawingBox = true;
					__dragStartX = convertDrawingXToDataX(@event.getX()) + __screenLeftX;
					__dragStartY = convertDrawingYToDataY(invertY(@event.getY())) + __screenBottomY;
				}
			}
		}
	}

	/// <summary>
	/// Responds to mouse released events by placing a dragged node or the dragged 
	/// legend in its new position, or by showing a popup menu. </summary>
	/// <param name="event"> the MouseEvent that happened. </param>
	public virtual void mouseReleased(MouseEvent @event)
	{
		if (@event.isPopupTrigger())
		{
			// find the node on which the popup menu was triggered
			int nodeNum = findNodeOrAnnotationAtXY(convertDrawingXToDataX(@event.getX()) + __screenLeftX, convertDrawingYToDataY(invertY(@event.getY())) + __screenBottomY);
			__popupX = convertDrawingXToDataX(@event.getX()) + __screenLeftX;
			__popupY = convertDrawingYToDataY(invertY(@event.getY())) + __screenBottomY;

			if (nodeNum > -1)
			{
				// if nodeNum > -1 then a node was clicked on.
				// findNodeAtXY() checks to see if the node is an
				// annotation node or not, and sets __isAnnotation appropriately
				__popupNodeNum = nodeNum;
				if (__isLastSelectedAnAnnotation)
				{
					__annotationPopup.show(@event.getComponent(), @event.getX(), @event.getY());
				}
				else
				{
					if (nodeHasLinks())
					{
						__deleteLinkMenuItem.setEnabled(true);
					}
					else
					{
						__deleteLinkMenuItem.setEnabled(false);
					}
					if (inStateModGUI())
					{
						// Adding and deleting nodes is controlled through the Edit menu in the StateMod GUI
						// in order to ensure that other data manipulation occurs properly.
						__addNodeMenuItem.setEnabled(false);
						__addNodeMenuItem.setToolTipText("Use the Edit > Add menu in " + "StateMod GUI and then finish setting network node properties.");
						__deleteNodeMenuItem.setEnabled(false);
						__deleteNodeMenuItem.setToolTipText("Use the Edit > Delete menu in StateMod GUI.");
					}
					__nodePopup.show(@event.getComponent(), @event.getX(), @event.getY());
				}
			}
			else
			{
				// no node was clicked on
				__networkPopup.show(@event.getComponent(), @event.getX(), @event.getY());
			}
			return;
		}

		if (!__editable && !(__screenDrag || __drawingBox))
		{
			return;
		}

		if (__clickedNodeNum > -1)
		{
			// if a node was being dragged ...
			__nodeDrag = false;

			// make sure that the user didn't simply click on a node and
			// release the mouse button without moving.  Without the 
			// following check there's a good chance that the node will be moved slightly. 
			if (__mouseDeviceX == @event.getX() && __mouseDeviceY == @event.getY())
			{
				__clickedNodeNum = -1;
				forceRepaint();
				return;
			}

			if (!__snapToGrid)
			{
				__mouseDataX = convertDrawingXToDataX(@event.getX()) + __screenLeftX;
				__mouseDataY = convertDrawingYToDataY(invertY(@event.getY())) + __screenBottomY;
			}

			if (__isLastSelectedAnAnnotation)
			{
				// prevent nodes from being dragged off the drawing area completely.
				__mouseDataX -= __xAdjust;
				__mouseDataY -= __yAdjust;

				HydrologyNode node = __annotations[__clickedNodeNum];
				GRLimits data = __drawingArea.getDataLimits();
				if (__mouseDataX < data.getLeftX())
				{
					__mouseDataX = data.getLeftX() + node.getWidth() / 2;
				}
				if (__mouseDataY < data.getBottomY())
				{
					__mouseDataY = data.getBottomY() + node.getHeight() / 2;
				}
				if (__mouseDataX > data.getRightX())
				{
				__mouseDataX = data.getRightX() - node.getWidth() / 2;
				}
				if (__mouseDataY > data.getTopY())
				{
					__mouseDataY = data.getTopY() - node.getHeight() / 2;
				}

				nodeWasMoved();

				node.setX(__mouseDataX);
				node.setY(__mouseDataY);
				updateAnnotationLocation(__clickedNodeNum);
				node.setDirty(true);
				forceRepaint();
				__parent.displayNode(node);
			}
			else
			{
				moveDraggedNodes();
			}
			__clickedNodeNum = -1;
		}
		else if (__legendDrag)
		{
			// otherwise, if the legend was dragged ...
			__legendDrag = false;
			__eraseLegend = false;

			if (__snapToGrid)
			{
				// TODO SAM Evaluate logic
				//__mouseDataX = __mouseDataX;
				//__mouseDataY = __mouseDataY;
			}
			else
			{
				__mouseDataX = convertDrawingXToDataX(@event.getX()) + __screenLeftX - __xAdjust;
				__mouseDataY = convertDrawingYToDataY(invertY(@event.getY())) + __screenBottomY - __yAdjust;
			}

			// prevent the legend from being dragged off the edge of the screen
			GRLimits data = __drawingArea.getDataLimits();
			if (__mouseDataX < data.getLeftX())
			{
				__mouseDataX = data.getLeftX() + 10;
			}
			if (__mouseDataY < data.getBottomY())
			{
				__mouseDataY = data.getBottomY() + 10;
			}
			if (__mouseDataX > data.getRightX())
			{
				__mouseDataX = data.getRightX() + 10;
			}
			if (__mouseDataY > data.getTopY())
			{
				__mouseDataY = data.getTopY() + 10;
			}
			if (__snapToGrid)
			{
				__legendDataLimits.setLeftX(__mouseDataX - __xAdjust);
				__legendDataLimits.setBottomY(__mouseDataY - __yAdjust);
			}
			else
			{
				__legendDataLimits.setLeftX(__mouseDataX);
				__legendDataLimits.setBottomY(__mouseDataY);
			}
			forceRepaint();
		}
		else if (__screenDrag)
		{
			// cancel the screen drag
			__screenDrag = false;
		}
		else if (__drawingBox)
		{
			// finish drawing the box and select all the nodes that were within it
			__drawingBox = false;

			double lx, rx, ty, by;
			if (__currDragX < __dragStartX)
			{
				lx = __currDragX;
				rx = __dragStartX;
			}
			else
			{
				rx = __currDragX;
				lx = __dragStartX;
			}

			if (__currDragY < __dragStartY)
			{
				by = __currDragY;
				ty = __dragStartY;
			}
			else
			{
				ty = __currDragY;
				by = __dragStartY;
			}

			GRLimits limits = new GRLimits(lx, by, rx, ty);

			for (int i = 0; i < __nodes.Length; i++)
			{
				if (within(limits, __nodes[i]))
				{
					__nodes[i].setSelected(true);
				}
				else
				{
					__nodes[i].setSelected(false);
				}
			}
			forceRepaint();
		}
	}

	/// <summary>
	/// Does the actual work of moving a node from one location to another. </summary>
	/// <param name="num"> the index of the node (in the __nodes array) to be moved. </param>
	private void moveDraggedNode(int num)
	{
		double mainX = __mouseDataX - __xAdjust;
		double mainY = __mouseDataY - __yAdjust;

		double dx = mainX - __mouseDownX;
		double dy = mainY - __mouseDownY;

		double nodeX = __draggedNodesXs[num] + dx;
		double nodeY = __draggedNodesYs[num] + dy;

		if (__snapToGrid)
		{
			__nodes[__draggedNodes[num]].setX(nodeX + __nodes[__draggedNodes[num]].getWidth() / 2);
			__nodes[__draggedNodes[num]].setY(nodeY + __nodes[__draggedNodes[num]].getHeight() / 2);
		}
		else
		{
			__nodes[__draggedNodes[num]].setX(nodeX);
			__nodes[__draggedNodes[num]].setY(nodeY);
		}
	}

	/// <summary>
	/// Changes the position of dragged nodes once the mouse button is released on a drag.
	/// </summary>
	private void moveDraggedNodes()
	{
		// Prevent nodes from being dragged off the drawing area completely.
		GRLimits data = __drawingArea.getDataLimits();
		if (__mouseDataX < data.getLeftX())
		{
			__mouseDataX = data.getLeftX() + __nodes[__clickedNodeNum].getWidth() / 2;
		}
		if (__mouseDataY < data.getBottomY())
		{
			__mouseDataY = data.getBottomY() + __nodes[__clickedNodeNum].getHeight() / 2;
		}
		if (__mouseDataX > data.getRightX())
		{
			__mouseDataX = data.getRightX() - __nodes[__clickedNodeNum].getWidth() / 2;
		}
		if (__mouseDataY > data.getTopY())
		{
			__mouseDataY = data.getTopY() - __nodes[__clickedNodeNum].getHeight() / 2;
		}

		if (__draggedNodes == null || __draggedNodes.Length == 0)
		{
			nodeWasMoved();
		}
		else
		{
			createMultiNodeChangeOperation();
		}

		__nodes[__clickedNodeNum].setX(__mouseDataX);
		__nodes[__clickedNodeNum].setY(__mouseDataY);
		__nodes[__clickedNodeNum].setDirty(true);
	//	Message.printStatus(1, "", "Node moved, set dirty: " + true);

		for (int i = 0; i < __draggedNodes.Length; i++)
		{
			moveDraggedNode(i);
		}

		forceRepaint();
		__parent.displayNode(__nodes[__clickedNodeNum]);
	//	__nodes[__clickedNodeNum].setSelected(false);
	}

	/// <summary>
	/// Checks to see if a node has any links. </summary>
	/// <returns> true if the node has links, otherwise false. </returns>
	private bool nodeHasLinks()
	{
		if (__links == null)
		{
			return false;
		}
		string s = null;
		string id = __nodes[__popupNodeNum].getCommonID();
		foreach (PropList p in this.__links)
		{
			s = p.getValue("FromNodeID");
			if (s.Equals(id))
			{
				return true;
			}
			s = p.getValue("ToNodeID");
			if (s.Equals(id))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Called when a node was moved internally.  Creates a node change operation.
	/// </summary>
	private void nodeWasMoved()
	{
		addNodeChangeOperation(__clickedNodeNum, __mouseDataX, __mouseDataY);
	}

	/// <summary>
	/// Paints the screen. </summary>
	/// <param name="g"> the Graphics context to use for painting. </param>
	public virtual void paint(Graphics g)
	{
		// When printing is called, the print(Graphics ...) routine ends up
		// getting called three times.  Just a foible of Java, that routine
		// gets called more than once, even if just one page should be printed.
		// Multiple calls to print can result in some weirdly-drawing things,
		// plus they slow it down.  The following check makes sure that when
		// the network is being printed, it is only drawn one time.
		//int dl=5;
		string routine = "StateMod_Network_JComponenet.paint";
		if (this.__printingNetwork && !this.__useOldPrinting)
		{
			// Printing the network using new code.  Just print without all the interactive checks
			// The drawing limits, font size, etc, will have been set in the print(...) method since that
			// has information about the selected paper size
			// Graphics2D object was already set in the print() method before calling this method.
			//drawTestPage ( g );
			// The font size in points for full-scale printing is from the network
			string nodeLabelFontSizeString = getSelectedPageLayoutPropList().getValue("NodeLabelFontSize");
			if (string.ReferenceEquals(nodeLabelFontSizeString, null))
			{
				nodeLabelFontSizeString = "10";
			}
			double fontSize = double.Parse(nodeLabelFontSizeString);
			this.__drawingArea.setFont("Helvetica", "Plain", (int)fontSize);
			setAntiAlias(__antiAlias);
			// Set the node size
			string nodeSizeString = getSelectedPageLayoutPropList().getValue("NodeSize");
			if (string.ReferenceEquals(nodeSizeString, null))
			{
				nodeSizeString = "10";
			}
			double nodeSize = double.Parse(nodeSizeString);
			setPrintNodeSize(nodeSize);
			// The following does bad things
			//scaleUnscalables();
			for (int i = 0; i < __nodes.Length; i++)
			{
				__nodes[i].calculateExtents(__drawingArea);
			}
			drawNodes(getDrawingArea(), getNodesArray());
			drawNetworkLines();
			drawLinks(getDrawingArea(), getLinkList(), getNodesArray(), getDashPattern());
			drawLegend();
			drawAnnotations();
			return;
		}
		if (this.__printCount > 0)
		{
			// Apparently this is only an issue with in-memory image drawing but for printing multiple
			// calls are needed to complete rendering (?).
			return;
		}
		// Sets the graphics in the base class appropriately (double-buffered
		// if doing double-buffered drawing, single-buffered if not)
		if (__printingNetwork)
		{
			Font f = __drawingArea.getFont();
			__bufferGraphics.setFont(new Font(f.getName(), f.getStyle(), __printFontSizePoints));
			forceGraphics(__bufferGraphics);
			__bufferGraphics.setFont(new Font(f.getName(), f.getStyle(), __printFontSizePoints));
		}
		else if (__printingScreen || __savingNetwork || __savingScreen)
		{
			// force the graphics to use the double-buffer graphics
			forceGraphics(__bufferGraphics);
		}
		else
		{
			// normal paint, just use the provided graphics
			setGraphics(g);
		}

		setAntiAlias(__antiAlias);

		// First time ever through, do the following ...
		if (!__initialized)
		{
			__initialized = true;

			__drawingAreaHeight = getBounds().height;
			__drawingAreaWidth = getBounds().width;

			Font f = __drawingArea.getFont();
			__drawingArea.setFont(f.getName(), f.getStyle(), 10);

			if (!__printingNetwork)
			{
				setupDoubleBuffer(0, 0, getBounds().width, getBounds().height);
			}
			GRLimits limits = new GRLimits(0, 0, getBounds().width, getBounds().height);
			__drawingArea.setDrawingLimits(limits, GRUnits.DEVICE, GRLimits.DEVICE);

			GRLimits data = null;

			// Determine the datalimits to be drawn in the current screen
			if (__fitWidth)
			{
				double pct = ((double)(getBounds().height)) / ((double)(getBounds().width));
				double height = pct * __totalDataWidth;
				data = new GRLimits(__dataLeftX, __dataBottomY, __dataLeftX + __totalDataWidth, __dataBottomY + height);
			}
			else
			{
				double pct = ((double)(getBounds().width)) / ((double)(getBounds().height));
				double width = pct * __totalDataHeight;
				data = new GRLimits(__dataLeftX, __dataBottomY, __dataLeftX + width, __dataBottomY + __totalDataHeight);
			}

			__screenLeftX = __dataLeftX;
			__screenBottomY = __dataBottomY;
			__screenDataWidth = data.getWidth();
			__screenDataHeight = data.getHeight();
			__drawingArea.setDataLimits(data);

			// TODO (JTS - 2004-04-05) On the subject of node sizes:
			// Nodes are drawn in data units, so that whatever is specified
			// as the print node size is the number of pixels across the
			// node will appear on screen.  However, because of the magic
			// of different DPIs for print-outs and screen displays,
			// this will need adjusted.  On a PC, 48 pixels across is
			// one half an inch.  On the printed page, a half inch is
			// 36 pixels across. Whatever the final node size is, it 
			// will need adjusted to fit the printed DPI.  

			// and that doesn't even BEGIN to get into the problems with points vs. pixels ...

			setPrintNodeSize(__currNodeSize);

			for (int i = 0; i < __nodes.Length; i++)
			{
				__nodes[i].calculateExtents(__drawingArea);
			}

			// Odd-looking, but it works.  With the above things in place,
			// it recalls this method (but this time the initialization
			// section won't be entered).  That sets up some more things
			// and then the NEXT time through, a complete refresh is 
			// forced just to make sure it all drew properly.
			zoomOneToOne();

			// set the grid step to be 1/8s of an inch
			if (__fitWidth)
			{
				__gridStep = convertDrawingXToDataX(__dpi / 8);
			}
			else
			{
				__gridStep = convertDrawingYToDataY(__dpi / 8);
			}

			repaint();
			__forceRefresh = true;
			scaleUnscalables();
		}
		else
		{
			// Check to see if the bounds of the device have changed --
			// if they have then the GUI window has been resized and
			// the double buffer size needs changed accordingly.
			if (__drawingAreaHeight != getBounds().height || __drawingAreaWidth != getBounds().width)
			{
				adjustForResize();
				__drawingAreaHeight = getBounds().height;
				__drawingAreaWidth = getBounds().width;
				GRLimits limits = new GRLimits(0, 0, getBounds().width, getBounds().height);
				__drawingArea.setDrawingLimits(limits, GRUnits.DEVICE, GRLimits.DEVICE);
				setupDoubleBuffer(0, 0, getBounds().width, getBounds().height);
				__forceRefresh = true;
			}
		}

		// The following section is for when networks are read in from
		// XML files.  The values below are read in after the GUI is
		// instantiated, and are set so that whenever the GUI finds that
		// one of them is set during a repaint it will apply them to the gui settings.
		bool repaint = false;
		if (!string.ReferenceEquals(__holdPaperSize, null))
		{
			setPaperSize(__holdPaperSize);
			repaint = true;
		}
		if (!string.ReferenceEquals(__holdPaperOrientation, null))
		{
			setOrientation(__holdPaperOrientation);
			repaint = true;
		}
		if (__holdPrintNodeSize != -1)
		{
			setPrintNodeSize(__holdPrintNodeSize);
			repaint = true;
		}
		if (__holdPrintFontSize != -1)
		{
			setPrintFontSize(__holdPrintFontSize);
			repaint = true;
		}

		if (repaint)
		{
			__forceRefresh = true;
		}

		// Only do the following if explicitly instructed to ...
		if (__forceRefresh)
		{
			Font f = __drawingArea.getFont();
			// A normal paint() call -- translate the screen so the proper
			// portion is drawn in the screen and set up the drawing limits.
			if (!__printingNetwork && !__printingScreen && !__savingNetwork && !__savingScreen)
			{
				__drawingArea.setFont(f.getName(), f.getStyle(), __fontSizePoints);
				setLimits(getLimits(true));
				__drawingArea.setDataLimits(new GRLimits(__screenLeftX, __screenBottomY, __screenLeftX + __screenDataWidth, __screenBottomY + __screenDataHeight));
				clear();
			}
			// If printing the entire network, do a translation so that the
			// entire network is drawn in the BufferedImage.  No X change 
			// is needed, but the bottom of the network needs aligned properly.
			else if (__printingNetwork)
			{
				__holdLimits = __drawingArea.getDataLimits();
				GRLimits data = new GRLimits(__dataLeftX, __dataBottomY, __dataLeftX + __totalDataWidth, __dataBottomY + __totalDataHeight);
				__drawingArea.setDataLimits(data);
				__drawingArea.setDrawingLimits(new GRLimits(0, 0, __totalBufferWidth, __totalBufferHeight), GRUnits.DEVICE, GRLimits.DEVICE);
				translate(0, __totalBufferHeight - getBounds().height);
				scaleUnscalables();
				clear();
				if (this.__useOldPrinting)
				{
					__bufferGraphics.setFont(new Font(f.getName(), f.getStyle(), __printFontSizePoints));
				}
				//  Message.printDebug(dl, routine, "dataLimits: " + __drawingArea.getDataLimits().toString());
				//  Message.printDebug(dl, routine, "drawingLimits: " + __drawingArea.getDrawingLimits().toString());
			}
			else if (__savingNetwork)
			{
				__holdLimits = __drawingArea.getDataLimits();
				GRLimits data = new GRLimits(__dataLeftX, __dataBottomY, __dataLeftX + __totalDataWidth, __dataBottomY + __totalDataHeight);
				__drawingArea.setDataLimits(data);
				__drawingArea.setDrawingLimits(new GRLimits(0, 0, __totalBufferWidth * (72.0 / (double)__dpi), __totalBufferHeight * (72.0 / (double)__dpi)), GRUnits.DEVICE, GRLimits.DEVICE);
				translate(0, (int)(__totalBufferHeight * (72.0 / (double)__dpi)) - getBounds().height);
				clear();
				__drawingArea.setFont(f.getName(), f.getStyle(), __fontSizePoints);
			}
			// if just the current screen is drawn, the same translation 
			// can be done that was done for normal drawing.
			else if (__printingScreen || __savingScreen)
			{
				clear();
				__drawingArea.setFont(f.getName(), f.getStyle(), __fontSizePoints);
			}

			// Draw annotations below the network since node size is exaggerated
			try
			{
				foreach (StateMod_Network_AnnotationData annotationData in getAnnotationData())
				{
					StateMod_Network_AnnotationRenderer annotationRenderer = annotationData.getStateModNetworkAnnotationRenderer();
					annotationRenderer.renderStateModNetworkAnnotation(this, annotationData);
				}
			}
			catch (Exception e)
			{
				Message.printWarning(3, routine, "Error drawing annotations (" + e + ").");
				Message.printWarning(3, routine, e);
			}

			setAntiAlias(__antiAlias);
			drawNetworkLines();
			setAntiAlias(__antiAlias);
			drawLinks(getDrawingArea(), getLinkList(), getNodesArray(), getDashPattern());
			setAntiAlias(__antiAlias);
			__drawingArea.setFont(f.getName(), f.getStyle(), __fontSizePoints);
			drawNodes(getDrawingArea(), getNodesArray());

			setAntiAlias(__antiAlias);
			if (!__eraseLegend)
			{
				drawLegend();
			}

			setAntiAlias(__antiAlias);
			drawAnnotations();

			// If the grid should be drawn, do so ...
			setAntiAlias(__antiAlias);
			if (__drawInchGrid)
			{
				// Change the limits so that the drawing is done in device units, not data units
				__drawingArea.setFloatLineDash(__dashes, 0);

				double maxX = __totalBufferWidth;
				double maxY = __totalBufferHeight;

				GRDrawingAreaUtil.setColor(__drawingArea, GRColor.red);
				int j = 0;
				int minY = (int)(convertAbsY(0) + __dataBottomY);
				int tempMaxY = (int)(convertAbsY(maxY) + __dataBottomY);
				for (int i = 0; i < maxX; i += ((72 / __printScale) / 2))
				{
					j = (int)(convertAbsX(i) + __dataLeftX);
					GRDrawingAreaUtil.drawLine(__drawingArea, j, minY, j, tempMaxY);
					GRDrawingAreaUtil.drawText(__drawingArea, "" + ((double)i / (72 / __printScale)),j, minY, 0, GRText.CENTER_X | GRText.BOTTOM);
				}

				int minX = (int)(convertAbsX(0) + __dataLeftX);
				int tempMaxX = (int)(convertAbsX(maxX) + __dataLeftX);
				for (int i = 0; i < maxY; i += ((72 / __printScale) / 2))
				{
					j = (int)(convertAbsY(i) + __dataBottomY);
					GRDrawingAreaUtil.drawLine(__drawingArea, minX, j, tempMaxX, j);
					GRDrawingAreaUtil.drawText(__drawingArea, "" + ((double)i / (72 / __printScale)),minX, j, 0, GRText.CENTER_Y | GRText.LEFT);
				}

				// Set the data limits back
				__drawingArea.setFloatLineDash(null, 0);
			}
			setAntiAlias(__antiAlias);
			if (__drawPixelGrid)
			{
				for (int i = (int)__dataBottomY; i < __totalDataHeight; i += 20)
				{
					GRDrawingAreaUtil.setColor(__drawingArea, GRColor.green);
					GRDrawingAreaUtil.drawLine(__drawingArea, __dataLeftX, i, __totalDataWidth, i);
					for (int j = i + 5; j < (i + 20); j += 5)
					{
						GRDrawingAreaUtil.setColor(__drawingArea, GRColor.yellow);
						GRDrawingAreaUtil.drawLine(__drawingArea, __dataLeftX, j, __totalDataWidth, j);
					}
				}
			}
			setAntiAlias(__antiAlias);
			if (__drawMargin)
			{
				__drawingArea.setFloatLineDash(__dashes, 0);
				GRDrawingAreaUtil.setColor(__drawingArea, GRColor.cyan);

				double leftX = __pageFormat.getImageableX() / __printScale;
				double topY = (__pageFormat.getHeight() - __pageFormat.getImageableY()) / __printScale;
				double rightX = (leftX + __pageFormat.getImageableWidth() / __printScale) - 1;
				double bottomY = ((__pageFormat.getHeight() - (__pageFormat.getImageableY() + __pageFormat.getImageableHeight())) / __printScale) + 1;
				leftX = convertAbsX(leftX) + __dataLeftX;
				topY = convertAbsY(topY) + __dataBottomY;
				rightX = convertAbsX(rightX) + __dataLeftX;
				bottomY = convertAbsY(bottomY) + __dataBottomY;

				GRDrawingAreaUtil.drawLine(__drawingArea, leftX, topY, leftX, bottomY);
				GRDrawingAreaUtil.drawLine(__drawingArea, rightX, topY, rightX, bottomY);
				GRDrawingAreaUtil.drawLine(__drawingArea, leftX, topY, rightX, topY);
				GRDrawingAreaUtil.drawLine(__drawingArea, leftX, bottomY, rightX, bottomY);

				__drawingArea.setFloatLineDash(null, 0);
			}
			setAntiAlias(__antiAlias);
			if (true && !__savingNetwork)
			{
				GRDrawingAreaUtil.setColor(__drawingArea, GRColor.yellow);

				double leftX = 0;
				double topY = (__pageFormat.getHeight() / __printScale);
				double rightX = (__pageFormat.getWidth() / __printScale);
				double bottomY = 0;
				leftX = convertAbsX(leftX) + __dataLeftX;
				topY = convertAbsY(topY) + __dataBottomY - 1;
				rightX = convertAbsX(rightX) + __dataLeftX - 1;
				bottomY = convertAbsY(bottomY) + __dataBottomY;

				GRDrawingAreaUtil.drawLine(__drawingArea, leftX, topY, leftX, bottomY);
				GRDrawingAreaUtil.drawLine(__drawingArea, rightX, topY, rightX, bottomY);
				GRDrawingAreaUtil.drawLine(__drawingArea, leftX, topY, rightX, topY);
				GRDrawingAreaUtil.drawLine(__drawingArea, leftX, bottomY, rightX, bottomY);
			}

			// Make sure the reference window represents the current status of this window
			if (__referenceJComponent != null)
			{
				__referenceJComponent.forceRepaint();
			}
			__forceRefresh = false;
		}

		if (!__printingNetwork && !__savingNetwork && !__printingScreen && !__savingScreen)
		{
			// Draw the border lines that separate the drawing area from the rest of the GUI.

			GRDrawingAreaUtil.setColor(__drawingArea, GRColor.black);
			GRDrawingAreaUtil.drawLine(__drawingArea, __screenLeftX, __screenBottomY, __screenLeftX, __screenBottomY + __screenDataHeight);
			GRDrawingAreaUtil.drawLine(__drawingArea, __screenLeftX, __screenBottomY + __screenDataHeight, __screenLeftX + __screenDataWidth, __screenBottomY + __screenDataHeight);

			// The lower- and right-side lines need to be drawn with a
			// width of 2 so that they appear, otherwise they are just
			// the other side of drawing are and not visible.  The top-
			// and left-side lines appear fine normally.
			GRDrawingAreaUtil.setLineWidth(__drawingArea, 2);
			GRDrawingAreaUtil.drawLine(__drawingArea, __screenLeftX + __screenDataWidth, __screenBottomY, __screenLeftX + __screenDataWidth, __screenBottomY + __screenDataHeight);
			GRDrawingAreaUtil.drawLine(__drawingArea, __screenLeftX, __screenBottomY, __screenLeftX + __screenDataWidth, __screenBottomY);
			GRDrawingAreaUtil.setLineWidth(__drawingArea, 1);
		}

		setAntiAlias(__antiAlias);
		// Only show the double buffered image to screen if not printing
		if (!__printingNetwork && !__printingScreen && !__savingNetwork && !__savingScreen)
		{
			showDoubleBuffer(g);
		}
		else if (__printingScreen || __savingScreen)
		{
			return;
		}
		else
		{
			// return here because nodes can't ever be in drag when printing occurs.
			__drawingArea.setDataLimits(__holdLimits);
			__drawingArea.setDrawingLimits(new GRLimits(0, 0, getBounds().width, getBounds().height), GRUnits.DEVICE, GRLimits.DEVICE);
			scaleUnscalables();
			return;
		}
		setAntiAlias(__antiAlias);
		// If a node is currently being dragged around the screen, draw the
		// outline of the table on top of the double-buffer
		if (__nodeDrag)
		{
			drawNodesOutlines(g);
		}
		else if (__legendDrag)
		{
			// force the graphics context to be the on-screen one, not the double-buffered one
			forceGraphics(g);
			GRDrawingAreaUtil.setColor(__drawingArea, GRColor.black);
			GRDrawingAreaUtil.drawLine(__drawingArea, __mouseDataX - __xAdjust, __mouseDataY - __yAdjust, __legendDataLimits.getWidth() + __mouseDataX - __xAdjust, __mouseDataY - __yAdjust);
			GRDrawingAreaUtil.drawLine(__drawingArea, __mouseDataX - __xAdjust, __mouseDataY + __legendDataLimits.getHeight() - __yAdjust, __legendDataLimits.getWidth() + __mouseDataX - __xAdjust, __mouseDataY + __legendDataLimits.getHeight() - __yAdjust);
			GRDrawingAreaUtil.drawLine(__drawingArea, __mouseDataX - __xAdjust, __mouseDataY - __yAdjust, __mouseDataX - __xAdjust, __mouseDataY + __legendDataLimits.getHeight() - __yAdjust);
			GRDrawingAreaUtil.drawLine(__drawingArea, __mouseDataX + __legendDataLimits.getWidth() - __xAdjust, __mouseDataY - __yAdjust, __mouseDataX + __legendDataLimits.getWidth() - __xAdjust, __mouseDataY + __legendDataLimits.getHeight() - __yAdjust);
		}
		else if (__drawingBox)
		{
			g.setXORMode(Color.white);
			forceGraphics(g);
			GRDrawingAreaUtil.setColor(__drawingArea, GRColor.cyan);
			GRDrawingAreaUtil.drawLine(__drawingArea, __dragStartX, __dragStartY, __dragStartX, __currDragY);
			GRDrawingAreaUtil.drawLine(__drawingArea, __currDragX, __dragStartY, __currDragX, __currDragY);
			GRDrawingAreaUtil.drawLine(__drawingArea, __dragStartX, __dragStartY, __currDragX, __dragStartY);
			GRDrawingAreaUtil.drawLine(__drawingArea, __dragStartX, __currDragY, __currDragX, __currDragY);
		}
	}

	/// <summary>
	/// Returns whether the network is displayed in StateModGUI or not. </summary>
	/// <returns> true if the network is in StateModGUI. </returns>
	public virtual bool inStateModGUI()
	{
		return __parent.inStateModGUI();
	}

	/// <summary>
	/// Sets up a print job and submits it.
	/// </summary>
	public virtual void print()
	{
		bool useOldCode = false;
		try
		{
			if (useOldCode)
			{
				PrinterJob printJob = PrinterJob.getPrinterJob();
				printJob.setPrintable(this, __pageFormat);
				PrintUtil.print(this, __pageFormat);
			}
			else
			{
				// Create a new StateMod_Network_JComponent that is isolated from the interactive plotting.
				// It is OK to use the same network because printing won't change anything
				// Use the current page layout for the settings
				StateMod_Network_JComponent networkPrintable = new StateMod_Network_JComponent(getNetwork(), __parent.getSelectedPageLayout());
				networkPrintable.initializeForPrinting();
				// Get the attributes needed for the printer job
				networkPrintable.initializeForNetworkPageLayout(__parent.getSelectedPageLayout());
				string orientation = __parent.getSelectedOrientation();
				string paperSizeFromLayout = __parent.getSelectedPaperSize();
				if (paperSizeFromLayout.IndexOf(" ", StringComparison.Ordinal) > 0)
				{
					// Have Size - hxw
					paperSizeFromLayout = StringUtil.getToken(paperSizeFromLayout, " ", 0, 0).Trim();
				}
				double margin = networkPrintable.getMargin();
				// Open the printer job, which will display the print dialog and allow the user to change
				// settings (mostly they should just pick the printer.
				new GraphicsPrinterJob(networkPrintable, "Network", null, null, null, orientation, margin, margin, margin, margin, null, true); // show print configuration dialog
			}
		}
		catch (Exception e)
		{
			string routine = "StateMod_Network_JComponent.print()";
			Message.printWarning(1, routine, "Error printing network (" + e + ").");
			Message.printWarning(3, routine, e);
		}
	}

	/// <summary>
	/// Prints a page. </summary>
	/// <param name="g"> the Graphics context to which to print. </param>
	/// <param name="pageFormat"> the pageFormat to use for printing. </param>
	/// <param name="pageIndex"> the index of the page to print. </param>
	/// <returns> Printable.NO_SUCH_PAGE if no page should be printed, or 
	/// Printable.PAGE_EXISTS if a page should be printed. </returns>
	public virtual int print(Graphics g, PageFormat pageFormat, int pageIndex)
	{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		string routine = this.GetType().FullName + ".print";
		if (pageIndex > 0)
		{
			return NO_SUCH_PAGE;
		}
		// Save the graphics and page format for use elsewhere in this class
		Graphics2D g2d = (Graphics2D)g;
		setGraphics(g2d);
		this.__pageFormat = pageFormat;
		// Set the drawing area to the imageable area, which is determined from the pageFormat from
		// the PrinterJob, etc., that controls the call to this method
		double pageHeight = pageFormat.getHeight();
		double imageablePageHeight = pageFormat.getImageableHeight();
		double pageWidth = pageFormat.getWidth();
		double imageablePageWidth = pageFormat.getImageableWidth();
		double imageableX = pageFormat.getImageableX();
		double imageableY = pageFormat.getImageableY();
		Message.printStatus(2, routine, "Page dimensions are: width=" + pageWidth + " height=" + pageHeight);
		Message.printStatus(2, routine, "Imageable page dimensions are: width=" + imageablePageWidth + " height=" + imageablePageHeight);
		Message.printStatus(2, routine, "Imageable origin: X=" + imageableX + " Y=" + imageableY);
		// Set the device (page limits)
		GRLimits deviceLimits = new GRLimits(0,0,pageWidth,pageHeight);
		setLimits(deviceLimits);
		// Set the drawing limits on the page to the imageable area...
		GRLimits drawingLimits = new GRLimits(imageableX, (pageHeight - imageableY - imageablePageHeight), (imageableX + imageablePageWidth), (pageHeight - imageableY));
		this.__drawingArea = new GRJComponentDrawingArea(this, "StateMod_Network DrawingArea", GRAspect.TRUE, drawingLimits, GRUnits.DEVICE, GRLimits.DEVICE, getDataLimits());
		Message.printStatus(2, routine, "Print drawing area limits (from printer imageable area): " + drawingLimits);
		Message.printStatus(2, routine, "Print data limits (from network data): " + getDataLimits());
		Message.printStatus(2, routine, "Print font: " + this.__drawingArea.getFont());
		StateMod_NodeNetwork network = getNetwork();
		// Calculate the data limits necessary to maintain aspect of the data and fit the page for full scale
		this.__drawingAreaFullScale = createDrawingAreaFullScale(network, getSelectedPageLayoutPropList());
		// Calculate the data limits necessary to maintain aspect of the data and fit the page
		// Use the actual network data
		this.__drawingArea.setDataLimits(calculateDataLimitsForMedia(getDrawingArea().getDrawingLimits(), new GRLimits(network.getLX(),network.getBY(),network.getRX(),network.getTY()), network.getEdgeBuffer(), true));
		// Set the node icon size based on the scale.

		// Set the font size for the scale...
		// TODO SAM 2011-07-08 Only full scale printing currently is supported
		//int fontSize = this.__drawingArea.calculateFontSize(g2d, this.__fontSizePixels);

		// TODO SAM 2011-07-05 Adjust the drawing limits accordingly to center on the imageable area when
		// the selected page size does not match the layout size
		// For full-sized printing it should fit as is with no scale adjustments

		// This is from TSGraphJComponent.paint(), which contains inline comments to explain.
		// The imageable Y is the first Java pixel (going DOWN) at which drawing can occur.
		// It is therefore the LAST GR=package pixel at which drawing can occur.
		// The bottom margin of the graph is already enforced by setting the size of the drawing area.
		// This makes sure the graph is shifted to fit in the printable area of the page.
		//double transY = pageFormat.getImageableY();
		//g2d.translate(0, transY);
		// The paint() method and subsequent calls will pick up on __printingNetwork = true
		// and adjust accordingly
		paint(g2d);

		/*
		double hold = __currNodeSize;
		double pct = 72.0 / (double)__dpi;
		setPrintNodeSize((double)(__currNodeSize / pct));
		__legendNodeDiameter = hold;
		Graphics2D g2d = (Graphics2D)g;
		// Set for the GRDevice because we will temporarily use that to do the drawing...	
	
		__forceRefresh = true;
		
		Message.printStatus(2, "", "Printing network, printScale=" + __printScale);
		
		if (!__printingScreen) {
			g2d.scale(__printScale, __printScale);  // This doesn't appear to make any difference? - CEN
			// Not sure RepaintManger is effective in this case
			RepaintManager currentManager = RepaintManager.currentManager(this);
			currentManager.setDoubleBufferingEnabled(false);
			// Don't need? because printNetwork and printScreen both call forceRepaint()
			//paint(g2d);
			// __tempBuffer was created for purpose of printing and already has content
			g2d.drawImage(__tempBuffer, 0, 0, null);
			currentManager.setDoubleBufferingEnabled(true);
		}
		else {
			double transX = 0;
			double transY = 0;
		
			if (!StringUtil.startsWithIgnoreCase(PrintUtil.pageFormatToString(pageFormat), "Plotter")) {
				// TODO SAM 2009-02-10 What does the following accomplish?
				if (pageFormat.getOrientation() == PageFormat.LANDSCAPE) {
					transX = pageFormat.getImageableX() * (1 / 1);
					transY = pageFormat.getImageableY() * (1 / 1);
				}
				else {
					transX = pageFormat.getImageableX() * (1 / 1);
					transY = pageFormat.getImageableY() * (1 / 1);
				}
			}
	
			double iw = pageFormat.getImageableWidth();
			double w = pageFormat.getWidth();
			double ih = pageFormat.getImageableHeight();
			double h = pageFormat.getHeight();
	
			double scale = 0;
	
			if (w > h) {
				scale = iw / getBounds().width;
			}
			else {
				scale = ih / getBounds().height;
			}
	
			g2d.translate(transX, transY);
			paint(g2d);
		}*/
		__printCount++;

		return PAGE_EXISTS;
	}

	/// <summary>
	/// Prints the entire network.
	/// </summary>
	protected internal virtual void printNetwork()
	{
		string routine = "StateMod_Network_JComponent.printNetwork";
		Message.printStatus(2, routine, "Printing entire network");
		if (!__useOldPrinting)
		{
			// New printing creates a new instance of the component so that printing does not intermingle
			// with the interactive rendering
			new ResponseJDialog(__parent, "Print Network", "You must select a printer and page size that match the network page layout.\n" + "The printer dialog will take a few seconds to display.", ResponseJDialog.OK);
			print();
			return;
		}
		// Save current settings...
		bool drawMargin = __drawMargin;
		double zoom = __zoomPercentage;
		// Setup for printing...
		RepaintManager currentManager = printNetworkSetup();
		// Print...
		print();
		// Now shift back to normal interaction
		// TODO SAM 2011-07-03 Need to separate out printing completely so it does not
		// mess with interactive network editor.
		__tempBuffer = null;
		__printingNetwork = false;
		currentManager.setDoubleBufferingEnabled(true);
		System.GC.Collect();
		__printCount = 0;

		__ignoreRepaint = true;

		// go back to the previous zoom level
		if (zoom > 100)
		{
			for (double d = 100; d < zoom; d *= 2)
			{
				zoomIn();
			}
		}
		else if (zoom == 100)
		{
		}
		else
		{
			for (double d = 100; d > zoom; d /= 2)
			{
				zoomOut();
			}
		}

		__ignoreRepaint = false;

		__drawMargin = drawMargin;
		__antiAlias = __antiAliasSetting;
		forceRepaint();
	}

	/// <summary>
	/// Performs steps prior to printing the entire network.  This can be called in batch mode before
	/// printing the network.
	/// </summary>
	public virtual RepaintManager printNetworkSetup()
	{
		RepaintManager currentManager = RepaintManager.currentManager(this);
		currentManager.setDoubleBufferingEnabled(false);
		__drawMargin = false;
	//	__antiAlias = false;
		__antiAlias = true; // On for printing, to look better

		__printCount = 0;
		__tempBuffer = new BufferedImage(__totalBufferWidth, __totalBufferHeight, BufferedImage.TYPE_4BYTE_ABGR);
		__bufferGraphics = (Graphics2D)(__tempBuffer.createGraphics());
		// TODO SAM 2011-07-04 Had to put the following in for batch processing
		// because graphics is used for checking font size, etc.
		if (_graphics == null)
		{
			_graphics = __bufferGraphics;
		}
		__printingNetwork = true;

		// Make sure that none of the nodes are selected so they don't print blue (print without selection).  
		for (int i = 0; i < __nodes.Length; i++)
		{
			__nodes[i].setSelected(false);
		}
		zoomOneToOne();
		// Redraw with printer settings...
		forceRepaint();
		return currentManager;
	}

	/// <summary>
	/// Prints information about the nodes in the network to status level 2.  Used for debugging.
	/// </summary>
	private void printNetworkInfo()
	{
		if (__network == null)
		{
			return;
		}
		IList<string> v = __network.getNodeCountsVector();
		Message.printStatus(2, "StateMod_Network_JComponent.printNetworkInfo", "--- Network Node Summary ---");
		foreach (string s in v)
		{
			Message.printStatus(2, "StateMod_Network_JComponent.printNetworkInfo", "" + s);
		}
	}

	/// <summary>
	/// Prints whatever is visible on the screen, scaled to fit the default piece of paper.
	/// </summary>
	protected internal virtual void printScreen()
	{
		__printCount = 0;
		double leftX = __pageFormat.getImageableX() / __printScale;
		double topY = (__pageFormat.getHeight() - __pageFormat.getImageableY()) / __printScale;
		double rightX = (leftX + __pageFormat.getImageableWidth() / __printScale) - 1;
		double bottomY = ((__pageFormat.getHeight() - (__pageFormat.getImageableY() + __pageFormat.getImageableHeight())) / __printScale) + 1;
		__tempBuffer = new BufferedImage((int)(rightX - leftX), (int)(topY - bottomY), BufferedImage.TYPE_4BYTE_ABGR);
		__bufferGraphics = (Graphics2D)(__tempBuffer.createGraphics());
		__printingScreen = true;
		forceRepaint();
		print();
		__tempBuffer = null;
		__printingScreen = false;
		System.GC.Collect();
		__printCount = 0;
		forceRepaint();
	}

	/// <summary>
	/// Processes nodes that were read in from an XML file and fills in their related
	/// node information so they can be drawn on the screen.  This method is called 
	/// the first time annotations are drawn after a network has been read.
	/// </summary>
	private void processAnnotationsFromNetwork()
	{
		PropList p = null;
		foreach (HydrologyNode node in this.__annotations)
		{
			p = (PropList)node.getAssociatedObject();
			string text = p.getValue("Text");
			string point = p.getValue("Point");
			int index = point.IndexOf(",", StringComparison.Ordinal);
			string xs = point.Substring(0, index);
			string ys = point.Substring(index + 1, point.Length - (index + 1));
			string position = p.getValue("TextPosition");
			double x = (Convert.ToDouble(xs));
			double y = (Convert.ToDouble(ys));

			int fontSize = (new int?(p.getValue("OriginalFontSize")));
			fontSize = calculateScaledFont(p.getValue("FontName"), p.getValue("FontStyle"), fontSize, false);

			GRLimits limits = GRDrawingAreaUtil.getTextExtents(__drawingArea, text, GRUnits.DEVICE, p.getValue("FontName"), p.getValue("FontStyle"), fontSize);
			double w = convertDrawingXToDataX(limits.getWidth());
			double h = convertDrawingYToDataY(limits.getHeight());

			// Calculate the actual limits for the from the lower-left  corner
			// to the upper-right, in order to know when the text has been 
			// clicked on (for dragging, or popup menus).

			if (position.Equals("UpperRight", StringComparison.OrdinalIgnoreCase))
			{
				node.setPosition(x, y, w, h);
			}
			else if (position.Equals("Right", StringComparison.OrdinalIgnoreCase))
			{
				node.setPosition(x, y - (h / 2), w, h);
			}
			else if (position.Equals("LowerRight", StringComparison.OrdinalIgnoreCase))
			{
				node.setPosition(x, y - h, w, h);
			}
			else if (position.Equals("Below", StringComparison.OrdinalIgnoreCase) || position.Equals("BelowCenter", StringComparison.OrdinalIgnoreCase))
			{
				node.setPosition(x - (w / 2), y - h, w, h);
			}
			else if (position.Equals("LowerLeft", StringComparison.OrdinalIgnoreCase))
			{
				node.setPosition(x - w, y - h, w, h);
			}
			else if (position.Equals("Left", StringComparison.OrdinalIgnoreCase))
			{
				node.setPosition(x - w, y - (h / 2), w, h);
			}
			else if (position.Equals("UpperLeft", StringComparison.OrdinalIgnoreCase))
			{
				node.setPosition(x - w, y, w, h);
			}
			else if (position.Equals("Above", StringComparison.OrdinalIgnoreCase) || position.Equals("AboveCenter", StringComparison.OrdinalIgnoreCase))
			{
				node.setPosition(x - (w / 2), y, w, h);
			}
			else if (position.Equals("Center", StringComparison.OrdinalIgnoreCase))
			{
				node.setPosition(x - (w / 2), y - (h / 2), w, h);
			}
		}
	}

	/// <summary>
	/// Reads a network from a makenet file. </summary>
	/// <param name="nodeDataProvider"> the data provider to use for helping to read the file. </param>
	/// <param name="filename"> the name of the makenet file to read. </param>
	protected internal virtual void readMakenetFile(StateMod_NodeDataProvider nodeDataProvider, string filename)
	{
		__network = new StateMod_NodeNetwork();
		__network.readMakenetNetworkFile(nodeDataProvider, filename, true);
		__annotations = __network.getAnnotationList();
		__processAnnotations = true;
		if (__annotations == null)
		{
			__annotations = new List<HydrologyNode>();
		}
		__links = __network.getLinkList();
		printNetworkInfo();
		buildNodeArray();
		findMaxReachLevel();
	}

	/// <summary>
	/// Redoes one change operation.
	/// </summary>
	protected internal virtual void redo()
	{
		if (__undoPos == __undoOperations.Count || !__editable)
		{
			return;
		}

		StateMod_Network_UndoData data = (StateMod_Network_UndoData)__undoOperations[__undoPos];
		__undoPos++;
		__nodes[data.nodeNum].setX(data.newX);
		__nodes[data.nodeNum].setY(data.newY);

		if (data.otherNodes != null)
		{
			for (int i = 0; i < data.otherNodes.Length; i++)
			{
				__nodes[data.otherNodes[i]].setX(data.newXs[i]);
				__nodes[data.otherNodes[i]].setY(data.newYs[i]);
			}
		}

		forceRepaint();

		if (__undoPos == __undoOperations.Count)
		{
			__parent.setRedo(false);
		}
		else
		{
			__parent.setRedo(true);
		}
		__parent.setUndo(true);
	}

	/// <summary>
	/// Removes all links that involve the node with the given ID.  This is called when
	/// a node is deleted so that links don't try to point to a nonexistent node. </summary>
	/// <param name="id"> the ID of the node that was deleted and which should not be in any links. </param>
	private void removeIDFromLinks(string id)
	{
		string routine = "StateMod_Network_JComponent.removeIDFromLinks";

		bool found = false;

		if (__links == null)
		{
			return;
		}

		int size = __links.Count;
		PropList p = null;
		string id1 = null;
		string id2 = null;
		for (int i = size - 1; i >= 0; i--)
		{
			found = false;
			p = __links[i];
			id1 = p.getValue("FromNodeID");
			id2 = p.getValue("ToNodeID");

			if (id1.Equals(id))
			{
				found = true;
			}
			else if (id2.Equals(id))
			{
				found = true;
			}

			if (found)
			{
				__links.Remove(i);
				Message.printWarning(2, routine, "ID '" + id + "' found in a link.  The link will no longer be drawn.");
			}
		}
	}

	/// <summary>
	/// Saves the entire network to an image file.
	/// </summary>
	protected internal virtual void saveNetworkAsImage()
	{
		__savingNetwork = true;
		__tempBuffer = new BufferedImage((int)(__totalBufferWidth * (72.0 / (double)__dpi)), (int)(__totalBufferHeight * (72.0 / (double)__dpi)), BufferedImage.TYPE_4BYTE_ABGR);
		__bufferGraphics = (Graphics2D)(__tempBuffer.createGraphics());
		forceRepaint();
		new RTi.Util.GUI.SaveImageGUI(__tempBuffer, __parent);
		__tempBuffer = null;
		__savingNetwork = false;
	}

	/// <summary>
	/// Saves what is currently visible on screen to a graphic file by dumping the canvas area.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void saveScreenAsImage() throws java.io.IOException
	protected internal virtual void saveScreenAsImage()
	{
		bool useNewCode = true;
		if (useNewCode)
		{
			string last_directory = JGUIUtil.getLastFileDialogDirectory();
			JFileChooser fc = JFileChooserFactory.createJFileChooser(last_directory);
			fc.setDialogTitle("Save Visible Network as Image File");
			fc.setAcceptAllFileFilterUsed(false);
			SimpleFileFilter jpg_sff = new SimpleFileFilter("jpeg", "JPEG Image File");
			fc.addChoosableFileFilter(jpg_sff);
			SimpleFileFilter png_sff = new SimpleFileFilter("png", "PNG Image File");
			fc.setFileFilter(png_sff);
			if (fc.showSaveDialog(this) != JFileChooser.APPROVE_OPTION)
			{
				// Canceled...
				return;
			}
			// Else figure out the file format and location and then do the save...
			last_directory = fc.getSelectedFile().getParent();
			string path = fc.getSelectedFile().getPath();
			JGUIUtil.setLastFileDialogDirectory(last_directory);
			//__parent.setWaitCursor(true);
			if (fc.getFileFilter() == png_sff)
			{
				path = IOUtil.enforceFileExtension(path, "png");
			}
			// Leave *.jpeg alone but by default enforce *.jpg
			else if ((fc.getFileFilter() == jpg_sff) && !StringUtil.endsWithIgnoreCase(path,"jpeg"))
			{
				path = IOUtil.enforceFileExtension(path, "jpg");
			}
			// The following will examine the extension and save as PNG or JPG accordingly
			 saveAsFile(path);
		}
		else
		{
			__savingScreen = true;
			__tempBuffer = new BufferedImage(getBounds().width, getBounds().height, BufferedImage.TYPE_4BYTE_ABGR);
			__bufferGraphics = (Graphics2D)(__tempBuffer.createGraphics());
			forceRepaint();
			new RTi.Util.GUI.SaveImageGUI(__tempBuffer, __parent);
			__tempBuffer = null;
			__savingScreen = false;
		}
	}

	// FIXME SAM 2008-12-11 Need a way to save the selected filename so that it can be the default
	// for the next save.
	/// <summary>
	/// Saves the network as an XML file. </summary>
	/// <param name="filename"> the name of the file to put into the JFileChooser.  If null, it will be ignored.  </param>
	protected internal virtual void saveXML(string filename)
	{
		string lastDirectorySelected = JGUIUtil.getLastFileDialogDirectory();
		JFileChooser fc = JFileChooserFactory.createJFileChooser(lastDirectorySelected);
		fc.setDialogTitle("Save XML Network File");
		SimpleFileFilter netff = new SimpleFileFilter("net", "Network Files");
		fc.addChoosableFileFilter(netff);
		fc.setFileFilter(netff);
		fc.setDialogType(JFileChooser.SAVE_DIALOG);

		if (!string.ReferenceEquals(filename, null))
		{
			int index = filename.LastIndexOf(File.separator);
			if (index > -1)
			{
				filename = filename.Substring(index + 1);
			}
			fc.setSelectedFile(new File(filename));
		}

		int retVal = fc.showSaveDialog(this);
		if (retVal != JFileChooser.APPROVE_OPTION)
		{
			return;
		}

		string currDir = (fc.getCurrentDirectory()).ToString();

		if (!currDir.Equals(lastDirectorySelected, StringComparison.OrdinalIgnoreCase))
		{
			JGUIUtil.setLastFileDialogDirectory(currDir);
		}
		string selectedFilename = fc.getSelectedFile().getPath();

		selectedFilename = IOUtil.enforceFileExtension(selectedFilename, "net");

		// These are the limits based on the 
		GRLimits limits = new GRLimits(__dataLeftX, __dataBottomY, __totalDataWidth + __dataLeftX, __totalDataHeight + __dataBottomY);

		PropList p = new PropList("");
		p.set("ID=\"Main\"");
		p.set("PaperSize=\"" + PrintUtil.pageFormatToString(__pageFormat) + "\"");
		p.set("PageOrientation=\"" + PrintUtil.getOrientationAsString(__pageFormat) + "\"");
		p.set("NodeLabelFontPointSize=" + __printFontSizePixels);
		p.set("NodeSize=" + __nodeSizeFullScale);
		double[] edgeBuffer = new double[] {0, 0, 0, 0};
		try
		{
			__network.writeXML(selectedFilename, limits, __parent.getLayouts(), __annotations, __links, __legendDataLimits, edgeBuffer);
		}
		catch (Exception e)
		{
			string routine = "StateMod_Network_JComponent.saveXML()";
			Message.printWarning(1, routine, "Error saving network XML file.");
			Message.printWarning(2, routine, e);
		}
		printNetworkInfo();
	}

	/// <summary>
	/// Scales things that are drawn that are not scaled nicely by the GR package.
	/// In particular, this does font scaling, which needs REVISIT ed anyways!!
	/// </summary>
	private void scaleUnscalables()
	{
		double scale = 0;
		if (__fitWidth)
		{
			double pixels = __dpi * (int)(__pageFormat.getWidth() / 72);
			double pct = (getBounds().width / pixels);
			double width = __totalDataWidth * pct;
			scale = width / __screenDataWidth;
		}
		else
		{
			double pixels = __dpi * (int)(__pageFormat.getHeight() / 72);
			double pct = (getBounds().height / pixels);
			double height = __totalDataHeight * pct;
			scale = height / __screenDataHeight;
		}
		__fontSizePixels = (int)(__printFontSizePixels * scale);
		__fontSizePoints = __drawingArea.calculateFontSize(__fontSizePixels);
		double temp = (double)__fontSizePoints * ((double)__dpi / 72.0);
		temp += 0.5;
		__printFontSizePoints = (int)temp + 2;
		__lineThickness = (int)(__printLineThickness * scale);
	}

	/// <summary>
	/// Sets the visible data limits.  No additional computations are done. </summary>
	/// <param name="dataLimits"> the data limits for the visible network. </param>
	private void setDataLimits(GRLimits dataLimits)
	{
		__dataLimits = dataLimits;
	}

	// TODO SAM 2010-12-29 need to change the limits when a node is added or deleted
	// - not a big deal right now because the max is used for zooming
	/// <summary>
	/// Sets the maximum data limits for the data. </summary>
	/// <param name="dataLimitsMax"> the maximum data limits for the network. </param>
	private void setDataLimitsMax(GRLimits dataLimitsMax)
	{
		__dataLimitsMax = dataLimitsMax;
	}

	/// <summary>
	/// Sets whether the network and its nodes are dirty.  Usually will be called with a parameter of false
	/// after a save has occurred. </summary>
	/// <param name="dirty"> whether to mark everything dirty or not. </param>
	protected internal virtual void setDirty(bool dirty)
	{
	//	Message.printStatus(1, "", "SetDirty: " + dirty);
		setNetworkChanged(dirty);
		for (int i = 0; i < __nodes.Length; i++)
		{
			// FIXME SAM 2008-12-11 Why is the following always false?
			__nodes[i].setDirty(false);
		}
	}

	/// <summary>
	/// Sets the drawing area to be used with this device. </summary>
	/// <param name="drawingArea"> the drawingArea to use with this device. </param>
	protected internal virtual void setDrawingArea(GRJComponentDrawingArea drawingArea)
	{
		__drawingArea = drawingArea;
	}

	/// <summary>
	/// Sets the mode the network should be in in regard to how it responds to mouse presses. </summary>
	/// <param name="mode"> the mode the network should be in in regard to how it responds to mouse presses. </param>
	protected internal virtual void setMode(int mode)
	{
		__networkMouseMode = mode;
	}

	/// <summary>
	/// Sets the network to be used.  Called by the code that has read in a network from an XML file. </summary>
	/// <param name="network"> the network to use. </param>
	/// <param name="dirty"> indicates whether the network should be marked dirty (changed) - use when? </param>
	/// <param name="doAll"> indicates whether all initialization work should occur (internal lists to help with processing). </param>
	protected internal virtual void setNetwork(StateMod_NodeNetwork network, bool dirty, bool doAll)
	{
		if (__network == null && network != null)
		{
			// new network
			__links = network.getLinkList();
			__annotations = network.getAnnotationList();
		}

		__network = network;
		__processAnnotations = true;
		if (__annotations == null)
		{
			__annotations = new List<HydrologyNode>();
		}
		printNetworkInfo();
		setNetworkChanged(dirty);
		if (!doAll)
		{
			return;
		}
		buildNodeArray();
		findMaxReachLevel();
		__network.setLinkList(__links);
		__network.setAnnotationList(__annotations);
		if (__referenceJComponent != null)
		{
			// Reference network is not used for printing
			__referenceJComponent.setNetwork(__network);
			__referenceJComponent.setNodesArray(__nodes);
		}
	}

	/// <summary>
	/// Set whether the network has been changed, in particular nodes added/deleted.
	/// </summary>
	public virtual void setNetworkChanged(bool networkChanged)
	{
		__networkChanged = networkChanged;
	}

	/// <summary>
	/// Sets the size of the nodes (in pixels) at the 1:1 zoom level. </summary>
	/// <param name="size"> the size of the nodes. </param>
	public virtual void setNodeSize(double size)
	{
		__nodeSizeFullScale = (int)size;
		setPrintNodeSize(size);
	}

	/// <summary>
	/// Sets the orientation of the paper. </summary>
	/// <param name="orientation"> either "Landscape" or "Portrait". </param>
	public virtual void setOrientation(string orientation)
	{
		if (_graphics == null)
		{
			__holdPaperOrientation = orientation;
			return;
		}
		else
		{
			__holdPaperOrientation = null;
		}
		try
		{
			__pageFormat = PrintUtil.getPageFormat(PrintUtil.pageFormatToString(__pageFormat));
			if (orientation.Trim().Equals("Landscape", StringComparison.OrdinalIgnoreCase))
			{
				PrintUtil.setPageFormatOrientation(__pageFormat, PageFormat.LANDSCAPE);
			}
			else
			{
				PrintUtil.setPageFormatOrientation(__pageFormat, PageFormat.PORTRAIT);
			}
			double margin = getMargin();
			PrintUtil.setPageFormatMargins(__pageFormat, margin, margin, margin, margin);
			int hPixels = (int)(__pageFormat.getWidth() / __printScale);
			int vPixels = (int)(__pageFormat.getHeight() / __printScale);
			setTotalSize(hPixels, vPixels);
			calculateDataLimits();

			zoomOneToOne();
			forceRepaint();
		}
		catch (Exception e)
		{
			string routine = "StateMod_Network_JComponent.setOrientation";
			Message.printWarning(1, routine, "Error setting orientation.");
			Message.printWarning(2, routine, e);
		}
	}

	/// <summary>
	/// Sets the pageformat to use. </summary>
	/// <param name="pageFormat"> the pageFormat to use. </param>
	public virtual void setPageFormat(PageFormat pageFormat)
	{
		__pageFormat = pageFormat;
	}

	/// <summary>
	/// Changes the paper size. </summary>
	/// <param name="size"> the size to set the paper to (see PrintUtil for a list of supported paper sizes). </param>
	public virtual void setPaperSize(string size)
	{
		if (_graphics == null)
		{
			__holdPaperSize = size;
			return;
		}
		else
		{
			__holdPaperSize = null;
		}
		try
		{
			__pageFormat = PrintUtil.getPageFormat(size);
			PrintUtil.setPageFormatOrientation(__pageFormat, PageFormat.LANDSCAPE);
			double margin = getMargin();
			PrintUtil.setPageFormatMargins(__pageFormat, margin, margin, margin, margin);
			int hPixels = (int)(__pageFormat.getWidth() / __printScale);
			int vPixels = (int)(__pageFormat.getHeight() / __printScale);
			setTotalSize(hPixels, vPixels);
			zoomOneToOne();
			forceRepaint();
		}
		catch (Exception e)
		{
			string routine = "StateMod_Network_JComponent.setPaperSize";
			Message.printWarning(1, routine, "Error setting paper size.");
			Message.printWarning(3, routine, e);
		}
	}

	/// <summary>
	/// Sets the printing scale.  The network is designed to be set up so 
	/// that it fits well on a certain size printed page, and this scale is used to ensure that everything fits.
	/// <para>
	/// By default, Java prints out everything at 72 dpi.  At this size, a Letter-sized
	/// piece of paper could only have 792 x 612 pixels on it.  Since each node in the
	/// network is drawn at 20 pixels high, it is possible that larger networks would run out of space.
	/// </para>
	/// <para>
	/// At the same time, it should be possible to scale larger networks so that 
	/// </para>
	/// even then can fit on a smaller piece of paper.  Thus the use of the printing scale.  <para>
	/// The printing scale basically adjusts the DPI at which Java prints.  A scale
	/// of .5 would result in 144 DPI and .25 in 288 DPI.  2 would result in 36 DPI.<P>
	/// .25 is a good scale for most purposes, but further adjustments may be necessary
	/// for larger networks and smaller papers.
	/// </para>
	/// </summary>
	/// <param name="scale"> the printing scale. </param>
	/* TODO SAM 2007-03-01 Evaluate whether needed
	private void setPrintingScale(double scale) {	
		__printScale = scale;
	}
	*/

	/// <summary>
	/// Sets the size in points that fonts should be printed at when printed at 1:1. </summary>
	/// <param name="fontSizePoints"> the pixel size of fonts when printed at 1:1. </param>
	/// <param name="doCalcs"> if true, do extra legacy calculations, if false, just set the value </param>
	public virtual void setPrintFontSize(int fontSizePoints)
	{
		if (_graphics == null)
		{
			__holdPrintFontSize = fontSizePoints;
			return;
		}
		else
		{
			__holdPrintFontSize = -1;
		}
		__printFontSizePixels = fontSizePoints;
		scaleUnscalables();
		forceRepaint();
	}

	/// <summary>
	/// Sets the size (in points) that nodes should be printed at. </summary>
	/// <param name="nodeSizePoints"> the size (in points) of nodes when printed at 1:1. </param>
	public virtual void setPrintNodeSize(double nodeSizePoints)
	{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		string routine = this.GetType().FullName + ".setPrintNodeSize";
		if (__printingNetwork)
		{
			Message.printStatus(2, routine, "Setting print node size=" + nodeSizePoints + " points.");
		}
		if (_graphics == null)
		{
			__holdPrintNodeSize = nodeSizePoints;
			return;
		}
		else
		{
			__holdPrintNodeSize = -1;
		}

		__currNodeSize = nodeSizePoints;
		if (nodeSizePoints < 1)
		{
			nodeSizePoints = 1;
		}
		__legendNodeDiameter = nodeSizePoints;
		double diam = 0;
		if (__printingNetwork)
		{
			// TODO SAM 2011-07-09 Some of this method code seems not right.  The node expects to have the size
			// in data coordinates, which is what the following code does.
			// The diam seems to be what should be calculated in data units, but why is it not set for the
			// node icon diameter?
			// A more straightforward way of setting the size for rendering?
			// For now try the following.  When printing, the node diameter size (points) agrees with the
			// drawing limits units, so can use the standard method to convert to data units
			//nodeSizePoints = getDrawingArea().scaleXData(nodeSizePoints);
		}
		if (__fitWidth)
		{
			diam = convertDrawingXToDataX(nodeSizePoints);
		}
		else
		{
			diam = convertDrawingYToDataY(nodeSizePoints);
		}
		if (__printingNetwork)
		{
			Message.printStatus(2, routine, "Node size drawing=" + nodeSizePoints + ", data=" + diam);
		}
		for (int i = 0; i < __nodes.Length; i++)
		{
			__nodes[i].setIconDiameter((int)(nodeSizePoints));
			__nodes[i].setSymbol(null);
			__nodes[i].setBoundsCalculated(false);
			__nodes[i].setDataDiameter(diam);
			__nodes[i].calculateExtents(getDrawingArea());
		}
		forceRepaint();
	}

	/// <summary>
	/// Sets the reference display to use in conjunction with this display. </summary>
	/// <param name="reference"> the reference display that goes along with this network display. </param>
	protected internal virtual void setReference(StateMod_NetworkReference_JComponent reference)
	{
		__referenceJComponent = reference;
	}

	/// <summary>
	/// Set the selected network page layout properties for rendering.  This generally will either come from the
	/// network editor UI, or from a command parameter when batch printing. </summary>
	/// <param name="selectedPageLayoutPropList"> the page layout that should be used for rendering </param>
	public virtual void setSelectedPageLayoutPropList(PropList selectedPageLayoutPropList)
	{
		this.__selectedPageLayoutPropList = selectedPageLayoutPropList;
	}

	/// <summary>
	/// Sets the total height and width of the entire network display, in device unit pixels. </summary>
	/// <param name="w"> the total width of the entire network. </param>
	/// <param name="h"> the total height of the entire network. </param>
	protected internal virtual void setTotalSize(int w, int h)
	{
		// Message.printStatus(1, "", "Set Total Size: " + w + ", " + h);
		__totalBufferWidth = w;
		__totalBufferHeight = h;
	}

	/// <summary>
	/// Sets up the double buffer.
	/// </summary>
	/* TODO SAM 2007-03-01 Evaluate whether needed
	private void setupDoubleBuffering() {
		setupDoubleBuffer(0, 0, getBounds().width, getBounds().height);
		forceRepaint();
	}
	*/

	/// <summary>
	/// Sets the part of the network being viewed. </summary>
	/// <param name="x"> the left X point of the screen, in device units. </param>
	/// <param name="y"> the bottom Y point of the screen, in device units. </param>
	protected internal virtual void setViewPosition(int x, int y)
	{
		__screenBottomY = y;
		__screenLeftX = x;

		// make sure the view position is not moved beyond the network drawing boundaries
		if (__screenLeftX > (__dataLeftX + __totalDataWidth) - convertDrawingXToDataX(getBounds().width))
		{
			__screenLeftX = (__dataLeftX + __totalDataWidth) - convertDrawingXToDataX(getBounds().width);
		}
		if (__screenLeftX < __dataLeftX)
		{
			__screenLeftX = __dataLeftX;
		}

		if (__screenBottomY > (__dataBottomY + __totalDataHeight) - convertDrawingYToDataY(getBounds().height))
		{
			__screenBottomY = (__dataBottomY + __totalDataHeight) - convertDrawingYToDataY(getBounds().height);
		}
		if (__screenBottomY < __dataBottomY)
		{
			__screenBottomY = __dataBottomY;
		}

		forceRepaint();
	}

	/// <summary>
	/// Sets the data limits for the network from what was read in an XML file. </summary>
	/// <param name="lx"> the left-most x value in data units. </param>
	/// <param name="by"> the bottom-most y value in data units. </param>
	/// <param name="w"> the width of the network in data units. </param>
	/// <param name="h"> the height of the network in data units. </param>
	protected internal virtual void setXMLDataLimits(double lx, double by, double w, double h)
	{
		GRLimits limits = new GRLimits(lx, by, lx + w, by + h);
		__dataLimits = limits;
		setDataLimitsMax(new GRLimits(__dataLimits));
		__drawingArea.setDataLimits(limits);
		calculateDataLimits();
	}

	/// <summary>
	/// Takes a double and trims its decimal values so that it only has 6 places of precision. </summary>
	/// <param name="d"> the double to trim. </param>
	/// <returns> the same double with only 6 places of precision. </returns>
	private double toSixDigits(double d)
	{
		string s = StringUtil.formatString(d, "%20.6f");
		double? D = Convert.ToDouble(s);
		return D.Value;
	}

	/// <summary>
	/// Converts an X value in data units to an X value in drawing units. </summary>
	/// <param name="x"> the x value to convert. </param>
	/// <returns> the x value, converted from being scaled for data limits to being scaled for drawing units. </returns>
	/* TODO SAM Evaluate use
	private double unconvertX(double x) {
		GRLimits data = __drawingArea.getDataLimits();
		GRLimits draw = __drawingArea.getDrawingLimits();
		double lx = data.getLeftX();
		double xAdjust = 0;
		if (lx < 0) {
			xAdjust = lx;
		}
		else {
			xAdjust = 0 - lx;
		}	
		x += xAdjust;
		double width = data.getWidth();
		double pct = x / width;
		return draw.getWidth() * pct;
	}
	*/

	/// <summary>
	/// Converts an Y value in data units to an Y value in drawing units. </summary>
	/// <param name="y"> the y value to convert. </param>
	/// <returns> the y value, converted from being scaled for data limits to being scaled for drawing units. </returns>
	/* TODO SAM 2007-03-01 Evaluate whether needed
	private double unconvertY(double y) {
		GRLimits data = __drawingArea.getDataLimits();
		GRLimits draw = __drawingArea.getDrawingLimits();
		double by = data.getBottomY();
		double yAdjust = 0;
		if (by < 0) {
			yAdjust = by;
		}
		else {
			yAdjust = 0 - by;
		}
		// now it's 0-based
		y += yAdjust;
	
		double height = data.getHeight();
		double pct = y / height;
		return draw.getHeight() * pct;
	}
	*/

	/// <summary>
	/// Undoes one change operation.
	/// </summary>
	protected internal virtual void undo()
	{
		if (__undoPos == 0 || !__editable)
		{
			return;
		}

		__undoPos--;
		StateMod_Network_UndoData data = (StateMod_Network_UndoData)__undoOperations[__undoPos];
		__nodes[data.nodeNum].setX(data.oldX);
		__nodes[data.nodeNum].setY(data.oldY);

		if (data.otherNodes != null)
		{
			for (int i = 0; i < data.otherNodes.Length; i++)
			{
				__nodes[data.otherNodes[i]].setX(data.oldXs[i]);
				__nodes[data.otherNodes[i]].setY(data.oldYs[i]);
			}
		}

		forceRepaint();
		if (__undoPos == 0)
		{
			__parent.setUndo(false);
		}
		else
		{
			__parent.setUndo(true);
		}
		__parent.setRedo(true);
	}

	/// <summary>
	/// Given a certain annotation, updates the proplist that holds the annotation
	/// info in reaction to the annotation being moved on the screen. </summary>
	/// <param name="annotation"> the number of the annotation in the __annotations list to have the proplist updated. </param>
	private void updateAnnotationLocation(int annotation)
	{
		HydrologyNode node = __annotations[annotation];

		double x = node.getX();
		double y = node.getY();
		double w = node.getWidth();
		double h = node.getHeight();

		PropList p = (PropList)node.getAssociatedObject();

		string position = p.getValue("TextPosition");

		if (position.Equals("UpperRight", StringComparison.OrdinalIgnoreCase))
		{
			p.setValue("Point", "" + x + "," + y);
		}
		else if (position.Equals("Right", StringComparison.OrdinalIgnoreCase))
		{
			p.setValue("Point", "" + x + "," + (y + (h / 2)));
		}
		else if (position.Equals("LowerRight", StringComparison.OrdinalIgnoreCase))
		{
			p.setValue("Point", "" + x + "," + (y + h));
		}
		else if (position.Equals("Below", StringComparison.OrdinalIgnoreCase) || position.Equals("BelowCenter", StringComparison.OrdinalIgnoreCase))
		{
			p.setValue("Point", "" + (x + (w / 2)) + "," + (y + h));
		}
		else if (position.Equals("LowerLeft", StringComparison.OrdinalIgnoreCase))
		{
			p.setValue("Point", "" + (x + w) + "," + (y + h));
		}
		else if (position.Equals("Left", StringComparison.OrdinalIgnoreCase))
		{
			p.setValue("Point", "" + (x + w) + "," + (y + (h / 2)));
		}
		else if (position.Equals("UpperLeft", StringComparison.OrdinalIgnoreCase))
		{
			p.setValue("Point", "" + (x + w) + "," + y);
		}
		else if (position.Equals("Above", StringComparison.OrdinalIgnoreCase) || position.Equals("AboveCenter", StringComparison.OrdinalIgnoreCase))
		{
			p.setValue("Point", "" + (x + (w / 2)) + "," + y);
		}
		else if (position.Equals("Center", StringComparison.OrdinalIgnoreCase))
		{
			p.setValue("Point", "" + (x + (w / 2)) + "," + (y + (h / 2)));
		}
	}

	/// <summary>
	/// Updates one of the annotation nodes with location and text information stored in the passed-in node. </summary>
	/// <param name="nodeNum"> the number of the node (in the node array) to update. </param>
	/// <param name="node"> the node holding information with which the other node should be updated. </param>
	protected internal virtual void updateAnnotation(int nodeNum, HydrologyNode node)
	{
		PropList p = (PropList)node.getAssociatedObject();

		HydrologyNode vNode = __annotations[nodeNum];
		PropList vp = (PropList)vNode.getAssociatedObject();
		vNode.setAssociatedObject(p);

		string text = p.getValue("Text");
		bool labelChanged = false;
		if (!text.Equals(vp.getValue("Text")))
		{
			vp.setValue("Text", text);
			labelChanged = true;
		}

		string val = p.getValue("Point").Trim();
		string position = p.getValue("TextPosition");
		string fontSize = p.getValue("OriginalFontSize");
		string fontName = p.getValue("FontName");
		string fontStyle = p.getValue("FontStyle");

		if (!val.Equals(vp.getValue("Point")) || labelChanged || !position.Equals(vp.getValue("TextPosition")) || !fontSize.Equals(vp.getValue("OriginalFontSize")) || !fontName.Equals(vp.getValue("FontName")) || !fontStyle.Equals(vp.getValue("FontStyle")))
		{

			int size = (new int?(p.getValue("OriginalFontSize")));
			size = calculateScaledFont(p.getValue("FontName"), p.getValue("FontStyle"), size, false);
			GRLimits limits = GRDrawingAreaUtil.getTextExtents(__drawingArea, text, GRUnits.DEVICE, p.getValue("FontName"), p.getValue("FontStyle"), size);

			double w = convertDrawingXToDataX(limits.getWidth());
			double h = convertDrawingYToDataY(limits.getHeight());

			if (!val.Equals(vp.getValue("Point")))
			{
				vp.setValue("Point", val);
				vNode.setDirty(true);
			}
			if (!position.Equals(vp.getValue("TextPosition")))
			{
				vp.setValue("TextPosition", position);
				vNode.setDirty(true);
			}

			string temp = StringUtil.getToken(val, ",", 0, 0);
			double x = (Convert.ToDouble(temp));
			temp = StringUtil.getToken(val, ",", 0, 1);
			double y = (Convert.ToDouble(temp));

			if (position.Equals("UpperRight", StringComparison.OrdinalIgnoreCase))
			{
				vNode.setPosition(x, y, w, h);
				vNode.setDirty(true);
			}
			else if (position.Equals("Right", StringComparison.OrdinalIgnoreCase))
			{
				vNode.setPosition(x, y - (h / 2), w, h);
			}
			else if (position.Equals("LowerRight", StringComparison.OrdinalIgnoreCase))
			{
				vNode.setPosition(x, y - h, w, h);
			}
			else if (position.Equals("Below", StringComparison.OrdinalIgnoreCase) || position.Equals("BelowCenter", StringComparison.OrdinalIgnoreCase))
			{
				vNode.setPosition(x - (w / 2), y - h, w, h);
			}
			else if (position.Equals("LowerLeft", StringComparison.OrdinalIgnoreCase))
			{
				vNode.setPosition(x - w, y - h, w, h);
			}
			else if (position.Equals("Left", StringComparison.OrdinalIgnoreCase))
			{
				vNode.setPosition(x - w, y - (h / 2), w, h);
			}
			else if (position.Equals("UpperLeft", StringComparison.OrdinalIgnoreCase))
			{
				vNode.setPosition(x - w, y, w, h);
			}
			else if (position.Equals("Above", StringComparison.OrdinalIgnoreCase) || position.Equals("AboveCenter", StringComparison.OrdinalIgnoreCase))
			{
				vNode.setPosition(x - (w / 2), y, w, h);
			}
			else if (position.Equals("Center", StringComparison.OrdinalIgnoreCase))
			{
				vNode.setPosition(x - (w / 2), y - (h / 2), w, h);
			}
		}

		if (!fontName.Equals(vp.getValue("FontName")))
		{
			vNode.setDirty(true);
			vp.setValue("FontName", fontName);
		}

		val = p.getValue("OriginalFontSize");
		if (!val.Equals(vp.getValue("OriginalFontSize")))
		{
			vNode.setDirty(true);
			vp.setValue("OriginalFontSize", val);
		}

		if (!fontStyle.Equals(vp.getValue("FontStyle")))
		{
			vNode.setDirty(true);
			vp.setValue("FontStyle", fontStyle);
		}
		vNode.setAssociatedObject(vp);
		forceRepaint();
	}

	/// <summary>
	/// Checks to see if a node is within the limits defined the passed-in GRLimits. </summary>
	/// <param name="rect"> GRLimits to check if the node is within. </param>
	/// <param name="node"> the node to check to see if it is within the node. </param>
	/// <returns> true if the node is within the limits, false if not. </returns>
	private bool within(GRLimits rect, HydrologyNode node)
	{
		double x = node.getX();
		double y = node.getY();
		double diam = node.getDataDiameter() / 2;
		if (rect.contains(x, y))
		{
			return true;
		}

		if (x < rect.getLeftX())
		{
			x += diam;
		}
		else if (x > rect.getRightX())
		{
			x -= diam;
		}

		if (y < rect.getBottomY())
		{
			y += diam;
		}
		else if (y > rect.getTopY())
		{
			y -= diam;
		}

		if (rect.contains(x, y))
		{
			return true;
		}

		return false;
	}

	/// <summary>
	/// Zooms to the specified limits.
	/// </summary>
	protected internal virtual void zoomCenterOn(GRLimits limits)
	{
		setDataLimits(limits);
		forceRepaint();
	}

	/// <summary>
	/// Zooms in the current view to twice the current view.
	/// </summary>
	protected internal virtual void zoomIn()
	{
		if ((__totalDataWidth / __screenDataWidth) > 17)
		{
			return;
		}

		double cx = __screenLeftX + (__screenDataWidth / 2);
		double cy = __screenBottomY + (__screenDataHeight / 2);

		__screenDataWidth /= 2;
		__screenDataHeight /= 2;
		__DY = __screenDataHeight / 50;
		__DY = 1;
		__DX = __screenDataWidth / 50;
		__DX = 1;

		__screenLeftX = cx - (__screenDataWidth / 2);
		__screenBottomY = cy - (__screenDataHeight / 2);

		setPrintNodeSize(__currNodeSize * 2);
		scaleUnscalables();
		if (!__ignoreRepaint)
		{
			forceRepaint();
		}
		__zoomPercentage *= 2;
		if (__zoomPercentage == 100)
		{
			__parent.setZoomedOneToOne(true);
		}
		else
		{
			__parent.setZoomedOneToOne(false);
		}

		// Done to avoid a bug in java when painting anti-aliased when zoomed-out
		if (__zoomPercentage < 100)
		{
			__antiAlias = false;
		}
		else
		{
			__antiAlias = __antiAliasSetting;
		}
	}

	/// <summary>
	/// Zooms so that the network as shown on the screen is how it will look when printed at full scale.
	/// Although not all of the network may be shown on the screen, it will be at full scale.
	/// </summary>
	protected internal virtual void zoomOneToOne()
	{
		//__screenLeftX = __dataLeftX;
		//__screenBottomY = __dataBottomY;

		__antiAlias = true;

		if (__fitWidth)
		{
			// Find out how many pixels across it should be given the
			// dpi for the screen and the paper size
			double pixels = __dpi * (int)(__pageFormat.getWidth() / 72);

			// Figure out the percentage of the entire paper that can fit on the screen
			double pct = (getBounds().width / pixels);

			// Show that percentage of the data at once
			double width = __totalDataWidth * pct;

			double ratio = __screenDataHeight / __screenDataWidth;
			double height = ratio * width;

			__screenDataWidth = width;
			__screenDataHeight = height;

			GRLimits data = __drawingArea.getDataLimits();
			data.setLeftX(__screenLeftX);
			data.setBottomY(__screenBottomY);
			data.setRightX(__screenLeftX + width);
			data.setTopY(__screenBottomY + height);
			__drawingArea.setDataLimits(data);
			setPrintNodeSize(__nodeSizeFullScale);
			scaleUnscalables();
			forceRepaint();
		}
		else
		{
			// Find out how many pixels high it should be given the
			// dpi for the screen and the paper size
			double pixels = __dpi * (int)(__pageFormat.getHeight() / 72);

			// Figure out the percentage of the entire paper that can fit on the screen
			double pct = (getBounds().height / pixels);

			// Show that percentage of the data at once
			double height = __totalDataHeight * pct;

			double ratio = __screenDataWidth / __screenDataHeight;
			double width = ratio * height;

			__screenDataHeight = height;
			__screenDataWidth = width;

			GRLimits data = __drawingArea.getDataLimits();
			data.setLeftX(__screenLeftX);
			data.setBottomY(__screenBottomY);
			data.setRightX(__screenLeftX + width);
			data.setTopY(__screenBottomY + height);
			__drawingArea.setDataLimits(data);
			setPrintNodeSize(__nodeSizeFullScale);
			scaleUnscalables();
			forceRepaint();
		}
		__zoomPercentage = 100;
		if (__parent != null)
		{
			__parent.setZoomedOneToOne(true);
		}
	}

	/// <summary>
	/// Zooms out the current view to half the current view.
	/// </summary>
	protected internal virtual void zoomOut()
	{
		if ((__screenDataWidth / __totalDataWidth) > 9)
		{
			return;
		}

		double cx = __screenLeftX + (__screenDataWidth / 2);
		double cy = __screenBottomY + (__screenDataHeight / 2);

		__screenDataWidth *= 2;
		__screenDataHeight *= 2;
		__DY = __screenDataHeight / 500;
		__DY = 1;
		__DX = __screenDataWidth / 500;
		__DX = 1;

		__screenLeftX = cx - (__screenDataWidth / 2);
		__screenBottomY = cy - (__screenDataHeight / 2);

		setPrintNodeSize(__currNodeSize / 2);
		scaleUnscalables();
		if (!__ignoreRepaint)
		{
			forceRepaint();
		}
		__zoomPercentage /= 2;
		if (__zoomPercentage == 100)
		{
			__parent.setZoomedOneToOne(true);
		}
		else
		{
			__parent.setZoomedOneToOne(false);
		}

		// Done to avoid a bug in java when painting anti-aliased when zoomed-out
		if (__zoomPercentage < 100)
		{
			__antiAlias = false;
		}
		else
		{
			__antiAlias = __antiAliasSetting;
		}
	}

	/// <summary>
	/// Zooms so that the height of the network fits exactly in the height of the screen.
	/// </summary>
	/* TODO SAM 2007-03-01 Evaluate whether needed
	private void zoomToHeight() {
		//__screenLeftX = __dataLeftX;
		//__screenBottomY = __dataBottomY;
	
		__screenDataHeight = __totalDataHeight;
		double pct = ((double)(getBounds().width)) / ((double)(getBounds().height));
		__screenDataWidth = pct * __totalDataHeight;
	
		scaleUnscalables();
		forceRepaint();
	}
	*/

	/// <summary>
	/// Zooms so that the network fits in the screen and all dimension are visible.
	/// FIXME (JTS - 2004-04-01) Doesn't work 100% right.
	/// </summary>
	/* TODO SAM Evaluate whether needed
	private void zoomToScreen() {
		int height = getBounds().height;
		int width = getBounds().width;
		double screenRatio = (double)height / (double)width;
		double dataRatio = __totalDataHeight / __totalDataWidth;
	
		if (__totalDataWidth > __totalDataHeight) {
			if (screenRatio < dataRatio) {
				zoomToHeight();
			}
			else {
				zoomToWidth();
			}
		}
		else {
			if (screenRatio < dataRatio) {
				zoomToWidth();
			}
			else {
				zoomToHeight();
			}
	
		}
		scaleUnscalables();
		forceRepaint();
	}
	*/

	/// <summary>
	/// Zooms so that the width of the network fits exactly in the width of the screen.
	/// </summary>
	/* TODO SAM 2007-03-01 Evaluate whether needed
	private void zoomToWidth() {
		__screenDataWidth = __totalDataWidth;
		double pct = ((double)(getBounds().height)) / ((double)(getBounds().width));
		__screenDataHeight = pct * __totalDataWidth;
		
		scaleUnscalables();
		forceRepaint();
	}
	*/

	}

	/*
	TODO 2004-11-11 (JTS) - when printing at a larger paper size than double 
	__mouseDownX = 0;
	
	 8.5x11, the 
	canvas does not seem to fill the paper.  To recreate:
	 - open an XML file that starts at 8.5x11
	 - print it -- the margins, etc, match on screen
	 - change paper size to 17x11 and print
	 - the network no longer fills the paper as it should
	*/

	/*
	TODO (2005-12-22) 
	
	- legend cannot be dragged off the screen, looks stupid sometimes
	- annotation moves cannot be undone, and in fact interfere with undo actions
		on nodes that WERE moved
	*/

}