using Discord;

namespace CCWallet.DiscordBot.Utilities.Discord
{
    public static class BotReaction
    {
        public static readonly Emoji Denied     = new Emoji("\U0001f6ab"); // U+1F6AB is :no_entry_sign:
        public static readonly Emoji Error      = new Emoji("\u26a0");     // U+26A0  is :warning:
        public static readonly Emoji InProgress = new Emoji("\u23f3");     // U+23F3  is :hourglass_flowing_sand:
        public static readonly Emoji Unknown    = new Emoji("\u2753");     // U+2753  is :question:
        public static readonly Emoji Failure    = new Emoji("\u274e");     // U+274E  is :negative_squared_cross_mark:
        public static readonly Emoji Success    = new Emoji("\u2705");     // U+2705  is :white_check_mark:
        public static readonly Emoji RateLimited  = new Emoji("\u23f1");     // U+23F1  is :stopwatch:
    }
}
