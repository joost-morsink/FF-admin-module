# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
# Install NodeJs
RUN apt-get update && \
apt-get install -y wget && \
apt-get install -y gnupg2 && \
wget -qO- https://deb.nodesource.com/setup_16.x | bash - && \
apt-get install -y build-essential nodejs
# End Install
WORKDIR /app

COPY ./FfAdminWeb/ClientApp/package*.json ./FfAdminWeb/ClientApp/
RUN npm install ./FfAdminWeb/ClientApp --prefix ./FfAdminWeb/ClientApp

COPY ./Directory.Build.props ./
COPY ./FfAdminWeb.sln ./
COPY ./Common/Common.csproj ./Common/
COPY ./External.GiveWp/External.GiveWp.csproj ./External.GiveWp/
COPY ./External.Banking/External.Banking.csproj ./External.Banking/
COPY ./AdminModule/AdminModule.csproj ./AdminModule/
COPY ./EventStore/EventStore.csproj ./EventStore/
COPY ./FfAdmin.Test/FfAdmin.Test.csproj ./FfAdmin.Test/
COPY ./FfAdminWeb/FfAdminWeb.csproj ./FfAdminWeb/

RUN dotnet restore

COPY ./ ./
RUN dotnet publish FfAdminWeb -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
RUN apt-get update && apt-get -y install git
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "FfAdminWeb.dll"]