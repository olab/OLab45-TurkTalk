using Dawn;

namespace OLabWebAPI.TurkTalk.Commands
{
    public abstract class Method
    {
        public string MethodName { get; set; }
        public string CommandChannel { get; set; }

        public Method(string recipientGroupName, string methodName)
        {
            Guard.Argument(recipientGroupName).NotEmpty(recipientGroupName);
            Guard.Argument(methodName).NotEmpty(methodName);

            MethodName = methodName;
            CommandChannel = recipientGroupName;
        }

        public abstract string ToJson();
    }
}