FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file first for better layer caching
COPY TarsMessanger.csproj .
RUN dotnet restore TarsMessanger.csproj

# Copy the rest of the source code
COPY . .

# Build and publish (restore will be done automatically if needed)
RUN dotnet publish TarsMessanger.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "TarsMessanger.dll"]
