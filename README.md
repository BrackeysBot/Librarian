<h1 align="center">Librarian</h1>
<p align="center"><img src="icon.png" width="128"></p>
<p align="center"><i>A Discord bot for managing bookmarks.</i></p>
<p align="center">
<a href="https://github.com/BrackeysBot/Librarian/releases"><img src="https://img.shields.io/github/v/release/BrackeysBot/Librarian?include_prereleases&style=flat-square"></a>
<a href="https://github.com/BrackeysBot/Librarian/actions/workflows/dotnet.yml"><img src="https://img.shields.io/github/actions/workflow/status/BrackeysBot/Librarian/dotnet.yml?branch=main&style=flat-square" alt="GitHub Workflow Status" title="GitHub Workflow Status"></a>
<a href="https://github.com/BrackeysBot/Librarian/issues"><img src="https://img.shields.io/github/issues/BrackeysBot/Librarian?style=flat-square" alt="GitHub Issues" title="GitHub Issues"></a>
<a href="https://github.com/BrackeysBot/Librarian/blob/main/LICENSE.md"><img src="https://img.shields.io/github/license/BrackeysBot/Librarian?style=flat-square" alt="MIT License" title="MIT License"></a>
</p>

## About
Librarian is a Discord bot which allows users to bookmark messages and view them later. It is designed to be used in the
[Brackeys Discord server](https://discord.gg/brackeys), but is open source and can be used in other servers.

## Installing and configuring Librarian 
Librarian runs in a Docker container, and there is a [docker-compose.yaml](docker-compose.yaml) file which simplifies this process.

### Clone the repository
To start off, clone the repository into your desired directory:
```bash
git clone https://github.com/BrackeysBot/Librarian.git
```
Step into the Librarian directory using `cd Librarian`, and continue with the steps below.

### Setting things up
The bot's token is passed to the container using the `DISCORD_TOKEN` environment variable. Create a file named `.env`, and add the
following line:
```
DISCORD_TOKEN=your_token_here
```

One directory is required to exist for Docker compose to mount as a container volume, so create the `logs` directory:
```bash
mkdir logs
```

The `logs` directory is used to store logs in a format similar to that of a Minecraft server. `latest.log` will contain the log
for the current day and current execution. All past logs are archived.

This bot does not require any configuration, nor does it persist any data. This means that you can simply run the bot, and as long
as a logs folder is mounted, it will work out of the box.

### Launch Librarian
To launch Librarian, simply run the following commands:
```bash
sudo docker-compose build
sudo docker-compose up --detach
```

## Updating Librarian
To update Librarian, simply pull the latest changes from the repo and restart the container:
```bash
git pull
sudo docker-compose stop
sudo docker-compose build
sudo docker-compose up --detach
```

## Using Librarian
To bookmark a message, simply react to it with the bookmark emoji (🔖) or Apps > Bookmark Message. Bookmarks are sent as a DM to
the user who bookmarked the message.

## License
This bot is under the [MIT License](LICENSE.md).

## Disclaimer
This bot is tailored for use within the [Brackeys Discord server](https://discord.gg/brackeys). While this bot is open source, and you are free to use it
in your own servers, you accept responsibility for any mishaps which may arise from the use of this software. Use at your own
risk.
