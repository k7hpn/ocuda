﻿using Microsoft.EntityFrameworkCore;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Data
{
    public abstract class PromenadeContext : Utility.Data.DbContextBase
    {
        protected PromenadeContext(DbContextOptions options) : base(options) { }

        public DbSet<LocationHours> LocationHours { get; }
        public DbSet<Location> Location { get; }
        public DbSet<LocationGroup> LocationGroup { get; }
        public DbSet<LocationFeature> LocationFeature { get; }
        public DbSet<Group> Group { get; }
        public DbSet<Feature> Feature { get; }
    }
}
