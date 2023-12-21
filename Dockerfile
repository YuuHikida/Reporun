# カスタマイズ方法やVisual Studioの利用方法に関する情報へのリンク
#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# ASP.NET Core 6.0ランタイムを含む基本イメージを取得
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base

# コンテナ内の作業ディレクトリを /app に設定
WORKDIR /app

# コンテナがポート80（HTTP）と443（HTTPS）で通信することを示す
EXPOSE 80
EXPOSE 443

# .NET Core 6.0 SDKを含むビルド用の基本イメージを取得
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

# ビルドステージでの作業ディレクトリを /src に設定
WORKDIR /src

# プロジェクトファイル（csproj）をコンテナにコピー
COPY ["ReportSystem.csproj", "."]

# プロジェクトの依存関係を復元
RUN dotnet restore "./ReportSystem.csproj"

# 残りのプロジェクトファイルをコンテナにコピー
COPY . .

# ビルドステージの作業ディレクトリを更新
WORKDIR "/src/."

# プロジェクトをリリース構成でビルドし、/app/build に出力
RUN dotnet build "ReportSystem.csproj" -c Release -o /app/build

# ビルドステージからpublishステージへ移行
FROM build AS publish

# アプリケーションをパブリッシュ（コンパイルとリソースのパッケージング）
RUN dotnet publish "ReportSystem.csproj" -c Release -o /app/publish /p:UseAppHost=false

# baseステージから最終的な実行イメージを作成
FROM base AS final

# 最終イメージでの作業ディレクトリを /app に設定
WORKDIR /app

# publishステージで作成されたファイルを最終イメージにコピー
COPY --from=publish /app/publish .

# コンテナが起動した際に実行されるコマンドを設定
ENTRYPOINT ["dotnet", "ReportSystem.dll"]
