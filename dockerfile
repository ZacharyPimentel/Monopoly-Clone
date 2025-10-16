# Dockerfile
# Stage 1: Build API
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-api
WORKDIR /src
COPY ./api/*.csproj ./api/
RUN dotnet restore ./api/api.csproj
COPY ./api/. ./api/
WORKDIR /src/api
RUN dotnet build "api.csproj" -c Release -o /app/build

# Stage 2: Publish API
FROM build-api AS publish-api
RUN dotnet publish "api.csproj" -c Release -o /app/publish

# Stage 3: Build UI
FROM node:22-alpine AS build-ui
WORKDIR /app
COPY ./ui/. ./
# Set the environment variable for Vite build mode
ARG BUILD_MODE=production
RUN npm install
# Use the mode from the build argument
RUN npm run build -- --mode ${BUILD_MODE}

# Stage 4: Final image for API
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final-api
WORKDIR /app
COPY --from=publish-api /app/publish .
EXPOSE 5000
ENTRYPOINT ["dotnet", "dotnet-monopoly-api.dll"]

# Stage 5: Final image for UI
FROM nginx:alpine AS final-ui
COPY --from=build-ui /app/dist /usr/share/nginx/html
COPY ./ui/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
