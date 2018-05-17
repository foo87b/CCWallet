using CCWallet.DiscordBot.Utilities.Preference;
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;

namespace CCWallet.DiscordBot.Services
{
    public class PreferenceService
    {
        private Uri BaseUri { get; }
        private Dictionary<ulong, Group> GroupCaches { get; } = new Dictionary<ulong, Group>();
        private Dictionary<ulong, Guild> GuildCaches { get; } = new Dictionary<ulong, Guild>();
        private Dictionary<ulong, User> UserCaches { get; } = new Dictionary<ulong, User>();

        public PreferenceService(ConfigureService configure)
        {
            BaseUri = new Uri(configure.GetString("CCWALLET_API").TrimEnd('/'), UriKind.Absolute);

            Group.PreferenceService = this;
            Guild.PreferenceService = this;
            User.PreferenceService = this;
        }

        public Group GetGroupPreference(ulong id)
        {
            if (!GroupCaches.ContainsKey(id))
            {
                var endpoint = new UriBuilder(BaseUri);
                endpoint.Path += $"/group/{id}";

                try
                {
                    GroupCaches[id] = FetchAsync<Group>(endpoint.Uri).Result;
                }
                catch (AggregateException e) when (e.InnerExceptions.Count == 1 && e.InnerExceptions[0] is WebException)
                {
                    GroupCaches[id] = new Group() { GroupId = id };
                }
            }

            return GroupCaches[id];
        }

        public Guild GetGuildPreference(ulong id)
        {
            if (!GuildCaches.ContainsKey(id))
            {
                var endpoint = new UriBuilder(BaseUri);
                endpoint.Path += $"/guild/{id}";

                try
                {
                    GuildCaches[id] = FetchAsync<Guild>(endpoint.Uri).Result;
                }
                catch (AggregateException e) when (e.InnerExceptions.Count == 1 && e.InnerExceptions[0] is WebException)
                {
                    GuildCaches[id] = new Guild() {GuildId = id};
                }
            }

            return GuildCaches[id];
        }

        public User GetUserPreference(ulong id)
        {
            if (!UserCaches.ContainsKey(id))
            {
                var endpoint = new UriBuilder(BaseUri);
                endpoint.Path += $"/user/{id}";

                try
                {
                    UserCaches[id] = FetchAsync<User>(endpoint.Uri).Result;
                }
                catch (AggregateException e) when (e.InnerExceptions.Count == 1 && e.InnerExceptions[0] is WebException)
                {
                    UserCaches[id] = new User() { UserId = id };
                }
            }

            return UserCaches[id];
        }

        public void Update<T>(T preference) where T : class, AWSLambda.Entities.IDynamoTable
        {
            var endpoint = new UriBuilder(BaseUri);

            switch (preference)
            {
                case Group _:
                    endpoint.Path += $"/group/{preference.GetHashKey()}";
                    break;

                case Guild _:
                    endpoint.Path += $"/guild/{preference.GetHashKey()}";
                    break;

                case User _:
                    endpoint.Path += $"/user/{preference.GetHashKey()}";
                    break;

                case AWSLambda.Entities.CommandLog _:
                    endpoint.Path += $"/log/{preference.GetHashKey()}";
                    break;

                default:
                    throw new ArgumentException();
            }

            PostAsync(endpoint.Uri, preference).Wait();
        }

        private async Task<T> FetchAsync<T>(Uri uri) where T : class
        {
            var request = WebRequest.Create(uri);
            var response = await request.GetResponseAsync();

            using (var stream = response.GetResponseStream())
            {
                var serializer = new DataContractJsonSerializer(typeof(T), new DataContractJsonSerializerSettings()
                {
                    UseSimpleDictionaryFormat = true,
                });

                return serializer.ReadObject(stream) as T;
            }
        }

        private async Task<WebResponse> PostAsync<T>(Uri uri, T param) where T : class
        {
            var request = WebRequest.Create(uri);
            request.Method = WebRequestMethods.Http.Post;
            request.ContentType = "application/json";
            
            using (var stream = request.GetRequestStream())
            {
                new DataContractJsonSerializer(typeof(T), new DataContractJsonSerializerSettings()
                {
                    UseSimpleDictionaryFormat = true,
                }).WriteObject(stream, param);
            }

            return await request.GetResponseAsync();
        }
    }
}
