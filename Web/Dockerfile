# Cette phase est utilisée pour générer le projet Web
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copier les fichiers de projet et restaurer les dépendances
COPY ["Web/Web.csproj", "Web/"]
COPY ["Shared/Shared.csproj", "Shared/"]
RUN dotnet restore "Web/Web.csproj"

# Copier le code source
COPY . .
WORKDIR "/src/Web"

# Build du projet
RUN dotnet build "Web.csproj" -c "$BUILD_CONFIGURATION" -o /app/build

# Cette étape permet de publier le projet Web
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Web.csproj" -c "$BUILD_CONFIGURATION" -o /app/publish /p:UseAppHost=false

# Cette phase utilise nginx pour servir les fichiers statiques
FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html

# Copier les fichiers publiés
COPY --from=publish /app/publish/wwwroot .

# Configuration nginx personnalisée pour SPA
COPY Web/nginx.conf /etc/nginx/nginx.conf

EXPOSE 80