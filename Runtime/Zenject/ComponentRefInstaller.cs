using Dythervin.SerializedReference.Refs;

namespace Dythervin.AutoAttach.Zenject
{
    public abstract class ComponentRefInstaller<TComponent> :

#if DYTHERVIN_REF
        ComponentInstaller<Ref<TComponent>>
        where TComponent :
        class

    {
        public override void InstallBindings()
        {
            Bind(component.Value);
        }
    }
#else
        ComponentInstaller<TComponent> { }
#endif
}