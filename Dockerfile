ARG API_KEY
ARG API_DB_URL
ARG CLOUDINARY_NAME
ARG CLOUDINARY_KEY
ARG CLOUDINARY_SECRET


FROM mcr.microsoft.com/dotnet/sdk:7.0.202-bullseye-slim AS build

ENV API_KEY=${API_KEY}
ENV API_DB_USER=${API_DB_USER}
ENV API_DB_PASSWORD=${API_DB_PASSWORD}
ENV API_DB_HOST=${API_DB_HOST}

WORKDIR /app


# copy csproj file and restore
COPY src ./src
RUN dotnet restore ./src/api.csproj --runtime alpine-x64

# removed Tests for now as it is empty. TODO: add test step before shipping to production

# Copy everything else and build

COPY . .
RUN dotnet publish ./src -c Release -o out \
    --no-restore true \
    --runtime alpine-x64 \
    --self-contained true \
    /p:PublishTrimmed=true \
    /p:PublishSingleFile=true
WORKDIR /app/src


# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime-deps:7.0.4-alpine3.17 AS runtime

ENV API_KEY=${API_KEY}
ENV API_DB_USER=${API_DB_USER}
ENV API_DB_PASSWORD=${API_DB_PASSWORD}
ENV API_DB_HOST=${API_DB_HOST}

EXPOSE 5000

RUN adduser --disabled-password \
    --home /app \
    --gecos '' dotnetuser && chown -R dotnetuser /app

# upgrade musl to remove potential vulnerability
RUN apk upgrade musl
RUN apk add curl
RUN echo "http://dl-cdn.alpinelinux.org/alpine/edge/testing" >> /etc/apk/repositories && apk update && apk add --no-cache libgdiplus

USER dotnetuser

COPY --from=build /app/out .

ENTRYPOINT ["./api", "--urls", "http://*:5000"]