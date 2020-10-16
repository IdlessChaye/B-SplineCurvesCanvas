using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chaye
{
	public static class BSplineCurves
	{
		public static int Segments = 150;

		private static Dictionary<PrimaryParam, float> _primaryFuncDict = new Dictionary<PrimaryParam, float>();

		private static List<KnotPoint> _knotVector = default;

		public static Vector3 GetPoint(Path path, float u)
		{
			var point = Vector3.zero;

			uint rank = path.Rank;
			_knotVector = path.KnotVector;
			if (_knotVector.Count == 0 || _knotVector.Count <= rank)
				return point;

			if (u >= 0 && u <= 1)
			{
				_primaryFuncDict.Clear();
				for (int i = 0; i < path.ControlPoints.Count; i++)
				{
					point += GetPrimaryFuncValue((uint)i, rank, u) * path.ControlPoints[i].Anchor;
				}
			}

			if (point == Vector3.zero)
				point = new Vector3(10f, 10f); // trick
			return point;
		}

		public static List<Vector3> GetPoints(Path path)
		{
			var points = new List<Vector3>();
			float floatSegments = Segments;
			uint rank = path.Rank;
			_knotVector = path.KnotVector;
			if (_knotVector.Count == 0 || _knotVector.Count <= rank)
				return points;

			int jStart = (int)(_knotVector[(int)rank].Value * floatSegments);
			int jEnd = (int)(_knotVector[_knotVector.Count - (int)rank - 1].Value * floatSegments);
			for (int j = jStart; j <= jEnd; j++)
			{
				float u = j / floatSegments;
				Vector3 point = Vector3.zero;
				_primaryFuncDict.Clear();
				for (int i = 0; i < path.ControlPoints.Count; i++)
				{
					point += GetPrimaryFuncValue((uint)i, rank, u) * path.ControlPoints[i].Anchor;
				}
				if (point != Vector3.zero) // trick
					points.Add(point);
			}

			return points;
		}

		private static float GetPrimaryFuncValue(uint i, uint rank, float u)
		{
			var param = new PrimaryParam { Index = i, Rank = rank };

			float value;
			if (_primaryFuncDict.TryGetValue(param, out value))
			{
				return value;
			}

			value = 0f;
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

				float leftWeight = 0;
				if (Utilities.isFloatEqual(u_ip, u_i) == false)
				{ 
					leftWeight = (u - u_i) / (u_ip - u_i);
					value += leftWeight * GetPrimaryFuncValue(i, rank - 1, u);
				}
				float rightWeight = 0;
				if (Utilities.isFloatEqual(u_ip_1, u_i_1) == false)
				{ 
					rightWeight = (u_ip_1 - u) / (u_ip_1 - u_i_1);
					value += rightWeight * GetPrimaryFuncValue(i + 1, rank - 1, u);
				}
			}

			_primaryFuncDict.Add(param, value);
			return value;
		}

		public static List<float> GenerateKnotVector(int knotCount, uint rank, bool isUniform)
		{
			if (knotCount < 2)
				return null;

			List<float> result = null;

			if (isUniform)
			{
				result = new List<float>(knotCount);

				float stepLength = 1.0f / (knotCount - 1);
				for (int i = 0; i < knotCount; i++)
				{
					float value = stepLength * i;
					result.Add(value);
				}
			}
			else
			{
				int multiplicity = (int)rank + 1;
				int remainPointCount = knotCount - 2 * multiplicity;
				if (remainPointCount >= 0)
				{
					result = new List<float>(knotCount);
					
					for (int i = 0; i < multiplicity; i++) 
						result.Add(0);
					float stepLength = 1f / (remainPointCount + 1);
					for (int i = 1; i <= remainPointCount; i++)
						result.Add(stepLength * i);
					for (int i = 0; i < multiplicity; i++)
						result.Add(1);
				}
				else
				{
					// do nothing.
				}
			}

			return result;
		}
	}
}