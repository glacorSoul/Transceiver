// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using System.Text.Json.Serialization;

namespace Transceiver;

[JsonDerivedType(typeof(RequestResponseFactory))]
public interface IRequestResponseFactory
{
    IIdentifiable CreateServerResponse(Type requestType, Type responseType, IIdentifiable data, IIdentifiable request);
    IServerResponse<TResponse> CreateServerResponse<TRequest, TResponse>(TResponse data, IClientRequest<TRequest, TResponse> request);
    IIdentifiable CreateClientRequest(Type requestType, Type responseType, IIdentifiable data);
    IClientRequest<TRequest, TResponse> CreateClientRequest<TRequest, TResponse>(TRequest data);
}

public class RequestResponseFactory : IRequestResponseFactory
{
    public IIdentifiable CreateServerResponse(Type requestType, Type responseType, IIdentifiable data, IIdentifiable request)
    {
        Type serverResponseType = typeof(ServerResponse<,>).MakeGenericType(requestType, responseType);
        IIdentifiable result = (IIdentifiable)Activator.CreateInstance(serverResponseType)!;
        return result;
    }

    public IServerResponse<TResponse> CreateServerResponse<TRequest, TResponse>(TResponse data, IClientRequest<TRequest, TResponse> request)
    {
        return new ServerResponse<TRequest, TResponse>(data, request);
    }

    public IIdentifiable CreateClientRequest(Type requestType, Type responseType, IIdentifiable data)
    {
        Type serverResponseType = typeof(ClientRequest<,>).MakeGenericType(requestType, responseType);
        IIdentifiable result = (IIdentifiable)Activator.CreateInstance(serverResponseType)!;
        return result;
    }

    public IClientRequest<TRequest, TResponse> CreateClientRequest<TRequest, TResponse>(TRequest data)
    {
        return new ClientRequest<TRequest, TResponse>(data);
    }
}
