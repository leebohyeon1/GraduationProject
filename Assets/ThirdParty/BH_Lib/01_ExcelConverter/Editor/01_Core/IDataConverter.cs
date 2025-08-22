using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BH_Lib.ExcelConverter.Core
{
    /// <summary>
    /// 다양한 데이터 타입 간의 변환 기능을 제공하는 인터페이스
    /// </summary>
    public interface IDataConverter
    {
        /// <summary>
        /// 문자열 값을 지정된 타입으로 변환
        /// </summary>
        /// <param name="value">변환할 문자열</param>
        /// <param name="targetType">변환 대상 타입</param>
        /// <returns>변환된 값</returns>
        object ConvertToType(string value, Type targetType);

        /// <summary>
        /// 값을 엑셀 셀에 쓸 수 있는 문자열로 변환
        /// </summary>
        /// <param name="value">변환할 값</param>
        /// <returns>변환된 문자열</returns>
        string ConvertToString(object value);

        /// <summary>
        /// 문자열을 배열 또는 리스트로 변환
        /// </summary>
        /// <param name="value">문자열 값</param>
        /// <param name="targetType">배열 또는 리스트 타입</param>
        /// <returns>변환된 배열 또는 리스트</returns>
        object ConvertToArrayOrList(string value, Type targetType);

        /// <summary>
        /// 문자열을 int 타입으로 변환
        /// </summary>
        /// <param name="value">변환할 문자열</param>
        /// <returns>변환된 int 값</returns>
        int ConvertToInt(string value);

        /// <summary>
        /// 문자열을 float 타입으로 변환
        /// </summary>
        /// <param name="value">변환할 문자열</param>
        /// <returns>변환된 float 값</returns>
        float ConvertToFloat(string value);

        /// <summary>
        /// 문자열을 Vector2 타입으로 변환
        /// </summary>
        /// <param name="value">x,y 형식의 문자열</param>
        /// <returns>변환된 Vector2 값</returns>
        Vector2 ConvertToVector2(string value);

        /// <summary>
        /// 문자열을 Vector3 타입으로 변환
        /// </summary>
        /// <param name="value">x,y,z 형식의 문자열</param>
        /// <returns>변환된 Vector3 값</returns>
        Vector3 ConvertToVector3(string value);

        /// <summary>
        /// 문자열을 Color 타입으로 변환
        /// </summary>
        /// <param name="value">r,g,b,a 또는 #RRGGBB 형식의 문자열</param>
        /// <returns>변환된 Color 값</returns>
        Color ConvertToColor(string value);
    }
}
