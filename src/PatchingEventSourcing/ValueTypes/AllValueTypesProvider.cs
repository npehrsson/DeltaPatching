using System;
using System.Collections.Generic;

namespace PatchingEventSourcing.ValueTypes {
    public static class AllValueTypesProvider {
        public static IDictionary<Type, IValueType> ValueTypes = new Dictionary<Type, IValueType>()
        {
           { new StringValueType().Type, new StringValueType() },
           { new IntValueType().Type, new IntValueType() }
        }; 
    }
}
