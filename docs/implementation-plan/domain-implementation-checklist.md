# Domain 구현 체크리스트

## 1. 구현 범위와 고정 결정 확인

- [x] Aggregate 경계는 `Employee` 단건으로 고정
- [x] 구현 대상은 `Employee`, `EmployeeName`, `EmployeeEmail`, `EmployeePhoneNumber`, `DomainException`, `EmployeeDomainException` 계층, Domain 테스트로 한정
- [x] 저장소 인터페이스, 중복 검사, CSV/JSON 파싱, DTO, EF Core 매핑, ProblemDetails 계약은 이번 범위에서 제외
- [x] `Employee`는 공개 생성자 없이 정적 팩터리 `Create(...)`만 제공
- [x] 값 객체는 `Employee`의 공개 계약에도 사용한다
- [x] `Employee` 도메인은 `DomainException` 기반 전용 예외 계층을 사용
- [x] `Joined`는 `DateOnly`를 직접 받고 `DateOnly.MinValue`만 금지
- [x] 미래 날짜 금지 규칙은 이번 구현에 포함하지 않음
- [x] `CreatedAt`, `UpdatedAt`은 `DateTimeOffset`이며 UTC 오프셋 `+00:00` 기준으로 관리
- [x] 내부 모델 명칭은 `PhoneNumber`를 사용하고 API response body에서만 `tel`로 매핑

## 2. 파일 구조 준비

- [ ] `src/EmployeeContacts.Domain/Common/DomainException.cs` 생성
- [ ] `src/EmployeeContacts.Domain/Employees/Employee.cs` 생성
- [ ] `src/EmployeeContacts.Domain/Employees/ValueObjects/EmployeeName.cs` 생성
- [ ] `src/EmployeeContacts.Domain/Employees/ValueObjects/EmployeeEmail.cs` 생성
- [ ] `src/EmployeeContacts.Domain/Employees/ValueObjects/EmployeePhoneNumber.cs` 생성
- [ ] `tests/EmployeeContacts.Domain.Tests/Common/DomainExceptionTests.cs` 위치 확인
- [ ] `tests/EmployeeContacts.Domain.Tests/Employees/EmployeeTests.cs` 위치 확인
- [ ] `tests/EmployeeContacts.Domain.Tests/Employees/ValueObjects/EmployeeNameTests.cs` 위치 확인
- [ ] `tests/EmployeeContacts.Domain.Tests/Employees/ValueObjects/EmployeeEmailTests.cs` 위치 확인
- [ ] `tests/EmployeeContacts.Domain.Tests/Employees/ValueObjects/EmployeePhoneNumberTests.cs` 위치 확인
- [ ] 테스트 폴더 구조가 구현 폴더 구조와 동일한지 확인

## 3. Phase 1. 테스트 준비

- [ ] `tests/EmployeeContacts.Domain.Tests` 프로젝트 참조 상태 확인
- [ ] `FluentAssertions` 사용 여부 확인
- [ ] `Employees` 폴더 아래 테스트 파일 생성
- [ ] 테스트 메서드명은 영문으로 작성
- [ ] 각 테스트 의도는 xUnit `DisplayName` 한글 설명으로 기록

## 4. Phase 2. 값 객체 테스트 Red 확인

### EmployeeName

- [ ] `Create_ShouldTrimName()` 작성
- [ ] `Create_ShouldThrow_WhenNameIsNull()` 작성
- [ ] `Create_ShouldThrow_WhenNameIsWhitespace()` 작성

### EmployeeEmail

- [ ] `Create_ShouldTrimAndLowercaseEmail()` 작성
- [ ] `Create_ShouldThrow_WhenEmailIsNull()` 작성
- [ ] `Create_ShouldThrow_WhenEmailIsWhitespace()` 작성
- [ ] `Create_ShouldThrow_WhenEmailDoesNotContainAtSymbol()` 작성
- [ ] `Create_ShouldThrow_WhenEmailContainsMultipleAtSymbols()` 작성
- [ ] `Create_ShouldThrow_WhenEmailHasNoDomainPart()` 작성
- [ ] `Create_ShouldThrow_WhenEmailDomainHasNoDot()` 작성

### EmployeePhoneNumber

- [ ] `Create_ShouldNormalizeHyphenatedPhoneNumber()` 작성
- [ ] `Create_ShouldKeepDigitsOnlyForValidInput()` 작성
- [ ] `Create_ShouldThrow_WhenPhoneContainsInvalidCharacters()` 작성
- [ ] `Create_ShouldThrow_WhenPhoneDoesNotStartWith010()` 작성
- [ ] `Create_ShouldThrow_WhenPhoneLengthIsNot11()` 작성

### Red 확인

- [ ] 값 객체 테스트 작성 완료
- [ ] 실패 테스트 실행
- [ ] 실패 원인이 기대한 Red 상태인지 확인

## 5. Phase 3. 값 객체 구현

- [ ] `DomainException` 구현
- [ ] `EmployeeName` 구현
- [ ] `EmployeeEmail` 구현
- [ ] `EmployeePhoneNumber` 구현
- [ ] 이름 `null`, 공백, `trim` 규칙 반영
- [ ] 이메일 `trim` 및 소문자 정규화 반영
- [ ] 이메일 `@` 1개, 로컬 파트, 도메인 파트, 도메인 `.` 규칙 반영
- [ ] 전화번호 숫자와 하이픈만 허용
- [ ] 전화번호 하이픈 제거 후 숫자만 저장
- [ ] 전화번호가 `010`으로 시작하고 길이 11인지 검증
- [ ] 값 객체 테스트 녹색화

## 6. Phase 4. 엔터티 테스트 Red 확인

- [ ] `tests/EmployeeContacts.Domain.Tests/Employees/EmployeeTests.cs` 작성
- [ ] `Create_ShouldBuildEmployee_WithNormalizedValues()` 작성
- [ ] `Create_ShouldThrow_WhenJoinedIsDefault()` 작성
- [ ] `Create_ShouldAllowDuplicateNamesAsDomainConcern()` 작성
- [ ] 엔터티 실패 테스트 실행
- [ ] 실패 원인이 기대한 Red 상태인지 확인

## 7. Phase 5. 엔터티 구현

- [ ] `Employee` private 생성자 구현
- [ ] `Employee.Create(Guid, EmployeeName, EmployeeEmail, EmployeePhoneNumber, DateOnly)` 구현
- [ ] `Name`, `Email`, `PhoneNumber` 값 객체 공개 속성 구현
- [ ] `Joined` 기본값 검증 구현
- [ ] `CreatedAt`, `UpdatedAt`을 `DateTimeOffset`으로 구현
- [ ] `CreatedAt`, `UpdatedAt`이 UTC 오프셋 `+00:00`인지 보장
- [ ] 생성 시 `CreatedAt`과 `UpdatedAt`을 같은 값으로 설정
- [ ] 이름 중복을 Domain 규칙으로 금지하지 않음
- [ ] 엔터티 테스트 녹색화

## 8. 에러 코드 및 detail 고정

- [ ] 이름 검증 실패 시 `invalid_name` / `Employee name is required.` 사용
- [ ] 이메일 검증 실패 시 `invalid_email` / `Employee email is invalid.` 사용
- [ ] 전화번호 검증 실패 시 `invalid_tel` / `Employee phone number must be an 11-digit mobile number starting with 010.` 사용
- [ ] 입사일 검증 실패 시 `invalid_joined` / `Employee joined date is required.` 사용
- [ ] 값 객체 내부에서 검증 실패 시 `DomainException`을 던지도록 확인
- [ ] `Employee.Create`가 값 객체 검증과 입사일 검증을 조합하는지 확인
- [ ] `DomainException.Message`가 `detail`과 같은 값인지 확인
- [ ] Application이 `code`와 `detail`을 외부 계약으로 매핑한다는 기준 확인

## 9. Phase 6. 정리

- [ ] 중복 검증 코드 최소화
- [ ] 메시지 상수화 필요성 재검토
- [ ] 파일/타입 네이밍 최종 점검

## 10. 완료 기준 점검

- [ ] `EmployeeContacts.Domain`이 외부 패키지 의존성 없이 유지되는지 확인
- [ ] `Employee.Create(...)`가 항상 정규화된 값 객체만 노출하는지 확인
- [ ] 잘못된 이름, 이메일, 전화번호, 입사일이 Domain에서 차단되는지 확인
- [ ] 필수 Domain 테스트가 모두 녹색인지 확인
- [ ] Application이 별도 정규화 로직 없이 Domain 규칙을 재사용할 수 있는지 확인
