FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["CourseService.API.csproj", "."]
RUN dotnet restore "CourseService.API.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "CourseService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CourseService.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CourseService.API.dll"]
