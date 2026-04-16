namespace LinkUp254.Features.Shared;


public class ServiceResult<T>
{

    public bool IsSuccess { get; set; }

    public T? Data { get; set; }


    public string? Message { get; set; }

    public static ServiceResult<T> Success(T data)
    {
        return new ServiceResult<T> { IsSuccess = true, Data = data };
    }


    public static ServiceResult<T> Success(T data, string message)
    {
        return new ServiceResult<T> { IsSuccess = true, Data = data, Message = message };
    }

    public static ServiceResult<T> Failure(string message)
    {
        return new ServiceResult<T> { IsSuccess = false, Message = message };
    }
}