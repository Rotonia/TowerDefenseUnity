using System.Collections.Generic;
using Actors;
using Events;
using Zenject;

namespace Services
{
    public interface ITargetRegistry
    {
        HashSet<ITargetable> GetTargetsForTeamByCategory(int team, string category);
    }
    public class TargetRegistry : IInitializable, ITargetRegistry
    {
        readonly SignalBus _signalBus;
        public Dictionary<int, HashSet<ITargetable>> _teamDict = new Dictionary<int, HashSet<ITargetable>>();
        public Dictionary<int, Dictionary<string,HashSet<ITargetable>>> _teamByCategoryDict = new Dictionary<int, Dictionary< string, HashSet<ITargetable>>>();
        
        public TargetRegistry(
            SignalBus signalBus)
        {
            _signalBus = signalBus;
        }
        
        public void Initialize()
        {
            _signalBus.Subscribe<ActorCreatedSignal>(OnActorCreated);
            _signalBus.Subscribe<ActorRemovedSignal>(OnActorRemoved);
        }

        public void OnActorCreated(ActorCreatedSignal signal)
        {
            RegisterTarget(signal.target);
        }
        
        public void OnActorRemoved(ActorRemovedSignal signal)
        {
            DeregisterTarget(signal.target);
        }
        
        public void RegisterTarget(ITargetable target)
        {
           var targets = GetTargetsForTeam(target.Team());
           if (targets == null)
           {
               targets = new HashSet<ITargetable>();
               _teamDict[target.Team()] = targets;
           }

           targets.Add(target);

           RegisterTargetForCategory(target);
        }

        private void RegisterTargetForCategory(ITargetable target)
        {
            if (!_teamByCategoryDict.TryGetValue(target.Team(), out var teamCategories))
            {
                teamCategories = new Dictionary<string, HashSet<ITargetable>>();
                _teamByCategoryDict[target.Team()] = teamCategories;
            }

            if (!teamCategories.TryGetValue(target.Category, out var categoryTargets))
            {
                categoryTargets = new HashSet<ITargetable>();
                teamCategories[target.Category] = categoryTargets;
            }

            categoryTargets.Add(target);
        }

        public HashSet<ITargetable> GetTargetsForTeam(int team)
        {
            _teamDict.TryGetValue(team, out var teamList);
            return teamList;
        }

        public HashSet<ITargetable> GetTargetsForTeamByCategory(int team, string category)
        {
            if (string.IsNullOrEmpty(category))
            {
                return GetTargetsForTeam(team);
            }
            
            if (_teamByCategoryDict.TryGetValue(team, out var teamList))
            {
                teamList.TryGetValue(category, out var categoryList);
                return categoryList;
            }

            return null;
        }
        
        public void DeregisterTarget(ITargetable target)
        {
            var targets = GetTargetsForTeam(target.Team());
            targets?.Remove(target);
            DeregisterTargetForCategory(target);
        }
        
        private void DeregisterTargetForCategory(ITargetable target)
        {
            if (!_teamByCategoryDict.TryGetValue(target.Team(), out var teamCategories))
            {
                return;
            }

            if (!teamCategories.TryGetValue(target.Category, out var categoryTargets))
            {
                return;
            }

            categoryTargets.Remove(target);
        }
    }
}