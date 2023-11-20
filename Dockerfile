ARG API_KEY
ARG API_DB_USER
ARG API_DB_PASSWORD
ARG API_DB_HOST
ARG CLOUDINARY_NAME
ARG CLOUDINARY_KEY
ARG CLOUDINARY_SECRET

FROM mcr.microsoft.com/dotnet/sdk:8.0.100-1-bookworm-slim-amd64 AS build

ENV API_KEY=${API_KEY}
ENV API_DB_USER=${API_DB_USER}
ENV API_DB_PASSWORD=${API_DB_PASSWORD}
ENV API_DB_HOST=${API_DB_HOST}

WORKDIR /app

# Install dependencies
RUN apt-get update && \
    apt-get upgrade -y && \
    apt-get install -y clang zlib1g-dev

# Copy csproj file and restore
COPY src ./src
RUN dotnet restore ./src/api.csproj --runtime linux-musl-x64

# Copy everything else and build
COPY . .
RUN dotnet publish ./src -c Release -o out \
    --no-restore true \
    --runtime linux-musl-x64 \
    /p:OptimizationPreference=Speed \
    /p:CopyOutputSymbolsToPublishDirectory=false \
    /p:PublishAot=true 

## Remove unnecessary files
RUN rm -rf src && apt-get clean

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime-deps:8.0.0-alpine3.18 AS runtime

ENV API_KEY=${API_KEY}
ENV API_DB_USER=${API_DB_USER}
ENV API_DB_PASSWORD=${API_DB_PASSWORD}
ENV API_DB_HOST=${API_DB_HOST}

EXPOSE 80

RUN adduser --disabled-password \
    --home /app \
    --gecos '' dotnetuser && chown -R dotnetuser /app

USER dotnetuser

COPY --from=build /app/out .

ENTRYPOINT ["./api", "--urls", "http://*:80"]
