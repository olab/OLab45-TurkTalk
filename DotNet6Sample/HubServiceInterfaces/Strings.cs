namespace HubServiceInterfaces;

public static class Strings
{
    public static string HubUrl => "https://localhost:5002/hubs/clock";

    public static class Events
    {
        public static string TimeSent => nameof(IClock.Command);
    }
}
