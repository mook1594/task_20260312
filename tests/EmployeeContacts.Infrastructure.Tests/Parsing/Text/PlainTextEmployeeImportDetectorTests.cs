namespace EmployeeContacts.Infrastructure.Tests.Parsing.Text;

public class PlainTextEmployeeImportDetectorTests
{
    [Fact(DisplayName = "text/plain 입력이 유효한 JSON 배열이면 JSON parser를 선택한다.")]
    public void Resolve_ShouldReturnJsonParser_WhenContentIsValidJsonArray()
    {
        IPlainTextEmployeeImportDetector detector = InfrastructureTestHost.CreatePlainTextDetector();

        IEmployeeImportParser parser = detector.Resolve(
            """[{ "name": "김철수", "email": "kim@example.com", "tel": "01012345678", "joined": "2024-02-01" }]""");

        Assert.Equal("JsonEmployeeImportParser", parser.GetType().Name);
    }

    [Fact(DisplayName = "text/plain 입력이 JSON 배열이 아니면 CSV parser를 선택한다.")]
    public void Resolve_ShouldReturnCsvParser_WhenContentIsNotJsonArray()
    {
        IPlainTextEmployeeImportDetector detector = InfrastructureTestHost.CreatePlainTextDetector();

        IEmployeeImportParser parser = detector.Resolve("김철수,kim@example.com,01012345678,2024-02-01");

        Assert.Equal("CsvEmployeeImportParser", parser.GetType().Name);
    }

    [Fact(DisplayName = "detector는 parser 선택 과정에서 예외를 던지지 않는다.")]
    public void Resolve_ShouldNotThrow_WhenContentIsMalformedJson()
    {
        IPlainTextEmployeeImportDetector detector = InfrastructureTestHost.CreatePlainTextDetector();

        IEmployeeImportParser parser = detector.Resolve("{");

        Assert.Equal("CsvEmployeeImportParser", parser.GetType().Name);
    }
}
