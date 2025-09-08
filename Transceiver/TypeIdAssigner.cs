// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Transceiver.Requests;

namespace Transceiver;

public sealed class TypeIdAssigner
{
    private static readonly Type[] SystemTypes = [
        typeof(ClientRequest<ServiceDiscoveryRequest, ServiceDiscoveryResponse>),
        typeof(ServerResponse<ServiceDiscoveryRequest, ServiceDiscoveryResponse>)
    ];

    private readonly ManualResetEventSlim _hasBeenInitialized = new(false);

    public TypeIdAssigner()
    {
        TypeToIdMap = [];
        TypeToUShortIdMap = [];

        for (int i = 0; i < SystemTypes.Length; i++)
        {
            Type type = SystemTypes[i];
            TypeToIdMap[GetKey(type)] = (ushort)i;
            TypeToUShortIdMap[type] = (ushort)i;
        }
        foreach (KeyValuePair<string, ushort> pair in TypeToIdMap)
        {
            Type? type = pair.Key.ToTypeFromShortPrettyString();
            if (type != null)
            {
                TypeToUShortIdMap[type] = pair.Value;
            }
        }
    }

    [JsonConstructor]
    public TypeIdAssigner(Dictionary<string, ushort> typeToIdMap) : this()
    {
        TypeToIdMap = typeToIdMap;
        foreach (KeyValuePair<string, ushort> pair in TypeToIdMap)
        {
            Type? type = pair.Key.ToTypeFromShortPrettyString();
            if (type != null)
            {
                TypeToUShortIdMap[type] = pair.Value;
            }
        }
        _hasBeenInitialized.Set();
    }

    public Dictionary<string, ushort> TypeToIdMap { get; }

    private Dictionary<Type, ushort> TypeToUShortIdMap { get; }

    public static TypeIdAssigner CreateServerAssigner()
    {
        TypeIdAssigner typeIdAssigner = new();
        typeIdAssigner.InitializeServer();
        return typeIdAssigner;
    }

    public Type GetType(ushort id)
    {
        if (id < SystemTypes.Length)
        {
            return SystemTypes[id];
        }
        _hasBeenInitialized.Wait();
        lock (TypeToIdMap)
        {
            foreach (KeyValuePair<Type, ushort> pair in TypeToUShortIdMap)
            {
                if (pair.Value == id)
                {
                    return pair.Key;
                }
            }
            throw new KeyNotFoundException($"Type ID {id} not found.");
        }
    }

    public ushort GetTypeId(Type type)
    {
        int idxType = Array.IndexOf(SystemTypes, type);
        if (idxType >= 0)
        {
            return (ushort)idxType;
        }
        _hasBeenInitialized.Wait();
        string typeName = GetKey(type)!;
        lock (TypeToIdMap)
        {
            if (TypeToIdMap.TryGetValue(typeName, out ushort id))
            {
                return id;
            }
            throw new KeyNotFoundException($"Type {typeName} not found.");
        }
    }

    public void UpdateFrom(TypeIdAssigner idAssigner)
    {
        lock (TypeToIdMap)
        {
            foreach (KeyValuePair<string, ushort> pair in idAssigner.TypeToIdMap)
            {
                if (!TypeToIdMap.ContainsKey(pair.Key))
                {
                    TypeToIdMap[pair.Key] = pair.Value;
                    Type? type = pair.Key.ToTypeFromShortPrettyString();
                    if (type != null)
                    {
                        TypeToUShortIdMap[type] = pair.Value;
                    }
                }
            }
            _hasBeenInitialized.Set();
        }
    }

    private static string GetKey(Type type)
    {
        return type.ToShortPrettyString()!;
    }

    private ushort GetOrCreateTypeId(Type type)
    {
        string typeName = GetKey(type);
        lock (TypeToIdMap)
        {
            if (TypeToIdMap.TryGetValue(typeName, out ushort id))
            {
                return id;
            }
            id = (ushort)(TypeToIdMap.Values.Max() + 1);
            TypeToIdMap[typeName] = id;
            TypeToUShortIdMap[type] = id;
            return id;
        }
    }

    private void InitializeServer()
    {
        Assembly[] assemblies = [Assembly.GetExecutingAssembly(), Assembly.GetEntryAssembly()!];
        if (File.Exists("TypeIdMap.json"))
        {
            string json = File.ReadAllText("TypeIdMap.json");
            Dictionary<string, ushort> savedMap = JsonSerializer.Deserialize<Dictionary<string, ushort>>(json)!;
            foreach (KeyValuePair<string, ushort> entry in savedMap)
            {
                if (!TypeToIdMap.ContainsKey(entry.Key))
                {
                    TypeToIdMap[entry.Key] = entry.Value;
                    Type? type = assemblies.Select(assembly => assembly.GetType(entry.Key)).FirstOrDefault();
                    if (type != null)
                    {
                        TypeToUShortIdMap[type] = entry.Value;
                    }
                }
            }
        }

        Type clientRequestType = typeof(ClientRequest<,>);
        Type serverResponseType = typeof(ServerResponse<,>);
        foreach (Assembly assembly in assemblies)
        {
            IEnumerable<Type> discoverTypes = assembly.DiscoverType(typeof(ITransceiver<,>))
                .Concat(assembly.DiscoverType(typeof(IProcessor<,>)));
            foreach (Type transceiverType in discoverTypes)
            {
                Type[] genericArguments = transceiverType.GetGenericArguments();
                if (!genericArguments.Any())
                {
                    genericArguments = transceiverType.GetInterfaces()
                        .First(i => i.GetGenericTypeDefinition() == typeof(IProcessor<,>)
                            || i.GetGenericTypeDefinition() == typeof(ITransceiver<,>)
                        ).GenericTypeArguments;
                }
                Type specificRequestType = clientRequestType.MakeGenericType(genericArguments);
                Type specificResponseType = serverResponseType.MakeGenericType(genericArguments);
                if (!TypeToUShortIdMap.ContainsKey(specificRequestType))
                {
                    _ = GetOrCreateTypeId(specificRequestType);
                }
                if (!TypeToUShortIdMap.ContainsKey(specificResponseType))
                {
                    _ = GetOrCreateTypeId(specificResponseType);
                }
            }
        }
        SaveTypes();
        _hasBeenInitialized.Set();
    }

    private void SaveTypes()
    {
        string json = JsonSerializer.Serialize(TypeToIdMap, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText($"{nameof(TypeIdAssigner)}.json", json);
    }
}