using System.Reflection;

namespace Dythervin.AutoAttach.Setters
{
    public class FieldData
    {
        public readonly AttachAttribute attribute;
        private readonly FieldInfo[] _fieldInfos;

        public FieldInfo Field => _fieldInfos[_fieldInfos.Length - 1];

        public FieldData(AttachAttribute attribute, FieldInfo[] fieldInfos)
        {
            this.attribute = attribute;
            _fieldInfos = fieldInfos;
        }

        public object GetContext(object obj)
        {
            for (int i = 0; i < _fieldInfos.Length - 1; i++)
                obj = _fieldInfos[i].GetValue(obj);

            return obj;
        }
    }
}