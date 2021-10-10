using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Assignment4.Core;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Assignment4.Entities.Tests
{
    public class TagRepositoryTests
    {

        private readonly SqliteConnection _sqliteConnection;
        private readonly KanbanContext _context;
        private TagRepository _repo;

        public TagRepositoryTests()
        {
            _sqliteConnection = new SqliteConnection("Filename=:memory:");
            _sqliteConnection.Open();
            var builder = new DbContextOptionsBuilder<KanbanContext>();
            builder.UseSqlite(_sqliteConnection);
            _context = new KanbanContext(builder.Options);
            _context.Database.EnsureCreated();
            _repo = new TagRepository(_context);



            // base tag, no associated tasks
            var tag = new Tag { Id = 1, Name = "Buzzword" };
            _context.Tags.Add(tag);
            _context.SaveChanges();


            // prepare test user
            var user = new User { Id = 1, Name = "Bob", Email = "uncle@bob.com"};
            _context.Users.Add(user);
            _context.SaveChanges();

            // tag that is in use
            tag = new Tag { Id = 2, Name = "Urgent" };
            _context.Tags.Add(tag);
            _context.SaveChanges();
            
            // create a task for the test user

            var tags = new Collection<Tag>();
            tags.Add(tag);
            var task = new Task { Id = 1, Title = "Important task", Description = "This is important", AssignedTo = user, tags = tags};
            _context.Tasks.Add(task);
            _context.SaveChanges();
        }

        [Fact]
        public void Create_unique_tag_name_should_be_created()
        {
            var entity = new TagCreateDTO { Name = "unique tag name" };

            (Response response, int tid) = _repo.Create(entity);

            Assert.Equal(Response.Created, response);
            Assert.Equal(tid, 3);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Create_tag_with_empty_name_should_return_bad_request(string name)
        {
            var entity = new TagCreateDTO { Name = name };

            (Response response, int tid) = _repo.Create(entity);

            Assert.Equal(Response.BadRequest, response);
            Assert.Equal(tid, -1);
        }

        [Fact]
        public void Create_attempt_creating_tag_with_existing_name_should_return_conflict()
        {
            var entity = new TagCreateDTO { Name = "Buzzword" };

            (Response response, int tid) = _repo.Create(entity);

            Assert.Equal(Response.Conflict, response);
            Assert.Equal(tid, -1);
        }

        [Fact]
        public void Delete_unused_tag_without_force_should_return_deleted()
        {
            var tid = 1;

            var response = _repo.Delete(tid);
            
            Assert.Equal(Response.Deleted, response);
        }

        [Fact]
        public void Delete_tag_does_not_exist_should_return_response_not_found()
        {
            var tid = 1337;

            var response = _repo.Delete(tid);

            Assert.Equal(Response.NotFound, response);
        }

        [Fact]
        public void Delete_tag_in_use_without_force_should_return_conflict()
        {
            var tid = 2;

            var response = _repo.Delete(tid, false);

            Assert.Equal(Response.Conflict, response);
        }

        [Fact]
        public void Delete_tag_in_use_with_force_should_return_deleted()
        {
            var tid = 2;

            var response = _repo.Delete(tid, true);

            Assert.Equal(Response.Deleted, response);
        }

        [Fact]
        public void Read_given_id_1_should_return_buzzword_tag()
        {
            var tid = 1;

            var entity = _repo.Read(tid);

            Assert.Equal("Buzzword", entity.Name);
            Assert.Equal(tid, entity.Id);
        }

        [Fact]
        public void Read_given_id_1337_which_no_tag_exists_with_that_id_should_return_null()
        {
            var tid = 1337;

            var entity = _repo.Read(tid);

            Assert.Null(entity);
        }

        [Fact]
        public void ReadAll_returns_all_two_tags_Buzzword_and_Urgent()
        {
            var tags = _repo.ReadAll();

            Assert.Collection(tags,
                t => Assert.Equal(new TagDTO(1, "Buzzword"), t),
                t => Assert.Equal(new TagDTO(2, "Urgent"), t));
        }
    }
}
