using System.Collections.Generic;

namespace BH_Lib.ExcelConverter.Settings
{
    /// <summary>
    /// 엑셀 동기화 설정 정보를 제공하는 인터페이스
    /// </summary>
    public interface ISettingsProvider
    {
        /// <summary>
        /// 전역 자동 동기화 활성화 여부
        /// </summary>
        bool GlobalAutoSyncEnabled { get; set; }

        /// <summary>
        /// 로그 출력 여부
        /// </summary>
        bool ShowLogs { get; set; }

        /// <summary>
        /// 자동 동기화 지연 시간(초)
        /// </summary>
        float SyncDelay { get; set; }

        /// <summary>
        /// 동기화 항목 리스트
        /// </summary>
        List<SyncSettingItem> SyncItems { get; }

        /// <summary>
        /// 이름으로 설정 항목 조회
        /// </summary>
        /// <param name="name">설정 항목 이름</param>
        /// <returns>설정 항목</returns>
        SyncSettingItem GetSyncItem(string name);

        /// <summary>
        /// 에셋 경로로 설정 항목 조회
        /// </summary>
        /// <param name="assetPath">에셋 경로</param>
        /// <returns>설정 항목</returns>
        SyncSettingItem GetSyncItemByAssetPath(string assetPath);

        /// <summary>
        /// 새 설정 항목 추가
        /// </summary>
        /// <param name="name">이름</param>
        /// <param name="assetPath">에셋 경로</param>
        /// <param name="excelPath">엑셀 파일 경로</param>
        /// <param name="autoSync">자동 동기화 여부</param>
        /// <returns>추가된 설정 항목</returns>
        SyncSettingItem AddSyncItem(string name, string assetPath, string excelPath, bool autoSync);

        /// <summary>
        /// 설정 항목 삭제
        /// </summary>
        /// <param name="name">이름</param>
        /// <returns>삭제 성공 여부</returns>
        bool RemoveSyncItem(string name);

        /// <summary>
        /// 전체 경로로 엑셀 파일 위치 반환
        /// </summary>
        /// <param name="item">설정 항목</param>
        /// <returns>전체 엑셀 파일 경로</returns>
        string GetFullExcelPath(SyncSettingItem item);

        /// <summary>
        /// 전체 경로로 ScriptableObject 에셋 반환
        /// </summary>
        /// <param name="item">설정 항목</param>
        /// <returns>전체 에셋 경로</returns>
        string GetFullAssetPath(SyncSettingItem item);
    }
}