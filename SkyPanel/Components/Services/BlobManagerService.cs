using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DotNetEnv;

namespace SkyPanel.Components.Services;

public class BlobManagerService
{
    private BlobServiceClient ServiceClient { get; }
    private IList<BlobContainerItem> blobContainers;

    private Dictionary<BlobContainerItem, List<BlobItem>> blobs = new Dictionary<BlobContainerItem, List<BlobItem>>();

    public BlobManagerService(string connectionString) 
    {
        ServiceClient = new BlobServiceClient(connectionString);
        blobContainers = new List<BlobContainerItem>();
        blobs = new Dictionary<BlobContainerItem, List<BlobItem>>();
    }


    public void GetContainers()
    {
        blobContainers.Clear();
        blobs.Clear();
        
        foreach (var container in ServiceClient.GetBlobContainers())
        {
            blobContainers.Add(container);
        }
    } 
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