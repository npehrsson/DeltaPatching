using System;

namespace PatchingEventSourcing.ValueTypes {
    public interface IValueType {
        Type Type { get; }
        bool TryParse(string data, out object value);
        object Parse(string data);
    }
}
