FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["SneakerShop.Api/SneakerShop.Api.csproj", "SneakerShop.Api/"]
RUN dotnet restore "SneakerShop.Api/SneakerShop.Api.csproj"

COPY . .
WORKDIR "/src/SneakerShop.Api"

RUN dotnet publish "SneakerShop.Api.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "SneakerShop.Api.dll"]