# TreinpositiesBot
Whenever someone says 3 or more digits, this bot will post a random image of a vehicle with that number on [Treinposities](https://treinposities.nl/).

It ignores numbers inside urls, mentions, and words, and understands spaces within the numbers.

If you block the bot, it won't reply to your messages.

## Docker deployment
Dockerfile in TreinpositiesBot folder, no additional dependencies.
Envvars (optional unless stated):
- BOT_TOKEN (required)
- BLOCKED_PHOTOGRAPHERS (semicolon separated list of usernames whose photos should never be posted)
- WEBHOOK_URL (error reports are sent here)
- COOLDOWN_SECONDS (default: 60)
- NO_RESULTS_EMOTE (reaction added if there are no photos)

Consider restricting the bot to specific channels because my users *love* spamming this thing.
