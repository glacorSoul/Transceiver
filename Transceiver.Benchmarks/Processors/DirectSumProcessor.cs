// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

namespace Transceiver.Benchmarks.Processors;

internal class DirectSumProcessor : IProcessor<DirectSumRequest, DirectSumResponse>
{
    public Task<DirectSumResponse> ProcessRequestAsync(IClientRequest<DirectSumRequest, DirectSumResponse> request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new DirectSumResponse { Result = request.Data.A + request.Data.B });
    }
}
