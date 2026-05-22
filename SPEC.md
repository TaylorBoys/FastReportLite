# FastReportLite - WPF报表工具规范

## 1. 项目概述

### 项目名称
**FastReportLite** - 轻量级WPF报表设计工具

### 项目类型
桌面应用程序（WPF/.NET 8）

### 核心功能概述
一个类似FastReport.Net的报表设计工具，支持可视化报表设计、数据绑定、报表预览和多格式导出。

### 目标用户
- .NET开发人员
- 报表设计师
- 数据分析师

## 2. UI/UX规范

### 布局结构

#### 主窗口设计
- **窗口类型**: 单主窗口 + 多文档界面(MDI)风格
- **最小尺寸**: 1280 x 720 像素
- **默认尺寸**: 1600 x 900 像素

#### 布局区域
```
┌──────────────────────────────────────────────────────────────┐
│  菜单栏 (Menu Bar)                                           │
├──────────────────────────────────────────────────────────────┤
│  工具栏 (Tool Bar)                                           │
├─────────────┬────────────────────────────┬───────────────────┤
│  工具箱     │                            │  属性面板          │
│  (Toolbox)  │    报表设计画布             │  (Properties)     │
│  200px      │    (Report Designer)       │  250px            │
│             │                            │                   │
├─────────────┴────────────────────────────┴───────────────────┤
│  数据源面板 (Data Sources Panel) - 可折叠                    │
│  高度: 200px                                                  │
├─────────────────────────────────────────────────────────────┤
│  状态栏 (Status Bar)                                         │
└──────────────────────────────────────────────────────────────┘
```

### 视觉设计

#### 配色方案
- **主色调**: #2D5A8A (深蓝色)
- **次要色**: #4A90D9 (亮蓝色)
- **强调色**: #E67E22 (橙色)
- **背景色**: #FFFFFF (白色)
- **设计区域背景**: #F5F5F5 (浅灰色)
- **边框色**: #CCCCCC (灰色)
- **文字色**: #333333 (深灰色)

#### 字体规范
- **标题字体**: Segoe UI, 14px, SemiBold
- **正文字体**: Segoe UI, 12px, Regular
- **代码字体**: Consolas, 11px, Regular

#### 间距系统
- **基准间距**: 8px
- **小间距**: 4px
- **大间距**: 16px
- **面板内边距**: 12px

#### 视觉效果
- **阴影**: 轻微阴影 (0 2px 4px rgba(0,0,0,0.1))
- **边框圆角**: 4px
- **过渡动画**: 150ms ease-in-out

### 组件规范

#### 工具箱组件
| 组件名称 | 图标 | 说明 |
|---------|------|------|
| BandContainer | 📄 | 波段容器 |
| TextBox | 📝 | 文本框 |
| PictureBox | 🖼️ | 图片 |
| Table | 📊 | 表格 |
| Chart | 📈 | 图表 |
| Line | ➖ | 线条 |
| Barcode | 📶 | 条形码 |

#### 报表波段 (Bands)
- **Report Title**: 报表标题
- **Page Header**: 页眉
- **Data Band**: 数据区
- **Group Header**: 分组头
- **Group Footer**: 分组尾
- **Page Footer**: 页脚
- **Report Summary**: 报表汇总

#### 工具栏按钮
- 新建报表
- 打开报表
- 保存报表
- 预览报表
- 导出报表
- 撤销/重做
- 剪切/复制/粘贴
- 对齐工具
- 缩放控制

### 交互规范

#### 拖放操作
1. 从工具箱拖动组件到设计画布
2. 在波段内调整组件位置和大小
3. 显示对齐辅助线和网格

#### 属性编辑
- 选中组件后，属性面板显示所有可编辑属性
- 支持属性分组（位置、大小、数据、外观等）

#### 数据绑定
- 拖放数据字段到组件上进行绑定
- 支持表达式编辑

## 3. 功能规范

### 3.1 报表设计功能

#### 组件系统
- 支持多种报表组件：文本框、图片、表格、图表、线条、条形码
- 组件支持属性设置：位置、尺寸、字体、颜色、边框等
- 支持组件的层级管理

#### 波段系统
- 报表由多个波段组成
- 每个波段可以放置多个组件
- 波段高度可调整
- 数据波段支持数据重复

#### 对齐和分布
- 组件对齐工具（左对齐、居中、右对齐等）
- 间距调整工具
- 网格对齐功能

### 3.2 数据源系统

#### 支持的数据源
- SQL Server数据库
- SQLite数据库
- CSV文件
- JSON数据
- 内存对象

#### 数据绑定
- 拖放字段到组件进行绑定
- 支持表达式绑定 {Orders.CustomerName}
- 支持计算字段
- 支持参数化查询

### 3.3 报表预览

#### 预览功能
- 实时预览报表输出
- 分页显示
- 缩放控制
- 页码导航

#### 数据模拟
- 预览时使用模拟数据
- 支持刷新预览

### 3.4 导出功能

#### 导出格式
- PDF文档
- Excel (.xlsx)
- HTML网页
- 图片 (PNG/JPEG)

#### 导出选项
- 纸张大小选择
- 边距设置
- 质量设置

### 3.5 报表文件格式

#### .frl格式
```xml
<?xml version="1.0" encoding="utf-8"?>
<Report>
  <Bands>
    <Band Type="ReportTitle">
      <Components>
        <TextBox Name="Title" Text="报表标题" ... />
      </Components>
    </Band>
    <!-- 更多波段 -->
  </Bands>
  <DataSources>
    <DataSource Name="Orders" ConnectionString="..." Query="..." />
  </DataSources>
</Report>
```

## 4. 技术架构

### 项目结构
```
FastReportLite/
├── FastReportLite.sln
├── src/
│   └── FastReportLite/
│       ├── FastReportLite.csproj
│       ├── App.xaml
│       ├── App.xaml.cs
│       ├── MainWindow.xaml
│       ├── MainWindow.xaml.cs
│       ├── Models/
│       │   ├── Report.cs
│       │   ├── Band.cs
│       │   ├── ReportComponent.cs
│       │   └── DataSource.cs
│       ├── ViewModels/
│       │   ├── MainViewModel.cs
│       │   ├── DesignerViewModel.cs
│       │   └── PropertiesViewModel.cs
│       ├── Views/
│       │   ├── DesignerView.xaml
│       │   ├── ToolboxView.xaml
│       │   ├── PropertiesView.xaml
│       │   ├── DataSourcesView.xaml
│       │   └── PreviewView.xaml
│       ├── Controls/
│       │   ├── ReportCanvas.cs
│       │   ├── DesignSurface.cs
│       │   └── ReportComponentControl.cs
│       ├── Services/
│       │   ├── ReportSerializer.cs
│       │   ├── DataService.cs
│       │   ├── ExportService.cs
│       │   └── PreviewService.cs
│       ├── Converters/
│       │   └── Various converters
│       └── Resources/
│           └── Styles and templates
└── README.md
```

### 核心技术栈
- .NET 8.0
- WPF (Windows Presentation Foundation)
- MVVM架构模式
- CommunityToolkit.Mvvm
- System.Data.SQLite
- Newtonsoft.Json
- DocumentFormat.OpenXml (Excel导出)
- PdfSharp (PDF导出)

## 5. 数据流与处理

### 报表创建流程
1. 创建新报表或打开现有报表
2. 配置数据源
3. 设计报表布局（拖放组件）
4. 设置数据绑定
5. 预览报表
6. 导出或打印

### 渲染流程
1. 加载报表定义
2. 连接数据源
3. 获取数据
4. 创建可视化树
5. 应用数据绑定
6. 分页处理
7. 输出到目标格式

## 6. 验收标准

### 功能验收
- [ ] 可以创建新的空白报表
- [ ] 可以保存和加载报表文件
- [ ] 所有工具箱组件可以拖放到设计区域
- [ ] 组件属性可以在属性面板中编辑
- [ ] 可以添加和配置数据源
- [ ] 可以将数据字段绑定到组件
- [ ] 报表预览功能正常工作
- [ ] 可以导出为PDF格式
- [ ] 撤销/重做功能正常

### 视觉验收
- [ ] 界面布局符合设计规范
- [ ] 配色方案符合规范
- [ ] 工具箱图标清晰可见
- [ ] 属性面板分组清晰
- [ ] 设计画布网格显示正确

### 性能验收
- [ ] 启动时间 < 3秒
- [ ] 报表加载时间 < 1秒
- [ ] 预览生成时间 < 2秒
- [ ] 导出PDF时间 < 3秒

### 兼容性验收
- [ ] Windows 10/11兼容
- [ ] .NET 8运行时兼容
