using System.Collections;

namespace MiniDashboard.Models.Common;

public class WebApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public T? Data { get; set; }
    public int Total { get; set; }

    public WebApiResponse(bool success, string message, T? data, int total)
    {
        Success = success;
        Message = message;
        Data = data;
        Total = total;
    }

    public static WebApiResponse<T> Ok()
    {
        return new WebApiResponse<T>(true, "Success", default, 0);
    }

    public static WebApiResponse<T> Ok(T? data)
    {
        if (data == null)
        {
            return new WebApiResponse<T>(true, "Success", default, 0);
        }

        if (IsEmptyCollection(data))
        {
            return new WebApiResponse<T>(true, "Success", default, 0);
        }

        int total = ShouldIncludeTotal(data) ? GetTotalFromData(data) : 1;

        return new WebApiResponse<T>(true, "Success", data, total);
    }

    public static WebApiResponse<T> Ok(T? data, int total) =>
        new WebApiResponse<T>(true, "Success", data, total);

    public static WebApiResponse<T> Fail(string message) =>
        new WebApiResponse<T>(false, message, default, 0);

    private static int GetTotalFromData(T? data)
    {
        if (data == null) return 0;

        if (data is ICollection<object> collection) return collection.Count;

        if (data is IEnumerable enumerable && data is not string && data is not bool)
            return enumerable.Cast<object>().Count();

        return 0;
    }

    private static bool IsEmptyCollection(T? data)
    {
        if (data is ICollection<object> collection) return collection.Count == 0;

        if (data is IEnumerable enumerable && data is not string && data is not bool)
            return !enumerable.Cast<object>().Any();

        return false;
    }

    private static bool ShouldIncludeTotal(T? data)
    {
        return data is IEnumerable && data is not string && data is not bool;
    }
}

