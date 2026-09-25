FROM node:22-alpine
WORKDIR /app
COPY package*.json ./
RUN npm install --omit=dev
COPY src/ src/
ENV PORT=4000 DATA_DIR=/data
EXPOSE 4000
CMD ["node", "src/server.js"]
