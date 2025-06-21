namespace CrudApiDemo.Responses
{
    public class ApiResponse<T>
    {
        public int Status { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        public object? Error { get; set; }

        public ApiResponse(int status, string? message = null, T? data = default, object? error = null)
        {
            Status = status;
            Message = message;
            Data = data;
            Error = error;
        }
    }
}
