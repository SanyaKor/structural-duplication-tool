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
        
        Dictionary<string, string> fileContents = ReadFiles();
        int fixes = 0;
        
        log.info($"Running ParamsDuplicator");
        
        foreach (var file in fileContents)
        {
            var tree = CSharpSyntaxTree.ParseText(file.Value, path: this._outputath + file.Key);
            var root = tree.GetRoot();
            var methods = root.DescendantNodes().OfType<MethodDeclarationSyntax>();
            
            root = root.TrackNodes(methods);
            
            foreach (var m in methods)
            {
                if (m.ParameterList.Parameters.Count == 1)
                {
                    var currentMethod = root.GetCurrentNode(m)!;
                    var firstParam = currentMethod.ParameterList.Parameters.First();
                    var duplicatedParam = firstParam
                        .WithIdentifier(Identifier(firstParam.Identifier.Text + "_duplicate"));

                    var comment = SyntaxFactory.Comment("\n// duplicated parameter added");

                    var newMethod = currentMethod.WithLeadingTrivia(
                        TriviaList(comment, ElasticCarriageReturnLineFeed)
                    );
                    
                    root = root.ReplaceNode(currentMethod, newMethod);
                    fixes++;
                    log.debug($"Duplicating in {currentMethod.Identifier.ValueText}, file {file.Key}");
                }
            }
            
            fileContents[file.Key] = root.NormalizeWhitespace().ToFullString();
        }
        log.info($"Duplication completed: [Duplicated {fixes} params]");
        
        WriteToFiles(this._outputath, fileContents);
    }
    
    
    private async Task<Dictionary<string, string>> ReadFilesAsync()
    {
        Dictionary<string, string> results = new Dictionary<string, string>();
        log.debug($"Reading files in {this._inputpath} async ...");
        IEnumerable<string> files = Directory.EnumerateFiles(this._inputpath, "*.cs", SearchOption.AllDirectories);

        await Parallel.ForEachAsync(files, async (file, _) =>
        {
            var (path, content) = await ReadFileAsync(file);
            if (content.Length > 0)
            {
                lock (results)
                {
                    results[Path.GetRelativePath(path,file )] = content;
                }
            }
        });
        return results;
    }
    private Dictionary<string, string> ReadFiles()
    {
        
        Dictionary<string, string> results = new Dictionary<string, string>();
        
        IEnumerable<string> files = Directory.EnumerateFiles(this._inputpath, "*.cs", SearchOption.AllDirectories);
        
        log.info($"Reading {files.Count()} files  {this._inputpath} ...");
        
        foreach (string file in files)
        {
            string content =  ReadFile(file);
            results[Path.GetRelativePath(this._inputpath,file )] = content;
        }

        return results;
    }
    
    private async Task WriteToFilesAsync(
        string directory,
        Dictionary<string, string> fileContents,
        bool append = false)
    {
        Directory.CreateDirectory(directory);

        var tasks = new List<Task>();
        log.info($"Writing {fileContents.Count} files to {directory} async ...");
        foreach (var kv in fileContents)
        {
            var fileName = kv.Key;
            var content = kv.Value;
            var path = Path.Combine(directory, fileName);
            
            var destPath = Path.Combine(directory, fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);
            
            Task t = append
                ? File.AppendAllTextAsync(path, content)
                : File.WriteAllTextAsync(path, content);

            tasks.Add(t);
        }

        await Task.WhenAll(tasks);
    }
    
    private void WriteToFiles(
        string directory,
        Dictionary<string, string> fileContents,
        bool append = false)
    {
        
        log.info($"Writing {fileContents.Count} files to {directory} ...");
        Directory.CreateDirectory(directory);
        
        foreach (var kv in fileContents)
        {
            var sourcePath = kv.Key;
            var content = kv.Value;
            
            var destPath = Path.Combine(directory, sourcePath);

            Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);

            if (append)
                File.AppendAllText(destPath, content);
            else
                File.WriteAllText(destPath, content);
            
        }
    }
    
    
    
    private async Task WriteToFileAsync(string directory, string fileName, string content, bool append = false)
    {
        log.debug($"Writing file {fileName} to to {directory} async ...");
        
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, fileName);

        if (append)
            await File.AppendAllTextAsync(path, content);
        else
            await File.WriteAllTextAsync(path, content);
    }
    
    private void WriteToFile(string directory, string fileName, string content, bool append = false)
    {
        log.debug($"Writing file {fileName} ...");

        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, fileName);

        if (append)
            File.AppendAllText(path, content);
        else
            File.WriteAllText(path, content);
    }
    
    private async Task<(string path, string content)> ReadFileAsync(string path)
    {
        log.debug($"Reading file async {path}");
        try
        {
            string content = await File.ReadAllTextAsync(path);
            return (path, content);
        }
        catch (Exception ex)
        {
            log.error($"Read error: {path} :: {ex.Message}");
            return (path, string.Empty);
        }
    }
    private string ReadFile(string path)
    {
        log.debug($"Reading file {path}");
        try
        {
            string content = File.ReadAllText(path);
            return content;
        }
        catch (Exception ex)
        {
            log.error($"Read error: {path} :: {ex.Message}");
            return string.Empty;
        }
    }
}