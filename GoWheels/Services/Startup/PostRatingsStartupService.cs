using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using GoWheels.Services.Interfaces;

namespace GoWheels.Services.Startup
{
    public class PostRatingsStartupService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public PostRatingsStartupService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var ratingsService = scope.ServiceProvider.GetRequiredService<IRatingsService>();

            await ratingsService.RecalculateAllPostsRateAverageAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}