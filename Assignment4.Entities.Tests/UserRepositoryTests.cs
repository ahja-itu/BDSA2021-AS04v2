using System;
using Assignment4.Core;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;


namespace Assignment4.Entities.Tests
{
    public class UserRepositoryTests : IDisposable
    {

        private readonly SqliteConnection _sqliteConnection;
        private readonly KanbanContext _context;

        private UserRepository _repo;
        public UserRepositoryTests()
        {
            // do the database login thing
            _sqliteConnection = new SqliteConnection("Filename=:memory:");
            _sqliteConnection.Open();
            var builder = new DbContextOptionsBuilder<KanbanContext>();
            builder.UseSqlite(_sqliteConnection);
            _context = new KanbanContext(builder.Options);
            _context.Database.EnsureCreated();
            _repo = new UserRepository(_context);

            // Now we can add data to the database here for the test cases to use and validate against
            
            // add an example user
            var entity = new User { Name = "Bob", Email = "uncle@bob.com" };
            _context.Users.Add(entity);
            _context.SaveChanges();

            // add an example user with tasks associated with them
            entity = new User { Name = "Suzy", Email = "email@server.com" };
            _context.Users.Add(entity);
            _context.SaveChanges();

            var tasks = new Task[] {
                new Task { AssignedTo = entity, Title = "Clean kitchen", Description = "The kitchen is dirty", State = State.New },
                new Task { AssignedTo = entity, Title = "Clean bathroom", Description = "The bathroom is smelly", State = State.Active }
            };
            _context.Tasks.AddRange(tasks);
            _context.SaveChanges();
        }

        [Fact]
        public void Create_given_user_with_name_return_created_response_and_id()
        {
            var entity = new UserCreateDTO { Name = "John", Email = "john@doe.com" };
            (Response response, int uid) = _repo.Create(entity);

            Assert.Equal(Response.Created, response);
            Assert.Equal(uid, 3);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Create_given_user_with_missing_name_return_reponse_bad_request(string input)
        {
            var entity = new UserCreateDTO { Name = input, Email = "john@doe.com" };
            
            (Response response, int uid) = _repo.Create(entity);

            Assert.Equal(Response.BadRequest, response);
            Assert.Equal(-1, uid);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Create_given_user_with_missing_email_return_bad_response(string input)
        {
            var entity = new UserCreateDTO { Name = "John", Email = input };

            (Response response, int uid) = _repo.Create(entity);

            Assert.Equal(Response.BadRequest, response);
            Assert.Equal(-1, uid);
        }

        [Fact]
        public void Create_given_email_in_use_should_return_conflict_response()
        {
            var entity = new UserCreateDTO { Name = "Robert", Email = "uncle@bob.com" };

            (Response response, int uid) = _repo.Create(entity);

            Assert.Equal(Response.Conflict, response);
            Assert.Equal(-1, uid);
        }


        [Fact]
        public void Delete_delete_the_example_user_should_return_deleted_response()
        {
            var response = _repo.Delete(1);

            Assert.Equal(Response.Deleted, response);
        }

        [Fact]
        public void Delete_delete_not_existing_user_should_return_reponse_notfound()
        {
            var response = _repo.Delete(1337);

            Assert.Equal(Response.NotFound, response);
        }

        [Fact]
        public void Delete_user_with_associated_tasks_should_not_delete()
        {
            var response = _repo.Delete(2);

            Assert.Equal(Response.Conflict, response);
        }

        [Fact]
        public void Delete_user_with_associated_tasks_force_delete_should_delete()
        {
            var response = _repo.Delete(2, true);

            Assert.Equal(Response.Deleted, response);
        }

        [Fact]
        public void Read_given_user_id_1_read_user_with_name_john_and_email_john_at_doe_dot_com()
        {
            var response = _repo.Read(1);

            var expected = new UserDTO(1, "Bob", "uncle@bob.com");
            Assert.Equal(expected, response);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(1337)]
        public void Read_given_invalid_or_not_existing_id_values_expect_null_value_returned(int id)
        {
            var response = _repo.Read(id);

            Assert.Null(response);
        }

        [Fact]
        public void ReadAll_returns_two_users_bob_and_suzy()
        {
            var users = _repo.ReadAll();

            Assert.Collection(users,
                u => Assert.Equal(new UserDTO(1, "Bob", "uncle@bob.com"), u),
                u => Assert.Equal(new UserDTO(2, "Suzy", "email@server.com"), u));
        }

        [Fact]
        public void Update_update_bob_new_name_john_user_with_id_1_should_be_named_john()
        {
            var user = _repo.Read(1);
            UserUpdateDTO newUser = new UserUpdateDTO { Id = user.Id, Name = "John", Email = user.Email };
            
            var response = _repo.Update(newUser);
            var updatedUser = _repo.Read(1);

            Assert.Equal("John", updatedUser.Name);
            Assert.Equal("uncle@bob.com", updatedUser.Email);
            Assert.Equal(Response.Updated, response);
        }

        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData(null)]
        public void Update_attempt_update_with_bad_user_names_should_return_bad_request(string badname)
        {
            var updateUser = new UserUpdateDTO { Id = 1, Name = badname, Email = "uncle@bob.com" };

            var response = _repo.Update(updateUser);
            var notUpdatedUser = _repo.Read(1);

            Assert.Equal("Bob", notUpdatedUser.Name);
            Assert.Equal("uncle@bob.com", notUpdatedUser.Email);
            Assert.Equal(Response.BadRequest, response);
        }

        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData(null)]
        public void Update_attempt_update_with_bad_email_account_should_return_bad_request(string bademail)
        {
            var updateUser = new UserUpdateDTO { Id = 1, Name = "Bob", Email = bademail };

            var response = _repo.Update(updateUser);
            var notUpdatedUser = _repo.Read(1);

            Assert.Equal("Bob", notUpdatedUser.Name);
            Assert.Equal("uncle@bob.com", notUpdatedUser.Email);
            Assert.Equal(Response.BadRequest, response);
        }

        public void Dispose()
        {
            _context.Dispose();
            _sqliteConnection.Dispose();
        }
    }
}
