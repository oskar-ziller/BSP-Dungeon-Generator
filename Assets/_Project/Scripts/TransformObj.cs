#region

using UnityEngine;

#endregion

internal class TransformObj
{
	#region Constants and Fields

	public Transform Obj;
	public int X, Y;

	#endregion

	#region Constructors and Destructors

	public TransformObj(Transform o, int xpos, int ypos)
	{
		Obj = o;
		X = xpos;
		Y = ypos;
	}

	#endregion
}