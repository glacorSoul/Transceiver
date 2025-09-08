// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Transceiver;

public class TransceiverStreamProtocol : ReceiveMessagesProtocol<Stream>
{
    private readonly Stream _readStream;
    private readonly Stream _writeStream;

    public TransceiverStreamProtocol(Stream readStream, Stream writeStream,
        IMessageProcessor messageProcessor,
        ISerializer serializer,
        ILogger<TransceiverStreamProtocol> logger,
        IOptions<TransceiverConfiguration> configuration)
        : base(messageProcessor, serializer, logger, configuration)
    {
        _readStream = readStream;
        _writeStream = writeStream;
    }

    public sealed override Task<Stream> SetupWriterAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(_writeStream);
    }

    protected override async Task<(int, object)> ReadAsync(Stream reader, byte[] buffer, CancellationToken cancellationToken)
    {
        int bytesRead = await reader.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
        return (bytesRead, reader);
    }

    protected sealed override Task<Stream> SetupReadAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(_readStream);
    }
    protected override async Task WriteAsync(Stream transceiver, object client, byte[] data, CancellationToken cancellationToken)
    {
        await _writeStream.WriteAsync(data, 0, data.Length, cancellationToken);
        await _writeStream.FlushAsync(cancellationToken);
    }
}