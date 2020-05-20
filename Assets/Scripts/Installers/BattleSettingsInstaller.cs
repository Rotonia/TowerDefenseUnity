using System;
using UnityEngine;
using Zenject;

namespace Installers
{
    [CreateAssetMenu(menuName = "DD/Game Settings")]
    public class BattleSettingsInstaller : ScriptableObjectInstaller<BattleSettingsInstaller>
    {
        public BattleSceneInstaller.Settings BattleInitSettings;
        
        [Serializable]
        public class SystemSettings
        {
            public BattleSceneInstaller.Settings BattleInitSettings;
        }

        public override void InstallBindings()
        {
            // Use IfNotBound to allow overriding for eg. from play mode tests
            //Container.BindInstance(EnemySpawner).IfNotBound();
            //Container.BindInstance(GameRestartHandler).IfNotBound();
            //Container.BindInstance(GameInstaller).IfNotBound();

            Container.BindInstance(BattleInitSettings).IfNotBound();
        }
    }
}

