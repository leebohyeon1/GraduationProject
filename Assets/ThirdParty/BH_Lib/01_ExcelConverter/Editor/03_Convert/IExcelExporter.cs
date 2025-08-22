using System;
using UnityEngine;

namespace BH_Lib.ExcelConverter.Convert
{
    /// <summary>
    /// ScriptableObject에서 엑셀 파일로 데이터를 내보내는 기능을 제공하는 인터페이스
    /// </summary>
    /// <typeparam name="T">ScriptableObject 타입</typeparam>
    public interface IExcelExporter<T> where T : ScriptableObject
    {
        /// <summary>
        /// ScriptableObject의 데이터를 엑셀 파일로 내보내기
        /// </summary>
        /// <returns>성공 여부</returns>
        bool Export();
    }
}