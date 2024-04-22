using AppWithPlugin;
using PluginBase;
using System.Diagnostics;
using System.Reflection;
Dictionary<int, string> Events = new Dictionary<int, string>
{
    { 1, "Plugin Ekle" },
    { 2, "Plugin Listele" },
    { 3, "Plugin Çalıştır" },
    { 0, "Çıkış" },
};
List<ICommand> Plugins = new List<ICommand>();
try
{
    Start();

}
catch (Exception ex)
{
    Console.WriteLine(ex);
}

void AddPlugin()
{
    Console.WriteLine("----PLUGIN EKLEME----");
    while (true)
    {
        if (Plugins.Count() > 0)
            Console.WriteLine("Geri Dönmek için '0'");
        Console.Write("Plugin Path:");

        var input = Console.ReadLine();
        if (input != null)
        {
            if (input == "0")
            {
                Start();
                break;
            }
            else
            {
                Assembly pluginAssembly = LoadPlugin(input);
                foreach (var item in CreateCommands(pluginAssembly))
                {
                    Plugins.Add(item);
                }
            }
        }
        else AddPlugin();
    }
}

static Assembly LoadPlugin(string relativePath)
{
    // Navigate up to the solution root
    string root = Path.GetFullPath(Path.Combine(
        Path.GetDirectoryName(
            Path.GetDirectoryName(
                Path.GetDirectoryName(
                    Path.GetDirectoryName(
                        Path.GetDirectoryName(typeof(Program).Assembly.Location)))))));

    string pluginLocation = Path.GetFullPath(Path.Combine(root, relativePath.Replace('\\', Path.DirectorySeparatorChar)));
    //Console.WriteLine($"Loading commands from: {pluginLocation}");
    PluginLoadContext loadContext = new PluginLoadContext(pluginLocation);
    return loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(pluginLocation)));
}

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

void Start()
{
    Console.WriteLine("----PLUGIN MANAGER----");
    Console.WriteLine(string.Join("\n", Events.Select(e => { return $"{e.Key} - {e.Value}"; })));
    var Event = Console.ReadLine();

    switch (Event)
    {
        case "0":
            break;
        case "1":
            AddPlugin();
            break;
        case "2":
            ListPlugins();
            break;
        case "3":
            ExecutePlugin();
            break;
        default:
            Start();
            break;
    }
}

void ExecutePlugin()
{
    Console.WriteLine("----PLUGIN ÇALIŞTIR----");
    if (Plugins.Count() > 0)
    {
        Console.WriteLine("Geri Dönmek için '0'");

        Console.WriteLine(string.Join("\n", Plugins.Select((plugin, index) => { return $"Sıra: [{index}] -> Plugin Adı: [{plugin.Name}]"; })));
        int index = Convert.ToInt16(Console.ReadLine());
        Plugins[0].Execute();
    }
    else
        Console.WriteLine("Çalıştırılacak plugin bulunamadı!");

    Start();
}

void ListPlugins()
{
    Console.WriteLine("----PLUGIN LİSTELEME----");
    if (Plugins.Count() > 0)
    {
        Console.WriteLine(string.Join("\n", Plugins.Select((plugin, index) => { return $"Sıra: [{index}] -> Plugin Adı: [{plugin.Name}] -> Plugin Mesajı: [{plugin.Message}]"; })));
    }
    else
        Console.WriteLine("Ekli plugin bulunamadı!");
    Start();
}