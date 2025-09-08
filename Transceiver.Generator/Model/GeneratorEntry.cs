// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Transceiver.Generator;

public class GeneratorEntry : IComparable<GeneratorEntry>
{
    public GeneratorEntry(string url, ClassDeclarationSyntax classSyntax)
    {
        Url = url;
        ClassSyntax = classSyntax;
    }

    public ClassDeclarationSyntax ClassSyntax { get; set; }
    public string Url { get; set; }

    public int CompareTo(GeneratorEntry other)
    {
        return other.Url.CompareTo(Url);
    }

    public override bool Equals(object obj)
    {
        if (obj is not GeneratorEntry other)
        {
            return false;
        }
        return other.Url.Equals(Url);
    }

    public override int GetHashCode()
    {
        return Url.GetHashCode();
    }
}