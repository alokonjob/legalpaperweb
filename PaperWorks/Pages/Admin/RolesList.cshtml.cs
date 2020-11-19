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
    public class RolesListModel : PageModel
    {
        private readonly RoleManager<MongoRole> roleManager;

        public List<string> AllRoles { get; set; }

        public RolesListModel(RoleManager<MongoRole> roleManager)
        {
            this.roleManager = roleManager;
        }
        public async Task<IActionResult> OnGet()
        {
            
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string roleName)
        {
            MongoRole Role = new MongoRole();
            Role.Name = roleName;// c_$!5V;
            await roleManager.CreateAsync(Role);
            return Page();
        }
    }
}