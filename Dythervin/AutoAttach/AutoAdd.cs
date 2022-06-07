namespace Dythervin.AutoAttach
{
    public class AttachOrAddAttribute : AttachAttribute
    {
        public AttachOrAddAttribute(bool readOnly = true) : base(readOnly: readOnly) { }
    }
}