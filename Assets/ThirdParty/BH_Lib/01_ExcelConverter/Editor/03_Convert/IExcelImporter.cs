using System;
using UnityEngine;

namespace BH_Lib.ExcelConverter.Convert
{
    /// <summary>
    /// 엑셀 파일에서 ScriptableObject로 데이터를 가져오는 기능을 제공하는 인터페이스
    /// </summary>
    /// <typeparam name="T">ScriptableObject 타입</typeparam>
    public interface IExcelImporter<T> where T : ScriptableObject
    {
        /// <summary>
        /// 엑셀 파일에서 데이터를 가져와 ScriptableObject에 적용
        /// </summary>
        /// <returns>성공 여부</returns>
        bool Import();
    }
}
