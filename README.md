BaggyBot-2
==========

BaggyBot is an IRC bot built for collecting statistics in an IRC channel.
In addition, it provides various other features to the users.

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

Besides this, the bot has a few features unrelated to logging:
 - A C# and Python REPL (be VERY careful with this one -- users that you do not trust can abuse these to cause serious damage. Some basic security features have been implemented, but in the end, if someone wants to abuse these commands, they WILL succeed). This feature can be disabled with a config setting, to prevent abuse.
 - A simple editor for the settings file, allowing you to change bot settings trough IRC
 - ...


Long-term goals
---------------

 - Move away from DBLinq (It's a little buggy, and does not provide an implementation for certain operations)
 - Add the ability to update using an update command, without breaking the connection to the IRC server
 - ...
