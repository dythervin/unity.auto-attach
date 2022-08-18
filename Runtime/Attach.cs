namespace Dythervin.AutoAttach
{
    public enum Attach : byte
    {
        Default = 0,
        Child = 1,
        Parent = 2,
        Scene = 3,
#if ZENJECT
        ZenjectContext = 4
#endif
    }
}