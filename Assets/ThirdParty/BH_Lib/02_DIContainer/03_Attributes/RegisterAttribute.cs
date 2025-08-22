using System;

namespace BH_Lib.DI
{
    /// <summary>
    /// 클래스를 DI 컨테이너에 자동으로 등록하기 위한 어트리뷰트
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class RegisterAttribute : Attribute
    {
        public Type[] AsTypes { get; private set; }
        public LifetimeScope Lifetime { get; private set; }

        /// <summary>
        /// 클래스를 DI 컨테이너에 등록합니다.
        /// </summary>
        /// <param name="lifetime">객체의 수명 범위</param>
        public RegisterAttribute(LifetimeScope lifetime = LifetimeScope.Singleton)
        {
            Lifetime = lifetime;
            AsTypes = null;
        }

        /// <summary>
        /// 클래스를 DI 컨테이너에 지정된 인터페이스 타입으로 등록합니다.
        /// </summary>
        /// <param name="asType">등록할 타입 (대개 인터페이스)</param>
        /// <param name="lifetime">객체의 수명 범위</param>
        public RegisterAttribute(Type asType, LifetimeScope lifetime = LifetimeScope.Singleton)
        {
            Lifetime = lifetime;
            AsTypes = new[] { asType };
        }

        /// <summary>
        /// 클래스를 DI 컨테이너에 여러 인터페이스 타입으로 등록합니다.
        /// </summary>
        /// <param name="asTypes">등록할 타입 배열 (대개 인터페이스들)</param>
        /// <param name="lifetime">객체의 수명 범위</param>
        public RegisterAttribute(Type[] asTypes, LifetimeScope lifetime = LifetimeScope.Singleton)
        {
            Lifetime = lifetime;
            AsTypes = asTypes;
        }
    }
}
