﻿using Microsoft.EntityFrameworkCore;
using PlatformService.Models;

namespace PlatformService.Repo
{
    public class AppDbContext : DbContext
    {
        public AppDbContext( DbContextOptions<AppDbContext> opt) : base(opt)
        {
            
        }

        public DbSet<Platform> Platforms { get; set; }
    }
}
