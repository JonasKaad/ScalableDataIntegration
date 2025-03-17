using Microsoft.EntityFrameworkCore;
using SkyPanel.Components.Models;

namespace SkyPanel.Components.Services;

public class StatisticsDatabaseService: DbContext
{
    public StatisticsDatabaseService(DbContextOptions<StatisticsDatabaseService> options) : base(options) { }
    
    public DbSet<Dataset> Datasets { get; set; }
}