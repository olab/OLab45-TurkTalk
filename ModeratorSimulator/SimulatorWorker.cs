using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using NLog;
using OLabWebAPI.Dto;
using OLabWebAPI.Model;

namespace OLab.TurkTalk.ModeratorSimulator.SimulationThread
{
  public partial class SimulatorWorker
  {
    private Settings _settings;
    private ILogger _logger;

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
        Random rnd = new Random();

        // set up a thread execution counter. Needs to be set to '1'
        // initially so it can be incremented without error
        using (CountdownEvent cde = new CountdownEvent(1))
        {
          // dispatch all the threads, and keep track of the number
          // created in the countdown event
          foreach (var moderator in _settings.Moderators)
          {
            // increment the thread count
            cde.AddCount();

            var workerThreadParam = new WorkerThreadParameter
            {
              CountdownEvent = cde,
              Moderator = moderator,
              Settings = _settings,
              Rnd = rnd,
              Logger = _logger
            };

            var proc = new ModeratorThread(workerThreadParam);
            ThreadPool.QueueUserWorkItem(new WaitCallback(o => proc.RunProc()), workerThreadParam);
          }

          // Decrease the counter (as it was initialized with the value 1).
          cde.Signal();

          // Wait until the counter is zero - meaning all the threads have run
          cde.Wait();
        }
      }
      catch (Exception ex)
      {
        // eat all exceptions
      }

    }

  }
}