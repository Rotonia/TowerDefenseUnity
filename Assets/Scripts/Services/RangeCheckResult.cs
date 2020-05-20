using System.Collections.Generic;
using Actors;
using UnityEngine;

namespace DefaultNamespace
{
    public class RangeResult
    {
        public readonly ITargetable Target;
        public readonly float Distance;
        public readonly Vector3 ClosestPoint;

        public RangeResult(ITargetable target, float distance, Vector3 closestPoint)
        {
            Target = target;
            Distance = distance;
            ClosestPoint = closestPoint;
        }
    }
    
    public class RangeCheckResults
    {
        public readonly Vector3 Position;
        public readonly float Range;

        public readonly List<RangeResult> rangeResults = new List<RangeResult>();

        public RangeCheckResults(Vector3 pos, float range)
        {
            Position = pos;
            Range = range;
        }
        
        public void EvaluateTarget(ITargetable target)
        {
            if (target.InRange(Position, Range, out Vector3 closestPoint, out float distToTarget))
            {
                rangeResults.Add(new RangeResult(target, distToTarget, closestPoint));
            }
        }

        public RangeResult GetClosestTarget()
        {
            RangeResult closest = null;
            
            foreach (var result in rangeResults)
            {
                if (closest == null)
                {
                    closest = result;
                    continue;
                }
                
                if (closest.Distance > result.Distance)
                {
                    closest = result;
                }
            }

            return closest;
        }
    }
}