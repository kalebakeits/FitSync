# syntax=docker/dockerfile:1
FROM node:20-alpine AS build
WORKDIR /app

COPY src/FitSync.Gui/package*.json ./

RUN --mount=type=cache,target=/root/.npm \
    npm ci

COPY src/FitSync.Gui/ .

RUN npm run build

FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY src/FitSync.Gui/nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
