using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chaye
{
	public static class BSplineCurves
	{
		private static Dictionary<PrimaryParam, float> _primaryFuncDict = new Dictionary<PrimaryParam, float>();

		private static List<KnotPoint> _knotVector = default;

		public static List<Vector3> GetPoints(Path path, int segments)
		{
			segments = Mathf.Max(segments, 1);

			var points = new List<Vector3>();
			float floatSegments = segments;
			uint rank = path.Rank;
			_knotVector = path.KnotVector;

			for (int j = 0; j < segments; j++)
			{
				float u = j / floatSegments;
				_primaryFuncDict.Clear();
				Vector3 point = Vector3.zero;
				for (int i = 0; i < path.ControlPoints.Count; i++)
				{
					point += GetPrimaryFuncValue((uint)i, rank, u) * path.ControlPoints[i].Anchor;
				}
				points.Add(point);
			}

			return points;
		}

		private static float GetPrimaryFuncValue(uint i, uint rank, float u)
		{
			float value;
			var param = new PrimaryParam { Index = i, Rank = rank };

			if (_primaryFuncDict.TryGetValue(param, out value))
			{
				return value;
			}

			if (rank == 0)
			{
				float u_i = _knotVector[(int)i].Value;
				float u_i_1 = _knotVector[(int)(i + 1)].Value;
				if (u_i <= u && u < u_i_1)
					value = 1;
				else
					value = 0;
			}
			else
			{
				float u_i = _knotVector[(int)i].Value;
				float u_i_1 = _knotVector[(int)(i+1)].Value;
				float u_ip = _knotVector[(int)(i+rank)].Value;
				float u_ip_1 = _knotVector[(int)(i+rank+1)].Value;

				float leftWeight = (u - u_i) / (u_ip - u_i);
				float rightWeight = (u_ip_1 - u) / (u_ip_1 - u_i_1);

				value = leftWeight * GetPrimaryFuncValue(i, rank - 1, u) + rightWeight * GetPrimaryFuncValue(i + 1, rank - 1, u);
			}

			_primaryFuncDict.Add(param, value);
			return value;
		}
	}
}