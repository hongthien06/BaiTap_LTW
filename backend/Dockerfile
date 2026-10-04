# syntax=docker/dockerfile:1

# ---------- Build ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy rieng file project truoc de tan dung cache: doi code mo khong phai restore lai.
COPY BaiTapLTW.sln ./
COPY backend/NhaGiaKim.Domain/NhaGiaKim.Domain.csproj          backend/NhaGiaKim.Domain/
COPY backend/NhaGiaKim.Application/NhaGiaKim.Application.csproj backend/NhaGiaKim.Application/
COPY backend/NhaGiaKim.Infrastructure/NhaGiaKim.Infrastructure.csproj backend/NhaGiaKim.Infrastructure/
COPY backend/NhaGiaKim.Api/NhaGiaKim.Api.csproj                backend/NhaGiaKim.Api/
COPY backend/NhaGiaKim.Tests/NhaGiaKim.Tests.csproj            backend/NhaGiaKim.Tests/
RUN dotnet restore backend/NhaGiaKim.Api/NhaGiaKim.Api.csproj

COPY backend/ backend/
RUN dotnet publish backend/NhaGiaKim.Api/NhaGiaKim.Api.csproj \
    -c Release -o /app/publish --no-restore

# ---------- Runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# curl cho healthcheck cua compose.
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish ./

# Thu muc file tai len phai ghi duoc boi user khong phai root.
RUN mkdir -p /app/wwwroot/uploads && chown -R $APP_UID:$APP_UID /app/wwwroot

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

# Khong chay bang root.
USER $APP_UID

ENTRYPOINT ["dotnet", "NhaGiaKim.Api.dll"]
