# syntax=docker/dockerfile:1.7
# Build context is `src/webapp/`.

# ---- Build stage ------------------------------------------------------------
FROM node:22-alpine AS build
WORKDIR /src

# Install deps first so the layer cache survives source-only changes.
COPY package.json package-lock.json ./
RUN npm ci

# The OpenAPI generator runs as part of `npm run gen-api` if swagger.json is
# present; the dev workflow regenerates it from the API build. For the prod
# image we expect the typed client to already be present in src/api/generated/
# (committed via the gen-api script during local dev). Anything missing falls
# back to whatever's checked into source — vite build will fail loudly if a
# referenced symbol is absent.
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
