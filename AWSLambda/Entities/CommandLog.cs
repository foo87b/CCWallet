using Amazon.DynamoDBv2.DataModel;
using System.Runtime.Serialization;

namespace CCWallet.AWSLambda.Entities
{
    [DataContract, DynamoDBTable("ccwallet.log_commands")]
    public class CommandLog : IDynamoTable
    {
        [DataMember, DynamoDBHashKey("id")]
        public virtual ulong MessageId { get; set; }

        [DataMember, DynamoDBProperty("user")]
        public virtual ulong UserId { get; set; }

        [DataMember, DynamoDBProperty("guild")]
        public virtual ulong GuildId { get; set; }

        [DataMember, DynamoDBProperty("channel")]
        public virtual ulong ChannelId { get; set; }

        [DataMember, DynamoDBProperty("type")]
        public virtual int ChannelType { get; set; }

        [DataMember, DynamoDBProperty("prefix")]
        public virtual string Prefix { get; set; }

        [DataMember, DynamoDBProperty("module")]
        public virtual string Module { get; set; }

        [DataMember, DynamoDBProperty("command")]
        public virtual string Command { get; set; }

        [DataMember, DynamoDBProperty("input")]
        public virtual string Input { get; set; }

        [DataMember, DynamoDBProperty("reason")]
        public virtual string ErrorReason { get; set; }
        
        public ulong GetHashKey() => MessageId;
    }
}
