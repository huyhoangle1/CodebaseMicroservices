FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5002

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["OrderService.API.csproj", "."]
RUN dotnet restore "OrderService.API.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "OrderService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OrderService.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrderService.API.dll"]
