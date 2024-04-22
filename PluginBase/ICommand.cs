namespace PluginBase
{
    public interface ICommand
    {
        string Name { get; }
        string Message { get; }

        int Execute();
    }
}
