using NLog;

namespace OLab.TurkTalk.ParticipantSimulator.SimulationThread
{
  public partial class SimulatorWorker
  {
    private readonly Settings _settings;
    private readonly ILogger _logger;

    public SimulatorWorker(Settings settings, ILogger logger)
    {
      _settings = settings;
      _logger = logger;
    }

    public void ExecuteWorkers()
    {
      var workerThreads = new List<Thread>();
      var workerThreadParams = new List<WorkerThreadParameter>();

      try
      {
        var rnd = new Random();

        // see if users need to be generated
        if ( _settings.ParticipantInfo != null )
          GenerateParticipants();

        // set up a thread execution counter. Needs to be set to '1'
        // initially so it can be incremented without error
        using ( var cde = new CountdownEvent( 1 ) )
        {
          // dispatch all the threads, and keep track of the number
          // created in the countdown event
          foreach ( var participant in _settings.Participants )
          {
            // increment the thread count
            cde.AddCount();

            var workerThreadParam = new WorkerThreadParameter
            {
              CountdownEvent = cde,
              Participant = participant,
              Settings = _settings,
              Rnd = rnd,
              Logger = _logger
            };

            var proc = new ParticipantThread( workerThreadParam );
#pragma warning disable CS4014 
            ThreadPool.QueueUserWorkItem( new WaitCallback( o => proc.RunProc() ), workerThreadParam );
#pragma warning restore CS4014
          }

          // Decrease the counter (as it was initialized with the value 1).
          cde.Signal();

          // Wait until the counter is zero - meaning all the threads have run
          cde.Wait();
        }
      }
      catch ( Exception ex )
      {
        // eat all exceptions
      }

    }

    private void GenerateParticipants()
    {
      // wipe existing participant list, if present
      _settings.Participants.Clear();

      var indexWidth = _settings.ParticipantInfo.UserIdPrefix.Count( x => x == '#' );
      var userIdPrefix = _settings.ParticipantInfo.UserIdPrefix.Replace( "#", "" );

      var pauseMs = new PauseMs();

      if ( _settings.ParticipantInfo.PauseMs != null )
        pauseMs = _settings.ParticipantInfo.PauseMs;
      else
        pauseMs = _settings.PauseMs;

      var startsAt = 1;
      if ( _settings.ParticipantInfo.StartsAt.HasValue )
        startsAt = _settings.ParticipantInfo.StartsAt.Value;

      var endsAt = startsAt + _settings.ParticipantInfo.NumUsers - 1;

      for ( var i = startsAt; i <= endsAt; i++ )
      {
        var index = i.ToString( $"D{indexWidth}" );
        var userId = $"{userIdPrefix}{index}";

        _logger.Info( $"generating user {userId}" );

        _settings.Participants.Add(
          new Participant
          {
            UserId = userId,
            Password = _settings.ParticipantInfo.Password,
            PauseMs = pauseMs
          }
          );
      }
    }
  }
}