# Byond Hub [![Build Status](https://travis-ci.org/bo20202/ByondHub.svg?branch=master)](https://travis-ci.org/bo20202/ByondHub)

A hub for managing SS13 Byond servers. Shipped with Discord bot🤖

## Getting Started

Create `config.json` in ByondHub and ByondHub.DiscordBot directories and copy contents of `config.example.json` from these directories to it.
Then run Hub and Bot with `dotnet run` in corresponding directories. You can get authentication code for your bot [here](https://discordapp.com/developers/applications/me)

### Prerequisites

You will need: 

* .NET Core v2.1.4
* BYOND

### Installing

A step by step series of examples that tell you have to get a development env running:

Clone the repository

```
git clone https://github.com/bo20202/ByondHub.git
```

Install Nuget dependencies

```
dotnet restore
```

Choose project (Bot or Hub)

```
cd ByondHub/ByondHub.DiscordBot
```

And run!

```
dotnet run
```

Remember, if you start Bot you should also start Hub.

Try to send some commands to Bot.
```
%prefix from config%server %id% start
```

## Running the tests

TODO

## Deployment

Run `dotnet publish` inside of ByondHub and ByondHub.Bot

## Built With

* [ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/) - The main web framework.
* [Discord.Net](https://github.com/RogueException/Discord.Net) - Discord API framework.
* [Libgit2Sharp](https://github.com/libgit2/libgit2sharp) - Used to update builds.

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details
