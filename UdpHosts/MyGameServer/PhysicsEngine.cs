using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace MyGameServer {
	public class PhysicsEngine {
		public double Accumulator { get; protected set; }
		public double SimulatedTime { get; protected set; }
		public double DT { get; }

		public PhysicsEngine(double dt) {
			Accumulator = 0.0;
			SimulatedTime = 0.0;
			DT = dt;
		}

		public void Tick( double deltaTime, ulong currTime, CancellationToken ct ) {
			Accumulator += deltaTime;

			while( !ct.IsCancellationRequested && Accumulator >= DT ) {
				Integrate( /*state*/null, SimulatedTime, DT );
				Accumulator -= DT;
				SimulatedTime += DT;
			}
		}

		protected void Integrate( object state, double t, double dt) {

		}
	}
}
