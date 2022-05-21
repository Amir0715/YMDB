# YMDB - Yandex Music Discord Bot
YMDB это музыкальный бот для дискорда написанный на C# и используя библиотеки [DSharp+](https://github.com/DSharpPlus/DSharpPlus), [Yandex.Music.Api](https://github.com/K1llMan/Yandex.Music.Api). Он воспроизводит в голосовом канале дискорда песни по запросу пользователей из сервиса YandexMusic.

[![Deployd](https://github.com/Amir0715/YMDB/actions/workflows/ci-cd.yml/badge.svg?branch=master)](https://github.com/Amir0715/YMDB/actions/workflows/ci-cd.yml)
[![Build](https://github.com/Amir0715/YMDB/actions/workflows/build.yml/badge.svg?branch=master)](https://github.com/Amir0715/YMDB/actions/workflows/build.yml)

## Зависимости

Для успешной установки и работы бота необходимо установить следущие зависимости:

- [DotNet 5.0](https://dotnet.microsoft.com/download/dotnet/5.0)
- [Ffmpeg](https://ffmpeg.org/download.html)
- [Git](https://git-scm.com/downloads)



## Установка

Перед тем как начинать выполнять этот этап у вас должы быть установлены все необходимые зависимости.

### Шаг 0
Выполните следующую команду:

```terminal
sudo apt-get install libsodium23 libsodium-dev
```

## Gentoo
```terminal
sudo MAKEOPTS="-j8" emerge -av "<dev-dotnet/dotnet-sdk-bin-6.0" dev-libs/libsodium
```

### Шаг 1
#### Склонируйте репозиторий

```terminal
git clone https://github.com/Amir0715/YMBD.git $YOUR_DIR -b master
```

### Шаг 2
#### Создайте файл BotConfig.json
Создайте файл BotConfig.json в каталоге YMDB/config и добавьте в него следующее:

```json
{
  "token": "<DISCORD_BOT_TOKEN>",
  "yandexToken": "<YANDEX_MUSIC_TOKEN>",
  "prefix" : "<DISCORD_BOT_PREFIX>",
  "login" : "<YANDEX_MUSIC_LOGIN>",
  "password" : "<YANDEX_MUSIC_PASSWORD>",
  "downloadPath" : "<ABSOLUTE_DOWNLOAD_DIR>"
}
```
Рассмотрим его подробнее:<br>

`<DISCORD_BOT_TOKEN>` - Ваш токен который вам необходимо получить на [странице](https://discord.com/developers/applications) для разработчиков.</br>
`<YANDEX_MUSIC_TOKEN>` - Ваш токен от яндекс музыки</br>
`<DISCORD_BOT_PREFIX>` - Префикс к командам для бота.</br>
`<YANDEX_MUSIC_LOGIN>` - Ваш логин к аккаунту yandex.music.</br>
`<YANDEX_MUSIC_PASSWORD>` - Ваш пароль к аккаунту yandex.music.</br>
`<ABSOLUTE_DOWNLOAD_DIR>` - Абсолютный путь к аталогу для временного хранения аудиофайлов.</br>

Чтобы получить токен необходимо:
  1. Создать на указанной странице приложение.
  2. На странице приложения нажать на вкладку Бот.
  3. На странице бота справа от аватарки кнопка скопировать.

Чтобы получить токен яндекса, выполните запрос

```shell
curl --location --request POST 'https://oauth.yandex.ru/token' \
--header 'Content-Type: application/x-www-form-urlencoded' \
--data-urlencode 'grant_type=password' \
--data-urlencode 'client_id=23cabbbdc6cd418abb4b39c32c41195d' \
--data-urlencode 'client_secret=53bc75238f0c4d08a118e51fe9203300' \
--data-urlencode 'username=<YANDEX_MUSIC_LOGIN>' \
--data-urlencode 'password=<YANDEX_MUSIC_PASSWORD>'
```

Заполнение токена яндекса не обязательно. 
В случае, если токен яндекса существует, то используется он, 
в обратном случае используется авторизация через логин и пароль

### Шаг 3

#### Соберите проект
Востановите зависимости проекта и соберите его следущими командами:
```terminal
cd ~/YMDB
dotnet restore
dotnet build --no-restore
```
Собранное приложение будет лежать в каталоге `$YOUR_DIR/YMBD/YMBD/bin/Debug/net5.0` с названием `YMBD.dll`.

### Шаг 

#### Добавьте бота на сервер

По ссылке выше (дискорд портал разработчиков) зайдите в свое приложение.
Нажмите OAuth2 > URL Generator.
Поставьте галку bot.
в графе Voice Permissions
Выставьте:
 * Connect
 * Speak
 * Use voice activity

В графе Text permissions
Выставьте:
 * Read message history

### Шаг 5
#### Запуск приложения
Запустить его можно вызвав команду:
```terminal
dotnet $YOUR_DIR/YMDB/YMDB/bin/Debug/net5.0/YMBD.dll
```

### Управление ботом.
Все команды пишутся с использованием префикса, указанного вами ранее в конфиге следующим образом:
"префикс команда аргументы ( если требуются)"
Разделенные пробелами.

#### Основные команды
Команды указаны в виде 
"сокращение-полная команда (список возможных аргументов через запятую) # описание"
Аргументы относятся как к сокращенной, так и к полной команде

j-join (название канала) \# присоединиться к голосовому каналу  
p-play (url, название песни, группы, плейлиста, альбома) \# запустить проигрывание (...) иногда нужно пару раз вызвать  
s-stop   
se-search(название трека)  
n-next  
le-leave \# выйти из канала
