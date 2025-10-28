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
    
    public void GenerateClass(string filename, string className,  ClassKind kind)
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
        
        var arrayTypes = returnTypes
            .Select(t => t + "[]")
            .ToList();
        
        returnTypes.AddRange(arrayTypes);

        
        
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
        
        
        var nullableReturnTypes = paramsList
            .Select(t => t + "?")
            .ToList();
        
        paramsList.AddRange(nullableReturnTypes);
        
        List<MethodDeclarationSyntax> methods = new List<MethodDeclarationSyntax>();
        
        MethodDeclarationSyntax methodNoParam = CreateMethod("test_0", "void");

        methods.Add(methodNoParam);
        
        int methodCount = 1;
        
        foreach (var param in paramsList)
        {
            MethodDeclarationSyntax methodSingleParam = CreateMethod(
                "test_" + methodCount.ToString(), "void",
                (param, "param1"));
            
            methodCount++;
            methods.Add(methodSingleParam);
            
            MethodDeclarationSyntax methodMultipleParam = CreateMethod(
                "test_" + methodCount.ToString(), "void",
                (param, "param1"),
                (param, "param2"));
            
            methodCount++;
            methods.Add(methodMultipleParam);
        }
        
        
        
        
        var cls = ClassDeclaration(className)
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddMembers([..methods]) ;
        
        switch (kind)
        {
            case ClassKind.Abstract:
                cls = cls.AddModifiers(Token(SyntaxKind.AbstractKeyword));
                break;

            case ClassKind.Sealed:
                cls = cls.AddModifiers(Token(SyntaxKind.SealedKeyword));
                break;

            case ClassKind.Static:
                cls = cls.AddModifiers(Token(SyntaxKind.StaticKeyword));
                break;

            case ClassKind.Partial:
                cls = cls.AddModifiers(Token(SyntaxKind.PartialKeyword));
                break;
        }

        
        var root = CompilationUnit().AddMembers(cls).NormalizeWhitespace();
        
        File.WriteAllText(this.outputpath + filename, root.ToFullString());
    }

    
    private MethodDeclarationSyntax CreateMethod(string name, string returnType = "void", params (string Type, string Name)[] parameters)
    {
        var parameterList = new SeparatedSyntaxList<ParameterSyntax>();
        
        foreach (var (type, paramName) in parameters)
        {
            if (!string.IsNullOrWhiteSpace(type) && !string.IsNullOrWhiteSpace(paramName))
            {
                parameterList = parameterList.Add(
                    Parameter(Identifier(paramName))
                        .WithType(ParseTypeName(type))
                );
            }
        }

        var method = MethodDeclaration(ParseTypeName(returnType), Identifier(name))
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .WithParameterList(ParameterList(parameterList))
            .WithBody(Block());

        return method;
    }
    
    public enum ClassKind
    {
        Normal,
        Abstract,
        Sealed,
        Static,
        Partial,
    }
    
}