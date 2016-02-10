
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

He's meant to be used with a script (or website) that retrieves those statistics from the database and visualises them.

Building
--------
The build process is fairly straighforward. However, because this repository uses submodules,
you should, after cloning this repository, first set them up. Navigate to the repository root,
then run `git submodule init` and then clone the submodule repositories with `git submodule update`.

Occasionally, as the submodules get updated, you might have to run that command again.

To build BaggyBot from VS, no additional steps are required. To build him from commandline,
you'll have to restore all referenced packages first. In the repository root, run `nuget restore`.

Now you can build the project, running either `xbuild` or `msbuild`, depending on whether you want
to build the project using the Mono or the Microsoft compiler. No additional arguments are required.

To build for the release configuration, run `(x/ms)build /p:Configuration=Release`

TODO
--------

 - ~~Improve the help command. Help information is currently stored in the Help command's class.
   It is better to store help for each command in the class belonging to that command, and 
   have the help command grab the correct help information from that class.~~ **(Done)**
 - Create a generic argument parsing class to help with efficient, exception-less argument parsing
   for commands (ideally this should be done for process arguments as well). **(Needs more work)**
 - Improve user matching code. It should be less reluctant to try a NickServ match, and more conservative
   when determining whether a user matches another, already known user.
 - Better finetuning of permissions. Allow specifying multiple bot operators, and allow specifying permissions
   on a per-channel, per-command basis, overriding the default permissions for each command.
 - Additionally, create another permission level for channel operators, and give them access to a few more
   commands if it makes sense.
 - Allow connecting to multiple servers simultaneously.
