using System;
using UnityEngine;
using Zenject;

namespace Dythervin.AutoAttach.Zenject
{
    public abstract class ComponentInstaller<TComponent> : MonoInstaller
    {
        [SerializeField, AttachDynamic(nameof(attachType))] protected TComponent component;
        [SerializeField] protected Attach attachType = Attach.ZenjectContext;
        [SerializeField] private bool includeInterfaces;
        [SerializeField] private bool getType;

        public override void InstallBindings()
        {
            Bind(component);
        }

        protected void Bind<T>(T value)
        {
            Type type = getType ? value.GetType() : typeof(T);
            if (includeInterfaces)
                Container.BindInterfacesAndSelfTo(type).FromInstance(value).AsSingle();
            else
                Container.Bind(type).FromInstance(value).AsSingle();
        }
    }
}