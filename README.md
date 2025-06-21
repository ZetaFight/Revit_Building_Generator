# Revit Building Generator

这是一个为 Autodesk Revit 开发的插件，用于自动生成建筑几何体。使用 C# 和 Visual Studio 2019 编写，支持参数化控制与用户界面交互。
![image](https://github.com/user-attachments/assets/4d9c986b-bd7b-485e-ab2b-6278a1788fa3)


## 功能特性

🔷 **参数化建筑生成**  
根据用户自定义参数（如轴网间距、标高等），自动生成规则化建筑模型，支持快速变更设计方案。

🔷 **集成于 Revit 插件界面**  
插件作为 Revit 扩展功能加载到“插件”面板，界面友好，操作简便，无需切换软件或外部工具。

🔷 **WPF 界面交互**  
通过 WPF 实现的配置窗口，支持实时参数输入与预览，增强用户体验。

🔷 **楼层结构与轴网自动生成**  
根据输入参数自动布置建筑的梁柱板墙窗户等基础元素生成逻辑。

🔷 **模块化设计**  
项目使用清晰的模块划分（命令模块、UI模块、工具模块），便于二次开发和维护。


## 如何使用

1. 使用 Visual Studio 2019 打开 `MyPlugin.sln`
2. 构建项目，生成 DLL 文件
3. 将 DLL 复制到 Revit 的插件目录中：  
   `%AppData%\Autodesk\Revit\Addins\2018\`
4. 打开 Revit，载入插件，工具栏面板自动添加"My Plugin"板块，点击使用

## 环境要求

- Autodesk Revit 2018
- .NET Framework 4.5.2
- Visual Studio 2019

## 作者

ZetaFight  
GitHub: [ZetaFight](https://github.com/ZetaFight)
