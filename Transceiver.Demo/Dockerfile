# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/runtime:10.0-noble-chiseled-extra AS base
USER $APP_UID
WORKDIR /app


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Packages.props", "."]
COPY ["Directory.Build.props", "."]
COPY ["Transceiver.Demo/Transceiver.Demo.csproj", "Transceiver.Demo/"]
COPY ["Transceiver.AmazonSqs/Transceiver.AmazonSqs.csproj", "Transceiver.AmazonSqs/"]
COPY ["Transceiver/Transceiver.csproj", "Transceiver/"]
COPY ["Transceiver.AzureQueue/Transceiver.AzureQueue.csproj", "Transceiver.AzureQueue/"]
COPY ["Transceiver.DomainSockets/Transceiver.DomainSockets.csproj", "Transceiver.DomainSockets/"]
COPY ["Transceiver.Generator/Transceiver.Generator.csproj", "Transceiver.Generator/"]
COPY ["Transceiver.GooglePubSub/Transceiver.GooglePubSub.csproj", "Transceiver.GooglePubSub/"]
COPY ["Transceiver.Kafka/Transceiver.Kafka.csproj", "Transceiver.Kafka/"]
COPY ["Transceiver.RabbitMq/Transceiver.RabbitMq.csproj", "Transceiver.RabbitMq/"]
COPY ["Transceiver.Serializer.SerializerMessagePack/Transceiver.Serializer.SerializerMessagePack.csproj", "Transceiver.Serializer.SerializerMessagePack/"]
COPY ["Transceiver.Websockets/Transceiver.Websockets.csproj", "Transceiver.Websockets/"]
COPY ["Transceiver.ZeroMq/Transceiver.ZeroMq.csproj", "Transceiver.ZeroMq/"]
RUN dotnet restore "./Transceiver.Demo/Transceiver.Demo.csproj"
COPY . .
WORKDIR "/src/Transceiver.Demo"
RUN dotnet build "./Transceiver.Demo.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Transceiver.Demo.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Transceiver.Demo.dll"]