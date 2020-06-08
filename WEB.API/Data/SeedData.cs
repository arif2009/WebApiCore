using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static WEB.API.Utility.Enums;

namespace WEB.API.Data
{
    public static class SeedData
    {
        public static void Seed(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole { Id = ((long)Roles.SuperAdmin).ToString(), Name = Roles.SuperAdmin.ToString(), NormalizedName = Roles.SuperAdmin.ToString().ToUpper(), ConcurrencyStamp = "88f0dec2-5364-4881-9817-1f2a135a8649" },
                new IdentityRole { Id = ((long)Roles.Admin).ToString(), Name = Roles.Admin.ToString(), NormalizedName = Roles.Admin.ToString().ToUpper(), ConcurrencyStamp = "5719c2b8-22fd-4eee-9c21-4bfbd2ce18d7" },
                new IdentityRole { Id = ((long)Roles.User).ToString(), Name = Roles.User.ToString(), NormalizedName = Roles.User.ToString().ToUpper(), ConcurrencyStamp = "eccc7115-422c-487d-95b0-58cfa8e66a94" }
            );
        }
    }
}
