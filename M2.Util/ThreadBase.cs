// This class and all other files in this assembly are the property and copyright of Mark II Software, LLC
// Copyright (c)2008-2011 Mark II Software, LLC.  All Rights Reserved.
// You may not use, copy, decompile, or in any way reference this file or functionality without the express written
// consent of an officer of the Mark II Software, LLC

using System;
using System.Threading;

namespace M2.Util
{
	// 1. Sublass this
	// 2. Override DoWork and create any other methods/state in the subclass
	// 3. Call Start<>
	public abstract class ThreadWorkerBase
	{
		private EventWaitHandle _stopEvent = new ManualResetEvent(false);

		public int Timeout { get; set; }

		private Thread _workerThread = null;
		private ThreadWorkerBase _worker = null; 

        private void ThreadStart()
        {
            OnStart();

            do
            {
                DoWork();
            } while (!ShouldStop(true));

            _stopEvent.Reset();

            OnStop();
        }

        public bool ShouldStop(bool useTimeout)
        {
            return _stopEvent.WaitOne(useTimeout ? Timeout : 50);
        }

        /////////////////////////////////////////////////////////////

        public void Start<T>(int timeout) where T : ThreadWorkerBase
		{
			_worker = Objects.GetNewObject<T>();
			_worker.Timeout = timeout;
			_workerThread = new Thread(_worker.ThreadStart);
			_workerThread.Start();
		}

		public void Stop()
		{
			if (_worker == null)
				return;

			_worker._stopEvent.Set();
			_workerThread.Join();
		}

		public abstract void DoWork();  // Must override this

		public virtual void OnStart() { }
		public virtual void OnStop() {}
	}
}
