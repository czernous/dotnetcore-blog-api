ARG API_KEY
ARG API_DB_URL
ARG CLOUDINARY_NAME
ARG CLOUDINARY_KEY
ARG CLOUDINARY_SECRET


FROM mcr.microsoft.com/dotnet/sdk:8.0.100-1-bookworm-slim-amd64 AS build

ENV API_KEY=${API_KEY}
ENV API_DB_USER=${API_DB_USER}
ENV API_DB_PASSWORD=${API_DB_PASSWORD}
ENV API_DB_HOST=${API_DB_HOST}

WORKDIR /app


# copy csproj file and restore
COPY src ./src
RUN dotnet restore ./src/api.csproj --runtime linux-musl-x64

# removed Tests for now as it is empty. TODO: add test step before shipping to production

# Copy everything else and build

COPY . .
RUN dotnet publish ./src -c Release -o out \
    --no-restore true \
    --runtime linux-musl-x64 \
    /p:PublishTrimmed=true \
    /p:PublishAot=true\
    /p:StripSymbols=true
# /p:PublishSingleFile=true
##    --self-contained true \
WORKDIR /app/src


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

# upgrade musl to remove potential vulnerability
RUN apk upgrade musl
RUN apk add curl
RUN echo "http://dl-cdn.alpinelinux.org/alpine/edge/testing" >> /etc/apk/repositories && apk update && apk add --no-cache libgdiplus

USER dotnetuser

COPY --from=build /app/out .

ENTRYPOINT ["./api", "--urls", "http://*:80"]