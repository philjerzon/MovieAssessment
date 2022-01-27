using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace MovieCentral.Models
{
    public class dbContext : DbContext
    {
        public dbContext() : base("DbConnection") { }
        public DbSet<accountModel> accounts { get; set; }
        public DbSet<accountRoleModel> accountRoles { get; set; }
        public DbSet<movieModel> movies { get; set; }
        public DbSet<movieSchedModel> movieScheds { get; set; }
        public DbSet<customerModel> customers { get; set; }
        public DbSet<seatRowModel> seatRows { get; set; }
        public DbSet<seatModel> seats { get; set; }
        public DbSet<reservationModel> reservations { get; set; }

    }
}