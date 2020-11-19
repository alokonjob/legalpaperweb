using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCore.Identity.Mongo.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PaperWorks
{
    public class CreateRoleModel : PageModel
    {
        private readonly RoleManager<MongoRole> roleManager;

        public CreateRoleModel(RoleManager<MongoRole> roleManager)
        {
            this.roleManager = roleManager;
        }

        [BindProperty]
        public string RoleName { get; set; }
        
        public void OnGet()
        {

        }

        public async Task<IActionResult> OnPostAsync()
        {
            MongoRole newRole = new MongoRole();
            newRole.Name = RoleName;
            await roleManager.CreateAsync(newRole);
            return RedirectToPage("/Admin/RolesList");
        }

        
    }
}