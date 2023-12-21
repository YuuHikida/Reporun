FROM node:latest

WORKDIR /usr/src/app

# ホストの package.json と package-lock.json をコピー
COPY package*.json ./

# npm パッケージのインストール
RUN npm install

# ホストのソースコードをコンテナの作業ディレクトリにコピー
COPY . .

EXPOSE 3000

CMD ["npm", "start"]
