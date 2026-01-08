using ClosedXML.Excel;
using JoiabagurPV.Application.Interfaces;

namespace JoiabagurPV.Application.Services;

/// <summary>
/// Service for generating Excel templates with consistent formatting.
/// </summary>
public class ExcelTemplateService : IExcelTemplateService
{
    /// <inheritdoc/>
    public MemoryStream GenerateTemplate(ExcelTemplateConfig config)
    {
        var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(config.SheetName);

        // Add headers
        for (int i = 0; i < config.Columns.Count; i++)
        {
            var column = config.Columns[i];
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = column.Name;

            // Style header
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
            cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Add required indicator
            if (column.IsRequired)
            {
                cell.Style.Font.FontColor = XLColor.DarkRed;
            }

            // Add comment with description if provided
            if (!string.IsNullOrWhiteSpace(column.Description))
            {
                var comment = cell.CreateComment();
                comment.AddText(column.Description);
            }

            // Set column width
            if (column.Width.HasValue)
            {
                worksheet.Column(i + 1).Width = column.Width.Value;
            }
            else
            {
                // Auto-fit with minimum width
                worksheet.Column(i + 1).Width = Math.Max(column.Name.Length + 2, 12);
            }

            // Add data validation for the data column (rows 2-1000)
            var dataRange = worksheet.Range(2, i + 1, 1000, i + 1);
            
            switch (column.DataType)
            {
                case ExcelDataType.Integer:
                    var intValidation = dataRange.CreateDataValidation();
                    intValidation.WholeNumber.Between(
                        (int)(column.MinValue ?? int.MinValue),
                        (int)(column.MaxValue ?? int.MaxValue));
                    intValidation.ErrorMessage = $"{column.Name} must be a whole number.";
                    intValidation.ErrorStyle = XLErrorStyle.Warning;
                    break;

                case ExcelDataType.Decimal:
                    var decValidation = dataRange.CreateDataValidation();
                    decValidation.Decimal.Between(
                        (double)(column.MinValue ?? decimal.MinValue),
                        (double)(column.MaxValue ?? decimal.MaxValue));
                    decValidation.ErrorMessage = $"{column.Name} must be a number.";
                    decValidation.ErrorStyle = XLErrorStyle.Warning;
                    break;
            }
        }

        // Protect header row
        worksheet.Row(1).Style.Protection.Locked = true;

        // Add example rows
        for (int rowIndex = 0; rowIndex < config.ExampleRows.Count; rowIndex++)
        {
            var exampleRow = config.ExampleRows[rowIndex];
            for (int colIndex = 0; colIndex < config.Columns.Count; colIndex++)
            {
                var column = config.Columns[colIndex];
                if (exampleRow.TryGetValue(column.Name, out var value) && value != null)
                {
                    var cell = worksheet.Cell(rowIndex + 2, colIndex + 1);
                    switch (value)
                    {
                        case decimal d:
                            cell.Value = d;
                            cell.Style.NumberFormat.Format = "#,##0.00";
                            break;
                        case int i:
                            cell.Value = i;
                            break;
                        case double db:
                            cell.Value = db;
                            cell.Style.NumberFormat.Format = "#,##0.00";
                            break;
                        default:
                            cell.Value = value.ToString();
                            break;
                    }
                    // Mark example rows with italic
                    cell.Style.Font.Italic = true;
                    cell.Style.Font.FontColor = XLColor.Gray;
                }
            }
        }

        // Add instructions as a comment on cell A1 if provided
        if (!string.IsNullOrWhiteSpace(config.Instructions))
        {
            var instructionCell = worksheet.Cell(1, 1);
            var existingComment = instructionCell.GetComment();
            if (existingComment != null)
            {
                existingComment.AddNewLine();
                existingComment.AddText("---");
                existingComment.AddNewLine();
                existingComment.AddText(config.Instructions);
            }
            else
            {
                var comment = instructionCell.CreateComment();
                comment.AddText(config.Instructions);
            }
        }

        // Freeze header row
        worksheet.SheetView.FreezeRows(1);

        // Apply auto-filter
        if (config.Columns.Count > 0)
        {
            worksheet.Range(1, 1, 1, config.Columns.Count).SetAutoFilter();
        }

        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        return stream;
    }
}

