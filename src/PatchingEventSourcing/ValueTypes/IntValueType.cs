using System;

namespace PatchingEventSourcing.ValueTypes
{
    public class IntValueType : IValueType
    {
        public Type Type { get { return typeof (int); } }
        public bool TryParse(string data, out object value)
        {
            int parameter;
            if (int.TryParse(data, out parameter))
            {
                value = parameter;
                return true;
            }
            value = null;
            return false;
        }

        public object Parse(string data)
        {
            return int.Parse(data);
        }
    }
}