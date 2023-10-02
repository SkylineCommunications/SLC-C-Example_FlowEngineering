﻿namespace Skyline.DataMiner.FlowEngineering.Protocol
{
	using System;
	using System.Collections.Generic;

	using Skyline.DataMiner.CommunityLibrary.FlowProvisioning.Enums;
	using Skyline.DataMiner.CommunityLibrary.FlowProvisioning.Info;
	using Skyline.DataMiner.FlowEngineering.Protocol.Model;
	using Skyline.DataMiner.Scripting;

	public class FlowEngineeringManager
	{
		public FlowEngineeringManager()
		{
			Interfaces = new Interfaces(this);
			IncomingFlows = new RxFlows(this);
			OutgoingFlows = new TxFlows(this);
		}

		public static FlowEngineeringManager GetInstance(SLProtocol protocol) => FlowEngineeringManagerInstances.GetInstance(protocol);

		public Interfaces Interfaces { get; }

		public RxFlows IncomingFlows { get; }

		public TxFlows OutgoingFlows { get; }

		public void LoadTables(SLProtocol protocol)
		{
			Interfaces.LoadTable(protocol);
			IncomingFlows.LoadTable(protocol);
			OutgoingFlows.LoadTable(protocol);
		}

		public void UpdateTables(SLProtocol protocol, bool includeStatistics = true)
		{
			Interfaces.UpdateTable(protocol, includeStatistics);
			IncomingFlows.UpdateTable(protocol, includeStatistics);
			OutgoingFlows.UpdateTable(protocol, includeStatistics);
		}

		public void UpdateInterfaceAndIncomingFlowsTables(SLProtocol protocol, bool includeStatistics = true)
		{
			Interfaces.UpdateTable(protocol, includeStatistics);
			IncomingFlows.UpdateTable(protocol, includeStatistics);
		}

		public void UpdateInterfaceAndOutgoingFlowsTables(SLProtocol protocol, bool includeStatistics = true)
		{
			Interfaces.UpdateTable(protocol, includeStatistics);
			OutgoingFlows.UpdateTable(protocol, includeStatistics);
		}

		public (ICollection<Flow> addedFlows, ICollection<Flow> removedFlows) HandleInterAppMessage(SLProtocolExt protocol, FlowInfoMessage message, bool ignoreDestinationPort = false)
		{
			if (protocol == null)
			{
				throw new ArgumentNullException(nameof(protocol));
			}

			if (message == null)
			{
				throw new ArgumentNullException(nameof(message));
			}

			var addedFlows = new List<Flow>();
			var removedFlows = new List<Flow>();

			foreach (var flowInfo in message.Flows)
			{
				Flow flow;

				switch (message.ActionType)
				{
					case ActionType.Create:
					case ActionType.Update:
						if (flowInfo.FlowDirection == FlowDirection.RX)
							flow = IncomingFlows.RegisterFlowEngineeringFlow(flowInfo, ignoreDestinationPort);
						else
							flow = OutgoingFlows.RegisterFlowEngineeringFlow(flowInfo, ignoreDestinationPort);

						if (flow != null)
							addedFlows.Add(flow);

						break;

					case ActionType.Delete:
						if (flowInfo.FlowDirection == FlowDirection.RX)
							flow = IncomingFlows.UnregisterFlowEngineeringFlow(flowInfo, ignoreDestinationPort);
						else
							flow = OutgoingFlows.UnregisterFlowEngineeringFlow(flowInfo, ignoreDestinationPort);

						if (flow != null)
							removedFlows.Add(flow);

						break;

					default:
						throw new InvalidOperationException($"Unknown action: {message.ActionType}");
				}
			}

			UpdateTables(protocol);

			return (addedFlows, removedFlows);
		}
	}
}