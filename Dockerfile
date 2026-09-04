FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY IUBAT_Student_Service/IUBAT_Student_Service.csproj IUBAT_Student_Service/
RUN dotnet restore IUBAT_Student_Service/IUBAT_Student_Service.csproj

COPY IUBAT_Student_Service/ IUBAT_Student_Service/
RUN dotnet publish IUBAT_Student_Service/IUBAT_Student_Service.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:$PORT
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_USE_POLLING_FILE_WATCHER=true
ENV DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE=false
ENV RENDER=true

EXPOSE 5000

ENTRYPOINT ["dotnet", "IUBAT_Student_Service.dll"]
