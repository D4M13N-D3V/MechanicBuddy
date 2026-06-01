# ---------- build ----------
FROM node:22 AS build
WORKDIR /app

# Version build args
ARG VERSION=0.0.0
ARG BUILD_SHA=unknown

# API URL build args. NEXT_PUBLIC_* is inlined into the client bundle at build
# time, so the production URL must be supplied here (not at runtime). Defaults
# target local dev.
ARG API_URL=http://localhost:15567
ARG NEXT_PUBLIC_API_URL=http://localhost:15567

COPY frontend/package*.json ./
RUN npm ci

COPY frontend/. .

# Set dummy env vars for build time (actual values injected at runtime)
ENV SESSION_SECRET=build-time-placeholder-secret-32chars
ENV SERVER_SECRET=build-time-placeholder
ENV API_URL=$API_URL
ENV NEXT_PUBLIC_API_URL=$NEXT_PUBLIC_API_URL
ENV NEXT_PUBLIC_APP_VERSION=${VERSION}
ENV NEXT_PUBLIC_BUILD_SHA=${BUILD_SHA}

RUN npm run build

# ---------- runtime ----------
FROM node:22-slim
WORKDIR /app

# Version build args
ARG VERSION=0.0.0
ARG BUILD_SHA=unknown

ENV NODE_ENV=production
ENV NEXT_PUBLIC_APP_VERSION=${VERSION}
ENV NEXT_PUBLIC_BUILD_SHA=${BUILD_SHA}

COPY --from=build /app ./
EXPOSE 3000
CMD ["npm","start"]

