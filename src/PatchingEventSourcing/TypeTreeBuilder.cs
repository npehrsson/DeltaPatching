using System;
using System.Collections.Generic;
using System.Reflection;

namespace PatchingEventSourcing {
    public class TypeTreeBuilder {
        public IDictionary<string, IList<PropertyInfo>> Build<T>() {
            var tree = new Dictionary<string, IList<PropertyInfo>>();
            RegisterPaths(string.Empty, typeof(T), new List<PropertyInfo>(), tree);

            return tree;
        }

        private void RegisterPaths(string path, Type type, IList<PropertyInfo> parentPropertyChain, IDictionary<string, IList<PropertyInfo>> tree) {
            foreach (var property in type.GetProperties()) {
                var propertyChain = new List<PropertyInfo>();
                propertyChain.AddRange(parentPropertyChain);
                propertyChain.Add(property);
                var newPath = path + "/" + property.Name;

                if (property.PropertyType.IsValueType || typeof(string) == property.PropertyType) {
                    tree.Add(newPath, propertyChain);
                    continue;
                }

                RegisterPaths(newPath, property.PropertyType, propertyChain, tree);
            }
        }
    }
}