using EmployeeContacts.Application.Behaviors;
using FluentValidation;
using MediatR;

namespace EmployeeContacts.Application.Tests.Behaviors;

public class ValidationBehaviorTests
{
    [Fact(DisplayName = "ValidationBehavior는 Handler 실행 전에 검증 예외를 던진다.")]
    public async Task Handle_ShouldThrowValidationException_WhenValidationFails()
    {
        var behavior = new ValidationBehavior<SampleRequest, string>(
            [new SampleRequestValidator()]);
        bool nextCalled = false;

        Task<string> Next()
        {
            nextCalled = true;
            return Task.FromResult("ok");
        }

        RequestHandlerDelegate<string> next = _ => Next();

        await Assert.ThrowsAsync<ValidationException>(
            () => behavior.Handle(new SampleRequest(string.Empty), next, CancellationToken.None));

        Assert.False(nextCalled);
    }

    [Fact(DisplayName = "ValidationBehavior는 검증 성공 시 다음 단계를 실행한다.")]
    public async Task Handle_ShouldInvokeNext_WhenValidationSucceeds()
    {
        var behavior = new ValidationBehavior<SampleRequest, string>(
            [new SampleRequestValidator()]);
        bool nextCalled = false;

        Task<string> Next()
        {
            nextCalled = true;
            return Task.FromResult("ok");
        }

        RequestHandlerDelegate<string> next = _ => Next();
        string result = await behavior.Handle(new SampleRequest("김철수"), next, CancellationToken.None);

        Assert.True(nextCalled);
        Assert.Equal("ok", result);
    }

    public sealed record SampleRequest(string Name) : IRequest<string>;

    private sealed class SampleRequestValidator : AbstractValidator<SampleRequest>
    {
        public SampleRequestValidator()
        {
            RuleFor(request => request.Name).NotEmpty();
        }
    }
}
