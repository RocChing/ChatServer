FROM mcr.microsoft.com/dotnet/core/runtime:3.0-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["ChatServer/ChatServer.csproj", "ChatServer/"]
COPY ["ChatRepository/ChatRepository.csproj", "ChatRepository/"]
COPY ["ChatModel/ChatModel.csproj", "ChatModel/"]
COPY ["BeetleX/BeetleX.csproj", "BeetleX/"]
RUN dotnet restore "ChatServer/ChatServer.csproj"

COPY . .
WORKDIR "/src/ChatServer"
RUN dotnet build "ChatServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ChatServer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ChatServer.dll"]