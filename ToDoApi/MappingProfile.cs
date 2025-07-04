﻿using Application.DTOs;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace LibraryApp
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<UserForRegistrationDto, User>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email));

            CreateMap<User, AuthResponseDto>();

            CreateMap<CreateTaskDto, Taska>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => "New")) 
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

            CreateMap<UpdateTaskDto, Taska>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null)); 

            CreateMap<Taska, TaskDto>();

            CreateMap<ChangeTaskStatusDto, Taska>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

            CreateMap<CreateCommentDto, Comment>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

            CreateMap<UpdateCommentDto, Comment>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Comment, CommentDto>();
            CreateMap<TaskHistory, TaskHistoryDto>();

            CreateMap<Taska, TaskCsvDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority.ToString()))
                .ForMember(dest => dest.DueDate, opt => opt.MapFrom(src => src.DueDate.ToString("o")))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt.ToString("o")))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt.ToString("o")));
        }
    }
}