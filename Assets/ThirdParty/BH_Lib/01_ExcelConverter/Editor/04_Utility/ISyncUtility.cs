using System;
using UnityEngine;

namespace BH_Lib.ExcelConverter.Utility
{
    /// <summary>
    /// ScriptableObject와 엑셀 파일 간 동기화 기능을 제공하는 인터페이스
    /// </summary>
    public interface ISyncUtility
    {
        /// <summary>
        /// 동기화 처리 중인지 확인하는 프로퍼티
        /// </summary>
        bool IsSyncProcessing { get; }

        /// <summary>
        /// 특정 동기화 항목의 ScriptableObject에서 엑셀 파일로 데이터를 내보내는 메소드
        /// </summary>
        /// <typeparam name="T">ScriptableObject 타입</typeparam>
        /// <param name="syncItem">동기화 설정 항목</param>
        /// <param name="scriptableObject">ScriptableObject 인스턴스</param>
        /// <returns>성공 여부</returns>
        bool ExportSOToExcel<T>(Settings.SyncSettingItem syncItem, T scriptableObject) where T : ScriptableObject;

        /// <summary>
        /// 특정 동기화 항목의 엑셀 파일에서 ScriptableObject로 데이터를 가져오는 메소드
        /// </summary>
        /// <typeparam name="T">ScriptableObject 타입</typeparam>
        /// <param name="syncItem">동기화 설정 항목</param>
        /// <param name="scriptableObject">ScriptableObject 인스턴스</param>
        /// <returns>성공 여부</returns>
        bool ImportExcelToSO<T>(Settings.SyncSettingItem syncItem, T scriptableObject) where T : ScriptableObject;

        /// <summary>
        /// 동기화 항목 등록된 모든 ScriptableObject를 엑셀로 내보내기
        /// </summary>
        void ExportAllSOToExcel();

        /// <summary>
        /// 동기화 항목 등록된 모든 엑셀을 ScriptableObject로 가져오기
        /// </summary>
        void ImportAllExcelToSO();

        /// <summary>
        /// 특정 이름의 동기화 항목 찾아서 ScriptableObject를 엑셀로 동기화
        /// </summary>
        /// <param name="syncItemName">동기화 항목 이름</param>
        /// <returns>성공 여부</returns>
        bool ExportSOToExcel(string syncItemName);

        /// <summary>
        /// 특정 이름의 동기화 항목 찾아서 엑셀을 ScriptableObject로 동기화
        /// </summary>
        /// <param name="syncItemName">동기화 항목 이름</param>
        /// <returns>성공 여부</returns>
        bool ImportExcelToSO(string syncItemName);

        /// <summary>
        /// ScriptableObject 직접 전달하여 엑셀로 내보내기
        /// </summary>
        /// <param name="so">ScriptableObject 인스턴스</param>
        /// <returns>성공 여부</returns>
        bool ExportSOToExcel(ScriptableObject so);

        /// <summary>
        /// ScriptableObject 직접 전달하여 엑셀에서 가져오기
        /// </summary>
        /// <param name="so">ScriptableObject 인스턴스</param>
        /// <returns>성공 여부</returns>
        bool ImportExcelToSO(ScriptableObject so);
    }
}