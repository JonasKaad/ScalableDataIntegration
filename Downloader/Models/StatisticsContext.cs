using Microsoft.EntityFrameworkCore;

namespace Sdi.Parser.Models;


public class StatisticsContext: DbContext
{
    public StatisticsContext(DbContextOptions<StatisticsContext> options) : base(options) { }
    
    public DbSet<Dataset> Datasets { get; set; }
}