# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Create a user with a known UID/GID within range 10000-20000.
# This is required by Choreo to run the container as a non-root user.
RUN adduser \
    --disabled-password \
    --gecos "" \
    --home "/nonexistent" \
    --shell "/sbin/nologin" \
    --no-create-home \
    --uid 10014 \
    "choreo"

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["FinalGirlStatBot/FinalGirlStatBot.csproj", "FinalGirlStatBot/"]
COPY ["FinalGirlStatBot.DB/FinalGirlStatBot.DB.csproj", "FinalGirlStatBot/"]
RUN dotnet restore "./FinalGirlStatBot/FinalGirlStatBot.csproj"
COPY . .
WORKDIR "/src/FinalGirlStatBot"
RUN dotnet build "./FinalGirlStatBot.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./FinalGirlStatBot.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Use the above created unprivileged user
USER 10014

ENTRYPOINT ["dotnet", "FinalGirlStatBot.dll"]
EXPOSE 443 5432 9090