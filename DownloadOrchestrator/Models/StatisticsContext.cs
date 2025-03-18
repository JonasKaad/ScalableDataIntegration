using Microsoft.EntityFrameworkCore;

namespace DownloadOrchestrator.Models;

public class StatisticsContext: DbContext
{
    public StatisticsContext(DbContextOptions<StatisticsContext> options) : base(options) { }
    
    public DbSet<Dataset> Datasets { get; set; }
}