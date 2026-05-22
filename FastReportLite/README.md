# FastReportLite - 轻量级WPF报表设计工具

## 项目概述

FastReportLite 是一个类似 FastReport.Net 的轻量级WPF报表设计工具，提供了可视化的报表设计功能。

## 功能特性

### 报表设计
- **可视化设计器**: 拖放式报表设计
- **多种组件**: 文本框、图片、表格、图表、线条、条形码
- **波段系统**: 报表标题、页眉、数据区、页脚等
- **属性编辑**: 实时编辑组件属性

### 数据源支持
- SQLite 数据库
- JSON 数据文件
- CSV 数据文件
- 内存数据

### 导出功能
- PDF 格式导出
- Excel (.xlsx) 格式导出
- HTML 网页导出

### 其他功能
- 报表文件保存和加载 (.frl格式)
- 报表预览
- 缩放控制
- 组件拖拽定位

## 系统要求

- Windows 10/11 或 Windows Server 2016+
- .NET 8.0 Runtime

## 项目结构

```
FastReportLite/
├── FastReportLite.sln
├── src/
│   └── FastReportLite/
│       ├── Models/          # 数据模型
│       ├── ViewModels/      # 视图模型
│       ├── Views/           # 视图
│       ├── Controls/        # 自定义控件
│       ├── Services/        # 服务层
│       ├── Converters/      # 值转换器
│       └── Resources/       # 资源文件
└── SPEC.md                 # 项目规范
```

## 技术栈

- .NET 8.0
- WPF (Windows Presentation Foundation)
- MVVM 架构模式
- CommunityToolkit.Mvvm
- PdfSharp (PDF生成)
- DocumentFormat.OpenXml (Excel生成)
- Microsoft.Data.Sqlite (SQLite支持)
- Newtonsoft.Json (JSON处理)

## 快速开始

### 构建项目

```bash
# 恢复依赖
dotnet restore FastReportLite/FastReportLite.sln

# 构建项目
dotnet build FastReportLite/FastReportLite.sln --configuration Release
```

### 运行项目

构建完成后，可执行文件位于:
```
FastReportLite/src/FastReportLite/bin/Release/net8.0-windows/FastReportLite.exe
```

## 使用指南

### 1. 创建新报表
- 点击 "文件" → "新建报表" 或使用快捷键 Ctrl+N

### 2. 添加组件
- 从左侧工具箱拖动组件到报表设计区域
- 支持的组件：文本框、图片、表格、图表、线条、条形码

### 3. 编辑属性
- 选中组件后，在右侧属性面板中编辑属性
- 可编辑：名称、位置、大小、字体、颜色等

### 4. 添加数据源
- 点击 "数据" → "添加数据源"
- 配置数据源连接信息

### 5. 数据绑定
- 在文本框的"数据绑定"属性中输入字段名
- 格式：`{字段名}`，例如：`{CustomerName}`

### 6. 预览报表
- 按 F5 或点击预览按钮查看报表效果

### 7. 导出报表
- 点击 "文件" → "导出" 选择导出格式

## 报表文件格式

报表使用 .frl (FastReportLite) 格式存储，基于JSON格式：

```json
{
  "Name": "报表名称",
  "Description": "报表描述",
  "PageSettings": { ... },
  "Bands": [
    {
      "BandType": "Data",
      "Components": [ ... ]
    }
  ],
  "DataSources": [ ... ]
}
```

## 开发说明

### 主要类说明

- **Report**: 报表主类，包含波段和数据源
- **Band**: 波段类，代表报表的一个区域
- **ReportComponent**: 报表组件基类
- **ReportSerializer**: 报表序列化服务
- **DataService**: 数据服务，处理数据源连接和查询
- **ExportService**: 导出服务，负责导出各种格式

### 扩展开发

如需扩展新组件，只需：

1. 在 `Models/ReportComponent.cs` 中添加新的组件类型枚举值
2. 在 `MainWindow.xaml` 中添加工具箱按钮和组件渲染模板
3. 如需特殊渲染，在相应的服务中添加处理逻辑

## 许可证

本项目仅供学习和参考使用。

## 版本历史

- v1.0.0 - 初始版本
  - 可视化报表设计
  - 多组件支持
  - 数据源连接
  - PDF/Excel/HTML导出
