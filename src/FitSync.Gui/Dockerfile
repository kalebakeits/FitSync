# syntax=docker/dockerfile:1
FROM node:20-alpine AS build
WORKDIR /build/src/FitSync.Gui

COPY src/FitSync.Gui/package*.json ./

RUN --mount=type=cache,target=/root/.npm \
    npm ci

COPY src/FitSync.Gui/ .
COPY assets/ /build/assets/

RUN npm run build

FROM nginx:alpine
COPY --from=build /build/src/FitSync.Gui/dist /usr/share/nginx/html
COPY src/FitSync.Gui/nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
