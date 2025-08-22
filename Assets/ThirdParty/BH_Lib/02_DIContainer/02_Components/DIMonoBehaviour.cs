using UnityEngine;

namespace BH_Lib.DI
{
    /// <summary>
    /// 자동으로 의존성 주입을 받는 MonoBehaviour 확장 클래스입니다.
    /// 이 클래스를 상속받는 컴포넌트는 자동으로 DI 컨테이너에서 필요한 의존성을 주입받습니다.
    /// </summary>
    public abstract class DIMonoBehaviour : MonoBehaviour
    {
        /// <summary>
        /// 의존성 주입이 완료되었는지 표시하는 플래그
        /// </summary>
        private bool _injected = false;
        
        /// <summary>
        /// DI 컨테이너에 대한 참조
        /// </summary>
        private IDIContainer _container => DIContainer.Instance;

        /// <summary>
        /// MonoBehaviour 생명주기 시작시 호출되며, 의존성 주입을 자동으로 실행합니다.
        /// 하위 클래스에서 반드시 base.Awake()를 호출해야 합니다.
        /// </summary>
        protected virtual void Awake()
        {
            // Awake에서 의존성 주입 실행
            InjectDependencies();
        }

        /// <summary>
        /// 컴포넌트가 활성화될 때 호출되며, 의존성 주입이 안되어 있는 경우 자동으로 실행합니다.
        /// 하위 클래스에서 반드시 base.OnEnable()을 호출해야 합니다.
        /// </summary>
        protected virtual void OnEnable()
        {
            // 컴포넌트가 활성화될 때 의존성 주입이 안되어 있으면 실행
            InjectDependencies();
        }

        /// <summary>
        /// 컴포넌트에 의존성을 주입합니다.
        /// 이미 주입되어 있는 경우 추가 주입을 실행하지 않습니다.
        /// </summary>
        public void InjectDependencies()
        {
            if (!_injected)
            {
                _container.InjectInto(this);
                _injected = true;
            }
        }
        
        /// <summary>
        /// 의존성 주입 상태를 초기화하고 다시 주입합니다.
        /// 주의: 대부분의 경우 필요하지 않으며, 특별한 경우에만 사용해야 합니다.
        /// </summary>
        public void ReinjectDependencies()
        {
            _injected = false;
            InjectDependencies();
        }
    }
}
