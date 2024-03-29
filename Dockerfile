#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=”development”
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["VideoManager/VideoManager.csproj", "VideoManager/"]
RUN dotnet restore "VideoManager/VideoManager.csproj"
COPY . .
WORKDIR "/src/VideoManager"
RUN dotnet build "VideoManager.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VideoManager.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VideoManager.dll"]