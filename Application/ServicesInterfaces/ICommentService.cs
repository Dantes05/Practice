using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ServicesInterfaces
{
    public interface ICommentService
    {
        Task<CommentDto> CreateCommentAsync(CreateCommentDto createCommentDto, string authorId);
        Task<CommentDto> GetCommentByIdAsync(string id);
        Task<IEnumerable<CommentDto>> GetCommentsForTaskAsync(string taskId);
        Task<IEnumerable<CommentDto>> GetAllCommentsAsync();
        Task UpdateCommentAsync(string id, UpdateCommentDto updateCommentDto, string userId);
        Task DeleteCommentAsync(string id, string userId);
    }
}
