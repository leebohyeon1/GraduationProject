// ExcelConverterInitializer.cs
using UnityEngine;
using BH_Lib.DI;
using UnityEditor;
using BH_Lib.ExcelConverter.Core;
using BH_Lib.ExcelConverter.Convert;
using BH_Lib.ExcelConverter.Settings;
using BH_Lib.ExcelConverter.Utility;

namespace BH_Lib.ExcelConverter
{
    /// <summary>
    /// Excel 컨버터 모듈 초기화 담당 클래스
    /// </summary>
    [InitializeOnLoad]
    public static class ExcelConverterInitializer
    {
        /// <summary>
        /// 정적 생성자 - 에디터 로드 시 호출됨
        /// </summary>
        static ExcelConverterInitializer()
        {
            Initialize();
        }

        /// <summary>
        /// 모듈 초기화 메서드
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            // DIContainer에 필요한 객체들 등록
            RegisterServices();

            UnityEngine.Debug.Log("Excel Converter 모듈 초기화 완료");
        }

        /// <summary>
        /// DIContainer에 필요한 서비스들 등록
        /// </summary>
        private static void RegisterServices()
        {
            var container = DIContainer.Instance;

            // DataConverter 등록
            container.Register<IDataConverter, DataConverter>();

            // SyncUtility 등록
            container.Register<ISyncUtility, SyncUtility>();

            // SettingsProvider 등록
            container.RegisterInstance<ISettingsProvider>(ExcelSyncSettingsSO.Instance);

            // 어셈블리에서 [Register] 어트리뷰트가 적용된 타입들 자동 등록
            container.RegisterAssemblyTypes(typeof(ExcelConverterInitializer).Assembly);
        }
    }
}