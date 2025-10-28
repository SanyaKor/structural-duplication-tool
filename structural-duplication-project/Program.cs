using System.Diagnostics;

namespace StructuralDuplication;
class Program
{
    static void Main(string[] args)
    {   
        log.setup(minimum: LogLevel.Info);
        
        Dictionary<string, string> flags = ParseArgs(args);
        
        string projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"../../../.."));
        
        string inputpath = flags.GetValueOrDefault("--input", $"{projectRoot}" + "/testing/Samples");
        string outputpath = flags.GetValueOrDefault("--output", $"{projectRoot}" + "/testing/Results");
        
        bool all = flags.ContainsKey("--all");
    
        
        
        /*
        TestsGen tg = new TestsGen(inputpath);
        tg.GenerateClass("test_gen_class_normal","test_gen_class_normal", TestsGen.ClassKind.Normal);
        */
        
        ParamsDuplicator duplicator = new ParamsDuplicator(inputpath, outputpath);
        duplicator.Run(true);
    }
    
    static Dictionary<string, string> ParseArgs(string[] args)
    {
        Dictionary<string, string> dict = new Dictionary<string, string>();
        
        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];
            
            if (arg.StartsWith("--"))
            {
                string value = (i + 1 < args.Length && !args[i + 1].StartsWith("--"))
                    ? args[i + 1]
                    : "true";
                
                dict[arg] = value;
            }
        }
        return dict;
    }
    
}