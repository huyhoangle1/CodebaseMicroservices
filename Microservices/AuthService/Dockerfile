FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5003

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["AuthService.API.csproj", "."]
RUN dotnet restore "AuthService.API.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "AuthService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AuthService.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AuthService.API.dll"]
