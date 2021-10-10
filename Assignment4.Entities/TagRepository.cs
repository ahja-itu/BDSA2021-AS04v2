using System.Collections.Generic;
using System.Collections.ObjectModel;
using Assignment4.Core;
using System.Linq;

namespace Assignment4.Entities
{
    public class TagRepository : ITagRepository
    {

        private readonly KanbanContext _context;

        public TagRepository(KanbanContext context)
        {
            _context = context;
        }

        /*

            [X] Tags which are assigned to a task may only be deleted using the force.
            [X] Trying to delete a tag in use without the force should return Conflict.
            [X] Trying to create a tag which exists already should return Conflict.


            [X] Trying to update or delete a non-existing entity should return NotFound.
            [X] Create, Read, and Update should return a proper Response.
            [X] Your are not allowed to write throw new ... - use the Response instead.
            [X] If a task, tag, or user is not found, return null.

        */

        public (Response Response, int TagId) Create(TagCreateDTO tag)
        {
            if (IsEmpty(tag.Name))
            {
                return (Response.BadRequest, -1);
            }

            var existingTag = _context.Tags.FirstOrDefault(t => t.Name == tag.Name);

            if (existingTag != null)
            {
                return (Response.Conflict, -1);
            }

            var createEntity = new Tag { Name = tag.Name };

            _context.Tags.Add(createEntity);
            _context.SaveChanges();

            return (Response.Created, createEntity.Id);
        }

        public Response Delete(int tagId, bool force = false)
        {
            var tag = _context.Tags.Find(tagId);
            if (tag == null)
            {
                return Response.NotFound;
            }

            var numTagUsages = _context.Tasks
                                .Where(t => t.tags.Contains(tag))
                                .Count();

            var tasksWithTag = from task in _context.Tasks
                               where task.tags.Contains(tag)
                               select task;

            if (numTagUsages > 0)
            {
                if (force)
                {

                    foreach (var task in tasksWithTag)
                    {
                        task.tags.Remove(tag);
                        _context.Tasks.Update(task);
                        _context.SaveChanges();
                    }

                    _context.SaveChanges();

                }
                else
                {
                    return Response.Conflict;
                }
            }

            _context.Tags.Remove(tag);
            _context.SaveChanges();
            return Response.Deleted;
        }

        public TagDTO Read(int tagId)
        {
            var tag = _context.Tags.Find(tagId);

            return tag == null ? null : new TagDTO(tag.Id, tag.Name);
        }

        public IReadOnlyCollection<TagDTO> ReadAll()
        {
            var tagDTOs = from tag in _context.Tags
                          select new TagDTO(tag.Id, tag.Name);

            return new ReadOnlyCollection<TagDTO>(tagDTOs.ToArray());
        }

        public Response Update(TagUpdateDTO tag)
        {
            // This is a weird decision: if not setting tag id, then it defaults to 0
            // as per int standards. This might mean that client forgot to set it
            // as I'm assuming here. My reasoning is that IDs in the DB starts from 1
            // so id 0 should be a mistake.
            if (tag.Id == 0 || IsEmpty(tag.Name)) {
                return Response.BadRequest;
            }

            var oldTag = _context.Tags.Find(tag.Id);
            if (oldTag == null)
            {
                return Response.NotFound;
            }

            oldTag.Name = tag.Name;
            _context.Tags.Update(oldTag);
            _context.SaveChanges();

            return Response.Updated;
        }


        private bool IsEmpty(string str) => str == null || str.Trim().Length == 0;
    }
}
