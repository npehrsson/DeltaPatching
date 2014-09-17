using System;
using System.Collections.Generic;
using System.Linq;
using PatchingEventSourcing.ValueTypes;

namespace PatchingEventSourcing {
    public class PatchValidator {
        private readonly TypeTreeCache _typeTreeCache;
        private readonly IDictionary<Type, IValueType> _valueTypes;

        public PatchValidator(TypeTreeCache typeTreeCache, IDictionary<Type, IValueType> valueTypes) {
            if (typeTreeCache == null) throw new ArgumentNullException("typeTreeCache");
            if (valueTypes == null) throw new ArgumentNullException("valueTypes");
            _typeTreeCache = typeTreeCache;
            _valueTypes = valueTypes;
        }

        public bool Validate<T>(Patch patch) {
            var typeInfo = _typeTreeCache.GetOrCreate<T>();
            PropertyAccessor propertyChain;

            if (!typeInfo.Accessors.TryGetValue(patch.Path, out propertyChain)) {
                return false;
            }

            return ValidateDataType(propertyChain.PropertyChain.Last().PropertyType, patch.Value);
        }

        private bool ValidateDataType(Type declaringType, string value) {
            IValueType valueType;
            if (!_valueTypes.TryGetValue(declaringType, out valueType)) {
                throw new NotSupportedException();
            }

            object outParameter;
            return valueType.TryParse(value, out outParameter);
        }
    }
}