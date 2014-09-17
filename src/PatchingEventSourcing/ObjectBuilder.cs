using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PatchingEventSourcing.ValueTypes;

namespace PatchingEventSourcing {
    public class ObjectBuilder<T> {
        private readonly TypeTreeCache _typeTreeCache;
        private readonly IDictionary<Type, IValueType> _valueTypes;
        private readonly TypeInfo _typeInfo;
        private readonly Regex _splitRegex = new Regex(@"/\d+", RegexOptions.Compiled);
        private readonly Regex _indexesRegex = new Regex(@"\d+", RegexOptions.Compiled);
        public ObjectBuilder(TypeTreeCache typeTreeCache, IDictionary<Type, IValueType> valueTypes) {

            if (typeTreeCache == null) throw new ArgumentNullException("typeTreeCache");
            if (valueTypes == null) throw new ArgumentNullException("valueTypes");
            _typeInfo = typeTreeCache.GetOrCreate<T>();
            _typeTreeCache = typeTreeCache;
            _valueTypes = valueTypes;
        }

        public T Build(IEnumerable<Patch> patches) {
            if (patches == null) throw new ArgumentNullException("patches");
            var entity = Activator.CreateInstance<T>();

            Update(entity, patches);

            return entity;
        }

        public void Update(object entity, IEnumerable<Patch> patches) {
            foreach (var patch in patches) {
                ApplyPatch(entity, patch);
            }
        }

        public void ApplyPatch(object entity, Patch patch) {
            var accessors = GetAccessors(patch.Path);
            var indexes = GetIndexes(patch.Path);

            var currentValue = GetLeafValue(entity, accessors, indexes);
            var lastAccessor = accessors.Last();
 
            if (patch.Operation == "replace") {
                var lastPropertyInfo = lastAccessor.PropertyChain.Last();
                var value = GetValue(patch.Value, lastPropertyInfo.PropertyType);
                lastPropertyInfo.SetValue(currentValue, value);
                return;
            }

            if (patch.Operation == "add") {
                var value = CreateInstance(lastAccessor.GenericType);
                lastAccessor.AddToCollection(currentValue, value, indexes[indexes.Length - 1]);
                return;
            }

            if (patch.Operation == "remove") {
                lastAccessor.RemoveAt(currentValue, indexes[indexes.Length - 1]);
            }
        }

        private object GetLeafValue(object value, IList<PropertyAccessor> accessors, int[] indexes) {
            var currentIndexIndex = 0;
 
            foreach (var accessor in accessors) {
                var propertyChain = accessor.PropertyChain;

                foreach (var propertyInfo in propertyChain) {
                    if (!accessor.IsCollection && propertyChain.Last() == propertyInfo) {
                        continue;
                    }

                    var newValue = propertyInfo.GetValue(value);

                    if (newValue != null) {
                        value = newValue;
                        continue;
                    }

                    newValue = CreateInstance(propertyInfo.PropertyType);
                    propertyInfo.SetValue(value, newValue);
                    value = newValue;
                }

                if (!accessor.IsCollection || accessors.Last() == accessor) {
                    continue;
                }

                value = accessor.GetByIndex(value, indexes[currentIndexIndex]);
                currentIndexIndex++;
            }

            return value;
        }

        private object CreateInstance(Type type)
        {
            return _typeTreeCache.GetOrCreate(type).CreateInstance();
        }

        private IList<PropertyAccessor> GetAccessors(string path) {
            var paths = GetPaths(path);

            var accessors = new List<PropertyAccessor>();

            var tree = _typeInfo;

            for (var i = 0; i < paths.Length; i++) {
                if (i != 0) {
                    tree = _typeTreeCache.GetOrCreate(accessors[i - 1].GenericType);
                }

                accessors.Add(tree.Accessors[paths[i]]);
            }

            return accessors;
        }

        public string[] GetPaths(string path) {
            return _splitRegex.Split(path).Where(x => !string.IsNullOrEmpty(x)).ToArray();    
        }

        public int[] GetIndexes(string path) {
            var indexes = new List<int>();

            foreach (Match match in _indexesRegex.Matches(path)) {
                indexes.Add(int.Parse(match.Groups[0].Value));
            }

            return indexes.ToArray();
        }

        private object GetValue(string value, Type declaringType) {
            IValueType valueType;
            if (!_valueTypes.TryGetValue(declaringType, out valueType)) {
                throw new NotSupportedException();
            }

            return valueType.Parse(value);
        }
    }
}
