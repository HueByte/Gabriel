# syntax=docker/dockerfile:1.7
# Build context is `src/webapp/`.

# ---- Build stage ------------------------------------------------------------
FROM node:22-alpine AS build
WORKDIR /src

# Install deps first so the layer cache survives source-only changes.
COPY package.json package-lock.json ./
RUN npm ci

# Bring in the source. Both swagger.json and src/api/generated/ are gitignored
# but expected to exist on disk - they're regenerated on every `dotnet build`
# by the MSBuild OpenAPI target in Gabriel.API.csproj. Workflow contract: run
# `dotnet build` once before `docker compose build webapp` so this COPY has
# something to pick up. Vite will fail loudly here if either is missing.
COPY . .
RUN npm run build

# ---- Runtime stage ----------------------------------------------------------
FROM nginx:1.27-alpine AS runtime

# Replace the default nginx site with our SPA + reverse-proxy config.
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Vite emits to `dist/`. Serve it from nginx's default html root.
COPY --from=build /src/dist /usr/share/nginx/html

EXPOSE 80

# Run nginx in the foreground so docker tracks the process lifecycle.
CMD ["nginx", "-g", "daemon off;"]
