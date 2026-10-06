# Console Plugin Manager

**Console Plugin Manager**, .NET uygulamalarında runtime'da harici plugin'lerin yüklenmesini ve çalıştırılmasını göstermek amacıyla geliştirilmiş bir örnek projedir.

Proje; plugin'lerin ana uygulamadan bağımsız olarak yüklenmesini, bağımlılıklarının izole edilmesini ve ortak bir interface üzerinden keşfedilip çalıştırılmasını göstermektedir.

Temel olarak **`AssemblyLoadContext`** ve **`AssemblyDependencyResolver`** kullanılarak dinamik assembly loading mekanizması uygulanmıştır.

## 🎯 Amaç

Bu projenin temel amacı, bir .NET uygulamasının plugin projelerine **compile-time dependency** oluşturmadan, plugin'leri çalışma zamanında keşfedip yükleyebilmesini göstermektir.

Örneğin:

```text
AppWithPlugin
    │
    ├── PluginBase
    │
    ├── HelloPlugin
    │
    └── GithubPlugin
```

`AppWithPlugin`, `HelloPlugin` veya `GithubPlugin` projelerine doğrudan bağımlı değildir.

Plugin'ler DLL path'i üzerinden runtime'da yüklenir.

## 🛠️ Teknolojiler

* C#
* .NET 8
* .NET `AssemblyLoadContext`
* `AssemblyDependencyResolver`
* Reflection
* Dynamic Assembly Loading
* Interface-based Plugin Architecture

## 🏗️ Proje Yapısı

```text
Console-Plugin-Example/
│
├── AppWithPlugin/
│   └── Host console application
│
├── PluginBase/
│   └── Common plugin contract
│
├── HelloPlugin/
│   └── Example plugin
│
├── GithubPlugin/
│   └── Example plugin
│
├── PluginTest.sln
└── README.md
```

### AppWithPlugin

Plugin'lerin runtime'da yüklendiği ana console uygulamasıdır.

Temel sorumlulukları:

* Plugin path'lerini almak
* Plugin assembly'lerini yüklemek
* Plugin'leri keşfetmek
* Plugin'leri listelemek
* Kullanıcının seçtiği plugin'i çalıştırmak

### PluginBase

Host uygulaması ile plugin'ler arasındaki ortak sözleşmeyi tanımlar.

Plugin'ler `ICommand` interface'i üzerinden host uygulama ile iletişim kurar.

Bu sayede host uygulamanın plugin'in gerçek implementasyonunu bilmesine gerek kalmaz.

### HelloPlugin

Plugin mimarisini göstermek amacıyla hazırlanmış basit örnek plugin'dir.

### GithubPlugin

Aynı plugin altyapısının birden fazla plugin tarafından kullanılabileceğini göstermek amacıyla oluşturulmuş ikinci örnek plugin'dir.

## 🔌 Plugin Mimarisi

Plugin'lerin ortak sözleşmesi `PluginBase` içerisinde tanımlanır.

Host uygulama assembly içerisinde `ICommand` interface'ini implement eden type'ları reflection kullanarak bulur.

Temel akış:

```text
Plugin DLL
    │
    ▼
AssemblyLoadContext
    │
    ▼
AssemblyDependencyResolver
    │
    ▼
Assembly
    │
    ▼
Reflection
    │
    ▼
ICommand implementations
    │
    ▼
Plugin instance
    │
    ▼
Execute()
```

## 📦 Runtime Plugin Loading

Plugin yükleme işlemi özel bir `PluginLoadContext` üzerinden gerçekleştirilir.

Örnek:

```csharp
static Assembly LoadPlugin(string relativePath)
{
    string pluginLocation = Path.GetFullPath(
        Path.Combine(root, relativePath));

    PluginLoadContext loadContext =
        new PluginLoadContext(pluginLocation);

    return loadContext.LoadFromAssemblyName(
        new AssemblyName(
            Path.GetFileNameWithoutExtension(pluginLocation)));
}
```

Bu yaklaşım sayesinde plugin'in assembly'leri host uygulamanın assembly context'inden ayrıştırılabilir.

## 🧩 AssemblyDependencyResolver

Plugin'lerin kendilerine ait dependency'leri bulunabilir.

`AssemblyDependencyResolver`, plugin'in bulunduğu konuma göre dependency'lerin çözülmesini sağlar.

```text
Host Application
       │
       ├── Host dependencies
       │
       └── PluginLoadContext
                │
                ├── Plugin.dll
                ├── Dependency A
                └── Dependency B
```

Bu yapı, farklı plugin'lerin farklı dependency versiyonlarına sahip olabileceği senaryolarda önemli bir izolasyon mekanizması sağlar.

## 🔎 Plugin Discovery

Plugin assembly'si yüklendikten sonra reflection kullanılarak `ICommand` interface'ini implement eden sınıflar aranır.

```csharp
foreach (Type type in assembly.GetTypes())
{
    if (typeof(ICommand).IsAssignableFrom(type))
    {
        ICommand result =
            Activator.CreateInstance(type) as ICommand;

        if (result != null)
        {
            yield return result;
        }
    }
}
```

Böylece host uygulamanın plugin içerisindeki concrete class'ları önceden bilmesine gerek kalmaz.

## ⚙️ Plugin Projesi Gereksinimleri

Plugin olarak kullanılacak projelerde dynamic loading desteğinin etkinleştirilmesi gerekir.

```xml
<PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <EnableDynamicLoading>true</EnableDynamicLoading>
</PropertyGroup>
```

`PluginBase` referansı ise runtime çıktısına kopyalanmayacak şekilde yapılandırılır:

```xml
<ItemGroup>
    <ProjectReference Include="..\PluginBase\PluginBase.csproj">
        <Private>false</Private>
        <ExcludeAssets>runtime</ExcludeAssets>
    </ProjectReference>
</ItemGroup>
```

Bu yapı, `PluginBase.dll` dosyasının plugin output klasörüne gereksiz şekilde kopyalanmasını önlemeye yardımcı olur.

## ▶️ Kullanım

Uygulamanın temel kullanım akışı:

```text
1. Plugin path'i tanımlanır
        ↓
2. Plugin DLL yüklenir
        ↓
3. Plugin assembly keşfedilir
        ↓
4. ICommand implementasyonları bulunur
        ↓
5. Plugin'ler listelenir
        ↓
6. Kullanıcı plugin seçer
        ↓
7. Execute() çalıştırılır
```

Örnek plugin path'leri:

```text
HelloPlugin\bin\Debug\net8.0\HelloPlugin.dll

GithubPlugin\bin\Debug\net8.0\GithubPlugin.dll
```

## 🚀 Çalıştırma

Repository'yi klonlayın:

```bash
git clone https://github.com/ufukgulec/Console-Plugin-Example.git

cd Console-Plugin-Example
```

Solution'ı restore edin:

```bash
dotnet restore
```

Build:

```bash
dotnet build
```

Ardından `AppWithPlugin` projesini çalıştırın:

```bash
dotnet run --project AppWithPlugin
```

Plugin DLL'lerinin önce build edilmiş olması gerekir.

## Ekran Görüntüleri
![image](https://github.com/ufukgulec/Console-Plugin-Example/assets/51711890/349b8cd4-ebfa-48f5-9d0d-5f2633ba739a)j
![image](https://github.com/ufukgulec/Console-Plugin-Example/assets/51711890/8791ee8d-acb3-43b5-a616-eb1215451a7a)
![image](https://github.com/ufukgulec/Console-Plugin-Example/assets/51711890/214bebd8-f0f0-4c08-942c-cd81995d892f)
![image](https://github.com/ufukgulec/Console-Plugin-Example/assets/51711890/1cc0aacd-d32a-4094-91a8-65a398229def) 

## 📋 Kullanım Senaryosu

Uygulama çalıştırıldığında plugin'ler yüklenerek listelenebilir.

Örneğin:

```text
Available Plugins:

1 - HelloPlugin
2 - GithubPlugin

Select plugin:
```

Plugin seçildiğinde ilgili plugin'in `Execute()` metodu çalıştırılır.

Bu sayede ana uygulamaya yeni bir plugin eklemek için host uygulamanın kodunu değiştirmek gerekmez.

## 🔐 Dependency Isolation

Plugin mimarisinin önemli noktalarından biri dependency isolation'dır.

Normal bir uygulamada:

```text
Application
    │
    ├── Library A v1
    └── Library B v1
```

Plugin mimarisinde ise plugin'lerin kendi dependency context'leri olabilir:

```text
Application
│
├── Plugin A
│    └── Library X v1
│
└── Plugin B
     └── Library X v2
```

`AssemblyLoadContext` ile oluşturulan ayrı loading context'leri, bu tür senaryolarda assembly'lerin birbirinden izole edilmesine yardımcı olur.

## 💡 Bu Yaklaşım Nerelerde Kullanılabilir?

Plugin mimarisi özellikle aşağıdaki sistemlerde kullanılabilir:

* Modüler masaüstü uygulamaları
* ERP uygulamaları
* Workflow platformları
* CMS sistemleri
* IDE ve developer tools
* Raporlama sistemleri
* Kurumsal uygulamalara modül ekleme
* Integration platformları
* CLI uygulamalarına komut ekleme
* Third-party extension sistemleri

## 🔮 Geliştirme Fikirleri

Projeyi daha kapsamlı bir plugin framework'üne dönüştürmek için:

* [ ] Plugin metadata sistemi
* [ ] Plugin versioning
* [ ] Plugin enable / disable
* [ ] Plugin lifecycle yönetimi
* [ ] Plugin dependency management
* [ ] Plugin configuration
* [ ] Plugin logging
* [ ] Plugin manifest
* [ ] Plugin hot reload
* [ ] Plugin unload
* [ ] Plugin permission model
* [ ] NuGet tabanlı plugin dağıtımı
* [ ] Plugin repository / marketplace

## 📚 Referans

Projenin temel yaklaşımı Microsoft'un .NET'teki **Plugin / AssemblyLoadContext** yaklaşımından yararlanılarak hazırlanmıştır.

## 📄 License

Bu repository, .NET plugin mimarisini ve runtime assembly loading yaklaşımını incelemek ve örneklemek amacıyla oluşturulmuştur.

## 👤 Author

**Ufuk Güleç**

* GitHub: https://github.com/ufukgulec
* Portfolio: https://ufukgulec.github.io/

```
```
