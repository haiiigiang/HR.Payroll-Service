FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["HRPayroll.API/HRPayroll.API.csproj", "HRPayroll.API/"]
COPY ["HRPayroll.Application/HRPayroll.Application.csproj", "HRPayroll.Application/"]
COPY ["HRPayroll.Domain/HRPayroll.Domain.csproj", "HRPayroll.Domain/"]
COPY ["HRPayroll.Infrastructure/HRPayroll.Infrastructure.csproj", "HRPayroll.Infrastructure/"]
RUN dotnet restore "HRPayroll.API/HRPayroll.API.csproj"

COPY . .
WORKDIR "/src/HRPayroll.API"
RUN dotnet build "HRPayroll.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HRPayroll.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HRPayroll.API.dll"]
