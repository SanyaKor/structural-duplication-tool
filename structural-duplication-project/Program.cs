using Serilog;

namespace StructuralDuplication;
class Program
{
    static void Main(string[] args)
    {   
        log.setup(minimum: LogLevel.Info);
        
        if (args.Length == 0)
        {
            log.error("Usage: dotnet run <path_to_file_or_directory>. Exiting..");
            return;
        }
        string path = args[0];

        if (File.Exists(path))
        {
            log.info("File found: {0}", Path.GetFullPath(path));
        }
        else if (Directory.Exists(path))
        {
            log.info($"Directory found {0}: {Path.GetFullPath(path)}");
        }
        else
        {
            log.error("Path not found: {0}", path);
        }
    }
}