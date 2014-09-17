using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace PatchingEventSourcing {
    public class TypeTreeCache {
        private readonly TypeTreeBuilder _builder;
        private readonly IDictionary<Type, TypeInfo> _propertyPaths;

        public TypeTreeCache(TypeTreeBuilder builder) {
            if (builder == null) throw new ArgumentNullException("builder");
            _builder = builder;
            _propertyPaths = new Dictionary<Type, TypeInfo>();
        }

        public TypeInfo GetOrCreate(Type type) {
            TypeInfo tree;

            if (_propertyPaths.TryGetValue(type, out tree)) {
                return tree;
            }

            tree = new TypeInfo(type, _builder.Build(type));
            _propertyPaths.Add(type, tree);

            foreach (var propertyAccessor in tree.Accessors.Values) {
                if (!propertyAccessor.HasGenericType) {
                    continue;
                }

                if (_propertyPaths.ContainsKey(propertyAccessor.GenericType)) {
                    continue;
                }

                _propertyPaths.Add(propertyAccessor.GenericType, new TypeInfo(propertyAccessor.GenericType, _builder.Build(propertyAccessor.GenericType)));
            }

            return tree;
        }

        public TypeInfo GetOrCreate<T>() {
            return GetOrCreate(typeof(T));
        }
    }

    public class TypeInfo
    {
        public TypeInfo(Type type, IDictionary<string, PropertyAccessor> accessors)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (accessors == null) throw new ArgumentNullException("accessors");
            Type = type;
            Accessors = accessors;

            Activator = CreateCtor(type);
        }

        public ObjectActivator Activator { get; set; }

        public Type Type { get; private set; }
        public IDictionary<string, PropertyAccessor> Accessors { get; private set; }

        public object CreateInstance()
        {
            return Activator();
        }

        public static ObjectActivator CreateCtor(Type type) {
            var emptyConstructor = type.GetConstructor(Type.EmptyTypes);
            var dynamicMethod = new DynamicMethod("CreateInstance", type, Type.EmptyTypes, true);
            var ilGenerator = dynamicMethod.GetILGenerator();
            ilGenerator.Emit(OpCodes.Nop);
            ilGenerator.Emit(OpCodes.Newobj, emptyConstructor);
            ilGenerator.Emit(OpCodes.Ret);
            return (ObjectActivator)dynamicMethod.CreateDelegate(typeof(ObjectActivator));
        }

        public delegate object ObjectActivator();
    }


}