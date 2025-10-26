using System;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace StructuralDuplication;

public class TestsGen
{
    private string outputpath;
    
    public TestsGen(string outputpath)
    {
        this.outputpath = outputpath;
    }
    
    public void GenerateClass(string filename, string className)
    {
        filename = $"{className}.cs";
        
        List<string> returnTypes = new()
        {
            "int",
            "string",
            "bool",
            "double",
            "float",
            "MyClass"
        };
        
        List<string> modifiers = new() { "ref", "out", "in" };
        List<string> containers = new() { "List", "Dictionary" };
        
        var nullableReturnTypes = returnTypes
            .Select(t => t + "?")
            .ToList();
        
        returnTypes.AddRange(nullableReturnTypes);
        
        List<string> paramsList = new List<string>();
        
        foreach (var type in returnTypes)
        {
            paramsList.Add(type);

            foreach (var c in containers)
            {
                if (c == "List")
                    paramsList.Add($"{c}<{type}>");
                else if (c == "Dictionary")
                    paramsList.Add($"{c}<string, {type}>");
            }

            foreach (var mod in modifiers)
            {
                paramsList.Add($"{mod} {type}");
                foreach (var c in containers)
                {
                    if (c == "List")
                        paramsList.Add($"{mod} {c}<{type}>");
                    else if (c == "Dictionary")
                        paramsList.Add($"{mod} {c}<string, {type}>");
                }
            }
        }
        
        List<MethodDeclarationSyntax> methods = new List<MethodDeclarationSyntax>();
        
        int methodCount = 1;
        
        foreach (var param in paramsList)
        {
            MethodDeclarationSyntax newMethod = CreateMethod("test_" + methodCount.ToString(), "void",
                (param, "param1"));
            
            methodCount++;
            methods.Add(newMethod);
        }
        
        var cls = ClassDeclaration(className)
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddMembers([..methods]) ;
        
        
        var root = CompilationUnit().AddMembers(cls).NormalizeWhitespace();
        
        File.WriteAllText(this.outputpath + filename, root.ToFullString());
    }

    
    private MethodDeclarationSyntax CreateMethod(string name, string returnType = "void", params (string Type, string Name)[] parameters)
    {
        var parameterList = new SeparatedSyntaxList<ParameterSyntax>();
        foreach (var (type, paramName) in parameters)
        {
            parameterList = parameterList.Add(Parameter(Identifier(paramName))
                    .WithType(ParseTypeName(type))
            );
        }

        var method = MethodDeclaration(ParseTypeName(returnType), Identifier(name))
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .WithParameterList(ParameterList(parameterList))
            .WithBody(Block());

        return method;
    }
    
}