using System.Collections.Generic;
using Actors;
using DefaultNamespace;
using UnityEngine;

namespace Services
{
    public interface ITargetFinder
    {
        RangeCheckResults GetTargetsInRangeForCategories(int team, string[] categories, Vector3 position, float range);
    }
    
    public class TargetFinder : ITargetFinder
    {
        private ITargetRegistry _targetRegistry;

        public TargetFinder(ITargetRegistry targetRegistry)
        {
            _targetRegistry = targetRegistry;
        }

        public RangeCheckResults GetTargetsInRangeForCategories(int team, string[] categories, Vector3 position, float range)
        {
            RangeCheckResults targetsInRange = new RangeCheckResults(position, range);

            foreach (var category in categories)
            {
                HashSet<ITargetable> targets = _targetRegistry.GetTargetsForTeamByCategory(team, category);
                if (targets != null)
                {
                    foreach (var target in targets)
                    {
                        if (target.IsAlive() && !target.IsPaused())
                        {
                            targetsInRange.EvaluateTarget(target);
                        }
                    }
                }
            }

            return targetsInRange;
        }
    }
}