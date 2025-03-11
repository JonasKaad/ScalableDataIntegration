using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace SkyPanel.Components.Features;

public partial class ParserPanel : ComponentBase
{
    IList<IBrowserFile> _files = new List<IBrowserFile>();
    private void UploadFiles(IReadOnlyList<IBrowserFile>? files)
    {
        //TODO upload the files to the server / handle them
        if (files != null) {
            foreach (var file in files)
            {
                _files.Add(file);
            }
        }
    }

    private void UploadFiles2(IBrowserFile file)
    {
        //TODO upload the file to the server / handle it
        _files.Add(file);
    }
}