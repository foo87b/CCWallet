using Amazon.DynamoDBv2.DataModel;
using System.Runtime.Serialization;

namespace CCWallet.AWSLambda.Entities
{
    [DataContract, DynamoDBTable("ccwallet.preference_users")]
    public class UserPreference : IPreference
    {
        [DataMember, DynamoDBHashKey("id")]
        public virtual ulong UserId { get; set; }

        [DataMember, DynamoDBProperty("updated")]
        public virtual ulong LastUpdate { get; set; }

        [DataMember, DynamoDBProperty("language")]
        public virtual string Language { get; set; }

        public ulong GetHashKey() => UserId;
    }
}
