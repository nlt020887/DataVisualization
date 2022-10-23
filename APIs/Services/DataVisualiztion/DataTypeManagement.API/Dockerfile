#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80


FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Services/DataVisualiztion/DataTypeManagement.API/DataTypeManagement.API.csproj", "Services/DataVisualiztion/DataTypeManagement.API/"]
RUN dotnet restore "Services/DataVisualiztion/DataTypeManagement.API/DataTypeManagement.API.csproj"
COPY . .
WORKDIR "/src/Services/DataVisualiztion/DataTypeManagement.API"
RUN dotnet build "DataTypeManagement.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DataTypeManagement.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DataTypeManagement.API.dll"]