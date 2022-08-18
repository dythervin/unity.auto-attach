using Dythervin.SerializedReference.Refs;
using UnityEngine;

namespace Dythervin.AutoAttach.Zenject
{
    public abstract class ComponentPtrInstaller<TComponent> :
#if DYTHERVIN_REF
        ComponentInstaller<ObjPtr<TComponent>>
        where TComponent : Object
    {
        public override void InstallBindings()
        {
            base.InstallBindings();
            if (component.HasValue)
                Bind(component.Value);
        }
    }
#else
    ComponentInstaller<TComponent>{}
#endif
}