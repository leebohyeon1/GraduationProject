using UnityEngine;
using BH_Lib.DI;
using BH_Lib.Log;

namespace ProjectStudio.DI.Examples
{
    /// <summary>
    /// DI Container 사용 예제를 위한 인터페이스 정의
    /// </summary>
    public interface IExampleService
    {
        void DoSomething();
        string GetData();
    }

    /// <summary>
    /// DI Container에 자동으로 등록되는 서비스 구현
    /// </summary>
    [Register(typeof(IExampleService))]
    public class ExampleService : IExampleService
    {
        private readonly string _data;

        public ExampleService()
        {
            _data = "Default Example Data";
            Log.Print("ExampleService created!");
        }

        public void DoSomething()
        {
            Log.Print("ExampleService doing something...");
        }

        public string GetData()
        {
            return _data;
        }
    }

    /// <summary>
    /// DI를 활용한 MonoBehaviour 사용 예제
    /// </summary>
    public class ExampleController : DIMonoBehaviour
    {
        // 프로퍼티 주입 예제
        [Inject]
        public IExampleService ExampleService { get; private set; }

        // 필드 주입 예제
        [Inject]
        private IExampleService _anotherExampleService;

        protected override void Awake()
        {
            // 반드시 base.Awake()를 호출하여 의존성 주입을 활성화해야 합니다
            base.Awake();

            Log.Print("ExampleController initialized");
        }

        private void Start()
        {
            // 주입된 서비스 사용
            ExampleService.DoSomething();
            Log.Print($"Service data: {_anotherExampleService.GetData()}");
        }
    }

    /// <summary>
    /// 수동 주입 예제를 위한 일반 클래스
    /// </summary>
    public class ManualInjectionExample
    {
        [Inject]
        private IExampleService _exampleService;

        // 생성자 주입 예제 (MonoBehaviour가 아닌 클래스에서 사용 가능)
        // DIContainer에 의해 이 생성자가 자동으로 선택됩니다 (생성자에는 [Inject] 어트리뷰트가 필요하지 않음)
        public ManualInjectionExample(IExampleService exampleService)
        {
            // 주입된 의존성 저장
            Log.Print($"Constructor injection: {exampleService.GetData()}");
        }

        // 이 클래스의 인스턴스를 직접 생성하는 경우, 수동으로 주입을 호출해야 합니다
        public void Initialize()
        {
            DIContainer.Instance.InjectInto(this);
            
            if (_exampleService != null)
            {
                _exampleService.DoSomething();
            }
        }
    }

    /// <summary>
    /// DI Container 설정 및 사용 예제
    /// </summary>
    public class DIExampleSetup : MonoBehaviour
    {
        private void Start()
        {
            // DIContainerInitializer가 이미 등록을 처리했지만, 수동 등록 예제를 보여줍니다
            
            // 인터페이스와 구현체 직접 등록
            DIContainer.Instance.Register<IExampleService, ExampleService>();
            
            // 인스턴스 직접 등록
            var customService = new ExampleService();
            DIContainer.Instance.RegisterInstance<IExampleService>(customService, "CustomService");
            
            // 리졸브 예제
            var service = DIContainer.Instance.Resolve<IExampleService>();
            service.DoSomething();
            
            // ID를 통한 리졸브 예제
            var customServiceInstance = DIContainer.Instance.ResolveById<IExampleService>("CustomService");
            Log.Print($"Custom service data: {customServiceInstance.GetData()}");
            
            // 일반 클래스에 대한 수동 주입 예제
            var manualExample = new ManualInjectionExample(service);
            manualExample.Initialize();
        }
    }
}
