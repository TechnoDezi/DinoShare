using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DinoShare.ViewModels.DropzoneModelFactory
{
    public class DzCommit
    {
        public string dzIdentifier { get; set; }
        public string fileName { get; set; }
        public Int64 expectedBytes { get; set; }
        public Int64 totalChunks { get; set; }
        public Guid folderID { get; set; }
        public Guid? parentFolderDirectoryFileID { get; set; }
    }

    public class DzMeta
    {
        public Int64 intChunkNumber = 0;
        public string dzChunkNumber { get; set; }
        public string dzChunkSize { get; set; }
        public string dzCurrentChunkSize { get; set; }
        public string dzTotalSize { get; set; }
        public string dzIdentifier { get; set; }
        public string dzFilename { get; set; }
        public string dzTotalChunks { get; set; }
        public string dzCurrentChunkByteOffset { get; set; }
        public Guid folderID { get; set; }
        public Guid? parentFolderDirectoryFileID { get; set; }

        public DzMeta()
        {

        }

        public DzMeta(Dictionary<string, string> values)
        {
            dzChunkNumber = values["dzChunkIndex"];
            dzChunkSize = values["dzChunkSize"];
            dzCurrentChunkSize = values["dzCurrentChunkSize"];
            dzTotalSize = values["dzTotalFileSize"];
            dzIdentifier = values["dzUuid"];
            dzFilename = values["dzFileName"];
            dzTotalChunks = values["dzTotalChunkCount"];
            dzCurrentChunkByteOffset = values["dzChunkByteOffset"];
            folderID = Guid.Parse(values["folderID"]);

            if (values.ContainsKey("parentFolderDirectoryFileID") && !string.IsNullOrEmpty(values["parentFolderDirectoryFileID"]) && values["parentFolderDirectoryFileID"] != "null")
            {
                parentFolderDirectoryFileID = Guid.Parse(values["parentFolderDirectoryFileID"]);
            }
            else
            {
                parentFolderDirectoryFileID = null;
            }

            Int64.TryParse(dzChunkNumber, out intChunkNumber);
        }

        public DzMeta(IFormCollection values)
        {
            dzChunkNumber = values["dzChunkIndex"].First();
            dzChunkSize = values["dzChunkSize"].First();
            dzCurrentChunkSize = values["dzCurrentChunkSize"].First();
            dzTotalSize = values["dzTotalFileSize"].First();
            dzIdentifier = values["dzUuid"].First();
            dzFilename = values["dzFileName"].First();
            dzTotalChunks = values["dzTotalChunkCount"].First();
            dzCurrentChunkByteOffset = values["dzChunkByteOffset"].First();
            folderID = Guid.Parse(values["folderID"].First());

            if (values.ContainsKey("parentFolderDirectoryFileID") && !string.IsNullOrEmpty(values["parentFolderDirectoryFileID"].First()) && values["parentFolderDirectoryFileID"].First() != "null")
            {
                parentFolderDirectoryFileID = Guid.Parse(values["parentFolderDirectoryFileID"].First());
            }
            else
            {
                parentFolderDirectoryFileID = null;
            }

            Int64.TryParse(dzChunkNumber, out intChunkNumber);
        }
    }
}
