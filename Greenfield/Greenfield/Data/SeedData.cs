using Greenfield.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace Greenfield.Data
{
    public class SeedData
    {

        public static async Task SeedUserAndRoles(IServiceProvider serviceProvider, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            //Seeded my roles
            string[] roleNames = { "Admin", "Producer", "Standard", "Developer" }; //Here is a list of role
            foreach (string roleName in roleNames) //Creates a loop going through each role stored inside roleNames one by one
            {
                var roleExists = await roleManager.RoleExistsAsync(roleName); //Checks if the roles exists
                if (!roleExists) //If the role does not exist
                {
                    var role = new IdentityRole(roleName); 
                    await roleManager.CreateAsync(role);
                }
            }
            //Seeding users and assigning roles, one for each type of users for now
            var adminUser = await userManager.FindByEmailAsync("admin@example.com");
            if (adminUser == null)
            {
                adminUser = new IdentityUser { UserName = "admin@example.com", Email = "admin@example.com", EmailConfirmed = true };
                await userManager.CreateAsync(adminUser, "Password123!");
            }
            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            var producerUser = await userManager.FindByEmailAsync("producer@example.com");
            if (producerUser == null)
            {
                producerUser = new IdentityUser { UserName = "producer@example.com", Email = "producer@example.com", EmailConfirmed = true };
                await userManager.CreateAsync(producerUser, "Password123!");
            }

            if (!await userManager.IsInRoleAsync(producerUser, "Producer"))
            {
                await userManager.AddToRoleAsync(producerUser, "Producer");
            }

            var producerUser2 = await userManager.FindByEmailAsync("producer2@example.com");
            if (producerUser2 == null)
            {
                producerUser2 = new IdentityUser { UserName = "producer2@example.com", Email = "producer2@example.com", EmailConfirmed = true };
                await userManager.CreateAsync(producerUser2, "Password123!");
            }

            if (!await userManager.IsInRoleAsync(producerUser2, "Producer"))
            {
                await userManager.AddToRoleAsync(producerUser2, "Producer");
            }

            var producerUser3 = await userManager.FindByEmailAsync("producer3@example.com");
            if (producerUser3 == null)
            {
                producerUser3 = new IdentityUser { UserName = "producer3@example.com", Email = "producer3@example.com", EmailConfirmed = true };
                await userManager.CreateAsync(producerUser3, "Password123!");
            }

            if (!await userManager.IsInRoleAsync(producerUser3, "Producer"))
            {
                await userManager.AddToRoleAsync(producerUser3, "Producer");
            }

            var devUser = await userManager.FindByEmailAsync("dev@example.com");
            if (devUser == null)
            {
                devUser = new IdentityUser { UserName = "dev@example.com", Email = "dev@example.com", EmailConfirmed = true };
                await userManager.CreateAsync(devUser, "Password123!");
            }

            if (!await userManager.IsInRoleAsync(devUser, "Developer"))
            {
                await userManager.AddToRoleAsync(devUser, "Developer");
            }

            var normalUser = await userManager.FindByEmailAsync("user@example.com");
            if (normalUser == null)
            {
                normalUser = new IdentityUser { UserName = "user@example.com", Email = "user@example.com", EmailConfirmed = true };
                await userManager.CreateAsync(normalUser, "Password123!");
            }

            if (!await userManager.IsInRoleAsync(normalUser, "Standard"))
            {
                await userManager.AddToRoleAsync(normalUser, "Standard");
            }
        }
        public static async Task SeedProducers(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            //Find the user by email
            var ProducerUser1 = await userManager.FindByEmailAsync("producer@example.com");
            var ProducerUser2 = await userManager.FindByEmailAsync("producer2@example.com");
            var ProducerUser3 = await userManager.FindByEmailAsync("producer3@example.com");

            if (ProducerUser1 == null & ProducerUser2 == null & ProducerUser3 == null)
            {
                throw new Exception("Producer user not found");
            }

            //Prevent duplicate seeding
            if (context.Producer.Any())
                return;

            var producers = new List<Producer>
            {
                new Producer
                {
                    ProducerName = "Harvest Moon Farm",
                    ProducerEmail = "contact@harvest.co.uk",
                    ProducerInformation = "Supporting local markets with seasonal, high-quality farm produce.",
                    UserId = ProducerUser1.Id
                },
                new Producer
                {
                    ProducerName = "WildCare Farm",
                    ProducerEmail = "farm@wildcare.co.uk",
                    ProducerInformation = "Producing carefully crafted, high-quality farm goods with attention to detail at every stage.",
                    UserId = ProducerUser2.Id
                },
                new Producer
                {
                    ProducerName = "Saint Micheal Farm",
                    ProducerEmail = "saintmicheal@farm.ac.uk",
                    ProducerInformation = "Local farm located in Sandwell supplying organic fruits and vegetables",
                    UserId = ProducerUser3.Id
                }
            };

            context.Producer.AddRange(producers);
            await context.SaveChangesAsync();
        }
        public static async Task SeedProducts(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            //Find the producer
            var WildCareFarm = await context.Producer.FirstOrDefaultAsync(x => x.ProducerName == "WildCare Farm");
            var HarvestMoonFarm = await context.Producer.FirstOrDefaultAsync(x => x.ProducerName == "Harvest Moon Farm");
            var SaintMichealFarm = await context.Producer.FirstOrDefaultAsync(x => x.ProducerName == "Saint Micheal Farm");

            if (WildCareFarm == null & HarvestMoonFarm == null & SaintMichealFarm == null)
            {
                throw new Exception("Producers not found");
            }

            if (!context.Product.Any())
            {
                var products = new List<Product>()
                {
                    new Product
                    {
                        ProductName = "Egg",
                        stock = 15,
                        Price = 1.50f,
                        ProducerId = HarvestMoonFarm.ProducerId,
                        ImagePath = "/images/egg.png",
                        Description = "A smooth, oval product laid by birds",
                        IsAvailable = true,
                        Rating = 4f
                    },
                    new Product
                    {
                        ProductName = "Carrot",
                        stock = 32,
                        Price = 3.40f,
                        ProducerId = WildCareFarm.ProducerId,
                        ImagePath = "/images/carrot.png",
                        Description = "A long, orange, root vegetable",
                        IsAvailable= true,
                        Rating = 5f
                        
                    },
                    new Product
                    {
                        ProductName = "Orange",
                        stock = 3,
                        Price = 2.50f,
                        ProducerId = SaintMichealFarm.ProducerId,
                        ImagePath = "/images/orange.png",
                        Description = "A round fruit which is a good source for vitamin D.", 
                        IsAvailable= true,
                        Rating = 5f
                    },
                    new Product
                    {
                        ProductName = "Corn",
                        stock = 17,
                        Price = 1.60f,
                        ProducerId = WildCareFarm.ProducerId,
                        ImagePath = "/images/corn.png",
                        Description = "A tall plant",
                        IsAvailable= true,
                        Rating = 4f
                    }
                };

                await context.Product.AddRangeAsync(products);
                await context.SaveChangesAsync();
            }
        } 
    }
}
