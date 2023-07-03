FROM node:18 AS build
WORKDIR /src

COPY package.json package-lock.json ./
RUN npm ci

COPY ./ ./
RUN npm run build

FROM nginx:alpine

COPY docker/default.conf.patch /default.conf.patch
RUN apk add patch && patch /etc/nginx/conf.d/default.conf /default.conf.patch

COPY --from=build /src/dist/webadmin-frontend /usr/share/nginx/html
