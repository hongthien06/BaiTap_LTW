# syntax=docker/dockerfile:1

# ---------- Build ----------
FROM node:22-alpine AS build
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci

COPY . .
# Khong dat VITE_API_BASE_URL: nginx proxy /api va /uploads cung origin,
# nho vay khong vuong CORS va thuoc tinh download cua file review moi co tac dung.
RUN npm run build

# ---------- Runtime ----------
FROM nginx:1.27-alpine AS runtime

COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80

# Dung 127.0.0.1 chu khong phai localhost: trong alpine, localhost giai ra ::1 truoc,
# ma nginx "listen 80" chi bind IPv4 -> healthcheck bao connection refused du web van chay.
HEALTHCHECK --interval=15s --timeout=3s --start-period=5s --retries=5 \
    CMD wget -qO- http://127.0.0.1/ >/dev/null || exit 1
