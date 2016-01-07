Table: UserCombinations

int		combi_id	(autoincrement)
int		user_id
string	nicks
string	ident
string	hostmask
string	nickserv

Priority:
1: nickserv		A nickserv match is treated as a guaranteed match.
2: ident		An ident match is not a guaranteed match, 
				as idents like ~webchat and ~quassel are often used.
3: nick 		People tend to faff about.
4: hostmask		A lot of people use bouncers, so they share a hostmask.

How to match:
If ident, nick and hostmask match, it is treated as a guaranteed match.
Worst-case scenario:
-webchat
-yes
-irc.squid.li
Highly unlikely as people who use a bouncer generally do not use webchat. 
Even if there is a match on webchat or another ident, the nickname will 
generally be unique.
On the rare occasion that this is not the case, nothing will break as
combinations are stored as loose groups of data, instead of keeping track
of the nicks each person has used.
Amount of invalid data added: minimal

If nick and ident match, it is treated as a guaranteed match.
Worst-case scenario:
-webchat
-yes
Rather unlikely, though possible.
Amount of invalid data added: minimal

If there are no matches on nick and ident, the bot will first attempt to 
check for NickServ matches. If a NickServ match is found, it's treated as 
a guaranteed match. Otherwise, the bot will attempt to create a new user.


Table: Userstats

int 		user_id
int 		lines
int 		words
