using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PatchingEventSourcing.ValueTypes;

namespace PatchingEventSourcing {
    public class ObjectBuilder<T> {
        private readonly IDictionary<Type, IValueType> _valueTypes;
        private readonly IDictionary<string, IList<PropertyInfo>> _tree;

        public ObjectBuilder(TypeTreeCache typeTreeCache, IDictionary<Type, IValueType> valueTypes) {

            if (typeTreeCache == null) throw new ArgumentNullException("typeTreeCache");
            if (valueTypes == null) throw new ArgumentNullException("valueTypes");
            _tree = typeTreeCache.GetOrCreate<T>();
            _valueTypes = valueTypes;
        }

        public T Build(IEnumerable<Patch> patches) {
            if (patches == null) throw new ArgumentNullException("patches");
            var entity = Activator.CreateInstance<T>();


            foreach (var patch in patches) {
                ApplyPatch(entity, patch);
            }

            return entity;
        }

        public void ApplyPatch(object entity, Patch patch) {
            var propertyChain = _tree[patch.Path];
            var value = GetValue(patch.Value, propertyChain.Last().PropertyType);

            var currentValue = entity;

            for (var i = 0; i < propertyChain.Count; i++) {
                var propertyInfo = propertyChain[i];
                if (i == propertyChain.Count - 1) {
                    propertyInfo.SetValue(currentValue, value);
                }

                var newValue = propertyInfo.GetValue(currentValue);

                if (newValue != null) {
                    currentValue = newValue;
                    continue;
                }
                newValue = Activator.CreateInstance(propertyInfo.PropertyType);
                propertyInfo.SetValue(currentValue, newValue);
                currentValue = newValue;
            }
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