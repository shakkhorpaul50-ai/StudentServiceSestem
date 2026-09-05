using Microsoft.AspNetCore.Identity;
using IUBAT_Student_Service.Models;

namespace IUBAT_Student_Service.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roles = { "Student", "Staff" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            string staffEmail = "s.paul@iubat.edu";
            const string staffPassword = "P@ssW0rd";
            var existingStaff = await userManager.FindByEmailAsync(staffEmail);
            if (existingStaff == null)
            {
                var staffUser = new ApplicationUser
                {
                    UserName = staffEmail,
                    Email = staffEmail,
                    FirstName = "Shakkhor",
                    LastName = "Paul",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(staffUser, staffPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(staffUser, "Staff");
                    Console.WriteLine($"[Seed] Staff {staffEmail} created with P@ssW0rd");
                }
                else
                {
                    Console.WriteLine($"[Seed] Failed to create staff {staffEmail}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                // Ensure Staff role
                if (!await userManager.IsInRoleAsync(existingStaff, "Staff"))
                {
                    await userManager.AddToRoleAsync(existingStaff, "Staff");
                    Console.WriteLine($"[Seed] Added Staff role to {staffEmail}");
                }
                // Ensure password is P@ssW0rd — reset if needed
                var check = await userManager.CheckPasswordAsync(existingStaff, staffPassword);
                if (!check)
                {
                    var token = await userManager.GeneratePasswordResetTokenAsync(existingStaff);
                    var reset = await userManager.ResetPasswordAsync(existingStaff, token, staffPassword);
                    if (reset.Succeeded)
                        Console.WriteLine($"[Seed] Staff {staffEmail} password reset to P@ssW0rd");
                    else
                        Console.WriteLine($"[Seed] Failed to reset password for {staffEmail}: {string.Join(", ", reset.Errors.Select(e => e.Description))}");
                }
                // Ensure email confirmed
                if (!existingStaff.EmailConfirmed)
                {
                    existingStaff.EmailConfirmed = true;
                    await userManager.UpdateAsync(existingStaff);
                }
            }
        }
    }
}
