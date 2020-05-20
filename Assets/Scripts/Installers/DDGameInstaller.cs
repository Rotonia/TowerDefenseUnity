using Actors;
using Core;
using Data;
using Zenject;

namespace Installers
{
    public class DDGameInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<DataLoader>().AsSingle();
            Container.BindInterfacesAndSelfTo<AssetResourceLoader>().AsSingle();
        }
    }
}