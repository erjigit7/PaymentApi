# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY PaymentApi.Api/PaymentApi.Api.csproj PaymentApi.Api/
COPY PaymentApi.Application/PaymentApi.Application.csproj PaymentApi.Application/
COPY PaymentApi.Domain/PaymentApi.Domain.csproj PaymentApi.Domain/
COPY PaymentApi.Infrastructure/PaymentApi.Infrastructure.csproj PaymentApi.Infrastructure/

RUN dotnet restore PaymentApi.Api/PaymentApi.Api.csproj

# Copy remaining sources and publish
COPY . .
WORKDIR /src/PaymentApi.Api
RUN dotnet publish PaymentApi.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS="http://+:8080;https://+:8081"

COPY --from=build /app/publish ./

EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "PaymentApi.Api.dll"]