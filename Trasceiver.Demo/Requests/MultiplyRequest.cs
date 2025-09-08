// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using Transceiver.Demo.Requests;

namespace Transceiver.Demo;

public sealed class MultiplyProcessor : IProcessor<GeneratedRequests.Requests.MultiplyRequest, GeneratedRequests.Responses.MultiplyResponse>
{
    public Task<GeneratedRequests.Responses.MultiplyResponse> ProcessRequest(GeneratedRequests.Requests.MultiplyRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new GeneratedRequests.Responses.MultiplyResponse
        {
            Result = request.A * request.B
        });
    }
}