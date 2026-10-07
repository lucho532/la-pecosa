FROM mcr.microsoft.com/dotnet/sdk:10.0 AS compilacion
WORKDIR /codigo

COPY Directory.Build.props ./
COPY src/LaPecosa.Api/LaPecosa.Api.csproj src/LaPecosa.Api/
COPY src/LaPecosa.Aplicacion/LaPecosa.Aplicacion.csproj src/LaPecosa.Aplicacion/
COPY src/LaPecosa.Dominio/LaPecosa.Dominio.csproj src/LaPecosa.Dominio/
COPY src/LaPecosa.Infraestructura/LaPecosa.Infraestructura.csproj src/LaPecosa.Infraestructura/
RUN dotnet restore src/LaPecosa.Api/LaPecosa.Api.csproj

COPY src/ src/
RUN dotnet publish src/LaPecosa.Api/LaPecosa.Api.csproj -c Release -o /publicado --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=compilacion /publicado ./
EXPOSE 8080
ENTRYPOINT ["dotnet", "LaPecosa.Api.dll"]
