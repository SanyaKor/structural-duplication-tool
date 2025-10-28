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
    private int _methodsprocessed;
    private int _methodsskipped;
    
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
        this._methodsprocessed = 0;
        this._methodsskipped = 0;
    }
    

    public void Run(bool duplicateAllParams = false )
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
                
                updatedRoot = DuplicateParams(code, resultPath, duplicateAllParams);
                
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
        log.info($"[ParamsDuplicator] Methods modified/skipped: [{this._methodsprocessed}/{this._methodsskipped}]");
        log.info($"[ParamsDuplicator] Parameters duplicated: {this._duplications}");
    }

    public async Task RunAsync(bool duplicateAllParams = false)
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

                updatedRoot = DuplicateParams(code,  resultPath, duplicateAllParams);

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
        log.info($"[ParamsDuplicator] Methods modified/skipped: [{this._methodsprocessed}/{this._methodsskipped}]");
        log.info($"[ParamsDuplicator] Parameters duplicated: {this._duplications}");
    }

    private string DuplicateParams(string code,  string filePath, bool duplicateAllParams = false)
    {
        int paramsDublicated = 0;
        int methodsProcessed = 0;
        int methodsSkipped = 0;
        
        var tree = CSharpSyntaxTree.ParseText(code, path: filePath);
        var root = tree.GetRoot();
        
        log.info($"[ParamsDuplicator] Running in {Path.GetFileName(tree.FilePath)} ");
        
        var methods = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>();
        
        methodsSkipped = methods.Count();
        
        methods = methods.Where(m => m.ParameterList.Parameters.Count > 0)
            .ToList();
        
        
        if(!duplicateAllParams) 
            methods = methods.Where(m => m.ParameterList.Parameters.Count == 1)
            .ToList();
        
        root = root.TrackNodes(methods);
        
        methodsSkipped -= methods.Count();
        
        foreach (var m in methods)
        {
            methodsProcessed++;
            log.debug($"[ParamsDuplicator] Running in method {m.Identifier.Text}({m.ParameterList.Parameters.Count} param(s))");
            
            MethodDeclarationSyntax currentMethod = root.GetCurrentNode(m)!;
            MethodDeclarationSyntax updatedMethod = currentMethod;
            
            foreach (var parameter in currentMethod.ParameterList.Parameters)
            {
                var duplicatedParam = parameter.WithIdentifier(Identifier(parameter.Identifier.ValueText + "_duplicate"));
                updatedMethod = updatedMethod.AddParameterListParameters(duplicatedParam);
                paramsDublicated++;
            }
            
            updatedMethod = updatedMethod.WithLeadingTrivia(
                TriviaList(Comment("\n\t// duplicated parameter added"), ElasticCarriageReturnLineFeed));
            
            root = root.ReplaceNode(currentMethod, updatedMethod);
            log.debug($"[ParamsDuplicator] Successfully duplicated params in method: {m.Identifier.Text}");
        }
        
        log.info($"[ParamsDuplicator] Finished processing {Path.GetFileName(tree.FilePath)} — [{methodsProcessed}/{methodsSkipped}] methods processed/skipped, {paramsDublicated} params duplicated");
        
        this._duplications += paramsDublicated;
        this._methodsprocessed += methodsProcessed;
        this._methodsskipped += methodsSkipped;
        
        return root.NormalizeWhitespace().ToFullString();
    }
}