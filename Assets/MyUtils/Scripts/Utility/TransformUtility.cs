using System.Collections.Generic;
using UnityEngine;
namespace Utilities
{
    public static class TransformUtility
    {
        public static void DestroyChildren(this Transform transform)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                GameObject.Destroy(transform.GetChild(i).gameObject);
            }
        }


        public static void PrintDict(this Dictionary<int, bool> dict)
        {
#if UNITY_EDITOR
            string debug = "";
            foreach (var keyValue in dict)
            {
                debug += $"{keyValue.Key} has {keyValue.Value}\n";
            }
            Debug.Log(debug);
#endif
        }

        public static void FaceToPlayer(this Transform self, Transform player)
        {
            if (Vector3.Dot((player.position - self.position).normalized, self.right) < 0)
            {
                self.localScale = new Vector3(-1, 1, 1);
            }
            else
            {
                self.localScale = Vector3.one;
            }
        }

        public static int GetFurthestPositionFrom(this Transform[] perimeterPositions, Transform reference, int exclude = -1)
        {
            if (perimeterPositions == null || perimeterPositions.Length == 0) return -1;

            int max = 0;
            for (int i = 0; i < perimeterPositions.Length; i++)
            {
                if (i == exclude) continue;
                if (!reference.position.CompareDist(perimeterPositions[i].position, perimeterPositions[max].position))
                {
                    max = i;
                }
            }
            return max;
        }
        public static int GetClosestPositionFrom(this Transform[] perimeterPositions, Transform reference, int exclude = -1)
        {
            if (perimeterPositions == null || perimeterPositions.Length == 0) return -1;
            int min = 0;
            for (int i = 0; i < perimeterPositions.Length; i++)
            {
                if (i == exclude) continue;
                if (reference.position.CompareDist(perimeterPositions[i].position, perimeterPositions[min].position))
                {
                    min = i;
                }
            }
            return min;
        }
    }

}