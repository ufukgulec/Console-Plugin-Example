using ICommand = PluginBase.ICommand;

namespace HelloPlugin
{
    public class HelloCommand : ICommand
    {
        public string Name { get => "Hello"; }
        public string Message { get => @$"{Name} açıklaması..."; }

        public int Execute()
        {
            Console.WriteLine(@$"{Name} plugini selamlıyor...");
            return 0;
        }
    }
}