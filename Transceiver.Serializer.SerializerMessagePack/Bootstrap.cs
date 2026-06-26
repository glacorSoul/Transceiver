// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Transceiver.Serializer.SerializerMessagePack;

public static class Bootstrap
{
    public static IServiceCollection AddTransceiverMessagePack(this IServiceCollection services)
    {

        _ = services.AddSingleton<IRequestResponseFactory, RequestResponseFactoryMessagePack>();
        return services;
    }
}
