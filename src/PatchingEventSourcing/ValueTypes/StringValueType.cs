using System;

namespace PatchingEventSourcing.ValueTypes
{
    public class StringValueType : IValueType
    {
        public Type Type { get { return typeof (string); } }
        public bool TryParse(string data, out object value)
        {
            value = data;
            return true;
        }

        public object Parse(string data)
        {
            return data;
        }
    }
}