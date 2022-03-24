using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.Model;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
//using Microsoft.ServiceFabric.Services.Communication.Wcf;
//using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace RecommendationService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class RecommendationService : StatefulService , IRecommendationService
    {
        public RecommendationService(StatefulServiceContext context)
            : base(context)
        { }

        public async Task AddRecomendation(Recommendation recommendation)
        {
            var stateManager = this.StateManager;
            var recommendations = await stateManager.GetOrAddAsync<IReliableDictionary<int, Recommendation>>("recommendations");

            try
            {
                using (var tx = stateManager.CreateTransaction())
                {
                    await recommendations.AddOrUpdateAsync(tx, recommendation.ID, recommendation, (key, value) => value);

                    await tx.CommitAsync();
                }
            }
            catch ( Exception e)
            {

                ServiceEventSource.Current.ServiceMessage(this.Context,"Exception in RecommendationService - AddRecomendation():" + e.Message);
            }
        }

        public async Task<List<Recommendation>> GetRecommendations()
        {
            var stateManager = this.StateManager;
            var recommendations = await stateManager.GetOrAddAsync<IReliableDictionary<int, Recommendation>>("recommendations");
            List<Recommendation> recommendationList = new List<Recommendation>();

            try
            {
                using (var tx = stateManager.CreateTransaction())
                {
                    var enumerableData = await recommendations.CreateEnumerableAsync(tx);
                    var enumerator = enumerableData.GetAsyncEnumerator();
                    while (await enumerator.MoveNextAsync(new CancellationToken()))
                    {
                        recommendationList.Add(enumerator.Current.Value);
                    }
                    await tx.CommitAsync();
                }
            }
            catch (Exception e)
            {

                ServiceEventSource.Current.ServiceMessage(this.Context, "Exception in RecommendationService - GetRecommendations():" + e.Message);
            }

            return recommendationList;
        }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return this.CreateServiceRemotingReplicaListeners();
            
        }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("myDictionary");

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                using (var tx = this.StateManager.CreateTransaction())
                {
                    var result = await myDictionary.TryGetValueAsync(tx, "Counter");

                    ServiceEventSource.Current.ServiceMessage(this.Context, "Current Counter Value: {0}",
                        result.HasValue ? result.Value.ToString() : "Value does not exist.");

                    await myDictionary.AddOrUpdateAsync(tx, "Counter", 0, (key, value) => ++value);

                    // If an exception is thrown before calling CommitAsync, the transaction aborts, all changes are 
                    // discarded, and nothing is saved to the secondary replicas.
                    await tx.CommitAsync();
                }

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}
