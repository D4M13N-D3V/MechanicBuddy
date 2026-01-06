# ---------- build ----------
FROM node:22 AS build
WORKDIR /app

COPY frontend/package*.json ./
RUN npm ci

COPY frontend/. .

# Set dummy env vars for build time (actual values injected at runtime)
ENV SESSION_SECRET=build-time-placeholder-secret-32chars
ENV SERVER_SECRET=build-time-placeholder
ENV API_URL=http://localhost:15567
ENV NEXT_PUBLIC_API_URL=http://localhost:15567

RUN npm run build

# ---------- runtime ----------
FROM node:22-slim
WORKDIR /app
ENV NODE_ENV=production
COPY --from=build /app ./
EXPOSE 3000
CMD ["npm","start"]

