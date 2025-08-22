using System;
using System.Reflection;

namespace BH_Lib.DI
{
    /// <summary>
    /// DI 컨테이너를 위한 인터페이스 정의
    /// 의존성 역전 원칙(DIP)을 따르기 위한 추상화입니다.
    /// </summary>
    public interface IDIContainer
    {
        /// <summary>
        /// 모든 등록된 서비스를 초기화합니다.
        /// </summary>
        void ResetContainer();

        /// <summary>
        /// 서비스를 등록합니다.
        /// </summary>
        /// <typeparam name="TService">서비스 타입</typeparam>
        /// <typeparam name="TImplementation">구현 타입</typeparam>
        /// <param name="lifetime">생명주기</param>
        /// <param name="id">선택적 ID</param>
        void Register<TService, TImplementation>(LifetimeScope lifetime = LifetimeScope.Singleton, string id = null)
            where TImplementation : TService;

        /// <summary>
        /// 서비스를 등록합니다.
        /// </summary>
        /// <param name="serviceType">서비스 타입</param>
        /// <param name="implementationType">구현 타입</param>
        /// <param name="lifetime">생명주기</param>
        /// <param name="id">선택적 ID</param>
        void Register(Type serviceType, Type implementationType, LifetimeScope lifetime = LifetimeScope.Singleton, string id = null);

        /// <summary>
        /// 인스턴스를 직접 등록합니다.
        /// </summary>
        /// <typeparam name="TService">서비스 타입</typeparam>
        /// <param name="instance">등록할 인스턴스</param>
        /// <param name="id">선택적 ID</param>
        void RegisterInstance<TService>(TService instance, string id = null);

        /// <summary>
        /// 인스턴스를 직접 등록합니다.
        /// </summary>
        /// <param name="serviceType">서비스 타입</param>
        /// <param name="instance">등록할 인스턴스</param>
        /// <param name="id">선택적 ID</param>
        void RegisterInstance(Type serviceType, object instance, string id = null);

        /// <summary>
        /// 특정 타입의 서비스를 가져옵니다.
        /// </summary>
        /// <typeparam name="T">서비스 타입</typeparam>
        /// <returns>서비스 인스턴스</returns>
        T Resolve<T>();

        /// <summary>
        /// 특정 ID로 등록된 서비스를 가져옵니다.
        /// </summary>
        /// <typeparam name="T">서비스 타입</typeparam>
        /// <param name="id">서비스 ID</param>
        /// <returns>서비스 인스턴스</returns>
        T ResolveById<T>(string id);

        /// <summary>
        /// 특정 타입의 서비스를 가져옵니다.
        /// </summary>
        /// <param name="serviceType">서비스 타입</param>
        /// <returns>서비스 인스턴스</returns>
        object Resolve(Type serviceType);

        /// <summary>
        /// 특정 ID로 등록된 서비스를 가져옵니다.
        /// </summary>
        /// <param name="id">서비스 ID</param>
        /// <returns>서비스 인스턴스</returns>
        object ResolveById(string id);

        /// <summary>
        /// 특정 객체에 의존성을 주입합니다.
        /// </summary>
        /// <param name="instance">의존성을 주입할 객체</param>
        void InjectInto(object instance);

        /// <summary>
        /// Assembly에서 RegisterAttribute가 지정된 모든 타입을 자동으로 등록합니다.
        /// </summary>
        void RegisterAssemblyTypes();

        /// <summary>
        /// 특정 Assembly에서 RegisterAttribute가 지정된 모든 타입을 자동으로 등록합니다.
        /// </summary>
        /// <param name="assembly">스캔할 Assembly</param>
        void RegisterAssemblyTypes(Assembly assembly);
    }
}