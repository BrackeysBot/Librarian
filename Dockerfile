FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Librarian/Librarian.csproj", "Librarian/"]
RUN dotnet restore "Librarian/Librarian.csproj"
COPY . .
WORKDIR "/src/Librarian"
RUN dotnet build "Librarian.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Librarian.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Librarian.dll"]
