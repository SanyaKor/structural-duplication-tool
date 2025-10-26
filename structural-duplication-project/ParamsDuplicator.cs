using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using System.Linq;

namespace StructuralDuplication;

public class ParamsDuplicator
{
    private string _inputpath;
    private string _outputath;

    public ParamsDuplicator(string intputpath, string outputath)
    {
        if (!Directory.Exists(intputpath))
        {
            log.error($"Directory not found: {intputpath}. Exiting...");
            return;
        }
        if (!Directory.Exists(outputath))
        {
            log.error($"Directory not found: {outputath}. Exiting...");
            return;
        }
        this._inputpath = intputpath;
        this._outputath = outputath;
    }
    

    public void Run()
    {
        var files = Directory.EnumerateFiles(this._inputpath, "*.cs", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            string code, updatedRoot;
            try
            {
                code = File.ReadAllText(file);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Read error: {file} :: {ex.Message}. Exiting ...");
                return;
            }
            try
            {
                string relativePath = Path.GetRelativePath(this._inputpath, file);
                string resultPath = Path.Combine(this._outputath, relativePath);
                log.info($"file: {resultPath}");

                Directory.CreateDirectory(Path.GetDirectoryName(resultPath)!);
                File.WriteAllText(resultPath, code);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Duplication error: {file} :: {ex.Message}. Exiting ...");
                return;
            }
        }
    }

    public async Task RunAsync()
    {
        var files = Directory.EnumerateFiles(this._inputpath, "*.cs", SearchOption.AllDirectories);
        var opts = new ParallelOptions { MaxDegreeOfParallelism = Math.Min(8, Environment.ProcessorCount) };
        
        await Parallel.ForEachAsync(files, opts, async (filePath, _) =>
        {
            string code, updatedRoot;
            try
            {
                code = await File.ReadAllTextAsync(filePath, _);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Read error: {filePath} :: {ex.Message}. Exiting ...");
                return;
            }

            try
            {
                //updatedRoot = DuplicateParams(code);
                string relativePath = Path.GetRelativePath(this._inputpath, filePath);
                string resultPath = Path.Combine(this._outputath, relativePath);
                log.info($"file: {resultPath}");

                Directory.CreateDirectory(Path.GetDirectoryName(resultPath)!);
                await File.WriteAllTextAsync(resultPath, code, _);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Duplication error: {filePath} :: {ex.Message}. Exiting ...");
                return;
            }
        });
    }
}