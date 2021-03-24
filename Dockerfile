FROM mcr.microsoft.com/dotnet/sdk:latest as setup

LABEL maintainer="kamolov.amir2000@yandex.ru" \
      version="1.0" \
      description="There is music bot for discord that support yandex.music ."

RUN mkdir -p /usr/src/app 

RUN apt-get update -y && apt-get upgrade -y

RUN apt-get install -y libsodium23 libsodium-dev
RUN apt-get install -y ffmpeg
RUN apt-get install -y libopus0 libopus-dev

FROM setup as build

COPY . /usr/src/app

WORKDIR /usr/src/app
RUN dotnet restore
RUN dotnet build --no-restore --configuration Release

FROM build as run

CMD ["dotnet", "YMDB/bin/Release/net5.0/YMDB.dll", "/usr/src/app/config/BotConfig.json"]