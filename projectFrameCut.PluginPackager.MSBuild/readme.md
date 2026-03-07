# projectFrameCut Plugin Packager
这个工具允许你使用MSBuild打包一个兼容[projectFrameCut](https://github.com/hexadecimal0x12e/projectFrameCut)的插件。

# 如何使用
把这个包作为你的项目的引用：
```xml
<PackageReference Include="projectFrameCut.PluginPackager.MSBuild" Version="2.0.0" />
```
开发你的插件，然后使用MSBuild生成你的项目：
```bash
dotnet build -c Release
```

更多的信息请参考[仓库的主文档](https://github.com/hexadecimal0x12e/projectFrameCut.PluginTemplate)。

# 打包器配置
打包器向MSBuild提供了一些参数来控制打包器的行为：
### 基础信息
这些配置不是由打包器特有的，而是标准的NuGet包属性：
* `PackageId`： 插件的包ID，也会作为它的唯一标识符，
    **请确保他和你的插件类的全名一致，并且不得以`projectFrameCut`开头（不区分大小写）**。 
    如果同时配置了`PluginIDOverride`和`PackageId`，打包器会使用`PluginIDOverride`的值作为插件ID。
* `Version`： 插件的版本号
* `PackageProjectUrl`： 插件的项目主页URL，可以留空

下面这些属性都是没有本地化的，如果你想要本地化这些信息，请配置插件基类的`LocalizationProvider`。
* `Title`： 插件的显示名称
* `Authors`： 插件的作者
* `Description`： 插件的描述

### 签名
* `PluginSignFilePath`：必须设置这个属性来配置签名密钥
    最方便的方式是使用`PluginKeyGenerator.cs`来生成一个签名文件。
    如果你想要手动构建签名文件，你需要先准备一个Base64格式的PKCS #8密钥对，然后创建一个JSON文件，填入公钥到签名文件的`Key`字段，填入私钥到`Value`字段。

### 素材
* `PluginAssetPath`：插件素材的路径，打包器会把这个路径下的所有文件都包含进插件包里。如果不需要素材，可以不设置这个属性。

### 源生成
* `GeneratePluginInfoSource`：是否生成插件信息源代码文件，默认为 `true`。如果你通过继承标准插件来实现应用程序插件，或者就是想手动实现插件信息，设置为 `false` 可以阻止源生成。
* `GeneratePluginInfoSourcePath`：生成的插件信息源代码文件的路径，
    默认为空，表示生成在 `obj\GeneratedSource\PluginInfo.g.cs`。如果你想指定路径，可以设置为你想要的路径。
* `PluginIDOverride`：用于覆盖生成的插件ID，而不是使用NuGet属性`PackageId`。如果你通过继承标准插件来实现应用程序插件，你需要设置这个参数来确保他们的插件ID一致。


