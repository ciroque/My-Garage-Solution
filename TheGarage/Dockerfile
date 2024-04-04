# Use the appropriate SDK image as a build environment
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

#COPY TheGarage/TheGarage.csproj TheGarage/
#RUN dotnet restore TheGarage/TheGarage.csproj

COPY . .
COPY ../Garage/Photo.cs .

RUN echo "Current directory is $(pwd)" && ls -la

WORKDIR /src/TheGarage

RUN echo "Current directory is $(pwd)" && ls -la

RUN dotnet restore . # TheGarage.csproj
RUN dotnet build . -c Release -o /app/build

# Publish the project
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Build the final image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Expose the port
# EXPOSE 7213

# Start the application
ENTRYPOINT ["dotnet", "TheGarage.dll"]  
