// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

namespace Transceiver.Serializer.SerializerMessagePack;

public class RequestResponseFactoryMessagePack : IRequestResponseFactory
{
    public IIdentifiable CreateClientRequest(Type requestType, Type responseType, IIdentifiable data)
    {
        Type clientRequestType = typeof(ClientRequestMessagePack<,>).MakeGenericType(requestType, responseType);
        IIdentifiable result = (IIdentifiable)Activator.CreateInstance(clientRequestType)!;
        return result;
    }

    public IClientRequest<TRequest, TResponse> CreateClientRequest<TRequest, TResponse>(TRequest data)
    {
        return new ClientRequestMessagePack<TRequest, TResponse>(data);
    }

    public IIdentifiable CreateServerResponse(Type requestType, Type responseType, IIdentifiable data, IIdentifiable request)
    {
        Type clientRequestType = typeof(ServerResponseMessagePack<>).MakeGenericType(responseType);
        IIdentifiable result = (IIdentifiable)Activator.CreateInstance(clientRequestType)!;
        return result;
    }

    public IServerResponse<TResponse> CreateServerResponse<TRequest, TResponse>(TResponse data, IClientRequest<TRequest, TResponse> request)
    {
        return new ServerResponseMessagePack<TResponse>(data, request.Id);
    }
}
