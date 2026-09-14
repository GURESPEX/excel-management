FROM node:24-alpine AS build
WORKDIR /app
COPY package.json package-lock.json ./
# ponytail: --legacy-peer-deps works around openapi-typescript's peer typescript@^5.x
# vs. root devDependency typescript@~6.0.2 (npm ci resolves this fine locally with
# npm 12 but not with the older npm bundled in node:24-alpine); revisit once
# openapi-typescript publishes a typescript 6.x-compatible peer range.
RUN npm ci --legacy-peer-deps
COPY . .
RUN npm run build

FROM nginx:alpine AS runtime
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
