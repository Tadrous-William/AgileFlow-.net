using AutoMapper;
using AgileTaskManager.Models.Entities;
using AgileTaskManager.Models.ViewModels;

namespace AgileTaskManager.Services;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<TaskItem, TaskListViewModel>()
            .ForMember(d => d.AssignedToName, o => o.MapFrom(s => s.AssignedTo != null ? s.AssignedTo.FullName : "Unassigned"))
            .ForMember(d => d.IsBlocked, o => o.MapFrom(s => s.DependsOn != null && s.DependsOn.Status != Models.Enums.TaskStatus.Done));

        CreateMap<TaskItem, TaskDetailsViewModel>()
            .ForMember(d => d.AssignedToName, o => o.MapFrom(s => s.AssignedTo != null ? s.AssignedTo.FullName : "Unassigned"))
            .ForMember(d => d.DependsOnTitle, o => o.MapFrom(s => s.DependsOn != null ? s.DependsOn.Title : null))
            .ForMember(d => d.IsBlocked, o => o.MapFrom(s => s.DependsOn != null && s.DependsOn.Status != Models.Enums.TaskStatus.Done))
            .ForMember(d => d.Comments, o => o.Ignore())
            .ForMember(d => d.Attachments, o => o.Ignore())
            .ForMember(d => d.ActivityLogs, o => o.Ignore())
            .ForMember(d => d.Feedback, o => o.Ignore());

        CreateMap<Comment, CommentViewModel>()
            .ForMember(d => d.AuthorName, o => o.MapFrom(s => s.User.FullName));

        CreateMap<Feedback, FeedbackViewModel>()
            .ForMember(d => d.ClientName, o => o.MapFrom(s => s.Client.FullName));

        CreateMap<Attachment, AttachmentViewModel>();

        CreateMap<ActivityLog, ActivityLogViewModel>()
            .ForMember(d => d.ActorName, o => o.MapFrom(s => s.Actor.FullName));

        CreateMap<Notification, NotificationViewModel>();

        CreateMap<ApplicationUser, UserListViewModel>()
            .ForMember(d => d.AssignedTasksCount, o => o.MapFrom(s => s.AssignedTasks.Count));

        CreateMap<Project, ProjectCreateViewModel>().ReverseMap();
    }
}
