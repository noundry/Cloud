FROM mcr.microsoft.com/dotnet/sdk:{{.Framework}} AS build
WORKDIR /src

COPY ./src/{{.ProjectName}}/*.csproj ./src/{{.ProjectName}}/
RUN dotnet restore ./src/{{.ProjectName}}/{{.ProjectName}}.csproj

COPY ./src/{{.ProjectName}}/ ./src/{{.ProjectName}}/
RUN dotnet publish ./src/{{.ProjectName}}/{{.ProjectName}}.csproj -c Release -o /out \
    -p:PublishReadyToRun=true \
    -p:PublishSingleFile=true \
    -p:InvariantGlobalization=true \
    -p:TieredPGO=true \
    --self-contained=false

FROM mcr.microsoft.com/dotnet/aspnet:{{.Framework}} AS runtime
ENV ASPNETCORE_URLS=http://0.0.0.0:{{.Port}} \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_EnableDiagnostics=0

WORKDIR /app

RUN useradd -r -u 10001 appuser

COPY --from=build /out/ /app/
EXPOSE {{.Port}}
USER 10001
ENTRYPOINT ["./{{.ProjectName}}"]