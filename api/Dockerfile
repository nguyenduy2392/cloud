# =========================
# BUILD STAGE
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY WebApi.sln .

COPY Api/Api.csproj Api/
COPY Application/Application.csproj Application/
COPY Core/Core.csproj Core/

RUN dotnet restore

COPY . .

WORKDIR /src/Api
RUN dotnet publish -c Release -o /app/publish

# =========================
# RUNTIME STAGE
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# heif-convert (libheif-examples) dùng để convert ảnh HEIC/HEIF (iPhone) sang JPEG khi upload
RUN apt-get update \
    && apt-get install -y --no-install-recommends libheif-examples \
    && rm -rf /var/lib/apt/lists/*

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Api.dll"]
