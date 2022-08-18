using Zenject;

namespace Dythervin.AutoAttach.Zenject
{
    public abstract class ComponentGlobalInstaller : MonoInstaller
    {
        protected void SubContainerInHierarchySingle<T>()
        {
            Container.Bind<T>().FromSubContainerResolve().ByMethod(container => InnerInstaller<T>(container)).AsSingle();
        }


        public static void InnerInstaller<T>(DiContainer container)
        {
            container.BindInterfacesAndSelfTo<T>().FromComponentInHierarchy().AsSingle();
        }
    }
}