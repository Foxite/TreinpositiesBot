# TreinpositiesBot
Whenever someone says 3 or more digits, this bot will post a random image of a vehicle with that number on [Treinposities](https://treinposities.nl/).

It ignores numbers inside urls, mentions, and words, and understands spaces within the numbers.

If you block the bot, it won't reply to your messages.

## Docker deployment
Dockerfile in TreinpositiesBot folder, no additional dependencies.

Configuration is present in `/app/appsettings.json`. You may either mount your own file, or [use envvars to override items in the configuration](https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration-providers#environment-variable-configuration-provider). Example:
- Core__DiscordToken
- Treinposities__BlockedPhotographers__0
- Core__SourcesByGuild__805008823081107467__0

Consider restricting the bot to specific channels because my users *love* spamming this thing.
