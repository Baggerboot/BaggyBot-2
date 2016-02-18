
BaggyBot is an IRC bot built for collecting statistics in an IRC channel.
He also has an elaborate command system, providing features such as searching
Wolfram Alpha and Urban Dictionary, interactive interpreters for Python and C#,
performing DNS queries, and many others, allowing him to additionally function
as a general 'utility' IRC bot.
He also has a Slack compatibility mode, allowing him to connect to a Slack IRC
server and handling it as if it's a regular IRC server (by stripping the URLs
Slack adds to messages and translating IRC formatting codes to Slack formatting
codes.

Features
--------

His main feature is collecting statistics about each user, as well as the channel as a whole.
Nearly all IRC activity is logged to a database, which means the bot can also be used for logging IRC history.
Furthermore, specific statistics are kept about each user, such as:
 - Total amount of lines sent
 - Total amount of words sent
 - Total amount of actions (/me <action>)
 - Total amount of swears
 
In addition, statistics about individual words, emoticons, username references (pings) and URLS are stored as well; 
generally containing information about how often the word/username/emoticon/URL has been encountered,
as well as the name of the last user who said it.

Finally, BaggyBot is able to grab random quotes from users, and store these in the database as well.

He's meant to be used with a script (or website) that retrieves those statistics from the database and visualises them.

Building
--------
As this repository uses submodules, you should, after cloning this repository, first set them up. 
Navigate to the repository root, then run `git submodule init` and then clone the submodule
repositories with `git submodule update`.

Occasionally, as the submodules get updated, you might have to run that command again.

To build BaggyBot from VS, no additional steps are required. To build him from commandline,
you'll have to restore all referenced packages first. In the repository root, run `nuget restore`.

On Windows, you can simply build the project with msbuild. On Linux, however (and Windows too, if you
want to use xbuild instead), you'll have to build `baggybot-mono.sln` instead, which references
`baggybot-mono.csproj`, which differs from `baggybot.csproj` in that the version number generation
task is removed, as xbuild does not support this task. 

An easy (linux-only) way to run this task is by running `build.sh`, which generates the right
version number, inserts it into `Version.cs`, and then builds `baggybot-mono.sln` with xbuild under
the release configuration.

TODO
--------

 - ~~Improve the help command. Help information is currently stored in the Help command's class.
   It is better to store help for each command in the class belonging to that command, and 
   have the help command grab the correct help information from that class.~~ **(Done)**
 - Create a generic argument parsing class to help with efficient, exception-less argument parsing
   for commands (ideally this should be done for process arguments as well). **(Testing)**
 - Improve user matching code. It should be less reluctant to try a NickServ match, and more conservative
   when determining whether a user matches another, already known user.
 - Better finetuning of permissions. Allow specifying multiple bot operators, and allow specifying permissions
   on a per-channel, per-command basis, overriding the default permissions for each command.
 - Additionally, create another permission level for channel operators, and give them access to a few more
   commands if it makes sense.
 - ~~Allow connecting to multiple servers simultaneously.~~ **(Done)**
