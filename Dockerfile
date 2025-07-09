# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy everything and publish the release
COPY . ./
RUN dotnet publish -c Release -o out

# Use the ASP.NET runtime image to run the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out ./

# Set the port from environment
ENV ASPNETCORE_URLS=http://+:$PORT

# Run the app
ENTRYPOINT ["dotnet", "DEMO.dll"]
