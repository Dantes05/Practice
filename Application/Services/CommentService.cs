using Application.DTOs;
using Application.ServicesInterfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly ITaskRepository _taskRepository;
        private readonly IMapper _mapper;

        public CommentService(
            ICommentRepository commentRepository,
            ITaskRepository taskRepository,
            IMapper mapper)
        {
            _commentRepository = commentRepository;
            _taskRepository = taskRepository;
            _mapper = mapper;
        }

        public async Task<CommentDto> CreateCommentAsync(CreateCommentDto createCommentDto, string authorId)
        {
            if (!await _taskRepository.ExistsAsync(createCommentDto.TaskaId))
                throw new KeyNotFoundException("Task not found");

            var comment = _mapper.Map<Comment>(createCommentDto);
            comment.Id = Guid.NewGuid().ToString();
            comment.AuthorId = authorId;
            comment.CreatedAt = DateTime.UtcNow;

            await _commentRepository.AddAsync(comment);
            return _mapper.Map<CommentDto>(comment);
        }

        public async Task<CommentDto> GetCommentByIdAsync(string id)
        {
            var comment = await _commentRepository.GetByIdAsync(id);
            if (comment == null)
                throw new KeyNotFoundException("Comment not found");

            return _mapper.Map<CommentDto>(comment);
        }

        public async Task<IEnumerable<CommentDto>> GetCommentsForTaskAsync(string taskId)
        {
            var comments = await _commentRepository.GetCommentsForTaskAsync(taskId);
            return _mapper.Map<IEnumerable<CommentDto>>(comments);
        }

        public async Task UpdateCommentAsync(string id, UpdateCommentDto updateCommentDto, string userId)
        {
            var comment = await _commentRepository.GetByIdAsync(id);
            if (comment == null)
                throw new KeyNotFoundException("Comment not found");

            if (comment.AuthorId != userId)
                throw new UnauthorizedAccessException("You can only update your own comments");

            comment.Text = updateCommentDto.Text;
            await _commentRepository.UpdateAsync(comment);
        }

        public async Task DeleteCommentAsync(string id, string userId)
        {
            var comment = await _commentRepository.GetByIdAsync(id);
            if (comment == null)
                throw new KeyNotFoundException("Comment not found");

            
            await _commentRepository.DeleteAsync(comment);
        }
        public async Task<IEnumerable<CommentDto>> GetAllCommentsAsync()
        {
            var comments = await _commentRepository.GetAllCommentsAsync();
            return _mapper.Map<IEnumerable<CommentDto>>(comments);
        }
    }
}
