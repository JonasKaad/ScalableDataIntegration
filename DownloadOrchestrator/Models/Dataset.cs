using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DownloadOrchestrator.Models;

[Table("datasets")]
public class Dataset
{
    [Column("id")]
    public int Id { get; init; }
    [Column("parser"), MaxLength(255)] // VARCHAR(255)
    public string? Parser { get; init; }
    [Column("date")]
    public DateTime Date { get; init; }
    [Column("downloaded_amount")]
    public int DownloadedAmount { get; init; }
}