// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using Transceiver.OtherComponents;

namespace Transceiver;

public class TransceiverSslProtocol : ReceiveMessagesProtocol<Stream>
{
    private readonly Socket _connectSocket;
    private readonly Socket _listenSocket;
    private readonly Lazy<Task<Stream>> _setupWriter;
    private readonly ISocketFactory _socketFactory;
    private readonly SemaphoreSlim _writeLock = new(1, 1);
    private readonly ICertificateLoader _certificateLoader;

    public TransceiverSslProtocol(
        ISocketFactory socketFactory,
        ICertificateLoader certificateLoader,
        IMessageProcessor messageProcessor,
        ISerializer serializer,
        ILogger<TransceiverSslProtocol> logger,
        IOptions<TransceiverConfiguration> configuration)
        : base(messageProcessor, serializer, logger, configuration)
    {
        _socketFactory = socketFactory;
        _certificateLoader = certificateLoader;
        _listenSocket = socketFactory.Listen();
        _connectSocket = socketFactory.Connect();
        _setupWriter = new(SetupWriter);
    }

    public sealed override Task<Stream> SetupWriterAsync(CancellationToken cancellationToken)
    {
        return _setupWriter.Value;
    }

    protected sealed override async Task<(int, object)> ReadAsync(Stream reader, byte[] buffer, CancellationToken cancellationToken)
    {
        int bytesRead = await reader.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
        return (bytesRead, reader);
    }

    protected sealed override async Task<Stream> SetupReadAsync(CancellationToken cancellationToken)
    {
        NetworkStream networkStream = new(await _socketFactory.AcceptAsync(_listenSocket), true);
        SslStream sslStream = new(networkStream, false);
        await sslStream.AuthenticateAsServerAsync(_certificateLoader.LoadCertificate(Configuration.Value.CertificateThumbprint));
        return sslStream;
    }

    protected override async Task WriteAsync(Stream transceiver, object client, byte[] data, CancellationToken cancellationToken)
    {
        await _writeLock.WaitAsync(cancellationToken);
        try
        {
            await transceiver.WriteAsync(data, 0, data.Length, cancellationToken);
            await transceiver.FlushAsync(cancellationToken);
        }
        finally
        {
            _ = _writeLock.Release();
        }
    }

    private async Task<Stream> SetupWriter()
    {
        SslStream writeStream = new(new NetworkStream(_connectSocket, true), false, ValidateServerCertificate, null);
        await writeStream.AuthenticateAsClientAsync(Configuration.Value.Server.Name);
        return writeStream;
    }

    private bool ValidateServerCertificate(
      object sender,
      X509Certificate? certificate,
      X509Chain? chain,
      SslPolicyErrors sslPolicyErrors)
    {
        if (sslPolicyErrors == SslPolicyErrors.None)
        {
            return true;
        }

        Logger.LogWarning("Certificate error: {SslError}", sslPolicyErrors);
        return false;
    }
}