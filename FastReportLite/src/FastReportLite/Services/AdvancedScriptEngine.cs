#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using FastReportLite.Models;

namespace FastReportLite.Services
{
    public class AdvancedScriptEngine
    {
        private ScriptOptions _scriptOptions;
        private Dictionary<string, Func<object[], object>> _customFunctions;
        private Dictionary<string, object> _globalVariables;

        public AdvancedScriptEngine()
        {
            _customFunctions = new Dictionary<string, Func<object[], object>>();
            _globalVariables = new Dictionary<string, object>();
            InitializeScriptOptions();
            RegisterDefaultFunctions();
        }

        private void InitializeScriptOptions()
        {
            var references = new[]
            {
                Assembly.Load("System.Runtime"),
                Assembly.Load("System.Private.CoreLib"),
                Assembly.Load("System.Core"),
                Assembly.Load("System.Data"),
                Assembly.Load("System.Collections"),
                Assembly.Load("System.Linq"),
                Assembly.GetExecutingAssembly(),
                typeof(Report).Assembly,
                typeof(Dictionary<string, object>).Assembly
            };

            _scriptOptions = ScriptOptions.Default
                .WithReferences(references)
                .WithImports(
                    "System",
                    "System.Linq",
                    "System.Collections.Generic",
                    "FastReportLite.Models",
                    "FastReportLite.Scripting"
                );
        }

        private void RegisterDefaultFunctions()
        {
            _customFunctions["IIf"] = args =>
            {
                if (args.Length < 3) return null;
                bool condition = Convert.ToBoolean(args[0]);
                return condition ? args[1] : args[2];
            };

            _customFunctions["FormatNumber"] = args =>
            {
                if (args.Length < 2) return args[0]?.ToString() ?? "";
                try
                {
                    return Convert.ToDecimal(args[0]).ToString(args[1]?.ToString());
                }
                catch
                {
                    return args[0]?.ToString() ?? "";
                }
            };

            _customFunctions["FormatDate"] = args =>
            {
                if (args.Length < 2) return args[0]?.ToString() ?? "";
                try
                {
                    return Convert.ToDateTime(args[0]).ToString(args[1]?.ToString());
                }
                catch
                {
                    return args[0]?.ToString() ?? "";
                }
            };
        }

        public void RegisterFunction(string name, Func<object[], object> function)
        {
            _customFunctions[name] = function;
        }

        public void SetGlobalVariable(string name, object value)
        {
            _globalVariables[name] = value;
        }

        public object GetGlobalVariable(string name)
        {
            return _globalVariables.TryGetValue(name, out var value) ? value : null;
        }

        public async Task<object> ExecuteScriptAsync(string code, ReportScriptGlobals globals = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code))
                    return null;

                var scriptGlobals = globals ?? new ReportScriptGlobals
                {
                    Report = null,
                    Data = new Dictionary<string, object>(),
                    Functions = _customFunctions,
                    Globals = _globalVariables
                };

                var result = await CSharpScript.EvaluateAsync(code, _scriptOptions, scriptGlobals);
                return result;
            }
            catch (Exception ex)
            {
                return $"Script Error: {ex.Message}";
            }
        }

        public object ExecuteScript(string code, ReportScriptGlobals globals = null)
        {
            return ExecuteScriptAsync(code, globals).GetAwaiter().GetResult();
        }

        public async Task<T> ExecuteScriptAsync<T>(string code, ReportScriptGlobals globals = null)
        {
            try
            {
                var result = await ExecuteScriptAsync(code, globals);
                if (result is T typedResult)
                    return typedResult;
                
                return (T)Convert.ChangeType(result, typeof(T));
            }
            catch
            {
                return default;
            }
        }

        public T ExecuteScript<T>(string code, ReportScriptGlobals globals = null)
        {
            return ExecuteScriptAsync<T>(code, globals).GetAwaiter().GetResult();
        }

        public bool ValidateScript(string code, out string errorMessage)
        {
            try
            {
                var testGlobals = new ReportScriptGlobals
                {
                    Report = null,
                    Data = new Dictionary<string, object>(),
                    Functions = _customFunctions,
                    Globals = _globalVariables
                };

                CSharpScript.Create(code, _scriptOptions, typeof(ReportScriptGlobals));
                errorMessage = string.Empty;
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }
    }

    public class ReportScriptGlobals
    {
        public Report Report { get; set; }
        public Dictionary<string, object> Data { get; set; }
        public Dictionary<string, Func<object[], object>> Functions { get; set; }
        public Dictionary<string, object> Globals { get; set; }
        
        public object this[string key]
        {
            get
            {
                if (Data != null && Data.TryGetValue(key, out var value))
                    return value;
                return null;
            }
        }

        public object Call(string functionName, params object[] args)
        {
            if (Functions != null && Functions.TryGetValue(functionName, out var func))
                return func(args);
            return null;
        }

        public void Log(string message)
        {
            System.Diagnostics.Debug.WriteLine($"[Script Log] {message}");
        }
    }
}
