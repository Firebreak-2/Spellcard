using Discord.WebSocket;

namespace Spellcard
{
    public static class Extensions
    {
        public static T RandomItem<T>(this IEnumerable<T> collection) => collection.ToArray()[LaresUtils.Random.Int(0, collection.Count() - 1)];
        public static SocketGuild GetGuild(this ISocketMessageChannel channel) => Program.Client.Guilds.First(x => x.Channels.Any(y => y.Id == channel.Id));
    }
}
