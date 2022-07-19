#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebProject.Models;

namespace WebProject.Data
{
    public class WebProjectSQL : DbContext
    {
        public WebProjectSQL(DbContextOptions<WebProjectSQL> options)
            : base(options)
        {
        }

        public DbSet<WebProject.Models.UserModel> UserModel { get; set; }
    }
}
