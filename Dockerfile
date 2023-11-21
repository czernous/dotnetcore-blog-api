ARG API_KEY
ARG API_DB_USER
ARG API_DB_PASSWORD
ARG API_DB_HOST
ARG CLOUDINARY_NAME
ARG CLOUDINARY_KEY
ARG CLOUDINARY_SECRET


FROM mcr.microsoft.com/dotnet/nightly/sdk:8.0-alpine AS build

ENV API_KEY=${API_KEY}
ENV API_DB_USER=${API_DB_USER}
ENV API_DB_PASSWORD=${API_DB_PASSWORD}
ENV API_DB_HOST=${API_DB_HOST}

WORKDIR /app 
# Install dependencies
# RUN apt-get update && \
#     apt-get upgrade -y && \
#     apt-get install -y clang zlib1g-dev

# Copy csproj file and restore
COPY src ./src
ARG RUNTIME_ID=linux-musl-x64
RUN dotnet restore ./src/api.csproj -r $RUNTIME_ID

# Copy everything else and build
COPY . .
RUN dotnet publish ./src/api.csproj -c Release -o out \
    --no-restore true \
    --runtime $RUNTIME_ID \
    --self-contained true \
    /p:PublishTrimmed=true \
    /p:PublishSingleFile=true


# Build runtime image
FROM mcr.microsoft.com/dotnet/nightly/runtime-deps:8.0-alpine AS runtime

ENV API_KEY=${API_KEY}
ENV API_DB_USER=${API_DB_USER}
ENV API_DB_PASSWORD=${API_DB_PASSWORD}
ENV API_DB_HOST=${API_DB_HOST}

WORKDIR /app

# RUN apk upgrade musl
RUN apk update && apk add --no-cache libgdiplus

EXPOSE 80

COPY --from=build /app/out .


ENTRYPOINT ["./api", "--urls", "http://*:80"]
