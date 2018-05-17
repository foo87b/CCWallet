using Amazon.DynamoDBv2.DataModel;
using System.Runtime.Serialization;

namespace CCWallet.AWSLambda.Entities
{
    [DataContract, DynamoDBTable("ccwallet.preference_groups")]
    public class GroupPreference : IPreference
    {
        [DataMember, DynamoDBHashKey("id")]
        public virtual ulong GroupId { get; set; }

        [DataMember, DynamoDBProperty("updated")]
        public virtual ulong LastUpdate { get; set; }

        [DataMember, DynamoDBProperty("language")]
        public virtual string Language { get; set; }

        public ulong GetHashKey() => GroupId;
    }
}
