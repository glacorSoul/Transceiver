// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using System.Reflection;

namespace Transceiver;

public static class ReflectionUtils
{
    public static IEnumerable<Type> AssignableFrom(this IEnumerable<Type> types, Type targetType)
    {
        IEnumerable<Type> result = [.. types
            .Where(type =>
            {
                if(!targetType.IsGenericType)
                {
                    return false;
                }
                if(type.IsInterface && type.IsGenericType)
                {
                    bool isValidType = !type.ContainsGenericParameters && type.GetGenericTypeDefinition() == targetType;
                    return isValidType;
                }
                bool result = !type.ContainsGenericParameters
                    && type.GetInterfaces().Any(interfaceType => interfaceType.IsGenericType
                    && interfaceType.GetGenericTypeDefinition() == targetType);
                if(result)
                {
                    return result;
                }
                return false;
            })];
        return result;
    }

    public static IEnumerable<Type> DiscoverableTypes(this Assembly currentAssembly)
    {
        IEnumerable<Type> types = [];
        types = types.Concat(currentAssembly
            .DefinedTypes
            .SelectMany(type => type.DeclaredConstructors)
            .SelectMany(ctor => ctor.GetParameters())
            .Select(parameter => parameter.ParameterType));

        types = types.Concat(currentAssembly.DefinedTypes
            .Where(t => t.Name.FirstOrDefault() != '<'));

        types = [.. types.Concat(currentAssembly
            .DefinedTypes
            .SelectMany(type => type.DeclaredProperties)
            .Select(property => property.PropertyType))
            .OrderBy(t => t.Name)
            .Distinct()];

        types = [.. types.Concat(currentAssembly
            .DefinedTypes
            .SelectMany(type => type.DeclaredFields)
            .Select(field => field.FieldType))
            .OrderBy(t => t.Name)
            .Distinct()];

        return types;
    }

    public static IEnumerable<Type> DiscoverType(this Assembly assembly, Type typeToDiscover)
    {
        return assembly.DiscoverableTypes().AssignableFrom(typeToDiscover);
    }

    public static bool IsEnumOrNullableEnumType(this Type type)
    {
        return type.IsEnum || type.IsNullableOfNestedType(t => t.IsEnum);
    }

    public static bool IsNullable(this Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }

    public static bool IsNullableOfNestedType(this Type type, Func<Type, bool> nestedTypePredicate)
    {
        Type[] genericArguments = type.GetGenericArguments();

        if (genericArguments.Length == 0)
        {
            return false;
        }
        if (!nestedTypePredicate(genericArguments[0]))
        {
            return false;
        }
        bool isNullable = typeof(Nullable<>).MakeGenericType(genericArguments[0]) == type;
        return isNullable;
    }

    public static Dictionary<string, object?> ToDictionary(this object obj)
    {
        return obj.GetType()
            .GetProperties()
            .Where(p => p.CanRead)
            .ToDictionary(p => p.Name, p => p.GetValue(obj))!;
    }

    public static string ToFullyQualifiedPrettyString(this MethodBase method)
    {
        Type classType = method.DeclaringType!;
        IEnumerable<string> parameterNames = method.GetParameters()
            .Select(p => p.ParameterType.ToFullyQualifiedPrettyString() + " " + p.Name);
        string parameters = string.Join(",\n", parameterNames);
        return $"{classType.ToFullyQualifiedPrettyString()}.{method.Name}({parameters})";
    }

    public static string ToFullyQualifiedPrettyString(this Type type)
    {
        Type[] genericArguments = type.GetGenericArguments();
        if (genericArguments.Length == 0)
        {
            return $"{type.Namespace}.{type.ToHumanReadableShortName()}";
        }
        string argNames = string.Join(",", genericArguments.Select(a => a.ToFullyQualifiedPrettyString()));
        return $"{type.Namespace}.{type.ToHumanReadableShortName()}<{argNames}>";
    }

    public static string ToHumanReadableShortName(this Type type)
    {
        if (!type.IsGenericType)
        {
            return type.Name;
        }
        int idxGenericMarker = type.Name.IndexOf('`');
        if (idxGenericMarker < 0)
        {
            return type.Name;
        }
        if (IsNullable(type))
        {
            Type[] genericArguments = type.GetGenericArguments();
            return genericArguments[0].Name + "?";
        }
        return type.Name.Substring(0, idxGenericMarker);
    }

    public static string ToLettersOnly(this string value)
    {
        return new([.. value.Where(char.IsLetter)]);
    }

    public static string ToShortPrettyString(this Type type)
    {
        Type[] genericArguments = type.GetGenericArguments();
        if (genericArguments.Length == 0)
        {
            return $"{type.ToHumanReadableShortName()}";
        }
        string argNames = string.Join(",", genericArguments.Select(a => a.ToShortPrettyString()));
        return $"{type.ToHumanReadableShortName()}<{argNames}>";
    }

    public static Type ToTypeFromShortPrettyString(this string typeName)
    {
        typeName = typeName.Trim();

        if (typeName.Last() == '?')
        {
            string underlyingTypeName = typeName.Substring(0, typeName.Length - 1);
            Type underlyingType = underlyingTypeName.ToTypeFromShortPrettyString();
            return typeof(Nullable<>).MakeGenericType(underlyingType);
        }

        int genericStartIndex = typeName.IndexOf('<');
        if (genericStartIndex == -1)
        {
            return FindType(typeName) ?? throw new ArgumentException($"Type '{typeName}' could not be resolved.");
        }

        string mainTypeName = typeName.Substring(0, genericStartIndex);
        Type mainType = FindType(mainTypeName) ?? throw new ArgumentException($"Type '{mainTypeName}' could not be resolved.");

        int genericEndIndex = typeName.LastIndexOf('>');
        if (genericEndIndex == -1 || genericEndIndex <= genericStartIndex)
        {
            throw new ArgumentException($"Invalid generic type format: '{typeName}'");
        }

        string genericArgumentsString = typeName.Substring(genericStartIndex + 1, genericEndIndex - genericStartIndex - 1);
        string[] genericArgumentNames = genericArgumentsString.Split(',');

        Type[] genericArguments = [..genericArgumentNames
            .Select(arg => arg.Trim().ToTypeFromShortPrettyString())];

        return mainType.MakeGenericType(genericArguments);
    }

    public static Type TryUnboxType(this Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            return type.GetGenericArguments()[0];
        }
        return type;
    }

    private static TypeInfo FindType(string name)
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.DefinedTypes)
            .First(t => t.FullName!.Contains(name))
            .GetTypeInfo();
    }
}