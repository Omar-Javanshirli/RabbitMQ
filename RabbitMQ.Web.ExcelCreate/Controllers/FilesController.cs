using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Web.ExcelCreate.Hubs;
using RabbitMQ.Web.ExcelCreate.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace RabbitMQ.Web.ExcelCreate.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly AppDbContext context;
        private readonly IHubContext<MyHub> hubContext;

        public FilesController(AppDbContext context, IHubContext<MyHub> hubContext)
        {
            this.context = context;
            this.hubContext = hubContext;
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile excelFile, int fileId)
        {
            if (excelFile is not { Length: > 0 })
                return BadRequest();

            var userFile = await this.context.UserFiles.FirstAsync(x => x.Id == fileId);

            //Path.GetExtension(excelFile.FileName) => file-in adini gonderirik hansi formatda oldugunu deyir (png,jpg)ve.s
            var filePath = userFile.FileName + Path.GetExtension(excelFile.FileName);

            //wwwroot-un icinde ki files papkasina gelen file save edirik.
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/files", filePath);

            await using FileStream stream = new(path, FileMode.Create);
            await excelFile.CopyToAsync(stream);

            userFile.CreatedDate = DateTime.Now;
            userFile.FilePath = filePath;
            userFile.FileStatus = FileStatus.Complated;

            await this.context.SaveChangesAsync();

            //========================SignalR========================
            await hubContext.Clients.User(userFile.UserId).SendAsync("CompletedFile");

            return Ok();
        }
    }
}
