-- Update the word count for all users
WITH word_count_per_user AS (
	SELECT sender, count(*) as word_count
	FROM (
		SELECT sender, regexp_split_to_table(message, '\s') as word
		FROM irc_log
		-- If you wish to do exclude/include any specific kinds of messages,
		-- you can do so here. For example:
		-- WHERE channel = '#channel'
	) t
	JOIN irc_user ON irc_user.id = sender
	GROUP BY sender
	order by word_count desc
)
UPDATE user_statistic
SET words = (
	SELECT word_count
	FROM word_count_per_user
	WHERE sender = user_statistic.user_id
)
