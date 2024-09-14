FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
USER root
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TheIsland.Website/TheIsland.Website.csproj", "TheIsland.Website/"]
RUN dotnet restore "./TheIsland.Website/TheIsland.Website.csproj"
COPY . .
WORKDIR "/src/TheIsland.Website"
RUN dotnet build "./TheIsland.Website.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TheIsland.Website.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false
RUN rm /app/publish/dual.yaml /app/publish/website.json

FROM base AS final
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV ASPNETCORE_URLS="http://+:38080;https://+:38443"
ENV ASPNETCORE_HTTPS_PORT=38443
WORKDIR /app
RUN apk add --no-cache \
      gcc \
      g++ \
      make \
      libc-dev \
      libstdc++ 

RUN apk add gcompat
RUN apk add musl-dev

EXPOSE 38080
EXPOSE 38443

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "TheIsland.Website.dll"]