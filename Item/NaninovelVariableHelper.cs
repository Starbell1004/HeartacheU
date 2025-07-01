using Naninovel;
using UnityEngine;

/// <summary>
/// 나니노벨 변수 처리를 위한 헬퍼 클래스
/// </summary>
public static class NaninovelVariableHelper
{
    /// <summary>
    /// 나니노벨 변수 값을 string으로 안전하게 가져옵니다
    /// </summary>
    /// <param name="variableName">변수명</param>
    /// <param name="defaultValue">기본값 (변수가 없거나 오류 시 반환)</param>
    /// <returns>변수 값 (string)</returns>
    public static string GetVariableAsString(string variableName, string defaultValue = "")
    {
        if (string.IsNullOrEmpty(variableName))
            return defaultValue;

        try
        {
            var variableManager = Engine.GetService<ICustomVariableManager>();
            if (variableManager == null)
            {
                Debug.LogWarning($"[NaninovelVariableHelper] ICustomVariableManager를 찾을 수 없습니다.");
                return defaultValue;
            }

            var variableValue = variableManager.GetVariableValue(variableName);

            // CustomVariableValue를 직접 ToString()으로 변환
            string stringValue = variableValue.ToString();

            // null이나 빈 문자열인 경우 기본값 반환
            if (string.IsNullOrEmpty(stringValue))
            {
                return defaultValue;
            }

            return stringValue;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[NaninovelVariableHelper] 변수 '{variableName}' 가져오기 실패: {ex.Message}");
            return defaultValue;
        }
    }

    /// <summary>
    /// 나니노벨 변수 값을 int로 안전하게 가져옵니다
    /// </summary>
    /// <param name="variableName">변수명</param>
    /// <param name="defaultValue">기본값</param>
    /// <returns>변수 값 (int)</returns>
    public static int GetVariableAsInt(string variableName, int defaultValue = 0)
    {
        string stringValue = GetVariableAsString(variableName, defaultValue.ToString());

        if (int.TryParse(stringValue, out int result))
        {
            return result;
        }

        return defaultValue;
    }

    /// <summary>
    /// 나니노벨 변수 값을 bool로 안전하게 가져옵니다
    /// </summary>
    /// <param name="variableName">변수명</param>
    /// <param name="defaultValue">기본값</param>
    /// <returns>변수 값 (bool)</returns>
    public static bool GetVariableAsBool(string variableName, bool defaultValue = false)
    {
        string stringValue = GetVariableAsString(variableName, defaultValue.ToString());

        // "1", "true", "True", "TRUE" 등을 true로 처리
        if (stringValue == "1" || string.Equals(stringValue, "true", System.StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // "0", "false", "False", "FALSE" 등을 false로 처리
        if (stringValue == "0" || string.Equals(stringValue, "false", System.StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // 파싱할 수 없으면 기본값 반환
        return defaultValue;
    }

    /// <summary>
    /// 나니노벨 변수에 string 값을 안전하게 설정합니다
    /// </summary>
    /// <param name="variableName">변수명</param>
    /// <param name="value">설정할 값</param>
    /// <returns>성공 여부</returns>
    public static bool SetVariable(string variableName, string value)
    {
        if (string.IsNullOrEmpty(variableName))
        {
            Debug.LogWarning("[NaninovelVariableHelper] 변수명이 비어있습니다.");
            return false;
        }

        try
        {
            var variableManager = Engine.GetService<ICustomVariableManager>();
            if (variableManager == null)
            {
                Debug.LogWarning("[NaninovelVariableHelper] ICustomVariableManager를 찾을 수 없습니다.");
                return false;
            }

            variableManager.SetVariableValue(variableName, new CustomVariableValue(value ?? ""));
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[NaninovelVariableHelper] 변수 '{variableName}' 설정 실패: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 나니노벨 변수에 int 값을 안전하게 설정합니다
    /// </summary>
    /// <param name="variableName">변수명</param>
    /// <param name="value">설정할 값</param>
    /// <returns>성공 여부</returns>
    public static bool SetVariable(string variableName, int value)
    {
        return SetVariable(variableName, value.ToString());
    }

    /// <summary>
    /// 나니노벨 변수에 bool 값을 안전하게 설정합니다 (1 또는 0으로 저장)
    /// </summary>
    /// <param name="variableName">변수명</param>
    /// <param name="value">설정할 값</param>
    /// <returns>성공 여부</returns>
    public static bool SetVariable(string variableName, bool value)
    {
        return SetVariable(variableName, value ? "1" : "0");
    }
}