using Barbershop.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Barbershop.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Services> Services { get; set; }
        public DbSet<Products> Products { get; set; }
        public DbSet<ProductCategory> ProductCategory { get; set; }
        public DbSet<Specializations> Specializations { get; set; }
        public DbSet<WorkPositions> WorkPositions { get; set; }
        public DbSet<Barbers> Barbers { get; set; }
        public DbSet<BarbershopUser> BarbershopUser { get; set; }
        public DbSet<ProductImages> ProductImages { get; set; }
        public DbSet<OrderHeader> OrderHeader { get; set; }
        public DbSet<OrderDetail> OrderDetail { get; set; }
        public DbSet<BarberSchedule> BarberSchedule { get; set; }   
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<AppointmentDetail> AppointmentDetails { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<News> News { get; set; }
        public DbSet<NewsImages> NewsImages { get; set; }
        public DbSet<PromoCodes> PromoCodes { get; set; }


    }
}
