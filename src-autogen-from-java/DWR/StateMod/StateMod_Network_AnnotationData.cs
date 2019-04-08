// StateMod_Network_AnnotationData - data management of StateMod_Network_AnnotationRenderer instances and associated data

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
	using GRLimits = RTi.GR.GRLimits;

	/// <summary>
	/// This class provides for data management of StateMod_Network_AnnotationRenderer instances and associated data so
	/// that the information can be used to provide a list of annotations in the network editor and provide
	/// data back to the renderers when the annotations need to be rendered.
	/// </summary>
	public class StateMod_Network_AnnotationData
	{

	/// <summary>
	/// Renderer for the data object.
	/// </summary>
	private StateMod_Network_AnnotationRenderer __annotationRenderer = null;

	/// <summary>
	/// Object that will be rendered.
	/// </summary>
	private object __object = null;

	/// <summary>
	/// Label for the object (displayed in the network editor).
	/// </summary>
	protected internal string __label = null;

	/// <summary>
	/// Data limits for the rendered object (network coordinate system data units).
	/// </summary>
	private GRLimits __limits = null;

	/// <summary>
	/// Construct an instance from primitive data. </summary>
	/// <param name="annotationRenderer"> the object that will render the data object </param>
	/// <param name="object"> the data object to be rendered </param>
	/// <param name="label"> the label to display on the rendered object </param>
	/// <param name="limits"> the data limits for the object, to simplify zooming </param>
	public StateMod_Network_AnnotationData(StateMod_Network_AnnotationRenderer annotationRenderer, object @object, string label, GRLimits limits)
	{
		__annotationRenderer = annotationRenderer;
		__object = @object;
		__label = label;
		setLimits(limits);
	}

	/// <summary>
	/// Finalize before garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~StateMod_Network_AnnotationData()
	{
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Return the label for the object. </summary>
	/// <returns> the label for the object </returns>
	public virtual string getLabel()
	{
		return __label;
	}

	/// <summary>
	/// Return the limits for the object. </summary>
	/// <returns> the limits for the object </returns>
	public virtual GRLimits getLimits()
	{
		return __limits;
	}

	/// <summary>
	/// Return the object to be rendered. </summary>
	/// <returns> the object to be rendered </returns>
	public virtual object getObject()
	{
		return __object;
	}


	/// <summary>
	/// Return the StateMod_Network_AnnotationRenderer for the data. </summary>
	/// <returns> the StateMod_Network_AnnotationRenderer for the data </returns>
	public virtual StateMod_Network_AnnotationRenderer getStateModNetworkAnnotationRenderer()
	{
		return __annotationRenderer;
	}

	/// <summary>
	/// Set the limits for the object.  These may need to be set after the initial construction because
	/// the renderer has more information about the extent of the annotation. </summary>
	/// <param name="the"> limits for the object </param>
	private void setLimits(GRLimits limits)
	{
		__limits = limits;
	}

	}

}