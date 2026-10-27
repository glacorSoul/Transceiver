// This file is part of Transceiver.
// Transceiver is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// Transceiver is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using Wolverine;
using Wolverine.Http;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseWolverine();

builder.Services.AddWolverineHttp();

WebApplication app = builder.Build();

app.MapWolverineEndpoints();

await app.RunAsync();
