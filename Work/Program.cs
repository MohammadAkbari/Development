using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.Sql;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading;

namespace Work
{
    class Program
    {
        const string KEY = "session";
        const int EXPIRY_TIME = 60000;

        static void Main(string[] args)
        {
            EfSample();

            Console.ReadKey();
        }

        private static void Accounting()
        {
            var connectionMultiplexer = ConnectionMultiplexer.Connect("192.168.20.82:6379,abortConnect=false,syncTimeout=10000");
            IDatabase redis = connectionMultiplexer.GetDatabase(15);

            for (int i = 1; i < 10; i++)
            {
                redis.HashSetAsync($"content:{i}", new HashEntry[] {
                    new HashEntry("click", i*100),
                    new HashEntry("impression", i*10),
                    new HashEntry("advertiser", $"a-{i}"),
                    new HashEntry("campaign",  $"c-{i}"),
                });
            }

            for (int i = 1; i < 10; i++)
            {
                redis.StringSet($"advertiser:a-{i}", i * 1000000);
            }

            AddMassiveData(redis);

            Thread.Sleep(EXPIRY_TIME);

            AddLittleData(redis);

            RemoveExpiredData(redis);
        }

        private static void RemoveExpiredData(IDatabase redis)
        {
            var start = 0;
            var stop = (long)TimeSpan.FromTicks(DateTime.Now.AddSeconds(-EXPIRY_TIME).Ticks).TotalSeconds;

            var items = redis.SortedSetRangeByScore(KEY, start, stop);

            var values = items.Select(e => ToRedisValue($"session:{e}:content:*")).ToArray();

            redis.ScriptEvaluate(@"
                for nameCount = 1, #ARGV do
                    local keys = redis.call('keys', ARGV[nameCount]) 
                    for i=1,#keys,5000 do 
                        redis.call('del', unpack(keys, i, math.min(i+4999, #keys)))
                    end
                end", values: values);
        }

        private static void AddLittleData(IDatabase redis)
        {
            for (int i = 1; i < 50; i++)
            {
                var contentId = i % 79;
                var sessionKey = 6;
                var price = i % 11 * 10;

                AddToRedis(redis, sessionKey, contentId);
            }
        }

        private static void AddMassiveData(IDatabase redis)
        {
            Random random = new Random();


            for (int i = 1; i < 500000; i++)
            {
                var contentId = random.Next(1, 10);

                var sessionKey = i % 5;

                AddToRedis(redis, sessionKey, contentId);
            }
        }

        private static void AddToRedis(IDatabase redis, int sessionKey, int contentId)
        {
            var value = redis.StringGet($"session:{sessionKey}:content:{contentId}");

            var redisValues = redis.HashGet($"content:{contentId}",
                new RedisValue[] {
                    "click",
                    "advertiser",
                    "campaign"
                });

            int newPrice = (int)redisValues[0];
            string advertiserId = redisValues[1];
            string campaignId = redisValues[2];

            var oldPrice = value.HasValue? (int)value : 0;

            var difference = newPrice - oldPrice;

            if (difference > 0)
            {
                redis.StringIncrement($"session:{sessionKey}:content:{contentId}", difference);

                redis.StringDecrement($"advertiser:{advertiserId}", difference);

                redis.StringIncrement($"campaign:{campaignId}", difference);
            }

            var now = DateTime.Now;
            var totalSeconds = (long)TimeSpan.FromTicks(now.Ticks).TotalSeconds;
            redis.SortedSetAdd(KEY, sessionKey, totalSeconds);
        }

        private static void StructVsClass()
        {
            Class1 c1 = new Class1
            {
                Name = "Class"
            };

            Struct1 s1;
            s1.Name = "Struct";
            Console.WriteLine(s1.Name);

            Struct1 s2 = s1;
            Class1 c2 = c1;

            c2.Name = "Class Changed";

            Console.WriteLine(c1.Name);
            Console.WriteLine(c2.Name);

            s2.Name = "Struct Changed";

            Console.WriteLine(s1.Name);
            Console.WriteLine(s2.Name);
        }

        private static void EfSample()
        {
            using (var db = new ApplicationDbContext())
            {
                //AddCampaign(db, 1);
                //AddCampaign(db, 4);

                //db.SaveChanges();

                var ts = new TimeSpan(12, 0, 0);

                var list = db.Campaigns.Where(e => e.From.Add(e.Offset) < ts)
                    .Where(e => e.To.Add(e.Offset) > ts)
                    //.WithSqlTweaks()
                    .ToList();
            }

            for (int i = 1; i < 2; i++)
            {
                using (var db = new ApplicationDbContext())
                {
                    var events = db.Events.Take(5).ToList();

                    var random = new Random();

                    var value1 = new Value()
                    {
                        Name = random.Next(100000, 200000).ToString()
                    };

                    var value2 = new Value()
                    {
                        Name = random.Next(100000, 200000).ToString()
                    };

                    var @event = new Event
                    {
                        Guid = Guid.NewGuid(),
                        Price = random.Next(10, 20),
                        Value1 = value1,
                        Value2 = value2,
                        DayOfWeek = DayOfWeek.Friday
                    };

                    db.Events.Add(@event);

                    db.SaveChanges();

                    if (i % 10 == 0)
                    {
                        Console.WriteLine(DateTime.Now);
                    }
                }
            }
        }

        private static void AddCampaign(ApplicationDbContext db, int month)
        {
            DateTime dateTime = new DateTime(2019, month, 10, 8, 0, 0);

            var campaign = new Campaign
            {
                From = dateTime.ToUniversalTime().TimeOfDay,
                To = dateTime.AddHours(8).ToUniversalTime().TimeOfDay,
                Offset = new DateTimeOffset(dateTime).Offset,
                Date = DateTime.UtcNow
            };

            db.Campaigns.Add(campaign);
        }

        public static RedisValue ToRedisValue(string v) => v;
    }

    #region MyRegion

    public class ApplicationDbContext : DbContext
    {
        public DbSet<Event> Events { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Campaign> Campaigns { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder
                .UseSqlServer(@"Data Source=.;Initial Catalog=ConsoleDb;Integrated Security=true;TrustServerCertificate=True;MultipleActiveResultSets=true")
                .ReplaceService<INodeTypeProviderFactory, CustomMethodInfoBasedNodeTypeRegistryFactory>()
                .ReplaceService<ISelectExpressionFactory, CustomSelectExpressionFactory>()
                .ReplaceService<IQuerySqlGeneratorFactory, CustomSqlServerQuerySqlGeneratorFactory>(); ;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ValueConfiguration());
            modelBuilder.ApplyConfiguration(new CampaignConfiguration());

            base.OnModelCreating(modelBuilder);
        }
    }

    internal class CampaignConfiguration : IEntityTypeConfiguration<Campaign>
    {
        public void Configure(EntityTypeBuilder<Campaign> builder)
        {
            builder.Property(e => e.Date)
                   .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
        }
    }

    public class Campaign
    {
        public int Id { get; set; }

        public TimeSpan From { get; set; }

        public TimeSpan To { get; set; }

        public TimeSpan Offset { get; set; }

        //[DateTimeKind]
        public DateTime Date { get; set; }
    }

    public class Post
    {
        public int PostId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }

        public string AuthorUserId { get; set; }
        public User Author { get; set; }

        public string ContributorUserId { get; set; }
        public User Contributor { get; set; }
    }

    public class User
    {
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        [InverseProperty(nameof(Post.Author))]
        public List<Post> AuthoredPosts { get; set; }

        [InverseProperty(nameof(Post.Contributor))]
        public List<Post> ContributedToPosts { get; set; }
    }

    public class Event
    {
        public Event()
        {
            CreatedOn = DateTime.Now;
        }

        public int Id { get; set; }

        public decimal Price { get; set; }

        public Guid Guid { get; set; }

        public DateTime CreatedOn { get; private set; }

        public Value Value1 { get; set; }
        public Value Value2 { get; set; }

        public DayOfWeek? DayOfWeek { get; set; }
    }

    public class ValueRelation
    {
        public int Id { get; set; }

        public string Description { get; set; }
    }

    public class Value
    {
        public int? ValueRelationId { get; set; }

        public ValueRelation ValueRelation { get; set; }

        public string Name { get; set; }
    }

    public class ValueConfiguration : IEntityTypeConfiguration<Event>
    {
        public void Configure(EntityTypeBuilder<Event> builder)
        {
            builder.OwnsOne(e => e.Value1);
            builder.OwnsOne(e => e.Value2);
            builder.Property(e => e.DayOfWeek)
                .HasConversion<string>();
        }
    }

    class SimpleClass
    {
        static readonly long baseline;

        public SimpleClass()
        {

        }

        static SimpleClass()
        {
            baseline = DateTime.Now.Ticks;
        }
    }

    public static class StaticClass
    {
        static StaticClass()
        {

        }
    }

    public class Class1
    {
        public string Name{ get; set; }
}

    public struct Struct1
    {
        public string Name; // { get; set; }
    }

    #endregion
}
