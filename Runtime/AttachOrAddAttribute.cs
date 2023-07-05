using System.Diagnostics;
using Dythervin.Core.Utils;

namespace Dythervin.AutoAttach
{
    [Conditional(Symbols.UNITY_EDITOR)]
    public class AttachOrAddAttribute : AttachAttribute
    {
        public AttachOrAddAttribute(bool isReadOnly = true) : base(isReadOnly) { }
    }
}