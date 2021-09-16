using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.CommandLineUtils;

namespace PDUserList
{
    public class Team
    {
        public string id { get; set; }
        public string type { get; set; }
        public string summary { get; set; }
        public string self { get; set; }
        public string html_url { get; set; }
    }

    public class ContactMethod
    {
        public string id { get; set; }
        public string type { get; set; }
        public string summary { get; set; }
        public string self { get; set; }
        public object html_url { get; set; }
    }

    public class NotificationRule
    {
        public string id { get; set; }
        public string type { get; set; }
        public string summary { get; set; }
        public string self { get; set; }
        public object html_url { get; set; }
    }

    public class User
    {
        public string name { get; set; }
        public string email { get; set; }
        public string time_zone { get; set; }
        public string color { get; set; }
        public string avatar_url { get; set; }
        public bool billed { get; set; }
        public string role { get; set; }
        public object description { get; set; }
        public bool invitation_sent { get; set; }
        public object job_title { get; set; }
        public List<Team> teams { get; set; }
        public List<ContactMethod> contact_methods { get; set; }
        public List<NotificationRule> notification_rules { get; set; }
        public List<object> coordinated_incidents { get; set; }
        public string id { get; set; }
        public string type { get; set; }
        public string summary { get; set; }
        public string self { get; set; }
        public string html_url { get; set; }
    }

    public class OneUser
    {
        public User user { get; set; }
    }

    public class UserReplyList
    {
        public List<User> users { get; set; }
        public int limit { get; set; }
        public int offset { get; set; }
        public object total { get; set; }
        public bool more { get; set; }
    }


    class Program
    {
        private static readonly HttpClient client = new HttpClient();

        static async Task<int> Main(string[] args)
        {
            // Description of the application
            var app = new CommandLineApplication()
            {
                Name = "PDUserList.exe",
                FullName = "PDUserList",
                Description = "PagerDuty User AddressBook"
            };

            // Handle help and version arguments
            // You can declare alias using "|"
            app.HelpOption("-?|-h|--help");
            app.VersionOption("--version", "1.0.0");

            // The first argument is the option template.
            // It starts with a pipe-delimited list of option flags/names to use
            // Optionally, It is then followed by a space and a short description of the value to specify.
            // e.g. here we could also just use "-o|--option"
            var userOption = app.Option("-u|--user <optionvalue>",
                    "Some option value",
                    CommandOptionType.SingleValue);

            client.DefaultRequestHeaders.Accept.Clear();

            client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation UserList Reporter");
            client.DefaultRequestHeaders.Add("Accept", "application/vnd.pagerduty+json;version=2");
            client.DefaultRequestHeaders.Add("Authorization", "Token token=y_NbAkKc66ryYTWUXYEu");

            var user0 = await GetUser("PG7TXJ8");

            // Code of the console application when there is no argument
            app.OnExecute(async () =>
            {
                // Use the HasValue() method to check if the option was specified
                if (userOption.HasValue())
                {
                    Console.WriteLine("User details, key: {0}", userOption.Value());

                    var user = await GetUser(userOption.Value());
                    Console.WriteLine($"{user.name}");

                    foreach(var contactMethod in user.contact_methods)
                    {
                        Console.WriteLine($"{contactMethod.id} {contactMethod.type}");
                    }
                    Console.WriteLine();
                }
                else
                {
                    var userList = ProcessUserLoop();

                    await foreach(var user in userList)
                    {
                        Console.WriteLine($"{user.id} {user.name}");
                    }
                }

                return 0;
            });

            // Parse the command line and execute the right code
            try
            {
                app.Execute(args);
                return 0;
            }
            catch (CommandParsingException ex)
            {
                Console.WriteLine(ex.Message);
                app.ShowHelp();
            }

            return 0;
        }

        private static async IAsyncEnumerable<User> ProcessUserLoop()
        {
            int offset = 0;
            int limit = 10;
            while (true)
            {
                var userReplyList = await ProcessUserList(offset, limit);

                foreach (var user in userReplyList.users)
                {
                    yield return user;
                }

                if (userReplyList.more == false)
                    break;

                offset += limit;
            }
        }

        private static async Task<UserReplyList> ProcessUserList(int offset, int limit)
        {
            var queryRequest = $"https://api.pagerduty.com/users?total=true&offset={offset}&limit={limit}";

            var streamTask = client.GetStreamAsync(queryRequest);
            var repositories = await JsonSerializer.DeserializeAsync<UserReplyList>(await streamTask);
            return repositories;
        }

        private static async Task<User> GetUser(string key)
        {
            var queryRequest = $"https://api.pagerduty.com/users/{key}";

            var streamTask = client.GetStreamAsync(queryRequest);

            //StreamReader reader = new StreamReader(streamTask);
            //string text = reader.ReadToEnd();

            var oneUser = await JsonSerializer.DeserializeAsync<OneUser>(await streamTask);
            return oneUser.user;
        }
    }
}
