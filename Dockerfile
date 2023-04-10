FROM mcr.microsoft.com/dotnet/sdk:7.0.202-alpine3.17 AS build
WORKDIR /app

# copy csproj file and restore
COPY src ./src
RUN dotnet restore ./src/api.csproj --runtime alpine-x64

# Copy everything else and build

COPY . ./

WORKDIR /app\
    RUN dotnet publish "./src/api.csproj" -c Release -o out \
    --no-restore \
    --runtime alpine-x64 \
    --self-contained true \
    /p:PublishTrimmed=true \
    /p:PublishSingleFile=true
WORKDIR /app/src


# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime-deps:7.0.4-alpine3.17 AS runtime

RUN adduser --disabled-password \
    --home /app \
    --gecos '' dotnetuser && chown -R dotnetuser /app

# upgrade musl to remove potential vulnerability
RUN apk upgrade musl

USER dotnetuser

COPY --from=build /app/out .

ENTRYPOINT ["./api", "--urls", "http://*:9000"]