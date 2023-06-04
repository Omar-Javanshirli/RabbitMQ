using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace RabbitMQ.Web.ExcelCreate.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }



        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var hasUser = await this.userManager.FindByEmailAsync(email);

            if (hasUser == null)
                return View();

            var signInResult = await this.signInManager.PasswordSignInAsync(hasUser, password, true, false);
            
            if(!signInResult.Succeeded)
                return View(signInResult);

            return RedirectToAction(nameof(HomeController.Index),"Home");
        }
    }
}
