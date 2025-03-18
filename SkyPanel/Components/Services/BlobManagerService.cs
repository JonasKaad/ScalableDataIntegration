using System.Globalization;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using SkyPanel.Components.Models;

namespace SkyPanel.Components.Services;

public class BlobManagerService
{
    private BlobServiceClient ServiceClient { get; }
    private IList<BlobContainerItem> _blobContainers;
    private List<BlobDataItem> _blobDataItems = new List<BlobDataItem>();
    private Dictionary<BlobContainerItem, List<BlobItem>> _blobs;

    public BlobManagerService(string connectionString) 
    {
        ServiceClient = new BlobServiceClient(connectionString);
        _blobContainers = new List<BlobContainerItem>();
        _blobs = new Dictionary<BlobContainerItem, List<BlobItem>>();
    }


    public void GetContainers()
    {
        _blobContainers.Clear();
        _blobs.Clear();
        
        foreach (var container in ServiceClient.GetBlobContainers())
        {
            _blobContainers.Add(container);
        }
    } 
    
    /// <summary>
    /// Fetches the names of the containers in Azure Blob Storage
    /// </summary>
    /// <returns>
    /// A list of container names found in Azure Blob Storage
    /// </returns>
    public List<string> GetContainerNames()
    {
        var containerNames = new List<string>();        
        
        foreach (var container in ServiceClient.GetBlobContainers())
        {
            containerNames.Add(container.Name);
        }
        
        return containerNames;
    }
    public void GetAllBlobItems()
    {
        foreach (var container in blobContainers)
        {
            Console.WriteLine(container.Name);
            var containerClient = ServiceClient.GetBlobContainerClient(container.Name);
            var blobItems = new List<BlobItem>();
            foreach (var blob in containerClient.GetBlobs())
            {
                blobItems.Add(blob);
            }
            blobs.Add(container, blobItems);
        }
    }


    /// <summary>
    /// Extracts a unique key from a blob name to match raw and parsed files
    /// </summary>
    /// <param name="containerName">The name of the container</param>
    /// <param name="blobName">The path/name of the blob</param>
    /// <returns>A key that uniquely identifies the dataset</returns>
    private string ExtractKeyFromBlobName(string containerName, string blobName)
    {
        string timestamp = blobName;

        if (blobName.Contains("_raw"))
        {
            timestamp = blobName.Substring(0, blobName.IndexOf("_raw", StringComparison.Ordinal));
        }
        if (blobName.Contains("-raw"))
        {
            timestamp = blobName.Substring(0, blobName.IndexOf("-raw", StringComparison.Ordinal));
        }
        if (blobName.Contains("_parsed"))
        {
            timestamp = blobName.Substring(0, blobName.IndexOf("_parsed", StringComparison.Ordinal));
        }
        if (blobName.Contains("-parsed"))
        {
            timestamp = blobName.Substring(0, blobName.IndexOf("-parsed", StringComparison.Ordinal));
        }
        return $"{containerName}_{timestamp}";
    }

    public void PrintDictionary()
    {
        foreach (var (container, blob) in blobs)
        {
            foreach (var blobItem in blob)
            {
                Console.WriteLine(container.Name + " " + blobItem.Name);
            }
        }
    }
    
    public void GetBlobItems(string containerName)
    {
        var containerClient = ServiceClient.GetBlobContainerClient(containerName);
        foreach (var blobItem in containerClient.GetBlobs())
        {
            Console.WriteLine("\t" + blobItem.Name);
        }
    }
}