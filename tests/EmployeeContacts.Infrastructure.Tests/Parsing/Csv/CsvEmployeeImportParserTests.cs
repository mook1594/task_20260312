namespace EmployeeContacts.Infrastructure.Tests.Parsing.Csv;

public class CsvEmployeeImportParserTests
{
    [Fact(DisplayName = "CSV 헤더가 있으면 첫 데이터 행부터 row 1을 부여한다.")]
    public async Task ParseAsync_ShouldAssignRowNumbers_FromFirstDataRow_WhenHeaderExists()
    {
        IEmployeeImportParser parser = InfrastructureTestHost.CreateCsvParser();

        IReadOnlyList<BulkEmployeeRecord> records = await parser.ParseAsync(
            "name,email,tel,joined\n김철수,kim@example.com,01012345678,2024-02-01\n박영희,park@example.com,01087654321,2024-02-02",
            CancellationToken.None);

        Assert.Equal(2, records.Count);
        Assert.Equal(1, records[0].Row);
        Assert.Equal(2, records[1].Row);
    }

    [Fact(DisplayName = "CSV 헤더가 없으면 첫 줄을 row 1로 처리한다.")]
    public async Task ParseAsync_ShouldAssignRowNumbers_FromFirstLine_WhenHeaderDoesNotExist()
    {
        IEmployeeImportParser parser = InfrastructureTestHost.CreateCsvParser();

        IReadOnlyList<BulkEmployeeRecord> records = await parser.ParseAsync(
            "김철수,kim@example.com,01012345678,2024-02-01\n박영희,park@example.com,01087654321,2024-02-02",
            CancellationToken.None);

        Assert.Equal(1, records[0].Row);
        Assert.Equal(2, records[1].Row);
    }

    [Fact(DisplayName = "CSV 헤더 순서가 다르면 형식 오류로 실패한다.")]
    public async Task ParseAsync_ShouldThrow_WhenCsvHeaderOrderIsInvalid()
    {
        IEmployeeImportParser parser = InfrastructureTestHost.CreateCsvParser();

        EmployeeApplicationException exception = await Assert.ThrowsAsync<EmployeeApplicationException>(() =>
            parser.ParseAsync(
                "name,tel,email,joined\n김철수,01012345678,kim@example.com,2024-02-01",
                CancellationToken.None));

        Assert.Equal("invalid_format", exception.Code);
    }

    [Fact(DisplayName = "CSV 컬럼 수가 4개가 아니면 형식 오류로 실패한다.")]
    public async Task ParseAsync_ShouldThrow_WhenCsvColumnCountIsInvalid()
    {
        IEmployeeImportParser parser = InfrastructureTestHost.CreateCsvParser();

        EmployeeApplicationException exception = await Assert.ThrowsAsync<EmployeeApplicationException>(() =>
            parser.ParseAsync(
                "김철수,kim@example.com,01012345678",
                CancellationToken.None));

        Assert.Equal("invalid_format", exception.Code);
    }

    [Fact(DisplayName = "CSV 큰따옴표 인용은 지원하지 않는다.")]
    public async Task ParseAsync_ShouldThrow_WhenCsvContainsQuotedField()
    {
        IEmployeeImportParser parser = InfrastructureTestHost.CreateCsvParser();

        EmployeeApplicationException exception = await Assert.ThrowsAsync<EmployeeApplicationException>(() =>
            parser.ParseAsync(
                "\"김철수\",kim@example.com,01012345678,2024-02-01",
                CancellationToken.None));

        Assert.Equal("invalid_format", exception.Code);
    }

    [Fact(DisplayName = "CSV 빈 입력은 형식 오류로 실패한다.")]
    public async Task ParseAsync_ShouldThrow_WhenContentIsBlank()
    {
        IEmployeeImportParser parser = InfrastructureTestHost.CreateCsvParser();

        EmployeeApplicationException exception = await Assert.ThrowsAsync<EmployeeApplicationException>(() =>
            parser.ParseAsync("   ", CancellationToken.None));

        Assert.Equal("invalid_format", exception.Code);
    }

    [Fact(DisplayName = "CSV 마지막 빈 줄은 무시한다.")]
    public async Task ParseAsync_ShouldIgnoreTrailingBlankLine()
    {
        IEmployeeImportParser parser = InfrastructureTestHost.CreateCsvParser();

        IReadOnlyList<BulkEmployeeRecord> records = await parser.ParseAsync(
            "김철수,kim@example.com,01012345678,2024-02-01\n",
            CancellationToken.None);

        Assert.Single(records);
        Assert.Equal(1, records[0].Row);
    }

    [Fact(DisplayName = "CSV 중간 빈 줄은 형식 오류로 처리한다.")]
    public async Task ParseAsync_ShouldThrow_WhenBlankLineExistsInTheMiddle()
    {
        IEmployeeImportParser parser = InfrastructureTestHost.CreateCsvParser();

        EmployeeApplicationException exception = await Assert.ThrowsAsync<EmployeeApplicationException>(() =>
            parser.ParseAsync(
                "김철수,kim@example.com,01012345678,2024-02-01\n\n박영희,park@example.com,01087654321,2024-02-02",
                CancellationToken.None));

        Assert.Equal("invalid_format", exception.Code);
    }

    [Fact(DisplayName = "CSV UTF-8 BOM 입력을 허용한다.")]
    public async Task ParseAsync_ShouldAllowUtf8Bom()
    {
        IEmployeeImportParser parser = InfrastructureTestHost.CreateCsvParser();

        IReadOnlyList<BulkEmployeeRecord> records = await parser.ParseAsync(
            "\uFEFFname,email,tel,joined\n김철수,kim@example.com,01012345678,2024-02-01",
            CancellationToken.None);

        Assert.Single(records);
        Assert.Equal("김철수", records[0].Name);
    }

    [Fact(DisplayName = "CSV parser는 필드 trim 없이 원문을 유지한다.")]
    public async Task ParseAsync_ShouldPreserveOriginalFieldValues()
    {
        IEmployeeImportParser parser = InfrastructureTestHost.CreateCsvParser();

        IReadOnlyList<BulkEmployeeRecord> records = await parser.ParseAsync(
            "  김철수  , Kim@Example.Com ,010-1234-5678,2024-02-01",
            CancellationToken.None);

        Assert.Single(records);
        Assert.Equal("  김철수  ", records[0].Name);
        Assert.Equal(" Kim@Example.Com ", records[0].Email);
    }
}
