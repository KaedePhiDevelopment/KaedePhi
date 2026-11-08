# KaedePhi
我知晓，那无边的旅途。

## 安装与运行
从 GitHub `Release` 下载应用安装包或便携版压缩包，也可以从源码编译。应用发布包采用框架依赖部署（FDD），不会捆绑 .NET 运行时。

### 安装运行时
- Windows：安装 [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/10.0)，再运行安装包或便携版中的 `KaedePhi.Tool.App.exe`。
- Linux：安装 [.NET 10 Runtime](https://dotnet.microsoft.com/download/dotnet/10.0)，解压 `linux-x64` 便携版后运行 `KaedePhi.Tool.App`。
- macOS：从 GitHub Release 下载 `KaedePhi-App-v*-macOS-x86_64-Installer.dmg`（Intel）或 `KaedePhi-App-v*-macOS-arm64-Installer.dmg`（Apple 芯片），打开后将 `KaedePhi.app` 拖入“应用程序”文件夹；首次运行前需安装 [.NET 10 Runtime](https://dotnet.microsoft.com/download/dotnet/10.0)。当前应用包未进行 Apple 开发者签名与公证，首次打开时请按住 Control 键点按应用并选择“打开”，再确认运行。
- 正式应用产物使用 `net10.0` FDD：提供 Windows x64、Linux x64 便携版，Windows x64 安装包，以及分别面向 Intel 和 Apple 芯片的 macOS `.dmg` 安装包。

## 注意事项
> [!CAUTION]
> <span style="color:red">**如果您使用本软件进行低质量创作，本软件将对您进行道德谴责，受限于开源协议，项目维护者无权阻止您的任何行为！**</span>  

> **本项目自1.2.0版本行为趋于稳定，欢迎各位开发者提出意见，但是在跨大版本更新时，仍然可能存在破坏性更改的可能。**

### 关于StellateRePhiEditExtended
这是由[不会特效の点缀星空](https://space.bilibili.com/1792961650)为RPE Recorder开发的，基于RePhiEdit格式的非格式修改性扩展，从其它格式转换到此格式**只能保证在3:2比例尺下显示效果正确，有不可避免的坐标系畸变问题**。此问题我无法修复，对此深感抱歉。

## CLI 使用
发布包中的 `KaedePhi.Tool.App` 同时提供 CLI 和 GUI。传入命令或 `--cli` 时使用 CLI，传入 `--gui` 时启动 GUI；交互式终端中直接运行也会进入 CLI。

```bash
# 查看命令和选项
KaedePhi.Tool.App --help

# 查看版本
KaedePhi.Tool.App version

# 将谱面转换为 PhiEdit .pec，输入格式会自动检测
KaedePhi.Tool.App convert --input input.json --target PhiEdit --output output.pec --format

# 转换为 Phigros v3 JSON
KaedePhi.Tool.App convert --input input.pec --target PhigrosV3 --output output.json

# 将事件渲染为 PNG，默认输出到输入文件旁的 render_output 目录
KaedePhi.Tool.App render --input input.json

# 使用工作区进行多步处理
KaedePhi.Tool.App load --input input.json --workspace demo
KaedePhi.Tool.App convert --workspace demo --target PhiEdit --output output.pec
KaedePhi.Tool.App workspace list
KaedePhi.Tool.App workspace clear --id demo
KaedePhi.Tool.App workspace clear --all

# 重置 CLI 与 GUI 共用的配置
KaedePhi.Tool.App config reset
```

## 配置与数据路径
应用使用 .NET 的本机应用数据目录：
- Windows：`%LOCALAPPDATA%\KaedePhi\config\config.yaml`、`%LOCALAPPDATA%\KaedePhi\workspaces`、`%LOCALAPPDATA%\KaedePhi\logs`
- Linux：通常为 `~/.local/share/KaedePhi/config/config.yaml`、`~/.local/share/KaedePhi/workspaces`、`~/.local/share/KaedePhi/logs`

配置文件由 CLI 和 GUI 共享。工作区只保存名为 `chart.json` 的原始谱面文件，工作区 ID 只允许字母、数字、下划线和连字符。

## 限制说明
- FDD 便携版和安装版都要求先安装对应的 .NET 10 运行时，不能脱离运行时单独执行。
- macOS 安装包未进行 Apple 开发者签名与公证，首次启动需手动确认；Intel 与 Apple 芯片用户需下载对应架构的安装包。
- 目前正式应用产物提供 Windows x64、Linux x64 和 macOS x86_64/arm64；其他系统和架构需要自行编译验证。
- 部分目标格式不支持源格式的全部事件或缓动类型，转换时可能进行采样、拟合或压缩；大型谱面可尝试 `--stream` 降低内存占用。

## 开发者文档
构建、测试、发布流程、谱面格式扩展和接口迁移说明见 [DEVELOPMENT.md](DEVELOPMENT.md)。

## 招新
本项目需要更多人开发与维护，欢迎发送邮件到 nrlt@nuanr-mxi.com 来加入开发！  
若您需要其它私下联系方式，请在邮件中说明。

## 开源许可证
[GNU LESSER GENERAL PUBLIC LICENSE 3.0](https://www.gnu.org/licenses/lgpl-3.0.html)

## Copyright
NuanR_Mxi Lazy Team Copyright © 2026 KaedePhi Project.  
NuanR_Star Lazy Team Copyright © 2026 KaedePhi Project.  
NuanR_Star Ciallo Team Copyright © 2026 KaedePhi Project.
KaedePhiDevelopment Team Copyright © 2026 KaedePhi Project.
## 致谢
[cmdysj](https://space.bilibili.com/252635690)  
[HLMC](https://space.bilibili.com/357681195)  
[不会特效の点缀星空](https://space.bilibili.com/1792961650)  
[PhiFans](https://github.com/PhiFans)  
[Ivan-1F](https://github.com/Ivan-1F)  
所有参与测试反馈的各位以及贡献者  
和屏幕前的你！

# 本仓库内 PNG、ICO 文件的授权及版权声明

本仓库内所使用的 PNG、ICO 等图像文件均由 MySxan 绘制，其著作权归 NuanR_Mxi 个人所有（不含字体文件）。

上述文件的使用授权范围仅限于以下用途：

1. 本项目及其相关内容；
2. 基于本项目产生的衍生作品；
3. NuanR_Star Ciallo Team 的相关项目及活动，但不包括其附属组织或关联团队，例如 MoeRain。

除上述授权范围外，未经 NuanR_Mxi 明确书面授权，任何个人或组织不得以任何形式使用、复制、修改、传播、再授权或用于其他商业或非商业用途。

禁止用途包括但不限于：

- 制作或开发其他软件、应用程序或游戏；
- 制作或运营其他网站、网页或相关服务；
- 将相关文件用于 AI 训练数据集、模型训练或其他人工智能相关用途；
- 将相关文件用于任何与本项目无关的商业或非商业项目。

本声明仅针对上述 PNG、ICO 等图像文件，其授权范围不因本仓库其他文件所采用的开源许可证或其他授权条款而自动扩大。

NuanR_Mxi All Rights Reserved.

# 外部 NuGet 包及其许可证

下表汇总本仓库所有已提交 `packages.lock.json` 中的 112 个外部 NuGet 包，包含直接依赖、传递依赖以及测试和构建工具依赖。版本列列出各目标框架和运行时标识中锁定的全部版本；“引用关系”表示该包在依赖图中作为直接依赖、传递依赖或两者出现。项目内部项目引用不属于外部包。包自身及其内含组件的许可证声明可能不同，重新分发时还应保留包内的 `LICENSE`、`NOTICE` 和 `THIRD-PARTY-NOTICES` 文件。

> **特别注意：** `FluentAssertions` 8.11.0 仅用于测试，其许可为 Xceed Community License Agreement（Non-Commercial Use），并非 MIT 等通用开源许可证。该许可对组织或商业场景的使用有限制；如项目在组织或商业场景中开发、测试或使用，请先核对完整条款，必要时取得相应商业许可或替换该依赖。
>
> `Microsoft.NETCore.Platforms` 和 `Microsoft.Testing.Extensions.CodeCoverage` 使用 Microsoft Software License Terms，而非 SPDX 开源许可证；使用或重新分发时应遵守其各自的许可条款及包内第三方声明。
>
> `Avalonia.Fonts.Inter` 的 NuGet 包许可为 MIT，但其中的 Inter 字体另遵循 [SIL Open Font License 1.1](https://github.com/rsms/inter/blob/master/LICENSE.txt)。`SkiaSharp.NativeAssets.*` 包还带有单独的 `THIRD-PARTY-NOTICES.txt`，其中包含其内含组件的声明。
>
> 本仓库直接附带的 Font Awesome 字体许可见下文“Font Awesome 7 Free Solid 字体授权声明”及 `KaedePhi.Tool.App/Assets/Fonts/LICENSE.txt`。

| NuGet 包                                              | 引用关系   | 许可证                                                                                                                       |
|-------------------------------------------------------|------------|------------------------------------------------------------------------------------------------------------------------------|
| `Avalonia`                                            | 直接、传递 | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `Avalonia.Angle.Windows.Natives`                      | 传递       | [BSD-3-Clause](https://www.nuget.org/packages/Avalonia.Angle.Windows.Natives/2.1.27548.20260419#license)                     |
| `Avalonia.BuildServices`                              | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `Avalonia.Controls.ColorPicker`                       | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `Avalonia.Desktop`                                    | 直接、传递 | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `Avalonia.Diagnostics`                                | 直接、传递 | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `Avalonia.Fonts.Inter`                                | 直接、传递 | [MIT](https://spdx.org/licenses/MIT.html)；内含 Inter 字体另见上文                                                           |
| `Avalonia.FreeDesktop`                                | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `Avalonia.FreeDesktop.AtSpi`                          | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `Avalonia.HarfBuzz`                                   | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `Avalonia.Native`                                     | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `Avalonia.Remote.Protocol`                            | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `Avalonia.Skia`                                       | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `Avalonia.Themes.Fluent`                              | 直接、传递 | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `Avalonia.Themes.Simple`                              | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `Avalonia.Win32`                                      | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `Avalonia.X11`                                        | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `CAPCOM.REDox`                                        | 直接、传递 | [Apache-2.0](https://spdx.org/licenses/Apache-2.0.html)                                                                      |
| `CAPCOM.REDox.Serialization.NewtonsoftJson`           | 直接、传递 | [Apache-2.0](https://spdx.org/licenses/Apache-2.0.html)                                                                      |
| `Castle.Core`                                         | 传递       | [Apache-2.0](https://spdx.org/licenses/Apache-2.0.html)                                                                      |
| `DiffPlex`                                            | 传递       | [Apache-2.0](https://spdx.org/licenses/Apache-2.0.html)                                                                      |
| `FluentAssertions`                                    | 直接       | [Xceed Community License Agreement (Non-Commercial Use)](https://www.nuget.org/packages/FluentAssertions/8.11.0#license)     |
| `HarfBuzzSharp`                                       | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `HarfBuzzSharp.NativeAssets.Linux`                    | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `HarfBuzzSharp.NativeAssets.macOS`                    | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `HarfBuzzSharp.NativeAssets.WebAssembly`              | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `HarfBuzzSharp.NativeAssets.Win32`                    | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `Humanizer.Core`                                      | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `JetBrains.Annotations`                               | 直接、传递 | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `MicroCom.Runtime`                                    | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `Microsoft.ApplicationInsights`                       | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `Microsoft.Bcl.AsyncInterfaces`                       | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `Microsoft.CodeAnalysis.Analyzer.Testing`             | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `Microsoft.CodeAnalysis.Analyzers`                    | 直接、传递 | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `Microsoft.CodeAnalysis.CodeFix.Testing`              | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `Microsoft.CodeAnalysis.Common`                       | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `Microsoft.CodeAnalysis.CSharp`                       | 直接、传递 | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `Microsoft.CodeAnalysis.CSharp.Analyzer.Testing`      | 直接       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `Microsoft.CodeAnalysis.CSharp.CodeFix.Testing`       | 直接       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `Microsoft.CodeAnalysis.CSharp.Workspaces`            | 直接、传递 | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `Microsoft.CodeAnalysis.Workspaces.Common`            | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `Microsoft.CodeCoverage`                              | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `Microsoft.DiaSymReader`                              | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `Microsoft.Extensions.DependencyModel`                | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `Microsoft.NET.Test.Sdk`                              | 直接       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `Microsoft.NETCore.Platforms`                         | 传递       | [Microsoft Software License Terms](https://www.nuget.org/packages/Microsoft.NETCore.Platforms/1.1.0#license)                 |
| `Microsoft.Testing.Extensions.CodeCoverage`           | 直接       | [Microsoft Software License Terms](https://www.nuget.org/packages/Microsoft.Testing.Extensions.CodeCoverage/18.11.2#license) |
| `Microsoft.Testing.Extensions.Telemetry`              | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `Microsoft.Testing.Extensions.TrxReport.Abstractions` | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `Microsoft.Testing.Platform`                          | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `Microsoft.Testing.Platform.MSBuild`                  | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `Microsoft.TestPlatform.ObjectModel`                  | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `Microsoft.TestPlatform.TestHost`                     | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `Microsoft.VisualStudio.Composition`                  | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `Microsoft.VisualStudio.Composition.NetFxAttributes`  | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `Microsoft.VisualStudio.Validation`                   | 传递       | [MIT](https://raw.githubusercontent.com/Microsoft/vs-validation/1fc5bdcf81/LICENSE)                                          |
| `Microsoft.Win32.Registry`                            | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `Moq`                                                 | 直接       | [BSD-3-Clause](https://spdx.org/licenses/BSD-3-Clause.html)                                                                  |
| `NETStandard.Library`                                 | 直接       | [MIT](https://github.com/dotnet/standard/blob/master/LICENSE.TXT)                                                            |
| `Newtonsoft.Json`                                     | 直接、传递 | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `NuGet.Common`                                        | 传递       | [Apache-2.0](https://spdx.org/licenses/Apache-2.0.html)                                                                      |
| `NuGet.Configuration`                                 | 传递       | [Apache-2.0](https://spdx.org/licenses/Apache-2.0.html)                                                                      |
| `NuGet.Frameworks`                                    | 传递       | [Apache-2.0](https://spdx.org/licenses/Apache-2.0.html)                                                                      |
| `NuGet.Packaging`                                     | 传递       | [Apache-2.0](https://spdx.org/licenses/Apache-2.0.html)                                                                      |
| `NuGet.Protocol`                                      | 传递       | [Apache-2.0](https://spdx.org/licenses/Apache-2.0.html)                                                                      |
| `NuGet.Resolver`                                      | 传递       | [Apache-2.0](https://spdx.org/licenses/Apache-2.0.html)                                                                      |
| `NuGet.Versioning`                                    | 传递       | [Apache-2.0](https://spdx.org/licenses/Apache-2.0.html)                                                                      |
| `Serilog`                                             | 直接、传递 | [Apache-2.0](https://spdx.org/licenses/Apache-2.0.html)                                                                      |
| `Serilog.Sinks.Console`                               | 直接、传递 | [Apache-2.0](https://spdx.org/licenses/Apache-2.0.html)                                                                      |
| `Serilog.Sinks.File`                                  | 直接、传递 | [Apache-2.0](https://spdx.org/licenses/Apache-2.0.html)                                                                      |
| `SkiaSharp`                                           | 直接、传递 | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `SkiaSharp.NativeAssets.Linux`                        | 直接、传递 | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `SkiaSharp.NativeAssets.macOS`                        | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `SkiaSharp.NativeAssets.WebAssembly`                  | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `SkiaSharp.NativeAssets.Win32`                        | 直接、传递 | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `System.Buffers`                                      | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `System.Collections.Immutable`                        | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `System.CommandLine`                                  | 直接、传递 | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `System.ComponentModel.Composition`                   | 传递       | [MIT](https://github.com/dotnet/corefx/blob/master/LICENSE.TXT)                                                              |
| `System.Composition`                                  | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `System.Composition.AttributedModel`                  | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `System.Composition.Convention`                       | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `System.Composition.Hosting`                          | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `System.Composition.Runtime`                          | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `System.Composition.TypedParts`                       | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `System.Diagnostics.EventLog`                         | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `System.IO.Pipelines`                                 | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `System.Memory`                                       | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `System.Numerics.Vectors`                             | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `System.Reflection.Metadata`                          | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `System.Runtime.CompilerServices.Unsafe`              | 直接、传递 | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `System.Security.AccessControl`                       | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `System.Security.Cryptography.Pkcs`                   | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `System.Security.Cryptography.ProtectedData`          | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `System.Security.Permissions`                         | 传递       | [MIT](https://github.com/dotnet/corefx/blob/master/LICENSE.TXT)                                                              |
| `System.Text.Encoding.CodePages`                      | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `System.Text.Encodings.Web`                           | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `System.Text.Json`                                    | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `System.Threading.Channels`                           | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `System.Threading.Tasks.Extensions`                   | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `Tmds.DBus.Protocol`                                  | 传递       | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |
| `xunit.analyzers`                                     | 传递       | [Apache-2.0](https://spdx.org/licenses/Apache-2.0.html)                                                                      |
| `xunit.runner.visualstudio`                           | 直接       | [Apache-2.0](https://spdx.org/licenses/Apache-2.0.html)                                                                      |
| `xunit.v3`                                            | 直接       | [Apache-2.0](https://spdx.org/licenses/Apache-2.0.html)                                                                      |
| `xunit.v3.assert`                                     | 传递       | [Apache-2.0](https://spdx.org/licenses/Apache-2.0.html)                                                                      |
| `xunit.v3.common`                                     | 传递       | [Apache-2.0](https://spdx.org/licenses/Apache-2.0.html)                                                                      |
| `xunit.v3.core.mtp-v2`                                | 传递       | [Apache-2.0](https://spdx.org/licenses/Apache-2.0.html)                                                                      |
| `xunit.v3.extensibility.core`                         | 传递       | [Apache-2.0](https://spdx.org/licenses/Apache-2.0.html)                                                                      |
| `xunit.v3.mtp-v2`                                     | 传递       | [Apache-2.0](https://spdx.org/licenses/Apache-2.0.html)                                                                      |
| `xunit.v3.runner.common`                              | 传递       | [Apache-2.0](https://spdx.org/licenses/Apache-2.0.html)                                                                      |
| `xunit.v3.runner.inproc.console`                      | 传递       | [Apache-2.0](https://spdx.org/licenses/Apache-2.0.html)                                                                      |
| `YamlDotNet`                                          | 直接、传递 | [MIT](https://spdx.org/licenses/MIT.html)                                                                                    |

# Font Awesome 7 Free Solid 字体授权声明

详细请参阅本仓库内 KaedePhi.Tool.App/Assets/Fonts/LICENSE.txt 文本内容。
