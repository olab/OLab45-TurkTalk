namespace OLabWebAPI.TurkTalk.Contracts
{
    public class TargetNode
    {
        public uint MapId { get; set; }
        public uint NodeId { get; set; }
        public string NodeName { get; set; }
    }

    public class JumpNodePayload
    {
        public Envelope Envelope { get; set; }
        public TargetNode Data { get; set; }
        public SessionInfo Session { get; set; }

        public JumpNodePayload(Envelope envelope, TargetNode data, SessionInfo session)
        {
            Envelope = envelope;
            Data = data;
            Session = session;
        }

    }
}
