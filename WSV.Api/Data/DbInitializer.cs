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
                Description = "Wave",
                IsEnabled = true,
                IsPublic = true,
                Behaviour = BehaviourProfile.Stable
            },

            new Source
            {
                Name = "Machine Beta",
                Description = "Stable",
                IsEnabled = true,
                Behaviour = BehaviourProfile.Wave
            },

            new Source
            {
                Name = "Machine Gamma",
                Description = "Spiky",
                IsEnabled = true,
                Behaviour = BehaviourProfile.Spiky
            }
        };

        context.Sources.AddRange(sources);
        context.SaveChanges();
    }
}