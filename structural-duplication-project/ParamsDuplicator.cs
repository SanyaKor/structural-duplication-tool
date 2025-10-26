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

    public ParamsDuplicator(string inputpath, string outputath)
    {
        if (!Directory.Exists(inputpath))
        {
            log.error($"Directory not found: {inputpath}. Exiting...");
            return;
        }
        if (!Directory.Exists(outputath))
        {
            log.error($"Directory not found: {outputath}. Exiting...");
            return;
        }
        this._inputpath = inputpath;
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
                log.error($"Read error: {file} :: {ex.Message}. Exiting ...");
                return;
            }
            try
            {
                updatedRoot = DuplicateParams(code);
                string relativePath = Path.GetRelativePath(this._inputpath, file);
                string resultPath = Path.Combine(this._outputath, relativePath);
                log.info($"file: {resultPath}");

                Directory.CreateDirectory(Path.GetDirectoryName(resultPath)!);
                File.WriteAllText(resultPath, updatedRoot);
            }
            catch(Exception ex)
            {
                log.error($"Duplication error: {file} :: {ex.Message}. Exiting ...");
                return;
            }
        }
    }

    public async Task RunAsync()
    {
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
                log.error($"Read error: {filePath} :: {ex.Message}. Exiting ...");
                return;
            }

            try
            {
                updatedRoot = DuplicateParams(code);
                string relativePath = Path.GetRelativePath(this._inputpath, filePath);
                string resultPath = Path.Combine(this._outputath, relativePath);
                log.info($"file: {resultPath}");

                Directory.CreateDirectory(Path.GetDirectoryName(resultPath)!);
                await File.WriteAllTextAsync(resultPath, updatedRoot, _);
            }
            catch(Exception ex)
            {
                log.error($"Duplication error: {filePath} :: {ex.Message}. Exiting ...");
                return;
            }
        });
    }

    private static string DuplicateParams(string code)
    {
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();

        var methods = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>();
        
        var fileteredMethods = methods.Where(m => m.ParameterList.Parameters.Count == 1)
            .ToList();

        root = root.TrackNodes(fileteredMethods);

        foreach (var m in fileteredMethods)
        {
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
        }

        return root.NormalizeWhitespace().ToFullString();
    }
}