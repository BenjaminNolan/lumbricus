# Lumbricus

Lumbricus is a modular IRC bot written in C♯ which is designed to help channel
operators who manage several channels keep some semblance of sanity.

[Documentation](docs/index.md) for [users](docs/users.md) and
[plugin developers](docs/plugin_development.md) is located in the `docs/`
directory.

## Quick start

Create a database in your favourite rDBMS system and grant at least `SELECT`,
`INSERT`, `UPDATE` and `DELETE` on it to a user and password of your choosing.
Import the appropriate schema file from `db/schemata/` into your new DB.

    cd /path/to/where/you/want/lumbricus/to/live
    git clone https://github.com/TwoWholeWorms/lumbricus ./
    cd lumbricus
    xbuild /p:Configuration=Release Lumbricus.sln

Edit `Lumbricus/bin/Release/Lumbricus.exe.config` and configure the database
connection strings to point at your database server, etc.

Now run this command:

    mono Lumbricus/bin/Release/Lumbricus.exe

Congratulations, you got an error. :) Insert a server into the `servers` table,
and at least one channel for that server into `channels` and now run it again.
If you're still getting errors, join `#lumbricus` on `irc.freenode.net` and I'll
do my best to help you out.

## TODO list

### General

* Split IrcConnection into several parts so it's more efficient and much less monolithic
* Consolidate the .isOp() methods from AbstractPlugin and AbstractCommand into one
* DRY up the plugin initialisation code for the core plugins
* Internationalisation
* Many, many, many other things I haven't thought of yet o.o

### Modules

#### Working

* HelpPlugin — Responds to !help with a URL (as spam is horrible, but I suppose I can add text help later on)
* InfoPlugin — Allows users to set, modify, and clear an info line about themselves in a database table
* IrcLogPlugin — Logs all lines to a database table
* SeenPlugin — Tracks users' first and last seen dates and times, and provides a command to extract that data (currently OPs only)
* TrackUserPlugin — Tracks user movements (eg, JOIN, PART, QUIT, NICK, etc)

#### In progress

* BotBanPlugin — Provides commands and mechanisms to ban users from using the bot at all
* BanInfoPlugin — Adds a !baninfo command which can be used to get info about a user's bans (which can be multitudinous)
* MugshotsPlugin — Allows users to set, modify, and clear a mugshot photo of themselves in a database table
* SearchLogPlugin — Adds a !searchlog command which searches the logs table created by IrcLogPlugin
* TrackBanPlugin — Tracks bans and unbans in a database table
* TrackKickPlugin — Tracks kicks in a DB table, adding the message to a ban if a user was just banned

#### Planned

* AlizePlugin — Fartificial Intelligence plugin which turns the bot into something you can have a fake conversation with
* TellPlugin — Provides a way to tell a user or channel a message at a specific time or when they're next seen by the bot

## License

Lumbricus is licensed under the MIT License, the full terms of which are
available in the [LICENSE](LICENSE) file.
