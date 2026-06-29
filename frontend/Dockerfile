# Build stage
FROM node:24-alpine AS build
WORKDIR /app

COPY package*.json ./
RUN npm ci

COPY . .
RUN npm run build

# Serve stage
FROM nginx:1.27-alpine AS final

# nginx substitutes env vars into templates at container start (build once, run anywhere).
COPY nginx/templates/ /etc/nginx/templates/
# Custom entrypoint script that derives runtime config (e.g. the API base URL) before nginx starts.
COPY docker-entrypoint.d/ /docker-entrypoint.d/
RUN chmod +x /docker-entrypoint.d/*.sh

COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80
