using System;

namespace BH_Lib.ExcelConverter.Settings
{
    /// <summary>
    /// 엑셀 파일과 ScriptableObject 간 동기화를 위한 설정 항목 클래스
    /// </summary>
    [Serializable]
    public class SyncSettingItem
    {
        /// <summary>
        /// 동기화 대상 ScriptableObject 이름
        /// </summary>
        public string Name;

        /// <summary>
        /// ScriptableObject 에셋 경로 (Assets/ 이하부터)
        /// </summary>
        public string AssetPath;

        /// <summary>
        /// 엑셀 파일 경로 (Assets/ 이하부터)
        /// </summary>
        public string ExcelPath;

        /// <summary>
        // 엑셀을 수정했을 때 SO 업데이트
        /// </summary>
        public bool EnableExcelToSO = true;

        /// <summary>
        /// SO를 수정했을 때 엑셀 업데이트
        /// </summary>
        public bool EnableSOToExcel = true;
    }
}