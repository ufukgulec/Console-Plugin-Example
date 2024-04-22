# Console - Plugin Manager
Eklentileri yüklemek için özel bir AssemblyLoadContext'in nasıl oluşturulacağını gösteren ufak bir projedir. Eklentinin bağımlılıklarını çözmek için bir AssemblyDependencyResolver kullanılır. Eklentinin bağımlılıklarını barındırma uygulamasından doğru şekilde izole eder.

Göz attığım [microsoft dökümanı](https://learn.microsoft.com/en-us/dotnet/core/tutorials/creating-app-with-plugin-support/)

## Özellikler
Ana projede (AppWithPlugin) eklenmiş olan eklenti projeleri (HelloPlugin ve GithubPlugin) bağımlılık olarak eklemeden çalışmasını sağlayan bir çözüm oluşturun. Bu, ana proje ile eklenti projeleri arasındaki bağımlılığı ortadan kaldırarak projeler arası esneklik ve bağımsızlık kazandırır.

## AppWithPlugin/Program.cs - LoadPlugin Metodu
> Aynı çözüm altında oluşan farklı projeler olarak düşündüğüm için çözümün path'ini kök dizin `string root` olarak aldım.

```csharp
static Assembly LoadPlugin(string relativePath)
{
    
    string root = Path.GetFullPath(Path.Combine(
        Path.GetDirectoryName(
            Path.GetDirectoryName(
                Path.GetDirectoryName(
                    Path.GetDirectoryName(
                        Path.GetDirectoryName(typeof(Program).Assembly.Location)))))));

    string pluginLocation = Path.GetFullPath(Path.Combine(root, relativePath.Replace('\\', Path.DirectorySeparatorChar)));

    PluginLoadContext loadContext = new PluginLoadContext(pluginLocation);

    return loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(pluginLocation)));
}
```
## AppWithPlugin/Program.cs - CreateCommands Metodu
> `ICommand` interface'inden türeyen sınıfları bulur. 
```csharp
static IEnumerable<ICommand> CreateCommands(Assembly assembly)
{
    int count = 0;

    foreach (Type type in assembly.GetTypes())
    {
        if (typeof(ICommand).IsAssignableFrom(type))
        {
            ICommand result = Activator.CreateInstance(type) as ICommand;
            if (result != null)
            {
                count++;
                yield return result;
            }
        }
    }

    if (count == 0)
    {
        string availableTypes = string.Join(",", assembly.GetTypes().Select(t => t.FullName));
        throw new ApplicationException(
            $"Can't find any type which implements ICommand in {assembly} from {assembly.Location}.\n" +
            $"Available types: {availableTypes}");
    }
}
```
> [!IMPORTANT]
> Plugin olarak hazırlanan projelerde csproj dosyasında düzenlemeler yapılmalıdır.


```javascript
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <EnableDynamicLoading>true</EnableDynamicLoading>
  </PropertyGroup>

  <ItemGroup>
	  <ProjectReference Include="..\PluginBase\PluginBase.csproj">
		  <Private>false</Private>
		  <ExcludeAssets>runtime</ExcludeAssets>
	  </ProjectReference>
  </ItemGroup>

</Project>
```
- EnableDynamicLoading → Projeyi eklenti olarak kullanılabilecek şekilde hazırlar . Diğer şeylerin yanı sıra bu, tüm bağımlılıklarını projenin çıktısına kopyalayacaktır.
- Private → Önemlidir. Bu, MSBuild'e PluginBase.dll dosyasını HelloPlugin'in çıkış dizinine kopyalamamasını söyler.
- ExcludeAssets → Önemlidir. PluginBaseBu ayar aynı etkiye sahiptir ancak projenin veya bağımlılıklarından birinin içerebileceği <Private>false</Private>paket referansları üzerinde çalışır.
## Kullanım
- Pluginler eklenir. (Pathleri verilerek eklenir.)
- Listelemede eklenen pluginler gözükür. (Pluginin `Name` ve `Message` alanları gözükür.)
- Çalıştırmak istediğimizde pluginin `Execute` metodu çalışır.

## Plugin Projelerin Pathleri
1. `HelloPlugin\bin\Debug\net8.0\HelloPlugin.dll`
2. `GithubPlugin\bin\Debug\net8.0\GithubPlugin.dll`
  
## Ekran Görüntüleri
![image](https://github.com/ufukgulec/Console-Plugin-Example/assets/51711890/349b8cd4-ebfa-48f5-9d0d-5f2633ba739a)j
![image](https://github.com/ufukgulec/Console-Plugin-Example/assets/51711890/8791ee8d-acb3-43b5-a616-eb1215451a7a)
![image](https://github.com/ufukgulec/Console-Plugin-Example/assets/51711890/214bebd8-f0f0-4c08-942c-cd81995d892f)
![image](https://github.com/ufukgulec/Console-Plugin-Example/assets/51711890/1cc0aacd-d32a-4094-91a8-65a398229def)  
