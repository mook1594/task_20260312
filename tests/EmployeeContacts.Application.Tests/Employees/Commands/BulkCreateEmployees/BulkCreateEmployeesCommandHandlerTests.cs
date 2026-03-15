using EmployeeContacts.Application.Employees.Commands.BulkCreateEmployees;
using EmployeeContacts.Application.Tests.TestDoubles;

namespace EmployeeContacts.Application.Tests.Employees.Commands.BulkCreateEmployees;

public class BulkCreateEmployeesCommandHandlerTests
{
    [Fact(DisplayName = "정상 행은 Employee를 생성하고 저장한다.")]
    public async Task Handle_ShouldCreateEmployees_ForValidRows()
    {
        var repository = new TestEmployeeRepository();
        var unitOfWork = new RecordingUnitOfWork();
        BulkCreateEmployeesCommandHandler handler = CreateHandler(repository, unitOfWork);
        var command = new BulkCreateEmployeesCommand(
        [
            new BulkEmployeeRecord(1, "  김철수  ", " Kim@Example.Com ", "010-1234-5678", "2024-02-01"),
            new BulkEmployeeRecord(2, "박영희", "park@example.com", "01087654321", "2024-02-02")
        ]);

        BulkCreateEmployeesResult result = await handler.Handle(command, CancellationToken.None);

        Assert.Equal(2, result.Total);
        Assert.Equal(2, result.Created);
        Assert.Equal(0, result.Failed);
        Assert.Empty(result.Errors);
        Assert.Equal(1, repository.AddRangeCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
        Assert.Equal(2, repository.AddedEmployees.Count);
        Assert.Equal("김철수", repository.AddedEmployees[0].Name.Value);
        Assert.Equal("kim@example.com", repository.AddedEmployees[0].Email.Value);
        Assert.Equal("01012345678", repository.AddedEmployees[0].PhoneNumber.Value);
        Assert.Equal(["kim@example.com", "park@example.com"], repository.LastEmailLookupValues);
        Assert.Equal(["01012345678", "01087654321"], repository.LastPhoneLookupValues);
    }

    [Fact(DisplayName = "요청 내부 이메일 중복은 뒤에 나온 행을 실패 처리한다.")]
    public async Task Handle_ShouldFailDuplicateEmailWithinRequest()
    {
        var repository = new TestEmployeeRepository();
        var unitOfWork = new RecordingUnitOfWork();
        BulkCreateEmployeesCommandHandler handler = CreateHandler(repository, unitOfWork);

        BulkCreateEmployeesResult result = await handler.Handle(
            new BulkCreateEmployeesCommand(
            [
                new BulkEmployeeRecord(1, "김철수", "dup@example.com", "01012345678", "2024-02-01"),
                new BulkEmployeeRecord(2, "박영희", " DUP@example.com ", "01087654321", "2024-02-02")
            ]),
            CancellationToken.None);

        Assert.Equal(1, result.Created);
        Assert.Equal(1, result.Failed);
        Assert.Single(result.Errors);
        Assert.Equal(2, result.Errors[0].Row);
        Assert.Equal("email", result.Errors[0].Field);
        Assert.Equal("duplicate_email", result.Errors[0].Code);
        Assert.Equal("email already exists", result.Errors[0].Message);
    }

    [Fact(DisplayName = "요청 내부 전화번호 중복은 뒤에 나온 행을 실패 처리한다.")]
    public async Task Handle_ShouldFailDuplicateTelWithinRequest()
    {
        var repository = new TestEmployeeRepository();
        var unitOfWork = new RecordingUnitOfWork();
        BulkCreateEmployeesCommandHandler handler = CreateHandler(repository, unitOfWork);

        BulkCreateEmployeesResult result = await handler.Handle(
            new BulkCreateEmployeesCommand(
            [
                new BulkEmployeeRecord(1, "김철수", "kim@example.com", "010-1234-5678", "2024-02-01"),
                new BulkEmployeeRecord(2, "박영희", "park@example.com", "01012345678", "2024-02-02")
            ]),
            CancellationToken.None);

        Assert.Equal(1, result.Created);
        Assert.Equal(1, result.Failed);
        Assert.Single(result.Errors);
        Assert.Equal(2, result.Errors[0].Row);
        Assert.Equal("tel", result.Errors[0].Field);
        Assert.Equal("duplicate_tel", result.Errors[0].Code);
    }

    [Fact(DisplayName = "요청 내부 중복은 저장소 중복 검사보다 먼저 처리한다.")]
    public async Task Handle_ShouldPrioritizeRequestDuplicate_BeforeRepositoryDuplicateChecks()
    {
        var repository = new TestEmployeeRepository();
        repository.ExistingPhoneNumbers.Add("01011112222");
        var unitOfWork = new RecordingUnitOfWork();
        BulkCreateEmployeesCommandHandler handler = CreateHandler(repository, unitOfWork);

        BulkCreateEmployeesResult result = await handler.Handle(
            new BulkCreateEmployeesCommand(
            [
                new BulkEmployeeRecord(1, "김철수", "dup@example.com", "01011112222", "2024-02-01"),
                new BulkEmployeeRecord(2, "박영희", " DUP@example.com ", "01099998888", "2024-02-02")
            ]),
            CancellationToken.None);

        Assert.Equal(0, result.Created);
        Assert.Equal(2, result.Failed);
        Assert.Equal(["dup@example.com"], repository.LastEmailLookupValues);
        Assert.Equal(["01011112222"], repository.LastPhoneLookupValues);
        Assert.Equal([1, 2], result.Errors.Select(error => error.Row).ToArray());
        Assert.Equal("tel", result.Errors[0].Field);
        Assert.Equal("email", result.Errors[1].Field);
    }

    [Fact(DisplayName = "기존 이메일 중복은 실패 처리한다.")]
    public async Task Handle_ShouldFail_WhenEmailAlreadyExists()
    {
        var repository = new TestEmployeeRepository();
        repository.ExistingEmails.Add("kim@example.com");
        var unitOfWork = new RecordingUnitOfWork();
        BulkCreateEmployeesCommandHandler handler = CreateHandler(repository, unitOfWork);

        BulkCreateEmployeesResult result = await handler.Handle(
            new BulkCreateEmployeesCommand(
            [
                new BulkEmployeeRecord(1, "김철수", "kim@example.com", "01012345678", "2024-02-01")
            ]),
            CancellationToken.None);

        Assert.Equal(0, result.Created);
        Assert.Equal(1, result.Failed);
        Assert.Single(result.Errors);
        Assert.Equal("email", result.Errors[0].Field);
        Assert.Equal("duplicate_email", result.Errors[0].Code);
        Assert.Equal(0, repository.AddRangeCallCount);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact(DisplayName = "기존 전화번호 중복은 실패 처리한다.")]
    public async Task Handle_ShouldFail_WhenTelAlreadyExists()
    {
        var repository = new TestEmployeeRepository();
        repository.ExistingPhoneNumbers.Add("01012345678");
        var unitOfWork = new RecordingUnitOfWork();
        BulkCreateEmployeesCommandHandler handler = CreateHandler(repository, unitOfWork);

        BulkCreateEmployeesResult result = await handler.Handle(
            new BulkCreateEmployeesCommand(
            [
                new BulkEmployeeRecord(1, "김철수", "kim@example.com", "01012345678", "2024-02-01")
            ]),
            CancellationToken.None);

        Assert.Equal(0, result.Created);
        Assert.Equal(1, result.Failed);
        Assert.Single(result.Errors);
        Assert.Equal("tel", result.Errors[0].Field);
        Assert.Equal("duplicate_tel", result.Errors[0].Code);
        Assert.Equal("tel already exists", result.Errors[0].Message);
    }

    [Fact(DisplayName = "도메인 예외를 등록 오류 항목으로 변환한다.")]
    public async Task Handle_ShouldMapDomainException_ToBulkError()
    {
        BulkCreateEmployeesCommandHandler handler = CreateHandler(new TestEmployeeRepository(), new RecordingUnitOfWork());

        BulkCreateEmployeesResult result = await handler.Handle(
            new BulkCreateEmployeesCommand(
            [
                new BulkEmployeeRecord(1, "김철수", "not-an-email", "01012345678", "2024-02-01")
            ]),
            CancellationToken.None);

        Assert.Equal(0, result.Created);
        Assert.Equal(1, result.Failed);
        Assert.Single(result.Errors);
        Assert.Equal(1, result.Errors[0].Row);
        Assert.Equal("email", result.Errors[0].Field);
        Assert.Equal("invalid_email", result.Errors[0].Code);
    }

    [Fact(DisplayName = "이름 도메인 예외는 name 필드 오류로 변환한다.")]
    public async Task Handle_ShouldMapNameDomainException_ToBulkError()
    {
        BulkCreateEmployeesCommandHandler handler = CreateHandler(new TestEmployeeRepository(), new RecordingUnitOfWork());

        BulkCreateEmployeesResult result = await handler.Handle(
            new BulkCreateEmployeesCommand(
            [
                new BulkEmployeeRecord(1, "   ", "kim@example.com", "01012345678", "2024-02-01")
            ]),
            CancellationToken.None);

        Assert.Single(result.Errors);
        Assert.Equal("name", result.Errors[0].Field);
        Assert.Equal("invalid_name", result.Errors[0].Code);
        Assert.Equal("Employee name is required.", result.Errors[0].Message);
    }

    [Fact(DisplayName = "일부 행만 성공해도 결과 집계를 올바르게 반환한다.")]
    public async Task Handle_ShouldReturnPartialSuccessResult()
    {
        var repository = new TestEmployeeRepository();
        repository.ExistingEmails.Add("exists@example.com");
        var unitOfWork = new RecordingUnitOfWork();
        BulkCreateEmployeesCommandHandler handler = CreateHandler(repository, unitOfWork);

        BulkCreateEmployeesResult result = await handler.Handle(
            new BulkCreateEmployeesCommand(
            [
                new BulkEmployeeRecord(1, "김철수", "kim@example.com", "01012345678", "2024-02-01"),
                new BulkEmployeeRecord(2, "박영희", "exists@example.com", "01087654321", "2024-02-02"),
                new BulkEmployeeRecord(3, "홍길동", "hong@example.com", "01011112222", "2024-02-03")
            ]),
            CancellationToken.None);

        Assert.Equal(3, result.Total);
        Assert.Equal(2, result.Created);
        Assert.Equal(1, result.Failed);
        Assert.Single(result.Errors);
        Assert.Equal(2, repository.AddedEmployees.Count);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact(DisplayName = "한 건도 생성되지 않으면 실패 결과를 반환한다.")]
    public async Task Handle_ShouldReturnFailureResult_WhenNothingIsCreated()
    {
        var repository = new TestEmployeeRepository();
        var unitOfWork = new RecordingUnitOfWork();
        BulkCreateEmployeesCommandHandler handler = CreateHandler(repository, unitOfWork);

        BulkCreateEmployeesResult result = await handler.Handle(
            new BulkCreateEmployeesCommand(
            [
                new BulkEmployeeRecord(1, "", "kim@example.com", "01012345678", "2024-02-01"),
                new BulkEmployeeRecord(2, "박영희", "park@example.com", "0108765432", "2024-02-02")
            ]),
            CancellationToken.None);

        Assert.Equal(0, result.Created);
        Assert.Equal(2, result.Failed);
        Assert.Equal(0, repository.AddRangeCallCount);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact(DisplayName = "오류는 입력 행 순서를 유지한다.")]
    public async Task Handle_ShouldPreserveErrorOrderByRow()
    {
        var repository = new TestEmployeeRepository();
        repository.ExistingPhoneNumbers.Add("01099998888");
        var unitOfWork = new RecordingUnitOfWork();
        BulkCreateEmployeesCommandHandler handler = CreateHandler(repository, unitOfWork);

        BulkCreateEmployeesResult result = await handler.Handle(
            new BulkCreateEmployeesCommand(
            [
                new BulkEmployeeRecord(10, "김철수", "bad-email", "01012345678", "2024-02-01"),
                new BulkEmployeeRecord(20, "박영희", "park@example.com", "01099998888", "2024-02-02"),
                new BulkEmployeeRecord(30, "홍길동", "park@example.com", "01011112222", "2024-02-03")
            ]),
            CancellationToken.None);

        Assert.Equal([10, 20, 30], result.Errors.Select(error => error.Row).ToArray());
    }

    [Fact(DisplayName = "joined 형식이 잘못되면 고정 메시지로 실패 처리한다.")]
    public async Task Handle_ShouldReturnFixedMessage_WhenJoinedIsInvalid()
    {
        BulkCreateEmployeesCommandHandler handler = CreateHandler(new TestEmployeeRepository(), new RecordingUnitOfWork());

        BulkCreateEmployeesResult result = await handler.Handle(
            new BulkCreateEmployeesCommand(
            [
                new BulkEmployeeRecord(1, "김철수", "kim@example.com", "01012345678", "2024/02/01")
            ]),
            CancellationToken.None);

        Assert.Single(result.Errors);
        Assert.Equal("joined", result.Errors[0].Field);
        Assert.Equal("invalid_joined", result.Errors[0].Code);
        Assert.Equal("joined must be yyyy-MM-dd", result.Errors[0].Message);
    }

    private static BulkCreateEmployeesCommandHandler CreateHandler(
        TestEmployeeRepository repository,
        RecordingUnitOfWork unitOfWork)
        => new(repository, unitOfWork);
}
