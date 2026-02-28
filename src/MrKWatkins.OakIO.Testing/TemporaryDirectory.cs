namespace MrKWatkins.OakIO.Testing;

public sealed class TemporaryDirectory : IDisposable
{
    private readonly string directory;

    private TemporaryDirectory(string directory)
    {
        this.directory = directory;
    }

    [Pure]
    public string GetFilePath(string filename) => Path.Combine(directory, filename);

    [MustUseReturnValue]
    [MustDisposeResource]
    public static TemporaryDirectory Create()
    {
        var path = Path.GetTempFileName();
        File.Delete(path);
        Directory.CreateDirectory(path);
        return new TemporaryDirectory(path);
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(directory, true);
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Exception deleting temporary directory {directory}: {exception}");
        }
    }
}