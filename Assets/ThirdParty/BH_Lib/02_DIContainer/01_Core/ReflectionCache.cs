using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace BH_Lib.DI
{
    /// <summary>
    /// 리플렉션 정보를 캐시하여 성능을 최적화하는 클래스입니다.
    /// </summary>
    public class ReflectionCache
    {
        private readonly ConcurrentDictionary<Type, ConstructorInfo[]> _constructorCache = new ConcurrentDictionary<Type, ConstructorInfo[]>();
        private readonly ConcurrentDictionary<Type, FieldInfo[]> _fieldCache = new ConcurrentDictionary<Type, FieldInfo[]>();
        private readonly ConcurrentDictionary<Type, PropertyInfo[]> _propertyCache = new ConcurrentDictionary<Type, PropertyInfo[]>();
        private readonly ConcurrentDictionary<Type, MethodInfo[]> _methodCache = new ConcurrentDictionary<Type, MethodInfo[]>();

        /// <summary>
        /// 캐시된 생성자 정보를 가져옵니다.
        /// </summary>
        public ConstructorInfo[] GetConstructors(Type type)
        {
            return _constructorCache.GetOrAdd(type, t => 
                t.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
        }

        /// <summary>
        /// 캐시된 필드 정보를 가져옵니다.
        /// </summary>
        public FieldInfo[] GetFields(Type type)
        {
            return _fieldCache.GetOrAdd(type, t => 
                t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
        }

        /// <summary>
        /// 캐시된 프로퍼티 정보를 가져옵니다.
        /// </summary>
        public PropertyInfo[] GetProperties(Type type)
        {
            return _propertyCache.GetOrAdd(type, t => 
                t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
        }

        /// <summary>
        /// 캐시된 메소드 정보를 가져옵니다.
        /// </summary>
        public MethodInfo[] GetMethods(Type type)
        {
            return _methodCache.GetOrAdd(type, t => 
                t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
        }

        /// <summary>
        /// 캐시를 초기화합니다.
        /// </summary>
        public void Clear()
        {
            _constructorCache.Clear();
            _fieldCache.Clear();
            _propertyCache.Clear();
            _methodCache.Clear();
        }
    }
}