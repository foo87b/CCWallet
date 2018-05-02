using Discord;
using System;
using System.Collections.Generic;
using System.Text;

namespace CCWallet.DiscordBot.Utilities
{
    public static class DiscordReaction
    {
        public static readonly Emoji Denied     = new Emoji("\U0001f6ab"); // U+1F6AB is :no_entry_sign:
        public static readonly Emoji Error      = new Emoji("\u26a0");     // U+26A0  is :warning:
        public static readonly Emoji InProgress = new Emoji("\u23f3");     // U+23F3  is :hourglass_flowing_sand:
        public static readonly Emoji Unknown    = new Emoji("\u2753");     // U+2753  is :question:
    }
}
