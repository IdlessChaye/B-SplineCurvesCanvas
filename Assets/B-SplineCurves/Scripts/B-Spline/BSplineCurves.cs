using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chaye
{
	public static class BSplineCurves
	{
		public static int Segments = 100;

		private static Dictionary<PrimaryParam, float> _primaryFuncDict = new Dictionary<PrimaryParam, float>();

		private static List<KnotPoint> _knotVector = default;

		public static Vector3 GetPoint(Path path, float u)
		{
			var point = Vector3.zero;

			uint rank = path.Rank;
			_knotVector = path.KnotVector;
			if (_knotVector.Count == 0 || _knotVector.Count <= rank)
				return point;

			//float uStart = _knotVector[(int)rank].Value;
			//float uEnd = _knotVector[_knotVector.Count - (int)rank - 1].Value;
			if (u >= 0 && u <= 1)
			{
				_primaryFuncDict.Clear();
				for (int i = 0; i < path.ControlPoints.Count; i++)
				{
					point += GetPrimaryFuncValue((uint)i, rank, u) * path.ControlPoints[i].Anchor;
				}
			}

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

		public static List<float> GenerateKnotVector(int knotCount)
		{
			if (knotCount < 2)
				return null;

			var result = new List<float>();
			float stepLength = 1.0f / (knotCount - 1);

			for (int i = 0; i < knotCount; i++)
			{
				float value = stepLength * i;
				result.Add(value);
			}
			
			return result;
		}
	}
}