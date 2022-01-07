FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

# copy csproj file and restore
COPY *.sln ./
COPY src ./src
COPY Tests ./Tests
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish api.sln -c Release -o out
WORKDIR /app/src

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 as runtime

ENV ASPNETCORE_URLS=http://*:9000
#ENV ASPNETCORE_ENVIRONMENT=”production”
#EXPOSE 9000

RUN apt-get update && apt-get install -y apt-utils libgdiplus libc6-dev

COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "api.dll"] 