FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /app

RUN apt-get update && apt-get install -y apt-utils libgdiplus libc6-dev

# copy csproj file and restore
COPY *.sln ./
COPY src ./src
COPY Tests ./Tests
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish api.sln -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:5.0 as runtime

COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "api.dll"]