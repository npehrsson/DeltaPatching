using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PatchingEventSourcing {
    public class PropertyAccessor
    {
        public PropertyAccessor(IList<PropertyInfo> propertyChain) {
            if (propertyChain == null) throw new ArgumentNullException("propertyChain");
            PropertyChain = propertyChain;
        }

        public PropertyAccessor(IList<PropertyInfo> propertyChain, Type genericType, Type collectionType) : this(propertyChain)
        {
            if (genericType == null) throw new ArgumentNullException("genericType");
            GenericType = genericType;
            CollectionType = collectionType;
            AddMethod = CollectionType.GetMethod("Add");
            ItemPropertyInfo = propertyChain.Last().PropertyType.GetProperty("Item");
            RemoveAtMethod = propertyChain.Last().PropertyType.GetMethod("RemoveAt");
            IsCollection = true;
        }

        public MethodInfo AddMethod { get; private set; }
        public PropertyInfo ItemPropertyInfo { get; private set; }

        public MethodInfo RemoveAtMethod { get; private set; }
        public Type GenericType { get; private set; }
        public Type CollectionType { get; private set; }
        public bool IsCollection { get; private set; }
        public IList<PropertyInfo> PropertyChain { get; private set; }
        public bool HasGenericType { get { return GenericType != null; } }

        public void AddToCollection(object collection, object collectionItem, int index)
        {
            AddMethod.Invoke(collection, new [] { collectionItem });
        }

        public object GetByIndex(object collection, int index)
        {
            return ItemPropertyInfo.GetValue(collection, new object[] { index });
        }

        public void RemoveAt(object collection, int index)
        {
            RemoveAtMethod.Invoke(collection, new object[] {index});
        }
    }
}