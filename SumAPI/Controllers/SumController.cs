// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using Microsoft.AspNetCore.Mvc;

namespace SumAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class SumController : ControllerBase
{
    [HttpGet("{a}/{b}")]
    public int Get([FromRoute] int a, [FromRoute] int b)
    {
        return a + b;
    }
}
