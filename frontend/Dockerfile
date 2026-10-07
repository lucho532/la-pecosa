FROM node:22-alpine AS compilacion
WORKDIR /codigo

COPY package.json package-lock.json ./
RUN npm ci

COPY . .
ARG VITE_URL_API=http://localhost:8080
ENV VITE_URL_API=$VITE_URL_API
RUN npm run build

FROM nginx:1.27-alpine
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=compilacion /codigo/dist /usr/share/nginx/html
EXPOSE 80
