using Common.Model;
using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    //[ServiceContract]
    public interface IRecommendationService : IService
    {
        //[OperationContract]
        Task<List<Recommendation>> GetRecommendations();
        //[OperationContract]
        Task AddRecomendation(Recommendation recommendation);
    }
}
