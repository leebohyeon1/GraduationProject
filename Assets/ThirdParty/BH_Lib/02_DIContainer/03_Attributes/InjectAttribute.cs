using System;

namespace BH_Lib.DI
{
    /// <summary>
    /// 의존성 주입을 위한 어트리뷰트입니다.
    /// 필드, 프로퍼티, 생성자 매개변수에 적용할 수 있습니다.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public class InjectAttribute : Attribute
    {
        public string Id { get; private set; }

        /// <summary>
        /// 기본 생성자 - 타입에 따라 적절한 의존성을 주입합니다.
        /// </summary>
        public InjectAttribute()
        {
            Id = null;
        }

        /// <summary>
        /// 특정 ID를 가진 의존성을 주입할 때 사용합니다.
        /// </summary>
        /// <param name="id">의존성의 고유 ID</param>
        public InjectAttribute(string id)
        {
            Id = id;
        }
    }
}
