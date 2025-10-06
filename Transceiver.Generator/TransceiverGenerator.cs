// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text.Json;

namespace Transceiver.Generator;

[Generator(LanguageNames.CSharp)]
public class TransceiverGenerator : IIncrementalGenerator
{
    public static string GetMemberName(MemberDeclarationSyntax member)
    {
        return member switch
        {
            ClassDeclarationSyntax classDeclaration => classDeclaration.Identifier.Text,
            MethodDeclarationSyntax methodDeclaration => methodDeclaration.Identifier.Text,
            PropertyDeclarationSyntax propertyDeclaration => propertyDeclaration.Identifier.Text,
            FieldDeclarationSyntax fieldDeclaration => fieldDeclaration.Declaration.Variables.First().Identifier.Text,
            EventDeclarationSyntax eventDeclaration => eventDeclaration.Identifier.Text,
            _ => throw new NotSupportedException($"Unsupported member type: {member.GetType().Name}")
        };
    }

    private static IEnumerable<MemberDeclarationSyntax> GetTypeFromJsonElement(string key, JsonElement jsonElement)
    {
        if (jsonElement.ValueKind == JsonValueKind.String)
        {
            return [CreateProperty(key, nameof(String))];
        }
        else if (jsonElement.ValueKind == JsonValueKind.Number)
        {
            if (jsonElement.TryGetInt32(out _))
            {
                return [CreateProperty(key, nameof(Int32))];
            }
            else if (jsonElement.TryGetInt64(out _))
            {
                return [CreateProperty(key, nameof(Int64))];
            }
            else if (jsonElement.TryGetDecimal(out _))
            {
                return [CreateProperty(key, nameof(Decimal))];
            }
        }
        else if (jsonElement.ValueKind is JsonValueKind.True or JsonValueKind.False)
        {
            return [CreateProperty(key, nameof(Boolean))];
        }
        else if (jsonElement.ValueKind == JsonValueKind.Object)
        {
            List<MemberDeclarationSyntax> typeMembers = [];
            List<MemberDeclarationSyntax> result = [];
            foreach (JsonProperty prop in jsonElement.EnumerateObject())
            {
                if (prop.Name.StartsWith("%"))
                {
                    typeMembers.Add(CreateProperty(prop.Name.Substring(1), "Dictionary<string, object>"));
                }
                else
                {
                    MemberDeclarationSyntax[] memberSyntax = [.. GetTypeFromJsonElement(prop.Name, prop.Value)];
                    typeMembers.AddRange(memberSyntax);
                }
            }
            result.Add(CreateProperty(key, key + "Model"));
            result.Add(SyntaxFactory.ClassDeclaration(key + "Model")
                .WithModifiers(
                        SyntaxFactory.TokenList(
                            SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithMembers(SyntaxFactory.List(typeMembers)));
            return result;
        }
        else if (jsonElement.ValueKind == JsonValueKind.Array)
        {
            JsonElement.ArrayEnumerator enumerator = jsonElement.EnumerateArray();
            if (enumerator.MoveNext())
            {
                if (enumerator.Current.ValueKind == JsonValueKind.String)
                {
                    return [CreateProperty(key, typeof(string[]).Name)];
                }
                else if (enumerator.Current.ValueKind == JsonValueKind.Number)
                {
                    if (enumerator.Current.TryGetInt32(out _))
                    {
                        return [CreateProperty(key, typeof(int[]).Name)];
                    }
                    else if (enumerator.Current.TryGetInt64(out _))
                    {
                        return [CreateProperty(key, typeof(long[]).Name)];
                    }
                    else if (enumerator.Current.TryGetDecimal(out _))
                    {
                        return [CreateProperty(key, typeof(decimal[]).Name)];
                    }
                }
                else if (enumerator.Current.ValueKind is JsonValueKind.True or JsonValueKind.False)
                {
                    return [CreateProperty(key, typeof(bool[]).Name)];
                }
                else
                {
                    IEnumerable<MemberDeclarationSyntax> members = GetTypeFromJsonElement(key, enumerator.Current);
                    ClassDeclarationSyntax arrayClass = members.OfType<ClassDeclarationSyntax>().FirstOrDefault(m => GetMemberName(m).EndsWith(key + "Model"));
                    List<MemberDeclarationSyntax> result = [];
                    result.Add(CreateProperty(key, GetMemberName(arrayClass) + "[]"));
                    result.Add(arrayClass);
                    return result;
                }
            }
        }
        return [];
    }

    private static PropertyDeclarationSyntax CreateProperty(string propertyName, string typeName)
    {
        return SyntaxFactory.PropertyDeclaration(
                SyntaxFactory.ParseTypeName(typeName),
                SyntaxFactory.Identifier(propertyName))
            .WithModifiers(
                SyntaxFactory.TokenList(
                    SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
            .WithAccessorList(
                SyntaxFactory.AccessorList(
                    SyntaxFactory.List(
                        [
                            SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                            SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                        ])));
    }

    public static CompilationUnitSyntax GenerateCode(ClassDeclarationSyntax classSyntax, ServiceDiscoveryResponse serviceResponse)
    {
        IEnumerable<ClassDeclarationSyntax> requests = serviceResponse.Services.Select(s =>
        {
            ClassDeclarationSyntax requestDeclaration = SyntaxFactory.ClassDeclaration(s.RequestName)
                .WithModifiers(
                        SyntaxFactory.TokenList(
                            SyntaxFactory.Token(SyntaxKind.PublicKeyword)));
            foreach (KeyValuePair<string, object> prop in s.RequestProperties)
            {
                MemberDeclarationSyntax[] members = [.. GetTypeFromJsonElement(prop.Key, (JsonElement)prop.Value)];
                requestDeclaration = requestDeclaration.AddMembers(members);
            }
            return requestDeclaration;
        });
        IEnumerable<ClassDeclarationSyntax> responses = serviceResponse.Services.Select(s =>
        {
            ClassDeclarationSyntax responseDeclaration = SyntaxFactory.ClassDeclaration(s.ResponseName)
                .WithModifiers(
                        SyntaxFactory.TokenList(
                            SyntaxFactory.Token(SyntaxKind.PublicKeyword)));
            foreach (KeyValuePair<string, object> prop in s.ResponseProperties)
            {
                MemberDeclarationSyntax[] members = [.. GetTypeFromJsonElement(prop.Key, (JsonElement)prop.Value)];
                responseDeclaration = responseDeclaration.AddMembers(members);
            }
            return responseDeclaration;
        });
        SyntaxList<UsingDirectiveSyntax> usings = SyntaxFactory.List(
        [
            SyntaxFactory.UsingDirective(
                SyntaxFactory.IdentifierName("System"))
                .WithUsingKeyword(
                    SyntaxFactory.Token(
                        SyntaxFactory.TriviaList(
                            SyntaxFactory.Comment("// <auto-generated>")),
                        SyntaxKind.UsingKeyword,
                        SyntaxFactory.TriviaList())),
                SyntaxFactory.UsingDirective(
                    SyntaxFactory.QualifiedName(
                        SyntaxFactory.IdentifierName("System"),
                        SyntaxFactory.IdentifierName("Collections"))),
                SyntaxFactory.UsingDirective(
                    SyntaxFactory.QualifiedName(
                        SyntaxFactory.QualifiedName(
                            SyntaxFactory.IdentifierName("System"),
                            SyntaxFactory.IdentifierName("Collections")),
                        SyntaxFactory.IdentifierName("Generic"))
            )
        ]);

        return SyntaxFactory.CompilationUnit()
            .WithUsings(usings)
            .WithMembers(
                SyntaxFactory.SingletonList<MemberDeclarationSyntax>(
                    SyntaxFactory.FileScopedNamespaceDeclaration(
                        SyntaxFactory.IdentifierName(Parent<FileScopedNamespaceDeclarationSyntax>(classSyntax).Name.ToFullString())
                    ).WithMembers(
                        SyntaxFactory.SingletonList<MemberDeclarationSyntax>(
                            SyntaxFactory.ClassDeclaration(classSyntax.Identifier.ValueText)
                            .WithModifiers(classSyntax.Modifiers)
                            .WithMembers(
                                SyntaxFactory.List<MemberDeclarationSyntax>(
                                    [
                                        SyntaxFactory.ClassDeclaration("Requests")
                                        .WithModifiers(
                                            SyntaxFactory.TokenList(
                                                SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                                        .WithMembers([..requests]),
                                        SyntaxFactory.ClassDeclaration("Responses")
                                        .WithModifiers(
                                            SyntaxFactory.TokenList(
                                                SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                                        .WithMembers([..responses])
                                    ])))
                        )
                ))
            .NormalizeWhitespace();
    }

    private static T Parent<T>(SyntaxNode syntax)
    {
        SyntaxNode? current = syntax.Parent;
        while (current is not null)
        {
            if (current is T t)
            {
                return t;
            }
            current = current.Parent;
        }
        throw new InvalidOperationException("Node not found");
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValueProvider<ImmutableArray<GeneratorEntry>> serviceDiscoveryProvider
            = context.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName: "Transceiver.ServiceDiscoveryAttribute",
            predicate: (node, cancelationToken) =>
            {
                if (node is not ClassDeclarationSyntax classNode)
                {
                    return false;
                }
                return classNode.Modifiers.Any(SyntaxKind.PartialKeyword);
            },
        transform: (syntaxContext, cancellationToken) =>
        {
            ClassDeclarationSyntax classNode = (syntaxContext.TargetNode as ClassDeclarationSyntax)!;
            string url = classNode.AttributeLists.SelectMany(a => a.Attributes)
                .Where(a => "ServiceDiscoveryAttribute".StartsWith(((IdentifierNameSyntax)a.Name).Identifier.ValueText))
                .Select(a => a.ArgumentList!.Arguments.First().Expression)
                .OfType<LiteralExpressionSyntax>()
                .Select(lit => lit.Token.ValueText)
                .FirstOrDefault();

            return new GeneratorEntry(url, classNode);
        }).WithComparer(EqualityComparer<GeneratorEntry>.Default)
        .Collect();

        IncrementalValueProvider<ImmutableArray<AdditionalText>> jsonFiles = context.AdditionalTextsProvider
            .Where(text => text.Path.EndsWith(".json"))
            .Collect();

        IncrementalValueProvider<ImmutableArray<(GeneratorEntry generatorEntry, ServiceDiscoveryResponse response)>> incrementalValueProvider
            = serviceDiscoveryProvider.Combine(jsonFiles).Select((providers, cancelationToken) =>
            {
                return providers.Left
                .Where(generatorEntry => providers.Right.Any(f => f.Path.IndexOf(generatorEntry.Url, StringComparison.InvariantCultureIgnoreCase) >= 0))
                .Select(generatorEntry =>
                {
                    SourceText text = providers.Right
                        .First(f => f.Path.IndexOf(generatorEntry.Url, StringComparison.InvariantCultureIgnoreCase) >= 0)
                        .GetText(cancelationToken)!;
                    string json = text.ToString();
                    JsonSerializerOptions options = new(JsonSerializerOptions.Default);
                    ServiceDiscoveryResponse response = JsonSerializer.Deserialize<ServiceDiscoveryResponse>(json, options)!;

                    return (generatorEntry, response);
                }).ToImmutableArray();
            });

        context.RegisterSourceOutput(incrementalValueProvider, (sourceProductionContext, source) =>
        {
            foreach ((GeneratorEntry generatorEntry, ServiceDiscoveryResponse response) in source)
            {
                CompilationUnitSyntax code = GenerateCode(generatorEntry.ClassSyntax, response);
                sourceProductionContext.AddSource($"{generatorEntry.ClassSyntax.Identifier.ValueText}.g.cs", code.GetText().ToString());
            }
        });
    }
}