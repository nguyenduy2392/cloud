using Application.Auth;
using Application.CloudServices;
using Application.Helper;
using Application.UserServices;
using Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Application
{
    public static class ServiceHelper
    {
        public static void Registers(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IRepository, Repository>();
            builder.Services.AddScoped<ICryptorFactory, CryptorFactory>();
            builder.Services.AddScoped<IJwtFactory, JwtFactory>();
            builder.Services.AddScoped<IAppContextAccessor, AppContextAccessor>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IFolderService, FolderService>();
            builder.Services.AddScoped<IFileService, FileService>();
            builder.Services.AddScoped<IResourcePermissionService, ResourcePermissionService>();
            builder.Services.AddScoped<IUserStorageService, UserStorageService>();
            builder.Services.AddScoped<IDatabaseService, DatabaseService>();
        }
    }
}
