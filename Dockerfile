FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Portfolio.Api.csproj", "./"]
RUN dotnet restore "Portfolio.Api.csproj"

COPY . .
RUN dotnet publish "Portfolio.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://0.0.0.0:10000

COPY --from=build /app/publish .

EXPOSE 10000

ENTRYPOINT ["dotnet", "Portfolio.Api.dll"]