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

    public async Task<List<BlobDataItem>> SetupAndReturnBlobs()
    {
        await GetContainers();
        await GetAllBlobItems();
        
        CreateBlobDataItems();
        return GetBlobDataItems();;
    }

    private async Task GetContainers()
    {
        _blobContainers.Clear();
        _blobs.Clear();

        await foreach (var container in ServiceClient.GetBlobContainersAsync())
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

    private async Task GetAllBlobItems()
    {
        foreach (var container in _blobContainers)
        {
            var containerClient = ServiceClient.GetBlobContainerClient(container.Name);
            var blobItems = new List<BlobItem>();
            await foreach (var blob in containerClient.GetBlobsAsync())
            {
                blobItems.Add(blob);
            }
            _blobs.Add(container, blobItems);
        }
    }
    
    /// <summary> 
    /// Creates a list of BlobDataItems from the blobs in the dictionary by pairing raw and parsed files
    /// </summary>
    private void CreateBlobDataItems()
    {
        _blobDataItems.Clear();
        // Dictionary to store raw and parsed file paths
        var rawFiles = new Dictionary<string, (string? containerName, string blobPath)>();
        var parsedFiles = new Dictionary<string, (string? containerName, string blobPath)>();

        
        // Categorize blobs as raw or parsed based one if they contain "_raw" or "_parsed" in their name
        foreach (var (container, blob) in _blobs)
        {
            foreach (var blobItem in blob)
            {
                string blobName = blobItem.Name;
                string containerName = container.Name;
                string key = ExtractKeyFromBlobName(containerName, blobName);

                if (blobName.Contains("_raw") || blobName.Contains("-raw"))
                {
                    rawFiles[key] = (containerName, blobName);
                }
                else if (blobName.Contains("_parsed") || blobName.Contains("-parsed"))
                {
                    parsedFiles[key] = (containerName, blobName);
                }
            }
        }

        // Iterate through raw files and pair them with parsed files (if they exist)
        foreach (var key in rawFiles.Keys)
        {
            string? parsedPath = null;
            if (parsedFiles.TryGetValue(key, out var parsedInfo))
            {
                parsedPath = parsedInfo.blobPath;
            }
            
            var rawInfo = rawFiles[key];
            DateTime dateTime = ParseBlobToDateTime(rawInfo.blobPath);
            
            _blobDataItems.Add(new BlobDataItem(
                rawInfo.containerName, 
                dateTime,
                rawInfo.blobPath, 
                parsedPath));
        }
        
        // Add any parsed files that don't have a raw counterpart
        foreach (var key in parsedFiles.Keys)
        {
            if (!rawFiles.ContainsKey(key))
            {
                var parsedInfo = parsedFiles[key];
                DateTime dateTime = ParseBlobToDateTime(parsedInfo.blobPath);
            
                _blobDataItems.Add(new BlobDataItem(
                    parsedInfo.containerName,
                    dateTime,
                    null,
                    parsedInfo.blobPath
                ));
            }
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

    /// <summary>
    /// Parses a blob path/name in format "yyyy/MM/dd/HHmm-xxx.txt" into a DateTime object
    /// </summary>
    /// <param name="blobName">The path/name of the blob</param>
    /// <returns>A DateTime object representing the date and time in the blob path</returns>
    private DateTime ParseBlobToDateTime(string blobName)
    {
        try
        {
            DateTime t;
            // yyyy/MM/dd/HHmm format
            string datePortion = blobName.Substring(0, 15);
            if (DateTime.TryParseExact(datePortion, "yyyy/MM/dd/HHmm", null, DateTimeStyles.AdjustToUniversal, out t))
            {
                return t;
            }
            
            // yyyy/MM/dd/HH format
            datePortion = blobName.Substring(0, 13);
            if (DateTime.TryParseExact(datePortion, "yyyy/MM/dd/HH", null, DateTimeStyles.AdjustToUniversal, out t))
            {
                return t;
            }
            
            // yyyy/MM/dd format
            datePortion = blobName.Substring(0, 10);
            if (DateTime.TryParseExact(datePortion, "yyyy/MM/dd", null, DateTimeStyles.AdjustToUniversal, out t))
            {
                return t;
            }
            
            // yyyy/MM format
            datePortion = blobName.Substring(0, 7);
            if (DateTime.TryParseExact(datePortion, "yyyy/MM", null, DateTimeStyles.AdjustToUniversal, out t))
            {
                return t;
            }
            
            // yyyy format
            datePortion = blobName.Substring(0, 4);
            if (DateTime.TryParseExact(datePortion, "yyyy", null, DateTimeStyles.AdjustToUniversal, out t))
            {
                return t;
            }
            
            return DateTime.Now;

        }
        catch (Exception ex) when (ex is FormatException || ex is IndexOutOfRangeException)
        {
            Console.WriteLine($"Failed to parse blob path: {blobName}. Error: {ex.Message}");
            throw new FormatException($"Could not parse blob path '{blobName}' into DateTime", ex);
        }
    }

    private List<BlobDataItem> GetBlobDataItems()
    {
        return _blobDataItems;
        
    }

    public void PrintBlobDataItems()
    {
        foreach (var blobDataItem in _blobDataItems)
        {
            Console.WriteLine(blobDataItem);
        }
    }
    public void PrintDictionary()
    {
        foreach (var (container, blob) in _blobs)
        {
            foreach (var blobItem in blob)
            {
                Console.WriteLine(container.Name + " " + blobItem.Name);
            }
        }
    }
    
    /// <summary>
    /// Refreshes the blob containers and data items from Azure Storage
    /// </summary>
    /// <returns>A list of updated BlobDataItems</returns>
    public async Task<List<BlobDataItem>> RefreshBlobsAsync()
    {
        // Clear existing collections
        _blobContainers.Clear();
        _blobs.Clear();
        _blobDataItems.Clear();
    
        // Reuse the existing functionality to get fresh data
        return await SetupAndReturnBlobs();
    }
    
    public async Task<Stream> DownloadBlob(string? containerName, string? blobName)
    {
        var containerClient = ServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);
        
        var st = await blobClient.DownloadStreamingAsync();
        return st.Value.Content;
    }
}