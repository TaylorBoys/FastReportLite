# FastReportLite C# 脚本功能使用说明

## 概述

FastReportLite 现在支持在报表组件中使用完整的 C# 脚本语言，可以对数据源和报表控件进行强大的编程控制。

## 功能特性

### 1. 脚本编辑器

在属性面板中，脚本部分现在包含以下事件脚本编辑器：

- **表达式脚本** - 用于简单的计算表达式
- **OnBeforePrint** - 在组件打印前执行的代码
- **OnGetValue** - 计算组件显示值的代码
- **OnAfterPrint** - 在组件打印后执行的代码
- **OnFormatting** - 用于条件格式化的代码（返回颜色值）

### 2. 脚本全局对象

在脚本中可以访问以下全局对象：

#### Report
访问当前报表对象

#### Data
访问当前数据行的字典，可以通过 `Data["字段名"]` 或 `["字段名"]` 来获取数据

#### Functions
访问内置函数库，可以使用 `Call("函数名", 参数)` 来调用

#### Globals
访问全局变量集合

### 3. 内置函数

- `IIf(condition, trueValue, falseValue)` - 条件函数
- `FormatNumber(value, format)` - 格式化数字
- `FormatDate(value, format)` - 格式化日期

### 4. 事件说明

#### OnBeforePrint
在组件打印前执行，可用于修改组件属性或执行准备工作。

示例：
```csharp
// 修改组件文本
Report.Bands[0].Components[0].Text = "新的文本";

// 检查数据条件
if (Data["Amount"] is decimal amount && amount > 1000)
{
    // 做一些处理
}
```

#### OnGetValue
用于动态计算组件的显示值，返回的结果将作为组件的最终显示值。

示例：
```csharp
// 计算总金额
decimal price = Convert.ToDecimal(Data["Price"]);
int quantity = Convert.ToInt32(Data["Quantity"]);
return (price * quantity).ToString("C2");
```

#### OnAfterPrint
在组件打印后执行，可用于清理工作或记录信息。

示例：
```csharp
// 记录日志
Log($"已打印组件: {Report.Bands[0].Components[0].Name}");
```

#### OnFormatting
用于条件格式化，返回颜色值（如 "#FF0000" 代表红色）。

示例：
```csharp
// 根据值设置颜色
decimal value = Convert.ToDecimal(Data["Value"]);
if (value > 1000)
    return "#FF0000"; // 红色
else if (value > 500)
    return "#FFFF00"; // 黄色
else
    return "#000000"; // 黑色
```

## 完整的使用示例

### 示例 1: 简单计算

在 OnGetValue 中：
```csharp
decimal quantity = Convert.ToDecimal(Data["Quantity"]);
decimal unitPrice = Convert.ToDecimal(Data["UnitPrice"]);
return (quantity * unitPrice).ToString("C2");
```

### 示例 2: 文本格式化

在 OnGetValue 中：
```csharp
string firstName = Data["FirstName"]?.ToString() ?? "";
string lastName = Data["LastName"]?.ToString() ?? "";
return $"{lastName}, {firstName}";
```

### 示例 3: 条件格式化

在 OnFormatting 中：
```csharp
decimal value = Convert.ToDecimal(Data["Amount"]);
if (value < 0)
    return "#FF0000"; // 红色表示负数
else
    return "#000000"; // 黑色表示正数
```

### 示例 4: 组合多种操作

在 OnBeforePrint 中：
```csharp
// 检查数据类型
if (Data["ProductType"]?.ToString() == "Electronics")
{
    // 对电子产品应用特殊处理
    Log("处理电子产品");
}
```

## 更新的文件列表

以下是支持此新功能的文件：

| 文件 | 描述 |
|------|------|
| `FastReportLite.csproj` | 添加了 Microsoft.CodeAnalysis.CSharp.Scripting 依赖 |
| `Models/ReportComponent.cs` | 添加了事件脚本属性 |
| `Services/ReportSerializer.cs` | 更新了序列化支持 |
| `Services/AdvancedScriptEngine.cs` | 新增！高级 C# 脚本引擎 |
| `Services/ExportService.cs` | 更新为集成高级脚本引擎 |
| `Scripting/ReportScripting.cs` | 新增！脚本对象模型 |
| `MainWindow.xaml` | 更新了属性面板的脚本编辑器 |
| `ScriptEngine.cs` | 保留的简单表达式引擎（原版本） |

## 构建和运行

项目已成功构建，输出位于：
`src/FastReportLite/bin/Release/net8.0-windows/FastReportLite.dll`

## 注意事项

1. 脚本执行是安全的，使用了受限的上下文
2. 脚本有错误时，会返回错误信息而不会导致崩溃
3. 所有数据访问应通过 Data 字典进行
4. 建议先在简单场景测试脚本，确保正确
