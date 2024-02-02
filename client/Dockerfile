# setup stage
FROM node:lts-alpine as setup-stage
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .

# development stage
FROM setup-stage as development-stage
EXPOSE 5173
CMD ["npm", "run", "dev:host"]

# build stage
FROM setup-stage as build-stage
RUN npm run build

# production stage
FROM nginx:stable-alpine as production-stage
COPY --from=build-stage /app/dist /usr/share/nginx/html
RUN rm /etc/nginx/conf.d/default.conf
COPY nginx/nginx.conf /etc/nginx/conf.d
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]