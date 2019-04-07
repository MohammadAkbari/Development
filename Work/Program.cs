using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
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
        static void Main(string[] args)
        {
            var connectionMultiplexer = ConnectionMultiplexer.Connect("192.168.20.82:6379,abortConnect=false,syncTimeout=10000");
            IDatabase redis = connectionMultiplexer.GetDatabase(15);
            var key = "session";

            for (int i = 1; i < 500000; i++)
            {
                var contentId = i % 79;
                var sessionKey = i % 5;
                var price = i % 11 * 10;
                var value = redis.StringGet($"session:{sessionKey}:content:{contentId}");

                if (!value.HasValue || price > (int)value)
                {
                    redis.StringSet($"session:{sessionKey}:content:{i}", price);
                }

                var now = DateTime.Now;
                var totalSeconds = (long)TimeSpan.FromTicks(now.Ticks).TotalSeconds;
                redis.SortedSetAdd(key, sessionKey, totalSeconds);
            }

            Thread.Sleep(60000);

            for (int i = 1; i < 50; i++)
            {
                var contentId = i % 79;
                var sessionKey = 6;
                var price = i % 11 * 10;
                var value = redis.StringGet($"session:{sessionKey}:content:{contentId}");

                if (!value.HasValue || price > (int)value)
                {
                    redis.StringSet($"session:{sessionKey}:content:{i}", price);
                }

                var now = DateTime.Now;
                var totalSeconds = (long)TimeSpan.FromTicks(now.Ticks).TotalSeconds;
                redis.SortedSetAdd(key, sessionKey, totalSeconds);
            }

            var start = 0;
            var stop = (long)TimeSpan.FromTicks(DateTime.Now.AddSeconds(-30).Ticks).TotalSeconds;

            var items = redis.SortedSetRangeByScore(key, start, stop);

            var values = items.Select(e => ToRedisValue($"session:{e}:content:*")).ToArray();

            redis.ScriptEvaluate(@"
                for nameCount = 1, #ARGV do
                    local keys = redis.call('keys', ARGV[nameCount]) 
                    for i=1,#keys,5000 do 
                        redis.call('del', unpack(keys, i, math.min(i+4999, #keys)))
                    end
                end", values: values);
            
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

                    if(i%10 == 0)
                    {
                        Console.WriteLine(DateTime.Now);
                    }
                }
            }

            //Class1 c1 = new Class1
            //{
            //    Name = "Class"
            //};

            //Struct1 s1;
            //s1.Name = "Struct";
            //Console.WriteLine(s1.Name);


            //Struct1 s2 = s1;
            //Class1 c2 = c1;

            //c2.Name = "Class Changed";

            //Console.WriteLine(c1.Name);
            //Console.WriteLine(c2.Name);

            //s2.Name = "Struct Changed";

            //Console.WriteLine(s1.Name);
            //Console.WriteLine(s2.Name);

            SimpleClass c1 = new SimpleClass();
            SimpleClass c2 = new SimpleClass();

            Console.ReadKey();
        }

        public static RedisValue ToRedisValue(string v) => v;
    }

    public class ApplicationDbContext : DbContext
    {
        public DbSet<Event> Events { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.UseSqlServer(@"Data Source=.;Initial Catalog=ConsoleDb;Integrated Security=true;TrustServerCertificate=True;MultipleActiveResultSets=true");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ValueConfiguration());

            base.OnModelCreating(modelBuilder);
        }
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

    public class Value
    {
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
}
