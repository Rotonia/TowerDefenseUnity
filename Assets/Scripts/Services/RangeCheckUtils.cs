using UnityEngine;

namespace DefaultNamespace
{
    public class RangeCheckUtils
    {
        public static bool InRangePos(Vector3 pos, Vector3 pos2, float range, out Vector3 closestPoint, out float distanceBetween)
        {
            closestPoint = Vector3.zero;
            distanceBetween = 0;
            
            float centerDist = Vector3.Distance(pos2, pos);
            if (centerDist <= range)
            {
                closestPoint = pos2;
                distanceBetween = centerDist;
                return true;
            }
            
            return false;
        }
        
        public static bool InRangeBounds(Vector3 pos, Bounds bounds, float range, out Vector3 closestPoint, out float distanceBetween)
        {
            closestPoint = Vector3.zero;
            distanceBetween = 0;
            Vector3 point = bounds.ClosestPoint(pos);
            float dist = Vector3.Distance(point, pos);
            if (dist <= range)
            {
                closestPoint = point;
                distanceBetween = dist;
                return true;
            }

            return false;
            
        }
    }
}