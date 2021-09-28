# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build-env
# Install NodeJs
RUN apt-get update && \
apt-get install -y wget && \
apt-get install -y gnupg2 && \
wget -qO- https://deb.nodesource.com/setup_16.x | bash - && \
apt-get install -y build-essential nodejs
# End Install
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./ ./
RUN dotnet restore
RUN dotnet build -c Release

# Copy everything else and build
RUN dotnet publish FfAdminWeb -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim
RUN apt-get update && apt-get -y install git
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "FfAdminWeb.dll"]