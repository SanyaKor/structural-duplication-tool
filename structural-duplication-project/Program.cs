using System.Diagnostics;

namespace StructuralDuplication;
class Program
{
    static void Main(string[] args)
    {   
        log.setup(minimum: LogLevel.Info);
        
        if (args.Length == 0)
        {
            log.error("Usage: dotnet run <intput path> <output path>. Exiting..");
            return;
        }
        string inputpath = args[0];
        string outputpath = args[1];

        if (!File.Exists(inputpath) &&  !Directory.Exists(inputpath))
        {
            log.info("File found: ", Path.GetFullPath(inputpath));
        }
        else if (Directory.Exists(inputpath))
        {
            log.info($"Input Directory found: {Path.GetFullPath(inputpath)}");
        }
        else
        {
            log.error("Input Path not found: ", inputpath);
        }
        
        
        if (File.Exists(outputpath))
        {
            log.info("File found: ", Path.GetFullPath(outputpath));
        }
        else if (Directory.Exists(outputpath))
        {
            log.info($"Output Directory found: {Path.GetFullPath(outputpath)}");
        }
        else
        {
            log.error("Output Path not found: ", outputpath);
        }
        
        /*TestsGen tg = new TestsGen(inputpath);
        tg.GenerateClass("test_gen_class_normal","test_gen_class_normal", TestsGen.ClassKind.Normal);
        */
        
        ParamsDuplicator duplicator = new ParamsDuplicator(inputpath, outputpath);
        duplicator.Run();
    }
    
}