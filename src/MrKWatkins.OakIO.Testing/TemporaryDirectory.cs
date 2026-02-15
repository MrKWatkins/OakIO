namespace MrKWatkins.OakIO.Testing;

public sealed class TemporaryDirectory : IDisposable
{
    private TemporaryDirectory(string directory)
    {
        Directory = directory;
    }

    public string Directory { get; }

    [Pure]
    public string GetFilePath(string filename) => Path.Combine(Directory, filename);

    [MustUseReturnValue]
    [MustDisposeResource]
    public static TemporaryDirectory Create()
    {
        var path = Path.GetTempFileName();
        File.Delete(path);
        System.IO.Directory.CreateDirectory(path);
        return new TemporaryDirectory(path);
    }

    public void Dispose()
    {
        try
        {
            System.IO.Directory.Delete(Directory, true);
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Exception deleting temporary directory {Directory}: {exception}");
        }
    }
}