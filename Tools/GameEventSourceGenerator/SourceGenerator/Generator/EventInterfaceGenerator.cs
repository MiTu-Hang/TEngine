﻿using Microsoft.CodeAnalysis;  
using Microsoft.CodeAnalysis.CSharp;  
using Microsoft.CodeAnalysis.CSharp.Syntax;  
using System.Collections.Generic;  
using System.Linq;  
using System.Text;
using Analyzer;

[Generator]  
public class EventInterfaceGenerator : ISourceGenerator  
{  
    public void Initialize(GeneratorInitializationContext context)  
    {  
        // 可以在这里进行初始化  
    }  

    public void Execute(GeneratorExecutionContext context)  
    {  
        // 获取当前语法树  
        var syntaxTrees = context.Compilation.SyntaxTrees;  
        
        List<string> classNameList = new List<string>();

        foreach (var tree in syntaxTrees)  
        {  
            var root = tree.GetRoot();  
            var interfaces = root.DescendantNodes()  
                .OfType<InterfaceDeclarationSyntax>()  
                .Where(i => i.AttributeLists.Count > 0 &&   
                            i.AttributeLists  
                             .Any(a => a.Attributes  
                             .Any(attr => attr.Name.ToString() == $"{Definition.EventInterface}")));  
           
            foreach (var interfaceNode in interfaces)  
            {  
                var interfaceName = interfaceNode.Identifier.ToString();  
                var fullName = interfaceNode.SyntaxTree.GetRoot()
                    .DescendantNodes()
                    .OfType<NamespaceDeclarationSyntax>()
                    .Select(ns => ns.Name.ToString())
                    .Concat(new[] { interfaceName })
                    .Aggregate((a, b) => a + "." + b);
                var eventClassName = $"{interfaceName}_Event";  
                var eventClassCode = GenerateEventClass(interfaceName, eventClassName, interfaceNode);  

                context.AddSource($"{eventClassName}.g.cs", eventClassCode);  

                // 生成实现类  
                var implementationClassCode = GenerateImplementationClass(fullName, interfaceName, interfaceNode,context);  
                context.AddSource($"{interfaceName}_Gen.g.cs", implementationClassCode);  
                
                
                classNameList.Add($"{interfaceName}_Gen");
            }  
        }  
        
        string uniqueFileName = $"GameEventHelper.g.cs";
        context.AddSource(uniqueFileName, GenerateGameEventHelper(classNameList));
    }  
    
    private string GenerateGameEventHelper(List<string> classNameList)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"//------------------------------------------------------------------------------");
        sb.AppendLine($"//  <auto-generated>");
        sb.AppendLine($"//     This code was generated by autoBindTool.");
        sb.AppendLine($"//     Changes to this file may cause incorrect behavior and will be lost if");
        sb.AppendLine($"//     the code is regenerated.");
        sb.AppendLine($"//  </auto-generated>");
        sb.AppendLine($"//------------------------------------------------------------------------------");
        sb.AppendLine();
        sb.AppendLine($"using UnityEngine;");
        sb.AppendLine($"using UnityEngine.UI;");
        sb.AppendLine($"using {Definition.FrameworkNameSpace};");
        sb.AppendLine();
        sb.AppendLine($"namespace {Definition.NameSpace}");
        sb.AppendLine($"{{");
        sb.AppendLine($"    public static class GameEventHelper");
        sb.AppendLine("    {");
        sb.AppendLine($"        public static void Init()");
        sb.AppendLine("        {");

        foreach (var className in classNameList)
        {
            sb.AppendLine($"            var m_{className} = new {className}(GameEvent.EventMgr.GetDispatcher());");
        }

        sb.AppendLine("        }");


        sb.AppendLine("    }");
        sb.AppendLine("}");
        return sb.ToString();
    }

    private string GenerateEventClass(string interfaceName, string className, InterfaceDeclarationSyntax interfaceNode)  
    {  
        var methods = interfaceNode.Members.OfType<MethodDeclarationSyntax>();  
        var sb = new StringBuilder();  

        sb.AppendLine($"//------------------------------------------------------------------------------");  
        sb.AppendLine($"//	<auto-generated>");  
        sb.AppendLine($"//		This code was generated by autoBindTool.");  
        sb.AppendLine($"//		Changes to this file may cause incorrect behavior and will be lost if");  
        sb.AppendLine($"//		the code is regenerated.");  
        sb.AppendLine($"//	</auto-generated>");  
        sb.AppendLine($"//------------------------------------------------------------------------------");  
        sb.AppendLine();  
        sb.AppendLine($"using UnityEngine;");  
        sb.AppendLine($"using UnityEngine.UI;");  
        sb.AppendLine($"using {Definition.FrameworkNameSpace};");  
        sb.AppendLine();  
        sb.AppendLine($"namespace {Definition.NameSpace}");  
        sb.AppendLine("{");  
        sb.AppendLine($"    public partial class {className}");  
        sb.AppendLine("    {");  

        foreach (var method in methods)  
        {  
            var methodName = method.Identifier.ToString();  
            var parameters = string.Join(", ", method.ParameterList.Parameters.Select(p => $"{p.Type} {p.Identifier}"));  
            sb.AppendLine($"        public static readonly int {methodName} = {Definition.StringToHash}(\"{className}.{methodName}\");");  
        }  

        sb.AppendLine("    }");  
        sb.AppendLine("}");  
        return sb.ToString();  
    }  

    private string GenerateImplementationClass(string interfaceFullName, string interfaceName, InterfaceDeclarationSyntax interfaceNode,GeneratorExecutionContext context)  
    {  
        var semanticModel = context.Compilation.GetSemanticModel(interfaceNode.SyntaxTree);
        var sb = new StringBuilder();  

        sb.AppendLine($"//------------------------------------------------------------------------------");  
        sb.AppendLine($"//	<auto-generated>");  
        sb.AppendLine($"//		This code was generated by autoBindTool.");  
        sb.AppendLine($"//		Changes to this file may cause incorrect behavior and will be lost if");  
        sb.AppendLine($"//		the code is regenerated.");  
        sb.AppendLine($"//	</auto-generated>");  
        sb.AppendLine($"//------------------------------------------------------------------------------");  
        sb.AppendLine();  
        sb.AppendLine($"using UnityEngine;");  
        sb.AppendLine($"using UnityEngine.UI;");  
        sb.AppendLine($"using {Definition.FrameworkNameSpace};");  
        sb.AppendLine();  
        sb.AppendLine($"namespace {Definition.NameSpace}");  
        sb.AppendLine($"{{");  
        sb.AppendLine($"    public partial class {interfaceName}_Gen : {interfaceName}");  
        sb.AppendLine("    {");  
        sb.AppendLine("        private EventDispatcher _dispatcher;");  
        sb.AppendLine($"        public {interfaceName}_Gen(EventDispatcher dispatcher)");  
        sb.AppendLine("        {");  
        sb.AppendLine("            _dispatcher = dispatcher;");  
        sb.AppendLine($"             GameEvent.EventMgr.RegWrapInterface(\"{interfaceFullName}\", this);");
        sb.AppendLine("        }");  

        foreach (var method in interfaceNode.Members.OfType<MethodDeclarationSyntax>())  
        {  
            var methodName = method.Identifier.ToString();  
            var parameters = GenerateParameters(method, semanticModel);

            sb.AppendLine($"        public void {methodName}({parameters})");  
            sb.AppendLine("        {");  
            if (method.ParameterList.Parameters.Count > 0)  
            {  
                var paramNames = string.Join(", ", method.ParameterList.Parameters.Select(p => p.Identifier.ToString()));  
                sb.AppendLine($"            _dispatcher.Send({interfaceName}_Event.{methodName}, {paramNames});");  
            }  
            else  
            {  
                sb.AppendLine($"            _dispatcher.Send({interfaceName}_Event.{methodName});");  
            }  
            sb.AppendLine("        }");  
        }  

        sb.AppendLine("    }");  
        sb.AppendLine("}");  
        return sb.ToString();  
    }  

    private string GenerateParameters(MethodDeclarationSyntax method, SemanticModel semanticModel)
    {
        return string.Join(", ", method.ParameterList.Parameters.Select(p => 
        {
            var typeSymbol = semanticModel.GetTypeInfo(p.Type).Type;
            return typeSymbol != null 
                ? $"{typeSymbol.ToDisplayString()} {p.Identifier}"
                : $"{p.Type} {p.Identifier}";
        }));
    }
}