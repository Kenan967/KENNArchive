namespace KENNArchive.Application.DTOs;

// Məhsulun bir ölçüsü və həmin ölçünün stoku.
public class ProductSizeDto
{
    public int Id { get; set; }

    public string Size { get; set; } = string.Empty;

    public int Stock { get; set; }
}
