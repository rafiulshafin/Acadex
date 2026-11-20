# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["Acadex.Api/Acadex.Api.csproj", "Acadex.Api/"]
RUN dotnet restore "Acadex.Api/Acadex.Api.csproj"

# Copy source code and build release
COPY Acadex.Api/ Acadex.Api/
WORKDIR "/src/Acadex.Api"
RUN dotnet publish "Acadex.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime Image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Acadex.Api.dll"]
