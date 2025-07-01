using Naninovel;
using System.Reflection;

public static class CustomVariableManagerExtensions
{
    private static FieldInfo variablesField;

    /// <summary>
    /// 커스텀 변수가 존재하는지 확인합니다.
    /// </summary>
    public static bool VariableExists(this ICustomVariableManager variableManager, string name)
    {
        if (variableManager == null || string.IsNullOrEmpty(name))
            return false;

        try
        {
            // Reflection을 사용하여 내부 variables 딕셔너리에 접근
            if (variablesField == null)
            {
                var type = variableManager.GetType();
                variablesField = type.GetField("variables", BindingFlags.NonPublic | BindingFlags.Instance);
            }

            if (variablesField != null)
            {
                var variables = variablesField.GetValue(variableManager) as System.Collections.Generic.Dictionary<string, CustomVariableValue>;
                return variables != null && variables.ContainsKey(name);
            }

            // Reflection이 실패하면 try-catch로 확인
            variableManager.GetVariableValue(name);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 변수 값을 안전하게 가져옵니다. 변수가 없으면 기본값을 반환합니다.
    /// </summary>
    public static string GetVariableValueSafe(this ICustomVariableManager variableManager, string name, string defaultValue = "")
    {
        if (!variableManager.VariableExists(name))
            return defaultValue;

        try
        {
            return variableManager.GetVariableValue(name).ToString();
        }
        catch
        {
            return defaultValue;
        }
    }
}