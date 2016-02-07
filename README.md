
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

TODO
--------

 - Improve the help command. Help information is currently stored in the Help command's class.
   It is better to store help for each command in the class belonging to that command, and 
   have the help command grab the correct help information from that class.
 - Create a generic argument parsing class to help with efficient, exception-less argument parsing
   for commands (ideally this should be done for process arguments as well).
 - Improve user matching code. It should be less reluctant to try a NickServ match, and more conservative
   when determining whether a user matches another, already known user.
 - Better finetuning of permissions. Allow specifying multiple operators, and allow specifying permissions
   on a per-channel, per-command basis, overriding the default permissions for each command.
 - Additionally, create another permission level for channel operators, and give them access to a few more
   commands if it makes sense.

