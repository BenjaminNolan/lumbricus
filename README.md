# Lumbricus

Lumbricus is a modular IRC bot written in C♯ which is designed to help channel
operators who manage several channels keep some semblance of sanity.

[Documentation](docs/index.md) for [users](docs/users.md) and
[plugin developers](docs/plugin_development.md) is located in the `docs/`
directory.

## Quick start

Create a database in your favourite rDBMS system and grant at least SELECT,
INSERT, UPDATE and DELETE on it to a user and password of your choosing.
Import the appropriate schema file from `db/schemata/` into your new DB.

    cd /path/to/where/you/want/lumbricus/to/live
    git clone https://github.com/TwoWholeWorms/lumbricus ./
    cd lumbricus
    xbuild /p:Configuration=Release Lumbricus.sln

Edit Lumbricus/bin/Release/Lumbricus.exe.config and configure the database
connection strings to point at your database server, etc.

Now run this command:

    mono Lumbricus/bin/Release/Lumbricus.exe

Congratulations, you got an error. :) Insert a server into the `servers` table,
and at least one channel for that server into `channels` and now run it again.
If you're still getting errors, join `#lumbricus` on `irc.freenode.net` and I'll
do my best to help you out.

## License

Lumbricus is licensed under the MIT License, the full terms of which are
available in the [LICENSE](LICENSE) file.
