# YMDB - Yandex Music Discord Bot.
YMDB это музыкальный бот для дискорда написанный на C# и используя библиотеки [DSharp+](https://github.com/DSharpPlus/DSharpPlus), [Yandex.Music.Api](https://github.com/K1llMan/Yandex.Music.Api). Он воспроизводит в голосовом канале дискорда песни по запросу пользователей из сервиса YandexMusic.
[![Deployd](https://github.com/Amir0715/YMDB/actions/workflows/ci-cd.yml/badge.svg?branch=master)](https://github.com/Amir0715/YMDB/actions/workflows/ci-cd.yml)
[![Build](https://github.com/Amir0715/YMDB/actions/workflows/build.yml/badge.svg?branch=master)](https://github.com/Amir0715/YMDB/actions/workflows/build.yml)
## Зависимости.
Для успешной установки и работы бота необходимо установить следущие зависимости:

- [DotNet 5.0](https://dotnet.microsoft.com/download/dotnet/5.0)
- [Ffmpeg](https://ffmpeg.org/download.html)
- [Git](https://git-scm.com/downloads)



## Установка.
Перед тем как начинать выполнять этот этап у вас должы быть установлены все необходимые зависимости.

### Шаг 0.

Выполните следующую команду:

```terminal
sudo apt-get install libsodium23 libsodium-dev
```

### Шаг 1.

#### Склонируйте репозиторий.

```terminal
git clone https://github.com/Amir0715/YMBD.git ~/YMDB -b master
```

### Шаг 2.
#### Создайте файл BotConfig.json.
Создайте файл BotConfig.json в каталоге YMDB и добавьте в него следующее:

```json
{
  "token": "<DISCORD_BOT_TOKEN>",
  "prefix" : "<DISCORD_BOT_PREFIX>",
  "login" : "<YANDEX_MUSIC_LOGIN>",
  "password" : "<YANDEX_MUSIC_PASSWORD>",
  "downloadPath" : "<ABSOLUTE_DOWNLOAD_DIR>"
}
```
Рассмотрим его подробнее:<br>

`<DISCORD_BOT_TOKEN>` - Ваш токен который вам необходимо получить на [странице](https://discord.com/developers/applications) для разработчиков.</br>
`<DISCORD_BOT_PREFIX>` - Префикс к командам для бота.</br>
`<YANDEX_MUSIC_LOGIN>` - Ваш логин к аккаунту yandex.music.</br>
`<YANDEX_MUSIC_PASSWORD>` - Ваш пароль к аккаунту yandex.music.</br>
`<ABSOLUTE_DOWNLOAD_DIR>` - Абсолютный путь к аталогу для временного хранения аудиофайлов.</br>

### Шаг 3. 

#### Соберите проект. 
Востановите зависимости проекта и соберите его следущими командами:
```terminal
cd ~/YMDB
dotnet restore
dotnet build --no-restore
```
Собранное приложение будет лежать в каталоге `~/YMBD/YMBD/bin/Debug/net5.0` с названием `YMBD.dll`.  

### Шаг 4.
#### Запуск приложения 
Запустить его можно вызвав команду: 
```terminal
dotnet ~/YMDB/YMDB/bin/Debug/net5.0/YMBD.dll
```
