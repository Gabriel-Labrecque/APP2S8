using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Sanssoussi.Data;
using Sanssoussi.Models;

namespace Sanssoussi.Services
{
    // Only place in the application that reads or writes comments.
    // Every query is scoped to the member who owns the comments.
    public class CommentService
    {
        public const int MaxCommentsPerUser = 100;

        private readonly SanssoussiContext _context;

        public CommentService(SanssoussiContext context)
        {
            this._context = context;
        }

        public Task<List<string>> GetAsync(string userId)
        {
            return this._context.Comments
                .Where(c => c.UserId == userId)
                .Select(c => c.Text)
                .ToListAsync();
        }

        public Task<List<string>> SearchAsync(string userId, string search)
        {
            return this._context.Comments
                .Where(c => c.UserId == userId && EF.Functions.Like(c.Text, "%" + search + "%"))
                .Select(c => c.Text)
                .ToListAsync();
        }

        // Returns false when the member has reached the maximum number of comments.
        public async Task<bool> AddAsync(string userId, string text)
        {
            if (await this._context.Comments.CountAsync(c => c.UserId == userId) >= MaxCommentsPerUser)
            {
                return false;
            }

            this._context.Comments.Add(new Comment { CommentId = Guid.NewGuid().ToString(), UserId = userId, Text = text });
            await this._context.SaveChangesAsync();
            return true;
        }
    }
}
