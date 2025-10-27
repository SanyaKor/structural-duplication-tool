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
    private int _duplications;
    private int _filesprocessed;
    
    public ParamsDuplicator(string inputpath, string outputath)
    {
        if (!Directory.Exists(inputpath))
        {
            log.error($"Input directory not found: {inputpath}. Exiting...");
            return;
        }
        
        log.info($"[ParamsDuplicator] Input directory found: {inputpath}");
        
        if (!Directory.Exists(outputath))
        {
            log.error($"Output directory not found: {outputath}. Exiting...");
            return;
        }
        
        log.info($"[ParamsDuplicator] Output directory found: {outputath}");
        
        this._inputpath = inputpath;
        this._outputath = outputath;
        
        this._duplications = 0;
        this._filesprocessed = 0;
    }
    

    public void Run()
    {
        log.info($"[ParamsDuplicator] Running in directory: {this._inputpath}");
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
                log.error($"[ParamsDuplicator] Read error: {file} :: {ex.Message}. Exiting ...");
                return;
            }
            try
            {
                string relativePath = Path.GetRelativePath(this._inputpath, file);
                string resultPath = Path.Combine(this._outputath, relativePath);

                log.debug($"[ParamsDuplicator] Processing file: {resultPath}");
                
                updatedRoot = DuplicateParams(code, resultPath);
                
                Directory.CreateDirectory(Path.GetDirectoryName(resultPath)!);
                File.WriteAllText(resultPath, updatedRoot);
                
                this._filesprocessed++;
                log.debug($"[ParamsDuplicator] Saved updated file: {resultPath}");
            }
            catch(Exception ex)
            {
                log.error($"[ParamsDuplicator] Error: {file} :: {ex.Message}. Exiting ...");
                return;
            }
        }
        log.info($"[ParamsDuplicator] Saved updated files to directory: {this._outputath}");
        log.info($"[ParamsDuplicator] Files processed: {this._filesprocessed}");
        log.info($"[ParamsDuplicator] Methods modified: {this._duplications}");
        log.info($"[ParamsDuplicator] Parameters duplicated: {this._duplications}");
    }

    public async Task RunAsync()
    {
        log.info($"[ParamsDuplicator] Running in directory: {this._inputpath}");

        var files = Directory.EnumerateFiles(this._inputpath, "*.cs", SearchOption.AllDirectories);

        await Parallel.ForEachAsync(files, async (filePath, _) =>
        {
            string code, updatedRoot;
            try
            {
                code = await File.ReadAllTextAsync(filePath, _);
            }
            catch(Exception ex)
            {
                log.error($"[ParamsDuplicator] Read error: {filePath} :: {ex.Message}. Exiting ...");
                return;
            }

            try
            {
                string relativePath = Path.GetRelativePath(this._inputpath, filePath);
                string resultPath = Path.Combine(this._outputath, relativePath);
                
                log.debug($"[ParamsDuplicator] Processing file: {resultPath}");

                updatedRoot = DuplicateParams(code,  resultPath);

                Directory.CreateDirectory(Path.GetDirectoryName(resultPath)!);
                await File.WriteAllTextAsync(resultPath, updatedRoot, _);
                
                this._filesprocessed++;
                log.debug($"[ParamsDuplicator] Saved updated file: {resultPath}");
            }
            catch(Exception ex)
            {
                log.error($"[ParamsDuplicator] Error: {filePath} :: {ex.Message}. Exiting ...");
                return;
            }
        });
        log.info($"[ParamsDuplicator] Saved updated files to directory: {this._outputath}");
        log.info($"[ParamsDuplicator] Files processed: {this._filesprocessed}");
        log.info($"[ParamsDuplicator] Methods modified: {this._duplications}");
        log.info($"[ParamsDuplicator] Parameters duplicated: {this._duplications}");
    }

    private string DuplicateParams(string code,  string filePath)
    {
        var tree = CSharpSyntaxTree.ParseText(code, path: filePath);
        var root = tree.GetRoot();
        
        log.info($"[ParamsDuplicator] Running in {Path.GetFileName(tree.FilePath)} ");
        
        var methods = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>();
        
        var fileteredMethods = methods.Where(m => m.ParameterList.Parameters.Count == 1)
            .ToList();

        root = root.TrackNodes(fileteredMethods);
        int counter = 0;
        
        foreach (var m in fileteredMethods)
        {
            counter++;
            log.debug($"[ParamsDuplicator] Running in method {m.Identifier.Text}({m.ParameterList.Parameters.Count} param(s))");
            var currentMethod = root.GetCurrentNode(m)!;
            
            var methodParams= currentMethod.ParameterList.Parameters[0];
            var dup= methodParams.WithIdentifier(Identifier(methodParams.Identifier.ValueText + "_duplicate"));

            var updated = currentMethod
                .AddParameterListParameters(dup)
                .WithLeadingTrivia(
                    TriviaList(
                        Comment("\n\t// duplicated parameter added"),
                        ElasticCarriageReturnLineFeed));

            root = root.ReplaceNode(currentMethod, updated);
            log.debug($"[ParamsDuplicator] Successfully duplicated params in method: {m.Identifier.Text}");
        }
        
        log.info($"[ParamsDuplicator] Finished processing {Path.GetFileName(tree.FilePath)} — {counter} methods duplicated");
        this._duplications += counter;
        
        return root.NormalizeWhitespace().ToFullString();
    }
}