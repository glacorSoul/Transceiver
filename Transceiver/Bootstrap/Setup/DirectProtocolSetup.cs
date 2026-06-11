// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using Microsoft.Extensions.DependencyInjection;

namespace Transceiver;

internal sealed class DirectProtocolSetup : BaseTransceiverSetup
{
    public DirectProtocolSetup(Type transceiverType, IServiceCollection services) : base(transceiverType, services)
    {
        _ = services.AddSingleton<ITransceiverProtocol, DirectProtocol>();
        SetupServer(true);
    }

    public override void SetupClient()
    {
        //SetupClient is implemented as a NoOp.
    }

    public override void SetupServer(bool serverOnly)
    {
        if (!serverOnly)
        {
            throw new ArgumentException("Direct Transceiver only works within the same process. In this case the server and client are the same instance.");
        }
        Type directTransceiverType = typeof(DirectTransceiver<,>).MakeGenericType(TransceiverType.GetGenericArguments());
        _ = Services.AddSingleton(ITransceiverType, provider =>
        {
            object pipeline = provider.GetRequiredService(PipelineType);
            object processor = provider.GetRequiredService(ProcessorType);
            object transceiver = Activator.CreateInstance(directTransceiverType, [processor, pipeline])!;
            return transceiver;
        });
        _ = Services.AddSingleton(provider =>
        {
            return TypeIdAssigner.CreateServerAssigner();
        });
    }
}