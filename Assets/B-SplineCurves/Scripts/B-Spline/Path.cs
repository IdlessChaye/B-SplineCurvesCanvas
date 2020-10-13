
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chaye
{
	public struct Path
	{
		public uint Rank;
		public List<ControlPoint> ControlPoints;
		public List<KnotPoint> KnotVector;
	}
}