using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using DinoShare.Helpers;
using DinoShare.Helpers.EmailServiceFactory;
using DinoShare.Models;
using DinoShare.ViewModels;
using DinoShare.ViewModels.DropzoneModelFactory;
using Hangfire;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DinoShare.Controllers
{
    [Microsoft.AspNetCore.Mvc.Route("api/FileShare")]
    [ApiController]
    [Microsoft.AspNetCore.Authorization.Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = PublicEnums.UserRoleList.ROLE_ADMINISTRATOR + "," + PublicEnums.UserRoleList.ROLE_USER)]
    public class ApiFileShareController : ControllerBase
    {
        private IWebHostEnvironment _env;
        private readonly AppDBContext _context;
        private readonly SecurityOptions _securityOptions;
        private readonly IEmailService _emailService;

        public ApiFileShareController(IWebHostEnvironment env, AppDBContext context, IOptions<SecurityOptions> securityOptions, IEmailService emailService)
        {
            _env = env;
            _context = context;
            _securityOptions = securityOptions.Value;
            _emailService = emailService;
        }

        [Microsoft.AspNetCore.Mvc.Route("UploadFile")]
        [Microsoft.AspNetCore.Mvc.HttpPost, DisableRequestSizeLimit]
        public async Task<HttpResponseMessage> UploadFile()
        {
            HttpResponseMessage response = new HttpResponseMessage { StatusCode = HttpStatusCode.Created };

            try
            {
                if (!Request.HasFormContentType)
                {
                    //No Files uploaded
                    response.StatusCode = HttpStatusCode.BadRequest;
                    response.Content = new StringContent("No file uploaded or MIME multipart content not as expected!");
                    throw new HttpResponseException(response);
                }

                Request.EnableBuffering();

                var meta = new DzMeta(HttpContext.Request.Form);

                var chunkDirBasePath = Path.Combine(_env.ContentRootPath, "App_Data");

                var path = string.Format(@"{0}\{1}", chunkDirBasePath, meta.dzIdentifier);
                var filename = string.Format(@"{0}.{1}.{2}.tmp", meta.dzFilename, (meta.intChunkNumber + 1).ToString().PadLeft(4, '0'), meta.dzTotalChunks.PadLeft(4, '0'));

                Directory.CreateDirectory(path);

                var files = Request.Form.Files;
                using (var stream = new FileStream(Path.Combine(path, filename), FileMode.Create))
                {
                    await files[0].CopyToAsync(stream);
                }
            }
            catch (HttpResponseException ex)
            {
                HelperFunctions.Log(_context, PublicEnums.LogLevel.LEVEL_EXCEPTION, "Controllers.ApiFileShareController.UploadFile", ex.Message, User, ex);
            }
            catch (Exception ex)
            {
                HelperFunctions.Log(_context, PublicEnums.LogLevel.LEVEL_EXCEPTION, "Controllers.ApiFileShareController.UploadFile", "Error uploading/saving chunk to file system", User, ex);
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Content = new StringContent(string.Format("Error uploading/saving chunk to file system: {0}", ex.Message));
            }

            return response;
        }

        [Microsoft.AspNetCore.Mvc.Route("DeleteCanceledChunks")]
        [Microsoft.AspNetCore.Mvc.HttpPost, DisableRequestSizeLimit]
        public HttpResponseMessage DeleteCanceledChunks([Microsoft.AspNetCore.Mvc.FromBody] DzCommit model)
        {
            HttpResponseMessage response = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };

            try
            {
                var chunkDirBasePath = Path.Combine(_env.ContentRootPath, "App_Data");
                var path = string.Format(@"{0}\{1}", chunkDirBasePath, model.dzIdentifier);

                BackgroundJob.Enqueue<BackgroundJobHelper>(x => x.DeleteFolder(path));
            }
            catch (Exception ex)
            {
                HelperFunctions.Log(_context, PublicEnums.LogLevel.LEVEL_EXCEPTION, "Controllers.ApiFileShareController.DeleteCanceledChunks", ex.Message, User, ex);
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Content = new StringContent(string.Format("Error deleting canceled chunks: {0}", ex.Message));
            }

            return response;
        }

        [Microsoft.AspNetCore.Mvc.Route("CommitChunks")]
        [Microsoft.AspNetCore.Mvc.HttpPost, DisableRequestSizeLimit]
        public async Task<HttpResponseMessage> CommitChunks([Microsoft.AspNetCore.Mvc.FromBody] DzCommit model)
        {
            HttpResponseMessage response = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };

            try
            {
                BackgroundJob.Enqueue<BackgroundJobHelper>(x => x.CommitFileChunks(model));
            }
            catch (Exception ex)
            {
                HelperFunctions.Log(_context, PublicEnums.LogLevel.LEVEL_EXCEPTION, "Controllers.ApiCourseManagementController.CommitChunks", ex.Message, User, ex);
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Content = new StringContent(string.Format("Error merging chunked upload: {0}", ex.Message));
            }

            return response;
        }
    }
}
