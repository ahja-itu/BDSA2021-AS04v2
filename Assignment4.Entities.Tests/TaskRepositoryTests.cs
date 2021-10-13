using System.Collections.Generic;
using System.Collections.ObjectModel;
using Assignment4.Core;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System.Linq;
using System;

namespace Assignment4.Entities.Tests
{
    public class TaskRepositoryTests
    {

        private readonly SqliteConnection _sqliteConnection;
        private readonly KanbanContext _context;
        private TaskRepository _repo;

        
        public TaskRepositoryTests()
        {
            _sqliteConnection = new SqliteConnection("Filename=:memory:");
            _sqliteConnection.Open();
            var builder = new DbContextOptionsBuilder<KanbanContext>();
            builder.UseSqlite(_sqliteConnection);
            _context = new KanbanContext(builder.Options);
            _context.Database.EnsureCreated();
            _repo = new TaskRepository(_context);


            // define two test tags: "Buzzword" and "Urgent"
            var tag = new Tag { Id = 1, Name = "Buzzword" };
            _context.Tags.Add(tag);
            _context.SaveChanges();

            tag = new Tag { Id = 2, Name = "Urgent" };
            _context.Tags.Add(tag);
            _context.SaveChanges();

            // prepare test user
            var user = new User { Id = 1, Name = "Bob", Email = "uncle@bob.com"};
            _context.Users.Add(user);
            _context.SaveChanges();

            var task = new Task 
            { 
                Id = 1, 
                Title = "Original Task",
                Description = "Original Description",
                tags = new ReadOnlyCollection<Tag>(new Tag[] { tag }),
                AssignedTo = user,
                State = State.New
            };
            _context.Tasks.Add(task);
            _context.SaveChanges();


            task = new Task 
            { 
                Id = 2, 
                Title = "Active Task",
                Description = "Original Description",
                State = State.Active
            };
            _context.Tasks.Add(task);
            _context.SaveChanges();

            task = new Task 
            { 
                Id = 3, 
                Title = "Resolved Task",
                Description = "Original Description",
                State = State.Resolved
            };
            _context.Tasks.Add(task);
            _context.SaveChanges();

            task = new Task 
            { 
                Id = 4, 
                Title = "Removed Task",
                Description = "Original Description",
                State = State.Removed
            };
            _context.Tasks.Add(task);
            _context.SaveChanges();

            task = new Task 
            { 
                Id = 5, 
                Title = "Closed Task",
                Description = "Original Description",
                State = State.Closed
            };
            _context.Tasks.Add(task);
            _context.SaveChanges();
                                
        }

        [Fact]
        public void Create_task_without_any_assignee_but_with_all_tags_should_return_its_id()
        {

            var tags = _context.Tags.Select(t => new string(t.Name)).ToArray();
            var tagCollection = new ReadOnlyCollection<string>(tags);
            var entity = new TaskDTO { Title = "Title", 
                                       Description = "This is a description",
                                       Tags = tags};

            (Response response, int tid) = _repo.Create(entity);

            Assert.Equal(6, tid);
            Assert.Equal(Response.Created, response);
        }

        [Fact]
        public void Create_task_without_any_assignee_or_tags_should_return_its_id()
        {
            var entity = new TaskDTO { Title = "Creative Title",
                                       Description = "Creative Description"};

            (Response response, int tid) = _repo.Create(entity);

            Assert.Equal(6, tid);
            Assert.Equal(Response.Created, response);
        }

        [Fact]
        public void Create_task_with_assignee_but_no_tags_should_return_its_id()
        {
            int? userId = _context.Users.FirstOrDefault().Id;
            var entity = new TaskDTO { Title = "Cool Title",
                                       Description = "Cool Description",
                                       AssignedToId = userId};
            
            (Response response, int tid) = _repo.Create(entity);

            Assert.Equal(6, tid);
            Assert.Equal(Response.Created, response);
        }

        [Theory]
        [InlineData("")]
        [InlineData("    ")]
        public void Create_task_with_empty_title_should_return_invalid_id_indicating_failure(string title)
        {
            var entity = new TaskDTO 
            {
                Title = title,
                Description = "This is going to fail!",
            };

            (Response response, int tid) = _repo.Create(entity);

            Assert.Equal(-1, tid);
            Assert.Equal(Response.BadRequest, response);
        }

        public void Create_task_title_is_null_should_throw_DbUpdateException()
        {
            
            var entity = new TaskDTO
            { 
                Title = null
            };

            Assert.Throws<DbUpdateException>(() => _repo.Create(entity));
        }

        public void Create_task_title_is_never_set_should_throw_DbUpdateException()
        {
            var entity = new TaskDTO();

            Assert.Throws<DbUpdateException>(() => _repo.Create(entity));
        }

        public void Create_task_with_title_of_101_characters_should_throw_DbUpdateException()
        {
            var entity = new TaskDTO
            {
                Title = "01234567890123456789012345678901234567890123456789" +
                    "01234567890123456789012345678901234567890123456789" +
                    "q"
            };

            Assert.Equal(101, entity.Title.Length);

            Assert.Throws<DbUpdateException>(() => _repo.Create(entity));
        }

        [Fact]
        public void Create_task_with_title_of_100_characters_should_return_new_task_id()
        {
            var entity = new TaskDTO
            {
                Title = "01234567890123456789012345678901234567890123456789" +
                    "01234567890123456789012345678901234567890123456789"
            };
            Assert.Equal(100, entity.Title.Length);

            (Response response, int tid) = _repo.Create(entity);
            
            Assert.Equal(6, tid);
            Assert.Equal(Response.Created, response);
        }

        [Fact]
        public void Create_task_with_assigned_user_should_save_the_assigning()
        {
            var entity = new TaskDTO
            {
                Title = "Sample Title",
                Description = "Sample Description",
                AssignedToId = 1
            };

            _repo.Create(entity);
            var task = _context.Tasks.Find(6);

            Assert.NotNull(task.AssignedTo);
            Assert.Equal(1, task.AssignedTo.Id);
        }

        [Fact]
        public void Create_task_without_any_assignee_ansignedTo_should_be_null()
        {
            var entity = new TaskDTO
            {
                Title = "Sample Title",
                Description = "Sample Description"
            };

            _repo.Create(entity);
            var task = _context.Tasks.Find(6);

            Assert.Null(task.AssignedTo);
        }

        [Fact]
        public void Create_task_gets_assigned_timestamp_should_be_within_two_seconds_old()
        {
            var entity = new TaskDTO
            {
                Title = "Sample Title"
            };

            var timer = DateTime.UtcNow;

            _repo.Create(entity);
            var task = _context.Tasks.Find(6);
            var timeSpanCreated = timer - task.Created;
            var timeSpanUpdated = timer - task.StateUpdated;
            
            Assert.NotNull(task);
            Assert.True(timeSpanCreated.TotalSeconds < 2);
            Assert.True(timeSpanUpdated.TotalSeconds < 2);
        }

        [Fact]
        public void Delete_given_existing_id_should_remove()
        {
            int taskId = 1;

            var response = _repo.Delete(taskId);

            Assert.Equal(Response.Deleted, response);
        }

        [Fact]
        public void Delete_task_with_state_active_should_change_state_to_Removed_and_return_response_deleted()
        {

            // id 2
            int taskId = 2;

            var response = _repo.Delete(taskId);
            var task = _context.Tasks.Find(taskId);

            Assert.Equal(Response.Deleted, response);
            Assert.Equal(State.Removed, task.State);
        }

        [Theory]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        public void Delete_task_with_state_either_resolved_closed_or_removed_should_return_response_conflict(int id)
        {
            var response = _repo.Delete(id);

            Assert.Equal(Response.Conflict, response);
        }

        [Fact]
        public void Delete_given_non_existing_id_should_return_bad_request()
        {
            var taskId = 1337;

            var response = _repo.Delete(taskId);

            Assert.Equal(Response.BadRequest, response);            
        }

        [Fact]
        public void FindById_given_non_existing_id_should_return_null()
        {
            var task = _repo.FindById(1337);

            Assert.Null(task);
        }

        [Fact]
        public void FindById_given_id_1_should_be_assigned_to_user_and_have_one_tag()
        {
            int taskId = 1;

            var task = _repo.FindById(taskId);

            Assert.Equal("Bob", task.AssignedToName);
            Assert.Equal("uncle@bob.com", task.AssignedToEmail);
            Assert.Equal(1, task.AssignedToId);
            Assert.Equal(State.New, task.State);
            Assert.Equal("Original Task", task.Title);
            Assert.Equal("Original Description", task.Description);
            Assert.Collection(task.Tags, 
                t => Assert.Equal("Urgent", t));
        }

        [Fact]
        public void FindById_given_id_2_should_not_be_assigned_to_any_user_and_have_no_tags()
        {
            var taskId = 2;

            var task = _repo.FindById(taskId);

            Assert.Equal("Active Task", task.Title);
            Assert.Equal("Original Description", task.Description);
            Assert.Equal(State.Active, task.State);
            Assert.Null(task.AssignedToEmail);
            Assert.Null(task.AssignedToId);
            Assert.Null(task.AssignedToName);
            Assert.Null(task.Tags);
        }

        [Fact]
        public void Update_update_task_id_1_with_new_title_to_new_title_should_be_new_title()
        {
            var taskId = 1;
            var task = _context.Tasks.Find(taskId);
            var taskDTO = new TaskDTO
            {
                Id = task.Id,
                Title = "New Title",
                Description = task.Description,
                State = task.State,
                Tags = new ReadOnlyCollection<string>(task.tags.Select(t => t.Name).ToArray())
            };

            var response = _repo.Update(taskDTO);
            var updatedTask = _context.Tasks.Find(taskId);

            Assert.Equal(Response.Updated, response);
            Assert.Equal(taskDTO.Title, updatedTask.Title);
            Assert.Equal(taskDTO.Description, updatedTask.Description);
            Assert.Equal(taskDTO.State, updatedTask.State);
            Assert.Equal(1, updatedTask.tags.Count()); // this is the lazy way :|
        }

        [Fact]
        public void All_should_get_all_5_tasks()
        {
            //Given
            var tag = _context.Tags.Find(1);
            //When
            var tasks = _repo.All();
        
            //Then
            Assert.Collection(tasks,
                t => Assert.Equal("Original Task", t.Title),
                t => Assert.Equal("Active Task", t.Title),
                t => Assert.Equal("Resolved Task", t.Title),
                t => Assert.Equal("Removed Task", t.Title),
                t => Assert.Equal("Closed Task", t.Title));
            }
    }
}
