using System.Collections.Generic;
using System.Collections.ObjectModel;
using Assignment4.Core;
using Assignment4;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Assignment4.Entities
{
    public class UserRepository : IUserRepository
    {
        private DbContextOptionsBuilder<KanbanContext> optionsBuilder;

        private readonly KanbanContext _context;
        public UserRepository(KanbanContext context)
        {
            _context = context;
        }

        public (Response Response, int UserId) Create(UserCreateDTO user)
        {
            // TODO: Add email validation, but perhapst the DB takes care of that
            if (user == null
                || IsEmptyString(user.Name)
                || IsEmptyString(user.Email))
            {
                return (Response.BadRequest, -1);
            }

            try
            {
                // Make sure the email is not associated with an existing user
                var existingUser = _context.Users.FirstOrDefault(u => u.Email == user.Email);
                
                if (existingUser != null)
                {
                    return (Response.Conflict, -1);
                }

                var newUser = new User
                {
                    Name = user.Name,
                    Email = user.Email
                };
                _context.Users.Add(newUser);
                _context.SaveChanges();
                return (Response.Created, newUser.Id);
            }
            catch
            {
                return (Response.BadRequest, -1);
            }
        }

        public Response Delete(int userId, bool force = false)
        {
            if (userId < 1)
            {
                return Response.BadRequest;
            }

            var entity = _context.Users.Find(userId);

            if (entity == null)
            {
                return Response.NotFound;
            }

            // find out if the user has tasks
            var taskCountForEntity = _context.Tasks
                                        .Where(t => t.AssignedTo == entity)
                                        .Count();

            if (taskCountForEntity == 0 || (taskCountForEntity > 0 && force))
            {
                if (taskCountForEntity > 0)
                {
                    var tasks = _context.Tasks
                                    .Where(t => t.AssignedTo == entity)
                                    .Select(t => t);
                    _context.RemoveRange(tasks);
                    _context.SaveChanges();
                }

                _context.Remove(entity);
                _context.SaveChanges();

                return Response.Deleted;
            }
            else
            {
                return Response.Conflict;
            }
        }

        public UserDTO Read(int userId)
        {
            var user = _context.Users.Find(userId);

            return user == null ? null : new UserDTO(user.Id, user.Name, user.Email);
        }

        public IReadOnlyCollection<UserDTO> ReadAll()
        {
            var users = _context.Users
                            .Select(u => new UserDTO(u.Id, u.Name, u.Email))
                            .ToList();
            return new ReadOnlyCollection<UserDTO>(users);
        }

        public Response Update(UserUpdateDTO user)
        {

            // Handle bad input
            if (user == null
                || IsEmptyString(user.Name)
                || IsEmptyString(user.Email))
            {
                return Response.BadRequest;
            }

            var storedUser = _context.Users.Find(user.Id);

            if (storedUser == null)
            {
                return Response.NotFound;
            }
        
            storedUser.Name = user.Name;
            storedUser.Email = user.Email;

            _context.Users.Update(storedUser);
            _context.SaveChanges();
            return Response.Updated;
        }

        private bool IsEmptyString(string input)
        {
            return input == null || input.Trim().Length == 0;
        }
    }
}