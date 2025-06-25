using Application.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.ServicesInterfaces
{
    public interface ICommentService
    {
        Task<CommentDto> CreateCommentAsync(CreateCommentDto createCommentDto, string authorId, CancellationToken cancellationToken = default);
        Task<CommentDto> GetCommentByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<CommentDto>> GetCommentsForTaskAsync(string taskId, CancellationToken cancellationToken = default);
        Task<IEnumerable<CommentDto>> GetAllCommentsAsync(CancellationToken cancellationToken = default);
        Task UpdateCommentAsync(string id, UpdateCommentDto updateCommentDto, string userId, CancellationToken cancellationToken = default);
        Task DeleteCommentAsync(string id, string userId, CancellationToken cancellationToken = default);
    }
}