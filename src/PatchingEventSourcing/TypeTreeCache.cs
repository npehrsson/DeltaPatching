using System;
using System.Collections.Generic;
using System.Reflection;

namespace PatchingEventSourcing
{
    public class TypeTreeCache
    {
        private readonly TypeTreeBuilder _builder;
        private readonly IDictionary<Type, IDictionary<string, IList<PropertyInfo>>> _propertyPaths;

        public TypeTreeCache(TypeTreeBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException("builder");
            _builder = builder;
            _propertyPaths = new Dictionary<Type, IDictionary<string, IList<PropertyInfo>>>();
        }

        public IDictionary<string, IList<PropertyInfo>> GetOrCreate<T>()
        {
            IDictionary<string, IList<PropertyInfo>> tree;
            var type = typeof (T);

            if (_propertyPaths.TryGetValue(type, out tree))
            {
                return tree;
            }

            tree = _builder.Build<T>();

            _propertyPaths.Add(type, tree);

            return tree;
        }
    }
}