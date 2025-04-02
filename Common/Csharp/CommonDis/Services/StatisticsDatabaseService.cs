using CommonDis.Models;
using Microsoft.EntityFrameworkCore;

namespace CommonDis.Services;

public class StatisticsDatabaseService: DbContext
{
    public StatisticsDatabaseService(DbContextOptions<StatisticsDatabaseService> options) : base(options) { }
    
    public DbSet<Dataset> Datasets { get; set; }
}