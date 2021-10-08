using Assignment4.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Assignment4.Entities
{
    public class KanbanContext : DbContext
    {

        public DbSet<Task> Tasks { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<User> Users { get; set; }

        public KanbanContext(DbContextOptions<KanbanContext> options) : base(options) {}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Task>()
                .Property(e => e.State)
                .HasConversion(new EnumToStringConverter<State>());
        }

        public static void Seed(KanbanContext context)
        {
            // Clear out the database
            context.Database.ExecuteSqlRaw("DELETE dbo.Tags");
            context.Database.ExecuteSqlRaw("DELETE dbo.Tasks");
            context.Database.ExecuteSqlRaw("DELETE dbo.TagTask");
            context.Database.ExecuteSqlRaw("DELETE dbo.Users");
            context.Database.ExecuteSqlRaw("DBCC CHECKIDENT ('dbo.Tasks', RESEED, 0)");
            context.Database.ExecuteSqlRaw("DBCC CHECKIDENT ('dbo.Users', RESEED, 0)");


            var users = new User[]{
                new User {  Name = "Filberto",  Email =  "fsnaden0@linkedin.com" },
                new User {  Name = "Tito",      Email =  "tzmitrichenko1@ycombinator.com" },
                new User {  Name = "Alfie",     Email =  "ahubback2@i2i.jp" },
                new User {  Name = "Conrad",    Email =  "cmclae3@phpbb.com" },
                new User {  Name = "Suzi",      Email =  "sflude4@networksolutions.com" },
                new User {  Name = "Wenonah",   Email =  "wbeecheno5@yahoo.co" },
                new User {  Name = "Matthias",  Email =  "mharfleet6@bbc.co" },
                new User {  Name = "Leupold",   Email =  "lstenton7@newyorker.com" },
                new User {  Name = "Margot",    Email =  "mextence8@kickstarter.com" },
                new User {  Name = "Gavrielle", Email =  "gstedall9@instagram.com" },
                new User {  Name = "Charlton",  Email =  "cacorsa@state.gov" },
                new User {  Name = "Carling",   Email =  "comahonyb@archive.org" },
                new User {  Name = "Pat",       Email =  "pbertomieuc@yolasite.com" },
                new User {  Name = "Ellynn",    Email =  "edonavand@shinystat.com" },
                new User {  Name = "Idalina",   Email =  "icorteze@livejournal.com" },
                new User {  Name = "Zea",       Email =  "znewboldg@rambler.ru" },
                new User {  Name = "Klara",     Email =  "kmarrowf@squidoo.com" },
                new User {  Name = "Angelique", Email =  "adarkh@msn.com" },
                new User {  Name = "Lola",      Email =  "lflahyi@yahoo.co" },
                new User {  Name = "Briana",    Email =  "btouretj@gmpg.org" },
                new User {  Name = "Sosanna",   Email =  "scurnowk@so-net" },
                new User {  Name = "Giorgio",   Email =  "gkenyonl@icio.us" },
                new User {  Name = "Shandra",   Email =  "sgaveym@rediff.com" },
                new User {  Name = "Nicholas",  Email =  "ngilfoylen@hostgator.com" },
                new User {  Name = "Yale",      Email =  "ysuteo@4shared.com" }
            };

            context.Users.AddRange(users);

            // Seed tasks
            var tasks = new Task[] {
                new Task { Title = "systemic",              AssignedTo = users[1],      Description = "Suspendisse potenti.",                                                   State = State.New },
                new Task { Title = "focus group",           AssignedTo = users[14],     Description = "Mauris lacinia sapien quis libero.",                                     State = State.New },
                new Task { Title = "middleware",            AssignedTo = users[22],     Description = "Etiam vel augue.",                                                       State = State.New },
                new Task { Title = "Object-based",          AssignedTo = users[17],     Description = "Suspendisse potenti.",                                                   State = State.New },
                new Task { Title = "Team-oriented",         AssignedTo = users[13],     Description = "Nam dui.",                                                               State = State.New },
                new Task { Title = "4th generation",        AssignedTo = users[8],      Description = "Quisque id justo sit amet sapien dignissim vestibulum.",                 State = State.New },
                new Task { Title = "web-enabled",           AssignedTo = users[21],     Description = "In hac habitasse platea dictumst.",                                      State = State.Active },
                new Task { Title = "support",               AssignedTo = users[1],      Description = "Curabitur in libero ut massa volutpat convallis.",                       State = State.Active },
                new Task { Title = "Streamlined",           AssignedTo = users[5],      Description = "Phasellus id sapien in sapien iaculis congue.",                          State = State.Active },
                new Task { Title =  "Progressive",          AssignedTo = users[3],      Description = "Vivamus tortor.",                                                        State = State.Active },
                new Task { Title =  "customer loyalty",     AssignedTo = users[7],      Description = "In hac habitasse platea dictumst.",                                      State = State.Active },
                new Task { Title =  "leverage",             AssignedTo = users[13],     Description = "Suspendisse ornare consequat lectus.",                                   State = State.Active },
                new Task { Title =  "data-warehouse",       AssignedTo = users[1],      Description = "Sed vel enim sit amet nunc viverra dapibus.",                            State = State.Removed },
                new Task { Title =  "knowledge user",       AssignedTo = users[10],     Description = "Curabitur in libero ut massa volutpat convallis.",                       State = State.Removed },
                new Task { Title =  "Robust",               AssignedTo = users[8],      Description = "Cras mi pede, malesuada in, imperdiet et, commodo vulputate, justo.",    State = State.Removed },
                new Task { Title =  "throughput",           AssignedTo = users[6],      Description = "Suspendisse potenti.",                                                   State = State.Removed },
                new Task { Title =  "neural-net",           AssignedTo = users[7],      Description = "Morbi ut odio.",                                                         State = State.Removed },
                new Task { Title =  "interactive",          AssignedTo = users[19],     Description = "Nullam porttitor lacus at turpis.",                                      State = State.Removed },
                new Task { Title =  "Stand-alone",          AssignedTo = users[13],     Description = "Nam congue, risus semper porta volutpat, quam pede lobortis ligula",     State = State.Removed },
                new Task { Title =  "Robust",               AssignedTo = users[19],     Description = "Vestibulum ante ipsum primis in faucibus orci luctus et ultrices",       State = State.Removed },
                new Task { Title =  "architecture",         AssignedTo = users[5],      Description = "In est risus, auctor sed, tristique in, tempus sit amet, sem.",          State = State.Removed },
                new Task { Title =  "open architecture",    AssignedTo = users[17],     Description = "Suspendisse potenti.",                                                   State = State.Removed },
                new Task { Title =  "portal",               AssignedTo = users[18],     Description = "Donec dapibus.",                                                         State = State.Removed },
                new Task { Title =  "bifurcated",           AssignedTo = users[24],     Description = "In congue.",                                                             State = State.Removed },
                new Task { Title =  "non-volatile",         AssignedTo = users[1],      Description = "Maecenas tristique, est et tempus semper, est quam pharetra magna.",     State = State.Closed },
                new Task { Title =  "hardware",             AssignedTo = users[11],     Description = "Quisque porta volutpat erat.",                                           State = State.Closed },
                new Task { Title =  "4th generation",       AssignedTo = users[6],      Description = "Sed vel enim sit amet nunc viverra dapibus.",                            State = State.Closed },
                new Task { Title =  "Enterprise-wide",      AssignedTo = users[22],     Description = "Suspendisse potenti.",                                                   State = State.Closed },
                new Task { Title =  "Polarised",            AssignedTo = users[21],     Description = "Integer ac leo.",                                                        State = State.Closed },
                new Task { Title =  "secondary",            AssignedTo = users[12],     Description = "Pellentesque viverra pede ac diam.",                                     State = State.Closed },
                new Task { Title =  "Persistent",           AssignedTo = users[14],     Description = "Duis at velit eu est congue elementum.",                                 State = State.Closed },
                new Task { Title =  "Object-based",         AssignedTo = users[22],     Description = "Aenean sit amet justo.",                                                 State = State.Closed },
                new Task { Title =  "Persevering",          AssignedTo = users[1],      Description = "Vestibulum sed magna at nunc commodo placerat.",                         State = State.Closed },
                new Task { Title =  "Function-based",       AssignedTo = users[20],     Description = "Nullam molestie nibh in lectus.",                                        State = State.Closed },
                new Task { Title =  "De-engineered",        AssignedTo = users[19],     Description = "Maecenas tincidunt lacus at velit.",                                     State = State.Closed },
                new Task { Title =  "hierarchy",            AssignedTo = users[2],      Description = "Pellentesque viverra pede ac diam.",                                     State = State.Closed },
                new Task { Title =  "benchmark",            AssignedTo = users[14],     Description = "Sed vel enim sit amet nunc viverra dapibus.",                            State = State.Closed },
                new Task { Title =  "zero tolerance",       AssignedTo = users[13],     Description = "Nulla ac enim.",                                                         State = State.Closed },
                new Task { Title =  "motivating",           AssignedTo = users[2],      Description = "Donec quis orci eget orci vehicula condimentum.",                        State = State.Closed },
                new Task { Title =  "monitoring",           AssignedTo = users[7],      Description = "Suspendisse potenti.",                                                   State = State.Active },
                new Task { Title =  "responsive",           AssignedTo = users[24],     Description = "Quisque id justo sit amet sapien dignissim vestibulum.",                 State = State.Active },
                new Task { Title =  "4th generation",       AssignedTo = users[6],      Description = "Fusce posuere felis sed lacus.",                                         State = State.Active },
                new Task { Title =  "Reactive",             AssignedTo = users[1],      Description = "Duis at velit eu est congue elementum.",                                 State = State.Active },
                new Task { Title =  "static",               AssignedTo = users[2],      Description = "Vestibulum rutrum rutrum neque.",                                        State = State.Active },
                new Task { Title =  "24 hour",              AssignedTo = users[7],      Description = "Sed accumsan felis.",                                                    State = State.Active },
                new Task { Title =  "Persevering",          AssignedTo = users[23],     Description = "Vivamus metus arcu, adipiscing molestie, hendrerit at, vulputate vitae", State = State.Active },
                new Task { Title =  "fresh-thinking",       AssignedTo = users[1],      Description = "Praesent id massa id nisl venenatis lacinia.",                           State = State.New },
                new Task { Title =  "orchestration",        AssignedTo = users[23],     Description = "Vestibulum rutrum rutrum neque.",                                        State = State.New },
                new Task { Title =  "Multi-channelled",     AssignedTo = users[1],      Description = "Proin at turpis a pede posuere nonummy.",                                State = State.New },
                new Task { Title =  "Re-engineered",        AssignedTo = users[22],     Description = "Morbi porttitor lorem id ligula.",                                       State = State.New }
            };

            // Seed tags
            var tags = new Tag[]
            {
                new Tag { Name = "Innovative" },
                new Tag { Name = "asynchronous" },
                new Tag { Name = "Centralized" },
                new Tag { Name = "Switchable" },
                new Tag { Name = "secondary" },
                new Tag { Name = "algorithm" },
                new Tag { Name = "Secured" },
                new Tag { Name = "process improvement" },
                new Tag { Name = "needs based" },
                new Tag { Name = "Self enabling" },
                new Tag { Name = "bottom line" },
                new Tag { Name = "matrix" },
                new Tag { Name = "bifurcated" },
                new Tag { Name = "Devolved" },
                new Tag { Name = "discrete" },
                new Tag { Name = "dynamic" },
                new Tag { Name = "adapter" },
                new Tag { Name = "workforce" },
                new Tag { Name = "emulation" },
                new Tag { Name = "encoding" },
                new Tag { Name = "Configurable" },
                new Tag { Name = "User friendly" },
                new Tag { Name = "Operative" },
                new Tag { Name = "asymmetric" },
                new Tag { Name = "utilisation" },
                new Tag { Name = "capacity" },
                new Tag { Name = "cohesive" },
                new Tag { Name = "web enabled" },
                new Tag { Name = "Integrated" },
                new Tag { Name = "open system" },
                new Tag { Name = "client driven" },
                new Tag { Name = "incremental" },
                new Tag { Name = "demand driven" },
                new Tag { Name = "logistical" },
                new Tag { Name = "Streamlined" },
                new Tag { Name = "Multi tiered" },
                new Tag { Name = "neutral" },
                new Tag { Name = "encryption" },
                new Tag { Name = "Virtual" },
                new Tag { Name = "secondary" },
                new Tag { Name = "Centralized" },
                new Tag { Name = "logistical" },
                new Tag { Name = "orchestration" },
                new Tag { Name = "Programmable" },
                new Tag { Name = "Versatile" },
                new Tag { Name = "Sharable" },
                new Tag { Name = "eco centric" },
                new Tag { Name = "multimedia" },
                new Tag { Name = "Team oriented" },
                new Tag { Name = "frame" },
                new Tag { Name = "Profound" },
                new Tag { Name = "open system" },
                new Tag { Name = "forecast" },
                new Tag { Name = "Balanced" },
                new Tag { Name = "dedicated" },
                new Tag { Name = "knowledge user" },
                new Tag { Name = "matrix" },
                new Tag { Name = "Integrated" },
                new Tag { Name = "De engineered" },
                new Tag { Name = "mobile" },
                new Tag { Name = "Intuitive" },
                new Tag { Name = "implementation" },
                new Tag { Name = "optimizing" },
                new Tag { Name = "Business focused" },
                new Tag { Name = "Monitored" },
                new Tag { Name = "full range" },
                new Tag { Name = "local" },
                new Tag { Name = "array" },
                new Tag { Name = "local" },
                new Tag { Name = "Front line" },
                new Tag { Name = "emulation" },
                new Tag { Name = "Integrated" },
                new Tag { Name = "info mediaries" },
                new Tag { Name = "Universal" },
                new Tag { Name = "Mandatory" },
                new Tag { Name = "directional" },
                new Tag { Name = "systemic" },
                new Tag { Name = "Optimized" },
                new Tag { Name = "non volatile" },
                new Tag { Name = "Adaptive" },
                new Tag { Name = "Function based" },
                new Tag { Name = "strategy" },
                new Tag { Name = "Open architected" },
                new Tag { Name = "ability" },
                new Tag { Name = "core" },
                new Tag { Name = "Synergistic" },
                new Tag { Name = "help desk" },
                new Tag { Name = "Optimized" },
                new Tag { Name = "middleware" },
                new Tag { Name = "Fully configurable" },
                new Tag { Name = "capability" },
                new Tag { Name = "productivity" },
                new Tag { Name = "Optimized" },
                new Tag { Name = "foreground" },
                new Tag { Name = "project" },
                new Tag { Name = "analyzer" },
                new Tag { Name = "context sensitive" },
                new Tag { Name = "collaboration" },
                new Tag { Name = "Monitored" },
                new Tag { Name = "adapter" }
            };

            context.AddRange(tags);
            
            for(var i = 0; i < tasks.Length * 2; i++)
            {
                tasks[i % 49].tags.Add(tags[i % 98]);
            }

            context.AddRange(tasks);
            context.SaveChanges();
        }
    }
}

