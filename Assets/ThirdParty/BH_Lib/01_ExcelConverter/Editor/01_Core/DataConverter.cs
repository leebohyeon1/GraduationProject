using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BH_Lib.DI;

namespace BH_Lib.ExcelConverter.Core
{
    /// <summary>
    /// 다양한 데이터 타입 간의 변환 기능을 제공하는 유틸리티 클래스
    /// </summary>
    [Register(typeof(IDataConverter))]
    public class DataConverter : IDataConverter
    {
        /// <summary>
        /// 문자열 값을 지정된 타입으로 변환
        /// </summary>
        /// <param name="value">변환할 문자열</param>
        /// <param name="targetType">변환 대상 타입</param>
        /// <returns>변환된 값</returns>
        public object ConvertToType(string value, Type targetType)
        {
            if (string.IsNullOrEmpty(value))
            {
                return GetDefaultValue(targetType);
            }

            try
            {
                // 배열이나 리스트 타입인 경우
                if (targetType.IsArray || (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(List<>)))
                {
                    return ConvertToArrayOrList(value, targetType);
                }

                // 각 타입별 변환 메서드 호출
                if (targetType == typeof(int)) return ConvertToInt(value);
                if (targetType == typeof(float)) return ConvertToFloat(value);
                if (targetType == typeof(double)) return ConvertToDouble(value);
                if (targetType == typeof(bool)) return ConvertToBool(value);
                if (targetType == typeof(string)) return value;
                if (targetType == typeof(Vector2)) return ConvertToVector2(value);
                if (targetType == typeof(Vector3)) return ConvertToVector3(value);
                if (targetType == typeof(Color)) return ConvertToColor(value);
                if (targetType == typeof(DateTime)) return ConvertToDateTime(value);
                if (targetType.IsEnum) return ConvertToEnum(value, targetType);

                // 지원하지 않는 타입의 경우 기본값 반환
                UnityEngine.Debug.LogWarning($"지원하지 않는 타입 변환 시도: {targetType.Name}");
                return GetDefaultValue(targetType);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"값 '{value}'를 {targetType.Name} 타입으로 변환 중 오류: {ex.Message}");
                return GetDefaultValue(targetType);
            }
        }

        /// <summary>
        /// 문자열을 배열 또는 리스트로 변환
        /// </summary>
        /// <param name="value">문자열 값</param>
        /// <param name="targetType">배열 또는 리스트 타입</param>
        /// <returns>변환된 배열 또는 리스트</returns>
        public object ConvertToArrayOrList(string value, Type targetType)
        {
            // 공백이나 빈 문자열인 경우 빈 배열/리스트 반환
            if (string.IsNullOrWhiteSpace(value))
            {
                if (targetType.IsArray)
                {
                    // 배열인 경우 빈 배열 생성
                    Type elementType = targetType.GetElementType();
                    return Array.CreateInstance(elementType, 0);
                }
                else if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    // 리스트인 경우 빈 리스트 생성
                    return Activator.CreateInstance(targetType);
                }
            }

            try
            {
                // {1,2,3} 형태에서 괄호 및 공백 제거, 및 다른 형태 처리
                value = value.Trim();
                if (value.StartsWith("{") && value.EndsWith("}"))
                {
                    value = value.Substring(1, value.Length - 2);
                }

                // 콤마로 구분하여 값 분리
                string[] elements = value.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

                if (targetType.IsArray)
                {
                    // 배열 타입 처리
                    Type elementType = targetType.GetElementType();
                    Array array = Array.CreateInstance(elementType, elements.Length);

                    for (int i = 0; i < elements.Length; i++)
                    {
                        object convertedValue = ConvertToType(elements[i].Trim(), elementType);
                        array.SetValue(convertedValue, i);
                    }

                    return array;
                }
                else if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    // 리스트 타입 처리
                    Type elementType = targetType.GetGenericArguments()[0];
                    var list = Activator.CreateInstance(targetType) as IList;

                    foreach (string element in elements)
                    {
                        object convertedValue = ConvertToType(element.Trim(), elementType);
                        list.Add(convertedValue);
                    }

                    return list;
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"배열/리스트 변환 오류: {ex.Message}");
            }

            // 오류 발생 시 기본값 반환
            return GetDefaultValue(targetType);
        }

        /// <summary>
        /// 문자열을 int 타입으로 변환
        /// </summary>
        /// <param name="value">변환할 문자열</param>
        /// <returns>변환된 int 값</returns>
        public int ConvertToInt(string value)
        {
            if (int.TryParse(value, out int result))
            {
                return result;
            }

            // 소수점이 있는 경우 반올림
            if (float.TryParse(value, out float floatValue))
            {
                return Mathf.RoundToInt(floatValue);
            }

            return 0;
        }

        /// <summary>
        /// 문자열을 float 타입으로 변환
        /// </summary>
        /// <param name="value">변환할 문자열</param>
        /// <returns>변환된 float 값</returns>
        public float ConvertToFloat(string value)
        {
            if (float.TryParse(value, out float result))
            {
                return result;
            }
            return 0f;
        }

        /// <summary>
        /// 문자열을 double 타입으로 변환
        /// </summary>
        /// <param name="value">변환할 문자열</param>
        /// <returns>변환된 double 값</returns>
        public double ConvertToDouble(string value)
        {
            if (double.TryParse(value, out double result))
            {
                return result;
            }
            return 0d;
        }

        /// <summary>
        /// 문자열을 bool 타입으로 변환
        /// </summary>
        /// <param name="value">변환할 문자열</param>
        /// <returns>변환된 bool 값</returns>
        public bool ConvertToBool(string value)
        {
            // 직접적인 bool 문자열 처리
            if (bool.TryParse(value, out bool result))
            {
                return result;
            }

            // 숫자 1/0 처리
            if (int.TryParse(value, out int intValue))
            {
                return intValue != 0;
            }

            // Y/N, 예/아니오 등의 문자열 처리
            value = value.ToLower().Trim();
            return value == "y" || value == "yes" || value == "true" || value == "예" || value == "t" || value == "1";
        }

        /// <summary>
        /// 문자열을 Vector2 타입으로 변환
        /// </summary>
        /// <param name="value">x,y 형식의 문자열</param>
        /// <returns>변환된 Vector2 값</returns>
        public Vector2 ConvertToVector2(string value)
        {
            try
            {
                // 괄호 및 공백 제거
                value = value.Trim('(', ')', ' ', '[', ']', '{', '}');

                // 쉼표로 분리
                string[] components = value.Split(',', StringSplitOptions.RemoveEmptyEntries);

                if (components.Length >= 2)
                {
                    float x = ConvertToFloat(components[0].Trim());
                    float y = ConvertToFloat(components[1].Trim());
                    return new Vector2(x, y);
                }
            }
            catch
            {
                // 변환 실패 시 로그 없이 기본값 반환 (이미 상위 메서드에서 로깅)
            }

            return Vector2.zero;
        }

        /// <summary>
        /// 문자열을 Vector3 타입으로 변환
        /// </summary>
        /// <param name="value">x,y,z 형식의 문자열</param>
        /// <returns>변환된 Vector3 값</returns>
        public Vector3 ConvertToVector3(string value)
        {
            try
            {
                // 괄호 및 공백 제거
                value = value.Trim('(', ')', ' ', '[', ']', '{', '}');

                // 쉼표로 분리
                string[] components = value.Split(',', StringSplitOptions.RemoveEmptyEntries);

                if (components.Length >= 3)
                {
                    float x = ConvertToFloat(components[0].Trim());
                    float y = ConvertToFloat(components[1].Trim());
                    float z = ConvertToFloat(components[2].Trim());
                    return new Vector3(x, y, z);
                }
                else if (components.Length == 2)
                {
                    // Vector2처럼 입력되었을 경우 z=0으로 처리
                    float x = ConvertToFloat(components[0].Trim());
                    float y = ConvertToFloat(components[1].Trim());
                    return new Vector3(x, y, 0);
                }
            }
            catch
            {
                // 변환 실패 시 로그 없이 기본값 반환 (이미 상위 메서드에서 로깅)
            }

            return Vector3.zero;
        }

        /// <summary>
        /// 문자열을 Color 타입으로 변환
        /// </summary>
        /// <param name="value">r,g,b,a 또는 #RRGGBB 형식의 문자열</param>
        /// <returns>변환된 Color 값</returns>
        public Color ConvertToColor(string value)
        {
            try
            {
                // #RRGGBB 또는 #RRGGBBAA 형식인 경우
                if (value.StartsWith("#"))
                {
                    return ColorUtility.TryParseHtmlString(value, out Color color) ? color : Color.white;
                }

                // 괄호 및 공백 제거
                value = value.Trim('(', ')', ' ', '[', ']', '{', '}');

                // 쉼표로 분리
                string[] components = value.Split(',', StringSplitOptions.RemoveEmptyEntries);

                if (components.Length >= 3)
                {
                    float r = ConvertToFloat(components[0].Trim()) / 255f;
                    float g = ConvertToFloat(components[1].Trim()) / 255f;
                    float b = ConvertToFloat(components[2].Trim()) / 255f;
                    float a = components.Length >= 4 ? ConvertToFloat(components[3].Trim()) / 255f : 1f;

                    // 0~1 범위 체크 (255 스케일로 입력했을 수 있음)
                    if (r > 1f || g > 1f || b > 1f)
                    {
                        r = Mathf.Clamp01(r / 255f);
                        g = Mathf.Clamp01(g / 255f);
                        b = Mathf.Clamp01(b / 255f);
                        a = Mathf.Clamp01(a);
                    }

                    return new Color(r, g, b, a);
                }
            }
            catch
            {
                // 변환 실패 시 로그 없이 기본값 반환 (이미 상위 메서드에서 로깅)
            }

            return Color.white;
        }

        /// <summary>
        /// 문자열을 DateTime 타입으로 변환
        /// </summary>
        /// <param name="value">날짜 형식 문자열</param>
        /// <returns>변환된 DateTime 값</returns>
        public DateTime ConvertToDateTime(string value)
        {
            if (DateTime.TryParse(value, out DateTime result))
            {
                return result;
            }
            return DateTime.Now;
        }

        /// <summary>
        /// 문자열을 Enum 타입으로 변환
        /// </summary>
        /// <param name="value">변환할 문자열</param>
        /// <param name="enumType">Enum 타입</param>
        /// <returns>변환된 Enum 값</returns>
        public object ConvertToEnum(string value, Type enumType)
        {
            try
            {
                return Enum.Parse(enumType, value, true);
            }
            catch
            {
                // 정확한 이름으로 변환되지 않을 경우, 숫자로 시도
                if (int.TryParse(value, out int intValue))
                {
                    return Enum.ToObject(enumType, intValue);
                }

                // 실패 시 첫 번째 값 반환
                return Enum.GetValues(enumType).GetValue(0);
            }
        }

        /// <summary>
        /// 값을 엑셀 셀에 쓸 수 있는 문자열로 변환
        /// </summary>
        /// <param name="value">변환할 값</param>
        /// <returns>변환된 문자열</returns>
        public string ConvertToString(object value)
        {
            if (value == null) return string.Empty;

            Type type = value.GetType();

            // 리스트나 배열 처리
            if (type.IsArray || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)))
            {
                var enumerable = value as System.Collections.IEnumerable;
                if (enumerable != null)
                {
                    var items = new List<string>();
                    foreach (var item in enumerable)
                    {
                        items.Add(item.ToString());
                    }
                    return $"{{({string.Join(",", items)})}}";
                }
            }
            else if (type == typeof(Vector2))
            {
                Vector2 v = (Vector2)value;
                return $"{v.x},{v.y}";
            }
            else if (type == typeof(Vector3))
            {
                Vector3 v = (Vector3)value;
                return $"{v.x},{v.y},{v.z}";
            }
            else if (type == typeof(Color))
            {
                Color c = (Color)value;
                return $"#{ColorUtility.ToHtmlStringRGBA(c)}";
            }
            else if (type == typeof(DateTime))
            {
                return ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss");
            }

            return value.ToString();
        }

        /// <summary>
        /// 지정된 타입의 기본값 반환
        /// </summary>
        /// <param name="type">값 타입</param>
        /// <returns>기본값</returns>
        private object GetDefaultValue(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }
    }
}