namespace SkyPanel.Components.Models;

public class UploadResult
{
    public string? Message { get; set; }
    public bool Success { get; set; }
    
    public Result? Result { get; set; }
    
    public UploadResult(bool success, string? message = "", Result? result = Models.Result.None)
    {
        Message = message;
        Success = success;
        Result = result;
    }
}

public enum Result
{
    None,
    AlreadyExists,
    ExceptionOccured,
    UploadSuccess,
    UploadError,
    FileFormatError,
}