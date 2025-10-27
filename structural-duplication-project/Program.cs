using System.Diagnostics;

namespace StructuralDuplication;
class Program
{
    static void Main(string[] args)
    {   
        log.setup(minimum: LogLevel.Info);
        
        if (args.Length == 0)
        {
            log.error("[CLI] Usage: dotnet run <intput path> <output path>. Exiting..");
            return;
        }
        string inputpath = args[0];
        string outputpath = args[1];
        
        /*TestsGen tg = new TestsGen(inputpath);
        tg.GenerateClass("test_gen_class_normal","test_gen_class_normal", TestsGen.ClassKind.Normal);
        */
        
        ParamsDuplicator duplicator = new ParamsDuplicator(inputpath, outputpath);
        duplicator.Run();
    }
    
}