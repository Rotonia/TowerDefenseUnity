using System;
using Actors;
using Events;
using Services;
using Zenject;

namespace Installers
{
    public class BattleSceneInstaller: MonoInstaller
    {
        [Inject]
        Settings _settings = null;
        public override void InstallBindings()
        {
            //Setup Services and singletons
            Container.BindInterfacesAndSelfTo<TargetFinder>().AsSingle();
            Container.BindInterfacesAndSelfTo<TargetRegistry>().AsSingle();
            Container.BindInterfacesAndSelfTo<InventoryService>().AsSingle();
            
            //Setup Factories
            ProjectileActor.BindActor(Container, 20, _settings.projectileActorPrefab, "Projectiles");
            CreepController.BindActor(Container, 20, _settings.creepPrefab, "Creeps");
            TurretActor.BindActor(Container, 20, _settings.turretPrefab, "Turrets");
            WeaponActor.BindActor(Container, 20, _settings.weaponPrefab, "Weapons");
            BuildingActor.BindActor(Container, 20, _settings.buildingPrefab, "Buildings");

            //Setup Signals
            SignalBusInstaller.Install(Container);
            Container.DeclareSignal<ActorCreatedSignal>();
            Container.DeclareSignal<ActorRemovedSignal>();
            Container.DeclareSignal<PipsGainedSignal>();
            Container.DeclareSignal<BuildingPlacedSignal>();
            Container.DeclareSignal<DiceSpentSignal>();
            Container.DeclareSignal<InventoryDiceChangeSignal>();
            Container.DeclareSignal<InventoryBuildingPurchaseSignal>();
            Container.DeclareSignal<InventoryBuildingPlacementSignal>();
            Container.DeclareSignal<InventoryBuildingPurchaseCompleteSignal>();
            Container.DeclareSignal<InventoryBuildingPurchaseCancelSignal>();
            Container.DeclareSignal<StartBattleSignal>();
            Container.DeclareSignal<EndBattleSignal>();
        }
        
        [Serializable]
        public class Settings
        {
            public ProjectileActor projectileActorPrefab;
            public CreepController creepPrefab;
            public TurretActor turretPrefab;
            public WeaponActor weaponPrefab;
            public BuildingActor buildingPrefab;
        }
    }
}