# Usa la imagen oficial de ASP.NET Core Runtime para ejecutar la app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Usa la imagen del SDK para compilar
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Examen Parcial.csproj", "./"]
RUN dotnet restore "./Examen Parcial.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "Examen Parcial.csproj" -c Release -o /app/build

# Publicar el binario
FROM build AS publish
RUN dotnet publish "Examen Parcial.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Crear la imagen final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Punto de entrada de la app
ENTRYPOINT ["dotnet", "Examen Parcial.dll"]
