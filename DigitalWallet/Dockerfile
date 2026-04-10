FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Defaults to Gateway; override in Render build args for other services.
ARG PROJECT_PATH=src/Gateway/Gateway.csproj
COPY src/ ./src/
RUN dotnet restore "$PROJECT_PATH"
RUN dotnet publish "$PROJECT_PATH" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Defaults to Gateway; override in Render build args.
ARG APP_DLL=Gateway.dll
ENV APP_DLL=${APP_DLL}
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true

COPY --from=build /app/publish ./
EXPOSE 8080

# Render provides PORT dynamically.
ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} dotnet ${APP_DLL}"]
