namespace EmployeeContacts.Infrastructure.Tests.Parsing.Json;

public class JsonEmployeeImportParserTests
{
    [Fact(DisplayName = "JSON 배열을 BulkEmployeeRecord 목록으로 변환한다.")]
    public async Task ParseAsync_ShouldParseJsonArray_ToBulkEmployeeRecords()
    {
        IEmployeeImportParser parser = InfrastructureTestHost.CreateJsonParser();

        IReadOnlyList<BulkEmployeeRecord> records = await parser.ParseAsync(
            """
            [
              { "name": "김철수", "email": "kim@example.com", "tel": "01012345678", "joined": "2024-02-01" },
              { "name": "박영희", "email": "park@example.com", "tel": "01087654321", "joined": "2024-02-02" }
            ]
            """,
            CancellationToken.None);

        Assert.Equal(2, records.Count);
        Assert.Equal(1, records[0].Row);
        Assert.Equal(2, records[1].Row);
        Assert.Equal("박영희", records[1].Name);
    }

    [Fact(DisplayName = "JSON 루트가 배열이 아니면 형식 오류로 실패한다.")]
    public async Task ParseAsync_ShouldThrow_WhenJsonRootIsNotArray()
    {
        IEmployeeImportParser parser = InfrastructureTestHost.CreateJsonParser();

        EmployeeApplicationException exception = await Assert.ThrowsAsync<EmployeeApplicationException>(() =>
            parser.ParseAsync(
                """{ "name": "김철수", "email": "kim@example.com", "tel": "01012345678", "joined": "2024-02-01" }""",
                CancellationToken.None));

        Assert.Equal("invalid_format", exception.Code);
    }

    [Fact(DisplayName = "JSON 배열 원소가 객체가 아니면 형식 오류로 실패한다.")]
    public async Task ParseAsync_ShouldThrow_WhenJsonElementIsNotObject()
    {
        IEmployeeImportParser parser = InfrastructureTestHost.CreateJsonParser();

        EmployeeApplicationException exception = await Assert.ThrowsAsync<EmployeeApplicationException>(() =>
            parser.ParseAsync(
                """["value"]""",
                CancellationToken.None));

        Assert.Equal("invalid_format", exception.Code);
    }

    [Fact(DisplayName = "JSON 필수 속성이 누락되면 형식 오류로 실패한다.")]
    public async Task ParseAsync_ShouldThrow_WhenJsonPropertyIsMissing()
    {
        IEmployeeImportParser parser = InfrastructureTestHost.CreateJsonParser();

        EmployeeApplicationException exception = await Assert.ThrowsAsync<EmployeeApplicationException>(() =>
            parser.ParseAsync(
                """[{ "name": "김철수", "email": "kim@example.com", "joined": "2024-02-01" }]""",
                CancellationToken.None));

        Assert.Equal("invalid_format", exception.Code);
    }

    [Fact(DisplayName = "JSON 필수 속성이 문자열이 아니면 형식 오류로 실패한다.")]
    public async Task ParseAsync_ShouldThrow_WhenJsonPropertyValueIsNotString()
    {
        IEmployeeImportParser parser = InfrastructureTestHost.CreateJsonParser();

        EmployeeApplicationException exception = await Assert.ThrowsAsync<EmployeeApplicationException>(() =>
            parser.ParseAsync(
                """[{ "name": "김철수", "email": "kim@example.com", "tel": 1012345678, "joined": "2024-02-01" }]""",
                CancellationToken.None));

        Assert.Equal("invalid_format", exception.Code);
    }

    [Fact(DisplayName = "JSON 추가 속성은 무시한다.")]
    public async Task ParseAsync_ShouldIgnoreAdditionalProperties()
    {
        IEmployeeImportParser parser = InfrastructureTestHost.CreateJsonParser();

        IReadOnlyList<BulkEmployeeRecord> records = await parser.ParseAsync(
            """[{ "name": "김철수", "email": "kim@example.com", "tel": "01012345678", "joined": "2024-02-01", "extra": "ignored" }]""",
            CancellationToken.None);

        Assert.Single(records);
        Assert.Equal("김철수", records[0].Name);
    }

    [Fact(DisplayName = "JSON 빈 배열은 형식 오류로 실패한다.")]
    public async Task ParseAsync_ShouldThrow_WhenJsonArrayIsEmpty()
    {
        IEmployeeImportParser parser = InfrastructureTestHost.CreateJsonParser();

        EmployeeApplicationException exception = await Assert.ThrowsAsync<EmployeeApplicationException>(() =>
            parser.ParseAsync("[]", CancellationToken.None));

        Assert.Equal("invalid_format", exception.Code);
    }
}
