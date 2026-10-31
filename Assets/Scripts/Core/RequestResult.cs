// Generic success/failure wrapper. Used for raw JSON (RequestResult<string>)
// and for parsed data (RequestResult<WeatherReport>), so errors flow the same way at every step.
public readonly struct RequestResult<T>
{
    public bool Success { get; }
    public T Data { get; }
    public string Error { get; }

    RequestResult(bool success, T data, string error)
    {
        Success = success;
        Data = data;
        Error = error;
    }

    public static RequestResult<T> Ok(T data) => new RequestResult<T>(true, data, null);
    public static RequestResult<T> Fail(string error) => new RequestResult<T>(false, default, error);
}
