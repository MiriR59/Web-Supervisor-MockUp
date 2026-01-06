using WSV.Api.Models;

namespace WSV.Api.Data;

public static class DbInitializer
{
    public static void Seed(AppDbContext context)
    {
        if (context.Sources.Any())
            return;
        
        var sources = new List<Source>
        {
            new Source
            {
                Name = "Machine Alpha",
                Description = "Stable",
                IsEnabled = true,
                IsPublic = true,
                BehaviourProfile = "Stable"
            },

            new Source
            {
                Name = "Machine Beta",
                Description = "Wave",
                IsEnabled = true,
                BehaviourProfile = "Wave"
            },

            new Source
            {
                Name = "Machine Gamma",
                Description = "Spiky",
                IsEnabled = true,
                BehaviourProfile = "Spiky"
            }
        };

        context.Sources.AddRange(sources);
        context.SaveChanges();
    }
}