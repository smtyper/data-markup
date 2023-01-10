using System.Net;

namespace DataMarkup.Frontend;

public static class Extentions
{
    public static bool IsSuccesStatusCode(this HttpStatusCode statusCode) =>
        (int)statusCode >= 200 && (int)statusCode <= 299;
}
