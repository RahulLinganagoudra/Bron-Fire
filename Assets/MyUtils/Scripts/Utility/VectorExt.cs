using UnityEngine;
namespace Utilities
{
	public static class VectorExt
	{
		public static Vector3 CubicBezier(Vector3 start, Vector3 end, float hieght, float t)
		{
			float _1MinusT = 1 - t;

			Vector3 mid = (start + end) / 2;
			mid.y += hieght;


			return _1MinusT * _1MinusT * start
				+ 2 * _1MinusT * t * mid
				+ t * t * end;
		}
		public static Vector2 Clamp(this Vector2 original, Vector2 min, Vector2 max)
		{
			return new Vector2(
				Mathf.Clamp(original.x, min.x, max.x),
				Mathf.Clamp(original.y, min.y, max.y)
				);
		}
		public static Vector3 Clamp(this Vector3 original, Vector3 min, Vector3 max)
		{
			return new Vector3(
				Mathf.Clamp(original.x, min.x, max.x),
				Mathf.Clamp(original.y, min.y, max.y),
				Mathf.Clamp(original.z, min.z, max.z)
				);
		}

		public static Vector3 CubicBezierDistIndipendent(Vector3 start, Vector3 end, float hieght, float t)
		{
			float sqDist = (end - start).sqrMagnitude;
			float _1MinusT = 1 - t;


			Vector3 mid = (start + end) / 2;
			mid.y += hieght;


			return _1MinusT * _1MinusT * start
				+ 2 * _1MinusT * t * mid
				+ t * t * end;
		}
		public static Vector3 XZ(this Vector3 original)
		{
			return new Vector3(original.x, 0, original.z);
		}
		public static Vector3 XY(this Vector3 original)
		{
			return new Vector3(original.x, original.y, 0);
		}
		public static Vector2 XY2D(this Vector3 original)
		{
			return new Vector2(original.x, original.y);
		}
		public static Vector2 XZ_To_XY(this Vector3 original)
		{
			return new Vector2(original.x, original.z);
		}
		public static Vector3 XY_To_XZ(this Vector2 original)
		{
			return new Vector3(original.x, 0, original.y);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="self"></param>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns>true if distance from a is less than or equal to b</returns>
		public static bool CompareDist(this Vector3 self, Vector3 a, Vector3 b)
		{
			return (self - a).sqrMagnitude <= (self - b).sqrMagnitude;
		}
		public static bool CompareDist(this Vector3 self, Vector3 a, float distance)
		{
			return (self - a).sqrMagnitude <= distance.SQ();
		}
		public static bool IsBetween(this Vector3 self, float minDistance, float maxDistance)
		{
			return self.sqrMagnitude > minDistance.SQ() && self.sqrMagnitude <= maxDistance.SQ();
		}
		public static float SQ(this float original)
		{
			return original * original;
		}


	}
	public class GuidSO : ScriptableObject
	{
		static int uid = 1;
		[SerializeField] int id = 0;
		public int ID => id;
		protected virtual void OnValidate()
		{
			if (id == 0)
				id = uid++;
		}
	}
}