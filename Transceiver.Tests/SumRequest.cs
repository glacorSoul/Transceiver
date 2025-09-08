// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

namespace Transceiver.Tests;

public sealed class ChannelsSumRequest
{
    public ChannelsSumRequest(int a, int b)
    {
        A = a;
        B = b;
    }

    public int A { get; set; }
    public int B { get; set; }
}

public sealed class DomainSocketsSumRequest
{
    public DomainSocketsSumRequest(int a, int b)
    {
        A = a;
        B = b;
    }

    public int A { get; set; }
    public int B { get; set; }
}

public sealed class DomainSocketsSumResponse
{
    public DomainSocketsSumResponse(int result)
    {
        Result = result;
    }

    public int Result { get; set; }
}

public sealed class SumRequest
{
    public SumRequest(int a, int b)
    {
        A = a;
        B = b;
    }

    public int A { get; set; }
    public int B { get; set; }
}

public sealed class SumResponse
{
    public SumResponse(int result)
    {
        Result = result;
    }

    public int Result { get; set; }
}

public sealed class TcpSumRequest
{
    public TcpSumRequest(int a, int b)
    {
        A = a;
        B = b;
    }

    public int A { get; set; }
    public int B { get; set; }
}

public sealed class TcpSumResponse
{
    public TcpSumResponse(int result)
    {
        Result = result;
    }

    public int Result { get; set; }
}

public sealed class UdpSumRequest
{
    public UdpSumRequest(int a, int b)
    {
        A = a;
        B = b;
    }

    public int A { get; set; }
    public int B { get; set; }
}

public sealed class UdpSumResponse
{
    public UdpSumResponse(int result)
    {
        Result = result;
    }

    public int Result { get; set; }
}

public sealed class ChannelsSumResponse
{
    public ChannelsSumResponse(int result)
    {
        Result = result;
    }

    public int Result { get; set; }
}