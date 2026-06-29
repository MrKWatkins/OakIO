namespace MrKWatkins.OakIO.Testing;

public sealed class TemporaryDirectory : IDisposable
{
    private TemporaryDirectory(string path)
    {
        Path = path;
    }

    public string Path { get; }

    [Pure]
    public string GetFilePath(string filename) => System.IO.Path.Combine(Path, filename);

    [MustUseReturnValue]
    [MustDisposeResource]
    public static TemporaryDirectory Create()
    {
        var path = System.IO.Path.GetTempFileName();
        File.Delete(path);
        Directory.CreateDirectory(path);
        return new TemporaryDirectory(path);
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(Path, true);
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Exception deleting temporary directory {Path}: {exception}");
        }
    }
}