namespace Domain.Enums;

public enum AuditAction
{
    Registration = 0,
    Login = 1,
    Logout = 2,
    PasswordReset = 3,
    EmailVerification = 4,
    FailedLogin = 5,
    PasswordChanged = 6
}
