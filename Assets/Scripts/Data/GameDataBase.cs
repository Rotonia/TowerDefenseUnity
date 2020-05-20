using System;
using UnityEngine;

namespace Data
{
    [Serializable]
    public abstract class GameDataBase : ScriptableObject
    {
        public abstract string category { get; }
        public abstract string nameInHierarchy { get; }
        public abstract int Id { get; }
    }
}