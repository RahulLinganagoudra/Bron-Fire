using UnityEngine;

namespace MyUtils
{
    public static class Utilities
    {
        #region Vector
        public static Vector3 AngleToVectorXY(this Vector3 vector, float radians)
        {
            Vector3 direction=vector.normalized;
            float angleofVector=-Vector3.SignedAngle(Vector3.right,direction,Vector3.up);
            float theta = radians + angleofVector * Mathf.Deg2Rad;
            return new Vector3(Mathf.Cos(theta), Mathf.Sin(theta),0f);
            
        }
        public static Vector3 AngleToVectorXZ(this Vector3 vector, float radians)
        {
            Vector3 direction=vector.normalized;
            float angleofVector=-Vector3.SignedAngle(Vector3.right,direction,Vector3.up);
            float theta = radians + angleofVector * Mathf.Deg2Rad;
            return new Vector3(Mathf.Cos(theta),0f, Mathf.Sin(theta));
            
        }
        public static Vector2 AngleToVector(this Vector2 vector, float radians)
        {
            Vector2 direction=vector.normalized;

            float angleofVector=-Vector3.SignedAngle(Vector2.right,direction,Vector2.up);
            float theta = radians + angleofVector * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(theta), Mathf.Sin(theta));
            
        }
        public static bool SquaredDistanceLessThanCheck(this Vector3 vector,float distance)
        {
            return vector.sqrMagnitude < distance*distance;
        }



        #endregion

        public static bool IsPlayer(this Component component)
        {
            return component != null && component.CompareTag("Player");
        }

    }
}
