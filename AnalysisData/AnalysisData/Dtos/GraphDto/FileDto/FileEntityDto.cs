namespace AnalysisData.Dtos.GraphDto.FileDto;

public class FileEntityDto
{
    public int Id { get; set; }
    public string Category { get; set; }
    public string FileName { get; set; }

    public DateTime UploadDate { get; set; }
}