// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Transceiver;

internal class ResilientProtocol : ITransceiverProtocol
{
    private static readonly Random Random = new();
    private readonly IOptions<TransceiverConfiguration> _config;
    private readonly ILogger<ResilientProtocol> _logger;
    private readonly ITransceiverProtocol _protocol;

    public ResilientProtocol(ITransceiverProtocol protocol, ILogger<ResilientProtocol> logger, IOptions<TransceiverConfiguration> config)
    {
        _protocol = protocol;
        _logger = logger;
        _config = config;
    }

    public AsyncSource<T> ReceiveObjects<T>(Guid requestId) where T : IIdentifiable
    {
        return _protocol.ReceiveObjects<T>(requestId);
    }

    public async Task SendObjectToClientAsync<T>(T data, CancellationToken cancellationToken) where T : IIdentifiable
    {
        for (int i = 0; i < _config.Value.NRetries; ++i)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                await _protocol.SendObjectToClientAsync(data, cancellationToken);
                break;
            }
            catch (Exception ex)
            {
                int jit = Random.Next(-1 * _config.Value.DelayBetweenRetriesMs / 10, _config.Value.DelayBetweenRetriesMs / 10);
                TimeSpan delay = TimeSpan.FromMilliseconds((_config.Value.DelayBetweenRetriesMs * (i + 1)) + jit);
                _logger.LogError(ex, "An error occured when sending data to client");
                await Task.Delay(delay);
            }
        }
    }

    public async Task SendObjectToServerAsync<T>(T data, CancellationToken cancellationToken) where T : IIdentifiable
    {
        for (int i = 0; i < _config.Value.NRetries; ++i)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                await _protocol.SendObjectToServerAsync(data, cancellationToken);
                break;
            }
            catch (Exception ex)
            {
                int jit = Random.Next(-1 * _config.Value.DelayBetweenRetriesMs / 10, _config.Value.DelayBetweenRetriesMs / 10);
                TimeSpan delay = TimeSpan.FromMilliseconds((_config.Value.DelayBetweenRetriesMs * (i + 1)) + jit);
                _logger.LogError(ex, "An error occured when sending data to server");
                await Task.Delay(delay);
            }
        }
    }
}