using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Web.ExcelCreate.Models;
using RabbitMQ.Web.ExcelCreate.Servicies;
using System;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;

namespace RabbitMQ.Web.ExcelCreate.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly AppDbContext appDbContext;
        private readonly UserManager<IdentityUser> userManager;
        private readonly RabbitMQPublisher rabbitMQPublisher;

        public ProductController(AppDbContext appDbContext, UserManager<IdentityUser> userManager, RabbitMQPublisher rabbitMQPublisher)
        {
            this.appDbContext = appDbContext;
            this.userManager = userManager;
            this.rabbitMQPublisher = rabbitMQPublisher;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> CreateProductExcel()
        {
            var user = await this.userManager.FindByNameAsync(User.Identity.Name);

            var fileName = $"product-excel-{Guid.NewGuid().ToString().Substring(1, 10)}";

            UserFile userFile = new()
            {
                UserId = user.Id,
                FileName = fileName,
                FileStatus = FileStatus.Creating
            };

            await this.appDbContext.UserFiles.AddAsync(userFile);
            await this.appDbContext.SaveChangesAsync();

            this.rabbitMQPublisher.Publish(new Shared.CreateExcelMessage() { FileId=userFile.Id});

            TempData["StartCreatingExcell"] = true;

            return RedirectToAction(nameof(Files));
        }

        public async Task<IActionResult> Files()
        {
            var user = await this.userManager.FindByNameAsync(User.Identity.Name);

            return View(await this.appDbContext.UserFiles.Where(x => x.UserId == user.Id).ToListAsync());
        }
    }
}
