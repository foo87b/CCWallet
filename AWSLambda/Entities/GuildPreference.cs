using Amazon.DynamoDBv2.DataModel;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CCWallet.AWSLambda.Entities
{
    [DataContract, DynamoDBTable("ccwallet.preference_guilds")]
    public class GuildPreference : IPreference
    {
        [DataMember, DynamoDBHashKey("id")]
        public virtual ulong GuildId { get; set; }

        [DataMember, DynamoDBProperty("updated")]
        public virtual ulong LastUpdate { get; set; }

        [DataMember, DynamoDBProperty("language")]
        public virtual Dictionary<string, string> Languages { get; set; }

        public ulong GetHashKey() => GuildId;
    }
}
