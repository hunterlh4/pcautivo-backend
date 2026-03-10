namespace PCautivoCore.Shared.Requests;

public class AuthContext
{
    public int UserId { get; set; }
}

public interface IAuthContext
{
    AuthContext GetAuthContext();
    void SetAuthContext(AuthContext authContext);
    int UserId { get; }
}

public class AuthContextService : IAuthContext
{
    private AuthContext _authContext = new();

    public AuthContext GetAuthContext()
    {
        return _authContext;
    }

    public void SetAuthContext(AuthContext authContext)
    {
        _authContext = authContext;
    }

    public int UserId => _authContext.UserId;
}