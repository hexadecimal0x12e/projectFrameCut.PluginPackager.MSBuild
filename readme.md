# projectFrameCut.PluginPackager.MSBuild

一个为 projectFrameCut 插件开发者提供的 MSBuild 任务，实现自动创建插件信息和加载器，以及在构建过程中自动打包插件。
要了解更多，请参阅[插件模板](https://github.com/hexadecimal0x12e/projectFrameCut.PluginTemplate)


# 快速开始（MSBuild 集成）

1. 在你的项目中导入 targets：

```xml
<PackageReference Include="projectFrameCut.PluginPackager.MSBuild" Version="3.0.0" />
```

2. 配置插件API版本号和签名：
```xml
<PropertyGroup>
        ...
	<PluginMajorVersion>4</PluginMajorVersion>
	<AppLevelPluginMajorVersion>4</AppLevelPluginMajorVersion>
    <PluginMinorVersion>0</PluginMinorVersion>
    <PluginSignFilePath>D:\path\key.json</PluginSignFilePath>
        ...
</PropertyGroup>
```

3. 使用`dotnet publish`命令来发布，这时会触发打包任务并在输出目录生成插件包。

# 打包器配置
打包器向MSBuild提供了一些参数来控制打包器的行为：
### 基础信息
**你必须配置他们**，这些配置不是由打包器特有的，而是标准的NuGet包属性：
* `PackageId`： 插件的包ID，也会作为它的唯一标识符，
    **请确保他和你的插件类的全名一致，并且不得以`projectFrameCut`开头（不区分大小写）**。 
    如果同时配置了`PluginIDOverride`和`PackageId`，打包器会使用`PluginIDOverride`的值作为插件ID。
* `Version`： 插件的版本号
* `PackageProjectUrl`： 插件的项目主页URL，可以留空

下面这些属性都是没有本地化的，如果你想要本地化这些信息，请配置插件基类的`LocalizationProvider`。
* `Title`： 插件的显示名称
* `Authors`： 插件的作者
* `Description`： 插件的描述

### 插件API版本
**你必须配置他们**，来告诉打包器你的插件是基于哪个版本的插件API开发的，否则打包器会报错。
* `PluginMajorVersion`：插件API的主版本号，必须设置这个属性来告诉打包器你的插件是基于哪个版本的插件API开发的。
* `AppLevelPluginMajorVersion`：应用程序级插件API的主版本号，必须设置这个属性来告诉打包器你的插件是基于哪个版本的应用程序级插件API开发的。
* `PluginMinorVersion`：插件API的次版本号，默认为0，可以不设置这个属性。

### 签名
* `PluginSignFilePath`：必须设置这个属性来配置签名密钥
    最方便的方式是使用[插件模板](https://github.com/hexadecimal0x12e/projectFrameCut.PluginTemplate)里提供的`PluginKeyGenerator.cs`来生成一个签名文件。

    如果你想要手动构建签名文件，你需要先准备一个Base64格式的PKCS #8密钥对，然后创建一个JSON文件，填入公钥到签名文件的`Key`字段，填入私钥到`Value`字段。
    类似于这样：
    ```json
    {
        "Key": "-----BEGIN PUBLIC KEY-----\nMIIBIjAN***(REDACTED)***BgQAB\r\n-----END PUBLIC KEY-----\n",
        "Value": "-----BEGIN PRIVATE KEY-----\nMII***(REDACTED)***4VnM=\r\n-----END PRIVATE KEY-----\n"
    }
    ```

### 素材
* `PluginAssetPath`：插件素材的路径，打包器会把这个路径下的所有文件都包含进插件包里。如果不需要素材，可以不设置这个属性。

### 源生成
* `GeneratePluginInfoSource`：是否生成插件信息源代码文件，默认为 `true`。如果你通过继承标准插件来实现应用程序插件，或者就是想手动实现插件信息，设置为 `false` 可以阻止源生成。
* `GeneratePluginInfoSourcePath`：生成的插件信息源代码文件的路径，
    默认为空，表示生成在 `obj\GeneratedSource\PluginInfo.g.cs`。如果你想指定路径，可以设置为你想要的路径。
* `PluginIDOverride`：用于覆盖生成的插件ID，而不是使用NuGet属性`PackageId`。如果你通过继承标准插件来实现应用程序插件，你需要设置这个参数来确保他们的插件ID一致。

# 打包插件
使用`dotnet publish`命令来打包你的插件，编译完成之后，在输出目录下会生成一个`.pfcplugin`的插件包文件。

你必须使用 .NET CLI来打包，**不要使用Visual Studio的打包功能**。如果你已经在使用命令行但是持续遇到报错`To bundle a plugin, please use 'dotnet publish' command, instead of using Visual Studio's Publish tool...`，请添加命令行参数`-p:BundlePlugin=true` 到`dotnet publish`命令里。


