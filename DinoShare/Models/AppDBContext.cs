using DinoShare.Models.AccountDataModelFactory;
using DinoShare.Models.FolderDataModelFactory;
using DinoShare.Models.SystemModelFactory;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DinoShare.Models
{
    public class AppDBContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<LinkUserRole> LinkUserRole { get; set; }
        public DbSet<ApplicationLog> ApplicationLog { get; set; }
        public DbSet<EmailTemplate> EmailTemplates { get; set; }
        public DbSet<SystemConfiguration> SystemConfiguration { get; set; }
        public DbSet<TemporaryTokensType> TemporaryTokensType { get; set; }
        public DbSet<UserTemporaryToken> UserTemporaryToken { get; set; }
        public DbSet<Folder> Folders { get; set; }
        public DbSet<FolderDirectory> FolderDirectories { get; set; }
        public DbSet<FolderUser> FolderUsers { get; set; }
        public DbSet<FolderDirectoryFile> FolderDirectoryFiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

        }

        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {

        }
    }
}
