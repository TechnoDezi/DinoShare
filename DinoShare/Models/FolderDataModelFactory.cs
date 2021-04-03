using DinoShare.Models.AccountDataModelFactory;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DinoShare.Models.FolderDataModelFactory
{
    public class Folder
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid FolderID { get; set; }
        [ForeignKey("ParentFolder")]
        public Guid? ParentFolderID { get; set; }
        public string Description { get; set; }

        public Folder ParentFolder { get; set; }
    }

    public class FolderDirectory
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid FolderDirectoryID { get; set; }
        public Guid FolderID { get; set; }
        public string FolderPath { get; set; }
        public bool IsUploadDirectory { get; set; }
        public bool RequireCredentials { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public Folder Folder { get; set; }
    }

    public class FolderUser
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid FolderUserID { get; set; }
        public Guid FolderID { get; set; }
        public Guid UserID { get; set; }

        public bool AllowEdit { get; set; }
        public bool AllowDelete { get; set; }

        public Folder Folder { get; set; }
        public User User { get; set; }
    }

    public class FolderDirectoryFile
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid FolderDirectoryFileID { get; set; }
        public Guid FolderDirectoryID { get; set; }
        
        public string FileName { get; set; }
        public string FileType { get; set; }
        public string FileExtention { get; set; }
        public string FullPath { get; set; }
        public string SizeMB { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsDirectory { get; set; }
        public Guid? ParentFolderDirectoryFileID { get; set; }

        public FolderDirectory FolderDirectory { get; set; }
    }
}
