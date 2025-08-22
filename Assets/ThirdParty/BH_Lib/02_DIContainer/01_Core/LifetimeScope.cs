namespace BH_Lib.DI
{
    /// <summary>
    /// DI 컨테이너에 등록된 객체의 수명 범위를 정의합니다.
    /// </summary>
    public enum LifetimeScope
    {
        /// <summary>
        /// 싱글톤 - 애플리케이션 전체에서 하나의 인스턴스만 생성됩니다.
        /// </summary>
        Singleton,

        /// <summary>
        /// 트랜지언트 - 요청할 때마다 새 인스턴스가 생성됩니다.
        /// </summary>
        Transient,

        /// <summary>
        /// 씬 - 현재 씬에 종속된 인스턴스입니다. 씬이 언로드될 때 자동으로 정리됩니다.
        /// </summary>
        Scene
    }
}
