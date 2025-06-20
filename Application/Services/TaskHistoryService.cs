using Application.DTOs;
using Application.ServicesInterfaces;
using AutoMapper;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class TaskHistoryService : ITaskHistoryService
    {
        private readonly ITaskHistoryRepository _taskHistoryRepository;
        private readonly IMapper _mapper;

        public TaskHistoryService(ITaskHistoryRepository taskHistoryRepository, IMapper mapper)
        {
            _taskHistoryRepository = taskHistoryRepository;
            _mapper = mapper;
        }

        public async Task<TaskHistoryDto> GetByIdAsync(string id)
        {
            var history = await _taskHistoryRepository.GetByIdAsync(id);
            return _mapper.Map<TaskHistoryDto>(history);
        }

        public async Task<IEnumerable<TaskHistoryDto>> GetAllAsync()
        {
            var histories = await _taskHistoryRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<TaskHistoryDto>>(histories);
        }

        public async Task<IEnumerable<TaskHistoryDto>> GetByTaskIdAsync(string taskId)
        {
            var histories = await _taskHistoryRepository.GetByTaskIdAsync(taskId);
            return _mapper.Map<IEnumerable<TaskHistoryDto>>(histories);
        }
    }
}
