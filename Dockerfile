# SDK for building.
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env

WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ByondLang/ByondLang.csproj ./
RUN dotnet restore

COPY ./ByondLang ./
RUN dotnet publish -c Release -o out


# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=build-env /app/out .

EXPOSE 1945

ENTRYPOINT ["dotnet", "ByondLang.dll"]