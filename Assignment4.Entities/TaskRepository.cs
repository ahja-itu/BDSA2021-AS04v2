using System;
using System.Collections.Generic;
using Assignment4.Core;
using System.Linq;
using System.Collections.ObjectModel;

namespace Assignment4.Entities
{


    /*

    - [X] Creating a task will set its state to New and Created/StateUpdated to current time in UTC.
    - [X] Assigning a user which does not exist should return BadRequest.
    
    - [ ] Create/update task must allow for editing tags.
    
    - [ ] Updating the State of a task will change the StateUpdated to current time in UTC.

    - [X] Only tasks with the state New can be deleted from the database.
    - [X] Deleting a task which is Active should set its state to Removed.
    - [X] Deleting a task which is Resolved, Closed, or Removed should return Conflict.

    - [X] TaskRepository may not depend on TagRepository or UserRepository.

    */
    public class TaskRepository : ITaskRepository
    {
        private readonly KanbanContext _context;

        public TaskRepository(KanbanContext context)
        {
            _context = context;
        }

        public IReadOnlyCollection<TaskDTO> All()
        {
            var taskDTOs = _context.Tasks.Select(t => 
                new TaskDTO
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    AssignedToId = t.AssignedTo != null ? t.AssignedTo.Id : null,
                    Tags = new ReadOnlyCollection<string>(t.tags.Select(tag => tag.Name).ToArray()),
                    State = t.State
                }).ToArray();

            return new ReadOnlyCollection<TaskDTO>(taskDTOs);
        }

        public (Response, int) Create(TaskDTO task)
        {
            var user = _context.Users.Find(task.AssignedToId);

            var entity = new Task
            {
                Title = task.Title,
                Description = task.Description,
                AssignedTo = user,
                StateUpdated = DateTime.UtcNow,
                Created = DateTime.UtcNow
            };

            if (IsEmpty(task.Title))
            {
                return (Response.BadRequest, -1);
            }

            if (task.Tags != null)
            {
                entity.tags = new Collection<Tag>();

                foreach (var tag in task.Tags)
                {
                    var tagEntry = _context.Tags.FirstOrDefault(t => t.Name == tag);
                    if (tagEntry != null)
                    {
                        entity.tags.Add(tagEntry);
                    }
                }
            }

            _context.Tasks.Add(entity);
            _context.SaveChanges();

            return (Response.Created, entity.Id);
        }

        public Response Delete(int taskId)
        {
            var task = _context.Tasks.Find(taskId);
            if (task == null)
            {
                return Response.BadRequest;
            }

            if (task.State == State.New)
            {
                _context.Tasks.Remove(task);
                _context.SaveChanges();
                return Response.Deleted;
            }
            else
            {
                if (task.State == State.Active)
                {
                    task.State = State.Removed;
                    _context.Update(task);
                    _context.SaveChanges();
                    return Response.Deleted;
                }
                else
                {
                    return Response.Conflict;
                }
            }


        }

        public void Dispose()
        {
            _context.Dispose();
        }

        public TaskDetailsDTO FindById(int id)
        {
            var task = _context.Tasks.Find(id);

            if (task == null)
            {
                return null;
            }

            var taskDetails =  new TaskDetailsDTO
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                State = task.State
            };

            if (task.AssignedTo != null)
            {
                taskDetails = taskDetails with 
                {
                    AssignedToId = task.AssignedTo.Id,
                    AssignedToEmail = task.AssignedTo.Email,
                    AssignedToName = task.AssignedTo.Name
                };
            }

            if (task.tags != null && task.tags.Count() > 0)
            {
                taskDetails = taskDetails with
                {
                    Tags = task.tags.Select(t => t.Name)
                };
            }

            return taskDetails;
        }

        public Response Update(TaskDTO task)
        {
            var savedTask = _context.Tasks.Find(task.Id);

            savedTask.Title = task.Title;
            savedTask.Description = task.Description;
            savedTask.AssignedTo = task.AssignedToId != null ? _context.Users.Find(task.AssignedToId) : null;
            savedTask.tags = task.Tags.Select(tagName => _context.Tags.Where(t => t.Name == tagName).First()).ToList();
            savedTask.StateUpdated = DateTime.UtcNow;

            _context.Tasks.Update(savedTask);
            _context.SaveChanges();

            return Response.Updated;
        }

        private bool IsEmpty(string input) => input == null || input.Trim().Length == 0;
    }
}
