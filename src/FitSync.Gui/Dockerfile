# Build stage
FROM node:20-alpine AS build

WORKDIR /app

# Copy package files from GUI directory
COPY src/FitSync.Gui/package*.json ./

# Install dependencies
RUN npm ci

# Copy GUI source files
COPY src/FitSync.Gui/ .

# Build the app
RUN npm run build

# Production stage
FROM nginx:alpine

# Copy built assets from build stage
COPY --from=build /app/dist /usr/share/nginx/html

# Copy nginx configuration
COPY src/FitSync.Gui/nginx.conf /etc/nginx/conf.d/default.conf

# Expose port
EXPOSE 80

# Start nginx
CMD ["nginx", "-g", "daemon off;"]
