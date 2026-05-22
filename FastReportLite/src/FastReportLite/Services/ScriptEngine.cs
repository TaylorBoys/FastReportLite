#nullable disable

using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;

namespace FastReportLite.Services
{
    public class ScriptEngine
    {
        private Dictionary<string, object> _variables;

        public ScriptEngine()
        {
            _variables = new Dictionary<string, object>();
        }

        public void SetVariable(string name, object value)
        {
            _variables[name] = value;
        }

        public object GetVariable(string name)
        {
            return _variables.TryGetValue(name, out var value) ? value : null;
        }

        public void ClearVariables()
        {
            _variables.Clear();
        }

        public void LoadFromDataRow(Dictionary<string, object> dataRow)
        {
            foreach (var kvp in dataRow)
            {
                _variables[kvp.Key] = kvp.Value;
            }
        }

        public object Evaluate(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return string.Empty;

            try
            {
                // 处理内置函数
                string processedExpr = ProcessFunctions(expression);
                
                // 替换变量
                processedExpr = ReplaceVariables(processedExpr);
                
                // 简单的条件表达式处理
                if (processedExpr.Contains("?"))
                {
                    return EvaluateConditional(processedExpr);
                }
                
                // 使用 DataTable.Compute 进行数学计算
                return ComputeExpression(processedExpr);
            }
            catch (Exception ex)
            {
                return $"[Error: {ex.Message}]";
            }
        }

        public T Evaluate<T>(string expression, T defaultValue = default)
        {
            try
            {
                var result = Evaluate(expression);
                if (result == null)
                    return defaultValue;

                return (T)Convert.ChangeType(result, typeof(T), System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                return defaultValue;
            }
        }

        private string ProcessFunctions(string expr)
        {
            string result = expr;
            
            // 处理 IF/IIF 函数: IF(condition, trueVal, falseVal)
            result = Regex.Replace(result, @"II?F\s*\(([^,]+),([^,]+),([^)]+)\)", match =>
            {
                string condition = match.Groups[1].Value.Trim();
                string trueVal = match.Groups[2].Value.Trim();
                string falseVal = match.Groups[3].Value.Trim();
                return $"{condition} ? {trueVal} : {falseVal}";
            }, RegexOptions.IgnoreCase);

            // 处理简单的数学函数
            result = Regex.Replace(result, @"ABS\s*\(([^)]+)\)", match =>
            {
                return $"Math.Abs({match.Groups[1].Value})";
            }, RegexOptions.IgnoreCase);

            // 处理字符串函数
            result = Regex.Replace(result, @"LEN\s*\(([^)]+)\)", match =>
            {
                var arg = match.Groups[1].Value.Trim();
                if (arg.StartsWith("'") || arg.StartsWith("\""))
                {
                    return $"{arg.Substring(1, arg.Length - 2).Length}";
                }
                return $"({arg}).Length";
            }, RegexOptions.IgnoreCase);

            // 处理字符串替换
            result = Regex.Replace(result, @"REPLACE\s*\(([^,]+),([^,]+),([^)]+)\)", match =>
            {
                string input = match.Groups[1].Value.Trim();
                string oldStr = match.Groups[2].Value.Trim();
                string newStr = match.Groups[3].Value.Trim();
                return $"{input}.Replace({oldStr}, {newStr})";
            }, RegexOptions.IgnoreCase);

            // 处理上下限
            result = Regex.Replace(result, @"ROUND\s*\(([^)]+)\)", match =>
            {
                return $"Math.Round({match.Groups[1].Value})";
            }, RegexOptions.IgnoreCase);

            return result;
        }

        private string ReplaceVariables(string expr)
        {
            string result = expr;
            
            foreach (var kvp in _variables)
            {
                string pattern = $@"\b{Regex.Escape(kvp.Key)}\b";
                string replacement = FormatValue(kvp.Value);
                result = Regex.Replace(result, pattern, replacement);
            }

            return result;
        }

        private string FormatValue(object value)
        {
            if (value == null)
                return "null";

            if (value is string str)
                return $"'{str.Replace("'", "''")}'";

            if (value is DateTime dt)
                return $"'{dt.ToString("yyyy-MM-dd HH:mm:ss")}'";

            if (value is bool b)
                return b ? "true" : "false";

            return value.ToString() ?? "null";
        }

        private object EvaluateConditional(string expr)
        {
            // 简单的三元表达式处理
            var match = Regex.Match(expr, @"^(.*?)\s*\?\s*(.*?)\s*:\s*(.*)$");
            if (match.Success)
            {
                string condition = match.Groups[1].Value.Trim();
                string trueVal = match.Groups[2].Value.Trim();
                string falseVal = match.Groups[3].Value.Trim();

                bool result = (bool)ComputeExpression(condition);
                return result ? trueVal : falseVal;
            }

            return expr;
        }

        private object ComputeExpression(string expr)
        {
            try
            {
                // 移除可能导致问题的部分
                string cleanExpr = expr.Replace("Math.", "");
                
                using var dt = new DataTable();
                return dt.Compute(cleanExpr, "");
            }
            catch
            {
                // 如果 DataTable.Compute 失败，尝试简单的字符串表达式
                return expr;
            }
        }
    }
}
