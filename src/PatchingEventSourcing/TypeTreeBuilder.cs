using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace PatchingEventSourcing {
    public class TypeTreeBuilder {
        private readonly Type _enumerableType = typeof(IEnumerable);
        private readonly Type _collectionType = typeof(ICollection<>);
        public IDictionary<string, PropertyAccessor> Build<T>() {
            return Build(typeof(T));
        }

        public IDictionary<string, PropertyAccessor> Build(Type type)
        {
            var tree = new Dictionary<string, PropertyAccessor>();
            RegisterPaths(string.Empty, type, new List<PropertyInfo>(), tree);
            return tree;
        }

        private void RegisterPaths(string path, Type type, IList<PropertyInfo> parentPropertyChain, IDictionary<string, PropertyAccessor> tree) {
            foreach (var property in type.GetProperties()) {
                var propertyChain = new List<PropertyInfo>();
                propertyChain.AddRange(parentPropertyChain);
                propertyChain.Add(property);
                var newPath = path + "/" + property.Name;

                if (_enumerableType.IsAssignableFrom(property.PropertyType)) {
                    var lastType = property.PropertyType;

                    if (lastType.IsGenericType) {
                        var genericType = lastType.GetGenericArguments()[0];
                        var genericCollectionType = _collectionType.MakeGenericType(genericType);
                        if (!genericCollectionType.IsAssignableFrom(property.PropertyType)) {
                            throw new NotSupportedException();
                        }

                        tree.Add(newPath, new PropertyAccessor(propertyChain, genericType, genericCollectionType));
                        continue;
                    }
                }

                if (property.PropertyType.IsValueType || typeof(string) == property.PropertyType) {
                    tree.Add(newPath, new PropertyAccessor(propertyChain));
                    continue;
                }

                RegisterPaths(newPath, property.PropertyType, propertyChain, tree);
            }
        }
    }
}