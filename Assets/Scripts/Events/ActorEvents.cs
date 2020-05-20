using Actors;

namespace Events
{
    public struct StartBattleSignal
    { }
    
    public struct EndBattleSignal
    { }
    
    public struct ActorCreatedSignal
    {
        public ITargetable target;
    }

    public struct ActorRemovedSignal
    {
        public ITargetable target;
    }
}