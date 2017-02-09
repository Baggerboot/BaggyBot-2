
BaggyBot is an chat bot built for collecting statistics in one or multile channels.
He also has an elaborate command system, providing features such as searching
the internet for websites or images; querying Wolfram Alpha, Urban Dictionary,
and Wikipedia; interactive interpreters for Python and C#;
performing DNS queries; and many others, allowing him to additionally function
as a general 'utility' bot.

Originally developed as an IRC bot, BaggyBot now has an abstraction layer
and plugin system that allows him to connect to all kinds of servers.
Plugins for Slack, Discord and Curse are already implemented, and adding
new plugins is relatively simple.

Features
--------

His main feature is collecting statistics about each user, as well as the channel as a whole.
Nearly all IRC activity is logged to a database, which means the bot can also be used for logging IRC history.
Furthermore, specific statistics are kept and generated about each user, such as:
 - Total amount of lines sent
 - Total amount of words sent
 - Total amount of actions (/me <action>)
 - Total amount of swears
 
In addition, statistics about individual words, emoticons, username references (pings) and URLS are stored as well; 
generally containing information about how often the word/username/emoticon/URL has been encountered,
as well as the name of the last user who said it.

Finally, BaggyBot is able to grab random quotes from users, and store these in the database as well.

He's meant to be used with a script (or website (such as https://github.com/Baggykiin/baggybot-stats)) that retrieves those statistics from the database and visualises them.

Building
--------
As this repository uses submodules, you should, after cloning this repository, first set them up. 
Navigate to the repository root, then run `git submodule init` and then clone the submodule
repositories with `git submodule update`.

Occasionally, as the submodules get updated, you might have to run that command again.

To build BaggyBot from VS, no additional steps are required. To build him from commandline,
you'll have to restore all referenced packages first. In the repository root, run `nuget restore`.

