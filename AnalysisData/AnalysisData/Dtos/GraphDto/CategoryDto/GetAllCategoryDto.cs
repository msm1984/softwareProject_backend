using AnalysisData.Models.GraphModel.Category;

namespace AnalysisData.Dtos.GraphDto.CategoryDto;

public class GetAllCategoryDto
{
    public IEnumerable<Category> Categories;

    public GetAllCategoryDto(IEnumerable<Category> categories)
    {
        Categories = categories;
    }
}