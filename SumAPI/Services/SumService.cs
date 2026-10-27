// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using Grpc.Core;

namespace SumAPI.Services;

public class SumService : Summer.SummerBase
{
    public override Task<SumReply> Sum(SumRequest request, ServerCallContext context)
    {
        int result = request.A + request.B;
        return Task.FromResult(new SumReply { Result = result });
    }
}
